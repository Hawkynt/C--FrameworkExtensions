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
/// Provides LZCNT (Leading Zero Count) intrinsic operations.
/// This is a polyfill for older frameworks where LZCNT intrinsics are not available.
/// </summary>
public abstract class Lzcnt : X86Base {

  /// <summary>
  /// Gets a value indicating whether LZCNT instructions are supported by the hardware.
  /// </summary>
  public new static bool IsSupported => false;

  /// <summary>
  /// Counts the number of leading zero bits in the source.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint LeadingZeroCount(uint value) {
    if (value == 0)
      return 32;

    uint count = 0;
    if ((value & 0xFFFF0000) == 0) { count += 16; value <<= 16; }
    if ((value & 0xFF000000) == 0) { count += 8; value <<= 8; }
    if ((value & 0xF0000000) == 0) { count += 4; value <<= 4; }
    if ((value & 0xC0000000) == 0) { count += 2; value <<= 2; }
    if ((value & 0x80000000) == 0) { ++count; }
    return count;
  }

  /// <summary>
  /// Provides 64-bit specific LZCNT operations.
  /// </summary>
  public new abstract class X64 : X86Base.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit LZCNT instructions are supported.
    /// </summary>
    public new static bool IsSupported => false;

    /// <summary>
    /// Counts the number of leading zero bits in the source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong LeadingZeroCount(ulong value) {
      if (value == 0)
        return 64;

      ulong count = 0;
      if ((value & 0xFFFFFFFF00000000) == 0) { count += 32; value <<= 32; }
      if ((value & 0xFFFF000000000000) == 0) { count += 16; value <<= 16; }
      if ((value & 0xFF00000000000000) == 0) { count += 8; value <<= 8; }
      if ((value & 0xF000000000000000) == 0) { count += 4; value <<= 4; }
      if ((value & 0xC000000000000000) == 0) { count += 2; value <<= 2; }
      if ((value & 0x8000000000000000) == 0) { ++count; }
      return count;
    }
  }
}

#endif
