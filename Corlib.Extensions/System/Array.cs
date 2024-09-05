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

using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
#if !UNSAFE
using System.Security.Permissions;
#endif

namespace System;

public static partial class ArrayExtensions {
  #region nested types

#if !UNSAFE
  private sealed class DisposableGCHandle<TValue> : IDisposable where TValue : class {
    private GCHandle handle;

    public DisposableGCHandle(TValue value, GCHandleType type) => this.handle = GCHandle.Alloc(value, type);
    public IntPtr AddrOfPinnedObject() => this.handle.AddrOfPinnedObject();
    private void _Free() => this.handle.Free();

  #region Properties

    public TValue Target {
      get => (TValue)this.handle.Target;
      set => this.handle.Target = value;
    }

    private bool _IsAllocated => this.handle.IsAllocated;

  #endregion

  #region DisposePattern

    private void Dispose(bool disposing) {
      if (disposing && this._IsAllocated)
        this._Free();

      GC.SuppressFinalize(this);
    }

    public void Dispose() => this.Dispose(true);
    ~DisposableGCHandle() => this.Dispose(false);

  #endregion

  }

  private static class DisposableGCHandle {
    public static DisposableGCHandle<TValue> Pin<TValue>(TValue value) where TValue : class => new DisposableGCHandle<TValue>(value, GCHandleType.Pinned);

  }

#endif

  #endregion

  private const int _INDEX_WHEN_NOT_FOUND = -1;

  /// <summary>
  ///   Compares two arrays against each other.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="other">The other Array.</param>
  /// <param name="comparer">The value comparer; optional: uses default.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this TItem[] @this, TItem[] other, IEqualityComparer<TItem> comparer = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    comparer ??= EqualityComparer<TItem>.Default;

    var thisElements = @this.Length;
    var otherElements = other.Length;

    const int _SMALL_THRESHOLD = 10_000;
    const int _MID_THRESHOLD = 1_000_000;

