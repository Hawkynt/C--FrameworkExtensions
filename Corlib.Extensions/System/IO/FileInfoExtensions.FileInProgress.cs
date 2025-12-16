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
using System.Text;

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
    private bool _isDisposed;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FileInProgress" /> class for managing modifications to the specified
    ///   source file.
    /// </summary>
    /// <param name="sourceFile">The source <see cref="FileInfo" /> object representing the file to be modified.</param>
    /// <remarks>
    ///   This constructor creates a temporary file in the same directory as the source file. The temporary file is used to
    ///   accumulate changes.
    ///   The temporary file's name is derived from the source file's name with an added ".$$$" extension to denote its
    ///   temporary status.
    /// </remarks>
    /// <example>
    ///   <code>
    /// FileInfo sourceFile = new FileInfo(@"C:\path\to\your\file.txt");
    /// var fileInProgress = new FileInProgress(sourceFile);
    /// // Perform operations with fileInProgress
    /// </code>
    ///   This example demonstrates creating a <see cref="FileInProgress" /> instance to manage changes to a file without
    ///   directly affecting the original file.
    /// </example>
    public FileInProgress(FileInfo sourceFile) {
      this.OriginalFile = sourceFile;
      this._token = PathExtensions.GetTempFileToken(sourceFile.Name + ".$$$", sourceFile.DirectoryName);
    }

    ~FileInProgress() => this.Dispose();

    #region Implementation of IDisposable

    /// <inheritdoc />
    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      GC.SuppressFinalize(this);

      if (!this.CancelChanges)
        _ReplaceWithRetry(this.OriginalFile, this._TemporaryFile);

      this._token.Dispose();
    }

    private static void _ReplaceWithRetry(FileInfo target, FileInfo source, int maxRetries = 3, int delayMs = 100) {
      for (var attempt = 1; ; ++attempt) {
        try {
          target.ReplaceWith(source);
          return;
        } catch (IOException) when (attempt < maxRetries) {
          // File might be temporarily locked due to lingering handles, force cleanup and retry
          GC.Collect();
          GC.WaitForPendingFinalizers();
          Threading.Thread.Sleep(delayMs);
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
    public void CopyFrom(FileInfo source) => source.CopyTo(this._TemporaryFile, true);

    /// <inheritdoc />
    public Encoding GetEncoding() => this._TemporaryFile.GetEncoding();

    /// <inheritdoc />
    public string ReadAllText() => this._TemporaryFile.ReadAllText();

    /// <inheritdoc />
    public string ReadAllText(Encoding encoding) => this._TemporaryFile.ReadAllText(encoding);

    /// <inheritdoc />
    public IEnumerable<string> ReadLines() => this._TemporaryFile.ReadLines();

    /// <inheritdoc />
    public IEnumerable<string> ReadLines(Encoding encoding) => this._TemporaryFile.ReadLines(encoding);

    /// <inheritdoc />
    public void WriteAllText(string text) => this._TemporaryFile.WriteAllText(text);

    /// <inheritdoc />
    public void WriteAllText(string text, Encoding encoding) => this._TemporaryFile.WriteAllText(text, encoding);

    /// <inheritdoc />
    public void WriteAllLines(IEnumerable<string> lines) => this._TemporaryFile.WriteAllLines(lines);

    /// <inheritdoc />
    public void WriteAllLines(IEnumerable<string> lines, Encoding encoding) => this._TemporaryFile.WriteAllLines(lines, encoding);

    /// <inheritdoc />
    public void AppendLine(string line) => this._TemporaryFile.AppendLine(line);

    /// <inheritdoc />
    public void AppendLine(string line, Encoding encoding) => this._TemporaryFile.AppendLine(line, encoding);

    /// <inheritdoc />
    public void AppendAllLines(IEnumerable<string> lines) => this._TemporaryFile.AppendAllLines(lines);

    /// <inheritdoc />
    public void AppendAllLines(IEnumerable<string> lines, Encoding encoding) => this._TemporaryFile.AppendAllLines(lines, encoding);

    /// <inheritdoc />
    public void AppendAllText(string text) => this._TemporaryFile.AppendAllText(text);

    /// <inheritdoc />
    public void AppendAllText(string text, Encoding encoding) => this._TemporaryFile.AppendAllText(text, encoding);

    /// <inheritdoc />
    public FileStream Open(FileAccess access) => this._TemporaryFile.Open(FileMode.OpenOrCreate, access, FileShare.None);

    /// <inheritdoc />
    public byte[] ReadAllBytes() => this._TemporaryFile.ReadAllBytes();

    /// <inheritdoc />
    public void WriteAllBytes(byte[] data) => this._TemporaryFile.WriteAllBytes(data);

    /// <inheritdoc />
    public IEnumerable<byte> ReadBytes() => this._TemporaryFile.ReadBytes();

    /// <inheritdoc />
    public void KeepFirstLines(int count) => this._TemporaryFile.KeepFirstLines(count);

    /// <inheritdoc />
    public void KeepFirstLines(int count, Encoding encoding) => this._TemporaryFile.KeepFirstLines(count, encoding);

    /// <inheritdoc />
    public void KeepLastLines(int count) => this._TemporaryFile.KeepLastLines(count);

    /// <inheritdoc />
    public void KeepLastLines(int count, Encoding encoding) => this._TemporaryFile.KeepLastLines(count, encoding);

    /// <inheritdoc />
    public void RemoveFirstLines(int count, Encoding encoding) => this._TemporaryFile.RemoveFirstLines(count, encoding);

    /// <inheritdoc />
    public void RemoveFirstLines(int count) => this._TemporaryFile.RemoveFirstLines(count);

    #endregion
  }
}
