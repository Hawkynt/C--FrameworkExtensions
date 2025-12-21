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
using System.Drawing.ColorSpaces;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing;

public static partial class BitmapExtensions {
  private sealed class ARGB32BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(
      bitmap,
      rect,
      flags,
      format
    ) {
    protected override int _BytesPerPixel => 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Rgba32 GetPixelRgba32(int x, int y) {
      unsafe {
        var data = this.BitmapData;
        var pointer = (uint*)data.Scan0;
        Debug.Assert(pointer != null, nameof(pointer) + " != null");
        var stride = data.Stride >> 2;
        var offset = stride * y + x;
        return new(pointer[offset]);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetPixelRgba32(int x, int y, Rgba32 color) {
      unsafe {
        var data = this.BitmapData;
        var pointer = (uint*)data.Scan0;
        Debug.Assert(pointer != null, nameof(pointer) + " != null");
        var stride = data.Stride >> 2;
        var offset = stride * y + x;
        pointer[offset] = color.Packed;
      }
    }

    public override Color this[int x, int y] {
      get {
        unsafe {
          var data = this.BitmapData;
          var pointer = (int*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride >> 2;
          var offset = stride * y + x;
          return Color.FromArgb(pointer[offset]);
        }
      }
      set {
        unsafe {
          var data = this.BitmapData;
          var pointer = (int*)data.Scan0;
          Debug.Assert(pointer != null, nameof(pointer) + " != null");
          var stride = data.Stride >> 2;
          var offset = stride * y + x;
          pointer[offset] = value.ToArgb();
        }
      }
    }


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
            case < 0x01000000: continue;
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

  }
}
