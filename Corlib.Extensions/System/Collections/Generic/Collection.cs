#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using Guard;
using System.Linq;
using System.Diagnostics;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable PartialTypeWithSinglePart
namespace System.Collections.Generic; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class CollectionExtensions {

  /// <summary>
  /// Implements a faster shortcut for LINQ's .Any()
  /// </summary>
  /// <param name="this">This <see cref="ICollection{T}"/></param>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <returns><see langword="true"/> if there is at least one item in the <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static bool Any<TItem>(this ICollection<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Executes an action on each item.
  /// </summary>
  /// <typeparam name="TValue">The type of the values.</typeparam>
  /// <param name="this">The collection.</param>
  /// <param name="action">The call to execute.</param>
  public static void ForEach<TValue>(this ICollection<TValue> @this, Action<TValue> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    Array.ForEach(@this.ToArray(), action);
  }

  /// <summary>
  /// Converts all.
  /// </summary>
  /// <typeparam name="TIn">The type of the input collection.</typeparam>
  /// <typeparam name="TOut">The type of the output collection.</typeparam>
  /// <param name="this">The collection.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns></returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static TOut[] ConvertAll<TIn, TOut>(this ICollection<TIn> @this, Converter<TIn, TOut> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    return Array.ConvertAll(@this.ToArray(), converter);
  }

  /// <summary>
  /// Adds a range of items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="ICollection{T}"/>.</param>
  /// <param name="items">The items.</param>
  public static void AddRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    // PERF: check for special list first
    if (@this is List<TItem> list) {
      list.AddRange(items);
      return;
    }

    foreach (var item in items)
      @this.Add(item);
  }

  /// <summary>
  /// Removes the range of items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Collection.</param>
  /// <param name="items">The items.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void RemoveRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Remove(item);
  }

  /// <summary>
  /// Determines whether a given <see cref="ICollection"/> contains exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="ICollection"/></param>
  /// <returns><see langword="true"/> if the <see cref="ICollection"/> has one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsSingle<TValue>(this ICollection<TValue> @this) {
    Against.ThisIsNull(@this);  
    return @this.Count == 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="ICollection"/> contains more than one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="ICollection"/></param>
  /// <returns><see langword="true"/> if the <see cref="ICollection"/> has more than one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsMultiple<TValue>(this ICollection<TValue> @this) {
    Against.ThisIsNull(@this);
    return @this.Count > 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="ICollection"/> contains not exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="ICollection"/></param>
  /// <returns><see langword="true"/> if the <see cref="ICollection"/> has less or more than one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNoSingle<TValue>(this ICollection<TValue> @this) {
    Against.ThisIsNull(@this);
    return @this.Count != 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="ICollection"/> contains no more than one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="ICollection"/></param>
  /// <returns><see langword="true"/> if the <see cref="ICollection"/> has less than two elements; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNoMultiple<TValue>(this ICollection<TValue> @this) {
    Against.ThisIsNull(@this);
    return @this.Count <= 1;
  }
}