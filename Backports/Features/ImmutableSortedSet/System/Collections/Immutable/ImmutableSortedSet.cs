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
using System.Linq;

namespace System.Collections.Immutable;

/// <summary>
/// Represents an immutable sorted set.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class ImmutableSortedSet<T> : IImmutableSet<T>, ISet<T>, IList<T>, IList, ICollection {
  private readonly SortedSet<T> _set;

  /// <summary>
  /// Gets an empty immutable sorted set.
  /// </summary>
  public static readonly ImmutableSortedSet<T> Empty = new([], Comparer<T>.Default);

  internal ImmutableSortedSet(SortedSet<T> set, IComparer<T> comparer) {
    this._set = set;
    this.KeyComparer = comparer ?? Comparer<T>.Default;
  }

  /// <summary>
  /// Gets the comparer used to order elements in the set.
  /// </summary>
  public IComparer<T> KeyComparer { get; }

  /// <inheritdoc />
  public int Count => this._set.Count;

  /// <summary>
  /// Gets a value indicating whether this set is empty.
  /// </summary>
  public bool IsEmpty => this._set.Count == 0;

  /// <summary>
  /// Gets the minimum value in the set.
  /// </summary>
  public T? Min => this._set.Min;

  /// <summary>
  /// Gets the maximum value in the set.
  /// </summary>
  public T? Max => this._set.Max;

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  public T this[int index] {
    get {
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._set.Count);
      return this._set.ElementAt(index);
    }
  }

  /// <inheritdoc />
  public bool Contains(T value) => this._set.Contains(value);

  /// <inheritdoc />
  public bool TryGetValue(T equalValue, out T actualValue) {
    foreach (var item in this._set)
      if (this.KeyComparer.Compare(item, equalValue) == 0) {
        actualValue = item;
        return true;
      }

    actualValue = default!;
    return false;
  }

  /// <summary>
  /// Returns the index of the specified item in the set.
  /// </summary>
  public int IndexOf(T item) {
    var index = 0;
    foreach (var element in this._set) {
      if (this.KeyComparer.Compare(element, item) == 0)
        return index;

      ++index;
    }
    return -1;
  }

  /// <summary>
  /// Adds the specified value to the set.
  /// </summary>
  /// <param name="value">The value to add.</param>
  /// <returns>A new set with the value added, or this set if the value already exists.</returns>
  public ImmutableSortedSet<T> Add(T value) {
    if (this._set.Contains(value))
      return this;

    var newSet = new SortedSet<T>(this._set, this.KeyComparer) { value };
    return new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Removes the specified value from the set.
  /// </summary>
  /// <param name="value">The value to remove.</param>
  /// <returns>A new set with the value removed, or this set if the value does not exist.</returns>
  public ImmutableSortedSet<T> Remove(T value) {
    if (!this._set.Contains(value))
      return this;

    var newSet = new SortedSet<T>(this._set, this.KeyComparer);
    newSet.Remove(value);
    return new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Returns an empty set.
  /// </summary>
  /// <returns>An empty immutable sorted set.</returns>
  public ImmutableSortedSet<T> Clear() =>
    this._set.Count == 0 ? this : new(new(this.KeyComparer), this.KeyComparer);

  /// <summary>
  /// Produces a set that contains all elements from both sets.
  /// </summary>
  /// <param name="other">The elements to add to this set.</param>
  /// <returns>A new set with all elements from both sets.</returns>
  public ImmutableSortedSet<T> Union(IEnumerable<T> other) {
    var newSet = new SortedSet<T>(this._set, this.KeyComparer);
    var originalCount = newSet.Count;
    foreach (var item in other)
      newSet.Add(item);

    return newSet.Count == originalCount ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Produces a set that contains elements that exist in both sets.
  /// </summary>
  /// <param name="other">The elements to intersect with this set.</param>
  /// <returns>A new set with only the common elements.</returns>
  public ImmutableSortedSet<T> Intersect(IEnumerable<T> other) {
    var newSet = new SortedSet<T>(this.KeyComparer);
    var otherSet = other is SortedSet<T> ss ? ss : new(other, this.KeyComparer);
    foreach (var item in this._set)
      if (otherSet.Contains(item))
        newSet.Add(item);

    return newSet.Count == this._set.Count ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Produces a set that contains elements in this set but not in the specified sequence.
  /// </summary>
  /// <param name="other">The elements to remove from this set.</param>
  /// <returns>A new set with the specified elements removed.</returns>
  public ImmutableSortedSet<T> Except(IEnumerable<T> other) {
    var newSet = new SortedSet<T>(this._set, this.KeyComparer);
    var originalCount = newSet.Count;
    foreach (var item in other)
      newSet.Remove(item);

    return newSet.Count == originalCount ? this : new(newSet, this.KeyComparer);
  }

  /// <summary>
  /// Produces a set that contains elements that are in either set but not both.
  /// </summary>
  /// <param name="other">The elements to compare with this set.</param>
  /// <returns>A new set with the symmetric difference.</returns>
  public ImmutableSortedSet<T> SymmetricExcept(IEnumerable<T> other) {
    var newSet = new SortedSet<T>(this._set, this.KeyComparer);
    foreach (var item in other)
      if (!newSet.Remove(item))
        newSet.Add(item);

    return new(newSet, this.KeyComparer);
  }

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
  /// Returns a subset of the set from the specified lower and upper bounds.
  /// </summary>
  public ImmutableSortedSet<T> GetViewBetween(T lowerValue, T upperValue) {
    var view = this._set.GetViewBetween(lowerValue, upperValue);
    return new(new(view, this.KeyComparer), this.KeyComparer);
  }

  /// <summary>
  /// Returns the set in reverse order.
  /// </summary>
  public IEnumerable<T> Reverse() => this._set.Reverse();

  /// <summary>
  /// Creates a new sorted set with the specified comparer.
  /// </summary>
  public ImmutableSortedSet<T> WithComparer(IComparer<T> comparer) {
    comparer ??= Comparer<T>.Default;
    if (comparer == this.KeyComparer)
      return this;

    var newSet = new SortedSet<T>(comparer);
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
  public Enumerator GetEnumerator() => new(this._set.GetEnumerator());

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._set.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => this._set.GetEnumerator();

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

  #region IList<T> explicit implementation

  T IList<T>.this[int index] {
    get => this[index];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  void IList<T>.Insert(int index, T item) => throw new NotSupportedException("Collection is read-only.");

  void IList<T>.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");

  #endregion

  #region IList explicit implementation

  object? IList.this[int index] {
    get => this[index];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  bool IList.IsFixedSize => true;

  bool IList.IsReadOnly => true;

  int IList.Add(object value) => throw new NotSupportedException("Collection is read-only.");

  void IList.Clear() => throw new NotSupportedException("Collection is read-only.");

  bool IList.Contains(object value) => value is T t && this._set.Contains(t);

  int IList.IndexOf(object value) => value is T t ? this.IndexOf(t) : -1;

  void IList.Insert(int index, object value) => throw new NotSupportedException("Collection is read-only.");

  void IList.Remove(object value) => throw new NotSupportedException("Collection is read-only.");

  void IList.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");

  #endregion

  #region ICollection explicit implementation

  bool ICollection.IsSynchronized => false;

  object ICollection.SyncRoot => this;

  void ICollection.CopyTo(Array array, int index) => ((ICollection)this._set).CopyTo(array, index);

  #endregion

  #region IImmutableSet explicit implementation

  IImmutableSet<T> IImmutableSet<T>.Add(T value) => this.Add(value);

  IImmutableSet<T> IImmutableSet<T>.Remove(T value) => this.Remove(value);

  IImmutableSet<T> IImmutableSet<T>.Clear() => this.Clear();

  IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other) => this.Union(other);

  IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other) => this.Intersect(other);

  IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other) => this.Except(other);

  IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other) => this.SymmetricExcept(other);

  #endregion

  /// <summary>
  /// Enumerates the contents of the immutable sorted set.
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
  /// A builder for creating immutable sorted sets efficiently.
  /// </summary>
  public sealed class Builder : ISet<T>, IReadOnlyCollection<T>, IList<T>, IList, ICollection {
    private readonly SortedSet<T> _set;
    private readonly IComparer<T> _comparer;

    internal Builder(SortedSet<T> set, IComparer<T> comparer) {
      this._set = set;
      this._comparer = comparer ?? Comparer<T>.Default;
    }

    /// <summary>
    /// Gets the comparer.
    /// </summary>
    public IComparer<T> KeyComparer => this._comparer;

    /// <inheritdoc />
    public int Count => this._set.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the minimum value in the set.
    /// </summary>
    public T? Min => this._set.Min;

    /// <summary>
    /// Gets the maximum value in the set.
    /// </summary>
    public T? Max => this._set.Max;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public T this[int index] {
      get {
        if (index < 0 || index >= this._set.Count)
          throw new ArgumentOutOfRangeException(nameof(index));
        return this._set.ElementAt(index);
      }
      set => throw new NotSupportedException("Cannot set element by index in a sorted set.");
    }

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
        if (this._comparer.Compare(item, equalValue) == 0) {
          actualValue = item;
          return true;
        }

      actualValue = default!;
      return false;
    }

    /// <summary>
    /// Returns the index of the specified item in the set.
    /// </summary>
    public int IndexOf(T item) {
      var index = 0;
      foreach (var element in this._set) {
        if (this._comparer.Compare(element, item) == 0)
          return index;
        ++index;
      }
      return -1;
    }

    /// <summary>
    /// Returns the set in reverse order.
    /// </summary>
    public IEnumerable<T> Reverse() => this._set.Reverse();

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => this._set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this._set.GetEnumerator();

    /// <summary>
    /// Creates an immutable sorted set based on the contents of this builder.
    /// </summary>
    public ImmutableSortedSet<T> ToImmutable() =>
      new(new(this._set, this._comparer), this._comparer);

    #region IList<T> explicit implementation

    void IList<T>.Insert(int index, T item) => throw new NotSupportedException("Cannot insert by index in a sorted set.");

    void IList<T>.RemoveAt(int index) {
      var item = this._set.ElementAt(index);
      this._set.Remove(item);
    }

    #endregion

    #region IList explicit implementation

    object? IList.this[int index] {
      get => this[index];
      set => throw new NotSupportedException("Cannot set element by index in a sorted set.");
    }

    bool IList.IsFixedSize => false;

    bool IList.IsReadOnly => false;

    int IList.Add(object? value) {
      this._set.Add((T)value!);
      return this.IndexOf((T)value!);
    }

    bool IList.Contains(object value) => value is T t && this._set.Contains(t);

    int IList.IndexOf(object value) => value is T t ? this.IndexOf(t) : -1;

    void IList.Insert(int index, object value) => throw new NotSupportedException("Cannot insert by index in a sorted set.");

    void IList.Remove(object value) {
      if (value is T t)
        this._set.Remove(t);
    }

    void IList.RemoveAt(int index) {
      var item = this._set.ElementAt(index);
      this._set.Remove(item);
    }

    #endregion

    #region ICollection explicit implementation

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)this._set).CopyTo(array, index);

    #endregion
  }
}

