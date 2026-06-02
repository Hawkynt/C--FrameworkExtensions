using NUnit.Framework;

namespace System;

/// <summary>
/// Bit-exact anchors for the legacy float formats, taken from authoritative published encodings:
/// IBM hexadecimal floating-point (en.wikipedia.org/wiki/IBM_hexadecimal_floating-point, MATLAB ibm2ieee),
/// VAX F_floating (USGS libvaxdata / VAX ISA manual), and Microsoft Binary Format
/// (en.wikipedia.org/wiki/Microsoft_Binary_Format).
/// </summary>
[TestFixture]
public class BitBenchLegacyFloatGoldenTests {

  // ---- IBM hexadecimal floating-point (single) ----

  [Test]
  [TestCase(1.0f, 0x41100000u)]   // 0.1₁₆ × 16¹
  [TestCase(0.5f, 0x40800000u)]   // 0.8₁₆ × 16⁰
  [TestCase(-118.625f, 0xC276A000u)]
  public void IbmFloat32_Golden(float value, uint expectedRaw) {
    Assert.AreEqual(expectedRaw, IbmFloat32.FromSingle(value).RawValue, "encode");
    Assert.AreEqual(value, IbmFloat32.FromRaw(expectedRaw).ToSingle(), 1e-4f, "decode");
  }

  // ---- VAX F_floating ----

  [Test]
  public void VaxFloat_One_Golden() {
    // 1.0 = 0.5 × 2¹ -> biased exponent 129 (0x81) at bits 7..14, zero fraction.
    Assert.AreEqual(0x4080u, VaxFloat.FromSingle(1.0f).RawValue);
    Assert.AreEqual(1.0f, VaxFloat.FromRaw(0x4080u).ToSingle(), 1e-6f);
  }

  // ---- Microsoft Binary Format ----

  [Test]
  [TestCase(1.0f, 0x81000000u)]   // exponent byte 0x81 (129), mantissa 0
  [TestCase(0.5f, 0x80000000u)]   // exponent byte 0x80 (128), mantissa 0
  public void MBF32_Golden(float value, uint expectedRaw) {
    Assert.AreEqual(expectedRaw, MBF32.FromSingle(value).RawValue, "encode");
    Assert.AreEqual(value, MBF32.FromRaw(expectedRaw).ToSingle(), 1e-6f, "decode");
  }

  [Test]
  public void MBF64_One_Golden() {
    Assert.AreEqual(0x8100000000000000ul, MBF64.FromDouble(1.0).RawValue);
    Assert.AreEqual(1.0, MBF64.FromRaw(0x8100000000000000ul).ToDouble(), 1e-12);
  }
}
