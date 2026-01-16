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

using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("RoundTrip")]
public class ColorSpaceRoundTripTests {

  #region Bgra8888 Tests

  [TestCase(0, 0, 0, 0)]
  [TestCase(255, 255, 255, 255)]
  [TestCase(128, 128, 128, 128)]
  [TestCase(1, 127, 254, 128)]
  [TestCase(0, 255, 0, 255)]
  [TestCase(255, 0, 255, 0)]
  public void Bgra8888_RoundTrip_PreservesColor(byte r, byte g, byte b, byte a) {
    var original = new Bgra8888(r, g, b, a);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = Bgra8888.FromNormalized(c1, c2, c3, alpha);
    Assert.That(restored, Is.EqualTo(original));
  }

  [Test]
  public void Bgra8888_RoundTrip_AllPrimaryColors() {
    var colors = new[] {
      new Bgra8888(255, 0, 0, 255),   // Red
      new Bgra8888(0, 255, 0, 255),   // Green
      new Bgra8888(0, 0, 255, 255),   // Blue
      new Bgra8888(0, 0, 0, 255),     // Black
      new Bgra8888(255, 255, 255, 255), // White
      new Bgra8888(0, 0, 0, 0),       // Transparent
    };

    foreach (var original in colors) {
      var (c1, c2, c3, alpha) = original.ToNormalized();
      var restored = Bgra8888.FromNormalized(c1, c2, c3, alpha);
      Assert.That(restored, Is.EqualTo(original), $"Failed for R={original.R}, G={original.G}, B={original.B}, A={original.A}");
    }
  }

  #endregion

  #region Rgba64 Tests

  [TestCase(0, 0, 0, 0)]
  [TestCase(255, 255, 255, 255)]
  [TestCase(128, 128, 128, 128)]
  [TestCase(1, 127, 254, 128)]
  public void Rgba64_RoundTrip_PreservesColor(byte r, byte g, byte b, byte a) {
    var original = new Rgba64(r, g, b, a);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = Rgba64.FromNormalized(c1, c2, c3, alpha);

    // Compare byte values since Rgba64 constructor scales bytes to ushort
    Assert.That(restored.R, Is.EqualTo(original.R), "Red channel mismatch");
    Assert.That(restored.G, Is.EqualTo(original.G), "Green channel mismatch");
    Assert.That(restored.B, Is.EqualTo(original.B), "Blue channel mismatch");
    Assert.That(restored.A, Is.EqualTo(original.A), "Alpha channel mismatch");
  }

  #endregion

  #region Argb4444 Tests (Lossy - 4-bit precision)

  [TestCase(0, 0, 0, 0)]
  [TestCase(255, 255, 255, 255)]
  [TestCase(128, 128, 128, 128)]
  public void Argb4444_RoundTrip_WithinTolerance(byte r, byte g, byte b, byte a) {
    var original = new Argb4444(r, g, b, a);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = Argb4444.FromNormalized(c1, c2, c3, alpha);

    // 4-bit precision means each component has only 16 levels (0-15)
    // Tolerance of 17 per component (255/15 = 17)
    const int tolerance = 17;
    Assert.That(restored.R, Is.EqualTo(original.R).Within(tolerance), "R mismatch");
    Assert.That(restored.G, Is.EqualTo(original.G).Within(tolerance), "G mismatch");
    Assert.That(restored.B, Is.EqualTo(original.B).Within(tolerance), "B mismatch");
    Assert.That(restored.A, Is.EqualTo(original.A).Within(tolerance), "Alpha mismatch");
  }

  [Test]
  public void Argb4444_SelfRoundTrip_ExactMatch() {
    // If we create from 4-bit values and round-trip, it should be exact
    var original = new Argb4444((ushort)0x8844);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = Argb4444.FromNormalized(c1, c2, c3, alpha);
    Assert.That(restored.Packed, Is.EqualTo(original.Packed));
  }

  #endregion

  #region Argb1555 Tests (Lossy - 5-bit RGB, 1-bit alpha)

  [TestCase(0, 0, 0, 0)]
  [TestCase(255, 255, 255, 255)]
  [TestCase(128, 128, 128, 255)]
  public void Argb1555_RoundTrip_WithinTolerance(byte r, byte g, byte b, byte a) {
    var original = new Argb1555(r, g, b, a);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = Argb1555.FromNormalized(c1, c2, c3, alpha);

    // 5-bit precision means each component has only 32 levels (0-31)
    // Tolerance of 9 per RGB component (255/31 ≈ 8.2)
    const int rgbTolerance = 9;
    Assert.That(restored.R, Is.EqualTo(original.R).Within(rgbTolerance), "R mismatch");
    Assert.That(restored.G, Is.EqualTo(original.G).Within(rgbTolerance), "G mismatch");
    Assert.That(restored.B, Is.EqualTo(original.B).Within(rgbTolerance), "B mismatch");

    // Alpha is 1-bit: either 0 or 255
    var expectedAlpha = original.A >= 128 ? 255 : 0;
    Assert.That(restored.A, Is.EqualTo(expectedAlpha), "Alpha should be binary (0 or 255)");
  }

