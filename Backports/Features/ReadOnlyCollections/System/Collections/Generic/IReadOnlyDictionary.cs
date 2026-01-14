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

#if !SUPPORTS_READ_ONLY_COLLECTIONS

using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

/// <summary>
/// Represents a generic read-only collection of key/value pairs.
/// </summary>
/// <typeparam name="TKey">The type of keys in the read-only dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the read-only dictionary.</typeparam>
public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> {
  /// <summary>
  /// Gets the element that has the specified key in the read-only dictionary.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <returns>The element that has the specified key in the read-only dictionary.</returns>
  TValue this[TKey key] { get; }

  /// <summary>
  /// Gets an enumerable collection that contains the keys in the read-only dictionary.
  /// </summary>
  IEnumerable<TKey> Keys { get; }

  /// <summary>
  /// Gets an enumerable collection that contains the values in the read-only dictionary.
  /// </summary>
  IEnumerable<TValue> Values { get; }

  /// <summary>
  /// Determines whether the read-only dictionary contains an element that has the specified key.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <returns><see langword="true"/> if the read-only dictionary contains an element with the key; otherwise, <see langword="false"/>.</returns>
  bool ContainsKey(TKey key);

  /// <summary>
  /// Gets the value that is associated with the specified key.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
  /// <returns><see langword="true"/> if the object that implements the <see cref="IReadOnlyDictionary{TKey,TValue}"/> interface contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
  bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);
}

#endif
