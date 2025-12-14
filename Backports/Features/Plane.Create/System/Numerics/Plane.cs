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
//

// Requires System.Numerics.Vectors (SUPPORTS_VECTOR or OFFICIAL_VECTOR) for base types
#if (SUPPORTS_VECTOR || OFFICIAL_VECTOR) && !SUPPORTS_PLANE_CREATE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Plane factory methods added in .NET 10.
/// </summary>
public static partial class PlanePolyfills {

  /// <summary>
  /// Creates a Plane from a Vector4.
  /// </summary>
  /// <param name="value">The vector containing the plane equation coefficients (Normal.X, Normal.Y, Normal.Z, D).</param>
  /// <returns>A new Plane.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane Create(Vector4 value) => new(value);

  /// <summary>
  /// Creates a Plane from a normal vector and distance.
  /// </summary>
  /// <param name="normal">The normal vector of the plane.</param>
  /// <param name="d">The distance from the origin to the plane along the normal.</param>
  /// <returns>A new Plane.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane Create(Vector3 normal, float d) => new(normal, d);

  /// <summary>
  /// Creates a Plane from the specified components.
  /// </summary>
  /// <param name="x">The X component of the normal vector.</param>
  /// <param name="y">The Y component of the normal vector.</param>
  /// <param name="z">The Z component of the normal vector.</param>
  /// <param name="d">The distance from the origin to the plane along the normal.</param>
  /// <returns>A new Plane.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane Create(float x, float y, float z, float d) => new(x, y, z, d);

}

#endif
