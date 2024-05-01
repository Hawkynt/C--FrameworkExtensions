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

using System.Collections.Concurrent;
using System.Diagnostics;
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable MemberCanBePrivate.Global

namespace System.Collections.Generic;

using Guard;

public static partial class EnumerableExtensions {

  #region nested types

  public enum ChangeType {
    Equal = 0,
    Changed = 1,
    Added = 2,
    Removed = 3,
  }

  public interface IChangeSet<out TItem> {
    ChangeType Type { get; }
    int CurrentIndex { get; }
    TItem Current { get; }
    int OtherIndex { get; }
    TItem Other { get; }
  }

  private class ChangeSet<TItem> : IChangeSet<TItem> {
    public ChangeSet(ChangeType type, int currentIndex, TItem current, int otherIndex, TItem other) {
      this.Type = type;
      this.CurrentIndex = currentIndex;
      this.Current = current;
      this.OtherIndex = otherIndex;
      this.Other = other;
    }

    #region Implementation of IChangeSet<TValue>

    public ChangeType Type { get; }
    public int CurrentIndex { get; }
    public TItem Current { get; }
    public int OtherIndex { get; }
    public TItem Other { get; }

    #endregion
  }

  /// <summary>
  /// An IEnumerable of Disposables whose elements can also be accessed by an indexer
  /// </summary>
  /// <typeparam name="T">The type of the items in the IEnumerable (has to be IDisposable)</typeparam>
  public interface IDisposableCollection<T> : IEnumerable<T>, IDisposable where T : IDisposable {
    T this[int i] { get; }
  }

  private class DisposableCollection<T> : List<T>, IDisposableCollection<T> where T : IDisposable {
    public DisposableCollection(IEnumerable<T> collection) : base(collection) { }

    public void Dispose() {
      this.ForEach(i => i.Dispose());
    }
  }

  #endregion

  /// <summary>
  /// Appends the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="items">The items to append at the start.</param>
  /// <returns>The new <see cref="IEnumerable{T}"/></returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
  public static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> @this, params TItem[] items) {
    Against.ArgumentIsNull(@this);
    Against.ArgumentIsNull(items);

    return Invoke(@this, items);
    
    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, TItem[] items) {
      foreach (var i in items)
        yield return i;

      foreach (var i in @this)
        yield return i;
    }
  }

  /// <summary>
  /// Appends the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="items">The items to append at the end.</param>
  /// <returns>The new <see cref="IEnumerable{T}"/></returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
  public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> @this, params TItem[] items) {
    Against.ArgumentIsNull(@this);
    Against.ArgumentIsNull(items);

    return Invoke(@this, items);
    
    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, TItem[] items) {
      foreach (var i in @this)
        yield return i;

      foreach (var i in items)
        yield return i;
    }
  }

  /// <summary>
  /// Prepends the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="items">The items to append at the start.</param>
  /// <returns>The new <see cref="IEnumerable{T}"/></returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
  public static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items) {
    Against.ArgumentIsNull(@this);
    Against.ArgumentIsNull(items);

    return Invoke(@this, items);
    
    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, IEnumerable<TItem> items) {
      foreach (var i in items)
        yield return i;

      foreach (var i in @this)
        yield return i;
    }
  }

  /// <summary>
  /// Appends the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="items">The items to append at the end.</param>
  /// <returns>The new <see cref="IEnumerable{T}"/></returns>
  /// <exception cref="ArgumentNullException">When the given <see cref="IEnumerable{T}"/> is <see langword="null"/></exception>
  public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items) {
    Against.ArgumentIsNull(@this);
    Against.ArgumentIsNull(items);

    return Invoke(@this, items);

    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, IEnumerable<TItem> items) {
      foreach (var i in @this)
        yield return i;

      foreach (var i in items)
        yield return i;
    }
  }

  /// <summary>
  /// Modifies the resultset to include filtering based on the given query string.
  /// Multiple filters may be present, split by whitespaces, combined with AND.
  /// </summary>
  /// <typeparam name="TRow">The type of rows</typeparam>
  /// <param name="this">This IEnumerable</param>
  /// <param name="query">The query, eg. "green white" (means only entries with "green" AND "white")</param>
  /// <param name="selector">Which column of the record to filter</param>
  public static IEnumerable<TRow> FilterIfNeeded<TRow>(this IEnumerable<TRow> @this, Func<TRow, string> selector, string query) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    if (query.IsNullOrWhiteSpace())
      return @this;

    var results = @this;
    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var filter in query.Trim().Split(' ')) {
      if (filter.IsNullOrWhiteSpace())
        continue;

      results = results.Where(i => selector(i)?.Contains(filter) ?? false);
    }

    return results;
  }

  /// <summary>
  /// Compares two arrays against each other.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="other">The other Array.</param>
  /// <param name="comparer">The value comparer; optional: uses default.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    return Invoke(@this, other, comparer ?? EqualityComparer<TItem>.Default);

    static IEnumerable<IChangeSet<TItem>> Invoke(IEnumerable<TItem> @this, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer) {

      var target = other.ToArray();
      var targetIndex = 0;
      var currentSourceBuffer = new Queue<Tuple<int, TItem>>();

      var i = -1;
      foreach (var item in @this) {
        ++i;
        var foundAt = _IndexOf(target, item, targetIndex, comparer);
        if (foundAt < 0) {
          // does not exist in target
          currentSourceBuffer.Enqueue(Tuple.Create(i, item));
          continue;
        }

        // found
        while (targetIndex <= foundAt) {
          if (targetIndex == foundAt) {
            // last iteration
            while (currentSourceBuffer.Count > 0) {
              var index = currentSourceBuffer.Dequeue();
              yield return new ChangeSet<TItem>(ChangeType.Added, index.Item1, index.Item2, -1, default(TItem));
            }

            yield return new ChangeSet<TItem>(ChangeType.Equal, i, item, targetIndex, target[targetIndex]);
          } else {
            if (currentSourceBuffer.Count > 0) {
              var index = currentSourceBuffer.Dequeue();
              yield return new ChangeSet<TItem>(ChangeType.Changed, index.Item1, index.Item2, targetIndex,
                target[targetIndex]);
            } else
              yield return new ChangeSet<TItem>(ChangeType.Removed, -1, default(TItem), targetIndex,
                target[targetIndex]);
          }

          ++targetIndex;
        }
      }

      var targetLen = target.Length;
      while (currentSourceBuffer.Count > 0) {
        if (targetIndex < targetLen) {
          var index = currentSourceBuffer.Dequeue();
          yield return new ChangeSet<TItem>(ChangeType.Changed, index.Item1, index.Item2, targetIndex,
            target[targetIndex]);
          ++targetIndex;
        } else {
          var index = currentSourceBuffer.Dequeue();
          yield return new ChangeSet<TItem>(ChangeType.Added, index.Item1, index.Item2, -1, default(TItem));
        }
      }

      while (targetIndex < targetLen) {
        yield return new ChangeSet<TItem>(ChangeType.Removed, -1, default(TItem), targetIndex, target[targetIndex]);
        ++targetIndex;
      }
    }
  }

  private static int _IndexOf<TItem>(TItem[] values, TItem item, int startIndex, IEqualityComparer<TItem> comparer) {
    for (var i = startIndex; i < values.Length; ++i)
      if (ReferenceEquals(values[i], item) || comparer.Equals(values[i], item))
        return i;

    return -1;
  }

  /// <summary>
  /// Returns the enumeration or <c>null</c> if it is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <returns><c>null</c> if the enumeration is empty; otherwise, the enumeration itself </returns>
  [DebuggerStepThrough]
  public static IEnumerable<TItem> ToNullIfEmpty<TItem>(this IEnumerable<TItem> @this) {
    switch (@this) {
      case null:
        return null;
      case TItem[] array:
        return array.Length < 1 ? null : @this;
      case ICollection<TItem> collection:
        return collection.Count < 1 ? null : @this;
      default:
        // ReSharper disable PossibleMultipleEnumeration
        using (var enumerator = @this.GetEnumerator())
          return enumerator.MoveNext() ? @this : null;
      // ReSharper restore PossibleMultipleEnumeration
    }
  }

  /// <summary>
  /// Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="initialCapacity">The initial capacity.</param>
  /// <returns>
  /// A hashset
  /// </returns>
  [DebuggerStepThrough]
  public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this, int initialCapacity) {
    Against.ArgumentIsNull(@this);
    Against.NegativeValues(initialCapacity);
      
    List<TItem> items = new(initialCapacity);
    items.AddRange(@this);
    return new(items);
  }

  /// <summary>
  /// Creates a hash set from the given enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="comparer">The comparer, if any; otherwise, uses the default comparer for the given item type.</param>
  /// <returns>
  /// A hashset
  /// </returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static HashSet<TResult> ToHashSet<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, IEqualityComparer<TResult> comparer = null) {
    Against.ArgumentIsNull(@this);
    Against.ArgumentIsNull(selector);

    return comparer == null ? new(@this.Select(selector)) : new HashSet<TResult>(@this.Select(selector), comparer);
  }

  /// <summary>
  /// Tests whether two enumerations of the same type are equal.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This IEnumerable.</param>
  /// <param name="other">The other IEnumerable.</param>
  /// <param name="comparer">The comparer, if any; otherwise, uses the default comparer for the given item type.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static bool AreEqual<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer = null) {
    if (ReferenceEquals(@this, other))
      return true;

    if (@this == null || other == null)
      return false;

    if (@this is ICollection<TItem> thisCollection && other is ICollection<TItem> otherCollection && thisCollection.Count != otherCollection.Count)
      return false;

    comparer ??= EqualityComparer<TItem>.Default;

    using var thisEnumerator = @this.GetEnumerator();
    using var otherEnumerator = other.GetEnumerator();
    bool hasMoreItems;

    // until at least one enumeration has ended
    while ((hasMoreItems = thisEnumerator.MoveNext()) == otherEnumerator.MoveNext()) {

      // both ended, so they must be equal
      if (!hasMoreItems)
        return true;

      // if current element differs, break
      if (!comparer.Equals(thisEnumerator.Current, otherEnumerator.Current))
        return false;

    }

    // one of the enumerations ended first, so they are not equal
    return false;
  }

  /// <summary>
  /// Shuffles the specified enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This IEnumerable.</param>
  /// <param name="random">The random number generator.</param>
  /// <returns>A shuffled enumeration.</returns>
  [DebuggerStepThrough]
  public static IEnumerable<TItem> Shuffle<TItem>(this IEnumerable<TItem> @this, Random random = null) {
    Against.ThisIsNull(@this);

#if SUPPORTS_RANDOM_SHARED
    random ??= Random.Shared;
#else
    random ??= new();
#endif

    return
      @this
        .Select(i => new { r = random.Next(), i })
        .OrderBy(a => a.r)
        .Select(a => a.i)
      ;
  }

  [DebuggerStepThrough]
  public static bool IsNotNullOrEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(true)] 
