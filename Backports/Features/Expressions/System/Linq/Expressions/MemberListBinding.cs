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

// MemberListBinding exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents initializing the elements of a collection member of a newly created object.
/// </summary>
public sealed class MemberListBinding : MemberBinding {

  private readonly IList<ElementInit> _initializers;

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberListBinding"/> class.
  /// </summary>
  internal MemberListBinding(MemberInfo member, IList<ElementInit> initializers)
    : base(MemberBindingType.ListBinding, member) {
    this._initializers = initializers;
  }

  /// <summary>
  /// Gets the element initializers for initializing a collection member of a newly created object.
  /// </summary>
  /// <value>A collection of <see cref="ElementInit"/> objects to initialize a collection member with.</value>
  public ReadOnlyCollection<ElementInit> Initializers => new(this._initializers);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MemberListBinding Update(IEnumerable<ElementInit> initializers) {
    var initList = initializers as IList<ElementInit> ?? new List<ElementInit>(initializers);
    if (initList == this._initializers)
      return this;

    return new(this.Member, initList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberListBinding"/>.
  /// </summary>
  public override string ToString() => $"{this.Member.Name} = {{ {string.Join(", ", this._initializers)} }}";

}

#endif
