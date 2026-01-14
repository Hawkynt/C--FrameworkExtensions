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
// See LICENSE file for more details.

#endregion

#if !OFFICIAL_IMMUTABLE_COLLECTIONS

using System.Collections.Generic;

namespace System.Collections.Immutable;

#region interfaces

/// <summary>
/// Represents an immutable collection of key/value pairs.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public interface IImmutableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {
  /// <summary>
  /// Adds an element with the specified key and value to the dictionary.
  /// </summary>
  IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value);

  /// <summary>
  /// Adds the specified key/value pairs to the dictionary.
  /// </summary>
  IImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs);

  /// <summary>
  /// Retrieves an empty dictionary that has the same ordering and key/value comparison rules as this dictionary instance.
  /// </summary>
  IImmutableDictionary<TKey, TValue> Clear();

  /// <summary>
  /// Determines whether the dictionary contains an element with the specified key.
  /// </summary>
  bool Contains(KeyValuePair<TKey, TValue> pair);

  /// <summary>
  /// Removes the element with the specified key from the dictionary.
  /// </summary>
  IImmutableDictionary<TKey, TValue> Remove(TKey key);

  /// <summary>
  /// Removes the elements with the specified keys from the dictionary.
  /// </summary>
  IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys);

  /// <summary>
  /// Sets the specified key and value in the dictionary, possibly overwriting an existing value for the key.
  /// </summary>
  IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value);

  /// <summary>
  /// Sets the specified key/value pairs in the dictionary, possibly overwriting existing values for the keys.
  /// </summary>
  IImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items);

  /// <summary>
  /// Gets the value comparer used to determine value equality.
  /// </summary>
  IEqualityComparer<TValue> ValueComparer { get; }

  /// <summary>
  /// Gets the key comparer used to determine key equality.
  /// </summary>
  IEqualityComparer<TKey> KeyComparer { get; }
}

#endregion

