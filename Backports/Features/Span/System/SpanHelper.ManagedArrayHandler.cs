#region (c)2010-2042 Hawkynt

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

#endregion

#if !SUPPORTS_SPAN

namespace System;

internal static partial class SpanHelper {
  /// <summary>
  ///   Provides a managed array implementation of <see cref="IMemoryHandler{T}" />, allowing for array operations and
  ///   manipulations based on the <see cref="IMemoryHandler{T}" /> interface.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the managed array.</typeparam>
  /// <remarks>
  ///   This class manages an array segment by providing direct access and manipulation capabilities over a portion of an
  ///   array, beginning at a specified index.
  /// </remarks>
  public class ManagedArrayHandler<T>(T[] source, int start) : IMemoryHandler<T> {
    #region Implementation of IMemoryHandler<T>

    /// <inheritdoc />
    public ref T this[int index] => ref source[start + index];

    /// <inheritdoc />
    public IMemoryHandler<T> SliceFrom(int offset) => new ManagedArrayHandler<T>(source, start + offset);

    /// <inheritdoc />
    public unsafe void CopyTo(IMemoryHandler<T> other, int length) {
      if (length == 0)
        return;

      fixed (T* token = &source[start]) {
        var sourcePointer = token;
        var numberOfFullBlocks = length >> 3;
        var i = 0;

        switch (length & 7) {
          case 0: goto case_8or0;
          case 1: goto case_1;
          case 2: goto case_2;
          case 3: goto case_3;
          case 4: goto case_4;
          case 5: goto case_5;
          case 6: goto case_6;
          case 7: goto case_7;
          default: return; /* can not happen due to (length & 7) */
        }

        case_8or0:
        if (numberOfFullBlocks-- <= 0)
          return;

        other[i++] = *sourcePointer++;
        case_7: other[i++] = *sourcePointer++;
        case_6: other[i++] = *sourcePointer++;
        case_5: other[i++] = *sourcePointer++;
        case_4: other[i++] = *sourcePointer++;
        case_3: other[i++] = *sourcePointer++;
        case_2: other[i++] = *sourcePointer++;
        case_1: other[i++] = *sourcePointer++;

        goto case_8or0;
      }
    }

    /// <inheritdoc />
    public void CopyTo(T[] target, int count) => Array.Copy(source, start, target, 0, count);

    #endregion
  }
}

#endif
