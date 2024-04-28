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

using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  [CompilerGenerated]
  // ReSharper disable once InconsistentNaming
  private sealed class __HandleCallback<TControl> where TControl : Control {
#pragma warning disable CC0074 // Make field readonly
    public Action<TControl> method;
    public ManualResetEventSlim resetEvent;
#pragma warning restore CC0074 // Make field readonly

    public void Invoke(object sender, EventArgs _) {
      var control = (TControl)sender;
      control.HandleCreated -= this.Invoke;
      try {
        this.method(control);
      } finally {
        this.resetEvent?.Set();
      }
    }
  }
}
