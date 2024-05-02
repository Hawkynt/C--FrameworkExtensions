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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Guard;

namespace System.IO;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global
public static partial class LinkExtensions {
  
  #region Nested types
  
  /// <summary>
  /// The native methods.
  /// </summary>
  private static class NativeMethods {
    
    #region consts
    public const uint symLinkTag = 0xA000000C;
    public const uint junctionTag = 0xA0000003;
    public const int maxUnicodePathLength = 0x3FF0;
    public const string UnparsedPrefix = @"\??\";

    public static readonly IntPtr InvalidHandle = new(-1);
    public const int ERROR_MORE_DATA = 0xea;
    public const uint ERROR_HANDLE_EOF = 0x26;

    public enum SymbolLinkFlag : uint {
      TargetIsFile = 0,
      TargetIsDirectory = 1,
    }

    public enum SymbolLinkTargetFlag : uint {
      Absolute = 0,
      Relative = 1,
    }

    [Flags]
    public enum GenericAccessRights : uint {
      All = 0x10000000,
      Read = 0x80000000,
      Write = 0x40000000,
      Execute = 0x20000000,
    }

    [Flags]
    public enum ShareMode : uint {
      Delete = 0x04,
      Read = 0x01,
      Write = 0x02,
      All = Read | Write | Delete,
    }

    public enum CreationDisposition : uint {
      CreateAlways = 0x02,
      CreateNew = 0x01,
      OpenAlways = 0x04,
      OpenExisting = 0x03,
      TruncateExisting = 0x05,
    }

    [Flags]
    public enum _FileAttributes : uint {
      ReadOnly = 1,
      Hidden = 2,
      System = 4,
      Directory = 16,
      Archive = 32,
      Device = 64,
      Normal = 128,
      Temporary = 0x100,
      SparseFile = 0x200,
      ReparsePoint = 0x400,
      Compressed = 2048,
      Offline = 0x1000,
      NotContentIndexed = 0x2000,
      Encrypted = 16384,
      Virtual = 0x10000,
      IntegrityStream = 0x8000,
      NoScrubData = 0x20000,
      BackupSemantics = 0x2000000,
      DeleteOnClose = 0x4000000,
      NoBuffering = 0x20000000,
      OpenNoRecall = 0x00100000,
      OpenReparsePoint = 0x00200000,
      OverlappedIo = 0x40000000,
      PosixSemantics = 0x0100000,
      RandomAccess = 0x10000000,
      SessionAware = 0x00800000,
      SequentialScan = 0x08000000,
      WriteThrough = 0x80000000,
    }

    public enum IoControl : int {
      SetCompression = 0x9C040,
      GetReparsePoint = 0x000900A8,
      SetReparsePoint = 0x000900A4,
      DeleteReparsePoint = 0x000900AC,
    }

    [Flags]
    public enum FindFirstFileNameFlags : uint {
      None = 0,
    }
    
    #endregion
    
