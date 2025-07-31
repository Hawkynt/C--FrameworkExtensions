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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using Guard;
using Encoding = System.Text.Encoding;
#if !NETCOREAPP3_1_OR_GREATER && !NETSTANDARD
using System.Security.AccessControl;
#endif
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
#endif

namespace System.IO;

using LineBreakMode = StringExtensions.LineBreakMode;

public static partial class FileInfoExtensions {

  #region consts

  private static Encoding _utf8NoBom;

  /// <summary>
  ///   Gets a UTF-8 encoding instance without a Byte Order Mark (BOM).
  /// </summary>
  /// <remarks>
  ///   This property provides a thread-safe, lazy-initialized UTF-8 encoding object that does not emit a Byte Order Mark
  ///   (BOM).
  ///   It is useful for text operations requiring UTF-8 encoding format without the presence of a BOM, such as generating
  ///   text files
  ///   that are compliant with systems or specifications that do not recognize or require a BOM.
  /// </remarks>
  /// <value>
  ///   A <see cref="System.Text.UTF8Encoding" /> instance configured to not emit a Byte Order Mark (BOM).
  /// </value>
  internal static Encoding Utf8NoBom {
    get {
      if (_utf8NoBom != null)
        return _utf8NoBom;

      UTF8Encoding utF8Encoding = new(false, true);
      Thread.MemoryBarrier();
      return _utf8NoBom = utF8Encoding;
    }
  }

  #endregion

  #region native file compression

  /// <summary>
  ///   Enables NTFS file compression on the specified file.
  /// </summary>
  /// <param name="this">The file on which to enable compression.</param>
  /// <exception cref="ArgumentNullException">Thrown if the file object is null.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <exception cref="Win32Exception">Thrown if the compression operation fails.</exception>
  /// <example>
  ///   Here is how to use <see cref="EnableCompression" /> to compress a file:
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// fileInfo.EnableCompression();
  /// </code>
  ///   This example compresses "file.txt" using NTFS compression.
  /// </example>
  public static void EnableCompression(this FileInfo @this) {
    Against.ThisIsNull(@this);

    @this.Refresh();
    if (!@this.Exists)
      throw new FileNotFoundException(@this.FullName);

    short defaultCompressionFormat = 1;

    int result;
    using (var f = File.Open(@this.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
      result = NativeMethods.DeviceIoControl(
        // ReSharper disable once PossibleNullReferenceException
        f.SafeFileHandle.DangerousGetHandle(),
        NativeMethods.FileSystemControl.SetCompression,
        ref defaultCompressionFormat,
        sizeof(short),
        IntPtr.Zero,
        0,
        out _,
        IntPtr.Zero
      );

    if (result < 0)
      throw new Win32Exception();
  }

  /// <summary>
  ///   Attempts to enable NTFS file compression on the specified <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The file on which to attempt to enable compression.</param>
  /// <returns><see langword="true" /> if compression was successfully enabled; otherwise, <see langword="false" />.</returns>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo(@"path\to\your\file.txt");
  /// bool isCompressionEnabled = fileInfo.TryEnableCompression();
  /// Console.WriteLine(isCompressionEnabled ? "Compression enabled." : "Compression not enabled.");
  /// </code>
  ///   This example demonstrates checking if NTFS file compression can be enabled for a specified file,
  ///   and prints the outcome.
  /// </example>
  public static bool TryEnableCompression(this FileInfo @this) {
    Against.ThisIsNull(@this);

    @this.Refresh();
    if (!@this.Exists)
      return false;

    short defaultCompressionFormat = 1;

    FileStream f = null;
    try {
      f = File.Open(@this.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      var result = NativeMethods.DeviceIoControl(
        // ReSharper disable once PossibleNullReferenceException
        f.SafeFileHandle.DangerousGetHandle(),
        NativeMethods.FileSystemControl.SetCompression,
        ref defaultCompressionFormat,
        sizeof(short),
        IntPtr.Zero,
        0,
        out _,
        IntPtr.Zero
      );
      return result >= 0;
    } catch (Exception) {
      return false;
    } finally {
      f?.Dispose();
    }
  }

  #endregion

  #region shell information

  /// <summary>
  ///   Retrieves the description of the file type for the specified <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file for which to retrieve the type description.</param>
  /// <returns>A <see cref="string" /> containing the description of the file type, such as "Text Document" for a .txt file.</returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown if the provided <see cref="FileInfo" /> instance is
  ///   <see langword="null" />.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo(@"C:\path\to\file.txt");
  /// string fileTypeDescription = fileInfo.GetTypeDescription();
  /// Console.WriteLine($"The file type is: {fileTypeDescription}");
  /// </code>
  ///   This example retrieves the file type description of "file.txt" and prints it to the console.
  /// </example>
  public static string GetTypeDescription(this FileInfo @this) {
    Against.ThisIsNull(@this);

    NativeMethods.SHFILEINFO shellFileInfo = new();
    NativeMethods.SHGetFileInfo(@this.FullName, 0, ref shellFileInfo, (uint)Marshal.SizeOf(shellFileInfo), NativeMethods.ShellFileInfoFlags.Typename);
    return shellFileInfo.szTypeName.Trim();
  }

  #endregion

  #region file copy/move/rename

#if SUPPORTS_ASYNC

  /// <summary>
  ///   Asynchronously copies the current file to the specified target directory.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetDirectory">The <see cref="DirectoryInfo" /> instance representing the target directory.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetDirectory" /> is <see langword="null" />.</exception>
  /// <exception cref="DirectoryNotFoundException">Thrown if the target directory does not exist.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists in the target directory or an I/O
  ///   error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// DirectoryInfo targetDirectory = new DirectoryInfo("destinationDir");
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(targetDirectory);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a target directory with basic error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target directory
  ///   exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory)
    => CopyToAsync(@this, targetDirectory, false, CancellationToken.None);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target directory, with support for cancellation.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetDirectory">The <see cref="DirectoryInfo" /> instance representing the target directory.</param>
  /// <param name="token">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetDirectory" /> is <see langword="null" />.</exception>
  /// <exception cref="DirectoryNotFoundException">Thrown if the target directory does not exist.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists in the target directory or an I/O
  ///   error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// DirectoryInfo targetDirectory = new DirectoryInfo("destinationDir");
  /// CancellationTokenSource cts = new CancellationTokenSource();
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(targetDirectory, cts.Token);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (OperationCanceledException)
  /// {
  ///     Console.WriteLine("File copy was canceled.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a target directory with support for cancellation and
  ///   error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target directory
  ///   exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, CancellationToken token)
    => CopyToAsync(@this, targetDirectory, false, token);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target directory, with an option to overwrite any existing
  ///   file.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetDirectory">The <see cref="DirectoryInfo" /> instance representing the target directory.</param>
  /// <param name="overwrite">
  ///   <see langword="true" /> to allow overwriting an existing file in the target directory;
  ///   otherwise, <see langword="false" />.
  /// </param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetDirectory" /> is <see langword="null" />.</exception>
  /// <exception cref="DirectoryNotFoundException">Thrown if the target directory does not exist.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists in the target directory and
  ///   <paramref name="overwrite" /> is <see langword="false" /> or an I/O error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// DirectoryInfo targetDirectory = new DirectoryInfo("destinationDir");
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(targetDirectory, overwrite: true);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a target directory, allowing for error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target directory
  ///   exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)
    => CopyToAsync(@this, targetDirectory, overwrite, CancellationToken.None);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target directory, with an option to overwrite any existing
  ///   file and support for cancellation.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetDirectory">The <see cref="DirectoryInfo" /> instance representing the target directory.</param>
  /// <param name="overwrite">
  ///   <see langword="true" /> to allow overwriting an existing file in the target directory;
  ///   otherwise, <see langword="false" />.
  /// </param>
  /// <param name="token">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetDirectory" /> is <see langword="null" />.</exception>
  /// <exception cref="DirectoryNotFoundException">Thrown if the target directory does not exist.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists in the target directory and
  ///   <paramref name="overwrite" /> is <see langword="false" /> or an I/O error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// DirectoryInfo targetDirectory = new DirectoryInfo("destinationDir");
  /// CancellationTokenSource cts = new CancellationTokenSource();
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(targetDirectory, overwrite: true, token: cts.Token);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (OperationCanceledException)
  /// {
  ///     Console.WriteLine("File copy was canceled.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a target directory, allowing for cancellation and
  ///   error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target directory
  ///   exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite, CancellationToken token) {
    Against.ArgumentIsNull(targetDirectory);
    Against.False(targetDirectory.Exists);

    return CopyToAsync(@this, targetDirectory.File(@this.Name), overwrite, token);
  }

  /// <summary>
  ///   Asynchronously copies the current file to the specified target file.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetFile">The <see cref="FileInfo" /> instance representing the target file.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetFile" /> is <see langword="null" />.</exception>
  /// <exception cref="IOException">Thrown if the target file exists or an I/O error occurs during the copy operation.</exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// FileInfo destinationFile = new FileInfo("destination.txt");
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(destinationFile);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a new location with basic error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target file's
  ///   directory exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, FileInfo targetFile)
    => CopyToAsync(@this, targetFile, false, CancellationToken.None);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target file, with support for cancellation.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetFile">The <see cref="FileInfo" /> instance representing the target file.</param>
  /// <param name="token">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetFile" /> is <see langword="null" />.</exception>
  /// <exception cref="IOException">Thrown if the target file exists or an I/O error occurs during the copy operation.</exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// FileInfo destinationFile = new FileInfo("destination.txt");
  /// CancellationTokenSource cts = new CancellationTokenSource();
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(destinationFile, cts.Token);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (OperationCanceledException)
  /// {
  ///     Console.WriteLine("File copy was canceled.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a new location with support for cancellation and error
  ///   handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target file's
  ///   directory exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, FileInfo targetFile, CancellationToken token)
    => CopyToAsync(@this, targetFile, false, token);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target file, with an option to overwrite the target file.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetFile">The <see cref="FileInfo" /> instance representing the target file.</param>
  /// <param name="overwrite">
  ///   <see langword="true" /> to allow overwriting the target file if it exists; otherwise,
  ///   <see langword="false" />.
  /// </param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetFile" /> is <see langword="null" />.</exception>
  /// <exception cref="IOException">
  ///   Thrown if the target file exists and <paramref name="overwrite" /> is
  ///   <see langword="false" /> or an I/O error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// FileInfo destinationFile = new FileInfo("destination.txt");
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(destinationFile, overwrite: true);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a new location, allowing for error handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target file's
  ///   directory exists before calling this method.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task CopyToAsync(this FileInfo @this, FileInfo targetFile, bool overwrite)
    => CopyToAsync(@this, targetFile, overwrite, CancellationToken.None);

  /// <summary>
  ///   Asynchronously copies the current file to the specified target file, with an option to overwrite the target file and
  ///   support for cancellation.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the source file.</param>
  /// <param name="targetFile">The <see cref="FileInfo" /> instance representing the target file.</param>
  /// <param name="overwrite">
  ///   <see langword="true" /> to allow overwriting the target file if it exists; otherwise,
  ///   <see langword="false" />.
  /// </param>
  /// <param name="token">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous copy operation.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="InvalidOperationException">Thrown if <paramref name="this" /> does not exist.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetFile" /> is <see langword="null" />.</exception>
  /// <exception cref="IOException">
  ///   Thrown if the target file exists and <paramref name="overwrite" /> is
  ///   <see langword="false" /> or an I/O error occurs during the copy operation.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo("source.txt");
  /// FileInfo destinationFile = new FileInfo("destination.txt");
  /// CancellationTokenSource cts = new CancellationTokenSource();
  /// 
  /// try
  /// {
  ///     await sourceFile.CopyToAsync(destinationFile, overwrite: true, token: cts.Token);
  ///     Console.WriteLine("File copied successfully.");
  /// }
  /// catch (OperationCanceledException)
  /// {
  ///     Console.WriteLine("File copy was canceled.");
  /// }
  /// catch (IOException ex)
  /// {
  ///     Console.WriteLine($"I/O error occurred: {ex.Message}");
  /// }
  /// </code>
  ///   This example demonstrates how to asynchronously copy a file to a new location, allowing for cancellation and error
  ///   handling.
  /// </example>
  /// <remarks>
  ///   This method uses asynchronous file operations to avoid blocking the calling thread. Ensure that the target file's
  ///   directory exists before calling this method.
  /// </remarks>
  public static async Task CopyToAsync(this FileInfo @this, FileInfo targetFile, bool overwrite, CancellationToken token) {
    Against.ThisIsNull(@this);
    Against.False(@this.Exists);
    Against.ArgumentIsNull(targetFile);

    if (!overwrite && targetFile.Exists)
      throw new IOException("The target file already exists.");

    const int BUFFER_SIZE_IN_BYTES = 65536;

    using var sourceStream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: BUFFER_SIZE_IN_BYTES, useAsync: true);
    using var destinationStream = new FileStream(targetFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: BUFFER_SIZE_IN_BYTES, useAsync: true);
    await sourceStream.CopyToAsync(destinationStream, BUFFER_SIZE_IN_BYTES, token).ConfigureAwait(false);
  }

#endif

  /// <summary>
  ///   Copies the specified <see cref="FileInfo" /> instance to the specified target <see cref="DirectoryInfo" />,
  ///   maintaining the original file name.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to copy.</param>
  /// <param name="targetDirectory">The target <see cref="DirectoryInfo" /> where the file should be copied.</param>
  /// <remarks>
  ///   This method copies the file to the target directory without overwriting existing files with the same name.
  ///   If a file with the same name already exists in the target directory, this method will throw an
  ///   <see cref="IOException" />.
  /// </remarks>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target directory is <see langword="null" />.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists in the target directory.</exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\document.txt");
  /// DirectoryInfo targetDir = new DirectoryInfo(@"C:\target");
  /// sourceFile.CopyTo(targetDir);
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  ///   This example demonstrates copying a file from a source directory to a target directory.
  /// </example>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), false);

  /// <summary>
  ///   Copies the specified <see cref="FileInfo" /> instance to the specified target <see cref="DirectoryInfo" />,
  ///   maintaining the original file name, with an option to overwrite the existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to copy.</param>
  /// <param name="targetDirectory">The target <see cref="DirectoryInfo" /> where the file should be copied.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file with the same name.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists.
  /// </param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target directory is <see langword="null" />.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists in the target directory and
  ///   <paramref name="overwrite" /> is <see langword="false" />.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\document.txt");
  /// DirectoryInfo targetDir = new DirectoryInfo(@"C:\target");
  /// sourceFile.CopyTo(targetDir, true); // Overwrite existing file if it exists
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  ///   This example demonstrates copying a file from a source directory to a target directory with the option to overwrite
  ///   existing files.
  /// </example>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), overwrite);

  /// <summary>
  ///   Copies the specified <see cref="FileInfo" /> instance to the location specified by a target <see cref="FileInfo" />
  ///   object,
  ///   without overwriting an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to copy.</param>
  /// <param name="targetFile">
  ///   The target <see cref="FileInfo" /> object that specifies the destination path and name of the
  ///   file.
  /// </param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target file is <see langword="null" />.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists at the target location.</exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo targetFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.CopyTo(targetFile);
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  ///   This example demonstrates copying a file from one location to another without overwriting an existing file at the
  ///   destination.
  /// </example>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile)
    => @this.CopyTo(targetFile.FullName, false);

  /// <summary>
  ///   Copies the specified <see cref="FileInfo" /> instance to the location specified by a target <see cref="FileInfo" />
  ///   object,
  ///   with an option to overwrite an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to copy.</param>
  /// <param name="targetFile">
  ///   The target <see cref="FileInfo" /> object that specifies the destination path and name of the
  ///   file.
  /// </param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file with the same name at the target location.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists.
  /// </param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target file is <see langword="null" />.</exception>
  /// <exception cref="IOException">
  ///   Thrown if a file with the same name already exists at the target location and
  ///   <paramref name="overwrite" /> is <see langword="false" />.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo targetFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.CopyTo(targetFile, true); // Overwrite existing file if it exists
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  ///   This example demonstrates copying a file from one location to another with the option to overwrite an existing file
  ///   at the destination.
  /// </example>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite)
    => @this.CopyTo(targetFile.FullName, overwrite);

  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location represented by a <see cref="FileInfo" />
  ///   object, without overwriting an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo" /> object that represents the destination file.</param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.MoveTo(destFile);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo" /> objects,
  ///   without the risk of overwriting an existing file at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile) => @this.MoveTo(destFile.FullName, false);

  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location represented by a <see cref="FileInfo" />
  ///   object, without overwriting an existing file,
  ///   and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo" /> object that represents the destination file.</param>
  /// <param name="timeout">
  ///   The maximum <see cref="TimeSpan" /> to retry the deletion of the source file if it is locked or
  ///   cannot be deleted immediately.
  /// </param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destFile, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo" /> objects,
  ///   without overwriting an existing file
  ///   at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, TimeSpan timeout) => @this.MoveTo(destFile.FullName, false, timeout);

  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location represented by a <see cref="FileInfo" />
  ///   object, with an option to overwrite an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo" /> object that represents the destination file.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file at the destination.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists at the destination.
  /// </param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.MoveTo(destFile, true);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo" /> objects,
  ///   with the option to overwrite an existing file
  ///   at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite) => @this.MoveTo(destFile.FullName, overwrite);

  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location represented by a <see cref="FileInfo" />
  ///   object, with an option to overwrite an existing file,
  ///   and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo" /> object that represents the destination file.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file at the destination.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists at the destination.
  /// </param>
  /// <param name="timeout">
  ///   The maximum <see cref="TimeSpan" /> to retry the deletion of the source file if it is locked or
  ///   cannot be deleted immediately.
  /// </param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destFile, true, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo" /> objects,
  ///   with the option to overwrite an existing file
  ///   at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite, TimeSpan timeout) => @this.MoveTo(destFile.FullName, overwrite, timeout);

  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location with an option to overwrite an existing file,
  ///   and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFileName">The path to the destination file. This cannot be a directory.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file at the destination.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists at the destination.
  /// </param>
  /// <param name="timeout">
  ///   The maximum <see cref="TimeSpan" /> to retry the deletion of the source file if it is locked or
  ///   cannot be deleted immediately.
  /// </param>
  /// <exception cref="ArgumentNullException">Thrown if the source file is <see langword="null" />.</exception>
  /// <exception cref="IOException">
  ///   Thrown if the file cannot be moved, typically because the source or destination cannot be accessed,
  ///   or the deletion of the source file exceeds the specified timeout.
  /// </exception>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// string destinationPath = @"D:\destination\example.txt";
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destinationPath, true, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, with the option to overwrite an existing file
  ///   at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite, TimeSpan timeout) {
    Against.ThisIsNull(@this);

    // copy file 
    using TransactionScope scope = new();
    @this.CopyTo(destFileName, overwrite);

    // delete source, retry during timeout
    var delay = TimeSpan.FromSeconds(1);
    var tries = (int)(timeout.Ticks / delay.Ticks);
    while (true)
      try {
        @this.Delete();
        break;
      } catch (IOException) {
        if (tries-- < 1)
          throw;

        Thread.Sleep(delay);
      }

    scope.Complete();
  }

