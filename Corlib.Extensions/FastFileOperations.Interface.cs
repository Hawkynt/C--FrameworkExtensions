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

using System.Collections.Generic;

namespace System.IO;

public static partial class FastFileOperations {
  /// <summary>
  ///   The type of reason a report is done.
  /// </summary>
  public enum ReportType {
    StartOperation,
    FinishedOperation,
    AbortedOperation,
    CreatedLink,
    StartRead,
    StartWrite,
    FinishedRead,
    FinishedWrite,
  }

  /// <summary>
  ///   The type of continuation used.
  /// </summary>
  public enum ContinuationType {
    /// <summary>
    ///   Proceed normally with the current operation.
    /// </summary>
    Proceed,

    /// <summary>
    ///   Retries the current chunk.
    /// </summary>
    RetryChunk,

    /// <summary>
    ///   Retries the whole stream.
    /// </summary>
    RetryStream,

    /// <summary>
    ///   Queues the whole stream for later retry.
    /// </summary>
    QueueStream,

    /// <summary>
    ///   Aborts the currently running operation.
    /// </summary>
    AbortOperation,
  }

  public interface IFileSystemReport {
    ReportType ReportType { get; }
    IFileSystemOperation Operation { get; }
    int StreamIndex { get; }
    long StreamOffset { get; }
    long ChunkOffset { get; }
    long ChunkSize { get; }
    long StreamSize { get; }
    FileSystemInfo Source { get; }
    FileSystemInfo Target { get; }
    ContinuationType ContinuationType { get; set; }
  }

  public interface IFileReport : IFileSystemReport;

  public interface IDirectoryReport : IFileSystemReport;

  public interface IFileSystemOperation {
    FileSystemInfo Source { get; }
    FileSystemInfo Target { get; }
    long TotalSize { get; }
    long BytesToTransfer { get; }
    long BytesRead { get; }
    long BytesTransferred { get; }
    int StreamCount { get; set; }
    Exception Exception { get; }
    bool IsDone { get; }
    bool ThrewException { get; }
    void Abort();
    void WaitTillDone();
    bool WaitTillDone(TimeSpan timeout);
  }

  public interface IFileOperation : IFileSystemOperation;

  public interface IDirectoryOperation : IFileSystemOperation {
    int CrawlerCount { get; set; }
  }

  public interface IFileComparer : IEqualityComparer<FileInfo>;
}
