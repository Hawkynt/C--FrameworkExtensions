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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global


namespace System.Drawing {
  // ReSharper disable once PartialTypeWithSinglePart
  internal static partial class BitmapExtensions {

    #region nested types
    
    // ReSharper disable once PartialTypeWithSinglePart
    private static partial class NativeMethods {
      
      [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
      private static extern IntPtr _MemoryCopy(IntPtr dst, IntPtr src, int count);

      [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
      private static extern IntPtr _MemorySet(IntPtr dst, int value, int count);

      public static void MemoryCopy(IntPtr source, IntPtr target, int count)
        => _MemoryCopy(target, source, count)
      ;
    
      public static void MemorySet(IntPtr source, byte value, int count)
        => _MemorySet(source, value, count)
      ;

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
      void DrawRectangle(Rectangle rect, Color color);
      void FillRectangle(Rectangle rect, Color color);
      
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

      void CopyFromGrid(IBitmapLocker other, int column, int row, int width, int height, int dx = 0, int dy = 0,int offsetX = 0, int offsetY = 0, int targetX = 0, int targetY = 0);
      void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize);
      void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance);
      void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset);
      void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target);
      
      /// <summary>
      /// <c>true</c> when all pixels have the same color; otherwise, <c>false</c>.
      /// </summary>
      bool IsFlatColor { get; }
      int Width { get; }
      int Height { get; }
    }

    private class BitmapLocker : IBitmapLocker {

      private interface IPixelProcessor {
        Color this[int x, int y] { get; set; }
        void DrawHorizontalLine(int x, int y, int count, Color color);
        void DrawVerticalLine(int x, int y, int count, Color color);
        void FillRectangle(int x, int y, int width, int height, Color color);
        void CopyFrom(IPixelProcessor other, int xs, int ys, int xt, int yt, int width, int height);
      }

      private abstract class PixelProcessorBase : IPixelProcessor {
        protected readonly BitmapData _bitmapData;
        protected readonly int _bytesPerPixel;

        protected PixelProcessorBase(BitmapData bitmapData) {
          this._bitmapData = bitmapData;
          switch (bitmapData.PixelFormat) {
            case PixelFormat.Format32bppArgb:
            case PixelFormat.Format32bppRgb:
            case PixelFormat.Format32bppPArgb:
              this._bytesPerPixel = 4;
              break;
            case PixelFormat.Format24bppRgb:
              this._bytesPerPixel = 3;
              break;
            case PixelFormat.Format48bppRgb:
              this._bytesPerPixel = 6;
              break;
            case PixelFormat.Format64bppArgb:
            case PixelFormat.Format64bppPArgb:
              this._bytesPerPixel = 8;
              break;
            case PixelFormat.Format16bppArgb1555:
            case PixelFormat.Format16bppGrayScale:
            case PixelFormat.Format16bppRgb555:
            case PixelFormat.Format16bppRgb565:
              this._bytesPerPixel = 2;
              break;
          }
        }

        public abstract Color this[int x, int y] { get; set; }

        public virtual void DrawHorizontalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          while (count-- > 0)
            this[x++, y] = color;
        }

        public virtual void DrawVerticalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          while (count-- > 0)
            this[x, y++] = color;
        }

        public virtual void FillRectangle(int x, int y, int width, int height, Color color) {
          Debug.Assert(width > 0);
          Debug.Assert(height > 0);
          for (var i = height; i > 0; ++y, --i)
            this.DrawHorizontalLine(x, y, width, color);
        }

        public void CopyFrom(IPixelProcessor other, int xs, int ys, int xt, int yt, int width, int height) {
          if (width == 0 || height == 0)
            return;

          // copy faster if both have the same pixel format
          if (other is PixelProcessorBase ppb) {
            var bitmapDataTarget = this._bitmapData;
            var pixelFormat = bitmapDataTarget.PixelFormat;
            var bitmapDataSource = ppb._bitmapData;
            if (pixelFormat == bitmapDataSource.PixelFormat) {
              var bytesPerPixel = this._bytesPerPixel;
              if (bytesPerPixel > 0) {
                var sourceStride = bitmapDataSource.Stride;
                var targetStride = bitmapDataTarget.Stride;

                var yOffsetTarget = bitmapDataTarget.Scan0 + (targetStride * yt + bytesPerPixel * xt);
                var yOffsetSource = bitmapDataSource.Scan0 + (sourceStride * ys + bytesPerPixel * xs);
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

                  height7: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height6: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height5: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height4: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height3: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height2: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine); yOffsetSource += sourceStride; yOffsetTarget += targetStride;
                  height1: _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                  height0: return;
                  heightAbove7:

                  var heightOcts = height >> 3;
                  height &= 7;

                  // unrolled loop, copying 8 lines in one go - increasing performance roughly by 20% according to benchmarks
                  do {
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                    _memoryCopyCall(yOffsetSource, yOffsetTarget, byteCountPerLine);
                    yOffsetSource += sourceStride;yOffsetTarget += targetStride;
                  } while (--heightOcts > 0);

                } while (true);

              } // end if bytes-per-pixel > 0
            } // end if other has same pixel format
          } // end if other is also PixelProcessorBase

          // slow way of copying
          for (var y = 0; y < height; ++ys, ++yt, ++y)
          for (int x = 0, xcs = xs, xct = xt; x < width; ++xcs, ++xct, ++x)
            this[xct, yt] = other[xcs, ys];
        }
      }

      #region optimized pixel formats

      private sealed class Unsupported : IPixelProcessor {
        private readonly NotSupportedException _exception;
        public Unsupported(PixelFormat format) => this._exception = new NotSupportedException(
          // ReSharper disable once ArrangeRedundantParentheses
          (_SUPPORTED_PIXEL_PROCESSORS.Count == 0
            ? "No supported pixel formats"
            : $"Wrong pixel format {format} (supported: {string.Join(",", _SUPPORTED_PIXEL_PROCESSORS.Keys.Select(i => i.ToString()))})"
          )
#if !UNSAFE
          + " - try compile in unsafe mode to support more"
#endif
        );

        public Color this[int x, int y] {
          get => throw this._exception;
          set => throw this._exception;
        }

        public void DrawHorizontalLine(int x, int y, int count, Color color) => throw this._exception;
        public void DrawVerticalLine(int x, int y, int count, Color color) => throw this._exception;
        public void FillRectangle(int x, int y, int width, int height, Color color) => throw this._exception;
        public void CopyFrom(IPixelProcessor other, int xs, int ys, int xt, int yt, int width, int height) =>throw this._exception;

      }
      
      private class RGB24 : PixelProcessorBase {
        public RGB24(BitmapData bitmapData) : base(bitmapData) { }

        public override Color this[int x, int y] {
          get {

#if UNSAFE

            unsafe {
              var data = this._bitmapData;
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

            var data = this._bitmapData;
            var pointer = data.Scan0;
            Debug.Assert(pointer != null, nameof(pointer) + " != null");
            var stride = data.Stride;
            var offset = stride * y + x * 3;
            pointer += offset;
            var b = Marshal.ReadByte(pointer, 0);
            var g = Marshal.ReadByte(pointer, 1);
            var r = Marshal.ReadByte(pointer, 2);
            return Color.FromArgb(r, g, b);

#endif

          }
          set {

#if UNSAFE

            unsafe {
              var data = this._bitmapData;
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

            var data = this._bitmapData;
            var pointer = data.Scan0;
            Debug.Assert(pointer != null, nameof(pointer) + " != null");
            var stride = data.Stride;
            var offset = stride * y + x * 3;
            pointer += offset;
            Marshal.WriteByte(pointer, 0, value.B);
            Marshal.WriteByte(pointer, 1, value.G);
            Marshal.WriteByte(pointer, 2, value.R);

#endif

          }
        }

#if UNSAFE

        public override unsafe void DrawHorizontalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          var data = this._bitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
          var blue = color.B;
          var green = color.G;
          var red = color.R;
          pointer += offset;
          var mask = (ushort)(green << 8 | blue);
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
            var mask1 = (uint)(blue << 24 | red << 16 | green << 8 | blue);
            var mask2 = (uint)(green << 24 | blue << 16 | red << 8 | green);
            var mask3 = (uint)(red << 24 | green << 16 | blue << 8 | red);

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

        public override unsafe void DrawVerticalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          var data = this._bitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
          var blue = color.B;
          var green = color.G;
          var red = color.R;
          var mask = (ushort)(green << 8 | blue);
          pointer += offset;
          while (count-- > 0) {
            *(ushort*)pointer = mask;
            pointer[2] = red;
            pointer += stride;
          }
        }

#endif

        public override void FillRectangle(int x, int y, int width, int height, Color color) {
          Debug.Assert(width > 0);
          Debug.Assert(height > 0);

          this.DrawHorizontalLine(x, y, width, color);
          if (height <= 1)
            return;

          var data = this._bitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + x * 3;
          pointer += offset;
          var nextPointer = pointer + stride;
          while (--height > 0) {
            _memoryCopyCall(pointer, nextPointer, stride);
            nextPointer += stride;
          }
        }

      }

      private class RGB32 : PixelProcessorBase {
        public RGB32(BitmapData bitmapData) : base(bitmapData) { }

        public override Color this[int x, int y] {
          get {

#if UNSAFE

            unsafe {
              var data = this._bitmapData;
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

            var data = this._bitmapData;
            var pointer = data.Scan0;
            Debug.Assert(pointer != null, nameof(pointer) + " != null");
            var stride = data.Stride;
            var offset = stride * y + (x << 2);
            pointer += offset;
            var b = Marshal.ReadByte(pointer, 0);
            var g = Marshal.ReadByte(pointer, 1);
            var r = Marshal.ReadByte(pointer, 2);
            return Color.FromArgb(r, g, b);

#endif

          }
          set {

#if UNSAFE

            unsafe {
              var data = this._bitmapData;
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

            var data = this._bitmapData;
            var pointer = data.Scan0;
            Debug.Assert(pointer != null, nameof(pointer) + " != null");
            var stride = data.Stride;
            var offset = stride * y + (x << 2);
            pointer += offset;
            Marshal.WriteByte(pointer, 0, value.B);
            Marshal.WriteByte(pointer, 1, value.G);
            Marshal.WriteByte(pointer, 2, value.R);

#endif

          }
        }

#if UNSAFE

        public override unsafe void DrawHorizontalLine(int x, int y, int count, Color color) {
          var data = this._bitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          pointer += offset;
          var red = color.R;
          var green = color.G;
          var blue = color.B;
          var mask = (ushort)(green << 8 | blue);

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

        public override unsafe void DrawVerticalLine(int x, int y, int count, Color color) {
          var data = this._bitmapData;
          var pointer = (byte*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          pointer += offset;
          var red = color.R;
          var green = color.G;
          var blue = color.B;
          var mask = (ushort)(green << 8 | blue);
          while (count-- > 0) {
            *(ushort*)pointer = mask;
            pointer[2] = red;
            pointer += stride;
          }
        }

#endif

      }

      private class ARGB32 : PixelProcessorBase {
        public ARGB32(BitmapData bitmapData) : base(bitmapData) { }

        public override Color this[int x, int y] {
          get {

#if UNSAFE

            unsafe {
              var data = this._bitmapData;
              var pointer = (int*)data.Scan0;
              Debug.Assert(pointer != null, nameof(pointer) + " != null");
              var stride = data.Stride >> 2;
              var offset = stride * y + x;
              return Color.FromArgb(pointer[offset]);
            }

#else

            var data = this._bitmapData;
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
              var data = this._bitmapData;
              var pointer = (int*)data.Scan0;
              Debug.Assert(pointer != null, nameof(pointer) + " != null");
              var stride = data.Stride >> 2;
              var offset = stride * y + x;
              pointer[offset] = value.ToArgb();
            }

#else

            var data = this._bitmapData;
            var pointer = data.Scan0;
            Debug.Assert(pointer != null, nameof(pointer) + " != null");
            var stride = data.Stride;
            var offset = stride * y + (x << 2);
            Marshal.WriteInt32(pointer, offset, value.ToArgb());

#endif

          }
        }

#if UNSAFE

        public override unsafe void DrawHorizontalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          var data = this._bitmapData;
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

        public override unsafe void DrawVerticalLine(int x, int y, int count, Color color) {
          Debug.Assert(count > 0);
          var data = this._bitmapData;
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

        public override void FillRectangle(int x, int y, int width, int height, Color color) {
          Debug.Assert(width > 0);
          Debug.Assert(height > 0);

          this.DrawHorizontalLine(x, y, width, color);
          if (height <= 1)
            return;

          var data = this._bitmapData;
          var pointer = data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride;
          var offset = stride * y + (x << 2);
          pointer += offset;
          var nextPointer = pointer + stride;
          // TODO: allow filling multiple strides if possible
          while (--height > 0) {
            _memoryCopyCall(pointer, nextPointer, stride);
            nextPointer += stride;
          }
        }

#endif

      }

      #endregion

      private static readonly Dictionary<PixelFormat, Func<BitmapData, IPixelProcessor>> _SUPPORTED_PIXEL_PROCESSORS = new Dictionary<PixelFormat, Func<BitmapData, IPixelProcessor>> {
        {PixelFormat.Format24bppRgb,l=>new RGB24(l)},
        {PixelFormat.Format32bppRgb,l=>new RGB32(l)},
        {PixelFormat.Format32bppArgb,l=>new ARGB32(l)},
      };

      private readonly Bitmap _bitmap;
      private readonly IPixelProcessor _pixelProcessor;

      public BitmapData BitmapData { get; }

      public BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) {
        this._bitmap = bitmap;
          this.BitmapData = bitmap.LockBits(rect, flags, format);
        this._pixelProcessor = _SUPPORTED_PIXEL_PROCESSORS.TryGetValue(format, out var factory)
          ? factory(this.BitmapData)
          : new Unsupported(format)
          ;
      }

      #region Implementation of IDisposable

      private bool _isDisposed;

      ~BitmapLocker() => this.Dispose();

      public void Dispose() {
        if (this._isDisposed)
          return;

        this._isDisposed = true;

        if (this._bitmap != null && this.BitmapData != null)
          this._bitmap.UnlockBits(this.BitmapData);
      }

      #endregion

      public Color this[int x, int y] {
        get => this._pixelProcessor[x, y];
        set => this._pixelProcessor[x, y] = value;
      }

      public Color this[Point p]
      {
        get => this[p.X,p.Y];
        set => this[p.X,p.Y] = value;
      }

      #region Rectangles

      public void DrawRectangle(Rectangle rect, Color color) {
        var count = rect.Width + 1;
        this.DrawHorizontalLine(rect.Left, rect.Top, count, color);
        this.DrawHorizontalLine(rect.Left, rect.Bottom, count, color);
        count = rect.Bottom - rect.Top - 2;
        this.DrawVerticalLine(rect.Left, rect.Top + 1, count, color);
        this.DrawVerticalLine(rect.Right, rect.Top + 1, count, color);
      }

      public void FillRectangle(Rectangle rect, Color color) {
        if (rect.Width < 1 || rect.Height < 1)
          return;

        this._pixelProcessor.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);
      }

      #endregion

      #region CopyFrom

      public void CopyFrom(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
          this.CopyFromUnchecked(other, xs, ys, width, height, xt, yt);
      }
      
#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other) => this.CopyFrom(other, 0, 0, other.Width, other.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other, Point target) => this.CopyFrom(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other, Point source, Size size) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFrom(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other, Rectangle source) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFrom(IBitmapLocker other, Rectangle source, Point target) => this.CopyFrom(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);


      public void CopyFromChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
        this.CopyFromUnchecked(other,xs,ys,width,height,xt,yt);
      }

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height, 0,0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other, Point target) => this.CopyFromChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other, Point source, Size size) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other, Rectangle source) => this.CopyFromChecked(other,source.X,source.Y,source.Width,source.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromChecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromChecked(other, source.X, source.Y, source.Width, source.Height,target.X,target.Y);

      public void CopyFromUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        switch (other) {
          case BitmapLocker bl:
            // directly use pixel processor if possible
            this._pixelProcessor.CopyFrom(bl._pixelProcessor, xs, ys, xt, yt, width, height);
            break;
          default: {
              // slow way
              for (var y = 0; y < height; ++ys, ++yt, ++y)
                for (int x = 0, xcs = xs, xct = xt; x < width; ++xcs, ++xct, ++x)
                  this[xct, yt] = other[xcs, ys];
              break;
            }
        }
      }

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other, Point target) => this.CopyFromUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other, Rectangle source) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.CopyFromUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

      #endregion

      #region CopyFromGrid

      public void CopyFromGrid(IBitmapLocker other, int column, int row, int width, int height, int dx = 0, int dy = 0,int offsetX = 0, int offsetY = 0, int targetX = 0, int targetY = 0) {
        var sourceX = column * (width + dx) + offsetX;
        var sourceY = row * (height + dy) + offsetY;
        this.CopyFromChecked(other, sourceX, sourceY, width, height, targetX, targetY);
      }

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize)
        => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height)
      ;

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance)
        => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height)
      ;

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset)
        => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height, offset.Width, offset.Height)
      ;

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void CopyFromGrid(IBitmapLocker other, Point tile, Size tileSize, Size distance, Size offset, Point target) 
        => this.CopyFromGrid(other, tile.X, tile.Y, tileSize.Width, tileSize.Height, distance.Width, distance.Height,offset.Width, offset.Height, target.X, target.Y)
      ;

      #endregion

      #region BlendWith

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other) => this.BlendWith(other, 0, 0, other.Width, other.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other, Point target) => this.BlendWith(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other, Point source, Size size) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWith(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other, Rectangle source) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWith(IBitmapLocker other, Rectangle source, Point target) => this.BlendWith(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

      public void BlendWith(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        if (this._FixCopyParametersToBeInbounds(other, ref xs, ref ys, ref width, ref height, ref xt, ref yt))
          this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
      }
      
#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other,Point target) => this.BlendWithChecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other, Point source,Size size) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other, Point source, Size size,Point target) => this.BlendWithChecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other, Rectangle source) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithChecked(IBitmapLocker other, Rectangle source,Point target) => this.BlendWithChecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

      public void BlendWithChecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        this._CheckCopyParameters(other, xs, ys, width, height, xt, yt);
        this.BlendWithUnchecked(other, xs, ys, width, height, xt, yt);
      }

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other, Point target) => this.BlendWithUnchecked(other, 0, 0, other.Width, other.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other, Point source, Size size, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, size.Width, size.Height, target.X, target.Y);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other, Rectangle source) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height, 0, 0);