  /// <summary>
  /// Renames the file represented by this <see cref="FileInfo"/> instance to the specified new name.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> instance representing the file to be renamed.</param>
  /// <param name="newName">The new name for the file (without directory path).</param>
  /// <exception cref="System.NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="newName"/> is <see langword="null"/> or empty.
  /// </exception>
  /// <exception cref="System.InvalidOperationException">
  /// Thrown if <paramref name="newName"/> contains directory separators.
  /// </exception>
  /// <exception cref="System.IO.IOException">Thrown if the file cannot be moved or renamed due to an I/O error.</exception>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// file.RenameTo("newname.txt");
  /// // The file "example.txt" is now renamed to "newname.txt".
  /// </code>
  /// </example>
  public static void RenameTo(this FileInfo @this, string newName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(newName);
    Against.True(newName.Contains(Path.DirectorySeparatorChar) || newName.Contains(Path.AltDirectorySeparatorChar));

    var destFileName = Path.Combine(@this.Directory.FullName,newName);
    @this.MoveTo(destFileName);
  }

  /// <summary>
  /// Changes the extension of the file represented by this <see cref="FileInfo"/> instance to the specified new extension.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> instance representing the file to change the extension for.</param>
  /// <param name="newExtension">The new file extension. If it doesn't start with a dot ('.'), one will be added automatically.</param>
  /// <exception cref="System.NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="newExtension"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.InvalidOperationException">Thrown if <paramref name="newExtension"/> contains directory separator characters.</exception>
  /// <exception cref="System.IO.IOException">Thrown if the file cannot be moved or renamed due to an I/O error.</exception>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// file.ChangeExtension("md");
  /// // The file "example.txt" is now renamed to "example.md".
  /// </code>
  /// </example>
  public static void ChangeExtension(this FileInfo @this, string newExtension) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(newExtension);
    Against.True(newExtension.Contains(Path.DirectorySeparatorChar) || newExtension.Contains(Path.AltDirectorySeparatorChar));

