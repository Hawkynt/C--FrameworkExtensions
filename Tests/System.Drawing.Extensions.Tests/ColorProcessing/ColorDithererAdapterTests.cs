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
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.Drawing.ColorDomain;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ColorDomain")]
public class ColorDithererAdapterTests {

  private static Bitmap CreateGradient(int width = 16, int height = 16) {
    var bitmap = new Bitmap(width, height);
    using var locker = bitmap.Lock();
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var v = (byte)((x * 255 + y * 255) / (width + height));
      locker[x, y] = Color.FromArgb(v, v, v);
    }
    return bitmap;
  }

  private static Color[] BlackWhitePalette() => new[] { Color.Black, Color.White };

  private static byte[] DitherToIndices(IColorDitherer ditherer, Bitmap source, Color[] palette) {
    using var target = new Bitmap(source.Width, source.Height, PixelFormat.Format8bppIndexed);
    var data = target.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
    try {
      using var locker = source.Lock();
      ditherer.Dither(locker, data, palette);
      var bytes = new byte[source.Width * source.Height];
      unsafe {
        var ptr = (byte*)data.Scan0;
        for (var y = 0; y < source.Height; ++y)
        for (var x = 0; x < source.Width; ++x)
          bytes[y * source.Width + x] = ptr[y * data.Stride + x];
      }
      return bytes;
    } finally {
      target.UnlockBits(data);
    }
  }

  [Test]
  public void Adapter_ProducesIndicesWithinPaletteBounds() {
    using var source = CreateGradient();
    var palette = BlackWhitePalette();
    var adapter = new ColorDithererAdapter(ErrorDiffusion.FloydSteinberg);

    var indices = DitherToIndices(adapter, source, palette);

    foreach (var b in indices)
      Assert.That(b, Is.LessThan(palette.Length), "Adapter produced an index outside the palette");
  }

  [Test]
  public void Adapter_EmptyPalette_ZeroFillsTarget() {
    using var source = CreateGradient(8, 8);
    var adapter = new ColorDithererAdapter(NoDithering.Instance);

    var indices = DitherToIndices(adapter, source, []);

    foreach (var b in indices)
      Assert.That(b, Is.EqualTo(0));
  }

  [Test]
  public void Adapter_DifferentDitherers_ProduceDifferentIndices() {
    using var source = CreateGradient(32, 32);
    var palette = BlackWhitePalette();

    var floyd = DitherToIndices(new ColorDithererAdapter(ErrorDiffusion.FloydSteinberg), source, palette);
    var ordered = DitherToIndices(new ColorDithererAdapter(OrderedDitherer.Bayer8x8), source, palette);

    var diffs = 0;
    for (var i = 0; i < floyd.Length; ++i)
      if (floyd[i] != ordered[i])
        ++diffs;
    Assert.That(diffs, Is.GreaterThan(floyd.Length / 4),
      "Floyd-Steinberg vs Bayer8x8 should differ in many pixels for a continuous gradient");
  }

  [Test]
  public void Adapter_NullInner_Throws() {
    Assert.Throws<ArgumentNullException>(() => _ = new ColorDithererAdapter(null!));
  }

  [Test]
  public void Adapter_InnerProperty_ReturnsConstructorArg() {
    var inner = ErrorDiffusion.Atkinson;
    var adapter = new ColorDithererAdapter(inner);
    Assert.That(adapter.Inner, Is.EqualTo((Hawkynt.ColorProcessing.IDitherer)inner));
  }
}
