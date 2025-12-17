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
[Category("Rectangle")]
public class RectangleExtensionsTests {

  #region MultiplyBy Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_SingleFactor_ScalesAllComponents() {
    var rect = new Rectangle(10, 20, 30, 40);
    var scaled = rect.MultiplyBy(2);

    Assert.That(scaled.X, Is.EqualTo(20));
    Assert.That(scaled.Y, Is.EqualTo(40));
    Assert.That(scaled.Width, Is.EqualTo(60));
    Assert.That(scaled.Height, Is.EqualTo(80));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_TwoFactors_ScalesXYSeparately() {
    var rect = new Rectangle(10, 20, 30, 40);
    var scaled = rect.MultiplyBy(2, 3);

    Assert.That(scaled.X, Is.EqualTo(20));
    Assert.That(scaled.Y, Is.EqualTo(60));
    Assert.That(scaled.Width, Is.EqualTo(60));
    Assert.That(scaled.Height, Is.EqualTo(120));
  }

  [Test]
  [Category("HappyPath")]
  public void MultiplyBy_FactorOfOne_ReturnsSameValues() {
    var rect = new Rectangle(5, 10, 15, 20);
    var scaled = rect.MultiplyBy(1);

    Assert.That(scaled.X, Is.EqualTo(rect.X));
    Assert.That(scaled.Y, Is.EqualTo(rect.Y));
    Assert.That(scaled.Width, Is.EqualTo(rect.Width));
    Assert.That(scaled.Height, Is.EqualTo(rect.Height));
  }

  [Test]
  [Category("EdgeCase")]
  public void MultiplyBy_FactorOfZero_ReturnsZeroRect() {
    var rect = new Rectangle(10, 20, 30, 40);
    var scaled = rect.MultiplyBy(0);

    Assert.That(scaled.X, Is.EqualTo(0));
    Assert.That(scaled.Y, Is.EqualTo(0));
    Assert.That(scaled.Width, Is.EqualTo(0));
    Assert.That(scaled.Height, Is.EqualTo(0));
  }

  #endregion

  #region CollidesWith Rectangle Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_Overlapping_ReturnsTrue() {
    var rect1 = new Rectangle(0, 0, 20, 20);
    var rect2 = new Rectangle(10, 10, 20, 20);

