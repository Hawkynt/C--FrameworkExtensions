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

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  private sealed class SuspendedLayoutToken : ISuspendedLayoutToken {
    private readonly Control _targetControl;

    public SuspendedLayoutToken(Control targetControl) {
      targetControl.SuspendLayout();
      this._targetControl = targetControl;
    }

    ~SuspendedLayoutToken() => this._Dispose();

    private void _Dispose() => this._targetControl.ResumeLayout(true);

    public void Dispose() {
      this._Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
