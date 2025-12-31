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

#if !SUPPORTS_ARMBASE_WAVE1

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.Arm;

/// <summary>
/// Provides a base class for ARM intrinsics with support detection.
/// This is a polyfill for older frameworks where ARM intrinsics are not available.
/// </summary>
public abstract class ArmBase {

  /// <summary>
  /// Gets a value indicating whether ARM base instructions are supported.
  /// </summary>
  public static bool IsSupported => false;

  /// <summary>
  /// Counts the number of leading zero bits.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LeadingZeroCount(int value) => LeadingZeroCount((uint)value);

  /// <summary>
  /// Counts the number of leading zero bits.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LeadingZeroCount(uint value) {
    if (value == 0)
      return 32;

    int count = 0;
    if ((value & 0xFFFF0000) == 0) { count += 16; value <<= 16; }
    if ((value & 0xFF000000) == 0) { count += 8; value <<= 8; }
    if ((value & 0xF0000000) == 0) { count += 4; value <<= 4; }
    if ((value & 0xC0000000) == 0) { count += 2; value <<= 2; }
    if ((value & 0x80000000) == 0) { ++count; }
    return count;
  }

  /// <summary>
  /// Reverses the bit order of a 32-bit integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ReverseElementBits(int value) => (int)ReverseElementBits((uint)value);

  /// <summary>
  /// Reverses the bit order of a 32-bit unsigned integer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseElementBits(uint value) {
    value = ((value >> 1) & 0x55555555) | ((value & 0x55555555) << 1);
    value = ((value >> 2) & 0x33333333) | ((value & 0x33333333) << 2);
    value = ((value >> 4) & 0x0F0F0F0F) | ((value & 0x0F0F0F0F) << 4);
    value = ((value >> 8) & 0x00FF00FF) | ((value & 0x00FF00FF) << 8);
    return (value >> 16) | (value << 16);
  }

  /// <summary>
  /// Provides 64-bit specific ARM base operations.
  /// </summary>
  public abstract class Arm64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit ARM base instructions are supported.
    /// </summary>
    public static bool IsSupported => false;

    /// <summary>
    /// Counts the number of leading sign bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LeadingSignCount(int value) {
      if (value < 0)
        value = ~value;
      return ArmBase.LeadingZeroCount((uint)value) - 1;
    }

    /// <summary>
    /// Counts the number of leading sign bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LeadingSignCount(long value) {
      if (value < 0)
        value = ~value;
      return LeadingZeroCount((ulong)value) - 1;
    }

    /// <summary>
    /// Counts the number of leading zero bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LeadingZeroCount(long value) => LeadingZeroCount((ulong)value);

    /// <summary>
    /// Counts the number of leading zero bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LeadingZeroCount(ulong value) {
      if (value == 0)
        return 64;

      int count = 0;
      if ((value & 0xFFFFFFFF00000000) == 0) { count += 32; value <<= 32; }
      if ((value & 0xFFFF000000000000) == 0) { count += 16; value <<= 16; }
      if ((value & 0xFF00000000000000) == 0) { count += 8; value <<= 8; }
      if ((value & 0xF000000000000000) == 0) { count += 4; value <<= 4; }
      if ((value & 0xC000000000000000) == 0) { count += 2; value <<= 2; }
      if ((value & 0x8000000000000000) == 0) { ++count; }
      return count;
    }

    /// <summary>
    /// Multiplies two 64-bit signed integers and returns the high 64 bits of the 128-bit result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long MultiplyHigh(long left, long right) {
      var isNegative = (left < 0) ^ (right < 0);
      var absLeft = (ulong)(left < 0 ? -left : left);
      var absRight = (ulong)(right < 0 ? -right : right);
      var high = MultiplyHigh(absLeft, absRight);

      if (!isNegative)
        return (long)high;

      // For negative results, we need to adjust
      var low = (ulong)left * (ulong)right;
      if (low != 0)
        high = ~high;
      else
        high = ~high + 1;

      return (long)high;
    }

    /// <summary>
    /// Multiplies two 64-bit unsigned integers and returns the high 64 bits of the 128-bit result.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MultiplyHigh(ulong left, ulong right) {
      // Split into 32-bit parts
      var leftLow = (uint)left;
      var leftHigh = (uint)(left >> 32);
      var rightLow = (uint)right;
      var rightHigh = (uint)(right >> 32);

      // Compute partial products
      var lowLow = (ulong)leftLow * rightLow;
      var lowHigh = (ulong)leftLow * rightHigh;
      var highLow = (ulong)leftHigh * rightLow;
      var highHigh = (ulong)leftHigh * rightHigh;

      // Add middle terms with proper carry handling
      var carry = (lowLow >> 32) + (uint)lowHigh + (uint)highLow;
      var high = highHigh + (lowHigh >> 32) + (highLow >> 32) + (carry >> 32);

      return high;
    }

    /// <summary>
    /// Reverses the bit order of a 64-bit integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReverseElementBits(long value) => (long)ReverseElementBits((ulong)value);

    /// <summary>
    /// Reverses the bit order of a 64-bit unsigned integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReverseElementBits(ulong value) {
      value = ((value >> 1) & 0x5555555555555555UL) | ((value & 0x5555555555555555UL) << 1);
      value = ((value >> 2) & 0x3333333333333333UL) | ((value & 0x3333333333333333UL) << 2);
      value = ((value >> 4) & 0x0F0F0F0F0F0F0F0FUL) | ((value & 0x0F0F0F0F0F0F0F0FUL) << 4);
      value = ((value >> 8) & 0x00FF00FF00FF00FFUL) | ((value & 0x00FF00FF00FF00FFUL) << 8);
      value = ((value >> 16) & 0x0000FFFF0000FFFFUL) | ((value & 0x0000FFFF0000FFFFUL) << 16);
      return (value >> 32) | (value << 32);
    }
  }
}

#endif
