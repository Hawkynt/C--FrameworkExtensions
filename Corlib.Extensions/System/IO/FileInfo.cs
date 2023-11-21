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
using System.ComponentModel;
using System.Diagnostics;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
#if !SUPPORTS_ENUMERATING_IO
using System.Linq;
#endif
#if !NETCOREAPP3_1_OR_GREATER && !NETSTANDARD
using System.Security.AccessControl;
#endif
#if SUPPORTS_STREAM_ASYNC
using System.Threading.Tasks;
#endif

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace System.IO;

using Guard;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class FileInfoExtensions {

  /// <summary>
  /// The native methods.
  /// </summary>
  private static class NativeMethods {

#pragma warning disable CC0021 // Use nameof
#pragma warning disable CC0074 // Make field readonly
    // ReSharper disable UnusedMember.Local
    // ReSharper disable MemberCanBePrivate.Local
    // ReSharper disable InconsistentNaming

#region consts

    public enum FileSystemControl {
      SetCompression = 0x9C040,
    }

    [Flags]
    public enum ShellFileInfoFlags : uint {
      AddOverlays = 0x20,
      AttributesSpecified = 0x20000,
      Attributes = 0x800,
      DisplayName = 0x200,
      ExecutableType = 0x2000,
      Icon = 0x100,
      IconLocation = 0x1000,
      LargeIcon = 0x00,
      LinkOverlay = 0x8000,
      OpenIcon = 0x02,
      OverlayIndex = 0x40,
      PointerToItemList = 0x08,
      Selected = 0x10000,
      ShellIconSize = 0x04,
      SmallIcon = 0x01,
      SystemIconIndex = 0x4000,
      Typename = 0x400,
      UseFileAttributes = 0x10,
    }

    [Flags]
    public enum FileAttribute : uint {
      Normal = 0x80,
    }

    public enum SFGAO : uint {
      CanCopy = 0x01,
      CanMove = 0x02,
      CanLink = 0x04,
      Storage = 0x08,
      CanRename = 0x10,
      CanDelete = 0x20,
      HasPropertySheet = 0x40,
      IsDropTarget = 0x100,
      CapabilityMask = CanCopy | CanMove | CanLink | CanRename | CanDelete | HasPropertySheet | IsDropTarget,
      IsSystem = 0x1000,
      IsEncrypted = 0x2000,
      IsSlow = 0x4000,
      IsGhosted = 0x8000,
      IsShortcut = 0x10000,
      IsShared = 0x20000,
      IsReadOnly = 0x40000,
      IsHidden = 0x80000,
      IsNonEnumerated = 0x100000,
      IsNewContent = 0x200000,
      HasStream = 0x400000,
      HasStorageAncestor = 0x800000,
      Validate = 0x1000000,
      IsRemovable = 0x2000000,
      IsCompressed = 0x4000000,
      IsBrowseable = 0x8000000,
      HasFilesystemAncestor = 0x10000000,
      IsFolder = 0x20000000,
      IsFilesystemItem = 0x40000000,
      StorageCapabilityMask = Storage | IsShortcut | IsReadOnly | HasStream | HasStorageAncestor | HasFilesystemAncestor | IsFolder | IsFilesystemItem,
      HasSubFolder = 0x80000000,
    }

#endregion

#region imports
    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int DeviceIoControl(
      IntPtr hDevice,
      [MarshalAs(UnmanagedType.I4)]FileSystemControl dwIoControlCode,
      ref short lpInBuffer,
      int nInBufferSize,
      IntPtr lpOutBuffer,
      int nOutBufferSize,
      out int lpBytesReturned,
      IntPtr lpOverlapped
    );

    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
    public static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileAttribute dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, [MarshalAs(UnmanagedType.U4)] ShellFileInfoFlags uFlags);

#endregion

#region nested types
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO {
      private const int _MAX_TYPE_NAME_SIZE = 80;
      private const int _MAX_DISPLAY_NAME_SIZE = 260;
      public IntPtr hIcon;
      public int iIcon;
      public SFGAO dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_DISPLAY_NAME_SIZE)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_TYPE_NAME_SIZE)]
      public string szTypeName;
    };
