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

using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class DriveInfoExtensions {
  /// <summary>
  ///   Tests whether the specified drive exists.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo" />.</param>
  /// <returns><see langword="true" />  when the drive exists; otherwise, <see langword="false" />.</returns>
  public static bool Exists(this DriveInfo @this) {
    Against.ThisIsNull(@this);

    try {
      return @this.IsReady || @this.DriveType != DriveType.NoRootDirectory;
    } catch {
      return false;
    }
  }

  /// <summary>
  ///   Gets the percentage of free space on the drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo" />.</param>
  /// <returns>A value between 0 and 100 representing the percentage of free space.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double PercentFree(this DriveInfo @this) => @this.AvailableFreeSpace * 100.0 / @this.TotalSize;

  /// <summary>
  ///   Gets the percentage of used space on the drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo" />.</param>
  /// <returns>A value between 0 and 100 representing the percentage of used space.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double PercentUsed(this DriveInfo @this) => 100 - @this.PercentFree();

  /// <summary>
  ///   Gets the size of a single cluster in bytes.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo" />.</param>
  /// <returns>The size of a single cluster in bytes.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
  ///   Gets the size of a single sector in bytes.
  /// </summary>
  /// <param name="this">This <see cref="DriveInfo" />.</param>
  /// <returns>The size of a single sector in bytes.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