    return thisElements switch {
      < _SMALL_THRESHOLD when otherElements < _SMALL_THRESHOLD => _CompareToLCS(@this, other, comparer),
      < _MID_THRESHOLD when otherElements < _MID_THRESHOLD => _CompareToLookupTable(@this, other, comparer),
      _ => _CompareToNaïve(@this, other, comparer)
    };
  }

  private static IEnumerable<IChangeSet<TItem>> _CompareToNaïve<TItem>(TItem[] currentState, TItem[] oldState, IEqualityComparer<TItem> comparer) {
    var oldStateIndex = 0;
    Queue<int> currentSourceBuffer = new();

    for (var i = 0; i < currentState.Length; ++i) {
      var item = currentState[i];
      var foundAt = oldState.IndexOf(item, oldStateIndex, comparer);
      if (foundAt < 0) {
        // does not exist in target
        currentSourceBuffer.Enqueue(i);
        continue;
      }

      // found
      while (oldStateIndex <= foundAt) {
        if (oldStateIndex == foundAt) {
          // last iteration
          while (currentSourceBuffer.Count > 0) {
            var index = currentSourceBuffer.Dequeue();
            yield return new ChangeSet<TItem>(ChangeType.Added, index, currentState[index], _INDEX_WHEN_NOT_FOUND, default);
          }

          yield return new ChangeSet<TItem>(ChangeType.Equal, i, currentState[i], oldStateIndex, oldState[oldStateIndex]);
        } else {
          if (currentSourceBuffer.Count > 0) {
            var index = currentSourceBuffer.Dequeue();
            yield return new ChangeSet<TItem>(ChangeType.Changed, index, currentState[index], oldStateIndex, oldState[oldStateIndex]);
          } else
            yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldStateIndex, oldState[oldStateIndex]);
        }

        ++oldStateIndex;
      }
    }

    var targetLen = oldState.Length;
    while (currentSourceBuffer.Count > 0)
      if (oldStateIndex < targetLen) {
        var index = currentSourceBuffer.Dequeue();
        yield return new ChangeSet<TItem>(ChangeType.Changed, index, currentState[index], oldStateIndex, oldState[oldStateIndex]);
        ++oldStateIndex;
      } else {
        var index = currentSourceBuffer.Dequeue();
        yield return new ChangeSet<TItem>(ChangeType.Added, index, currentState[index], _INDEX_WHEN_NOT_FOUND, default);
      }

    while (oldStateIndex < targetLen) {
      yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldStateIndex, oldState[oldStateIndex]);
      ++oldStateIndex;
    }
  }

  private static IEnumerable<IChangeSet<TItem>> _CompareToLookupTable<TItem>(TItem[] currentState, TItem[] oldState, IEqualityComparer<TItem> comparer) {
    // 'indexes' contains all indices where a value was found in ascending order without duplicates
    // 'start' is the first index we would accept, higher or equal is OK if present, lower is not
    static int LookupIndex(IList<int> indexes, int start) {
      var right = indexes.Count - 1;
      switch (right) {
        case < 0: return _INDEX_WHEN_NOT_FOUND;
        case 0:
          // Only one element present
          return indexes[0] >= start ? indexes[0] : _INDEX_WHEN_NOT_FOUND;
        case <= 8:
          // When the list is small, a linear search is sufficient and can be faster.
          foreach (var i in indexes)
            if (i >= start)
              return i;

          return _INDEX_WHEN_NOT_FOUND;
      }

      var left = 0;
      while (left <= right) {
        var middle = left + (right - left) / 2;
        var i = indexes[middle];
        if (i < start)
          left = middle + 1;
        if (i > start)
          right = middle - 1;
        else
          return start;
      }

      return left < indexes.Count && indexes[left] >= start ? indexes[left] : _INDEX_WHEN_NOT_FOUND;
    }

    Dictionary<TItem, List<int>> oldPositions = new(comparer);
    List<int> nullPositions = [];
    for (var i = 0; i < oldState.Length; ++i) {
      var current = oldState[i];
      if (current is null)
        nullPositions.Add(i);
      else
        oldPositions.GetOrAdd(current, () => []).Add(i);
    }

    var oldStateIndex = 0;
    Queue<int> currentSourceBuffer = [];

    for (var i = 0; i < currentState.Length; ++i) {
      var item = currentState[i];

      // use the oldPositions and nullPositions and call LookupIndex to find the item
      var foundAt =
          item is null
            ? LookupIndex(nullPositions, oldStateIndex)
            : oldPositions.TryGetValue(item, out var indexes)
              ? LookupIndex(indexes, oldStateIndex)
              : _INDEX_WHEN_NOT_FOUND
        ;

      if (foundAt < 0) {
        // does not exist in target
        currentSourceBuffer.Enqueue(i);
        continue;
      }

      // found
      while (oldStateIndex <= foundAt) {
        if (oldStateIndex == foundAt) {
          // last iteration
          while (currentSourceBuffer.Count > 0) {
            var index = currentSourceBuffer.Dequeue();
            yield return new ChangeSet<TItem>(ChangeType.Added, index, currentState[index], _INDEX_WHEN_NOT_FOUND, default);
          }

          yield return new ChangeSet<TItem>(ChangeType.Equal, i, currentState[i], oldStateIndex, oldState[oldStateIndex]);
        } else {
          if (currentSourceBuffer.Count > 0) {
            var index = currentSourceBuffer.Dequeue();
            yield return new ChangeSet<TItem>(ChangeType.Changed, index, currentState[index], oldStateIndex, oldState[oldStateIndex]);
          } else
            yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldStateIndex, oldState[oldStateIndex]);
        }

        ++oldStateIndex;
      }
    }

    var targetLen = oldState.Length;
    while (currentSourceBuffer.Count > 0)
      if (oldStateIndex < targetLen) {
        var index = currentSourceBuffer.Dequeue();
        yield return new ChangeSet<TItem>(ChangeType.Changed, index, currentState[index], oldStateIndex, oldState[oldStateIndex]);
        ++oldStateIndex;
      } else {
        var index = currentSourceBuffer.Dequeue();
        yield return new ChangeSet<TItem>(ChangeType.Added, index, currentState[index], _INDEX_WHEN_NOT_FOUND, default);
      }

    while (oldStateIndex < targetLen) {
      yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldStateIndex, oldState[oldStateIndex]);
      ++oldStateIndex;
    }
  }

  private static IEnumerable<IChangeSet<TItem>> _CompareToLCS<TItem>(TItem[] currentState, TItem[] oldState, IEqualityComparer<TItem> comparer) {
    static List<T> LongestCommonSubsequence<T>(T[] seq1, T[] seq2, IEqualityComparer<T> comparer) {
      var seq1Length = seq1.Length;
      var seq2Length = seq2.Length;
      var lcsTable = new int[seq1Length + 1, seq2Length + 1];

      // Build the LCS table
      for (var i = 0; i <= seq1Length; ++i)
      for (var j = 0; j <= seq2Length; ++j)
        if (i == 0 || j == 0)
          lcsTable[i, j] = 0;
        else if (comparer.Equals(seq1[i - 1], seq2[j - 1]))
          lcsTable[i, j] = lcsTable[i - 1, j - 1] + 1;
        else
          lcsTable[i, j] = Math.Max(lcsTable[i - 1, j], lcsTable[i, j - 1]);

      // Recover the LCS from the table
      var lcsList = new List<T>();
      for (int k = seq1Length, l = seq2Length; k > 0 && l > 0;)
        if (comparer.Equals(seq1[k - 1], seq2[l - 1])) {
          lcsList.Add(seq1[k - 1]);
          --k;
          --l;
        } else if (lcsTable[k - 1, l] > lcsTable[k, l - 1])
          --k;
        else
          --l;

      lcsList.Reverse();
      return lcsList;
    }


    // Calculate the LCS between currentState and oldState
    var lcs = LongestCommonSubsequence(currentState, oldState, comparer);

    var currentIndex = 0;
    var oldIndex = 0;
    for (var lcsIndex = 0; currentIndex < currentState.Length && oldIndex < oldState.Length;) {
      var itemsInLcsLeft = lcsIndex < lcs.Count;
      switch (itemsInLcsLeft) {
        // If the current item in both currentState and oldState matches the item in LCS, it is unchanged
        case true
          when comparer.Equals(currentState[currentIndex], lcs[lcsIndex])
               && comparer.Equals(oldState[oldIndex], lcs[lcsIndex]):
          yield return new ChangeSet<TItem>(ChangeType.Equal, currentIndex, currentState[currentIndex], oldIndex, oldState[oldIndex]);
          ++currentIndex;
          ++oldIndex;
          ++lcsIndex;

          continue;

        // If the current item in currentState matches the item in LCS, but the oldState does not, it was removed
        case true
          when comparer.Equals(currentState[currentIndex], lcs[lcsIndex]):
          yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldIndex, oldState[oldIndex]);
          ++oldIndex;

          continue;

        // If the current item in oldState matches the item in LCS, but the currentState does not, it was added
        case true
          when comparer.Equals(oldState[oldIndex], lcs[lcsIndex]):
          yield return new ChangeSet<TItem>(ChangeType.Added, currentIndex, currentState[currentIndex], _INDEX_WHEN_NOT_FOUND, default);
          ++currentIndex;

          continue;

        // Otherwise, the current items in both currentState and oldState do not match the LCS, it was changed
        default:
          yield return new ChangeSet<TItem>(ChangeType.Changed, currentIndex, currentState[currentIndex], oldIndex, oldState[oldIndex]);
          ++currentIndex;
          ++oldIndex;
          continue;
      }
    }

    // If there are remaining items in currentState, they were added
    for (; currentIndex < currentState.Length; ++currentIndex)
      yield return new ChangeSet<TItem>(ChangeType.Added, currentIndex, currentState[currentIndex], _INDEX_WHEN_NOT_FOUND, default);

    // If there are remaining items in oldState, they were removed
    for (; oldIndex < oldState.Length; ++oldIndex)
      yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, oldIndex, oldState[oldIndex]);
  }

  /// <summary>
  ///   Returns the enumeration or <c>null</c> if it is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <returns><c>null</c> if the enumeration is empty; otherwise, the enumeration itself </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem[] ToNullIfEmpty<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this : null;

  /// <summary>
  ///   Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length; negative values mean: till the end.</param>
  /// <returns>An array slice which accesses the underlying array but can only be read.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static ReadOnlySpan<TItem> ReadOnlySlice<TItem>(this TItem[] @this, int start, int length = -1) {
    Against.ThisIsNull(@this);

    return new(@this, start, length < 0 ? @this.Length - start : length);
  }

  /// <summary>
  ///   Slices the specified array for read-only access.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="size">The size of the slices.</param>
  /// <returns>
  ///   An enumeration of read-only slices
  /// </returns>
  [DebuggerStepThrough]
  public static IEnumerable<ReadOnlyArraySlice<TItem>> ReadOnlySlices<TItem>(this TItem[] @this, int size) {
    Against.ThisIsNull(@this);
    Against.NegativeValuesAndZero(size);

    return Invoke(@this, size);

    static IEnumerable<ReadOnlyArraySlice<TItem>> Invoke(TItem[] @this, int size) {
      var length = @this.Length;
      for (var index = 0; index < length; index += size)
        yield return new(@this, index, Math.Min(length - index, size));
    }
  }

  /// <summary>
  ///   Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length; negative values mean: till the end.</param>
  /// <returns>An array slice which accesses the underlying array.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static Span<TItem> Slice<TItem>(this TItem[] @this, int start, int length = -1) {
    Against.ThisIsNull(@this);

    return new(@this, start, length < 0 ? @this.Length - start : length);
  }

  /// <summary>
  ///   Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="size">The size of the slices.</param>
  /// <returns>An enumeration of slices</returns>
  [DebuggerStepThrough]
  public static IEnumerable<ArraySlice<TItem>> Slices<TItem>(this TItem[] @this, int size) {
    Against.ThisIsNull(@this);
    Against.NegativeValuesAndZero(size);

    return Invoke(@this, size);

    static IEnumerable<ArraySlice<TItem>> Invoke(TItem[] @this, int size) {
      var length = @this.Length;
      for (var index = 0; index < length; index += size)
        yield return new(@this, index, Math.Min(length - index, size));
    }
  }

  /// <summary>
  ///   Gets a random element.
  /// </summary>
  /// <typeparam name="TItem">The type of the values.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="random">The random number generator, if any.</param>
  /// <returns>
  ///   A random element from the array.
  /// </returns>
  [DebuggerStepThrough]
  public static TItem GetRandomElement<TItem>(this TItem[] @this, Random random = null) {
    Against.ThisIsNull(@this);

    if (@this.Length == 0)
      AlwaysThrow.InvalidOperationException("No Elements!");

    random ??= Utilities.Random.Shared;

    var index = random.Next(@this.Length);
    return @this[index];
  }

  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index) {
    Against.ThisIsNull(@this);

    return @this.Length <= index ? default : @this[index];
  }

  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, TItem defaultValue) {
    Against.ThisIsNull(@this);

    return @this.Length <= index ? defaultValue : @this[index];
  }

  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="factory">The factory to create default values.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<TItem> factory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(factory);

    return @this.Length <= index ? factory() : @this[index];
  }

  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="factory">The factory to create default values.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<int, TItem> factory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(factory);

    return @this.Length <= index ? factory(index) : @this[index];
  }

  /// <summary>
  ///   Clones the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <returns>A new array or <c>null</c> if this array was <c>null</c>.</returns>
  [DebuggerStepThrough]
  public static TItem[] SafelyClone<TItem>(this TItem[] @this) => (TItem[])@this?.Clone();

  /// <summary>
  ///   Joins the specified elements into a string.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="join">The delimiter.</param>
  /// <param name="skipDefaults">if set to <c>true</c> all default values will be skipped.</param>
  /// <param name="converter">The converter.</param>
  /// <returns>The joines string.</returns>
  [DebuggerStepThrough]
  public static string Join<TItem>(this TItem[] @this, string join = ", ", bool skipDefaults = false, Func<TItem, string> converter = null) {
    Against.ThisIsNull(@this);

    StringBuilder result = new();
    var gotElements = false;
    var defaultValue = default(TItem);

    // ReSharper disable ForCanBeConvertedToForeach
    for (var i = 0; i < @this.Length; ++i) {
      var item = @this[i];
      if (skipDefaults && (ReferenceEquals(item, defaultValue) || EqualityComparer<TItem>.Default.Equals(item, defaultValue)))
        continue;

      if (gotElements)
        result.Append(join);
      else
        gotElements = true;

      result.Append(converter == null ? item is null ? string.Empty : item.ToString() : converter(item));
    }
    // ReSharper restore ForCanBeConvertedToForeach

    return gotElements ? result.ToString() : null;
  }

  /// <summary>
  ///   Splices the specified array (returns part of that array).
  /// </summary>
  /// <typeparam name="TItem">Type of data in the array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="startIndex">The start element which should be included in the splice.</param>
  /// <param name="count">The number of elements from there on.</param>
  /// <returns></returns>
  public static TItem[] Range<TItem>(this TItem[] @this, int startIndex, int count) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(startIndex);
    Against.CountOutOfRange(count, @this.Length - startIndex);

    var result = new TItem[count];
    Array.Copy(@this, startIndex, result, 0, count);
    return result;
  }

  /// <summary>
  ///   Swaps the specified data in an array.
  /// </summary>
  /// <typeparam name="TItem">Type of data in the array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="firstElementIndex">The first value.</param>
  /// <param name="secondElementIndex">The the second value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Swap<TItem>(this TItem[] @this, int firstElementIndex, int secondElementIndex) {
    Against.ThisIsNull(@this);

    (@this[firstElementIndex], @this[secondElementIndex]) = (@this[secondElementIndex], @this[firstElementIndex]);
  }

  /// <summary>
  ///   Shuffles the specified data.
  /// </summary>
  /// <typeparam name="TItem">Type of elements in the array.</typeparam>
  /// <param name="this">This array.</param>
  public static void Shuffle<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    var index = @this.Length;
    Random random = new();
    while (index > 1)
      @this.Swap(random.Next(index), --index);
  }

  /// <summary>
  ///   Quick-sort the given array.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements</typeparam>
  /// <param name="this">This array.</param>
  /// <returns>A sorted array copy.</returns>
  public static TItem[] QuickSorted<TItem>(this TItem[] @this) where TItem : IComparable<TItem> {
    Against.ThisIsNull(@this);

    var result = new TItem[@this.Length];
    @this.CopyTo(result, 0);
    result.QuickSort();
    return result;
  }

  /// <summary>
  ///   Quick-sort the given array.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements.</typeparam>
  /// <param name="this">This array.</param>
  public static void QuickSort<TItem>(this TItem[] @this) where TItem : IComparable<TItem> {
    Against.ThisIsNull(@this);

    if (@this.Length > 0)
      _QuickSort_Comparable(@this, 0, @this.Length - 1);
  }

  private static void _QuickSort_Comparable<TValue>(TValue[] @this, int left, int right) where TValue : IComparable<TValue> {
    if (@this == null)
      // nothing to sort
      return;

    var comparer = Comparer<TValue>.Default;
    var leftIndex = left;
    var rightIndex = right;
    var pivotItem = @this[(leftIndex + rightIndex) >> 1];

    while (leftIndex <= rightIndex) {
      while (comparer.Compare(pivotItem, @this[leftIndex]) > 0)
        ++leftIndex;

      while (comparer.Compare(pivotItem, @this[rightIndex]) < 0)
        --rightIndex;

      if (leftIndex > rightIndex)
        continue;

      @this.Swap(leftIndex, rightIndex);
      ++leftIndex;
      --rightIndex;
    }

    if (left < rightIndex)
      _QuickSort_Comparable(@this, left, rightIndex);

    if (leftIndex < right)
      _QuickSort_Comparable(@this, leftIndex, right);
  }

  /// <summary>
  ///   Converts all elements.
  /// </summary>
  /// <typeparam name="TItem">The type the elements.</typeparam>
  /// <typeparam name="TOutput">The output type.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns>An array containing the converted values.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Converter<TItem, TOutput> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    return Array.ConvertAll(@this, converter);
  }

  /// <summary>
  ///   Converts all elements.
  /// </summary>
  /// <typeparam name="TItem">The type the elements.</typeparam>
  /// <typeparam name="TOutput">The output type.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns>An array containing the converted values.</returns>
  public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Func<TItem, int, TOutput> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    var length = @this.Length;
    var result = new TOutput[length];
    for (var index = length - 1; index >= 0; --index)
      result[index] = converter(@this[index], index);

    return result;
  }

  /// <summary>
  ///   Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    Array.ForEach(@this, action);
  }

