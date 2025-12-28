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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

#if !SUPPORTS_FROZEN_COLLECTIONS

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Frozen;

/// <summary>
/// Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public class FrozenDictionary<TKey, TValue> :
  IReadOnlyDictionary<TKey, TValue>,
  IDictionary<TKey, TValue>,
  IDictionary,
  ICollection
  where TKey : notnull {
  private readonly Dictionary<TKey, TValue> _dictionary;

  /// <summary>
  /// Gets an empty <see cref="FrozenDictionary{TKey, TValue}"/>.
  /// </summary>
  public static FrozenDictionary<TKey, TValue> Empty { get; } = new(new(), EqualityComparer<TKey>.Default);

  internal FrozenDictionary(Dictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
    this._dictionary = dictionary;
    this.Comparer = comparer;
  }

  /// <summary>
  /// Gets the equality comparer used to determine equality of keys.
  /// </summary>
  public IEqualityComparer<TKey> Comparer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
  }

  /// <summary>
  /// Gets the number of key/value pairs in the dictionary.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._dictionary.Count;
  }

  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  public TValue this[TKey key] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._dictionary[key];
  }

  /// <summary>
  /// Gets a collection containing the keys in the dictionary.
  /// </summary>
  public ImmutableKeyCollection Keys => field ??= new(this._dictionary.Keys);

  /// <summary>
  /// Gets a collection containing the values in the dictionary.
  /// </summary>
  public ImmutableValueCollection Values => field ??= new(this._dictionary.Values);

  IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;
  IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;
  ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;
  ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;
  ICollection IDictionary.Keys => this.Keys;
  ICollection IDictionary.Values => this.Values;

  /// <summary>
  /// Determines whether the dictionary contains the specified key.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

  /// <summary>
  /// Copies the elements of the dictionary to an array.
  /// </summary>
  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
    ArgumentNullException.ThrowIfNull(array);
    ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
  }

  /// <summary>
  /// Copies the elements of the dictionary to a span.
  /// </summary>
  public void CopyTo(Span<KeyValuePair<TKey, TValue>> destination) {
    if (destination.Length < this._dictionary.Count)
      throw new ArgumentException("Destination span is too short.");

    var i = 0;
    foreach (var kvp in this._dictionary)
      destination[i++] = kvp;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the dictionary.
  /// </summary>
  public Enumerator GetEnumerator() => new(this._dictionary);

  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this._dictionary.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();
  IDictionaryEnumerator IDictionary.GetEnumerator() => this._dictionary.GetEnumerator();

  #region Explicit interface implementations (read-only throws)

  TValue IDictionary<TKey, TValue>.this[TKey key] {
    get => this[key];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  object? IDictionary.this[object key] {
    get => key is TKey typedKey ? this._dictionary.TryGetValue(typedKey, out var value) ? value : null : null;
    set => throw new NotSupportedException("Collection is read-only.");
  }

  bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;
  bool IDictionary.IsFixedSize => true;
  bool IDictionary.IsReadOnly => true;
  bool ICollection.IsSynchronized => false;
  object ICollection.SyncRoot => ((ICollection)this._dictionary).SyncRoot;

  void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException("Collection is read-only.");
  void IDictionary.Add(object key, object? value) => throw new NotSupportedException("Collection is read-only.");
  bool IDictionary<TKey, TValue>.Remove(TKey key) => throw new NotSupportedException("Collection is read-only.");
  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException("Collection is read-only.");
  void IDictionary.Remove(object key) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new NotSupportedException("Collection is read-only.");
  void IDictionary.Clear() => throw new NotSupportedException("Collection is read-only.");

  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    => this._dictionary.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

  bool IDictionary.Contains(object key) => key is TKey typedKey && this._dictionary.ContainsKey(typedKey);

  void ICollection.CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

  #endregion

  #region Nested Types

  /// <summary>
  /// Enumerates the elements of a <see cref="FrozenDictionary{TKey, TValue}"/>.
  /// </summary>
  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    private Dictionary<TKey, TValue>.Enumerator _enumerator;

    internal Enumerator(Dictionary<TKey, TValue> dictionary) => this._enumerator = dictionary.GetEnumerator();

    /// <inheritdoc/>
    public readonly KeyValuePair<TKey, TValue> Current => this._enumerator.Current;
    readonly object IEnumerator.Current => this.Current;

    /// <inheritdoc/>
    public bool MoveNext() => this._enumerator.MoveNext();

    /// <inheritdoc/>
    public void Reset() => ((IEnumerator)this._enumerator).Reset();

    /// <inheritdoc/>
    public void Dispose() => this._enumerator.Dispose();
  }

  /// <summary>
  /// Represents an immutable collection of keys.
  /// </summary>
  public sealed class ImmutableKeyCollection : IReadOnlyCollection<TKey>, ICollection<TKey>, ICollection {
    private readonly Dictionary<TKey, TValue>.KeyCollection _keys;

    internal ImmutableKeyCollection(Dictionary<TKey, TValue>.KeyCollection keys) => this._keys = keys;

    public int Count => this._keys.Count;
    bool ICollection<TKey>.IsReadOnly => true;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)this._keys).SyncRoot;

    public bool Contains(TKey item) => ((ICollection<TKey>)this._keys).Contains(item);
    public void CopyTo(TKey[] array, int arrayIndex) => this._keys.CopyTo(array, arrayIndex);
    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._keys).CopyTo(array, index);

    public IEnumerator<TKey> GetEnumerator() => this._keys.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this._keys.GetEnumerator();

    void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException("Collection is read-only.");
    bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException("Collection is read-only.");
    void ICollection<TKey>.Clear() => throw new NotSupportedException("Collection is read-only.");
  }

  /// <summary>
  /// Represents an immutable collection of values.
  /// </summary>
  public sealed class ImmutableValueCollection : IReadOnlyCollection<TValue>, ICollection<TValue>, ICollection {
    private readonly Dictionary<TKey, TValue>.ValueCollection _values;

    internal ImmutableValueCollection(Dictionary<TKey, TValue>.ValueCollection values) => this._values = values;

    public int Count => this._values.Count;
    bool ICollection<TValue>.IsReadOnly => true;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)this._values).SyncRoot;

    public bool Contains(TValue item) => ((ICollection<TValue>)this._values).Contains(item);
    public void CopyTo(TValue[] array, int arrayIndex) => this._values.CopyTo(array, arrayIndex);
    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._values).CopyTo(array, index);

    public IEnumerator<TValue> GetEnumerator() => this._values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this._values.GetEnumerator();

    void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException("Collection is read-only.");
    bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException("Collection is read-only.");
    void ICollection<TValue>.Clear() => throw new NotSupportedException("Collection is read-only.");
  }

  #endregion
}

