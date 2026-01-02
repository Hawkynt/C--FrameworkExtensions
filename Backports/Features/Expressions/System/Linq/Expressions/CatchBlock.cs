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

// CatchBlock was added in .NET 4.0 but is used by expression types that depend on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents a catch statement in a try block.
/// </summary>
public sealed class CatchBlock {

  /// <summary>
  /// Initializes a new instance of the <see cref="CatchBlock"/> class.
  /// </summary>
  internal CatchBlock(Type test, ParameterExpression? variable, Expression body, Expression? filter) {
    this.Test = test;
    this.Variable = variable;
    this.Body = body;
    this.Filter = filter;
  }

  /// <summary>
  /// Gets a reference to the <see cref="Exception"/> object caught by this handler.
  /// </summary>
  /// <value>The <see cref="ParameterExpression"/> representing the exception variable.</value>
  public ParameterExpression? Variable { get; }

  /// <summary>
  /// Gets the type of <see cref="Exception"/> this handler catches.
  /// </summary>
  /// <value>The <see cref="Type"/> of exception this handler catches.</value>
  public Type Test { get; }

  /// <summary>
  /// Gets the body of the catch block.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the catch body.</value>
  public Expression Body { get; }

  /// <summary>
  /// Gets the body of the <see cref="CatchBlock"/>'s filter.
  /// </summary>
  /// <value>The <see cref="Expression"/> representing the filter body, or null if no filter.</value>
  public Expression? Filter { get; }

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public CatchBlock Update(ParameterExpression? variable, Expression? filter, Expression body) {
    if (variable == this.Variable && filter == this.Filter && body == this.Body)
      return this;

    return new(this.Test, variable, body, filter);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="CatchBlock"/>.
  /// </summary>
  public override string ToString() {
    var varStr = this.Variable != null ? $" {this.Variable.Name}" : string.Empty;
    var filterStr = this.Filter != null ? $" when ({this.Filter})" : string.Empty;
    return $"catch ({this.Test.Name}{varStr}){filterStr} {{ {this.Body} }}";
  }

}

#endif
