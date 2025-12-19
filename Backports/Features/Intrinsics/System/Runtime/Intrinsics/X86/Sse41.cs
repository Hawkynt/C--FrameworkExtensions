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
/// Software fallback implementation of SSE4.1 intrinsics.
/// </summary>
public abstract class Sse41 : Ssse3 {

  /// <summary>Gets a value indicating whether SSE4.1 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Blend Operations

  /// <summary>Blends packed single-precision floating-point values based on control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Blend(Vector128<float> left, Vector128<float> right, byte control)
    => Vector128.Create(
      ((control >> 0) & 1) != 0 ? right[0] : left[0],
      ((control >> 1) & 1) != 0 ? right[1] : left[1],
      ((control >> 2) & 1) != 0 ? right[2] : left[2],
      ((control >> 3) & 1) != 0 ? right[3] : left[3]
    );

  /// <summary>Blends packed double-precision floating-point values based on control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Blend(Vector128<double> left, Vector128<double> right, byte control)
    => Vector128.Create(
      ((control >> 0) & 1) != 0 ? right[0] : left[0],
      ((control >> 1) & 1) != 0 ? right[1] : left[1]
    );

  /// <summary>Blends packed 16-bit integers based on control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Blend(Vector128<short> left, Vector128<short> right, byte control) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 16-bit unsigned integers based on control byte.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Blend(Vector128<ushort> left, Vector128<ushort> right, byte control) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed single-precision floating-point values using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> BlendVariable(Vector128<float> left, Vector128<float> right, Vector128<float> mask) {
    var maskInt = Vector128.As<float, int>(mask);
    return Vector128.Create(
      maskInt[0] < 0 ? right[0] : left[0],
      maskInt[1] < 0 ? right[1] : left[1],
      maskInt[2] < 0 ? right[2] : left[2],
      maskInt[3] < 0 ? right[3] : left[3]
    );
  }

  /// <summary>Blends packed double-precision floating-point values using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> BlendVariable(Vector128<double> left, Vector128<double> right, Vector128<double> mask) {
    var maskLong = Vector128.As<double, long>(mask);
    return Vector128.Create(
      maskLong[0] < 0 ? right[0] : left[0],
      maskLong[1] < 0 ? right[1] : left[1]
    );
  }

  /// <summary>Blends packed 8-bit signed integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> BlendVariable(Vector128<sbyte> left, Vector128<sbyte> right, Vector128<sbyte> mask) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, mask[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 8-bit unsigned integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> BlendVariable(Vector128<byte> left, Vector128<byte> right, Vector128<byte> mask) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, (mask[i] & 0x80) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 16-bit signed integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> BlendVariable(Vector128<short> left, Vector128<short> right, Vector128<short> mask) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, mask[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 16-bit unsigned integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> BlendVariable(Vector128<ushort> left, Vector128<ushort> right, Vector128<ushort> mask) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, (mask[i] & 0x8000) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 32-bit signed integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> BlendVariable(Vector128<int> left, Vector128<int> right, Vector128<int> mask)
    => Vector128.Create(
      mask[0] < 0 ? right[0] : left[0],
      mask[1] < 0 ? right[1] : left[1],
      mask[2] < 0 ? right[2] : left[2],
      mask[3] < 0 ? right[3] : left[3]
    );

  /// <summary>Blends packed 32-bit unsigned integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> BlendVariable(Vector128<uint> left, Vector128<uint> right, Vector128<uint> mask)
    => Vector128.Create(
      (mask[0] & 0x80000000) != 0 ? right[0] : left[0],
      (mask[1] & 0x80000000) != 0 ? right[1] : left[1],
      (mask[2] & 0x80000000) != 0 ? right[2] : left[2],
      (mask[3] & 0x80000000) != 0 ? right[3] : left[3]
    );

  /// <summary>Blends packed 64-bit signed integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> BlendVariable(Vector128<long> left, Vector128<long> right, Vector128<long> mask)
    => Vector128.Create(
      mask[0] < 0 ? right[0] : left[0],
      mask[1] < 0 ? right[1] : left[1]
    );

  /// <summary>Blends packed 64-bit unsigned integers using mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> BlendVariable(Vector128<ulong> left, Vector128<ulong> right, Vector128<ulong> mask)
    => Vector128.Create(
      (mask[0] & 0x8000000000000000UL) != 0 ? right[0] : left[0],
      (mask[1] & 0x8000000000000000UL) != 0 ? right[1] : left[1]
    );

  #endregion

  #region Rounding Operations

  /// <summary>Rounds packed single-precision floating-point values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Ceiling(Vector128<float> value)
    => Vector128.Create(
      MathF.Ceiling(value[0]),
      MathF.Ceiling(value[1]),
      MathF.Ceiling(value[2]),
      MathF.Ceiling(value[3])
    );

  /// <summary>Rounds packed double-precision floating-point values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Ceiling(Vector128<double> value)
    => Vector128.Create(Math.Ceiling(value[0]), Math.Ceiling(value[1]));

  /// <summary>Rounds the lower single-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> CeilingScalar(Vector128<float> value)
    => Vector128.Create(MathF.Ceiling(value[0]), value[1], value[2], value[3]);

  /// <summary>Rounds the lower single-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> CeilingScalar(Vector128<float> upper, Vector128<float> value)
    => Vector128.Create(MathF.Ceiling(value[0]), upper[1], upper[2], upper[3]);

  /// <summary>Rounds the lower double-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CeilingScalar(Vector128<double> value)
    => Vector128.Create(Math.Ceiling(value[0]), value[1]);

  /// <summary>Rounds the lower double-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> CeilingScalar(Vector128<double> upper, Vector128<double> value)
    => Vector128.Create(Math.Ceiling(value[0]), upper[1]);

  /// <summary>Rounds packed single-precision floating-point values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Floor(Vector128<float> value)
    => Vector128.Create(
      MathF.Floor(value[0]),
      MathF.Floor(value[1]),
      MathF.Floor(value[2]),
      MathF.Floor(value[3])
    );

  /// <summary>Rounds packed double-precision floating-point values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> Floor(Vector128<double> value)
    => Vector128.Create(Math.Floor(value[0]), Math.Floor(value[1]));

  /// <summary>Rounds the lower single-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> FloorScalar(Vector128<float> value)
    => Vector128.Create(MathF.Floor(value[0]), value[1], value[2], value[3]);

  /// <summary>Rounds the lower single-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> FloorScalar(Vector128<float> upper, Vector128<float> value)
    => Vector128.Create(MathF.Floor(value[0]), upper[1], upper[2], upper[3]);

  /// <summary>Rounds the lower double-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> FloorScalar(Vector128<double> value)
    => Vector128.Create(Math.Floor(value[0]), value[1]);

  /// <summary>Rounds the lower double-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> FloorScalar(Vector128<double> upper, Vector128<double> value)
    => Vector128.Create(Math.Floor(value[0]), upper[1]);

  /// <summary>Rounds packed single-precision floating-point values toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToZero(Vector128<float> value)
    => Vector128.Create(
      MathF.Truncate(value[0]),
      MathF.Truncate(value[1]),
      MathF.Truncate(value[2]),
      MathF.Truncate(value[3])
    );

  /// <summary>Rounds packed double-precision floating-point values toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToZero(Vector128<double> value)
    => Vector128.Create(Math.Truncate(value[0]), Math.Truncate(value[1]));

  /// <summary>Rounds the lower single-precision floating-point value toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToZeroScalar(Vector128<float> value)
    => Vector128.Create(MathF.Truncate(value[0]), value[1], value[2], value[3]);

  /// <summary>Rounds the lower single-precision floating-point value toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToZeroScalar(Vector128<float> upper, Vector128<float> value)
    => Vector128.Create(MathF.Truncate(value[0]), upper[1], upper[2], upper[3]);

  /// <summary>Rounds the lower double-precision floating-point value toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToZeroScalar(Vector128<double> value)
    => Vector128.Create(Math.Truncate(value[0]), value[1]);

  /// <summary>Rounds the lower double-precision floating-point value toward zero (truncate).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToZeroScalar(Vector128<double> upper, Vector128<double> value)
    => Vector128.Create(Math.Truncate(value[0]), upper[1]);

  /// <summary>Rounds packed single-precision floating-point values to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNearestInteger(Vector128<float> value)
    => Vector128.Create(
      MathF.Round(value[0], MidpointRounding.ToEven),
      MathF.Round(value[1], MidpointRounding.ToEven),
      MathF.Round(value[2], MidpointRounding.ToEven),
      MathF.Round(value[3], MidpointRounding.ToEven)
    );

  /// <summary>Rounds packed double-precision floating-point values to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNearestInteger(Vector128<double> value)
    => Vector128.Create(
      Math.Round(value[0], MidpointRounding.ToEven),
      Math.Round(value[1], MidpointRounding.ToEven)
    );

  /// <summary>Rounds the lower single-precision floating-point value to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNearestIntegerScalar(Vector128<float> value)
    => Vector128.Create(MathF.Round(value[0], MidpointRounding.ToEven), value[1], value[2], value[3]);

  /// <summary>Rounds the lower single-precision floating-point value to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNearestIntegerScalar(Vector128<float> upper, Vector128<float> value)
    => Vector128.Create(MathF.Round(value[0], MidpointRounding.ToEven), upper[1], upper[2], upper[3]);

  /// <summary>Rounds the lower double-precision floating-point value to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNearestIntegerScalar(Vector128<double> value)
    => Vector128.Create(Math.Round(value[0], MidpointRounding.ToEven), value[1]);

  /// <summary>Rounds the lower double-precision floating-point value to the nearest integer.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNearestIntegerScalar(Vector128<double> upper, Vector128<double> value)
    => Vector128.Create(Math.Round(value[0], MidpointRounding.ToEven), upper[1]);

  /// <summary>Rounds packed single-precision floating-point values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNegativeInfinity(Vector128<float> value) => Floor(value);

  /// <summary>Rounds packed double-precision floating-point values toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNegativeInfinity(Vector128<double> value) => Floor(value);

  /// <summary>Rounds the lower single-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNegativeInfinityScalar(Vector128<float> value) => FloorScalar(value);

  /// <summary>Rounds the lower single-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToNegativeInfinityScalar(Vector128<float> upper, Vector128<float> value) => FloorScalar(upper, value);

  /// <summary>Rounds the lower double-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNegativeInfinityScalar(Vector128<double> value) => FloorScalar(value);

  /// <summary>Rounds the lower double-precision floating-point value toward negative infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToNegativeInfinityScalar(Vector128<double> upper, Vector128<double> value) => FloorScalar(upper, value);

  /// <summary>Rounds packed single-precision floating-point values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToPositiveInfinity(Vector128<float> value) => Ceiling(value);

  /// <summary>Rounds packed double-precision floating-point values toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToPositiveInfinity(Vector128<double> value) => Ceiling(value);

  /// <summary>Rounds the lower single-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToPositiveInfinityScalar(Vector128<float> value) => CeilingScalar(value);

  /// <summary>Rounds the lower single-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundToPositiveInfinityScalar(Vector128<float> upper, Vector128<float> value) => CeilingScalar(upper, value);

  /// <summary>Rounds the lower double-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToPositiveInfinityScalar(Vector128<double> value) => CeilingScalar(value);

  /// <summary>Rounds the lower double-precision floating-point value toward positive infinity.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundToPositiveInfinityScalar(Vector128<double> upper, Vector128<double> value) => CeilingScalar(upper, value);

  /// <summary>Rounds using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundCurrentDirection(Vector128<float> value) => RoundToNearestInteger(value);

  /// <summary>Rounds using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundCurrentDirection(Vector128<double> value) => RoundToNearestInteger(value);

  /// <summary>Rounds scalar using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundCurrentDirectionScalar(Vector128<float> value) => RoundToNearestIntegerScalar(value);

  /// <summary>Rounds scalar using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> RoundCurrentDirectionScalar(Vector128<float> upper, Vector128<float> value) => RoundToNearestIntegerScalar(upper, value);

  /// <summary>Rounds scalar using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundCurrentDirectionScalar(Vector128<double> value) => RoundToNearestIntegerScalar(value);

  /// <summary>Rounds scalar using current rounding mode.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> RoundCurrentDirectionScalar(Vector128<double> upper, Vector128<double> value) => RoundToNearestIntegerScalar(upper, value);

  #endregion

  #region Conversion Operations

  /// <summary>Sign-extends packed 8-bit signed integers to packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ConvertToVector128Int16(Vector128<sbyte> value)
    => Vector128.Create(
      value[0], value[1], value[2], value[3],
      value[4], value[5], value[6], value[7]
    );

  /// <summary>Zero-extends packed 8-bit unsigned integers to packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ConvertToVector128Int16(Vector128<byte> value)
    => Vector128.Create(
      value[0], value[1], value[2], value[3],
      value[4], value[5], value[6], value[7]
    );

  /// <summary>Sign-extends packed 16-bit signed integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<short> value)
    => Vector128.Create(value[0], value[1], value[2], value[3]);

  /// <summary>Zero-extends packed 16-bit unsigned integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<ushort> value)
    => Vector128.Create(value[0], value[1], value[2], value[3]);

  /// <summary>Sign-extends packed 8-bit signed integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<sbyte> value)
    => Vector128.Create(value[0], value[1], value[2], value[3]);

  /// <summary>Zero-extends packed 8-bit unsigned integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ConvertToVector128Int32(Vector128<byte> value)
    => Vector128.Create(value[0], value[1], value[2], value[3]);

  /// <summary>Sign-extends packed 32-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<int> value)
    => Vector128.Create(value[0], value[1]);

  /// <summary>Zero-extends packed 32-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<uint> value)
    => Vector128.Create(value[0], value[1]);

  /// <summary>Sign-extends packed 16-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<short> value)
    => Vector128.Create(value[0], value[1]);

  /// <summary>Zero-extends packed 16-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<ushort> value)
    => Vector128.Create(value[0], value[1]);

  /// <summary>Sign-extends packed 8-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<sbyte> value)
    => Vector128.Create(value[0], value[1]);

  /// <summary>Zero-extends packed 8-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ConvertToVector128Int64(Vector128<byte> value)
    => Vector128.Create(value[0], value[1]);

  #endregion

  #region Comparison Operations

  /// <summary>Compares packed 64-bit integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> CompareEqual(Vector128<long> left, Vector128<long> right) {
    const long AllBitsSet = unchecked((long)0xFFFFFFFFFFFFFFFF);
    const long NoBitsSet = 0L;
    return Vector128.Create(
      left[0] == right[0] ? AllBitsSet : NoBitsSet,
      left[1] == right[1] ? AllBitsSet : NoBitsSet
    );
  }

  /// <summary>Compares packed 64-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> CompareEqual(Vector128<ulong> left, Vector128<ulong> right) {
    const ulong AllBitsSet = 0xFFFFFFFFFFFFFFFF;
    const ulong NoBitsSet = 0UL;
    return Vector128.Create(
      left[0] == right[0] ? AllBitsSet : NoBitsSet,
      left[1] == right[1] ? AllBitsSet : NoBitsSet
    );
  }

  #endregion

  #region Dot Product

  /// <summary>Computes conditional dot product of single-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> DotProduct(Vector128<float> left, Vector128<float> right, byte control) {
    var sum = 0.0f;
    for (var i = 0; i < 4; ++i)
      if (((control >> (i + 4)) & 1) != 0)
        sum += left[i] * right[i];

    return Vector128.Create(
      ((control >> 0) & 1) != 0 ? sum : 0,
      ((control >> 1) & 1) != 0 ? sum : 0,
      ((control >> 2) & 1) != 0 ? sum : 0,
      ((control >> 3) & 1) != 0 ? sum : 0
    );
  }

  /// <summary>Computes conditional dot product of double-precision floating-point values.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> DotProduct(Vector128<double> left, Vector128<double> right, byte control) {
    var sum = 0.0;
    for (var i = 0; i < 2; ++i)
      if (((control >> (i + 4)) & 1) != 0)
        sum += left[i] * right[i];

    return Vector128.Create(
      ((control >> 0) & 1) != 0 ? sum : 0,
      ((control >> 1) & 1) != 0 ? sum : 0
    );
  }

  #endregion

  #region Extract Operations

  /// <summary>Extracts a 32-bit integer from a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Extract(Vector128<int> value, byte index) => value[index & 3];

  /// <summary>Extracts a 32-bit unsigned integer from a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Extract(Vector128<uint> value, byte index) => value[index & 3];

  /// <summary>Extracts an 8-bit unsigned integer from a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Extract(Vector128<byte> value, byte index) => value[index & 15];

  /// <summary>Extracts a single-precision floating-point value from a packed float vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Extract(Vector128<float> value, byte index) => value[index & 3];

  #endregion

  #region Insert Operations

  /// <summary>Inserts a single-precision floating-point value into a packed float vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> Insert(Vector128<float> value, Vector128<float> data, byte control) {
    var srcIndex = (control >> 6) & 0x3;
    var destIndex = (control >> 4) & 0x3;
    var zeroMask = control & 0xF;

    var result = Vector128<float>.Zero;
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, ((zeroMask >> i) & 1) != 0 ? 0f : value[i]);
    result = result.WithElement(destIndex, data[srcIndex]);
    return result;
  }

  /// <summary>Inserts a 32-bit signed integer into a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Insert(Vector128<int> value, int data, byte index)
    => value.WithElement(index & 3, data);

  /// <summary>Inserts a 32-bit unsigned integer into a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Insert(Vector128<uint> value, uint data, byte index)
    => value.WithElement(index & 3, data);

  /// <summary>Inserts an 8-bit signed integer into a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Insert(Vector128<sbyte> value, sbyte data, byte index)
    => value.WithElement(index & 15, data);

  /// <summary>Inserts an 8-bit unsigned integer into a packed integer vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Insert(Vector128<byte> value, byte data, byte index)
    => value.WithElement(index & 15, data);

  #endregion

  #region Min/Max Operations

  /// <summary>Computes the maximum of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Max(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Max(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, Math.Max(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the maximum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Max(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      Math.Max(left[0], right[0]),
      Math.Max(left[1], right[1]),
      Math.Max(left[2], right[2]),
      Math.Max(left[3], right[3])
    );

  /// <summary>Computes the maximum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Max(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(
      Math.Max(left[0], right[0]),
      Math.Max(left[1], right[1]),
      Math.Max(left[2], right[2]),
      Math.Max(left[3], right[3])
    );

  /// <summary>Computes the minimum of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Min(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public new static Vector128<byte> Min(Vector128<byte> left, Vector128<byte> right) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Min(Vector128<ushort> left, Vector128<ushort> right) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, Math.Min(left[i], right[i]));
    return result;
  }

  /// <summary>Computes the minimum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Min(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      Math.Min(left[0], right[0]),
      Math.Min(left[1], right[1]),
      Math.Min(left[2], right[2]),
      Math.Min(left[3], right[3])
    );

  /// <summary>Computes the minimum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Min(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(
      Math.Min(left[0], right[0]),
      Math.Min(left[1], right[1]),
      Math.Min(left[2], right[2]),
      Math.Min(left[3], right[3])
    );

  /// <summary>Finds the horizontal minimum of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> MinHorizontal(Vector128<ushort> value) {
    var minValue = value[0];
    var minIndex = 0;
    for (var i = 1; i < 8; ++i) {
      if (value[i] < minValue) {
        minValue = value[i];
        minIndex = i;
      }
    }
    return Vector128.Create(minValue, (ushort)minIndex, 0, 0, 0, 0, 0, 0);
  }

  #endregion

  #region Multiply Operations

  /// <summary>Multiplies packed 32-bit signed integers, returns low 32 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> MultiplyLow(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      left[0] * right[0],
      left[1] * right[1],
      left[2] * right[2],
      left[3] * right[3]
    );

  /// <summary>Multiplies packed 32-bit unsigned integers, returns low 32 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> MultiplyLow(Vector128<uint> left, Vector128<uint> right)
    => Vector128.Create(
      left[0] * right[0],
      left[1] * right[1],
      left[2] * right[2],
      left[3] * right[3]
    );

  /// <summary>Multiplies packed 32-bit signed integers, returns 64-bit results.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> Multiply(Vector128<int> left, Vector128<int> right)
    => Vector128.Create(
      (long)left[0] * right[0],
      (long)left[2] * right[2]
    );

  #endregion

  #region Pack Operations

  /// <summary>Packs 32-bit signed integers to 16-bit unsigned integers with unsigned saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> PackUnsignedSaturate(Vector128<int> left, Vector128<int> right) {
    static ushort Saturate(int value) => value < 0 ? (ushort)0 : value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
    return Vector128.Create(
      Saturate(left[0]), Saturate(left[1]), Saturate(left[2]), Saturate(left[3]),
      Saturate(right[0]), Saturate(right[1]), Saturate(right[2]), Saturate(right[3])
    );
  }

  #endregion

  #region Sum of Absolute Differences

  /// <summary>Computes sum of absolute differences of unsigned 8-bit integers in groups of 4.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> MultipleSumAbsoluteDifferences(Vector128<byte> left, Vector128<byte> right, byte control) {
    var result = Vector128<ushort>.Zero;
    var leftOffset = (control >> 0) & 3;
    var rightOffset = (control >> 2) & 3;

    for (var i = 0; i < 8; ++i) {
      var sum = 0;
      for (var j = 0; j < 4; ++j) {
        var l = left[(leftOffset * 4 + i + j) & 15];
        var r = right[(rightOffset * 4 + j) & 15];
        sum += Math.Abs(l - r);
      }
      result = result.WithElement(i, (ushort)sum);
    }
    return result;
  }

  #endregion

  #region Test Operations

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<sbyte> left, Vector128<sbyte> right) {
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<byte> left, Vector128<byte> right) {
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<short> left, Vector128<short> right) {
    for (var i = 0; i < Vector128<short>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<ushort> left, Vector128<ushort> right) {
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<int> left, Vector128<int> right) {
    for (var i = 0; i < Vector128<int>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<uint> left, Vector128<uint> right) {
    for (var i = 0; i < Vector128<uint>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<long> left, Vector128<long> right) {
    for (var i = 0; i < Vector128<long>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits are zero in the result of ANDing two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestZ(Vector128<ulong> left, Vector128<ulong> right) {
    for (var i = 0; i < Vector128<ulong>.Count; ++i)
      if ((left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<sbyte> left, Vector128<sbyte> right) {
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<byte> left, Vector128<byte> right) {
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<short> left, Vector128<short> right) {
    for (var i = 0; i < Vector128<short>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<ushort> left, Vector128<ushort> right) {
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<int> left, Vector128<int> right) {
    for (var i = 0; i < Vector128<int>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<uint> left, Vector128<uint> right) {
    for (var i = 0; i < Vector128<uint>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<long> left, Vector128<long> right) {
    for (var i = 0; i < Vector128<long>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if all bits in right are set in left (CF flag).</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestC(Vector128<ulong> left, Vector128<ulong> right) {
    for (var i = 0; i < Vector128<ulong>.Count; ++i)
      if ((~left[i] & right[i]) != 0)
        return false;
    return true;
  }

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<sbyte> left, Vector128<sbyte> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<byte> left, Vector128<byte> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<short> left, Vector128<short> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<ushort> left, Vector128<ushort> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<int> left, Vector128<int> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<uint> left, Vector128<uint> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<long> left, Vector128<long> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=0 and CF=0.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestNotZAndNotC(Vector128<ulong> left, Vector128<ulong> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<sbyte> left, Vector128<sbyte> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<byte> left, Vector128<byte> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<int> left, Vector128<int> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<uint> left, Vector128<uint> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<long> left, Vector128<long> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests if ZF=1 or CF=1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestMixOnesZeros(Vector128<ulong> left, Vector128<ulong> right) => !TestZ(left, right) && !TestC(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<sbyte> left, Vector128<sbyte> right) => TestZ(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<byte> left, Vector128<byte> right) => TestZ(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<int> left, Vector128<int> right) => TestZ(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<uint> left, Vector128<uint> right) => TestZ(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<long> left, Vector128<long> right) => TestZ(left, right);

  /// <summary>Tests all zeroes in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllZeros(Vector128<ulong> left, Vector128<ulong> right) => TestZ(left, right);

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<sbyte> value) => TestC(value, Vector128.Create((sbyte)-1));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<byte> value) => TestC(value, Vector128.Create((byte)0xFF));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<short> value) => TestC(value, Vector128.Create((short)-1));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<ushort> value) => TestC(value, Vector128.Create(ushort.MaxValue));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<int> value) => TestC(value, Vector128.Create(-1));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<uint> value) => TestC(value, Vector128.Create(uint.MaxValue));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<long> value) => TestC(value, Vector128.Create(-1L));

  /// <summary>Tests all ones in masked elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TestAllOnes(Vector128<ulong> value) => TestC(value, Vector128.Create(ulong.MaxValue));

  #endregion

  #region Load Operations

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<sbyte> LoadAlignedVector128NonTemporal(sbyte* address) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<byte> LoadAlignedVector128NonTemporal(byte* address) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<short> LoadAlignedVector128NonTemporal(short* address) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ushort> LoadAlignedVector128NonTemporal(ushort* address) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> LoadAlignedVector128NonTemporal(int* address) {
    var result = Vector128<int>.Zero;
    for (var i = 0; i < Vector128<int>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> LoadAlignedVector128NonTemporal(uint* address) {
    var result = Vector128<uint>.Zero;
    for (var i = 0; i < Vector128<uint>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<long> LoadAlignedVector128NonTemporal(long* address) {
    var result = Vector128<long>.Zero;
    for (var i = 0; i < Vector128<long>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  /// <summary>Loads 128 bits from memory using non-temporal hint.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<ulong> LoadAlignedVector128NonTemporal(ulong* address) {
    var result = Vector128<ulong>.Zero;
    for (var i = 0; i < Vector128<ulong>.Count; ++i)
      result = result.WithElement(i, address[i]);
    return result;
  }

  #endregion

  #region X64 Nested Class

  /// <summary>Provides 64-bit specific SSE4.1 operations.</summary>
  public new abstract class X64 : Ssse3.X64 {
    /// <summary>Gets a value indicating whether 64-bit SSE4.1 instructions are supported.</summary>
    public new static bool IsSupported => false;

    /// <summary>Extracts a 64-bit integer from a packed integer vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Extract(Vector128<long> value, byte index) => value[index & 1];

    /// <summary>Extracts a 64-bit unsigned integer from a packed integer vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Extract(Vector128<ulong> value, byte index) => value[index & 1];

    /// <summary>Inserts a 64-bit integer into a packed integer vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<long> Insert(Vector128<long> value, long data, byte index)
      => value.WithElement(index & 1, data);

    /// <summary>Inserts a 64-bit unsigned integer into a packed integer vector.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<ulong> Insert(Vector128<ulong> value, ulong data, byte index)
      => value.WithElement(index & 1, data);
  }

  #endregion
}

#endif
