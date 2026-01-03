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

// ExpandoObject is available in net40+ via BCL, and in net20 via our polyfill.
// Since NUnit requires net35+, these tests run against the BCL implementation on net40+.
// They serve to verify expected behavior and document the API contract.
#if SUPPORTS_DYNAMIC

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using NUnit.Framework;
using Category = NUnit.Framework.CategoryAttribute;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Dynamic")]
[Category("ExpandoObject")]
public class ExpandoObjectTests {

  #region Basic Initialization

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_DefaultConstructor_CreatesEmptyInstance() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  #endregion

  #region Dictionary Interface

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_AddProperty_CanRetrieve() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["Name"] = "Test";
    Assert.That(dict["Name"], Is.EqualTo("Test"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_AddMultipleProperties_CountIncreases() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["First"] = 1;
    dict["Second"] = 2;
    dict["Third"] = 3;
    Assert.That(dict.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_ContainsKey_ReturnsTrueForExistingKey() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["Exists"] = true;
    Assert.That(dict.ContainsKey("Exists"), Is.True);
    Assert.That(dict.ContainsKey("NotExists"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_TryGetValue_ReturnsCorrectValues() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["Key"] = "Value";

    var success = dict.TryGetValue("Key", out var value);
    Assert.That(success, Is.True);
    Assert.That(value, Is.EqualTo("Value"));

    var notFound = dict.TryGetValue("Missing", out var missing);
    Assert.That(notFound, Is.False);
    Assert.That(missing, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Remove_RemovesProperty() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["ToRemove"] = "Value";
    Assert.That(dict.Count, Is.EqualTo(1));

    var removed = dict.Remove("ToRemove");
    Assert.That(removed, Is.True);
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Remove_ReturnsFalseForMissingKey() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    var removed = dict.Remove("NotExists");
    Assert.That(removed, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Clear_RemovesAllProperties() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["One"] = 1;
    dict["Two"] = 2;
    dict["Three"] = 3;
    Assert.That(dict.Count, Is.EqualTo(3));

    dict.Clear();
    Assert.That(dict.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Keys_ReturnsAllKeys() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["Alpha"] = 1;
    dict["Beta"] = 2;
    dict["Gamma"] = 3;

    var keys = dict.Keys;
    Assert.That(keys, Is.EquivalentTo(new[] { "Alpha", "Beta", "Gamma" }));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Values_ReturnsAllValues() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["A"] = 10;
    dict["B"] = 20;
    dict["C"] = 30;

    var values = dict.Values;
    Assert.That(values, Is.EquivalentTo(new object[] { 10, 20, 30 }));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_Enumeration_ReturnsAllKeyValuePairs() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["X"] = "x";
    dict["Y"] = "y";

    var pairs = new List<KeyValuePair<string, object>>();
    foreach (var kvp in dict)
      pairs.Add(kvp);

    Assert.That(pairs.Count, Is.EqualTo(2));
    Assert.That(pairs, Has.One.Matches<KeyValuePair<string, object>>(kvp => kvp.Key == "X" && (string)kvp.Value == "x"));
    Assert.That(pairs, Has.One.Matches<KeyValuePair<string, object>>(kvp => kvp.Key == "Y" && (string)kvp.Value == "y"));
  }

  #endregion

  #region Dynamic Access

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_DynamicSet_CanRetrieveViaDictionary() {
    dynamic expando = new ExpandoObject();
    expando.DynamicProperty = "DynamicValue";

    var dict = (IDictionary<string, object>)expando;
    Assert.That(dict["DynamicProperty"], Is.EqualTo("DynamicValue"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_DictionarySet_CanRetrieveViaDynamic() {
    dynamic expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["DictProperty"] = 123;

    Assert.That((int)expando.DictProperty, Is.EqualTo(123));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_DynamicNestedObjects() {
    dynamic expando = new ExpandoObject();
    expando.Child = new ExpandoObject();
    expando.Child.Name = "ChildName";
    expando.Child.Value = 42;

    Assert.That((string)expando.Child.Name, Is.EqualTo("ChildName"));
    Assert.That((int)expando.Child.Value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_DynamicFunction() {
    dynamic expando = new ExpandoObject();
    expando.Add = (Func<int, int, int>)((a, b) => a + b);

    var result = expando.Add(3, 5);
    Assert.That((int)result, Is.EqualTo(8));
  }

  #endregion

  #region Value Types

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_StoresValueTypes() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["Int"] = 42;
    dict["Double"] = 3.14;
    dict["Bool"] = true;
    dict["DateTime"] = new DateTime(2024, 1, 1);

    Assert.That(dict["Int"], Is.EqualTo(42));
    Assert.That(dict["Double"], Is.EqualTo(3.14));
    Assert.That(dict["Bool"], Is.EqualTo(true));
    Assert.That(dict["DateTime"], Is.EqualTo(new DateTime(2024, 1, 1)));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_StoresNullValues() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict["NullValue"] = null;

    Assert.That(dict.ContainsKey("NullValue"), Is.True);
    Assert.That(dict["NullValue"], Is.Null);
  }

  #endregion

  #region INotifyPropertyChanged

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_PropertyChanged_FiresOnAdd() {
    var expando = new ExpandoObject();
    var notify = (INotifyPropertyChanged)expando;
    var dict = (IDictionary<string, object>)expando;

    string changedProperty = null;
    notify.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

    dict["NewProperty"] = "NewValue";
    Assert.That(changedProperty, Is.EqualTo("NewProperty"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_PropertyChanged_FiresOnUpdate() {
    var expando = new ExpandoObject();
    var notify = (INotifyPropertyChanged)expando;
    var dict = (IDictionary<string, object>)expando;

    dict["Property"] = "Initial";

    string changedProperty = null;
    notify.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

    dict["Property"] = "Updated";
    Assert.That(changedProperty, Is.EqualTo("Property"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandoObject_PropertyChanged_FiresOnRemove() {
    var expando = new ExpandoObject();
    var notify = (INotifyPropertyChanged)expando;
    var dict = (IDictionary<string, object>)expando;

    dict["ToRemove"] = "Value";

    string changedProperty = null;
    notify.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

    dict.Remove("ToRemove");
    Assert.That(changedProperty, Is.EqualTo("ToRemove"));
  }

  #endregion

  #region Exception Cases

  [Test]
  [Category("Exception")]
  public void ExpandoObject_GetMissingKey_ThrowsKeyNotFoundException() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    Assert.Throws<KeyNotFoundException>(() => _ = dict["MissingKey"]);
  }

  [Test]
  [Category("Exception")]
  public void ExpandoObject_AddDuplicateKey_ThrowsArgumentException() {
    var expando = new ExpandoObject();
    var dict = (IDictionary<string, object>)expando;
    dict.Add("Key", "Value1");
    Assert.Throws<ArgumentException>(() => dict.Add("Key", "Value2"));
  }

  #endregion

}

#endif
