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
using System.Linq;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Filters")]
public class FilterTests {

  #region VonKries Tests

  [Test]
  [Category("HappyPath")]
  public void VonKries_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void VonKries_SolidColor_ProducesReasonableOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(VonKries.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(20));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(20));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void VonKries_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Blue);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void VonKries_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ApplyFilter(VonKries.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Threshold Tests

  [Test]
  [Category("HappyPath")]
  public void Threshold_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_SolidWhite_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_SolidBlack_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Black);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Threshold_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Gray);
    using var result = source.ApplyFilter(Threshold.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  #endregion

  #region Sharpen Tests

  [Test]
  [Category("HappyPath")]
  public void Sharpen_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Sharpen.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Sharpen_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.ApplyFilter(Sharpen.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Blue.R).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(Color.Blue.G).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(Color.Blue.B).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Sharpen_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Gold);
    using var result = source.ApplyFilter(Sharpen.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Blur Tests

  [Test]
  [Category("HappyPath")]
  public void Blur_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Blur.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Blur_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.ApplyFilter(Blur.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(Color.Green.R).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(Color.Green.G).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(Color.Green.B).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Blur_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Coral);
    using var result = source.ApplyFilter(Blur.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Grayscale Tests

  [Test]
  [Category("HappyPath")]
  public void Grayscale_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(Grayscale.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Grayscale_SolidWhite_StaysNearWhite() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.White);
    using var result = source.ApplyFilter(Grayscale.Default);

    using var locker = result.Lock();
    var centerColor = locker[5, 5];

    Assert.That(centerColor.R, Is.EqualTo(255).Within(5));
    Assert.That(centerColor.G, Is.EqualTo(255).Within(5));
    Assert.That(centerColor.B, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Grayscale_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Purple);
    using var result = source.ApplyFilter(Grayscale.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ChannelExtraction Tests

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_Default_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_Default_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void ChannelExtraction_1x1Bitmap_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Yellow);
    using var result = source.ApplyFilter(ChannelExtraction.Default);

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ChannelExtraction_GreenChannel_ProducesOutput() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Lime);
    using var result = source.ApplyFilter(new ChannelExtraction(ColorChannel.Green));

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  #endregion
}
