#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Guard;

namespace System.IO;

public partial class FastFileOperations {
  public delegate void FileReportCallback(IFileReport report);

  private sealed class FileCopyOperation(FileInfo source, FileInfo target, FileReportCallback callback, bool canReadDuringWrite, int chunkSize = -1, int maxReadAheadSize = -1)
    : IFileOperation {
    #region nested types

    /// <summary>
    ///   A report from a running file-copy operation.
    /// </summary>
    private sealed class FileCopyReport(ReportType reportType, FileCopyOperation operation, int streamIndex, long chunkOffset, long chunkSize)
      : IFileReport {
      #region Implementation of IFileReport

      public ReportType ReportType { get; } = reportType;
      public IFileSystemOperation Operation { get; } = operation;
      public int StreamIndex { get; } = streamIndex;
      public long StreamOffset { get; } = 0;
      public long ChunkOffset { get; } = chunkOffset;
      public long ChunkSize { get; } = chunkSize;
      public long StreamSize { get; } = operation.BytesToTransfer;
      public FileSystemInfo Source { get; } = operation.Source;
      public FileSystemInfo Target { get; } = operation.Target;
      public ContinuationType ContinuationType { get; set; } = reportType == ReportType.AbortedOperation ? ContinuationType.AbortOperation : ContinuationType.Proceed;

      #endregion
    }

    /// <summary>
    ///   A bunch of data that is read or being written.
    /// </summary>
    private sealed class Chunk(long offset, int size) {
      public readonly long offset = offset;
      public readonly byte[] data = new byte[size];
      public int length = size;
    }

    #endregion

    #region fields

    private readonly FileReportCallback _callback = callback ?? (_ => { });
    private readonly ManualResetEventSlim _finishEvent = new();
    private readonly long _maxReadAheadSize = maxReadAheadSize > 0 ? maxReadAheadSize : _MAX_READ_AHEAD;
    private readonly ConcurrentQueue<Chunk> _readAheadCache = new();
    private int _streamCount = 1;
    private long _bytesRead;
    private long _bytesTransferred;
    private long _readOffset;

    #endregion

    #region props

    public FileStream SourceStream { private get; set; }
    public FileStream TargetStream { private get; set; }
    public int ChunkSize { get; } = chunkSize > 0 ? chunkSize : source.Length < _MAX_SMALL_FILESIZE ? _DEFAULT_BUFFER_SIZE : _DEFAULT_LARGE_BUFFER_SIZE;

    private long _CurrentReadAheadSize => this._readAheadCache.ToArray().Sum(c => c.length);

    private bool _CanRead {
      get {
        if (this.IsDone)
          return false;

        if (canReadDuringWrite)
          return this._CurrentReadAheadSize < this._maxReadAheadSize;

        return !this._readAheadCache.Any();
      }
    }

    private bool _AcquireWriteStream {
      get {
        if (Interlocked.Decrement(ref this._streamCount) >= 0)
          return true;

        Interlocked.Increment(ref this._streamCount);
        return false;
      }
    }

    #endregion

    #region ctor,dtor

    private void _Dispose() {
      this._UnregisterFromAppUnload();
      _TryDisposeStream(this.SourceStream);
      _TryDisposeStream(this.TargetStream);
      this._readAheadCache.Clear();
    }

    #endregion

    #region chunk management

    private Chunk _GetReadChunk() {
      if (this.IsDone)
        return null;

      while (!this._CanRead)
        Thread.Sleep(1);

      var size = this.ChunkSize;
      var offset = Interlocked.Add(ref this._readOffset, size) - size;
      if (offset >= this.TotalSize)
        return null;

      this._CreateReport(ReportType.StartRead, 0, offset, size);
      return new(offset, size);
    }

    private void _ReleaseReadChunk(Chunk chunk) {
      Interlocked.Add(ref this._bytesRead, chunk.length);
      this._CreateReport(ReportType.FinishedRead, 0, chunk.offset, chunk.length);
      this._readAheadCache.Enqueue(chunk);
    }

    private Chunk _GetWriteChunk() {
      Chunk result = null;
      while (!(this.IsDone || this._readAheadCache.TryDequeue(out result)))
        Thread.Sleep(1);

      if (this.IsDone)
        return null;

      this._CreateReport(ReportType.StartWrite, 0, result.offset, result.length);
      return result;
    }

    private void _ReleaseWriteChunk(Chunk chunk) {
      var result = Interlocked.Add(ref this._bytesTransferred, chunk.length);
      this._CreateReport(ReportType.FinishedWrite, 0, chunk.offset, chunk.length);

      if (result >= this.TotalSize)
        this.OperationFinished();
    }

    #endregion

    #region appdomain messing

    /// <summary>
    ///   Handles application exit and deletes any unfinished target file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void _AppDomainUnload(object sender, EventArgs args) => this.OperationAborted(new OperationCanceledException(_EX_APP_UNLOAD));

    /// <summary>
    ///   Registers an event handler to the ProcessExit event of the current appdomain.
    /// </summary>
    private void _RegisterToAppUnload() => AppDomain.CurrentDomain.ProcessExit += this._AppDomainUnload;

    /// <summary>
    ///   Unregisters the event handler from the ProcessExit event of the current appdomain.
    /// </summary>
    private void _UnregisterFromAppUnload() => AppDomain.CurrentDomain.ProcessExit -= this._AppDomainUnload;

    #endregion

    /// <summary>
    ///   Tries to dispose a stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    private static bool _TryDisposeStream(FileStream stream) {
      if (stream == null)
        return true;

      try {
        stream.Dispose();
        return true;
      } catch {
        return false;
      }
    }