    var destFileName = Path.GetFileNameWithoutExtension(@this.FullName) + (newExtension.StartsWith(".") ? newExtension : "." + newExtension);
    @this.MoveTo(destFileName);
  }
  
  #endregion

  #region hash computation

  /// <summary>
  ///   Computes and returns the hash of the file represented by the <see cref="FileInfo" /> instance using the specified
  ///   hash algorithm.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm to use, derived from <see cref="HashAlgorithm" />.</typeparam>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the hash for.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeHash&lt;SHA256Managed&gt;();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this) where THashAlgorithm : HashAlgorithm, new() {
    Against.ThisIsNull(@this);

    using THashAlgorithm provider = new();
    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }

  /// <summary>
  ///   Computes the hash of the file represented by the <see cref="FileInfo" /> instance using the specified hash algorithm
  ///   and block size.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm to use, derived from <see cref="HashAlgorithm" />.</typeparam>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the hash for.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize" /> is less than or equal to zero.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeHash&lt;SHA256Managed&gt;(1024);
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-256 hash of "file.txt" using a block size of 1024 bytes and prints the hash in
  ///   hexadecimal format.
  /// </example>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this, int blockSize) where THashAlgorithm : HashAlgorithm, new() {
    Against.ThisIsNull(@this);

    using THashAlgorithm provider = new();
    return ComputeHash(@this, provider, blockSize);
  }

  /// <summary>
  ///   Computes the hash of the file represented by the <see cref="FileInfo" /> instance using the specified hash algorithm.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the hash for.</param>
  /// <param name="provider">The hash algorithm provider used to compute the file hash.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown if the file object or the hash algorithm provider is
  ///   <see langword="null" />.
  /// </exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// using (var sha256 = SHA256.Create())
  /// {
  ///     byte[] hash = fileInfo.ComputeHash(sha256);
  ///     Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// }
  /// </code>
  ///   This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(provider);

    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }

  /// <summary>
  ///   Computes the hash of the file represented by the <see cref="FileInfo" /> instance using the specified hash algorithm
  ///   and block size.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the hash for.</param>
  /// <param name="provider">The hash algorithm provider used to compute the file hash.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown if the file object or the hash algorithm provider is
  ///   <see langword="null" />.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize" /> is less than or equal to zero.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// using (var sha256 = SHA256.Create())
  /// {
  ///     byte[] hash = fileInfo.ComputeHash(sha256, 1024);
  ///     Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// }
  /// </code>
  ///   This example computes the SHA-256 hash of "file.txt" using a block size of 1024 bytes and prints the hash in
  ///   hexadecimal format.
  /// </example>
  public static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider, int blockSize) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(provider);

    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, blockSize);

    provider.Initialize();
    var lastBlock = new byte[blockSize];
    var currentBlock = new byte[blockSize];

    var lastBlockSize = stream.Read(lastBlock, 0, blockSize);
    for (;;) {
      var currentBlockSize = stream.Read(currentBlock, 0, blockSize);
      if (currentBlockSize < 1)
        break;

      provider.TransformBlock(lastBlock, 0, lastBlockSize, null, 0);
      (currentBlock, lastBlock) = (lastBlock, currentBlock);
      lastBlockSize = currentBlockSize;
    }

    provider.TransformFinalBlock(lastBlock, 0, lastBlockSize < 1 ? blockSize : lastBlockSize);
    return provider.Hash;
  }

  /// <summary>
  ///   Computes the SHA-512 hash of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the SHA-512 hash for.</param>
  /// <returns>A byte array containing the SHA-512 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA512Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-512 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA512Hash(this FileInfo @this) {
    using var provider = SHA512.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  ///   Computes the SHA-512 hash of the file represented by the <see cref="FileInfo" /> instance using a specified block
  ///   size.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the SHA-512 hash for.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the SHA-512 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize" /> is less than or equal to zero.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA512Hash(1024);
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-512 hash of "file.txt" using a block size of 1024 bytes and prints the hash in
  ///   hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA512Hash(this FileInfo @this, int blockSize) {
    using var provider = SHA512.Create();
    return @this.ComputeHash(provider, blockSize);
  }

  /// <summary>
  ///   Computes the SHA-384 hash of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the SHA-384 hash for.</param>
  /// <returns>A byte array containing the SHA-384 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA384Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-384 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA384Hash(this FileInfo @this) {
    using var provider = SHA384.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  ///   Computes the SHA-256 hash of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the SHA-256 hash for.</param>
  /// <returns>A byte array containing the SHA-256 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA256Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA256Hash(this FileInfo @this) {
    using var provider = SHA256.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  ///   Computes the SHA-1 hash of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the SHA-1 hash for.</param>
  /// <returns>A byte array containing the SHA-1 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA1Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  ///   This example computes the SHA-1 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA1Hash(this FileInfo @this) {
    using var provider = SHA1.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  ///   Computes the MD5 hash of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file to compute the MD5 hash for.</param>
  /// <returns>A byte array containing the MD5 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null" />.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeMD5Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  ///   This example computes the MD5 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeMD5Hash(this FileInfo @this) {
    using var provider = MD5.Create();
    return @this.ComputeHash(provider);
  }

  #endregion

  #region reading

  /// <summary>
  ///   Determines the encoding used in the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>The detected <see cref="Encoding" /> of the file or <see lanword="null" /> when the file is empty.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// Encoding encoding = file.GetEncoding();
  /// Console.WriteLine($"File encoding: {encoding.EncodingName}");
  /// </code>
  ///   This example determines and prints the encoding of "example.txt".
  /// </example>
  public static Encoding GetEncoding(this FileInfo @this) {
    // Read the BOM
    var bom = new byte[4];
    using (FileStream file = new(@this.FullName, FileMode.Open, FileAccess.Read)) {
      var bytesRead = file.Read(bom, 0, 4);
      if (bytesRead == 0)
        return null;
    }

    // Analyze the BOM
#if DEPRECATED_UTF7
    if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.Default;
#else
    if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
      return Encoding.UTF7;
#endif
    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
      return Encoding.UTF8;
    if (bom[0] == 0xff && bom[1] == 0xfe)
      return Encoding.Unicode; //UTF-16LE
    if (bom[0] == 0xfe && bom[1] == 0xff)
      return Encoding.BigEndianUnicode; //UTF-16BE
    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
      return Encoding.UTF32;
    return Encoding.Default;
  }

  /// <summary>
  ///   Reads all text from the file represented by the <see cref="FileInfo" /> instance using the detected encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>A <see cref="string" /> containing all the text from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// string content = file.ReadAllText();
  /// Console.WriteLine(content);
  /// </code>
  ///   This example reads and prints the content of "example.txt" using the detected encoding.
  /// </example>
  public static string ReadAllText(this FileInfo @this) => File.ReadAllText(@this.FullName);

  /// <summary>
  ///   Reads all text from the file represented by the <see cref="FileInfo" /> instance using the specified
  ///   <see cref="Encoding" />.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding" /> to use when reading the file.</param>
  /// <returns>A <see cref="string" /> containing all the text from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// string content = file.ReadAllText(Encoding.UTF8);
  /// Console.WriteLine(content);
  /// </code>
  ///   This example reads and prints the content of "example.txt" using UTF-8 encoding.
  /// </example>
  public static string ReadAllText(this FileInfo @this, Encoding encoding) => File.ReadAllText(@this.FullName, encoding);

  /// <summary>
  ///   Reads all lines from the file represented by the <see cref="FileInfo" /> instance using the detected encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>An array of <see cref="string" /> containing all the lines from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// foreach (var line in file.ReadAllLines())
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example reads and prints each line of "example.txt" using the detected encoding.
  /// </example>
  public static string[] ReadAllLines(this FileInfo @this) => File.ReadAllLines(@this.FullName);

  /// <summary>
  ///   Tries to read all lines from the file represented by the <see cref="FileInfo" /> instance using the detected
  ///   encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="result">
  ///   When this method returns, contains an array of <see cref="string" /> containing all the lines from
  ///   the file if the read was successful, or <see langword="null" /> if it fails.
  /// </param>
  /// <returns><see langword="true" /> if the lines were successfully read; otherwise, <see langword="false" />.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// if (file.TryReadAllLines(out string[] lines))
  /// {
  ///     foreach (var line in lines)
  ///     {
  ///         Console.WriteLine(line);
  ///     }
  /// }
  /// else
  /// {
  ///     Console.WriteLine("Failed to read lines.");
  /// }
  /// </code>
  ///   This example attempts to read and print each line of "example.txt". If unsuccessful, it prints a failure message.
  /// </example>
  public static bool TryReadAllLines(this FileInfo @this, out string[] result) {
    try {
      result = File.ReadAllLines(@this.FullName);
      return true;
    } catch (Exception) {
      result = null;
      return true;
    }
  }

  /// <summary>
  ///   Reads all lines from the file represented by the <see cref="FileInfo" /> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding" /> to use when reading the file.</param>
  /// <returns>An array of <see cref="string" /> containing all the lines from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// foreach (var line in file.ReadAllLines(Encoding.UTF8))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example reads and prints each line of "example.txt" using UTF-8 encoding.
  /// </example>
  public static string[] ReadAllLines(this FileInfo @this, Encoding encoding) => File.ReadAllLines(@this.FullName, encoding);

  /// <summary>
  ///   Attempts to read all lines from the file represented by the <see cref="FileInfo" /> instance using the specified
  ///   <see cref="Encoding" />.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding" /> to use for reading the file.</param>
  /// <param name="result">
  ///   When this method returns, contains an array of <see cref="string" /> containing all the lines from
  ///   the file if the read was successful, or <see langword="null" /> if it fails.
  /// </param>
  /// <returns><see langword="true" /> if the lines were successfully read; otherwise, <see langword="false" />.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// if (file.TryReadAllLines(Encoding.UTF8, out string[] lines))
  /// {
  ///     foreach (var line in lines)
  ///     {
  ///         Console.WriteLine(line);
  ///     }
  /// }
  /// else
  /// {
  ///     Console.WriteLine("Failed to read lines.");
  /// }
  /// </code>
  ///   This example attempts to read and print each line of "example.txt" using UTF-8 encoding. If unsuccessful, it prints a
  ///   failure message.
  /// </example>
  public static bool TryReadAllLines(this FileInfo @this, Encoding encoding, out string[] result) {
    try {
      result = File.ReadAllLines(@this.FullName, encoding);
      return true;
    } catch (Exception) {
      result = null;
      return true;
    }
  }

  /// <summary>
  ///   Reads bytes from the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>An <see cref="IEnumerable{Byte}" /> containing all bytes from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.bin");
  /// IEnumerable&lt;byte&gt; bytes = file.ReadBytes();
  /// foreach (byte b in bytes)
  /// {
  ///     Console.WriteLine(b);
  /// }
  /// </code>
  ///   This example reads and prints each byte of "example.bin".
  /// </example>
  public static IEnumerable<byte> ReadBytes(this FileInfo @this) {
    using var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    for (;;) {
      var result = stream.ReadByte();
      if (result < 0)
        yield break;

      yield return (byte)result;
    }
  }

  /// <summary>
  ///   Reads all bytes from the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>A byte array containing all bytes from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.bin");
  /// byte[] bytes = file.ReadAllBytes();
  /// Console.WriteLine(BitConverter.ToString(bytes));
  /// </code>
  ///   This example reads all bytes from "example.bin" and prints them as a hexadecimal string.
  /// </example>
  public static byte[] ReadAllBytes(this FileInfo @this) => File.ReadAllBytes(@this.FullName);

  /// <summary>
  ///   Attempts to read all bytes from the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="result">
  ///   When this method returns, contains the byte array of all bytes from the file if the read was
  ///   successful, or <see langword="null" /> if it fails.
  /// </param>
  /// <returns><see langword="true" /> if the bytes were successfully read; otherwise, <see langword="false" />.</returns>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.bin");
  /// if (file.TryReadAllBytes(out byte[] bytes))
  /// {
  ///     Console.WriteLine(BitConverter.ToString(bytes));
  /// }
  /// else
  /// {
  ///     Console.WriteLine("Failed to read bytes.");
  /// }
  /// </code>
  ///   This example attempts to read all bytes from "example.bin" and prints them as a hexadecimal string. If unsuccessful,
  ///   it prints a failure message.
  /// </example>
  public static bool TryReadAllBytes(this FileInfo @this, out byte[] result) {
    try {
      result = File.ReadAllBytes(@this.FullName);
      return true;
    } catch (Exception) {
      result = null;
      return true;
    }
  }

  /// <summary>
  ///   Reads lines from the file represented by the <see cref="FileInfo" /> instance using the default encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <returns>An <see cref="IEnumerable{String}" /> of lines read from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// foreach (var line in fileInfo.ReadLines())
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example demonstrates how to enumerate through each line of "example.txt".
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this) => File.ReadLines(@this.FullName);
#else
  public static IEnumerable<string> ReadLines(this FileInfo @this) => ReadLines(@this, FileShare.Read);
#endif

  /// <summary>
  ///   Reads lines from the file represented by the <see cref="FileInfo" /> instance using the default encoding and
  ///   specified <see cref="FileShare" /> mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="share">The <see cref="FileShare" /> mode to use when opening the file.</param>
  /// <returns>An <see cref="IEnumerable{String}" /> of lines read from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("shared_example.txt");
  /// foreach (var line in fileInfo.ReadLines(FileShare.Read))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example reads lines from "shared_example.txt", allowing other processes to read the file simultaneously.
  /// </example>
  public static IEnumerable<string> ReadLines(this FileInfo @this, FileShare share) {
    const int bufferSize = 4096;
    using var stream = (Stream)new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, share, bufferSize, FileOptions.SequentialScan);
    using StreamReader reader = new(stream);
    while (!reader.EndOfStream) {
      var line = reader.ReadLine();
      if (line == null)
        yield break;

      yield return line;
    }
  }

  /// <summary>
  ///   Reads lines from the file represented by the <see cref="FileInfo" /> instance using the specified encoding and
  ///   <see cref="FileShare" /> mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding" /> to use for reading the file.</param>
  /// <param name="share">The <see cref="FileShare" /> mode to use when opening the file.</param>
  /// <returns>An <see cref="IEnumerable{String}" /> of lines read from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("shared_example.txt");
  /// foreach (var line in fileInfo.ReadLines(Encoding.UTF8, FileShare.Read))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example reads lines from "shared_example.txt" using UTF-8 encoding, allowing other processes to read the file
  ///   simultaneously.
  /// </example>
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding, FileShare share) {
    const int bufferSize = 4096;
    using var stream = (Stream)new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, share, bufferSize, FileOptions.SequentialScan);
    using StreamReader reader = new(stream, encoding);
    while (!reader.EndOfStream) {
      var line = reader.ReadLine();
      if (line == null)
        yield break;

      yield return line;
    }
  }

  /// <summary>
  ///   Reads lines from the file represented by the <see cref="FileInfo" /> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding" /> to use for reading the file.</param>
  /// <returns>An <see cref="IEnumerable{String}" /> of lines read from the file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// foreach (var line in fileInfo.ReadLines(Encoding.UTF8))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  ///   This example reads lines from "example.txt" using UTF-8 encoding.
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => File.ReadLines(@this.FullName, encoding);
#else
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => ReadLines(@this, encoding,FileShare.Read);
#endif

  #endregion

  #region writing

  /// <summary>
  ///   Writes a string to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The string to write to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.WriteAllText("Hello World");
  /// Console.WriteLine("Text written successfully.");
  /// </code>
  ///   This example writes "Hello World" to "example.txt", overwriting any existing content.
  /// </example>
  public static void WriteAllText(this FileInfo @this, string contents) => File.WriteAllText(@this.FullName, contents);

  /// <summary>
  ///   Writes a string to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The string to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the string.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.WriteAllText("Hello World", Encoding.UTF8);
  /// Console.WriteLine("Text written successfully with UTF-8 encoding.");
  /// </code>
  ///   This example writes "Hello World" to "example.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
  public static void WriteAllText(this FileInfo @this, string contents, Encoding encoding) => File.WriteAllText(@this.FullName, contents, encoding);

  /// <summary>
  ///   Writes an array of strings to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The string array to write to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// string[] lines = { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines);
  /// Console.WriteLine("Lines written successfully.");
  /// </code>
  ///   This example writes two lines to "lines.txt", overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, string[] contents) => File.WriteAllLines(@this.FullName, contents);

  /// <summary>
  ///   Writes an array of strings to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The string array to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the strings.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// string[] lines = { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines written successfully with UTF-8 encoding.");
  /// </code>
  ///   This example writes two lines to "lines.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, string[] contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents, encoding);

  /// <summary>
  ///   Writes a byte array to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="bytes">The byte array to write to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.bin");
  /// byte[] data = { 0x00, 0x0F, 0xF0 };
  /// fileInfo.WriteAllBytes(data);
  /// Console.WriteLine("Bytes written successfully.");
  /// </code>
  ///   This example writes a byte array to "example.bin", overwriting any existing content.
  /// </example>
  public static void WriteAllBytes(this FileInfo @this, byte[] bytes) => File.WriteAllBytes(@this.FullName, bytes);

  /// <summary>
  ///   Writes a sequence of strings to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The sequence of strings to write to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines);
  /// Console.WriteLine("Lines written successfully.");
  /// </code>
  ///   This example writes two lines to "lines.txt", overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents) {
    using var writer = new StreamWriter(@this.FullName);
    foreach (var line in contents)
      writer.WriteLine(line);
  }

  /// <summary>
  ///   Writes a sequence of strings to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The sequence of strings to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the strings.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines written successfully with UTF-8 encoding.");
  /// </code>
  ///   This example writes two lines to "lines.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) {
    using var writer = new StreamWriter(@this.FullName, false, encoding);
    foreach (var line in contents)
      writer.WriteLine(line);
  }

  #endregion

  #region appending

  /// <summary>
  ///   Appends text to the end of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The text to append to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendAllText("Appended text");
  /// Console.WriteLine("Text appended successfully.");
  /// </code>
  ///   This example appends "Appended text" to the end of "example.txt".
  /// </example>
  public static void AppendAllText(this FileInfo @this, string contents) => File.AppendAllText(@this.FullName, contents);

  /// <summary>
  ///   Appends text to the end of the file represented by the <see cref="FileInfo" /> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The text to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended text.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendAllText("Appended text", Encoding.UTF8);
  /// Console.WriteLine("Text appended successfully with UTF-8 encoding.");
  /// </code>
  ///   This example appends "Appended text" to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendAllText(this FileInfo @this, string contents, Encoding encoding) => File.AppendAllText(@this.FullName, contents, encoding);

  /// <summary>
  ///   Appends lines to the end of the file represented by the <see cref="FileInfo" /> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The lines to append to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First appended line", "Second appended line" };
  /// fileInfo.AppendAllLines(lines);
  /// Console.WriteLine("Lines appended successfully.");
  /// </code>
  ///   This example appends two lines to the end of "example.txt".
  /// </example>
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents) {
    using var writer = new StreamWriter(@this.FullName, append: true);
    foreach (var line in contents)
      writer.WriteLine(line);
  }

  /// <summary>
  ///   Appends lines to the end of the file represented by the <see cref="FileInfo" /> instance using the specified
  ///   encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The lines to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended lines.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First appended line", "Second appended line" };
  /// fileInfo.AppendAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines appended successfully with UTF-8 encoding.");
  /// </code>
  ///   This example appends two lines to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) {
    using var writer = new StreamWriter(@this.FullName, true, encoding);
    foreach (var line in contents)
      writer.WriteLine(line);
  }

  /// <summary>
  ///   Appends a line to the end of the file represented by the <see cref="FileInfo" /> instance, followed by a line
  ///   terminator.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The line to append to the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendLine("Appended line");
  /// Console.WriteLine("Line appended successfully.");
  /// </code>
  ///   This example appends "Appended line" followed by a line terminator to the end of "example.txt".
  /// </example>
  public static void AppendLine(this FileInfo @this, string contents) {
    using var writer = new StreamWriter(@this.FullName, append: true);
    writer.WriteLine(contents);
  }

  /// <summary>
  ///   Appends a line to the end of the file represented by the <see cref="FileInfo" /> instance using the specified
  ///   encoding, followed by a line terminator.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="contents">The line to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended line.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendLine("Appended line", Encoding.UTF8);
  /// Console.WriteLine("Line appended successfully with UTF-8 encoding.");
  /// </code>
  ///   This example appends "Appended line" followed by a line terminator to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendLine(this FileInfo @this, string contents, Encoding encoding) {
    using var writer = new StreamWriter(@this.FullName, true, encoding);
    writer.WriteLine(contents);
  }

  #endregion

  #region async

