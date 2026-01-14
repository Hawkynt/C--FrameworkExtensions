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
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ImmutableList")]
public class ImmutableListTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyList() {
    var empty = ImmutableList<int>.Empty;
    Assert.That(empty.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmpty_ReturnsTrue() {
    var empty = ImmutableList<int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptyList() {
    var list = ImmutableList.Create<int>();
    Assert.That(list.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SingleItem_ReturnsListWithOneElement() {
    var list = ImmutableList.Create(42);
    Assert.That(list.Count, Is.EqualTo(1));
    Assert.That(list[0], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_MultipleItems_ReturnsListWithElements() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.Count, Is.EqualTo(3));
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesList() {
    var source = new[] { 1, 2, 3 };
    var list = ImmutableList.CreateRange(source);
    Assert.That(list.Count, Is.EqualTo(3));
  }

  #endregion

  #region ToImmutableList

  [Test]
  [Category("HappyPath")]
  public void ToImmutableList_FromEnumerable_CreatesList() {
    var source = new[] { 1, 2, 3 };
    var list = source.ToImmutableList();
    Assert.That(list.Count, Is.EqualTo(3));
  }

  #endregion

  #region Add/Remove

  [Test]
  [Category("HappyPath")]
  public void Add_ReturnsNewListWithElement() {
    var list = ImmutableList.CreateRange([1, 2]);
    var newList = list.Add(3);
    Assert.That(newList.Count, Is.EqualTo(3));
    Assert.That(newList[2], Is.EqualTo(3));
    Assert.That(list.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AddRange_ReturnsNewListWithElements() {
    var list = ImmutableList.CreateRange([1, 2]);
    var newList = list.AddRange([3, 4]);
    Assert.That(newList.Count, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_ExistingElement_ReturnsNewListWithoutElement() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var newList = list.Remove(2);
    Assert.That(newList.Count, Is.EqualTo(2));
    Assert.That(newList.Contains(2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveAt_ReturnsNewListWithoutElementAtIndex() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var newList = list.RemoveAt(1);
    Assert.That(newList.Count, Is.EqualTo(2));
    Assert.That(newList[0], Is.EqualTo(1));
    Assert.That(newList[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptyList() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var newList = list.Clear();
    Assert.That(newList.IsEmpty, Is.True);
  }

  #endregion

  #region Insert/SetItem

  [Test]
  [Category("HappyPath")]
  public void Insert_ReturnsNewListWithElementAtIndex() {
    var list = ImmutableList.CreateRange([1, 3]);
    var newList = list.Insert(1, 2);
    Assert.That(newList.Count, Is.EqualTo(3));
    Assert.That(newList[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void SetItem_ReturnsNewListWithUpdatedElement() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var newList = list.SetItem(1, 42);
    Assert.That(newList[1], Is.EqualTo(42));
    Assert.That(list[1], Is.EqualTo(2));
  }

  #endregion

  #region IndexOf/Contains

  [Test]
  [Category("HappyPath")]
  public void IndexOf_ExistingElement_ReturnsIndex() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.IndexOf(2), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_NonExistingElement_ReturnsNegativeOne() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.IndexOf(4), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_ExistingElement_ReturnsTrue() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_NonExistingElement_ReturnsFalse() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.Contains(4), Is.False);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var sum = 0;
    foreach (var item in list)
      sum += item;

    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region Builder

  [Test]
  [Category("HappyPath")]
  public void CreateBuilder_ReturnsEmptyBuilder() {
    var builder = ImmutableList.CreateBuilder<int>();
    Assert.That(builder.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_Add_AddsElement() {
    var builder = ImmutableList.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    Assert.That(builder.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_ToImmutable_CreatesImmutableList() {
    var builder = ImmutableList.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    var list = builder.ToImmutable();
    Assert.That(list.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ToBuilder_ReturnsBuilderWithElements() {
    var list = ImmutableList.CreateRange([1, 2, 3]);
    var builder = list.ToBuilder();
    Assert.That(builder.Count, Is.EqualTo(3));
    builder.Add(4);
    Assert.That(builder.Count, Is.EqualTo(4));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void IList_Add_ThrowsNotSupportedException() {
    System.Collections.Generic.IList<int> list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.Throws<NotSupportedException>(() => list.Add(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    System.Collections.Generic.ICollection<int> list = ImmutableList.CreateRange([1, 2, 3]);
    Assert.That(list.IsReadOnly, Is.True);
  }

  #endregion

}
