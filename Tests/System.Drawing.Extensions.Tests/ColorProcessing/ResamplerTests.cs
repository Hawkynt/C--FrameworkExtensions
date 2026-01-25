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
using System.Drawing.Tests;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Resizing.Resamplers;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Resamplers")]
public class ResamplerTests {

  #region Basic Resamplers

  [Test]
  [Category("HappyPath")]
  public void NearestNeighbor_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<NearestNeighbor>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void NearestNeighbor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Blue);
    using var result = source.Resample<NearestNeighbor>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Blue.R));
    Assert.That(centerColor.G, Is.EqualTo(Color.Blue.G));
    Assert.That(centerColor.B, Is.EqualTo(Color.Blue.B));
  }

  [Test]
  [Category("HappyPath")]
  public void Bilinear_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Bilinear>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Bilinear_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Green);
    using var result = source.Resample<Bilinear>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(1));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(1));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Bicubic_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Bicubic>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Box_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Box>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Hermite_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Hermite>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Gaussian_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Gaussian>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Lanczos Family

  [Test]
  [Category("HappyPath")]
  public void Lanczos_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos2_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos2>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos3>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Purple);
    using var result = source.Resample<Lanczos3>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Purple.R).Within(2));
    Assert.That(centerColor.G, Is.EqualTo(Color.Purple.G).Within(2));
    Assert.That(centerColor.B, Is.EqualTo(Color.Purple.B).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos4_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos4>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos5_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos5>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Mitchell-Netravali Family

  [Test]
  [Category("HappyPath")]
  public void MitchellNetravali_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<MitchellNetravali>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void CatmullRom_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<CatmullRom>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Robidoux_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Robidoux>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void RobidouxSharp_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<RobidouxSharp>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void RobidouxSoft_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<RobidouxSoft>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Splines

  [Test]
  [Category("HappyPath")]
  public void Spline16_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Spline16>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Spline36_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Spline36>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Spline64_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Spline64>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Higher-Order B-Splines

  [Test]
  [Category("HappyPath")]
  public void BSpline2_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline2>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline4_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline4>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  [Category("KnownAnswer")]
  public void BSpline4_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Purple);
    using var result = source.Resample<BSpline4>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Purple.R).Within(3));
    Assert.That(centerColor.G, Is.EqualTo(Color.Purple.G).Within(3));
    Assert.That(centerColor.B, Is.EqualTo(Color.Purple.B).Within(3));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline4_HasPrefilter() {
    var resampler = new BSpline4();
    Assert.That(resampler.Prefilter, Is.Not.Null);
    Assert.That(resampler.Radius, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline5_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline5>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline7_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline7>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline9_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline9>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline11_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BSpline11>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Lagrange

  [Test]
  [Category("HappyPath")]
  public void Lagrange3_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lagrange3>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  [Category("KnownAnswer")]
  public void Lagrange3_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Cyan);
    using var result = source.Resample<Lagrange3>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Cyan.R).Within(3));
    Assert.That(centerColor.G, Is.EqualTo(Color.Cyan.G).Within(3));
    Assert.That(centerColor.B, Is.EqualTo(Color.Cyan.B).Within(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Lagrange3_HasNoPrefilter() {
    var resampler = new Lagrange3();
    Assert.That(resampler.Prefilter, Is.Null);
    Assert.That(resampler.Radius, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Lagrange5_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lagrange5>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  [Category("KnownAnswer")]
  public void Lagrange5_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Orange);
    using var result = source.Resample<Lagrange5>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Orange.R).Within(3));
    Assert.That(centerColor.G, Is.EqualTo(Color.Orange.G).Within(3));
    Assert.That(centerColor.B, Is.EqualTo(Color.Orange.B).Within(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Lagrange5_HasCorrectRadius() {
    var resampler = new Lagrange5();
    Assert.That(resampler.Prefilter, Is.Null);
    Assert.That(resampler.Radius, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Lagrange7_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lagrange7>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  [Category("KnownAnswer")]
  public void Lagrange7_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Magenta);
    using var result = source.Resample<Lagrange7>(16, 16);

    using var locker = result.Lock();
    var centerColor = locker[8, 8];

    Assert.That(centerColor.R, Is.EqualTo(Color.Magenta.R).Within(3));
    Assert.That(centerColor.G, Is.EqualTo(Color.Magenta.G).Within(3));
    Assert.That(centerColor.B, Is.EqualTo(Color.Magenta.B).Within(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Lagrange7_HasCorrectRadius() {
    var resampler = new Lagrange7();
    Assert.That(resampler.Prefilter, Is.Null);
    Assert.That(resampler.Radius, Is.EqualTo(4));
  }

  #endregion

  #region O-Moms

  [Test]
  [Category("HappyPath")]
  public void OMoms3_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<OMoms3>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void OMoms5_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<OMoms5>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void OMoms7_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<OMoms7>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Schaum

  [Test]
  [Category("HappyPath")]
  public void Schaum2_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Schaum2>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Schaum3_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Schaum3>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Jinc-based

  [Test]
  [Category("HappyPath")]
  public void Jinc_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Jinc>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void EwaLanczos_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<EwaLanczos>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Sinc Window Functions

  [Test]
  [Category("HappyPath")]
  public void Blackman_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Blackman>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Hann_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Hann>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Hamming_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Hamming>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Kaiser_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Kaiser>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Welch_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Welch>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Bartlett_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Bartlett>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Nuttal_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Nuttal>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BlackmanNuttal_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BlackmanNuttal>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BlackmanHarris_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BlackmanHarris>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void FlatTop_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<FlatTop>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Cosine_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Cosine>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void PowerOfCosine_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<PowerOfCosine>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Tukey_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Tukey>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Poisson_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Poisson>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void BartlettHann_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<BartlettHann>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void HanningPoisson_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<HanningPoisson>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Bohman_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Bohman>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Cauchy_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Cauchy>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Rectangular_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Rectangular>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Magic Kernel

  [Test]
  [Category("HappyPath")]
  public void MagicKernelSharp_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<MagicKernelSharp>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region NoHalo and LoHalo

  [Test]
  [Category("HappyPath")]
  public void NoHalo_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<NoHalo>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void LoHalo_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<LoHalo>(16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void NoHalo_Downscaling_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(16, 16, Color.Red);
    using var result = source.Resample<NoHalo>(8, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void LoHalo_Downscaling_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(16, 16, Color.Red);
    using var result = source.Resample<LoHalo>(8, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Lanczos3_1x1Input_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Red);
    using var result = source.Resample<Lanczos3>(4, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(4));
    Assert.That(result.Height, Is.EqualTo(4));
  }

  [Test]
  [Category("EdgeCase")]
  public void MitchellNetravali_1x1Input_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.Resample<MitchellNetravali>(4, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(4));
    Assert.That(result.Height, Is.EqualTo(4));
  }

  [Test]
  [Category("EdgeCase")]
  public void Bilinear_SameSize_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Green);
    using var result = source.Resample<Bilinear>(8, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Lanczos3_SameSize_PreservesContent() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Yellow);
    using var result = source.Resample<Lanczos3>(8, 8);

    using var locker = result.Lock();
    var centerColor = locker[4, 4];

    Assert.That(centerColor.R, Is.EqualTo(Color.Yellow.R).Within(2));
    Assert.That(centerColor.G, Is.EqualTo(Color.Yellow.G).Within(2));
    Assert.That(centerColor.B, Is.EqualTo(Color.Yellow.B).Within(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void Bicubic_ExtremeUpscaling_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(2, 2, Color.Cyan);
    using var result = source.Resample<Bicubic>(100, 100);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(100));
    Assert.That(result.Height, Is.EqualTo(100));
  }

  [Test]
  [Category("EdgeCase")]
  public void Lanczos3_Downscaling_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(16, 16, Color.Magenta);
    using var result = source.Resample<Lanczos3>(8, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void MitchellNetravali_NonSquare_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 4, Color.Orange);
    using var result = source.Resample<MitchellNetravali>(16, 8);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Spline36_NonSquareDownscale_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 10, Color.Coral);
    using var result = source.Resample<Spline36>(10, 5);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(5));
  }

  #endregion

  #region Parameterized Resampler Tests

  [Test]
  [Category("HappyPath")]
  public void Gaussian_CustomSigma_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new Gaussian(1.0f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Kaiser_CustomBeta_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new Kaiser(3, 12.0f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void MitchellNetravali_CustomBC_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new MitchellNetravali(0.5f, 0.25f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void PowerOfCosine_CustomAlpha_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new PowerOfCosine(3, 2.0f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Tukey_CustomAlpha_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new Tukey(3, 0.75f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Poisson_CustomDecay_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new Poisson(3, 30f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void Cauchy_CustomAlpha_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new Cauchy(3, 5.0f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  [Test]
  [Category("HappyPath")]
  public void HanningPoisson_CustomAlpha_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample(new HanningPoisson(3, 3.0f), 16, 16);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Output Format Tests

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Red);
    using var result = source.Resample<Lanczos3>(16, 16);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Bilinear_24bppInput_ProducesValidOutput() {
    using var source = TestUtilities.CreateSolidBitmap(8, 8, Color.Yellow, PixelFormat.Format24bppRgb);
    using var result = source.Resample<Bilinear>(16, 16);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
  }

  #endregion

  #region Alpha Channel Tests

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_TransparentPixels_HandlesAlpha() {
    using var source = new Bitmap(4, 4, PixelFormat.Format32bppArgb);
    using (var locker = source.Lock()) {
      locker.Clear(Color.Transparent);
      locker[1, 1] = Color.Red;
      locker[2, 1] = Color.Red;
      locker[1, 2] = Color.Red;
      locker[2, 2] = Color.Red;
    }

    using var result = source.Resample<Lanczos3>(8, 8);

    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void MitchellNetravali_SemiTransparent_HandlesAlpha() {
    using var source = new Bitmap(4, 4, PixelFormat.Format32bppArgb);
    using (var locker = source.Lock()) {
      var semiTransparent = Color.FromArgb(128, 255, 0, 0);
      locker.Clear(semiTransparent);
    }

    using var result = source.Resample<MitchellNetravali>(8, 8);

    Assert.That(result.Width, Is.EqualTo(8));
    Assert.That(result.Height, Is.EqualTo(8));
  }

  #endregion

  #region Property Tests

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_RadiusIsPositive() {
    var resampler = new Lanczos3();
    Assert.That(resampler.Radius, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void MitchellNetravali_RadiusIsPositive() {
    var resampler = new MitchellNetravali();
    Assert.That(resampler.Radius, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void BSpline_HasPrefilter() {
    var resampler = new BSpline();
    Assert.That(resampler.Prefilter, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_HasNoPrefilter() {
    var resampler = new Lanczos3();
    Assert.That(resampler.Prefilter, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void AllResamplers_ScalePropertyReturnsDefault() {
    var lanczos = new Lanczos3();
    var mitchell = new MitchellNetravali();
    var bilinear = new Bilinear();

    Assert.That(lanczos.Scale, Is.EqualTo(default(ScaleFactor)));
    Assert.That(mitchell.Scale, Is.EqualTo(default(ScaleFactor)));
    Assert.That(bilinear.Scale, Is.EqualTo(default(ScaleFactor)));
  }

  #endregion

  #region Test Pattern Tests

  [Test]
  [Category("HappyPath")]
  public void Lanczos3_TestPattern_ScalesCorrectly() {
    using var source = TestUtilities.CreateTestPattern(10, 10);
    using var result = source.Resample<Lanczos3>(20, 20);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));

    using var locker = result.Lock();
    var topLeft = locker[5, 5];
    var bottomRight = locker[15, 15];

    Assert.That(topLeft.R, Is.GreaterThan(180), "Top-left quadrant should be red");
    Assert.That(bottomRight.R, Is.GreaterThan(180), "Bottom-right quadrant should be yellow (high red)");
    Assert.That(bottomRight.G, Is.GreaterThan(180), "Bottom-right quadrant should be yellow (high green)");
  }

  [Test]
  [Category("HappyPath")]
  public void MitchellNetravali_Checkerboard_ProducesResult() {
    using var source = TestUtilities.CreateCheckerboard(8, 8, 2, Color.Black, Color.White);
    using var result = source.Resample<MitchellNetravali>(16, 16);

    Assert.That(result.Width, Is.EqualTo(16));
    Assert.That(result.Height, Is.EqualTo(16));
    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Bicubic_Gradient_ProducesResult() {
    using var source = TestUtilities.CreateGradientBitmap(10, 10, Color.Black, Color.White);
    using var result = source.Resample<Bicubic>(20, 20);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  #endregion

}
