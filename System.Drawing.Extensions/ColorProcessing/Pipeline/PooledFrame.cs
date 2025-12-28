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

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Represents a frame of pixels with pooled memory and scoped pinning.
/// </summary>
/// <typeparam name="TPixel">The pixel type.</typeparam>
/// <remarks>
/// <para>
/// This type uses <see cref="ArrayPool{T}"/> for memory allocation, returning
/// arrays to the pool on disposal. Unlike <see cref="Frame{TPixel}"/> which
/// permanently pins arrays on the POH, this type only pins during explicit
/// <see cref="Pin"/> scopes.
/// </para>
/// <para>
/// Use this type for intermediate storage during image processing operations
/// where pointer access is needed temporarily rather than for the entire lifetime.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// using var frame = new PooledFrame&lt;Bgra8888&gt;(width, height);
/// using (var pinned = frame.Pin()) {
///   Bgra8888* ptr = pinned.Pointer;
///   // Work with pointer...
/// }
/// // Array is unpinned, frame still usable via Span
/// </code>
/// </para>
/// </remarks>
public struct PooledFrame<TPixel> : IDisposable where TPixel : unmanaged, IColorSpace {
  private TPixel[]? _pixels;
  private readonly bool _fromPool;
  private readonly int _size;

  /// <summary>Width of the frame in pixels.</summary>
  public readonly int Width;

  /// <summary>Height of the frame in pixels.</summary>
  public readonly int Height;

  /// <summary>Stride of the frame in pixels (equals Width).</summary>
  public int Stride {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Width;
  }

  /// <summary>
  /// Gets a read-only span over the pixel data.
  /// </summary>
  public ReadOnlySpan<TPixel> Pixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._pixels.AsSpan(0, this._size);
  }

  /// <summary>
  /// Gets a mutable span over the pixel data.
  /// </summary>
  public Span<TPixel> MutablePixels {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._pixels.AsSpan(0, this._size);
  }

  /// <summary>
  /// Gets a reference to the pixel at the specified coordinates.
  /// </summary>
  public ref TPixel this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this._pixels![y * this.Stride + x];
  }

  /// <summary>
  /// Gets a span for a single row.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<TPixel> GetRow(int y) => this._pixels.AsSpan(y * this.Stride, this.Width);

  /// <summary>
  /// Creates a new pooled frame with the specified dimensions.
  /// </summary>
  /// <param name="width">Width in pixels.</param>
  /// <param name="height">Height in pixels.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PooledFrame(int width, int height) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

    this.Width = width;
    this.Height = height;
    this._size = width * height;
    this._pixels = ArrayPool<TPixel>.Shared.Rent(this._size);
    this._fromPool = true;
  }

  /// <summary>
  /// Creates a pooled frame wrapping an external buffer.
  /// </summary>
  /// <param name="buffer">The external buffer to wrap.</param>
  /// <param name="width">Width in pixels.</param>
  /// <param name="height">Height in pixels.</param>
  /// <remarks>
  /// The buffer is NOT returned to any pool on disposal.
  /// Use this for wrapping bitmap data or other externally-owned memory.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal PooledFrame(TPixel[] buffer, int width, int height) {
    ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, width * height);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

    this._pixels = buffer;
    this.Width = width;
    this.Height = height;
    this._size = width * height;
    this._fromPool = false;
  }

  /// <summary>
  /// Pins the array for unsafe pointer access.
  /// </summary>
  /// <returns>A <see cref="PinnedScope{T}"/> that must be disposed to unpin the array.</returns>
  /// <remarks>
  /// The returned scope MUST be disposed to unpin the array.
  /// Prefer using statements: <c>using var pinned = frame.Pin();</c>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PinnedScope<TPixel> Pin() => new(this._pixels!, this._size);

  /// <summary>
  /// Creates a <see cref="PixelFrame{TPixel}"/> view over this frame's data.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PixelFrame<TPixel> AsPixelFrame() => new(this.MutablePixels, this.Width, this.Height, this.Stride);

  /// <summary>
  /// Returns the buffer to the pool if it was rented.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Dispose() {
    if (!this._fromPool || this._pixels == null)
      return;

    ArrayPool<TPixel>.Shared.Return(this._pixels);
    this._pixels = null;
  }
}
