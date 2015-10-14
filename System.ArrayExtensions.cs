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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class ArrayExtensions {

    #region nested types
    [DebuggerDisplay("{ToString()}")]
    internal class ReadOnlyArraySlice<TItem> : IEnumerable<TItem> {

      protected readonly TItem[] _source;
      protected readonly int _start;

      public ReadOnlyArraySlice(TItem[] source, int start, int length) {
#if NETFX_4
        Contract.Requires(source != null);
#endif
        if (start + length > source.Length)
          throw new ArgumentException("Exceeding source length", nameof(length));

        this._source = source;
        this._start = start;
        this.Length = length;
      }

      public int Length { get; }

      public TItem this[int index] {
        get {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          return (this._source[index + this._start]);
        }
      }

      public IEnumerable<TItem> Values {
        get {
          var maxIndex = this._start + this.Length;
          for (var i = this._start; i < maxIndex; ++i)
            yield return this._source[i];
        }
      }

      #region Implementation of IEnumerable

      public IEnumerator<TItem> GetEnumerator() => this.Values.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

      #endregion

      public static explicit operator TItem[] (ReadOnlyArraySlice<TItem> @this) => @this.ToArray();

      #region Overrides of Object

      public override string ToString() => $"{typeof(TItem).Name}[{this._start}..{this._start + this.Length - 1}]";

      #endregion
    }
    internal class ArraySlice<TItem> : ReadOnlyArraySlice<TItem> {

      public ArraySlice(TItem[] source, int start, int length) : base(source, start, length) {
#if NETFX_4
        Contract.Requires(source != null);
#endif
      }

      public new TItem this[int index] {
        get {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          return (this._source[index + this._start]);
        }
        set {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          this._source[index + this._start] = value;
        }
      }
    }
    #endregion

    private static readonly Exception _noElements = new InvalidOperationException("No Elements!");

    /// <summary>
    /// Slices the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <returns>An array slice which accesses the underlying array but can only be read.</returns>
    public static ReadOnlyArraySlice<TItem> ReadOnlySlice<TItem>(this TItem[] @this, int start, int length) => new ReadOnlyArraySlice<TItem>(@this, start, length);

    /// <summary>
    /// Slices the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <returns>An array slice which accesses the underlying array.</returns>
    public static ArraySlice<TItem> Slice<TItem>(this TItem[] @this, int start, int length) => new ArraySlice<TItem>(@this, start, length);

    /// <summary>
    /// Gets a random element.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="This">This Array.</param>
    /// <param name="random">The random number generator, if any.</param>
    /// <returns>A random element from the array.</returns>
    public static TValue GetRandomElement<TValue>(this TValue[] This, Random random = null) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var length = This.Length;
      if (length == 0)
        throw _noElements;

      if (random == null)
        random = new Random();

      var index = random.Next(length);
      return (This[index]);
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(startIndex + count <= This.Length);
      Contract.Requires(startIndex >= 0);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] This, Converter<TInput, TOutput> converter) => Array.ConvertAll(This, converter);

    /// <summary>
    /// Converts all elements.
    /// </summary>
    /// <typeparam name="TInput">The type the elements.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns>An array containing the converted values.</returns>
    public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] This, Func<TInput, int, TOutput> converter) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
