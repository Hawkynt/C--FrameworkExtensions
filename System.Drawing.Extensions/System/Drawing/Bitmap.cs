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

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global


namespace System.Drawing;
// ReSharper disable once PartialTypeWithSinglePart

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class BitmapExtensions {
  #region nested types

  // ReSharper disable once PartialTypeWithSinglePart
  private static partial class NativeMethods {
    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
    private static extern IntPtr _MemoryCopy(IntPtr dst, IntPtr src, int count);

    [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
    private static extern IntPtr _MemorySet(IntPtr dst, int value, int count);

    public static void MemoryCopy(IntPtr source, IntPtr target, int count) => _MemoryCopy(target, source, count);

    public static void MemorySet(IntPtr source, byte value, int count) => _MemorySet(source, value, count);
  }

  [SuppressMessage("ReSharper", "UnusedMember.Global")]
  public interface IBitmapLocker : IDisposable {
    BitmapData BitmapData { get; }
    Color this[int x, int y] { get; set; }
    Color this[Point p] { get; set; }

    void Clear(Color color);
    void DrawHorizontalLine(int x, int y, int count, Color color);
    void DrawHorizontalLine(Point p, int count, Color color);
    void DrawVerticalLine(int x, int y, int count, Color color);
    void DrawVerticalLine(Point p, int count, Color color);
    void DrawLine(int x0, int y0, int x1, int y1, Color color);
    void DrawLine(Point a, Point b, Color color);
    void DrawCross(Point a1, Point b1, Point a2, Point b2, int thickness, Color color);
    void DrawCross(Rectangle rect, int thickness, Color color);

    void DrawRectangle(int x, int y, int width, int height, Color color);
    void DrawRectangle(Point p, Size size, Color color);
    void DrawRectangleChecked(Rectangle rect, Color color, int lineWidth);
    void DrawRectangle(Rectangle rect, Color color);
    void DrawRectangle(int x, int y, int width, int height, Color color, int lineWidth);
    void DrawRectangleChecked(int x, int y, int width, int height, Color color);
    void DrawRectangleChecked(Point p, Size size, Color color);
    void DrawRectangleChecked(Rectangle rect, Color color);
    void DrawRectangleUnchecked(int x, int y, int width, int height, Color color);
    void DrawRectangleUnchecked(Point p, Size size, Color color);
    void DrawRectangleUnchecked(Rectangle rect, Color color);
    void FillRectangle(int x, int y, int width, int height, Color color);
    void FillRectangle(Point p, Size size, Color color);
    void FillRectangle(Rectangle rect, Color color);
    void FillRectangleChecked(int x, int y, int width, int height, Color color);
    void FillRectangleChecked(Point p, Size size, Color color);
    void FillRectangleChecked(Rectangle rect, Color color);
    void FillRectangleUnchecked(int x, int y, int width, int height, Color color);
    void FillRectangleUnchecked(Point p, Size size, Color color);
    void FillRectangleUnchecked(Rectangle rect, Color color);

    void CopyFrom(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFrom(IBitmapLocker other);
    void CopyFrom(IBitmapLocker other, Point target);
    void CopyFrom(IBitmapLocker other, Point source, Size size);
    void CopyFrom(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFrom(IBitmapLocker other, Rectangle source);
    void CopyFrom(IBitmapLocker other, Rectangle source, Point target);
    void CopyFromChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFromChecked(IBitmapLocker other);
    void CopyFromChecked(IBitmapLocker other, Point target);
    void CopyFromChecked(IBitmapLocker other, Point source, Size size);
    void CopyFromChecked(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFromChecked(IBitmapLocker other, Rectangle source);
    void CopyFromChecked(IBitmapLocker other, Rectangle source, Point target);
    void CopyFromUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void CopyFromUnchecked(IBitmapLocker other);
    void CopyFromUnchecked(IBitmapLocker other, Point target);
    void CopyFromUnchecked(IBitmapLocker other, Point source, Size size);
    void CopyFromUnchecked(IBitmapLocker other, Point source, Size size, Point target);
    void CopyFromUnchecked(IBitmapLocker other, Rectangle source);
    void CopyFromUnchecked(IBitmapLocker other, Rectangle source, Point target);

    void BlendWith(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWith(IBitmapLocker other);
    void BlendWith(IBitmapLocker other, Point target);
    void BlendWith(IBitmapLocker other, Point source, Size size);
    void BlendWith(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWith(IBitmapLocker other, Rectangle source);
    void BlendWith(IBitmapLocker other, Rectangle source, Point target);
    void BlendWithChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWithChecked(IBitmapLocker other);
    void BlendWithChecked(IBitmapLocker other, Point target);
    void BlendWithChecked(IBitmapLocker other, Point source, Size size);
    void BlendWithChecked(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWithChecked(IBitmapLocker other, Rectangle source);
    void BlendWithChecked(IBitmapLocker other, Rectangle source, Point target);
    void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0);
    void BlendWithUnchecked(IBitmapLocker other);
    void BlendWithUnchecked(IBitmapLocker other, Point target);
    void BlendWithUnchecked(IBitmapLocker other, Point source, Size size);
    void BlendWithUnchecked(IBitmapLocker other, Point source, Size size, Point target);
    void BlendWithUnchecked(IBitmapLocker other, Rectangle source);
    void BlendWithUnchecked(IBitmapLocker other, Rectangle source, Point target);

    void CopyFromGrid(IBitmapLocker other, int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0, int targetX = 0, int targetY = 0);

    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset);
    void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target);

    Bitmap CopyFromGrid(int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0);

    Bitmap CopyFromGrid(Point tile, Size tileSize);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset);
    Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset, Point target);

    /// <summary>
    ///   <c>true</c> when all pixels have the same color; otherwise, <c>false</c>.
    /// </summary>
    bool IsFlatColor { get; }

    int Width { get; }
    int Height { get; }
  }

  private abstract class BitmapLockerBase : IBitmapLocker {
    private readonly Bitmap _bitmap;
    public BitmapData BitmapData { get; }

    protected BitmapLockerBase(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) {
      this._bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
      this.BitmapData = bitmap.LockBits(rect, flags, format);
    }

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
        case byte.MinValue:
          return;
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
          case 0:
            goto height0;
          case 1:
            goto height1;
          case 2:
            goto height2;
          case 3:
            goto height3;
          case 4:
            goto height4;
          case 5:
            goto height5;
          case 6:
            goto height6;
          case 7:
            goto height7;
          case 8:
            goto height8;
          default:
            goto heightAbove8;
        }

        height8:
        this._DrawHorizontalLine(x, y++, width, color);
        height7:
        this._DrawHorizontalLine(x, y++, width, color);
        height6:
        this._DrawHorizontalLine(x, y++, width, color);
        height5:
        this._DrawHorizontalLine(x, y++, width, color);
        height4:
        this._DrawHorizontalLine(x, y++, width, color);
        height3:
        this._DrawHorizontalLine(x, y++, width, color);
        height2:
        this._DrawHorizontalLine(x, y++, width, color);
        height1:
        this._DrawHorizontalLine(x, y, width, color);
        height0:
        return;
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
          case 0:
            goto height0;
          case 1:
            goto height1;
          case 2:
            goto height2;
          case 3:
            goto height3;
          case 4:
            goto height4;
          case 5:
            goto height5;
          case 6:
            goto height6;
          case 7:
            goto height7;
          case 8:
            goto height8;
          default:
            goto heightAbove8;
        }

        height8:
        this._BlendHorizontalLine(x, y++, width, color);
        height7:
        this._BlendHorizontalLine(x, y++, width, color);
        height6:
        this._BlendHorizontalLine(x, y++, width, color);
        height5:
        this._BlendHorizontalLine(x, y++, width, color);
        height4:
        this._BlendHorizontalLine(x, y++, width, color);
        height3:
        this._BlendHorizontalLine(x, y++, width, color);
        height2:
        this._BlendHorizontalLine(x, y++, width, color);
        height1:
        this._BlendHorizontalLine(x, y, width, color);
        height0:
        return;
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

    private void _FillRectangleFast(int x, int y, int width, int height, Color color, int bytesPerPixel) {
      var bitmapData = this.BitmapData;
      var stride = bitmapData.Stride;
      var byteCount = bytesPerPixel * width;

#if SUPPORTS_POINTER_ARITHMETIC
        var startOffset = bitmapData.Scan0 + (y * stride + x * bytesPerPixel);
#else
      var startOffset = _Add(bitmapData.Scan0, y * stride + x * bytesPerPixel);
#endif
      this._DrawHorizontalLine(x, y, width, color); // TODO: if line is long enough, draw part of it, than memory copy
#if SUPPORTS_POINTER_ARITHMETIC
        var offset = startOffset + stride;
#else
      var offset = _Add(startOffset, stride);
#endif
      --height;
      do {
        // Duff's device
        switch (height) {
          case 0:
            goto height0;
          case 1:
            goto height1;
          case 2:
            goto height2;
          case 3:
            goto height3;
          case 4:
            goto height4;
          case 5:
            goto height5;
          case 6:
            goto height6;
          case 7:
            goto height7;
          case 8:
            goto height8;
          case 9:
            goto height9;
          case 10:
            goto height10;
          case 11:
            goto height11;
          case 12:
            goto height12;
          case 13:
            goto height13;
          case 14:
            goto height14;
          case 15:
            goto height15;
          default:
            goto heightAbove15;
        }

        height15:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height14:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height13:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height12:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height11:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height10:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height9:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height8:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height7:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height6:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height5:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height4:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height3:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height2:
        _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
          offset += stride;
#else
        _Add(ref offset, stride);
#endif
        height1:
        _memoryCopyCall(startOffset, offset, byteCount);
        height0:
        return;
        heightAbove15:

        // loop unrolled 8-times
        var heightOcts = height >> 3;
        height &= 0b111;

        do {
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
          _memoryCopyCall(startOffset, offset, byteCount);
#if SUPPORTS_POINTER_ARITHMETIC
            offset += stride;
#else
          _Add(ref offset, stride);
#endif
        } while (--heightOcts > 0);
      } while (true);
    }

    #endregion

    #region CopyFrom

    public void CopyFrom(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
        this.CopyFromUnchecked(other, xs, ys, width, height, xt, yt);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other) => this.CopyFrom(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other, Point target) => this.CopyFrom(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other, Point source, Size size) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other, Rectangle source) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFrom(IBitmapLocker other, Rectangle source, Point target) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);


    public void CopyFromChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
      this.CopyFromUnchecked(other, xs, ys, width, height, xt, yt);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other, Point target) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other, Point source, Size size) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other, Rectangle source) => this.CopyFromChecked(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromChecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromChecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    private void _CopyFromUncheckedNaiive(IBitmapLocker other, int xs, int ys, int width, int height, int xt, int yt) {
      for (var y = 0; y < height; ++ys, ++yt, ++y)
      for (int x = 0, xcs = xs, xct = xt; x < width; ++xcs, ++xct, ++x)
        this[xct, yt] = other[xcs, ys];
    }

    private static void _CopyFromUncheckedFast(int xs, int ys, int width, int height, int xt, int yt, BitmapData bitmapDataSource, BitmapData bitmapDataTarget, int bytesPerPixel) {
      var sourceStride = bitmapDataSource.Stride;
      var targetStride = bitmapDataTarget.Stride;

#if SUPPORTS_POINTER_ARITHMETIC
        var yOffsetTarget = bitmapDataTarget.Scan0 + (targetStride * yt + bytesPerPixel * xt);
        var yOffsetSource = bitmapDataSource.Scan0 + (sourceStride * ys + bytesPerPixel * xs);
#else
      var yOffsetTarget = _Add(bitmapDataTarget.Scan0, targetStride * yt + bytesPerPixel * xt);
      var yOffsetSource = _Add(bitmapDataSource.Scan0, sourceStride * ys + bytesPerPixel * xs);
#endif
      var byteCountPerLine = width * bytesPerPixel;

      do {
        // handle all cases less than 8 lines - usind a Duff's device eliminating all jumps and loops except one
        switch (height) {
          case 0:
            goto height0;
          case 1:
            goto height1;
          case 2:
            goto height2;
          case 3:
            goto height3;
          case 4:
            goto height4;
          case 5:
            goto height5;
          case 6:
            goto height6;
          case 7:
            goto height7;
          default:
            goto heightAbove7;
        }

        height7:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height6:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height5:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height4:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height3:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height2:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
          yOffsetSource += sourceStride;
          yOffsetTarget += targetStride;
#else
        _Add(ref yOffsetSource, sourceStride);
        _Add(ref yOffsetTarget, targetStride);
#endif
        height1:
        _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
        height0:
        return;
        heightAbove7:

        var heightOcts = height >> 3;
        height &= 7;

        // unrolled loop, copying 8 lines in one go - increasing performance roughly by 20% according to benchmarks
        do {
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
          _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
#if SUPPORTS_POINTER_ARITHMETIC
            yOffsetSource += sourceStride;
            yOffsetTarget += targetStride;
#else
          _Add(ref yOffsetSource, sourceStride);
          _Add(ref yOffsetTarget, targetStride);
#endif
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
            _CopyFromUncheckedFast(xs, ys, width, height, xt, yt, bitmapDataSource, bitmapDataTarget, bytesPerPixel);
            return;
          }
        }
      }

      this._CopyFromUncheckedNaiive(other, xs, ys, width, height, xt, yt);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other, Point target) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other, Rectangle source) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    #endregion

    #region CopyFromGrid

    public void CopyFromGrid(IBitmapLocker other, int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0, int targetX = 0, int targetY = 0) {
      var sourceX = column * (width + dx) + offsetX;
      var sourceY = row * (height + dy) + offsetY;
      this.CopyFromChecked(other, sourceX, sourceY, width, height, targetX, targetY);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target) => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height, target.X, target.Y);

    public Bitmap CopyFromGrid(int column, int row, int width, int height, int dx = 0, int dy = 0, int offsetX = 0, int offsetY = 0) {
      var result = new Bitmap(width, height);
      using var target = result.Lock(ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
      target.CopyFromGrid(this, column, row, width, height, dx, dy, offsetX, offsetY, 0, 0);
      return result;
    }

    public Bitmap CopyFromGrid(Point tile, Size tileSize) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height);

    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height);

    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset) => this.CopyFromGrid(tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height);

    public Bitmap CopyFromGrid(Point tile, Size tileSize, Size distance, Size offset, Point target) => throw new NotImplementedException();

    #endregion

    #region BlendWith

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other) => this.BlendWith(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other, Point target) => this.BlendWith(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other, Point source, Size size) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other, Rectangle source) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWith(IBitmapLocker other, Rectangle source, Point target) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    public void BlendWith(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
        this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other, Point target) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other, Point source, Size size) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other, Rectangle source) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithChecked(IBitmapLocker other, Rectangle source, Point target) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    public void BlendWithChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
      this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
    }

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other, Point target) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other, Rectangle source) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height);

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void BlendWithUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

    public virtual void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      for (var y = ys; height > 0; ++y, ++yt, --height)
      for (int x = xs, xct = xt, i = width; i > 0; ++x, ++xct, --i)
        this._SetBlendedPixel(xct, yt, other[x, y]);
    }

    #endregion

    [DebuggerHidden]
    private void _CheckRectangleParameters(int x, int y, int width, int height) {
      if (x < 0 || x >= this.Width)
        throw new ArgumentOutOfRangeException(nameof(x));
      if (y < 0 || y >= this.Height)
        throw new ArgumentOutOfRangeException(nameof(y));
      if (width < 0 || x + width > this.Width)
        throw new ArgumentOutOfRangeException(nameof(width));
      if (height < 0 || y + height > this.Height)
        throw new ArgumentOutOfRangeException(nameof(height));
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
      if (other == null)
        throw new ArgumentNullException(nameof(other));
      if (xs < 0 || xs >= other.Width)
        throw new ArgumentOutOfRangeException(nameof(xs));
      if (ys < 0 || ys >= other.Height)
        throw new ArgumentOutOfRangeException(nameof(ys));
      if (width < 1 || xs + width > other.Width)
        throw new ArgumentOutOfRangeException(nameof(width));
      if (height < 1 || ys + height > other.Height)
        throw new ArgumentOutOfRangeException(nameof(height));
      if (xt < 0 || xt + width > this.Width)
        throw new ArgumentOutOfRangeException(nameof(xt));
      if (yt < 0 || yt + height > this.Height)
        throw new ArgumentOutOfRangeException(nameof(yt));
    }

    private bool _FixCopyParametersToBeInbounds(IBitmapLocker other, ref int xs, ref int ys, ref int width, ref int height, ref int xt, ref int yt) {
      if (other == null)
        throw new ArgumentNullException(nameof(other));

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

      if (width < 1)
        return false;

      if (height < 1)
        return false;

      return true;
    }

    #region lines

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void _SetBlendedPixel(int x, int y, Color color) {
      var source = this[x, y];
      var alpha = color.A;
      switch (alpha) {
        case byte.MinValue:
          return;
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
          this[x, y] = Color.FromArgb(source.A, r, g, b);
          return;
        }
      }
    }

    protected virtual void _BlendHorizontalLine(int x, int y, int count, Color color) {
      do {
        // Duff's device
        switch (count) {
          case 0:
            goto count0;
          case 1:
            goto count1;
          case 2:
            goto count2;
          case 3:
            goto count3;
          case 4:
            goto count4;
          case 5:
            goto count5;
          case 6:
            goto count6;
          case 7:
            goto count7;
          default:
            goto countAbove7;
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
          case 0:
            goto count0;
          case 1:
            goto count1;
          case 2:
            goto count2;
          case 3:
            goto count3;
          case 4:
            goto count4;
          case 5:
            goto count5;
          case 6:
            goto count6;
          case 7:
            goto count7;
          default:
            goto countAbove7;
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
          case 0:
            goto count0;
          case 1:
            goto count1;
          case 2:
            goto count2;
          case 3:
            goto count3;
          case 4:
            goto count4;
          case 5:
            goto count5;
          case 6:
            goto count6;
          case 7:
            goto count7;
          default:
            goto countAbove7;
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
          case 0:
            goto count0;
          case 1:
            goto count1;
          case 2:
            goto count2;
          case 3:
            goto count3;
          case 4:
            goto count4;
          case 5:
            goto count5;
          case 6:
            goto count6;
          case 7:
            goto count7;
          default:
            goto countAbove7;
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
      if (color.A == byte.MinValue)
        return;

      if (color.A < byte.MaxValue)
        _DrawHorizontalLine(x, y, count, color, this._BlendHorizontalLine);
      else
        _DrawHorizontalLine(x, y, count, color, this._DrawHorizontalLine);
    }

    public void DrawVerticalLine(int x, int y, int count, Color color) {
      if (color.A == byte.MinValue)
        return;

      if (color.A < byte.MaxValue)
        _DrawVerticalLine(x, y, count, color, this._BlendVerticalLine);
      else
        _DrawVerticalLine(x, y, count, color, this._DrawVerticalLine);
    }

    private static void _DrawHorizontalLine(int x, int y, int count, Color color, Action<int, int, int, Color> call) {
      if (count == 0)
        return;
      if (count < 0)
        call(x - count, y, -count, color);
      else
        call(x, y, count, color);
    }

    private static void _DrawVerticalLine(int x, int y, int count, Color color, Action<int, int, int, Color> call) {
      if (count == 0)
        return;
      if (count < 0)
        call(x, y - count, -count, color);
      else
        call(x, y, count, color);
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

    public void DrawLine(int x0, int y0, int x1, int y1, int thickness) {
      throw new NotImplementedException();
    }

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

    public void DrawHorizontalLine(Point p, int count, Color color) => this.DrawHorizontalLine(p.X, p.Y, count, color);
    public void DrawVerticalLine(Point p, int count, Color color) => this.DrawVerticalLine(p.X, p.Y, count, color);
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
        color);

    #endregion
  }

  #region optimized pixel format handlers

  private sealed class ARGB32BitmapLocker : BitmapLockerBase {
    public ARGB32BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) : base(bitmap,
      rect, flags, format) { }

    protected override int _BytesPerPixel => 4;

    public override Color this[int x, int y] {
      get {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (int*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride >> 2;
          var offset = stride * y + x;
          return Color.FromArgb(pointer[offset]);
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          return Color.FromArgb(Marshal.ReadInt32(pointer, offset));

#endif
      }
      set {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (int*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride >> 2;
          var offset = stride * y + x;
          pointer[offset] = value.ToArgb();
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          Marshal.WriteInt32(pointer, offset, value.ToArgb());

#endif
      }
    }


#if UNSAFE

    protected override unsafe void _DrawHorizontalLine(int x, int y, int count, Color color) {
      Debug.Assert(count > 0);
      var data = this.BitmapData;
      var pointer = (int*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride >> 2;
      var offset = stride * y + x;
      pointer += offset;
      var value = color.ToArgb();

#if !PLATFORM_X86

      // unrolled loop 8-times (saving 87.5% of comparisons, using the highest IL-constant opcode of 8 [ldc.i4.8])
      if (count >= 16) {
        const int pixelsPerLoop = 16;
        const int pointerIncreasePerLoop = 16;
        var mask = ((ulong)value << 32) | (uint)value;
        while (count >= pixelsPerLoop) {
          var longPointer = (ulong*)pointer;
          longPointer[0] = mask;
          longPointer[1] = mask;
          longPointer[2] = mask;
          longPointer[3] = mask;
          longPointer[4] = mask;
          longPointer[5] = mask;
          longPointer[6] = mask;
          longPointer[7] = mask;
          count -= pixelsPerLoop;
          pointer += pointerIncreasePerLoop;
        }
      }
#endif

      // unrolled loop 8-times (saving 87.5% of comparisons, using the highest IL-constant opcode of 8 [ldc.i4.8])
      {
        const int pixelsPerLoop = 8;
        const int pointerIncreasePerLoop = 8;
        while (count >= pixelsPerLoop) {
          pointer[0] = value;
          pointer[1] = value;
          pointer[2] = value;
          pointer[3] = value;
          pointer[4] = value;
          pointer[5] = value;
          pointer[6] = value;
          pointer[7] = value;
          count -= pixelsPerLoop;
          pointer += pointerIncreasePerLoop;
        }
      }

      while (count-- > 0)
        *pointer++ = value;
    }

    protected override unsafe void _DrawVerticalLine(int x, int y, int count, Color color) {
      Debug.Assert(count > 0);
      var data = this.BitmapData;
      var pointer = (int*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride >> 2;
      var offset = stride * y + x;
      var value = color.ToArgb();
      pointer += offset;
      while (count-- > 0) {
        *pointer = value;
        pointer += stride;
      }
    }

#endif

#if UNSAFE

    public override unsafe void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
      if (other is not ARGB32BitmapLocker otherLocker) {
        base.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
        return;
      }

      var thisStride = this.BitmapData.Stride;
      var otherStride = otherLocker.BitmapData.Stride;
      const int bpp = 4;

      var thisOffset = (byte*)this.BitmapData.Scan0 + yt * thisStride + xt * bpp;
      var otherOffset = (byte*)otherLocker.BitmapData.Scan0 + ys * otherStride + xs * bpp;

      for (; height > 0; thisOffset += thisStride, otherOffset += otherStride, --height) {
        var to = thisOffset;
        var oo = otherOffset;

        for (var x = width; x > 0; to += bpp, oo += bpp, --x) {
          var op = *(uint*)oo;
          switch (op) {
            case < 0x01000000:
              continue;
            case >= 0xff000000:
              *(uint*)to = op;
              continue;
          }

          var sourcePixel = *(uint*)to;

          uint b1 = (byte)op;
          op >>= 8;
          uint g1 = (byte)op;
          op >>= 8;
          uint r1 = (byte)op;
          op >>= 8;
          var alpha = op;
          var factor = 1d / (255 + alpha);

          var newPixel = sourcePixel & 0xff000000;
          uint b = (byte)sourcePixel;
          sourcePixel >>= 8;
          uint g = (byte)sourcePixel;
          sourcePixel >>= 8;
          uint r = (byte)sourcePixel;
          b *= byte.MaxValue;
          g *= byte.MaxValue;
          r *= byte.MaxValue;
          b1 *= alpha;
          g1 *= alpha;
          r1 *= alpha;
          b += b1;
          g += g1;
          r += r1;

          b = (uint)(b * factor);
          g = (uint)(g * factor);
          r = (uint)(r * factor);

          newPixel |=
            b
            | (g << 8)
            | (r << 16)
            ;

          *(uint*)to = newPixel;
        } // x
      } // y
    }

#else
      public override void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        if (!(other is ARGB32BitmapLocker otherLocker)) {
          base.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
          return;
        }

        var thisStride = this.BitmapData.Stride;
        var otherStride = otherLocker.BitmapData.Stride;
        const int bpp = 4;

#if SUPPORTS_POINTER_ARITHMETIC
        var thisOffset = this.BitmapData.Scan0 + yt * thisStride + xt * bpp;
        var otherOffset = otherLocker.BitmapData.Scan0 + ys * otherStride + xs * bpp;
#else
        var thisOffset = _Add(this.BitmapData.Scan0, yt * thisStride + xt * bpp);
        var otherOffset = _Add(otherLocker.BitmapData.Scan0, ys * otherStride + xs * bpp);
#endif

#if SUPPORTS_POINTER_ARITHMETIC
        for (; height > 0; thisOffset += thisStride, otherOffset += otherStride, --height) {
#else
        for (; height > 0; _Add(ref thisOffset, thisStride), _Add(ref otherOffset, otherStride), --height) {
#endif
          var to = thisOffset;
          var oo = otherOffset;

#if SUPPORTS_POINTER_ARITHMETIC
          for (var x = width; x > 0; to += bpp, oo += bpp, --x) {
#else
          for (var x = width; x > 0; _Add(ref to, bpp), _Add(ref oo, bpp), --x) {
#endif
            var otherPixel = (uint)Marshal.ReadInt32(oo);
            if (otherPixel < 0x01000000)
              continue;

            if (otherPixel >= 0xff000000) {
              Marshal.WriteInt32(to, (int)otherPixel);
              continue;
            }

            var sourcePixel = (uint)Marshal.ReadInt32(to);

            var alpha = otherPixel >> 24;
            var factor = 1d / (255 + alpha);

            var b = (uint)(((byte)(sourcePixel) * byte.MaxValue + (byte)(otherPixel) * alpha) * factor);
            var g = (uint)(((byte)(sourcePixel >> 8) * byte.MaxValue + (byte)(otherPixel >> 8) * alpha) * factor);
            var r = (uint)(((byte)(sourcePixel >> 16) * byte.MaxValue + (byte)(otherPixel >> 16) * alpha) * factor);

            var newPixel =
                (sourcePixel & 0xff000000)
                | b
                | (g << 8)
                | (r << 16)
              ;

            Marshal.WriteInt32(to, (int)newPixel);
          } // x
        } // y
      }

#endif
  } // ARGB32BitmapLocker

  private sealed class RGB32BitmapLocker : BitmapLockerBase {
    public RGB32BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) : base(bitmap, rect, flags, format) { }

    protected override int _BytesPerPixel => 4;

    public override Color this[int x, int y] {
      get {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          pointer += offset;
          var b = pointer[0];
          var g = pointer[1];
          var r = pointer[2];
          return Color.FromArgb(r, g, b);
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
#if SUPPORTS_POINTER_ARITHMETIC
          pointer += offset;
#else
          _Add(ref pointer, offset);
#endif
          var b = Marshal.ReadByte(pointer, 0);
          var g = Marshal.ReadByte(pointer, 1);
          var r = Marshal.ReadByte(pointer, 2);
          return Color.FromArgb(r, g, b);

#endif
      }
      set {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          pointer += offset;
          pointer[0] = value.B;
          pointer[1] = value.G;
          pointer[2] = value.R;
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
#if SUPPORTS_POINTER_ARITHMETIC
          pointer += offset;
#else
          _Add(ref pointer, offset);
#endif
          Marshal.WriteByte(pointer, 0, value.B);
          Marshal.WriteByte(pointer, 1, value.G);
          Marshal.WriteByte(pointer, 2, value.R);

#endif
      }
    }

#if UNSAFE

    protected override unsafe void _DrawHorizontalLine(int x, int y, int count, Color color) {
      var data = this.BitmapData;
      var pointer = (byte*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride;
      var offset = stride * y + (x << 2);
      pointer += offset;
      var red = color.R;
      var green = color.G;
      var blue = color.B;
      var mask = (ushort)((green << 8) | blue);

      // unrolled loop 2 times (saving 50% of comparisons, using the highest IL-constant opcode of 8 [ldc.i4.8])
      {
        const int pixelsPerLoop = 2;
        const int pointerIncreasePerLoop = 8;
        while (count >= pixelsPerLoop) {
          *(ushort*)pointer = mask;
          pointer[2] = red;

          ((ushort*)pointer)[2] = mask;
          pointer[6] = red;

          pointer += pointerIncreasePerLoop;
          count -= pixelsPerLoop;
        }
      }

      while (count-- > 0) {
        *(ushort*)pointer = mask;
        pointer[2] = red;
        pointer += 4;
      }
    }

    protected override unsafe void _DrawVerticalLine(int x, int y, int count, Color color) {
      var data = this.BitmapData;
      var pointer = (byte*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride;
      var offset = stride * y + (x << 2);
      pointer += offset;
      var red = color.R;
      var green = color.G;
      var blue = color.B;
      var mask = (ushort)((green << 8) | blue);
      while (count-- > 0) {
        *(ushort*)pointer = mask;
        pointer[2] = red;
        pointer += stride;
      }
    }

#endif
  } // RGB32BitmapLocker

  private sealed class RGB24BitmapLocker : BitmapLockerBase {
    public RGB24BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) : base(bitmap,
      rect, flags, format) { }

    protected override int _BytesPerPixel => 3;

    public override Color this[int x, int y] {
      get {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
          pointer += offset;
          var b = pointer[0];
          var g = pointer[1];
          var r = pointer[2];
          return Color.FromArgb(r, g, b);
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
#if SUPPORTS_POINTER_ARITHMETIC
          pointer += offset;
#else
          _Add(ref pointer, offset);
#endif
          var b = Marshal.ReadByte(pointer, 0);
          var g = Marshal.ReadByte(pointer, 1);
          var r = Marshal.ReadByte(pointer, 2);
          return Color.FromArgb(r, g, b);

#endif
      }
      set {
#if UNSAFE

        unsafe {
          var data = this.BitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
          pointer += offset;
          pointer[0] = value.B;
          pointer[1] = value.G;
          pointer[2] = value.R;
        }

#else
          var data = this.BitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
#if SUPPORTS_POINTER_ARITHMETIC
          pointer += offset;
#else
          _Add(ref pointer, offset);
#endif
          Marshal.WriteByte(pointer, 0, value.B);
          Marshal.WriteByte(pointer, 1, value.G);
          Marshal.WriteByte(pointer, 2, value.R);

#endif
      }
    }

#if UNSAFE

    protected override unsafe void _DrawHorizontalLine(int x, int y, int count, Color color) {
      Debug.Assert(count > 0);
      var data = this.BitmapData;
      var pointer = (byte*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride;
      var offset = stride * y + x * 3;
      var blue = color.B;
      var green = color.G;
      var red = color.R;
      pointer += offset;
      var mask = (ushort)((green << 8) | blue);
      if (count >= 4) {
        // align offset for later access
        var paddingBytes = (int)((long)pointer & 3);
        if (paddingBytes > 0) {
          // 1 --> 4
          // 2 --> 5 --> 8
          // 3 --> 6 --> 9 --> 12

          // ReSharper disable once SwitchStatementMissingSomeCases
          switch (paddingBytes) {
            case 3:
              *(ushort*)pointer = mask;
              pointer[2] = red;
              pointer += 3;
              goto case 2;
            case 2:
              *(ushort*)pointer = mask;
              pointer[2] = red;
              pointer += 3;
              goto case 1;
            case 1:
              *(ushort*)pointer = mask;
              pointer[2] = red;
              pointer += 3;
              break;
          }

          count -= paddingBytes;
        }

        // prepare 4 pixel = 3 uint masks
        var mask1 = (uint)((blue << 24) | (red << 16) | (green << 8) | blue);
        var mask2 = (uint)((green << 24) | (blue << 16) | (red << 8) | green);
        var mask3 = (uint)((red << 24) | (green << 16) | (blue << 8) | red);

        // unrolled loop 3 times (saving 75% of comparisons, using the highest IL-constant opcode of 8 [ldc.i4.8])
        {
          const int pixelsPerLoop = 12;
          const int pointerIncreasePerLoop = 36;
          while (count >= pixelsPerLoop) {
            var i = (uint*)pointer;
            i[0] = mask1;
            i[1] = mask2;
            i[2] = mask3;

            i[3] = mask1;
            i[4] = mask2;
            i[5] = mask3;

            i[6] = mask1;
            i[7] = mask2;
            i[8] = mask3;

            pointer += pointerIncreasePerLoop;
            count -= pixelsPerLoop;
          }
        }

        {
          const int pixelsPerLoop = 4;
          const int pointerIncreasePerLoop = 12;
          while (count >= 4) {
            var i = (uint*)pointer;
            i[0] = mask1;
            i[1] = mask2;
            i[2] = mask3;
            pointer += pointerIncreasePerLoop;
            count -= pixelsPerLoop;
          }
        }
      }

      // fill pixels left
      while (count-- > 0) {
        *(ushort*)pointer = mask;
        pointer[2] = red;
        pointer += 3;
      }
    }

    protected override unsafe void _DrawVerticalLine(int x, int y, int count, Color color) {
      Debug.Assert(count > 0);
      var data = this.BitmapData;
      var pointer = (byte*)data.Scan0;
      Debug.Assert(pointer != null, nameof(pointer) + " != null");
      var stride = data.Stride;
      var offset = stride * y + x * 3;
      var blue = color.B;
      var green = color.G;
      var red = color.R;
      var mask = (ushort)((green << 8) | blue);
      pointer += offset;
      while (count-- > 0) {
        *(ushort*)pointer = mask;
        pointer[2] = red;
        pointer += stride;
      }
    }

#endif
  } // RGB24BitmapLocker

  private sealed class UnsupportedDrawingBitmapLocker : BitmapLockerBase {
    public UnsupportedDrawingBitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) : base(bitmap, rect, flags, format) {
      this._exception = new(
        $"Wrong pixel format {format} (supported: {string.Join(",", _LOCKER_TYPES.Keys.Select(i => i.ToString()).ToArray())})"
      );
    }

    private readonly NotSupportedException _exception;

    public override Color this[int x, int y] {
      get => throw this._exception;
      set => throw this._exception;
    }
  } // UnsupportedDrawingBitmapLocker

  #endregion

  private delegate IBitmapLocker LockerFactory(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format);

  private static readonly Dictionary<PixelFormat, LockerFactory> _LOCKER_TYPES = new() {
    { PixelFormat.Format32bppArgb, (b, r, f, f2) => new ARGB32BitmapLocker(b, r, f, f2) },
    { PixelFormat.Format32bppRgb, (b, r, f, f2) => new RGB32BitmapLocker(b, r, f, f2) },
    { PixelFormat.Format24bppRgb, (b, r, f, f2) => new RGB24BitmapLocker(b, r, f, f2) },
  };

  #endregion

  #region method delegates

  public delegate void MemoryCopyDelegate(IntPtr source, IntPtr target, int count);

  public delegate void MemoryFillDelegate(IntPtr source, byte value, int count);

  public static MemoryCopyDelegate _memoryCopyCall = NativeMethods.MemoryCopy;

  public static MemoryCopyDelegate MemoryCopyCall {
    get => _memoryCopyCall;
    // ReSharper disable once LocalizableElement
    set => _memoryCopyCall = value ?? throw new ArgumentNullException(nameof(value), "There must be a valid method pointer");
  }

  public static MemoryFillDelegate _memoryFillCall = NativeMethods.MemorySet;

  public static MemoryFillDelegate MemoryFillCall {
    get => _memoryFillCall;
    // ReSharper disable once LocalizableElement
    set => _memoryFillCall = value ?? throw new ArgumentNullException(nameof(value), "There must be a valid method pointer");
  }

  #endregion

#if !SUPPORTS_POINTER_ARITHMETIC
  private static IntPtr _Add(IntPtr src, int count) => new(src.ToInt64() + count);
  private static void _Add(ref IntPtr src, int count) => src = _Add(src, count);
#endif

  #region Lock

  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags, PixelFormat format)
    => _LOCKER_TYPES.TryGetValue(format, out var factory)
      ? factory(@this, rect, flags, format)
      : new UnsupportedDrawingBitmapLocker(@this, rect, flags, format);

  public static IBitmapLocker Lock(this Bitmap @this) => Lock(@this, new(Point.Empty, @this.Size), ImageLockMode.ReadWrite, @this.PixelFormat);

  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect) => Lock(@this, rect, ImageLockMode.ReadWrite, @this.PixelFormat);

  public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags) => Lock(@this, new(Point.Empty, @this.Size), flags, @this.PixelFormat);

  public static IBitmapLocker Lock(this Bitmap @this, PixelFormat format) => Lock(@this, new(Point.Empty, @this.Size), ImageLockMode.ReadWrite, format);

  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags) => Lock(@this, rect, flags, @this.PixelFormat);

  public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, PixelFormat format) => Lock(@this, rect, ImageLockMode.ReadWrite, format);

  public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags, PixelFormat format) => Lock(@this, new(Point.Empty, @this.Size), flags, format);

  #endregion

  public static Bitmap ConvertPixelFormat(this Bitmap @this, PixelFormat format) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    if (@this.PixelFormat == format)
      return (Bitmap)@this.Clone();

    var result = new Bitmap(@this.Width, @this.Height, format);

