using System.Collections;
using NUnit.Framework;

namespace System;

[TestFixture]
public class Fp4Tests {

  // ---- E2M1 element ----

  [Test]
  [TestCase(0x0, 0f)]
  [TestCase(0x1, 0.5f)]
  [TestCase(0x2, 1f)]
  [TestCase(0x3, 1.5f)]
  [TestCase(0x4, 2f)]
  [TestCase(0x5, 3f)]
  [TestCase(0x6, 4f)]
  [TestCase(0x7, 6f)]
  [TestCase(0x9, -0.5f)]
  [TestCase(0xA, -1f)]
  [TestCase(0xF, -6f)]
  public void E2M1_ToSingle_DecodesCodes(int raw, float expected)
    => Assert.AreEqual(expected, E2M1.FromRaw((byte)raw).ToSingle());

  [Test]
  public void E2M1_AllRepresentableValues_RoundTrip() {
    float[] reps = [0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 6f];
    foreach (var v in reps) {
      Assert.AreEqual(v, E2M1.FromSingle(v).ToSingle(), $"+{v}");
      Assert.AreEqual(-v, E2M1.FromSingle(-v).ToSingle(), $"-{v}");
    }
  }

  [Test]
  [TestCase(0.25f, 0f)]   // tie -> even (0)
  [TestCase(0.75f, 1f)]   // tie -> even (1.0)
  [TestCase(1.25f, 1f)]   // tie -> even (1.0)
  [TestCase(2.5f, 2f)]    // tie -> even (2.0)
  [TestCase(5f, 4f)]      // tie -> even (4.0)
  [TestCase(0.4f, 0.5f)]
  [TestCase(0.1f, 0f)]
  [TestCase(2.9f, 3f)]
  [TestCase(10f, 6f)]     // saturate
  [TestCase(-10f, -6f)]   // saturate
  public void E2M1_FromSingle_RoundsAndSaturates(float input, float expected)
    => Assert.AreEqual(expected, E2M1.FromSingle(input).ToSingle());

  [Test]
  public void E2M1_NaN_MapsToZero() => Assert.AreEqual(0f, E2M1.FromSingle(float.NaN).ToSingle());

  [Test]
  public void E2M1_SignedZero_ComparesEqual() {
    Assert.AreEqual(E2M1.Zero, E2M1.NegativeZero);
    Assert.AreEqual(E2M1.Zero.GetHashCode(), E2M1.NegativeZero.GetHashCode());
    Assert.That(E2M1.Zero.CompareTo(E2M1.NegativeZero), Is.Zero);
  }

  [Test]
  public void E2M1Codec_InPackedBuffer_RoundTrips() {
    var buffer = new PackedBuffer<float, E2M1Codec, LsbFirst>(8);
    float[] reps = [0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 6f];
    buffer.EncodeFrom(reps);
    Assert.AreEqual(reps, buffer.ToArray());
  }

  // ---- E8M0 scale ----

  [Test]
  [TestCase(127, 1f)]
  [TestCase(128, 2f)]
  [TestCase(126, 0.5f)]
  [TestCase(130, 8f)]
  public void E8M0_ToSingle_IsPowerOfTwo(int raw, float expected)
    => Assert.AreEqual(expected, E8M0.FromRaw((byte)raw).ToSingle());

  [Test]
  public void E8M0_FromExponent_And_FromSingle() {
    Assert.AreEqual(1f, E8M0.FromExponent(0).ToSingle());
    Assert.AreEqual(16f, E8M0.FromExponent(4).ToSingle());
    Assert.AreEqual(4f, E8M0.FromSingle(4f).ToSingle());
    Assert.AreEqual(4f, E8M0.FromSingle(5f).ToSingle());   // nearest power of two (rounds to 4)
    Assert.IsTrue(E8M0.IsNaN(E8M0.FromSingle(float.NaN)));
  }

  // ---- MXFP4 ----

  [Test]
  public void MXFP4_RepresentableBlock_RoundTripsExactly() {
    // amax = 6 -> scale 2^0 = 1, so these land exactly on the E2M1 grid
    float[] values = [0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 6f];
    var mx = MXFP4.Encode(values);
    Assert.AreEqual(values, mx.ToArray());
  }

  [Test]
  public void MXFP4_PowerOfTwoScaledBlock_RoundTripsExactly() {
    // scaling the representable grid by 4 keeps it exact (scale becomes 2^2)
    float[] values = [0f, 2f, 4f, 6f, 8f, 12f, 16f, 24f];
    var mx = MXFP4.Encode(values);
    Assert.AreEqual(values, mx.ToArray());
    Assert.AreEqual(4f, mx.GetScale(0).ToSingle());
  }

  [Test]
  public void MXFP4_Shape_IsCorrect() {
    var values = new float[40];
    for (var i = 0; i < values.Length; ++i)
      values[i] = i;
    var mx = MXFP4.Encode(values);
    Assert.AreEqual(40, mx.Length);
    Assert.AreEqual(2, mx.BlockCount);                 // 40 / 32 -> 2 blocks
    Assert.AreEqual(20, mx.PackedData.Length);         // 40 * 4 bits / 8
  }

  [Test]
  public void MXFP4_AllZero_DecodesZero() {
    var mx = MXFP4.Encode(new float[32]);
    foreach (var v in mx.ToArray())
      Assert.AreEqual(0f, v);
  }

  [Test]
  public void MXFP4_FromPacked_RoundTrips() {
    float[] values = [0f, 2f, 4f, 6f, 8f, 12f, 16f, 24f];
    var mx = MXFP4.Encode(values);
    var rebuilt = MXFP4.FromPacked(mx.PackedData.ToArray(), mx.Scales.ToArray(), mx.Length);
    Assert.AreEqual(mx.ToArray(), rebuilt.ToArray());
  }

  // ---- NVFP4 ----

  [Test]
  public void NVFP4_Shape_IsCorrect() {
    var values = new float[20];
    for (var i = 0; i < values.Length; ++i)
      values[i] = i + 1;
    var nv = NVFP4.Encode(values);
    Assert.AreEqual(20, nv.Length);
    Assert.AreEqual(2, nv.BlockCount);                 // 20 / 16 -> 2 blocks
    Assert.AreEqual(10, nv.PackedData.Length);         // 20 * 4 bits / 8
    Assert.That(nv.TensorScale, Is.GreaterThan(0f));
  }

  [Test]
  public void NVFP4_AllZero_DecodesZero() {
    var nv = NVFP4.Encode(new float[16]);
    foreach (var v in nv.ToArray())
      Assert.AreEqual(0f, v);
  }

  [Test]
  public void NVFP4_RoundTrip_IsCloseToOriginal() {
    var values = new float[16];
    for (var i = 0; i < values.Length; ++i)
      values[i] = (i - 8) * 1.3f; // mix of signs and magnitudes
    var nv = NVFP4.Encode(values);
    var decoded = nv.ToArray();

    var amax = 0f;
    foreach (var v in values)
      amax = Math.Max(amax, Math.Abs(v));

    for (var i = 0; i < values.Length; ++i)
      // E2M1 has ~3 bits of precision; allow generous relative error against the block maximum
      Assert.That(Math.Abs(decoded[i] - values[i]), Is.LessThanOrEqualTo(0.34f * amax), $"index {i}");
  }
}
