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
/// Specifies the visual style (ForeColor, BackColor) for items in a <see cref="ListView"/>, <see cref="ListBox"/>, or <see cref="ComboBox"/>.
/// This attribute is applied at the class level and affects the entire item/row.
/// </summary>
/// <param name="foreColor">
/// The color name used for the item's foreground (text) color.
/// <remarks>Supports color names like "Red", hex values like "#FF0000", and system colors.</remarks>
/// </param>
/// <param name="backColor">
/// The color name used for the item's background color.
/// <remarks>Supports color names like "Red", hex values like "#FF0000", and system colors.</remarks>
/// </param>
/// <param name="foreColorPropertyName">
/// The name of a <see cref="Color"/> property that provides the foreground color dynamically.
/// </param>
/// <param name="backColorPropertyName">
/// The name of a <see cref="Color"/> property that provides the background color dynamically.
/// </param>
/// <param name="conditionalPropertyName">
/// The name of a <see cref="bool"/> property that determines whether this style should be applied.
/// </param>
/// <example>
/// <code>
/// // Apply red text when item is overdue, green when complete
/// [ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsOverdue))]
/// [ListItemStyle(foreColor: "Green", conditionalPropertyName: nameof(IsComplete))]
/// public class TaskItem {
///   public string Name { get; set; }
///   public bool IsOverdue { get; set; }
///   public bool IsComplete { get; set; }
/// }
///
/// // Usage with ListView
/// listView.EnableExtendedAttributes();
/// listView.DataSource = tasks;
///
/// // Usage with ListBox
/// listBox.EnableExtendedAttributes();
/// listBox.DataSource = tasks;
///
/// // Usage with ComboBox
/// comboBox.EnableExtendedAttributes();
/// comboBox.DataSource = tasks;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ListItemStyleAttribute(
  string foreColor = null,
  string backColor = null,
  string foreColorPropertyName = null,
  string backColorPropertyName = null,
  string conditionalPropertyName = null
) : Attribute {

  internal Color? ForeColor { get; } = foreColor?.ParseColor();
  internal Color? BackColor { get; } = backColor?.ParseColor();
  internal string ForeColorPropertyName { get; } = foreColorPropertyName;
  internal string BackColorPropertyName { get; } = backColorPropertyName;
  internal string ConditionalPropertyName { get; } = conditionalPropertyName;

  /// <summary>
  /// Determines whether this style attribute is enabled for the given data object.
  /// </summary>
  /// <param name="data">The data object to check.</param>
  /// <returns><see langword="true"/> if the style should be applied; otherwise, <see langword="false"/>.</returns>
  internal bool IsEnabled(object data)
    => ListControlExtensions.GetPropertyValueOrDefault(data, this.ConditionalPropertyName, true, true, false, false);

  /// <summary>
  /// Gets the foreground color for the given data object.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The foreground color, or <see langword="null"/> if not specified.</returns>
  internal Color? GetForeColor(object data)
    => ListControlExtensions.GetPropertyValueOrDefault<Color?>(data, this.ForeColorPropertyName, null, null, null, null)
       ?? this.ForeColor;

  /// <summary>
  /// Gets the background color for the given data object.
  /// </summary>
  /// <param name="data">The data object.</param>
  /// <returns>The background color, or <see langword="null"/> if not specified.</returns>
  internal Color? GetBackColor(object data)
    => ListControlExtensions.GetPropertyValueOrDefault<Color?>(data, this.BackColorPropertyName, null, null, null, null)
       ?? this.BackColor;
}
