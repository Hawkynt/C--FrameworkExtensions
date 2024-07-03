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

using Diagnostics;
using Guard;

public static partial class ArrayExtensions {
  [DebuggerDisplay("{" + nameof(ReadOnlyArraySlice<TItem>.ToString) + "()}")]
  public class ArraySlice<TItem>(TItem[] source, int start, int length) : ReadOnlyArraySlice<TItem>(source, start, length) {
    /// <summary>
    ///   Gets or sets the <see cref="TItem" /> at the specified index.
    /// </summary>
    /// <value>
    ///   The <see cref="TItem" />.
    /// </value>
    /// <param name="index">The index.</param>
    /// <returns>The item at the given index</returns>
    public new TItem this[int index] {
      get {
        Against.IndexOutOfRange(index, this.Length);

        return this._source[index + this._start];
      }
      set {
        Against.IndexOutOfRange(index, this.Length);

        this._source[index + this._start] = value;
      }
    }

    /// <summary>
    ///   Slices the specified array.
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

      return new(this._source, start + this._start, length);
    }
  }
}
