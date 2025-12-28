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
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("FrozenSet")]
public class FrozenSetTests {

  #region ToFrozenSet extension method

  [Test]
  [Category("HappyPath")]
  public void ToFrozenSet_FromEnumerable_CreatesFrozenSet() {
    var source = new[] { 1, 2, 3 };
    var frozen = source.ToFrozenSet();
    Assert.That(frozen.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ToFrozenSet_WithComparer_UsesComparer() {
    var source = new[] { "A", "b" };
    var frozen = source.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    Assert.That(frozen.Contains("a"), Is.True);
    Assert.That(frozen.Contains("B"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ToFrozenSet_RemovesDuplicates() {
    var source = new[] { 1, 2, 2, 3, 3, 3 };
    var frozen = source.ToFrozenSet();
    Assert.That(frozen.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void ToFrozenSet_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null!;
    Assert.Throws<ArgumentNullException>(() => source.ToFrozenSet());
  }

  #endregion

  #region Empty FrozenSet

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptySet() {
    var empty = FrozenSet<int>.Empty;
    Assert.That(empty.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsSameInstance() {
    var empty1 = FrozenSet<int>.Empty;
    var empty2 = FrozenSet<int>.Empty;
    Assert.That(empty1, Is.SameAs(empty2));
  }

  #endregion

  #region Contains

  [Test]
  [Category("HappyPath")]
  public void Contains_ExistingElement_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.That(frozen.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_NonExistingElement_ReturnsFalse() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.That(frozen.Contains(4), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Contains_EmptySet_ReturnsFalse() {
    var frozen = Array.Empty<int>().ToFrozenSet();
    Assert.That(frozen.Contains(1), Is.False);
  }

  #endregion

  #region Set operations - IsProperSubsetOf/IsProperSupersetOf

  [Test]
  [Category("HappyPath")]
  public void IsProperSubsetOf_ProperSubset_ReturnsTrue() {
    var frozen = new[] { 1, 2 }.ToFrozenSet();
    var other = new[] { 1, 2, 3 };
    Assert.That(frozen.IsProperSubsetOf(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsProperSubsetOf_EqualSets_ReturnsFalse() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 1, 2, 3 };
    Assert.That(frozen.IsProperSubsetOf(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsProperSupersetOf_ProperSuperset_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 1, 2 };
    Assert.That(frozen.IsProperSupersetOf(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsProperSupersetOf_EqualSets_ReturnsFalse() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 1, 2, 3 };
    Assert.That(frozen.IsProperSupersetOf(other), Is.False);
  }

  #endregion

  #region Set operations - IsSubsetOf/IsSupersetOf

  [Test]
  [Category("HappyPath")]
  public void IsSubsetOf_Subset_ReturnsTrue() {
    var frozen = new[] { 1, 2 }.ToFrozenSet();
    var other = new[] { 1, 2, 3 };
    Assert.That(frozen.IsSubsetOf(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsSubsetOf_EqualSets_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 3, 2, 1 };
    Assert.That(frozen.IsSubsetOf(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsSubsetOf_NotSubset_ReturnsFalse() {
    var frozen = new[] { 1, 2, 4 }.ToFrozenSet();
    var other = new[] { 1, 2, 3 };
    Assert.That(frozen.IsSubsetOf(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsSupersetOf_Superset_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 1, 2 };
    Assert.That(frozen.IsSupersetOf(other), Is.True);
  }

  #endregion

  #region Set operations - Overlaps/SetEquals

  [Test]
  [Category("HappyPath")]
  public void Overlaps_HasCommonElements_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 3, 4, 5 };
    Assert.That(frozen.Overlaps(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Overlaps_NoCommonElements_ReturnsFalse() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 4, 5, 6 };
    Assert.That(frozen.Overlaps(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SetEquals_EqualSets_ReturnsTrue() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 3, 1, 2 };
    Assert.That(frozen.SetEquals(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SetEquals_DifferentSets_ReturnsFalse() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var other = new[] { 1, 2, 4 };
    Assert.That(frozen.SetEquals(other), Is.False);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var source = new[] { 1, 2, 3 };
    var frozen = source.ToFrozenSet();
    var enumerated = frozen.ToList();
    Assert.That(enumerated, Has.Count.EqualTo(3));
    Assert.That(enumerated, Does.Contain(1));
    Assert.That(enumerated, Does.Contain(2));
    Assert.That(enumerated, Does.Contain(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Linq_WorksCorrectly() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var sum = frozen.Sum();
    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void ICollection_Add_ThrowsNotSupportedException() {
    ICollection<int> frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.Throws<NotSupportedException>(() => frozen.Add(4));
  }

  [Test]
  [Category("Exception")]
  public void ICollection_Remove_ThrowsNotSupportedException() {
    ICollection<int> frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.Throws<NotSupportedException>(() => frozen.Remove(1));
  }

  [Test]
  [Category("Exception")]
  public void ICollection_Clear_ThrowsNotSupportedException() {
    ICollection<int> frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.Throws<NotSupportedException>(() => frozen.Clear());
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    ICollection<int> frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    Assert.That(frozen.IsReadOnly, Is.True);
  }

  #endregion

  #region Comparer

  [Test]
  [Category("HappyPath")]
  public void Comparer_ReturnsUsedComparer() {
    var frozen = new[] { "a" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    Assert.That(frozen.Comparer, Is.EqualTo(StringComparer.OrdinalIgnoreCase));
  }

  #endregion

  #region CopyTo

  [Test]
  [Category("HappyPath")]
  public void CopyTo_CopiesElements() {
    var frozen = new[] { 1, 2, 3 }.ToFrozenSet();
    var array = new int[3];
    frozen.CopyTo(array, 0);
    Assert.That(array, Does.Contain(1));
    Assert.That(array, Does.Contain(2));
    Assert.That(array, Does.Contain(3));
  }

  #endregion

}
