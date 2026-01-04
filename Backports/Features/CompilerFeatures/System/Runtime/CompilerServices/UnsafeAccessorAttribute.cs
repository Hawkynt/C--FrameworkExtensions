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

// UnsafeAccessorAttribute was added in .NET 8.0
#if !SUPPORTS_UNSAFE_ACCESSOR_ATTRIBUTE

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides access to private and internal members of a type.
/// </summary>
/// <remarks>
/// <para>
/// This attribute can be applied to an extern static method to provide access to private or internal
/// members of the first parameter's type. The runtime will generate code that directly accesses
/// the member without going through reflection.
/// </para>
/// <para>
/// Note: This polyfill only provides the attribute definition for compilation purposes.
/// The actual member access functionality requires runtime support and is only available in .NET 8.0+.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class UnsafeAccessorAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="UnsafeAccessorAttribute"/> class with the specified accessor kind.
  /// </summary>
  /// <param name="kind">The kind of member being accessed.</param>
  public UnsafeAccessorAttribute(UnsafeAccessorKind kind) => this.Kind = kind;

  /// <summary>
  /// Gets the kind of member being accessed.
  /// </summary>
  public UnsafeAccessorKind Kind { get; }

  /// <summary>
  /// Gets or sets the name of the member to access.
  /// </summary>
  /// <remarks>
  /// If not specified, the name of the attributed method is used.
  /// For constructors, this property is ignored.
  /// </remarks>
  public string? Name { get; set; }

}

#endif
