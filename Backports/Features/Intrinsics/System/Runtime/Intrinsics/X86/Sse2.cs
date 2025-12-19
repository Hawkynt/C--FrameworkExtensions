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

#if !SUPPORTS_INTRINSICS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of SSE2 (Streaming SIMD Extensions 2) intrinsics.
/// Provides complete implementations for all SSE2 operations on Vector128 types.
/// </summary>
public abstract class Sse2 : Sse {

  /// <summary>Gets a value indicating whether SSE2 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Constants

  private const byte AllBitsSetByte = 0xFF;
  private const ushort AllBitsSetUShort = 0xFFFF;
  private const uint AllBitsSetUInt = 0xFFFFFFFF;
  private const ulong AllBitsSetULong = 0xFFFFFFFFFFFFFFFF;

  #endregion

  #region Arithmetic Operations - Add

  /// <summary>Adds packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Add(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Add(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Add(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Add(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Add(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(left[0] + right[0], left[1] + right[1], left[2] + right[2], left[3] + right[3]);

  /// <summary>Adds packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Add(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(left[0] + right[0], left[1] + right[1], left[2] + right[2], left[3] + right[3]);

  /// <summary>Adds packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> Add(Vector128<long> left, Vector128<long> right)
    => Vector128.Create(left[0] + right[0], left[1] + right[1]);

  /// <summary>Adds packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> Add(Vector128<ulong> left, Vector128<ulong> right)
    => Vector128.Create(left[0] + right[0], left[1] + right[1]);

  /// <summary>Adds packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Add(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[0] + right[0], left[1] + right[1]);

  /// <summary>Adds the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> AddScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, left[0] + right[0]);

  #endregion

  #region Arithmetic Operations - AddSaturate

