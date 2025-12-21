using NUnit.Framework;

namespace System.MathExtensionsTests;

[TestFixture]
[Category("Unit")]
public partial class MathTests {
  #region And Tests

  [Test]
  [TestCase((byte)0xFF, (byte)0x0F, (byte)0x0F)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0x00)]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0xFF)]
  [Category("HappyPath")]
  [Description("Validates byte And computes correctly")]
  public void And_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.And(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int And computes correctly")]
  public void And_Int_ComputesCorrectly() {
    Assert.That(0xFF.And(0x0F), Is.EqualTo(0x0F));
    Assert.That((-1).And(-1), Is.EqualTo(-1));
    Assert.That(0x7FFFFFFF.And(0x0FFFFFFF), Is.EqualTo(0x0FFFFFFF));
  }

  [Test]
  [TestCase(0xFFUL, 0x0FUL, 0x0FUL)]
  [TestCase(0xAAAAAAAAAAAAAAAAUL, 0x5555555555555555UL, 0UL)]
  [Category("HappyPath")]
  [Description("Validates ulong And computes correctly")]
  public void And_ULong_ComputesCorrectly(ulong a, ulong b, ulong expected) {
    var result = a.And(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Or Tests

  [Test]
  [TestCase((byte)0xF0, (byte)0x0F, (byte)0xFF)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0xFF)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0x00)]
  [Category("HappyPath")]
  [Description("Validates byte Or computes correctly")]
  public void Or_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.Or(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0xF0, 0x0F, 0xFF)]
  [TestCase(0, 0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Or computes correctly")]
  public void Or_Int_ComputesCorrectly(int a, int b, int expected) {
    var result = a.Or(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Xor Tests

  [Test]
  [TestCase((byte)0xFF, (byte)0x0F, (byte)0xF0)]
  [TestCase((byte)0xAA, (byte)0xAA, (byte)0x00)]
  [TestCase((byte)0x00, (byte)0xFF, (byte)0xFF)]
  [Category("HappyPath")]
  [Description("Validates byte Xor computes correctly")]
  public void Xor_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.Xor(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int Xor computes correctly")]
  public void Xor_Int_ComputesCorrectly() {
    Assert.That(0xFF.Xor(0x0F), Is.EqualTo(0xF0));
    Assert.That(0x12345678.Xor(0x12345678), Is.EqualTo(0));
    Assert.That((-1).Xor(-1), Is.EqualTo(0));
  }

  #endregion

  #region Not Tests

  [Test]
  [TestCase((byte)0x00, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0x00)]
  [TestCase((byte)0xAA, (byte)0x55)]
  [Category("HappyPath")]
  [Description("Validates byte Not computes correctly")]
  public void Not_Byte_ComputesCorrectly(byte value, byte expected) {
    var result = value.Not();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0, -1)]
  [TestCase(-1, 0)]
  [Category("HappyPath")]
  [Description("Validates int Not computes correctly")]
  public void Not_Int_ComputesCorrectly(int value, int expected) {
    var result = value.Not();
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Nand Tests

  [Test]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0x00)]
  [TestCase((byte)0xFF, (byte)0x00, (byte)0xFF)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0xFF)]
  [Category("HappyPath")]
  [Description("Validates byte Nand computes correctly")]
  public void Nand_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.Nand(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-1, -1, 0)]
  [TestCase(-1, 0, -1)]
  [TestCase(0, 0, -1)]
  [Category("HappyPath")]
  [Description("Validates int Nand computes correctly")]
  public void Nand_Int_ComputesCorrectly(int a, int b, int expected) {
    var result = a.Nand(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Nor Tests

  [Test]
  [TestCase((byte)0x00, (byte)0x00, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0x00, (byte)0x00)]
  [TestCase((byte)0x00, (byte)0xFF, (byte)0x00)]
  [Category("HappyPath")]
  [Description("Validates byte Nor computes correctly")]
  public void Nor_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.Nor(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(0, 0, -1)]
  [TestCase(-1, 0, 0)]
  [TestCase(0, -1, 0)]
  [Category("HappyPath")]
  [Description("Validates int Nor computes correctly")]
  public void Nor_Int_ComputesCorrectly(int a, int b, int expected) {
    var result = a.Nor(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Equ Tests

  [Test]
  [TestCase((byte)0xFF, (byte)0xFF, (byte)0xFF)]
  [TestCase((byte)0x00, (byte)0x00, (byte)0xFF)]
  [TestCase((byte)0xFF, (byte)0x00, (byte)0x00)]
  [TestCase((byte)0xAA, (byte)0x55, (byte)0x00)]
  [Category("HappyPath")]
  [Description("Validates byte Equ (XNOR) computes correctly")]
  public void Equ_Byte_ComputesCorrectly(byte a, byte b, byte expected) {
    var result = a.Equ(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [TestCase(-1, -1, -1)]
  [TestCase(0, 0, -1)]
  [TestCase(-1, 0, 0)]
  [Category("HappyPath")]
  [Description("Validates int Equ (XNOR) computes correctly")]
  public void Equ_Int_ComputesCorrectly(int a, int b, int expected) {
    var result = a.Equ(b);
    Assert.That(result, Is.EqualTo(expected));
  }

  #endregion

  #region Bits Tests

  [Test]
  [TestCase((byte)0b11110000, (byte)4, (byte)4, (byte)0b1111)]
  [TestCase((byte)0b11001100, (byte)2, (byte)4, (byte)0b0011)]
  [TestCase((byte)0xFF, (byte)0, (byte)8, (byte)0xFF)]
  [Category("HappyPath")]
  [Description("Validates byte Bits extraction works correctly")]
  public void Bits_Byte_ExtractsCorrectly(byte value, byte offset, byte count, byte expected) {
    var result = value.Bits(offset, count);
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates int Bits extraction works correctly")]
  public void Bits_Int_ExtractsCorrectly() {
    Assert.That(0b11110000.Bits(4, 4), Is.EqualTo(0b1111));
    Assert.That(0b11001100.Bits(2, 4), Is.EqualTo(0b0011));
    Assert.That((-1).Bits(0, 8), Is.EqualTo(0xFF));
  }

  [Test]
  [Category("HappyPath")]
  [Description("Validates ulong Bits extraction works correctly")]
  public void Bits_ULong_ExtractsCorrectly() {
    Assert.That(0b11110000UL.Bits(4, 4), Is.EqualTo(0b1111UL));
  }

  #endregion

  #region EdgeCase Tests - Bitwise Boundaries

  [Test]
  [Category("EdgeCase")]
  [Description("Validates And with zero always returns zero")]
  public void And_Int_WithZero_ReturnsZero() {
    Assert.That(0.And(0), Is.EqualTo(0));
    Assert.That(int.MaxValue.And(0), Is.EqualTo(0));
    Assert.That(int.MinValue.And(0), Is.EqualTo(0));
    Assert.That((-1).And(0), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates And with -1 (all bits set) returns original")]
  public void And_Int_WithAllOnes_ReturnsOriginal() {
    Assert.That(12345.And(-1), Is.EqualTo(12345));
    Assert.That((-12345).And(-1), Is.EqualTo(-12345));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Or with zero returns original")]
  public void Or_Int_WithZero_ReturnsOriginal() {
    Assert.That(12345.Or(0), Is.EqualTo(12345));
    Assert.That((-12345).Or(0), Is.EqualTo(-12345));
    Assert.That(0.Or(0), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Or with -1 (all bits set) returns -1")]
  public void Or_Int_WithAllOnes_ReturnsAllOnes() {
    Assert.That(12345.Or(-1), Is.EqualTo(-1));
    Assert.That(0.Or(-1), Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Xor with itself returns zero")]
  public void Xor_Int_WithSelf_ReturnsZero() {
    Assert.That(12345.Xor(12345), Is.EqualTo(0));
    Assert.That((-12345).Xor(-12345), Is.EqualTo(0));
    Assert.That(int.MaxValue.Xor(int.MaxValue), Is.EqualTo(0));
    Assert.That(int.MinValue.Xor(int.MinValue), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Xor with zero returns original")]
  public void Xor_Int_WithZero_ReturnsOriginal() {
    Assert.That(12345.Xor(0), Is.EqualTo(12345));
    Assert.That((-12345).Xor(0), Is.EqualTo(-12345));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Not is self-inverse")]
  public void Not_Int_DoubleNot_ReturnsOriginal() {
    Assert.That(12345.Not().Not(), Is.EqualTo(12345));
    Assert.That((-12345).Not().Not(), Is.EqualTo(-12345));
    Assert.That(0.Not().Not(), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Nand truth table for boundary values")]
  public void Nand_Int_BoundaryValues_ComputesCorrectly() {
    Assert.That(int.MaxValue.Nand(int.MaxValue), Is.EqualTo(int.MinValue));
    Assert.That(int.MaxValue.Nand(0), Is.EqualTo(-1));
    Assert.That(0.Nand(int.MaxValue), Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Nor truth table for boundary values")]
  public void Nor_Int_BoundaryValues_ComputesCorrectly() {
    Assert.That(int.MaxValue.Nor(0), Is.EqualTo(int.MinValue));
    Assert.That(0.Nor(int.MaxValue), Is.EqualTo(int.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Equ (XNOR) is self-inverse with Xor")]
  public void Equ_Int_XnorProperty_WorksCorrectly() {
    // XNOR should return all 1s when both inputs are same
    Assert.That(12345.Equ(12345), Is.EqualTo(-1));
    Assert.That((-12345).Equ(-12345), Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Bits extraction with zero count returns zero")]
  public void Bits_Int_ZeroCount_ReturnsZero() {
    Assert.That((-1).Bits(0, 0), Is.EqualTo(0));
    Assert.That(int.MaxValue.Bits(16, 0), Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates Bits extraction at boundaries")]
  public void Bits_Int_BoundaryExtraction_WorksCorrectly() {
    // Extract high bit only from int.MinValue
    Assert.That(int.MinValue.Bits(31, 1), Is.EqualTo(1));
    // Extract from position 0
    Assert.That(0b1111.Bits(0, 4), Is.EqualTo(0b1111));
    // Extract lower 8 bits
    Assert.That(0x12345678.Bits(0, 8), Is.EqualTo(0x78));
    // Extract middle 8 bits
    Assert.That(0x12345678.Bits(8, 8), Is.EqualTo(0x56));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates ulong bitwise operations with max value")]
  public void BitwiseOps_ULong_MaxValue_WorkCorrectly() {
    Assert.That(ulong.MaxValue.And(ulong.MaxValue), Is.EqualTo(ulong.MaxValue));
    Assert.That(ulong.MaxValue.Or(0UL), Is.EqualTo(ulong.MaxValue));
    Assert.That(ulong.MaxValue.Xor(ulong.MaxValue), Is.EqualTo(0UL));
    Assert.That(ulong.MaxValue.Not(), Is.EqualTo(0UL));
  }

  [Test]
  [Category("EdgeCase")]
  [Description("Validates byte bitwise operations preserve 8-bit range")]
  public void BitwiseOps_Byte_PreservesRange() {
    Assert.That(((byte)0xFF).Not(), Is.EqualTo((byte)0x00));
    Assert.That(((byte)0x00).Not(), Is.EqualTo((byte)0xFF));
    Assert.That(((byte)0xF0).Xor((byte)0x0F), Is.EqualTo((byte)0xFF));
  }

  #endregion
}
