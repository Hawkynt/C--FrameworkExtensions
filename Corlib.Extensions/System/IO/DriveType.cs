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

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.IO;

public static partial class DriveTypeExtensions {
  /// <summary>
  ///   Tests whether the specified drive is removable.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is removable; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsRemovable(this DriveType @this) => @this == DriveType.Removable;

  /// <summary>
  ///   Tests whether the specified drive is not removable.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is not removable; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotRemovable(this DriveType @this) => @this != DriveType.Removable;

  /// <summary>
  ///   Tests whether the specified drive is fixed.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is fixed; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsFixed(this DriveType @this) => @this == DriveType.Fixed;

  /// <summary>
  ///   Tests whether the specified drive is not fixed.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is not fixed; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotFixed(this DriveType @this) => @this != DriveType.Fixed;

  /// <summary>
  ///   Tests whether the specified drive is a network drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is a network drive; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNetwork(this DriveType @this) => @this == DriveType.Network;

  /// <summary>
  ///   Tests whether the specified drive is not a network drive.
  /// </summary>
  /// <param name="this">This <see cref="DriveType" />.</param>
  /// <returns><see langword="true" /> if the drive is not a network drive; otherwise, <see langword="false" />.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotNetwork(this DriveType @this) => @this != DriveType.Network;
}
