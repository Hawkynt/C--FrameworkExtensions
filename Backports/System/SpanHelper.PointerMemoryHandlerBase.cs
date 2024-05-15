#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

using Runtime.InteropServices;

internal static unsafe partial class SpanHelper {

  /// <summary>
  /// Provides a base class for handling memory using a pointer to a block of memory of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type of elements pointed to by the memory.</typeparam>
  /// <remarks>
  /// This class is intended to be inherited by more specific memory handler implementations that operate on pointers.
  /// It provides common functionality and the basic structure needed to work with unmanaged memory in a managed environment,
  /// adhering to the <see cref="IMemoryHandler{T}"/> interface.
  /// </remarks>
  public abstract class PointerMemoryHandlerBase<T>(T* pointer): IMemoryHandler<T> {

    protected T* Pointer { get; } = pointer;

    private void CopyTo(PointerMemoryHandlerBase<T> other, int length) {
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof(length));
      
      var source = this.Pointer;
      var target = other.Pointer;
#if SUPPORTS_BUFFER_MEMORYCOPY
      var totalBytes = Marshal.SizeOf<T>() * length;
      Buffer.MemoryCopy(source, target, totalBytes, totalBytes);
#else
      if (length < 128)
        for (;;)
          switch (length) {
            case 0:
              return;
            case 1:
              *target = *source;
              goto case 0;
            case 2:
              target[1] = source[1];
              goto case 1;
            case 3:
              target[2] = source[2];
              goto case 2;
            case 4:
              target[3] = source[3];
              goto case 3;
            case 5:
              target[4] = source[4];
              goto case 4;
            case 6:
              target[5] = source[5];
              goto case 5;
            case 7:
              target[6] = source[6];
              goto case 6;
            default:
              do {
                *target = *source;
                target[1] = source[1];
                target[2] = source[2];
                target[3] = source[3];
                target[4] = source[4];
                target[5] = source[5];
                target[6] = source[6];
                target[7]= source[7];
                length -= 8;
                source += 8;
                target += 8;
              } while (length >= 8);

              continue;
          }

      var totalBytes = Marshal.SizeOf(typeof(T)) * length;
      var byteSource = (byte*)source;
      var byteTarget = (byte*)target;

      // Copy chunks of 8 bytes (ulong) at a time
      while (totalBytes >= 8) {
        *(long*)byteTarget = *(long*)byteSource;
        totalBytes -= 8;
        byteTarget += 8;
        byteSource += 8;
      }

      // Copy remaining 4 bytes (uint) if possible
      if (totalBytes >= 4) {
        *(int*)byteTarget = *(int*)byteSource;
        totalBytes -= 4;
        byteTarget += 4;
        byteSource += 4;
      }

      // Copy remaining 2 bytes (ushort) if possible
      if (totalBytes >= 2) {
        *(short*)byteTarget = *(short*)byteSource;
        totalBytes -= 2;
        byteTarget += 2;
        byteSource += 2;
      }

      // Copy remaining byte if necessary
      if (totalBytes >= 1)
        *byteTarget = *byteSource;

#endif
    }

    /// <inheritdoc />
    public ref T this[int index] => ref this.Pointer[index];

    /// <inheritdoc />
    public void CopyTo(T[] target, int count) {
      var pointer = this.Pointer;
      for (var i = 0; i < count; ++i)
        target[i] = pointer[i];
    }

    /// <inheritdoc />
    public abstract IMemoryHandler<T> SliceFrom(int offset);

    /// <inheritdoc />
    public void CopyTo(IMemoryHandler<T> other, int length) {
      if(other is PointerMemoryHandlerBase<T> pointerBased)
        this.CopyTo(pointerBased, length);

      for (var i = 0; i < length; ++i)
        other[i] = this[i];
    }
  }
}
