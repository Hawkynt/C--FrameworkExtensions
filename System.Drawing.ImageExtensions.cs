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
  }
}