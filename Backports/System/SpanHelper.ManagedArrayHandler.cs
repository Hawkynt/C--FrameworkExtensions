namespace System;

internal static partial class SpanHelper {

  /// <summary>
  /// Provides a managed array implementation of <see cref="IMemoryHandler{T}"/>, allowing for array operations and manipulations based on the <see cref="IMemoryHandler{T}"/> interface.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the managed array.</typeparam>
  /// <remarks>
  /// This class manages an array segment by providing direct access and manipulation capabilities over a portion of an array, beginning at a specified index.
  /// </remarks>
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
