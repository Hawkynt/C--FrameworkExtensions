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

// MemberAssignment exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents assignment operation for a field or property of an object.
/// </summary>
public sealed class MemberAssignment : MemberBinding {

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberAssignment"/> class.
  /// </summary>
  internal MemberAssignment(MemberInfo member, Expression expression)
    : base(MemberBindingType.Assignment, member) {
    this.Expression = expression;
  }

  /// <summary>
  /// Gets the expression to assign to the field or property.
  /// </summary>
  /// <value>The <see cref="System.Linq.Expressions.Expression"/> that represents the value to assign to the field or property.</value>
  public Expression Expression { get; }

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MemberAssignment Update(Expression expression) {
    if (expression == this.Expression)
      return this;

    return new(this.Member, expression);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberAssignment"/>.
  /// </summary>
  public override string ToString() => $"{this.Member.Name} = {this.Expression}";

}

#endif
