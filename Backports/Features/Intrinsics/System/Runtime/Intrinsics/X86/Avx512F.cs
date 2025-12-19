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

#if !SUPPORTS_INTRINSICS_AVX512F

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of AVX-512F (Foundation) intrinsics.
/// Provides 512-bit vector operations for float, double, and integer types.
/// </summary>
#if SUPPORTS_INTRINSICS
public static class Avx512F {
#else
public abstract class Avx512F : Avx2 {
#endif

  /// <summary>Gets a value indicating whether AVX-512F instructions are supported.</summary>
  public
#if !SUPPORTS_INTRINSICS
  new
#endif
  static bool IsSupported => false;

  #region Add Operations

  /// <summary>Adds packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Add(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Add(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Add(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Add(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Add(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Add(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  #endregion

  #region Subtract Operations

  /// <summary>Subtracts packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Subtract(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Subtract(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Subtract(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Subtract(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Subtract(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Subtract(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  #endregion

  #region Multiply Operations

  /// <summary>Multiplies packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Multiply(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  /// <summary>Multiplies packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Multiply(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  /// <summary>Multiplies packed 32-bit signed integers (low 32 bits of result).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> MultiplyLow(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  /// <summary>Multiplies packed 32-bit unsigned integers (low 32 bits of result).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> MultiplyLow(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  #endregion

  #region Divide Operations

  /// <summary>Divides packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Divide(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] / right[i]);
    return result;
  }