#endif
    this IEnumerable<TItem> @this) => !IsNullOrEmpty(@this);

  /// <summary>
  /// Determines whether the enumeration is <c>null</c> or empty.
  /// </summary>
  /// <param name="this">This Enumeration.</param>
  /// <returns>
  ///   <c>true</c> if the enumeration is <c>null</c> or empty; otherwise, <c>false</c>.
  /// </returns>
  [DebuggerStepThrough]
  public static bool IsNullOrEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(false)] 
#endif
    this IEnumerable<TItem> @this) {
    switch (@this) {
      case null:
        return true;
      case TItem[] array:
        return array.Length == 0;
      case ICollection<TItem> collection:
        return collection.Count == 0;
      default:
        using (var enumerator = @this.GetEnumerator())
          return !enumerator.MoveNext();
    }
  }

  [DebuggerStepThrough]
  public static bool IsNotNullOrEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(true)] 
#endif
    this IEnumerable<TItem> @this, Func<TItem, bool> predicate) => !IsNullOrEmpty(@this, predicate);

  /// <summary>
  /// Determines whether the enumeration is <c>null</c> or empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>
  ///   <c>true</c> if the enumeration is <c>null</c> or empty; otherwise, <c>false</c>.
  /// </returns>
  [DebuggerStepThrough]
  public static bool IsNullOrEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(false)] 
#endif
    this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    switch (@this) {
      case null:
        return true;
      case TItem[] array:
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in array)
          if (predicate(item))
            return true;

        return false;
      case IList<TItem> list: {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < list.Count; ++i)
          if (predicate(list[i]))
            return true;

        return false;
      }
      default:
        using (var enumerator = @this.GetEnumerator())
          while (enumerator.MoveNext())
            if (predicate(enumerator.Current))
              return true;

        return false;
    }
  }

  /// <summary>
  /// Concat all byte arrays into one.
  /// </summary>
  /// <param name="this">This Enumerable of byte[].</param>
  /// <returns>A block of bytes with all parts joined.</returns>
  [DebuggerStepThrough]
  public static byte[] ConcatAll(this IEnumerable<byte[]> @this) {
    if (@this == null)
      return null;

    List<byte[]> chunks;
    var totalSize = 0;
    if (@this is IList<byte[]> list) {
      chunks = new(list.Count);
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var i = 0; i < list.Count; i++) {
        var chunk = list[i];
        if (chunk == null || chunk.Length < 1)
          continue;

        totalSize += chunk.Length;
        chunks.Add(chunk);
      }
    } else {
      chunks = @this is not ICollection<byte[]> collection ? new() : new List<byte[]>(collection.Count);
      foreach (var chunk in @this) {
        if (chunk == null || chunk.Length < 1)
          continue;

        totalSize += chunk.Length;
        chunks.Add(chunk);
      }
    }

    var result = new byte[totalSize];
    var index = 0;
    foreach (var chunk in chunks) {
      Buffer.BlockCopy(chunk, 0, result, index, chunk.Length);
      index += chunk.Length;
    }
    return result;
  }

  /// <summary>
  /// Concat all enumerations into one.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">The this.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<TItem> ConcatAll<TItem>(this IEnumerable<IEnumerable<TItem>> @this) {
    Against.ThisIsNull(@this);
      
    return @this.SelectMany(c => c as TItem[] ?? c.ToArray());
  }

