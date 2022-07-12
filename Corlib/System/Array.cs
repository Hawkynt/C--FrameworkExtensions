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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if NET40_OR_GREATER
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.IO.Compression;
#if NET45_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Security.Cryptography;
using System.Text;
#if NET40_OR_GREATER
using System.Threading.Tasks;
#endif
using System.Runtime.InteropServices;
#if !UNSAFE
using System.Security.Permissions;
#endif
using Block2 = System.UInt16;
using Block4 = System.UInt32;
using Block8 = System.UInt64;

using SBlock2 = System.Int16;
using SBlock4 = System.Int32;
using SBlock8 = System.Int64;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {

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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
    public class ArraySlice<TItem> : ReadOnlyArraySlice<TItem> {

      public ArraySlice(TItem[] source, int start, int length) : base(source, start, length) {
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
          Contract.Requires(index < this.Length);
#endif
          return this._source[index + this._start];
        }
        set {
#if NET40_OR_GREATER
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

#if UNSAFE
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    private struct Block32 {
      public readonly Block4 a;
      public readonly Block4 b;
      public readonly Block4 c;
      public readonly Block4 d;
      public readonly Block4 e;
      public readonly Block4 f;
      public readonly Block4 g;
      public readonly Block4 h;

      public Block32(Block4 u) {
        this.a = u;
        this.b = u;
        this.c = u;
        this.d = u;
        this.e = u;
        this.f = u;
        this.g = u;
        this.h = u;
      }

      public Block32(Block2 u) : this(0x00010001U * u) { }
      public Block32(byte u) : this(0x01010101U * u) { }

    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct Block64 {
      public readonly Block8 a;
      public readonly Block8 b;
      public readonly Block8 c;
      public readonly Block8 d;
      public readonly Block8 e;
      public readonly Block8 f;
      public readonly Block8 g;
      public readonly Block8 h;

      public Block64(Block8 u) {
        this.a = u;
        this.b = u;
        this.c = u;
        this.d = u;
        this.e = u;
        this.f = u;
        this.g = u;
        this.h = u;
      }

      public Block64(Block4 u) : this(0x0000000100000001UL * u) { }
      public Block64(Block2 u) : this(0x0001000100010001UL * u) { }
      public Block64(byte u) : this(0x0101010101010101UL * u) { }
    }

#else

    [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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
      if (@this == null)
        throw new NullReferenceException();
      if (other == null)
        throw new ArgumentNullException(nameof(other));

#if NET40_OR_GREATER
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
          yield return new ChangeSet<TItem>(ChangeType.Added, index, @this[index], _INDEX_WHEN_NOT_FOUND, default(TItem));
        }
      }

      while (targetIndex < targetLen) {
        yield return new ChangeSet<TItem>(ChangeType.Removed, _INDEX_WHEN_NOT_FOUND, default(TItem), targetIndex, other[targetIndex]);
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
#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void ForEach<TItem>(this TItem[] @this, Action<TItem> action) => Array.ForEach(@this, action);

#if NET40_OR_GREATER
    /// <summary>
    /// Executes a callback with each element in an array in parallel.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <param name="action">The callback to execute for each element.</param>
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool Exists<TItem>(this TItem[] @this, Predicate<TItem> predicate) => Array.Exists(@this, predicate);

    /// <summary>
    /// Gets the reverse.
    /// </summary>
    /// <typeparam name="TItem">The type of the input array.</typeparam>
    /// <param name="this">This array.</param>
    /// <returns>An array where all values are inverted.</returns>
#if NET40_OR_GREATER
    [Pure]
#endif
    public static TItem[] Reverse<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
    [Pure]
#endif
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
    [Pure]
#endif
    public static bool Contains(this Array @this, object value) {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
    [Pure]
#endif
    public static object[] ToArray(this Array @this) {
      if (@this == null)
        throw new NullReferenceException();
      if (@this.Rank < 1)
        throw new ArgumentException("Rank must be > 0", nameof(@this));
#if NET40_OR_GREATER
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
    public static int IndexOf<TItem>(this TItem[] @this, Predicate<TItem> predicate) => IndexOfOrDefault<TItem>(@this, predicate, _INDEX_WHEN_NOT_FOUND);

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
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

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
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

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
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

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
#if NET40_OR_GREATER
    [Pure]
#endif
    public static int IndexOf<TItem>(this TItem[] @this, TItem value, IEqualityComparer<TItem> comparer = null) {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

      if (comparer == null)
        comparer = EqualityComparer<TItem>.Default;

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
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif
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
      var first = @this[0];
      for (var i = 1; i < @this.Length; ++i)
        @this[i - 1] = @this[i];

      @this[@this.Length - 1] = first;
    }

    /// <summary>
    /// Allows processing an array in chunks
    /// </summary>
    /// <typeparam name="TItem">The type of the items</typeparam>
    /// <param name="this">This Array</param>
    /// <param name="chunkSize">The maximum chunk size to process</param>
    /// <param name="processor">The action to execute on each chunk</param>
    public static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor)
      => ProcessInChunks(@this, chunkSize, processor, @this.Length, 0)
    ;

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
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNullEmpty<TItem>(this TItem[] @this)=>@this==null||@this.Length<=0;

    /// <summary>
    /// Determines whether the given array is not empty.
    /// </summary>
    /// <typeparam name="TItem">The type of the elements</typeparam>
    /// <param name="this">This <see cref="Array"/></param>
    /// <returns><c>true</c> if the array reference is not <c>null</c> and the array has elements; otherwise, <c>false</c></returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNotNullEmpty<TItem>(this TItem[] @this) => @this != null && @this.Length > 0;

    /// <summary>
    /// Initializes a jagged array with default values.
    /// </summary>
    /// <typeparam name="TArray">The type of the array</typeparam>
    /// <param name="lengths">The lengths in all dimensions</param>
    /// <returns>The resulting array</returns>
    public static TArray CreatedJaggedArray<TArray>(params int[] lengths)
      => (TArray)_InitializeJaggedArray(typeof(TArray).GetElementType(), 0, lengths)
    ;

    private static object _InitializeJaggedArray(Type arrayType, int index, int[] lengths) {

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

    #region high performance linq for arrays

#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

      for (var i = @this.LongLength - 1; i >= 0; --i) {
        var current = @this[i];
        if (predicate(current))
          return current;
      }
      throw new InvalidOperationException("No Elements!");
    }

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static TItem FirstOrDefault<TItem>(this TItem[] @this) => @this.Length == 0 ? default(TItem) : @this[0];

    [DebuggerStepThrough]
    public static TItem LastOrDefault<TItem>(this TItem[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

      var result = seed;
      for (var i = 0; i < @this.LongLength; ++i)
        result = func(result, @this[i]);

      return result;
    }

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static int Count<TItem>(this TItem[] @this) => @this.Length;

#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
      Contract.EndContractBlock();
#endif

      for (var i = 0; i < @this.LongLength; ++i) {
        var item = @this.GetValue(i);
        if (item is TResult result)
          yield return result;
      }
    }

    [DebuggerStepThrough]
    public static object FirstOrDefault(this Array @this, Predicate<object> predicate, object defaultValue = null) {
      if (@this == null)
        throw new NullReferenceException();
      if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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

    #region comparison

#if UNSAFE

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceUnsafe(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
      fixed (byte* sourceFixedPointer = &source[sourceOffset])
      fixed (byte* targetFixedPointer = &target[targetOffset]) {
        var sourcePointer = sourceFixedPointer;
        var targetPointer = targetFixedPointer;

        const int THRESHOLD = 4;

        { // try 2048-Bit
          var localCount = count >> 8;
          if (localCount >= THRESHOLD) {
            var result = _SequenceEqual256Bytewise(ref sourcePointer, ref targetPointer, localCount);
            if (!result)
              return false;

            count &= 0b11111111;
            if (count == 0)
              return true;
          }
        }

#if !PLATFORM_X86
        { // try 512-Bit
          var localCount = count >> 6;
          if (localCount >= THRESHOLD) {
            var result = _SequenceEqual64Bytewise(ref sourcePointer, ref targetPointer, localCount);
            if (!result)
              return false;

            count &= 0b111111;
            if (count == 0)
              return true;
          }
        }
#endif

        { // try 256-Bit
          var localCount = count >> 5;
          if (localCount >= THRESHOLD) {
            var result = _SequenceEqual32Bytewise(ref sourcePointer, ref targetPointer, localCount);
            if (!result)
              return false;

            count &= 0b11111;
            if (count == 0)
              return true;
          }
        }

#if !PLATFORM_X86
        { // try 64-Bit
          var localCount = count >> 3;
          if (localCount >= THRESHOLD) {
            var result = _SequenceEqual8Bytewise(ref sourcePointer, ref targetPointer, localCount);
            if (!result)
              return false;

            count &= 0b111;
            if (count == 0)
              return true;
          }
        }
#endif

        { // try 32-Bit
          var localCount = count >> 2;
          if (localCount >= THRESHOLD) {
            var result = _SequenceEqual4Bytewise(ref sourcePointer, ref targetPointer, localCount);
            if (!result)
              return false;

            count &= 0b11;
            if (count == 0)
              return true;
          }
        }

        if (count > 0)
          return _SequenceEqualBytewise(ref sourcePointer, ref targetPointer, count);

        return true;
      }
    }

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqualBytewise(ref byte* source, ref byte* target, int count) {
      while (count > 0) {
        if (*source != *target)
          return false;

        ++source;
        ++target;
        --count;
      }

      return true;
    }

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual4Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block4*)s;
      var target = (Block4*)t;
      while (count > 0) {
        if (*source != *target)
          return false;

        ++source;
        ++target;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#if !PLATFORM_X86

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual8Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block8*)s;
      var target = (Block8*)t;

      while (count > 0) {
        if (*source != *target)
          return false;

        ++source;
        ++target;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#endif

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual32Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block32*)s;
      var target = (Block32*)t;

      while (count > 0) {
        if (
          (*source).a != (*target).a
          || (*source).b != (*target).b
          || (*source).c != (*target).c
          || (*source).d != (*target).d
          || (*source).e != (*target).e
          || (*source).f != (*target).f
          || (*source).g != (*target).g
          || (*source).h != (*target).h
        )
          return false;

        ++source;
        ++target;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#if !PLATFORM_X86

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual64Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block64*)s;
      var target = (Block64*)t;

      while (count > 0) {
        if (
          (*source).a != (*target).a
          || (*source).b != (*target).b
          || (*source).c != (*target).c
          || (*source).d != (*target).d
          || (*source).e != (*target).e
          || (*source).f != (*target).f
          || (*source).g != (*target).g
          || (*source).h != (*target).h
        )
          return false;

        ++source;
        ++target;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual256Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block64*)s;
      var target = (Block64*)t;

      while (count > 0) {
        if (
          (*source).a != (*target).a
          || (*source).b != (*target).b
          || (*source).c != (*target).c
          || (*source).d != (*target).d
          || (*source).e != (*target).e
          || (*source).f != (*target).f
          || (*source).g != (*target).g
          || (*source).h != (*target).h

          || source[1].a != target[1].a
          || source[1].b != target[1].b
          || source[1].c != target[1].c
          || source[1].d != target[1].d
          || source[1].e != target[1].e
          || source[1].f != target[1].f
          || source[1].g != target[1].g
          || source[1].h != target[1].h

          || source[2].a != target[2].a
          || source[2].b != target[2].b
          || source[2].c != target[2].c
          || source[2].d != target[2].d
          || source[2].e != target[2].e
          || source[2].f != target[2].f
          || source[2].g != target[2].g
          || source[2].h != target[2].h

          || source[3].a != target[3].a
          || source[3].b != target[3].b
          || source[3].c != target[3].c
          || source[3].d != target[3].d
          || source[3].e != target[3].e
          || source[3].f != target[3].f
          || source[3].g != target[3].g
          || source[3].h != target[3].h

        )
          return false;

        source += 4;
        target += 4;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#else

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static unsafe bool _SequenceEqual256Bytewise(ref byte* s, ref byte* t, int count) {
      var source = (Block32*)s;
      var target = (Block32*)t;

      while (count > 0) {
        if (
          (*source).a != (*target).a
          || (*source).b != (*target).b
          || (*source).c != (*target).c
          || (*source).d != (*target).d
          || (*source).e != (*target).e
          || (*source).f != (*target).f
          || (*source).g != (*target).g
          || (*source).h != (*target).h

          || source[1].a != target[1].a
          || source[1].b != target[1].b
          || source[1].c != target[1].c
          || source[1].d != target[1].d
          || source[1].e != target[1].e
          || source[1].f != target[1].f
          || source[1].g != target[1].g
          || source[1].h != target[1].h

          || source[2].a != target[2].a
          || source[2].b != target[2].b
          || source[2].c != target[2].c
          || source[2].d != target[2].d
          || source[2].e != target[2].e
          || source[2].f != target[2].f
          || source[2].g != target[2].g
          || source[2].h != target[2].h

          || source[3].a != target[3].a
          || source[3].b != target[3].b
          || source[3].c != target[3].c
          || source[3].d != target[3].d
          || source[3].e != target[3].e
          || source[3].f != target[3].f
          || source[3].g != target[3].g
          || source[3].h != target[3].h

          || source[4].a != target[4].a
          || source[4].b != target[4].b
          || source[4].c != target[4].c
          || source[4].d != target[4].d
          || source[4].e != target[4].e
          || source[4].f != target[4].f
          || source[4].g != target[4].g
          || source[4].h != target[4].h

          || source[5].a != target[5].a
          || source[5].b != target[5].b
          || source[5].c != target[5].c
          || source[5].d != target[5].d
          || source[5].e != target[5].e
          || source[5].f != target[5].f
          || source[5].g != target[5].g
          || source[5].h != target[5].h

          || source[6].a != target[6].a
          || source[6].b != target[6].b
          || source[6].c != target[6].c
          || source[6].d != target[6].d
          || source[6].e != target[6].e
          || source[6].f != target[6].f
          || source[6].g != target[6].g
          || source[6].h != target[6].h

          || source[7].a != target[7].a
          || source[7].b != target[7].b
          || source[7].c != target[7].c
          || source[7].d != target[7].d
          || source[7].e != target[7].e
          || source[7].f != target[7].f
          || source[7].g != target[7].g
          || source[7].h != target[7].h
        )
          return false;

        source += 8;
        target += 8;
        --count;
      }

      s = (byte*)source;
      t = (byte*)target;
      return true;
    }

#endif

#else

#if DEBUG && !PLATFORM_X86

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static bool _SequenceEqualManagedPointers(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
      using (var sourceFixedPointer = DisposableGCHandle.Pin(source))
      using (var targetFixedPointer = DisposableGCHandle.Pin(target)) {
        var sourcePointer = sourceFixedPointer.AddrOfPinnedObject();
        var targetPointer = targetFixedPointer.AddrOfPinnedObject();
        while (count >= 8) {
          if (Marshal.ReadInt64(sourcePointer, sourceOffset) != Marshal.ReadInt64(targetPointer, targetOffset))
            return false;

          sourceOffset += 8;
          targetOffset += 8;
          count -= 8;
        }

        while (count > 0) {
          if (Marshal.ReadByte(sourcePointer, sourceOffset) != Marshal.ReadByte(targetPointer, targetOffset))
            return false;

          ++sourceOffset;
          ++targetOffset;
          --count;
        }
      }

      return true;
    }

#endif

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static bool _SequenceEqualNaive(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
      while (count > 0) {
        if (source[sourceOffset] != target[targetOffset])
          return false;

        ++sourceOffset;
        ++targetOffset;
        --count;
      }

      return true;
    }

#endif

    private static bool _SequenceEqual(byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
#if UNSAFE
      return _SequenceUnsafe(source, sourceOffset, target, targetOffset, count);
#else
#if DEBUG && !PLATFORM_X86
      return _SequenceEqualManagedPointers(source, sourceOffset, target, targetOffset, count);
#else
      return _SequenceEqualNaive(source, sourceOffset, target, targetOffset, count);
#endif
#endif
    }

    public static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset, int count) {
      if (ReferenceEquals(source, target) && sourceOffset == targetOffset)
        return true;
      if (source == null || target == null)
        return false;

      var sourceLeft = source.Length - sourceOffset;
      var targetLeft = target.Length - targetOffset;
      if (sourceLeft < count)
        throw new ArgumentOutOfRangeException("Source has too few bytes left");

      if (targetLeft < count)
        throw new ArgumentOutOfRangeException("Target has too few bytes left");

      return _SequenceEqual(source, sourceOffset, target, targetOffset, sourceLeft);
    }

    public static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset) {
      if (ReferenceEquals(source, target) && sourceOffset == targetOffset)
        return true;
      if (source == null || target == null)
        return false;

      var sourceLeft = source.Length - sourceOffset;
      var targetLeft = target.Length - targetOffset;
      if (sourceLeft != targetLeft)
        return false;

      return _SequenceEqual(source, sourceOffset, target, targetOffset, sourceLeft);
    }

    public static bool SequenceEqual(this byte[] source, byte[] target) {
      if (ReferenceEquals(source, target))
        return true;
      if (source == null || target == null)
        return false;

      var sourceLeft = source.Length;
      var targetLeft = target.Length;
      if (sourceLeft != targetLeft)
        return false;

      if (sourceLeft == 0)
        return true;

      return _SequenceEqual(source, 0, target, 0, sourceLeft);
    }

    #endregion

    /// <summary>
    /// Creates random data in the given buffer; thus effectively overwriting it in-place.
    /// </summary>
    /// <param name="this">This buffer.</param>
    /// <returns>The given buffer</returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void RandomizeBuffer(this byte[] @this) => new RNGCryptoServiceProvider().GetBytes(@this);

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
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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

    #region compression

    /// <summary>
    /// GZips the given bytes.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>A GZipped byte array.</returns>
    [DebuggerStepThrough]
    public static byte[] GZip(this byte[] @this) {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
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

    #endregion

    #region Byte Array IndexOf

    private static int _GetInvalidIndex(byte[] _, byte[] __) => _INDEX_WHEN_NOT_FOUND;

    public static int IndexOfOrMinusOne(this byte[] @this, byte[] searchString, int offset = 0)
      => IndexOfOrDefault(@this, searchString, offset, _GetInvalidIndex)
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int defaultValue)
      => IndexOfOrDefault(@this, searchString, 0, (_, __) => defaultValue)
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<int> defaultValueFunc)
      => IndexOfOrDefault(@this, searchString, 0, (_, __) => defaultValueFunc())
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], int> defaultValueFunc)
      => IndexOfOrDefault(@this, searchString, 0, (t, _) => defaultValueFunc(t))
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], byte[], int> defaultValueFunc)
      => IndexOfOrDefault(@this, searchString, 0, defaultValueFunc)
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, int defaultValue)
      => IndexOfOrDefault(@this, searchString, offset, (_, __) => defaultValue)
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<int> defaultValueFunc)
      => IndexOfOrDefault(@this, searchString, offset, (_, __) => defaultValueFunc())
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], int> defaultValueFunc)
      => IndexOfOrDefault(@this, searchString, offset, (t, _) => defaultValueFunc(t))
    ;

    public static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], byte[], int> defaultValueFunc) {
      if (@this == null || searchString == null)
        throw new ArgumentNullException();

      if (ReferenceEquals(@this, searchString))
        return 0;

      var searchStringLength = searchString.Length;
      var dataStringLength = @this.Length;

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

    internal static int _ContainsNaïve(byte[] haystack, byte[] needle, int offset) {
      var searchStringLength = needle.Length;
      var dataStringLength = haystack.Length;

      for (var i = 0; i < dataStringLength; ++i) {
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
    internal static unsafe int _ContainsBNDM(byte[] haystack, byte[] needle, int offset) {
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

    internal static int _ContainsBoyerMoore(byte[] haystack, byte[] needle, int offset) {
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

    #region Array Clear

    public static void Clear(this byte[] @this) => _FillWithBytes(@this, 0, @this.Length, 0);

    public static void Clear(this ushort[] @this) {
#if UNSAFE
      unsafe {
        fixed (ushort* pointer = &@this[0])
          _FillWords(pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this short[] @this) {
#if UNSAFE
      unsafe {
        fixed (short* pointer = &@this[0])
          _FillWords((ushort*)pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this uint[] @this) {
#if UNSAFE
      unsafe {
        fixed (uint* pointer = &@this[0])
          _FillDWords(pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillDWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this int[] @this) {
#if UNSAFE
      unsafe {
        fixed (int* pointer = &@this[0])
          _FillDWords((uint*)pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillDWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this float[] @this) {
#if UNSAFE
      unsafe {
        fixed (float* pointer = &@this[0])
          _FillDWords((uint*)pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillDWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this ulong[] @this) {
#if UNSAFE
      unsafe {
        fixed (ulong* pointer = &@this[0])
          _FillQWords(pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillQWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this long[] @this) {
#if UNSAFE
      unsafe {
        fixed (long* pointer = &@this[0])
          _FillQWords((ulong*)pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillQWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    public static void Clear(this double[] @this) {
#if UNSAFE
      unsafe {
        fixed (double* pointer = &@this[0])
          _FillQWords((ulong*)pointer, @this.Length, 0);
      }
#else
      using (var sourceFixedPointer = DisposableGCHandle.Pin(@this)) {
        var pointer = sourceFixedPointer.AddrOfPinnedObject();
        _FillQWordPointer(pointer, 0, @this.Length, 0);
      }
#endif
    }

    #endregion

    #region Byte Array Fill

    public static void Fill(this byte[] @this, byte value) => _FillWithBytes(@this, 0, @this.Length, value);
    public static void Fill(this byte[] @this, byte value, int offset) {
      if (offset < 0 || offset > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithBytes(@this, offset, @this.Length - offset, value);
    }
    public static void Fill(this byte[] @this, byte value, int offset, int count) {
      if (offset < 0 || count < 0 || offset + count > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithBytes(@this, offset, count, value);
    }

    public static void Fill(this byte[] @this, Block2 value, int count) {
      if (count < 0 || count << 1 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithWords(@this, 0, count, value);
    }
    public static void Fill(this byte[] @this, Block2 value, int offset, int count) {
      if (offset < 0 || count < 0 || (offset + count) << 1 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithWords(@this, offset, count, value);
    }

    public static void Fill(this byte[] @this, Block4 value, int count) {
      if (count < 0 || count << 2 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithDWords(@this, 0, count, value);
    }
    public static void Fill(this byte[] @this, Block4 value, int offset, int count) {
      if (offset < 0 || count < 0 || (offset + count) << 2 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithDWords(@this, offset, count, value);
    }

    public static void Fill(this byte[] @this, Block8 value, int count) {
      if (count < 0 || count << 3 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithQWords(@this, 0, count, value);
    }
    public static void Fill(this byte[] @this, Block8 value, int offset, int count) {
      if (offset < 0 || count < 0 || (offset + count) << 3 > @this.Length)
        throw new ArgumentOutOfRangeException();

      _FillWithQWords(@this, offset, count, value);
    }

    public static void Fill(this IntPtr @this, byte value, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillBytePointer(@this, 0, count, value);
    }

    public static void Fill(this IntPtr @this, byte value, int offset, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillBytePointer(@this, offset, count, value);
    }

    public static void Fill(this IntPtr @this, Block2 value, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillWordPointer(@this, 0, count, value);
    }

    public static void Fill(this IntPtr @this, Block2 value, int offset, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillWordPointer(@this, offset, count, value);
    }

    public static void Fill(this IntPtr @this, Block4 value, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillDWordPointer(@this, 0, count, value);
    }

    public static void Fill(this IntPtr @this, Block4 value, int offset, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillDWordPointer(@this, offset, count, value);
    }

    public static void Fill(this IntPtr @this, Block8 value, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillQWordPointer(@this, 0, count, value);
    }


    public static void Fill(this IntPtr @this, Block8 value, int offset, int count) {
      if (count < 0)
        throw new ArgumentOutOfRangeException();

      _FillQWordPointer(@this, offset, count, value);
    }


#if UNSAFE

    private static unsafe void _FillWithBytes(byte[] source, int offset, int count, byte value) {
      fixed (byte* pointer = &source[offset])
        _FillBytes(pointer, count, value);
    }

    private static unsafe void _FillWithWords(byte[] source, int offset, int count, Block2 value) {
      fixed (byte* pointer = &source[offset << 1])
        _FillWords((Block2*)pointer, count, value);
    }

    private static unsafe void _FillWithDWords(byte[] source, int offset, int count, Block4 value) {
      fixed (byte* pointer = &source[offset << 2])
        _FillDWords((Block4*)pointer, count, value);
    }

    private static unsafe void _FillWithQWords(byte[] source, int offset, int count, Block8 value) {
      fixed (byte* pointer = &source[offset << 3])
        _FillQWords((Block8*)pointer, count, value);
    }

    private static unsafe void _FillBytePointer(IntPtr source, int offset, int count, byte value) => _FillBytes((byte*)source.ToPointer() + offset, count, value);
    private static unsafe void _FillWordPointer(IntPtr source, int offset, int count, Block2 value) => _FillWords((Block2*)source.ToPointer() + offset, count, value);
    private static unsafe void _FillDWordPointer(IntPtr source, int offset, int count, Block4 value) => _FillDWords((Block4*)source.ToPointer() + offset, count, value);
    private static unsafe void _FillQWordPointer(IntPtr source, int offset, int count, Block8 value) => _FillQWords((Block8*)source.ToPointer() + offset, count, value);

    private static unsafe void _FillBytes(byte* source, int count, byte value) {
      if (count >= 64) {
        var localCount = count >> 6;
        var localSource = (Block64*)source;

        _Fill64ByteBlocks(ref localSource, localCount, new Block64(value));
        count &= 0b111111;
        source = (byte*)localSource;
      }

      while (count >= 8) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source += 8;
        count -= 8;
      }

      while (--count >= 0)
        *source++ = value;
    }

    private static unsafe void _FillWords(Block2* source, int count, Block2 value) {
      if (count >= 64) {
        var localCount = count >> 5;
        var localSource = (Block64*)source;
        _Fill64ByteBlocks(ref localSource, localCount, new Block64(value));
        count &= 0b11111;
        source = (Block2*)localSource;
      }

      while (count >= 8) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source += 8;
        count -= 8;
      }

      while (--count >= 0)
        *source++ = value;
    }

    private static unsafe void _FillDWords(Block4* source, int count, Block4 value) {
      if (count >= 64) {
        var localCount = count >> 4;
        var localSource = (Block64*)source;
        _Fill64ByteBlocks(ref localSource, localCount, new Block64(value));
        count &= 0b1111;
        source = (Block4*)localSource;
      }

      while (count >= 8) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source += 8;
        count -= 8;
      }

      while (--count >= 0)
        *source++ = value;
    }

    private static unsafe void _FillQWords(Block8* source, int count, Block8 value) {
      if (count >= 64) {
        var localCount = count >> 3;
        var localSource = (Block64*)source;
        _Fill64ByteBlocks(ref localSource, localCount, new Block64(value));
        count &= 0b111;
        source = (Block8*)localSource;
      }

      while (count >= 8) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source += 8;
        count -= 8;
      }

      while (--count >= 0)
        *source++ = value;
    }

    private static unsafe void _Fill64ByteBlocks(ref Block64* source, int count, Block64 value) {
      while (count >= 16) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source[8] = value;
        source[9] = value;
        source[10] = value;
        source[11] = value;
        source[12] = value;
        source[13] = value;
        source[14] = value;
        source[15] = value;
        source += 16;
        count -= 16;
      }

      if (count >= 8) {
        *source = value;
        source[1] = value;
        source[2] = value;
        source[3] = value;
        source[4] = value;
        source[5] = value;
        source[6] = value;
        source[7] = value;
        source += 8;
        count -= 8;
      }

      while (--count >= 0)
        *source++ = value;
    }

#else // Managed stuff

    private static void _FillWithBytes(byte[] source, int offset, int count, byte value) {
      using (var sourceFixedPointer = DisposableGCHandle.Pin(source))
        _FillBytePointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithWords(byte[] source, int offset, int count, Block2 value) {
      using (var sourceFixedPointer = DisposableGCHandle.Pin(source))
        _FillWordPointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithDWords(byte[] source, int offset, int count, Block4 value) {
      using (var sourceFixedPointer = DisposableGCHandle.Pin(source))
        _FillDWordPointer(sourceFixedPointer.AddrOfPinnedObject(), offset, count, value);
    }

    private static void _FillWithQWords(byte[] source, int offset, int count, Block8 value) {
      offset <<= 3;
      using (var sourceFixedPointer = DisposableGCHandle.Pin(source))
        _FillWithQWords(sourceFixedPointer.AddrOfPinnedObject(), ref offset, count, value);
    }

    private static void _FillBytePointer(IntPtr source, int offset, int count, byte value) {
      if (count >= 8) {
        var localCount = count >> 3;
        _FillWithQWords(source, ref offset, localCount, (Block8)value << 56 | (Block8)value << 48 | (Block8)value << 40 | (Block8)value << 32 | (Block8)value << 24 | (Block8)value << 16 | (Block8)value << 8 | value);
        count &= 0b111;
      }

      while (--count >= 0)
        Marshal.WriteByte(source, offset++, value);
    }

    private static void _FillWordPointer(IntPtr source, int offset, int count, Block2 value) {
      offset = offset << 1;

      if (count >= 4) {
        var localCount = count >> 2;
        _FillWithQWords(source, ref offset, localCount, (Block8)value << 32 | (Block8)value << 24 | (Block8)value << 16 | value);
        count &= 0b11;
      }

      while (--count >= 0) {
        Marshal.WriteInt16(source, offset, (short)value);
        offset += 2;
      }
    }

    private static void _FillDWordPointer(IntPtr source, int offset, int count, Block4 value) {
      offset = offset << 2;

      if (count >= 2) {
        var localCount = count >> 1;
        _FillWithQWords(source, ref offset, localCount, (Block8)value << 32 | value);
        count &= 0b1;
      }

      while (--count >= 0) {
        Marshal.WriteInt32(source, offset, (int)value);
        offset += 4;
      }
    }

    private static void _FillQWordPointer(IntPtr source, int offset, int count, Block8 value) {
      offset <<= 3;
      _FillWithQWords(source, ref offset, count, value);
    }

#if !MONO
    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
    private static extern IntPtr _MemoryCopy(IntPtr dst, IntPtr src, int count);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
    private static extern void __MemoryFill(IntPtr dst, int value, int count);
    private static void _MemoryFill(IntPtr dst, byte value, int count) => __MemoryFill(dst, value, count);
#endif

    private static void _FillWithQWords(IntPtr source, ref int offset, int count, Block8 value) {
      var v = (long)value;
      source += offset;
      offset += count << 3;

#if !MONO
      if (count >= 64) {
        Marshal.WriteInt64(source, 0, v);
        Marshal.WriteInt64(source, 8, v);
        Marshal.WriteInt64(source, 16, v);
        Marshal.WriteInt64(source, 24, v);
        Marshal.WriteInt64(source, 32, v);
        Marshal.WriteInt64(source, 40, v);
        Marshal.WriteInt64(source, 48, v);
        Marshal.WriteInt64(source, 56, v);

        var sizeInBytes = 64;
        var start = source;
        source += sizeInBytes;
        count -= 8;

        var countInBytes = count << 3;
        while (countInBytes > sizeInBytes) {
          _MemoryCopy(source, start, sizeInBytes);
          source += sizeInBytes;
          countInBytes -= sizeInBytes;
          sizeInBytes <<= 1;
        }
        _MemoryCopy(source, start, countInBytes);
        return;
      }
#endif

      while (count >= 8) {
        Marshal.WriteInt64(source, 0, v);
        Marshal.WriteInt64(source, 8, v);
        Marshal.WriteInt64(source, 16, v);
        Marshal.WriteInt64(source, 24, v);
        Marshal.WriteInt64(source, 32, v);
        Marshal.WriteInt64(source, 40, v);
        Marshal.WriteInt64(source, 48, v);
        Marshal.WriteInt64(source, 56, v);
        source += 64;
        count -= 8;
      }

      while (count > 0) {
        Marshal.WriteInt64(source, 0, v);
        source += 8;
        --count;
      }
    }

#endif

    #endregion

    #region hash computation

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <typeparam name="THashAlgorithm">The type of the hash algorithm.</typeparam>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The result of the hash algorithm</returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeHash<THashAlgorithm>(this byte[] @this) where THashAlgorithm : HashAlgorithm, new() {
      if (@this == null)
        throw new NullReferenceException();
#if NET40_OR_GREATER
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
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA512Hash(this byte[] @this) => @this.ComputeHash<SHA512CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA384 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA384Hash(this byte[] @this) => @this.ComputeHash<SHA384CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA256 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA256Hash(this byte[] @this) => @this.ComputeHash<SHA256CryptoServiceProvider>();

    /// <summary>
    /// Calculates the SHA-1 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static byte[] ComputeSHA1Hash(this byte[] @this) => @this.ComputeHash<SHA1CryptoServiceProvider>();

    /// <summary>
    /// Calculates the MD5 hash.
    /// </summary>
    /// <param name="this">This Byte-Array.</param>
    /// <returns>The hash</returns>
#if NET45_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];

#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] &= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] |= operand[i];
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] ^= 0xffff;
#if NET40_OR_GREATER
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
#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];
#if NET40_OR_GREATER
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

#if NET40_OR_GREATER
        if (source.Length < RuntimeConfiguration.MIN_ITEMS_FOR_PARALELLISM) {
#endif
          for (var i = 0; i < source.Length; i++)
            source[i] = ~source[i];
#if NET40_OR_GREATER
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

    #region bitwise operations

#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void AndBytewise(this byte[] @this, byte[] operand) => FastAnd.ProcessBytewise(@this, operand);

#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void OrBytewise(this byte[] @this, byte[] operand) => FastOr.ProcessBytewise(@this, operand);

#if NET45_OR_GREATER
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

#if NET45_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [DebuggerStepThrough]
    public static void NotBytewise(this byte[] @this) => FastNot.ProcessBytewise(@this);

    #endregion

    #endregion

  }
}