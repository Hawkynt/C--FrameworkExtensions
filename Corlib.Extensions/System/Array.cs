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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.IO.Compression;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
using System.Runtime.InteropServices;
#if !UNSAFE
using System.Security.Permissions;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class ArrayExtensions {

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

  [DebuggerDisplay("{" + nameof(ToString) + "()}")]
  public class ReadOnlyArraySlice<TItem> : IEnumerable<TItem> {

    protected readonly TItem[] _source;
    protected readonly int _start;

    public ReadOnlyArraySlice(TItem[] source, int start, int length) {
      Guard.Against.ThisIsNull(source);

      if (start + length > source.Length)
        Guard.AlwaysThrow.ArgumentException(nameof(length), "Exceeding source length");

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
#if SUPPORTS_CONTRACTS
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

      return new(this._source, start + this._start, length);
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

    public static explicit operator TItem[](ReadOnlyArraySlice<TItem> @this) => @this.ToArray();

    #region Overrides of Object

    public override string ToString() => $"{typeof(TItem).Name}[{this._start}..{this._start + this.Length - 1}]";

    #endregion
  }

  [DebuggerDisplay("{" + nameof(ToString) + "()}")]
  public class ArraySlice<TItem> : ReadOnlyArraySlice<TItem> {

    public ArraySlice(TItem[] source, int start, int length) : base(source, start, length) { }

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
#if SUPPORTS_CONTRACTS
        Contract.Requires(index < this.Length);
#endif
        return this._source[index + this._start];
      }
      set {
#if SUPPORTS_CONTRACTS
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

  [StructLayout(LayoutKind.Sequential, Size = 32)]
  private struct Block32 {
    public readonly uint a;
    public readonly uint b;
    public readonly uint c;
    public readonly uint d;
    public readonly uint e;
    public readonly uint f;
    public readonly uint g;
    public readonly uint h;

    public Block32(uint u) {
      this.a = u;
      this.b = u;
      this.c = u;
      this.d = u;
      this.e = u;
      this.f = u;
      this.g = u;
      this.h = u;
    }

    public Block32(ushort u) : this(0x00010001U * u) { }
    public Block32(byte u) : this(0x01010101U * u) { }

  }

  [StructLayout(LayoutKind.Sequential, Size = 64)]
  private struct Block64 {
    public readonly ulong a;
    public readonly ulong b;
    public readonly ulong c;
    public readonly ulong d;
    public readonly ulong e;
    public readonly ulong f;
    public readonly ulong g;
    public readonly ulong h;

    public Block64(ulong u) {
      this.a = u;
      this.b = u;
      this.c = u;
      this.d = u;
      this.e = u;
      this.f = u;
      this.g = u;
      this.h = u;
    }

    public Block64(uint u) : this(0x0000000100000001UL * u) { }
    public Block64(ushort u) : this(0x0001000100010001UL * u) { }
    public Block64(byte u) : this(0x0101010101010101UL * u) { }
  }

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
  /// Compares two arrays against each other.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="other">The other Array.</param>
  /// <param name="comparer">The value comparer; optional: uses default.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this TItem[] @this, TItem[] other, IEqualityComparer<TItem> comparer = null) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(other);

    comparer ??= EqualityComparer<TItem>.Default;

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
            yield return new ChangeSet<TItem>(ChangeType.Added, index, @this[index], _INDEX_WHEN_NOT_FOUND, default(TItem));
          }

          yield return new ChangeSet<TItem>(ChangeType.Equal, i, @this[i], targetIndex, other[targetIndex]);
        } else {
          if (currentSourceBuffer.Count > 0) {
            var index = currentSourceBuffer.Dequeue();
            yield return new ChangeSet<TItem>(ChangeType.Changed, index, @this[index], targetIndex, other[targetIndex]);
          } else
            yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default(TItem), targetIndex, other[targetIndex]);
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
        yield return new ChangeSet<TItem>(ChangeType.Added, index, @this[index], _INDEX_WHEN_NOT_FOUND, default);
      }
    }

    while (targetIndex < targetLen) {
      yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default, targetIndex, other[targetIndex]);
      ++targetIndex;
    }
  }

  private static int _IndexOf<TValue>(TValue[] values, TValue item, int startIndex, IEqualityComparer<TValue> comparer) {
    for (var i = startIndex; i < values.Length; ++i)
      if (ReferenceEquals(values[i], item) || comparer.Equals(values[i], item))
        return i;

    return _INDEX_WHEN_NOT_FOUND;
  }

  /// <summary>
  /// Returns the enumeration or <c>null</c> if it is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <returns><c>null</c> if the enumeration is empty; otherwise, the enumeration itself </returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem[] ToNullIfEmpty<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this : null;

  /// <summary>
  /// Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length; negative values mean: till the end.</param>
  /// <returns>An array slice which accesses the underlying array but can only be read.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static ReadOnlyArraySlice<TItem> ReadOnlySlice<TItem>(this TItem[] @this, int start, int length = -1) {
    Guard.Against.ThisIsNull(@this);

    return new(@this, start, length < 0 ? @this.Length - start : length);
  }

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
    Guard.Against.ThisIsNull(@this);
    Guard.Against.NegativeValuesAndZero(size);

    var length = @this.Length;
    for (var index = 0; index < length; index += size)
      yield return new(@this, index, Math.Min(length - index, size));
  }

  /// <summary>
  /// Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length; negative values mean: till the end.</param>
  /// <returns>An array slice which accesses the underlying array.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static ArraySlice<TItem> Slice<TItem>(this TItem[] @this, int start, int length = -1) {
    Guard.Against.ThisIsNull(@this);

    return new(@this, start, length < 0 ? @this.Length - start : length);
  }

  /// <summary>
  /// Slices the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="size">The size of the slices.</param>
  /// <returns>An enumeration of slices</returns>
  [DebuggerStepThrough]
  public static IEnumerable<ArraySlice<TItem>> Slices<TItem>(this TItem[] @this, int size) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.NegativeValuesAndZero(size);

    var length = @this.Length;
    for (var index = 0; index < length; index += size)
      yield return new(@this, index, Math.Min(length - index, size));
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
    Guard.Against.ThisIsNull(@this);

    if (@this.Length == 0)
      Guard.AlwaysThrow.InvalidOperationException("No Elements!");

