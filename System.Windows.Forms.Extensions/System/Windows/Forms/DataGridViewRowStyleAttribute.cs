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

using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///   Allows adjusting the cell style in a DataGridView for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class DataGridViewRowStyleAttribute : Attribute {
  public DataGridViewRowStyleAttribute(
    string foreColor = null,
    string backColor = null,
    string format = null,
    string conditionalPropertyName = null,
    string foreColorPropertyName = null,
    string backColorPropertyName = null,
    bool isBold = false,
    bool isItalic = false,
    bool isStrikeout = false,
    bool isUnderline = false
  ) {
    this.ForeColor = foreColor?.ParseColor();
    this.BackColor = backColor?.ParseColor();
    this.ConditionalPropertyName = conditionalPropertyName;
    this.Format = format;
    this.ForeColorPropertyName = foreColorPropertyName;
    this.BackColorPropertyName = backColorPropertyName;
    var fontStyle = FontStyle.Regular;
    if (isBold)
      fontStyle |= FontStyle.Bold;
    if (isItalic)
      fontStyle |= FontStyle.Italic;
    if (isStrikeout)
      fontStyle |= FontStyle.Strikeout;
    if (isUnderline)
      fontStyle |= FontStyle.Underline;
    this.FontStyle = fontStyle;
  }

  public string ConditionalPropertyName { get; }
  public Color? ForeColor { get; }
  public Color? BackColor { get; }
  public string Format { get; }
  public FontStyle FontStyle { get; }
  public string ForeColorPropertyName { get; }
  public string BackColorPropertyName { get; }

  private void _ApplyTo(DataGridViewRow row, object rowData) {
    var style = row.DefaultCellStyle;

    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
                  rowData,
                  this.ForeColorPropertyName,
                  null,
                  null,
                  null,
                  null
                )
                ?? this.ForeColor;
    if (color != null)
      style.ForeColor = color.Value;

    color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
              rowData,
              this.BackColorPropertyName,
              null,
              null,
              null,
              null
            )
            ?? this.BackColor;
    if (color != null)
      style.BackColor = color.Value;

    if (this.Format != null)
      style.Format = this.Format;

    if (this.FontStyle != FontStyle.Regular)
      style.Font = new(style.Font ?? row.InheritedStyle.Font, this.FontStyle);
  }

  private bool _IsEnabled(object value)
    => DataGridViewExtensions.GetPropertyValueOrDefault(value, this.ConditionalPropertyName, true, true, false, false);

  public static void OnRowPrepaint(IEnumerable<DataGridViewRowStyleAttribute> @this, DataGridViewRow row, object data, DataGridViewRowPrePaintEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(row, data);
  }
}
