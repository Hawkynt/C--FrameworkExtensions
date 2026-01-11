using NUnit.Framework;

namespace System;

[TestFixture]
public class BCDTests {

  // PackedBCD8 tests
  [Test]
  [TestCase(0, (byte)0x00)]
  [TestCase(9, (byte)0x09)]
  [TestCase(10, (byte)0x10)]
  [TestCase(42, (byte)0x42)]
  [TestCase(99, (byte)0x99)]
  public void PackedBCD8_Encoding_IsCorrect(int value, byte expectedRaw) {
    var bcd = PackedBCD8.FromValue(value);
    Assert.AreEqual(expectedRaw, bcd.RawValue);
    Assert.AreEqual(value, bcd.Value);
  }

  [Test]
  public void PackedBCD8_Constants_AreCorrect() {
    Assert.AreEqual(0, PackedBCD8.Zero.Value);
    Assert.AreEqual(1, PackedBCD8.One.Value);
    Assert.AreEqual(99, PackedBCD8.MaxValue.Value);
    Assert.AreEqual(0, PackedBCD8.MinValue.Value);
  }

  [Test]
  public void PackedBCD8_Arithmetic_Works() {
    var a = PackedBCD8.FromValue(50);
    var b = PackedBCD8.FromValue(25);
    Assert.AreEqual(75, (a + b).Value);
    Assert.AreEqual(25, (a - b).Value);
  }

  [Test]
  public void PackedBCD8_Overflow_Throws() {
    var a = PackedBCD8.FromValue(99);
    var b = PackedBCD8.FromValue(1);
    Assert.Throws<OverflowException>(() => { var _ = a + b; });
  }

  [Test]
  public void PackedBCD8_InvalidRaw_Throws() {
    Assert.Throws<ArgumentException>(() => PackedBCD8.FromRaw(0xAA));
    Assert.Throws<ArgumentException>(() => PackedBCD8.FromRaw(0x1A));
    Assert.Throws<ArgumentException>(() => PackedBCD8.FromRaw(0xA1));
  }

  // PackedBCD16 tests
  [Test]
  [TestCase(0, (ushort)0x0000)]
  [TestCase(1234, (ushort)0x1234)]
  [TestCase(9999, (ushort)0x9999)]
  public void PackedBCD16_Encoding_IsCorrect(int value, ushort expectedRaw) {
    var bcd = PackedBCD16.FromValue(value);
    Assert.AreEqual(expectedRaw, bcd.RawValue);
    Assert.AreEqual(value, bcd.Value);
  }

  [Test]
  public void PackedBCD16_Arithmetic_Works() {
    var a = PackedBCD16.FromValue(5000);
    var b = PackedBCD16.FromValue(2500);
    Assert.AreEqual(7500, (a + b).Value);
    Assert.AreEqual(2500, (a - b).Value);
  }

  // PackedBCD32 tests
  [Test]
  [TestCase(0, 0x00000000u)]
  [TestCase(12345678, 0x12345678u)]
  [TestCase(99999999, 0x99999999u)]
  public void PackedBCD32_Encoding_IsCorrect(int value, uint expectedRaw) {
    var bcd = PackedBCD32.FromValue(value);
    Assert.AreEqual(expectedRaw, bcd.RawValue);
    Assert.AreEqual(value, bcd.Value);
  }

  [Test]
  public void PackedBCD32_Comparison_Works() {
    var a = PackedBCD32.FromValue(12345678);
    var b = PackedBCD32.FromValue(12345679);
    var c = PackedBCD32.FromValue(12345678);
    Assert.IsTrue(a < b);
    Assert.IsTrue(b > a);
    Assert.IsTrue(a == c);
    Assert.IsFalse(a != c);
  }

  // UnpackedBCD tests
  [Test]
  [TestCase(0)]
  [TestCase(5)]
  [TestCase(9)]
  public void UnpackedBCD_Values_AreCorrect(int value) {
    var bcd = UnpackedBCD.FromValue(value);
    Assert.AreEqual(value, bcd.Value);
    Assert.AreEqual((byte)value, bcd.RawValue);
  }

  [Test]
  public void UnpackedBCD_OutOfRange_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => UnpackedBCD.FromValue(10));
    Assert.Throws<ArgumentOutOfRangeException>(() => UnpackedBCD.FromValue(-1));
  }

  [Test]
  public void UnpackedBCD_Arithmetic_Works() {
    var a = UnpackedBCD.FromValue(5);
    var b = UnpackedBCD.FromValue(3);
    Assert.AreEqual(8, (a + b).Value);
    Assert.AreEqual(2, (a - b).Value);
  }

  [Test]
  public void UnpackedBCD_Overflow_Throws() {
    var a = UnpackedBCD.FromValue(9);
    var b = UnpackedBCD.FromValue(1);
    Assert.Throws<OverflowException>(() => { var _ = a + b; });
  }

  // Widening conversions
  [Test]
  public void PackedBCD8_ToPackedBCD16_Widening_Works() {
    PackedBCD8 small = PackedBCD8.FromValue(42);
    PackedBCD16 wide = small;
    Assert.AreEqual(42, wide.Value);
  }

  [Test]
  public void UnpackedBCD_ToPackedBCD_Widening_Works() {
    UnpackedBCD digit = UnpackedBCD.FromValue(7);
    PackedBCD8 packed8 = digit;
    PackedBCD16 packed16 = digit;
    PackedBCD32 packed32 = digit;
    Assert.AreEqual(7, packed8.Value);
    Assert.AreEqual(7, packed16.Value);
    Assert.AreEqual(7, packed32.Value);
  }

}