#if SUPPORTS_RANDOM_SHARED
    random ??= Random.Shared;
#else
    random ??= new((int)Stopwatch.GetTimestamp());
#endif

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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index) {
    Guard.Against.ThisIsNull(@this);

    return @this.Length <= index ? default : @this[index];
  }

  /// <summary>
  /// Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, TItem defaultValue) {
    Guard.Against.ThisIsNull(@this);

    return @this.Length <= index ? defaultValue : @this[index];
  }

  /// <summary>
  /// Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="factory">The factory to create default values.</param>
  /// <returns></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<TItem> factory) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(factory);

    return @this.Length <= index ? factory() : @this[index];
  }

  /// <summary>
  /// Gets the value or default.
  /// </summary>
  /// <typeparam name="TItem">The type of the value.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="index">The index.</param>
  /// <param name="factory">The factory to create default values.</param>
  /// <returns></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<int, TItem> factory) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(factory);

    return @this.Length <= index ? factory(index) : @this[index];
  }

  /// <summary>
  /// Clones the specified array.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Array.</param>
  /// <returns>A new array or <c>null</c> if this array was <c>null</c>.</returns>
  [DebuggerStepThrough]
  public static TItem[] SafelyClone<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

    var len = @this.Length;
    var result = new TItem[len];
    if (len > 0)
      @this.CopyTo(result, len);

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
    Guard.Against.ThisIsNull(@this);

    var result = new StringBuilder();
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
  /// Splices the specified array (returns part of that array).
  /// </summary>
  /// <typeparam name="TItem">Type of data in the array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="startIndex">The start element which should be included in the splice.</param>
  /// <param name="count">The number of elements from there on.</param>
  /// <returns></returns>
  public static TItem[] Range<TItem>(this TItem[] @this, int startIndex, int count) {
    Guard.Against.ThisIsNull(@this);

#if SUPPORTS_CONTRACTS
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Swap<TItem>(this TItem[] @this, int firstElementIndex, int secondElementIndex) {
    Guard.Against.ThisIsNull(@this);

    (@this[firstElementIndex], @this[secondElementIndex]) = (@this[secondElementIndex], @this[firstElementIndex]);
  }

  /// <summary>
  /// Shuffles the specified data.
  /// </summary>
  /// <typeparam name="TItem">Type of elements in the array.</typeparam>
  /// <param name="this">This array.</param>
  public static void Shuffle<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

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
    Guard.Against.ThisIsNull(@this);

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
    Guard.Against.ThisIsNull(@this);

    if (@this.Length > 0)
      _QuickSort_Comparable(@this, 0, @this.Length - 1);
  }

  private static void _QuickSort_Comparable<TValue>(TValue[] @this, int left, int right) where TValue : IComparable<TValue> {
    if (@this == null) {
      // nothing to sort
      return;
    }

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
  /// Converts all elements.
  /// </summary>
  /// <typeparam name="TItem">The type the elements.</typeparam>
  /// <typeparam name="TOutput">The output type.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns>An array containing the converted values.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Converter<TItem, TOutput> converter) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(converter);

    return Array.ConvertAll(@this, converter);
  }

  /// <summary>
  /// Converts all elements.
  /// </summary>
  /// <typeparam name="TItem">The type the elements.</typeparam>
  /// <typeparam name="TOutput">The output type.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="converter">The converter function.</param>
  /// <returns>An array containing the converted values.</returns>
  public static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Func<TItem, int, TOutput> converter) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(converter);

    var length = @this.Length;
    var result = new TOutput[length];
    for (var index = length - 1; index >= 0; --index)
      result[index] = converter(@this[index], index);

    return result;
  }

  /// <summary>
  /// Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem> action) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(action);

    Array.ForEach(@this, action);
  }

