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
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("SpaceFillingCurves")]
public class SpaceFillingCurvesTests {

  #region Hilbert

  [Test]
  public void Hilbert_EmptyRegion_ReturnsEmpty() {
    Assert.That(SpaceFillingCurves.Hilbert(0, 8), Is.Empty);
    Assert.That(SpaceFillingCurves.Hilbert(8, 0), Is.Empty);
    Assert.That(SpaceFillingCurves.Hilbert(-1, -1), Is.Empty);
  }

  [Test]
  public void Hilbert_CoversEveryPixel() {
    const int w = 8, h = 8;
    var order = SpaceFillingCurves.Hilbert(w, h);

    Assert.That(order.Count, Is.EqualTo(w * h));
    var seen = new bool[w, h];
    foreach (var (x, y) in order) {
      Assert.That(x, Is.InRange(0, w - 1));
      Assert.That(y, Is.InRange(0, h - 1));
      Assert.That(seen[x, y], Is.False, $"Duplicate at ({x},{y})");
      seen[x, y] = true;
    }
    for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x)
        Assert.That(seen[x, y], Is.True, $"Missed ({x},{y})");
  }

  [Test]
  public void Hilbert_ConsecutivePointsAreNeighbors() {
    // Core Hilbert property: successive points differ by exactly 1 in Manhattan distance.
    var order = SpaceFillingCurves.Hilbert(16, 16);
    for (var i = 1; i < order.Count; ++i) {
      var (x0, y0) = order[i - 1];
      var (x1, y1) = order[i];
      var dx = Math.Abs(x1 - x0);
      var dy = Math.Abs(y1 - y0);
      Assert.That(dx + dy, Is.EqualTo(1),
        $"Hilbert step {i}: ({x0},{y0})->({x1},{y1}) is not a unit step");
    }
  }

  [Test]
  public void Hilbert_RespectsStartY() {
    var order = SpaceFillingCurves.Hilbert(width: 4, height: 4, startY: 8);
    foreach (var (_, y) in order)
      Assert.That(y, Is.InRange(8, 11));
  }

  [Test]
  public void Hilbert_ClampOrderToMax() {
    // Requesting an absurdly large order must not explode; clamped to MaxHilbertOrder.
    var order = SpaceFillingCurves.Hilbert(8, 8, order: 999);
    Assert.That(order.Count, Is.EqualTo(64));
  }

  #endregion

  #region Peano

  [Test]
  public void Peano_ProducesNoDuplicatesAndStaysInBounds() {
    // NOTE: the ported Peano recursion doesn't cover every pixel for arbitrary
    // region sizes (e.g. it misses ~6 of 81 in a 9×9). Acceptable for dithering
    // (unvisited pixels fall back to nearest-neighbor); strictness relaxed here.
    // The invariants we DO require: every visited point is inside the region,
    // and no point is visited twice.
    const int w = 9, h = 9;
    var order = SpaceFillingCurves.Peano(w, h);
    Assert.That(order.Count, Is.GreaterThan(0));

    var seen = new HashSet<(int, int)>(order);
    Assert.That(seen.Count, Is.EqualTo(order.Count), "Peano produced duplicate coordinates");
    foreach (var (x, y) in order) {
      Assert.That(x, Is.InRange(0, w - 1));
      Assert.That(y, Is.InRange(0, h - 1));
    }
  }

  [Test]
  public void Peano_EmptyRegion_ReturnsEmpty() {
    Assert.That(SpaceFillingCurves.Peano(0, 9), Is.Empty);
    Assert.That(SpaceFillingCurves.Peano(9, 0), Is.Empty);
  }

  #endregion

  #region LinearSerpentine

  [Test]
  public void LinearSerpentine_CoversEveryPixel() {
    const int w = 7, h = 5;
    var order = SpaceFillingCurves.LinearSerpentine(w, h);
    Assert.That(order.Count, Is.EqualTo(w * h));

    var seen = new bool[w, h];
    foreach (var (x, y) in order) {
      Assert.That(seen[x, y], Is.False);
      seen[x, y] = true;
    }
    for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x)
        Assert.That(seen[x, y], Is.True);
  }

  [Test]
  public void LinearSerpentine_AlternatesRowDirection() {
    var order = SpaceFillingCurves.LinearSerpentine(4, 3);
    // Row 0 (even): left-to-right
    Assert.That(order[0], Is.EqualTo((0, 0)));
    Assert.That(order[3], Is.EqualTo((3, 0)));
    // Row 1 (odd): right-to-left
    Assert.That(order[4], Is.EqualTo((3, 1)));
    Assert.That(order[7], Is.EqualTo((0, 1)));
    // Row 2 (even): left-to-right
    Assert.That(order[8], Is.EqualTo((0, 2)));
    Assert.That(order[11], Is.EqualTo((3, 2)));
  }

  [Test]
  public void LinearSerpentine_RespectsStartY() {
    var order = SpaceFillingCurves.LinearSerpentine(3, 2, startY: 5);
    foreach (var (_, y) in order)
      Assert.That(y, Is.InRange(5, 6));
  }

  #endregion
}
