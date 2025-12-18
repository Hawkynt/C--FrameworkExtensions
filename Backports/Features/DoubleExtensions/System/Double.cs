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
//   SUPPORTS_DOUBLE_ISFINITE: Std 2.1, Core 5.0+ - IsFinite, IsNegative, IsNormal, IsSubnormal

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class DoublePolyfills {

#if !SUPPORTS_DOUBLE_ISFINITE

  extension(double) {

    /// <summary>
    /// Determines whether the specified value is finite (not NaN or infinity).
    /// </summary>
    /// <param name="d">The double-precision floating-point value to test.</param>
    /// <returns>true if the value is finite; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(double d) => !double.IsNaN(d) && !double.IsInfinity(d);

    /// <summary>
    /// Determines whether the specified value is negative.
    /// </summary>
    /// <param name="d">The double-precision floating-point value to test.</param>
    /// <returns>true if the value is negative; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(double d) => BitConverter.DoubleToInt64Bits(d) < 0;

    /// <summary>
    /// Determines whether the specified value represents a normal number.
    /// </summary>
    /// <param name="d">The double-precision floating-point value to test.</param>
    /// <returns>true if the value is a normal number; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(double d) {
      var bits = BitConverter.DoubleToInt64Bits(d);
      var exp = (int)((bits >> 52) & 0x7FF);
      return exp != 0 && exp != 0x7FF;
    }

    /// <summary>
    /// Determines whether the specified value is subnormal (denormalized).
    /// </summary>
    /// <param name="d">The double-precision floating-point value to test.</param>
    /// <returns>true if the value is subnormal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(double d) {
      var bits = BitConverter.DoubleToInt64Bits(d);
      var exp = (int)((bits >> 52) & 0x7FF);
      var mantissa = bits & 0xFFFFFFFFFFFFF;
      return exp == 0 && mantissa != 0;
    }

  }

#endif

}
