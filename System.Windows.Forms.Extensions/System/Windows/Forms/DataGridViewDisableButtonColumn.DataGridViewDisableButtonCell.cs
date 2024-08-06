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

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms;

public partial class DataGridViewDisableButtonColumn {
  /// <summary>
  ///   The cell template to use for drawing the cells' content.
  /// </summary>
  internal sealed class DataGridViewDisableButtonCell : DataGridViewButtonCell {

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="DataGridViewDisableButtonCell" /> is enabled.
    /// </summary>
    /// <value>
    ///   <c>true</c> if enabled; otherwise, <c>false</c>.
    /// </value>
    public bool Enabled { get; set; } = true; // By default, enable the button cell.

    // Override the Clone method so that the Enabled property is copied.
    public override object Clone() {
      var cell = (DataGridViewDisableButtonCell)base.Clone();
      cell.Enabled = this.Enabled;
      return cell;
    }
    
    protected override void Paint(
      Graphics graphics,
      Rectangle clipBounds,
      Rectangle cellBounds,
      int rowIndex,
      DataGridViewElementStates elementState,
      object value,
      object formattedValue,
      string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts
    ) {
      // If button cell is enabled, let the base class draw everything.
      var isEnabled = this.Enabled;

      // The button cell is disabled, so paint the border,
      // background, and disabled button for the cell.

      // Draw the cell background, if specified.
      var backColor = cellStyle.BackColor;
      if (backColor == Color.Empty)
        backColor = SystemColors.Control;

      if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
        using (var cellBackground = new SolidBrush(backColor))
          graphics.FillRectangle(cellBackground, cellBounds);

      // Draw the cell borders, if specified.
      if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      // Calculate the area in which to draw the button.
      var buttonArea = cellBounds;
      var buttonAdjustment = this.BorderWidths(advancedBorderStyle);
      buttonArea.X += buttonAdjustment.X;
      buttonArea.Y += buttonAdjustment.Y;
      buttonArea.Height -= buttonAdjustment.Height;
      buttonArea.Width -= buttonAdjustment.Width;

      // Draw the button
      if (isEnabled) {
        static int Clamp(int a) => a < byte.MinValue ? byte.MinValue : a > byte.MaxValue ? byte.MaxValue : a;
        const int shadingAmount = 64;

        var lighterColor = Color.FromArgb(backColor.A, Clamp(backColor.R + shadingAmount), Clamp(backColor.G + shadingAmount), Clamp(backColor.B + shadingAmount));
        var darkerColor = Color.FromArgb(backColor.A, Clamp(backColor.R - shadingAmount), Clamp(backColor.G - shadingAmount), Clamp(backColor.B - shadingAmount));

        var borderWidth = 3;
        buttonArea.Inflate(-borderWidth / 2, -borderWidth / 2);

        using (var pen = new Pen(lighterColor, borderWidth)) {
          graphics.DrawLine(pen, buttonArea.Left, buttonArea.Top, buttonArea.Right, buttonArea.Top);
          graphics.DrawLine(pen, buttonArea.Left, buttonArea.Top, buttonArea.Left, buttonArea.Bottom);
        }

        using (var pen = new Pen(darkerColor, borderWidth)) {
          graphics.DrawLine(pen, buttonArea.Right, buttonArea.Bottom, buttonArea.Left, buttonArea.Bottom);
          graphics.DrawLine(pen, buttonArea.Right, buttonArea.Bottom, buttonArea.Right, buttonArea.Top);
        }
      } else
        ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Disabled);

      // draw button text
      var s = formattedValue?.ToString() ?? string.Empty;
      Color foreColor;
      if (isEnabled) {
        foreColor = cellStyle.ForeColor;
        if (foreColor == Color.Empty)
          foreColor = SystemColors.ControlText;
      } else
        foreColor = SystemColors.GrayText;

      TextRenderer.DrawText(graphics, s, this.DataGridView.Font, buttonArea, foreColor);
    }
  }
}
