#region (c)2010-2020 Hawkynt
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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;

namespace System.IO {
  internal static partial class FileInfoExtensions {

    /// <summary>
    /// The native methods.
    /// </summary>
    private static class NativeMethods {

      #region consts
      public const int FSCTL_SET_COMPRESSION = 0x9C040;
      public const uint SHGFI_DISPLAYNAME = 0x200;
      public const uint SHGFI_TYPENAME = 0x400;
      public const uint SHGFI_ICON = 0x100;
      public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
      public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
      public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
      public const uint SHGFI_LINKOVERLAY = 0x8000;
      public const uint SHGFI_USEFILEATTRIBUTES = 0x10;

      #endregion

      #region imports
      [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl")]
      public static extern int DeviceIoControl(
        IntPtr hDevice,
        int dwIoControlCode,
        ref short lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        ref int lpBytesReturned,
        IntPtr lpOverlapped
        );
      [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
      public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
      [DllImport("user32.dll", SetLastError = true, EntryPoint = "DestroyIcon")]
      public static extern bool DestroyIcon(IntPtr hIcon);

      #endregion

      #region nested types
      [StructLayout(LayoutKind.Sequential)]
      public struct SHFILEINFO {
        private const int _MAX_TYPE_NAME_SIZE = 80;
        private const int _MAX_DISPLAY_NAME_SIZE = 260;
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_DISPLAY_NAME_SIZE)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_TYPE_NAME_SIZE)]
        public string szTypeName;
      };
      #endregion
    }

    #region consts

    internal static Encoding _UTF8NoBOM;
    internal static Encoding UTF8NoBOM {
      get {
        if (_UTF8NoBOM != null)
          return _UTF8NoBOM;

        var utF8Encoding = new UTF8Encoding(false, true);
        Thread.MemoryBarrier();
        return (_UTF8NoBOM = utF8Encoding);
      }
    }

    #endregion

    /// <summary>
    /// Enables the file compression on NTFS volumes.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns><c>true</c> on success; otherwise <c>false</c>.</returns>
    public static bool EnableCompression(this FileInfo This) {
      Contract.Requires(This != null);

      if (!This.Exists)
        throw new FileNotFoundException(This.FullName);
      short COMPRESSION_FORMAT_DEFAULT = 1;

      var lpBytesReturned = 0;
      int result;
      using (var f = File.Open(This.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
        result = NativeMethods.DeviceIoControl(
          f.Handle,
          NativeMethods.FSCTL_SET_COMPRESSION,
          ref COMPRESSION_FORMAT_DEFAULT,
          2 /*sizeof(short)*/,
          IntPtr.Zero,
          0,
          ref lpBytesReturned,
          IntPtr.Zero);
      }
      return (result >= 0 && result <= 0x7FFFFFFF);
    }

    /// <summary>
    /// Gets the type description.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The description shown in the windows explorer under filetype.</returns>
    public static string GetTypeDescription(this FileInfo This) {
      Contract.Requires(This != null);

      var shinfo = new NativeMethods.SHFILEINFO();
      NativeMethods.SHGetFileInfo(This.FullName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), NativeMethods.SHGFI_TYPENAME);
      return (shinfo.szTypeName.Trim());
    }

    /// <summary>
    /// Gets the icon.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="smallIcon">if set to <c>true</c> we'll return a 16x16 version; otherwise, it will be 32x32.</param>
    /// <param name="linkOverlay">if set to <c>true</c> the link overlays on shortcuts will be returned along the icon.</param>
    /// <returns>The icon used by the windows explorer for this file.</returns>
    public static Icon GetIcon(this FileInfo This, bool smallIcon = false, bool linkOverlay = false) {
      Contract.Requires(This != null);

      var flags = NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_USEFILEATTRIBUTES | (smallIcon ? NativeMethods.SHGFI_SMALLICON : NativeMethods.SHGFI_LARGEICON);
      if (linkOverlay)
        flags |= NativeMethods.SHGFI_LINKOVERLAY;

      var shfi = new NativeMethods.SHFILEINFO();
      NativeMethods.SHGetFileInfo(This.FullName, NativeMethods.FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

      var result = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
      NativeMethods.DestroyIcon(shfi.hIcon);

      return (result);
    }

    /// <summary>
    /// Moves the file to the target directory.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="destFileName">Name of the destination file.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
    public static void MoveTo(this FileInfo This, string destFileName, bool overwrite) {
      Contract.Requires(This != null);
      using (var scope = new TransactionScope()) {
        This.CopyTo(destFileName, overwrite);
        This.Delete();
        scope.Complete();
      }
    }

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The result of the hash algorithm</returns>
    public static byte[] ComputeHash<THashAlgorithm>(this FileInfo This) where THashAlgorithm : HashAlgorithm, new() {
      Contract.Requires(This != null);
      using (var provider = new THashAlgorithm())
      using (var stream = new FileStream(This.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
        return (provider.ComputeHash(stream));
    }

    /// <summary>
    /// Calculates the SHA512 hash.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA512Hash(this FileInfo This) {
      return (This.ComputeHash<SHA512CryptoServiceProvider>());
    }

    /// <summary>
    /// Calculates the SHA384 hash.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA384Hash(this FileInfo This) {
      return (This.ComputeHash<SHA384CryptoServiceProvider>());
    }

    /// <summary>
    /// Calculates the SHA256 hash.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA256Hash(this FileInfo This) {
      return (This.ComputeHash<SHA256CryptoServiceProvider>());
    }

