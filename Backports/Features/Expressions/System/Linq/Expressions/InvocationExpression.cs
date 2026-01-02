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

// InvocationExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents an expression that applies a delegate or lambda expression to a list of argument expressions.
/// </summary>
public sealed class InvocationExpression : Expression {

  private readonly IList<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="InvocationExpression"/> class.
  /// </summary>
  internal InvocationExpression(Expression expression, IList<Expression> arguments)
    : base(ExpressionType.Invoke, GetReturnType(expression.Type)) {
    this.Expression = expression;
    this._arguments = arguments;
  }

  private static Type GetReturnType(Type delegateType) {
    var invokeMethod = delegateType.GetMethod("Invoke");
    return invokeMethod?.ReturnType ?? typeof(object);
  }

  /// <summary>
  /// Gets the delegate or lambda expression to be applied.
  /// </summary>
  /// <value>An <see cref="System.Linq.Expressions.Expression"/> that represents the delegate to be applied.</value>
  public Expression Expression { get; }

  /// <summary>
  /// Gets the arguments that the delegate or lambda expression is applied to.
  /// </summary>
  /// <value>A <see cref="ReadOnlyCollection{T}"/> of <see cref="System.Linq.Expressions.Expression"/> objects which represent the arguments that the delegate is applied to.</value>
  public ReadOnlyCollection<Expression> Arguments => new(this._arguments);

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitInvocation(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public InvocationExpression Update(Expression expression, IEnumerable<Expression> arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments);
    if (expression == this.Expression && ReferenceEquals(argList, this._arguments))
      return this;

    return new(expression, argList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="InvocationExpression"/>.
  /// </summary>
  public override string ToString() {
    var args = JoinExpressions(this._arguments);
    return $"Invoke({this.Expression}, {args})";
  }

  private static string JoinExpressions(IList<Expression> items) {
    if (items.Count == 0)
      return string.Empty;
    var result = new System.Text.StringBuilder();
    for (var i = 0; i < items.Count; ++i) {
      if (i > 0)
        result.Append(", ");
      result.Append(items[i]);
    }
    return result.ToString();
  }

}

#endif
