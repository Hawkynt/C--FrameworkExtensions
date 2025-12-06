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

#if !SUPPORTS_BITOPERATIONS
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Utility methods for intrinsic bit-twiddling operations.
/// The methods use hardware intrinsics when available on the underlying platform,
/// otherwise they use optimized software fallbacks.
/// </summary>
public static class BitOperations {

  private static readonly int[] TrailingZeroCountDeBruijn = [
    00, 01, 28, 02, 29, 14, 24, 03,
    30, 22, 20, 15, 25, 17, 04, 08,
    31, 27, 13, 23, 21, 19, 16, 07,
    26, 12, 18, 06, 11, 05, 10, 09
  ];

  private static readonly int[] Log2DeBruijn = [
    00, 09, 01, 10, 13, 21, 02, 29,
    11, 14, 16, 18, 22, 25, 03, 30,
    08, 12, 20, 28, 15, 17, 24, 07,
    19, 27, 23, 06, 26, 05, 04, 31
  ];

  /// <summary>Round the given integral value up to a power of 2.</summary>
  /// <param name="value">The value.</param>
  /// <returns>
  /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
  /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RoundUpToPowerOf2(uint value) {

    // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
    --value;
    value |= value >> 1;
    value |= value >> 2;
    value |= value >> 4;
    value |= value >> 8;
    value |= value >> 16;
    return value + 1;
  }

  /// <summary>
  /// Round the given integral value up to a power of 2.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <returns>
  /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
  /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RoundUpToPowerOf2(ulong value) {

    // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
    --value;
    value |= value >> 1;
    value |= value >> 2;
    value |= value >> 4;
    value |= value >> 8;
    value |= value >> 16;
    value |= value >> 32;
    return value + 1;
  }

  /// <summary>
  /// Round the given integral value up to a power of 2.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <returns>
  /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
  /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static nuint RoundUpToPowerOf2(nuint value) => Utilities.Runtime.Is64BitArchitecture ? (nuint)RoundUpToPowerOf2((ulong)value) : RoundUpToPowerOf2((uint)value);

  /// <summary>
  /// Count the number of leading zero bits in a mask.
  /// Similar in behavior to the x86 instruction LZCNT.
  /// </summary>
  /// <notes>
  /// Unguarded fallback contract is 0->31, BSR contract is 0->undefined
  /// </notes>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LeadingZeroCount(uint value) => value == 0 ? 32 : 31 ^ Log2(value);

  /// <summary>
  /// Count the number of leading zero bits in a mask.
  /// Similar in behavior to the x86 instruction LZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LeadingZeroCount(ulong value) => value == 0 ? 64 : 63 ^ Log2(value);

  /// <summary>
  /// Count the number of leading zero bits in a mask.
  /// Similar in behavior to the x86 instruction LZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LeadingZeroCount(nuint value) => Utilities.Runtime.Is64BitArchitecture ? LeadingZeroCount((ulong)value) : LeadingZeroCount((uint)value);

  /// <summary>
  /// Returns the integer (floor) log of the specified value, base 2.
  /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(uint value) {
    if (value == 0)
      return 0;

    value |= value >> 1;
    value |= value >> 2;
    value |= value >> 4;
    value |= value >> 8;
    value |= value >> 16;
    var index = (value * 0x07C4ACDDU) >> 27;
    return Log2DeBruijn[index];
  }

  /// <summary>
  /// Returns the integer (floor) log of the specified value, base 2.
  /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(ulong value) {
    if (value == 0)
      return 0;
    
    var hi = (uint)(value >> 32);
    return hi == 0 
      ? BitOperations.Log2((uint)value) 
      : 32 + BitOperations.Log2(hi)
      ;
  }

  /// <summary>
  /// Returns the integer (floor) log of the specified value, base 2.
  /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Log2(nuint value) => Utilities.Runtime.Is64BitArchitecture ? Log2((ulong)value) : Log2((uint)value);

  /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int Log2Ceiling(uint value) {
    var result = Log2(value);
    if (PopCount(value) != 1)
      ++result;

    return result;
  }

  /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int Log2Ceiling(ulong value) {
    var result = Log2(value);
    if (PopCount(value) != 1)
      ++result;

    return result;
  }

  /// <summary>
  /// Returns the population count (number of bits set) of a mask.
  /// Similar in behavior to the x86 instruction POPCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PopCount(uint value) {
    const uint c1 = 0x_55555555u;
    const uint c2 = 0x_33333333u;
    const uint c3 = 0x_0F0F0F0Fu;
    const uint c4 = 0x_01010101u;