    /// <summary>
    /// Calculates the SHA-1 hash.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA1Hash(this FileInfo This) {
      return (This.ComputeHash<SHA1CryptoServiceProvider>());
    }

    /// <summary>
    /// Calculates the MD5 hash.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeMD5Hash(this FileInfo This) {
      return (This.ComputeHash<MD5CryptoServiceProvider>());
    }

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns></returns>
    public static string ReadAllText(this FileInfo This) {
      Contract.Requires(This != null);
      return (File.ReadAllText(This.FullName));
    }

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns></returns>
    public static string ReadAllText(this FileInfo This, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      return (File.ReadAllText(This.FullName, encoding));
    }

    /// <summary>
    /// Reads all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns></returns>
    public static string[] ReadAllLines(this FileInfo This) {
      Contract.Requires(This != null);
      return (File.ReadAllLines(This.FullName));
    }

    /// <summary>
    /// Reads all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns></returns>
    public static string[] ReadAllLines(this FileInfo This, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      return (File.ReadAllLines(This.FullName, encoding));
    }

    /// <summary>
    /// Reads all bytes.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns></returns>
    public static byte[] ReadAllBytes(this FileInfo This) {
      Contract.Requires(This != null);
      return (File.ReadAllBytes(This.FullName));
    }

    /// <summary>
    /// Reads the lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(this FileInfo This) {
      Contract.Requires(This != null);
      return (File.ReadLines(This.FullName));
    }

    /// <summary>
    /// Reads the lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns></returns>
    public static IEnumerable<string> ReadLines(this FileInfo This, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      return (File.ReadLines(This.FullName, encoding));
    }

    /// <summary>
    /// Writes all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void WriteAllText(this FileInfo This, string contents) {
      Contract.Requires(This != null);
      File.WriteAllText(This.FullName, contents);
    }

    /// <summary>
    /// Writes all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void WriteAllText(this FileInfo This, string contents, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      File.WriteAllText(This.FullName, contents, encoding);
    }

    /// <summary>
    /// Writes all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void WriteAllLines(this FileInfo This, string[] contents) {
      Contract.Requires(This != null);
      File.WriteAllLines(This.FullName, contents);
    }

    /// <summary>
    /// Writes all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void WriteAllLines(this FileInfo This, string[] contents, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      File.WriteAllLines(This.FullName, contents, encoding);
    }

    /// <summary>
    /// Writes all bytes.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="bytes">The bytes.</param>
    public static void WriteAllBytes(this FileInfo This, byte[] bytes) {
      Contract.Requires(This != null);
      File.WriteAllBytes(This.FullName, bytes);
    }

    /// <summary>
    /// Writes all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void WriteAllLines(this FileInfo This, IEnumerable<string> contents) {
      Contract.Requires(This != null);
      File.WriteAllLines(This.FullName, contents);
    }

    /// <summary>
    /// Writes all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void WriteAllLines(this FileInfo This, IEnumerable<string> contents, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      File.WriteAllLines(This.FullName, contents, encoding);
    }

    /// <summary>
    /// Appends all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void AppendAllText(this FileInfo This, string contents) {
      Contract.Requires(This != null);
      File.AppendAllText(This.FullName, contents);
    }

    /// <summary>
    /// Appends all text.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void AppendAllText(this FileInfo This, string contents, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);
      File.AppendAllText(This.FullName, contents, encoding);
    }

    /// <summary>
    /// Appends all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void AppendAllLines(this FileInfo This, IEnumerable<string> contents) {
      Contract.Requires(This != null);
      Contract.Requires(contents != null);
      File.AppendAllLines(This.FullName, contents);
    }

    /// <summary>
    /// Appends all lines.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void AppendAllLines(this FileInfo This, IEnumerable<string> contents, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(contents != null);
      Contract.Requires(encoding != null);
      File.AppendAllLines(This.FullName, contents, encoding);
    }

    /// <summary>
    /// Appends the line.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    public static void AppendLine(this FileInfo This, string contents) {
      This.AppendAllLines(new[] { contents });
    }

    /// <summary>
    /// Appends the line.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public static void AppendLine(this FileInfo This, string contents, Encoding encoding) {
      This.AppendAllLines(new[] { contents }, encoding);
    }

    /// <summary>
    /// Keeps the first lines of a text file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    public static void KeepFirstLines(this FileInfo This, int count) {
      This.KeepFirstLines(count, UTF8NoBOM);
    }

    /// <summary>
    /// Keeps the first lines of a text file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    /// <param name="encoding">The encoding.</param>
    public static void KeepFirstLines(this FileInfo This, int count, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);

      FileInfo tempFile = null;
      try {
        tempFile = new FileInfo(Path.GetTempFileName());

        using (var inputFile = This.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var inputReader = new StreamReader(inputFile, encoding))
        using (var outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
        using (var outputWriter = new StreamWriter(outputFile, encoding))
          for (var i = 0; i < count && !inputReader.EndOfStream; ++i)
            outputWriter.WriteLine(inputReader.ReadLine());

        tempFile.MoveTo(This.FullName, true);

      } finally {
        if (tempFile != null && tempFile.Exists)
          tempFile.Delete();
      }
    }

    /// <summary>
    /// Keeps the last lines of a text file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    public static void KeepLastLines(this FileInfo This, int count) {
      This.KeepLastLines(count, UTF8NoBOM);
    }

    /// <summary>
    /// Keeps the last lines of a text file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    /// <param name="encoding">The encoding.</param>
    public static void KeepLastLines(this FileInfo This, int count, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);

      // TODO: this is not optimal because it reads the whole file in, first
      FileInfo tempFile = null;
      try {
        tempFile = new FileInfo(Path.GetTempFileName());

        var lineBuffer = new Queue<string>();

        using (var inputFile = This.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var inputReader = new StreamReader(inputFile, encoding))
          while (!inputReader.EndOfStream) {
            lineBuffer.Enqueue(inputReader.ReadLine());
            if (lineBuffer.Count > count)
              lineBuffer.Dequeue();
          }

        using (var outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
        using (var outputWriter = new StreamWriter(outputFile, encoding))
          while (lineBuffer.Count > 0)
            outputWriter.WriteLine(lineBuffer.Dequeue());

        tempFile.MoveTo(This.FullName, true);

      } finally {
        if (tempFile != null && tempFile.Exists)
          tempFile.Delete();
      }
    }

    /// <summary>
    /// Removes the first n lines from a file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    public static void RemoveFirstLines(this FileInfo This, int count) {
      This.RemoveFirstLines(count, UTF8NoBOM);
    }

    /// <summary>
    /// Removes the first n lines from a file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="count">The count.</param>
    /// <param name="encoding">The encoding.</param>
    public static void RemoveFirstLines(this FileInfo This, int count, Encoding encoding) {
      Contract.Requires(This != null);
      Contract.Requires(encoding != null);

      FileInfo tempFile = null;
      try {
        tempFile = new FileInfo(Path.GetTempFileName());

        using (var inputFile = This.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var inputReader = new StreamReader(inputFile, encoding))
        using (var outputFile = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
        using (var outputWriter = new StreamWriter(outputFile, encoding)) {

          // skip lines
          for (var i = 0; i < count && !inputReader.EndOfStream; ++i)
            inputReader.ReadLine();

          // write rest
          while (!inputReader.EndOfStream)
            outputWriter.WriteLine(inputReader.ReadLine());
        }

        tempFile.MoveTo(This.FullName, true);

      } finally {
        if (tempFile != null && tempFile.Exists)
          tempFile.Delete();
      }
    }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="access">The access.</param>
    /// <param name="share">The share.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <returns></returns>
    public static FileStream Open(this FileInfo This, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
      Contract.Requires(This != null);
      return (new FileStream(This.FullName, mode, access, share, bufferSize));
    }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="access">The access.</param>
    /// <param name="share">The share.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static FileStream Open(this FileInfo This, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) {
      Contract.Requires(This != null);
      return (new FileStream(This.FullName, mode, access, share, bufferSize, options));
    }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="access">The access.</param>
    /// <param name="share">The share.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="useAsync">if set to <c>true</c> [use async].</param>
    /// <returns></returns>
    public static FileStream Open(this FileInfo This, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) {
      Contract.Requires(This != null);
      return (new FileStream(This.FullName, mode, access, share, bufferSize, useAsync));
    }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="rights">The rights.</param>
    /// <param name="share">The share.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public static FileStream Open(this FileInfo This, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options) {
      Contract.Requires(This != null);
      return (new FileStream(This.FullName, mode, rights, share, bufferSize, options));
    }

    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="rights">The rights.</param>
    /// <param name="share">The share.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="options">The options.</param>
    /// <param name="security">The security.</param>
    /// <returns></returns>
    public static FileStream Open(this FileInfo This, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity security) {
      Contract.Requires(This != null);
      return (new FileStream(This.FullName, mode, rights, share, bufferSize, options, security));
    }

    /// <summary>
    /// Gets the filename without extension.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The filename without the extension.</returns>
    public static string GetFilenameWithoutExtension(this FileInfo This) {
      Contract.Requires(This != null);
      return (Path.GetFileNameWithoutExtension(This.FullName));
    }
  }
}


