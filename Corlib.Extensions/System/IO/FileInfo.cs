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
  /// Provides access to native Windows API functions and constants, facilitating operations such as file system control and shell file information retrieval.
  /// </summary>
  private static class NativeMethods {
    // ReSharper disable UnusedMember.Local
    // ReSharper disable MemberCanBePrivate.Local
    // ReSharper disable InconsistentNaming

    #region consts

    /// <summary>
    /// Defines control codes for file system operations.
    /// </summary>
    public enum FileSystemControl {
      /// <summary>
      /// Control code to set file or directory compression.
      /// </summary>
      SetCompression = 0x9C040,
    }

    /// <summary>
    /// Flags for specifying the information to retrieve with <see cref="SHGetFileInfo"/>.
    /// </summary>
    /// <summary>
    /// Flags that specify the file information to retrieve with SHGetFileInfo.
    /// </summary>
    [Flags]
    public enum ShellFileInfoFlags : uint {
      /// <summary>
      /// Add overlay icons to the file's icon. The overlay is typically used to indicate shortcuts and shared items.
      /// </summary>
      AddOverlays = 0x20,

      /// <summary>
      /// Indicates that the caller specified file attributes (dwFileAttributes parameter of SHGetFileInfo is valid).
      /// </summary>
      AttributesSpecified = 0x20000,

      /// <summary>
      /// Retrieve the file attributes.
      /// </summary>
      Attributes = 0x800,

      /// <summary>
      /// Retrieve the display name of the file. The display name is the name as it appears in Windows Explorer.
      /// </summary>
      DisplayName = 0x200,

      /// <summary>
      /// Retrieve the type of the executable file if the file is executable (e.g., .exe, .dll, .com).
      /// </summary>
      ExecutableType = 0x2000,

      /// <summary>
      /// Retrieve the file's icon.
      /// </summary>
      Icon = 0x100,

      /// <summary>
      /// Retrieve the location of the file's icon.
      /// </summary>
      IconLocation = 0x1000,

      /// <summary>
      /// Retrieve the file's large icon. This is the default behavior.
      /// </summary>
      LargeIcon = 0x00,

      /// <summary>
      /// Put a link overlay on the file's icon to indicate it's a shortcut.
      /// </summary>
      LinkOverlay = 0x8000,

      /// <summary>
      /// Retrieve the file's open icon. An open icon is typically shown when the application associated with the file is running.
      /// </summary>
      OpenIcon = 0x02,

      /// <summary>
      /// Retrieve the index of the overlay icon. The overlay index is returned in the upper eight bits of the iIcon member of the SHFILEINFO structure.
      /// </summary>
      OverlayIndex = 0x40,

      /// <summary>
      /// Retrieve the pointer to the item identifier list (PIDL) of the file's parent folder.
      /// </summary>
      PointerToItemList = 0x08,

      /// <summary>
      /// Modify the file's icon, indicating that the file is selected. The selection is typically shown by a background color change.
      /// </summary>
      Selected = 0x10000,

      /// <summary>
      /// Retrieve the shell-sized icon for the file rather than the size specified by the system metrics.
      /// </summary>
      ShellIconSize = 0x04,

      /// <summary>
      /// Retrieve the file's small icon.
      /// </summary>
      SmallIcon = 0x01,

      /// <summary>
      /// Retrieve the system icon index for the file. The system icon index is the index of the file's icon in the system image list.
      /// </summary>
      SystemIconIndex = 0x4000,

      /// <summary>
      /// Retrieve the file type name. For example, for a .txt file, the file type name is "Text Document".
      /// </summary>
      Typename = 0x400,

      /// <summary>
      /// Indicates that the caller wants to retrieve information about a file type based on the file attributes. This flag cannot be specified with the Icon flag.
      /// </summary>
      UseFileAttributes = 0x10,
    }


    /// <summary>
    /// Defines file attributes for use with <see cref="SHGetFileInfo"/>.
    /// </summary>
    [Flags]
    public enum FileAttribute : uint {
      Normal = 0x80,
    }

    /// <summary>
    /// Specifies the attributes of the shell object.
    /// </summary>
    [Flags]
    public enum SFGAO : uint {
      /// <summary>
      /// The object can be copied.
      /// </summary>
      CanCopy = 0x01,

      /// <summary>
      /// The object can be moved.
      /// </summary>
      CanMove = 0x02,

      /// <summary>
      /// The object can be linked.
      /// </summary>
      CanLink = 0x04,

      /// <summary>
      /// The object supports storage.
      /// </summary>
      Storage = 0x08,

      /// <summary>
      /// The object can be renamed.
      /// </summary>
      CanRename = 0x10,

      /// <summary>
      /// The object can be deleted.
      /// </summary>
      CanDelete = 0x20,

      /// <summary>
      /// The object has a property sheet.
      /// </summary>
      HasPropertySheet = 0x40,

      /// <summary>
      /// The object is a drop target.
      /// </summary>
      IsDropTarget = 0x100,

      /// <summary>
      /// Mask for capability flags.
      /// </summary>
      CapabilityMask = CanCopy | CanMove | CanLink | CanRename | CanDelete | HasPropertySheet | IsDropTarget,

      /// <summary>
      /// The object is part of the operating system.
      /// </summary>
      IsSystem = 0x1000,

      /// <summary>
      /// The object is encrypted.
      /// </summary>
      IsEncrypted = 0x2000,

      /// <summary>
      /// Accessing the object is slow.
      /// </summary>
      IsSlow = 0x4000,

      /// <summary>
      /// The object is ghosted.
      /// </summary>
      IsGhosted = 0x8000,

      /// <summary>
      /// The object is a shortcut.
      /// </summary>
      IsShortcut = 0x10000,

      /// <summary>
      /// The object is shared.
      /// </summary>
      IsShared = 0x20000,

      /// <summary>
      /// The object is read-only.
      /// </summary>
      IsReadOnly = 0x40000,

      /// <summary>
      /// The object is hidden.
      /// </summary>
      IsHidden = 0x80000,

      /// <summary>
      /// The object should not be enumerated.
      /// </summary>
      IsNonEnumerated = 0x100000,

      /// <summary>
      /// The object contains new content.
      /// </summary>
      IsNewContent = 0x200000,

      /// <summary>
      /// The object has an associated stream.
      /// </summary>
      HasStream = 0x400000,

      /// <summary>
      /// Indicates that the object has a storage ancestor.
      /// </summary>
      HasStorageAncestor = 0x800000,

      /// <summary>
      /// Validates that the object is a shell item.
      /// </summary>
      Validate = 0x1000000,

      /// <summary>
      /// The object is removable.
      /// </summary>
      IsRemovable = 0x2000000,

      /// <summary>
      /// The object is compressed.
      /// </summary>
      IsCompressed = 0x4000000,

      /// <summary>
      /// The object is browseable.
      /// </summary>
      IsBrowseable = 0x8000000,

      /// <summary>
      /// The object has a filesystem ancestor.
      /// </summary>
      HasFilesystemAncestor = 0x10000000,

      /// <summary>
      /// The object is a folder.
      /// </summary>
      IsFolder = 0x20000000,

      /// <summary>
      /// The object is a filesystem item (file/folder).
      /// </summary>
      IsFilesystemItem = 0x40000000,

      /// <summary>
      /// Mask for storage capability flags.
      /// </summary>
      StorageCapabilityMask = Storage | IsShortcut | IsReadOnly | HasStream | HasStorageAncestor | HasFilesystemAncestor | IsFolder | IsFilesystemItem,

      /// <summary>
      /// The folder or object has subfolders or objects. This attribute is advisory only and does not guarantee the presence of subfolders or objects.
      /// </summary>
      HasSubFolder = 0x80000000,
    }

    #endregion

    #region imports

    /// <summary>
    /// Sends a control code directly to a specified device driver, causing the corresponding device to perform the corresponding operation.
    /// </summary>
    /// <param name="hDevice">A handle to the device on which the operation is to be performed. The device is typically a volume, directory, file, or stream. To retrieve a device handle, use the CreateFile function.</param>
    /// <param name="dwIoControlCode">The control code for the operation. This value identifies the specific operation to be performed and the type of device on which to perform it.</param>
    /// <param name="lpInBuffer">A pointer to the input buffer that contains the data required to perform the operation. The format of this data depends on the value of the <paramref name="dwIoControlCode"/> parameter.</param>
    /// <param name="nInBufferSize">The size, in bytes, of the input buffer pointed to by <paramref name="lpInBuffer"/>.</param>
    /// <param name="lpOutBuffer">A pointer to the output buffer that is to receive the data returned by the operation. The format of this data depends on the value of the <paramref name="dwIoControlCode"/> parameter.</param>
    /// <param name="nOutBufferSize">The size, in bytes, of the output buffer pointed to by <paramref name="lpOutBuffer"/>.</param>
    /// <param name="lpBytesReturned">A pointer to a variable that receives the size, in bytes, of the data stored into the output buffer.</param>
    /// <param name="lpOverlapped">A pointer to an OVERLAPPED structure. This structure is required if the <paramref name="hDevice"/> was opened with FILE_FLAG_OVERLAPPED. If <paramref name="hDevice"/> was opened with FILE_FLAG_OVERLAPPED, <paramref name="lpOverlapped"/> cannot be null. Otherwise, <paramref name="lpOverlapped"/> can be null.</param>
    /// <returns>If the operation completes successfully, the return value is nonzero (true). If the operation fails or is pending, the return value is zero (false). To get extended error information, call GetLastError.</returns>
    /// <remarks>
    /// The <see cref="DeviceIoControl"/> function allows you to perform an operation on a specified device or a volume. The specific operation is determined by <paramref name="dwIoControlCode"/>, which is generally defined in the Windows API and MSDN documentation. It's essential to ensure that the buffers passed to DeviceIoControl are appropriately sized according to the requirements of the specific control code being used.
    /// <para>
    /// For certain control codes, the operation might be asynchronous. In such cases, <paramref name="lpOverlapped"/> must point to a valid OVERLAPPED structure. This structure allows for simultaneous I/O operations on the device or file.
    /// </para>
    /// <para>
    /// This method requires that the caller have the necessary privileges to communicate with the device. It's also important that handles are opened with the correct flags (e.g., FILE_FLAG_OVERLAPPED) as required by the operation.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
    /// </summary>
    /// <param name="pszPath">The path of the file object.</param>
    /// <param name="dwFileAttributes">A combination of <see cref="FileAttribute"/> flags that specify the file attributes.</param>
    /// <param name="psfi">A pointer to a <see cref="SHFILEINFO"/> structure to receive the file information.</param>
    /// <param name="cbSizeFileInfo">The size, in bytes, of the <see cref="SHFILEINFO"/> structure pointed to by the <paramref name="psfi"/> parameter.</param>
    /// <param name="uFlags">The flags that specify the file information to retrieve.</param>
    /// <returns>A handle to the function that retrieves the file information.</returns>

    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
    public static extern IntPtr SHGetFileInfo(string pszPath, [MarshalAs(UnmanagedType.U4)] FileAttribute dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, [MarshalAs(UnmanagedType.U4)] ShellFileInfoFlags uFlags);

    #endregion

    #region nested types

    /// <summary>
    /// Contains information about a file object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO {
      private const int _MAX_TYPE_NAME_SIZE = 80;
      private const int _MAX_DISPLAY_NAME_SIZE = 260;

      /// <summary>
      /// A handle to the icon that represents the file.
      /// </summary>
      public IntPtr hIcon;

      /// <summary>
      /// The index of the icon image within the system image list.
      /// </summary>
      public int iIcon;

      /// <summary>
      /// The attributes of the file.
      /// </summary>
      public SFGAO dwAttributes;

      /// <summary>
      /// The display name of the file as it appears in the Windows shell, or the path and file name of the file that contains the icon representing the file.
      /// </summary>
      /// <remarks>
      /// This string is of a fixed size (MAX_PATH, 260 characters). If the path or file name is longer than this limit, it is truncated.
      /// </remarks>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_DISPLAY_NAME_SIZE)]
      public string szDisplayName;

      /// <summary>
      /// The string that describes the file's type.
      /// </summary>
      /// <remarks>
      /// This string is of a fixed size (80 characters). If the file type description is longer than this limit, it is truncated.
      /// </remarks>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = _MAX_TYPE_NAME_SIZE)]
      public string szTypeName;

    }

