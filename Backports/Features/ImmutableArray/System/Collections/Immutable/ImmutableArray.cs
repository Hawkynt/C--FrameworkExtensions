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
/// Represents an immutable array.
/// </summary>
/// <typeparam name="T">The type of element stored in the array.</typeparam>
[DebuggerDisplay("Length = {Length}")]
public readonly struct ImmutableArray<T> : IReadOnlyList<T>, IList<T>, IEquatable<ImmutableArray<T>>, IStructuralComparable, IStructuralEquatable {

  /// <summary>
  /// Gets an empty immutable array.
  /// </summary>
  public static readonly ImmutableArray<T> Empty = new([]);

  private readonly T[]? _array;

  internal ImmutableArray(T[] array) => this._array = array;

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      this._ThrowIfDefault();
      return this._array![index];
    }
  }

  /// <summary>
  /// Gets the number of elements in the array.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._array?.Length ?? 0;
  }

  /// <summary>
  /// Gets a value indicating whether this array was declared but not initialized.
  /// </summary>
  public bool IsDefault {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._array == null;
  }

  /// <summary>
  /// Gets a value indicating whether this array is empty or was not initialized.
  /// </summary>
  public bool IsDefaultOrEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._array == null || this._array.Length == 0;
  }

  /// <summary>
  /// Gets a value indicating whether this array is empty.
  /// </summary>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._array?.Length == 0;
  }

  int IReadOnlyCollection<T>.Count => this.Length;
  int ICollection<T>.Count => this.Length;
  bool ICollection<T>.IsReadOnly => true;

  T IList<T>.this[int index] {
    get => this[index];
    set => throw new NotSupportedException("Collection is read-only.");
  }

  /// <summary>
  /// Adds the specified item to the end of the array.
  /// </summary>
  public ImmutableArray<T> Add(T item) {
    this._ThrowIfDefault();
    var newArray = new T[this._array!.Length + 1];
    Array.Copy(this._array, newArray, this._array.Length);
    newArray[this._array.Length] = item;
    return new(newArray);
  }

  /// <summary>
  /// Adds the specified items to the end of the array.
  /// </summary>
  public ImmutableArray<T> AddRange(IEnumerable<T> items) {
    this._ThrowIfDefault();
    ArgumentNullException.ThrowIfNull(items);
    var list = items.ToList();
    if (list.Count == 0)
      return this;

    var newArray = new T[this._array!.Length + list.Count];
    Array.Copy(this._array, newArray, this._array.Length);
    list.CopyTo(newArray, this._array.Length);
    return new(newArray);
  }

  /// <summary>
  /// Adds the specified items to the end of the array.
  /// </summary>
  public ImmutableArray<T> AddRange(ImmutableArray<T> items) {
    this._ThrowIfDefault();
    items._ThrowIfDefault();
    if (items.Length == 0)
      return this;
    if (this.Length == 0)
      return items;

    var newArray = new T[this._array!.Length + items._array!.Length];
    Array.Copy(this._array, newArray, this._array.Length);
    Array.Copy(items._array, 0, newArray, this._array.Length, items._array.Length);
    return new(newArray);
  }

  /// <summary>
  /// Returns an array with all the elements removed.
  /// </summary>
  public ImmutableArray<T> Clear() => Empty;

  /// <summary>
  /// Determines whether the array contains the specified item.
  /// </summary>
  public bool Contains(T item) {
    this._ThrowIfDefault();
    return Array.IndexOf(this._array!, item) >= 0;
  }

  /// <summary>
  /// Copies the elements to the specified array.
  /// </summary>
  public void CopyTo(T[] destination) {
    this._ThrowIfDefault();
    Array.Copy(this._array!, destination, this._array!.Length);
  }

  /// <summary>
  /// Copies the elements to the specified array.
  /// </summary>
  public void CopyTo(T[] destination, int destinationIndex) {
    this._ThrowIfDefault();
    Array.Copy(this._array!, 0, destination, destinationIndex, this._array!.Length);
  }

  /// <summary>
  /// Copies the elements to the specified array.
  /// </summary>
  public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length) {
    this._ThrowIfDefault();
    Array.Copy(this._array!, sourceIndex, destination, destinationIndex, length);
  }

  /// <summary>
  /// Searches for the specified item and returns the zero-based index.
  /// </summary>
  public int IndexOf(T item) {
    this._ThrowIfDefault();
    return Array.IndexOf(this._array!, item);
  }

  /// <summary>
  /// Searches for the specified item and returns the zero-based index.
  /// </summary>
  public int IndexOf(T item, int startIndex) {
    this._ThrowIfDefault();
    return Array.IndexOf(this._array!, item, startIndex);
  }

  /// <summary>
  /// Searches for the specified item and returns the zero-based index.
  /// </summary>
  public int IndexOf(T item, int startIndex, int count) {
    this._ThrowIfDefault();
    return Array.IndexOf(this._array!, item, startIndex, count);
  }

  /// <summary>
  /// Inserts the specified item at the specified index.
  /// </summary>
  public ImmutableArray<T> Insert(int index, T item) {
    this._ThrowIfDefault();
    if (index < 0 || index > this._array!.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var newArray = new T[this._array.Length + 1];
    if (index > 0)
      Array.Copy(this._array, 0, newArray, 0, index);
    newArray[index] = item;
    if (index < this._array.Length)
      Array.Copy(this._array, index, newArray, index + 1, this._array.Length - index);
    return new(newArray);
  }

  /// <summary>
  /// Inserts the specified items at the specified index.
  /// </summary>
  public ImmutableArray<T> InsertRange(int index, IEnumerable<T> items) {
    this._ThrowIfDefault();
    ArgumentNullException.ThrowIfNull(items);
    if (index < 0 || index > this._array!.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var list = items.ToList();
    if (list.Count == 0)
      return this;

    var newArray = new T[this._array.Length + list.Count];
    if (index > 0)
      Array.Copy(this._array, 0, newArray, 0, index);
    list.CopyTo(newArray, index);
    if (index < this._array.Length)
      Array.Copy(this._array, index, newArray, index + list.Count, this._array.Length - index);
    return new(newArray);
  }

  /// <summary>
  /// Searches for the specified item and returns the zero-based index of the last occurrence.
  /// </summary>
  public int LastIndexOf(T item) {
    this._ThrowIfDefault();
    return Array.LastIndexOf(this._array!, item);
  }

  /// <summary>
  /// Removes the first occurrence of the specified item.
  /// </summary>
  public ImmutableArray<T> Remove(T item) {
    var index = this.IndexOf(item);
    return index < 0 ? this : this.RemoveAt(index);
  }

  /// <summary>
  /// Removes all the items that match the specified predicate.
  /// </summary>
  public ImmutableArray<T> RemoveAll(Predicate<T> match) {
    this._ThrowIfDefault();
    ArgumentNullException.ThrowIfNull(match);

    var list = new List<T>(this._array!.Length);
    foreach (var item in this._array)
      if (!match(item))
        list.Add(item);

    return list.Count == this._array.Length ? this : new([.. list]);
  }

  /// <summary>
  /// Removes the item at the specified index.
  /// </summary>
  public ImmutableArray<T> RemoveAt(int index) {
    this._ThrowIfDefault();
    if (index < 0 || index >= this._array!.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    if (this._array.Length == 1)
      return Empty;

    var newArray = new T[this._array.Length - 1];
    if (index > 0)
      Array.Copy(this._array, 0, newArray, 0, index);
    if (index < this._array.Length - 1)
      Array.Copy(this._array, index + 1, newArray, index, this._array.Length - index - 1);
    return new(newArray);
  }

  /// <summary>
  /// Removes items at the specified range.
  /// </summary>
  public ImmutableArray<T> RemoveRange(int index, int count) {
    this._ThrowIfDefault();
    if (index < 0 || index >= this._array!.Length)
      throw new ArgumentOutOfRangeException(nameof(index));
    if (count < 0 || index + count > this._array.Length)
      throw new ArgumentOutOfRangeException(nameof(count));

    if (count == 0)
      return this;
    if (count == this._array.Length)
      return Empty;

    var newArray = new T[this._array.Length - count];
    if (index > 0)
      Array.Copy(this._array, 0, newArray, 0, index);
    if (index + count < this._array.Length)
      Array.Copy(this._array, index + count, newArray, index, this._array.Length - index - count);
    return new(newArray);
  }

  /// <summary>
  /// Replaces the item at the specified index.
  /// </summary>
  public ImmutableArray<T> SetItem(int index, T item) {
    this._ThrowIfDefault();
    if (index < 0 || index >= this._array!.Length)
      throw new ArgumentOutOfRangeException(nameof(index));

    var newArray = new T[this._array.Length];
    Array.Copy(this._array, newArray, this._array.Length);
    newArray[index] = item;
    return new(newArray);
  }

  /// <summary>
  /// Returns a builder that can be used to create an immutable array.
  /// </summary>
  public Builder ToBuilder() {
    this._ThrowIfDefault();
    var builder = new Builder(this._array!.Length);
    builder.AddRange(this);
    return builder;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the array.
  /// </summary>
  public Enumerator GetEnumerator() {
    this._ThrowIfDefault();
    return new(this._array!);
  }

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(this._array ?? [])).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => (this._array ?? []).GetEnumerator();

  void IList<T>.Insert(int index, T item) => throw new NotSupportedException("Collection is read-only.");
  void IList<T>.RemoveAt(int index) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<T>.Clear() => throw new NotSupportedException("Collection is read-only.");
  bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Collection is read-only.");

  void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this.CopyTo(array, arrayIndex);

  int IList<T>.IndexOf(T item) => this.IndexOf(item);
  bool ICollection<T>.Contains(T item) => this.Contains(item);

  /// <inheritdoc/>
  public bool Equals(ImmutableArray<T> other) => this._array == other._array;

  /// <inheritdoc/>
  public override bool Equals(object? obj) => obj is ImmutableArray<T> other && this.Equals(other);

  /// <inheritdoc/>
  public override int GetHashCode() => this._array?.GetHashCode() ?? 0;

  int IStructuralComparable.CompareTo(object? other, IComparer comparer) {
    if (other is not ImmutableArray<T> otherArray)
      throw new ArgumentException("Object is not an ImmutableArray<T>.", nameof(other));

    var thisArray = this._array ?? [];
    var thatArray = otherArray._array ?? [];
    var minLength = Math.Min(thisArray.Length, thatArray.Length);
    for (var i = 0; i < minLength; ++i) {
      var result = comparer.Compare(thisArray[i], thatArray[i]);
      if (result != 0)
        return result;
    }
    return thisArray.Length.CompareTo(thatArray.Length);
  }

  bool IStructuralEquatable.Equals(object? other, IEqualityComparer comparer) {
    if (other is not ImmutableArray<T> otherArray)
      return false;

    var thisArray = this._array ?? [];
    var thatArray = otherArray._array ?? [];
    if (thisArray.Length != thatArray.Length)
      return false;

    for (var i = 0; i < thisArray.Length; ++i)
      if (!comparer.Equals(thisArray[i], thatArray[i]))
        return false;

    return true;
  }

  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var array = this._array ?? [];
    var hash = 0;
    for (var i = 0; i < array.Length; ++i)
      hash = (hash * 31) + comparer.GetHashCode(array[i]);

    return hash;
  }

  /// <summary>
  /// Determines whether two arrays are equal.
  /// </summary>
  public static bool operator ==(ImmutableArray<T> left, ImmutableArray<T> right) => left.Equals(right);

  /// <summary>
  /// Determines whether two arrays are not equal.
  /// </summary>
  public static bool operator !=(ImmutableArray<T> left, ImmutableArray<T> right) => !left.Equals(right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _ThrowIfDefault() {
    if (this._array == null)
      throw new InvalidOperationException("This operation cannot be performed on a default instance of ImmutableArray<T>.");
  }

  /// <summary>
  /// Enumerates the elements of an <see cref="ImmutableArray{T}"/>.
  /// </summary>
  public struct Enumerator {
    private readonly T[] _array;
    private int _index;

    internal Enumerator(T[] array) {
      this._array = array;
      this._index = -1;
    }

    /// <summary>
    /// Gets the element at the current position.
    /// </summary>
    public readonly T Current => this._array[this._index];

    /// <summary>
    /// Advances the enumerator to the next element.
    /// </summary>
    public bool MoveNext() => ++this._index < this._array.Length;
  }

  /// <summary>
  /// A builder for creating immutable arrays.
  /// </summary>
  public sealed class Builder : IList<T>, IReadOnlyList<T> {
    private T[] _elements;
    private int _count;

    internal Builder(int capacity) {
      this._elements = capacity > 0 ? new T[capacity] : [];
      this._count = 0;
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    public T this[int index] {
      get {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._count);
        return this._elements[index];
      }
      set {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._count);
        this._elements[index] = value;
      }
    }

    /// <summary>
    /// Gets the number of elements in the builder.
    /// </summary>
    public int Count => this._count;

    /// <summary>
    /// Gets or sets the capacity of the builder.
    /// </summary>
    public int Capacity {
      get => this._elements.Length;
      set {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, this._count);
        if (value == this._elements.Length)
          return;

        var newArray = new T[value];
        Array.Copy(this._elements, newArray, this._count);
        this._elements = newArray;
      }
    }

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Adds an item to the builder.
    /// </summary>
    public void Add(T item) {
      this._EnsureCapacity(this._count + 1);
      this._elements[this._count++] = item;
    }

    /// <summary>
    /// Adds items to the builder.
    /// </summary>
    public void AddRange(IEnumerable<T> items) {
      ArgumentNullException.ThrowIfNull(items);
      foreach (var item in items)
        this.Add(item);
    }

    /// <summary>
    /// Adds items to the builder.
    /// </summary>
    public void AddRange(ImmutableArray<T> items) {
      if (items.IsDefault)
        return;
      this._EnsureCapacity(this._count + items.Length);
      items.CopyTo(this._elements, this._count);
      this._count += items.Length;
    }

    /// <summary>
    /// Removes all items from the builder.
    /// </summary>
    public void Clear() {
      if (this._count <= 0)
        return;

      Array.Clear(this._elements, 0, this._count);
      this._count = 0;
    }

    /// <summary>
    /// Determines whether the builder contains the specified item.
    /// </summary>
    public bool Contains(T item) => this.IndexOf(item) >= 0;

    /// <summary>
    /// Copies the elements to the specified array.
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => Array.Copy(this._elements, 0, array, arrayIndex, this._count);

    /// <summary>
    /// Searches for the specified item and returns the zero-based index.
    /// </summary>
    public int IndexOf(T item) => Array.IndexOf(this._elements, item, 0, this._count);

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    public void Insert(int index, T item) {
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, this._count);

      this._EnsureCapacity(this._count + 1);
      if (index < this._count)
        Array.Copy(this._elements, index, this._elements, index + 1, this._count - index);
      
      this._elements[index] = item;
      ++this._count;
    }

    /// <summary>
    /// Removes the first occurrence of the specified item.
    /// </summary>
    public bool Remove(T item) {
      var index = this.IndexOf(item);
      if (index < 0)
        return false;

      this.RemoveAt(index);
      return true;
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    public void RemoveAt(int index) {
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._count);
      if (index < this._count - 1)
        Array.Copy(this._elements, index + 1, this._elements, index, this._count - index - 1);

      this._elements[--this._count] = default!;
    }

    /// <summary>
    /// Creates an immutable array from the builder.
    /// </summary>
    public ImmutableArray<T> ToImmutable() {
      if (this._count == 0)
        return Empty;
      var result = new T[this._count];
      Array.Copy(this._elements, result, this._count);
      return new(result);
    }

    /// <summary>
    /// Creates an immutable array from the builder and clears the builder.
    /// </summary>
    public ImmutableArray<T> MoveToImmutable() {
      var result = this.ToImmutable();
      this.Clear();
      return result;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the builder.
    /// </summary>
    public IEnumerator<T> GetEnumerator() {
      for (var i = 0; i < this._count; ++i)
        yield return this._elements[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private void _EnsureCapacity(int capacity) {
      if (this._elements.Length >= capacity)
        return;

      var newCapacity = Math.Max(this._elements.Length * 2, capacity);
      var newArray = new T[newCapacity];
      Array.Copy(this._elements, newArray, this._count);
      this._elements = newArray;
    }
  }
}

/// <summary>
/// Provides static methods for creating immutable arrays.
/// </summary>
public static class ImmutableArray {
  /// <summary>
  /// Creates an empty immutable array.
  /// </summary>
  public static ImmutableArray<T> Create<T>() => ImmutableArray<T>.Empty;

  /// <summary>
  /// Creates an immutable array with the specified item.
  /// </summary>
  public static ImmutableArray<T> Create<T>(T item) => new([item]);

  /// <summary>
  /// Creates an immutable array with the specified items.
  /// </summary>
  public static ImmutableArray<T> Create<T>(T item1, T item2) => new([item1, item2]);

  /// <summary>
  /// Creates an immutable array with the specified items.
  /// </summary>
  public static ImmutableArray<T> Create<T>(T item1, T item2, T item3) => new([item1, item2, item3]);

  /// <summary>
  /// Creates an immutable array with the specified items.
  /// </summary>
  public static ImmutableArray<T> Create<T>(T item1, T item2, T item3, T item4) => new([item1, item2, item3, item4]);

  /// <summary>
  /// Creates an immutable array with the specified items.
  /// </summary>
  public static ImmutableArray<T> Create<T>(params T[] items) {
    if (items == null || items.Length == 0)
      return ImmutableArray<T>.Empty;

    var copy = new T[items.Length];
    Array.Copy(items, copy, items.Length);
    return new(copy);
  }

  /// <summary>
  /// Creates an immutable array from the specified span.
  /// </summary>
  public static ImmutableArray<T> Create<T>(ReadOnlySpan<T> items) {
    if (items.Length == 0)
      return ImmutableArray<T>.Empty;

    var array = new T[items.Length];
    items.CopyTo(array);
    return new(array);
  }

  /// <summary>
  /// Creates an immutable array builder.
  /// </summary>
  public static ImmutableArray<T>.Builder CreateBuilder<T>() => new(8);

  /// <summary>
  /// Creates an immutable array builder with the specified capacity.
  /// </summary>
  public static ImmutableArray<T>.Builder CreateBuilder<T>(int initialCapacity) => new(initialCapacity);

  /// <summary>
  /// Creates an immutable array from the specified range.
  /// </summary>
  public static ImmutableArray<T> CreateRange<T>(IEnumerable<T> items) {
    ArgumentNullException.ThrowIfNull(items);
    var array = items.ToArray();
    return array.Length == 0 ? ImmutableArray<T>.Empty : new(array);
  }

  /// <summary>
  /// Converts the specified enumerable to an immutable array.
  /// </summary>
  public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerable<T> source) {
    ArgumentNullException.ThrowIfNull(source);
    if (source is ImmutableArray<T> existing)
      return existing;
    var array = source.ToArray();
    return array.Length == 0 ? ImmutableArray<T>.Empty : new(array);
  }

  /// <summary>
  /// Converts the specified builder to an immutable array.
  /// </summary>
  public static ImmutableArray<T> ToImmutableArray<T>(this ImmutableArray<T>.Builder builder) {
    ArgumentNullException.ThrowIfNull(builder);
    return builder.ToImmutable();
  }
}

#endif