#endregion

    // ReSharper restore InconsistentNaming
    // ReSharper restore MemberCanBePrivate.Local
    // ReSharper restore UnusedMember.Local
#pragma warning restore CC0074 // Make field readonly
#pragma warning restore CC0021 // Use nameof

  }

#region consts

  private static Encoding _utf8NoBom;
  internal static Encoding Utf8NoBom {
    get {
      if (_utf8NoBom != null)
        return _utf8NoBom;

      UTF8Encoding utF8Encoding = new(false, true);
      return _utf8NoBom = utF8Encoding;
    }
  }

#endregion

#region native file compression

  /// <summary>
  /// Enables the file compression on NTFS volumes.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  public static void EnableCompression(this FileInfo @this) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

    @this.Refresh();
    if (!@this.Exists)
      throw new FileNotFoundException(@this.FullName);

    short defaultCompressionFormat = 1;

    int result;
    using (var f = File.Open(@this.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
      result = NativeMethods.DeviceIoControl(
        // ReSharper disable once PossibleNullReferenceException
        f.SafeFileHandle.DangerousGetHandle(),
        NativeMethods.FileSystemControl.SetCompression,
        ref defaultCompressionFormat,
        sizeof(short),
        IntPtr.Zero,
        0,
        out var lpBytesReturned,
        IntPtr.Zero
      );
    }
    if (result < 0)
      throw new Win32Exception();

  }

  /// <summary>
  /// Tries to enable the file compression on NTFS volumes.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> on success; otherwise <c>false</c>.</returns>
  public static bool TryEnableCompression(this FileInfo @this) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

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
        out var lpBytesReturned,
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
  /// Gets the type description.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The description shown in the windows explorer under filetype.</returns>
  public static string GetTypeDescription(this FileInfo @this) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

    var shinfo = new NativeMethods.SHFILEINFO();
    NativeMethods.SHGetFileInfo(@this.FullName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), NativeMethods.ShellFileInfoFlags.Typename);
    return shinfo.szTypeName.Trim();
  }
#endregion

#region file copy/move/rename

  /// <summary>
  /// Copies the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="targetDirectory">The destination directory.</param>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), false)
  ;

  /// <summary>
  /// Copies the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="targetDirectory">The destination directory.</param>
  /// <param name="overwrite"><c>true</c> if existing files shall be overwritten; otherwise, <c>false</c>; default to <c>false</c>.</param>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), overwrite)
  ;

  /// <summary>
  /// Copies the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="targetFile">The target file.</param>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile)
    => @this.CopyTo(targetFile.FullName, false)
  ;

  /// <summary>
  /// Copies the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="targetFile">The target file.</param>
  /// <param name="overwrite"><c>true</c> if existing files shall be overwritten; otherwise, <c>false</c>; default to <c>false</c>.</param>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite)
    => @this.CopyTo(targetFile.FullName, overwrite)
  ;

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFile">The destination file.</param>
  public static void MoveTo(this FileInfo @this, FileInfo destFile) => @this.MoveTo(destFile.FullName, false);

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFile">The destination file.</param>
  /// <param name="timeout">The timeout.</param>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, TimeSpan timeout) => @this.MoveTo(destFile.FullName, false, timeout);

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFile">The destination file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite) => @this.MoveTo(destFile.FullName, overwrite);

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFile">The destination file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
  /// <param name="timeout">The timeout.</param>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite, TimeSpan timeout) => @this.MoveTo(destFile.FullName, overwrite, timeout);

#if !SUPPORTS_MOVETO_OVERWRITE

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFileName">Name of the destination file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite) => MoveTo(@this, destFileName, overwrite, TimeSpan.FromSeconds(30));

