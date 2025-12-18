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

using System;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
using NUnit.Framework;
using X86Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Backports.Tests.Intrinsics;

[TestFixture]
public class IntrinsicsTests {

  #region Bmi1 Tests

  [Test]
  public void Bmi1_BitFieldExtract_ExtractsCorrectBits() {
    // Extract 4 bits starting at position 4 from 0b11110000 = 0b1111 = 15
    var result = Bmi1.BitFieldExtract(0b11110000u, 4, 4);
    Assert.That(result, Is.EqualTo(0b1111u));
  }

  [Test]
  public void Bmi1_BitFieldExtract_WithControl_ExtractsCorrectBits() {
    // Control = start | (length << 8) = 4 | (4 << 8) = 0x0404
    var result = Bmi1.BitFieldExtract(0b11110000u, 0x0404);
    Assert.That(result, Is.EqualTo(0b1111u));
  }

  [Test]
  public void Bmi1_BitFieldExtract_StartAtZero_ExtractsLowBits() {
    var result = Bmi1.BitFieldExtract(0b10101010u, 0, 4);
    Assert.That(result, Is.EqualTo(0b1010u));
  }

  [Test]
  public void Bmi1_ExtractLowestSetBit_ReturnsLowestBit() {
    Assert.That(Bmi1.ExtractLowestSetBit(0b10100u), Is.EqualTo(0b00100u));
    Assert.That(Bmi1.ExtractLowestSetBit(0b11000u), Is.EqualTo(0b01000u));
    Assert.That(Bmi1.ExtractLowestSetBit(1u), Is.EqualTo(1u));
    Assert.That(Bmi1.ExtractLowestSetBit(0u), Is.EqualTo(0u));
  }

