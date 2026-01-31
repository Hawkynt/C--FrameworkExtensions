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

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class ListExtensions {

  /// <summary>
  /// Returns the list as an <see cref="IReadOnlyList{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements in the list.</typeparam>
  /// <param name="this">The list to convert.</param>
  /// <returns>
  /// On .NET 4.5+: the same list instance (since <see cref="List{T}"/> implements <see cref="IReadOnlyList{T}"/>).
  /// On older frameworks: a <see cref="ReadOnlyList{T}"/> wrapper.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IReadOnlyList<T> AsIReadOnlyList<T>(this List<T> @this)
#if SUPPORTS_READ_ONLY_COLLECTIONS
    => @this;
#else
    => new ReadOnlyList<T>(@this);
#endif

  /// <summary>
  /// Returns the list as an <see cref="IReadOnlyList{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements in the list.</typeparam>
  /// <param name="this">The list to convert.</param>
  /// <returns>
  /// On .NET 4.5+: the same list instance if it implements <see cref="IReadOnlyList{T}"/>; otherwise a <see cref="ReadOnlyList{T}"/> wrapper.
  /// On older frameworks: a <see cref="ReadOnlyList{T}"/> wrapper.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IReadOnlyList<T> AsIReadOnlyList<T>(this IList<T> @this)
#if SUPPORTS_READ_ONLY_COLLECTIONS
    => @this as IReadOnlyList<T> ?? new ReadOnlyList<T>(@this);
#else
    => new ReadOnlyList<T>(@this);
#endif
}