#endregion

    // ReSharper restore InconsistentNaming
    // ReSharper restore MemberCanBePrivate.Local
    // ReSharper restore UnusedMember.Local
  }

#region consts

  private static Encoding _utf8NoBom;

  /// <summary>
  /// Gets a UTF-8 encoding instance without a Byte Order Mark (BOM).
  /// </summary>
  /// <remarks>
  /// This property provides a thread-safe, lazy-initialized UTF-8 encoding object that does not emit a Byte Order Mark (BOM).
  /// It is useful for text operations requiring UTF-8 encoding format without the presence of a BOM, such as generating text files
  /// that are compliant with systems or specifications that do not recognize or require a BOM.
  /// </remarks>
  /// <value>
  /// A <see cref="System.Text.UTF8Encoding"/> instance configured to not emit a Byte Order Mark (BOM).
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
  /// Enables NTFS file compression on the specified file.
  /// </summary>
  /// <param name="this">The file on which to enable compression.</param>
  /// <exception cref="ArgumentNullException">Thrown if the file object is null.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <exception cref="Win32Exception">Thrown if the compression operation fails.</exception>
  /// <example>
  /// Here is how to use <see cref="EnableCompression"/> to compress a file:
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// fileInfo.EnableCompression();
  /// </code>
  /// This example compresses "file.txt" using NTFS compression.
  /// </example>
  public static void EnableCompression(this FileInfo @this) {
    Against.ThisIsNull(@this);

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
        out _,
        IntPtr.Zero
      );
    }
    if (result < 0)
      throw new Win32Exception();

  }

  /// <summary>
  /// Attempts to enable NTFS file compression on the specified <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The file on which to attempt to enable compression.</param>
  /// <returns><see langword="true"/> if compression was successfully enabled; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo(@"path\to\your\file.txt");
  /// bool isCompressionEnabled = fileInfo.TryEnableCompression();
  /// Console.WriteLine(isCompressionEnabled ? "Compression enabled." : "Compression not enabled.");
  /// </code>
  /// This example demonstrates checking if NTFS file compression can be enabled for a specified file, 
  /// and prints the outcome.
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
  /// Retrieves the description of the file type for the specified <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file for which to retrieve the type description.</param>
  /// <returns>A <see cref="string"/> containing the description of the file type, such as "Text Document" for a .txt file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the provided <see cref="FileInfo"/> instance is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo(@"C:\path\to\file.txt");
  /// string fileTypeDescription = fileInfo.GetTypeDescription();
  /// Console.WriteLine($"The file type is: {fileTypeDescription}");
  /// </code>
  /// This example retrieves the file type description of "file.txt" and prints it to the console.
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
  /// Copies the specified <see cref="FileInfo"/> instance to the specified target <see cref="DirectoryInfo"/>, maintaining the original file name.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to copy.</param>
  /// <param name="targetDirectory">The target <see cref="DirectoryInfo"/> where the file should be copied.</param>
  /// <remarks>
  /// This method copies the file to the target directory without overwriting existing files with the same name.
  /// If a file with the same name already exists in the target directory, this method will throw an <see cref="IOException"/>.
  /// </remarks>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target directory is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists in the target directory.</exception>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\document.txt");
  /// DirectoryInfo targetDir = new DirectoryInfo(@"C:\target");
  /// sourceFile.CopyTo(targetDir);
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  /// This example demonstrates copying a file from a source directory to a target directory.
  /// </example>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), false)
  ;

  /// <summary>
  /// Copies the specified <see cref="FileInfo"/> instance to the specified target <see cref="DirectoryInfo"/>, 
  /// maintaining the original file name, with an option to overwrite the existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to copy.</param>
  /// <param name="targetDirectory">The target <see cref="DirectoryInfo"/> where the file should be copied.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file with the same name. 
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown 
  /// if a file with the same name already exists.</param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target directory is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists in the target directory and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\document.txt");
  /// DirectoryInfo targetDir = new DirectoryInfo(@"C:\target");
  /// sourceFile.CopyTo(targetDir, true); // Overwrite existing file if it exists
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  /// This example demonstrates copying a file from a source directory to a target directory with the option to overwrite existing files.
  /// </example>
  public static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)
    => @this.CopyTo(Path.Combine(targetDirectory.FullName, @this.Name), overwrite)
  ;

  /// <summary>
  /// Copies the specified <see cref="FileInfo"/> instance to the location specified by a target <see cref="FileInfo"/> object, 
  /// without overwriting an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to copy.</param>
  /// <param name="targetFile">The target <see cref="FileInfo"/> object that specifies the destination path and name of the file.</param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target file is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists at the target location.</exception>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo targetFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.CopyTo(targetFile);
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  /// This example demonstrates copying a file from one location to another without overwriting an existing file at the destination.
  /// </example>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile)
    => @this.CopyTo(targetFile.FullName, false)
  ;

  /// <summary>
  /// Copies the specified <see cref="FileInfo"/> instance to the location specified by a target <see cref="FileInfo"/> object,
  /// with an option to overwrite an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to copy.</param>
  /// <param name="targetFile">The target <see cref="FileInfo"/> object that specifies the destination path and name of the file.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file with the same name at the target location.
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown
  /// if a file with the same name already exists.</param>
  /// <exception cref="ArgumentNullException">Thrown if the source file or target file is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if a file with the same name already exists at the target location and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo targetFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.CopyTo(targetFile, true); // Overwrite existing file if it exists
  /// Console.WriteLine("File copied successfully.");
  /// </code>
  /// This example demonstrates copying a file from one location to another with the option to overwrite an existing file at the destination.
  /// </example>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite)
    => @this.CopyTo(targetFile.FullName, overwrite)
  ;

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location represented by a <see cref="FileInfo"/> object, without overwriting an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo"/> object that represents the destination file.</param>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.MoveTo(destFile);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo"/> objects, without the risk of overwriting an existing file at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile) => @this.MoveTo(destFile.FullName, false);

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location represented by a <see cref="FileInfo"/> object, without overwriting an existing file,
  /// and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo"/> object that represents the destination file.</param>
  /// <param name="timeout">The maximum <see cref="TimeSpan"/> to retry the deletion of the source file if it is locked or cannot be deleted immediately.</param>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destFile, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo"/> objects, without overwriting an existing file
  /// at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, TimeSpan timeout) => @this.MoveTo(destFile.FullName, false, timeout);

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location represented by a <see cref="FileInfo"/> object, with an option to overwrite an existing file.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo"/> object that represents the destination file.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file at the destination.
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown
  /// if a file with the same name already exists at the destination.</param>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// sourceFile.MoveTo(destFile, true);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo"/> objects, with the option to overwrite an existing file
  /// at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite) => @this.MoveTo(destFile.FullName, overwrite);

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location represented by a <see cref="FileInfo"/> object, with an option to overwrite an existing file,
  /// and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFile">The target <see cref="FileInfo"/> object that represents the destination file.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file at the destination.
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown
  /// if a file with the same name already exists at the destination.</param>
  /// <param name="timeout">The maximum <see cref="TimeSpan"/> to retry the deletion of the source file if it is locked or cannot be deleted immediately.</param>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// FileInfo destFile = new FileInfo(@"D:\destination\example.txt");
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destFile, true, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, represented by <see cref="FileInfo"/> objects, with the option to overwrite an existing file
  /// at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite, TimeSpan timeout) => @this.MoveTo(destFile.FullName, overwrite, timeout);

