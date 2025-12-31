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
[Category("HashSet")]
public class HashSetTests {

  #region Basic Operations

  [Test]
  [Category("HappyPath")]
  public void HashSet_Add_IncreasesCount() {
    var set = new HashSet<int>();
    set.Add(1);
    set.Add(2);
    set.Add(3);

    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Add_ReturnsTrueForNewItems() {
    var set = new HashSet<int>();

    Assert.That(set.Add(1), Is.True);
    Assert.That(set.Add(2), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_Add_ReturnsFalseForDuplicates() {
    var set = new HashSet<int> { 1, 2, 3 };

    Assert.That(set.Add(2), Is.False);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Contains_ReturnsTrueForExistingItem() {
    var set = new HashSet<int> { 1, 2, 3 };

    Assert.That(set.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Contains_ReturnsFalseForMissingItem() {
    var set = new HashSet<int> { 1, 2, 3 };

    Assert.That(set.Contains(5), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Remove_RemovesItem() {
    var set = new HashSet<int> { 1, 2, 3 };

    Assert.That(set.Remove(2), Is.True);
    Assert.That(set.Count, Is.EqualTo(2));
    Assert.That(set.Contains(2), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_Remove_ReturnsFalseForMissingItem() {
    var set = new HashSet<int> { 1, 2, 3 };

    Assert.That(set.Remove(5), Is.False);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Clear_RemovesAllItems() {
    var set = new HashSet<int> { 1, 2, 3, 4, 5 };
    set.Clear();

    Assert.That(set.Count, Is.EqualTo(0));
  }

  #endregion

  #region Constructors

  [Test]
  [Category("HappyPath")]
  public void HashSet_FromEnumerable_ContainsAllItems() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var set = new HashSet<int>(source);

    Assert.That(set.Count, Is.EqualTo(5));
    Assert.That(source.All(i => set.Contains(i)), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_FromEnumerableWithDuplicates_ContainsUniqueItems() {
    var source = new[] { 1, 2, 2, 3, 3, 3 };
    var set = new HashSet<int>(source);

    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_WithComparer_UsesComparer() {
    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    set.Add("Hello");

    Assert.That(set.Contains("hello"), Is.True);
    Assert.That(set.Contains("HELLO"), Is.True);
  }

  #endregion

  #region Set Operations

  [Test]
  [Category("HappyPath")]
  public void HashSet_UnionWith_CombinesSets() {
    var set1 = new HashSet<int> { 1, 2, 3 };
    var set2 = new[] { 3, 4, 5 };

    set1.UnionWith(set2);

    Assert.That(set1.Count, Is.EqualTo(5));
    Assert.That(set1.Contains(1), Is.True);
    Assert.That(set1.Contains(5), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IntersectWith_KeepsCommonItems() {
    var set1 = new HashSet<int> { 1, 2, 3, 4, 5 };
    var set2 = new[] { 3, 4, 5, 6, 7 };

    set1.IntersectWith(set2);

    Assert.That(set1.Count, Is.EqualTo(3));
    Assert.That(set1.Contains(3), Is.True);
    Assert.That(set1.Contains(4), Is.True);
    Assert.That(set1.Contains(5), Is.True);
    Assert.That(set1.Contains(1), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_ExceptWith_RemovesCommonItems() {
    var set1 = new HashSet<int> { 1, 2, 3, 4, 5 };
    var set2 = new[] { 3, 4, 5 };

    set1.ExceptWith(set2);

    Assert.That(set1.Count, Is.EqualTo(2));
    Assert.That(set1.Contains(1), Is.True);
    Assert.That(set1.Contains(2), Is.True);
    Assert.That(set1.Contains(3), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_SymmetricExceptWith_KeepsOnlyUniqueToEach() {
    var set1 = new HashSet<int> { 1, 2, 3 };
    var set2 = new[] { 2, 3, 4 };

    set1.SymmetricExceptWith(set2);

    Assert.That(set1.Count, Is.EqualTo(2));
    Assert.That(set1.Contains(1), Is.True);
    Assert.That(set1.Contains(4), Is.True);
    Assert.That(set1.Contains(2), Is.False);
    Assert.That(set1.Contains(3), Is.False);
  }

  #endregion

  #region Set Comparisons

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsSubsetOf_ReturnsTrueForSubset() {
    var set = new HashSet<int> { 2, 3 };
    var superset = new[] { 1, 2, 3, 4, 5 };

    Assert.That(set.IsSubsetOf(superset), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsSubsetOf_ReturnsFalseForNonSubset() {
    var set = new HashSet<int> { 2, 3, 6 };
    var other = new[] { 1, 2, 3, 4, 5 };

    Assert.That(set.IsSubsetOf(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsSupersetOf_ReturnsTrueForSuperset() {
    var set = new HashSet<int> { 1, 2, 3, 4, 5 };
    var subset = new[] { 2, 3 };

    Assert.That(set.IsSupersetOf(subset), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsSupersetOf_ReturnsFalseForNonSuperset() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 2, 3, 4 };

    Assert.That(set.IsSupersetOf(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Overlaps_ReturnsTrueForOverlapping() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 3, 4, 5 };

    Assert.That(set.Overlaps(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Overlaps_ReturnsFalseForDisjoint() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 4, 5, 6 };

    Assert.That(set.Overlaps(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_SetEquals_ReturnsTrueForEqualSets() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 3, 1, 2 };

    Assert.That(set.SetEquals(other), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_SetEquals_ReturnsFalseForDifferentSets() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 1, 2, 4 };

    Assert.That(set.SetEquals(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsProperSubsetOf_ReturnsTrueForProperSubset() {
    var set = new HashSet<int> { 2, 3 };
    var superset = new[] { 1, 2, 3, 4 };

    Assert.That(set.IsProperSubsetOf(superset), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_IsProperSubsetOf_ReturnsFalseForEqualSets() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 1, 2, 3 };

    Assert.That(set.IsProperSubsetOf(other), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_IsProperSupersetOf_ReturnsTrueForProperSuperset() {
    var set = new HashSet<int> { 1, 2, 3, 4 };
    var subset = new[] { 2, 3 };

    Assert.That(set.IsProperSupersetOf(subset), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_IsProperSupersetOf_ReturnsFalseForEqualSets() {
    var set = new HashSet<int> { 1, 2, 3 };
    var other = new[] { 1, 2, 3 };

    Assert.That(set.IsProperSupersetOf(other), Is.False);
  }

  #endregion

  #region TryGetValue

  [Test]
  [Category("HappyPath")]
  public void HashSet_TryGetValue_FindsMatchingItem() {
    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hello", "World" };

    var found = set.TryGetValue("HELLO", out var actual);

    Assert.That(found, Is.True);
    Assert.That(actual, Is.EqualTo("Hello"));
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_TryGetValue_ReturnsFalseForMissing() {
    var set = new HashSet<string> { "Hello", "World" };

    var found = set.TryGetValue("Missing", out var actual);

    Assert.That(found, Is.False);
    Assert.That(actual, Is.Null);
  }

  #endregion

  #region ToHashSet Extension

  [Test]
  [Category("HappyPath")]
  public void ToHashSet_CreatesHashSetFromEnumerable() {
    var source = new[] { 1, 2, 3, 4, 5 };

    var set = source.ToHashSet();

    Assert.That(set.Count, Is.EqualTo(5));
    Assert.That(source.All(i => set.Contains(i)), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ToHashSet_RemovesDuplicates() {
    var source = new[] { 1, 2, 2, 3, 3, 3 };

    var set = source.ToHashSet();

    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHashSet_WithComparer_UsesComparer() {
    var source = new[] { "Hello", "HELLO", "World" };

    var set = source.ToHashSet(StringComparer.OrdinalIgnoreCase);

    Assert.That(set.Count, Is.EqualTo(2));
  }

  #endregion

  #region RemoveWhere

  [Test]
  [Category("HappyPath")]
  public void HashSet_RemoveWhere_RemovesMatchingItems() {
    var set = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    var removed = set.RemoveWhere(x => x % 2 == 0);

    Assert.That(removed, Is.EqualTo(5));
    Assert.That(set.Count, Is.EqualTo(5));
    Assert.That(set.All(x => x % 2 != 0), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_RemoveWhere_NoMatches_RemovesNothing() {
    var set = new HashSet<int> { 1, 3, 5, 7, 9 };

    var removed = set.RemoveWhere(x => x % 2 == 0);

    Assert.That(removed, Is.EqualTo(0));
    Assert.That(set.Count, Is.EqualTo(5));
  }

  #endregion

  #region CopyTo

  [Test]
  [Category("HappyPath")]
  public void HashSet_CopyTo_CopiesAllItems() {
    var set = new HashSet<int> { 1, 2, 3 };
    var array = new int[3];

    set.CopyTo(array);

    Assert.That(array.Length, Is.EqualTo(3));
    Assert.That(set.All(i => array.Contains(i)), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_CopyTo_WithOffset_CopiesCorrectly() {
    var set = new HashSet<int> { 1, 2, 3 };
    var array = new int[5];

    set.CopyTo(array, 2);

    Assert.That(array[0], Is.EqualTo(0));
    Assert.That(array[1], Is.EqualTo(0));
    Assert.That(array.Skip(2).All(i => set.Contains(i)), Is.True);
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void HashSet_Empty_HasZeroCount() {
    var set = new HashSet<int>();

    Assert.That(set.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_NullItems_CanBeAddedForReferenceTypes() {
    var set = new HashSet<string>();

    Assert.That(set.Add(null!), Is.True);
    Assert.That(set.Contains(null!), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_Enumerate_ReturnsAllItems() {
    var set = new HashSet<int> { 1, 2, 3, 4, 5 };
    var enumerated = new List<int>();

    foreach (var item in set)
      enumerated.Add(item);

    Assert.That(enumerated.Count, Is.EqualTo(5));
    Assert.That(set.All(i => enumerated.Contains(i)), Is.True);
  }

  #endregion

}
