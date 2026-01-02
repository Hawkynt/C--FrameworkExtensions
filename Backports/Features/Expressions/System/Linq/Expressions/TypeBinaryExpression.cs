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

// TypeBinaryExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents an operation between an expression and a type.
/// </summary>
public sealed class TypeBinaryExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="TypeBinaryExpression"/> class.
  /// </summary>
  internal TypeBinaryExpression(Expression expression, Type typeOperand, ExpressionType nodeType)
    : base(nodeType, typeof(bool)) {
    this.Expression = expression;
    this.TypeOperand = typeOperand;
  }

  /// <summary>
  /// Gets the expression operand of a type test operation.
  /// </summary>
  /// <value>An <see cref="System.Linq.Expressions.Expression"/> that represents the expression operand of a type test operation.</value>
  public Expression Expression { get; }

  /// <summary>
  /// Gets the type operand of a type test operation.
  /// </summary>
  /// <value>A <see cref="Type"/> that represents the type operand of a type test operation.</value>
  public Type TypeOperand { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitTypeBinary(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public TypeBinaryExpression Update(Expression expression) {
    if (expression == this.Expression)
      return this;

    return new(expression, this.TypeOperand, this.NodeType);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="TypeBinaryExpression"/>.
  /// </summary>
  public override string ToString() {
    var op = this.NodeType == ExpressionType.TypeIs ? "is" : "TypeEqual";
    return $"({this.Expression} {op} {this.TypeOperand.Name})";
  }

}

#endif
