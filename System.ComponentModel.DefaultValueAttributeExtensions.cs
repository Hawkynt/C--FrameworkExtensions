#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel {
  internal static partial class DefaultValueAttributeExtensions {
    /// <summary>
    /// Sets the properties of an instance to their default values.
    /// </summary>
    /// <typeparam name="TType">The type of the instance.</typeparam>
    /// <param name="This">This Instance.</param>
    /// <param name="alsoNonPublic">if set to <c>true</c> non-public properties are also set.</param>
    /// <param name="flattenHierarchies">if set to <c>true</c> [flatten hierarchies].</param>
    public static void SetPropertiesToDefaultValues<TType>(this TType This, bool alsoNonPublic = false, bool flattenHierarchies = true) {
      var type = ReferenceEquals(This, null) ? typeof(TType) : This.GetType();
      var properties = type.GetProperties(BindingFlags.Instance | (alsoNonPublic ? BindingFlags.NonPublic : 0) | BindingFlags.Public | (flattenHierarchies ? BindingFlags.FlattenHierarchy : 0));
      var writeableProperties = properties.Where(p => p.CanWrite);
      foreach (var prop in writeableProperties) {
        var defaultValueAttributes = prop.GetCustomAttributes(typeof(DefaultValueAttribute), false).OfType<DefaultValueAttribute>();
        var attr = defaultValueAttributes.FirstOrDefault();
        if (attr == null) continue;
        var value = attr.Value;
        try {
          var targetValue = _TryChangeType(prop.PropertyType, value);
          prop.SetValue(This, targetValue, null);
        } catch (Exception) {
          Trace.WriteLine($"Could not set property to default: type:{type}, property:{prop.Name}, value:{value}");
        }
      }
    }

    private static object _TryChangeType(Type targetType, object value) {
      if (ReferenceEquals(value, null))
        return (null);
      var sourceType = value.GetType();
      if (sourceType == targetType)
        return (value);
      if (sourceType.IsCastableTo(targetType))
        return (Convert.ChangeType(value, targetType));
      if (targetType.IsIntegerType() && sourceType.IsIntegerType() && sourceType.IsSigned() && !targetType.IsSigned() && Math.Sign((float)Convert.ChangeType(value, TypeCode.Single)) >= 0)
        return (Convert.ChangeType(value, targetType));
      throw new InvalidOperationException($"Can not convert from {sourceType.FullName} to {targetType.FullName}");
    }
  }
}