#if SUPPORTS_ASYNC
  /// <summary>
  ///   Executes a callback with each element in an array in parallel.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback to execute for each element.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static void ParallelForEach<TItem>(this TItem[] @this, Action<TItem> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    Parallel.ForEach(@this, action);
  }

#endif

  /// <summary>
  ///   Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem, int> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    for (var i = @this.Length - 1; i >= 0; --i)
      action(@this[i], i);
  }

  /// <summary>
  ///   Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem, long> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      action(@this[i], i);
  }

  /// <summary>
  ///   Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, TItem> worker) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(worker);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      @this[i] = worker(@this[i]);
  }

  /// <summary>
  ///   Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, int, TItem> worker) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(worker);

    for (var i = @this.Length - 1; i >= 0; --i)
      @this[i] = worker(@this[i], i);
  }

  /// <summary>
  ///   Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, long, TItem> worker) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(worker);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      @this[i] = worker(@this[i], i);
  }

  /// <summary>
  ///   Returns true if there exists an array
  /// </summary>
  /// <typeparam name="TItem">The type of the input.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns><c>true</c> if a given element exists; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Exists<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return Array.Exists(@this, predicate);
  }

  /// <summary>
  ///   Gets the reverse.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <returns>An array where all values are inverted.</returns>
  public static TItem[] Reverse<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    var length = @this.LongLength;
    var result = new TItem[length];
    for (long i = 0, j = length - 1; j >= 0; ++i, --j)
      result[j] = @this[i];

    return result;
  }

  /// <summary>
  ///   Determines whether the given array contains the specified value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="value">The value.</param>
  /// <returns>
  ///   <c>true</c> if [contains] [the specified this]; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Contains<TItem>(this TItem[] @this, TItem value) {
    Against.ThisIsNull(@this);

    return @this.IndexOf(value) >= 0;
  }

  /// <summary>
  ///   Determines whether an array contains the specified value or not.
  /// </summary>
  /// <param name="this">This array.</param>
  /// <param name="value">The value.</param>
  /// <returns>
  ///   <c>true</c> if the array contains that value; otherwise, <c>false</c>.
  /// </returns>
  public static bool Contains(this Array @this, object value) {
    Against.ThisIsNull(@this);

    // ReSharper disable LoopCanBeConvertedToQuery
    foreach (var item in @this)
      if (item == value)
        return true;
    // ReSharper restore LoopCanBeConvertedToQuery
    return false;
  }

  /// <summary>
  ///   Converts the array instance to a real array.
  /// </summary>
  /// <param name="this">This Array.</param>
  /// <returns>An array of objects holding the contents.</returns>
  public static object[] ToArray(this Array @this) {
    Against.ThisIsNull(@this);

    if (@this.Rank < 1)
      AlwaysThrow.ArgumentException(nameof(@this), "Rank must be > 0");

    var result = new object[@this.Length];
    var lbound = @this.GetLowerBound(0);
    for (var i = @this.Length; i > 0;) {
      --i;
      result[i] = @this.GetValue(i + lbound);
    }

    return result;
  }

  /// <summary>
  ///   Gets the index of the first item matching the predicate, if any or -1.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>
  ///   The index of the item in the array or -1.
  /// </returns>
  public static int IndexOf<TItem>(this TItem[] @this, Predicate<TItem> predicate) => IndexOfOrDefault(@this, predicate, _INDEX_WHEN_NOT_FOUND);

  /// <summary>
  ///   Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>
  ///   The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, int defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValue;
  }

  /// <summary>
  ///   Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValueFactory">The function that generates the default value.</param>
  /// <returns>
  ///   The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<int> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValueFactory();
  }

  /// <summary>
  ///   Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValueFactory">The function that generates the default value.</param>
  /// <returns>
  ///   The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<TItem[], int> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValueFactory(@this);
  }

  /// <summary>
  ///   Gets the index of the first item matching the given, if any or -1.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="value">The value.</param>
  /// <returns>
  ///   The index of the item in the array or -1.
  /// </returns>
  public static int IndexOf<TItem>(this TItem[] @this, TItem value) => IndexOf(@this, value, 0, EqualityComparer<TItem>.Default);

  /// <summary>
  ///   Gets the index of the first item matching the given, if any or -1.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="value">The value.</param>
  /// <param name="offset">The index to start searching</param>
  /// <returns>
  ///   The index of the item in the array or -1.
  /// </returns>
  public static int IndexOf<TItem>(this TItem[] @this, TItem value, int offset) => IndexOf(@this, value, offset, EqualityComparer<TItem>.Default);

  /// <summary>
  ///   Gets the index of the first item matching the given, if any or -1.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="value">The value.</param>
  /// <param name="comparer">The comparer.</param>
  /// <returns>
  ///   The index of the item in the array or -1.
  /// </returns>
  public static int IndexOf<TItem>(this TItem[] @this, TItem value, IEqualityComparer<TItem> comparer) => IndexOf(@this, value, 0, comparer);

  /// <summary>
  ///   Gets the index of the first item matching the given, if any or -1.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="value">The value.</param>
  /// <param name="offset">The index to start searching</param>
  /// <param name="comparer">The comparer.</param>
  /// <returns>
  ///   The index of the item in the array or -1.
  /// </returns>
  public static int IndexOf<TItem>(this TItem[] @this, TItem value, int offset, IEqualityComparer<TItem> comparer) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(offset);
    Against.ArgumentIsNull(comparer);

    for (var i = offset; i < @this.Length; ++i)
      if (ReferenceEquals(value, @this[i]) || comparer.Equals(value, @this[i]))
        return i;

    return _INDEX_WHEN_NOT_FOUND;
  }

  /// <summary>
  ///   Gets the index of the first item matching the predicate, if any or -1.
  /// </summary>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>The index of the item in the array or -1.</returns>
  [DebuggerStepThrough]
  public static int IndexOf(this Array @this, Predicate<object> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
      if (predicate(@this.GetValue(i)))
        return i;

    return _INDEX_WHEN_NOT_FOUND;
  }

  /// <summary>
  ///   Rotates all elements in the array one index down.
  /// </summary>
  /// <typeparam name="TItem">The type of the array elements</typeparam>
  /// <param name="this">This array</param>
  public static void RotateTowardsZero<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    if (@this.Length == 0)
      return;

    var first = @this[0];
    for (var i = 1; i < @this.Length; ++i)
      @this[i - 1] = @this[i];

    @this[^1] = first;
  }

  /// <summary>
  ///   Allows processing an array in chunks
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This Array</param>
  /// <param name="chunkSize">The maximum chunk size to process</param>
  /// <param name="processor">The action to execute on each chunk</param>
  public static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor)
    => ProcessInChunks(@this, chunkSize, processor, @this.Length, 0);

  /// <summary>
  ///   Allows processing an array in chunks
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This Array</param>
  /// <param name="chunkSize">The maximum chunk size to process</param>
  /// <param name="processor">The action to execute on each chunk</param>
  /// <param name="length"></param>
  /// <param name="offset">Optional: an offset to start at</param>
  public static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor, int length, int offset = 0) {
    if (offset < 0)
      throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must be >= 0, is {offset}");
    if (length <= 0)
      throw new ArgumentOutOfRangeException(nameof(length));

    while (offset < length) {
      var size = Math.Min(length - offset, chunkSize);
      processor(@this, offset, size);
      offset += size;
    }
  }

  /// <summary>
  ///   Determines whether the given array is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns>
  ///   <c>true</c> if the array reference is <c>null</c> or the array has no elements; otherwise, <c>false</c>
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNullEmpty<TItem>([NotNullWhen(false)] this TItem[] @this) => @this is not { Length: > 0 };

  /// <summary>
  ///   Determines whether the given array is not empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns>
  ///   <c>true</c> if the array reference is not <c>null</c> and the array has elements; otherwise, <c>false</c>
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotNullEmpty<TItem>([NotNullWhen(true)] this TItem[] @this) => @this is { Length: > 0 };

  /// <summary>
  ///   Initializes a jagged array with default values.
  /// </summary>
  /// <typeparam name="TArray">The type of the array</typeparam>
  /// <param name="lengths">The lengths in all dimensions</param>
  /// <returns>The resulting array</returns>
  public static TArray CreatedJaggedArray<TArray>(params int[] lengths) {
    // ReSharper disable once VariableHidesOuterVariable
    object _InitializeJaggedArray(Type arrayType, int index, int[] lengths) {
      // create array in current dimension
      var result = Array.CreateInstance(arrayType, lengths[index]);
      var elementType = arrayType.GetElementType();
      if (elementType == null)
        return result;

      // set next sub-dimension
      var nextIndex = index + 1;
      for (var i = 0; i < lengths[index]; ++i)
        result.SetValue(_InitializeJaggedArray(elementType, nextIndex, lengths), i);

      return result;
    }

    return (TArray)_InitializeJaggedArray(typeof(TArray).GetElementType(), 0, lengths);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringInstance(this char[] @this) => new(@this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringInstance(this char[] @this, int startIndex) => @this == null || @this.Length <= startIndex ? string.Empty : new(@this, startIndex, @this.Length - startIndex);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringInstance(this char[] @this, int startIndex, int length) => @this == null || length < 1 || @this.Length <= startIndex ? string.Empty : new(@this, startIndex, length);

  #region high performance linq for arrays

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static bool Any<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    return @this.Length > 0;
  }

  [DebuggerStepThrough]
  public static bool Any<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; i++)
      if (predicate(@this[i]))
        return true;

    return false;
  }

  [DebuggerStepThrough]
  public static TItem First<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    if (@this.Length == 0)
      AlwaysThrow.InvalidOperationException("No Elements!");

    return @this[0];
  }

  [DebuggerStepThrough]
  public static TItem Last<TItem>(this TItem[] @this) {
    Against.ThisIsNull(@this);

    var length = @this.LongLength;
    if (length == 0)
      AlwaysThrow.InvalidOperationException("No Elements!");

    return @this[^1];
  }

  /// <summary>
  ///   Tries to get the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="result">The value or the <see langword="default" /> for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be retrieved; otherwise, <see langword="false" />.</returns>
  public static bool TryGetFirst<T>(this T[] @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Length > 0) {
      result = @this[0];
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  ///   Tries to get the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="result">The value or the <see langword="default" /> for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be retrieved; otherwise, <see langword="false" />.</returns>
  public static bool TryGetLast<T>(this T[] @this, out T result) {
    Against.ThisIsNull(@this);

    if (@this.Length <= 0) {
      result = default;
      return false;
    }

    result = @this[^1];
    return true;
  }

  /// <summary>
  ///   Tries to get the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="index">The items' position</param>
  /// <param name="result">The value or the <see langword="default" /> for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be retrieved; otherwise, <see langword="false" />.</returns>
  public static bool TryGetItem<T>(this T[] @this, int index, out T result) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Length <= index) {
      result = default;
      return false;
    }

    result = @this[index];
    return true;
  }

  /// <summary>
  ///   Tries to set the first item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be assigned; otherwise, <see langword="false" />.</returns>
  public static bool TrySetFirst<T>(this T[] @this, T value) {
    Against.ThisIsNull(@this);

    if (@this.Length <= 0)
      return false;

    @this[0] = value;
    return true;
  }

  /// <summary>
  ///   Tries to set the last item.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be assigned; otherwise, <see langword="false" />.</returns>
  public static bool TrySetLast<T>(this T[] @this, T value) {
    Against.ThisIsNull(@this);

    if (@this.Length <= 0)
      return false;

    @this[^1] = value;
    return true;
  }

  /// <summary>
  ///   Tries to set the item at the given index.
  /// </summary>
  /// <typeparam name="T">The type of the item.</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <param name="index">The items' position</param>
  /// <param name="value">The value for the given datatype.</param>
  /// <returns><see langword="true" /> when the item could be assigned; otherwise, <see langword="false" />.</returns>
  public static bool TrySetItem<T>(this T[] @this, int index, T value) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    if (@this.Length <= index)
      return false;

    @this[index] = value;
    return true;
  }

  [DebuggerStepThrough]
  public static TItem First<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; ++i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    AlwaysThrow.InvalidOperationException("No Elements!");
    return default;
  }

  [DebuggerStepThrough]
  public static TItem Last<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = @this.LongLength - 1; i >= 0; --i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    AlwaysThrow.InvalidOperationException("No Elements!");
    return default;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this[0] : default;

  [DebuggerStepThrough]
  public static TItem LastOrDefault<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this[^1] : default;

  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; ++i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    return default;
  }

  [DebuggerStepThrough]
  public static TItem LastOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    for (var i = @this.LongLength - 1; i >= 0; --i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    return default;
  }

  [DebuggerStepThrough]
  public static TItem Aggregate<TItem>(this TItem[] @this, Func<TItem, TItem, TItem> func) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(func);

    if (@this.LongLength == 0)
      AlwaysThrow.InvalidOperationException("No Elements!");

    var result = @this[0];
    for (var i = 1; i < @this.LongLength; ++i)
      result = func(result, @this[i]);

    return result;
  }

  [DebuggerStepThrough]
  public static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] @this, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(func);

    if (@this.LongLength == 0)
      AlwaysThrow.InvalidOperationException("No Elements!");

    var result = seed;
    for (var i = 0; i < @this.LongLength; ++i)
      result = func(result, @this[i]);

    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static int Count<TItem>(this TItem[] @this) => @this.Length;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static long LongCount<TItem>(this TItem[] @this) => @this.LongLength;

  [DebuggerStepThrough]
  public static int Count<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    var result = 0;
    // ReSharper disable LoopCanBeConvertedToQuery
    // ReSharper disable ForCanBeConvertedToForeach
    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        ++result;
    // ReSharper restore ForCanBeConvertedToForeach
    // ReSharper restore LoopCanBeConvertedToQuery
    return result;
  }

  [DebuggerStepThrough]
  public static long LongCount<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    var result = (long)0;
    for (var i = 0; i < @this.LongLength; ++i)
      if (predicate(@this[i]))
        ++result;

    return result;
  }


  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, TItem defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  #region these special Array ...s

  [DebuggerStepThrough]
  public static IEnumerable<TResult> OfType<TResult>(this Array @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<TResult> Invoke(Array @this) {
      for (var i = 0; i < @this.LongLength; ++i) {
        var item = @this.GetValue(i);
        if (item is TResult result)
          yield return result;
      }
    }
  }

  [DebuggerStepThrough]
  public static object FirstOrDefault(this Array @this, Predicate<object> predicate, object defaultValue = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  [DebuggerStepThrough]
  public static IEnumerable<object> Reverse(this Array @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<object> Invoke(Array @this) {
      for (var i = @this.GetUpperBound(0); i >= @this.GetLowerBound(0); --i)
        yield return @this.GetValue(i);
    }
  }

  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this Array @this, Predicate<TItem> predicate, TItem defaultValue = default) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate((TItem)item))
        return (TItem)item;

    return defaultValue;
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Cast<TResult>(this Array @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<TResult> Invoke(Array @this) {
      for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
        yield return (TResult)@this.GetValue(i);
    }
  }

  #endregion

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(TItem[] @this, Func<TItem, TResult> selector) {
      var length = @this.Length;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i]);
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(TItem[] @this, Func<TItem, TResult> selector) {
      var length = @this.LongLength;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i]);
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, int, TResult> selector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(TItem[] @this, Func<TItem, int, TResult> selector) {
      var length = @this.Length;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i], i);
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, long, TResult> selector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return Invoke(@this, selector);

    static IEnumerable<TResult> Invoke(TItem[] @this, Func<TItem, long, TResult> selector) {
      var length = @this.LongLength;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i], i);
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return Invoke(@this, predicate);

    static IEnumerable<TItem> Invoke(TItem[] @this, Predicate<TItem> predicate) {
      var length = @this.LongLength;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current))
          yield return current;
      }
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Func<TItem, int, bool> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return Invoke(@this, predicate);

    static IEnumerable<TItem> Invoke(TItem[] @this, Func<TItem, int, bool> predicate) {
      var length = @this.Length;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current, i))
          yield return current;
      }
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> WhereLong<TItem>(this TItem[] @this, Func<TItem, long, bool> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return Invoke(@this, predicate);

    static IEnumerable<TItem> Invoke(TItem[] @this, Func<TItem, long, bool> predicate) {
      var length = @this.LongLength;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current, i))
          yield return current;
      }
    }
  }

  #endregion

  #region byte-array specials

  /// <summary>
  /// Converts the byte array to a binary string representation.
  /// </summary>
  /// <param name="this">The byte array to convert.</param>
  /// <returns>A string representing the binary format of the byte array.</returns>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// byte[] bytes = { 0xA3, 0x1F };
  /// string binaryString = bytes.ToBin();
  /// Console.WriteLine(binaryString); 
  /// // Output: "1010001100011111"
  /// </code>
  /// </example>
  public static unsafe string ToBin(this byte[] @this) {
    if (@this == null)
      return null;

    const ulong ZERO = '0';
    const ulong ZEROZEROZEROZERO = ZERO | (ZERO << 16) | (ZERO << 32) | (ZERO << 48);
    var result = new char[@this.Length << 3];
    fixed (char* resultFixed = &result[0]) {
      var resultPointer = resultFixed;
      for (var i = 0; i < @this.Length; ++i, resultPointer += 8) {

        // This part converts a single byte into its binary representation as 8 characters ('0' or '1'), 
        // with each character being stored as a 16-bit value (totaling 128 bits, split into two 64-bit ulongs).
        // Extract the current byte and cast it to a 64-bit unsigned integer
        var value = (ulong)(@this[i] & 0xff);

        // The following steps will:
        //   * Convert each bit of the byte into a character ('0' or '1') in ASCII form, padded with a leading zero byte.
        //   * Reverse the order of bits due to little-endian (LE) system requirements.
        //   * Use multiplication to spread the bits across 128 bits (two 64-bit ulongs). [fac1/fac2]
        //   * Add necessary bits that aren't captured during multiplication using bitwise shifts. [r1/r2]
        //   * Combine these intermediate results using OR operations.
        //   * Mask out unwanted bits to isolate the correct binary representation.
        //   * Add the ASCII '0' characters to convert the bits into '0'/'1' characters.
        //   * Store the final 128-bit binary string representation in memory.
        //
        //            in : 0b_abcdefgh
        //
        //          out1 : 0b_00000000_0000000h_00000000_0000000g_00000000_0000000f_00000000_0000000e
        //          int1 : 0b_00000000_abcdefgh_00000000_0abcdefg_h0000000_00abcdef_gh000000_00000000
        //            r1 : 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_000abcde
        const ulong fac1 = 0b_00000000_00000001_00000000_00000000_10000000_00000000_01000000_00000000UL;
        //
        //          out2 : 0b_00000000_0000000d_00000000_0000000c_00000000_0000000b_00000000_0000000a
        //          int2 : 0b_00000000_0000abcd_efgh0000_00000abc_defgh000_000000ab_cdefgh00_00000000
        //            r2 : 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_0000000a
        const ulong fac2 = 0b_00000000_00000000_00010000_00000000_00001000_00000000_00000100_00000000UL;
        const ulong mask = 0b_00000000_00000001_00000000_00000001_00000000_00000001_00000000_00000001UL;

        var r1 = value >> 3;
        var r2 = value >> 7;

        var hi = value * fac1;
        var lo = value * fac2;

        hi |= r1;
        lo |= r2;

        hi &= mask;
        lo &= mask;

        hi += ZEROZEROZEROZERO;
        lo += ZEROZEROZEROZERO;

        *(ulong*)resultPointer = lo;
        ((ulong*)resultPointer)[1] = hi;
      }
    }

    return result.ToStringInstance();
  }

  /// <summary>
  ///   Converts the bytes to a hex representation.
  /// </summary>
  /// <param name="this">These Bytes</param>
  /// <param name="allUpperCase">Uses upper-case (<c>true</c>) hex letters only or lower-case (<c>false</c>).</param>
  /// <returns>A hex string or <c>null</c></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToHex(this byte[] @this, bool allUpperCase = false) {
    if (@this == null)
      return null;

    const byte ZERO = (byte)'0';
    const byte LOWA = (byte)'a' - 10;
    const byte HIGHA = (byte)'A' - 10;

    const int ZEROZERO = ZERO | (ZERO << 8);
    const int ZERO_TO_LOWA_DELTA = LOWA - ZERO;
    const int ZERO_TO_HIGHA_DELTA = HIGHA - ZERO;

    var letterDelta = allUpperCase ? ZERO_TO_HIGHA_DELTA : ZERO_TO_LOWA_DELTA;
    var result = new char[@this.Length << 1];
    for (int i = 0, j = 0; i < @this.Length && j < result.Length - 1; ++i, j += 2) {
      var value = @this[i];

      var hi = (value & 0xf0) << 4;
      var lo = value & 0x0f;
      var both = hi + lo + ZEROZERO; // bring into number range
      var mask = both - ZEROZERO + 0x0606; // let value above or equal 10 overflow into the next digit
      mask &= 0x1010; // get a mask containing what overflowed
      mask >>>= 4; // push down one nibble
      mask *= letterDelta; // multiply with difference to numbers
      both += mask; // add delta to letters

      hi = both >> 8;
      lo = both & 0xff;

      result[j] = (char)hi;
      result[j + 1] = (char)lo;
    }

    return result.ToStringInstance();
  }

  /// <summary>
  ///   Creates random data in the given buffer; thus effectively overwriting it in-place.
  /// </summary>
  /// <param name="this">This buffer.</param>
  /// <returns>The given buffer</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static void RandomizeBuffer(this byte[] @this) {
