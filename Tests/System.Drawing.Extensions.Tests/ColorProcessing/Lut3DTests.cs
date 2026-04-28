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

using System.Drawing;
using Hawkynt.ColorProcessing.LookupTable;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("LookupTable")]
public class Lut3DTests {

  [Test] public void Identity_Trilinear_RoundTripsExactly() {
    var lut = Lut3D.Identity(17);
    var (r, g, b) = lut.Sample(0.5f, 0.25f, 0.75f, Lut3DInterpolation.Trilinear);
    Assert.That(r, Is.EqualTo(0.5f).Within(1e-3f));
    Assert.That(g, Is.EqualTo(0.25f).Within(1e-3f));
    Assert.That(b, Is.EqualTo(0.75f).Within(1e-3f));
  }

  [Test] public void Identity_Tetrahedral_RoundTripsExactly() {
    var lut = Lut3D.Identity(17);
    var (r, g, b) = lut.Sample(0.5f, 0.25f, 0.75f, Lut3DInterpolation.Tetrahedral);
    Assert.That(r, Is.EqualTo(0.5f).Within(1e-3f));
    Assert.That(g, Is.EqualTo(0.25f).Within(1e-3f));
    Assert.That(b, Is.EqualTo(0.75f).Within(1e-3f));
  }

  [Test] public void Identity_AtCorners_Exact() {
    var lut = Lut3D.Identity(5);
    var (r, g, b) = lut.Sample(1f, 0f, 1f);
    Assert.That(r, Is.EqualTo(1f).Within(1e-5f));
    Assert.That(g, Is.EqualTo(0f).Within(1e-5f));
    Assert.That(b, Is.EqualTo(1f).Within(1e-5f));
  }

  [Test] public void OutOfRangeInput_ClampsCleanly() {
    var lut = Lut3D.Identity(5);
    var (r, _, _) = lut.Sample(2f, -1f, 0.5f);
    Assert.That(r, Is.EqualTo(1f).Within(1e-5f));
  }

  [Test] public void ReadCubeFromString_Parses() {
    const string cube = """
                        # comment
                        TITLE "test"
                        LUT_3D_SIZE 2
                        DOMAIN_MIN 0.0 0.0 0.0
                        DOMAIN_MAX 1.0 1.0 1.0
                        0.0 0.0 0.0
                        1.0 0.0 0.0
                        0.0 1.0 0.0
                        1.0 1.0 0.0
                        0.0 0.0 1.0
                        1.0 0.0 1.0
                        0.0 1.0 1.0
                        1.0 1.0 1.0
                        """;
    var lut = Lut3DReader.ReadCubeFromString(cube);
    Assert.That(lut.Size, Is.EqualTo(2));
    var (r, g, b) = lut.Sample(0.5f, 0.5f, 0.5f, Lut3DInterpolation.Trilinear);
    Assert.That(r, Is.EqualTo(0.5f).Within(1e-3f));
    Assert.That(g, Is.EqualTo(0.5f).Within(1e-3f));
    Assert.That(b, Is.EqualTo(0.5f).Within(1e-3f));
  }

  [Test] public void Lut3DFilter_Identity_PreservesPixels() {
    using var src = TestUtilities.CreateSolidBitmap(8, 8, Color.FromArgb(120, 80, 40));
    using var dst = src.ApplyFilter(Lut3DFilter.Default);
    using var lk = dst.Lock();
    var c = lk[4, 4];
    Assert.That(c.R, Is.EqualTo(120).Within(2));
    Assert.That(c.G, Is.EqualTo(80).Within(2));
    Assert.That(c.B, Is.EqualTo(40).Within(2));
  }

  [Test] public void Lut3DFilter_InvertingLut_SwapsBlackAndWhite() {
    // Build a 2-corner LUT that flips R: maps 0→1 and 1→0 across.
    var data = new float[] {
      // (B=0,G=0,R=0)  → red=1, others copied
      1, 0, 0,
      // (B=0,G=0,R=1) → red=0
      0, 0, 0,
      // (B=0,G=1,R=0)
      1, 1, 0,
      // (B=0,G=1,R=1)
      0, 1, 0,
      // (B=1,G=0,R=0)
      1, 0, 1,
      // (B=1,G=0,R=1)
      0, 0, 1,
      // (B=1,G=1,R=0)
      1, 1, 1,
      // (B=1,G=1,R=1)
      0, 1, 1,
    };
    var lut = new Lut3D(2, data);
    var filter = new Lut3DFilter(lut);

    using var src = TestUtilities.CreateSolidBitmap(8, 8, Color.FromArgb(255, 255, 255));
    using var dst = src.ApplyFilter(filter);
    using var lk = dst.Lock();
    var c = lk[4, 4];
    Assert.That(c.R, Is.LessThan(20));   // flipped: 1 → 0
    Assert.That(c.G, Is.GreaterThan(200));
    Assert.That(c.B, Is.GreaterThan(200));
  }
}
