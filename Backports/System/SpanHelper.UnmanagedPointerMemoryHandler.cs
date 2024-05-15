namespace System;

internal static unsafe partial class SpanHelper {
  
  /// <summary>
  /// Represents a memory handler for unmanaged memory blocks, providing access to elements of type <typeparamref name="T"/> using a pointer.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the unmanaged memory block.</typeparam>
  /// <remarks>
  /// This class enables managed code to read from and write to a block of unmanaged memory. It is crucial to ensure that the lifecycle of the memory block
  /// is properly managed to avoid memory leaks or accessing invalid memory locations.
  /// </remarks>
#pragma warning disable CS8500
  public sealed class UnmanagedPointerMemoryHandler<T>(T* pointer) : PointerMemoryHandlerBase<T>(pointer) {
    
    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override IMemoryHandler<T> SliceFrom(int offset) => new UnmanagedPointerMemoryHandler<T>(this.Pointer + offset);

    #endregion

  }
}
