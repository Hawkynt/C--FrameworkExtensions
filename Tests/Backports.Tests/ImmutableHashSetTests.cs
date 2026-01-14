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
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ImmutableHashSet")]
public class ImmutableHashSetTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptySet() {
    var empty = ImmutableHashSet<int>.Empty;
    Assert.That(empty.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmpty_ReturnsTrue() {
    var empty = ImmutableHashSet<int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptySet() {
    var set = ImmutableHashSet.Create<int>();
    Assert.That(set.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SingleItem_ReturnsSetWithOneElement() {
    var set = ImmutableHashSet.Create(42);
    Assert.That(set.Count, Is.EqualTo(1));
    Assert.That(set.Contains(42), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Create_MultipleItems_ReturnsSetWithElements() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_WithComparer_UsesComparer() {
    var set = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "A");
    Assert.That(set.Contains("a"), Is.True);
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesSet() {
    var source = new[] { 1, 2, 3 };
    var set = ImmutableHashSet.CreateRange(source);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CreateRange_RemovesDuplicates() {
    var source = new[] { 1, 2, 2, 3, 3, 3 };
    var set = ImmutableHashSet.CreateRange(source);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  #endregion

  #region ToImmutableHashSet

  [Test]
  [Category("HappyPath")]
  public void ToImmutableHashSet_FromEnumerable_CreatesSet() {
    var source = new[] { 1, 2, 3 };
    var set = source.ToImmutableHashSet();
    Assert.That(set.Count, Is.EqualTo(3));
  }

  #endregion

  #region Add/Remove

  [Test]
  [Category("HappyPath")]
  public void Add_NewElement_ReturnsNewSetWithElement() {
    var set = ImmutableHashSet.CreateRange([1, 2]);
    var newSet = set.Add(3);
    Assert.That(newSet.Count, Is.EqualTo(3));
    Assert.That(newSet.Contains(3), Is.True);
    Assert.That(set.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Add_ExistingElement_ReturnsSameSet() {
    var set = ImmutableHashSet.CreateRange([1, 2]);
    var newSet = set.Add(2);
    Assert.That(newSet, Is.SameAs(set));
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_ExistingElement_ReturnsNewSetWithoutElement() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var newSet = set.Remove(2);
    Assert.That(newSet.Count, Is.EqualTo(2));
    Assert.That(newSet.Contains(2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_NonExistingElement_ReturnsSameSet() {
    var set = ImmutableHashSet.CreateRange([1, 2]);
    var newSet = set.Remove(3);
    Assert.That(newSet, Is.SameAs(set));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptySet() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var newSet = set.Clear();
    Assert.That(newSet.Count, Is.EqualTo(0));
  }

  #endregion

  #region Set operations

  [Test]
  [Category("HappyPath")]
  public void Union_ReturnsSetWithAllElements() {
    var set = ImmutableHashSet.CreateRange([1, 2]);
    var newSet = set.Union([2, 3]);
    Assert.That(newSet.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Intersect_ReturnsSetWithCommonElements() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var newSet = set.Intersect([2, 3, 4]);
    Assert.That(newSet.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Except_ReturnsSetWithoutOtherElements() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var newSet = set.Except([2]);
    Assert.That(newSet.Count, Is.EqualTo(2));
    Assert.That(newSet.Contains(2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SymmetricExcept_ReturnsSetWithSymmetricDifference() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var newSet = set.SymmetricExcept([2, 3, 4]);
    Assert.That(newSet.Count, Is.EqualTo(2));
    Assert.That(newSet.Contains(1), Is.True);
    Assert.That(newSet.Contains(4), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsSubsetOf_Subset_ReturnsTrue() {
    var set = ImmutableHashSet.CreateRange([1, 2]);
    Assert.That(set.IsSubsetOf([1, 2, 3]), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsSupersetOf_Superset_ReturnsTrue() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.IsSupersetOf([1, 2]), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Overlaps_HasCommonElements_ReturnsTrue() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.Overlaps([3, 4, 5]), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SetEquals_EqualSets_ReturnsTrue() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.SetEquals([3, 1, 2]), Is.True);
  }

  #endregion

  #region Contains

  [Test]
  [Category("HappyPath")]
  public void Contains_ExistingElement_ReturnsTrue() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_NonExistingElement_ReturnsFalse() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.Contains(4), Is.False);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var sum = 0;
    foreach (var item in set)
      sum += item;
    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region Builder

  [Test]
  [Category("HappyPath")]
  public void CreateBuilder_ReturnsEmptyBuilder() {
    var builder = ImmutableHashSet.CreateBuilder<int>();
    Assert.That(builder.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_Add_AddsElement() {
    var builder = ImmutableHashSet.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    Assert.That(builder.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_ToImmutable_CreatesImmutableHashSet() {
    var builder = ImmutableHashSet.CreateBuilder<int>();
    builder.Add(1);
    builder.Add(2);
    var set = builder.ToImmutable();
    Assert.That(set.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ToBuilder_ReturnsBuilderWithElements() {
    var set = ImmutableHashSet.CreateRange([1, 2, 3]);
    var builder = set.ToBuilder();
    Assert.That(builder.Count, Is.EqualTo(3));
    builder.Add(4);
    Assert.That(builder.Count, Is.EqualTo(4));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void ICollection_Add_ThrowsNotSupportedException() {
    ICollection<int> set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.Throws<NotSupportedException>(() => set.Add(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    ICollection<int> set = ImmutableHashSet.CreateRange([1, 2, 3]);
    Assert.That(set.IsReadOnly, Is.True);
  }

  #endregion

}
