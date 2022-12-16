#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Concurrent;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Threading;

namespace System.IO {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  partial class FastFileOperations {

    public delegate void FileReportCallback(IFileReport report);

    private class FileCopyOperation : IFileOperation {

      #region nested types
      /// <summary>
      /// A report from a running file-copy operation.
      /// </summary>
      private class FileCopyReport : IFileReport {
        #region fields
        private readonly ReportType _reportType;
        private readonly FileCopyOperation _operation;
        private readonly FileSystemInfo _source;
        private readonly FileSystemInfo _target;
        private readonly int _streamIndex;
        private readonly long _streamOffset;
        private readonly long _streamSize;
        private readonly long _chunkOffset;
        private readonly long _chunkSize;

        #endregion

        #region Implementation of IFileReport
        public ReportType ReportType => this._reportType;
        public IFileSystemOperation Operation => this._operation;
        public int StreamIndex => this._streamIndex;
        public long StreamOffset => this._streamOffset;
        public long ChunkOffset => this._chunkOffset;
        public long ChunkSize => this._chunkSize;
        public long StreamSize => this._streamSize;
        public FileSystemInfo Source => this._source;
        public FileSystemInfo Target => this._target;
        public ContinuationType ContinuationType { get; set; }

        #endregion
        public FileCopyReport(ReportType reportType, FileCopyOperation operation, int streamIndex, long chunkOffset, long chunkSize) {
          this._reportType = reportType;
          this._operation = operation;
          this._streamIndex = streamIndex;
          this._chunkOffset = chunkOffset;
          this._chunkSize = chunkSize;
          this._source = operation.Source;
          this._target = operation.Target;
          this._streamOffset = 0;
          this._streamSize = operation.BytesToTransfer;
          this.ContinuationType = reportType == ReportType.AbortedOperation ? ContinuationType.AbortOperation : ContinuationType.Proceed;
        }
      }

      /// <summary>
      /// A bunch of data that is read or being written.
      /// </summary>
      private class Chunk {
        public readonly long offset;
        public readonly byte[] data;
        public int length;
        public Chunk(long offset, int size) {
          this.offset = offset;
          this.data = new byte[size];
          this.length = size;
        }
      }
      #endregion

      #region fields
      private readonly FileInfo _source;
      private readonly FileInfo _target;
      private readonly FileReportCallback _callback;
      private readonly ManualResetEventSlim _finishEvent = new ManualResetEventSlim();
      private readonly bool _canReadDuringWrite;
      private readonly int _chunkSize;
      private readonly long _maxReadAheadSize;
      private readonly ConcurrentQueue<Chunk> _readAheadCache = new ConcurrentQueue<Chunk>();
      private Exception _exception;
      private int _streamCount = 1;
      private long _bytesRead;
      private long _bytesTransferred;
      private long _readOffset;
      #endregion

      #region props
      public FileStream SourceStream { private get; set; }
      public FileStream TargetStream { private get; set; }
      public int ChunkSize => this._chunkSize;
      private long _CurrentReadAheadSize { get { return this._readAheadCache.ToArray().Sum(c => c.length); } }

      private bool _CanRead {
        get {
          if (this.IsDone)
            return false;

          if (this._canReadDuringWrite)
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
      
      public FileCopyOperation(FileInfo source, FileInfo target, FileReportCallback callback, bool canReadDuringWrite, int chunkSize = -1, int maxReadAheadSize = -1) {
        this._source = source;
        this._target = target;
        this._callback = callback ?? (_ => { });
        this._canReadDuringWrite = canReadDuringWrite;
        this._chunkSize = chunkSize > 0 ? chunkSize : source.Length < _MAX_SMALL_FILESIZE ? _DEFAULT_BUFFER_SIZE : _DEFAULT_LARGE_BUFFER_SIZE;
        this._maxReadAheadSize = maxReadAheadSize > 0 ? maxReadAheadSize : _MAX_READ_AHEAD;
      }

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

        var size = this._chunkSize;
        var offset = Interlocked.Add(ref this._readOffset, size) - size;
        if (offset >= this.TotalSize)
          return null;

        this._CreateReport(ReportType.StartRead, 0, offset, size);
        return new Chunk(offset, size);
      }

      private void _ReleaseReadChunk(Chunk chunk) {
#if SUPPORTS_CONTRACTS
        Contract.Requires(chunk != null);
#endif
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
#if SUPPORTS_CONTRACTS
        Contract.Requires(chunk != null);
#endif
        var result = Interlocked.Add(ref this._bytesTransferred, chunk.length);
        this._CreateReport(ReportType.FinishedWrite, 0, chunk.offset, chunk.length);

        if (result >= this.TotalSize)
          this.OperationFinished();
      }
      #endregion

      #region appdomain messing
      /// <summary>
      /// Handles application exit and deletes any unfinished target file.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
      private void _AppDomainUnload(object sender, EventArgs args) {
        this.OperationAborted(new OperationCanceledException(_EX_APP_UNLOAD));
      }

      /// <summary>
      /// Registers an event handler to the ProcessExit event of the current appdomain.
      /// </summary>
      private void _RegisterToAppUnload() {
        AppDomain.CurrentDomain.ProcessExit += this._AppDomainUnload;
      }

      /// <summary>
      /// Unregisters the event handler from the ProcessExit event of the current appdomain.
      /// </summary>
      private void _UnregisterFromAppUnload() {
        AppDomain.CurrentDomain.ProcessExit -= this._AppDomainUnload;
      }
      #endregion

      /// <summary>
      /// Tries to dispose a stream.
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
        var result = new FileCopyReport(reportType, this, streamIndex, offset, size);
        this._callback(result);
        return result;
      }

