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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET35
using System.Diagnostics;
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Reflection;

namespace System {
  internal static partial class TypeExtensions {
    /// <summary>
    /// Holds information about properties which are important for designer components.
    /// </summary>
    public struct PropertyDesignerDetails {
      /// <summary>
      /// The underlying PropertyInfo
      /// </summary>
      public readonly PropertyInfo Info;

      /// <summary>
      /// Cache
      /// </summary>
      private string __name;
      /// <summary>
      /// Gets the name.
      /// </summary>
      public string Name { get { return this.__name ?? (this.__name = (this.Info == null ? null : this.Info.Name)); } }

      /// <summary>
      /// Gets a value indicating whether this instance is readable.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is readable; otherwise, <c>false</c>.
      /// </value>
      public bool IsReadable { get { return (this.Info != null && this.Info.CanRead); } }

      /// <summary>
      /// Gets a value indicating whether this instance is writable.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is writable; otherwise, <c>false</c>.
      /// </value>
      public bool IsWritable { get { return (this.Info != null && this.Info.CanWrite && !this.ReadOnly); } }

      /// <summary>
      /// Cache
      /// </summary>
      private string __description;
      /// <summary>
      /// Gets the descriptions.
      /// </summary>
      public string Descriptions {
        get {
          return (this.__description ?? (this.__description = this._customAttributes.OfType<DescriptionAttribute>().Where(i => i != null).Select(i => i.Description).FirstOrDefault()));
        }
      }

      /// <summary>
      /// Cache
      /// </summary>
      private string __displayName;
      /// <summary>
      /// Gets the descriptions.
      /// </summary>
      public string DisplayName {
        get {
          return (this.__displayName ?? (this.__displayName = this._customAttributes.OfType<DisplayNameAttribute>().Where(i => i != null).Select(i => i.DisplayName).FirstOrDefault()));
        }
      }

      /// <summary>
      /// Cache
      /// </summary>
      private string __category;
      /// <summary>
      /// Gets the categories.
      /// </summary>
      public string Category {
        get {
          return (this.__category ?? (this.__category = this._customAttributes.OfType<CategoryAttribute>().Where(i => i != null).Select(i => i.Category).FirstOrDefault()));
        }
      }

      /// <summary>
      /// Cache
      /// </summary>
      private bool? __readOnly;
      /// <summary>
      /// Gets the categories.
      /// </summary>
      public bool ReadOnly {
        get {
          return (bool)(this.__readOnly ?? (this.__readOnly = this._customAttributes.OfType<ReadOnlyAttribute>().Where(i => i != null).Any(i => i.IsReadOnly)));
        }
      }

      /// <summary>
      /// Cache
      /// </summary>
      private IEnumerable<EditorBrowsableState> __editorBrowseableStates;
      /// <summary>
      /// Gets the editor browseable states.
      /// </summary>
      public IEnumerable<EditorBrowsableState> EditorBrowseableStates {
        get {
          return (this.__editorBrowseableStates ?? (this.__editorBrowseableStates = this._customAttributes.OfType<EditorBrowsableAttribute>().Where(i => i != null).Select(i => i.State)));
        }
      }

      /// <summary>
      /// Cache
      /// </summary>
      private Type __propertyType;
      /// <summary>
      /// Gets the type of the property.
      /// </summary>
      /// <value>
      /// The type of the property.
      /// </value>
      public Type PropertyType {
        get { return this.__propertyType ?? (this.__propertyType = (this.Info == null ? TypeObject : this.Info.PropertyType)); }
      }
      #region value getter
      public object GetValue(object instance = null) {
        var propertyInfo = this.Info;
#if !NET35
        Contract.Assume(propertyInfo != null);
#endif
        return (propertyInfo.GetValue(instance, null));
      }
      public TValue GetValue<TValue>(object instance = null) {
        return ((TValue)this.GetValue(instance));
      }
      public object GetValueOrDefault(object instance = null, object defaultValue = null) {
        try {
          return (this.GetValue(instance));
        } catch {
          return (defaultValue);
        }
      }
      public TValue GetValueOrDefault<TValue>(object instance = null) {
        try {
          return (this.GetValue<TValue>(instance));
        } catch {
          return (default(TValue));
        }
      }
      public TValue GetValueOrDefault<TValue>(object instance, TValue defaultValue) {
        try {
          return (this.GetValue<TValue>(instance));
        } catch {
          return (defaultValue);
        }
      }
      public TValue GetValueOrDefault<TValue>(TValue defaultValue) {
        try {
          return (this.GetValue<TValue>());
        } catch {
          return (defaultValue);
        }
      }

