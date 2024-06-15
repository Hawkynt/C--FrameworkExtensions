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

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Collections.Generic;
using System.ComponentModel;
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

    stream.SetLength(stream.Position);
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
      writePosition = stream.Position;

    for (;;) {
      var startOfLine = stream.Position;
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

      readPosition = stream.Position;
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
      var startOfLine = stream.Position;
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

  #region needed consts for converting filename patterns into regexes

  private static readonly Regex _ILEGAL_CHARACTERS_REGEX = new("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
  private static readonly Regex _CATCH_EXTENSION_REGEX = new(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);

  #endregion

  /// <summary>
  ///   Converts a given filename pattern into a regular expression.
  /// </summary>
  /// <param name="pattern">The pattern.</param>
  /// <returns>The regex.</returns>
  private static string _ConvertFilePatternToRegex(string pattern) {
    Against.ArgumentIsNull(pattern);

    pattern = pattern.Trim();

    if (pattern.Length == 0)
      throw new ArgumentException("Pattern is empty.", nameof(pattern));
    
    if (_ILEGAL_CHARACTERS_REGEX.IsMatch(pattern))
      throw new ArgumentException("Patterns contains illegal characters.", nameof(pattern));

    const string nonDotCharacters = "[^.]*";

    var hasExtension = _CATCH_EXTENSION_REGEX.IsMatch(pattern);
    var matchExact = false;

    if (pattern.IndexOf('?') >= 0)
      matchExact = true;
    else if (hasExtension)
      matchExact = _CATCH_EXTENSION_REGEX.Match(pattern).Groups[1].Length != 3;

    var regexString = Regex.Escape(pattern);
    regexString = @"(^|[\\\/])" + Regex.Replace(regexString, @"\\\*", ".*");
    regexString = Regex.Replace(regexString, @"\\\?", ".");

    if (!matchExact && hasExtension)
      regexString += nonDotCharacters;

    regexString += "$";
    return regexString;
  }

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