      public IFileReport OperationStarted() {
        this._RegisterToAppUnload();
        return this._CreateReport(ReportType.StartOperation, -1, 0, this.TotalSize);
      }

      public IFileReport OperationFinished() {
        this._Dispose();
        this._target.Attributes = this._source.Attributes;
        this._target.CreationTimeUtc = this._source.CreationTimeUtc;
        this._target.LastWriteTimeUtc = this._source.LastWriteTimeUtc;

        this._finishEvent.Set();
        return this._CreateReport(ReportType.FinishedOperation, -1, 0, this.TotalSize);
      }

      public IFileReport OperationAborted(Exception exception) {
        this._exception = exception;
        var result = this._CreateReport(ReportType.AbortedOperation, -1, 0, this.TotalSize);
        if (result.ContinuationType == ContinuationType.AbortOperation) {
          this._Dispose();
          this._target.Delete();
          this._finishEvent.Set();
        }
        return result;
      }

      public IFileReport CreatedLink() {
        return this._CreateReport(ReportType.CreatedLink, -1, 0, this.TotalSize);
      }
      #endregion

      #region async read/write
      /// <summary>
      /// Starts the reading.
      /// </summary>
      public void StartReading() {
        this._ReadCallback(null);
      }

      /// <summary>
      /// The read chunk callback.
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

            if (result.ContinuationType == ContinuationType.Proceed) {
              bytesRead = chunk.length;
            } else if (result.ContinuationType == ContinuationType.RetryChunk && !this.IsDone) {
              this._BeginReadChunk(chunk);
              return;
            } else if (result.ContinuationType == ContinuationType.RetryStream && !this.IsDone) {
              // TODO: abort other threads to continue writing, reset both streams, restart reading
              throw new NotImplementedException();
            } else {
              return;
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
      /// Begins reading the given chunk.
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
          if (result.ContinuationType == ContinuationType.Proceed)
            this._ReadCallback(null);
          else if (result.ContinuationType == ContinuationType.RetryChunk && !this.IsDone)
            this._BeginReadChunk(chunk);
          else if (result.ContinuationType == ContinuationType.RetryStream && !this.IsDone)
            // TODO: abort other threads to continue writing, reset both streams, restart reading
            throw new NotImplementedException();
        }
      }

      /// <summary>
      /// The write callback.
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

            if (result.ContinuationType == ContinuationType.Proceed) {
              ;
            } else if (result.ContinuationType == ContinuationType.RetryChunk && !this.IsDone) {
              this._BeginWriteChunk(chunk);
              return;
            } else if (result.ContinuationType == ContinuationType.RetryStream && !this.IsDone) {
              // TODO: abort other threads to continue writing, reset both streams, restart reading
              throw new NotImplementedException();
            } else {
              return;
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
      /// Begins writing the given chunk.
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
          if (result.ContinuationType == ContinuationType.Proceed)
            this._WriteCallback(null);
          else if (result.ContinuationType == ContinuationType.RetryChunk && !this.IsDone)
            this._BeginWriteChunk(chunk);
          else if (result.ContinuationType == ContinuationType.RetryStream && !this.IsDone)
            // TODO: abort other threads to continue writing, reset both streams, restart reading
            throw new NotImplementedException();
        }
      }

