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

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using Guard;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace System.IO;

public static partial class FileInfoExtensions {
  /// <summary>
  ///   The native methods.
  /// </summary>
  private static class NativeMethods {
    #region consts

    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
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

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
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

      StorageCapabilityMask = Storage | IsShortcut | IsReadOnly | HasStream | HasStorageAncestor |
                              HasFilesystemAncestor | IsFolder | IsFilesystemItem,
      HasSubFolder = 0x80000000,
    }

    #endregion

    #region imports

    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
    public static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileAttribute dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, [MarshalAs(UnmanagedType.U4)] ShellFileInfoFlags uFlags);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "DestroyIcon")]
    public static extern bool DestroyIcon(IntPtr hIcon);

    #endregion

    #region nested types

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
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
  }

  /// <summary>
  ///   Gets the icon.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  /// <param name="smallIcon">if set to <c>true</c> we'll return a 16x16 version; otherwise, it will be 32x32.</param>
  /// <param name="linkOverlay">if set to <c>true</c> the link overlays on shortcuts will be returned along the icon.</param>
  /// <returns>The icon used by the windows explorer for this file.</returns>
  public static Icon GetIcon(this FileInfo @this, bool smallIcon = false, bool linkOverlay = false) {
    Against.ThisIsNull(@this);

    var flags = NativeMethods.ShellFileInfoFlags.Icon | NativeMethods.ShellFileInfoFlags.UseFileAttributes |
                (smallIcon ? NativeMethods.ShellFileInfoFlags.SmallIcon : NativeMethods.ShellFileInfoFlags.LargeIcon);
    if (linkOverlay)
      flags |= NativeMethods.ShellFileInfoFlags.LinkOverlay;

    var shfi = new NativeMethods.SHFILEINFO();
    NativeMethods.SHGetFileInfo(@this.FullName, NativeMethods.FileAttribute.Normal, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

    var result = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
    NativeMethods.DestroyIcon(shfi.hIcon);

    return result;
  }
}
