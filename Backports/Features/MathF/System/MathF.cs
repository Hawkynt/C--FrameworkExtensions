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

// ===================================================================================================
// Portions of the code implemented below are based on the 'Berkeley SoftFloat Release 3e' algorithms.
// ===================================================================================================

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

// Wave 1: Base MathF class (.NET Framework, .NET Standard 2.0)
#if !SUPPORTS_MATHF_WAVE1

public static class MathF {

  public const float E = 2.71828183f;
  public const float PI = 3.14159265f;
  public const float Tau = 6.283185307f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Abs(float x) => _Int32BitsToSingle(_SingleToInt32Bits(x) & MAGNITUDE_MASK);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Sign(float x) {
    var bits = _SingleToInt32Bits(x);
    var magnitude = bits & MAGNITUDE_MASK;
    if (magnitude == 0)
      return 0;
    return (bits >> 31) | 1;
  }

  public static float IEEERemainder(float x, float y) {
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

  public static float Log(float x, float y) => (x, y) switch {
    var (f, _) when float.IsNaN(f) => x,
    var (_, f) when float.IsNaN(f) => y,
    (_, 1) => float.NaN,
    (_, _) when ((x != 1) && (y == 0 || float.IsPositiveInfinity(y))) => float.NaN,
    _ => Log(x) / Log(y)
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Max(float x, float y) => x >= y ? x : y;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Min(float x, float y) => x <= y ? x : y;

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

  private const int SIGN_MASK = unchecked((int)0x80000000);
  private const int MAGNITUDE_MASK = 0x7fffffff;
  private static readonly float NegativeZero = _Int32BitsToSingle(SIGN_MASK | 0);
  private const uint SIGN_MASK_UINT = unchecked((uint)SIGN_MASK);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe bool _IsNegative(float x) => (*(uint*)&x & SIGN_MASK_UINT) == SIGN_MASK_UINT;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe int _SingleToInt32Bits(float x) => *(int*)&x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe float _Int32BitsToSingle(int x) => *(float*)&x;

}

#endif

// Wave 2: Asinh, Acosh, Atanh (.NET Core 2.1)
#if !SUPPORTS_MATHF_WAVE2

public static partial class MathFPolyfillsWave2 {

  extension(MathF) {

    /// <summary>
    /// Returns the inverse hyperbolic sine of the specified number.
    /// </summary>
    /// <param name="x">The number whose inverse hyperbolic sine is to be found.</param>
    /// <returns>The inverse hyperbolic sine of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Asinh(float x) => MathF.Log(x + MathF.Sqrt(x * x + 1));

    /// <summary>
    /// Returns the inverse hyperbolic cosine of the specified number.
    /// </summary>
    /// <param name="x">The number whose inverse hyperbolic cosine is to be found. Must be >= 1.</param>
    /// <returns>The inverse hyperbolic cosine of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Acosh(float x) => MathF.Log(x + MathF.Sqrt(x * x - 1));

    /// <summary>
    /// Returns the inverse hyperbolic tangent of the specified number.
    /// </summary>
    /// <param name="x">The number whose inverse hyperbolic tangent is to be found. Must be in (-1, 1).</param>
    /// <returns>The inverse hyperbolic tangent of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Atanh(float x) => 0.5f * MathF.Log((1 + x) / (1 - x));

  }

}

#endif

// Wave 3: BitIncrement, BitDecrement, CopySign, ILogB, ScaleB, MaxMagnitude, MinMagnitude (.NET Core 3.0)
#if !SUPPORTS_MATHF_WAVE3

public static partial class MathFPolyfillsWave3 {

  private const int _SIGN_MASK = unchecked((int)0x80000000);
  private const int _MAGNITUDE_MASK = 0x7fffffff;
  private const uint _SIGN_MASK_UINT = unchecked((uint)_SIGN_MASK);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe int _SingleToInt32Bits(float x) => *(int*)&x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float _Int32BitsToSingle(int x) => *(float*)&x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe bool _IsNegative(float x) => (*(uint*)&x & _SIGN_MASK_UINT) == _SIGN_MASK_UINT;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Abs(float x) => _Int32BitsToSingle(_SingleToInt32Bits(x) & _MAGNITUDE_MASK);

  extension(MathF) {

    /// <summary>
    /// Returns the smallest value that compares greater than a specified value.
    /// </summary>
    public static float BitIncrement(float x) {
      var bits = _SingleToInt32Bits(x);

      if ((bits & 0x7F800000) >= 0x7F800000)
        return (bits == unchecked((int)0xFF800000)) ? float.MinValue : x;

      if (bits == unchecked((int)0x80000000))
        return float.Epsilon;

      bits += bits < 0 ? -1 : +1;
      return _Int32BitsToSingle(bits);
    }

    /// <summary>
    /// Returns the largest value that compares less than a specified value.
    /// </summary>
    public static float BitDecrement(float x) {
      var bits = _SingleToInt32Bits(x);

      if ((bits & 0x7F800000) >= 0x7F800000)
        return bits == 0x7F800000 ? float.MaxValue : x;

      if (bits == 0x00000000)
        return -float.Epsilon;

      bits += bits < 0 ? +1 : -1;
      return _Int32BitsToSingle(bits);
    }

    /// <summary>
    /// Returns a value with the magnitude of x and the sign of y.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CopySign(float x, float y)
      => _Int32BitsToSingle((_SingleToInt32Bits(x) & ~_SIGN_MASK) | (_SingleToInt32Bits(y) & _SIGN_MASK));

    /// <summary>
    /// Returns the base 2 integer logarithm of a specified number.
    /// </summary>
    public static int ILogB(float x) {
      const int ILogB_NaN = 0x7fffffff;
      const int ILogB_Zero = -1 - 0x7fffffff;

      if (float.IsNaN(x))
        return ILogB_NaN;

      var i = (uint)_SingleToInt32Bits(x);
      var e = (int)((i >> 23) & 0xFF);

      switch (e) {
        case 0: {
          i <<= 9;
          if (i == 0)
            return ILogB_Zero;
          for (e = -0x7F; (i >> 31) == 0; --e, i <<= 1) ;
          return e;
        }
        case 0xFF:
          return i << 9 != 0 ? ILogB_Zero : int.MaxValue;
        default:
          return e - 0x7F;
      }
    }

    /// <summary>
    /// Returns x * 2^n computed efficiently.
    /// </summary>
    public static float ScaleB(float x, int n) {
      const float C1 = 1.7014118E+38f;
      const float C2 = 1.1754944E-38f;
      const float C3 = 16777216f;

      var y = x;
      switch (n) {
        case > 127: {
          y *= C1;
          n -= 127;
          if (n > 127) {
            y *= C1;
            n -= 127;
            if (n > 127)
              n = 127;
          }
          break;
        }
        case < -126: {
          y *= C2 * C3;
          n += 126 - 24;
          if (n < -126) {
            y *= C2 * C3;
            n += 126 - 24;
            if (n < -126)
              n = -126;
          }
          break;
        }
      }

      return y * _Int32BitsToSingle(0x7f + n << 23);
    }

    /// <summary>
    /// Returns the larger magnitude of two single-precision floating-point numbers.
    /// </summary>
    public static float MaxMagnitude(float x, float y) {
      var ax = _Abs(x);
      var ay = _Abs(y);

      if ((ax > ay) || float.IsNaN(ax))
        return x;
      if (ax == ay)
        return _IsNegative(x) ? y : x;
      return y;
    }

    /// <summary>
    /// Returns the smaller magnitude of two single-precision floating-point numbers.
    /// </summary>
    public static float MinMagnitude(float x, float y) {
      var ax = _Abs(x);
      var ay = _Abs(y);

      if ((ax < ay) || float.IsNaN(ax))
        return x;
      if (ax == ay)
        return _IsNegative(x) ? x : y;
      return y;
    }

  }

}

#endif

// Wave 4: ReciprocalEstimate, ReciprocalSqrtEstimate (.NET 6.0)
#if !SUPPORTS_MATHF_WAVE4

public static partial class MathFPolyfillsWave4 {

  extension(MathF) {

    /// <summary>
    /// Returns an estimate of the reciprocal of a specified number.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReciprocalEstimate(float x) => 1.0f / x;

    /// <summary>
    /// Returns an estimate of the reciprocal square root of a specified number.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReciprocalSqrtEstimate(float x) => 1.0f / MathF.Sqrt(x);

  }

}

#endif
