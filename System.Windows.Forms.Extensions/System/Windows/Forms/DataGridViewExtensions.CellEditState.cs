﻿#region (c)2010-2042 Hawkynt

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

public static partial class DataGridViewExtensions {
  private sealed class CellEditState {
    private readonly Color _foreColor;
    private readonly Color _backColor;

    private CellEditState(Color foreColor, Color backColor) {
      this._foreColor = foreColor;
      this._backColor = backColor;
    }

    public static CellEditState FromCell(DataGridViewCell cell) {
      var style = cell.Style;
      return new(style.ForeColor, style.BackColor);
    }

    public void ToCell(DataGridViewCell cell) {
      var style = cell.Style;
      style.ForeColor = this._foreColor;
      style.BackColor = this._backColor;
    }
  }
}
