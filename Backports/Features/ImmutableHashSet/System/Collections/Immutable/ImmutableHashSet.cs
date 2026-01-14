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
/// Represents an immutable unordered collection of unique elements.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public interface IImmutableSet<T> : IReadOnlyCollection<T> {
  /// <summary>
  /// Adds the specified element to the set.
  /// </summary>
  IImmutableSet<T> Add(T value);

  /// <summary>
  /// Removes all elements in the specified collection from the current set.
  /// </summary>
  IImmutableSet<T> Except(IEnumerable<T> other);

  /// <summary>
  /// Creates a set that contains only elements that exist in both this set and the specified collection.
  /// </summary>
  IImmutableSet<T> Intersect(IEnumerable<T> other);

  /// <summary>
  /// Removes the specified element from the set.
  /// </summary>
  IImmutableSet<T> Remove(T value);

  /// <summary>
  /// Creates a set that contains only elements that are present either in the current set or in the specified collection, but not both.
  /// </summary>
  IImmutableSet<T> SymmetricExcept(IEnumerable<T> other);

  /// <summary>
  /// Creates a set that contains all elements that are present in either the current set or the specified collection.
  /// </summary>
  IImmutableSet<T> Union(IEnumerable<T> other);

  /// <summary>
  /// Retrieves an empty set that has the same sorting and ordering semantics as this instance.
  /// </summary>
  IImmutableSet<T> Clear();

  /// <summary>
  /// Determines whether the set contains a specific element.
  /// </summary>
  bool Contains(T value);

  /// <summary>
  /// Determines whether the current set is a proper subset of the specified collection.
  /// </summary>
  bool IsProperSubsetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a proper superset of the specified collection.
  /// </summary>
  bool IsProperSupersetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a subset of the specified collection.
  /// </summary>
  bool IsSubsetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set is a superset of the specified collection.
  /// </summary>
  bool IsSupersetOf(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set overlaps with the specified collection.
  /// </summary>
  bool Overlaps(IEnumerable<T> other);

  /// <summary>
  /// Determines whether the current set and the specified collection contain the same elements.
  /// </summary>
  bool SetEquals(IEnumerable<T> other);

  /// <summary>
  /// Adds the specified element to the set if it's not already present.
  /// </summary>
  bool TryGetValue(T equalValue, out T actualValue);
}

#endregion

