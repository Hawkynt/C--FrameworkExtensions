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

using NUnit.Framework;

namespace System.Drawing.Tests;

[TestFixture]
[Category("Unit")]
[Category("System.Drawing")]
[Category("RectangleF")]
public class RectangleFExtensionsTests {

  private const float Tolerance = 0.0001f;

  #region MultiplyBy Single Int Factor Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_IntFactor_ScalesAllComponents() {
    var rect = new RectangleF(10.5f, 20.5f, 30.5f, 40.5f);
    var scaled = rect.MultiplyBy(2);

    Assert.That(scaled.X, Is.EqualTo(21f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(41f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(61f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(81f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_IntFactorOfOne_ReturnsSameValues() {
    var rect = new RectangleF(5.5f, 10.5f, 15.5f, 20.5f);
    var scaled = rect.MultiplyBy(1);

    Assert.That(scaled.X, Is.EqualTo(rect.X).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(rect.Y).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(rect.Width).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(rect.Height).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MultiplyBy_IntFactorOfZero_ReturnsZeroRect() {
    var rect = new RectangleF(10.5f, 20.5f, 30.5f, 40.5f);
    var scaled = rect.MultiplyBy(0);

    Assert.That(scaled.X, Is.EqualTo(0f));
    Assert.That(scaled.Y, Is.EqualTo(0f));
    Assert.That(scaled.Width, Is.EqualTo(0f));
    Assert.That(scaled.Height, Is.EqualTo(0f));
  }

  #endregion

  #region MultiplyBy Single Float Factor Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_FloatFactor_ScalesAllComponents() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(1.5f);

    Assert.That(scaled.X, Is.EqualTo(15f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(30f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(45f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(60f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_FloatFractionFactor_ReducesSize() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(0.5f);

    Assert.That(scaled.X, Is.EqualTo(5f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(10f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(15f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(20f).Within(Tolerance));
  }

  #endregion

  #region MultiplyBy Two Int Factors Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_TwoIntFactors_ScalesXYSeparately() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(2, 3);

    Assert.That(scaled.X, Is.EqualTo(20f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(60f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(60f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(120f).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void MultiplyBy_TwoIntFactors_OnlyXZero_ZeroesXComponents() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(0, 2);

    Assert.That(scaled.X, Is.EqualTo(0f));
    Assert.That(scaled.Width, Is.EqualTo(0f));
    Assert.That(scaled.Y, Is.EqualTo(40f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(80f).Within(Tolerance));
  }

  #endregion

  #region MultiplyBy Two Float Factors Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_TwoFloatFactors_ScalesXYSeparately() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(1.5f, 2.5f);

    Assert.That(scaled.X, Is.EqualTo(15f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(50f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(45f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(100f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_TwoFloatFactors_FractionalValues_Works() {
    var rect = new RectangleF(10f, 20f, 30f, 40f);
    var scaled = rect.MultiplyBy(0.25f, 0.5f);

    Assert.That(scaled.X, Is.EqualTo(2.5f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(10f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(7.5f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(20f).Within(Tolerance));
  }

  #endregion

  #region Center Tests

  [Test]
  [Category("HappyPath")]
  public void Center_SquareRectangle_ReturnsCorrectCenter() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(10f).Within(Tolerance));
    Assert.That(center.Y, Is.EqualTo(10f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_WithOffset_IncludesOffset() {
    var rect = new RectangleF(10f, 20f, 20f, 40f);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(20f).Within(Tolerance));
    Assert.That(center.Y, Is.EqualTo(40f).Within(Tolerance));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_FractionalDimensions_ReturnsPreciseCenter() {
    var rect = new RectangleF(0.5f, 1.5f, 10.5f, 20.5f);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(5.75f).Within(Tolerance));
    Assert.That(center.Y, Is.EqualTo(11.75f).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void Center_ZeroSize_ReturnsOrigin() {
    var rect = new RectangleF(5.5f, 10.5f, 0f, 0f);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(5.5f).Within(Tolerance));
    Assert.That(center.Y, Is.EqualTo(10.5f).Within(Tolerance));
  }

  #endregion

  #region CollidesWith Rectangle Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_Overlapping_ReturnsTrue() {
    var rectF = new RectangleF(0f, 0f, 20f, 20f);
    var rect = new Rectangle(10, 10, 20, 20);

    Assert.That(rectF.CollidesWith(rect), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_NonOverlapping_ReturnsFalse() {
    var rectF = new RectangleF(0f, 0f, 10f, 10f);
    var rect = new Rectangle(20, 20, 10, 10);

    Assert.That(rectF.CollidesWith(rect), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_Touching_ReturnsTrue() {
    var rectF = new RectangleF(0f, 0f, 10f, 10f);
    var rect = new Rectangle(10, 0, 10, 10);

    Assert.That(rectF.CollidesWith(rect), Is.True);
  }

  #endregion

  #region CollidesWith RectangleF Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_RectangleF_Overlapping_ReturnsTrue() {
    var rect1 = new RectangleF(0f, 0f, 20f, 20f);
    var rect2 = new RectangleF(15.5f, 15.5f, 10f, 10f);

    Assert.That(rect1.CollidesWith(rect2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_RectangleF_NonOverlapping_ReturnsFalse() {
    var rect1 = new RectangleF(0f, 0f, 10f, 10f);
    var rect2 = new RectangleF(20.5f, 20.5f, 10f, 10f);

    Assert.That(rect1.CollidesWith(rect2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_RectangleF_FractionalTouching_ReturnsTrue() {
    var rect1 = new RectangleF(0f, 0f, 10.5f, 10.5f);
    var rect2 = new RectangleF(10.5f, 0f, 10f, 10f);

    Assert.That(rect1.CollidesWith(rect2), Is.True);
  }

  #endregion

  #region CollidesWith Point Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_Inside_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new Point(10, 10);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_Outside_ReturnsFalse() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new Point(30, 30);

    Assert.That(rect.CollidesWith(point), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_OnEdge_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new Point(0, 10);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_IntCoordinates_Inside_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);

    Assert.That(rect.CollidesWith(10, 10), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_IntCoordinates_Outside_ReturnsFalse() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);

    Assert.That(rect.CollidesWith(30, 30), Is.False);
  }

  #endregion

  #region CollidesWith PointF Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_PointF_Inside_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new PointF(10.5f, 10.5f);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_PointF_Outside_ReturnsFalse() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new PointF(20.1f, 20.1f);

    Assert.That(rect.CollidesWith(point), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_PointF_OnExactBoundary_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var point = new PointF(20f, 20f);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_FloatCoordinates_Inside_ReturnsTrue() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);

    Assert.That(rect.CollidesWith(10.5f, 10.5f), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_FloatCoordinates_Outside_ReturnsFalse() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);

    Assert.That(rect.CollidesWith(20.1f, 20.1f), Is.False);
  }

  #endregion

  #region Round Tests

  [Test]
  [Category("HappyPath")]
  public void Round_FractionalValues_RoundsToNearest() {
    var rectF = new RectangleF(1.4f, 2.6f, 10.5f, 20.4f);
    var rect = rectF.Round();

    Assert.That(rect.Left, Is.EqualTo(1));
    Assert.That(rect.Top, Is.EqualTo(3));
    // Right = Left + Width -> 1.4 + 10.5 = 11.9 -> Round = 12
    Assert.That(rect.Right, Is.EqualTo(12));
    // Bottom = Top + Height -> 2.6 + 20.4 = 23.0 -> Round = 23
    Assert.That(rect.Bottom, Is.EqualTo(23));
  }

  [Test]
  [Category("HappyPath")]
  public void Round_ExactHalf_RoundsToEven() {
    var rectF = new RectangleF(0.5f, 1.5f, 9.5f, 8.5f);
    var rect = rectF.Round();

    Assert.That(rect.Left, Is.EqualTo(0).Or.EqualTo(1)); // 0.5 rounds to 0 (banker's rounding)
    Assert.That(rect.Top, Is.EqualTo(2).Or.EqualTo(1)); // 1.5 rounds to 2 (banker's rounding)
  }

  [Test]
  [Category("HappyPath")]
  public void Round_WholeNumbers_ReturnsSameValues() {
    var rectF = new RectangleF(5f, 10f, 20f, 30f);
    var rect = rectF.Round();

    Assert.That(rect.Left, Is.EqualTo(5));
    Assert.That(rect.Top, Is.EqualTo(10));
    Assert.That(rect.Right, Is.EqualTo(25));
    Assert.That(rect.Bottom, Is.EqualTo(40));
  }

  #endregion

  #region Ceiling Tests

  [Test]
  [Category("HappyPath")]
  public void Ceiling_FractionalValues_CeilsUp() {
    var rectF = new RectangleF(1.1f, 2.1f, 9.9f, 19.9f);
    var rect = rectF.Ceiling();

    Assert.That(rect.Left, Is.EqualTo(2));
    Assert.That(rect.Top, Is.EqualTo(3));
    // Right = 1.1 + 9.9 = 11.0 -> Ceiling = 11
    Assert.That(rect.Right, Is.EqualTo(11));
    // Bottom = 2.1 + 19.9 = 22.0 -> Ceiling = 22
    Assert.That(rect.Bottom, Is.EqualTo(22));
  }

  [Test]
  [Category("HappyPath")]
  public void Ceiling_SmallFractions_StillCeilsUp() {
    var rectF = new RectangleF(1.0001f, 2.0001f, 9.0001f, 19.0001f);
    var rect = rectF.Ceiling();

    Assert.That(rect.Left, Is.EqualTo(2));
    Assert.That(rect.Top, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Ceiling_WholeNumbers_ReturnsSameValues() {
    var rectF = new RectangleF(5f, 10f, 20f, 30f);
    var rect = rectF.Ceiling();

    Assert.That(rect.Left, Is.EqualTo(5));
    Assert.That(rect.Top, Is.EqualTo(10));
    Assert.That(rect.Right, Is.EqualTo(25));
    Assert.That(rect.Bottom, Is.EqualTo(40));
  }

  #endregion

  #region Floor Tests

  [Test]
  [Category("HappyPath")]
  public void Floor_FractionalValues_FloorsDown() {
    var rectF = new RectangleF(1.9f, 2.9f, 9.1f, 19.1f);
    var rect = rectF.Floor();

    Assert.That(rect.Left, Is.EqualTo(1));
    Assert.That(rect.Top, Is.EqualTo(2));
    // Right = 1.9 + 9.1 = 11.0 -> Floor = 11
    Assert.That(rect.Right, Is.EqualTo(11));
    // Bottom = 2.9 + 19.1 = 22.0 -> Floor = 22
    Assert.That(rect.Bottom, Is.EqualTo(22));
  }

  [Test]
  [Category("HappyPath")]
  public void Floor_SmallFractions_StillFloorsDown() {
    var rectF = new RectangleF(1.9999f, 2.9999f, 9.9999f, 19.9999f);
    var rect = rectF.Floor();

    Assert.That(rect.Left, Is.EqualTo(1));
    Assert.That(rect.Top, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Floor_WholeNumbers_ReturnsSameValues() {
    var rectF = new RectangleF(5f, 10f, 20f, 30f);
    var rect = rectF.Floor();

    Assert.That(rect.Left, Is.EqualTo(5));
    Assert.That(rect.Top, Is.EqualTo(10));
    Assert.That(rect.Right, Is.EqualTo(25));
    Assert.That(rect.Bottom, Is.EqualTo(40));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void MultiplyBy_NegativeFactor_FlipsRectangle() {
    var rect = new RectangleF(10f, 10f, 20f, 20f);
    var scaled = rect.MultiplyBy(-1f);

    Assert.That(scaled.X, Is.EqualTo(-10f).Within(Tolerance));
    Assert.That(scaled.Y, Is.EqualTo(-10f).Within(Tolerance));
    Assert.That(scaled.Width, Is.EqualTo(-20f).Within(Tolerance));
    Assert.That(scaled.Height, Is.EqualTo(-20f).Within(Tolerance));
  }

  [Test]
  [Category("EdgeCase")]
  public void CollidesWith_EmptyRectangleF_ChecksPointLocation() {
    var rect = new RectangleF(0f, 0f, 20f, 20f);
    var emptyInside = new RectangleF(5f, 5f, 0f, 0f);
    var emptyOutside = new RectangleF(25f, 25f, 0f, 0f);

    Assert.That(rect.CollidesWith(emptyInside), Is.True);
    Assert.That(rect.CollidesWith(emptyOutside), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Round_NegativeCoordinates_WorksCorrectly() {
    var rectF = new RectangleF(-10.5f, -20.5f, 5.5f, 10.5f);
    var rect = rectF.Round();

    Assert.That(rect.Left, Is.EqualTo(-10).Or.EqualTo(-11));
    Assert.That(rect.Top, Is.EqualTo(-20).Or.EqualTo(-21));
  }

  [Test]
  [Category("EdgeCase")]
  public void Floor_NegativeCoordinates_FloorsAwayFromZero() {
    var rectF = new RectangleF(-10.5f, -20.5f, 5f, 10f);
    var rect = rectF.Floor();

    Assert.That(rect.Left, Is.EqualTo(-11));
    Assert.That(rect.Top, Is.EqualTo(-21));
  }

  [Test]
  [Category("EdgeCase")]
  public void Ceiling_NegativeCoordinates_CeilsTowardZero() {
    var rectF = new RectangleF(-10.5f, -20.5f, 5f, 10f);
    var rect = rectF.Ceiling();

    Assert.That(rect.Left, Is.EqualTo(-10));
    Assert.That(rect.Top, Is.EqualTo(-20));
  }

  #endregion
}
