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

using System.Drawing.Imaging;
using Hawkynt.Drawing.Lockers;
using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("BitmapLocker")]
public class BitmapLockerTests {

  #region Pixel Read/Write Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(PixelFormat.Format32bppArgb)]
  [TestCase(PixelFormat.Format32bppRgb)]
  [TestCase(PixelFormat.Format24bppRgb)]
  public void PixelReadWrite_FullColorFormat_RoundTrips(PixelFormat format) {
    using var bitmap = new Bitmap(10, 10, format);
    using var locker = bitmap.Lock();

    var testColor = Color.FromArgb(255, 128, 64, 32);
    locker[5, 5] = testColor;
    var readColor = locker[5, 5];

    var expectedAlpha = TestUtilities.SupportsAlpha(format) ? testColor.A : 255;
    Assert.That(readColor.A, Is.EqualTo(expectedAlpha), "Alpha mismatch");
    Assert.That(readColor.R, Is.EqualTo(testColor.R), "Red mismatch");
    Assert.That(readColor.G, Is.EqualTo(testColor.G), "Green mismatch");
    Assert.That(readColor.B, Is.EqualTo(testColor.B), "Blue mismatch");
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(PixelFormat.Format16bppRgb565)]
  [TestCase(PixelFormat.Format16bppRgb555)]
  [TestCase(PixelFormat.Format16bppArgb1555)]
  public void PixelReadWrite_16BitFormat_RoundTripsWithTolerance(PixelFormat format) {
    using var bitmap = new Bitmap(10, 10, format);
    using var locker = bitmap.Lock();

    var testColor = Color.FromArgb(255, 128, 64, 32);
    locker[5, 5] = testColor;
    var readColor = locker[5, 5];

    var tolerance = TestUtilities.GetColorTolerance(format);
    Assert.That(Math.Abs(readColor.R - testColor.R), Is.LessThanOrEqualTo(tolerance), "Red outside tolerance");
    Assert.That(Math.Abs(readColor.G - testColor.G), Is.LessThanOrEqualTo(tolerance), "Green outside tolerance");
    Assert.That(Math.Abs(readColor.B - testColor.B), Is.LessThanOrEqualTo(tolerance), "Blue outside tolerance");
  }