  [Test]
  public void Argb1555_BinaryAlpha_PreservedCorrectly() {
    // Test that alpha threshold is correctly applied
    var transparent = new Argb1555(128, 128, 128, 0);
    var opaque = new Argb1555(128, 128, 128, 255);

    var (_, _, _, alphaT) = transparent.ToNormalized();
    var (_, _, _, alphaO) = opaque.ToNormalized();

    Assert.That(alphaT.ToByte(), Is.EqualTo(0), "Transparent should have alpha 0");
    Assert.That(alphaO.ToByte(), Is.EqualTo(255), "Opaque should have alpha 255");
  }

  #endregion

  #region LinearRgbaF Tests (Float precision)

  private const float FloatTolerance = 1e-6f;

  [TestCase(0f, 0f, 0f, 0f)]
  [TestCase(1f, 1f, 1f, 1f)]
  [TestCase(0.5f, 0.5f, 0.5f, 0.5f)]
  [TestCase(0.25f, 0.75f, 0.1f, 0.9f)]
  public void LinearRgbaF_RoundTrip_PreservesColor(float r, float g, float b, float a) {
    var original = new LinearRgbaF(r, g, b, a);
    var (c1, c2, c3, alpha) = original.ToNormalized();
    var restored = LinearRgbaF.FromNormalized(c1, c2, c3, alpha);

    Assert.That(restored.R, Is.EqualTo(original.R).Within(FloatTolerance), "Red mismatch");
    Assert.That(restored.G, Is.EqualTo(original.G).Within(FloatTolerance), "Green mismatch");
    Assert.That(restored.B, Is.EqualTo(original.B).Within(FloatTolerance), "Blue mismatch");
    Assert.That(restored.A, Is.EqualTo(original.A).Within(FloatTolerance), "Alpha mismatch");
  }

  [Test]
  public void LinearRgbaF_RoundTrip_PrimaryColors() {
    var colors = new[] {
      new LinearRgbaF(1f, 0f, 0f, 1f),   // Red
      new LinearRgbaF(0f, 1f, 0f, 1f),   // Green
      new LinearRgbaF(0f, 0f, 1f, 1f),   // Blue
      new LinearRgbaF(0f, 0f, 0f, 1f),   // Black
      new LinearRgbaF(1f, 1f, 1f, 1f),   // White
      new LinearRgbaF(0f, 0f, 0f, 0f),   // Transparent
    };

    foreach (var original in colors) {
      var (c1, c2, c3, alpha) = original.ToNormalized();
      var restored = LinearRgbaF.FromNormalized(c1, c2, c3, alpha);

      Assert.That(restored.R, Is.EqualTo(original.R).Within(FloatTolerance), $"Red mismatch for {original}");
      Assert.That(restored.G, Is.EqualTo(original.G).Within(FloatTolerance), $"Green mismatch for {original}");
      Assert.That(restored.B, Is.EqualTo(original.B).Within(FloatTolerance), $"Blue mismatch for {original}");
      Assert.That(restored.A, Is.EqualTo(original.A).Within(FloatTolerance), $"Alpha mismatch for {original}");
    }
  }

  #endregion

  #region OklabaF Tests (Float precision with shifted a/b)

  [TestCase(0f, 0f, 0f, 0f)]
  [TestCase(1f, 0f, 0f, 1f)]
  [TestCase(0.5f, 0.2f, -0.2f, 0.5f)]
  [TestCase(0.75f, -0.3f, 0.3f, 1f)]
  public void OklabaF_RoundTrip_PreservesColor(float l, float a, float b, float alpha) {
    var original = new OklabaF(l, a, b, alpha);
    var (c1, c2, c3, alphaResult) = original.ToNormalized();
    var restored = OklabaF.FromNormalized(c1, c2, c3, alphaResult);

    Assert.That(restored.L, Is.EqualTo(original.L).Within(FloatTolerance), "L mismatch");
    Assert.That(restored.A, Is.EqualTo(original.A).Within(FloatTolerance), "a mismatch");
    Assert.That(restored.B, Is.EqualTo(original.B).Within(FloatTolerance), "b mismatch");
    Assert.That(restored.Alpha, Is.EqualTo(original.Alpha).Within(FloatTolerance), "Alpha mismatch");
  }

  [Test]
  public void OklabaF_RoundTrip_NeutralGrays() {
    // In OkLab, neutral grays have a=0, b=0
    var grays = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };

    foreach (var lightness in grays) {
      var original = new OklabaF(lightness, 0f, 0f, 1f);
      var (c1, c2, c3, alpha) = original.ToNormalized();
      var restored = OklabaF.FromNormalized(c1, c2, c3, alpha);

      Assert.That(restored.L, Is.EqualTo(original.L).Within(FloatTolerance), $"L mismatch for gray {lightness}");
      Assert.That(restored.A, Is.EqualTo(0f).Within(FloatTolerance), $"a should be 0 for gray {lightness}");
      Assert.That(restored.B, Is.EqualTo(0f).Within(FloatTolerance), $"b should be 0 for gray {lightness}");
    }
  }

  #endregion

}
