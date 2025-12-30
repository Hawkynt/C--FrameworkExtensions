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
[Category("CollectionExtensions")]
public class CollectionExtensionsAsReadOnlyTests {

  #region AsReadOnly - IList<T>

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_List_ReturnsReadOnlyCollection() {
    IList<int> list = new List<int> { 1, 2, 3, 4, 5 };

    var readOnly = list.AsReadOnly();
    Assert.That(readOnly, Is.InstanceOf<ReadOnlyCollection<int>>());
    Assert.That(readOnly.Count, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_List_ContainsSameElements() {
    IList<string> list = new List<string> { "a", "b", "c" };

    var readOnly = list.AsReadOnly();

    Assert.That(readOnly[0], Is.EqualTo("a"));
    Assert.That(readOnly[1], Is.EqualTo("b"));
    Assert.That(readOnly[2], Is.EqualTo("c"));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_List_ReflectsChangesToOriginal() {
    var list = new List<int> { 1, 2, 3 };
    var readOnly = ((IList<int>)list).AsReadOnly();

    list.Add(4);

    Assert.That(readOnly.Count, Is.EqualTo(4));
    Assert.That(readOnly[3], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_EmptyList_ReturnsEmptyReadOnlyCollection() {
    IList<int> list = new List<int>();

    var readOnly = list.AsReadOnly();
    Assert.That(readOnly.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_Array_ReturnsReadOnlyCollection() {
    var array = new[] { 1, 2, 3, 4, 5 };

    var readOnly = ((IList<int>)array).AsReadOnly();
    Assert.That(readOnly, Is.InstanceOf<ReadOnlyCollection<int>>());
    Assert.That(readOnly.Count, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_ExtensionMethod_WorksCorrectly() {
    IList<int> list = new List<int> { 1, 2, 3 };

    var readOnly = list.AsReadOnly();

    Assert.That(readOnly, Is.InstanceOf<ReadOnlyCollection<int>>());
    Assert.That(readOnly.Count, Is.EqualTo(3));
  }

  #endregion

  #region AsReadOnly - IDictionary<TKey, TValue>

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_Dictionary_ReturnsReadOnlyDictionary() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "one", 1 },
      { "two", 2 },
      { "three", 3 }
    };

    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly, Is.InstanceOf<ReadOnlyDictionary<string, int>>());
    Assert.That(readOnly.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_Dictionary_ContainsSameElements() {
    IDictionary<int, string> dict = new Dictionary<int, string> {
      { 1, "one" },
      { 2, "two" },
      { 3, "three" }
    };

    var readOnly = dict.AsReadOnly();
    Assert.That(readOnly[1], Is.EqualTo("one"));
    Assert.That(readOnly[2], Is.EqualTo("two"));
    Assert.That(readOnly[3], Is.EqualTo("three"));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_Dictionary_ReflectsChangesToOriginal() {
    var dict = new Dictionary<string, int> {
      { "a", 1 },
      { "b", 2 }
    };
    var readOnly = dict.AsReadOnly();

    dict["c"] = 3;

    Assert.That(readOnly.Count, Is.EqualTo(3));
    Assert.That(readOnly["c"], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_EmptyDictionary_ReturnsEmptyReadOnlyDictionary() {
    IDictionary<string, int> dict = new Dictionary<string, int>();

    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AsReadOnly_Dictionary_ExtensionMethod_WorksCorrectly() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "one", 1 },
      { "two", 2 }
    };

    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly, Is.InstanceOf<ReadOnlyDictionary<string, int>>());
    Assert.That(readOnly.Count, Is.EqualTo(2));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_NullList_ThrowsArgumentNullException() {
    IList<int> list = null!;

    Assert.Throws<ArgumentNullException>(() => list.AsReadOnly());
  }

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_NullDictionary_ThrowsArgumentNullException() {
    IDictionary<string, int> dict = null!;

    Assert.Throws<ArgumentNullException>(() => dict.AsReadOnly());
  }

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_Dictionary_CanLookupByKey() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "key1", 100 },
      { "key2", 200 }
    };
    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly.ContainsKey("key1"), Is.True);
    Assert.That(readOnly.ContainsKey("nonexistent"), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_Dictionary_TryGetValueWorks() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "test", 42 }
    };
    var readOnly = dict.AsReadOnly();

    var found = readOnly.TryGetValue("test", out var value);

    Assert.That(found, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_Dictionary_Keys_ReturnsAllKeys() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "a", 1 },
      { "b", 2 },
      { "c", 3 }
    };
    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly.Keys.Count(), Is.EqualTo(3));
    Assert.That(readOnly.Keys, Does.Contain("a"));
    Assert.That(readOnly.Keys, Does.Contain("b"));
    Assert.That(readOnly.Keys, Does.Contain("c"));
  }

  [Test]
  [Category("EdgeCase")]
  public void AsReadOnly_Dictionary_Values_ReturnsAllValues() {
    IDictionary<string, int> dict = new Dictionary<string, int> {
      { "a", 1 },
      { "b", 2 },
      { "c", 3 }
    };
    var readOnly = dict.AsReadOnly();

    Assert.That(readOnly.Values.Count(), Is.EqualTo(3));
    Assert.That(readOnly.Values, Does.Contain(1));
    Assert.That(readOnly.Values, Does.Contain(2));
    Assert.That(readOnly.Values, Does.Contain(3));
  }

  #endregion

}
