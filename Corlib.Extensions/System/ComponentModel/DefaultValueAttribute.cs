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
    var properties = type.GetProperties(BindingFlags.Instance | (alsoNonPublic ? BindingFlags.NonPublic : 0) | BindingFlags.Public | (flattenHierarchies ? BindingFlags.FlattenHierarchy : 0));
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

  private static object _TryChangeType(Type targetType, object value) {
    if (value == null)
      return null;

    var sourceType = value.GetType();
    if (sourceType == targetType)
      return value;

    if (IsCastableTo(sourceType, targetType))
      return Convert.ChangeType(value, targetType);

    if (IsIntegerType(targetType) && IsIntegerType(sourceType) && IsSigned(sourceType) && !IsSigned(targetType) && Math.Sign((float)Convert.ChangeType(value, TypeCode.Single)) >= 0)
      return Convert.ChangeType(value, targetType);

    throw new InvalidOperationException($"Can not convert from {sourceType.FullName} to {targetType.FullName}");

    bool IsCastableTo(Type @this, Type target) {
      // check inheritance
      if (target.IsAssignableFrom(@this))
        return true;

      // check cache
      if (_IMPLICIT_CONVERSIONS.TryGetValue(target, out var source) && source.Contains(@this))
        return true;

      return @this.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Any(
          m => m.ReturnType == target &&
               m.Name == "op_Implicit" ||
               m.Name == "op_Explicit"
        );
    }

    bool IsNullable(Type @this) => @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);

    bool IsIntegerType(Type @this) => @this == typeof(byte) || @this == typeof(sbyte) || @this == typeof(short) || @this == typeof(ushort) || @this == typeof(int) || @this == typeof(uint) || @this == typeof(long) || @this == typeof(ulong);

    bool IsSigned(Type @this) => @this == typeof(sbyte) || @this == typeof(short) || @this == typeof(int) || @this == typeof(long) || @this == typeof(float) || @this == typeof(double) || @this == typeof(decimal) || (IsNullable(@this) && IsSigned(@this.GetGenericArguments()[0]));
  }
}
