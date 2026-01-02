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

// MemberExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents accessing a field or property.
/// </summary>
public class MemberExpression : Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberExpression"/> class for a property.
  /// </summary>
  internal MemberExpression(Expression? expression, PropertyInfo property)
    : base(ExpressionType.MemberAccess, property.PropertyType) {
    this.Expression = expression;
    this.Member = property;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberExpression"/> class for a field.
  /// </summary>
  internal MemberExpression(Expression? expression, FieldInfo field)
    : base(ExpressionType.MemberAccess, field.FieldType) {
    this.Expression = expression;
    this.Member = field;
  }

  /// <summary>
  /// Gets the containing object of the field or property.
  /// </summary>
  /// <value>An <see cref="System.Linq.Expressions.Expression"/> that represents the containing object of the field or property.</value>
  public Expression? Expression { get; }

  /// <summary>
  /// Gets the field or property to be accessed.
  /// </summary>
  /// <value>The <see cref="MemberInfo"/> that represents the field or property to be accessed.</value>
  public MemberInfo Member { get; }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitMember(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MemberExpression Update(Expression? expression) {
    if (expression == this.Expression)
      return this;

    if (this.Member is PropertyInfo property)
      return new(expression, property);
    return new(expression, (FieldInfo)this.Member);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberExpression"/>.
  /// </summary>
  public override string ToString() =>
    this.Expression != null
      ? $"{this.Expression}.{this.Member.Name}"
      : this.Member.Name;

}

#endif
