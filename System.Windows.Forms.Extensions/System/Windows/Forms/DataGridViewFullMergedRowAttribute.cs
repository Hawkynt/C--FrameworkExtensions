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
/// Specifies that a row in a <see cref="System.Windows.Forms.DataGridView"/> should be fully merged and formatted as a heading row.
/// </summary>
/// <param name="headingTextPropertyName">The name of the property that provides the heading text for the row.</param>
/// <param name="foreColor">(Optional: defaults to <see langword="null"/>) The color name to use for the heading text.</param>
/// <param name="textSize">(Optional: defaults to -1) The size of the heading text.</param>
/// <example>
/// <code>
/// // Define a common interface for the data grid view rows
/// public interface IDataRow
/// {
///     int Id { get; set; }
///     string Name { get; set; }
/// }
///
/// // Define a custom class for regular rows
/// public class RegularRow : IDataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
/// }
///
/// // Define a custom class for heading rows
/// [DataGridViewFullMergedRow(nameof(HeadingText), foreColor: "Blue", textSize: 12)]
/// public class HeadingRow : IDataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public string HeadingText { get; set; }
/// }
///
/// // Create an array of IDataRow instances
/// var dataRows = new IDataRow[]
/// {
///     new HeadingRow { Id = 0, Name = "Header", HeadingText = "Heading 1" },
///     new RegularRow { Id = 1, Name = "Row 1" },
///     new RegularRow { Id = 2, Name = "Row 2" },
///     new HeadingRow { Id = 3, Name = "Header", HeadingText = "Heading 2" },
///     new RegularRow { Id = 4, Name = "Row 3" }
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
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewFullMergedRowAttribute(
  string headingTextPropertyName,
  string foreColor = null,
  float textSize = -1
)
  : Attribute {
  internal Color? ForeColor { get; } = foreColor?.ParseColor();
  internal float? TextSize { get; } = textSize < 0 ? null : textSize;
  
  public string GetHeadingText(object rowData)
    => DataGridViewExtensions.GetPropertyValueOrDefault(rowData, headingTextPropertyName, string.Empty, string.Empty, string.Empty, string.Empty);

}
