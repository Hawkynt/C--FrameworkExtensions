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

using System.Drawing.Imaging;

namespace System.Drawing;

public static partial class BitmapExtensions {
  private sealed class RGB555BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(bitmap, rect, flags, format) {

    protected override int _BytesPerPixel => 2;

    public override Color this[int x, int y] {
      get {
        unsafe {
          var data = this.BitmapData;
          var pointer = (ushort*)((byte*)data.Scan0 + y * data.Stride + (x << 1));
          var value = *pointer;

          return Color.FromArgb(
            ((value >> 10) & 0x1F) << 3,
            ((value >> 5) & 0x1F) << 3,
            (value & 0x1F) << 3
          );
        }
      }

      set {
        unsafe {
          var data = this.BitmapData;
          var pointer = (ushort*)((byte*)data.Scan0 + y * data.Stride + (x << 1));
          *pointer = (ushort)(
            ((value.R >> 3) << 10) |
            ((value.G >> 3) << 5) |
            (value.B >> 3)
          );
        }
      }
    }

  }
}
