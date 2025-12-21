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
  private sealed class RGB32BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(bitmap, rect, flags, format) {
    protected override int _BytesPerPixel => 4;

    public override Color this[int x, int y] {
      get {
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
      }
      set {
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
      }
    }

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

  }
}
