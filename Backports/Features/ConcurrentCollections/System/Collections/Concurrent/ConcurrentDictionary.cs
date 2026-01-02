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

#if !SUPPORTS_CONCURRENT_COLLECTIONS

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Concurrent;

/// <summary>
/// Represents a thread-safe collection of key/value pairs that can be accessed by multiple threads concurrently.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// <para>
/// This implementation uses striped locking with <see cref="SpinLock"/> for optimal performance
/// under contention. Operations on different keys may proceed in parallel when they hash to
/// different lock stripes.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull {

  /// <summary>
  /// A bucket containing entries that hash to the same lock stripe.
  /// </summary>
  private sealed class Bucket {
    public volatile Node? Head;
  }

  /// <summary>
  /// A node in the bucket's linked list.
  /// </summary>
  private sealed class Node {
    public readonly TKey Key;
    public TValue Value;
    public readonly int HashCode;
    public volatile Node? Next;

    public Node(TKey key, TValue value, int hashCode, Node? next) {
      this.Key = key;
      this.Value = value;
      this.HashCode = hashCode;
      this.Next = next;
    }
  }

  /// <summary>
  /// The buckets array. Each bucket is protected by a corresponding lock.
  /// </summary>
  private volatile Bucket[] _buckets;

  /// <summary>
  /// The array of locks (SpinLock). Each lock protects a subset of buckets.
  /// </summary>
  private SpinLock[] _locks;

  /// <summary>
  /// The number of lock stripes to use.
  /// </summary>
  private readonly int _lockCount;

  /// <summary>
  /// The equality comparer for keys.
  /// </summary>
  private readonly IEqualityComparer<TKey> _comparer;

  /// <summary>
  /// The current count of items. Updated atomically.
  /// </summary>
  private volatile int _count;

  /// <summary>
  /// Default concurrency level based on processor count.
  /// </summary>
  private static int DefaultConcurrencyLevel => Environment.ProcessorCount;

  /// <summary>
  /// Default initial capacity.
  /// </summary>
  private const int _DEFAULT_CAPACITY = 31;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class.
  /// </summary>
  public ConcurrentDictionary()
    : this(DefaultConcurrencyLevel, _DEFAULT_CAPACITY, EqualityComparer<TKey>.Default) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class
  /// that uses the specified <see cref="IEqualityComparer{TKey}"/>.
  /// </summary>
  /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
  public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
    : this(DefaultConcurrencyLevel, _DEFAULT_CAPACITY, comparer) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class
  /// that contains elements copied from the specified <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
  /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
  public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    : this(collection, EqualityComparer<TKey>.Default) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class
  /// that contains elements copied from the specified <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
  /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
  /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
  public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
    : this(DefaultConcurrencyLevel, _DEFAULT_CAPACITY, comparer) {
    if (collection == null)
      throw new ArgumentNullException(nameof(collection));

    foreach (var kvp in collection)
      this.TryAddInternal(kvp.Key, kvp.Value, false, true, out _);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class
  /// with the specified concurrency level and capacity.
  /// </summary>
  /// <param name="concurrencyLevel">The estimated number of threads that will update the dictionary concurrently.</param>
  /// <param name="capacity">The initial number of elements the dictionary can contain.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="concurrencyLevel"/> is less than 1, or <paramref name="capacity"/> is less than 0.
  /// </exception>
  public ConcurrentDictionary(int concurrencyLevel, int capacity)
    : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentDictionary{TKey, TValue}"/> class
  /// with the specified concurrency level, capacity, and comparer.
  /// </summary>
  /// <param name="concurrencyLevel">The estimated number of threads that will update the dictionary concurrently.</param>
  /// <param name="capacity">The initial number of elements the dictionary can contain.</param>
  /// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="concurrencyLevel"/> is less than 1, or <paramref name="capacity"/> is less than 0.
  /// </exception>
  /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
  public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) {
    if (concurrencyLevel < 1)
      throw new ArgumentOutOfRangeException(nameof(concurrencyLevel), concurrencyLevel, "Concurrency level must be positive.");
    if (capacity < 0)
      throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be non-negative.");

    this._comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    this._lockCount = concurrencyLevel;

    // Initialize locks
    this._locks = new SpinLock[concurrencyLevel];
    for (var i = 0; i < concurrencyLevel; ++i)
      this._locks[i] = new SpinLock(false);

    // Initialize buckets (use at least concurrencyLevel buckets)
    var bucketCount = Math.Max(capacity, concurrencyLevel);
    this._buckets = new Bucket[bucketCount];
    for (var i = 0; i < bucketCount; ++i)
      this._buckets[i] = new Bucket();
  }

  /// <summary>
  /// Gets the hash code for a key and ensures it's positive.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int GetHashCode(TKey key) => this._comparer.GetHashCode(key) & 0x7FFFFFFF;

  /// <summary>
  /// Gets the bucket index for a given hash code.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int GetBucketIndex(int hashCode, int bucketCount) => hashCode % bucketCount;

  /// <summary>
  /// Gets the lock index for a given bucket index.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int GetLockIndex(int bucketIndex) => bucketIndex % this._lockCount;

  /// <summary>
  /// Acquires a lock and returns whether it was taken.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void AcquireLock(int lockIndex, ref bool lockTaken) => this._locks[lockIndex].Enter(ref lockTaken);

  /// <summary>
  /// Releases a lock if it was taken.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ReleaseLock(int lockIndex, bool lockTaken) {
    if (lockTaken)
      this._locks[lockIndex].Exit(false);
  }

  /// <summary>
  /// Acquires all locks in order.
  /// </summary>
  private void AcquireAllLocks(ref bool[] locksTaken) {
    locksTaken = new bool[this._lockCount];
    for (var i = 0; i < this._lockCount; ++i)
      this._locks[i].Enter(ref locksTaken[i]);
  }

  /// <summary>
  /// Releases all locks that were taken.
  /// </summary>
  private void ReleaseAllLocks(bool[] locksTaken) {
    for (var i = locksTaken.Length - 1; i >= 0; --i)
      if (locksTaken[i])
        this._locks[i].Exit(false);
  }

  /// <summary>
  /// Attempts to add a key/value pair to the dictionary.
  /// </summary>
  /// <param name="key">The key to add.</param>
  /// <param name="value">The value to add.</param>
  /// <returns><c>true</c> if the key/value pair was added; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool TryAdd(TKey key, TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    return this.TryAddInternal(key, value, false, true, out _);
  }

  /// <summary>
  /// Internal method to add a key/value pair.
  /// </summary>
  private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue) {
    var hashCode = this.GetHashCode(key);

    while (true) {
      var buckets = this._buckets;
      var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);
      var lockIndex = this.GetLockIndex(bucketIndex);
      var lockTaken = false;

      try {
        if (acquireLock)
          this.AcquireLock(lockIndex, ref lockTaken);

        // Check if buckets array changed (resize occurred)
        if (buckets != this._buckets)
          continue;

        // Search for existing key
        var bucket = buckets[bucketIndex];
        for (var node = bucket.Head; node != null; node = node.Next) {
          if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key)) {
            if (updateIfExists) {
              node.Value = value;
              resultingValue = value;
              return true;
            }
            resultingValue = node.Value;
            return false;
          }
        }

        // Key not found, add new node
        var newNode = new Node(key, value, hashCode, bucket.Head);
        bucket.Head = newNode;
        Interlocked.Increment(ref this._count);
        resultingValue = value;
        return true;
      } finally {
        this.ReleaseLock(lockIndex, lockTaken);
      }
    }
  }

  /// <summary>
  /// Attempts to get the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get.</param>
  /// <param name="value">
  /// When this method returns, contains the value associated with the specified key, if the key is found;
  /// otherwise, the default value for the type of the <paramref name="value"/> parameter.
  /// </param>
  /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    var hashCode = this.GetHashCode(key);
    var buckets = this._buckets;
    var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);

    // Read-only traversal doesn't need locking for snapshot consistency
    for (var node = buckets[bucketIndex].Head; node != null; node = node.Next) {
      if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key)) {
        value = node.Value;
        return true;
      }
    }

    value = default;
    return false;
  }

  /// <summary>
  /// Attempts to remove and return the value that has the specified key.
  /// </summary>
  /// <param name="key">The key of the element to remove and return.</param>
  /// <param name="value">
  /// When this method returns, contains the object removed from the dictionary, or the default value
  /// if the operation failed.
  /// </param>
  /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    var hashCode = this.GetHashCode(key);

    while (true) {
      var buckets = this._buckets;
      var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);
      var lockIndex = this.GetLockIndex(bucketIndex);
      var lockTaken = false;

      try {
        this.AcquireLock(lockIndex, ref lockTaken);

        // Check if buckets array changed
        if (buckets != this._buckets)
          continue;

        var bucket = buckets[bucketIndex];
        Node? prev = null;
        for (var node = bucket.Head; node != null; prev = node, node = node.Next) {
          if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key)) {
            if (prev == null)
              bucket.Head = node.Next;
            else
              prev.Next = node.Next;

            value = node.Value;
            Interlocked.Decrement(ref this._count);
            return true;
          }
        }

        value = default;
        return false;
      } finally {
        this.ReleaseLock(lockIndex, lockTaken);
      }
    }
  }

  /// <summary>
  /// Determines whether the dictionary contains the specified key.
  /// </summary>
  /// <param name="key">The key to locate.</param>
  /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool ContainsKey(TKey key) => this.TryGetValue(key, out _);

  /// <summary>
  /// Adds a key/value pair to the dictionary if the key does not already exist,
  /// or updates the value if the key already exists.
  /// </summary>
  /// <param name="key">The key to be added or whose value should be updated.</param>
  /// <param name="addValue">The value to be added for an absent key.</param>
  /// <param name="updateValueFactory">
  /// The function used to generate a new value for an existing key based on the key's existing value.
  /// </param>
  /// <returns>
  /// The new value for the key. This will be either <paramref name="addValue"/> (if the key was absent)
  /// or the result of <paramref name="updateValueFactory"/> (if the key was present).
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="updateValueFactory"/> is null.</exception>
  public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));
    if (updateValueFactory == null)
      throw new ArgumentNullException(nameof(updateValueFactory));

    var hashCode = this.GetHashCode(key);

    while (true) {
      var buckets = this._buckets;
      var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);
      var lockIndex = this.GetLockIndex(bucketIndex);
      var lockTaken = false;

      try {
        this.AcquireLock(lockIndex, ref lockTaken);

        // Check if buckets array changed
        if (buckets != this._buckets)
          continue;

        var bucket = buckets[bucketIndex];

        // Search for existing key
        for (var node = bucket.Head; node != null; node = node.Next) {
          if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key)) {
            var newValue = updateValueFactory(key, node.Value);
            node.Value = newValue;
            return newValue;
          }
        }

        // Key not found, add new node
        var newNode = new Node(key, addValue, hashCode, bucket.Head);
        bucket.Head = newNode;
        Interlocked.Increment(ref this._count);
        return addValue;
      } finally {
        this.ReleaseLock(lockIndex, lockTaken);
      }
    }
  }

  /// <summary>
  /// Adds a key/value pair to the dictionary if the key does not already exist,
  /// or updates the value if the key already exists.
  /// </summary>
  /// <param name="key">The key to be added or whose value should be updated.</param>
  /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
  /// <param name="updateValueFactory">
  /// The function used to generate a new value for an existing key based on the key's existing value.
  /// </param>
  /// <returns>
  /// The new value for the key. This will be either the result of <paramref name="addValueFactory"/>
  /// (if the key was absent) or the result of <paramref name="updateValueFactory"/> (if the key was present).
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="key"/>, <paramref name="addValueFactory"/>, or <paramref name="updateValueFactory"/> is null.
  /// </exception>
  public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));
    if (addValueFactory == null)
      throw new ArgumentNullException(nameof(addValueFactory));
    if (updateValueFactory == null)
      throw new ArgumentNullException(nameof(updateValueFactory));

    var hashCode = this.GetHashCode(key);

    while (true) {
      var buckets = this._buckets;
      var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);
      var lockIndex = this.GetLockIndex(bucketIndex);
      var lockTaken = false;

      try {
        this.AcquireLock(lockIndex, ref lockTaken);

        // Check if buckets array changed
        if (buckets != this._buckets)
          continue;

        var bucket = buckets[bucketIndex];

        // Search for existing key
        for (var node = bucket.Head; node != null; node = node.Next) {
          if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key)) {
            var newValue = updateValueFactory(key, node.Value);
            node.Value = newValue;
            return newValue;
          }
        }

        // Key not found, add new node
        var addValue = addValueFactory(key);
        var newNode = new Node(key, addValue, hashCode, bucket.Head);
        bucket.Head = newNode;
        Interlocked.Increment(ref this._count);
        return addValue;
      } finally {
        this.ReleaseLock(lockIndex, lockTaken);
      }
    }
  }

  /// <summary>
  /// Adds a key/value pair to the dictionary if the key does not already exist.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="value">The value to be added, if the key does not already exist.</param>
  /// <returns>
  /// The value for the key. This will be either the existing value for the key if the key is already
  /// in the dictionary, or the new value if the key was not in the dictionary.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public TValue GetOrAdd(TKey key, TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    this.TryAddInternal(key, value, false, true, out var resultingValue);
    return resultingValue;
  }

  /// <summary>
  /// Adds a key/value pair to the dictionary by using the specified function
  /// if the key does not already exist.
  /// </summary>
  /// <param name="key">The key of the element to add.</param>
  /// <param name="valueFactory">The function used to generate a value for the key.</param>
  /// <returns>
  /// The value for the key. This will be either the existing value for the key if the key is already
  /// in the dictionary, or the new value if the key was not in the dictionary.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="valueFactory"/> is null.</exception>
  public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));
    if (valueFactory == null)
      throw new ArgumentNullException(nameof(valueFactory));

    var hashCode = this.GetHashCode(key);

    // Fast path: check if key exists without lock
    var buckets = this._buckets;
    var bucketIndex = this.GetBucketIndex(hashCode, buckets.Length);
    for (var node = buckets[bucketIndex].Head; node != null; node = node.Next) {
      if (node.HashCode == hashCode && this._comparer.Equals(node.Key, key))
        return node.Value;
    }

    // Slow path: create value and try to add
    var newValue = valueFactory(key);
    this.TryAddInternal(key, newValue, false, true, out var resultingValue);
    return resultingValue;
  }

  /// <summary>
  /// Gets the number of key/value pairs contained in the dictionary.
  /// </summary>
  public int Count => this._count;

  /// <summary>
  /// Gets a value that indicates whether the dictionary is empty.
  /// </summary>
  public bool IsEmpty => this._count == 0;

  /// <summary>
  /// Gets or sets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key of the value to get or set.</param>
  /// <returns>The value associated with the specified key.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  /// <exception cref="KeyNotFoundException">
  /// The property is retrieved and <paramref name="key"/> does not exist in the collection.
  /// </exception>
  public TValue this[TKey key] {
    get {
      if (!this.TryGetValue(key, out var value))
        throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
      return value;
    }
    set {
      if (key == null)
        throw new ArgumentNullException(nameof(key));

      this.TryAddInternal(key, value, true, true, out _);
    }
  }

  /// <summary>
  /// Gets a collection containing the keys in the dictionary.
  /// </summary>
  public ICollection<TKey> Keys {
    get {
      bool[]? locksTaken = null;
      try {
        this.AcquireAllLocks(ref locksTaken!);
        var keys = new List<TKey>(this._count);
        foreach (var bucket in this._buckets)
          for (var node = bucket.Head; node != null; node = node.Next)
            keys.Add(node.Key);
        return keys;
      } finally {
        if (locksTaken != null)
          this.ReleaseAllLocks(locksTaken);
      }
    }
  }

  /// <summary>
  /// Gets a collection containing the values in the dictionary.
  /// </summary>
  public ICollection<TValue> Values {
    get {
      bool[]? locksTaken = null;
      try {
        this.AcquireAllLocks(ref locksTaken!);
        var values = new List<TValue>(this._count);
        foreach (var bucket in this._buckets)
          for (var node = bucket.Head; node != null; node = node.Next)
            values.Add(node.Value);
        return values;
      } finally {
        if (locksTaken != null)
          this.ReleaseAllLocks(locksTaken);
      }
    }
  }

  /// <summary>
  /// Removes all keys and values from the dictionary.
  /// </summary>
  public void Clear() {
    bool[]? locksTaken = null;
    try {
      this.AcquireAllLocks(ref locksTaken!);

      // Clear all buckets
      foreach (var bucket in this._buckets)
        bucket.Head = null;

      this._count = 0;
    } finally {
      if (locksTaken != null)
        this.ReleaseAllLocks(locksTaken);
    }
  }

  /// <summary>
  /// Copies the key and value pairs stored in the dictionary to a new array.
  /// </summary>
  /// <returns>A new array containing a snapshot of key and value pairs copied from the dictionary.</returns>
  public KeyValuePair<TKey, TValue>[] ToArray() {
    bool[]? locksTaken = null;
    try {
      this.AcquireAllLocks(ref locksTaken!);
      var array = new KeyValuePair<TKey, TValue>[this._count];
      var index = 0;
      foreach (var bucket in this._buckets)
        for (var node = bucket.Head; node != null; node = node.Next)
          array[index++] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
      return array;
    } finally {
      if (locksTaken != null)
        this.ReleaseAllLocks(locksTaken);
    }
  }

  /// <summary>
  /// Returns an enumerator that iterates through the dictionary.
  /// </summary>
  /// <returns>An enumerator for the dictionary.</returns>
  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
    // Take a snapshot for enumeration
    var array = this.ToArray();
    foreach (var kvp in array)
      yield return kvp;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the dictionary.
  /// </summary>
  /// <returns>An enumerator for the dictionary.</returns>
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