  /// <summary>Divides packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Divide(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] / right[i]);
    return result;
  }

  #endregion

  #region Logical Operations

  /// <summary>Computes bitwise AND of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> And(Vector512<float> left, Vector512<float> right)
    => Vector512.As<int, float>(Vector512.As<float, int>(left) & Vector512.As<float, int>(right));

  /// <summary>Computes bitwise AND of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> And(Vector512<double> left, Vector512<double> right)
    => Vector512.As<long, double>(Vector512.As<double, long>(left) & Vector512.As<double, long>(right));

  /// <summary>Computes bitwise AND of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> And(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> And(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> And(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> And(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND-NOT of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> AndNot(Vector512<float> left, Vector512<float> right)
    => Vector512.As<int, float>(~Vector512.As<float, int>(left) & Vector512.As<float, int>(right));

  /// <summary>Computes bitwise AND-NOT of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> AndNot(Vector512<double> left, Vector512<double> right)
    => Vector512.As<long, double>(~Vector512.As<double, long>(left) & Vector512.As<double, long>(right));

  /// <summary>Computes bitwise AND-NOT of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> AndNot(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND-NOT of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> AndNot(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND-NOT of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> AndNot(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND-NOT of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> AndNot(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Or(Vector512<float> left, Vector512<float> right)
    => Vector512.As<int, float>(Vector512.As<float, int>(left) | Vector512.As<float, int>(right));

  /// <summary>Computes bitwise OR of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Or(Vector512<double> left, Vector512<double> right)
    => Vector512.As<long, double>(Vector512.As<double, long>(left) | Vector512.As<double, long>(right));

  /// <summary>Computes bitwise OR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Or(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Or(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Or(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Or(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Xor(Vector512<float> left, Vector512<float> right)
    => Vector512.As<int, float>(Vector512.As<float, int>(left) ^ Vector512.As<float, int>(right));

  /// <summary>Computes bitwise XOR of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Xor(Vector512<double> left, Vector512<double> right)
    => Vector512.As<long, double>(Vector512.As<double, long>(left) ^ Vector512.As<double, long>(right));

  /// <summary>Computes bitwise XOR of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> Xor(Vector512<byte> left, Vector512<byte> right) {
    var result = Vector512<byte>.Zero;
    for (var i = 0; i < Vector512<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] ^ right[i]));
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Xor(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Xor(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Xor(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Xor(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  #endregion

  #region Comparison Operations

  /// <summary>Compares packed single-precision floating-point values for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareEqual(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareEqual(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> CompareEqual(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    const int allBitsSet = unchecked((int)0xFFFFFFFF);
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? allBitsSet : 0);
    return result;
  }

  /// <summary>Compares packed 64-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> CompareEqual(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    const long allBitsSet = unchecked((long)0xFFFFFFFFFFFFFFFF);
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? allBitsSet : 0L);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for greater than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareGreaterThan(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for greater than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareGreaterThan(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for greater than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> CompareGreaterThan(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    const int allBitsSet = unchecked((int)0xFFFFFFFF);
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? allBitsSet : 0);
    return result;
  }

  /// <summary>Compares packed 64-bit signed integers for greater than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> CompareGreaterThan(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    const long allBitsSet = unchecked((long)0xFFFFFFFFFFFFFFFF);
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? allBitsSet : 0L);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for less than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareLessThan(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for less than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareLessThan(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for greater than or equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareGreaterThanOrEqual(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] >= right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for greater than or equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareGreaterThanOrEqual(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] >= right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for less than or equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareLessThanOrEqual(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] <= right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for less than or equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareLessThanOrEqual(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] <= right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for not equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareNotEqual(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] != right[i] ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for not equal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareNotEqual(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] != right[i] ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for unordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareUnordered(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, float.IsNaN(left[i]) || float.IsNaN(right[i]) ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for unordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareUnordered(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, double.IsNaN(left[i]) || double.IsNaN(right[i]) ? allBitsSet : 0d);
    return result;
  }

  /// <summary>Compares packed single-precision floating-point values for ordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> CompareOrdered(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    var allBitsSet = BitConverter.Int32BitsToSingle(unchecked((int)0xFFFFFFFF));
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, !float.IsNaN(left[i]) && !float.IsNaN(right[i]) ? allBitsSet : 0f);
    return result;
  }

  /// <summary>Compares packed double-precision floating-point values for ordered.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> CompareOrdered(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    var allBitsSet = BitConverter.Int64BitsToDouble(unchecked((long)0xFFFFFFFFFFFFFFFF));
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, !double.IsNaN(left[i]) && !double.IsNaN(right[i]) ? allBitsSet : 0d);
    return result;
  }

  #endregion

  #region Min/Max Operations

  /// <summary>Computes the minimum of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Min(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, MathF.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Min(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Min(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Min(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Min(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Min(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Max(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, MathF.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Max(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Max(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Max(Vector512<uint> left, Vector512<uint> right) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Max(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Max(Vector512<ulong> left, Vector512<ulong> right) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  #endregion

  #region Absolute Value Operations

  /// <summary>Computes the absolute value of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Abs(Vector512<int> value) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      var v = value[i];
      result = result.WithElement(i, v >= 0 ? (uint)v : unchecked((uint)(-v)));
    }
    return result;
  }

  /// <summary>Computes the absolute value of packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Abs(Vector512<long> value) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      var v = value[i];
      result = result.WithElement(i, v >= 0 ? (ulong)v : unchecked((ulong)(-v)));
    }
    return result;
  }

  #endregion

  #region Square Root Operations

  /// <summary>Computes the square root of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Sqrt(Vector512<float> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, MathF.Sqrt(value[i]));
    return result;
  }

  /// <summary>Computes the square root of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Sqrt(Vector512<double> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, Math.Sqrt(value[i]));
    return result;
  }

  #endregion

  #region Reciprocal Operations

  /// <summary>Computes approximate reciprocal of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Reciprocal14(Vector512<float> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, 1.0f / value[i]);
    return result;
  }

  /// <summary>Computes approximate reciprocal of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Reciprocal14(Vector512<double> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, 1.0 / value[i]);
    return result;
  }

  /// <summary>Computes approximate reciprocal square root of packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ReciprocalSqrt14(Vector512<float> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, 1.0f / MathF.Sqrt(value[i]));
    return result;
  }

  /// <summary>Computes approximate reciprocal square root of packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ReciprocalSqrt14(Vector512<double> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, 1.0 / Math.Sqrt(value[i]));
    return result;
  }

  #endregion

  #region Fused Multiply-Add Operations

  /// <summary>Computes fused multiply-add (a * b + c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplyAdd(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-add (a * b + c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplyAdd(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-subtract (a * b - c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplySubtract(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-subtract (a * b - c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplySubtract(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>Computes fused negated multiply-add (-(a * b) + c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplyAddNegated(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, c[i] - a[i] * b[i]);
    return result;
  }

  /// <summary>Computes fused negated multiply-add (-(a * b) + c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplyAddNegated(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, c[i] - a[i] * b[i]);
    return result;
  }

  /// <summary>Computes fused negated multiply-subtract (-(a * b) - c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplySubtractNegated(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) - c[i]);
    return result;
  }

  /// <summary>Computes fused negated multiply-subtract (-(a * b) - c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplySubtractNegated(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) - c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-add-subtract (odd: a * b + c, even: a * b - c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplyAddSubtract(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) != 0 ? a[i] * b[i] + c[i] : a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-add-subtract (odd: a * b + c, even: a * b - c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplyAddSubtract(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) != 0 ? a[i] * b[i] + c[i] : a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-subtract-add (odd: a * b - c, even: a * b + c) for packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> FusedMultiplySubtractAdd(Vector512<float> a, Vector512<float> b, Vector512<float> c) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) != 0 ? a[i] * b[i] - c[i] : a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>Computes fused multiply-subtract-add (odd: a * b - c, even: a * b + c) for packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> FusedMultiplySubtractAdd(Vector512<double> a, Vector512<double> b, Vector512<double> c) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) != 0 ? a[i] * b[i] - c[i] : a[i] * b[i] + c[i]);
    return result;
  }

  #endregion

  #region Shift Operations

  /// <summary>Shifts packed 32-bit signed integers left by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftLeftLogical(Vector512<int> value, byte count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers left by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> ShiftLeftLogical(Vector512<uint> value, byte count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 64-bit signed integers left by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftLeftLogical(Vector512<long> value, byte count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers left by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ShiftLeftLogical(Vector512<ulong> value, byte count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 32-bit signed integers right arithmetically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftRightArithmetic(Vector512<int> value, byte count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 64-bit signed integers right arithmetically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftRightArithmetic(Vector512<long> value, byte count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 32-bit signed integers right logically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftRightLogical(Vector512<int> value, byte count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, (int)((uint)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers right logically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> ShiftRightLogical(Vector512<uint> value, byte count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 64-bit signed integers right logically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftRightLogical(Vector512<long> value, byte count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, (long)((ulong)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers right logically by count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ShiftRightLogical(Vector512<ulong> value, byte count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 32-bit integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftLeftLogicalVariable(Vector512<int> value, Vector512<uint> count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, count[i] < 32 ? value[i] << (int)count[i] : 0);
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> ShiftLeftLogicalVariable(Vector512<uint> value, Vector512<uint> count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, count[i] < 32 ? value[i] << (int)count[i] : 0u);
    return result;
  }

  /// <summary>Shifts packed 64-bit integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftLeftLogicalVariable(Vector512<long> value, Vector512<ulong> count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, count[i] < 64 ? value[i] << (int)count[i] : 0L);
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ShiftLeftLogicalVariable(Vector512<ulong> value, Vector512<ulong> count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, count[i] < 64 ? value[i] << (int)count[i] : 0UL);
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right arithmetically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftRightArithmeticVariable(Vector512<int> value, Vector512<uint> count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      var shift = (int)Math.Min(count[i], 31);
      result = result.WithElement(i, value[i] >> shift);
    }
    return result;
  }

  /// <summary>Shifts packed 64-bit integers right arithmetically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftRightArithmeticVariable(Vector512<long> value, Vector512<ulong> count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      var shift = (int)Math.Min(count[i], 63);
      result = result.WithElement(i, value[i] >> shift);
    }
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right logically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ShiftRightLogicalVariable(Vector512<int> value, Vector512<uint> count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, count[i] < 32 ? (int)((uint)value[i] >> (int)count[i]) : 0);
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers right logically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> ShiftRightLogicalVariable(Vector512<uint> value, Vector512<uint> count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, count[i] < 32 ? value[i] >> (int)count[i] : 0u);
    return result;
  }

  /// <summary>Shifts packed 64-bit integers right logically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ShiftRightLogicalVariable(Vector512<long> value, Vector512<ulong> count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, count[i] < 64 ? (long)((ulong)value[i] >> (int)count[i]) : 0L);
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers right logically by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ShiftRightLogicalVariable(Vector512<ulong> value, Vector512<ulong> count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, count[i] < 64 ? value[i] >> (int)count[i] : 0UL);
    return result;
  }

  /// <summary>Rotates packed 32-bit integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> RotateLeftVariable(Vector512<int> value, Vector512<uint> count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      var rot = (int)(count[i] & 31);
      var v = (uint)value[i];
      result = result.WithElement(i, (int)((v << rot) | (v >> (32 - rot))));
    }
    return result;
  }

  /// <summary>Rotates packed 32-bit unsigned integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> RotateLeftVariable(Vector512<uint> value, Vector512<uint> count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i) {
      var rot = (int)(count[i] & 31);
      result = result.WithElement(i, (value[i] << rot) | (value[i] >> (32 - rot)));
    }
    return result;
  }

  /// <summary>Rotates packed 64-bit integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> RotateLeftVariable(Vector512<long> value, Vector512<ulong> count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      var rot = (int)(count[i] & 63);
      var v = (ulong)value[i];
      result = result.WithElement(i, (long)((v << rot) | (v >> (64 - rot))));
    }
    return result;
  }

  /// <summary>Rotates packed 64-bit unsigned integers left by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> RotateLeftVariable(Vector512<ulong> value, Vector512<ulong> count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i) {
      var rot = (int)(count[i] & 63);
      result = result.WithElement(i, (value[i] << rot) | (value[i] >> (64 - rot)));
    }
    return result;
  }

  /// <summary>Rotates packed 32-bit integers right by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> RotateRightVariable(Vector512<int> value, Vector512<uint> count) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      var rot = (int)(count[i] & 31);
      var v = (uint)value[i];
      result = result.WithElement(i, (int)((v >> rot) | (v << (32 - rot))));
    }
    return result;
  }

  /// <summary>Rotates packed 32-bit unsigned integers right by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> RotateRightVariable(Vector512<uint> value, Vector512<uint> count) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i) {
      var rot = (int)(count[i] & 31);
      result = result.WithElement(i, (value[i] >> rot) | (value[i] << (32 - rot)));
    }
    return result;
  }

  /// <summary>Rotates packed 64-bit integers right by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> RotateRightVariable(Vector512<long> value, Vector512<ulong> count) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      var rot = (int)(count[i] & 63);
      var v = (ulong)value[i];
      result = result.WithElement(i, (long)((v >> rot) | (v << (64 - rot))));
    }
    return result;
  }

  /// <summary>Rotates packed 64-bit unsigned integers right by variable amounts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> RotateRightVariable(Vector512<ulong> value, Vector512<ulong> count) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i) {
      var rot = (int)(count[i] & 63);
      result = result.WithElement(i, (value[i] >> rot) | (value[i] << (64 - rot)));
    }
    return result;
  }

  #endregion

  #region Permute Operations

  /// <summary>Permutes 32-bit elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> PermuteVar16x32(Vector512<int> value, Vector512<int> control) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, value[control[i] & 15]);
    return result;
  }

  /// <summary>Permutes 32-bit elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> PermuteVar16x32(Vector512<uint> value, Vector512<uint> control) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, value[(int)(control[i] & 15)]);
    return result;
  }

  /// <summary>Permutes single-precision floating-point elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> PermuteVar16x32(Vector512<float> value, Vector512<int> control) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i)
      result = result.WithElement(i, value[control[i] & 15]);
    return result;
  }

  /// <summary>Permutes 64-bit elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> PermuteVar8x64(Vector512<long> value, Vector512<long> control) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, value[(int)(control[i] & 7)]);
    return result;
  }

  /// <summary>Permutes 64-bit elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> PermuteVar8x64(Vector512<ulong> value, Vector512<ulong> control) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, value[(int)(control[i] & 7)]);
    return result;
  }

  /// <summary>Permutes double-precision floating-point elements using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> PermuteVar8x64(Vector512<double> value, Vector512<long> control) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, value[(int)(control[i] & 7)]);
    return result;
  }

  /// <summary>Permutes 32-bit elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> PermuteVar16x32x2(Vector512<int> lower, Vector512<int> indices, Vector512<int> upper) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < 16; ++i) {
      var idx = indices[i] & 31;
      result = result.WithElement(i, idx < 16 ? lower[idx] : upper[idx - 16]);
    }
    return result;
  }

  /// <summary>Permutes 32-bit elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> PermuteVar16x32x2(Vector512<uint> lower, Vector512<uint> indices, Vector512<uint> upper) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < 16; ++i) {
      var idx = (int)(indices[i] & 31);
      result = result.WithElement(i, idx < 16 ? lower[idx] : upper[idx - 16]);
    }
    return result;
  }

  /// <summary>Permutes single-precision floating-point elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> PermuteVar16x32x2(Vector512<float> lower, Vector512<int> indices, Vector512<float> upper) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < 16; ++i) {
      var idx = indices[i] & 31;
      result = result.WithElement(i, idx < 16 ? lower[idx] : upper[idx - 16]);
    }
    return result;
  }

  /// <summary>Permutes 64-bit elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> PermuteVar8x64x2(Vector512<long> lower, Vector512<long> indices, Vector512<long> upper) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < 8; ++i) {
      var idx = (int)(indices[i] & 15);
      result = result.WithElement(i, idx < 8 ? lower[idx] : upper[idx - 8]);
    }
    return result;
  }

  /// <summary>Permutes 64-bit elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> PermuteVar8x64x2(Vector512<ulong> lower, Vector512<ulong> indices, Vector512<ulong> upper) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < 8; ++i) {
      var idx = (int)(indices[i] & 15);
      result = result.WithElement(i, idx < 8 ? lower[idx] : upper[idx - 8]);
    }
    return result;
  }

  /// <summary>Permutes double-precision floating-point elements from two sources using indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> PermuteVar8x64x2(Vector512<double> lower, Vector512<long> indices, Vector512<double> upper) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < 8; ++i) {
      var idx = (int)(indices[i] & 15);
      result = result.WithElement(i, idx < 8 ? lower[idx] : upper[idx - 8]);
    }
    return result;
  }

  #endregion

  #region Shuffle Operations

  /// <summary>Shuffles packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Shuffle(Vector512<float> left, Vector512<float> right, byte control) {
    var result = Vector512<float>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 4;
      result = result.WithElement(laneOffset, left[laneOffset + (control & 3)]);
      result = result.WithElement(laneOffset + 1, left[laneOffset + ((control >> 2) & 3)]);
      result = result.WithElement(laneOffset + 2, right[laneOffset + ((control >> 4) & 3)]);
      result = result.WithElement(laneOffset + 3, right[laneOffset + ((control >> 6) & 3)]);
    }
    return result;
  }

  /// <summary>Shuffles packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Shuffle(Vector512<double> left, Vector512<double> right, byte control) {
    var result = Vector512<double>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 2;
      result = result.WithElement(laneOffset, (control & (1 << lane)) != 0 ? left[laneOffset + 1] : left[laneOffset]);
      result = result.WithElement(laneOffset + 1, (control & (1 << (lane + 4))) != 0 ? right[laneOffset + 1] : right[laneOffset]);
    }
    return result;
  }

  /// <summary>Shuffles packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Shuffle(Vector512<int> value, byte control) {
    var result = Vector512<int>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 4;
      result = result.WithElement(laneOffset, value[laneOffset + (control & 3)]);
      result = result.WithElement(laneOffset + 1, value[laneOffset + ((control >> 2) & 3)]);
      result = result.WithElement(laneOffset + 2, value[laneOffset + ((control >> 4) & 3)]);
      result = result.WithElement(laneOffset + 3, value[laneOffset + ((control >> 6) & 3)]);
    }
    return result;
  }

  /// <summary>Shuffles packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Shuffle(Vector512<uint> value, byte control) {
    var result = Vector512<uint>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 4;
      result = result.WithElement(laneOffset, value[laneOffset + (control & 3)]);
      result = result.WithElement(laneOffset + 1, value[laneOffset + ((control >> 2) & 3)]);
      result = result.WithElement(laneOffset + 2, value[laneOffset + ((control >> 4) & 3)]);
      result = result.WithElement(laneOffset + 3, value[laneOffset + ((control >> 6) & 3)]);
    }
    return result;
  }

  #endregion

  #region Unpack Operations

  /// <summary>Unpacks and interleaves low 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> UnpackLow(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 4;
      var dstOffset = lane * 4;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
      result = result.WithElement(dstOffset + 2, left[srcOffset + 1]);
      result = result.WithElement(dstOffset + 3, right[srcOffset + 1]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> UnpackHigh(Vector512<int> left, Vector512<int> right) {
    var result = Vector512<int>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 4 + 2;
      var dstOffset = lane * 4;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
      result = result.WithElement(dstOffset + 2, left[srcOffset + 1]);
      result = result.WithElement(dstOffset + 3, right[srcOffset + 1]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> UnpackLow(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 2;
      var dstOffset = lane * 2;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> UnpackHigh(Vector512<long> left, Vector512<long> right) {
    var result = Vector512<long>.Zero;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 2 + 1;
      var dstOffset = lane * 2;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> UnpackLow(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 4;
      var dstOffset = lane * 4;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
      result = result.WithElement(dstOffset + 2, left[srcOffset + 1]);
      result = result.WithElement(dstOffset + 3, right[srcOffset + 1]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> UnpackHigh(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 4 + 2;
      var dstOffset = lane * 4;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
      result = result.WithElement(dstOffset + 2, left[srcOffset + 1]);
      result = result.WithElement(dstOffset + 3, right[srcOffset + 1]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> UnpackLow(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 2;
      var dstOffset = lane * 2;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> UnpackHigh(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var lane = 0; lane < 4; ++lane) {
      var srcOffset = lane * 2 + 1;
      var dstOffset = lane * 2;
      result = result.WithElement(dstOffset, left[srcOffset]);
      result = result.WithElement(dstOffset + 1, right[srcOffset]);
    }
    return result;
  }

  #endregion

  #region Ternary Logic Operations

  /// <summary>Computes bitwise ternary logic operation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> TernaryLogic(Vector512<int> a, Vector512<int> b, Vector512<int> c, byte control) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      var aVal = a[i];
      var bVal = b[i];
      var cVal = c[i];
      var resultVal = 0;
      for (var bit = 0; bit < 32; ++bit) {
        var aBit = (aVal >> bit) & 1;
        var bBit = (bVal >> bit) & 1;
        var cBit = (cVal >> bit) & 1;
        var tableIndex = (aBit << 2) | (bBit << 1) | cBit;
        var resultBit = (control >> tableIndex) & 1;
        resultVal |= resultBit << bit;
      }
      result = result.WithElement(i, resultVal);
    }
    return result;
  }

  /// <summary>Computes bitwise ternary logic operation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> TernaryLogic(Vector512<uint> a, Vector512<uint> b, Vector512<uint> c, byte control) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i) {
      var aVal = a[i];
      var bVal = b[i];
      var cVal = c[i];
      var resultVal = 0u;
      for (var bit = 0; bit < 32; ++bit) {
        var aBit = (int)((aVal >> bit) & 1);
        var bBit = (int)((bVal >> bit) & 1);
        var cBit = (int)((cVal >> bit) & 1);
        var tableIndex = (aBit << 2) | (bBit << 1) | cBit;
        var resultBit = (uint)((control >> tableIndex) & 1);
        resultVal |= resultBit << bit;
      }
      result = result.WithElement(i, resultVal);
    }
    return result;
  }

  /// <summary>Computes bitwise ternary logic operation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> TernaryLogic(Vector512<long> a, Vector512<long> b, Vector512<long> c, byte control) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      var aVal = a[i];
      var bVal = b[i];
      var cVal = c[i];
      var resultVal = 0L;
      for (var bit = 0; bit < 64; ++bit) {
        var aBit = (int)((aVal >> bit) & 1);
        var bBit = (int)((bVal >> bit) & 1);
        var cBit = (int)((cVal >> bit) & 1);
        var tableIndex = (aBit << 2) | (bBit << 1) | cBit;
        var resultBit = (long)((control >> tableIndex) & 1);
        resultVal |= resultBit << bit;
      }
      result = result.WithElement(i, resultVal);
    }
    return result;
  }

  /// <summary>Computes bitwise ternary logic operation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> TernaryLogic(Vector512<ulong> a, Vector512<ulong> b, Vector512<ulong> c, byte control) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i) {
      var aVal = a[i];
      var bVal = b[i];
      var cVal = c[i];
      var resultVal = 0UL;
      for (var bit = 0; bit < 64; ++bit) {
        var aBit = (int)((aVal >> bit) & 1);
        var bBit = (int)((bVal >> bit) & 1);
        var cBit = (int)((cVal >> bit) & 1);
        var tableIndex = (aBit << 2) | (bBit << 1) | cBit;
        var resultBit = (ulong)((control >> tableIndex) & 1);
        resultVal |= resultBit << bit;
      }
      result = result.WithElement(i, resultVal);
    }
    return result;
  }

  /// <summary>Computes bitwise ternary logic operation for single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> TernaryLogic(Vector512<float> a, Vector512<float> b, Vector512<float> c, byte control) {
    var ai = Vector512.As<float, int>(a);
    var bi = Vector512.As<float, int>(b);
    var ci = Vector512.As<float, int>(c);
    var ri = TernaryLogic(ai, bi, ci, control);
    return Vector512.As<int, float>(ri);
  }

  /// <summary>Computes bitwise ternary logic operation for double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> TernaryLogic(Vector512<double> a, Vector512<double> b, Vector512<double> c, byte control) {
    var ai = Vector512.As<double, long>(a);
    var bi = Vector512.As<double, long>(b);
    var ci = Vector512.As<double, long>(c);
    var ri = TernaryLogic(ai, bi, ci, control);
    return Vector512.As<long, double>(ri);
  }

  #endregion

  #region Broadcast Operations

  /// <summary>Broadcasts a 32-bit value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> BroadcastScalarToVector512(int value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a 32-bit value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> BroadcastScalarToVector512(uint value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a 64-bit value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> BroadcastScalarToVector512(long value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a 64-bit value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> BroadcastScalarToVector512(ulong value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a single-precision floating-point value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> BroadcastScalarToVector512(float value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a double-precision floating-point value to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> BroadcastScalarToVector512(double value)
    => Vector512.Create(value);

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> BroadcastVector128ToVector512(Vector128<int> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> BroadcastVector128ToVector512(Vector128<uint> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> BroadcastVector128ToVector512(Vector128<long> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> BroadcastVector128ToVector512(Vector128<ulong> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> BroadcastVector128ToVector512(Vector128<float> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector128 to all 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> BroadcastVector128ToVector512(Vector128<double> value) {
    var v256 = Vector256.Create(value, value);
    return Vector512.Create(v256, v256);
  }

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> BroadcastVector256ToVector512(Vector256<int> value)
    => Vector512.Create(value, value);

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> BroadcastVector256ToVector512(Vector256<uint> value)
    => Vector512.Create(value, value);

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> BroadcastVector256ToVector512(Vector256<long> value)
    => Vector512.Create(value, value);

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> BroadcastVector256ToVector512(Vector256<ulong> value)
    => Vector512.Create(value, value);

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> BroadcastVector256ToVector512(Vector256<float> value)
    => Vector512.Create(value, value);

  /// <summary>Broadcasts a Vector256 to both 256-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> BroadcastVector256ToVector512(Vector256<double> value)
    => Vector512.Create(value, value);

  #endregion

  #region Extract/Insert Operations

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ExtractVector256(Vector512<int> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> ExtractVector256(Vector512<uint> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ExtractVector256(Vector512<long> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> ExtractVector256(Vector512<ulong> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> ExtractVector256(Vector512<float> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 256-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> ExtractVector256(Vector512<double> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ExtractVector128(Vector512<int> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ExtractVector128(Vector512<uint> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ExtractVector128(Vector512<long> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ExtractVector128(Vector512<ulong> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> ExtractVector128(Vector512<float> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Extracts a 128-bit vector from the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> ExtractVector128(Vector512<double> value, byte index) {
    var v256 = (index & 2) == 0 ? value.GetLower() : value.GetUpper();
    return (index & 1) == 0 ? v256.GetLower() : v256.GetUpper();
  }

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> InsertVector256(Vector512<int> value, Vector256<int> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> InsertVector256(Vector512<uint> value, Vector256<uint> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> InsertVector256(Vector512<long> value, Vector256<long> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> InsertVector256(Vector512<ulong> value, Vector256<ulong> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> InsertVector256(Vector512<float> value, Vector256<float> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 256-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> InsertVector256(Vector512<double> value, Vector256<double> data, byte index)
    => (index & 1) == 0 ? Vector512.Create(data, value.GetUpper()) : Vector512.Create(value.GetLower(), data);

  /// <summary>Inserts a 128-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> InsertVector128(Vector512<int> value, Vector128<int> data, byte index) {
    var lower = value.GetLower();
    var upper = value.GetUpper();
    switch (index & 3) {
      case 0:
        lower = Vector256.Create(data, lower.GetUpper());
        break;
      case 1:
        lower = Vector256.Create(lower.GetLower(), data);
        break;
      case 2:
        upper = Vector256.Create(data, upper.GetUpper());
        break;
      default:
        upper = Vector256.Create(upper.GetLower(), data);
        break;
    }
    return Vector512.Create(lower, upper);
  }

  /// <summary>Inserts a 128-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> InsertVector128(Vector512<uint> value, Vector128<uint> data, byte index) {
    var lower = value.GetLower();
    var upper = value.GetUpper();
    switch (index & 3) {
      case 0:
        lower = Vector256.Create(data, lower.GetUpper());
        break;
      case 1:
        lower = Vector256.Create(lower.GetLower(), data);
        break;
      case 2:
        upper = Vector256.Create(data, upper.GetUpper());
        break;
      default:
        upper = Vector256.Create(upper.GetLower(), data);
        break;
    }
    return Vector512.Create(lower, upper);
  }

  /// <summary>Inserts a 128-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> InsertVector128(Vector512<float> value, Vector128<float> data, byte index) {
    var lower = value.GetLower();
    var upper = value.GetUpper();
    switch (index & 3) {
      case 0:
        lower = Vector256.Create(data, lower.GetUpper());
        break;
      case 1:
        lower = Vector256.Create(lower.GetLower(), data);
        break;
      case 2:
        upper = Vector256.Create(data, upper.GetUpper());
        break;
      default:
        upper = Vector256.Create(upper.GetLower(), data);
        break;
    }
    return Vector512.Create(lower, upper);
  }

  /// <summary>Inserts a 128-bit vector into the specified lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> InsertVector128(Vector512<double> value, Vector128<double> data, byte index) {
    var lower = value.GetLower();
    var upper = value.GetUpper();
    switch (index & 3) {
      case 0:
        lower = Vector256.Create(data, lower.GetUpper());
        break;
      case 1:
        lower = Vector256.Create(lower.GetLower(), data);
        break;
      case 2:
        upper = Vector256.Create(data, upper.GetUpper());
        break;
      default:
        upper = Vector256.Create(upper.GetLower(), data);
        break;
    }
    return Vector512.Create(lower, upper);
  }

  #endregion

  #region Conversion Operations

  /// <summary>Converts packed 32-bit integers to packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToVector512Single(Vector512<int> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 32-bit unsigned integers to packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> ConvertToVector512Single(Vector512<uint> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 64-bit integers to packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToVector512Double(Vector512<long> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 64-bit unsigned integers to packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToVector512Double(Vector512<ulong> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed single-precision floating-point values to packed 32-bit integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> ConvertToVector512Int32WithTruncation(Vector512<float> value) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, (int)value[i]);
    return result;
  }

  /// <summary>Converts packed double-precision floating-point values to packed 32-bit integers with truncation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32WithTruncation(Vector512<double> value) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, (int)value[i]);
    return result;
  }

  /// <summary>Converts packed 32-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> ConvertToVector512Int64(Vector256<int> value) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, Vector256.GetElement(value, i));
    return result;
  }

  /// <summary>Converts packed 32-bit unsigned integers to packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> ConvertToVector512UInt64(Vector256<uint> value) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, Vector256.GetElement(value, i));
    return result;
  }

  /// <summary>Converts packed single-precision floating-point values to packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> ConvertToVector512Double(Vector256<float> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, Vector256.GetElement(value, i));
    return result;
  }

  /// <summary>Converts packed double-precision floating-point values to packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> ConvertToVector256Single(Vector512<double> value) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, (float)value[i]);
    return result;
  }

  #endregion

  #region Load/Store Operations

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<byte> LoadVector512(byte* address) {
    var result = Vector512<byte>.Zero;
    for (var i = 0; i < Vector512<byte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<sbyte> LoadVector512(sbyte* address) {
    var result = Vector512<sbyte>.Zero;
    for (var i = 0; i < Vector512<sbyte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<short> LoadVector512(short* address) {
    var result = Vector512<short>.Zero;
    for (var i = 0; i < Vector512<short>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<ushort> LoadVector512(ushort* address) {
    var result = Vector512<ushort>.Zero;
    for (var i = 0; i < Vector512<ushort>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<int> LoadVector512(int* address) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<uint> LoadVector512(uint* address) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<long> LoadVector512(long* address) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<ulong> LoadVector512(ulong* address) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<float> LoadVector512(float* address) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<double> LoadVector512(double* address) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<int> LoadAlignedVector512(int* address)
    => LoadVector512(address);

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<uint> LoadAlignedVector512(uint* address)
    => LoadVector512(address);

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<long> LoadAlignedVector512(long* address)
    => LoadVector512(address);

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<ulong> LoadAlignedVector512(ulong* address)
    => LoadVector512(address);

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<float> LoadAlignedVector512(float* address)
    => LoadVector512(address);

  /// <summary>Loads 512 bits from aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector512<double> LoadAlignedVector512(double* address)
    => LoadVector512(address);

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(byte* address, Vector512<byte> source) {
    for (var i = 0; i < Vector512<byte>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(sbyte* address, Vector512<sbyte> source) {
    for (var i = 0; i < Vector512<sbyte>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(short* address, Vector512<short> source) {
    for (var i = 0; i < Vector512<short>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ushort* address, Vector512<ushort> source) {
    for (var i = 0; i < Vector512<ushort>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(int* address, Vector512<int> source) {
    for (var i = 0; i < Vector512<int>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(uint* address, Vector512<uint> source) {
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(long* address, Vector512<long> source) {
    for (var i = 0; i < Vector512<long>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(ulong* address, Vector512<ulong> source) {
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(float* address, Vector512<float> source) {
    for (var i = 0; i < Vector512<float>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(double* address, Vector512<double> source) {
    for (var i = 0; i < Vector512<double>.Count; ++i)
      address[i] = source[i];
  }

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(int* address, Vector512<int> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(uint* address, Vector512<uint> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(long* address, Vector512<long> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(ulong* address, Vector512<ulong> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(float* address, Vector512<float> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAligned(double* address, Vector512<double> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(int* address, Vector512<int> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(uint* address, Vector512<uint> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(long* address, Vector512<long> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(ulong* address, Vector512<ulong> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(float* address, Vector512<float> source)
    => Store(address, source);

  /// <summary>Stores 512 bits to aligned memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void StoreAlignedNonTemporal(double* address, Vector512<double> source)
    => Store(address, source);

  #endregion

  #region Blend Operations

  /// <summary>Blends packed 32-bit integers using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> BlendVariable(Vector512<int> left, Vector512<int> right, Vector512<int> mask) {
    var result = Vector512<int>.Zero;
    for (var i = 0; i < Vector512<int>.Count; ++i)
      result = result.WithElement(i, mask[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 32-bit unsigned integers using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> BlendVariable(Vector512<uint> left, Vector512<uint> right, Vector512<uint> mask) {
    var result = Vector512<uint>.Zero;
    for (var i = 0; i < Vector512<uint>.Count; ++i)
      result = result.WithElement(i, (mask[i] & 0x80000000) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 64-bit integers using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> BlendVariable(Vector512<long> left, Vector512<long> right, Vector512<long> mask) {
    var result = Vector512<long>.Zero;
    for (var i = 0; i < Vector512<long>.Count; ++i)
      result = result.WithElement(i, mask[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 64-bit unsigned integers using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> BlendVariable(Vector512<ulong> left, Vector512<ulong> right, Vector512<ulong> mask) {
    var result = Vector512<ulong>.Zero;
    for (var i = 0; i < Vector512<ulong>.Count; ++i)
      result = result.WithElement(i, (mask[i] & 0x8000000000000000) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed single-precision floating-point values using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> BlendVariable(Vector512<float> left, Vector512<float> right, Vector512<float> mask) {
    var maskInt = Vector512.As<float, int>(mask);
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, maskInt[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed double-precision floating-point values using a mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> BlendVariable(Vector512<double> left, Vector512<double> right, Vector512<double> mask) {
    var maskInt = Vector512.As<double, long>(mask);
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, maskInt[i] < 0 ? right[i] : left[i]);
    return result;
  }

  #endregion

  #region Rounding Operations

  /// <summary>Rounds packed single-precision floating-point values to nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> RoundScale(Vector512<float> value, byte control) {
    var result = Vector512<float>.Zero;
    var roundingMode = control & 3;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      var v = value[i];
      var rounded = roundingMode switch {
        0 => MathF.Round(v, MidpointRounding.ToEven),
        1 => MathF.Floor(v),
        2 => MathF.Ceiling(v),
        3 => MathF.Truncate(v),
        _ => v
      };
      result = result.WithElement(i, rounded);
    }
    return result;
  }

  /// <summary>Rounds packed double-precision floating-point values to nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> RoundScale(Vector512<double> value, byte control) {
    var result = Vector512<double>.Zero;
    var roundingMode = control & 3;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      var v = value[i];
      var rounded = roundingMode switch {
        0 => Math.Round(v, MidpointRounding.ToEven),
        1 => Math.Floor(v),
        2 => Math.Ceiling(v),
        3 => Math.Truncate(v),
        _ => v
      };
      result = result.WithElement(i, rounded);
    }
    return result;
  }

  #endregion

  #region Fixup Operations

  /// <summary>Fixes up elements of a float vector using a control table.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Fixup(Vector512<float> left, Vector512<float> right, Vector512<int> table, byte control) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      var leftVal = left[i];
      var rightVal = right[i];
      var tableVal = table[i];

      int token;
      if (float.IsNaN(rightVal))
        token = (tableVal >> 0) & 0xF;
      else if (float.IsNegativeInfinity(rightVal))
        token = (tableVal >> 4) & 0xF;
      else if (rightVal == -0.0f || (rightVal < 0.0f && rightVal > float.NegativeInfinity))
        token = (tableVal >> 8) & 0xF;
      else if (rightVal == 0.0f)
        token = (tableVal >> 12) & 0xF;
      else if (rightVal > 0.0f && rightVal < float.PositiveInfinity)
        token = (tableVal >> 16) & 0xF;
      else if (float.IsPositiveInfinity(rightVal))
        token = (tableVal >> 20) & 0xF;
      else if (float.IsNaN(leftVal))
        token = (tableVal >> 24) & 0xF;
      else
        token = (tableVal >> 28) & 0xF;

      var fixedVal = token switch {
        0 => leftVal,
        1 => rightVal,
        2 => float.NaN,
        3 => float.NegativeInfinity,
        4 => float.PositiveInfinity,
        5 => BitConverter.Int32BitsToSingle(unchecked((int)0x7F800001)),
        6 => -0.0f,
        7 => 0.0f,
        8 => 1.0f,
        9 => -1.0f,
        10 => 0.5f,
        11 => 90.0f,
        12 => (float)Math.PI / 2,
        13 => float.MaxValue,
        14 => float.MinValue,
        15 => float.Epsilon,
        _ => leftVal
      };
      result = result.WithElement(i, fixedVal);
    }
    return result;
  }

  /// <summary>Fixes up elements of a double vector using a control table.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Fixup(Vector512<double> left, Vector512<double> right, Vector512<long> table, byte control) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      var leftVal = left[i];
      var rightVal = right[i];
      var tableVal = table[i];

      int token;
      if (double.IsNaN(rightVal))
        token = (int)((tableVal >> 0) & 0xF);
      else if (double.IsNegativeInfinity(rightVal))
        token = (int)((tableVal >> 4) & 0xF);
      else if (rightVal == -0.0 || (rightVal < 0.0 && rightVal > double.NegativeInfinity))
        token = (int)((tableVal >> 8) & 0xF);
      else if (rightVal == 0.0)
        token = (int)((tableVal >> 12) & 0xF);
      else if (rightVal > 0.0 && rightVal < double.PositiveInfinity)
        token = (int)((tableVal >> 16) & 0xF);
      else if (double.IsPositiveInfinity(rightVal))
        token = (int)((tableVal >> 20) & 0xF);
      else if (double.IsNaN(leftVal))
        token = (int)((tableVal >> 24) & 0xF);
      else
        token = (int)((tableVal >> 28) & 0xF);

      var fixedVal = token switch {
        0 => leftVal,
        1 => rightVal,
        2 => double.NaN,
        3 => double.NegativeInfinity,
        4 => double.PositiveInfinity,
        5 => BitConverter.Int64BitsToDouble(unchecked((long)0x7FF0000000000001)),
        6 => -0.0,
        7 => 0.0,
        8 => 1.0,
        9 => -1.0,
        10 => 0.5,
        11 => 90.0,
        12 => Math.PI / 2,
        13 => double.MaxValue,
        14 => double.MinValue,
        15 => double.Epsilon,
        _ => leftVal
      };
      result = result.WithElement(i, fixedVal);
    }
    return result;
  }

  #endregion

  #region Compress/Expand Operations

  /// <summary>Compresses active elements of a 32-bit integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Compress(Vector512<int> value, Vector512<int> mask) {
    var result = Vector512<int>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      if (mask[i] < 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Compresses active elements of a 32-bit unsigned integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Compress(Vector512<uint> value, Vector512<uint> mask) {
    var result = Vector512<uint>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<uint>.Count; ++i) {
      if ((mask[i] & 0x80000000) != 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Compresses active elements of a 64-bit integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Compress(Vector512<long> value, Vector512<long> mask) {
    var result = Vector512<long>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      if (mask[i] < 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Compresses active elements of a 64-bit unsigned integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Compress(Vector512<ulong> value, Vector512<ulong> mask) {
    var result = Vector512<ulong>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<ulong>.Count; ++i) {
      if ((mask[i] & 0x8000000000000000) != 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Compresses active elements of a single-precision floating-point vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Compress(Vector512<float> value, Vector512<float> mask) {
    var maskInt = Vector512.As<float, int>(mask);
    var result = Vector512<float>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      if (maskInt[i] < 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Compresses active elements of a double-precision floating-point vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Compress(Vector512<double> value, Vector512<double> mask) {
    var maskLong = Vector512.As<double, long>(mask);
    var result = Vector512<double>.Zero;
    var destIndex = 0;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      if (maskLong[i] < 0)
        result = result.WithElement(destIndex++, value[i]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<int> Expand(Vector512<int> value, Vector512<int> mask) {
    var result = Vector512<int>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<int>.Count; ++i) {
      if (mask[i] < 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<uint> Expand(Vector512<uint> value, Vector512<uint> mask) {
    var result = Vector512<uint>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<uint>.Count; ++i) {
      if ((mask[i] & 0x80000000) != 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<long> Expand(Vector512<long> value, Vector512<long> mask) {
    var result = Vector512<long>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<long>.Count; ++i) {
      if (mask[i] < 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<ulong> Expand(Vector512<ulong> value, Vector512<ulong> mask) {
    var result = Vector512<ulong>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<ulong>.Count; ++i) {
      if ((mask[i] & 0x8000000000000000) != 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Expand(Vector512<float> value, Vector512<float> mask) {
    var maskInt = Vector512.As<float, int>(mask);
    var result = Vector512<float>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      if (maskInt[i] < 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  /// <summary>Expands elements from source into destination based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Expand(Vector512<double> value, Vector512<double> mask) {
    var maskLong = Vector512.As<double, long>(mask);
    var result = Vector512<double>.Zero;
    var srcIndex = 0;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      if (maskLong[i] < 0)
        result = result.WithElement(i, value[srcIndex++]);
    }
    return result;
  }

  #endregion

  #region GetExponent/GetMantissa Operations

  /// <summary>Extracts exponent from packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> GetExponent(Vector512<float> value) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      var v = value[i];
      if (float.IsNaN(v) || float.IsInfinity(v) || v == 0.0f)
        result = result.WithElement(i, v);
      else {
        var bits = BitConverter.SingleToInt32Bits(v);
        var exp = ((bits >> 23) & 0xFF) - 127;
        result = result.WithElement(i, exp);
      }
    }
    return result;
  }

  /// <summary>Extracts exponent from packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> GetExponent(Vector512<double> value) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      var v = value[i];
      if (double.IsNaN(v) || double.IsInfinity(v) || v == 0.0)
        result = result.WithElement(i, v);
      else {
        var bits = BitConverter.DoubleToInt64Bits(v);
        var exp = ((int)((bits >> 52) & 0x7FF)) - 1023;
        result = result.WithElement(i, (double)exp);
      }
    }
    return result;
  }

  /// <summary>Extracts mantissa from packed single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> GetMantissa(Vector512<float> value, byte control) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i) {
      var v = value[i];
      if (float.IsNaN(v) || float.IsInfinity(v))
        result = result.WithElement(i, v);
      else if (v == 0.0f)
        result = result.WithElement(i, v);
      else {
        var bits = BitConverter.SingleToInt32Bits(v);
        var mantissa = bits & 0x7FFFFF;
        var sign = bits & unchecked((int)0x80000000);
        var normalizedMantissa = BitConverter.Int32BitsToSingle(sign | 0x3F800000 | mantissa);
        result = result.WithElement(i, normalizedMantissa);
      }
    }
    return result;
  }

  /// <summary>Extracts mantissa from packed double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> GetMantissa(Vector512<double> value, byte control) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i) {
      var v = value[i];
      if (double.IsNaN(v) || double.IsInfinity(v))
        result = result.WithElement(i, v);
      else if (v == 0.0)
        result = result.WithElement(i, v);
      else {
        var bits = BitConverter.DoubleToInt64Bits(v);
        var mantissa = bits & 0xFFFFFFFFFFFFF;
        var sign = bits & unchecked((long)0x8000000000000000);
        var normalizedMantissa = BitConverter.Int64BitsToDouble(sign | 0x3FF0000000000000 | mantissa);
        result = result.WithElement(i, normalizedMantissa);
      }
    }
    return result;
  }

  #endregion

  #region Scale Operations

  /// <summary>Scales packed single-precision floating-point values by powers of 2.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<float> Scale(Vector512<float> left, Vector512<float> right) {
    var result = Vector512<float>.Zero;
    for (var i = 0; i < Vector512<float>.Count; ++i)
      result = result.WithElement(i, left[i] * MathF.Pow(2, MathF.Truncate(right[i])));
    return result;
  }

  /// <summary>Scales packed double-precision floating-point values by powers of 2.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<double> Scale(Vector512<double> left, Vector512<double> right) {
    var result = Vector512<double>.Zero;
    for (var i = 0; i < Vector512<double>.Count; ++i)
      result = result.WithElement(i, left[i] * Math.Pow(2, Math.Truncate(right[i])));
    return result;
  }

  #endregion

  #region Align Right Operation

  /// <summary>Concatenates and right shifts pairs of 128-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<sbyte> AlignRight(Vector512<sbyte> left, Vector512<sbyte> right, byte mask) {
    var result = Vector512<sbyte>.Zero;
    var shift = mask & 0x1F;
    // Process each 128-bit lane
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 16;
      for (var i = 0; i < 16; ++i) {
        var srcIndex = i + shift;
        sbyte val;
        if (srcIndex < 16)
          val = right[laneOffset + srcIndex];
        else if (srcIndex < 32)
          val = left[laneOffset + srcIndex - 16];
        else
          val = 0;
        result = result.WithElement(laneOffset + i, val);
      }
    }
    return result;
  }

  /// <summary>Concatenates and right shifts pairs of 128-bit lanes.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector512<byte> AlignRight(Vector512<byte> left, Vector512<byte> right, byte mask) {
    var result = Vector512<byte>.Zero;
    var shift = mask & 0x1F;
    for (var lane = 0; lane < 4; ++lane) {
      var laneOffset = lane * 16;
      for (var i = 0; i < 16; ++i) {
        var srcIndex = i + shift;
        byte val;
        if (srcIndex < 16)
          val = right[laneOffset + srcIndex];
        else if (srcIndex < 32)
          val = left[laneOffset + srcIndex - 16];
        else
          val = 0;
        result = result.WithElement(laneOffset + i, val);
      }
    }
    return result;
  }

  #endregion

  #region MoveMask Operations

  /// <summary>Creates a 16-bit mask from the most significant bit of each 32-bit element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector512<float> value) {
    var maskInt = Vector512.As<float, int>(value);
    var mask = 0;
    for (var i = 0; i < 16; ++i) {
      if (maskInt[i] < 0)
        mask |= 1 << i;
    }
    return mask;
  }

  /// <summary>Creates an 8-bit mask from the most significant bit of each 64-bit element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector512<double> value) {
    var maskLong = Vector512.As<double, long>(value);
    var mask = 0;
    for (var i = 0; i < 8; ++i) {
      if (maskLong[i] < 0)
        mask |= 1 << i;
    }
    return mask;
  }

  #endregion

  #region X64 Nested Class

  /// <summary>Provides 64-bit specific AVX-512F operations.</summary>
#if SUPPORTS_INTRINSICS
  public static class X64 {
#else
  public new abstract class X64 : Avx2.X64 {
#endif
    /// <summary>Gets a value indicating whether 64-bit AVX-512F instructions are supported.</summary>
    public
#if !SUPPORTS_INTRINSICS
    new
#endif
    static bool IsSupported => false;

    /// <summary>Converts scalar 64-bit integer to double.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> ConvertScalarToVector512Double(Vector512<double> upper, long value) {
      var result = upper;
      result = result.WithElement(0, (double)value);
      return result;
    }

    /// <summary>Converts scalar 64-bit unsigned integer to double.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector512<double> ConvertScalarToVector512Double(Vector512<double> upper, ulong value) {
      var result = upper;
      result = result.WithElement(0, (double)value);
      return result;
    }

    /// <summary>Converts scalar double to 64-bit integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64(Vector512<double> value)
      => (long)Math.Round(value[0]);

    /// <summary>Converts scalar double to 64-bit unsigned integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertToUInt64(Vector512<double> value)
      => (ulong)Math.Round(value[0]);

    /// <summary>Converts scalar double to 64-bit integer with truncation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64WithTruncation(Vector512<double> value)
      => (long)value[0];

    /// <summary>Converts scalar double to 64-bit unsigned integer with truncation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertToUInt64WithTruncation(Vector512<double> value)
      => (ulong)value[0];

    /// <summary>Converts scalar float to 64-bit integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64(Vector512<float> value)
      => (long)Math.Round(value[0]);

    /// <summary>Converts scalar float to 64-bit unsigned integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertToUInt64(Vector512<float> value)
      => (ulong)Math.Round(value[0]);

    /// <summary>Converts scalar float to 64-bit integer with truncation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertToInt64WithTruncation(Vector512<float> value)
      => (long)value[0];

    /// <summary>Converts scalar float to 64-bit unsigned integer with truncation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertToUInt64WithTruncation(Vector512<float> value)
      => (ulong)value[0];
  }

  #endregion
}

#endif
