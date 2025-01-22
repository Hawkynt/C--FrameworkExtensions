using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
using System.Linq;

namespace System.Collections.Generic;

[TestFixture]
public class DictionaryTests {

  [Test]
  public void AddOrUpdate_NullDictionary_Throws() {
    Dictionary<string, int>? dict = null;
    Assert.Throws<NullReferenceException>(() => dict!.AddOrUpdate("key", 42));
  }

  [Test]
  public void AddOrUpdate_NullKey_Throws() {
    var dict = new Dictionary<string, int>();
    string? key = null;
    Assert.Throws<ArgumentNullException>(() => dict.AddOrUpdate(key!, 42));
  }

  [Test]
  public void AddOrUpdate_Adding_Existing_Key_Updates_Value() {
    var dict = new Dictionary<string, int> { { "key", 10 } };
    dict.AddOrUpdate("key", 42);

    Assert.AreEqual(42, dict["key"], "Existing key value should be updated.");
  }

  [Test]
  public void AddOrUpdate_Adding_New_Key_Adds_Value() {
    var dict = new Dictionary<string, int>();
    dict.AddOrUpdate("newKey", 42);

    Assert.IsTrue(dict.ContainsKey("newKey"), "New key should be added.");
    Assert.AreEqual(42, dict["newKey"], "New key should have correct value.");
  }

  [Test]
  public void AddOrUpdate_Adding_Existing_ReferenceTypeKey_Updates_Value() {
    var dict = new Dictionary<object, string>();
    var key = new object();
    dict[key] = "oldValue";

    dict.AddOrUpdate(key, "newValue");

    Assert.AreEqual("newValue", dict[key], "Existing reference key value should be updated.");
  }

  [Test]
  public void AddOrUpdate_Adding_New_ReferenceTypeKey_Adds_Value() {
    var dict = new Dictionary<object, string>();
    var key = new object();

    dict.AddOrUpdate(key, "newValue");

    Assert.IsTrue(dict.ContainsKey(key), "New reference type key should be added.");
    Assert.AreEqual("newValue", dict[key], "New reference key should have correct value.");
  }

  [Test]
  public void AddOrUpdate_Handles_ValueTypeKeys_Correctly() {
    var dict = new Dictionary<int, string>();
    dict.AddOrUpdate(1, "value");

    Assert.IsTrue(dict.ContainsKey(1), "Value type key should be added.");
    Assert.AreEqual("value", dict[1], "Value type key should have correct value.");
  }

  [Test]
  public void GetOrAddDefault_NullDictionary_Throws() {
    Dictionary<string, int>? dict = null;
    Assert.Throws<NullReferenceException>(() => dict!.GetOrAddDefault("key"));
  }

  [Test]
  public void GetOrAddDefault_Getting_Existing_Key_Returns_Value() {
    var dict = new Dictionary<string, int> { { "key", 100 } };

    var result = dict.GetOrAddDefault("key");

    Assert.AreEqual(100, result, "Existing key value should be returned.");
    Assert.AreEqual(100, dict["key"], "Existing value should remain unchanged.");
  }

  [Test]
  public void GetOrAddDefault_Getting_New_Key_Returns_Default_And_Adds_Key_For_Value_Types() {
    var dict = new Dictionary<int, double>();

    var result = dict.GetOrAddDefault(1);

    Assert.AreEqual(0.0, result, "Default value should be returned for new key.");
    Assert.IsTrue(dict.ContainsKey(1), "New key should be added.");
    Assert.AreEqual(0.0, dict[1], "Default value should be stored.");
  }

  [Test]
  public void GetOrAddDefault_Getting_New_Key_Returns_Default_And_Adds_Key_For_Reference_Types() {
    var dict = new Dictionary<string, string>();

    var result = dict.GetOrAddDefault("newKey");

    Assert.AreEqual(null, result, "Default value should be returned for new key.");
    Assert.IsTrue(dict.ContainsKey("newKey"), "New key should be added.");
    Assert.AreEqual(null, dict["newKey"], "Default value should be stored.");
  }

  [Test]
  public void GetOrAdd_NullDictionary_Throws() {
    Dictionary<string, int>? dict = null;
    Assert.Throws<NullReferenceException>(() => dict!.GetOrAdd("key", 42));
    Assert.Throws<NullReferenceException>(() => dict!.GetOrAdd("key", () => 42));
    Assert.Throws<NullReferenceException>(() => dict!.GetOrAdd("key", k => 42));
  }

  [Test]
  public void GetOrAdd_Existing_Key_Returns_Value_And_Doesnt_Update() {
    var dict = new Dictionary<string, int> { { "key", 10 } };

    var result = dict.GetOrAdd("key", 42);
    var funcResult = dict.GetOrAdd("key", () => 42);
    var funcWithKeyResult = dict.GetOrAdd("key", k => 42);

    Assert.AreEqual(10, result, "Existing value should not change.");
    Assert.AreEqual(10, funcResult, "Existing value should not change.");
    Assert.AreEqual(10, funcWithKeyResult, "Existing value should not change.");
  }

  [Test]
  public void GetOrAdd_New_Key_Returns_Given_Value_And_Adds_Key_For_Reference_Type() {
    var dict = new Dictionary<string, string>();

    var result = dict.GetOrAdd("newKey", "defaultValue");

    Assert.AreEqual("defaultValue", result, "New key should return the provided value.");
    Assert.IsTrue(dict.ContainsKey("newKey"), "New key should be added.");
    Assert.AreEqual("defaultValue", dict["newKey"], "Stored value should match the provided one.");
  }

