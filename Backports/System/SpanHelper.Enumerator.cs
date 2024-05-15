namespace System;

using Collections;
using Collections.Generic;

internal static partial class SpanHelper {

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
