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
/// Allows defining the cell tooltip in a <see cref="System.Windows.Forms.DataGridView"/> for automatically generated columns.
/// </summary>
/// <param name="toolTipText">The text to be used as the tooltip for the cell.</param>
/// <param name="toolTipTextPropertyName">The name of the property that retrieves the value for the tooltip text.</param>
/// <param name="conditionalPropertyName">The name of the <see cref="bool"/> property that decides if this attribute should be enabled.</param>
/// <param name="format">The format string to apply to the tooltip text.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsTooltipEnabled { get; set; }
///     public string TooltipText { get; set; }
///
///     [DataGridViewCellTooltip(tooltipText: "Default tooltip", toolTipTextPropertyName: nameof(TooltipText), conditionalPropertyName: nameof(IsTooltipEnabled))]
///     public string DisplayText { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsTooltipEnabled = true, TooltipText = "Tooltip for Row 1", DisplayText = "Row 1 Display" },
///     new DataRow { Id = 2, Name = "Row 2", IsTooltipEnabled = false, TooltipText = "Tooltip for Row 2", DisplayText = "Row 2 Display" }
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
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DataGridViewCellTooltipAttribute(
  string toolTipText = null,
  string toolTipTextPropertyName = null,
  string conditionalPropertyName = null,
  string format = null
)
  : Attribute {

  private void _ApplyTo(DataGridViewCell cell, object data) {
    var conditional = DataGridViewExtensions.GetPropertyValueOrDefault(data, conditionalPropertyName, false, true, false, false);
    if (!conditional) {
      cell.ToolTipText = string.Empty;
      return;
    }

    var text = DataGridViewExtensions.GetPropertyValueOrDefault<object>(data, toolTipTextPropertyName, null, null, null, null) ?? toolTipText;
    cell.ToolTipText = (format != null && text is IFormattable f ? f.ToString(format, null) : text?.ToString())
                       ?? string.Empty
      ;
  }

  internal static void OnCellFormatting(DataGridViewCellTooltipAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is { } dgvCell)
      @this._ApplyTo(dgvCell, data);
  }

}
