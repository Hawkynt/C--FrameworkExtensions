#region (c)2010-2042 Hawkynt
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
// TODO: base file offset in dircopy event tokens
// TODO: exception interceptor callback to allow continue or retry on exceptions in dircopy and filecopy
// TODO: more events to allow logging actions in dircopy
// TODO: symlink support  (source is sl, target should sl to exact same destination either relative or absolute)
// TODO: junction support (source is jt, target should jt to exact same destination either relative or absolute)
// TODO: hardlink support (source is hl, target should hl to same relative destinations)

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

using System.Collections.Concurrent;
using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Threading;

namespace System.IO {
  /// <summary>
  /// The fastest copy/move/delete/sync ops for files and directories.
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class FastFileOperations {
    partial interface IFileSystemOperation { }
    partial interface IDirectoryOperation { }
    partial interface IDirectoryReport { }
    partial interface IFileSystemReport { }
    partial interface IFileComparer { }
    partial class FileLengthComparer : IFileComparer { }
    partial class FileLastWriteTimeComparer : IFileComparer { }
    partial class FileSimpleAttributesComparer : IFileComparer { }
    partial class FileCreationTimeComparer : IFileComparer { }

    private class DirectoryCopyOperation : IDirectoryOperation {
      #region nested types

      private class DirectoryCopyReport : IDirectoryReport {
        #region fields

        private readonly ReportType _reportType;
        private readonly DirectoryCopyOperation _operation;
        private readonly int _streamIndex;
        private readonly long _streamOffset;
        private readonly long _chunkOffset;
        private readonly long _chunkSize;
        private readonly long _streamSize;
        private readonly FileSystemInfo _source;
        private readonly FileSystemInfo _target;
        private ContinuationType _continuationType;

        #endregion

        #region Implementation of IDirectoryReport
        public ReportType ReportType => this._reportType;
        public IFileSystemOperation Operation => this._operation;
        public int StreamIndex => this._streamIndex;
        public long StreamOffset => this._streamOffset;
        public long ChunkOffset => this._chunkOffset;
        public long ChunkSize => this._chunkSize;
        public long StreamSize => this._streamSize;
        public FileSystemInfo Source => this._source;
        public FileSystemInfo Target => this._target;

        public ContinuationType ContinuationType {
          get => this._continuationType;
          set => this._continuationType = value;
        }

        #endregion

        public DirectoryCopyReport(ReportType reportType, DirectoryCopyOperation operation, FileSystemInfo source, FileSystemInfo target, int streamIndex, long streamOffset, long chunkOffset, long chunkSize) {
          this._reportType = reportType;
          this._operation = operation;
          this._source = source;
          this._target = target;
          this._streamIndex = streamIndex;
          this._streamOffset = streamOffset;
          this._chunkOffset = chunkOffset;
          this._chunkSize = chunkSize;
          this._streamSize = source is FileInfo ? ((FileInfo)source).Length : 0;
          this._continuationType = reportType == ReportType.AbortedOperation ? ContinuationType.AbortOperation : ContinuationType.Proceed;
        }

      }
      #endregion

      #region fields

      private readonly IFileComparer[] _comparers =  {
        new FileLengthComparer(),
        new FileLastWriteTimeComparer(),
        new FileSimpleAttributesComparer(),
        new FileCreationTimeComparer(),
      };

      private readonly DirectoryInfo _source;
      private readonly DirectoryInfo _target;
      private readonly Action<IDirectoryReport> _callback;
      private readonly Func<FileSystemInfo, bool> _filter;
      private readonly ManualResetEventSlim _finishEvent = new ManualResetEventSlim();
      private readonly ConcurrentBag<string> _filesToCopy = new ConcurrentBag<string>();
      private readonly ConcurrentQueue<string> _directoriesToParse = new ConcurrentQueue<string>();
      private readonly bool _allowDelete;
      private readonly bool _allowOverwrite;
      private readonly bool _allowHardLinks;
      private readonly bool _allowIntegrate;
      private readonly bool _dontResolveSymbolicLinks;
      private long _totalSize;
      private long _bytesToTransfer;
      private long _bytesRead;
      private long _bytesTransferred;
      private long _bytesProcessing;

      private int _workingStreams;
      private int _streamCount = Math.Max(1, Environment.ProcessorCount >> 1);
      private int _lastStreamIndex;

      private int _workingCrawlers;
      private int _crawlerCount = Math.Max(1, Environment.ProcessorCount >> 1);
      private int _lastCrawlerIndex;

      private Exception _exception;
      #endregion

      #region props
      private bool IsSuccessfulEndConditionReached => Interlocked.Read(ref this._bytesTransferred) == Interlocked.Read(ref this._bytesToTransfer) && !this._filesToCopy.Any() && !this._directoriesToParse.Any();

      #endregion

      #region ctor,dtor
      public DirectoryCopyOperation(DirectoryInfo source, DirectoryInfo target, Action<IDirectoryReport> callback, Func<FileSystemInfo, bool> filter, bool allowDelete, bool allowOverwrite, bool allowHardLinks, bool dontResolveSymbolicLinks, bool allowIntegrate) {
        this._source = source;
        this._target = target;
        this._allowDelete = allowDelete;
        this._allowOverwrite = allowOverwrite;
        this._allowHardLinks = allowHardLinks;
        this._allowIntegrate = allowIntegrate;
        this._dontResolveSymbolicLinks = dontResolveSymbolicLinks;
        this._filter = filter ?? (_ => true);
        this._callback = callback ?? (_ => { });
      }

      private void _Dispose() {
        this._directoriesToParse.Clear();
        this._filesToCopy.Clear();
      }
      #endregion

      #region Implementation of IDirectoryOperation
      public FileSystemInfo Source => this._source;
      public FileSystemInfo Target => this._target;
      public long TotalSize => Interlocked.Read(ref this._totalSize);
      public long BytesToTransfer => Interlocked.Read(ref this._bytesToTransfer);
      public long BytesRead => Interlocked.Read(ref this._bytesRead);
      public long BytesTransferred => Interlocked.Read(ref this._bytesTransferred);

      public int StreamCount {
        get => this._streamCount;
        set {
          this._streamCount = Math.Max(1, value);
          Thread.MemoryBarrier();
          this._StartStreamThreads();
        }
      }

      public int CrawlerCount {
        get => this._crawlerCount;
        set {
          this._crawlerCount = Math.Max(1, value);
          Thread.MemoryBarrier();
          this._StartCrawlerThreads();
        }
      }

      public Exception Exception => this._exception;
      public bool IsDone => this._finishEvent.IsSet;
      public bool ThrewException => this._exception != null;

      public void Abort() {
        this.AbortOperation(new OperationCanceledException(_EX_USER_ABORT));
      }

      public void WaitTillDone() {
        this._finishEvent.Wait();
      }

      public bool WaitTillDone(TimeSpan timeout) {
        return this._finishEvent.Wait(timeout);
      }
      #endregion

      #region methods

      #region statistics
      private void _AddTotalBytes(long count) {
        Interlocked.Add(ref this._totalSize, count);
      }

      private void _AddBytesToTransfer(long count) {
        Interlocked.Add(ref this._bytesToTransfer, count);
      }

      private void _AddBytesRead(long count) {
        Interlocked.Add(ref this._bytesRead, count);
      }

      private void _AddBytesTransferred(long count) {
        Interlocked.Add(ref this._bytesTransferred, count);
      }

      private long _AddBytesProcessing(long count) {
        return Interlocked.Add(ref this._bytesProcessing, count) - count;
      }
      #endregion

      /// <summary>
      /// Tests if a fileinfo matches the given filter.
      /// </summary>
      /// <param name="info">The file/folder information.</param>
      /// <returns><c>true</c> if it matches; otherwise, <c>false</c>.</returns>
      private bool _MatchesFilter(FileSystemInfo info) {
#if SUPPORTS_CONTRACTS
        Contract.Requires(info != null);
#endif

        return this._filter(info);
      }

      /// <summary>
      /// Determines whether the given file needs to be copied to the target.
      /// </summary>
      /// <param name="sourceFile">The source file.</param>
      /// <param name="targetFile">The expected target file.</param>
      /// <returns><c>true</c> if the file needs to be copied; otherwise, <c>false</c>.</returns>
      private bool _FileNeedsSync(FileInfo sourceFile, FileInfo targetFile) {
#if SUPPORTS_CONTRACTS
        Contract.Requires(sourceFile != null);
        Contract.Requires(targetFile != null);
#endif
        if (!sourceFile.Exists)
          return false;

        if (!targetFile.Exists)
          return true;

        if (!this._allowOverwrite)
          return false;

        return !this._comparers.All(c => c.Equals(sourceFile, targetFile));
      }

      #region threads
      private void _StartStreamThreads() {
        for (var i = this._lastStreamIndex; i < this.StreamCount; ++i)
          this._TryStartStreamThread();
      }

      private void _StartCrawlerThreads() {
        for (var i = this._lastCrawlerIndex; i < this.CrawlerCount; ++i)
          this._TryStartCrawlerThread();
      }

      /// <summary>
      /// Creates the index for a new crawler thread.
      /// </summary>
      /// <returns>The new index or -1, if no more free crawlers available.</returns>
      private int _CreateCrawlerIndex() {
        var result = Interlocked.Increment(ref this._lastCrawlerIndex) - 1;
        if (result >= this._crawlerCount) {
          Interlocked.Decrement(ref this._lastCrawlerIndex);
          return -1;
        }

        return result;
      }

      /// <summary>
      /// Releases a crawler thread.
      /// </summary>
      private void _ReleaseCrawler() {
        var result = Interlocked.Decrement(ref this._lastCrawlerIndex);
        if (this.IsDone)
          return;

        if (result >= this._crawlerCount)
          return;

        this._StartCrawlerThreads();
      }

      /// <summary>
      /// Creates the index for a new stream thread.
      /// </summary>
      /// <returns>The new index or -1, if no more free streams available.</returns>
      private int _CreateStreamIndex() {
        var result = Interlocked.Increment(ref this._lastStreamIndex) - 1;
        if (result >= this._streamCount) {
          Interlocked.Decrement(ref this._lastStreamIndex);
          return -1;
        }

        return result;
      }

      /// <summary>
      /// Releases a stream thread.
      /// </summary>
      private void _ReleaseStream() {
        var result = Interlocked.Decrement(ref this._lastStreamIndex);
        if (this.IsDone)
          return;

        if (result >= this._streamCount)
          return;

        this._StartStreamThreads();
      }

      /// <summary>
      /// Tries to start a new crawler thread.
      /// </summary>
      /// <returns>The created thread instance or <c>null</c>.</returns>
      private Thread _TryStartCrawlerThread() {
        var index = this._CreateCrawlerIndex();
        if (index < 0)
          return null;

        var result = new Thread(this._CrawlerThread) {
          IsBackground = true,
          Name = "Crawler #" + index
        };
        result.Start(index);
        return result;
      }

      /// <summary>
      /// Tries to start a new stream thread.
      /// </summary>
      /// <returns>The created thread instance or <c>null</c>.</returns>
      private Thread _TryStartStreamThread() {
        var index = this._CreateStreamIndex();
        if (index < 0)
          return null;

        var result = new Thread(this._StreamThread) {
          IsBackground = true,
          Name = "Stream #" + index
        };
        result.Start(index);
        return result;
      }


      /// <summary>
      /// Crawls for files and directories to be copied.
      /// Note: Creates target directories and checks file sync status.
      /// </summary>
      /// <param name="state">The current crawler index.</param>
      private void _CrawlerThread(object state) {
        try {
          var index = (int)state;
          while (!this.IsDone && index < this.CrawlerCount) {

            string directoryName;
            if (!this._directoriesToParse.TryDequeue(out directoryName)) {

              // check end condition
              if (!this._directoriesToParse.Any() && this._workingCrawlers < 1 && !this._filesToCopy.Any() && this._workingStreams < 1) {
                this.FinishOperation();
                return;
              }

              Thread.Sleep(1);
              continue;
            }

            try {
              Interlocked.Increment(ref this._workingCrawlers);

              var sourceDirectory = new DirectoryInfo(Path.Combine(this._source.FullName, directoryName));
              var targetDirectory = new DirectoryInfo(Path.Combine(this._target.FullName, directoryName));

              if (targetDirectory.Exists && !this._allowIntegrate)
                if (this.AbortOperation(new IOException(string.Format(_EX_TARGET_DIRECTORY_ALREADY_EXISTS, targetDirectory.FullName))).ContinuationType == ContinuationType.AbortOperation)
                  return;

              if (!(targetDirectory.Exists || targetDirectory.TryCreate()))
                if (this.AbortOperation(new IOException(string.Format(_EX_COULD_NOT_CREATE_TARGET_DIR, targetDirectory.FullName))).ContinuationType == ContinuationType.AbortOperation)
                  return;

              targetDirectory.TrySetAttributes(sourceDirectory.Attributes);
              targetDirectory.TrySetCreationTimeUtc(sourceDirectory.CreationTimeUtc);
              targetDirectory.TrySetLastWriteTimeUtc(sourceDirectory.LastWriteTimeUtc);

              var dirs = new HashSet<string>();
              var files = new HashSet<string>();

              // find everything in the source dir to sync
              foreach (var item in sourceDirectory.EnumerateFileSystemInfos().Where(this._MatchesFilter)) {
                if (item is DirectoryInfo) {
                  dirs.Add(item.Name);
                  this._directoriesToParse.Enqueue(Path.Combine(directoryName, item.Name));
                  continue;
                }

                var fileInfo = item as FileInfo;
                if (fileInfo == null)
                  continue;

                files.Add(fileInfo.Name);
                this._AddTotalBytes(fileInfo.Length);

                if (!this._FileNeedsSync(fileInfo, targetDirectory.File(item.Name)))
                  continue;

                this._AddBytesToTransfer(fileInfo.Length);
                this._filesToCopy.Add(Path.Combine(directoryName, item.Name));
              }

              // delete everything in target dir, that is too much
              if (!this._TrySynchronizeTargetDirectory(targetDirectory, dirs, files))
                return;

            } finally {
              Interlocked.Decrement(ref this._workingCrawlers);
            }
          }
        } finally {
          this._ReleaseCrawler();
        }
      }

      /// <summary>
      /// Synchronizes the target directory if necessary.
      /// </summary>
      /// <param name="targetDirectory">The target directory.</param>
      /// <param name="dirs">The directories that should be present.</param>
      /// <param name="files">The files that should be present.</param>
      /// <returns></returns>
      private bool _TrySynchronizeTargetDirectory(DirectoryInfo targetDirectory, HashSet<string> dirs, HashSet<string> files) {
        if (!this._allowDelete)
          return true;

        foreach (var item in targetDirectory.EnumerateFileSystemInfos().Where(this._MatchesFilter)) {

          var directoryInfo = item as DirectoryInfo;
          if (directoryInfo != null) {
            if (!dirs.Contains(item.Name)) {
              try {
                directoryInfo.Delete(true);
              } catch (Exception e) {
                if (this.AbortOperation(e).ContinuationType == ContinuationType.AbortOperation)
                  return false;
              }
            }

            continue;
          }

          if (item is FileInfo) {
            if (!files.Contains(item.Name)) {
              try {
                item.Delete();
              } catch (Exception e) {
                if (this.AbortOperation(e).ContinuationType == ContinuationType.AbortOperation)
                  return false;
              }
            }
          }
        }

        return true;
      }

      /// <summary>
      /// Copies files in the files list.
      /// </summary>
      /// <param name="state">The current stream index.</param>
      private void _StreamThread(object state) {
        try {
          var index = (int)state;
          while (!this.IsDone && index < this.StreamCount) {
            string fileName;
            if (!this._filesToCopy.TryTake(out fileName)) {
              Thread.Sleep(1);
              continue;
            }

            try {
              Interlocked.Increment(ref this._workingStreams);
              var sourceFile = new FileInfo(Path.Combine(this._source.FullName, fileName));
              var targetFile = new FileInfo(Path.Combine(this._target.FullName, fileName));
              var baseOffset = this._AddBytesProcessing(sourceFile.Length);

              var token = sourceFile.CopyToAsync(targetFile, this._allowOverwrite, this._allowHardLinks, this._dontResolveSymbolicLinks, t => this._HandleFileOperationCallback(t, baseOffset, index));
              token.Operation.WaitTillDone();
              if (token.Operation.ThrewException)
                continue;

              sourceFile.Attributes &= ~FileAttributes.Archive;
              targetFile.Attributes &= ~FileAttributes.Archive;
            } finally {
              Interlocked.Decrement(ref this._workingStreams);
            }
          }
        } finally {
          this._ReleaseStream();
        }
      }

      /// <summary>
      /// This handles status callbacks from a file copy and invokes status updates from the current directory copy operation.
      /// </summary>
      /// <param name="fileSystemReport">The fileSystemReport.</param>
      /// <param name="streamOffset">The stream offset.</param>
      /// <param name="streamIndex">Index of the stream.</param>
      private void _HandleFileOperationCallback(IFileSystemReport fileSystemReport, long streamOffset, int streamIndex) {
        switch (fileSystemReport.ReportType) {
          case ReportType.StartRead:
          case ReportType.StartWrite: {
              this._CreateReport(fileSystemReport.ReportType, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              break;
            }
          case ReportType.FinishedRead: {
              this._AddBytesRead(fileSystemReport.ChunkSize);
              this._CreateReport(fileSystemReport.ReportType, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              break;
            }
          case ReportType.FinishedWrite: {
              this._AddBytesTransferred(fileSystemReport.ChunkSize);
              this._CreateReport(fileSystemReport.ReportType, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              break;
            }
          case ReportType.FinishedOperation: {
              if (this.IsSuccessfulEndConditionReached)
                this.FinishOperation();

              this._CreateReport(ReportType.FinishedWrite, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              break;
            }
          case ReportType.CreatedLink: {
              this._AddBytesTransferred(fileSystemReport.StreamSize);
              this._CreateReport(fileSystemReport.ReportType, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              break;
            }
          case ReportType.AbortedOperation: {
              var result = this._CreateReport(fileSystemReport.ReportType, fileSystemReport.Source, fileSystemReport.Target, streamIndex, streamOffset, fileSystemReport.ChunkOffset, fileSystemReport.ChunkSize);
              fileSystemReport.ContinuationType = result.ContinuationType;
              break;
            }
        }
      }
      #endregion
      #endregion

      #region generate reports
      private DirectoryCopyReport _CreateReport(ReportType reportType, FileSystemInfo source, FileSystemInfo target, int streamIndex, long streamOffset, long offset, long size) {
        var result = new DirectoryCopyReport(reportType, this, source, target, streamIndex, streamOffset, offset, size);
        this._callback(result);
        return result;
      }

      private DirectoryCopyReport AbortOperation(Exception exception) {
        this._exception = exception;
        var result = this._CreateReport(ReportType.AbortedOperation, this.Source, this.Target, -1, 0, 0, this.TotalSize);
        if (result.ContinuationType == ContinuationType.AbortOperation) {
          this._Dispose();
          this._finishEvent.Set();
        }
        return result;
      }


      public IDirectoryReport StartOperation() {
        this._directoriesToParse.Enqueue(".");
        this._StartCrawlerThreads();
        this._StartStreamThreads();
        return this._CreateReport(ReportType.StartOperation, this._source, this._target, -1, 0, 0, 0);
      }

      private DirectoryCopyReport FinishOperation() {
        this._Dispose();
        this._finishEvent.Set();
        return this._CreateReport(ReportType.FinishedOperation, this._source, this._target, -1, 0, 0, this.TotalSize);
      }
      #endregion

    }

    #region copy directory
    /// <summary>
    /// Copies a directory asynchronous.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="target">The target directory.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites all target files.</param>
    /// <param name="allowHardLinks">if set to <c>true</c> allows hard links instead of real copy.</param>
    /// <param name="dontResolveSymbolicLinks">if set to <c>true</c> doesnt resolve symbolic links (ie copies links instead of content).</param>
    /// <param name="allowIntegrate">if set to <c>true</c> allows integrating new files into existing directories.</param>
    /// <param name="synchronizeTarget">if set to <c>true</c> synchronizes target (ie delete files, not present in source).</param>
    /// <param name="predicate">The predicate to use for filtering source items.</param>
    /// <param name="callback">The callback for status reports.</param>
    /// <param name="crawlerThreads">The number of crawler threads.</param>
    /// <param name="streamThreads">The number of stream threads.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    /// <exception cref="System.IO.IOException"></exception>
    public static IDirectoryReport CopyToAsync(this DirectoryInfo This, DirectoryInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, bool allowIntegrate = false, bool synchronizeTarget = false, Func<FileSystemInfo, bool> predicate = null, Action<IDirectoryReport> callback = null, int crawlerThreads = -1, int streamThreads = -1) {
      if (!Directory.Exists(This.FullName))
        throw new FileNotFoundException(string.Format(_EX_SOURCE_DIRECTORY_DOES_NOT_EXIST, This.FullName));

      if (Directory.Exists(target.FullName)) {
        if (!(overwrite || synchronizeTarget || allowIntegrate))
          throw new IOException(string.Format(_EX_TARGET_DIRECTORY_ALREADY_EXISTS, target.FullName));

      }

      var operation = new DirectoryCopyOperation(
        This, target, callback, predicate, synchronizeTarget, overwrite, allowHardLinks, dontResolveSymbolicLinks, allowIntegrate
      );

      if (!Directory.Exists(target.FullName))
        target.Create();

      return operation.StartOperation();
    }

    /// <summary>
    /// Copies a directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="target">The target directory.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites all target files.</param>
    /// <param name="allowHardLinks">if set to <c>true</c> allows hard links instead of real copy.</param>
    /// <param name="dontResolveSymbolicLinks">if set to <c>true</c> doesnt resolve symbolic links (ie copies links instead of content).</param>
    /// <param name="allowIntegrate">if set to <c>true</c> allows integrating new files into existing directories.</param>
    /// <param name="synchronizeTarget">if set to <c>true</c> synchronizes target (ie delete files, not present in source).</param>
    /// <param name="predicate">The predicate to use for filtering source items.</param>
    /// <param name="callback">The callback for status reports.</param>
    /// <param name="crawlerThreads">The number of crawler threads.</param>
    /// <param name="streamThreads">The number of stream threads.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    /// <exception cref="System.IO.IOException"></exception>
    public static void CopyTo(this DirectoryInfo This, DirectoryInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, bool allowIntegrate = false, bool synchronizeTarget = false, Func<FileSystemInfo, bool> predicate = null, Action<IDirectoryReport> callback = null, int crawlerThreads = -1, int streamThreads = -1) {
      var token = CopyToAsync(This, target, overwrite, allowHardLinks, dontResolveSymbolicLinks, allowIntegrate, synchronizeTarget, predicate, callback, crawlerThreads, streamThreads);
      token.Operation.WaitTillDone();
      if (token.Operation.ThrewException)
        throw token.Operation.Exception;
    }
    #endregion
  }
}