      public bool TryGetValue(out object value) {
        try {
          value = (this.GetValue());
          return (true);
        } catch {
          value = null;
          return (false);
        }
      }
      public bool TryGetValue(object instance, out object value) {
        try {
          value = (this.GetValue(instance));
          return (true);
        } catch {
          value = null;
          return (false);
        }
      }
      public bool TryGetValue<TValue>(out TValue value) {
        try {
          value = (this.GetValue<TValue>());
          return (true);
        } catch {
          value = default(TValue);
          return (false);
        }
      }
      public bool TryGetValue<TValue>(object instance, out TValue value) {
        try {
          value = (this.GetValue<TValue>(instance));
          return (true);
        } catch {
          value = default(TValue);
          return (false);
        }
      }
      #endregion
      #region value setter
      public void SetValue(object value) {
        var propertyInfo = this.Info;
#if !NET35
        Contract.Assume(propertyInfo != null);
#endif
        propertyInfo.SetValue(null, value, null);
      }
      public void SetValue(object instance, object value) {
        var propertyInfo = this.Info;
#if !NET35
        Contract.Assume(propertyInfo != null);
#endif
        propertyInfo.SetValue(instance, value, null);
      }
      public bool TrySetValue(object value) {
        var propertyInfo = this.Info;
#if !NET35
        Contract.Assume(propertyInfo != null);
#endif
        try {
          propertyInfo.SetValue(null, value, null);
          return (true);
        } catch (Exception e) {
          Trace.WriteLine(string.Format("Could not set static property {0}: {1}", this.Name, e));
          return (false);
        }
      }
      public bool TrySetValue(object instance, object value) {
        var propertyInfo = this.Info;
#if !NET35
        Contract.Assume(propertyInfo != null);
#endif
        try {
          propertyInfo.SetValue(instance, value, null);
          return (true);
        } catch (Exception e) {
          Trace.WriteLine(string.Format("Could not set instance property {0}: {1}", this.Name, e));
          return (false);
        }
      }
      #endregion
      /// <summary>
      /// Cache
      /// </summary>
      private object[] __customAttributes;
      /// <summary>
      /// Gets all custom attributes.
      /// </summary>
      private object[] _customAttributes { get { return (this.__customAttributes ?? (this.__customAttributes = this.Info == null ? new object[0] : this.Info.GetCustomAttributes(true))); } }

      public PropertyDesignerDetails(PropertyInfo info)
        : this() {
#if !NET35
        Contract.Requires(info != null);
#endif
        this.Info = info;
      }
    }

    #region specific type
    public static readonly Type TypeVoid = typeof(void);

    public static readonly Type TypeBool = typeof(bool);
    public static readonly Type TypeChar = typeof(char);
    public static readonly Type TypeByte = typeof(byte);
    public static readonly Type TypeSByte = typeof(sbyte);
    public static readonly Type TypeShort = typeof(short);
    public static readonly Type TypeWord = typeof(ushort);
    public static readonly Type TypeInt = typeof(int);
    public static readonly Type TypeDWord = typeof(uint);
    public static readonly Type TypeLong = typeof(long);
    public static readonly Type TypeQWord = typeof(ulong);
    public static readonly Type TypeFloat = typeof(float);
    public static readonly Type TypeDouble = typeof(double);
    public static readonly Type TypeDecimal = typeof(decimal);
    public static readonly Type TypeString = typeof(string);
    public static readonly Type TypeObject = typeof(object);
    public static readonly Type TypeTimeSpan = typeof(TimeSpan);
    public static readonly Type TypeDateTime = typeof(DateTime);
    #endregion

    #region type conversion
    private static readonly Dictionary<Type, Type[]> _IMPLICIT_CONVERSIONS = new Dictionary<Type, Type[]> {
        { TypeDecimal, new [] { TypeSByte, TypeByte, TypeShort, TypeWord, TypeInt, TypeDWord, TypeLong, TypeQWord, TypeChar } },
        { TypeDouble, new []{ TypeSByte, TypeByte, TypeShort, TypeWord, TypeInt, TypeDWord, TypeLong, TypeQWord, TypeChar, TypeFloat } },
        { TypeFloat, new [] { TypeSByte, TypeByte, TypeShort, TypeWord, TypeInt, TypeDWord, TypeLong, TypeQWord, TypeChar } },
        { TypeQWord, new [] { TypeByte, TypeWord, TypeDWord, TypeChar } },
        { TypeLong, new []{ TypeSByte, TypeByte, TypeShort, TypeWord, TypeInt, TypeDWord, TypeChar } },
        { TypeDWord, new [] { TypeByte, TypeWord, TypeChar } },
        { TypeInt, new []{ TypeSByte, TypeByte, TypeShort, TypeWord, TypeChar } },
        { TypeWord, new [] { TypeByte, TypeChar } },
        { TypeShort, new []{ TypeByte } }
    };

