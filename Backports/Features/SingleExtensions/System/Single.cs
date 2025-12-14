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

// Feature flags:
//   SUPPORTS_SINGLE_ISFINITE: Std 2.1, Core 5.0+ - IsFinite, IsNegative, IsNormal, IsSubnormal

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class SinglePolyfills {

#if !SUPPORTS_SINGLE_ISFINITE

  extension(float) {

    /// <summary>
    /// Determines whether the specified value is finite (not NaN or infinity).
    /// </summary>
    /// <param name="f">The floating-point value to test.</param>
    /// <returns>true if the value is finite; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);

    /// <summary>
    /// Determines whether the specified value is negative.
    /// </summary>
    /// <param name="f">The floating-point value to test.</param>
    /// <returns>true if the value is negative; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(float f) => _GetBits(f) < 0;

    /// <summary>
    /// Determines whether the specified value represents a normal number.
    /// </summary>
    /// <param name="f">The floating-point value to test.</param>
    /// <returns>true if the value is a normal number; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(float f) {
      var bits = _GetBits(f);
      var exp = (bits >> 23) & 0xFF;
      return exp != 0 && exp != 0xFF;
    }

    /// <summary>
    /// Determines whether the specified value is subnormal (denormalized).
    /// </summary>
    /// <param name="f">The floating-point value to test.</param>
    /// <returns>true if the value is subnormal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(float f) {
      var bits = _GetBits(f);
      var exp = (bits >> 23) & 0xFF;
      var mantissa = bits & 0x7FFFFF;
      return exp == 0 && mantissa != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int _GetBits(float f) => *(int*)&f;

  }

#endif

#if !SUPPORTS_SINGLE_LERP

  extension(float) {

    /// <summary>
    /// Performs a linear interpolation between two values.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="amount">A value between 0 and 1 indicating the weight of value2.</param>
    /// <returns>The interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float value1, float value2, float amount)
      => value1 + (value2 - value1) * amount;

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesToRadians(float degrees)
      => degrees * 0.017453292f; // PI / 180

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The angle in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadiansToDegrees(float radians)
      => radians * 57.29578f; // 180 / PI

  }

#endif

}
