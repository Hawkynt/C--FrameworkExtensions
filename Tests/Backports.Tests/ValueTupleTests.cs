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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ValueTuple")]
public class ValueTupleTests {

  #region ValueTuple.Create Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Create_ZeroTuple_ReturnsEmptyTuple() {
    var tuple = ValueTuple.Create();
    Assert.That(tuple, Is.EqualTo(default(ValueTuple)));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_OneTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(42);
    Assert.That(tuple.Item1, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_TwoTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, "hello");
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo("hello"));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_ThreeTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_FourTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3, 4);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
    Assert.That(tuple.Item4, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_FiveTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3, 4, 5);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
    Assert.That(tuple.Item4, Is.EqualTo(4));
    Assert.That(tuple.Item5, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SixTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3, 4, 5, 6);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
    Assert.That(tuple.Item4, Is.EqualTo(4));
    Assert.That(tuple.Item5, Is.EqualTo(5));
    Assert.That(tuple.Item6, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SevenTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3, 4, 5, 6, 7);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
    Assert.That(tuple.Item4, Is.EqualTo(4));
    Assert.That(tuple.Item5, Is.EqualTo(5));
    Assert.That(tuple.Item6, Is.EqualTo(6));
    Assert.That(tuple.Item7, Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_EightTuple_ReturnsCorrectValues() {
    var tuple = ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item3, Is.EqualTo(3));
    Assert.That(tuple.Item4, Is.EqualTo(4));
    Assert.That(tuple.Item5, Is.EqualTo(5));
    Assert.That(tuple.Item6, Is.EqualTo(6));
    Assert.That(tuple.Item7, Is.EqualTo(7));
    Assert.That(tuple.Rest.Item1, Is.EqualTo(8));
  }

  #endregion

  #region Equality

  [Test]
  [Category("HappyPath")]
  public void Equals_SameValues_ReturnsTrue() {
    var a = (1, 2, 3);
    var b = (1, 2, 3);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Equals_DifferentValues_ReturnsFalse() {
    var a = (1, 2, 3);
    var b = (1, 2, 4);
    Assert.That(a.Equals(b), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void EqualityOperator_SameValues_ReturnsTrue() {
    var a = (1, "test");
    var b = (1, "test");
    Assert.That(a == b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void InequalityOperator_DifferentValues_ReturnsTrue() {
    var a = (1, "test");
    var b = (2, "test");
    Assert.That(a != b, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameValues_ReturnsSameHash() {
    var a = (1, 2, 3);
    var b = (1, 2, 3);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  #endregion

  #region Comparison

  [Test]
  [Category("HappyPath")]
  public void CompareTo_LessThan_ReturnsNegative() {
    var a = (1, 2);
    var b = (2, 2);
    Assert.That(a.CompareTo(b), Is.LessThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_Equal_ReturnsZero() {
    var a = (1, 2);
    var b = (1, 2);
    Assert.That(a.CompareTo(b), Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_GreaterThan_ReturnsPositive() {
    var a = (2, 2);
    var b = (1, 2);
    Assert.That(a.CompareTo(b), Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_SecondElementDiffers_ComparesCorrectly() {
    var a = (1, 1);
    var b = (1, 2);
    Assert.That(a.CompareTo(b), Is.LessThan(0));
  }

  #endregion

  #region ToString

  [Test]
  [Category("HappyPath")]
  public void ToString_EmptyTuple_ReturnsParentheses() {
    var tuple = ValueTuple.Create();
    Assert.That(tuple.ToString(), Is.EqualTo("()"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToString_OneTuple_ReturnsFormattedString() {
    var tuple = ValueTuple.Create(42);
    Assert.That(tuple.ToString(), Is.EqualTo("(42)"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToString_TwoTuple_ReturnsFormattedString() {
    var tuple = (1, 2);
    Assert.That(tuple.ToString(), Is.EqualTo("(1, 2)"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToString_ThreeTuple_ReturnsFormattedString() {
    var tuple = (1, 2, 3);
    Assert.That(tuple.ToString(), Is.EqualTo("(1, 2, 3)"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToString_WithNull_HandlesNull() {
    var tuple = (1, (string)null, 3);
    var str = tuple.ToString();
    Assert.That(str, Does.Contain("1"));
    Assert.That(str, Does.Contain("3"));
  }

  #endregion

  #region Mutability

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_FieldsAreMutable() {
    var tuple = (1, 2);
    tuple.Item1 = 10;
    tuple.Item2 = 20;
    Assert.That(tuple.Item1, Is.EqualTo(10));
    Assert.That(tuple.Item2, Is.EqualTo(20));
  }

  #endregion

  #region Mixed Types

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_MixedTypes_StoresCorrectly() {
    var tuple = (42, "hello", 3.14, true, 'A');
    Assert.That(tuple.Item1, Is.EqualTo(42));
    Assert.That(tuple.Item2, Is.EqualTo("hello"));
    Assert.That(tuple.Item3, Is.EqualTo(3.14));
    Assert.That(tuple.Item4, Is.True);
    Assert.That(tuple.Item5, Is.EqualTo('A'));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_NestedTuples_StoresCorrectly() {
    var tuple = ((1, 2), (3, 4));
    Assert.That(tuple.Item1.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item1.Item2, Is.EqualTo(2));
    Assert.That(tuple.Item2.Item1, Is.EqualTo(3));
    Assert.That(tuple.Item2.Item2, Is.EqualTo(4));
  }

  #endregion

}
