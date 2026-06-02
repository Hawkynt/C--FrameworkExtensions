using NUnit.Framework;

namespace System;

[TestFixture]
public class BitBenchLegacyFloatTests {

  private static readonly double[] RoundTripValues = { 0d, 1d, 2d, 0.5d, -1d, -2d, 0.25d, 10d, 100d };

  // Relative tolerance helper: these formats have limited precision.
  private static double Tolerance(double v) => Math.Max(Math.Abs(v) * 1e-5, 1e-6);

  #region IbmFloat32

  [Test]
  public void IbmFloat32_One_HasReferenceEncoding() {
    Assert.That(IbmFloat32.FromSingle(1f).RawValue, Is.EqualTo(0x41100000u));
  }

  [Test]
  public void IbmFloat32_RoundTrip([ValueSource(nameof(RoundTripValues))] double v) {
    var f = (float)v;
    var actual = IbmFloat32.FromSingle(f).ToSingle();
    Assert.That(actual, Is.EqualTo(f).Within(Tolerance(v)));
  }

  [Test]
  public void IbmFloat32_Zero_IsZero() {
    Assert.That(IbmFloat32.Zero.ToSingle(), Is.EqualTo(0f));
    Assert.That(IbmFloat32.FromSingle(0f).ToSingle(), Is.EqualTo(0f));
  }

  [Test]
  public void IbmFloat32_SignHandling() {
    Assert.That(IbmFloat32.FromSingle(-3.5f).ToSingle(), Is.EqualTo(-3.5f).Within(Tolerance(3.5)));
    Assert.That(IbmFloat32.FromSingle(3.5f).ToSingle(), Is.EqualTo(3.5f).Within(Tolerance(3.5)));
    Assert.That(IbmFloat32.IsNegative(IbmFloat32.FromSingle(-3.5f)), Is.True);
    Assert.That(IbmFloat32.IsNegative(IbmFloat32.FromSingle(3.5f)), Is.False);
  }

  #endregion

  #region VaxFloat

  [Test]
  public void VaxFloat_One_HasReferenceEncoding() {
    var raw = VaxFloat.FromSingle(1f).RawValue;
    Assert.That((raw >> 7) & 0xFF, Is.EqualTo(129u)); // exponent 0x81
    Assert.That(((raw & 0x7F) << 16) | ((raw >> 16) & 0xFFFF), Is.EqualTo(0u)); // fraction 0
    Assert.That((raw >> 15) & 1, Is.EqualTo(0u)); // sign 0
  }

  [Test]
  public void VaxFloat_RoundTrip([ValueSource(nameof(RoundTripValues))] double v) {
    var f = (float)v;
    var actual = VaxFloat.FromSingle(f).ToSingle();
    Assert.That(actual, Is.EqualTo(f).Within(Tolerance(v)));
  }

  [Test]
  public void VaxFloat_Zero_IsZero() {
    Assert.That(VaxFloat.Zero.ToSingle(), Is.EqualTo(0f));
    Assert.That(VaxFloat.FromSingle(0f).ToSingle(), Is.EqualTo(0f));
  }

  [Test]
  public void VaxFloat_SignHandling() {
    Assert.That(VaxFloat.FromSingle(-3.5f).ToSingle(), Is.EqualTo(-3.5f).Within(Tolerance(3.5)));
    Assert.That(VaxFloat.IsNegative(VaxFloat.FromSingle(-3.5f)), Is.True);
    Assert.That(VaxFloat.IsNegative(VaxFloat.FromSingle(3.5f)), Is.False);
  }

  #endregion

  #region MBF32

  [Test]
  public void MBF32_One_HasReferenceEncoding() {
    Assert.That(MBF32.FromSingle(1f).RawValue, Is.EqualTo(0x81000000u));
  }

  [Test]
  public void MBF32_RoundTrip([ValueSource(nameof(RoundTripValues))] double v) {
    var f = (float)v;
    var actual = MBF32.FromSingle(f).ToSingle();
    Assert.That(actual, Is.EqualTo(f).Within(Tolerance(v)));
  }

  [Test]
  public void MBF32_Zero_IsZero() {
    Assert.That(MBF32.Zero.ToSingle(), Is.EqualTo(0f));
    Assert.That(MBF32.FromSingle(0f).ToSingle(), Is.EqualTo(0f));
  }

  [Test]
  public void MBF32_SignHandling() {
    Assert.That(MBF32.FromSingle(-3.5f).ToSingle(), Is.EqualTo(-3.5f).Within(Tolerance(3.5)));
    Assert.That(MBF32.IsNegative(MBF32.FromSingle(-3.5f)), Is.True);
    Assert.That(MBF32.IsNegative(MBF32.FromSingle(3.5f)), Is.False);
  }

  #endregion

  #region MBF64

  [Test]
  public void MBF64_One_HasReferenceEncoding() {
    Assert.That(MBF64.FromDouble(1.0).RawValue, Is.EqualTo(0x8100000000000000UL));
  }

  [Test]
  public void MBF64_RoundTrip([ValueSource(nameof(RoundTripValues))] double v) {
    var actual = MBF64.FromDouble(v).ToDouble();
    Assert.That(actual, Is.EqualTo(v).Within(Tolerance(v)));
  }

  [Test]
  public void MBF64_Zero_IsZero() {
    Assert.That(MBF64.Zero.ToDouble(), Is.EqualTo(0d));
    Assert.That(MBF64.FromDouble(0d).ToDouble(), Is.EqualTo(0d));
  }

  [Test]
  public void MBF64_SignHandling() {
    Assert.That(MBF64.FromDouble(-3.5).ToDouble(), Is.EqualTo(-3.5).Within(Tolerance(3.5)));
    Assert.That(MBF64.IsNegative(MBF64.FromDouble(-3.5)), Is.True);
    Assert.That(MBF64.IsNegative(MBF64.FromDouble(3.5)), Is.False);
  }

  [Test]
  public void MBF64_HasMorePrecisionThanMBF32() {
    // 55-bit fraction should represent values that MBF32's 23-bit fraction cannot.
    const double value = 1.0000000001;
    Assert.That(MBF64.FromDouble(value).ToDouble(), Is.EqualTo(value).Within(1e-9));
  }

  #endregion

}
