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

using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///   allows to show an image alongside to the displayed text.
/// </summary>
public sealed class DataGridViewTextAndImageColumnAttribute : Attribute {
  public string ImageListPropertyName { get; }
  public string ImageKeyPropertyName { get; }
  public string ImagePropertyName { get; }
  public TextImageRelation TextImageRelation { get; }
  public uint FixedImageWidth { get; }
  public uint FixedImageHeight { get; }
  public bool KeepAspectRatio { get; }

  public DataGridViewTextAndImageColumnAttribute(string imageListPropertyName, string imageKeyPropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText, uint fixedImageWidth = 0,
    uint fixedImageHeight = 0, bool keepAspectRatio = true) {
    this.ImageListPropertyName = imageListPropertyName;
    this.ImageKeyPropertyName = imageKeyPropertyName;
    this.TextImageRelation = textImageRelation;
    this.FixedImageWidth = fixedImageWidth;
    this.FixedImageHeight = fixedImageHeight;
    this.KeepAspectRatio = keepAspectRatio;
  }

  public DataGridViewTextAndImageColumnAttribute(string imagePropertyName,
    TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText, uint fixedImageWidth = 0,
    uint fixedImageHeight = 0, bool keepAspectRatio = true) {
    this.ImagePropertyName = imagePropertyName;
    this.TextImageRelation = textImageRelation;
    this.FixedImageWidth = fixedImageWidth;
    this.FixedImageHeight = fixedImageHeight;
    this.KeepAspectRatio = keepAspectRatio;
  }

  private Image _GetImage(object row, object value) {
    if (value is null)
      return null;

    Image image = null;
    var imagePropertyName = this.ImagePropertyName;
    if (imagePropertyName != null)
      image = DataGridViewExtensions.GetPropertyValueOrDefault<Image>(row, imagePropertyName, null, null, null, null);
    else {
      var imageList =
        DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(row, this.ImageListPropertyName, null, null, null,
          null);
      if (imageList == null)
        return null;

      var imageKey =
        DataGridViewExtensions.GetPropertyValueOrDefault<object>(row, this.ImageKeyPropertyName, null, null, null,
          null);
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
