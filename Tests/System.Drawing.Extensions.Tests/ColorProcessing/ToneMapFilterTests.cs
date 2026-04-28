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
using Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Filters")]
[Category("ToneMap")]
public class ToneMapFilterTests {

  private static Bitmap MakeBitmap(int w, int h, Color color) => TestUtilities.CreateSolidBitmap(w, h, color);

  [Test] public void Reinhard_Default_PreservesDimensions() {
    using var src = MakeBitmap(16, 16, Color.FromArgb(200, 100, 50));
    using var dst = src.ApplyFilter(Reinhard.Default);
    Assert.That(dst.Width, Is.EqualTo(16));
    Assert.That(dst.Height, Is.EqualTo(16));
    Assert.That(dst.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test] public void Reinhard_Black_StaysBlack() {
    using var src = MakeBitmap(8, 8, Color.Black);
    using var dst = src.ApplyFilter(Reinhard.Default);
    using var lk = dst.Lock();
    var c = lk[4, 4];
    Assert.That(c.R, Is.EqualTo(0));
    Assert.That(c.G, Is.EqualTo(0));
    Assert.That(c.B, Is.EqualTo(0));
  }

  [Test] public void Reinhard_White_RemainsHighIntensity() {
    using var src = MakeBitmap(8, 8, Color.White);
    using var dst = src.ApplyFilter(Reinhard.Default);
    using var lk = dst.Lock();
    var c = lk[4, 4];
    // White luminance=1 maps to 0.5; per-pixel scale = 0.5/1 = 0.5 — so r/g/b ~127.
    Assert.That(c.R, Is.GreaterThan(80));
    Assert.That(c.R, Is.LessThan(200));
  }

  [Test] public void ReinhardExtended_Default_Builds() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(50, 50, 50));
    using var dst = src.ApplyFilter(ReinhardExtended.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
  }

  [Test] public void Aces_Default_PreservesDimensions() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(180, 90, 60));
    using var dst = src.ApplyFilter(Aces.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
  }

  [Test] public void Aces_Black_StaysBlack() {
    using var src = MakeBitmap(8, 8, Color.Black);
    using var dst = src.ApplyFilter(Aces.Default);
    using var lk = dst.Lock();
    var c = lk[4, 4];
    Assert.That(c.R, Is.LessThanOrEqualTo(2));
  }

  [Test] public void Hable_Default_PreservesDimensions() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(120, 120, 120));
    using var dst = src.ApplyFilter(Hable.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
  }

  [Test] public void Drago_Default_Builds() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(180, 90, 60));
    using var dst = src.ApplyFilter(Drago.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
    Assert.That(dst.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test] public void Mantiuk_Default_Builds() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(180, 90, 60));
    using var dst = src.ApplyFilter(Mantiuk.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
  }

  [Test] public void Lottes_Default_PreservesDimensions() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(180, 90, 60));
    using var dst = src.ApplyFilter(Lottes.Default);
    Assert.That(dst.Width, Is.EqualTo(8));
  }

  [Test] public void Reinhard_BrightInput_OutputClampsAtOrBelow255() {
    using var src = MakeBitmap(8, 8, Color.FromArgb(255, 255, 255));
    using var dst = src.ApplyFilter(new Reinhard(10f));
    using var lk = dst.Lock();
    var c = lk[4, 4];
    Assert.That(c.R, Is.LessThanOrEqualTo(255));
    Assert.That(c.G, Is.LessThanOrEqualTo(255));
    Assert.That(c.B, Is.LessThanOrEqualTo(255));
  }
}
