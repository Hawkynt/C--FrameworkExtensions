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
using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("Graphics")]
public class GraphicsExtensionsTests {

  #region DrawString with Alignment Tests

  [Test]
  [Category("HappyPath")]
  public void DrawString_TopLeft_DrawsAtPosition() {
    using var bitmap = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var font = new Font("Arial", 10);
    using var brush = new SolidBrush(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawString(10, 10, "Test", font, brush, ContentAlignment.TopLeft);

    // Verify text was drawn (some pixels should be white near 10,10)
    using var locker = bitmap.Lock();
    var hasWhitePixel = false;
    for (var y = 10; y < 30 && !hasWhitePixel; ++y)
    for (var x = 10; x < 50 && !hasWhitePixel; ++x)
      if (locker[x, y].R > 100)
        hasWhitePixel = true;

    Assert.That(hasWhitePixel, Is.True, "Text should be drawn near position");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawString_MiddleCenter_CentersText() {
    using var bitmap = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var font = new Font("Arial", 10);
    using var brush = new SolidBrush(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawString(50, 50, "X", font, brush, ContentAlignment.MiddleCenter);

    // Text should be centered around 50,50
    using var locker = bitmap.Lock();
    var hasPixelNearCenter = false;
    for (var y = 40; y < 60 && !hasPixelNearCenter; ++y)
    for (var x = 40; x < 60 && !hasPixelNearCenter; ++x)
      if (locker[x, y].R > 100)
        hasPixelNearCenter = true;

    Assert.That(hasPixelNearCenter, Is.True, "Text should be centered");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawString_BottomRight_AlignsToBottomRight() {
    using var bitmap = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var font = new Font("Arial", 10);
    using var brush = new SolidBrush(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawString(90, 90, "X", font, brush, ContentAlignment.BottomRight);

    // Text should be to the left and above 90,90
    using var locker = bitmap.Lock();
    var hasPixelNearPosition = false;
    for (var y = 70; y < 90 && !hasPixelNearPosition; ++y)
    for (var x = 70; x < 90 && !hasPixelNearPosition; ++x)
      if (locker[x, y].R > 100)
        hasPixelNearPosition = true;

    Assert.That(hasPixelNearPosition, Is.True, "Text should be aligned bottom-right");
  }

  #endregion

  #region DrawCross Tests

  [Test]
  [Category("HappyPath")]
  public void DrawCross_Float_DrawsFourLines() {
    using var bitmap = new Bitmap(50, 50, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var pen = new Pen(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawCross(25f, 25f, 10f, pen);

    using var locker = bitmap.Lock();
    // Check horizontal line
    Assert.That(locker[15, 25].R, Is.GreaterThan(100), "Left arm of cross");
    Assert.That(locker[35, 25].R, Is.GreaterThan(100), "Right arm of cross");
    // Check vertical line
    Assert.That(locker[25, 15].R, Is.GreaterThan(100), "Top arm of cross");
    Assert.That(locker[25, 35].R, Is.GreaterThan(100), "Bottom arm of cross");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawCross_Int_DrawsFourLines() {
    using var bitmap = new Bitmap(50, 50, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var pen = new Pen(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawCross(25, 25, 10, pen);

    using var locker = bitmap.Lock();
    Assert.That(locker[15, 25].R, Is.GreaterThan(100), "Left arm of cross");
    Assert.That(locker[35, 25].R, Is.GreaterThan(100), "Right arm of cross");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawCross_Point_DrawsAtPoint() {
    using var bitmap = new Bitmap(50, 50, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var pen = new Pen(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawCross(new Point(25, 25), 10, pen);

    using var locker = bitmap.Lock();
    Assert.That(locker[25, 25].R, Is.GreaterThan(100), "Center of cross");
  }

  [Test]
  [Category("HappyPath")]
  public void DrawCross_PointF_DrawsAtPoint() {
    using var bitmap = new Bitmap(50, 50, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var pen = new Pen(Color.White);

    graphics.Clear(Color.Black);
    graphics.DrawCross(new PointF(25.5f, 25.5f), 10, pen);

    using var locker = bitmap.Lock();
    Assert.That(locker[25, 25].R, Is.GreaterThan(100), "Near center of cross");
  }

  #endregion

  #region DrawCircle Tests

  [Test]
  [Category("HappyPath")]
  public void DrawCircle_DrawsCircleOutline() {
    using var bitmap = new Bitmap(80, 80, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var pen = new Pen(Color.White);

    graphics.Clear(Color.Black);
    // Note: DrawCircle uses (x, y) as top-left of bounding box, not center
    // With x=10, y=10, radius=15: bounding box is (10,10) to (40,40), center at (25,25)
    graphics.DrawCircle(pen, 10f, 10f, 15f);

    using var locker = bitmap.Lock();
    // Check points on circle edge - top edge should be near y=10
    var hasTopEdge = false;
    for (var y = 8; y <= 13 && !hasTopEdge; ++y)
      if (locker[25, y].R > 100)
        hasTopEdge = true;
    Assert.That(hasTopEdge, Is.True, "Circle should have top edge");

    // Center of circle (25, 25) should be empty (it's just outline)
    Assert.That(locker[25, 25].R, Is.LessThan(50), "Center should be empty");
  }

  #endregion

  #region FillCircle Tests

  [Test]
  [Category("HappyPath")]
  public void FillCircle_FillsCircle() {
    using var bitmap = new Bitmap(60, 60, PixelFormat.Format32bppArgb);
    using var graphics = Graphics.FromImage(bitmap);
    using var brush = new SolidBrush(Color.White);

    graphics.Clear(Color.Black);
    graphics.FillCircle(brush, 30f, 30f, 15f);

    using var locker = bitmap.Lock();
    // Center should be filled
    Assert.That(locker[30, 30].R, Is.GreaterThan(100), "Center should be filled");
    // Points near center should be filled
    Assert.That(locker[25, 30].R, Is.GreaterThan(100), "Left of center should be filled");
    Assert.That(locker[35, 30].R, Is.GreaterThan(100), "Right of center should be filled");
    // Points outside circle should be empty
    Assert.That(locker[10, 10].R, Is.LessThan(50), "Corner should be empty");
  }

  #endregion

}
