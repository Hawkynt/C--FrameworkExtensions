#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion


#if !NETCOREAPP && !NET5_0_OR_GREATER

using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Guard;

namespace System.Drawing;

public static partial class ImageExtensions {
  
  /// <summary>
  /// Converts a GDI+ image into a WPF BitmapImage.
  /// </summary>
  /// <param name="this">The image.</param>
  /// <returns>The BitmapImage</returns>
  public static BitmapImage ToBitmapImage(this Image @this) {
    Against.ThisIsNull(@this);

    using var memoryStream = new MemoryStream();
    @this.Save(memoryStream, ImageFormat.Png);
    memoryStream.Position = 0;
    
    var result = new BitmapImage();
    result.BeginInit();
    result.CacheOption = BitmapCacheOption.OnLoad;
    result.UriSource = null;
    result.StreamSource = memoryStream;
    result.EndInit();
    
    return result;
  }

}

#endif
