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

namespace System;

internal static unsafe partial class SpanHelper {
  /// <summary>
  ///   Represents a memory handler for unmanaged memory blocks, providing access to elements of type
  ///   <typeparamref name="T" /> using a pointer.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the unmanaged memory block.</typeparam>
  /// <remarks>
  ///   This class enables managed code to read from and write to a block of unmanaged memory. It is crucial to ensure that
  ///   the lifecycle of the memory block
  ///   is properly managed to avoid memory leaks or accessing invalid memory locations.
  /// </remarks>
#pragma warning disable CS8500
  public sealed class UnmanagedPointerMemoryHandler<T>(T* pointer) : PointerMemoryHandlerBase<T>(pointer) {
    #region Overrides of MemoryHandlerBase<T>

    /// <inheritdoc />
    public override IMemoryHandler<T> SliceFrom(int offset) => new UnmanagedPointerMemoryHandler<T>(this.Pointer + offset);

    #endregion
  }
}

#endif
