
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
using System.Runtime.InteropServices;

namespace System.Drawing {
  internal static partial class BitmapExtensions {


#if UNSAFE
    // ReSharper disable once PartialTypeWithSinglePart
    private static partial class NativeMethods {

      [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
      private static extern unsafe byte* memcpy(byte* dst, byte* src, int count);

      public static unsafe void MemoryCopy(IntPtr source, IntPtr target, int count)
        => memcpy((byte*) target, (byte*) source, count)
      ;

    }
#endif

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
      if (@this == null)
        throw new ArgumentNullException(nameof(@this));
      
      if (@this.PixelFormat == format)
        return (Bitmap) @this.Clone();

      var result = new Bitmap(@this.Width, @this.Height, format);
      var sourceFormat = @this.PixelFormat;
#if UNSAFE
      if (sourceFormat == PixelFormat.Format24bppRgb && format == PixelFormat.Format32bppArgb) {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using (var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat))
        using (var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format))
          unsafe {
            var source = (byte*) sourceData.BitmapData.Scan0;
            var target = (byte*) targetData.BitmapData.Scan0;
            var sourceStride = sourceData.BitmapData.Stride;
            var targetStride = targetData.BitmapData.Stride;
            for (var y = @this.Height; y > 0; --y) {
              var sourceRow = source;
              var targetRow = target;
              for (var x = @this.Width; x > 0; --x) {

                var bg = *((short*) sourceRow);
                var r = sourceRow[2];

                *((short*) targetRow) = bg;
                targetRow[2] = r;
                targetRow[3] = 0xff;

                sourceRow += 3;
                targetRow += 4;
              }

              source += sourceStride;
              target += targetStride;
            }
          }

        return result;
      }

      if (sourceFormat == PixelFormat.Format32bppArgb && format == PixelFormat.Format24bppRgb) {
        var rect = new Rectangle(0, 0, @this.Width, @this.Height);
        using (var sourceData = Lock(@this, rect, ImageLockMode.ReadOnly, sourceFormat))
        using (var targetData = Lock(result, rect, ImageLockMode.WriteOnly, format))
          unsafe {
            var source = (byte*)sourceData.BitmapData.Scan0;
            var target = (byte*)targetData.BitmapData.Scan0;
            var sourceStride = sourceData.BitmapData.Stride;
            var targetStride = targetData.BitmapData.Stride;
            for (var y = @this.Height; y > 0; --y) {
              var sourceRow = source;
              var targetRow = target;
              for (var x = @this.Width; x > 0; --x) {
                var bg = *((short*)sourceRow);
                var r = sourceRow[2];

                *((short*)targetRow) = bg;
                targetRow[2] = r;

                sourceRow += 4;
                targetRow += 3;
              }

              source += sourceStride;
              target += targetStride;
            }
          }

        return result;
      }
#endif

      using (var g = Graphics.FromImage(result)) {
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.DrawImage(@this, Point.Empty);
      }

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