#if !SUPPORTS_MOVETO_OVERWRITE

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location with an option to overwrite an existing file,
  /// using a default timeout period for retrying the deletion of the source file if it is locked or cannot be deleted immediately.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFileName">The path to the destination file. This cannot be a directory.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file at the destination.
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown
  /// if a file with the same name already exists at the destination.</param>
  /// <remarks>
  /// This method delegates to <see cref="MoveTo(FileInfo, string, bool, TimeSpan)"/>, specifying a default timeout of 30 seconds
  /// for retrying the deletion of the source file.
  /// </remarks>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// string destinationPath = @"D:\destination\example.txt";
  /// sourceFile.MoveTo(destinationPath, true);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, with the option to overwrite an existing file
  /// at the destination and a default timeout of 30 seconds for retrying deletion of the source file if necessary.
  /// </example>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite) => MoveTo(@this, destFileName, overwrite, TimeSpan.FromSeconds(30));

#endif

  /// <summary>
  /// Moves the specified <see cref="FileInfo"/> instance to a new location with an option to overwrite an existing file,
  /// and retries deletion of the source file within a specified timeout period if necessary.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo"/> object to move.</param>
  /// <param name="destFileName">The path to the destination file. This cannot be a directory.</param>
  /// <param name="overwrite">A <see langword="bool"/> indicating whether to overwrite an existing file at the destination.
  /// If <see langword="true"/>, the file will be overwritten; if <see langword="false"/>, an <see cref="IOException"/> will be thrown
  /// if a file with the same name already exists at the destination.</param>
  /// <param name="timeout">The maximum <see cref="TimeSpan"/> to retry the deletion of the source file if it is locked or cannot be deleted immediately.</param>
  /// <exception cref="ArgumentNullException">Thrown if the source file is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if the file cannot be moved, typically because the source or destination cannot be accessed,
  /// or the deletion of the source file exceeds the specified timeout.</exception>
  /// <example>
  /// <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// string destinationPath = @"D:\destination\example.txt";
  /// TimeSpan timeout = TimeSpan.FromSeconds(5);
  /// sourceFile.MoveTo(destinationPath, true, timeout);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  /// This example demonstrates moving a file from one location to another, with the option to overwrite an existing file
  /// at the destination and retrying the deletion of the source file for up to 5 seconds if it fails initially.
  /// </example>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite, TimeSpan timeout) {
    Against.ThisIsNull(@this);

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

  /// <summary>
  /// Computes and returns the hash of the file represented by the <see cref="FileInfo"/> instance using the specified hash algorithm.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm to use, derived from <see cref="HashAlgorithm"/>.</typeparam>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the hash for.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeHash&lt;SHA256Managed&gt;();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this) where THashAlgorithm : HashAlgorithm, new() {
    Against.ThisIsNull(@this);

    using THashAlgorithm provider = new();
    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }

  /// <summary>
  /// Computes the hash of the file represented by the <see cref="FileInfo"/> instance using the specified hash algorithm and block size.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm to use, derived from <see cref="HashAlgorithm"/>.</typeparam>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the hash for.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize"/> is less than or equal to zero.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeHash&lt;SHA256Managed&gt;(1024);
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-256 hash of "file.txt" using a block size of 1024 bytes and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this, int blockSize) where THashAlgorithm : HashAlgorithm, new() {
    Against.ThisIsNull(@this);

    using THashAlgorithm provider = new();
    return ComputeHash(@this, provider, blockSize);
  }

  /// <summary>
  /// Computes the hash of the file represented by the <see cref="FileInfo"/> instance using the specified hash algorithm.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the hash for.</param>
  /// <param name="provider">The hash algorithm provider used to compute the file hash.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object or the hash algorithm provider is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// using (var sha256 = SHA256.Create())
  /// {
  ///     byte[] hash = fileInfo.ComputeHash(sha256);
  ///     Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// }
  /// </code>
  /// This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(provider);

    using FileStream stream = new(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
    return provider.ComputeHash(stream);
  }

  /// <summary>
  /// Computes the hash of the file represented by the <see cref="FileInfo"/> instance using the specified hash algorithm and block size.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the hash for.</param>
  /// <param name="provider">The hash algorithm provider used to compute the file hash.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the computed hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object or the hash algorithm provider is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize"/> is less than or equal to zero.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// using (var sha256 = SHA256.Create())
  /// {
  ///     byte[] hash = fileInfo.ComputeHash(sha256, 1024);
  ///     Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// }
  /// </code>
  /// This example computes the SHA-256 hash of "file.txt" using a block size of 1024 bytes and prints the hash in hexadecimal format.
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
  /// Computes the SHA-512 hash of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the SHA-512 hash for.</param>
  /// <returns>A byte array containing the SHA-512 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA512Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-512 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA512Hash(this FileInfo @this) {
    using var provider=SHA512.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Computes the SHA-512 hash of the file represented by the <see cref="FileInfo"/> instance using a specified block size.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the SHA-512 hash for.</param>
  /// <param name="blockSize">The size of each block of data to read from the file at a time, in bytes.</param>
  /// <returns>A byte array containing the SHA-512 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blockSize"/> is less than or equal to zero.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA512Hash(1024);
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-512 hash of "file.txt" using a block size of 1024 bytes and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA512Hash(this FileInfo @this, int blockSize) {
    using var provider = SHA512.Create();
    return @this.ComputeHash(provider,blockSize);
  }

  /// <summary>
  /// Computes the SHA-384 hash of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the SHA-384 hash for.</param>
  /// <returns>A byte array containing the SHA-384 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA384Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-384 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA384Hash(this FileInfo @this) {
    using var provider = SHA384.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Computes the SHA-256 hash of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the SHA-256 hash for.</param>
  /// <returns>A byte array containing the SHA-256 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA256Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-256 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA256Hash(this FileInfo @this) {
    using var provider = SHA256.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Computes the SHA-1 hash of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the SHA-1 hash for.</param>
  /// <returns>A byte array containing the SHA-1 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeSHA1Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  /// This example computes the SHA-1 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeSHA1Hash(this FileInfo @this) {
    using var provider = SHA1.Create();
    return @this.ComputeHash(provider);
  }

  /// <summary>
  /// Computes the MD5 hash of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file to compute the MD5 hash for.</param>
  /// <returns>A byte array containing the MD5 hash of the file.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the file object is <see langword="null"/>.</exception>
  /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("path/to/your/file.txt");
  /// byte[] hash = fileInfo.ComputeMD5Hash();
  /// Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant());
  /// </code>
  /// This example computes the MD5 hash of "file.txt" and prints the hash in hexadecimal format.
  /// </example>
  public static byte[] ComputeMD5Hash(this FileInfo @this) {
    using var provider = MD5.Create();
    return @this.ComputeHash(provider);
  }

  #endregion

  #region reading

  /// <summary>
  /// Determines the encoding used in the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>The detected <see cref="Encoding"/> of the file or <see lanword="null"/> when the file is empty.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// Encoding encoding = file.GetEncoding();
  /// Console.WriteLine($"File encoding: {encoding.EncodingName}");
  /// </code>
  /// This example determines and prints the encoding of "example.txt".
  /// </example>
  public static Encoding GetEncoding(this FileInfo @this) {
    // Read the BOM
    var bom = new byte[4];
    using (FileStream file = new(@this.FullName, FileMode.Open, FileAccess.Read)) {
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
  /// Reads all text from the file represented by the <see cref="FileInfo"/> instance using the detected encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>A <see cref="string"/> containing all the text from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// string content = file.ReadAllText();
  /// Console.WriteLine(content);
  /// </code>
  /// This example reads and prints the content of "example.txt" using the detected encoding.
  /// </example>
  public static string ReadAllText(this FileInfo @this) => File.ReadAllText(@this.FullName);

  /// <summary>
  /// Reads all text from the file represented by the <see cref="FileInfo"/> instance using the specified <see cref="Encoding"/>.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding"/> to use when reading the file.</param>
  /// <returns>A <see cref="string"/> containing all the text from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// string content = file.ReadAllText(Encoding.UTF8);
  /// Console.WriteLine(content);
  /// </code>
  /// This example reads and prints the content of "example.txt" using UTF-8 encoding.
  /// </example>
  public static string ReadAllText(this FileInfo @this, Encoding encoding) => File.ReadAllText(@this.FullName, encoding);

  /// <summary>
  /// Reads all lines from the file represented by the <see cref="FileInfo"/> instance using the detected encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>An array of <see cref="string"/> containing all the lines from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// foreach (var line in file.ReadAllLines())
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example reads and prints each line of "example.txt" using the detected encoding.
  /// </example>
  public static string[] ReadAllLines(this FileInfo @this) => File.ReadAllLines(@this.FullName);

  /// <summary>
  /// Tries to read all lines from the file represented by the <see cref="FileInfo"/> instance using the detected encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="result">When this method returns, contains an array of <see cref="string"/> containing all the lines from the file if the read was successful, or <see langword="null"/> if it fails.</param>
  /// <returns><see langword="true"/> if the lines were successfully read; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
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
  /// This example attempts to read and print each line of "example.txt". If unsuccessful, it prints a failure message.
  /// </example>
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
  /// Reads all lines from the file represented by the <see cref="FileInfo"/> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding"/> to use when reading the file.</param>
  /// <returns>An array of <see cref="string"/> containing all the lines from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.txt");
  /// foreach (var line in file.ReadAllLines(Encoding.UTF8))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example reads and prints each line of "example.txt" using UTF-8 encoding.
  /// </example>
  public static string[] ReadAllLines(this FileInfo @this, Encoding encoding) => File.ReadAllLines(@this.FullName, encoding);

  /// <summary>
  /// Attempts to read all lines from the file represented by the <see cref="FileInfo"/> instance using the specified <see cref="Encoding"/>.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding"/> to use for reading the file.</param>
  /// <param name="result">When this method returns, contains an array of <see cref="string"/> containing all the lines from the file if the read was successful, or <see langword="null"/> if it fails.</param>
  /// <returns><see langword="true"/> if the lines were successfully read; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
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
  /// This example attempts to read and print each line of "example.txt" using UTF-8 encoding. If unsuccessful, it prints a failure message.
  /// </example>
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
  /// Reads bytes from the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>An <see cref="IEnumerable{Byte}"/> containing all bytes from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.bin");
  /// IEnumerable&lt;byte&gt; bytes = file.ReadBytes();
  /// foreach (byte b in bytes)
  /// {
  ///     Console.WriteLine(b);
  /// }
  /// </code>
  /// This example reads and prints each byte of "example.bin".
  /// </example>
  public static IEnumerable<byte> ReadBytes(this FileInfo @this) {
    using var stream = @this.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
    while (!stream.IsAtEndOfStream())
      yield return (byte)stream.ReadByte();
  }

  /// <summary>
  /// Reads all bytes from the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>A byte array containing all bytes from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo file = new FileInfo("example.bin");
  /// byte[] bytes = file.ReadAllBytes();
  /// Console.WriteLine(BitConverter.ToString(bytes));
  /// </code>
  /// This example reads all bytes from "example.bin" and prints them as a hexadecimal string.
  /// </example>
  public static byte[] ReadAllBytes(this FileInfo @this) => File.ReadAllBytes(@this.FullName);

  /// <summary>
  /// Attempts to read all bytes from the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="result">When this method returns, contains the byte array of all bytes from the file if the read was successful, or <see langword="null"/> if it fails.</param>
  /// <returns><see langword="true"/> if the bytes were successfully read; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
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
  /// This example attempts to read all bytes from "example.bin" and prints them as a hexadecimal string. If unsuccessful, it prints a failure message.
  /// </example>
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
  /// Reads lines from the file represented by the <see cref="FileInfo"/> instance using the default encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <returns>An <see cref="IEnumerable{String}"/> of lines read from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// foreach (var line in fileInfo.ReadLines())
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example demonstrates how to enumerate through each line of "example.txt".
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this) => File.ReadLines(@this.FullName);
#else
  public static IEnumerable<string> ReadLines(this FileInfo @this) => ReadLines(@this, FileShare.Read);
#endif

  /// <summary>
  /// Reads lines from the file represented by the <see cref="FileInfo"/> instance using the default encoding and specified <see cref="FileShare"/> mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="share">The <see cref="FileShare"/> mode to use when opening the file.</param>
  /// <returns>An <see cref="IEnumerable{String}"/> of lines read from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("shared_example.txt");
  /// foreach (var line in fileInfo.ReadLines(FileShare.Read))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example reads lines from "shared_example.txt", allowing other processes to read the file simultaneously.
  /// </example>
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

  /// <summary>
  /// Reads lines from the file represented by the <see cref="FileInfo"/> instance using the specified encoding and <see cref="FileShare"/> mode.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding"/> to use for reading the file.</param>
  /// <param name="share">The <see cref="FileShare"/> mode to use when opening the file.</param>
  /// <returns>An <see cref="IEnumerable{String}"/> of lines read from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("shared_example.txt");
  /// foreach (var line in fileInfo.ReadLines(Encoding.UTF8, FileShare.Read))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example reads lines from "shared_example.txt" using UTF-8 encoding, allowing other processes to read the file simultaneously.
  /// </example>
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
  /// Reads lines from the file represented by the <see cref="FileInfo"/> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="encoding">The <see cref="Encoding"/> to use for reading the file.</param>
  /// <returns>An <see cref="IEnumerable{String}"/> of lines read from the file.</returns>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// foreach (var line in fileInfo.ReadLines(Encoding.UTF8))
  /// {
  ///     Console.WriteLine(line);
  /// }
  /// </code>
  /// This example reads lines from "example.txt" using UTF-8 encoding.
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => File.ReadLines(@this.FullName, encoding);
#else
  public static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding) => ReadLines(@this, encoding,FileShare.Read);