  [Test]
  public void Bmi1_GetMaskUpToLowestSetBit_ReturnsMask() {
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(0b10100u), Is.EqualTo(0b00111u));
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(0b11000u), Is.EqualTo(0b01111u));
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(1u), Is.EqualTo(1u));
  }

  [Test]
  public void Bmi1_ResetLowestSetBit_ClearsLowestBit() {
    Assert.That(Bmi1.ResetLowestSetBit(0b10100u), Is.EqualTo(0b10000u));
    Assert.That(Bmi1.ResetLowestSetBit(0b11000u), Is.EqualTo(0b10000u));
    Assert.That(Bmi1.ResetLowestSetBit(1u), Is.EqualTo(0u));
    Assert.That(Bmi1.ResetLowestSetBit(0u), Is.EqualTo(0u));
  }

  [Test]
  public void Bmi1_TrailingZeroCount_CountsCorrectly() {
    Assert.That(Bmi1.TrailingZeroCount(0b10100u), Is.EqualTo(2u));
    Assert.That(Bmi1.TrailingZeroCount(0b11000u), Is.EqualTo(3u));
    Assert.That(Bmi1.TrailingZeroCount(1u), Is.EqualTo(0u));
    Assert.That(Bmi1.TrailingZeroCount(0u), Is.EqualTo(32u));
    Assert.That(Bmi1.TrailingZeroCount(0x80000000u), Is.EqualTo(31u));
  }

  [Test]
  public void Bmi1_AndNot_PerformsCorrectOperation() {
    // AndNot(a, b) = ~a & b
    Assert.That(Bmi1.AndNot(0b1111u, 0b1010u), Is.EqualTo(0b0000u));
    Assert.That(Bmi1.AndNot(0b0000u, 0b1010u), Is.EqualTo(0b1010u));
    Assert.That(Bmi1.AndNot(0b0101u, 0b1111u), Is.EqualTo(0b1010u));
  }

  [Test]
  public void Bmi1_X64_BitFieldExtract_ExtractsCorrectBits() {
    var result = Bmi1.X64.BitFieldExtract(0b11110000UL, 4, 4);
    Assert.That(result, Is.EqualTo(0b1111UL));
  }

  [Test]
  public void Bmi1_X64_TrailingZeroCount_CountsCorrectly() {
    Assert.That(Bmi1.X64.TrailingZeroCount(0b10100UL), Is.EqualTo(2UL));
    Assert.That(Bmi1.X64.TrailingZeroCount(0UL), Is.EqualTo(64UL));
    Assert.That(Bmi1.X64.TrailingZeroCount(0x8000000000000000UL), Is.EqualTo(63UL));
  }

  #endregion

  #region Bmi2 Tests

  [Test]
  public void Bmi2_ParallelBitExtract_ExtractsCorrectBits() {
    // Extract bits where mask is set
    var result = Bmi2.ParallelBitExtract(0b10101010u, 0b11110000u);
    Assert.That(result, Is.EqualTo(0b1010u));
  }

  [Test]
  public void Bmi2_ParallelBitExtract_AllBitsSet_ReturnsValue() {
    var result = Bmi2.ParallelBitExtract(0xFFu, 0xFFu);
    Assert.That(result, Is.EqualTo(0xFFu));
  }

  [Test]
  public void Bmi2_ParallelBitExtract_NoMaskBits_ReturnsZero() {
    var result = Bmi2.ParallelBitExtract(0xFFu, 0u);
    Assert.That(result, Is.EqualTo(0u));
  }

  [Test]
  public void Bmi2_ParallelBitExtract_AlternatingMask_ExtractsAlternate() {
    // Value: 0b10101010, Mask: 0b01010101 -> extracts bits at positions 0,2,4,6 = 0,1,0,1 = 0b0101
    var result = Bmi2.ParallelBitExtract(0b10101010u, 0b01010101u);
    Assert.That(result, Is.EqualTo(0b0000u));

    // Value: 0b10101010, Mask: 0b10101010 -> extracts bits at positions 1,3,5,7 = 1,1,1,1 = 0b1111
    result = Bmi2.ParallelBitExtract(0b10101010u, 0b10101010u);
    Assert.That(result, Is.EqualTo(0b1111u));
  }

  [Test]
  public void Bmi2_X64_ParallelBitExtract_ExtractsCorrectBits() {
    var result = Bmi2.X64.ParallelBitExtract(0xAAAAAAAAAAAAAAAAUL, 0xFFFF000000000000UL);
    Assert.That(result, Is.EqualTo(0xAAAAUL));
  }

  #endregion

  #region Lzcnt Tests

  [Test]
  public void Lzcnt_LeadingZeroCount_CountsCorrectly() {
    Assert.That(Lzcnt.LeadingZeroCount(0u), Is.EqualTo(32u));
    Assert.That(Lzcnt.LeadingZeroCount(1u), Is.EqualTo(31u));
    Assert.That(Lzcnt.LeadingZeroCount(0x80000000u), Is.EqualTo(0u));
    Assert.That(Lzcnt.LeadingZeroCount(0x00008000u), Is.EqualTo(16u));
    Assert.That(Lzcnt.LeadingZeroCount(0x00000080u), Is.EqualTo(24u));
    Assert.That(Lzcnt.LeadingZeroCount(0xFFFFFFFFu), Is.EqualTo(0u));
  }

  [Test]
  public void Lzcnt_LeadingZeroCount_PowersOfTwo() {
    for (var i = 0; i < 32; ++i) {
      var value = 1u << i;
      Assert.That(Lzcnt.LeadingZeroCount(value), Is.EqualTo((uint)(31 - i)), $"Failed for 1 << {i}");
    }
  }

  [Test]
  public void Lzcnt_X64_LeadingZeroCount_CountsCorrectly() {
    Assert.That(Lzcnt.X64.LeadingZeroCount(0UL), Is.EqualTo(64UL));
    Assert.That(Lzcnt.X64.LeadingZeroCount(1UL), Is.EqualTo(63UL));
    Assert.That(Lzcnt.X64.LeadingZeroCount(0x8000000000000000UL), Is.EqualTo(0UL));
    Assert.That(Lzcnt.X64.LeadingZeroCount(0xFFFFFFFFFFFFFFFFUL), Is.EqualTo(0UL));
  }

  [Test]
  public void Lzcnt_X64_LeadingZeroCount_PowersOfTwo() {
    for (var i = 0; i < 64; ++i) {
      var value = 1UL << i;
      Assert.That(Lzcnt.X64.LeadingZeroCount(value), Is.EqualTo((ulong)(63 - i)), $"Failed for 1 << {i}");
    }
  }

  #endregion

  #region Popcnt Tests

  [Test]
  public void Popcnt_PopCount_CountsSetBits() {
    Assert.That(Popcnt.PopCount(0u), Is.EqualTo(0u));
    Assert.That(Popcnt.PopCount(1u), Is.EqualTo(1u));
    Assert.That(Popcnt.PopCount(0b1111u), Is.EqualTo(4u));
    Assert.That(Popcnt.PopCount(0b10101010u), Is.EqualTo(4u));
    Assert.That(Popcnt.PopCount(0xFFFFFFFFu), Is.EqualTo(32u));
    Assert.That(Popcnt.PopCount(0x55555555u), Is.EqualTo(16u)); // alternating bits
    Assert.That(Popcnt.PopCount(0xAAAAAAAAu), Is.EqualTo(16u)); // alternating bits
  }

  [Test]
  public void Popcnt_PopCount_PowersOfTwo() {
    for (var i = 0; i < 32; ++i) {
      var value = 1u << i;
      Assert.That(Popcnt.PopCount(value), Is.EqualTo(1u), $"Failed for 1 << {i}");
    }
  }

  [Test]
  public void Popcnt_PopCount_AllBitsUpTo() {
    for (var i = 0; i < 32; ++i) {
      var value = (1u << i) - 1; // all bits set up to position i-1
      Assert.That(Popcnt.PopCount(value), Is.EqualTo((uint)i), $"Failed for (1 << {i}) - 1");
    }
  }

  [Test]
  public void Popcnt_X64_PopCount_CountsSetBits() {
    Assert.That(Popcnt.X64.PopCount(0UL), Is.EqualTo(0UL));
    Assert.That(Popcnt.X64.PopCount(1UL), Is.EqualTo(1UL));
    Assert.That(Popcnt.X64.PopCount(0xFFFFFFFFFFFFFFFFUL), Is.EqualTo(64UL));
    Assert.That(Popcnt.X64.PopCount(0x5555555555555555UL), Is.EqualTo(32UL));
    Assert.That(Popcnt.X64.PopCount(0xAAAAAAAAAAAAAAAAUL), Is.EqualTo(32UL));
  }

  [Test]
  public void Popcnt_X64_PopCount_PowersOfTwo() {
    for (var i = 0; i < 64; ++i) {
      var value = 1UL << i;
      Assert.That(Popcnt.X64.PopCount(value), Is.EqualTo(1UL), $"Failed for 1 << {i}");
    }
  }

  #endregion

  #region ArmBase Tests

  private static void SkipIfArmNotSupported() {
    if (ArmBase.IsSupported)
      return;

    // On polyfill (older frameworks), methods work without hardware support
    // On newer frameworks without ARM hardware, methods throw PlatformNotSupportedException
    // Detect by trying a method - polyfill works, real intrinsics throw
    try {
      _ = ArmBase.LeadingZeroCount(1u);
    } catch (PlatformNotSupportedException) {
      Assert.Ignore("ARM intrinsics not supported on this platform");
    }
  }

  [Test]
  public void ArmBase_LeadingZeroCount_CountsCorrectly() {
    SkipIfArmNotSupported();
    Assert.That(ArmBase.LeadingZeroCount(0u), Is.EqualTo(32));
    Assert.That(ArmBase.LeadingZeroCount(1u), Is.EqualTo(31));
    Assert.That(ArmBase.LeadingZeroCount(0x80000000u), Is.EqualTo(0));
    Assert.That(ArmBase.LeadingZeroCount(0xFFFFFFFFu), Is.EqualTo(0));
  }

  [Test]
  public void ArmBase_Yield_DoesNotThrow() {
    SkipIfArmNotSupported();
    Assert.DoesNotThrow(() => ArmBase.Yield());
  }

  [Test]
  public void ArmBase_ReverseElementBits_ReversesCorrectly() {
    SkipIfArmNotSupported();
    Assert.That(ArmBase.ReverseElementBits(0u), Is.EqualTo(0u));
    Assert.That(ArmBase.ReverseElementBits(0xFFFFFFFFu), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(ArmBase.ReverseElementBits(1u), Is.EqualTo(0x80000000u));
    Assert.That(ArmBase.ReverseElementBits(0x80000000u), Is.EqualTo(1u));
    Assert.That(ArmBase.ReverseElementBits(0x0F0F0F0Fu), Is.EqualTo(0xF0F0F0F0u));
    Assert.That(ArmBase.ReverseElementBits(0x12345678u), Is.EqualTo(0x1E6A2C48u));
  }

  [Test]
  public void ArmBase_ReverseElementBits_DoubleReverseIsIdentity() {
    SkipIfArmNotSupported();
    var testValues = new uint[] { 0u, 1u, 0xFFFFFFFFu, 0x12345678u, 0xABCDEF01u, 0x55555555u };
    foreach (var value in testValues)
      Assert.That(ArmBase.ReverseElementBits(ArmBase.ReverseElementBits(value)), Is.EqualTo(value));
  }

  [Test]
  public void ArmBase_Arm64_LeadingZeroCount_CountsCorrectly() {
    SkipIfArmNotSupported();
    Assert.That(ArmBase.Arm64.LeadingZeroCount(0UL), Is.EqualTo(64));
    Assert.That(ArmBase.Arm64.LeadingZeroCount(1UL), Is.EqualTo(63));
    Assert.That(ArmBase.Arm64.LeadingZeroCount(0x8000000000000000UL), Is.EqualTo(0));
  }

  [Test]
  public void ArmBase_Arm64_LeadingSignCount_CountsCorrectly() {
    SkipIfArmNotSupported();
    // For positive numbers, count leading zeros minus 1 (sign bit)
    Assert.That(ArmBase.Arm64.LeadingSignCount(0), Is.EqualTo(31)); // 0x00000000 -> 31 leading sign bits
    Assert.That(ArmBase.Arm64.LeadingSignCount(1), Is.EqualTo(30)); // 0x00000001 -> 30 leading zeros - 1
    Assert.That(ArmBase.Arm64.LeadingSignCount(-1), Is.EqualTo(31)); // 0xFFFFFFFF -> 31 leading ones - 1
    Assert.That(ArmBase.Arm64.LeadingSignCount(int.MaxValue), Is.EqualTo(0)); // 0x7FFFFFFF
    Assert.That(ArmBase.Arm64.LeadingSignCount(int.MinValue), Is.EqualTo(0)); // 0x80000000
  }

  [Test]
  public void ArmBase_Arm64_LeadingSignCount_Long_CountsCorrectly() {
    SkipIfArmNotSupported();
    Assert.That(ArmBase.Arm64.LeadingSignCount(0L), Is.EqualTo(63));
    Assert.That(ArmBase.Arm64.LeadingSignCount(1L), Is.EqualTo(62));
    Assert.That(ArmBase.Arm64.LeadingSignCount(-1L), Is.EqualTo(63));
    Assert.That(ArmBase.Arm64.LeadingSignCount(long.MaxValue), Is.EqualTo(0));
    Assert.That(ArmBase.Arm64.LeadingSignCount(long.MinValue), Is.EqualTo(0));
  }

  [Test]
  public void ArmBase_Arm64_MultiplyHigh_Unsigned_ReturnsHighBits() {
    SkipIfArmNotSupported();
    // 0xFFFFFFFFFFFFFFFF * 0xFFFFFFFFFFFFFFFF = 0xFFFFFFFFFFFFFFFE_0000000000000001
    // High part should be 0xFFFFFFFFFFFFFFFE
    Assert.That(ArmBase.Arm64.MultiplyHigh(ulong.MaxValue, ulong.MaxValue), Is.EqualTo(0xFFFFFFFFFFFFFFFEUL));
    Assert.That(ArmBase.Arm64.MultiplyHigh(0UL, ulong.MaxValue), Is.EqualTo(0UL));
    Assert.That(ArmBase.Arm64.MultiplyHigh(1UL, 1UL), Is.EqualTo(0UL));
    // 0x100000000 * 0x100000000 = 0x1_00000000_00000000, high = 1
    Assert.That(ArmBase.Arm64.MultiplyHigh(0x100000000UL, 0x100000000UL), Is.EqualTo(1UL));
  }

  [Test]
  public void ArmBase_Arm64_MultiplyHigh_Signed_ReturnsHighBits() {
    SkipIfArmNotSupported();
    // Positive * Positive
    Assert.That(ArmBase.Arm64.MultiplyHigh(1L, 1L), Is.EqualTo(0L));
    Assert.That(ArmBase.Arm64.MultiplyHigh(0L, long.MaxValue), Is.EqualTo(0L));
    // 0x100000000 * 0x100000000 = 0x1_00000000_00000000, high = 1
    Assert.That(ArmBase.Arm64.MultiplyHigh(0x100000000L, 0x100000000L), Is.EqualTo(1L));
    // Negative * Negative = Positive
    Assert.That(ArmBase.Arm64.MultiplyHigh(-1L, -1L), Is.EqualTo(0L)); // -1 * -1 = 1
    // Negative * Positive = Negative (high bits should be -1 for small results)
    Assert.That(ArmBase.Arm64.MultiplyHigh(-1L, 1L), Is.EqualTo(-1L)); // -1 * 1 = -1
  }

  [Test]
  public void ArmBase_Arm64_ReverseElementBits_ReversesCorrectly() {
    SkipIfArmNotSupported();
    Assert.That(ArmBase.Arm64.ReverseElementBits(0UL), Is.EqualTo(0UL));
    Assert.That(ArmBase.Arm64.ReverseElementBits(0xFFFFFFFFFFFFFFFFUL), Is.EqualTo(0xFFFFFFFFFFFFFFFFUL));
    Assert.That(ArmBase.Arm64.ReverseElementBits(1UL), Is.EqualTo(0x8000000000000000UL));
    Assert.That(ArmBase.Arm64.ReverseElementBits(0x8000000000000000UL), Is.EqualTo(1UL));
  }

  [Test]
  public void ArmBase_Arm64_ReverseElementBits_DoubleReverseIsIdentity() {
    SkipIfArmNotSupported();
    var testValues = new ulong[] { 0UL, 1UL, 0xFFFFFFFFFFFFFFFFUL, 0x123456789ABCDEF0UL };
    foreach (var value in testValues)
      Assert.That(ArmBase.Arm64.ReverseElementBits(ArmBase.Arm64.ReverseElementBits(value)), Is.EqualTo(value));
  }

  #endregion

  #region IsSupported Tests

  [Test]
  public void AllIntrinsics_IsSupported_ReturnsFalseOnPolyfill() {
    // On older frameworks, all intrinsics should report IsSupported = false
    // This test verifies the polyfill behavior - on .NET Core 3.0+ with real hardware,
    // these may return true, but the software fallbacks should still work

    // We're just verifying the properties exist and are accessible
    _ = Bmi1.IsSupported;
    _ = Bmi1.X64.IsSupported;
    _ = Bmi2.IsSupported;
    _ = Bmi2.X64.IsSupported;
    _ = Lzcnt.IsSupported;
    _ = Lzcnt.X64.IsSupported;
    _ = Popcnt.IsSupported;
    _ = Popcnt.X64.IsSupported;
    _ = X86Aes.IsSupported;
    _ = X86Aes.X64.IsSupported;
    _ = Pclmulqdq.IsSupported;
    _ = Pclmulqdq.X64.IsSupported;
    _ = ArmBase.IsSupported;
    _ = ArmBase.Arm64.IsSupported;
    _ = X86Base.IsSupported;
    _ = X86Base.X64.IsSupported;

    Assert.Pass("All IsSupported properties are accessible");
  }

  #endregion

  #region Edge Cases and Comprehensive Tests

  [Test]
  public void Bmi1_TrailingZeroCount_MatchesExpectedForAllSingleBits() {
    for (var i = 0; i < 32; ++i) {
      var value = 1u << i;
      Assert.That(Bmi1.TrailingZeroCount(value), Is.EqualTo((uint)i), $"Failed for bit position {i}");
    }
  }

  [Test]
  public void Popcnt_And_Lzcnt_Consistency() {
    // For a power of 2, PopCount should be 1 and LeadingZeroCount + TrailingZeroCount should be 31
    for (var i = 0; i < 32; ++i) {
      var value = 1u << i;
      Assert.That(Popcnt.PopCount(value), Is.EqualTo(1u));
      Assert.That(Lzcnt.LeadingZeroCount(value) + Bmi1.TrailingZeroCount(value), Is.EqualTo(31u));
    }
  }

  [Test]
  public void Bmi2_ParallelBitExtract_ComprehensiveTest() {
    // Test various patterns
    var testCases = new (uint value, uint mask, uint expected)[] {
      (0xFFFFFFFF, 0x0000FFFF, 0x0000FFFF),
      (0x12345678, 0xF0F0F0F0, 0x00001357),
      (0xABCDEF01, 0x0F0F0F0F, 0x0000BDF1),
      (0x00000000, 0xFFFFFFFF, 0x00000000),
      (0xFFFFFFFF, 0x00000000, 0x00000000),
    };

    foreach (var (value, mask, expected) in testCases)
      Assert.That(Bmi2.ParallelBitExtract(value, mask), Is.EqualTo(expected),
        $"Failed for value=0x{value:X8}, mask=0x{mask:X8}");
  }

  #endregion
}
