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

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Dithering")]
[Category("Riemersma")]
public class RiemersmaExtendedCurveTests {

  [TestCase(SpaceFillingCurve.Moore)]
  [TestCase(SpaceFillingCurve.Gilbert)]
  [TestCase(SpaceFillingCurve.Coil)]
  [TestCase(SpaceFillingCurve.HalfCoil)]
  [TestCase(SpaceFillingCurve.Meurthe)]
  [TestCase(SpaceFillingCurve.Morton)]
  [TestCase(SpaceFillingCurve.Spiral)]
  [TestCase(SpaceFillingCurve.DiagonalSerpentine)]
  public void NewCurve_DithersNonSquareImage(SpaceFillingCurve curveType) {
    using var bitmap = TestUtilities.CreateGradientBitmap(17, 13, Color.Black, Color.White);
    var ditherer = new RiemersmaDitherer(16, curveType);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(17));
    Assert.That(result.Height, Is.EqualTo(13));
  }

  [TestCase(SpaceFillingCurve.Gilbert)]
  [TestCase(SpaceFillingCurve.Spiral)]
  [TestCase(SpaceFillingCurve.DiagonalSerpentine)]
  [TestCase(SpaceFillingCurve.Linear)]
  public void NonRecursiveCurves_IgnoreCurveOrder(SpaceFillingCurve curveType) {
    using var bitmap = TestUtilities.CreateGradientBitmap(11, 7, Color.Black, Color.White);
    var ditherer = new RiemersmaDitherer(16, curveType, curveOrder: 999);

    using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Width, Is.EqualTo(11));
    Assert.That(result.Height, Is.EqualTo(7));
  }

  [Test]
  public void ExistingEnumValues_RemainStable() {
    Assert.That((int)SpaceFillingCurve.Hilbert, Is.EqualTo(0));
    Assert.That((int)SpaceFillingCurve.Peano, Is.EqualTo(1));
    Assert.That((int)SpaceFillingCurve.Linear, Is.EqualTo(2));
  }

  [Test]
  public void NewPreconfiguredInstances_ProduceOutput() {
    using var bitmap = TestUtilities.CreateGradientBitmap(9, 9, Color.Black, Color.White);
    var ditherers = new[] {
      RiemersmaDitherer.Moore,
      RiemersmaDitherer.Gilbert,
      RiemersmaDitherer.Coil,
      RiemersmaDitherer.HalfCoil,
      RiemersmaDitherer.Meurthe,
      RiemersmaDitherer.Morton,
      RiemersmaDitherer.SpiralScan,
      RiemersmaDitherer.DiagonalScan
    };

    foreach (var ditherer in ditherers) {
      using var result = bitmap.ReduceColors(new MedianCutQuantizer(), ditherer, 4);
      Assert.That(result, Is.Not.Null);
      Assert.That(result.Width, Is.EqualTo(9));
      Assert.That(result.Height, Is.EqualTo(9));
    }
  }
}
