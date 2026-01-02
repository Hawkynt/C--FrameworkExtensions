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

// BinaryExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents an expression that has a binary operator.
/// </summary>
public class BinaryExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="BinaryExpression"/> class.
  /// </summary>
  internal BinaryExpression(ExpressionType nodeType, Expression left, Expression right, Type type, MethodInfo? method)
    : base(nodeType, type) {
    this.Left = left;
    this.Right = right;
    this.Method = method;
  }

  /// <summary>
  /// Gets the left operand of the binary operation.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the left operand of the binary operation.</value>
  public Expression Left { get; }

  /// <summary>
  /// Gets the right operand of the binary operation.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the right operand of the binary operation.</value>
  public Expression Right { get; }

  /// <summary>
  /// Gets the implementing method for the binary operation.
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
  /// Gets the type conversion function that is used by a coalescing or compound assignment operation.
  /// </summary>
  /// <value>A <see cref="LambdaExpression"/> that represents a type conversion function.</value>
  public LambdaExpression? Conversion => null;

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitBinary(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public BinaryExpression Update(Expression left, LambdaExpression? conversion, Expression right) {
    if (left == this.Left && right == this.Right && conversion == this.Conversion)
      return this;

    return new(this.NodeType, left, right, this.Type, this.Method);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="BinaryExpression"/>.
  /// </summary>
  public override string ToString() {
    var op = this.NodeType switch {
      ExpressionType.Add or ExpressionType.AddChecked => "+",
      ExpressionType.Subtract or ExpressionType.SubtractChecked => "-",
      ExpressionType.Multiply or ExpressionType.MultiplyChecked => "*",
      ExpressionType.Divide => "/",
      ExpressionType.Modulo => "%",
      ExpressionType.And => "&",
      ExpressionType.Or => "|",
      ExpressionType.ExclusiveOr => "^",
      ExpressionType.AndAlso => "&&",
      ExpressionType.OrElse => "||",
      ExpressionType.Equal => "==",
      ExpressionType.NotEqual => "!=",
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      ExpressionType.LeftShift => "<<",
      ExpressionType.RightShift => ">>",
      ExpressionType.Coalesce => "??",
      ExpressionType.ArrayIndex => "[]",
      _ => this.NodeType.ToString()
    };

    return $"({this.Left} {op} {this.Right})";
  }

}

#endif
