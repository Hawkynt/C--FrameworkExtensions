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
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DataGridViewCellStyleAttribute : Attribute {
  /// <summary>
  ///   Creates a new <see cref="DataGridViewCellStyleAttribute" />.
  /// </summary>
  /// <param name="foreColor">
  ///   The color-name used for the property <see cref="DataGridViewCellStyle.ForeColor" />.
  ///   <remarks>Supports many types of color-names, such as hex values, (a)rgb values, known-colors, system-colors, etc.</remarks>
  /// </param>
  /// <param name="backColor">
  ///   The color-name used for the property <see cref="DataGridViewCellStyle.BackColor" />.
  ///   <remarks>Supports many types of color-names, such as hex values, (a)rgb values, known-colors, system-colors, etc.</remarks>
  /// </param>
  /// <param name="format">The value for the property <see cref="DataGridViewCellStyle.Format" />.</param>
  /// <param name="alignment">The value for the property <see cref="DataGridViewCellStyle.Alignment" />.</param>
  /// <param name="wrapMode">The value for the property <see cref="DataGridViewCellStyle.WrapMode" />.</param>
  /// <param name="conditionalPropertyName">
  ///   The name of the <see cref="bool" />-property, deciding if this attribute should
  ///   be enabled.
  /// </param>
  /// <param name="foreColorPropertyName">
  ///   The name of the <see cref="Color" />-property, retrieving the value for
  ///   <see cref="DataGridViewCellStyle.ForeColor" />.
  /// </param>
  /// <param name="backColorPropertyName">
  ///   The name of the <see cref="Color" />-property, retrieving the value for
  ///   <see cref="DataGridViewCellStyle.BackColor" />.
  /// </param>
  /// <param name="wrapModePropertyName">
  ///   The name of the <see cref="DataGridViewTriState" />-property, retrieving the value
  ///   for <see cref="DataGridViewCellStyle.WrapMode" />.
  /// </param>
  public DataGridViewCellStyleAttribute(
    string foreColor = null,
    string backColor = null,
    string format = null,
    DataGridViewContentAlignment alignment = DataGridViewContentAlignment.NotSet,
    DataGridViewTriState wrapMode = DataGridViewTriState.NotSet,
    string conditionalPropertyName = null,
    string foreColorPropertyName = null,
    string backColorPropertyName = null,
    string wrapModePropertyName = null
  ) {
    this.ForeColor = foreColor?.ParseColor();
    this.BackColor = backColor?.ParseColor();
    this.ConditionalPropertyName = conditionalPropertyName;
    this.Format = format;
    this.WrapMode = wrapMode;
    this.ForeColorPropertyName = foreColorPropertyName;
    this.BackColorPropertyName = backColorPropertyName;
    this.WrapModePropertyName = wrapModePropertyName;
    this.Alignment = alignment;
  }

  public string ConditionalPropertyName { get; }
  public Color? ForeColor { get; }
  public Color? BackColor { get; }
  public string Format { get; }
  public DataGridViewContentAlignment Alignment { get; }
  public DataGridViewTriState WrapMode { get; }
  public string ForeColorPropertyName { get; }
  public string BackColorPropertyName { get; }
  public string WrapModePropertyName { get; }

  private void _ApplyTo(DataGridViewCellStyle style, object data) {
    var color = DataGridViewExtensions.GetPropertyValueOrDefault<Color?>(
                  data,
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
              data,
              this.BackColorPropertyName,
              null,
              null,
              null,
              null
            )
            ?? this.BackColor;
    if (color != null)
      style.BackColor = color.Value;

    var wrapMode = DataGridViewExtensions.GetPropertyValueOrDefault(
      data,
      this.WrapModePropertyName,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet,
      DataGridViewTriState.NotSet
    );
    style.WrapMode = this.WrapMode != DataGridViewTriState.NotSet ? this.WrapMode : wrapMode;

    if (this.Format != null)
      style.Format = this.Format;

    style.Alignment = this.Alignment;
  }

  private bool _IsEnabled(object data)
    => DataGridViewExtensions.GetPropertyValueOrDefault(data, this.ConditionalPropertyName, true, true, false, false);

  public static void OnCellFormatting(IEnumerable<DataGridViewCellStyleAttribute> @this, DataGridViewRow row, DataGridViewColumn column, object data, string columnName, DataGridViewCellFormattingEventArgs e) {
    foreach (var attribute in @this)
      if (attribute._IsEnabled(data))
        attribute._ApplyTo(e.CellStyle, data);
  }
}
