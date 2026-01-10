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

// System.Text.Json polyfill for older frameworks that don't have the official package
#if !SUPPORTS_SYSTEM_TEXT_JSON && !OFFICIAL_SYSTEM_TEXT_JSON

using System.Collections.Generic;
using System.Globalization;

namespace System.Text.Json;

/// <summary>
/// Represents a specific JSON value within a <see cref="JsonDocument"/>.
/// </summary>
public readonly struct JsonElement {

  private readonly object? _value;
  private readonly JsonValueKind _kind;

  internal JsonElement(object? value, JsonValueKind kind) {
    this._value = value;
    this._kind = kind;
  }

  /// <summary>
  /// Gets the type of the current JSON value.
  /// </summary>
  public JsonValueKind ValueKind => this._kind;

  /// <summary>
  /// Gets the value of the element as a string.
  /// </summary>
  /// <returns>The value as a string.</returns>
  public string? GetString() {
    if (this._kind == JsonValueKind.Null)
      return null;
    if (this._kind != JsonValueKind.String)
      throw new InvalidOperationException("Element is not a string.");
    return (string?)this._value;
  }

  /// <summary>
  /// Gets the value of the element as an <see cref="int"/>.
  /// </summary>
  /// <returns>The value as an int.</returns>
  public int GetInt32() {
    if (this._kind != JsonValueKind.Number)
      throw new InvalidOperationException("Element is not a number.");
    return Convert.ToInt32(this._value, CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="long"/>.
  /// </summary>
  /// <returns>The value as a long.</returns>
  public long GetInt64() {
    if (this._kind != JsonValueKind.Number)
      throw new InvalidOperationException("Element is not a number.");
    return Convert.ToInt64(this._value, CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="double"/>.
  /// </summary>
  /// <returns>The value as a double.</returns>
  public double GetDouble() {
    if (this._kind != JsonValueKind.Number)
      throw new InvalidOperationException("Element is not a number.");
    return Convert.ToDouble(this._value, CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="decimal"/>.
  /// </summary>
  /// <returns>The value as a decimal.</returns>
  public decimal GetDecimal() {
    if (this._kind != JsonValueKind.Number)
      throw new InvalidOperationException("Element is not a number.");
    return Convert.ToDecimal(this._value, CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="bool"/>.
  /// </summary>
  /// <returns>The value as a bool.</returns>
  public bool GetBoolean() {
    if (this._kind == JsonValueKind.True)
      return true;
    if (this._kind == JsonValueKind.False)
      return false;
    throw new InvalidOperationException("Element is not a boolean.");
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="Guid"/>.
  /// </summary>
  /// <returns>The value as a Guid.</returns>
  public Guid GetGuid() {
    var s = this.GetString();
    return s != null ? Guid.Parse(s) : default;
  }

  /// <summary>
  /// Gets the value of the element as a <see cref="DateTime"/>.
  /// </summary>
  /// <returns>The value as a DateTime.</returns>
  public DateTime GetDateTime() {
    var s = this.GetString();
    return s != null ? DateTime.Parse(s, CultureInfo.InvariantCulture) : default;
  }

  /// <summary>
  /// Gets a <see cref="JsonElement"/> representing the property with the specified name.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <returns>The element for the property.</returns>
  public JsonElement GetProperty(string propertyName) {
    if (this.TryGetProperty(propertyName, out var result))
      return result;
    throw new KeyNotFoundException($"Property '{propertyName}' not found.");
  }

  /// <summary>
  /// Looks for a property named <paramref name="propertyName"/> in the current object.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value of the property, if found.</param>
  /// <returns><see langword="true"/> if the property was found; otherwise, <see langword="false"/>.</returns>
  public bool TryGetProperty(string propertyName, out JsonElement value) {
    if (this._kind != JsonValueKind.Object) {
      value = default;
      return false;
    }

    var dict = (Dictionary<string, JsonElement>)this._value!;
    return dict.TryGetValue(propertyName, out value);
  }

  /// <summary>
  /// Gets the number of elements in the current JSON array.
  /// </summary>
  /// <returns>The number of elements.</returns>
  public int GetArrayLength() {
    if (this._kind != JsonValueKind.Array)
      throw new InvalidOperationException("Element is not an array.");
    return ((List<JsonElement>)this._value!).Count;
  }

  /// <summary>
  /// Gets the element at the specified index in the current JSON array.
  /// </summary>
  /// <param name="index">The index of the element.</param>
  /// <returns>The element at the specified index.</returns>
  public JsonElement this[int index] {
    get {
      if (this._kind != JsonValueKind.Array)
        throw new InvalidOperationException("Element is not an array.");
      return ((List<JsonElement>)this._value!)[index];
    }
  }

  /// <summary>
  /// Gets an enumerator to enumerate the values in the JSON array.
  /// </summary>
  /// <returns>An enumerator.</returns>
  public ArrayEnumerator EnumerateArray() {
    if (this._kind != JsonValueKind.Array)
      throw new InvalidOperationException("Element is not an array.");
    return new ArrayEnumerator((List<JsonElement>)this._value!);
  }

  /// <summary>
  /// Gets an enumerator to enumerate the properties in the JSON object.
  /// </summary>
  /// <returns>An enumerator.</returns>
  public ObjectEnumerator EnumerateObject() {
    if (this._kind != JsonValueKind.Object)
      throw new InvalidOperationException("Element is not an object.");
    return new ObjectEnumerator((Dictionary<string, JsonElement>)this._value!);
  }

  /// <summary>
  /// Gets the raw text representation of this element.
  /// </summary>
  /// <returns>The raw JSON text.</returns>
  public string GetRawText() {
    switch (this._kind) {
      case JsonValueKind.Undefined:
        return "";
      case JsonValueKind.Null:
        return "null";
      case JsonValueKind.True:
        return "true";
      case JsonValueKind.False:
        return "false";
      case JsonValueKind.String:
        return $"\"{_EscapeString((string)this._value!)}\"";
      case JsonValueKind.Number:
        return Convert.ToString(this._value, CultureInfo.InvariantCulture) ?? "0";
      case JsonValueKind.Array:
        var arr = (List<JsonElement>)this._value!;
        var sb = new StringBuilder("[");
        for (var i = 0; i < arr.Count; ++i) {
          if (i > 0)
            sb.Append(',');
          sb.Append(arr[i].GetRawText());
        }
        sb.Append(']');
        return sb.ToString();
      case JsonValueKind.Object:
        var dict = (Dictionary<string, JsonElement>)this._value!;
        var objSb = new StringBuilder("{");
        var first = true;
        foreach (var kvp in dict) {
          if (!first)
            objSb.Append(',');
          first = false;
          objSb.Append($"\"{_EscapeString(kvp.Key)}\":{kvp.Value.GetRawText()}");
        }
        objSb.Append('}');
        return objSb.ToString();
      default:
        return "";
    }
  }

  private static string _EscapeString(string value) {
    var sb = new StringBuilder();
    foreach (var c in value)
      switch (c) {
        case '"':
          sb.Append("\\\"");
          break;
        case '\\':
          sb.Append("\\\\");
          break;
        case '\b':
          sb.Append("\\b");
          break;
        case '\f':
          sb.Append("\\f");
          break;
        case '\n':
          sb.Append("\\n");
          break;
        case '\r':
          sb.Append("\\r");
          break;
        case '\t':
          sb.Append("\\t");
          break;
        default:
          if (c < ' ')
            sb.Append($"\\u{(int)c:X4}");
          else
            sb.Append(c);
          break;
      }
    return sb.ToString();
  }

  /// <summary>
  /// Tries to get the value as an int.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetInt32(out int value) {
    if (this._kind != JsonValueKind.Number) {
      value = 0;
      return false;
    }
    try {
      value = Convert.ToInt32(this._value, CultureInfo.InvariantCulture);
      return true;
    } catch {
      value = 0;
      return false;
    }
  }

  /// <summary>
  /// Tries to get the value as a long.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetInt64(out long value) {
    if (this._kind != JsonValueKind.Number) {
      value = 0;
      return false;
    }
    try {
      value = Convert.ToInt64(this._value, CultureInfo.InvariantCulture);
      return true;
    } catch {
      value = 0;
      return false;
    }
  }

  /// <summary>
  /// Tries to get the value as a double.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetDouble(out double value) {
    if (this._kind != JsonValueKind.Number) {
      value = 0;
      return false;
    }
    try {
      value = Convert.ToDouble(this._value, CultureInfo.InvariantCulture);
      return true;
    } catch {
      value = 0;
      return false;
    }
  }

  /// <summary>
  /// Returns the string representation of this element.
  /// </summary>
  /// <returns>The string representation.</returns>
  public override string ToString() => this.GetRawText();

  /// <summary>
  /// Enumerator for JSON array elements.
  /// </summary>
  public struct ArrayEnumerator : IEnumerator<JsonElement>, IEnumerable<JsonElement> {
    private readonly List<JsonElement> _list;
    private int _index;

    internal ArrayEnumerator(List<JsonElement> list) {
      this._list = list;
      this._index = -1;
    }

    /// <summary>
    /// Gets the current element.
    /// </summary>
    public JsonElement Current => this._list[this._index];

    object System.Collections.IEnumerator.Current => this.Current;

    /// <summary>
    /// Moves to the next element.
    /// </summary>
    /// <returns><see langword="true"/> if there are more elements; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext() => ++this._index < this._list.Count;

    /// <summary>
    /// Resets the enumerator.
    /// </summary>
    public void Reset() => this._index = -1;

    /// <summary>
    /// Disposes the enumerator.
    /// </summary>
    public void Dispose() { }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>This enumerator.</returns>
    public ArrayEnumerator GetEnumerator() => this;

    IEnumerator<JsonElement> IEnumerable<JsonElement>.GetEnumerator() => this;

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this;
  }

  /// <summary>
  /// Enumerator for JSON object properties.
  /// </summary>
  public struct ObjectEnumerator : IEnumerator<JsonProperty>, IEnumerable<JsonProperty> {
    private readonly Dictionary<string, JsonElement> _dict;
    private Dictionary<string, JsonElement>.Enumerator _enumerator;

    internal ObjectEnumerator(Dictionary<string, JsonElement> dict) {
      this._dict = dict;
      this._enumerator = dict.GetEnumerator();
    }

    /// <summary>
    /// Gets the current property.
    /// </summary>
    public JsonProperty Current => new(this._enumerator.Current.Key, this._enumerator.Current.Value);

    object System.Collections.IEnumerator.Current => this.Current;

    /// <summary>
    /// Moves to the next property.
    /// </summary>
    /// <returns><see langword="true"/> if there are more properties; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext() => this._enumerator.MoveNext();

    /// <summary>
    /// Resets the enumerator.
    /// </summary>
    public void Reset() => this._enumerator = this._dict.GetEnumerator();

    /// <summary>
    /// Disposes the enumerator.
    /// </summary>
    public void Dispose() => this._enumerator.Dispose();

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>This enumerator.</returns>
    public ObjectEnumerator GetEnumerator() => this;

    IEnumerator<JsonProperty> IEnumerable<JsonProperty>.GetEnumerator() => this;

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this;
  }

}

#endif