    /// <summary>
    /// Determines whether a given type can be casted to another one.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <param name="target">Target type.</param>
    /// <returns>
    ///   <c>true</c> if the given type can be casted to the target; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsCastableTo(this Type This, Type target) {
#if !NET35
      Contract.Requires(This != null);
      Contract.Requires(target != null);

#else
      Diagnostics.Debug.Assert(This != null);
      Diagnostics.Debug.Assert(target != null);
#endif
      // check inheritance
      if (target.IsAssignableFrom(This))
        return true;

      // check cache
      if (_IMPLICIT_CONVERSIONS.ContainsKey(target)) {
        Contract.Assume(_IMPLICIT_CONVERSIONS[target] != null);
        if (_IMPLICIT_CONVERSIONS[target].Contains(This))
          return true;
      }
      return (
        This.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Any(
          m => m.ReturnType == target &&
          m.Name == "op_Implicit" ||
          m.Name == "op_Explicit"
        )
      );
    }

    /// <summary>
    /// Determines whether the given type can be casted from the given source type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <param name="source">The source type.</param>
    /// <returns>
    ///   <c>true</c> if this Type can be casted from the source; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsCastableFrom(this Type This, Type source) {
#if !NET35
      Contract.Requires(This != null);
      Contract.Requires(source != null);

#else
      Diagnostics.Debug.Assert(This != null);
      Diagnostics.Debug.Assert(source != null);
#endif
      return (source.IsCastableTo(This));
    }
    #endregion

    #region messing for designers
    /// <summary>
    /// Cache
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>> _typeCache = new ConcurrentDictionary<Type, ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>>();
    /// <summary>
    /// Gets the designer detailed properties from a given type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <param name="bindingFlags">The bindingflags to use, defaults to Public and Instance.</param>
    /// <returns>An array of PropertyDesignerDetails.</returns>
    public static PropertyDesignerDetails[] GetDesignerProperties(this Type This, BindingFlags? bindingFlags = null) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif

      PropertyDesignerDetails[] result;

#if !NET35
      if (bindingFlags == null)
        bindingFlags = BindingFlags.Instance | BindingFlags.Public;

      // try to get from cache first
      ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]> inner;
      if (_typeCache.TryGetValue(This, out inner) && inner != null && inner.TryGetValue(bindingFlags.Value, out result))
        return (result);
#endif

      // harvest properties
      var props = This.GetProperties(bindingFlags.Value);
      result = Array.ConvertAll(props, i => new PropertyDesignerDetails(i));

#if !NET35
      // try to add to cache
      inner = _typeCache.GetOrAdd(This, i => new ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>());
      inner.TryAdd(bindingFlags.Value, result);
