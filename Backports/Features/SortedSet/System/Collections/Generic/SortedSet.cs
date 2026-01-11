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

// SortedSet was added in .NET 4.0 together with ISet<T>
#if !SUPPORTS_ISET

using Guard;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

/// <summary>
/// Represents a collection of objects that is maintained in sorted order.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public class SortedSet<T>(IComparer<T>? comparer) : ICollection<T>, IEnumerable<T> {

  /// <summary>Gets the <see cref="IComparer{T}"/> object that is used to order the values in the <see cref="SortedSet{T}"/>.</summary>
  public IComparer<T> Comparer { get; } = comparer ?? Comparer<T>.Default;

  private readonly SortedDictionary<T, byte> _items = new(comparer ?? Comparer<T>.Default);

  #region Constructors

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public SortedSet() : this((IComparer<T>?)null) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public SortedSet(IEnumerable<T> collection) : this(collection, null) { }

  public SortedSet(IEnumerable<T> collection, IComparer<T>? comparer) : this(comparer) {
    ArgumentNullException.ThrowIfNull(collection);

    foreach (var item in collection) {
      if (!this._items.ContainsKey(item))
        this._items.Add(item, 0);
    }
  }

  #endregion

  #region ICollection<T> implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  void ICollection<T>.Add(T item) => this.Add(item);

  /// <summary>Removes all elements from the <see cref="SortedSet{T}"/>.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Clear() => this._items.Clear();

  /// <summary>Determines whether the <see cref="SortedSet{T}"/> contains a specific value.</summary>
  /// <param name="item">The object to locate in the <see cref="SortedSet{T}"/>.</param>
  /// <returns>true if the <see cref="SortedSet{T}"/> contains the specified value; otherwise, false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Contains(T item) => this._items.ContainsKey(item);

  /// <summary>Copies the complete <see cref="SortedSet{T}"/> to a compatible one-dimensional array, starting at the specified array index.</summary>
  /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="SortedSet{T}"/>.</param>
  /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
  public void CopyTo(T[] array, int arrayIndex) {
    ArgumentNullException.ThrowIfNull(array);
    if (arrayIndex < 0 || arrayIndex > array.Length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(arrayIndex));
    if (array.Length - arrayIndex < this.Count)
      AlwaysThrow.ArgumentException(nameof(array), "Array is too small");

    foreach (var item in this._items.Keys)
      array[arrayIndex++] = item;
  }

  /// <summary>Removes a specific object from the <see cref="SortedSet{T}"/>.</summary>
  /// <param name="item">The object to remove from the <see cref="SortedSet{T}"/>.</param>
  /// <returns>true if item was successfully removed; otherwise, false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Remove(T item) => this._items.Remove(item);

  /// <summary>Gets the number of elements contained in the <see cref="SortedSet{T}"/>.</summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._items.Count;
  }

  bool ICollection<T>.IsReadOnly {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => false;
  }

  #endregion

  #region IEnumerable implementation

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<T> GetEnumerator() => this._items.Keys.GetEnumerator();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #endregion

  #region SortedSet methods

  /// <summary>Adds an element to the set and returns a value that indicates if it was successfully added.</summary>
  /// <param name="item">The element to add to the set.</param>
  /// <returns>true if item is added to the set; otherwise, false.</returns>
  public bool Add(T item) {
    if (this._items.ContainsKey(item))
      return false;

    this._items.Add(item, 0);
    return true;
  }
  
  /// <summary>Returns the minimum value in the set, as defined by the comparer.</summary>
  public T? Min {
    get {
      if (this._items.Count == 0)
        return default;
      using var enumerator = this._items.Keys.GetEnumerator();
      return enumerator.MoveNext() ? enumerator.Current : default;
    }
  }

  /// <summary>Returns the maximum value in the set, as defined by the comparer.</summary>
  public T? Max {
    get {
      if (this._items.Count == 0)
        return default;
      T? max = default;
      foreach (var item in this._items.Keys)
        max = item;
      return max;
    }
  }

  /// <summary>Returns a view of a subset in a <see cref="SortedSet{T}"/>.</summary>
  /// <param name="lowerValue">The lowest desired value in the view.</param>
  /// <param name="upperValue">The highest desired value in the view.</param>
  /// <returns>A subset view that contains only the values in the specified range.</returns>
  public SortedSet<T> GetViewBetween(T lowerValue, T upperValue) {
    if (this.Comparer.Compare(lowerValue, upperValue) > 0)
      AlwaysThrow.ArgumentException(nameof(lowerValue), "lowerValue must be less than or equal to upperValue");

    var result = new SortedSet<T>(this.Comparer);
    foreach (var item in this._items.Keys) {
      if (this.Comparer.Compare(item, lowerValue) >= 0 && this.Comparer.Compare(item, upperValue) <= 0)
        result.Add(item);
    }
    return result;
  }

  /// <summary>Modifies the current <see cref="SortedSet{T}"/> object so that it contains all elements that are present in itself, the specified collection, or both.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  public void UnionWith(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    foreach (var item in other)
      this.Add(item);
  }

  /// <summary>Modifies the current <see cref="SortedSet{T}"/> object so that it contains only elements that are also in a specified collection.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  public void IntersectWith(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0 || ReferenceEquals(other, this))
      return;

    if (other is ICollection<T> { Count: 0 }) {
      this.Clear();
      return;
    }

    var otherSet = new SortedSet<T>(other, this.Comparer);
    var toRemove = new List<T>();
    foreach (var item in this._items.Keys)
      if (!otherSet.Contains(item))
        toRemove.Add(item);

    foreach (var item in toRemove)
      this._items.Remove(item);
  }

  /// <summary>Removes all elements in the specified collection from the current <see cref="SortedSet{T}"/> object.</summary>
  /// <param name="other">The collection of items to remove from the <see cref="SortedSet{T}"/> object.</param>
  public void ExceptWith(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0)
      return;

    if (ReferenceEquals(other, this)) {
      this.Clear();
      return;
    }

    foreach (var item in other)
      this.Remove(item);
  }

  /// <summary>Modifies the current <see cref="SortedSet{T}"/> object so that it contains only elements that are present either in the current object or in the specified collection, but not both.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  public void SymmetricExceptWith(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0) {
      this.UnionWith(other);
      return;
    }

    if (ReferenceEquals(other, this)) {
      this.Clear();
      return;
    }

    var toRemove = new List<T>();
    foreach (var item in other) {
      if (!this.Add(item))
        toRemove.Add(item);
    }

    foreach (var item in toRemove)
      this.Remove(item);
  }

  /// <summary>Determines whether a <see cref="SortedSet{T}"/> object is a subset of the specified collection.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  /// <returns>true if the current <see cref="SortedSet{T}"/> object is a subset of other; otherwise, false.</returns>
  public bool IsSubsetOf(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0)
      return true;

    var otherSet = other as SortedSet<T> ?? new SortedSet<T>(other, this.Comparer);
    return this._items.Keys.All(otherSet.Contains);
  }

  /// <summary>Determines whether a <see cref="SortedSet{T}"/> object is a superset of the specified collection.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  /// <returns>true if the current <see cref="SortedSet{T}"/> object is a superset of other; otherwise, false.</returns>
  public bool IsSupersetOf(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    return other.All(this.Contains);
  }

  /// <summary>Determines whether a <see cref="SortedSet{T}"/> object is a proper subset of the specified collection.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  /// <returns>true if the current <see cref="SortedSet{T}"/> object is a proper subset of other; otherwise, false.</returns>
  public bool IsProperSubsetOf(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    var otherSet = other as SortedSet<T> ?? new SortedSet<T>(other, this.Comparer);
    return this.Count < otherSet.Count && this.IsSubsetOf(otherSet);
  }

  /// <summary>Determines whether a <see cref="SortedSet{T}"/> object is a proper superset of the specified collection.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  /// <returns>true if the current <see cref="SortedSet{T}"/> object is a proper superset of other; otherwise, false.</returns>
  public bool IsProperSupersetOf(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0)
      return false;

    var otherSet = other as SortedSet<T> ?? new SortedSet<T>(other, this.Comparer);
    return this.Count > otherSet.Count && this.IsSupersetOf(otherSet);
  }

  /// <summary>Determines whether the current <see cref="SortedSet{T}"/> object and a specified collection share common elements.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/> object.</param>
  /// <returns>true if the two collections share at least one common element; otherwise, false.</returns>
  public bool Overlaps(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    if (this.Count == 0)
      return false;

    return other.Any(this.Contains);
  }

  /// <summary>Determines whether the current <see cref="SortedSet{T}"/> and a specified collection contain the same elements.</summary>
  /// <param name="other">The collection to compare to the current <see cref="SortedSet{T}"/>.</param>
  /// <returns>true if the current <see cref="SortedSet{T}"/> is equal to other; otherwise, false.</returns>
  public bool SetEquals(IEnumerable<T> other) {
    ArgumentNullException.ThrowIfNull(other);

    var otherSet = other as SortedSet<T> ?? new SortedSet<T>(other, this.Comparer);
    return this.Count == otherSet.Count && this.IsSubsetOf(otherSet);
  }

  /// <summary>Copies the complete <see cref="SortedSet{T}"/> to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
  /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="SortedSet{T}"/>.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(T[] array) => this.CopyTo(array, 0, this.Count);

  /// <summary>Copies a specified number of elements from <see cref="SortedSet{T}"/> to a compatible one-dimensional array, starting at the specified array index.</summary>
  /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="SortedSet{T}"/>.</param>
  /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
  /// <param name="count">The number of elements to copy.</param>
  public void CopyTo(T[] array, int arrayIndex, int count) {
    ArgumentNullException.ThrowIfNull(array);
    if (arrayIndex < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(arrayIndex));
    if (count < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(count));
    if (arrayIndex > array.Length || count > array.Length - arrayIndex)
      AlwaysThrow.ArgumentException(nameof(array), "Array is too small");

    foreach (var item in this._items.Keys) {
      if (count <= 0)
        break;
      array[arrayIndex++] = item;
      --count;
    }
  }

  /// <summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="SortedSet{T}"/> collection.</summary>
  /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
  /// <returns>The number of elements that were removed from the <see cref="SortedSet{T}"/> collection.</returns>
  public int RemoveWhere(Predicate<T> match) {
    ArgumentNullException.ThrowIfNull(match);

    var toRemove = new List<T>();
    foreach (var item in this._items.Keys)
      if (match(item))
        toRemove.Add(item);

    foreach (var item in toRemove)
      this._items.Remove(item);

    return toRemove.Count;
  }

  /// <summary>Returns an <see cref="IEnumerable{T}"/> that iterates over the <see cref="SortedSet{T}"/> in reverse order.</summary>
  /// <returns>An enumerator that iterates over the <see cref="SortedSet{T}"/> in reverse order.</returns>
  public IEnumerable<T> Reverse() {
    var items = this._items.Keys.ToArray();
    for (var i = items.Length - 1; i >= 0; --i)
      yield return items[i];
  }

  #endregion
}

#endif
