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

// BlockExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a block that contains a sequence of expressions where variables can be defined.
/// </summary>
public class BlockExpression : Expression {

  private readonly IList<ParameterExpression> _variables;
  private readonly IList<Expression> _expressions;

  /// <summary>
  /// Initializes a new instance of the <see cref="BlockExpression"/> class.
  /// </summary>
  internal BlockExpression(IList<ParameterExpression> variables, IList<Expression> expressions, Type type)
    : base(ExpressionType.Block, type) {
    this._variables = variables;
    this._expressions = expressions;
  }

  /// <summary>
  /// Gets the variables defined in this block.
  /// </summary>
  /// <value>The <see cref="ReadOnlyCollection{T}"/> containing all variables defined in this block.</value>
  public ReadOnlyCollection<ParameterExpression> Variables => new(this._variables);

  /// <summary>
  /// Gets the expressions in this block.
  /// </summary>
  /// <value>The <see cref="ReadOnlyCollection{T}"/> containing all expressions in this block.</value>
  public ReadOnlyCollection<Expression> Expressions => new(this._expressions);

  /// <summary>
  /// Gets the last expression in this block.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the last expression in this block.</value>
  public Expression Result => this._expressions.Count > 0 ? this._expressions[^1] : Expression.Default(typeof(void));

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitBlock(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public BlockExpression Update(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
    var varList = variables as IList<ParameterExpression> ?? new List<ParameterExpression>(variables);
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions);

    if (ReferenceEquals(varList, this._variables) && ReferenceEquals(exprList, this._expressions))
      return this;

    return new(varList, exprList, this.Type);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="BlockExpression"/>.
  /// </summary>
  public override string ToString() {
    var items = JoinExpressions(this._expressions);
    return $"{{ {items} }}";
  }

  private static string JoinExpressions(IList<Expression> items) {
    if (items.Count == 0)
      return string.Empty;
    var result = new System.Text.StringBuilder();
    for (var i = 0; i < items.Count; ++i) {
      if (i > 0)
        result.Append("; ");
      result.Append(items[i]);
    }
    return result.ToString();
  }

}

#endif
