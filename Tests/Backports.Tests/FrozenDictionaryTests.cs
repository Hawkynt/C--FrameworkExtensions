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
[Category("FrozenDictionary")]
public class FrozenDictionaryTests {

  #region ToFrozenDictionary extension methods

  [Test]
  [Category("HappyPath")]
  public void ToFrozenDictionary_FromKeyValuePairs_CreatesFrozenDictionary() {
    var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
    var frozen = source.ToFrozenDictionary();
    Assert.That(frozen.Count, Is.EqualTo(2));
    Assert.That(frozen["a"], Is.EqualTo(1));
    Assert.That(frozen["b"], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ToFrozenDictionary_WithComparer_UsesComparer() {
    var source = new Dictionary<string, int> { ["Key"] = 1 };
    var frozen = source.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    Assert.That(frozen.ContainsKey("KEY"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ToFrozenDictionary_WithKeySelector_SelectsKeys() {
    var source = new[] { "apple", "banana" };
    var frozen = source.ToFrozenDictionary(s => s[0]);
    Assert.That(frozen['a'], Is.EqualTo("apple"));
    Assert.That(frozen['b'], Is.EqualTo("banana"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToFrozenDictionary_WithKeyAndElementSelector_SelectsBoth() {
    var source = new[] { "apple", "banana" };
    var frozen = source.ToFrozenDictionary(s => s[0], s => s.Length);
    Assert.That(frozen['a'], Is.EqualTo(5));
    Assert.That(frozen['b'], Is.EqualTo(6));
  }

  [Test]
  [Category("Exception")]
  public void ToFrozenDictionary_NullSource_ThrowsArgumentNullException() {
    IEnumerable<KeyValuePair<string, int>> source = null!;
    Assert.Throws<ArgumentNullException>(() => source.ToFrozenDictionary());
  }

  #endregion

  #region Empty FrozenDictionary

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyDictionary() {
    var empty = FrozenDictionary<string, int>.Empty;
    Assert.That(empty.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsSameInstance() {
    var empty1 = FrozenDictionary<string, int>.Empty;
    var empty2 = FrozenDictionary<string, int>.Empty;
    Assert.That(empty1, Is.SameAs(empty2));
  }

  #endregion

  #region Lookup and ContainsKey

  [Test]
  [Category("HappyPath")]
  public void Indexer_ExistingKey_ReturnsValue() {
    var frozen = new Dictionary<string, int> { ["key"] = 42 }.ToFrozenDictionary();
    Assert.That(frozen["key"], Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void Indexer_NonExistingKey_ThrowsKeyNotFoundException() {
    var frozen = new Dictionary<string, int>().ToFrozenDictionary();
    Assert.Throws<KeyNotFoundException>(() => _ = frozen["missing"]);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_ExistingKey_ReturnsTrue() {
    var frozen = new Dictionary<string, int> { ["key"] = 42 }.ToFrozenDictionary();
    Assert.That(frozen.ContainsKey("key"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsKey_NonExistingKey_ReturnsFalse() {
    var frozen = new Dictionary<string, int> { ["key"] = 42 }.ToFrozenDictionary();
    Assert.That(frozen.ContainsKey("other"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_ExistingKey_ReturnsTrueAndValue() {
    var frozen = new Dictionary<string, int> { ["key"] = 42 }.ToFrozenDictionary();
    var result = frozen.TryGetValue("key", out var value);
    Assert.That(result, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetValue_NonExistingKey_ReturnsFalse() {
    var frozen = new Dictionary<string, int>().ToFrozenDictionary();
    var result = frozen.TryGetValue("missing", out _);
    Assert.That(result, Is.False);
  }

  #endregion

  #region Keys and Values collections

  [Test]
  [Category("HappyPath")]
  public void Keys_ReturnsAllKeys() {
    var frozen = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.ToFrozenDictionary();
    Assert.That(frozen.Keys.Count, Is.EqualTo(2));
    Assert.That(frozen.Keys, Does.Contain("a"));
    Assert.That(frozen.Keys, Does.Contain("b"));
  }

  [Test]
  [Category("HappyPath")]
  public void Values_ReturnsAllValues() {
    var frozen = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.ToFrozenDictionary();
    Assert.That(frozen.Values.Count, Is.EqualTo(2));
    Assert.That(frozen.Values, Does.Contain(1));
    Assert.That(frozen.Values, Does.Contain(2));
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesAllElements() {
    var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    var frozen = source.ToFrozenDictionary();
    var count = 0;
    foreach (var kvp in frozen) {
      Assert.That(source[kvp.Key], Is.EqualTo(kvp.Value));
      ++count;
    }
    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Linq_WorksCorrectly() {
    var frozen = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }.ToFrozenDictionary();
    var sum = frozen.Sum(kvp => kvp.Value);
    Assert.That(sum, Is.EqualTo(3));
  }

  #endregion

  #region Read-only enforcement

  [Test]
  [Category("Exception")]
  public void IDictionary_Add_ThrowsNotSupportedException() {
    IDictionary<string, int> frozen = new Dictionary<string, int>().ToFrozenDictionary();
    Assert.Throws<NotSupportedException>(() => frozen.Add("key", 1));
  }

  [Test]
  [Category("Exception")]
  public void IDictionary_Remove_ThrowsNotSupportedException() {
    IDictionary<string, int> frozen = new Dictionary<string, int> { ["key"] = 1 }.ToFrozenDictionary();
    Assert.Throws<NotSupportedException>(() => frozen.Remove("key"));
  }

  [Test]
  [Category("Exception")]
  public void IDictionary_Clear_ThrowsNotSupportedException() {
    IDictionary<string, int> frozen = new Dictionary<string, int> { ["key"] = 1 }.ToFrozenDictionary();
    Assert.Throws<NotSupportedException>(() => frozen.Clear());
  }

  [Test]
  [Category("Exception")]
  public void IDictionary_SetIndexer_ThrowsNotSupportedException() {
    IDictionary<string, int> frozen = new Dictionary<string, int> { ["key"] = 1 }.ToFrozenDictionary();
    Assert.Throws<NotSupportedException>(() => frozen["key"] = 2);
  }

  [Test]
  [Category("HappyPath")]
  public void ICollection_IsReadOnly_ReturnsTrue() {
    ICollection<KeyValuePair<string, int>> frozen = new Dictionary<string, int>().ToFrozenDictionary();
    Assert.That(frozen.IsReadOnly, Is.True);
  }

  #endregion

  #region Comparer

  [Test]
  [Category("HappyPath")]
  public void Comparer_ReturnsUsedComparer() {
    var frozen = new Dictionary<string, int>().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    Assert.That(frozen.Comparer, Is.EqualTo(StringComparer.OrdinalIgnoreCase));
  }

  [Test]
  [Category("HappyPath")]
  public void Comparer_DefaultComparer_ReturnsDefault() {
    var frozen = new Dictionary<string, int>().ToFrozenDictionary();
    Assert.That(frozen.Comparer, Is.EqualTo(EqualityComparer<string>.Default));
  }

  #endregion

  #region Count

  [Test]
  [Category("HappyPath")]
  public void Count_ReturnsCorrectCount() {
    var frozen = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 }.ToFrozenDictionary();
    Assert.That(frozen.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Count_EmptyDictionary_ReturnsZero() {
    var frozen = new Dictionary<string, int>().ToFrozenDictionary();
    Assert.That(frozen.Count, Is.EqualTo(0));
  }

  #endregion

}
