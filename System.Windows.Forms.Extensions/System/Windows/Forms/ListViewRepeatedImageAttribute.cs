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

using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Specifies that a column in a <see cref="ListView"/> should display repeated images based on a numeric value.
/// This is useful for displaying ratings (e.g., stars), progress indicators, or other visual counts.
/// </summary>
/// <param name="imageListPropertyName">
/// The name of the property that provides the <see cref="ImageList"/> containing the image to repeat.
/// </param>
/// <param name="imageKey">
/// The key of the image in the <see cref="ImageList"/> to repeat.
/// </param>
/// <param name="maxCount">
/// The maximum number of times the image can be repeated. Default is 5.
/// </param>
/// <example>
/// <code>
/// public class Product {
///   [ListViewColumn("Name")]
///   public string Name { get; set; }
///
///   [ListViewColumn("Rating")]
///   [ListViewRepeatedImage(nameof(StarImages), "star", MaxCount = 5)]
///   public int Rating { get; set; }  // Shows 0-5 stars based on value
///
///   [ListViewColumn("Quality")]
///   [ListViewRepeatedImage(nameof(StarImages), "diamond", MaxCount = 3)]
///   public int Quality { get; set; }  // Shows 0-3 diamonds
///
///   public ImageList StarImages { get; set; }
/// }
///
/// // Setup ImageList
/// var imageList = new ImageList { ImageSize = new Size(16, 16) };
/// imageList.Images.Add("star", Resources.Star);
/// imageList.Images.Add("diamond", Resources.Diamond);
///
/// // Usage
/// listView.EnableExtendedAttributes();
/// listView.DataSource = products.Select(p => new Product {
///   Name = p.Name,
///   Rating = p.Rating,
///   Quality = p.Quality,
///   StarImages = imageList
/// });
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ListViewRepeatedImageAttribute(
  string imageListPropertyName,
  string imageKey,
  int maxCount = 5
) : Attribute {

  /// <summary>
  /// Gets the name of the property that provides the <see cref="ImageList"/>.
  /// </summary>
  internal string ImageListPropertyName { get; } = imageListPropertyName;

  /// <summary>
  /// Gets the key of the image to repeat.
  /// </summary>
  internal string ImageKey { get; } = imageKey;

  /// <summary>
  /// Gets the maximum number of times the image can be repeated.
  /// </summary>
  internal int MaxCount { get; } = maxCount;

  /// <summary>
  /// Gets the <see cref="ImageList"/> from the data object.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The <see cref="ImageList"/>, or <see langword="null"/> if not available.</returns>
  internal ImageList GetImageList(object data)
    => ListControlExtensions.GetPropertyValueOrDefault<ImageList>(data, this.ImageListPropertyName, null, null, null, null);

  /// <summary>
  /// Gets the image to repeat from the <see cref="ImageList"/>.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The <see cref="Image"/> to repeat, or <see langword="null"/> if not available.</returns>
  internal Image GetImage(object data) {
    var imageList = this.GetImageList(data);
    if (imageList == null || !imageList.Images.ContainsKey(this.ImageKey))
      return null;

    return imageList.Images[this.ImageKey];
  }
}