#if SUPPORTS_FILE_ASYNC

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<byte[]> ReadAllBytesAsync(this FileInfo @this) => File.ReadAllBytesAsync(@this.FullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<byte[]> ReadAllBytesAsync(this FileInfo @this, CancellationToken token) => File.ReadAllBytesAsync(@this.FullName, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this) => File.ReadAllLinesAsync(@this.FullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this, CancellationToken token) => File.ReadAllLinesAsync(@this.FullName, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding) => File.ReadAllLinesAsync(@this.FullName, encoding);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding, CancellationToken token) => File.ReadAllLinesAsync(@this.FullName, encoding, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this) => File.ReadAllTextAsync(@this.FullName);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this, CancellationToken token) => File.ReadAllTextAsync(@this.FullName, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding) => File.ReadAllTextAsync(@this.FullName, encoding);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding, CancellationToken token) => File.ReadAllTextAsync(@this.FullName, encoding, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllBytesAsync(this FileInfo @this, byte[] data) => File.WriteAllBytesAsync(@this.FullName, data);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllBytesAsync(this FileInfo @this, byte[] data, CancellationToken token) => File.WriteAllBytesAsync(@this.FullName, data, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data) => File.WriteAllLinesAsync(@this.FullName, data);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, CancellationToken token) => File.WriteAllLinesAsync(@this.FullName, data, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding) => File.WriteAllLinesAsync(@this.FullName, data, encoding);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding, CancellationToken token) => File.WriteAllLinesAsync(@this.FullName, data, encoding, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data) => File.WriteAllTextAsync(@this.FullName, data);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data, CancellationToken token) => File.WriteAllTextAsync(@this.FullName, data, token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding) => File.WriteAllTextAsync(@this.FullName, data, encoding);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding, CancellationToken token) => File.WriteAllTextAsync(@this.FullName, data, encoding, token);

