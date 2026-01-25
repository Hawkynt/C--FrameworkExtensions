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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Guard;

namespace System.Windows.Forms;

/// <summary>
/// Shared utility methods for list control extensions (ListView, ListBox, ComboBox).
/// </summary>
internal static class ListControlExtensions {

  #region Property Getter Cache

  private static readonly ConcurrentDictionary<string, Func<object, object>> _PROPERTY_GETTER_CACHE = new();

  /// <summary>
  /// Gets a property value from an object with caching.
  /// </summary>
  /// <typeparam name="TValue">The expected type of the value.</typeparam>
  /// <param name="value">The object to get the property from.</param>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="defaultValueNullValue">Default when object is null.</param>
  /// <param name="defaultValueNoProperty">Default when property name is null.</param>
  /// <param name="defaultValuePropertyNotFound">Default when property doesn't exist.</param>
  /// <param name="defaultValuePropertyWrongType">Default when property type doesn't match.</param>
  /// <returns>The property value or appropriate default.</returns>
  internal static TValue GetPropertyValueOrDefault<TValue>(
    object value,
    string propertyName,
    TValue defaultValueNullValue,
    TValue defaultValueNoProperty,
    TValue defaultValuePropertyNotFound,
    TValue defaultValuePropertyWrongType
  ) {
    if (value is null)
      return defaultValueNullValue;

    if (propertyName == null)
      return defaultValueNoProperty;

    var type = value.GetType();
    var key = type.FullName + "\0" + propertyName;

    if (!_PROPERTY_GETTER_CACHE.TryGetValue(key, out var getter)) {
      var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (property == null) {
        _PROPERTY_GETTER_CACHE[key] = null;
        return defaultValuePropertyNotFound;
      }

      getter = obj => property.GetValue(obj);
      _PROPERTY_GETTER_CACHE[key] = getter;
    }

    if (getter == null)
      return defaultValuePropertyNotFound;

    var result = getter(value);
    if (result is TValue typedResult)
      return typedResult;

    if (result == null && !typeof(TValue).IsValueType)
      return default;

    return defaultValuePropertyWrongType;
  }

  #endregion

  #region Attribute Cache

  private static readonly ConcurrentDictionary<Type, ListItemStyleAttribute[]> _STYLE_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<Type, ListItemImageAttribute> _IMAGE_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<string, ListViewColumnAttribute> _COLUMN_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<string, ListViewColumnColorAttribute[]> _COLUMN_COLOR_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<string, ListViewRepeatedImageAttribute> _REPEATED_IMAGE_ATTRIBUTE_CACHE = new();
  private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _COLUMN_PROPERTIES_CACHE = new();

  /// <summary>
  /// Gets the style attributes for a type.
  /// </summary>
  internal static ListItemStyleAttribute[] GetStyleAttributes(Type type)
    => _STYLE_ATTRIBUTE_CACHE.GetOrAdd(type, t =>
      t.GetCustomAttributes(typeof(ListItemStyleAttribute), true)
        .Cast<ListItemStyleAttribute>()
        .ToArray()
    );

  /// <summary>
  /// Gets the image attribute for a type.
  /// </summary>
  internal static ListItemImageAttribute GetImageAttribute(Type type)
    => _IMAGE_ATTRIBUTE_CACHE.GetOrAdd(type, t =>
      t.GetCustomAttributes(typeof(ListItemImageAttribute), true)
        .Cast<ListItemImageAttribute>()
        .FirstOrDefault()
    );

  /// <summary>
  /// Gets the column attribute for a property.
  /// </summary>
  internal static ListViewColumnAttribute GetColumnAttribute(Type type, string propertyName) {
    var key = type.FullName + "\0" + propertyName;
    return _COLUMN_ATTRIBUTE_CACHE.GetOrAdd(key, _ => {
      var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      return property?.GetCustomAttribute<ListViewColumnAttribute>();
    });
  }

  /// <summary>
  /// Gets the column color attributes for a property.
  /// </summary>
  internal static ListViewColumnColorAttribute[] GetColumnColorAttributes(Type type, string propertyName) {
    var key = type.FullName + "\0" + propertyName;
    return _COLUMN_COLOR_ATTRIBUTE_CACHE.GetOrAdd(key, _ => {
      var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      return property?.GetCustomAttributes<ListViewColumnColorAttribute>().ToArray() ?? [];
    });
  }

  /// <summary>
  /// Gets the repeated image attribute for a property.
  /// </summary>
  internal static ListViewRepeatedImageAttribute GetRepeatedImageAttribute(Type type, string propertyName) {
    var key = type.FullName + "\0" + propertyName;
    return _REPEATED_IMAGE_ATTRIBUTE_CACHE.GetOrAdd(key, _ => {
      var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
      return property?.GetCustomAttribute<ListViewRepeatedImageAttribute>();
    });
  }

  /// <summary>
  /// Gets properties with ListViewColumnAttribute ordered by DisplayIndex.
  /// </summary>
  internal static PropertyInfo[] GetColumnProperties(Type type)
    => _COLUMN_PROPERTIES_CACHE.GetOrAdd(type, t => {
      var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetCustomAttribute<ListViewColumnAttribute>() != null)
        .ToList();

      // Sort by DisplayIndex, then by declaration order for same index
      props.Sort((a, b) => {
        var attrA = a.GetCustomAttribute<ListViewColumnAttribute>();
        var attrB = b.GetCustomAttribute<ListViewColumnAttribute>();
        var indexA = attrA?.DisplayIndex ?? -1;
        var indexB = attrB?.DisplayIndex ?? -1;

        if (indexA == -1 && indexB == -1)
          return 0;
        if (indexA == -1)
          return 1;
        if (indexB == -1)
          return -1;
        return indexA.CompareTo(indexB);
      });

      return props.ToArray();
    });

  #endregion

  #region Display Value Formatting

  /// <summary>
  /// Gets the display value for a property with optional formatting.
  /// </summary>
  internal static string GetDisplayValue(object data, PropertyInfo property, ListViewColumnAttribute columnAttr) {
    var value = property.GetValue(data);
    if (value == null)
      return string.Empty;

    if (columnAttr?.Format != null && value is IFormattable formattable)
      return formattable.ToString(columnAttr.Format, null);

    return value.ToString();
  }

  #endregion
}
