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

#if !SUPPORTS_INTRINSICS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Provides BMI1 (Bit Manipulation Instruction Set 1) intrinsic operations.
/// This is a polyfill for older frameworks where BMI1 intrinsics are not available.
/// </summary>
public abstract class Bmi1 : X86Base {

  /// <summary>
  /// Gets a value indicating whether BMI1 instructions are supported by the hardware.
  /// </summary>
  public new static bool IsSupported => false;

  /// <summary>
  /// Extracts contiguous bits from the source using index and length specified in control.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint BitFieldExtract(uint value, byte start, byte length) {
    var mask = (1u << length) - 1;
    return (value >> start) & mask;
  }

  /// <summary>
  /// Extracts contiguous bits from the source using control value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint BitFieldExtract(uint value, ushort control) {
    var start = (byte)(control & 0xFF);
    var length = (byte)(control >> 8);
    return BitFieldExtract(value, start, length);
  }

  /// <summary>
  /// Extracts the lowest set bit from the source.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ExtractLowestSetBit(uint value) => value & (uint)(-(int)value);

  /// <summary>
  /// Gets the mask up to and including the lowest set bit.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint GetMaskUpToLowestSetBit(uint value) => value ^ (value - 1);

  /// <summary>
  /// Resets the lowest set bit in the source.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ResetLowestSetBit(uint value) => value & (value - 1);

  /// <summary>
  /// Counts the number of trailing zero bits.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint TrailingZeroCount(uint value) {
    if (value == 0)
      return 32;

    uint count = 0;
    while ((value & 1) == 0) {
      value >>= 1;
      ++count;
    }
    return count;
  }

  /// <summary>
  /// Clears all bits higher than the specified bit position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint AndNot(uint left, uint right) => ~left & right;

  /// <summary>
  /// Provides 64-bit specific BMI1 operations.
  /// </summary>
  public new abstract class X64 : X86Base.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit BMI1 instructions are supported.
    /// </summary>
    public new static bool IsSupported => false;

    /// <summary>
    /// Extracts contiguous bits from the source using index and length specified.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitFieldExtract(ulong value, byte start, byte length) {
      var mask = (1ul << length) - 1;
      return (value >> start) & mask;
    }

    /// <summary>
    /// Extracts contiguous bits from the source using control value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BitFieldExtract(ulong value, ushort control) {
      var start = (byte)(control & 0xFF);
      var length = (byte)(control >> 8);
      return BitFieldExtract(value, start, length);
    }

    /// <summary>
    /// Extracts the lowest set bit from the source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ExtractLowestSetBit(ulong value) => value & (ulong)(-(long)value);

    /// <summary>
    /// Gets the mask up to and including the lowest set bit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetMaskUpToLowestSetBit(ulong value) => value ^ (value - 1);

    /// <summary>
    /// Resets the lowest set bit in the source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ResetLowestSetBit(ulong value) => value & (value - 1);

    /// <summary>
    /// Counts the number of trailing zero bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong TrailingZeroCount(ulong value) {
      if (value == 0)
        return 64;

      ulong count = 0;
      while ((value & 1) == 0) {
        value >>= 1;
        ++count;
      }
      return count;
    }

    /// <summary>
    /// Performs a bitwise AND NOT operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AndNot(ulong left, ulong right) => ~left & right;
  }
}

#endif