    #region generate reports

    private FileCopyReport _CreateReport(ReportType reportType, int streamIndex, long offset, long size) {
      FileCopyReport result = new(reportType, this, streamIndex, offset, size);
      this._callback(result);
      return result;
    }

    public IFileReport OperationStarted() {
      this._RegisterToAppUnload();
      return this._CreateReport(ReportType.StartOperation, -1, 0, this.TotalSize);
    }

    public IFileReport OperationFinished() {
      this._Dispose();
      target.Attributes = source.Attributes;
      target.CreationTimeUtc = source.CreationTimeUtc;
      target.LastWriteTimeUtc = source.LastWriteTimeUtc;

      this._finishEvent.Set();
      return this._CreateReport(ReportType.FinishedOperation, -1, 0, this.TotalSize);
    }

    public IFileReport OperationAborted(Exception exception) {
      this.Exception = exception;
      var result = this._CreateReport(ReportType.AbortedOperation, -1, 0, this.TotalSize);
      if (result.ContinuationType != ContinuationType.AbortOperation)
        return result;

      this._Dispose();
      target.Delete();
      this._finishEvent.Set();
      return result;
    }

    public IFileReport CreatedLink() => this._CreateReport(ReportType.CreatedLink, -1, 0, this.TotalSize);

    #endregion

    #region async read/write

    /// <summary>
    ///   Starts the reading.
    /// </summary>
    public void StartReading() => this._ReadCallback(null);

    /// <summary>
    ///   The read chunk callback.
    /// </summary>
    /// <param name="asyncResult">The asynchronous result.</param>
    private void _ReadCallback(IAsyncResult asyncResult) {
      if (asyncResult != null) {
        var chunk = (Chunk)asyncResult.AsyncState;

        // end read
        int bytesRead;
        try {
          bytesRead = this.SourceStream.EndRead(asyncResult);
        } catch (Exception e) {
          var result = this.OperationAborted(e);

          switch (result.ContinuationType) {
            case ContinuationType.Proceed:
              bytesRead = chunk.length;
              break;
            case ContinuationType.RetryChunk when !this.IsDone:
              this._BeginReadChunk(chunk);
              return;
            case ContinuationType.RetryStream when !this.IsDone:
              // TODO: abort other threads to continue writing, reset both streams, restart reading
              throw new NotImplementedException();
            default: return;
          }
        }

        chunk.length = bytesRead;
        this._ReleaseReadChunk(chunk);

        // try to start a write stream
        if (this._AcquireWriteStream)
          this._WriteCallback(null);
      }

      var newChunk = this._GetReadChunk();
      if (newChunk == null)
        return;

      this._BeginReadChunk(newChunk);
    }