#endif
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
    public static void ForEach<TInput>(this TInput[] This, Action<TInput> action) => Array.ForEach(This, action);

    /// <summary>
    /// Executes a callback with each element in an array in parallel.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback to execute for each element.</param>
    public static void ParallelForEach<TInput>(this TInput[] This, Action<TInput> action) => Parallel.ForEach(This, action);

    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TInput>(this TInput[] This, Action<TInput, int> action) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(action != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(worker != null);
#endif
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
    public static bool Exists<TInput>(this TInput[] This, Predicate<TInput> predicate) => Array.Exists(This, predicate);

    /// <summary>
    /// Gets the reverse.
    /// </summary>
    /// <typeparam name="TInput">The type of the input array.</typeparam>
    /// <param name="This">This array.</param>
    /// <returns>An array where all values are inverted.</returns>
#if NETFX_4
    [Pure]
#endif
    public static TInput[] Reverse<TInput>(this TInput[] This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
    [Pure]
#endif
    public static bool Contains<TItem>(this TItem[] This, TItem value) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
    [Pure]
#endif
    public static bool Contains(this Array This, object value) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
    [Pure]
#endif
    public static object[] ToArray(this Array This) {
#if NETFX_4
      Contract.Requires(This != null && This.Rank > 0);
#endif
      var result = new object[This.Length];
      var lbound = This.GetLowerBound(0);
      for (var i = This.Length; i > 0;) {
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
    public static int IndexOf<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
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
#if NETFX_4
    [Pure]
#endif
    public static int IndexOf<TItem>(this TItem[] This, TItem value) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
    public static int IndexOf(this Array This, Predicate<object> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      for (var i = This.GetLowerBound(0); i <= This.GetUpperBound(0); i++)
        if (predicate(This.GetValue(i)))
          return (i);
      return (-1);
    }

    #region high performance linq for arrays
    public static bool Any<TItem>(this TItem[] This) => This.Length > 0;

    public static bool Any<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        if (predicate(This[i]))
          return (true);
      return (false);
    }
    public static TItem First<TItem>(this TItem[] This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var length = This.Length;
      if (length == 0)
        throw _noElements;
      return (This[0]);
    }
    public static TItem Last<TItem>(this TItem[] This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      return (This[length - 1]);
    }
    public static TItem First<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      throw _noElements;
    }
    public static TItem Last<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = length - 1; i >= 0; i--) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      throw _noElements;
    }
    public static TItem FirstOrDefault<TItem>(this TItem[] This) => This.Length == 0 ? default(TItem) : This[0];

    public static TItem LastOrDefault<TItem>(this TItem[] This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var length = This.LongLength;
      return length == 0 ? default(TItem) : This[length - 1];
    }
    public static TItem FirstOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      return (default(TItem));
    }
    public static TItem LastOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = length - 1; i >= 0; i--) {
        var current = This[i];
        if (predicate(current))
          return (current);
      }
      return (default(TItem));
    }
    public static TItem Aggregate<TItem>(this TItem[] This, Func<TItem, TItem, TItem> func) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(func != null);
#endif
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      var result = This[0];
      for (var i = 1; i < length; i++)
        result = func(result, This[i]);
      return (result);
    }
    public static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] This, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(func != null);
#endif
      var length = This.LongLength;
      if (length == 0)
        throw _noElements;
      var result = seed;
      for (var i = 0; i < length; i++)
        result = func(result, This[i]);
      return (result);
    }
    public static int Count<TItem>(this TItem[] This) => This.Length;

    public static long LongCount<TItem>(this TItem[] This) => This.LongLength;

    public static int Count<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var result = (long)0;
      for (var i = 0; i < This.LongLength; i++)
        if (predicate(This[i]))
          result++;
      return (result);
    }


    public static TItem FirstOrDefault<TItem>(this TItem[] This, Predicate<TItem> predicate, TItem defaultValue) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      foreach (var item in This.Where(predicate))
        return (item);
      return (defaultValue);
    }

    #region these special Array ...s
    public static IEnumerable<TResult> OfType<TResult>(this Array This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      for (var i = 0; i < This.LongLength; i++) {
        var item = This.GetValue(i);
        if (item is TResult)
          yield return (TResult)item;
      }
    }

    public static object FirstOrDefault(this Array This, Predicate<object> predicate, object defaultValue = null) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      foreach (var item in This) {
        if (predicate((TItem)item))
          return ((TItem)item);
      }
      return (defaultValue);
    }

    public static IEnumerable<TResult> Cast<TResult>(this Array This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      for (var i = This.GetLowerBound(0); i <= This.GetUpperBound(0); i++) {
        var value = This.GetValue(i);
        yield return value == null ? default(TResult) : (TResult)value;
      }
    }
    #endregion

    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] This, Func<TItem, TResult> selector) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      var length = This.Length;
      for (var i = 0; i < length; i++)
        yield return selector(This[i]);
    }
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] This, Func<TItem, TResult> selector) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        yield return selector(This[i]);
    }
    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] This, Func<TItem, int, TResult> selector) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      var length = This.Length;
      for (var i = 0; i < length; i++)
        yield return selector(This[i], i);
    }
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] This, Func<TItem, long, TResult> selector) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++)
        yield return selector(This[i], i);
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Predicate<TItem> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.LongLength;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current))
          yield return current;
      }
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Func<TItem, int, bool> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
      var length = This.Length;
      for (var i = 0; i < length; i++) {
        var current = This[i];
        if (predicate(current, i))
          yield return current;
      }
    }
    public static IEnumerable<TItem> Where<TItem>(this TItem[] This, Func<TItem, long, bool> predicate) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(predicate != null);
