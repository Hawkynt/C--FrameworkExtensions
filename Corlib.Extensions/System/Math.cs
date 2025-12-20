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


using System.Collections.Generic;
using System.Numerics;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantCast
// ReSharper disable CompareOfFloatsByEqualityOperator
namespace System;

public static partial class MathEx {

  public const decimal Pi     = 3.14159265358979323846264338327m; // taken from https://3.141592653589793238462643383279502884197169399375105820974944592.eu/
  public const decimal E      = 2.71828182845904523536028747135m; // taken from https://www.mathsisfun.com/numbers/e-eulers-number.html
  public const decimal Sqrt2  = 1.41421356237309504880168872421m; // taken from https://en.wikipedia.org/wiki/Square_root_of_2
  public const decimal Sqrt3  = 1.73205080756887729352744634151m; // taken from https://en.wikipedia.org/wiki/Square_root_of_3
  public const decimal Sqrt5  = 2.23606797749978969640917366873m; // taken from https://en.wikipedia.org/wiki/Square_root_of_5
  public const decimal Sqrt6  = 2.44948974278317809819728407471m; // taken from https://en.wikipedia.org/wiki/Square_root_of_6
  public const decimal Sqrt7  = 2.64575131106459059050161575364m; // taken from https://en.wikipedia.org/wiki/Square_root_of_7
  public const decimal Sqrt8  = 2.82842712474619009760337744842m; // taken from https://en.wikipedia.org/wiki/Square_root#Square_roots_of_positive_integers
  public const decimal Sqrt10 = 3.16227766016837933199889354443m; // taken from https://en.wikipedia.org/wiki/Square_root#Square_roots_of_positive_integers
  public const decimal Ln10   = 2.30258509299404568401799145468m; // taken from https://www.wolframalpha.com/input?i=ln%2810%29
  public const decimal Ln2    = 0.69314718055994530941723212145m; // taken from https://www.wolframalpha.com/input?i=ln%282%29

  private const int MAX_TAN_ITERATIONS = 100;
  private const int MAX_ATAN_ITERATIONS = 100;
  private const int MAX_LOG_ITERATIONS = 100;
  private const int MAX_EXP_ITERATIONS = 1000;
  private const decimal DEFAULT_EPSILON = 1E-28m;

  /// <summary>
  /// Extracts the lower nibble of the specified byte.
  /// </summary>
  /// <param name="this">The byte from which to extract the lower half.</param>
  /// <returns>A byte containing the lower 4 bits of the input byte.</returns>
  /// <example>
  /// <code>
  /// byte value = 0xAB;
  /// byte lowerHalf = value.LowerHalf(); // lowerHalf is 0x0B
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LowerHalf(this byte @this) => (byte)(@this & 0x0F);

  /// <summary>
  /// Extracts the upper nibble of the specified byte.
  /// </summary>
  /// <param name="this">The byte from which to extract the upper half.</param>
  /// <returns>A byte containing the upper 4 bits of the input byte.</returns>
  /// <example>
  /// <code>
  /// byte value = 0xAB;
  /// byte upperHalf = value.UpperHalf(); // upperHalf is 0x0A
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte UpperHalf(this byte @this) => (byte)(@this >> 4);

  /// <summary>
  /// Extracts the lower byte of the specified word.
  /// </summary>
  /// <param name="this">The ushort from which to extract the lower half.</param>
  /// <returns>A byte containing the lower 8 bits of the input ushort.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0xABCD;
  /// byte lowerHalf = value.LowerHalf(); // lowerHalf is 0xCD
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LowerHalf(this ushort @this) => (byte)@this;

  /// <summary>
  /// Extracts the upper byte of the specified word.
  /// </summary>
  /// <param name="this">The ushort from which to extract the upper half.</param>
  /// <returns>A byte containing the upper 8 bits of the input ushort.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0xABCD;
  /// byte upperHalf = value.UpperHalf(); // upperHalf is 0xAB
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte UpperHalf(this ushort @this) => (byte)(@this >> 8);

  /// <summary>
  /// Extracts the lower word of the specified dword.
  /// </summary>
  /// <param name="this">The uint from which to extract the lower half.</param>
  /// <returns>A ushort containing the lower 16 bits of the input uint.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xABCDEF12;
  /// ushort lowerHalf = value.LowerHalf(); // lowerHalf is 0xEF12
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort LowerHalf(this uint @this) => (ushort)@this;

  /// <summary>
  /// Extracts the upper word of the specified dword.
  /// </summary>
  /// <param name="this">The uint from which to extract the upper half.</param>
  /// <returns>A ushort containing the upper 16 bits of the input uint.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xABCDEF12;
  /// ushort upperHalf = value.UpperHalf(); // upperHalf is 0xABCD
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort UpperHalf(this uint @this) => (ushort)(@this >> 16);

  /// <summary>
  /// Extracts the lower dword of the specified qword.
  /// </summary>
  /// <param name="this">The ulong from which to extract the lower half.</param>
  /// <returns>A uint containing the lower 32 bits of the input ulong.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0xABCDEF1234567890;
  /// uint lowerHalf = value.LowerHalf(); // lowerHalf is 0x34567890
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint LowerHalf(this ulong @this) => (uint)@this;

  /// <summary>
  /// Extracts the upper dword of the specified qword.
  /// </summary>
  /// <param name="this">The ulong from which to extract the upper half.</param>
  /// <returns>A uint containing the upper 32 bits of the input ulong.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0xABCDEF1234567890;
  /// uint upperHalf = value.UpperHalf(); // upperHalf is 0xABCDEF12
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint UpperHalf(this ulong @this) => (uint)(@this >> 32);

  /// <summary>
  /// Performs a bitwise left rotation on an 8-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="byte"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate left by.</param>
  /// <returns>The rotated <see cref="byte"/> result.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_00110101;
  /// byte result = value.RotateLeft(3); // result == 0b_10101001
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte RotateLeft(this byte @this, byte count) {
    var result = @this * 0x01010101U;
    result = BitOperations.RotateLeft(result, count);
    return (byte)result;
  }

  /// <summary>
  /// Performs a bitwise right rotation on an 8-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="byte"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate right by.</param>
  /// <returns>The rotated <see cref="byte"/> result.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_00110101;
  /// byte result = value.RotateRight(3); // result == 0b_10100110
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte RotateRight(this byte @this, byte count) {
    var result = @this * 0x01010101U;
    result = BitOperations.RotateRight(result, count);
    return (byte)result;
  }

