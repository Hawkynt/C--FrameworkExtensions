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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Frozen;

/// <summary>
/// Provides an immutable, read-only set optimized for fast lookup and enumeration.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public class FrozenSet<T> :
  ISet<T>,
  IReadOnlySet<T>,
  ICollection
{
  private readonly HashSet<T> _set;

  /// <summary>
  /// Gets an empty <see cref="FrozenSet{T}"/>.
  /// </summary>
  public static FrozenSet<T> Empty { get; } = new(new(), EqualityComparer<T>.Default);

  internal FrozenSet(HashSet<T> set, IEqualityComparer<T> comparer) {
    this._set = set;
    this.Comparer = comparer;
  }

  /// <summary>
  /// Gets the equality comparer used to determine equality of elements.
  /// </summary>
  public IEqualityComparer<T> Comparer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get;
  }

  /// <summary>
  /// Gets the number of elements in the set.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._set.Count;
  }

  /// <summary>
  /// Gets an enumerable that iterates through the set.
  /// </summary>
  public IEnumerable<T> Items => this._set;

  /// <summary>
  /// Determines whether the set contains the specified element.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Contains(T item) => this._set.Contains(item);

  /// <summary>
  /// Searches the set for a given value and returns the equal value it finds, if any.
  /// </summary>
  public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) => this._set.TryGetValue(equalValue, out actualValue!);

  /// <summary>
  /// Determines whether the current set is a proper subset of a specified collection.
  /// </summary>
  public bool IsProperSubsetOf(IEnumerable<T> other) => this._set.IsProperSubsetOf(other);

  /// <summary>
  /// Determines whether the current set is a proper superset of a specified collection.
  /// </summary>
  public bool IsProperSupersetOf(IEnumerable<T> other) => this._set.IsProperSupersetOf(other);

  /// <summary>
  /// Determines whether the current set is a subset of a specified collection.
  /// </summary>
  public bool IsSubsetOf(IEnumerable<T> other) => this._set.IsSubsetOf(other);

  /// <summary>
  /// Determines whether the current set is a superset of a specified collection.
  /// </summary>
  public bool IsSupersetOf(IEnumerable<T> other) => this._set.IsSupersetOf(other);

  /// <summary>
  /// Determines whether the current set overlaps with the specified collection.
  /// </summary>
  public bool Overlaps(IEnumerable<T> other) => this._set.Overlaps(other);

  /// <summary>
  /// Determines whether the current set and the specified collection contain the same elements.
  /// </summary>
  public bool SetEquals(IEnumerable<T> other) => this._set.SetEquals(other);

  /// <summary>
  /// Copies the elements of the set to an array.
  /// </summary>
  public void CopyTo(T[] array, int arrayIndex) {
    ArgumentNullException.ThrowIfNull(array);
    this._set.CopyTo(array, arrayIndex);
  }

  /// <summary>
  /// Copies the elements of the set to a span.
  /// </summary>
  public void CopyTo(Span<T> destination) {
    if (destination.Length < this._set.Count)
      throw new ArgumentException("Destination span is too short.");

    var i = 0;
    foreach (var item in this._set)
      destination[i++] = item;
  }

  /// <summary>
  /// Returns an enumerator that iterates through the set.
  /// </summary>
  public Enumerator GetEnumerator() => new(this._set);

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)this._set).GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this._set).GetEnumerator();

  #region Explicit interface implementations (read-only throws)

  bool ICollection<T>.IsReadOnly => true;
  bool ICollection.IsSynchronized => false;
  object ICollection.SyncRoot => field ??= new();

  void ICollection<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");
  bool ISet<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");
  void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");
  void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");
  void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");
  void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotSupportedException("Collection is read-only.");
  bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Collection is read-only.");
  void ICollection<T>.Clear() => throw new NotSupportedException("Collection is read-only.");

  void ICollection.CopyTo(Array array, int index) {
    ArgumentNullException.ThrowIfNull(array);
    if (array.Rank != 1)
      throw new ArgumentException("Multi-dimensional arrays are not supported.");
    if (index < 0 || index > array.Length)
      throw new ArgumentOutOfRangeException(nameof(index));
    if (array.Length - index < this.Count)
      throw new ArgumentException("Destination array is too small.");

    foreach (var item in this._set)
      array.SetValue(item, index++);
  }

  #endregion

  #region Nested Types

  /// <summary>
  /// Enumerates the elements of a <see cref="FrozenSet{T}"/>.
  /// </summary>
  public readonly struct Enumerator : IEnumerator<T> {
    private readonly IEnumerator<T> _enumerator;

    internal Enumerator(HashSet<T> set) => this._enumerator = ((IEnumerable<T>)set).GetEnumerator();

    /// <inheritdoc/>
    public T Current => this._enumerator.Current;

    object? IEnumerator.Current => this.Current;

    /// <inheritdoc/>
    public bool MoveNext() => this._enumerator.MoveNext();

    /// <inheritdoc/>
    public void Reset() => this._enumerator.Reset();

    /// <inheritdoc/>
    public void Dispose() => this._enumerator.Dispose();
  }

  #endregion
}

/// <summary>
/// Provides a set of initialization methods for instances of the <see cref="FrozenSet{T}"/> class.
/// </summary>
public static class FrozenSet {
  /// <summary>
  /// Creates a <see cref="FrozenSet{T}"/> from the specified collection.
  /// </summary>
  public static FrozenSet<T> ToFrozenSet<T>(
    this IEnumerable<T> source,
    IEqualityComparer<T>? comparer = null
  ) {
    ArgumentNullException.ThrowIfNull(source);

    comparer ??= EqualityComparer<T>.Default;
    var set = new HashSet<T>(comparer);
    foreach (var item in source)
      set.Add(item);

    return set.Count == 0 && comparer == EqualityComparer<T>.Default
      ? FrozenSet<T>.Empty
      : new(set, comparer);
  }
}

#endif
