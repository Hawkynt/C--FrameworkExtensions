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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel;

public static partial class DefaultValueAttributeExtensions {
  /// <summary>
  ///   Sets the properties of an instance to their default values.
  /// </summary>
  /// <typeparam name="TType">The type of the instance.</typeparam>
  /// <param name="this">This Instance.</param>
  /// <param name="alsoNonPublic">if set to <c>true</c> non-public properties are also set.</param>
  /// <param name="flattenHierarchies">if set to <c>true</c> [flatten hierarchies].</param>
  public static void SetPropertiesToDefaultValues<TType>(this TType @this, bool alsoNonPublic = false, bool flattenHierarchies = true) {
    var type = @this is null ? typeof(TType) : @this.GetType();
    var properties = type.GetProperties(
      BindingFlags.Instance 
      | (alsoNonPublic ? BindingFlags.NonPublic : 0) 
      | BindingFlags.Public 
      | (flattenHierarchies ? BindingFlags.FlattenHierarchy : 0)
      | (!flattenHierarchies ? BindingFlags.DeclaredOnly : 0)
      );
    var writableProperties = properties.Where(p => p.CanWrite);
    foreach (var prop in writableProperties) {
      var defaultValueAttributes = prop.GetCustomAttributes(typeof(DefaultValueAttribute), false).OfType<DefaultValueAttribute>();
      var attr = defaultValueAttributes.FirstOrDefault();
      if (attr == null)
        continue;
      var value = attr.Value;
      try {
        var targetValue = _TryChangeType(prop.PropertyType, value);
        prop.SetValue(@this, targetValue, null);
      } catch (Exception) {
        Trace.WriteLine($"Could not set property to default: type:{type}, property:{prop.Name}, value:{value}");
      }
    }

    return;

    static object _TryChangeType(Type targetType, object value) {
      if (value == null)
        return null;

      var sourceType = value.GetType();
      if (sourceType == targetType)
        return value;

      // Special handling for enums
      if (targetType.IsEnum)
        return value is string str ? Enum.Parse(targetType, str) : Enum.ToObject(targetType, value);

      // Use Convert.ChangeType for built-in supported conversions
      try {
        if (IsCastableTo(sourceType, targetType))
          return Convert.ChangeType(value, targetType);
      } catch {
        // Fall through
      }

      // Manually handle numeric conversions
      if (targetType.IsPrimitive && value is IConvertible convertible) {
        try {
          // unchecked avoids overflow exceptions, like regular implicit C# casts
          return unchecked(targetType switch {
            not null when targetType == typeof(short) => (short)convertible.ToInt32(null),
            not null when targetType == typeof(ushort) => (ushort)convertible.ToUInt32(null),
            not null when targetType == typeof(byte) => (byte)convertible.ToByte(null),
            not null when targetType == typeof(sbyte) => (sbyte)convertible.ToSByte(null),
            not null when targetType == typeof(int) => convertible.ToInt32(null),
            not null when targetType == typeof(uint) => convertible.ToUInt32(null),
            not null when targetType == typeof(long) => convertible.ToInt64(null),
            not null when targetType == typeof(ulong) => convertible.ToUInt64(null),
            not null when targetType == typeof(float) => convertible.ToSingle(null),
            not null when targetType == typeof(double) => convertible.ToDouble(null),
            not null when targetType == typeof(decimal) => convertible.ToDecimal(null),
            not null when targetType == typeof(char) => convertible.ToChar(null),
            _ => throw new InvalidOperationException()
          });
        } catch {
          // Final fallback: failure
        }
      }

      throw new InvalidOperationException($"Cannot convert from {sourceType} to {targetType}");

      bool IsCastableTo(Type from, Type to) =>
        to.IsAssignableFrom(from) || _IMPLICIT_CONVERSIONS.TryGetValue(to, out var list) && list.Contains(from);
    }

  }

  private static readonly Dictionary<Type, Type[]> _IMPLICIT_CONVERSIONS = new() {
    {
      typeof(decimal), [
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(char)
      ]
    }, {
      typeof(double), [
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(char),
        typeof(float)
      ]
    }, {
      typeof(float), [
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(char)
      ]
    },
    { typeof(ulong), [typeof(byte), typeof(ushort), typeof(uint), typeof(char)] },
    { typeof(long), [typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(char)] },
    { typeof(uint), [typeof(byte), typeof(ushort), typeof(char)] },
    { typeof(int), [typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char)] },
    { typeof(ushort), [typeof(byte), typeof(char)] },
    { typeof(short), [typeof(byte)] }
  };

}
