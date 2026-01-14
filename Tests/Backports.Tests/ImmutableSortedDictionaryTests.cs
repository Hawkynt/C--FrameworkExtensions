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
[Category("ImmutableSortedDictionary")]
public class ImmutableSortedDictionaryTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyDictionary() {
    var empty = ImmutableSortedDictionary<string, int>.Empty;
    Assert.That(empty.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmpty_ReturnsTrue() {
    var empty = ImmutableSortedDictionary<string, int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptyDictionary() {
    var dict = ImmutableSortedDictionary.Create<string, int>();
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_WithComparer_UsesComparer() {
    var dict = ImmutableSortedDictionary.Create<string, int>(StringComparer.OrdinalIgnoreCase);
    var newDict = dict.Add("Key", 1);
    Assert.That(newDict.ContainsKey("key"), Is.True);
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesDictionary() {
    var source = new[] {
      new KeyValuePair<string, int>("a", 1),
      new KeyValuePair<string, int>("b", 2)
    };
    var dict = ImmutableSortedDictionary.CreateRange(source);
    Assert.That(dict.Count, Is.EqualTo(2));
  }

  #endregion

  #region ToImmutableSortedDictionary

  [Test]
  [Category("HappyPath")]
  public void ToImmutableSortedDictionary_FromEnumerable_CreatesDictionary() {
    var source = new[] { 1, 2, 3 };
    var dict = source.ToImmutableSortedDictionary(x => x.ToString(), x => x);
    Assert.That(dict.Count, Is.EqualTo(3));
  }

  #endregion

  #region Add/Remove

  [Test]
  [Category("HappyPath")]
  public void Add_ReturnsNewDictionaryWithElement() {
    var dict = ImmutableSortedDictionary<string, int>.Empty;
    var newDict = dict.Add("key", 42);
    Assert.That(newDict.Count, Is.EqualTo(1));
    Assert.That(newDict["key"], Is.EqualTo(42));
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AddRange_ReturnsNewDictionaryWithElements() {
    var dict = ImmutableSortedDictionary<string, int>.Empty;
    var newDict = dict.AddRange(
    [
      new("a", 1),
      new("b", 2)
    ]
    );
    Assert.That(newDict.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_ExistingKey_ReturnsNewDictionaryWithoutElement() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
    var newDict = dict.Remove("a");
    Assert.That(newDict.Count, Is.EqualTo(1));
    Assert.That(newDict.ContainsKey("a"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_NonExistingKey_ReturnsSameDictionary() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1);
    var newDict = dict.Remove("b");
    Assert.That(newDict, Is.SameAs(dict));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptyDictionary() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
    var newDict = dict.Clear();
    Assert.That(newDict.Count, Is.EqualTo(0));
  }

  #endregion

  #region SetItem

  [Test]
  [Category("HappyPath")]
  public void SetItem_NewKey_AddsElement() {
    var dict = ImmutableSortedDictionary<string, int>.Empty;
    var newDict = dict.SetItem("key", 42);
    Assert.That(newDict["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void SetItem_ExistingKey_UpdatesElement() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("key", 1);
    var newDict = dict.SetItem("key", 42);
    Assert.That(newDict["key"], Is.EqualTo(42));
    Assert.That(dict["key"], Is.EqualTo(1));
  }

  #endregion

  #region ContainsKey/TryGetValue

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_ExistingKey_ReturnsTrue() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("key", 42);
    Assert.That(dict.ContainsKey("key"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_NonExistingKey_ReturnsFalse() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("key", 42);
    Assert.That(dict.ContainsKey("other"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_ExistingKey_ReturnsTrueAndValue() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("key", 42);
    var found = dict.TryGetValue("key", out var value);
    Assert.That(found, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_NonExistingKey_ReturnsFalse() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("key", 42);
    var found = dict.TryGetValue("other", out _);
    Assert.That(found, Is.False);
  }

  #endregion

  #region Sorting

  [Test]
  [Category("HappyPath")]
  public void Enumeration_ReturnsSortedOrder() {
    var dict = ImmutableSortedDictionary<string, int>.Empty
      .Add("c", 3)
      .Add("a", 1)
      .Add("b", 2);
    var keys = dict.Keys.ToArray();
    Assert.That(keys, Is.EqualTo(new[] { "a", "b", "c" }));
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var dict = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1).Add("b", 2);
    var count = 0;
    foreach (var kvp in dict)
      ++count;
    Assert.That(count, Is.EqualTo(2));
  }

  #endregion

  #region Builder

  [Test]
  [Category("HappyPath")]
  public void CreateBuilder_ReturnsEmptyBuilder() {
    var builder = ImmutableSortedDictionary.CreateBuilder<string, int>();
    Assert.That(builder.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_Add_AddsElement() {
    var builder = ImmutableSortedDictionary.CreateBuilder<string, int>();
    builder.Add("a", 1);
    builder.Add("b", 2);
    Assert.That(builder.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Builder_ToImmutable_CreatesImmutableSortedDictionary() {
    var builder = ImmutableSortedDictionary.CreateBuilder<string, int>();
    builder.Add("a", 1);
    builder.Add("b", 2);
    var dict = builder.ToImmutable();
    Assert.That(dict.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ToBuilder_ReturnsBuilderWithElements() {
    var dict = ImmutableSortedDictionary<string, int>.Empty;
    dict = dict.Add("a", 1);
    dict = dict.Add("b", 2);
    var builder = dict.ToBuilder();
    Assert.That(builder.Count, Is.EqualTo(2));
    builder.Add("c", 3);
    Assert.That(builder.Count, Is.EqualTo(3));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void IDictionary_Add_ThrowsNotSupportedException() {
    IDictionary<string, int> dict = ImmutableSortedDictionary<string, int>.Empty.Add("a", 1);
    Assert.Throws<NotSupportedException>(() => dict.Add("b", 2));
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    ICollection<KeyValuePair<string, int>> dict = ImmutableSortedDictionary<string, int>.Empty;
    Assert.That(dict.IsReadOnly, Is.True);
  }

  #endregion

}
