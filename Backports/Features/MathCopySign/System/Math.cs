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

#if !SUPPORTS_MATH_COPYSIGN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {

  extension(Math) {

    /// <summary>
    /// Returns a value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.
    /// </summary>
    /// <param name="x">A number whose magnitude is used in the result.</param>
    /// <param name="y">A number whose sign is used in the result.</param>
    /// <returns>A value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CopySign(double x, double y) {
      var xBits = BitConverter.DoubleToInt64Bits(x);
      var yBits = BitConverter.DoubleToInt64Bits(y);

      // Clear sign bit from x and get sign bit from y
      const long signMask = unchecked((long)0x8000000000000000);
      var result = (xBits & ~signMask) | (yBits & signMask);

      return BitConverter.Int64BitsToDouble(result);
    }

    /// <summary>
    /// Returns a value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.
    /// </summary>
    /// <param name="x">A number whose magnitude is used in the result.</param>
    /// <param name="y">A number whose sign is used in the result.</param>
    /// <returns>A value with the magnitude of <paramref name="x"/> and the sign of <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CopySign(float x, float y) {
      var xBits = _SingleToInt32Bits(x);
      var yBits = _SingleToInt32Bits(y);

      // Clear sign bit from x and get sign bit from y
      const int signMask = unchecked((int)0x80000000);
      var result = (xBits & ~signMask) | (yBits & signMask);

      return _Int32BitsToSingle(result);
    }

    private static unsafe int _SingleToInt32Bits(float value) => *(int*)&value;

    private static unsafe float _Int32BitsToSingle(int value) => *(float*)&value;

  }

}

#endif
