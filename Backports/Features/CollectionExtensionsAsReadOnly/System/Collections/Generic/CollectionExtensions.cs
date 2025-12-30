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

#if !SUPPORTS_COLLECTIONEXTENSIONS_ASREADONLY

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

/// <summary>
/// Polyfills for <see cref="CollectionExtensions"/> methods added in .NET 6.0.
/// </summary>
public static partial class CollectionExtensionsPolyfills {

  /// <summary>
  /// Returns a read-only <see cref="ReadOnlyCollection{T}"/> wrapper for the specified list.
  /// </summary>
  /// <typeparam name="T">The type of elements in the collection.</typeparam>
  /// <param name="list">The list to wrap.</param>
  /// <returns>A read-only wrapper around the specified list.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// This method does not copy the elements of the list. Instead, it creates a wrapper around the
  /// original list. Changes to the underlying list will be reflected in the read-only collection.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) {
    ArgumentNullException.ThrowIfNull(list);
    return new(list);
  }

  /// <summary>
  /// Returns a read-only <see cref="ReadOnlyDictionary{TKey, TValue}"/> wrapper for the specified dictionary.
  /// </summary>
  /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
  /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
  /// <param name="dictionary">The dictionary to wrap.</param>
  /// <returns>A read-only wrapper around the specified dictionary.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// This method does not copy the elements of the dictionary. Instead, it creates a wrapper around the
  /// original dictionary. Changes to the underlying dictionary will be reflected in the read-only dictionary.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull {
    ArgumentNullException.ThrowIfNull(dictionary);
    return new(dictionary);
  }
}

#endif
