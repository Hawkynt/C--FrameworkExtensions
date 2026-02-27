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
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("BitmapTransforms")]
public class BitmapTransformTests {

  #region FlipHorizontal Tests

  [Test]
  [Category("HappyPath")]
  public void FlipHorizontal_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.FlipHorizontal();

    using var locker = result.Lock();
    Assert.That(locker[5, 5].R, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipHorizontal_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 15, Color.Blue);
    using var result = source.FlipHorizontal();

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipHorizontal_KnownPixel_MirrorsCorrectly() {
    using var source = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(source)) {
      g.Clear(Color.Black);
    }

    using (var srcLock = source.Lock()) {
      srcLock[0, 0] = Color.Red;
    }

    using var result = source.FlipHorizontal();
    using var dstLock = result.Lock();
    Assert.That(dstLock[9, 0].R, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipHorizontal_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Gold);
    using var result = source.FlipHorizontal();

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  [Test]
  [Category("EdgeCase")]
  public void FlipHorizontal_1x1Bitmap_PreservesPixel() {
    using var source = TestUtilities.CreateSolidBitmap(1, 1, Color.Green);
    using var result = source.FlipHorizontal();

    Assert.That(result.Width, Is.EqualTo(1));
    Assert.That(result.Height, Is.EqualTo(1));
  }

  #endregion

  #region FlipVertical Tests

  [Test]
  [Category("HappyPath")]
  public void FlipVertical_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.FlipVertical();

    using var locker = result.Lock();
    Assert.That(locker[5, 5].B, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipVertical_OutputDimensionsAreSame() {
    using var source = TestUtilities.CreateSolidBitmap(20, 15, Color.Green);
    using var result = source.FlipVertical();

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipVertical_KnownPixel_MirrorsCorrectly() {
    using var source = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(source)) {
      g.Clear(Color.Black);
    }

    using (var srcLock = source.Lock()) {
      srcLock[0, 0] = Color.Green;
    }

    using var result = source.FlipVertical();
    using var dstLock = result.Lock();
    Assert.That(dstLock[0, 9].G, Is.EqualTo(128).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void FlipVertical_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Coral);
    using var result = source.FlipVertical();

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region MirrorAlongAxis Tests

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongAxis_HorizontalLine_ProducesFlip() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    using var result = source.MirrorAlongAxis(new PointF(0, 5), new PointF(10, 5));

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongAxis_VerticalLine_ProducesFlip() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.MirrorAlongAxis(new PointF(5, 0), new PointF(5, 10));

    Assert.That(result.Width, Is.EqualTo(10));
    Assert.That(result.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongAxis_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.MirrorAlongAxis(new PointF(0, 0), new PointF(10, 10));

    using var locker = result.Lock();
    var c = locker[5, 5];
    Assert.That(c.G, Is.EqualTo(128).Within(10));
  }

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongAxis_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    using var result = source.MirrorAlongAxis(new PointF(0, 5), new PointF(10, 5));

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region ZoomToPoint Tests

  [Test]
  [Category("HappyPath")]
  public void ZoomToPoint_Factor1_PreservesDimensions() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.ZoomToPoint(new PointF(10, 10), 1.0f);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomToPoint_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var result = source.ZoomToPoint(new PointF(10, 10), 2.0f);

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.B, Is.EqualTo(255).Within(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomToPoint_CenterZoom_PreservesCenterPixel() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(128, 64, 32));
    using var result = source.ZoomToPoint(new PointF(10, 10), 3.0f);

    using var locker = result.Lock();
    var c = locker[10, 10];
    Assert.That(c.R, Is.EqualTo(128).Within(10));
    Assert.That(c.G, Is.EqualTo(64).Within(10));
    Assert.That(c.B, Is.EqualTo(32).Within(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomToPoint_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Magenta);
    using var result = source.ZoomToPoint(new PointF(5, 5), 2.0f);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region Straighten Tests

  [Test]
  [Category("HappyPath")]
  public void Straighten_ZeroAngle_ReturnsClone() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.Straighten(0f);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Straighten_SmallAngle_ProducesSmallOutput() {
    using var source = TestUtilities.CreateSolidBitmap(40, 40, Color.Green);
    using var result = source.Straighten(5f);

    Assert.That(result.Width, Is.GreaterThan(0));
    Assert.That(result.Height, Is.GreaterThan(0));
    Assert.That(result.Width, Is.LessThanOrEqualTo(40));
    Assert.That(result.Height, Is.LessThanOrEqualTo(40));
  }

  [Test]
  [Category("HappyPath")]
  public void Straighten_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(40, 40, Color.Blue);
    using var result = source.Straighten(3f);

    using var locker = result.Lock();
    var c = locker[result.Width / 2, result.Height / 2];
    Assert.That(c.B, Is.EqualTo(255).Within(10));
  }

  #endregion

  #region Skew Tests

  [Test]
  [Category("HappyPath")]
  public void Skew_ZeroAngles_PreservesDimensions() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.Skew(0f, 0f);

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Skew_HorizontalShear_ProducesWiderOutput() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var result = source.Skew(15f, 0f);

    Assert.That(result.Width, Is.GreaterThan(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Skew_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Green);
    using var result = source.Skew(10f, 5f);

    using var locker = result.Lock();
    var c = locker[result.Width / 2, result.Height / 2];
    Assert.That(c.G, Is.EqualTo(128).Within(30));
  }

  [Test]
  [Category("HappyPath")]
  public void Skew_OutputIsFormat32bppArgb() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var result = source.Skew(5f, 5f);

    Assert.That(result.PixelFormat, Is.EqualTo(PixelFormat.Format32bppArgb));
  }

  #endregion

  #region AutoRotate Tests

  [Test]
  [Category("HappyPath")]
  public void AutoRotate_NoExif_ReturnsSameDimensions() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = source.AutoRotate();

    Assert.That(result.Width, Is.EqualTo(20));
    Assert.That(result.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoRotate_SolidColor_PreservesColor() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    using var result = source.AutoRotate();

    using var locker = result.Lock();
    Assert.That(locker[5, 5].B, Is.EqualTo(255).Within(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoRotate_OutputIsValid() {
    using var source = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    using var result = source.AutoRotate();

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.GreaterThan(0));
    Assert.That(result.Height, Is.GreaterThan(0));
  }

  #endregion
}
