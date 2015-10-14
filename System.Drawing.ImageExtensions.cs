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

#if NETFX_4
using System.Diagnostics.Contracts;
#endif
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.Drawing {
  internal static partial class ImageExtensions {
    /// <summary>
    /// Gets a single page of a multipage image.
    /// </summary>
    /// <param name="This">The image.</param>
    /// <param name="page">The page.</param>
    /// <returns>The content of the requested page.</returns>
    public static Image GetPageAt(this Image This, int page) {
      var totalPages = This.GetPageCount();
      if (page >= totalPages)
        return (null);

      if (page < 0)
        return (null);

      This.SelectActiveFrame(FrameDimension.Page, page);
      var result = new Bitmap(This);
      return (result);
    }

    /// <summary>
    /// Gets the number of pages of the given image.
    /// </summary>
    /// <param name="This">The image.</param>
    /// <returns>Number of pages.</returns>
    public static int GetPageCount(this Image This) => This.GetFrameCount(FrameDimension.Page);

    /// <summary>
    /// Saves an image into a png file.
    /// </summary>
    /// <param name="This">This image.</param>
    /// <param name="fileName">The file where it should be saved.</param>
    public static void SaveToPng(this Image This, string fileName) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(fileName != null);
#endif
      var encoder = GetEncoder(ImageFormat.Png);
      if (encoder == null)
        throw new NotSupportedException("Png encoder not available");

      This.Save(fileName, ImageFormat.Png);
    }


    /// <summary>
    /// Saves an image into a jpeg file.
    /// </summary>
    /// <param name="This">This image.</param>
    /// <param name="fileName">The file where it should be saved.</param>
    /// <param name="quality">The compression quality between 0(worst) and 1(best, default).</param>
    public static void SaveToJpeg(this Image This, string fileName, double quality = 1) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(fileName != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(stream != null);
#endif
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
#if NETFX_4
      Contract.Requires(format != null);
#endif
      return (from i in ImageCodecInfo.GetImageEncoders()
              where i.FormatID == format.Guid
              select i).FirstOrDefault();
    }

    /// <summary>
    /// All sizes that are supported for icons.
    /// </summary>
    private static readonly int[] _SUPPORTED_ICON_RESOLUTIONS = { 16, 24, 32, 48, 64, 128 };
    /// <summary>
    /// Converts a given image to an icon.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="targetRes">The target resolution, values &lt;=0 mean auto-detect.</param>
    /// <returns>
    /// The icon.
    /// </returns>
    public static Icon ToIcon(this Image This, int targetRes = 0) {
#if NETFX_4
      Contract.Requires(This != null);
#endif

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
              targetRes = longestEdge - nextLessIconSize < nextBiggerIconSize - longestEdge ? nextLessIconSize : nextBiggerIconSize;
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
#if NETFX_4
      Contract.Requires(image != null);
#endif
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
#if NETFX_4
      Contract.Requires(source is Bitmap);
#endif
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
#if NETFX_4
      Contract.Requires(source is Bitmap);
#endif
      return (source.ApplyPixelProcessor(c => {
        var mean = (c.R + c.G + c.B) / 3.0;
        var color = mean < threshold ? byte.MinValue : byte.MaxValue;
        return (Color.FromArgb(c.A, color, color, color));
      }));
    }

    /// <summary>
    /// Applies a pixel processor to the image.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="processor">The processor.</param>
    /// <returns>The processed image.</returns>
    public static Bitmap ApplyPixelProcessor(this Image source, Func<Color, Color> processor) {
#if NETFX_4
      Contract.Requires(source is Bitmap);
      Contract.Requires(processor != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
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
#if NETFX_4
      Contract.Requires(This != null);
#endif
      if (width < 1 && height < 1)
        throw new ArgumentException("At least one argument has to be > 0", nameof(width));

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
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var result = new Bitmap(This);

      while (angle < 0)
        angle += 360;

      if (angle >= 360)
        angle = angle % 360;

      if (angle == 0)
        return (result);

      if (angle == 90) {
        result.RotateFlip(RotateFlipType.Rotate90FlipNone);
        return (result);
      }

      if (angle == 180) {
        result.RotateFlip(RotateFlipType.Rotate180FlipNone);
        return (result);
      }

      if (angle == 270) {
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

    /// <summary>
    /// Prints the image using the system's printer dialog.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="documentName">Name of the document.</param>
    /// <param name="dialog">The dialog to use; creates its own when none is given.</param>
    /// <returns>The used printersettings</returns>
    public static PrinterSettings PrintImageWithDialog(this Image This, string documentName = null, PrintDialog dialog = null) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      using (var document = new PrintDocument()) {
        var pageCount = This.GetPageCount();

        var noDialog = dialog == null;
        try {

          if (noDialog)
            dialog = new PrintDialog();

          if (documentName != null)
            document.DocumentName = documentName;

          var currentPageIndex = 0;
          document.PrintPage += (o, ea) => {

            using (var currentPage = This.GetPageAt(currentPageIndex)) {
              var imageIsLandscape = currentPage.Width > currentPage.Height;
              var marginBounds = ea.PageBounds;
              var paperIsLandscape = marginBounds.Width > marginBounds.Height;
              Image printImage = null;
              try {
                printImage = imageIsLandscape == paperIsLandscape ? currentPage : currentPage.Rotate(90);

                var ratio = printImage.Width / (double)printImage.Height;
                if (marginBounds.Width / ratio > marginBounds.Height)
                  marginBounds.Width = (int)(marginBounds.Height * ratio);
                else
                  marginBounds.Height = (int)(marginBounds.Width / ratio);

                ea.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                ea.Graphics.DrawImage(printImage, marginBounds);

              } finally {
                if (imageIsLandscape != paperIsLandscape)
                  printImage?.Dispose();
              }
            }
            ea.HasMorePages = ++currentPageIndex < pageCount;
          };

          dialog.PrinterSettings.MinimumPage = 1;
          dialog.PrinterSettings.MaximumPage = pageCount;
          dialog.Document = document;

          if (dialog.ShowDialog() != DialogResult.OK)
            return (null);

          document.Print();
          return (document.PrinterSettings);

        } finally {
          if (noDialog)
            dialog?.Dispose();
        }
      }

    }

    /// <summary>
    /// Prints the image without a printer dialog.
    /// </summary>
    /// <param name="This">This Image.</param>
    /// <param name="documentName">Name of the document.</param>
    /// <param name="settings">The settings.</param>
    public static void PrintImage(this Image This, string documentName = null, PrinterSettings settings = null) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      using (var document = new PrintDocument()) {
        var pageCount = This.GetPageCount();

        if (documentName != null)
          document.DocumentName = documentName;

        var currentPageIndex = 0;
        document.PrintPage += (o, ea) => {

          using (var currentPage = This.GetPageAt(currentPageIndex)) {
            var imageIsLandscape = currentPage.Width > currentPage.Height;
            var marginBounds = ea.PageBounds;
            var paperIsLandscape = marginBounds.Width > marginBounds.Height;
            Image printImage = null;
            try {
              printImage = imageIsLandscape == paperIsLandscape ? currentPage : currentPage.Rotate(90);

              var ratio = printImage.Width / (double)printImage.Height;
              if (marginBounds.Width / ratio > marginBounds.Height)
                marginBounds.Width = (int)(marginBounds.Height * ratio);
              else
                marginBounds.Height = (int)(marginBounds.Width / ratio);

              ea.Graphics.CompositingQuality = CompositingQuality.HighQuality;
              ea.Graphics.DrawImage(printImage, marginBounds);

            } finally {
              if (imageIsLandscape != paperIsLandscape)
                printImage?.Dispose();
            }
          }
          ea.HasMorePages = ++currentPageIndex < pageCount;
        };

        if (settings != null)
          document.PrinterSettings = settings;

        document.Print();
      }
    }

  }
}