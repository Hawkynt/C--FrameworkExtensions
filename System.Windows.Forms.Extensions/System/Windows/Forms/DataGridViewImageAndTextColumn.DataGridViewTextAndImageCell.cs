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

public partial class DataGridViewImageAndTextColumn {
  internal sealed class DataGridViewTextAndImageCell : DataGridViewTextBoxCell {
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
        var padding = this.TextImageRelation switch {
          TextImageRelation.ImageBeforeText => new(0 + this._imageSize.Width, inheritedPadding.Top, inheritedPadding.Right, inheritedPadding.Bottom),
          TextImageRelation.TextBeforeImage => new(inheritedPadding.Left, inheritedPadding.Top, 0 + this._imageSize.Width, inheritedPadding.Bottom),
          TextImageRelation.ImageAboveText => new(inheritedPadding.Left, 0 + this._imageSize.Width, inheritedPadding.Right, inheritedPadding.Bottom),
          TextImageRelation.TextAboveImage => new(inheritedPadding.Left, inheritedPadding.Top, inheritedPadding.Right, 0 + this._imageSize.Width),
          TextImageRelation.Overlay => inheritedPadding,
          _ => inheritedPadding
        };

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
      DataGridViewPaintParts paintParts
    ) {
      // Paint the base content
      base.Paint(
        graphics,
        clipBounds,
        cellBounds,
        rowIndex,
        cellState,
        value,
        formattedValue,
        errorText,
        cellStyle,
        advancedBorderStyle,
        paintParts
      );

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
        graphics.DrawImage(image, new(x, y, imageWidth, imageHeight), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
      else
        graphics.DrawImageUnscaled(image, x, y);

      graphics.EndContainer(container);
    }

    private DataGridViewImageAndTextColumn _OwningDataGridViewImageAndTextColumn
      => this.OwningColumn as DataGridViewImageAndTextColumn;
  }
}
