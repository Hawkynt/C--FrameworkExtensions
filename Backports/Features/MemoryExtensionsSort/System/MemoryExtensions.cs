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

#if !SUPPORTS_MEMORYEXTENSIONS_SORT

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension(MemoryExtensions) {

    /// <summary>
    /// Sorts the elements in the entire <see cref="Span{T}"/> using the <see cref="IComparable{T}"/> implementation of each element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(Span<T> span) {
      if (span.Length <= 1)
        return;

      var array = span.ToArray();
      Array.Sort(array);
      array.AsSpan().CopyTo(span);
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="Span{T}"/> using the specified <see cref="Comparison{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to sort.</param>
    /// <param name="comparison">The comparison to use when comparing elements.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(Span<T> span, Comparison<T> comparison) {
      if (span.Length <= 1)
        return;

      var array = span.ToArray();
      Array.Sort(array, comparison);
      array.AsSpan().CopyTo(span);
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="Span{T}"/> using the specified <see cref="IComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <typeparam name="TComparer">The type of the comparer.</typeparam>
    /// <param name="span">The span to sort.</param>
    /// <param name="comparer">The comparer to use when comparing elements.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T, TComparer>(Span<T> span, TComparer comparer) where TComparer : IComparer<T> {
      if (span.Length <= 1)
        return;

      var array = span.ToArray();
      Array.Sort(array, comparer);
      array.AsSpan().CopyTo(span);
    }

    /// <summary>
    /// Sorts a pair of spans based on the keys in the first span.
    /// </summary>
    /// <typeparam name="TKey">The type of elements in the keys span.</typeparam>
    /// <typeparam name="TValue">The type of elements in the items span.</typeparam>
    /// <param name="keys">The span containing the keys to sort by.</param>
    /// <param name="items">The span containing the items to sort based on the keys.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TValue>(Span<TKey> keys, Span<TValue> items) {
      if (keys.Length <= 1)
        return;

      var keyArray = keys.ToArray();
      var itemArray = items.ToArray();
      Array.Sort(keyArray, itemArray);
      keyArray.AsSpan().CopyTo(keys);
      itemArray.AsSpan().CopyTo(items);
    }

    /// <summary>
    /// Sorts a pair of spans based on the keys in the first span using the specified comparison.
    /// </summary>
    /// <typeparam name="TKey">The type of elements in the keys span.</typeparam>
    /// <typeparam name="TValue">The type of elements in the items span.</typeparam>
    /// <param name="keys">The span containing the keys to sort by.</param>
    /// <param name="items">The span containing the items to sort based on the keys.</param>
    /// <param name="comparison">The comparison to use when comparing keys.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TValue>(Span<TKey> keys, Span<TValue> items, Comparison<TKey> comparison) {
      if (keys.Length <= 1)
        return;

      var keyArray = keys.ToArray();
      var itemArray = items.ToArray();
      Array.Sort(keyArray, itemArray, new _ComparisonComparer<TKey>(comparison));
      keyArray.AsSpan().CopyTo(keys);
      itemArray.AsSpan().CopyTo(items);
    }

    /// <summary>
    /// Sorts a pair of spans based on the keys in the first span using the specified comparer.
    /// </summary>
    /// <typeparam name="TKey">The type of elements in the keys span.</typeparam>
    /// <typeparam name="TValue">The type of elements in the items span.</typeparam>
    /// <typeparam name="TComparer">The type of the comparer.</typeparam>
    /// <param name="keys">The span containing the keys to sort by.</param>
    /// <param name="items">The span containing the items to sort based on the keys.</param>
    /// <param name="comparer">The comparer to use when comparing keys.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TValue, TComparer>(Span<TKey> keys, Span<TValue> items, TComparer comparer) where TComparer : IComparer<TKey> {
      if (keys.Length <= 1)
        return;

      var keyArray = keys.ToArray();
      var itemArray = items.ToArray();
      Array.Sort(keyArray, itemArray, comparer);
      keyArray.AsSpan().CopyTo(keys);
      itemArray.AsSpan().CopyTo(items);
    }

  }

  private sealed class _ComparisonComparer<T>(Comparison<T> comparison) : IComparer<T> {
    public int Compare(T x, T y) => comparison(x, y);
  }

}

#endif