#endif
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
    /// Copies the given buffer.
    /// </summary>
    /// <param name="This">This byte[].</param>
    /// <returns>A copy of the original array.</returns>
    public static byte[] Copy(this byte[] This) {
      if (This == null)
        return (null);

      var length = This.Length;
      var result = new byte[length];
      Buffer.BlockCopy(This, 0, result, 0, length);
      return (result);
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
    /// Padds the specified byte array to a certain length if it is smaller.
    /// </summary>
    /// <param name="This">This Byte-Array.</param>
    /// <param name="length">The final length.</param>
    /// <param name="data">The data to use for padding, default to null-bytes.</param>
    /// <returns>The original array if it already exceeds the wanted size, or an array with the correct size.</returns>
    public static byte[] Padd(this byte[] This, int length, byte data = 0) {
      if (This == null)
        return (null);

      var currentSize = This.Length;
      if (currentSize >= length)
        return (This);

      var result = new byte[length];
      Buffer.BlockCopy(This, 0, result, 0, currentSize);
      for (var i = currentSize; i < length; ++i)
        result[i] = data;

      return (result);
    }

    /// <summary>
    /// GZips the given bytes.
    /// </summary>
    /// <param name="This">This Byte-Array.</param>
    /// <returns>A GZipped byte array.</returns>
    public static byte[] GZip(this byte[] This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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

    #region hash computation
    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The result of the hash algorithm</returns>
    public static byte[] ComputeHash<THashAlgorithm>(this byte[] @this) where THashAlgorithm : HashAlgorithm, new() {
#if NETFX_4
      Contract.Requires(@this != null);
#endif

      using (var provider = new THashAlgorithm())
        return (provider.ComputeHash(@this));
    }

    /// <summary>
    /// Calculates the SHA512 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA512Hash(this byte[] @this) => @this.ComputeHash<SHA512CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA384 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA384Hash(this byte[] @this) => @this.ComputeHash<SHA384CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA256 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA256Hash(this byte[] @this) => @this.ComputeHash<SHA256CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA-1 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeSHA1Hash(this byte[] @this) => @this.ComputeHash<SHA1CryptoServiceProvider>();

    /// <summary>
    /// Calculates the MD5 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
    public static byte[] ComputeMD5Hash(this byte[] @this) => @this.ComputeHash<MD5CryptoServiceProvider>();

    #endregion

    #region utility classes
    private static class RuntimeConfiguration {
      public static readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

      public static bool Has16BitRegisters => (IntPtr.Size >= 2);

      public static bool Has32BitRegisters => (IntPtr.Size >= 4);

      public static bool Has64BitRegisters => (IntPtr.Size >= 8);

      public const int MIN_ITEMS_FOR_PARALELLISM = 2048;
      public const int MIN_ITEMS_PER_THREAD = 128;

      public const int DEFAULT_MAX_CHUNK_SIZE = 1024 * 64;

      public const int ALLOCATION_WORD = 128;
      public const int ALLOCATION_DWORD = 256;
      public const int ALLOCATION_QWORD = 512;

      public const int BLOCKCOPY_WORD = 2;
      public const int BLOCKCOPY_DWORD = 4;
      public const int BLOCKCOPY_QWORD = 8;
    }

    private static class FastXor {
      private static void _DoBytes(byte[] source, byte[] operand, int offset, int length) {
        var end = offset + length;
        for (var i = offset; i < end; ++i)
          source[i] ^= operand[i];
      }

      private static void _DoWords(ushort[] source, ushort[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] ^= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] ^= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] ^= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      public static void ProcessBytewise(byte[] source, byte[] operand) {
        _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
      }

#if UNSAFE
      public static unsafe void ProcessInUnsafeChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        fixed (byte* srcPointer = source, opPointer = operand) {

          if (RuntimeConfiguration.Has64BitRegisters) {
            var sourcePtr = (ulong*)(srcPointer + offset);
            var operandPtr = (ulong*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {
              *sourcePtr ^= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 8;
              offset += 8;
            }
          }
          if (RuntimeConfiguration.Has32BitRegisters) {
            var sourcePtr = (uint*)(srcPointer + offset);
            var operandPtr = (uint*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {
              *sourcePtr ^= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 4;
              offset += 4;
            }
          }
          if (RuntimeConfiguration.Has16BitRegisters) {
            var sourcePtr = (ushort*)(srcPointer + offset);
            var operandPtr = (ushort*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {
              *sourcePtr ^= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 2;
              offset += 2;
            }
          }
          {
            var sourcePtr = srcPointer + offset;
            var operandPtr = opPointer + offset;
            while (bytesLeft > 0) {
              *sourcePtr ^= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft--;
              offset++;
            }
          }
        }
      }
#endif

      public static void ProcessInChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        // long part
        if (RuntimeConfiguration.Has64BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_QWORD) {

          var chunk = new ulong[maxChunkSize >> 3];
          var secondChunk = new ulong[maxChunkSize >> 3];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 3;
            chunkLength = itemCount << 3;

            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoQWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }

        }

        // int part
        if (RuntimeConfiguration.Has32BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_DWORD) {
          var chunk = new uint[maxChunkSize >> 2];
          var secondChunk = new uint[maxChunkSize >> 2];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 2;
            chunkLength = itemCount << 2;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoDWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // short part
        if (RuntimeConfiguration.Has16BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_WORD) {
          var chunk = new ushort[maxChunkSize >> 1];
          var secondChunk = new ushort[maxChunkSize >> 1];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 1;
            chunkLength = itemCount << 1;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // remaining bytes
        if (bytesLeft > 0)
          _DoBytes(source, operand, offset, bytesLeft);

      }
    }

    private static class FastAnd {
      private static void _DoBytes(byte[] source, byte[] operand, int offset, int length) {
        var end = offset + length;
        for (var i = offset; i < end; ++i)
          source[i] &= operand[i];
      }

      private static void _DoWords(ushort[] source, ushort[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] &= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] &= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] &= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      public static void ProcessBytewise(byte[] source, byte[] operand) {
        _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
      }

#if UNSAFE
      public static unsafe void ProcessInUnsafeChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        fixed (byte* srcPointer = source, opPointer = operand) {

          if (RuntimeConfiguration.Has64BitRegisters) {
            var sourcePtr = (ulong*)(srcPointer + offset);
            var operandPtr = (ulong*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {
              *sourcePtr &= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 8;
              offset += 8;
            }
          }
          if (RuntimeConfiguration.Has32BitRegisters) {
            var sourcePtr = (uint*)(srcPointer + offset);
            var operandPtr = (uint*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {
              *sourcePtr &= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 4;
              offset += 4;
            }
          }
          if (RuntimeConfiguration.Has16BitRegisters) {
            var sourcePtr = (ushort*)(srcPointer + offset);
            var operandPtr = (ushort*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {
              *sourcePtr &= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 2;
              offset += 2;
            }
          }
          {
            var sourcePtr = srcPointer + offset;
            var operandPtr = opPointer + offset;
            while (bytesLeft > 0) {
              *sourcePtr &= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft--;
              offset++;
            }
          }
        }
      }
#endif

      public static void ProcessInChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        // long part
        if (RuntimeConfiguration.Has64BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_QWORD) {

          var chunk = new ulong[maxChunkSize >> 3];
          var secondChunk = new ulong[maxChunkSize >> 3];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 3;
            chunkLength = itemCount << 3;

            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoQWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }

        }

        // int part
        if (RuntimeConfiguration.Has32BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_DWORD) {
          var chunk = new uint[maxChunkSize >> 2];
          var secondChunk = new uint[maxChunkSize >> 2];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 2;
            chunkLength = itemCount << 2;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoDWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // short part
        if (RuntimeConfiguration.Has16BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_WORD) {
          var chunk = new ushort[maxChunkSize >> 1];
          var secondChunk = new ushort[maxChunkSize >> 1];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 1;
            chunkLength = itemCount << 1;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // remaining bytes
        if (bytesLeft > 0)
          _DoBytes(source, operand, offset, bytesLeft);

      }
    }

    private static class FastOr {
      private static void _DoBytes(byte[] source, byte[] operand, int offset, int length) {
        var end = offset + length;
        for (var i = offset; i < end; ++i)
          source[i] |= operand[i];
      }

      private static void _DoWords(ushort[] source, ushort[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] |= operand[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      public static void ProcessBytewise(byte[] source, byte[] operand) {
        _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
      }

#if UNSAFE
      public static unsafe void ProcessInUnsafeChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        fixed (byte* srcPointer = source, opPointer = operand) {

          if (RuntimeConfiguration.Has64BitRegisters) {
            var sourcePtr = (ulong*)(srcPointer + offset);
            var operandPtr = (ulong*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {
              *sourcePtr |= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 8;
              offset += 8;
            }
          }
          if (RuntimeConfiguration.Has32BitRegisters) {
            var sourcePtr = (uint*)(srcPointer + offset);
            var operandPtr = (uint*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {
              *sourcePtr |= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 4;
              offset += 4;
            }
          }
          if (RuntimeConfiguration.Has16BitRegisters) {
            var sourcePtr = (ushort*)(srcPointer + offset);
            var operandPtr = (ushort*)(opPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {
              *sourcePtr |= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft -= 2;
              offset += 2;
            }
          }
          {
            var sourcePtr = srcPointer + offset;
            var operandPtr = opPointer + offset;
            while (bytesLeft > 0) {
              *sourcePtr |= *operandPtr;
              sourcePtr++;
              operandPtr++;
              bytesLeft--;
              offset++;
            }
          }
        }
      }
#endif

      public static void ProcessInChunks(byte[] source, byte[] operand, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, operand, 0, Math.Min(source.Length, operand.Length));
          return;
        }

        var bytesLeft = Math.Min(source.Length, operand.Length);
        var offset = 0;

        // long part
        if (RuntimeConfiguration.Has64BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_QWORD) {

          var chunk = new ulong[maxChunkSize >> 3];
          var secondChunk = new ulong[maxChunkSize >> 3];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 3;
            chunkLength = itemCount << 3;

            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoQWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }

        }

        // int part
        if (RuntimeConfiguration.Has32BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_DWORD) {
          var chunk = new uint[maxChunkSize >> 2];
          var secondChunk = new uint[maxChunkSize >> 2];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 2;
            chunkLength = itemCount << 2;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoDWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // short part
        if (RuntimeConfiguration.Has16BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_WORD) {
          var chunk = new ushort[maxChunkSize >> 1];
          var secondChunk = new ushort[maxChunkSize >> 1];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 1;
            chunkLength = itemCount << 1;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            Buffer.BlockCopy(operand, offset, secondChunk, 0, chunkLength);
            _DoWords(chunk, secondChunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // remaining bytes
        if (bytesLeft > 0)
          _DoBytes(source, operand, offset, bytesLeft);

      }
    }

    private static class FastNot {
      private static void _DoBytes(byte[] source, int offset, int length) {
        var end = offset + length;
        for (var i = offset; i < end; ++i)
          source[i] ^= 0xff;
      }

      private static void _DoWords(ushort[] source) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] ^= 0xffff;

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] ^= 0xffff;
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoDWords(uint[] source) {
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] = ~source[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      private static void _DoQWords(ulong[] source) {

        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];

          return;
        }

        var maxDegree = Math.Min(RuntimeConfiguration.MaxDegreeOfParallelism, source.Length / RuntimeConfiguration.MIN_ITEMS_PER_THREAD);
        var index = 0;
        var o = new object();
        Action action = () => {
          int start;
          lock (o)
            start = index++;
          for (var i = start; i < source.Length; i += maxDegree)
            source[i] = ~source[i];
        };

        var actions = new Action[maxDegree];
        for (var i = maxDegree - 1; i >= 0; --i)
          actions[i] = action;

        Parallel.Invoke(actions);
      }

      public static void ProcessBytewise(byte[] source) {
        _DoBytes(source, 0, source.Length);
      }

#if UNSAFE
      public static unsafe void ProcessInUnsafeChunks(byte[] source, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, 0, source.Length);
          return;
        }

        var bytesLeft = source.Length;
        var offset = 0;

        fixed (byte* srcPointer = source) {

          if (RuntimeConfiguration.Has64BitRegisters) {
            var sourcePtr = (ulong*)(srcPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {
              *sourcePtr = ~*sourcePtr;
              sourcePtr++;
              bytesLeft -= 8;
              offset += 8;
            }
          }
          if (RuntimeConfiguration.Has32BitRegisters) {
            var sourcePtr = (uint*)(srcPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {
              *sourcePtr = ~*sourcePtr;
              sourcePtr++;
              bytesLeft -= 4;
              offset += 4;
            }
          }
          if (RuntimeConfiguration.Has16BitRegisters) {
            var sourcePtr = (ushort*)(srcPointer + offset);
            while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {
              *sourcePtr ^= 0xffff;
              sourcePtr++;
              bytesLeft -= 2;
              offset += 2;
            }
          }
          {
            var sourcePtr = srcPointer + offset;
            while (bytesLeft > 0) {
              *sourcePtr ^= 0xff;
              sourcePtr++;
              bytesLeft--;
              offset++;
            }
          }
        }
      }
#endif

      public static void ProcessInChunks(byte[] source, int maxChunkSize = -1) {
        if (maxChunkSize < 1)
          maxChunkSize = RuntimeConfiguration.DEFAULT_MAX_CHUNK_SIZE;

        if (maxChunkSize < 2) {
          _DoBytes(source, 0, source.Length);
          return;
        }

        var bytesLeft = source.Length;
        var offset = 0;

        // long part
        if (RuntimeConfiguration.Has64BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_QWORD) {

          var chunk = new ulong[maxChunkSize >> 3];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_QWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 3;
            chunkLength = itemCount << 3;

            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoQWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }

        }

        // int part
        if (RuntimeConfiguration.Has32BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_DWORD) {
          var chunk = new uint[maxChunkSize >> 2];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_DWORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 2;
            chunkLength = itemCount << 2;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoDWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // short part
        if (RuntimeConfiguration.Has16BitRegisters && bytesLeft > RuntimeConfiguration.ALLOCATION_WORD) {
          var chunk = new ushort[maxChunkSize >> 1];

          while (bytesLeft > RuntimeConfiguration.BLOCKCOPY_WORD) {

            var chunkLength = Math.Min(bytesLeft, maxChunkSize);
            var itemCount = chunkLength >> 1;
            chunkLength = itemCount << 1;


            Buffer.BlockCopy(source, offset, chunk, 0, chunkLength);
            _DoWords(chunk);
            Buffer.BlockCopy(chunk, 0, source, offset, chunkLength);

            bytesLeft -= chunkLength;
            offset += chunkLength;
          }
        }

        // remaining bytes
        if (bytesLeft > 0)
          _DoBytes(source, offset, bytesLeft);

      }
    }
    #endregion

    public static void Xor(this byte[] This, byte[] operand) {
#if UNSAFE
      FastXor.ProcessInUnsafeChunks(This, operand);
#else
      FastXor.ProcessInChunks(This, operand);
#endif
    }

    public static void XorBytewise(this byte[] This, byte[] operand) {
      FastXor.ProcessBytewise(This, operand);
    }

    public static void And(this byte[] This, byte[] operand) {
#if UNSAFE
      FastAnd.ProcessInUnsafeChunks(This, operand);
#else
      FastAnd.ProcessInChunks(This, operand);
#endif
    }

    public static void AndBytewise(this byte[] This, byte[] operand) {
      FastAnd.ProcessBytewise(This, operand);
    }

    public static void Or(this byte[] This, byte[] operand) {
#if UNSAFE
      FastOr.ProcessInUnsafeChunks(This, operand);
#else
      FastOr.ProcessInChunks(This, operand);
#endif
    }

    public static void OrBytewise(this byte[] This, byte[] operand) {
      FastOr.ProcessBytewise(This, operand);
    }

    public static void Not(this byte[] This) {
#if UNSAFE
      FastNot.ProcessInUnsafeChunks(This);
#else
      FastNot.ProcessInChunks(This);
#endif
    }

    public static void NotBytewise(this byte[] This) {
      FastNot.ProcessBytewise(This);
    }

    #endregion

  }
}