#if SUPPORTS_LINQ

  /// <summary>
  /// Returns an array of key/value pairs sorted by the specified key selector.
  /// </summary>
  /// <typeparam name="TSort">The type of the sort key.</typeparam>
  /// <param name="keySelector">A function to extract a sort key from each element.</param>
  /// <returns>An array of key/value pairs sorted by the specified key.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public KeyValuePair<TKey, TValue>[] OrderBy<TSort>(Func<KeyValuePair<TKey, TValue>, TSort> keySelector)
    => this.ToArray().OrderBy(keySelector).ToArray();

  /// <summary>
  /// Returns an array of key/value pairs sorted in descending order by the specified key selector.
  /// </summary>
  /// <typeparam name="TSort">The type of the sort key.</typeparam>
  /// <param name="keySelector">A function to extract a sort key from each element.</param>
  /// <returns>An array of key/value pairs sorted in descending order by the specified key.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public KeyValuePair<TKey, TValue>[] OrderByDescending<TSort>(Func<KeyValuePair<TKey, TValue>, TSort> keySelector)
    => this.ToArray().OrderByDescending(keySelector).ToArray();

#endif

  #region IDictionary<TKey, TValue> Implementation

  /// <summary>
  /// Gets a value indicating whether the dictionary is read-only.
  /// </summary>
  bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

  /// <summary>
  /// Adds a key/value pair to the dictionary.
  /// </summary>
  /// <param name="key">The key to add.</param>
  /// <param name="value">The value to add.</param>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  /// <exception cref="ArgumentException">A key with the same value already exists.</exception>
  void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
    if (!this.TryAdd(key, value))
      throw new ArgumentException("An element with the same key already exists in the dictionary.", nameof(key));
  }

  /// <summary>
  /// Removes the element with the specified key from the dictionary.
  /// </summary>
  /// <param name="key">The key of the element to remove.</param>
  /// <returns><c>true</c> if the element was successfully removed; otherwise, <c>false</c>.</returns>
  bool IDictionary<TKey, TValue>.Remove(TKey key) => this.TryRemove(key, out _);

  /// <summary>
  /// Adds a key/value pair to the dictionary.
  /// </summary>
  /// <param name="item">The key/value pair to add.</param>
  void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    => ((IDictionary<TKey, TValue>)this).Add(item.Key, item.Value);

  /// <summary>
  /// Determines whether the dictionary contains a specific key/value pair.
  /// </summary>
  /// <param name="item">The key/value pair to locate.</param>
  /// <returns><c>true</c> if the dictionary contains the key/value pair; otherwise, <c>false</c>.</returns>
  bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    => this.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

  /// <summary>
  /// Copies the elements of the dictionary to an array, starting at the specified array index.
  /// </summary>
  /// <param name="array">The destination array.</param>
  /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
  void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));
    if (arrayIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(arrayIndex));

    var items = this.ToArray();
    if (arrayIndex > array.Length - items.Length)
      throw new ArgumentException("The destination array is not large enough.");

    Array.Copy(items, 0, array, arrayIndex, items.Length);
  }

  /// <summary>
  /// Removes a key/value pair from the dictionary.
  /// </summary>
  /// <param name="item">The key/value pair to remove.</param>
  /// <returns><c>true</c> if the key/value pair was successfully removed; otherwise, <c>false</c>.</returns>
  bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
    if (this.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value))
      return this.TryRemove(item.Key, out _);
    return false;
  }

  #endregion

  #region IReadOnlyDictionary<TKey, TValue> Implementation

  /// <summary>
  /// Gets a collection containing the keys in the dictionary.
  /// </summary>
  IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

  /// <summary>
  /// Gets a collection containing the values in the dictionary.
  /// </summary>
  IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

  #endregion

}

#endif
