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

using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class ToolTipExtensions {
  /// <summary>
  ///   Sets the tooltips on each child control of the base control.
  /// </summary>
  /// <param name="this">This ToolTip.</param>
  /// <param name="baseControl">The base control.</param>
  /// <param name="toolTipText">The tool tip text.</param>
  public static void SetToolTips(this ToolTip @this, Control baseControl, string toolTipText) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(baseControl);

    @this.SetToolTip(baseControl, toolTipText);
    foreach (var c in baseControl.Controls.Cast<Control>().Where(c => c != null))
      @this.SetToolTip(c, toolTipText);
  }
}