/// <summary>
/// Represents an immutable, unordered collection of unique elements.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class ImmutableHashSet<T> : IImmutableSet<T>, ISet<T>, ICollection {
  private readonly HashSet<T> _set;

  /// <summary>
  /// Gets an empty immutable hash set.
  /// </summary>
  public static readonly ImmutableHashSet<T> Empty = new([], EqualityComparer<T>.Default);

  internal ImmutableHashSet(HashSet<T> set, IEqualityComparer<T> comparer) {
    this._set = set;
    this.KeyComparer = comparer ?? EqualityComparer<T>.Default;
  }

  /// <summary>
  /// Gets the key comparer used to determine element equality.
  /// </summary>
  public IEqualityComparer<T> KeyComparer { get; }

  /// <inheritdoc />
  public int Count => this._set.Count;

  /// <summary>
  /// Gets a value indicating whether this set is empty.
  /// </summary>
  public bool IsEmpty => this._set.Count == 0;

  /// <inheritdoc />
  public bool Contains(T value) => this._set.Contains(value);

  /// <inheritdoc />
  public bool TryGetValue(T equalValue, out T actualValue) {
    foreach (var item in this._set)
      if (this.KeyComparer.Equals(item, equalValue)) {
        actualValue = item;
        return true;
      }

    actualValue = default!;
    return false;
  }

  /// <summary>
  /// Adds the specified element to the set.
  /// </summary>
  public ImmutableHashSet<T> Add(T value) {
    if (this._set.Contains(value))
      return this;

    var newSet = new HashSet<T>(this._set, this.KeyComparer) { value };
    return new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Removes the specified element from the set.
  /// </summary>
  public ImmutableHashSet<T> Remove(T value) {
    if (!this._set.Contains(value))
      return this;

    var newSet = new HashSet<T>(this._set, this.KeyComparer);
    newSet.Remove(value);
    return new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Retrieves an empty set that has the same sorting and ordering semantics as this instance.
  /// </summary>
  public ImmutableHashSet<T> Clear() =>
    this._set.Count == 0 ? this : new(new(this.KeyComparer), this.KeyComparer);

  /// <summary>
  /// Creates a set that contains all elements that are present in either the current set or the specified collection.
  /// </summary>
  public ImmutableHashSet<T> Union(IEnumerable<T> other) {
    var newSet = new HashSet<T>(this._set, this.KeyComparer);
    var originalCount = newSet.Count;
    foreach (var item in other)
      newSet.Add(item);

    return newSet.Count == originalCount ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Creates a set that contains only elements that exist in both this set and the specified collection.
  /// </summary>
  public ImmutableHashSet<T> Intersect(IEnumerable<T> other) {
    var newSet = new HashSet<T>(this.KeyComparer);
    var otherSet = other is HashSet<T> hs ? hs : new(other, this.KeyComparer);
    foreach (var item in this._set)
      if (otherSet.Contains(item))
        newSet.Add(item);

    return newSet.Count == this._set.Count ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Removes all elements in the specified collection from the current set.
  /// </summary>
  public ImmutableHashSet<T> Except(IEnumerable<T> other) {
    var newSet = new HashSet<T>(this._set, this.KeyComparer);
    var originalCount = newSet.Count;
    foreach (var item in other)
      newSet.Remove(item);

    return newSet.Count == originalCount ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Creates a set that contains only elements that are present either in the current set or in the specified collection, but not both.
  /// </summary>
  public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other) {
    var newSet = new HashSet<T>(this._set, this.KeyComparer);
    foreach (var item in other)
      if (!newSet.Remove(item))
        newSet.Add(item);

    return new(newSet, this.KeyComparer);
  }

  #region IImmutableSet explicit implementation

  IImmutableSet<T> IImmutableSet<T>.Add(T value) => this.Add(value);
  IImmutableSet<T> IImmutableSet<T>.Remove(T value) => this.Remove(value);
  IImmutableSet<T> IImmutableSet<T>.Clear() => this.Clear();
  IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other) => this.Union(other);
  IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other) => this.Intersect(other);
  IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other) => this.Except(other);
  IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other) => this.SymmetricExcept(other);

  #endregion

  /// <inheritdoc />
  public bool IsSubsetOf(IEnumerable<T> other) => this._set.IsSubsetOf(other);

  /// <inheritdoc />
  public bool IsSupersetOf(IEnumerable<T> other) => this._set.IsSupersetOf(other);

  /// <inheritdoc />
  public bool IsProperSubsetOf(IEnumerable<T> other) => this._set.IsProperSubsetOf(other);

  /// <inheritdoc />
  public bool IsProperSupersetOf(IEnumerable<T> other) => this._set.IsProperSupersetOf(other);

  /// <inheritdoc />
  public bool Overlaps(IEnumerable<T> other) => this._set.Overlaps(other);

  /// <inheritdoc />
  public bool SetEquals(IEnumerable<T> other) => this._set.SetEquals(other);

  /// <summary>
  /// Creates a new set with the specified comparer.
  /// </summary>
  public ImmutableHashSet<T> WithComparer(IEqualityComparer<T> comparer) {
    comparer ??= EqualityComparer<T>.Default;
    if (comparer == this.KeyComparer)
      return this;

    var newSet = new HashSet<T>(comparer);
    foreach (var item in this._set)
      newSet.Add(item);
    return new(newSet, comparer);
  }

  /// <summary>
  /// Creates a mutable builder for this set.
  /// </summary>
  public Builder ToBuilder() => new(new(this._set, this.KeyComparer), this.KeyComparer);

  /// <summary>
  /// Returns an enumerator that iterates through the set.
  /// </summary>
  public Enumerator GetEnumerator() => new(((IEnumerable<T>)this._set).GetEnumerator());

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)this._set).GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._set).GetEnumerator();

  #region ISet<T> explicit implementation

  bool ISet<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");

  void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");

  void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");

  void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");

  void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");

  #endregion

  #region ICollection<T> explicit implementation

  bool ICollection<T>.IsReadOnly => true;

  void ICollection<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");

  void ICollection<T>.Clear() => throw new NotSupportedException("Collection is read-only.");

  bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Collection is read-only.");

  void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this._set.CopyTo(array, arrayIndex);

  #endregion

  #region ICollection explicit implementation

  bool ICollection.IsSynchronized => false;

  object ICollection.SyncRoot => this;

  void ICollection.CopyTo(Array array, int index) => ((ICollection)this._set).CopyTo(array, index);

  #endregion

  /// <summary>
  /// Enumerates the contents of the immutable hash set.
  /// </summary>
  public readonly struct Enumerator : IEnumerator<T> {
    private readonly IEnumerator<T> _enumerator;

    internal Enumerator(IEnumerator<T> enumerator) => this._enumerator = enumerator;

    /// <inheritdoc />
    public T Current => this._enumerator.Current;

    object? IEnumerator.Current => this._enumerator.Current;

    /// <inheritdoc />
    public bool MoveNext() => this._enumerator.MoveNext();

    /// <inheritdoc />
    public void Reset() => this._enumerator.Reset();

    /// <inheritdoc />
    public void Dispose() => this._enumerator.Dispose();
  }

  /// <summary>
  /// A builder for creating immutable hash sets efficiently.
  /// </summary>
  public sealed class Builder : ISet<T>, IReadOnlyCollection<T>, ICollection {
    private readonly HashSet<T> _set;

    internal Builder(HashSet<T> set, IEqualityComparer<T> comparer) {
      this._set = set;
      this.KeyComparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Gets the key comparer.
    /// </summary>
    public IEqualityComparer<T> KeyComparer { get; }

    /// <inheritdoc />
    public int Count => this._set.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public bool Add(T item) => this._set.Add(item);

    void ICollection<T>.Add(T item) => this._set.Add(item);

    /// <inheritdoc />
    public void Clear() => this._set.Clear();

    /// <inheritdoc />
    public bool Contains(T item) => this._set.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => this._set.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(T item) => this._set.Remove(item);

    /// <inheritdoc />
    public void ExceptWith(IEnumerable<T> other) => this._set.ExceptWith(other);

    /// <inheritdoc />
    public void IntersectWith(IEnumerable<T> other) => this._set.IntersectWith(other);

    /// <inheritdoc />
    public bool IsProperSubsetOf(IEnumerable<T> other) => this._set.IsProperSubsetOf(other);

    /// <inheritdoc />
    public bool IsProperSupersetOf(IEnumerable<T> other) => this._set.IsProperSupersetOf(other);

    /// <inheritdoc />
    public bool IsSubsetOf(IEnumerable<T> other) => this._set.IsSubsetOf(other);

    /// <inheritdoc />
    public bool IsSupersetOf(IEnumerable<T> other) => this._set.IsSupersetOf(other);

    /// <inheritdoc />
    public bool Overlaps(IEnumerable<T> other) => this._set.Overlaps(other);

    /// <inheritdoc />
    public bool SetEquals(IEnumerable<T> other) => this._set.SetEquals(other);

    /// <inheritdoc />
    public void SymmetricExceptWith(IEnumerable<T> other) => this._set.SymmetricExceptWith(other);

    /// <inheritdoc />
    public void UnionWith(IEnumerable<T> other) => this._set.UnionWith(other);

    /// <summary>
    /// Gets the value in the set that is equal to the specified value.
    /// </summary>
    public bool TryGetValue(T equalValue, out T actualValue) {
      foreach (var item in this._set)
        if (this.KeyComparer.Equals(item, equalValue)) {
          actualValue = item;
          return true;
        }

      actualValue = default!;
      return false;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)this._set).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._set).GetEnumerator();

    /// <summary>
    /// Creates an immutable hash set based on the contents of this builder.
    /// </summary>
    public ImmutableHashSet<T> ToImmutable() =>
      new(new(this._set, this.KeyComparer), this.KeyComparer);

    #region ICollection explicit implementation

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._set).CopyTo(array, index);

    #endregion
  }
}

