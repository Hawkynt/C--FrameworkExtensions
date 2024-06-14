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
///   Allows to show an image next to the displayed text when a condition is met.
/// </summary>
/// <param name="imagePropertyName">The name of the property which returns the image to display</param>
/// <param name="conditionalPropertyName">The name of the property which defines, if the image is shown</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class SupportsConditionalImageAttribute(string imagePropertyName, string conditionalPropertyName = null) : Attribute {
  public string ImagePropertyName { get; } = imagePropertyName;
  public string ConditionalPropertyName { get; } = conditionalPropertyName;

  public Image GetImage(object row, object value) {
    if (value is null)
      return null;

    if (!DataGridViewExtensions.GetPropertyValueOrDefault(row, this.ConditionalPropertyName, false, true, true, false))
      return null;

    var image = DataGridViewExtensions.GetPropertyValueOrDefault<Image>(
      row,
      this.ImagePropertyName,
      null,
      null,
      null,
      null
    );
    return image;
  }

  public static void OnCellFormatting(
    IEnumerable<SupportsConditionalImageAttribute> @this,
    DataGridViewRow row,
    DataGridViewColumn column,
    object data,
    string columnName,
    DataGridViewCellFormattingEventArgs e
  ) {
    if (row.Cells[e.ColumnIndex] is not DataGridViewImageAndTextColumn.DataGridViewTextAndImageCell cell)
      return;

    foreach (var attribute in @this) {
      var image = attribute.GetImage(data, e.Value);
      cell.Image = image;
      if (image == null)
        continue;

      cell.TextImageRelation = TextImageRelation.ImageBeforeText;
      cell.KeepAspectRatio = true;
      cell.FixedImageWidth = 0;
      cell.FixedImageHeight = 0;
      e.FormattingApplied = true;
      break;
    }
  }
}
