#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

using Runtime.InteropServices;

internal static unsafe partial class SpanHelper {
  public sealed class ManagedArrayPin<T>(GCHandle handle, T* pointer) {

    public T* Pointer { get; } = pointer;

    ~ManagedArrayPin() {
      if (handle.IsAllocated)
        handle.Free();
    }

    public static bool TryPin(T[] array, out ManagedArrayPin<T> instance) {
      try {
        instance = new(GCHandle.Alloc(array, GCHandleType.Pinned), (T*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0));
        return true;
      } catch (Exception) {
        instance = null;
        return false;
      }
    }

  }
}