#endif

  /// <summary>
  /// Moves the file to the target directory.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="destFileName">Name of the destination file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
  /// <param name="timeout">The timeout.</param>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite, TimeSpan timeout) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

    // copy file 
    using TransactionScope scope = new ();
    @this.CopyTo(destFileName, overwrite);

    // delete source, retry during timeout
    var delay = TimeSpan.FromSeconds(1);
    var tries = (int)(timeout.Ticks / delay.Ticks);
    while (true) {
      try {
        @this.Delete();
        break;

      } catch (IOException) {
        if (tries-- < 1)
          throw;

        Thread.Sleep(delay);
      }
    }
    scope.Complete();
  }
#endregion

#region hash computation

#if !NET6_0_OR_GREATER

  /// <summary>
  /// Computes the hash.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The result of the hash algorithm</returns>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this) where THashAlgorithm : HashAlgorithm,new() {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

    using THashAlgorithm provider = new();
    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }
    
  /// <summary>
  /// Computes the hash.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
  /// <param name="this">This FileInfo.</param>
  /// <param name="blockSize">Size of the block.</param>
  /// <returns>
  /// The result of the hash algorithm
  /// </returns>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this, int blockSize) where THashAlgorithm : HashAlgorithm, new() {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

    using THashAlgorithm provider = new();
    return ComputeHash(@this, provider, blockSize);
  }

#endif

  /// <summary>
  /// Computes the hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="provider">The type of the hash algorithm.</param>
  /// <returns>The result of the hash algorithm</returns>
  public static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
    Contract.Requires(provider != null);
#endif
    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }
    
  /// <summary>
  /// Computes the hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="provider">The type of the hash algorithm.</param>
  /// <param name="blockSize">Size of the block.</param>
  /// <returns>
  /// The result of the hash algorithm
  /// </returns>
  public static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider, int blockSize) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
    Contract.Requires(provider != null);
