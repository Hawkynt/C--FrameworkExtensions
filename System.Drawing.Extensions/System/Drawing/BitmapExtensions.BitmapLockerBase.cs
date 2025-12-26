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

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing;

public static partial class BitmapExtensions {
  private abstract class BitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : IBitmapLocker {
    private readonly Bitmap _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
    public BitmapData BitmapData { get; } = bitmap.LockBits(rect, flags, format);

    #region Implementation of IDisposable

    private bool _isDisposed;

    ~BitmapLockerBase() => this.Dispose();

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      this._bitmap.UnlockBits(this.BitmapData);
      GC.SuppressFinalize(this);
    }

    #endregion

    protected virtual int _BytesPerPixel
      => this.BitmapData.PixelFormat switch {
        PixelFormat.Format32bppArgb => 4,
        PixelFormat.Format32bppRgb => 4,
        PixelFormat.Format32bppPArgb => 4,
        PixelFormat.Format24bppRgb => 3,
        PixelFormat.Format48bppRgb => 6,
        PixelFormat.Format64bppArgb => 8,
        PixelFormat.Format64bppPArgb => 8,
        PixelFormat.Format16bppArgb1555 => 2,
        PixelFormat.Format16bppGrayScale => 2,
        PixelFormat.Format16bppRgb555 => 2,
        PixelFormat.Format16bppRgb565 => 2,
        PixelFormat.Format8bppIndexed => 1,
        _ => 0
      };

    public int Width => this.BitmapData.Width;
    public int Height => this.BitmapData.Height;

    public bool IsFlatColor {
      get {
        var firstColor = this[0, 0];
        for (var y = 0; y < this.BitmapData.Height; ++y)
        for (var x = 0; x < this.BitmapData.Width; ++x)
          if (this[x, y] != firstColor)
            return false;

        return true;
      }
    }

    public abstract Color this[int x, int y] { get; set; }

    public Color this[Point p] {
      get => this[p.X, p.Y];
      set => this[p.X, p.Y] = value;
    }

    /// <summary>
    /// Gets the pixel color as Rgba32. Override in derived classes for optimized access.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual ColorSpaces.Rgba32 GetPixelRgba32(int x, int y) => new(this[x, y]);

    /// <summary>
    /// Sets the pixel color from Rgba32. Override in derived classes for optimized access.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void SetPixelRgba32(int x, int y, ColorSpaces.Rgba32 color) => this[x, y] = color.ToColor();

    #region Rectangles

    public void Clear(Color color) => this.FillRectangle(0, 0, this.Width, this.Height, color);

    public void DrawRectangle(int x, int y, int width, int height, Color color) {
      if (!this._FixRectangleParameters(ref x, ref y, ref width, ref height))
        return;

      this.DrawRectangleUnchecked(x, y, width, height, color);
    }

    public void DrawRectangle(Point p, Size size, Color color) => this.DrawRectangle(p.X, p.Y, size.Width, size.Height, color);

    public void DrawRectangle(Rectangle rect, Color color) {
      // TODO: fix params
      this.DrawRectangleUnchecked(rect, color);
    }

    public void DrawRectangle(int x, int y, int width, int height, Color color, int lineWidth) {
      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);

