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
  private sealed class Indexed8BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format)
    : BitmapLockerBase(bitmap, rect, flags, format) {

    private readonly Color[] _palette = bitmap.Palette.Entries;

    protected override int _BytesPerPixel => 1;

    public override unsafe Color this[int x, int y] {
      get {

        var data = this.BitmapData;
        var stride = data.Stride;
        var ptr = (byte*)data.Scan0 + y * stride;
        return this._palette[ptr[x]];
      }
      set {
        var colorIndex = Array.IndexOf(this._palette, value);
        Against.IndexBelowZero(colorIndex);

        var data = this.BitmapData;
        var stride = data.Stride;
        var ptr = (byte*)data.Scan0 + y * stride;
        
        ptr[x] = (byte) colorIndex;
      }
    }
  }
}
