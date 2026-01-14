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

#if !OFFICIAL_IMMUTABLE_COLLECTIONS

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Immutable;

/// <summary>
/// Represents an immutable list of elements.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public sealed class ImmutableList<T> : IImmutableList<T>, IReadOnlyList<T>, IList<T>, IList {
  /// <summary>
  /// Gets an empty immutable list.
  /// </summary>
  public static readonly ImmutableList<T> Empty = new([]);

  private readonly T[] _items;

  private ImmutableList(T[] items) => this._items = items;

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._items[index];
  }

  /// <summary>
  /// Gets the number of elements in the list.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._items.Length;
  }

  /// <summary>
  /// Gets a value indicating whether the list is empty.
  /// </summary>
  public bool IsEmpty => this._items.Length == 0;

  T IList<T>.this[int index] {
    get => this[index];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  object? IList.this[int index] {
    get => this[index];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  bool ICollection<T>.IsReadOnly => true;
  bool IList.IsReadOnly => true;
  bool IList.IsFixedSize => true;
  bool ICollection.IsSynchronized => true;
  object ICollection.SyncRoot => this;

  /// <summary>
  /// Adds the specified item to the end of the list.
  /// </summary>
  public ImmutableList<T> Add(T value) {
    var newItems = new T[this._items.Length + 1];
    Array.Copy(this._items, newItems, this._items.Length);
    newItems[this._items.Length] = value;
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.Add(T value) => this.Add(value);

  /// <summary>
  /// Adds the specified items to the end of the list.
  /// </summary>
  public ImmutableList<T> AddRange(IEnumerable<T> items) {
    ArgumentNullException.ThrowIfNull(items);
    var list = items.ToList();
    if (list.Count == 0)
      return this;

    var newItems = new T[this._items.Length + list.Count];
    Array.Copy(this._items, newItems, this._items.Length);
    list.CopyTo(newItems, this._items.Length);
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items) => this.AddRange(items);

  /// <summary>
  /// Returns an empty list.
  /// </summary>
  public ImmutableList<T> Clear() => Empty;

  IImmutableList<T> IImmutableList<T>.Clear() => this.Clear();

  /// <summary>
  /// Determines whether the list contains the specified item.
  /// </summary>
  public bool Contains(T value) => this.IndexOf(value) >= 0;

  /// <summary>
  /// Searches for the specified item and returns the zero-based index.
  /// </summary>
  public int IndexOf(T item) => this.IndexOf(item, 0, this._items.Length, EqualityComparer<T>.Default);

  /// <summary>
  /// Searches for the specified item and returns the zero-based index.
  /// </summary>
  public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) {
    equalityComparer ??= EqualityComparer<T>.Default;
    var end = index + count;
    for (var i = index; i < end; ++i)
      if (equalityComparer.Equals(this._items[i], item))
        return i;
    return -1;
  }

  /// <summary>
  /// Inserts the specified item at the specified index.
  /// </summary>
  public ImmutableList<T> Insert(int index, T element) {
    if (index < 0 || index > this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var newItems = new T[this._items.Length + 1];
    if (index > 0)
      Array.Copy(this._items, 0, newItems, 0, index);
    newItems[index] = element;
    if (index < this._items.Length)
      Array.Copy(this._items, index, newItems, index + 1, this._items.Length - index);
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.Insert(int index, T element) => this.Insert(index, element);

  /// <summary>
  /// Inserts the specified items at the specified index.
  /// </summary>
  public ImmutableList<T> InsertRange(int index, IEnumerable<T> items) {
    ArgumentNullException.ThrowIfNull(items);
    if (index < 0 || index > this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var list = items.ToList();
    if (list.Count == 0)
      return this;

    var newItems = new T[this._items.Length + list.Count];
    if (index > 0)
      Array.Copy(this._items, 0, newItems, 0, index);
    list.CopyTo(newItems, index);
    if (index < this._items.Length)
      Array.Copy(this._items, index, newItems, index + list.Count, this._items.Length - index);
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items) => this.InsertRange(index, items);

  /// <summary>
  /// Searches for the specified item and returns the zero-based index of the last occurrence.
  /// </summary>
  public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) {
    equalityComparer ??= EqualityComparer<T>.Default;
    var end = index - count + 1;
    for (var i = index; i >= end; --i)
      if (equalityComparer.Equals(this._items[i], item))
        return i;
    return -1;
  }

  /// <summary>
  /// Removes the first occurrence of the specified item.
  /// </summary>
  public ImmutableList<T> Remove(T value) => this.Remove(value, EqualityComparer<T>.Default);

  /// <summary>
  /// Removes the first occurrence of the specified item.
  /// </summary>
  public ImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer) {
    var index = this.IndexOf(value, 0, this._items.Length, equalityComparer);
    return index < 0 ? this : this.RemoveAt(index);
  }

  IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T>? equalityComparer)
    => this.Remove(value, equalityComparer);

  /// <summary>
  /// Removes all items that match the predicate.
  /// </summary>
  public ImmutableList<T> RemoveAll(Predicate<T> match) {
    ArgumentNullException.ThrowIfNull(match);

    var kept = new List<T>(this._items.Length);
    foreach (var item in this._items)
      if (!match(item))
        kept.Add(item);

    return kept.Count == this._items.Length ? this : new([.. kept]);
  }

  IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match) => this.RemoveAll(match);

  /// <summary>
  /// Removes the item at the specified index.
  /// </summary>
  public ImmutableList<T> RemoveAt(int index) {
    if (index < 0 || index >= this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    if (this._items.Length == 1)
      return Empty;

    var newItems = new T[this._items.Length - 1];
    if (index > 0)
      Array.Copy(this._items, 0, newItems, 0, index);
    if (index < this._items.Length - 1)
      Array.Copy(this._items, index + 1, newItems, index, this._items.Length - index - 1);
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.RemoveAt(int index) => this.RemoveAt(index);

  /// <summary>
  /// Removes items in the specified range.
  /// </summary>
  public ImmutableList<T> RemoveRange(int index, int count) {
    if (index < 0 || index >= this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(index));
    if (count < 0 || index + count > this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(count));

    if (count == 0)
      return this;
    if (count == this._items.Length)
      return Empty;

    var newItems = new T[this._items.Length - count];
    if (index > 0)
      Array.Copy(this._items, 0, newItems, 0, index);
    if (index + count < this._items.Length)
      Array.Copy(this._items, index + count, newItems, index, this._items.Length - index - count);
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count) => this.RemoveRange(index, count);

  /// <summary>
  /// Removes the specified items from the list.
  /// </summary>
  public ImmutableList<T> RemoveRange(IEnumerable<T> items) => this.RemoveRange(items, EqualityComparer<T>.Default);

  /// <summary>
  /// Removes the specified items from the list.
  /// </summary>
  public ImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) {
    ArgumentNullException.ThrowIfNull(items);
    equalityComparer ??= EqualityComparer<T>.Default;

    var toRemove = new HashSet<T>(items, equalityComparer);
    if (toRemove.Count == 0)
      return this;

    var kept = new List<T>(this._items.Length);
    foreach (var item in this._items)
      if (!toRemove.Contains(item))
        kept.Add(item);

    return kept.Count == this._items.Length ? this : new([.. kept]);
  }

  IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
    => this.RemoveRange(items, equalityComparer);

  /// <summary>
  /// Replaces the first occurrence of the old value with the new value.
  /// </summary>
  public ImmutableList<T> Replace(T oldValue, T newValue) => this.Replace(oldValue, newValue, EqualityComparer<T>.Default);

  /// <summary>
  /// Replaces the first occurrence of the old value with the new value.
  /// </summary>
  public ImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) {
    var index = this.IndexOf(oldValue, 0, this._items.Length, equalityComparer);
    if (index < 0)
      throw new ArgumentException("Value not found.", nameof(oldValue));
    return this.SetItem(index, newValue);
  }

  IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
    => this.Replace(oldValue, newValue, equalityComparer);

  /// <summary>
  /// Sets the item at the specified index.
  /// </summary>
  public ImmutableList<T> SetItem(int index, T value) {
    if (index < 0 || index >= this._items.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var newItems = new T[this._items.Length];
    Array.Copy(this._items, newItems, this._items.Length);
    newItems[index] = value;
    return new(newItems);
  }

  IImmutableList<T> IImmutableList<T>.SetItem(int index, T value) => this.SetItem(index, value);

  /// <summary>
  /// Copies the elements to the specified array.
  /// </summary>
  public void CopyTo(T[] array, int arrayIndex) => Array.Copy(this._items, 0, array, arrayIndex, this._items.Length);

  /// <summary>
  /// Returns a builder for creating immutable lists.
  /// </summary>
  public Builder ToBuilder() {
    var builder = new Builder();
    builder.AddRange(this._items);
    return builder;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the list.
  /// </summary>
  public Enumerator GetEnumerator() => new(this._items);

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)this._items).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => this._items.GetEnumerator();

  void IList<T>.Insert(int index, T item) => throw new NotSupportedException("Collection is read-only.");
  void IList<T>.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<T>.Clear() => throw new NotSupportedException("Collection is read-only.");
  bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Collection is read-only.");

  int IList.Add(object? value) => throw new NotSupportedException("Collection is read-only.");
  void IList.Clear() => throw new NotSupportedException("Collection is read-only.");
  void IList.Insert(int index, object? value) => throw new NotSupportedException("Collection is read-only.");
  void IList.Remove(object? value) => throw new NotSupportedException("Collection is read-only.");
  void IList.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");

  bool IList.Contains(object? value) => value is T item && this.Contains(item);
  int IList.IndexOf(object? value) => value is T item ? this.IndexOf(item) : -1;

  void ICollection.CopyTo(Array array, int index) => Array.Copy(this._items, 0, array, index, this._items.Length);

  internal static ImmutableList<T> CreateFromArray(T[] array) {
    if (array.Length == 0)
      return Empty;
    var copy = new T[array.Length];
    Array.Copy(array, copy, array.Length);
    return new(copy);
  }

  /// <summary>
  /// Enumerates the elements of an <see cref="ImmutableList{T}"/>.
  /// </summary>
  public struct Enumerator : IEnumerator<T> {
    private readonly T[] _items;
    private int _index;

    internal Enumerator(T[] items) {
      this._items = items;
      this._index = -1;
    }

    /// <inheritdoc/>
    public readonly T Current => this._items[this._index];

    readonly object? IEnumerator.Current => this.Current;

    /// <inheritdoc/>
    public bool MoveNext() => ++this._index < this._items.Length;

    /// <inheritdoc/>
    public void Reset() => this._index = -1;

    /// <inheritdoc/>
    public readonly void Dispose() { }
  }

  /// <summary>
  /// A builder for creating immutable lists.
  /// </summary>
  public sealed class Builder : IList<T>, IReadOnlyList<T> {
    private readonly List<T> _items = [];

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    public T this[int index] {
      get => this._items[index];
      set => this._items[index] = value;
    }

    /// <summary>
    /// Gets the number of elements in the builder.
    /// </summary>
    public int Count => this._items.Count;

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Adds an item to the builder.
    /// </summary>
    public void Add(T item) => this._items.Add(item);

    /// <summary>
    /// Adds items to the builder.
    /// </summary>
    public void AddRange(IEnumerable<T> items) => this._items.AddRange(items);

    /// <summary>
    /// Removes all items from the builder.
    /// </summary>
    public void Clear() => this._items.Clear();

    /// <summary>
    /// Determines whether the builder contains the specified item.
    /// </summary>
    public bool Contains(T item) => this._items.Contains(item);

    /// <summary>
    /// Copies the elements to the specified array.
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => this._items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Searches for the specified item and returns the zero-based index.
    /// </summary>
    public int IndexOf(T item) => this._items.IndexOf(item);

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    public void Insert(int index, T item) => this._items.Insert(index, item);

    /// <summary>
    /// Removes the first occurrence of the specified item.
    /// </summary>
    public bool Remove(T item) => this._items.Remove(item);

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    public void RemoveAt(int index) => this._items.RemoveAt(index);

    /// <summary>
    /// Creates an immutable list from the builder.
    /// </summary>
    public ImmutableList<T> ToImmutable() => ImmutableList<T>.CreateFromArray([.. this._items]);

    /// <summary>
    /// Returns an enumerator that iterates through the builder.
    /// </summary>
    public List<T>.Enumerator GetEnumerator() => this._items.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this._items.GetEnumerator();
  }
}

