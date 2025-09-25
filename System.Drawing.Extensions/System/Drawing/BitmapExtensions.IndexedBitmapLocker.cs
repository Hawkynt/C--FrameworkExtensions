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
using Guard;

namespace System.Drawing;

public static partial class BitmapExtensions {
  private sealed class IndexedBitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(bitmap, rect, flags, format) {

    private readonly Color[] _palette = bitmap.Palette.Entries;
    private readonly PixelFormat _format = format;

    protected override int _BytesPerPixel => this._format switch {
      PixelFormat.Format8bppIndexed => 1,
      PixelFormat.Format4bppIndexed => 0, // packed 2 pixels per byte
      PixelFormat.Format1bppIndexed => 0, // packed 8 pixels per byte
      _ => throw new NotSupportedException($"Unsupported indexed format: {this._format}")
    };

    public override unsafe Color this[int x, int y] {
      get {

        var data = this.BitmapData;
        var stride = data.Stride;
        var ptr = (byte*)data.Scan0 + y * stride;
        if(this._format == PixelFormat.Format4bppIndexed) {
          var index = x >> 1;
          var highNibble = (x & 1) == 0;
          var val = highNibble ? ptr[index] >> 4 : ptr[index] & 0x0F;
          return this._palette[val];
        }

        if(this._format == PixelFormat.Format1bppIndexed) {
          var index = x >> 3;
          var bit = 7 - (x & 7);
          var val = (ptr[index] >> bit) & 1;
          return this._palette[val];
        }
        
        throw new NotSupportedException();
      }
      set {
        var colorIndex = Array.IndexOf(this._palette, value);
        Against.IndexBelowZero(colorIndex);

        var data = this.BitmapData;
        var stride = data.Stride;
        var ptr = (byte*)data.Scan0 + y * stride;
        
        switch (this._format) {
          case PixelFormat.Format4bppIndexed: {
            var index = x >> 1;
            ptr[index] = (x & 1) == 0 
              ? (byte)((ptr[index] & 0x0F) | (colorIndex << 4)) 
              : (byte)((ptr[index] & 0xF0) | (colorIndex & 0x0F))
              ;

            break;
          }

          case PixelFormat.Format1bppIndexed: {
            var index = x >> 3;
            var bit = 7 - (x & 7);
            if (colorIndex == 1)
              ptr[index] |= (byte) (1 << bit);
            else
              ptr[index] &= (byte) ~(1 << bit);
            break;
          }

          default:
            throw new NotSupportedException();
        }
      }
    }
  }
}