#if SUPPORTS_TUPLES
  public static Tuple<IEnumerable<TItem>, IEnumerable<TItem>> Split<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);
      
    var groups = @this.GroupBy(predicate).ToArray();
    var result = Tuple.Create(
      groups.Where(i => i.Key).SelectMany(i => i),
      groups.Where(i => !i.Key).SelectMany(i => i)
    );
    return result;
  }
#endif

  /// <summary>
  /// Determines whether the specified enumeration contains not the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <param name="item">The item.</param>
  /// <param name="comparer">The comparer; if any.</param>
  /// <returns><c>true</c> if the enumeration does not contain the listed value; otherwise, <c>false</c>.</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool ContainsNot<TItem>(this IEnumerable<TItem> @this, TItem item, IEqualityComparer<TItem> comparer = null) {
    Against.ThisIsNull(@this);

    return !(comparer == null ? @this.Contains(item) : @this.Contains(item, comparer));
  }

  /// <summary>
  /// Determines whether the specified enumeration contains not the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <param name="items">The items.</param>
  /// <param name="comparer">The comparer; if any.</param>
  /// <returns><c>true</c> if the enumeration does not contain the listed values; otherwise, <c>false</c>.</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool ContainsNotAny<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items, IEqualityComparer<TItem> comparer = null) {
    Against.ThisIsNull(@this);

    return !ContainsAny(@this, items, comparer);
  }

  /// <summary>
  /// Determines whether the specified enumeration contains any of the items given by the second enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="list">The list of items we should look for.</param>
  /// <param name="equalityComparer">The equality comparer; if any.</param>
  /// <returns>
  ///   <c>true</c> if the enumeration contains any of the listed values; otherwise, <c>false</c>.
  /// </returns>
  public static bool ContainsAny<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> list, IEqualityComparer<TItem> equalityComparer = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(list);

    // we'll cache all visited values from the list, so we don't have to enumerate more than once
    List<TItem> itemCache = new();

    equalityComparer ??= EqualityComparer<TItem>.Default;

    using var enumerator = list.GetEnumerator();

    // let's look at all our items
    foreach (var item in @this) {

      // search the cache first
      if (itemCache.Contains(item))
        return true;

      // try continuing the list
      while (enumerator.MoveNext()) {

        // get next item
        var current = enumerator.Current;

        // add to cache
        itemCache.Add(current);

        // check for equality
        object box1 = item;
        object box2 = current;
        if (ReferenceEquals(box1, box2))
          return true;

        if (box1 == null) {

        } else if (box2 != null) {
          if (equalityComparer.Equals(item, current))
            return true;
        }

      } // next list item

    } // next item

    // we found nothing that matched
    return false;
  }

  /// <summary>
  /// Executes the given action on each element of the enumeration.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumerable.</param>
  /// <param name="action">The action.</param>
  [DebuggerStepThrough]
  public static void ForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    foreach (var item in @this)
      action(item);
  }

  /// <summary>
  /// Executes a callback for each item.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="action">The call to execute.</param>
  [DebuggerStepThrough]
  public static void ForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem, int> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    var index = 0;
    foreach (var item in @this)
      action(item, index++);
  }

#if SUPPORTS_ASYNC
  /// <summary>
  /// Executes a callback for each item in parallel.
  /// </summary>
  /// <typeparam name="TIn">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="action">The call to execute.</param>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void ParallelForEach<TIn>(this IEnumerable<TIn> @this, Action<TIn> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);
      
    Parallel.ForEach(@this, action);
  }

  /// <summary>
  /// Executes a callback for each item in parallel.
  /// </summary>
  /// <typeparam name="TIn">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="action">The call to execute.</param>
  public static void ParallelForEach<TIn>(this IEnumerable<TIn> @this, Action<TIn, int> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

#pragma warning disable CC0031 // Check for null before calling a delegate
    @this.Select((v, i) => new Action(() => action(v, i))).ParallelForEach(i => i());
#pragma warning restore CC0031 // Check for null before calling a delegate
  }

