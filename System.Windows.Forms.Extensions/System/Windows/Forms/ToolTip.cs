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

using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class ToolTipExtensions {

  /// <summary>
  /// Sets the same tooltip text for the specified control and all of its child controls.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ToolTip"/> instance.</param>
  /// <param name="baseControl">The base control to which the tooltip text will be applied, along with its child controls.</param>
  /// <param name="toolTipText">The text to display as the tooltip.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="baseControl"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ToolTip toolTip = new ToolTip();
  /// Button button = new Button();
  /// Panel panel = new Panel();
  /// panel.Controls.Add(button);
  ///
  /// toolTip.SetToolTips(panel, "This is a tooltip for the panel and its controls.");
  /// // The tooltip "This is a tooltip for the panel and its controls." is now set for the panel and the button.
  /// </code>
  /// </example>
  public static void SetToolTips(this ToolTip @this, Control baseControl, string toolTipText) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(baseControl);

    @this.SetToolTip(baseControl, toolTipText);
    foreach (var c in baseControl.Controls.Cast<Control>().Where(c => c != null))
      @this.SetToolTip(c, toolTipText);
  }
}
