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

using System.Collections.Generic;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.ColorProcessing.Storage;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Verifies that <see cref="AduQuantizer.Metric"/> (new in this revision) actually
/// influences the competitive-learning palette output — different metrics must yield
/// different palettes on the same histogram.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
[Category("AduMetric")]
public class AduQuantizerMetricTests {

  private static IEnumerable<(Bgra8888 color, uint count)> BuildDiverseHistogram() {
    // Dense spread across the RGB cube so different metrics produce different
    // winner-selection trajectories. Values chosen so Euclidean / Manhattan / Chebyshev
    // rankings of (input, candidate) pairs actually disagree for many inputs.
    var rng = new Random(123);
    for (var i = 0; i < 64; ++i) {
      var r = (byte)rng.Next(256);
      var g = (byte)rng.Next(256);
      var b = (byte)rng.Next(256);
      var count = (uint)rng.Next(10, 200);
      yield return (new Bgra8888(r, g, b), count);
    }
  }

  private static Bgra8888[] Reduce(AduMetric metric, int iterations = 50) {
    IQuantizer quantizer = new AduQuantizer(iterations, metric);
    return quantizer.CreateKernel<Bgra8888>().GeneratePalette(BuildDiverseHistogram(), colorCount: 16);
  }

  [Test]
  public void AduMetric_DefaultsToEuclidean() {
    IQuantizer defaultQuantizer = new AduQuantizer();
    IQuantizer euclidQuantizer = new AduQuantizer(iterationCount: 10, metric: AduMetric.Euclidean);
    var defaultPalette = defaultQuantizer.CreateKernel<Bgra8888>()
      .GeneratePalette(BuildDiverseHistogram(), 16);
    var explicitEuclidean = euclidQuantizer.CreateKernel<Bgra8888>()
      .GeneratePalette(BuildDiverseHistogram(), 16);
    Assert.That(explicitEuclidean, Is.EqualTo(defaultPalette),
      "Default AduQuantizer must be equivalent to explicit Euclidean metric");
  }

  [Test]
  public void AduMetric_Manhattan_ProducesDifferentPaletteThanEuclidean() {
    var euclid = Reduce(AduMetric.Euclidean);
    var manhattan = Reduce(AduMetric.Manhattan);
    Assert.That(manhattan, Is.Not.EqualTo(euclid),
      "Changing AduMetric must influence the resulting palette");
  }

  [Test]
  public void AduMetric_Chebyshev_ProducesDifferentPaletteThanEuclidean() {
    var euclid = Reduce(AduMetric.Euclidean);
    var cheby = Reduce(AduMetric.Chebyshev);
    Assert.That(cheby, Is.Not.EqualTo(euclid),
      "Changing AduMetric must influence the resulting palette");
  }
}
