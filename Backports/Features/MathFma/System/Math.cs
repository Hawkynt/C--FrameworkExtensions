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

#if !SUPPORTS_FMADD

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {
  extension(Math) {
    /// <summary>
    /// Returns (x * y) + z, rounded as one ternary operation.
    /// </summary>
    /// <param name="x">The first value to multiply.</param>
    /// <param name="y">The second value to multiply.</param>
    /// <param name="z">The value to add to the product.</param>
    /// <returns>The result of (x * y) + z.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FusedMultiplyAdd(double x, double y, double z)
      => x * y + z;
  }

  extension(MathF) {
    /// <summary>
    /// Returns (x * y) + z, rounded as one ternary operation.
    /// </summary>
    /// <param name="x">The first value to multiply.</param>
    /// <param name="y">The second value to multiply.</param>
    /// <param name="z">The value to add to the product.</param>
    /// <returns>The result of (x * y) + z.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FusedMultiplyAdd(float x, float y, float z)
      => (float)((double)x * y + z);
  }
}

#endif
