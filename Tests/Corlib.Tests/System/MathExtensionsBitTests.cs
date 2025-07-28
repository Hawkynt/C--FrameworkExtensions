using NUnit.Framework;

namespace System;

[TestFixture]
public class MathExtensionsBitTests {
    [Test]
    [TestCase((byte)0xAB, (byte)0x0B)]
    public void LowerHalfByte(byte value, byte expected) => Assert.AreEqual(expected, value.LowerHalf());

    [Test]
    [TestCase((byte)0xAB, (byte)0x0A)]
    public void UpperHalfByte(byte value, byte expected) => Assert.AreEqual(expected, value.UpperHalf());

    [Test]
    [TestCase((ushort)0xABCD, (byte)0xCD)]
    public void LowerHalfWord(ushort value, byte expected) => Assert.AreEqual(expected, value.LowerHalf());

    [Test]
    [TestCase((ushort)0xABCD, (byte)0xAB)]
    public void UpperHalfWord(ushort value, byte expected) => Assert.AreEqual(expected, value.UpperHalf());

    [Test]
    [TestCase((uint)0xABCDEF12, (ushort)0xEF12)]
    public void LowerHalfDWord(uint value, ushort expected) => Assert.AreEqual(expected, value.LowerHalf());

    [Test]
    [TestCase((uint)0xABCDEF12, (ushort)0xABCD)]
    public void UpperHalfDWord(uint value, ushort expected) => Assert.AreEqual(expected, value.UpperHalf());

    [Test]
    [TestCase(0b_0010_0000, (byte)5)]
    [TestCase(0, (byte)8)]
    public void TrailingZeroCountByte(byte value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

    [Test]
    [TestCase((ushort)0b_1111_1100_0010_0000, (byte)5)]
    [TestCase((ushort)0, (byte)16)]
    public void TrailingZeroCountWord(ushort value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

    [Test]
    [TestCase((uint)0b1111_1111_0000_0000_0000_0000_0010_0000, (byte)5)]
    [TestCase((uint)0, (byte)32)]
    public void TrailingZeroCountDWord(uint value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

    [Test]
    [TestCase((ulong)0b_0010_0000UL, (byte)5)]
    [TestCase((ulong)0, (byte)64)]
    public void TrailingZeroCountQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.TrailingZeroCount());

    [Test]
    [TestCase(0b_0000_0100, (byte)5)]
    [TestCase(0, (byte)8)]
    public void LeadingZeroCountByte(byte value, byte expected) => Assert.AreEqual(expected, value.LeadingZeroCount());

    [Test]
    [TestCase((ushort)0b_0000_0100, (byte)13)]
    [TestCase((ushort)0, (byte)16)]
    public void LeadingZeroCountWord(ushort value, byte expected) => Assert.AreEqual(expected, value.LeadingZeroCount());

    [Test]
    [TestCase((uint)0b_0000_0100U, (byte)29)]
    [TestCase((uint)0, (byte)32)]
    public void LeadingZeroCountDWord(uint value, byte expected) => Assert.AreEqual(expected, value.LeadingZeroCount());

    [Test]
    [TestCase((ulong)0b_0000_0100UL, (byte)61)]
    [TestCase((ulong)0, (byte)64)]
    public void LeadingZeroCountQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.LeadingZeroCount());

    [Test]
    [TestCase(0b_0011_1111, (byte)6)]
    public void TrailingOneCountByte(byte value, byte expected) => Assert.AreEqual(expected, value.TrailingOneCount());

    [Test]
    [TestCase((ushort)0b_0011_1111, (byte)6)]
    public void TrailingOneCountWord(ushort value, byte expected) => Assert.AreEqual(expected, value.TrailingOneCount());

    [Test]
    [TestCase((uint)0b_0011_1111U, (byte)6)]
    public void TrailingOneCountDWord(uint value, byte expected) => Assert.AreEqual(expected, value.TrailingOneCount());

    [Test]
    [TestCase((ulong)0b_0011_1111UL, (byte)6)]
    public void TrailingOneCountQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.TrailingOneCount());

    [Test]
    [TestCase(0b_1111_0000, (byte)4)]
    public void LeadingOneCountByte(byte value, byte expected) => Assert.AreEqual(expected, value.LeadingOneCount());

    [Test]
    [TestCase((ushort)0b_1111_0000_1010_1010, (byte)4)]
    public void LeadingOneCountWord(ushort value, byte expected) => Assert.AreEqual(expected, value.LeadingOneCount());

    [Test]
    [TestCase((uint)0b_1111_0000_1010_1010_1111_0000_1010_1010U, (byte)4)]
    public void LeadingOneCountDWord(uint value, byte expected) => Assert.AreEqual(expected, value.LeadingOneCount());

