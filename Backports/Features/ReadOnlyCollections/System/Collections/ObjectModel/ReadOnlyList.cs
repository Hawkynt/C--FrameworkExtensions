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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.ObjectModel;

/// <summary>
/// A wrapper that provides <see cref="IReadOnlyList{T}"/> interface for any <see cref="IList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <remarks>
/// On .NET 4.5+, <see cref="List{T}"/> natively implements <see cref="IReadOnlyList{T}"/>.
/// This wrapper provides the same capability on older frameworks.
/// </remarks>
public sealed class ReadOnlyList<T> : IReadOnlyList<T>, IList<T> {

  private readonly IList<T> _list;

  /// <summary>
  /// Initializes a new instance wrapping the specified list.
  /// </summary>
  /// <param name="list">The list to wrap.</param>
  /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlyList(IList<T> list) => this._list = list ?? throw new ArgumentNullException(nameof(list));

  /// <inheritdoc/>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._list[index];
  }

  /// <inheritdoc/>
  T IList<T>.this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._list[index];
    set => ThrowReadOnly();
  }

  /// <inheritdoc/>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._list.Count;
  }

  /// <inheritdoc/>
  public bool IsReadOnly {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => true;
  }

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerator<T> GetEnumerator() => this._list.GetEnumerator();

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Contains(T item) => this._list.Contains(item);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTo(T[] array, int arrayIndex) => this._list.CopyTo(array, arrayIndex);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int IndexOf(T item) => this._list.IndexOf(item);

  /// <inheritdoc/>
  void IList<T>.Insert(int index, T item) => ThrowReadOnly();

  /// <inheritdoc/>
  void IList<T>.RemoveAt(int index) => ThrowReadOnly();

  /// <inheritdoc/>
  void ICollection<T>.Add(T item) => ThrowReadOnly();

  /// <inheritdoc/>
  void ICollection<T>.Clear() => ThrowReadOnly();

  /// <inheritdoc/>
  bool ICollection<T>.Remove(T item) => ThrowReadOnly<bool>();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void ThrowReadOnly() => ThrowReadOnly<bool>();

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static TResult ThrowReadOnly<TResult>() => throw new NotSupportedException("Collection is read-only.");
}
