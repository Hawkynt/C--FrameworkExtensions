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

// SwitchExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a control expression that handles multiple selections by passing control to a <see cref="SwitchCase"/>.
/// </summary>
public sealed class SwitchExpression : Expression {

  private readonly IList<SwitchCase> _cases;

  /// <summary>
  /// Initializes a new instance of the <see cref="SwitchExpression"/> class.
  /// </summary>
  internal SwitchExpression(Type type, Expression switchValue, Expression? defaultBody, MethodInfo? comparison, IList<SwitchCase> cases)
    : base(ExpressionType.Switch, type) {
    this.SwitchValue = switchValue;
    this.DefaultBody = defaultBody;
    this.Comparison = comparison;
    this._cases = cases;
  }

  /// <summary>
  /// Gets the test for the switch.
  /// </summary>
  /// <value>The <see cref="Expression"/> object representing the switch test.</value>
  public Expression SwitchValue { get; }

  /// <summary>
  /// Gets the collection of <see cref="SwitchCase"/> objects for the switch.
  /// </summary>
  /// <value>The <see cref="ReadOnlyCollection{T}"/> containing the switch cases.</value>
  public ReadOnlyCollection<SwitchCase> Cases => new(this._cases);

  /// <summary>
  /// Gets the test for the switch.
  /// </summary>
  /// <value>The <see cref="Expression"/> object representing the default body of the switch.</value>
  public Expression? DefaultBody { get; }

  /// <summary>
  /// Gets the equality comparison method, if any.
  /// </summary>
  /// <value>The <see cref="MethodInfo"/> representing the equality comparison method.</value>
  public MethodInfo? Comparison { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitSwitch(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public SwitchExpression Update(Expression switchValue, IEnumerable<SwitchCase> cases, Expression? defaultBody) {
    var caseList = cases as IList<SwitchCase> ?? new List<SwitchCase>(cases);

    if (switchValue == this.SwitchValue && ReferenceEquals(caseList, this._cases) && defaultBody == this.DefaultBody)
      return this;

    return new(this.Type, switchValue, defaultBody, this.Comparison, caseList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="SwitchExpression"/>.
  /// </summary>
  public override string ToString() {
    var cases = JoinCases(this._cases);
    var defaultStr = this.DefaultBody != null ? $" default: {this.DefaultBody}" : string.Empty;
    return $"switch ({this.SwitchValue}) {{ {cases}{defaultStr} }}";
  }

  private static string JoinCases(IList<SwitchCase> items) {
    if (items.Count == 0)
      return string.Empty;
    var result = new System.Text.StringBuilder();
    for (var i = 0; i < items.Count; ++i) {
      if (i > 0)
        result.Append(" ");
      result.Append(items[i]);
    }
    return result.ToString();
  }

}

#endif
