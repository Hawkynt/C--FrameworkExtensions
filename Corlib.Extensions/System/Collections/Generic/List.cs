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
#if !DEPRECATED_BINARY_FORMATTER
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Diagnostics;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMember.Global
#endif

namespace System.Collections.Generic; 

public static partial class ListExtensions {

  /// <summary>
  /// Implements a faster shortcut for LINQ's .Any()
  /// </summary>
  /// <param name="this">This <see cref="IList{T}"/></param>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <returns><see langword="true"/> if there is at least one item in the <see cref="IList{T}"/>; otherwise, <see langword="false"/>.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static bool Any<TItem>(this IList<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Tries to get the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetFirst<T>(this IList<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[0];
    return true;
  }

  /// <summary>
  /// Tries to get the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetLast<T>(this IList<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[^1];
    return true;
  }

  /// <summary>
  /// Tries to get the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetItem<T>(this IList<T> @this, int index, out T result) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Count <= index) {
      result = default;
      return false;
    }

    result = @this[index];
    return true;
  }

  /// <summary>
  /// Tries to set the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetFirst<T>(this IList<T> @this, T value) {
    Against.ThisIsNull(@this);
    
    if (@this.Count <= 0)
      return false;
    
    @this[0] = value;
    return true;
  }

  /// <summary>
  /// Tries to set the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetLast<T>(this IList<T> @this, T value) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0)
      return false;

    @this[^1] = value;
    return true;
  }

  /// <summary>
  /// Tries to set the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IList"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetItem<T>(this IList<T> @this, int index, T value) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Count <= index)
      return false;

    @this[index] = value;
    return true;
  }

#if SUPPORTS_READ_ONLY_COLLECTIONS
  
  /// <summary>
  /// Implements a faster shortcut for LINQ's .Any()
  /// </summary>
  /// <param name="this">This <see cref="IReadOnlyList{T}"/></param>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <returns><see langword="true"/> if there is at least one item in the <see cref="IReadOnlyList{T}"/>; otherwise, <see langword="false"/>.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static bool Any<TItem>(this IReadOnlyList<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Implements a faster shortcut for LINQ's .Any()
  /// </summary>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <returns><see langword="true"/> if there is at least one item in the <see cref="List{T}"/>; otherwise, <see langword="false"/>.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static bool Any<TItem>(this List<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Tries to get the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetFirst<T>(this List<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[0];
    return true;
  }
  
  /// <summary>
  /// Tries to get the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetLast<T>(this List<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[^1];
    return true;
  }

  /// <summary>
  /// Tries to get the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetItem<T>(this List<T> @this, int index, out T result) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Count <= index) {
      result = default;
      return false;
    }

    result = @this[index];
    return true;
  }

  /// <summary>
  /// Tries to set the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetFirst<T>(this List<T> @this, T value) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0)
      return false;

    @this[0] = value;
    return true;
  }

  /// <summary>
  /// Tries to set the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetLast<T>(this List<T> @this, T value) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0)
      return false;

    @this[^1] = value;
    return true;
  }

  /// <summary>
  /// Tries to set the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="List{T}"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be assigned; otherwise, <see langword="false"/>.</returns>
  public static bool TrySetItem<T>(this List<T> @this, int index, T value) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Count <= index)
      return false;

    @this[index] = value;
    return true;
  }

  /// <summary>
  /// Tries to get the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IReadOnlyList{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetFirst<T>(this IReadOnlyList<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[0];
    return true;
  }

  /// <summary>
  /// Tries to get the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IReadOnlyList{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetLast<T>(this IReadOnlyList<T> @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Count <= 0) {
      result = default;
      return false;
    }

    result = @this[^1];
    return true;
  }

  /// <summary>
  /// Tries to get the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IReadOnlyList{T}"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetItem<T>(this IReadOnlyList<T> @this, int index, out T result) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Count <= index) {
      result = default;
      return false;
    }

    result = @this[index];
    return true;
  }

