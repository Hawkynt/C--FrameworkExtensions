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

// ParameterExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents a named parameter expression.
/// </summary>
public class ParameterExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="ParameterExpression"/> class.
  /// </summary>
  /// <param name="type">The type of the parameter.</param>
  /// <param name="name">The name of the parameter.</param>
  internal ParameterExpression(Type type, string? name)
    : base(ExpressionType.Parameter, type) =>
    this.Name = name;

  /// <summary>
  /// Gets the name of the parameter or variable.
  /// </summary>
  /// <value>A <see cref="string"/> that represents the name of the parameter or variable.</value>
  public string? Name { get; }

  /// <summary>
  /// Indicates that this <see cref="ParameterExpression"/> is to be treated as a <c>ByRef</c> parameter.
  /// </summary>
  /// <value><c>true</c> if this <see cref="ParameterExpression"/> is a <c>ByRef</c> parameter; otherwise, <c>false</c>.</value>
  public bool IsByRef => this.Type.IsByRef;

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitParameter(this);

  /// <summary>
  /// Returns a textual representation of the <see cref="ParameterExpression"/>.
  /// </summary>
  public override string ToString() => this.Name ?? $"Param_{this.GetHashCode():X}";

}

#endif
