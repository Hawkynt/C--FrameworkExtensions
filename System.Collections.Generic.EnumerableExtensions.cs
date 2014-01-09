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

using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dword = System.UInt32;
using qword = System.UInt64;
using word = System.UInt16;

namespace System.Collections.Generic {
  internal static partial class EnumerableExtensions {

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
    /// <returns>
    ///   <c>true</c> if the enumeration contains any of the listed values; otherwise, <c>false</c>.
    /// </returns>
    public static bool ContainsAny<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> list) {
      Contract.Requires(This != null);
      Contract.Requires(list != null);

      // we'll cache all visited values from the list, so we don't have to enumerate more than once
      var itemCache = new List<TItem>();
      var enumerator = list.GetEnumerator();

      // we'll need this for direct comparisons
      var equalityComparer = EqualityComparer<TItem>.Default;

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
      Contract.Requires(This != null);
      Contract.Requires(action != null);

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
      Contract.Requires(This != null);
      Contract.Requires(action != null);
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
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      Parallel.ForEach(This, action);
    }
    /// <summary>
    /// Executes a callback for each item in parallel.
    /// </summary>
    /// <typeparam name="TIn">The type of the items.</typeparam>
    /// <param name="This">This enumeration.</param>
    /// <param name="action">The call to execute.</param>
    public static void ParallelForEach<TIn>(this IEnumerable<TIn> This, Action<TIn, int> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      var index = 0;
      Parallel.ForEach(This, item => action(item, index++));
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
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
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
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
      var collection = This as ICollection<TIn> ?? new List<TIn>(This);
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
      Contract.Requires(This != null);
      Contract.Requires(progressCallback != null);
      if (length == 0) {
        progressCallback(100);
      } else {
        var currentIndex = 0;
        progressCallback(0);
        foreach (var item in This) {
          if (delayed)
            progressCallback(Math.Min(100, currentIndex++ * 100d / length));
          yield return (item);
          if (!delayed)
            progressCallback(Math.Min(100, currentIndex++ * 100d / length));
        }
        progressCallback(100);
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
      Contract.Requires(This != null);
      Contract.Requires(condition != null);

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
      Contract.Requires(This != null);
      Contract.Requires(selector != null);

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
      Contract.Requires(This != null);
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
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      var result = 0;
      foreach (var item in This)
        if (selector(item))
          return (result);
        else
          result++;

      return (defaultValue);
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
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      foreach (var item in This.Where(selector))
        return (item);

      return (defaultValue);
    }
    /// <summary>
    /// Firsts the or default.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static TIn FirstOrDefault<TIn>(this IEnumerable<TIn> This, TIn defaultValue) {
      Contract.Requires(This != null);
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
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      Contract.Requires(defaultValueFactory != null);
      foreach (var item in This.Where(selector))
        return (item);

      return (defaultValueFactory());
    }

