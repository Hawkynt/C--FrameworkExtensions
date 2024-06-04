#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if !SUPPORTS_SPAN

namespace System;

internal static unsafe partial class SpanHelper {
  
  /// <summary>
  /// Represents a memory handler for unmanaged memory blocks, providing access to elements of type <typeparamref name="T"/> using a pointer.
  /// </summary>
  /// <typeparam name="T">The type of elements stored in the unmanaged memory block.</typeparam>
  /// <remarks>
  /// This class enables managed code to read from and write to a block of unmanaged memory. It is crucial to ensure that the lifecycle of the memory block
  /// is properly managed to avoid memory leaks or accessing invalid memory locations.
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
