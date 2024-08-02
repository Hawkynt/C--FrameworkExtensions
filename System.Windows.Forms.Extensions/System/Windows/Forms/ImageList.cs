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

using System.IO;
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class ImageListExtensions {

  /// <summary>
  /// Saves all images in the <see cref="System.Windows.Forms.ImageList"/> to the specified directory.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ImageList"/> instance.</param>
  /// <param name="directoryName">The name of the directory where the images will be saved.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ImageList imageList = new ImageList();
  /// imageList.Images.Add(Image.FromFile("path/to/image1.png"));
  /// imageList.Images.Add(Image.FromFile("path/to/image2.png"));
  /// 
  /// string directoryName = "C:\\Images";
  /// imageList.SaveToDirectory(directoryName);
  /// // All images in the imageList are now saved to the specified directory.
  /// </code>
  /// </example>
  public static void SaveToDirectory(this ImageList @this, string directoryName) {
    Against.ThisIsNull(@this);

    var images = @this.Images;
    foreach (var image in from i in Enumerable.Range(0, images.Count) select Tuple.Create(i, images[i], images.Keys[i]))
      image.Item2.Save(Path.Combine(directoryName, image.Item3 + ".png"));
  }

  /// <summary>
  /// Saves all images in the <see cref="System.Windows.Forms.ImageList"/> to the specified directory.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ImageList"/> instance.</param>
  /// <param name="directory">The directory where the images will be saved.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ImageList imageList = new ImageList();
  /// imageList.Images.Add(Image.FromFile("path/to/image1.png"));
  /// imageList.Images.Add(Image.FromFile("path/to/image2.png"));
  /// 
  /// DirectoryInfo directory = new DirectoryInfo("C:\\Images");
  /// imageList.SaveToDirectory(directory);
  /// // All images in the imageList are now saved to the specified directory.
  /// </code>
  /// </example>
  public static void SaveToDirectory(this ImageList @this, DirectoryInfo directory) => SaveToDirectory(@this, directory.FullName);

}