#if SUPPORTS_RNG_FILL
    RandomNumberGenerator.Fill(@this);
#else
#if NEEDS_RNG_DISPOSE
    using RNGCryptoServiceProvider provider = new();
    provider.GetBytes(@this);
#else
    new RNGCryptoServiceProvider().GetBytes(@this);
#endif
#endif
  }

  /// <summary>
  ///   Gets a small portion of a byte array.
  /// </summary>
  /// <param name="this">This byte[].</param>
  /// <param name="offset">The offset.</param>
  /// <param name="count">The count.</param>
  /// <returns>The subsequent elements.</returns>
  [DebuggerStepThrough]
  public static byte[] Range(this byte[] @this, int offset, int count) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(offset);
    Against.CountBelowOrEqualZero(count);

    var length = @this.Length;
    var max = count < length - offset ? count : length - offset;
    var result = new byte[max];
    if (max > 0)
      Buffer.BlockCopy(@this, offset, result, 0, max);

    return result;
  }

  /// <summary>
  ///   Padds the specified byte array to a certain length if it is smaller.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <param name="length">The final length.</param>
  /// <param name="data">The data to use for padding, default to null-bytes.</param>
  /// <returns>The original array if it already exceeds the wanted size, or an array with the correct size.</returns>
  [DebuggerStepThrough]
  public static byte[] Padd(this byte[] @this, int length, byte data = 0) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(length);

    var currentSize = @this.Length;
    if (currentSize >= length)
      return @this;

    var result = new byte[length];
    if (currentSize > 0)
      Buffer.BlockCopy(@this, 0, result, 0, currentSize);

    for (var i = currentSize; i < length; ++i)
      result[i] = data;

    return result;
  }

  #region compression

  /// <summary>
  ///   GZips the given bytes.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>A GZipped byte array.</returns>
  [DebuggerStepThrough]
  public static byte[] GZip(this byte[] @this) {
    Against.ThisIsNull(@this);

    using MemoryStream targetStream = new();
    using (GZipStream gZipStream = new(targetStream, CompressionMode.Compress, false))
      gZipStream.Write(@this, 0, @this.Length);

    return targetStream.ToArray();
  }

  /// <summary>
  ///   Un-GZips the given bytes.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The unzipped byte array.</returns>
  [DebuggerStepThrough]
  public static byte[] UnGZip(this byte[] @this) {
    Against.ThisIsNull(@this);

    using MemoryStream targetStream = new();
    using (MemoryStream sourceStream = new(@this))
    using (GZipStream gZipStream = new(sourceStream, CompressionMode.Decompress, false)) {
      // decompress all bytes
      var buffer = new byte[64 * 1024];
      var bytesRead = gZipStream.Read(buffer, 0, buffer.Length);
      while (bytesRead > 0) {
        targetStream.Write(buffer, 0, bytesRead);
        bytesRead = gZipStream.Read(buffer, 0, buffer.Length);
      }
    }

    return targetStream.ToArray();
  }

  #endregion

  #region Byte Array IndexOf

  private static int _GetInvalidIndex(byte[] _, byte[] __) => _INDEX_WHEN_NOT_FOUND;

  public static int IndexOfOrMinusOne(this byte[] @this, byte[] searchString, int offset = 0)
    => IndexOfOrDefault(@this, searchString, offset, _GetInvalidIndex);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int defaultValue)
    => IndexOfOrDefault(@this, searchString, 0, (_, _) => defaultValue);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, (_, _) => defaultValueFunc());

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, (t, _) => defaultValueFunc(t));

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, defaultValueFunc);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, int defaultValue)
    => IndexOfOrDefault(@this, searchString, offset, (_, _) => defaultValue);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, offset, (_, _) => defaultValueFunc());

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, offset, (t, _) => defaultValueFunc(t));

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], byte[], int> defaultValueFunc) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(searchString);

    if (ReferenceEquals(@this, searchString))
      return 0;

    var searchStringLength = searchString.Length;
    var dataStringLength = @this.Length;

    if (searchStringLength < 1)
      return 0;

    if (dataStringLength + offset < searchStringLength)
      return _INDEX_WHEN_NOT_FOUND;

    // ReSharper disable once JoinDeclarationAndInitializer
    int index;