#elif SUPPORTS_STREAM_ASYNC

  public static async Task<byte[]> ReadAllBytesAsync(this FileInfo @this) {
    using var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    var buffer = new byte[stream.Length];
    await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
    return buffer;
  }

  public static async Task<byte[]> ReadAllBytesAsync(this FileInfo @this, CancellationToken token) {
    using var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    var buffer = new byte[stream.Length];
    await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
    return buffer;
  }

  public static async Task<string[]> ReadAllLinesAsync(this FileInfo @this) {
    var lines = new List<string>();
    using (var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
    using (var reader = new StreamReader(stream))
      while (!reader.EndOfStream)
        lines.Add(await reader.ReadLineAsync().ConfigureAwait(false));

    return lines.ToArray();
  }

  public static async Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding) {
    var lines = new List<string>();
    using (var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
    using (var reader = new StreamReader(stream, encoding))
      while (!reader.EndOfStream)
        lines.Add(await reader.ReadLineAsync().ConfigureAwait(false));

    return lines.ToArray();
  }

  public static async Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding, CancellationToken token) {
    var lines = new List<string>();
    using (var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
    using (var reader = new StreamReader(stream, encoding))
      while (!reader.EndOfStream) {
        token.ThrowIfCancellationRequested();
        lines.Add(await reader.ReadLineAsync().ConfigureAwait(false));
      }

    return lines.ToArray();
  }

  public static async Task<string> ReadAllTextAsync(this FileInfo @this) {
    using var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync().ConfigureAwait(false);
  }

  public static async Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding) {
    using var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    using var reader = new StreamReader(stream, encoding);
    return await reader.ReadToEndAsync().ConfigureAwait(false);
  }

  public static async Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding, CancellationToken token) {
    using var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    using var reader = new StreamReader(stream, encoding);
    return await reader.ReadToEndAsync().ConfigureAwait(false);
  }

  public static async Task WriteAllBytesAsync(this FileInfo @this, byte[] data) {
    using var stream = new FileStream(@this.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
    await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
  }

  public static async Task WriteAllBytesAsync(this FileInfo @this, byte[] data, CancellationToken token) {
    using var stream = new FileStream(@this.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
    await stream.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
  }

  public static async Task WriteAllTextAsync(this FileInfo @this, string data) {
    using var stream = new FileStream(@this.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
    using var writer = new StreamWriter(stream);
    await writer.WriteAsync(data).ConfigureAwait(false);
  }

  public static async Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding) {
    using var stream = new FileStream(@this.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
    using var writer = new StreamWriter(stream, encoding);
    await writer.WriteAsync(data).ConfigureAwait(false);
  }

  public static async Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding, CancellationToken token) {
    using var stream = new FileStream(@this.FullName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
    using var writer = new StreamWriter(stream, encoding);
    await writer.WriteAsync(data).ConfigureAwait(false);
  }

#elif SUPPORTS_ASYNC

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<byte[]> ReadAllBytesAsync(this FileInfo @this, CancellationToken token = default) => Task.Factory.StartNew(() => File.ReadAllBytes(@this.FullName), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this, CancellationToken token = default) => Task.Factory.StartNew(() => File.ReadAllLines(@this.FullName), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding, CancellationToken token = default) => Task.Factory.StartNew(() => File.ReadAllLines(@this.FullName, encoding), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this, CancellationToken token = default) => Task.Factory.StartNew(() => File.ReadAllText(@this.FullName), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding, CancellationToken token = default) => Task.Factory.StartNew(() => File.ReadAllText(@this.FullName, encoding), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllBytesAsync(this FileInfo @this, byte[] data, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllBytes(@this.FullName, data), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllLines(@this.FullName, data), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllLines(@this.FullName, data, encoding), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllText(@this.FullName, data), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllText(@this.FullName, data, encoding), token);

#endif

  #endregion

  #region trimming text files

  /// <summary>
  ///   Keeps only the specified number of first lines in the file, discarding the rest.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the beginning of the file to keep.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepFirstLines(10);
  /// Console.WriteLine("First 10 lines kept, others removed.");
  /// </code>
  ///   This example keeps the first 10 lines of "example.txt" and removes the rest.
  /// </example>
  public static void KeepFirstLines(this FileInfo @this, int count) => _KeepFirstLines(@this, count, null, LineBreakMode.AutoDetect);

  /// <summary>
  ///   Keeps only the specified number of first lines in the file, discarding the rest, using the provided encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the beginning of the file to keep.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepFirstLines(10, Encoding.UTF8);
  /// Console.WriteLine("First 10 lines kept using UTF-8 encoding, others removed.");
  /// </code>
  ///   This example keeps the first 10 lines of "example.txt" using UTF-8 encoding and removes the rest.
  /// </example>
  public static void KeepFirstLines(this FileInfo @this, int count, Encoding encoding) {
    Against.ArgumentIsNull(encoding);

    _KeepFirstLines(@this, count, encoding, LineBreakMode.AutoDetect);
  }

  /// <summary>
  ///   Keeps only the specified number of first lines in the file, discarding the rest, based on the specified line break
  ///   mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the beginning of the file to keep.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepFirstLines(10, LineBreakMode.CrLf);
  /// Console.WriteLine("First 10 lines kept using CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the first 10 lines of "example.txt" based on CrLf line breaks and removes the rest.
  /// </example>
  public static void KeepFirstLines(this FileInfo @this, int count, LineBreakMode newLine) => _KeepFirstLines(@this, count, null, newLine);

  /// <summary>
  ///   Keeps only the specified number of first lines in the file, discarding the rest, using the provided encoding and line
  ///   break mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the beginning of the file to keep.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepFirstLines(10, Encoding.UTF8, LineBreakMode.CrLf);
  /// Console.WriteLine("First 10 lines kept using UTF-8 encoding and CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the first 10 lines of "example.txt" using UTF-8 encoding and CrLf line breaks, removing the rest.
  /// </example>
  public static void KeepFirstLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ArgumentIsNull(encoding);

    _KeepFirstLines(@this, count, encoding, newLine);
  }

  private static void _KeepFirstLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(newLine);

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    CustomTextReader.Initialized reader =
        encoding == null
          ? new(stream, true, newLine)
          : new(stream, encoding, newLine)
      ;

    var lineCounter = 0;
    do
      if (reader.ReadLine() == null)
        break;
    while (++lineCounter < count);

    if (lineCounter < count)
      return;

    stream.SetLength(reader.Position);
  }

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5);
  /// Console.WriteLine("Last 5 lines kept, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines of "example.txt" and removes all other preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count) => _KeepLastLines(@this, count, null, LineBreakMode.AutoDetect, 0);

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="offsetInLines">The number of lines to keep at the start of the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, 1);
  /// Console.WriteLine("Last 5 lines and the first kept, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines and the first of "example.txt" and removes all other preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, int offsetInLines) => _KeepLastLines(@this, count, null, LineBreakMode.AutoDetect, offsetInLines);

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, using the provided encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, Encoding.UTF8);
  /// Console.WriteLine("Last 5 lines kept using UTF-8 encoding, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines of "example.txt" using UTF-8 encoding and removes all other preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, Encoding encoding) {
    Against.ArgumentIsNull(encoding);

    _KeepLastLines(@this, count, encoding, LineBreakMode.AutoDetect, 0);
  }

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, using the provided encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="offsetInLines">The number of lines to keep at the start of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, 1, Encoding.UTF8);
  /// Console.WriteLine("Last 5 lines and the first kept using UTF-8 encoding, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines  and the first of "example.txt" using UTF-8 encoding and removes all other
  ///   preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, Encoding encoding) {
    Against.ArgumentIsNull(encoding);

    _KeepLastLines(@this, count, encoding, LineBreakMode.AutoDetect, offsetInLines);
  }

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, based on the specified line break
  ///   mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, LineBreakMode.CrLf);
  /// Console.WriteLine("Last 5 lines kept using CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines of "example.txt" based on CrLf line breaks and removes all other preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, LineBreakMode newLine) => _KeepLastLines(@this, count, null, newLine, 0);

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, based on the specified line break
  ///   mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="offsetInLines">The number of lines to keep at the start of the file.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, 1, LineBreakMode.CrLf);
  /// Console.WriteLine("Last 5 lines and the first kept using CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines and the first of "example.txt" based on CrLf line breaks and removes all other
  ///   preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, LineBreakMode newLine) => _KeepLastLines(@this, count, null, newLine, offsetInLines);

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, using the provided encoding and line
  ///   break mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, Encoding.UTF8, LineBreakMode.CrLf);
  /// Console.WriteLine("Last 5 lines kept using UTF-8 encoding and CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines of "example.txt" using UTF-8 encoding and CrLf line breaks, removing all other
  ///   preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ArgumentIsNull(encoding);

    _KeepLastLines(@this, count, encoding, newLine, 0);
  }

  /// <summary>
  ///   Keeps only the specified number of last lines in the file, discarding the rest, using the provided encoding and line
  ///   break mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines from the end of the file to keep.</param>
  /// <param name="offsetInLines">The number of lines to keep at the start of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.KeepLastLines(5, 1, Encoding.UTF8, LineBreakMode.CrLf);
  /// Console.WriteLine("Last 5 lines and the first kept using UTF-8 encoding and CrLf line breaks, others removed.");
  /// </code>
  ///   This example keeps the last 5 lines and the first of "example.txt" using UTF-8 encoding and CrLf line breaks,
  ///   removing all other preceding lines.
  /// </example>
  public static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, Encoding encoding, LineBreakMode newLine) {
    Against.ArgumentIsNull(encoding);

    _KeepLastLines(@this, count, encoding, newLine, offsetInLines);
  }

  private static void _KeepLastLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine, int offsetInLines) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(newLine);
    Against.NegativeValues(offsetInLines);

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var linePositions = new long[count];
    var index = 0;

    CustomTextReader.Initialized reader =
        encoding == null
          ? new(stream, true, newLine)
          : new(stream, encoding, newLine)
      ;

    var writePosition = reader.PreambleSize;
    while (offsetInLines-- > 0 && reader.ReadLine() != null)
      writePosition = reader.Position;

    for (;;) {
      var startOfLine = reader.Position;
      if (reader.ReadLine() == null)
        break;

      linePositions[index] = ++startOfLine;
      index = ++index % linePositions.Length;
    }

    var readPosition = linePositions[index];
    if (readPosition <= 1)
      return;

    --readPosition;

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    for (;;) {
      stream.Position = readPosition;
      var bytesRead = stream.Read(buffer, 0, bufferSize);
      if (bytesRead <= 0)
        break;

      stream.Position = writePosition;
      stream.Write(buffer, 0, bytesRead);

      readPosition += bytesRead;
      writePosition += bytesRead;
    }

    stream.SetLength(writePosition);
  }

  /// <summary>
  ///   Removes a specified number of lines from the beginning of the file.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the beginning of the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveFirstLines(3);
  /// Console.WriteLine("First 3 lines removed.");
  /// </code>
  ///   This example removes the first 3 lines from "example.txt".
  /// </example>
  public static void RemoveFirstLines(this FileInfo @this, int count) => _RemoveFirstLines(@this, count, null, LineBreakMode.AutoDetect);

  /// <summary>
  ///   Removes a specified number of lines from the beginning of the file using the provided encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the beginning of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveFirstLines(3, Encoding.UTF8);
  /// Console.WriteLine("First 3 lines removed using UTF-8 encoding.");
  /// </code>
  ///   This example removes the first 3 lines from "example.txt" using UTF-8 encoding.
  /// </example>
  public static void RemoveFirstLines(this FileInfo @this, int count, Encoding encoding) {
    Against.ArgumentIsNull(encoding);

    _RemoveFirstLines(@this, count, encoding, LineBreakMode.AutoDetect);
  }

  /// <summary>
  ///   Removes a specified number of lines from the beginning of the file, recognizing line breaks based on the specified
  ///   mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the beginning of the file.</param>
  /// <param name="newLine">The line break mode to use for identifying line endings.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveFirstLines(3, LineBreakMode.CrLf);
  /// Console.WriteLine("First 3 lines removed using CrLf line breaks.");
  /// </code>
  ///   This example removes the first 3 lines from "example.txt", identifying lines based on carriage return and line feed
  ///   (CrLf).
  /// </example>
  public static void RemoveFirstLines(this FileInfo @this, int count, LineBreakMode newLine) => _RemoveFirstLines(@this, count, null, newLine);

  /// <summary>
  ///   Removes a specified number of lines from the beginning of the file using the provided encoding and recognizing line
  ///   breaks based on the specified mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the beginning of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <param name="newLine">The line break mode to determine the line endings in the file.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveFirstLines(3, Encoding.UTF8, LineBreakMode.CrLf);
  /// Console.WriteLine("First 3 lines removed using UTF-8 encoding and CrLf line breaks.");
  /// </code>
  ///   This example removes the first 3 lines from "example.txt" using UTF-8 encoding and CrLf line breaks.
  /// </example>
  public static void RemoveFirstLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ArgumentIsNull(encoding);
    _RemoveFirstLines(@this, count, encoding, newLine);
  }

  private static void _RemoveFirstLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(newLine);

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    CustomTextReader.Initialized reader = encoding == null
        ? new(stream, true, newLine)
        : new(stream, encoding, newLine)
      ;

    var readPosition = 0L;
    var lineCounter = 0;

    while (lineCounter < count) {
      var line = reader.ReadLine();
      if (line == null)
        break;

      readPosition = reader.Position;
      ++lineCounter;
    }

    if (lineCounter < count) {
      stream.SetLength(reader.PreambleSize);
      return;
    }

    var writePosition = reader.PreambleSize;

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    for (;;) {
      stream.Position = readPosition;
      var bytesRead = stream.Read(buffer, 0, bufferSize);
      if (bytesRead <= 0)
        break;

      stream.Position = writePosition;
      stream.Write(buffer, 0, bytesRead);

      readPosition += bytesRead;
      writePosition += bytesRead;
    }

    stream.SetLength(writePosition);
  }

  /// <summary>
  ///   Removes a specified number of lines from the end of the file.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the end of the file.</param>
  /// <note>
  ///   Line-Endings used and encoding is automatically detected.
  /// </note>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveLastLines(2);
  /// Console.WriteLine("Last two lines removed.");
  /// </code>
  ///   This example removes the last two lines from "example.txt".
  /// </example>
  public static void RemoveLastLines(this FileInfo @this, int count) => _RemoveLastLines(@this, count, null, LineBreakMode.AutoDetect);

  /// <summary>
  ///   Removes a specified number of lines from the end of the file using the provided encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the end of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <note>
  ///   Line-Endings used are automatically detected.
  /// </note>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveLastLines(2, Encoding.UTF8);
  /// Console.WriteLine("Last two lines removed using UTF-8 encoding.");
  /// </code>
  ///   This example removes the last two lines from "example.txt" using UTF-8 encoding.
  /// </example>
  public static void RemoveLastLines(this FileInfo @this, int count, Encoding encoding) {
    Against.ArgumentIsNull(encoding);

    _RemoveLastLines(@this, count, encoding, LineBreakMode.AutoDetect);
  }

  /// <summary>
  ///   Removes a specified number of lines from the end of the file, recognizing line breaks based on the specified mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the end of the file.</param>
  /// <param name="newLine">The line break mode to use for identifying line endings.</param>
  /// <note>
  ///   Encoding is automatically detected.
  /// </note>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveLastLines(2, LineBreakMode.CrLf);
  /// Console.WriteLine("Last two lines removed using CrLf line breaks.");
  /// </code>
  ///   This example removes the last two lines from "example.txt", identifying lines based on carriage return and line feed
  ///   (CrLf).
  /// </example>
  public static void RemoveLastLines(this FileInfo @this, int count, LineBreakMode newLine) => _RemoveLastLines(@this, count, null, newLine);

  /// <summary>
  ///   Removes a specified number of lines from the end of the file using the provided encoding and recognizing line breaks
  ///   based on the specified mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> object representing the file.</param>
  /// <param name="count">The number of lines to remove from the end of the file.</param>
  /// <param name="encoding">The encoding to use for interpreting the file's content.</param>
  /// <param name="newLine">The line break mode to use for identifying line endings.</param>
  /// <example>
  ///   <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.RemoveLastLines(2, Encoding.UTF8, LineBreakMode.CrLf);
  /// Console.WriteLine("Last two lines removed using UTF-8 encoding and CrLf line breaks.");
  /// </code>
  ///   This example removes the last two lines from "example.txt" using UTF-8 encoding and identifying lines based on
  ///   carriage return and line feed (CrLf).
  /// </example>
  public static void RemoveLastLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ArgumentIsNull(encoding);

    _RemoveLastLines(@this, count, encoding, newLine);
  }

  private static void _RemoveLastLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(newLine);

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var linePositions = new long[count];
    var index = 0;

    CustomTextReader.Initialized reader = encoding == null
        ? new(stream, true, newLine)
        : new(stream, encoding, newLine)
      ;

    for (;;) {
      var startOfLine = reader.Position;
      if (reader.ReadLine() == null)
        break;

      linePositions[index] = ++startOfLine;
      index = ++index % linePositions.Length;
    }

    var truncate = Math.Max(reader.PreambleSize, linePositions[index] - 1);
    stream.SetLength(truncate);
  }

  #endregion

  private static LineBreakMode _DetectLineBreakMode(CustomTextReader.Initialized stream) {
    const char CR = (char)LineBreakMode.CarriageReturn;
    const char LF = (char)LineBreakMode.LineFeed;
    const char FF = (char)LineBreakMode.FormFeed;
    const char NEL = (char)LineBreakMode.NextLine;
    const char LS = (char)LineBreakMode.LineSeparator;
    const char PS = (char)LineBreakMode.ParagraphSeparator;
    const char NL = (char)LineBreakMode.NegativeAcknowledge;
    const char EOL = (char)LineBreakMode.EndOfLine;
    const char ZX = (char)LineBreakMode.Zx;
    const char NUL = (char)LineBreakMode.Null;

    var previousCharacter = stream.Read();
    if (previousCharacter < 0)
      return LineBreakMode.None;

    switch (previousCharacter) {
      case FF: return LineBreakMode.FormFeed;
      case NEL: return LineBreakMode.NextLine;
      case LS: return LineBreakMode.LineSeparator;
      case PS: return LineBreakMode.ParagraphSeparator;
      case NL: return LineBreakMode.NegativeAcknowledge;
      case EOL: return LineBreakMode.EndOfLine;
      case ZX: return LineBreakMode.Zx;
      case NUL: return LineBreakMode.Null;
    }

    for (;;) {
      var currentCharacter = stream.Read();
      if (currentCharacter < 0)
        break;

      switch (currentCharacter) {
        case CR when previousCharacter == LF: return LineBreakMode.LfCr;
        case CR when previousCharacter == CR: return LineBreakMode.CarriageReturn;
        case LF when previousCharacter == LF: return LineBreakMode.LineFeed;
        case LF when previousCharacter == CR: return LineBreakMode.CrLf;
        case FF: return LineBreakMode.FormFeed;
        case NEL: return LineBreakMode.NextLine;
        case LS: return LineBreakMode.LineSeparator;
        case PS: return LineBreakMode.ParagraphSeparator;
        case NL: return LineBreakMode.NegativeAcknowledge;
        case EOL: return LineBreakMode.EndOfLine;
        case ZX: return LineBreakMode.Zx;
        case NUL: return LineBreakMode.Null;
      }

      previousCharacter = currentCharacter;
    }

    return previousCharacter switch {
      CR => LineBreakMode.CarriageReturn,
      LF => LineBreakMode.LineFeed,
      _ => LineBreakMode.None
    };
  }

  public static LineBreakMode DetectLineBreakMode(this FileInfo @this) {
    Against.ThisIsNull(@this);

    using var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    var reader = new CustomTextReader.Initialized(stream, true, LineBreakMode.None);
    return _DetectLineBreakMode(reader);
  }

  public static LineBreakMode DetectLineBreakMode(this FileInfo @this, Encoding encoding) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(encoding);

    using var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    var reader = new CustomTextReader.Initialized(stream, encoding, LineBreakMode.None);
    return _DetectLineBreakMode(reader);
  }


  private static readonly Dictionary<int, Range[]> _CODEPOINT_RANGES = new() {
    { 37, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 437, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9A,0xA0..0xA5,0xE0..0xEB,0xED..0xEE] },
    { 500, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 708, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x82..0x83,0x85..0x85,0x87..0x8C,0x93..0x93,0x96..0x97,0xC1..0xDA,0xE0..0xEA] },
    { 720, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x82..0x83,0x85..0x85,0x87..0x8C,0x91..0x93,0x95..0x9B,0x9D..0xAD,0xE0..0xEF,0xF1..0xF6] },
    { 737, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0xAF,0xE0..0xF0,0xF4..0xF5] },
    { 775, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x95,0x97..0x9B,0x9D..0x9D,0xA0..0xA5,0xAD..0xAD,0xB5..0xB8,0xBD..0xBE,0xC6..0xC7,0xCF..0xD8,0xE0..0xEE] },
    { 850, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9B,0x9D..0x9D,0xA0..0xA5,0xB5..0xB7,0xC6..0xC7,0xD0..0xD8,0xDE..0xDE,0xE0..0xED] },
    { 852, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9D,0x9F..0xA9,0xAB..0xAD,0xB5..0xB8,0xBD..0xBE,0xC6..0xC7,0xD0..0xD8,0xDD..0xDE,0xE0..0xEE,0xFB..0xFD] },
    { 855, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0xAD,0xB5..0xB8,0xBD..0xBE,0xC6..0xC7,0xD0..0xD8,0xDD..0xDE,0xE0..0xEE,0xF1..0xFC] },
    { 857, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9B,0x9D..0xA7,0xB5..0xB7,0xC6..0xC7,0xD2..0xD4,0xD6..0xD8,0xDE..0xDE,0xE0..0xE5,0xE9..0xED] },
    { 858, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9B,0x9D..0x9D,0xA0..0xA5,0xB5..0xB7,0xC6..0xC7,0xD0..0xD8,0xE0..0xE5,0xE7..0xED] },
    { 860, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9A,0x9D..0xA5,0xA9..0xA9,0xE0..0xEB,0xED..0xEE] },
    { 861, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9B,0x9D..0xA7,0xE0..0xEB,0xED..0xEE] },
    { 862, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9A,0x9F..0xA5,0xE0..0xEB,0xED..0xEE] },
    { 863, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x85,0x87..0x8C,0x8E..0x8E,0x90..0x97,0x99..0x9A,0x9D..0x9F,0xA2..0xA3,0xA8..0xA8,0xE0..0xEB,0xED..0xEE] },
    { 864, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x99..0x9A,0x9D..0x9E,0xA2..0xA2,0xA5..0xA5,0xA8..0xDA,0xDF..0xDF,0xE1..0xFD] },
    { 865, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9B,0x9D..0x9D,0xA0..0xA5,0xE0..0xEB,0xED..0xEE] },
    { 866, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0xAF,0xE0..0xF7] },
    { 869, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x86..0x86,0x8D..0x8D,0x8F..0x92,0x95..0x96,0x98..0x98,0x9B..0x9B,0x9D..0xAA,0xAC..0xAD,0xB5..0xB8,0xBD..0xBE,0xC6..0xC7,0xCF..0xD8,0xDD..0xDE,0xE0..0xEE,0xF2..0xF4,0xF6..0xF6,0xFA..0xFD] },
    { 870, [0x42..0x49,0x51..0x59,0x62..0x63,0x65..0x69,0x71..0x78,0x81..0x8F,0x91..0x9C,0xA0..0xA0,0xA2..0xAF,0xB1..0xB4,0xB6..0xBC,0xC1..0xC9,0xCB..0xCF,0xD1..0xDF,0xE2..0xFE] },
    { 874, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xD0,0xD2..0xD3,0xE0..0xE6,0xF0..0xF9] },
    { 875, [0x41..0x49,0x51..0x59,0x62..0x69,0x71..0x73,0x75..0x78,0x81..0x8F,0x91..0x9F,0xA2..0xAF,0xB1..0xBF,0xC1..0xC9,0xCB..0xCD,0xD1..0xD9,0xE2..0xE9,0xF0..0xF9] },
    { 932, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA6..0xDF] },
    { 936, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 949, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 950, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 1026, [0x42..0x47,0x49..0x4A,0x51..0x5B,0x62..0x67,0x69..0x6A,0x70..0x79,0x7B..0x7C,0x7F..0x89,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFB,0xFD..0xFE] },
    { 1047, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAC,0xAE..0xAE,0xBA..0xBA,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1140, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1141, [0x42..0x42,0x44..0x4A,0x51..0x58,0x5A..0x5A,0x62..0x62,0x64..0x6A,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFB,0xFD..0xFE] },
    { 1142, [0x42..0x46,0x48..0x49,0x51..0x59,0x5B..0x5B,0x62..0x66,0x68..0x6A,0x71..0x78,0x7B..0x7C,0x81..0x89,0x8C..0x8E,0x91..0x99,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xD9,0xDB..0xDB,0xDD..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1143, [0x42..0x42,0x44..0x46,0x48..0x49,0x52..0x59,0x5B..0x5B,0x62..0x62,0x64..0x66,0x68..0x6A,0x70..0x70,0x72..0x79,0x7B..0x7C,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFE] },
    { 1144, [0x42..0x43,0x45..0x47,0x49..0x49,0x52..0x53,0x55..0x57,0x59..0x5A,0x62..0x6A,0x70..0x79,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCC,0xCE..0xD9,0xDB..0xDC,0xDE..0xE0,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1145, [0x42..0x48,0x51..0x59,0x62..0x68,0x6A..0x6A,0x70..0x78,0x7B..0x7B,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1146, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1147, [0x42..0x43,0x45..0x47,0x49..0x49,0x52..0x53,0x55..0x59,0x62..0x6A,0x70..0x79,0x7C..0x7C,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xD9,0xDB..0xDC,0xDE..0xE0,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1148, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 1149, [0x42..0x4A,0x51..0x5A,0x5F..0x5F,0x62..0x69,0x70..0x79,0x7C..0x7C,0x80..0x89,0x8D..0x8D,0x91..0x99,0xA1..0xA9,0xAD..0xAD,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFE] },
    { 1250, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x8A..0x8A,0x8C..0x8F,0x9A..0x9A,0x9C..0x9F,0xA3..0xA3,0xA5..0xA5,0xAA..0xAA,0xAF..0xAF,0xB3..0xB3,0xB5..0xB5,0xB9..0xBA,0xBC..0xBC,0xBE..0xD6,0xD8..0xF6,0xF8..0xFE] },
    { 1251, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x81,0x83..0x83,0x8A..0x8A,0x8C..0x90,0x9A..0x9A,0x9C..0x9F,0xA1..0xA3,0xA5..0xA5,0xA8..0xA8,0xAA..0xAA,0xAF..0xAF,0xB2..0xB5,0xB8..0xB8,0xBA..0xBA,0xBC..0xFF] },
    { 1252, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x80,0x8A..0x8A,0x8C..0x8C,0x8E..0x8E,0x9A..0x9A,0x9C..0x9C,0x9E..0x9F,0xA1..0xA2,0xC0..0xD6,0xD8..0xF6,0xF8..0xFF] },
    { 1253, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA2..0xA2,0xB8..0xBA,0xBC..0xBC,0xBE..0xD1,0xD3..0xFE] },
    { 1254, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x83..0x83,0x88..0x88,0x8A..0x8A,0x8C..0x8C,0x9A..0x9A,0x9C..0x9C,0x9F..0x9F,0xC0..0xD6,0xD8..0xF6,0xF8..0xFF] },
    { 1255, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xD4..0xD6,0xE0..0xFA] },
    { 1256, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x81..0x81,0x88..0x88,0x8A..0x8A,0x8C..0x90,0x98..0x98,0x9A..0x9A,0x9C..0x9C,0x9F..0x9F,0xAA..0xAA,0xC0..0xD6,0xD8..0xEF,0xF4..0xF4,0xF9..0xF9,0xFB..0xFC,0xFF..0xFF] },
    { 1257, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x8E..0x8E,0xA8..0xA8,0xAA..0xAA,0xAF..0xAF,0xB8..0xB8,0xBA..0xBA,0xBF..0xD6,0xD8..0xF6,0xF8..0xFE] },
    { 1258, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x8C..0x8C,0x9C..0x9C,0x9F..0x9F,0xC0..0xCB,0xCD..0xD1,0xD3..0xD6,0xD8..0xDD,0xDF..0xEB,0xED..0xF1,0xF3..0xF6,0xF8..0xFD,0xFF..0xFF] },
    { 1361, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 10000, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA7..0xA7,0xAE..0xAF,0xBD..0xBF,0xCB..0xCF,0xD8..0xD9,0xDE..0xDF,0xE5..0xEF,0xF1..0xF6] },
    { 10001, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA6..0xDF] },
    { 10002, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 10003, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 10004, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x80,0x82..0x8B,0x8D..0x92,0x94..0x97,0x99..0x9A,0x9C..0x9F,0xB0..0xB9,0xC1..0xDA,0xE0..0xEA,0xF3..0xFA,0xFE..0xFF] },
    { 10005, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xE0..0xFA] },
    { 10006, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x80,0x83..0x83,0x85..0x86,0x88..0x8A,0x8D..0x91,0x94..0x95,0x99..0x9A,0x9D..0x9F,0xA1..0xA7,0xAA..0xAB,0xB0..0xB0,0xB5..0xC1,0xC3..0xC4,0xC6..0xC6,0xCB..0xCF,0xD7..0xFE] },
    { 10007, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA7..0xA7,0xAB..0xAC,0xAE..0xAF,0xB4..0xB4,0xB7..0xC1,0xCB..0xCF,0xD8..0xDB,0xDD..0xFE] },
    { 10008, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 10010, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA7..0xA7,0xAE..0xAF,0xBB..0xBF,0xCB..0xCF,0xD8..0xD9,0xDE..0xDF,0xE5..0xEF,0xF1..0xF5] },
    { 10017, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA2..0xA2,0xA7..0xA7,0xAB..0xAC,0xAE..0xAF,0xB4..0xB4,0xB6..0xC1,0xCB..0xCF,0xD8..0xDB,0xDD..0xFE] },
    { 10021, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xD0,0xD2..0xD3,0xE0..0xE6,0xF0..0xF9] },
    { 10029, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA2..0xA2,0xA7..0xA7,0xAB..0xAB,0xAE..0xB1,0xB4..0xB5,0xB8..0xC1,0xC4..0xC5,0xCB..0xCF,0xD8..0xDB,0xDE..0xE1,0xE4..0xFE] },
    { 10079, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0xA0,0xA7..0xA7,0xAE..0xAF,0xBD..0xBF,0xCB..0xCF,0xD8..0xD9,0xDC..0xE0,0xE5..0xEF,0xF1..0xF5] },
    { 10081, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA7..0xA7,0xAE..0xAF,0xBD..0xBF,0xCB..0xCF,0xD8..0xDF,0xE5..0xEF,0xF1..0xF4] },
    { 10082, [0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0x9F,0xA7..0xA7,0xA9..0xA9,0xAE..0xAF,0xB9..0xB9,0xBB..0xBF,0xC6..0xC6,0xC8..0xC8,0xCB..0xD0,0xDE..0xDE,0xE5..0xF5,0xF9..0xFA,0xFD..0xFE] },
    { 20000, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20001, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20002, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20003, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20004, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20005, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20105, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20106, [0x30..0x39,0x41..0x5D,0x61..0x7E] },
    { 20107, [0x30..0x39,0x40..0x5E,0x60..0x7E] },
    { 20108, [0x30..0x39,0x41..0x5D,0x61..0x7D] },
    { 20127, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20261, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xE0..0xE4,0xE6..0xFE] },
    { 20269, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xE0..0xE2,0xE4..0xE4,0xE6..0xFD] },
    { 20273, [0x42..0x42,0x44..0x4A,0x51..0x58,0x5A..0x5A,0x62..0x62,0x64..0x6A,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFB,0xFD..0xFE] },
    { 20277, [0x42..0x46,0x48..0x49,0x51..0x59,0x5B..0x5B,0x62..0x66,0x68..0x6A,0x71..0x78,0x7B..0x7C,0x81..0x89,0x8C..0x8E,0x91..0x99,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xD9,0xDB..0xDB,0xDD..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20278, [0x42..0x42,0x44..0x46,0x48..0x49,0x52..0x59,0x5B..0x5B,0x62..0x62,0x64..0x66,0x68..0x6A,0x70..0x70,0x72..0x79,0x7B..0x7C,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFE] },
    { 20280, [0x42..0x43,0x45..0x47,0x49..0x49,0x52..0x53,0x55..0x57,0x59..0x5A,0x62..0x6A,0x70..0x79,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA1..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xCC,0xCE..0xD9,0xDB..0xDC,0xDE..0xE0,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20284, [0x42..0x48,0x51..0x59,0x62..0x68,0x6A..0x6A,0x70..0x78,0x7B..0x7B,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20285, [0x42..0x49,0x51..0x59,0x62..0x69,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20290, [0x46..0x49,0x51..0x56,0x58..0x58,0x62..0x69,0x71..0x78,0x81..0x9B,0x9D..0x9F,0xA2..0xAF,0xB3..0xBF,0xC1..0xC9,0xD1..0xD9,0xE2..0xE9,0xF0..0xF9] },
    { 20297, [0x42..0x43,0x45..0x47,0x49..0x49,0x52..0x53,0x55..0x59,0x62..0x6A,0x70..0x79,0x7C..0x7C,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9C,0x9E..0x9E,0xA2..0xA9,0xAC..0xAE,0xC0..0xC9,0xCB..0xD9,0xDB..0xDC,0xDE..0xE0,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20420, [0x42..0x44,0x46..0x49,0x51..0x52,0x55..0x59,0x62..0x69,0x70..0x76,0x78..0x78,0x81..0x8A,0x8C..0x8C,0x8E..0xA0,0xA2..0xB5,0xB8..0xBF,0xC1..0xC9,0xCB..0xCB,0xCD..0xCD,0xCF..0xCF,0xD1..0xDF,0xE2..0xEB,0xED..0xF9,0xFB..0xFE] },
    { 20423, [0x41..0x49,0x51..0x59,0x62..0x67,0x71..0x73,0x75..0x78,0x80..0xA0,0xA2..0xAF,0xB1..0xBF,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xFC] },
    { 20424, [0x41..0x49,0x51..0x59,0x62..0x69,0x71..0x71,0x81..0x89,0x91..0x99,0xA0..0xA0,0xA2..0xA9,0xC1..0xC9,0xD1..0xD9,0xE2..0xE9,0xF0..0xF9] },
    { 20833, [0x42..0x49,0x52..0x59,0x62..0x69,0x72..0x78,0x81..0x8F,0x91..0x9F,0xA2..0xAF,0xBA..0xBC,0xC1..0xC9,0xD1..0xD9,0xE2..0xE9,0xF0..0xF9] },
    { 20838, [0x42..0x48,0x52..0x58,0x62..0x68,0x72..0x78,0x81..0x8F,0x91..0x9F,0xA2..0xBB,0xBD..0xBE,0xC1..0xC9,0xD1..0xD9,0xDB..0xDF,0xE2..0xEB,0xF0..0xF9] },
    { 20866, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA3..0xA3,0xB3..0xB3,0xC0..0xFF] },
    { 20871, [0x42..0x4A,0x51..0x5A,0x5F..0x5F,0x62..0x69,0x70..0x79,0x7C..0x7C,0x80..0x89,0x91..0x99,0xA1..0xA9,0xAD..0xAD,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFE] },
    { 20880, [0x42..0x49,0x51..0x57,0x59..0x59,0x62..0x69,0x70..0x72,0x74..0x78,0x80..0xA0,0xA2..0xBF,0xC1..0xCF,0xD1..0xDF,0xE2..0xFE] },
    { 20905, [0x42..0x45,0x47..0x47,0x49..0x4A,0x51..0x5B,0x62..0x65,0x67..0x67,0x69..0x6A,0x71..0x79,0x7B..0x7C,0x7F..0x7F,0x81..0x8D,0x91..0x9C,0xA1..0xAD,0xB2..0xB2,0xB4..0xB4,0xBA..0xBC,0xC0..0xC9,0xCB..0xCB,0xCD..0xD9,0xDB..0xDB,0xDD..0xDE,0xE0..0xE0,0xE2..0xE9,0xEB..0xEB,0xED..0xF9,0xFB..0xFB,0xFD..0xFE] },
    { 20924, [0x42..0x4A,0x51..0x59,0x62..0x6A,0x70..0x78,0x80..0x89,0x8C..0x8E,0x91..0x99,0x9C..0x9E,0xA2..0xA9,0xAC..0xAC,0xAE..0xAE,0xB7..0xB9,0xBB..0xBB,0xBE..0xBE,0xC1..0xC9,0xCB..0xCF,0xD1..0xD9,0xDB..0xDF,0xE2..0xE9,0xEB..0xF9,0xFB..0xFE] },
    { 20932, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20936, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 20949, [0x30..0x39,0x41..0x5A,0x61..0x7A] },
    { 21025, [0x42..0x49,0x51..0x57,0x59..0x59,0x62..0x69,0x70..0x72,0x74..0x78,0x80..0xA0,0xA2..0xBF,0xC1..0xCF,0xD1..0xDF,0xE2..0xFE] },
    { 21866, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA3..0xA4,0xA6..0xA7,0xAD..0xAE,0xB3..0xB4,0xB6..0xB7,0xBD..0xBE,0xC0..0xFF] },
    { 28591, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xC0..0xD6,0xD8..0xF6,0xF8..0xFF] },
    { 28592, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xA1,0xA3..0xA3,0xA5..0xA6,0xA9..0xAC,0xAE..0xAF,0xB1..0xB1,0xB3..0xB3,0xB5..0xB6,0xB9..0xBC,0xBE..0xD6,0xD8..0xF6,0xF8..0xFE] },
    { 28593, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xA1,0xA6..0xA6,0xA9..0xAC,0xAF..0xAF,0xB1..0xB1,0xB5..0xB6,0xB9..0xBC,0xBF..0xC2,0xC4..0xCF,0xD1..0xD6,0xD8..0xE2,0xE4..0xEF,0xF1..0xF6,0xF8..0xFE] },
    { 28594, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xA3,0xA5..0xA6,0xA9..0xAC,0xAE..0xAE,0xB1..0xB1,0xB3..0xB3,0xB5..0xB7,0xB9..0xD6,0xD8..0xF6,0xF8..0xFE] },
    { 28595, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA1..0xAC,0xAE..0xEF,0xF1..0xFC,0xFE..0xFF] },
    { 28596, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xC1..0xDA,0xE0..0xEA] },
    { 28597, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xB6..0xB6,0xB8..0xBA,0xBC..0xBC,0xBE..0xD1,0xD3..0xFE] },
    { 28598, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xE0..0xFA] },
    { 28599, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xC0..0xD6,0xD8..0xF6,0xF8..0xFF] },
    { 28603, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA8..0xA8,0xAA..0xAA,0xAF..0xAF,0xB8..0xB8,0xBA..0xBA,0xBF..0xD6,0xD8..0xF6,0xF8..0xFE] },
    { 28605, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xA6..0xA6,0xA8..0xA8,0xB4..0xB5,0xB8..0xB8,0xBC..0xBE,0xC0..0xD6,0xD8..0xF6,0xF8..0xFF] },
    { 29001, [0x01..0x06,0x17..0x17,0x1C..0x1C,0x30..0x39,0x41..0x5A,0x61..0x7A,0x80..0xB2,0xB4..0xB9,0xBD..0xBE,0xC1..0xC1,0xC6..0xC7,0xCA..0xCC,0xCE..0xD8,0xDB..0xF9,0xFB..0xFE] },
    { 38598, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xE0..0xFA] },
    { 50220, [0x30..0x39,0x41..0x5A,0x61..0x7A,0xE0..0xFA] },
  };

  /// <summary>
  /// Detects the encoding of a file by analyzing its contents.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> instance representing the file to analyze.</param>
  /// <param name="heuristicSize">
  /// (Optional: defaults to 4096) The number of bytes to read from the beginning of the file to analyze for encoding detection. 
  /// A larger value may improve accuracy but increases the read time.
  /// </param>
  /// <returns>
  /// The detected <see cref="Encoding"/> of the file, or <see langword="null"/> if the encoding cannot be determined.
  /// </returns>
  /// <exception cref="System.NullReferenceException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.IO.IOException">
  /// Thrown if an I/O error occurs while reading the file.
  /// </exception>
  /// <note>If a BOM is found, the heuristic is not used.</note>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// Encoding encoding = file.DetectEncoding();
  /// Console.WriteLine(encoding?.EncodingName ?? "maybe that's a binary file"); // Output: Detected encoding name
  /// </code>
  /// </example>
  public static Encoding DetectEncoding(this FileInfo @this, int heuristicSize = 4096) {
    Against.ThisIsNull(@this);

    byte[] buffer;
    int bytesRead;

    using (var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
      var detectFromByteOrderMark = CustomTextReader.DetectFromByteOrderMark(stream);
      if (detectFromByteOrderMark != null)
        return detectFromByteOrderMark;

      // No BOM detected, use heuristic to guess encoding
      stream.Seek(0, SeekOrigin.Begin);
      buffer = new byte[heuristicSize];
      bytesRead = stream.Read(buffer, 0, buffer.Length);
    }

    // Create a histogram for bytes read
    var byteHistogram = new int[256];
    for (var i = 0; i < bytesRead; ++i)
      ++byteHistogram[buffer[i]];

    buffer = null;

    // Create statistics
    var usedCharsPerCodepage = new Dictionary<int, HashSet<byte>>();
    var charsInRangePerCodepage = new Dictionary<int, int>();
    var charsOutOfRangePerCodepage = new Dictionary<int, int>();
    for (var i = 0; i < 256; ++i) {
      if (byteHistogram[i] == 0)
        continue;

      var currentByte = (byte)i;
      foreach (var (codepage, ranges) in _CODEPOINT_RANGES) {
        if (!i.IsInRange(ranges)) {
          charsOutOfRangePerCodepage[codepage] += byteHistogram[i];
          continue;
        }

        charsInRangePerCodepage[codepage] += byteHistogram[i];
        usedCharsPerCodepage.GetOrAdd(codepage, () => []).Add(currentByte);
      }
    }

    // Create a list of all Encodings that scored ordered by likelihood
    var most = bytesRead * 0.75f;
    var systemCodepage = Encoding.Default.CodePage;
    var likelyEncodings =
        from kvp in charsInRangePerCodepage
        let codepage = kvp.Key
        let charsCounted = kvp.Value
        where charsCounted >= most
        let availableCharsInCodepage = _CODEPOINT_RANGES[codepage].Sum(range => range.End.Value - range.Start.Value + 1)
        let usedCharsFromCodepage = usedCharsPerCodepage[codepage].Count
        let relativeUsage = (float)usedCharsFromCodepage / availableCharsInCodepage
        let charsOutsideCodepage = charsOutOfRangePerCodepage[codepage]
        orderby charsCounted descending,
          codepage == systemCodepage descending,
          relativeUsage descending,
          charsOutsideCodepage
        select (count: kvp.Value, getEncoding: CreateEncodingFactory(kvp.Key))
      ;

    return likelyEncodings.Select(i => {
      try {
        return i.getEncoding();
      } catch {
        return null;
      }
    }).FirstOrDefault();

    Func<Encoding> CreateEncodingFactory(int codepage) => () => Encoding.GetEncoding(codepage);
  }

  #region opening

  /// <summary>
  ///   Opens the specified file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="access">The access.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize) => new(@this.FullName, mode, access, share, bufferSize);

  /// <summary>
  ///   Opens the specified file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="access">The access.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <param name="options">The options.</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) => new(@this.FullName, mode, access, share, bufferSize, options);

  /// <summary>
  ///   Opens the specified file.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="access">The access.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <param name="useAsync">if set to <c>true</c> [use async].</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) => new(@this.FullName, mode, access, share, bufferSize, useAsync);

