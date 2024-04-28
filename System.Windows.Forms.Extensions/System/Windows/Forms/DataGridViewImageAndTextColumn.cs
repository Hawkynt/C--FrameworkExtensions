﻿#region (c)2010-2042 Hawkynt

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

public partial class DataGridViewImageAndTextColumn : DataGridViewTextBoxColumn {
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
}