    /// <summary>
    ///   Begins reading the given chunk.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void _BeginReadChunk(Chunk chunk) {
      // TODO: race with more than one read stream because they could interfere in changing the file seek
      try {
        this.SourceStream.Position = chunk.offset;
        this.SourceStream.BeginRead(chunk.data, 0, chunk.length, this._ReadCallback, chunk);
      } catch (Exception e) {
        var result = this.OperationAborted(e);
        switch (result.ContinuationType) {
          case ContinuationType.Proceed:
            this._ReadCallback(null);
            break;
          case ContinuationType.RetryChunk when !this.IsDone:
            this._BeginReadChunk(chunk);
            break;
          // TODO: abort other threads to continue writing, reset both streams, restart reading
          case ContinuationType.RetryStream when !this.IsDone: throw new NotImplementedException();
        }
      }
    }

    /// <summary>
    ///   The write callback.
    /// </summary>
    /// <param name="asyncResult">The asynchronous result.</param>
    private void _WriteCallback(IAsyncResult asyncResult) {
      Chunk chunk;
      if (asyncResult != null) {
        // end write
        chunk = (Chunk)asyncResult.AsyncState;
        try {
          this.TargetStream.EndWrite(asyncResult);
        } catch (Exception e) {
          var result = this.OperationAborted(e);

          switch (result.ContinuationType) {
            case ContinuationType.Proceed: break;
            case ContinuationType.RetryChunk when !this.IsDone:
              this._BeginWriteChunk(chunk);
              return;
            case ContinuationType.RetryStream when !this.IsDone:
              // TODO: abort other threads to continue writing, reset both streams, restart reading
              throw new NotImplementedException();
            default: return;
          }
        }

        this._ReleaseWriteChunk(chunk);
      }

      // wait for next chunk to write
      chunk = this._GetWriteChunk();

      // already aborted
      if (chunk == null)
        return;

      // begin write
      this._BeginWriteChunk(chunk);
    }

    /// <summary>
    ///   Begins writing the given chunk.
    /// </summary>
    /// <param name="chunk">The chunk.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void _BeginWriteChunk(Chunk chunk) {
      // TODO: race with more than one write stream because they could interfere in changing the file seek
      try {
        this.TargetStream.Position = chunk.offset;
        this.TargetStream.BeginWrite(chunk.data, 0, chunk.length, this._WriteCallback, chunk);
      } catch (Exception e) {
        var result = this.OperationAborted(e);
        switch (result.ContinuationType) {
          case ContinuationType.Proceed:
            this._WriteCallback(null);
            break;
          case ContinuationType.RetryChunk when !this.IsDone:
            this._BeginWriteChunk(chunk);
            break;
          // TODO: abort other threads to continue writing, reset both streams, restart reading
          case ContinuationType.RetryStream when !this.IsDone: throw new NotImplementedException();
        }
      }
    }

    #endregion

    #region Implementation of IFileOperation

    public FileSystemInfo Source => source;
    public FileSystemInfo Target => target;
    public long TotalSize => source.Length;
    public long BytesToTransfer => this.TotalSize;
    public long BytesRead => Interlocked.Read(ref this._bytesRead);
    public long BytesTransferred => Interlocked.Read(ref this._bytesTransferred);

    public int StreamCount {
      get => this._streamCount;
      set {
        this._streamCount = Math.Max(1, value);
        Thread.MemoryBarrier();
      }
    }

    public Exception Exception { get; private set; }
    public bool IsDone => this._finishEvent.IsSet;
    public bool ThrewException => this.Exception != null;
    public void Abort() => this.OperationAborted(new OperationCanceledException(_EX_USER_ABORT));
    public void WaitTillDone() => this._finishEvent.Wait();
    public bool WaitTillDone(TimeSpan timeout) => this._finishEvent.Wait(timeout);

    #endregion
  }

  #region copy file

  /// <summary>
  ///   Copies the file.
  /// </summary>
  /// <param name="this">This source file.</param>
  /// <param name="target">The target file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites existing; otherwise throws IOException.</param>
  /// <param name="allowHardLinks">if set to <c>true</c> allows creation of hard links.</param>
  /// <param name="dontResolveSymbolicLinks">if set to <c>true</c> [dont resolve symbolic links].</param>
  /// <param name="callback">The callback, if any.</param>
  /// <param name="allowedStreams">The allowed number of streams.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <exception cref="System.IO.FileNotFoundException">When source file does not exist</exception>
  /// <exception cref="System.IO.IOException">When target file exists and should not overwrite</exception>
  public static void CopyTo(this FileInfo @this, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = _DEFAULT_BUFFER_SIZE) {
    var token = CopyToAsync(@this, target, overwrite, allowHardLinks, dontResolveSymbolicLinks, callback, allowedStreams, bufferSize);
    token.Operation.WaitTillDone();
    if (token.Operation.ThrewException)
      throw token.Operation.Exception;
  }

  /// <summary>
  ///   Copies the file asynchronous.
  ///   This reads one chunk at a time and when the OS signals that a read is done, it starts writing that chunk while,
  ///   possibly at the same time, reading the next.
  /// </summary>
  /// <param name="this">This source file.</param>
  /// <param name="target">The target file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites existing; otherwise throws IOException.</param>
  /// <param name="allowHardLinks">if set to <c>true</c> allows creation of hard links.</param>
  /// <param name="dontResolveSymbolicLinks">if set to <c>true</c> [dont resolve symbolic links].</param>
  /// <param name="callback">The callback, if any.</param>
  /// <param name="allowedStreams">The allowed number of streams.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <returns></returns>
  /// <exception cref="System.IO.FileNotFoundException">When source file does not exist</exception>
  /// <exception cref="System.IO.IOException">When target file exists and should not overwrite</exception>
  public static IFileReport CopyToAsync(this FileInfo @this, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = -1) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);

    if (!File.Exists(@this.FullName))
      throw new FileNotFoundException(string.Format(_EX_SOURCE_FILE_DOES_NOT_EXIST, @this.FullName));

    // special hard link handling
    var sourceIsHardLink = @this.IsHardLink();
    if (sourceIsHardLink) {
      var targets = @this.GetHardLinkTargets();

      // if target file is already a hard link of the source, don't copy
      if (targets.Any(t => t.FullName == target.FullName)) {
        FileCopyOperation operation = new(@this, target, callback, false);
        operation.OperationStarted();
        return operation.OperationFinished();
      }
    }

    if (File.Exists(target.FullName)) {
      if (!overwrite)
        throw new IOException(string.Format(_EX_TARGET_FILE_ALREADY_EXISTS, target.FullName));

      File.Delete(target.FullName);
    }

    // create symlink at target with the same source
    if (dontResolveSymbolicLinks) {
      var copySymlink = _CopySymbolicLinkIfNeeded(@this, target, callback);
      if (copySymlink != null)
        return copySymlink;
    }

    FileCopyOperation mainToken = new(@this, target, callback, !@this.IsOnSamePhysicalDrive(target), bufferSize);

    // create link if possible and allowed
    if (allowHardLinks && @this.TryCreateHardLinkAt(target)) {
      // link creation successful
      mainToken.OperationStarted();
      mainToken.CreatedLink();
      return mainToken.OperationFinished();
    }

    // do copy
    var result = mainToken.OperationStarted();

    // open streams
    try {
      mainToken.SourceStream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, mainToken.ChunkSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

      // set target size first to avoid fragmentation
      _SetFileSize(target, Math.Min(mainToken.TotalSize, _MAX_PREALLOCATION_SIZE));

      mainToken.TargetStream = new(target.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, mainToken.ChunkSize, FileOptions.Asynchronous | (mainToken.TotalSize < _MAX_CACHED_SIZE ? FileOptions.None : FileOptions.WriteThrough));
    } catch (Exception e) {
      return mainToken.OperationAborted(e);
    }

    // trigger start reading
    if (mainToken.BytesToTransfer > 0)
      mainToken.StartReading();
    else
      result = mainToken.OperationFinished();
    return result;
  }

  /// <summary>
  ///   Copies a symbolic link if needed.
  /// </summary>
  /// <param name="source">The source.</param>
  /// <param name="target">The target.</param>
  /// <param name="callback">The callback.</param>
  /// <returns>The last report or <c>null</c>.</returns>
  private static IFileReport _CopySymbolicLinkIfNeeded(FileInfo source, FileInfo target, FileReportCallback callback) {
    if (!source.IsSymbolicLink())
      return null;

    FileCopyOperation operation = new(source, target, callback, false);
    operation.OperationStarted();
    try {
      var targets = source.GetSymbolicLinkTarget();
      target.CreateSymbolicLinkAt(targets);
      operation.CreatedLink();
      return operation.OperationFinished();
    } catch (Exception e) {
      return operation.OperationAborted(e);
    }
  }

  /// <summary>
  ///   Sets the file size for a given file.
  /// </summary>
  /// <param name="target">The target.</param>
  /// <param name="length">The length.</param>
  private static void _SetFileSize(FileInfo target, long length) {
    using FileStream targetStream = new(target.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 8, FileOptions.WriteThrough);
    targetStream.SetLength(length);
  }

  #endregion
}