#if SUPPORTS_ASYNC
  /// <summary>
  /// Executes a callback with each element in an array in parallel.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback to execute for each element.</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static void ParallelForEach<TItem>(this TItem[] @this, Action<TItem> action) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(action);

    Parallel.ForEach(@this, action);
  }

#endif

  /// <summary>
  /// Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem, int> action) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(action);

    for (var i = @this.Length - 1; i >= 0; --i)
      action(@this[i], i);
  }

  /// <summary>
  /// Executes a callback with each element in an array.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="action">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Action<TItem, long> action) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(action);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      action(@this[i], i);
  }

  /// <summary>
  /// Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, TItem> worker) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(worker);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      @this[i] = worker(@this[i]);
  }

  /// <summary>
  /// Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, int, TItem> worker) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(worker);

    for (var i = @this.Length - 1; i >= 0; --i)
      @this[i] = worker(@this[i], i);
  }

  /// <summary>
  /// Executes a callback with each element in an array and writes back the result.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="worker">The callback for each element.</param>
  public static void ForEach<TItem>(this TItem[] @this, Func<TItem, long, TItem> worker) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(worker);

    for (var i = @this.LongLength - 1; i >= 0; --i)
      @this[i] = worker(@this[i], i);
  }

  /// <summary>
  /// Returns true if there exists an array
  /// </summary>
  /// <typeparam name="TItem">The type of the input.</typeparam>
  /// <param name="this">This array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns><c>true</c> if a given element exists; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool Exists<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    return Array.Exists(@this, predicate);
  }

  /// <summary>
  /// Gets the reverse.
  /// </summary>
  /// <typeparam name="TItem">The type of the input array.</typeparam>
  /// <param name="this">This array.</param>
  /// <returns>An array where all values are inverted.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
  public static TItem[] Reverse<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

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
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool Contains<TItem>(this TItem[] @this, TItem value) {
    Guard.Against.ThisIsNull(@this);

    return @this.IndexOf(value) >= 0;
  }

  /// <summary>
  /// Determines whether an array contains the specified value or not.
  /// </summary>
  /// <param name="this">This array.</param>
  /// <param name="value">The value.</param>
  /// <returns>
  ///   <c>true</c> if the array contains that value; otherwise, <c>false</c>.
  /// </returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
  public static bool Contains(this Array @this, object value) {
    Guard.Against.ThisIsNull(@this);

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
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
  public static object[] ToArray(this Array @this) {
    Guard.Against.ThisIsNull(@this);

    if (@this.Rank < 1)
      Guard.AlwaysThrow.ArgumentException(nameof(@this), "Rank must be > 0");

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
  public static int IndexOf<TItem>(this TItem[] @this, Predicate<TItem> predicate) => IndexOfOrDefault(@this, predicate, _INDEX_WHEN_NOT_FOUND);

  /// <summary>
  /// Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>
  /// The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, int defaultValue) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValue;
  }

  /// <summary>
  /// Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValueFactory">The function that generates the default value.</param>
  /// <returns>
  /// The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<int> defaultValueFactory) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValueFactory();
  }

  /// <summary>
  /// Gets the index of the first item matching the predicate, if any or the given default value.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <param name="defaultValueFactory">The function that generates the default value.</param>
  /// <returns>
  /// The index of the item in the array or the default value.
  /// </returns>
  public static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<TItem[], int> defaultValueFactory) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.Length; ++i)
      if (predicate(@this[i]))
        return i;

    return defaultValueFactory(@this);
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
  public static int IndexOf<TItem>(this TItem[] @this, TItem value, IEqualityComparer<TItem> comparer = null) {
    Guard.Against.ThisIsNull(@this);

    comparer ??= EqualityComparer<TItem>.Default;

    for (var i = 0; i < @this.Length; ++i)
      if (comparer.Equals(value, @this[i]))
        return i;

    return _INDEX_WHEN_NOT_FOUND;
  }

  /// <summary>
  /// Gets the index of the first item matching the predicate, if any or -1.
  /// </summary>
  /// <param name="this">This Array.</param>
  /// <param name="predicate">The predicate.</param>
  /// <returns>The index of the item in the array or -1.</returns>
  [DebuggerStepThrough]
  public static int IndexOf(this Array @this, Predicate<object> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
      if (predicate(@this.GetValue(i)))
        return i;

    return _INDEX_WHEN_NOT_FOUND;
  }

  /// <summary>
  /// Rotates all elements in the array one index down.
  /// </summary>
  /// <typeparam name="TItem">The type of the array elements</typeparam>
  /// <param name="this">This array</param>
  public static void RotateTowardsZero<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

    if (@this.Length == 0)
      return;

    var first = @this[0];
    for (var i = 1; i < @this.Length; ++i)
      @this[i - 1] = @this[i];

    @this[^1] = first;
  }

  /// <summary>
  /// Allows processing an array in chunks
  /// </summary>
  /// <typeparam name="TItem">The type of the items</typeparam>
  /// <param name="this">This Array</param>
  /// <param name="chunkSize">The maximum chunk size to process</param>
  /// <param name="processor">The action to execute on each chunk</param>
  public static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor)
    => ProcessInChunks(@this, chunkSize, processor, @this.Length, 0);

  /// <summary>
  /// Allows processing an array in chunks
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
  /// Determines whether the given array is empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><c>true</c> if the array reference is <c>null</c> or the array has no elements; otherwise, <c>false</c></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNullEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
    [NotNullWhen(false)]
