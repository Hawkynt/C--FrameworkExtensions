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
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ReadOnlyCollections")]
public class ReadOnlyCollectionsTests {

  #region IReadOnlyCollection<T> via ReadOnlyList<T>

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyCollection_Count_ReturnsCorrectCount() {
    IReadOnlyCollection<int> collection = new ReadOnlyList<int>(new List<int> { 1, 2, 3 });
    Assert.That(collection.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyCollection_EmptyCollection_CountIsZero() {
    IReadOnlyCollection<string> collection = new ReadOnlyList<string>(new List<string>());
    Assert.That(collection.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyCollection_IsEnumerable() {
    IReadOnlyCollection<int> collection = new ReadOnlyList<int>(new List<int> { 10, 20, 30 });
    var sum = 0;
    foreach (var item in collection)
      sum += item;
    Assert.That(sum, Is.EqualTo(60));
  }

  #endregion

  #region IReadOnlyList<T> via ReadOnlyList<T>

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyList_Indexer_ReturnsCorrectElement() {
    IReadOnlyList<int> list = new ReadOnlyList<int>(new List<int> { 1, 2, 3 });
    Assert.That(list[0], Is.EqualTo(1));
    Assert.That(list[1], Is.EqualTo(2));
    Assert.That(list[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyList_Count_ReturnsCorrectCount() {
    IReadOnlyList<string> list = new ReadOnlyList<string>(new List<string> { "a", "b" });
    Assert.That(list.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void IReadOnlyList_ArrayAsReadOnlyList_Works() {
    IReadOnlyList<int> list = new ReadOnlyList<int>(new List<int> { 5, 10, 15 });
    Assert.That(list.Count, Is.EqualTo(3));
    Assert.That(list[1], Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyList_CanEnumerate() {
    IReadOnlyList<int> list = new ReadOnlyList<int>(new List<int> { 1, 2, 3, 4, 5 });
    var result = new List<int>();
    foreach (var item in list)
      result.Add(item);
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  #endregion

  #region IReadOnlyDictionary<TKey, TValue> via ReadOnlyDictionary<TKey, TValue>

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_Indexer_ReturnsCorrectValue() {
    IReadOnlyDictionary<string, int> dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> {
      { "one", 1 },
      { "two", 2 }
    });
    Assert.That(dict["one"], Is.EqualTo(1));
    Assert.That(dict["two"], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_ContainsKey_ReturnsCorrectResult() {
    IReadOnlyDictionary<string, int> dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> {
      { "one", 1 },
      { "two", 2 }
    });
    Assert.That(dict.ContainsKey("one"), Is.True);
    Assert.That(dict.ContainsKey("three"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_TryGetValue_Works() {
    IReadOnlyDictionary<string, int> dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> {
      { "one", 1 }
    });
    Assert.That(dict.TryGetValue("one", out var value), Is.True);
    Assert.That(value, Is.EqualTo(1));
    Assert.That(dict.TryGetValue("missing", out _), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_Keys_ReturnsAllKeys() {
    IReadOnlyDictionary<string, int> dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> {
      { "a", 1 },
      { "b", 2 },
      { "c", 3 }
    });
    var keys = dict.Keys.ToList();
    Assert.That(keys, Is.EquivalentTo(new[] { "a", "b", "c" }));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_Values_ReturnsAllValues() {
    IReadOnlyDictionary<string, int> dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> {
      { "a", 1 },
      { "b", 2 },
      { "c", 3 }
    });
    var values = dict.Values.ToList();
    Assert.That(values, Is.EquivalentTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void IReadOnlyDictionary_Count_ReturnsCorrectCount() {
    IReadOnlyDictionary<int, string> dict = new ReadOnlyDictionary<int, string>(new Dictionary<int, string> {
      { 1, "one" },
      { 2, "two" }
    });
    Assert.That(dict.Count, Is.EqualTo(2));
  }

  #endregion

  #region ReadOnlyDictionary<TKey, TValue>

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyDictionary_WrapsUnderlyingDictionary() {
    var inner = new Dictionary<string, int> { { "key", 42 } };
    var readOnly = new ReadOnlyDictionary<string, int>(inner);
    Assert.That(readOnly["key"], Is.EqualTo(42));
    Assert.That(readOnly.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyDictionary_ContainsKey_Works() {
    var inner = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
    var readOnly = new ReadOnlyDictionary<int, string>(inner);
    Assert.That(readOnly.ContainsKey(1), Is.True);
    Assert.That(readOnly.ContainsKey(999), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyDictionary_TryGetValue_Works() {
    var inner = new Dictionary<string, double> { { "pi", 3.14 } };
    var readOnly = new ReadOnlyDictionary<string, double>(inner);
    Assert.That(readOnly.TryGetValue("pi", out var value), Is.True);
    Assert.That(value, Is.EqualTo(3.14));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyDictionary_ImplementsIReadOnlyDictionary() {
    var inner = new Dictionary<string, int> { { "test", 1 } };
    IReadOnlyDictionary<string, int> readOnly = new ReadOnlyDictionary<string, int>(inner);
    Assert.That(readOnly.ContainsKey("test"), Is.True);
    Assert.That(readOnly.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void ReadOnlyDictionary_NullConstructorArg_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new ReadOnlyDictionary<string, int>(null));
  }

  #endregion

  #region ReadOnlyList<T>

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyList_WrapsUnderlyingList() {
    var inner = new List<int> { 1, 2, 3 };
    var readOnly = new ReadOnlyList<int>(inner);
    Assert.That(readOnly[0], Is.EqualTo(1));
    Assert.That(readOnly[2], Is.EqualTo(3));
    Assert.That(readOnly.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyList_Contains_Works() {
    var inner = new List<string> { "a", "b", "c" };
    var readOnly = new ReadOnlyList<string>(inner);
    Assert.That(readOnly.Contains("b"), Is.True);
    Assert.That(readOnly.Contains("z"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyList_IndexOf_Works() {
    var inner = new List<int> { 10, 20, 30 };
    var readOnly = new ReadOnlyList<int>(inner);
    Assert.That(readOnly.IndexOf(20), Is.EqualTo(1));
    Assert.That(readOnly.IndexOf(999), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyList_ImplementsIReadOnlyList() {
    var inner = new List<int> { 5, 10, 15 };
    IReadOnlyList<int> readOnly = new ReadOnlyList<int>(inner);
    Assert.That(readOnly[1], Is.EqualTo(10));
    Assert.That(readOnly.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void ReadOnlyList_NullConstructorArg_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new ReadOnlyList<int>(null));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlyList_IsReadOnly_ReturnsTrue() {
    var readOnly = new ReadOnlyList<int>(new List<int> { 1, 2, 3 });
    Assert.That(readOnly.IsReadOnly, Is.True);
  }

  #endregion
}
