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

// see https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/stretch-a-toolstriptextbox-to-fill-the-remaining-width-of-a-toolstrip-wf?view=netframeworkdesktop-4.8
public class ToolStripSpringTextBox : ToolStripTextBox {
  public override Size GetPreferredSize(Size constrainingSize) {
    // Use the default size if the text box is on the overflow menu
    // or is on a vertical ToolStrip.
    if (this.IsOnOverflow || this.Owner.Orientation == Orientation.Vertical)
      return this.DefaultSize;

    // Declare a variable to store the total available width as
    // it is calculated, starting with the display width of the
    // owning ToolStrip.
    var width = this.Owner.DisplayRectangle.Width;

    // Subtract the width of the overflow button if it is displayed.
    if (this.Owner.OverflowButton.Visible)
      width = width - this.Owner.OverflowButton.Width - this.Owner.OverflowButton.Margin.Horizontal;

    // Declare a variable to maintain a count of ToolStripSpringTextBox
    // items currently displayed in the owning ToolStrip.
    var springBoxCount = 0;

    foreach (ToolStripItem item in this.Owner.Items) {
      // Ignore items on the overflow menu.
      if (item.IsOnOverflow)
        continue;

      if (item is ToolStripSpringTextBox) {
        // For ToolStripSpringTextBox items, increment the count and
        // subtract the margin width from the total available width.
        ++springBoxCount;
        width -= item.Margin.Horizontal;
      } else
        // For all other items, subtract the full width from the total
        // available width.
        width = width - item.Width - item.Margin.Horizontal;
    }

    // If there are multiple ToolStripSpringTextBox items in the owning
    // ToolStrip, divide the total available width between them.
    if (springBoxCount > 1)
      width /= springBoxCount;

    // If the available width is less than the default width, use the
    // default width, forcing one or more items onto the overflow menu.
    if (width < this.DefaultSize.Width)
      width = this.DefaultSize.Width;

    // Retrieve the preferred size from the base class, but change the
    // width to the calculated width.
    var size = base.GetPreferredSize(constrainingSize);
    size.Width = width;
    return size;
  }
}
