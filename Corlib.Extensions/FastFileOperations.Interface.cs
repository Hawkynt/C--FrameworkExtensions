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

using System.Collections.Generic;

namespace System.IO;

public static partial class FastFileOperations {
  /// <summary>
  /// The type of reason a report is done.
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
  /// The type of continuation used.
  /// </summary>
  public enum ContinuationType {
    /// <summary>
    /// Proceed normally with the current operation.
    /// </summary>
    Proceed,
    /// <summary>
    /// Retries the current chunk.
    /// </summary>
    RetryChunk,
    /// <summary>
    /// Retries the whole stream.
    /// </summary>
    RetryStream,
    /// <summary>
    /// Queues the whole stream for later retry.
    /// </summary>
    QueueStream,
    /// <summary>
    /// Aborts the currently running operation.
    /// </summary>
    AbortOperation,
  }

  public partial interface IFileSystemReport {
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

  public partial interface IFileReport : IFileSystemReport { }
  public partial interface IDirectoryReport : IFileSystemReport { }

  public partial interface IFileSystemOperation {
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

  public partial interface IFileOperation : IFileSystemOperation { }

  public partial interface IDirectoryOperation : IFileSystemOperation {
    int CrawlerCount { get; set; }
  }

  public partial interface IFileComparer : IEqualityComparer<FileInfo> { }
}