/// <summary>
/// Provides a set of static methods for creating immutable hash sets.
/// </summary>
public static class ImmutableHashSet {
  /// <summary>
  /// Creates an empty immutable hash set.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>() =>
    ImmutableHashSet<T>.Empty;

  /// <summary>
  /// Creates an empty immutable hash set with the specified comparer.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> comparer) =>
    new(new(comparer), comparer);

  /// <summary>
  /// Creates an immutable hash set with the specified item.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>(T item) =>
    new([item], EqualityComparer<T>.Default);

  /// <summary>
  /// Creates an immutable hash set with the specified items.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>(params T[] items) =>
    new([..items], EqualityComparer<T>.Default);

  /// <summary>
  /// Creates an immutable hash set with the specified comparer and item.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> comparer, T item) =>
    new(new(comparer) { item }, comparer);

  /// <summary>
  /// Creates an immutable hash set with the specified comparer and items.
  /// </summary>
  public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> comparer, params T[] items) =>
    new(new(items, comparer), comparer);

  /// <summary>
  /// Creates a new builder for creating immutable hash sets.
  /// </summary>
  public static ImmutableHashSet<T>.Builder CreateBuilder<T>() =>
    new([], EqualityComparer<T>.Default);

  /// <summary>
  /// Creates a new builder for creating immutable hash sets with the specified comparer.
  /// </summary>
  public static ImmutableHashSet<T>.Builder CreateBuilder<T>(IEqualityComparer<T> comparer) =>
    new(new(comparer), comparer);

  /// <summary>
  /// Creates an immutable hash set from the specified items.
  /// </summary>
  public static ImmutableHashSet<T> CreateRange<T>(IEnumerable<T> items) =>
    new([..items], EqualityComparer<T>.Default);

  /// <summary>
  /// Creates an immutable hash set from the specified items with the specified comparer.
  /// </summary>
  public static ImmutableHashSet<T> CreateRange<T>(IEqualityComparer<T> comparer, IEnumerable<T> items) =>
    new(new(items, comparer), comparer);

  extension<T>(IEnumerable<T> source)
  {
    /// <summary>
    /// Enumerates a sequence and produces an immutable hash set of its contents.
    /// </summary>
    public ImmutableHashSet<T> ToImmutableHashSet() =>
      CreateRange(source);

    /// <summary>
    /// Enumerates a sequence and produces an immutable hash set of its contents with the specified comparer.
    /// </summary>
    public ImmutableHashSet<T> ToImmutableHashSet(IEqualityComparer<T> comparer) =>
      CreateRange(comparer, source);
  }
}

#endif