#if NET45
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      public void BlendWithUnchecked(IBitmapLocker other, Rectangle source, Point target) => this.BlendWithUnchecked(other, source.X, source.Y, source.Width, source.Height, target.X, target.Y);

      // TODO: optimize me
      public void BlendWithUnchecked(IBitmapLocker other, int xs, int ys, int width, int height, int xt = 0, int yt = 0) {
        for (var y = ys; height > 0; ++y, ++yt, --height)
        for (int x = xs, xct = xt, i = width; i > 0; ++x, ++xct, --i) {
          var sourcePixel = this[xct, yt];
          var otherPixel = other[x, y];
          Color newPixel;
          if (otherPixel.A == 255)
            newPixel = otherPixel;
          else if (otherPixel.A == 0)
            newPixel = sourcePixel;
          else {
            var factor = 255 + otherPixel.A;
            var r = sourcePixel.R * 255 + otherPixel.R * otherPixel.A;
            var g = sourcePixel.G * 255 + otherPixel.G * otherPixel.A;
            var b = sourcePixel.B * 255 + otherPixel.B * otherPixel.A;
            r /= factor;
            g /= factor;
            b /= factor;
            newPixel = Color.FromArgb(sourcePixel.A, r, g, b);
          }

          this[xct, yt] = newPixel;
        }
      }

      #endregion

      [DebuggerHidden]
      private void _CheckCopyParameters(IBitmapLocker other, int xs, int ys, int width, int height, int xt, int yt) {
        if (other == null)
          throw new ArgumentNullException(nameof(other));
        if (xs < 0 || xs >= other.Width)
          throw new ArgumentOutOfRangeException(nameof(xs));
        if (ys < 0 || ys >= other.Height)
          throw new ArgumentOutOfRangeException(nameof(ys));
        if (width < 1 || (xs + width) > other.Width)
          throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1 || (ys + height) > other.Height)
          throw new ArgumentOutOfRangeException(nameof(height));
        if (xt < 0 || (xt + width) > this.Width)
          throw new ArgumentOutOfRangeException(nameof(xt));
        if (yt < 0 || (yt + height) > this.Height)
          throw new ArgumentOutOfRangeException(nameof(yt));
      }

      private bool _FixCopyParametersToBeInbounds(IBitmapLocker other, ref int xs, ref int ys, ref int width, ref int height, ref int xt, ref int yt) {
        if (other == null)
          throw new ArgumentNullException(nameof(other));

        if (xs < 0) {
          width += xs;
          xs = 0;
        }

        if (ys < 0) {
          height += ys;
          ys = 0;
        }

        if (xs >= other.Width)
          return false;

        if (ys >= other.Height)
          return false;

        if (xs + width > other.Width)
          width = other.Width - xs;

        if (ys + height > other.Height)
          height = other.Height - ys;

        if (xt < 0) {
          width += xt;
          xt = 0;
        }

        if (yt < 0) {
          height += yt;
          yt = 0;
        }

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

      // TODO: optimize me
      public void Clear(Color color) => this._pixelProcessor.FillRectangle(0, 0, this.Width, this.Height, color);

      public int Width => this.BitmapData.Width;

      public int Height => this.BitmapData.Height;

      // TODO: optimize me
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

      #region lines

      public void DrawHorizontalLine(int x, int y, int count, Color color) {
        if (count == 0)
          return;
        if (count < 0)
          this._pixelProcessor.DrawHorizontalLine(x - count, y, -count, color);
        else
          this._pixelProcessor.DrawHorizontalLine(x, y, count, color);
      }

      public void DrawHorizontalLine(Point p, int count, Color color) => this.DrawHorizontalLine(p.X, p.Y, count, color);

      public void DrawVerticalLine(int x, int y, int count, Color color) {
        if (count == 0)
          return;
        if (count < 0)
          this._pixelProcessor.DrawVerticalLine(x, y - count, -count, color);
        else
          this._pixelProcessor.DrawVerticalLine(x, y, count, color);
      }

      public void DrawVerticalLine(Point p, int count, Color color) => this.DrawVerticalLine(p.X, p.Y, count, color);

      public void DrawLine(Point a, Point b, Color color) => this.DrawLine(a.X, a.Y, b.X, b.Y, color);

      public void DrawLine(int x0,int y0,int x1,int y1, Color color) {
        var dx = x1 - x0;
        var dy = y1 - y0;
        if (dx == 0) {
          this.DrawVerticalLine(x0,y0,y1-y0,color);
          return;
        }

        if (dy == 0) {
          this.DrawHorizontalLine(x0,y0,x1-x0,color);
          return;
        }

        this._DrawDiagonalLine(x0,y0,dx,dy,color);
      }

      private void _DrawDiagonalLine(int x0, int y0, int dx, int dy, Color color)
      {
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

        if (dx > dy)
        {
          pdx = incx; 
          pdy = 0;
          ddx = incx; 
          ddy = incy;
          deltaslowdirection = dy; 
          deltafastdirection = dx;
        }
        else
        {
          pdx = 0; 
          pdy = incy;
          ddx = incx;
          ddy = incy;
          deltaslowdirection = dx;
          deltafastdirection = dy;
        }

        var x = x0;
        var y = y0;
        var err = deltafastdirection >>1;
        this[x,y]=color;

        for (var t = 0; t < deltafastdirection; ++t)
        {
          err -= deltaslowdirection;
          if (err < 0)
          {
            err += deltafastdirection;
            x += ddx;
            y += ddy;
          }
          else
          {
            x += pdx;
            y += pdy;
          }
          this[x, y] = color;
        }
      }
      
      private void _DrawDiagonalLine2(int x0,int y0,int dx,int dy, Color color) {
        var xSign = Math.Sign(dx);
        var ySign = Math.Sign(dy);
        dx = Math.Abs(dx);
        dy = Math.Abs(dy);

        var x = x0;
        var y = y0;
        var runTo = Math.Max(dx, dy);
        dy = -dy;
        var epsilon = dx + dy;
        for (var i = 0; i <= runTo; ++i)
        {
          this[x,y]=color;
          var epsilon2 = epsilon << 1;
          if (epsilon2 > dy)
          {
            epsilon += dy;
            x += xSign;
          }

          if (epsilon2 < dx)
          {
            epsilon += dx;
            y += ySign;
          }
        }
      }

      #endregion

    }

    #endregion
    
    #region method delegates

    public delegate void MemoryCopyDelegate(IntPtr source, IntPtr target, int count);
    public delegate void MemoryFillDelegate(IntPtr source, byte value, int count);

    public static MemoryCopyDelegate _memoryCopyCall = NativeMethods.MemoryCopy;

    public static MemoryCopyDelegate MemoryCopyCall
    {
      get => _memoryCopyCall;
      set => _memoryCopyCall = value ?? throw new ArgumentNullException(nameof(value), "There must be a valid method pointer");
    }

    public static MemoryFillDelegate _memoryFillCall = NativeMethods.MemorySet;

    public static MemoryFillDelegate MemoryFillCall
    {
      get => _memoryFillCall;
      set => _memoryFillCall = value ?? throw new ArgumentNullException(nameof(value), "There must be a valid method pointer");
    }

    #endregion

    #region Lock

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags, PixelFormat format)
      => new BitmapLocker(@this, rect, flags, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), ImageLockMode.ReadWrite, @this.PixelFormat)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect)
      => Lock(@this, rect, ImageLockMode.ReadWrite, @this.PixelFormat)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), flags, @this.PixelFormat)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, PixelFormat format)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), ImageLockMode.ReadWrite, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags)
      => Lock(@this, rect, flags, @this.PixelFormat)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, PixelFormat format)
      => Lock(@this, rect, ImageLockMode.ReadWrite, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags, PixelFormat format)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), flags, format)
    ;

    #endregion

    public static Bitmap ConvertPixelFormat(this Bitmap @this, PixelFormat format) {
      if (@this == null)
        throw new ArgumentNullException(nameof(@this));

      if (@this.PixelFormat == format)
        return (Bitmap)@this.Clone();

      var result = new Bitmap(@this.Width, @this.Height, format);
      var sourceFormat = @this.PixelFormat;

#if UNSAFE

      if (sourceFormat == PixelFormat.Format24bppRgb && format == PixelFormat.Format32bppArgb) {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using (var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat))
        using (var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format))
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
                var bg = *((ushort*)sourceRow);
                var r = sourceRow[2];

                *((ushort*)targetRow) = bg;
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

      if (sourceFormat == PixelFormat.Format32bppArgb && format == PixelFormat.Format24bppRgb) {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using (var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat))
        using (var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format))
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

                var bg = *((ushort*)sourceRow);
                var r = sourceRow[2];

                *((ushort*)targetRow) = bg;
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

#endif

      using (var g = Graphics.FromImage(result)) {
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(@this, Point.Empty);
      }

      return result;
    }

    public static Bitmap Crop(this Bitmap @this, Rectangle rect, PixelFormat format = PixelFormat.DontCare) {
      rect = Rectangle.FromLTRB(rect.Left, rect.Top, Math.Min(rect.Right, @this.Width), Math.Min(rect.Bottom, @this.Height));

      var result = new Bitmap(rect.Width, rect.Height, format == PixelFormat.DontCare ? @this.PixelFormat : format);
      using (var g = Graphics.FromImage(result)) {
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighSpeed;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(@this, new Rectangle(Point.Empty, new Size(rect.Width, rect.Height)), rect, GraphicsUnit.Pixel);
      }

      return result;
    }

  }
}