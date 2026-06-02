using NUnit.Framework;

namespace System;

[TestFixture]
public class BitBenchBatch1Tests {

  // ---- TF32 ----

  [Test]
  [TestCase(1f)]
  [TestCase(2f)]
  [TestCase(0.5f)]
  [TestCase(-3.25f)]
  [TestCase(100f)]
  [TestCase(0f)]
  public void TF32_ExactlyRepresentable_RoundTrips(float value)
    => Assert.AreEqual(value, TF32.FromSingle(value).ToSingle());

  [Test]
  public void TF32_LowMantissaBits_AreZero() {
    var t = TF32.FromSingle(1.0f / 3.0f);
    Assert.AreEqual(0u, t.RawValue & 0x1FFF, "low 13 mantissa bits must be zero");
  }

  [Test]
  public void TF32_PrecisionLoss_IsBoundedByHalfStep() {
    // TF32 keeps 10 mantissa bits; relative error must be <= 2^-11
    foreach (var v in new[] { 1.0f / 3.0f, 1.2345678f, 9876.5432f, (float)Math.PI }) {
      var decoded = TF32.FromSingle(v).ToSingle();
      Assert.That(Math.Abs(decoded - v), Is.LessThanOrEqualTo(Math.Abs(v) * (float)Math.Pow(2, -11)), $"{v}");
    }
  }

  [Test]
  public void TF32_SpecialValues() {
    Assert.IsTrue(TF32.IsNaN(TF32.FromSingle(float.NaN)));
    Assert.IsTrue(TF32.IsInfinity(TF32.FromSingle(float.PositiveInfinity)));
    Assert.AreEqual(float.PositiveInfinity, TF32.PositiveInfinity.ToSingle());
    Assert.AreEqual(float.NegativeInfinity, TF32.NegativeInfinity.ToSingle());
    Assert.AreEqual(-1f, (-TF32.One).ToSingle());
  }

  // ---- µ-law / A-law ----

  [Test]
  public void MuLaw_AllCodes_DecodedPcmIsStableUnderReEncode() {
    // matched companding pair: decoding is canonical, so encode(decode(code)) decodes to the same PCM.
    // (Note ±0 collapse: codes 0x7F and 0xFF both decode to 0, so raw codes are NOT always identical.)
    for (var code = 0; code < 256; ++code) {
      var pcm = MuLaw.FromRaw((byte)code).ToPcm16();
      Assert.AreEqual(pcm, MuLaw.FromPcm16(pcm).ToPcm16(), $"code 0x{code:X2}");
    }
  }

  [Test]
  public void ALaw_AllCodes_DecodedPcmIsStableUnderReEncode() {
    for (var code = 0; code < 256; ++code) {
      var pcm = ALaw.FromRaw((byte)code).ToPcm16();
      Assert.AreEqual(pcm, ALaw.FromPcm16(pcm).ToPcm16(), $"code 0x{code:X2}");
    }
  }

  [Test]
  public void Companding_PreservesSignAndIsMonotonicIshAroundZero() {
    Assert.That(MuLaw.FromPcm16(1000).ToPcm16(), Is.GreaterThan((short)0));
    Assert.That(MuLaw.FromPcm16(-1000).ToPcm16(), Is.LessThan((short)0));
    Assert.That(ALaw.FromPcm16(1000).ToPcm16(), Is.GreaterThan((short)0));
    Assert.That(ALaw.FromPcm16(-1000).ToPcm16(), Is.LessThan((short)0));
  }

  [Test]
  public void Companding_LargerInputDecodesLarger() {
    Assert.That(MuLaw.FromPcm16(8000).ToPcm16(), Is.GreaterThan(MuLaw.FromPcm16(1000).ToPcm16()));
    Assert.That(ALaw.FromPcm16(8000).ToPcm16(), Is.GreaterThan(ALaw.FromPcm16(1000).ToPcm16()));
  }

  // ---- MIDI ----

  [Test]
  [TestCase(69, "A4", 440.0)]
  [TestCase(60, "C4", 261.625565)]
  [TestCase(0, "C-1", 8.175799)]
  [TestCase(127, "G9", 12543.853951)]
  public void MidiNote_NameOctaveFrequency(int number, string expectedName, double expectedFreq) {
    var note = MidiNote.FromNumber(number);
    Assert.AreEqual(expectedName, note.ToString());
    Assert.That(note.Frequency, Is.EqualTo(expectedFreq).Within(0.001));
  }

  [Test]
  public void MidiNote_OutOfRange_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => MidiNote.FromNumber(128));
    Assert.Throws<ArgumentOutOfRangeException>(() => MidiNote.FromNumber(-1));
  }
}
