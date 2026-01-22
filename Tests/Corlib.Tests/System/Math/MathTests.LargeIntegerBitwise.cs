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

using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public class LargeIntegerBitwiseTests {

  #region UInt96 Bitwise Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 And computes correctly")]
  public void And_UInt96_ComputesCorrectly() {
    UInt96 a = 0xFF;
    UInt96 b = 0x0F;
    Assert.That(a.And(b), Is.EqualTo((UInt96)0x0F));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Or computes correctly")]
  public void Or_UInt96_ComputesCorrectly() {
    UInt96 a = 0xF0;
    UInt96 b = 0x0F;
    Assert.That(a.Or(b), Is.EqualTo((UInt96)0xFF));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Xor computes correctly")]
  public void Xor_UInt96_ComputesCorrectly() {
    UInt96 a = 0xFF;
    UInt96 b = 0x0F;
    Assert.That(a.Xor(b), Is.EqualTo((UInt96)0xF0));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Not computes correctly")]
  public void Not_UInt96_ComputesCorrectly() {
    UInt96 a = UInt96.Zero;
    Assert.That(a.Not(), Is.EqualTo(UInt96.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Nand computes correctly")]
  public void Nand_UInt96_ComputesCorrectly() {
    UInt96 a = UInt96.MaxValue;
    UInt96 b = UInt96.MaxValue;
    Assert.That(a.Nand(b), Is.EqualTo(UInt96.Zero));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Nor computes correctly")]
  public void Nor_UInt96_ComputesCorrectly() {
    UInt96 a = UInt96.Zero;
    UInt96 b = UInt96.Zero;
    Assert.That(a.Nor(b), Is.EqualTo(UInt96.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 Equ (XNOR) computes correctly")]
  public void Equ_UInt96_ComputesCorrectly() {
    UInt96 a = 0x12345678;
    UInt96 b = 0x12345678;
    Assert.That(a.Equ(b), Is.EqualTo(UInt96.MaxValue));
  }

  #endregion

  #region Int96 Bitwise Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 And computes correctly")]
  public void And_Int96_ComputesCorrectly() {
    Int96 a = 0xFF;
    Int96 b = 0x0F;
    Assert.That(a.And(b), Is.EqualTo((Int96)0x0F));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Or computes correctly")]
  public void Or_Int96_ComputesCorrectly() {
    Int96 a = 0xF0;
    Int96 b = 0x0F;
    Assert.That(a.Or(b), Is.EqualTo((Int96)0xFF));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Xor computes correctly")]
  public void Xor_Int96_ComputesCorrectly() {
    Int96 a = 0xFF;
    Int96 b = 0x0F;
    Assert.That(a.Xor(b), Is.EqualTo((Int96)0xF0));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Not computes correctly")]
  public void Not_Int96_ComputesCorrectly() {
    Int96 a = Int96.Zero;
    Assert.That(a.Not(), Is.EqualTo((Int96)(-1)));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Nand computes correctly")]
  public void Nand_Int96_ComputesCorrectly() {
    Int96 a = -1;
    Int96 b = -1;
    Assert.That(a.Nand(b), Is.EqualTo(Int96.Zero));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Nor computes correctly")]
  public void Nor_Int96_ComputesCorrectly() {
    Int96 a = Int96.Zero;
    Int96 b = Int96.Zero;
    Assert.That(a.Nor(b), Is.EqualTo((Int96)(-1)));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int96 Equ (XNOR) computes correctly")]
  public void Equ_Int96_ComputesCorrectly() {
    Int96 a = 0x12345678;
    Int96 b = 0x12345678;
    Assert.That(a.Equ(b), Is.EqualTo((Int96)(-1)));
  }

  #endregion

  #region UInt128 Bitwise Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 And computes correctly")]
  public void And_UInt128_ComputesCorrectly() {
    UInt128 a = 0xFF;
    UInt128 b = 0x0F;
    Assert.That(a.And(b), Is.EqualTo((UInt128)0x0F));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Or computes correctly")]
  public void Or_UInt128_ComputesCorrectly() {
    UInt128 a = 0xF0;
    UInt128 b = 0x0F;
    Assert.That(a.Or(b), Is.EqualTo((UInt128)0xFF));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Xor computes correctly")]
  public void Xor_UInt128_ComputesCorrectly() {
    UInt128 a = 0xFF;
    UInt128 b = 0x0F;
    Assert.That(a.Xor(b), Is.EqualTo((UInt128)0xF0));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Not computes correctly")]
  public void Not_UInt128_ComputesCorrectly() {
    UInt128 a = UInt128.Zero;
    Assert.That(a.Not(), Is.EqualTo(UInt128.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Nand computes correctly")]
  public void Nand_UInt128_ComputesCorrectly() {
    UInt128 a = UInt128.MaxValue;
    UInt128 b = UInt128.MaxValue;
    Assert.That(a.Nand(b), Is.EqualTo(UInt128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Nor computes correctly")]
  public void Nor_UInt128_ComputesCorrectly() {
    UInt128 a = UInt128.Zero;
    UInt128 b = UInt128.Zero;
    Assert.That(a.Nor(b), Is.EqualTo(UInt128.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 Equ (XNOR) computes correctly")]
  public void Equ_UInt128_ComputesCorrectly() {
    UInt128 a = 0x12345678;
    UInt128 b = 0x12345678;
    Assert.That(a.Equ(b), Is.EqualTo(UInt128.MaxValue));
  }

  #endregion

  #region Int128 Bitwise Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 And computes correctly")]
  public void And_Int128_ComputesCorrectly() {
    Int128 a = 0xFF;
    Int128 b = 0x0F;
    Assert.That(a.And(b), Is.EqualTo((Int128)0x0F));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Or computes correctly")]
  public void Or_Int128_ComputesCorrectly() {
    Int128 a = 0xF0;
    Int128 b = 0x0F;
    Assert.That(a.Or(b), Is.EqualTo((Int128)0xFF));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Xor computes correctly")]
  public void Xor_Int128_ComputesCorrectly() {
    Int128 a = 0xFF;
    Int128 b = 0x0F;
    Assert.That(a.Xor(b), Is.EqualTo((Int128)0xF0));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Not computes correctly")]
  public void Not_Int128_ComputesCorrectly() {
    Int128 a = Int128.Zero;
    Assert.That(a.Not(), Is.EqualTo((Int128)(-1)));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Nand computes correctly")]
  public void Nand_Int128_ComputesCorrectly() {
    Int128 a = -1;
    Int128 b = -1;
    Assert.That(a.Nand(b), Is.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Nor computes correctly")]
  public void Nor_Int128_ComputesCorrectly() {
    Int128 a = Int128.Zero;
    Int128 b = Int128.Zero;
    Assert.That(a.Nor(b), Is.EqualTo((Int128)(-1)));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates Int128 Equ (XNOR) computes correctly")]
  public void Equ_Int128_ComputesCorrectly() {
    Int128 a = 0x12345678;
    Int128 b = 0x12345678;
    Assert.That(a.Equ(b), Is.EqualTo((Int128)(-1)));
  }

  #endregion

  #region Rotate Extension Tests

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 RotateLeft extension works")]
  public void RotateLeft_UInt96_Extension_Works() {
    UInt96 a = 1;
    Assert.That(a.RotateLeft(4), Is.EqualTo((UInt96)16));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt96 RotateRight extension works")]
  public void RotateRight_UInt96_Extension_Works() {
    UInt96 a = 16;
    Assert.That(a.RotateRight(4), Is.EqualTo((UInt96)1));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 RotateLeft extension works")]
  public void RotateLeft_UInt128_Extension_Works() {
    UInt128 a = 1;
    Assert.That(a.RotateLeft(4), Is.EqualTo((UInt128)16));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates UInt128 RotateRight extension works")]
  public void RotateRight_UInt128_Extension_Works() {
    UInt128 a = 16;
    Assert.That(a.RotateRight(4), Is.EqualTo((UInt128)1));
  }

  #endregion

  #region EdgeCase Tests

  [Test]
  [Category("EdgeCase")]
  [Description("Validates UInt96 And with zero returns zero")]
  public void And_UInt96_WithZero_ReturnsZero() {
    Assert.That(UInt96.MaxValue.And(UInt96.Zero), Is.EqualTo(UInt96.Zero));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates UInt128 Or with zero returns original")]
  public void Or_UInt128_WithZero_ReturnsOriginal() {
    UInt128 a = 12345;
    Assert.That(a.Or(UInt128.Zero), Is.EqualTo(a));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Int128 Xor with itself returns zero")]
  public void Xor_Int128_WithSelf_ReturnsZero() {
    Int128 a = 12345;
    Assert.That(a.Xor(a), Is.EqualTo(Int128.Zero));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Int96 Not is self-inverse")]
  public void Not_Int96_DoubleNot_ReturnsOriginal() {
    Int96 a = 12345;
    Assert.That(a.Not().Not(), Is.EqualTo(a));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates rotation by zero returns original")]
  public void Rotate_ByZero_ReturnsOriginal() {
    UInt96 a96 = 0x123456;
    UInt128 a128 = 0x123456;
    Assert.That(a96.RotateLeft(0), Is.EqualTo(a96));
    Assert.That(a96.RotateRight(0), Is.EqualTo(a96));
    Assert.That(a128.RotateLeft(0), Is.EqualTo(a128));
    Assert.That(a128.RotateRight(0), Is.EqualTo(a128));
  }

  #endregion

}
