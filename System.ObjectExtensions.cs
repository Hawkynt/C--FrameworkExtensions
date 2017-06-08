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

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

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
#if NETFX_4
      Contract.Requires(@this != null);
#endif
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

        if (result.ContainsKey(prop.Name))
          result[prop.Name] = value;
        else
          result.Add(prop.Name, value);
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
#if NETFX_4
      Contract.Requires(@this != null);
#endif

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

        if (result.ContainsKey(field.Name))
          result[field.Name] = value;
        else
          result.Add(field.Name, value);
      }

      return (result);
    }

    /// <summary>
    /// Resets the default values on properties that have one.
    /// </summary>
    /// <param name="this">This Object.</param>
    /// <param name="flattenHierarchy">if set to <c>true</c> flattens the hierarchy.</param>
    public static void ResetDefaultValues(this object @this, bool flattenHierarchy = true) {
#if NETFX_4
      Contract.Requires(@this != null);
#endif

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
#if NETFX_4
      Contract.Requires(types != null);
#endif
      if (@this == null)
        return (types.Any(t => t == null || !t.IsValueType));

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

    /// <summary>
    /// Executes code when the given object is <c>null</c>.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="action">The action to execute.</param>
    public static void WhenNull<TType>(this TType @this, Action action) where TType : class {
      if (ReferenceEquals(@this, null))
        action();
    }

    /// <summary>
    /// Executes a function when the given object is <c>null</c>.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="function">The function to execute.</param>
    /// <param name="defaultValue">The default value to return when not <c>null</c>.</param>
    /// <returns>The result of the function or the default value.</returns>
    public static TResult WhenNull<TType, TResult>(this TType @this, Func<TResult> function, TResult defaultValue = default(TResult)) where TType : class => ReferenceEquals(@this, null) ? function() : defaultValue;

    /// <summary>
    /// Executes code when the given object is not <c>null</c>.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="action">The action to execute.</param>
    public static void WhenNotNull<TType>(this TType @this, Action<TType> action) where TType : class {
      if (!ReferenceEquals(@this, null))
        action(@this);
    }

    /// <summary>
    /// Executes a function when the given object is not <c>null</c>.
    /// </summary>
    /// <typeparam name="TType">The type of the object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="this">This object.</param>
    /// <param name="function">The function to execute.</param>
    /// <param name="defaultValue">The default value to return when <c>null</c>.</param>
    /// <returns>The result of the function or the default value.</returns>
    public static TResult WhenNotNull<TType, TResult>(this TType @this, Func<TType, TResult> function, TResult defaultValue = default(TResult)) where TType : class => ReferenceEquals(@this, null) ? defaultValue : function(@this);

    /// <summary>
    /// Gets the number of bytes used by the given value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This value.</param>
    /// <returns>The used memory in bytes.</returns>
    public static long GetMemorySize<TValue>(this TValue @this) => _GetMemorySize(@this, typeof(TValue).IsValueType);

    /// <summary>
    /// Gets the number of bytes used by the given value.
    /// </summary>
    /// <param name="this">This value.</param>
    /// <returns>The used memory in bytes.</returns>
    public static long GetMemorySize(this object @this) => _GetMemorySize(@this, false);

    /// <summary>
    /// Gets the number of bytes used by the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="withoutBoxing">if set to <c>true</c>, returns value without pointer size for boxing.</param>
    /// <param name="visitedRefs">The visited references to avoid duplicate calculation of circular references.</param>
    /// <returns>
    /// The used memory in bytes.
    /// </returns>
    private static long _GetMemorySize(object value, bool withoutBoxing, HashSet<object> visitedRefs = null) {
      var pointerSize = IntPtr.Size;

      if (ReferenceEquals(null, value))
        return pointerSize;

      var type = value.GetType();
      if (type.IsPrimitive)
        return Marshal.SizeOf(type) + (withoutBoxing ? 0 : pointerSize);

      if (visitedRefs == null)
        visitedRefs = new HashSet<object>();

      if (type.IsArray) {
        if (visitedRefs.Contains(value))
          return pointerSize;

        var valueElements = type.GetElementType().IsValueType;
        return pointerSize + sizeof(int) + ((IEnumerable)value).Cast<object>().Sum(v => _GetMemorySize(v, valueElements, visitedRefs));
      }

      var fields = type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      long sum = withoutBoxing ? 0 : pointerSize;
      foreach (var field in fields) {
        var fieldValue = field.GetValue(value);
        if (visitedRefs.Contains(fieldValue))
          sum += pointerSize;
        else {
          sum += _GetMemorySize(fieldValue, field.FieldType.IsValueType, visitedRefs);
          visitedRefs.Add(fieldValue);
        }
      }

      return sum;
    }

    /// <summary>
    /// Serializes this item to a XML file.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This item.</param>
    /// <param name="file">The file to write to.</param>
    public static void ToXmlFile<TItem>(this TItem @this, FileInfo file) {
      using (var stream = file.OpenWrite()) {
        stream.SetLength(0);
        _GetSerializerForType<TItem>().Serialize(stream, @this);
      }
    }

    private static readonly ConcurrentDictionary<Type, XmlSerializer> _CACHE = new ConcurrentDictionary<Type, XmlSerializer>();
    /// <summary>
    /// Creates an item from a XML file.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="file">The file.</param>
    /// <returns>The deserialized item</returns>
    public static TItem FromXmlFile<TItem>(FileInfo file) {
      using (var stream = file.OpenRead())
        return (TItem)_GetSerializerForType<TItem>().Deserialize(stream);
    }

    private static XmlSerializer _GetSerializerForType<TType>()
      => _CACHE.GetOrAdd(typeof(TType), t => new XmlSerializer(t))
      ;
  }
}