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

using System;
using System.Globalization;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class HalfTests {

  [Test]
  public void Half_Zero_IsCorrect() {
    var half = (Half)0.0f;
    Assert.That((float)half, Is.EqualTo(0.0f));
  }

  [Test]
  public void Half_PositiveValue_RoundTrips() {
    var half = (Half)3.14f;
    var result = (float)half;
    Assert.That(result, Is.EqualTo(3.14f).Within(0.01f));
  }

  [Test]
  public void Half_NegativeValue_RoundTrips() {
    var half = (Half)(-2.5f);
    var result = (float)half;
    Assert.That(result, Is.EqualTo(-2.5f).Within(0.01f));
  }

  [Test]
  public void Half_MinValue_IsFinite() {
    Assert.That(Half.IsFinite(Half.MinValue), Is.True);
  }

  [Test]
  public void Half_MaxValue_IsFinite() {
    Assert.That(Half.IsFinite(Half.MaxValue), Is.True);
  }

  [Test]
  public void Half_NaN_IsNaN() {
    Assert.That(Half.IsNaN(Half.NaN), Is.True);
  }

  [Test]
  public void Half_PositiveInfinity_IsInfinity() {
    Assert.That(Half.IsInfinity(Half.PositiveInfinity), Is.True);
    Assert.That(Half.IsPositiveInfinity(Half.PositiveInfinity), Is.True);
  }

  [Test]
  public void Half_NegativeInfinity_IsInfinity() {
    Assert.That(Half.IsInfinity(Half.NegativeInfinity), Is.True);
    Assert.That(Half.IsNegativeInfinity(Half.NegativeInfinity), Is.True);
  }

  [Test]
  public void Half_Comparison_Works() {
    var a = (Half)1.0f;
    var b = (Half)2.0f;
    Assert.That(a < b, Is.True);
    Assert.That(b > a, Is.True);
    Assert.That(a <= b, Is.True);
    Assert.That(b >= a, Is.True);
    Assert.That(a != b, Is.True);
  }

  [Test]
  public void Half_Equality_Works() {
    var a = (Half)3.5f;
    var b = (Half)3.5f;
    Assert.That(a == b, Is.True);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  public void Half_ToString_ReturnsString() {
    var half = (Half)3.14f;
    var str = half.ToString();
    Assert.That(str, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  public void Half_Parse_Works() {
    var half = Half.Parse("3.14", CultureInfo.InvariantCulture);
    Assert.That((float)half, Is.EqualTo(3.14f).Within(0.01f));
  }

  [Test]
  public void Half_TryParse_ValidInput_ReturnsTrue() {
    var success = Half.TryParse("2.5", NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That((float)result, Is.EqualTo(2.5f).Within(0.01f));
  }

  [Test]
  public void Half_TryParse_InvalidInput_ReturnsFalse() {
    var success = Half.TryParse("not a number", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  public void Half_IsNegative_Works() {
    Assert.That(Half.IsNegative((Half)(-1.0f)), Is.True);
    Assert.That(Half.IsNegative((Half)1.0f), Is.False);
    Assert.That(Half.IsNegative((Half)0.0f), Is.False);
  }

  [Test]
  public void Half_IsNormal_Works() {
    Assert.That(Half.IsNormal((Half)1.0f), Is.True);
    Assert.That(Half.IsNormal(Half.NaN), Is.False);
    Assert.That(Half.IsNormal(Half.PositiveInfinity), Is.False);
  }

  [Test]
  public void Half_CompareTo_Works() {
    var a = (Half)1.0f;
    var b = (Half)2.0f;
    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(a), Is.EqualTo(0));
  }

  [Test]
  public void Half_GetHashCode_ConsistentForEqualValues() {
    var a = (Half)3.5f;
    var b = (Half)3.5f;
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

}
