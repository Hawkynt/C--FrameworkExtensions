﻿#region (c)2010-2042 Hawkynt

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace System.IO;

public static partial class VolumeExtensions {
  #region nested types

  /// <summary>
  ///   A volume in the system.
  /// </summary>
  public class Volume(string name) {

    /// <summary>
    ///   Gets the name.
    /// </summary>
    /// <value>
    ///   The name.
    /// </value>
    public string Name { get; } = name;

    /// <summary>
    ///   Gets the path names.
    /// </summary>
    /// <value>
    ///   The path names.
    /// </value>
    public IEnumerable<string> PathNames => GetVolumePathNames(this.Name);

    /// <summary>
    ///   Gets the mount points.
    /// </summary>
    /// <value>
    ///   The mount points.
    /// </value>
    public IEnumerable<string> MountPoints => GetVolumeMountPoints(this.Name);

    public override string ToString() => this.Name;
  }

  /// <summary>
  ///   P/Invoke stuff
  /// </summary>
  private static class NativeMethods {
    public const int _MAX_PATH = 124;
    public static readonly IntPtr INVALID_PTR = new(-1);
    public const int ERROR_MORE_DATA = 234;

    [DllImport("kernel32.dll", EntryPoint = "FindFirstVolume", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr FindFirstVolume(StringBuilder lpszVolumeName, int cchBufferLength);

    [DllImport("kernel32.dll", EntryPoint = "FindNextVolume", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool FindNextVolume(IntPtr hFindVolume, StringBuilder lpszVolumeName, int cchBufferLength);

    [DllImport("kernel32.dll", EntryPoint = "FindVolumeClose", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool FindVolumeClose(IntPtr hFindVolume);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr FindFirstVolumeMountPoint(string lpszRootPathName, [Out] StringBuilder lpszVolumeMountPoint, uint cchBufferLength);

    [DllImport("kernel32.dll")]
    public static extern bool FindNextVolumeMountPoint(IntPtr hFindVolumeMountPoint, [Out] StringBuilder lpszVolumeMountPoint, uint cchBufferLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FindVolumeMountPointClose(IntPtr hFindVolumeMountPoint);

    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetVolumePathNamesForVolumeNameW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetVolumePathNamesForVolumeNameW(string lpszVolumeName, char[] lpszVolumePathNames, uint cchBuferLength, ref uint lpcchReturnLength);
  }

  #endregion

  /// <summary>
  ///   Converts a mask like abc*def?ghi into a regex, that can be used, to match the mask.
  /// </summary>
  /// <param name="mask">The mask.</param>
  /// <returns>The compiled regex.</returns>
  private static Regex _ConvertMaskToRegex(string mask) {
    mask = mask.Replace("\\", "\\\\");
    mask = mask.Replace("[", "\\[");
    mask = mask.Replace("]", "\\]");
    mask = mask.Replace("(", "\\(");
    mask = mask.Replace(")", "\\)");
    mask = mask.Replace("{", "\\{");
    mask = mask.Replace("}", "\\}");
    mask = mask.Replace(".", "\\.");
    mask = mask.Replace("+", "\\+");
    mask = mask.Replace("?", ".");
    mask = mask.Replace("*", ".*?");
    return new(mask, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
  }

  /// <summary>
  ///   Gets the volumes.
  /// </summary>
  /// <param name="filterMask">The filter mask.</param>
  /// <returns></returns>
  public static IEnumerable<Volume> GetVolumes(string filterMask) => GetVolumes(_ConvertMaskToRegex(filterMask));

  /// <summary>
  ///   Gets the volumes.
  /// </summary>
  /// <param name="regex">The regex.</param>
  /// <returns></returns>
  public static IEnumerable<Volume> GetVolumes(Regex regex) => GetVolumes().Where(v => regex.IsMatch(v.Name));

  /// <summary>
  ///   Gets the volumes.
  /// </summary>
  /// <returns></returns>
  public static IEnumerable<Volume> GetVolumes() {
    StringBuilder cVolumeName = new(NativeMethods._MAX_PATH);
    var volumeHandle = NativeMethods.INVALID_PTR;
    try {
      volumeHandle = NativeMethods.FindFirstVolume(cVolumeName, NativeMethods._MAX_PATH);
      if (volumeHandle == NativeMethods.INVALID_PTR)
        yield break;

      do
        yield return new(cVolumeName.ToString());
      while (NativeMethods.FindNextVolume(volumeHandle, cVolumeName, NativeMethods._MAX_PATH));
    } finally {
      if (volumeHandle != NativeMethods.INVALID_PTR)
        NativeMethods.FindVolumeClose(volumeHandle);
    }
  }

  /// <summary>
  ///   Gets the volume path names.
  /// </summary>
  /// <param name="volumeName">Name of the volume.</param>
  /// <returns></returns>
  /// <exception cref="System.ComponentModel.Win32Exception"></exception>
  public static IEnumerable<string> GetVolumePathNames(string volumeName) {
    uint bufferSize = 0;
    if (NativeMethods.GetVolumePathNamesForVolumeNameW(volumeName, [], 0, ref bufferSize))
      return Utilities.Array.Empty<string>();

    var result = new char[bufferSize];
    if (!NativeMethods.GetVolumePathNamesForVolumeNameW(volumeName, result, bufferSize, ref bufferSize))
      throw new Win32Exception();

    return new string(result).Split('\0').Where(t => t.Length > 0);
  }

  /// <summary>
  ///   Gets the names of the volume mount points by volume.
  /// </summary>
  /// <param name="volumeName">Name of the volume.</param>
  /// <returns></returns>
  public static IEnumerable<string> GetVolumeMountPoints(string volumeName) {
    var mountHandle = NativeMethods.INVALID_PTR;
    try {
      StringBuilder cVolumeName = new(NativeMethods._MAX_PATH);
      mountHandle = NativeMethods.FindFirstVolumeMountPoint(volumeName, cVolumeName, NativeMethods._MAX_PATH);
      if (mountHandle == NativeMethods.INVALID_PTR)
        yield break;

      do
        yield return cVolumeName.ToString();
      while (NativeMethods.FindNextVolumeMountPoint(mountHandle, cVolumeName, NativeMethods._MAX_PATH));
    } finally {
      if (mountHandle != NativeMethods.INVALID_PTR)
        NativeMethods.FindVolumeMountPointClose(mountHandle);
    }
  }
}
