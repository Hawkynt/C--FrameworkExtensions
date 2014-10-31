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

using System.Diagnostics.Contracts;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace System.Drawing {
  internal static partial class ImageExtensions {

    /// <summary>
    /// Saves an image into a jpeg file.
    /// </summary>
    /// <param name="This">This image.</param>
    /// <param name="fileName">The file where it should be saved.</param>
    /// <param name="quality">The compression quality between 0(worst) and 1(best, default).</param>
    public static void SaveToJpeg(this Image This, string fileName, double quality = 1) {
      Contract.Requires(This != null);
      Contract.Requires(fileName != null);
      var encoder = GetEncoder(ImageFormat.Jpeg);
      if (encoder == null)
        throw new NotSupportedException("Jpeg encoder not available");
      using (var encoderParameters = new EncoderParameters(1)) {
        encoderParameters.Param[0] = new EncoderParameter(
          Encoder.Quality, (long)(Math.Min(Math.Max(quality, 0), 1) * 100));
        This.Save(fileName, encoder, encoderParameters);
      }
    }

    /// <summary>
    /// Saves an image into a jpeg stream.
    /// </summary>
    /// <param name="This">This image.</param>
    /// <param name="stream">The stream where it should be saved.</param>
    /// <param name="quality">The compression quality between 0(worst) and 1(best, default).</param>
    public static void SaveToJpeg(this Image This, Stream stream, double quality = 1) {
      Contract.Requires(This != null);
      Contract.Requires(stream != null);
      var encoder = GetEncoder(ImageFormat.Jpeg);
      if (encoder == null)
        throw new NotSupportedException("Jpeg encoder not available");
      using (var encoderParameters = new EncoderParameters(1)) {
        encoderParameters.Param[0] = new EncoderParameter(
          Encoder.Quality, (long)(Math.Min(Math.Max(quality, 0), 1) * 100));
        This.Save(stream, encoder, encoderParameters);
      }
    }

    /// <summary>
    /// Get a suitable encoder.
    /// </summary>
    /// <param name="format">The format to encode.</param>
    /// <returns>A suitable encoder or <c>null</c>.</returns>
    public static ImageCodecInfo GetEncoder(ImageFormat format) {
      Contract.Requires(format != null);
      return (from i in ImageCodecInfo.GetImageEncoders()
              where i.FormatID == format.Guid
              select i).FirstOrDefault();
    }

    /// <summary>
    /// All sizes that are supported for icons.
    /// </summary>
    private static readonly int[] _SUPPORTED_ICON_RESOLUTIONS = new[] { 16, 24, 32, 48, 64, 128 };
    /// <summary>
    /// Converts a given image to an icon.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="targetRes">The target resolution, values &lt;=0 mean auto-detect.</param>
    /// <returns>
    /// The icon.
    /// </returns>
    public static Icon ToIcon(this Image This, int targetRes = 0) {
      Contract.Requires(This != null);

      if (targetRes <= 0) {

        // auto-detect
        var width = This.Width;
        var height = This.Height;

        // if width/height different or not in the supported dimensions list
        if (width != height || !_SUPPORTED_ICON_RESOLUTIONS.Contains(targetRes)) {
          var longestEdge = Math.Max(width, height);
          var nextBiggerIconSize = _SUPPORTED_ICON_RESOLUTIONS.FirstOrDefault(r => r > longestEdge);
          var nextLessIconSize = _SUPPORTED_ICON_RESOLUTIONS.Reverse().FirstOrDefault(r => r < longestEdge);

          if (nextBiggerIconSize == 0)

            // when source image is bigger than the max support, use max support
            targetRes = nextLessIconSize;
          else {

            if (nextLessIconSize == 0)

              // if source image is smaller than min support, use min support
              targetRes = nextBiggerIconSize;
            else {

              // if edge length is closer to the next smaller size, take it, otherwise use the next bigger size
              if (longestEdge - nextLessIconSize < nextBiggerIconSize - longestEdge)
                targetRes = nextLessIconSize;
              else
                targetRes = nextBiggerIconSize;
            }
          }
        }
      }

      using (var bitmap = new Bitmap(targetRes, targetRes)) {

        // draw/resize image
        using (var graphics = Graphics.FromImage(bitmap)) {
          graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
          graphics.DrawImage(This, 0, 0, targetRes, targetRes);
        }

        // create gdi handle
        var hIcon = bitmap.GetHicon();

        // return icon
        return (Icon.FromHandle(hIcon));
      }
    }

    /// <summary>
    /// Converts a GDI+ image into a WPF BitmapImage.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>The BitmapImage</returns>
    public static BitmapImage ToBitmapImage(this Image image) {
      Contract.Requires(image != null);
      using (var memoryStream = new MemoryStream()) {
        image.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0;
        var result = new BitmapImage();
        result.BeginInit();
        result.CacheOption = BitmapCacheOption.OnLoad;
        result.UriSource = null;
        result.StreamSource = memoryStream;
        result.EndInit();
        return (result);
      }
    }

    /// <summary>
    /// Converts image to grayscale.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    public static Bitmap MakeGrayscale(this Image source) {
      Contract.Requires(source != null && source is Bitmap);
      return (source.ApplyPixelProcessor(c => {
        var grayScale = (int)((c.R * .3) + (c.G * .59) + (c.B * .11));
        return (Color.FromArgb(grayScale, grayScale, grayScale));
      }));
    }

    /// <summary>
    /// Converts image to black&amp;white.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="threshold">The threshold under which a pixel is considered black.</param>
    /// <returns></returns>
    public static Bitmap Threshold(this Image source, byte threshold = 127) {
      Contract.Requires(source != null && source is Bitmap);
      return (source.ApplyPixelProcessor(c => {
        var mean = (c.R + c.G + c.B) / 3.0;
        var color = mean < threshold ? byte.MinValue : byte.MaxValue;
        return (Color.FromArgb(color, color, color));
      }));
    }

    /// <summary>
    /// Applies a pixel processor to the image.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="processor">The processor.</param>
    /// <returns>The processed image.</returns>
    public static Bitmap ApplyPixelProcessor(this Image source, Func<Color, Color> processor) {
      Contract.Requires(source != null && source is Bitmap);
      Contract.Requires(processor != null);
      var original = (Bitmap)source;
      //make an empty bitmap the same size as original
      var newBitmap = new Bitmap(original.Width, original.Height);

      for (var i = 0; i < original.Width; i++) {
        for (var j = 0; j < original.Height; j++) {
          //get the pixel from the original image
          var originalColor = original.GetPixel(i, j);

          //create the color object
          var newColor = processor(originalColor);

          //set the new image's pixel to the grayscale version
          newBitmap.SetPixel(i, j, newColor);
        }
      }

      return newBitmap;
    }

    /// <summary>
    /// Mirrors the image along X.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <returns>A mirrored image version</returns>
    public static Image MirrorAlongX(this Image This) {
      var result = (Image)This.Clone();
      result.RotateFlip(RotateFlipType.RotateNoneFlipX);
      return (result);
    }

    /// <summary>
    /// Mirrors the image along Y.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <returns>A mirrored image version</returns>
    public static Image MirrorAlongY(this Image This) {
      var result = (Image)This.Clone();
      result.RotateFlip(RotateFlipType.RotateNoneFlipY);
      return (result);
    }

    /// <summary>
    /// Resizes the specified Image.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="interpolation">The interpolation.</param>
    /// <returns></returns>
    public static Bitmap Resize(this Image This, int width = -1, int height = -1, InterpolationMode interpolation = InterpolationMode.Default) {
      Contract.Requires(This != null);
      if (width < 1 && height < 1)
        throw new ArgumentException("At least one argument has to be > 0", "width");

      // aspect ratio-preserving resize
      if (width < 1)
        width = (int)(This.Width * height / (double)This.Height);
      if (height < 1)
        height = (int)(This.Height * width / (double)This.Width);

      // set resolution
      var result = new Bitmap(width, height, This.PixelFormat);
      result.SetResolution(This.HorizontalResolution, This.VerticalResolution);

      // do resize action
      using (var graphics = Graphics.FromImage(result)) {
        graphics.InterpolationMode = interpolation;
        graphics.DrawImage(This, new Rectangle(0, 0, width, height), new Rectangle(0, 0, This.Width, This.Height), GraphicsUnit.Pixel);
      }
      return (result);
    }

    /// <summary>
    /// Rotates the specified image.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="angle">The angle.</param>
    /// <returns></returns>
    public static Bitmap Rotate(this Image This, float angle) {
      Contract.Requires(This != null);
      var result = new Bitmap(This);
      if (Math.Abs(angle) % 360 == 0)
        return (result);
      if (angle == 90 || angle == -270) {
        result.RotateFlip(RotateFlipType.Rotate90FlipNone);
        return (result);
      }
      if (Math.Abs(angle) % 180 == 0) {
        result.RotateFlip(RotateFlipType.Rotate180FlipNone);
        return (result);
      }
      if (angle == 270 || angle == -90) {
        result.RotateFlip(RotateFlipType.Rotate270FlipNone);
        return (result);
      }

      using (var graphics = Graphics.FromImage(result)) {
        graphics.TranslateTransform(result.Width / 2f, result.Height / 2f);
        graphics.RotateTransform(angle);
        graphics.TranslateTransform(-result.Width / 2f, -result.Height / 2f);
        graphics.DrawImage(result, new Point(0, 0));
      }
      return (result);
    }
  }
}