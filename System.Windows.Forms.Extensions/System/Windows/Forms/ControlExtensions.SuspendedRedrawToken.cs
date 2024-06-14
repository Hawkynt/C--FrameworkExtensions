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

using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  private sealed class SuspendedRedrawToken : ISuspendedRedrawToken {
    private readonly IntPtr _targetControl;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

    private const int WM_SETREDRAW = 11;

    public SuspendedRedrawToken(Control targetControl)
      => SendMessage(this._targetControl = targetControl.Handle, WM_SETREDRAW, false, 0);

    ~SuspendedRedrawToken() => this._Dispose();

    private void _Dispose() => SendMessage(this._targetControl, WM_SETREDRAW, true, 0);

    public void Dispose() {
      this._Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
