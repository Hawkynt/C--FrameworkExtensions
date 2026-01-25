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
/// Specifies column-specific colors for a property in a <see cref="ListView"/>.
/// This attribute allows per-item color deviation from the row-level <see cref="ListItemStyleAttribute"/>.
/// </summary>
/// <param name="foreColor">
/// The color name used for the column's foreground (text) color.
/// <remarks>Supports color names like "Red", hex values like "#FF0000", and system colors.</remarks>
/// </param>
/// <param name="backColor">
/// The color name used for the column's background color.
/// <remarks>Supports color names like "Red", hex values like "#FF0000", and system colors.</remarks>
/// </param>
/// <param name="foreColorPropertyName">
/// The name of a <see cref="Color"/> property that provides the foreground color dynamically.
/// </param>
/// <param name="backColorPropertyName">
/// The name of a <see cref="Color"/> property that provides the background color dynamically.
/// </param>
/// <param name="conditionalPropertyName">
/// The name of a <see cref="bool"/> property that determines whether this color should be applied.
/// </param>
/// <example>
/// <code>
/// // Row is red when overdue, but Status column can be orange when pending
/// [ListItemStyle(foreColor: "Red", conditionalPropertyName: nameof(IsOverdue))]
/// public class Task {
///   [ListViewColumn("Name")]
///   public string Name { get; set; }
///
///   [ListViewColumn("Status")]
///   [ListViewColumnColor(foreColor: "Orange", conditionalPropertyName: nameof(IsPending))]
///   public string Status { get; set; }
///
///   [ListViewColumn("Priority")]
///   [ListViewColumnColor(foreColorPropertyName: nameof(PriorityColor))]
///   public string Priority { get; set; }
///
///   public bool IsOverdue { get; set; }
///   public bool IsPending { get; set; }
///   public Color PriorityColor { get; set; }
/// }
///
/// // Usage
/// listView.EnableExtendedAttributes();
/// listView.DataSource = tasks;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ListViewColumnColorAttribute(
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
  /// Determines whether this color attribute is enabled for the given data object.
  /// </summary>
  /// <param name="data">The data object to check.</param>
  /// <returns><see langword="true"/> if the color should be applied; otherwise, <see langword="false"/>.</returns>
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
