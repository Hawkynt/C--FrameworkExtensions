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

using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///   allows to show an image alongside to the displayed text.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewTextAndImageColumnAttribute(
  string imageListPropertyName,
  string imageKeyPropertyName,
  TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText,
  uint fixedImageWidth = 0,
  uint fixedImageHeight = 0,
  bool keepAspectRatio = true
)
  : Attribute {
  public string ImageListPropertyName { get; } = imageListPropertyName;
  public string ImageKeyPropertyName { get; } = imageKeyPropertyName;
  public string ImagePropertyName { get; }
  public TextImageRelation TextImageRelation { get; } = textImageRelation;
  public uint FixedImageWidth { get; } = fixedImageWidth;
  public uint FixedImageHeight { get; } = fixedImageHeight;
  public bool KeepAspectRatio { get; } = keepAspectRatio;

  public DataGridViewTextAndImageColumnAttribute(
    string imagePropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText,
    uint fixedImageWidth = 0,
    uint fixedImageHeight = 0,
    bool keepAspectRatio = true
  ) : this(null, null, textImageRelation, fixedImageWidth, fixedImageHeight, keepAspectRatio)
    => this.ImagePropertyName = imagePropertyName;

  private Image _GetImage(object row, object value) {
    if (value is null)
      return null;

    Image image = null;
    var imagePropertyName = this.ImagePropertyName;
    if (imagePropertyName != null)
      image = DataGridViewExtensions.GetPropertyValueOrDefault<Image>(row, imagePropertyName, null, null, null, null);
    else {
      var imageList =
        DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(
          row,
          this.ImageListPropertyName,
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
          this.ImageKeyPropertyName,
          null,
          null,
          null,
          null
        );
      if (imageKey != null)
        image = imageKey is int index && !index.GetType().IsEnum
            ? imageList.Images[index]
            : imageList.Images[imageKey.ToString()]
          ;
    }

    return image;
  }

  public static void OnCellFormatting(DataGridViewTextAndImageColumnAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is not DataGridViewImageAndTextColumn.DataGridViewTextAndImageCell textAndImageCell)
      return;

    textAndImageCell.TextImageRelation = @this.TextImageRelation;
    textAndImageCell.KeepAspectRatio = @this.KeepAspectRatio;
    textAndImageCell.FixedImageWidth = @this.FixedImageWidth;
    textAndImageCell.FixedImageHeight = @this.FixedImageHeight;
    textAndImageCell.Image = @this._GetImage(data, e.Value);
    e.FormattingApplied = true;
  }
}