#endif
    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, blockSize);

    provider.Initialize();
    var lastBlock = new byte[blockSize];
    var currentBlock = new byte[blockSize];

    var lastBlockSize = stream.Read(lastBlock, 0, blockSize);
    while (true) {
      var currentBlockSize = stream.Read(currentBlock, 0, blockSize);
      if (currentBlockSize < 1)
        break;

      provider.TransformBlock(lastBlock, 0, lastBlockSize, null, 0);

      // ReSharper disable once SwapViaDeconstruction
      var temp = lastBlock;
      lastBlock = currentBlock;
      currentBlock = temp;

      lastBlockSize = currentBlockSize;
    }

    provider.TransformFinalBlock(lastBlock, 0, lastBlockSize < 1 ? blockSize : lastBlockSize);
    return provider.Hash;
  }

  /// <summary>
  /// Calculates the SHA512 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The hash</returns>
  public static byte[] ComputeSHA512Hash(this FileInfo @this) {
    using var provider=SHA512.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Calculates the SHA512 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="blockSize">Size of the block.</param>
  /// <returns>
  /// The hash
  /// </returns>
  public static byte[] ComputeSHA512Hash(this FileInfo @this, int blockSize) {
    using var provider = SHA512.Create();
    return @this.ComputeHash(provider,blockSize);
  }

  /// <summary>
  /// Calculates the SHA384 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The hash</returns>
  public static byte[] ComputeSHA384Hash(this FileInfo @this) {
    using var provider = SHA384.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Calculates the SHA256 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The hash</returns>
  public static byte[] ComputeSHA256Hash(this FileInfo @this) {
    using var provider = SHA256.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Calculates the SHA-1 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The hash</returns>
  public static byte[] ComputeSHA1Hash(this FileInfo @this) {
    using var provider = SHA1.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Calculates the MD5 hash.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The hash</returns>
  public static byte[] ComputeMD5Hash(this FileInfo @this) {
    using var provider = MD5.Create();
    return @this.ComputeHash(provider);
  }

#endregion

#region reading
    
  /// <summary>
  /// Identifies the file's encoding.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The encoding or <c>null</c> when the file has zero bytes length.</returns>
  public static Encoding GetEncoding(this FileInfo @this) {
    // Read the BOM
    var bom = new byte[4];
    using (FileStream file = new(@this.FullName, FileMode.Open, FileAccess.Read))
    {
      var bytesRead = file.Read(bom, 0, 4);
      if(bytesRead == 0)
        return null;
    }

    // Analyze the BOM
#if DEPRECATED_UTF7
      if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.Default;
#else
    if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#endif
    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
    if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
    if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
    return Encoding.Default;
  }
    
  /// <summary>
  /// Reads all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns></returns>
  public static string ReadAllText(this FileInfo @this) => File.ReadAllText(@this.FullName);

  /// <summary>
  /// Reads all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns></returns>
  public static string ReadAllText(this FileInfo @this, Encoding encoding) => File.ReadAllText(@this.FullName, encoding);

  /// <summary>
  /// Reads all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns></returns>
  public static string[] ReadAllLines(this FileInfo @this) => File.ReadAllLines(@this.FullName);

  /// <summary>
  /// Tries to read all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="result">This file contents.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryReadAllLines(this FileInfo @this, out string[] result){
    try{
      result = File.ReadAllLines(@this.FullName);
      return true;
    }catch(Exception){
      result = null;
      return true;
    }
  }

  /// <summary>
  /// Reads all lines.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/>.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns></returns>
  public static string[] ReadAllLines(this FileInfo @this, Encoding encoding) => File.ReadAllLines(@this.FullName, encoding);

  /// <summary>
  /// Tries to read all lines.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/>.</param>
  /// <param name="encoding">The encoding.</param>
  /// <param name="result">This file contents.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryReadAllLines(this FileInfo @this, Encoding encoding, out string[] result){
    try{
      result = File.ReadAllLines(@this.FullName, encoding);
      return true;
    }catch(Exception){
      result = null;
      return true;
    }
  }

  /// <summary>
  /// Reads bytes from the file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/>.</param>
  /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Byte"/></returns>
  public static IEnumerable<byte> ReadBytes(this FileInfo @this) {
    using var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    while (!stream.IsAtEndOfStream())
      yield return (byte)stream.ReadByte();
  }

  /// <summary>
  /// Reads all bytes.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/>.</param>
  /// <returns></returns>
  public static byte[] ReadAllBytes(this FileInfo @this) => File.ReadAllBytes(@this.FullName);

  /// <summary>
  /// Tries to read all bytes.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/>.</param>
  /// <param name="result">This file contents.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryReadAllBytes(this FileInfo @this, out byte[] result){
    try{
      result = File.ReadAllBytes(@this.FullName);
      return true;
    }catch(Exception){
      result = null;
      return true;
    }
  }

  /// <summary>
  /// Reads the lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns></returns>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this) => File.ReadLines(@this.FullName);
#else
    public static IEnumerable<string> ReadLines(this FileInfo @this) => ReadLines(@this, FileShare.Read);
#endif

  public static IEnumerable<string> ReadLines(this FileInfo @this, FileShare share) {
    const int bufferSize = 4096;
    using var stream = (Stream)new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, share, bufferSize, FileOptions.SequentialScan);
    using StreamReader reader = new(stream);
    while(!reader.EndOfStream){
      var line=reader.ReadLine();
      if(line==null)
        yield break;

      yield return line;
    }
  }

  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding,FileShare share) {
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
  /// Reads the lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="encoding">The encoding.</param>
  /// <returns></returns>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => File.ReadLines(@this.FullName, encoding);
#else
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => ReadLines(@this, encoding,FileShare.Read);
#endif

#endregion

#region writing
  /// <summary>
  /// Writes all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  public static void WriteAllText(this FileInfo @this, string contents) => File.WriteAllText(@this.FullName, contents);

  /// <summary>
  /// Writes all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
  public static void WriteAllText(this FileInfo @this, string contents, Encoding encoding) => File.WriteAllText(@this.FullName, contents, encoding);

  /// <summary>
  /// Writes all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  public static void WriteAllLines(this FileInfo @this, string[] contents) => File.WriteAllLines(@this.FullName, contents);

  /// <summary>
  /// Writes all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
  public static void WriteAllLines(this FileInfo @this, string[] contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents, encoding);

  /// <summary>
  /// Writes all bytes.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="bytes">The bytes.</param>
  public static void WriteAllBytes(this FileInfo @this, byte[] bytes) => File.WriteAllBytes(@this.FullName, bytes);

  /// <summary>
  /// Writes all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
#if SUPPORTS_ENUMERATING_IO
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents) => File.WriteAllLines(@this.FullName, contents);
#else
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents) => File.WriteAllLines(@this.FullName, contents.ToArray());
#endif

  /// <summary>
  /// Writes all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
