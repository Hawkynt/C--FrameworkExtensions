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

// NullabilityInfoContext was added in .NET 6.0
#if !SUPPORTS_NULLABILITY_INFO

namespace System.Reflection;

/// <summary>
/// Provides APIs for populating nullability information and context from reflection members.
/// </summary>
public sealed class NullabilityInfoContext {

  private const string _NULLABLE_ATTRIBUTE = "System.Runtime.CompilerServices.NullableAttribute";
  private const string _NULLABLE_CONTEXT_ATTRIBUTE = "System.Runtime.CompilerServices.NullableContextAttribute";

  /// <summary>
  /// Populates a <see cref="NullabilityInfo"/> for the given <see cref="ParameterInfo"/>.
  /// </summary>
  /// <param name="parameterInfo">The parameter for which to populate the nullability info.</param>
  /// <returns>The nullability info for the parameter.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="parameterInfo"/> is <see langword="null"/>.</exception>
  public NullabilityInfo Create(ParameterInfo parameterInfo) {
    if (parameterInfo == null)
      throw new ArgumentNullException(nameof(parameterInfo));

    var declaringMember = parameterInfo.Member;
    var type = parameterInfo.ParameterType;
    var nullableContext = _GetNullableContext(declaringMember);
    var nullableAttribute = _GetNullableAttribute(parameterInfo);

    return _CreateNullabilityInfo(type, nullableAttribute, nullableContext, 0);
  }

  /// <summary>
  /// Populates a <see cref="NullabilityInfo"/> for the given <see cref="PropertyInfo"/>.
  /// </summary>
  /// <param name="propertyInfo">The property for which to populate the nullability info.</param>
  /// <returns>The nullability info for the property.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
  public NullabilityInfo Create(PropertyInfo propertyInfo) {
    if (propertyInfo == null)
      throw new ArgumentNullException(nameof(propertyInfo));

    var type = propertyInfo.PropertyType;
    var nullableContext = _GetNullableContext(propertyInfo);
    var nullableAttribute = _GetNullableAttribute(propertyInfo);

    var info = _CreateNullabilityInfo(type, nullableAttribute, nullableContext, 0);

    // Adjust read/write states based on property accessibility
    var getMethod = propertyInfo.GetGetMethod(true);
    var setMethod = propertyInfo.GetSetMethod(true);

    if (getMethod == null)
      info.ReadState = NullabilityState.Unknown;
    if (setMethod == null)
      info.WriteState = NullabilityState.Unknown;

    return info;
  }

  /// <summary>
  /// Populates a <see cref="NullabilityInfo"/> for the given <see cref="EventInfo"/>.
  /// </summary>
  /// <param name="eventInfo">The event for which to populate the nullability info.</param>
  /// <returns>The nullability info for the event.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="eventInfo"/> is <see langword="null"/>.</exception>
  public NullabilityInfo Create(EventInfo eventInfo) {
    if (eventInfo == null)
      throw new ArgumentNullException(nameof(eventInfo));

    var type = eventInfo.EventHandlerType!;
    var nullableContext = _GetNullableContext(eventInfo);
    var nullableAttribute = _GetNullableAttribute(eventInfo);

    return _CreateNullabilityInfo(type, nullableAttribute, nullableContext, 0);
  }

  /// <summary>
  /// Populates a <see cref="NullabilityInfo"/> for the given <see cref="FieldInfo"/>.
  /// </summary>
  /// <param name="fieldInfo">The field for which to populate the nullability info.</param>
  /// <returns>The nullability info for the field.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
  public NullabilityInfo Create(FieldInfo fieldInfo) {
    if (fieldInfo == null)
      throw new ArgumentNullException(nameof(fieldInfo));

    var type = fieldInfo.FieldType;
    var nullableContext = _GetNullableContext(fieldInfo);
    var nullableAttribute = _GetNullableAttribute(fieldInfo);

    return _CreateNullabilityInfo(type, nullableAttribute, nullableContext, 0);
  }

