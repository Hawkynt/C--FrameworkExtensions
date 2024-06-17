﻿#region (c)2010-2042 Hawkynt

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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class TabControlExtensions {
  public static void AddImageToImageList(this TabControl @this, Image image, string key) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(image);
    Against.ArgumentIsNullOrEmpty(key);

    var imageList = @this.ImageList;
    imageList.Images.Add(key, image);
    if (!ImageAnimator.CanAnimate(image))
      return;

    var uniquePart = new Random().Next(); // NOTE: we shuffle the key to prevent users accidentially using the generated images

    var numImages = image.GetFrameCount(FrameDimension.Time);
    var createdImages = new Dictionary<string, Image>();
    for (var i = numImages - 1; i >= 0; --i) {
      image.SelectActiveFrame(FrameDimension.Time, i);
      var image2 = (Image)image.Clone();
      var subKey = KeyGenerator(key, i);
      createdImages.Add(subKey, image2);
      imageList.Images.Add(subKey, image2);
    }

    var currentlyShownImageIndex = 0;

    // start animating
    ImageAnimator.Animate(image, OnFrameChangedHandler);
    @this.Disposed += OnDisposed;

    return;

    // remove handler and dispose image on destruction of tabcontrol
    void OnDisposed(object _, EventArgs __) {
      ImageAnimator.StopAnimate(image, OnFrameChangedHandler);
      foreach (var kvp in createdImages) {
        imageList.Images.RemoveByKey(kvp.Key);
        kvp.Value.Dispose();
      }

      @this.Disposed -= OnDisposed;
    }

    void OnFrameChangedHandler(object _, EventArgs __) {
      currentlyShownImageIndex = (currentlyShownImageIndex + 1) % numImages;
      var currentKey = KeyGenerator(key, currentlyShownImageIndex);

      // refresh all tabpages using this image
      var result = @this.BeginInvoke(
        new Action(
          () => {
            var pages = @this.TabPages.Cast<TabPage>().Where(t => t.ImageKey == key || t.ImageKey.StartsWith(key + "\0"));

            foreach (var page in pages)
              page.ImageKey = currentKey;
          }
        )
      );

      @this.EndInvoke(result);
    }

    string KeyGenerator(string @base, int index) => $"{@base}\0{uniquePart}\0{index}";
  }

  #region messing with tab color

  private static readonly Dictionary<TabControl, Dictionary<TabPage, Color>> _MANAGED_TABCONTROLS = [];

  /// <summary>
  ///   Sets the color of the tab header.
  /// </summary>
  /// <param name="this">This TabControl.</param>
  /// <param name="page">The page.</param>
  /// <param name="color">The color.</param>
  public static void SetTabHeaderColor(this TabControl @this, TabPage page, Color? color = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(page);

    if (!_MANAGED_TABCONTROLS.TryGetValue(@this, out var managedTabs)) {
      _MANAGED_TABCONTROLS.Add(@this, managedTabs = []);

      // register handler
      @this.DrawItem -= _HeaderPainter; // avoid duplicate subscription
      @this.DrawItem += _HeaderPainter;

      // make sure it gets called
      @this.DrawMode = TabDrawMode.OwnerDrawFixed;

      // make sure we forget about this control when its disposed
      @this.Disposed += (_, _) => {
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
  ///   Paints the header.
  /// </summary>
  /// <param name="sender">The sending tabcontrol</param>
  /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs" /> instance containing the event data.</param>
  private static void _HeaderPainter(object sender, DrawItemEventArgs e) {
    if (sender is not TabControl tabControl)
      return;

    var page = tabControl.TabPages[e.Index];
    var textColor = tabControl.ForeColor;
    var isSelected = e.State == DrawItemState.Selected;

    // check if we manage this control
    if (_MANAGED_TABCONTROLS.TryGetValue(tabControl, out var colors)) {
      // paint the default background
      using (var brush = new SolidBrush(tabControl.BackColor))
        e.Graphics.FillRectangle(brush, new(e.Bounds.Location, new(e.Bounds.Width, e.Bounds.Height + 2)));

      // check if we know this tab's color and it's not selected
      if (!isSelected && colors.TryGetValue(page, out var color)) {
        // tint the tab header
        using (var brush = new SolidBrush(color))
          e.Graphics.FillRectangle(brush, new(e.Bounds.Location, new(e.Bounds.Width, e.Bounds.Height + 2)));

        // decide on new textcolor
        textColor = (color.R + color.B + color.G) / 3 > 127 ? Color.Black : Color.White;
      }
    }

    // get image if any
    var list = tabControl.ImageList;
    var image = list == null ? null :
      page.ImageIndex < 0 ? page.ImageKey == null ? null : list.Images[page.ImageKey] : list.Images[page.ImageIndex];

    // write text
    var paddedBounds = e.Bounds;
    if (image == null) {
      // this is how we draw without an image
      paddedBounds.Offset(1, isSelected ? -2 : 1);
      TextRenderer.DrawText(e.Graphics, page.Text, tabControl.Font, paddedBounds, textColor);
    } else {
      // this is how we draw when an image is present
      var imgx = paddedBounds.Left + (isSelected ? 3 : 2);
      var imgy = paddedBounds.Top + (isSelected ? 3 : 2);
      e.Graphics.DrawImage(image, imgx, imgy);
      paddedBounds = new(
        imgx + list.ImageSize.Width,
        imgy,
        paddedBounds.Width + paddedBounds.Left - imgx - list.ImageSize.Width,
        list.ImageSize.Height
      );
      TextRenderer.DrawText(
        e.Graphics,
        page.Text,
        tabControl.Font,
        paddedBounds,
        textColor,
        TextFormatFlags.VerticalCenter | TextFormatFlags.Left
      );
    }
  }

  #endregion
}