#endif
    this TItem[] @this) => @this is not { Length: > 0 };

  /// <summary>
  /// Determines whether the given array is not empty.
  /// </summary>
  /// <typeparam name="TItem">The type of the elements</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><c>true</c> if the array reference is not <c>null</c> and the array has elements; otherwise, <c>false</c></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotNullEmpty<TItem>(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
    [NotNullWhen(true)]
#endif
    this TItem[] @this) => @this is { Length: > 0 };

  /// <summary>
  /// Initializes a jagged array with default values.
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

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string ToStringInstance(this char[] @this) => new(@this);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string ToStringInstance(this char[] @this, int startIndex) => @this == null || @this.Length <= startIndex ? string.Empty : new(@this, startIndex, @this.Length - startIndex);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string ToStringInstance(this char[] @this, int startIndex, int length) => @this == null || length < 1 || @this.Length <= startIndex ? string.Empty : new(@this, startIndex, length);

  #region high performance linq for arrays

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static bool Any<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

    return @this.Length > 0;
  }

  [DebuggerStepThrough]
  public static bool Any<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; i++)
      if (predicate(@this[i]))
        return true;

    return false;
  }

  [DebuggerStepThrough]
  public static TItem First<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

    if (@this.Length == 0)
      Guard.AlwaysThrow.InvalidOperationException("No Elements!");

    return @this[0];
  }

  [DebuggerStepThrough]
  public static TItem Last<TItem>(this TItem[] @this) {
    Guard.Against.ThisIsNull(@this);

    var length = @this.LongLength;
    if (length == 0)
      Guard.AlwaysThrow.InvalidOperationException("No Elements!");

    return @this[^1];
  }

  [DebuggerStepThrough]
  public static TItem First<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; ++i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    Guard.AlwaysThrow.InvalidOperationException("No Elements!");
    return default;
  }

  [DebuggerStepThrough]
  public static TItem Last<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = @this.LongLength - 1; i >= 0; --i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    Guard.AlwaysThrow.InvalidOperationException("No Elements!");
    return default;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this[0] : default;

  [DebuggerStepThrough]
  public static TItem LastOrDefault<TItem>(this TItem[] @this) => @this is { Length: > 0 } ? @this[^1] : default;

  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = 0; i < @this.LongLength; ++i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    return default;
  }

  [DebuggerStepThrough]
  public static TItem LastOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    for (var i = @this.LongLength - 1; i >= 0; --i) {
      var current = @this[i];
      if (predicate(current))
        return current;
    }

    return default;
  }

  [DebuggerStepThrough]
  public static TItem Aggregate<TItem>(this TItem[] @this, Func<TItem, TItem, TItem> func) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(func);

    if (@this.LongLength == 0)
      Guard.AlwaysThrow.InvalidOperationException("No Elements!");

    var result = @this[0];
    for (var i = 1; i < @this.LongLength; ++i)
      result = func(result, @this[i]);

    return result;
  }

  [DebuggerStepThrough]
  public static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] @this, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(func);

    if (@this.LongLength == 0)
      Guard.AlwaysThrow.InvalidOperationException("No Elements!");

    var result = seed;
    for (var i = 0; i < @this.LongLength; ++i)
      result = func(result, @this[i]);

    return result;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static int Count<TItem>(this TItem[] @this) => @this.Length;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static long LongCount<TItem>(this TItem[] @this) => @this.LongLength;

  [DebuggerStepThrough]
  public static int Count<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

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
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    var result = (long)0;
    for (var i = 0; i < @this.LongLength; ++i)
      if (predicate(@this[i]))
        ++result;

    return result;
  }


  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, TItem defaultValue) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  #region these special Array ...s

  [DebuggerStepThrough]
  public static IEnumerable<TResult> OfType<TResult>(this Array @this) {
    Guard.Against.ThisIsNull(@this);

    for (var i = 0; i < @this.LongLength; ++i) {
      var item = @this.GetValue(i);
      if (item is TResult result)
        yield return result;
    }
  }

  [DebuggerStepThrough]
  public static object FirstOrDefault(this Array @this, Predicate<object> predicate, object defaultValue = null) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  [DebuggerStepThrough]
  public static IEnumerable<object> Reverse(this Array @this) {
    Guard.Against.ThisIsNull(@this);

    for (var i = @this.GetUpperBound(0); i >= @this.GetLowerBound(0); --i)
      yield return @this.GetValue(i);
  }

  [DebuggerStepThrough]
  public static TItem FirstOrDefault<TItem>(this Array @this, Predicate<TItem> predicate, TItem defaultValue = default) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    foreach (var item in @this)
      if (predicate((TItem)item))
        return (TItem)item;

    return defaultValue;
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Cast<TResult>(this Array @this) {
    Guard.Against.ThisIsNull(@this);

    for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); ++i)
      yield return (TResult)@this.GetValue(i);
  }

  #endregion

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(selector);

    var length = @this.Length;
    for (var i = 0; i < length; ++i)
      yield return selector(@this[i]);
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(selector);

    var length = @this.LongLength;
    for (var i = 0; i < length; ++i)
      yield return selector(@this[i]);
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, int, TResult> selector) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(selector);

    var length = @this.Length;
    for (var i = 0; i < length; ++i)
      yield return selector(@this[i], i);
  }

  [DebuggerStepThrough]
  public static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, long, TResult> selector) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(selector);

    var length = @this.LongLength;
    for (var i = 0; i < length; ++i)
      yield return selector(@this[i], i);
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Predicate<TItem> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    var length = @this.LongLength;
    for (var i = 0; i < length; ++i) {
      var current = @this[i];
      if (predicate(current))
        yield return current;
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Func<TItem, int, bool> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    var length = @this.Length;
    for (var i = 0; i < length; ++i) {
      var current = @this[i];
      if (predicate(current, i))
        yield return current;
    }
  }

  [DebuggerStepThrough]
  public static IEnumerable<TItem> WhereLong<TItem>(this TItem[] @this, Func<TItem, long, bool> predicate) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(predicate);

    var length = @this.LongLength;
    for (var i = 0; i < length; ++i) {
      var current = @this[i];
      if (predicate(current, i))
        yield return current;
    }
  }

  #endregion

  #region byte-array specials

  private static readonly string _HEX_BYTES_UPPER_CASE = "0123456789ABCDEF";
  private static readonly string _HEX_BYTES_LOWER_CASE = "0123456789abcdef";

  /// <summary>
  /// Converts the bytes to a hex representation.
  /// </summary>
  /// <param name="this">These Bytes</param>
  /// <param name="allUpperCase">Uses upper-case (<c>true</c>) hex letters only or lower-case (<c>false</c>).</param>
  /// <returns>A hex string or <c>null</c></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string ToHex(this byte[] @this, bool allUpperCase = false) {
    if (@this == null)
      return null;

    var format = allUpperCase ? _HEX_BYTES_UPPER_CASE : _HEX_BYTES_LOWER_CASE;
    var result = new char[@this.Length << 1];
    for (int i = 0, j = 0; i < @this.Length; ++i, j += 2) {
      var value = @this[i];
      result[j] = format[value >> 4];
      result[j + 1] = format[value & 0b1111];
    }

    return new(result);
  }

  /// <summary>
  /// Creates random data in the given buffer; thus effectively overwriting it in-place.
  /// </summary>
  /// <param name="this">This buffer.</param>
  /// <returns>The given buffer</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static void RandomizeBuffer(this byte[] @this) {
