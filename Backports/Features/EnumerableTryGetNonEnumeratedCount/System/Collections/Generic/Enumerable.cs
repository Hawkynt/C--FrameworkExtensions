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
//

#if !SUPPORTS_ENUMERABLE_TRYGETNONENUMERATEDCOUNT

using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class EnumerablePolyfills {

  /// <summary>
  ///   Attempts to determine the number of elements in a sequence without forcing an enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements of <paramref name="source" />.</typeparam>
  /// <param name="source">A sequence that contains elements to be counted.</param>
  /// <param name="count">
  ///     When this method returns, contains the count of <paramref name="source" /> if successful,
  ///     or zero if the method failed to determine the count.</param>
  /// <returns>
  ///   <see langword="true" /> if the count of <paramref name="source"/> can be determined without enumeration;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  /// <remarks>
  ///   The method performs a series of type tests, identifying common subtypes whose
  ///   count can be determined without enumerating; this includes <see cref="ICollection{T}"/>,
  ///   <see cref="ICollection"/>.
  ///
  ///   The method is typically a constant-time operation, but ultimately this depends on the complexity
  ///   characteristics of the underlying collection implementation.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetNonEnumeratedCount<TItem>(this IEnumerable<TItem> source, out int count) {
    switch (source) {
      case null:
        AlwaysThrow.ArgumentNullException(nameof(source));
        goto default;
      case TItem[] array:
        count = array.Length;
        return true;
      case ICollection<TItem> collection:
        count = collection.Count;
        return true;
      default:
        count = 0;
        return false;
    }
  }
  
}

#endif