#if UNSAFE

    var sourceFormat = @this.PixelFormat;

    switch (sourceFormat) {
      case PixelFormat.Format24bppRgb when format == PixelFormat.Format32bppArgb: {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat);
        using var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format);
        unsafe {
          var source = (byte*)sourceData.BitmapData.Scan0;
          Debug.Assert(source != null, nameof(source) + " != null");
          var target = (byte*)targetData.BitmapData.Scan0;
          Debug.Assert(target != null, nameof(target) + " != null");

          var sourceStride = sourceData.BitmapData.Stride;
          var targetStride = targetData.BitmapData.Stride;
          for (var y = @this.Height; y > 0; --y) {
            var sourceRow = source;
            var targetRow = target;
            for (var x = @this.Width; x > 0; --x) {
              var bg = *(ushort*)sourceRow;
              var r = sourceRow[2];

              *(ushort*)targetRow = bg;
              targetRow[2] = r;
              targetRow[3] = 0xff;

              sourceRow += 3;
              targetRow += 4;
            }

            source += sourceStride;
            target += targetStride;
          }
        }

        return result;
      }
      case PixelFormat.Format32bppArgb when format == PixelFormat.Format24bppRgb: {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat);
        using var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format);
        unsafe {
          var source = (byte*)sourceData.BitmapData.Scan0;
          Debug.Assert(source != null, nameof(source) + " != null");
          var target = (byte*)targetData.BitmapData.Scan0;
          Debug.Assert(target != null, nameof(target) + " != null");

          var sourceStride = sourceData.BitmapData.Stride;
          var targetStride = targetData.BitmapData.Stride;
          for (var y = @this.Height; y > 0; --y) {
            var sourceRow = source;
            var targetRow = target;
            for (var x = @this.Width; x > 0; --x) {
              var bg = *(ushort*)sourceRow;
              var r = sourceRow[2];

              *(ushort*)targetRow = bg;
              targetRow[2] = r;

              sourceRow += 4;
              targetRow += 3;
            }

            source += sourceStride;
            target += targetStride;
          }
        }

        return result;
      }
    }

