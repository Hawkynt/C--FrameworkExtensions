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

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32;
using System.Diagnostics;
#if NETFX_4
using System.Diagnostics.Contracts;
using System.Collections.Concurrent;
#endif
using System.Linq;
using System.Reflection;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class TypeExtensions {
    #region nested types
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
      public string Name => this.__name ?? (this.__name = this.Info?.Name);

      /// <summary>
      /// Gets a value indicating whether this instance is readable.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is readable; otherwise, <c>false</c>.
      /// </value>
      public bool IsReadable => this.Info != null && this.Info.CanRead;

      /// <summary>
      /// Gets a value indicating whether this instance is writable.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is writable; otherwise, <c>false</c>.
      /// </value>
      public bool IsWritable => this.Info != null && this.Info.CanWrite && !this.ReadOnly;

      /// <summary>
      /// Cache
      /// </summary>
      private string __description;
      /// <summary>
      /// Gets the descriptions.
      /// </summary>
      public string Descriptions => this.__description ?? (this.__description = this._customAttributes.OfType<DescriptionAttribute>().Where(i => i != null).Select(i => i.Description).FirstOrDefault());

      /// <summary>
      /// Cache
      /// </summary>
      private string __displayName;
      /// <summary>
      /// Gets the descriptions.
      /// </summary>
      public string DisplayName => this.__displayName ?? (this.__displayName = this._customAttributes.OfType<DisplayNameAttribute>().Where(i => i != null).Select(i => i.DisplayName).FirstOrDefault());

      /// <summary>
      /// Cache
      /// </summary>
      private string __category;
      /// <summary>
      /// Gets the categories.
      /// </summary>
      public string Category => this.__category ?? (this.__category = this._customAttributes.OfType<CategoryAttribute>().Where(i => i != null).Select(i => i.Category).FirstOrDefault());

      /// <summary>
      /// Cache
      /// </summary>
      private bool? __readOnly;
      /// <summary>
      /// Gets the categories.
      /// </summary>
      public bool ReadOnly => (bool)(this.__readOnly ?? (this.__readOnly = this._customAttributes.OfType<ReadOnlyAttribute>().Where(i => i != null).Any(i => i.IsReadOnly)));

      /// <summary>
      /// Cache
      /// </summary>
      private IEnumerable<EditorBrowsableState> __editorBrowseableStates;
      /// <summary>
      /// Gets the editor browseable states.
      /// </summary>
      public IEnumerable<EditorBrowsableState> EditorBrowseableStates => this.__editorBrowseableStates ?? (this.__editorBrowseableStates = this._customAttributes.OfType<EditorBrowsableAttribute>().Where(i => i != null).Select(i => i.State));

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
      public Type PropertyType => this.__propertyType ?? (this.__propertyType = this.Info?.PropertyType ?? TypeObject);

      #region value getter
      public object GetValue(object instance = null) {
        var propertyInfo = this.Info;
#if NETFX_4
        Contract.Assume(propertyInfo != null);
#endif
        return (propertyInfo.GetValue(instance, null));
      }
      public TValue GetValue<TValue>(object instance = null) => (TValue)this.GetValue(instance);

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
#if NETFX_4
        Contract.Assume(propertyInfo != null);
#endif
        propertyInfo.SetValue(null, value, null);
      }
      public void SetValue(object instance, object value) {
        var propertyInfo = this.Info;
#if NETFX_4
        Contract.Assume(propertyInfo != null);
#endif
        propertyInfo.SetValue(instance, value, null);
      }
      public bool TrySetValue(object value) {
        var propertyInfo = this.Info;
#if NETFX_4
        Contract.Assume(propertyInfo != null);
#endif
        try {
          propertyInfo.SetValue(null, value, null);
          return (true);
        } catch (Exception e) {
          Trace.WriteLine($"Could not set static property {this.Name}: {e}");
          return (false);
        }
      }
      public bool TrySetValue(object instance, object value) {
        var propertyInfo = this.Info;
#if NETFX_4
        Contract.Assume(propertyInfo != null);
#endif
        try {
          propertyInfo.SetValue(instance, value, null);
          return (true);
        } catch (Exception e) {
          Trace.WriteLine($"Could not set instance property {this.Name}: {e}");
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
      private object[] _customAttributes => this.__customAttributes ?? (this.__customAttributes = this.Info?.GetCustomAttributes(true) ?? new object[0]);

      public PropertyDesignerDetails(PropertyInfo info)
        : this() {
#if NETFX_4
        Contract.Requires(info != null);
#endif
        this.Info = info;
      }
    }

    /// <summary>
    /// Holds parameters and their type information for ctor calls.
    /// </summary>
    private class CtorParameter {
      public CtorParameter(Type type, object value) {
        this.Type = type;
        this.Value = value;
      }

      public Type Type { get; }
      public object Value { get; }
    }
    #endregion

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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(target != null);

#else
      Debug.Assert(This != null);
      Debug.Assert(target != null);
#endif
      // check inheritance
      if (target.IsAssignableFrom(This))
        return true;

      // check cache
      if (_IMPLICIT_CONVERSIONS.ContainsKey(target)) {
#if NETFX_4
        Contract.Assume(_IMPLICIT_CONVERSIONS[target] != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(source != null);

#else
      Debug.Assert(This != null);
      Debug.Assert(source != null);
#endif
      return (source.IsCastableTo(This));
    }
    #endregion

    #region messing for designers

    /// <summary>
    /// Cache
    /// </summary>
#if NETFX_4
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>> _typeCache = new ConcurrentDictionary<Type, ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>>();
#endif

    /// <summary>
    /// Gets the designer detailed properties from a given type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <param name="bindingFlags">The bindingflags to use, defaults to Public and Instance.</param>
    /// <returns>An array of PropertyDesignerDetails.</returns>
    public static PropertyDesignerDetails[] GetDesignerProperties(this Type This, BindingFlags? bindingFlags = null) {
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
#endif

      // ReSharper disable once JoinDeclarationAndInitializer
      PropertyDesignerDetails[] result;

#if NETFX_4
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

#if NETFX_4
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
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
#endif
      return ((TAttribute)This.Assembly.GetCustomAttributes(typeof(TAttribute), inherit)[index]);
    }

    /// <summary>
    /// Simples the name.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns></returns>
    public static string SimpleName(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
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
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
#endif
      return (This == TypeByte) || (This == TypeSByte) || (This == TypeShort) || (This == TypeWord) || (This == TypeInt) || (This == TypeDWord) || (This == TypeLong) || (This == TypeQWord);
    }

    /// <summary>
    /// Gets the minimum value of an int type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The minium value supported for this type.</returns>
    public static decimal GetMinValueForIntType(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
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
#if NETFX_4
      Contract.Requires(This != null);
#else
      Debug.Assert(This != null);
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
    public static bool IsSigned(this Type This) => (This == TypeSByte) || (This == TypeShort) || (This == TypeInt) || (This == TypeLong) || (This == TypeFloat) || (This == TypeDouble) || (This == TypeDecimal);

    /// <summary>
    /// Determines whether the specified this is unsigned.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <returns>
    ///   <c>true</c> if the specified this is unsigned; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUnsigned(this Type This) => !IsSigned(This);

    /// <summary>
    /// Determines whether the specified type is a float type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsFloatType(this Type This) => (This == TypeFloat) || (This == TypeDouble) || (This == TypeDecimal);

    /// <summary>
    /// Determines whether the specified type is a float type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDecimalType(this Type This) => (This == TypeDecimal);

    /// <summary>
    /// Determines whether the specified type is a string.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a string type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsStringType(this Type This) => (This == TypeString);

    /// <summary>
    /// Determines whether the specified type is a boolean type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a boolean type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBooleanType(this Type This) => (This == TypeBool);

    /// <summary>
    /// Determines whether the specified type is a TimeSpan type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a TimeSpan type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsTimeSpanType(this Type This) => (This == TypeTimeSpan);

    /// <summary>
    /// Determines whether the specified type is a DateTime type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is a DateTime type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDateTimeType(this Type This) => (This == TypeDateTime);

    /// <summary>
    /// Determines whether the specified type is an enum.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>
    ///   <c>true</c> if the given type is an enum; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEnumType(this Type This) => (This.IsEnum);

    /// <summary>
    /// Determines whether the specified type is nullable.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns><c>true</c> if it is Nullable; otherwise, <c>false</c>.</returns>
    public static bool IsNullable(this Type This) => This.IsGenericType && This.GetGenericTypeDefinition() == typeof(Nullable<>);

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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(fieldName != null);
      Contract.Requires(getter != null);
#else
      Debug.Assert(This != null);
      Debug.Assert(fieldName != null);
      Debug.Assert(getter != null);
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
      return This.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
    }

    /// <summary>
    /// Gets the description name.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The value of the description attribute if any.</returns>
    public static string GetDescription(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      return This.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
    }

    /// <summary>
    /// Gets the short name of the type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The part behind the last dot (.).</returns>
    public static string GetShortTypeName(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var fullName = This.FullName;
#if NETFX_4
      if (string.IsNullOrWhiteSpace(fullName))
#else
      if (fullName == null || fullName.Trim().Length < 1)
#endif
        return (fullName);

      return (fullName.Split('.').LastOrDefault());
    }

    /// <summary>
    /// Gets the implemented types of the given interface/base type.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>Every type in the current app domain, which implements the given type.</returns>
    public static IEnumerable<Type> GetImplementedTypes(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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

    /// <summary>
    /// Creates the instance.
    /// </summary>
    /// <typeparam name="TType">The type of the instance.</typeparam>
    /// <param name="This">This Type.</param>
    /// <returns>An instance of the given type.</returns>
    public static TType CreateInstance<TType>(this Type This) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(typeof(TType).IsAssignableFrom(This));
#endif
      return (TType)Activator.CreateInstance(This);
    }

    /// <summary>
    /// Compares two arrays of types for complete equality.
    /// </summary>
    /// <param name="array1">The 1st array.</param>
    /// <param name="array2">The 2nd array.</param>
    /// <param name="allowImplicitConversion">if set to <c>true</c> [allow implicit conversion].</param>
    /// <returns>
    ///   <c>true</c> if both arrays are equal; otherwise, <c>false</c>.
    /// </returns>
    private static bool _TypeArrayEquals(Type[] array1, ParameterInfo[] array2, bool allowImplicitConversion = false) {

      // if only one of the arrays is null, return false
      if ((array1 == null || array2 == null) && !(array1 == null && array2 == null))
        return (false);

      // both arrays are null, return true
      if (array1 == null)
        return (true);

      // no array is null, compare
      if (array1.Length != array2.Length)
        return (false);

      // compare elements
      if (allowImplicitConversion)
        return (array1.Select((t, i) => new { t, i }).All(t => array2[t.i].ParameterType.IsAssignableFrom(t.t)));

      return (array1.Select((t, i) => new { t, i }).All(t => array2[t.i].ParameterType == t.t));
    }

    /// <summary>
    /// Creates an instance from a ctor matching the given parameter types.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <param name="type">The type.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>
    /// Anew types' instance.
    /// </returns>
    private static TType _FromConstructor<TType>(Type type, IEnumerable<CtorParameter> parameters) {
#if NETFX_4
      Contract.Requires(type != null && typeof(TType).IsAssignableFrom(type));
      Contract.Requires(parameters != null);
#endif
      var pars = parameters.ToArray();

      var typeOfParams = (
        from i in pars
        select i.Type
        ).ToArray();

      var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      var matchingCtor = (

        // try to find an exact matching ctor first
        from i in ctors
        let par = i.GetParameters()
        where par != null && par.Length == typeOfParams.Length && _TypeArrayEquals(typeOfParams, par)
        select i
      ).FirstOrDefault() ?? (

        // if none found, try to get a ctor that could be filled by implicit parameter conversions
        from i in ctors
        let par = i.GetParameters()
        where par != null && par.Length == typeOfParams.Length && _TypeArrayEquals(typeOfParams, par, true)
        select i
        ).FirstOrDefault();

      if (matchingCtor == null)
        throw new NotSupportedException("No matching ctor found");

      return (TType)matchingCtor.Invoke((from i in pars
                                         select i.Value).ToArray());

    }

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <returns>The instance of the given type.</returns>
    public static TType FromConstructor<TType, TParam0>(this Type type, TParam0 param0) => _FromConstructor<TType>(type, new[] {
      new CtorParameter(typeof (TParam0), param0)
    });

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <typeparam name="TParam1">The type of the 2nd parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <param name="param1">The 2nd parameter.</param>
    /// <returns>
    /// The instance of the given type.
    /// </returns>
    public static TType FromConstructor<TType, TParam0, TParam1>(this Type type, TParam0 param0, TParam1 param1) => _FromConstructor<TType>(type, new[] {
      new CtorParameter(typeof (TParam0), param0),
      new CtorParameter(typeof (TParam1), param1)
    });

    /// <summary>
    /// Creates an instance of the given type by calling the ctor with the given parameter type.
    /// </summary>
    /// <typeparam name="TType">The type to create.</typeparam>
    /// <typeparam name="TParam0">The type of the 1st parameter.</typeparam>
    /// <typeparam name="TParam1">The type of the 2nd parameter.</typeparam>
    /// <typeparam name="TParam2">The type of the 3rd parameter.</typeparam>
    /// <param name="param0">The 1st parameter.</param>
    /// <param name="param1">The 2nd parameter.</param>
    /// <param name="param2">The 3rd parameter.</param>
    /// <returns>
    /// The instance of the given type.
    /// </returns>
    public static TType FromConstructor<TType, TParam0, TParam1, TParam2>(this Type type, TParam0 param0, TParam1 param1, TParam2 param2) => _FromConstructor<TType>(type, new[] {
      new CtorParameter(typeof (TParam0), param0),
      new CtorParameter(typeof (TParam1), param1),
      new CtorParameter(typeof (TParam2), param2)
    });

    /// <summary>
    /// Returns the file location for the given type or COM object.
    /// </summary>
    /// <param name="This">This Type.</param>
    /// <returns>The path to the executable or type library or <c>null</c>.</returns>
    public static string FileLocation(this Type This) {
      if (This == null)
        return (null);

      if (!This.IsCOMObject)
        return (This.Assembly.Location);

      var guid = This.GUID;

      var key = $@"HKEY_CLASSES_ROOT\CLSID\{{{guid}}}\InprocServer32";
      var result = (string)Registry.GetValue(key, null, null);
      if (result != null)
        return (result);

      key = $@"HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{{{guid}}}\LocalServer32";
      result = (string)Registry.GetValue(key, null, null);
      return (result);
    }

    /// <summary>
    /// Gets the static property value.
    /// </summary>
    /// <typeparam name="TType">The type of the property.</typeparam>
    /// <param name="This">This Type.</param>
    /// <param name="name">The name.</param>
    /// <returns>The value of the static property</returns>
    /// <exception cref="System.ArgumentException">Property not found;name</exception>
    public static TType GetStaticPropertyValue<TType>(this Type This, string name) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var prop = This.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty);
      if (prop == null) throw new ArgumentException("Property not found", nameof(name));
      return ((TType)prop.GetValue(null, null));
    }

    /// <summary>
    /// Gets the static field value.
    /// </summary>
    /// <typeparam name="TType">The type of the field.</typeparam>
    /// <param name="This">This field.</param>
    /// <param name="name">The name.</param>
    /// <returns>The value of the static field</returns>
    /// <exception cref="System.ArgumentException">Property not found;name</exception>
    public static TType GetStaticFieldValue<TType>(this Type This, string name) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var prop = This.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetField);
      if (prop == null) throw new ArgumentException("Property not found", nameof(name));
      return ((TType)prop.GetValue(null));
    }
  }
}