/// <summary>
/// Provides static methods for creating immutable lists.
/// </summary>
public static class ImmutableList {
  /// <summary>
  /// Creates an empty immutable list.
  /// </summary>
  public static ImmutableList<T> Create<T>() => ImmutableList<T>.Empty;

  /// <summary>
  /// Creates an immutable list with the specified item.
  /// </summary>
  public static ImmutableList<T> Create<T>(T item) => ImmutableList<T>.CreateFromArray([item]);

  /// <summary>
  /// Creates an immutable list with the specified items.
  /// </summary>
  public static ImmutableList<T> Create<T>(params T[] items) {
    if (items == null || items.Length == 0)
      return ImmutableList<T>.Empty;
    return ImmutableList<T>.CreateFromArray(items);
  }

  /// <summary>
  /// Creates an immutable list from the specified range.
  /// </summary>
  public static ImmutableList<T> CreateRange<T>(IEnumerable<T> items) {
    ArgumentNullException.ThrowIfNull(items);
    return ImmutableList<T>.CreateFromArray(items.ToArray());
  }

  /// <summary>
  /// Creates an immutable list builder.
  /// </summary>
  public static ImmutableList<T>.Builder CreateBuilder<T>() => [];

  /// <summary>
  /// Converts the specified enumerable to an immutable list.
  /// </summary>
  public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source) {
    ArgumentNullException.ThrowIfNull(source);
    if (source is ImmutableList<T> existing)
      return existing;
    return ImmutableList<T>.CreateFromArray(source.ToArray());
  }

  /// <summary>
  /// Converts the specified builder to an immutable list.
  /// </summary>
  public static ImmutableList<T> ToImmutableList<T>(this ImmutableList<T>.Builder builder) {
    ArgumentNullException.ThrowIfNull(builder);
    return builder.ToImmutable();
  }
}