  [Test]
  [Category("HappyPath")]
  public void PixelReadWrite_Indexed8_ReadsFromPalette() {
    // Test that indexed format reads from palette correctly
    // Note: Writing to indexed formats requires exact Color match with palette entries
    // which is implementation-dependent, so we only test reading here
    using var bitmap = new Bitmap(10, 10, PixelFormat.Format8bppIndexed);

    // Set up palette with explicit colors
    var palette = bitmap.Palette;
    palette.Entries[0] = Color.FromArgb(255, 255, 0, 0); // Red at index 0
    palette.Entries[1] = Color.FromArgb(255, 0, 255, 0); // Green at index 1
    bitmap.Palette = palette;

    using var locker = bitmap.Lock();
    // Default pixel value is 0, which maps to palette entry 0 (Red)
    var readColor = locker[0, 0];

    Assert.That(readColor.R, Is.EqualTo(255));
    Assert.That(readColor.G, Is.EqualTo(0));
    Assert.That(readColor.B, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PixelReadWrite_GrayScale16_ConvertsToGray() {
    using var bitmap = new Bitmap(10, 10, PixelFormat.Format16bppGrayScale);
    using var locker = bitmap.Lock();

    var testColor = Color.FromArgb(255, 100, 150, 200);
    locker[5, 5] = testColor;
    var readColor = locker[5, 5];

    Assert.That(readColor.G, Is.EqualTo(readColor.R), "Gray R!=G");
    Assert.That(readColor.B, Is.EqualTo(readColor.G), "Gray G!=B");
  }

  [Test]
  [Category("HappyPath")]
  public void PixelReadWrite_PointOverload_Works() {
    using var bitmap = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    var point = new Point(3, 7);
    var testColor = Color.Magenta;
    locker[point] = testColor;

    Assert.That(locker[point].ToArgb(), Is.EqualTo(testColor.ToArgb()));
    Assert.That(locker[3, 7].ToArgb(), Is.EqualTo(testColor.ToArgb()));
  }

  #endregion

  #region Clear Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(PixelFormat.Format32bppArgb)]
  [TestCase(PixelFormat.Format24bppRgb)]
  public void Clear_FillsEntireBitmap(PixelFormat format) {
    using var bitmap = new Bitmap(20, 20, format);
    using var locker = bitmap.Lock();

    var clearColor = Color.Cyan;
    locker.Clear(clearColor);

    for (var y = 0; y < 20; ++y)
    for (var x = 0; x < 20; ++x) {
      var pixel = locker[x, y];
      Assert.That(pixel.R, Is.EqualTo(clearColor.R), $"Mismatch at ({x},{y})");
      Assert.That(pixel.G, Is.EqualTo(clearColor.G), $"Mismatch at ({x},{y})");
      Assert.That(pixel.B, Is.EqualTo(clearColor.B), $"Mismatch at ({x},{y})");
    }
  }

  #endregion

  #region DrawHorizontalLine Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(PixelFormat.Format32bppArgb)]
  [TestCase(PixelFormat.Format24bppRgb)]
  public void DrawHorizontalLine_DrawsCorrectPixels(PixelFormat format) {
    using var bitmap = new Bitmap(20, 20, format);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawHorizontalLine(5, 10, 8, Color.White);

    for (var x = 5; x < 13; ++x)
      Assert.That(locker[x, 10].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Line pixel at x={x} should be white");

    Assert.That(locker[4, 10].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel before line should be black");
    Assert.That(locker[13, 10].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel after line should be black");
    Assert.That(locker[5, 9].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel above line should be black");
    Assert.That(locker[5, 11].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel below line should be black");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawHorizontalLine_MultiplePixels_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawHorizontalLine(2, 5, 4, Color.Red);

    for (var x = 2; x < 6; ++x)
      Assert.That(locker[x, 5].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void DrawHorizontalLine_PointOverload_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawHorizontalLine(new Point(2, 5), 4, Color.Red);

    for (var x = 2; x < 6; ++x)
      Assert.That(locker[x, 5].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  #endregion

  #region DrawVerticalLine Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(PixelFormat.Format32bppArgb)]
  [TestCase(PixelFormat.Format24bppRgb)]
  public void DrawVerticalLine_DrawsCorrectPixels(PixelFormat format) {
    using var bitmap = new Bitmap(20, 20, format);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawVerticalLine(10, 5, 8, Color.White);

    for (var y = 5; y < 13; ++y)
      Assert.That(locker[10, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Line pixel at y={y} should be white");

    Assert.That(locker[10, 4].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel before line should be black");
    Assert.That(locker[10, 13].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Pixel after line should be black");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawVerticalLine_PointOverload_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawVerticalLine(new Point(10, 5), 8, Color.White);

    for (var y = 5; y < 13; ++y)
      Assert.That(locker[10, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Line pixel at y={y} should be white");
  }

  #endregion

  #region DrawLine Tests (Diagonal - Bresenham)

  [Test]
  [Category("HappyPath")]
  public void DrawLine_Diagonal_DrawsCorrectly() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawLine(0, 0, 9, 9, Color.White);

    // Verify endpoints are drawn
    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.White.ToArgb()), "Start point should be white");
    Assert.That(locker[9, 9].ToArgb(), Is.EqualTo(Color.White.ToArgb()), "End point should be white");

    // Count white pixels - should be at least 10 for a 10-pixel diagonal
    var whiteCount = 0;
    for (var y = 0; y < 10; ++y)
    for (var x = 0; x < 10; ++x)
      if (locker[x, y].ToArgb() == Color.White.ToArgb())
        ++whiteCount;
    Assert.That(whiteCount, Is.GreaterThanOrEqualTo(10), "Should have at least 10 white pixels");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawLine_Horizontal_DrawsCorrectly() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawLine(2, 5, 12, 5, Color.Green);

    // DrawLine uses count = x1-x0 = 10, so draws from x=2 for 10 pixels (x=2 to x=11)
    for (var x = 2; x < 12; ++x)
      Assert.That(locker[x, 5].ToArgb(), Is.EqualTo(Color.Green.ToArgb()), $"Pixel at x={x} should be green");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawLine_Vertical_DrawsCorrectly() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawLine(5, 2, 5, 12, Color.Blue);

    // DrawLine uses count = y1-y0, so draws from y=2 for 10 pixels (y=2 to y=11)
    for (var y = 2; y < 12; ++y)
      Assert.That(locker[5, y].ToArgb(), Is.EqualTo(Color.Blue.ToArgb()), $"Pixel at y={y} should be blue");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawLine_ShortDiagonal_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawLine(0, 0, 5, 5, Color.Yellow);

    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.Yellow.ToArgb()));
    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Yellow.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void DrawLine_PointOverload_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawLine(new Point(0, 0), new Point(5, 5), Color.Yellow);

    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.Yellow.ToArgb()));
    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Yellow.ToArgb()));
  }

  #endregion

  #region DrawRectangle Tests

  [Test]
  [Category("HappyPath")]
  public void DrawRectangle_DrawsOutline() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawRectangle(5, 5, 10, 10, Color.White);

    for (var x = 5; x < 15; ++x)
      Assert.That(locker[x, 5].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Top edge at x={x}");

    for (var x = 5; x < 15; ++x)
      Assert.That(locker[x, 14].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Bottom edge at x={x}");

    for (var y = 5; y < 15; ++y)
      Assert.That(locker[5, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Left edge at y={y}");

    for (var y = 5; y < 15; ++y)
      Assert.That(locker[14, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Right edge at y={y}");

    Assert.That(locker[10, 10].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Interior should be black");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawRectangle_SmallRectangle_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawRectangle(2, 2, 5, 5, Color.Red);

    Assert.That(locker[2, 2].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    Assert.That(locker[6, 6].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void DrawRectangle_RectangleOverload_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.DrawRectangle(new Rectangle(5, 5, 10, 10), Color.White);

    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.White.ToArgb()), "Top-left corner");
    Assert.That(locker[14, 14].ToArgb(), Is.EqualTo(Color.White.ToArgb()), "Bottom-right corner");
    Assert.That(locker[10, 10].ToArgb(), Is.EqualTo(Color.Black.ToArgb()), "Interior should be black");
  }

  #endregion

  #region FillRectangle Tests

  [Test]
  [Category("HappyPath")]
  public void FillRectangle_FillsArea() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.FillRectangle(5, 5, 10, 10, Color.White);

    for (var y = 5; y < 15; ++y)
    for (var x = 5; x < 15; ++x)
      Assert.That(locker[x, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Pixel at ({x},{y}) should be white");

    Assert.That(locker[4, 4].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
    Assert.That(locker[15, 15].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void FillRectangle_ClipsToValidArea() {
    // FillRectangle (not Checked) clips to valid area
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.FillRectangle(-5, -5, 30, 30, Color.White);

    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.White.ToArgb()));
    Assert.That(locker[19, 19].ToArgb(), Is.EqualTo(Color.White.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void FillRectangle_NegativeCoords_ClipsCorrectly() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.FillRectangle(-5, -5, 10, 10, Color.White);

    // Should only fill from (0,0) to (4,4) since rectangle starts at -5,-5 with size 10x10
    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.White.ToArgb()));
    Assert.That(locker[4, 4].ToArgb(), Is.EqualTo(Color.White.ToArgb()));
    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void FillRectangle_RectangleOverload_Works() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.FillRectangle(new Rectangle(5, 5, 10, 10), Color.White);

    for (var y = 5; y < 15; ++y)
    for (var x = 5; x < 15; ++x)
      Assert.That(locker[x, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Pixel at ({x},{y}) should be white");

    Assert.That(locker[4, 4].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
    Assert.That(locker[15, 15].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
  }

  [Test]
  [Category("HappyPath")]
  public void FillRectangleChecked_ValidBounds_FillsArea() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.Clear(Color.Black);
    locker.FillRectangleChecked(5, 5, 10, 10, Color.White);

    for (var y = 5; y < 15; ++y)
    for (var x = 5; x < 15; ++x)
      Assert.That(locker[x, y].ToArgb(), Is.EqualTo(Color.White.ToArgb()), $"Pixel at ({x},{y}) should be white");
  }

  [Test]
  [Category("Exception")]
  public void FillRectangleChecked_OutOfBounds_ThrowsException() {
    using var bitmap = new Bitmap(20, 20, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    Assert.Throws<ArgumentOutOfRangeException>(() => locker.FillRectangleChecked(-1, 0, 10, 10, Color.White));
    Assert.Throws<ArgumentOutOfRangeException>(() => locker.FillRectangleChecked(0, -1, 10, 10, Color.White));
    Assert.Throws<ArgumentOutOfRangeException>(() => locker.FillRectangleChecked(15, 0, 10, 10, Color.White));
    Assert.Throws<ArgumentOutOfRangeException>(() => locker.FillRectangleChecked(0, 15, 10, 10, Color.White));
  }

  #endregion

  #region CopyFrom Tests

  [Test]
  [Category("HappyPath")]
  public void CopyFrom_CopiesEntireBitmap() {
    using var source = TestUtilities.CreateTestPattern(20, 20, PixelFormat.Format32bppArgb);
    using var dest = new Bitmap(20, 20, PixelFormat.Format32bppArgb);

    using (var sourceLock = source.Lock())
    using (var destLock = dest.Lock()) {
      destLock.CopyFrom(sourceLock);
    }

    Assert.That(TestUtilities.AreBitmapsEqual(source, dest), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CopyFrom_CopiesRegion() {
    using var source = TestUtilities.CreateSolidBitmap(20, 20, Color.Red, PixelFormat.Format32bppArgb);
    using var dest = TestUtilities.CreateSolidBitmap(20, 20, Color.Blue, PixelFormat.Format32bppArgb);

    using (var sourceLock = source.Lock())
    using (var destLock = dest.Lock()) {
      destLock.CopyFrom(sourceLock, 0, 0, 10, 10, 5, 5);
    }

    using var destLock2 = dest.Lock();
    Assert.That(destLock2[5, 5].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    Assert.That(destLock2[14, 14].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    Assert.That(destLock2[0, 0].ToArgb(), Is.EqualTo(Color.Blue.ToArgb()));
    Assert.That(destLock2[15, 15].ToArgb(), Is.EqualTo(Color.Blue.ToArgb()));
  }

  #endregion

  #region IsFlatColor Tests

  [Test]
  [Category("HappyPath")]
  public void IsFlatColor_SolidBitmap_ReturnsTrue() {
    using var bitmap = TestUtilities.CreateSolidBitmap(20, 20, Color.Red, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    Assert.That(locker.IsFlatColor, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsFlatColor_MultiColorBitmap_ReturnsFalse() {
    using var bitmap = TestUtilities.CreateCheckerboard(20, 20, 5, Color.Black, Color.White, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    Assert.That(locker.IsFlatColor, Is.False);
  }

  #endregion

  #region Width/Height Tests

  [Test]
  [Category("HappyPath")]
  public void WidthHeight_ReturnsCorrectDimensions() {
    using var bitmap = new Bitmap(25, 35, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    Assert.That(locker.Width, Is.EqualTo(25));
    Assert.That(locker.Height, Is.EqualTo(35));
  }

  #endregion

  #region BitmapData Property Tests

  [Test]
  [Category("HappyPath")]
  public void BitmapData_IsNotNull() {
    using var bitmap = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    Assert.That(locker.BitmapData, Is.Not.Null);
    Assert.That(locker.BitmapData.Width, Is.EqualTo(10));
    Assert.That(locker.BitmapData.Height, Is.EqualTo(10));
  }

  #endregion

  #region Alpha Blending Tests

  [Test]
  [Category("HappyPath")]
  public void BlendWith_BlendsSemiTransparentPixels() {
    using var source = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using var dest = TestUtilities.CreateSolidBitmap(10, 10, Color.Blue, PixelFormat.Format32bppArgb);

    using (var sourceLock = source.Lock()) {
      // Use fully opaque red to verify blending actually copies pixels
      sourceLock.Clear(Color.FromArgb(255, 255, 0, 0));
    }

    using (var sourceLock = source.Lock())
    using (var destLock = dest.Lock()) {
      destLock.BlendWith(sourceLock);
    }

    using var destLock2 = dest.Lock();
    var blendedColor = destLock2[5, 5];

    // With opaque source, destination should be replaced by source
    Assert.That(blendedColor.R, Is.EqualTo(255), "Fully opaque source should replace dest red");
    Assert.That(blendedColor.B, Is.EqualTo(0), "Fully opaque source should replace dest blue");
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void PixelReadWrite_AtBounds_Works() {
    using var bitmap = new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker[0, 0] = Color.Red;
    locker[9, 0] = Color.Green;
    locker[0, 9] = Color.Blue;
    locker[9, 9] = Color.Yellow;

    Assert.That(locker[0, 0].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    Assert.That(locker[9, 0].ToArgb(), Is.EqualTo(Color.Green.ToArgb()));
    Assert.That(locker[0, 9].ToArgb(), Is.EqualTo(Color.Blue.ToArgb()));
    Assert.That(locker[9, 9].ToArgb(), Is.EqualTo(Color.Yellow.ToArgb()));
  }

  [Test]
  [Category("EdgeCase")]
  public void DrawHorizontalLine_ZeroLength_DoesNothing() {
    using var bitmap = TestUtilities.CreateSolidBitmap(10, 10, Color.Black, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.DrawHorizontalLine(5, 5, 0, Color.White);

    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
  }

  [Test]
  [Category("EdgeCase")]
  public void FillRectangle_ZeroSize_DoesNothing() {
    using var bitmap = TestUtilities.CreateSolidBitmap(10, 10, Color.Black, PixelFormat.Format32bppArgb);
    using var locker = bitmap.Lock();

    locker.FillRectangle(5, 5, 0, 0, Color.White);

    Assert.That(locker[5, 5].ToArgb(), Is.EqualTo(Color.Black.ToArgb()));
  }

  #endregion
}
