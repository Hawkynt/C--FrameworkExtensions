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

#if !OFFICIAL_TENSORS

using System.Runtime.CompilerServices;

namespace System.Numerics.Tensors;

public static partial class Tensor {

  /// <summary>
  /// Validates that two tensor shapes match exactly.
  /// </summary>
  private static void _ValidateShapesMatch(ReadOnlySpan<nint> a, ReadOnlySpan<nint> b) {
    if (a.Length != b.Length)
      throw new ArgumentException($"Tensor ranks must match: {a.Length} vs {b.Length}");

    for (var i = 0; i < a.Length; ++i)
      if (a[i] != b[i])
        throw new ArgumentException($"Tensor shapes must match at dimension {i}: {a[i]} vs {b[i]}");
  }

  /// <summary>
  /// Validates that the destination shape matches the source shape.
  /// </summary>
  private static void _ValidateDestinationShape(ReadOnlySpan<nint> source, ReadOnlySpan<nint> destination) {
    if (destination.Length != source.Length)
      throw new ArgumentException($"Destination rank {destination.Length} does not match source rank {source.Length}");

    for (var i = 0; i < source.Length; ++i)
      if (destination[i] < source[i])
        throw new ArgumentException($"Destination dimension {i} ({destination[i]}) is smaller than source ({source[i]})");
  }

  /// <summary>
  /// Validates that the destination has sufficient capacity for the given flattened length.
  /// </summary>
  private static void _ValidateDestinationCapacity(nint requiredLength, nint actualLength) {
    if (actualLength < requiredLength)
      throw new ArgumentException($"Destination capacity {actualLength} is smaller than required {requiredLength}");
  }

  /// <summary>
  /// Densifies a tensor span to a contiguous span. If already dense, returns the underlying span directly.
  /// </summary>
  private static T[] _DensifyToArray<T>(in ReadOnlyTensorSpan<T> tensor) {
    var length = (int)_ComputeFlatLength(tensor.Lengths);
    var buffer = new T[length];
    tensor.FlattenTo(buffer);
    return buffer;
  }

  /// <summary>
  /// Gets the flat span from a tensor span, densifying if necessary.
  /// </summary>
  private static ReadOnlySpan<T> _GetFlatSpan<T>(in ReadOnlyTensorSpan<T> tensor)
    => tensor.IsDense ? tensor._GetSpan() : _DensifyToArray(tensor);

  /// <summary>
  /// Gets the flat span from a mutable tensor span, densifying if necessary.
  /// </summary>
  private static Span<T> _GetFlatSpan<T>(in TensorSpan<T> tensor)
    => tensor.IsDense ? tensor._GetSpan() : throw new InvalidOperationException("Cannot get mutable span from non-dense tensor");

  /// <summary>
  /// Computes the strides for a row-major tensor layout.
  /// </summary>
  private static nint[] _ComputeStrides(ReadOnlySpan<nint> lengths) {
    if (lengths.IsEmpty)
      return [];

    var strides = new nint[lengths.Length];
    nint stride = 1;
    for (var i = lengths.Length - 1; i >= 0; --i) {
      strides[i] = stride;
      stride *= lengths[i];
    }

    return strides;
  }

  /// <summary>
  /// Copies the lengths array.
  /// </summary>
  private static nint[] _CopyLengths(ReadOnlySpan<nint> lengths) => lengths.ToArray();

  #region Bitwise Helpers