/// <summary>
/// Represents an immutable list interface.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public interface IImmutableList<T> : IReadOnlyList<T> {
  /// <summary>
  /// Adds an item to the list.
  /// </summary>
  IImmutableList<T> Add(T value);

  /// <summary>
  /// Adds items to the list.
  /// </summary>
  IImmutableList<T> AddRange(IEnumerable<T> items);

  /// <summary>
  /// Clears the list.
  /// </summary>
  IImmutableList<T> Clear();

  /// <summary>
  /// Searches for the specified item.
  /// </summary>
  int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer);

  /// <summary>
  /// Inserts an item at the specified index.
  /// </summary>
  IImmutableList<T> Insert(int index, T element);

  /// <summary>
  /// Inserts items at the specified index.
  /// </summary>
  IImmutableList<T> InsertRange(int index, IEnumerable<T> items);

  /// <summary>
  /// Searches for the last occurrence of the specified item.
  /// </summary>
  int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer);

  /// <summary>
  /// Removes the first occurrence of the specified item.
  /// </summary>
  IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer);

  /// <summary>
  /// Removes all items that match the predicate.
  /// </summary>
  IImmutableList<T> RemoveAll(Predicate<T> match);

  /// <summary>
  /// Removes the item at the specified index.
  /// </summary>
  IImmutableList<T> RemoveAt(int index);

  /// <summary>
  /// Removes items in the specified range.
  /// </summary>
  IImmutableList<T> RemoveRange(int index, int count);

  /// <summary>
  /// Removes the specified items from the list.
  /// </summary>
  IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer);

  /// <summary>
  /// Replaces the first occurrence of the old value with the new value.
  /// </summary>
  IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer);

  /// <summary>
  /// Sets the item at the specified index.
  /// </summary>
  IImmutableList<T> SetItem(int index, T value);
}

#endif
