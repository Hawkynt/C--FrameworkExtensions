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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics;

file static class TypeCodeCache<T> {
  public static TypeCode Code =>
    typeof(T) == typeof(byte) ? TypeCode.Byte :
    typeof(T) == typeof(sbyte) ? TypeCode.SByte :
    typeof(T) == typeof(ushort) ? TypeCode.UInt16 :
    typeof(T) == typeof(short) ? TypeCode.Int16 :
    typeof(T) == typeof(uint) ? TypeCode.UInt32 :
    typeof(T) == typeof(int) ? TypeCode.Int32 :
    typeof(T) == typeof(ulong) ? TypeCode.UInt64 :
    typeof(T) == typeof(long) ? TypeCode.Int64 :
    typeof(T) == typeof(float) ? TypeCode.Single :
    typeof(T) == typeof(double) ? TypeCode.Double :
    typeof(T) == typeof(decimal) ? TypeCode.Decimal :
    TypeCode.Object
  ;
}

internal static class Scalar<T> {
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ExtractMostSignificantBit(T value) => Unsafe.SizeOf<T>() switch {
    1 => (As<byte>(value) & 0x80) != 0,
    2 => (As<ushort>(value) & 0x8000) != 0,
    4 => (As<uint>(value) & 0x80000000) != 0,
    8 => (As<ulong>(value) & 0x8000000000000000) != 0,
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Add(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(left) + As<byte>(right))),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(left) + As<sbyte>(right))),
    TypeCode.Int16 => Promote((short)(As<short>(left) + As<short>(right))),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(left) + As<ushort>(right))),
    TypeCode.Int32 => Promote(As<int>(left) + As<int>(right)),
    TypeCode.UInt32 => Promote(As<uint>(left) + As<uint>(right)),
    TypeCode.Int64 => Promote(As<long>(left) + As<long>(right)),
    TypeCode.UInt64 => Promote(As<ulong>(left) + As<ulong>(right)),
    TypeCode.Single => Promote(As<float>(left) + As<float>(right)),
    TypeCode.Double => Promote(As<double>(left) + As<double>(right)),
    TypeCode.Decimal => Promote(As<decimal>(left) + As<decimal>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Subtract(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(left) - As<byte>(right))),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(left) - As<sbyte>(right))),
    TypeCode.Int16 => Promote((short)(As<short>(left) - As<short>(right))),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(left) - As<ushort>(right))),
    TypeCode.Int32 => Promote(As<int>(left) - As<int>(right)),
    TypeCode.UInt32 => Promote(As<uint>(left) - As<uint>(right)),
    TypeCode.Int64 => Promote(As<long>(left) - As<long>(right)),
    TypeCode.UInt64 => Promote(As<ulong>(left) - As<ulong>(right)),
    TypeCode.Single => Promote(As<float>(left) - As<float>(right)),
    TypeCode.Double => Promote(As<double>(left) - As<double>(right)),
    TypeCode.Decimal => Promote(As<decimal>(left) - As<decimal>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Multiply(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(left) * As<byte>(right))),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(left) * As<sbyte>(right))),
    TypeCode.Int16 => Promote((short)(As<short>(left) * As<short>(right))),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(left) * As<ushort>(right))),
    TypeCode.Int32 => Promote(As<int>(left) * As<int>(right)),
    TypeCode.UInt32 => Promote(As<uint>(left) * As<uint>(right)),
    TypeCode.Int64 => Promote(As<long>(left) * As<long>(right)),
    TypeCode.UInt64 => Promote(As<ulong>(left) * As<ulong>(right)),
    TypeCode.Single => Promote(As<float>(left) * As<float>(right)),
    TypeCode.Double => Promote(As<double>(left) * As<double>(right)),
    TypeCode.Decimal => Promote(As<decimal>(left) * As<decimal>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Divide(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(left) / As<byte>(right))),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(left) / As<sbyte>(right))),
    TypeCode.Int16 => Promote((short)(As<short>(left) / As<short>(right))),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(left) / As<ushort>(right))),
    TypeCode.Int32 => Promote(As<int>(left) / As<int>(right)),
    TypeCode.UInt32 => Promote(As<uint>(left) / As<uint>(right)),
    TypeCode.Int64 => Promote(As<long>(left) / As<long>(right)),
    TypeCode.UInt64 => Promote(As<ulong>(left) / As<ulong>(right)),
    TypeCode.Single => Promote(As<float>(left) / As<float>(right)),
    TypeCode.Double => Promote(As<double>(left) / As<double>(right)),
    TypeCode.Decimal => Promote(As<decimal>(left) / As<decimal>(right)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Abs(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.SByte => Promote(Math.Abs(As<sbyte>(value))),
    TypeCode.Int16 => Promote(Math.Abs(As<short>(value))),
    TypeCode.Int32 => Promote(Math.Abs(As<int>(value))),
    TypeCode.Int64 => Promote(Math.Abs(As<long>(value))),
    TypeCode.Single => Promote(Math.Abs(As<float>(value))),
    TypeCode.Double => Promote(Math.Abs(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Abs(As<decimal>(value))),
    TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => value, // Already positive
    _ => ThrowNotSupported<T>()
  };

  public static readonly T One = TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)1),
    TypeCode.SByte => Promote((sbyte)1),
    TypeCode.UInt16 => Promote((ushort)1),
    TypeCode.Int16 => Promote((short)1),
    TypeCode.UInt32 => Promote(1u),
    TypeCode.Int32 => Promote(1),
    TypeCode.UInt64 => Promote(1ul),
    TypeCode.Int64 => Promote(1L),
    TypeCode.Single => Promote(1.0f),
    TypeCode.Double => Promote(1.0),
    TypeCode.Decimal => Promote(1m),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Zero() => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)0),
    TypeCode.SByte => Promote((sbyte)0),
    TypeCode.UInt16 => Promote((ushort)0),
    TypeCode.Int16 => Promote((short)0),
    TypeCode.UInt32 => Promote(0u),
    TypeCode.Int32 => Promote(0),
    TypeCode.UInt64 => Promote(0ul),
    TypeCode.Int64 => Promote(0L),
    TypeCode.Single => Promote(0.0f),
    TypeCode.Double => Promote(0.0),
    TypeCode.Decimal => Promote(0m),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Ceiling(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Single => Promote(
#if SUPPORTS_MATHF
      MathF.Ceiling(As<float>(value))
#else
      (float)Math.Ceiling(As<float>(value))
#endif
    ),
    TypeCode.Double => Promote(Math.Ceiling(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Ceiling(As<decimal>(value))),
    // For integer types, ceiling is identity
    TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.Int16 or
    TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Floor(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Single => Promote(
#if SUPPORTS_MATHF
      MathF.Floor(As<float>(value))
#else
      (float)Math.Floor(As<float>(value))
#endif
    ),
    TypeCode.Double => Promote(Math.Floor(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Floor(As<decimal>(value))),
    // For integer types, floor is identity
    TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.Int16 or
    TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Round(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Single => Promote(
#if SUPPORTS_MATHF
      MathF.Round(As<float>(value))
#else
      (float)Math.Round(As<float>(value))
#endif
    ),
    TypeCode.Double => Promote(Math.Round(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Round(As<decimal>(value))),
    // For integer types, floor is identity
    TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.Int16 or
      TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Truncate(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Single => Promote(
#if SUPPORTS_MATHF
      MathF.Truncate(As<float>(value))
#else
      (float)Math.Truncate(As<float>(value))
#endif
    ),
    TypeCode.Double => Promote(Math.Truncate(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Truncate(As<decimal>(value))),
    // For integer types, floor is identity
    TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.Int16 or
      TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 => value,
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Sqrt(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Single => Promote(
#if SUPPORTS_MATHF
      MathF.Sqrt(As<float>(value))
#else
      (float)Math.Sqrt(As<float>(value))
#endif
    ),
    TypeCode.Double => Promote(Math.Sqrt(As<double>(value))),
    TypeCode.Decimal => Promote((decimal)Math.Sqrt((double)As<decimal>(value))),
    // For integer types, convert to double, take sqrt, and convert back
    TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.Int16 or
    TypeCode.UInt32 or TypeCode.Int32 or TypeCode.UInt64 or TypeCode.Int64 =>
      From(Math.Sqrt(ToDouble(value))),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T AddSaturate(T left, T right) => TypeCodeCache<T>.Code switch {
    // can only overflow
    TypeCode.Byte => Promote((byte)Math.Min(byte.MaxValue, As<byte>(left) + As<byte>(right))),
    TypeCode.UInt16 => Promote((ushort)Math.Min(ushort.MaxValue, As<ushort>(left) + As<ushort>(right))),
    TypeCode.UInt32 => Promote((uint)Math.Min(uint.MaxValue, As<uint>(left) + (ulong)As<uint>(right))),
    TypeCode.UInt64 => AddSaturateUInt64(As<ulong>(left), As<ulong>(right)),
    // can overflow and underflow
    TypeCode.SByte => Promote((sbyte)Math.Min(sbyte.MaxValue, Math.Max(sbyte.MinValue, As<sbyte>(left) + As<sbyte>(right)))),
    TypeCode.Int16 => Promote((short)Math.Min(short.MaxValue, Math.Max(short.MinValue, As<short>(left) + As<short>(right)))),
    TypeCode.Int32 => Promote((int)Math.Min(int.MaxValue, Math.Max(int.MinValue, As<int>(left) + (long)As<int>(right)))),
    TypeCode.Int64 => AddSaturateInt64(As<long>(left), As<long>(right)),
    // For floating point, regular addition (no saturation)  
    TypeCode.Single or TypeCode.Double or TypeCode.Decimal => Add(left, right),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T SubtractSaturate(T left, T right) => TypeCodeCache<T>.Code switch {
    // can only underflow
    TypeCode.Byte => Promote((byte)Math.Max(byte.MinValue, As<byte>(left) - As<byte>(right))),
    TypeCode.UInt16 => Promote((ushort)Math.Max(ushort.MinValue, As<ushort>(left) - As<ushort>(right))),
    TypeCode.UInt32 => SubtractSaturateUInt32(As<uint>(left), As<uint>(right)),
    TypeCode.UInt64 => SubtractSaturateUInt64(As<ulong>(left), As<ulong>(right)),
    // can overflow and underflow
    TypeCode.SByte => Promote((sbyte)Math.Min(sbyte.MaxValue, Math.Max(sbyte.MinValue, As<sbyte>(left) - As<sbyte>(right)))),
    TypeCode.Int16 => Promote((short)Math.Min(short.MaxValue, Math.Max(short.MinValue, As<short>(left) - As<short>(right)))),
    TypeCode.Int32 => Promote((int)Math.Min(int.MaxValue, Math.Max(int.MinValue, As<int>(left) - (long)As<int>(right)))),
    TypeCode.Int64 => SubtractSaturateInt64(As<long>(left), As<long>(right)),
    // For floating point, regular subtraction (no saturation)
    TypeCode.Single or TypeCode.Double or TypeCode.Decimal => Subtract(left, right),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftLeft(T value, int count) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(value) << count)),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(value) << count)),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(value) << count)),
    TypeCode.Int16 => Promote((short)(As<short>(value) << count)),
    TypeCode.UInt32 => Promote(As<uint>(value) << count),
    TypeCode.Int32 => Promote(As<int>(value) << count),
    TypeCode.UInt64 => Promote(As<ulong>(value) << count),
    TypeCode.Int64 => Promote(As<long>(value) << count),
#if SUPPORTS_MATHF
    TypeCode.Single => Promote(As<float>(value) * MathF.Pow(2, count)),
#else
    TypeCode.Single => Promote((float)(As<float>(value) * Math.Pow(2, count))),
#endif
    TypeCode.Double => Promote((As<double>(value) * Math.Pow(2, count))),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftRightArithmetic(T value, int count) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)(As<byte>(value) >> count)),
    TypeCode.SByte => Promote((sbyte)(As<sbyte>(value) >> count)),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(value) >> count)),
    TypeCode.Int16 => Promote((short)(As<short>(value) >> count)),
    TypeCode.UInt32 => Promote(As<uint>(value) >> count),
    TypeCode.Int32 => Promote(As<int>(value) >> count),
    TypeCode.UInt64 => Promote(As<ulong>(value) >> count),
    TypeCode.Int64 => Promote(As<long>(value) >> count),
#if SUPPORTS_MATHF
  TypeCode.Single => Promote(As<float>(value) / MathF.Pow(2, count)),
#else
    TypeCode.Single => Promote((float)(As<float>(value) / Math.Pow(2, count))),
#endif
    TypeCode.Double => Promote(As<double>(value) / Math.Pow(2, count)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ShiftRightLogical(T value, int count) => TypeCodeCache<T>.Code switch {
    TypeCode.SByte => Promote((sbyte)((byte)As<sbyte>(value) >> count)),
    TypeCode.Int16 => Promote((short)((ushort)As<short>(value) >> count)),
    TypeCode.Int32 => Promote(As<int>(value) >>> count),
    TypeCode.Int64 => Promote(As<long>(value) >>> count),
    _ => ShiftRightArithmetic(value, count)
  };

  public static T MultiplyAddEstimate(T left, T right, T addend) => Add(Multiply(left, right), addend);

  public static T AllBitsSet => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote(byte.MaxValue),
    TypeCode.SByte => Promote((sbyte)~0),
    TypeCode.UInt16 => Promote(ushort.MaxValue),
    TypeCode.Int16 => Promote((short)~0),
    TypeCode.UInt32 => Promote(uint.MaxValue),
    TypeCode.Int32 => Promote(-1),
    TypeCode.UInt64 => Promote(ulong.MaxValue),
    TypeCode.Int64 => Promote(-1L),
    TypeCode.Single => Promote(As<float,int>(-1)),
    TypeCode.Double => Promote(As<double, long>(-1L)),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThan(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => As<byte>(left) > As<byte>(right),
    TypeCode.SByte => As<sbyte>(left) > As<sbyte>(right),
    TypeCode.UInt16 => As<ushort>(left) > As<ushort>(right),
    TypeCode.Int16 => As<short>(left) > As<short>(right),
    TypeCode.UInt32 => As<uint>(left) > As<uint>(right),
    TypeCode.Int32 => As<int>(left) > As<int>(right),
    TypeCode.UInt64 => As<ulong>(left) > As<ulong>(right),
    TypeCode.Int64 => As<long>(left) > As<long>(right),
    TypeCode.Single => As<float>(left) > As<float>(right),
    TypeCode.Double => As<double>(left) > As<double>(right),
    TypeCode.Decimal => As<decimal>(left) > As<decimal>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GreaterThanOrEqual(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => As<byte>(left) >= As<byte>(right),
    TypeCode.SByte => As<sbyte>(left) >= As<sbyte>(right),
    TypeCode.UInt16 => As<ushort>(left) >= As<ushort>(right),
    TypeCode.Int16 => As<short>(left) >= As<short>(right),
    TypeCode.UInt32 => As<uint>(left) >= As<uint>(right),
    TypeCode.Int32 => As<int>(left) >= As<int>(right),
    TypeCode.UInt64 => As<ulong>(left) >= As<ulong>(right),
    TypeCode.Int64 => As<long>(left) >= As<long>(right),
    TypeCode.Single => As<float>(left) >= As<float>(right),
    TypeCode.Double => As<double>(left) >= As<double>(right),
    TypeCode.Decimal => As<decimal>(left) >= As<decimal>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThan(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => As<byte>(left) < As<byte>(right),
    TypeCode.SByte => As<sbyte>(left) < As<sbyte>(right),
    TypeCode.UInt16 => As<ushort>(left) < As<ushort>(right),
    TypeCode.Int16 => As<short>(left) < As<short>(right),
    TypeCode.UInt32 => As<uint>(left) < As<uint>(right),
    TypeCode.Int32 => As<int>(left) < As<int>(right),
    TypeCode.UInt64 => As<ulong>(left) < As<ulong>(right),
    TypeCode.Int64 => As<long>(left) < As<long>(right),
    TypeCode.Single => As<float>(left) < As<float>(right),
    TypeCode.Double => As<double>(left) < As<double>(right),
    TypeCode.Decimal => As<decimal>(left) < As<decimal>(right),
    _ => ThrowNotSupported<bool>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool LessThanOrEqual(T left, T right) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => As<byte>(left) <= As<byte>(right),
    TypeCode.SByte => As<sbyte>(left) <= As<sbyte>(right),
    TypeCode.UInt16 => As<ushort>(left) <= As<ushort>(right),
    TypeCode.Int16 => As<short>(left) <= As<short>(right),
    TypeCode.UInt32 => As<uint>(left) <= As<uint>(right),
    TypeCode.Int32 => As<int>(left) <= As<int>(right),
    TypeCode.UInt64 => As<ulong>(left) <= As<ulong>(right),
    TypeCode.Int64 => As<long>(left) <= As<long>(right),
    TypeCode.Single => As<float>(left) <= As<float>(right),
    TypeCode.Double => As<double>(left) <= As<double>(right),
    TypeCode.Decimal => As<decimal>(left) <= As<decimal>(right),
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
    TypeCode.SByte => Promote((sbyte)Math.Sign(As<sbyte>(value))),
    TypeCode.Int16 => Promote((short)Math.Sign(As<short>(value))),
    TypeCode.Int32 => Promote(Math.Sign(As<int>(value))),
    TypeCode.Int64 => Promote(Math.Sign(As<long>(value))),
    TypeCode.Single => Promote(Math.Sign(As<float>(value))),
    TypeCode.Double => Promote(Math.Sign(As<double>(value))),
    TypeCode.Decimal => Promote(Math.Sign(As<decimal>(value))),
    TypeCode.Byte => Promote((byte)(As<byte>(value) == 0 ? 0 : 1)),
    TypeCode.UInt16 => Promote((ushort)(As<ushort>(value) == 0 ? 0 : 1)),
    TypeCode.UInt32 => Promote(As<uint>(value) == 0 ? 0u : 1u),
    TypeCode.UInt64 => Promote(As<ulong>(value) == 0 ? 0ul : 1ul),
    _ => ThrowNotSupported<T>()
  };

  #region Helper methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T From(int value) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)value),
    TypeCode.SByte => Promote((sbyte)value),
    TypeCode.UInt16 => Promote((ushort)value),
    TypeCode.Int16 => Promote((short)value),
    TypeCode.UInt32 => Promote((uint)value),
    TypeCode.Int32 => Promote(value),
    TypeCode.UInt64 => Promote((ulong)value),
    TypeCode.Int64 => Promote((long)value),
    TypeCode.Single => Promote((float)value),
    TypeCode.Double => Promote((double)value),
    TypeCode.Decimal => Promote((decimal)value),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static T From(double value) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => Promote((byte)value),
    TypeCode.SByte => Promote((sbyte)value),
    TypeCode.UInt16 => Promote((ushort)value),
    TypeCode.Int16 => Promote((short)value),
    TypeCode.UInt32 => Promote((uint)value),
    TypeCode.Int32 => Promote((int)value),
    TypeCode.UInt64 => Promote((ulong)value),
    TypeCode.Int64 => Promote((long)value),
    TypeCode.Single => Promote((float)value),
    TypeCode.Double => Promote(value),
    TypeCode.Decimal => Promote((decimal)value),
    _ => ThrowNotSupported<T>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double ToDouble(T value) => TypeCodeCache<T>.Code switch {
    TypeCode.Byte => As<byte>(value),
    TypeCode.SByte => As<sbyte>(value),
    TypeCode.UInt16 => As<ushort>(value),
    TypeCode.Int16 => As<short>(value),
    TypeCode.UInt32 => As<uint>(value),
    TypeCode.Int32 => As<int>(value),
    TypeCode.UInt64 => As<ulong>(value),
    TypeCode.Int64 => As<long>(value),
    TypeCode.Single => As<float>(value),
    TypeCode.Double => As<double>(value),
    TypeCode.Decimal => (double)As<decimal>(value),
    _ => ThrowNotSupported<double>()
  };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  private static unsafe T Promote<TFrom>(TFrom value) where TFrom : unmanaged => *(T*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  private static unsafe G As<G>(T value) where G : unmanaged => *(G*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  private static unsafe G As<G,H>(H value) where G : unmanaged where H:unmanaged => *(G*)&value;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type


  [MethodImpl(MethodImplOptions.NoInlining)]
  [DoesNotReturn]
  [StackTraceHidden]
  private static G ThrowNotSupported<G>() => throw new NotSupportedException($"Scalar: The type {typeof(T)} is not supported.");

  #endregion

  // Specialized saturate helpers
  private static T AddSaturateInt64(long left, long right) => Promote(left switch {
    > 0 when right > long.MaxValue - left => long.MaxValue,
    < 0 when right < long.MinValue - left => long.MinValue,
    _ => left + right
  });

  private static T AddSaturateUInt64(ulong left, ulong right) => Promote(
    left > ulong.MaxValue - right ? ulong.MaxValue : left + right
  );

  private static T SubtractSaturateInt64(long left, long right) => Promote(right switch {
    > 0 when left < long.MinValue + right => long.MinValue,
    < 0 when left > long.MaxValue + right => long.MaxValue,
    _ => left - right
  });

  private static T SubtractSaturateUInt32(uint left, uint right) =>
    Promote(left < right ? 0u : left - right);

  private static T SubtractSaturateUInt64(ulong left, ulong right) =>
    Promote(left < right ? 0ul : left - right);

}