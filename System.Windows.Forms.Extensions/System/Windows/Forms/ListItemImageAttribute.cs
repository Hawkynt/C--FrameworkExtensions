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
/// Specifies an image to display next to items in a <see cref="ListView"/>, <see cref="ListBox"/>, or <see cref="ComboBox"/>.
/// This attribute is applied at the class level and provides image configuration for all items.
/// </summary>
/// <param name="imageListPropertyName">
/// The name of the property that provides the <see cref="ImageList"/> containing the images.
/// </param>
/// <param name="imageKeyPropertyName">
/// The name of the property that provides the image key (string) to look up in the <see cref="ImageList"/>.
/// </param>
/// <param name="imageIndexPropertyName">
/// The name of the property that provides the image index (int) to look up in the <see cref="ImageList"/>.
/// Use this as an alternative to <paramref name="imageKeyPropertyName"/>.
/// </param>
/// <example>
/// <code>
/// // Display status icons next to items
/// [ListItemImage(imageListPropertyName: nameof(Icons), imageKeyPropertyName: nameof(StatusIcon))]
/// public class StatusItem {
///   public string Name { get; set; }
///   public ImageList Icons { get; set; }
///   public string StatusIcon { get; set; }  // "success", "warning", "error", etc.
/// }
///
/// // Alternative using image index
/// [ListItemImage(imageListPropertyName: nameof(Icons), imageIndexPropertyName: nameof(IconIndex))]
/// public class IndexedItem {
///   public string Name { get; set; }
///   public ImageList Icons { get; set; }
///   public int IconIndex { get; set; }  // 0, 1, 2, etc.
/// }
///
/// // Usage with ListView
/// listView.EnableExtendedAttributes();
/// listView.DataSource = items;
///
/// // Usage with ListBox
/// listBox.EnableExtendedAttributes();
/// listBox.DataSource = items;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ListItemImageAttribute(
  string imageListPropertyName = null,
  string imageKeyPropertyName = null,
  string imageIndexPropertyName = null
) : Attribute {

  internal string ImageListPropertyName { get; } = imageListPropertyName;
  internal string ImageKeyPropertyName { get; } = imageKeyPropertyName;
  internal string ImageIndexPropertyName { get; } = imageIndexPropertyName;

  /// <summary>
  /// Gets the <see cref="ImageList"/> from the data object.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The <see cref="ImageList"/>, or <see langword="null"/> if not available.</returns>
  internal ImageList GetImageList(object data)
    => ListControlExtensions.GetPropertyValueOrDefault<ImageList>(data, this.ImageListPropertyName, null, null, null, null);

  /// <summary>
  /// Gets the image for the given data object.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The <see cref="Image"/>, or <see langword="null"/> if not available.</returns>
  internal Image GetImage(object data) {
    var imageList = this.GetImageList(data);
    if (imageList == null)
      return null;

    // Try image key first
    if (this.ImageKeyPropertyName != null) {
      var key = ListControlExtensions.GetPropertyValueOrDefault<string>(data, this.ImageKeyPropertyName, null, null, null, null);
      if (key != null && imageList.Images.ContainsKey(key))
        return imageList.Images[key];
    }

    // Try image index
    if (this.ImageIndexPropertyName != null) {
      var index = ListControlExtensions.GetPropertyValueOrDefault(data, this.ImageIndexPropertyName, -1, -1, -1, -1);
      if (index >= 0 && index < imageList.Images.Count)
        return imageList.Images[index];
    }

    return null;
  }

  /// <summary>
  /// Gets the image key for the given data object (for ListView SmallImageList/LargeImageList).
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The image key, or <see langword="null"/> if using index or not available.</returns>
  internal string GetImageKey(object data)
    => this.ImageKeyPropertyName != null
      ? ListControlExtensions.GetPropertyValueOrDefault<string>(data, this.ImageKeyPropertyName, null, null, null, null)
      : null;

  /// <summary>
  /// Gets the image index for the given data object (for ListView SmallImageList/LargeImageList).
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The image index, or -1 if using key or not available.</returns>
  internal int GetImageIndex(object data)
    => this.ImageIndexPropertyName != null
      ? ListControlExtensions.GetPropertyValueOrDefault(data, this.ImageIndexPropertyName, -1, -1, -1, -1)
      : -1;
}
