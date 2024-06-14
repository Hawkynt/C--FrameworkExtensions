#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_HASHSET

using System.Diagnostics;
using System.Linq;


namespace System.Collections.Generic {
  [DebuggerDisplay("Count = {Count}")]
  public class HashSet<T> : ICollection<T> {
    /// <summary>
    ///   Wraps values to allow <see langword="null" /> values to be used as keys.
    /// </summary>
    private readonly struct Wrapper(T value, HashSet<T> parent) {
      private readonly T value = value;

      private IEqualityComparer<T> Comparer => parent.Comparer;

      public override int GetHashCode() => this.value == null ? 0 : this.Comparer.GetHashCode(this.value);

      #region Overrides of ValueType

      public override bool Equals(object obj) => obj is Wrapper w && this.Comparer.Equals(this.value, w.value);

      #endregion
    }

    private readonly Dictionary<Wrapper, T> _hashtable;

    #region Constructors

    public HashSet() : this((IEqualityComparer<T>)null) { }

    public HashSet(IEqualityComparer<T> comparer) => this.Comparer = comparer ?? EqualityComparer<T>.Default;

    public HashSet(int capacity) : this(capacity, null) { }

    public HashSet(IEnumerable<T> enumerable) : this(enumerable, null) { }

    public HashSet(IEnumerable<T> enumerable, IEqualityComparer<T> comparer) : this(comparer) {
      if (enumerable == null)
        throw new ArgumentNullException(nameof(enumerable));

      this._hashtable = new(enumerable is ICollection collection ? collection.Count : 10);
      foreach (var item in enumerable) {
        Wrapper wrapper = new(item, this);
        if (!this._hashtable.ContainsKey(wrapper))
          this._hashtable.Add(wrapper, item);
      }
    }

    public HashSet(int capacity, IEqualityComparer<T> comparer) : this(comparer) {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException(nameof(capacity));

      this._hashtable = new(capacity);
    }

    #endregion

    #region ICollection<T> methods

    void ICollection<T>.Add(T item) => this.Add(item);

    /// <summary>Removes all elements from the <see cref="HashSet{T}" /> object.</summary>
    public void Clear() => this._hashtable.Clear();

    /// <summary>Determines whether the <see cref="HashSet{T}" /> contains the specified element.</summary>
    /// <param name="item">The element to locate in the <see cref="HashSet{T}" /> object.</param>
    /// <returns>true if the <see cref="HashSet{T}" /> object contains the specified element; otherwise, false.</returns>
    public bool Contains(T item) => this._hashtable.ContainsKey(new(item, this));

    public bool Remove(T item) {
      Wrapper key = new(item, this);
      if (!this._hashtable.ContainsKey(key))
        return false;

      this._hashtable.Remove(key);
      return true;
    }

    /// <summary>Gets the number of elements that are contained in the set.</summary>
    public int Count => this._hashtable.Count;

    bool ICollection<T>.IsReadOnly => false;

    #endregion

    #region IEnumerable methods

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this._Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

    private IEnumerable<T> _Enumerate() {
      foreach (var kvp in this._hashtable)
        yield return kvp.Value;
    }

    #endregion

    #region HashSet methods

    /// <summary>Adds the specified element to the <see cref="HashSet{T}" />.</summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>true if the element is added to the <see cref="HashSet{T}" /> object; false if the element is already present.</returns>
    public bool Add(T item) {
      Wrapper wrapper = new(item, this);
      if (this._hashtable.ContainsKey(wrapper))
        return false;

      this._hashtable.Add(wrapper, item);
      return true;
    }

