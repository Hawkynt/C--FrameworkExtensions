#region (c)2010-2020 Hawkynt
/*
  @this file is part of Hawkynt's .NET Framework extensions.

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable MemberCanBePrivate.Global

namespace System {
  internal static partial class ObjectExtensions {
    /// <summary>
    /// Gets the property values of the object.
    /// </summary>
    /// <param name="this">This Object.</param>
    /// <param name="flattenHierarchy">if set to <c>true</c> flattens the hierarchy.</param>
    /// <param name="allowNonPublic">if set to <c>true</c> allows non public to be returned.</param>
    /// <param name="specialNames">if set to <c>true</c> special names will also be returned.</param>
    /// <param name="exceptionHandler">The exception handler that returns a value on exceptions, if needed.</param>
    /// <returns>A collection of KeyValuePairs.</returns>
    public static Dictionary<string, object> GetProperties(this object @this, bool flattenHierarchy = true, bool allowNonPublic = true, bool specialNames = true, Func<Exception, object> exceptionHandler = null) {
      Contract.Requires(@this != null);
      var result = new Dictionary<string, object>();
      var type = @this.GetType();
      var flags =
        (flattenHierarchy ? BindingFlags.FlattenHierarchy : 0) |
        (allowNonPublic ? BindingFlags.NonPublic : 0) |
          (BindingFlags.Instance | BindingFlags.Public);

      foreach (var prop in type.GetProperties(flags)) {
        if (!prop.CanRead)
          continue;

        if (prop.IsSpecialName && !specialNames)
          continue;

        object value;
        try {
          value = prop.GetValue(@this, null);
        } catch (Exception e) {
          value = exceptionHandler?.Invoke(e);
        }

        result.AddOrUpdate(prop.Name, value);
      }

      return (result);
    }

    /// <summary>
    /// Gets the field values of the object.
    /// </summary>
    /// <param name="this">This Object.</param>
    /// <param name="flattenHierarchy">if set to <c>true</c> flattens the hierarchy.</param>
    /// <param name="allowNonPublic">if set to <c>true</c> allows non public to be returned.</param>
    /// <param name="specialNames">if set to <c>true</c> special names will also be returned.</param>
    /// <param name="exceptionHandler">The exception handler that returns a value on exceptions, if needed.</param>
    /// <returns>A collection of KeyValuePairs.</returns>
    public static Dictionary<string, object> GetFields(this object @this, bool flattenHierarchy = true, bool allowNonPublic = true, bool specialNames = true, Func<Exception, object> exceptionHandler = null) {
      Contract.Requires(@this != null);

      var result = new Dictionary<string, object>();
      var type = @this.GetType();
      var flags =
        (flattenHierarchy ? BindingFlags.FlattenHierarchy : 0) |
        (allowNonPublic ? BindingFlags.NonPublic : 0) |
          (BindingFlags.Instance | BindingFlags.Public);

      foreach (var field in type.GetFields(flags)) {
        if (field.IsSpecialName && !specialNames)
          continue;

        object value;
        try {
          value = field.GetValue(@this);
        } catch (Exception e) {
          value = exceptionHandler?.Invoke(e);
        }

        result.AddOrUpdate(field.Name, value);
      }

      return (result);
    }

    /// <summary>
    /// Resets the default values on properties that have one.
    /// </summary>
    /// <param name="this">This Object.</param>
    /// <param name="flattenHierarchy">if set to <c>true</c> flattens the hierarchy.</param>
    public static void ResetDefaultValues(this object @this, bool flattenHierarchy = true) {
      Contract.Requires(@this != null);

      var type = @this.GetType();
      var flags =
        (flattenHierarchy ? BindingFlags.FlattenHierarchy : 0) |
        BindingFlags.NonPublic |
        (BindingFlags.Instance | BindingFlags.Public);

      foreach (var prop in type.GetProperties(flags)) {
        var defaultValueAttribute = prop.GetCustomAttributes(typeof(DefaultValueAttribute), flattenHierarchy).Cast<DefaultValueAttribute>().FirstOrDefault();
        if (defaultValueAttribute == null)
          continue;
        prop.SetValue(@this, defaultValueAttribute.Value, null);
      }
    }

    /// <summary>
    /// Determines whether this object is of a specific type.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="this">This Object.</param>
    /// <returns>
    ///   <c>true</c> if the given object is of the specific type; otherwise, <c>false</c>.
    /// </returns>
    public static bool Is<TType>(this object @this) => @this is TType;

    /// <summary>
    /// Determines whether this object is of any specified type.
    /// </summary>
    /// <param name="this">This Object.</param>
    /// <param name="types">The types.</param>
    /// <returns>
    ///   <c>true</c> if the given object is of the specific type; otherwise, <c>false</c>.
    /// </returns>
    public static bool TypeIsAnyOf(this object @this, params Type[] types) {
      Contract.Requires(types != null);
      if (@this == null) {
        return (types.Any(t => t == null || !t.IsValueType));
      }

      var type = @this.GetType();
      return (types.Any(t => t == type));
    }

    public static bool TypeIsAnyOf<TType1, TType2>(this object @this) => TypeIsAnyOf(@this, typeof(TType1), typeof(TType2));
    public static bool TypeIsAnyOf<TType1, TType2, TType3>(this object @this) => TypeIsAnyOf(@this, typeof(TType1), typeof(TType2), typeof(TType3));
    public static bool TypeIsAnyOf<TType1, TType2, TType3, TType4>(this object @this) => TypeIsAnyOf(@this, typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4));
    public static bool TypeIsAnyOf<TType1, TType2, TType3, TType4, TType5>(this object @this) => TypeIsAnyOf(@this, typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4), typeof(TType5));
    public static bool TypeIsAnyOf<TType1, TType2, TType3, TType4, TType5, TType6>(this object @this) => TypeIsAnyOf(@this, typeof(TType1), typeof(TType2), typeof(TType3), typeof(TType4), typeof(TType5), typeof(TType6));

    /// <summary>
    /// Determines whether this object is of a specific type.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="this">This object.</param>
    /// <returns>
    ///   <c>true</c> if the given object is of the specific type; otherwise, <c>false</c>.
    /// </returns>
    public static TType As<TType>(this object @this) where TType : class => @this as TType;

    /// <summary>
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <param name="this">This Object.</param>
    /// <param name="condition">The predicate.</param>
    /// <returns>
    ///   <c>true</c> if the specified object matches the condition; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsTrue<TType>(this TType @this, Predicate<TType> condition) => condition(@this);

    /// <summary>
    /// Determines whether the specified condition is false.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <param name="this">This Object.</param>
    /// <param name="condition">The predicate.</param>
    /// <returns>
    ///   <c>false</c> if the specified object matches the condition; otherwise, <c>true</c>.
    /// </returns>
    public static bool IsFalse<TType>(this TType @this, Predicate<TType> condition) => !condition(@this);

    /// <summary>
    /// Determines whether the specified value is any of the given ones.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="values">The values.</param>
    /// <returns><c>true</c> if it is; otherwise, <c>false</c></returns>
    public static bool IsAnyOf<TType>(this TType @this, IEnumerable<TType> values) => values.Any(i => Equals(i, @this));

    /// <summary>
    /// Determines whether the specified value is any of the given ones.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="values">The values.</param>
    /// <returns><c>true</c> if it is; otherwise, <c>false</c></returns>
    public static bool IsAnyOf<TType>(this TType @this, params TType[] values) => IsAnyOf(@this, (IEnumerable<TType>)values);
  }
}