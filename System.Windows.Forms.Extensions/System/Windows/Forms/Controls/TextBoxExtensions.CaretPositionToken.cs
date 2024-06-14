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

namespace System.Windows.Controls;

public static partial class TextBoxExtensions {
  private sealed class CaretPositionToken : ICaretPositionToken {
    private readonly Point _point;

    public CaretPositionToken() => NativeMethods._GetCaretPos(out this._point);

    public void Dispose() {
      NativeMethods._SetCaretPos(this._point.X, this._point.Y);
      GC.SuppressFinalize(this);
    }

    ~CaretPositionToken() => this.Dispose();

    public Point Position => this._point;
  }
}
