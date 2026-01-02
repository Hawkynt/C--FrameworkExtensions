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

// MemberInitExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions;

/// <summary>
/// Represents calling a constructor and initializing one or more members of the new object.
/// </summary>
public sealed class MemberInitExpression : Expression {

  private readonly IList<MemberBinding> _bindings;

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberInitExpression"/> class.
  /// </summary>
  internal MemberInitExpression(NewExpression newExpression, IList<MemberBinding> bindings)
    : base(ExpressionType.MemberInit, newExpression.Type) {
    this.NewExpression = newExpression;
    this._bindings = bindings;
  }

  /// <summary>
  /// Gets the expression that represents the constructor call.
  /// </summary>
  /// <value>The <see cref="System.Linq.Expressions.NewExpression"/> that represents the constructor call.</value>
  public NewExpression NewExpression { get; }

  /// <summary>
  /// Gets the bindings that describe how to initialize the members of the newly created object.
  /// </summary>
  /// <value>A collection of <see cref="MemberBinding"/> objects which describe how to initialize the members.</value>
  public ReadOnlyCollection<MemberBinding> Bindings => new(this._bindings);

  /// <summary>
  /// Gets a value that indicates whether the expression tree node can be reduced.
  /// </summary>
  public override bool CanReduce => true;

  /// <summary>
  /// Reduces the <see cref="MemberInitExpression"/> to a simpler expression.
  /// </summary>
  public override Expression Reduce() {
    // MemberInit can be reduced to a block containing the new and member assignments
    // For simplicity, we just return the new expression as a basic reduction
    return this.NewExpression;
  }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitMemberInit(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MemberInitExpression Update(NewExpression newExpression, IEnumerable<MemberBinding> bindings) {
    var bindingList = bindings as IList<MemberBinding> ?? new List<MemberBinding>(bindings);
    if (newExpression == this.NewExpression && bindingList == this._bindings)
      return this;

    return new(newExpression, bindingList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberInitExpression"/>.
  /// </summary>
  public override string ToString() => $"new {this.NewExpression.Type.Name} {{ {string.Join(", ", this._bindings)} }}";

}

#endif
