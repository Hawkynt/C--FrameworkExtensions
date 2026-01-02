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

// TryExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a try/catch/finally/fault block.
/// </summary>
public sealed class TryExpression : Expression {

  private readonly IList<CatchBlock> _handlers;

  /// <summary>
  /// Initializes a new instance of the <see cref="TryExpression"/> class.
  /// </summary>
  internal TryExpression(Type type, Expression body, Expression? @finally, Expression? fault, IList<CatchBlock> handlers)
    : base(ExpressionType.Try, type) {
    this.Body = body;
    this.Finally = @finally;
    this.Fault = fault;
    this._handlers = handlers;
  }

  /// <summary>
  /// Gets the <see cref="Expression"/> representing the body of the try block.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the try body.</value>
  public Expression Body { get; }

  /// <summary>
  /// Gets the collection of <see cref="CatchBlock"/>s associated with the try block.
  /// </summary>
  /// <value>The <see cref="ReadOnlyCollection{T}"/> of <see cref="CatchBlock"/>s.</value>
  public ReadOnlyCollection<CatchBlock> Handlers => new(this._handlers);

  /// <summary>
  /// Gets the <see cref="Expression"/> representing the finally block.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the finally block, or null if none.</value>
  public Expression? Finally { get; }

  /// <summary>
  /// Gets the <see cref="Expression"/> representing the fault block.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the fault block, or null if none.</value>
  public Expression? Fault { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitTry(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public TryExpression Update(Expression body, IEnumerable<CatchBlock> handlers, Expression? @finally, Expression? fault) {
    var handlerList = handlers as IList<CatchBlock> ?? new List<CatchBlock>(handlers);

    if (body == this.Body && ReferenceEquals(handlerList, this._handlers) && @finally == this.Finally && fault == this.Fault)
      return this;

    return new(this.Type, body, @finally, fault, handlerList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="TryExpression"/>.
  /// </summary>
  public override string ToString() {
    var catchStr = JoinHandlers(this._handlers);
    var finallyStr = this.Finally != null ? $" finally {{ {this.Finally} }}" : string.Empty;
    var faultStr = this.Fault != null ? $" fault {{ {this.Fault} }}" : string.Empty;
    return $"try {{ {this.Body} }}{catchStr}{finallyStr}{faultStr}";
  }

  private static string JoinHandlers(IList<CatchBlock> items) {
    if (items.Count == 0)
      return string.Empty;
    var result = new System.Text.StringBuilder();
    for (var i = 0; i < items.Count; ++i) {
      result.Append(" ");
      result.Append(items[i]);
    }
    return result.ToString();
  }

}

#endif