    [Test]
    [TestCase((ulong)0b_1111_0000_1010_1010_1111_0000_1010_1010_1111_0000_1010_1010_1111_0000_1010_1010UL, (byte)4)]
    public void LeadingOneCountQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.LeadingOneCount());

    [Test]
    [TestCase(0b_1101_0101, (byte)5)]
    public void CountSetBitsByte(byte value, byte expected) => Assert.AreEqual(expected, value.CountSetBits());

    [Test]
    [TestCase((ushort)0b_1101_0101, (byte)5)]
    public void CountSetBitsWord(ushort value, byte expected) => Assert.AreEqual(expected, value.CountSetBits());

    [Test]
    [TestCase((uint)0b_1101_0101U, (byte)5)]
    public void CountSetBitsDWord(uint value, byte expected) => Assert.AreEqual(expected, value.CountSetBits());

    [Test]
    [TestCase((ulong)0b_1101_0101UL, (byte)5)]
    public void CountSetBitsQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.CountSetBits());

    [Test]
    [TestCase(0b_1101_0101, (byte)3)]
    public void CountUnsetBitsByte(byte value, byte expected) => Assert.AreEqual(expected, value.CountUnsetBits());

    [Test]
    [TestCase((ushort)0b_1101_0101, (byte)11)]
    public void CountUnsetBitsWord(ushort value, byte expected) => Assert.AreEqual(expected, value.CountUnsetBits());

    [Test]
    [TestCase((uint)0b_1101_0101U, (byte)27)]
    public void CountUnsetBitsDWord(uint value, byte expected) => Assert.AreEqual(expected, value.CountUnsetBits());

    [Test]
    [TestCase((ulong)0b_1101_0101UL, (byte)59)]
    public void CountUnsetBitsQWord(ulong value, byte expected) => Assert.AreEqual(expected, value.CountUnsetBits());

    [Test]
    [TestCase(0b_1110_0100, true)]
    [TestCase(0b_1110_0101, false)]
    public void ParityByte(byte value, bool expected) => Assert.AreEqual(expected, value.Parity());

    [Test]
    [TestCase((ushort)0b_1110_0100, true)]
    [TestCase((ushort)0b_1110_0101, false)]
    public void ParityWord(ushort value, bool expected) => Assert.AreEqual(expected, value.Parity());

    [Test]
    [TestCase((uint)0b_1110_0100U, true)]
    [TestCase((uint)0b_1110_0101U, false)]
    public void ParityDWord(uint value, bool expected) => Assert.AreEqual(expected, value.Parity());

    [Test]
    [TestCase((ulong)0b_1110_0100UL, true)]
    [TestCase((ulong)0b_1110_0101UL, false)]
    public void ParityQWord(ulong value, bool expected) => Assert.AreEqual(expected, value.Parity());

    [Test]
    [TestCase(0b_0001_0110, (byte)0b_0110_1000)]
    public void ReverseBitsByte(byte value, byte expected) => Assert.AreEqual(expected, value.ReverseBits());

    [Test]
    [TestCase((ushort)0b_0001_0110, (ushort)0b_0110_1000_0000_0000)]
    public void ReverseBitsWord(ushort value, ushort expected) => Assert.AreEqual(expected, value.ReverseBits());

    [Test]
    [TestCase((uint)0b_0001_0110U, (uint)0b_0110_1000_0000_0000_0000_0000_0000_0000)]
    public void ReverseBitsDWord(uint value, uint expected) => Assert.AreEqual(expected, value.ReverseBits());

    [Test]
    [TestCase((ulong)0b_0001_0110UL, (ulong)0b_0110_1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000)]
    public void ReverseBitsQWord(ulong value, ulong expected) => Assert.AreEqual(expected, value.ReverseBits());

    [Test]
    [TestCase(0b_1010_1010, 1, 0b_1010_1011)]
    [TestCase(0b_1010_1010, 7, 0b_0010_1010)]
    public void SetBitByte(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.SetBit(index));

    [Test]
    [TestCase(0b_1010_1010, 1, 0b_1010_1000)]
    [TestCase(0b_1010_1010, 7, 0b_0010_1010)]
    public void ClearBitByte(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.ClearBit(index));

    [Test]
    [TestCase(0b_1010_1010, 0, 0b_1010_1011)]
    [TestCase(0b_1010_1010, 7, 0b_0010_1010)]
    public void FlipBitByte(byte value, byte index, byte expected) => Assert.AreEqual(expected, value.FlipBit(index));

    [Test]
    [TestCase((ushort)0b_1100_1100_1010_1010, (byte)0b_1111, (byte)0b_0000)]
    public void PairwiseDeinterleaveBitsWord(ushort value, byte oddExpected, byte evenExpected) {
        var (odd, even) = value.PairwiseDeinterleaveBits();
        Assert.AreEqual(oddExpected, odd);
        Assert.AreEqual(evenExpected, even);
    }

    [Test]
    [TestCase(0b_1111_0000, 0, 4, (byte)0b_1111)]
    [TestCase(0b_1010_1010, 1, 3, (byte)0b_0001_0101)]
    public void BitsByte(byte value, byte index, byte count, byte expected) => Assert.AreEqual(expected, value.Bits(index, count));
}