#endif

    using var g = Graphics.FromImage(result);
    g.CompositingMode = CompositingMode.SourceCopy;
    g.InterpolationMode = InterpolationMode.NearestNeighbor;
    g.DrawImage(@this, Point.Empty);

    return result;
  }

  public static Bitmap Crop(this Bitmap @this, Rectangle rect, PixelFormat format = PixelFormat.DontCare) {
    rect = Rectangle.FromLTRB(rect.Left, rect.Top, Math.Min(rect.Right, @this.Width), Math.Min(rect.Bottom, @this.Height));

    var result = new Bitmap(rect.Width, rect.Height, format == PixelFormat.DontCare ? @this.PixelFormat : format);
    using var g = Graphics.FromImage(result);
    g.CompositingMode = CompositingMode.SourceCopy;
    g.CompositingQuality = CompositingQuality.HighSpeed;
    g.InterpolationMode = InterpolationMode.NearestNeighbor;
    g.DrawImage(@this, new Rectangle(Point.Empty, new(rect.Width, rect.Height)), rect, GraphicsUnit.Pixel);

    return result;
  }

  public static Bitmap Resize(this Bitmap @this, int width, int height, InterpolationMode mode = InterpolationMode.Bicubic) {
    var result = new Bitmap(width, height, @this.PixelFormat);
    using var graphics = Graphics.FromImage(result);
    graphics.CompositingMode = CompositingMode.SourceCopy;
    graphics.InterpolationMode = mode;
    graphics.DrawImage(@this, new Rectangle(Point.Empty, result.Size), new(Point.Empty, @this.Size), GraphicsUnit.Pixel);

    return result;
  }
}
