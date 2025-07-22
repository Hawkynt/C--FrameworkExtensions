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
  private sealed class Gray16BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(bitmap, rect, flags, format) {

    protected override int _BytesPerPixel => 2;

    public override Color this[int x, int y] {
      get {
#if UNSAFE
        unsafe {
          var data = this.BitmapData;
          var pointer = (ushort*)((byte*)data.Scan0 + y * data.Stride + (x << 1));
          var value = *pointer;
          var gray = (byte)(value >> 8); // upper byte is intensity
          return Color.FromArgb(gray, gray, gray);
        }
#else
      throw new PlatformNotSupportedException("Safe access for RGB16BitmapLocker not implemented.");
#endif
      }

      set {
#if UNSAFE
        unsafe {
          var data = this.BitmapData;
          var pointer = (ushort*)((byte*)data.Scan0 + y * data.Stride + (x << 1));
          var gray = (byte)(value.R * 0.3 + value.G * 0.59 + value.B * 0.11);
          (*pointer) = (ushort)((gray << 8) | gray);
        }
#else
      throw new PlatformNotSupportedException("Safe access for RGB16BitmapLocker not implemented.");
#endif
      }
    }

  }
}
