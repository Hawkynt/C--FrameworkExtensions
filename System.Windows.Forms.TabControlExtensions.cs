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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;

namespace System.Windows.Forms {
  internal static partial class TabControlExtensions {
    private static readonly Dictionary<TabControl, Dictionary<TabPage, Color>> _MANAGED_TABCONTROLS = new Dictionary<TabControl, Dictionary<TabPage, Color>>();

    public static void SetTabHeaderColor(this TabControl This, TabPage page, Color? color = null) {
      Contract.Requires(This != null);
      Contract.Requires(page != null);
      Dictionary<TabPage, Color> managedTabs;
      if (!_MANAGED_TABCONTROLS.TryGetValue(This, out managedTabs)) {
        _MANAGED_TABCONTROLS.Add(This, managedTabs = new Dictionary<TabPage, Color>());

        // register handler
        This.DrawItem += _HeaderPainter;

        // make sure it gets called
        This.DrawMode = TabDrawMode.OwnerDrawFixed;

        // make sure we forget about this control when its disposed
        This.Disposed += (_, __) => {

          // remove painter
          This.DrawItem -= _HeaderPainter;

          // forget the list of colored tabs
          if (_MANAGED_TABCONTROLS.ContainsKey(This))
            _MANAGED_TABCONTROLS.Remove(This);
        };
      }

      // remove the key if it already exists
      if (managedTabs.ContainsKey(page))
        managedTabs.Remove(page);

      // add it if necessary
      if (color != null)
        managedTabs.Add(page, color.Value);

    }

    /// <summary>
    /// Paints the header.
    /// </summary>
    /// <param name="sender">The sending tabcontrol</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
    private static void _HeaderPainter(object sender, DrawItemEventArgs e) {
      var tabControl = sender as TabControl;
      if (tabControl == null)
        return;

      var page = tabControl.TabPages[e.Index];

      var textcolor = tabControl.ForeColor;

      var isSelected = e.State == DrawItemState.Selected;

      // check if we manage this control
      Dictionary<TabPage, Color> colors;
      if (_MANAGED_TABCONTROLS.TryGetValue(tabControl, out colors)) {

        // paint the default background
        using (var brush = new SolidBrush(tabControl.BackColor))
          e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.Location, new Drawing.Size(e.Bounds.Width, e.Bounds.Height + 2)));

        // check if we know this tab's color and it's not selected
        Color color;
        if (!isSelected && colors.TryGetValue(page, out color)) {
          // tint the tab header
          using (var brush = new SolidBrush(color))
            e.Graphics.FillRectangle(
              brush, new Rectangle(e.Bounds.Location, new Drawing.Size(e.Bounds.Width, e.Bounds.Height + 2)));

          // decide on new textcolor
          textcolor = (color.R + color.B + color.G) / 3 > 127 ? Color.Black : Color.White;
        }
      }

      // get image if any
      var list = tabControl.ImageList;
      var image = list == null ? null : page.ImageIndex < 0 ? page.ImageKey == null ? null : list.Images[page.ImageKey] : list.Images[page.ImageIndex];

      // write text
      var paddedBounds = e.Bounds;
      if (image == null) {

        // this is how we draw without an image
        paddedBounds.Offset(1, isSelected ? -2 : 1);
        TextRenderer.DrawText(e.Graphics, page.Text, tabControl.Font, paddedBounds, textcolor);
      } else {

        // this is how we draw when an image is present
        var imgx = paddedBounds.Left + (isSelected ? 3 : 2);
        var imgy = paddedBounds.Top + (isSelected ? 3 : 2);
        e.Graphics.DrawImage(image, imgx, imgy);
        paddedBounds = new Rectangle(imgx + list.ImageSize.Width, imgy, paddedBounds.Width + paddedBounds.Left - imgx - list.ImageSize.Width, list.ImageSize.Height);
        TextRenderer.DrawText(e.Graphics, page.Text, tabControl.Font, paddedBounds, textcolor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

      }

    }
  }
}