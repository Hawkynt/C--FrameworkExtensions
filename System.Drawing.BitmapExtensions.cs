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
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global


namespace System.Drawing {
  // ReSharper disable once PartialTypeWithSinglePart
  internal static partial class BitmapExtensions {

    // ReSharper disable once PartialTypeWithSinglePart
    private static partial class NativeMethods {

#if UNSAFE

      [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
      private static extern unsafe byte* _MemoryCopy(byte* dst, byte* src, int count);

      [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
      private static extern unsafe byte* _MemorySet(byte* dst, int value, int count);

      public static unsafe void MemoryCopy(byte* source, byte* target, int count)
        => _MemoryCopy(target, source, count)
      ;

      public static unsafe void MemorySet(byte* source, byte value, int count)
        => _MemorySet(source, value, count)
      ;


#endif

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
      void DrawRectangle(Rectangle rect, Color color);
      void DrawHorizontalLine(int x, int y, int count, Color color);
      void DrawHorizontalLine(Point p, int count, Color color);
      void DrawVerticalLine(int x, int y, int count, Color color);
      void DrawVerticalLine(Point p, int count, Color color);
      void FillRectangle(Rectangle rect, Color color);
      void DrawLine(int x0, int y0, int x1, int y1, Color color);
      void DrawLine(Point a,Point b, Color color);
      void CopyFrom(IBitmapLocker other, int xs, int ys, int xt, int yt, int width, int height);
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
          // copy faster if both have the same pixel format
          if (other is PixelProcessorBase ppb) {
            var bitmapDataTarget = this._bitmapData;
            var pixelFormat = bitmapDataTarget.PixelFormat;
            var bitmapDataSource = ppb._bitmapData;
            if (pixelFormat == bitmapDataSource.PixelFormat) {
              var bytesPerPixel = this._bytesPerPixel;
              if (bytesPerPixel > 0) {
                var yOffsetTarget = bitmapDataTarget.Scan0+ (bitmapDataTarget.Stride * yt + bytesPerPixel * xt);
                var yOffsetSource = bitmapDataSource.Scan0+ (bitmapDataSource.Stride * ys + bytesPerPixel * xs);
                var byteCountPerLine = width * bytesPerPixel;
                while (height > 0) {
                  NativeMethods.MemoryCopy(yOffsetSource, yOffsetTarget, byteCountPerLine);
                  yOffsetSource += bitmapDataSource.Stride;
                  yOffsetTarget += bitmapDataTarget.Stride;
                  --height;
                }

                return;
              }
            }
          }

          for (var y = 0; y < height; ++ys, ++yt, ++y)
          for (int x = 0, xcs = xs, xct = xt; x < width; ++xcs, ++xct, ++x)
            this[xct, yt] = other[xcs, ys];
        }
      }

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
            NativeMethods.MemoryCopy(pointer, nextPointer, stride);
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
          while (--height > 0) {
            NativeMethods.MemoryCopy(pointer, nextPointer, stride);
            nextPointer += stride;
          }
        }

#endif

      }

      private static readonly Dictionary<PixelFormat, Func<BitmapData, IPixelProcessor>> _SUPPORTED_PIXEL_PROCESSORS = new Dictionary<PixelFormat, Func<BitmapData, IPixelProcessor>> {
        {PixelFormat.Format24bppRgb,l=>new RGB24(l)},
        {PixelFormat.Format32bppRgb,l=>new RGB32(l)},
        {PixelFormat.Format32bppArgb,l=>new ARGB32(l)},
      };

      private readonly Bitmap _bitmap;
      private bool _isDisposed;
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

      ~BitmapLocker() => this.Dispose();

      public void Dispose() {
        if (this._isDisposed)
          return;

        this._isDisposed = true;

        if (this._bitmap != null && this.BitmapData != null)
          this._bitmap.UnlockBits(this.BitmapData);
      }

      public Color this[int x, int y] {
        get => this._pixelProcessor[x, y];
        set => this._pixelProcessor[x, y] = value;
      }

      public Color this[Point p]
      {
        get => this[p.X,p.Y];
        set => this[p.X,p.Y] = value;
      }

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

      public void DrawLine(Point a, Point b, Color color) => this.DrawLine(a.X, a.Y, b.X, b.Y, color);

      public void CopyFrom(IBitmapLocker other, int xs, int ys, int xt, int yt, int width, int height) {
        switch (other) {
          case null:
            throw new ArgumentNullException(nameof(other));
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

    }

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