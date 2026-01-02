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

// MemberBinding exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Provides the base class from which the classes that represent bindings that are used to initialize members of a newly created object derive.
/// </summary>
public abstract class MemberBinding {

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberBinding"/> class.
  /// </summary>
  protected MemberBinding(MemberBindingType type, MemberInfo member) {
    this.BindingType = type;
    this.Member = member;
  }

  /// <summary>
  /// Gets the type of binding that is represented.
  /// </summary>
  /// <value>One of the <see cref="MemberBindingType"/> values.</value>
  public MemberBindingType BindingType { get; }

  /// <summary>
  /// Gets the field or property to be initialized.
  /// </summary>
  /// <value>The <see cref="MemberInfo"/> that represents the field or property to be initialized.</value>
  public MemberInfo Member { get; }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberBinding"/>.
  /// </summary>
  public override string ToString() => $"{this.Member.Name}";

}

#endif
