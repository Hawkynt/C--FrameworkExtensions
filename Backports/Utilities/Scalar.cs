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

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Utilities;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

internal static class Scalar<T> {

  public static bool IsUnsigned {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => TypeCodeCache<T>.Code switch {
      CachedTypeCode.Byte or CachedTypeCode.UInt16 or CachedTypeCode.UInt32 or CachedTypeCode.UInt64 or CachedTypeCode.UPointer or CachedTypeCode.UInt128 => true,
      _ => false
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ExtractMostSignificantBit(T value) => Unsafe.SizeOf<T>() switch {
    1 => (As<byte>(value) & 0x80) != 0,
    2 => (As<ushort>(value) & 0x8000) != 0,
    4 => (As<uint>(value) & 0x80000000) != 0,
    8 => (As<ulong>(value) & 0x8000000000000000) != 0,
    16 => (_GetUpper(As<UInt128>(value)) & 0x8000000000000000) != 0,
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ExtractMostSignificantBitValue(T value) {
    var size = Unsafe.SizeOf<T>();

    if (size == 16) {
      var v128 = As<UInt128>(value);
      var upper = _GetUpper(v128);
      var lower = _GetLower(v128);
      if (upper != 0) {
        for (var i = 63; i >= 0; --i) {
          var bit = 1UL << i;
          if ((upper & bit) != 0)
            return Promote(new UInt128(bit, 0));
        }
      }
      if (lower != 0) {
        for (var i = 63; i >= 0; --i) {
          var bit = 1UL << i;
          if ((lower & bit) != 0)
            return Promote(new UInt128(0, bit));
        }
      }
      return Zero();
    }

    var unsignedValue = size switch {
      1 => As<byte>(value),
      2 => As<ushort>(value),
      4 => As<uint>(value),
      8 => As<ulong>(value),
      _ => ThrowNotSupported<ulong>()
    };

    if (unsignedValue == 0)
      return Zero();

    var bitCount = size * 8;
    for (var i = bitCount - 1; i >= 0; --i) {
      var bit = 1UL << i;
      if ((unsignedValue & bit) != 0)
        return size switch {
          1 => Promote((byte)bit),
          2 => Promote((ushort)bit),
          4 => Promote((uint)bit),
          8 => Promote(bit),
          _ => ThrowNotSupported<T>()
        };
    }

    return Zero();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Add(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Pointer => Promote(As<nint>(left) + As<nint>(right)),
    CachedTypeCode.UPointer => Promote(As<nuint>(left) + As<nuint>(right)),
    CachedTypeCode.Char => Promote((char)(As<char>(left) + As<char>(right))),
    CachedTypeCode.Byte => Promote((byte)(As<byte>(left) + As<byte>(right))),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(left) + As<sbyte>(right))),
    CachedTypeCode.Int16 => Promote((short)(As<short>(left) + As<short>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(left) + As<ushort>(right))),
    CachedTypeCode.Int32 => Promote(As<int>(left) + As<int>(right)),
    CachedTypeCode.UInt32 => Promote(As<uint>(left) + As<uint>(right)),
    CachedTypeCode.Int64 => Promote(As<long>(left) + As<long>(right)),
    CachedTypeCode.UInt64 => Promote(As<ulong>(left) + As<ulong>(right)),
    CachedTypeCode.Single => Promote(As<float>(left) + As<float>(right)),
    CachedTypeCode.Double => Promote(As<double>(left) + As<double>(right)),
    CachedTypeCode.Decimal => Promote(As<decimal>(left) + As<decimal>(right)),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(left) + (float)As<Half>(right))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(left) + As<UInt128>(right)),
    CachedTypeCode.Int128 => Promote(As<Int128>(left) + As<Int128>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Subtract(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Pointer => Promote(As<nint>(left) - As<nint>(right)),
    CachedTypeCode.UPointer => Promote(As<nuint>(left) - As<nuint>(right)),
    CachedTypeCode.Char => Promote((char)(As<char>(left) - As<char>(right))),
    CachedTypeCode.Byte => Promote((byte)(As<byte>(left) - As<byte>(right))),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(left) - As<sbyte>(right))),
    CachedTypeCode.Int16 => Promote((short)(As<short>(left) - As<short>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(left) - As<ushort>(right))),
    CachedTypeCode.Int32 => Promote(As<int>(left) - As<int>(right)),
    CachedTypeCode.UInt32 => Promote(As<uint>(left) - As<uint>(right)),
    CachedTypeCode.Int64 => Promote(As<long>(left) - As<long>(right)),
    CachedTypeCode.UInt64 => Promote(As<ulong>(left) - As<ulong>(right)),
    CachedTypeCode.Single => Promote(As<float>(left) - As<float>(right)),
    CachedTypeCode.Double => Promote(As<double>(left) - As<double>(right)),
    CachedTypeCode.Decimal => Promote(As<decimal>(left) - As<decimal>(right)),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(left) - (float)As<Half>(right))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(left) - As<UInt128>(right)),
    CachedTypeCode.Int128 => Promote(As<Int128>(left) - As<Int128>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Multiply(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => Promote((byte)(As<byte>(left) * As<byte>(right))),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(left) * As<sbyte>(right))),
    CachedTypeCode.Int16 => Promote((short)(As<short>(left) * As<short>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(left) * As<ushort>(right))),
    CachedTypeCode.Int32 => Promote(As<int>(left) * As<int>(right)),
    CachedTypeCode.UInt32 => Promote(As<uint>(left) * As<uint>(right)),
    CachedTypeCode.Int64 => Promote(As<long>(left) * As<long>(right)),
    CachedTypeCode.UInt64 => Promote(As<ulong>(left) * As<ulong>(right)),
    CachedTypeCode.Single => Promote(As<float>(left) * As<float>(right)),
    CachedTypeCode.Double => Promote(As<double>(left) * As<double>(right)),
    CachedTypeCode.Decimal => Promote(As<decimal>(left) * As<decimal>(right)),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(left) * (float)As<Half>(right))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(left) * As<UInt128>(right)),
    CachedTypeCode.Int128 => Promote(As<Int128>(left) * As<Int128>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Divide(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => Promote((byte)(As<byte>(left) / As<byte>(right))),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(left) / As<sbyte>(right))),
    CachedTypeCode.Int16 => Promote((short)(As<short>(left) / As<short>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(left) / As<ushort>(right))),
    CachedTypeCode.Int32 => Promote(As<int>(left) / As<int>(right)),
    CachedTypeCode.UInt32 => Promote(As<uint>(left) / As<uint>(right)),
    CachedTypeCode.Int64 => Promote(As<long>(left) / As<long>(right)),
    CachedTypeCode.UInt64 => Promote(As<ulong>(left) / As<ulong>(right)),
    CachedTypeCode.Single => Promote(As<float>(left) / As<float>(right)),
    CachedTypeCode.Double => Promote(As<double>(left) / As<double>(right)),
    CachedTypeCode.Decimal => Promote(As<decimal>(left) / As<decimal>(right)),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(left) / (float)As<Half>(right))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(left) / As<UInt128>(right)),
    CachedTypeCode.Int128 => Promote(As<Int128>(left) / As<Int128>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Abs(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => Promote(Math.Abs(As<char>(value))),
    CachedTypeCode.Pointer => Promote(Math.Abs(As<nint>(value))),
    CachedTypeCode.SByte => Promote(Math.Abs(As<sbyte>(value))),
    CachedTypeCode.Int16 => Promote(Math.Abs(As<short>(value))),
    CachedTypeCode.Int32 => Promote(Math.Abs(As<int>(value))),
    CachedTypeCode.Int64 => Promote(Math.Abs(As<long>(value))),
    CachedTypeCode.Single => Promote(Math.Abs(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Abs(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Abs(As<decimal>(value))),
    CachedTypeCode.Byte or CachedTypeCode.UInt16 or CachedTypeCode.UInt32 or CachedTypeCode.UInt64 or CachedTypeCode.UPointer or CachedTypeCode.UInt128 => value, // Already positive
    CachedTypeCode.Half => Promote((Half)Math.Abs((float)As<Half>(value))),
    CachedTypeCode.Int128 => Promote(Int128.IsNegative(As<Int128>(value)) ? -As<Int128>(value) : As<Int128>(value)),
    _ => ThrowNotSupported<T>()
  };

  public static readonly T One = TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => Promote((char)1),
    CachedTypeCode.Pointer => Promote((nint)1),
    CachedTypeCode.UPointer => Promote((nuint)1),
    CachedTypeCode.Byte => Promote((byte)1),
    CachedTypeCode.SByte => Promote((sbyte)1),
    CachedTypeCode.UInt16 => Promote((ushort)1),
    CachedTypeCode.Int16 => Promote((short)1),
    CachedTypeCode.UInt32 => Promote(1u),
    CachedTypeCode.Int32 => Promote(1),
    CachedTypeCode.UInt64 => Promote(1ul),
    CachedTypeCode.Int64 => Promote(1L),
    CachedTypeCode.Single => Promote(1.0f),
    CachedTypeCode.Double => Promote(1.0),
    CachedTypeCode.Decimal => Promote(1m),
    CachedTypeCode.Half => Promote((Half)1.0f),
    CachedTypeCode.UInt128 => Promote(UInt128.One),
    CachedTypeCode.Int128 => Promote(Int128.One),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Zero() => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => Promote((char)0),
    CachedTypeCode.Pointer => Promote((nint)0),
    CachedTypeCode.UPointer => Promote((nuint)0),
    CachedTypeCode.Byte => Promote((byte)0),
    CachedTypeCode.SByte => Promote((sbyte)0),
    CachedTypeCode.UInt16 => Promote((ushort)0),
    CachedTypeCode.Int16 => Promote((short)0),
    CachedTypeCode.UInt32 => Promote(0u),
    CachedTypeCode.Int32 => Promote(0),
    CachedTypeCode.UInt64 => Promote(0ul),
    CachedTypeCode.Int64 => Promote(0L),
    CachedTypeCode.Single => Promote(0.0f),
    CachedTypeCode.Double => Promote(0.0),
    CachedTypeCode.Decimal => Promote(0m),
    CachedTypeCode.Half => Promote((Half)0.0f),
    CachedTypeCode.UInt128 => Promote(UInt128.Zero),
    CachedTypeCode.Int128 => Promote(Int128.Zero),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Ceiling(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Ceiling(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Ceiling(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Ceiling(As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Ceiling((float)As<Half>(value))),
    // For integer types, ceiling is identity
    CachedTypeCode.Char or CachedTypeCode.Pointer or CachedTypeCode.UPointer or
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Floor(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Floor(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Floor(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Floor(As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Floor((float)As<Half>(value))),
    // For integer types, floor is identity
    CachedTypeCode.Char or CachedTypeCode.Pointer or CachedTypeCode.UPointer or
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Round(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Round(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Round(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Round(As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Round((float)As<Half>(value))),
    // For integer types, round is identity
    CachedTypeCode.Char or CachedTypeCode.Pointer or CachedTypeCode.UPointer or
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Truncate(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Truncate(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Truncate(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Truncate(As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Truncate((float)As<Half>(value))),
    // For integer types, truncate is identity
    CachedTypeCode.Char or CachedTypeCode.Pointer or CachedTypeCode.UPointer or
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Sqrt(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Sqrt(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Sqrt(As<double>(value))),
    CachedTypeCode.Decimal => Promote((decimal)Math.Sqrt((double)As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Sqrt((float)As<Half>(value))),
    // For integer types, convert to double, take sqrt, and convert back
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 =>
      From(Math.Sqrt(To<double>(value))),
    CachedTypeCode.UInt128 => Promote(_SqrtUInt128(As<UInt128>(value))),
    CachedTypeCode.Int128 => Promote(_SqrtInt128(As<Int128>(value))),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Exp(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Exp(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Exp(As<double>(value))),
    CachedTypeCode.Decimal => Promote((decimal)Math.Exp((double)As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Exp((float)As<Half>(value))),
    // For integer types, convert to double, calculate, and convert back
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 =>
      From(Math.Exp(To<double>(value))),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Log(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Log(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Log(As<double>(value))),
    CachedTypeCode.Decimal => Promote((decimal)Math.Log((double)As<decimal>(value))),
    CachedTypeCode.Half => Promote((Half)MathF.Log((float)As<Half>(value))),
    // For integer types, convert to double, calculate, and convert back
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 =>
      From(Math.Log(To<double>(value))),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Log2(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Single => Promote(MathF.Log(As<float>(value), 2f)),
    CachedTypeCode.Double => Promote(Math.Log(As<double>(value), 2)),
    CachedTypeCode.Decimal => Promote((decimal)Math.Log((double)As<decimal>(value), 2)),
    CachedTypeCode.Half => Promote((Half)MathF.Log((float)As<Half>(value), 2f)),
    // For integer types, convert to double, calculate, and convert back
    CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.UInt16 or CachedTypeCode.Int16 or
    CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.UInt64 or CachedTypeCode.Int64 or
    CachedTypeCode.UInt128 or CachedTypeCode.Int128 =>
      From(Math.Log(To<double>(value), 2)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T AddSaturate(T left, T right) => TypeCodeCache<T>.Code switch {
    // can only overflow
    CachedTypeCode.Byte => Promote((byte)Math.Min(byte.MaxValue, As<byte>(left) + As<byte>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)Math.Min(ushort.MaxValue, As<ushort>(left) + As<ushort>(right))),
    CachedTypeCode.UInt32 => Promote((uint)Math.Min(uint.MaxValue, As<uint>(left) + (ulong)As<uint>(right))),
    CachedTypeCode.UInt64 => Promote(
      As<ulong>(left) switch {
        var l => (l + As<ulong>(right)) switch {
          var result when result < l => ulong.MaxValue,
          var result => result
        }
      }
    ),
    CachedTypeCode.UPointer => Promote(
      As<nuint>(left) switch {
        var l => (nuint)(l + (ulong)As<nuint>(right)) switch {
          // If result is less than either operand, we overflowed
          var result when result < l => MaxUPointer,
          var result => result
        }
      }
    ),
    CachedTypeCode.UInt128 => Promote(_AddSaturateUInt128(As<UInt128>(left), As<UInt128>(right))),
    // can overflow and underflow
    CachedTypeCode.Char => Promote((char)Math.Min(char.MaxValue, Math.Max(char.MinValue, As<char>(left) + As<char>(right)))),
    CachedTypeCode.Pointer => Promote((As<nint>(left), As<nint>(right)) switch {
      var (l, r) => (l + r) switch {
        var result when l > 0 && r > 0 && result < 0 => MaxPointer,
        var result when l < 0 && r < 0 && result > 0 => MinPointer,
        var result => result
      }
    }),
    CachedTypeCode.SByte => Promote((sbyte)Math.Min(sbyte.MaxValue, Math.Max(sbyte.MinValue, As<sbyte>(left) + As<sbyte>(right)))),
    CachedTypeCode.Int16 => Promote((short)Math.Min(short.MaxValue, Math.Max(short.MinValue, As<short>(left) + As<short>(right)))),
    CachedTypeCode.Int32 => Promote((int)Math.Min(int.MaxValue, Math.Max(int.MinValue, As<int>(left) + (long)As<int>(right)))),
    CachedTypeCode.Int64 => Promote((As<long>(left), As<long>(right)) switch {
      var (l, r) => (l + r) switch {
        var result when l > 0 && r > 0 && result < 0 => long.MaxValue,
        var result when l < 0 && r < 0 && result > 0 => long.MinValue,
        var result => result
      }
    }),
    CachedTypeCode.Int128 => Promote(_AddSaturateInt128(As<Int128>(left), As<Int128>(right))),
    // For floating point, regular addition (no saturation)
    CachedTypeCode.Single or CachedTypeCode.Double or CachedTypeCode.Decimal or CachedTypeCode.Half => Add(left, right),
    _ => ThrowNotSupported<T>()
  };
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T SubtractSaturate(T left, T right) => TypeCodeCache<T>.Code switch {
    // can only underflow
    CachedTypeCode.Byte => Promote((byte)Math.Max(byte.MinValue, As<byte>(left) - As<byte>(right))),
    CachedTypeCode.UInt16 => Promote((ushort)Math.Max(ushort.MinValue, As<ushort>(left) - As<ushort>(right))),
    CachedTypeCode.UInt32 => SubtractSaturateUInt32(As<uint>(left), As<uint>(right)),
    CachedTypeCode.UInt64 => SubtractSaturateUInt64(As<ulong>(left), As<ulong>(right)),
    CachedTypeCode.UPointer => Promote(
      As<nuint>(left) switch {
        var l => (nuint)(l - (ulong)As<nuint>(right)) switch {
          // If result is less than either operand, we underflowed
          var result when result > l => (nuint)UIntPtr.Zero,
          var result => result
        }
      }
    ),
    CachedTypeCode.UInt128 => Promote(_SubtractSaturateUInt128(As<UInt128>(left), As<UInt128>(right))),
    // can overflow and underflow
    CachedTypeCode.Char => Promote((char)Math.Min(char.MaxValue, Math.Max(char.MinValue, As<char>(left) - As<char>(right)))),
    CachedTypeCode.Pointer => Promote((As<nint>(left), As<nint>(right)) switch {
      var (l, r) => (l - r) switch {
        var result when l > 0 && r < 0 && result < 0 => MaxPointer,
        var result when l < 0 && r > 0 && result > 0 => MinPointer,
        var result => result
      }
    }),
    CachedTypeCode.SByte => Promote((sbyte)Math.Min(sbyte.MaxValue, Math.Max(sbyte.MinValue, As<sbyte>(left) - As<sbyte>(right)))),
    CachedTypeCode.Int16 => Promote((short)Math.Min(short.MaxValue, Math.Max(short.MinValue, As<short>(left) - As<short>(right)))),
    CachedTypeCode.Int32 => Promote((int)Math.Min(int.MaxValue, Math.Max(int.MinValue, As<int>(left) - (long)As<int>(right)))),
    CachedTypeCode.Int64 => Promote((As<long>(left), As<long>(right)) switch {
      var(l,r) => r switch {
        > 0 when l < long.MinValue + r => long.MinValue,
        < 0 when l > long.MaxValue + r => long.MaxValue,
        _ => l - r
      }
    }),
    CachedTypeCode.Int128 => Promote(_SubtractSaturateInt128(As<Int128>(left), As<Int128>(right))),
    // For floating point, regular subtraction (no saturation)
    CachedTypeCode.Single or CachedTypeCode.Double or CachedTypeCode.Decimal or CachedTypeCode.Half => Subtract(left, right),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftLeft(T value, int count) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => Promote((byte)(As<byte>(value) << count)),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(value) << count)),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(value) << count)),
    CachedTypeCode.Int16 => Promote((short)(As<short>(value) << count)),
    CachedTypeCode.UInt32 => Promote(As<uint>(value) << count),
    CachedTypeCode.Int32 => Promote(As<int>(value) << count),
    CachedTypeCode.UInt64 => Promote(As<ulong>(value) << count),
    CachedTypeCode.Int64 => Promote(As<long>(value) << count),
    CachedTypeCode.Single => Promote(As<float>(value) * MathF.Pow(2, count)),
    CachedTypeCode.Double => Promote((As<double>(value) * Math.Pow(2, count))),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(value) * MathF.Pow(2, count))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(value) << count),
    CachedTypeCode.Int128 => Promote(As<Int128>(value) << count),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftRightArithmetic(T value, int count) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => Promote((byte)(As<byte>(value) >> count)),
    CachedTypeCode.SByte => Promote((sbyte)(As<sbyte>(value) >> count)),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(value) >> count)),
    CachedTypeCode.Int16 => Promote((short)(As<short>(value) >> count)),
    CachedTypeCode.UInt32 => Promote(As<uint>(value) >> count),
    CachedTypeCode.Int32 => Promote(As<int>(value) >> count),
    CachedTypeCode.UInt64 => Promote(As<ulong>(value) >> count),
    CachedTypeCode.Int64 => Promote(As<long>(value) >> count),
    CachedTypeCode.Single => Promote(As<float>(value) / MathF.Pow(2, count)),
    CachedTypeCode.Double => Promote(As<double>(value) / Math.Pow(2, count)),
    CachedTypeCode.Half => Promote((Half)((float)As<Half>(value) / MathF.Pow(2, count))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(value) >> count),
    CachedTypeCode.Int128 => Promote(As<Int128>(value) >> count),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftRightLogical(T value, int count) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.SByte => Promote((sbyte)((byte)As<sbyte>(value) >> count)),
    CachedTypeCode.Int16 => Promote((short)((ushort)As<short>(value) >> count)),
    CachedTypeCode.Int32 => Promote(As<int>(value) >>> count),
    CachedTypeCode.Int64 => Promote(As<long>(value) >>> count),
    CachedTypeCode.Int128 => Promote(As<Int128>(value) >>> count),
    _ => ShiftRightArithmetic(value, count)
  };

  public static T MultiplyAddEstimate(T left, T right, T addend) => Add(Multiply(left, right), addend);

  public static T AllBitsSet => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => Promote(byte.MaxValue),
    CachedTypeCode.SByte => Promote((sbyte)~0),
    CachedTypeCode.UInt16 => Promote(ushort.MaxValue),
    CachedTypeCode.Int16 => Promote((short)~0),
    CachedTypeCode.UInt32 => Promote(uint.MaxValue),
    CachedTypeCode.Int32 => Promote(-1),
    CachedTypeCode.UInt64 => Promote(ulong.MaxValue),
    CachedTypeCode.Int64 => Promote(-1L),
    CachedTypeCode.Single => Promote(As<float,int>(-1)),
    CachedTypeCode.Double => Promote(As<double, long>(-1L)),
    CachedTypeCode.Char => Promote(unchecked((char)~0)),
    CachedTypeCode.Pointer => Promote((nint)(-1L)),
    CachedTypeCode.UPointer => Promote(MaxUPointer),
    CachedTypeCode.Half => Promote(As<Half, ushort>(ushort.MaxValue)),
    CachedTypeCode.UInt128 => Promote(UInt128.MaxValue),
    CachedTypeCode.Int128 => Promote(Int128.NegativeOne),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThan(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => As<char>(left) > As<char>(right),
    CachedTypeCode.Pointer => As<nint>(left) > As<nint>(right),
    CachedTypeCode.UPointer => As<nuint>(left) > As<nuint>(right),
    CachedTypeCode.Byte => As<byte>(left) > As<byte>(right),
    CachedTypeCode.SByte => As<sbyte>(left) > As<sbyte>(right),
    CachedTypeCode.UInt16 => As<ushort>(left) > As<ushort>(right),
    CachedTypeCode.Int16 => As<short>(left) > As<short>(right),
    CachedTypeCode.UInt32 => As<uint>(left) > As<uint>(right),
    CachedTypeCode.Int32 => As<int>(left) > As<int>(right),
    CachedTypeCode.UInt64 => As<ulong>(left) > As<ulong>(right),
    CachedTypeCode.Int64 => As<long>(left) > As<long>(right),
    CachedTypeCode.Single => As<float>(left) > As<float>(right),
    CachedTypeCode.Double => As<double>(left) > As<double>(right),
    CachedTypeCode.Decimal => As<decimal>(left) > As<decimal>(right),
    CachedTypeCode.Half => (float)As<Half>(left) > (float)As<Half>(right),
    CachedTypeCode.UInt128 => As<UInt128>(left) > As<UInt128>(right),
    CachedTypeCode.Int128 => As<Int128>(left) > As<Int128>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqual(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => As<char>(left) >= As<char>(right),
    CachedTypeCode.Pointer => As<nint>(left) >= As<nint>(right),
    CachedTypeCode.UPointer => As<nuint>(left) >= As<nuint>(right),
    CachedTypeCode.Byte => As<byte>(left) >= As<byte>(right),
    CachedTypeCode.SByte => As<sbyte>(left) >= As<sbyte>(right),
    CachedTypeCode.UInt16 => As<ushort>(left) >= As<ushort>(right),
    CachedTypeCode.Int16 => As<short>(left) >= As<short>(right),
    CachedTypeCode.UInt32 => As<uint>(left) >= As<uint>(right),
    CachedTypeCode.Int32 => As<int>(left) >= As<int>(right),
    CachedTypeCode.UInt64 => As<ulong>(left) >= As<ulong>(right),
    CachedTypeCode.Int64 => As<long>(left) >= As<long>(right),
    CachedTypeCode.Single => As<float>(left) >= As<float>(right),
    CachedTypeCode.Double => As<double>(left) >= As<double>(right),
    CachedTypeCode.Decimal => As<decimal>(left) >= As<decimal>(right),
    CachedTypeCode.Half => (float)As<Half>(left) >= (float)As<Half>(right),
    CachedTypeCode.UInt128 => As<UInt128>(left) >= As<UInt128>(right),
    CachedTypeCode.Int128 => As<Int128>(left) >= As<Int128>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThan(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => As<char>(left) < As<char>(right),
    CachedTypeCode.Pointer => As<nint>(left) < As<nint>(right),
    CachedTypeCode.UPointer => As<nuint>(left) < As<nuint>(right),
    CachedTypeCode.Byte => As<byte>(left) < As<byte>(right),
    CachedTypeCode.SByte => As<sbyte>(left) < As<sbyte>(right),
    CachedTypeCode.UInt16 => As<ushort>(left) < As<ushort>(right),
    CachedTypeCode.Int16 => As<short>(left) < As<short>(right),
    CachedTypeCode.UInt32 => As<uint>(left) < As<uint>(right),
    CachedTypeCode.Int32 => As<int>(left) < As<int>(right),
    CachedTypeCode.UInt64 => As<ulong>(left) < As<ulong>(right),
    CachedTypeCode.Int64 => As<long>(left) < As<long>(right),
    CachedTypeCode.Single => As<float>(left) < As<float>(right),
    CachedTypeCode.Double => As<double>(left) < As<double>(right),
    CachedTypeCode.Decimal => As<decimal>(left) < As<decimal>(right),
    CachedTypeCode.Half => (float)As<Half>(left) < (float)As<Half>(right),
    CachedTypeCode.UInt128 => As<UInt128>(left) < As<UInt128>(right),
    CachedTypeCode.Int128 => As<Int128>(left) < As<Int128>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqual(T left, T right) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => As<char>(left) <= As<char>(right),
    CachedTypeCode.Pointer => As<nint>(left) <= As<nint>(right),
    CachedTypeCode.UPointer => As<nuint>(left) <= As<nuint>(right),
    CachedTypeCode.Byte => As<byte>(left) <= As<byte>(right),
    CachedTypeCode.SByte => As<sbyte>(left) <= As<sbyte>(right),
    CachedTypeCode.UInt16 => As<ushort>(left) <= As<ushort>(right),
    CachedTypeCode.Int16 => As<short>(left) <= As<short>(right),
    CachedTypeCode.UInt32 => As<uint>(left) <= As<uint>(right),
    CachedTypeCode.Int32 => As<int>(left) <= As<int>(right),
    CachedTypeCode.UInt64 => As<ulong>(left) <= As<ulong>(right),
    CachedTypeCode.Int64 => As<long>(left) <= As<long>(right),
    CachedTypeCode.Single => As<float>(left) <= As<float>(right),
    CachedTypeCode.Double => As<double>(left) <= As<double>(right),
    CachedTypeCode.Decimal => As<decimal>(left) <= As<decimal>(right),
    CachedTypeCode.Half => (float)As<Half>(left) <= (float)As<Half>(right),
    CachedTypeCode.UInt128 => As<UInt128>(left) <= As<UInt128>(right),
    CachedTypeCode.Int128 => As<Int128>(left) <= As<Int128>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ObjectEquals(T left, T right) => EqualityComparer<T>.Default.Equals(left, right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Min(T left, T right) => LessThanOrEqual(left, right) ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Max(T left, T right) => GreaterThanOrEqual(left, right) ? left : right;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Clamp(T value, T min, T max) => Max(min, Min(max, value));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Negate(T value) => Subtract(Zero(), value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Sign(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => Promote((char)Math.Sign(As<char>(value))),
    CachedTypeCode.Pointer => Promote((nint)Math.Sign(As<nint>(value))),
    CachedTypeCode.SByte => Promote((sbyte)Math.Sign(As<sbyte>(value))),
    CachedTypeCode.Int16 => Promote((short)Math.Sign(As<short>(value))),
    CachedTypeCode.Int32 => Promote(Math.Sign(As<int>(value))),
    CachedTypeCode.Int64 => Promote(Math.Sign(As<long>(value))),
    CachedTypeCode.Single => Promote(Math.Sign(As<float>(value))),
    CachedTypeCode.Double => Promote(Math.Sign(As<double>(value))),
    CachedTypeCode.Decimal => Promote(Math.Sign(As<decimal>(value))),
    CachedTypeCode.UPointer => Promote((nuint)(As<nuint>(value) == 0 ? 0 : 1)),
    CachedTypeCode.Byte => Promote((byte)(As<byte>(value) == 0 ? 0 : 1)),
    CachedTypeCode.UInt16 => Promote((ushort)(As<ushort>(value) == 0 ? 0 : 1)),
    CachedTypeCode.UInt32 => Promote(As<uint>(value) == 0 ? 0u : 1u),
    CachedTypeCode.UInt64 => Promote(As<ulong>(value) == 0 ? 0ul : 1ul),
    CachedTypeCode.Half => Promote((Half)Math.Sign((float)As<Half>(value))),
    CachedTypeCode.UInt128 => Promote(As<UInt128>(value) == UInt128.Zero ? UInt128.Zero : UInt128.One),
    CachedTypeCode.Int128 => Promote(_SignInt128(As<Int128>(value))),
    _ => ThrowNotSupported<T>()
  };

  #region Helper methods

  // TODO: chars, pointer,upointer
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ToInt32(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Byte => As<byte>(value),
    CachedTypeCode.SByte => As<sbyte>(value),
    CachedTypeCode.UInt16 => As<ushort>(value),
    CachedTypeCode.Int16 => As<short>(value),
    CachedTypeCode.UInt32 => (int)As<uint>(value),
    CachedTypeCode.Int32 => As<int>(value),
    CachedTypeCode.UInt64 => (int)As<ulong>(value),
    CachedTypeCode.Int64 => (int)As<long>(value),
    _ => ThrowNotSupported<int>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T From<TFrom>(TFrom value) {
    if (TypeCodeCache<T>.Code == TypeCodeCache<TFrom>.Code)
      return As<T, TFrom>(value);

    return TypeCodeCache<T>.Code switch {
      CachedTypeCode.Byte => Promote(TypeCodeCache<TFrom>.Code switch {
          CachedTypeCode.SByte => (byte)Scalar<TFrom>.As<sbyte>(value),
          CachedTypeCode.UInt16 => (byte)Scalar<TFrom>.As<ushort>(value),
          CachedTypeCode.Int16 => (byte)Scalar<TFrom>.As<short>(value),
          CachedTypeCode.UInt32 => (byte)Scalar<TFrom>.As<uint>(value),
          CachedTypeCode.Int32 => (byte)Scalar<TFrom>.As<int>(value),
          CachedTypeCode.UInt64 => (byte)Scalar<TFrom>.As<ulong>(value),
          CachedTypeCode.Int64 => (byte)Scalar<TFrom>.As<long>(value),
          CachedTypeCode.Single => (byte)Scalar<TFrom>.As<float>(value),
          CachedTypeCode.Double => (byte)Scalar<TFrom>.As<double>(value),
          CachedTypeCode.Decimal => (byte)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<byte>()
      }),
      CachedTypeCode.SByte => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => (sbyte)Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.UInt16 => (sbyte)Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (sbyte)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (sbyte)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (sbyte)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (sbyte)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (sbyte)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (sbyte)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (sbyte)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (sbyte)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<sbyte>()
      }),
      CachedTypeCode.UInt16 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (ushort)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.Int16 => (ushort)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (ushort)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (ushort)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (ushort)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (ushort)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (ushort)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (ushort)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (ushort)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<ushort>()
      }),
      CachedTypeCode.Int16 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => (short)Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.UInt32 => (short)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (short)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (short)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (short)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (short)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (short)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (short)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<short>()
      }),
      CachedTypeCode.UInt32 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (uint)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (uint)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.Int32 => (uint)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (uint)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (uint)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (uint)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (uint)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (uint)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<uint>()
      }),
      CachedTypeCode.Int32 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (int)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.UInt64 => (int)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (int)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (int)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (int)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (int)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<int>()
      }),
      CachedTypeCode.UInt64 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (ulong)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (ulong)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (ulong)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.Int64 => (ulong)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (ulong)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (ulong)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (ulong)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<ulong>()
      }),
      CachedTypeCode.Int64 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (long)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Single => (long)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (long)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (long)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<long>()
      }),
      CachedTypeCode.Single => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Double => (float)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (float)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<float>()
      }),
      CachedTypeCode.Double => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Decimal => (double)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<double>()
      }),
      CachedTypeCode.Decimal => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (decimal)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (decimal)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Half => (decimal)(float)Scalar<TFrom>.As<Half>(value),
        _ => ThrowNotSupported<decimal>()
      }),
      CachedTypeCode.Half => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => (Half)Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (Half)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => (Half)Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (Half)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (Half)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (Half)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (Half)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (Half)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (Half)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (Half)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Decimal => (Half)(float)Scalar<TFrom>.As<decimal>(value),
        _ => ThrowNotSupported<Half>()
      }),
      CachedTypeCode.UInt128 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => (UInt128)Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (UInt128)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => (UInt128)Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (UInt128)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (UInt128)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (UInt128)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (UInt128)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (UInt128)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (UInt128)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (UInt128)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.Int128 => (UInt128)Scalar<TFrom>.As<Int128>(value),
        _ => ThrowNotSupported<UInt128>()
      }),
      CachedTypeCode.Int128 => Promote(TypeCodeCache<TFrom>.Code switch {
        CachedTypeCode.Byte => (Int128)Scalar<TFrom>.As<byte>(value),
        CachedTypeCode.SByte => (Int128)Scalar<TFrom>.As<sbyte>(value),
        CachedTypeCode.UInt16 => (Int128)Scalar<TFrom>.As<ushort>(value),
        CachedTypeCode.Int16 => (Int128)Scalar<TFrom>.As<short>(value),
        CachedTypeCode.UInt32 => (Int128)Scalar<TFrom>.As<uint>(value),
        CachedTypeCode.Int32 => (Int128)Scalar<TFrom>.As<int>(value),
        CachedTypeCode.UInt64 => (Int128)Scalar<TFrom>.As<ulong>(value),
        CachedTypeCode.Int64 => (Int128)Scalar<TFrom>.As<long>(value),
        CachedTypeCode.Single => (Int128)Scalar<TFrom>.As<float>(value),
        CachedTypeCode.Double => (Int128)Scalar<TFrom>.As<double>(value),
        CachedTypeCode.UInt128 => (Int128)Scalar<TFrom>.As<UInt128>(value),
        _ => ThrowNotSupported<Int128>()
      }),
      _ => ThrowNotSupported<T>()
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TTo To<TTo>(T value) => TypeCodeCache<T>.Code switch {
    CachedTypeCode.Char => Scalar<TTo>.From(As<char>(value)),
    CachedTypeCode.Pointer => Scalar<TTo>.From(As<nint>(value)),
    CachedTypeCode.UPointer => Scalar<TTo>.From(As<nuint>(value)),
    CachedTypeCode.Byte => Scalar<TTo>.From(As<byte>(value)),
    CachedTypeCode.SByte => Scalar<TTo>.From(As<sbyte>(value)),
    CachedTypeCode.UInt16 => Scalar<TTo>.From(As<ushort>(value)),
    CachedTypeCode.Int16 => Scalar<TTo>.From(As<short>(value)),
    CachedTypeCode.UInt32 => Scalar<TTo>.From(As<uint>(value)),
    CachedTypeCode.Int32 => Scalar<TTo>.From(As<int>(value)),
    CachedTypeCode.UInt64 => Scalar<TTo>.From(As<ulong>(value)),
    CachedTypeCode.Int64 => Scalar<TTo>.From(As<long>(value)),
    CachedTypeCode.Single => Scalar<TTo>.From(As<float>(value)),
    CachedTypeCode.Double => Scalar<TTo>.From(As<double>(value)),
    CachedTypeCode.Decimal => Scalar<TTo>.From(As<decimal>(value)),
    CachedTypeCode.Half => Scalar<TTo>.From(As<Half>(value)),
    CachedTypeCode.UInt128 => Scalar<TTo>.From(As<UInt128>(value)),
    CachedTypeCode.Int128 => Scalar<TTo>.From(As<Int128>(value)),
    _ => ThrowNotSupported<TTo>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  private static unsafe T Promote<TFrom>(TFrom value) where TFrom : unmanaged => *(T*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  public static unsafe TTo As<TTo>(T value) where TTo : unmanaged => *(TTo*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  private static unsafe TTo As<TTo,TFrom>(TFrom value) => *(TTo*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type


  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  [StackTraceHidden]
  private static G ThrowNotSupported<G>([CallerMemberName] string? source = null) => throw new NotSupportedException($"Scalar: The type {typeof(T)} is not supported{(source == null ? string.Empty : $"for method '{source}'")}.");

  #endregion

  // Specialized saturate helpers
  private static T SubtractSaturateUInt32(uint left, uint right) =>
    Promote(left < right ? 0u : left - right);

  private static T SubtractSaturateUInt64(ulong left, ulong right) =>
    Promote(left < right ? 0ul : left - right);

  private static nint MinPointer => IntPtr.Size == 4 ? int.MinValue : unchecked((nint)long.MinValue);
  private static nint MaxPointer => IntPtr.Size == 4 ? int.MaxValue : unchecked((nint)long.MaxValue);
  private static nuint MaxUPointer => UIntPtr.Size == 4 ? uint.MaxValue : unchecked((nuint)ulong.MaxValue);

  // 128-bit helper methods
  private static UInt128 _SqrtUInt128(UInt128 value) {
    if (value == UInt128.Zero)
      return UInt128.Zero;

    // Newton-Raphson method for integer square root
    var x = value;
    var y = (x + UInt128.One) >> 1;
    while (y < x) {
      x = y;
      y = (x + value / x) >> 1;
    }
    return x;
  }

  private static Int128 _SqrtInt128(Int128 value) {
    if (Int128.IsNegative(value))
      throw new ArgumentOutOfRangeException(nameof(value), "Cannot take square root of negative number");

    if (value == Int128.Zero)
      return Int128.Zero;

    // Newton-Raphson method for integer square root
    var x = value;
    var y = (x + Int128.One) >> 1;
    while (y < x) {
      x = y;
      y = (x + value / x) >> 1;
    }
    return x;
  }

  private static UInt128 _AddSaturateUInt128(UInt128 left, UInt128 right) {
    var result = left + right;
    return result < left ? UInt128.MaxValue : result;
  }

  private static Int128 _AddSaturateInt128(Int128 left, Int128 right) {
    var result = left + right;
    if (Int128.IsPositive(left) && Int128.IsPositive(right) && Int128.IsNegative(result))
      return Int128.MaxValue;
    if (Int128.IsNegative(left) && Int128.IsNegative(right) && Int128.IsPositive(result))
      return Int128.MinValue;
    return result;
  }

  private static UInt128 _SubtractSaturateUInt128(UInt128 left, UInt128 right) =>
    left < right ? UInt128.Zero : left - right;

  private static Int128 _SubtractSaturateInt128(Int128 left, Int128 right) {
    var result = left - right;
    if (Int128.IsPositive(left) && Int128.IsNegative(right) && Int128.IsNegative(result))
      return Int128.MaxValue;
    if (Int128.IsNegative(left) && Int128.IsPositive(right) && Int128.IsPositive(result))
      return Int128.MinValue;
    return result;
  }

  private static Int128 _SignInt128(Int128 value) =>
    Int128.IsNegative(value) ? Int128.NegativeOne :
    value == Int128.Zero ? Int128.Zero :
    Int128.One;

  // UInt128 upper/lower accessors (works with both native and polyfill types)
#if SUPPORTS_UINT128
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500
  private static unsafe ulong _GetUpper(UInt128 value) => ((ulong*)&value)[1];
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe ulong _GetLower(UInt128 value) => ((ulong*)&value)[0];
#pragma warning restore CS8500
#else
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _GetUpper(UInt128 value) => value.Upper;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong _GetLower(UInt128 value) => value.Lower;
#endif

}