/// <summary>
/// Represents an immutable, unordered collection of keys and values.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public sealed class ImmutableDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IDictionary<TKey, TValue>, IDictionary {
  private readonly Dictionary<TKey, TValue> _dictionary;

  /// <summary>
  /// Gets an empty immutable dictionary.
  /// </summary>
  public static readonly ImmutableDictionary<TKey, TValue> Empty = new(new(), EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);

  internal ImmutableDictionary(Dictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) {
    this._dictionary = dictionary;
    this.KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    this.ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
  }

  /// <inheritdoc />
  public IEqualityComparer<TKey> KeyComparer { get; }

  /// <inheritdoc />
  public IEqualityComparer<TValue> ValueComparer { get; }

  /// <inheritdoc />
  public TValue this[TKey key] => this._dictionary[key];

  /// <inheritdoc />
  public IEnumerable<TKey> Keys => this._dictionary.Keys;

  /// <inheritdoc />
  public IEnumerable<TValue> Values => this._dictionary.Values;

  /// <inheritdoc />
  public int Count => this._dictionary.Count;

  /// <summary>
  /// Gets a value indicating whether this dictionary is empty.
  /// </summary>
  public bool IsEmpty => this._dictionary.Count == 0;

  /// <inheritdoc />
  public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

  /// <inheritdoc />
  public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

  /// <summary>
  /// Gets the value for a given key if a matching key exists in the dictionary.
  /// </summary>
  public TValue? GetValueOrDefault(TKey key) => this._dictionary.TryGetValue(key, out var value) ? value : default;

  /// <summary>
  /// Gets the value for a given key if a matching key exists in the dictionary.
  /// </summary>
  public TValue GetValueOrDefault(TKey key, TValue defaultValue) => this._dictionary.TryGetValue(key, out var value) ? value : defaultValue;

  /// <inheritdoc />
  public bool Contains(KeyValuePair<TKey, TValue> pair) =>
    this._dictionary.TryGetValue(pair.Key, out var value) && this.ValueComparer.Equals(value, pair.Value);

  /// <summary>
  /// Adds an element with the specified key and value to the dictionary.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value) {
    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer) { { key, value } };
    return new(newDict, this.KeyComparer, this.ValueComparer);
  }

  /// <summary>
  /// Adds the specified key/value pairs to the dictionary.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) {
    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer);
    foreach (var pair in pairs)
      newDict.Add(pair.Key, pair.Value);
    return new(newDict, this.KeyComparer, this.ValueComparer);
  }

  /// <summary>
  /// Sets the specified key and value in the dictionary, possibly overwriting an existing value for the key.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value) {
    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer) { [key] = value };
    return new(newDict, this.KeyComparer, this.ValueComparer);
  }

  /// <summary>
  /// Sets the specified key/value pairs in the dictionary, possibly overwriting existing values for the keys.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) {
    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer);
    foreach (var item in items)
      newDict[item.Key] = item.Value;
    return new(newDict, this.KeyComparer, this.ValueComparer);
  }

  /// <summary>
  /// Removes the element with the specified key from the dictionary.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> Remove(TKey key) {
    if (!this._dictionary.ContainsKey(key))
      return this;

    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer);
    newDict.Remove(key);
    return new(newDict, this.KeyComparer, this.ValueComparer);
  }

  /// <summary>
  /// Removes the elements with the specified keys from the dictionary.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys) {
    var newDict = new Dictionary<TKey, TValue>(this._dictionary, this.KeyComparer);
    var anyRemoved = false;
    foreach (var key in keys)
      if (newDict.Remove(key))
        anyRemoved = true;

    return anyRemoved ? new(newDict, this.KeyComparer, this.ValueComparer) : this;
  }

  /// <summary>
  /// Retrieves an empty dictionary that has the same ordering and key/value comparison rules as this dictionary instance.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> Clear() =>
    this._dictionary.Count == 0 ? this : new(new(this.KeyComparer), this.KeyComparer, this.ValueComparer);

  #region IImmutableDictionary explicit implementation

  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value) => this.Add(key, value);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) => this.AddRange(pairs);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value) => this.SetItem(key, value);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) => this.SetItems(items);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key) => this.Remove(key);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys) => this.RemoveRange(keys);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear() => this.Clear();

  #endregion

  /// <summary>
  /// Creates a new dictionary with the specified key comparer.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer) =>
    this.WithComparers(keyComparer, this.ValueComparer);

  /// <summary>
  /// Creates a new dictionary with the specified key and value comparers.
  /// </summary>
  public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) {
    keyComparer ??= EqualityComparer<TKey>.Default;
    valueComparer ??= EqualityComparer<TValue>.Default;
    if (keyComparer == this.KeyComparer && valueComparer == this.ValueComparer)
      return this;

    var newDict = new Dictionary<TKey, TValue>(keyComparer);
    foreach (var kvp in this._dictionary)
      newDict.Add(kvp.Key, kvp.Value);
    return new(newDict, keyComparer, valueComparer);
  }

  /// <summary>
  /// Creates a mutable builder for this dictionary.
  /// </summary>
  public Builder ToBuilder() => new(new(this._dictionary, this.KeyComparer), this.KeyComparer, this.ValueComparer);

  /// <summary>
  /// Returns an enumerator that iterates through the dictionary.
  /// </summary>
  public Enumerator GetEnumerator() => new(this._dictionary.GetEnumerator());

  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this._dictionary.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();

  #region IDictionary<TKey, TValue> explicit implementation

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

  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => this.Contains(item);

  void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);

  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException("Collection is read-only.");

  #endregion

  #region IDictionary explicit implementation

  object? IDictionary.this[object key] {
    get => this._dictionary[(TKey)key];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  bool IDictionary.IsFixedSize => true;

  bool IDictionary.IsReadOnly => true;

  ICollection IDictionary.Keys => this._dictionary.Keys;

  ICollection IDictionary.Values => this._dictionary.Values;

  bool ICollection.IsSynchronized => false;

  object ICollection.SyncRoot => this;

  void IDictionary.Add(object key, object value) => throw new NotSupportedException("Collection is read-only.");

  void IDictionary.Clear() => throw new NotSupportedException("Collection is read-only.");

  bool IDictionary.Contains(object key) => key is TKey k && this._dictionary.ContainsKey(k);

  IDictionaryEnumerator IDictionary.GetEnumerator() => this._dictionary.GetEnumerator();

  void IDictionary.Remove(object key) => throw new NotSupportedException("Collection is read-only.");

  void ICollection.CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

  #endregion

  /// <summary>
  /// Enumerates the contents of the immutable dictionary.
  /// </summary>
  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    private Dictionary<TKey, TValue>.Enumerator _enumerator;

    internal Enumerator(Dictionary<TKey, TValue>.Enumerator enumerator) => this._enumerator = enumerator;

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue> Current => this._enumerator.Current;

    object IEnumerator.Current => this._enumerator.Current;

    /// <inheritdoc />
    public bool MoveNext() => this._enumerator.MoveNext();

    /// <inheritdoc />
    public void Reset() => ((IEnumerator)this._enumerator).Reset();

    /// <inheritdoc />
    public void Dispose() => this._enumerator.Dispose();
  }

  /// <summary>
  /// A builder for creating immutable dictionaries efficiently.
  /// </summary>
  public sealed class Builder : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary {
    private Dictionary<TKey, TValue> _dictionary;

    internal Builder(Dictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) {
      this._dictionary = dictionary;
      this.KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
      this.ValueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
    }

    /// <summary>
    /// Gets or sets the key comparer.
    /// </summary>
    public IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Gets or sets the value comparer.
    /// </summary>
    public IEqualityComparer<TValue> ValueComparer { get; }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    public TValue this[TKey key] {
      get => this._dictionary[key];
      set => this._dictionary[key] = value;
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => this._dictionary.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => this._dictionary.Values;

    /// <inheritdoc />
    public int Count => this._dictionary.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this._dictionary.Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this._dictionary.Values;

    /// <inheritdoc />
    public void Add(TKey key, TValue value) => this._dictionary.Add(key, value);

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item) => this._dictionary.Add(item.Key, item.Value);

    /// <summary>
    /// Adds a range of key/value pairs to the dictionary.
    /// </summary>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items) {
      foreach (var item in items)
        this._dictionary.Add(item.Key, item.Value);
    }

    /// <inheritdoc />
    public void Clear() => this._dictionary.Clear();

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item) =>
      this._dictionary.TryGetValue(item.Key, out var value) && this.ValueComparer.Equals(value, item.Value);

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

    /// <summary>
    /// Determines whether the dictionary contains a specific value.
    /// </summary>
    public bool ContainsValue(TValue value) => this._dictionary.ContainsValue(value);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);

    /// <summary>
    /// Gets the value for a given key if a matching key exists in the dictionary.
    /// </summary>
    public TValue? GetValueOrDefault(TKey key) => this._dictionary.TryGetValue(key, out var value) ? value : default;

    /// <summary>
    /// Gets the value for a given key if a matching key exists in the dictionary.
    /// </summary>
    public TValue GetValueOrDefault(TKey key, TValue defaultValue) => this._dictionary.TryGetValue(key, out var value) ? value : defaultValue;

    /// <inheritdoc />
    public bool Remove(TKey key) => this._dictionary.Remove(key);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item) {
      if (!this.Contains(item))
        return false;
      return this._dictionary.Remove(item.Key);
    }

    /// <summary>
    /// Removes any entries with keys that match those found in the specified sequence.
    /// </summary>
    public void RemoveRange(IEnumerable<TKey> keys) {
      foreach (var key in keys)
        this._dictionary.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

    /// <summary>
    /// Gets the value for a given key, or adds a new value if the key does not exist.
    /// </summary>
    public TValue GetOrAdd(TKey key, TValue value) {
      if (this._dictionary.TryGetValue(key, out var existingValue))
        return existingValue;
      this._dictionary.Add(key, value);
      return value;
    }

    /// <summary>
    /// Gets the value for a given key, or adds a new value using the specified factory if the key does not exist.
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
      if (this._dictionary.TryGetValue(key, out var existingValue))
        return existingValue;
      var value = valueFactory(key);
      this._dictionary.Add(key, value);
      return value;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();

    /// <summary>
    /// Creates an immutable dictionary based on the contents of this builder.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutable() =>
      new(new(this._dictionary, this.KeyComparer), this.KeyComparer, this.ValueComparer);

    #region IDictionary explicit implementation

    object? IDictionary.this[object key] {
      get => this._dictionary[(TKey)key];
      set => this._dictionary[(TKey)key] = (TValue)value!;
    }

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    ICollection IDictionary.Keys => this._dictionary.Keys;

    ICollection IDictionary.Values => this._dictionary.Values;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    void IDictionary.Add(object key, object value) => this._dictionary.Add((TKey)key, (TValue)value);

    bool IDictionary.Contains(object key) => key is TKey k && this._dictionary.ContainsKey(k);

    IDictionaryEnumerator IDictionary.GetEnumerator() => this._dictionary.GetEnumerator();

    void IDictionary.Remove(object key) => this._dictionary.Remove((TKey)key);

    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._dictionary).CopyTo(array, index);

    #endregion
  }
}

