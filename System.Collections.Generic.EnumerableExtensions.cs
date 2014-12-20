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


#if NET35
using System.Diagnostics;
#else
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
#endif
using System.Linq;
using System.Text;

namespace System.Collections.Generic {
  internal static partial class EnumerableExtensions {

    /// <summary>
    /// Tests whether two enumerations of the same type are equal.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This IEnumerable.</param>
    /// <param name="other">The other IEnumerable.</param>
    /// <param name="comparer">The comparer, if any; otherwise, uses the default comparer for the given item type.</param>
    /// <returns></returns>
    public static bool AreEqual<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer = null) {
      if (ReferenceEquals(This, other))
        return (true);

      if (This == null || other == null)
        return (false);

      if (comparer == null)
        comparer = EqualityComparer<TItem>.Default;

      using (var thisEnumerator = This.GetEnumerator())
      using (var otherEnumerator = other.GetEnumerator()) {
        bool hasMoreItems;

        // until at least one enumeration has ended
        while ((hasMoreItems = thisEnumerator.MoveNext()) == otherEnumerator.MoveNext()) {

          // both ended, so they must be equal
          if (!hasMoreItems)
            return (true);

          // if current element differs, break
          if (!comparer.Equals(thisEnumerator.Current, otherEnumerator.Current))
            return (false);

        }

        // one of the enumerations ended first, so they are not equal
        return (false);
      }
    }

    /// <summary>
    /// Shuffles the specified enumeration.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">This IEnumerable.</param>
    /// <param name="rng">The random number generator.</param>
    /// <returns>A shuffled enumeration.</returns>
    public static IEnumerable<TItem> Shuffle<TItem>(this IEnumerable<TItem> This, Random rng = null) {
      Contract.Requires(This != null);
      if (rng == null)
        rng = new Random();

      return (
        This
        .Select(i => new { r = rng.Next(), i })
        .OrderBy(a => a.r)
        .Select(a => a.i)
      );
    }

    /// <summary>
    /// Determines whether the enumeration is <c>null</c> or empty.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns>
    ///   <c>true</c> if the enumeration is <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> This) {
      return This == null || !This.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// Concats all byte arrays into one.
    /// </summary>
    /// <param name="This">This Enumerable of byte[].</param>
    /// <returns>A block of bytes with all parts joined.</returns>
    public static byte[] ConcatAll(this IEnumerable<byte[]> This) {
      if (This == null)
        return (null);
      var chunks = (
        from i in This
        where i != null && i.Length > 0
        select new { Size = i.Length, Data = i }
      ).ToArray();
      var totalSize = chunks.Sum(i => i.Size);
      var result = new byte[totalSize];
      var index = 0;
      foreach (var chunk in chunks) {
        Buffer.BlockCopy(chunk.Data, 0, result, index, chunk.Size);
        index += chunk.Size;
      }
      return (result);
    }

    /// <summary>
    /// Concats all enumerations into one.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">The this.</param>
    /// <returns></returns>
    public static IEnumerable<TItem> ConcatAll<TItem>(this IEnumerable<IEnumerable<TItem>> This) {
      return (This.SelectMany(c => c));
    }

    /// <summary>
    /// Determines whether the specified enumeration contains any of the items given by the second enumeration.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="list">The list of items we should look for.</param>
    /// <param name="equalityComparer">The equality comparer.</param>
    /// <returns>
    ///   <c>true</c> if the enumeration contains any of the listed values; otherwise, <c>false</c>.
    /// </returns>
    public static bool ContainsAny<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> list, IEqualityComparer<TItem> equalityComparer = null) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(list != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(list != null);
#endif

      // we'll cache all visited values from the list, so we don't have to enumerate more than once
      var itemCache = new List<TItem>();
      var enumerator = list.GetEnumerator();

      // we'll need this for direct comparisons
      if (equalityComparer == null)
        equalityComparer = EqualityComparer<TItem>.Default;

      // let's look at all our items
      foreach (var item in This) {

        // search the cache first
        if (itemCache.Contains(item))
          return (true);

        // try continuing the list
        while (enumerator.MoveNext()) {

          // get next item
          var current = enumerator.Current;

          // add to cache
          itemCache.Add(current);

          // check for equality
          object box1 = item;
          object box2 = current;
          if (box1 == null) {
            if (box2 == null)
              return (true);
          } else if (box2 != null) {
            if (equalityComparer.Equals(item, current))
              return (true);
          }

        } // next list item

      } // next item

      // we found nothing that matched
      return (false);
    }

    /// <summary>
    /// Executes the given action on each element of the enumeration.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This enumerable.</param>
    /// <param name="action">The action.</param>
    public static void ForEach<TItem>(this IEnumerable<TItem> This, Action<TItem> action) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(action != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif

      foreach (var item in This)
        action(item);

    }

    /// <summary>
    /// Executes a callback for each item.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    public static void ForEach<TIn>(this IEnumerable<TIn> This, Action<TIn, int> action) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(action != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif
      var index = 0;
      foreach (var item in This)
        action(item, index++);
    }
    /// <summary>
    /// Executes a callback for each item in parallel.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    public static void ParallelForEach<TIn>(this IEnumerable<TIn> This, Action<TIn> action) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(action != null);
      foreach (var item in This)
        action.BeginInvoke(item, action.EndInvoke, null);
