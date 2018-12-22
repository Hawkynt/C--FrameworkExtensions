
#region (c)2010-2020 Hawkynt
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

using System.Drawing.Imaging;

namespace System.Drawing {
  internal static partial class BitmapExtensions {

    public interface IBitmapLocker : IDisposable {
      BitmapData BitmapData { get; }
    }


    private class BitmapLocker : IBitmapLocker {
      public BitmapData BitmapData { get; }
      private readonly Bitmap _bitmap;
      private bool _isDisposed;

      public BitmapLocker(Bitmap bitmap, Rectangle rect, ImageLockMode flags, PixelFormat format) {
        this._bitmap = bitmap;
        this.BitmapData = bitmap.LockBits(rect, flags, format);
      }

      ~BitmapLocker() {
        this.Dispose();
      }

      public void Dispose() {
        if (this._isDisposed)
          return;

        this._isDisposed = true;

        if (this._bitmap != null && this.BitmapData != null)
          this._bitmap.UnlockBits(this.BitmapData);
      }
    }

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags, PixelFormat format)
      => new BitmapLocker(@this, rect, flags, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect)
      => Lock(@this, rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), flags, PixelFormat.Format32bppArgb)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, PixelFormat format)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), ImageLockMode.ReadWrite, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, ImageLockMode flags)
      => Lock(@this, rect, flags, PixelFormat.Format32bppArgb)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, Rectangle rect, PixelFormat format)
      => Lock(@this, rect, ImageLockMode.ReadWrite, format)
    ;

    public static IBitmapLocker Lock(this Bitmap @this, ImageLockMode flags, PixelFormat format)
      => Lock(@this, new Rectangle(Point.Empty, @this.Size), flags, format)
    ;

  }
}