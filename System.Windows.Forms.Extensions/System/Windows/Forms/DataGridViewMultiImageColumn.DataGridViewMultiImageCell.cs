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

using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms;

public partial class DataGridViewMultiImageColumn {
  internal class DataGridViewMultiImageCell : DataGridViewTextBoxCell {
    private readonly List<CellImage> _images = new();
    private Size? _oldCellBounds;

    private static readonly ToolTip tooltip = new() { Active = true, ShowAlways = true };
    private bool ShowCellToolTipCacheValue;

    public int ImageSize { get; set; }
    public Padding Margin { get; set; }
    public Padding Padding { get; set; }

    #region Overrides of DataGridViewCell

    protected override void OnMouseMove(DataGridViewCellMouseEventArgs e) {
      var text = string.Empty;

      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        image.IsHovered = image.Bounds.Contains(e.Location);
        this._images[i] = image;

        if (!image.Bounds.Contains(e.Location))
          continue;

        text = ((DataGridViewMultiImageColumn)this.OwningColumn)._tooltipTextProvider?.Invoke(this.DataGridView.Rows[e.RowIndex].DataBoundItem, i) ?? string.Empty;
      }

      this.DataGridView.InvalidateCell(this);

      this.ShowCellToolTipCacheValue = this.DataGridView.ShowCellToolTips;
      this.DataGridView.ShowCellToolTips = false;

      if (tooltip.Tag != null && (string)tooltip.Tag == text) {
        this.DataGridView.ShowCellToolTips = this.ShowCellToolTipCacheValue;
        return;
      }

      var cellBounds = this.DataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

      tooltip.Tag = text;
      tooltip.Show(text, this.DataGridView, e.Location.X + cellBounds.X + this.ImageSize, e.Location.Y + cellBounds.Y);

      this.DataGridView.ShowCellToolTips = this.ShowCellToolTipCacheValue;
    }

    protected override void OnMouseLeave(int rowIndex) {
      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        image.IsHovered = false;
        this._images[i] = image;
      }

      tooltip.Hide(this.DataGridView);
      this.DataGridView.InvalidateCell(this);
    }

    protected override void OnMouseClick(DataGridViewCellMouseEventArgs e) {
      tooltip.UseAnimation = false;
      tooltip.Hide(this.DataGridView);
      tooltip.UseAnimation = true;

      for (var i = 0; i < this._images.Count; ++i) {
        var image = this._images[i];

        if (!image.Bounds.Contains(e.Location))
          continue;

        ((DataGridViewMultiImageColumn)this.OwningColumn)._onClickMethod?.Invoke(this.DataGridView.Rows[e.RowIndex].DataBoundItem, i);
      }
    }

    public override object Clone() {
      var cell = (DataGridViewMultiImageCell)base.Clone();
      cell.ImageSize = this.ImageSize;
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
      if (paintParts.HasFlag(DataGridViewPaintParts.Border))
        this.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);

      var borderRect = this.BorderWidths(advancedBorderStyle);
      var paintRect = new Rectangle(
        cellBounds.Left + borderRect.Left,
        cellBounds.Top + borderRect.Top,
        cellBounds.Width - borderRect.Right,
        cellBounds.Height - borderRect.Bottom
      );

      var isSelected = cellState.HasFlag(DataGridViewElementStates.Selected);
      var bkColor = isSelected && paintParts.HasFlag(DataGridViewPaintParts.SelectionBackground)
          ? cellStyle.SelectionBackColor
          : cellStyle.BackColor
        ;

      if (paintParts.HasFlag(DataGridViewPaintParts.Background))
        using (var backBrush = new SolidBrush(bkColor))
          graphics.FillRectangle(backBrush, paintRect);

      paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
      paintRect.Width -= cellStyle.Padding.Horizontal;
      paintRect.Height -= cellStyle.Padding.Vertical;

      var images = value == null ? [] : (Image[])value;
      var count = images.Length;

      if (!this._oldCellBounds.HasValue || !this._oldCellBounds.Equals(paintRect.Size) || this._images.Count != count) {
        this._oldCellBounds = paintRect.Size;
        this._RecreateDrawingPanel(paintRect, count);
      }

      for (var i = 0; i < this._images.Count; ++i) {
        var imageRect = this._images[i];

        if (imageRect.IsHovered)
          using (var hoverBrush = new SolidBrush(isSelected ? cellStyle.BackColor : cellStyle.SelectionBackColor))
            graphics.FillRectangle(
              hoverBrush,
              imageRect.Bounds.X + paintRect.X,
              imageRect.Bounds.Y + paintRect.Y,
              imageRect.Bounds.Size.Width,
              imageRect.Bounds.Size.Height
            );

        graphics.DrawImage(
          images[i],
          imageRect.Bounds.X + paintRect.X + this.Padding.Left,
          imageRect.Bounds.Y + paintRect.Y + this.Padding.Top,
          imageRect.Bounds.Size.Width - (this.Padding.Left + this.Padding.Right),
          imageRect.Bounds.Size.Height - (this.Padding.Top + this.Padding.Bottom)
        );
      }
    }

    #endregion

    private void _RecreateDrawingPanel(Rectangle cellBounds, int imageCount) {
      var size = this.ImageSize;
      var maxImages = cellBounds.Width / (size + this.Margin.Left + this.Margin.Right) * (cellBounds.Height / (size + this.Margin.Top + this.Margin.Bottom));

      //resizing
      while (maxImages < imageCount) {
        size -= 8;

        maxImages = cellBounds.Width / (size + this.Margin.Left + this.Margin.Right) * (cellBounds.Height / (size + this.Margin.Top + this.Margin.Bottom));
      }

      this._images.Clear();

      var x = this.Margin.Left;
      var y = this.Margin.Top;

      for (var i = 0; i < imageCount; ++i) {
        if (x + size + this.Margin.Right > cellBounds.Width) {
          x = this.Margin.Left;
          y += size + this.Margin.Bottom;
        }

        this._images.Add(new(new(x, y, size, size)));
        x += size + this.Margin.Right;
      }
    }

    private struct CellImage(Rectangle bounds) {
      public Rectangle Bounds { get; } = bounds;
      public bool IsHovered { get; set; } = false;
    }
  }
}
