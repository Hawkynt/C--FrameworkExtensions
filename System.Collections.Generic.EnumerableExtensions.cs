#region (c)2010-2020 Hawkynt
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


using System.Diagnostics;
#if NETFX_4
using System.Threading.Tasks;
#endif
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable MemberCanBePrivate.Global

namespace System.Collections.Generic {
  internal static partial class EnumerableExtensions {

    /// <summary>
    /// Returns the enumeration or <c>null</c> if it is empty.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Enumeration.</param>
    /// <returns><c>null</c> if the enumeration is empty; otherwise, the enumeration itself </returns>
    [DebuggerStepThrough]
    public static IEnumerable<TItem> ToNullIfEmpty<TItem>(this IEnumerable<TItem> @this) {
      if (@this == null)
        return null;

      var collection = @this as ICollection<TItem>;
      if (collection != null)
        return collection.Count < 1 ? null : @this;

      // ReSharper disable PossibleMultipleEnumeration
      using (var enumerator = @this.GetEnumerator())
        return enumerator.MoveNext() ? @this : null;
      // ReSharper restore PossibleMultipleEnumeration
    }

    /// <summary>
    /// Creates a hash set from the given enumeration.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <returns>A hashset</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this) => new HashSet<TItem>(@this);

    /// <summary>
    /// Creates a hash set from the given enumeration.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    /// A hashset
    /// </returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this, IEqualityComparer<TItem> comparer) => new HashSet<TItem>(@this, comparer);

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
      if (@this == null) throw new NullReferenceException();
      if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity), initialCapacity, "Must be positive");

      var items = new List<TItem>(initialCapacity);
      items.AddRange(@this);
      return new HashSet<TItem>(items);
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
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static HashSet<TResult> ToHashSet<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, IEqualityComparer<TResult> comparer = null) => comparer == null ? new HashSet<TResult>(@this.Select(selector)) : new HashSet<TResult>(@this.Select(selector), comparer);

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

      var thisCollection = @this as ICollection<TItem>;
      var otherCollection = other as ICollection<TItem>;
      if (thisCollection != null && otherCollection != null && thisCollection.Count != otherCollection.Count)
        return false;

      if (comparer == null)
        comparer = EqualityComparer<TItem>.Default;

      using (var thisEnumerator = @this.GetEnumerator())
      using (var otherEnumerator = other.GetEnumerator()) {
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
      if (@this == null) throw new NullReferenceException();

      if (random == null)
        random = new Random();

      return
        @this
        .Select(i => new { r = random.Next(), i })
        .OrderBy(a => a.r)
        .Select(a => a.i)
        ;
    }

    /// <summary>
    /// Determines whether the enumeration is <c>null</c> or empty.
    /// </summary>
    /// <param name="this">This Enumeration.</param>
    /// <returns>
    ///   <c>true</c> if the enumeration is <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
    [DebuggerStepThrough]
    public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> @this) {
      if (@this == null)
        return true;

      var collection = @this as ICollection<TItem>;
      if (collection != null)
        return collection.Count == 0;

      using (var enumerator = @this.GetEnumerator())
        return !enumerator.MoveNext();
    }

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
    public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

      if (@this == null)
        return true;

      var list = @this as IList<TItem>;
      if (list != null) {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < list.Count; ++i)
          if (predicate(list[i]))
            return true;

        return false;
      }

      using (var enumerator = @this.GetEnumerator())
        while (enumerator.MoveNext())
          if (predicate(enumerator.Current))
            return true;

      return false;
    }

    /// <summary>
    /// Concats all byte arrays into one.
    /// </summary>
    /// <param name="this">This Enumerable of byte[].</param>
    /// <returns>A block of bytes with all parts joined.</returns>
    [DebuggerStepThrough]
    public static byte[] ConcatAll(this IEnumerable<byte[]> @this) {
      if (@this == null)
        return null;

      List<byte[]> chunks;
      var totalSize = 0;
      var list = @this as IList<byte[]>;
      if (list != null) {
        chunks = new List<byte[]>(list.Count);
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < list.Count; i++) {
          var chunk = list[i];
          if (chunk == null || chunk.Length < 1)
            continue;

          totalSize += chunk.Length;
          chunks.Add(chunk);
        }
      } else {
        var collection = @this as ICollection<byte[]>;
        chunks = collection == null ? new List<byte[]>() : new List<byte[]>(collection.Count);
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
    /// Concats all enumerations into one.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">The this.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<TItem> ConcatAll<TItem>(this IEnumerable<IEnumerable<TItem>> @this) => @this.SelectMany(c => c as TItem[] ?? c.ToArray());

#if NETFX_4
    public static Tuple<IEnumerable<TItem>, IEnumerable<TItem>> Split<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) {
      if (@this == null) throw new NullReferenceException();
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

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
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool ContainsNot<TItem>(this IEnumerable<TItem> @this, TItem item, IEqualityComparer<TItem> comparer = null)
      => !(comparer == null ? @this.Contains(item) : @this.Contains(item, comparer))
      ;

    /// <summary>
    /// Determines whether the specified enumeration contains not the given items.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This Enumeration.</param>
    /// <param name="items">The items.</param>
    /// <param name="comparer">The comparer; if any.</param>
    /// <returns><c>true</c> if the enumeration does not contain the listed values; otherwise, <c>false</c>.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool ContainsNotAny<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items, IEqualityComparer<TItem> comparer = null)
      => !ContainsAny(@this, items, comparer)
      ;

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
      if (@this == null) throw new NullReferenceException();
      if (list == null) throw new ArgumentNullException(nameof(list));

      // we'll cache all visited values from the list, so we don't have to enumerate more than once
      var itemCache = new List<TItem>();

      if (equalityComparer == null)
        equalityComparer = EqualityComparer<TItem>.Default;

      using (var enumerator = list.GetEnumerator()) {

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
              if (box2 == null)
                return true;
            } else if (box2 != null) {
              if (equalityComparer.Equals(item, current))
                return true;
            }

          } // next list item

        } // next item
      }

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
      if (@this == null) throw new NullReferenceException();
      if (action == null) throw new ArgumentNullException(nameof(action));

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
      if (@this == null) throw new NullReferenceException();
      if (action == null) throw new ArgumentNullException(nameof(action));

      var index = 0;
      foreach (var item in @this)
        action(item, index++);
    }

