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

using System.Runtime.InteropServices;

namespace System.IO;

public static partial class FileInfoExtensions {
  /// <summary>
  ///   Provides access to native Windows API functions and constants, facilitating operations such as file system control
  ///   and shell file information retrieval.
  /// </summary>
  private static class NativeMethods {

    #region consts

    /// <summary>
    ///   Defines control codes for file system operations.
    /// </summary>
    public enum FileSystemControl {
      /// <summary>
      ///   Control code to set file or directory compression.
      /// </summary>
      SetCompression = 0x9C040,
    }

    /// <summary>
    ///   Flags for specifying the information to retrieve with <see cref="SHGetFileInfo" />.
    /// </summary>
    /// <summary>
    ///   Flags that specify the file information to retrieve with SHGetFileInfo.
    /// </summary>
    [Flags]
    public enum ShellFileInfoFlags : uint {
      /// <summary>
      ///   Add overlay icons to the file's icon. The overlay is typically used to indicate shortcuts and shared items.
      /// </summary>
      AddOverlays = 0x20,

      /// <summary>
      ///   Indicates that the caller specified file attributes (dwFileAttributes parameter of SHGetFileInfo is valid).
      /// </summary>
      AttributesSpecified = 0x20000,

      /// <summary>
      ///   Retrieve the file attributes.
      /// </summary>
      Attributes = 0x800,

      /// <summary>
      ///   Retrieve the display name of the file. The display name is the name as it appears in Windows Explorer.
      /// </summary>
      DisplayName = 0x200,

      /// <summary>
      ///   Retrieve the type of the executable file if the file is executable (e.g., .exe, .dll, .com).
      /// </summary>
      ExecutableType = 0x2000,

      /// <summary>
      ///   Retrieve the file's icon.
      /// </summary>
      Icon = 0x100,

      /// <summary>
      ///   Retrieve the location of the file's icon.
      /// </summary>
      IconLocation = 0x1000,

      /// <summary>
      ///   Retrieve the file's large icon. This is the default behavior.
      /// </summary>
      LargeIcon = 0x00,

      /// <summary>
      ///   Put a link overlay on the file's icon to indicate it's a shortcut.
      /// </summary>
      LinkOverlay = 0x8000,

      /// <summary>
      ///   Retrieve the file's open icon. An open icon is typically shown when the application associated with the file is
      ///   running.
      /// </summary>
      OpenIcon = 0x02,

      /// <summary>
      ///   Retrieve the index of the overlay icon. The overlay index is returned in the upper eight bits of the iIcon member of
      ///   the SHFILEINFO structure.
      /// </summary>
      OverlayIndex = 0x40,

      /// <summary>
      ///   Retrieve the pointer to the item identifier list (PIDL) of the file's parent folder.
      /// </summary>
      PointerToItemList = 0x08,

      /// <summary>
      ///   Modify the file's icon, indicating that the file is selected. The selection is typically shown by a background color
      ///   change.
      /// </summary>
      Selected = 0x10000,

      /// <summary>
      ///   Retrieve the shell-sized icon for the file rather than the size specified by the system metrics.
      /// </summary>
      ShellIconSize = 0x04,

      /// <summary>
      ///   Retrieve the file's small icon.
      /// </summary>
      SmallIcon = 0x01,

      /// <summary>
      ///   Retrieve the system icon index for the file. The system icon index is the index of the file's icon in the system
      ///   image list.
      /// </summary>
      SystemIconIndex = 0x4000,

      /// <summary>
      ///   Retrieve the file type name. For example, for a .txt file, the file type name is "Text Document".
      /// </summary>
      Typename = 0x400,

      /// <summary>
      ///   Indicates that the caller wants to retrieve information about a file type based on the file attributes. This flag
      ///   cannot be specified with the Icon flag.
      /// </summary>
      UseFileAttributes = 0x10,
    }


    /// <summary>
    ///   Defines file attributes for use with <see cref="SHGetFileInfo" />.
    /// </summary>
    [Flags]
    public enum FileAttribute : uint {
      Normal = 0x80,
    }

    /// <summary>
    ///   Specifies the attributes of the shell object.
    /// </summary>
    [Flags]
    public enum SFGAO : uint {
      /// <summary>
      ///   The object can be copied.
      /// </summary>
      CanCopy = 0x01,

      /// <summary>
      ///   The object can be moved.
      /// </summary>
      CanMove = 0x02,

      /// <summary>
      ///   The object can be linked.
      /// </summary>
      CanLink = 0x04,

