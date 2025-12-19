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

#if !SUPPORTS_INTRINSICS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of AVX (Advanced Vector Extensions) intrinsics.
/// Provides 256-bit vector operations for single and double-precision floating-point.
/// </summary>
public abstract class Avx : Sse42 {

  /// <summary>Gets a value indicating whether AVX instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Arithmetic Operations - Add

  /// <summary>Adds packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Add(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] + right[0], left[1] + right[1], left[2] + right[2], left[3] + right[3],
      left[4] + right[4], left[5] + right[5], left[6] + right[6], left[7] + right[7]
    );

  /// <summary>Adds packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Add(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] + right[0], left[1] + right[1], left[2] + right[2], left[3] + right[3]
    );

  #endregion

  #region Arithmetic Operations - Subtract

  /// <summary>Subtracts packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Subtract(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] - right[0], left[1] - right[1], left[2] - right[2], left[3] - right[3],
      left[4] - right[4], left[5] - right[5], left[6] - right[6], left[7] - right[7]
    );

  /// <summary>Subtracts packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Subtract(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] - right[0], left[1] - right[1], left[2] - right[2], left[3] - right[3]
    );

  #endregion

  #region Arithmetic Operations - Multiply

  /// <summary>Multiplies packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Multiply(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] * right[0], left[1] * right[1], left[2] * right[2], left[3] * right[3],
      left[4] * right[4], left[5] * right[5], left[6] * right[6], left[7] * right[7]
    );

  /// <summary>Multiplies packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Multiply(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] * right[0], left[1] * right[1], left[2] * right[2], left[3] * right[3]
    );

  #endregion

  #region Arithmetic Operations - Divide

  /// <summary>Divides packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Divide(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] / right[0], left[1] / right[1], left[2] / right[2], left[3] / right[3],
      left[4] / right[4], left[5] / right[5], left[6] / right[6], left[7] / right[7]
    );

  /// <summary>Divides packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Divide(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] / right[0], left[1] / right[1], left[2] / right[2], left[3] / right[3]
    );

  #endregion

  #region Arithmetic Operations - Sqrt/Reciprocal

  /// <summary>Computes square root of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Sqrt(Vector256<float> value)
    => Vector256.Create(
      MathF.Sqrt(value[0]), MathF.Sqrt(value[1]), MathF.Sqrt(value[2]), MathF.Sqrt(value[3]),
      MathF.Sqrt(value[4]), MathF.Sqrt(value[5]), MathF.Sqrt(value[6]), MathF.Sqrt(value[7])
    );

  /// <summary>Computes square root of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Sqrt(Vector256<double> value)
    => Vector256.Create(
      Math.Sqrt(value[0]), Math.Sqrt(value[1]), Math.Sqrt(value[2]), Math.Sqrt(value[3])
    );

  /// <summary>Computes reciprocal approximation of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Reciprocal(Vector256<float> value)
    => Vector256.Create(
      1f / value[0], 1f / value[1], 1f / value[2], 1f / value[3],
      1f / value[4], 1f / value[5], 1f / value[6], 1f / value[7]
    );

  /// <summary>Computes reciprocal square root approximation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> ReciprocalSqrt(Vector256<float> value)
    => Vector256.Create(
      1f / MathF.Sqrt(value[0]), 1f / MathF.Sqrt(value[1]), 1f / MathF.Sqrt(value[2]), 1f / MathF.Sqrt(value[3]),
      1f / MathF.Sqrt(value[4]), 1f / MathF.Sqrt(value[5]), 1f / MathF.Sqrt(value[6]), 1f / MathF.Sqrt(value[7])
    );

  #endregion

  #region Min/Max Operations

  /// <summary>Computes the minimum of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Min(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      Math.Min(left[0], right[0]), Math.Min(left[1], right[1]), Math.Min(left[2], right[2]), Math.Min(left[3], right[3]),
      Math.Min(left[4], right[4]), Math.Min(left[5], right[5]), Math.Min(left[6], right[6]), Math.Min(left[7], right[7])
    );

  /// <summary>Computes the minimum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Min(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      Math.Min(left[0], right[0]), Math.Min(left[1], right[1]), Math.Min(left[2], right[2]), Math.Min(left[3], right[3])
    );

  /// <summary>Computes the maximum of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Max(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      Math.Max(left[0], right[0]), Math.Max(left[1], right[1]), Math.Max(left[2], right[2]), Math.Max(left[3], right[3]),
      Math.Max(left[4], right[4]), Math.Max(left[5], right[5]), Math.Max(left[6], right[6]), Math.Max(left[7], right[7])
    );

  /// <summary>Computes the maximum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Max(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      Math.Max(left[0], right[0]), Math.Max(left[1], right[1]), Math.Max(left[2], right[2]), Math.Max(left[3], right[3])
    );

  #endregion

  #region Horizontal Add/Subtract

  /// <summary>Horizontally adds adjacent pairs of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> HorizontalAdd(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] + left[1], left[2] + left[3], right[0] + right[1], right[2] + right[3],
      left[4] + left[5], left[6] + left[7], right[4] + right[5], right[6] + right[7]
    );

  /// <summary>Horizontally adds adjacent pairs of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> HorizontalAdd(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] + left[1], right[0] + right[1], left[2] + left[3], right[2] + right[3]
    );

  /// <summary>Horizontally subtracts adjacent pairs of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> HorizontalSubtract(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] - left[1], left[2] - left[3], right[0] - right[1], right[2] - right[3],
      left[4] - left[5], left[6] - left[7], right[4] - right[5], right[6] - right[7]
    );

  /// <summary>Horizontally subtracts adjacent pairs of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> HorizontalSubtract(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] - left[1], right[0] - right[1], left[2] - left[3], right[2] - right[3]
    );

  /// <summary>Performs alternating subtract and add on single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> AddSubtract(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0] - right[0], left[1] + right[1], left[2] - right[2], left[3] + right[3],
      left[4] - right[4], left[5] + right[5], left[6] - right[6], left[7] + right[7]
    );

  /// <summary>Performs alternating subtract and add on double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> AddSubtract(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(
      left[0] - right[0], left[1] + right[1], left[2] - right[2], left[3] + right[3]
    );

  #endregion

  #region Logical Operations

  /// <summary>Computes bitwise AND of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> And(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    return Vector256.As<uint, float>(Vector256.Create(
      l[0] & r[0], l[1] & r[1], l[2] & r[2], l[3] & r[3],
      l[4] & r[4], l[5] & r[5], l[6] & r[6], l[7] & r[7]
    ));
  }

  /// <summary>Computes bitwise AND of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> And(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    return Vector256.As<ulong, double>(Vector256.Create(
      l[0] & r[0], l[1] & r[1], l[2] & r[2], l[3] & r[3]
    ));
  }

  /// <summary>Computes bitwise AND-NOT of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> AndNot(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    return Vector256.As<uint, float>(Vector256.Create(
      ~l[0] & r[0], ~l[1] & r[1], ~l[2] & r[2], ~l[3] & r[3],
      ~l[4] & r[4], ~l[5] & r[5], ~l[6] & r[6], ~l[7] & r[7]
    ));
  }

  /// <summary>Computes bitwise AND-NOT of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> AndNot(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    return Vector256.As<ulong, double>(Vector256.Create(
      ~l[0] & r[0], ~l[1] & r[1], ~l[2] & r[2], ~l[3] & r[3]
    ));
  }

  /// <summary>Computes bitwise OR of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Or(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    return Vector256.As<uint, float>(Vector256.Create(
      l[0] | r[0], l[1] | r[1], l[2] | r[2], l[3] | r[3],
      l[4] | r[4], l[5] | r[5], l[6] | r[6], l[7] | r[7]
    ));
  }

  /// <summary>Computes bitwise OR of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Or(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    return Vector256.As<ulong, double>(Vector256.Create(
      l[0] | r[0], l[1] | r[1], l[2] | r[2], l[3] | r[3]
    ));
  }

  /// <summary>Computes bitwise XOR of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Xor(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    return Vector256.As<uint, float>(Vector256.Create(
      l[0] ^ r[0], l[1] ^ r[1], l[2] ^ r[2], l[3] ^ r[3],
      l[4] ^ r[4], l[5] ^ r[5], l[6] ^ r[6], l[7] ^ r[7]
    ));
  }

  /// <summary>Computes bitwise XOR of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Xor(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    return Vector256.As<ulong, double>(Vector256.Create(
      l[0] ^ r[0], l[1] ^ r[1], l[2] ^ r[2], l[3] ^ r[3]
    ));
  }

  #endregion

  #region Comparison Operations

  /// <summary>Compares packed single-precision floating-point values for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Compare(Vector256<float> left, Vector256<float> right, FloatComparisonMode mode) {
    const uint AllBitsSet = 0xFFFFFFFF;
    const uint NoBitsSet = 0x00000000;

    static uint CompareElements(float l, float r, FloatComparisonMode m) {
      return m switch {
        FloatComparisonMode.OrderedEqualNonSignaling => l == r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedLessThanSignaling => l < r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedLessThanOrEqualSignaling => l <= r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNonSignaling => float.IsNaN(l) || float.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotEqualNonSignaling => l != r || float.IsNaN(l) || float.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotLessThanSignaling => !(l < r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotLessThanOrEqualSignaling => !(l <= r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedNonSignaling => !float.IsNaN(l) && !float.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedEqualNonSignaling => l == r || float.IsNaN(l) || float.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotGreaterThanOrEqualSignaling => !(l >= r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotGreaterThanSignaling => !(l > r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.FalseNonSignaling => NoBitsSet,
        FloatComparisonMode.OrderedNotEqualNonSignaling => l != r && !float.IsNaN(l) && !float.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedGreaterThanOrEqualSignaling => l >= r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedGreaterThanSignaling => l > r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.TrueNonSignaling => AllBitsSet,
        _ => NoBitsSet
      };
    }

    var result = Vector256.Create(
      CompareElements(left[0], right[0], mode),
      CompareElements(left[1], right[1], mode),
      CompareElements(left[2], right[2], mode),
      CompareElements(left[3], right[3], mode),
      CompareElements(left[4], right[4], mode),
      CompareElements(left[5], right[5], mode),
      CompareElements(left[6], right[6], mode),
      CompareElements(left[7], right[7], mode)
    );

    return Vector256.As<uint, float>(result);
  }

  /// <summary>Compares packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Compare(Vector256<double> left, Vector256<double> right, FloatComparisonMode mode) {
    const ulong AllBitsSet = 0xFFFFFFFFFFFFFFFF;
    const ulong NoBitsSet = 0x0000000000000000;

    static ulong CompareElements(double l, double r, FloatComparisonMode m) {
      return m switch {
        FloatComparisonMode.OrderedEqualNonSignaling => l == r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedLessThanSignaling => l < r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedLessThanOrEqualSignaling => l <= r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNonSignaling => double.IsNaN(l) || double.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotEqualNonSignaling => l != r || double.IsNaN(l) || double.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotLessThanSignaling => !(l < r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotLessThanOrEqualSignaling => !(l <= r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedNonSignaling => !double.IsNaN(l) && !double.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedEqualNonSignaling => l == r || double.IsNaN(l) || double.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotGreaterThanOrEqualSignaling => !(l >= r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.UnorderedNotGreaterThanSignaling => !(l > r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.FalseNonSignaling => NoBitsSet,
        FloatComparisonMode.OrderedNotEqualNonSignaling => l != r && !double.IsNaN(l) && !double.IsNaN(r) ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedGreaterThanOrEqualSignaling => l >= r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.OrderedGreaterThanSignaling => l > r ? AllBitsSet : NoBitsSet,
        FloatComparisonMode.TrueNonSignaling => AllBitsSet,
        _ => NoBitsSet
      };
    }

    var result = Vector256.Create(
      CompareElements(left[0], right[0], mode),
      CompareElements(left[1], right[1], mode),
      CompareElements(left[2], right[2], mode),
      CompareElements(left[3], right[3], mode)
    );

    return Vector256.As<ulong, double>(result);
  }

  #endregion

  #region Blend Operations

  /// <summary>Blends packed single-precision floating-point values using control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Blend(Vector256<float> left, Vector256<float> right, byte control)
    => Vector256.Create(
      ((control >> 0) & 1) != 0 ? right[0] : left[0],
      ((control >> 1) & 1) != 0 ? right[1] : left[1],
      ((control >> 2) & 1) != 0 ? right[2] : left[2],
      ((control >> 3) & 1) != 0 ? right[3] : left[3],
      ((control >> 4) & 1) != 0 ? right[4] : left[4],
      ((control >> 5) & 1) != 0 ? right[5] : left[5],
      ((control >> 6) & 1) != 0 ? right[6] : left[6],
      ((control >> 7) & 1) != 0 ? right[7] : left[7]
    );

  /// <summary>Blends packed double-precision floating-point values using control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Blend(Vector256<double> left, Vector256<double> right, byte control)
    => Vector256.Create(
      ((control >> 0) & 1) != 0 ? right[0] : left[0],
      ((control >> 1) & 1) != 0 ? right[1] : left[1],
      ((control >> 2) & 1) != 0 ? right[2] : left[2],
      ((control >> 3) & 1) != 0 ? right[3] : left[3]
    );

  /// <summary>Blends packed single-precision floating-point values using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> BlendVariable(Vector256<float> left, Vector256<float> right, Vector256<float> mask) {
    var m = Vector256.As<float, int>(mask);
    return Vector256.Create(
      m[0] < 0 ? right[0] : left[0],
      m[1] < 0 ? right[1] : left[1],
      m[2] < 0 ? right[2] : left[2],
      m[3] < 0 ? right[3] : left[3],
      m[4] < 0 ? right[4] : left[4],
      m[5] < 0 ? right[5] : left[5],
      m[6] < 0 ? right[6] : left[6],
      m[7] < 0 ? right[7] : left[7]
    );
  }

  /// <summary>Blends packed double-precision floating-point values using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> BlendVariable(Vector256<double> left, Vector256<double> right, Vector256<double> mask) {
    var m = Vector256.As<double, long>(mask);
    return Vector256.Create(
      m[0] < 0 ? right[0] : left[0],
      m[1] < 0 ? right[1] : left[1],
      m[2] < 0 ? right[2] : left[2],
      m[3] < 0 ? right[3] : left[3]
    );
  }

  #endregion

  #region Rounding Operations

  /// <summary>Rounds packed single-precision values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Ceiling(Vector256<float> value)
    => Vector256.Create(
      MathF.Ceiling(value[0]), MathF.Ceiling(value[1]), MathF.Ceiling(value[2]), MathF.Ceiling(value[3]),
      MathF.Ceiling(value[4]), MathF.Ceiling(value[5]), MathF.Ceiling(value[6]), MathF.Ceiling(value[7])
    );

  /// <summary>Rounds packed double-precision values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Ceiling(Vector256<double> value)
    => Vector256.Create(
      Math.Ceiling(value[0]), Math.Ceiling(value[1]), Math.Ceiling(value[2]), Math.Ceiling(value[3])
    );

  /// <summary>Rounds packed single-precision values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Floor(Vector256<float> value)
    => Vector256.Create(
      MathF.Floor(value[0]), MathF.Floor(value[1]), MathF.Floor(value[2]), MathF.Floor(value[3]),
      MathF.Floor(value[4]), MathF.Floor(value[5]), MathF.Floor(value[6]), MathF.Floor(value[7])
    );

  /// <summary>Rounds packed double-precision values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Floor(Vector256<double> value)
    => Vector256.Create(
      Math.Floor(value[0]), Math.Floor(value[1]), Math.Floor(value[2]), Math.Floor(value[3])
    );

  /// <summary>Rounds packed single-precision values toward zero.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> RoundToZero(Vector256<float> value)
    => Vector256.Create(
      MathF.Truncate(value[0]), MathF.Truncate(value[1]), MathF.Truncate(value[2]), MathF.Truncate(value[3]),
      MathF.Truncate(value[4]), MathF.Truncate(value[5]), MathF.Truncate(value[6]), MathF.Truncate(value[7])
    );

  /// <summary>Rounds packed double-precision values toward zero.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> RoundToZero(Vector256<double> value)
    => Vector256.Create(
      Math.Truncate(value[0]), Math.Truncate(value[1]), Math.Truncate(value[2]), Math.Truncate(value[3])
    );

  /// <summary>Rounds packed single-precision values to nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> RoundToNearestInteger(Vector256<float> value)
    => Vector256.Create(
      MathF.Round(value[0], MidpointRounding.ToEven), MathF.Round(value[1], MidpointRounding.ToEven),
      MathF.Round(value[2], MidpointRounding.ToEven), MathF.Round(value[3], MidpointRounding.ToEven),
      MathF.Round(value[4], MidpointRounding.ToEven), MathF.Round(value[5], MidpointRounding.ToEven),
      MathF.Round(value[6], MidpointRounding.ToEven), MathF.Round(value[7], MidpointRounding.ToEven)
    );

  /// <summary>Rounds packed double-precision values to nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> RoundToNearestInteger(Vector256<double> value)
    => Vector256.Create(
      Math.Round(value[0], MidpointRounding.ToEven), Math.Round(value[1], MidpointRounding.ToEven),
      Math.Round(value[2], MidpointRounding.ToEven), Math.Round(value[3], MidpointRounding.ToEven)
    );

  #endregion

  #region Permute Operations

  /// <summary>Permutes single-precision floating-point values using control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Permute(Vector256<float> value, byte control)
    => Vector256.Create(
      value[(control >> 0) & 3],
      value[(control >> 2) & 3],
      value[4 + ((control >> 4) & 3)],
      value[4 + ((control >> 6) & 3)],
      value[(control >> 0) & 3],
      value[(control >> 2) & 3],
      value[4 + ((control >> 4) & 3)],
      value[4 + ((control >> 6) & 3)]
    );

  /// <summary>Permutes double-precision floating-point values using control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Permute(Vector256<double> value, byte control)
    => Vector256.Create(
      value[(control >> 0) & 1],
      value[(control >> 1) & 1],
      value[2 + ((control >> 2) & 1)],
      value[2 + ((control >> 3) & 1)]
    );

  /// <summary>Permutes 128-bit lanes of two 256-bit vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Permute2x128(Vector256<float> left, Vector256<float> right, byte control) {
    static Vector128<float> SelectLane(Vector256<float> l, Vector256<float> r, int selector) {
      return selector switch {
        0 => l.GetLower(),
        1 => l.GetUpper(),
        2 => r.GetLower(),
        3 => r.GetUpper(),
        _ => Vector128<float>.Zero
      };
    }

    var loControl = control & 0x03;
    var hiControl = (control >> 4) & 0x03;
    var loZero = (control & 0x08) != 0;
    var hiZero = (control & 0x80) != 0;

    var lower = loZero ? Vector128<float>.Zero : SelectLane(left, right, loControl);
    var upper = hiZero ? Vector128<float>.Zero : SelectLane(left, right, hiControl);

    return Vector256.Create(lower, upper);
  }

  /// <summary>Permutes 128-bit lanes of two 256-bit vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Permute2x128(Vector256<double> left, Vector256<double> right, byte control) {
    static Vector128<double> SelectLane(Vector256<double> l, Vector256<double> r, int selector) {
      return selector switch {
        0 => l.GetLower(),
        1 => l.GetUpper(),
        2 => r.GetLower(),
        3 => r.GetUpper(),
        _ => Vector128<double>.Zero
      };
    }

    var loControl = control & 0x03;
    var hiControl = (control >> 4) & 0x03;
    var loZero = (control & 0x08) != 0;
    var hiZero = (control & 0x80) != 0;

    var lower = loZero ? Vector128<double>.Zero : SelectLane(left, right, loControl);
    var upper = hiZero ? Vector128<double>.Zero : SelectLane(left, right, hiControl);

    return Vector256.Create(lower, upper);
  }

  #endregion

  #region Shuffle Operations

  /// <summary>Shuffles single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> Shuffle(Vector256<float> value, Vector256<float> right, byte control)
    => Vector256.Create(
      value[(control >> 0) & 3],
      value[(control >> 2) & 3],
      right[(control >> 4) & 3],
      right[(control >> 6) & 3],
      value[4 + ((control >> 0) & 3)],
      value[4 + ((control >> 2) & 3)],
      right[4 + ((control >> 4) & 3)],
      right[4 + ((control >> 6) & 3)]
    );

  /// <summary>Shuffles double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Shuffle(Vector256<double> value, Vector256<double> right, byte control)
    => Vector256.Create(
      value[(control >> 0) & 1],
      right[(control >> 1) & 1],
      value[2 + ((control >> 2) & 1)],
      right[2 + ((control >> 3) & 1)]
    );

  #endregion

  #region Unpack Operations

  /// <summary>Unpacks and interleaves the low single-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> UnpackLow(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[0], right[0], left[1], right[1],
      left[4], right[4], left[5], right[5]
    );

  /// <summary>Unpacks and interleaves the high single-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> UnpackHigh(Vector256<float> left, Vector256<float> right)
    => Vector256.Create(
      left[2], right[2], left[3], right[3],
      left[6], right[6], left[7], right[7]
    );

  /// <summary>Unpacks and interleaves the low double-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> UnpackLow(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(left[0], right[0], left[2], right[2]);

  /// <summary>Unpacks and interleaves the high double-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> UnpackHigh(Vector256<double> left, Vector256<double> right)
    => Vector256.Create(left[1], right[1], left[3], right[3]);

  #endregion

  #region Move Operations

  /// <summary>Duplicates odd-indexed single-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> DuplicateOddIndexed(Vector256<float> value)
    => Vector256.Create(value[1], value[1], value[3], value[3], value[5], value[5], value[7], value[7]);

  /// <summary>Duplicates even-indexed single-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> DuplicateEvenIndexed(Vector256<float> value)
    => Vector256.Create(value[0], value[0], value[2], value[2], value[4], value[4], value[6], value[6]);

  /// <summary>Duplicates double-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> DuplicateEvenIndexed(Vector256<double> value)
    => Vector256.Create(value[0], value[0], value[2], value[2]);

  #endregion

  #region Extract/Insert Lane Operations

  /// <summary>Extracts a 128-bit lane from a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ExtractVector128(Vector256<float> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 128-bit lane from a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ExtractVector128(Vector256<double> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Inserts a 128-bit lane into a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> InsertVector128(Vector256<float> value, Vector128<float> data, byte index)
    => (index & 1) == 0 ? Vector256.Create(data, value.GetUpper()) : Vector256.Create(value.GetLower(), data);

  /// <summary>Inserts a 128-bit lane into a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> InsertVector128(Vector256<double> value, Vector128<double> data, byte index)
    => (index & 1) == 0 ? Vector256.Create(data, value.GetUpper()) : Vector256.Create(value.GetLower(), data);

  #endregion

  #region Load Operations

  /// <summary>Loads 256 bits of float data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<float> LoadVector256(float* address)
    => Vector256.Create(address[0], address[1], address[2], address[3], address[4], address[5], address[6], address[7]);

  /// <summary>Loads 256 bits of double data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> LoadVector256(double* address)
    => Vector256.Create(address[0], address[1], address[2], address[3]);

  /// <summary>Loads 256 bits of byte data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<byte> LoadVector256(byte* address) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 256 bits of sbyte data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<sbyte> LoadVector256(sbyte* address) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 256 bits of int data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<int> LoadVector256(int* address)
    => Vector256.Create(address[0], address[1], address[2], address[3], address[4], address[5], address[6], address[7]);

  /// <summary>Loads 256 bits of uint data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<uint> LoadVector256(uint* address)
    => Vector256.Create(address[0], address[1], address[2], address[3], address[4], address[5], address[6], address[7]);

  /// <summary>Loads 256 bits of long data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<long> LoadVector256(long* address)
    => Vector256.Create(address[0], address[1], address[2], address[3]);

  /// <summary>Loads 256 bits of ulong data from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<ulong> LoadVector256(ulong* address)
    => Vector256.Create(address[0], address[1], address[2], address[3]);

  /// <summary>Broadcasts a single-precision value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<float> BroadcastScalarToVector256(float* source)
    => Vector256.Create(*source);

  /// <summary>Broadcasts a double-precision value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> BroadcastScalarToVector256(double* source)
    => Vector256.Create(*source);

  /// <summary>Broadcasts a 128-bit vector to both lanes of a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<float> BroadcastVector128ToVector256(float* source) {
    var v128 = Vector128.Create(source[0], source[1], source[2], source[3]);
    return Vector256.Create(v128, v128);
  }

  /// <summary>Broadcasts a 128-bit vector to both lanes of a 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> BroadcastVector128ToVector256(double* source) {
    var v128 = Vector128.Create(source[0], source[1]);
    return Vector256.Create(v128, v128);
  }

  #endregion

  #region Store Operations

  /// <summary>Stores 256 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(float* address, Vector256<float> source) {
    for (var i = 0; i < 8; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 256 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(double* address, Vector256<double> source) {
    for (var i = 0; i < 4; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 256 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(byte* address, Vector256<byte> source) {
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 256 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(int* address, Vector256<int> source) {
    for (var i = 0; i < 8; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 256 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(long* address, Vector256<long> source) {
    for (var i = 0; i < 4; ++i)
      address[i] = source[i];
  }

  #endregion

  #region Dot Product

  /// <summary>Computes conditional dot product.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> DotProduct(Vector256<float> left, Vector256<float> right, byte control) {
    var sum1 = 0f;
    var sum2 = 0f;
    for (var i = 0; i < 4; ++i) {
      if (((control >> (i + 4)) & 1) != 0) {
        sum1 += left[i] * right[i];
        sum2 += left[i + 4] * right[i + 4];
      }
    }

    return Vector256.Create(
      ((control >> 0) & 1) != 0 ? sum1 : 0,
      ((control >> 1) & 1) != 0 ? sum1 : 0,
      ((control >> 2) & 1) != 0 ? sum1 : 0,
      ((control >> 3) & 1) != 0 ? sum1 : 0,
      ((control >> 0) & 1) != 0 ? sum2 : 0,
      ((control >> 1) & 1) != 0 ? sum2 : 0,
      ((control >> 2) & 1) != 0 ? sum2 : 0,
      ((control >> 3) & 1) != 0 ? sum2 : 0
    );
  }

  #endregion

  #region Test Operations

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    for (var i = 0; i < 8; ++i)
      if ((l[i] & r[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    for (var i = 0; i < 4; ++i)
      if ((l[i] & r[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector256<float> left, Vector256<float> right) {
    var l = Vector256.As<float, uint>(left);
    var r = Vector256.As<float, uint>(right);
    for (var i = 0; i < 8; ++i)
      if ((~l[i] & r[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector256<double> left, Vector256<double> right) {
    var l = Vector256.As<double, ulong>(left);
    var r = Vector256.As<double, ulong>(right);
    for (var i = 0; i < 4; ++i)
      if ((~l[i] & r[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector256<float> left, Vector256<float> right)
    => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector256<double> left, Vector256<double> right)
    => !TestZ(left, right) && !TestC(left, right);

  #endregion

  #region Mask Operations

  /// <summary>Creates a mask from the most significant bits of each element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector256<float> value) {
    var i = Vector256.As<float, int>(value);
    var mask = 0;
    for (var j = 0; j < 8; ++j)
      if (i[j] < 0)
        mask |= 1 << j;
    return mask;
  }

  /// <summary>Creates a mask from the most significant bits of each element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector256<double> value) {
    var l = Vector256.As<double, long>(value);
    var mask = 0;
    for (var i = 0; i < 4; ++i)
      if (l[i] < 0)
        mask |= 1 << i;
    return mask;
  }

  #endregion

  #region Conversion Operations

  /// <summary>Converts packed 32-bit integers to single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> ConvertToVector256Single(Vector256<int> value)
    => Vector256.Create(
      (float)value[0], (float)value[1], (float)value[2], (float)value[3],
      (float)value[4], (float)value[5], (float)value[6], (float)value[7]
    );

  /// <summary>Converts packed single-precision floating-point values to 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32(Vector256<float> value)
    => Vector256.Create(
      (int)MathF.Round(value[0]), (int)MathF.Round(value[1]), (int)MathF.Round(value[2]), (int)MathF.Round(value[3]),
      (int)MathF.Round(value[4]), (int)MathF.Round(value[5]), (int)MathF.Round(value[6]), (int)MathF.Round(value[7])
    );

  /// <summary>Converts packed single-precision floating-point values to 32-bit integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32WithTruncation(Vector256<float> value)
    => Vector256.Create(
      (int)value[0], (int)value[1], (int)value[2], (int)value[3],
      (int)value[4], (int)value[5], (int)value[6], (int)value[7]
    );

  /// <summary>Converts packed double-precision values to single-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ConvertToVector128Single(Vector256<double> value)
    => Vector128.Create((float)value[0], (float)value[1], (float)value[2], (float)value[3]);

  /// <summary>Converts packed single-precision values to double-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> ConvertToVector256Double(Vector128<float> value)
    => Vector256.Create((double)value[0], (double)value[1], (double)value[2], (double)value[3]);

  /// <summary>Converts packed 32-bit integers to double-precision values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> ConvertToVector256Double(Vector128<int> value)
    => Vector256.Create((double)value[0], (double)value[1], (double)value[2], (double)value[3]);

  /// <summary>Converts packed double-precision values to 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector256<double> value)
    => Vector128.Create(
      (int)Math.Round(value[0]), (int)Math.Round(value[1]),
      (int)Math.Round(value[2]), (int)Math.Round(value[3])
    );

  /// <summary>Converts packed double-precision values to 32-bit integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32WithTruncation(Vector256<double> value)
    => Vector128.Create((int)value[0], (int)value[1], (int)value[2], (int)value[3]);

  #endregion

  #region Zeroing

  /// <summary>Sets all elements to zero.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ZeroAll() {
    // Software fallback - no actual state to zero
  }

  /// <summary>Zeros the upper 128 bits of all YMM registers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void ZeroUpper() {
    // Software fallback - no actual state to zero
  }

  #endregion

  /// <summary>Provides 64-bit specific AVX operations.</summary>
  public new abstract class X64 : Sse42.X64 {

    /// <summary>Gets a value indicating whether 64-bit AVX instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif
