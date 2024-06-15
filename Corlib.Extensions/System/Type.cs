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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Guard;
#if NETFRAMEWORK
using Microsoft.Win32;
#endif
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TypeExtensions {
  #region nested types

  /// <summary>
  ///   Holds information about properties which are important for designer components.
  /// </summary>
  public struct PropertyDesignerDetails {
    /// <summary>
    ///   The underlying PropertyInfo
    /// </summary>
    public readonly PropertyInfo Info;

    /// <summary>
    ///   Cache
    /// </summary>
    private string __name;

    /// <summary>
    ///   Gets the name.
    /// </summary>
    public string Name => this.__name ??= this.Info?.Name;

    /// <summary>
    ///   Gets a value indicating whether this instance is readable.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is readable; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadable => this.Info is { CanRead: true };

    /// <summary>
    ///   Gets a value indicating whether this instance is writable.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is writable; otherwise, <c>false</c>.
    /// </value>
    public bool IsWritable => this.Info is { CanWrite: true } && !this.ReadOnly;

    private TResult _GetCachedFieldOrAttributeValue<TResult, TValue, TAttribute>(ref TResult backingField, Func<TAttribute, TValue> getter, Func<IEnumerable<TValue>, TResult> transformer)
      => backingField ??= transformer(this._CustomAttributes.OfType<TAttribute>().Where(i => i != null).Select(getter));

    /// <summary>
    ///   Gets the descriptions.
    /// </summary>
    public string[] Descriptions => this._GetCachedFieldOrAttributeValue(ref this.__description, (DescriptionAttribute i) => i.Description, i => i.ToArray());

    private string[] __description;

    /// <summary>
    ///   Gets the descriptions.
    /// </summary>
    public string DisplayName => this._GetCachedFieldOrAttributeValue(ref this.__displayName, (DisplayNameAttribute i) => i.DisplayName, i => i.FirstOrDefault());

    private string __displayName;

    /// <summary>
    ///   Gets the categories.
    /// </summary>
    public string Category => this._GetCachedFieldOrAttributeValue(ref this.__category, (CategoryAttribute i) => i.Category, i => i.FirstOrDefault());

    private string __category;

    /// <summary>
    ///   Gets the categories.
    /// </summary>
    public bool ReadOnly => this._GetCachedFieldOrAttributeValue(ref this.__readOnly, (ReadOnlyAttribute i) => i.IsReadOnly, i => i.Contains(true)) ?? false;

    private bool? __readOnly;

    /// <summary>
    ///   Gets the categories.
    /// </summary>
    public bool Browsable => this._GetCachedFieldOrAttributeValue(ref this.__browsable, (BrowsableAttribute i) => i.Browsable, i => !i.Contains(false)) ?? true;

    private bool? __browsable;

    /// <summary>
    ///   Gets the editor browseable states.
    /// </summary>
    public IEnumerable<EditorBrowsableState> EditorBrowseableStates => this._GetCachedFieldOrAttributeValue(ref this.__editorBrowseableStates, (EditorBrowsableAttribute i) => i.State, i => i);

    private IEnumerable<EditorBrowsableState> __editorBrowseableStates;

    /// <summary>
    ///   Gets the type of the property.
    /// </summary>
    /// <value>
    ///   The type of the property.
    /// </value>
    public Type PropertyType => this.__propertyType ??= this.Info?.PropertyType ?? TypeObject;

    private Type __propertyType;

    #region value getter

    public readonly object GetValue(object instance) => this.Info.GetValue(instance, null);
    public readonly TValue GetValue<TValue>(object instance) => (TValue)this.GetValue(instance);

    public readonly object GetValueOrDefault(object instance, object defaultValue)
      => this.TryGetValue(instance, out var result) ? result : defaultValue;

    public readonly TValue GetValueOrDefault<TValue>(object instance)
      => this.TryGetValue<TValue>(instance, out var result) ? result : default;

    public readonly TValue GetValueOrDefault<TValue>(object instance, TValue defaultValue)
      => this.TryGetValue<TValue>(instance, out var result) ? result : defaultValue;

    public readonly object GetValueOrDefault(object defaultValue = null)
      => this.TryGetValue(out var result) ? result : defaultValue;

    public readonly TValue GetValueOrDefault<TValue>()
      => this.TryGetValue<TValue>(out var result) ? result : default;

    public readonly TValue GetValueOrDefault<TValue>(TValue defaultValue)
      => this.TryGetValue<TValue>(out var result) ? result : defaultValue;

    public readonly bool TryGetValue(out object value) {
      try {
        value = this.GetValue(null);
        return true;
      } catch {
        value = null;
        return false;
      }
    }

    public readonly bool TryGetValue(object instance, out object value) {
      try {
        value = this.GetValue(instance);
        return true;
      } catch {
        value = null;
        return false;
      }
    }

    public readonly bool TryGetValue<TValue>(out TValue value) {
      try {
        value = this.GetValue<TValue>(null);
        return true;
      } catch {
        value = default;
        return false;
      }
    }

    public readonly bool TryGetValue<TValue>(object instance, out TValue value) {
      try {
        value = this.GetValue<TValue>(instance);
        return true;
      } catch {
        value = default;
        return false;
      }
    }

    #endregion

    #region value setter

    public readonly void SetValue(object value) => this.Info.SetValue(null, value, null);

    public readonly void SetValue(object instance, object value) => this.Info.SetValue(instance, value, null);

    public bool TrySetValue(object value) {
      var propertyInfo = this.Info;
      try {
        propertyInfo.SetValue(null, value, null);
        return true;
      } catch (Exception e) {
        Trace.WriteLine($"Could not set static property {this.Name}: {e}");
        return false;
      }
    }

    public bool TrySetValue(object instance, object value) {
      var propertyInfo = this.Info;
      try {
        propertyInfo.SetValue(instance, value, null);
        return true;
      } catch (Exception e) {
        Trace.WriteLine($"Could not set instance property {this.Name}: {e}");
        return false;
      }
    }

    #endregion

    /// <summary>
    ///   Gets all custom attributes.
    /// </summary>
    private object[] _CustomAttributes {
      get {
        if (this.__customAttributes != null)
          return this.__customAttributes;

        var info = this.Info;
        if (info == null)
          return this.__customAttributes = Utilities.Array.Empty<object>();

        var name = info.Name;
        List<object> results = [];

        // walk inheritance chain
        var declaringType = info.DeclaringType;
        var type = declaringType;
        while (type != null) {
          var pInfo = type.GetProperty(name);
          if (pInfo != null)
            foreach (var attribute in pInfo.GetCustomAttributes(true))
              if (!results.Contains(attribute))
                results.Add(attribute);

          type = type.BaseType;
        }

        // walk interfaces
        if (declaringType != null)
          foreach (var @interface in declaringType.GetInterfaces()) {
            var property = @interface.GetProperty(name);
            if (property == null)
              continue;

            foreach (var attribute in property.GetCustomAttributes(true))
              if (!results.Contains(attribute))
                results.Add(attribute);
          }

        return this.__customAttributes = results.ToArray();
      }
    }

    private object[] __customAttributes;

    public PropertyDesignerDetails(PropertyInfo info) : this() {
      Against.ArgumentIsNull(info);

      this.Info = info;
    }
  }

  /// <summary>
  ///   Holds parameters and their type information for ctor calls.
  /// </summary>
  private readonly struct TypeWithValue(Type type, object value) {
    public readonly Type Type = type;
    public readonly object Value = value;
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

  private static readonly Dictionary<Type, Type[]> _IMPLICIT_CONVERSIONS = new() {
    {
      TypeDecimal, [
        TypeSByte,
        TypeByte,
        TypeShort,
        TypeWord,
        TypeInt,
        TypeDWord,
        TypeLong,
        TypeQWord,
        TypeChar
      ]
    }, {
      TypeDouble, [
        TypeSByte,
        TypeByte,
        TypeShort,
        TypeWord,
        TypeInt,
        TypeDWord,
        TypeLong,
        TypeQWord,
        TypeChar,
        TypeFloat
      ]
    }, {
      TypeFloat, [
        TypeSByte,
        TypeByte,
        TypeShort,
        TypeWord,
        TypeInt,
        TypeDWord,
        TypeLong,
        TypeQWord,
        TypeChar
      ]
    },
    { TypeQWord, [TypeByte, TypeWord, TypeDWord, TypeChar] },
    { TypeLong, [TypeSByte, TypeByte, TypeShort, TypeWord, TypeInt, TypeDWord, TypeChar] },
    { TypeDWord, [TypeByte, TypeWord, TypeChar] },
    { TypeInt, [TypeSByte, TypeByte, TypeShort, TypeWord, TypeChar] },
    { TypeWord, [TypeByte, TypeChar] },
    { TypeShort, [TypeByte] }
  };

  /// <summary>
  ///   Determines whether a given type can be casted to another one.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <param name="target">Target type.</param>
  /// <returns>
  ///   <c>true</c> if the given type can be casted to the target; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsCastableTo(this Type @this, Type target) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(target);

    // check inheritance
    if (target.IsAssignableFrom(@this))
      return true;

    // check cache
    if (_IMPLICIT_CONVERSIONS.TryGetValue(target, out var value) && value.Contains(@this))
      return true;

    return
      @this
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Any(m => (m.ReturnType == target && m.Name == "op_Implicit") || m.Name == "op_Explicit");
  }

  /// <summary>
  ///   Determines whether the given type can be casted from the given source type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <param name="source">The source type.</param>
  /// <returns>
  ///   <c>true</c> if this Type can be casted from the source; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsCastableFrom(this Type @this, Type source) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(source);

    return source.IsCastableTo(@this);
  }

  #endregion

  #region messing for designers

  /// <summary>
  ///   Cache
  /// </summary>
  private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<BindingFlags, PropertyDesignerDetails[]>> _typeCache = new();

  /// <summary>
  ///   Gets the designer detailed properties from a given type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <param name="bindingFlags">The bindingflags to use, defaults to Public and Instance.</param>
  /// <returns>An array of PropertyDesignerDetails.</returns>
  public static PropertyDesignerDetails[] GetDesignerProperties(this Type @this, BindingFlags? bindingFlags = null) {
    Against.ThisIsNull(@this);


    // ReSharper disable once JoinDeclarationAndInitializer

    bindingFlags ??= BindingFlags.Instance | BindingFlags.Public;

    // try to get from cache first
    if (
      _typeCache.TryGetValue(@this, out var inner)
      && inner != null
      && inner.TryGetValue(bindingFlags.Value, out var result)
    )
      return result;

    // harvest properties
    var props = @this.GetProperties(bindingFlags.Value);
    result = Array.ConvertAll(props, i => new PropertyDesignerDetails(i));

    // try to add to cache
    inner = _typeCache.GetOrAdd(@this, i => new());
    inner.TryAdd(bindingFlags.Value, result);
    return result;
  }

  #endregion

  /// <summary>
  ///   Gets the assembly attribute.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This type.</param>
  /// <param name="inherit">if set to <c>true</c> inherited attributes would also be returned; otherwise, not.</param>
  /// <param name="index">The index to use if multiple attributes were found of that kind.</param>
  /// <returns>The given attribute instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TAttribute GetAssemblyAttribute<TAttribute>(this Type @this, bool inherit = false, int index = 0) {
    Against.ThisIsNull(@this);

    return (TAttribute)@this.Assembly.GetCustomAttributes(typeof(TAttribute), inherit)[index];
  }

  /// <summary>
  ///   Simples the name.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <param name="useLanguageTypes">Whether to use language type names like eg int for Int32; defaults to <c>false</c>.</param>
  /// <returns></returns>
  public static string SimpleName(this Type @this, bool useLanguageTypes = false) {
    Against.ThisIsNull(@this);

    var name = @this.FullName;
    if (name == null)
      return null;

    name = name.Trim();
    var index = name.LastIndexOf('.');
    return index < 0 ? name : name[(index + 1)..];
  }

  /// <summary>
  ///   Generates a random value of the specified type. For reference types, an instance can optionally be created.
  /// </summary>
  /// <param name="this">The type for which to generate a random value.</param>
  /// <param name="allowInstanceCreationForReferenceTypes">
  ///   If set to <see langword="true" />, allows the creation of a new
  ///   instance for reference types. Default is false.
  /// </param>
  /// <returns>
  ///   A random value of the specified type or a new instance of the type if it's a reference type and instance
  ///   creation is allowed.
  /// </returns>
  /// <exception cref="ArgumentException">Thrown if it's not possible to create a value of the specified type.</exception>
  /// <example>
  ///   <code>
  /// Type intType = typeof(int);
  /// object randomInt = intType.GetRandomValue();
  /// Console.WriteLine($"Random int: {randomInt}");
  /// 
  /// Type stringType = typeof(string);
  /// // Note that without instance creation, the default for reference types like string is null.
  /// object randomString = stringType.GetRandomValue(true);
  /// Console.WriteLine($"Random string: {randomString ?? "null"}");
  /// </code>
  ///   This example shows how to generate a random integer and an instance of a string. For the string, instance creation is
  ///   allowed.
  /// </example>
  /// <remarks>
  ///   This method can handle primitive types, structs, and reference types. For reference types, a new instance is only
  ///   created if <paramref name="allowInstanceCreationForReferenceTypes" /> is true, otherwise, the default value (null) is
  ///   returned.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static object GetRandomValue(this Type @this, bool allowInstanceCreationForReferenceTypes = false) {
    Against.ThisIsNull(@this);

    return GetRandomValueFor(@this, allowInstanceCreationForReferenceTypes, Utilities.Random.Shared);
  }

  internal static object GetRandomValueFor(Type type, bool allowInstanceCreationForReferenceTypes, Random entropySource) {
    Against.ArgumentIsNull(type);
    Against.ArgumentIsNull(entropySource);

    if (TryCreateForWellKnownValueType(type, entropySource, out var result))
      return result;

    if (Nullable.GetUnderlyingType(type) is { } ul)
      return CreateForNullable(ul, allowInstanceCreationForReferenceTypes, entropySource);

    if (type.IsEnum)
      return CreateForEnum(type, entropySource);

    if (type.IsClass)
      return CreateForRefType(type, allowInstanceCreationForReferenceTypes, entropySource);

    if (type.IsValueType)
      return CreateForValueType(type, entropySource);

    throw new NotSupportedException("Unknown type");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static object CreateForNullable(Type underlyingType, bool allowInstanceCreationForReferenceTypes, Random entropySource)
      => entropySource.GetBoolean() ? null : GetRandomValueFor(underlyingType, allowInstanceCreationForReferenceTypes, entropySource)
    ;

    static object CreateForRefType(Type type, bool allowInstanceCreationForReferenceTypes, Random entropySource) {
      if (entropySource.GetBoolean())
        return null;

      if (!allowInstanceCreationForReferenceTypes) {
        Trace.WriteLine($"[Warning]Not allowed to create instance of type {type.FullName} by parameter, always returning <null>");
        return null;
      }

      var constructors = type.GetConstructors();
      if (constructors.Length <= 0) {
        Trace.WriteLine($"[Warning]No public constructors available for type {type.FullName}");
        return null;
      }

      // use any ctor randomly
      var ctor = constructors[entropySource.Next(constructors.Length)];
      var parameters = ctor.GetParameters().Select(p => GetRandomValueFor(p.ParameterType, true, entropySource)).ToArray();
      return Activator.CreateInstance(type, parameters);
    }

    static object CreateForValueType(Type type, Random entropySource) {
      var size = Marshal.SizeOf(type);
      var data = new byte[size];

      if (entropySource.GetBoolean())
        entropySource.NextBytes(data);

      var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
      try {
        var pointer = handle.AddrOfPinnedObject();
        return Marshal.PtrToStructure(pointer, type);
      } finally {
        handle.Free();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool TryCreateForWellKnownValueType(Type type, Random entropySource, out object result) => (__tryCreateRandomValueForWellKnownType ??= new()).Invoke(type, entropySource, out result);

    static object CreateForEnum(Type type, Random entropySource) {
      // Return any integer value in the range of the underlying enum type
      if (entropySource.GetBoolean()) {
        var underlyingType = Enum.GetUnderlyingType(type);
        return
          underlyingType == TypeInt || underlyingType == TypeDWord || underlyingType == TypeChar ? Enum.ToObject(type, entropySource.NextInt64(uint.MaxValue + 1L))
          : underlyingType == TypeLong || underlyingType == TypeQWord ? Enum.ToObject(type, entropySource.NextInt64())
          : underlyingType == TypeShort || underlyingType == TypeWord ? Enum.ToObject(type, entropySource.Next(ushort.MaxValue))
          : underlyingType == TypeByte || underlyingType == TypeSByte ? Enum.ToObject(type, entropySource.Next(byte.MaxValue))
          : Enum.ToObject(type, 0)
          ;
      }

      var isFlagged = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
      var enumValues = Enum.GetValues(type);

      // For a flagged enum, start with no flags and randomly combine them
      if (isFlagged) {
        var combinedValue = 0UL;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in enumValues)
          if (entropySource.GetBoolean())
            combinedValue |= Convert.ToUInt64(value);

        return Enum.ToObject(type, combinedValue);
      }

      // Include the possibility of generating values explicitly defined in the enum
      var randomIndex = entropySource.Next(enumValues.Length);
      return enumValues.GetValue(randomIndex);
    }
  }

  private static __TryCreateRandomValueForWellKnownType __tryCreateRandomValueForWellKnownType;

  private sealed class __TryCreateRandomValueForWellKnownType {
    private readonly Dictionary<Type, Func<Random, object>> _generators = new() {
      { TypeBool, r => r.GetBoolean() },
      { TypeChar, r => r.GetChar() },
      { TypeSByte, r => r.GetInt8() },
      { TypeByte, r => r.GetUInt8() },
      { TypeShort, r => r.GetInt16() },
      { TypeWord, r => r.GetUInt16() },
      { TypeInt, r => r.GetInt32() },
      { TypeDWord, r => r.GetUInt32() },
      { TypeLong, r => r.GetInt64() },
      { TypeQWord, r => r.GetUInt64() },
      { TypeFloat, r => r.GetFloat() },
      { TypeDouble, r => r.GetDouble() },
      { TypeDecimal, r => r.GetDecimal() },
      { TypeString, r => r.GetString(0, 65536, true) },
      { TypeObject, r => r.GetBoolean() ? null : new object() },
      { TypeTimeSpan, r => new TimeSpan(r.NextInt64()) },
      { TypeDateTime, r => new DateTime(r.NextInt64(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks)) },
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Invoke(Type type, Random entropySource, out object result) {
      if (this._generators.TryGetValue(type, out var generator)) {
        result = generator(entropySource);
        return true;
      }

      result = default;
      return false;
    }
  }

  /// <summary>
  ///   Retrieves the default value for the specified type.
  /// </summary>
  /// <param name="this">The <see cref="Type" /> for which to retrieve the default value.</param>
  /// <returns>
  ///   The default value for the type specified. This returns <see langword="null" /> for reference types and zero or
  ///   equivalent for value types.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <example>
  ///   <code>
  /// Type intType = typeof(int);
  /// object defaultInt = intType.GetDefaultValue();
  /// Console.WriteLine($"Default value for int: {defaultInt}"); // Outputs "Default value for int: 0"
  /// 
  /// Type stringType = typeof(string);
  /// object defaultString = stringType.GetDefaultValue();
  /// Console.WriteLine($"Default value for string: {defaultString}"); // Outputs "Default value for string: null"
  /// </code>
  ///   This example demonstrates retrieving the default values for an integer and a string.
  /// </example>
  public static object GetDefaultValue(this Type @this) {
    Against.ThisIsNull(@this);

    if (@this.IsNullable())
      return null;
    if (@this.IsClass)
      return null;
    if (@this.IsEnum)
      return Enum.ToObject(@this, (byte)0);
    if (@this == TypeBool)
      return false;
    if (@this == TypeChar)
      return '\0';
    if (@this == TypeSByte)
      return (sbyte)0;
    if (@this == TypeByte)
      return (byte)0;
    if (@this == TypeShort)
      return (short)0;
    if (@this == TypeWord)
      return (ushort)0;
    if (@this == TypeInt)
      return 0;
    if (@this == TypeDWord)
      return 0U;
    if (@this == TypeLong)
      return 0L;
    if (@this == TypeQWord)
      return 0UL;
    if (@this == TypeFloat)
      return 0f;
    if (@this == TypeDouble)
      return 0d;
    if (@this == TypeDecimal)
      return 0m;
    if (@this.IsValueType)
      return Activator.CreateInstance(@this);
    if (@this.IsArray)
      return null;

    throw new NotSupportedException($"Unknown type {@this}");
  }

  /// <summary>
  ///   Determines whether the specified type is an integer type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the specified type is integer; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsIntegerType(this Type @this) =>
    @this == TypeByte
    || @this == TypeSByte
    || @this == TypeShort
    || @this == TypeWord
    || @this == TypeInt
    || @this == TypeDWord
    || @this == TypeLong
    || @this == TypeQWord;

  /// <summary>
  ///   Gets the minimum value of an int type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>The minimum value supported for this type.</returns>
  public static decimal GetMinValueForIntType(this Type @this) {
    Against.ThisIsNull(@this);

    if (@this == TypeByte)
      return byte.MinValue;
    if (@this == TypeSByte)
      return sbyte.MinValue;
    if (@this == TypeWord)
      return ushort.MinValue;
    if (@this == TypeShort)
      return short.MinValue;
    if (@this == TypeDWord)
      return uint.MinValue;
    if (@this == TypeInt)
      return int.MinValue;
    if (@this == TypeQWord)
      return ulong.MinValue;
    if (@this == TypeLong)
      return long.MinValue;

    throw new NotSupportedException();
  }

  /// <summary>
  ///   Gets the maximum value of an int type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>The maximum value supported for this type.</returns>
  public static decimal GetMaxValueForIntType(this Type @this) {
    Against.ThisIsNull(@this);

    if (@this == TypeByte)
      return byte.MaxValue;
    if (@this == TypeSByte)
      return sbyte.MaxValue;
    if (@this == TypeWord)
      return ushort.MaxValue;
    if (@this == TypeShort)
      return short.MaxValue;
    if (@this == TypeDWord)
      return uint.MaxValue;
    if (@this == TypeInt)
      return int.MaxValue;
    if (@this == TypeQWord)
      return ulong.MaxValue;
    if (@this == TypeLong)
      return long.MaxValue;
    throw new NotSupportedException();
  }

  /// <summary>
  ///   Determines whether the specified this is signed.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns>
  ///   <c>true</c> if the specified this is signed; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsSigned(this Type @this)
    => @this == TypeSByte
       || @this == TypeShort
       || @this == TypeInt
       || @this == TypeLong
       || @this == TypeFloat
       || @this == TypeDouble
       || @this == TypeDecimal
       || (@this.IsNullable() && IsSigned(@this.GetGenericArguments()[0]));

  /// <summary>
  ///   Determines whether the specified this is unsigned.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns>
  ///   <c>true</c> if the specified this is unsigned; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsUnsigned(this Type @this) => !IsSigned(@this);

  /// <summary>
  ///   Determines whether the specified type is a float type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsFloatType(this Type @this) => @this == TypeFloat || @this == TypeDouble || @this == TypeDecimal;

  /// <summary>
  ///   Determines whether the specified type is a float type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a floating point type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsDecimalType(this Type @this) => @this == TypeDecimal;

  /// <summary>
  ///   Determines whether the specified type is a string.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a string type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsStringType(this Type @this) => @this == TypeString;

  /// <summary>
  ///   Determines whether the specified type is a boolean type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a boolean type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsBooleanType(this Type @this) => @this == TypeBool;

  /// <summary>
  ///   Determines whether the specified type is a TimeSpan type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a TimeSpan type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsTimeSpanType(this Type @this) => @this == TypeTimeSpan;

  /// <summary>
  ///   Determines whether the specified type is a DateTime type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is a DateTime type; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsDateTimeType(this Type @this) => @this == TypeDateTime;

  /// <summary>
  ///   Determines whether the specified type is an enum.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>
  ///   <c>true</c> if the given type is an enum; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsEnumType(this Type @this) => @this.IsEnum;

  /// <summary>
  ///   Determines whether the specified type is nullable.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns><c>true</c> if it is Nullable; otherwise, <c>false</c>.</returns>
  public static bool IsNullable(this Type @this) => @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);

  /// <summary>
  ///   Gets the attribute value.
  /// </summary>
  /// <typeparam name="TAttributeType">The type of the attribute type.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="fieldName">Name of the field.</param>
  /// <param name="getter">The getter.</param>
  /// <returns></returns>
  public static TValue GetFieldOrPropertyAttributeValue<TAttributeType, TValue>(this Type @this, string fieldName, Func<TAttributeType, TValue> getter) where TAttributeType : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fieldName);
    Against.ArgumentIsNull(getter);

    object[] attributes;
    var field = @this.GetField(fieldName);
    if (field != null)
      attributes = field.GetCustomAttributes(typeof(TAttributeType), true);
    else {
      var prop = @this.GetProperty(fieldName);
      if (prop == null)
        throw new ArgumentException();
      attributes = prop.GetCustomAttributes(typeof(TAttributeType), true);
    }

    var value = getter(attributes.OfType<TAttributeType>().First());
    return value;
  }

  /// <summary>
  ///   Gets the display name.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>The value of the displayname attribute if any</returns>
  public static string GetDisplayName(this Type @this) {
    Against.ThisIsNull(@this);

    return @this.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
  }

  /// <summary>
  ///   Gets the description name.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>The value of the description attribute if any.</returns>
  public static string GetDescription(this Type @this) {
    Against.ThisIsNull(@this);

    return @this.GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
  }

  /// <summary>
  ///   Gets the implemented types of the given interface/base type.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>Every type in the current app domain, which implements the given type.</returns>
  public static IEnumerable<Type> GetImplementedTypes(this Type @this) {
    Against.ThisIsNull(@this);

    return AppDomain
      .CurrentDomain
      .GetAssemblies()
      .Where(a => a != null)
      .SelectMany(a => a.GetTypes())
      .Where(t => t != null)
      .Distinct()
      .Where(@this.IsAssignableFrom);
  }

  private delegate object Constructor();

  private static readonly Type _ctorType = typeof(Constructor);
  private static readonly ConcurrentDictionary<Type, Constructor> _ctorCache = new();

  /// <summary>
  ///   Creates the instance.
  /// </summary>
  /// <typeparam name="TType">The type of the instance.</typeparam>
  /// <param name="this">This Type.</param>
  /// <returns>An instance of the given type.</returns>
  public static TType CreateInstance<TType>(this Type @this) {
    Against.ThisIsNull(@this);
    Against.False(typeof(TType).IsAssignableFrom(@this));

#if MONO
    return (TType)Activator.CreateInstance(@this);
#else
    return (TType)_ctorCache.GetOrAdd(@this, _ => _GetConstructorFactory(@this))();
#endif
  }

  /// <summary>
  ///   Creates a delegate to construct an object of the given type.
  /// </summary>
  /// <param name="this">The type of the instance.</param>
  /// <returns>
  ///   A delegate to be called.
  /// </returns>
  private static Constructor _GetConstructorFactory(this Type @this) {
    var parameterTypes = Type.EmptyTypes;
    var ctor = @this.GetConstructor(parameterTypes);
    if (ctor == null)
      return () => Activator.CreateInstance(@this);

    DynamicMethod method = new("Ctor_" + @this.Name, @this, parameterTypes, @this);
    var ilgen = method.GetILGenerator();
    ilgen.Emit(OpCodes.Newobj, ctor);
    ilgen.Emit(OpCodes.Ret);
    return (Constructor)method.CreateDelegate(_ctorType);
  }

  /// <summary>
  ///   Creates the instance.
  /// </summary>
  /// <typeparam name="TType">The type of the instance.</typeparam>
  /// <param name="this">This Type.</param>
  /// <param name="parameters">The parameters.</param>
  /// <returns>
  ///   An instance of the given type.
  /// </returns>
  public static TType CreateInstance<TType>(this Type @this, params object[] parameters) {
    Against.ThisIsNull(@this);
    Against.False(typeof(TType).IsAssignableFrom(@this));

    return (TType)Activator.CreateInstance(@this, parameters);
  }

  /// <summary>
  ///   Compares two arrays of types for complete equality.
  /// </summary>
  /// <param name="array1">The 1st array.</param>
  /// <param name="array2">The 2nd array.</param>
  /// <param name="allowImplicitConversion">if set to <c>true</c> [allow implicit conversion].</param>
  /// <returns>
  ///   <c>true</c> if both arrays are equal; otherwise, <c>false</c>.
  /// </returns>
  private static bool _TypeArrayEquals(TypeWithValue[] array1, ParameterInfo[] array2, bool allowImplicitConversion) {
    switch (array1) {
      // if only one of the arrays is null, return false
      case null when array2 != null:
      case not null when array2 == null:
        return false;

      // both arrays are null, return true
      case null: return true;
    }

    // no array is null, compare
    if (array1.Length != array2.Length)
      return false;

    // compare elements
    if (allowImplicitConversion) {
      for (var i = 0; i < array1.Length; ++i)
        if (!array2[i].ParameterType.IsAssignableFrom(array1[i].Type))
          return false;
    } else
      for (var i = 0; i < array1.Length; ++i)
        if (array1[i].Type != array2[i].ParameterType)
          return false;

    return true;
  }

  /// <summary>
  ///   Creates an instance from a ctor matching the given parameter types.
  /// </summary>
  /// <typeparam name="TType">The type to create.</typeparam>
  /// <param name="type">The type.</param>
  /// <param name="parameters">The parameters.</param>
  /// <returns>
  ///   Anew types' instance.
  /// </returns>
  private static TType _FromConstructor<TType>(Type type, params TypeWithValue[] parameters) {
    Against.ThisIsNull(type);
    Against.False(typeof(TType).IsAssignableFrom(type));

    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    var matchingCtor = (
                         // try to find an exact matching ctor first
                         from i in ctors
                         let par = i.GetParameters()
                         where _TypeArrayEquals(parameters, par, false)
                         select i
                       ).FirstOrDefault()
                       ?? (
                         // if none found, try to get a ctor that could be filled by implicit parameter conversions
                         from i in ctors
                         let par = i.GetParameters()
                         where _TypeArrayEquals(parameters, par, true)
                         select i
                       ).FirstOrDefault();

    return matchingCtor == null
        ? throw new NotSupportedException("No matching ctor found")
        : (TType)matchingCtor.Invoke(parameters.Select(i => i.Value).ToArray())
      ;
  }

