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

// MethodCallExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a call to either static or an instance method.
/// </summary>
public class MethodCallExpression : Expression {

  private readonly IList<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="MethodCallExpression"/> class.
  /// </summary>
  internal MethodCallExpression(Expression? @object, MethodInfo method, IList<Expression> arguments)
    : base(ExpressionType.Call, method.ReturnType) {
    this.Object = @object;
    this.Method = method;
    this._arguments = arguments;
  }

  /// <summary>
  /// Gets the <see cref="Expression"/> that represents the instance for instance method calls or null for static method calls.
  /// </summary>
  /// <value>An <see cref="Expression"/> that represents the receiving object of the method.</value>
  public Expression? Object { get; }

  /// <summary>
  /// Gets the <see cref="MethodInfo"/> for the method to be called.
  /// </summary>
  /// <value>The <see cref="MethodInfo"/> that represents the called method.</value>
  public MethodInfo Method { get; }

  /// <summary>
  /// Gets a collection of expressions that represent arguments to the method call.
  /// </summary>
  /// <value>A <see cref="ReadOnlyCollection{T}"/> of <see cref="Expression"/> objects representing the arguments to the method.</value>
  public ReadOnlyCollection<Expression> Arguments => new(this._arguments);

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitMethodCall(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MethodCallExpression Update(Expression? @object, IEnumerable<Expression> arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments);
    if (@object == this.Object && ReferenceEquals(argList, this._arguments))
      return this;

    return new(@object, this.Method, argList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MethodCallExpression"/>.
  /// </summary>
  public override string ToString() {
    var args = JoinExpressions(this._arguments);
    if (this.Object != null)
      return $"{this.Object}.{this.Method.Name}({args})";
    return $"{this.Method.DeclaringType?.Name}.{this.Method.Name}({args})";
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
