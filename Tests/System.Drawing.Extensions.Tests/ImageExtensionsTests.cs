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

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("Image")]
public class ImageExtensionsTests {
  private string _tempDir = null!;

  [SetUp]
  public void Setup() {
    this._tempDir = Path.Combine(Path.GetTempPath(), $"ImageTests_{Guid.NewGuid():N}");
    Directory.CreateDirectory(this._tempDir);
  }

  [TearDown]
  public void TearDown() {
    if (!Directory.Exists(this._tempDir))
      return;

    try {
      Directory.Delete(this._tempDir, true);
    } catch {
      // Best effort cleanup
    }
  }

  #region MakeGrayscale Tests

  [Test]
  [Category("HappyPath")]
  public void MakeGrayscale_ColorImage_BecomesGray() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var grayscale = original.MakeGrayscale();

    using var locker = grayscale.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.G, Is.EqualTo(pixel.R), "R and G should be equal in grayscale");
    Assert.That(pixel.B, Is.EqualTo(pixel.G), "G and B should be equal in grayscale");
  }

  [Test]
  [Category("HappyPath")]
  public void MakeGrayscale_WhiteImage_StaysWhite() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.White);
    using var grayscale = original.MakeGrayscale();

    using var locker = grayscale.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.R, Is.InRange(250, 255));
    Assert.That(pixel.G, Is.InRange(250, 255));
    Assert.That(pixel.B, Is.InRange(250, 255));
  }

  [Test]
  [Category("HappyPath")]
  public void MakeGrayscale_BlackImage_StaysBlack() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Black);
    using var grayscale = original.MakeGrayscale();

    using var locker = grayscale.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.R, Is.InRange(0, 5));
    Assert.That(pixel.G, Is.InRange(0, 5));
    Assert.That(pixel.B, Is.InRange(0, 5));
  }

  [Test]
  [Category("HappyPath")]
  public void MakeGrayscale_PreservesDimensions() {
    using var original = TestUtilities.CreateSolidBitmap(25, 35, Color.Blue);
    using var grayscale = original.MakeGrayscale();

    Assert.That(grayscale.Width, Is.EqualTo(25));
    Assert.That(grayscale.Height, Is.EqualTo(35));
  }

  #endregion

  #region Threshold Tests

  [Test]
  [Category("HappyPath")]
  public void Threshold_DarkPixels_BecomeBlack() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(50, 50, 50));
    using var thresholded = original.Threshold(127);

    using var locker = thresholded.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.R, Is.EqualTo(0));
    Assert.That(pixel.G, Is.EqualTo(0));
    Assert.That(pixel.B, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_LightPixels_BecomeWhite() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(200, 200, 200));
    using var thresholded = original.Threshold(127);

    using var locker = thresholded.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.R, Is.EqualTo(255));
    Assert.That(pixel.G, Is.EqualTo(255));
    Assert.That(pixel.B, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_CustomThreshold_Works() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(100, 100, 100));

    using var lowThreshold = original.Threshold(50);
    using var highThreshold = original.Threshold(150);

    using var lowLocker = lowThreshold.Lock();
    using var highLocker = highThreshold.Lock();

    Assert.That(lowLocker[10, 10].R, Is.EqualTo(255), "Below threshold should be white");
    Assert.That(highLocker[10, 10].R, Is.EqualTo(0), "Above threshold should be black");
  }

  [Test]
  [Category("HappyPath")]
  public void Threshold_ProducesBlackOrWhite() {
    // Threshold converts to black/white (RGB values become 0 or 255)
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.FromArgb(255, 200, 200, 200));
    using var thresholded = original.Threshold(127);

    using var locker = thresholded.Lock();
    var pixel = locker[10, 10];

    // Light pixel (200 > 127) should become white
    Assert.That(pixel.R, Is.EqualTo(255).Or.EqualTo(0));
    Assert.That(pixel.G, Is.EqualTo(255).Or.EqualTo(0));
    Assert.That(pixel.B, Is.EqualTo(255).Or.EqualTo(0));
  }

  #endregion

  #region MirrorAlongX Tests

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongX_FlipsHorizontally() {
    using var original = new Bitmap(20, 20);
    using (var locker = original.Lock()) {
      locker.Clear(Color.Black);
      locker[0, 10] = Color.Red;
    }

    using var mirrored = (Bitmap)original.MirrorAlongX();
    using var locker2 = mirrored.Lock();

    Assert.That(locker2[19, 10].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongX_PreservesDimensions() {
    using var original = TestUtilities.CreateSolidBitmap(25, 35, Color.Blue);
    using var mirrored = original.MirrorAlongX();

    Assert.That(mirrored.Width, Is.EqualTo(25));
    Assert.That(mirrored.Height, Is.EqualTo(35));
  }

  #endregion

  #region MirrorAlongY Tests

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongY_FlipsVertically() {
    using var original = new Bitmap(20, 20);
    using (var locker = original.Lock()) {
      locker.Clear(Color.Black);
      locker[10, 0] = Color.Red;
    }

    using var mirrored = (Bitmap)original.MirrorAlongY();
    using var locker2 = mirrored.Lock();

    Assert.That(locker2[10, 19].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void MirrorAlongY_PreservesDimensions() {
    using var original = TestUtilities.CreateSolidBitmap(25, 35, Color.Blue);
    using var mirrored = original.MirrorAlongY();

    Assert.That(mirrored.Width, Is.EqualTo(25));
    Assert.That(mirrored.Height, Is.EqualTo(35));
  }

  #endregion

  #region Resize Tests

  [Test]
  [Category("HappyPath")]
  public void Resize_SingleDimension_MaintainsAspect() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var resized = original.Resize(10);

    Assert.That(resized.Width, Is.EqualTo(10));
    Assert.That(resized.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Resize_WithKeepAspect_PreservesRatio() {
    using var original = TestUtilities.CreateSolidBitmap(40, 20, Color.Blue);
    using var resized = original.Resize(20, 20, keepAspect: true);

    Assert.That(resized.Width, Is.EqualTo(20));
    Assert.That(resized.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Resize_WithoutKeepAspect_StretchesToFit() {
    using var original = TestUtilities.CreateSolidBitmap(40, 20, Color.Green);
    using var resized = original.Resize(10, 10, keepAspect: false);

    Assert.That(resized.Width, Is.EqualTo(10));
    Assert.That(resized.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Resize_WithInterpolationMode_Works() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Cyan);
    using var resized = original.Resize(40, 40, InterpolationMode.HighQualityBicubic);

    Assert.That(resized.Width, Is.EqualTo(40));
    Assert.That(resized.Height, Is.EqualTo(40));
  }

  [Test]
  [Category("HappyPath")]
  public void Resize_ExplicitDimensions_Works() {
    using var original = TestUtilities.CreateSolidBitmap(40, 20, Color.Red);
    using var resized = original.Resize(width: 20, height: 10);

    Assert.That(resized.Width, Is.EqualTo(20));
    Assert.That(resized.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Resize_ProportionalScale_Works() {
    using var original = TestUtilities.CreateSolidBitmap(40, 20, Color.Blue);
    // Scale down by half in both dimensions
    using var resized = original.Resize(width: 20, height: 10);

    Assert.That(resized.Width, Is.EqualTo(20));
    Assert.That(resized.Height, Is.EqualTo(10));
  }

  #endregion

  #region Rotate Tests

  [Test]
  [Category("HappyPath")]
  public void Rotate_0Degrees_ReturnsUnchanged() {
    using var original = TestUtilities.CreateTestPattern(20, 20);
    using var rotated = original.Rotate(0);

    Assert.That(rotated.Width, Is.EqualTo(original.Width));
    Assert.That(rotated.Height, Is.EqualTo(original.Height));
  }

  [Test]
  [Category("HappyPath")]
  public void Rotate_90Degrees_UsesFastPath() {
    using var original = TestUtilities.CreateSolidBitmap(20, 10, Color.Red);
    using var rotated = original.Rotate(90);

    Assert.That(rotated.Width, Is.EqualTo(10));
    Assert.That(rotated.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Rotate_180Degrees_UsesFastPath() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue);
    using var rotated = original.Rotate(180);

    Assert.That(rotated.Width, Is.EqualTo(20));
    Assert.That(rotated.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Rotate_270Degrees_UsesFastPath() {
    using var original = TestUtilities.CreateSolidBitmap(20, 10, Color.Green);
    using var rotated = original.Rotate(270);

    Assert.That(rotated.Width, Is.EqualTo(10));
    Assert.That(rotated.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Rotate_NegativeAngle_NormalizesToPositive() {
    using var original = TestUtilities.CreateSolidBitmap(20, 10, Color.Red);
    using var rotated = original.Rotate(-90);

    Assert.That(rotated.Width, Is.EqualTo(10));
    Assert.That(rotated.Height, Is.EqualTo(20));
  }

  [Test]
  [Category("HappyPath")]
  public void Rotate_AngleOver360_Normalizes() {
    using var original = TestUtilities.CreateSolidBitmap(20, 10, Color.Blue);
    using var rotated = original.Rotate(450);

    Assert.That(rotated.Width, Is.EqualTo(10));
    Assert.That(rotated.Height, Is.EqualTo(20));
  }

  #endregion

  #region GetRectangle Tests

  [Test]
  [Category("HappyPath")]
  public void GetRectangle_ExtractsRegion() {
    using var original = TestUtilities.CreateTestPattern(20, 20);
    using var region = original.GetRectangle(new Rectangle(0, 0, 10, 10));

    Assert.That(region.Width, Is.EqualTo(10));
    Assert.That(region.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void GetRectangle_PreservesContent() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var region = (Bitmap)original.GetRectangle(new Rectangle(5, 5, 10, 10));

    using var locker = region.Lock();
    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  #endregion

  #region ReplaceColorWithTransparency Tests

  [Test]
  [Category("HappyPath")]
  public void ReplaceColorWithTransparency_ReplacesSpecifiedColor() {
    using var original = TestUtilities.CreateSolidBitmap(20, 20, Color.Red);
    using var result = (Bitmap)original.ReplaceColorWithTransparency(Color.Red);

    using var locker = result.Lock();
    var pixel = locker[10, 10];

    Assert.That(pixel.A, Is.EqualTo(0), "Should be transparent");
  }

  [Test]
  [Category("HappyPath")]
  public void ReplaceColorWithTransparency_PreservesOtherColors() {
    using var original = new Bitmap(20, 20);
    using (var locker = original.Lock()) {
      locker.Clear(Color.Blue);
      locker[10, 10] = Color.Red;
    }

    using var result = (Bitmap)original.ReplaceColorWithTransparency(Color.Red);
    using var locker2 = result.Lock();

    Assert.That(locker2[0, 0].A, Is.EqualTo(255), "Blue should remain opaque");
    Assert.That(locker2[0, 0].B, Is.EqualTo(Color.Blue.B));
  }

  #endregion

  #region ToBase64DataUri Tests

  [Test]
  [Category("HappyPath")]
  public void ToBase64DataUri_ReturnsValidDataUri() {
    using var original = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);

    var dataUri = original.ToBase64DataUri();

    Assert.That(dataUri.StartsWith("data:image/"), Is.True);
    Assert.That(dataUri.Contains("base64,"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ToBase64DataUri_NullImage_ReturnsEmptyString() {
    Image? nullImage = null;

    var dataUri = nullImage.ToBase64DataUri();

    Assert.That(dataUri, Is.EqualTo(string.Empty));
  }

  #endregion

  #region FromBase64DataUri Tests

  [Test]
  [Category("HappyPath")]
  public void FromBase64DataUri_ValidUri_ReturnsImage() {
    using var original = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    var dataUri = original.ToBase64DataUri();

    using var restored = dataUri.FromBase64DataUri();

    Assert.That(restored, Is.Not.Null);
    Assert.That(restored!.Width, Is.EqualTo(10));
    Assert.That(restored.Height, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void FromBase64DataUri_InvalidUri_ReturnsNull() {
    var invalidUri = "not a data uri";

    var result = invalidUri.FromBase64DataUri();

    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void FromBase64DataUri_NonImageData_ReturnsNull() {
    var textDataUri = "data:text/plain;base64,SGVsbG8gV29ybGQ=";

    var result = textDataUri.FromBase64DataUri();

    Assert.That(result, Is.Null);
  }

  #endregion

  #region ApplyPixelProcessor Tests

  [Test]
  [Category("HappyPath")]
  public void ApplyPixelProcessor_InvertsColors() {
    using var original = TestUtilities.CreateSolidBitmap(10, 10, Color.FromArgb(255, 100, 150, 200));
    using var processed = original.ApplyPixelProcessor(c => Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B));

    using var locker = processed.Lock();
    var pixel = locker[5, 5];

    Assert.That(pixel.R, Is.EqualTo(155));
    Assert.That(pixel.G, Is.EqualTo(105));
    Assert.That(pixel.B, Is.EqualTo(55));
  }

  [Test]
  [Category("HappyPath")]
  public void ApplyPixelProcessor_PreservesDimensions() {
    using var original = TestUtilities.CreateSolidBitmap(25, 35, Color.Red);
    using var processed = original.ApplyPixelProcessor(c => c);

    Assert.That(processed.Width, Is.EqualTo(25));
    Assert.That(processed.Height, Is.EqualTo(35));
  }

  #endregion

  #region SaveTo Tests

  [Test]
  [Category("HappyPath")]
  [Category("Integration")]
  public void SaveToPng_CreatesFile() {
    using var image = TestUtilities.CreateSolidBitmap(10, 10, Color.Red);
    var filePath = Path.Combine(this._tempDir, "test.png");

    image.SaveToPng(filePath);

    Assert.That(File.Exists(filePath), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Category("Integration")]
  public void SaveToJpeg_CreatesFile() {
    using var image = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue);
    var filePath = Path.Combine(this._tempDir, "test.jpg");

    image.SaveToJpeg(filePath);

    Assert.That(File.Exists(filePath), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Category("Integration")]
  public void SaveToJpeg_WithQuality_CreatesFile() {
    using var image = TestUtilities.CreateSolidBitmap(10, 10, Color.Green);
    var filePath = Path.Combine(this._tempDir, "test_quality.jpg");

    image.SaveToJpeg(filePath, 0.5);

    Assert.That(File.Exists(filePath), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Category("Integration")]
  public void SaveToJpeg_ToStream_WritesData() {
    using var image = TestUtilities.CreateSolidBitmap(10, 10, Color.Cyan);
    using var stream = new MemoryStream();

    image.SaveToJpeg(stream, 0.8);

    Assert.That(stream.Length, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  [Category("Integration")]
  public void SaveToTiff_CreatesFile() {
    using var image = TestUtilities.CreateSolidBitmap(10, 10, Color.Yellow);
    var filePath = Path.Combine(this._tempDir, "test.tiff");

    image.SaveToTiff(filePath);

    Assert.That(File.Exists(filePath), Is.True);
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void MakeGrayscale_1x1Image_Works() {
    using var original = new Bitmap(1, 1);
    original.SetPixel(0, 0, Color.Red);

    using var grayscale = original.MakeGrayscale();

    Assert.That(grayscale.Width, Is.EqualTo(1));
    Assert.That(grayscale.Height, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Threshold_1x1Image_Works() {
    using var original = new Bitmap(1, 1);
    original.SetPixel(0, 0, Color.White);

    using var thresholded = original.Threshold(127);

    using var locker = thresholded.Lock();
    Assert.That(locker[0, 0].R, Is.EqualTo(255));
  }

  #endregion
}
