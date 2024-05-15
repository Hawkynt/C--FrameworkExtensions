#if !SUPPORTS_SPAN

namespace System;

using Collections;
using Collections.Generic;

public readonly struct ReadOnlySpan<T> : IEnumerable<T> {
  private readonly SpanHelper.MemoryHandlerBase<T> _memoryHandler;

  private ReadOnlySpan(SpanHelper.MemoryHandlerBase<T> handler, int length) {
    this._memoryHandler = handler;
    this.Length = length;
  }

  public ReadOnlySpan(T[] array) : this(array, 0, array?.Length ?? 0) { }
  
  public ReadOnlySpan(T[] array, int start, int length):this(new SpanHelper.PinnedArrayMemoryHandler<T>(array, start), length) { }

#pragma warning disable CS8500
  public unsafe ReadOnlySpan(void* pointer, int length):this(new SpanHelper.UnmanagedMemoryHandler<T>((T*)pointer),length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public ref readonly T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        throw new ArgumentOutOfRangeException();
      
      return ref this._memoryHandler[index];
    }
  }

  public ReadOnlySpan<T> Slice(int start, int length) {
    if (start < 0 || length < 0 || start + length > this.Length)
      throw new ArgumentOutOfRangeException();

    return new(this._memoryHandler.SliceFrom(start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this._memoryHandler.CopyTo(array, this.Length);
    
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

  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this._memoryHandler, @this.Length);
  public static implicit operator ReadOnlySpan<T>(T[] array) => new(array);
  
}

#endif