    #region imports
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, [MarshalAs(UnmanagedType.U4)] SymbolLinkFlag dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeFileHandle CreateFile(string lpFileName, GenericAccessRights dwDesiredAccess, ShareMode dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, _FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool DeviceIoControl(
      IntPtr hDevice,
      [MarshalAs(UnmanagedType.I4)]IoControl dwIoControlCode,
      IntPtr lpInBuffer,
      int nInBufferSize,
      IntPtr lpOutBuffer,
      int nOutBufferSize,
      out int lpBytesReturned,
      IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", EntryPoint = "FindFirstFileNameW", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr FindFirstFileName(string lpFileName, FindFirstFileNameFlags dwFlags, ref uint size, StringBuilder linkName);

    [DllImport("kernel32.dll", EntryPoint = "FindNextFileName", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool FindNextFileName(IntPtr hFindStream, ref uint size, StringBuilder linkName);

    [DllImport("kernel32.dll", EntryPoint = "FindClose", SetLastError = true)]
    public static extern bool FindClose(IntPtr hFindStream);
    #endregion
    
    #region nested types
    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct SymbolicLinkReparseBuffer {
      public uint ReparseTag;
      public ushort ReparseDataLength;
      public ushort Reserved;
      public ushort SubstituteNameOffset;
      public ushort SubstituteNameLength;
      public ushort PrintNameOffset;
      public ushort PrintNameLength;
      public SymbolLinkTargetFlag Flags;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
      public byte[] PathBuffer;
    }
      
    [StructLayout(LayoutKind.Sequential)]
    public struct MountPointReparseBuffer {
      public uint ReparseTag;
      public ushort ReparseDataLength;
      public ushort Reserved;
      public ushort SubstituteNameOffset;
      public ushort SubstituteNameLength;
      public ushort PrintNameOffset;
      public ushort PrintNameLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
      public byte[] PathBuffer;
    }
      
      
    [StructLayout(LayoutKind.Sequential)]
    public struct BY_HANDLE_FILE_INFORMATION {
      public _FileAttributes FileAttributes;
      public Runtime.InteropServices.ComTypes.FILETIME CreationTime;
      public Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
      public Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
      public uint VolumeSerialNumber;
      public uint FileSizeHigh;
      public uint FileSizeLow;
      public uint NumberOfLinks;
      public uint FileIndexHigh;
      public uint FileIndexLow;

      public ulong FileSize => (ulong)this.FileSizeHigh << 32 | this.FileSizeLow;
      public ulong FileIndex => (ulong)this.FileIndexHigh << 32 | this.FileIndexLow;
    }

    #endregion
    
  }
  
  #endregion

  #region Internal target getters
  
  private static int _InternalGetSymbolicLinkTargetCount(FileSystemInfo @this) => _InternalGetSymbolicLinkTarget(@this) == null ? 0 : 1;
  private static string _InternalGetSymbolicLinkTarget(FileSystemInfo @this)=> _GetTargetS(@this.FullName);
  private static int _InternalGetJunctionTargetCount(DirectoryInfo @this)=> _InternalGetJunctionTarget(@this) == null ? 0 : 1;
  private static string _InternalGetJunctionTarget(DirectoryInfo @this) => _GetTargetJ(@this.FullName);
  
  private static int _InternalGetHardLinkTargetCount(FileInfo @this) {
    if (@this.IsSymbolicLink())
      return 0;

    using var handle = @this.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    if (!NativeMethods.GetFileInformationByHandle(handle.SafeFileHandle, out var lpFileInformation))
      throw new Win32Exception();

    return (int)lpFileInformation.NumberOfLinks - 1;
  }

  private static IEnumerable<string> _InternalGetHardLinkTargets(FileInfo This) {
    var handle = NativeMethods.InvalidHandle;
    try {
      handle = _FindFirstFileName(This.FullName, out var buffer);
      if (buffer == null)
        yield break;

      var root = Path.GetPathRoot(This.FullName).TrimEnd(Path.DirectorySeparatorChar);
      do {
        buffer.Insert(0, root);
        var result = buffer.ToString(); // HINT: Buffer content always starts with '\'
        yield return result;
      } while (_FindNextFileName(handle, ref buffer));

    } finally {
      if (handle != NativeMethods.InvalidHandle)
        if (!NativeMethods.FindClose(handle))
          throw new Win32Exception();
    }
  }
  #endregion

  #region Internal creators
  private static Exception _InternalCreateSymbolicLinkFrom(FileSystemInfo This,string source) => NativeMethods.CreateSymbolicLink(This.FullName, source, NativeMethods.SymbolLinkFlag.TargetIsFile) ? null : new Win32Exception();

  private static Exception _InternalCreateHardLinkFrom(FileInfo This, string source) {
    if(!File.Exists(source))
      throw new FileNotFoundException("HardLink source missing");

    return NativeMethods.CreateHardLink(This.FullName, source, IntPtr.Zero)?null:new Win32Exception();
  }
    
  private static Exception _InternalCreateJunctionFrom(DirectoryInfo This, string source) {
    This.Refresh();
    if(!This.Exists)
      This.Create();

    var fileHandle = NativeMethods.CreateFile(This.FullName, NativeMethods.GenericAccessRights.Write, NativeMethods.ShareMode.All, IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting, NativeMethods._FileAttributes.BackupSemantics | NativeMethods._FileAttributes.OpenReparsePoint, IntPtr.Zero);
    if (fileHandle.IsInvalid)
      throw new Win32Exception();

    var printName = Encoding.Unicode.GetBytes(source);
    var substTarget = NativeMethods.UnparsedPrefix + new DirectoryInfo(source).FullName + Path.DirectorySeparatorChar;
    var substName = Encoding.Unicode.GetBytes(substTarget);

    var buffer = new NativeMethods.MountPointReparseBuffer {
      ReparseTag = NativeMethods.junctionTag,
      ReparseDataLength = (ushort)(substName.Length + printName.Length + 12),
      SubstituteNameOffset = 0,
      SubstituteNameLength = (ushort)substName.Length,
      PrintNameOffset = (ushort)(substName.Length + 2),
      PrintNameLength = (ushort)printName.Length,
      PathBuffer = new byte[NativeMethods.maxUnicodePathLength],
    };
    Buffer.BlockCopy(substName, 0, buffer.PathBuffer, buffer.SubstituteNameOffset, substName.Length);
    Buffer.BlockCopy(printName, 0, buffer.PathBuffer, buffer.PrintNameOffset, printName.Length);

    var blockSize = Marshal.SizeOf(buffer);
    var unmanagedBlock = IntPtr.Zero;
    try {
      unmanagedBlock = Marshal.AllocHGlobal(blockSize);
      Marshal.StructureToPtr(buffer, unmanagedBlock, false);
      var result = NativeMethods.DeviceIoControl(fileHandle.DangerousGetHandle(), NativeMethods.IoControl.SetReparsePoint, unmanagedBlock, buffer.ReparseDataLength + 8, IntPtr.Zero, 0, out _, IntPtr.Zero);
      return !result ? new Win32Exception() : null;
    } finally {
      if (unmanagedBlock != IntPtr.Zero)
        Marshal.FreeHGlobal(unmanagedBlock);
    }
      
  }
  #endregion

  #region methods
  private static string _InternalGetHardLinkTarget(FileInfo @this) => _InternalGetHardLinkTargets(@this).FirstOrDefault();
  public static IEnumerable<FileInfo> GetHardLinkTargets(this FileInfo @this) {
    Against.ThisIsNull(@this);
    
    return _InternalGetHardLinkTargets(@this).Select(f => new FileInfo(f)).Where(f => f.FullName != @this.FullName);
  }

  /// <summary>
  /// Wraps call to FindFirstFileName.
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="buffer">The buffer to create.</param>
  /// <returns>The handle or throws.</returns>
  private static IntPtr _FindFirstFileName(string fileName, out StringBuilder buffer) {
    var size = 0U;
    buffer = null;
    for (; ;) {
      var handle = NativeMethods.FindFirstFileName(fileName, NativeMethods.FindFirstFileNameFlags.None, ref size, buffer);

      // if we got a handle, we can escape this loop
      if (handle != NativeMethods.InvalidHandle)
        return handle;

      Win32Exception exception = new();

      // if something other than a more data request occured, throw
      if (exception.NativeErrorCode != NativeMethods.ERROR_MORE_DATA)
        throw exception;

      // resize buffer and try again
      buffer = new((int)size);
    }
  }

  /// <summary>
  /// Wraps calls to FindNextFileName.
  /// </summary>
  /// <param name="handle">The handle.</param>
  /// <param name="buffer">The buffer (which could be adjusted in size to accomodate space requirements).</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c> (end of list).</returns>
  private static bool _FindNextFileName(IntPtr handle, ref StringBuilder buffer) {
    var size = (uint)buffer.Length;
    for(;;) {
      if (NativeMethods.FindNextFileName(handle, ref size, buffer))
        return true;

      Win32Exception exception = new();

      // end of list
      if (exception.NativeErrorCode == NativeMethods.ERROR_HANDLE_EOF)
        return false;

      // something strange happened, crash
      if (exception.NativeErrorCode != NativeMethods.ERROR_MORE_DATA)
        throw exception;

      // try again with bigger buffer
      buffer = new((int)size);
    }
  }
        
  /// <summary>
  /// Gets the target of a link.
  /// </summary>
  /// <param name="path">The path.</param>
  /// <returns>The target or <c>null</c>.</returns>
  /// <exception cref="System.ComponentModel.Win32Exception"></exception>
  private static string _GetTargetS(string path) {
    NativeMethods.SymbolicLinkReparseBuffer reparseDataBuffer;

    using (var fileHandle = NativeMethods.CreateFile(path, NativeMethods.GenericAccessRights.Read, NativeMethods.ShareMode.All, IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting, NativeMethods._FileAttributes.BackupSemantics | NativeMethods._FileAttributes.OpenReparsePoint, IntPtr.Zero)) {
      if (fileHandle.IsInvalid)
        throw new Win32Exception();

      var outBufferSize = Marshal.SizeOf(typeof(NativeMethods.SymbolicLinkReparseBuffer));
      var outBuffer = IntPtr.Zero;
      try {
        outBuffer = Marshal.AllocHGlobal(outBufferSize);
        var success = NativeMethods.DeviceIoControl(
          fileHandle.DangerousGetHandle(), NativeMethods.IoControl.GetReparsePoint, IntPtr.Zero, 0,
          outBuffer, outBufferSize, out _, IntPtr.Zero
        );

        fileHandle.Close();
        const uint ERROR_NOT_A_REPARSE_POINT = 0x1126;
        const uint ERROR_INVALID_FUNCTION = 0x1;

        if (!success) {
          Win32Exception exception = new();

          if (exception.NativeErrorCode == ERROR_NOT_A_REPARSE_POINT)
            return null;

          if (exception.NativeErrorCode == ERROR_INVALID_FUNCTION)
            return null;

          throw exception;
        }

        reparseDataBuffer = (NativeMethods.SymbolicLinkReparseBuffer)Marshal.PtrToStructure(outBuffer, typeof(NativeMethods.SymbolicLinkReparseBuffer));
      } finally {
        Marshal.FreeHGlobal(outBuffer);
      }
    }

    if (reparseDataBuffer.ReparseTag != NativeMethods.symLinkTag)
      return null;

    var subst = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);
    var target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

    return target;
  }

  /// <summary>
  /// Gets the target of a junction.
  /// </summary>
  /// <param name="path">The path.</param>
  /// <returns>The target or <c>null</c>.</returns>
  /// <exception cref="System.ComponentModel.Win32Exception"></exception>
  private static string _GetTargetJ(string path) {
    NativeMethods.MountPointReparseBuffer reparseDataBuffer;

    using (var fileHandle = NativeMethods.CreateFile(path, NativeMethods.GenericAccessRights.Read, NativeMethods.ShareMode.All, IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting, NativeMethods._FileAttributes.BackupSemantics | NativeMethods._FileAttributes.OpenReparsePoint, IntPtr.Zero)) {
      if (fileHandle.IsInvalid)
        throw new Win32Exception();

      var outBufferSize = Marshal.SizeOf(typeof(NativeMethods.MountPointReparseBuffer));
      var outBuffer = IntPtr.Zero;
      try {
        outBuffer = Marshal.AllocHGlobal(outBufferSize);
        var success = NativeMethods.DeviceIoControl(
          fileHandle.DangerousGetHandle(), NativeMethods.IoControl.GetReparsePoint, IntPtr.Zero, 0,
          outBuffer, outBufferSize, out _, IntPtr.Zero
        );

        fileHandle.Close();
        const uint ERROR_NOT_A_REPARSE_POINT = 0x1126;
        const uint ERROR_INVALID_FUNCTION = 0x1;

        if (!success) {
          Win32Exception exception = new();

          if (exception.NativeErrorCode == ERROR_NOT_A_REPARSE_POINT)
            return null;

          if (exception.NativeErrorCode == ERROR_INVALID_FUNCTION)
            return null;

          throw exception;
        }

        reparseDataBuffer = (NativeMethods.MountPointReparseBuffer)Marshal.PtrToStructure(outBuffer, typeof(NativeMethods.MountPointReparseBuffer));
      } finally {
        Marshal.FreeHGlobal(outBuffer);
      }
    }

    if (reparseDataBuffer.ReparseTag != NativeMethods.junctionTag)
      return null;

    var subst = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);
    var target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

    var result = subst.StartsWith(NativeMethods.UnparsedPrefix) ? subst[NativeMethods.UnparsedPrefix.Length..] : subst;
    if(result.StartsWith("UNC\\"))
      result= $@"\\{result[4..]}";

    return result;
  }
  #endregion
  
  #region file copy
  /// <summary>
  /// Copies this file to a target location possibly allowing hard-linking.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <param name="targetFileName">Name of the target file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any target file/link.</param>
  /// <param name="allowHardLinking">if set to <c>true</c> allows hard linking.</param>
  /// <exception cref="System.IO.IOException">Target file already exists</exception>
  public static void CopyTo(this FileInfo @this, string targetFileName, bool overwrite = false, bool allowHardLinking = false) {
    Against.ThisIsNull(@this);
    
    @this.CopyTo(new FileInfo(targetFileName), overwrite, allowHardLinking);
  }

  /// <summary>
  /// Gets a temporary filename in the same directory with a similar name.
  /// </summary>
  /// <param name="This">The this.</param>
  /// <param name="allowCreation">if set to <c>true</c> allows file creation.</param>
  /// <returns></returns>
  private static FileInfo _GetSimilarTempFile(this FileInfo This, bool allowCreation = false) {
    var path = This.DirectoryName;
    var name = This.GetFilenameWithoutExtension();
    const string extension = ".$$$";
    var index = 0;
    FileInfo result;
    do {
      result = new(Path.Combine(path, name + "." + ++index + extension));
    } while ((allowCreation && !result.TryCreate()) || (!allowCreation && result.Exists));
    result.Refresh();
    return result;
  }

  /// <summary>
  /// Copies a file from source to target as fast as possible.
  /// </summary>
  /// <param name="sourceFileName">The source file.</param>
  /// <param name="targetFileName">The target file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any existing target file.</param>
  private static void _CopyFromTo(string sourceFileName, string targetFileName, bool overwrite) => File.Copy(sourceFileName, targetFileName, overwrite);
  
  /// <summary>
  /// Copies this file to a target location possibly allowing hard-linking.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <param name="targetFile">The target file.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites any target file/link.</param>
  /// <param name="allowHardLinking">if set to <c>true</c> allows hard linking.</param>
  /// <exception cref="System.IO.IOException">Target file already exists</exception>
  public static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite = false, bool allowHardLinking = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(targetFile);

    if (!allowHardLinking) {
      _CopyFromTo(@this.FullName, targetFile.FullName, overwrite);
      return;
    }

    // simply try to create a hard link first
    if (@this.TryCreateHardLinkAt(targetFile))
      return;

    if (File.Exists(targetFile.FullName)) {

      // could not create hard link because file exists already
      FileInfo tempFile = null;
      var restoreNeeded = false;
      try {

        // backup old file
        tempFile = targetFile._GetSimilarTempFile(true);
        File.Replace(targetFile.FullName, tempFile.FullName, null, true);
        restoreNeeded = true;

        // old file was moved now to temp, retry create or copy
        if (!@this.TryCreateHardLinkAt(targetFile))
          _CopyFromTo(@this.FullName, targetFile.FullName, overwrite);

        restoreNeeded = false;
      } finally {
        if (tempFile != null && File.Exists(tempFile.FullName))
          if (restoreNeeded)
            File.Replace(tempFile.FullName, targetFile.FullName, null, true);
          else
            File.Delete(tempFile.FullName);
      }

    } else {

      // could not create hard link because file is on another patition/drive
      _CopyFromTo(@this.FullName, targetFile.FullName, overwrite);
    }
  }
  #endregion

}
