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
///   Allows defining the cell tooltip in a DataGridView for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DataGridViewCellTooltipAttribute(
  string tooltipText = null,
  string tooltipTextPropertyName = null,
  string conditionalPropertyName = null,
  string format = null
)
  : Attribute {
  public string ToolTipText { get; } = tooltipText;
  public string TooltipTextPropertyName { get; } = tooltipTextPropertyName;
  public string ConditionalPropertyName { get; } = conditionalPropertyName;
  public string Format { get; } = format;

  private void _ApplyTo(DataGridViewCell cell, object data) {
    var conditional =
      DataGridViewExtensions.GetPropertyValueOrDefault(data, this.ConditionalPropertyName, false, true, false, false);
    if (!conditional) {
      cell.ToolTipText = string.Empty;
      return;
    }

    var text = DataGridViewExtensions.GetPropertyValueOrDefault<object>(data, this.TooltipTextPropertyName, null, null, null, null) ?? this.ToolTipText;
    cell.ToolTipText = (this.Format != null && text is IFormattable f ? f.ToString(this.Format, null) : text?.ToString())
                       ?? string.Empty
      ;
  }

  public static void OnCellFormatting(DataGridViewCellTooltipAttribute @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    if (row.Cells[e.ColumnIndex] is { } dgvCell)
      @this._ApplyTo(dgvCell, data);
  }
}
