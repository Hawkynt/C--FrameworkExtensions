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
using System.Runtime.Intrinsics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of SSSE3 intrinsics for platforms without native support.
/// Provides supplemental SSE3 operations including horizontal operations, absolute values, and byte shuffling.
/// </summary>
public abstract class Ssse3 : Sse3 {

  /// <summary>Gets a value indicating whether SSSE3 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Absolute Value Operations

  /// <summary>
  /// Computes the absolute value of packed 8-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Abs(Vector128<sbyte> value) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      var v = value[i];
      result = result.WithElement(i, (byte)(v < 0 ? -v : v));
    }
    return result;
  }

  /// <summary>
  /// Computes the absolute value of packed 16-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> Abs(Vector128<short> value) {
    var result = Vector128<ushort>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      var v = value[i];
      result = result.WithElement(i, (ushort)(v < 0 ? -v : v));
    }
    return result;
  }

  /// <summary>
  /// Computes the absolute value of packed 32-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> Abs(Vector128<int> value) {
    var result = Vector128<uint>.Zero;
    for (var i = 0; i < Vector128<int>.Count; ++i) {
      var v = value[i];
      result = result.WithElement(i, (uint)(v < 0 ? -v : v));
    }
    return result;
  }

  #endregion

  #region AlignRight Operations

  /// <summary>
  /// Concatenates pairs of 16 byte blocks and extracts the result shifted by a constant number of bytes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> AlignRight(Vector128<sbyte> left, Vector128<sbyte> right, byte mask) {
    var offset = mask & 0x1F;
    if (offset >= 32)
      return Vector128<sbyte>.Zero;

    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      var sourceIndex = offset + i;
      if (sourceIndex < 16)
        result = result.WithElement(i, right[sourceIndex]);
      else if (sourceIndex < 32)
        result = result.WithElement(i, left[sourceIndex - 16]);
    }
    return result;
  }

  /// <summary>
  /// Concatenates pairs of 16 byte blocks and extracts the result shifted by a constant number of bytes.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> AlignRight(Vector128<byte> left, Vector128<byte> right, byte mask) {
    var offset = mask & 0x1F;
    if (offset >= 32)
      return Vector128<byte>.Zero;

    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i) {
      var sourceIndex = offset + i;
      if (sourceIndex < 16)
        result = result.WithElement(i, right[sourceIndex]);
      else if (sourceIndex < 32)
        result = result.WithElement(i, left[sourceIndex - 16]);
    }
    return result;
  }

  #endregion

  #region Horizontal Add Operations

  /// <summary>
  /// Horizontally adds adjacent pairs of 16-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> HorizontalAdd(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    result = result.WithElement(0, (short)(left[0] + left[1]));
    result = result.WithElement(1, (short)(left[2] + left[3]));
    result = result.WithElement(2, (short)(left[4] + left[5]));
    result = result.WithElement(3, (short)(left[6] + left[7]));
    result = result.WithElement(4, (short)(right[0] + right[1]));
    result = result.WithElement(5, (short)(right[2] + right[3]));
    result = result.WithElement(6, (short)(right[4] + right[5]));
    result = result.WithElement(7, (short)(right[6] + right[7]));
    return result;
  }

  /// <summary>
  /// Horizontally adds adjacent pairs of 32-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> HorizontalAdd(Vector128<int> left, Vector128<int> right) {
    var result = Vector128<int>.Zero;
    result = result.WithElement(0, left[0] + left[1]);
    result = result.WithElement(1, left[2] + left[3]);
    result = result.WithElement(2, right[0] + right[1]);
    result = result.WithElement(3, right[2] + right[3]);
    return result;
  }

  #endregion

  #region Horizontal Add Saturate Operations

  /// <summary>
  /// Horizontally adds adjacent pairs of 16-bit integers with saturation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> HorizontalAddSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    result = result.WithElement(0, _SaturateInt16(left[0] + left[1]));
    result = result.WithElement(1, _SaturateInt16(left[2] + left[3]));
    result = result.WithElement(2, _SaturateInt16(left[4] + left[5]));
    result = result.WithElement(3, _SaturateInt16(left[6] + left[7]));
    result = result.WithElement(4, _SaturateInt16(right[0] + right[1]));
    result = result.WithElement(5, _SaturateInt16(right[2] + right[3]));
    result = result.WithElement(6, _SaturateInt16(right[4] + right[5]));
    result = result.WithElement(7, _SaturateInt16(right[6] + right[7]));
    return result;
  }

  #endregion

  #region Horizontal Subtract Operations

  /// <summary>
  /// Horizontally subtracts adjacent pairs of 16-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> HorizontalSubtract(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    result = result.WithElement(0, (short)(left[0] - left[1]));
    result = result.WithElement(1, (short)(left[2] - left[3]));
    result = result.WithElement(2, (short)(left[4] - left[5]));
    result = result.WithElement(3, (short)(left[6] - left[7]));
    result = result.WithElement(4, (short)(right[0] - right[1]));
    result = result.WithElement(5, (short)(right[2] - right[3]));
    result = result.WithElement(6, (short)(right[4] - right[5]));
    result = result.WithElement(7, (short)(right[6] - right[7]));
    return result;
  }

  /// <summary>
  /// Horizontally subtracts adjacent pairs of 32-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> HorizontalSubtract(Vector128<int> left, Vector128<int> right) {
    var result = Vector128<int>.Zero;
    result = result.WithElement(0, left[0] - left[1]);
    result = result.WithElement(1, left[2] - left[3]);
    result = result.WithElement(2, right[0] - right[1]);
    result = result.WithElement(3, right[2] - right[3]);
    return result;
  }

  #endregion

  #region Horizontal Subtract Saturate Operations

  /// <summary>
  /// Horizontally subtracts adjacent pairs of 16-bit integers with saturation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> HorizontalSubtractSaturate(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    result = result.WithElement(0, _SaturateInt16(left[0] - left[1]));
    result = result.WithElement(1, _SaturateInt16(left[2] - left[3]));
    result = result.WithElement(2, _SaturateInt16(left[4] - left[5]));
    result = result.WithElement(3, _SaturateInt16(left[6] - left[7]));
    result = result.WithElement(4, _SaturateInt16(right[0] - right[1]));
    result = result.WithElement(5, _SaturateInt16(right[2] - right[3]));
    result = result.WithElement(6, _SaturateInt16(right[4] - right[5]));
    result = result.WithElement(7, _SaturateInt16(right[6] - right[7]));
    return result;
  }

  #endregion

  #region Multiply Add Adjacent Operations

  /// <summary>
  /// Multiplies vertically each unsigned 8-bit integer from left with the corresponding signed 8-bit integer from right,
  /// producing intermediate signed 16-bit integers. Horizontally add each adjacent pair of signed 16-bit integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> MultiplyAddAdjacent(Vector128<byte> left, Vector128<sbyte> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      var idx = i * 2;
      var prod1 = left[idx] * right[idx];
      var prod2 = left[idx + 1] * right[idx + 1];
      result = result.WithElement(i, _SaturateInt16(prod1 + prod2));
    }
    return result;
  }

  #endregion

  #region Multiply High Round Scale Operations

  /// <summary>
  /// Multiplies packed 16-bit signed integers, producing intermediate signed 32-bit integers.
  /// Truncates the intermediate integers to the 18 most significant bits.
  /// Rounds by adding 1 to the least significant bit of the 18-bit value.
  /// Extracts bits 16:1 from each 32-bit intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> MultiplyHighRoundScale(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      var prod = left[i] * right[i];
      var truncated = (prod >> 14) & 0x3FFFF;
      var rounded = truncated + 1;
      result = result.WithElement(i, (short)((rounded >> 1) & 0xFFFF));
    }
    return result;
  }

  #endregion

  #region Sign Operations

  /// <summary>
  /// Negates packed 8-bit integers when the corresponding signed 8-bit integer is negative,
  /// and zeroes each element when the corresponding signed 8-bit integer is zero.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Sign(Vector128<sbyte> left, Vector128<sbyte> right) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, (sbyte)-left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
      // else stays zero
    }
    return result;
  }

  /// <summary>
  /// Negates packed 16-bit integers when the corresponding signed 16-bit integer is negative,
  /// and zeroes each element when the corresponding signed 16-bit integer is zero.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> Sign(Vector128<short> left, Vector128<short> right) {
    var result = Vector128<short>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, (short)-left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
      // else stays zero
    }
    return result;
  }

  /// <summary>
  /// Negates packed 32-bit integers when the corresponding signed 32-bit integer is negative,
  /// and zeroes each element when the corresponding signed 32-bit integer is zero.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> Sign(Vector128<int> left, Vector128<int> right) {
    var result = Vector128<int>.Zero;
    for (var i = 0; i < Vector128<int>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, -left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
      // else stays zero
    }
    return result;
  }

  #endregion

  #region Shuffle Operations

  /// <summary>
  /// Shuffles bytes within 128-bit lanes according to indices specified in the control mask.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> Shuffle(Vector128<sbyte> value, Vector128<sbyte> mask) {
    var result = Vector128<sbyte>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i) {
      var index = mask[i];
      if ((index & 0x80) != 0)
        result = Vector128.WithElement(result, i, (sbyte)0);
      else
        result = Vector128.WithElement(result, i, value[index & 0x0F]);
    }
    return result;
  }

  /// <summary>
  /// Shuffles bytes within 128-bit lanes according to indices specified in the control mask.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> Shuffle(Vector128<byte> value, Vector128<byte> mask) {
    var result = Vector128<byte>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i) {
      var index = mask[i];
      if ((index & 0x80) != 0)
        result = Vector128.WithElement(result, i, (byte)0);
      else
        result = Vector128.WithElement(result, i, value[index & 0x0F]);
    }
    return result;
  }

  #endregion

  #region Helper Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static short _SaturateInt16(int value) {
    if (value > short.MaxValue)
      return short.MaxValue;
    if (value < short.MinValue)
      return short.MinValue;
    return (short)value;
  }

  #endregion

  /// <summary>Provides 64-bit specific SSSE3 operations.</summary>
  public new abstract class X64 : Sse3.X64 {

    /// <summary>Gets a value indicating whether 64-bit SSSE3 instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif
