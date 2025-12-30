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
using System.Drawing;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Abstract generic base class for typed bitmap lockers with optimized drawing operations.
/// </summary>
/// <typeparam name="TPixel">The pixel storage type.</typeparam>
internal abstract class TypedBitmapLockerBase<TPixel> : BitmapLockerBase, IFrameAccessor<TPixel>
  where TPixel : unmanaged {

  /// <summary>
  /// Initializes a new instance of the <see cref="TypedBitmapLockerBase{TPixel}"/> class.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected TypedBitmapLockerBase(Bitmap bitmap, ImageLockMode lockMode, int bytesPerPixel, params PixelFormat[] validFormats)
    : base(bitmap, lockMode, bytesPerPixel, validFormats) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TypedBitmapLockerBase{TPixel}"/> class with region support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected TypedBitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, int bytesPerPixel, params PixelFormat[] validFormats)
    : base(bitmap, rect, lockMode, bytesPerPixel, validFormats) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TypedBitmapLockerBase{TPixel}"/> class with region and target format support.
  /// </summary>
  /// <param name="bitmap">The bitmap to lock.</param>
  /// <param name="rect">The region of the bitmap to lock.</param>
  /// <param name="lockMode">The lock mode.</param>
  /// <param name="bytesPerPixel">The number of bytes per pixel.</param>
  /// <param name="targetFormat">The pixel format to lock the bitmap as.</param>
  /// <param name="validFormats">The valid pixel formats.</param>
  protected TypedBitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode lockMode, int bytesPerPixel, PixelFormat targetFormat, params PixelFormat[] validFormats)
    : base(bitmap, rect, lockMode, bytesPerPixel, targetFormat, validFormats) { }

  /// <inheritdoc/>
  public abstract Span<TPixel> Pixels { get; }

  /// <summary>Converts a native pixel to a Color.</summary>
  /// <param name="pixel">The pixel value.</param>
  /// <returns>The corresponding Color.</returns>
  protected abstract Color PixelToColor(TPixel pixel);

  /// <summary>Converts a Color to a native pixel.</summary>
  /// <param name="color">The color.</param>
  /// <returns>The corresponding pixel value.</returns>
  protected abstract TPixel ColorToPixel(Color color);

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PixelFrame<TPixel> AsFrame() => new(this.Pixels, this.Width, this.Height, this.Stride);

  #region Optimized Drawing Methods

  /// <inheritdoc/>
  public override void Clear(Color color) {
    var pixel = this.ColorToPixel(color);
    this.Pixels.Fill(pixel);
  }

  /// <inheritdoc/>
  public override void DrawHorizontalLine(int x, int y, int length, Color color) {
    if (y < 0 || y >= this.Height || length <= 0)
      return;

    var startX = Math.Max(0, x);
    var endX = Math.Min(this.Width, x + length);
    var actualLength = endX - startX;
    if (actualLength <= 0)
      return;

    var pixel = this.ColorToPixel(color);
    var rowStart = y * this.Stride + startX;
    this.Pixels.Slice(rowStart, actualLength).Fill(pixel);
  }

  /// <inheritdoc/>
  public override void DrawVerticalLine(int x, int y, int length, Color color) {
    if (x < 0 || x >= this.Width || length <= 0)
      return;

    var startY = Math.Max(0, y);
    var endY = Math.Min(this.Height, y + length);
    if (startY >= endY)
      return;

    var pixel = this.ColorToPixel(color);
    var stride = this.Stride;
    var pixels = this.Pixels;
    var offset = startY * stride + x;

    for (var py = startY; py < endY; ++py) {
      pixels[offset] = pixel;
      offset += stride;
    }
  }

  /// <inheritdoc/>
  public override void FillRectangle(int x, int y, int width, int height, Color color) {
    var startX = Math.Max(0, x);
    var startY = Math.Max(0, y);
    var endX = Math.Min(this.Width, x + width);
    var endY = Math.Min(this.Height, y + height);

    var actualWidth = endX - startX;
    var actualHeight = endY - startY;
    if (actualWidth <= 0 || actualHeight <= 0)
      return;

    var pixel = this.ColorToPixel(color);
    var stride = this.Stride;
    var pixels = this.Pixels;

    // Fill first row
    var firstRowStart = startY * stride + startX;
    var firstRow = pixels.Slice(firstRowStart, actualWidth);
    firstRow.Fill(pixel);

    // Copy first row to subsequent rows
    for (var row = startY + 1; row < endY; ++row) {
      var rowStart = row * stride + startX;
      firstRow.CopyTo(pixels.Slice(rowStart, actualWidth));
    }
  }

  /// <inheritdoc/>
  public override void CopyFrom(IBitmapLocker source) {
    // Fast path: same concrete type
    if (source is TypedBitmapLockerBase<TPixel> typedSource) {
      var width = Math.Min(this.Width, typedSource.Width);
      var height = Math.Min(this.Height, typedSource.Height);
      var srcStride = typedSource.Stride;
      var dstStride = this.Stride;

      var srcPixels = typedSource.Pixels;
      var dstPixels = this.Pixels;

      for (var row = 0; row < height; ++row) {
        var srcRow = srcPixels.Slice(row * srcStride, width);
        var dstRow = dstPixels.Slice(row * dstStride, width);
        srcRow.CopyTo(dstRow);
      }
      return;
    }

    // Slow path: per-pixel via Color conversion
    base.CopyFrom(source);
  }

  /// <inheritdoc/>
  public override void CopyFrom(IBitmapLocker source, int srcX, int srcY, int width, int height, int destX, int destY) {
    // Clip to valid bounds
    if (srcX < 0) { width += srcX; destX -= srcX; srcX = 0; }
    if (srcY < 0) { height += srcY; destY -= srcY; srcY = 0; }
    if (destX < 0) { width += destX; srcX -= destX; destX = 0; }
    if (destY < 0) { height += destY; srcY -= destY; destY = 0; }

    width = Math.Min(width, Math.Min(source.Width - srcX, this.Width - destX));
    height = Math.Min(height, Math.Min(source.Height - srcY, this.Height - destY));

    if (width <= 0 || height <= 0)
      return;

    // Fast path: same concrete type
    if (source is TypedBitmapLockerBase<TPixel> typedSource) {
      var srcStride = typedSource.Stride;
      var dstStride = this.Stride;
      var srcPixels = typedSource.Pixels;
      var dstPixels = this.Pixels;

      for (var row = 0; row < height; ++row) {
        var srcRow = srcPixels.Slice((srcY + row) * srcStride + srcX, width);
        var dstRow = dstPixels.Slice((destY + row) * dstStride + destX, width);
        srcRow.CopyTo(dstRow);
      }
      return;
    }

    // Slow path: per-pixel via Color conversion
    base.CopyFrom(source, srcX, srcY, width, height, destX, destY);
  }

  /// <inheritdoc/>
  public override bool IsFlatColor {
    get {
      if (this.Width == 0 || this.Height == 0)
        return true;

      var pixels = this.Pixels;
      if (pixels.Length == 0)
        return true;

      var firstPixel = pixels[0];
      var stride = this.Stride;
      var width = this.Width;
      var height = this.Height;

      // For contiguous layout (stride == width), use single pass
      if (stride == width) {
        var length = width * height;
        for (var i = 1; i < length; ++i)
          if (!pixels[i].Equals(firstPixel))
            return false;
        return true;
      }

      // Non-contiguous: check row by row
      for (var y = 0; y < height; ++y) {
        var rowStart = y * stride;
        for (var x = 0; x < width; ++x)
          if (!pixels[rowStart + x].Equals(firstPixel))
            return false;
      }
      return true;
    }
  }

  #endregion
}
