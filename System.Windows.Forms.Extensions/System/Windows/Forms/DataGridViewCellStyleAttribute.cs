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
/// Allows adjusting the cell style in a <see cref="System.Windows.Forms.DataGridView"/> for automatically generated columns.
/// </summary>
/// <param name="foreColor">
/// The color name used for the property <see cref="DataGridViewCellStyle.ForeColor"/>.
/// <remarks>Supports many types of color names, such as hex values, (a)rgb values, known colors, system colors, etc.</remarks>
/// </param>
/// <param name="backColor">
/// The color name used for the property <see cref="DataGridViewCellStyle.BackColor"/>.
/// <remarks>Supports many types of color names, such as hex values, (a)rgb values, known colors, system colors, etc.</remarks>
/// </param>
/// <param name="format">The value for the property <see cref="DataGridViewCellStyle.Format"/>.</param>
/// <param name="alignment">The value for the property <see cref="DataGridViewCellStyle.Alignment"/>.</param>
/// <param name="wrapMode">The value for the property <see cref="DataGridViewCellStyle.WrapMode"/>.</param>
/// <param name="conditionalPropertyName">
/// The name of the <see cref="bool"/> property that decides if this attribute should be enabled.
/// </param>
/// <param name="foreColorPropertyName">
/// The name of the <see cref="System.Drawing.Color"/> property that retrieves the value for <see cref="DataGridViewCellStyle.ForeColor"/>.
/// </param>
/// <param name="backColorPropertyName">
/// The name of the <see cref="System.Drawing.Color"/> property that retrieves the value for <see cref="DataGridViewCellStyle.BackColor"/>.
/// </param>
/// <param name="wrapModePropertyName">
/// The name of the <see cref="DataGridViewTriState"/> property that retrieves the value for <see cref="DataGridViewCellStyle.WrapMode"/>.
/// </param>
/// <example>
/// <code>
/// // Define a custom class for the data grid view rows
/// public class DataRow
/// {
///     public int Id { get; set; }
///     public string Name { get; set; }
///     public bool IsHighlighted { get; set; }
///     public System.Drawing.Color HighlightColor { get; set; }
///
///     [DataGridViewCellStyle(foreColor: "Red", backColor: "Yellow", conditionalPropertyName: nameof(IsHighlighted), foreColorPropertyName: nameof(HighlightColor))]
///     public string DisplayText { get; set; }
/// }
///
/// // Create an array of DataRow instances
/// var dataRows = new[]
/// {
///     new DataRow { Id = 1, Name = "Row 1", IsHighlighted = true, HighlightColor = System.Drawing.Color.Red, DisplayText = "Highlighted" },
///     new DataRow { Id = 2, Name = "Row 2", IsHighlighted = false, HighlightColor = System.Drawing.Color.Empty, DisplayText = "Normal" }
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
public sealed class DataGridViewCellStyleAttribute(
  string foreColor = null,
  string backColor = null,
  string format = null,
  DataGridViewContentAlignment alignment = DataGridViewContentAlignment.NotSet,
  DataGridViewTriState wrapMode = DataGridViewTriState.NotSet,
  string conditionalPropertyName = null,
  string foreColorPropertyName = null,
  string backColorPropertyName = null,
  string wrapModePropertyName = null
) : Attribute {

  private Color? ForeColor { get; } = foreColor?.ParseColor();
  private Color? BackColor { get; } = backColor?.ParseColor();
  
  private void _ApplyTo(DataGridViewCellStyle style, object data) {
    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
                  data,
                  foreColorPropertyName,
                  null,
                  null,
                  null,
                  null
                )
                ?? this.ForeColor;
    if (color != null)
      style.ForeColor = color.Value;

    color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
              data,
              backColorPropertyName,
              null,
              null,
              null,
              null
            )
            ?? this.BackColor;
    if (color != null)
      style.BackColor = color.Value;

    var localWrapMode = DataGridViewExtensions.GetPropertyValueOrDefault(
      data,
      wrapModePropertyName,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet
    );
    style.WrapMode = localWrapMode != DataGridViewTriState.NotSet ? wrapMode : localWrapMode;

    if (format != null)
      style.Format = format;

    style.Alignment = alignment;
  }

  private bool _IsEnabled(object data)
    => DataGridViewExtensions.GetPropertyValueOrDefault(data, conditionalPropertyName, true, true, false, false);

  internal static void OnCellFormatting(IEnumerable<DataGridViewCellStyleAttribute> @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(e.CellStyle, data);
  }
}