#if !NETCOREAPP3_1_OR_GREATER && !NETSTANDARD

  /// <summary>
  ///   Opens the specified file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="rights">The rights.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <param name="options">The options.</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options) => new(@this.FullName, mode, rights, share, bufferSize, options);

  /// <summary>
  ///   Opens the specified file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="rights">The rights.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <param name="options">The options.</param>
  /// <param name="security">The security.</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity security) => new(@this.FullName, mode, rights, share, bufferSize, options, security);

#endif

  #endregion

  #region get part of filename

  /// <summary>
  ///   Gets the filename without extension.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The filename without the extension.</returns>
  public static string GetFilenameWithoutExtension(this FileInfo @this) => Path.GetFileNameWithoutExtension(@this.FullName);

  /// <summary>
  ///   Gets the filename.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The filename.</returns>
  public static string GetFilename(this FileInfo @this) => Path.GetFileName(@this.FullName);

  /// <summary>
  ///   Creates an instance with a new extension.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="extension">The extension.</param>
  /// <returns>A new FileInfo instance with given extension.</returns>
  public static FileInfo WithNewExtension(this FileInfo @this, string extension) => new(Path.ChangeExtension(@this.FullName, extension));

  #endregion

  /// <summary>
  ///   Tries to delete the given file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryDelete(this FileInfo @this) {
    Against.ThisIsNull(@this);

    try {
      var fullName = @this.FullName;
      if (File.Exists(fullName))
        File.Delete(fullName);
      else
        return false;
      return true;
    } catch (Exception) {
      return false;
    }
  }

  /// <summary>
  ///   Tries to create a new file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns>
  ///   <c>true</c> if the file didn't exist and was successfully created; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreate(this FileInfo @this, FileAttributes attributes = FileAttributes.Normal) {
    Against.ThisIsNull(@this);

    if (@this.Exists)
      return false;

    try {
      var fileHandle = @this.Open(FileMode.CreateNew, FileAccess.Write);
      fileHandle.Close();
      @this.Attributes = attributes;
      return true;
    } catch (UnauthorizedAccessException) {
      // in case multiple threads try to create the same file, this gets fired
      return false;
    } catch (IOException) {
      // file already exists
      return false;
    }
  }

  /// <summary>
  ///   Changes the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  public static void Touch(this FileInfo @this) => @this.LastWriteTimeUtc = DateTime.UtcNow;

  /// <summary>
  ///   Tries to change the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryTouch(this FileInfo @this) {
    try {
      Touch(@this);
      return true;
    } catch (Exception) {
      return false;
    }
  }

  /// <summary>
  ///   Tries to change the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="waitTime">The wait time.</param>
  /// <param name="repeat">The repeat.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryTouch(this FileInfo @this, TimeSpan waitTime, int repeat = 3) {
    while (--repeat >= 0) {
      if (TryTouch(@this))
        return true;

      Thread.Sleep(waitTime);
    }

    return false;
  }

  /// <summary>
  ///   Checks whether the given file does not exist.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
  public static bool NotExists(this FileInfo @this) => !@this.Exists;

  private static __ConvertFilePatternToRegex __convertFilePatternToRegex;

  private sealed partial class __ConvertFilePatternToRegex {

#if SUPPORTS_GENERATED_REGEX
    [GeneratedRegex("[" + @"\/:<>|" + "\"]")]
    private partial Regex _IllegalCharactersRegex();
#else
    private readonly Regex _ILLEGAL_CHARACTERS_REGEX = new("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
    private Regex _IllegalCharactersRegex() => this._ILLEGAL_CHARACTERS_REGEX;
#endif

#if SUPPORTS_GENERATED_REGEX
    [GeneratedRegex(@"^\s*.+\.([^\.]+)\s*$")]
    private partial Regex _CatchExtensionRegex();
#else
    private readonly Regex _CATCH_EXTENSION_REGEX = new(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
    private Regex _CatchExtensionRegex() => this._CATCH_EXTENSION_REGEX;
#endif

    public string Invoke(string pattern) {
      Against.ArgumentIsNull(pattern);

      pattern = pattern.Trim();

      if (pattern.Length == 0)
        throw new ArgumentException("Pattern is empty.", nameof(pattern));

      if (this._IllegalCharactersRegex().IsMatch(pattern))
        throw new ArgumentException("Patterns contains illegal characters.", nameof(pattern));

      const string nonDotCharacters = "[^.]*";

      var hasExtension = this._CatchExtensionRegex().IsMatch(pattern);
      var matchExact = false;

      if (pattern.Contains('?'))
        matchExact = true;
      else if (hasExtension)
        matchExact = this._CatchExtensionRegex().Match(pattern).Groups[1].Length != 3;

      var regexString = Regex.Escape(pattern);
      regexString = @"(^|[\\\/])" + Regex.Replace(regexString, @"\\\*", ".*");
      regexString = Regex.Replace(regexString, @"\\\?", ".");

      if (!matchExact && hasExtension)
        regexString += nonDotCharacters;

      regexString += "$";
      return regexString;
    }
  }

  /// <summary>
  ///   Converts a given filename pattern into a regular expression.
  /// </summary>
  /// <param name="pattern">The pattern.</param>
  /// <returns>The regex.</returns>
  private static string _ConvertFilePatternToRegex(string pattern) => (__convertFilePatternToRegex ??= new()).Invoke(pattern);

  public static bool MatchesFilter(this FileInfo @this, string filter) {
    var regex = _ConvertFilePatternToRegex(filter);
    return Regex.IsMatch(@this.FullName, regex, RegexOptions.IgnoreCase);
  }

#if SUPPORTS_STREAM_ASYNC

  /// <summary>
  ///   Compares two files for content equality.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="other">The other FileInfo.</param>
  /// <param name="bufferSize">The number of bytes to compare in each step (Beware of the 85KB-LOH limit).</param>
  /// <returns><c>true</c> if both are bytewise equal; otherwise, <c>false</c>.</returns>
  public static bool IsContentEqualTo(this FileInfo @this, FileInfo other, int bufferSize = 65536) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    if (@this.FullName == other.FullName)
      return true;

    var myLength = @this.Length;
    if (myLength != other.Length)
      return false;

    if (myLength == 0)
      return true;

    using FileStream sourceStream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    using FileStream comparisonStream = new(other.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    {
      // NOTE: we're going to compare buffers (A, A') while reading the next blocks (B, B') in already
      var sourceBufferA = new byte[bufferSize];
      var comparisonBufferA = new byte[bufferSize];

      var sourceBufferB = new byte[bufferSize];
      var comparisonBufferB = new byte[bufferSize];

      var blockCount = Math.DivRem(myLength, bufferSize, out var lastBlockSize);

      // if there are bytes left in a partly filled last block - we need one block more
      if (lastBlockSize != 0)
        ++blockCount;

      using var enumerator = BlockIndexShuffler(blockCount).GetEnumerator();

      // NOTE: should never land here, because only 0-byte files would get us an empty enumerator
      if (!enumerator.MoveNext())
        return false;

      var blockIndex = enumerator.Current;

      // start reading buffers into A and A'
      var position = blockIndex * bufferSize;
      var sourceAsync = sourceStream.ReadBytesAsync(position, sourceBufferA);
      var comparisonAsync = comparisonStream.ReadBytesAsync(position, comparisonBufferA);
      int sourceBytes;
      int comparisonBytes;

      while (enumerator.MoveNext()) {
        sourceBytes = sourceAsync.Result;
        comparisonBytes = comparisonAsync.Result;

        // start reading next buffers into B and B'
        blockIndex = enumerator.Current;
        position = blockIndex * bufferSize;
        sourceAsync = sourceStream.ReadBytesAsync(position, sourceBufferB);
        comparisonAsync = comparisonStream.ReadBytesAsync(position, comparisonBufferB);

        // compare A and A' and return false upon difference
        if (sourceBytes != comparisonBytes || !sourceBufferA.SequenceEqual(0, comparisonBufferA, 0, sourceBytes))
          return false;

        // switch A and B and A' and B'
        (sourceBufferA, sourceBufferB, comparisonBufferA, comparisonBufferB) = (sourceBufferB, sourceBufferA, comparisonBufferB, comparisonBufferA);
      }

      // compare A and A'
      sourceBytes = sourceAsync.Result;
      comparisonBytes = comparisonAsync.Result;
      return sourceBytes == comparisonBytes && sourceBufferA.SequenceEqual(0, comparisonBufferA, 0, sourceBytes);
    }

    static IEnumerable<long> BlockIndexShuffler(long blockCount) {
      var lowerBlockIndex = 0;
      var upperBlockIndex = blockCount - 1;

      while (lowerBlockIndex < upperBlockIndex) {
        yield return lowerBlockIndex++;
        yield return upperBlockIndex--;
      }

      // if odd number of elements, return the last element (which is in the middle)
      if ((blockCount & 1) == 1)
        yield return lowerBlockIndex;
    }
  }

#endif

  /// <summary>
  ///   Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo" /></param>
  /// <param name="other">The file that should replace this file</param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other)
    => ReplaceWith(@this, other, null, false);

  /// <summary>
  ///   Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo" /></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="ignoreMetaDataErrors">
  ///   <see langword="true" /> when metadata errors should be ignored; otherwise,
  ///   <see langword="false" />.
  /// </param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other, bool ignoreMetaDataErrors)
    => ReplaceWith(@this, other, null, ignoreMetaDataErrors);

  /// <summary>
  ///   Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo" /></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="backupFile">The file that gets a backup from this file; optional</param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other, FileInfo backupFile)
    => ReplaceWith(@this, other, backupFile, false);

  /// <summary>
  ///   Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo" /></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="backupFile">The file that gets a backup from this file; optional</param>
  /// <param name="ignoreMetaDataErrors">
  ///   <see langword="true" /> when metadata errors should be ignored; otherwise,
  ///   <see langword="false" />.
  /// </param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other, FileInfo backupFile, bool ignoreMetaDataErrors) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    var targetDir = @this.DirectoryName;
    var sourceDir = other.DirectoryName;
    Against.ValuesAreNotEqual(targetDir, sourceDir, StringComparison.OrdinalIgnoreCase);

    if (backupFile != null) {
      var backupDir = backupFile.DirectoryName;
      Against.ValuesAreNotEqual(targetDir, backupDir, StringComparison.OrdinalIgnoreCase);
    }

    // NOTE: we don't use the FileInfo method of other and @this directly to avoid them being updated to a new path internally and to always get the actual state without caching
    try {
      File.Move(other.FullName, @this.FullName);
      @this.Refresh();
      return;
    } catch (IOException) when (File.Exists(@this.FullName)) {
      // target file exists - continue below
    }

    try {
      File.Replace(other.FullName, @this.FullName, backupFile?.FullName, ignoreMetaDataErrors);
      @this.Refresh();
      return;
    } catch (PlatformNotSupportedException) {
      // Replace method isn't supported on this platform
    }

    // If Replace isn't available or fails, handle the backup file manually
    if (backupFile != null)
      new FileInfo(@this.FullName).MoveTo(backupFile, true);

    // Move the source file to the destination - overwrite whats there
    new FileInfo(other.FullName).MoveTo(@this, true);
    @this.Refresh();
  }

  /// <summary>
  ///   Initiates a work-in-progress operation on a file, optionally copying its contents to a temporary working file.
  /// </summary>
  /// <param name="this">The source file to start the operation on.</param>
  /// <param name="copyContents">Specifies whether the contents of the source file should be copied to the temporary file.</param>
  /// <returns>An <see cref="IFileInProgress" /> instance for managing the work-in-progress file.</returns>
  /// <example>
  ///   <code>
  /// FileInfo originalFile = new FileInfo("path/to/file.txt");
  /// using (var workInProgress = originalFile.StartWorkInProgress(copyContents: true))
  /// {
  ///     // Perform operations on the work-in-progress file
  ///     workInProgress.WriteAllText("New content");
  ///     
  ///     // Optionally cancel changes
  ///     // workInProgress.CancelChanges = true;
  /// }
  /// // Changes are automatically saved unless canceled.
  /// </code>
  ///   This example demonstrates starting a work-in-progress operation on a file,
  ///   modifying its content, and optionally canceling the changes.
  /// </example>
  public static IFileInProgress StartWorkInProgress(this FileInfo @this, bool copyContents = false) {
    Against.ThisIsNull(@this);
  
    var result = new FileInProgress(@this);
    if (copyContents)
      result.CopyFrom(@this);

    return result;
  }

  /// <summary>
  ///   Determines whether the current file is a text file by checking its content.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo" /> instance representing the file to check.</param>
  /// <returns><see langword="true" /> if the file is a text file; otherwise, <see langword="false" />.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs while opening or reading the file.</exception>
  /// <example>
  ///   <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// bool isTextFile = file.IsTextFile();
  /// Console.WriteLine($"Is text file: {isTextFile}");
  /// </code>
  ///   This example demonstrates how to check if a file is a text file.
  /// </example>
  /// <remarks>
  ///   This method attempts to determine if a file is a text file by reading a portion of its contents and checking for
  ///   non-text characters.
  ///   It is not guaranteed to be foolproof and may yield false positives or negatives for certain types of files.
  /// </remarks>
  public static unsafe bool IsTextFile(this FileInfo @this) {
    Against.ThisIsNull(@this);

    if (@this.NotExists())
      return false;

    const int BUFFER_SIZE = 65536;
    var buffer = stackalloc byte[BUFFER_SIZE];
    int size;
    using (var fileStream = @this.OpenRead())
      size = fileStream.Read(new Span<byte>(buffer, BUFFER_SIZE));

    switch (size) {
      case 0: return false;
      case 1: return !((char)buffer[0]).IsControlButNoWhiteSpace();
      case >= 2 when
        *(ushort*)buffer == 0xfffe // UTF-16 LE
        || *(ushort*)buffer == 0xfeff // UTF-16 BE
        :
        return true;
      case 2:
        return !(
          ((char)buffer[0]).IsControlButNoWhiteSpace()
          || ((char)buffer[1]).IsControlButNoWhiteSpace()
        );
      case >= 3 when
        (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76) // UTF-7
        || (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf) // UTF-8
        :
        return true;
      case 3:
        return !(
          ((char)buffer[0]).IsControlButNoWhiteSpace()
          || ((char)buffer[1]).IsControlButNoWhiteSpace()
          || ((char)buffer[2]).IsControlButNoWhiteSpace()
        );
      case >= 4 when
        *(uint*)buffer == 0xfffe0000 // UTF-32 LE
        || *(uint*)buffer == 0x0000feff // UTF-32 BE
        :
        return true;
      default:
        for (var i = 0; i < size; ++i)
          if (((char)buffer[i]).IsControlButNoWhiteSpace())
            return false;

        return true;
    }
  }
}
