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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
#if NETFX_4
using System.Threading.Tasks;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class ArrayExtensions {

    #region nested types

    internal enum ChangeType {
      Equal = 0,
      Changed = 1,
      Added = 2,
      Removed = 3,
    }

    internal interface IChangeSet<out TItem> {
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


    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
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

      /// <summary>
      /// Gets the number of elements in this slice.
      /// </summary>
      /// <value>
      /// The length.
      /// </value>
      public int Length { get; }

      /// <summary>
      /// Gets the <see cref="TItem"/> at the specified index.
      /// </summary>
      /// <value>
      /// The <see cref="TItem"/>.
      /// </value>
      /// <param name="index">The index.</param>
      /// <returns>The item at the given index.</returns>
      public TItem this[int index] {
        get {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          return this._source[index + this._start];
        }
      }

      /// <summary>
      /// Gets the values.
      /// </summary>
      /// <value>
      /// The values.
      /// </value>
      public IEnumerable<TItem> Values {
        get {
          var maxIndex = this._start + this.Length;
          for (var i = this._start; i < maxIndex; ++i)
            yield return this._source[i];
        }
      }

      /// <summary>
      /// Slices the specified array.
      /// </summary>
      /// <typeparam name="TItem">The type of the items.</typeparam>
      /// <param name="start">The start.</param>
      /// <param name="length">The length; negative values mean: till the end.</param>
      /// <returns>An array slice which accesses the underlying array but can only be read.</returns>
      public ReadOnlyArraySlice<TItem> ReadOnlySlice(int start, int length = -1) {
        if (length < 0)
          length = this.Length - start;

        if (start + length > this.Length)
          throw new ArgumentException("Exceeding source length", nameof(length));

        return new ReadOnlyArraySlice<TItem>(this._source, start + this._start, length);
      }

      /// <summary>
      /// Copies this slice into a new array.
      /// </summary>
      /// <returns></returns>
      public TItem[] ToArray() {
        var result = new TItem[this.Length];
        if (typeof(TItem) == typeof(byte))
          Buffer.BlockCopy(this._source, this._start, result, 0, this.Length);
        else
          Array.Copy(this._source, this._start, result, 0, this.Length);
        return result;
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

    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    internal class ArraySlice<TItem> : ReadOnlyArraySlice<TItem> {

      public ArraySlice(TItem[] source, int start, int length) : base(source, start, length) {
#if NETFX_4
        Contract.Requires(source != null);
#endif
      }

      /// <summary>
      /// Gets or sets the <see cref="TItem" /> at the specified index.
      /// </summary>
      /// <value>
      /// The <see cref="TItem" />.
      /// </value>
      /// <param name="index">The index.</param>
      /// <returns>The item at the given index</returns>
      public new TItem this[int index] {
        get {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          return this._source[index + this._start];
        }
        set {
#if NETFX_4
          Contract.Requires(index < this.Length);
#endif
          this._source[index + this._start] = value;
        }
      }

      /// <summary>
      /// Slices the specified array.
      /// </summary>
      /// <typeparam name="TItem">The type of the items.</typeparam>
      /// <param name="start">The start.</param>
      /// <param name="length">The length; negative values mean: till the end.</param>
      /// <returns>An array slice which accesses the underlying array.</returns>
      public ArraySlice<TItem> Slice(int start, int length = -1) {
        if (length < 0)
          length = this.Length - start;

        if (start + length > this.Length)
          throw new ArgumentException("Exceeding source length", nameof(length));

        return new ArraySlice<TItem>(this._source, start + this._start, length);
      }
    }

    #endregion


    /// <summary>
    /// Compares two arrays against each other.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="other">The other Array.</param>
    /// <param name="comparer">The value comparer; optional: uses default.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this TItem[] @this, TItem[] other, IEqualityComparer<TItem> comparer = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (other == null)
        throw new ArgumentNullException(nameof(other));

#if NETFX_4
      Contract.EndContractBlock();
#endif

      if (comparer == null)
        comparer = EqualityComparer<TItem>.Default;

      var targetIndex = 0;
      var currentSourceBuffer = new Queue<int>();

      for (var i = 0; i < @this.Length; ++i) {
        var item = @this[i];
        var foundAt = _IndexOf(other, item, targetIndex, comparer);
        if (foundAt < 0) {
          // does not exist in target
          currentSourceBuffer.Enqueue(i);
          continue;
        }

        // found
        while (targetIndex <= foundAt) {
          if (targetIndex == foundAt) {
            // last iteration
            while (currentSourceBuffer.Count > 0) {
              var index = currentSourceBuffer.Dequeue();
              yield return new ChangeSet<TItem>(ChangeType.Added, index, @this[index], -1, default(TItem));
            }

            yield return new ChangeSet<TItem>(ChangeType.Equal, i, @this[i], targetIndex, other[targetIndex]);
          } else {
            if (currentSourceBuffer.Count > 0) {
              var index = currentSourceBuffer.Dequeue();
              yield return new ChangeSet<TItem>(ChangeType.Changed, index, @this[index], targetIndex, other[targetIndex]);
            } else
              yield return new ChangeSet<TItem>(ChangeType.Removed, -1, default(TItem), targetIndex, other[targetIndex]);
          }
          ++targetIndex;
        }
      }

      var targetLen = other.Length;
      while (currentSourceBuffer.Count > 0) {
        if (targetIndex < targetLen) {
          var index = currentSourceBuffer.Dequeue();
          yield return new ChangeSet<TItem>(ChangeType.Changed, index, @this[index], targetIndex, other[targetIndex]);
          ++targetIndex;
        } else {
          var index = currentSourceBuffer.Dequeue();
          yield return new ChangeSet<TItem>(ChangeType.Added, index, @this[index], -1, default(TItem));
        }
      }

      while (targetIndex < targetLen) {
        yield return new ChangeSet<TItem>(ChangeType.Removed, -1, default(TItem), targetIndex, other[targetIndex]);
        ++targetIndex;
      }
    }

    private static int _IndexOf<TValue>(TValue[] values, TValue item, int startIndex, IEqualityComparer<TValue> comparer) {
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
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem[] ToNullIfEmpty<TItem>(this TItem[] @this) => @this?.Length > 0 ? @this : null;

    /// <summary>
    /// Slices the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length; negative values mean: till the end.</param>
    /// <returns>An array slice which accesses the underlying array but can only be read.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static ReadOnlyArraySlice<TItem> ReadOnlySlice<TItem>(this TItem[] @this, int start, int length = -1) => new ReadOnlyArraySlice<TItem>(@this, start, length < 0 ? @this.Length - start : length);

    /// <summary>
    /// Slices the specified array for read-only access.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="size">The size of the slices.</param>
    /// <returns>
    /// An enumeration of read-only slices
    /// </returns>
    [DebuggerStepThrough]
    public static IEnumerable<ReadOnlyArraySlice<TItem>> ReadOnlySlices<TItem>(this TItem[] @this, int size) {
      if (@this == null)
        throw new NullReferenceException();
      if (size < 1)
        throw new ArgumentOutOfRangeException(nameof(size), size, "Must be > 0");
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      for (var index = 0; index < length; index += size)
        yield return new ReadOnlyArraySlice<TItem>(@this, index, Math.Min(length - index, size));
    }

    /// <summary>
    /// Slices the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length; negative values mean: till the end.</param>
    /// <returns>An array slice which accesses the underlying array.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static ArraySlice<TItem> Slice<TItem>(this TItem[] @this, int start, int length = -1) => new ArraySlice<TItem>(@this, start, length < 0 ? @this.Length - start : length);

    /// <summary>
    /// Slices the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="size">The size of the slices.</param>
    /// <returns>An enumeration of slices</returns>
    [DebuggerStepThrough]
    public static IEnumerable<ArraySlice<TItem>> Slices<TItem>(this TItem[] @this, int size) {
      if (@this == null)
        throw new NullReferenceException();
      if (size < 1)
        throw new ArgumentOutOfRangeException(nameof(size), size, "Must be > 0");
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      for (var index = 0; index < length; index += size)
        yield return new ArraySlice<TItem>(@this, index, Math.Min(length - index, size));
    }

    /// <summary>
    /// Gets a random element.
    /// </summary>
    /// <typeparam name="TItem">The type of the values.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="random">The random number generator, if any.</param>
    /// <returns>
    /// A random element from the array.
    /// </returns>
    [DebuggerStepThrough]
    public static TItem GetRandomElement<TItem>(this TItem[] @this, Random random = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (@this.Length == 0)
        throw new InvalidOperationException("No Elements!");

#if NETFX_4
      Contract.EndContractBlock();
#endif

      if (random == null)
        random = new Random((int)Stopwatch.GetTimestamp());

      var index = random.Next(@this.Length);
      return @this[index];
    }

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="index">The index.</param>
    /// <returns></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index) => @this.Length <= index ? default(TItem) : @this[index];

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, TItem defaultValue) => @this.Length <= index ? defaultValue : @this[index];

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="index">The index.</param>
    /// <param name="factory">The factory to create default values.</param>
    /// <returns></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<TItem> factory) => @this.Length <= index ? factory() : @this[index];

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="index">The index.</param>
    /// <param name="factory">The factory to create default values.</param>
    /// <returns></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<int, TItem> factory) => @this.Length <= index ? factory(index) : @this[index];

    /// <summary>
    /// Clones the specified array.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This Array.</param>
    /// <returns>A new array or <c>null</c> if this array was <c>null</c>.</returns>
    [DebuggerStepThrough]
    public static TItem[] SafelyClone<TItem>(this TItem[] @this) {
      if (@this == null)
        return null;

      var len = @this.Length;
      var result = new TItem[len];
      if (len > 0)
        Array.Copy(@this, result, len);

      return result;
    }

    /// <summary>
    /// Joins the specified elements into a string.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This enumeration.</param>
    /// <param name="join">The delimiter.</param>
    /// <param name="skipDefaults">if set to <c>true</c> all default values will be skipped.</param>
    /// <param name="converter">The converter.</param>
    /// <returns>The joines string.</returns>
    [DebuggerStepThrough]
    public static string Join<TItem>(this TItem[] @this, string join = ", ", bool skipDefaults = false, Func<TItem, string> converter = null) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var result = new StringBuilder();
      var gotElements = false;
      var defaultValue = default(TItem);

      // ReSharper disable ForCanBeConvertedToForeach
      for (var i = 0; i < @this.Length; i++) {
        var item = @this[i];
        if (skipDefaults && (ReferenceEquals(item, defaultValue) || EqualityComparer<TItem>.Default.Equals(item, defaultValue)))
          continue;

        if (gotElements)
          result.Append(join);
        else
          gotElements = true;

        result.Append(converter == null ? ReferenceEquals(null, item) ? string.Empty : item.ToString() : converter(item));
      }
      // ReSharper restore ForCanBeConvertedToForeach

      return gotElements ? result.ToString() : null;
    }

    /// <summary>
    /// Splices the specified array (returns part of that array).
    /// </summary>
    /// <typeparam name="TItem">Type of data in the array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="startIndex">The start element which should be included in the splice.</param>
    /// <param name="count">The number of elements from there on.</param>
    /// <returns></returns>
    public static TItem[] Range<TItem>(this TItem[] @this, int startIndex, int count) {
#if NETFX_4
      Contract.Requires(@this != null);
      Contract.Requires(startIndex + count <= @this.Length);
      Contract.Requires(startIndex >= 0);
#endif
      var result = new TItem[count];
      Array.Copy(@this, startIndex, result, 0, count);
      return result;
    }
    /// <summary>
    /// Swaps the specified data in an array.
    /// </summary>
    /// <typeparam name="TItem">Type of data in the array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="firstElementIndex">The first value.</param>
    /// <param name="secondElementIndex">The the second value.</param>
    public static void Swap<TItem>(this TItem[] @this, int firstElementIndex, int secondElementIndex) {
#if NETFX_4
      Contract.Requires(@this != null);
#endif
      var value = @this[firstElementIndex];
      @this[firstElementIndex] = @this[secondElementIndex];
      @this[secondElementIndex] = value;
    }
    /// <summary>
    /// Shuffles the specified data.
    /// </summary>
    /// <typeparam name="TItem">Type of elements in the array.</typeparam>
    /// <param name="this">This array.</param>
    public static void Shuffle<TItem>(this TItem[] @this) {
#if NETFX_4
      Contract.Requires(@this != null);
#endif
      var index = @this.Length;
      var random = new Random();
      while (index > 1)
        @this.Swap(random.Next(index), --index);
    }
    /// <summary>
    /// Quick-sort the given array.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements</typeparam>
    /// <param name="this">This array.</param>
    /// <returns>A sorted array copy.</returns>
    public static TItem[] QuickSorted<TItem>(this TItem[] @this) where TItem : IComparable<TItem> {
#if NETFX_4
      Contract.Requires(@this != null);
#endif
      var result = new TItem[@this.Length];
      @this.CopyTo(result, 0);
      result.QuickSort();
      return result;
    }
    /// <summary>
    /// Quick-sort the given array.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements.</typeparam>
    /// <param name="this">This array.</param>
    public static void QuickSort<TItem>(this TItem[] @this) where TItem : IComparable<TItem> {
#if NETFX_4
      Contract.Requires(@this != null);
#endif
      if (@this.Length > 0)
        QuickSort_Comparable(@this, 0, @this.Length - 1);
    }
    private static void QuickSort_Comparable<TValue>(TValue[] @this, int left, int right) where TValue : IComparable<TValue> {
      if (@this == null) {
        // nothing to sort
      } else {
        var leftIndex = left;
        var rightIndex = right;
        var pivotItem = @this[(leftIndex + rightIndex) >> 1];
        while (leftIndex <= rightIndex) {
          while (Comparer<TValue>.Default.Compare(pivotItem, @this[leftIndex]) > 0)
            leftIndex++;
          while (Comparer<TValue>.Default.Compare(pivotItem, @this[rightIndex]) < 0)
            rightIndex--;
          if (leftIndex > rightIndex) {
            continue;
          }
          @this.Swap(leftIndex, rightIndex);
          leftIndex++;
          rightIndex--;
        }
        if (left < rightIndex)
          QuickSort_Comparable(@this, left, rightIndex);
        if (leftIndex < right)
          QuickSort_Comparable(@this, leftIndex, right);
      }
    }

    /// <summary>
    /// Converts all elements.
    /// </summary>
    /// <typeparam name="TItem">The type the elements.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns>An array containing the converted values.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Converter<TItem, TOutput> converter) => Array.ConvertAll(@this, converter);

    /// <summary>
    /// Converts all elements.
    /// </summary>
    /// <typeparam name="TItem">The type the elements.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="converter">The converter function.</param>
    /// <returns>An array containing the converted values.</returns>
    public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Func<TItem, int, TOutput> converter) {
#if NETFX_4
      Contract.Requires(@this != null);
      Contract.Requires(converter != null);
#endif
      var length = @this.Length;
      var result = new TOutput[length];
      for (var index = length - 1; index >= 0; index--)
        result[index] = converter(@this[index], index);
      return result;
    }
    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="action">The callback for each element.</param>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void ForEach<TItem>(this TItem[] @this, Action<TItem> action) => Array.ForEach(@this, action);

