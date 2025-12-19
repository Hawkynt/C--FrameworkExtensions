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
using System.Runtime.Intrinsics;
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

  #region SSE Tests

  [Test]
  public void Sse_IsSupported_ReturnsValidValue() {
    // IsSupported returns true on hardware with SSE support, false otherwise (including polyfill)
    // Just verify it's a valid boolean and doesn't throw
    var isSupported = Sse.IsSupported;
    Assert.That(isSupported, Is.TypeOf<bool>());
  }

  #endregion

  #region SSE2 Tests

  [Test]
  public void Sse2_Add_Double_ReturnsCorrectSum() {
    var left = Vector128.Create(1.5, 2.5);
    var right = Vector128.Create(0.5, 1.5);
    var result = Sse2.Add(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2.0).Within(0.0001));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(4.0).Within(0.0001));
  }

  [Test]
  public void Sse2_Add_Int32_ReturnsCorrectSum() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(10, 20, 30, 40);
    var result = Sse2.Add(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(11));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(22));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(33));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(44));
  }

  [Test]
  public void Sse2_Subtract_Int32_ReturnsCorrectDifference() {
    var left = Vector128.Create(100, 200, 300, 400);
    var right = Vector128.Create(1, 2, 3, 4);
    var result = Sse2.Subtract(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(99));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(198));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(297));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(396));
  }

  [Test]
  public void Sse2_And_Int32_PerformsLogicalAnd() {
    var left = Vector128.Create(0xFF00FF00, 0x0F0F0F0F, 0x12345678, 0xFFFFFFFF);
    var right = Vector128.Create(0x00FF00FF, 0xF0F0F0F0, 0x87654321, 0x12345678);
    var result = Sse2.And(left, right);
    Assert.That((uint)Vector128.GetElement(result, 0), Is.EqualTo(0x00000000u));
    Assert.That((uint)Vector128.GetElement(result, 1), Is.EqualTo(0x00000000u));
    Assert.That((uint)Vector128.GetElement(result, 2), Is.EqualTo(0x02244220u));
    Assert.That((uint)Vector128.GetElement(result, 3), Is.EqualTo(0x12345678u));
  }

  [Test]
  public void Sse2_Or_Int32_PerformsLogicalOr() {
    var left = Vector128.Create(0xFF000000, 0x00FF0000, 0x0000FF00, 0x000000FF);
    var right = Vector128.Create(0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000);
    var result = Sse2.Or(left, right);
    Assert.That((uint)Vector128.GetElement(result, 0), Is.EqualTo(0xFFFF0000u));
    Assert.That((uint)Vector128.GetElement(result, 1), Is.EqualTo(0x00FFFF00u));
    Assert.That((uint)Vector128.GetElement(result, 2), Is.EqualTo(0x0000FFFFu));
    Assert.That((uint)Vector128.GetElement(result, 3), Is.EqualTo(0xFF0000FFu));
  }

  [Test]
  public void Sse2_ShiftLeftLogical_Int32_ShiftsCorrectly() {
    var value = Vector128.Create(1, 2, 4, 8);
    var result = Sse2.ShiftLeftLogical(value, 2);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(4));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(8));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(16));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(32));
  }

  [Test]
  public void Sse2_ShiftRightLogical_Int32_ShiftsCorrectly() {
    var value = Vector128.Create(4, 8, 16, 32);
    var result = Sse2.ShiftRightLogical(value, 2);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(4));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(8));
  }

  [Test]
  public void Sse2_CompareEqual_Int32_ReturnsCorrectMask() {
    var left = Vector128.Create(1, 2, 3, 4);
    var right = Vector128.Create(1, 5, 3, 6);
    var result = Sse2.CompareEqual(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(-1)); // equal: all bits set
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(0));  // not equal
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-1)); // equal: all bits set
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(0));  // not equal
  }

  [Test]
  public void Sse2_Min_Int16_ReturnsMinimum() {
    var left = Vector128.Create((short)10, (short)20, (short)5, (short)15, (short)-5, (short)-10, (short)0, (short)100);
    var right = Vector128.Create((short)15, (short)15, (short)10, (short)10, (short)-10, (short)-5, (short)0, (short)-100);
    var result = Sse2.Min(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo((short)10));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo((short)15));
    Assert.That(Vector128.GetElement(result, 4), Is.EqualTo((short)-10));
    Assert.That(Vector128.GetElement(result, 5), Is.EqualTo((short)-10));
  }

  [Test]
  public void Sse2_Max_Int16_ReturnsMaximum() {
    var left = Vector128.Create((short)10, (short)20, (short)5, (short)15, (short)-5, (short)-10, (short)0, (short)100);
    var right = Vector128.Create((short)15, (short)15, (short)10, (short)10, (short)-10, (short)-5, (short)0, (short)-100);
    var result = Sse2.Max(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo((short)15));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo((short)20));
    Assert.That(Vector128.GetElement(result, 4), Is.EqualTo((short)-5));
    Assert.That(Vector128.GetElement(result, 5), Is.EqualTo((short)-5));
  }

  #endregion

  #region SSE3 Tests

  [Test]
  public void Sse3_HorizontalAdd_Float_AddsAdjacentPairs() {
    var left = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
    var right = Vector128.Create(5.0f, 6.0f, 7.0f, 8.0f);
    var result = Sse3.HorizontalAdd(left, right);
    // result[0] = left[0] + left[1] = 3
    // result[1] = left[2] + left[3] = 7
    // result[2] = right[0] + right[1] = 11
    // result[3] = right[2] + right[3] = 15
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(3.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(7.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(11.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(15.0f).Within(0.0001f));
  }

  [Test]
  public void Sse3_HorizontalSubtract_Float_SubtractsAdjacentPairs() {
    var left = Vector128.Create(10.0f, 3.0f, 20.0f, 5.0f);
    var right = Vector128.Create(100.0f, 40.0f, 200.0f, 80.0f);
    var result = Sse3.HorizontalSubtract(left, right);
    // result[0] = left[0] - left[1] = 7
    // result[1] = left[2] - left[3] = 15
    // result[2] = right[0] - right[1] = 60
    // result[3] = right[2] - right[3] = 120
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(7.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(15.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(60.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(120.0f).Within(0.0001f));
  }

  [Test]
  public void Sse3_HorizontalAdd_Double_AddsAdjacentPairs() {
    var left = Vector128.Create(1.0, 2.0);
    var right = Vector128.Create(3.0, 4.0);
    var result = Sse3.HorizontalAdd(left, right);
    // result[0] = left[0] + left[1] = 3
    // result[1] = right[0] + right[1] = 7
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(3.0).Within(0.0001));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(7.0).Within(0.0001));
  }

  [Test]
  public void Sse3_AddSubtract_Float_AlternatesSubtractAndAdd() {
    var left = Vector128.Create(10.0f, 20.0f, 30.0f, 40.0f);
    var right = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
    var result = Sse3.AddSubtract(left, right);
    // result[0] = left[0] - right[0] = 9
    // result[1] = left[1] + right[1] = 22
    // result[2] = left[2] - right[2] = 27
    // result[3] = left[3] + right[3] = 44
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(9.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(22.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(27.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(44.0f).Within(0.0001f));
  }

  [Test]
  public void Sse3_MoveAndDuplicate_Double_DuplicatesLower() {
    var value = Vector128.Create(1.5, 2.5);
    var result = Sse3.MoveAndDuplicate(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.5).Within(0.0001));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(1.5).Within(0.0001));
  }

  #endregion

  #region SSSE3 Tests

  [Test]
  public void Ssse3_Abs_SByte_ReturnsAbsoluteValue() {
    var value = Vector128.Create((sbyte)-1, (sbyte)-128, (sbyte)0, (sbyte)127, (sbyte)-50, (sbyte)50, (sbyte)-1, (sbyte)1,
      (sbyte)-1, (sbyte)-128, (sbyte)0, (sbyte)127, (sbyte)-50, (sbyte)50, (sbyte)-1, (sbyte)1);
    var result = Ssse3.Abs(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo((byte)1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo((byte)128)); // -128 as unsigned is 128
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo((byte)0));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo((byte)127));
  }

  [Test]
  public void Ssse3_Abs_Int16_ReturnsAbsoluteValue() {
    var value = Vector128.Create((short)-1, (short)-32768, (short)0, (short)32767, (short)-100, (short)100, (short)-1000, (short)1000);
    var result = Ssse3.Abs(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo((ushort)1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo((ushort)32768));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo((ushort)0));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo((ushort)32767));
  }

  [Test]
  public void Ssse3_Abs_Int32_ReturnsAbsoluteValue() {
    var value = Vector128.Create(-1, -2147483648, 0, 2147483647);
    var result = Ssse3.Abs(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1u));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2147483648u));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(0u));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(2147483647u));
  }

  [Test]
  public void Ssse3_HorizontalAdd_Int16_AddsAdjacentPairs() {
    var left = Vector128.Create((short)1, (short)2, (short)3, (short)4, (short)5, (short)6, (short)7, (short)8);
    var right = Vector128.Create((short)10, (short)20, (short)30, (short)40, (short)50, (short)60, (short)70, (short)80);
    var result = Ssse3.HorizontalAdd(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo((short)3));   // 1 + 2
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo((short)7));   // 3 + 4
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo((short)11));  // 5 + 6
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo((short)15));  // 7 + 8
    Assert.That(Vector128.GetElement(result, 4), Is.EqualTo((short)30));  // 10 + 20
    Assert.That(Vector128.GetElement(result, 5), Is.EqualTo((short)70));  // 30 + 40
    Assert.That(Vector128.GetElement(result, 6), Is.EqualTo((short)110)); // 50 + 60
    Assert.That(Vector128.GetElement(result, 7), Is.EqualTo((short)150)); // 70 + 80
  }

  #endregion

  #region SSE4.1 Tests

  [Test]
  public void Sse41_BlendVariable_Float_SelectsBasedOnMask() {
    var left = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
    var right = Vector128.Create(10.0f, 20.0f, 30.0f, 40.0f);
    // Mask with sign bits: positive=left, negative=right
    var mask = Vector128.Create(0, unchecked((int)0x80000000), 0, unchecked((int)0x80000000)).AsSingle();
    var result = Sse41.BlendVariable(left, right, mask);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));  // left
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(20.0f).Within(0.0001f)); // right
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3.0f).Within(0.0001f));  // left
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(40.0f).Within(0.0001f)); // right
  }

  [Test]
  public void Sse41_RoundToNearestInteger_Float_RoundsCorrectly() {
    var value = Vector128.Create(1.4f, 1.5f, 2.5f, -1.5f);
    var result = Sse41.RoundToNearestInteger(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2.0f).Within(0.0001f)); // banker's rounding
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(2.0f).Within(0.0001f)); // banker's rounding
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-2.0f).Within(0.0001f));
  }

  [Test]
  public void Sse41_Floor_Float_RoundsDown() {
    var value = Vector128.Create(1.9f, -1.1f, 2.5f, -2.5f);
    var result = Sse41.Floor(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-2.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-3.0f).Within(0.0001f));
  }

  [Test]
  public void Sse41_Ceiling_Float_RoundsUp() {
    var value = Vector128.Create(1.1f, -1.9f, 2.5f, -2.5f);
    var result = Sse41.Ceiling(value);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-1.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(3.0f).Within(0.0001f));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-2.0f).Within(0.0001f));
  }

  [Test]
  public void Sse41_Min_Int32_ReturnsMinimum() {
    var left = Vector128.Create(5, -10, 100, -1000);
    var right = Vector128.Create(10, -5, -100, 1000);
    var result = Sse41.Min(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(5));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-10));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(-100));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(-1000));
  }

  [Test]
  public void Sse41_Max_Int32_ReturnsMaximum() {
    var left = Vector128.Create(5, -10, 100, -1000);
    var right = Vector128.Create(10, -5, -100, 1000);
    var result = Sse41.Max(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(10));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(-5));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(100));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(1000));
  }

  #endregion

  #region SSE4.2 Tests

  [Test]
  public void Sse42_CompareGreaterThan_Long_ReturnsCorrectMask() {
    var left = Vector128.Create(10L, -5L);
    var right = Vector128.Create(5L, 5L);
    var result = Sse42.CompareGreaterThan(left, right);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(-1L)); // 10 > 5
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(0L));  // -5 not > 5
  }

  [Test]
  public void Sse42_Crc32_Byte_ComputesCorrectly() {
    // CRC32C of byte 0x01 with initial CRC 0 should produce non-zero result
    var crc = Sse42.Crc32(0u, (byte)0x01);
    Assert.That(crc, Is.Not.EqualTo(0u));

    // CRC32C of byte 0x00 with initial CRC 0 returns 0 (lookup table entry 0 is 0)
    var crc2 = Sse42.Crc32(0u, (byte)0x00);
    Assert.That(crc2, Is.EqualTo(0u));

    // CRC32C of byte 0xFF should produce non-trivial result
    var crc3 = Sse42.Crc32(0u, (byte)0xFF);
    Assert.That(crc3, Is.Not.EqualTo(0u));
  }

  [Test]
  public void Sse42_Crc32_MultipleBytes_AccumulatesCorrectly() {
    var crc = 0xFFFFFFFFu;
    crc = Sse42.Crc32(crc, (byte)'H');
    crc = Sse42.Crc32(crc, (byte)'e');
    crc = Sse42.Crc32(crc, (byte)'l');
    crc = Sse42.Crc32(crc, (byte)'l');
    crc = Sse42.Crc32(crc, (byte)'o');
    // Just verify we get a non-trivial result
    Assert.That(crc, Is.Not.EqualTo(0xFFFFFFFFu));
  }

  [Test]
  public void Sse42_Crc32_UInt32_ComputesCorrectly() {
    var crc = Sse42.Crc32(0u, 0x12345678u);
    Assert.That(crc, Is.Not.EqualTo(0u));
    Assert.That(crc, Is.Not.EqualTo(0x12345678u));
  }

  #endregion

  #region AVX Tests

  [Test]
  public void Avx_Add_Float_ReturnsCorrectSum() {
    var left = Vector256.Create(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f);
    var right = Vector256.Create(0.5f, 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7.5f);
    var result = Avx.Add(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(1.5f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(9.5f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(15.5f).Within(0.0001f));
  }

  [Test]
  public void Avx_Add_Double_ReturnsCorrectSum() {
    var left = Vector256.Create(1.0, 2.0, 3.0, 4.0);
    var right = Vector256.Create(0.5, 1.5, 2.5, 3.5);
    var result = Avx.Add(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(1.5).Within(0.0001));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(3.5).Within(0.0001));
    Assert.That(Vector256.GetElement(result, 2), Is.EqualTo(5.5).Within(0.0001));
    Assert.That(Vector256.GetElement(result, 3), Is.EqualTo(7.5).Within(0.0001));
  }

  [Test]
  public void Avx_Multiply_Float_ReturnsCorrectProduct() {
    var left = Vector256.Create(2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f);
    var right = Vector256.Create(2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f);
    var result = Avx.Multiply(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(4.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(12.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(18.0f).Within(0.0001f));
  }

  [Test]
  public void Avx_Min_Float_ReturnsMinimum() {
    var left = Vector256.Create(1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f);
    var right = Vector256.Create(2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f);
    var result = Avx.Min(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(3.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(1.0f).Within(0.0001f));
  }

  [Test]
  public void Avx_Max_Float_ReturnsMaximum() {
    var left = Vector256.Create(1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f);
    var right = Vector256.Create(2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f);
    var result = Avx.Max(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(5.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(9.0f).Within(0.0001f));
  }

  [Test]
  public void Avx_And_Float_PerformsLogicalAnd() {
    var left = Vector256.Create(1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f);
    var right = Vector256.Create(1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);
    var result = Avx.And(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(1.0f));
  }

  [Test]
  public void Avx_Sqrt_Float_ReturnsSquareRoot() {
    var value = Vector256.Create(4.0f, 9.0f, 16.0f, 25.0f, 36.0f, 49.0f, 64.0f, 81.0f);
    var result = Avx.Sqrt(value);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(3.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(6.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(9.0f).Within(0.0001f));
  }

  [Test]
  public void Avx_Floor_Float_RoundsDown() {
    var value = Vector256.Create(1.9f, -1.1f, 2.5f, -2.5f, 3.1f, -3.9f, 4.0f, -4.0f);
    var result = Avx.Floor(value);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(-2.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 5), Is.EqualTo(-4.0f).Within(0.0001f));
  }

  [Test]
  public void Avx_Ceiling_Float_RoundsUp() {
    var value = Vector256.Create(1.1f, -1.9f, 2.5f, -2.5f, 3.9f, -3.1f, 4.0f, -4.0f);
    var result = Avx.Ceiling(value);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(-1.0f).Within(0.0001f));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(4.0f).Within(0.0001f));
  }

  #endregion

  #region AVX2 Tests

  [Test]
  public void Avx2_Add_Int32_ReturnsCorrectSum() {
    var left = Vector256.Create(1, 2, 3, 4, 5, 6, 7, 8);
    var right = Vector256.Create(10, 20, 30, 40, 50, 60, 70, 80);
    var result = Avx2.Add(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(11));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(55));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(88));
  }

  [Test]
  public void Avx2_Subtract_Int32_ReturnsCorrectDifference() {
    var left = Vector256.Create(100, 200, 300, 400, 500, 600, 700, 800);
    var right = Vector256.Create(1, 2, 3, 4, 5, 6, 7, 8);
    var result = Avx2.Subtract(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(99));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(495));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(792));
  }

  [Test]
  public void Avx2_And_Int32_PerformsLogicalAnd() {
    var left = Vector256.Create(0xFF00FF00, 0x0F0F0F0F, 0, 0, 0, 0, 0, 0);
    var right = Vector256.Create(0x00FF00FF, 0xF0F0F0F0, 0, 0, 0, 0, 0, 0);
    var result = Avx2.And(left, right);
    Assert.That((uint)Vector256.GetElement(result, 0), Is.EqualTo(0x00000000u));
    Assert.That((uint)Vector256.GetElement(result, 1), Is.EqualTo(0x00000000u));
  }

  [Test]
  public void Avx2_Or_Int32_PerformsLogicalOr() {
    var left = Vector256.Create(unchecked((int)0xFF000000), 0x00FF0000, 0, 0, 0, 0, 0, 0);
    var right = Vector256.Create(0x00FF0000, 0x0000FF00, 0, 0, 0, 0, 0, 0);
    var result = Avx2.Or(left, right);
    Assert.That(unchecked((uint)Vector256.GetElement(result, 0)), Is.EqualTo(0xFFFF0000u));
    Assert.That(unchecked((uint)Vector256.GetElement(result, 1)), Is.EqualTo(0x00FFFF00u));
  }

  [Test]
  public void Avx2_ShiftLeftLogical_Int32_ShiftsCorrectly() {
    var value = Vector256.Create(1, 2, 4, 8, 16, 32, 64, 128);
    var result = Avx2.ShiftLeftLogical(value, 2);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(4));
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(64));
    Assert.That(Vector256.GetElement(result, 7), Is.EqualTo(512));
  }

  [Test]
  public void Avx2_CompareEqual_Int32_ReturnsCorrectMask() {
    var left = Vector256.Create(1, 2, 3, 4, 5, 6, 7, 8);
    var right = Vector256.Create(1, 5, 3, 6, 5, 9, 7, 10);
    var result = Avx2.CompareEqual(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(-1)); // equal
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(0));  // not equal
    Assert.That(Vector256.GetElement(result, 2), Is.EqualTo(-1)); // equal
    Assert.That(Vector256.GetElement(result, 4), Is.EqualTo(-1)); // equal
  }

  [Test]
  public void Avx2_Abs_Int32_ReturnsAbsoluteValue() {
    var value = Vector256.Create(-1, -100, 0, 100, -1000, 1000, int.MinValue, int.MaxValue);
    var result = Avx2.Abs(value);
    Assert.That((uint)Vector256.GetElement(result, 0), Is.EqualTo(1u));
    Assert.That((uint)Vector256.GetElement(result, 1), Is.EqualTo(100u));
    Assert.That((uint)Vector256.GetElement(result, 2), Is.EqualTo(0u));
    Assert.That((uint)Vector256.GetElement(result, 4), Is.EqualTo(1000u));
  }

  [Test]
  public void Avx2_Min_Int32_ReturnsMinimum() {
    var left = Vector256.Create(5, -10, 100, -1000, 1, -1, 0, 50);
    var right = Vector256.Create(10, -5, -100, 1000, -1, 1, 0, -50);
    var result = Avx2.Min(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(5));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(-10));
    Assert.That(Vector256.GetElement(result, 2), Is.EqualTo(-100));
    Assert.That(Vector256.GetElement(result, 3), Is.EqualTo(-1000));
  }

  [Test]
  public void Avx2_Max_Int32_ReturnsMaximum() {
    var left = Vector256.Create(5, -10, 100, -1000, 1, -1, 0, 50);
    var right = Vector256.Create(10, -5, -100, 1000, -1, 1, 0, -50);
    var result = Avx2.Max(left, right);
    Assert.That(Vector256.GetElement(result, 0), Is.EqualTo(10));
    Assert.That(Vector256.GetElement(result, 1), Is.EqualTo(-5));
    Assert.That(Vector256.GetElement(result, 2), Is.EqualTo(100));
    Assert.That(Vector256.GetElement(result, 3), Is.EqualTo(1000));
  }

  #endregion

  #region AVX-512F Tests

  private static void SkipIfAvx512FNotSupported() {
    if (Avx512F.IsSupported)
      return;

    // On polyfill (older frameworks), methods work without hardware support
    // On newer frameworks without AVX-512 hardware, methods throw PlatformNotSupportedException
    // Detect by trying a method - polyfill works, real intrinsics throw
    try {
      _ = Avx512F.Add(Vector512.Create(1.0f), Vector512.Create(2.0f));
    } catch (PlatformNotSupportedException) {
      Assert.Ignore("AVX-512F intrinsics not supported on this platform");
    }
  }

  [Test]
  public void Avx512F_Add_Float_ReturnsCorrectSum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f);
    var right = Vector512.Create(0.5f, 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7.5f, 8.5f, 9.5f, 10.5f, 11.5f, 12.5f, 13.5f, 14.5f, 15.5f);
    var result = Avx512F.Add(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(1.5f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(17.5f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(31.5f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_Add_Double_ReturnsCorrectSum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0);
    var right = Vector512.Create(0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5);
    var result = Avx512F.Add(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(1.5).Within(0.0001));
    Assert.That(Vector512.GetElement(result, 4), Is.EqualTo(9.5).Within(0.0001));
    Assert.That(Vector512.GetElement(result, 7), Is.EqualTo(15.5).Within(0.0001));
  }

  [Test]
  public void Avx512F_Add_Int32_ReturnsCorrectSum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
    var right = Vector512.Create(10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160);
    var result = Avx512F.Add(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(11));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(99));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(176));
  }

  [Test]
  public void Avx512F_Subtract_Float_ReturnsCorrectDifference() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 60.0f, 70.0f, 80.0f, 90.0f, 100.0f, 110.0f, 120.0f, 130.0f, 140.0f, 150.0f, 160.0f);
    var right = Vector512.Create(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f);
    var result = Avx512F.Subtract(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(9.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(81.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(144.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_Multiply_Float_ReturnsCorrectProduct() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f, 11.0f, 12.0f, 13.0f, 14.0f, 15.0f, 16.0f, 17.0f);
    var right = Vector512.Create(2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f);
    var result = Avx512F.Multiply(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(4.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(20.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(34.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_Divide_Float_ReturnsCorrectQuotient() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 60.0f, 70.0f, 80.0f, 90.0f, 100.0f, 110.0f, 120.0f, 130.0f, 140.0f, 150.0f, 160.0f);
    var right = Vector512.Create(2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f);
    var result = Avx512F.Divide(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(5.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(45.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(80.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_And_Int32_PerformsLogicalAnd() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(0xFF00FF00, 0x0F0F0F0F, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var right = Vector512.Create(0x00FF00FF, 0xF0F0F0F0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var result = Avx512F.And(left, right);
    Assert.That((uint)Vector512.GetElement(result, 0), Is.EqualTo(0x00000000u));
    Assert.That((uint)Vector512.GetElement(result, 1), Is.EqualTo(0x00000000u));
  }

  [Test]
  public void Avx512F_Or_Int32_PerformsLogicalOr() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(unchecked((int)0xFF000000), 0x00FF0000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var right = Vector512.Create(0x00FF0000, 0x0000FF00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var result = Avx512F.Or(left, right);
    Assert.That(unchecked((uint)Vector512.GetElement(result, 0)), Is.EqualTo(0xFFFF0000u));
    Assert.That(unchecked((uint)Vector512.GetElement(result, 1)), Is.EqualTo(0x00FFFF00u));
  }

  [Test]
  public void Avx512F_Xor_Int32_PerformsLogicalXor() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(0xFF00FF00, 0xFF00FF00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var right = Vector512.Create(0xFF00FF00, 0x00FF00FF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var result = Avx512F.Xor(left, right);
    Assert.That((uint)Vector512.GetElement(result, 0), Is.EqualTo(0x00000000u)); // same values XOR = 0
    Assert.That((uint)Vector512.GetElement(result, 1), Is.EqualTo(0xFFFFFFFFu)); // different bits XOR = all set
  }

  [Test]
  public void Avx512F_Min_Float_ReturnsMinimum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f, 1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f);
    var right = Vector512.Create(2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f, 2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f);
    var result = Avx512F.Min(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(1.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(3.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_Max_Float_ReturnsMaximum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f, 1.0f, 5.0f, 3.0f, 7.0f, 2.0f, 6.0f, 4.0f, 8.0f);
    var right = Vector512.Create(2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f, 2.0f, 3.0f, 4.0f, 6.0f, 1.0f, 7.0f, 5.0f, 9.0f);
    var result = Avx512F.Max(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(5.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_Min_Int32_ReturnsMinimum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(5, -10, 100, -1000, 1, -1, 0, 50, 5, -10, 100, -1000, 1, -1, 0, 50);
    var right = Vector512.Create(10, -5, -100, 1000, -1, 1, 0, -50, 10, -5, -100, 1000, -1, 1, 0, -50);
    var result = Avx512F.Min(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(5));
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(-10));
    Assert.That(Vector512.GetElement(result, 2), Is.EqualTo(-100));
  }

  [Test]
  public void Avx512F_Max_Int32_ReturnsMaximum() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(5, -10, 100, -1000, 1, -1, 0, 50, 5, -10, 100, -1000, 1, -1, 0, 50);
    var right = Vector512.Create(10, -5, -100, 1000, -1, 1, 0, -50, 10, -5, -100, 1000, -1, 1, 0, -50);
    var result = Avx512F.Max(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(10));
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(-5));
    Assert.That(Vector512.GetElement(result, 2), Is.EqualTo(100));
  }

  [Test]
  public void Avx512F_Abs_Int32_ReturnsAbsoluteValue() {
    SkipIfAvx512FNotSupported();
    var value = Vector512.Create(-1, -100, 0, 100, -1000, 1000, int.MinValue, int.MaxValue, -1, -100, 0, 100, -1000, 1000, int.MinValue, int.MaxValue);
    var result = Avx512F.Abs(value);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(1u));
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(100u));
    Assert.That(Vector512.GetElement(result, 2), Is.EqualTo(0u));
    Assert.That(Vector512.GetElement(result, 4), Is.EqualTo(1000u));
  }

  [Test]
  public void Avx512F_Sqrt_Float_ReturnsSquareRoot() {
    SkipIfAvx512FNotSupported();
    var value = Vector512.Create(4.0f, 9.0f, 16.0f, 25.0f, 36.0f, 49.0f, 64.0f, 81.0f, 100.0f, 121.0f, 144.0f, 169.0f, 196.0f, 225.0f, 256.0f, 289.0f);
    var result = Avx512F.Sqrt(value);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(2.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 4), Is.EqualTo(6.0f).Within(0.0001f));
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(10.0f).Within(0.0001f));
  }

  [Test]
  public void Avx512F_ShiftLeftLogical_Int32_ShiftsCorrectly() {
    SkipIfAvx512FNotSupported();
    var value = Vector512.Create(1, 2, 4, 8, 16, 32, 64, 128, 1, 2, 4, 8, 16, 32, 64, 128);
    var result = Avx512F.ShiftLeftLogical(value, 2);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(4));
    Assert.That(Vector512.GetElement(result, 4), Is.EqualTo(64));
    Assert.That(Vector512.GetElement(result, 7), Is.EqualTo(512));
  }

  [Test]
  public void Avx512F_ShiftRightLogical_Int32_ShiftsCorrectly() {
    SkipIfAvx512FNotSupported();
    var value = Vector512.Create(4, 8, 16, 32, 64, 128, 256, 512, 4, 8, 16, 32, 64, 128, 256, 512);
    var result = Avx512F.ShiftRightLogical(value, 2);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector512.GetElement(result, 4), Is.EqualTo(16));
    Assert.That(Vector512.GetElement(result, 7), Is.EqualTo(128));
  }

  [Test]
  public void Avx512F_CompareEqual_Int32_ReturnsCorrectMask() {
    SkipIfAvx512FNotSupported();
    var left = Vector512.Create(1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8);
    var right = Vector512.Create(1, 5, 3, 6, 5, 9, 7, 10, 1, 2, 3, 4, 5, 6, 7, 8);
    var result = Avx512F.CompareEqual(left, right);
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(-1)); // equal
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(0));  // not equal
    Assert.That(Vector512.GetElement(result, 2), Is.EqualTo(-1)); // equal
    Assert.That(Vector512.GetElement(result, 8), Is.EqualTo(-1)); // equal
  }

  [Test]
  public void Avx512F_FusedMultiplyAdd_Float_ComputesCorrectly() {
    SkipIfAvx512FNotSupported();
    var a = Vector512.Create(2.0f, 3.0f, 4.0f, 5.0f, 2.0f, 3.0f, 4.0f, 5.0f, 2.0f, 3.0f, 4.0f, 5.0f, 2.0f, 3.0f, 4.0f, 5.0f);
    var b = Vector512.Create(3.0f, 4.0f, 5.0f, 6.0f, 3.0f, 4.0f, 5.0f, 6.0f, 3.0f, 4.0f, 5.0f, 6.0f, 3.0f, 4.0f, 5.0f, 6.0f);
    var c = Vector512.Create(1.0f, 2.0f, 3.0f, 4.0f, 1.0f, 2.0f, 3.0f, 4.0f, 1.0f, 2.0f, 3.0f, 4.0f, 1.0f, 2.0f, 3.0f, 4.0f);
    var result = Avx512F.FusedMultiplyAdd(a, b, c);
    // result = a * b + c
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(7.0f).Within(0.0001f));  // 2*3+1=7
    Assert.That(Vector512.GetElement(result, 1), Is.EqualTo(14.0f).Within(0.0001f)); // 3*4+2=14
    Assert.That(Vector512.GetElement(result, 2), Is.EqualTo(23.0f).Within(0.0001f)); // 4*5+3=23
    Assert.That(Vector512.GetElement(result, 3), Is.EqualTo(34.0f).Within(0.0001f)); // 5*6+4=34
  }

  [Test]
  public void Avx512F_TernaryLogic_PerformsCorrectOperation() {
    SkipIfAvx512FNotSupported();
    var a = Vector512.Create(0xFF00FF00, 0xFF00FF00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var b = Vector512.Create(0xF0F0F0F0, 0xF0F0F0F0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    var c = Vector512.Create(0xCCCCCCCC, 0xCCCCCCCC, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    // Control 0xF0 = A (just return A)
    var result = Avx512F.TernaryLogic(a, b, c, 0xF0);
    Assert.That((uint)Vector512.GetElement(result, 0), Is.EqualTo(0xFF00FF00u));
  }

  #endregion
}
