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

#if !SUPPORTS_RUNTIME_REFLECTION_EXTENSIONS

using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Reflection;

/// <summary>
/// Provides polyfills for RuntimeReflectionExtensions from .NET 4.5+.
/// </summary>
public static partial class RuntimeReflectionExtensionsPolyfills {

  extension(Delegate @this) {
    /// <summary>
    /// Gets the MethodInfo representing the method represented by the delegate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MethodInfo GetMethodInfo() {
      Against.ThisIsNull(@this);
      return @this.Method;
    }
  }

  extension(Type @this) {
    /// <summary>
    /// Returns the TypeInfo representation of the Type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypeInfo GetTypeInfo() {
      Against.ThisIsNull(@this);
      return new(@this);
    }
  }

}

/// <summary>
/// Provides a simple TypeInfo implementation for pre-.NET 4.5 frameworks.
/// </summary>
public class TypeInfo : ICustomAttributeProvider {
  private readonly Type _type;

  internal TypeInfo(Type type) => _type = type;

  public Type AsType() => _type;

  public bool IsValueType => _type.IsValueType;
  public bool IsGenericType => _type.IsGenericType;
  public bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;
  public bool IsInterface => _type.IsInterface;
  public bool IsAbstract => _type.IsAbstract;
  public bool IsSealed => _type.IsSealed;
  public bool IsClass => _type.IsClass;
  public bool IsEnum => _type.IsEnum;
  public bool IsArray => _type.IsArray;
  public bool IsPrimitive => _type.IsPrimitive;
  public bool IsPublic => _type.IsPublic;
  public bool IsNestedPublic => _type.IsNestedPublic;
  public bool IsVisible => _type.IsVisible;
  public bool IsNotPublic => _type.IsNotPublic;
  public string? Name => _type.Name;
  public string? Namespace => _type.Namespace;
  public string? FullName => _type.FullName;
  public Assembly Assembly => _type.Assembly;
  public Module Module => _type.Module;
  public Type? BaseType => _type.BaseType;
  public Type? DeclaringType => _type.DeclaringType;
  public MemberTypes MemberType => _type.MemberType;
  public TypeAttributes Attributes => _type.Attributes;
  public MethodInfo[] DeclaredMethods => _type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
  public PropertyInfo[] DeclaredProperties => _type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
  public FieldInfo[] DeclaredFields => _type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
  public EventInfo[] DeclaredEvents => _type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
  public ConstructorInfo[] DeclaredConstructors => _type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
  public Type[] ImplementedInterfaces => _type.GetInterfaces();
  public Type[] GenericTypeArguments => _type.IsGenericType ? _type.GetGenericArguments() : Type.EmptyTypes;

  public Type GetGenericTypeDefinition() => _type.GetGenericTypeDefinition();
  public Type[] GetGenericParameterConstraints() => _type.GetGenericParameterConstraints();
  public Type MakeGenericType(params Type[] typeArguments) => _type.MakeGenericType(typeArguments);
  public Type MakeArrayType() => _type.MakeArrayType();
  public Type MakeArrayType(int rank) => _type.MakeArrayType(rank);
  public Type MakeByRefType() => _type.MakeByRefType();
  public Type MakePointerType() => _type.MakePointerType();

  public bool IsAssignableFrom(TypeInfo? typeInfo) => typeInfo is not null && _type.IsAssignableFrom(typeInfo._type);
  public bool IsSubclassOf(Type c) => _type.IsSubclassOf(c);
  public bool IsInstanceOfType(object? o) => _type.IsInstanceOfType(o);

  // ICustomAttributeProvider implementation
  public object[] GetCustomAttributes(bool inherit) => _type.GetCustomAttributes(inherit);
  public object[] GetCustomAttributes(Type attributeType, bool inherit) => _type.GetCustomAttributes(attributeType, inherit);
  public bool IsDefined(Type attributeType, bool inherit) => _type.IsDefined(attributeType, inherit);
}

#endif
