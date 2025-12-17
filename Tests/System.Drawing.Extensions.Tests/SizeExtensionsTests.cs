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
[Category("Size")]
public class SizeExtensionsTests {

  #region Center Tests

  [Test]
  [Category("HappyPath")]
  public void Center_EvenDimensions_ReturnsCorrectCenter() {
    var size = new Size(100, 200);
    var center = size.Center();

    Assert.That(center.X, Is.EqualTo(50));
    Assert.That(center.Y, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_OddDimensions_ReturnsFloorCenter() {
    var size = new Size(101, 201);
    var center = size.Center();

    // Using bit shift >> 1, so 101 >> 1 = 50, 201 >> 1 = 100
    Assert.That(center.X, Is.EqualTo(50));
    Assert.That(center.Y, Is.EqualTo(100));
  }

  [Test]
  [Category("EdgeCase")]
  public void Center_ZeroSize_ReturnsZeroPoint() {
    var size = new Size(0, 0);
    var center = size.Center();

    Assert.That(center.X, Is.EqualTo(0));
    Assert.That(center.Y, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Center_OneByOne_ReturnsZero() {
    var size = new Size(1, 1);
    var center = size.Center();

    Assert.That(center.X, Is.EqualTo(0));
    Assert.That(center.Y, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Center_LargeSize_ReturnsCorrectCenter() {
    var size = new Size(1000, 2000);
    var center = size.Center();

    Assert.That(center.X, Is.EqualTo(500));
    Assert.That(center.Y, Is.EqualTo(1000));
  }

  #endregion

}
