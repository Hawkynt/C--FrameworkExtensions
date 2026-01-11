using NUnit.Framework;

namespace System;

[TestFixture]
public class ZigZagTests {

  // ZigZag8 tests
  [Test]
  [TestCase((sbyte)0, (byte)0)]
  [TestCase((sbyte)-1, (byte)1)]
  [TestCase((sbyte)1, (byte)2)]
  [TestCase((sbyte)-2, (byte)3)]
  [TestCase((sbyte)2, (byte)4)]
  [TestCase(sbyte.MaxValue, (byte)254)]
  [TestCase(sbyte.MinValue, (byte)255)]
  public void ZigZag8_EncodeDecode_RoundTrips(sbyte decoded, byte expectedEncoded) {
    ZigZag8 zz = decoded;
    Assert.AreEqual(expectedEncoded, zz.EncodedValue, "Encoding mismatch");
    Assert.AreEqual(decoded, zz.DecodedValue, "Decoding mismatch");
  }

  [Test]
  public void ZigZag8_Constants_AreCorrect() {
    Assert.AreEqual(0, ZigZag8.Zero.DecodedValue);
    Assert.AreEqual(sbyte.MaxValue, ZigZag8.MaxValue.DecodedValue);
    Assert.AreEqual(sbyte.MinValue, ZigZag8.MinValue.DecodedValue);
  }

  [Test]
  public void ZigZag8_Equality_Works() {
    ZigZag8 a = 5;
    ZigZag8 b = 5;
    ZigZag8 c = -5;
    Assert.IsTrue(a == b);
    Assert.IsFalse(a == c);
    Assert.IsTrue(a != c);
  }

  [Test]
  public void ZigZag8_Comparison_Works() {
    ZigZag8 negative = -5;
    ZigZag8 zero = 0;
    ZigZag8 positive = 5;
    Assert.IsTrue(negative < zero);
    Assert.IsTrue(zero < positive);
    Assert.IsTrue(positive > negative);
  }

  // ZigZag16 tests
  [Test]
  [TestCase((short)0, (ushort)0)]
  [TestCase((short)-1, (ushort)1)]
  [TestCase((short)1, (ushort)2)]
  [TestCase(short.MaxValue, (ushort)65534)]
  [TestCase(short.MinValue, (ushort)65535)]
  public void ZigZag16_EncodeDecode_RoundTrips(short decoded, ushort expectedEncoded) {
    ZigZag16 zz = decoded;
    Assert.AreEqual(expectedEncoded, zz.EncodedValue, "Encoding mismatch");
    Assert.AreEqual(decoded, zz.DecodedValue, "Decoding mismatch");
  }

  [Test]
  public void ZigZag16_WideningConversion_Works() {
    ZigZag8 small = 42;
    ZigZag16 wide = small;
    Assert.AreEqual(42, wide.DecodedValue);
  }

  // ZigZag32 tests
  [Test]
  [TestCase(0, 0u)]
  [TestCase(-1, 1u)]
  [TestCase(1, 2u)]
  [TestCase(int.MaxValue, 4294967294u)]
  [TestCase(int.MinValue, 4294967295u)]
  public void ZigZag32_EncodeDecode_RoundTrips(int decoded, uint expectedEncoded) {
    ZigZag32 zz = decoded;
    Assert.AreEqual(expectedEncoded, zz.EncodedValue, "Encoding mismatch");
    Assert.AreEqual(decoded, zz.DecodedValue, "Decoding mismatch");
  }

  // ZigZag64 tests
  [Test]
  [TestCase(0L, 0UL)]
  [TestCase(-1L, 1UL)]
  [TestCase(1L, 2UL)]
  [TestCase(long.MaxValue, 18446744073709551614UL)]
  [TestCase(long.MinValue, 18446744073709551615UL)]
  public void ZigZag64_EncodeDecode_RoundTrips(long decoded, ulong expectedEncoded) {
    ZigZag64 zz = decoded;
    Assert.AreEqual(expectedEncoded, zz.EncodedValue, "Encoding mismatch");
    Assert.AreEqual(decoded, zz.DecodedValue, "Decoding mismatch");
  }

  [Test]
  public void ZigZag64_WideningConversions_Work() {
    ZigZag8 zz8 = -100;
    ZigZag16 zz16 = -1000;
    ZigZag32 zz32 = -100000;

    ZigZag64 from8 = zz8;
    ZigZag64 from16 = zz16;
    ZigZag64 from32 = zz32;

    Assert.AreEqual(-100, from8.DecodedValue);
    Assert.AreEqual(-1000, from16.DecodedValue);
    Assert.AreEqual(-100000, from32.DecodedValue);
  }

}
