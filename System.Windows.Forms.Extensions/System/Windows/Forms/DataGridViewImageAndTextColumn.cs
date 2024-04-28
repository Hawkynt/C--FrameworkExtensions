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

public class DataGridViewImageAndTextColumn : DataGridViewTextBoxColumn {
  private Image imageValue;

  public DataGridViewImageAndTextColumn() => this.CellTemplate = new DataGridViewTextAndImageCell();

  public override object Clone() {
    var c = base.Clone() as DataGridViewImageAndTextColumn;
    c.imageValue = this.imageValue;
    c.ImageSize = this.ImageSize;
    return c;
  }

  public Image Image {
    get => this.imageValue;
    set {
      if (this.Image == value)
        return;

      this.imageValue = value;
      this.ImageSize = value.Size;

      if (this.InheritedStyle == null)
        return;

      var inheritedPadding = this.InheritedStyle.Padding;
      this.DefaultCellStyle.Padding = new(this.ImageSize.Width,
        inheritedPadding.Top, inheritedPadding.Right,
        inheritedPadding.Bottom);
    }
  }

  internal Size ImageSize { get; private set; }

  public class DataGridViewTextAndImageCell : DataGridViewTextBoxCell {
    private Image _imageValue;
    private Size _imageSize;
    private bool _needsResize;

    public TextImageRelation TextImageRelation { get; set; }
    public bool KeepAspectRatio { get; set; }
    public uint FixedImageWidth { get; set; }
    public uint FixedImageHeight { get; set; }

    public override object Clone() {
      var c = base.Clone() as DataGridViewTextAndImageCell;
      c._imageValue = this._imageValue;
      c._imageSize = this._imageSize;
      return c;
    }

    public Image Image {
      get {
        if (this.OwningColumn == null || this._OwningDataGridViewImageAndTextColumn == null)
          return this._imageValue;

        return this._imageValue ?? this._OwningDataGridViewImageAndTextColumn.Image;
      }
      set {
        this._imageValue = value;
        this._needsResize = false;
        if (value == null)
          this._imageSize = Size.Empty;
        else {
          var size = value.Size;
          var fixedWidth = this.FixedImageWidth;
          var fixedHeight = this.FixedImageHeight;
          var keepAspectRatio = this.KeepAspectRatio;
          var width = size.Width;
          var height = size.Height;

          if (fixedWidth > 0) {
            if (keepAspectRatio)
              height = (int)((float)height * fixedWidth / width);
            else if (fixedHeight > 0)
              height = (int)fixedHeight;

            width = (int)fixedWidth;
          } else if (fixedHeight > 0) {
            if (keepAspectRatio)
              width = (int)((float)width * fixedHeight / height);

            height = (int)fixedHeight;
          }

          this._needsResize = width != size.Width || height != size.Height;
          this._imageSize = new(width, height);
        }

        var inheritedPadding = this.InheritedStyle.Padding;
        Padding padding;
        switch (this.TextImageRelation) {
          case TextImageRelation.ImageBeforeText:
            padding = new(0 + this._imageSize.Width, inheritedPadding.Top, inheritedPadding.Right,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.TextBeforeImage:
            padding = new(inheritedPadding.Left, inheritedPadding.Top, 0 + this._imageSize.Width,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.ImageAboveText:
            padding = new(inheritedPadding.Left, 0 + this._imageSize.Width, inheritedPadding.Right,
              inheritedPadding.Bottom);
            break;
          case TextImageRelation.TextAboveImage:
            padding = new(inheritedPadding.Left, inheritedPadding.Top, inheritedPadding.Right,
              0 + this._imageSize.Width);
            break;
          case TextImageRelation.Overlay:
          default:
            padding = inheritedPadding;
            break;
        }

        this.Style.Padding = padding;
      }
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
      DataGridViewPaintParts paintParts) {
      // Paint the base content
      base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
        value, formattedValue, errorText, cellStyle,
        advancedBorderStyle, paintParts);

      var image = this.Image;
      if (image == null)
        return;

      // Draw the image clipped to the cell.
      var container = graphics.BeginContainer();

      graphics.SetClip(cellBounds);

      var imageSize = this._imageSize;
      var imageWidth = imageSize.Width;
      var imageHeight = imageSize.Height;

      int x, y;
      switch (this.TextImageRelation) {
        case TextImageRelation.TextBeforeImage:
          x = cellBounds.Width - imageWidth - 1;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
        case TextImageRelation.ImageBeforeText:
          x = 0;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
        case TextImageRelation.ImageAboveText:
          x = (cellBounds.Width - imageWidth) / 2;
          y = 0;
          break;
        case TextImageRelation.TextAboveImage:
          x = (cellBounds.Width - imageWidth) / 2;
          y = cellBounds.Height - imageHeight - 1;
          break;
        case TextImageRelation.Overlay:
        default:
          x = (cellBounds.Width - imageWidth) / 2;
          y = (cellBounds.Height - imageHeight) / 2;
          break;
      }

      x += cellBounds.Location.X;
      y += cellBounds.Location.Y;

      if (this._needsResize)
        graphics.DrawImage(image, new(x, y, imageWidth, imageHeight), 0, 0, image.Width, image.Height,
          GraphicsUnit.Pixel);
      else
        graphics.DrawImageUnscaled(image, x, y);

      graphics.EndContainer(container);
    }

    private DataGridViewImageAndTextColumn _OwningDataGridViewImageAndTextColumn =>
      this.OwningColumn as DataGridViewImageAndTextColumn;
  }
}
