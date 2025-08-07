using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {
  [Test]
  [TestCase((short)0b0000000000000001, 0, 0b0000000000000001)]
  [TestCase((short)0b0000000000000001, 1, 0b0000000000000010)]
  [TestCase((short)0b0100000000000000, 1, 0b0000000000000000)]
  [TestCase((short)-(1U << 15), 1, (short)-(1U << 15))]
  [TestCase((short)0b0000000000000001, 15, 0b0000000000000000)]
  [TestCase((short)0b0000000000000001, 16, 0b0000000000000000)]
  [TestCase((short)0b0000000000000001, 32, 0b0000000000000000)]
  public void ArithmeticShiftLeftShort(short inp, byte count, short expected) => Assert.AreEqual(expected, inp.ArithmeticShiftLeft(count));

  [Test]
  [TestCase((short)0b0000000000000001, 0, 0b0000000000000001)]
  [TestCase((short)0b0000000000000010, 1, 0b0000000000000001)]
  [TestCase((short)-(1U << 15), 1, (short)-(1L << 14))]
  [TestCase((short)0b0100000000000000, 14, 0b0000000000000001)]
  [TestCase((short)0b0100000000000000, 15, 0b0000000000000000)]
  [TestCase((short)0b0100000000000000, 16, 0b0000000000000000)]
  [TestCase((short)0b0100000000000000, 32, 0b0000000000000000)]
  public void ArithmeticShiftRightShort(short inp, byte count, short expected) => Assert.AreEqual(expected, inp.ArithmeticShiftRight(count));

  [Test]
  [TestCase((short)0b0000000000000001, 0, (short)0b0000000000000001)]
  [TestCase((short)0b0000000000000001, 1, (short)0b0000000000000010)]
  [TestCase((short)0b0100000000000000, 1, unchecked((short)0b1000000000000000))]
  [TestCase((short)0b0100000000000000, 2, (short)0b0000000000000000)]
  [TestCase((short)0b0000000000000001, 16, (short)0b0000000000000000)]
  [TestCase((short)0b0000000000000001, 32, (short)0b0000000000000000)]
  public void LogicalShiftLeftShort(short inp, byte count, short expected) => Assert.AreEqual(expected, inp.LogicalShiftLeft(count));

  [Test]
  [TestCase((short)0b0000000000000001, 0, (short)0b0000000000000001)]
  [TestCase((short)0b0000000000000010, 1, (short)0b0000000000000001)]
  [TestCase(unchecked((short)0b1000000000000000), 1, (short)0b0100000000000000)]
  [TestCase(unchecked((short)0b1000000000000000), 15, (short)0b0000000000000001)]
  [TestCase(unchecked((short)0b1000000000000000), 16, (short)0b0000000000000000)]
  [TestCase(unchecked((short)0b1000000000000000), 32, (short)0b0000000000000000)]
  public void LogicalShiftRightShort(short inp, byte count, short expected) => Assert.AreEqual(expected, inp.LogicalShiftRight(count));

  [Test]
  [TestCase(0b00011011, 0b11011000, 0b00011000, 0b11011011, 0b11000011, 0b11100111, 0b00100100, 0b00111100)]
  public void BitwiseShort(byte self, byte operand, byte and, byte or, byte xor, byte nand, byte nor, byte equ) {
    var valuePack = new[] { self, operand, and, or, xor, nand, nor, equ };
    var wordPack = valuePack.Select(i => (ushort)((i << 8) | i)).ToArray();
    var shortValues = wordPack.Select(i => (short)i).ToArray();

    Assert.AreEqual(shortValues[2], shortValues[0].And(shortValues[1]), "And broken");
    Assert.AreEqual(shortValues[3], shortValues[0].Or(shortValues[1]), "Or broken");
    Assert.AreEqual(shortValues[4], shortValues[0].Xor(shortValues[1]), "Xor broken");
    Assert.AreEqual(shortValues[5], shortValues[0].Nand(shortValues[1]), "Nand broken");
    Assert.AreEqual(shortValues[6], shortValues[0].Nor(shortValues[1]), "Nor broken");
    Assert.AreEqual(shortValues[7], shortValues[0].Equ(shortValues[1]), "Equ broken");
  }
}