#endif

  /// <summary>
  /// Converts all.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <typeparam name="TResult">The type of the output.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<TResult> ConvertAll<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    return @this.Select(converter);
  }

  /// <summary>
  /// Converts all.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <typeparam name="TResult">The type of the output.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<TResult> ConvertAll<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, int, TResult> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);
      
    return @this.Select(converter);
  }

  /// <summary>
  /// Reports the progress while walking through the enumerable.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="progressCallback">The progress callback.</param>
  /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
  /// <returns>
  /// A new enumeration which automatically calls the progress callback when items are pulled.
  /// </returns>
  public static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, Action<double> progressCallback, bool delayed = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(progressCallback);

    var collection = @this as ICollection<TItem> ?? @this.ToList();
    return AsProgressReporting(collection, collection.Count, progressCallback, delayed);
  }

  /// <summary>
  /// Reports the progress while walking through the enumerable.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="progressCallback">The progress callback.</param>
  /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
  /// <returns>
  /// A new enumeration which automatically calls the progress callback when items are pulled.
  /// </returns>
  public static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, Action<long, long> progressCallback, bool delayed = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(progressCallback);

    var collection = @this as ICollection<TItem> ?? @this.ToList();
    return AsProgressReporting(collection, collection.Count, progressCallback, delayed);
  }

  /// <summary>
  /// Reports the progress while walking through the enumerable.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="length">The length of the enumeration.</param>
  /// <param name="progressCallback">The progress callback.</param>
  /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
  /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
  public static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, int length, Action<double> progressCallback, bool delayed = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(progressCallback);

    return AsProgressReporting(@this, length, (i, c) => progressCallback(i == c ? 1 : (double)i / c), delayed);
  }

  /// <summary>
  /// Reports the progress while walking through the enumerable.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="length">The length of the enumeration.</param>
  /// <param name="progressCallback">The progress callback.</param>
  /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
  /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
  public static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, int length, Action<long, long> progressCallback, bool delayed = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(progressCallback);

    return Invoke(@this, length, progressCallback, delayed);
    
    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, int length, Action<long, long> progressCallback, bool delayed) {
      if (length == 0)
        progressCallback(0, 0);
      else {
        progressCallback(0, length);

        void ProgressAction(int i) => progressCallback(i, length);
        void NullAction(int _) { }

        var preAction = delayed ? (Action<int>)ProgressAction : NullAction;
        var postAction = delayed ? NullAction : (Action<int>)ProgressAction;

        var currentIndex = 0;
        foreach (var item in @this) {
          preAction(currentIndex);
          yield return item;
          postAction(currentIndex);
          ++currentIndex;
        }

        progressCallback(length, length);
      }
    }
  }

  /// <summary>
  /// Tests whether the given condition applies to all elements or not.
  /// </summary>
  /// <typeparam name="TItem">The type of the source.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <param name="condition">The condition.</param>
  /// <returns></returns>
  public static bool All<TItem>(this IEnumerable<TItem> @this, Func<TItem, int, bool> condition) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(condition);

    // original but slower implementation
    //return (!This.Where((o, i) => !condition(o, i)).Any());

    // faster by avoiding inner lambda
    var index = -1;
    foreach (var item in @this) {
      checked { ++index; }
      if (!condition(item, index))
        return false;
    }
    return true;
  }

  /// <summary>
  /// Distinct the specified enumeration by the specified selector.
  /// </summary>
  /// <typeparam name="TItem">The type of the input elements.</typeparam>
  /// <typeparam name="TCompare">The type of the comparison elements.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <returns>An enumeration with distinct elements.</returns>
  public static IEnumerable<TItem> Distinct<TItem, TCompare>(this IEnumerable<TItem> @this, Func<TItem, TCompare> selector) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return @this
      .Select(i => new { comparer = selector(i), data = i })
      .GroupBy(i => i.comparer)
      .Select(g => g.First().data)
      ;
  }

  /// <summary>
  /// Flattens a sequence of sequences (<see cref="IEnumerable{TItem}"/> of <see cref="IEnumerable{TItem}"/>) into a single sequence by concatenating each sub-sequence.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements in the sequences.</typeparam>
  /// <param name="this">The sequence of sequences to flatten.</param>
  /// <returns>A single <see cref="IEnumerable{TItem}"/> sequence containing all the elements from each sub-sequence in the original sequence.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the source sequence is <see langword="null"/>.</exception>
  /// <remarks>
  /// This method uses deferred execution, meaning that the input sequence is not iterated until the returned sequence is itself iterated.
  /// </remarks>
  /// <example>
  /// Here's how to use the <c>SelectMany</c> extension method:
  /// <code>
  /// var listOfLists = new List&lt;List&lt;int&gt;&gt;
  /// {
  ///     new List&lt;int&gt; { 1, 2, 3 },
  ///     new List&lt;int&gt; { 4, 5, 6 },
  ///     new List&lt;int&gt; { 7, 8, 9 }
  /// };
  /// 
  /// foreach(var item in listOfLists.SelectMany())
  /// {
  ///     Console.WriteLine(item);
  /// }
  /// </code>
  /// This code will output:
  /// <code>
  /// 1
  /// 2
  /// 3
  /// 4
  /// 5
  /// 6
  /// 7
  /// 8
  /// 9
  /// </code>
  /// </example>
  [DebuggerStepThrough]
  public static IEnumerable<TItem> SelectMany<TItem>(this IEnumerable<IEnumerable<TItem>> @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);
    
    static IEnumerable<TItem> Invoke(IEnumerable<IEnumerable<TItem>> @this) {
      foreach (var enumeration in @this)
      foreach (var item in enumeration)
        yield return item;
    }
  }

  /// <summary>
  /// Joins the specified elements into a string.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="join">The delimiter.</param>
  /// <param name="skipDefaults">if set to <c>true</c> all default values will be skipped.</param>
  /// <param name="converter">The converter.</param>
  /// <returns>The joined string.</returns>
  public static string Join<TItem>(this IEnumerable<TItem> @this, string join = ", ", bool skipDefaults = false, Func<TItem, string> converter = null) {
    Against.ThisIsNull(@this);

    StringBuilder result = new();
    var gotElements = false;
    var defaultValue = default(TItem);

    // if no converter was given, use the default toString if not null
    converter ??= i => i?.ToString();
      
    foreach (var item in skipDefaults ? @this.Where(item => !EqualityComparer<TItem>.Default.Equals(item, defaultValue)) : @this) {
      if (gotElements)
        result.Append(join);
      else
        gotElements = true;

#pragma warning disable CC0031 // Check for null before calling a delegate
      result.Append(converter(item));
#pragma warning restore CC0031 // Check for null before calling a delegate
    }
    return gotElements ? result.ToString() : string.Empty;
  }

  /// <summary>
  /// Gets the index of the first item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The index of the matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, int defaultValue = -1) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    var result = 0;
    foreach (var item in @this)
      if (selector(item))
        return result;
      else
        ++result;

    return defaultValue;
  }

  /// <summary>
  /// Gets the index of the first item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The index of the matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<int> defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);
    Against.ArgumentIsNull(defaultValue);
      
    var result = 0;
    foreach (var item in @this)
      if (selector(item))
        return result;
      else
        ++result;

    return defaultValue();
  }

  /// <summary>
  /// Gets the index of the first item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The index of the matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, int> defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);
    Against.ArgumentIsNull(defaultValue);
      
    var result = 0;
    // ReSharper disable once PossibleMultipleEnumeration
    foreach (var item in @this)
      if (selector(item))
        return result;
      else
        ++result;

    // ReSharper disable once PossibleMultipleEnumeration
    return defaultValue(@this);
  }

  /// <summary>
  /// Gets the index of the given value.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="item">The item.</param>
  /// <returns>The position of the item in the enumeration or -1</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int IndexOf<TItem>(this IEnumerable<TItem> @this, TItem item) => IndexOrDefault(@this, a => Equals(a, item), -1);

  /// <summary>
  /// Tries to get the first item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetFirst<TItem>(this IEnumerable<TItem> @this, out TItem result) {
    Against.ThisIsNull(@this);

    foreach (var item in @this) {
      result = item;
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  /// Tries to get the last item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetLast<TItem>(this IEnumerable<TItem> @this, out TItem result) {
    Against.ThisIsNull(@this);

    result = default;
    var foundItems = false;
    foreach (var item in @this) {
      result = item;
      foundItems = true;
    }

    return foundItems;
  }

  /// <summary>
  /// Tries to get the item at the given index.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="index">The items' position</param>
  /// <param name="result">The value or the <see langword="default"/> for the given datatype.</param>
  /// <returns><see langword="true"/> when the item could be retrieved; otherwise, <see langword="false"/>.</returns>
  public static bool TryGetItem<TItem>(this IEnumerable<TItem> @this, int index, out TItem result) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    foreach (var item in @this) {
      if (index > 0) {
        --index;
        continue;
      }

      result = item;
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  /// Tries to apply the given selector in case the <see cref="IEnumerable{T}"/> is not empty without enumerating multiple times.
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <typeparam name="TResult">The selectors' result type</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="selector">The selector to apply to the enumeration</param>
  /// <param name="result">The result of the selector when applied to the <see cref="IEnumerable{T}"/></param>
  /// <returns><see langword="true"/> when the enumeration contains at least one item; otherwise, <see langword="false"/>.</returns>
  public static bool TryGet<TItem, TResult>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TResult> selector, out TResult result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    var enumerator=@this.GetEnumerator();
    if (!enumerator.MoveNext()) {
      result = default;
      return false;
    }

    result = selector(Enumerate(enumerator));
    return true;

    static IEnumerable<TItem> Enumerate(IEnumerator<TItem> enumerator) {
      yield return enumerator.Current;
      while(enumerator.MoveNext())
        yield return enumerator.Current;

      enumerator.Dispose();
    }
  }

  /// <summary>
  /// Tries to calculate the maximum of the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="result">The maximum or the <see langword="default"/></param>
  /// <returns><see langword="true"/> when the enumeration contains at least one item; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryGetMax<TItem>(this IEnumerable<TItem> @this, out TItem result)
    => TryGet(@this, t => t.Max(), out result)
  ;

  /// <summary>
  /// Tries to calculate the maximum of the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <typeparam name="TResult">The type of result</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="selector">The selector on how to get the sort-value.</param>
  /// <param name="result">The maximum or the <see langword="default"/></param>
  /// <returns><see langword="true"/> when the enumeration contains at least one item; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryGetMaxBy<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, out TItem result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return TryGet(@this, t => t.MaxBy(selector), out result);
  }

  /// <summary>
  /// Tries to calculate the minimum of the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="result">The minimum or the <see langword="default"/></param>
  /// <returns><see langword="true"/> when the enumeration contains at least one item; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryGetMin<TItem>(this IEnumerable<TItem> @this, out TItem result)
    => TryGet(@this, t => t.Min(), out result)
  ;

  /// <summary>
  /// Tries to calculate the minimum of the given items.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <typeparam name="TResult">The type of result</typeparam>
  /// <param name="this">This <see cref="IEnumerable{T}"/></param>
  /// <param name="selector">The selector on how to get the sort-value.</param>
  /// <param name="result">The minimum or the <see langword="default"/></param>
  /// <returns><see langword="true"/> when the enumeration contains at least one item; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryGetMinBy<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, out TItem result) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    return TryGet(@this, t => t.MinBy(selector), out result);
  }
  
  /// <summary>
  /// Retrieves the first element from the specified <see cref="IEnumerable{TItem}"/> collection of reference types, or returns <see langword="null"/> if the collection is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a reference type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TItem}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to enforce that the method's type argument is a reference type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The first element from the collection if it is not empty; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TItem}"/>.
  /// It is particularly useful in scenarios where it is acceptable for the collection to be empty and the caller needs to safely handle such a case <c>without throwing an exception</c>.
  /// The <typeparamref name="TItem"/> constraint ensures that this method can only be used with reference types, aligning with the possibility of returning <see langword="null"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the <c>FirstOrNull</c> method:
  /// <code>
  /// var strings = new List&lt;string&gt; { "Hello", "World" };
  /// var firstString = strings.FirstOrNull();
  /// Console.WriteLine(firstString);
  /// 
  /// var emptyStrings = new List&lt;string&gt;();
  /// var firstEmpty = emptyStrings.FirstOrNull();
  /// Console.WriteLine(firstEmpty ?? "No element");
  /// </code>
  /// This example will output:
  /// <code>
  /// Hello
  /// No element
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem FirstOrNull<TItem>(this IEnumerable<TItem> @this, __ClassForcingTag<TItem> _ = null) where TItem : class
    => @this.TryGetFirst(out var result) ? result : null
  ;

  /// <summary>
  /// Retrieves the first element from the specified <see cref="IEnumerable{TItem}"/> collection of value types, or returns <see langword="null"/> if the collection is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a value type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TItem}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to enforce that the method's type argument is a value type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The first element from the collection if it is not empty, wrapped in a nullable type; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TItem}"/>.
  /// It is particularly useful for collections of value types where a distinction between 'no items' and 'default value item' is necessary. By returning a nullable value type, this method provides a clear indication of an empty collection.
  /// The <typeparamref name="TItem"/> constraint ensures that this method can only be used with structs, aligning with the method's return type of a nullable <typeparamref name="TItem"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the <c>FirstOrNull</c> method with a collection of value types:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 1, 2, 3 };
  /// var firstNumber = numbers.FirstOrNull();
  /// Console.WriteLine(firstNumber.HasValue ? firstNumber.ToString() : "No element");
  /// 
  /// var emptyNumbers = new List&lt;int&gt;();
  /// var firstEmpty = emptyNumbers.FirstOrNull();
  /// Console.WriteLine(firstEmpty.HasValue ? firstEmpty.ToString() : "No element");
  /// </code>
  /// This example will output:
  /// <code>
  /// 1
  /// No element
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem? FirstOrNull<TItem>(this IEnumerable<TItem> @this, __StructForcingTag<TItem> _ = null) where TItem : struct
    => @this.TryGetFirst(out var result) ? result : null
  ;

  /// <summary>
  /// Gets the first item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, TItem defaultValue) {
    Against.ThisIsNull(@this);

    foreach (var item in @this)
      return item;

    return defaultValue;
  }

  /// <summary>
  /// Gets the first item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(defaultValueFactory);

    foreach (var item in @this)
      return item;

    return defaultValueFactory();
  }

  /// <summary>
  /// Gets the first item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(defaultValueFactory);

    // ReSharper disable once PossibleMultipleEnumeration
    foreach (var item in @this)
      return item;

    // ReSharper disable once PossibleMultipleEnumeration
    return defaultValueFactory(@this);
  }

  /// <summary>
  /// Gets the first item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, TItem defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);

    foreach (var item in @this)
      if (selector(item))
        return item;

    return defaultValue;
  }

  /// <summary>
  /// Gets the first item matching the condition or the result of the default value factory.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValueFactory">The default value.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);
    Against.ArgumentIsNull(defaultValueFactory);
      
    foreach (var item in @this)
      if (selector(item))
        return item;

    return defaultValueFactory();
  }

  /// <summary>
  /// Gets the first item matching the condition or the result of the default value factory.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValueFactory">The default value.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(selector);
    Against.ArgumentIsNull(defaultValueFactory);
      
    // ReSharper disable once PossibleMultipleEnumeration
    foreach (var item in @this)
      if (selector(item))
        return item;

    // ReSharper disable once PossibleMultipleEnumeration
    return defaultValueFactory(@this);
  }

  /// <summary>
  /// Gets the last item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, TItem defaultValue)
    => FirstOrDefault(@this.Reverse(), selector, defaultValue)
  ;

  /// <summary>
  /// Gets the last item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<TItem> defaultValueFactory)
    => FirstOrDefault(@this.Reverse(), selector, defaultValueFactory)
  ;

  /// <summary>
  /// Gets the last item matching the condition or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="selector">The selector.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns>The matched item or the given default value.</returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, TItem> defaultValueFactory)
    => FirstOrDefault(@this.Reverse(), selector, defaultValueFactory)
  ;
  
  /// <summary>
  /// Retrieves the last element from the specified <see cref="IEnumerable{TItem}"/> collection of reference types, or returns <see langword="null"/> if the collection is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a reference type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TItem}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to enforce that the method's type argument is a reference type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The last element from the collection if it is not empty; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TItem}"/>.
  /// It is particularly useful in scenarios where it is acceptable for the collection to be empty and the caller needs to safely handle such a case <c>without throwing an exception</c>.
  /// The <typeparamref name="TItem"/> constraint ensures that this method can only be used with reference types, aligning with the possibility of returning <see langword="null"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the <c>LastOrNull</c> method:
  /// <code>
  /// var strings = new List&lt;string&gt; { "Hello", "World" };
  /// var lastString = strings.LastOrNull();
  /// Console.WriteLine(lastString);
  /// 
  /// var emptyStrings = new List&lt;string&gt;();
  /// var lastEmpty = emptyStrings.LastOrNull();
  /// Console.WriteLine(lastEmpty ?? "No element");
  /// </code>
  /// This example will output:
  /// <code>
  /// World
  /// No element
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrNull<TItem>(this IEnumerable<TItem> @this, __ClassForcingTag<TItem> _ = null) where TItem : class
    => @this.TryGetLast(out var result) ? result : null
  ;

  /// <summary>
  /// Retrieves the last element from the specified <see cref="IEnumerable{TItem}"/> collection of value types, or returns <see langword="null"/> if the collection is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a value type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TItem}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to enforce that the method's type argument is a value type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The last element from the collection if it is not empty, wrapped in a nullable type; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TItem}"/>.
  /// It is particularly useful for collections of value types where a distinction between 'no items' and 'default value item' is necessary. By returning a nullable value type, this method provides a clear indication of an empty collection.
  /// The <typeparamref name="TItem"/> constraint ensures that this method can only be used with structs, aligning with the method's return type of a nullable <typeparamref name="TItem"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the <c>LastOrNull</c> method with a collection of value types:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 1, 2, 3 };
  /// var lastNumber = numbers.LastOrNull();
  /// Console.WriteLine(lastNumber.HasValue ? lastNumber.ToString() : "No element");
  /// 
  /// var emptyNumbers = new List&lt;int&gt;();
  /// var lastEmpty = emptyNumbers.LastOrNull();
  /// Console.WriteLine(lastEmpty.HasValue ? lastEmpty.ToString() : "No element");
  /// </code>
  /// This example will output:
  /// <code>
  /// 3
  /// No element
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem? LastOrNull<TItem>(this IEnumerable<TItem> @this, __StructForcingTag<TItem> _ = null) where TItem : struct
    => @this.TryGetLast(out var result) ? result : null
  ;

  /// <summary>
  /// Gets the last item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, TItem defaultValue)
    => FirstOrDefault(@this.Reverse(), defaultValue)
  ;

  /// <summary>
  /// Gets the last item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory)
    => FirstOrDefault(@this.Reverse(), defaultValueFactory)
  ;

  /// <summary>
  /// Gets the last item or the default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TItem> defaultValueFactory)
    => FirstOrDefault(@this.Reverse(), defaultValueFactory)
  ;

  /// <summary>
  /// Orders the collection by its elements itself.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<TItem> OrderBy<TItem>(this IEnumerable<TItem> @this) {
    Against.ThisIsNull(@this);
      
    return @this.OrderBy(i => i);
  }

  /// <summary>
  /// Orders the collection by its elements itself.
  /// </summary>
  /// <typeparam name="TItem">The type of the in.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<TItem> OrderByDescending<TItem>(this IEnumerable<TItem> @this) {
    Against.ThisIsNull(@this);

    return @this.OrderByDescending(i => i);
  }

  /// <summary>
  /// Returns the items in a randomized order.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <param name="random">The random instance, if any.</param>
  /// <returns>The items in a randomized order.</returns>
  public static IEnumerable<TItem> Randomize<TItem>(this IEnumerable<TItem> @this, Random random = null) {
    Against.ThisIsNull(@this);

#if SUPPORTS_RANDOM_SHARED
    random ??= Random.Shared;
#else
    random ??= new();
#endif

    return Invoke(@this, random);
    
    static IEnumerable<TItem> Invoke(IEnumerable<TItem> @this, Random random) {
      var list = @this.Select(o => o).ToList();
      int count;
      while ((count = list.Count) > 0) {
        var index = count == 1 ? 0 : random.Next(0, count);
        yield return list[index];
        list.RemoveAt(index);
      }
    }
  }

  /// <summary>
  /// Takes items until a given condition is met.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static IEnumerable<TItem> TakeUntil<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return @this.TakeWhile(i => !predicate(i));
  }

  /// <summary>
  /// Skips items until a given condition is met.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This enumeration.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static IEnumerable<TItem> SkipUntil<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(predicate);

    return @this.SkipWhile(i => !predicate(i));
  }

  /// <summary>
  /// wraps this collection of Disposables in an <see cref="IDisposableCollection{T}"/> which can be used within an using statement
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the IEnumerable (has to be IDisposable)</typeparam>
  /// <param name="this">This IEnumerable</param>
  /// <returns>An <see cref="IDisposableCollection{T}"/> containing the elements of this IEnumerable</returns>
  public static IDisposableCollection<TItem> WrapAsDisposableCollection<TItem>(this IEnumerable<TItem> @this) where TItem : IDisposable =>
    new DisposableCollection<TItem>(@this)
  ;

  public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TItem, TKey, TValue>(this TItem[] @this,Func<TItem,TKey> keyGetter,Func<TItem,TValue> valueGetter,IEqualityComparer<TKey> equalityComparer=null ) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(keyGetter);
    Against.ArgumentIsNull(valueGetter);

    var result = equalityComparer == null ? new() : new ConcurrentDictionary<TKey, TValue>(equalityComparer);
    foreach (var item in @this)
      result.TryAdd(keyGetter(item), valueGetter(item));

    return result;
  }

