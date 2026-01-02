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

// MemberMemberBinding exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Represents initializing members of a member of a newly created object.
/// </summary>
public sealed class MemberMemberBinding : MemberBinding {

  private readonly IList<MemberBinding> _bindings;

  /// <summary>
  /// Initializes a new instance of the <see cref="MemberMemberBinding"/> class.
  /// </summary>
  internal MemberMemberBinding(MemberInfo member, IList<MemberBinding> bindings)
    : base(MemberBindingType.MemberBinding, member) {
    this._bindings = bindings;
  }

  /// <summary>
  /// Gets the bindings that describe how to initialize the members of a member.
  /// </summary>
  /// <value>A collection of <see cref="MemberBinding"/> objects that describe how to initialize the members of the member.</value>
  public ReadOnlyCollection<MemberBinding> Bindings => new(this._bindings);

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  public MemberMemberBinding Update(IEnumerable<MemberBinding> bindings) {
    var bindingList = bindings as IList<MemberBinding> ?? new List<MemberBinding>(bindings);
    if (bindingList == this._bindings)
      return this;

    return new(this.Member, bindingList);
  }

  /// <summary>
  /// Returns a textual representation of the <see cref="MemberMemberBinding"/>.
  /// </summary>
  public override string ToString() => $"{this.Member.Name} = {{ {string.Join(", ", this._bindings)} }}";

}

#endif
