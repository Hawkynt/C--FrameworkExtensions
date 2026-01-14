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
[Category("ImmutableArray")]
public class ImmutableArrayTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyArray() {
    var empty = ImmutableArray<int>.Empty;
    Assert.That(empty.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmpty_ReturnsTrue() {
    var empty = ImmutableArray<int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptyArray() {
    var array = ImmutableArray.Create<int>();
    Assert.That(array.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SingleItem_ReturnsArrayWithOneElement() {
    var array = ImmutableArray.Create(42);
    Assert.That(array.Length, Is.EqualTo(1));
    Assert.That(array[0], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_MultipleItems_ReturnsArrayWithElements() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.Length, Is.EqualTo(3));
    Assert.That(array[0], Is.EqualTo(1));
    Assert.That(array[1], Is.EqualTo(2));
    Assert.That(array[2], Is.EqualTo(3));
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesArray() {
    var source = new[] { 1, 2, 3 };
    var array = ImmutableArray.CreateRange(source);
    Assert.That(array.Length, Is.EqualTo(3));
  }

  #endregion

  #region ToImmutableArray

  [Test]
  [Category("HappyPath")]
  public void ToImmutableArray_FromEnumerable_CreatesArray() {
    var source = new[] { 1, 2, 3 };
    var array = source.ToImmutableArray();
    Assert.That(array.Length, Is.EqualTo(3));
  }

  #endregion

  #region Add/Remove

  [Test]
  [Category("HappyPath")]
  public void Add_ReturnsNewArrayWithElement() {
    var array = ImmutableArray.CreateRange([1, 2]);
    var newArray = array.Add(3);
    Assert.That(newArray.Length, Is.EqualTo(3));
    Assert.That(newArray[2], Is.EqualTo(3));
    Assert.That(array.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_ExistingElement_ReturnsNewArrayWithoutElement() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var newArray = array.Remove(2);
    Assert.That(newArray.Length, Is.EqualTo(2));
    Assert.That(newArray.Contains(2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveAt_ReturnsNewArrayWithoutElementAtIndex() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var newArray = array.RemoveAt(1);
    Assert.That(newArray.Length, Is.EqualTo(2));
    Assert.That(newArray[0], Is.EqualTo(1));
    Assert.That(newArray[1], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptyArray() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var newArray = array.Clear();
    Assert.That(newArray.IsEmpty, Is.True);
  }

  #endregion

  #region Insert/SetItem

  [Test]
  [Category("HappyPath")]
  public void Insert_ReturnsNewArrayWithElementAtIndex() {
    var array = ImmutableArray.CreateRange([1, 3]);
    var newArray = array.Insert(1, 2);
    Assert.That(newArray.Length, Is.EqualTo(3));
    Assert.That(newArray[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void SetItem_ReturnsNewArrayWithUpdatedElement() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var newArray = array.SetItem(1, 42);
    Assert.That(newArray[1], Is.EqualTo(42));
    Assert.That(array[1], Is.EqualTo(2));
  }

  #endregion

  #region IndexOf/Contains

  [Test]
  [Category("HappyPath")]
  public void IndexOf_ExistingElement_ReturnsIndex() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.IndexOf(2), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_NonExistingElement_ReturnsNegativeOne() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.IndexOf(4), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_ExistingElement_ReturnsTrue() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_NonExistingElement_ReturnsFalse() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.Contains(4), Is.False);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var sum = 0;
    foreach (var item in array)
      sum += item;
    Assert.That(sum, Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void Linq_WorksCorrectly() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var sum = array.Sum();
    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region Builder

  [Test]
  [Category("HappyPath")]
  public void CreateBuilder_ReturnsEmptyBuilder() {
    var builder = ImmutableArray.CreateBuilder<int>();
    Assert.That(builder.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_Add_AddsElement() {
    var builder = ImmutableArray.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    Assert.That(builder.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_ToImmutable_CreatesImmutableArray() {
    var builder = ImmutableArray.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    var array = builder.ToImmutable();
    Assert.That(array.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ToBuilder_ReturnsBuilderWithElements() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    var builder = array.ToBuilder();
    Assert.That(builder.Count, Is.EqualTo(3));
    builder.Add(4);
    Assert.That(builder.Count, Is.EqualTo(4));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void IList_Add_ThrowsNotSupportedException() {
    System.Collections.Generic.IList<int> array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.Throws<NotSupportedException>(() => array.Add(4));
  }

  [Test]
  [Category("Exception")]
  public void IList_Remove_ThrowsNotSupportedException() {
    System.Collections.Generic.IList<int> array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.Throws<NotSupportedException>(() => array.Remove(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    System.Collections.Generic.ICollection<int> array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.IsReadOnly, Is.True);
  }

  #endregion

  #region Equality

  [Test]
  [Category("HappyPath")]
  public void Equals_SameInstance_ReturnsTrue() {
    var array = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array.Equals(array), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Equals_DifferentInstances_SameContents_ReturnsFalse() {
    var array1 = ImmutableArray.CreateRange([1, 2, 3]);
    var array2 = ImmutableArray.CreateRange([1, 2, 3]);
    Assert.That(array1.Equals(array2), Is.False);
  }

  #endregion

}