#if NETFX_4
    /// <summary>
    /// Executes a callback for each item in parallel.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void ParallelForEach<TIn>(this IEnumerable<TIn> @this, Action<TIn> action)
      => Parallel.ForEach(@this, action)
      ;

    /// <summary>
    /// Executes a callback for each item in parallel.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    public static void ParallelForEach<TIn>(this IEnumerable<TIn> @this, Action<TIn, int> action) {
      if (@this == null) throw new NullReferenceException();
      if (action == null) throw new ArgumentNullException(nameof(action));

#pragma warning disable CC0031 // Check for null before calling a delegate
      @this.Select((v, i) => new Action(() => action(v, i))).ParallelForEach(i => i());
#pragma warning restore CC0031 // Check for null before calling a delegate
    }

#endif

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> @this, Func<TIn, TOut> converter)
      => @this.Select(converter)
      ;

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> @this, Func<TIn, int, TOut> converter)
      => @this.Select(converter)
      ;

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>
    /// A new enumeration which automatically calls the progress callback when items are pulled.
    /// </returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> @this, Action<double> progressCallback, bool delayed = false) {
      if (@this == null) throw new NullReferenceException();
      if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

      var collection = @this as ICollection<TIn> ?? @this.ToList();
      return AsProgressReporting(collection, collection.Count, progressCallback, delayed);
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>
    /// A new enumeration which automatically calls the progress callback when items are pulled.
    /// </returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> @this, Action<long, long> progressCallback, bool delayed = false) {
      if (@this == null) throw new NullReferenceException();
      if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

      var collection = @this as ICollection<TIn> ?? @this.ToList();
      return AsProgressReporting(collection, collection.Count, progressCallback, delayed);
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="length">The length of the enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> @this, int length, Action<double> progressCallback, bool delayed = false) {
      if (@this == null) throw new NullReferenceException();
      if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

      return AsProgressReporting(@this, length, (i, c) => progressCallback(i == c ? 1 : (double)i / c), delayed);
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="length">The length of the enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> @this, int length, Action<long, long> progressCallback, bool delayed = false) {
      if (@this == null) throw new NullReferenceException();
      if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

      if (length == 0)
        progressCallback(0, 0);
      else {
        progressCallback(0, length);

        Action<int> action = i => progressCallback(i, length);
        Action<int> nullAction = _ => { };
        var preAction = delayed ? action : nullAction;
        var postAction = delayed ? nullAction : action;

        var currentIndex = 0;
        foreach (var item in @this) {
          preAction(currentIndex);
          yield return item;
          postAction(currentIndex);
          currentIndex++;
        }

        progressCallback(length, length);
      }
    }

    /// <summary>
    /// Tests whether the given condition applies to all elements or not.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="this">This Enumeration.</param>
    /// <param name="condition">The condition.</param>
    /// <returns></returns>
    public static bool All<TSource>(this IEnumerable<TSource> @this, Func<TSource, int, bool> condition) {
      if (@this == null) throw new NullReferenceException();
      if (condition == null) throw new ArgumentNullException(nameof(condition));

      // original but slower implementation
      //return (!This.Where((o, i) => !condition(o, i)).Any());

      // fast by avoiding inner lambda
      var index = -1;
      foreach (var item in @this) {
        checked { ++index; }
        if (!condition(item, index))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Distincts the specified enumeration by the specified selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the inpute elements.</typeparam>
    /// <typeparam name="TCompare">The type of the comparison elements.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>An enumeration with distinct elements.</returns>
    public static IEnumerable<TIn> Distinct<TIn, TCompare>(this IEnumerable<TIn> @this, Func<TIn, TCompare> selector) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));

      var list = (
        from i in @this
        select new {
          comparer = selector(i),
          data = i
        }
      ).ToList();

      var groups = list.GroupBy(i => i.comparer);
      var firstOfGroup = groups.Select(g => g.First().data);
      return firstOfGroup;
    }

    /// <summary>
    /// Combines all sub-elements of the elements into one result set.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">The source.</param>
    /// <returns>A list of items.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<TItem> SelectMany<TItem>(this IEnumerable<IEnumerable<TItem>> @this)
      => @this.SelectMany(s => s as TItem[] ?? s.ToArray())
      ;

    /// <summary>
    /// Joins the specified elements into a string.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="join">The delimiter.</param>
    /// <param name="skipDefaults">if set to <c>true</c> all default values will be skipped.</param>
    /// <param name="converter">The converter.</param>
    /// <returns>The joines string.</returns>
    public static string Join<TIn>(this IEnumerable<TIn> @this, string join = ", ", bool skipDefaults = false, Func<TIn, string> converter = null) {
      if (@this == null) throw new NullReferenceException();

      var result = new StringBuilder();
      var gotElements = false;
      var defaultValue = default(TIn);

      // if no converter was given, use the default toString if not null
      if (converter == null)
        converter = i => ReferenceEquals(i, null) ? null : i.ToString();


      foreach (var item in (skipDefaults ? @this.Where(item => !EqualityComparer<TIn>.Default.Equals(item, defaultValue)) : @this)) {
        if (gotElements)
          result.Append(join);
        else
          gotElements = true;

#pragma warning disable CC0031 // Check for null before calling a delegate
        result.Append(converter(item));
#pragma warning restore CC0031 // Check for null before calling a delegate
      }
      return gotElements ? result.ToString() : null;
    }

    /// <summary>
    /// Gets the index of the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The index of the matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static int IndexOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, int defaultValue = -1) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));

      var result = 0;
      foreach (var item in @this)
        if (selector(item))
          return result;
        else
          result++;

      return defaultValue;
    }

    /// <summary>
    /// Gets the index of the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The index of the matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static int IndexOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<int> defaultValue) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));
      if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));

      var result = 0;
      foreach (var item in @this)
        if (selector(item))
          return result;
        else
          result++;

      return defaultValue();
    }

    /// <summary>
    /// Gets the index of the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The index of the matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static int IndexOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<IEnumerable<TIn>, int> defaultValue) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));
      if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));

      var result = 0;
      // ReSharper disable once PossibleMultipleEnumeration
      foreach (var item in @this)
        if (selector(item))
          return result;
        else
          result++;

      // ReSharper disable once PossibleMultipleEnumeration
      return defaultValue(@this);
    }

    /// <summary>
    /// Gets the index of the given value.
    /// </summary>
    /// <typeparam name="TIn">The type of the elements.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="item">The item.</param>
    /// <returns>The position of the item in the enumeration or -1</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static int IndexOf<TIn>(this IEnumerable<TIn> @this, TIn item) => IndexOrDefault(@this, a => Equals(a, item), -1);

    /// <summary>
    /// Gets the first item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, TIn defaultValue) {
      if (@this == null) throw new NullReferenceException();

      foreach (var item in @this)
        return item;

      return defaultValue;
    }

    /// <summary>
    /// Gets the first item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn> defaultValueFactory) {
      if (@this == null) throw new NullReferenceException();
      if (defaultValueFactory == null) throw new ArgumentNullException(nameof(defaultValueFactory));

      foreach (var item in @this)
        return item;

      return defaultValueFactory();
    }

    /// <summary>
    /// Gets the first item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<IEnumerable<TIn>, TIn> defaultValueFactory) {
      if (@this == null) throw new NullReferenceException();
      if (defaultValueFactory == null) throw new ArgumentNullException(nameof(defaultValueFactory));

      // ReSharper disable once PossibleMultipleEnumeration
      foreach (var item in @this)
        return item;

      // ReSharper disable once PossibleMultipleEnumeration
      return defaultValueFactory(@this);
    }

    /// <summary>
    /// Gets the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));

      foreach (var item in @this)
        if (selector(item))
          return item;

      return default(TIn);
    }

    /// <summary>
    /// Gets the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, TIn defaultValue) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));

      foreach (var item in @this)
        if (selector(item))
          return item;

      return defaultValue;
    }

    /// <summary>
    /// Gets the first item matching the condition or the result of the default value factory.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<TIn> defaultValueFactory) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));
      if (defaultValueFactory == null) throw new ArgumentNullException(nameof(defaultValueFactory));

      foreach (var item in @this)
        if (selector(item))
          return item;

      return defaultValueFactory();
    }

    /// <summary>
    /// Gets the first item matching the condition or the result of the default value factory.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<IEnumerable<TIn>, TIn> defaultValueFactory) {
      if (@this == null) throw new NullReferenceException();
      if (selector == null) throw new ArgumentNullException(nameof(selector));
      if (defaultValueFactory == null) throw new ArgumentNullException(nameof(defaultValueFactory));

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
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector)
      => FirstOrDefault(@this.Reverse(), selector)
    ;

    /// <summary>
    /// Gets the last item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, TIn defaultValue)
      => FirstOrDefault(@this.Reverse(), selector, defaultValue)
    ;

    /// <summary>
    /// Gets the last item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<TIn> defaultValueFactory)
      => FirstOrDefault(@this.Reverse(), selector, defaultValueFactory)
    ;

    /// <summary>
    /// Gets the last item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>The matched item or the given default value.</returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn, bool> selector, Func<IEnumerable<TIn>, TIn> defaultValueFactory)
      => FirstOrDefault(@this.Reverse(), selector, defaultValueFactory)
    ;

    /// <summary>
    /// Gets the last item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, TIn defaultValue)
      => FirstOrDefault(@this.Reverse(), defaultValue)
      ;

    /// <summary>
    /// Gets the last item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<TIn> defaultValueFactory)
      => FirstOrDefault(@this.Reverse(), defaultValueFactory)
    ;

    /// <summary>
    /// Gets the last item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> @this, Func<IEnumerable<TIn>, TIn> defaultValueFactory)
      => FirstOrDefault(@this.Reverse(), defaultValueFactory)
    ;

    /// <summary>
    /// Orders the collection by its elements itself.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<TIn> OrderBy<TIn>(this IEnumerable<TIn> @this) => @this.OrderBy(i => i);

    /// <summary>
    /// Returns the items in a randomized order.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Enumeration.</param>
    /// <param name="random">The random instance, if any.</param>
    /// <returns>The items in a randomized order.</returns>
    public static IEnumerable<TItem> Randomize<TItem>(this IEnumerable<TItem> @this, Random random = null) {
      if (random == null)
        random = new Random();

      var list = @this.Select(o => o).ToList();
      int count;
      while ((count = list.Count) > 0) {
        var index = count == 1 ? 0 : random.Next(0, count);
        yield return list[index];
        list.RemoveAt(index);
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
      if (@this == null) throw new NullReferenceException();
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

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
      if (@this == null) throw new NullReferenceException();
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));

      return @this.SkipWhile(i => !predicate(i));
    }
  }
}