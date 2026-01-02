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

// LoopExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents an infinite loop. It can be exited with "break".
/// </summary>
public sealed class LoopExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="LoopExpression"/> class.
  /// </summary>
  internal LoopExpression(Expression body, LabelTarget? breakLabel, LabelTarget? continueLabel)
    : base(ExpressionType.Loop, breakLabel?.Type ?? typeof(void)) {
    this.Body = body;
    this.BreakLabel = breakLabel;
    this.ContinueLabel = continueLabel;
  }

  /// <summary>
  /// Gets the <see cref="Expression"/> that is the body of the loop.
  /// </summary>
  /// <value>The <see cref="Expression"/> that is the body of the loop.</value>
  public Expression Body { get; }

  /// <summary>
  /// Gets the <see cref="LabelTarget"/> that is used by the loop body as a break statement target.
  /// </summary>
  /// <value>The <see cref="LabelTarget"/> that is used by the loop body as a break statement target.</value>
  public LabelTarget? BreakLabel { get; }

  /// <summary>
  /// Gets the <see cref="LabelTarget"/> that is used by the loop body as a continue statement target.
  /// </summary>
  /// <value>The <see cref="LabelTarget"/> that is used by the loop body as a continue statement target.</value>
  public LabelTarget? ContinueLabel { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitLoop(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public LoopExpression Update(LabelTarget? breakLabel, LabelTarget? continueLabel, Expression body) {
    if (breakLabel == this.BreakLabel && continueLabel == this.ContinueLabel && body == this.Body)
      return this;

    return new(body, breakLabel, continueLabel);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="LoopExpression"/>.
  /// </summary>
  public override string ToString() => $"loop {{ {this.Body} }}";

}

#endif
