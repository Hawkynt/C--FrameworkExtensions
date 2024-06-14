#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_SPAN

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System;

internal static unsafe partial class SpanHelper {
  /// <summary>
  ///   Provides a memory handler that pins a managed array in memory, allowing for safe and performant access
  ///   to its elements as if they were unmanaged, using a pointer.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the array.</typeparam>
  /// <remarks>
  ///   This class is useful for interoperability scenarios where a managed array needs to be passed to unmanaged code
  ///   without copying the data, by pinning its contents in memory to prevent the garbage collector from moving it.
  /// </remarks>
  public sealed class PinnedArrayMemoryHandler<T> : PointerMemoryHandlerBase<T> {
    private readonly ManagedArrayPin<T> _pin;

    private PinnedArrayMemoryHandler(ManagedArrayPin<T> pin, T* pointer) : base(pointer) => this._pin = pin;

    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override IMemoryHandler<T> SliceFrom(int offset) => new PinnedArrayMemoryHandler<T>(this._pin, this.Pointer + offset);

    #endregion

    private const int PIN_THRESHOLD_ELEMENT_COUNT = 256;

    public static IMemoryHandler<T> FromManagedArray(T[] array, int start)
      => typeof(T).IsPrimitive && array.Length - start < PIN_THRESHOLD_ELEMENT_COUNT
        ? new ManagedArrayHandler<T>(array, start)
        : ManagedArrayPin<T>.TryPin(array, out var pin)
          ? new PinnedArrayMemoryHandler<T>(pin, pin.Pointer + start)
          : new ManagedArrayHandler<T>(array, start);
  }
}

#endif
