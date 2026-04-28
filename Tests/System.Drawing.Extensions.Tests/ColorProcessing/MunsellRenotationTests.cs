#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Spot-checks the Munsell renotation table (ASTM D1535 / Newhall-Nickerson-Judd 1943) by
/// asserting canonical chips round-trip through MunsellF &lt;-&gt; XyzF with sub-&#916;E2000-1
/// accuracy.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Munsell")]
public class MunsellRenotationTests {

  // Canonical chips from the renotation table (illuminant C). xyY values straight from all.dat.
  // Format: {hueIndex, value, chroma, x, y, Y_renotation_/100} where hueIndex matches the
  // MunsellRenotationTable encoding (0 = 2.5R, 1 = 5R, 2 = 7.5R, 3 = 10R, 4 = 2.5YR, ...).
  // 5R 4/14: hueIdx=1, V=4, C=14, x=0.5734, y=0.3057, Y=0.12 (12.0/100).
  // 10YR 6/8: family 1 (YR), prefix=10 -> p=3, hueIdx = 4*1+3 = 7, V=6, C=8, x=0.457, y=0.4249, Y=0.3003.
  // 5G 5/10: family 4 (G), prefix=5 -> p=1, hueIdx = 4*4+1 = 17, V=5, C=10, x=0.2329, y=0.4331, Y=0.1977.
  // 5B 5/8: family 6 (B), prefix=5 -> p=1, hueIdx = 4*6+1 = 25, V=5, C=8, x=0.1958, y=0.2519, Y=0.1977.
  // 5Y 8/12: family 2 (Y), prefix=5, hueIdx = 4*2+1 = 9, V=8, C=12, x=0.4562, y=0.4788, Y=0.591.

  private static readonly object[] CanonicalChips = {
    new object[] { "5R 4/14",  1, 4f, 14f, 0.5734f, 0.3057f, 0.1200f },
    new object[] { "10YR 6/8", 7, 6f,  8f, 0.4570f, 0.4249f, 0.3003f },
    new object[] { "5G 5/10", 17, 5f, 10f, 0.2329f, 0.4331f, 0.1977f },
    new object[] { "5B 5/8",  25, 5f,  8f, 0.1958f, 0.2519f, 0.1977f },
    new object[] { "5Y 8/12",  9, 8f, 12f, 0.4562f, 0.4788f, 0.5910f },
  };

  [TestCaseSource(nameof(CanonicalChips))]
  public void CanonicalChip_ForwardMatchesRenotationTable(
    string label, int hueIdx, float v, float chroma,
    float expectedX, float expectedY, float expectedBigY) {
    // API H is offset by -1 from internal hue index, then divided by 40.
    var apiH = ((hueIdx - 1f) / 40f + 1f) % 1f;
    if (apiH < 0f) apiH += 1f;
    var m = new MunsellF(apiH, v / 10f, chroma / 30f);

    var xyzD65 = new MunsellFToXyzF().Project(m);

    // Adapt back to illuminant C for direct table comparison.
    var xc = ColorMatrices.Brad65ToC_XX * xyzD65.X + ColorMatrices.Brad65ToC_XY * xyzD65.Y + ColorMatrices.Brad65ToC_XZ * xyzD65.Z;
    var yc = ColorMatrices.Brad65ToC_YX * xyzD65.X + ColorMatrices.Brad65ToC_YY * xyzD65.Y + ColorMatrices.Brad65ToC_YZ * xyzD65.Z;
    var zc = ColorMatrices.Brad65ToC_ZX * xyzD65.X + ColorMatrices.Brad65ToC_ZY * xyzD65.Y + ColorMatrices.Brad65ToC_ZZ * xyzD65.Z;
    var sum = xc + yc + zc;
    var x = xc / sum;
    var y = yc / sum;

    Assert.That(x, Is.EqualTo(expectedX).Within(0.002f), $"{label}: x mismatch (out={x}, expected={expectedX})");
    Assert.That(y, Is.EqualTo(expectedY).Within(0.002f), $"{label}: y mismatch (out={y}, expected={expectedY})");
    Assert.That(yc, Is.EqualTo(expectedBigY).Within(0.005f), $"{label}: Y mismatch (out={yc}, expected={expectedBigY})");
  }

  [TestCaseSource(nameof(CanonicalChips))]
  public void CanonicalChip_RoundTripPreservesChip(
    string label, int hueIdx, float v, float chroma,
    float expectedX, float expectedY, float expectedBigY) {
    _ = expectedX; _ = expectedY; _ = expectedBigY; // unused on the round-trip path
    var apiH = ((hueIdx - 1f) / 40f + 1f) % 1f;
    if (apiH < 0f) apiH += 1f;
    var original = new MunsellF(apiH, v / 10f, chroma / 30f);

    var xyz = new MunsellFToXyzF().Project(original);
    var roundTripped = new XyzFToMunsellF().Project(xyz);

    var dh = HueDelta(roundTripped.H, original.H);
    Assert.That(dh, Is.LessThan(0.001f),  // 0.001 cyclic = 0.04 hue family step
      $"{label}: hue drift |Δ|={dh} (out H={roundTripped.H}, in H={original.H})");
    Assert.That(roundTripped.V, Is.EqualTo(original.V).Within(0.002f),  // 0.02 Munsell V steps
      $"{label}: value drift (out V={roundTripped.V}, in V={original.V})");
    Assert.That(roundTripped.C, Is.EqualTo(original.C).Within(0.003f),  // 0.09 chroma units
      $"{label}: chroma drift (out C={roundTripped.C}, in C={original.C})");
  }

  /// <summary>Smallest cyclic distance between two hue values in [0,1).</summary>
  private static float HueDelta(float a, float b) {
    var d = a - b;
    while (d > 0.5f) d -= 1f;
    while (d < -0.5f) d += 1f;
    return d < 0f ? -d : d;
  }

  /// <summary>
  /// Round-trips a sweep of representative sRGB colours through
  /// LinearRgbF -&gt; MunsellF -&gt; LinearRgbF and asserts the residual stays small
  /// in linear-RGB Euclidean distance. Catches gross failures in the inverse refinement.
  /// </summary>
  [Test]
  public void LinearRgbRoundTrip_StaysClose() {
    var probes = new[] {
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.5f, 0.5f, 0.5f),
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.8f, 0.2f, 0.1f),
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.2f, 0.6f, 0.3f),
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.1f, 0.3f, 0.7f),
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.7f, 0.7f, 0.2f),
      new Hawkynt.ColorProcessing.Working.LinearRgbF(0.4f, 0.4f, 0.4f),
    };

    foreach (var rgb in probes) {
      var m = new LinearRgbFToMunsellF().Project(rgb);
      var back = new MunsellFToLinearRgbF().Project(m);
      var dr = back.R - rgb.R;
      var dg = back.G - rgb.G;
      var db = back.B - rgb.B;
      var dist = MathF.Sqrt(dr * dr + dg * dg + db * db);
      Assert.That(dist, Is.LessThan(0.05f),
        $"RGB({rgb.R},{rgb.G},{rgb.B}) -> Munsell({m.H},{m.V},{m.C}) -> RGB({back.R},{back.G},{back.B}) drifted by {dist}");
    }
  }
}
