using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
public class CollectionsMarshalTests {

  #region AsSpan

  [Test]
  [Category("HappyPath")]
  public void AsSpan_EmptyList_ReturnsEmptySpan() {
    var list = new List<int>();

    var span = CollectionsMarshal.AsSpan(list);

    Assert.That(span.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_NullList_ReturnsEmptySpan() {
    List<int>? list = null;

    var span = CollectionsMarshal.AsSpan(list);

    Assert.That(span.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ListWithItems_ReturnsSpanWithCorrectLength() {
    var list = new List<int> { 1, 2, 3, 4, 5 };

    var span = CollectionsMarshal.AsSpan(list);

    Assert.That(span.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ListWithItems_CanReadValues() {
    var list = new List<int> { 10, 20, 30 };

    var span = CollectionsMarshal.AsSpan(list);

    Assert.That(span[0], Is.EqualTo(10));
    Assert.That(span[1], Is.EqualTo(20));
    Assert.That(span[2], Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ModifySpan_UpdatesList() {
    var list = new List<int> { 1, 2, 3 };

    var span = CollectionsMarshal.AsSpan(list);
    span[1] = 99;

    Assert.That(list[1], Is.EqualTo(99));
  }

  #endregion

  #region GetValueRefOrNullRef

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrNullRef_ExistingKey_ReturnsRef() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, "key1");

    Assert.That(Unsafe.IsNullRef(ref value), Is.False);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrNullRef_NonExistingKey_ReturnsNullRef() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, "nonexistent");

    Assert.That(Unsafe.IsNullRef(ref value), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrNullRef_ModifyRef_UpdatesDictionary() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, "key1");
    value = 999;

    Assert.That(dictionary["key1"], Is.EqualTo(999));
  }

  #endregion

  #region SetCount

  [Test]
  [Category("HappyPath")]
  public void SetCount_IncreaseCount_IncreasesListCount() {
    var list = new List<int> { 1, 2, 3 };
    list.Capacity = 10;

    CollectionsMarshal.SetCount(list, 5);

    Assert.That(list.Count, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void SetCount_DecreaseCount_DecreasesListCount() {
    var list = new List<int> { 1, 2, 3, 4, 5 };

    CollectionsMarshal.SetCount(list, 2);

    Assert.That(list.Count, Is.EqualTo(2));
    Assert.That(list[0], Is.EqualTo(1));
    Assert.That(list[1], Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void SetCount_SetToZero_ClearsCount() {
    var list = new List<int> { 1, 2, 3 };

    CollectionsMarshal.SetCount(list, 0);

    Assert.That(list.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void SetCount_NegativeCount_ThrowsArgumentOutOfRangeException() {
    var list = new List<int> { 1, 2, 3 };

    Assert.Throws<ArgumentOutOfRangeException>(() => CollectionsMarshal.SetCount(list, -1));
  }

  [Test]
  [Category("HappyPath")]
  public void SetCount_CountExceedsCapacity_ExpandsCapacity() {
    var list = new List<int> { 1, 2, 3 };
    var targetCount = list.Capacity + 5;

    CollectionsMarshal.SetCount(list, targetCount);

    Assert.That(list.Count, Is.EqualTo(targetCount));
    Assert.That(list.Capacity, Is.GreaterThanOrEqualTo(targetCount));
  }

  [Test]
  [Category("Exception")]
  public void SetCount_NullList_ThrowsNullReferenceException() {
    List<int> list = null!;

    Assert.Throws<NullReferenceException>(() => CollectionsMarshal.SetCount(list, 5));
  }

  #endregion

  #region GetValueRefOrAddDefault

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_NewKey_ReturnsDefaultAndAddsKey() {
    var dictionary = new Dictionary<string, int>();

    ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out var exists);

    Assert.That(exists, Is.False);
    Assert.That(value, Is.EqualTo(0)); // default(int)
    Assert.That(dictionary.ContainsKey("key1"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_ExistingKey_ReturnsExistingValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out var exists);

    Assert.That(exists, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_ModifyRef_UpdatesDictionary() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out _);
    value = 999;

    Assert.That(dictionary["key1"], Is.EqualTo(999));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_IncrementRef_UpdatesDictionary() {
    var dictionary = new Dictionary<string, int>();

    ++CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out _);

    Assert.That(dictionary["key1"], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_MultipleIncrements_AccumulatesCorrectly() {
    var dictionary = new Dictionary<string, uint>();

    for (var i = 0; i < 100; ++i)
      ++CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out _);

    Assert.That(dictionary["key1"], Is.EqualTo(100u));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_ReferenceType_WorksCorrectly() {
    var dictionary = new Dictionary<string, string>();

    ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out var exists);

    Assert.That(exists, Is.False);
    Assert.That(value, Is.Null); // default(string)
    Assert.That(dictionary.ContainsKey("key1"), Is.True);
  }

  [Test]
  [Category("Exception")]
  public void GetValueRefOrAddDefault_NullDictionary_ThrowsNullReferenceException() {
    Dictionary<string, int> dictionary = null!;

    Assert.Throws<NullReferenceException>(() => CollectionsMarshal.GetValueRefOrAddDefault(dictionary, "key1", out _));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValueRefOrAddDefault_HistogramPattern_WorksCorrectly() {
    var dictionary = new Dictionary<int, uint>();
    var data = new[] { 1, 2, 3, 1, 2, 1, 1, 3, 2, 1 };

    foreach (var item in data)
      ++CollectionsMarshal.GetValueRefOrAddDefault(dictionary, item, out _);

    Assert.That(dictionary[1], Is.EqualTo(5u)); // 1 appears 5 times
    Assert.That(dictionary[2], Is.EqualTo(3u)); // 2 appears 3 times
    Assert.That(dictionary[3], Is.EqualTo(2u)); // 3 appears 2 times
  }

  #endregion
}