  /// <summary>
  /// Performs element-wise bitwise AND on two spans.
  /// </summary>
  private static unsafe void _BitwiseAndUnconstrained<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    for (var i = 0; i < x.Length; ++i) {
      var left = x[i];
      var right = y[i];
      var result = default(T)!;
      var pLeft = (byte*)Unsafe.AsPointer(ref left);
      var pRight = (byte*)Unsafe.AsPointer(ref right);
      var pResult = (byte*)Unsafe.AsPointer(ref result);
      for (var j = 0; j < size; ++j)
        pResult[j] = (byte)(pLeft[j] & pRight[j]);
      destination[i] = result;
    }
  }

  /// <summary>
  /// Performs element-wise bitwise OR on two spans.
  /// </summary>
  private static unsafe void _BitwiseOrUnconstrained<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    for (var i = 0; i < x.Length; ++i) {
      var left = x[i];
      var right = y[i];
      var result = default(T)!;
      var pLeft = (byte*)Unsafe.AsPointer(ref left);
      var pRight = (byte*)Unsafe.AsPointer(ref right);
      var pResult = (byte*)Unsafe.AsPointer(ref result);
      for (var j = 0; j < size; ++j)
        pResult[j] = (byte)(pLeft[j] | pRight[j]);
      destination[i] = result;
    }
  }

  /// <summary>
  /// Performs element-wise bitwise XOR on two spans.
  /// </summary>
  private static unsafe void _BitwiseXorUnconstrained<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    for (var i = 0; i < x.Length; ++i) {
      var left = x[i];
      var right = y[i];
      var result = default(T)!;
      var pLeft = (byte*)Unsafe.AsPointer(ref left);
      var pRight = (byte*)Unsafe.AsPointer(ref right);
      var pResult = (byte*)Unsafe.AsPointer(ref result);
      for (var j = 0; j < size; ++j)
        pResult[j] = (byte)(pLeft[j] ^ pRight[j]);
      destination[i] = result;
    }
  }

  /// <summary>
  /// Performs element-wise bitwise NOT on a span.
  /// </summary>
  private static unsafe void _OnesComplementUnconstrained<T>(ReadOnlySpan<T> x, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    for (var i = 0; i < x.Length; ++i) {
      var value = x[i];
      var result = default(T)!;
      var pValue = (byte*)Unsafe.AsPointer(ref value);
      var pResult = (byte*)Unsafe.AsPointer(ref result);
      for (var j = 0; j < size; ++j)
        pResult[j] = (byte)~pValue[j];
      destination[i] = result;
    }
  }

  /// <summary>
  /// Shifts each element left by the specified amount.
  /// </summary>
  private static unsafe void _ShiftLeftUnconstrained<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    var bitSize = size * 8;
    shiftAmount %= bitSize;
    if (shiftAmount < 0)
      shiftAmount += bitSize;

    for (var i = 0; i < x.Length; ++i) {
      var value = x[i];
      var result = default(T)!;
      var pValue = (byte*)Unsafe.AsPointer(ref value);
      var pResult = (byte*)Unsafe.AsPointer(ref result);

      // Shift implementation based on size
      switch (size) {
        case 1:
          *(byte*)pResult = (byte)(*(byte*)pValue << shiftAmount);
          break;
        case 2:
          *(ushort*)pResult = (ushort)(*(ushort*)pValue << shiftAmount);
          break;
        case 4:
          *(uint*)pResult = *(uint*)pValue << shiftAmount;
          break;
        case 8:
          *(ulong*)pResult = *(ulong*)pValue << shiftAmount;
          break;
        default:
          throw new NotSupportedException($"Shift operations not supported for size {size}");
      }
      destination[i] = result;
    }
  }

  /// <summary>
  /// Shifts each element right arithmetically by the specified amount.
  /// </summary>
  private static unsafe void _ShiftRightArithmeticUnconstrained<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    var bitSize = size * 8;
    shiftAmount %= bitSize;
    if (shiftAmount < 0)
      shiftAmount += bitSize;

    for (var i = 0; i < x.Length; ++i) {
      var value = x[i];
      var result = default(T)!;
      var pValue = (byte*)Unsafe.AsPointer(ref value);
      var pResult = (byte*)Unsafe.AsPointer(ref result);

      switch (size) {
        case 1:
          *(sbyte*)pResult = (sbyte)(*(sbyte*)pValue >> shiftAmount);
          break;
        case 2:
          *(short*)pResult = (short)(*(short*)pValue >> shiftAmount);
          break;
        case 4:
          *(int*)pResult = *(int*)pValue >> shiftAmount;
          break;
        case 8:
          *(long*)pResult = *(long*)pValue >> shiftAmount;
          break;
        default:
          throw new NotSupportedException($"Shift operations not supported for size {size}");
      }
      destination[i] = result;
    }
  }

  /// <summary>
  /// Shifts each element right logically by the specified amount.
  /// </summary>
  private static unsafe void _ShiftRightLogicalUnconstrained<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    var size = Unsafe.SizeOf<T>();
    var bitSize = size * 8;
    shiftAmount %= bitSize;
    if (shiftAmount < 0)
      shiftAmount += bitSize;

    for (var i = 0; i < x.Length; ++i) {
      var value = x[i];
      var result = default(T)!;
      var pValue = (byte*)Unsafe.AsPointer(ref value);
      var pResult = (byte*)Unsafe.AsPointer(ref result);

      switch (size) {
        case 1:
          *(byte*)pResult = (byte)(*(byte*)pValue >> shiftAmount);
          break;
        case 2:
          *(ushort*)pResult = (ushort)(*(ushort*)pValue >> shiftAmount);
          break;
        case 4:
          *(uint*)pResult = *(uint*)pValue >> shiftAmount;
          break;
        case 8:
          *(ulong*)pResult = *(ulong*)pValue >> shiftAmount;
          break;
        default:
          throw new NotSupportedException($"Shift operations not supported for size {size}");
      }
      destination[i] = result;
    }
  }

  #endregion

  #region Bit Counting Helpers

  /// <summary>
  /// Computes the integer log base 2 of a value.
  /// For floating-point types, extracts the exponent from IEEE 754 representation.
  /// For integer types, computes floor(log2(value)).
  /// </summary>
  private static unsafe int _ILogB<T>(T value) {
    // Check for floating-point types first
    if (typeof(T) == typeof(float)) {
      var f = *(float*)Unsafe.AsPointer(ref value);
      return MathF.ILogB(f);
    }
    if (typeof(T) == typeof(double)) {
      var d = *(double*)Unsafe.AsPointer(ref value);
      return Math.ILogB(d);
    }
    if (typeof(T) == typeof(Half)) {
      var h = *(Half*)Unsafe.AsPointer(ref value);
      return _ILogBHalf(h);
    }

    // For integer types, compute floor(log2(value))
    var size = Unsafe.SizeOf<T>();
    var pValue = (byte*)Unsafe.AsPointer(ref value);

    return size switch {
      1 => _ILogByte(*(byte*)pValue),
      2 => _ILogBUShort(*(ushort*)pValue),
      4 => _ILogBUInt(*(uint*)pValue),
      8 => _ILogBULong(*(ulong*)pValue),
      _ => throw new NotSupportedException($"ILogB not supported for size {size}")
    };
  }

  private static int _ILogBHalf(Half value) {
    // For Half, extract exponent from IEEE 754 half-precision format
    var bits = BitConverter.HalfToUInt16Bits(value);
    var biasedExp = (int)((bits >> 10) & 0x1F);
    if (biasedExp == 0) return int.MinValue; // Zero or subnormal
    if (biasedExp == 31) return int.MaxValue; // Inf or NaN
    return biasedExp - 15; // Unbias the exponent (bias is 15 for half)
  }

  private static int _ILogByte(byte value) => value == 0 ? int.MinValue : 7 - _LeadingZeroByte(value);
  private static int _ILogBUShort(ushort value) => value == 0 ? int.MinValue : 15 - _LeadingZeroUShort(value);
  private static int _ILogBUInt(uint value) => value == 0 ? int.MinValue : 31 - _LeadingZeroUInt(value);
  private static int _ILogBULong(ulong value) => value == 0 ? int.MinValue : 63 - _LeadingZeroULong(value);

  /// <summary>
  /// Counts the number of set bits in a value.
  /// </summary>
  private static unsafe T _PopCount<T>(T value) {
    var size = Unsafe.SizeOf<T>();
    var pValue = (byte*)Unsafe.AsPointer(ref value);

    var count = size switch {
      1 => _PopCountByte(*(byte*)pValue),
      2 => _PopCountUShort(*(ushort*)pValue),
      4 => _PopCountUInt(*(uint*)pValue),
      8 => _PopCountULong(*(ulong*)pValue),
      _ => throw new NotSupportedException($"PopCount not supported for size {size}")
    };

    var result = default(T)!;
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    switch (size) {
      case 1: *(byte*)pResult = (byte)count; break;
      case 2: *(ushort*)pResult = (ushort)count; break;
      case 4: *(uint*)pResult = (uint)count; break;
      case 8: *(ulong*)pResult = (ulong)count; break;
    }
    return result;
  }

  private static int _PopCountByte(byte value) {
    var v = (uint)value;
    v = v - ((v >> 1) & 0x55);
    v = (v & 0x33) + ((v >> 2) & 0x33);
    return (int)((v + (v >> 4)) & 0x0F);
  }

  private static int _PopCountUShort(ushort value) {
    var v = (uint)value;
    v = v - ((v >> 1) & 0x5555);
    v = (v & 0x3333) + ((v >> 2) & 0x3333);
    v = (v + (v >> 4)) & 0x0F0F;
    return (int)((v * 0x0101) >> 8);
  }

  private static int _PopCountUInt(uint value) {
    var v = value;
    v = v - ((v >> 1) & 0x55555555);
    v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
    v = (v + (v >> 4)) & 0x0F0F0F0F;
    return (int)((v * 0x01010101) >> 24);
  }

  private static int _PopCountULong(ulong value) {
    var v = value;
    v = v - ((v >> 1) & 0x5555555555555555UL);
    v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
    v = (v + (v >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
    return (int)((v * 0x0101010101010101UL) >> 56);
  }

  /// <summary>
  /// Counts the number of leading zeros in a value.
  /// </summary>
  private static unsafe T _LeadingZeroCount<T>(T value) {
    var size = Unsafe.SizeOf<T>();
    var pValue = (byte*)Unsafe.AsPointer(ref value);

    var count = size switch {
      1 => _LeadingZeroByte(*(byte*)pValue),
      2 => _LeadingZeroUShort(*(ushort*)pValue),
      4 => _LeadingZeroUInt(*(uint*)pValue),
      8 => _LeadingZeroULong(*(ulong*)pValue),
      _ => throw new NotSupportedException($"LeadingZeroCount not supported for size {size}")
    };

    var result = default(T)!;
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    switch (size) {
      case 1: *(byte*)pResult = (byte)count; break;
      case 2: *(ushort*)pResult = (ushort)count; break;
      case 4: *(uint*)pResult = (uint)count; break;
      case 8: *(ulong*)pResult = (ulong)count; break;
    }
    return result;
  }

  private static int _LeadingZeroByte(byte value) {
    if (value == 0) return 8;
    var n = 0;
    if ((value & 0xF0) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC0) == 0) { n += 2; value <<= 2; }
    if ((value & 0x80) == 0) { n += 1; }
    return n;
  }

  private static int _LeadingZeroUShort(ushort value) {
    if (value == 0) return 16;
    var n = 0;
    if ((value & 0xFF00) == 0) { n += 8; value <<= 8; }
    if ((value & 0xF000) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC000) == 0) { n += 2; value <<= 2; }
    if ((value & 0x8000) == 0) { n += 1; }
    return n;
  }

  private static int _LeadingZeroUInt(uint value) {
    if (value == 0) return 32;
    var n = 0;
    if ((value & 0xFFFF0000) == 0) { n += 16; value <<= 16; }
    if ((value & 0xFF000000) == 0) { n += 8; value <<= 8; }
    if ((value & 0xF0000000) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC0000000) == 0) { n += 2; value <<= 2; }
    if ((value & 0x80000000) == 0) { n += 1; }
    return n;
  }

  private static int _LeadingZeroULong(ulong value) {
    if (value == 0) return 64;
    var n = 0;
    if ((value & 0xFFFFFFFF00000000UL) == 0) { n += 32; value <<= 32; }
    if ((value & 0xFFFF000000000000UL) == 0) { n += 16; value <<= 16; }
    if ((value & 0xFF00000000000000UL) == 0) { n += 8; value <<= 8; }
    if ((value & 0xF000000000000000UL) == 0) { n += 4; value <<= 4; }
    if ((value & 0xC000000000000000UL) == 0) { n += 2; value <<= 2; }
    if ((value & 0x8000000000000000UL) == 0) { n += 1; }
    return n;
  }

  /// <summary>
  /// Counts the number of trailing zeros in a value.
  /// </summary>
  private static unsafe T _TrailingZeroCount<T>(T value) {
    var size = Unsafe.SizeOf<T>();
    var pValue = (byte*)Unsafe.AsPointer(ref value);

    var count = size switch {
      1 => _TrailingZeroByte(*(byte*)pValue),
      2 => _TrailingZeroUShort(*(ushort*)pValue),
      4 => _TrailingZeroUInt(*(uint*)pValue),
      8 => _TrailingZeroULong(*(ulong*)pValue),
      _ => throw new NotSupportedException($"TrailingZeroCount not supported for size {size}")
    };

    var result = default(T)!;
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    switch (size) {
      case 1: *(byte*)pResult = (byte)count; break;
      case 2: *(ushort*)pResult = (ushort)count; break;
      case 4: *(uint*)pResult = (uint)count; break;
      case 8: *(ulong*)pResult = (ulong)count; break;
    }
    return result;
  }

  private static int _TrailingZeroByte(byte value) {
    if (value == 0) return 8;
    var n = 0;
    if ((value & 0x0F) == 0) { n += 4; value >>= 4; }
    if ((value & 0x03) == 0) { n += 2; value >>= 2; }
    if ((value & 0x01) == 0) { n += 1; }
    return n;
  }

  private static int _TrailingZeroUShort(ushort value) {
    if (value == 0) return 16;
    var n = 0;
    if ((value & 0x00FF) == 0) { n += 8; value >>= 8; }
    if ((value & 0x000F) == 0) { n += 4; value >>= 4; }
    if ((value & 0x0003) == 0) { n += 2; value >>= 2; }
    if ((value & 0x0001) == 0) { n += 1; }
    return n;
  }

  private static int _TrailingZeroUInt(uint value) {
    if (value == 0) return 32;
    var n = 0;
    if ((value & 0x0000FFFF) == 0) { n += 16; value >>= 16; }
    if ((value & 0x000000FF) == 0) { n += 8; value >>= 8; }
    if ((value & 0x0000000F) == 0) { n += 4; value >>= 4; }
    if ((value & 0x00000003) == 0) { n += 2; value >>= 2; }
    if ((value & 0x00000001) == 0) { n += 1; }
    return n;
  }

  private static int _TrailingZeroULong(ulong value) {
    if (value == 0) return 64;
    var n = 0;
    if ((value & 0x00000000FFFFFFFFUL) == 0) { n += 32; value >>= 32; }
    if ((value & 0x000000000000FFFFUL) == 0) { n += 16; value >>= 16; }
    if ((value & 0x00000000000000FFUL) == 0) { n += 8; value >>= 8; }
    if ((value & 0x000000000000000FUL) == 0) { n += 4; value >>= 4; }
    if ((value & 0x0000000000000003UL) == 0) { n += 2; value >>= 2; }
    if ((value & 0x0000000000000001UL) == 0) { n += 1; }
    return n;
  }

  /// <summary>
  /// Converts a double value to the target type.
  /// </summary>
  private static unsafe T _FromDouble<T>(double value) {
    var size = Unsafe.SizeOf<T>();
    var result = default(T)!;
    var pResult = (byte*)Unsafe.AsPointer(ref result);

    switch (size) {
      case 1:
        *(byte*)pResult = (byte)value;
        break;
      case 2:
        *(ushort*)pResult = (ushort)value;
        break;
      case 4:
        // Could be int, uint, or float
        if (typeof(T) == typeof(float))
          *(float*)pResult = (float)value;
        else
          *(uint*)pResult = (uint)value;
        break;
      case 8:
        // Could be long, ulong, or double
        if (typeof(T) == typeof(double))
          *(double*)pResult = value;
        else
          *(ulong*)pResult = (ulong)value;
        break;
      default:
        throw new NotSupportedException($"FromDouble not supported for size {size}");
    }
    return result;
  }

  #endregion

}

#endif
