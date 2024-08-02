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
/// Represents a <see cref="System.Windows.Forms.DataGridViewTextBoxColumn"/> that can display both an image and text.
/// </summary>
/// <example>
/// <code>
/// // Create a DataGridView and set its data source
/// var dataGridView = new DataGridView
/// {
///     DataSource = new[]
///     {
///         new { Id = 1, Name = "Row 1" },
///         new { Id = 2, Name = "Row 2" }
///     }
/// };
///
/// // Create a DataGridViewImageAndTextColumn and add it to the DataGridView
/// var imageAndTextColumn = new DataGridViewImageAndTextColumn
/// {
///     Name = "ImageAndTextColumn",
///     HeaderText = "Image and Text",
///     DataPropertyName = "Name",
///     IsDataBound = true,
///     Image = Image.FromFile("path/to/defaultImage.png")
/// };
/// dataGridView.Columns.Add(imageAndTextColumn);
/// </code>
/// </example>
public partial class DataGridViewImageAndTextColumn : DataGridViewTextBoxColumn {
  
  private Image _imageValue;

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewImageAndTextColumn"/> class.
  /// </summary>
  public DataGridViewImageAndTextColumn() => this.CellTemplate = new DataGridViewTextAndImageCell();

  /// <inheritdoc />
  public override object Clone() {
    var c = base.Clone() as DataGridViewImageAndTextColumn;
    c._imageValue = this._imageValue;
    c.ImageSize = this.ImageSize;
    return c;
  }

  /// <summary>
  /// Gets or sets the image displayed in the column cells.
  /// </summary>
  public Image Image {
    get => this._imageValue;
    set {
      if (this.Image == value)
        return;

      this._imageValue = value;
      this.ImageSize = value.Size;

      if (this.InheritedStyle == null)
        return;

      var inheritedPadding = this.InheritedStyle.Padding;
      this.DefaultCellStyle.Padding = new(
        this.ImageSize.Width,
        inheritedPadding.Top,
        inheritedPadding.Right,
        inheritedPadding.Bottom
      );
    }
  }

  /// <summary>
  /// Gets the size of the image displayed in the column cells.
  /// </summary>
  internal Size ImageSize { get; private set; }
}
