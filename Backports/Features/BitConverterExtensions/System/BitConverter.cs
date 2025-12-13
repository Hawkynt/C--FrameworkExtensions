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

#if !SUPPORTS_BITCONVERTER_FLOAT_CONVERSION

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Polyfills for BitConverter methods missing in older frameworks.
/// </summary>
public static class BitConverterPolyfills {

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

#if !SUPPORTS_BITCONVERTER_DOUBLE_INT64_CONVERSION

    /// <summary>
    /// Reinterprets the specified 64-bit signed integer as a double-precision floating-point value.
    /// </summary>
    /// <param name="value">The 64-bit signed integer to convert.</param>
    /// <returns>A double-precision floating-point value with the same bit pattern as the specified integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double Int64BitsToDouble(long value) => *(double*)&value;

    /// <summary>
    /// Reinterprets the specified double-precision floating-point value as a 64-bit signed integer.
    /// </summary>
    /// <param name="value">The double-precision floating-point value to convert.</param>
    /// <returns>A 64-bit signed integer with the same bit pattern as the specified floating-point value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long DoubleToInt64Bits(double value) => *(long*)&value;

#endif

  }

}

#endif
