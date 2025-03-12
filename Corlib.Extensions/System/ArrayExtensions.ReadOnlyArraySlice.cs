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

namespace System;

using Collections;
using Collections.Generic;
using Diagnostics;
using Guard;

public static partial class ArrayExtensions {
  [DebuggerDisplay("{" + nameof(ToString) + "()}")]
  public class ReadOnlyArraySlice<TItem> : IEnumerable<TItem> {
    protected readonly TItem[] _source;
    protected readonly int _start;

    public ReadOnlyArraySlice(TItem[] source, int start, int length) {
      Against.ThisIsNull(source);

      if (start + length > source.Length)
        AlwaysThrow.ArgumentException(nameof(length), "Exceeding source length");

      this._source = source;
      this._start = start;
      this.Length = length;
    }

    /// <summary>
    ///   Gets the number of elements in this slice.
    /// </summary>
    /// <value>
    ///   The length.
    /// </value>
    public int Length { get; }

    /// <summary>
    ///   Gets the <see cref="TItem" /> at the specified index.
    /// </summary>
    /// <value>
    ///   The <see cref="TItem" />.
    /// </value>
    /// <param name="index">The index.</param>
    /// <returns>The item at the given index.</returns>
    public TItem this[int index] {
      get {
        Against.IndexOutOfRange(index, this.Length);

        return this._source[index + this._start];
      }
    }

    /// <summary>
    ///   Gets or sets the <see cref="TItem" /> at the specified index.
    /// </summary>
    /// <param name="index">The index, which can be from start or end (^).</param>
    /// <returns>The item at the given index</returns>
    public TItem this[Index index] {
      get {
        var actualIndex = index.IsFromEnd ? this.Length - index.Value : index.Value;
        return this[actualIndex];
      }
    }

    /// <summary>
    ///   Gets a slice of the array using range syntax.
    /// </summary>
    /// <param name="range">The range to slice.</param>
    /// <returns>An array slice representing the specified range.</returns>
    public ReadOnlyArraySlice<TItem> this[Range range] {
      get {
        var (offset, rangeLength) = range.GetOffsetAndLength(this.Length);
        return this.ReadOnlySlice(offset, rangeLength);
      }
    }

    /// <summary>
    ///   Gets the values.
    /// </summary>
    /// <value>
    ///   The values.
    /// </value>
    public IEnumerable<TItem> Values {
      get {
        var maxIndex = this._start + this.Length;
        for (var i = this._start; i < maxIndex; ++i)
          yield return this._source[i];
      }
    }

    /// <summary>
    ///   Slices the specified array.
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
    ///   Copies this slice into a new array.
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
}