#else
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      Parallel.ForEach(This, action);
#endif
    }

    /// <summary>
    /// Executes a callback for each item in parallel.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    public static void ParallelForEach<TIn>(this IEnumerable<TIn> This, Action<TIn, int> action) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(action != null);
      var index = 0;
      foreach (var item in This)
        action.BeginInvoke(item, index++, action.EndInvoke, null);
#else
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      var index = 0;
      Parallel.ForEach(This, item => action(item, index++));
#endif
    }

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns></returns>
    public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> This, Func<TIn, TOut> converter) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(converter != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
#endif
      return (This.Select(converter));
    }

    /// <summary>
    /// Converts all.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns></returns>
    public static IEnumerable<TOut> ConvertAll<TIn, TOut>(this IEnumerable<TIn> This, Func<TIn, int, TOut> converter) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(converter != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
#endif
      return (This.Select(converter));
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>
    /// A new enumeration which automatically calls the progress callback when items are pulled.
    /// </returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> This, Action<double> progressCallback, bool delayed = false) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(progressCallback != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
#endif
      var collection = This as ICollection<TIn> ?? This.ToList();
      return (collection.AsProgressReporting((collection).Count, progressCallback, delayed));
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>
    /// A new enumeration which automatically calls the progress callback when items are pulled.
    /// </returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> This, Action<long, long> progressCallback, bool delayed = false) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(progressCallback != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
#endif
      var collection = This as ICollection<TIn> ?? This.ToList();
      return (collection.AsProgressReporting((collection).Count, progressCallback, delayed));
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="length">The length of the enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> This, int length, Action<double> progressCallback, bool delayed = false) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(progressCallback != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
#endif
      return (This.AsProgressReporting(length, (i, c) => progressCallback(i == c ? 1 : (double)i / c), delayed));
    }

    /// <summary>
    /// Reports the progress while walking through the enumerable.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="length">The length of the enumeration.</param>
    /// <param name="progressCallback">The progress callback.</param>
    /// <param name="delayed">if set to <c>true</c> the progress will be set delayed (when the next item is fetched).</param>
    /// <returns>A new enumeration which automatically calls the progress callback when items are pulled.</returns>
    public static IEnumerable<TIn> AsProgressReporting<TIn>(this IEnumerable<TIn> This, int length, Action<long, long> progressCallback, bool delayed = false) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(progressCallback != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
#endif
      if (length == 0) {
        progressCallback(0, 0);
      } else {
        progressCallback(0, length);

        Action<int> action = i => progressCallback(i, length);
        Action<int> nullAction = _ => { };
        var preAction = delayed ? action : nullAction;
        var postAction = delayed ? nullAction : action;

        var currentIndex = 0;
        foreach (var item in This) {
          preAction(currentIndex);
          yield return (item);
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
    /// <param name="This">This Enumeration.</param>
    /// <param name="condition">The condition.</param>
    /// <returns></returns>
    public static bool All<TSource>(this IEnumerable<TSource> This, Func<TSource, int, bool> condition) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(condition != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(condition != null);
#endif

      // original but slower implementation
      //return (!This.Where((o, i) => !condition(o, i)).Any());

      // fast by avoiding inner lambda
      var index = -1;
      foreach (var item in This) {
        checked { ++index; }
        if (!condition(item, index))
          return (false);
      }
      return (true);
    }

    /// <summary>
    /// Distincts the specified enumeration by the specified selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the inpute elements.</typeparam>
    /// <typeparam name="TCompare">The type of the comparison elements.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>An enumeration with distinct elements.</returns>
    public static IEnumerable<TIn> Distinct<TIn, TCompare>(this IEnumerable<TIn> This, Func<TIn, TCompare> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif

      var list = (
        from i in This
        select new {
          comparer = selector(i),
          data = i
        }
      ).ToList();
      var groups = list.GroupBy(i => i.comparer);
      var firstOfGroup = groups.Select(g => g.First().data);
      return (firstOfGroup);
    }

    /// <summary>
    /// Combines all sub-elements of the elements into one result set.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>A list of items.</returns>
    public static IEnumerable<TItem> SelectMany<TItem>(this IEnumerable<IEnumerable<TItem>> source) {
      return (source.SelectMany(s => s));
    }

    /// <summary>
    /// Joins the specified elements into a string.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="join">The delimiter.</param>
    /// <param name="skipDefaults">if set to <c>true</c> all default values will be skipped.</param>
    /// <param name="ptrConverter">The converter.</param>
    /// <returns>The joines string.</returns>
    public static string Join<TIn>(this IEnumerable<TIn> This, string join = ", ", bool skipDefaults = false, Func<TIn, string> ptrConverter = null) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var result = new StringBuilder();
      var gotElements = false;
      var defaultValue = default(TIn);

      // if no converter was given, use the default toString if not null
      if (ptrConverter == null)
        ptrConverter = i => ReferenceEquals(i, null) ? null : i.ToString();


      foreach (var item in (skipDefaults ? This.Where(item => !EqualityComparer<TIn>.Default.Equals(item, defaultValue)) : This)) {
        if (gotElements)
          result.Append(join);
        else
          gotElements = true;

        result.Append(ptrConverter(item));
      }
      return (gotElements ? result.ToString() : null);
    }

    /// <summary>
    /// Gets the index of the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The index of the matched item or the given default value.</returns>
    public static int IndexOrDefault<TIn>(this IEnumerable<TIn> This, Func<TIn, bool> selector, int defaultValue = -1) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      var result = 0;
      foreach (var item in This)
        if (selector(item))
          return (result);
        else
          result++;

      return (defaultValue);
    }

    /// <summary>
    /// Gets the index of the given value.
    /// </summary>
    /// <typeparam name="TIn">The type of the elements.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="item">The item.</param>
    /// <returns>The position of the item in the enumeration or -1</returns>
    public static int IndexOf<TIn>(this IEnumerable<TIn> This, TIn item) {
      return (This.IndexOrDefault(a => object.Equals(a, item)));
    }

    /// <summary>
    /// Gets the first item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> This, Func<TIn, bool> selector, TIn defaultValue = default(TIn)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      foreach (var item in This.Where(selector))
        return (item);

      return (defaultValue);
    }
    /// <summary>
    /// Gets the first item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> This, TIn defaultValue) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      foreach (var item in This)
        return (item);

      return (defaultValue);
    }
    /// <summary>
    /// Gets the first item matching the condition or the result of the default value factory.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> This, Func<TIn, bool> selector, Func<TIn> defaultValueFactory) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
      Debug.Assert(defaultValueFactory != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      Contract.Requires(defaultValueFactory != null);
#endif
      foreach (var item in This.Where(selector))
        return (item);

      return (defaultValueFactory());
    }


    /// <summary>
    /// Gets the last item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> This, Func<TIn, bool> selector, TIn defaultValue = default(TIn)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (This.Reverse().FirstOrDefault(selector, defaultValue));
    }

    /// <summary>
    /// Gets the last item or the default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> This, TIn defaultValue) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (This.Reverse().FirstOrDefault(defaultValue));
    }

    /// <summary>
    /// Gets the last item matching the condition or the given default value.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValueFactory">The default value.</param>
    /// <returns>The matched item or the given default value.</returns>
    public static TIn LastOrDefault<TIn>(this IEnumerable<TIn> This, Func<TIn, bool> selector, Func<TIn> defaultValueFactory) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (This.Reverse().FirstOrDefault(selector, defaultValueFactory));
    }

    /// <summary>
    /// Orders the collection by its elements itself.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="This">The this.</param>
    /// <returns></returns>
    public static IEnumerable<TIn> OrderBy<TIn>(this IEnumerable<TIn> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (This.OrderBy(i => i));
    }

    /// <summary>
    /// Returns the items in a randomized order.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="random">The random instance, if any.</param>
    /// <returns>The items in a randomized order.</returns>
    public static IEnumerable<TItem> Randomize<TItem>(this IEnumerable<TItem> This, Random random = null) {
      if (random == null)
        random = new Random();

      var list = This.Select(o => o).ToList();
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
    /// <param name="This">The this.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static IEnumerable<TItem> TakeUntil<TItem>(this IEnumerable<TItem> This, Func<TItem, bool> predicate) {
      return (This.TakeWhile(i => !predicate(i)));
    }

    /// <summary>
    /// Skips items until a given condition is met.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns></returns>
    public static IEnumerable<TItem> SkipUntil<TItem>(this IEnumerable<TItem> This, Func<TItem, bool> predicate) {
      return (This.SkipWhile(i => !predicate(i)));
    }

  }
}