using NUnit.Framework;

namespace System;

/// <summary>
/// Spec-conformance of the MXFP4 / NVFP4 block scaling. The E2M1 elements and E8M0/E4M3 scales are already
/// verified bit-exact against ml_dtypes (see <see cref="AiFloatGoldenTests"/>); these tests check the
/// block-level scale-selection formulas (OCP Microscaling spec for MXFP4; NVIDIA recipe for NVFP4) and
/// exact reconstruction of on-grid blocks.
/// </summary>
[TestFixture]
public class MxFp4SpecTests {

  private static float Amax(float[] v) {
    var m = 0f;
    foreach (var x in v)
      m = Math.Max(m, Math.Abs(x));
    return m;
  }

  // ---- MXFP4: shared E8M0 scale = 2^(floor(log2(amax)) - emax(E2M1)), emax(E2M1) = 2 ----

  [Test]
  [TestCase(5.3f)]
  [TestCase(11.0f)]
  [TestCase(0.37f)]
  [TestCase(100.0f)]
  public void MXFP4_SharedScale_FollowsOcpFormula(float amax) {
    var values = new float[MXFP4.BlockSize];
    values[0] = amax;          // block maximum
    values[1] = -amax / 3;
    var mx = MXFP4.Encode(values);
    var expectedExponent = (int)Math.Floor(Math.Log(amax, 2)) - 2;
    Assert.AreEqual(expectedExponent, mx.GetScale(0).Exponent);
  }

  [Test]
  public void MXFP4_OnGridBlock_RoundTripsExactly() {
    // E2M1 grid scaled by 2^k stays on the grid, so the block is reconstructed exactly and the scale is 2^k.
    float[] grid = [0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 6f];
    var values = new float[MXFP4.BlockSize];
    for (var i = 0; i < values.Length; ++i)
      values[i] = grid[i % grid.Length] * 4f; // k = 2
    var mx = MXFP4.Encode(values);
    Assert.AreEqual(2, mx.GetScale(0).Exponent);
    Assert.AreEqual(values, mx.ToArray());
  }

  [Test]
  public void MXFP4_Reconstruction_OnGridIsExact_OffGridBounded() {
    float[] values = [0.1f, 0.9f, 1.3f, 2.7f, 3.9f, 5.5f, -0.4f, -2.2f];
    var padded = new float[MXFP4.BlockSize];
    Array.Copy(values, padded, values.Length);
    var mx = MXFP4.Encode(padded);
    var scale = mx.GetScale(0).ToSingle();
    var decoded = mx.ToArray();
    // E2M1 quantization error is at most half the local step; the largest step is 2 (4..6), so <= 1 * scale.
    for (var i = 0; i < padded.Length; ++i)
      Assert.That(Math.Abs(decoded[i] - padded[i]), Is.LessThanOrEqualTo(1.0f * scale + 1e-6f), $"index {i}");
  }

  // ---- NVFP4: per-tensor FP32 scale = amax / (6 * 448); per-block E4M3 = amaxBlock / (6 * tensorScale) ----

  [Test]
  public void NVFP4_Scales_FollowNvidiaRecipe() {
    var values = new float[NVFP4.BlockSize * 2];
    values[0] = 12.5f;
    values[1] = -3.2f;
    values[NVFP4.BlockSize] = 0.8f; // second block, smaller amax
    var nv = NVFP4.Encode(values);

    var amaxTensor = Amax(values);
    Assert.That(nv.TensorScale, Is.EqualTo(amaxTensor / (6f * 448f)).Within(amaxTensor * 1e-6f));

    // block 0 scale: E4M3 of amaxBlock0 / (6 * tensorScale)
    var amaxBlock0 = 12.5f;
    var expected0 = E4M3.FromSingle(amaxBlock0 / (6f * nv.TensorScale)).ToSingle();
    Assert.That(nv.GetScale(0).ToSingle(), Is.EqualTo(expected0).Within(expected0 * 1e-6f));
  }

  [Test]
  public void NVFP4_AllZero_DecodesZero() {
    var nv = NVFP4.Encode(new float[NVFP4.BlockSize]);
    foreach (var v in nv.ToArray())
      Assert.AreEqual(0f, v);
  }

  [Test]
  public void NVFP4_Reconstruction_IsBoundedByBlockMax() {
    var values = new float[NVFP4.BlockSize];
    for (var i = 0; i < values.Length; ++i)
      values[i] = (i - 8) * 1.7f;
    var nv = NVFP4.Encode(values);
    var decoded = nv.ToArray();
    var amax = Amax(values);
    // ~3 bits of precision: error stays within a generous fraction of the block maximum.
    for (var i = 0; i < values.Length; ++i)
      Assert.That(Math.Abs(decoded[i] - values[i]), Is.LessThanOrEqualTo(0.34f * amax), $"index {i}");
  }
}