#if SUPPORTS_TASK_RUN
  /// <summary>
  /// Iterates through the given enumeration in a separate thread and executes an action for every item
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the IEnumerable</typeparam>
  /// <param name="this">This IEnumerable</param>
  /// <param name="action">the action performed for each element in the IEnumerable</param>
  /// <returns>A awaitable task representing this action</returns>
  public static async Task DoForEveryItemAsync<TItem>(this IEnumerable<TItem> @this, Action<TItem> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);
  
    await Task.Run(() => {
      foreach (var item in @this)
        action.Invoke(item);
    });
  }
#endif

  /// <summary>
  /// Tries to get a single element from the specified <see cref="IEnumerable{T}"/> collection. 
  /// This method is designed to return <see langword="true"/> if the collection contains exactly one element. 
  /// If the collection contains no elements or more than one element, it returns <see langword="false"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="result">The single item from the collection if it contains exactly one element; otherwise, the default value for the type <typeparamref name="TItem"/>.</param>
  /// <returns>
  /// <see langword="true"/> if the collection contains exactly one element; otherwise, <see langword="false"/>. When this method returns <see langword="false"/>, the <paramref name="result"/> parameter is set to its default value.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// It is designed to be a safe way to attempt to retrieve a single item from a collection <c>without throwing exceptions</c>
  /// if the collection does not contain exactly one element. This method internally checks the provided collection for the number of elements
  /// and sets the <paramref name="result"/> parameter accordingly.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 42 };
  /// if(numbers.TryGetSingle(out int singleValue)) {
  ///     Console.WriteLine($"Single value: {singleValue}");
  /// } else {
  ///     Console.WriteLine("The collection does not contain exactly one element.");
  /// }
  /// </code>
  /// This example will output:
  /// <code>
  /// Single value: 42
  /// </code>
  /// </example>
  [DebuggerStepThrough]
  public static bool TryGetSingle<TItem>(this IEnumerable<TItem> @this, out TItem result) {
    Against.ThisIsNull(@this);

    switch (@this) {
      case TItem[] { Length: 1 } array: {
        result = array[0];
        return true;
      }
      case TItem[]: {
        result = default;
        return false;
      }
      case IList<TItem> { Count: 1 } list: {
        result = list[0];
        return true;
      }
      case IList<TItem>: {
        result = default;
        return false;
      }
      default: {
        using var enumerator = @this.GetEnumerator();
        if (enumerator.MoveNext()) {
          result = enumerator.Current;
          if (!enumerator.MoveNext())
            return true;
        }

        result = default;
        return false;
      }
    }
  }

  /// <summary>
  /// Retrieves a single element from the specified <see cref="IEnumerable{T}"/> collection, or returns <see langword="null"/> if the collection does not contain exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a reference type, not a value type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to force the method's type argument to be a reference type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The single element from the collection if it contains exactly one element; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// The generic type parameter <typeparamref name="TItem"/> is constrained to reference types to ensure that the return value can be <see langword="null"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method:
  /// <code>
  /// var numbers = new List&lt;string&gt; { "example" };
  /// var result = numbers.SingleOrNull();
  /// Console.WriteLine(result ?? "No single item");
  /// 
  /// var emptyList = new List&lt;string&gt;();
  /// var emptyResult = emptyList.SingleOrNull();
  /// Console.WriteLine(emptyResult ?? "No single item");
  /// </code>
  /// This example will output:
  /// <code>
  /// example
  /// No single item
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem SingleOrNull<TItem>(this IEnumerable<TItem> @this,__ClassForcingTag<TItem> _=null) where TItem : class 
    => TryGetSingle(@this, out var result) ? result : null
  ;

  /// <summary>
  /// Retrieves a single element from the specified <see cref="IEnumerable{T}"/> collection of value types, or returns <see langword="null"/> if the collection does not contain exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection. This type must be a value type.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="_">An optional parameter used to force the method's type argument to be a value type. This parameter is not used in the method body and must be omitted when calling the method.</param>
  /// <returns>
  /// The single element from the collection if it contains exactly one element, wrapped in a nullable type; otherwise, <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// The generic type parameter <typeparamref name="TItem"/> is constrained to structs to ensure the method correctly handles value types, with the return type being a nullable <typeparamref name="TItem"/> to allow for null returns.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method with a collection of value types:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 42 };
  /// var result = numbers.SingleOrNull();
  /// Console.WriteLine(result.HasValue ? result.ToString() : "No single item");
  /// 
  /// var emptyList = new List&lt;int&gt;();
  /// var emptyResult = emptyList.SingleOrNull();
  /// Console.WriteLine(emptyResult.HasValue ? emptyResult.ToString() : "No single item");
  /// </code>
  /// This example will output:
  /// <code>
  /// 42
  /// No single item
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem? SingleOrNull<TItem>(this IEnumerable<TItem> @this,__StructForcingTag<TItem> _=null) where TItem : struct 
    => TryGetSingle(@this, out var result) ? result : null
  ;

  /// <summary>
  /// Retrieves a single element from the specified <see cref="IEnumerable{T}"/> collection, or returns the specified default value if the collection does not contain exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="defaultValue">The default value to return if the collection does not contain exactly one element.</param>
  /// <returns>
  /// The single element from the collection if it contains exactly one element; otherwise, the specified default value.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 42 };
  /// var singleValue = numbers.SingleOrDefault(-1);
  /// Console.WriteLine(singleValue);
  /// 
  /// var emptyList = new List&lt;int&gt;();
  /// var defaultValue = emptyList.SingleOrDefault(-1);
  /// Console.WriteLine(defaultValue);
  /// </code>
  /// This example will output:
  /// <code>
  /// 42
  /// -1
  /// </code>
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem SingleOrDefault<TItem>(this IEnumerable<TItem> @this, TItem defaultValue)
    => TryGetSingle(@this, out var result) ? result : defaultValue
  ;

  /// <summary>
  /// Retrieves a single element from the specified <see cref="IEnumerable{T}"/> collection, or invokes a function to return a default value if the collection does not contain exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="defaultValueFactory">A function that produces the default value to return if the collection does not contain exactly one element.</param>
  /// <returns>
  /// The single element from the collection if it contains exactly one element; otherwise, the value produced by the <paramref name="defaultValueFactory"/> function.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// Using a function to provide a default value allows for lazy evaluation of the default value, which can be beneficial if the default value is expensive to compute or should only be created if necessary.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method with a default value factory function:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 42 };
  /// var singleValue = numbers.SingleOrDefault(() =&gt; ComputeExpensiveDefaultValue());
  /// Console.WriteLine(singleValue);
  /// 
  /// var emptyList = new List&lt;int&gt;();
  /// var defaultValue = emptyList.SingleOrDefault(() =&gt; ComputeExpensiveDefaultValue());
  /// Console.WriteLine(defaultValue);
  /// </code>
  /// This example will output:
  /// <code>
  /// 42
  /// // The result of ComputeExpensiveDefaultValue()
  /// </code>
  /// Assume <c>ComputeExpensiveDefaultValue</c> is a method defined elsewhere that computes and returns an expensive default value.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem SingleOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(defaultValueFactory);

    return TryGetSingle(@this, out var result) ? result : defaultValueFactory();
  }

  /// <summary>
  /// Retrieves a single element from the specified <see cref="IEnumerable{T}"/> collection, or invokes a function with the collection as its argument to return a default value if the collection does not contain exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
  /// <param name="this">The <see cref="IEnumerable{TValue}"/> instance on which this extension method is called.</param>
  /// <param name="defaultValueFactory">A function that takes the entire collection as its parameter and produces the default value to return if the collection does not contain exactly one element.</param>
  /// <returns>
  /// The single element from the collection if it contains exactly one element; otherwise, the value produced by the <paramref name="defaultValueFactory"/> function using the entire collection.
  /// </returns>
  /// <remarks>
  /// This method is an extension method and can be called directly on any object that implements <see cref="IEnumerable{TValue}"/>.
  /// This variant of the SingleOrDefault method is particularly useful when the default value is dependent on the entire collection, allowing for dynamic computation of the default value based on the collection's current state.
  /// </remarks>
  /// <example>
  /// Here is an example of using the method with a default value factory function that depends on the collection:
  /// <code>
  /// var numbers = new List&lt;int&gt; { 42 };
  /// var singleValue = numbers.SingleOrDefault(collection => collection.Average());
  /// Console.WriteLine(singleValue);
  /// 
  /// var emptyList = new List&lt;int&gt;();
  /// var defaultValue = emptyList.SingleOrDefault(collection => collection.AverageOrDefault() ?? -1);
  /// Console.WriteLine(defaultValue);
  /// </code>
  /// This example will output:
  /// <code>
  /// 42
  /// -1
  /// </code>
  /// The default value factory function uses the collection to determine the default value, showcasing the method's flexibility in handling collections that do not contain exactly one item.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static TItem SingleOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>,TItem> defaultValueFactory) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(defaultValueFactory);

    // ReSharper disable PossibleMultipleEnumeration
    return TryGetSingle(@this, out var result) ? result : defaultValueFactory(@this);
    // ReSharper restore PossibleMultipleEnumeration
  }

  /// <summary>
  /// Determines whether a given <see cref="Enumerable"/> contains exactly one element.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <returns><see langword="true"/> if the <see cref="Enumerable"/> has one element; otherwise, <see langword="false"/>.</returns>
  public static bool IsSingle<TItem>(this IEnumerable<TItem> @this) {
    Against.ThisIsNull(@this);

    switch (@this) {
      case TItem[] array: return array.Length == 1;
      case ICollection<TItem> collection: return collection.Count == 1;
      default: {
        var found = false;
        foreach (var _ in @this) {
          if (found)
            return false;

          found = true;
        }

        return found;
      }
    }
  }

  /// <summary>
  /// Determines whether the given <see cref="Enumerable"/> contains more or less than one element.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <returns><see langword="true"/> if the <see cref="Enumerable"/> has less or more than one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNoSingle<TItem>(this IEnumerable<TItem> @this)
    => !IsSingle(@this)
  ;

  /// <summary>
  /// Determines whether a given <see cref="Enumerable"/> has more than one element.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <returns><see langword="true"/> if the <see cref="Enumerable"/> has at least two elements; otherwise, <see langword="false"/>.</returns>
  public static bool IsMultiple<TItem>(this IEnumerable<TItem> @this) {
    Against.ThisIsNull(@this);

    switch (@this) {
      case TItem[] array:
        return array.Length > 1;
      case ICollection<TItem> collection:
        return collection.Count > 1;
      default: {
        var found = false;
        foreach (var _ in @this) {
          if (found)
            return true;

          found = true;
        }

        return false;
      }
    }
  }

  /// <summary>
  /// Determines whether a given <see cref="Enumerable"/> has no more than one element.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <returns><see langword="true"/> if the <see cref="Enumerable"/> has zero or one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
