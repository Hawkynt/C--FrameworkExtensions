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

namespace System.Drawing.Tests;

/// <summary>
/// Utility methods for creating and comparing bitmaps in tests.
/// </summary>
internal static class TestUtilities {

  /// <summary>
  /// Creates a solid-color bitmap with the specified dimensions and pixel format.
  /// </summary>
  public static Bitmap CreateSolidBitmap(int width, int height, Color color, PixelFormat format = PixelFormat.Format32bppArgb) {
    var bitmap = new Bitmap(width, height, format);

    if (format == PixelFormat.Format8bppIndexed || format == PixelFormat.Format4bppIndexed || format == PixelFormat.Format1bppIndexed) {
      // For indexed formats, set up palette and use locker
      var palette = bitmap.Palette;
      palette.Entries[0] = color;
      if (format != PixelFormat.Format1bppIndexed) {
        for (var i = 1; i < palette.Entries.Length; ++i)
          palette.Entries[i] = Color.FromArgb(i, i, i);
      } else {
        palette.Entries[1] = Color.White;
      }
      bitmap.Palette = palette;
    }

    using var locker = bitmap.Lock();
    locker.Clear(color);
    return bitmap;
  }

  /// <summary>
  /// Creates a horizontal gradient bitmap from one color to another.
  /// </summary>
  public static Bitmap CreateGradientBitmap(int width, int height, Color from, Color to, PixelFormat format = PixelFormat.Format32bppArgb) {
    var bitmap = new Bitmap(width, height, format);
    using var locker = bitmap.Lock();

    for (var x = 0; x < width; ++x) {
      var t = width > 1 ? (float)x / (width - 1) : 0;
      var color = InterpolateColor(from, to, t);
      locker.DrawVerticalLine(x, 0, height, color);
    }

    return bitmap;
  }

  /// <summary>
  /// Creates a checkerboard pattern bitmap.
  /// </summary>
  public static Bitmap CreateCheckerboard(int width, int height, int cellSize, Color c1, Color c2, PixelFormat format = PixelFormat.Format32bppArgb) {
    var bitmap = new Bitmap(width, height, format);

    if (format == PixelFormat.Format8bppIndexed || format == PixelFormat.Format4bppIndexed || format == PixelFormat.Format1bppIndexed) {
      var palette = bitmap.Palette;
      palette.Entries[0] = c1;
      palette.Entries[1] = c2;
      bitmap.Palette = palette;
    }

    using var locker = bitmap.Lock();

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var isEven = ((x / cellSize) + (y / cellSize)) % 2 == 0;
      locker[x, y] = isEven ? c1 : c2;
    }

    return bitmap;
  }

  /// <summary>
  /// Creates a bitmap with a simple test pattern (colored quadrants).
  /// </summary>
  public static Bitmap CreateTestPattern(int width, int height, PixelFormat format = PixelFormat.Format32bppArgb) {
    var bitmap = new Bitmap(width, height, format);
    using var locker = bitmap.Lock();

    var halfW = width / 2;
    var halfH = height / 2;

    // Top-left: Red
    locker.FillRectangle(0, 0, halfW, halfH, Color.Red);
    // Top-right: Green
    locker.FillRectangle(halfW, 0, width - halfW, halfH, Color.Green);
    // Bottom-left: Blue
    locker.FillRectangle(0, halfH, halfW, height - halfH, Color.Blue);
    // Bottom-right: Yellow
    locker.FillRectangle(halfW, halfH, width - halfW, height - halfH, Color.Yellow);

    return bitmap;
  }

  /// <summary>
  /// Compares two bitmaps for equality with optional tolerance.
  /// </summary>
  /// <param name="a">First bitmap.</param>
  /// <param name="b">Second bitmap.</param>
  /// <param name="tolerance">Maximum allowed difference per color channel (0-255).</param>
  /// <returns>True if bitmaps are equal within tolerance.</returns>
  public static bool AreBitmapsEqual(Bitmap a, Bitmap b, int tolerance = 0) {
    if (a.Width != b.Width || a.Height != b.Height)
      return false;

    using var lockerA = a.Lock();
    using var lockerB = b.Lock();

    for (var y = 0; y < a.Height; ++y)
    for (var x = 0; x < a.Width; ++x)
      if (!AreColorsEqual(lockerA[x, y], lockerB[x, y], tolerance))
        return false;

    return true;
  }

  /// <summary>
  /// Compares two colors for equality with optional tolerance.
  /// </summary>
  public static bool AreColorsEqual(Color a, Color b, int tolerance = 0) {
    if (tolerance == 0)
      return a.ToArgb() == b.ToArgb();

    return Math.Abs(a.A - b.A) <= tolerance &&
           Math.Abs(a.R - b.R) <= tolerance &&
           Math.Abs(a.G - b.G) <= tolerance &&
           Math.Abs(a.B - b.B) <= tolerance;
  }

  /// <summary>
  /// Interpolates between two colors.
  /// </summary>
  private static Color InterpolateColor(Color from, Color to, float t) {
    var a = (int)(from.A + (to.A - from.A) * t);
    var r = (int)(from.R + (to.R - from.R) * t);
    var g = (int)(from.G + (to.G - from.G) * t);
    var b = (int)(from.B + (to.B - from.B) * t);
    return Color.FromArgb(a, r, g, b);
  }

  /// <summary>
  /// Gets all testable pixel formats.
  /// </summary>
  public static PixelFormat[] GetTestablePixelFormats() => [
    PixelFormat.Format32bppArgb,
    PixelFormat.Format32bppRgb,
    PixelFormat.Format24bppRgb,
    PixelFormat.Format16bppRgb565,
    PixelFormat.Format16bppRgb555,
    PixelFormat.Format16bppArgb1555,
    PixelFormat.Format16bppGrayScale,
    PixelFormat.Format8bppIndexed,
    PixelFormat.Format4bppIndexed,
    PixelFormat.Format1bppIndexed
  ];

  /// <summary>
  /// Gets pixel formats that support full color (not indexed).
  /// </summary>
  public static PixelFormat[] GetFullColorPixelFormats() => [
    PixelFormat.Format32bppArgb,
    PixelFormat.Format32bppRgb,
    PixelFormat.Format24bppRgb,
    PixelFormat.Format16bppRgb565,
    PixelFormat.Format16bppRgb555,
    PixelFormat.Format16bppArgb1555,
    PixelFormat.Format16bppGrayScale
  ];

  /// <summary>
  /// Gets the expected tolerance for color comparisons based on pixel format bit depth.
  /// </summary>
  public static int GetColorTolerance(PixelFormat format) => format switch {
    PixelFormat.Format32bppArgb => 0,
    PixelFormat.Format32bppRgb => 0,
    PixelFormat.Format24bppRgb => 0,
    PixelFormat.Format16bppRgb565 => 8,   // 5-6-5 bits = up to 8 levels lost
    PixelFormat.Format16bppRgb555 => 8,   // 5-5-5 bits = up to 8 levels lost
    PixelFormat.Format16bppArgb1555 => 8, // 5-5-5 bits + 1-bit alpha
    PixelFormat.Format16bppGrayScale => 1,
    _ => 0
  };

  /// <summary>
  /// Determines if a pixel format supports alpha transparency.
  /// </summary>
  public static bool SupportsAlpha(PixelFormat format) => format switch {
    PixelFormat.Format32bppArgb => true,
    PixelFormat.Format16bppArgb1555 => true, // Only 1-bit alpha
    _ => false
  };
}