#if SUPPORTS_RNG_FILL
    RandomNumberGenerator.Fill(@this);
#else
#if NEEDS_RNG_DISPOSE
    using var provider = new RNGCryptoServiceProvider();
    provider.GetBytes(@this);
#else
    new RNGCryptoServiceProvider().GetBytes(@this);
#endif
#endif
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
    Guard.Against.ThisIsNull(@this);
    Guard.Against.IndexBelowZero(offset);
    Guard.Against.CountBelowOrEqualZero(count);

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
    Guard.Against.ThisIsNull(@this);
    Guard.Against.CountBelowOrEqualZero(length);

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
  /// GZips the given bytes.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>A GZipped byte array.</returns>
  [DebuggerStepThrough]
  public static byte[] GZip(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var targetStream = new MemoryStream();
    using (var gZipStream = new GZipStream(targetStream, CompressionMode.Compress, false))
      gZipStream.Write(@this, 0, @this.Length);

    return targetStream.ToArray();
  }

  /// <summary>
  /// Un-GZips the given bytes.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The unzipped byte array.</returns>
  [DebuggerStepThrough]
  public static byte[] UnGZip(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var targetStream = new MemoryStream();
    using (var sourceStream = new MemoryStream(@this))
    using (var gZipStream = new GZipStream(sourceStream, CompressionMode.Decompress, false)) {

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
    => IndexOfOrDefault(@this, searchString, 0, (_, __) => defaultValue);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, (_, __) => defaultValueFunc());

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, (t, _) => defaultValueFunc(t));

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, 0, defaultValueFunc);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, int defaultValue)
    => IndexOfOrDefault(@this, searchString, offset, (_, __) => defaultValue);

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, offset, (_, __) => defaultValueFunc());

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], int> defaultValueFunc)
    => IndexOfOrDefault(@this, searchString, offset, (t, _) => defaultValueFunc(t));

  public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], byte[], int> defaultValueFunc) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(searchString);

    if (ReferenceEquals(@this, searchString))
      return 0;

    var searchStringLength = searchString.Length;
    var dataStringLength = @this.Length;

    if (searchStringLength < 1)
      return 0;

    if ((dataStringLength + offset) < searchStringLength)
      return _INDEX_WHEN_NOT_FOUND;

    // ReSharper disable once JoinDeclarationAndInitializer
    int index;
