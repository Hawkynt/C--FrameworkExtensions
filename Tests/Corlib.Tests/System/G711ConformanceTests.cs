using NUnit.Framework;

namespace System;

/// <summary>
/// Bit-exact conformance of <see cref="MuLaw"/>/<see cref="ALaw"/> against the ITU-T G.191 STL reference
/// algorithm (ulaw_expand / ulaw_compress / alaw_expand / alaw_compress), ported verbatim below as an
/// independent oracle. Source: https://github.com/openitu/STL (src/g711/g711.c).
/// </summary>
[TestFixture]
public class G711ConformanceTests {

  // ---- ITU-T G.191 reference oracle (independent of the implementation under test) ----

  private static short ItuUlawExpand(byte code) {
    int logbuf = code;
    var sign = logbuf < 0x80 ? -1 : 1;
    var mantissa = ~logbuf;
    var exponent = (mantissa >> 4) & 0x07;
    var segment = exponent + 1;
    mantissa &= 0x0F;
    var step = 4 << segment;
    return (short)(sign * ((0x80 << exponent) + step * mantissa + step / 2 - 4 * 33));
  }

  private static byte ItuUlawCompress(short lin) {
    var absno = lin < 0 ? ((~lin) >> 2) + 33 : (lin >> 2) + 33;
    if (absno > 0x1FFF)
      absno = 0x1FFF;

    var i = absno >> 6;
    var segno = 1;
    while (i != 0) {
      ++segno;
      i >>= 1;
    }

    var high = 8 - segno;
    var low = 0x0F - ((absno >> segno) & 0x0F);
    var code = (high << 4) | low;
    if (lin >= 0)
      code |= 0x80;
    return (byte)code;
  }

  private static short ItuAlawExpand(byte code) {
    var ix = code ^ 0x55;
    ix &= 0x7F;
    var iexp = ix >> 4;
    var mant = ix & 0x0F;
    if (iexp > 0)
      mant += 16;
    mant = (mant << 4) + 0x08;
    if (iexp > 1)
      mant <<= iexp - 1;
    return (short)(code > 127 ? mant : -mant);
  }

  private static byte ItuAlawCompress(short lin) {
    var ix = lin < 0 ? (~lin) >> 4 : lin >> 4;
    if (ix > 15) {
      var iexp = 1;
      while (ix > 16 + 15) {
        ix >>= 1;
        ++iexp;
      }

      ix -= 16;
      ix += iexp << 4;
    }

    if (lin >= 0)
      ix |= 0x80;
    return (byte)(ix ^ 0x55);
  }

  // ---- decode: must equal the ITU reference for every one of the 256 codes ----

  [Test]
  public void MuLaw_Decode_MatchesItuReference_AllCodes() {
    for (var code = 0; code < 256; ++code)
      Assert.AreEqual(ItuUlawExpand((byte)code), MuLaw.FromRaw((byte)code).ToPcm16(), $"code 0x{code:X2}");
  }

  [Test]
  public void ALaw_Decode_MatchesItuReference_AllCodes() {
    for (var code = 0; code < 256; ++code)
      Assert.AreEqual(ItuAlawExpand((byte)code), ALaw.FromRaw((byte)code).ToPcm16(), $"code 0x{code:X2}");
  }

  // ---- encode: our code and the ITU code must round-trip to the same PCM for every 16-bit input
  //      (compared via decode to be robust to the harmless +0/-0 code aliasing) ----

  [Test]
  public void MuLaw_ItuConvention_Encode_MatchesItuReference_AllSamples() {
    for (var s = short.MinValue; s < short.MaxValue; ++s)
      Assert.AreEqual(ItuUlawCompress((short)s), MuLaw.FromPcm16<ItuG711>((short)s).RawValue, $"sample {s}");
  }

  [Test]
  public void ALaw_ItuConvention_Encode_MatchesItuReference_AllSamples() {
    for (var s = short.MinValue; s < short.MaxValue; ++s)
      Assert.AreEqual(ItuAlawCompress((short)s), ALaw.FromPcm16<ItuG711>((short)s).RawValue, $"sample {s}");
  }

  [Test]
  public void MuLaw_SunAndItuConventions_DifferNearFullScale() {
    // Documented difference: the Sun (Reese-Campbell) quantizer clips earlier than the ITU quantizer.
    var sun = MuLaw.FromPcm16<SunG711>(-31612).ToPcm16();
    var itu = MuLaw.FromPcm16<ItuG711>(-31612).ToPcm16();
    Assert.AreEqual(-32124, sun);
    Assert.AreEqual(-31100, itu);
    Assert.AreNotEqual(sun, itu);
  }
}
