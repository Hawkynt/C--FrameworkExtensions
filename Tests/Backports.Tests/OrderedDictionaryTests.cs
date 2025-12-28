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
[Category("OrderedDictionary")]
public class OrderedDictionaryTests {

  #region Constructors

  [Test]
  [Category("HappyPath")]
  public void Constructor_Default_CreatesEmptyDictionary() {
    var dict = new OrderedDictionary<string, int>();
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacity_CreatesEmptyDictionary() {
    var dict = new OrderedDictionary<string, int>(10);
    Assert.That(dict.Count, Is.EqualTo(0));
    Assert.That(dict.Capacity, Is.GreaterThanOrEqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithComparer_UsesComparer() {
    var dict = new OrderedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    dict.Add("Key", 1);
    Assert.That(dict.ContainsKey("KEY"), Is.True);
    Assert.That(dict.Comparer, Is.EqualTo(StringComparer.OrdinalIgnoreCase));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityAndComparer_CreatesEmptyDictionary() {
    var dict = new OrderedDictionary<string, int>(10, StringComparer.OrdinalIgnoreCase);
    Assert.That(dict.Count, Is.EqualTo(0));
    Assert.That(dict.Comparer, Is.EqualTo(StringComparer.OrdinalIgnoreCase));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithDictionary_CopiesElements() {
    var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var dict = new OrderedDictionary<string, int>(source);
    Assert.That(dict.Count, Is.EqualTo(3));
    Assert.That(dict["a"], Is.EqualTo(1));
    Assert.That(dict["b"], Is.EqualTo(2));
    Assert.That(dict["c"], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEnumerable_CopiesElements() {
    var source = new[] {
      new KeyValuePair<string, int>("x", 10),
      new KeyValuePair<string, int>("y", 20)
    };
    var dict = new OrderedDictionary<string, int>(source);
    Assert.That(dict.Count, Is.EqualTo(2));
    Assert.That(dict["x"], Is.EqualTo(10));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNullDictionary_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new OrderedDictionary<string, int>((IDictionary<string, int>)null!));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNullEnumerable_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new OrderedDictionary<string, int>((IEnumerable<KeyValuePair<string, int>>)null!));
  }

  #endregion

  #region Add / TryAdd

  [Test]
  [Category("HappyPath")]
  public void Add_NewKey_AddsElement() {
    var dict = new OrderedDictionary<string, int>();
    dict.Add("key", 42);
    Assert.That(dict.Count, Is.EqualTo(1));
    Assert.That(dict["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void Add_DuplicateKey_ThrowsArgumentException() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 1 };
    Assert.Throws<ArgumentException>(() => dict.Add("key", 2));
  }

  [Test]
  [Category("HappyPath")]
  public void TryAdd_NewKey_ReturnsTrue() {
    var dict = new OrderedDictionary<string, int>();
    var result = dict.TryAdd("key", 42);
    Assert.That(result, Is.True);
    Assert.That(dict["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TryAdd_DuplicateKey_ReturnsFalse() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 1 };
    var result = dict.TryAdd("key", 2);
    Assert.That(result, Is.False);
    Assert.That(dict["key"], Is.EqualTo(1));
  }

  #endregion

  #region Remove / RemoveAt

  [Test]
  [Category("HappyPath")]
  public void Remove_ExistingKey_ReturnsTrue() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var result = dict.Remove("b");
    Assert.That(result, Is.True);
    Assert.That(dict.Count, Is.EqualTo(2));
    Assert.That(dict.ContainsKey("b"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_NonExistingKey_ReturnsFalse() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    var result = dict.Remove("z");
    Assert.That(result, Is.False);
    Assert.That(dict.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Remove_WithOutValue_ReturnsRemovedValue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    var result = dict.Remove("key", out var value);
    Assert.That(result, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveAt_ValidIndex_RemovesElement() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    dict.RemoveAt(1);
    Assert.That(dict.Count, Is.EqualTo(2));
    Assert.That(dict.ContainsKey("b"), Is.False);
  }

  [Test]
  [Category("Exception")]
  public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    Assert.Throws<ArgumentOutOfRangeException>(() => dict.RemoveAt(5));
  }

  #endregion

  #region ContainsKey / ContainsValue / TryGetValue

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_ExistingKey_ReturnsTrue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    Assert.That(dict.ContainsKey("key"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_NonExistingKey_ReturnsFalse() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    Assert.That(dict.ContainsKey("other"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsValue_ExistingValue_ReturnsTrue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    Assert.That(dict.ContainsValue(42), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsValue_NonExistingValue_ReturnsFalse() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    Assert.That(dict.ContainsValue(99), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_ExistingKey_ReturnsTrueAndValue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    var result = dict.TryGetValue("key", out var value);
    Assert.That(result, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_NonExistingKey_ReturnsFalse() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    var result = dict.TryGetValue("other", out var value);
    Assert.That(result, Is.False);
    Assert.That(value, Is.EqualTo(default(int)));
  }

  #endregion

  #region Indexer by Key

  [Test]
  [Category("HappyPath")]
  public void Indexer_GetExistingKey_ReturnsValue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    Assert.That(dict["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void Indexer_GetNonExistingKey_ThrowsKeyNotFoundException() {
    var dict = new OrderedDictionary<string, int>();
    Assert.Throws<KeyNotFoundException>(() => _ = dict["missing"]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_SetExistingKey_UpdatesValue() {
    var dict = new OrderedDictionary<string, int> { ["key"] = 42 };
    dict["key"] = 100;
    Assert.That(dict["key"], Is.EqualTo(100));
    Assert.That(dict.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_SetNewKey_AddsElement() {
    var dict = new OrderedDictionary<string, int>();
    dict["key"] = 42;
    Assert.That(dict["key"], Is.EqualTo(42));
    Assert.That(dict.Count, Is.EqualTo(1));
  }

  #endregion

  #region Index-based operations (GetAt, SetAt, Insert, IndexOf)

  [Test]
  [Category("HappyPath")]
  public void GetAt_ValidIndex_ReturnsKeyValuePair() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var kvp = dict.GetAt(1);
    Assert.That(kvp.Key, Is.EqualTo("b"));
    Assert.That(kvp.Value, Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void GetAt_InvalidIndex_ThrowsArgumentOutOfRangeException() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    Assert.Throws<ArgumentOutOfRangeException>(() => dict.GetAt(5));
  }

  [Test]
  [Category("HappyPath")]
  public void SetAt_ValidIndex_UpdatesValue() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2 };
    dict.SetAt(1, 99);
    Assert.That(dict["b"], Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void Insert_ValidIndex_InsertsElement() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["c"] = 3 };
    dict.Insert(1, "b", 2);
    Assert.That(dict.Count, Is.EqualTo(3));
    Assert.That(dict.GetAt(0).Key, Is.EqualTo("a"));
    Assert.That(dict.GetAt(1).Key, Is.EqualTo("b"));
    Assert.That(dict.GetAt(2).Key, Is.EqualTo("c"));
  }

  [Test]
  [Category("Exception")]
  public void Insert_DuplicateKey_ThrowsArgumentException() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    Assert.Throws<ArgumentException>(() => dict.Insert(0, "a", 2));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_ExistingKey_ReturnsIndex() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    Assert.That(dict.IndexOf("b"), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_NonExistingKey_ReturnsMinusOne() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    Assert.That(dict.IndexOf("z"), Is.EqualTo(-1));
  }

  #endregion

  #region Keys and Values collections

  [Test]
  [Category("HappyPath")]
  public void Keys_ReturnsAllKeys() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var keys = dict.Keys.ToList();
    Assert.That(keys, Has.Count.EqualTo(3));
    Assert.That(keys, Does.Contain("a"));
    Assert.That(keys, Does.Contain("b"));
    Assert.That(keys, Does.Contain("c"));
  }

  [Test]
  [Category("HappyPath")]
  public void Keys_PreservesOrder() {
    var dict = new OrderedDictionary<string, int> { ["c"] = 3, ["a"] = 1, ["b"] = 2 };
    var keys = dict.Keys.ToList();
    Assert.That(keys[0], Is.EqualTo("c"));
    Assert.That(keys[1], Is.EqualTo("a"));
    Assert.That(keys[2], Is.EqualTo("b"));
  }

  [Test]
  [Category("HappyPath")]
  public void Values_ReturnsAllValues() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var values = dict.Values.ToList();
    Assert.That(values, Has.Count.EqualTo(3));
    Assert.That(values, Does.Contain(1));
    Assert.That(values, Does.Contain(2));
    Assert.That(values, Does.Contain(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Values_PreservesOrder() {
    var dict = new OrderedDictionary<string, int> { ["c"] = 30, ["a"] = 10, ["b"] = 20 };
    var values = dict.Values.ToList();
    Assert.That(values[0], Is.EqualTo(30));
    Assert.That(values[1], Is.EqualTo(10));
    Assert.That(values[2], Is.EqualTo(20));
  }

  #endregion

  #region Enumeration and Order Preservation

  [Test]
  [Category("HappyPath")]
  public void Enumeration_PreservesInsertionOrder() {
    var dict = new OrderedDictionary<string, int>();
    dict.Add("third", 3);
    dict.Add("first", 1);
    dict.Add("second", 2);

    var keys = dict.Select(kvp => kvp.Key).ToList();
    Assert.That(keys[0], Is.EqualTo("third"));
    Assert.That(keys[1], Is.EqualTo("first"));
    Assert.That(keys[2], Is.EqualTo("second"));
  }

  [Test]
  [Category("HappyPath")]
  public void Enumeration_AfterRemoval_PreservesOrder() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3, ["d"] = 4 };
    dict.Remove("b");
    var keys = dict.Select(kvp => kvp.Key).ToList();
    Assert.That(keys, Is.EqualTo(new[] { "a", "c", "d" }));
  }

  [Test]
  [Category("HappyPath")]
  public void Enumeration_AfterInsert_PreservesOrder() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["c"] = 3 };
    dict.Insert(1, "b", 2);
    var keys = dict.Select(kvp => kvp.Key).ToList();
    Assert.That(keys, Is.EqualTo(new[] { "a", "b", "c" }));
  }

  #endregion

  #region Clear and Count

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllElements() {
    var dict = new OrderedDictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    dict.Clear();
    Assert.That(dict.Count, Is.EqualTo(0));
    Assert.That(dict.ContainsKey("a"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Count_ReturnsCorrectCount() {
    var dict = new OrderedDictionary<string, int>();
    Assert.That(dict.Count, Is.EqualTo(0));
    dict.Add("a", 1);
    Assert.That(dict.Count, Is.EqualTo(1));
    dict.Add("b", 2);
    Assert.That(dict.Count, Is.EqualTo(2));
    dict.Remove("a");
    Assert.That(dict.Count, Is.EqualTo(1));
  }

  #endregion

  #region Interface implementations

  [Test]
  [Category("HappyPath")]
  public void IDictionary_Add_AddsElement() {
    IDictionary<string, int> dict = new OrderedDictionary<string, int>();
    dict.Add("key", 42);
    Assert.That(dict["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsFalse() {
    ICollection<KeyValuePair<string, int>> collection = new OrderedDictionary<string, int>();
    Assert.That(collection.IsReadOnly, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_Keys_ReturnsKeys() {
    IReadOnlyDictionary<string, int> dict = new OrderedDictionary<string, int> { ["a"] = 1 };
    Assert.That(dict.Keys.Count(), Is.EqualTo(1));
  }

  #endregion

}
