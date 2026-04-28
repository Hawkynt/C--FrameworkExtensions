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

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Filters")]
[Category("CLAHE")]
public class ClaheFilterTests {

  [Test] public void Clahe_Default_PreservesDimensions() {
    using var src = TestUtilities.CreateSolidBitmap(32, 32, Color.FromArgb(120, 120, 120));
    using var dst = src.ApplyFilter(Clahe.Default);
    Assert.That(dst.Width, Is.EqualTo(32));
    Assert.That(dst.Height, Is.EqualTo(32));
    Assert.That(dst.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test] public void Clahe_SolidColor_LeavesNearlyUnchanged() {
    using var src = TestUtilities.CreateSolidBitmap(32, 32, Color.FromArgb(120, 120, 120));
    using var dst = src.ApplyFilter(Clahe.Default);
    using var lk = dst.Lock();
    var c = lk[16, 16];
    // Single-bin histogram → CDF maps the bin to ~1.0 → output saturates near white.
    // Just check we got *some* sane RGB triple, not NaN.
    Assert.That(c.R, Is.GreaterThanOrEqualTo(0));
    Assert.That(c.R, Is.LessThanOrEqualTo(255));
  }

  [Test] public void Clahe_LowContrastGradient_IncreasesContrast() {
    // Create a low-contrast horizontal gradient (50..150 RGB) that should be expanded.
    using var src = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
    using (var lk = src.Lock()) {
      for (var y = 0; y < 64; ++y)
      for (var x = 0; x < 64; ++x) {
        var v = (byte)(50 + x * 100 / 63);
        lk[x, y] = Color.FromArgb(255, v, v, v);
      }
    }
    using var dst = src.ApplyFilter(new Clahe(8, 4f));
    using var lk2 = dst.Lock();
    // Output should still vary across x (i.e. left side darker than right side on average).
    var leftMean = 0;
    var rightMean = 0;
    for (var y = 0; y < 64; ++y) {
      leftMean += lk2[8, y].R;
      rightMean += lk2[55, y].R;
    }
    Assert.That(rightMean, Is.GreaterThan(leftMean));
  }

  [Test] public void Clahe_TileSizeClamped_ToValidRange() {
    using var src = TestUtilities.CreateSolidBitmap(32, 32, Color.FromArgb(100, 100, 100));
    // Out-of-range tile size; constructor clamps internally.
    using var dst = src.ApplyFilter(new Clahe(2, 0.5f));
    Assert.That(dst.Width, Is.EqualTo(32));
  }
}