#if SUPPORTS_ENUMERATING_IO
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents, encoding);
#else
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents.ToArray(), encoding);
#endif

#endregion

#region appending
  /// <summary>
  /// Appends all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  public static void AppendAllText(this FileInfo @this, string contents) => File.AppendAllText(@this.FullName, contents);

  /// <summary>
  /// Appends all text.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
  public static void AppendAllText(this FileInfo @this, string contents, Encoding encoding) => File.AppendAllText(@this.FullName, contents, encoding);

  /// <summary>
  /// Appends all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
#if SUPPORTS_ENUMERATING_IO
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents) => File.AppendAllLines(@this.FullName, contents);
#else
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents) => File.AppendAllText(@this.FullName, string.Join(Environment.NewLine, contents.ToArray()) + Environment.NewLine);
#endif

  /// <summary>
  /// Appends all lines.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
#if SUPPORTS_ENUMERATING_IO
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.AppendAllLines(@this.FullName, contents, encoding);
#else
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.AppendAllText(@this.FullName, string.Join(Environment.NewLine, contents.ToArray()) + Environment.NewLine, encoding);
#endif

  /// <summary>
  /// Appends the line.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  public static void AppendLine(this FileInfo @this, string contents) => @this.AppendAllLines(new[] { contents });

  /// <summary>
  /// Appends the line.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding.</param>
  public static void AppendLine(this FileInfo @this, string contents, Encoding encoding) => @this.AppendAllLines(new[] { contents }, encoding);

#endregion

#region trimming text files
  /// <summary>
  /// Keeps the first lines of a text file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  public static void KeepFirstLines(this FileInfo @this, int count) => @this.KeepFirstLines(count, Utf8NoBom);

  /// <summary>
  /// Keeps the first lines of a text file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  /// <param name="encoding">The encoding.</param>
  public static void KeepFirstLines(this FileInfo @this, int count, Encoding encoding) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
    Contract.Requires(encoding != null);
