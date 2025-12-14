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

// Feature flags:
//   SUPPORTS_BITCONVERTER_FLOAT_CONVERSION: Core 2.0+ - Int32BitsToSingle, SingleToInt32Bits
//   SUPPORTS_BITCONVERTER_SPAN: Std 2.1, Core 2.1+ - Span-based overloads
//   SUPPORTS_BITCONVERTER_UINT_CONVERSION: .NET 6.0+ - UInt32/UInt64 bits conversion

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Polyfills for BitConverter methods missing in older frameworks.
/// </summary>
public static partial class BitConverterPolyfills {

#if !SUPPORTS_BITCONVERTER_FLOAT_CONVERSION

  extension(BitConverter) {

    /// <summary>
    /// Reinterprets the specified 32-bit signed integer as a single-precision floating-point value.
    /// </summary>
    /// <param name="value">The 32-bit signed integer to convert.</param>
    /// <returns>A single-precision floating-point value with the same bit pattern as the specified integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float Int32BitsToSingle(int value) => *(float*)&value;

    /// <summary>
    /// Reinterprets the specified single-precision floating-point value as a 32-bit signed integer.
    /// </summary>
    /// <param name="value">The single-precision floating-point value to convert.</param>
    /// <returns>A 32-bit signed integer with the same bit pattern as the specified floating-point value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int SingleToInt32Bits(float value) => *(int*)&value;

  }

#endif

#if !SUPPORTS_BITCONVERTER_UINT_CONVERSION

  extension(BitConverter) {

    /// <summary>
    /// Reinterprets the specified 32-bit unsigned integer as a single-precision floating-point value.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer to convert.</param>
    /// <returns>A single-precision floating-point value with the same bit pattern as the specified integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float UInt32BitsToSingle(uint value) => *(float*)&value;

    /// <summary>
    /// Reinterprets the specified single-precision floating-point value as a 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The single-precision floating-point value to convert.</param>
    /// <returns>A 32-bit unsigned integer with the same bit pattern as the specified floating-point value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint SingleToUInt32Bits(float value) => *(uint*)&value;

    /// <summary>
    /// Reinterprets the specified 64-bit unsigned integer as a double-precision floating-point value.
    /// </summary>
    /// <param name="value">The 64-bit unsigned integer to convert.</param>
    /// <returns>A double-precision floating-point value with the same bit pattern as the specified integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double UInt64BitsToDouble(ulong value) => *(double*)&value;

    /// <summary>
    /// Reinterprets the specified double-precision floating-point value as a 64-bit unsigned integer.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to convert.</param>
    /// <returns>A 64-bit unsigned integer with the same bit pattern as the specified floating-point value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong DoubleToUInt64Bits(double value) => *(ulong*)&value;

  }

#endif

#if !SUPPORTS_BITCONVERTER_SPAN

  extension(BitConverter) {

    /// <summary>
    /// Returns a Boolean value converted from the byte at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBoolean(ReadOnlySpan<byte> value) => value[0] != 0;

    /// <summary>
    /// Returns a character converted from two bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe char ToChar(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(char*)ptr;
    }

    /// <summary>
    /// Returns a 16-bit signed integer converted from two bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe short ToInt16(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(short*)ptr;
    }

    /// <summary>
    /// Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort ToUInt16(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(ushort*)ptr;
    }

    /// <summary>
    /// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int ToInt32(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(int*)ptr;
    }

    /// <summary>
    /// Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint ToUInt32(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(uint*)ptr;
    }

    /// <summary>
    /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long ToInt64(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(long*)ptr;
    }

    /// <summary>
    /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong ToUInt64(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(ulong*)ptr;
    }

    /// <summary>
    /// Returns a single-precision floating point number converted from four bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float ToSingle(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(float*)ptr;
    }

    /// <summary>
    /// Returns a double-precision floating point number converted from eight bytes at a specified position in a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double ToDouble(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(double*)ptr;
    }

    /// <summary>
    /// Tries to convert a Boolean into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBytes(Span<byte> destination, bool value) {
      if (destination.Length < 1)
        return false;
      destination[0] = value ? (byte)1 : (byte)0;
      return true;
    }

    /// <summary>
    /// Tries to convert a character into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, char value) {
      if (destination.Length < sizeof(char))
        return false;
      fixed (byte* ptr = destination)
        *(char*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 16-bit signed integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, short value) {
      if (destination.Length < sizeof(short))
        return false;
      fixed (byte* ptr = destination)
        *(short*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 16-bit unsigned integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, ushort value) {
      if (destination.Length < sizeof(ushort))
        return false;
      fixed (byte* ptr = destination)
        *(ushort*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 32-bit signed integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, int value) {
      if (destination.Length < sizeof(int))
        return false;
      fixed (byte* ptr = destination)
        *(int*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 32-bit unsigned integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, uint value) {
      if (destination.Length < sizeof(uint))
        return false;
      fixed (byte* ptr = destination)
        *(uint*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 64-bit signed integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, long value) {
      if (destination.Length < sizeof(long))
        return false;
      fixed (byte* ptr = destination)
        *(long*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a 64-bit unsigned integer into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, ulong value) {
      if (destination.Length < sizeof(ulong))
        return false;
      fixed (byte* ptr = destination)
        *(ulong*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a single-precision floating-point value into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, float value) {
      if (destination.Length < sizeof(float))
        return false;
      fixed (byte* ptr = destination)
        *(float*)ptr = value;
      return true;
    }

    /// <summary>
    /// Tries to convert a double-precision floating-point value into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, double value) {
      if (destination.Length < sizeof(double))
        return false;
      fixed (byte* ptr = destination)
        *(double*)ptr = value;
      return true;
    }

  }

#endif

#if !SUPPORTS_BITCONVERTER_HALF

  extension(BitConverter) {

    /// <summary>
    /// Reinterprets the specified 16-bit signed integer value as a half-precision floating-point value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Half Int16BitsToHalf(short value) => *(Half*)&value;

    /// <summary>
    /// Reinterprets the specified half-precision floating-point value as a 16-bit signed integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe short HalfToInt16Bits(Half value) => *(short*)&value;

    /// <summary>
    /// Reinterprets the specified 16-bit unsigned integer value as a half-precision floating-point value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Half UInt16BitsToHalf(ushort value) => *(Half*)&value;

    /// <summary>
    /// Reinterprets the specified half-precision floating-point value as a 16-bit unsigned integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort HalfToUInt16Bits(Half value) => *(ushort*)&value;

    /// <summary>
    /// Returns the specified half-precision floating point value as an array of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte[] GetBytes(Half value) {
      var bytes = new byte[2];
      fixed (byte* ptr = bytes)
        *(Half*)ptr = value;
      return bytes;
    }

    /// <summary>
    /// Returns a half-precision floating point number converted from two bytes at a specified position in a byte array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Half ToHalf(byte[] value, int startIndex) {
      fixed (byte* ptr = &value[startIndex])
        return *(Half*)ptr;
    }

    /// <summary>
    /// Returns a half-precision floating point number converted from two bytes at the beginning of a byte span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Half ToHalf(ReadOnlySpan<byte> value) {
      fixed (byte* ptr = value)
        return *(Half*)ptr;
    }

    /// <summary>
    /// Tries to convert a half-precision floating-point value into a span of bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool TryWriteBytes(Span<byte> destination, Half value) {
      if (destination.Length < 2)
        return false;
      fixed (byte* ptr = destination)
        *(Half*)ptr = value;
      return true;
    }

  }

#endif

}
