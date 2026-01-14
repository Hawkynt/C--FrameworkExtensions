#region (c)2010-2042 Hawkynt

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

#endregion

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Guard;

namespace System.Drawing;

public static partial class ImageExtensions {
  /// <param name="this">The image.</param>
  extension(Image @this)
  {
    /// <summary>
    ///   Gets a single page of a multipage image.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>The content of the requested page.</returns>
    public Image? GetPageAt(int page) {
      var totalPages = @this.GetPageCount();
      if (page >= totalPages)
        return null;

      if (page < 0)
        return null;

      @this.SelectActiveFrame(FrameDimension.Page, page);
      var result = new Bitmap(@this);
      return result;
    }

    /// <summary>
    ///   Gets the number of pages of the given image.
    /// </summary>
    /// <returns>Number of pages.</returns>
    public int GetPageCount() => @this.GetFrameCount(FrameDimension.Page);

    /// <summary>
    ///   Saves an image into a png file.
    /// </summary>
    /// <param name="file">The file where it should be saved.</param>
    public void SaveToPng(FileInfo file) => SaveToPng(@this, file.FullName);

    /// <summary>
    ///   Saves an image into a png file.
    /// </summary>
    /// <param name="fileName">The file where it should be saved.</param>
    public void SaveToPng(string fileName) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNullOrEmpty(fileName);

      _ = _GetEncoder(ImageFormat.Png) ?? throw new NotSupportedException("Png encoder not available");
      @this.Save(fileName, ImageFormat.Png);
    }

    /// <summary>
    ///   Saves an image into a tif file.
    /// </summary>
    /// <param name="fileName">The file where it should be saved.</param>
    public void SaveToTiff(string fileName) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNullOrEmpty(fileName);