    /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">
    ///   The value from the set that the search found, or the default value of
    ///   <typeparamref name="T" /> when the search yielded no match.
    /// </param>
    /// <returns>A value indicating whether the search was successful.</returns>
    /// <remarks>
    ///   This can be useful when you want to reuse a previously stored reference instead of
    ///   a newly constructed one (so that more sharing of references can occur) or to look up
    ///   a value that has more complete data than the value you currently have, although their
    ///   comparer functions indicate they are equal.
    /// </remarks>
    public bool TryGetValue(T equalValue, out T actualValue)
      => this._hashtable.TryGetValue(new(equalValue, this), out actualValue);

    /// <summary>
    ///   Modifies the current <see cref="HashSet{T}" /> object to contain all elements that are present in itself, the
    ///   specified collection, or both.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    public void UnionWith(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      foreach (var item in other)
        this.Add(item);
    }

    /// <summary>
    ///   Modifies the current <see cref="HashSet{T}" /> object to contain only elements that are present in that object
    ///   and in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    public void IntersectWith(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0 || ReferenceEquals(other, this))
        return;

      if (other is ICollection<T> { Count: 0 }) {
        this.Clear();
        return;
      }

      var set = other.ToHashSet(this.Comparer);
      foreach (var item in this._hashtable.Keys.ToArray())
        if (!set._hashtable.ContainsKey(item))
          this._hashtable.Remove(item);
    }

    /// <summary>Removes all elements in the specified collection from the current <see cref="HashSet{T}" /> object.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    public void ExceptWith(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0)
        return;

      if (ReferenceEquals(other, this)) {
        this.Clear();
        return;
      }

      foreach (var element in other)
        this.Remove(element);
    }

    /// <summary>
    ///   Modifies the current <see cref="HashSet{T}" /> object to contain only elements that are present either in that
    ///   object or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    public void SymmetricExceptWith(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0) {
        this.UnionWith(other);
        return;
      }

      if (ReferenceEquals(other, this)) {
        this.Clear();
        return;
      }

      var tempSet = new HashSet<T>();
      foreach (var item in other)
        // Try to add to temporary set, if item exists in current, it will be removed
        if (!this.Add(item)) // This checks if the item is already present in this set
          tempSet.Add(item);

      // Remove items that are in both sets
      foreach (var item in tempSet)
        this.Remove(item);
    }

    /// <summary>Determines whether a <see cref="HashSet{T}" /> object is a subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>true if the <see cref="HashSet{T}" /> object is a subset of <paramref name="other" />; otherwise, false.</returns>
    public bool IsSubsetOf(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0 || ReferenceEquals(other, this))
        return true;

      var (uniqueCount, unfoundCount) = this.CheckUniqueAndUnfoundElements(other, false);
      return uniqueCount == this.Count && unfoundCount >= 0;
    }

    /// <summary>Determines whether a <see cref="HashSet{T}" /> object is a proper subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>
    ///   true if the <see cref="HashSet{T}" /> object is a proper subset of <paramref name="other" />; otherwise,
    ///   false.
    /// </returns>
    public bool IsProperSubsetOf(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (ReferenceEquals(other, this))
        return false;

      var (uniqueCount, unfoundCount) = this.CheckUniqueAndUnfoundElements(other, false);
      return uniqueCount == this.Count && unfoundCount > 0;
    }

    /// <summary>Determines whether a <see cref="HashSet{T}" /> object is a proper superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>true if the <see cref="HashSet{T}" /> object is a superset of <paramref name="other" />; otherwise, false.</returns>
    public bool IsSupersetOf(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (ReferenceEquals(other, this))
        return true;

      foreach (var element in other)
        if (!this.Contains(element))
          return false;

      return true;
    }

    /// <summary>Determines whether a <see cref="HashSet{T}" /> object is a proper superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>
    ///   true if the <see cref="HashSet{T}" /> object is a proper superset of <paramref name="other" />; otherwise,
    ///   false.
    /// </returns>
    public bool IsProperSupersetOf(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0 || ReferenceEquals(other, this))
        return false;

      var (uniqueCount, unfoundCount) = this.CheckUniqueAndUnfoundElements(other, true);
      return uniqueCount < this.Count && unfoundCount == 0;
    }

    /// <summary>
    ///   Determines whether the current <see cref="HashSet{T}" /> object and a specified collection share common
    ///   elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>
    ///   true if the <see cref="HashSet{T}" /> object and <paramref name="other" /> share at least one common element;
    ///   otherwise, false.
    /// </returns>
    public bool Overlaps(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (this.Count == 0)
        return false;

      if (ReferenceEquals(other, this))
        return true;

      foreach (var element in other)
        if (this.Contains(element))
          return true;

      return false;
    }

    /// <summary>Determines whether a <see cref="HashSet{T}" /> object and the specified collection contain the same elements.</summary>
    /// <param name="other">The collection to compare to the current <see cref="HashSet{T}" /> object.</param>
    /// <returns>true if the <see cref="HashSet{T}" /> object is equal to <paramref name="other" />; otherwise, false.</returns>
    public bool SetEquals(IEnumerable<T> other) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      if (ReferenceEquals(other, this))
        return true;

      if (this.Count == 0 && other is ICollection<T> { Count: > 0 })
        return false;

      var (uniqueCount, unfoundCount) = this.CheckUniqueAndUnfoundElements(other, true);
      return uniqueCount == this.Count && unfoundCount == 0;
    }

    private (int UniqueCount, int UnfoundCount) CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

      var foundInOther = new HashSet<T>();
      var uniqueCount = 0;
      var unfoundCount = 0;

      // Check for unique elements in 'other' not found in this set
      foreach (var item in other) {
        Wrapper wrapped = new(item, this);
        if (!this._hashtable.ContainsKey(wrapped)) {
          ++uniqueCount;
          if (returnIfUnfound)
            return (uniqueCount, unfoundCount);
        } else
          foundInOther.Add(item);
      }

      // Check for elements in this set not found in 'other'
      foreach (var wrapped in this._hashtable.Keys)
        if (!foundInOther.Contains(this._hashtable[wrapped]))
          ++unfoundCount;

      return (uniqueCount, unfoundCount);
    }

    public void CopyTo(T[] array) => this.CopyTo(array, 0, this.Count);

    /// <summary>Copies the elements of a <see cref="HashSet{T}" /> object to an array, starting at the specified array index.</summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex) => this.CopyTo(array, arrayIndex, this.Count);

    public void CopyTo(T[] array, int arrayIndex, int count) {
      if (array == null)
        throw new ArgumentNullException(nameof(array));
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException(nameof(arrayIndex));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof(count));

      if (arrayIndex > array.Length || count > array.Length - arrayIndex)
        throw new ArgumentException("Target array too small");

      foreach (var kvp in this._hashtable) {
        array[arrayIndex++] = kvp.Value;
        if (--count == 0)
          break;
      }
    }

    /// <summary>
    ///   Removes all elements that match the conditions defined by the specified predicate from a
    ///   <see cref="HashSet{T}" /> collection.
    /// </summary>
    public int RemoveWhere(Predicate<T> match) {
      if (match == null)
        throw new ArgumentNullException(nameof(match));

      var result = 0;
      foreach (var kvp in this._hashtable)
        if (match(kvp.Value)) {
          this._hashtable.Remove(kvp.Key);
          ++result;
        }

      return result;
    }

    /// <summary>Gets the <see cref="IEqualityComparer" /> object that is used to determine equality for the values in the set.</summary>
    public IEqualityComparer<T> Comparer { get; }

    /// <summary>Ensures that this hash set can hold the specified number of elements without growing.</summary>
    public int EnsureCapacity(int capacity) => capacity;

    /// <summary>
    ///   Sets the capacity of a <see cref="HashSet{T}" /> object to the actual number of elements it contains,
    ///   rounded up to a nearby, implementation-specific value.
    /// </summary>
    public void TrimExcess() { }

    #endregion
  }
}

#endif