#if PLATFORM_X86
    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
    if (searchStringLength <= 32)
      index = _ContainsBNDM(@this, searchString, offset);
#else
    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
    if (searchStringLength <= 64)
      index = _ContainsBNDM(@this, searchString, offset);
#endif
    else
      index = _ContainsBoyerMoore(@this, searchString, offset);

    return index < 0 && defaultValueFunc != null ? defaultValueFunc(@this, searchString) : index;
  }

  private static int _ContainsNaïve(byte[] haystack, byte[] needle, int offset) {
    var searchStringLength = needle.Length;
    var dataStringLength = haystack.Length - offset;

    for (var i = offset; i < dataStringLength; ++i) {
      var found = true;
      for (var j = 0; j < searchStringLength; ++j) {
        if (haystack[i + j] == needle[j])
          continue;

        found = false;
        break;
      }

      if (found)
        return i;
    }

    return _INDEX_WHEN_NOT_FOUND;
  }

  // maximum Length allowed for @this and searchByteArray = 32/64
  // ReSharper disable once SuggestBaseTypeForParameter
  private static unsafe int _ContainsBNDM(byte[] haystack, byte[] needle, int offset) {
    var searchStringLength = needle.Length;
    var dataStringLength = haystack.Length;

#if PLATFORM_X86
    var matchArray = stackalloc uint[256];
#else
    var matchArray = stackalloc ulong[256];
#endif

    int i;
    /* Pre-processing */
#if PLATFORM_X86
    var s = 1U;
#else
    var s = 1UL;
#endif
    for (i = searchStringLength - 1; i >= 0; --i) {
      matchArray[needle[i]] |= s;
      s <<= 1;
    }

    /* Searching phase */
    var j = offset;
    while (j <= dataStringLength - searchStringLength) {
      i = searchStringLength - 1;
      var last = searchStringLength;
#if PLATFORM_X86
      s = ~0U;
#else
      s = ~0UL;
#endif
      while (i >= 0 && s != 0) {
        s &= matchArray[haystack[j + i]];
        --i;
        if (s != 0) {
          if (i >= 0)
            last = i + 1;
          else
            return j;
        }

        s <<= 1;
      }

      j += last;
    }

    return _INDEX_WHEN_NOT_FOUND;
  }

  private static int _ContainsBoyerMoore(byte[] haystack, byte[] needle, int offset) {
    var searchStringLength = needle.Length;
    var dataStringLength = haystack.Length;

    Dictionary<byte, int> dictComparisonBytes = new();
    for (var j = 0; j < searchStringLength; ++j) {
      var value = searchStringLength - j - 1;
      if (dictComparisonBytes.ContainsKey(needle[j]))
        dictComparisonBytes[needle[j]] = value;
      else
        dictComparisonBytes.Add(needle[j], value);
    }

    var i = offset; // First index to check.

    // Loop while there's still room for search term
    while (i <= dataStringLength - searchStringLength) {
      // Look if we have a match at this position
      var j = searchStringLength - 1;
      while (j >= 0 && needle[j] == haystack[i + j])
        --j;

      // Match found
      if (j < 0)
        return i;

      // Advance to next comparision
      var k = 1 + j;
      i += dictComparisonBytes.TryGetValue(haystack[i + j], out var x) ? Math.Max(x - searchStringLength + k, 1) : k;
    }

    // No Match found
    return _INDEX_WHEN_NOT_FOUND;
  }

  #endregion

  #region hash computation

