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

using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  private class SuspendedRedrawToken : ISuspendedRedrawToken {
    private readonly IntPtr _targetControl;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

    private const int WM_SETREDRAW = 11;

    public SuspendedRedrawToken(Control targetControl) 
      => SendMessage(this._targetControl = targetControl.Handle, WM_SETREDRAW, false, 0)
      ;

    ~SuspendedRedrawToken() => this._Dispose(false);

    private void _Dispose(bool isManagedDisposal) {
      SendMessage(this._targetControl, WM_SETREDRAW, true, 0);
      if (isManagedDisposal)
        GC.SuppressFinalize(this);
    }

    public void Dispose() => this._Dispose(true);
    
  }
}
