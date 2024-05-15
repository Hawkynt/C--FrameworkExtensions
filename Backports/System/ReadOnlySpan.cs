﻿#if !SUPPORTS_SPAN

namespace System;

using Collections;
using Collections.Generic;

public readonly struct ReadOnlySpan<T> : IEnumerable<T> {
  private readonly SpanHelper.IMemoryHandler<T> _pointerMemoryHandler;

  private ReadOnlySpan(SpanHelper.IMemoryHandler<T> handler, int length) {
    this._pointerMemoryHandler = handler;
    this.Length = length;
  }

  public ReadOnlySpan(T[] array) : this(array, 0, array?.Length ?? 0) { }
  
  public ReadOnlySpan(T[] array, int start, int length) : this(SpanHelper.PinnedArrayMemoryHandler<T>.FromManagedArray(array, start), length) { }

#pragma warning disable CS8500
  public unsafe ReadOnlySpan(void* pointer, int length):this(new SpanHelper.UnmanagedPointerMemoryHandler<T>((T*)pointer),length) { }
#pragma warning restore CS8500

  public int Length { get; }

  public bool IsEmpty => this.Length == 0;

  public ref readonly T this[int index] {
    get {
      if ((uint)index >= (uint)this.Length)
        throw new ArgumentOutOfRangeException();
      
      return ref this._pointerMemoryHandler[index];
    }
  }

  public ReadOnlySpan<T> Slice(int start, int length) {
    if (start < 0 || length < 0 || start + length > this.Length)
      throw new ArgumentOutOfRangeException();

    return new(this._pointerMemoryHandler.SliceFrom(start), length);
  }

  public T[] ToArray() {
    var array = new T[this.Length];
    this._pointerMemoryHandler.CopyTo(array, this.Length);
    
    return array;
  }

  public void CopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      throw new ArgumentOutOfRangeException();

    this._pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
  }

  public bool TryCopyTo(Span<T> other) {
    var length = this.Length;
    if (other.Length < length)
      return false;

    this._pointerMemoryHandler.CopyTo(other.pointerMemoryHandler, this.Length);
    return true;
  }

  public IEnumerator<T> GetEnumerator() => new SpanHelper.Enumerator<T>(this._pointerMemoryHandler, this.Length);
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public static implicit operator ReadOnlySpan<T>(Span<T> @this) => new(@this.pointerMemoryHandler, @this.Length);
  public static implicit operator ReadOnlySpan<T>(T[] array) => new(array);
  
}

#endif
