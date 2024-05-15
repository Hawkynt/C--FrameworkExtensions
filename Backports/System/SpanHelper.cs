using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace System;
internal static unsafe class SpanHelper {
  
  public abstract class MemoryHandlerBase<T> {
    protected MemoryHandlerBase(T* pointer) {
      this.Pointer = pointer;
    }

    protected T* Pointer { get; init; }

    public void CopyTo(MemoryHandlerBase<T> other, int length) {
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

    public ref T this[int index] => ref this.Pointer[index];

    public void CopyTo(T[] target, int count) {
      var pointer = this.Pointer;
      for (var i = 0; i < count; ++i)
        target[i] = pointer[i];
    }

    public abstract MemoryHandlerBase<T> SliceFrom(int offset);

  }
  
  public sealed class PinnedArrayMemoryHandler<T> : MemoryHandlerBase<T> {

    private sealed class AnotherHandler : MemoryHandlerBase<T> {

#pragma warning disable IDE0052
      // ReSharper disable once NotAccessedField.Local
      // This "unused" field is intentionally retained to prevent the source from being garbage collected. By holding a reference to the source, we ensure that any pinned pointers to managed objects remain valid, as it prevents the garbage collector from relocating these objects, which would otherwise change their memory addresses and invalidate the pointers.
      private readonly MemoryHandlerBase<T> _source;
#pragma warning restore IDE0052

      public AnotherHandler(MemoryHandlerBase<T> source, T* pointer):base(pointer) => this._source = source;

      #region Overrides of MemoryHandlerBase<T>

      /// <inheritdoc />
      public override MemoryHandlerBase<T> SliceFrom(int offset) => new AnotherHandler(this, this.Pointer + offset);

      #endregion
    }
    
    private GCHandle _handle;

    public PinnedArrayMemoryHandler(T[] array, int start):base(null) {
      this._handle = GCHandle.Alloc(array, GCHandleType.Pinned);
      this.Pointer = (T*)Marshal.UnsafeAddrOfPinnedArrayElement(array, start);
    }

    ~PinnedArrayMemoryHandler() {
      if (this._handle.IsAllocated)
        this._handle.Free();
    }

    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override MemoryHandlerBase<T> SliceFrom(int offset) => new AnotherHandler(this, this.Pointer + offset);

    #endregion
  }

  public sealed class UnmanagedMemoryHandler<T> : MemoryHandlerBase<T> {
    public UnmanagedMemoryHandler(T* pointer) : base(pointer) { }

    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override MemoryHandlerBase<T> SliceFrom(int offset) => new UnmanagedMemoryHandler<T>(this.Pointer + offset);

    #endregion
  }

  public sealed class Enumerator<T>(MemoryHandlerBase<T> source, int length) : IEnumerator<T> {
    private int _index;

    public void Reset() => this._index = -1;
    object IEnumerator.Current => this.Current;

    public bool MoveNext() => ++this._index < length;
    public T Current => source[this._index];

    #region Implementation of IDisposable

    public void Dispose() { }

    #endregion

  }
  
}