  private static NullabilityInfo _CreateNullabilityInfo(Type type, byte[]? nullableFlags, byte contextFlag, int flagIndex) {
    NullabilityInfo? elementType = null;
    var genericTypeArguments = Array.Empty<NullabilityInfo>();

    // Handle Nullable<T>
    var underlyingType = Nullable.GetUnderlyingType(type);
    if (underlyingType != null) {
      // T? where T : struct - always nullable
      return new(type, NullabilityState.Nullable, NullabilityState.Nullable, null, Array.Empty<NullabilityInfo>());
    }

    // Handle arrays
    if (type.IsArray) {
      var elementTypeInfo = type.GetElementType()!;
      var nextIndex = flagIndex + 1;
      elementType = _CreateNullabilityInfo(elementTypeInfo, nullableFlags, contextFlag, nextIndex);
    }

    // Handle generic types
    if (type is { IsGenericType: true, IsGenericTypeDefinition: false }) {
      var genericArgs = type.GetGenericArguments();
      genericTypeArguments = new NullabilityInfo[genericArgs.Length];
      var nextIndex = flagIndex + 1;

      for (var i = 0; i < genericArgs.Length; ++i) {
        genericTypeArguments[i] = _CreateNullabilityInfo(genericArgs[i], nullableFlags, contextFlag, nextIndex);
        nextIndex += _CountTypeReferences(genericArgs[i]);
      }
    }

    // Determine nullability state
    var state = _GetNullabilityState(type, nullableFlags, contextFlag, flagIndex);

    return new(type, state, state, elementType, genericTypeArguments);
  }

  private static NullabilityState _GetNullabilityState(Type type, byte[]? nullableFlags, byte contextFlag, int flagIndex) {
    // Value types (excluding Nullable<T>) are never null
    if (type.IsValueType)
      return NullabilityState.NotNull;

    // Check the nullable attribute flags
    if (nullableFlags != null && flagIndex < nullableFlags.Length)
      return _ByteToNullabilityState(nullableFlags[flagIndex]);

    // Use the context flag if available
    if (nullableFlags is { Length: 1 })
      return _ByteToNullabilityState(nullableFlags[0]);

    // Fall back to context
    return _ByteToNullabilityState(contextFlag);
  }

  private static NullabilityState _ByteToNullabilityState(byte value) => value switch {
    1 => NullabilityState.NotNull,
    2 => NullabilityState.Nullable,
    _ => NullabilityState.Unknown
  };

  private static int _CountTypeReferences(Type type) {
    var count = 1;

    if (type.IsArray && type.GetElementType() is { } elementType)
      count += _CountTypeReferences(elementType);

    if (type is not { IsGenericType: true, IsGenericTypeDefinition: false })
      return count;

    foreach (var arg in type.GetGenericArguments())
      count += _CountTypeReferences(arg);

    return count;
  }

  private static byte[]? _GetNullableAttribute(ICustomAttributeProvider member) {
    foreach (var attr in member.GetCustomAttributes(false))
      if (attr.GetType().FullName == _NULLABLE_ATTRIBUTE)
        return _ExtractNullableFlags(attr);

    return null;
  }

  private static byte _GetNullableContext(MemberInfo member) {
    // Check the member itself
    foreach (var attr in member.GetCustomAttributes(false))
      if (attr.GetType().FullName == _NULLABLE_CONTEXT_ATTRIBUTE)
        return _ExtractContextFlag(attr);

    // Check the declaring type
    var declaringType = member.DeclaringType;
    while (declaringType != null) {
      foreach (var attr in declaringType.GetCustomAttributes(false))
        if (attr.GetType().FullName == _NULLABLE_CONTEXT_ATTRIBUTE)
          return _ExtractContextFlag(attr);

      declaringType = declaringType.DeclaringType;
    }

    // Check the module/assembly
    if (member is Type typeInfo)
      return _GetModuleNullableContext(typeInfo.Module);

    return member.DeclaringType != null ? _GetModuleNullableContext(member.DeclaringType.Module) : (byte)0;
  }

  private static byte _GetModuleNullableContext(Module module) {
    foreach (var attr in module.GetCustomAttributes(false))
      if (attr.GetType().FullName == _NULLABLE_CONTEXT_ATTRIBUTE)
        return _ExtractContextFlag(attr);

    return 0;
  }

  private static byte[] _ExtractNullableFlags(object attribute) {
    var type = attribute.GetType();

    // Try to get NullableFlags property (byte[])
    var flagsProperty = type.GetProperty("NullableFlags");
    if (flagsProperty?.GetValue(attribute) is byte[] flags)
      return flags;

    // Fall back to Flag field (byte)
    var flagField = type.GetField("NullableFlags");
    if (flagField?.GetValue(attribute) is byte[] fieldFlags)
      return fieldFlags;

    // Single byte value
    var singleField = type.GetField("Flag");
    if (singleField?.GetValue(attribute) is byte singleFlag)
      return [singleFlag];

    return [0];
  }

  private static byte _ExtractContextFlag(object attribute) {
    var type = attribute.GetType();

    // Get Flag property or field
    var flagProperty = type.GetProperty("Flag");
    if (flagProperty?.GetValue(attribute) is byte propFlag)
      return propFlag;

    var flagField = type.GetField("Flag");
    if (flagField?.GetValue(attribute) is byte fieldFlag)
      return fieldFlag;

    return 0;
  }

}

#endif