    Assert.That(rect1.CollidesWith(rect2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_NonOverlapping_ReturnsFalse() {
    var rect1 = new Rectangle(0, 0, 10, 10);
    var rect2 = new Rectangle(20, 20, 10, 10);

    Assert.That(rect1.CollidesWith(rect2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_Touching_ReturnsTrue() {
    var rect1 = new Rectangle(0, 0, 10, 10);
    var rect2 = new Rectangle(10, 0, 10, 10);

    Assert.That(rect1.CollidesWith(rect2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Rectangle_Contained_OuterContainsInner() {
    var outer = new Rectangle(0, 0, 100, 100);
    var inner = new Rectangle(25, 25, 50, 50);

    // CollidesWith checks if OTHER's corners are inside THIS
    // outer.CollidesWith(inner): inner's corners (25,25,75,75) are inside outer - TRUE
    Assert.That(outer.CollidesWith(inner), Is.True);
    // inner.CollidesWith(outer): outer's corners (0,0,100,100) are NOT inside inner - FALSE
    Assert.That(inner.CollidesWith(outer), Is.False);
  }

  #endregion

  #region CollidesWith Point Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_Inside_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new Point(10, 10);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_Outside_ReturnsFalse() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new Point(30, 30);

    Assert.That(rect.CollidesWith(point), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_OnEdge_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new Point(0, 10);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_Point_OnCorner_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new Point(0, 0);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_IntCoordinates_Inside_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);

    Assert.That(rect.CollidesWith(10, 10), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_IntCoordinates_Outside_ReturnsFalse() {
    var rect = new Rectangle(0, 0, 20, 20);

    Assert.That(rect.CollidesWith(30, 30), Is.False);
  }

  #endregion

  #region CollidesWith PointF Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_PointF_Inside_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new PointF(10.5f, 10.5f);

    Assert.That(rect.CollidesWith(point), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_PointF_Outside_ReturnsFalse() {
    var rect = new Rectangle(0, 0, 20, 20);
    var point = new PointF(20.1f, 20.1f);

    Assert.That(rect.CollidesWith(point), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_FloatCoordinates_Inside_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);

    Assert.That(rect.CollidesWith(10.5f, 10.5f), Is.True);
  }

  #endregion

  #region CollidesWith RectangleF Tests

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_RectangleF_Overlapping_ReturnsTrue() {
    var rect = new Rectangle(0, 0, 20, 20);
    var rectF = new RectangleF(15.5f, 15.5f, 10, 10);

    Assert.That(rect.CollidesWith(rectF), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollidesWith_RectangleF_NonOverlapping_ReturnsFalse() {
    var rect = new Rectangle(0, 0, 10, 10);
    var rectF = new RectangleF(20.5f, 20.5f, 10, 10);

    Assert.That(rect.CollidesWith(rectF), Is.False);
  }

  #endregion

  #region Center Tests

  [Test]
  [Category("HappyPath")]
  public void Center_EvenDimensions_ReturnsCorrectCenter() {
    var rect = new Rectangle(0, 0, 20, 20);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(10));
    Assert.That(center.Y, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_OddDimensions_ReturnsFloorCenter() {
    var rect = new Rectangle(0, 0, 21, 21);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(10));
    Assert.That(center.Y, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_WithOffset_IncludesOffset() {
    var rect = new Rectangle(10, 20, 20, 20);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(20));
    Assert.That(center.Y, Is.EqualTo(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void Center_ZeroSize_ReturnsOrigin() {
    var rect = new Rectangle(5, 10, 0, 0);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(5));
    Assert.That(center.Y, Is.EqualTo(10));
  }

  #endregion

  #region SetLeft/Right/Top/Bottom Tests

  [Test]
  [Category("HappyPath")]
  public void SetLeft_MovesLeftEdge() {
    var rect = new Rectangle(10, 10, 20, 20);
    var modified = rect.SetLeft(5);

    Assert.That(modified.Left, Is.EqualTo(5));
    Assert.That(modified.Right, Is.EqualTo(30));
    Assert.That(modified.Top, Is.EqualTo(10));
    Assert.That(modified.Bottom, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void SetRight_MovesRightEdge() {
    var rect = new Rectangle(10, 10, 20, 20);
    var modified = rect.SetRight(50);

    Assert.That(modified.Left, Is.EqualTo(10));
    Assert.That(modified.Right, Is.EqualTo(50));
    Assert.That(modified.Top, Is.EqualTo(10));
    Assert.That(modified.Bottom, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void SetTop_MovesTopEdge() {
    var rect = new Rectangle(10, 10, 20, 20);
    var modified = rect.SetTop(5);

    Assert.That(modified.Left, Is.EqualTo(10));
    Assert.That(modified.Right, Is.EqualTo(30));
    Assert.That(modified.Top, Is.EqualTo(5));
    Assert.That(modified.Bottom, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void SetBottom_MovesBottomEdge() {
    var rect = new Rectangle(10, 10, 20, 20);
    var modified = rect.SetBottom(50);

    Assert.That(modified.Left, Is.EqualTo(10));
    Assert.That(modified.Right, Is.EqualTo(30));
    Assert.That(modified.Top, Is.EqualTo(10));
    Assert.That(modified.Bottom, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void SetLeft_DoesNotModifyOriginal() {
    var original = new Rectangle(10, 10, 20, 20);
    var _ = original.SetLeft(5);

    Assert.That(original.Left, Is.EqualTo(10));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void MultiplyBy_NegativeFactor_FlipsRectangle() {
    var rect = new Rectangle(10, 10, 20, 20);
    var scaled = rect.MultiplyBy(-1);

    Assert.That(scaled.X, Is.EqualTo(-10));
    Assert.That(scaled.Y, Is.EqualTo(-10));
    Assert.That(scaled.Width, Is.EqualTo(-20));
    Assert.That(scaled.Height, Is.EqualTo(-20));
  }

  [Test]
  [Category("EdgeCase")]
  public void CollidesWith_EmptyRectangle_ChecksPointLocation() {
    var rect = new Rectangle(0, 0, 20, 20);
    // Empty rectangle at (5,5) collapses to a single point
    var emptyInside = new Rectangle(5, 5, 0, 0);
    var emptyOutside = new Rectangle(25, 25, 0, 0);

    // Empty rect's "corners" are all at (5,5) which IS inside rect
    Assert.That(rect.CollidesWith(emptyInside), Is.True);
    // Empty rect's "corners" are all at (25,25) which is NOT inside rect
    Assert.That(rect.CollidesWith(emptyOutside), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Center_SinglePixelRectangle_ReturnsOrigin() {
    var rect = new Rectangle(5, 10, 1, 1);
    var center = rect.Center();

    Assert.That(center.X, Is.EqualTo(5));
    Assert.That(center.Y, Is.EqualTo(10));
  }

  #endregion
}
