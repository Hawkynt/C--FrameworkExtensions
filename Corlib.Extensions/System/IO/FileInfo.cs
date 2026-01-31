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
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

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
    
    // Refresh the target file's info to reflect the new state
    targetFile.Refresh();
  }

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

#else

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
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllLines(@this.FullName, data.ToArray()), token);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding, CancellationToken token = default) => Task.Factory.StartNew(() => File.WriteAllLines(@this.FullName, data.ToArray(), encoding), token);

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

    // Try fast byte-scanning path for ASCII-compatible encodings
    if (_TryFastKeepFirstLines(@this, count, encoding, newLine))
      return;

    // Fall back to character-by-character scanning
    _KeepFirstLinesSlow(@this, count, encoding, newLine);
  }

  private static void _KeepFirstLinesSlow(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using CustomTextReader.Initialized reader =
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

  private static bool _TryFastKeepFirstLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    // For explicit modes (except All), verify we support them before opening the file
    if (newLine != LineBreakMode.AutoDetect && newLine != LineBreakMode.All && !_TryGetLineEndingBytes(newLine, out _))
      return false;

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var fileLength = stream.Length;

    // Detect preamble size and encoding type from BOM
    var preambleSize = 0L;
    Utf16Endianness? utf16Endianness = null;
    var bom = new byte[4];
    var bomRead = stream.Read(bom, 0, 4);

    if (encoding == null) {
      if (bomRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        preambleSize = 3; // UTF-8 BOM
      else if (bomRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) {
        preambleSize = 2; // UTF-16 LE BOM
        utf16Endianness = Utf16Endianness.LittleEndian;
      } else if (bomRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) {
        preambleSize = 2; // UTF-16 BE BOM
        utf16Endianness = Utf16Endianness.BigEndian;
      }
    } else {
      // Check if explicit encoding is UTF-16
      var encodingName = encoding.WebName.ToLowerInvariant();
      if (encodingName.Contains("utf-16") || encodingName.Contains("unicodefffe") || encodingName == "utf-16be")
        utf16Endianness = encodingName.Contains("be") || encodingName == "unicodefffe" ? Utf16Endianness.BigEndian : Utf16Endianness.LittleEndian;
      else if (!_IsAsciiCompatibleEncoding(encoding))
        return false; // Unsupported encoding

      var preamble = encoding.GetPreamble();
      if (preamble.Length > 0 && bomRead >= preamble.Length) {
        var matches = true;
        for (var i = 0; i < preamble.Length && matches; ++i)
          matches = bom[i] == preamble[i];
        if (matches)
          preambleSize = preamble.Length;
      }
    }

    // Handle LineBreakMode.All with multi-pattern byte scanning
    if (newLine == LineBreakMode.All) {
      long truncatePosition;
      if (utf16Endianness.HasValue)
        truncatePosition = _ScanForwardForLineEndAllPatternsUtf16(stream, fileLength, preambleSize, count, utf16Endianness.Value);
      else
        truncatePosition = _ScanForwardForLineEndAllPatterns(stream, fileLength, preambleSize, count);

      if (truncatePosition < 0)
        return true; // File has fewer lines than requested, nothing to do

      stream.SetLength(truncatePosition);
      return true;
    }

    // UTF-16 with non-All mode falls back to slow path (need character-level scanning for specific line endings)
    if (utf16Endianness.HasValue)
      return false;

    // Resolve AutoDetect by scanning file content for line endings
    var actualMode = newLine;
    if (actualMode == LineBreakMode.AutoDetect) {
      actualMode = _DetectLineBreakModeFromStream(stream, preambleSize);
      if (actualMode == LineBreakMode.None || actualMode == LineBreakMode.All)
        return false; // Can't use fast path for mixed line endings
    }

    // Only support simple line endings
    if (!_TryGetLineEndingBytes(actualMode, out var lineEndingBytes))
      return false;

    // Scan forward to find the position after N lines
    var truncatePosition2 = _ScanForwardForLineEnd(stream, fileLength, preambleSize, lineEndingBytes, count);
    if (truncatePosition2 < 0)
      return true; // File has fewer lines than requested, nothing to do

    stream.SetLength(truncatePosition2);
    return true;
  }

  /// <summary>
  ///   Scans forward from the start of file to find the position after N lines.
  /// </summary>
  /// <returns>Position after the Nth line ending, or -1 if file has fewer lines than requested.</returns>
  private static long _ScanForwardForLineEnd(Stream stream, long fileLength, long preambleSize, byte[] lineEnding, int count) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var position = preambleSize;
    var linesFound = 0;

    while (position < fileLength && linesFound < count) {
      stream.Position = position;
      var bytesToRead = (int)Math.Min(bufferSize, fileLength - position);
      var bytesRead = stream.Read(buffer, 0, bytesToRead);
      if (bytesRead <= 0)
        break;

      // Scan the buffer for line endings
      for (var i = 0; i <= bytesRead - lineEnding.Length && linesFound < count; ++i) {
        if (_IsLineEndingAt(buffer, i, bytesRead, lineEnding)) {
          ++linesFound;
          if (linesFound >= count)
            return position + i + lineEnding.Length;

          // Skip past the line ending
          i += lineEnding.Length - 1;
        }
      }

      position += bytesRead;
    }

    // File has fewer lines than requested
    return -1;
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

    // Try fast reverse scan for simple cases (no offset, simple line endings)
    if (offsetInLines == 0 && _TryFastKeepLastLines(@this, count, encoding, newLine))
      return;

    // Fall back to forward scanning
    _KeepLastLinesForward(@this, count, encoding, newLine, offsetInLines);
  }

  private static void _KeepLastLinesForward(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine, int offsetInLines) {
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
  ///   Attempts to keep the last N lines using fast reverse byte scanning.
  ///   Works for simple line endings (CrLf, Lf, Cr) and LineBreakMode.All with ASCII-compatible and UTF-16 encodings.
  /// </summary>
  /// <returns><see langword="true"/> if the operation was performed; otherwise <see langword="false"/> to fall back to forward scanning.</returns>
  private static bool _TryFastKeepLastLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    // For explicit modes (except All), verify we support them before opening the file
    if (newLine != LineBreakMode.AutoDetect && newLine != LineBreakMode.All && !_TryGetLineEndingBytes(newLine, out _))
      return false;

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var fileLength = stream.Length;

    // Detect preamble size and encoding type from BOM
    var preambleSize = 0L;
    Utf16Endianness? utf16Endianness = null;
    var bom = new byte[4];
    var bomRead = stream.Read(bom, 0, 4);

    if (encoding == null) {
      if (bomRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        preambleSize = 3; // UTF-8 BOM
      else if (bomRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) {
        preambleSize = 2; // UTF-16 LE BOM
        utf16Endianness = Utf16Endianness.LittleEndian;
      } else if (bomRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) {
        preambleSize = 2; // UTF-16 BE BOM
        utf16Endianness = Utf16Endianness.BigEndian;
      }
    } else {
      // Check if explicit encoding is UTF-16
      var encodingName = encoding.WebName.ToLowerInvariant();
      if (encodingName.Contains("utf-16") || encodingName.Contains("unicodefffe") || encodingName == "utf-16be")
        utf16Endianness = encodingName.Contains("be") || encodingName == "unicodefffe" ? Utf16Endianness.BigEndian : Utf16Endianness.LittleEndian;
      else if (!_IsAsciiCompatibleEncoding(encoding))
        return false; // Unsupported encoding

      var preamble = encoding.GetPreamble();
      if (preamble.Length > 0 && bomRead >= preamble.Length) {
        var matches = true;
        for (var i = 0; i < preamble.Length && matches; ++i)
          matches = bom[i] == preamble[i];
        if (matches)
          preambleSize = preamble.Length;
      }
    }

    long[] lineStartPositions;

    // Handle LineBreakMode.All with multi-pattern byte scanning
    if (newLine == LineBreakMode.All) {
      if (utf16Endianness.HasValue)
        lineStartPositions = _ScanBackwardsForLineStartsAllPatternsUtf16(stream, fileLength, preambleSize, count, utf16Endianness.Value);
      else
        lineStartPositions = _ScanBackwardsForLineStartsAllPatterns(stream, fileLength, preambleSize, count);

      if (lineStartPositions == null || lineStartPositions.Length == 0)
        return true; // File has fewer lines than requested, nothing to do
    } else {
      // UTF-16 with non-All mode falls back to slow path (need character-level scanning for specific line endings)
      if (utf16Endianness.HasValue)
        return false;
      // Resolve AutoDetect by scanning file content for line endings
      var actualMode = newLine;
      if (actualMode == LineBreakMode.AutoDetect) {
        actualMode = _DetectLineBreakModeFromStream(stream, preambleSize);
        if (actualMode == LineBreakMode.None || actualMode == LineBreakMode.All)
          return false; // Can't use fast path for mixed line endings
      }

      // Only support simple single-byte or two-byte line endings
      if (!_TryGetLineEndingBytes(actualMode, out var lineEndingBytes))
        return false;

      // Scan backwards to find line positions
      lineStartPositions = _ScanBackwardsForLineStarts(stream, fileLength, preambleSize, lineEndingBytes, count);
      if (lineStartPositions == null)
        return false; // File has fewer lines than requested, nothing to do
    }

    var readPosition = lineStartPositions[0];
    var writePosition = preambleSize;

    // Copy data from readPosition to writePosition
    const int bufferSize = 256 * 1024; // Larger buffer for better performance
    var buffer = new byte[bufferSize];

    while (readPosition < fileLength) {
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
    return true;
  }

  /// <summary>
  ///   Detects line break mode by scanning the stream for CR/LF bytes.
  /// </summary>
  private static LineBreakMode _DetectLineBreakModeFromStream(Stream stream, long startPosition) {
    const int scanBufferSize = 64 * 1024;
    var buffer = new byte[scanBufferSize];

    stream.Position = startPosition;
    var bytesRead = stream.Read(buffer, 0, scanBufferSize);
    if (bytesRead <= 0)
      return LineBreakMode.None;

    const byte CR = (byte)'\r';
    const byte LF = (byte)'\n';

    for (var i = 0; i < bytesRead; ++i) {
      var b = buffer[i];
      if (b == CR) {
        if (i + 1 < bytesRead && buffer[i + 1] == LF)
          return LineBreakMode.CrLf;
        return LineBreakMode.CarriageReturn;
      }

      if (b == LF) {
        if (i + 1 < bytesRead && buffer[i + 1] == CR)
          return LineBreakMode.LfCr;
        return LineBreakMode.LineFeed;
      }
    }

    return LineBreakMode.None;
  }

  /// <summary>
  ///   Scans backwards from end of file to find the starting positions of the last N lines.
  /// </summary>
  /// <returns>Array of line start positions (sorted ascending), or null if file has fewer lines than requested.</returns>
  private static long[] _ScanBackwardsForLineStarts(Stream stream, long fileLength, long preambleSize, byte[] lineEnding, int count) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var lineStarts = new List<long>(count + 1);

    var position = fileLength;
    var linesFound = 0;

    while (position > preambleSize && linesFound < count) {
      var readStart = Math.Max(preambleSize, position - bufferSize);
      var readLength = (int)(position - readStart);

      stream.Position = readStart;
      var bytesRead = stream.Read(buffer, 0, readLength);
      if (bytesRead <= 0)
        break;

      // Scan the buffer backwards
      for (var i = bytesRead - 1; i >= 0 && linesFound < count; --i) {
        if (_IsLineEndingAt(buffer, i, bytesRead, lineEnding)) {
          // Position after the line ending is the start of the next line
          var lineStartPos = readStart + i + lineEnding.Length;
          // Only count if there's content after this line ending (skip trailing newlines)
          if (lineStartPos < fileLength) {
            lineStarts.Add(lineStartPos);
            ++linesFound;
          }

          // Skip past the line ending bytes we just found
          i -= lineEnding.Length - 1;
        }
      }

      position = readStart;
    }

    // If we're at the beginning of file content, that's also a line start
    if (linesFound < count && position <= preambleSize) {
      lineStarts.Add(preambleSize);
      ++linesFound;
    }

    // If we didn't find enough lines, file has fewer lines than requested
    if (linesFound < count)
      return null;

    // Return positions sorted ascending
    lineStarts.Reverse();
    return lineStarts.ToArray();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool _IsLineEndingAt(byte[] buffer, int index, int bufferLength, byte[] lineEnding) {
    if (index + lineEnding.Length > bufferLength)
      return false;

    for (var j = 0; j < lineEnding.Length; ++j)
      if (buffer[index + j] != lineEnding[j])
        return false;

    return true;
  }

  private static bool _TryGetLineEndingBytes(LineBreakMode mode, out byte[] bytes) {
    switch (mode) {
      case LineBreakMode.CrLf:
        bytes = [(byte)'\r', (byte)'\n'];
        return true;
      case LineBreakMode.LineFeed:
        bytes = [(byte)'\n'];
        return true;
      case LineBreakMode.CarriageReturn:
        bytes = [(byte)'\r'];
        return true;
      case LineBreakMode.LfCr:
        bytes = [(byte)'\n', (byte)'\r'];
        return true;
      default:
        bytes = null;
        return false;
    }
  }

  private static bool _IsAsciiCompatibleEncoding(Encoding encoding) {
    // UTF-8 and most single-byte encodings are ASCII-compatible for bytes < 128
    var name = encoding.WebName.ToLowerInvariant();
    return name.StartsWith("utf-8")
           || name.StartsWith("iso-8859")
           || name.StartsWith("windows-")
           || name == "us-ascii"
           || name == "ascii";
  }

  // Line ending byte patterns for ASCII-compatible encodings (UTF-8, ISO-8859-*, etc.)
  // Ordered by length descending for greedy matching (longer patterns first)
  private static readonly byte[][] _AllLineEndingPatternsAscii = [
    [0xE2, 0x80, 0xA9], // Paragraph Separator (U+2029) - UTF-8
    [0xE2, 0x80, 0xA8], // Line Separator (U+2028) - UTF-8
    [0xC2, 0x85],       // Next Line (NEL, U+0085) - UTF-8
    [0x0D, 0x0A],       // CRLF
    [0x0A, 0x0D],       // LFCR (rare)
    [0x0A],             // LF
    [0x0D],             // CR
    [0x0C],             // Form Feed
    [0x0B],             // Vertical Tab
    [0x1E],             // Record Separator
    [0x15],             // Negative Acknowledge (EBCDIC newline)
    [0x00],             // NUL
  ];

  // Quick lookup: bytes that could start a line ending pattern
  private static readonly bool[] _LineEndingStartBytesAscii = _BuildStartByteLookup();

  // Quick lookup: bytes that could end a line ending pattern (for backward scanning)
  private static readonly bool[] _LineEndingEndBytesAscii = _BuildEndByteLookup();

  private static bool[] _BuildStartByteLookup() {
    var lookup = new bool[256];
    foreach (var pattern in _AllLineEndingPatternsAscii)
      lookup[pattern[0]] = true;
    return lookup;
  }

  private static bool[] _BuildEndByteLookup() {
    var lookup = new bool[256];
    foreach (var pattern in _AllLineEndingPatternsAscii)
      lookup[pattern[^1]] = true;
    return lookup;
  }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

  // Byte arrays for SIMD-optimized IndexOfAny lookups (ASCII/UTF-8)
  private static readonly byte[] _LineEndingStartBytesForIndexOfAnyAscii = _BuildBytesForIndexOfAny(_AllLineEndingPatternsAscii, p => p[0]);
  private static readonly byte[] _LineEndingEndBytesForIndexOfAnyAscii = _BuildBytesForIndexOfAny(_AllLineEndingPatternsAscii, p => p[^1]);

  private static byte[] _BuildBytesForIndexOfAny(byte[][] patterns, Func<byte[], byte> selector) {
    var uniqueBytes = new HashSet<byte>();
    foreach (var pattern in patterns)
      uniqueBytes.Add(selector(pattern));
    var result = new byte[uniqueBytes.Count];
    uniqueBytes.CopyTo(result);
    return result;
  }

#endif

  // UTF-16 encoding types for byte-level scanning
  private enum Utf16Endianness {
    LittleEndian, // BOM: FF FE
    BigEndian     // BOM: FE FF
  }

  // Line ending byte patterns for UTF-16 Little Endian (each character is 2 bytes, low byte first)
  // Ordered by length descending for greedy matching
  private static readonly byte[][] _AllLineEndingPatternsUtf16LE = [
    [0x29, 0x20], // Paragraph Separator (U+2029) - UTF-16 LE
    [0x28, 0x20], // Line Separator (U+2028) - UTF-16 LE
    [0x85, 0x00], // Next Line (NEL, U+0085) - UTF-16 LE
    [0x0D, 0x00, 0x0A, 0x00], // CRLF - UTF-16 LE
    [0x0A, 0x00, 0x0D, 0x00], // LFCR - UTF-16 LE
    [0x0A, 0x00], // LF - UTF-16 LE
    [0x0D, 0x00], // CR - UTF-16 LE
    [0x0C, 0x00], // Form Feed - UTF-16 LE
    [0x0B, 0x00], // Vertical Tab - UTF-16 LE
    [0x1E, 0x00], // Record Separator - UTF-16 LE
    [0x15, 0x00], // Negative Acknowledge - UTF-16 LE
    [0x00, 0x00], // NUL - UTF-16 LE
  ];

  // Line ending byte patterns for UTF-16 Big Endian (each character is 2 bytes, high byte first)
  // Ordered by length descending for greedy matching
  private static readonly byte[][] _AllLineEndingPatternsUtf16BE = [
    [0x20, 0x29], // Paragraph Separator (U+2029) - UTF-16 BE
    [0x20, 0x28], // Line Separator (U+2028) - UTF-16 BE
    [0x00, 0x85], // Next Line (NEL, U+0085) - UTF-16 BE
    [0x00, 0x0D, 0x00, 0x0A], // CRLF - UTF-16 BE
    [0x00, 0x0A, 0x00, 0x0D], // LFCR - UTF-16 BE
    [0x00, 0x0A], // LF - UTF-16 BE
    [0x00, 0x0D], // CR - UTF-16 BE
    [0x00, 0x0C], // Form Feed - UTF-16 BE
    [0x00, 0x0B], // Vertical Tab - UTF-16 BE
    [0x00, 0x1E], // Record Separator - UTF-16 BE
    [0x00, 0x15], // Negative Acknowledge - UTF-16 BE
    [0x00, 0x00], // NUL - UTF-16 BE
  ];

  // Quick lookups for UTF-16 LE pattern start/end bytes
  private static readonly bool[] _LineEndingStartBytesUtf16LE = _BuildLookupForPatterns(_AllLineEndingPatternsUtf16LE, p => p[0]);
  private static readonly bool[] _LineEndingEndBytesUtf16LE = _BuildLookupForPatterns(_AllLineEndingPatternsUtf16LE, p => p[^1]);

  // Quick lookups for UTF-16 BE pattern start/end bytes
  private static readonly bool[] _LineEndingStartBytesUtf16BE = _BuildLookupForPatterns(_AllLineEndingPatternsUtf16BE, p => p[0]);
  private static readonly bool[] _LineEndingEndBytesUtf16BE = _BuildLookupForPatterns(_AllLineEndingPatternsUtf16BE, p => p[^1]);

  private static bool[] _BuildLookupForPatterns(byte[][] patterns, Func<byte[], byte> selector) {
    var lookup = new bool[256];
    foreach (var pattern in patterns)
      lookup[selector(pattern)] = true;
    return lookup;
  }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER

  // Byte arrays for SIMD-optimized IndexOfAny lookups (UTF-16 LE)
  private static readonly byte[] _LineEndingStartBytesForIndexOfAnyUtf16LE = _BuildBytesForIndexOfAny(_AllLineEndingPatternsUtf16LE, p => p[0]);
  private static readonly byte[] _LineEndingEndBytesForIndexOfAnyUtf16LE = _BuildBytesForIndexOfAny(_AllLineEndingPatternsUtf16LE, p => p[^1]);

  // Byte arrays for SIMD-optimized IndexOfAny lookups (UTF-16 BE)
  private static readonly byte[] _LineEndingStartBytesForIndexOfAnyUtf16BE = _BuildBytesForIndexOfAny(_AllLineEndingPatternsUtf16BE, p => p[0]);
  private static readonly byte[] _LineEndingEndBytesForIndexOfAnyUtf16BE = _BuildBytesForIndexOfAny(_AllLineEndingPatternsUtf16BE, p => p[^1]);

#endif

  /// <summary>
  /// Tries to match any UTF-16 line ending pattern at the given position in the buffer.
  /// Returns the length of the matched pattern, or 0 if no match.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TryMatchAnyLineEndingUtf16(byte[] buffer, int index, int bufferLength, Utf16Endianness endianness) {
    var patterns = endianness == Utf16Endianness.LittleEndian ? _AllLineEndingPatternsUtf16LE : _AllLineEndingPatternsUtf16BE;
    var startLookup = endianness == Utf16Endianness.LittleEndian ? _LineEndingStartBytesUtf16LE : _LineEndingStartBytesUtf16BE;

    var firstByte = buffer[index];

    // Quick reject: not a potential line ending start byte
    if (!startLookup[firstByte])
      return 0;

    // Try patterns in order (longest first for greedy matching)
    foreach (var pattern in patterns) {
      if (pattern[0] != firstByte)
        continue;

      if (index + pattern.Length > bufferLength)
        continue;

      var match = true;
      for (var j = 1; j < pattern.Length; ++j)
        if (buffer[index + j] != pattern[j]) {
          match = false;
          break;
        }

      if (match)
        return pattern.Length;
    }

    return 0;
  }

  /// <summary>
  /// Tries to match any UTF-16 line ending pattern that ENDS at the given position in the buffer.
  /// Used for backward scanning.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TryMatchAnyLineEndingEndingAtUtf16(byte[] buffer, int endIndex, Utf16Endianness endianness) {
    var patterns = endianness == Utf16Endianness.LittleEndian ? _AllLineEndingPatternsUtf16LE : _AllLineEndingPatternsUtf16BE;
    var endLookup = endianness == Utf16Endianness.LittleEndian ? _LineEndingEndBytesUtf16LE : _LineEndingEndBytesUtf16BE;

    var lastByte = buffer[endIndex];

    // Quick reject: not a potential line ending end byte
    if (!endLookup[lastByte])
      return 0;

    // Try patterns in order (longest first for greedy matching)
    foreach (var pattern in patterns) {
      if (pattern[^1] != lastByte)
        continue;

      var startIndex = endIndex - pattern.Length + 1;
      if (startIndex < 0)
        continue;

      var match = true;
      for (var j = 0; j < pattern.Length - 1; ++j)
        if (buffer[startIndex + j] != pattern[j]) {
          match = false;
          break;
        }

      if (match)
        return pattern.Length;
    }

    return 0;
  }

  /// <summary>
  /// Scans forward for line endings in UTF-16 encoded files.
  /// Uses SIMD-optimized IndexOfAny on .NET Core 2.1+ / .NET Standard 2.1+ for better performance.
  /// </summary>
  private static long _ScanForwardForLineEndAllPatternsUtf16(Stream stream, long fileLength, long preambleSize, int count, Utf16Endianness endianness) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var position = preambleSize;
    var linesFound = 0;

    while (position < fileLength && linesFound < count) {
      stream.Position = position;
      var bytesToRead = (int)Math.Min(bufferSize, fileLength - position);
      var bytesRead = stream.Read(buffer, 0, bytesToRead);
      if (bytesRead <= 0)
        break;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
      // SIMD-optimized path: use IndexOfAny to find candidate positions
      var span = buffer.AsSpan(0, bytesRead);
      var searchBytes = endianness == Utf16Endianness.LittleEndian
        ? _LineEndingStartBytesForIndexOfAnyUtf16LE.AsSpan()
        : _LineEndingStartBytesForIndexOfAnyUtf16BE.AsSpan();
      var offset = 0;

      while (offset < bytesRead - 1 && linesFound < count) {
        var remaining = span.Slice(offset);
        var candidateIndex = remaining.IndexOfAny(searchBytes);
        if (candidateIndex < 0)
          break; // No more candidates in this buffer

        var i = offset + candidateIndex;
        // Ensure 2-byte alignment (UTF-16 characters start at even positions)
        if (i % 2 != 0) {
          offset = i + 1;
          continue;
        }

        var matchLength = _TryMatchAnyLineEndingUtf16(buffer, i, bytesRead, endianness);
        if (matchLength > 0) {
          ++linesFound;
          if (linesFound >= count)
            return position + i + matchLength;

          offset = i + matchLength;
        } else
          offset = i + 2; // Move to next aligned position
      }
#else
      // Scan the buffer for any line ending (must be 2-byte aligned for UTF-16)
      for (var i = 0; i < bytesRead - 1 && linesFound < count; i += 2) {
        var matchLength = _TryMatchAnyLineEndingUtf16(buffer, i, bytesRead, endianness);
        if (matchLength > 0) {
          ++linesFound;
          if (linesFound >= count)
            return position + i + matchLength;

          // Skip past the line ending (matchLength is already multiple of 2)
          i += matchLength - 2;
        }
      }
#endif

      position += bytesRead;
    }

    return -1; // File has fewer lines than requested
  }

  /// <summary>
  /// Scans backward for line starts in UTF-16 encoded files.
  /// Uses SIMD-optimized LastIndexOfAny on .NET Core 2.1+ / .NET Standard 2.1+ for better performance.
  /// </summary>
  private static long[] _ScanBackwardsForLineStartsAllPatternsUtf16(Stream stream, long fileLength, long preambleSize, int count, Utf16Endianness endianness) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var lineStarts = new List<long>(count + 1);
    var position = fileLength;
    var linesFound = 0;

    while (position > preambleSize && linesFound < count) {
      var readStart = Math.Max(preambleSize, position - bufferSize);
      var bytesToRead = (int)(position - readStart);

      stream.Position = readStart;
      var bytesRead = stream.Read(buffer, 0, bytesToRead);
      if (bytesRead <= 0)
        break;

      // Calculate alignment offset from preamble
      var offsetFromPreamble = (int)((readStart - preambleSize) % 2);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
      // SIMD-optimized path: use LastIndexOfAny to find candidate positions
      var span = buffer.AsSpan(0, bytesRead);
      var searchBytes = endianness == Utf16Endianness.LittleEndian
        ? _LineEndingEndBytesForIndexOfAnyUtf16LE.AsSpan()
        : _LineEndingEndBytesForIndexOfAnyUtf16BE.AsSpan();
      var searchEnd = bytesRead;

      while (searchEnd > 1 && linesFound < count) {
        var searchSpan = span.Slice(0, searchEnd);
        var candidateIndex = searchSpan.LastIndexOfAny(searchBytes);
        if (candidateIndex < 1)
          break; // No more candidates in this buffer (need at least index 1 for UTF-16)

        // Ensure proper alignment (last byte of UTF-16 char is at odd position relative to preamble)
        if ((candidateIndex + offsetFromPreamble) % 2 == 0) {
          searchEnd = candidateIndex;
          continue;
        }

        // Check if any line ending pattern ENDS at this position
        var matchLength = _TryMatchAnyLineEndingEndingAtUtf16(buffer, candidateIndex, endianness);
        if (matchLength > 0) {
          // Line ending ends at position candidateIndex, so next line starts at candidateIndex + 1
          var lineStartPos = readStart + candidateIndex + 1;
          // Only count if there's content after this line ending
          if (lineStartPos < fileLength) {
            lineStarts.Add(lineStartPos);
            ++linesFound;
          }

          // Move search end to before the start of this pattern
          searchEnd = candidateIndex - matchLength + 1;
        } else
          searchEnd = candidateIndex; // False positive, move past this byte
      }
#else
      // Scan backwards through buffer, looking for bytes that could END a line ending
      // Must maintain 2-byte alignment for UTF-16
      var startIdx = bytesRead - 1;
      // Ensure we start on an odd index (last byte of a 2-byte char) relative to preamble
      if ((startIdx + offsetFromPreamble) % 2 == 0)
        --startIdx;

      for (var i = startIdx; i >= 1 && linesFound < count; i -= 2) {
        // Check if any line ending pattern ENDS at this position
        var matchLength = _TryMatchAnyLineEndingEndingAtUtf16(buffer, i, endianness);
        if (matchLength > 0) {
          // Line ending ends at position i, so next line starts at i + 1
          var lineStartPos = readStart + i + 1;
          // Only count if there's content after this line ending
          if (lineStartPos < fileLength) {
            lineStarts.Add(lineStartPos);
            ++linesFound;
          }

          // Skip past the rest of the multi-byte pattern when scanning backwards
          i -= matchLength - 2;
        }
      }
#endif

      position = readStart;
    }

    // If we're at the beginning of file content, that's also a line start
    if (linesFound < count && position <= preambleSize) {
      lineStarts.Add(preambleSize);
      ++linesFound;
    }

    // If we didn't find enough lines, return null
    if (linesFound < count)
      return null;

    // Return positions sorted ascending
    lineStarts.Reverse();
    return lineStarts.ToArray();
  }

  /// <summary>
  /// Tries to match any line ending pattern at the given position in the buffer.
  /// Returns the length of the matched pattern, or 0 if no match.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TryMatchAnyLineEnding(byte[] buffer, int index, int bufferLength) {
    var firstByte = buffer[index];

    // Quick reject: not a potential line ending start byte
    if (!_LineEndingStartBytesAscii[firstByte])
      return 0;

    // Try patterns in order (longest first for greedy matching)
    foreach (var pattern in _AllLineEndingPatternsAscii) {
      if (pattern[0] != firstByte)
        continue;

      if (index + pattern.Length > bufferLength)
        continue;

      var match = true;
      for (var j = 1; j < pattern.Length; ++j)
        if (buffer[index + j] != pattern[j]) {
          match = false;
          break;
        }

      if (match)
        return pattern.Length;
    }

    return 0;
  }

  /// <summary>
  /// Tries to match any line ending pattern that ENDS at the given position in the buffer.
  /// Returns the length of the matched pattern, or 0 if no match.
  /// Used for backward scanning where we encounter the last byte of a pattern first.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _TryMatchAnyLineEndingEndingAt(byte[] buffer, int endIndex) {
    var lastByte = buffer[endIndex];

    // Quick reject: not a potential line ending end byte
    if (!_LineEndingEndBytesAscii[lastByte])
      return 0;

    // Try patterns in order (longest first for greedy matching)
    foreach (var pattern in _AllLineEndingPatternsAscii) {
      if (pattern[^1] != lastByte)
        continue;

      var startIndex = endIndex - pattern.Length + 1;
      if (startIndex < 0)
        continue;

      var match = true;
      for (var j = 0; j < pattern.Length - 1; ++j)
        if (buffer[startIndex + j] != pattern[j]) {
          match = false;
          break;
        }

      if (match)
        return pattern.Length;
    }

    return 0;
  }

  /// <summary>
  /// Scans forward from the start of file to find the position after N lines, supporting all line ending types.
  /// Uses SIMD-optimized IndexOfAny on .NET Core 2.1+ / .NET Standard 2.1+ for better performance.
  /// </summary>
  private static long _ScanForwardForLineEndAllPatterns(Stream stream, long fileLength, long preambleSize, int count) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var position = preambleSize;
    var linesFound = 0;

    while (position < fileLength && linesFound < count) {
      stream.Position = position;
      var bytesToRead = (int)Math.Min(bufferSize, fileLength - position);
      var bytesRead = stream.Read(buffer, 0, bytesToRead);
      if (bytesRead <= 0)
        break;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
      // SIMD-optimized path: use IndexOfAny to find candidate positions
      var span = buffer.AsSpan(0, bytesRead);
      var searchBytes = _LineEndingStartBytesForIndexOfAnyAscii.AsSpan();
      var offset = 0;

      while (offset < bytesRead && linesFound < count) {
        var remaining = span.Slice(offset);
        var candidateIndex = remaining.IndexOfAny(searchBytes);
        if (candidateIndex < 0)
          break; // No more candidates in this buffer

        var i = offset + candidateIndex;
        var matchLength = _TryMatchAnyLineEnding(buffer, i, bytesRead);
        if (matchLength > 0) {
          ++linesFound;
          if (linesFound >= count)
            return position + i + matchLength;

          offset = i + matchLength;
        } else
          ++offset; // False positive, move past this byte
      }
#else
      // Traditional byte-by-byte scanning
      for (var i = 0; i < bytesRead && linesFound < count; ++i) {
        var matchLength = _TryMatchAnyLineEnding(buffer, i, bytesRead);
        if (matchLength > 0) {
          ++linesFound;
          if (linesFound >= count)
            return position + i + matchLength;

          // Skip past the line ending
          i += matchLength - 1;
        }
      }
#endif

      position += bytesRead;
    }

    return -1; // File has fewer lines than requested
  }

  /// <summary>
  /// Scans backward from end of file to find line start positions, supporting all line ending types.
  /// Uses SIMD-optimized LastIndexOfAny on .NET Core 2.1+ / .NET Standard 2.1+ for better performance.
  /// Note: Backward scanning may give different line boundaries than forward scanning for overlapping
  /// patterns like CRLF/LFCR, but this is acceptable for backward operations.
  /// </summary>
  private static long[] _ScanBackwardsForLineStartsAllPatterns(Stream stream, long fileLength, long preambleSize, int count) {
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];
    var lineStarts = new List<long>(count + 1);
    var position = fileLength;
    var linesFound = 0;

    while (position > preambleSize && linesFound < count) {
      var readStart = Math.Max(preambleSize, position - bufferSize);
      var bytesToRead = (int)(position - readStart);

      stream.Position = readStart;
      var bytesRead = stream.Read(buffer, 0, bytesToRead);
      if (bytesRead <= 0)
        break;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
      // SIMD-optimized path: use LastIndexOfAny to find candidate positions
      var span = buffer.AsSpan(0, bytesRead);
      var searchBytes = _LineEndingEndBytesForIndexOfAnyAscii.AsSpan();
      var searchEnd = bytesRead;

      while (searchEnd > 0 && linesFound < count) {
        var searchSpan = span.Slice(0, searchEnd);
        var candidateIndex = searchSpan.LastIndexOfAny(searchBytes);
        if (candidateIndex < 0)
          break; // No more candidates in this buffer

        // Check if any line ending pattern ENDS at this position
        var matchLength = _TryMatchAnyLineEndingEndingAt(buffer, candidateIndex);
        if (matchLength > 0) {
          // Line ending ends at position candidateIndex, so next line starts at candidateIndex + 1
          var lineStartPos = readStart + candidateIndex + 1;
          // Only count if there's content after this line ending
          if (lineStartPos < fileLength) {
            lineStarts.Add(lineStartPos);
            ++linesFound;
          }

          // Move search end to before the start of this pattern
          searchEnd = candidateIndex - matchLength + 1;
        } else
          searchEnd = candidateIndex; // False positive, move past this byte
      }
#else
      // Scan backwards through buffer, looking for bytes that could END a line ending
      for (var i = bytesRead - 1; i >= 0 && linesFound < count; --i) {
        // Check if any line ending pattern ENDS at this position
        var matchLength = _TryMatchAnyLineEndingEndingAt(buffer, i);
        if (matchLength > 0) {
          // Line ending ends at position i, so next line starts at i + 1
          var lineStartPos = readStart + i + 1;
          // Only count if there's content after this line ending
          if (lineStartPos < fileLength) {
            lineStarts.Add(lineStartPos);
            ++linesFound;
          }

          // Skip past the rest of the multi-byte pattern when scanning backwards
          i -= matchLength - 1;
        }
      }
#endif

      position = readStart;
    }

    // If we're at the beginning of file content, that's also a line start
    if (linesFound < count && position <= preambleSize) {
      lineStarts.Add(preambleSize);
      ++linesFound;
    }

    // If we didn't find enough lines, return null
    if (linesFound < count)
      return null;

    // Return positions sorted ascending
    lineStarts.Reverse();
    return lineStarts.ToArray();
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

    // Try fast byte-scanning path for ASCII-compatible encodings
    if (_TryFastRemoveFirstLines(@this, count, encoding, newLine))
      return;

    // Fall back to character-by-character scanning
    _RemoveFirstLinesSlow(@this, count, encoding, newLine);
  }

  private static void _RemoveFirstLinesSlow(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);

    var readPosition = 0L;
    var preambleSize = 0L;

    // Use using to ensure reader is properly disposed and stream position is synced
    using (CustomTextReader.Initialized reader = encoding == null
          ? new(stream, true, newLine)
          : new(stream, encoding, newLine)) {

      var lineCounter = 0;

      while (lineCounter < count) {
        var line = reader.ReadLine();
        if (line == null)
          break;

        readPosition = reader.Position;
        ++lineCounter;
      }

      preambleSize = reader.PreambleSize;

      if (lineCounter < count) {
        stream.SetLength(preambleSize);
        return;
      }
    }

    // After disposing reader, the underlying stream position is now synced to readPosition
    // No need for additional Seek

    var writePosition = preambleSize;

    const int bufferSize = 64 * 1024;
    var buffer = new byte[bufferSize];

    for (;;) {
      stream.Seek(readPosition, SeekOrigin.Begin);
      var bytesRead = stream.Read(buffer, 0, bufferSize);
      if (bytesRead <= 0)
        break;

      stream.Seek(writePosition, SeekOrigin.Begin);
      stream.Write(buffer, 0, bytesRead);

      readPosition += bytesRead;
      writePosition += bytesRead;
    }

    stream.SetLength(writePosition);
  }

  private static bool _TryFastRemoveFirstLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    // For explicit modes (except All), verify we support them before opening the file
    if (newLine != LineBreakMode.AutoDetect && newLine != LineBreakMode.All && !_TryGetLineEndingBytes(newLine, out _))
      return false;

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var fileLength = stream.Length;

    // Detect preamble size and encoding type from BOM
    var preambleSize = 0L;
    Utf16Endianness? utf16Endianness = null;
    var bom = new byte[4];
    var bomRead = stream.Read(bom, 0, 4);

    if (encoding == null) {
      if (bomRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        preambleSize = 3; // UTF-8 BOM
      else if (bomRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) {
        preambleSize = 2; // UTF-16 LE BOM
        utf16Endianness = Utf16Endianness.LittleEndian;
      } else if (bomRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) {
        preambleSize = 2; // UTF-16 BE BOM
        utf16Endianness = Utf16Endianness.BigEndian;
      }
    } else {
      // Check if explicit encoding is UTF-16
      var encodingName = encoding.WebName.ToLowerInvariant();
      if (encodingName.Contains("utf-16") || encodingName.Contains("unicodefffe") || encodingName == "utf-16be")
        utf16Endianness = encodingName.Contains("be") || encodingName == "unicodefffe" ? Utf16Endianness.BigEndian : Utf16Endianness.LittleEndian;
      else if (!_IsAsciiCompatibleEncoding(encoding))
        return false; // Unsupported encoding

      var preamble = encoding.GetPreamble();
      if (preamble.Length > 0 && bomRead >= preamble.Length) {
        var matches = true;
        for (var i = 0; i < preamble.Length && matches; ++i)
          matches = bom[i] == preamble[i];
        if (matches)
          preambleSize = preamble.Length;
      }
    }

    long readPosition;

    // Handle LineBreakMode.All with multi-pattern byte scanning
    if (newLine == LineBreakMode.All) {
      if (utf16Endianness.HasValue)
        readPosition = _ScanForwardForLineEndAllPatternsUtf16(stream, fileLength, preambleSize, count, utf16Endianness.Value);
      else
        readPosition = _ScanForwardForLineEndAllPatterns(stream, fileLength, preambleSize, count);
    } else {
      // UTF-16 with non-All mode falls back to slow path
      if (utf16Endianness.HasValue)
        return false;
      // Resolve AutoDetect by scanning file content for line endings
      var actualMode = newLine;
      if (actualMode == LineBreakMode.AutoDetect) {
        actualMode = _DetectLineBreakModeFromStream(stream, preambleSize);
        if (actualMode == LineBreakMode.None || actualMode == LineBreakMode.All)
          return false; // Can't use fast path for mixed line endings
      }

      // Only support simple line endings
      if (!_TryGetLineEndingBytes(actualMode, out var lineEndingBytes))
        return false;

      // Scan forward to find the position after N lines
      readPosition = _ScanForwardForLineEnd(stream, fileLength, preambleSize, lineEndingBytes, count);
    }
    if (readPosition < 0) {
      // File has fewer lines than requested, truncate to just the preamble
      stream.SetLength(preambleSize);
      return true;
    }

    // Copy remaining data from readPosition to preambleSize
    var writePosition = preambleSize;
    const int bufferSize = 256 * 1024;
    var buffer = new byte[bufferSize];

    while (readPosition < fileLength) {
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
    return true;
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

    // Try fast reverse scan for simple cases
    if (_TryFastRemoveLastLines(@this, count, encoding, newLine))
      return;

    // Fall back to forward scanning
    _RemoveLastLinesForward(@this, count, encoding, newLine);
  }

  private static void _RemoveLastLinesForward(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
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

  /// <summary>
  ///   Attempts to remove the last N lines using fast reverse byte scanning.
  ///   Works for simple line endings (CrLf, Lf, Cr) and LineBreakMode.All with ASCII-compatible and UTF-16 encodings.
  /// </summary>
  /// <returns><see langword="true"/> if the operation was performed; otherwise <see langword="false"/> to fall back to forward scanning.</returns>
  private static bool _TryFastRemoveLastLines(FileInfo @this, int count, Encoding encoding, LineBreakMode newLine) {
    // For explicit modes (except All), verify we support them before opening the file
    if (newLine != LineBreakMode.AutoDetect && newLine != LineBreakMode.All && !_TryGetLineEndingBytes(newLine, out _))
      return false;

    using var stream = @this.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    var fileLength = stream.Length;

    // Detect preamble size and encoding type from BOM
    var preambleSize = 0L;
    Utf16Endianness? utf16Endianness = null;
    var bom = new byte[4];
    var bomRead = stream.Read(bom, 0, 4);

    if (encoding == null) {
      if (bomRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        preambleSize = 3; // UTF-8 BOM
      else if (bomRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) {
        preambleSize = 2; // UTF-16 LE BOM
        utf16Endianness = Utf16Endianness.LittleEndian;
      } else if (bomRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) {
        preambleSize = 2; // UTF-16 BE BOM
        utf16Endianness = Utf16Endianness.BigEndian;
      }
    } else {
      // Check if explicit encoding is UTF-16
      var encodingName = encoding.WebName.ToLowerInvariant();
      if (encodingName.Contains("utf-16") || encodingName.Contains("unicodefffe") || encodingName == "utf-16be")
        utf16Endianness = encodingName.Contains("be") || encodingName == "unicodefffe" ? Utf16Endianness.BigEndian : Utf16Endianness.LittleEndian;
      else if (!_IsAsciiCompatibleEncoding(encoding))
        return false; // Unsupported encoding

      var preamble = encoding.GetPreamble();
      if (preamble.Length > 0 && bomRead >= preamble.Length) {
        var matches = true;
        for (var i = 0; i < preamble.Length && matches; ++i)
          matches = bom[i] == preamble[i];
        if (matches)
          preambleSize = preamble.Length;
      }
    }

    long[] lineStartPositions;

    // Handle LineBreakMode.All with multi-pattern byte scanning
    if (newLine == LineBreakMode.All) {
      if (utf16Endianness.HasValue)
        lineStartPositions = _ScanBackwardsForLineStartsAllPatternsUtf16(stream, fileLength, preambleSize, count, utf16Endianness.Value);
      else
        lineStartPositions = _ScanBackwardsForLineStartsAllPatterns(stream, fileLength, preambleSize, count);

      if (lineStartPositions == null || lineStartPositions.Length == 0) {
        // File has fewer lines than requested - truncate to just preamble
        stream.SetLength(preambleSize);
        return true;
      }
    } else {
      // UTF-16 with non-All mode falls back to slow path
      if (utf16Endianness.HasValue)
        return false;
      // Resolve AutoDetect by scanning file content for line endings
      var actualMode = newLine;
      if (actualMode == LineBreakMode.AutoDetect) {
        actualMode = _DetectLineBreakModeFromStream(stream, preambleSize);
        if (actualMode == LineBreakMode.None || actualMode == LineBreakMode.All)
          return false; // Can't use fast path for mixed line endings
      }

      // Only support simple single-byte or two-byte line endings
      if (!_TryGetLineEndingBytes(actualMode, out var lineEndingBytes))
        return false;

      // Scan backwards to find the start of the lines to remove
      lineStartPositions = _ScanBackwardsForLineStarts(stream, fileLength, preambleSize, lineEndingBytes, count);
      if (lineStartPositions == null) {
        // File has fewer lines than requested - truncate to just preamble
        stream.SetLength(preambleSize);
        return true;
      }
    }

    // Truncate at the start of the first line we want to remove
    stream.SetLength(lineStartPositions[0]);
    return true;
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
  public static void Touch(this FileInfo @this) {
    if (!File.Exists(@this.FullName))
      using (@this.Create()) { }
    
    if ((@this.Attributes & FileAttributes.ReadOnly) != 0)
      throw new IOException("File is read-only");

    @this.LastWriteTimeUtc = DateTime.UtcNow;
    @this.Refresh();
  }

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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    var buffer = stackalloc byte[BUFFER_SIZE];
    int size;
    using (var fileStream = @this.OpenRead())
      size = fileStream.Read(new Span<byte>(buffer, BUFFER_SIZE));

    return _IsTextFileCore(buffer, size);
  }
#else
    var byteArray = new byte[BUFFER_SIZE];
    int size;
    using (var fileStream = @this.OpenRead())
      size = fileStream.Read(byteArray, 0, BUFFER_SIZE);

    fixed (byte* buffer = byteArray)
      return _IsTextFileCore(buffer, size);
  }
#endif

  private static unsafe bool _IsTextFileCore(byte* buffer, int size) {
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
