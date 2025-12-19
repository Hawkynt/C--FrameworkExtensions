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
/// Software fallback implementation of AVX2 (Advanced Vector Extensions 2) intrinsics.
/// Provides 256-bit integer vector operations.
/// </summary>
public abstract class Avx2 : Avx {

  /// <summary>Gets a value indicating whether AVX2 instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region Add Operations

  /// <summary>Adds packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Add(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Add(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Add(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Add(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] + right[i]));
    return result;
  }

  /// <summary>Adds packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Add(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Add(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Add(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  /// <summary>Adds packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Add(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] + right[i]);
    return result;
  }

  #endregion

  #region Add Saturating Operations

  /// <summary>Adds packed 8-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> AddSaturate(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, sum)));
    }
    return result;
  }

  /// <summary>Adds packed 8-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> AddSaturate(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, (byte)Math.Min(byte.MaxValue, sum));
    }
    return result;
  }

  /// <summary>Adds packed 16-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> AddSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    return result;
  }

  /// <summary>Adds packed 16-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> AddSaturate(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i) {
      var sum = left[i] + right[i];
      result = result.WithElement(i, (ushort)Math.Min(ushort.MaxValue, sum));
    }
    return result;
  }

  #endregion

  #region Subtract Operations

  /// <summary>Subtracts packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Subtract(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Subtract(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Subtract(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Subtract(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] - right[i]));
    return result;
  }

  /// <summary>Subtracts packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Subtract(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Subtract(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Subtract(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  /// <summary>Subtracts packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Subtract(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] - right[i]);
    return result;
  }

  #endregion

  #region Subtract Saturating Operations

  /// <summary>Subtracts packed 8-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> SubtractSaturate(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, diff)));
    }
    return result;
  }

  /// <summary>Subtracts packed 8-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> SubtractSaturate(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, (byte)Math.Max(0, diff));
    }
    return result;
  }

  /// <summary>Subtracts packed 16-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> SubtractSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, diff)));
    }
    return result;
  }

  /// <summary>Subtracts packed 16-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> SubtractSaturate(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i) {
      var diff = left[i] - right[i];
      result = result.WithElement(i, (ushort)Math.Max(0, diff));
    }
    return result;
  }

  #endregion

  #region Multiply Operations

  /// <summary>Multiplies packed 16-bit signed integers, returning low 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> MultiplyLow(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] * right[i]));
    return result;
  }

  /// <summary>Multiplies packed 16-bit unsigned integers, returning low 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> MultiplyLow(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] * right[i]));
    return result;
  }

  /// <summary>Multiplies packed 32-bit signed integers, returning low 32 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> MultiplyLow(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  /// <summary>Multiplies packed 32-bit unsigned integers, returning low 32 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> MultiplyLow(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] * right[i]);
    return result;
  }

  /// <summary>Multiplies packed 16-bit signed integers, returning high 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> MultiplyHigh(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i) {
      var product = left[i] * right[i];
      result = result.WithElement(i, (short)(product >> 16));
    }
    return result;
  }

  /// <summary>Multiplies packed 16-bit unsigned integers, returning high 16 bits.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> MultiplyHigh(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i) {
      var product = (uint)left[i] * right[i];
      result = result.WithElement(i, (ushort)(product >> 16));
    }
    return result;
  }

  /// <summary>Multiplies packed 32-bit signed integers to produce 64-bit results.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Multiply(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, (long)left[i * 2] * right[i * 2]);
    return result;
  }

  /// <summary>Multiplies packed 32-bit unsigned integers to produce 64-bit results.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Multiply(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, (ulong)left[i * 2] * right[i * 2]);
    return result;
  }

  /// <summary>Multiplies and adds packed 16-bit signed integers to produce 32-bit results.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> MultiplyAddAdjacent(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      var j = i * 2;
      result = result.WithElement(i, left[j] * right[j] + left[j + 1] * right[j + 1]);
    }
    return result;
  }

  /// <summary>Multiplies and adds adjacent pairs of 8-bit unsigned and signed values, producing 16-bit results with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> MultiplyAddAdjacent(Vector256<byte> left, Vector256<sbyte> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i) {
      var j = i * 2;
      var sum = left[j] * right[j] + left[j + 1] * right[j + 1];
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    return result;
  }

  /// <summary>Horizontally add adjacent pairs of 16-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> HorizontalAddSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 4; ++i) {
      var sum = left[i * 2] + left[i * 2 + 1];
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    for (var i = 0; i < 4; ++i) {
      var sum = right[i * 2] + right[i * 2 + 1];
      result = result.WithElement(i + 4, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    // Process upper 128-bit lane
    for (var i = 0; i < 4; ++i) {
      var sum = left[8 + i * 2] + left[8 + i * 2 + 1];
      result = result.WithElement(i + 8, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    for (var i = 0; i < 4; ++i) {
      var sum = right[8 + i * 2] + right[8 + i * 2 + 1];
      result = result.WithElement(i + 12, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, sum)));
    }
    return result;
  }

  /// <summary>Horizontally subtract adjacent pairs of 16-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> HorizontalSubtractSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 4; ++i) {
      var diff = left[i * 2] - left[i * 2 + 1];
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, diff)));
    }
    for (var i = 0; i < 4; ++i) {
      var diff = right[i * 2] - right[i * 2 + 1];
      result = result.WithElement(i + 4, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, diff)));
    }
    // Process upper 128-bit lane
    for (var i = 0; i < 4; ++i) {
      var diff = left[8 + i * 2] - left[8 + i * 2 + 1];
      result = result.WithElement(i + 8, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, diff)));
    }
    for (var i = 0; i < 4; ++i) {
      var diff = right[8 + i * 2] - right[8 + i * 2 + 1];
      result = result.WithElement(i + 12, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, diff)));
    }
    return result;
  }

  #endregion

  #region Horizontal Operations

  /// <summary>Horizontally add adjacent pairs of 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> HorizontalAdd(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, (short)(left[i * 2] + left[i * 2 + 1]));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 4, (short)(right[i * 2] + right[i * 2 + 1]));
    // Upper lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 8, (short)(left[8 + i * 2] + left[8 + i * 2 + 1]));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 12, (short)(right[8 + i * 2] + right[8 + i * 2 + 1]));
    return result;
  }

  /// <summary>Horizontally add adjacent pairs of 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> HorizontalAdd(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    // Lower lane
    result = result.WithElement(0, left[0] + left[1]);
    result = result.WithElement(1, left[2] + left[3]);
    result = result.WithElement(2, right[0] + right[1]);
    result = result.WithElement(3, right[2] + right[3]);
    // Upper lane
    result = result.WithElement(4, left[4] + left[5]);
    result = result.WithElement(5, left[6] + left[7]);
    result = result.WithElement(6, right[4] + right[5]);
    result = result.WithElement(7, right[6] + right[7]);
    return result;
  }

  /// <summary>Horizontally subtract adjacent pairs of 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> HorizontalSubtract(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, (short)(left[i * 2] - left[i * 2 + 1]));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 4, (short)(right[i * 2] - right[i * 2 + 1]));
    // Upper lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 8, (short)(left[8 + i * 2] - left[8 + i * 2 + 1]));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 12, (short)(right[8 + i * 2] - right[8 + i * 2 + 1]));
    return result;
  }

  /// <summary>Horizontally subtract adjacent pairs of 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> HorizontalSubtract(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    // Lower lane
    result = result.WithElement(0, left[0] - left[1]);
    result = result.WithElement(1, left[2] - left[3]);
    result = result.WithElement(2, right[0] - right[1]);
    result = result.WithElement(3, right[2] - right[3]);
    // Upper lane
    result = result.WithElement(4, left[4] - left[5]);
    result = result.WithElement(5, left[6] - left[7]);
    result = result.WithElement(6, right[4] - right[5]);
    result = result.WithElement(7, right[6] - right[7]);
    return result;
  }

  #endregion

  #region Logical Operations

  /// <summary>Computes bitwise AND of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> And(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> And(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> And(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> And(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> And(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> And(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> And(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> And(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> AndNot(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(~left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> AndNot(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(~left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> AndNot(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(~left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> AndNot(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(~left[i] & right[i]));
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> AndNot(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> AndNot(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> AndNot(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise AND NOT of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> AndNot(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, ~left[i] & right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Or(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] | right[i]));
    return result;
  }

  /// <summary>Computes bitwise OR of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Or(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] | right[i]));
    return result;
  }

  /// <summary>Computes bitwise OR of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Or(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] | right[i]));
    return result;
  }

  /// <summary>Computes bitwise OR of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Or(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] | right[i]));
    return result;
  }

  /// <summary>Computes bitwise OR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Or(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Or(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Or(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise OR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Or(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] | right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 8-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Xor(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (sbyte)(left[i] ^ right[i]));
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Xor(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)(left[i] ^ right[i]));
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 16-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Xor(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(left[i] ^ right[i]));
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Xor(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(left[i] ^ right[i]));
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 32-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Xor(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Xor(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 64-bit integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Xor(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  /// <summary>Computes bitwise XOR of packed 64-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Xor(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] ^ right[i]);
    return result;
  }

  #endregion

  #region Comparison Operations

  /// <summary>Compares packed 8-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> CompareEqual(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? (sbyte)-1 : (sbyte)0);
    return result;
  }

  /// <summary>Compares packed 8-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> CompareEqual(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? (byte)0xFF : (byte)0);
    return result;
  }

  /// <summary>Compares packed 16-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> CompareEqual(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? (short)-1 : (short)0);
    return result;
  }

  /// <summary>Compares packed 16-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> CompareEqual(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? (ushort)0xFFFF : (ushort)0);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> CompareEqual(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? -1 : 0);
    return result;
  }

  /// <summary>Compares packed 32-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> CompareEqual(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? 0xFFFFFFFF : 0u);
    return result;
  }

  /// <summary>Compares packed 64-bit signed integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> CompareEqual(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? -1L : 0L);
    return result;
  }

  /// <summary>Compares packed 64-bit unsigned integers for equality.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> CompareEqual(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, left[i] == right[i] ? 0xFFFFFFFFFFFFFFFF : 0UL);
    return result;
  }

  /// <summary>Compares packed 8-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> CompareGreaterThan(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? (sbyte)-1 : (sbyte)0);
    return result;
  }

  /// <summary>Compares packed 16-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> CompareGreaterThan(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? (short)-1 : (short)0);
    return result;
  }

  /// <summary>Compares packed 32-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> CompareGreaterThan(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? -1 : 0);
    return result;
  }

  /// <summary>Compares packed 64-bit signed integers for greater-than.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> CompareGreaterThan(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? -1L : 0L);
    return result;
  }

  #endregion

  #region Min/Max Operations

  /// <summary>Computes minimum of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Min(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes minimum of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Min(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes minimum of packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Min(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes minimum of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Min(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes minimum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Min(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes minimum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Min(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] < right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Max(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Max(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Max(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 16-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Max(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Max(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  /// <summary>Computes maximum of packed 32-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Max(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[i] > right[i] ? left[i] : right[i]);
    return result;
  }

  #endregion

  #region Absolute Value Operations

  /// <summary>Computes absolute value of packed 8-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Abs(Vector256<sbyte> value) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, (byte)(value[i] < 0 ? -value[i] : value[i]));
    return result;
  }

  /// <summary>Computes absolute value of packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Abs(Vector256<short> value) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (ushort)(value[i] < 0 ? -value[i] : value[i]));
    return result;
  }

  /// <summary>Computes absolute value of packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Abs(Vector256<int> value) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, (uint)(value[i] < 0 ? -value[i] : value[i]));
    return result;
  }

  #endregion

  #region Average Operations

  /// <summary>Computes average of packed 8-bit unsigned integers with rounding.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Average(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (byte)((left[i] + right[i] + 1) >> 1));
    return result;
  }

  /// <summary>Computes average of packed 16-bit unsigned integers with rounding.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Average(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)((left[i] + right[i] + 1) >> 1));
    return result;
  }

  #endregion

  #region Shift Operations

  /// <summary>Shifts packed 16-bit integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ShiftLeftLogical(Vector256<short> value, byte count) {
    if (count >= 16)
      return Vector256<short>.Zero;
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(value[i] << count));
    return result;
  }

  /// <summary>Shifts packed 16-bit unsigned integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> ShiftLeftLogical(Vector256<ushort> value, byte count) {
    if (count >= 16)
      return Vector256<ushort>.Zero;
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(value[i] << count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftLeftLogical(Vector256<int> value, byte count) {
    if (count >= 32)
      return Vector256<int>.Zero;
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> ShiftLeftLogical(Vector256<uint> value, byte count) {
    if (count >= 32)
      return Vector256<uint>.Zero;
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 64-bit integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ShiftLeftLogical(Vector256<long> value, byte count) {
    if (count >= 64)
      return Vector256<long>.Zero;
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers left by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> ShiftLeftLogical(Vector256<ulong> value, byte count) {
    if (count >= 64)
      return Vector256<ulong>.Zero;
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, value[i] << count);
    return result;
  }

  /// <summary>Shifts packed 128-bit integers left by specified byte count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> ShiftLeftLogical128BitLane(Vector256<sbyte> value, byte numBytes) {
    if (numBytes >= 16)
      return Vector256<sbyte>.Zero;
    var result = Vector256<sbyte>.Zero;
    // Process each 128-bit lane independently
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      for (var i = numBytes; i < 16; ++i)
        result = result.WithElement(laneOffset + i, value[laneOffset + i - numBytes]);
    }
    return result;
  }

  /// <summary>Shifts packed 128-bit integers left by specified byte count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> ShiftLeftLogical128BitLane(Vector256<byte> value, byte numBytes) {
    if (numBytes >= 16)
      return Vector256<byte>.Zero;
    var result = Vector256<byte>.Zero;
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      for (var i = numBytes; i < 16; ++i)
        result = result.WithElement(laneOffset + i, value[laneOffset + i - numBytes]);
    }
    return result;
  }

  /// <summary>Shifts packed 16-bit integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ShiftRightLogical(Vector256<short> value, byte count) {
    if (count >= 16)
      return Vector256<short>.Zero;
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)((ushort)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 16-bit unsigned integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> ShiftRightLogical(Vector256<ushort> value, byte count) {
    if (count >= 16)
      return Vector256<ushort>.Zero;
    var result = Vector256<ushort>.Zero;
    for (var i = 0; i < Vector256<ushort>.Count; ++i)
      result = result.WithElement(i, (ushort)(value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftRightLogical(Vector256<int> value, byte count) {
    if (count >= 32)
      return Vector256<int>.Zero;
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, (int)((uint)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> ShiftRightLogical(Vector256<uint> value, byte count) {
    if (count >= 32)
      return Vector256<uint>.Zero;
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 64-bit integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ShiftRightLogical(Vector256<long> value, byte count) {
    if (count >= 64)
      return Vector256<long>.Zero;
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i)
      result = result.WithElement(i, (long)((ulong)value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers right logically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> ShiftRightLogical(Vector256<ulong> value, byte count) {
    if (count >= 64)
      return Vector256<ulong>.Zero;
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 128-bit integers right logically by specified byte count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> ShiftRightLogical128BitLane(Vector256<sbyte> value, byte numBytes) {
    if (numBytes >= 16)
      return Vector256<sbyte>.Zero;
    var result = Vector256<sbyte>.Zero;
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      for (var i = 0; i < 16 - numBytes; ++i)
        result = result.WithElement(laneOffset + i, value[laneOffset + i + numBytes]);
    }
    return result;
  }

  /// <summary>Shifts packed 128-bit integers right logically by specified byte count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> ShiftRightLogical128BitLane(Vector256<byte> value, byte numBytes) {
    if (numBytes >= 16)
      return Vector256<byte>.Zero;
    var result = Vector256<byte>.Zero;
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      for (var i = 0; i < 16 - numBytes; ++i)
        result = result.WithElement(laneOffset + i, value[laneOffset + i + numBytes]);
    }
    return result;
  }

  /// <summary>Shifts packed 16-bit integers right arithmetically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ShiftRightArithmetic(Vector256<short> value, byte count) {
    if (count >= 16)
      count = 15;
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i)
      result = result.WithElement(i, (short)(value[i] >> count));
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right arithmetically by specified count.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftRightArithmetic(Vector256<int> value, byte count) {
    if (count >= 32)
      count = 31;
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, value[i] >> count);
    return result;
  }

  /// <summary>Shifts packed 32-bit integers left by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftLeftLogicalVariable(Vector256<int> value, Vector256<uint> count) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 32 ? 0 : value[i] << shift);
    }
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers left by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> ShiftLeftLogicalVariable(Vector256<uint> value, Vector256<uint> count) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 32 ? 0u : value[i] << shift);
    }
    return result;
  }

  /// <summary>Shifts packed 64-bit integers left by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ShiftLeftLogicalVariable(Vector256<long> value, Vector256<ulong> count) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 64 ? 0L : value[i] << shift);
    }
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers left by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> ShiftLeftLogicalVariable(Vector256<ulong> value, Vector256<ulong> count) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 64 ? 0UL : value[i] << shift);
    }
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right logically by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftRightLogicalVariable(Vector256<int> value, Vector256<uint> count) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 32 ? 0 : (int)((uint)value[i] >> shift));
    }
    return result;
  }

  /// <summary>Shifts packed 32-bit unsigned integers right logically by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> ShiftRightLogicalVariable(Vector256<uint> value, Vector256<uint> count) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 32 ? 0u : value[i] >> shift);
    }
    return result;
  }

  /// <summary>Shifts packed 64-bit integers right logically by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ShiftRightLogicalVariable(Vector256<long> value, Vector256<ulong> count) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 64 ? 0L : (long)((ulong)value[i] >> shift));
    }
    return result;
  }

  /// <summary>Shifts packed 64-bit unsigned integers right logically by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> ShiftRightLogicalVariable(Vector256<ulong> value, Vector256<ulong> count) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 64 ? 0UL : value[i] >> shift);
    }
    return result;
  }

  /// <summary>Shifts packed 32-bit integers right arithmetically by variable counts.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ShiftRightArithmeticVariable(Vector256<int> value, Vector256<uint> count) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      var shift = (int)count[i];
      result = result.WithElement(i, shift >= 32 ? (value[i] >> 31) : (value[i] >> shift));
    }
    return result;
  }

  #endregion

  #region Pack Operations

  /// <summary>Packs 16-bit signed integers into 8-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> PackSignedSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<sbyte>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, left[i])));
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 8, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, right[i])));
    // Process upper 128-bit lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 16, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, left[i + 8])));
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 24, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, right[i + 8])));
    return result;
  }

  /// <summary>Packs 32-bit signed integers into 16-bit signed integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> PackSignedSaturate(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<short>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, left[i])));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 4, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, right[i])));
    // Process upper 128-bit lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 8, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, left[i + 4])));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 12, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, right[i + 4])));
    return result;
  }

  /// <summary>Packs 16-bit signed integers into 8-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> PackUnsignedSaturate(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<byte>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, (byte)Math.Max(0, Math.Min(255, (int)left[i])));
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 8, (byte)Math.Max(0, Math.Min(255, (int)right[i])));
    // Process upper 128-bit lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 16, (byte)Math.Max(0, Math.Min(255, (int)left[i + 8])));
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i + 24, (byte)Math.Max(0, Math.Min(255, (int)right[i + 8])));
    return result;
  }

  /// <summary>Packs 32-bit signed integers into 16-bit unsigned integers with saturation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> PackUnsignedSaturate(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<ushort>.Zero;
    // Process lower 128-bit lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, (ushort)Math.Max(0, Math.Min(65535, left[i])));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 4, (ushort)Math.Max(0, Math.Min(65535, right[i])));
    // Process upper 128-bit lane
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 8, (ushort)Math.Max(0, Math.Min(65535, left[i + 4])));
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i + 12, (ushort)Math.Max(0, Math.Min(65535, right[i + 4])));
    return result;
  }

  #endregion

  #region Unpack Operations

  /// <summary>Unpacks and interleaves low 8-bit integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> UnpackLow(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i * 2, left[i]);
      result = result.WithElement(i * 2 + 1, right[i]);
    }
    // Upper lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(16 + i * 2, left[16 + i]);
      result = result.WithElement(16 + i * 2 + 1, right[16 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low 8-bit unsigned integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> UnpackLow(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i * 2, left[i]);
      result = result.WithElement(i * 2 + 1, right[i]);
    }
    // Upper lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(16 + i * 2, left[16 + i]);
      result = result.WithElement(16 + i * 2 + 1, right[16 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low 16-bit integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> UnpackLow(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i * 2, left[i]);
      result = result.WithElement(i * 2 + 1, right[i]);
    }
    // Upper lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(8 + i * 2, left[8 + i]);
      result = result.WithElement(8 + i * 2 + 1, right[8 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low 16-bit unsigned integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> UnpackLow(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i * 2, left[i]);
      result = result.WithElement(i * 2 + 1, right[i]);
    }
    // Upper lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(8 + i * 2, left[8 + i]);
      result = result.WithElement(8 + i * 2 + 1, right[8 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves low 32-bit integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> UnpackLow(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    // Lower lane
    result = result.WithElement(0, left[0]);
    result = result.WithElement(1, right[0]);
    result = result.WithElement(2, left[1]);
    result = result.WithElement(3, right[1]);
    // Upper lane
    result = result.WithElement(4, left[4]);
    result = result.WithElement(5, right[4]);
    result = result.WithElement(6, left[5]);
    result = result.WithElement(7, right[5]);
    return result;
  }

  /// <summary>Unpacks and interleaves low 32-bit unsigned integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> UnpackLow(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    // Lower lane
    result = result.WithElement(0, left[0]);
    result = result.WithElement(1, right[0]);
    result = result.WithElement(2, left[1]);
    result = result.WithElement(3, right[1]);
    // Upper lane
    result = result.WithElement(4, left[4]);
    result = result.WithElement(5, right[4]);
    result = result.WithElement(6, left[5]);
    result = result.WithElement(7, right[5]);
    return result;
  }

  /// <summary>Unpacks and interleaves low 64-bit integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> UnpackLow(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    result = result.WithElement(0, left[0]);
    result = result.WithElement(1, right[0]);
    result = result.WithElement(2, left[2]);
    result = result.WithElement(3, right[2]);
    return result;
  }

  /// <summary>Unpacks and interleaves low 64-bit unsigned integers from low halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> UnpackLow(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    result = result.WithElement(0, left[0]);
    result = result.WithElement(1, right[0]);
    result = result.WithElement(2, left[2]);
    result = result.WithElement(3, right[2]);
    return result;
  }

  /// <summary>Unpacks and interleaves high 8-bit integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> UnpackHigh(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i * 2, left[8 + i]);
      result = result.WithElement(i * 2 + 1, right[8 + i]);
    }
    // Upper lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(16 + i * 2, left[24 + i]);
      result = result.WithElement(16 + i * 2 + 1, right[24 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 8-bit unsigned integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> UnpackHigh(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<byte>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(i * 2, left[8 + i]);
      result = result.WithElement(i * 2 + 1, right[8 + i]);
    }
    // Upper lane
    for (var i = 0; i < 8; ++i) {
      result = result.WithElement(16 + i * 2, left[24 + i]);
      result = result.WithElement(16 + i * 2 + 1, right[24 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 16-bit integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> UnpackHigh(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i * 2, left[4 + i]);
      result = result.WithElement(i * 2 + 1, right[4 + i]);
    }
    // Upper lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(8 + i * 2, left[12 + i]);
      result = result.WithElement(8 + i * 2 + 1, right[12 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 16-bit unsigned integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> UnpackHigh(Vector256<ushort> left, Vector256<ushort> right) {
    var result = Vector256<ushort>.Zero;
    // Lower lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i * 2, left[4 + i]);
      result = result.WithElement(i * 2 + 1, right[4 + i]);
    }
    // Upper lane
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(8 + i * 2, left[12 + i]);
      result = result.WithElement(8 + i * 2 + 1, right[12 + i]);
    }
    return result;
  }

  /// <summary>Unpacks and interleaves high 32-bit integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> UnpackHigh(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    // Lower lane
    result = result.WithElement(0, left[2]);
    result = result.WithElement(1, right[2]);
    result = result.WithElement(2, left[3]);
    result = result.WithElement(3, right[3]);
    // Upper lane
    result = result.WithElement(4, left[6]);
    result = result.WithElement(5, right[6]);
    result = result.WithElement(6, left[7]);
    result = result.WithElement(7, right[7]);
    return result;
  }

  /// <summary>Unpacks and interleaves high 32-bit unsigned integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> UnpackHigh(Vector256<uint> left, Vector256<uint> right) {
    var result = Vector256<uint>.Zero;
    // Lower lane
    result = result.WithElement(0, left[2]);
    result = result.WithElement(1, right[2]);
    result = result.WithElement(2, left[3]);
    result = result.WithElement(3, right[3]);
    // Upper lane
    result = result.WithElement(4, left[6]);
    result = result.WithElement(5, right[6]);
    result = result.WithElement(6, left[7]);
    result = result.WithElement(7, right[7]);
    return result;
  }

  /// <summary>Unpacks and interleaves high 64-bit integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> UnpackHigh(Vector256<long> left, Vector256<long> right) {
    var result = Vector256<long>.Zero;
    result = result.WithElement(0, left[1]);
    result = result.WithElement(1, right[1]);
    result = result.WithElement(2, left[3]);
    result = result.WithElement(3, right[3]);
    return result;
  }

  /// <summary>Unpacks and interleaves high 64-bit unsigned integers from high halves of each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> UnpackHigh(Vector256<ulong> left, Vector256<ulong> right) {
    var result = Vector256<ulong>.Zero;
    result = result.WithElement(0, left[1]);
    result = result.WithElement(1, right[1]);
    result = result.WithElement(2, left[3]);
    result = result.WithElement(3, right[3]);
    return result;
  }

  #endregion

  #region Shuffle Operations

  /// <summary>Shuffles 8-bit integers within each 128-bit lane based on control mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Shuffle(Vector256<sbyte> value, Vector256<sbyte> mask) {
    var result = Vector256<sbyte>.Zero;
    // Lower lane
    for (var i = 0; i < 16; ++i) {
      var idx = mask[i];
      result = result.WithElement(i, (idx & 0x80) != 0 ? (sbyte)0 : value[idx & 0x0F]);
    }
    // Upper lane
    for (var i = 0; i < 16; ++i) {
      var idx = mask[16 + i];
      result = result.WithElement(16 + i, (idx & 0x80) != 0 ? (sbyte)0 : value[16 + (idx & 0x0F)]);
    }
    return result;
  }

  /// <summary>Shuffles 8-bit unsigned integers within each 128-bit lane based on control mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Shuffle(Vector256<byte> value, Vector256<byte> mask) {
    var result = Vector256<byte>.Zero;
    // Lower lane
    for (var i = 0; i < 16; ++i) {
      var idx = mask[i];
      result = result.WithElement(i, (idx & 0x80) != 0 ? (byte)0 : value[idx & 0x0F]);
    }
    // Upper lane
    for (var i = 0; i < 16; ++i) {
      var idx = mask[16 + i];
      result = result.WithElement(16 + i, (idx & 0x80) != 0 ? (byte)0 : value[16 + (idx & 0x0F)]);
    }
    return result;
  }

  /// <summary>Shuffles 32-bit integers using immediate control byte within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Shuffle(Vector256<int> value, byte control) {
    var result = Vector256<int>.Zero;
    // Lower lane
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    // Upper lane
    result = result.WithElement(4, value[4 + ((control >> 0) & 3)]);
    result = result.WithElement(5, value[4 + ((control >> 2) & 3)]);
    result = result.WithElement(6, value[4 + ((control >> 4) & 3)]);
    result = result.WithElement(7, value[4 + ((control >> 6) & 3)]);
    return result;
  }

  /// <summary>Shuffles 32-bit unsigned integers using immediate control byte within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Shuffle(Vector256<uint> value, byte control) {
    var result = Vector256<uint>.Zero;
    // Lower lane
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    // Upper lane
    result = result.WithElement(4, value[4 + ((control >> 0) & 3)]);
    result = result.WithElement(5, value[4 + ((control >> 2) & 3)]);
    result = result.WithElement(6, value[4 + ((control >> 4) & 3)]);
    result = result.WithElement(7, value[4 + ((control >> 6) & 3)]);
    return result;
  }

  /// <summary>Shuffles high 16-bit integers within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ShuffleHigh(Vector256<short> value, byte control) {
    var result = value;
    // Lower lane (indices 4-7)
    result = result.WithElement(4, value[4 + ((control >> 0) & 3)]);
    result = result.WithElement(5, value[4 + ((control >> 2) & 3)]);
    result = result.WithElement(6, value[4 + ((control >> 4) & 3)]);
    result = result.WithElement(7, value[4 + ((control >> 6) & 3)]);
    // Upper lane (indices 12-15)
    result = result.WithElement(12, value[12 + ((control >> 0) & 3)]);
    result = result.WithElement(13, value[12 + ((control >> 2) & 3)]);
    result = result.WithElement(14, value[12 + ((control >> 4) & 3)]);
    result = result.WithElement(15, value[12 + ((control >> 6) & 3)]);
    return result;
  }

  /// <summary>Shuffles high 16-bit unsigned integers within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> ShuffleHigh(Vector256<ushort> value, byte control) {
    var result = value;
    // Lower lane (indices 4-7)
    result = result.WithElement(4, value[4 + ((control >> 0) & 3)]);
    result = result.WithElement(5, value[4 + ((control >> 2) & 3)]);
    result = result.WithElement(6, value[4 + ((control >> 4) & 3)]);
    result = result.WithElement(7, value[4 + ((control >> 6) & 3)]);
    // Upper lane (indices 12-15)
    result = result.WithElement(12, value[12 + ((control >> 0) & 3)]);
    result = result.WithElement(13, value[12 + ((control >> 2) & 3)]);
    result = result.WithElement(14, value[12 + ((control >> 4) & 3)]);
    result = result.WithElement(15, value[12 + ((control >> 6) & 3)]);
    return result;
  }

  /// <summary>Shuffles low 16-bit integers within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ShuffleLow(Vector256<short> value, byte control) {
    var result = value;
    // Lower lane (indices 0-3)
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    // Upper lane (indices 8-11)
    result = result.WithElement(8, value[8 + ((control >> 0) & 3)]);
    result = result.WithElement(9, value[8 + ((control >> 2) & 3)]);
    result = result.WithElement(10, value[8 + ((control >> 4) & 3)]);
    result = result.WithElement(11, value[8 + ((control >> 6) & 3)]);
    return result;
  }

  /// <summary>Shuffles low 16-bit unsigned integers within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> ShuffleLow(Vector256<ushort> value, byte control) {
    var result = value;
    // Lower lane (indices 0-3)
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    // Upper lane (indices 8-11)
    result = result.WithElement(8, value[8 + ((control >> 0) & 3)]);
    result = result.WithElement(9, value[8 + ((control >> 2) & 3)]);
    result = result.WithElement(10, value[8 + ((control >> 4) & 3)]);
    result = result.WithElement(11, value[8 + ((control >> 6) & 3)]);
    return result;
  }

  #endregion

  #region Permute Operations

  /// <summary>Permutes 64-bit integers across the entire vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Permute4x64(Vector256<long> value, byte control) {
    var result = Vector256<long>.Zero;
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    return result;
  }

  /// <summary>Permutes 64-bit unsigned integers across the entire vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Permute4x64(Vector256<ulong> value, byte control) {
    var result = Vector256<ulong>.Zero;
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    return result;
  }

  /// <summary>Permutes 64-bit double values across the entire vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> Permute4x64(Vector256<double> value, byte control) {
    var result = Vector256<double>.Zero;
    result = result.WithElement(0, value[(control >> 0) & 3]);
    result = result.WithElement(1, value[(control >> 2) & 3]);
    result = result.WithElement(2, value[(control >> 4) & 3]);
    result = result.WithElement(3, value[(control >> 6) & 3]);
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Permute2x128(Vector256<sbyte> left, Vector256<sbyte> right, byte control) {
    var result = Vector256<sbyte>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    for (var i = 0; i < 16; ++i) {
      result = result.WithElement(i, zeroLow ? (sbyte)0 : srcLow[i]);
      result = result.WithElement(16 + i, zeroHigh ? (sbyte)0 : srcHigh[i]);
    }
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> Permute2x128(Vector256<byte> left, Vector256<byte> right, byte control) {
    var result = Vector256<byte>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    for (var i = 0; i < 16; ++i) {
      result = result.WithElement(i, zeroLow ? (byte)0 : srcLow[i]);
      result = result.WithElement(16 + i, zeroHigh ? (byte)0 : srcHigh[i]);
    }
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Permute2x128(Vector256<int> left, Vector256<int> right, byte control) {
    var result = Vector256<int>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i, zeroLow ? 0 : srcLow[i]);
      result = result.WithElement(4 + i, zeroHigh ? 0 : srcHigh[i]);
    }
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Permute2x128(Vector256<uint> left, Vector256<uint> right, byte control) {
    var result = Vector256<uint>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    for (var i = 0; i < 4; ++i) {
      result = result.WithElement(i, zeroLow ? 0u : srcLow[i]);
      result = result.WithElement(4 + i, zeroHigh ? 0u : srcHigh[i]);
    }
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> Permute2x128(Vector256<long> left, Vector256<long> right, byte control) {
    var result = Vector256<long>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    result = result.WithElement(0, zeroLow ? 0L : srcLow[0]);
    result = result.WithElement(1, zeroLow ? 0L : srcLow[1]);
    result = result.WithElement(2, zeroHigh ? 0L : srcHigh[0]);
    result = result.WithElement(3, zeroHigh ? 0L : srcHigh[1]);
    return result;
  }

  /// <summary>Permutes 128-bit lanes using immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> Permute2x128(Vector256<ulong> left, Vector256<ulong> right, byte control) {
    var result = Vector256<ulong>.Zero;
    var srcLow = (control & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var srcHigh = ((control >> 4) & 0x03) switch {
      0 => left.GetLower(),
      1 => left.GetUpper(),
      2 => right.GetLower(),
      _ => right.GetUpper()
    };
    var zeroLow = (control & 0x08) != 0;
    var zeroHigh = (control & 0x80) != 0;
    result = result.WithElement(0, zeroLow ? 0UL : srcLow[0]);
    result = result.WithElement(1, zeroLow ? 0UL : srcLow[1]);
    result = result.WithElement(2, zeroHigh ? 0UL : srcHigh[0]);
    result = result.WithElement(3, zeroHigh ? 0UL : srcHigh[1]);
    return result;
  }

  /// <summary>Permutes 32-bit integers using variable indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> PermuteVar8x32(Vector256<int> left, Vector256<int> control) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, left[control[i] & 7]);
    return result;
  }

  /// <summary>Permutes 32-bit unsigned integers using variable indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> PermuteVar8x32(Vector256<uint> left, Vector256<uint> control) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, left[(int)(control[i] & 7)]);
    return result;
  }

  /// <summary>Permutes 32-bit floats using variable indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> PermuteVar8x32(Vector256<float> left, Vector256<int> control) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, left[control[i] & 7]);
    return result;
  }

  #endregion

  #region Blend Operations

  /// <summary>Blends packed 8-bit signed integers based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> BlendVariable(Vector256<sbyte> left, Vector256<sbyte> right, Vector256<sbyte> mask) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      result = result.WithElement(i, mask[i] < 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 8-bit unsigned integers based on mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> BlendVariable(Vector256<byte> left, Vector256<byte> right, Vector256<byte> mask) {
    var result = Vector256<byte>.Zero;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      result = result.WithElement(i, (mask[i] & 0x80) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 16-bit signed integers based on immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Blend(Vector256<short> left, Vector256<short> right, byte control) {
    var result = Vector256<short>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    // Upper lane (same pattern)
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(8 + i, ((control >> i) & 1) != 0 ? right[8 + i] : left[8 + i]);
    return result;
  }

  /// <summary>Blends packed 16-bit unsigned integers based on immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> Blend(Vector256<ushort> left, Vector256<ushort> right, byte control) {
    var result = Vector256<ushort>.Zero;
    // Lower lane
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    // Upper lane (same pattern)
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(8 + i, ((control >> i) & 1) != 0 ? right[8 + i] : left[8 + i]);
    return result;
  }

  /// <summary>Blends packed 32-bit signed integers based on immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Blend(Vector256<int> left, Vector256<int> right, byte control) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    return result;
  }

  /// <summary>Blends packed 32-bit unsigned integers based on immediate control.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> Blend(Vector256<uint> left, Vector256<uint> right, byte control) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i)
      result = result.WithElement(i, ((control >> i) & 1) != 0 ? right[i] : left[i]);
    return result;
  }

  #endregion

  #region Broadcast Operations

  /// <summary>Broadcasts 8-bit integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> BroadcastScalarToVector256(Vector128<sbyte> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 8-bit unsigned integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> BroadcastScalarToVector256(Vector128<byte> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 16-bit integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> BroadcastScalarToVector256(Vector128<short> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 16-bit unsigned integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> BroadcastScalarToVector256(Vector128<ushort> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 32-bit integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> BroadcastScalarToVector256(Vector128<int> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 32-bit unsigned integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> BroadcastScalarToVector256(Vector128<uint> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 64-bit integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> BroadcastScalarToVector256(Vector128<long> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 64-bit unsigned integer to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> BroadcastScalarToVector256(Vector128<ulong> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 32-bit float to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> BroadcastScalarToVector256(Vector128<float> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 64-bit double to all elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> BroadcastScalarToVector256(Vector128<double> value)
    => Vector256.Create(value[0]);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> BroadcastVector128ToVector256(Vector128<sbyte> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> BroadcastVector128ToVector256(Vector128<byte> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> BroadcastVector128ToVector256(Vector128<short> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> BroadcastVector128ToVector256(Vector128<ushort> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> BroadcastVector128ToVector256(Vector128<int> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> BroadcastVector128ToVector256(Vector128<uint> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> BroadcastVector128ToVector256(Vector128<long> value)
    => Vector256.Create(value, value);

  /// <summary>Broadcasts 128-bit vector to both lanes of 256-bit result.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> BroadcastVector128ToVector256(Vector128<ulong> value)
    => Vector256.Create(value, value);

  #endregion

  #region Extract/Insert Operations

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<sbyte> ExtractVector128(Vector256<sbyte> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<byte> ExtractVector128(Vector256<byte> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<short> ExtractVector128(Vector256<short> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ushort> ExtractVector128(Vector256<ushort> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<int> ExtractVector128(Vector256<int> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<uint> ExtractVector128(Vector256<uint> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<long> ExtractVector128(Vector256<long> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Extracts 128-bit lane from 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<ulong> ExtractVector128(Vector256<ulong> value, byte index)
    => (index & 1) == 0 ? value.GetLower() : value.GetUpper();

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> InsertVector128(Vector256<sbyte> value, Vector128<sbyte> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> InsertVector128(Vector256<byte> value, Vector128<byte> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> InsertVector128(Vector256<short> value, Vector128<short> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> InsertVector128(Vector256<ushort> value, Vector128<ushort> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> InsertVector128(Vector256<int> value, Vector128<int> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<uint> InsertVector128(Vector256<uint> value, Vector128<uint> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> InsertVector128(Vector256<long> value, Vector128<long> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  /// <summary>Inserts 128-bit vector into specified lane of 256-bit vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ulong> InsertVector128(Vector256<ulong> value, Vector128<ulong> data, byte index)
    => (index & 1) == 0 ? Vector256.WithLower(value, data) : Vector256.WithUpper(value, data);

  #endregion

  #region Conversion Operations

  /// <summary>Converts packed 8-bit signed integers to packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ConvertToVector256Int16(Vector128<sbyte> value) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector128<sbyte>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 8-bit unsigned integers to packed 16-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> ConvertToVector256Int16(Vector128<byte> value) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector128<byte>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 8-bit signed integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32(Vector128<sbyte> value) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 8-bit unsigned integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32(Vector128<byte> value) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < 8; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 16-bit signed integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32(Vector128<short> value) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector128<short>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 16-bit unsigned integers to packed 32-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> ConvertToVector256Int32(Vector128<ushort> value) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector128<ushort>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 8-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<sbyte> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 8-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<byte> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 16-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<short> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 16-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<ushort> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < 4; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 32-bit signed integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<int> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector128<int>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  /// <summary>Converts packed 32-bit unsigned integers to packed 64-bit signed integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<long> ConvertToVector256Int64(Vector128<uint> value) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector128<uint>.Count; ++i)
      result = result.WithElement(i, value[i]);
    return result;
  }

  #endregion

  #region MoveMask Operations

  /// <summary>Creates a mask from the most significant bit of each 8-bit element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector256<sbyte> value) {
    var result = 0;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i)
      if (value[i] < 0)
        result |= 1 << i;
    return result;
  }

  /// <summary>Creates a mask from the most significant bit of each 8-bit unsigned element.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int MoveMask(Vector256<byte> value) {
    var result = 0;
    for (var i = 0; i < Vector256<byte>.Count; ++i)
      if ((value[i] & 0x80) != 0)
        result |= 1 << i;
    return result;
  }

  #endregion

  #region Sign Operations

  /// <summary>Negates, zeros, or preserves 8-bit signed integers based on sign mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> Sign(Vector256<sbyte> left, Vector256<sbyte> right) {
    var result = Vector256<sbyte>.Zero;
    for (var i = 0; i < Vector256<sbyte>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, (sbyte)-left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
    }
    return result;
  }

  /// <summary>Negates, zeros, or preserves 16-bit signed integers based on sign mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<short> Sign(Vector256<short> left, Vector256<short> right) {
    var result = Vector256<short>.Zero;
    for (var i = 0; i < Vector256<short>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, (short)-left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
    }
    return result;
  }

  /// <summary>Negates, zeros, or preserves 32-bit signed integers based on sign mask.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<int> Sign(Vector256<int> left, Vector256<int> right) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      if (right[i] < 0)
        result = result.WithElement(i, -left[i]);
      else if (right[i] > 0)
        result = result.WithElement(i, left[i]);
    }
    return result;
  }

  #endregion

  #region Sum of Absolute Differences

  /// <summary>Computes sum of absolute differences of 8-bit unsigned integers.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> SumAbsoluteDifferences(Vector256<byte> left, Vector256<byte> right) {
    var result = Vector256<ushort>.Zero;
    // Each 64-bit lane produces one sum
    for (var lane = 0; lane < 4; ++lane) {
      var sum = 0;
      for (var i = 0; i < 8; ++i) {
        var idx = lane * 8 + i;
        sum += Math.Abs(left[idx] - right[idx]);
      }
      result = result.WithElement(lane * 4, (ushort)sum);
    }
    return result;
  }

  #endregion

  #region Gather Operations

  /// <summary>Gathers 32-bit signed integers from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<int> GatherVector256(int* baseAddress, Vector256<int> index, byte scale) {
    var result = Vector256<int>.Zero;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      var address = (int*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 32-bit unsigned integers from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<uint> GatherVector256(uint* baseAddress, Vector256<int> index, byte scale) {
    var result = Vector256<uint>.Zero;
    for (var i = 0; i < Vector256<uint>.Count; ++i) {
      var address = (uint*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit signed integers from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<long> GatherVector256(long* baseAddress, Vector128<int> index, byte scale) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var address = (long*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit unsigned integers from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<ulong> GatherVector256(ulong* baseAddress, Vector128<int> index, byte scale) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i) {
      var address = (ulong*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 32-bit floats from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<float> GatherVector256(float* baseAddress, Vector256<int> index, byte scale) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i) {
      var address = (float*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit doubles from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> GatherVector256(double* baseAddress, Vector128<int> index, byte scale) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i) {
      var address = (double*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit signed integers from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<long> GatherVector256(long* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector256<long>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var address = (long*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit unsigned integers from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<ulong> GatherVector256(ulong* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector256<ulong>.Zero;
    for (var i = 0; i < Vector256<ulong>.Count; ++i) {
      var address = (ulong*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 64-bit doubles from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> GatherVector256(double* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i) {
      var address = (double*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 32-bit signed integers from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<int> GatherVector128(int* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector128<int>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var address = (int*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 32-bit unsigned integers from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<uint> GatherVector128(uint* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector128<uint>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var address = (uint*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  /// <summary>Gathers 32-bit floats from memory using 64-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<float> GatherVector128(float* baseAddress, Vector256<long> index, byte scale) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      var address = (float*)((byte*)baseAddress + index[i] * scale);
      result = result.WithElement(i, *address);
    }
    return result;
  }

  #endregion

  #region Masked Gather Operations

  /// <summary>Gathers 32-bit signed integers with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<int> GatherMaskVector256(Vector256<int> source, int* baseAddress, Vector256<int> index, Vector256<int> mask, byte scale) {
    var result = source;
    for (var i = 0; i < Vector256<int>.Count; ++i) {
      if (mask[i] < 0) {
        var address = (int*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  /// <summary>Gathers 32-bit unsigned integers with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<uint> GatherMaskVector256(Vector256<uint> source, uint* baseAddress, Vector256<int> index, Vector256<uint> mask, byte scale) {
    var result = source;
    for (var i = 0; i < Vector256<uint>.Count; ++i) {
      if ((mask[i] & 0x80000000) != 0) {
        var address = (uint*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  /// <summary>Gathers 64-bit signed integers with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<long> GatherMaskVector256(Vector256<long> source, long* baseAddress, Vector128<int> index, Vector256<long> mask, byte scale) {
    var result = source;
    for (var i = 0; i < Vector256<long>.Count; ++i) {
      if (mask[i] < 0) {
        var address = (long*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  /// <summary>Gathers 64-bit unsigned integers with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<ulong> GatherMaskVector256(Vector256<ulong> source, ulong* baseAddress, Vector128<int> index, Vector256<ulong> mask, byte scale) {
    var result = source;
    for (var i = 0; i < Vector256<ulong>.Count; ++i) {
      if ((mask[i] & 0x8000000000000000) != 0) {
        var address = (ulong*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  /// <summary>Gathers 32-bit floats with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<float> GatherMaskVector256(Vector256<float> source, float* baseAddress, Vector256<int> index, Vector256<float> mask, byte scale) {
    var result = source;
    var maskInt = Vector256.As<float, int>(mask);
    for (var i = 0; i < Vector256<float>.Count; ++i) {
      if (maskInt[i] < 0) {
        var address = (float*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  /// <summary>Gathers 64-bit doubles with mask from memory using 32-bit signed indices.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector256<double> GatherMaskVector256(Vector256<double> source, double* baseAddress, Vector128<int> index, Vector256<double> mask, byte scale) {
    var result = source;
    var maskLong = Vector256.As<double, long>(mask);
    for (var i = 0; i < Vector256<double>.Count; ++i) {
      if (maskLong[i] < 0) {
        var address = (double*)((byte*)baseAddress + index[i] * scale);
        result = result.WithElement(i, *address);
      }
    }
    return result;
  }

  #endregion

  #region Align Right Operations

  /// <summary>Concatenates and shifts right by specified byte count within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<sbyte> AlignRight(Vector256<sbyte> left, Vector256<sbyte> right, byte mask) {
    if (mask >= 32)
      return Vector256<sbyte>.Zero;

    var result = Vector256<sbyte>.Zero;
    var count = mask & 0x1F;

    // Process each 128-bit lane
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      if (count >= 16) {
        // Shift only from left
        var shift = count - 16;
        for (var i = 0; i < 16 - shift; ++i)
          result = result.WithElement(laneOffset + i, left[laneOffset + shift + i]);
      } else {
        // Concatenate and shift
        for (var i = 0; i < 16; ++i) {
          var srcIdx = i + count;
          if (srcIdx < 16)
            result = result.WithElement(laneOffset + i, right[laneOffset + srcIdx]);
          else
            result = result.WithElement(laneOffset + i, left[laneOffset + srcIdx - 16]);
        }
      }
    }
    return result;
  }

  /// <summary>Concatenates and shifts right by specified byte count within each 128-bit lane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<byte> AlignRight(Vector256<byte> left, Vector256<byte> right, byte mask) {
    if (mask >= 32)
      return Vector256<byte>.Zero;

    var result = Vector256<byte>.Zero;
    var count = mask & 0x1F;

    // Process each 128-bit lane
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      if (count >= 16) {
        // Shift only from left
        var shift = count - 16;
        for (var i = 0; i < 16 - shift; ++i)
          result = result.WithElement(laneOffset + i, left[laneOffset + shift + i]);
      } else {
        // Concatenate and shift
        for (var i = 0; i < 16; ++i) {
          var srcIdx = i + count;
          if (srcIdx < 16)
            result = result.WithElement(laneOffset + i, right[laneOffset + srcIdx]);
          else
            result = result.WithElement(laneOffset + i, left[laneOffset + srcIdx - 16]);
        }
      }
    }
    return result;
  }

  #endregion

  #region Multiple Sum of Absolute Differences

  /// <summary>Computes multiple sums of absolute differences.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<ushort> MultipleSumAbsoluteDifferences(Vector256<byte> left, Vector256<byte> right, byte mask) {
    var result = Vector256<ushort>.Zero;

    // Process each 128-bit lane
    for (var lane = 0; lane < 2; ++lane) {
      var laneOffset = lane * 16;
      var rightOffset = ((mask >> (lane * 2)) & 3) * 4;

      for (var block = 0; block < 8; ++block) {
        var sum = 0;
        for (var i = 0; i < 4; ++i)
          sum += Math.Abs(left[laneOffset + block + i] - right[laneOffset + rightOffset + i]);
        result = result.WithElement(lane * 8 + block, (ushort)sum);
      }
    }

    return result;
  }

  #endregion

  /// <summary>Provides 64-bit specific AVX2 operations.</summary>
  public new abstract class X64 : Avx.X64 {

    /// <summary>Gets a value indicating whether 64-bit AVX2 instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif
