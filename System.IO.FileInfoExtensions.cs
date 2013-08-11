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

using System.Diagnostics.Contracts;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.IO {
  internal static partial class FileInfoExtensions {
    #region consts
    private const uint SHGFI_DISPLAYNAME = 0x200;
    private const uint SHGFI_TYPENAME = 0x400;
    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0; // 'Large icon
    private const uint SHGFI_SMALLICON = 0x1; // 'Small icon
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint SHGFI_LINKOVERLAY = 0x8000;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

    #endregion

    #region imports
    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
    private static extern IntPtr _SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "DestroyIcon")]
    private static extern bool _DestroyIcon(IntPtr hIcon);

    #endregion

    #region nested types
    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO {
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

    /// <summary>
    /// Gets the type description.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The description shown in the windows explorer under filetype.</returns>
    public static string GetTypeDescription(this FileSystemInfo This) {
      Contract.Requires(This != null);

      var shinfo = new SHFILEINFO();
      var result = _SHGetFileInfo(This.FullName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_TYPENAME);
      return (shinfo.szTypeName.Trim());
    }

    /// <summary>
    /// Gets the icon.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="smallIcon">if set to <c>true</c> we'll return a 16x16 version; otherwise, it will be 32x32.</param>
    /// <param name="linkOverlay">if set to <c>true</c> the link overlays on shortcuts will be returned along the icon.</param>
    /// <returns>The icon used by the windows explorer for this file.</returns>
    public static Icon GetIcon(this FileSystemInfo This, bool smallIcon = false, bool linkOverlay = false) {
      Contract.Requires(This != null);

      var flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | (smallIcon ? SHGFI_SMALLICON : SHGFI_LARGEICON);
      if (linkOverlay)
        flags |= SHGFI_LINKOVERLAY;

      var shfi = new SHFILEINFO();
      _SHGetFileInfo(This.FullName, FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

      var result = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
      _DestroyIcon(shfi.hIcon);

      return (result);

    }

    /// <summary>
    /// Moves the file to the target directory.
    /// </summary>
    /// <param name="This">This FileInfo.</param>
    /// <param name="destFileName">Name of the destination file.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites any existing file; otherwise, it won't.</param>
    public static void MoveTo(this FileInfo This, string destFileName, bool overwrite) {
      This.CopyTo(destFileName, overwrite);
      This.Delete();
    }

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
    /// <param name="This">This FileInfo.</param>
    /// <returns>The result of the hash algorithm</returns>
    public static byte[] ComputeHash<THashAlgorithm>(this FileInfo This) where THashAlgorithm : HashAlgorithm, new() {
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

  }
}