      for (var i = 0; i < height - 2 * lineWidth; ++i, ++y) {
        this.DrawHorizontalLine(x, y, lineWidth, color);
        this.DrawHorizontalLine(x + width - lineWidth, y, lineWidth, color);
      }

      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);
    }

    public void DrawRectangleChecked(int x, int y, int width, int height, Color color) {
      this._CheckRectangleParameters(x, y, width, height);
      this.DrawRectangleUnchecked(x, y, width, height, color);
    }

    public void DrawRectangleChecked(Point p, Size size, Color color) => this.DrawRectangleChecked(p.X, p.Y, size.Width, size.Height, color);

    public void DrawRectangleChecked(Rectangle rect, Color color) {
      this._CheckRectangleParameters(rect.X, rect.Y, rect.Width, rect.Height);
      this.DrawRectangleUnchecked(rect, color);
    }

    public void DrawRectangleUnchecked(int x, int y, int width, int height, Color color) {
      this.DrawHorizontalLine(x, y, width, color);
      this.DrawHorizontalLine(x, y + height - 1, width, color);
      height -= 2;
      this.DrawVerticalLine(x, y + 1, height, color);
      this.DrawVerticalLine(x + width - 1, y + 1, height, color);
    }

    public void DrawRectangleUnchecked(Point p, Size size, Color color) => this.DrawRectangleUnchecked(p.X, p.Y, size.Width, size.Height, color);

    public void DrawRectangleUnchecked(Rectangle rect, Color color) => this.DrawRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

    public void DrawRectangleChecked(Rectangle rect, Color color, int lineWidth) {
      var x = rect.X;
      var y = rect.Y;
      var width = rect.Width;
      var height = rect.Height;

      if (!this._FixRectangleParameters(ref x, ref y, ref width, ref height))
        return;

      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);

      for (var i = 0; i < height - 2 * lineWidth; ++i, ++y) {
        this.DrawHorizontalLine(x, y, lineWidth, color);
        this.DrawHorizontalLine(x + width - lineWidth, y, lineWidth, color);
      }

      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);
    }

    public void DrawRectangleUnchecked(Rectangle rect, Color color, int lineWidth) {
      var x = rect.X;
      var y = rect.Y;
      var width = rect.Width;
      var height = rect.Height;

      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);

      for (var i = 0; i < height - 2 * lineWidth; ++i, ++y) {
        this.DrawHorizontalLine(x, y, lineWidth, color);
        this.DrawHorizontalLine(x + width - lineWidth, y, lineWidth, color);
      }

      for (var i = 0; i < lineWidth; ++i)
        this.DrawHorizontalLine(x, y++, width, color);
    }

    public void FillRectangle(int x, int y, int width, int height, Color color) {
      if (!this._FixRectangleParameters(ref x, ref y, ref width, ref height))
        return;

      this.FillRectangleUnchecked(x, y, width, height, color);
    }

    public void FillRectangle(Point p, Size size, Color color) => this.FillRectangle(p.X, p.Y, size.Width, size.Height, color);

    public void FillRectangle(Rectangle rect, Color color) => this.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);

    public void FillRectangleChecked(int x, int y, int width, int height, Color color) {
      this._CheckRectangleParameters(x, y, width, height);
      this.FillRectangleUnchecked(x, y, width, height, color);
    }

    public void FillRectangleChecked(Point p, Size size, Color color) => this.FillRectangleChecked(p.X, p.Y, size.Width, size.Height, color);

    public void FillRectangleChecked(Rectangle rect, Color color) => this.FillRectangleChecked(rect.X, rect.Y, rect.Width, rect.Height, color);

    public void FillRectangleUnchecked(int x, int y, int width, int height, Color color) {
      switch (color.A) {
        case byte.MinValue: return;
        case < byte.MaxValue:
          this._BlendRectangleNaiive(x, y, width, height, color);
          return;
      }

      if (height > 1 && width >= 8 && width * height >= 512) {
        var bytesPerPixel = this._BytesPerPixel;
        if (bytesPerPixel > 0) {
          this._FillRectangleFast(x, y, width, height, color, bytesPerPixel);
          return;
        }
      }

      this._FillRectangleNaiive(x, y, width, height, color);
    }

    public void FillRectangleUnchecked(Point p, Size size, Color color) => this.FillRectangleUnchecked(p.X, p.Y, size.Width, size.Height, color);

    public void FillRectangleUnchecked(Rectangle rect, Color color) => this.FillRectangleUnchecked(rect.X, rect.Y, rect.Width, rect.Height, color);

    private void _FillRectangleNaiive(int x, int y, int width, int height, Color color) {
      do {
        // Duff's device
        switch (height) {
          case 0: goto height0;
          case 1: goto height1;
          case 2: goto height2;
          case 3: goto height3;
          case 4: goto height4;
          case 5: goto height5;
          case 6: goto height6;
          case 7: goto height7;
          case 8: goto height8;
          default: goto heightAbove8;
        }

        height8: this._DrawHorizontalLine(x, y++, width, color);
        height7: this._DrawHorizontalLine(x, y++, width, color);
        height6: this._DrawHorizontalLine(x, y++, width, color);
        height5: this._DrawHorizontalLine(x, y++, width, color);
        height4: this._DrawHorizontalLine(x, y++, width, color);
        height3: this._DrawHorizontalLine(x, y++, width, color);
        height2: this._DrawHorizontalLine(x, y++, width, color);
        height1: this._DrawHorizontalLine(x, y, width, color);
        height0: return;
        heightAbove8:

        // loop unrolled 8-times
        var heightOcts = height >> 3;
        height &= 0b111;

        do {
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
          this._DrawHorizontalLine(x, y++, width, color);
        } while (--heightOcts > 0);
      } while (true);
    }

    private void _BlendRectangleNaiive(int x, int y, int width, int height, Color color) {
      do {
        // Duff's device
        switch (height) {
          case 0: goto height0;
          case 1: goto height1;
          case 2: goto height2;
          case 3: goto height3;
          case 4: goto height4;
          case 5: goto height5;
          case 6: goto height6;
          case 7: goto height7;
          case 8: goto height8;
          default: goto heightAbove8;
        }

        height8: this._BlendHorizontalLine(x, y++, width, color);
        height7: this._BlendHorizontalLine(x, y++, width, color);
        height6: this._BlendHorizontalLine(x, y++, width, color);
        height5: this._BlendHorizontalLine(x, y++, width, color);
        height4: this._BlendHorizontalLine(x, y++, width, color);
        height3: this._BlendHorizontalLine(x, y++, width, color);
        height2: this._BlendHorizontalLine(x, y++, width, color);
        height1: this._BlendHorizontalLine(x, y, width, color);
        height0: return;
        heightAbove8:

        // loop unrolled 8-times
        var heightOcts = height >> 3;
        height &= 0b111;

        do {
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
          this._BlendHorizontalLine(x, y++, width, color);
        } while (--heightOcts > 0);
      } while (true);
    }

    private unsafe void _FillRectangleFast(int x, int y, int width, int height, Color color, int bytesPerPixel) {
      this._DrawHorizontalLine(x, y, width, color); // TODO: if line is long enough, draw part of it, than memory copy

      var bitmapData = this.BitmapData;
      var stride = bitmapData.Stride;
      var firstLineOffset = (byte*)bitmapData.Scan0 + (y * stride + x * bytesPerPixel);
      var byteCount = bytesPerPixel * width;
      var singleLineSource = new ReadOnlySpan<byte>(firstLineOffset, byteCount);

      var offset = firstLineOffset + stride;
      --height;
      do {
        // Duff's device
        switch (height) {
          case 0: goto height0;
          case 1: goto height1;
          case 2: goto height2;
          case 3: goto height3;
          case 4: goto height4;
          case 5: goto height5;
          case 6: goto height6;
          case 7: goto height7;
          case 8: goto height8;
          case 9: goto height9;
          case 10: goto height10;
          case 11: goto height11;
          case 12: goto height12;
          case 13: goto height13;
          case 14: goto height14;
          case 15: goto height15;
          default: goto heightAbove15;
        }

        height15: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height14: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height13: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height12: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height11: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height10: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height9: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height8: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height7: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height6: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height5: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height4: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height3: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height2: singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        height1: singleLineSource.CopyTo(new(offset, byteCount));
        height0:
        
        return;
        heightAbove15:

        // loop unrolled 8-times
        var heightOcts = height >> 3;
        height &= 0b111;
        do {
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
          singleLineSource.CopyTo(new(offset, byteCount)); offset += stride;
        } while (--heightOcts > 0);
      } while (true);
    }

    #endregion

    #region CopyFrom

    public void CopyFrom(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
        this.CopyFromUnchecked(other, xs, ys, width, height, xt, yt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other) => this.CopyFrom(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other, Point target) => this.CopyFrom(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other, Point source, Size size) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other, Rectangle source) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFrom(IBitmapLocker other, Rectangle source, Point target) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    public void CopyFromChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
      this.CopyFromUnchecked(other, xs, ys, width, height, xt, yt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other, Point target) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other, Point source, Size size) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other, Rectangle source) => this.CopyFromChecked(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromChecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromChecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    private void _CopyFromUncheckedNaiive(IBitmapLocker other, int xs, int ys, int width, int height, int xt, int yt) {
      for (var y = 0; y < height; ++ys, ++yt, ++y)
      for (int x = 0, xcs = xs, xct = xt; x < width; ++xcs, ++xct, ++x)
        this[xct, yt] = other[xcs, ys];
    }

    private static unsafe void _CopyFromUncheckedFast(
      int xs,
      int ys,
      int width,
      int height,
      int xt,
      int yt,
      BitmapData bitmapDataSource,
      BitmapData bitmapDataTarget,
      int bytesPerPixel
    ) {
      var sourceStride = bitmapDataSource.Stride;
      var targetStride = bitmapDataTarget.Stride;

      var yOffsetTarget = (byte*)bitmapDataTarget.Scan0 + (targetStride * yt + bytesPerPixel * xt);
      var yOffsetSource = (byte*)bitmapDataSource.Scan0 + (sourceStride * ys + bytesPerPixel * xs);
      var byteCountPerLine = width * bytesPerPixel;

      do {
        // handle all cases less than 8 lines - usind a Duff's device eliminating all jumps and loops except one
        switch (height) {
          case 0: goto height0;
          case 1: goto height1;
          case 2: goto height2;
          case 3: goto height3;
          case 4: goto height4;
          case 5: goto height5;
          case 6: goto height6;
          case 7: goto height7;
          default: goto heightAbove7;
        }

        height7:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height6:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height5:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height4:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height3:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height2:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
        yOffsetSource += sourceStride;
        yOffsetTarget += targetStride;

        height1:
        new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));

        height0:
        return;

        heightAbove7:

        var heightOcts = height >> 3;
        height &= 7;

        // unrolled loop, copying 8 lines in one go - increasing performance roughly by 20% according to benchmarks
        do {
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
          new ReadOnlySpan<byte>(yOffsetSource, byteCountPerLine).CopyTo(new(yOffsetTarget, byteCountPerLine));
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
        } while (--heightOcts > 0);
      } while (true);
    }

    public void CopyFromUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (other is BitmapLockerBase ppb) {
        var bitmapDataTarget = this.BitmapData;
        var pixelFormat = bitmapDataTarget.PixelFormat;
        var bitmapDataSource = ppb.BitmapData;
        if (pixelFormat == bitmapDataSource.PixelFormat) {
          var bytesPerPixel = this._BytesPerPixel;
          if (bytesPerPixel > 0) {
            _CopyFromUncheckedFast(
              xs,
              ys,
              width,
              height,
              xt,
              yt,
              bitmapDataSource,
              bitmapDataTarget,
              bytesPerPixel
            );
            return;
          }
        }
      }

      this._CopyFromUncheckedNaiive(other, xs, ys, width, height, xt, yt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other, Point target) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other, Rectangle source) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    #endregion

    #region CopyFromGrid

    public void CopyFromGrid(
      IBitmapLocker other,
      int column,
      int row,
      int width,
      int height,
      int dx = 0,
      int dy = 0,
      int offsetX = 0,
      int offsetY = 0,
      int targetX = 0,
      int targetY = 0
    ) {
      var sourceX = column * (width + dx) + offsetX;
      var sourceY = row * (height + dy) + offsetY;
      this.CopyFromChecked(other, sourceX, sourceY, width, height, targetX, targetY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset) => this.CopyFromGrid(
      other,
      tile.X,
      tile.Y,
      tileSize.Width,
      tileSize.Height,
      distance.Width,
      distance.Height,
      offset.Width,
      offset.Height
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target) => this.CopyFromGrid(
      other,
      tile.X,
      tile.Y,
      tileSize.Width,
      tileSize.Height,
      distance.Width,
      distance.Height,
      offset.Width,
      offset.Height,
      target.X,
      target.Y
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap CopyFromGrid(int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0) {
      var result = new Bitmap(width, height);
      using var target = result.Lock(ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
      target.CopyFromGrid(
        this,
        column,
        row,
        width,
        height,
        dx,
        dy,
        offsetX,
        offsetY,
        0,
        0
      );
      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap CopyFromGrid(Point tile, Size tileSize) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset, Point target) => throw new NotImplementedException();

    #endregion

    #region BlendWith

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other) => this.BlendWith(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, Point target) => this.BlendWith(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, Point source, Size size) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, Rectangle source) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, Rectangle source, Point target) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWith(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
        this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, Point target) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, Point source, Size size) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, Rectangle source) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, Rectangle source, Point target) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
      this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other, Point target) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other, Rectangle source) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendWithUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    public virtual void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      for (var y = ys; height > 0; ++y, ++yt, --height)
      for (int x = xs, xct = xt, i = width; i > 0; ++x, ++xct, --i)
        this._SetBlendedPixel(xct, yt, other[x, y]);
    }

    #endregion

    [DebuggerHidden]
    private void _CheckRectangleParameters(int x, int y, int width, int height) {
      Against.ValuesOutOfRange(x,0,this.Width - 1);
      Against.ValuesOutOfRange(y, 0, this.Height - 1);
      Against.ValuesOutOfRange(width, 0, this.Width - x);
      Against.ValuesOutOfRange(height, 0, this.Height - y);
    }

    private bool _FixRectangleParameters(ref int x, ref int y, ref int width, ref int height) {
      if (x < 0) {
        width += x;
        x = 0;
      }

      if (y < 0) {
        height += y;
        y = 0;
      }

      if (x >= this.Width)
        return false;

      if (y >= this.Height)
        return false;

      if (x + width > this.Width)
        width = this.Width - x;

      if (y + height > this.Height)
        height = this.Height - y;

      return width > 0 && height > 0;
    }

    [DebuggerHidden]
    private void _CheckCopyParameters(IBitmapLocker other, int xs, int ys, int width, int height, int xt, int yt) {
      Against.ArgumentIsNull(other);
      Against.ValuesOutOfRange(xs, 0, other.Width - 1);
      Against.ValuesOutOfRange(ys, 0, other.Height - 1);
      Against.ValuesOutOfRange(width, 0, other.Width - xs);
      Against.ValuesOutOfRange(height, 0, other.Height - ys);
      Against.ValuesOutOfRange(xt, 0, this.Width - width);
      Against.ValuesOutOfRange(yt, 0, this.Height - height);
    }

    private bool _FixCopyParametersToBeInbounds(IBitmapLocker other, ref int xs, ref int ys, ref int width, ref int height, ref int xt, ref int yt) {
      Against.ArgumentIsNull(other);

      if (xs < 0) {
        width += xs;
        xs = -xs;
      }

      if (ys < 0) {
        height += ys;
        ys = -ys;
      }

      if (xt < 0) {
        width += xt;
        xs = -xt;
        xt = 0;
      }

      if (yt < 0) {
        height += yt;
        ys -= yt;
        yt = 0;
      }

      if (xs >= other.Width)
        return false;

      if (ys >= other.Height)
        return false;

      if (xs + width > other.Width)
        width = other.Width - xs;

      if (ys + height > other.Height)
        height = other.Height - ys;

      if (xt + width > this.Width)
        width = this.Width - xt;

      if (yt + height > this.Height)
        height = this.Height - yt;

      if (width <= 0)
        return false;

      if (height <= 0)
        return false;

      return true;
    }

    #region lines

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void _SetBlendedPixel(int x, int y, Color color) {
      var source = this[x, y];
      var alpha = color.A;
      switch (alpha) {
        case byte.MinValue: return;
        case byte.MaxValue:
          this[x, y] = color;
          return;
        default: {
          var factor = 255 + alpha;
          var r = source.R * byte.MaxValue + color.R * alpha;
          var g = source.G * byte.MaxValue + color.G * alpha;
          var b = source.B * byte.MaxValue + color.B * alpha;
          r /= factor;
          g /= factor;
          b /= factor;
          // Properly calculate result alpha: alpha_out = alpha_src + alpha_dst * (1 - alpha_src)
          var a = alpha + source.A * (255 - alpha) / 255;
          this[x, y] = Color.FromArgb(a, r, g, b);
          return;
        }
      }
    }

    protected virtual void _BlendHorizontalLine(int x, int y, int count, Color color) {
      do {
        // Duff's device
        switch (count) {
          case 0: goto count0;
          case 1: goto count1;
          case 2: goto count2;
          case 3: goto count3;
          case 4: goto count4;
          case 5: goto count5;
          case 6: goto count6;
          case 7: goto count7;
          default: goto countAbove7;
        }

        count7:
        this._SetBlendedPixel(x++, y, color);
        count6:
        this._SetBlendedPixel(x++, y, color);
        count5:
        this._SetBlendedPixel(x++, y, color);
        count4:
        this._SetBlendedPixel(x++, y, color);
        count3:
        this._SetBlendedPixel(x++, y, color);
        count2:
        this._SetBlendedPixel(x++, y, color);
        count1:
        this._SetBlendedPixel(x, y, color);
        count0:
        return;
        countAbove7:

        // loop unroll
        var countOcts = count >> 3;
        count &= 7;

        do {
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
          this._SetBlendedPixel(x++, y, color);
        } while (--countOcts > 0);
      } while (true);
    }

    protected virtual void _DrawHorizontalLine(int x, int y, int count, Color color) {
      do {
        // Duff's device
        switch (count) {
          case 0: goto count0;
          case 1: goto count1;
          case 2: goto count2;
          case 3: goto count3;
          case 4: goto count4;
          case 5: goto count5;
          case 6: goto count6;
          case 7: goto count7;
          default: goto countAbove7;
        }

        count7:
        this[x++, y] = color;
        count6:
        this[x++, y] = color;
        count5:
        this[x++, y] = color;
        count4:
        this[x++, y] = color;
        count3:
        this[x++, y] = color;
        count2:
        this[x++, y] = color;
        count1:
        this[x, y] = color;
        count0:
        return;
        countAbove7:

        // loop unroll
        var countOcts = count >> 3;
        count &= 7;

        do {
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
          this[x++, y] = color;
        } while (--countOcts > 0);
      } while (true);
    }

    protected virtual void _BlendVerticalLine(int x, int y, int count, Color color) {
      do {
        // Duff's device
        switch (count) {
          case 0: goto count0;
          case 1: goto count1;
          case 2: goto count2;
          case 3: goto count3;
          case 4: goto count4;
          case 5: goto count5;
          case 6: goto count6;
          case 7: goto count7;
          default: goto countAbove7;
        }

        count7:
        this._SetBlendedPixel(x, y++, color);
        count6:
        this._SetBlendedPixel(x, y++, color);
        count5:
        this._SetBlendedPixel(x, y++, color);
        count4:
        this._SetBlendedPixel(x, y++, color);
        count3:
        this._SetBlendedPixel(x, y++, color);
        count2:
        this._SetBlendedPixel(x, y++, color);
        count1:
        this._SetBlendedPixel(x, y, color);
        count0:
        return;
        countAbove7:

        // loop unroll
        var countOcts = count >> 3;
        count &= 7;

        do {
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
          this._SetBlendedPixel(x, y++, color);
        } while (--countOcts > 0);
      } while (true);
    }

    protected virtual void _DrawVerticalLine(int x, int y, int count, Color color) {
      do {
        // Duff's device
        switch (count) {
          case 0: goto count0;
          case 1: goto count1;
          case 2: goto count2;
          case 3: goto count3;
          case 4: goto count4;
          case 5: goto count5;
          case 6: goto count6;
          case 7: goto count7;
          default: goto countAbove7;
        }

        count7:
        this[x, y++] = color;
        count6:
        this[x, y++] = color;
        count5:
        this[x, y++] = color;
        count4:
        this[x, y++] = color;
        count3:
        this[x, y++] = color;
        count2:
        this[x, y++] = color;
        count1:
        this[x, y] = color;
        count0:
        return;
        countAbove7:

        // loop unroll
        var countOcts = count >> 3;
        count &= 7;

        do {
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
          this[x, y++] = color;
        } while (--countOcts > 0);
      } while (true);
    }

    public void DrawHorizontalLine(int x, int y, int count, Color color) {
      switch (color.A) {
        case byte.MinValue:
          return;
        case < byte.MaxValue:
          _DrawHorizontalLine(x, y, count, color, this._BlendHorizontalLine);
          break;
        default:
          _DrawHorizontalLine(x, y, count, color, this._DrawHorizontalLine);
          break;
      }
    }

    public void DrawVerticalLine(int x, int y, int count, Color color) {
      switch (color.A) {
        case byte.MinValue:
          return;
        case < byte.MaxValue:
          _DrawVerticalLine(x, y, count, color, this._BlendVerticalLine);
          break;
        default:
          _DrawVerticalLine(x, y, count, color, this._DrawVerticalLine);
          break;
      }
    }

    private static void _DrawHorizontalLine(int x, int y, int count, Color color, Action<int, int, int, Color> call) {
      switch (count) {
        case 0:
          return;
        case < 0:
          call(x - count, y, -count, color);
          break;
        default:
          call(x, y, count, color);
          break;
      }
    }

    private static void _DrawVerticalLine(int x, int y, int count, Color color, Action<int, int, int, Color> call) {
      switch (count) {
        case 0:
          return;
        case < 0:
          call(x, y - count, -count, color);
          break;
        default:
          call(x, y, count, color);
          break;
      }
    }

    public void DrawLine(int x0, int y0, int x1, int y1, Color color) {
      var dx = x1 - x0;
      var dy = y1 - y0;
      if (dx == 0) {
        this.DrawVerticalLine(x0, y0, y1 - y0, color);
        return;
      }

      if (dy == 0) {
        this.DrawHorizontalLine(x0, y0, x1 - x0, color);
        return;
      }

      this._DrawDiagonalLine(x0, y0, dx, dy, color);
    }

    public void DrawLine(int x0, int y0, int x1, int y1, int thickness) => throw new NotImplementedException();

    private void _DrawDiagonalLine(int x0, int y0, int dx, int dy, Color color) {
      int incx, incy, pdx, pdy, ddx, ddy, deltaslowdirection, deltafastdirection;


      if (dx < 0) {
        dx = -dx;
        incx = -1;
      } else
        incx = 1;

      if (dy < 0) {
        dy = -dy;
        incy = -1;
      } else
        incy = 1;

      if (dx > dy) {
        pdx = incx;
        pdy = 0;
        ddx = incx;
        ddy = incy;
        deltaslowdirection = dy;
        deltafastdirection = dx;
      } else {
        pdx = 0;
        pdy = incy;
        ddx = incx;
        ddy = incy;
        deltaslowdirection = dx;
        deltafastdirection = dy;
      }

      var x = x0;
      var y = y0;
      var err = deltafastdirection >> 1;
      this[x, y] = color;

      for (var t = 0; t < deltafastdirection; ++t) {
        err -= deltaslowdirection;
        if (err < 0) {
          err += deltafastdirection;
          x += ddx;
          y += ddy;
        } else {
          x += pdx;
          y += pdy;
        }

        this[x, y] = color;
      }
    }

    private void _DrawDiagonalLine2(int x0, int y0, int dx, int dy, Color color) {
      var xSign = Math.Sign(dx);
      var ySign = Math.Sign(dy);
      dx = Math.Abs(dx);
      dy = Math.Abs(dy);

      var x = x0;
      var y = y0;
      var runTo = Math.Max(dx, dy);
      dy = -dy;
      var epsilon = dx + dy;
      for (var i = 0; i <= runTo; ++i) {
        this[x, y] = color;
        var epsilon2 = epsilon << 1;
        if (epsilon2 > dy) {
          epsilon += dy;
          x += xSign;
        }

        if (epsilon2 < dx) {
          epsilon += dx;
          y += ySign;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawHorizontalLine(Point p, int count, Color color) => this.DrawHorizontalLine(p.X, p.Y, count, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawVerticalLine(Point p, int count, Color color) => this.DrawVerticalLine(p.X, p.Y, count, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawLine(Point a, Point b, Color color) => this.DrawLine(a.X, a.Y, b.X, b.Y, color);

    public void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color) {
      var offset = (int)((float)thickness / 2);

      for (var i = 0; i < thickness; ++i) {
        this.DrawLine(a1.X, a1.Y - offset + i, b1.X, b1.Y - offset + i, color);
        this.DrawLine(a2.X, a2.Y - offset + i, b2.X, b2.Y - offset + i, color);
      }
    }

    public void DrawCross(Rectangle rect, int thickness, Color color) =>
      this.DrawCross(
        new(rect.Left, rect.Top),
        new(rect.Right, rect.Bottom),
        new(rect.Right, rect.Top),
        new(rect.Left, rect.Bottom),
        thickness,
        color
      );

    #endregion
  }
}
