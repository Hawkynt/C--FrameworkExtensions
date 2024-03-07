using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace System;
internal static unsafe class SpanHelper {
  
  public abstract class MemoryHandlerBase<T> {
    #region Implementation of IMemoryHandler<T>

    public T* Pointer {
      get;
      init;
    }

    #endregion

    public void CopyTo(MemoryHandlerBase<T> other, int length) {
      var source = this.Pointer;
      var target = other.Pointer;
      for (var i = 0; i < length; ++i)
        target[i] = source[i];
    }

    public ref T this[int index] => ref this.Pointer[index];

    public void CopyTo(T[] target, int count) {
      var pointer = this.Pointer;
      for (var i = 0; i < count; ++i)
        target[i] = pointer[i];
    }

  }
  
  public sealed class ArrayMemoryHandler<T> : MemoryHandlerBase<T> {
    private GCHandle _handle;

    public ArrayMemoryHandler(T[] array, int start) {
      this._handle = GCHandle.Alloc(array, GCHandleType.Pinned);
      this.Pointer = (T*)Marshal.UnsafeAddrOfPinnedArrayElement(array, start);
    }

    ~ArrayMemoryHandler() {
      if (this._handle.IsAllocated)
        this._handle.Free();
    }
  }

  public sealed class UnmanagedMemoryHandler<T> : MemoryHandlerBase<T> {
    public UnmanagedMemoryHandler(T* pointer) => this.Pointer = pointer;
  }

  public sealed class AnotherHandler<T> : MemoryHandlerBase<T> {

#pragma warning disable IDE0052
    // ReSharper disable once NotAccessedField.Local
    // This "unused" field is intentionally retained to prevent the source from being garbage collected. By holding a reference to the source, we ensure that any pinned pointers to managed objects remain valid, as it prevents the garbage collector from relocating these objects, which would otherwise change their memory addresses and invalidate the pointers.
    private readonly MemoryHandlerBase<T> _source;
#pragma warning restore IDE0052

    public AnotherHandler(MemoryHandlerBase<T> source, int offset) {
      this._source = source;
      this.Pointer = source.Pointer + offset;
    }
  }

  public sealed class Enumerator<T> : IEnumerator<T> {
    private readonly MemoryHandlerBase<T> _source;
    private readonly int _length;
    private int _index;
    public Enumerator(MemoryHandlerBase<T> source, int length) {
      this._source = source;
      this._length = length;
    }

    public void Reset() => this._index = -1;
    object IEnumerator.Current => this.Current;

    public bool MoveNext() => ++this._index < this._length;
    public T Current => this._source.Pointer[this._index];

    #region Implementation of IDisposable

    public void Dispose() { }

    #endregion
  }
  
}
