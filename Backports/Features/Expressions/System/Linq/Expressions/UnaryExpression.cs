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

// UnaryExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents an expression that has a unary operator.
/// </summary>
public sealed class UnaryExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="UnaryExpression"/> class.
  /// </summary>
  internal UnaryExpression(ExpressionType nodeType, Expression operand, Type type, MethodInfo? method)
    : base(nodeType, type) {
    this.Operand = operand;
    this.Method = method;
  }

  /// <summary>
  /// Gets the operand of the unary operation.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the operand of the unary operation.</value>
  public Expression Operand { get; }

  /// <summary>
  /// Gets the implementing method for the unary operation.
  /// </summary>
  /// <value>The <see cref="MethodInfo"/> that represents the implementing method.</value>
  public MethodInfo? Method { get; }

  /// <summary>
  /// Gets a value that indicates whether the expression tree node represents a lifted call to an operator.
  /// </summary>
  /// <value><c>true</c> if the operator is lifted; otherwise, <c>false</c>.</value>
  public bool IsLifted => false;

  /// <summary>
  /// Gets a value that indicates whether the expression tree node represents a lifted call to an operator whose return type is lifted to a nullable type.
  /// </summary>
  /// <value><c>true</c> if the operator's return type is lifted to a nullable type; otherwise, <c>false</c>.</value>
  public bool IsLiftedToNull => false;

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitUnary(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public UnaryExpression Update(Expression operand) {
    if (operand == this.Operand)
      return this;

    return new(this.NodeType, operand, this.Type, this.Method);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="UnaryExpression"/>.
  /// </summary>
  public override string ToString() {
    var op = this.NodeType switch {
      ExpressionType.Negate or ExpressionType.NegateChecked => "-",
      ExpressionType.UnaryPlus => "+",
      ExpressionType.Not => "!",
      ExpressionType.Convert or ExpressionType.ConvertChecked => $"({this.Type.Name})",
      ExpressionType.TypeAs => $"as {this.Type.Name}",
      ExpressionType.ArrayLength => ".Length",
      ExpressionType.Quote => "Quote",
      ExpressionType.Throw => "throw",
      _ => this.NodeType.ToString()
    };

    return this.NodeType is ExpressionType.ArrayLength or ExpressionType.TypeAs
      ? $"({this.Operand}{op})"
      : $"({op}{this.Operand})";
  }

}

#endif
