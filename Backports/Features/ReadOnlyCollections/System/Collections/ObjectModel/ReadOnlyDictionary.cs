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

using System.Collections.Generic;

namespace System.Collections.ObjectModel;

/// <summary>
/// Represents a read-only, generic collection of key/value pairs.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue> {
  private readonly IDictionary<TKey, TValue> _dictionary;

  /// <summary>
  /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey, TValue}"/> class that is a wrapper around the specified dictionary.
  /// </summary>
  /// <param name="dictionary">The dictionary to wrap.</param>
  /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
  public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
    => this._dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));

  /// <inheritdoc />
  public TValue this[TKey key] => this._dictionary[key];

  /// <inheritdoc />
  public int Count => this._dictionary.Count;

  /// <inheritdoc />
  public IEnumerable<TKey> Keys => this._dictionary.Keys;

  /// <inheritdoc />
  public IEnumerable<TValue> Values => this._dictionary.Values;

  /// <inheritdoc />
  public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

  /// <inheritdoc />
  public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

  /// <inheritdoc />
  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._dictionary.GetEnumerator();

  /// <inheritdoc />
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #region IDictionary<TKey, TValue> explicit implementation (read-only)

  TValue IDictionary<TKey, TValue>.this[TKey key] {
    get => this._dictionary[key];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  ICollection<TKey> IDictionary<TKey, TValue>.Keys => this._dictionary.Keys;

  ICollection<TValue> IDictionary<TKey, TValue>.Values => this._dictionary.Values;

  bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

  void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw new NotSupportedException("Collection is read-only.");

  bool IDictionary<TKey, TValue>.Remove(TKey key) => throw new NotSupportedException("Collection is read-only.");

  void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException("Collection is read-only.");

  void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new NotSupportedException("Collection is read-only.");

  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

  void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => this._dictionary.CopyTo(array, arrayIndex);

  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException("Collection is read-only.");

  #endregion
}

#endif
