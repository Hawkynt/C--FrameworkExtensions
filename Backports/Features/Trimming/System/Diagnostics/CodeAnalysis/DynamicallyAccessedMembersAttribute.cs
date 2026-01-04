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
/// Indicates that certain members on a specified <see cref="Type"/> are accessed dynamically,
/// for example through <see cref="System.Reflection"/>.
/// </summary>
/// <remarks>
/// This allows tools to understand which members are being accessed during execution
/// of a program. This attribute is valid on members whose type is <see cref="Type"/> or
/// <see cref="string"/>.
/// When this attribute is applied to a location of type <see cref="string"/>, the assumption
/// is that the string represents a fully qualified type name.
/// </remarks>
[AttributeUsage(
  AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
  AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
  AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
  Inherited = false)]
public sealed class DynamicallyAccessedMembersAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicallyAccessedMembersAttribute"/> class
  /// with the specified member types.
  /// </summary>
  /// <param name="memberTypes">The types of members dynamically accessed.</param>
  public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    => this.MemberTypes = memberTypes;

  /// <summary>
  /// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
  /// of members dynamically accessed.
  /// </summary>
  public DynamicallyAccessedMemberTypes MemberTypes { get; }

}

#endif
