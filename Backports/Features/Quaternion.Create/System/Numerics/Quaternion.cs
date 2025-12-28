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
#if !SUPPORTS_QUATERNION_CREATE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Quaternion factory methods added in .NET 10.
/// </summary>
public static partial class QuaternionPolyfills {

  extension(Quaternion) {

    /// <summary>
    /// Creates a Quaternion from a vector part and scalar part.
    /// </summary>
    /// <param name="vectorPart">The vector part (X, Y, Z).</param>
    /// <param name="scalarPart">The scalar part (W).</param>
    /// <returns>A new Quaternion.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Create(Vector3 vectorPart, float scalarPart) => new(vectorPart, scalarPart);

    /// <summary>
    /// Creates a Quaternion from the specified components.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    /// <param name="w">The W component.</param>
    /// <returns>A new Quaternion.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Create(float x, float y, float z, float w) => new(x, y, z, w);

  }

}

#endif
