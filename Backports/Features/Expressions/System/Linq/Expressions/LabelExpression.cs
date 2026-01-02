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

// LabelExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents a label, which can be placed in any <see cref="Expression"/> context.
/// If it is jumped to, it will get the value provided by the corresponding <see cref="GotoExpression"/>.
/// Otherwise, it receives the value in <see cref="DefaultValue"/>.
/// </summary>
public sealed class LabelExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="LabelExpression"/> class.
  /// </summary>
  internal LabelExpression(LabelTarget target, Expression? defaultValue)
    : base(ExpressionType.Label, target.Type) {
    this.Target = target;
    this.DefaultValue = defaultValue;
  }

  /// <summary>
  /// The <see cref="LabelTarget"/> which this label is associated with.
  /// </summary>
  /// <value>The <see cref="LabelTarget"/> which this label is associated with.</value>
  public LabelTarget Target { get; }

  /// <summary>
  /// The value of the <see cref="LabelExpression"/> when the label is reached through normal control flow
  /// (e.g., is not jumped to).
  /// </summary>
  /// <value>The <see cref="Expression"/> object representing the default value of this <see cref="LabelExpression"/>.</value>
  public Expression? DefaultValue { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitLabel(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public LabelExpression Update(LabelTarget target, Expression? defaultValue) {
    if (target == this.Target && defaultValue == this.DefaultValue)
      return this;

    return new(target, defaultValue);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="LabelExpression"/>.
  /// </summary>
  public override string ToString() {
    if (this.DefaultValue != null)
      return $"{this.Target}: ({this.DefaultValue})";
    return $"{this.Target}:";
  }

}

#endif
