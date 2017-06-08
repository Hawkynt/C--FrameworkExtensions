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
using System.Drawing.Imaging;
using System.Linq;

namespace System.Windows.Forms {
  internal static partial class TabControlExtensions {

    public static void AddImageToImageList(this TabControl @this, Image image, string key) {
      Contract.Requires(image != null);
      Contract.Requires(!string.IsNullOrEmpty(key));

      var imageList = @this.ImageList;
      Contract.Assert(imageList != null, "Can only work on TabControls with assigned ImageList.");

      imageList.Images.Add(key, image);
      if (!ImageAnimator.CanAnimate(image))
        return;

      var uniquePart = new Random().Next(); // NOTE: we shuffle the key to prevent users accidentially using the generated images
      Func<string, int, string> keyGenerator = (@base, index) => @base + "\0" + uniquePart + "\0" + index;

      var numImages = image.GetFrameCount(FrameDimension.Time);
      var createdImages = new Dictionary<string, Image>();
      for (var i = numImages - 1; i >= 0; --i) {
        image.SelectActiveFrame(FrameDimension.Time, i);
        var image2 = (Image)image.Clone();
        var subKey = keyGenerator(key, i);
        createdImages.Add(subKey, image2);
        imageList.Images.Add(subKey, image2);
      }

      var currentlyShownImageIndex = 0;
      EventHandler onFrameChangedHandler = (_, __) => {
        currentlyShownImageIndex = (currentlyShownImageIndex + 1) % numImages;
        var currentKey = keyGenerator(key, currentlyShownImageIndex);

        // refresh all tabpages using this image
        @this.SafelyInvoke(() => @this.TabPages.Cast<TabPage>().Where(t => t.ImageKey == key || t.ImageKey.StartsWith(key + "\0")).ForEach(tp => tp.ImageKey = currentKey));
      };

      // start animating
      ImageAnimator.Animate(image, onFrameChangedHandler);

      // remove handler and dispose image on destruction of tabcontrol
      @this.Disposed += (_, __) => {
        ImageAnimator.StopAnimate(image, onFrameChangedHandler);
        foreach (var kvp in createdImages) {
          imageList.Images.RemoveByKey(kvp.Key);
          kvp.Value.Dispose();
        }
      };
    }

    #region messing with tab color

    private static readonly Dictionary<TabControl, Dictionary<TabPage, Color>> _MANAGED_TABCONTROLS = new Dictionary<TabControl, Dictionary<TabPage, Color>>();

    /// <summary>
    /// Sets the color of the tab header.
    /// </summary>
    /// <param name="this">This TabControl.</param>
    /// <param name="page">The page.</param>
    /// <param name="color">The color.</param>
    public static void SetTabHeaderColor(this TabControl @this, TabPage page, Color? color = null) {
      Contract.Requires(@this != null);
      Contract.Requires(page != null);
      Dictionary<TabPage, Color> managedTabs;
      if (!_MANAGED_TABCONTROLS.TryGetValue(@this, out managedTabs)) {
        _MANAGED_TABCONTROLS.Add(@this, managedTabs = new Dictionary<TabPage, Color>());

        // register handler
        @this.DrawItem -= _HeaderPainter; // avoid duplicate subscription
        @this.DrawItem += _HeaderPainter;

        // make sure it gets called
        @this.DrawMode = TabDrawMode.OwnerDrawFixed;

        // make sure we forget about this control when its disposed
        @this.Disposed += (_, __) => {

          // remove painter
          @this.DrawItem -= _HeaderPainter;

          // forget the list of colored tabs
          if (_MANAGED_TABCONTROLS.ContainsKey(@this))
            _MANAGED_TABCONTROLS.Remove(@this);
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
          e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.Location, new Size(e.Bounds.Width, e.Bounds.Height + 2)));

        // check if we know this tab's color and it's not selected
        Color color;
        if (!isSelected && colors.TryGetValue(page, out color)) {
          // tint the tab header
          using (var brush = new SolidBrush(color))
            e.Graphics.FillRectangle(brush, new Rectangle(e.Bounds.Location, new Size(e.Bounds.Width, e.Bounds.Height + 2)));

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

    #endregion

  }
}