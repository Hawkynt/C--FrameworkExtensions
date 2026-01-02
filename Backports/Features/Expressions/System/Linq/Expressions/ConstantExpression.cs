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

// ConstantExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents an expression that has a constant value.
/// </summary>
public class ConstantExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
  /// </summary>
  /// <param name="value">The constant value.</param>
  /// <param name="type">The type of the constant value.</param>
  internal ConstantExpression(object? value, Type type)
    : base(ExpressionType.Constant, type) =>
    this.Value = value;

  /// <summary>
  /// Gets the value of the constant expression.
  /// </summary>
  /// <value>An <see cref="object"/> equal to the value of the represented expression.</value>
  public object? Value { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitConstant(this);

  /// <summary>
  /// Returns a textual representation of the <see cref="ConstantExpression"/>.
  /// </summary>
  public override string ToString() => this.Value?.ToString() ?? "null";

}

#endif
