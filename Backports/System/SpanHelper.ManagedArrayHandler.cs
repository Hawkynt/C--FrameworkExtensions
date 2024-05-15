namespace System;

internal static partial class SpanHelper {

  public class ManagedArrayHandler<T>(T[] source, int start) : IMemoryHandler<T> {
    #region Implementation of IMemoryHandler<T>

    /// <inheritdoc />
    public ref T this[int index] => ref source[start + index];

    /// <inheritdoc />
    public IMemoryHandler<T> SliceFrom(int offset) => new ManagedArrayHandler<T>(source, start + offset);

    /// <inheritdoc />
    public void CopyTo(IMemoryHandler<T> other, int length) {
      for (int i = 0, offset = start; i < length; ++offset, ++i)
        other[i] = source[offset];
    }

    /// <inheritdoc />
    public void CopyTo(T[] target, int count) => Array.Copy(source, start, target, 0, count);

    #endregion
  }

}
