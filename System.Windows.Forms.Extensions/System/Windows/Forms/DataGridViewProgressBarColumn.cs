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
using System.Windows.Form.Extensions;

namespace System.Windows.Forms;

public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn {
  public class DataGridViewProgressBarCell : DataGridViewTextBoxCell {
    public double Maximum { get; set; } = 100;
    public double Minimum { get; set; }
    public override object DefaultNewRowValue => 0;

    public override object Clone() {
      var cell = (DataGridViewProgressBarCell)base.Clone();
      cell.Maximum = this.Maximum;
      cell.Minimum = this.Minimum;
      return cell;
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
      var paintRect = new Rectangle(cellBounds.Left + borderRect.Left, cellBounds.Top + borderRect.Top,
        cellBounds.Width - borderRect.Right, cellBounds.Height - borderRect.Bottom);

      var isSelected = cellState._FOS_HasFlag(DataGridViewElementStates.Selected);
      var bkColor =
          isSelected && paintParts._FOS_HasFlag(DataGridViewPaintParts.SelectionBackground)
            ? cellStyle.SelectionBackColor
            : cellStyle.BackColor
        ;

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.Background))
        using (var backBrush = new SolidBrush(bkColor))
          graphics.FillRectangle(backBrush, paintRect);

      paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
      paintRect.Width -= cellStyle.Padding.Horizontal;
      paintRect.Height -= cellStyle.Padding.Vertical;

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.ContentForeground)) {
        if (ProgressBarRenderer.IsSupported) {
          ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);
          var barBounds = new Rectangle(paintRect.Left + 3, paintRect.Top + 3, paintRect.Width - 4,
            paintRect.Height - 6);
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);
        } else {
          graphics.FillRectangle(Brushes.White, paintRect);
          graphics.DrawRectangle(Pens.Black, paintRect);
          var barBounds = new Rectangle(paintRect.Left + 1, paintRect.Top + 1, paintRect.Width - 1,
            paintRect.Height - 1);
          barBounds.Width = Convert.ToInt32(Math.Round(barBounds.Width * rate));
          graphics.FillRectangle(Brushes.Blue, barBounds);
        }
      }

      if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex &&
          this.DataGridView.CurrentCellAddress.Y == this.RowIndex &&
          paintParts._FOS_HasFlag(DataGridViewPaintParts.Focus) && this.DataGridView.Focused) {
        var focusRect = paintRect;
        focusRect.Inflate(-3, -3);
        ControlPaint.DrawFocusRectangle(graphics, focusRect);
      }

      if (paintParts._FOS_HasFlag(DataGridViewPaintParts.ContentForeground)) {
        var txt = $"{Math.Round(rate * 100)}%";
        const TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
        var fColor = cellStyle.ForeColor;
        paintRect.Inflate(-2, -2);
        TextRenderer.DrawText(graphics, txt, cellStyle.Font, paintRect, fColor, flags);
      }

      if (!paintParts._FOS_HasFlag(DataGridViewPaintParts.ErrorIcon) || !this.DataGridView.ShowCellErrors ||
          string.IsNullOrEmpty(errorText))
        return;

      var iconBounds = this.GetErrorIconBounds(graphics, cellStyle, rowIndex);
      iconBounds.Offset(cellBounds.X, cellBounds.Y);
      this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
    }
  }

  public DataGridViewProgressBarColumn() => this.CellTemplate = new DataGridViewProgressBarCell();

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      if (value is DataGridViewProgressBarCell)
        base.CellTemplate = value;
      else
        throw new InvalidCastException(nameof(DataGridViewProgressBarCell));
    }
  }

  public double Maximum {
    get => ((DataGridViewProgressBarCell)this.CellTemplate).Maximum;
    set {
      if (this.Maximum == value)
        return;

      ((DataGridViewProgressBarCell)this.CellTemplate).Maximum = value;

      if (this.DataGridView == null)
        return;

      var rowCount = this.DataGridView.RowCount;
      for (var i = 0; i <= rowCount - 1; ++i) {
        var r = this.DataGridView.Rows.SharedRow(i);
        ((DataGridViewProgressBarCell)r.Cells[this.Index]).Maximum = value;
      }
    }
  }

  public double Minimum {
    get => ((DataGridViewProgressBarCell)this.CellTemplate).Minimum;
    set {
      if (this.Minimum == value)
        return;

      ((DataGridViewProgressBarCell)this.CellTemplate).Minimum = value;

      if (this.DataGridView == null)
        return;

      var rowCount = this.DataGridView.RowCount;
      for (var i = 0; i <= rowCount - 1; ++i) {
        var r = this.DataGridView.Rows.SharedRow(i);
        ((DataGridViewProgressBarCell)r.Cells[this.Index]).Minimum = value;
      }
    }
  }
}