    value -= (value >> 1) & c1;
    value = (value & c2) + ((value >> 2) & c2);
    value = (((value + (value >> 4)) & c3) * c4) >> 24;

    return (int)value;
  }

  /// <summary>
  /// Returns the population count (number of bits set) of a mask.
  /// Similar in behavior to the x86 instruction POPCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PopCount(ulong value) {
    const ulong c1 = 0x_55555555_55555555ul;
    const ulong c2 = 0x_33333333_33333333ul;
    const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
    const ulong c4 = 0x_01010101_01010101ul;

    value -= (value >> 1) & c1;
    value = (value & c2) + ((value >> 2) & c2);
    value = (((value + (value >> 4)) & c3) * c4) >> 56;

    return (int)value;
  }

  /// <summary>
  /// Returns the population count (number of bits set) of a mask.
  /// Similar in behavior to the x86 instruction POPCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int PopCount(nuint value) => Utilities.Runtime.Is64BitArchitecture ? PopCount((ulong)value) : PopCount((uint)value);

  /// <summary>
  /// Count the number of trailing zero bits in an integer value.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(int value) => TrailingZeroCount((uint)value);

  /// <summary>
  /// Count the number of trailing zero bits in an integer value.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(uint value) {
    // Unguarded fallback contract is 0->0, BSF contract is 0->undefined
    if (value == 0)
      return 32;

    var isolated = value & (uint)-(int)value;
    var index = (isolated * 0x077CB531U) >> 27;
    return TrailingZeroCountDeBruijn[index];
  }

  /// <summary>
  /// Count the number of trailing zero bits in a mask.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(long value) => TrailingZeroCount((ulong)value);

  /// <summary>
  /// Count the number of trailing zero bits in a mask.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(ulong value) {
    var lo = (uint)value;

    if (lo == 0)
      return 32 + TrailingZeroCount((uint)(value >> 32));

    return TrailingZeroCount(lo);
  }

  /// <summary>
  /// Count the number of trailing zero bits in a mask.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(nint value) => Utilities.Runtime.Is64BitArchitecture ? TrailingZeroCount((ulong)(nuint)value) : TrailingZeroCount((uint)(nuint)value);

  /// <summary>
  /// Count the number of trailing zero bits in a mask.
  /// Similar in behavior to the x86 instruction TZCNT.
  /// </summary>
  /// <param name="value">The value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int TrailingZeroCount(nuint value) => Utilities.Runtime.Is64BitArchitecture ? TrailingZeroCount((ulong)value) : TrailingZeroCount((uint)value);

  /// <summary>
  /// Rotates the specified value left by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROL.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

  /// <summary>
  /// Rotates the specified value left by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROL.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateLeft(ulong value, int offset) => (value << offset) | (value >> (64 - offset));

  /// <summary>
  /// Rotates the specified value left by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROL.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..31] is treated as congruent mod 32 on a 32-bit process,
  /// and any value outside the range [0..63] is treated as congruent mod 64 on a 64-bit process.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static nuint RotateLeft(nuint value, int offset) => Utilities.Runtime.Is64BitArchitecture ? (nuint)RotateLeft((ulong)value, offset) : RotateLeft((uint)value, offset);

  /// <summary>
  /// Rotates the specified value right by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROR.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint RotateRight(uint value, int offset) => (value >> offset) | (value << (32 - offset));

  /// <summary>
  /// Rotates the specified value right by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROR.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong RotateRight(ulong value, int offset) => (value >> offset) | (value << (64 - offset));

  /// <summary>
  /// Rotates the specified value right by the specified number of bits.
  /// Similar in behavior to the x86 instruction ROR.
  /// </summary>
  /// <param name="value">The value to rotate.</param>
  /// <param name="offset">The number of bits to rotate by.
  /// Any value outside the range [0..31] is treated as congruent mod 32 on a 32-bit process,
  /// and any value outside the range [0..63] is treated as congruent mod 64 on a 64-bit process.</param>
  /// <returns>The rotated value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static nuint RotateRight(nuint value, int offset) => Utilities.Runtime.Is64BitArchitecture ? (nuint)RotateRight((ulong)value, offset) : RotateRight((uint)value, offset);

  /// <summary>
  /// Accumulates the CRC (Cyclic redundancy check) checksum.
  /// </summary>
  /// <param name="crc">The base value to calculate checksum on</param>
  /// <param name="data">The data for which to compute the checksum</param>
  /// <returns>The CRC-checksum</returns>

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32C(uint crc, byte data) => Crc32Fallback.Invoke(crc, data);

  /// <summary>
  /// Accumulates the CRC (Cyclic redundancy check) checksum.
  /// </summary>
  /// <param name="crc">The base value to calculate checksum on</param>
  /// <param name="data">The data for which to compute the checksum</param>
  /// <returns>The CRC-checksum</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32C(uint crc, ushort data) => Crc32Fallback.Invoke(crc, data);

  /// <summary>
  /// Accumulates the CRC (Cyclic redundancy check) checksum.
  /// </summary>
  /// <param name="crc">The base value to calculate checksum on</param>
  /// <param name="data">The data for which to compute the checksum</param>
  /// <returns>The CRC-checksum</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32C(uint crc, uint data) => Crc32Fallback.Invoke(crc, data);

  /// <summary>
  /// Accumulates the CRC (Cyclic redundancy check) checksum.
  /// </summary>
  /// <param name="crc">The base value to calculate checksum on</param>
  /// <param name="data">The data for which to compute the checksum</param>
  /// <returns>The CRC-checksum</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Crc32C(uint crc, ulong data) => Crc32C(Crc32C(crc, (uint)(data)), (uint)(data >> 32));

  private static class Crc32Fallback {
    // CRC-32 transition table.
    // While this implementation is based on the Castagnoli CRC-32 polynomial (CRC-32C),
    // x32 + x28 + x27 + x26 + x25 + x23 + x22 + x20 + x19 + x18 + x14 + x13 + x11 + x10 + x9 + x8 + x6 + x0,
    // this version uses reflected bit ordering, so 0x1EDC6F41 becomes 0x82F63B78u.
    private static readonly uint[] _crcTable = _Generate(0x82F63B78u);

    private static uint[] _Generate(uint polynomial) {
      const int tableSize = 256;
      var table = new uint[tableSize];

      for (uint i = 0; i < tableSize; ++i) {
        var crc = i;
        for (var bit = 0; bit < 8; ++bit) {
          var lowestBitSet = (crc & 1) != 0;
          crc >>= 1;
          if (lowestBitSet)
            crc ^= polynomial;
        }

        table[i] = crc;
      }

      return table;
    }

    internal static uint Invoke(uint crc, byte data) {
      crc = _crcTable[(byte)(crc ^ data)] ^ (crc >> 8);
      return crc;
    }

    internal static uint Invoke(uint crc, ushort data) {
      crc = _crcTable[(byte)(crc ^ (byte)data)] ^ (crc >> 8);
      data >>= 8;
      crc = _crcTable[(byte)(crc ^ (byte)data)] ^ (crc >> 8);
      return crc;
    }

    internal static uint Invoke(uint crc, uint data) => Crc32CCore(crc, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Crc32CCore(uint crc, uint data) {
      crc = _crcTable[(byte)(crc ^ (byte)data)] ^ (crc >> 8);
      data >>= 8;
      crc = _crcTable[(byte)(crc ^ (byte)data)] ^ (crc >> 8);
      data >>= 8;
      crc = _crcTable[(byte)(crc ^ (byte)data)] ^ (crc >> 8);
      data >>= 8;
      crc = _crcTable[(byte)(crc ^ data)] ^ (crc >> 8);
      return crc;
    }
  }

  /// <summary>
  /// Reset the lowest significant bit in the given value
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static uint ResetLowestSetBit(uint value) => value & (value - 1);

  /// <summary>
  /// Reset specific bit in the given value
  /// Reset the lowest significant bit in the given value
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static ulong ResetLowestSetBit(ulong value) => value & (value - 1);

  /// <summary>
  /// Flip the bit at a specific position in a given value.
  /// Similar in behavior to the x86 instruction BTC (Bit Test and Complement).
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="index">The zero-based index of the bit to flip.
  /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
  /// <returns>The new value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static uint FlipBit(uint value, int index) => value ^ (1u << index);

  /// <summary>
  /// Flip the bit at a specific position in a given value.
  /// Similar in behavior to the x86 instruction BTC (Bit Test and Complement).
  /// /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="index">The zero-based index of the bit to flip.
  /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
  /// <returns>The new value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static ulong FlipBit(ulong value, int index) => value ^ (1ul << index);

}

#endif