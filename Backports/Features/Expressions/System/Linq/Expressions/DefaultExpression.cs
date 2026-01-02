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

// DefaultExpression was added in .NET 4.0 but depends on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Represents the default value of a type or an empty expression.
/// </summary>
public sealed class DefaultExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="DefaultExpression"/> class.
  /// </summary>
  internal DefaultExpression(Type type)
    : base(ExpressionType.Default, type) { }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitDefault(this);

  /// <summary>
  /// Returns a textual representation of the <see cref="DefaultExpression"/>.
  /// </summary>
  public override string ToString() => $"default({this.Type.Name})";

}

#endif
