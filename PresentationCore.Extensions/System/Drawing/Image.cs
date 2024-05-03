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

using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Guard;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.Drawing;
public static partial class ImageExtensions {
  
#if !NETCOREAPP && !NET5_0_OR_GREATER
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

#endif
}