/// <summary>
/// Provides a set of static methods for creating immutable dictionaries.
/// </summary>
public static class ImmutableDictionary {
  /// <summary>
  /// Creates an empty immutable dictionary.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>() =>
    ImmutableDictionary<TKey, TValue>.Empty;

  /// <summary>
  /// Creates an empty immutable dictionary with the specified key comparer.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer) =>
    new(new(keyComparer), keyComparer, EqualityComparer<TValue>.Default);

  /// <summary>
  /// Creates an empty immutable dictionary with the specified key and value comparers.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) =>
    new(new(keyComparer), keyComparer, valueComparer);

  /// <summary>
  /// Creates a new builder for creating immutable dictionaries.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>() =>
    new(new(), EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);

  /// <summary>
  /// Creates a new builder for creating immutable dictionaries with the specified key comparer.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer) =>
    new(new(keyComparer), keyComparer, EqualityComparer<TValue>.Default);

  /// <summary>
  /// Creates a new builder for creating immutable dictionaries with the specified key and value comparers.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) =>
    new(new(keyComparer), keyComparer, valueComparer);

  /// <summary>
  /// Creates an immutable dictionary from the specified items.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items) {
    var dict = new Dictionary<TKey, TValue>();
    foreach (var item in items)
      dict.Add(item.Key, item.Value);
    return new(dict, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
  }

