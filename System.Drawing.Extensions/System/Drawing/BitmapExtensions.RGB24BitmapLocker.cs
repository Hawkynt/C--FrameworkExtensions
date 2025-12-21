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

namespace System.Drawing;

public static partial class BitmapExtensions {
  private sealed class RGB24BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(
      bitmap,
      rect,
      flags,
      format
    ) {
    protected override int _BytesPerPixel => 3;

    public override Color this[int x, int y] {
      get {
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
      }
      set {
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
      }
    }

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

  }
}
