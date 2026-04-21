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
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Verifies that <see cref="NoiseDitherer"/>'s additive-perturb mode (new in this revision)
/// produces visibly different output from the default threshold-selection mode on the
/// same source image — confirming the <see cref="NoiseMode"/> dispatch actually reaches
/// the new code path.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Dithering")]
[Category("NoiseMode")]
public class NoiseDithererAdditiveModeTests {

  private static Bitmap CreateGradient(int width = 64, int height = 64) {
    var bitmap = new Bitmap(width, height);
    using var locker = bitmap.Lock();
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var v = (byte)((x + y) * 255 / (width + height));
      locker[x, y] = Color.FromArgb(v, v, v);
    }
    return bitmap;
  }

  private static byte[] ExtractIndices(Bitmap indexed) {
    using var locker = indexed.Lock();
    var data = locker.BitmapData;
    var result = new byte[data.Width * data.Height];
    unsafe {
      var ptr = (byte*)data.Scan0;
      for (var y = 0; y < data.Height; ++y)
      for (var x = 0; x < data.Width; ++x)
        result[y * data.Width + x] = ptr[y * data.Stride + x];
    }
    return result;
  }

  [Test]
  public void AdditiveMode_ProducesDifferentOutputThanThresholdMode() {
    using var source = CreateGradient();

    var threshold = new NoiseDitherer(NoiseType.White, strength: 0.7f, seed: 42, NoiseMode.ThresholdSelection);
    var additive = new NoiseDitherer(NoiseType.White, strength: 0.7f, seed: 42, NoiseMode.AdditivePerturb);

    using var withThreshold = source.ReduceColors(new OctreeQuantizer(), threshold, colorCount: 4);
    using var withAdditive = source.ReduceColors(new OctreeQuantizer(), additive, colorCount: 4);

    var a = ExtractIndices(withThreshold);
    var b = ExtractIndices(withAdditive);

    Assert.That(a.Length, Is.EqualTo(b.Length));
    var differences = 0;
    for (var i = 0; i < a.Length; ++i)
      if (a[i] != b[i])
        ++differences;
    Assert.That(differences, Is.GreaterThan(a.Length / 20),
      $"Additive vs threshold mode produced too few differences ({differences}/{a.Length}); mode dispatch likely inert");
  }

  [Test]
  public void AdditivePresets_MatchExplicitlyConstructed() {
    var expected = new NoiseDitherer(NoiseType.White, 0.5f, 42, NoiseMode.AdditivePerturb);
    var preset = NoiseDitherer.WhiteNoiseAdditive;

    Assert.That(preset.NoiseType, Is.EqualTo(expected.NoiseType));
    Assert.That(preset.Strength, Is.EqualTo(expected.Strength));
    Assert.That(preset.Seed, Is.EqualTo(expected.Seed));
    Assert.That(preset.Mode, Is.EqualTo(expected.Mode));
  }

  [Test]
  public void WithMode_PreservesOtherSettings() {
    var start = new NoiseDitherer(NoiseType.Blue, 0.3f, 99);
    var switched = start.WithMode(NoiseMode.AdditivePerturb);

    Assert.That(switched.NoiseType, Is.EqualTo(NoiseType.Blue));
    Assert.That(switched.Strength, Is.EqualTo(0.3f));
    Assert.That(switched.Seed, Is.EqualTo(99));
    Assert.That(switched.Mode, Is.EqualTo(NoiseMode.AdditivePerturb));
  }
}