#endif

  /// <summary>
  /// Removes every occurance of the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/>.</param>
  /// <param name="item">The item to remove.</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void RemoveEvery<TItem>(this IList<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    while (@this.Remove(item)) { }
  }

  /// <summary>
  /// Removes the given items from the list.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumerable.</param>
  /// <param name="items">The items.</param>
  public static void RemoveAll<TItem>(this IList<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    List<TItem> removeables = [..items];
    foreach (var item in removeables)
      @this.Remove(item);
  }

  // return part 
  public static T[] Splice<T>(this IList<T> @this, int start, int count) {
    Against.ThisIsNull(@this);
    
    var result = new T[count];
    for (var i = count - 1; i >= 0; --i)
      result[i] = @this[i + start];

    return result;
  }

  // swap two elements
  public static void Swap<T>(this IList<T> @this, int i, int j) {
    Against.ThisIsNull(@this);
    
    (@this[i], @this[j]) = (@this[j], @this[i]);
  }

  // fisher-yates shuffle array
  public static void Shuffle<T>(this IList<T> @this) {
    Against.ThisIsNull(@this);
    
    var i = @this.Count;

#if SUPPORTS_RANDOM_SHARED
    var random = Random.Shared;
#else
    Random random = new();
#endif
    while (i-- > 1)
      @this.Swap(random.Next(i + 1), i);

  }

  public static TOutput[] ConvertAll<TInput, TOutput>(this IList<TInput> @this, Converter<TInput, TOutput> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    return Array.ConvertAll(@this.ToArray(), converter);
  }

  public static void ForEach<TInput>(this IList<TInput> @this, Action<TInput> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    Array.ForEach(@this.ToArray(), action);
  }

  /// <summary>
  /// Removes items at the given position.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This IList.</param>
  /// <param name="start">The start.</param>
  /// <param name="count">The count.</param>
  public static void RemoveRange<TInput>(this IList<TInput> @this, int start, int count) {
    Against.ThisIsNull(@this);

    switch (count) {
      // special case I - return when nothing to remove
      case < 1:
        return;
    
      // special case II - only one item removed
      case 1:
        @this.RemoveAt(start);
        return;
    }

    // special case, given a real List<T>
    if (@this is List<TInput> realList) {
      realList.RemoveRange(start, count);
      return;
    }

    // remove every single item, starting backwards to avoid broken indexes
    for (var i = Math.Min(@this.Count - 1, start + count - 1); i >= start; --i)
      @this.RemoveAt(i);
  }

  /// <summary>
  /// Adds the items.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/>.</param>
  /// <param name="items">The items.</param>
  [DebuggerStepThrough]
  public static void AddRange<TInput>(this IList<TInput> @this, IEnumerable<TInput> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    // special case, given a real List<T>
    if (@this is List<TInput> realList) {
      realList.AddRange(items);
      return;
    }

    foreach (var item in items)
      @this.Add(item);
  }

  /// <summary>
  /// Adds a range of items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="ICollection{T}"/>.</param>
  /// <param name="items">The items.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void AddRange<TItem>(this List<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    @this.AddRange(items);
  }

  /// <summary>
  /// Adds if not null.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/>.</param>
  /// <param name="item">The item.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void AddIfNotNull<TInput>(this IList<TInput> @this, TInput item) {
    Against.ThisIsNull(@this);

    if (item != null)
      @this.Add(item);
  }

  /// <summary>
  /// Keeps the first n items.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This IList.</param>
  /// <param name="count">The count.</param>
  public static void KeepFirst<TInput>(this IList<TInput> @this, int count) {
    Against.ThisIsNull(@this);

    switch (count) {
      // special case: keep nothing
      case < 1:
        @this.Clear();
        return;
    
      // special case: keep the first element
      case 1: {
        var item = @this[0];
        @this.Clear();
        @this.Add(item);
        return;
      }
    }

    // special case: keep all elements
    var len = @this.Count;
    if (count >= len)
      return;

    var index = count;
    count = len - index;

    if (count > index) {

      // more to remove than to keep, copy the items and clear the list, then re-add
      var copy = new TInput[index];
      for (var i = 0; i < index; ++i)
        copy[i] = @this[i];

      @this.Clear();
      @this.AddRange(copy);
    } else

      // only few elements to remove
      @this.RemoveRange(index, count);
  }

  /// <summary>
  /// Keeps the last n items.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This IList.</param>
  /// <param name="count">The count.</param>
  public static void KeepLast<TInput>(this IList<TInput> @this, int count) {
    Against.ThisIsNull(@this);

    // special case: remove all items
    if (count < 1) {
      @this.Clear();
      return;
    }

    // special case: keep all elements
    var len = @this.Count;
    if (count > len)
      return;

    // special case: keep last item
    if (count == 1) {
      var item = @this[len - 1];
      @this.Clear();
      @this.Add(item);
      return;
    }

    var index = len - count;
    if (count > index) {

      // more to remove than to keep
      var copy = new TInput[count];
      for (int i = count - 1, j = len - 1; i >= 0; --i, --j)
        copy[i] = @this[j];
      @this.Clear();
      @this.AddRange(copy);
    } else

      // only few elements to remove
      @this.RemoveRange(0, index);
  }

  /// <summary>
  /// Removes the first n items.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This IList.</param>
  /// <param name="count">The count.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void RemoveFirst<TInput>(this IList<TInput> @this, int count) {
    Against.ThisIsNull(@this);

    var remaining = @this.Count - count;
    @this.KeepLast(remaining);
  }

  /// <summary>
  /// Removes the last n items.
  /// </summary>
  /// <typeparam name="TInput">The type of the input.</typeparam>
  /// <param name="this">This IList.</param>
  /// <param name="count">The count.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void RemoveLast<TInput>(this IList<TInput> @this, int count) {
    Against.ThisIsNull(@this);

    var remaining = @this.Count - count;
    @this.KeepFirst(remaining);
  }

  /// <summary>
  /// Returns all permutations of the specified items.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="this">The items.</param>
  /// <param name="separateArrays">if set to <c>true</c> returns separate arrays; otherwise, returns the same array changed over and over again.</param>
  /// <returns></returns>
  public static IEnumerable<T[]> Permutate<T>(this IList<T> @this, bool separateArrays = false) {
    Against.ThisIsNull(@this);

    return Invoke(@this, separateArrays);
    
    static IEnumerable<T[]> Invoke(IList<T> @this, bool separateArrays) {

      var length = @this.Count;
      if (length < 1)
        yield break;

      var current = new T[length];
      var state = new int[length];
      for (var i = 0; i < length; ++i)
        current[i] = @this[state[i] = i];

      var lastIndex = length - 1;
      while (true) {

        // return copy or working array
        if (separateArrays) {
          var result = new T[length];
          current.CopyTo(result, 0);
          yield return result;
        } else
          yield return current;

        // increment the 2nd last digit
        var index = lastIndex - 1;
        while (true) {

          // increment as long as there are matching slots
          HashSet<int> slotsBefore = new(state.Take(index));
          do {
            ++state[index];
          } while (slotsBefore.Contains(state[index]));

          // if we did not ran out of digits
          if (state[index] <= lastIndex)
            break;

          // otherwise, try incrementing the left next slot
          --index;

          // no more slots, all permutations done
          if (index < 0)
            yield break;
        }

        // fill content by digit
        current[index] = @this[state[index]];

        // fill all slots after the incremented one
        for (var i = index + 1; i < length; ++i) {
          state[i] = 0;

          HashSet<int> slotsBefore = new(state.Take(i));
          while (slotsBefore.Contains(state[i]))
            ++state[i];

          current[i] = @this[state[i]];
        }

      }
    }
  }

  /// <summary>
  /// Returns all permutations of the specified items.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="this">The items.</param>
  /// <param name="length">The length of each permutation.</param>
  /// <param name="separateArrays">if set to <c>true</c> returns separate arrays; otherwise, returns the same array changed over and over again.</param>
  /// <returns></returns>
  public static IEnumerable<T[]> Permutate<T>(this IList<T> @this, int length, bool separateArrays = false) {
    Against.ThisIsNull(@this);

    return Invoke(@this, length, separateArrays);
    
    static IEnumerable<T[]> Invoke(IList<T> @this, int length, bool separateArrays) {

      if (length < 1)
        yield break;

      var itemLastIndex = @this.Count - 1;
      if (itemLastIndex < 0)
        yield break;

      var current = new T[length];
      for (var i = 0; i < length; ++i)
        current[i] = @this[0];

      var states = new int[length];
      --length;

      // this version creates a new array for each permutations and returns it
      if (separateArrays) {
        while (true) {
          var result = new T[length + 1];
          current.CopyTo(result, 0);
          yield return result;

          if (!_ArePermutationsLeft(@this, length, states, itemLastIndex, current))
            yield break;
        }
      }

      while (true) {
        yield return current;

        if (!_ArePermutationsLeft(@this, length, states, itemLastIndex, current))
          yield break;
      }
    }
  }

  /// <summary>
  /// The permutation core.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="items">The items.</param>
  /// <param name="length">The length.</param>
  /// <param name="states">The states.</param>
  /// <param name="itemLastIndex">Last index of the item.</param>
  /// <param name="current">The current.</param>
  /// <returns><c>true</c> if there are more permutations available; otherwise, <c>false</c>.</returns>
  private static bool _ArePermutationsLeft<T>(IList<T> items, int length, int[] states, int itemLastIndex, T[] current) {

    // set counter position back to last index
    var index = length;
    while (states[index] >= itemLastIndex) {
      states[index] = 0;
      current[index] = items[0];
      --index;

      // all permutations done
      if (index < 0)
        return false;
    }

    // create next permutation
    current[index] = items[++states[index]];

    return true;
  }

