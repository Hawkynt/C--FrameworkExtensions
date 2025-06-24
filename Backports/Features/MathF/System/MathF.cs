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

#if !SUPPORTS_MATHF

// ===================================================================================================
// Portions of the code implemented below are based on the 'Berkeley SoftFloat Release 3e' algorithms.
// ===================================================================================================

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static class MathF {

  public const float E = 2.71828183f;
  public const float PI = 3.14159265f;
  public const float Tau = 6.283185307f;
  private const int SIGN_MASK = unchecked((int)0x80000000);
  private const int MAGNITUDE_MASK = 0x7fffffff;
  private static readonly float NegativeZero = _Int32BitsToSingle(SIGN_MASK | 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _IsNegative(float x) => (*(uint*)&x & SIGN_MASK) == SIGN_MASK;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe int _SingleToInt32Bits(float x) => *(int*)&x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _Int32BitsToSingle(int x) => *(float*)&x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Abs(float x) => _Int32BitsToSingle(_SingleToInt32Bits(x) & MAGNITUDE_MASK);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Sign(float x) {
    var bits = _SingleToInt32Bits(x);
    var magnitude = bits & MAGNITUDE_MASK;
    var signBit = bits >> 31;
    var isZero = magnitude == 0 ? 1 : 0;
    return (1 | -signBit) & ~isZero;
  }

  public static float BitDecrement(float x) {
    var bits = _SingleToInt32Bits(x);

    if ((bits & 0x7F800000) >= 0x7F800000)
      // NaN returns NaN
      // -Infinity returns -Infinity
      // +Infinity returns float.MaxValue
      return bits == 0x7F800000 ? float.MaxValue : x;

    if (bits == 0x00000000)
      // +0.0 returns -float.Epsilon
      return -float.Epsilon;

    // Negative values need to be incremented
    // Positive values need to be decremented
    bits += bits < 0 ? +1 : -1;
    return _Int32BitsToSingle(bits);
  }

  public static float BitIncrement(float x) {
    var bits = _SingleToInt32Bits(x);

    if ((bits & 0x7F800000) >= 0x7F800000)
      // NaN returns NaN
      // -Infinity returns float.MinValue
      // +Infinity returns +Infinity
      return (bits == unchecked((int)(0xFF800000))) ? float.MinValue : x;

    if (bits == unchecked((int)(0x80000000)))
      // -0.0 returns float.Epsilon
      return float.Epsilon;

    // Negative values need to be decremented
    // Positive values need to be incremented
    bits += bits < 0 ? -1 : +1;
    return _Int32BitsToSingle(bits);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CopySign(float x, float y) => _Int32BitsToSingle((_SingleToInt32Bits(x) & ~SIGN_MASK) | (_SingleToInt32Bits(y) & SIGN_MASK));

  public static float IEEERemainder(float x, float y) {
    
    // IEEE 754-2008: NaN payload must be preserved
    if (float.IsNaN(x))
      return x; 

    if (float.IsNaN(y))
      return y;

    var regularMod = x % y;

    if (float.IsNaN(regularMod))
      return float.NaN;

    if ((regularMod == 0) && _IsNegative(x))
      return NegativeZero;

    var alternativeResult = regularMod - Abs(y) * Sign(x);
    if (Abs(alternativeResult) != Abs(regularMod))
      return Abs(alternativeResult) < Abs(regularMod) ? alternativeResult : regularMod;

    var divisionResult = x / y;
    var roundedResult = Round(divisionResult);
    return Abs(roundedResult) > Abs(divisionResult) ? alternativeResult : regularMod;

  }

  public static int ILogB(float x) {
    // Implementation based on https://git.musl-libc.org/cgit/musl/tree/src/math/ilogbf.c
    const int ILogB_NaN = 0x7fffffff;
    const int ILogB_Zero = (-1 - 0x7fffffff);
    
    if (float.IsNaN(x))
      return ILogB_NaN;

    var i = (uint)_SingleToInt32Bits(x);
    var e = (int)((i >> 23) & 0xFF);

    switch (e) {
      case 0: {
        i <<= 9;
        if (i == 0)
          return ILogB_Zero;

        for (e = -0x7F; (i >> 31) == 0; --e, i <<= 1)
          ;
        return e;
      }
      case 0xFF:
        return i << 9 != 0 ? ILogB_Zero : int.MaxValue;
      default:
        return e - 0x7F;
    }
  }

  public static float Log(float x, float y) => (x, y) switch {

    // IEEE 754-2008: NaN payload must be preserved
    // ReSharper disable once MergeIntoPattern
    var (f, _) when float.IsNaN(f) => x,
    // ReSharper disable once MergeIntoPattern
    var (_, f) when float.IsNaN(f) => y,

    (_, 1) => float.NaN,
    (_, _) when ((x != 1) && (y == 0 || float.IsPositiveInfinity(y))) => float.NaN,

    _ => Log(x) / Log(y)
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Max(float x, float y) => x >= y ? x : y;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Min(float x, float y) => x <= y ? x : y;

  public static float MaxMagnitude(float x, float y) {
    // This matches the IEEE 754:2019 `maximumMagnitude` function
    //
    // It propagates NaN inputs back to the caller and
    // otherwise returns the input with a greater magnitude.
    // It treats +0 as greater than -0 as per the specification.

    var ax = Abs(x);
    var ay = Abs(y);

    if ((ax > ay) || float.IsNaN(ax))
      return x;

    if (ax == ay)
      return _IsNegative(x) ? y : x;

    return y;
  }


  public static float MinMagnitude(float x, float y) {
    // This matches the IEEE 754:2019 `minimumMagnitude` function
    //
    // It propagates NaN inputs back to the caller and
    // otherwise returns the input with a lesser magnitude.
    // It treats +0 as greater than -0 as per the specification.

    var ax = Abs(x);
    var ay = Abs(y);

    if ((ax < ay) || float.IsNaN(ax))
      return x;

    if (ax == ay)
      return _IsNegative(x) ? x : y;

    return y;
  }

  /// <summary>Returns an estimate of the reciprocal of a specified number.</summary>
  /// <param name="x">The number whose reciprocal is to be estimated.</param>
  /// <returns>An estimate of the reciprocal of <paramref name="x" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ReciprocalEstimate(float x) => 1.0f / x;

  /// <summary>Returns an estimate of the reciprocal square root of a specified number.</summary>
  /// <param name="x">The number whose reciprocal square root is to be estimated.</param>
  /// <returns>An estimate of the reciprocal square root <paramref name="x" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float ReciprocalSqrtEstimate(float x) => 1.0f / Sqrt(x);

  public static float ScaleB(float x, int n) {
    // Implementation based on https://git.musl-libc.org/cgit/musl/tree/src/math/scalblnf.c
    //
    // Performs the calculation x * 2^n efficiently. It constructs a float from 2^n by building
    // the correct biased exponent. If n is greater than the maximum exponent (127) or less than
    // the minimum exponent (-126), adjust x and n to compute correct result.
    const float SCALEB_C1 = 1.7014118E+38f; // 0x1p127f
    const float SCALEB_C2 = 1.1754944E-38f; // 0x1p-126f
    const float SCALEB_C3 = 16777216f; // 0x1p24f

    var y = x;
    switch (n) {
      case > 127: {
        y *= SCALEB_C1;
        n -= 127;
        if (n > 127) {
          y *= SCALEB_C1;
          n -= 127;
          if (n > 127)
            n = 127;
        }

        break;
      }
      case < -126: {
        y *= SCALEB_C2 * SCALEB_C3;
        n += 126 - 24;
        if (n < -126) {
          y *= SCALEB_C2 * SCALEB_C3;
          n += 126 - 24;
          if (n < -126)
            n = -126;
        }

        break;
      }
    }

    var u = _Int32BitsToSingle(0x7f + n << 23);
    return y * u;
  }

  #region regular mappings
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Ceiling(float x) => (float)Math.Ceiling(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Floor(float x) => (float)Math.Floor(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(float x) => (float)Math.Round(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(float x, int digits) => (float)Math.Round(x, digits);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(float x, int digits, MidpointRounding mode) => (float)Math.Round(x, digits, mode);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(float x, MidpointRounding mode) => (float)Math.Round(x, mode);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Truncate(float x) => (float)Math.Truncate(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Acos(float x) => (float)Math.Acos(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Asin(float x) => (float)Math.Asin(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atan(float x) => (float)Math.Atan(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cos(float x) => (float)Math.Cos(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cosh(float x) => (float)Math.Cosh(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Exp(float x) => (float)Math.Exp(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Log(float x) => (float)Math.Log(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Log10(float x) => (float)Math.Log10(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Pow(float x, float y) => (float)Math.Pow(x, y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sin(float x) => (float)Math.Sin(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sinh(float x) => (float)Math.Sinh(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sqrt(float x) => (float)Math.Sqrt(x);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Tan(float x) => (float)Math.Tan(x);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Tanh(float x) => (float)Math.Tanh(x);

  #endregion

}

#endif