  [Test]
  public void GetOrAdd_New_Key_Returns_Given_Value_And_Adds_Key_For_Value_Type() {
    var dict = new Dictionary<int, double>();

    var result = dict.GetOrAdd(1, 3.14);

    Assert.AreEqual(3.14, result, "New key should return the provided value.");
    Assert.IsTrue(dict.ContainsKey(1), "New key should be added.");
    Assert.AreEqual(3.14, dict[1], "Stored value should match the provided one.");
  }

  [Test]
  public void GetOrAdd_New_Key_Call_Generator_Function_Without_Parameters() {
    var dict = new Dictionary<string, int>();
    var factoryCalled = false;

    var result = dict.GetOrAdd("newKey", GeneratorFunction);

    Assert.AreEqual(99, result, "Function should return the generated value.");
    Assert.IsTrue(factoryCalled, "Factory function should be called.");
    Assert.IsTrue(dict.ContainsKey("newKey"), "New key should be added.");
    Assert.AreEqual(99, dict["newKey"], "Stored value should match the generated one.");
    return;

    int GeneratorFunction() {
      factoryCalled = true;
      return 99;
    }
  }

  [Test]
  public void GetOrAdd_New_Key_Call_Generator_Function_With_Key_Parameter() {
    var dict = new Dictionary<string, string>();

    var result = dict.GetOrAdd("dynamicKey", GeneratorFunction);

    Assert.AreEqual("Generated-dynamicKey", result, "Function should return the generated value with key.");
    Assert.IsTrue(dict.ContainsKey("dynamicKey"), "New key should be added.");
    Assert.AreEqual("Generated-dynamicKey", dict["dynamicKey"], "Stored value should match the generated one.");
    return;

    string GeneratorFunction(string key) => $"Generated-{key}";
  }

  [Test]
  public void GetOrAdd_Null_Generator_Function_Throws_ArgumentNullException() {
    var dict = new Dictionary<string, int>();

    Assert.Throws<ArgumentNullException>(() => dict.GetOrAdd("key", (Func<int>)null!));
    Assert.Throws<ArgumentNullException>(() => dict.GetOrAdd("key", (Func<string, int>)null!));
  }

  [Test]
  public void TryAdd_NullDictionary_ThrowsArgumentNullException() {
    Dictionary<string, int>? dict = null;
    Assert.Throws<NullReferenceException>(() => dict!.TryAdd("key", 42));
  }

  [Test]
  public void TryAdd_New_Key_Returns_True_And_Adds_Value() {
    var dict = new Dictionary<string, int>();

    bool result = dict.TryAdd("newKey", 100);

    Assert.IsTrue(result, "TryAdd should return true when adding a new key.");
    Assert.IsTrue(dict.ContainsKey("newKey"), "New key should be added.");
    Assert.AreEqual(100, dict["newKey"], "Value should be stored correctly.");
  }

  [Test]
  public void TryAdd_Existing_Key_Returns_False_And_Does_Not_Update() {
    var dict = new Dictionary<string, string> { { "key", "oldValue" } };

    bool result = dict.TryAdd("key", "newValue");

    Assert.IsFalse(result, "TryAdd should return false when the key already exists.");
    Assert.AreEqual("oldValue", dict["key"], "Existing value should remain unchanged.");
  }

  [Test]
  public void TryAdd_Adding_Multiple_New_Keys_Returns_True_For_All() {
    var dict = new Dictionary<int, string>();

    bool result1 = dict.TryAdd(1, "first");
    bool result2 = dict.TryAdd(2, "second");

    Assert.IsTrue(result1, "First key should be added successfully.");
    Assert.IsTrue(result2, "Second key should be added successfully.");
    Assert.AreEqual("first", dict[1], "First value should be stored correctly.");
    Assert.AreEqual("second", dict[2], "Second value should be stored correctly.");
  }

  [Test]
  public void TryAdd_Adding_ReferenceTypeKey_Returns_True_And_Adds_Value() {
    var dict = new Dictionary<object, string>();
    var key = new object();

    bool result = dict.TryAdd(key, "referenceValue");

    Assert.IsTrue(result, "TryAdd should return true for reference type keys.");
    Assert.IsTrue(dict.ContainsKey(key), "Reference key should be added.");
    Assert.AreEqual("referenceValue", dict[key], "Value should be stored correctly.");
  }

  [Test]
  public void TryAdd_Adding_ValueTypeKey_Returns_True_And_Adds_Value() {
    var dict = new Dictionary<int, double>();

    bool result = dict.TryAdd(1, 3.14);

    Assert.IsTrue(result, "TryAdd should return true for value type keys.");
    Assert.IsTrue(dict.ContainsKey(1), "Value type key should be added.");
    Assert.AreEqual(3.14, dict[1], "Value should be stored correctly.");
  }

  [Test]
  public void TryAdd_Concurrent_Additions_Preserve_Integrity() {
    var dict = new Dictionary<int, string>();

    bool result1 = dict.TryAdd(1, "first");
    bool result2 = dict.TryAdd(1, "second");  // Duplicate key

    Assert.IsTrue(result1, "First key should be added successfully.");
    Assert.IsFalse(result2, "Duplicate key should not be added.");
    Assert.AreEqual("first", dict[1], "Value should remain unchanged after duplicate attempt.");
  }

}