#if !DEPRECATED_BINARY_FORMATTER
  public static List<T> DeepClone<T>(this List<T> list) 
  {
    object objResult;
    using (var  ms = new MemoryStream()) {
      var  bf =   new BinaryFormatter();
      bf.Serialize(ms, list);

      ms.Position = 0;
      objResult = bf.Deserialize(ms);
    }
    return (List<T>)objResult;
  }
#endif

  /// <summary>
  /// Performs a binary search on a section of a list of <see cref="IComparable{T}"/> elements, optionally returning the index of the first item greater than the searched item if the exact match is not found.
  /// </summary>
  /// <remarks>
  /// If no such item exists, -1 is returned.
  /// </remarks>
  /// <typeparam name="T">The type of elements in the list, which must implement <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/> to search.</param>
  /// <param name="item">The item to search for.</param>
  /// <returns> The index of the item in the list that matches the searched item or -1.</returns>
  public static int BinarySearchIndex<T>(this IList<T> @this, T item) where T : IComparable<T> {
    Against.ThisIsNull(@this);

    return _BinarySearchIndex(@this, item, 0, @this.Count, false);
  }

  /// <summary>
  /// Performs a binary search on a section of a list of <see cref="IComparable{T}"/> elements, optionally returning the index of the first item greater than the searched item if the exact match is not found.
  /// </summary>
  /// <remarks>
  /// If no such item exists, -1 is returned.
  /// </remarks>
  /// <typeparam name="T">The type of elements in the list, which must implement <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/> to search.</param>
  /// <param name="item">The item to search for.</param>
  /// <param name="startAt">The index of the first element in the section to search.</param>
  /// <param name="count">The number of elements in the section to search.</param>
  /// <returns> The index of the item in the list that matches the searched item or -1.</returns>

  public static int BinarySearchIndex<T>(this IList<T> @this, T item, int startAt, int count) where T : IComparable<T> {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(startAt);
    Against.CountBelowZero(count);

    return _BinarySearchIndex(@this, item, startAt, count, false);
  }

  /// <summary>
  /// Performs a binary search on a section of a list of <see cref="IComparable{T}"/> elements, optionally returning the index of the first item greater than the searched item if the exact match is not found.
  /// </summary>
  /// <remarks>
  /// If <paramref name="returnNextGreater"/> is <see langword="true"/> and the exact item is not found, this method will return the index of the first item that is greater than the searched item. If no such item exists, -1 is returned.
  /// </remarks>
  /// <typeparam name="T">The type of elements in the list, which must implement <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/> to search.</param>
  /// <param name="item">The item to search for.</param>
  /// <param name="returnNextGreater">If set to <see langword="true"/>, and the item is not found, the method will return the index of the first item greater than the searched item.</param>
  /// <returns>
  /// The index of the item in the list that matches the searched item. If the item is not found, and <paramref name="returnNextGreater"/> is <see langword="false"/>, -1 is returned. If <paramref name="returnNextGreater"/> is <see langword="true"/>, the index of the first item that is greater than the searched item is returned, or -1 if no such item exists.
  /// </returns>
  public static int BinarySearchIndex<T>(this IList<T> @this, T item, bool returnNextGreater) where T : IComparable<T> {
    Against.ThisIsNull(@this);

    return _BinarySearchIndex(@this, item, 0, @this.Count, returnNextGreater);
  }

  /// <summary>
  /// Performs a binary search on a section of a list of <see cref="IComparable{T}"/> elements, optionally returning the index of the first item greater than the searched item if the exact match is not found.
  /// </summary>
  /// <remarks>
  /// If <paramref name="returnNextGreater"/> is <see langword="true"/> and the exact item is not found, this method will return the index of the first item that is greater than the searched item. If no such item exists, -1 is returned.
  /// </remarks>
  /// <typeparam name="T">The type of elements in the list, which must implement <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/> to search.</param>
  /// <param name="item">The item to search for.</param>
  /// <param name="startAt">The index of the first element in the section to search.</param>
  /// <param name="count">The number of elements in the section to search.</param>
  /// <param name="returnNextGreater">If set to <see langword="true"/>, and the item is not found, the method will return the index of the first item greater than the searched item.</param>
  /// <returns>
  /// The index of the item in the list that matches the searched item. If the item is not found, and <paramref name="returnNextGreater"/> is <see langword="false"/>, -1 is returned. If <paramref name="returnNextGreater"/> is <see langword="true"/>, the index of the first item that is greater than the searched item is returned, or -1 if no such item exists.
  /// </returns>
  public static int BinarySearchIndex<T>(this IList<T> @this, T item, int startAt, int count, bool returnNextGreater) where T : IComparable<T> {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(startAt);
    Against.CountBelowZero(count);

    return _BinarySearchIndex(@this, item, startAt, count, returnNextGreater);
  }

  /// <summary>
  /// Performs a binary search on a section of a list of <see cref="IComparable{T}"/> elements, optionally returning the index of the first item greater than the searched item if the exact match is not found.
  /// </summary>
  /// <remarks>
  /// If <paramref name="returnNextGreater"/> is <see langword="true"/> and the exact item is not found, this method will return the index of the first item that is greater than the searched item. If no such item exists, -1 is returned.
  /// </remarks>
  /// <typeparam name="T">The type of elements in the list, which must implement <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="this">This <see cref="IList{T}"/> to search.</param>
  /// <param name="item">The item to search for.</param>
  /// <param name="startAt">The index of the first element in the section to search.</param>
  /// <param name="count">The number of elements in the section to search.</param>
  /// <param name="returnNextGreater">If set to <see langword="true"/>, and the item is not found, the method will return the index of the first item greater than the searched item.</param>
  /// <returns>
  /// The index of the item in the list that matches the searched item. If the item is not found, and <paramref name="returnNextGreater"/> is <see langword="false"/>, -1 is returned. If <paramref name="returnNextGreater"/> is <see langword="true"/>, the index of the first item that is greater than the searched item is returned, or -1 if no such item exists.
  /// </returns>
  private static int _BinarySearchIndex<T>(this IList<T> @this, T item,int startAt,int count, bool returnNextGreater) where T : IComparable<T> {
    const int INDEX_WHEN_NOT_FOUND = -1;

    var left = startAt;
    var right = startAt + count - 1;
    var foundIndex = INDEX_WHEN_NOT_FOUND;
    var isSearchingForNull = item == null;

    while (left <= right) {
      var mid = left + ((right - left) >> 1);
      var midItem = @this[mid];

      var comparison = midItem switch {
        null when isSearchingForNull => 0,
        null => -1,
        not null when isSearchingForNull => 1,
        not null => midItem.CompareTo(item)
      };
      
      switch (comparison) {
        case 0:
          // Found an item, continue to search towards the left for the first occurrence.
          foundIndex = mid;
          right = mid - 1;
          continue;
        case < 0:
          left = mid + 1;
          continue;
        case >0:
          // If we are looking for the next greater item, store this index.
          if (returnNextGreater && (foundIndex < 0 || mid < foundIndex))
            foundIndex = mid;

          right = mid - 1;
          continue;
      }
    }

    // If returnNextGreater is true and no exact match is found, return the index of the next greater item.
    // foundIndex will be -1 if no such item exists.
    return foundIndex;
  }

}