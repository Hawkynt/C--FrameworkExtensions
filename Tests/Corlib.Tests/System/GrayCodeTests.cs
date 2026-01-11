using NUnit.Framework;

namespace System;

[TestFixture]
public class GrayCodeTests {

  // Gray8 tests
  [Test]
  [TestCase((byte)0, (byte)0)]
  [TestCase((byte)1, (byte)1)]
  [TestCase((byte)2, (byte)3)]
  [TestCase((byte)3, (byte)2)]
  [TestCase((byte)4, (byte)6)]
  [TestCase((byte)5, (byte)7)]
  [TestCase((byte)6, (byte)5)]
  [TestCase((byte)7, (byte)4)]
  [TestCase((byte)255, (byte)128)]
  public void Gray8_EncodeDecode_RoundTrips(byte binary, byte expectedGray) {
    var gray = Gray8.FromBinary(binary);
    Assert.AreEqual(expectedGray, gray.GrayValue, "Gray encoding mismatch");
    Assert.AreEqual(binary, gray.BinaryValue, "Binary decoding mismatch");
  }

  [Test]
  public void Gray8_Constants_AreCorrect() {
    Assert.AreEqual((byte)0, Gray8.Zero.BinaryValue);
    Assert.AreEqual((byte)0, Gray8.MinValue.BinaryValue);
  }

  [Test]
  public void Gray8_Equality_Works() {
    Gray8 a = 5;
    Gray8 b = 5;
    Gray8 c = 10;
    Assert.IsTrue(a == b);
    Assert.IsFalse(a == c);
    Assert.IsTrue(a != c);
    Assert.IsTrue(a.Equals(b));
  }

  [Test]
  public void Gray8_Comparison_Works() {
    Gray8 small = 5;
    Gray8 medium = 10;
    Gray8 large = 20;
    Assert.IsTrue(small < medium);
    Assert.IsTrue(medium < large);
    Assert.IsTrue(large > small);
    Assert.IsTrue(small <= medium);
    Assert.IsTrue(small >= small);
  }

  [Test]
  public void Gray8_Increment_Works() {
    Gray8 value = 5;
    value++;
    Assert.AreEqual((byte)6, value.BinaryValue);
  }

  [Test]
  public void Gray8_SuccessiveValues_DifferByOneBit() {
    for (byte i = 0; i < 255; ++i) {
      var current = Gray8.FromBinary(i);
      var next = Gray8.FromBinary((byte)(i + 1));
      var xor = (byte)(current.GrayValue ^ next.GrayValue);
      var bitCount = _CountBits(xor);
      Assert.AreEqual(1, bitCount, $"Gray({i}) ^ Gray({i + 1}) should differ by exactly 1 bit");
    }
  }

  // Gray16 tests
  [Test]
  [TestCase((ushort)0, (ushort)0)]
  [TestCase((ushort)1, (ushort)1)]
  [TestCase((ushort)2, (ushort)3)]
  [TestCase((ushort)3, (ushort)2)]
  [TestCase((ushort)1000, (ushort)540)]
  public void Gray16_EncodeDecode_RoundTrips(ushort binary, ushort expectedGray) {
    var gray = Gray16.FromBinary(binary);
    Assert.AreEqual(expectedGray, gray.GrayValue, "Gray encoding mismatch");
    Assert.AreEqual(binary, gray.BinaryValue, "Binary decoding mismatch");
  }

  [Test]
  public void Gray16_WideningConversion_Works() {
    Gray8 small = 42;
    Gray16 wide = small;
    Assert.AreEqual((ushort)42, wide.BinaryValue);
  }

  [Test]
  public void Gray16_SuccessiveValues_DifferByOneBit() {
    for (ushort i = 0; i < 1000; ++i) {
      var current = Gray16.FromBinary(i);
      var next = Gray16.FromBinary((ushort)(i + 1));
      var xor = (ushort)(current.GrayValue ^ next.GrayValue);
      var bitCount = _CountBits(xor);
      Assert.AreEqual(1, bitCount, $"Gray({i}) ^ Gray({i + 1}) should differ by exactly 1 bit");
    }
  }

  // Gray32 tests
  [Test]
  [TestCase(0u, 0u)]
  [TestCase(1u, 1u)]
  [TestCase(2u, 3u)]
  [TestCase(3u, 2u)]
  [TestCase(1000000u, 582496u)]
  public void Gray32_EncodeDecode_RoundTrips(uint binary, uint expectedGray) {
    var gray = Gray32.FromBinary(binary);
    Assert.AreEqual(expectedGray, gray.GrayValue, "Gray encoding mismatch");
    Assert.AreEqual(binary, gray.BinaryValue, "Binary decoding mismatch");
  }

  [Test]
  public void Gray32_Comparison_Works() {
    var a = Gray32.FromBinary(100);
    var b = Gray32.FromBinary(200);
    var c = Gray32.FromBinary(100);
    Assert.IsTrue(a < b);
    Assert.IsTrue(b > a);
    Assert.IsTrue(a == c);
    Assert.IsFalse(a != c);
  }

  // Gray64 tests
  [Test]
  [TestCase(0UL, 0UL)]
  [TestCase(1UL, 1UL)]
  [TestCase(2UL, 3UL)]
  [TestCase(3UL, 2UL)]
  [TestCase(1000000000UL, 643280640UL)]
  public void Gray64_EncodeDecode_RoundTrips(ulong binary, ulong expectedGray) {
    var gray = Gray64.FromBinary(binary);
    Assert.AreEqual(expectedGray, gray.GrayValue, "Gray encoding mismatch");
    Assert.AreEqual(binary, gray.BinaryValue, "Binary decoding mismatch");
  }

  [Test]
  public void Gray64_WideningConversions_Work() {
    Gray8 g8 = 100;
    Gray16 g16 = 1000;
    Gray32 g32 = 100000;

    Gray64 from8 = g8;
    Gray64 from16 = g16;
    Gray64 from32 = g32;

    Assert.AreEqual(100UL, from8.BinaryValue);
    Assert.AreEqual(1000UL, from16.BinaryValue);
    Assert.AreEqual(100000UL, from32.BinaryValue);
  }

  [Test]
  public void Gray64_FromGray_Works() {
    var gray = Gray64.FromGray(12345UL);
    Assert.AreEqual(12345UL, gray.GrayValue);
  }

  private static int _CountBits(uint value) {
    var count = 0;
    while (value != 0) {
      count += (int)(value & 1);
      value >>= 1;
    }
    return count;
  }
}
