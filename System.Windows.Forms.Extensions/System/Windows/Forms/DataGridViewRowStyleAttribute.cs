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
/// Specifies the style of a row in a <see cref="System.Windows.Forms.DataGridView"/> with various conditions.
/// </summary>
/// <param name="foreColor">(Optional: defaults to <see langword="null"/>) The foreground color for the row cells.</param>
/// <param name="backColor">(Optional: defaults to <see langword="null"/>) The background color for the row cells.</param>
/// <param name="format">(Optional: defaults to <see langword="null"/>) The format string for the row cells.</param>
/// <param name="conditionalPropertyName">(Optional: defaults to <see langword="null"/>) The name of a boolean property that enables or disables the style setting. When this property is <see langword="true"/>, the style is applied.</param>
/// <param name="foreColorPropertyName">(Optional: defaults to <see langword="null"/>) The name of a property that provides the foreground color for the row cells.</param>
/// <param name="backColorPropertyName">(Optional: defaults to <see langword="null"/>) The name of a property that provides the background color for the row cells.</param>
/// <param name="isBold">(Optional: defaults to <see langword="false"/>) Indicates if the font should be bold.</param>
/// <param name="isItalic">(Optional: defaults to <see langword="false"/>) Indicates if the font should be italic.</param>
/// <param name="isStrikeout">(Optional: defaults to <see langword="false"/>) Indicates if the font should be strikeout.</param>
/// <param name="isUnderline">(Optional: defaults to <see langword="false"/>) Indicates if the font should be underline.</param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// [DataGridViewRowStyle(foreColorPropertyName: nameof(SpecialName), backColor: "Yellow", isBold: true, conditionalPropertyName: nameof(IsSpecial))]
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsSpecial { get; set; }
///     public Color SpecialName { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsSpecial = true, SpecialName = Color.Red },
///     new DataRow { Id = 2, Name = "Row 2", IsSpecial = false }
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
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class DataGridViewRowStyleAttribute(
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
) : Attribute {

  private readonly Color? _foreColor  = foreColor?.ParseColor();
  private readonly Color? _backColor = backColor?.ParseColor();
  private readonly FontStyle _fontStyle = FontStyle.Regular
                                         | (isBold ? FontStyle.Bold : FontStyle.Regular)
                                         | (isItalic ? FontStyle.Italic : FontStyle.Regular)
                                         | (isStrikeout ? FontStyle.Strikeout : FontStyle.Regular)
                                         | (isUnderline ? FontStyle.Underline : FontStyle.Regular);


  private void _ApplyTo(DataGridViewRow row, object rowData) {
    var style = row.DefaultCellStyle;

    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
                  rowData,
                  foreColorPropertyName,
                  null,
                  null,
                  null,
                  null
                )
                ?? this._foreColor;

    if (color != null)
      style.ForeColor = color.Value;

    color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
              rowData,
              backColorPropertyName,
              null,
              null,
              null,
              null
            )
            ?? this._backColor;

    if (color != null)
      style.BackColor = color.Value;

    if (format != null)
      style.Format = format;

    if (this._fontStyle != FontStyle.Regular)
      style.Font = new(style.Font ?? row.InheritedStyle.Font, this._fontStyle);
  }

  private bool _IsEnabled(object value)
    => DataGridViewExtensions.GetPropertyValueOrDefault(value, conditionalPropertyName, true, true, false, false);

  internal static void OnRowPrepaint(IEnumerable<DataGridViewRowStyleAttribute> @this, DataGridViewRow row, object data, DataGridViewRowPrePaintEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(row, data);
  }

}
