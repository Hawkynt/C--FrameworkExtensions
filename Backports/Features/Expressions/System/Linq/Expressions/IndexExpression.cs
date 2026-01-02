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

// IndexExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents indexing a property or array.
/// </summary>
public sealed class IndexExpression : Expression {

  private readonly IList<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="IndexExpression"/> class.
  /// </summary>
  internal IndexExpression(Expression? @object, PropertyInfo? indexer, IList<Expression> arguments)
    : base(ExpressionType.Index, indexer?.PropertyType ?? @object?.Type.GetElementType() ?? throw new ArgumentException("Cannot determine element type")) {
    this.Object = @object;
    this.Indexer = indexer;
    this._arguments = arguments;
  }

  /// <summary>
  /// Gets the object to index.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the object to index.</value>
  public Expression? Object { get; }

  /// <summary>
  /// Gets the property for the indexer.
  /// </summary>
  /// <value>The <see cref="PropertyInfo"/> for the indexer property, if there is one, or <c>null</c> if the indexer represents an array access.</value>
  public PropertyInfo? Indexer { get; }

  /// <summary>
  /// Gets the arguments for the indexer.
  /// </summary>
  /// <value>A collection of <see cref="Expression"/> representing the arguments for the indexer.</value>
  public ReadOnlyCollection<Expression> Arguments => new(this._arguments);

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitIndex(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public IndexExpression Update(Expression? @object, IEnumerable<Expression> arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments);
    if (@object == this.Object && argList == this._arguments)
      return this;

    return new(@object, this.Indexer, argList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="IndexExpression"/>.
  /// </summary>
  public override string ToString() {
    if (this.Indexer != null)
      return $"{this.Object}[{string.Join(", ", this._arguments)}]";
    return $"{this.Object}[{string.Join(", ", this._arguments)}]";
  }

}

#endif
