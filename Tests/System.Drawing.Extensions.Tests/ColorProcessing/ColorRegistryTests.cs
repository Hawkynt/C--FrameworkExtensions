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
using Hawkynt.Drawing.ColorDomain;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ColorDomain")]
public class ColorRegistryTests {

  #region Ditherer registry

  [Test]
  public void DithererRegistry_FindByExactName() {
    var d = ColorDithererRegistry.FindByName("ErrorDiffusion_FloydSteinberg");
    Assert.That(d, Is.Not.Null);
    Assert.That(d, Is.InstanceOf<ColorDithererAdapter>());
  }

  [Test]
  public void DithererRegistry_FindBySuffix() {
    // "FloydSteinberg" should suffix-match "ErrorDiffusion_FloydSteinberg" exactly once.
    var d = ColorDithererRegistry.FindByName("FloydSteinberg");
    Assert.That(d, Is.Not.Null);
  }

  [Test]
  public void DithererRegistry_UnknownName_ReturnsNull() {
    Assert.That(ColorDithererRegistry.FindByName("DefinitelyNotAdithererXYZ"), Is.Null);
  }

  [Test]
  public void DithererRegistry_AmbiguousSuffix_Throws() {
    // "Bayer8x8" appears under multiple types (Ordered, Barycentric, Tin, NaturalNeighbour).
    Assert.Throws<ArgumentException>(() => ColorDithererRegistry.FindByName("Bayer8x8"));
  }

  [Test]
  public void DithererRegistry_AllExposesAtLeastOneEntry() {
    Assert.That(ColorDithererRegistry.All.Any(), Is.True);
  }

  #endregion

  #region Quantizer registry

  [Test]
  public void QuantizerRegistry_FindByExactName() {
    var q = ColorQuantizerRegistry.FindByName("Octree");
    Assert.That(q, Is.Not.Null);
    Assert.That(q, Is.InstanceOf<ColorQuantizerAdapter>());
  }

  [Test]
  public void QuantizerRegistry_FindByDisplayNameWithSpace() {
    var q = ColorQuantizerRegistry.FindByName("Median Cut");
    Assert.That(q, Is.Not.Null);
  }

  [Test]
  public void QuantizerRegistry_PassesAllowFillingColorsThrough() {
    var q = ColorQuantizerRegistry.FindByName("Octree", allowFillingColors: false) as ColorQuantizerAdapter;
    Assert.That(q, Is.Not.Null);
    Assert.That(q!.AllowFillingColors, Is.False);
  }

  [Test]
  public void QuantizerRegistry_UnknownName_ReturnsNull() {
    Assert.That(ColorQuantizerRegistry.FindByName("NoSuchQuantizer"), Is.Null);
  }

  #endregion
}