#endif

  #endregion

  #region writing

  /// <summary>
  /// Writes a string to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The string to write to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.WriteAllText("Hello World");
  /// Console.WriteLine("Text written successfully.");
  /// </code>
  /// This example writes "Hello World" to "example.txt", overwriting any existing content.
  /// </example>
  public static void WriteAllText(this FileInfo @this, string contents) => File.WriteAllText(@this.FullName, contents);

  /// <summary>
  /// Writes a string to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The string to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the string.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.WriteAllText("Hello World", Encoding.UTF8);
  /// Console.WriteLine("Text written successfully with UTF-8 encoding.");
  /// </code>
  /// This example writes "Hello World" to "example.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
  public static void WriteAllText(this FileInfo @this, string contents, Encoding encoding) => File.WriteAllText(@this.FullName, contents, encoding);

  /// <summary>
  /// Writes an array of strings to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The string array to write to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// string[] lines = { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines);
  /// Console.WriteLine("Lines written successfully.");
  /// </code>
  /// This example writes two lines to "lines.txt", overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, string[] contents) => File.WriteAllLines(@this.FullName, contents);

  /// <summary>
  /// Writes an array of strings to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The string array to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the strings.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// string[] lines = { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines written successfully with UTF-8 encoding.");
  /// </code>
  /// This example writes two lines to "lines.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
  public static void WriteAllLines(this FileInfo @this, string[] contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents, encoding);

  /// <summary>
  /// Writes a byte array to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="bytes">The byte array to write to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.bin");
  /// byte[] data = { 0x00, 0x0F, 0xF0 };
  /// fileInfo.WriteAllBytes(data);
  /// Console.WriteLine("Bytes written successfully.");
  /// </code>
  /// This example writes a byte array to "example.bin", overwriting any existing content.
  /// </example>
  public static void WriteAllBytes(this FileInfo @this, byte[] bytes) => File.WriteAllBytes(@this.FullName, bytes);

  /// <summary>
  /// Writes a sequence of strings to a file, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The sequence of strings to write to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines);
  /// Console.WriteLine("Lines written successfully.");
  /// </code>
  /// This example writes two lines to "lines.txt", overwriting any existing content.
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents) => File.WriteAllLines(@this.FullName, contents);
#else
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents) => File.WriteAllLines(@this.FullName, contents.ToArray());
#endif

  /// <summary>
  /// Writes a sequence of strings to a file using the specified encoding, overwriting the file if it already exists.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The sequence of strings to write to the file.</param>
  /// <param name="encoding">The encoding to apply to the strings.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("lines.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First line", "Second line" };
  /// fileInfo.WriteAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines written successfully with UTF-8 encoding.");
  /// </code>
  /// This example writes two lines to "lines.txt" using UTF-8 encoding, overwriting any existing content.
  /// </example>
