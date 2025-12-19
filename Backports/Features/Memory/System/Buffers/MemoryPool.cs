#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

/// <summary>
/// Represents a pool of memory blocks.
/// </summary>
/// <typeparam name="T">The type of the items in the memory pool.</typeparam>
public abstract class MemoryPool<T> : IDisposable {

  private static readonly ArrayMemoryPool _shared = new();

  /// <summary>
  /// Gets a singleton instance of a memory pool based on arrays.
  /// </summary>
  public static MemoryPool<T> Shared {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _shared;
  }

  /// <summary>
  /// Gets the maximum buffer size supported by this pool.
  /// </summary>
  public abstract int MaxBufferSize { get; }

  /// <summary>
  /// Returns a memory block capable of holding at least <paramref name="minBufferSize"/> elements of <typeparamref name="T"/>.
  /// </summary>
  /// <param name="minBufferSize">The minimum length of the memory block needed.</param>
  public abstract IMemoryOwner<T> Rent(int minBufferSize = -1);

  /// <summary>
  /// Releases all resources used by the <see cref="MemoryPool{T}"/>.
  /// </summary>
  public void Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the unmanaged resources and optionally releases the managed resources.
  /// </summary>
  /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
  protected abstract void Dispose(bool disposing);

  /// <summary>
  /// A simple memory pool implementation based on <see cref="ArrayPool{T}"/>.
  /// </summary>
  private sealed class ArrayMemoryPool : MemoryPool<T> {

    public override int MaxBufferSize => int.MaxValue;

    public override IMemoryOwner<T> Rent(int minBufferSize = -1) {
      if (minBufferSize == -1)
        minBufferSize = 1 + (4095 / Unsafe.SizeOf<T>());
      else
        ArgumentOutOfRangeException.ThrowIfNegative(minBufferSize);

      return new ArrayMemoryPoolBuffer(minBufferSize);
    }

    protected override void Dispose(bool disposing) { }

    private sealed class ArrayMemoryPoolBuffer(int size) : IMemoryOwner<T> {
      private T[] _array = ArrayPool<T>.Shared.Rent(size);

      public Memory<T> Memory {
        get {
          var array = this._array;
          if (array == null)
            throw new ObjectDisposedException(nameof(ArrayMemoryPoolBuffer));

          return new Memory<T>(array);
        }
      }

      public void Dispose() {
        var array = this._array;
        if (array == null)
          return;

        this._array = null!;
        ArrayPool<T>.Shared.Return(array);
      }
    }
  }
}

#endif