# endif
  public static bool IsNoMultiple<TItem>(this IEnumerable<TItem> @this)
    => !IsMultiple(@this)
  ;

  /// <summary>
  /// Determines whether a given value is exactly once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="value">The value to look for</param>
  /// <returns><see langword="true"/> if the item is exactly one time in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
  public static bool HasSingle<TItem>(this IEnumerable<TItem> @this, TItem value) {
    Against.ThisIsNull(@this);

    var found = false;
    foreach (var item in @this)
      if (Equals(item, value)) {
        if(found) 
          return false;

        found = true;
      }

    return found;
  }

  /// <summary>
  /// Determines whether a given value is not exactly once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="value">The value to look for</param>
  /// <returns><see langword="true"/> if the item not or more than once in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasNoSingle<TItem>(this IEnumerable<TItem> @this, TItem value)
    => !HasSingle(@this, value)
  ;

  /// <summary>
  /// Determines whether a given value is more than once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="value">The value to look for</param>
  /// <returns><see langword="true"/> if the item is found more one time in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
  public static bool HasMultiple<TItem>(this IEnumerable<TItem> @this, TItem value) {
    Against.ThisIsNull(@this);

    var found = false;
    foreach (var item in @this)
      if (Equals(item, value)) {
        if (found)
          return true;

        found = true;
      }

    return false;
  }

  /// <summary>
  /// Determines whether a given value is not more than once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="value">The value to look for</param>
  /// <returns><see langword="true"/> if the item is found less than two times in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasNoMultiple<TItem>(this IEnumerable<TItem> @this, TItem value)
    => !HasMultiple(@this, value)
    ;


  /// <summary>
  /// Determines whether a given predicate matches exactly once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="predicate">The predicate to match</param>
  /// <returns><see langword="true"/> if the predicate matches exactly one time in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
  public static bool HasSingle<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);

    var found = false;
    foreach (var item in @this)
      if (predicate(item)) {
        if (found)
          return false;

        found = true;
      }

    return found;
  }

  /// <summary>
  /// Determines whether a given predicate matches not exactly once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="predicate">The predicate to match</param>
  /// <returns><see langword="true"/> if the predicate matches not at all or more than once in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasNoSingle<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)
    => !HasSingle(@this, predicate)
  ;

  /// <summary>
  /// Determines whether a given predicate matches more than once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="predicate">The predicate to match</param>
  /// <returns><see langword="true"/> if the predicate matches more than one time in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
  public static bool HasMultiple<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate) {
    Against.ThisIsNull(@this);

    var found = false;
    foreach (var item in @this)
      if (predicate(item)) {
        if (found)
          return true;

        found = true;
      }

    return false;
  }

  /// <summary>
  /// Determines whether a given predicate matches no more than once in the given <see cref="Enumerable"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of items</typeparam>
  /// <param name="this">This <see cref="Enumerable"/></param>
  /// <param name="predicate">The predicate to match</param>
  /// <returns><see langword="true"/> if the predicate matches less than two times in the <see cref="Enumerable"/>; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool HasNoMultiple<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)
    => !HasMultiple(@this, predicate)
  ;

}