#if NETFRAMEWORK

  /// <summary>
  ///   Returns the file location for the given type or COM object.
  /// </summary>
  /// <param name="this">This Type.</param>
  /// <returns>The path to the executable or type library or <c>null</c>.</returns>
  public static string FileLocation(this Type @this) {
    if (@this == null)
      return null;

    if (!@this.IsCOMObject)
      return @this.Assembly.Location;

    var guid = @this.GUID;

    var key = $@"HKEY_CLASSES_ROOT\CLSID\{{{guid}}}\InprocServer32";
    var result = (string)Registry.GetValue(key, null, null);
    if (result != null)
      return result;

    key = $@"HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{{{guid}}}\LocalServer32";
    result = (string)Registry.GetValue(key, null, null);
    return result;
  }

#endif

  /// <summary>
  ///   Gets the static property value.
  /// </summary>
  /// <typeparam name="TType">The type of the property.</typeparam>
  /// <param name="this">This Type.</param>
  /// <param name="name">The name.</param>
  /// <returns>The value of the static property</returns>
  /// <exception cref="System.ArgumentException">Property not found;name</exception>
  public static TType GetStaticPropertyValue<TType>(this Type @this, string name) {
    Against.ThisIsNull(@this);

    var prop = @this.GetProperty(
      name,
      BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty
    );
    return prop == null
        ? throw new ArgumentException("Property not found", nameof(name))
        : (TType)prop.GetValue(null, null)
      ;
  }

  /// <summary>
  ///   Gets the static field value.
  /// </summary>
  /// <typeparam name="TType">The type of the field.</typeparam>
  /// <param name="this">This Type.</param>
  /// <param name="name">The name.</param>
  /// <returns>The value of the static field</returns>
  /// <exception cref="System.ArgumentException">Property not found;name</exception>
  public static TType GetStaticFieldValue<TType>(this Type @this, string name) {
    Against.ThisIsNull(@this);

    var prop = @this.GetField(
      name,
      BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetField
    );
    return prop == null
        ? throw new ArgumentException("Field not found", nameof(name))
        : (TType)prop.GetValue(null)
      ;
  }

  public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type @this, bool inherit = false)
    where TAttribute : Attribute {
    Against.ThisIsNull(@this);

    return Invoke(@this, inherit);

    static IEnumerable<TAttribute> Invoke(Type @this, bool inherit) {
      if (!inherit)
        foreach (var result in @this.GetCustomAttributes(typeof(TAttribute), false))
          yield return (TAttribute)result;

      HashSet<TAttribute> alreadyReturned = [];
      var baseType = @this;
      do {
        foreach (var result in baseType.GetCustomAttributes(true)) {
          if (result is not TAttribute castedAttribute || !alreadyReturned.Add(castedAttribute))
            continue;

          yield return castedAttribute;
        }

        baseType = baseType.BaseType;
      } while (baseType != null);

      foreach (var @interface in @this.GetInterfaces())
      foreach (var result in GetAttributes<TAttribute>(@interface, true)) {
        if (!alreadyReturned.Add(result))
          continue;

        yield return result;
      }
    }
  }
}
