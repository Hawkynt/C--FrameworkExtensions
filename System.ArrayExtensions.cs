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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace System {
  internal static partial class ArrayExtensions {
    private static readonly Exception _noElements = new InvalidOperationException("No Elements!");

    /// <summary>
    /// Gets a random element.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Array.</param>
    /// <param name="random">The random number generator, if any.</param>
    /// <returns>A random element from the array.</returns>
    public static TValue GetRandomElement<TValue>(this TValue[] This, Random random = null) {
      Contract.Requires(This != null);
      var length = This.Length;
      if (length == 0)
        throw _noElements;

      if (random == null)
        random = new Random();

      var index = random.Next(length);
      return (This[index]);
    }

    /// <summary>
    /// Gets a small portion of a byte array.
    /// </summary>
    /// <param name="This">This byte[].</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    /// <returns>The subsequent elements.</returns>
    public static byte[] Range(this byte[] This, int offset, int count) {
      if (This == null)
        return (null);
      if (count < 0)
        return (null);
      if (offset < 0)
        return (null);
      var length = This.Length;
      var max = count < (length - offset) ? count : length - offset;
      var result = new byte[max];
      Buffer.BlockCopy(This, offset, result, 0, max);
      return (result);
    }

    /// <summary>
    /// Clones the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This Array.</param>
    /// <returns>A new array or <c>null</c> if this array was <c>null</c>.</returns>
    public static TItem[] SafelyClone<TItem>(this TItem[] This) {
      if (This == null)
        return (null);
      var len = This.Length;
      var result = new TItem[len];
      Array.Copy(This, result, len);
      return (result);
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
    public static string Join<TIn>(this TIn[] This, string join = ", ", bool skipDefaults = false, Func<TIn, string> ptrConverter = null) {
      Contract.Requires(This != null);
      var result = new StringBuilder();
      var gotElements = false;
      var defaultValue = default(TIn);
      // ReSharper disable ForCanBeConvertedToForeach
      for (var i = 0; i < This.Length; i++) {
        var item = This[i];
        if (skipDefaults && EqualityComparer<TIn>.Default.Equals(item, defaultValue))
          continue;
        if (gotElements)
          result.Append(join);
        else
          gotElements = true;
        result.Append(ptrConverter == null ? ReferenceEquals(null, item) ? string.Empty : item.ToString() : ptrConverter(item));
      }
      // ReSharper restore ForCanBeConvertedToForeach
      return (gotElements ? result.ToString() : null);
    }
    /// <summary>
    /// Splices the specified array (returns part of that array).
    /// </summary>
    /// <typeparam name="TValue">Type of data in the array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="startIndex">The start element which should be included in the splice.</param>
    /// <param name="count">The number of elements from there on.</param>
    /// <returns></returns>
    public static TValue[] Range<TValue>(this TValue[] This, int startIndex, int count) {
      Contract.Requires(This != null);
      Contract.Requires(startIndex + count <= This.Length);
      Contract.Requires(startIndex >= 0);
      var result = new TValue[count];
      Array.Copy(This, startIndex, result, 0, count);
      return (result);
    }
    /// <summary>
    /// Swaps the specified data in an array.
    /// </summary>
    /// <typeparam name="TValue">Type of data in the array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="firstElementIndex">The first value.</param>
    /// <param name="secondElementIndex">The the second value.</param>
    public static void Swap<TValue>(this TValue[] This, int firstElementIndex, int secondElementIndex) {
      Contract.Requires(This != null);
      var value = This[firstElementIndex];
      This[firstElementIndex] = This[secondElementIndex];
      This[secondElementIndex] = value;
    }
    /// <summary>
    /// Shuffles the specified data.
    /// </summary>
    /// <typeparam name="TValue">Type of elements in the array.</typeparam>
    /// <param name="This">This array.</param>
    public static void Shuffle<TValue>(this TValue[] This) {
      Contract.Requires(This != null);
      var index = This.Length;
      var random = new Random();
      while (index > 1)
        This.Swap(random.Next(index), --index);
    }
    /// <summary>
    /// Quick-sort the given array.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements</typeparam>
    /// <param name="This">This array.</param>
    /// <returns>A sorted array copy.</returns>
    public static TValue[] QuickSorted<TValue>(this TValue[] This) where TValue : IComparable<TValue> {
      Contract.Requires(This != null);
      var result = new TValue[This.Length];
      This.CopyTo(result, 0);
      result.QuickSort();
      return (result);
    }
    /// <summary>
    /// Quick-sort the given array.
    /// </summary>
    /// <typeparam name="TValue">The type of the elements.</typeparam>
    /// <param name="This">This array.</param>
    public static void QuickSort<TValue>(this TValue[] This) where TValue : IComparable<TValue> {
      Contract.Requires(This != null);
      if (This.Length > 0)
        QuickSort_Comparable(This, 0, This.Length - 1);
    }
    private static void QuickSort_Comparable<TValue>(TValue[] This, int left, int right) where TValue : IComparable<TValue> {
      if (This == null) {
        // nothing to sort
      } else {
        var leftIndex = left;
        var rightIndex = right;
        var pivotItem = This[(leftIndex + rightIndex) >> 1];
        while (leftIndex <= rightIndex) {
          while (Comparer<TValue>.Default.Compare(pivotItem, This[leftIndex]) > 0)
            leftIndex++;
          while (Comparer<TValue>.Default.Compare(pivotItem, This[rightIndex]) < 0)
            rightIndex--;
          if (leftIndex > rightIndex) {
            continue;
          }
          This.Swap(leftIndex, rightIndex);
          leftIndex++;
          rightIndex--;
        }
        if (left < rightIndex)
          QuickSort_Comparable(This, left, rightIndex);
        if (leftIndex < right)
          QuickSort_Comparable(This, leftIndex, right);
      }
    }

    /// <summary>
    /// Converts all elements.
    /// </summary>
    /// <typeparam name="TInput">The type the elements.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns>An array containing the converted values.</returns>
    [Pure]
    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] This, Converter<TInput, TOutput> converter) {
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
      return (Array.ConvertAll(This, converter));
    }
    /// <summary>
    /// Converts all elements.
    /// </summary>
    /// <typeparam name="TInput">The type the elements.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns>An array containing the converted values.</returns>
    [Pure]
    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] This, Func<TInput, int, TOutput> converter) {
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
      var length = This.Length;
      var result = new TOutput[length];
      for (var index = length - 1; index >= 0; index--)
        result[index] = converter(This[index], index);
      return (result);
    }
    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Action<TInput> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      Array.ForEach(This, action);
    }
    /// <summary>
    /// Executes a callback with each element in an array in parallel.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback to execute for each element.</param>
    public static void ParallelForEach<TInput>(this TInput[] This, Action<TInput> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      Parallel.ForEach(This, action);
    }
    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Action<TInput, int> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      for (var intI = This.Length - 1; intI >= 0; intI--)
        action(This[intI], intI);
    }
    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Action<TInput, long> action) {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      for (var intI = This.LongLength - 1; intI >= 0; intI--)
        action(This[intI], intI);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Func<TInput, TInput> worker) {
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
      for (var index = This.LongLength - 1; index >= 0; index--)
        This[index] = worker(This[index]);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Func<TInput, int, TInput> worker) {
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
      for (var index = This.Length - 1; index >= 0; index--)
        This[index] = worker(This[index], index);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Func<TInput, long, TInput> worker) {
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
      for (var index = This.LongLength - 1; index >= 0; index--)
        This[index] = worker(This[index], index);
    }
    /// <summary>
    /// Returns true if there exists an array 
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns><c>true</c> if a given element exists; otherwise, <c>false</c>.</returns>
    [Pure]
    public static bool Exists<TInput>(this TInput[] This, Predicate<TInput> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      return (Array.Exists(This, predicate));
    }
    /// <summary>
    /// Gets the reverse.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <returns>An array where all values are inverted.</returns>
    [Pure]
    public static TInput[] Reverse<TInput>(this TInput[] This) {
      Contract.Requires(This != null);
      var length = This.LongLength;
      var result = new TInput[length];
      for (long i = 0, j = length - 1; j >= 0; i++, j--)
        result[j] = This[i];
      return (result);
    }

    /// <summary>
    /// Determines whether the given array contains the specified value.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [contains] [the specified this]; otherwise, <c>false</c>.
    /// </returns>
    [Pure]
    public static bool Contains<TItem>(this TItem[] This, TItem value) {
      Contract.Requires(This != null);
      return (This.IndexOf(value) >= 0);
    }

    /// <summary>
    /// Determines whether an array contains the specified value or not.
    /// </summary>
    /// <param name="This">This array.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the array contains that value; otherwise, <c>false</c>.
    /// </returns>
    [Pure]
    public static bool Contains(this Array This, object value) {
      Contract.Requires(This != null);
      // ReSharper disable LoopCanBeConvertedToQuery
      foreach (var item in This)
        if (item == value)
          return (true);
      // ReSharper restore LoopCanBeConvertedToQuery
      return (false);
    }

    /// <summary>
    /// Converts the array instance to a real array.
    /// </summary>
    /// <param name="This">This Array.</param>
    /// <returns>An array of objects holding the contents.</returns>
    [Pure]
    public static object[] ToArray(this Array This) {
      Contract.Requires(This != null && This.Rank > 0);
      var result = new object[This.Length];
      var lbound = This.GetLowerBound(0);
      for (var i = This.Length; i > 0; ) {
        --i;
        result[i] = This.GetValue(i + lbound);
      }
      return (result);
    }

    /// <summary>
    /// Gets the index of the first item matching the predicate, if any or -1.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">This Array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>
    /// The index of the item in the array or -1.
    /// </returns>
    [Pure]
    public static int IndexOf<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      for (var i = 0; i < This.Length; i++)
        if (predicate(This[i]))
          return (i);
      return (-1);
    }

    /// <summary>
    /// Gets the index of the first item matching the given, if any or -1.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="This">This Array.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The index of the item in the array or -1.
    /// </returns>
    [Pure]
    public static int IndexOf<TItem>(this TItem[] This, TItem value) {
      Contract.Requires(This != null);
      var comparer = EqualityComparer<TItem>.Default;
      for (var i = 0; i < This.Length; i++)
        if (comparer.Equals(value, This[i]))
          return (i);
      return (-1);
    }

    /// <summary>
    /// Gets the index of the first item matching the predicate, if any or -1.
    /// </summary>
    /// <param name="This">This Array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The index of the item in the array or -1.</returns>
    [Pure]
    public static int IndexOf(this Array This, Predicate<object> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      for (var i = This.GetLowerBound(0); i <= This.GetUpperBound(0); i++)
        if (predicate(This.GetValue(i)))
          return (i);
      return (-1);
    }

    #region high performance linq for arrays
    public static bool Any<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      return (This.Length > 0);
    }
    public static bool Any<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        if (predicate(This[i]))
          return (true);
      return (false);
    }
    public static TItem First<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      var length = This.Length;
      if (length == 0)
        throw _noElements;
      return (This[0]);
    }
    public static TItem Last<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      return (This[length - 1]);
    }
    public static TItem First<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      throw _noElements;
    }
    public static TItem Last<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = length - 1; i >= 0; i--) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      throw _noElements;
    }
    public static TItem FirstOrDefault<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      var length = This.Length;
      if (length == 0)
        return (default(TItem));
      return (This[0]);
    }
    public static TItem LastOrDefault<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      var length = This.LongLength;
      if (length == 0)
        return (default(TItem));
      return (This[length - 1]);
    }
    public static TItem FirstOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      return (default(TItem));
    }
    public static TItem LastOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = length - 1; i >= 0; i--) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      return (default(TItem));
    }
    public static TItem Aggregate<TItem>(this TItem[] This, Func<TItem, TItem, TItem> func) {
      Contract.Requires(This != null);
      Contract.Requires(func != null);
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      var result = This[0];
      for (var i = 1; i < length; i++)
        result = func(result, This[i]);
      return (result);
    }
    public static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] This, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func) {
      Contract.Requires(This != null);
      Contract.Requires(func != null);
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      var result = seed;
      for (var i = 0; i < length; i++)
        result = func(result, This[i]);
      return (result);
    }
    public static int Count<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      return (This.Length);
    }
    public static long LongCount<TItem>(this TItem[] This) {
      Contract.Requires(This != null);
      return (This.LongLength);
    }
    public static int Count<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var result = 0;
      // ReSharper disable LoopCanBeConvertedToQuery
      // ReSharper disable ForCanBeConvertedToForeach
      for (var i = 0; i < This.Length; i++)
        if (predicate(This[i]))
          result++;
      // ReSharper restore ForCanBeConvertedToForeach
      // ReSharper restore LoopCanBeConvertedToQuery
      return (result);
    }
    public static long LongCount<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var result = (long)0;
      for (var i = 0; i < This.LongLength; i++)
        if (predicate(This[i]))
          result++;
      return (result);
    }


    public static TItem FirstOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate, TItem defaultValue) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      foreach (var item in This.Where(predicate))
        return (item);
      return (defaultValue);
    }

    #region these special Array ...s
    public static IEnumerable<TResult> OfType<TResult>(this Array This) {
      Contract.Requires(This != null);
      for (var i = 0; i < This.LongLength; i++) {
        var item = This.GetValue(i);
        if (item is TResult)
          yield return (TResult)item;
      }
    }

    public static object FirstOrDefault(this Array This, Predicate<object> predicate, object defaultValue = null) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      foreach (var item in This) {
        if (predicate(item))
          return (item);
      }
      return (defaultValue);
    }

    public static IEnumerable<object> Reverse(this Array This) {
      for (var i = This.GetUpperBound(0); i >= This.GetLowerBound(0); i--) {
        var value = This.GetValue(i);
        yield return value;
      }
    }

    public static TItem FirstOrDefault<TItem>(this Array This, Predicate<TItem> predicate, TItem defaultValue = default(TItem)) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      foreach (var item in This) {
        if (predicate((TItem)item))
          return ((TItem)item);
      }
      return (defaultValue);
    }

    public static IEnumerable<TResult> Cast<TResult>(this Array This) {
      Contract.Requires(This != null);
      for (var i = This.GetLowerBound(0); i <= This.GetUpperBound(0); i++) {
        var value = This.GetValue(i);
        yield return value == null ? default(TResult) : (TResult)value;
      }
    }
    #endregion

    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] This, Func<TItem, TResult> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      var length = This.Length;
      for (var i = 0; i < length; i++)
        yield return selector(This[i]);
    }
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] This, Func<TItem, TResult> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        yield return selector(This[i]);
    }
    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] This, Func<TItem, int, TResult> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      var length = This.Length;
      for (var i = 0; i < length; i++)
        yield return selector(This[i], i);
    }
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] This, Func<TItem, long, TResult> selector) {
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        yield return selector(This[i], i);
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Predicate<TItem> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          yield return current;
      }
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Func<TItem, int, bool> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.Length;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current, i))
          yield return current;
      }
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Func<TItem, long, bool> predicate) {
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current, i))
          yield return current;
      }
    }
    #endregion

    #region byte-array specials
    /// <summary>
    /// GZips the given bytes.
    /// </summary>
    /// <param name="This">This Byte-Array.</param>
    /// <returns>A GZipped byte array.</returns>
    public static byte[] GZip(this byte[] This) {
      Contract.Requires(This != null);
      using (var targetStream = new MemoryStream()) {
        using (var gZipStream = new GZipStream(targetStream, CompressionMode.Compress, false))
          gZipStream.Write(This, 0, This.Length);

        return (targetStream.ToArray());
      }
    }

    /// <summary>
    /// Un-GZips the given bytes.
    /// </summary>
    /// <param name="This">This Byte-Array.</param>
    /// <returns>The unzipped byte array.</returns>
    public static byte[] UnGZip(this byte[] This) {
      Contract.Requires(This != null);
      using (var targetStream = new MemoryStream()) {
        using (var sourceStream = new MemoryStream(This)) {
          using (var gZipStream = new GZipStream(sourceStream, CompressionMode.Decompress, false)) {

            // decompress all bytes
            var buffer = new byte[64 * 1024];
            var bytesRead = gZipStream.Read(buffer, 0, buffer.Length);
            while (bytesRead > 0) {
              targetStream.Write(buffer, 0, bytesRead);
              bytesRead = gZipStream.Read(buffer, 0, buffer.Length);
            }
          }
        }
        return (targetStream.ToArray());
      }
    }
    #endregion

  }
}