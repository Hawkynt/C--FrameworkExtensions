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

// GotoExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents an unconditional jump. This includes return statements, break and continue statements,
/// and other jumps.
/// </summary>
public sealed class GotoExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="GotoExpression"/> class.
  /// </summary>
  internal GotoExpression(GotoExpressionKind kind, LabelTarget target, Expression? value, Type type)
    : base(ExpressionType.Goto, type) {
    this.Kind = kind;
    this.Target = target;
    this.Value = value;
  }

  /// <summary>
  /// The kind of the "go to" expression. Serves informational purposes only.
  /// </summary>
  /// <value>The <see cref="GotoExpressionKind"/> object representing the kind of the goto expression.</value>
  public GotoExpressionKind Kind { get; }

  /// <summary>
  /// The target label where this node jumps to.
  /// </summary>
  /// <value>The <see cref="LabelTarget"/> that is the target of this <see cref="GotoExpression"/>.</value>
  public LabelTarget Target { get; }

  /// <summary>
  /// The value passed to the target, or null if the target is of type <see cref="System.Void"/>.
  /// </summary>
  /// <value>The <see cref="Expression"/> object representing the value passed to the target.</value>
  public Expression? Value { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitGoto(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public GotoExpression Update(LabelTarget target, Expression? value) {
    if (target == this.Target && value == this.Value)
      return this;

    return new(this.Kind, target, value, this.Type);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="GotoExpression"/>.
  /// </summary>
  public override string ToString() {
    var kindStr = this.Kind switch {
      GotoExpressionKind.Break => "break",
      GotoExpressionKind.Continue => "continue",
      GotoExpressionKind.Return => "return",
      _ => "goto"
    };

    if (this.Value != null)
      return $"{kindStr} {this.Target} ({this.Value})";
    return $"{kindStr} {this.Target}";
  }

}

#endif
