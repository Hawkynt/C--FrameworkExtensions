#if !SUPPORTS_SPAN

namespace System;

using Collections;
using Collections.Generic;

public readonly struct Span<T> : IEnumerable<T> {
  internal readonly SpanHelper.MemoryHandlerBase<T> _memoryHandler;

  private Span(SpanHelper.MemoryHandlerBase<T> handler, int length) {
    this._memoryHandler = handler;
    this.Length = length;
  }

  public Span(T[] array) : this(array, 0, array?.Length ?? 0) { }

  public Span(T[] array, int start, int length) : this(new SpanHelper.ArrayMemoryHandler<T>(array, start), length) { }

#pragma warning disable CS8500
  public unsafe Span(void* pointer, int length) : this(new SpanHelper.UnmanagedMemoryHandler<T>((T*)pointer), length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public ref T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        throw new ArgumentOutOfRangeException();

      return ref this._memoryHandler[index];
    }
  }

  public Span<T> Slice(int start, int length) {
    if (start < 0 || length < 0 || start + length > this.Length)
      throw new ArgumentOutOfRangeException();

    return new(new SpanHelper.AnotherHandler<T>(this._memoryHandler, start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this._memoryHandler.CopyTo(array,this.Length);
    
    return array;
  }

  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      throw new ArgumentOutOfRangeException();

    this._memoryHandler.CopyTo(other._memoryHandler, this.Length);
  }

  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this._memoryHandler.CopyTo(other._memoryHandler, this.Length);
    return true;
  }
  
  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this._memoryHandler, this.Length);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

}

#endif
