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
/// Provides POPCNT (Population Count) intrinsic operations.
/// This is a polyfill for older frameworks where POPCNT intrinsics are not available.
/// </summary>
public abstract class Popcnt : Sse42 {

  /// <summary>
  /// Gets a value indicating whether POPCNT instructions are supported by the hardware.
  /// </summary>
  public new static bool IsSupported => false;

  /// <summary>
  /// Counts the number of bits set to 1 in the source.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint PopCount(uint value) {
    value -= (value >> 1) & 0x55555555;
    value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
    value = (value + (value >> 4)) & 0x0F0F0F0F;
    return (value * 0x01010101) >> 24;
  }

  /// <summary>
  /// Provides 64-bit specific POPCNT operations.
  /// </summary>
  public new abstract class X64 : Sse42.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit POPCNT instructions are supported.
    /// </summary>
    public new static bool IsSupported => false;

    /// <summary>
    /// Counts the number of bits set to 1 in the source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong PopCount(ulong value) {
      value -= (value >> 1) & 0x5555555555555555UL;
      value = (value & 0x3333333333333333UL) + ((value >> 2) & 0x3333333333333333UL);
      value = (value + (value >> 4)) & 0x0F0F0F0F0F0F0F0FUL;
      return (value * 0x0101010101010101UL) >> 56;
    }
  }
}

#endif
