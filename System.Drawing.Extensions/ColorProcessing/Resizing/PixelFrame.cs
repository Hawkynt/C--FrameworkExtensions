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
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Represents a view over pixel storage data with dimensions.
/// </summary>
/// <typeparam name="TPixel">The pixel storage type.</typeparam>
/// <remarks>
/// This is a lightweight ref struct that wraps existing memory.
/// It does not own the memory and should not outlive its source.
/// </remarks>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct PixelFrame<TPixel>(Span<TPixel> pixels, int width, int height, int stride)
  where TPixel : unmanaged {

  /// <summary>The pixel data.</summary>
  public readonly Span<TPixel> Pixels = pixels;

  /// <summary>The pixel data.</summary>
  public readonly Span<TPixel> ReadOnlyPixels = pixels;

  /// <summary>Width of the frame in pixels.</summary>
  public readonly int Width = width;

  /// <summary>Height of the frame in pixels.</summary>
  public readonly int Height = height;

  /// <summary>Stride in pixels (typically equals Width).</summary>
  public readonly int Stride = stride;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PixelFrame(Span<TPixel> pixels, int width, int height)
    : this(pixels, width, height, width) { }

  /// <summary>
  /// Gets a reference to a pixel at the specified coordinates.
  /// </summary>
  public ref TPixel this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this.Pixels[y * this.Stride + x];
  }

  /// <summary>
  /// Gets a span for a single row.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<TPixel> GetRow(int y) => this.Pixels.Slice(y * this.Stride, this.Width);
}
