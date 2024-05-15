namespace System;

internal static partial class SpanHelper {
  public interface IMemoryHandler<T> {
    public ref T this[int index] { get; }
    public IMemoryHandler<T> SliceFrom(int offset);
    public void CopyTo(IMemoryHandler<T> other, int length);
    public void CopyTo(T[] target, int count);
  }
}
