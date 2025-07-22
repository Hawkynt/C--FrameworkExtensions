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
using System.Linq;

namespace System.Drawing;

public static partial class BitmapExtensions {
  private sealed class UnsupportedDrawingBitmapLocker : BitmapLockerBase {
    public UnsupportedDrawingBitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) : base(bitmap, rect, flags, format) => this._exception = new($"Wrong pixel format {format} (supported: {string.Join(",", _LOCKER_TYPES.Keys.Select(i => i.ToString()).ToArray())})");

    private readonly NotSupportedException _exception;

    public override Color this[int x, int y] {
      get => throw this._exception;
      set => throw this._exception;
    }
  }
}
