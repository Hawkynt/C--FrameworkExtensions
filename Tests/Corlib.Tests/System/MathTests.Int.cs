using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {
  [Test]
  [TestCase(0b00000000000000000000000000000001, 0, 0b00000000000000000000000000000001)]
  [TestCase(0b00000000000000000000000000000001, 1, 0b00000000000000000000000000000010)]
  [TestCase(0b01000000000000000000000000000000, 1, 0b00000000000000000000000000000000)]
  [TestCase((int)-(1U << 31), 1, (int)-(1U << 31))]
  [TestCase(0b00000000000000000000000000000001, 31, 0b00000000000000000000000000000000)]
  [TestCase(0b00000000000000000000000000000001, 32, 0b00000000000000000000000000000000)]
  [TestCase(0b00000000000000000000000000000001, 64, 0b00000000000000000000000000000000)]
  public void ArithmeticShiftLeftInt(int inp, byte count, int expected) => Assert.AreEqual(expected, inp.ArithmeticShiftLeft(count));

  [Test]
  [TestCase(0b00000000000000000000000000000001, 0, 0b00000000000000000000000000000001)]
  [TestCase(0b00000000000000000000000000000010, 1, 0b00000000000000000000000000000001)]
  [TestCase((int)-(1U << 31), 1, (int)-(1L << 30))]
  [TestCase(0b01000000000000000000000000000000, 30, 0b00000000000000000000000000000001)]
  [TestCase(0b01000000000000000000000000000000, 31, 0b00000000000000000000000000000000)]
  [TestCase(0b01000000000000000000000000000000, 64, 0b00000000000000000000000000000000)]
  public void ArithmeticShiftRightInt(int inp, byte count, int expected) => Assert.AreEqual(expected, inp.ArithmeticShiftRight(count));

  [Test]
  [TestCase(0b00000000000000000000000000000001, 0, 0b00000000000000000000000000000001)]
  [TestCase(0b00000000000000000000000000000001, 1, 0b00000000000000000000000000000010)]
  [TestCase(0b01000000000000000000000000000000, 1, unchecked((int)0b10000000000000000000000000000000))]
  [TestCase(0b01000000000000000000000000000000, 2, 0b00000000000000000000000000000000)]
  [TestCase(0b00000000000000000000000000000001, 32, 0b00000000000000000000000000000000)]
  [TestCase(0b00000000000000000000000000000001, 64, 0b00000000000000000000000000000000)]
  public void LogicalShiftLeftInt(int inp, byte count, int expected) => Assert.AreEqual(expected, inp.LogicalShiftLeft(count));

  [Test]
  [TestCase(0b00000000000000000000000000000001, 0, 0b00000000000000000000000000000001)]
  [TestCase(0b00000000000000000000000000000010, 1, 0b00000000000000000000000000000001)]
  [TestCase(unchecked((int)0b10000000000000000000000000000000), 1, 0b01000000000000000000000000000000)]
  [TestCase(unchecked((int)0b10000000000000000000000000000000), 31, 0b00000000000000000000000000000001)]
  [TestCase(unchecked((int)0b10000000000000000000000000000000), 32, 0b00000000000000000000000000000000)]
  [TestCase(unchecked((int)0b10000000000000000000000000000000), 64, 0b00000000000000000000000000000000)]
  public void LogicalShiftRightInt(int inp, byte count, int expected) => Assert.AreEqual(expected, inp.LogicalShiftRight(count));

  [Test]
  [TestCase(0b00011011, 0b11011000, 0b00011000, 0b11011011, 0b11000011, 0b11100111, 0b00100100, 0b00111100)]
  public void BitwiseInt(byte self, byte operand, byte and, byte or, byte xor, byte nand, byte nor, byte equ) {
    var valuePack = new[] { self, operand, and, or, xor, nand, nor, equ };
    var wordPack = valuePack.Select(i => (ushort)((i << 8) | i)).ToArray();
    var dwordPack = wordPack.Select(i => (uint)((i << 16) | i)).ToArray();
    var intValues = dwordPack.Select(i => (int)i).ToArray();

    Assert.AreEqual(intValues[2], intValues[0].And(intValues[1]), "And broken");
    Assert.AreEqual(intValues[3], intValues[0].Or(intValues[1]), "Or broken");
    Assert.AreEqual(intValues[4], intValues[0].Xor(intValues[1]), "Xor broken");
    Assert.AreEqual(intValues[5], intValues[0].Nand(intValues[1]), "Nand broken");
    Assert.AreEqual(intValues[6], intValues[0].Nor(intValues[1]), "Nor broken");
    Assert.AreEqual(intValues[7], intValues[0].Equ(intValues[1]), "Equ broken");
  }
}
