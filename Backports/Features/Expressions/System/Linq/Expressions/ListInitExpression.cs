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

// ListInitExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a constructor call that has a collection initializer.
/// </summary>
public sealed class ListInitExpression : Expression {

  private readonly IList<ElementInit> _initializers;

  /// <summary>
  /// Initializes a new instance of the <see cref="ListInitExpression"/> class.
  /// </summary>
  internal ListInitExpression(NewExpression newExpression, IList<ElementInit> initializers)
    : base(ExpressionType.ListInit, newExpression.Type) {
    this.NewExpression = newExpression;
    this._initializers = initializers;
  }

  /// <summary>
  /// Gets the expression that contains a call to the constructor of a collection type.
  /// </summary>
  /// <value>The <see cref="System.Linq.Expressions.NewExpression"/> that represents the call to the constructor.</value>
  public NewExpression NewExpression { get; }

  /// <summary>
  /// Gets the element initializers that are used to initialize a collection.
  /// </summary>
  /// <value>A collection of <see cref="ElementInit"/> objects which represent the elements that are used to initialize the collection.</value>
  public ReadOnlyCollection<ElementInit> Initializers => new(this._initializers);

  /// <summary>
  /// Gets a value that indicates whether the expression tree node can be reduced.
  /// </summary>
  public override bool CanReduce => true;

  /// <summary>
  /// Reduces the <see cref="ListInitExpression"/> to a simpler expression.
  /// </summary>
  public override Expression Reduce() {
    // ListInit can be reduced to a block containing the new and Add method calls
    // For simplicity, we just return the new expression as a basic reduction
    return this.NewExpression;
  }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitListInit(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public ListInitExpression Update(NewExpression newExpression, IEnumerable<ElementInit> initializers) {
    var initList = initializers as IList<ElementInit> ?? new List<ElementInit>(initializers);
    if (newExpression == this.NewExpression && initList == this._initializers)
      return this;

    return new(newExpression, initList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="ListInitExpression"/>.
  /// </summary>
  public override string ToString() => $"new {this.NewExpression.Type.Name} {{ {string.Join(", ", this._initializers)} }}";

}

#endif
