#region (c)2010-2020 Hawkynt
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
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Windows.Forms {
  internal static partial class ToolTipExtensions {

    /// <summary>
    /// Sets the tooltips on each child control of the base control.
    /// </summary>
    /// <param name="This">This ToolTip.</param>
    /// <param name="baseControl">The base control.</param>
    /// <param name="toolTipText">The tool tip text.</param>
    public static void SetToolTips(this ToolTip This, Control baseControl, string toolTipText) {
      Contract.Requires(This != null);
      Contract.Requires(baseControl != null);
      This.SetToolTip(baseControl, toolTipText);
      foreach (var c in baseControl.Controls.Cast<Control>().Where(c => c != null))
        This.SetToolTip(c, toolTipText);
    }
  }
}