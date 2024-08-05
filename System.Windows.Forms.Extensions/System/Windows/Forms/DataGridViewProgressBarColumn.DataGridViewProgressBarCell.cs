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

namespace System.Windows.Forms;

public partial class DataGridViewProgressBarColumn {

  internal sealed class DataGridViewProgressBarCell : DataGridViewTextBoxCell {
    internal const double DEFAULT_MAXIMUM = 100;
    internal const double DEFAULT_MINIMUM = 0;

    public double Maximum { get; set; } = DEFAULT_MAXIMUM;
    public double Minimum { get; set; } = DEFAULT_MINIMUM;
    public override object DefaultNewRowValue => 0;

    public override object Clone() {
      var result = (DataGridViewProgressBarCell)base.Clone();
      result.Maximum = this.Maximum;
      result.Minimum = this.Minimum;
      return result;
    }

    protected override void Paint(
      Graphics graphics,
      Rectangle clipBounds,
      Rectangle cellBounds,
      int rowIndex,
      DataGridViewElementStates cellState,
      object value,
      object formattedValue,
      string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts
    ) {
      var intValue = value switch {
        int i => i,
        float f => f,
        double d => d,
        decimal dec => (double)dec,
        _ => 0d
      };

      if (intValue < this.Minimum)
        intValue = this.Minimum;

      if (intValue > this.Maximum)
        intValue = this.Maximum;

      var rate = (intValue - this.Minimum) / (this.Maximum - this.Minimum);

      if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      var borderRect = this.BorderWidths(advancedBorderStyle);
      var paintRect = new Rectangle(
        cellBounds.Left + borderRect.Left,
        cellBounds.Top + borderRect.Top,
        cellBounds.Width - borderRect.Right,
        cellBounds.Height - borderRect.Bottom
      );

      var isSelected = cellState.HasFlag(DataGridViewElementStates.Selected);
      var bkColor =
          isSelected && paintParts.HasFlag(DataGridViewPaintParts.SelectionBackground)
            ? cellStyle.SelectionBackColor
            : cellStyle.BackColor
        ;

      if (paintParts.HasFlag(DataGridViewPaintParts.Background))
        using (var backBrush = new SolidBrush(bkColor))
          graphics.FillRectangle(backBrush, paintRect);

      paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
      paintRect.Width -= cellStyle.Padding.Horizontal;
      paintRect.Height -= cellStyle.Padding.Vertical;

      if (paintParts.HasFlag(DataGridViewPaintParts.ContentForeground)) {
        if (ProgressBarRenderer.IsSupported) {
          ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);
          var barBounds = new Rectangle(
            paintRect.Left + 3,
            paintRect.Top + 3,
            paintRect.Width - 4,
            paintRect.Height - 6
          );
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);
        } else {
          graphics.FillRectangle(Brushes.White, paintRect);
          graphics.DrawRectangle(Pens.Black, paintRect);
          var barBounds = new Rectangle(
            paintRect.Left + 1,
            paintRect.Top + 1,
            paintRect.Width - 1,
            paintRect.Height - 1
          );
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          graphics.FillRectangle(Brushes.Blue, barBounds);
        }
      }

      if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex && this.DataGridView.CurrentCellAddress.Y == this.RowIndex && paintParts.HasFlag(DataGridViewPaintParts.Focus) && this.DataGridView.Focused) {
        var focusRect = paintRect;
        focusRect.Inflate(-3, -3);
        ControlPaint.DrawFocusRectangle(graphics, focusRect);
      }

      if (paintParts.HasFlag(DataGridViewPaintParts.ContentForeground)) {
        var txt = $"{Math.Round(rate * 100)}%";
        const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
        var fColor = cellStyle.ForeColor;
        paintRect.Inflate(-2, -2);
        TextRenderer.DrawText(graphics, txt, cellStyle.Font, paintRect, fColor, flags);
      }

      if (!paintParts.HasFlag(DataGridViewPaintParts.ErrorIcon) || !this.DataGridView.ShowCellErrors || string.IsNullOrEmpty(errorText))
        return;

      var iconBounds = this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
      iconBounds.Offset(cellBounds.X, cellBounds.Y);
      this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
    }
  }

}