      #endregion

      #region Implementation of IFileOperation
      public FileSystemInfo Source => this._source;
      public FileSystemInfo Target => this._target;
      public long TotalSize => this._source.Length;
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
      public Exception Exception => this._exception;
      public bool IsDone => this._finishEvent.IsSet;
      public bool ThrewException => this._exception != null;

      public void Abort() {
        this.OperationAborted(new OperationCanceledException(_EX_USER_ABORT));
      }

      public void WaitTillDone() {
        this._finishEvent.Wait();
      }

      public bool WaitTillDone(TimeSpan timeout) {
        return this._finishEvent.Wait(timeout);
      }
      #endregion
    }

    #region copy file
    /// <summary>
    /// Copies the file.
    /// </summary>
    /// <param name="This">This source file.</param>
    /// <param name="target">The target file.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites existing; otherwise throws IOException.</param>
    /// <param name="allowHardLinks">if set to <c>true</c> allows creation of hard links.</param>
    /// <param name="dontResolveSymbolicLinks">if set to <c>true</c> [dont resolve symbolic links].</param>
    /// <param name="callback">The callback, if any.</param>
    /// <param name="allowedStreams">The allowed number of streams.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <exception cref="System.IO.FileNotFoundException">When source file does not exist</exception>
    /// <exception cref="System.IO.IOException">When target file exists and should not overwrite</exception>
    public static void CopyTo(this FileInfo This, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = _DEFAULT_BUFFER_SIZE) {
      var token = CopyToAsync(This, target, overwrite, allowHardLinks, dontResolveSymbolicLinks, callback, allowedStreams, bufferSize);
      token.Operation.WaitTillDone();
      if (token.Operation.ThrewException)
        throw token.Operation.Exception;
    }

    /// <summary>
    /// Copies the file asynchronous.
    /// This reads one chunk at a time and when the OS signals that a read is done, it starts writing that chunk while, possibly at the same time, reading the next.
    /// </summary>
    /// <param name="This">This source file.</param>
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
    public static IFileReport CopyToAsync(this FileInfo This, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = -1) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(target != null);
#endif

      if (!File.Exists(This.FullName))
        throw new FileNotFoundException(string.Format(_EX_SOURCE_FILE_DOES_NOT_EXIST, This.FullName));

      // special hard link handling
      var sourceIsHardLink = This.IsHardLink();
      if (sourceIsHardLink) {
        var targets = This.GetHardLinkTargets();

        // if target file is already a hard link of the source, don't copy
        if (targets.Any(t => t.FullName == target.FullName)) {
          var operation = new FileCopyOperation(This, target, callback, false);
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
        var copySymlink = _CopySymbolicLinkIfNeeded(This, target, callback);
        if (copySymlink != null)
          return copySymlink;
      }

      var mainToken = new FileCopyOperation(This, target, callback, !This.IsOnSamePhysicalDrive(target), bufferSize);

      // create link if possible and allowed
      if (allowHardLinks && This.TryCreateHardLinkAt(target)) {

        // link creation successful
        mainToken.OperationStarted();
        mainToken.CreatedLink();
        return mainToken.OperationFinished();
      }

      // do copy
      var result = mainToken.OperationStarted();

      // open streams
      try {
        mainToken.SourceStream = new FileStream(This.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, mainToken.ChunkSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        // set target size first to avoid fragmentation
        _SetFileSize(target, Math.Min(mainToken.TotalSize, _MAX_PREALLOCATION_SIZE));

        mainToken.TargetStream = new FileStream(target.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, mainToken.ChunkSize, FileOptions.Asynchronous | (mainToken.TotalSize < _MAX_CACHED_SIZE ? FileOptions.None : FileOptions.WriteThrough));
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
    /// Copies a symbolic link if needed.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>The last report or <c>null</c>.</returns>
    private static IFileReport _CopySymbolicLinkIfNeeded(FileInfo source, FileInfo target, FileReportCallback callback) {
      if (!source.IsSymbolicLink())
        return null;

      var operation = new FileCopyOperation(source, target, callback, false);
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
    /// Sets the file size for a given file.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="length">The length.</param>
    private static void _SetFileSize(FileInfo target, long length) {
      using (var targetStream = new FileStream(target.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 8, FileOptions.WriteThrough))
        targetStream.SetLength(length);
    }

    #endregion

  }
}
