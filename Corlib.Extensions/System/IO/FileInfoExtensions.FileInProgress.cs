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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LineBreakMode = System.StringExtensions.LineBreakMode;

namespace System.IO;

static partial class FileInfoExtensions {
  /// <summary>
  ///   A sealed class implementing <see cref="IFileInProgress" /> for handling file operations that might be rolled back or
  ///   committed.
  /// </summary>
  /// <remarks>
  ///   This class provides a robust mechanism for modifying files by working with a temporary copy until changes are either
  ///   finalized
  ///   by replacing the original file or discarded. It ensures that operations do not directly affect the original file,
  ///   minimizing
  ///   the risk of data loss or corruption during processing. The class implements <see cref="IDisposable" /> to clean up
  ///   resources,
  ///   particularly the temporary file, ensuring no leftover files consume disk space unintentionally.
  /// </remarks>
  private sealed class FileInProgress : IFileInProgress {
    private readonly PathExtensions.ITemporaryFileToken _token;
    private readonly DateTime? _capturedLastWriteTime;
    private readonly byte[] _capturedHash;
    private readonly FileStream _lockStream;
    private readonly FileStream _tempStream;
    private bool _isDisposed;
    private bool _isLockReleased;
    private bool _isTempReleased;

    /// <summary>
    ///   A stream wrapper that ignores Close/Dispose calls, allowing StreamReader/StreamWriter
    ///   to be disposed without closing the underlying stream. This provides cross-framework
    ///   compatibility for frameworks that don't support the leaveOpen parameter.
    /// </summary>
    private sealed class NonClosingStreamWrapper(Stream inner) : Stream {
      public override bool CanRead => inner.CanRead;
      public override bool CanSeek => inner.CanSeek;
      public override bool CanWrite => inner.CanWrite;
      public override long Length => inner.Length;
      public override long Position { get => inner.Position; set => inner.Position = value; }
      public override void Flush() => inner.Flush();
      public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
      public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
      public override void SetLength(long value) => inner.SetLength(value);
      public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
      protected override void Dispose(bool disposing) { /* intentionally empty - don't close inner stream */ }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FileInProgress" /> class for managing modifications to the specified
    ///   source file with the specified conflict resolution mode, optionally copying the source file's contents.
    /// </summary>
    /// <param name="sourceFile">The source <see cref="FileInfo" /> object representing the file to be modified.</param>
    /// <param name="conflictMode">The conflict resolution mode to use for this operation.</param>
    /// <param name="copyContents">
    ///   If <see langword="true" />, copies the source file's contents to the temporary file.
    ///   For lock modes, the copy is performed before acquiring the lock to avoid self-blocking.
    /// </param>
    /// <remarks>
    ///   This constructor creates a temporary file in the same directory as the source file. The temporary file is used to
    ///   accumulate changes. Based on the conflict mode, this constructor may also acquire a lock on the original file
    ///   or capture its current state for later verification.
    /// </remarks>
    public FileInProgress(FileInfo sourceFile, ConflictResolutionMode conflictMode = ConflictResolutionMode.None, bool copyContents = false) {
      this.OriginalFile = sourceFile;
      this.ConflictMode = conflictMode;
      this._token = PathExtensions.GetTempFileToken(sourceFile.Name + ".$$$", sourceFile.DirectoryName);

      // Always lock temp file exclusively - no other process should interfere with our working copy
      this._tempStream = new(this._TemporaryFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

      switch (conflictMode) {
        case ConflictResolutionMode.LockWithReadShare:
          this._lockStream = new(sourceFile.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
          break;
        case ConflictResolutionMode.LockExclusive:
          this._lockStream = new(sourceFile.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
          break;
        case ConflictResolutionMode.CheckLastWriteTimeAndThrow:
        case ConflictResolutionMode.CheckLastWriteTimeAndIgnoreUpdate:
          sourceFile.Refresh();
          this._capturedLastWriteTime = sourceFile.LastWriteTimeUtc;
          break;
        case ConflictResolutionMode.CheckChecksumAndThrow:
        case ConflictResolutionMode.CheckChecksumAndIgnoreUpdate:
          this._capturedHash = _ComputeSHA256(sourceFile);
          break;
      }

      if (!copyContents)
        return;

      // Copy contents: from locked original stream if available, otherwise directly from file
      if (this._lockStream != null) {
        this._lockStream.Seek(0, SeekOrigin.Begin);
        this._lockStream.CopyTo(this._tempStream);
      } else {
        using var source = sourceFile.OpenRead();
        source.CopyTo(this._tempStream);
      }

      this._tempStream.Flush();
    }

    ~FileInProgress() => this._Dispose(false);

    #region Implementation of IDisposable

    /// <inheritdoc />
    public void Dispose() {
      this._Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void _Dispose(bool disposing) {
      if (this._isDisposed)
        return;

      this._isDisposed = true;

      // Finalizer: only release unmanaged resources, don't access other managed objects
      if (!disposing) {
        this._token.Dispose();
        return;
      }

      try {
        if (this.CancelChanges)
          return;

        // For lock modes, write through the locked stream to avoid race conditions
        if (this._lockStream != null && !this._isLockReleased) {
          this._WriteThrough();
          return;
        }

        // For non-lock modes: flush and release temp stream before file operations
        this._tempStream.Flush(true);
        this._ReleaseTempStream();

        this._TemporaryFile.Refresh();
        if (!this._TemporaryFile.Exists)
          return;

        // Check if original file still exists
        this.OriginalFile.Refresh();
        if (!this.OriginalFile.Exists) {
          // Original was deleted - atomically move temp to target location
          _AtomicMove(this._TemporaryFile, this.OriginalFile);
          return;
        }

        // Check for conflicts based on mode
        if (this._HasConflict()) {
          if (this.ConflictMode is ConflictResolutionMode.CheckLastWriteTimeAndThrow or ConflictResolutionMode.CheckChecksumAndThrow)
            throw new FileConflictException($"File '{this.OriginalFile.FullName}' was modified externally during the work-in-progress operation.");

          // IgnoreUpdate modes: skip applying changes, just cleanup
          return;
        }

        // Atomically replace original with temp contents
        _AtomicReplace(this.OriginalFile, this._TemporaryFile);
      } finally {
        this._ReleaseTempStream();
        this._ReleaseLock();
        this._token.Dispose();
      }
    }

    private void _WriteThrough() {
      // Flush temp stream to ensure all writes are complete
      this._tempStream.Flush(true);
      var newLength = this._tempStream.Length;

      // Set length first - if file grows, OS can pre-allocate clusters; if shrinks, data is truncated after copy
      this._lockStream.SetLength(newLength);
      this._lockStream.Seek(0, SeekOrigin.Begin);
      this._tempStream.Seek(0, SeekOrigin.Begin);

      // Copy from our locked temp stream to the locked original
      this._tempStream.CopyTo(this._lockStream);

      // Flush to disk to ensure durability
      this._lockStream.Flush(true);
    }

    private void _ReleaseLock() {
      if (this._lockStream == null || this._isLockReleased)
        return;

      this._isLockReleased = true;
      this._lockStream.Dispose();
    }

    private void _ReleaseTempStream() {
      if (this._isTempReleased)
        return;

      this._isTempReleased = true;
      this._tempStream.Dispose();
    }

    private bool _HasConflict() {
      switch (this.ConflictMode) {
        case ConflictResolutionMode.CheckLastWriteTimeAndThrow:
        case ConflictResolutionMode.CheckLastWriteTimeAndIgnoreUpdate:
          this.OriginalFile.Refresh();
          return this.OriginalFile.LastWriteTimeUtc != this._capturedLastWriteTime;
        case ConflictResolutionMode.CheckChecksumAndThrow:
        case ConflictResolutionMode.CheckChecksumAndIgnoreUpdate:
          var currentHash = _ComputeSHA256(this.OriginalFile);
          return !currentHash.SequenceEqual(this._capturedHash);
        default:
          return false;
      }
    }

    private static byte[] _ComputeSHA256(FileInfo file) {
      using var provider = SHA256.Create();
      using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
      return provider.ComputeHash(stream);
    }

    /// <summary>
    ///   Atomically moves source to target location, even if target doesn't exist.
    ///   Handles cross-volume scenarios by creating a temp on target volume first.
    /// </summary>
    private static void _AtomicMove(FileInfo source, FileInfo target) {
      // Flush source to ensure data is on disk before any move
      using (var fs = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        fs.Flush(true);

      // Try direct move first (atomic on same volume)
      try {
        source.MoveTo(target.FullName);
        return;
      } catch (IOException) {
        // Likely cross-volume - need to copy to temp on target volume, then rename
      }

      // Cross-volume: create temp on target volume, copy there, then atomic rename
      var targetDir = target.DirectoryName ?? throw new InvalidOperationException("Target has no directory");
      var tempPath = Path.Combine(targetDir, target.Name + ".$$$");

      try {
        // Copy source to temp on target volume
        source.CopyTo(tempPath, true);

        // Flush to ensure data is on disk before rename
        using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          fs.Flush(true);

        // Atomic rename on target volume
        File.Move(tempPath, target.FullName);
      } catch {
        // Clean up temp on failure
        try {
          File.Delete(tempPath);
        } catch {
          // Ignore cleanup errors
        }

        throw;
      }
    }

    /// <summary>
    ///   Atomically replaces target with source content, with retry logic for transient locks.
    /// </summary>
    private static void _AtomicReplace(FileInfo target, FileInfo source, int maxRetries = 5, int delayMs = 100) {
      // Flush source to ensure data is on disk before replace
      using (var fs = new FileStream(source.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        fs.Flush(true);

      for (var attempt = 1; ; ++attempt) {
        try {
          // ReplaceWith uses File.Replace which is atomic
          target.ReplaceWith(source);
          return;
        } catch (IOException) when (attempt < maxRetries) {
          // File might be temporarily locked (antivirus, file system watchers, etc.)
          GC.Collect();
          GC.WaitForPendingFinalizers();
          Threading.Thread.Sleep(delayMs * attempt);
        }
      }
    }

    #endregion

    /// <summary>
    ///   Gets the temporary file associated with the current work-in-progress operation.
    /// </summary>
    /// <value>
    ///   A <see cref="FileInfo" /> representing the temporary file used for modifications until the work is finalized or
    ///   discarded.
    /// </value>
    /// <remarks>
    ///   This property provides access to the temporary file created to hold changes during the modification process. The
    ///   temporary file
    ///   is used to safely apply changes without directly affecting the original file until the operation is complete and
    ///   changes are
    ///   either committed or rolled back.
    /// </remarks>
    private FileInfo _TemporaryFile => this._token.File;

    #region Implementation of IFileInProgress

    /// <inheritdoc />
    public FileInfo OriginalFile { get; }

    /// <inheritdoc />
    public bool CancelChanges { get; set; }

    /// <inheritdoc />
    public ConflictResolutionMode ConflictMode { get; }

    /// <inheritdoc />
    public void CopyFrom(FileInfo source) {
      using var sourceStream = source.OpenRead();
      this._tempStream.Seek(0, SeekOrigin.Begin);
      this._tempStream.SetLength(0);
      sourceStream.CopyTo(this._tempStream);
      this._tempStream.Flush();
    }

    /// <inheritdoc />
    public Encoding GetEncoding() {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(new NonClosingStreamWrapper(this._tempStream), Encoding.UTF8, true);
      reader.Peek();
      return reader.CurrentEncoding;
    }

    /// <inheritdoc />
    public string ReadAllText() => this.ReadAllText(Encoding.UTF8);

    /// <inheritdoc />
    public string ReadAllText(Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(new NonClosingStreamWrapper(this._tempStream), encoding);
      return reader.ReadToEnd();
    }

    /// <inheritdoc />
    public IEnumerable<string> ReadLines() => this.ReadLines(Encoding.UTF8);

    /// <inheritdoc />
    public IEnumerable<string> ReadLines(Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(new NonClosingStreamWrapper(this._tempStream), encoding);
      while (reader.ReadLine() is { } line)
        yield return line;
    }

    /// <inheritdoc />
    public void WriteAllText(string text) => this.WriteAllText(text, Encoding.UTF8);

    /// <inheritdoc />
    public void WriteAllText(string text, Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      this._tempStream.SetLength(0);
      if (string.IsNullOrEmpty(text))
        return;
      using var writer = new StreamWriter(new NonClosingStreamWrapper(this._tempStream), encoding);
      writer.Write(text);
      writer.Flush();
    }

    /// <inheritdoc />
    public void WriteAllLines(IEnumerable<string> lines) => this.WriteAllLines(lines, Encoding.UTF8);

    /// <inheritdoc />
    public void WriteAllLines(IEnumerable<string> lines, Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      this._tempStream.SetLength(0);
      using var writer = new StreamWriter(new NonClosingStreamWrapper(this._tempStream), encoding);
      foreach (var line in lines)
        writer.WriteLine(line);
      writer.Flush();
    }

    /// <inheritdoc />
    public void AppendLine(string line) => this.AppendLine(line, Encoding.UTF8);

    /// <inheritdoc />
    public void AppendLine(string line, Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.End);
      using var writer = new StreamWriter(new NonClosingStreamWrapper(this._tempStream), encoding);
      writer.WriteLine(line);
      writer.Flush();
    }

    /// <inheritdoc />
    public void AppendAllLines(IEnumerable<string> lines) => this.AppendAllLines(lines, Encoding.UTF8);

    /// <inheritdoc />
    public void AppendAllLines(IEnumerable<string> lines, Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.End);
      using var writer = new StreamWriter(new NonClosingStreamWrapper(this._tempStream), encoding);
      foreach (var line in lines)
        writer.WriteLine(line);
      writer.Flush();
    }

    /// <inheritdoc />
    public void AppendAllText(string text) => this.AppendAllText(text, Encoding.UTF8);

    /// <inheritdoc />
    public void AppendAllText(string text, Encoding encoding) {
      this._tempStream.Seek(0, SeekOrigin.End);
      using var writer = new StreamWriter(new NonClosingStreamWrapper(this._tempStream), encoding);
      writer.Write(text);
      writer.Flush();
    }

    /// <inheritdoc />
    public FileStream Open(FileAccess access)
      => throw new InvalidOperationException("Cannot open additional handles on the work-in-progress file. Use the provided read/write methods instead.");


    /// <inheritdoc />
    public byte[] ReadAllBytes() {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      var buffer = new byte[this._tempStream.Length];
      _ = this._tempStream.Read(buffer, 0, buffer.Length);
      return buffer;
    }

    /// <inheritdoc />
    public void WriteAllBytes(byte[] data) {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      this._tempStream.SetLength(0);
      this._tempStream.Write(data, 0, data.Length);
      this._tempStream.Flush();
    }

    /// <inheritdoc />
    public IEnumerable<byte> ReadBytes() {
      this._tempStream.Seek(0, SeekOrigin.Begin);
      int b;
      while ((b = this._tempStream.ReadByte()) >= 0)
        yield return (byte)b;
    }

    /// <inheritdoc />
    public void KeepFirstLines(int count) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.KeepFirstLines(count);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void KeepFirstLines(int count, Encoding encoding) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.KeepFirstLines(count, encoding);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void KeepLastLines(int count) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.KeepLastLines(count);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void KeepLastLines(int count, Encoding encoding) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.KeepLastLines(count, encoding);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void RemoveFirstLines(int count) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.RemoveFirstLines(count);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void RemoveFirstLines(int count, Encoding encoding) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.RemoveFirstLines(count, encoding);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void RemoveLastLines(int count) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.RemoveLastLines(count);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    /// <inheritdoc />
    public void RemoveLastLines(int count, Encoding encoding) {
      // Flush and close our stream first to release all handles
      this._tempStream.Flush();
      this._tempStream.Close();
      try {
        // Use file-based operation which opens its own stream
        this._TemporaryFile.RemoveLastLines(count, encoding);
      } finally {
        // Reopen the stream
        this._ReopenTempStream();
      }
    }

    private void _ReopenTempStream() {
      // Use reflection to set the readonly field, or modify the field to not be readonly
      var field = typeof(FileInProgress).GetField("_tempStream", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      field?.SetValue(this, new FileStream(this._TemporaryFile.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
    }

    #endregion
  }
}
