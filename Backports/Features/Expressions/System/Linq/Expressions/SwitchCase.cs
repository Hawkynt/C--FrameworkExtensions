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

// SwitchCase was added in .NET 4.0 but is used by expression types that depend on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents one case of a <see cref="SwitchExpression"/>.
/// </summary>
public sealed class SwitchCase {

  private readonly IList<Expression> _testValues;

  /// <summary>
  /// Initializes a new instance of the <see cref="SwitchCase"/> class.
  /// </summary>
  internal SwitchCase(Expression body, IList<Expression> testValues) {
    this.Body = body;
    this._testValues = testValues;
  }

  /// <summary>
  /// Gets the values of this case. This case is selected for execution when the <see cref="SwitchExpression.SwitchValue"/>
  /// matches any of these values.
  /// </summary>
  /// <value>The <see cref="ReadOnlyCollection{T}"/> containing the test values.</value>
  public ReadOnlyCollection<Expression> TestValues => new(this._testValues);

  /// <summary>
  /// Gets the body of this case.
  /// </summary>
  /// <value>The <see cref="Expression"/> object representing the body of this case.</value>
  public Expression Body { get; }

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public SwitchCase Update(IEnumerable<Expression> testValues, Expression body) {
    var testList = testValues as IList<Expression> ?? new List<Expression>(testValues);

    if (ReferenceEquals(testList, this._testValues) && body == this.Body)
      return this;

    return new(body, testList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="SwitchCase"/>.
  /// </summary>
  public override string ToString() {
    var tests = JoinExpressions(this._testValues);
    return $"case {tests}: {this.Body}";
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
