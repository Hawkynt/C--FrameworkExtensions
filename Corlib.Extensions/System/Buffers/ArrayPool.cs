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
//

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

public static partial class ArrayPool {

  /// <summary>
  /// Represents a rented array from an <see cref="ArrayPool{T}"/> that is automatically returned to the pool when disposed.
  /// </summary>
  /// <typeparam name="T">The element type of the array.</typeparam>
  /// <remarks>
  /// Use this struct to temporarily work with pooled arrays while ensuring memory is properly returned.
  /// The array can be accessed through indexers or cast implicitly to <see cref="T[]"/>, <see cref="Span{T}"/>, or <see cref="ReadOnlySpan{T}"/>.
  /// </remarks>
  public readonly struct RentArray<T> : IDisposable {
    private readonly ArrayPool<T> _pool;

    internal RentArray(ArrayPool<T> pool,int minimumSize) {
      this._pool = pool;
      this.Length = minimumSize;
      this.Array = pool.Rent(minimumSize);
    }

    /// <summary>
    /// Gets the underlying array rented from the pool.
    /// </summary>
    public T[] Array { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <summary>
    /// Gets the logical length defined at rent time. This may be less than <see cref="Capacity"/>.
    /// </summary>
    public int Length { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <summary>
    /// Gets the actual length of the underlying rented array.
    /// </summary>
    public int Capacity {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Array.Length;
    }

    /// <summary>
    /// Provides indexed access to the underlying array.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when <paramref name="index"/> is outside the bounds of the array.
    /// </exception>
    public T this[int index] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Array[index];

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => this.Array[index] = value;
    }

    /// <summary>
    /// Returns a slice of the array defined by the specified <see cref="Range"/>.
    /// </summary>
    /// <param name="range">The range of elements to include in the slice.</param>
    /// <returns>A <see cref="Span{T}"/> representing the slice.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the range is outside the bounds of the array.
    /// </exception>
    public Span<T> this[Range range] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Array.AsSpan()[range];
    }

    /// <summary>
    /// Returns a slice of the array beginning at the specified <see cref="Index"/> and with the given length.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of elements in the slice.</param>
    /// <returns>A <see cref="Span{T}"/> over the specified range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified range exceeds array bounds.
    /// </exception>
    public Span<T> this[Index start, int length] {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Array.AsSpan(start.GetOffset(this.Length), length);
    }

    /// <summary>
    /// Returns a <see cref="Span{T}"/> over the entire array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => this.Array.AsSpan();

    /// <summary>
    /// Returns a <see cref="Span{T}"/> that starts at a specified index.
    /// </summary>
    /// <param name="start">The index to start the span.</param>
    /// <returns>A <see cref="Span{T}"/> from the start index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="start"/> is outside the array bounds.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start) => this.Array.AsSpan(start);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> over a range of the array starting at a specified index and length.
    /// </summary>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="length">The number of elements in the span.</param>
    /// <returns>A <see cref="Span{T}"/> over the given range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the range exceeds array bounds.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length) => this.Array.AsSpan(start, length);

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{T}"/> over the entire array.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => this.Array.AsSpan();
    
    #region IDisposable

    /// <inheritdoc />
    public void Dispose() => this._pool.Return(this.Array);

    #endregion

    /// <summary>
    /// Implicitly converts a <see cref="RentArray{T}"/> to the underlying <see cref="Array"/>.
    /// </summary>
    /// <param name="this">The rented array.</param>
    public static implicit operator T[](RentArray<T> @this) => @this.Array;

    /// <summary>
    /// Implicitly converts a <see cref="RentArray{T}"/> to a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="this">The rented array.</param>
    public static implicit operator Span<T>(RentArray<T> @this) => @this.Array.AsSpan();

    /// <summary>
    /// Implicitly converts a <see cref="RentArray{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="this">The rented array.</param>
    public static implicit operator ReadOnlySpan<T>(RentArray<T> @this) => @this.Array;
  }

  /// <summary>
  /// Rents a new array from the specified <see cref="ArrayPool{T}"/> with at least the given size,
  /// returning a wrapper that handles automatic return to the pool.
  /// </summary>
  /// <typeparam name="T">The element type of the array to rent.</typeparam>
  /// <param name="this">The <see cref="ArrayPool{T}"/> instance to rent the array from.</param>
  /// <param name="minimumSize">The minimum number of elements required in the rented array.</param>
  /// <returns>
  /// An <see cref="RentArray{T}"/> instance that provides access to the rented array
  /// and returns it to the pool when disposed.
  /// </returns>
  /// <example>
  /// <code>
  /// using System.Buffers;
  ///
  /// var pool = ArrayPool&lt;byte&gt;.Shared;
  /// using var rented = pool.Use(1024);
  /// byte[] span = rented.Array;
  /// span[0] = 123;
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static RentArray<T> Use<T>(this ArrayPool<T> @this, int minimumSize) => new(@this, minimumSize);

}