#endif

    FileInfo tempFile = null;
    try {
      tempFile = new(Path.GetTempFileName());

      using (var inputFile = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
      using (StreamReader inputReader = new(inputFile, encoding))
      using (var outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
      using (StreamWriter outputWriter = new(outputFile, encoding))
        for (var i = 0; i < count && !inputReader.EndOfStream; ++i)
          outputWriter.WriteLine(inputReader.ReadLine());

      tempFile.Attributes = @this.Attributes;
      tempFile.MoveTo(@this.FullName, true);

    } finally {
      if (tempFile is { Exists: true })
        tempFile.Delete();
    }
  }

  /// <summary>
  /// Keeps the last lines of a text file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  public static void KeepLastLines(this FileInfo @this, int count) => @this.KeepLastLines(count, Utf8NoBom);

  /// <summary>
  /// Keeps the last lines of a text file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  /// <param name="encoding">The encoding.</param>
  public static void KeepLastLines(this FileInfo @this, int count, Encoding encoding) {
    Debug.Assert(@this != null);
    Debug.Assert(encoding != null);

    const int _LINE_FEED_CHAR = 10;
    const int BUFFER_SIZE = 8192;

    long GetFilePosition(Stream file) {
      var buffer = new byte[BUFFER_SIZE];
      var currentFilePosition = file.Length - 1;
      while (currentFilePosition > 0) {
        var bytesToRead = buffer.Length > currentFilePosition ? (int) currentFilePosition : buffer.Length;
        currentFilePosition -= bytesToRead;

        file.Seek(currentFilePosition, SeekOrigin.Begin);
        var bytesRead = file.Read(buffer, 0, bytesToRead);

        var j = bytesRead - 1;
        for (; j > 0; --j)
          if (buffer[j] == _LINE_FEED_CHAR)
            if (--count <= 0)
              return currentFilePosition + j;
      }

      return 0;
    }

    FileInfo tmpFile = null;
    try {
      using (var inputReader = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read,BUFFER_SIZE)) {
        var filePosition = GetFilePosition(inputReader);
        if (filePosition <= 0)
          return;
          
        inputReader.Seek(filePosition+1, SeekOrigin.Begin);
        tmpFile = new(Path.GetTempFileName());
        using (var tmpFileWriter = tmpFile.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
          inputReader.CopyTo(tmpFileWriter);
          tmpFileWriter.Flush(true);
        }
      }

      tmpFile.Attributes = @this.Attributes;
      tmpFile.MoveTo(@this.FullName, true);

    } finally {
      if (tmpFile != null && File.Exists(tmpFile.FullName))
        tmpFile.Delete();
    }
  }

  /// <summary>
  /// Removes the first n lines from a file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  public static void RemoveFirstLines(this FileInfo @this, int count) => @this.RemoveFirstLines(count, Utf8NoBom);

  /// <summary>
  /// Removes the first n lines from a file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="count">The count.</param>
  /// <param name="encoding">The encoding.</param>
  public static void RemoveFirstLines(this FileInfo @this, int count, Encoding encoding) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
    Contract.Requires(encoding != null);
#endif

    FileInfo tempFile = null;
    try {
      tempFile = new(Path.GetTempFileName());

      using (var inputFile = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
      using (StreamReader inputReader = new(inputFile, encoding))
      using (var outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
      using (StreamWriter outputWriter = new(outputFile, encoding)) {

        // skip lines
        for (var i = 0; i < count && !inputReader.EndOfStream; ++i)
          inputReader.ReadLine();

        // write rest
        while (!inputReader.EndOfStream)
          outputWriter.WriteLine(inputReader.ReadLine());
      }
      tempFile.Attributes = @this.Attributes;
      tempFile.MoveTo(@this.FullName, true);

    } finally {
      if (tempFile is { Exists: true })
        tempFile.Delete();
    }
  }
#endregion

#region opening
  /// <summary>
  /// Opens the specified file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="mode">The mode.</param>
  /// <param name="access">The access.</param>
  /// <param name="share">The share.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <returns></returns>
  public static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize) => new(@this.FullName, mode, access, share, bufferSize);

  /// <summary>
  /// Opens the specified file.
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
  /// Opens the specified file.
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
    /// Opens the specified file.
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
    /// Opens the specified file.
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
  /// Gets the filename without extension.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The filename without the extension.</returns>
  public static string GetFilenameWithoutExtension(this FileInfo @this) => Path.GetFileNameWithoutExtension(@this.FullName);

  /// <summary>
  /// Gets the filename.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns>The filename.</returns>
  public static string GetFilename(this FileInfo @this) => Path.GetFileName(@this.FullName);

  /// <summary>
  /// Creates an instance with a new extension.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="extension">The extension.</param>
  /// <returns>A new FileInfo instance with given extension.</returns>
  public static FileInfo WithNewExtension(this FileInfo @this, string extension) => new(Path.ChangeExtension(@this.FullName, extension));

