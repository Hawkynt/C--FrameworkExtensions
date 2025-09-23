using System.Linq;
using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class DictionaryExtensionsTests {
  #region AddRange Tests

  [Test]
  public void AddRange_ParamsObjectArray_EmptyArray_NoItemsAdded() {
    // Arrange
    var dictionary = new Dictionary<string, int>();

    // Act
    dictionary.AddRange();

    // Assert
    Assert.AreEqual(0, dictionary.Count);
  }

  [Test]
  public void AddRange_ParamsObjectArray_EvenNumberOfParameters_AddsAllPairs() {
    // Arrange
    var dictionary = new Dictionary<string, int>();

    // Act
    dictionary.AddRange("key1", 1, "key2", 2, "key3", 3);

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual(1, dictionary["key1"]);
    Assert.AreEqual(2, dictionary["key2"]);
    Assert.AreEqual(3, dictionary["key3"]);
  }

  [Test]
  public void AddRange_ParamsObjectArray_OddNumberOfParameters_IgnoresLastParameter() {
    // Arrange
    var dictionary = new Dictionary<string, int>();

    // Act
    dictionary.AddRange("key1", 1, "key2", 2, "orphan");

    // Assert
    Assert.AreEqual(2, dictionary.Count);
    Assert.AreEqual(1, dictionary["key1"]);
    Assert.AreEqual(2, dictionary["key2"]);
    Assert.IsFalse(dictionary.ContainsKey("orphan"));
  }

  [Test]
  public void AddRange_ParamsObjectArray_NullArray_NoEffect() {
    // Arrange
    var dictionary = new Dictionary<string, int>();
    object[]? nullArray = null;

    // Act
    dictionary.AddRange(nullArray);

    // Assert
    Assert.AreEqual(0, dictionary.Count);
  }

  [Test]
  public void AddRange_ParamsObjectArray_TypeCasting_WorksCorrectly() {
    // Arrange
    var dictionary = new Dictionary<string, object>();

    // Act
    dictionary.AddRange("int", 42, "string", "hello", "bool", true);

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual(42, dictionary["int"]);
    Assert.AreEqual("hello", dictionary["string"]);
    Assert.AreEqual(true, dictionary["bool"]);
  }

  [Test]
  public void AddRange_ParamsObjectArray_DuplicateKey_ThrowsArgumentException() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "existing", 999 } };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => dictionary.AddRange("key1", 1, "existing", 2));
  }

  [Test]
  public void AddRange_KeyValuePairEnumerable_EmptyEnumerable_NoItemsAdded() {
    // Arrange
    var dictionary = new Dictionary<string, int>();
    var pairs = Enumerable.Empty<KeyValuePair<string, int>>();

    // Act
    dictionary.AddRange(pairs);

    // Assert
    Assert.AreEqual(0, dictionary.Count);
  }

  [Test]
  public void AddRange_KeyValuePairEnumerable_ValidPairs_AddsAllItems() {
    // Arrange
    var dictionary = new Dictionary<string, int>();
    var pairs = new[] { new KeyValuePair<string, int>("key1", 1), new KeyValuePair<string, int>("key2", 2), new KeyValuePair<string, int>("key3", 3) };

    // Act
    dictionary.AddRange(pairs);

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual(1, dictionary["key1"]);
    Assert.AreEqual(2, dictionary["key2"]);
    Assert.AreEqual(3, dictionary["key3"]);
  }

  [Test]
  public void AddRange_KeyValuePairEnumerable_NullEnumerable_ThrowsArgumentNullException() {
    // Arrange
    var dictionary = new Dictionary<string, int>();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => dictionary.AddRange((IEnumerable<KeyValuePair<string, int>>)null));
  }

  [Test]
  public void AddRange_NullDictionary_ThrowsArgumentNullException() {
    // Arrange
    Dictionary<string, int>? dictionary = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => dictionary.AddRange("key", 1));
  }

  #endregion

  #region HasKeyDo Tests

  [Test]
  public void HasKeyDo_ExistingKey_ActionExecuted_ReturnsTrue() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };
    var actionExecuted = false;
    string? capturedKey = null;
    var capturedValue = 0;

    // Act
    var result = dictionary.HasKeyDo(
      "key1",
      (key, value) => {
        actionExecuted = true;
        capturedKey = key;
        capturedValue = value;
      }
    );

    // Assert
    Assert.IsTrue(result);
    Assert.IsTrue(actionExecuted);
    Assert.AreEqual("key1", capturedKey);
    Assert.AreEqual(42, capturedValue);
  }

  [Test]
  public void HasKeyDo_NonExistentKey_ActionNotExecuted_ReturnsFalse() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };
    var actionExecuted = false;

    // Act
    var result = dictionary.HasKeyDo("nonexistent", (key, value) => { actionExecuted = true; });

    // Assert
    Assert.IsFalse(result);
    Assert.IsFalse(actionExecuted);
  }

  [Test]
  public void HasKeyDo_ValueOnlyAction_ExistingKey_ActionExecuted_ReturnsTrue() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };
    var capturedValue = 0;

    // Act
    var result = dictionary.HasKeyDo("key1", value => { capturedValue = value; });

    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(42, capturedValue);
  }

  [Test]
  public void HasKeyDo_NullAction_ThrowsArgumentNullException() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => dictionary.HasKeyDo("key1", (Action<string, int>)null));
  }

  #endregion

  #region GetValueOrDefault Tests

  [Test]
  public void GetValueOrDefault_ExistingKey_ReturnsValue() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act
    var result = dictionary.GetValueOrDefault("key1");

    // Assert
    Assert.AreEqual(42, result);
  }

  [Test]
  public void GetValueOrDefault_NonExistentKey_ReturnsDefaultValue() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act
    var result = dictionary.GetValueOrDefault("nonexistent");

    // Assert
    Assert.AreEqual(default(int), result);
    Assert.AreEqual(0, result);
  }

  [Test]
  public void GetValueOrDefault_WithCustomDefault_ExistingKey_ReturnsValue() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act
    var result = dictionary.GetValueOrDefault("key1", -1);

    // Assert
    Assert.AreEqual(42, result);
  }

  [Test]
  public void GetValueOrDefault_WithCustomDefault_NonExistentKey_ReturnsCustomDefault() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act
    var result = dictionary.GetValueOrDefault("nonexistent", -1);

    // Assert
    Assert.AreEqual(-1, result);
  }

  [Test]
  public void GetValueOrDefault_ReferenceType_NonExistentKey_ReturnsNull() {
    // Arrange
    var dictionary = new Dictionary<string, string> { { "key1", "value1" } };

    // Act
    var result = dictionary.GetValueOrDefault("nonexistent");

    // Assert
    Assert.IsNull(result);
  }

  #endregion

  #region Performance and Edge Case Tests

  [Test]
  public void AddRange_LargeNumberOfItems_PerformanceTest() {
    // Arrange
    var dictionary = new Dictionary<int, string>();
    const int itemCount = 10000;
    var pairs = Enumerable
      .Range(0, itemCount)
      .Select(i => new KeyValuePair<int, string>(i, $"value_{i}"))
      .ToArray();

    // Act
    dictionary.AddRange(pairs);

    // Assert
    Assert.AreEqual(itemCount, dictionary.Count);
    Assert.AreEqual("value_0", dictionary[0]);
    Assert.AreEqual($"value_{itemCount - 1}", dictionary[itemCount - 1]);
  }

  [Test]
  public void HasKeyDo_ExceptionInAction_PropagatesException() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act & Assert
    Assert.Throws<InvalidOperationException>(
      () =>
        dictionary.HasKeyDo("key1", (key, value) => throw new InvalidOperationException("Test exception"))
    );
  }

  [Test]
  public void GetValueOrDefault_NullKey_HandledGracefully() {
    // Arrange
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    // Act & Assert
    // For Dictionary<string, int>, null keys typically throw ArgumentNullException
    Assert.Throws<ArgumentNullException>(() => dictionary.GetValueOrDefault(null));
  }

  [Test]
  public void AddRange_MixedTypes_WithObjectDictionary_WorksCorrectly() {
    // Arrange
    var dictionary = new Dictionary<object, object>();

    // Act
    dictionary.AddRange(
      1,
      "one",
      "two",
      2,
      DateTime.Today,
      "today",
      true,
      false
    );

    // Assert
    Assert.AreEqual(4, dictionary.Count);
    Assert.AreEqual("one", dictionary[1]);
    Assert.AreEqual(2, dictionary["two"]);
    Assert.AreEqual("today", dictionary[DateTime.Today]);
    Assert.AreEqual(false, dictionary[true]);
  }

  [Test]
  public void GetValueOrDefault_ValueType_WithNullableDefault_WorksCorrectly() {
    // Arrange
    var dictionary = new Dictionary<string, int?> { { "key1", 42 } };

    // Act
    var existingResult = dictionary.GetValueOrDefault("key1");
    var nonExistentResult = dictionary.GetValueOrDefault("nonexistent");
    var customDefaultResult = dictionary.GetValueOrDefault("nonexistent", -1);

    // Assert
    Assert.AreEqual(42, existingResult);
    Assert.IsNull(nonExistentResult);
    Assert.AreEqual(-1, customDefaultResult);
  }

  [Test]
  public void Dictionary_StateConsistency_AfterMultipleOperations() {
    // Arrange
    var dictionary = new Dictionary<string, int>();

    // Act - Mixed operations
    dictionary.AddRange("a", 1, "b", 2);
    dictionary["c"] = 3;
    var hasKey = dictionary.HasKeyDo("b", value => { });
    var defaultValue = dictionary.GetValueOrDefault("d", 4);
    dictionary.AddRange(new[] { new KeyValuePair<string, int>("e", 5) });

    // Assert
    Assert.AreEqual(4, dictionary.Count);
    Assert.AreEqual(1, dictionary["a"]);
    Assert.AreEqual(2, dictionary["b"]);
    Assert.AreEqual(3, dictionary["c"]);
    Assert.AreEqual(5, dictionary["e"]);
    Assert.IsTrue(hasKey);
    Assert.AreEqual(4, defaultValue);
    Assert.IsFalse(dictionary.ContainsKey("d"));
  }

  #endregion
}
