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
/// Specifies that a column in a <see cref="System.Windows.Forms.DataGridView"/> should display images, and allows defining click, double-click events, and tooltip text.
/// </summary>
/// <param name="imageListPropertyName">(Optional: defaults to <see langword="null"/>) The name of the property that provides the image list for the column.</param>
/// <param name="onClickMethodName">(Optional: defaults to <see langword="null"/>) The name of the method to call when the image is clicked.</param>
/// <param name="onDoubleClickMethodName">(Optional: defaults to <see langword="null"/>) The name of the method to call when the image is double-clicked.</param>
/// <param name="toolTipTextPropertyName">(Optional: defaults to <see langword="null"/>) The name of the property that provides the tooltip text for the image.</param>
/// <remarks>
/// The column can either be of type <see cref="int"/> indexing in the <see cref="ImageList"/> or anything else converted via ToString() and used as a key to find the <see cref="Image"/> in the <see cref="ImageList"/>.
/// </remarks>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public ImageList ImageList { get; set; }
///     public string TooltipText { get; set; }
///
///     [DataGridViewImageColumn(nameof(ImageList), nameof(OnImageClick), nameof(OnImageDoubleClick), nameof(TooltipText))]
///     public int ImageIndex { get; set; }
///
///     public void OnImageClick()
///     {
///         // Handle image click event
///         Console.WriteLine("Image clicked");
///     }
///
///     public void OnImageDoubleClick()
///     {
///         // Handle image double-click event
///         Console.WriteLine("Image double-clicked");
///     }
/// }
///
/// // Create an ImageList with at least two images
/// var imageList = new ImageList();
/// imageList.Images.Add(Image.FromFile("path/to/image1.png"));
/// imageList.Images.Add(Image.FromFile("path/to/image2.png"));
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", ImageList = imageList, TooltipText = "Tooltip 1", ImageIndex = 0 },
///     new DataRow { Id = 2, Name = "Row 2", ImageList = imageList, TooltipText = "Tooltip 2", ImageIndex = 1 }
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
public sealed class DataGridViewImageColumnAttribute(
  string imageListPropertyName = null,
  string onClickMethodName = null,
  string onDoubleClickMethodName = null,
  string toolTipTextPropertyName = null
)
  : DataGridViewClickableAttribute(
    onClickMethodName,
    onDoubleClickMethodName
  ) {

  private Image _GetImage(object row, object value) {
    var imageList = DataGridViewExtensions.GetPropertyValueOrDefault<ImageList>(
      row,
      imageListPropertyName,
      null,
      null,
      null,
      null
    );
    if (imageList == null)
      return value as Image;

    var result = value is int index && !index.GetType().IsEnum
      ? imageList.Images[index]
      : imageList.Images[value.ToString()];

    return result;
  }

  private string _ToolTipText(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault<string>(row, toolTipTextPropertyName, null, null, null, null);

  internal static void OnCellFormatting(DataGridViewImageColumnAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    //should not be necessary but dgv throws format exception
    if (e.DesiredType != typeof(Image))
      return;

    e.Value = @this._GetImage(data, e.Value);
    e.FormattingApplied = true;
    row.Cells[column.Index].ToolTipText = @this._ToolTipText(data);
  }

}
