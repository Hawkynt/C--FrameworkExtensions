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
using System.Drawing.Printing;
using System.Windows.Forms;
using Guard;

namespace System.Drawing;

public static partial class ImageExtensions {
  /// <summary>
  ///   Gets a single page of a multipage image.
  /// </summary>
  /// <param name="image">The image.</param>
  /// <param name="page">The page.</param>
  /// <returns>The content of the requested page.</returns>
  private static Image _GetPageAt(Image image, int page) {
    var totalPages = _GetPageCount(image);
    if (page >= totalPages)
      return null;

    if (page < 0)
      return null;

    image.SelectActiveFrame(FrameDimension.Page, page);
    var result = new Bitmap(image);
    return result;
  }

  /// <summary>
  ///   Gets the number of pages of the given image.
  /// </summary>
  /// <param name="image">The image.</param>
  /// <returns>Number of pages.</returns>
  private static int _GetPageCount(Image image) => image.GetFrameCount(FrameDimension.Page);

  /// <summary>
  ///   Rotates the specified image.
  /// </summary>
  /// <param name="image">This Image.</param>
  /// <returns></returns>
  private static Bitmap _Rotate90(Image image) {
    var result = new Bitmap(image);
    result.RotateFlip(RotateFlipType.Rotate90FlipNone);
    return result;
  }

  /// <summary>
  ///   Prints the image using the system's printer dialog.
  /// </summary>
  /// <param name="this">This Image.</param>
  /// <param name="documentName">Name of the document.</param>
  /// <param name="dialog">The dialog to use; creates its own when none is given.</param>
  /// <returns>The used printersettings</returns>
  public static PrinterSettings PrintImageWithDialog(this Image @this, string documentName = null, PrintDialog dialog = null) {
    Against.ThisIsNull(@this);

    using var document = new PrintDocument();
    var pageCount = _GetPageCount(@this);

    var noDialog = dialog == null;
    try {
      if (noDialog)
        dialog = new();

      if (documentName != null)
        document.DocumentName = documentName;

      var currentPageIndex = 0;
      document.PrintPage += (_, ea) => {
        using (var currentPage = _GetPageAt(@this, currentPageIndex)) {
          var imageIsLandscape = currentPage.Width > currentPage.Height;
          var marginBounds = ea.PageBounds;
          var paperIsLandscape = marginBounds.Width > marginBounds.Height;
          Image printImage = null;
          try {
            printImage = imageIsLandscape == paperIsLandscape ? currentPage : _Rotate90(currentPage);

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
        return null;

      document.Print();
      return document.PrinterSettings;
    } finally {
      if (noDialog)
        dialog?.Dispose();
    }
  }

  /// <summary>
  ///   Prints the image without a printer dialog.
  /// </summary>
  /// <param name="this">This Image.</param>
  /// <param name="documentName">Name of the document.</param>
  /// <param name="settings">The settings.</param>
  public static void PrintImage(this Image @this, string documentName = null, PrinterSettings settings = null) {
    Against.ThisIsNull(@this);

    using var document = new PrintDocument();
    var pageCount = _GetPageCount(@this);

    if (documentName != null)
      document.DocumentName = documentName;

    var currentPageIndex = 0;
    document.PrintPage += (_, ea) => {
      using (var currentPage = _GetPageAt(@this, currentPageIndex)) {
        var imageIsLandscape = currentPage.Width > currentPage.Height;
        var marginBounds = ea.PageBounds;
        var paperIsLandscape = marginBounds.Width > marginBounds.Height;
        Image printImage = null;
        try {
          printImage = imageIsLandscape == paperIsLandscape ? currentPage : _Rotate90(currentPage);

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