/// <summary>
/// Provides a set of static methods for creating immutable sorted sets.
/// </summary>
public static class ImmutableSortedSet {
  /// <summary>
  /// Creates an empty immutable sorted set.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>() =>
    ImmutableSortedSet<T>.Empty;

  /// <summary>
  /// Creates an empty immutable sorted set with the specified comparer.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer) =>
    new(new(comparer), comparer);

  /// <summary>
  /// Creates an immutable sorted set with the specified item.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>(T item) =>
    new(new() { item }, Comparer<T>.Default);

  /// <summary>
  /// Creates an immutable sorted set with the specified items.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>(params T[] items) =>
    new(new(items), Comparer<T>.Default);

  /// <summary>
  /// Creates an immutable sorted set with the specified comparer and item.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, T item) =>
    new(new(comparer) { item }, comparer);

  /// <summary>
  /// Creates an immutable sorted set with the specified comparer and items.
  /// </summary>
  public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, params T[] items) =>
    new(new(items, comparer), comparer);

  /// <summary>
  /// Creates a new builder for creating immutable sorted sets.
  /// </summary>
  public static ImmutableSortedSet<T>.Builder CreateBuilder<T>() =>
    new(new(), Comparer<T>.Default);

  /// <summary>
  /// Creates a new builder for creating immutable sorted sets with the specified comparer.
  /// </summary>
  public static ImmutableSortedSet<T>.Builder CreateBuilder<T>(IComparer<T> comparer) =>
    new(new(comparer), comparer);

  /// <summary>
  /// Creates an immutable sorted set from the specified items.
  /// </summary>
  public static ImmutableSortedSet<T> CreateRange<T>(IEnumerable<T> items) =>
    new(new(items), Comparer<T>.Default);

  /// <summary>
  /// Creates an immutable sorted set from the specified items with the specified comparer.
  /// </summary>
  public static ImmutableSortedSet<T> CreateRange<T>(IComparer<T> comparer, IEnumerable<T> items) =>
    new(new(items, comparer), comparer);

  extension<T>(IEnumerable<T> source)
  {
    /// <summary>
    /// Enumerates a sequence and produces an immutable sorted set of its contents.
    /// </summary>
    public ImmutableSortedSet<T> ToImmutableSortedSet() =>
      CreateRange(source);

    /// <summary>
    /// Enumerates a sequence and produces an immutable sorted set of its contents with the specified comparer.
    /// </summary>
    public ImmutableSortedSet<T> ToImmutableSortedSet(IComparer<T> comparer) =>
      CreateRange(comparer, source);
  }
}

#endif
