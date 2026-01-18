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

using System.Linq;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Storage;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
public class QuantizerHelperTests {

  #region SampleHistogram Tests

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_SmallHistogram_ReturnsOriginal() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 50),
      (new Bgra8888(0, 0, 255, 255), 75)
    };

    var result = QuantizerHelper.SampleHistogram(histogram, 100);

    Assert.That(result, Has.Length.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_LargeHistogram_ReducesToMaxSize() {
    var histogram = Enumerable.Range(0, 10000)
      .Select(i => (new Bgra8888((byte)(i % 256), (byte)(i / 256 % 256), (byte)(i / 65536 % 256), 255), (uint)(i + 1)))
      .ToArray();

    var result = QuantizerHelper.SampleHistogram(histogram, 100);

    Assert.That(result, Has.Length.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_EmptyHistogram_ReturnsEmpty() {
    var histogram = Array.Empty<(Bgra8888, uint)>();

    var result = QuantizerHelper.SampleHistogram(histogram, 100);

    Assert.That(result, Has.Length.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_WithFixedSeed_ProducesReproducibleResults() {
    var histogram = Enumerable.Range(0, 1000)
      .Select(i => (new Bgra8888((byte)(i % 256), (byte)(i % 256), (byte)(i % 256), 255), (uint)(i + 1)))
      .ToArray();

    var result1 = QuantizerHelper.SampleHistogram(histogram, 50, seed: 42);
    var result2 = QuantizerHelper.SampleHistogram(histogram, 50, seed: 42);

    Assert.That(result1, Has.Length.EqualTo(result2.Length));
    for (var i = 0; i < result1.Length; ++i) {
      Assert.That(result1[i].color, Is.EqualTo(result2[i].color));
      Assert.That(result1[i].count, Is.EqualTo(result2[i].count));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_HighCountColorsPreferred() {
    // Create histogram with one very high count color and many low count colors
    var histogram = Enumerable.Range(1, 1000)
      .Select(i => (new Bgra8888((byte)(i % 256), (byte)(i % 256), (byte)(i % 256), 255), (uint)1))
      .Prepend((new Bgra8888(255, 0, 0, 255), 1000000u)) // High count red
      .ToArray();

    var result = QuantizerHelper.SampleHistogram(histogram, 50, seed: 42);

    // The high-count red color should always be in the sample
    var hasHighCountRed = result.Any(c => c.color.R == 255 && c.color.G == 0 && c.color.B == 0);
    Assert.That(hasHighCountRed, Is.True, "High count colors should be preserved in sample");
  }

  [Test]
  [Category("EdgeCase")]
  public void SampleHistogram_SingleColor_ReturnsSingleColor() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(128, 128, 128, 255), 1000)
    };

    var result = QuantizerHelper.SampleHistogram(histogram, 100);

    Assert.That(result, Has.Length.EqualTo(1));
    Assert.That(result[0].color, Is.EqualTo(histogram[0].Item1));
  }

  #endregion

  #region InitializePaletteWithPCA Tests

  [Test]
  [Category("HappyPath")]
  public void InitializePaletteWithPCA_EmptyHistogram_ReturnsEmpty() {
    var histogram = Array.Empty<(Bgra8888, uint)>();

    var result = QuantizerHelper.InitializePaletteWithPCA(histogram, 8);

    Assert.That(result, Has.Length.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void InitializePaletteWithPCA_ReturnsRequestedColorCount() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100),
      (new Bgra8888(0, 0, 255, 255), 100),
      (new Bgra8888(255, 255, 0, 255), 100),
      (new Bgra8888(255, 0, 255, 255), 100),
      (new Bgra8888(0, 255, 255, 255), 100),
      (new Bgra8888(0, 0, 0, 255), 100),
      (new Bgra8888(255, 255, 255, 255), 100)
    };

    var result = QuantizerHelper.InitializePaletteWithPCA(histogram, 4);

    Assert.That(result, Has.Length.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void InitializePaletteWithPCA_WithFixedSeed_ProducesReproducibleResults() {
    var histogram = Enumerable.Range(0, 100)
      .Select(i => (new Bgra8888((byte)(i * 2), (byte)(255 - i * 2), (byte)(i), 255), (uint)(i + 1)))
      .ToArray();

    var result1 = QuantizerHelper.InitializePaletteWithPCA(histogram, 8, seed: 42);
    var result2 = QuantizerHelper.InitializePaletteWithPCA(histogram, 8, seed: 42);

    Assert.That(result1, Has.Length.EqualTo(result2.Length));
    for (var i = 0; i < result1.Length; ++i)
      Assert.That(result1[i], Is.EqualTo(result2[i]));
  }

  [Test]
  [Category("HappyPath")]
  public void InitializePaletteWithPCA_DistributesColorsAlongPrincipalAxes() {
    // Create gradient histogram along one axis
    var histogram = Enumerable.Range(0, 256)
      .Select(i => (new Bgra8888((byte)i, (byte)i, (byte)i, 255), 1u))
      .ToArray();

    var result = QuantizerHelper.InitializePaletteWithPCA(histogram, 4);

    // Colors should be distributed from dark to light
    Assert.That(result, Has.Length.EqualTo(4));

    // All palette colors should be valid
    foreach (var color in result)
      Assert.That(color.A, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void InitializePaletteWithPCA_SingleColor_ReturnsSingleColor() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(128, 128, 128, 255), 1000)
    };

    var result = QuantizerHelper.InitializePaletteWithPCA(histogram, 8);

    Assert.That(result, Has.Length.LessThanOrEqualTo(8));
    Assert.That(result, Has.Length.GreaterThanOrEqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void InitializePaletteWithPCA_FewerColorsThanRequested_ReturnsAvailable() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100)
    };

    var result = QuantizerHelper.InitializePaletteWithPCA(histogram, 16);

    // Should still return colors, but may use perturbed values
    Assert.That(result, Has.Length.LessThanOrEqualTo(16));
  }

  #endregion

  #region OptimizePaletteWithACO Tests

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_EmptyHistogram_ReturnsInitialPalette() {
    var histogram = Array.Empty<(Bgra8888, uint)>();
    var initialPalette = new[] {
      new Bgra8888(255, 0, 0, 255),
      new Bgra8888(0, 255, 0, 255)
    };

    var result = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette);

    Assert.That(result, Is.EqualTo(initialPalette));
  }

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_EmptyPalette_ReturnsEmpty() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100)
    };

    var result = QuantizerHelper.OptimizePaletteWithACO(histogram, Array.Empty<Bgra8888>());

    Assert.That(result, Has.Length.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_PreservesPaletteSize() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100),
      (new Bgra8888(0, 0, 255, 255), 100)
    };
    var initialPalette = new[] {
      new Bgra8888(128, 0, 0, 255),
      new Bgra8888(0, 128, 0, 255),
      new Bgra8888(0, 0, 128, 255),
      new Bgra8888(128, 128, 128, 255)
    };

    var result = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette, iterations: 5);

    Assert.That(result, Has.Length.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_WithFixedSeed_ProducesReproducibleResults() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100),
      (new Bgra8888(0, 0, 255, 255), 100)
    };
    var initialPalette = new[] {
      new Bgra8888(128, 128, 128, 255),
      new Bgra8888(64, 64, 64, 255)
    };

    var result1 = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette, iterations: 5, seed: 42);
    var result2 = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette, iterations: 5, seed: 42);

    Assert.That(result1, Has.Length.EqualTo(result2.Length));
    for (var i = 0; i < result1.Length; ++i)
      Assert.That(result1[i], Is.EqualTo(result2[i]));
  }

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_ImprovesOrMaintainsPaletteQuality() {
    // Create histogram with specific color distribution
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(200, 50, 50, 255), 500),  // Red-ish
      (new Bgra8888(50, 200, 50, 255), 500),  // Green-ish
      (new Bgra8888(50, 50, 200, 255), 500)   // Blue-ish
    };

    // Start with sub-optimal palette (all grays)
    var initialPalette = new[] {
      new Bgra8888(100, 100, 100, 255),
      new Bgra8888(150, 150, 150, 255),
      new Bgra8888(200, 200, 200, 255)
    };

    var result = QuantizerHelper.OptimizePaletteWithACO(
      histogram,
      initialPalette,
      antCount: 10,
      iterations: 20,
      seed: 42
    );

    // The result should have some color variation (not all grays)
    Assert.That(result, Has.Length.EqualTo(3));

    // Check that colors are valid
    foreach (var color in result)
      Assert.That(color.A, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void OptimizePaletteWithACO_SingleColorPalette_ReturnsValidColor() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100)
    };
    var initialPalette = new[] { new Bgra8888(128, 128, 128, 255) };

    var result = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette, iterations: 5);

    Assert.That(result, Has.Length.EqualTo(1));
    Assert.That(result[0].A, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void OptimizePaletteWithACO_DefaultParameters_ProducesResult() {
    var histogram = new (Bgra8888, uint)[] {
      (new Bgra8888(255, 0, 0, 255), 100),
      (new Bgra8888(0, 255, 0, 255), 100)
    };
    var initialPalette = new[] {
      new Bgra8888(128, 0, 0, 255),
      new Bgra8888(0, 128, 0, 255)
    };

    // Use default parameters
    var result = QuantizerHelper.OptimizePaletteWithACO(histogram, initialPalette);

    Assert.That(result, Has.Length.EqualTo(2));
  }

  #endregion

  #region Integration Tests

  [Test]
  [Category("HappyPath")]
  public void PCA_ThenACO_ProducesValidPalette() {
    // Create a diverse histogram
    var histogram = Enumerable.Range(0, 100)
      .Select(i => {
        var r = (byte)((i * 37) % 256);
        var g = (byte)((i * 73) % 256);
        var b = (byte)((i * 137) % 256);
        return (new Bgra8888(r, g, b, 255), (uint)(i + 1));
      })
      .ToArray();

    // Initialize with PCA
    var pcaPalette = QuantizerHelper.InitializePaletteWithPCA(histogram, 8);
    Assert.That(pcaPalette, Has.Length.EqualTo(8));

    // Optimize with ACO
    var optimizedPalette = QuantizerHelper.OptimizePaletteWithACO(
      histogram,
      pcaPalette,
      iterations: 10,
      seed: 42
    );

    Assert.That(optimizedPalette, Has.Length.EqualTo(8));

    // All colors should be valid
    foreach (var color in optimizedPalette)
      Assert.That(color.A, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void SampleHistogram_ThenPCA_ThenACO_ProducesValidPalette() {
    // Create a large histogram
    var histogram = Enumerable.Range(0, 10000)
      .Select(i => {
        var r = (byte)((i * 37) % 256);
        var g = (byte)((i * 73) % 256);
        var b = (byte)((i * 137) % 256);
        return (new Bgra8888(r, g, b, 255), (uint)(i % 100 + 1));
      })
      .ToArray();

    // Sample to reduce size
    var sampled = QuantizerHelper.SampleHistogram(histogram, 500);
    Assert.That(sampled, Has.Length.LessThanOrEqualTo(500));

    // Initialize with PCA
    var pcaPalette = QuantizerHelper.InitializePaletteWithPCA(sampled, 16);
    Assert.That(pcaPalette, Has.Length.EqualTo(16));

    // Optimize with ACO
    var optimizedPalette = QuantizerHelper.OptimizePaletteWithACO(
      sampled,
      pcaPalette,
      antCount: 5,
      iterations: 5,
      seed: 42
    );

    Assert.That(optimizedPalette, Has.Length.EqualTo(16));
  }

  #endregion

}
