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
/// Specifies that a column in a <see cref="System.Windows.Forms.DataGridView"/> should display both text and images with specified settings.
/// </summary>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public string ImageKey { get; set; }
///     public ImageList ImageList { get; set; }
///
///     [DataGridViewImageAndTextColumn(imageListPropertyName: nameof(ImageList), imageKeyPropertyName: nameof(ImageKey))]
///     public string DisplayName => Name;
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", ImageKey = "key1", ImageList = imageList },
///     new DataRow { Id = 2, Name = "Row 2", ImageKey = "key2", ImageList = imageList }
/// };
///
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = dataRows
/// };
///
/// // Enable extended attributes to recognize the custom attributes
/// dataGridView.EnableExtendedAttributes();
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewImageAndTextColumnAttribute : Attribute {

  private readonly uint _fixedImageHeight;
  private readonly uint _fixedImageWidth;
  private readonly bool _keepAspectRatio;
  private readonly TextImageRelation _textImageRelation;
  private readonly Func<object, object, Image> _imageGetter;

  private DataGridViewImageAndTextColumnAttribute(
    TextImageRelation textImageRelation,
    uint fixedImageWidth,
    uint fixedImageHeight,
    bool keepAspectRatio
  ) {
    this._textImageRelation = textImageRelation;
    this._fixedImageWidth = fixedImageWidth;
    this._fixedImageHeight = fixedImageHeight;
    this._keepAspectRatio = keepAspectRatio;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewImageAndTextColumnAttribute"/> class with image list and key properties.
  /// </summary>
  /// <param name="imageListPropertyName">The name of the property that provides the <see cref="System.Windows.Forms.ImageList"/>.</param>
  /// <param name="imageKeyPropertyName">The name of the property that provides the key for the image in the <see cref="System.Windows.Forms.ImageList"/>.</param>
  /// <param name="textImageRelation">(Optional: defaults to <see cref="System.Windows.Forms.TextImageRelation.ImageBeforeText"/>) Specifies the position of the image relative to the text.</param>
  /// <param name="fixedImageWidth">(Optional: defaults to 0) The fixed width of the image in pixels.</param>
  /// <param name="fixedImageHeight">(Optional: defaults to 0) The fixed height of the image in pixels.</param>
  /// <param name="keepAspectRatio">(Optional: defaults to <see langword="true"/>) Indicates whether to maintain the aspect ratio of the image.</param>
  public DataGridViewImageAndTextColumnAttribute(
    string imageListPropertyName,
    string imageKeyPropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText,
    uint fixedImageWidth = 0,
    uint fixedImageHeight = 0,
    bool keepAspectRatio = true
  ) : this(textImageRelation, fixedImageWidth, fixedImageHeight, keepAspectRatio) {
    this._imageGetter = GetImageFromList;
    return;

    Image GetImageFromList(object row, object value) {
      if (value is null)
        return null;

      var imageList =
        DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(
          row,
          imageListPropertyName,
          null,
          null,
          null,
          null
        );
      if (imageList == null)
        return null;

      var imageKey =
        DataGridViewExtensions.GetPropertyValueOrDefault<object>(
          row,
          imageKeyPropertyName,
          null,
          null,
          null,
          null
        );

      return imageKey != null
          ? imageKey is int index && !index.GetType().IsEnum
            ? imageList.Images[index]
            : imageList.Images[imageKey.ToString()]
          : null
        ;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewImageAndTextColumnAttribute"/> class with an image property.
  /// </summary>
  /// <param name="imagePropertyName">The name of the property that provides the <see cref="System.Drawing.Image"/> directly.</param>
  /// <param name="textImageRelation">(Optional: defaults to <see cref="System.Windows.Forms.TextImageRelation.ImageBeforeText"/>) Specifies the position of the image relative to the text.</param>
  /// <param name="fixedImageWidth">(Optional: defaults to 0) The fixed width of the image in pixels.</param>
  /// <param name="fixedImageHeight">(Optional: defaults to 0) The fixed height of the image in pixels.</param>
  /// <param name="keepAspectRatio">(Optional: defaults to <see langword="true"/>) Indicates whether to maintain the aspect ratio of the image.</param>
  public DataGridViewImageAndTextColumnAttribute(
    string imagePropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText,
    uint fixedImageWidth = 0,
    uint fixedImageHeight = 0,
    bool keepAspectRatio = true
  ) : this(textImageRelation, fixedImageWidth, fixedImageHeight, keepAspectRatio) {
    this._imageGetter = GetImageFromProperty;
    return;

    Image GetImageFromProperty(object row, object value) {
      if (value is null)
        return null;

      return imagePropertyName != null
          ? DataGridViewExtensions.GetPropertyValueOrDefault<Image>(row, imagePropertyName, null, null, null, null)
          : null
        ;

    }
  }

  internal static void OnCellFormatting(DataGridViewImageAndTextColumnAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is not DataGridViewImageAndTextColumn.DataGridViewTextAndImageCell textAndImageCell)
      return;

    textAndImageCell.TextImageRelation = @this._textImageRelation;
    textAndImageCell.KeepAspectRatio = @this._keepAspectRatio;
    textAndImageCell.FixedImageWidth = @this._fixedImageWidth;
    textAndImageCell.FixedImageHeight = @this._fixedImageHeight;
    textAndImageCell.Image = @this._imageGetter(data, e.Value);
    e.FormattingApplied = true;
  }

}
