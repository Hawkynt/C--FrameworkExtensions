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

// ElementInit exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents an initializer for a single element of an <see cref="System.Collections.IEnumerable"/> collection.
/// </summary>
public sealed class ElementInit {

  private readonly IList<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="ElementInit"/> class.
  /// </summary>
  internal ElementInit(MethodInfo addMethod, IList<Expression> arguments) {
    this.AddMethod = addMethod;
    this._arguments = arguments;
  }

  /// <summary>
  /// Gets the instance method that is used to add an element to an <see cref="System.Collections.IEnumerable"/> collection.
  /// </summary>
  /// <value>The <see cref="MethodInfo"/> that represents an instance method that adds an element to a collection.</value>
  public MethodInfo AddMethod { get; }

  /// <summary>
  /// Gets the collection of arguments that are passed to a method that adds an element to an <see cref="System.Collections.IEnumerable"/> collection.
  /// </summary>
  /// <value>A collection of <see cref="Expression"/> that represent the arguments for a method that adds an element to a collection.</value>
  public ReadOnlyCollection<Expression> Arguments => new(this._arguments);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public ElementInit Update(IEnumerable<Expression> arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments);
    if (argList == this._arguments)
      return this;

    return new(this.AddMethod, argList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="ElementInit"/>.
  /// </summary>
  public override string ToString() => $"{this.AddMethod.Name}({string.Join(", ", this._arguments)})";

}

#endif
