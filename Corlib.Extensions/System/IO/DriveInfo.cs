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

using Guard;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.IO;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable OutParameterValueIsAlwaysDiscarded.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
public static partial class DriveInfoExtensions {

  private static class NativeMethods {
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetDiskFreeSpace")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _GetDiskFreeSpace(
      string lpRootPathName,
      out uint lpSectorsPerCluster,
      out uint lpBytesPerSector,
      out uint lpNumberOfFreeClusters,
      out uint lpTotalNumberOfClusters
    );

    public static void GetDiskFreeSpace(string rootPath, out uint sectorsPerCluster, out uint bytesPerSector, out uint numberOfFreeClusters, out uint totalNumberOfClusters) {
      if (!_GetDiskFreeSpace(rootPath, out sectorsPerCluster, out bytesPerSector, out numberOfFreeClusters, out totalNumberOfClusters))
        throw new Win32Exception();
    }

  }

  /// <summary>
  /// Tests whether the specified drive exists.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo"/>.</param>
  /// <returns><see langword="true"/>  when the drive exists; otherwise, <see langword="false"/>.</returns>
  public static bool Exists(this DriveInfo @this) {
    Against.ThisIsNull(@this);

    try {
      return @this.IsReady || @this.DriveType != DriveType.NoRootDirectory;
    } catch {
      return false;
    }
  }

  /// <summary>
  /// Gets the percentage of free space on the drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo"/>.</param>
  /// <returns>A value between 0 and 100 representing the percentage of free space.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double PercentFree(this DriveInfo @this) => @this.AvailableFreeSpace * 100.0 / @this.TotalSize;

  /// <summary>
  /// Gets the percentage of used space on the drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo"/>.</param>
  /// <returns>A value between 0 and 100 representing the percentage of used space.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double PercentUsed(this DriveInfo @this) => 100 - @this.PercentFree();

  /// <summary>
  /// Gets the size of a single cluster in bytes.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo"/>.</param>
  /// <returns>The size of a single cluster in bytes.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long ClusterSize(this DriveInfo @this) {
    NativeMethods.GetDiskFreeSpace(
      @this.Name,
      out var sectorsPerCluster,
      out var bytesPerSector,
      out _,
      out _
    );

    return sectorsPerCluster * bytesPerSector;
  }

  /// <summary>
  /// Gets the size of a single sector in bytes.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo"/>.</param>
  /// <returns>The size of a single sector in bytes.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long SectorSize(this DriveInfo @this) {
    NativeMethods.GetDiskFreeSpace(
      @this.Name,
      out _,
      out var result,
      out _,
      out _
    );

    return result;
  }
  
}