    /// <summary>
    /// Orders the collection by its elements itself.
    /// </summary>
    /// <typeparam name="TIn">The type of the in.</typeparam>
    /// <param name="This">The this.</param>
    /// <returns></returns>
    public static IEnumerable<TIn> OrderBy<TIn>(this IEnumerable<TIn> This) {
      Contract.Requires(This != null);
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

    #region sum
    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static TimeSpan Sum(this IEnumerable<TimeSpan> This) {
      Contract.Requires(This != null);
      return (TimeSpan.FromMilliseconds(This.Select(i => i.TotalMilliseconds).Sum()));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static TimeSpan Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, TimeSpan> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Sum());
    }

    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static word Sum(this IEnumerable<word> This) {
      Contract.Requires(This != null);
      return (This.Aggregate((word)0, (current, i) => (word)(current + i)));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static word Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, word> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Sum());
    }

    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static dword Sum(this IEnumerable<dword> This) {
      Contract.Requires(This != null);
      return (This.Aggregate((dword)0, (current, i) => current + i));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static dword Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, dword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Sum());
    }

    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static qword Sum(this IEnumerable<qword> This) {
      Contract.Requires(This != null);
      return (This.Aggregate((qword)0, (current, i) => current + i));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static qword Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, qword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Sum());
    }
    #endregion
    #region min
    public static sbyte Min(this IEnumerable<sbyte> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = sbyte.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static byte Min(this IEnumerable<byte> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = byte.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static word Min(this IEnumerable<word> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = word.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static short Min(this IEnumerable<short> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = short.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static dword Min(this IEnumerable<dword> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = dword.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static qword Min(this IEnumerable<qword> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = qword.MaxValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item < result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }

    public static sbyte Min<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    public static byte Min<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    public static word Min<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    public static short Min<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    public static dword Min<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    public static qword Min<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Min());
    }
    #endregion
    #region minOrDefault
    public static sbyte MinOrDefault(this IEnumerable<sbyte> This, sbyte defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static byte MinOrDefault(this IEnumerable<byte> This, byte defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static word MinOrDefault(this IEnumerable<word> This, word defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static short MinOrDefault(this IEnumerable<short> This, short defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static dword MinOrDefault(this IEnumerable<dword> This, dword defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static int MinOrDefault(this IEnumerable<int> This, int defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static qword MinOrDefault(this IEnumerable<qword> This, qword defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static long MinOrDefault(this IEnumerable<long> This, long defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static float MinOrDefault(this IEnumerable<float> This, float defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static double MinOrDefault(this IEnumerable<double> This, double defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }
    public static decimal MinOrDefault(this IEnumerable<decimal> This, decimal defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static sbyte MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector, sbyte defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static byte MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector, byte defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static word MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector, word defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static short MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector, short defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static dword MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector, dword defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static int MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, int> selector, int defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static qword MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector, qword defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static long MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, long> selector, long defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static float MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, float> selector, float defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static double MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, double> selector, double defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    public static decimal MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, decimal> selector, decimal defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min(selector));
    }
    #endregion
    #region max
    public static sbyte Max(this IEnumerable<sbyte> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = sbyte.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static byte Max(this IEnumerable<byte> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = byte.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static word Max(this IEnumerable<word> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = word.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static short Max(this IEnumerable<short> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = short.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static dword Max(this IEnumerable<dword> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = dword.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }
    public static qword Max(this IEnumerable<qword> This) {
      Contract.Requires(This != null);
      var hasValues = false;
      var result = qword.MinValue;
      foreach (var item in This) {
        if (hasValues) {
          if (item > result)
            result = item;
          continue;
        }
        result = item;
        hasValues = true;
      }
      if (!hasValues)
        throw new InvalidOperationException("Enumeration is empty.");
      return (result);
    }

    public static sbyte Max<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    public static byte Max<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    public static word Max<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    public static short Max<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    public static dword Max<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    public static qword Max<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      return (This.Select(selector).Max());
    }
    #endregion
    #region maxOrDefault
    public static sbyte MaxOrDefault(this IEnumerable<sbyte> This, sbyte defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static byte MaxOrDefault(this IEnumerable<byte> This, byte defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static word MaxOrDefault(this IEnumerable<word> This, word defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static short MaxOrDefault(this IEnumerable<short> This, short defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static dword MaxOrDefault(this IEnumerable<dword> This, dword defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static int MaxOrDefault(this IEnumerable<int> This, int defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static qword MaxOrDefault(this IEnumerable<qword> This, qword defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static long MaxOrDefault(this IEnumerable<long> This, long defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static float MaxOrDefault(this IEnumerable<float> This, float defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static double MaxOrDefault(this IEnumerable<double> This, double defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }
    public static decimal MaxOrDefault(this IEnumerable<decimal> This, decimal defaultValue = 0) {
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static sbyte MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector, sbyte defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static byte MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector, byte defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static word MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector, word defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static short MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector, short defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static dword MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector, dword defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static int MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, int> selector, int defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static qword MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector, qword defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static long MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, long> selector, long defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static float MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, float> selector, float defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static double MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, double> selector, double defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    public static decimal MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, decimal> selector, decimal defaultValue = 0) {
      Contract.Requires(selector != null);
      if (This == null)
        return (defaultValue);
      var items = This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max(selector));
    }
    #endregion
    #region AverageOrDefault
    public static double AverageOrDefault(this IEnumerable<double> This, double defaultValue = 0) {
      if (This == null)
        return (defaultValue);

      double result = 0;
      var count = 0;
      foreach (var item in This) {
        result += item;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }
    #endregion
  }
}