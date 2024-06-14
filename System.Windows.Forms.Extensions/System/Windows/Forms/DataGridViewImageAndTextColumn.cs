#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

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
      this.DefaultCellStyle.Padding = new(
        this.ImageSize.Width,
        inheritedPadding.Top,
        inheritedPadding.Right,
        inheritedPadding.Bottom
      );
    }
  }

  internal Size ImageSize { get; private set; }
}
