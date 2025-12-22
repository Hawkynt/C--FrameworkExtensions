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
/// Provides BMI2 (Bit Manipulation Instruction Set 2) intrinsic operations.
/// This is a polyfill for older frameworks where BMI2 intrinsics are not available.
/// </summary>
public abstract class Bmi2 : X86Base {

  /// <summary>
  /// Gets a value indicating whether BMI2 instructions are supported by the hardware.
  /// </summary>
  /// <value>Always <see langword="false"/> in this polyfill implementation.</value>
  public new static bool IsSupported => false;

  /// <summary>
  /// Extracts bits from the source operand using a parallel bit extract operation (PEXT).
  /// </summary>
  /// <param name="value">The source value from which to extract bits.</param>
  /// <param name="mask">A mask specifying which bits to extract. Each set bit in the mask causes the corresponding bit from the source to be extracted.</param>
  /// <returns>The extracted bits packed into the least significant bits of the result.</returns>
  /// <remarks>
  /// This software implementation provides the same behavior as the BMI2 PEXT instruction.
  /// For each bit set in the mask, the corresponding bit from the value is extracted and
  /// placed contiguously in the result, starting from the least significant bit.
  /// </remarks>
  /// <example>
  /// <code>
  /// uint value = 0b_1010_1100;
  /// uint mask =  0b_1111_0000;
  /// uint result = Bmi2.ParallelBitExtract(value, mask); // result == 0b_1010
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ParallelBitExtract(uint value, uint mask) {
    uint result = 0, bit = 1;
    while (mask != 0) {
      if ((mask & 1) != 0) {
        if ((value & 1) != 0)
          result |= bit;
        bit <<= 1;
      }
      value >>= 1;
      mask >>= 1;
    }
    return result;
  }

  /// <summary>
  /// Provides 64-bit specific BMI2 operations.
  /// </summary>
  public new abstract class X64 : X86Base.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit BMI2 instructions are supported by the hardware.
    /// </summary>
    /// <value>Always <see langword="false"/> in this polyfill implementation.</value>
    public new static bool IsSupported => false;

    /// <summary>
    /// Extracts bits from the source operand using a parallel bit extract operation (PEXT) for 64-bit values.
    /// </summary>
    /// <param name="value">The 64-bit source value from which to extract bits.</param>
    /// <param name="mask">A 64-bit mask specifying which bits to extract.</param>
    /// <returns>The extracted bits packed into the least significant bits of the result.</returns>
    /// <remarks>
    /// This is the 64-bit version of the parallel bit extract operation.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ParallelBitExtract(ulong value, ulong mask) {
      ulong result = 0, bit = 1;
      while (mask != 0) {
        if ((mask & 1) != 0) {
          if ((value & 1) != 0)
            result |= bit;
          bit <<= 1;
        }
        value >>= 1;
        mask >>= 1;
      }
      return result;
    }
  }
}

#endif
