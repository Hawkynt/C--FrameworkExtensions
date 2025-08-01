using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace System.Collections.Specialized;

[TestFixture]
public class OrderedDictionaryTests {

  [Test]
  public void Constructor_Default_CreatesEmptyDictionary() {
    // Act
    var dictionary = new OrderedDictionary<string, int>();
    
    // Assert
    Assert.AreEqual(0, dictionary.Count);
    Assert.IsEmpty(dictionary.Keys);
    Assert.IsEmpty(dictionary.Values);
  }

  [Test]
  public void Constructor_WithComparer_UsesProvidedComparer() {
    // Arrange
    var comparer = StringComparer.OrdinalIgnoreCase;
    
    // Act
    var dictionary = new OrderedDictionary<string, int>(comparer);
    dictionary.Add("Key", 1);
    
    // Assert
    Assert.IsTrue(dictionary.ContainsKey("KEY"));
    Assert.IsTrue(dictionary.ContainsKey("key"));
  }

  [Test]
  public void Add_SingleItem_AddsToCollectionAndMaintainsOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    
    // Act
    dictionary.Add("first", 1);
    
    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual(1, dictionary["first"]);
    Assert.AreEqual("first", dictionary.Keys.First());
    Assert.AreEqual(1, dictionary.Values.First());
  }

  [Test]
  public void Add_MultipleItems_MaintainsInsertionOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    
    // Act
    dictionary.Add("third", 3);
    dictionary.Add("first", 1);
    dictionary.Add("second", 2);
    
    // Assert
    Assert.AreEqual(3, dictionary.Count);
    
    var keys = dictionary.Keys.ToArray();
    var values = dictionary.Values.ToArray();
    
    Assert.AreEqual("third", keys[0]);
    Assert.AreEqual("first", keys[1]);
    Assert.AreEqual("second", keys[2]);
    
    Assert.AreEqual(3, values[0]);
    Assert.AreEqual(1, values[1]);
    Assert.AreEqual(2, values[2]);
  }

  [Test]
  public void Add_DuplicateKey_ThrowsArgumentException() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    dictionary.Add("key", 1);
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => dictionary.Add("key", 2));
  }

  [Test]
  public void Remove_ExistingKey_RemovesItemAndMaintainsOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    dictionary.Add("first", 1);
    dictionary.Add("second", 2);
    dictionary.Add("third", 3);
    
    // Act
    dictionary.Remove("second");
    
    // Assert
    Assert.AreEqual(2, dictionary.Count);
    Assert.IsFalse(dictionary.ContainsKey("second"));
    
    var keys = dictionary.Keys.ToArray();
    Assert.AreEqual("first", keys[0]);
    Assert.AreEqual("third", keys[1]);
  }

  [Test]
  public void Remove_NonExistentKey_NoEffect() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    dictionary.Add("existing", 1);
    
    // Act
    dictionary.Remove("nonexistent");
    
    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.IsTrue(dictionary.ContainsKey("existing"));
  }

  [Test]
  public void Clear_WithItems_RemovesAllItemsAndOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    dictionary.Add("first", 1);
    dictionary.Add("second", 2);
    dictionary.Add("third", 3);
    
    // Act
    dictionary.Clear();
    
    // Assert
    Assert.AreEqual(0, dictionary.Count);
    Assert.IsEmpty(dictionary.Keys);
    Assert.IsEmpty(dictionary.Values);
  }

  [Test]
  public void Keys_ReturnsKeysInInsertionOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    var expectedOrder = new[] { "zebra", "apple", "banana", "cherry" };
    
    // Act
    foreach (var key in expectedOrder) {
      dictionary.Add(key, key.Length);
    }
    
    // Assert
    CollectionAssert.AreEqual(expectedOrder, dictionary.Keys.ToArray());
  }

  [Test]
  public void Values_ReturnsValuesInInsertionOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    var keyValuePairs = new[] {
      ("third", 300),
      ("first", 100),
      ("second", 200)
    };
    
    // Act
    foreach (var (key, value) in keyValuePairs) {
      dictionary.Add(key, value);
    }
    
    // Assert
    var expectedValues = keyValuePairs.Select(kvp => kvp.Item2).ToArray();
    CollectionAssert.AreEqual(expectedValues, dictionary.Values.ToArray());
  }

  [Test]
  public void GetEnumerator_ReturnsItemsInInsertionOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    var expectedPairs = new[] {
      new KeyValuePair<string, int>("delta", 4),
      new KeyValuePair<string, int>("alpha", 1),
      new KeyValuePair<string, int>("gamma", 3),
      new KeyValuePair<string, int>("beta", 2)
    };
    
    // Act
    foreach (var pair in expectedPairs) {
      dictionary.Add(pair.Key, pair.Value);
    }
    
    // Assert
    var actualPairs = dictionary.ToArray();
    CollectionAssert.AreEqual(expectedPairs, actualPairs);
  }

  [Test]
  public void Indexer_GetSet_WorksCorrectlyWithoutAffectingOrder() {
    // Arrange
    var dictionary = new OrderedDictionary<string, int>();
    dictionary.Add("first", 1);
    dictionary.Add("second", 2);
    dictionary.Add("third", 3);
    
    // Act - Get values
    var firstValue = dictionary["first"];
    var secondValue = dictionary["second"];
    
    // Act - Set values
    dictionary["second"] = 22;
    dictionary["first"] = 11;
    
    // Assert
    Assert.AreEqual(1, firstValue);
    Assert.AreEqual(2, secondValue);
    Assert.AreEqual(11, dictionary["first"]);
    Assert.AreEqual(22, dictionary["second"]);
    Assert.AreEqual(3, dictionary["third"]);
    
    // Assert order is maintained
    var keys = dictionary.Keys.ToArray();
    Assert.AreEqual("first", keys[0]);
    Assert.AreEqual("second", keys[1]);
    Assert.AreEqual("third", keys[2]);
  }

  [Test]
  public void ComplexOperations_MaintainOrderThroughoutLifecycle() {
    // Arrange
    var dictionary = new OrderedDictionary<int, string>();
    
    // Act - Add items
    dictionary.Add(100, "hundred");
    dictionary.Add(1, "one");
    dictionary.Add(50, "fifty");
    
    // Act - Remove middle item
    dictionary.Remove(1);
    
    // Act - Add more items
    dictionary.Add(25, "twenty-five");
    dictionary.Add(75, "seventy-five");
    
    // Act - Remove first item
    dictionary.Remove(100);
    
    // Assert final state
    Assert.AreEqual(3, dictionary.Count);
    
    var keys = dictionary.Keys.ToArray();
    var values = dictionary.Values.ToArray();
    
    Assert.AreEqual(50, keys[0]);
    Assert.AreEqual(25, keys[1]);
    Assert.AreEqual(75, keys[2]);
    
    Assert.AreEqual("fifty", values[0]);
    Assert.AreEqual("twenty-five", values[1]);
    Assert.AreEqual("seventy-five", values[2]);
  }

  [Test]
  public void WithNullableValues_HandlesNullCorrectly() {
    // Arrange
    var dictionary = new OrderedDictionary<string, string>();
    
    // Act
    dictionary.Add("key1", "value1");
    dictionary.Add("key2", null);
    dictionary.Add("key3", "value3");
    
    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual("value1", dictionary["key1"]);
    Assert.IsNull(dictionary["key2"]);
    Assert.AreEqual("value3", dictionary["key3"]);
    
    var values = dictionary.Values.ToArray();
    Assert.AreEqual("value1", values[0]);
    Assert.IsNull(values[1]);
    Assert.AreEqual("value3", values[2]);
  }

  [Test]
  public void WithComplexTypes_MaintainsOrderForComplexKeyValueTypes() {
    // Arrange
    var dictionary = new OrderedDictionary<DateTime, TimeSpan>();
    var dates = new[] {
      new DateTime(2023, 12, 25),
      new DateTime(2023, 1, 1),
      new DateTime(2023, 7, 4)
    };
    
    // Act
    foreach (var date in dates) {
      dictionary.Add(date, TimeSpan.FromDays((date - DateTime.MinValue).TotalDays));
    }
    
    // Assert
    var keyArray = dictionary.Keys.ToArray();
    CollectionAssert.AreEqual(dates, keyArray);
    
    // Verify values correspond to keys in order
    var values = dictionary.Values.ToArray();
    for (var i = 0; i < dates.Length; i++) {
      var expectedValue = TimeSpan.FromDays((dates[i] - DateTime.MinValue).TotalDays);
      Assert.AreEqual(expectedValue, values[i]);
    }
  }

  [Test]
  public void PerformanceTest_HandlesLargeNumberOfOperations() {
    // Arrange
    var dictionary = new OrderedDictionary<int, string>();
    const int itemCount = 1000;
    
    // Act - Add many items
    for (var i = 0; i < itemCount; i++) {
      dictionary.Add(i, $"value_{i}");
    }
    
    // Assert
    Assert.AreEqual(itemCount, dictionary.Count);
    
    // Verify order is maintained
    var keys = dictionary.Keys.ToArray();
    for (var i = 0; i < itemCount; i++) {
      Assert.AreEqual(i, keys[i]);
    }
    
    // Act - Remove every other item
    for (var i = 0; i < itemCount; i += 2) {
      dictionary.Remove(i);
    }
    
    // Assert
    Assert.AreEqual(itemCount / 2, dictionary.Count);
    
    // Verify remaining items maintain order
    var remainingKeys = dictionary.Keys.ToArray();
    for (var i = 0; i < remainingKeys.Length; i++) {
      Assert.AreEqual(i * 2 + 1, remainingKeys[i]);
    }
  }

}