  /// <summary>
  /// Creates an immutable dictionary from the specified items with the specified key comparer.
  /// </summary>
  public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) {
    var dict = new Dictionary<TKey, TValue>(keyComparer);
    foreach (var item in items)
      dict.Add(item.Key, item.Value);
    return new(dict, keyComparer, EqualityComparer<TValue>.Default);
  }

  extension<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source)
  {
    /// <summary>
    /// Enumerates a sequence and produces an immutable dictionary of its contents.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary() =>
      CreateRange(source);

    /// <summary>
    /// Enumerates a sequence and produces an immutable dictionary of its contents with the specified key comparer.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary(IEqualityComparer<TKey> keyComparer) =>
      CreateRange(keyComparer, source);
  }

  extension<TSource>(IEnumerable<TSource> source)
  {
    /// <summary>
    /// Enumerates and transforms a sequence and produces an immutable dictionary of its contents.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) {
      var dict = new Dictionary<TKey, TValue>();
      foreach (var item in source)
        dict.Add(keySelector(item), valueSelector(item));
      return new(dict, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
    }

    /// <summary>
    /// Enumerates and transforms a sequence and produces an immutable dictionary of its contents with the specified key comparer.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> keyComparer) {
      var dict = new Dictionary<TKey, TValue>(keyComparer);
      foreach (var item in source)
        dict.Add(keySelector(item), valueSelector(item));
      return new(dict, keyComparer, EqualityComparer<TValue>.Default);
    }

    /// <summary>
    /// Enumerates and transforms a sequence and produces an immutable dictionary of its contents with the specified key and value comparers.
    /// </summary>
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer) {
      var dict = new Dictionary<TKey, TValue>(keyComparer);
      foreach (var item in source)
        dict.Add(keySelector(item), valueSelector(item));
      return new(dict, keyComparer, valueComparer);
    }

    /// <summary>
    /// Enumerates a sequence and produces an immutable dictionary of its contents by using the specified key selector function.
    /// </summary>
    public ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TKey>(Func<TSource, TKey> keySelector) {
      var dict = new Dictionary<TKey, TSource>();
      foreach (var item in source)
        dict.Add(keySelector(item), item);
      return new(dict, EqualityComparer<TKey>.Default, EqualityComparer<TSource>.Default);
    }

    /// <summary>
    /// Enumerates a sequence and produces an immutable dictionary of its contents by using the specified key selector function and key comparer.
    /// </summary>
    public ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer) {
      var dict = new Dictionary<TKey, TSource>(keyComparer);
      foreach (var item in source)
        dict.Add(keySelector(item), item);
      return new(dict, keyComparer, EqualityComparer<TSource>.Default);
    }
  }

  /// <summary>
  /// Determines whether the dictionary contains the specified key/value pair.
  /// </summary>
  public static bool Contains<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key, TValue value) =>
    dictionary.Contains(new(key, value));
}

#endif