#endif
      return (result);
    }
    #endregion

    /// <summary>
    /// Gets the assembly attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This type.</param>
    /// <param name="inherit">if set to <c>true</c> inherited attributes would also be returned; otherwise, not.</param>
    /// <param name="index">The index to use if multiple attributes were found of that kind.</param>
    /// <returns>The given attribute instance.</returns>
    public static TAttribute GetAssemblyAttribute<TAttribute>(this Type This, bool inherit = false, int index = 0) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return ((TAttribute)This.Assembly.GetCustomAttributes(typeof(TAttribute), inherit)[index]);
    }

    /// <summary>
    /// Simples the name.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns></returns>
    public static string SimpleName(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      var name = This.FullName;
      return (name == null ? null : name.Contains(".") ? name.Substring(name.LastIndexOf('.') + 1) : name);
    }

    /// <summary>
    /// Determines whether the specified type is an integer type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the specified type is integer; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsIntegerType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeByte) || (This == TypeSByte) || (This == TypeShort) || (This == TypeWord) || (This == TypeInt) || (This == TypeDWord) || (This == TypeLong) || (This == TypeQWord);
    }

    /// <summary>
    /// Gets the minimum value of an int type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The minium value supported for this type.</returns>
    public static decimal GetMinValueForIntType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      if (This == TypeByte)
        return (byte.MinValue);
      if (This == TypeSByte)
        return (sbyte.MinValue);
      if (This == TypeWord)
        return (ushort.MinValue);
      if (This == TypeShort)
        return (short.MinValue);
      if (This == TypeDWord)
        return (uint.MinValue);
      if (This == TypeInt)
        return (int.MinValue);
      if (This == TypeQWord)
        return (ulong.MinValue);
      if (This == TypeLong)
        return (long.MinValue);
      throw new NotSupportedException();
    }

    /// <summary>
    /// Gets the maximum value of an int type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The maxium value supported for this type.</returns>
    public static decimal GetMaxValueForIntType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      if (This == TypeByte)
        return (byte.MaxValue);
      if (This == TypeSByte)
        return (sbyte.MaxValue);
      if (This == TypeWord)
        return (ushort.MaxValue);
      if (This == TypeShort)
        return (short.MaxValue);
      if (This == TypeDWord)
        return (uint.MaxValue);
      if (This == TypeInt)
        return (int.MaxValue);
      if (This == TypeQWord)
        return (ulong.MaxValue);
      if (This == TypeLong)
        return (long.MaxValue);
      throw new NotSupportedException();
    }

    /// <summary>
    /// Determines whether the specified this is signed.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <returns>
    ///   <c>true</c> if the specified this is signed; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSigned(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeSByte) || (This == TypeShort) || (This == TypeInt) || (This == TypeLong) || (This == TypeFloat) || (This == TypeDouble) || (This == TypeDecimal);
    }

    /// <summary>
    /// Determines whether the specified this is unsigned.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <returns>
    ///   <c>true</c> if the specified this is unsigned; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUnsigned(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (!This.IsSigned());
    }

    /// <summary>
    /// Determines whether the specified type is a float type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsFloatType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeFloat) || (This == TypeDouble) || (This == TypeDecimal);
    }

    /// <summary>
    /// Determines whether the specified type is a float type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDecimalType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeDecimal);
    }

    /// <summary>
    /// Determines whether the specified type is a string.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a string type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStringType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeString);
    }

    /// <summary>
    /// Determines whether the specified type is a boolean type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a boolean type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBooleanType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This == TypeBool);
    }
    /// <summary>
    /// Determines whether the specified type is a TimeSpan type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a TimeSpan type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsTimeSpanType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#endif
      return (This == TypeTimeSpan);
    }
    /// <summary>
    /// Determines whether the specified type is a DateTime type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a DateTime type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDateTimeType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#endif
      return (This == TypeDateTime);
    }

    /// <summary>
    /// Determines whether the specified type is an enum.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is an enum; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEnumType(this Type This) {
#if !NET35
      Contract.Requires(This != null);
#else
      Diagnostics.Debug.Assert(This != null);
#endif
      return (This.IsEnum);
    }

    /// <summary>
    /// Gets the attribute value.
    /// </summary>
    /// <typeparam name="TAttributeType">The type of the attribute type.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="getter">The getter.</param>
    /// <returns></returns>
    public static TValue GetFieldOrPropertyAttributeValue<TAttributeType, TValue>(this Type This, string fieldName, Func<TAttributeType, TValue> getter) where TAttributeType : Attribute {
#if !NET35
      Contract.Requires(This != null);
      Contract.Requires(fieldName != null);
      Contract.Requires(getter != null);
#else
      Diagnostics.Debug.Assert(This != null);
      Diagnostics.Debug.Assert(fieldName != null);
      Diagnostics.Debug.Assert(getter != null);
#endif
      object[] attributes;
      var field = This.GetField(fieldName);
      if (field != null) {
        attributes = field.GetCustomAttributes(typeof(TAttributeType), true);
      } else {
        var prop = This.GetProperty(fieldName);
        if (prop == null) throw new ArgumentException();
        attributes = prop.GetCustomAttributes(typeof(TAttributeType), true);
      }
      var value = getter(attributes.OfType<TAttributeType>().First());
      return (value);
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The value of the displayname attribute if any</returns>
    public static string GetDisplayName(this Type This) {
      Contract.Requires(This != null);
      var attribute = This.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault();
      return (attribute == null ? null : attribute.DisplayName);
    }

    /// <summary>
    /// Gets the description name.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The value of the description attribute if any.</returns>
    public static string GetDescription(this Type This) {
      Contract.Requires(This != null);
      var attribute = This.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
      return (attribute == null ? null : attribute.Description);
    }

    /// <summary>
    /// Gets the short name of the type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The part behind the last dot (.).</returns>
    public static string GetShortTypeName(this Type This) {
      Contract.Requires(This != null);
      var fullName = This.FullName;
      if (string.IsNullOrWhiteSpace(fullName))
        return (fullName);
      return (fullName.Split('.').LastOrDefault());
    }

    /// <summary>
    /// Gets the implemented types of the given interface/base type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>Every type in the current app domain, which implements the given type.</returns>
    public static IEnumerable<Type> GetImplementedTypes(this Type This) {
      Contract.Requires(This != null);
      return (
        AppDomain
          .CurrentDomain
          .GetAssemblies()
          .Where(a => a != null)
          .SelectMany(a => a.GetTypes())
          .Where(t => t != null)
          .Distinct()
          .Where(This.IsAssignableFrom)
      );
    }
  }
}