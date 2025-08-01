using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {

  [Test]
  [TestCase((byte)0b00000001, 0, (byte)0b00000001)]
  [TestCase((byte)0b00000001, 1, (byte)0b00000010)]
  [TestCase((byte)0b01000000, 1, (byte)0b10000000)]
  [TestCase((byte)0b01000000, 2, (byte)0b00000000)]
  [TestCase((byte)0b00000001, 8, (byte)0b00000000)]
  [TestCase((byte)0b00000001, 16, (byte)0b00000000)]
  public void ArithmeticShiftLeftByte(byte inp, byte count, byte expected) => Assert.AreEqual(expected, inp.ArithmeticShiftLeft(count));

  [Test]
  [TestCase((byte)0b00000001, 0, (byte)0b00000001)]
  [TestCase((byte)0b00000010, 1, (byte)0b00000001)]
  [TestCase((byte)0b10000000, 7, (byte)0b00000001)]
  [TestCase((byte)0b10000000, 8, (byte)0b00000000)]
  [TestCase((byte)0b10000000, 16, (byte)0b00000000)]
  public void ArithmeticShiftRightByte(byte inp, byte count, byte expected) => Assert.AreEqual(expected, inp.ArithmeticShiftRight(count));

  [Test]
  [TestCase((byte)0b00000001, 0, (byte)0b00000001)]
  [TestCase((byte)0b00000001, 1, (byte)0b00000010)]
  [TestCase((byte)0b10000001, 1, (byte)0b00000011)]
  [TestCase((byte)0b10000000, 7, (byte)0b01000000)]
  [TestCase((byte)0b10000000, 8, (byte)0b10000000)]
  [TestCase((byte)0b10000000, 16, (byte)0b10000000)]
  public void RotateLeftByte(byte inp, byte count, byte expected) => Assert.AreEqual(expected, inp.RotateLeft(count));

  [Test]
  [TestCase((byte)0b00000001, 0, (byte)0b00000001)]
  [TestCase((byte)0b00000010, 1, (byte)0b00000001)]
  [TestCase((byte)0b10000001, 1, (byte)0b11000000)]
  [TestCase((byte)0b10000000, 7, (byte)0b00000001)]
  [TestCase((byte)0b10000000, 8, (byte)0b10000000)]
  [TestCase((byte)0b10000000, 16, (byte)0b10000000)]
  public void RotateRightByte(byte inp, byte count, byte expected) => Assert.AreEqual(expected, inp.RotateRight(count));

  [Test]
  [TestCase(0b00011011, 0b11011000, 0b00011000, 0b11011011, 0b11000011, 0b11100111, 0b00100100, 0b00111100)]
  public void BitwiseByte(byte self, byte operand, byte and, byte or, byte xor, byte nand, byte nor, byte equ) {
    Assert.AreEqual(and, self.And(operand), "And broken");
    Assert.AreEqual(or, self.Or(operand), "Or broken");
    Assert.AreEqual(xor, self.Xor(operand), "Xor broken");
    Assert.AreEqual(nand, self.Nand(operand), "Nand broken");
    Assert.AreEqual(nor, self.Nor(operand), "Nor broken");
    Assert.AreEqual(equ, self.Equ(operand), "Equ broken");
  }

  [Test]
  [TestCase(0b00000000, 0b00000000, 0b00000000)]
  [TestCase(0b11111111, 0b00000000, 0b00000000)]
  [TestCase(0b11111111, 0b11111111, 0b11111111)]
  [TestCase(0b10101010, 0b11110000, 0b00001010)]
  [TestCase(0b10101010, 0b00001111, 0b00001010)]
  [TestCase(0b11110000, 0b00001111, 0b00000000)]
  [TestCase(0b10101010, 0b10101010, 0b00001111)]
  [TestCase(0b11111111, 0b10000000, 0b00000001)]
  [TestCase(0b11111111, 0b01000000, 0b00000001)]
  [TestCase(0b11111111, 0b00100000, 0b00000001)]
  [TestCase(0b11111111, 0b00010000, 0b00000001)]
  [TestCase(0b11111111, 0b00001000, 0b00000001)]
  [TestCase(0b11111111, 0b00000100, 0b00000001)]
  [TestCase(0b11111111, 0b00000010, 0b00000001)]
  [TestCase(0b11111111, 0b00000001, 0b00000001)]
  public void ParallelBitExtractByte(byte value, byte mask, byte expected) => Assert.AreEqual(expected, value.ParallelBitExtract(mask));
  
}