#if NETFX_4
    /// <summary>
    /// Executes a callback with each element in an array in parallel.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="action">The callback to execute for each element.</param>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void ParallelForEach<TItem>(this TItem[] @this, Action<TItem> action) => Parallel.ForEach(@this, action);

#endif

    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TItem>(this TItem[] @this, Action<TItem, int> action) {
      if (@this == null)
        throw new NullReferenceException();
      if (action == null)
        throw new ArgumentNullException(nameof(action));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var intI = @this.Length - 1; intI >= 0; intI--)
        action(@this[intI], intI);
    }
    /// <summary>
    /// Executes a callback with each element in an array.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="action">The callback for each element.</param>
    public static void ForEach<TItem>(this TItem[] @this, Action<TItem, long> action) {
      if (@this == null)
        throw new NullReferenceException();
      if (action == null)
        throw new ArgumentNullException(nameof(action));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var intI = @this.LongLength - 1; intI >= 0; intI--)
        action(@this[intI], intI);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TItem>(this TItem[] @this, Func<TItem, TItem> worker) {
      if (@this == null)
        throw new NullReferenceException();
      if (worker == null)
        throw new ArgumentNullException(nameof(worker));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var index = @this.LongLength - 1; index >= 0; index--)
        @this[index] = worker(@this[index]);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TItem>(this TItem[] @this, Func<TItem, int, TItem> worker) {
      if (@this == null)
        throw new NullReferenceException();
      if (worker == null)
        throw new ArgumentNullException(nameof(worker));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var index = @this.Length - 1; index >= 0; index--)
        @this[index] = worker(@this[index], index);
    }
    /// <summary>
    /// Executes a callback with each element in an array and writes back the result.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="worker">The callback for each element.</param>
    public static void ForEach<TItem>(this TItem[] @this, Func<TItem, long, TItem> worker) {
      if (@this == null)
        throw new NullReferenceException();
      if (worker == null)
        throw new ArgumentNullException(nameof(worker));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var index = @this.LongLength - 1; index >= 0; index--)
        @this[index] = worker(@this[index], index);
    }
    /// <summary>
    /// Returns true if there exists an array
    /// </summary>
    /// <typeparam name="TItem">The type of the input.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns><c>true</c> if a given element exists; otherwise, <c>false</c>.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool Exists<TItem>(this TItem[] @this, Predicate<TItem> predicate) => Array.Exists(@this, predicate);

    /// <summary>
    /// Gets the reverse.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <returns>An array where all values are inverted.</returns>