#if SUPPORTS_ENUMERATING_IO
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents, encoding);
#else
  public static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(@this.FullName, contents.ToArray(), encoding);
#endif

  #endregion

  #region appending

  /// <summary>
  /// Appends text to the end of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The text to append to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendAllText("Appended text");
  /// Console.WriteLine("Text appended successfully.");
  /// </code>
  /// This example appends "Appended text" to the end of "example.txt".
  /// </example>
  public static void AppendAllText(this FileInfo @this, string contents) => File.AppendAllText(@this.FullName, contents);

  /// <summary>
  /// Appends text to the end of the file represented by the <see cref="FileInfo"/> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The text to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended text.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendAllText("Appended text", Encoding.UTF8);
  /// Console.WriteLine("Text appended successfully with UTF-8 encoding.");
  /// </code>
  /// This example appends "Appended text" to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendAllText(this FileInfo @this, string contents, Encoding encoding) => File.AppendAllText(@this.FullName, contents, encoding);

  /// <summary>
  /// Appends lines to the end of the file represented by the <see cref="FileInfo"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The lines to append to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First appended line", "Second appended line" };
  /// fileInfo.AppendAllLines(lines);
  /// Console.WriteLine("Lines appended successfully.");
  /// </code>
  /// This example appends two lines to the end of "example.txt".
  /// </example>
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents) {
    using var writer=new StreamWriter(@this.FullName, append: true);
    foreach (var line in contents) 
      writer.WriteLine(line);
  }

  /// <summary>
  /// Appends lines to the end of the file represented by the <see cref="FileInfo"/> instance using the specified encoding.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The lines to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended lines.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// IEnumerable&lt;string&gt; lines = new List&lt;string&gt; { "First appended line", "Second appended line" };
  /// fileInfo.AppendAllLines(lines, Encoding.UTF8);
  /// Console.WriteLine("Lines appended successfully with UTF-8 encoding.");
  /// </code>
  /// This example appends two lines to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding) {
    using var writer = new StreamWriter(@this.FullName, true, encoding);
    foreach (var line in contents)
      writer.WriteLine(line);
  }

  /// <summary>
  /// Appends a line to the end of the file represented by the <see cref="FileInfo"/> instance, followed by a line terminator.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The line to append to the file.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendLine("Appended line");
  /// Console.WriteLine("Line appended successfully.");
  /// </code>
  /// This example appends "Appended line" followed by a line terminator to the end of "example.txt".
  /// </example>
  public static void AppendLine(this FileInfo @this, string contents) {
    using var writer = new StreamWriter(@this.FullName, append: true);
    writer.WriteLine(contents);
  }

  /// <summary>
  /// Appends a line to the end of the file represented by the <see cref="FileInfo"/> instance using the specified encoding, followed by a line terminator.
  /// </summary>
  /// <param name="this">The <see cref="FileInfo"/> object representing the file.</param>
  /// <param name="contents">The line to append to the file.</param>
  /// <param name="encoding">The encoding to use for the appended line.</param>
  /// <example>
  /// <code>
  /// FileInfo fileInfo = new FileInfo("example.txt");
  /// fileInfo.AppendLine("Appended line", Encoding.UTF8);
  /// Console.WriteLine("Line appended successfully with UTF-8 encoding.");
  /// </code>
  /// This example appends "Appended line" followed by a line terminator to the end of "example.txt" using UTF-8 encoding.
  /// </example>
  public static void AppendLine(this FileInfo @this, string contents, Encoding encoding) {
    using var writer = new StreamWriter(@this.FullName, true, encoding);
    writer.WriteLine(contents);
  }

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
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(encoding);

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
        using var tmpFileWriter = tmpFile.Open(FileMode.Open, FileAccess.Write, FileShare.None);
        inputReader.CopyTo(tmpFileWriter);
        tmpFileWriter.Flush(true);
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
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(encoding);

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
    Against.ThisIsNull(@this);

    try
    {
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
  /// Changes the last write time of the given file to the current date/time.
  /// </summary>
  /// <param name="this">This FileInfo.</param>
  public static void Touch(this FileInfo @this) => @this.LastWriteTimeUtc = DateTime.UtcNow;
    
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
    if (_ILEGAL_CHARACTERS_REGEX.IsMatch(pattern)) throw new ArgumentException("Patterns contains illegal characters.", nameof(pattern));

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
      var sourceAsync =  sourceStream.ReadBytesAsync(position, sourceBufferA);
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

  /// <summary>
  /// Represents a file being modified or processed, with operations to manage changes before finalizing them.
  /// </summary>
  public interface IFileInProgress:IDisposable {
    
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
    void WriteAllLines(IEnumerable<string> lines,Encoding encoding);
    void AppendLine(string line);
    void AppendLine(string line,Encoding encoding);
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

  /// <summary>
  /// A sealed class implementing <see cref="IFileInProgress"/> for handling file operations that might be rolled back or committed.
  /// </summary>
  /// <remarks>
  /// This class provides a robust mechanism for modifying files by working with a temporary copy until changes are either finalized
  /// by replacing the original file or discarded. It ensures that operations do not directly affect the original file, minimizing
  /// the risk of data loss or corruption during processing. The class implements <see cref="IDisposable"/> to clean up resources,
  /// particularly the temporary file, ensuring no leftover files consume disk space unintentionally.
  /// </remarks>
  private sealed class FileInProgress : IFileInProgress {
    private readonly PathExtensions.ITemporaryFileToken _token;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInProgress"/> class for managing modifications to the specified source file.
    /// </summary>
    /// <param name="sourceFile">The source <see cref="FileInfo"/> object representing the file to be modified.</param>
    /// <remarks>
    /// This constructor creates a temporary file in the same directory as the source file. The temporary file is used to accumulate changes.
    /// The temporary file's name is derived from the source file's name with an added ".$$$" extension to denote its temporary status.
    /// </remarks>
    /// <example>
    /// <code>
    /// FileInfo sourceFile = new FileInfo(@"C:\path\to\your\file.txt");
    /// var fileInProgress = new FileInProgress(sourceFile);
    /// // Perform operations with fileInProgress
    /// </code>
    /// This example demonstrates creating a <see cref="FileInProgress"/> instance to manage changes to a file without directly affecting the original file.
    /// </example>
    public FileInProgress(FileInfo sourceFile) {
      this.OriginalFile = sourceFile;
      this._token = PathExtensions.GetTempFileToken(sourceFile.Name + ".$$$", sourceFile.DirectoryName);
    }

    ~FileInProgress() => this.Dispose();

    #region Implementation of IDisposable

    /// <inheritdoc/>
    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      GC.SuppressFinalize(this);

      if (!this.CancelChanges)
        this.OriginalFile.ReplaceWith(this._TemporaryFile);

      this._token.Dispose();
    }

    #endregion

    /// <summary>
    /// Gets the temporary file associated with the current work-in-progress operation.
    /// </summary>
    /// <value>
    /// A <see cref="FileInfo"/> representing the temporary file used for modifications until the work is finalized or discarded.
    /// </value>
    /// <remarks>
    /// This property provides access to the temporary file created to hold changes during the modification process. The temporary file
    /// is used to safely apply changes without directly affecting the original file until the operation is complete and changes are
    /// either committed or rolled back.
    /// </remarks>
    private FileInfo _TemporaryFile => this._token.File;

    #region Implementation of IFileInProgress

    /// <inheritdoc/>
    public FileInfo OriginalFile { get; }

    /// <inheritdoc/>
    public bool CancelChanges { get; set; }

    /// <inheritdoc/>
    public void CopyFrom(FileInfo source) => source.CopyTo(this._TemporaryFile, true);

    /// <inheritdoc/>
    public Encoding GetEncoding() => this._TemporaryFile.GetEncoding();

    /// <inheritdoc/>
    public string ReadAllText() => this._TemporaryFile.ReadAllText();

    /// <inheritdoc/>
    public string ReadAllText(Encoding encoding) => this._TemporaryFile.ReadAllText(encoding);

    /// <inheritdoc/>
    public IEnumerable<string> ReadLines() => this._TemporaryFile.ReadLines();

    /// <inheritdoc/>
    public IEnumerable<string> ReadLines(Encoding encoding) => this._TemporaryFile.ReadLines(encoding);

    /// <inheritdoc/>
    public void WriteAllText(string text) => this._TemporaryFile.WriteAllText(text);

    /// <inheritdoc/>
    public void WriteAllText(string text, Encoding encoding) => this._TemporaryFile.WriteAllText(text, encoding);

    /// <inheritdoc/>
    public void WriteAllLines(IEnumerable<string> lines) => this._TemporaryFile.WriteAllLines(lines);

    /// <inheritdoc/>
    public void WriteAllLines(IEnumerable<string> lines, Encoding encoding) => this._TemporaryFile.WriteAllLines(lines, encoding);

    /// <inheritdoc/>
    public void AppendLine(string line) => this._TemporaryFile.AppendLine(line);

    /// <inheritdoc/>
    public void AppendLine(string line, Encoding encoding) => this._TemporaryFile.AppendLine(line, encoding);

    /// <inheritdoc/>
    public void AppendAllLines(IEnumerable<string> lines) => this._TemporaryFile.AppendAllLines(lines);

    /// <inheritdoc/>
    public void AppendAllLines(IEnumerable<string> lines, Encoding encoding) => this._TemporaryFile.AppendAllLines(lines, encoding);

    /// <inheritdoc/>
    public void AppendAllText(string text) => this._TemporaryFile.AppendAllText(text);

    /// <inheritdoc/>
    public void AppendAllText(string text, Encoding encoding) => this._TemporaryFile.AppendAllText(text, encoding);

    /// <inheritdoc/>
    public FileStream Open(FileAccess access) => this._TemporaryFile.Open(FileMode.OpenOrCreate, access, FileShare.None);

    /// <inheritdoc/>
    public byte[] ReadAllBytes() => this._TemporaryFile.ReadAllBytes();

    /// <inheritdoc/>
    public void WriteAllBytes(byte[] data) => this._TemporaryFile.WriteAllBytes(data);

    /// <inheritdoc/>
    public IEnumerable<byte> ReadBytes() => this._TemporaryFile.ReadBytes();

    /// <inheritdoc/>
    public void KeepFirstLines(int count) => this._TemporaryFile.KeepFirstLines(count);

    /// <inheritdoc/>
    public void KeepFirstLines(int count, Encoding encoding) => this._TemporaryFile.KeepFirstLines(count, encoding);

    /// <inheritdoc/>
    public void KeepLastLines(int count) => this._TemporaryFile.KeepLastLines(count);

    /// <inheritdoc/>
    public void KeepLastLines(int count, Encoding encoding) => this._TemporaryFile.KeepLastLines(count, encoding);

    /// <inheritdoc/>
    public void RemoveFirstLines(int count, Encoding encoding) => this._TemporaryFile.RemoveFirstLines(count, encoding);

    /// <inheritdoc/>
    public void RemoveFirstLines(int count) => this._TemporaryFile.RemoveFirstLines(count);

    #endregion
  }

  /// <summary>
  /// Initiates a work-in-progress operation on a file, optionally copying its contents to a temporary working file.
  /// </summary>
  /// <param name="this">The source file to start the operation on.</param>
  /// <param name="copyContents">Specifies whether the contents of the source file should be copied to the temporary file.</param>
  /// <returns>An <see cref="IFileInProgress"/> instance for managing the work-in-progress file.</returns>
  /// <example>
  /// <code>
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
  /// This example demonstrates starting a work-in-progress operation on a file, 
  /// modifying its content, and optionally canceling the changes.
  /// </example>
  public static IFileInProgress StartWorkInProgress(this FileInfo @this, bool copyContents = false) {
    Against.ThisIsNull(@this);

    var result = new FileInProgress(@this);
    if (copyContents)
      result.CopyFrom(@this);

    return result;
  }

}