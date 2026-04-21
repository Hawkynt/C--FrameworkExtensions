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
using System.Drawing;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.ColorProcessing.Storage;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Verifies that the histogram-based quantization API is callable from outside the
/// assembly: <see cref="IQuantizer.CreateKernel{TWork}"/>, <see cref="IQuantizer{TWork}.GeneratePalette"/>,
/// and <see cref="PaletteFiller.GenerateFinalPalette{TWork}"/> are all public.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Quantization")]
[Category("PublicApi")]
public class PublicHistogramApiTests {

  private static IEnumerable<(Bgra8888 color, uint count)> BuildHistogram() {
    yield return (new Bgra8888(255, 0, 0), 100u);
    yield return (new Bgra8888(0, 255, 0), 80u);
    yield return (new Bgra8888(0, 0, 255), 60u);
    yield return (new Bgra8888(255, 255, 0), 40u);
    yield return (new Bgra8888(0, 255, 255), 30u);
    yield return (new Bgra8888(255, 0, 255), 20u);
    yield return (new Bgra8888(128, 128, 128), 10u);
    yield return (new Bgra8888(64, 64, 64), 5u);
  }

  [Test]
  public void IQuantizer_CreateKernel_IsCallableFromOutsideAssembly() {
    IQuantizer quantizer = new OctreeQuantizer();
    var kernel = quantizer.CreateKernel<Bgra8888>();
    Assert.That(kernel, Is.Not.Null);
  }

  [Test]
  public void IQuantizerKernel_GeneratePalette_ProducesRequestedColorCount() {
    IQuantizer quantizer = new WuQuantizer();
    var kernel = quantizer.CreateKernel<Bgra8888>();

    var palette = kernel.GeneratePalette(BuildHistogram(), colorCount: 4);

    Assert.That(palette, Is.Not.Null);
    Assert.That(palette.Length, Is.LessThanOrEqualTo(4));
    Assert.That(palette.Length, Is.GreaterThan(0));
  }

  [Test]
  public void PaletteFiller_GenerateFinalPalette_PadsToRequestedSize() {
    var proposed = new[] {
      new Bgra8888(255, 0, 0),
      new Bgra8888(0, 255, 0)
    };

    var filled = PaletteFiller.GenerateFinalPalette(proposed, colorCount: 10, allowFillingColors: true);

    Assert.That(filled.Length, Is.EqualTo(10), "Palette must be padded to exact requested size");
  }

  [Test]
  public void PaletteFiller_GenerateFinalPalette_WithoutFilling_LeavesSlotsTransparent() {
    var proposed = new[] { new Bgra8888(255, 0, 0) };

    var filled = PaletteFiller.GenerateFinalPalette(proposed, colorCount: 4, allowFillingColors: false);

    Assert.That(filled.Length, Is.EqualTo(4));
    Assert.That(filled[0], Is.EqualTo(new Bgra8888(255, 0, 0)));
    for (var i = 1; i < 4; ++i)
      Assert.That(filled[i].A, Is.EqualTo(0), $"slot {i} must be fully transparent when allowFillingColors=false");
  }

  [Test]
  public void HistogramApi_RoundTrip_ProducesDeterministicPalette() {
    IQuantizer quantizer = new MedianCutQuantizer();
    var kernel1 = quantizer.CreateKernel<Bgra8888>();
    var kernel2 = quantizer.CreateKernel<Bgra8888>();

    var p1 = kernel1.GeneratePalette(BuildHistogram(), colorCount: 5);
    var p2 = kernel2.GeneratePalette(BuildHistogram(), colorCount: 5);

    Assert.That(p2, Is.EqualTo(p1), "Deterministic quantizer must produce identical palette on repeat call");
  }
}
