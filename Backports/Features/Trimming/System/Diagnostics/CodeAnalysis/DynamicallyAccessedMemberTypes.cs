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

#if !SUPPORTS_DYNAMICALLY_ACCESSED_MEMBERS_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies the types of members that are dynamically accessed.
/// This enumeration is used by the <see cref="DynamicallyAccessedMembersAttribute"/>.
/// </summary>
[Flags]
public enum DynamicallyAccessedMemberTypes {
  /// <summary>
  /// Specifies no members.
  /// </summary>
  None = 0,

  /// <summary>
  /// Specifies the default, parameterless public constructor.
  /// </summary>
  PublicParameterlessConstructor = 1,

  /// <summary>
  /// Specifies all public constructors.
  /// </summary>
  PublicConstructors = 3,

  /// <summary>
  /// Specifies all non-public constructors.
  /// </summary>
  NonPublicConstructors = 4,

  /// <summary>
  /// Specifies all public methods.
  /// </summary>
  PublicMethods = 8,

  /// <summary>
  /// Specifies all non-public methods.
  /// </summary>
  NonPublicMethods = 16,

  /// <summary>
  /// Specifies all public fields.
  /// </summary>
  PublicFields = 32,

  /// <summary>
  /// Specifies all non-public fields.
  /// </summary>
  NonPublicFields = 64,

  /// <summary>
  /// Specifies all public nested types.
  /// </summary>
  PublicNestedTypes = 128,

  /// <summary>
  /// Specifies all non-public nested types.
  /// </summary>
  NonPublicNestedTypes = 256,

  /// <summary>
  /// Specifies all public properties.
  /// </summary>
  PublicProperties = 512,

  /// <summary>
  /// Specifies all non-public properties.
  /// </summary>
  NonPublicProperties = 1024,

  /// <summary>
  /// Specifies all public events.
  /// </summary>
  PublicEvents = 2048,

  /// <summary>
  /// Specifies all non-public events.
  /// </summary>
  NonPublicEvents = 4096,

  /// <summary>
  /// Specifies all interfaces implemented by the type.
  /// </summary>
  Interfaces = 8192,

  /// <summary>
  /// Specifies all members.
  /// </summary>
  All = -1
}

#endif
