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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Represents a frame of working-space pixels with pooled memory.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <remarks>
/// Uses ArrayPool for efficient memory allocation. Always call Return() when done.
/// </remarks>
public struct WorkFrame<TWork> : IDisposable where TWork : unmanaged {
  private TWork[] _buffer;
  private readonly int _size;

  /// <summary>Width of the frame in pixels.</summary>
  public readonly int Width;

  /// <summary>Height of the frame in pixels.</summary>
  public readonly int Height;

  /// <summary>
  /// Gets the pixel data as a span.
  /// </summary>
  public Span<TWork> Span {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._buffer.AsSpan(0, this._size);
  }

  /// <summary>
  /// Gets a reference to a pixel at the specified coordinates.
  /// </summary>
  public ref TWork this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this._buffer[y * this.Width + x];
  }

  /// <summary>
  /// Gets a span for a single row.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<TWork> GetRow(int y) => this._buffer.AsSpan(y * this.Width, this.Width);

  private WorkFrame(int width, int height, TWork[] buffer) {
    this.Width = width;
    this.Height = height;
    this._size = width * height;
    this._buffer = buffer;
  }

  /// <summary>
  /// Rents a work frame of the specified dimensions from the pool.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static WorkFrame<TWork> Rent(int width, int height) {
    var size = width * height;
    var buffer = ArrayPool<TWork>.Shared.Rent(size);
    return new(width, height, buffer);
  }

  /// <summary>
  /// Returns the frame's buffer to the pool.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Return() {
    if (this._buffer == null)
      return;
    ArrayPool<TWork>.Shared.Return(this._buffer);
    this._buffer = null;
  }

  /// <summary>
  /// Disposes the frame, returning its buffer to the pool.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Dispose() => this.Return();
}