#if NETFX_4
    [Pure]
#endif
    public static TItem[] Reverse<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      var result = new TItem[length];
      for (long i = 0, j = length - 1; j >= 0; ++i, --j)
        result[j] = @this[i];

      return result;
    }

    /// <summary>
    /// Determines whether the given array contains the specified value.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if [contains] [the specified this]; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_4
    [Pure]
#endif
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool Contains<TItem>(this TItem[] @this, TItem value) => @this.IndexOf(value) >= 0;

    /// <summary>
    /// Determines whether an array contains the specified value or not.
    /// </summary>
    /// <param name="this">This array.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the array contains that value; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_4
    [Pure]
#endif
    public static bool Contains(this Array @this, object value) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      // ReSharper disable LoopCanBeConvertedToQuery
      foreach (var item in @this)
        if (item == value)
          return true;
      // ReSharper restore LoopCanBeConvertedToQuery
      return false;
    }

    /// <summary>
    /// Converts the array instance to a real array.
    /// </summary>
    /// <param name="this">This Array.</param>
    /// <returns>An array of objects holding the contents.</returns>
#if NETFX_4
    [Pure]
#endif
    public static object[] ToArray(this Array @this) {
      if (@this == null)
        throw new NullReferenceException();
      if (@this.Rank < 1)
        throw new ArgumentException("Rank must be > 0", nameof(@this));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var result = new object[@this.Length];
      var lbound = @this.GetLowerBound(0);
      for (var i = @this.Length; i > 0;) {
        --i;
        result[i] = @this.GetValue(i + lbound);
      }
      return result;
    }

    /// <summary>
    /// Gets the index of the first item matching the predicate, if any or -1.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>
    /// The index of the item in the array or -1.
    /// </returns>
    public static int IndexOf<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.Length; ++i)
        if (predicate(@this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Gets the index of the first item matching the given, if any or -1.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="this">This Array.</param>
    /// <param name="value">The value.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    /// The index of the item in the array or -1.
    /// </returns>
#if NETFX_4
    [Pure]
#endif
    public static int IndexOf<TItem>(this TItem[] @this, TItem value, IEqualityComparer<TItem> comparer = null) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      if (comparer == null)
        comparer = EqualityComparer<TItem>.Default;

      for (var i = 0; i < @this.Length; ++i)
        if (comparer.Equals(value, @this[i]))
          return i;

      return -1;
    }

    /// <summary>
    /// Gets the index of the first item matching the predicate, if any or -1.
    /// </summary>
    /// <param name="this">This Array.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The index of the item in the array or -1.</returns>
    [DebuggerStepThrough]
    public static int IndexOf(this Array @this, Predicate<object> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif
      for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
        if (predicate(@this.GetValue(i)))
          return i;

      return -1;
    }

    #region high performance linq for arrays

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static bool Any<TItem>(this TItem[] @this) => @this.Length > 0;

    [DebuggerStepThrough]
    public static bool Any<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.LongLength; i++)
        if (predicate(@this[i]))
          return true;
      return false;
    }

    [DebuggerStepThrough]
    public static TItem First<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      if (@this.Length == 0)
        throw new InvalidOperationException("No Elements!");

      return @this[0];
    }

    [DebuggerStepThrough]
    public static TItem Last<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      if (length == 0)
        throw new InvalidOperationException("No Elements!");

      return @this[length - 1];
    }

    [DebuggerStepThrough]
    public static TItem First<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.LongLength; i++) {
        var current = @this[i];
        if (predicate(current))
          return current;
      }
      throw new InvalidOperationException("No Elements!");
    }

    [DebuggerStepThrough]
    public static TItem Last<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = @this.LongLength - 1; i >= 0; --i) {
        var current = @this[i];
        if (predicate(current))
          return current;
      }
      throw new InvalidOperationException("No Elements!");
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem FirstOrDefault<TItem>(this TItem[] @this) => @this.Length == 0 ? default(TItem) : @this[0];

    [DebuggerStepThrough]
    public static TItem LastOrDefault<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      return length == 0 ? default(TItem) : @this[length - 1];
    }

    [DebuggerStepThrough]
    public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.LongLength; ++i) {
        var current = @this[i];
        if (predicate(current))
          return current;
      }
      return default(TItem);
    }

    [DebuggerStepThrough]
    public static TItem LastOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = @this.LongLength - 1; i >= 0; --i) {
        var current = @this[i];
        if (predicate(current))
          return current;
      }
      return default(TItem);
    }

    [DebuggerStepThrough]
    public static TItem Aggregate<TItem>(this TItem[] @this, Func<TItem, TItem, TItem> func) {
      if (@this == null)
        throw new NullReferenceException();
      if (func == null)
        throw new ArgumentNullException(nameof(func));
      if (@this.LongLength == 0)
        throw new InvalidOperationException("No Elements!");

#if NETFX_4
      Contract.EndContractBlock();
#endif

      var result = @this[0];
      for (var i = 1; i < @this.LongLength; i++)
        result = func(result, @this[i]);

      return result;
    }

    [DebuggerStepThrough]
    public static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] @this, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func) {
      if (@this == null)
        throw new NullReferenceException();
      if (func == null)
        throw new ArgumentNullException(nameof(func));
      if (@this.LongLength == 0)
        throw new InvalidOperationException("No Elements!");

#if NETFX_4
      Contract.EndContractBlock();
#endif

      var result = seed;
      for (var i = 0; i < @this.LongLength; ++i)
        result = func(result, @this[i]);

      return result;
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static int Count<TItem>(this TItem[] @this) => @this.Length;

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static long LongCount<TItem>(this TItem[] @this) => @this.LongLength;

    [DebuggerStepThrough]
    public static int Count<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

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
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var result = (long)0;
      for (var i = 0; i < @this.LongLength; ++i)
        if (predicate(@this[i]))
          ++result;

      return result;
    }


    [DebuggerStepThrough]
    public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, TItem defaultValue) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      foreach (var item in @this)
        if (predicate(item))
          return item;

      return defaultValue;
    }

    #region these special Array ...s

    [DebuggerStepThrough]
    public static IEnumerable<TResult> OfType<TResult>(this Array @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.LongLength; ++i) {
        var item = @this.GetValue(i);
        if (item is TResult)
          yield return (TResult)item;
      }
    }

    [DebuggerStepThrough]
    public static object FirstOrDefault(this Array @this, Predicate<object> predicate, object defaultValue = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      foreach (var item in @this)
        if (predicate(item))
          return item;

      return defaultValue;
    }

    [DebuggerStepThrough]
    public static IEnumerable<object> Reverse(this Array @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = @this.GetUpperBound(0); i >= @this.GetLowerBound(0); --i)
        yield return @this.GetValue(i);
    }

    [DebuggerStepThrough]
    public static TItem FirstOrDefault<TItem>(this Array @this, Predicate<TItem> predicate, TItem defaultValue = default(TItem)) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      foreach (var item in @this)
        if (predicate((TItem)item))
          return (TItem)item;

      return defaultValue;
    }

    [DebuggerStepThrough]
    public static IEnumerable<TResult> Cast<TResult>(this Array @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
        yield return (TResult)@this.GetValue(i);
    }

    #endregion

    [DebuggerStepThrough]
    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
      if (@this == null)
        throw new NullReferenceException();
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i]);
    }

    [DebuggerStepThrough]
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
      if (@this == null)
        throw new NullReferenceException();
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i]);
    }

    [DebuggerStepThrough]
    public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, int, TResult> selector) {
      if (@this == null)
        throw new NullReferenceException();
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i], i);
    }

    [DebuggerStepThrough]
    public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, long, TResult> selector) {
      if (@this == null)
        throw new NullReferenceException();
      if (selector == null)
        throw new ArgumentNullException(nameof(selector));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      for (var i = 0; i < length; ++i)
        yield return selector(@this[i], i);
    }

    [DebuggerStepThrough]
    public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current))
          yield return current;
      }
    }

    [DebuggerStepThrough]
    public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Func<TItem, int, bool> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current, i))
          yield return current;
      }
    }

    [DebuggerStepThrough]
    public static IEnumerable<TItem> WhereLong<TItem>(this TItem[] @this, Func<TItem, long, bool> predicate) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.LongLength;
      for (var i = 0; i < length; ++i) {
        var current = @this[i];
        if (predicate(current, i))
          yield return current;
      }
    }

    #endregion

    #region byte-array specials

    /// <summary>
    /// Creates random data in the given buffer; thus effectively overwriting it in-place.
    /// </summary>
    /// <param name="this">This buffer.</param>
    /// <returns>The given buffer</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void RandomizeBuffer(this byte[] @this) => new RNGCryptoServiceProvider().GetBytes(@this);

    /// <summary>
    /// Copies the given buffer.
    /// </summary>
    /// <param name="this">This byte[].</param>
    /// <returns>A copy of the original array.</returns>
    [DebuggerStepThrough]
    public static byte[] Copy(this byte[] @this) {
      if (@this == null)
        return null;

      var length = @this.Length;
      var result = new byte[length];
      if (length > 0)
        Buffer.BlockCopy(@this, 0, result, 0, length);

      return result;
    }

    /// <summary>
    /// Gets a small portion of a byte array.
    /// </summary>
    /// <param name="this">This byte[].</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    /// <returns>The subsequent elements.</returns>
    [DebuggerStepThrough]
    public static byte[] Range(this byte[] @this, int offset, int count) {
      if (@this == null)
        throw new NullReferenceException();
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof(offset), offset, "Must be > 0");
      if (count < 1)
        throw new ArgumentOutOfRangeException(nameof(count), count, "Must be > 0");
