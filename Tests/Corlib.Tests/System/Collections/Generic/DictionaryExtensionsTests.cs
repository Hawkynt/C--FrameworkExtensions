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
    Assert.Throws<ArgumentNullException>(() => dictionary.AddRange((IEnumerable<KeyValuePair<string, int>>)null!));
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
    Assert.Throws<ArgumentNullException>(() => dictionary.HasKeyDo("key1", (Action<string, int>)null!));
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

  #region IncrementOrAdd Tests

  [Test]
  public void IncrementOrAdd_UInt_NewKey_AddsWithValue1() {
    var dictionary = new Dictionary<string, uint>();

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(1u, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_UInt_ExistingKey_IncrementsValue() {
    var dictionary = new Dictionary<string, uint> { { "key1", 5u } };

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(6u, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_UInt_MultipleIncrements_AccumulatesCorrectly() {
    var dictionary = new Dictionary<string, uint>();

    for (var i = 0; i < 100; ++i)
      dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(100u, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Int_NewKey_AddsWithValue1() {
    var dictionary = new Dictionary<string, int>();

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(1, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Int_ExistingKey_IncrementsValue() {
    var dictionary = new Dictionary<string, int> { { "key1", -5 } };

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(-4, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Long_NewKey_AddsWithValue1() {
    var dictionary = new Dictionary<string, long>();

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(1L, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Long_ExistingKey_IncrementsValue() {
    var dictionary = new Dictionary<string, long> { { "key1", long.MaxValue - 1 } };

    dictionary.IncrementOrAdd("key1");

    Assert.AreEqual(long.MaxValue, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_UInt_WithAmount_NewKey_AddsWithAmount() {
    var dictionary = new Dictionary<string, uint>();

    dictionary.IncrementOrAdd("key1", 10u);

    Assert.AreEqual(10u, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_UInt_WithAmount_ExistingKey_AddsAmount() {
    var dictionary = new Dictionary<string, uint> { { "key1", 5u } };

    dictionary.IncrementOrAdd("key1", 10u);

    Assert.AreEqual(15u, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Int_WithAmount_NewKey_AddsWithAmount() {
    var dictionary = new Dictionary<string, int>();

    dictionary.IncrementOrAdd("key1", -5);

    Assert.AreEqual(-5, dictionary["key1"]);
  }

  [Test]
  public void IncrementOrAdd_Int_WithAmount_ExistingKey_AddsAmount() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    dictionary.IncrementOrAdd("key1", -3);

    Assert.AreEqual(7, dictionary["key1"]);
  }

  #endregion

  #region GetOrAdd Tests

  [Test]
  public void GetOrAdd_WithFactory_NewKey_CallsFactoryAndAddsValue() {
    var dictionary = new Dictionary<string, int>();
    var factoryCalled = false;

    var result = dictionary.GetOrAdd("key1", k => {
      factoryCalled = true;
      return k.Length * 10;
    });

    Assert.IsTrue(factoryCalled);
    Assert.AreEqual(40, result); // "key1".Length * 10 = 4 * 10 = 40
    Assert.AreEqual(40, dictionary["key1"]);
  }

  [Test]
  public void GetOrAdd_WithFactory_ExistingKey_DoesNotCallFactory() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };
    var factoryCalled = false;

    var result = dictionary.GetOrAdd("key1", k => {
      factoryCalled = true;
      return 999;
    });

    Assert.IsFalse(factoryCalled);
    Assert.AreEqual(42, result);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void GetOrAdd_WithDefaultValue_NewKey_AddsDefaultValue() {
    var dictionary = new Dictionary<string, int>();

    var result = dictionary.GetOrAdd("key1", 42);

    Assert.AreEqual(42, result);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void GetOrAdd_WithDefaultValue_ExistingKey_ReturnsExistingValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    var result = dictionary.GetOrAdd("key1", 42);

    Assert.AreEqual(100, result);
    Assert.AreEqual(100, dictionary["key1"]);
  }

  [Test]
  public void GetOrAdd_NullFactory_ThrowsArgumentNullException() {
    var dictionary = new Dictionary<string, int>();

    Assert.Throws<ArgumentNullException>(() => dictionary.GetOrAdd("key1", (Func<string, int>)null!));
  }

  #endregion

  #region GetOrAdd Regression Tests (CollectionsMarshal polyfill)

  [Test]
  [Category("Regression")]
  [Description("Regression test for CollectionsMarshal polyfill bug where keys with hash code 0 (like '\\0') were not found after adding")]
  public void GetOrAdd_WithNullCharKey_WorksCorrectly() {
    var dictionary = new Dictionary<char, int>();

    var result = dictionary.GetOrAdd('\0', 42);

    Assert.AreEqual(42, result);
    Assert.AreEqual(42, dictionary['\0']);
    Assert.AreEqual(1, dictionary.Count);
  }

  [Test]
  [Category("Regression")]
  [Description("Regression test for CollectionsMarshal polyfill - multiple operations with null char key")]
  public void GetOrAdd_WithNullCharKey_MultipleOperations_WorksCorrectly() {
    var dictionary = new Dictionary<char, int>();

    var result1 = dictionary.GetOrAdd('\0', 42);
    var result2 = dictionary.GetOrAdd('\0', 100);
    var result3 = dictionary.GetOrAdd('a', 200);

    Assert.AreEqual(42, result1);
    Assert.AreEqual(42, result2);
    Assert.AreEqual(200, result3);
    Assert.AreEqual(2, dictionary.Count);
    Assert.AreEqual(42, dictionary['\0']);
    Assert.AreEqual(200, dictionary['a']);
  }

  [Test]
  [Category("Regression")]
  [Description("Regression test for CollectionsMarshal polyfill - factory with null char key")]
  public void GetOrAdd_WithFactory_NullCharKey_WorksCorrectly() {
    var dictionary = new Dictionary<char, string>();
    var factoryCalled = false;

    var result = dictionary.GetOrAdd('\0', k => {
      factoryCalled = true;
      return $"value for '{(int)k}'";
    });

    Assert.IsTrue(factoryCalled);
    Assert.AreEqual("value for '0'", result);
    Assert.AreEqual("value for '0'", dictionary['\0']);
  }

  [Test]
  [Category("Regression")]
  [Description("Regression test for CollectionsMarshal polyfill - IncrementOrAdd with null char key")]
  public void IncrementOrAdd_WithNullCharKey_WorksCorrectly() {
    var dictionary = new Dictionary<char, int>();

    dictionary.IncrementOrAdd('\0');
    dictionary.IncrementOrAdd('\0');
    dictionary.IncrementOrAdd('\0');

    Assert.AreEqual(3, dictionary['\0']);
  }

  [Test]
  [Category("Regression")]
  [Description("Regression test for CollectionsMarshal polyfill - mixed operations with keys hashing to same bucket")]
  public void GetOrAdd_MixedOperations_WithVariousKeys_WorksCorrectly() {
    var dictionary = new Dictionary<char, int>();

    dictionary.GetOrAdd('\0', 1);
    dictionary.GetOrAdd('A', 2);
    dictionary.GetOrAdd('\t', 3);
    dictionary.IncrementOrAdd('\0');
    dictionary.IncrementOrAdd('A');

    Assert.AreEqual(2, dictionary['\0']);
    Assert.AreEqual(3, dictionary['A']);
    Assert.AreEqual(3, dictionary['\t']);
    Assert.AreEqual(3, dictionary.Count);
  }

  #endregion

  #region AddOrUpdate with Factory Tests

  [Test]
  public void AddOrUpdate_WithFactories_NewKey_UsesAddValue() {
    var dictionary = new Dictionary<string, int>();

    var result = dictionary.AddOrUpdate("key1", 42, (k, v) => v + 100);

    Assert.AreEqual(42, result);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_WithFactories_ExistingKey_UsesUpdateFactory() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    var result = dictionary.AddOrUpdate("key1", 42, (k, v) => v + 100);

    Assert.AreEqual(110, result);
    Assert.AreEqual(110, dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_WithBothFactories_NewKey_UsesAddFactory() {
    var dictionary = new Dictionary<string, int>();
    var addFactoryCalled = false;
    var updateFactoryCalled = false;

    var result = dictionary.AddOrUpdate(
      "key1",
      k => {
        addFactoryCalled = true;
        return k.Length;
      },
      (k, v) => {
        updateFactoryCalled = true;
        return v + 1;
      });

    Assert.IsTrue(addFactoryCalled);
    Assert.IsFalse(updateFactoryCalled);
    Assert.AreEqual(4, result); // "key1".Length = 4
    Assert.AreEqual(4, dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_WithBothFactories_ExistingKey_UsesUpdateFactory() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };
    var addFactoryCalled = false;
    var updateFactoryCalled = false;

    var result = dictionary.AddOrUpdate(
      "key1",
      k => {
        addFactoryCalled = true;
        return k.Length;
      },
      (k, v) => {
        updateFactoryCalled = true;
        return v * 2;
      });

    Assert.IsFalse(addFactoryCalled);
    Assert.IsTrue(updateFactoryCalled);
    Assert.AreEqual(20, result); // 10 * 2 = 20
    Assert.AreEqual(20, dictionary["key1"]);
  }

  [Test]
  public void AddOrUpdate_NullUpdateFactory_ThrowsArgumentNullException() {
    var dictionary = new Dictionary<string, int>();

    Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate("key1", 42, (Func<string, int, int>)null!));
  }

  [Test]
  public void AddOrUpdate_NullAddFactory_ThrowsArgumentNullException() {
    var dictionary = new Dictionary<string, int>();

    Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate("key1", (Func<string, int>)null!, (k, v) => v));
  }

  #endregion

  #region DecrementOrAdd Tests

  [Test]
  public void DecrementOrAdd_Int_NewKey_AddsWithValueMinusOne() {
    var dictionary = new Dictionary<string, int>();

    dictionary.DecrementOrAdd("key1");

    Assert.AreEqual(-1, dictionary["key1"]);
  }

  [Test]
  public void DecrementOrAdd_Int_ExistingKey_DecrementsValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 5 } };

    dictionary.DecrementOrAdd("key1");

    Assert.AreEqual(4, dictionary["key1"]);
  }

  [Test]
  public void DecrementOrAdd_Int_WithAmount_NewKey_SubtractsAmount() {
    var dictionary = new Dictionary<string, int>();

    dictionary.DecrementOrAdd("key1", 10);

    Assert.AreEqual(-10, dictionary["key1"]);
  }

  [Test]
  public void DecrementOrAdd_Int_WithAmount_ExistingKey_SubtractsAmount() {
    var dictionary = new Dictionary<string, int> { { "key1", 15 } };

    dictionary.DecrementOrAdd("key1", 10);

    Assert.AreEqual(5, dictionary["key1"]);
  }

  [Test]
  public void DecrementOrAdd_UInt_NewKey_WrapsAround() {
    var dictionary = new Dictionary<string, uint>();

    dictionary.DecrementOrAdd("key1");

    Assert.AreEqual(uint.MaxValue, dictionary["key1"]);
  }

  [Test]
  public void DecrementOrAdd_Double_ExistingKey_DecrementsValue() {
    var dictionary = new Dictionary<string, double> { { "key1", 10.5 } };

    dictionary.DecrementOrAdd("key1", 0.5);

    Assert.AreEqual(10.0, dictionary["key1"], 1e-10);
  }

  #endregion

  #region ExchangeOrAdd Tests

  [Test]
  public void ExchangeOrAdd_Int_NewKey_ReturnsDefaultAndSetsValue() {
    var dictionary = new Dictionary<string, int>();

    var oldValue = dictionary.ExchangeOrAdd("key1", 42);

    Assert.AreEqual(0, oldValue);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void ExchangeOrAdd_Int_ExistingKey_ReturnsOldAndSetsNew() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    var oldValue = dictionary.ExchangeOrAdd("key1", 42);

    Assert.AreEqual(100, oldValue);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void ExchangeOrAdd_Double_ExistingKey_ReturnsOldAndSetsNew() {
    var dictionary = new Dictionary<string, double> { { "key1", 3.14 } };

    var oldValue = dictionary.ExchangeOrAdd("key1", 2.71);

    Assert.AreEqual(3.14, oldValue, 1e-10);
    Assert.AreEqual(2.71, dictionary["key1"], 1e-10);
  }

  [Test]
  public void ExchangeOrAdd_Long_MultipleExchanges_TracksCorrectly() {
    var dictionary = new Dictionary<string, long>();

    var first = dictionary.ExchangeOrAdd("key1", 10L);
    var second = dictionary.ExchangeOrAdd("key1", 20L);
    var third = dictionary.ExchangeOrAdd("key1", 30L);

    Assert.AreEqual(0L, first);
    Assert.AreEqual(10L, second);
    Assert.AreEqual(20L, third);
    Assert.AreEqual(30L, dictionary["key1"]);
  }

  #endregion

  #region CompareExchangeOrAdd Tests

  [Test]
  public void CompareExchangeOrAdd_Int_NewKey_ComparesWithDefault_Succeeds() {
    var dictionary = new Dictionary<string, int>();

    var oldValue = dictionary.CompareExchangeOrAdd("key1", 42, 0);

    Assert.AreEqual(0, oldValue);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void CompareExchangeOrAdd_Int_NewKey_ComparesWithNonDefault_Fails() {
    var dictionary = new Dictionary<string, int>();

    var oldValue = dictionary.CompareExchangeOrAdd("key1", 42, 100);

    Assert.AreEqual(0, oldValue);
    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void CompareExchangeOrAdd_Int_ExistingKey_MatchingComparand_Succeeds() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    var oldValue = dictionary.CompareExchangeOrAdd("key1", 42, 100);

    Assert.AreEqual(100, oldValue);
    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void CompareExchangeOrAdd_Int_ExistingKey_NonMatchingComparand_Fails() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    var oldValue = dictionary.CompareExchangeOrAdd("key1", 42, 50);

    Assert.AreEqual(100, oldValue);
    Assert.AreEqual(100, dictionary["key1"]);
  }

  [Test]
  public void CompareExchangeOrAdd_Long_SpinUntilSuccess_WorksCorrectly() {
    var dictionary = new Dictionary<string, long> { { "key1", 10L } };
    var iterations = 0;
    var oldValue = 10L; // Known initial value

    do {
      var result = dictionary.CompareExchangeOrAdd("key1", oldValue + 1, oldValue);
      ++iterations;
      if (result == oldValue)
        break; // Exchange succeeded
      oldValue = result; // Update for next attempt
    } while (iterations < 10);

    Assert.AreEqual(1, iterations);
    Assert.AreEqual(11L, dictionary["key1"]);
  }

  #endregion

  #region MultiplyOrSet Tests

  [Test]
  public void MultiplyOrSet_Int_NewKey_SetsToFactor() {
    var dictionary = new Dictionary<string, int>();

    dictionary.MultiplyOrSet("key1", 5);

    Assert.AreEqual(5, dictionary["key1"]);
  }

  [Test]
  public void MultiplyOrSet_Int_ExistingKey_MultipliesValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    dictionary.MultiplyOrSet("key1", 5);

    Assert.AreEqual(50, dictionary["key1"]);
  }

  [Test]
  public void MultiplyOrSet_Double_ExistingKey_MultipliesValue() {
    var dictionary = new Dictionary<string, double> { { "key1", 2.5 } };

    dictionary.MultiplyOrSet("key1", 4.0);

    Assert.AreEqual(10.0, dictionary["key1"], 1e-10);
  }

  [Test]
  public void MultiplyOrSet_Int_ExistingZero_RemainsZero() {
    var dictionary = new Dictionary<string, int> { { "key1", 0 } };

    dictionary.MultiplyOrSet("key1", 100);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  #endregion

  #region MaxOrAdd Tests

  [Test]
  public void MaxOrAdd_Int_NewKey_AddsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.MaxOrAdd("key1", 42);

    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void MaxOrAdd_Int_ExistingKey_ValueGreater_UpdatesToNewValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    dictionary.MaxOrAdd("key1", 42);

    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void MaxOrAdd_Int_ExistingKey_ValueSmaller_KeepsExisting() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    dictionary.MaxOrAdd("key1", 42);

    Assert.AreEqual(100, dictionary["key1"]);
  }

  [Test]
  public void MaxOrAdd_Double_TracksMaximum() {
    var dictionary = new Dictionary<string, double>();
    var values = new[] { 3.0, 1.0, 4.0, 1.0, 5.0, 9.0, 2.0, 6.0 };

    foreach (var v in values)
      dictionary.MaxOrAdd("key1", v);

    Assert.AreEqual(9.0, dictionary["key1"], 1e-10);
  }

  #endregion

  #region MinOrAdd Tests

  [Test]
  public void MinOrAdd_Int_NewKey_AddsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.MinOrAdd("key1", 42);

    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void MinOrAdd_Int_ExistingKey_ValueSmaller_UpdatesToNewValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    dictionary.MinOrAdd("key1", 42);

    Assert.AreEqual(42, dictionary["key1"]);
  }

  [Test]
  public void MinOrAdd_Int_ExistingKey_ValueGreater_KeepsExisting() {
    var dictionary = new Dictionary<string, int> { { "key1", 10 } };

    dictionary.MinOrAdd("key1", 42);

    Assert.AreEqual(10, dictionary["key1"]);
  }

  [Test]
  public void MinOrAdd_Double_TracksMinimum() {
    var dictionary = new Dictionary<string, double>();
    var values = new[] { 3.0, 1.0, 4.0, 1.0, 5.0, 9.0, 2.0, 6.0 };

    foreach (var v in values)
      dictionary.MinOrAdd("key1", v);

    Assert.AreEqual(1.0, dictionary["key1"], 1e-10);
  }

  #endregion

  #region OrOrAdd Tests (Bitwise OR)

  [Test]
  public void OrOrAdd_Int_NewKey_AddsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.OrOrAdd("key1", 0b1010);

    Assert.AreEqual(0b1010, dictionary["key1"]);
  }

  [Test]
  public void OrOrAdd_Int_ExistingKey_OrsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1100 } };

    dictionary.OrOrAdd("key1", 0b1010);

    Assert.AreEqual(0b1110, dictionary["key1"]);
  }

  [Test]
  public void OrOrAdd_Byte_AccumulatesFlags() {
    var dictionary = new Dictionary<string, byte>();

    dictionary.OrOrAdd("flags", 0x01);
    dictionary.OrOrAdd("flags", 0x02);
    dictionary.OrOrAdd("flags", 0x04);

    Assert.AreEqual(0x07, dictionary["flags"]);
  }

  #endregion

  #region AndOrAdd Tests (Bitwise AND)

  [Test]
  public void AndOrAdd_Int_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, int>();

    dictionary.AndOrAdd("key1", 0b1111);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void AndOrAdd_Int_ExistingKey_AndsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1110 } };

    dictionary.AndOrAdd("key1", 0b1010);

    Assert.AreEqual(0b1010, dictionary["key1"]);
  }

  [Test]
  public void AndOrAdd_Byte_MasksValue() {
    var dictionary = new Dictionary<string, byte> { { "value", 0xFF } };

    dictionary.AndOrAdd("value", 0x0F);

    Assert.AreEqual(0x0F, dictionary["value"]);
  }

  #endregion

  #region XorOrAdd Tests (Bitwise XOR)

  [Test]
  public void XorOrAdd_Int_NewKey_AddsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.XorOrAdd("key1", 0b1010);

    Assert.AreEqual(0b1010, dictionary["key1"]);
  }

  [Test]
  public void XorOrAdd_Int_ExistingKey_XorsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1100 } };

    dictionary.XorOrAdd("key1", 0b1010);

    Assert.AreEqual(0b0110, dictionary["key1"]);
  }

  [Test]
  public void XorOrAdd_Byte_TogglesBits() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b11110000 } };

    dictionary.XorOrAdd("value", 0b10101010);

    Assert.AreEqual(0b01011010, dictionary["value"]);
  }

  [Test]
  public void XorOrAdd_Int_DoubleXor_RestoresOriginal() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    dictionary.XorOrAdd("key1", 0xFF);
    dictionary.XorOrAdd("key1", 0xFF);

    Assert.AreEqual(42, dictionary["key1"]);
  }

  #endregion

  #region DivideOrSet Tests

  [Test]
  public void DivideOrSet_Int_NewKey_SetsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.DivideOrSet("key1", 5);

    Assert.AreEqual(5, dictionary["key1"]);
  }

  [Test]
  public void DivideOrSet_Int_ExistingKey_DividesValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 100 } };

    dictionary.DivideOrSet("key1", 5);

    Assert.AreEqual(20, dictionary["key1"]);
  }

  [Test]
  public void DivideOrSet_Double_ExistingKey_DividesValue() {
    var dictionary = new Dictionary<string, double> { { "key1", 10.0 } };

    dictionary.DivideOrSet("key1", 4.0);

    Assert.AreEqual(2.5, dictionary["key1"], 1e-10);
  }

  [Test]
  public void DivideOrSet_Int_MultipleDivisions() {
    var dictionary = new Dictionary<string, int> { { "key1", 1000 } };

    dictionary.DivideOrSet("key1", 10);
    dictionary.DivideOrSet("key1", 5);
    dictionary.DivideOrSet("key1", 2);

    Assert.AreEqual(10, dictionary["key1"]);
  }

  #endregion

  #region ModuloOrSet Tests

  [Test]
  public void ModuloOrSet_Int_NewKey_SetsValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.ModuloOrSet("key1", 7);

    Assert.AreEqual(7, dictionary["key1"]);
  }

  [Test]
  public void ModuloOrSet_Int_ExistingKey_ComputesModulo() {
    var dictionary = new Dictionary<string, int> { { "key1", 17 } };

    dictionary.ModuloOrSet("key1", 5);

    Assert.AreEqual(2, dictionary["key1"]);
  }

  [Test]
  public void ModuloOrSet_Int_ExactDivisor_ReturnsZero() {
    var dictionary = new Dictionary<string, int> { { "key1", 20 } };

    dictionary.ModuloOrSet("key1", 5);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void ModuloOrSet_Double_ComputesRemainder() {
    var dictionary = new Dictionary<string, double> { { "key1", 7.5 } };

    dictionary.ModuloOrSet("key1", 2.0);

    Assert.AreEqual(1.5, dictionary["key1"], 1e-10);
  }

  #endregion

  #region NandOrAdd Tests (Bitwise NAND)

  [Test]
  public void NandOrAdd_Int_NewKey_AddsNotValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.NandOrAdd("key1", 0b1010);

    Assert.AreEqual(~0b1010, dictionary["key1"]);
  }

  [Test]
  public void NandOrAdd_Int_ExistingKey_NandsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1100 } };

    dictionary.NandOrAdd("key1", 0b1010);

    Assert.AreEqual(~(0b1100 & 0b1010), dictionary["key1"]);
  }

  [Test]
  public void NandOrAdd_Byte_ComputesNand() {
    var dictionary = new Dictionary<string, byte> { { "value", 0xFF } };

    dictionary.NandOrAdd("value", 0x0F);

    Assert.AreEqual(0xF0, dictionary["value"]); // ~(0xFF & 0x0F) = ~0x0F = 0xF0 (byte)
  }

  #endregion

  #region NorOrAdd Tests (Bitwise NOR)

  [Test]
  public void NorOrAdd_Int_NewKey_AddsNotValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.NorOrAdd("key1", 0b1010);

    Assert.AreEqual(~0b1010, dictionary["key1"]);
  }

  [Test]
  public void NorOrAdd_Int_ExistingKey_NorsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1100 } };

    dictionary.NorOrAdd("key1", 0b1010);

    Assert.AreEqual(~(0b1100 | 0b1010), dictionary["key1"]);
  }

  [Test]
  public void NorOrAdd_Byte_ComputesNor() {
    var dictionary = new Dictionary<string, byte> { { "value", 0xF0 } };

    dictionary.NorOrAdd("value", 0x0F);

    Assert.AreEqual(0x00, dictionary["value"]); // ~(0xF0 | 0x0F) = ~0xFF = 0x00 (byte)
  }

  #endregion

  #region XnorOrAdd Tests (Bitwise XNOR)

  [Test]
  public void XnorOrAdd_Int_NewKey_AddsNotValue() {
    var dictionary = new Dictionary<string, int>();

    dictionary.XnorOrAdd("key1", 0b1010);

    Assert.AreEqual(~0b1010, dictionary["key1"]);
  }

  [Test]
  public void XnorOrAdd_Int_ExistingKey_XnorsValues() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1100 } };

    dictionary.XnorOrAdd("key1", 0b1010);

    Assert.AreEqual(~(0b1100 ^ 0b1010), dictionary["key1"]);
  }

  [Test]
  public void XnorOrAdd_Int_SameValues_AllOnes() {
    var dictionary = new Dictionary<string, int> { { "key1", 0b1010 } };

    dictionary.XnorOrAdd("key1", 0b1010);

    Assert.AreEqual(~0, dictionary["key1"]);
  }

  #endregion

  #region NotOrSet Tests (Bitwise NOT)

  [Test]
  public void NotOrSet_Int_NewKey_SetsAllOnes() {
    var dictionary = new Dictionary<string, int>();

    dictionary.NotOrSet("key1");

    Assert.AreEqual(-1, dictionary["key1"]);
  }

  [Test]
  public void NotOrSet_Int_ExistingKey_InvertsValue() {
    var dictionary = new Dictionary<string, int> { { "key1", 0 } };

    dictionary.NotOrSet("key1");

    Assert.AreEqual(-1, dictionary["key1"]);
  }

  [Test]
  public void NotOrSet_Byte_InvertsValue() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b10101010 } };

    dictionary.NotOrSet("value");

    Assert.AreEqual(0b01010101, dictionary["value"]);
  }

  [Test]
  public void NotOrSet_Int_DoubleNot_RestoresOriginal() {
    var dictionary = new Dictionary<string, int> { { "key1", 42 } };

    dictionary.NotOrSet("key1");
    dictionary.NotOrSet("key1");

    Assert.AreEqual(42, dictionary["key1"]);
  }

  #endregion

  #region LeftShiftOrAdd Tests

  [Test]
  public void LeftShiftOrAdd_Int_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, int>();

    dictionary.LeftShiftOrAdd("key1", 3);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void LeftShiftOrAdd_Int_ExistingKey_ShiftsLeft() {
    var dictionary = new Dictionary<string, int> { { "key1", 1 } };

    dictionary.LeftShiftOrAdd("key1", 4);

    Assert.AreEqual(16, dictionary["key1"]);
  }

  [Test]
  public void LeftShiftOrAdd_Byte_ShiftsLeft() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b00001111 } };

    dictionary.LeftShiftOrAdd("value", 4);

    Assert.AreEqual(0b11110000, dictionary["value"]);
  }

  #endregion

  #region RightShiftOrAdd Tests

  [Test]
  public void RightShiftOrAdd_Int_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, int>();

    dictionary.RightShiftOrAdd("key1", 3);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void RightShiftOrAdd_Int_ExistingKey_ShiftsRight() {
    var dictionary = new Dictionary<string, int> { { "key1", 16 } };

    dictionary.RightShiftOrAdd("key1", 4);

    Assert.AreEqual(1, dictionary["key1"]);
  }

  [Test]
  public void RightShiftOrAdd_Int_SignedNegative_ArithmeticShift() {
    var dictionary = new Dictionary<string, int> { { "key1", -16 } };

    dictionary.RightShiftOrAdd("key1", 2);

    Assert.AreEqual(-4, dictionary["key1"]);
  }

  [Test]
  public void RightShiftOrAdd_Byte_ShiftsRight() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b11110000 } };

    dictionary.RightShiftOrAdd("value", 4);

    Assert.AreEqual(0b00001111, dictionary["value"]);
  }

  #endregion

  #region UnsignedRightShiftOrAdd Tests

  [Test]
  public void UnsignedRightShiftOrAdd_Int_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, int>();

    dictionary.UnsignedRightShiftOrAdd("key1", 3);

    Assert.AreEqual(0, dictionary["key1"]);
  }

  [Test]
  public void UnsignedRightShiftOrAdd_Int_ExistingKey_ShiftsRight() {
    var dictionary = new Dictionary<string, int> { { "key1", 16 } };

    dictionary.UnsignedRightShiftOrAdd("key1", 4);

    Assert.AreEqual(1, dictionary["key1"]);
  }

  [Test]
  public void UnsignedRightShiftOrAdd_Int_SignedNegative_LogicalShift() {
    var dictionary = new Dictionary<string, int> { { "key1", -1 } };

    dictionary.UnsignedRightShiftOrAdd("key1", 1);

    Assert.AreEqual(int.MaxValue, dictionary["key1"]);
  }

  #endregion

  #region RotateLeftOrAdd Tests

  [Test]
  public void RotateLeftOrAdd_UInt_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, uint>();

    dictionary.RotateLeftOrAdd("key1", 5);

    Assert.AreEqual(0u, dictionary["key1"]);
  }

  [Test]
  public void RotateLeftOrAdd_Byte_RotatesLeft() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b10000001 } };

    dictionary.RotateLeftOrAdd("value", 1);

    Assert.AreEqual(0b00000011, dictionary["value"]);
  }

  [Test]
  public void RotateLeftOrAdd_UInt_RotatesLeft() {
    var dictionary = new Dictionary<string, uint> { { "value", 0x80000001 } };

    dictionary.RotateLeftOrAdd("value", 1);

    Assert.AreEqual(0x00000003u, dictionary["value"]);
  }

  [Test]
  public void RotateLeftOrAdd_UInt_FullRotation_RestoresOriginal() {
    var dictionary = new Dictionary<string, uint> { { "key1", 0x12345678u } };

    dictionary.RotateLeftOrAdd("key1", 32);

    Assert.AreEqual(0x12345678u, dictionary["key1"]);
  }

  #endregion

  #region RotateRightOrAdd Tests

  [Test]
  public void RotateRightOrAdd_UInt_NewKey_AddsZero() {
    var dictionary = new Dictionary<string, uint>();

    dictionary.RotateRightOrAdd("key1", 5);

    Assert.AreEqual(0u, dictionary["key1"]);
  }

  [Test]
  public void RotateRightOrAdd_Byte_RotatesRight() {
    var dictionary = new Dictionary<string, byte> { { "value", 0b00000011 } };

    dictionary.RotateRightOrAdd("value", 1);

    Assert.AreEqual(0b10000001, dictionary["value"]);
  }

  [Test]
  public void RotateRightOrAdd_UInt_RotatesRight() {
    var dictionary = new Dictionary<string, uint> { { "value", 0x00000003 } };

    dictionary.RotateRightOrAdd("value", 1);

    Assert.AreEqual(0x80000001u, dictionary["value"]);
  }

  [Test]
  public void RotateRightOrAdd_UInt_FullRotation_RestoresOriginal() {
    var dictionary = new Dictionary<string, uint> { { "key1", 0x12345678u } };

    dictionary.RotateRightOrAdd("key1", 32);

    Assert.AreEqual(0x12345678u, dictionary["key1"]);
  }

  [Test]
  public void RotateLeftAndRight_Cancel_RestoresOriginal() {
    var dictionary = new Dictionary<string, uint> { { "value", 0xDEADBEEF } };

    dictionary.RotateLeftOrAdd("value", 7);
    dictionary.RotateRightOrAdd("value", 7);

    Assert.AreEqual(0xDEADBEEFu, dictionary["value"]);
  }

  #endregion
}
