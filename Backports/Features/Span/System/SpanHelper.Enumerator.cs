﻿#region (c)2010-2042 Hawkynt

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

using System.Collections;
using System.Collections.Generic;

namespace System;

partial class SpanHelper {
  /// <summary>
  ///   Initializes a new instance of the <see cref="Enumerator{T}" /> class, which can iterate over a portion of a memory
  ///   buffer managed by an <see cref="MemoryHandlerBase{T}" />.
  /// </summary>
  /// <typeparam name="T">The type of elements in the memory buffer.</typeparam>
  /// <param name="source">The memory handler that provides access to the memory buffer.</param>
  /// <param name="length">
  ///   The number of elements from the buffer to enumerate. This length should not exceed the actual
  ///   available length in the provided memory handler.
  /// </param>
  public sealed class Enumerator<T>(MemoryHandlerBase<T> source, int length) : IEnumerator<T> {
    private const int INDEX_RESET = -1;

    private int _index = INDEX_RESET;

    public void Reset() => this._index = INDEX_RESET;
    object IEnumerator.Current => this.Current;

    public bool MoveNext() => ++this._index < length;
    public T Current => source.GetValue(this._index);

    #region Implementation of IDisposable

    public void Dispose() { }

    #endregion
  }
}

#endif
