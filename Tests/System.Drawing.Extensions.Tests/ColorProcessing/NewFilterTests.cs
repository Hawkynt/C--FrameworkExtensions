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
using System.Drawing.Imaging;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Unit tests for the net-new fixed-default filters (DoG, Frei-Chen, Marr-Hildreth,
/// guided filter, Laplacian sharpening, Fisheye). One or two tests per filter, mirroring
/// the structure of <c>ToneMapFilterTests</c>.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Filters")]
public class NewFilterTests {

  private static Bitmap MakeBitmap(int w, int h, Color color) => TestUtilities.CreateSolidBitmap(w, h, color);

  // -- DifferenceOfGaussians --

  [Test] public void DifferenceOfGaussians_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(120, 120, 120));
    using var dst = src.ApplyFilter(DifferenceOfGaussians.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
    Assert.That(dst.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test] public void DifferenceOfGaussians_FlatField_StaysGrey() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(120, 120, 120));
    using var dst = src.ApplyFilter(DifferenceOfGaussians.Default);
    using var lk = dst.Lock();
    var c = lk[8, 8];
    // Flat input → DoG response = 0 → centred output ≈ 0.5 grey.
    Assert.That(c.R, Is.InRange(110, 145));
    Assert.That(c.R, Is.EqualTo(c.G));
    Assert.That(c.G, Is.EqualTo(c.B));
  }

  // -- FreiChenEdge --

  [Test] public void FreiChenEdge_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(80, 80, 80));
    using var dst = src.ApplyFilter(FreiChenEdge.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
  }

  [Test] public void FreiChenEdge_FlatField_NoEdges() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(80, 80, 80));
    using var dst = src.ApplyFilter(FreiChenEdge.Default);
    using var lk = dst.Lock();
    var c = lk[8, 8];
    // Constant patch → edge energy = 0 → output near black.
    Assert.That(c.R, Is.LessThan(20));
    Assert.That(c.G, Is.LessThan(20));
    Assert.That(c.B, Is.LessThan(20));
  }

  // -- MarrHildrethEdge --

  [Test] public void MarrHildrethEdge_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(40, 40, 40));
    using var dst = src.ApplyFilter(MarrHildrethEdge.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
  }

  [Test] public void MarrHildrethEdge_FlatField_NoCrossings() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(40, 40, 40));
    using var dst = src.ApplyFilter(MarrHildrethEdge.Default);
    using var lk = dst.Lock();
    var c = lk[8, 8];
    // No zero-crossings on a flat patch → output black.
    Assert.That(c.R, Is.LessThan(10));
  }

  // -- GuidedFilter --

  [Test] public void GuidedFilter_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(160, 80, 40));
    using var dst = src.ApplyFilter(GuidedFilter.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
  }

  [Test] public void GuidedFilter_FlatField_PreservesColor() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(160, 80, 40));
    using var dst = src.ApplyFilter(GuidedFilter.Default);
    using var lk = dst.Lock();
    var c = lk[8, 8];
    // Constant input → linear model degenerates to the mean → output = input.
    Assert.That(c.R, Is.InRange(155, 165));
    Assert.That(c.G, Is.InRange(75, 85));
    Assert.That(c.B, Is.InRange(35, 45));
  }

  // -- LaplacianSharpen --

  [Test] public void LaplacianSharpen_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(128, 128, 128));
    using var dst = src.ApplyFilter(LaplacianSharpen.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
  }

  [Test] public void LaplacianSharpen_FlatField_Idempotent() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(128, 128, 128));
    using var dst = src.ApplyFilter(LaplacianSharpen.Default);
    using var lk = dst.Lock();
    var c = lk[8, 8];
    // Laplacian of a constant = 0 → output equals input.
    Assert.That(c.R, Is.InRange(125, 131));
    Assert.That(c.G, Is.InRange(125, 131));
    Assert.That(c.B, Is.InRange(125, 131));
  }

  // -- Fisheye --

  [Test] public void Fisheye_Default_PreservesDimensions() {
    using var src = MakeBitmap(32, 32, Color.FromArgb(64, 192, 96));
    using var dst = src.ApplyFilter(Fisheye.Default);
    Assert.That(dst.Width, Is.EqualTo(32));
    Assert.That(dst.Height, Is.EqualTo(32));
  }

  [Test] public void Fisheye_FlatField_StaysFlat() {
    using var src = MakeBitmap(32, 32, Color.FromArgb(64, 192, 96));
    using var dst = src.ApplyFilter(Fisheye.Default);
    using var lk = dst.Lock();
    // Distortion on a flat field → still a flat field at the centre.
    var c = lk[16, 16];
    Assert.That(c.R, Is.InRange(60, 70));
    Assert.That(c.G, Is.InRange(185, 200));
    Assert.That(c.B, Is.InRange(90, 102));
  }
}
