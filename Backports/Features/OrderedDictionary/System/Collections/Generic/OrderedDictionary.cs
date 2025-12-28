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

#if !SUPPORTS_ORDERED_DICTIONARY

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

using MethodImplOptions = Utilities.MethodImplOptions;

/// <summary>
/// Represents a generic collection of key/value pairs that are accessible by the key or index.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <remarks>
/// This is a polyfill implementation for .NET 9's OrderedDictionary&lt;TKey, TValue&gt;.
/// It maintains insertion order while providing O(1) key-based lookups.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class OrderedDictionary<TKey, TValue> :
  IDictionary<TKey, TValue>,
  IReadOnlyDictionary<TKey, TValue>,
  IList<KeyValuePair<TKey, TValue>>,
  IReadOnlyList<KeyValuePair<TKey, TValue>>,
  IDictionary,
  IList
  where TKey : notnull {

  private readonly Dictionary<TKey, int> _keyIndexMap;
  private readonly List<KeyValuePair<TKey, TValue>> _list;
  private readonly IEqualityComparer<TKey> _comparer;
  private bool _indexesDirty; // Lazy index rebuilding: defer O(n) updates until read

  /// <summary>
  /// Marks index map as needing rebuild (deferred until next read).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _MarkIndicesDirty() => this._indexesDirty = true;

  /// <summary>
  /// Rebuilds the key-to-index map if it's dirty.
  /// </summary>
  private void _EnsureIndicesValid() {
    if (!this._indexesDirty)
      return;

    for (var i = 0; i < this._list.Count; ++i)
      this._keyIndexMap[this._list[i].Key] = i;

    this._indexesDirty = false;
  }

  #region Constructors

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
  /// </summary>
  public OrderedDictionary()
    : this(0, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class with the specified capacity.
  /// </summary>
  /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
  public OrderedDictionary(int capacity)
    : this(capacity, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class with the specified comparer.
  /// </summary>
  /// <param name="comparer">The equality comparer to use when comparing keys.</param>
  public OrderedDictionary(IEqualityComparer<TKey>? comparer)
    : this(0, comparer) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class with the specified capacity and comparer.
  /// </summary>
  /// <param name="capacity">The initial number of elements that the dictionary can contain.</param>
  /// <param name="comparer">The equality comparer to use when comparing keys.</param>
  public OrderedDictionary(int capacity, IEqualityComparer<TKey>? comparer) {
    this._comparer = comparer ?? EqualityComparer<TKey>.Default;
    this._keyIndexMap = new(capacity, this._comparer);
    this._list = new(capacity);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class from the specified dictionary.
  /// </summary>
  /// <param name="dictionary">The dictionary to copy elements from.</param>
  public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
    : this(dictionary, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class from the specified dictionary.
  /// </summary>
  /// <param name="dictionary">The dictionary to copy elements from.</param>
  /// <param name="comparer">The equality comparer to use when comparing keys.</param>
  public OrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer)
    : this(dictionary?.Count ?? 0, comparer) {
    ArgumentNullException.ThrowIfNull(dictionary);
    foreach (var kvp in dictionary)
      this.Add(kvp.Key, kvp.Value);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class from the specified collection.
  /// </summary>
  /// <param name="collection">The collection to copy elements from.</param>
  public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    : this(collection, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class from the specified collection.
  /// </summary>
  /// <param name="collection">The collection to copy elements from.</param>
  /// <param name="comparer">The equality comparer to use when comparing keys.</param>
  public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
    : this(collection is ICollection<KeyValuePair<TKey, TValue>> c ? c.Count : 0, comparer) {
    ArgumentNullException.ThrowIfNull(collection);
    foreach (var kvp in collection)
      this.Add(kvp.Key, kvp.Value);
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the number of key/value pairs in the dictionary.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._list.Count;
  }

  /// <summary>
  /// Gets or sets the capacity of this dictionary.
  /// </summary>
  public int Capacity {
    get => this._list.Capacity;
    set {
      this._list.Capacity = value;
      this._keyIndexMap.EnsureCapacity(value);
    }
  }

  /// <summary>
  /// Gets the equality comparer used to determine equality of keys.
  /// </summary>
  public IEqualityComparer<TKey> Comparer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._comparer;
  }

  /// <summary>
  /// Gets a collection containing the keys in the dictionary.
  /// </summary>
  public KeyCollection Keys => field ??= new(this);

  /// <summary>
  /// Gets a collection containing the values in the dictionary.
  /// </summary>
  public ValueCollection Values => field ??= new(this);

  ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;
  ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;
  IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;
  IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;
  ICollection IDictionary.Keys => this.Keys;
  ICollection IDictionary.Values => this.Values;

  bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
  bool IDictionary.IsFixedSize => false;
  bool IDictionary.IsReadOnly => false;
  bool ICollection.IsSynchronized => false;
  object ICollection.SyncRoot => ((ICollection)this._list).SyncRoot;
  bool IList.IsFixedSize => false;
  bool IList.IsReadOnly => false;

  #endregion

  #region Indexers

  /// <summary>
  /// Gets or sets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get or set.</param>
  /// <returns>The value associated with the specified key.</returns>
  public TValue this[TKey key] {
    get {
      this._EnsureIndicesValid();
      return this._list[this._keyIndexMap[key]].Value;
    }
    set {
      this._EnsureIndicesValid();
      if (this._keyIndexMap.TryGetValue(key, out var index))
        this._list[index] = new(key, value);
      else
        this.Add(key, value);
    }
  }

  /// <summary>
  /// Gets the key/value pair at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to get.</param>
  /// <returns>The key/value pair at the specified index.</returns>
  KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index] {
    get => this._list[index];
    set {
      var oldKey = this._list[index].Key;
      if (!this._comparer.Equals(oldKey, value.Key)) {
        if (this._keyIndexMap.ContainsKey(value.Key))
          throw new ArgumentException("An element with the same key already exists.");
        this._keyIndexMap.Remove(oldKey);
        this._keyIndexMap[value.Key] = index;
      }
      this._list[index] = value;
    }
  }

  /// <summary>
  /// Gets the key/value pair at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to get.</param>
  /// <returns>The key/value pair at the specified index.</returns>
  KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => this._list[index];

  object? IDictionary.this[object key] {
    get {
      this._EnsureIndicesValid();
      return key is TKey typedKey && this._keyIndexMap.TryGetValue(typedKey, out var index) ? this._list[index].Value : null;
    }
    set {
      if (key is not TKey typedKey)
        throw new ArgumentException("Key is of the wrong type.", nameof(key));
      if (value is not TValue typedValue)
        throw new ArgumentException("Value is of the wrong type.", nameof(value));
      this[typedKey] = typedValue;
    }
  }

  object? IList.this[int index] {
    get => this._list[index];
    set {
      if (value is not KeyValuePair<TKey, TValue> kvp)
        throw new ArgumentException("Value is of the wrong type.", nameof(value));
      ((IList<KeyValuePair<TKey, TValue>>)this)[index] = kvp;
    }
  }

  #endregion

  #region Core Dictionary Operations

  /// <summary>
  /// Adds the specified key and value to the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="value">The value of the element to add.</param>
  public void Add(TKey key, TValue value) {
    this._keyIndexMap.Add(key, this._list.Count);
    this._list.Add(new(key, value));
  }

  /// <summary>
  /// Attempts to add the specified key and value to the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="value">The value of the element to add.</param>
  /// <returns><see langword="true"/> if the key/value pair was added; <see langword="false"/> if the key already exists.</returns>
  public bool TryAdd(TKey key, TValue value) {
    if (this._keyIndexMap.ContainsKey(key))
      return false;
    this.Add(key, value);
    return true;
  }

  /// <summary>
  /// Determines whether the dictionary contains the specified key.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsKey(TKey key) => this._keyIndexMap.ContainsKey(key);

  /// <summary>
  /// Determines whether the dictionary contains the specified value.
  /// </summary>
  /// <param name="value">The value to locate.</param>
  /// <returns><see langword="true"/> if the dictionary contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
  public bool ContainsValue(TValue value) {
    var comparer = EqualityComparer<TValue>.Default;
    for (var i = 0; i < this._list.Count; ++i)
      if (comparer.Equals(this._list[i].Value, value))
        return true;
    return false;
  }

  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get.</param>
  /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value.</param>
  /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
  public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
    this._EnsureIndicesValid();
    if (this._keyIndexMap.TryGetValue(key, out var index)) {
      value = this._list[index].Value;
      return true;
    }
    value = default;
    return false;
  }

  /// <summary>
  /// Removes the value with the specified key from the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to remove.</param>
  /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
  public bool Remove(TKey key) => this.Remove(key, out _);

  /// <summary>
  /// Removes the value with the specified key from the dictionary and returns the removed value.
  /// </summary>
  /// <param name="key">The key of the element to remove.</param>
  /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value.</param>
  /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
  public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value) {
    this._EnsureIndicesValid();
    if (!this._keyIndexMap.TryGetValue(key, out var index)) {
      value = default;
      return false;
    }

    value = this._list[index].Value;
    this._RemoveAtIndex(index);
    return true;
  }

  /// <summary>
  /// Removes all keys and values from the dictionary.
  /// </summary>
  public void Clear() {
    this._keyIndexMap.Clear();
    this._list.Clear();
  }

  #endregion

  #region Index-Based Operations

  /// <summary>
  /// Gets the index of the specified key.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <returns>The index of the key if found; otherwise, -1.</returns>
  public int IndexOf(TKey key) {
    this._EnsureIndicesValid();
    return this._keyIndexMap.TryGetValue(key, out var index) ? index : -1;
  }

  /// <summary>
  /// Gets the key/value pair at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to get.</param>
  /// <returns>The key/value pair at the specified index.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public KeyValuePair<TKey, TValue> GetAt(int index) => this._list[index];

  /// <summary>
  /// Sets the value at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to set.</param>
  /// <param name="value">The new value.</param>
  public void SetAt(int index, TValue value) {
    var kvp = this._list[index];
    this._list[index] = new(kvp.Key, value);
  }

  /// <summary>
  /// Inserts an element at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index at which the element should be inserted.</param>
  /// <param name="key">The key of the element to insert.</param>
  /// <param name="value">The value of the element to insert.</param>
  public void Insert(int index, TKey key, TValue value) {
    if (this._keyIndexMap.ContainsKey(key))
      throw new ArgumentException("An element with the same key already exists.", nameof(key));

    this._list.Insert(index, new(key, value));
    this._keyIndexMap[key] = index;

    // Defer index updates until next read - O(1) instead of O(n)
    this._MarkIndicesDirty();
  }

  /// <summary>
  /// Removes the element at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the element to remove.</param>
  public void RemoveAt(int index) => this._RemoveAtIndex(index);

  private void _RemoveAtIndex(int index) {
    var key = this._list[index].Key;
    this._keyIndexMap.Remove(key);
    this._list.RemoveAt(index);

    // Defer index updates until next read - O(1) instead of O(n)
    this._MarkIndicesDirty();
  }

  #endregion

  #region Enumeration

  /// <summary>
  /// Returns an enumerator that iterates through the dictionary.
  /// </summary>
  public Enumerator GetEnumerator() => new(this._list);

  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this._list.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => this._list.GetEnumerator();
  IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(this._list);

  #endregion

  #region Explicit Interface Implementations

  void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => this.Add(item.Key, item.Value);

  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
    this._EnsureIndicesValid();
    return this._keyIndexMap.TryGetValue(item.Key, out var index) &&
           EqualityComparer<TValue>.Default.Equals(this._list[index].Value, item.Value);
  }

  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
    this._EnsureIndicesValid();
    if (!this._keyIndexMap.TryGetValue(item.Key, out var index))
      return false;
    if (!EqualityComparer<TValue>.Default.Equals(this._list[index].Value, item.Value))
      return false;
    this._RemoveAtIndex(index);
    return true;
  }

  void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    => this._list.CopyTo(array, arrayIndex);

  int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item) {
    this._EnsureIndicesValid();
    if (!this._keyIndexMap.TryGetValue(item.Key, out var index))
      return -1;
    return EqualityComparer<TValue>.Default.Equals(this._list[index].Value, item.Value) ? index : -1;
  }

  void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
    => this.Insert(index, item.Key, item.Value);

  void IDictionary.Add(object key, object? value) {
    if (key is not TKey typedKey)
      throw new ArgumentException("Key is of the wrong type.", nameof(key));
    if (value is not TValue typedValue)
      throw new ArgumentException("Value is of the wrong type.", nameof(value));
    this.Add(typedKey, typedValue);
  }

  bool IDictionary.Contains(object key) => key is TKey typedKey && this._keyIndexMap.ContainsKey(typedKey);

  void IDictionary.Remove(object key) {
    if (key is TKey typedKey)
      this.Remove(typedKey);
  }

  void ICollection.CopyTo(Array array, int index) => ((ICollection)this._list).CopyTo(array, index);

  int IList.Add(object? value) {
    if (value is not KeyValuePair<TKey, TValue> kvp)
      throw new ArgumentException("Value is of the wrong type.", nameof(value));
    this.Add(kvp.Key, kvp.Value);
    return this._list.Count - 1;
  }

  bool IList.Contains(object? value)
    => value is KeyValuePair<TKey, TValue> kvp && ((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(kvp);

  int IList.IndexOf(object? value)
    => value is KeyValuePair<TKey, TValue> kvp ? ((IList<KeyValuePair<TKey, TValue>>)this).IndexOf(kvp) : -1;

  void IList.Insert(int index, object? value) {
    if (value is not KeyValuePair<TKey, TValue> kvp)
      throw new ArgumentException("Value is of the wrong type.", nameof(value));
    this.Insert(index, kvp.Key, kvp.Value);
  }

  void IList.Remove(object? value) {
    if (value is KeyValuePair<TKey, TValue> kvp)
      ((ICollection<KeyValuePair<TKey, TValue>>)this).Remove(kvp);
  }

  #endregion

  #region Nested Types

  /// <summary>
  /// Enumerates the elements of an <see cref="OrderedDictionary{TKey, TValue}"/>.
  /// </summary>
  public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
    private List<KeyValuePair<TKey, TValue>>.Enumerator _enumerator;

    internal Enumerator(List<KeyValuePair<TKey, TValue>> list) => this._enumerator = list.GetEnumerator();

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

  private sealed class DictionaryEnumerator : IDictionaryEnumerator {
    private readonly List<KeyValuePair<TKey, TValue>> _list;
    private int _index = -1;

    internal DictionaryEnumerator(List<KeyValuePair<TKey, TValue>> list) => this._list = list;

    public DictionaryEntry Entry => new(this._list[this._index].Key, this._list[this._index].Value);
    public object Key => this._list[this._index].Key;
    public object? Value => this._list[this._index].Value;
    public object Current => this.Entry;

    public bool MoveNext() => ++this._index < this._list.Count;
    public void Reset() => this._index = -1;
  }

  /// <summary>
  /// Represents the collection of keys in an <see cref="OrderedDictionary{TKey, TValue}"/>.
  /// </summary>
  public sealed class KeyCollection : IList<TKey>, IReadOnlyList<TKey>, ICollection {
    private readonly OrderedDictionary<TKey, TValue> _dictionary;

    internal KeyCollection(OrderedDictionary<TKey, TValue> dictionary) => this._dictionary = dictionary;

    /// <summary>
    /// Gets the number of keys in the collection.
    /// </summary>
    public int Count => this._dictionary.Count;

    /// <summary>
    /// Gets the key at the specified index.
    /// </summary>
    public TKey this[int index] => this._dictionary._list[index].Key;

    TKey IList<TKey>.this[int index] {
      get => this[index];
      set => throw new NotSupportedException("Collection is read-only.");
    }

    bool ICollection<TKey>.IsReadOnly => true;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)this._dictionary._list).SyncRoot;

    /// <summary>
    /// Determines whether the collection contains the specified key.
    /// </summary>
    public bool Contains(TKey item) => this._dictionary._keyIndexMap.ContainsKey(item);

    /// <summary>
    /// Gets the index of the specified key.
    /// </summary>
    public int IndexOf(TKey item) => this._dictionary.IndexOf(item);

    /// <summary>
    /// Copies the keys to an array.
    /// </summary>
    public void CopyTo(TKey[] array, int arrayIndex) {
      ArgumentNullException.ThrowIfNull(array);
      if (arrayIndex < 0 || arrayIndex > array.Length)
        throw new ArgumentOutOfRangeException(nameof(arrayIndex));
      if (array.Length - arrayIndex < this._dictionary.Count)
        throw new ArgumentException("Destination array is not long enough.");

      for (var i = 0; i < this._dictionary._list.Count; ++i)
        array[arrayIndex + i] = this._dictionary._list[i].Key;
    }

    void ICollection.CopyTo(Array array, int index) {
      ArgumentNullException.ThrowIfNull(array);
      if (array.Rank != 1)
        throw new ArgumentException("Array must be single-dimensional.");
      if (index < 0 || index > array.Length)
        throw new ArgumentOutOfRangeException(nameof(index));
      if (array.Length - index < this._dictionary.Count)
        throw new ArgumentException("Destination array is not long enough.");

      for (var i = 0; i < this._dictionary._list.Count; ++i)
        array.SetValue(this._dictionary._list[i].Key, index + i);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the keys.
    /// </summary>
    public IEnumerator<TKey> GetEnumerator() {
      foreach (var kvp in this._dictionary._list)
        yield return kvp.Key;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException("Collection is read-only.");
    bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException("Collection is read-only.");
    void ICollection<TKey>.Clear() => throw new NotSupportedException("Collection is read-only.");
    void IList<TKey>.Insert(int index, TKey item) => throw new NotSupportedException("Collection is read-only.");
    void IList<TKey>.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");
  }

  /// <summary>
  /// Represents the collection of values in an <see cref="OrderedDictionary{TKey, TValue}"/>.
  /// </summary>
  public sealed class ValueCollection : IList<TValue>, IReadOnlyList<TValue>, ICollection {
    private readonly OrderedDictionary<TKey, TValue> _dictionary;

    internal ValueCollection(OrderedDictionary<TKey, TValue> dictionary) => this._dictionary = dictionary;

    /// <summary>
    /// Gets the number of values in the collection.
    /// </summary>
    public int Count => this._dictionary.Count;

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    public TValue this[int index] => this._dictionary._list[index].Value;

    TValue IList<TValue>.this[int index] {
      get => this[index];
      set => this._dictionary.SetAt(index, value);
    }

    bool ICollection<TValue>.IsReadOnly => false;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)this._dictionary._list).SyncRoot;

    /// <summary>
    /// Determines whether the collection contains the specified value.
    /// </summary>
    public bool Contains(TValue item) => this._dictionary.ContainsValue(item);

    /// <summary>
    /// Gets the index of the specified value.
    /// </summary>
    public int IndexOf(TValue item) {
      var comparer = EqualityComparer<TValue>.Default;
      for (var i = 0; i < this._dictionary._list.Count; ++i)
        if (comparer.Equals(this._dictionary._list[i].Value, item))
          return i;
      return -1;
    }

    /// <summary>
    /// Copies the values to an array.
    /// </summary>
    public void CopyTo(TValue[] array, int arrayIndex) {
      ArgumentNullException.ThrowIfNull(array);
      if (arrayIndex < 0 || arrayIndex > array.Length)
        throw new ArgumentOutOfRangeException(nameof(arrayIndex));
      if (array.Length - arrayIndex < this._dictionary.Count)
        throw new ArgumentException("Destination array is not long enough.");

      for (var i = 0; i < this._dictionary._list.Count; ++i)
        array[arrayIndex + i] = this._dictionary._list[i].Value;
    }

    void ICollection.CopyTo(Array array, int index) {
      ArgumentNullException.ThrowIfNull(array);
      if (array.Rank != 1)
        throw new ArgumentException("Array must be single-dimensional.");
      if (index < 0 || index > array.Length)
        throw new ArgumentOutOfRangeException(nameof(index));
      if (array.Length - index < this._dictionary.Count)
        throw new ArgumentException("Destination array is not long enough.");

      for (var i = 0; i < this._dictionary._list.Count; ++i)
        array.SetValue(this._dictionary._list[i].Value, index + i);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the values.
    /// </summary>
    public IEnumerator<TValue> GetEnumerator() {
      foreach (var kvp in this._dictionary._list)
        yield return kvp.Value;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException("Cannot add to value collection directly.");
    bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException("Cannot remove from value collection directly.");
    void ICollection<TValue>.Clear() => throw new NotSupportedException("Cannot clear value collection directly.");
    void IList<TValue>.Insert(int index, TValue item) => throw new NotSupportedException("Cannot insert into value collection directly.");
    void IList<TValue>.RemoveAt(int index) => throw new NotSupportedException("Cannot remove from value collection directly.");
  }

  #endregion
}

#endif