  /// <summary>
  /// Performs a bitwise left rotation on a 16-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="ushort"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate left by.</param>
  /// <returns>The rotated <see cref="ushort"/> result.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1010_0011_0001_1100;
  /// ushort result = value.RotateLeft(4);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort RotateLeft(this ushort @this, byte count) {
    var result = @this * 0x00010001U;
    result = BitOperations.RotateLeft(result, count);
    return (ushort)result;
  }

  /// <summary>
  /// Performs a bitwise right rotation on a 16-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="ushort"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate right by.</param>
  /// <returns>The rotated <see cref="ushort"/> result.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1010_0011_0001_1100;
  /// ushort result = value.RotateRight(4);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort RotateRight(this ushort @this, byte count) {
    var result = @this * 0x00010001U;
    result = BitOperations.RotateRight(result, count);
    return (ushort)result;
  }

  /// <summary>
  /// Performs a bitwise left rotation on a 32-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="uint"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate left by.</param>
  /// <returns>The rotated <see cref="uint"/> result.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xA5A5A5A5U;
  /// uint result = value.RotateLeft(8);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateLeft(this uint @this, byte count) => BitOperations.RotateLeft(@this, count);

  /// <summary>
  /// Performs a bitwise right rotation on a 32-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="uint"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate right by.</param>
  /// <returns>The rotated <see cref="uint"/> result.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xA5A5A5A5U;
  /// uint result = value.RotateRight(8);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateRight(this uint @this, byte count) => BitOperations.RotateRight(@this, count);

  /// <summary>
  /// Performs a bitwise left rotation on a 64-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="ulong"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate left by.</param>
  /// <returns>The rotated <see cref="ulong"/> result.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0x0123456789ABCDEFUL;
  /// ulong result = value.RotateLeft(16);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateLeft(this ulong @this, byte count) => BitOperations.RotateLeft(@this, count);

  /// <summary>
  /// Performs a bitwise right rotation on a 64-bit unsigned integer.
  /// </summary>
  /// <param name="this">The <see cref="ulong"/> value to rotate.</param>
  /// <param name="count">The number of bits to rotate right by.</param>
  /// <returns>The rotated <see cref="ulong"/> result.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0x0123456789ABCDEFUL;
  /// ulong result = value.RotateRight(16);
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateRight(this ulong @this, byte count) => BitOperations.RotateRight(@this, count);

  /// <summary>
  /// Counts the number of trailing zero bits in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits following the least significant bit. Returns 8 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0010_0000;
  /// byte count = value.TrailingZeroCount(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingZeroCount(this byte @this) => (byte)(@this == 0 ? 8 : BitOperations.TrailingZeroCount((uint)@this));

  /// <summary>
  /// Counts the number of trailing zero bits in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits following the least significant bit. Returns 16 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1111_1100_0010_0000;
  /// byte count = value.TrailingZeroCount(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingZeroCount(this ushort @this) => (byte)(@this == 0 ? 16 : BitOperations.TrailingZeroCount((uint)@this));

  /// <summary>
  /// Counts the number of trailing zero bits in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits following the least significant bit. Returns 32 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b1111_1111_0000_0000_0000_0000_0010_0000;
  /// byte count = value.TrailingZeroCount(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingZeroCount(this uint @this) => (byte)BitOperations.TrailingZeroCount(@this);

  /// <summary>
  /// Counts the number of trailing zero bits in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits following the least significant bit. Returns 64 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_0010_0000UL;
  /// byte count = value.TrailingZeroCount(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingZeroCount(this ulong @this) => (byte)BitOperations.TrailingZeroCount(@this);

  /// <summary>
  /// Counts the number of leading zero bits in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits before the most significant set bit. Returns 8 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0000_0100;
  /// byte count = value.LeadingZeroCount(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingZeroCount(this byte @this) => (byte)(@this == 0 ? 8 : 7 ^ Log2(@this));

  /// <summary>
  /// Counts the number of leading zero bits in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits before the most significant set bit. Returns 16 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_0000_0100;
  /// byte count = value.LeadingZeroCount(); // count == 13
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingZeroCount(this ushort @this) => (byte)(@this == 0 ? 16 : BitOperations.LeadingZeroCount(@this) - 16);

  /// <summary>
  /// Counts the number of leading zero bits in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits before the most significant set bit. Returns 32 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_0000_0100U;
  /// byte count = value.LeadingZeroCount(); // count == 29
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingZeroCount(this uint @this) => (byte)BitOperations.LeadingZeroCount(@this);

  /// <summary>
  /// Counts the number of leading zero bits in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of zero bits before the most significant set bit. Returns 64 if the input is 0.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_0000_0100UL;
  /// byte count = value.LeadingZeroCount(); // count == 61
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingZeroCount(this ulong @this) => (byte)BitOperations.LeadingZeroCount(@this);

  /// <summary>
  /// Counts the number of trailing one bits in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the least significant bit.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0011_1111;
  /// byte count = value.TrailingOneCount(); // count == 6
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingOneCount(this byte @this) => TrailingZeroCount((byte)~@this);

  /// <summary>
  /// Counts the number of trailing one bits in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the least significant bit.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_0011_1111;
  /// byte count = value.TrailingOneCount(); // count == 6
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingOneCount(this ushort @this) => TrailingZeroCount((ushort)~@this);

  /// <summary>
  /// Counts the number of trailing one bits in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the least significant bit.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_0011_1111U;
  /// byte count = value.TrailingOneCount(); // count == 6
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingOneCount(this uint @this) => TrailingZeroCount(~@this);

  /// <summary>
  /// Counts the number of trailing one bits in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the least significant bit.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_0011_1111UL;
  /// byte count = value.TrailingOneCount(); // count == 6
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte TrailingOneCount(this ulong @this) => TrailingZeroCount(~@this);

  /// <summary>
  /// Counts the number of leading one bits in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the most significant bit.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1111_0000;
  /// byte count = value.LeadingOneCount(); // count == 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingOneCount(this byte @this) => LeadingZeroCount((byte)~@this);

  /// <summary>
  /// Counts the number of leading one bits in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the most significant bit.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1111_0000_1010_1010;
  /// byte count = value.LeadingOneCount(); // count == 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingOneCount(this ushort @this) => LeadingZeroCount((ushort)~@this);

  /// <summary>
  /// Counts the number of leading one bits in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the most significant bit.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_1111_0000_1010_1010_1111_0000_1010_1010U;
  /// byte count = value.LeadingOneCount(); // count == 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingOneCount(this uint @this) => LeadingZeroCount(~@this);

  /// <summary>
  /// Counts the number of leading one bits in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to examine.</param>
  /// <returns>The number of consecutive one bits starting from the most significant bit.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_1111_0000_1010_1010_1111_0000_1010_1010_1111_0000_1010_1010_1111_0000_1010_1010UL;
  /// byte count = value.LeadingOneCount(); // count == 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LeadingOneCount(this ulong @this) => LeadingZeroCount(~@this);

  /// <summary>
  /// Counts the number of bits set to <see langword="1"/> in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are set.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1101_0101;
  /// byte count = value.CountSetBits(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this byte @this) => (byte)BitOperations.PopCount(@this);

  /// <summary>
  /// Counts the number of bits set to <see langword="1"/> in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are set.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1101_0101;
  /// byte count = value.CountSetBits(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ushort @this) => (byte)BitOperations.PopCount(@this);

  /// <summary>
  /// Counts the number of bits set to <see langword="1"/> in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are set.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_1101_0101U;
  /// byte count = value.CountSetBits(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this uint @this) => (byte)BitOperations.PopCount(@this);

  /// <summary>
  /// Counts the number of bits set to <see langword="1"/> in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are set.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_1101_0101UL;
  /// byte count = value.CountSetBits(); // count == 5
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountSetBits(this ulong @this) => (byte)BitOperations.PopCount(@this);

  /// <summary>
  /// Counts the number of bits set to <see langword="0"/> in the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are unset.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1101_0101;
  /// byte count = value.CountUnsetBits(); // count == 3
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this byte @this) => (byte)(8 - CountSetBits(@this));

  /// <summary>
  /// Counts the number of bits set to <see langword="0"/> in the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are unset.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1101_0101;
  /// byte count = value.CountUnsetBits(); // count == 11
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this ushort @this) => (byte)(16 - CountSetBits(@this));

  /// <summary>
  /// Counts the number of bits set to <see langword="0"/> in the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are unset.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_1101_0101U;
  /// byte count = value.CountUnsetBits(); // count == 27
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this uint @this) => (byte)(32 - CountSetBits(@this));

  /// <summary>
  /// Counts the number of bits set to <see langword="0"/> in the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to analyze.</param>
  /// <returns>The number of bits that are unset.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_1101_0101UL;
  /// byte count = value.CountUnsetBits(); // count == 59
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte CountUnsetBits(this ulong @this) => (byte)(64 - CountSetBits(@this));

  /// <summary>
  /// Determines whether the number of set bits in the specified <see cref="byte"/> value is odd (odd parity).
  /// </summary>
  /// <param name="value">The value to evaluate.</param>
  /// <returns><see langword="true"/> if the number of bits set to <see langword="1"/> is odd; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1110_0100;
  /// bool parity = value.Parity(); // true (odd number of bits set)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Parity(this byte value) => (CountSetBits(value) & 1) == 0;

  /// <summary>
  /// Determines whether the number of set bits in the specified <see cref="ushort"/> value is odd (odd parity).
  /// </summary>
  /// <param name="value">The value to evaluate.</param>
  /// <returns><see langword="true"/> if the number of bits set to <see langword="1"/> is odd; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_1110_0100;
  /// bool parity = value.Parity(); // true (odd number of bits set)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Parity(this ushort value) => (CountSetBits(value) & 1) == 0;

  /// <summary>
  /// Determines whether the number of set bits in the specified <see cref="uint"/> value is odd (odd parity).
  /// </summary>
  /// <param name="value">The value to evaluate.</param>
  /// <returns><see langword="true"/> if the number of bits set to <see langword="1"/> is odd; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_1110_0100U;
  /// bool parity = value.Parity(); // true (odd number of bits set)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Parity(this uint value) => (CountSetBits(value) & 1) == 0;

  /// <summary>
  /// Determines whether the number of set bits in the specified <see cref="ulong"/> value is odd (odd parity).
  /// </summary>
  /// <param name="value">The value to evaluate.</param>
  /// <returns><see langword="true"/> if the number of bits set to <see langword="1"/> is odd; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_1110_0100UL;
  /// bool parity = value.Parity(); // true (odd number of bits set)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Parity(this ulong value) => (CountSetBits(value) & 1) == 0;

  /// <summary>
  /// Reverses the bit order of the specified <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The value to reverse.</param>
  /// <returns>A <see cref="byte"/> whose bits are in reverse order from the input.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0001_0110;
  /// byte reversed = value.ReverseBits(); // 0b_0110_1000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ReverseBits(this byte @this) => (byte)(((@this * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);

  /// <summary>
  /// Reverses the bit order of the specified <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The value to reverse.</param>
  /// <returns>A <see cref="ushort"/> whose bits are in reverse order from the input.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_0001_0110;
  /// ushort reversed = value.ReverseBits(); // 0b_0110_1000_0000_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ReverseBits(this ushort @this) {
    @this = (ushort)((@this >> 1) & 0x5555 | (@this & 0x5555) << 1);
    @this = (ushort)((@this >> 2) & 0x3333 | (@this & 0x3333) << 2);
    @this = (ushort)((@this >> 4) & 0x0F0F | (@this & 0x0F0F) << 4);
    @this = (ushort)((@this >> 8) | (@this << 8));
    return @this;
  }

  /// <summary>
  /// Reverses the bit order of the specified <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The value to reverse.</param>
  /// <returns>A <see cref="uint"/> whose bits are in reverse order from the input.</returns>
  /// <example>
  /// <code>
  /// uint value = 0b_0001_0110U;
  /// uint reversed = value.ReverseBits(); // 0b_0110_1000_0000_0000_0000_0000_0000_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBits(this uint @this) {
    @this = (@this >> 1) & 0x55555555 | (@this & 0x55555555) << 1;
    @this = (@this >> 2) & 0x33333333 | (@this & 0x33333333) << 2;
    @this = (@this >> 4) & 0x0F0F0F0F | (@this & 0x0F0F0F0F) << 4;
    @this = (@this >> 8) & 0x00FF00FF | (@this & 0x00FF00FF) << 8;
    @this = (@this >> 16) | (@this << 16);
    return @this;
  }

  /// <summary>
  /// Reverses the bit order of the specified <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The value to reverse.</param>
  /// <returns>A <see cref="ulong"/> whose bits are in reverse order from the input.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0b_0001_0110UL;
  /// ulong reversed = value.ReverseBits(); // 0b_0110_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ReverseBits(this ulong @this) {
    @this = (@this >> 1) & 0x5555555555555555UL | (@this & 0x5555555555555555UL) << 1;
    @this = (@this >> 2) & 0x3333333333333333UL | (@this & 0x3333333333333333UL) << 2;
    @this = (@this >> 4) & 0x0F0F0F0F0F0F0F0FUL | (@this & 0x0F0F0F0F0F0F0F0FUL) << 4;
    @this = (@this >> 8) & 0x00FF00FF00FF00FFUL | (@this & 0x00FF00FF00FF00FFUL) << 8;
    @this = (@this >> 16) & 0x0000FFFF0000FFFFUL | (@this & 0x0000FFFF0000FFFFUL) << 16;
    @this = (@this >> 32) | (@this << 32);
    return @this;
  }

  /// <summary>
  /// Extracts bits from the source <see cref="byte"/> according to the specified <paramref name="mask"/> using parallel bit extraction (PEXT).
  /// </summary>
  /// <param name="this">The source value from which to extract bits.</param>
  /// <param name="mask">A bitmask indicating which bits to extract.</param>
  /// <returns>
  /// A compacted <see cref="byte"/> value containing the extracted bits, densely packed in order of increasing bit significance.
  /// </returns>
  /// <remarks>
  /// If the platform supports BMI2 intrinsics, this will use <see cref="System.Runtime.Intrinsics.X86.Bmi2.ParallelBitExtract(uint, uint)"/> for optimal performance.
  /// Otherwise, a software fallback is used.
  /// </remarks>
  /// <example>
  /// <code>
  /// byte source = 0b_1101_0101;
  /// byte mask =   0b_1111_0000;
  /// byte extracted = source.ParallelBitExtract(mask); // extracted == 0b_1101
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ParallelBitExtract(this byte @this, byte mask) {
    if (Bmi2.IsSupported)
      return (byte)Bmi2.ParallelBitExtract(@this, mask);

    var maskedValue = @this & mask;
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;
    
    _ProcessParallelBitExtract(maskedValue, mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 4, ref bitPos, ref result);

    return (byte)result;
  }
  
  /// <summary>
  /// Extracts bits from the source <see cref="ushort"/> according to the specified <paramref name="mask"/> using parallel bit extraction (PEXT).
  /// </summary>
  /// <param name="this">The source value to extract from.</param>
  /// <param name="mask">A bitmask indicating which bits to extract.</param>
  /// <returns>
  /// A compacted <see cref="ushort"/> value containing the selected bits from <paramref name="this"/>, ordered by mask bit significance.
  /// </returns>
  /// <remarks>
  /// Uses <see cref="Bmi2.ParallelBitExtract(uint, uint)"/> if available, else falls back to software implementation.
  /// </remarks>
  /// <example>
  /// <code>
  /// ushort source = 0b_1011_0000_0001_1111;
  /// ushort mask =   0b_1111_0000_0000_0000;
  /// ushort result = source.ParallelBitExtract(mask); // result == 0b_1011
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ParallelBitExtract(this ushort @this, ushort mask) {
    if (Bmi2.IsSupported)
      return (ushort)Bmi2.ParallelBitExtract(@this, mask);

    var maskedValue = @this & mask;
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;

    _ProcessParallelBitExtract(maskedValue, mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 4, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 8, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, mask, 12, ref bitPos, ref result);

    return (ushort)result;
  }

  /// <summary>
  /// Extracts bits from a 32-bit unsigned integer using parallel bit extraction (PEXT).
  /// </summary>
  /// <param name="this">The value to extract bits from.</param>
  /// <param name="mask">A mask identifying which bits to extract and compress.</param>
  /// <returns>
  /// A <see cref="uint"/> with the selected bits packed into the least significant bits.
  /// </returns>
  /// <remarks>
  /// Utilizes <see cref="Bmi2.ParallelBitExtract(uint, uint)"/> on supported hardware, with a software fallback otherwise.
  /// </remarks>
  /// <example>
  /// <code>
  /// uint value = 0b_1010_1010_0000_1111;
  /// uint mask =  0b_0000_1111_0000_0000;
  /// uint result = value.ParallelBitExtract(mask); // result == 0b_1010
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ParallelBitExtract(this uint @this, uint mask) {
    if (Bmi2.IsSupported)
      return Bmi2.ParallelBitExtract(@this, mask);

    var maskedValue = (int)(@this & mask);
    if (maskedValue == 0)
      return 0;

    var result = 0;
    var bitPos = 0;

    _ProcessParallelBitExtract(maskedValue, (int)mask, 0, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 4, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 8, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 12, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 16, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 20, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 24, ref bitPos, ref result);
    _ProcessParallelBitExtract(maskedValue, (int)mask, 28, ref bitPos, ref result);

    return (uint)result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ProcessParallelBitExtract(int maskedValue, int mask, int startBitPos, ref int bitPos, ref int result) {
    var bitA = maskedValue >>> startBitPos;
    var maskA = mask >>> startBitPos;
    var bitB = maskedValue >>> (startBitPos + 1);
    var maskB = mask >>> (startBitPos + 1);
    var bitC = maskedValue >>> (startBitPos + 2);
    var maskC = mask >>> (startBitPos + 2);
    var bitD = maskedValue >>> (startBitPos + 3);
    var maskD = mask >>> (startBitPos + 3);

    bitA &= 1;
    maskA &= 1;
    bitB &= 1;
    maskB &= 1;
    bitC &= 1;
    maskC &= 1;
    bitD &= 1;
    maskD &= 1;

    bitA <<= bitPos;
    bitPos += maskA;

    bitB <<= bitPos;
    bitPos += maskB;

    bitC <<= bitPos;
    bitPos += maskC;

    bitD <<= bitPos;
    bitPos += maskD;

    result |= bitA;
    result |= bitB;
    result |= bitC;
    result |= bitD;
  }

  /// <summary>
  /// Performs parallel bit extraction (PEXT) on a 64-bit unsigned integer using the specified mask.
  /// </summary>
  /// <param name="this">The value from which to extract bits.</param>
  /// <param name="mask">The bitmask defining which bits to extract and how to compress them.</param>
  /// <returns>
  /// A <see cref="ulong"/> value with the masked bits packed into the lower bits of the result.
  /// </returns>
  /// <remarks>
  /// Uses <see cref="Bmi2.X64.ParallelBitExtract(ulong, ulong)"/> if available. If not, falls back to a portable software implementation.
  /// </remarks>
  /// <example>
  /// <code>
  /// ulong value = 0xF0F0_F0F0_0F0F_0F0FUL;
  /// ulong mask =  0x0000_F0F0_0000_0000UL;
  /// ulong result = value.ParallelBitExtract(mask); // result == 0b_1111_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ParallelBitExtract(this ulong @this, ulong mask) {
    if (Bmi2.X64.IsSupported)
      return Bmi2.X64.ParallelBitExtract(@this, mask);

    var result = 0UL;
    if ((@this & mask) != 0) {
      var bitPos = 0;

      // Only iterate over set bits in mask - guaranteed at least one iteration
      do {
        var lowestBit = mask & (~mask + 1); // Isolate lowest set bit
        var bitSetMask = @this & lowestBit; // Isolate this bit in the source value
        var isOneOrZero = bitSetMask == 0 ? 0UL : 1UL; // convert it to 1(set) or 0(unset)
        result |= isOneOrZero << bitPos; // shift it into target position
        ++bitPos; // increase current position in target
        mask ^= lowestBit; // Clear the bit
      } while (mask != 0);
    }

    return result;
  }

  /// <summary>
  /// Separates the bits of the <see cref="byte"/> into two groups: one with bits in odd positions and one with bits in even positions.
  /// </summary>
  /// <param name="this">The value to deinterleave.</param>
  /// <returns>
  /// A tuple where <c>odd</c> contains the bits from positions 0, 2, 4, 6 and <c>even</c> contains the bits from positions 1, 3, 5, 7.
  /// </returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1101_0110;
  /// var (odd, even) = value.DeinterleaveBits(); 
  /// // odd == 0b0000_0100, even == 0b0000_1011
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) DeinterleaveBits(this byte @this) => (
    ParallelBitExtract(@this, 0b01010101),
    ParallelBitExtract(@this, 0b10101010)
  );

  /// <summary>
  /// Deinterleaves the bits of a <see cref="ushort"/> value into two 8-bit streams: bits in even and odd positions.
  /// </summary>
  /// <param name="this">The ushort value to deinterleave.</param>
  /// <returns>
  /// A tuple where <c>odd</c> contains bits from even positions (0,2,4...) and <c>even</c> from odd positions (1,3,5...).
  /// </returns>
  /// <remarks>
  /// Uses <see cref="ParallelBitExtract(ushort, ushort)"/> for splitting interleaved data.
  /// </remarks>
  /// <example>
  /// <code>
  /// ushort value = 0b_1010101010101010;
  /// var (odd, even) = value.DeinterleaveBits(); 
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) DeinterleaveBits(this ushort @this) => (
    (byte)ParallelBitExtract(@this, 0b0101010101010101),
    (byte)ParallelBitExtract(@this, 0b1010101010101010)
  );

  /// <summary>
  /// Deinterleaves the bits of a <see cref="uint"/> into two <see cref="ushort"/> values by separating even and odd bits.
  /// </summary>
  /// <param name="this">The 32-bit value to deinterleave.</param>
  /// <returns>
  /// A tuple with <c>odd</c> and <c>even</c> 16-bit values representing the extracted bit lanes.
  /// </returns>
  /// <remarks>
  /// Internally invokes <see cref="ParallelBitExtract(uint, uint)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// uint value = 0b_01010101_10101010_01010101_10101010;
  /// var (odd, even) = value.DeinterleaveBits();
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (ushort odd, ushort even) DeinterleaveBits(this uint @this) => (
    (ushort)ParallelBitExtract(@this, 0b01010101010101010101010101010101U),
    (ushort)ParallelBitExtract(@this, 0b10101010101010101010101010101010U)
  );

  /// <summary>
  /// Deinterleaves a 64-bit unsigned integer into two 32-bit streams by extracting alternating bits.
  /// </summary>
  /// <param name="this">The 64-bit value to process.</param>
  /// <returns>
  /// A tuple of <see cref="uint"/> values containing bits from even and odd positions respectively.
  /// </returns>
  /// <remarks>
  /// Uses <see cref="ParallelBitExtract(ulong, ulong)"/> for optimized extraction.
  /// </remarks>
  /// <example>
  /// <code>
  /// ulong value = 0xAAAAAAAA55555555UL;
  /// var (odd, even) = value.DeinterleaveBits();
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (uint odd, uint even) DeinterleaveBits(this ulong @this) => (
    (uint)ParallelBitExtract(@this, 0b0101010101010101010101010101010101010101010101010101010101010101UL),
    (uint)ParallelBitExtract(@this, 0b1010101010101010101010101010101010101010101010101010101010101010UL)
  );

  /// <summary>
  /// Deinterleaves the <see cref="byte"/> value into two groups of adjacent bit pairs.
  /// </summary>
  /// <param name="this">The source value to deinterleave.</param>
  /// <returns>
  /// A tuple where <c>odd</c> contains all bits from pairs starting at positions 0, 2, 4, 6, and <c>even</c> contains the other half (1, 3, 5, 7).
  /// </returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1100_1010;
  /// var (odd, even) = value.PairwiseDeinterleaveBits(); 
  /// // odd == 0b_0000_0010, even == 0b_0000_0101
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) PairwiseDeinterleaveBits(this byte @this) => (
    ParallelBitExtract(@this, 0b00110011),
    ParallelBitExtract(@this, 0b11001100)
  );

  /// <summary>
  /// Deinterleaves a <see cref="ushort"/> into two 8-bit streams of paired bits using 2-bit wide masking.
  /// </summary>
  /// <param name="this">The ushort to deinterleave.</param>
  /// <returns>
  /// A tuple with <c>odd</c> and <c>even</c> 8-bit values of interleaved bit-pair lanes.
  /// </returns>
  /// <remarks>
  /// Internally calls <see cref="ParallelBitExtract(ushort, ushort)"/> with alternating pair masks.
  /// </remarks>
  /// <example>
  /// <code>
  /// ushort value = 0b_1100110011001100;
  /// var (odd, even) = value.PairwiseDeinterleaveBits(); 
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte odd, byte even) PairwiseDeinterleaveBits(this ushort @this) => (
    (byte)ParallelBitExtract(@this, 0b0011001100110011),
    (byte)ParallelBitExtract(@this, 0b1100110011001100)
  );

  /// <summary>
  /// Deinterleaves a <see cref="uint"/> value into two <see cref="ushort"/> values by extracting pairwise bit lanes.
  /// </summary>
  /// <param name="this">The 32-bit source value.</param>
  /// <returns>
  /// A tuple containing <c>odd</c> and <c>even</c> interleaved 2-bit chunks.
  /// </returns>
  /// <remarks>
  /// Uses <see cref="ParallelBitExtract(uint, uint)"/> internally for optimized execution.
  /// </remarks>
  /// <example>
  /// <code>
  /// uint value = 0b_00110011_11001100_00110011_11001100;
  /// var (odd, even) = value.PairwiseDeinterleaveBits();
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (ushort odd, ushort even) PairwiseDeinterleaveBits(this uint @this) => (
    (ushort)ParallelBitExtract(@this, 0b00110011001100110011001100110011U),
    (ushort)ParallelBitExtract(@this, 0b11001100110011001100110011001100U)
  );

  /// <summary>
  /// Deinterleaves a 64-bit unsigned integer into two 32-bit streams by splitting adjacent bit pairs.
  /// </summary>
  /// <param name="this">The 64-bit value to deinterleave.</param>
  /// <returns>
  /// A tuple with <c>odd</c> and <c>even</c> 32-bit values representing extracted 2-bit groups.
  /// </returns>
  /// <remarks>
  /// Uses <see cref="ParallelBitExtract(ulong, ulong)"/> with interleaved 2-bit pair masks.
  /// </remarks>
  /// <example>
  /// <code>
  /// ulong value = 0xCC33CC33CC33CC33UL;
  /// var (odd, even) = value.PairwiseDeinterleaveBits();
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (uint odd, uint even) PairwiseDeinterleaveBits(this ulong @this) => (
    (uint)ParallelBitExtract(@this, 0b0011001100110011001100110011001100110011001100110011001100110011UL),
    (uint)ParallelBitExtract(@this, 0b1100110011001100110011001100110011001100110011001100110011001100UL)
  );

  /// <summary>
  /// Toggles (inverts) the bit at the specified index in the <see cref="byte"/> value.
  /// </summary>
  /// <param name="this">The source byte.</param>
  /// <param name="index">The bit index (0–7). Values outside the range are masked with 7.</param>
  /// <returns>A new <see cref="byte"/> with the specified bit flipped.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0000_0001;
  /// byte result = value.FlipBit(0); // result == 0b_0000_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte FlipBit(this byte @this, byte index) => (byte)(@this ^ (1U << (index & 7)));

  /// <summary>
  /// Toggles (inverts) the bit at the specified index in the <see cref="ushort"/> value.
  /// </summary>
  /// <param name="this">The source ushort.</param>
  /// <param name="index">The bit index (0–15). Values outside the range are masked with 15.</param>
  /// <returns>A new <see cref="ushort"/> with the specified bit flipped.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0b_0000_0000_0000_0001;
  /// ushort result = value.FlipBit(0); // result == 0b_0000_0000_0000_0000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort FlipBit(this ushort @this, byte index) => (ushort)(@this ^ (1U << (index & 15)));

  /// <summary>
  /// Toggles (inverts) the bit at the specified index in the <see cref="uint"/> value.
  /// </summary>
  /// <param name="this">The source uint.</param>
  /// <param name="index">The bit index (0–31). Values outside the range are masked with 31.</param>
  /// <returns>A new <see cref="uint"/> with the specified bit flipped.</returns>
  /// <example>
  /// <code>
  /// uint value = 1U;
  /// uint result = value.FlipBit(0); // result == 0U
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint FlipBit(this uint @this, byte index) => @this ^ (1U << (index & 31));

  /// <summary>
  /// Toggles (inverts) the bit at the specified index in the <see cref="ulong"/> value.
  /// </summary>
  /// <param name="this">The source ulong.</param>
  /// <param name="index">The bit index (0–63). Values outside the range are masked with 63.</param>
  /// <returns>A new <see cref="ulong"/> with the specified bit flipped.</returns>
  /// <example>
  /// <code>
  /// ulong value = 1UL;
  /// ulong result = value.FlipBit(0); // result == 0UL
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong FlipBit(this ulong @this, byte index) => @this ^ (1UL << (index & 63));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this byte @this, byte index) => (@this & (1U << (index & 7))) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this ushort @this, byte index) => (@this & (1U << (index & 15))) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this uint @this, byte index) => (@this & (1U << (index & 31))) != 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetBit(this ulong @this, byte index) => (@this & (1U << (index & 63))) != 0;

  /// <summary>
  /// Sets the bit at the specified index in the <see cref="byte"/> to 1.
  /// </summary>
  /// <param name="this">The source byte.</param>
  /// <param name="index">The bit index (0–7). Values outside the range are masked with 7.</param>
  /// <returns>A new <see cref="byte"/> with the specified bit set.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_0000_0000;
  /// byte result = value.SetBit(1); // result == 0b_0000_0010
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetBit(this byte @this, byte index) => (byte)(@this | (1U << (index & 7)));

  /// <summary>
  /// Sets the bit at the specified index in the <see cref="ushort"/> to 1.
  /// </summary>
  /// <param name="this">The source ushort.</param>
  /// <param name="index">The bit index (0–15). Values outside the range are masked with 15.</param>
  /// <returns>A new <see cref="ushort"/> with the specified bit set.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0;
  /// ushort result = value.SetBit(3); // result == 0b_0000_0000_0000_1000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort SetBit(this ushort @this, byte index) => (ushort)(@this | (1U << (index & 15)));

  /// <summary>
  /// Sets the bit at the specified index in the <see cref="uint"/> to 1.
  /// </summary>
  /// <param name="this">The source uint.</param>
  /// <param name="index">The bit index (0–31). Values outside the range are masked with 31.</param>
  /// <returns>A new <see cref="uint"/> with the specified bit set.</returns>
  /// <example>
  /// <code>
  /// uint value = 0U;
  /// uint result = value.SetBit(30); // result == 1U &lt;&lt; 30
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint SetBit(this uint @this, byte index) => @this | (1U << (index & 31));

  /// <summary>
  /// Sets the bit at the specified index in the <see cref="ulong"/> to 1.
  /// </summary>
  /// <param name="this">The source ulong.</param>
  /// <param name="index">The bit index (0–63). Values outside the range are masked with 63.</param>
  /// <returns>A new <see cref="ulong"/> with the specified bit set.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0UL;
  /// ulong result = value.SetBit(63); // result == 0x8000000000000000
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong SetBit(this ulong @this, byte index) => @this | (1UL << (index & 63));

  /// <summary>
  /// Clears (sets to 0) the bit at the specified index in the <see cref="byte"/>.
  /// </summary>
  /// <param name="this">The source byte.</param>
  /// <param name="index">The bit index (0–7). Values outside the range are masked with 7.</param>
  /// <returns>A new <see cref="byte"/> with the specified bit cleared.</returns>
  /// <example>
  /// <code>
  /// byte value = 0b_1111_1111;
  /// byte result = value.ClearBit(2); // result == 0b_1111_1011
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ClearBit(this byte @this, byte index) => (byte)(@this & ~(1U << (index & 7)));

  /// <summary>
  /// Clears (sets to 0) the bit at the specified index in the <see cref="ushort"/>.
  /// </summary>
  /// <param name="this">The source ushort.</param>
  /// <param name="index">The bit index (0–15). Values outside the range are masked with 15.</param>
  /// <returns>A new <see cref="ushort"/> with the specified bit cleared.</returns>
  /// <example>
  /// <code>
  /// ushort value = 0xFFFF;
  /// ushort result = value.ClearBit(15); // result == 0x7FFF
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort ClearBit(this ushort @this, byte index) => (ushort)(@this & ~(1U << (index & 15)));

  /// <summary>
  /// Clears (sets to 0) the bit at the specified index in the <see cref="uint"/>.
  /// </summary>
  /// <param name="this">The source uint.</param>
  /// <param name="index">The bit index (0–31). Values outside the range are masked with 31.</param>
  /// <returns>A new <see cref="uint"/> with the specified bit cleared.</returns>
  /// <example>
  /// <code>
  /// uint value = 0xFFFFFFFF;
  /// uint result = value.ClearBit(0); // result == 0xFFFFFFFE
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ClearBit(this uint @this, byte index) => @this & ~(1U << (index & 31));

  /// <summary>
  /// Clears (sets to 0) the bit at the specified index in the <see cref="ulong"/>.
  /// </summary>
  /// <param name="this">The source ulong.</param>
  /// <param name="index">The bit index (0–63). Values outside the range are masked with 63.</param>
  /// <returns>A new <see cref="ulong"/> with the specified bit cleared.</returns>
  /// <example>
  /// <code>
  /// ulong value = 0xFFFFFFFFFFFFFFFFUL;
  /// ulong result = value.ClearBit(63); // result == 0x7FFFFFFFFFFFFFFFUL
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong ClearBit(this ulong @this, byte index) => @this & ~(1UL << (index & 63));

  /// <summary>
  /// Performs linear interpolation between two byte values using a normalized byte parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, normalized to byte range (0 = <paramref name="this"/>, 255 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// byte start = 100;
  /// byte end = 200;
  /// byte half = start.Lerp(end, 128); // result ≈ 150 (halfway point)
  /// byte quarter = start.Lerp(end, 64); // result ≈ 125 (quarter point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Lerp(this byte @this, byte b, byte t) => (byte)(@this + (b - @this) * t / byte.MaxValue);

  /// <summary>
  /// Performs linear interpolation between two ushort values using a normalized ushort parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, normalized to ushort range (0 = <paramref name="this"/>, 65535 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ushort start = 1000;
  /// ushort end = 9000;
  /// ushort half = start.Lerp(end, 32768); // result ≈ 5000 (halfway point)
  /// ushort third = start.Lerp(end, 21845); // result ≈ 3667 (one-third point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort Lerp(this ushort @this, ushort b, ushort t) {
    var invT = (uint)ushort.MaxValue - t;
    return (ushort)((@this * invT + (uint)b * t) / ushort.MaxValue);
  }

  /// <summary>
  /// Performs linear interpolation between two uint values using a normalized uint parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, normalized to uint range (0 = <paramref name="this"/>, 4294967295 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// uint start = 1000000;
  /// uint end = 9000000;
  /// uint half = start.Lerp(end, 2147483648); // result ≈ 5000000 (halfway point)
  /// uint tenth = start.Lerp(end, 429496730); // result ≈ 1800000 (one-tenth point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Lerp(this uint @this, uint b, uint t) {
    var invT = (ulong)uint.MaxValue - t;
    return (uint)((@this * invT + (ulong)b * t) / uint.MaxValue);
  }

  /// <summary>
  /// Performs linear interpolation between two ulong values using a normalized ulong parameter.
  /// Uses 128-bit precision arithmetic to avoid overflow.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, normalized to ulong range (0 = <paramref name="this"/>, 18446744073709551615 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ulong start = 1000000000000UL;
  /// ulong end = 9000000000000UL;
  /// ulong half = start.Lerp(end, 9223372036854775808UL); // result ≈ 5000000000000 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Lerp(this ulong @this, ulong b, ulong t) {
    // direction mask (all-ones if a>b)
    var gt = @this > b ? 1UL : 0UL;
    var m = 0UL - gt;

    // swap so that aa <= bb
    var d = (@this ^ b) & m;
    var aa = @this ^ d;
    var bb = b ^ d;
    var du = bb - aa;               // distance >= 0

    // 64x64 -> 128 multiply (du * t)
    var x0 = (uint)du;
    var x1 = (uint)(du >> 32);
    var y0 = (uint)t;
    var y1 = (uint)(t >> 32);

    var p00 = (ulong)x0 * y0;
    var p01 = (ulong)x0 * y1;
    var p10 = (ulong)x1 * y0;
    var p11 = (ulong)x1 * y1;

    var mid = p01 + p10;
    var carryMid = mid < p01 ? 1UL : 0UL;          // carry from p01+p10

    var midL = mid << 32;
    var lo = p00 + midL;
    var c0 = lo < p00 ? 1UL : 0UL;

    // include (carryMid << 32) into hi
    var hi = p11 + (mid >> 32) + c0 + (carryMid << 32);

    // q = floor((du*t) / (2^64 - 1)) using Mersenne property
    var geM = lo >= (ulong.MaxValue - hi) ? 1UL : 0UL; // (hi+lo) >= M ?
    var q = hi + geM;

    // apply direction (add if a<=b, subtract if a>b)
    var add = @this + q;
    var sub = @this - q;
    return (add & ~m) | (sub & m);
  }

  /// <summary>
  /// Performs linear interpolation between two byte values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// byte start = 100;
  /// byte end = 200;
  /// byte half = start.Lerp(end, 0.5f); // result = 150 (halfway point)
  /// byte quarter = start.Lerp(end, 0.25f); // result = 125 (quarter point)
  /// byte clamped = start.Lerp(end, 1.5f); // result = 200 (clamped to 1.0)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Lerp(this byte @this, byte b, float t) => (byte)(@this + (b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two byte values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// byte start = 100;
  /// byte end = 200;
  /// byte precise = start.Lerp(end, 0.333); // result ≈ 133 (one-third point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Lerp(this byte @this, byte b, double t) => (byte)(@this + (b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two byte values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// byte start = 100;
  /// byte end = 200;
  /// byte extrapolated = start.LerpUnclamped(end, 1.5f); // result = 250 (50% beyond end)
  /// byte negative = start.LerpUnclamped(end, -0.5f); // result = 50 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LerpUnclamped(this byte @this, byte b, float t) => (byte)(@this + (b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two byte values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// byte start = 100;
  /// byte end = 200;
  /// byte extrapolated = start.LerpUnclamped(end, 1.25); // result = 225 (25% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte LerpUnclamped(this byte @this, byte b, double t) => (byte)(@this + (b - @this) * t);

  /// <summary>
  /// Performs linear interpolation between two sbyte values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// sbyte start = -50;
  /// sbyte end = 50;
  /// sbyte result = start.Lerp(end, 0.5f); // result = 0 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte Lerp(this sbyte @this, sbyte b, float t) => (sbyte)(@this + (b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two sbyte values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// sbyte start = -50;
  /// sbyte end = 50;
  /// sbyte result = start.Lerp(end, 0.25); // result = -25 (quarter point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte Lerp(this sbyte @this, sbyte b, double t) => (sbyte)(@this + (b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two sbyte values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// sbyte start = -50;
  /// sbyte end = 50;
  /// sbyte extrapolated = start.LerpUnclamped(end, 1.5f); // result = 100 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte LerpUnclamped(this sbyte @this, sbyte b, float t) => (sbyte)(@this + (b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two sbyte values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// sbyte start = -50;
  /// sbyte end = 50;
  /// sbyte extrapolated = start.LerpUnclamped(end, -0.5); // result = -100 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static sbyte LerpUnclamped(this sbyte @this, sbyte b, double t) => (sbyte)(@this + (b - @this) * t);

  /// <summary>
  /// Performs linear interpolation between two ushort values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ushort start = 1000;
  /// ushort end = 2000;
  /// ushort result = start.Lerp(end, 0.75f); // result = 1750 (three-quarters point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort Lerp(this ushort @this, ushort b, float t) => (ushort)(@this + (b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two ushort values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ushort start = 1000;
  /// ushort end = 2000;
  /// ushort result = start.Lerp(end, 0.333); // result ≈ 1333 (one-third point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort Lerp(this ushort @this, ushort b, double t) => (ushort)(@this + (b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two ushort values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// ushort start = 1000;
  /// ushort end = 2000;
  /// ushort extrapolated = start.LerpUnclamped(end, 1.5f); // result = 2500 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort LerpUnclamped(this ushort @this, ushort b, float t) => (ushort)(@this + (b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two ushort values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// ushort start = 1000;
  /// ushort end = 2000;
  /// ushort extrapolated = start.LerpUnclamped(end, -0.25); // result = 750 (25% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ushort LerpUnclamped(this ushort @this, ushort b, double t) => (ushort)(@this + (b - @this) * t);

  /// <summary>
  /// Performs linear interpolation between two short values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// short start = -1000;
  /// short end = 1000;
  /// short result = start.Lerp(end, 0.5f); // result = 0 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short Lerp(this short @this, short b, float t) => (short)(@this + (b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two short values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// short start = -1000;
  /// short end = 1000;
  /// short result = start.Lerp(end, 0.666); // result ≈ 332 (two-thirds point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short Lerp(this short @this, short b, double t) => (short)(@this + (b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two short values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// short start = -1000;
  /// short end = 1000;
  /// short extrapolated = start.LerpUnclamped(end, 1.5f); // result = 2000 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short LerpUnclamped(this short @this, short b, float t) => (short)(@this + (b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two short values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// short start = -1000;
  /// short end = 1000;
  /// short extrapolated = start.LerpUnclamped(end, -0.5); // result = -2000 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static short LerpUnclamped(this short @this, short b, double t) => (short)(@this + (b - @this) * t);

  /// <summary>
  /// Performs linear interpolation between two uint values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// uint start = 1000000;
  /// uint end = 2000000;
  /// uint result = start.Lerp(end, 0.75f); // result = 1750000 (three-quarters point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Lerp(this uint @this, uint b, float t) => (uint)(@this + (long)(b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two uint values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// uint start = 1000000;
  /// uint end = 2000000;
  /// uint result = start.Lerp(end, 0.333); // result ≈ 1333000 (one-third point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Lerp(this uint @this, uint b, double t) => (uint)(@this + (long)(b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two uint values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// uint start = 1000000;
  /// uint end = 2000000;
  /// uint extrapolated = start.LerpUnclamped(end, 1.5f); // result = 2500000 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint LerpUnclamped(this uint @this, uint b, float t) => (uint)(@this + (long)(b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two uint values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// uint start = 1000000;
  /// uint end = 2000000;
  /// uint extrapolated = start.LerpUnclamped(end, -0.25); // result = 750000 (25% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint LerpUnclamped(this uint @this, uint b, double t) => (uint)(@this + (long)(b - @this) * t);

  /// <summary>
  /// Performs linear interpolation between two int values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// int start = -500000;
  /// int end = 500000;
  /// int result = start.Lerp(end, 0.5f); // result = 0 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Lerp(this int @this, int b, float t) => (int)(@this + (long)(b - @this) * t.ClampUnchecked(0f, 1f));

  /// <summary>
  /// Performs linear interpolation between two int values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// int start = -500000;
  /// int end = 500000;
  /// int result = start.Lerp(end, 0.8); // result = 300000 (80% towards end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Lerp(this int @this, int b, double t) => (int)(@this + (long)(b - @this) * t.ClampUnchecked(0.0, 1.0));

  /// <summary>
  /// Performs unclamped linear interpolation between two int values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// int start = -500000;
  /// int end = 500000;
  /// int extrapolated = start.LerpUnclamped(end, 1.5f); // result = 1000000 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LerpUnclamped(this int @this, int b, float t) => (int)(@this + (long)(b - @this) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two int values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// int start = -500000;
  /// int end = 500000;
  /// int extrapolated = start.LerpUnclamped(end, -0.5); // result = -1000000 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LerpUnclamped(this int @this, int b, double t) => (int)(@this + (long)(b - @this) * t);
  
  /// <summary>
  /// Performs linear interpolation between two ulong values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ulong start = 1000000000UL;
  /// ulong end = 2000000000UL;
  /// ulong result = start.Lerp(end, 0.5f); // result = 1500000000 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Lerp(this ulong @this, ulong b, float t) {
    var clampedT = t.ClampUnchecked(0f, 1f);
    return b >= @this ?
        @this + (ulong)((b - @this) * clampedT) :
        @this - (ulong)((@this - b) * clampedT);
  }

  /// <summary>
  /// Performs linear interpolation between two ulong values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// ulong start = 1000000000UL;
  /// ulong end = 2000000000UL;
  /// ulong result = start.Lerp(end, 0.25); // result = 1250000000 (quarter point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong Lerp(this ulong @this, ulong b, double t) {
    var clampedT = t.ClampUnchecked(0.0, 1.0);
    return b >= @this ?
        @this + (ulong)((b - @this) * clampedT) :
        @this - (ulong)((@this - b) * clampedT);
  }

  /// <summary>
  /// Performs unclamped linear interpolation between two ulong values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// ulong start = 1000000000UL;
  /// ulong end = 2000000000UL;
  /// ulong extrapolated = start.LerpUnclamped(end, 1.5f); // result = 2500000000 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong LerpUnclamped(this ulong @this, ulong b, float t) =>
      b >= @this ?
          @this + (ulong)((b - @this) * t) :
          @this - (ulong)((@this - b) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two ulong values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// ulong start = 1000000000UL;
  /// ulong end = 2000000000UL;
  /// ulong extrapolated = start.LerpUnclamped(end, -0.5); // result = 500000000 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong LerpUnclamped(this ulong @this, ulong b, double t) =>
      b >= @this ?
          @this + (ulong)((b - @this) * t) :
          @this - (ulong)((@this - b) * t);

  /// <summary>
  /// Performs linear interpolation between two long values using a floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// long start = -1000000000L;
  /// long end = 1000000000L;
  /// long result = start.Lerp(end, 0.5f); // result = 0 (halfway point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long Lerp(this long @this, long b, float t) {
    var clampedT = t.ClampUnchecked(0f, 1f);
    return b >= @this ?
        @this + (long)((ulong)(b - @this) * clampedT) :
        @this - (long)((ulong)(@this - b) * clampedT);
  }

  /// <summary>
  /// Performs linear interpolation between two long values using a double-precision floating-point parameter.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter, clamped to [0, 1] range (0 = <paramref name="this"/>, 1 = <paramref name="b"/>).</param>
  /// <returns>The interpolated value between <paramref name="this"/> and <paramref name="b"/>.</returns>
  /// <example>
  /// <code>
  /// long start = -1000000000L;
  /// long end = 1000000000L;
  /// long result = start.Lerp(end, 0.75); // result = 500000000 (three-quarters point)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long Lerp(this long @this, long b, double t) {
    var clampedT = t.ClampUnchecked(0.0, 1.0);
    return b >= @this ?
        @this + (long)((ulong)(b - @this) * clampedT) :
        @this - (long)((ulong)(@this - b) * clampedT);
  }

  /// <summary>
  /// Performs unclamped linear interpolation between two long values using a floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// long start = -1000000000L;
  /// long end = 1000000000L;
  /// long extrapolated = start.LerpUnclamped(end, 1.5f); // result = 2000000000 (50% beyond end)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long LerpUnclamped(this long @this, long b, float t) =>
      b >= @this ?
          @this + (long)((ulong)(b - @this) * t) :
          @this - (long)((ulong)(@this - b) * t);

  /// <summary>
  /// Performs unclamped linear interpolation between two long values using a double-precision floating-point parameter.
  /// Unlike Lerp, this method does not clamp the parameter to [0, 1] range, allowing extrapolation.
  /// </summary>
  /// <param name="this">The starting value.</param>
  /// <param name="b">The ending value.</param>
  /// <param name="t">The interpolation parameter (0 = <paramref name="this"/>, 1 = <paramref name="b"/>, values outside [0,1] extrapolate).</param>
  /// <returns>The interpolated/extrapolated value.</returns>
  /// <example>
  /// <code>
  /// long start = -1000000000L;
  /// long end = 1000000000L;
  /// long extrapolated = start.LerpUnclamped(end, -0.5); // result = -2000000000 (50% before start)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long LerpUnclamped(this long @this, long b, double t) =>
      b >= @this ?
          @this + (long)((ulong)(b - @this) * t) :
          @this - (long)((ulong)(@this - b) * t);

  /// <summary>
  /// Raises a decimal base to a decimal exponent with refined precision.
  /// </summary>
  /// <param name="this">The base value.</param>
  /// <param name="exponent">The exponent value.</param>
  /// <param name="epsilon">(Optional) The maximum allowed deviation in the result.</param>
  /// <returns><c>@this</c> raised to the power of <c>exponent</c></returns>
  /// <exception cref="ArgumentOutOfRangeException">If base is &lt;= 0 or epsilon is negative</exception>
  /// <remarks>
  /// Uses identity: a^b = exp(b * ln(a)), refined with Newton iteration.
  /// </remarks>
  /// <example>
  /// <code>
  /// decimal value = 2;
  /// decimal result = value.Pow(3); // result == 8
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Pow(this decimal @this, decimal exponent, decimal epsilon = 0) {
    Against.NegativeValues(epsilon);

    return exponent switch {
      0 => 1,
      1 => @this,
      < 0 when @this == 0 => AlwaysThrow.ArgumentOutOfRangeException<decimal>(nameof(@this), "Can not raise 0 to a negative power"),
      _ when @this == 1 => 1,
      _ when exponent == decimal.Truncate(exponent) && exponent is >= int.MinValue and <= int.MaxValue => Calculate(@this, (int)exponent),
      _ => Exp(exponent * Log(@this, epsilon: epsilon), epsilon: epsilon)
    };

    static decimal Calculate(decimal baseValue, int exponent) {
      switch (exponent) {
        case 0:
          return 1;
        case < 0:
          return 1 / Calculate(baseValue, -exponent);
        default:
          decimal result = 1;
          while (exponent > 0) {
            if ((exponent & 1) != 0)
              result *= baseValue;

            baseValue *= baseValue;
            exponent >>= 1;
          }
          return result;
      }
    }
  }

  /// <summary>
  ///   Calculate a more accurate square root, see
  ///   http://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
  /// </summary>
  /// <param name="this">The x.</param>
  /// <param name="epsilon">The epsilon.</param>
  /// <returns>The square root of x</returns>
  public static decimal Sqrt(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(@this);
    Against.NegativeValues(epsilon);

    return @this switch {
      0 => 0,
      1 => 1,
      2 => Sqrt2,
      3 => Sqrt3,
      4 => 2,
      5 => Sqrt5,
      6 => Sqrt6,
      7 => Sqrt7,
      8 => Sqrt8,
      9 => 3,
      10 => Sqrt10,
      _ => Calculate()
    };

    decimal Calculate() {
      var current = (decimal)Math.Sqrt((double)@this);
      decimal previous;
      const decimal factor = 2m;

      do {
        previous = current;
        current = (previous + @this / previous) / factor;
      } while (Math.Abs(previous - current) > epsilon);

      return current;
    }
  }

  public static decimal Sin(this decimal x, decimal epsilon = 0m) => Cos(x - Pi / 2m, epsilon);
  
  public static decimal Cos(this decimal x, decimal epsilon = 0m) {
    x = ReduceAngle(x);

    var result = 1m;
    var term = 1m;
    var x2 = x * x;
    var sign = -1;

    for (var n = 2; n < MAX_TAN_ITERATIONS; n += 2) {
      term *= x2 / ((n - 1) * n);
      var delta = term * sign;
      result += delta;

      if (Math.Abs(delta) < epsilon)
        break;

      sign = -sign;
    }

    return result;

    static decimal ReduceAngle(decimal x) {
      const decimal TwoPi = 2 * Pi;

      x %= TwoPi;

      // Reduce to [-π, π]
      if (x < -Pi)
        x += TwoPi;
      if (x > Pi)
        x -= TwoPi;

      return x;
    }
  }

  /// <summary>
  /// Computes the tangent of the specified <see langword="decimal"/> angle in radians.
  /// </summary>
  /// <param name="this">The angle in radians.</param>
  /// <param name="epsilon">(Optional: defaults to <c>0</c>) The maximum error tolerance for refinement using a power series.</param>
  /// <returns>The tangent of the angle as a <see cref="decimal"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="epsilon"/> is negative.</exception>
  /// <remarks>
  /// This implementation starts with a double-based approximation and refines the result using a truncated Maclaurin series.
  /// </remarks>
  /// <example>
  /// <code>
  /// decimal angle = Math.PIm / 4; // 45 degrees in radians
  /// decimal tan = angle.Tan();   // ≈ 1.0
  /// </code>
  /// </example>
  public static decimal Tan(this decimal @this, decimal epsilon = 0) {
    var cos = Cos(@this, epsilon / 10);
    if (cos == 0)
      throw new DivideByZeroException("tan(x) undefined at odd multiples of π/2");

    return Sin(@this, epsilon / 10) / cos;
  }

  /// <summary>
  /// Computes the arctangent (inverse tangent) of the specified <see langword="decimal"/> value.
  /// </summary>
  /// <param name="this">The tangent value to invert.</param>
  /// <param name="epsilon">(Optional: defaults to <c>0</c>) The maximum allowed deviation between successive approximations.</param>
  /// <returns>The angle in radians whose tangent is equal to the input value.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="epsilon"/> is negative.</exception>
  /// <remarks>
  /// Uses an adaptive Newton-Raphson approach for refinement based on <see cref="Math.Tan"/> and the identity <c>atan(x) = sign(x) * π/2 - atan(1 / |x|)</c> when <c>|x| &gt; 1</c>.
  /// </remarks>
  /// <example>
  /// <code>
  /// decimal value = 1m;
  /// decimal angle = value.Atan(); // ≈ π / 4
  /// </code>
  /// </example>
  public static decimal Atan(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(epsilon);

    const decimal PI_OVER_2 = Pi/2;
    return @this switch {
      0m => 0m,
      > 1m => PI_OVER_2 - Atan(1m / @this, epsilon),
      < -1m => -PI_OVER_2 - Atan(1m / @this, epsilon),
      _ => Calculate()
    };

    decimal Calculate() {
      var current = (decimal)Math.Atan((double)@this);
      decimal previous;
      var iterations = 0;
      do {
        previous = current;
        var tanY = Tan(current, epsilon);
        var sec2Y = 1m + tanY * tanY; // sec^2(y) = 1 + tan^2(y)
        current -= (tanY - @this) / sec2Y;
      }
      while (Math.Abs(current - previous) > epsilon && ++iterations < MAX_ATAN_ITERATIONS);

      return current;
    }
  }

  /// <summary>
  /// Returns the largest integral value less than or equal to the specified single-precision floating-point number.
  /// </summary>
  /// <param name="this">The <see langword="float"/> value to round down.</param>
  /// <returns>The largest integer less than or equal to <paramref name="this"/>.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Floor(float)"/> when available; otherwise falls back to <see cref="Math.Floor(double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float value = 3.7f;
  /// float result = value.Floor(); // result = 3.0f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Floor(this float @this) => MathF.Floor(@this);

  /// <summary>
  /// Returns the smallest integral value greater than or equal to the specified single-precision floating-point number.
  /// </summary>
  /// <param name="this">The <see langword="float"/> value to round up.</param>
  /// <returns>The smallest integer greater than or equal to <paramref name="this"/>.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Ceiling(float)"/> when available; otherwise falls back to <see cref="Math.Ceiling(double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float value = 3.2f;
  /// float result = value.Ceiling(); // result = 4.0f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Ceiling(this float @this) => MathF.Ceiling(@this);

  /// <summary>
  /// Calculates the integral part of the specified single-precision floating-point number by removing any fractional digits.
  /// </summary>
  /// <param name="this">The <see langword="float"/> value to truncate.</param>
  /// <returns>The integral part of <paramref name="this"/>, rounded toward zero.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Truncate(float)"/> when available; otherwise falls back to <see cref="Math.Truncate(double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float value = -3.9f;
  /// float result = value.Truncate(); // result = -3.0f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Truncate(this float @this) => MathF.Truncate(@this);

  /// <summary>Rounds a value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <returns>
  ///   The integer nearest the <paramref name="this" /> parameter. If the fractional component of
  ///   <paramref name="this" /> is halfway between two integers, one of which is even and the other odd, the even number is
  ///   returned.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this) => MathF.Round(@this);

  /// <summary>Rounds a value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="digits">The number of decimal places in the return value.</param>
  /// <returns>
  ///   The integer nearest the <paramref name="this" /> parameter. If the fractional component of
  ///   <paramref name="this" /> is halfway between two integers, one of which is even and the other odd, the even number is
  ///   returned.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this, int digits) {
    Against.ValuesOutOfRange(digits, 0, 15);

    return MathF.Round(@this, digits);
  }

  /// <summary>Rounds a value to an integer using the specified rounding convention.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="method">One of the enumeration values that specifies which rounding strategy to use.</param>
  /// <returns>The integer that <paramref name="this" /> is rounded to.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this, MidpointRounding method) => MathF.Round(@this, method);

  /// <summary>Rounds a value to an integer using the specified rounding convention.</summary>
  /// <param name="this">A number to be rounded.</param>
  /// <param name="digits">The number of decimal places in the return value.</param>
  /// <param name="method">One of the enumeration values that specifies which rounding strategy to use.</param>
  /// <returns>The integer that <paramref name="this" /> is rounded to.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Round(this float @this, int digits, MidpointRounding method) {
    Against.ValuesOutOfRange(digits, 0, 15);
    Against.UnknownEnumValues(method);

    return MathF.Round(@this, digits, method);
  }

  /// <summary>
  /// Calculates the logarithm of a specified number in a specified base.
  /// </summary>
  /// <param name="this">The number whose logarithm is to be found.</param>
  /// <param name="base">(Base) The base of the logarithm.</param>
  /// <returns>The logarithm of <paramref name="this"/> in the specified <paramref name="base"/>.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Log(float, float)"/> if available; otherwise falls back to <see cref="Math.Log(double, double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float result = 8f.Log(2f); // result = 3.0f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LogN(this float @this, float @base) => MathF.Log(@this, @base);

  /// <summary>
  /// Calculates the logarithm of a specified number in a specified base.
  /// </summary>
  /// <param name="this">The number whose logarithm is to be found.</param>
  /// <param name="base">(Base) The base of the logarithm.</param>
  /// <returns>The logarithm of <paramref name="this"/> in the specified <paramref name="base"/>.</returns>
  /// <example>
  /// <code>
  /// double result = 100.0.Log(10.0); // result = 2.0
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double LogN(this double @this, double @base) => Math.Log(@this, @base);

  /// <summary>
  /// Computes the logarithm with an arbitrary base using the change of base formula.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <param name="base">The logarithm base.</param>
  /// <param name="epsilon">Precision threshold (0 = maximum precision).</param>
  /// <returns>The logarithm of the input value with the specified base.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal LogN(this decimal @this, decimal @base, decimal epsilon = 0) {
    Against.NegativeValuesAndZero(@this);
    Against.NegativeValuesAndZero(@base);
    Against.ValuesAreEqual(@base, 1m);

    return @this.Log(epsilon: epsilon) / @base.Log(epsilon: epsilon);
  }

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  /// <param name="this">The value to test.</param>
  /// <returns><see langword="true"/> if <paramref name="this"/> is a power of two; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// Returns false for zero. Valid powers of two include values like 1, 2, 4, 8, etc.
  /// </remarks>
  /// <example>
  /// <code>
  /// bool isPower = ((byte)8).IsPowerOfTwo(); // true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPowerOfTwo(this byte @this) => IsPowerOfTwo((uint)@this);

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  /// <param name="this">The value to test.</param>
  /// <returns><see langword="true"/> if <paramref name="this"/> is a power of two; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// Returns false for zero. Valid powers of two include values like 1, 2, 4, 8, etc.
  /// </remarks>
  /// <example>
  /// <code>
  /// bool isPower = ((ushort)8).IsPowerOfTwo(); // true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPowerOfTwo(this ushort @this) => IsPowerOfTwo((uint)@this);

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  /// <param name="this">The value to test.</param>
  /// <returns><see langword="true"/> if <paramref name="this"/> is a power of two; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// Returns false for zero. Valid powers of two include values like 1, 2, 4, 8, etc.
  /// </remarks>
  /// <example>
  /// <code>
  /// bool isPower = 8U.IsPowerOfTwo(); // true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPowerOfTwo(this uint @this) => @this != 0 && (@this & (@this - 1)) == 0;

  /// <summary>
  /// Determines whether the specified value is a power of two.
  /// </summary>
  /// <param name="this">The value to test.</param>
  /// <returns><see langword="true"/> if <paramref name="this"/> is a power of two; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// Returns false for zero. Valid powers of two include values like 1, 2, 4, 8, etc.
  /// </remarks>
  /// <example>
  /// <code>
  /// bool isPower = 8UL.IsPowerOfTwo(); // true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPowerOfTwo(this ulong @this) => @this != 0 && (@this & (@this - 1)) == 0;

  /// <summary>
  /// Computes e^x using Taylor series expansion.
  /// </summary>
  /// <param name="this">The exponent value.</param>
  /// <param name="epsilon">Precision threshold (0 = maximum precision).</param>
  /// <returns>e raised to the power of the input value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Exp(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValues(epsilon);

    switch (@this) {
      case 0m:
        return 1m;
      case > 0 when @this.Truncate() == @this: {
        var value = E;
        while (@this-- > 1)
          value *= E;

        return value;
      }
    }


    epsilon = epsilon == 0 ? DEFAULT_EPSILON : epsilon;

    // Taylor series: e^x = Σ(x^n / n!) for n=0 to ∞
    var term = 1m;
    var result = 1m;
    var n = 1;

    while (Math.Abs(term) > epsilon && n <= MAX_EXP_ITERATIONS) {
      term *= @this / n;
      result += term;
      ++n;
    }

    return result;
  }

  /// <summary>
  /// Computes the natural logarithm (ln) of a decimal value using Newton-Raphson iteration.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <param name="epsilon">Precision threshold (0 = maximum precision).</param>
  /// <returns>The natural logarithm of the input value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Log(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValuesAndZero(@this);
    Against.NegativeValues(epsilon);

    if (@this == 1m)
      return 0m;

    // Start with double approximation
    var current = (decimal)Math.Log((double)@this);
    decimal previous;
    var iteration = 0;

    // Newton-Raphson: x_{n+1} = x_n - (e^x_n - target) / e^x_n
    do {
      previous = current;
      var exp = Exp(previous, epsilon);
      current -= (exp - @this) / exp;
    } while (Math.Abs(current - previous) > epsilon && ++iteration < MAX_LOG_ITERATIONS);

    return current;
  }

  /// <summary>
  /// Computes the base-10 logarithm using the change of base formula.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <param name="epsilon">Precision threshold (0 = maximum precision).</param>
  /// <returns>The base-10 logarithm of the input value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Log10(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValuesAndZero(@this);
    return Log(@this, epsilon) / Ln10;
  }

  /// <summary>
  /// Computes the integer base-2 logarithm of the specified value.
  /// </summary>
  /// <param name="this">The value to compute the log base-2 for.</param>
  /// <returns>The base-2 logarithm of the highest set bit position.</returns>
  /// <remarks>
  /// Uses <see cref="System.Numerics.BitOperations.Log2(uint)"/> or <see cref="System.Numerics.BitOperations.Log2(ulong)"/> depending on type.
  /// </remarks>
  /// <example>
  /// <code>
  /// int exponent = ((byte)16).Log2(); // result = 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(this byte @this) => BitOperations.Log2(@this);

  /// <summary>
  /// Computes the integer base-2 logarithm of the specified value.
  /// </summary>
  /// <param name="this">The value to compute the log base-2 for.</param>
  /// <returns>The base-2 logarithm of the highest set bit position.</returns>
  /// <remarks>
  /// Uses <see cref="System.Numerics.BitOperations.Log2(uint)"/> or <see cref="System.Numerics.BitOperations.Log2(ulong)"/> depending on type.
  /// </remarks>
  /// <example>
  /// <code>
  /// int exponent = ((ushort)16).Log2(); // result = 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(this ushort @this) => BitOperations.Log2(@this);

  /// <summary>
  /// Computes the integer base-2 logarithm of the specified value.
  /// </summary>
  /// <param name="this">The value to compute the log base-2 for.</param>
  /// <returns>The base-2 logarithm of the highest set bit position.</returns>
  /// <remarks>
  /// Uses <see cref="System.Numerics.BitOperations.Log2(uint)"/> or <see cref="System.Numerics.BitOperations.Log2(ulong)"/> depending on type.
  /// </remarks>
  /// <example>
  /// <code>
  /// int exponent = 16U.Log2(); // result = 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(this uint @this) => BitOperations.Log2(@this);

  /// <summary>
  /// Computes the integer base-2 logarithm of the specified value.
  /// </summary>
  /// <param name="this">The value to compute the log base-2 for.</param>
  /// <returns>The base-2 logarithm of the highest set bit position.</returns>
  /// <remarks>
  /// Uses <see cref="System.Numerics.BitOperations.Log2(uint)"/> or <see cref="System.Numerics.BitOperations.Log2(ulong)"/> depending on type.
  /// </remarks>
  /// <example>
  /// <code>
  /// int exponent = 16UL.Log2(); // result = 4
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(this ulong @this) => BitOperations.Log2(@this);

  /// <summary>
  /// Calculates the logarithm base 2 of the specified <see langword="float"/> value.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <returns>The base-2 logarithm of the value.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Log(float, float)"/> if supported; otherwise falls back to <see cref="Math.Log(double, double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float result = 32f.Log2(); // result ≈ 5.0f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Log2(this float @this) => MathF.Log(@this, 2);

  /// <summary>
  /// Calculates the logarithm base 2 of the specified <see langword="double"/> value.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <returns>The base-2 logarithm of the value.</returns>
  /// <example>
  /// <code>
  /// double result = 64.0.Log2(); // result = 6.0
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Log2(this double @this) => Math.Log(@this, 2);

  /// <summary>
  /// Computes the base-2 logarithm using the change of base formula.
  /// </summary>
  /// <param name="this">The value to compute the logarithm for.</param>
  /// <param name="epsilon">Precision threshold (0 = maximum precision).</param>
  /// <returns>The base-2 logarithm of the input value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Log2(this decimal @this, decimal epsilon = 0) {
    Against.NegativeValuesAndZero(@this);
    return Log(@this, epsilon) / Ln2;
  }

  /// <summary>
  /// Returns e raised to the power of the specified <see langword="float"/> value.
  /// </summary>
  /// <param name="this">A number specifying a power.</param>
  /// <returns>The number e raised to the power <paramref name="this"/>.</returns>
  /// <remarks>
  /// Uses <see cref="MathF.Exp(float)"/> if available; otherwise falls back to <see cref="Math.Exp(double)"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// float result = 1f.Exp(); // result ≈ 2.71828f
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Exp(this float @this) => MathF.Exp(@this);

  /// <summary>
  /// Returns e raised to the power of the specified <see langword="double"/> value.
  /// </summary>
  /// <param name="this">A number specifying a power.</param>
  /// <returns>The number e raised to the power <paramref name="this"/>.</returns>
  /// <example>
  /// <code>
  /// double result = 2.0.Exp(); // result ≈ 7.38906
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Exp(this double @this) => Math.Exp(@this);

  /// <summary>
  ///   Calculates the cubic root.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cbrt(this float @this) => @this < 0 ? -MathF.Pow(-@this, 1f / 3) : MathF.Pow(@this, 1f / 3);

  /// <summary>
  ///   Calculates the cubic root.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Cbrt(this double @this) => @this < 0 ? -Math.Pow(-@this, 1d / 3) : Math.Pow(@this, 1d / 3);

  /// <summary>
  ///   Calculates the cubic root.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static decimal Cbrt(this decimal @this) => @this switch {
    > 0 => @this.Pow(1m / 3),
    0 => 0,
    < 0 => -(-@this).Pow(1m / 3)
  };

  /// <summary>
  ///   Calculates the cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Cot(this double @this) => Math.Cos(@this) / Math.Sin(@this);

  /// <summary>
  ///   Calculates the hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Coth(this double @this) {
    var ex = Math.Exp(@this);
    var em = 1 / ex;
    return (ex + em) / (ex - em);
  }

  /// <summary>
  ///   Calculates the cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Csc(this double @this) => 1 / Math.Sin(@this);

  /// <summary>
  ///   Calculates the hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Csch(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex - 1 / ex);
  }

  /// <summary>
  ///   Calculates the secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Sec(this double @this) => 1 / Math.Cos(@this);

  /// <summary>
  ///   Calculates the hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Sech(this double @this) {
    var ex = Math.Exp(@this);
    return 2 / (ex + 1 / ex);
  }

  /// <summary>
  ///   Calculates the area hyperbolic sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arsinh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this + 1));

  /// <summary>
  ///   Calculates the area hyperbolic cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcosh(this double @this) => Math.Log(@this + Math.Sqrt(@this * @this - 1));

  /// <summary>
  ///   Calculates the area hyperbolic tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Artanh(this double @this) => 0.5d * Math.Log((1 + @this) / (1 - @this));

  /// <summary>
  ///   Calculates the area hyperbolic cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcoth(this double @this) => 0.5d * Math.Log((@this + 1) / (@this - 1));

  /// <summary>
  ///   Calculates the area hyperbolic secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arsech(this double @this) => Math.Log((1 + Math.Sqrt(1 - @this * @this)) / @this);

  /// <summary>
  ///   Calculates the area hyperbolic cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Arcsch(this double @this) => Math.Log((1 + Math.Sqrt(1 + @this * @this)) / @this);

  /// <summary>
  ///   Calculates the arcus sine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Asin(this double @this) => Math.Asin(@this);

  /// <summary>
  ///   Calculates the arcus cosine.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acos(this double @this) => Math.Acos(@this);

  /// <summary>
  ///   Calculates the arcus tangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Atan(this double @this) => Math.Atan(@this);

  /// <summary>
  ///   Calculates the arcus cotangent.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acot(this double @this) => Math.Atan(1 / @this);

  /// <summary>
  ///   Calculates the arcus secant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Asec(this double @this) => Math.Acos(1 / @this);

  /// <summary>
  ///   Calculates the arcus cosecant.
  /// </summary>
  /// <param name="this">This value.</param>
  /// <returns>Calculation result</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acsc(this double @this) => Math.Asin(1 / @this);

  public static bool IsPrime(this ulong candidate) {
    switch (candidate) {
      case < 2:
        return false;
      case 2:
        return true;
      case var _ when (candidate & 1) == 0: 
        return false;
    }

    // Phase 1: trial division with small primes up to ~100
    // Hardcoded to avoid allocation
    Span<ulong> smallPrimes = [
      3, 5, 7, 11, 13, 17, 19, 23,
      29, 31, 37, 41, 43, 47, 53, 59,
      61, 67, 71, 73, 79, 83, 89, 97
    ];

    foreach (var p in smallPrimes) {
      if (candidate == p)
        return true;
      if (candidate % p == 0)
        return false;
    }

    // Phase 2: odd trial division up to sqrt(candidate)
    var sqrt = (ulong)Math.Sqrt(candidate);
    for (ulong i = 101; i <= sqrt; i += 2)
      if (candidate % i == 0)
        return false;

    return true;
  }
  
  /// <summary>
  /// Enumerates all prime numbers in the ulong number space starting from 2 using a three-phase approach:
  /// <list type="number">
  /// <item><description>Bit-sieve for small odd numbers</description></item>
  /// <item><description>Buffer-assisted generation using known primes</description></item>
  /// <item><description>Naive primality testing beyond the buffer's range</description></item>
  /// </list>
  /// </summary>
  /// <remarks>
  /// May optionally color output if <c>COLOR_PRIME_GENERATION</c> is defined.
  /// Uses asynchronous tasks if <c>SUPPORTS_ASYNC</c> is defined.
  /// </remarks>
  /// <returns>A sequence of prime numbers as <see cref="ulong"/>.</returns>
  public static IEnumerable<ulong> EnumeratePrimes {
    get {
      return Enumerate();
      
      static IEnumerable<ulong> Enumerate() {
#if COLOR_PRIME_GENERATION
        Console.ForegroundColor = ConsoleColor.White;
#endif
        yield return 2;

        var buffer = new ulong[128];
        PrimeSieve sieve = new(buffer);
        KnownPrimesStorage knownPrimes = new(buffer);

#if COLOR_PRIME_GENERATION
        Console.ForegroundColor = ConsoleColor.Cyan;
#endif
        foreach (var prime in sieve.Enumerate()) {
          yield return prime;
          knownPrimes.Add(prime);
        }

#if COLOR_PRIME_GENERATION
        Console.ForegroundColor = ConsoleColor.Green;
#endif
        foreach (var prime in knownPrimes.Enumerate())
          yield return prime;

#if COLOR_PRIME_GENERATION
        Console.ForegroundColor = ConsoleColor.Red;
#endif
        foreach (var prime in EnumerateSlowPrimesWithKnowns())
          yield return prime;

        IEnumerable<ulong> EnumerateSlowPrimesWithKnowns() {
          var largestKnownPrime = buffer[^1];

          // Start from the square of the last known prime plus 2 (to ensure it's odd)
          var candidate = largestKnownPrime * largestKnownPrime + 2;

          var task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT, candidate);
          for (;;) {
            task.Wait();
            var isPrime = task.Result;

            if (isPrime)
              yield return candidate;

            // Ensure we only check odd numbers
            var next = candidate + 2;
            if (next < candidate)
              yield break; // we're at the end of the ulong range

            candidate = next;
            task = Task.Factory.StartNew(IsPrimeWithBufferAndBeyondT, candidate);
          }

        }

        bool IsPrimeWithBufferAndBeyondT(object state) => IsPrimeWithBufferAndBeyond((ulong)state);

        bool IsPrimeWithBufferAndBeyond(ulong candidate) {
          // 1. Check divisibility with all primes in the buffer
          // ReSharper disable once LoopCanBeConvertedToQuery
          foreach (var prime in buffer)
            if (candidate % prime == 0)
              return false;

          // 2. If none of the primes in the buffer divide the candidate, 
          //    check divisibility with numbers (only odd ones) up to the square root of the candidate
          var sqrtCandidate = (ulong)Math.Sqrt(candidate);
          for (var i = buffer[^1] + 2; i <= sqrtCandidate; i += 2)
            if (candidate % i == 0)
              return false;

          return true;
        }
      }
    }
  }

  /// <summary>
  /// Implements a bit-based sieve of Eratosthenes for odd numbers only,
  /// optimized for incremental prime generation.
  /// </summary>
  private readonly struct PrimeSieve(ulong[] values) {

    /// <summary>
    /// Enumerates prime numbers detected by the sieve, yielding each prime immediately
    /// and marking its multiples as composite in the background (if async is enabled).
    /// </summary>
    /// <returns>A sequence of odd primes discovered by the sieve.</returns>
    public IEnumerable<ulong> Enumerate() {
      ulong prime = 3;
      var values1 = values;
      for (var i = 0; i < values1.Length; ++i, prime += 2) {
        if (values1[i] != 0)
          continue;

        var task = this._FillSieveAsync(prime);
        yield return prime;
        task.Wait();
      }
    }

    private Task _FillSieveAsync(ulong prime) => Task.Factory.StartNew(this._FillSieveAction, prime);
    private void _FillSieveAction(object state) => this._FillSieveAction((ulong)state);

    private void _FillSieveAction(ulong prime) {
      var doublePrime = prime << 1;
      var values1 = values;
      var maxNumberInSieve = ((ulong)values1.Length << 1) + 3;
      for (var j = prime * prime; j < maxNumberInSieve; j += doublePrime)
        values1[(int)((j - 3) >> 1)] = j;
    }
  }

  /// <summary>
  /// Holds a buffer of known primes and enables generation of further primes
  /// based on previously discovered values. Once full, it uses a bounded square check
  /// before transitioning to the next phase.
  /// </summary>
  private struct KnownPrimesStorage(ulong[] primes) {
    private int _index;

    // no checks because only used internally and guaranteed to have a primes!=null && primes.Length > 0 

    private bool _IsSpaceInBufferLeft() => this._index < primes.Length;

    /// <summary>
    /// Adds a new prime to the internal buffer. Assumes space is available.
    /// </summary>
    /// <param name="prime">The prime number to add.</param>
    // no checks because we guarantee, that all calls occur while there is still space in the array
    public void Add(ulong prime) => primes[this._index++] = prime;

    /// <summary>
    /// Enumerates primes by first filling the internal buffer and then
    /// continuing generation with bounded range checks based on the square of the largest prime.
    /// </summary>
    /// <returns>A sequence of prime numbers extending beyond the initial sieve.</returns>
    public IEnumerable<ulong> Enumerate() {
      foreach (var prime in this._GenerateAndFillBuffer())
        yield return prime;

#if COLOR_PRIME_GENERATION
      Console.ForegroundColor = ConsoleColor.Yellow;
#endif

      foreach (var prime in this._EnumerateWithFullBuffer())
        yield return prime;
    }

    /// <summary>
    /// Generates and fills the buffer with subsequent prime numbers starting
    /// from the last known prime. Uses trial division against the current buffer.
    /// </summary>
    /// <returns>A partial sequence of primes up to the buffer capacity.</returns>
    private IEnumerable<ulong> _GenerateAndFillBuffer() {
      // array always valid
      var primes1 = primes;

      // array always contains at least one prime from the sieve
      var lastKnownPrime = primes1[this._index - 1];

      var task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
      for (;;) {
        task.Wait();
        lastKnownPrime = task.Result;
        this.Add(lastKnownPrime);
        if (this._IsSpaceInBufferLeft()) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithPartiallyFilledBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
    }

    private ulong _FindNextPrimeWithPartiallyFilledBuffer(object state) => this._FindNextPrimeWithPartiallyFilledBuffer((ulong)state);

    private ulong _FindNextPrimeWithPartiallyFilledBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do
        candidate += 2;
      while (!this._IsPrimeWithPartiallyFilledBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithPartiallyFilledBuffer(ulong candidate) {
      for (var i = 0; i < this._index; ++i)
        if (candidate % primes[i] == 0)
          return false;

      return true;
    }

    /// <summary>
    /// Continues prime generation using full buffer content as reference until the square
    /// of the last prime is reached. Useful for precomputing future candidates.
    /// </summary>
    /// <returns>A sequence of primes within the precomputed square limit.</returns>
    private IEnumerable<ulong> _EnumerateWithFullBuffer() {
      var lastKnownPrime = primes[^1];
      var upperPrimeSquare = lastKnownPrime * lastKnownPrime;

      var task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
      for (var candidate = lastKnownPrime + 2; candidate <= upperPrimeSquare; candidate = task.Result) {
        task.Wait();
        lastKnownPrime = task.Result;

        if (lastKnownPrime <= upperPrimeSquare) {
          task = Task.Factory.StartNew(this._FindNextPrimeWithFullBuffer, lastKnownPrime);
          yield return lastKnownPrime;
        } else {
          yield return lastKnownPrime;
          yield break;
        }
      }
    }

    private ulong _FindNextPrimeWithFullBuffer(object state) => this._FindNextPrimeWithFullBuffer((ulong)state);

    private ulong _FindNextPrimeWithFullBuffer(ulong lastKnownPrime) {
      var candidate = lastKnownPrime;
      do
        candidate += 2;
      while (!this._IsPrimeWithFullBuffer(candidate));

      return candidate;
    }

    private bool _IsPrimeWithFullBuffer(ulong candidate) {
      foreach (var prime in primes)
        if (candidate % prime == 0)
          return false;

      return true;
    }
  }

}
