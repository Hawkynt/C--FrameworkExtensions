namespace System;

internal static unsafe partial class SpanHelper {
#pragma warning disable CS8500
  public sealed class UnmanagedPointerMemoryHandler<T>(T* pointer) : PointerMemoryHandlerBase<T>(pointer) {
    
    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override IMemoryHandler<T> SliceFrom(int offset) => new UnmanagedPointerMemoryHandler<T>(this.Pointer + offset);

    #endregion

  }
}
