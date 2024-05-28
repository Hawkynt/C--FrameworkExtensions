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
using System.Text;

namespace System.IO;

static partial class FileInfoExtensions {
  /// <summary>
  /// Represents a file being modified or processed, with operations to manage changes before finalizing them.
  /// </summary>
  public interface IFileInProgress : IDisposable {

    /// <summary>
    /// Gets the original file information.
    /// </summary>
    FileInfo OriginalFile { get; }

    /// <summary>
    /// Gets or sets a value indicating whether changes to the file should be canceled.
    /// </summary>
    bool CancelChanges { get; set; }

    void CopyFrom(FileInfo source);
    Encoding GetEncoding();
    string ReadAllText();
    string ReadAllText(Encoding encoding);
    IEnumerable<string> ReadLines();
    IEnumerable<string> ReadLines(Encoding encoding);
    void WriteAllText(string text);
    void WriteAllText(string text, Encoding encoding);
    void WriteAllLines(IEnumerable<string> lines);
    void WriteAllLines(IEnumerable<string> lines, Encoding encoding);
    void AppendLine(string line);
    void AppendLine(string line, Encoding encoding);
    void AppendAllLines(IEnumerable<string> lines);
    void AppendAllLines(IEnumerable<string> lines, Encoding encoding);
    void AppendAllText(string text);
    void AppendAllText(string text, Encoding encoding);
    FileStream Open(FileAccess access);
    byte[] ReadAllBytes();
    void WriteAllBytes(byte[] data);
    IEnumerable<byte> ReadBytes();
    void KeepFirstLines(int count);
    void KeepFirstLines(int count, Encoding encoding);
    void KeepLastLines(int count);
    void KeepLastLines(int count, Encoding encoding);
    void RemoveFirstLines(int count, Encoding encoding);
    void RemoveFirstLines(int count);
  }
}
