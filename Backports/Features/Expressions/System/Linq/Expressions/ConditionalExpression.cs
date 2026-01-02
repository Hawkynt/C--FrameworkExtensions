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

// ConditionalExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents an expression that has a conditional operator.
/// </summary>
public class ConditionalExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="ConditionalExpression"/> class.
  /// </summary>
  internal ConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse, Type type)
    : base(ExpressionType.Conditional, type) {
    this.Test = test;
    this.IfTrue = ifTrue;
    this.IfFalse = ifFalse;
  }

  /// <summary>
  /// Gets the test of the conditional operation.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the test of the conditional operation.</value>
  public Expression Test { get; }

  /// <summary>
  /// Gets the expression to execute if the test evaluates to <c>true</c>.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the expression to execute if the test is <c>true</c>.</value>
  public Expression IfTrue { get; }

  /// <summary>
  /// Gets the expression to execute if the test evaluates to <c>false</c>.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the expression to execute if the test is <c>false</c>.</value>
  public Expression IfFalse { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitConditional(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public ConditionalExpression Update(Expression test, Expression ifTrue, Expression ifFalse) {
    if (test == this.Test && ifTrue == this.IfTrue && ifFalse == this.IfFalse)
      return this;

    return new(test, ifTrue, ifFalse, this.Type);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="ConditionalExpression"/>.
  /// </summary>
  public override string ToString() => $"({this.Test} ? {this.IfTrue} : {this.IfFalse})";

}

#endif
