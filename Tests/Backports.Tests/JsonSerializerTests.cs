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
using System.Text.Json;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class JsonSerializerTests {

  #region Serialize Primitives

  [Test]
  public void Serialize_String_Works() {
    var result = JsonSerializer.Serialize("hello");
    Assert.That(result, Is.EqualTo("\"hello\""));
  }

  [Test]
  public void Serialize_Integer_Works() {
    var result = JsonSerializer.Serialize(42);
    Assert.That(result, Is.EqualTo("42"));
  }

  [Test]
  public void Serialize_Double_Works() {
    var result = JsonSerializer.Serialize(3.14);
    Assert.That(result, Does.StartWith("3.14"));
  }

  [Test]
  public void Serialize_Boolean_Works() {
    Assert.That(JsonSerializer.Serialize(true), Is.EqualTo("true"));
    Assert.That(JsonSerializer.Serialize(false), Is.EqualTo("false"));
  }

  [Test]
  public void Serialize_Null_Works() {
    var result = JsonSerializer.Serialize<string>(null);
    Assert.That(result, Is.EqualTo("null"));
  }

  #endregion

  #region Serialize Objects

  [Test]
  public void Serialize_SimpleObject_Works() {
    var obj = new SimpleClass { Name = "Test", Value = 123 };
    var result = JsonSerializer.Serialize(obj);
    Assert.That(result, Does.Contain("\"Name\":\"Test\"").Or.Contain("\"name\":\"Test\""));
    Assert.That(result, Does.Contain("\"Value\":123").Or.Contain("\"value\":123"));
  }

  [Test]
  public void Serialize_Array_Works() {
    var arr = new[] { 1, 2, 3 };
    var result = JsonSerializer.Serialize(arr);
    Assert.That(result, Is.EqualTo("[1,2,3]"));
  }

  [Test]
  public void Serialize_List_Works() {
    var list = new List<string> { "a", "b", "c" };
    var result = JsonSerializer.Serialize(list);
    Assert.That(result, Is.EqualTo("[\"a\",\"b\",\"c\"]"));
  }

  [Test]
  public void Serialize_Dictionary_Works() {
    var dict = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } };
    var result = JsonSerializer.Serialize(dict);
    Assert.That(result, Does.Contain("\"one\":1"));
    Assert.That(result, Does.Contain("\"two\":2"));
  }

  #endregion

  #region Deserialize Primitives

  [Test]
  public void Deserialize_String_Works() {
    var result = JsonSerializer.Deserialize<string>("\"hello\"");
    Assert.That(result, Is.EqualTo("hello"));
  }

  [Test]
  public void Deserialize_Integer_Works() {
    var result = JsonSerializer.Deserialize<int>("42");
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  public void Deserialize_Double_Works() {
    var result = JsonSerializer.Deserialize<double>("3.14");
    Assert.That(result, Is.EqualTo(3.14).Within(0.001));
  }

  [Test]
  public void Deserialize_Boolean_Works() {
    Assert.That(JsonSerializer.Deserialize<bool>("true"), Is.True);
    Assert.That(JsonSerializer.Deserialize<bool>("false"), Is.False);
  }

  [Test]
  public void Deserialize_Null_Works() {
    var result = JsonSerializer.Deserialize<string>("null");
    Assert.That(result, Is.Null);
  }

  #endregion

  #region Deserialize Objects

  [Test]
  public void Deserialize_SimpleObject_Works() {
    var json = "{\"Name\":\"Test\",\"Value\":123}";
    var result = JsonSerializer.Deserialize<SimpleClass>(json);
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Name, Is.EqualTo("Test"));
    Assert.That(result.Value, Is.EqualTo(123));
  }

  [Test]
  public void Deserialize_Array_Works() {
    var result = JsonSerializer.Deserialize<int[]>("[1,2,3]");
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void Deserialize_List_Works() {
    var result = JsonSerializer.Deserialize<List<string>>("[\"a\",\"b\",\"c\"]");
    Assert.That(result, Is.EqualTo(new List<string> { "a", "b", "c" }));
  }

  #endregion

  #region JsonSerializerOptions

  [Test]
  public void Serialize_WithCamelCase_Works() {
    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var obj = new SimpleClass { Name = "Test", Value = 123 };
    var result = JsonSerializer.Serialize(obj, options);
    Assert.That(result, Does.Contain("\"name\":"));
    Assert.That(result, Does.Contain("\"value\":"));
  }

  [Test]
  public void Deserialize_CaseInsensitive_Works() {
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var json = "{\"name\":\"Test\",\"value\":123}";
    var result = JsonSerializer.Deserialize<SimpleClass>(json, options);
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Name, Is.EqualTo("Test"));
    Assert.That(result.Value, Is.EqualTo(123));
  }

  #endregion

  #region JsonDocument

  [Test]
  public void JsonDocument_Parse_Works() {
    using var doc = JsonDocument.Parse("{\"name\":\"Test\",\"value\":123}");
    var root = doc.RootElement;
    Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Object));
  }

  [Test]
  public void JsonElement_GetProperty_Works() {
    using var doc = JsonDocument.Parse("{\"name\":\"Test\",\"value\":123}");
    var root = doc.RootElement;
    Assert.That(root.GetProperty("name").GetString(), Is.EqualTo("Test"));
    Assert.That(root.GetProperty("value").GetInt32(), Is.EqualTo(123));
  }

  [Test]
  public void JsonElement_TryGetProperty_Works() {
    using var doc = JsonDocument.Parse("{\"name\":\"Test\"}");
    var root = doc.RootElement;
    Assert.That(root.TryGetProperty("name", out var prop), Is.True);
    Assert.That(prop.GetString(), Is.EqualTo("Test"));
    Assert.That(root.TryGetProperty("nonexistent", out _), Is.False);
  }

  [Test]
  public void JsonElement_EnumerateArray_Works() {
    using var doc = JsonDocument.Parse("[1,2,3]");
    var root = doc.RootElement;
    var values = new List<int>();
    foreach (var item in root.EnumerateArray())
      values.Add(item.GetInt32());
    Assert.That(values, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void JsonElement_EnumerateObject_Works() {
    using var doc = JsonDocument.Parse("{\"a\":1,\"b\":2}");
    var root = doc.RootElement;
    var keys = new List<string>();
    foreach (var prop in root.EnumerateObject())
      keys.Add(prop.Name);
    Assert.That(keys, Does.Contain("a"));
    Assert.That(keys, Does.Contain("b"));
  }

  [Test]
  public void JsonElement_GetArrayLength_Works() {
    using var doc = JsonDocument.Parse("[1,2,3,4,5]");
    Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(5));
  }

  [Test]
  public void JsonElement_ArrayIndexer_Works() {
    using var doc = JsonDocument.Parse("[10,20,30]");
    Assert.That(doc.RootElement[0].GetInt32(), Is.EqualTo(10));
    Assert.That(doc.RootElement[1].GetInt32(), Is.EqualTo(20));
    Assert.That(doc.RootElement[2].GetInt32(), Is.EqualTo(30));
  }

  #endregion

  #region JsonNamingPolicy

  [Test]
  public void JsonNamingPolicy_CamelCase_Works() {
    Assert.That(JsonNamingPolicy.CamelCase.ConvertName("PropertyName"), Is.EqualTo("propertyName"));
    Assert.That(JsonNamingPolicy.CamelCase.ConvertName("XMLParser"), Is.EqualTo("xmlParser"));
    Assert.That(JsonNamingPolicy.CamelCase.ConvertName("ID"), Is.EqualTo("id"));
  }

  #endregion

  #region Edge Cases

  [Test]
  public void Serialize_StringWithEscapes_Works() {
    var result = JsonSerializer.Serialize("hello\nworld");
    Assert.That(result, Does.Contain("\\n"));
  }

  [Test]
  public void Deserialize_WhitespaceJson_Works() {
    var json = "  { \"Name\" : \"Test\" }  ";
    var result = JsonSerializer.Deserialize<SimpleClass>(json);
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Name, Is.EqualTo("Test"));
  }

  [Test]
  public void Serialize_NestedObject_Works() {
    var obj = new NestedClass { Inner = new SimpleClass { Name = "Inner", Value = 42 } };
    var result = JsonSerializer.Serialize(obj);
    Assert.That(result, Does.Contain("\"Inner\"").Or.Contain("\"inner\""));
    Assert.That(result, Does.Contain("\"Name\":\"Inner\"").Or.Contain("\"name\":\"Inner\""));
  }

  #endregion

  #region Test Classes

  public class SimpleClass {
    public string Name { get; set; }
    public int Value { get; set; }
  }

  public class NestedClass {
    public SimpleClass Inner { get; set; }
  }

  #endregion

}