/// <summary>
/// Provides a set of initialization methods for instances of the <see cref="FrozenDictionary{TKey, TValue}"/> class.
/// </summary>
public static class FrozenDictionary {
  /// <summary>
  /// Creates a <see cref="FrozenDictionary{TKey, TValue}"/> from the specified collection.
  /// </summary>
  public static FrozenDictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(
    this IEnumerable<KeyValuePair<TKey, TValue>> source,
    IEqualityComparer<TKey>? comparer = null
  ) where TKey : notnull {
    ArgumentNullException.ThrowIfNull(source);

    comparer ??= EqualityComparer<TKey>.Default;
    var dictionary = new Dictionary<TKey, TValue>(comparer);
    foreach (var kvp in source)
      dictionary[kvp.Key] = kvp.Value;

    return dictionary.Count == 0 && comparer == EqualityComparer<TKey>.Default
      ? FrozenDictionary<TKey, TValue>.Empty
      : new(dictionary, comparer);
  }

  extension<TSource>(IEnumerable<TSource> source)
  {
    /// <summary>
    /// Creates a <see cref="FrozenDictionary{TKey, TValue}"/> from the specified collection using the provided key selector.
    /// </summary>
    public FrozenDictionary<TKey, TSource> ToFrozenDictionary<TKey>(
      Func<TSource, TKey> keySelector,
      IEqualityComparer<TKey>? comparer = null
    ) where TKey : notnull {
      ArgumentNullException.ThrowIfNull(source);
      ArgumentNullException.ThrowIfNull(keySelector);

      comparer ??= EqualityComparer<TKey>.Default;
      var dictionary = new Dictionary<TKey, TSource>(comparer);
      foreach (var item in source)
        dictionary[keySelector(item)] = item;

      return dictionary.Count == 0 && comparer == EqualityComparer<TKey>.Default
        ? FrozenDictionary<TKey, TSource>.Empty
        : new(dictionary, comparer);
    }

    /// <summary>
    /// Creates a <see cref="FrozenDictionary{TKey, TValue}"/> from the specified collection using the provided key and element selectors.
    /// </summary>
    public FrozenDictionary<TKey, TElement> ToFrozenDictionary<TKey, TElement>(
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      IEqualityComparer<TKey>? comparer = null
    ) where TKey : notnull {
      ArgumentNullException.ThrowIfNull(source);
      ArgumentNullException.ThrowIfNull(keySelector);
      ArgumentNullException.ThrowIfNull(elementSelector);

      comparer ??= EqualityComparer<TKey>.Default;
      var dictionary = new Dictionary<TKey, TElement>(comparer);
      foreach (var item in source)
        dictionary[keySelector(item)] = elementSelector(item);

      return dictionary.Count == 0 && comparer == EqualityComparer<TKey>.Default
        ? FrozenDictionary<TKey, TElement>.Empty
        : new(dictionary, comparer);
    }
  }
}

#endif
