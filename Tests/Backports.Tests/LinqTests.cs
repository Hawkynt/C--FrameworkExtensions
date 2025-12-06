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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Linq")]
public class LinqTests {

  #region ToArray

  [Test]
  [Category("HappyPath")]
  public void ToArray_ConvertsEnumerableToArray() {
    IEnumerable<int> source = new List<int> { 1, 2, 3 };
    var result = source.ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_WithEmptyEnumerable_ReturnsEmptyArray() {
    IEnumerable<int> source = new List<int>();
    var result = source.ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void ToArray_WithNull_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.ToArray());
  }

  #endregion

  #region Where

  [Test]
  [Category("HappyPath")]
  public void Where_FiltersElements() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.Where(x => x > 3).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 4, 5 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Where_WithIndex_FiltersElementsWithIndex() {
    var source = new[] { "a", "b", "c", "d" };
    var result = source.Where((x, i) => i % 2 == 0).ToArray();
    Assert.That(result, Is.EqualTo(new[] { "a", "c" }));
  }

  [Test]
  [Category("HappyPath")]
  public void Where_NoMatch_ReturnsEmpty() {
    var source = new[] { 1, 2, 3 };
    var result = source.Where(x => x > 10).ToArray();
    Assert.That(result, Is.Empty);
  }

  #endregion

  #region Select

  [Test]
  [Category("HappyPath")]
  public void Select_TransformsElements() {
    var source = new[] { 1, 2, 3 };
    var result = source.Select(x => x * 2).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 2, 4, 6 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Select_WithIndex_TransformsWithIndex() {
    var source = new[] { "a", "b", "c" };
    var result = source.Select((x, i) => $"{i}:{x}").ToArray();
    Assert.That(result, Is.EqualTo(new[] { "0:a", "1:b", "2:c" }));
  }

  #endregion

  #region First/FirstOrDefault

  [Test]
  [Category("HappyPath")]
  public void First_ReturnsFirstElement() {
    var source = new[] { 1, 2, 3 };
    var result = source.First();
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void First_WithPredicate_ReturnsFirstMatch() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.First(x => x > 3);
    Assert.That(result, Is.EqualTo(4));
  }

  [Test]
  [Category("Exception")]
  public void First_EmptySequence_ThrowsInvalidOperationException() {
    var source = new int[0];
    Assert.Throws<InvalidOperationException>(() => source.First());
  }

  [Test]
  [Category("HappyPath")]
  public void FirstOrDefault_EmptySequence_ReturnsDefault() {
    var source = new int[0];
    var result = source.FirstOrDefault();
    Assert.That(result, Is.EqualTo(0));
  }

  #endregion

  #region Last/LastOrDefault

  [Test]
  [Category("HappyPath")]
  public void Last_ReturnsLastElement() {
    var source = new[] { 1, 2, 3 };
    var result = source.Last();
    Assert.That(result, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Last_WithPredicate_ReturnsLastMatch() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.Last(x => x < 4);
    Assert.That(result, Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void Last_EmptySequence_ThrowsInvalidOperationException() {
    var source = new int[0];
    Assert.Throws<InvalidOperationException>(() => source.Last());
  }

  [Test]
  [Category("HappyPath")]
  public void LastOrDefault_EmptySequence_ReturnsDefault() {
    var source = new int[0];
    var result = source.LastOrDefault();
    Assert.That(result, Is.EqualTo(0));
  }

  #endregion

  #region Single/SingleOrDefault

  [Test]
  [Category("HappyPath")]
  public void Single_WithSingleElement_ReturnsElement() {
    var source = new[] { 42 };
    var result = source.Single();
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void Single_WithMultipleElements_ThrowsInvalidOperationException() {
    var source = new[] { 1, 2 };
    Assert.Throws<InvalidOperationException>(() => source.Single());
  }

  [Test]
  [Category("Exception")]
  public void Single_EmptySequence_ThrowsInvalidOperationException() {
    var source = new int[0];
    Assert.Throws<InvalidOperationException>(() => source.Single());
  }

  [Test]
  [Category("HappyPath")]
  public void SingleOrDefault_EmptySequence_ReturnsDefault() {
    var source = new int[0];
    var result = source.SingleOrDefault();
    Assert.That(result, Is.EqualTo(0));
  }

  #endregion

  #region Min/Max

  [Test]
  [Category("HappyPath")]
  public void Min_ReturnsMinimumValue() {
    var source = new[] { 3, 1, 4, 1, 5, 9 };
    var result = source.Min();
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Max_ReturnsMaximumValue() {
    var source = new[] { 3, 1, 4, 1, 5, 9 };
    var result = source.Max();
    Assert.That(result, Is.EqualTo(9));
  }

  [Test]
  [Category("Exception")]
  public void Min_EmptySequence_ThrowsInvalidOperationException() {
    var source = new int[0];
    Assert.Throws<InvalidOperationException>(() => source.Min());
  }

  [Test]
  [Category("Exception")]
  public void Max_EmptySequence_ThrowsInvalidOperationException() {
    var source = new int[0];
    Assert.Throws<InvalidOperationException>(() => source.Max());
  }

  #endregion

  #region OrderBy

  [Test]
  [Category("HappyPath")]
  public void OrderBy_SortsAscending() {
    var source = new[] { 3, 1, 4, 1, 5 };
    var result = source.OrderBy(x => x).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 1, 3, 4, 5 }));
  }

  [Test]
  [Category("HappyPath")]
  public void OrderBy_WithKeySelector_SortsByKey() {
    var source = new[] { "ccc", "a", "bb" };
    var result = source.OrderBy(x => x.Length).ToArray();
    Assert.That(result, Is.EqualTo(new[] { "a", "bb", "ccc" }));
  }

  #endregion

  #region Cast

  [Test]
  [Category("HappyPath")]
  public void Cast_CastsElements() {
    var source = new object[] { 1, 2, 3 };
    var result = source.Cast<int>().ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("Exception")]
  public void Cast_InvalidCast_ThrowsInvalidCastException() {
    var source = new object[] { 1, "two", 3 };
    Assert.Throws<InvalidCastException>(() => source.Cast<int>().ToArray());
  }

  #endregion

  #region GroupBy

  [Test]
  [Category("HappyPath")]
  public void GroupBy_GroupsElements() {
    var source = new[] { 1, 2, 3, 4, 5, 6 };
    var result = source.GroupBy(x => x % 2).ToArray();
    Assert.That(result.Length, Is.EqualTo(2));
  }

  #endregion

}
