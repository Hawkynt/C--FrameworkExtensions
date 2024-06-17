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
///   Allows an specific object to be represented as a full row header.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewFullMergedRowAttribute(
  string headingTextPropertyName,
  string foreColor = null,
  float textSize = -1
)
  : Attribute {
  public Color? ForeColor { get; } = foreColor?.ParseColor();
  public float? TextSize { get; } = textSize < 0 ? null : textSize;
  public string HeadingTextPropertyName { get; } = headingTextPropertyName;

  public string GetHeadingText(object rowData)
    => DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.HeadingTextPropertyName, string.Empty, string.Empty, string.Empty, string.Empty);
}