#endregion

  /// <summary>
  /// Tries to delete the given file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryDelete(this FileInfo @this) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif
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
  /// Tries to create a new file.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns>
  ///   <c>true</c> if the file didn't exist and was successfully created; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreate(this FileInfo @this, FileAttributes attributes = FileAttributes.Normal) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

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
  /// Changes the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  public static void Touch(this FileInfo @this) => @this.LastWriteTimeUtc=DateTime.UtcNow;
    
  /// <summary>
  /// Tries to change the last write time of the given file to the current date/time.
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
  /// Tries to change the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="waitTime">The wait time.</param>
  /// <param name="repeat">The repeat.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryTouch(this FileInfo @this, TimeSpan waitTime, int repeat = 3) {
    while(--repeat>=0) {
      if(TryTouch(@this))
        return true;

      Thread.Sleep(waitTime);
    }

    return false;
  }

  /// <summary>
  /// Checks whether the given file does not exist.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
  public static bool NotExists(this FileInfo @this) => !@this.Exists;

#region needed consts for converting filename patterns into regexes
  private static readonly Regex _ILEGAL_CHARACTERS_REGEX = new("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
  private static readonly Regex _CATCH_EXTENSION_REGEX = new(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
#endregion

  /// <summary>
  /// Converts a given filename pattern into a regular expression.
  /// </summary>
  /// <param name="pattern">The pattern.</param>
  /// <returns>The regex.</returns>
  private static string _ConvertFilePatternToRegex(string pattern) {
    Against.ArgumentIsNull(pattern);
    
    pattern = pattern.Trim();

    if (pattern.Length == 0) throw new ArgumentException("Pattern is empty.", nameof(pattern));
    if (_ILEGAL_CHARACTERS_REGEX.IsMatch(pattern)) throw new ArgumentException("Patterns contains ilegal characters.", nameof(pattern));

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

  public static bool MatchesFilter(this FileInfo @this,string filter){
    var regex=_ConvertFilePatternToRegex(filter);
    return Regex.IsMatch(@this.FullName, regex, RegexOptions.IgnoreCase);
  }

#if SUPPORTS_STREAM_ASYNC

  /// <summary>
  /// Compares two files for content equality.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="other">The other FileInfo.</param>
  /// <param name="bufferSize">The number of bytes to compare in each step (Beware of the 85KB-LOH limit).</param>
  /// <returns><c>true</c> if both are bytewise equal; otherwise, <c>false</c>.</returns>
  public static bool IsContentEqualTo(this FileInfo @this, FileInfo other, int bufferSize = 65536) {
    if (@this == null)
      throw new NullReferenceException();
    if (other == null)
      throw new ArgumentNullException(nameof(other));

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
      var sourceAsync = ReadBlockFromStream(sourceStream, blockIndex, sourceBufferA);
      var comparisonAsync = ReadBlockFromStream(comparisonStream, blockIndex, comparisonBufferA);
      int sourceBytes;
      int comparisonBytes;

      while (enumerator.MoveNext()) {
        sourceBytes = sourceAsync.Result;
        comparisonBytes = comparisonAsync.Result;

        // start reading next buffers into B and B'
        blockIndex = enumerator.Current;
        sourceAsync = ReadBlockFromStream(sourceStream, blockIndex, sourceBufferB);
        comparisonAsync = ReadBlockFromStream(comparisonStream, blockIndex, comparisonBufferB);

        // compare A and A' and return false upon difference
        if (sourceBytes != comparisonBytes || !AreBytesEqual(sourceBufferA, comparisonBufferA, sourceBytes))
          return false;

        // switch A and B and A' and B'
        Swap(ref sourceBufferA, ref sourceBufferB);
        Swap(ref comparisonBufferA, ref comparisonBufferB);
      }

      // compare A and A'
      sourceBytes = sourceAsync.Result;
      comparisonBytes = comparisonAsync.Result;
      return sourceBytes == comparisonBytes && AreBytesEqual(sourceBufferA, comparisonBufferA, sourceBytes);
    }

    static void Swap(ref byte[] bufferA, ref byte[] bufferB) => (bufferA, bufferB) = (bufferB, bufferA);

    static Task<int> ReadBlockFromStream(Stream stream, long blockIndex, byte[] buffer) {
      var blockSize = buffer.Length;
      stream.Seek(blockIndex * blockSize, SeekOrigin.Begin);
      return stream.ReadAsync(buffer, 0, blockSize);
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

#if UNSAFE

    static unsafe bool AreBytesEqual(byte[] source, byte[] comparand, int length) {
      if (ReferenceEquals(source, comparand))
        return true;

      if (source == null || comparand == null)
        return false;

      if (source.Length != comparand.Length)
        return false;

      fixed (byte* sourcePin = source, comparisonPin = comparand) {
        var sourcePointer = (long*)sourcePin;
        var comparisonPointer = (long*)comparisonPin;
        while (length >= 8) {
          if (*sourcePointer != *comparisonPointer)
            return false;

          ++sourcePointer;
          ++comparisonPointer;
          length -= 8;
        }

        var byteSourcePointer = (byte*)sourcePointer;
        var byteComparisonPointer = (byte*)comparisonPointer;
        while (length > 0) {
          if (*byteSourcePointer != *byteComparisonPointer)
            return false;

          ++byteSourcePointer;
          ++byteComparisonPointer;
          --length;
        }
      }

      return true;
    }

#else

    static bool AreBytesEqual(byte[] source, byte[] comparand, int length) {
      if (ReferenceEquals(source, comparand))
        return true;

      if (source == null || comparand == null)
        return false;

      if (source.Length != comparand.Length)
        return false;

      for (var i = 0; i < length; ++i)
        if (source[i] != comparand[i])
          return false;

      return true;
    }

#endif

  }

#endif

  /// <summary>
  /// Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/></param>
  /// <param name="other">The file that should replace this file</param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other)
    => ReplaceWith(@this, other, null, false)
  ;

  /// <summary>
  /// Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="ignoreMetaDataErrors"><see langword="true"/> when metadata errors should be ignored; otherwise, <see langword="false"/>.</param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other, bool ignoreMetaDataErrors)
    => ReplaceWith(@this, other, null, ignoreMetaDataErrors)
  ;

  /// <summary>
  /// Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="backupFile">The file that gets a backup from this file; optional</param>
  public static void ReplaceWith(this FileInfo @this, FileInfo other, FileInfo backupFile)
    => ReplaceWith(@this, other, backupFile, false)
  ;

  /// <summary>
  /// Replaces the contents of the file with that from another file.
  /// </summary>
  /// <param name="this">This <see cref="FileInfo"/></param>
  /// <param name="other">The file that should replace this file</param>
  /// <param name="backupFile">The file that gets a backup from this file; optional</param>
  /// <param name="ignoreMetaDataErrors"><see langword="true"/> when metadata errors should be ignored; otherwise, <see langword="false"/>.</param>
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

  public interface IFileInProgress:IDisposable {
    FileInfo TemporaryFile { get; }
    bool CancelChanges { get; set; }
  }

  private class FileInProgress : IFileInProgress {

    private readonly FileInfo _sourceFile;
    private readonly PathExtensions.ITemporaryFileToken _token;
    private bool _isDisposed;

    public FileInProgress(FileInfo sourceFile) {
      this._sourceFile = sourceFile;
      this._token = PathExtensions.GetTempFileToken(sourceFile.Name + ".$$$", sourceFile.DirectoryName);
    }

    ~FileInProgress() => this.Dispose();

    #region Implementation of IDisposable

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      GC.SuppressFinalize(this);

      if (!this.CancelChanges)
        this._sourceFile.ReplaceWith(this.TemporaryFile);

      this._token.Dispose();
    }

    #endregion

    #region Implementation of IFileInProgress

    public FileInfo TemporaryFile => this._token.File;

    public bool CancelChanges { get; set; }

    #endregion
  }

  public static IFileInProgress StartWorkInProgress(this FileInfo @this, bool copyContents = false) {
    Against.ThisIsNull(@this);

    var result = new FileInProgress(@this);
    if (copyContents)
      @this.CopyTo(result.TemporaryFile, true);

    return result;
  }

}