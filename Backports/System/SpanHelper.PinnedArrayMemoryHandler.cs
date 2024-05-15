#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

internal static unsafe partial class SpanHelper {
  public sealed class PinnedArrayMemoryHandler<T> : PointerMemoryHandlerBase<T> {

    private readonly ManagedArrayPin<T> _pin;
    
    private PinnedArrayMemoryHandler(ManagedArrayPin<T> pin, T* pointer) : base(pointer) => this._pin = pin;
    
    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override IMemoryHandler<T> SliceFrom(int offset) => new PinnedArrayMemoryHandler<T>(this._pin, this.Pointer + offset);

    #endregion

    public static IMemoryHandler<T> FromManagedArray(T[] array, int start) 
      => ManagedArrayPin<T>.TryPin(array, out var pin) 
        ? new PinnedArrayMemoryHandler<T>(pin, pin.Pointer + start) 
        : new ManagedArrayHandler<T>(array, start)
    ;
  }
}