#if UNSAFE
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
#endif
      index = _ContainsBoyerMoore(@this, searchString, offset);

    return (index < 0 && defaultValueFunc != null) ? defaultValueFunc(@this, searchString) : index;
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

#if UNSAFE
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
#endif

  private static int _ContainsBoyerMoore(byte[] haystack, byte[] needle, int offset) {
    var searchStringLength = needle.Length;
    var dataStringLength = haystack.Length;

    var dictComparisonBytes = new Dictionary<byte, int>();
    for (var j = 0; j < searchStringLength; ++j) {
      var value = searchStringLength - j - 1;
      if (dictComparisonBytes.ContainsKey(needle[j]))
        dictComparisonBytes[needle[j]] = value;
      else
        dictComparisonBytes.Add(needle[j], value);
    }

    var i = offset; // First index to check.

    // Loop while there's still room for search term
    while (i <= (dataStringLength - searchStringLength)) {
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
  /// Computes the hash.
  /// </summary>
  /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The result of the hash algorithm</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeHash<THashAlgorithm>(this byte[] @this) where THashAlgorithm : HashAlgorithm, new() {
    Guard.Against.ThisIsNull(@this);

    using var provider = new THashAlgorithm();
    return provider.ComputeHash(@this);
  }

#endif

  /// <summary>
  /// Calculates the SHA512 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeSHA512Hash(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var provider = SHA512.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  /// Calculates the SHA384 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeSHA384Hash(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var provider = SHA384.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  /// Calculates the SHA256 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeSHA256Hash(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var provider = SHA256.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  /// Calculates the SHA-1 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeSHA1Hash(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var provider = SHA1.Create();
    return provider.ComputeHash(@this);
  }

  /// <summary>
  /// Calculates the MD5 hash.
  /// </summary>
  /// <param name="this">This Byte-Array.</param>
  /// <returns>The hash</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static byte[] ComputeMD5Hash(this byte[] @this) {
    Guard.Against.ThisIsNull(@this);

    using var provider = MD5.Create();
    return provider.ComputeHash(@this);
  }

  #endregion

  #endregion

  /// <summary>
  /// Determines whether a given <see cref="Array"/> contains exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><see langword="true"/> if the <see cref="Array"/> has one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsSingle<TValue>(this TValue[] @this) {
    Guard.Against.ThisIsNull(@this);
    return @this.Length == 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="Array"/> contains exactly more than one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><see langword="true"/> if the <see cref="Array"/> has more than one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsMultiple<TValue>(this TValue[] @this) {
    Guard.Against.ThisIsNull(@this);
    return @this.Length > 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="Array"/> contains not exactly one element.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><see langword="true"/> if the <see cref="Array"/> has more or less than one element; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNoSingle<TValue>(this TValue[] @this) {
    Guard.Against.ThisIsNull(@this);
    return @this.Length != 1;
  }

  /// <summary>
  /// Determines whether a given <see cref="Array"/> contains less than two elements.
  /// </summary>
  /// <typeparam name="TValue">The type of items</typeparam>
  /// <param name="this">This <see cref="Array"/></param>
  /// <returns><see langword="true"/> if the <see cref="Array"/> has less than two elements; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNoMultiple<TValue>(this TValue[] @this) {
    Guard.Against.ThisIsNull(@this);
    return @this.Length <= 1;
  }
}
