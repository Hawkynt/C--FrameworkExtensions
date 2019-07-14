
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

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Drawing {
  internal static partial class BitmapExtensions {

    public interface IBitmapLocker : IDisposable {
      BitmapData BitmapData { get; }
      Color this[int x, int y] { get; set; }
      void DrawRectangle(Rectangle rect, Color c);
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

      public unsafe Color this[int x, int y] {
        get {
          var data = this.BitmapData;
          var pointer = data.Scan0;
          var stride = data.Stride;
          var offset = stride * y + x * 4;
          var r = ((byte*)pointer)[offset++];
          var g = ((byte*)pointer)[offset++];
          var b = ((byte*)pointer)[offset++];
          var a = ((byte*)pointer)[offset++];
          return Color.FromArgb(a, r, g, b);
        }
        set {
          var data = this.BitmapData;
          var pointer = data.Scan0;
          var stride = data.Stride;
          var offset = stride * y + x * 4;
          ((byte*)pointer)[offset++]=value.R;
          ((byte*)pointer)[offset++] = value.G;
          ((byte*)pointer)[offset++] = value.B;
          ((byte*)pointer)[offset++] = value.A;
        }
      }

      public void DrawRectangle(Rectangle rect, Color c) {
        for (var x = rect.Left; x <= rect.Right; ++x) {
          this[x, rect.Top] = c;
          this[x, rect.Bottom] = c;
        }

        for (var y = rect.Top+1; y <= rect.Bottom-1; ++y) {
          this[rect.Left,y] = c;
          this[rect.Right,y] = c;
        }
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

    public static Bitmap ConvertPixelFormat(this Bitmap @this, PixelFormat format) {
      var result = new Bitmap(@this.Width, @this.Height, format);
      using (var g = Graphics.FromImage(result))
        g.DrawImage(@this, Point.Empty);

      return result;
    }

    public static Bitmap Crop(this Bitmap @this, Rectangle rect, PixelFormat format = PixelFormat.DontCare) {
      rect=Rectangle.FromLTRB(rect.Left,rect.Top,Math.Min(rect.Right,@this.Width),Math.Min(rect.Bottom,@this.Height));
      
      var result = new Bitmap(rect.Width, rect.Height, format==PixelFormat.DontCare?@this.PixelFormat:format);
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