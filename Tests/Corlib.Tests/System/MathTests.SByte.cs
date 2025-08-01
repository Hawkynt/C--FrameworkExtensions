using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {

  [Test]
  [TestCase((sbyte)0b00000001, 0, 0b00000001)]
  [TestCase((sbyte)0b00000001, 1, 0b00000010)]
  [TestCase((sbyte)0b01000000, 1, 0b00000000)]
  [TestCase((sbyte)-(1U << 7), 1, (sbyte)-(1U << 7))]
  [TestCase((sbyte)0b00000001, 7, 0b00000000)]
  [TestCase((sbyte)0b00000001, 8, 0b00000000)]
  [TestCase((sbyte)0b00000001, 16, 0b00000000)]
  public void ArithmeticShiftLeftSByte(sbyte inp, byte count, sbyte expected) => Assert.AreEqual(expected, inp.ArithmeticShiftLeft(count));

  [Test]
  [TestCase((sbyte)0b00000001, 0, 0b00000001)]
  [TestCase((sbyte)0b00000010, 1, 0b00000001)]
  [TestCase(unchecked((sbyte)0x80), 1, unchecked((sbyte)0xC0))]
  [TestCase((sbyte)0b01000000, 6, 0b00000001)]
  [TestCase((sbyte)0b01000000, 7, 0b00000000)]
  [TestCase((sbyte)0b01000000, 8, 0b00000000)]
  [TestCase((sbyte)0b01000000, 16, 0b00000000)]
  public void ArithmeticShiftRightSByte(sbyte inp, byte count, sbyte expected) => Assert.AreEqual(expected, inp.ArithmeticShiftRight(count));

  [Test]
  [TestCase(unchecked((sbyte)0b00000001), 0, unchecked((sbyte)0b00000001))]
  [TestCase(unchecked((sbyte)0b00000001), 1, unchecked((sbyte)0b00000010))]
  [TestCase(unchecked((sbyte)0b01000000), 1, unchecked((sbyte)0b10000000))]
  [TestCase(unchecked((sbyte)0b01000000), 2, unchecked((sbyte)0b00000000))]
  [TestCase(unchecked((sbyte)0b00000001), 8, unchecked((sbyte)0b00000000))]
  [TestCase(unchecked((sbyte)0b00000001), 16, unchecked((sbyte)0b00000000))]
  public void LogicalShiftLeftSByte(sbyte inp, byte count, sbyte expected) => Assert.AreEqual(expected, inp.LogicalShiftLeft(count));

  [Test]
  [TestCase(unchecked((sbyte)0b00000001), 0, unchecked((sbyte)0b00000001))]
  [TestCase(unchecked((sbyte)0b00000010), 1, unchecked((sbyte)0b00000001))]
  [TestCase(unchecked((sbyte)0b10000000), 1, unchecked((sbyte)0b01000000))]
  [TestCase(unchecked((sbyte)0b10000000), 7, unchecked((sbyte)0b00000001))]
  [TestCase(unchecked((sbyte)0b10000000), 8, unchecked((sbyte)0b00000000))]
  [TestCase(unchecked((sbyte)0b10000000), 16, unchecked((sbyte)0b00000000))]
  public void LogicalShiftRightSByte(sbyte inp, byte count, sbyte expected) => Assert.AreEqual(expected, inp.LogicalShiftRight(count));

  [Test]
  [TestCase(0b00011011, 0b11011000, 0b00011000, 0b11011011, 0b11000011, 0b11100111, 0b00100100, 0b00111100)]
  public void BitwiseSByte(byte self, byte operand, byte and, byte or, byte xor, byte nand, byte nor, byte equ) {
    var valuePack = new []{self, operand, and, or, xor, nand, nor, equ};
    var sbyteValues = valuePack.Select(i=>(sbyte)i).ToArray();
    
    Assert.AreEqual(sbyteValues[2], sbyteValues[0].And(sbyteValues[1]), "And broken");
    Assert.AreEqual(sbyteValues[3], sbyteValues[0].Or(sbyteValues[1]), "Or broken");
    Assert.AreEqual(sbyteValues[4], sbyteValues[0].Xor(sbyteValues[1]), "Xor broken");
    Assert.AreEqual(sbyteValues[5], sbyteValues[0].Nand(sbyteValues[1]), "Nand broken");
    Assert.AreEqual(sbyteValues[6], sbyteValues[0].Nor(sbyteValues[1]), "Nor broken");
    Assert.AreEqual(sbyteValues[7], sbyteValues[0].Equ(sbyteValues[1]), "Equ broken");
  }

}