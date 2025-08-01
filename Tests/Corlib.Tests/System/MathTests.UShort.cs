using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public partial class MathTests {

  [Test]
  [TestCase((ushort)0b0000000000000001, 0, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b0000000000000001, 1, (ushort)0b0000000000000010)]
  [TestCase((ushort)0b0000000000000001, 15, (ushort)0b1000000000000000)]
  [TestCase((ushort)0b0000000000000001, 16, (ushort)0b0000000000000000)]
  [TestCase((ushort)0b0000000000000001, 32, (ushort)0b0000000000000000)]
  public void ArithmeticShiftLeftWord(ushort inp, byte count, ushort expected) => Assert.AreEqual(expected, inp.ArithmeticShiftLeft(count));

  [Test]
  [TestCase((ushort)0b0000000000000001, 0, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b0000000000000010, 1, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1000000000000000, 15, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1000000000000000, 16, (ushort)0b0000000000000000)]
  [TestCase((ushort)0b1000000000000000, 32, (ushort)0b0000000000000000)]
  public void ArithmeticShiftRightWord(ushort inp, byte count, ushort expected) => Assert.AreEqual(expected, inp.ArithmeticShiftRight(count));

  [Test]
  [TestCase((ushort)0b0000000000000001, 0, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b0000000000000001, 1, (ushort)0b0000000000000010)]
  [TestCase((ushort)0b1000000000000001, 1, (ushort)0b0000000000000011)]
  [TestCase((ushort)0b1000000000000000, 15, (ushort)0b0100000000000000)]
  [TestCase((ushort)0b1000000000000000, 16, (ushort)0b1000000000000000)]
  [TestCase((ushort)0b1000000000000000, 32, (ushort)0b1000000000000000)]
  public void RotateLeftWord(ushort inp, byte count, ushort expected) => Assert.AreEqual(expected, inp.RotateLeft(count));

  [Test]
  [TestCase((ushort)0b0000000000000001, 0, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b0000000000000010, 1, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1000000000000001, 1, (ushort)0b1100000000000000)]
  [TestCase((ushort)0b1000000000000000, 15, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1000000000000000, 16, (ushort)0b1000000000000000)]
  [TestCase((ushort)0b1000000000000000, 32, (ushort)0b1000000000000000)]
  public void RotateRightWord(ushort inp, byte count, ushort expected) => Assert.AreEqual(expected, inp.RotateRight(count));

  [Test]
  [TestCase(0b00011011, 0b11011000, 0b00011000, 0b11011011, 0b11000011, 0b11100111, 0b00100100, 0b00111100)]
  public void BitwiseUShort(byte self, byte operand, byte and, byte or, byte xor, byte nand, byte nor, byte equ) {
    var valuePack = new []{self, operand, and, or, xor, nand, nor, equ};
    var wordPack = valuePack.Select(i=>(ushort)(i<<8|i)).ToArray();
    
    Assert.AreEqual(wordPack[2], wordPack[0].And(wordPack[1]), "And broken");
    Assert.AreEqual(wordPack[3], wordPack[0].Or(wordPack[1]), "Or broken");
    Assert.AreEqual(wordPack[4], wordPack[0].Xor(wordPack[1]), "Xor broken");
    Assert.AreEqual(wordPack[5], wordPack[0].Nand(wordPack[1]), "Nand broken");
    Assert.AreEqual(wordPack[6], wordPack[0].Nor(wordPack[1]), "Nor broken");
    Assert.AreEqual(wordPack[7], wordPack[0].Equ(wordPack[1]), "Equ broken");
  }

  [Test]
  [TestCase((ushort)0b0000000000000000, (ushort)0b0000000000000000, (ushort)0b0000000000000000)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000000000, (ushort)0b0000000000000000)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b1111111111111111, (ushort)0b1111111111111111)]
  [TestCase((ushort)0b1010101010101010, (ushort)0b1111111100000000, (ushort)0b0000000010101010)]
  [TestCase((ushort)0b1010101010101010, (ushort)0b0000000011111111, (ushort)0b0000000010101010)]
  [TestCase((ushort)0b1111111100000000, (ushort)0b0000000011111111, (ushort)0b0000000000000000)]
  [TestCase((ushort)0b1010101010101010, (ushort)0b1010101010101010, (ushort)0b0000000011111111)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b1000000000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0100000000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0010000000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0001000000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000100000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000010000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000001000000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000100000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000010000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000001000000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000100000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000010000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000001000, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000000100, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000000010, (ushort)0b0000000000000001)]
  [TestCase((ushort)0b1111111111111111, (ushort)0b0000000000000001, (ushort)0b0000000000000001)]
  public void ParallelBitExtractWord(ushort value, ushort mask, ushort expected) => Assert.AreEqual(expected, value.ParallelBitExtract(mask));

}