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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// Provides functionality to serialize objects or value types to JSON and to deserialize JSON into objects or value types.
/// </summary>
public static class JsonSerializer {

  /// <summary>
  /// Converts the value of a specified type into a JSON string.
  /// </summary>
  /// <typeparam name="T">The type of the value to serialize.</typeparam>
  /// <param name="value">The value to convert.</param>
  /// <param name="options">Options to control the conversion behavior.</param>
  /// <returns>A JSON string representation of the value.</returns>
  public static string Serialize<T>(T value, JsonSerializerOptions? options = null) {
    options ??= JsonSerializerOptions.Default;
    var writer = new Utf8JsonWriter(options.WriteIndented);
    _WriteValue(writer, value, typeof(T), options, 0);
    return writer.ToString();
  }

  /// <summary>
  /// Converts the value of a specified type into a JSON string.
  /// </summary>
  /// <param name="value">The value to convert.</param>
  /// <param name="inputType">The type of the value to convert.</param>
  /// <param name="options">Options to control the conversion behavior.</param>
  /// <returns>A JSON string representation of the value.</returns>
  public static string Serialize(object? value, Type inputType, JsonSerializerOptions? options = null) {
    options ??= JsonSerializerOptions.Default;
    var writer = new Utf8JsonWriter(options.WriteIndented);
    _WriteValue(writer, value, inputType, options, 0);
    return writer.ToString();
  }

  /// <summary>
  /// Writes the JSON representation of a specified type to the provided stream.
  /// </summary>
  /// <typeparam name="T">The type of the value to serialize.</typeparam>
  /// <param name="utf8Json">The stream to write to.</param>
  /// <param name="value">The value to convert.</param>
  /// <param name="options">Options to control the conversion behavior.</param>
  public static void Serialize<T>(Stream utf8Json, T value, JsonSerializerOptions? options = null) {
    var json = Serialize(value, options);
    var bytes = Encoding.UTF8.GetBytes(json);
    utf8Json.Write(bytes, 0, bytes.Length);
  }