#if NETFX_4
      Contract.EndContractBlock();
#endif

      var length = @this.Length;
      var max = count < length - offset ? count : length - offset;
      var result = new byte[max];
      if (max > 0)
        Buffer.BlockCopy(@this, offset, result, 0, max);

      return result;
    }

    /// <summary>
    /// Padds the specified byte array to a certain length if it is smaller.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <param name="length">The final length.</param>
    /// <param name="data">The data to use for padding, default to null-bytes.</param>
    /// <returns>The original array if it already exceeds the wanted size, or an array with the correct size.</returns>
    [DebuggerStepThrough]
    public static byte[] Padd(this byte[] @this, int length, byte data = 0) {
      if (@this == null)
        throw new NullReferenceException();
      if (length < 1)
        throw new ArgumentOutOfRangeException(nameof(length), length, "Must be > 0");
#if NETFX_4
      Contract.EndContractBlock();
#endif

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

    /// <summary>
    /// GZips the given bytes.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>A GZipped byte array.</returns>
    [DebuggerStepThrough]
    public static byte[] GZip(this byte[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      using (var targetStream = new MemoryStream()) {
        using (var gZipStream = new GZipStream(targetStream, CompressionMode.Compress, false))
          gZipStream.Write(@this, 0, @this.Length);

        return targetStream.ToArray();
      }
    }

    /// <summary>
    /// Un-GZips the given bytes.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The unzipped byte array.</returns>
    [DebuggerStepThrough]
    public static byte[] UnGZip(this byte[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      using (var targetStream = new MemoryStream()) {
        using (var sourceStream = new MemoryStream(@this)) {
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
        return targetStream.ToArray();
      }
    }

    #region hash computation

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The result of the hash algorithm</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeHash<THashAlgorithm>(this byte[] @this) where THashAlgorithm : HashAlgorithm, new() {
      if (@this == null)
        throw new NullReferenceException();
#if NETFX_4
      Contract.EndContractBlock();
#endif

      using (var provider = new THashAlgorithm())
        return provider.ComputeHash(@this);
    }

    /// <summary>
    /// Calculates the SHA512 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA512Hash(this byte[] @this) => @this.ComputeHash<SHA512CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA384 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA384Hash(this byte[] @this) => @this.ComputeHash<SHA384CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA256 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA256Hash(this byte[] @this) => @this.ComputeHash<SHA256CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA-1 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA1Hash(this byte[] @this) => @this.ComputeHash<SHA1CryptoServiceProvider>();

    /// <summary>
    /// Calculates the MD5 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeMD5Hash(this byte[] @this) => @this.ComputeHash<MD5CryptoServiceProvider>();

    #endregion

    #region utility classes
    private static class RuntimeConfiguration {
      public static readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

      public static bool Has16BitRegisters => IntPtr.Size >= 2;

      public static bool Has32BitRegisters => IntPtr.Size >= 4;

      public static bool Has64BitRegisters => IntPtr.Size >= 8;

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
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];
#if NETFX_4
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
#endif
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];

#if NETFX_4
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
#endif
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];
#if NETFX_4
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
#endif
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
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NETFX_4
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
#endif
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NETFX_4
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
#endif
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NETFX_4
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
#endif
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
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NETFX_4
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
#endif
      }

      private static void _DoDWords(uint[] source, uint[] operand) {
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NETFX_4
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
#endif
      }

      private static void _DoQWords(ulong[] source, ulong[] operand) {

#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NETFX_4
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
#endif
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
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= 0xffff;
#if NETFX_4
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
#endif
      }

      private static void _DoDWords(uint[] source) {
#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];
#if NETFX_4
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
#endif
      }

      private static void _DoQWords(ulong[] source) {

#if NETFX_4
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];
#if NETFX_4
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
#endif
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

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void Xor(this byte[] @this, byte[] operand) {
#if UNSAFE
      FastXor.ProcessInUnsafeChunks(@this, operand);
#else
      FastXor.ProcessInChunks(@this, operand);
#endif
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void XorBytewise(this byte[] @this, byte[] operand) => FastXor.ProcessBytewise(@this, operand);

    public static void And(this byte[] @this, byte[] operand) {
#if UNSAFE
      FastAnd.ProcessInUnsafeChunks(@this, operand);
#else
      FastAnd.ProcessInChunks(@this, operand);
#endif
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void AndBytewise(this byte[] @this, byte[] operand) => FastAnd.ProcessBytewise(@this, operand);

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void Or(this byte[] @this, byte[] operand) {
#if UNSAFE
      FastOr.ProcessInUnsafeChunks(@this, operand);
#else
      FastOr.ProcessInChunks(@this, operand);
#endif
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void OrBytewise(this byte[] @this, byte[] operand) => FastOr.ProcessBytewise(@this, operand);

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void Not(this byte[] @this) {
#if UNSAFE
      FastNot.ProcessInUnsafeChunks(@this);
#else
      FastNot.ProcessInChunks(@this);
#endif
    }

#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void NotBytewise(this byte[] @this) => FastNot.ProcessBytewise(@this);

    #endregion

  }
}