      _ = _GetEncoder(ImageFormat.Tiff) ?? throw new NotSupportedException("Tiff encoder not available");
      @this.Save(fileName, ImageFormat.Tiff);
    }

    /// <summary>
    ///   Saves an image into a jpeg file.
    /// </summary>
    /// <param name="fileName">The file where it should be saved.</param>
    /// <param name="quality">The compression quality between 0(worst) and 1(best, default).</param>
    public void SaveToJpeg(string fileName, double quality = 1) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNullOrEmpty(fileName);

      var encoder = _GetEncoder(ImageFormat.Jpeg) ?? throw new NotSupportedException("Jpeg encoder not available");

      using var encoderParameters = new EncoderParameters(1);
      encoderParameters.Param[0] = new(Encoder.Quality, (long)(Math.Min(Math.Max(quality, 0), 1) * 100));
      @this.Save(fileName, encoder, encoderParameters);
    }

    /// <summary>
    ///   Saves an image into a jpeg stream.
    /// </summary>
    /// <param name="stream">The stream where it should be saved.</param>
    /// <param name="quality">The compression quality between 0(worst) and 1(best, default).</param>
    public void SaveToJpeg(Stream stream, double quality = 1) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNull(stream);

      var encoder = _GetEncoder(ImageFormat.Jpeg) ?? throw new NotSupportedException("Jpeg encoder not available");

      using var encoderParameters = new EncoderParameters(1);
      encoderParameters.Param[0] = new(Encoder.Quality, (long)(Math.Min(Math.Max(quality, 0), 1) * 100));
      @this.Save(stream, encoder, encoderParameters);
    }
  }

  /// <summary>
  ///   Get a suitable encoder.
  /// </summary>
  /// <param name="format">The format to encode.</param>
  /// <returns>A suitable encoder or <c>null</c>.</returns>
  private static ImageCodecInfo? _GetEncoder(ImageFormat format) {
    Against.ArgumentIsNull(format);

    return (from i in ImageCodecInfo.GetImageEncoders()
      where i.FormatID == format.Guid
      select i).FirstOrDefault();
  }

  /// <summary>
  ///   All sizes that are supported for icons.
  /// </summary>
  private static readonly int[] _SUPPORTED_ICON_RESOLUTIONS = [16, 24, 32, 48, 64, 128];

  /// <param name="this">This Image.</param>
  extension(Image @this)
  {
    /// <summary>
    ///   Converts a given image to an icon.
    /// </summary>
    /// <param name="targetRes">The target resolution, values &lt;=0 mean auto-detect.</param>
    /// <returns>
    ///   The icon.
    /// </returns>
    public Icon ToIcon(int targetRes = 0) {
      Against.ThisIsNull(@this);

      if (targetRes <= 0) {
        // auto-detect
        var width = @this.Width;
        var height = @this.Height;

        // if width/height different or not in the supported dimensions list
        if (width != height || !_SUPPORTED_ICON_RESOLUTIONS.Contains(targetRes)) {
          var longestEdge = Math.Max(width, height);
          var nextBiggerIconSize = _SUPPORTED_ICON_RESOLUTIONS.FirstOrDefault(r => r > longestEdge);
          var nextLessIconSize = Enumerable.Reverse(_SUPPORTED_ICON_RESOLUTIONS).FirstOrDefault(r => r < longestEdge);

          if (nextBiggerIconSize == 0)

            // when source image is bigger than the max support, use max support
            targetRes = nextLessIconSize;
          else {
            if (nextLessIconSize == 0)

              // if source image is smaller than min support, use min support
              targetRes = nextBiggerIconSize;
            else
              // if edge length is closer to the next smaller size, take it, otherwise use the next bigger size
              targetRes = longestEdge - nextLessIconSize < nextBiggerIconSize - longestEdge
                ? nextLessIconSize
                : nextBiggerIconSize;
          }
        }
      }

      using var bitmap = Resize(@this, targetRes, targetRes, true);

      // create gdi handle
      var hIcon = bitmap.GetHicon();

      // return icon
      return Icon.FromHandle(hIcon);
    }

    /// <summary>
    ///   Converts image to grayscale.
    /// </summary>
    /// <returns></returns>
    public Bitmap MakeGrayscale() {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNotOfType<Bitmap>(@this);

      var result = new Bitmap(@this.Width, @this.Height);
      using var graphics = Graphics.FromImage(result);
      var matrix = new ColorMatrix([
        [0.3f, 0.3f, 0.3f, 0, 0], 
        [0.59f, 0.59f, 0.59f, 0, 0],
        [0.11f, 0.11f, 0.11f, 0, 0],
        [0, 0, 0, 1f, 0],
        [0, 0, 0, 0, 1f]
      ]);
      var attributes = new ImageAttributes();
      attributes.SetColorMatrix(matrix);
      graphics.DrawImage(@this, new(Point.Empty, @this.Size), 0, 0, @this.Width, @this.Height, GraphicsUnit.Pixel, attributes);

      return result;
    }

    /// <summary>
    ///   Converts image to black&amp;white.
    /// </summary>
    /// <param name="threshold">The threshold under which a pixel is considered black.</param>
    /// <returns></returns>
    public Bitmap Threshold(byte threshold = 127) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNotOfType<Bitmap>(@this);

      return @this.ApplyPixelProcessor(
        c => {
          var mean = (c.R + c.G + c.B) / 3.0;
          var color = mean < threshold ? byte.MinValue : byte.MaxValue;
          return Color.FromArgb(255, color, color, color);
        }
      );
    }

    /// <summary>
    ///   Applies a pixel processor to the image.
    /// </summary>
    /// <param name="processor">The processor.</param>
    /// <returns>The processed image.</returns>
    public Bitmap ApplyPixelProcessor(Func<Color, Color> processor) {
      Against.ThisIsNull(@this);
      Against.ArgumentIsNotOfType<Bitmap>(@this);
      Against.ArgumentIsNull(processor);

      var original = (Bitmap)@this;
      //make an empty bitmap the same size as original
      var newBitmap = new Bitmap(original.Width, original.Height);

      for (var i = 0; i < original.Width; i++)
      for (var j = 0; j < original.Height; j++) {
        //get the pixel from the original image
        var originalColor = original.GetPixel(i, j);

        //create the color object
        var newColor = processor(originalColor);

        //set the new image's pixel to the grayscale version
        newBitmap.SetPixel(i, j, newColor);
      }

      return newBitmap;
    }

    /// <summary>
    ///   Mirrors the image along X.
    /// </summary>
    /// <returns>A mirrored image version</returns>
    public Image MirrorAlongX() {
      Against.ThisIsNull(@this);

      var result = (Image)@this.Clone();
      result.RotateFlip(RotateFlipType.RotateNoneFlipX);
      return result;
    }

    /// <summary>
    ///   Mirrors the image along Y.
    /// </summary>
    /// <returns>A mirrored image version</returns>
    public Image MirrorAlongY() {
      Against.ThisIsNull(@this);

      var result = (Image)@this.Clone();
      result.RotateFlip(RotateFlipType.RotateNoneFlipY);
      return result;
    }

    /// <summary>
    ///   Resizes the specified Image.
    /// </summary>
    /// <param name="longSide">The width/height in pixels.</param>
    /// <returns></returns>
    public Bitmap Resize(int longSide) => Resize(@this, longSide, longSide, true, null);

    /// <summary>
    ///   Resizes the specified Image.
    /// </summary>
    /// <param name="longSide">The width/height in pixels.</param>
    /// <param name="fillColor">Color of the fill.</param>
    /// <returns></returns>
    public Bitmap Resize(int longSide, Color fillColor) => Resize(@this, longSide, longSide, true, fillColor);

    /// <summary>
    ///   Resizes the specified Image.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="keepAspect">if set to <c>true</c> keeps image aspect and fills borders with fillColor.</param>
    /// <param name="fillColor">Color of the fill.</param>
    /// <returns></returns>
    public Bitmap Resize(int width, int height, bool keepAspect = true, Color? fillColor = null) {
      Against.ThisIsNull(@this);

      var result = new Bitmap(width, height);
      using var graphics = Graphics.FromImage(result);
      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
      graphics.SmoothingMode = SmoothingMode.HighQuality;
      graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
      graphics.CompositingQuality = CompositingQuality.HighQuality;

      if (keepAspect) {
        var ratioX = (double)result.Width / @this.Width;
        var ratioY = (double)result.Height / @this.Height;
        var ratio = ratioX < ratioY ? ratioX : ratioY;
        var newWidth = (int)(ratio * @this.Width);
        var newHeight = (int)(ratio * @this.Height);
        var x = (result.Width - newWidth) >> 1;
        var y = (result.Height - newHeight) >> 1;
        graphics.Clear(fillColor ?? Color.Transparent);
        graphics.DrawImage(@this, x, y, newWidth, newHeight);
      } else
        graphics.DrawImage(@this, 0, 0, result.Width, result.Height);

      return result;
    }

    /// <summary>
    ///   Resizes the specified Image.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="interpolation">The interpolation.</param>
    /// <returns></returns>
    public Bitmap Resize(int width = -1, int height = -1, InterpolationMode interpolation = InterpolationMode.Default) {
      Against.ThisIsNull(@this);

      width = width switch {
        < 1 when height < 1 => throw new ArgumentException("At least one argument has to be > 0", nameof(width)),
        // aspect ratio-preserving resize
        < 1 => (int)(@this.Width * height / (double)@this.Height),
        _ => width
      };

      if (height < 1)
        height = (int)(@this.Height * width / (double)@this.Width);

      // set resolution
      var result = new Bitmap(width, height, @this.PixelFormat);
      result.SetResolution(@this.HorizontalResolution, @this.VerticalResolution);

      // do resize action
      using var graphics = Graphics.FromImage(result);
      graphics.InterpolationMode = interpolation;
      graphics.DrawImage(@this, new Rectangle(0, 0, width, height), new(0, 0, @this.Width, @this.Height), GraphicsUnit.Pixel);

      return result;
    }

    /// <summary>
    ///   Rotates the specified image.
    /// </summary>
    /// <param name="angle">The angle.</param>
    /// <returns></returns>
    public Bitmap Rotate(float angle) {
      Against.ThisIsNull(@this);

      var result = new Bitmap(@this);

      while (angle < 0)
        angle += 360;

      if (angle >= 360)
        angle %= 360;

      switch (angle) {
        case 0: return result;
        case 90:
          result.RotateFlip(RotateFlipType.Rotate90FlipNone);
          return result;
        case 180:
          result.RotateFlip(RotateFlipType.Rotate180FlipNone);
          return result;
        case 270:
          result.RotateFlip(RotateFlipType.Rotate270FlipNone);
          return result;
        default: {
          using var graphics = Graphics.FromImage(result);
          graphics.TranslateTransform(result.Width / 2f, result.Height / 2f);
          graphics.RotateTransform(angle);
          graphics.TranslateTransform(-result.Width / 2f, -result.Height / 2f);
          graphics.DrawImage(result, new Point(0, 0));

          return result;
        }
      }
    }

    /// <summary>
    ///   Gets a rectangular area of the image.
    /// </summary>
    /// <param name="rect">The rectangle.</param>
    /// <returns>A new image from the given rectangle</returns>
    public Image GetRectangle(Rectangle rect) {
      Against.ThisIsNull(@this);

      var result = new Bitmap(rect.Width, rect.Height);
      using var graphics = Graphics.FromImage(result);
      graphics.DrawImage(@this, new Rectangle(Point.Empty, rect.Size), rect, GraphicsUnit.Pixel);

      return result;
    }

    /// <summary>
    ///   Replaces the given color with transparency.
    /// </summary>
    /// <param name="color">The color to replace.</param>
    /// <returns>A new image</returns>
    public Image ReplaceColorWithTransparency(Color color) {
      Against.ThisIsNull(@this);

      var result = new Bitmap(@this.Width, @this.Height);
      using var graphics = Graphics.FromImage(result);
      var imageAttributes = new ImageAttributes();
      imageAttributes.SetRemapTable([new() { OldColor = color, NewColor = Color.Transparent }], ColorAdjustType.Bitmap);
      graphics.DrawImage(@this, new(Point.Empty, @this.Size), 0, 0, @this.Width, @this.Height, GraphicsUnit.Pixel, imageAttributes);

      return result;
    }

    /// <summary>
    ///   Saves an image into a BASE64-encoded data URI
    /// </summary>
    /// <returns></returns>
    public string ToBase64DataUri() {
      if (@this == null)
        return string.Empty;

      var rawFormat = @this.RawFormat;
      string mimeType;
      for (;;) {
        var rawGuid = rawFormat.Guid;
        if (rawGuid == ImageFormat.Bmp.Guid) {
          mimeType = "image/bmp";
          break;
        }

        if (rawGuid == ImageFormat.Jpeg.Guid) {
          mimeType = "image/jpeg";
          break;
        }

        if (rawGuid == ImageFormat.Png.Guid) {
          mimeType = "image/png";
          break;
        }

        if (rawGuid == ImageFormat.Tiff.Guid) {
          mimeType = "image/tiff";
          break;
        }

        if (rawGuid == ImageFormat.Gif.Guid) {
          mimeType = "image/gif";
          break;
        }

        if (rawGuid == ImageFormat.Icon.Guid) {
          mimeType = "image/x-icon";
          break;
        }

        if (rawGuid == ImageFormat.Wmf.Guid) {
          mimeType = "windows/metafile";
          break;
        }

        rawFormat = ImageFormat.Png;
        mimeType = "image/png";
        break;
      }

      using var memoryStream = new MemoryStream();
      @this.Save(memoryStream, rawFormat);
      return $"data:{mimeType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
    }
  }

  /// <summary>
  ///   Gets an image from a BASE64-encoded data URI
  /// </summary>
  /// <param name="this">This Uri</param>
  /// <returns></returns>
  public static Image? FromBase64DataUri(this string @this) {
    if (!@this.StartsWith("data:image/"))
      return null;

    var index = @this.IndexOf("base64,", StringComparison.Ordinal);
    if (index < 0)
      return null;

    var base64 = @this[(index + 7)..];

    var bytes = Convert.FromBase64String(base64);
    using var ms = new MemoryStream(bytes, 0, bytes.Length);
    return Image.FromStream(ms, true);
  }
}
