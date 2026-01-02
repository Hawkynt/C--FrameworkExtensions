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

// MemberBindingType exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Describes the binding types that are used in <see cref="MemberInitExpression"/> objects.
/// </summary>
public enum MemberBindingType {

  /// <summary>
  /// A binding that represents initializing a member with the value of an expression.
  /// </summary>
  Assignment = 0,

  /// <summary>
  /// A binding that represents recursively initializing members of a member.
  /// </summary>
  MemberBinding = 1,

  /// <summary>
  /// A binding that represents initializing a member of type <see cref="System.Collections.IList"/> or <see cref="System.Collections.Generic.ICollection{T}"/> from a list of elements.
  /// </summary>
  ListBinding = 2

}

#endif
