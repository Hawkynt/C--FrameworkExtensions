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

// NewExpression exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a constructor call.
/// </summary>
public class NewExpression : Expression {

  private readonly IList<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="NewExpression"/> class.
  /// </summary>
  internal NewExpression(ConstructorInfo constructor, IList<Expression> arguments)
    : base(ExpressionType.New, constructor.DeclaringType!) {
    this.Constructor = constructor;
    this._arguments = arguments;
  }

  /// <summary>
  /// Gets the called constructor.
  /// </summary>
  /// <value>The <see cref="ConstructorInfo"/> that represents the called constructor.</value>
  public ConstructorInfo? Constructor { get; }

  /// <summary>
  /// Gets the arguments to the constructor.
  /// </summary>
  /// <value>A collection of <see cref="Expression"/> objects that represent the arguments to the constructor.</value>
  public ReadOnlyCollection<Expression> Arguments => new(this._arguments);

  /// <summary>
  /// Gets the members that can retrieve the values of the fields that were initialized with constructor arguments.
  /// </summary>
  /// <value>A collection of <see cref="MemberInfo"/> objects that represent the members that can retrieve the values of the fields that were initialized with constructor arguments.</value>
  public ReadOnlyCollection<MemberInfo>? Members => null;

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitNew(this);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public NewExpression Update(IEnumerable<Expression> arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments);
    if (ReferenceEquals(argList, this._arguments))
      return this;

    return new(this.Constructor!, argList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="NewExpression"/>.
  /// </summary>
  public override string ToString() {
    var args = JoinExpressions(this._arguments);
    return $"new {this.Type.Name}({args})";
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
