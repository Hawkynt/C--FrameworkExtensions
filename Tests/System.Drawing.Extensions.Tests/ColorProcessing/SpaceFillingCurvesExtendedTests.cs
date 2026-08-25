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
using System.Linq;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Dithering;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("SpaceFillingCurves")]
public class SpaceFillingCurvesExtendedTests {

  [Test]
  public void Moore_FullSquare_IsClosedAndManhattanContinuous() {
    const int order = 4;
    const int size = 1 << order;
    var curve = SpaceFillingCurves.Moore(size, size, order: order);

    _AssertCoversRectangle(curve, size, size);
    _AssertManhattanContinuous(curve);

    var first = curve[0];
    var last = curve[curve.Count - 1];
    Assert.That(_ManhattanDistance(first, last), Is.EqualTo(1), "Moore loop must close with a unit step");
  }

  [Test]
  public void Moore_ClippedRectangle_StillCoversEveryPixelExactlyOnce() {
    var curve = SpaceFillingCurves.Moore(11, 7);
    _AssertCoversRectangle(curve, 11, 7);
  }

  [TestCase(2, 7)]
  [TestCase(7, 2)]
  [TestCase(10, 7)]
  [TestCase(13, 10)]
  [TestCase(17, 13)]
  public void Gilbert_ArbitraryRectangle_CoversEveryPixelWithLocalSteps(int width, int height) {
    var curve = SpaceFillingCurves.Gilbert(width, height);

    _AssertCoversRectangle(curve, width, height);
    for (var i = 1; i < curve.Count; ++i) {
      var dx = Math.Abs(curve[i].x - curve[i - 1].x);
      var dy = Math.Abs(curve[i].y - curve[i - 1].y);
      Assert.That(Math.Max(dx, dy), Is.EqualTo(1), $"Gilbert step {i} jumped by ({dx},{dy})");
    }
  }

  [Test]
  public void Gilbert_EvenMajorDimension_IsManhattanContinuous() {
    var curve = SpaceFillingCurves.Gilbert(10, 7);
    _AssertManhattanContinuous(curve);
  }

  [TestCase(SpaceFillingCurve.Peano)]
  [TestCase(SpaceFillingCurve.Coil)]
  [TestCase(SpaceFillingCurve.HalfCoil)]
  [TestCase(SpaceFillingCurve.Meurthe)]
  public void PeanoFamily_Order3_CoversSquareAndIsManhattanContinuous(SpaceFillingCurve curveType) {
    const int order = 3;
    const int size = 27;
    var curve = _Generate(curveType, size, size, 0, order);

    _AssertCoversRectangle(curve, size, size);
    _AssertManhattanContinuous(curve);
  }

  [Test]
  public void PeanoFamily_Order2_VariantsProduceDifferentTraversals() {
    var curves = new[] {
      SpaceFillingCurves.Peano(9, 9, order: 2),
      SpaceFillingCurves.Coil(9, 9, order: 2),
      SpaceFillingCurves.HalfCoil(9, 9, order: 2),
      SpaceFillingCurves.Meurthe(9, 9, order: 2)
    };

    for (var i = 0; i < curves.Length; ++i)
      for (var j = i + 1; j < curves.Length; ++j)
        Assert.That(curves[i].SequenceEqual(curves[j]), Is.False, $"Variants {i} and {j} unexpectedly match");
  }

  [Test]
  public void Morton_Order1_HasCanonicalZOrder() {
    var curve = SpaceFillingCurves.Morton(2, 2, order: 1);
    Assert.That(curve, Is.EqualTo(new[] { (0, 0), (1, 0), (0, 1), (1, 1) }));
  }

  [Test]
  public void Morton_Rectangle_CoversEveryPixelButContainsJumps() {
    var curve = SpaceFillingCurves.Morton(8, 8);
    _AssertCoversRectangle(curve, 8, 8);

    var hasJump = false;
    for (var i = 1; i < curve.Count; ++i)
      hasJump |= _ManhattanDistance(curve[i - 1], curve[i]) > 1;

    Assert.That(hasJump, Is.True, "Morton is intentionally not a continuous path");
  }

  [Test]
  public void Spiral_ThreeByThree_HasExpectedClockwiseOrder() {
    var curve = SpaceFillingCurves.Spiral(3, 3);
    var expected = new[] {
      (0, 0), (1, 0), (2, 0),
      (2, 1), (2, 2), (1, 2),
      (0, 2), (0, 1), (1, 1)
    };

    Assert.That(curve, Is.EqualTo(expected));
    _AssertManhattanContinuous(curve);
  }

