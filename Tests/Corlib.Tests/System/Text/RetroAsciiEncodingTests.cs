using NUnit.Framework;

namespace System.Text;

/// <summary>
/// Tests for the ATASCII (Atari 8-bit) and Amstrad CPC text encodings: exact ASCII text range, lossless
/// byte round-trip (graphics/control via private-use placeholders), and plain-string encoding.
/// </summary>
[TestFixture]
public class RetroAsciiEncodingTests {

  private static readonly RetroSingleByteEncoding[] _all = [AtasciiEncoding.Instance, AmstradCpcEncoding.Instance];

  [Test]
  public void EveryByte_RoundTripsLosslessly() {
    foreach (var enc in _all)
      for (var b = 0; b < 256; ++b)
        Assert.AreEqual((byte)b, enc.ToByte(enc.ToChar((byte)b)), $"{enc.EncodingName} byte 0x{b:X2}");
  }

  [Test]
  public void Hello_EncodesToAsciiBytes() {
    byte[] expected = [0x48, 0x45, 0x4C, 0x4C, 0x4F]; // H E L L O
    foreach (var enc in _all) {
      Assert.AreEqual(expected, enc.GetBytes("HELLO"), enc.EncodingName);
      Assert.AreEqual("HELLO", enc.GetString(enc.GetBytes("HELLO")), enc.EncodingName);
    }
  }

  [Test]
  public void AsciiText_RoundTrips() {
    const string text = "Hello, World! 0123456789 (retro)";
    foreach (var enc in _all)
      Assert.AreEqual(text, enc.GetString(enc.GetBytes(text)), enc.EncodingName);
  }

  [Test]
  public void AsciiAnchors() {
    foreach (var enc in _all) {
      Assert.AreEqual(0x41, enc.ToByte('A'), enc.EncodingName);
      Assert.AreEqual(0x7A, enc.ToByte('z'), enc.EncodingName);
      Assert.AreEqual(0x30, enc.ToByte('0'), enc.EncodingName);
      Assert.AreEqual(0x20, enc.ToByte(' '), enc.EncodingName);
    }
  }

  [Test]
  public void Amstrad_FullAsciiPrintableRange_IsIdentity() {
    // 0x20..0x7E are exact ASCII on the Amstrad CPC.
    for (var b = 0x20; b <= 0x7E; ++b)
      Assert.AreEqual((char)b, AmstradCpcEncoding.Instance.ToChar((byte)b), $"0x{b:X2}");
  }

  [Test]
  public void GraphicsBytes_UsePrivateUsePlaceholders() {
    // Atari control/graphics byte 0x00 and inverse byte 0x80 land in the PUA, not real glyphs.
    Assert.AreEqual((char)0xE000, AtasciiEncoding.Instance.ToChar(0x00));
    Assert.AreEqual((char)0xE080, AtasciiEncoding.Instance.ToChar(0x80));
    Assert.AreEqual((char)0xE100, AmstradCpcEncoding.Instance.ToChar(0x00));
  }
}
