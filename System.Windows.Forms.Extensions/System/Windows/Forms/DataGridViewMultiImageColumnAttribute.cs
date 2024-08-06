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

namespace System.Windows.Forms;

/// <summary>
/// Specifies that a column in a <see cref="System.Windows.Forms.DataGridView"/> should display multiple images with specified click events and tooltips.
/// </summary>
/// <param name="onClickMethodName">The name of the method to call when an image is clicked.</param>
/// <param name="toolTipProviderMethodName">The name of the method that provides the tooltip text for the images.</param>
/// <param name="maximumImageSize">The maximum size of the images displayed in the column cells.</param>
/// <param name="paddingLeft">The left padding around the images within the cells.</param>
/// <param name="paddingTop">The top padding around the images within the cells.</param>
/// <param name="paddingRight">The right padding around the images within the cells.</param>
/// <param name="paddingBottom">The bottom padding around the images within the cells.</param>
/// <param name="marginLeft">The left margin around the cells.</param>
/// <param name="marginTop">The top margin around the cells.</param>
/// <param name="marginRight">The right margin around the cells.</param>
/// <param name="marginBottom">The bottom margin around the cells.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///
///     public void OnImageClick(object sender, int imageIndex)
///     {
///         // Handle image click event
///         Console.WriteLine($"Image {imageIndex} clicked in row {Id}");
///     }
///
///     public string GetTooltipText(object sender, int imageIndex)
///     {
///         // Provide tooltip text for the images
///         return $"Tooltip for image {imageIndex} in row {Id}";
///     }
///
///     [DataGridViewMultiImageColumn(
///         onClickMethodName: nameof(OnImageClick),
///         toolTipProviderMethodName: nameof(GetTooltipText),
///         maximumImageSize: 24,
///         paddingLeft: 2,
///         paddingTop: 2,
///         paddingRight: 2,
///         paddingBottom: 2,
///         marginLeft: 2,
///         marginTop: 2,
///         marginRight: 2,
///         marginBottom: 2)]
///     public string Images { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", Images = "image1" },
///     new DataRow { Id = 2, Name = "Row 2", Images = "image2" }
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
public sealed class DataGridViewMultiImageColumnAttribute(
  string onClickMethodName,
  string toolTipProviderMethodName,
  int maximumImageSize,
  int paddingLeft,
  int paddingTop,
  int paddingRight,
  int paddingBottom,
  int marginLeft,
  int marginTop,
  int marginRight,
  int marginBottom
)
  : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="DataGridViewMultiImageColumnAttribute"/> class with uniform padding and margin.
  /// </summary>
  /// <param name="onClickMethodName">(Optional: defaults to <see langword="null"/>) The name of the method to call when an image is clicked.</param>
  /// <param name="toolTipProviderMethodName">(Optional: defaults to <see langword="null"/>) The name of the method that provides the tooltip text for the images.</param>
  /// <param name="maximumImageSize">(Optional: defaults to 24) The maximum size of the images displayed in the column cells.</param>
  /// <param name="padding">(Optional: defaults to 0) The uniform padding around the images within the cells.</param>
  /// <param name="margin">(Optional: defaults to 0) The uniform margin around the cells.</param>
  public DataGridViewMultiImageColumnAttribute(
    string onClickMethodName = null,
    string toolTipProviderMethodName = null,
    int maximumImageSize = 24,
    int padding = 0,
    int margin = 0
  )
    : this(
      onClickMethodName,
      toolTipProviderMethodName,
      maximumImageSize,
      padding,
      padding,
      padding,
      padding,
      margin,
      margin,
      margin,
      margin
    ) { }

  /// <summary>
  /// Gets the maximum size of the images displayed in the column cells.
  /// </summary>
  internal int MaximumImageSize { get; } = maximumImageSize;
  
  /// <summary>
  /// Gets the name of the method to call when an image is clicked.
  /// </summary>
  internal string OnClickMethodName { get; } = onClickMethodName;

  /// <summary>
  /// Gets the name of the method that provides the tooltip text for the images.
  /// </summary>
  internal string ToolTipProviderMethodName { get; } = toolTipProviderMethodName;

  /// <summary>
  /// Gets the padding around the images within the cells.
  /// </summary>
  internal Padding Padding { get; } = new(paddingLeft, paddingTop, paddingRight, paddingBottom);

  /// <summary>
  /// Gets the margin around the cells.
  /// </summary>
  internal Padding Margin { get; } = new(marginLeft, marginTop, marginRight, marginBottom);

}