  /// <summary>
  /// Parses the text representing a single JSON value into an instance of the type specified by a generic type parameter.
  /// </summary>
  /// <typeparam name="T">The target type of the JSON value.</typeparam>
  /// <param name="json">The JSON text to parse.</param>
  /// <param name="options">Options to control the behavior during parsing.</param>
  /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
  public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null) {
    var result = Deserialize(json, typeof(T), options);
    return result == null ? default : (T)result;
  }

  /// <summary>
  /// Parses the text representing a single JSON value into an instance of the specified type.
  /// </summary>
  /// <param name="json">The JSON text to parse.</param>
  /// <param name="returnType">The type of the object to convert to and return.</param>
  /// <param name="options">Options to control the behavior during parsing.</param>
  /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
  public static object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null) {
    ArgumentNullException.ThrowIfNull(json);
    ArgumentNullException.ThrowIfNull(returnType);
    options ??= JsonSerializerOptions.Default;

    using var doc = JsonDocument.Parse(json);
    return _ReadValue(doc.RootElement, returnType, options);
  }

  /// <summary>
  /// Reads the UTF-8 encoded text representing a single JSON value into an instance of the type specified by a generic type parameter.
  /// </summary>
  /// <typeparam name="T">The target type of the JSON value.</typeparam>
  /// <param name="utf8Json">The stream to read from.</param>
  /// <param name="options">Options to control the behavior during parsing.</param>
  /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
  public static T? Deserialize<T>(Stream utf8Json, JsonSerializerOptions? options = null) {
    using var reader = new StreamReader(utf8Json, Encoding.UTF8);
    var json = reader.ReadToEnd();
    return Deserialize<T>(json, options);
  }

  private static void _WriteValue(Utf8JsonWriter writer, object? value, Type declaredType, JsonSerializerOptions options, int depth) {
    if (depth > options.MaxDepth)
      throw new JsonException($"Maximum depth of {options.MaxDepth} exceeded.");

    if (value == null) {
      writer.WriteNullValue();
      return;
    }

    var type = value.GetType();

    if (type == typeof(string)) {
      writer.WriteStringValue((string)value);
      return;
    }

    if (type == typeof(bool)) {
      writer.WriteBooleanValue((bool)value);
      return;
    }

    if (type == typeof(int)) {
      writer.WriteNumberValue((int)value);
      return;
    }

    if (type == typeof(long)) {
      writer.WriteNumberValue((long)value);
      return;
    }

    if (type == typeof(double)) {
      writer.WriteNumberValue((double)value);
      return;
    }

    if (type == typeof(float)) {
      writer.WriteNumberValue((float)value);
      return;
    }

    if (type == typeof(decimal)) {
      writer.WriteNumberValue((decimal)value);
      return;
    }

    if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
        type == typeof(uint) || type == typeof(ulong)) {
      writer.WriteNumberValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
      return;
    }

    if (type == typeof(DateTime)) {
      writer.WriteStringValue(((DateTime)value).ToString("O", CultureInfo.InvariantCulture));
      return;
    }

    if (type == typeof(DateTimeOffset)) {
      writer.WriteStringValue(((DateTimeOffset)value).ToString("O", CultureInfo.InvariantCulture));
      return;
    }

    if (type == typeof(Guid)) {
      writer.WriteStringValue(((Guid)value).ToString("D"));
      return;
    }

    if (type == typeof(TimeSpan)) {
      writer.WriteStringValue(((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture));
      return;
    }

    if (type.IsEnum) {
      writer.WriteStringValue(value.ToString());
      return;
    }

    if (type == typeof(JsonElement)) {
      writer.WriteValue((JsonElement)value);
      return;
    }

    if (value is IDictionary dict) {
      writer.WriteStartObject();
      foreach (DictionaryEntry entry in dict) {
        var key = entry.Key?.ToString() ?? "null";
        if (options.DictionaryKeyPolicy != null)
          key = options.DictionaryKeyPolicy.ConvertName(key);
        writer.WritePropertyName(key);
        _WriteValue(writer, entry.Value, typeof(object), options, depth + 1);
      }
      writer.WriteEndObject();
      return;
    }

    if (value is IEnumerable enumerable && type != typeof(string)) {
      writer.WriteStartArray();
      foreach (var item in enumerable)
        _WriteValue(writer, item, typeof(object), options, depth + 1);
      writer.WriteEndArray();
      return;
    }

    _WriteObject(writer, value, type, options, depth);
  }

  private static void _WriteObject(Utf8JsonWriter writer, object value, Type type, JsonSerializerOptions options, int depth) {
    writer.WriteStartObject();

    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    foreach (var prop in properties) {
      if (!prop.CanRead)
        continue;

      var ignoreAttr = prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), true);
      if (ignoreAttr.Length > 0) {
        var ignore = (JsonIgnoreAttribute)ignoreAttr[0];
        if (ignore.Condition == JsonIgnoreCondition.Always)
          continue;
      }

      var propValue = prop.GetValue(value, null);

      if (propValue == null) {
        if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
          continue;
      }

      var propName = prop.Name;
      var nameAttr = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
      if (nameAttr.Length > 0)
        propName = ((JsonPropertyNameAttribute)nameAttr[0]).Name;
      else if (options.PropertyNamingPolicy != null)
        propName = options.PropertyNamingPolicy.ConvertName(propName);

      writer.WritePropertyName(propName);
      _WriteValue(writer, propValue, prop.PropertyType, options, depth + 1);
    }

    if (options.IncludeFields) {
      var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
      foreach (var field in fields) {
        var ignoreAttr = field.GetCustomAttributes(typeof(JsonIgnoreAttribute), true);
        if (ignoreAttr.Length > 0)
          continue;

        var fieldValue = field.GetValue(value);

        if (fieldValue == null && options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
          continue;

        var fieldName = field.Name;
        var nameAttr = field.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
        if (nameAttr.Length > 0)
          fieldName = ((JsonPropertyNameAttribute)nameAttr[0]).Name;
        else if (options.PropertyNamingPolicy != null)
          fieldName = options.PropertyNamingPolicy.ConvertName(fieldName);

        writer.WritePropertyName(fieldName);
        _WriteValue(writer, fieldValue, field.FieldType, options, depth + 1);
      }
    }

    writer.WriteEndObject();
  }

  private static object? _ReadValue(JsonElement element, Type targetType, JsonSerializerOptions options) {
    if (element.ValueKind == JsonValueKind.Null)
      return null;

    if (targetType == typeof(object)) {
      switch (element.ValueKind) {
        case JsonValueKind.Object:
          var dict = new Dictionary<string, object?>();
          foreach (var prop in element.EnumerateObject())
            dict[prop.Name] = _ReadValue(prop.Value, typeof(object), options);
          return dict;
        case JsonValueKind.Array:
          var list = new List<object?>();
          foreach (var item in element.EnumerateArray())
            list.Add(_ReadValue(item, typeof(object), options));
          return list;
        case JsonValueKind.String:
          return element.GetString();
        case JsonValueKind.Number:
          if (element.TryGetInt64(out var lng))
            return lng;
          return element.GetDouble();
        case JsonValueKind.True:
          return true;
        case JsonValueKind.False:
          return false;
        default:
          return null;
      }
    }

    if (targetType == typeof(string))
      return element.GetString();

    if (targetType == typeof(bool))
      return element.GetBoolean();

    if (targetType == typeof(int))
      return element.GetInt32();

    if (targetType == typeof(long))
      return element.GetInt64();

    if (targetType == typeof(double))
      return element.GetDouble();

    if (targetType == typeof(float))
      return (float)element.GetDouble();

    if (targetType == typeof(decimal))
      return element.GetDecimal();

    if (targetType == typeof(byte))
      return (byte)element.GetInt32();

    if (targetType == typeof(sbyte))
      return (sbyte)element.GetInt32();

    if (targetType == typeof(short))
      return (short)element.GetInt32();

    if (targetType == typeof(ushort))
      return (ushort)element.GetInt32();

    if (targetType == typeof(uint))
      return (uint)element.GetInt64();

    if (targetType == typeof(ulong))
      return (ulong)element.GetInt64();

    if (targetType == typeof(DateTime))
      return element.GetDateTime();

    if (targetType == typeof(DateTimeOffset))
      return DateTimeOffset.Parse(element.GetString()!, CultureInfo.InvariantCulture);

    if (targetType == typeof(Guid))
      return element.GetGuid();

    if (targetType == typeof(TimeSpan))
      return TimeSpan.Parse(element.GetString()!, CultureInfo.InvariantCulture);

    if (targetType.IsEnum) {
      var enumStr = element.GetString();
      return enumStr != null ? Enum.Parse(targetType, enumStr, true) : Activator.CreateInstance(targetType);
    }

    if (targetType == typeof(JsonElement))
      return element;

    var underlyingType = Nullable.GetUnderlyingType(targetType);
    if (underlyingType != null)
      return _ReadValue(element, underlyingType, options);

    if (element.ValueKind == JsonValueKind.Array)
      return _ReadArray(element, targetType, options);

    if (element.ValueKind == JsonValueKind.Object)
      return _ReadObject(element, targetType, options);

    return null;
  }

  private static object _ReadArray(JsonElement element, Type targetType, JsonSerializerOptions options) {
    Type elementType;

    if (targetType.IsArray) {
      elementType = targetType.GetElementType()!;
      var count = element.GetArrayLength();
      var array = Array.CreateInstance(elementType, count);
      var index = 0;
      foreach (var item in element.EnumerateArray()) {
        array.SetValue(_ReadValue(item, elementType, options), index);
        ++index;
      }
      return array;
    }

    if (targetType.IsGenericType) {
      var genericDef = targetType.GetGenericTypeDefinition();
      if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) ||
          genericDef == typeof(ICollection<>) || genericDef == typeof(IEnumerable<>)) {
        elementType = targetType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in element.EnumerateArray())
          list.Add(_ReadValue(item, elementType, options));
        return list;
      }
    }

    var objList = new List<object?>();
    foreach (var item in element.EnumerateArray())
      objList.Add(_ReadValue(item, typeof(object), options));
    return objList;
  }

  private static object? _ReadObject(JsonElement element, Type targetType, JsonSerializerOptions options) {
    if (targetType.IsGenericType) {
      var genericDef = targetType.GetGenericTypeDefinition();
      if (genericDef == typeof(Dictionary<,>) || genericDef == typeof(IDictionary<,>)) {
        var keyType = targetType.GetGenericArguments()[0];
        var valueType = targetType.GetGenericArguments()[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var dict = (IDictionary)Activator.CreateInstance(dictType)!;
        foreach (var prop in element.EnumerateObject()) {
          var key = keyType == typeof(string) ? (object)prop.Name : Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
          dict.Add(key, _ReadValue(prop.Value, valueType, options));
        }
        return dict;
      }
    }

    var instance = Activator.CreateInstance(targetType);
    if (instance == null)
      return null;

    var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
    var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    foreach (var prop in properties) {
      if (!prop.CanWrite)
        continue;
      var nameAttr = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
      var name = nameAttr.Length > 0 ? ((JsonPropertyNameAttribute)nameAttr[0]).Name : prop.Name;
      propertyMap[name] = prop;
    }

    var fieldMap = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
    if (options.IncludeFields) {
      var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
      foreach (var field in fields) {
        var nameAttr = field.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
        var name = nameAttr.Length > 0 ? ((JsonPropertyNameAttribute)nameAttr[0]).Name : field.Name;
        fieldMap[name] = field;
      }
    }

    foreach (var jsonProp in element.EnumerateObject()) {
      var name = jsonProp.Name;

      if (propertyMap.TryGetValue(name, out var prop)) {
        var propValue = _ReadValue(jsonProp.Value, prop.PropertyType, options);
        prop.SetValue(instance, propValue, null);
        continue;
      }

      if (options.IncludeFields && fieldMap.TryGetValue(name, out var field)) {
        var fieldValue = _ReadValue(jsonProp.Value, field.FieldType, options);
        field.SetValue(instance, fieldValue);
      }
    }

    return instance;
  }

}

#endif
