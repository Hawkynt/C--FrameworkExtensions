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
using Hawkynt.ColorProcessing.Quality;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("QualityMetrics")]
public class QualityMetricTests {

  private static Bitmap _Gradient(int w, int h) {
    var b = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    using var lk = b.Lock();
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
      var rv = (byte)(x * 255 / Math.Max(1, w - 1));
      var gv = (byte)(y * 255 / Math.Max(1, h - 1));
      lk[x, y] = Color.FromArgb(255, rv, gv, 128);
    }
    return b;
  }

  private static Bitmap _Solid(int w, int h, Color c) => TestUtilities.CreateSolidBitmap(w, h, c);

  // ----- PSNR -----

  [Test] public void Psnr_IdenticalImages_IsInfinite() {
    using var a = _Gradient(32, 32);
    using var b = (Bitmap)a.Clone();
    var p = Psnr.Compute(a, b);
    Assert.That(double.IsPositiveInfinity(p), Is.True);
  }

  [Test] public void Psnr_DifferentImages_IsFiniteAndPositive() {
    using var a = _Solid(32, 32, Color.Black);
    using var b = _Solid(32, 32, Color.White);
    var p = Psnr.Compute(a, b);
    Assert.That(double.IsInfinity(p), Is.False);
    // Black vs white: MSE = 255² → PSNR = 0 dB exactly.
    Assert.That(p, Is.EqualTo(0).Within(0.5));
  }

  [Test] public void Psnr_SmallDifference_IsHigh() {
    using var a = _Solid(32, 32, Color.FromArgb(120, 120, 120));
    using var b = _Solid(32, 32, Color.FromArgb(122, 120, 120));
    var p = Psnr.Compute(a, b);
    Assert.That(p, Is.GreaterThan(40)); // tiny noise → very high PSNR
  }

  // ----- SSIM -----

  [Test] public void Ssim_IdenticalImages_IsOne() {
    using var a = _Gradient(32, 32);
    using var b = (Bitmap)a.Clone();
    var s = Ssim.Compute(a, b);
    Assert.That(s, Is.EqualTo(1.0).Within(1e-6));
  }

  [Test] public void Ssim_BlackVsWhite_IsLow() {
    using var a = _Solid(32, 32, Color.Black);
    using var b = _Solid(32, 32, Color.White);
    var s = Ssim.Compute(a, b);
    Assert.That(s, Is.LessThan(0.05));
  }

  // ----- MS-SSIM -----

  [Test] public void MsSsim_IdenticalImages_IsOne() {
    using var a = _Gradient(64, 64);
    using var b = (Bitmap)a.Clone();
    var s = MsSsim.Compute(a, b);
    Assert.That(s, Is.EqualTo(1.0).Within(1e-6));
  }

  [Test] public void MsSsim_DifferentImages_BelowOne() {
    using var a = _Gradient(64, 64);
    using var b = _Solid(64, 64, Color.Gray);
    var s = MsSsim.Compute(a, b);
    Assert.That(s, Is.LessThan(0.99));
    Assert.That(s, Is.GreaterThanOrEqualTo(0));
  }

  // ----- ΔE-RMS -----

  [Test] public void DeltaERms_IdenticalImages_IsZero() {
    using var a = _Gradient(32, 32);
    using var b = (Bitmap)a.Clone();
    var d = DeltaERms.Compute(a, b);
    Assert.That(d, Is.EqualTo(0).Within(1e-3));
  }

  [Test] public void DeltaERms_DifferentImages_IsPositive() {
    using var a = _Solid(32, 32, Color.Red);
    using var b = _Solid(32, 32, Color.Blue);
    var d = DeltaERms.Compute(a, b);
    Assert.That(d, Is.GreaterThan(10)); // Red vs Blue is enormous in Lab.
  }

  // ----- FSIM -----

  [Test] public void Fsim_IdenticalImages_IsOne() {
    using var a = _Gradient(32, 32);
    using var b = (Bitmap)a.Clone();
    var f = Fsim.Compute(a, b);
    Assert.That(f, Is.EqualTo(1.0).Within(1e-3));
  }

  [Test] public void Fsim_DifferentImages_BelowOne() {
    using var a = _Gradient(32, 32);
    using var b = _Solid(32, 32, Color.Gray);
    var f = Fsim.Compute(a, b);
    Assert.That(f, Is.LessThan(1.0));
    Assert.That(f, Is.GreaterThan(0.0));
  }

  // ----- Mismatched sizes -----

  [Test] public void Psnr_MismatchedSizes_Throws() {
    using var a = _Solid(32, 32, Color.Black);
    using var b = _Solid(33, 33, Color.Black);
    Assert.Throws<ArgumentException>(() => Psnr.Compute(a, b));
  }
}