      /// <summary>
      ///   The object supports storage.
      /// </summary>
      Storage = 0x08,

      /// <summary>
      ///   The object can be renamed.
      /// </summary>
      CanRename = 0x10,

      /// <summary>
      ///   The object can be deleted.
      /// </summary>
      CanDelete = 0x20,

      /// <summary>
      ///   The object has a property sheet.
      /// </summary>
      HasPropertySheet = 0x40,

      /// <summary>
      ///   The object is a drop target.
      /// </summary>
      IsDropTarget = 0x100,

      /// <summary>
      ///   Mask for capability flags.
      /// </summary>
      CapabilityMask = CanCopy | CanMove | CanLink | CanRename | CanDelete | HasPropertySheet | IsDropTarget,

      /// <summary>
      ///   The object is part of the operating system.
      /// </summary>
      IsSystem = 0x1000,

      /// <summary>
      ///   The object is encrypted.
      /// </summary>
      IsEncrypted = 0x2000,

      /// <summary>
      ///   Accessing the object is slow.
      /// </summary>
      IsSlow = 0x4000,

      /// <summary>
      ///   The object is ghosted.
      /// </summary>
      IsGhosted = 0x8000,

      /// <summary>
      ///   The object is a shortcut.
      /// </summary>
      IsShortcut = 0x10000,

      /// <summary>
      ///   The object is shared.
      /// </summary>
      IsShared = 0x20000,

      /// <summary>
      ///   The object is read-only.
      /// </summary>
      IsReadOnly = 0x40000,

      /// <summary>
      ///   The object is hidden.
      /// </summary>
      IsHidden = 0x80000,

      /// <summary>
      ///   The object should not be enumerated.
      /// </summary>
      IsNonEnumerated = 0x100000,

      /// <summary>
      ///   The object contains new content.
      /// </summary>
      IsNewContent = 0x200000,

      /// <summary>
      ///   The object has an associated stream.
      /// </summary>
      HasStream = 0x400000,

      /// <summary>
      ///   Indicates that the object has a storage ancestor.
      /// </summary>
      HasStorageAncestor = 0x800000,

      /// <summary>
      ///   Validates that the object is a shell item.
      /// </summary>
      Validate = 0x1000000,

      /// <summary>
      ///   The object is removable.
      /// </summary>
      IsRemovable = 0x2000000,

      /// <summary>
      ///   The object is compressed.
      /// </summary>
      IsCompressed = 0x4000000,

      /// <summary>
      ///   The object is browseable.
      /// </summary>
      IsBrowseable = 0x8000000,

      /// <summary>
      ///   The object has a filesystem ancestor.
      /// </summary>
      HasFilesystemAncestor = 0x10000000,

      /// <summary>
      ///   The object is a folder.
      /// </summary>
      IsFolder = 0x20000000,

      /// <summary>
      ///   The object is a filesystem item (file/folder).
      /// </summary>
      IsFilesystemItem = 0x40000000,

      /// <summary>
      ///   Mask for storage capability flags.
      /// </summary>
      StorageCapabilityMask = Storage | IsShortcut | IsReadOnly | HasStream | HasStorageAncestor | HasFilesystemAncestor | IsFolder | IsFilesystemItem,

      /// <summary>
      ///   The folder or object has subfolders or objects. This attribute is advisory only and does not guarantee the presence
      ///   of subfolders or objects.
      /// </summary>
      HasSubFolder = 0x80000000,
    }

    #endregion

    #region imports