  /// <summary>Adds packed 8-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> AddSaturate(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, sum > byte.MaxValue ? byte.MaxValue : (byte)sum);
    }
    return result;
  }

  /// <summary>Adds packed 8-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> AddSaturate(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, sum > sbyte.MaxValue ? sbyte.MaxValue : sum < sbyte.MinValue ? sbyte.MinValue : (sbyte)sum);
    }
    return result;
  }

  /// <summary>Adds packed 16-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> AddSaturate(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, sum > ushort.MaxValue ? ushort.MaxValue : (ushort)sum);
    }
    return result;
  }

  /// <summary>Adds packed 16-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> AddSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, sum > short.MaxValue ? short.MaxValue : sum < short.MinValue ? short.MinValue : (short)sum);
    }
    return result;
  }

  #endregion

  #region Arithmetic Operations - Subtract

  /// <summary>Subtracts packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Subtract(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Subtract(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Subtract(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Subtract(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Subtract(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(left[0] - right[0], left[1] - right[1], left[2] - right[2], left[3] - right[3]);

  /// <summary>Subtracts packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Subtract(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(left[0] - right[0], left[1] - right[1], left[2] - right[2], left[3] - right[3]);

  /// <summary>Subtracts packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> Subtract(Vector128<long> left, Vector128<long> right)
    => Vector128.Create(left[0] - right[0], left[1] - right[1]);

  /// <summary>Subtracts packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> Subtract(Vector128<ulong> left, Vector128<ulong> right)
    => Vector128.Create(left[0] - right[0], left[1] - right[1]);

  /// <summary>Subtracts packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Subtract(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[0] - right[0], left[1] - right[1]);

  /// <summary>Subtracts the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> SubtractScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, left[0] - right[0]);

  #endregion

  #region Arithmetic Operations - SubtractSaturate

  /// <summary>Subtracts packed 8-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> SubtractSaturate(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, diff < 0 ? (byte)0 : (byte)diff);
    }
    return result;
  }

  /// <summary>Subtracts packed 8-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> SubtractSaturate(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, diff > sbyte.MaxValue ? sbyte.MaxValue : diff < sbyte.MinValue ? sbyte.MinValue : (sbyte)diff);
    }
    return result;
  }

  /// <summary>Subtracts packed 16-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> SubtractSaturate(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, diff < 0 ? (ushort)0 : (ushort)diff);
    }
    return result;
  }

  /// <summary>Subtracts packed 16-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> SubtractSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, diff > short.MaxValue ? short.MaxValue : diff < short.MinValue ? short.MinValue : (short)diff);
    }
    return result;
  }

  #endregion

  #region Arithmetic Operations - Multiply

  /// <summary>Multiplies packed 16-bit signed integers and returns the low 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> MultiplyLow(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] * right[i]));
    return result;
  }

  /// <summary>Multiplies packed 16-bit unsigned integers and returns the low 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> MultiplyLow(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] * right[i]));
    return result;
  }

  /// <summary>Multiplies packed 16-bit signed integers and returns the high 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> MultiplyHigh(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)((left[i] * right[i]) >> 16));
    return result;
  }

  /// <summary>Multiplies packed 16-bit unsigned integers and returns the high 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> MultiplyHigh(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)((left[i] * right[i]) >> 16));
    return result;
  }

  /// <summary>Multiplies packed 32-bit unsigned integers and returns the low 32 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> Multiply(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create((ulong)left[0] * right[0], (ulong)left[2] * right[2]);

  /// <summary>Multiplies packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Multiply(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[0] * right[0], left[1] * right[1]);

  /// <summary>Multiplies the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, left[0] * right[0]);

  /// <summary>Multiplies and adds adjacent pairs of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> MultiplyAddAdjacent(Vector128<short> left, Vector128<short> right)
    => Vector128.Create(
      left[0] * right[0] + left[1] * right[1],
      left[2] * right[2] + left[3] * right[3],
      left[4] * right[4] + left[5] * right[5],
      left[6] * right[6] + left[7] * right[7]
    );

  #endregion

  #region Arithmetic Operations - Divide

  /// <summary>Divides packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Divide(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[0] / right[0], left[1] / right[1]);

  /// <summary>Divides the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> DivideScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, left[0] / right[0]);

  #endregion

  #region Arithmetic Operations - Sqrt

  /// <summary>Computes the square root of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Sqrt(Vector128<double> value)
    => Vector128.Create(Math.Sqrt(value[0]), Math.Sqrt(value[1]));

  /// <summary>Computes the square root of the lower double-precision floating-point value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> SqrtScalar(Vector128<double> value)
    => value.WithElement(0, Math.Sqrt(value[0]));

  /// <summary>Computes the square root of the lower double-precision floating-point value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> SqrtScalar(Vector128<double> upper, Vector128<double> value)
    => upper.WithElement(0, Math.Sqrt(value[0]));

  #endregion

  #region Arithmetic Operations - Min/Max

  /// <summary>Computes the minimum of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Min(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes the minimum of packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Min(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes the minimum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Min(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(Math.Min(left[0], right[0]), Math.Min(left[1], right[1]));

  /// <summary>Computes the minimum of the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MinScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, Math.Min(left[0], right[0]));

  /// <summary>Computes the maximum of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Max(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes the maximum of packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Max(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes the maximum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Max(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(Math.Max(left[0], right[0]), Math.Max(left[1], right[1]));

  /// <summary>Computes the maximum of the lower double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MaxScalar(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, Math.Max(left[0], right[0]));

  #endregion

  #region Arithmetic Operations - Average

  /// <summary>Computes the average of packed 8-bit unsigned integers with rounding.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Average(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, (byte)((left[i] + right[i] + 1) >> 1));
    return result;
  }

  /// <summary>Computes the average of packed 16-bit unsigned integers with rounding.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Average(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)((left[i] + right[i] + 1) >> 1));
    return result;
  }

  #endregion

  #region Arithmetic Operations - SumAbsoluteDifferences

  /// <summary>Computes the sum of absolute differences of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> SumAbsoluteDifferences(Vector128<byte> left, Vector128<byte> right) {
    var sum0 = 0;
    var sum1 = 0;
    for (var i = 0; i < 8; ++i)
      sum0 += Math.Abs(left[i] - right[i]);
    for (var i = 8; i < 16; ++i)
      sum1 += Math.Abs(left[i] - right[i]);
    return Vector128.Create((ushort)sum0, (ushort)0, (ushort)0, (ushort)0, (ushort)sum1, (ushort)0, (ushort)0, (ushort)0);
  }

  #endregion

  #region Logical Operations

  /// <summary>Computes the bitwise AND of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> And(Vector128<byte> left, Vector128<byte> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> And(Vector128<sbyte> left, Vector128<sbyte> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> And(Vector128<short> left, Vector128<short> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> And(Vector128<ushort> left, Vector128<ushort> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> And(Vector128<int> left, Vector128<int> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> And(Vector128<uint> left, Vector128<uint> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> And(Vector128<long> left, Vector128<long> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> And(Vector128<ulong> left, Vector128<ulong> right) => Vector128.BitwiseAnd(left, right);

  /// <summary>Computes the bitwise AND of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> And(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.BitwiseAnd(Vector128.AsUInt64(left), Vector128.AsUInt64(right)));

  /// <summary>Computes the bitwise AND-NOT of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> AndNot(Vector128<byte> left, Vector128<byte> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> AndNot(Vector128<sbyte> left, Vector128<sbyte> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> AndNot(Vector128<short> left, Vector128<short> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> AndNot(Vector128<ushort> left, Vector128<ushort> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> AndNot(Vector128<int> left, Vector128<int> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> AndNot(Vector128<uint> left, Vector128<uint> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> AndNot(Vector128<long> left, Vector128<long> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> AndNot(Vector128<ulong> left, Vector128<ulong> right) => Vector128.AndNot(left, right);

  /// <summary>Computes the bitwise AND-NOT of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> AndNot(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.AndNot(Vector128.AsUInt64(left), Vector128.AsUInt64(right)));

  /// <summary>Computes the bitwise OR of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Or(Vector128<byte> left, Vector128<byte> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Or(Vector128<sbyte> left, Vector128<sbyte> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Or(Vector128<short> left, Vector128<short> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Or(Vector128<ushort> left, Vector128<ushort> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Or(Vector128<int> left, Vector128<int> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Or(Vector128<uint> left, Vector128<uint> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> Or(Vector128<long> left, Vector128<long> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> Or(Vector128<ulong> left, Vector128<ulong> right) => Vector128.BitwiseOr(left, right);

  /// <summary>Computes the bitwise OR of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Or(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.BitwiseOr(Vector128.AsUInt64(left), Vector128.AsUInt64(right)));

  /// <summary>Computes the bitwise XOR of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Xor(Vector128<byte> left, Vector128<byte> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Xor(Vector128<sbyte> left, Vector128<sbyte> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Xor(Vector128<short> left, Vector128<short> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Xor(Vector128<ushort> left, Vector128<ushort> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Xor(Vector128<int> left, Vector128<int> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Xor(Vector128<uint> left, Vector128<uint> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> Xor(Vector128<long> left, Vector128<long> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> Xor(Vector128<ulong> left, Vector128<ulong> right) => Vector128.Xor(left, right);

  /// <summary>Computes the bitwise XOR of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Xor(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Xor(Vector128.AsUInt64(left), Vector128.AsUInt64(right)));

  #endregion

  #region Comparison Operations

  /// <summary>Compares packed 8-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> CompareEqual(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? AllBitsSetByte : (byte)0);
    return result;
  }

  /// <summary>Compares packed 8-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> CompareEqual(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? unchecked((sbyte)AllBitsSetByte) : (sbyte)0);
    return result;
  }

  /// <summary>Compares packed 16-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> CompareEqual(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? unchecked((short)AllBitsSetUShort) : (short)0);
    return result;
  }

  /// <summary>Compares packed 16-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> CompareEqual(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? AllBitsSetUShort : (ushort)0);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> CompareEqual(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      left[0] == right[0] ? unchecked((int)AllBitsSetUInt) : 0,
      left[1] == right[1] ? unchecked((int)AllBitsSetUInt) : 0,
      left[2] == right[2] ? unchecked((int)AllBitsSetUInt) : 0,
      left[3] == right[3] ? unchecked((int)AllBitsSetUInt) : 0
    );

  /// <summary>Compares packed 32-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> CompareEqual(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(
      left[0] == right[0] ? AllBitsSetUInt : 0u,
      left[1] == right[1] ? AllBitsSetUInt : 0u,
      left[2] == right[2] ? AllBitsSetUInt : 0u,
      left[3] == right[3] ? AllBitsSetUInt : 0u
    );

  /// <summary>Compares packed double-precision floating-point values for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] == right[0] ? AllBitsSetULong : 0UL,
      left[1] == right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed 8-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> CompareGreaterThan(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? unchecked((sbyte)AllBitsSetByte) : (sbyte)0);
    return result;
  }

  /// <summary>Compares packed 16-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> CompareGreaterThan(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? unchecked((short)AllBitsSetUShort) : (short)0);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> CompareGreaterThan(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      left[0] > right[0] ? unchecked((int)AllBitsSetUInt) : 0,
      left[1] > right[1] ? unchecked((int)AllBitsSetUInt) : 0,
      left[2] > right[2] ? unchecked((int)AllBitsSetUInt) : 0,
      left[3] > right[3] ? unchecked((int)AllBitsSetUInt) : 0
    );

  /// <summary>Compares packed double-precision floating-point values for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareGreaterThan(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] > right[0] ? AllBitsSetULong : 0UL,
      left[1] > right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for greater-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareGreaterThanOrEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] >= right[0] ? AllBitsSetULong : 0UL,
      left[1] >= right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed 8-bit signed integers for less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> CompareLessThan(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? unchecked((sbyte)AllBitsSetByte) : (sbyte)0);
    return result;
  }

  /// <summary>Compares packed 16-bit signed integers for less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> CompareLessThan(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? unchecked((short)AllBitsSetUShort) : (short)0);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> CompareLessThan(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      left[0] < right[0] ? unchecked((int)AllBitsSetUInt) : 0,
      left[1] < right[1] ? unchecked((int)AllBitsSetUInt) : 0,
      left[2] < right[2] ? unchecked((int)AllBitsSetUInt) : 0,
      left[3] < right[3] ? unchecked((int)AllBitsSetUInt) : 0
    );

  /// <summary>Compares packed double-precision floating-point values for less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareLessThan(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] < right[0] ? AllBitsSetULong : 0UL,
      left[1] < right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for less-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareLessThanOrEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] <= right[0] ? AllBitsSetULong : 0UL,
      left[1] <= right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for not-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareNotEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      left[0] != right[0] ? AllBitsSetULong : 0UL,
      left[1] != right[1] ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for not-greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareNotGreaterThan(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      !(left[0] > right[0]) ? AllBitsSetULong : 0UL,
      !(left[1] > right[1]) ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for not-greater-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareNotGreaterThanOrEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      !(left[0] >= right[0]) ? AllBitsSetULong : 0UL,
      !(left[1] >= right[1]) ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for not-less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareNotLessThan(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      !(left[0] < right[0]) ? AllBitsSetULong : 0UL,
      !(left[1] < right[1]) ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for not-less-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareNotLessThanOrEqual(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      !(left[0] <= right[0]) ? AllBitsSetULong : 0UL,
      !(left[1] <= right[1]) ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for ordered (neither is NaN).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareOrdered(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      !double.IsNaN(left[0]) && !double.IsNaN(right[0]) ? AllBitsSetULong : 0UL,
      !double.IsNaN(left[1]) && !double.IsNaN(right[1]) ? AllBitsSetULong : 0UL
    ));

  /// <summary>Compares packed double-precision floating-point values for unordered (at least one is NaN).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareUnordered(Vector128<double> left, Vector128<double> right)
    => Vector128.AsDouble(Vector128.Create(
      double.IsNaN(left[0]) || double.IsNaN(right[0]) ? AllBitsSetULong : 0UL,
      double.IsNaN(left[1]) || double.IsNaN(right[1]) ? AllBitsSetULong : 0UL
    ));

  #endregion

  #region Scalar Comparison Operations

  /// <summary>Compares the lower double-precision floating-point values for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] == right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarGreaterThan(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] > right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for greater-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarGreaterThanOrEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] >= right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarLessThan(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] < right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for less-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarLessThanOrEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] <= right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for not-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarNotEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(left[0] != right[0] ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for not-greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarNotGreaterThan(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(!(left[0] > right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for not-greater-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarNotGreaterThanOrEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(!(left[0] >= right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for not-less-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarNotLessThan(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(!(left[0] < right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for not-less-than-or-equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarNotLessThanOrEqual(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(!(left[0] <= right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for ordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarOrdered(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(!double.IsNaN(left[0]) && !double.IsNaN(right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for unordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CompareScalarUnordered(Vector128<double> left, Vector128<double> right)
    => left.WithElement(0, BitConverter.Int64BitsToDouble(double.IsNaN(left[0]) || double.IsNaN(right[0]) ? unchecked((long)AllBitsSetULong) : 0L));

  /// <summary>Compares the lower double-precision floating-point values for ordered equality (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedEqual(Vector128<double> left, Vector128<double> right) => left[0] == right[0];

  /// <summary>Compares the lower double-precision floating-point values for ordered greater-than (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedGreaterThan(Vector128<double> left, Vector128<double> right) => left[0] > right[0];

  /// <summary>Compares the lower double-precision floating-point values for ordered greater-than-or-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedGreaterThanOrEqual(Vector128<double> left, Vector128<double> right) => left[0] >= right[0];

  /// <summary>Compares the lower double-precision floating-point values for ordered less-than (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedLessThan(Vector128<double> left, Vector128<double> right) => left[0] < right[0];

  /// <summary>Compares the lower double-precision floating-point values for ordered less-than-or-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedLessThanOrEqual(Vector128<double> left, Vector128<double> right) => left[0] <= right[0];

  /// <summary>Compares the lower double-precision floating-point values for ordered not-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarOrderedNotEqual(Vector128<double> left, Vector128<double> right) => left[0] != right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered equality (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedEqual(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] == right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered greater-than (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedGreaterThan(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] > right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered greater-than-or-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedGreaterThanOrEqual(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] >= right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered less-than (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedLessThan(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] < right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered less-than-or-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedLessThanOrEqual(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] <= right[0];

  /// <summary>Compares the lower double-precision floating-point values for unordered not-equal (returns bool).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool CompareScalarUnorderedNotEqual(Vector128<double> left, Vector128<double> right)
    => double.IsNaN(left[0]) || double.IsNaN(right[0]) || left[0] != right[0];

  #endregion

  #region Shift Operations

  /// <summary>Shifts packed 16-bit integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShiftLeftLogical(Vector128<short> value, byte count) {
    if (count >= 16) return Vector128<short>.Zero;
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)(value[i] << count));
    return result;
  }

  /// <summary>Shifts packed 16-bit unsigned integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShiftLeftLogical(Vector128<ushort> value, byte count) {
    if (count >= 16) return Vector128<ushort>.Zero;
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(value[i] << count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ShiftLeftLogical(Vector128<int> value, byte count) {
    if (count >= 32) return Vector128<int>.Zero;
    return Vector128.Create(value[0] << count, value[1] << count, value[2] << count, value[3] << count);
  }

  /// <summary>Shifts packed 32-bit unsigned integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ShiftLeftLogical(Vector128<uint> value, byte count) {
    if (count >= 32) return Vector128<uint>.Zero;
    return Vector128.Create(value[0] << count, value[1] << count, value[2] << count, value[3] << count);
  }

  /// <summary>Shifts packed 64-bit integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ShiftLeftLogical(Vector128<long> value, byte count) {
    if (count >= 64) return Vector128<long>.Zero;
    return Vector128.Create(value[0] << count, value[1] << count);
  }

  /// <summary>Shifts packed 64-bit unsigned integers left by count bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ShiftLeftLogical(Vector128<ulong> value, byte count) {
    if (count >= 64) return Vector128<ulong>.Zero;
    return Vector128.Create(value[0] << count, value[1] << count);
  }

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> ShiftLeftLogical128BitLane(Vector128<sbyte> value, byte numBytes) {
    if (numBytes >= 16) return Vector128<sbyte>.Zero;
    var result = Vector128<sbyte>.Zero;
    for (var i = numBytes; i < 16; ++i)
      result = result.WithElement(i, value[i - numBytes]);
    return result;
  }

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> ShiftLeftLogical128BitLane(Vector128<byte> value, byte numBytes) {
    if (numBytes >= 16) return Vector128<byte>.Zero;
    var result = Vector128<byte>.Zero;
    for (var i = numBytes; i < 16; ++i)
      result = result.WithElement(i, value[i - numBytes]);
    return result;
  }

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShiftLeftLogical128BitLane(Vector128<short> value, byte numBytes)
    => Vector128.AsInt16(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShiftLeftLogical128BitLane(Vector128<ushort> value, byte numBytes)
    => Vector128.AsUInt16(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ShiftLeftLogical128BitLane(Vector128<int> value, byte numBytes)
    => Vector128.AsInt32(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ShiftLeftLogical128BitLane(Vector128<uint> value, byte numBytes)
    => Vector128.AsUInt32(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ShiftLeftLogical128BitLane(Vector128<long> value, byte numBytes)
    => Vector128.AsInt64(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector left by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ShiftLeftLogical128BitLane(Vector128<ulong> value, byte numBytes)
    => Vector128.AsUInt64(ShiftLeftLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts packed 16-bit integers right by count bits (arithmetic).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShiftRightArithmetic(Vector128<short> value, byte count) {
    if (count >= 16) count = 15;
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)(value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right by count bits (arithmetic).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ShiftRightArithmetic(Vector128<int> value, byte count) {
    if (count >= 32) count = 31;
    return Vector128.Create(value[0] >> count, value[1] >> count, value[2] >> count, value[3] >> count);
  }

  /// <summary>Shifts packed 16-bit integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShiftRightLogical(Vector128<short> value, byte count) {
    if (count >= 16) return Vector128<short>.Zero;
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, (short)((ushort)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 16-bit unsigned integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShiftRightLogical(Vector128<ushort> value, byte count) {
    if (count >= 16) return Vector128<ushort>.Zero;
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ShiftRightLogical(Vector128<int> value, byte count) {
    if (count >= 32) return Vector128<int>.Zero;
    return Vector128.Create((int)((uint)value[0] >> count), (int)((uint)value[1] >> count), (int)((uint)value[2] >> count), (int)((uint)value[3] >> count));
  }

  /// <summary>Shifts packed 32-bit unsigned integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ShiftRightLogical(Vector128<uint> value, byte count) {
    if (count >= 32) return Vector128<uint>.Zero;
    return Vector128.Create(value[0] >> count, value[1] >> count, value[2] >> count, value[3] >> count);
  }

  /// <summary>Shifts packed 64-bit integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ShiftRightLogical(Vector128<long> value, byte count) {
    if (count >= 64) return Vector128<long>.Zero;
    return Vector128.Create((long)((ulong)value[0] >> count), (long)((ulong)value[1] >> count));
  }

  /// <summary>Shifts packed 64-bit unsigned integers right by count bits (logical).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ShiftRightLogical(Vector128<ulong> value, byte count) {
    if (count >= 64) return Vector128<ulong>.Zero;
    return Vector128.Create(value[0] >> count, value[1] >> count);
  }

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> ShiftRightLogical128BitLane(Vector128<sbyte> value, byte numBytes) {
    if (numBytes >= 16) return Vector128<sbyte>.Zero;
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < 16 - numBytes; ++i)
      result = result.WithElement(i, value[i + numBytes]);
    return result;
  }

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> ShiftRightLogical128BitLane(Vector128<byte> value, byte numBytes) {
    if (numBytes >= 16) return Vector128<byte>.Zero;
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < 16 - numBytes; ++i)
      result = result.WithElement(i, value[i + numBytes]);
    return result;
  }

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShiftRightLogical128BitLane(Vector128<short> value, byte numBytes)
    => Vector128.AsInt16(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShiftRightLogical128BitLane(Vector128<ushort> value, byte numBytes)
    => Vector128.AsUInt16(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ShiftRightLogical128BitLane(Vector128<int> value, byte numBytes)
    => Vector128.AsInt32(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ShiftRightLogical128BitLane(Vector128<uint> value, byte numBytes)
    => Vector128.AsUInt32(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ShiftRightLogical128BitLane(Vector128<long> value, byte numBytes)
    => Vector128.AsInt64(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  /// <summary>Shifts the vector right by count bytes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ShiftRightLogical128BitLane(Vector128<ulong> value, byte numBytes)
    => Vector128.AsUInt64(ShiftRightLogical128BitLane(Vector128.AsByte(value), numBytes));

  #endregion

  #region Shuffle Operations

  /// <summary>Shuffles 32-bit integers using a control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Shuffle(Vector128<int> value, byte control)
    => Vector128.Create(
      value[control & 0x3],
      value[(control >> 2) & 0x3],
      value[(control >> 4) & 0x3],
      value[(control >> 6) & 0x3]
    );

  /// <summary>Shuffles 32-bit unsigned integers using a control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Shuffle(Vector128<uint> value, byte control)
    => Vector128.Create(
      value[control & 0x3],
      value[(control >> 2) & 0x3],
      value[(control >> 4) & 0x3],
      value[(control >> 6) & 0x3]
    );

  /// <summary>Shuffles double-precision floating-point values using a control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Shuffle(Vector128<double> left, Vector128<double> right, byte control)
    => Vector128.Create(
      left[control & 0x1],
      right[(control >> 1) & 0x1]
    );

  /// <summary>Shuffles the high 64-bit integers in each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShuffleHigh(Vector128<short> value, byte control)
    => Vector128.Create(
      value[0], value[1], value[2], value[3],
      value[4 + (control & 0x3)],
      value[4 + ((control >> 2) & 0x3)],
      value[4 + ((control >> 4) & 0x3)],
      value[4 + ((control >> 6) & 0x3)]
    );

  /// <summary>Shuffles the high 64-bit integers in each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShuffleHigh(Vector128<ushort> value, byte control)
    => Vector128.Create(
      value[0], value[1], value[2], value[3],
      value[4 + (control & 0x3)],
      value[4 + ((control >> 2) & 0x3)],
      value[4 + ((control >> 4) & 0x3)],
      value[4 + ((control >> 6) & 0x3)]
    );

  /// <summary>Shuffles the low 64-bit integers in each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ShuffleLow(Vector128<short> value, byte control)
    => Vector128.Create(
      value[control & 0x3],
      value[(control >> 2) & 0x3],
      value[(control >> 4) & 0x3],
      value[(control >> 6) & 0x3],
      value[4], value[5], value[6], value[7]
    );

  /// <summary>Shuffles the low 64-bit integers in each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ShuffleLow(Vector128<ushort> value, byte control)
    => Vector128.Create(
      value[control & 0x3],
      value[(control >> 2) & 0x3],
      value[(control >> 4) & 0x3],
      value[(control >> 6) & 0x3],
      value[4], value[5], value[6], value[7]
    );

  #endregion

  #region Unpack Operations

  /// <summary>Unpacks and interleaves the high 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> UnpackHigh(Vector128<byte> left, Vector128<byte> right)
    => Vector128.Create(
      left[8], right[8], left[9], right[9], left[10], right[10], left[11], right[11],
      left[12], right[12], left[13], right[13], left[14], right[14], left[15], right[15]
    );

  /// <summary>Unpacks and interleaves the high 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> UnpackHigh(Vector128<sbyte> left, Vector128<sbyte> right)
    => Vector128.Create(
      left[8], right[8], left[9], right[9], left[10], right[10], left[11], right[11],
      left[12], right[12], left[13], right[13], left[14], right[14], left[15], right[15]
    );

  /// <summary>Unpacks and interleaves the high 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> UnpackHigh(Vector128<short> left, Vector128<short> right)
    => Vector128.Create(left[4], right[4], left[5], right[5], left[6], right[6], left[7], right[7]);

  /// <summary>Unpacks and interleaves the high 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> UnpackHigh(Vector128<ushort> left, Vector128<ushort> right)
    => Vector128.Create(left[4], right[4], left[5], right[5], left[6], right[6], left[7], right[7]);

  /// <summary>Unpacks and interleaves the high 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> UnpackHigh(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(left[2], right[2], left[3], right[3]);

  /// <summary>Unpacks and interleaves the high 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> UnpackHigh(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(left[2], right[2], left[3], right[3]);

  /// <summary>Unpacks and interleaves the high 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> UnpackHigh(Vector128<long> left, Vector128<long> right)
    => Vector128.Create(left[1], right[1]);

  /// <summary>Unpacks and interleaves the high 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> UnpackHigh(Vector128<ulong> left, Vector128<ulong> right)
    => Vector128.Create(left[1], right[1]);

  /// <summary>Unpacks and interleaves the high double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> UnpackHigh(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[1], right[1]);

  /// <summary>Unpacks and interleaves the low 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> UnpackLow(Vector128<byte> left, Vector128<byte> right)
    => Vector128.Create(
      left[0], right[0], left[1], right[1], left[2], right[2], left[3], right[3],
      left[4], right[4], left[5], right[5], left[6], right[6], left[7], right[7]
    );

  /// <summary>Unpacks and interleaves the low 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> UnpackLow(Vector128<sbyte> left, Vector128<sbyte> right)
    => Vector128.Create(
      left[0], right[0], left[1], right[1], left[2], right[2], left[3], right[3],
      left[4], right[4], left[5], right[5], left[6], right[6], left[7], right[7]
    );

  /// <summary>Unpacks and interleaves the low 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> UnpackLow(Vector128<short> left, Vector128<short> right)
    => Vector128.Create(left[0], right[0], left[1], right[1], left[2], right[2], left[3], right[3]);

  /// <summary>Unpacks and interleaves the low 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> UnpackLow(Vector128<ushort> left, Vector128<ushort> right)
    => Vector128.Create(left[0], right[0], left[1], right[1], left[2], right[2], left[3], right[3]);

  /// <summary>Unpacks and interleaves the low 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> UnpackLow(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(left[0], right[0], left[1], right[1]);

  /// <summary>Unpacks and interleaves the low 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> UnpackLow(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(left[0], right[0], left[1], right[1]);

  /// <summary>Unpacks and interleaves the low 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> UnpackLow(Vector128<long> left, Vector128<long> right)
    => Vector128.Create(left[0], right[0]);

  /// <summary>Unpacks and interleaves the low 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> UnpackLow(Vector128<ulong> left, Vector128<ulong> right)
    => Vector128.Create(left[0], right[0]);

  /// <summary>Unpacks and interleaves the low double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> UnpackLow(Vector128<double> left, Vector128<double> right)
    => Vector128.Create(left[0], right[0]);

  #endregion

  #region Pack Operations

  /// <summary>Packs 16-bit signed integers to 8-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> PackSignedSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < 8; ++i) {
      var l = left[i];
      var r = right[i];
      result = result.WithElement(i, l > sbyte.MaxValue ? sbyte.MaxValue : l < sbyte.MinValue ? sbyte.MinValue : (sbyte)l);
      result = result.WithElement(i + 8, r > sbyte.MaxValue ? sbyte.MaxValue : r < sbyte.MinValue ? sbyte.MinValue : (sbyte)r);
    }
    return result;
  }

  /// <summary>Packs 32-bit signed integers to 16-bit signed integers with signed saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> PackSignedSaturate(Vector128<int> left, Vector128<int> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < 4; ++i) {
      var l = left[i];
      var r = right[i];
      result = result.WithElement(i, l > short.MaxValue ? short.MaxValue : l < short.MinValue ? short.MinValue : (short)l);
      result = result.WithElement(i + 4, r > short.MaxValue ? short.MaxValue : r < short.MinValue ? short.MinValue : (short)r);
    }
    return result;
  }

  /// <summary>Packs 16-bit signed integers to 8-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> PackUnsignedSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < 8; ++i) {
      var l = left[i];
      var r = right[i];
      result = result.WithElement(i, l > byte.MaxValue ? byte.MaxValue : l < 0 ? (byte)0 : (byte)l);
      result = result.WithElement(i + 8, r > byte.MaxValue ? byte.MaxValue : r < 0 ? (byte)0 : (byte)r);
    }
    return result;
  }

  #endregion

  #region Load Operations

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<byte> LoadVector128(byte* address)
    => Vector128.Create(
      address[0], address[1], address[2], address[3],
      address[4], address[5], address[6], address[7],
      address[8], address[9], address[10], address[11],
      address[12], address[13], address[14], address[15]
    );

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<sbyte> LoadVector128(sbyte* address)
    => Vector128.Create(
      address[0], address[1], address[2], address[3],
      address[4], address[5], address[6], address[7],
      address[8], address[9], address[10], address[11],
      address[12], address[13], address[14], address[15]
    );

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<short> LoadVector128(short* address)
    => Vector128.Create(address[0], address[1], address[2], address[3], address[4], address[5], address[6], address[7]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ushort> LoadVector128(ushort* address)
    => Vector128.Create(address[0], address[1], address[2], address[3], address[4], address[5], address[6], address[7]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadVector128(int* address)
    => Vector128.Create(address[0], address[1], address[2], address[3]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadVector128(uint* address)
    => Vector128.Create(address[0], address[1], address[2], address[3]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadVector128(long* address)
    => Vector128.Create(address[0], address[1]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadVector128(ulong* address)
    => Vector128.Create(address[0], address[1]);

  /// <summary>Loads a vector from the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadVector128(double* address)
    => Vector128.Create(address[0], address[1]);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<byte> LoadAlignedVector128(byte* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<sbyte> LoadAlignedVector128(sbyte* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<short> LoadAlignedVector128(short* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ushort> LoadAlignedVector128(ushort* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadAlignedVector128(int* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadAlignedVector128(uint* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadAlignedVector128(long* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadAlignedVector128(ulong* address) => LoadVector128(address);

  /// <summary>Loads a vector from the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadAlignedVector128(double* address) => LoadVector128(address);

  /// <summary>Loads a scalar value into the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadScalarVector128(int* address)
    => Vector128.Create(*address, 0, 0, 0);

  /// <summary>Loads a scalar value into the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadScalarVector128(uint* address)
    => Vector128.Create(*address, 0u, 0u, 0u);

  /// <summary>Loads a scalar value into the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadScalarVector128(long* address)
    => Vector128.Create(*address, 0L);

  /// <summary>Loads a scalar value into the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadScalarVector128(ulong* address)
    => Vector128.Create(*address, 0UL);

  /// <summary>Loads a scalar value into the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadScalarVector128(double* address)
    => Vector128.Create(*address, 0.0);

  /// <summary>Loads 64 bits into the high half of the vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadHigh(Vector128<double> lower, double* address)
    => Vector128.Create(lower[0], *address);

  /// <summary>Loads 64 bits into the low half of the vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<double> LoadLow(Vector128<double> upper, double* address)
    => Vector128.Create(*address, upper[1]);

  #endregion

  #region Store Operations

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(byte* address, Vector128<byte> source) {
    for (var i = 0; i < 16; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(sbyte* address, Vector128<sbyte> source) {
    for (var i = 0; i < 16; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(short* address, Vector128<short> source) {
    for (var i = 0; i < 8; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ushort* address, Vector128<ushort> source) {
    for (var i = 0; i < 8; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(int* address, Vector128<int> source) {
    for (var i = 0; i < 4; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(uint* address, Vector128<uint> source) {
    for (var i = 0; i < 4; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(long* address, Vector128<long> source) {
    for (var i = 0; i < 2; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ulong* address, Vector128<ulong> source) {
    for (var i = 0; i < 2; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores a vector to the given address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(double* address, Vector128<double> source) {
    address[0] = source[0];
    address[1] = source[1];
  }

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(byte* address, Vector128<byte> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(sbyte* address, Vector128<sbyte> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(short* address, Vector128<short> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(ushort* address, Vector128<ushort> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(int* address, Vector128<int> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(uint* address, Vector128<uint> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(long* address, Vector128<long> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(ulong* address, Vector128<ulong> source) => Store(address, source);

  /// <summary>Stores a vector to the given aligned address.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(double* address, Vector128<double> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(byte* address, Vector128<byte> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(sbyte* address, Vector128<sbyte> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(short* address, Vector128<short> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(ushort* address, Vector128<ushort> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(int* address, Vector128<int> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(uint* address, Vector128<uint> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(long* address, Vector128<long> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(ulong* address, Vector128<ulong> source) => Store(address, source);

  /// <summary>Stores a vector using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(double* address, Vector128<double> source) => Store(address, source);

  /// <summary>Stores the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreScalar(double* address, Vector128<double> source) => *address = source[0];

  /// <summary>Stores the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreScalar(int* address, Vector128<int> source) => *address = source[0];

  /// <summary>Stores the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreScalar(uint* address, Vector128<uint> source) => *address = source[0];

  /// <summary>Stores the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreScalar(long* address, Vector128<long> source) => *address = source[0];

  /// <summary>Stores the lower element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreScalar(ulong* address, Vector128<ulong> source) => *address = source[0];

  /// <summary>Stores the high 64 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreHigh(double* address, Vector128<double> source) => *address = source[1];

  /// <summary>Stores the low 64 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreLow(double* address, Vector128<double> source) => *address = source[0];

  /// <summary>Stores a value using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreNonTemporal(int* address, int value) => *address = value;

  /// <summary>Stores a value using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreNonTemporal(uint* address, uint value) => *address = value;

  #endregion

  #region Move Operations

  /// <summary>Moves the lower double-precision floating-point value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MoveScalar(Vector128<double> upper, Vector128<double> value)
    => Vector128.Create(value[0], upper[1]);

  /// <summary>Extracts the sign bits from packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector128<double> value) {
    var bits = Vector128.AsUInt64(value);
    var result = 0;
    if ((bits[0] & 0x8000000000000000UL) != 0) result |= 1;
    if ((bits[1] & 0x8000000000000000UL) != 0) result |= 2;
    return result;
  }

  /// <summary>Extracts the sign bits from packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector128<sbyte> value) {
    var result = 0;
    for (var i = 0; i < 16; ++i)
      if (value[i] < 0)
        result |= 1 << i;
    return result;
  }

  /// <summary>Extracts the sign bits from packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector128<byte> value) {
    var result = 0;
    for (var i = 0; i < 16; ++i)
      if ((value[i] & 0x80) != 0)
        result |= 1 << i;
    return result;
  }

  /// <summary>Conditionally stores bytes based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void MaskMove(Vector128<sbyte> source, Vector128<sbyte> mask, sbyte* address) {
    for (var i = 0; i < 16; ++i)
      if (mask[i] < 0)
        address[i] = source[i];
  }

  /// <summary>Conditionally stores bytes based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void MaskMove(Vector128<byte> source, Vector128<byte> mask, byte* address) {
    for (var i = 0; i < 16; ++i)
      if ((mask[i] & 0x80) != 0)
        address[i] = source[i];
  }

  #endregion

  #region Conversion Operations

  /// <summary>Converts the lower double-precision floating-point value to a 32-bit integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ConvertToInt32(Vector128<double> value) => (int)Math.Round(value[0]);

  /// <summary>Converts the lower double-precision floating-point value to a 32-bit integer with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ConvertToInt32WithTruncation(Vector128<double> value) => (int)value[0];

  /// <summary>Converts the lower 32-bit integer to a value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ConvertToInt32(Vector128<int> value) => value[0];

  /// <summary>Converts the lower 32-bit unsigned integer to a value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ConvertToUInt32(Vector128<uint> value) => value[0];

  /// <summary>Converts packed 32-bit signed integers to double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ConvertToVector128Double(Vector128<int> value)
    => Vector128.Create((double)value[0], (double)value[1]);

  /// <summary>Converts packed single-precision floating-point values to double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ConvertToVector128Double(Vector128<float> value)
    => Vector128.Create((double)value[0], (double)value[1]);

  /// <summary>Converts the lower scalar single-precision floating-point to double-precision.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ConvertScalarToVector128Double(Vector128<double> upper, Vector128<float> value)
    => Vector128.Create((double)value[0], upper[1]);

  /// <summary>Converts packed double-precision floating-point values to 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<double> value)
    => Vector128.Create((int)Math.Round(value[0]), (int)Math.Round(value[1]), 0, 0);

  /// <summary>Converts packed single-precision floating-point values to 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<float> value)
    => Vector128.Create(
      (int)Math.Round(value[0]),
      (int)Math.Round(value[1]),
      (int)Math.Round(value[2]),
      (int)Math.Round(value[3])
    );

  /// <summary>Converts packed double-precision floating-point values to 32-bit signed integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32WithTruncation(Vector128<double> value)
    => Vector128.Create((int)value[0], (int)value[1], 0, 0);

  /// <summary>Converts packed single-precision floating-point values to 32-bit signed integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32WithTruncation(Vector128<float> value)
    => Vector128.Create((int)value[0], (int)value[1], (int)value[2], (int)value[3]);

  /// <summary>Converts packed 32-bit signed integers to single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ConvertToVector128Single(Vector128<int> value)
    => Vector128.Create((float)value[0], (float)value[1], (float)value[2], (float)value[3]);

  /// <summary>Converts packed double-precision floating-point values to single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ConvertToVector128Single(Vector128<double> value)
    => Vector128.Create((float)value[0], (float)value[1], 0f, 0f);

  /// <summary>Converts the lower scalar double-precision floating-point to single-precision.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ConvertScalarToVector128Single(Vector128<float> upper, Vector128<double> value)
    => Vector128.Create((float)value[0], upper[1], upper[2], upper[3]);

  /// <summary>Converts a 32-bit integer to a scalar in a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertScalarToVector128Int32(int value)
    => Vector128.Create(value, 0, 0, 0);

  /// <summary>Converts a 32-bit unsigned integer to a scalar in a vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ConvertScalarToVector128UInt32(uint value)
    => Vector128.Create(value, 0u, 0u, 0u);

  /// <summary>Converts a 32-bit integer to a double-precision floating-point value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ConvertScalarToVector128Double(Vector128<double> upper, int value)
    => Vector128.Create((double)value, upper[1]);

  #endregion

  #region Extract/Insert Operations

  /// <summary>Extracts a 16-bit integer from the specified position.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort Extract(Vector128<ushort> value, byte index) => value[index & 0x7];

  /// <summary>Inserts a 16-bit integer at the specified position.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Insert(Vector128<short> value, short data, byte index)
    => value.WithElement(index & 0x7, data);

  /// <summary>Inserts a 16-bit unsigned integer at the specified position.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Insert(Vector128<ushort> value, ushort data, byte index)
    => value.WithElement(index & 0x7, data);

  #endregion

  #region Memory Fence

  /// <summary>Serializes all load and store operations.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void MemoryFence() => System.Threading.Thread.MemoryBarrier();

  /// <summary>Serializes all load operations.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void LoadFence() => System.Threading.Thread.MemoryBarrier();

  /// <summary>Serializes all store operations.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void StoreFence() => System.Threading.Thread.MemoryBarrier();

  #endregion

  #region X64 Nested Class

  /// <summary>Provides 64-bit specific SSE2 operations.</summary>
  public new abstract class X64 : Sse.X64 {
    /// <summary>Gets a value indicating whether 64-bit SSE2 instructions are supported.</summary>
    public new static bool IsSupported => false;

    /// <summary>Converts the lower double-precision floating-point value to a 64-bit integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64(Vector128<double> value) => (long)Math.Round(value[0]);

    /// <summary>Converts the lower double-precision floating-point value to a 64-bit integer with truncation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64WithTruncation(Vector128<double> value) => (long)value[0];

    /// <summary>Converts the lower 64-bit integer to a value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64(Vector128<long> value) => value[0];

    /// <summary>Converts the lower 64-bit unsigned integer to a value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertToUInt64(Vector128<ulong> value) => value[0];

    /// <summary>Converts a 64-bit integer to a scalar in a vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> ConvertScalarToVector128Int64(long value)
      => Vector128.Create(value, 0L);

    /// <summary>Converts a 64-bit unsigned integer to a scalar in a vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> ConvertScalarToVector128UInt64(ulong value)
      => Vector128.Create(value, 0UL);

    /// <summary>Converts a 64-bit integer to a double-precision floating-point value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<double> ConvertScalarToVector128Double(Vector128<double> upper, long value)
      => Vector128.Create((double)value, upper[1]);

    /// <summary>Stores a value using non-temporal hint.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreNonTemporal(long* address, long value) => *address = value;

    /// <summary>Stores a value using non-temporal hint.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void StoreNonTemporal(ulong* address, ulong value) => *address = value;
  }

  #endregion
}

#endif
