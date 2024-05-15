namespace System;

using Collections;
using Collections.Generic;

internal static partial class SpanHelper {

  /// <summary>
  /// Initializes a new instance of the <see cref="Enumerator{T}"/> class, which can iterate over a portion of a memory buffer managed by an <see cref="IMemoryHandler{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements in the memory buffer.</typeparam>
  /// <param name="source">The memory handler that provides access to the memory buffer.</param>
  /// <param name="length">The number of elements from the buffer to enumerate. This length should not exceed the actual available length in the provided memory handler.</param>
  public sealed class Enumerator<T>(IMemoryHandler<T> source, int length) : IEnumerator<T> {

    private const int INDEX_RESET = -1;
    
    private int _index = INDEX_RESET;

    public void Reset() => this._index = INDEX_RESET;
    object IEnumerator.Current => this.Current;

    public bool MoveNext() => ++this._index < length;
    public T Current => source[this._index];

    #region Implementation of IDisposable

    public void Dispose() { }

    #endregion

  }
}
