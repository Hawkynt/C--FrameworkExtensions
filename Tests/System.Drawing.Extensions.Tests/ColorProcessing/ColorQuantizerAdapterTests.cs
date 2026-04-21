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
using System.Linq;
using Hawkynt.ColorProcessing.Quantization;
using Hawkynt.Drawing.ColorDomain;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ColorDomain")]
public class ColorQuantizerAdapterTests {

  private static (Color color, uint count)[] BuildHistogram() => new[] {
    (Color.Red,    100u),
    (Color.Green,   80u),
    (Color.Blue,    60u),
    (Color.Yellow,  40u),
    (Color.Cyan,    30u),
    (Color.Magenta, 20u),
    (Color.Gray,    10u),
  };

  [Test]
  public void Adapter_ReducesToRequestedSize() {
    var quantizer = new ColorQuantizerAdapter(new OctreeQuantizer());
    var palette = quantizer.ReduceColorsTo(4, BuildHistogram());
    Assert.That(palette.Length, Is.EqualTo(4));
  }

  [Test]
  public void Adapter_PadsWhenAllowFillingColorsTrue() {
    var quantizer = new ColorQuantizerAdapter(new OctreeQuantizer(), allowFillingColors: true);
    var palette = quantizer.ReduceColorsTo(8, new[] { Color.Red });
    Assert.That(palette.Length, Is.EqualTo(8));
    var nonTransparent = palette.Count(c => c.A != 0);
    Assert.That(nonTransparent, Is.GreaterThan(1),
      "AllowFillingColors=true should pad with non-transparent palette entries");
  }

  [Test]
  public void Adapter_FillsTransparentWhenAllowFillingColorsFalse() {
    var quantizer = new ColorQuantizerAdapter(new OctreeQuantizer(), allowFillingColors: false);
    var palette = quantizer.ReduceColorsTo(8, new[] { Color.Red });
    Assert.That(palette.Length, Is.EqualTo(8));
    Assert.That(palette[0].ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
    for (var i = 1; i < palette.Length; ++i)
      Assert.That(palette[i].A, Is.EqualTo(0),
        $"AllowFillingColors=false: slot {i} must be fully transparent");
  }

  [Test]
  public void Adapter_EmptyInput_ReturnsEmpty() {
    var quantizer = new ColorQuantizerAdapter(new WuQuantizer());
    Assert.That(quantizer.ReduceColorsTo(8, Array.Empty<Color>()), Is.Empty);
  }

  [Test]
  public void Adapter_NumberOfColorsZero_ReturnsEmpty() {
    var quantizer = new ColorQuantizerAdapter(new WuQuantizer());
    Assert.That(quantizer.ReduceColorsTo(0, BuildHistogram()), Is.Empty);
  }

  [Test]
  public void Adapter_DeterministicForSameInput() {
    var quantizer = new ColorQuantizerAdapter(new MedianCutQuantizer());
    var p1 = quantizer.ReduceColorsTo(5, BuildHistogram());
    var p2 = quantizer.ReduceColorsTo(5, BuildHistogram());
    CollectionAssert.AreEqual(p1.Select(c => c.ToArgb()).ToArray(), p2.Select(c => c.ToArgb()).ToArray());
  }

  [Test]
  public void Adapter_NullInner_Throws() {
    Assert.Throws<ArgumentNullException>(() => _ = new ColorQuantizerAdapter(null!));
  }
}
