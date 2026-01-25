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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Event arguments for breadcrumb item events.
/// </summary>
public class BreadcrumbItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the item that raised the event.
  /// </summary>
  public BreadcrumbItem Item { get; }

  /// <summary>
  /// Gets the index of the item.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BreadcrumbItemEventArgs"/> class.
  /// </summary>
  public BreadcrumbItemEventArgs(BreadcrumbItem item, int index) {
    this.Item = item;
    this.Index = index;
  }
}

/// <summary>
/// Represents an item in a breadcrumb control.
/// </summary>
public class BreadcrumbItem {
  /// <summary>
  /// Gets or sets the text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets the icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets custom data.
  /// </summary>
  public object Tag { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BreadcrumbItem"/> class.
  /// </summary>
  public BreadcrumbItem() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BreadcrumbItem"/> class.
  /// </summary>
  public BreadcrumbItem(string text, object tag = null) {
    this.Text = text;
    this.Tag = tag;
  }
}

/// <summary>
/// Specifies how to handle breadcrumb overflow.
/// </summary>
public enum BreadcrumbOverflowMode {
  /// <summary>Shows ellipsis for hidden items.</summary>
  Ellipsis,
  /// <summary>Allows horizontal scrolling.</summary>
  Scroll,
  /// <summary>Wraps to multiple lines.</summary>
  Wrap
}

/// <summary>
/// A navigation breadcrumb trail control.
/// </summary>
/// <example>
/// <code>
/// var breadcrumb = new BreadcrumbControl();
/// breadcrumb.Push("Home");
/// breadcrumb.Push("Documents");
/// breadcrumb.Push("Reports");
/// breadcrumb.ItemClicked += (s, e) => NavigateTo(e.Index);
/// </code>
/// </example>
public class BreadcrumbControl : Control {
  private readonly List<BreadcrumbItem> _items = new();
  private string _separator = " > ";
  private bool _clickableItems = true;
  private BreadcrumbOverflowMode _overflowMode = BreadcrumbOverflowMode.Ellipsis;
  private int _hoveredIndex = -1;
  private readonly List<Rectangle> _itemRects = new();

  /// <summary>
  /// Occurs when an item is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is clicked.")]
  public event EventHandler<BreadcrumbItemEventArgs> ItemClicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="BreadcrumbControl"/> class.
  /// </summary>
  public BreadcrumbControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(300, 24);
  }

  /// <summary>
  /// Gets the items.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public BreadcrumbItem[] Items => this._items.ToArray();

  /// <summary>
  /// Gets or sets the separator string.
  /// </summary>
  [Category("Appearance")]
  [Description("The separator between items.")]
  [DefaultValue(" > ")]
  public string Separator {
    get => this._separator;
    set {
      this._separator = value ?? " > ";
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether items are clickable.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether items are clickable.")]
  [DefaultValue(true)]
  public bool ClickableItems {
    get => this._clickableItems;
    set {
      this._clickableItems = value;
      this.Cursor = value ? Cursors.Hand : Cursors.Default;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the overflow mode.
  /// </summary>
  [Category("Behavior")]
  [Description("How to handle overflow.")]
  [DefaultValue(BreadcrumbOverflowMode.Ellipsis)]
  public BreadcrumbOverflowMode OverflowMode {
    get => this._overflowMode;
    set {
      this._overflowMode = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Pushes a new item onto the breadcrumb trail.
  /// </summary>
  public void Push(string text, object tag = null) {
    this._items.Add(new BreadcrumbItem(text, tag));
    this.Invalidate();
  }

  /// <summary>
  /// Removes the last item.
  /// </summary>
  public void Pop() {
    if (this._items.Count > 0)
      this._items.RemoveAt(this._items.Count - 1);
    this.Invalidate();
  }

  /// <summary>
  /// Navigates to a specific index, removing all items after it.
  /// </summary>
  public void NavigateTo(int index) {
    if (index < 0 || index >= this._items.Count)
      return;

    while (this._items.Count > index + 1)
      this._items.RemoveAt(this._items.Count - 1);

    this.Invalidate();
  }

  /// <summary>
  /// Clears all items.
  /// </summary>
  public void Clear() {
    this._items.Clear();
    this.Invalidate();
  }

  /// <summary>
  /// Raises the <see cref="ItemClicked"/> event.
  /// </summary>
  protected virtual void OnItemClicked(BreadcrumbItemEventArgs e) => this.ItemClicked?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    this._itemRects.Clear();

    if (this._items.Count == 0)
      return;

    var x = 2;
    var y = (this.Height - this.Font.Height) / 2;
    var separatorSize = TextRenderer.MeasureText(this._separator, this.Font);

    for (var i = 0; i < this._items.Count; ++i) {
      var item = this._items[i];

      // Draw icon if present
      var itemX = x;
      if (item.Icon != null) {
        g.DrawImage(item.Icon, x, (this.Height - 16) / 2, 16, 16);
        x += 20;
      }

      // Draw text
      var textSize = TextRenderer.MeasureText(item.Text, this.Font);
      var textColor = this._clickableItems && i == this._hoveredIndex
        ? SystemColors.HotTrack
        : this.ForeColor;

      var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
      TextRenderer.DrawText(g, item.Text, this.Font, new Rectangle(x, 0, textSize.Width, this.Height), textColor, flags);

      // Underline if hovered and clickable
      if (this._clickableItems && i == this._hoveredIndex)
        using (var pen = new Pen(SystemColors.HotTrack)) {
          g.DrawLine(pen, x, y + this.Font.Height, x + textSize.Width - 4, y + this.Font.Height);
        }

      var itemRect = new Rectangle(itemX, 0, x + textSize.Width - itemX, this.Height);
      this._itemRects.Add(itemRect);

      x += textSize.Width;

      // Draw separator
      if (i >= this._items.Count - 1)
        continue;

      TextRenderer.DrawText(g, this._separator, this.Font, new Rectangle(x, 0, separatorSize.Width, this.Height),
        SystemColors.GrayText, flags);
      x += separatorSize.Width;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (!this._clickableItems)
      return;

    var newHovered = -1;
    for (var i = 0; i < this._itemRects.Count; ++i)
      if (this._itemRects[i].Contains(e.Location)) {
        newHovered = i;
        break;
      }

    if (newHovered != this._hoveredIndex) {
      this._hoveredIndex = newHovered;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    if (this._hoveredIndex != -1) {
      this._hoveredIndex = -1;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (!this._clickableItems || this._hoveredIndex < 0)
      return;

    var item = this._items[this._hoveredIndex];
    this.OnItemClicked(new BreadcrumbItemEventArgs(item, this._hoveredIndex));
  }
}
