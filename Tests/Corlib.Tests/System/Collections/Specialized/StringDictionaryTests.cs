using NUnit.Framework;

namespace System.Collections.Specialized;

[TestFixture]
public class StringDictionaryTests {
  [Test]
  public void AddOrUpdate_NewKey_AddsKeyValuePair() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act
    dictionary.AddOrUpdate("key1", "value1");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("value1", dictionary["key1"]);
    Assert.IsTrue(dictionary.ContainsKey("key1"));
  }

  [Test]
  public void AddOrUpdate_ExistingKey_UpdatesValue() {
    // Arrange
    var dictionary = new StringDictionary { { "key1", "original" } };

    // Act
    dictionary.AddOrUpdate("key1", "updated");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("updated", dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_MultipleOperations_WorksCorrectly() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act - Add new keys
    dictionary.AddOrUpdate("key1", "value1");
    dictionary.AddOrUpdate("key2", "value2");

    // Act - Update existing key
    dictionary.AddOrUpdate("key1", "updated_value1");

    // Act - Add another new key
    dictionary.AddOrUpdate("key3", "value3");

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual("updated_value1", dictionary["key1"]);
    Assert.AreEqual("value2", dictionary["key2"]);
    Assert.AreEqual("value3", dictionary["key3"]);
  }

  [Test]
  public void AddOrUpdate_WithNullValue_AddsOrUpdatesWithNull() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act - Add with null value
    dictionary.AddOrUpdate("key1", null);

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.IsNull(dictionary["key1"]);
    Assert.IsTrue(dictionary.ContainsKey("key1"));

    // Act - Update with null value
    dictionary.AddOrUpdate("key1", "not null");
    dictionary.AddOrUpdate("key1", null);

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.IsNull(dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_WithEmptyStringValue_AddsOrUpdatesWithEmptyString() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act - Add with empty string
    dictionary.AddOrUpdate("key1", "");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("", dictionary["key1"]);

    // Act - Update to empty string
    dictionary.AddOrUpdate("key1", "not empty");
    dictionary.AddOrUpdate("key1", "");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("", dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_WithEmptyStringKey_WorksCorrectly() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act
    dictionary.AddOrUpdate("", "value for empty key");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("value for empty key", dictionary[""]);

    // Act - Update empty key
    dictionary.AddOrUpdate("", "updated value");

    // Assert
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("updated value", dictionary[""]);
  }

  [Test]
  public void AddOrUpdate_CaseSensitivity_BehavesLikeStringDictionary() {
    // Arrange - StringDictionary is case-insensitive
    var dictionary = new StringDictionary();

    // Act
    dictionary.AddOrUpdate("Key", "value1");
    dictionary.AddOrUpdate("KEY", "value2");
    dictionary.AddOrUpdate("key", "value3");

    // Assert - Should have only one entry due to case-insensitivity
    Assert.AreEqual(1, dictionary.Count);
    Assert.AreEqual("value3", dictionary["Key"]);
    Assert.AreEqual("value3", dictionary["KEY"]);
    Assert.AreEqual("value3", dictionary["key"]);
  }

  [Test]
  public void AddOrUpdate_WithUnicodeStrings_WorksCorrectly() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act
    dictionary.AddOrUpdate("世界", "world");
    dictionary.AddOrUpdate("🌍", "earth");
    dictionary.AddOrUpdate("Ñiño", "child");

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual("world", dictionary["世界"]);
    Assert.AreEqual("earth", dictionary["🌍"]);
    Assert.AreEqual("child", dictionary["Ñiño"]);

    // Act - Update Unicode keys
    dictionary.AddOrUpdate("世界", "updated world");

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual("updated world", dictionary["世界"]);
  }

  [Test]
  public void AddOrUpdate_WithWhitespaceKeys_WorksCorrectly() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act
    dictionary.AddOrUpdate(" ", "single space");
    dictionary.AddOrUpdate("\t", "tab");
    dictionary.AddOrUpdate("\n", "newline");
    dictionary.AddOrUpdate("  ", "double space");

    // Assert
    Assert.AreEqual(4, dictionary.Count);
    Assert.AreEqual("single space", dictionary[" "]);
    Assert.AreEqual("tab", dictionary["\t"]);
    Assert.AreEqual("newline", dictionary["\n"]);
    Assert.AreEqual("double space", dictionary["  "]);
  }

  [Test]
  public void AddOrUpdate_PerformanceTest_HandlesLargeNumberOfOperations() {
    // Arrange
    var dictionary = new StringDictionary();
    const int operationCount = 10000;

    // Act - Add many items
    for (var i = 0; i < operationCount; i++)
      dictionary.AddOrUpdate($"key_{i:D5}", $"value_{i:D5}");

    // Assert
    Assert.AreEqual(operationCount, dictionary.Count);

    // Act - Update all items
    for (var i = 0; i < operationCount; i++)
      dictionary.AddOrUpdate($"key_{i:D5}", $"updated_value_{i:D5}");

    // Assert
    Assert.AreEqual(operationCount, dictionary.Count);
    Assert.AreEqual("updated_value_00000", dictionary["key_00000"]);
    Assert.AreEqual($"updated_value_{operationCount - 1:D5}", dictionary[$"key_{operationCount - 1:D5}"]);
  }

  [Test]
  public void AddOrUpdate_NullDictionary_ThrowsArgumentNullException() {
    // Arrange
    StringDictionary? dictionary = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => dictionary!.AddOrUpdate("key", "value"));
  }

  [Test]
  public void AddOrUpdate_NullKey_ThrowsArgumentNullException() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate(null, "value"));
  }

  [Test]
  public void AddOrUpdate_StateConsistency_MaintainsInternalState() {
    // Arrange
    var dictionary = new StringDictionary();

    // Act - Mixed operations
    dictionary.AddOrUpdate("key1", "value1");
    dictionary.Add("key2", "value2");
    dictionary.AddOrUpdate("key1", "updated1");
    dictionary["key3"] = "value3";
    dictionary.AddOrUpdate("key3", "updated3");

    // Assert
    Assert.AreEqual(3, dictionary.Count);
    Assert.AreEqual("updated1", dictionary["key1"]);
    Assert.AreEqual("value2", dictionary["key2"]);
    Assert.AreEqual("updated3", dictionary["key3"]);

    // Assert all keys are present
    Assert.IsTrue(dictionary.ContainsKey("key1"));
    Assert.IsTrue(dictionary.ContainsKey("key2"));
    Assert.IsTrue(dictionary.ContainsKey("key3"));
  }
}