#if !NET6_0_OR_GREATER
  /// <summary>
  ///   Computes the hash.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The result of the hash algorithm</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeHash<THashAlgorithm>(this byte[] @this) where THashAlgorithm : HashAlgorithm, new() {
    Against.ThisIsNull(@this);

    using THashAlgorithm provider = new();
    return provider.ComputeHash(@this);
  }

#endif

  /// <summary>
  ///   Calculates the SHA512 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeSHA512Hash(this byte[] @this) {
    Against.ThisIsNull(@this);

    using var provider = SHA512.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  ///   Calculates the SHA384 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeSHA384Hash(this byte[] @this) {
    Against.ThisIsNull(@this);

    using var provider = SHA384.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  ///   Calculates the SHA256 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeSHA256Hash(this byte[] @this) {
    Against.ThisIsNull(@this);

    using var provider = SHA256.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  ///   Calculates the SHA-1 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeSHA1Hash(this byte[] @this) {
    Against.ThisIsNull(@this);

    using var provider = SHA1.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  ///   Calculates the MD5 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static byte[] ComputeMD5Hash(this byte[] @this) {
    Against.ThisIsNull(@this);

    using var provider = MD5.Create();
    return provider.ComputeHash(@this);
  }

  #endregion

  #endregion

  /// <summary>
  ///   Determines whether a given <see cref="Array" /> contains exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns><see langword="true" /> if the <see cref="Array" /> has one element; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSingle<TValue>(this TValue[] @this) {
    Against.ThisIsNull(@this);
    return @this.Length == 1;
  }

  /// <summary>
  ///   Determines whether a given <see cref="Array" /> contains exactly more than one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns>
  ///   <see langword="true" /> if the <see cref="Array" /> has more than one element; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsMultiple<TValue>(this TValue[] @this) {
    Against.ThisIsNull(@this);
    return @this.Length > 1;
  }

  /// <summary>
  ///   Determines whether a given <see cref="Array" /> contains not exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns>
  ///   <see langword="true" /> if the <see cref="Array" /> has more or less than one element; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNoSingle<TValue>(this TValue[] @this) {
    Against.ThisIsNull(@this);
    return @this.Length != 1;
  }

  /// <summary>
  ///   Determines whether a given <see cref="Array" /> contains less than two elements.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array" /></param>
  /// <returns>
  ///   <see langword="true" /> if the <see cref="Array" /> has less than two elements; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNoMultiple<TValue>(this TValue[] @this) {
    Against.ThisIsNull(@this);
    return @this.Length <= 1;
  }
}