    /// <summary>
    ///   Sends a control code directly to a specified device driver, causing the corresponding device to perform the
    ///   corresponding operation.
    /// </summary>
    /// <param name="hDevice">
    ///   A handle to the device on which the operation is to be performed. The device is typically a
    ///   volume, directory, file, or stream. To retrieve a device handle, use the CreateFile function.
    /// </param>
    /// <param name="dwIoControlCode">
    ///   The control code for the operation. This value identifies the specific operation to be
    ///   performed and the type of device on which to perform it.
    /// </param>
    /// <param name="lpInBuffer">
    ///   A pointer to the input buffer that contains the data required to perform the operation. The
    ///   format of this data depends on the value of the <paramref name="dwIoControlCode" /> parameter.
    /// </param>
    /// <param name="nInBufferSize">The size, in bytes, of the input buffer pointed to by <paramref name="lpInBuffer" />.</param>
    /// <param name="lpOutBuffer">
    ///   A pointer to the output buffer that is to receive the data returned by the operation. The
    ///   format of this data depends on the value of the <paramref name="dwIoControlCode" /> parameter.
    /// </param>
    /// <param name="nOutBufferSize">The size, in bytes, of the output buffer pointed to by <paramref name="lpOutBuffer" />.</param>
    /// <param name="lpBytesReturned">
    ///   A pointer to a variable that receives the size, in bytes, of the data stored into the
    ///   output buffer.
    /// </param>
    /// <param name="lpOverlapped">
    ///   A pointer to an OVERLAPPED structure. This structure is required if the
    ///   <paramref name="hDevice" /> was opened with FILE_FLAG_OVERLAPPED. If <paramref name="hDevice" /> was opened with
    ///   FILE_FLAG_OVERLAPPED, <paramref name="lpOverlapped" /> cannot be null. Otherwise, <paramref name="lpOverlapped" />
    ///   can be null.
    /// </param>
    /// <returns>
    ///   If the operation completes successfully, the return value is nonzero (true). If the operation fails or is
    ///   pending, the return value is zero (false). To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    ///   The <see cref="DeviceIoControl" /> function allows you to perform an operation on a specified device or a volume. The
    ///   specific operation is determined by <paramref name="dwIoControlCode" />, which is generally defined in the Windows
    ///   API and MSDN documentation. It's essential to ensure that the buffers passed to DeviceIoControl are appropriately
    ///   sized according to the requirements of the specific control code being used.
    ///   <para>
    ///     For certain control codes, the operation might be asynchronous. In such cases, <paramref name="lpOverlapped" />
    ///     must point to a valid OVERLAPPED structure. This structure allows for simultaneous I/O operations on the device or
    ///     file.
    ///   </para>
    ///   <para>
    ///     This method requires that the caller have the necessary privileges to communicate with the device. It's also
    ///     important that handles are opened with the correct flags (e.g., FILE_FLAG_OVERLAPPED) as required by the operation.
    ///   </para>
    /// </remarks>
    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int DeviceIoControl(
      IntPtr hDevice,
      [MarshalAs(UnmanagedType.I4)] FileSystemControl dwIoControlCode,
      ref short lpInBuffer,
      int nInBufferSize,
      IntPtr lpOutBuffer,
      int nOutBufferSize,
      out int lpBytesReturned,
      IntPtr lpOverlapped
    );

    /// <summary>
    ///   Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
    /// </summary>
    /// <param name="pszPath">The path of the file object.</param>
    /// <param name="dwFileAttributes">A combination of <see cref="FileAttribute" /> flags that specify the file attributes.</param>
    /// <param name="psfi">A pointer to a <see cref="SHFILEINFO" /> structure to receive the file information.</param>
    /// <param name="cbSizeFileInfo">
    ///   The size, in bytes, of the <see cref="SHFILEINFO" /> structure pointed to by the
    ///   <paramref name="psfi" /> parameter.
    /// </param>
    /// <param name="uFlags">The flags that specify the file information to retrieve.</param>
    /// <returns>A handle to the function that retrieves the file information.</returns>
    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
    public static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileAttribute dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, [MarshalAs(UnmanagedType.U4)] ShellFileInfoFlags uFlags);

    #endregion

    #region nested types

    /// <summary>
    ///   Contains information about a file object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO {
      private const int _MAX_TYPE_NAME_SIZE = 80;
      private const int _MAX_DISPLAY_NAME_SIZE = 260;

      /// <summary>
      ///   A handle to the icon that represents the file.
      /// </summary>
      public IntPtr hIcon;

      /// <summary>
      ///   The index of the icon image within the system image list.
      /// </summary>
      public int iIcon;

      /// <summary>
      ///   The attributes of the file.
      /// </summary>
      public SFGAO dwAttributes;

      /// <summary>
      ///   The display name of the file as it appears in the Windows shell, or the path and file name of the file that contains
      ///   the icon representing the file.
      /// </summary>
      /// <remarks>
      ///   This string is of a fixed size (MAX_PATH, 260 characters). If the path or file name is longer than this limit, it is
      ///   truncated.
      /// </remarks>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_DISPLAY_NAME_SIZE)]
      public string szDisplayName;

      /// <summary>
      ///   The string that describes the file's type.
      /// </summary>
      /// <remarks>
      ///   This string is of a fixed size (80 characters). If the file type description is longer than this limit, it is
      ///   truncated.
      /// </remarks>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_TYPE_NAME_SIZE)]
      public string szTypeName;
    }

    #endregion

  }
}
