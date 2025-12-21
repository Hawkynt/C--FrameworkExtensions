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

#if !SUPPORTS_FILE_ASYNC

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

public static partial class FilePolyfills {
  extension(File) {

    #region ReadAllBytesAsync

    /// <summary>
    /// Asynchronously opens a binary file, reads the contents of the file into a byte array, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the byte array containing the contents of the file.</returns>
    public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
      var buffer = new byte[stream.Length];
      await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
      return buffer;
    }

    #endregion

    #region ReadAllTextAsync

    /// <summary>
    /// Asynchronously opens a text file, reads all the text in the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
    public static async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
      => await ReadAllTextAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously opens a text file, reads all the text in the file with the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
    public static async Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
      using var reader = new StreamReader(stream, encoding);
      return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region ReadAllLinesAsync

    /// <summary>
    /// Asynchronously opens a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
    public static async Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
      => await ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously opens a text file, reads all lines of the file with the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to open for reading.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
    public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default) {
      var lines = new List<string>();
      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
      using var reader = new StreamReader(stream, encoding);

      while (!reader.EndOfStream) {
        cancellationToken.ThrowIfCancellationRequested();
        var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        if (line != null)
          lines.Add(line);
      }

      return lines.ToArray();
    }

    #endregion

    #region WriteAllBytesAsync

    /// <summary>
    /// Asynchronously creates a new file, writes the specified byte array to the file, and then closes the file.
    /// If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="bytes">The bytes to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
      await stream.WriteAsync(bytes.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region WriteAllTextAsync

    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file, and then closes the file.
    /// If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
      => await WriteAllTextAsync(path, contents, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file.
    /// If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the string.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
      using var writer = new StreamWriter(stream, encoding);
      cancellationToken.ThrowIfCancellationRequested();
      await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
    }

    #endregion

    #region WriteAllLinesAsync

    /// <summary>
    /// Asynchronously creates a new file, writes the specified lines to the file, and then closes the file.
    /// If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
      => await WriteAllLinesAsync(path, contents, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously creates a new file, writes the specified lines to the file using the specified encoding, and then closes the file.
    /// If the target file already exists, it is overwritten.
    /// </summary>
    /// <param name="path">The file to write to.</param>
    /// <param name="contents">The lines to write to the file.</param>
    /// <param name="encoding">The encoding to apply to the lines.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
      using var writer = new StreamWriter(stream, encoding);

      foreach (var line in contents) {
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(line).ConfigureAwait(false);
      }
    }

    #endregion

    #region AppendAllTextAsync

    /// <summary>
    /// Asynchronously opens a file or creates a file if it does not already exist, appends the specified string to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to append the specified string to.</param>
    /// <param name="contents">The string to append to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    public static async Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
      => await AppendAllTextAsync(path, contents, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously opens a file or creates a file if it does not already exist, appends the specified string to the file using the specified encoding, and then closes the file.
    /// </summary>
    /// <param name="path">The file to append the specified string to.</param>
    /// <param name="contents">The string to append to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    public static async Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: true);
      using var writer = new StreamWriter(stream, encoding);
      cancellationToken.ThrowIfCancellationRequested();
      await writer.WriteAsync(contents ?? string.Empty).ConfigureAwait(false);
    }

    #endregion

    #region AppendAllLinesAsync

    /// <summary>
    /// Asynchronously appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to append the lines to.</param>
    /// <param name="contents">The lines to append to the file.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    public static async Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
      => await AppendAllLinesAsync(path, contents, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously appends lines to a file using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
    /// </summary>
    /// <param name="path">The file to append the lines to.</param>
    /// <param name="contents">The lines to append to the file.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous append operation.</returns>
    public static async Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) {
      using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: true);
      using var writer = new StreamWriter(stream, encoding);

      foreach (var line in contents) {
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(line).ConfigureAwait(false);
      }
    }

    #endregion

  }
}

#endif
