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

using System.Numerics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("BitOperations")]
public class BitOperationsTests {

  #region IsPow2 - int

  [Test]
  [Category("HappyPath")]
  public void IsPow2_Int_PowerOfTwo_ReturnsTrue() {
    Assert.That(BitOperations.IsPow2(1), Is.True);
    Assert.That(BitOperations.IsPow2(2), Is.True);
    Assert.That(BitOperations.IsPow2(4), Is.True);
    Assert.That(BitOperations.IsPow2(8), Is.True);
    Assert.That(BitOperations.IsPow2(16), Is.True);
    Assert.That(BitOperations.IsPow2(1024), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsPow2_Int_NotPowerOfTwo_ReturnsFalse() {
    Assert.That(BitOperations.IsPow2(0), Is.False);
    Assert.That(BitOperations.IsPow2(3), Is.False);
    Assert.That(BitOperations.IsPow2(5), Is.False);
    Assert.That(BitOperations.IsPow2(6), Is.False);
    Assert.That(BitOperations.IsPow2(7), Is.False);
    Assert.That(BitOperations.IsPow2(100), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsPow2_Int_NegativeValues_ReturnsFalse() {
    Assert.That(BitOperations.IsPow2(-1), Is.False);
    Assert.That(BitOperations.IsPow2(-2), Is.False);
    Assert.That(BitOperations.IsPow2(int.MinValue), Is.False);
  }

  #endregion

  #region IsPow2 - uint

  [Test]
  [Category("HappyPath")]
  public void IsPow2_UInt_PowerOfTwo_ReturnsTrue() {
    Assert.That(BitOperations.IsPow2(1u), Is.True);
    Assert.That(BitOperations.IsPow2(2u), Is.True);
    Assert.That(BitOperations.IsPow2(4u), Is.True);
    Assert.That(BitOperations.IsPow2(0x80000000u), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsPow2_UInt_NotPowerOfTwo_ReturnsFalse() {
    Assert.That(BitOperations.IsPow2(0u), Is.False);
    Assert.That(BitOperations.IsPow2(3u), Is.False);
    Assert.That(BitOperations.IsPow2(uint.MaxValue), Is.False);
  }

  #endregion

  #region IsPow2 - long

  [Test]
  [Category("HappyPath")]
  public void IsPow2_Long_PowerOfTwo_ReturnsTrue() {
    Assert.That(BitOperations.IsPow2(1L), Is.True);
    Assert.That(BitOperations.IsPow2(0x100000000L), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsPow2_Long_NotPowerOfTwo_ReturnsFalse() {
    Assert.That(BitOperations.IsPow2(0L), Is.False);
    Assert.That(BitOperations.IsPow2(-1L), Is.False);
  }

  #endregion

  #region IsPow2 - ulong

  [Test]
  [Category("HappyPath")]
  public void IsPow2_ULong_PowerOfTwo_ReturnsTrue() {
    Assert.That(BitOperations.IsPow2(1UL), Is.True);
    Assert.That(BitOperations.IsPow2(0x8000000000000000UL), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsPow2_ULong_NotPowerOfTwo_ReturnsFalse() {
    Assert.That(BitOperations.IsPow2(0UL), Is.False);
    Assert.That(BitOperations.IsPow2(ulong.MaxValue), Is.False);
  }

  #endregion

  #region LeadingZeroCount - uint

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_UInt_Zero_Returns32() {
    Assert.That(BitOperations.LeadingZeroCount(0u), Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_UInt_MaxValue_ReturnsZero() {
    Assert.That(BitOperations.LeadingZeroCount(uint.MaxValue), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_UInt_PowersOfTwo_ReturnsCorrect() {
    Assert.That(BitOperations.LeadingZeroCount(1u), Is.EqualTo(31));
    Assert.That(BitOperations.LeadingZeroCount(2u), Is.EqualTo(30));
    Assert.That(BitOperations.LeadingZeroCount(0x80000000u), Is.EqualTo(0));
  }

  #endregion

  #region LeadingZeroCount - ulong

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_ULong_Zero_Returns64() {
    Assert.That(BitOperations.LeadingZeroCount(0UL), Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_ULong_MaxValue_ReturnsZero() {
    Assert.That(BitOperations.LeadingZeroCount(ulong.MaxValue), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void LeadingZeroCount_ULong_PowersOfTwo_ReturnsCorrect() {
    Assert.That(BitOperations.LeadingZeroCount(1UL), Is.EqualTo(63));
    Assert.That(BitOperations.LeadingZeroCount(0x100000000UL), Is.EqualTo(31));
  }

  #endregion

  #region TrailingZeroCount - int

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_Int_Zero_Returns32() {
    Assert.That(BitOperations.TrailingZeroCount(0), Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_Int_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.TrailingZeroCount(1), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(2), Is.EqualTo(1));
    Assert.That(BitOperations.TrailingZeroCount(4), Is.EqualTo(2));
    Assert.That(BitOperations.TrailingZeroCount(8), Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_Int_OddValues_ReturnsZero() {
    Assert.That(BitOperations.TrailingZeroCount(1), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(3), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(5), Is.EqualTo(0));
  }

  #endregion

  #region TrailingZeroCount - uint

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_UInt_Zero_Returns32() {
    Assert.That(BitOperations.TrailingZeroCount(0u), Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_UInt_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.TrailingZeroCount(1u), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(0x80000000u), Is.EqualTo(31));
  }

  #endregion

  #region TrailingZeroCount - long

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_Long_Zero_Returns64() {
    Assert.That(BitOperations.TrailingZeroCount(0L), Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_Long_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.TrailingZeroCount(1L), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(0x100000000L), Is.EqualTo(32));
  }

  #endregion

  #region TrailingZeroCount - ulong

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_ULong_Zero_Returns64() {
    Assert.That(BitOperations.TrailingZeroCount(0UL), Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void TrailingZeroCount_ULong_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.TrailingZeroCount(1UL), Is.EqualTo(0));
    Assert.That(BitOperations.TrailingZeroCount(0x8000000000000000UL), Is.EqualTo(63));
  }

  #endregion

  #region Log2 - uint

  [Test]
  [Category("HappyPath")]
  public void Log2_UInt_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.Log2(1u), Is.EqualTo(0));
    Assert.That(BitOperations.Log2(2u), Is.EqualTo(1));
    Assert.That(BitOperations.Log2(4u), Is.EqualTo(2));
    Assert.That(BitOperations.Log2(8u), Is.EqualTo(3));
    Assert.That(BitOperations.Log2(1024u), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Log2_UInt_NonPowersOfTwo_ReturnsFloor() {
    Assert.That(BitOperations.Log2(3u), Is.EqualTo(1));
    Assert.That(BitOperations.Log2(5u), Is.EqualTo(2));
    Assert.That(BitOperations.Log2(7u), Is.EqualTo(2));
    Assert.That(BitOperations.Log2(100u), Is.EqualTo(6));
  }

  [Test]
  [Category("EdgeCase")]
  public void Log2_UInt_Zero_ReturnsZero() {
    Assert.That(BitOperations.Log2(0u), Is.EqualTo(0));
  }

  #endregion

  #region Log2 - ulong

  [Test]
  [Category("HappyPath")]
  public void Log2_ULong_PowersOfTwo_ReturnsExponent() {
    Assert.That(BitOperations.Log2(1UL), Is.EqualTo(0));
    Assert.That(BitOperations.Log2(0x100000000UL), Is.EqualTo(32));
    Assert.That(BitOperations.Log2(0x8000000000000000UL), Is.EqualTo(63));
  }

  [Test]
  [Category("EdgeCase")]
  public void Log2_ULong_Zero_ReturnsZero() {
    Assert.That(BitOperations.Log2(0UL), Is.EqualTo(0));
  }

  #endregion

  #region PopCount - uint

  [Test]
  [Category("HappyPath")]
  public void PopCount_UInt_Zero_ReturnsZero() {
    Assert.That(BitOperations.PopCount(0u), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PopCount_UInt_MaxValue_Returns32() {
    Assert.That(BitOperations.PopCount(uint.MaxValue), Is.EqualTo(32));
  }

  [Test]
  [Category("HappyPath")]
  public void PopCount_UInt_PowersOfTwo_ReturnsOne() {
    Assert.That(BitOperations.PopCount(1u), Is.EqualTo(1));
    Assert.That(BitOperations.PopCount(2u), Is.EqualTo(1));
    Assert.That(BitOperations.PopCount(4u), Is.EqualTo(1));
    Assert.That(BitOperations.PopCount(0x80000000u), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void PopCount_UInt_VariousValues_ReturnsCorrect() {
    Assert.That(BitOperations.PopCount(0b11111111u), Is.EqualTo(8));
    Assert.That(BitOperations.PopCount(0b10101010u), Is.EqualTo(4));
  }

  #endregion

  #region PopCount - ulong

  [Test]
  [Category("HappyPath")]
  public void PopCount_ULong_Zero_ReturnsZero() {
    Assert.That(BitOperations.PopCount(0UL), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PopCount_ULong_MaxValue_Returns64() {
    Assert.That(BitOperations.PopCount(ulong.MaxValue), Is.EqualTo(64));
  }

  [Test]
  [Category("HappyPath")]
  public void PopCount_ULong_PowersOfTwo_ReturnsOne() {
    Assert.That(BitOperations.PopCount(1UL), Is.EqualTo(1));
    Assert.That(BitOperations.PopCount(0x8000000000000000UL), Is.EqualTo(1));
  }

  #endregion

  #region RotateLeft - uint

  [Test]
  [Category("HappyPath")]
  public void RotateLeft_UInt_SingleBit_RotatesCorrectly() {
    Assert.That(BitOperations.RotateLeft(1u, 1), Is.EqualTo(2u));
    Assert.That(BitOperations.RotateLeft(1u, 4), Is.EqualTo(16u));
    Assert.That(BitOperations.RotateLeft(0x80000000u, 1), Is.EqualTo(1u));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateLeft_UInt_ZeroOffset_ReturnsSame() {
    Assert.That(BitOperations.RotateLeft(0x12345678u, 0), Is.EqualTo(0x12345678u));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateLeft_UInt_FullRotation_ReturnsSame() {
    Assert.That(BitOperations.RotateLeft(0x12345678u, 32), Is.EqualTo(0x12345678u));
  }

  #endregion

  #region RotateLeft - ulong

  [Test]
  [Category("HappyPath")]
  public void RotateLeft_ULong_SingleBit_RotatesCorrectly() {
    Assert.That(BitOperations.RotateLeft(1UL, 1), Is.EqualTo(2UL));
    Assert.That(BitOperations.RotateLeft(0x8000000000000000UL, 1), Is.EqualTo(1UL));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateLeft_ULong_FullRotation_ReturnsSame() {
    Assert.That(BitOperations.RotateLeft(0x123456789ABCDEF0UL, 64), Is.EqualTo(0x123456789ABCDEF0UL));
  }

  #endregion

  #region RotateRight - uint

  [Test]
  [Category("HappyPath")]
  public void RotateRight_UInt_SingleBit_RotatesCorrectly() {
    Assert.That(BitOperations.RotateRight(2u, 1), Is.EqualTo(1u));
    Assert.That(BitOperations.RotateRight(1u, 1), Is.EqualTo(0x80000000u));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateRight_UInt_ZeroOffset_ReturnsSame() {
    Assert.That(BitOperations.RotateRight(0x12345678u, 0), Is.EqualTo(0x12345678u));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateRight_UInt_FullRotation_ReturnsSame() {
    Assert.That(BitOperations.RotateRight(0x12345678u, 32), Is.EqualTo(0x12345678u));
  }

  #endregion

  #region RotateRight - ulong

  [Test]
  [Category("HappyPath")]
  public void RotateRight_ULong_SingleBit_RotatesCorrectly() {
    Assert.That(BitOperations.RotateRight(2UL, 1), Is.EqualTo(1UL));
    Assert.That(BitOperations.RotateRight(1UL, 1), Is.EqualTo(0x8000000000000000UL));
  }

  [Test]
  [Category("HappyPath")]
  public void RotateRight_ULong_FullRotation_ReturnsSame() {
    Assert.That(BitOperations.RotateRight(0x123456789ABCDEF0UL, 64), Is.EqualTo(0x123456789ABCDEF0UL));
  }

  #endregion

}
