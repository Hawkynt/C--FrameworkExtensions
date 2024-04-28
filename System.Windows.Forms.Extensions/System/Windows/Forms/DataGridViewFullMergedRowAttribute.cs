#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
///   Allows an specific object to be represented as a full row header.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewFullMergedRowAttribute : Attribute {
  public DataGridViewFullMergedRowAttribute(string headingTextPropertyName, string foreColor = null,
    float textSize = -1) {
    this.HeadingTextPropertyName = headingTextPropertyName;
    this.ForeColor = foreColor?.ParseColor();
    this.TextSize = textSize < 0 ? null : textSize;
  }

  public Color? ForeColor { get; }
  public float? TextSize { get; }
  public string HeadingTextPropertyName { get; }

  public string GetHeadingText(object rowData) => DataGridViewExtensions.GetPropertyValueOrDefault(rowData,
    this.HeadingTextPropertyName, string.Empty, string.Empty, string.Empty, string.Empty);
}