  [Test]
  public void Spiral_NonSquareRegionWithStartY_CoversAndStaysContinuous() {
    var curve = SpaceFillingCurves.Spiral(7, 4, startY: 5);
    _AssertCoversRectangle(curve, 7, 4, startY: 5);
    _AssertManhattanContinuous(curve);
  }

  [Test]
  public void DiagonalSerpentine_FourByFour_HasExpectedPrefixAndDiagonalSteps() {
    var curve = SpaceFillingCurves.DiagonalSerpentine(4, 4);
    _AssertCoversRectangle(curve, 4, 4);

    Assert.That(curve.Take(10), Is.EqualTo(new[] {
      (0, 0),
      (0, 1), (1, 0),
      (2, 0), (1, 1), (0, 2),
      (0, 3), (1, 2), (2, 1), (3, 0)
    }));

    var hasDiagonalStep = false;
    for (var i = 1; i < curve.Count; ++i) {
      var dx = Math.Abs(curve[i].x - curve[i - 1].x);
      var dy = Math.Abs(curve[i].y - curve[i - 1].y);
      Assert.That(Math.Max(dx, dy), Is.EqualTo(1));
      hasDiagonalStep |= dx == 1 && dy == 1;
    }
    Assert.That(hasDiagonalStep, Is.True);
  }

  [TestCase(SpaceFillingCurve.Moore)]
  [TestCase(SpaceFillingCurve.Gilbert)]
  [TestCase(SpaceFillingCurve.Coil)]
  [TestCase(SpaceFillingCurve.HalfCoil)]
  [TestCase(SpaceFillingCurve.Meurthe)]
  [TestCase(SpaceFillingCurve.Morton)]
  [TestCase(SpaceFillingCurve.Spiral)]
  [TestCase(SpaceFillingCurve.DiagonalSerpentine)]
  public void NewCurves_EmptyRegion_ReturnsEmpty(SpaceFillingCurve curveType) {
    Assert.That(_Generate(curveType, 0, 8), Is.Empty);
    Assert.That(_Generate(curveType, 8, 0), Is.Empty);
  }

  private static List<(int x, int y)> _Generate(
    SpaceFillingCurve curveType,
    int width,
    int height,
    int startY = 0,
    int? order = null) {

    return curveType switch {
      SpaceFillingCurve.Hilbert => SpaceFillingCurves.Hilbert(width, height, startY, order),
      SpaceFillingCurve.Peano => SpaceFillingCurves.Peano(width, height, startY, order),
      SpaceFillingCurve.Linear => SpaceFillingCurves.LinearSerpentine(width, height, startY),
      SpaceFillingCurve.Moore => SpaceFillingCurves.Moore(width, height, startY, order),
      SpaceFillingCurve.Gilbert => SpaceFillingCurves.Gilbert(width, height, startY),
      SpaceFillingCurve.Coil => SpaceFillingCurves.Coil(width, height, startY, order),
      SpaceFillingCurve.HalfCoil => SpaceFillingCurves.HalfCoil(width, height, startY, order),
      SpaceFillingCurve.Meurthe => SpaceFillingCurves.Meurthe(width, height, startY, order),
      SpaceFillingCurve.Morton => SpaceFillingCurves.Morton(width, height, startY, order),
      SpaceFillingCurve.Spiral => SpaceFillingCurves.Spiral(width, height, startY),
      SpaceFillingCurve.DiagonalSerpentine => SpaceFillingCurves.DiagonalSerpentine(width, height, startY),
      _ => throw new AssertionException($"Unsupported curve type {curveType}")
    };
  }

  private static void _AssertCoversRectangle(
    IReadOnlyCollection<(int x, int y)> curve,
    int width,
    int height,
    int startY = 0) {

    Assert.That(curve.Count, Is.EqualTo(width * height));
    var seen = new HashSet<(int, int)>();
    foreach (var point in curve) {
      Assert.That(point.x, Is.InRange(0, width - 1));
      Assert.That(point.y, Is.InRange(startY, startY + height - 1));
      Assert.That(seen.Add(point), Is.True, $"Duplicate at ({point.x},{point.y})");
    }
  }

  private static void _AssertManhattanContinuous(IReadOnlyList<(int x, int y)> curve) {
    for (var i = 1; i < curve.Count; ++i)
      Assert.That(
        _ManhattanDistance(curve[i - 1], curve[i]),
        Is.EqualTo(1),
        $"Step {i}: {curve[i - 1]} -> {curve[i]} is not a unit Manhattan step"
      );
  }

  private static int _ManhattanDistance((int x, int y) a, (int x, int y) b)
    => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y)
    ;
}
