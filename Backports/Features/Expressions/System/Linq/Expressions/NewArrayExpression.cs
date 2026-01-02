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

// NewArrayExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents creating a new array and possibly initializing the elements of the new array.
/// </summary>
public class NewArrayExpression : Expression {

  private readonly IList<Expression> _expressions;

  /// <summary>
  /// Initializes a new instance of the <see cref="NewArrayExpression"/> class.
  /// </summary>
  internal NewArrayExpression(ExpressionType nodeType, Type type, IList<Expression> expressions)
    : base(nodeType, type) =>
    this._expressions = expressions;

  /// <summary>
  /// Gets the bounds of the array if the value of the <see cref="Expression.NodeType"/> property is
  /// <see cref="ExpressionType.NewArrayBounds"/>, or the values to initialize the elements of the new array
  /// if the value of the <see cref="Expression.NodeType"/> property is <see cref="ExpressionType.NewArrayInit"/>.
  /// </summary>
  /// <value>A <see cref="ReadOnlyCollection{T}"/> of <see cref="Expression"/> objects which represent either the bounds
  /// of the array or the initialization values.</value>
  public ReadOnlyCollection<Expression> Expressions => new(this._expressions);

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitNewArray(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public NewArrayExpression Update(IEnumerable<Expression> expressions) {
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions);
    if (ReferenceEquals(exprList, this._expressions))
      return this;

    return new(this.NodeType, this.Type, exprList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="NewArrayExpression"/>.
  /// </summary>
  public override string ToString() {
    var elementType = this.Type.GetElementType()!;
    var items = JoinExpressions(this._expressions);
    if (this.NodeType == ExpressionType.NewArrayInit)
      return $"new {elementType.Name}[] {{ {items} }}";

    return $"new {elementType.Name}[{items}]";
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
