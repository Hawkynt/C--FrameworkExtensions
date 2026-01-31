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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms;

/// <summary>
/// An Outlook-style collapsible navigation sidebar with groups and items.
/// </summary>
/// <example>
/// <code>
/// var navPane = new NavigationPane {
///   ExpandedWidth = 200,
///   CollapsedWidth = 48
/// };
/// var group = navPane.AddGroup("Mail", mailIcon);
/// group.AddItem("Inbox", inboxIcon);
/// group.AddItem("Sent", sentIcon);
/// navPane.ItemSelected += (s, e) => Console.WriteLine($"Selected: {e.Item.Text}");
/// </code>
/// </example>
public class NavigationPane : ContainerControl {
  private readonly List<NavigationGroup> _groups = [];
  private NavigationPaneDisplayMode _displayMode = NavigationPaneDisplayMode.Expanded;
  private int _collapsedWidth = 48;
  private int _expandedWidth = 200;
  private bool _allowCollapse = true;
  private bool _animateCollapse = true;
  private NavigationItem _selectedItem;
  private Color _selectedColor = SystemColors.Highlight;
  private Color _hoverColor = SystemColors.ControlLight;

  private Timer _animationTimer;
  private int _targetWidth;
  private const int AnimationInterval = 16;
  private const int AnimationStep = 20;

  private NavigationItem _hoveredItem;
  private NavigationGroup _hoveredGroup;
  private int _scrollOffset;
  private int _totalContentHeight;

  private Rectangle _collapseButtonRect;
  private bool _collapseButtonHover;

  /// <summary>
  /// Occurs when an item is selected.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is selected.")]
  public event EventHandler<NavigationItemEventArgs> ItemSelected;

  /// <summary>
  /// Occurs when the display mode changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the display mode changes.")]
  public event EventHandler DisplayModeChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="NavigationPane"/> class.
  /// </summary>
  public NavigationPane() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.Selectable,
      true
    );

    this.Width = this._expandedWidth;
    this.Height = 300;
    this.BackColor = SystemColors.Control;

    this._animationTimer = new Timer { Interval = AnimationInterval };
    this._animationTimer.Tick += this._OnAnimationTick;
  }

  /// <summary>
  /// Gets the collection of navigation groups.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IReadOnlyList<NavigationGroup> Groups => this._groups.AsIReadOnlyList();

  /// <summary>
  /// Gets or sets the display mode.
  /// </summary>
  [Category("Appearance")]
  [Description("The display mode of the navigation pane.")]
  [DefaultValue(NavigationPaneDisplayMode.Expanded)]
  public NavigationPaneDisplayMode DisplayMode {
    get => this._displayMode;
    set {
      if (this._displayMode == value)
        return;
      this._displayMode = value;
      this._UpdateWidth();
      this.OnDisplayModeChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the width when collapsed.
  /// </summary>
  [Category("Layout")]
  [Description("The width when collapsed.")]
  [DefaultValue(48)]
  public int CollapsedWidth {
    get => this._collapsedWidth;
    set {
      this._collapsedWidth = Math.Max(24, value);
      if (this._displayMode == NavigationPaneDisplayMode.Collapsed)
        this._UpdateWidth();
    }
  }

  /// <summary>
  /// Gets or sets the width when expanded.
  /// </summary>
  [Category("Layout")]
  [Description("The width when expanded.")]
  [DefaultValue(200)]
  public int ExpandedWidth {
    get => this._expandedWidth;
    set {
      this._expandedWidth = Math.Max(100, value);
      if (this._displayMode == NavigationPaneDisplayMode.Expanded)
        this._UpdateWidth();
    }
  }

  /// <summary>
  /// Gets or sets whether the pane can be collapsed.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the pane can be collapsed.")]
  [DefaultValue(true)]
  public bool AllowCollapse {
    get => this._allowCollapse;
    set {
      this._allowCollapse = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to animate collapse/expand transitions.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to animate collapse/expand transitions.")]
  [DefaultValue(true)]
  public bool AnimateCollapse {
    get => this._animateCollapse;
    set => this._animateCollapse = value;
  }

  /// <summary>
  /// Gets or sets the currently selected item.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public NavigationItem SelectedItem {
    get => this._selectedItem;
    set {
      if (this._selectedItem == value)
        return;

      if (this._selectedItem != null)
        this._selectedItem.IsSelected = false;

      this._selectedItem = value;

      if (this._selectedItem != null) {
        this._selectedItem.IsSelected = true;
        this.OnItemSelected(new NavigationItemEventArgs(this._selectedItem));
      }

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color for selected items.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color for selected items.")]
  public Color SelectedColor {
    get => this._selectedColor;
    set {
      if (this._selectedColor == value)
        return;
      this._selectedColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color for hovered items.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color for hovered items.")]
  public Color HoverColor {
    get => this._hoverColor;
    set {
      if (this._hoverColor == value)
        return;
      this._hoverColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeSelectedColor() => this._selectedColor != SystemColors.Highlight;
  private void ResetSelectedColor() => this._selectedColor = SystemColors.Highlight;
  private bool ShouldSerializeHoverColor() => this._hoverColor != SystemColors.ControlLight;
  private void ResetHoverColor() => this._hoverColor = SystemColors.ControlLight;

  /// <summary>
  /// Adds a new navigation group.
  /// </summary>
  public NavigationGroup AddGroup(string header, Image icon = null) {
    var group = new NavigationGroup(this) {
      Header = header,
      Icon = icon
    };
    this._groups.Add(group);
    this.Invalidate();
    return group;
  }

  /// <summary>
  /// Removes a navigation group.
  /// </summary>
  public void RemoveGroup(NavigationGroup group) {
    if (this._groups.Remove(group))
      this.Invalidate();
  }

  /// <summary>
  /// Expands the navigation pane.
  /// </summary>
  public void Expand() {
    if (this._displayMode == NavigationPaneDisplayMode.Expanded)
      return;
    this.DisplayMode = NavigationPaneDisplayMode.Expanded;
  }

  /// <summary>
  /// Collapses the navigation pane.
  /// </summary>
  public void Collapse() {
    if (this._displayMode == NavigationPaneDisplayMode.Collapsed)
      return;
    this.DisplayMode = NavigationPaneDisplayMode.Collapsed;
  }

  /// <summary>
  /// Toggles between expanded and collapsed states.
  /// </summary>
  public void Toggle() {
    if (this._displayMode == NavigationPaneDisplayMode.Expanded)
      this.Collapse();
    else
      this.Expand();
  }

  /// <summary>
  /// Raises the <see cref="ItemSelected"/> event.
  /// </summary>
  protected virtual void OnItemSelected(NavigationItemEventArgs e) => this.ItemSelected?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="DisplayModeChanged"/> event.
  /// </summary>
  protected virtual void OnDisplayModeChanged(EventArgs e) => this.DisplayModeChanged?.Invoke(this, e);

  private void _UpdateWidth() {
    var targetWidth = this._displayMode switch {
      NavigationPaneDisplayMode.Collapsed => this._collapsedWidth,
      NavigationPaneDisplayMode.Minimal => 0,
      _ => this._expandedWidth
    };

    if (this._animateCollapse && !this.DesignMode) {
      this._targetWidth = targetWidth;
      this._animationTimer.Start();
    } else {
      this.Width = targetWidth;
      this.Invalidate();
    }
  }

  private void _OnAnimationTick(object sender, EventArgs e) {
    var diff = this._targetWidth - this.Width;
    if (Math.Abs(diff) <= AnimationStep) {
      this.Width = this._targetWidth;
      this._animationTimer.Stop();
    } else
      this.Width += diff > 0 ? AnimationStep : -AnimationStep;

    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var isCollapsed = this._displayMode == NavigationPaneDisplayMode.Collapsed || this.Width <= this._collapsedWidth;

    // Draw background
    using (var brush = new SolidBrush(this.BackColor))
      g.FillRectangle(brush, bounds);

    // Draw border
    using (var pen = new Pen(SystemColors.ControlDark))
      g.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom);

    // Draw collapse button at bottom
    if (this._allowCollapse)
      this._DrawCollapseButton(g, bounds, isCollapsed);

    // Draw groups and items
    var y = 0;
    var itemHeight = 36;
    var groupHeaderHeight = 32;
    var collapseButtonHeight = this._allowCollapse ? 32 : 0;
    var availableHeight = bounds.Height - collapseButtonHeight;

    foreach (var group in this._groups) {
      if (!group.IsVisible)
        continue;

      // Draw group header
      var headerRect = new Rectangle(0, y - this._scrollOffset, bounds.Width, groupHeaderHeight);
      if (headerRect.Bottom > 0 && headerRect.Top < availableHeight)
        this._DrawGroupHeader(g, group, headerRect, isCollapsed);

      y += groupHeaderHeight;

      if (!group.IsExpanded)
        continue;

      // Draw items
      foreach (var item in group.Items) {
        if (!item.IsEnabled)
          continue;

        var itemRect = new Rectangle(0, y - this._scrollOffset, bounds.Width, itemHeight);
        if (itemRect.Bottom > 0 && itemRect.Top < availableHeight)
          this._DrawItem(g, item, itemRect, isCollapsed);

        y += itemHeight;

        // Draw sub-items
        foreach (var subItem in item.SubItems) {
          if (!subItem.IsEnabled)
            continue;

          var subItemRect = new Rectangle(isCollapsed ? 0 : 20, y - this._scrollOffset, bounds.Width - (isCollapsed ? 0 : 20), itemHeight);
          if (subItemRect.Bottom > 0 && subItemRect.Top < availableHeight)
            this._DrawItem(g, subItem, subItemRect, isCollapsed);

          y += itemHeight;
        }
      }
    }

    this._totalContentHeight = y;
  }

  private void _DrawGroupHeader(Graphics g, NavigationGroup group, Rectangle bounds, bool isCollapsed) {
    var isHovered = group == this._hoveredGroup;

    if (isHovered) {
      using var brush = new SolidBrush(this._hoverColor);
      g.FillRectangle(brush, bounds);
    }

    // Draw expand/collapse indicator
    var indicatorSize = 8;
    var indicatorX = isCollapsed ? (bounds.Width - indicatorSize) / 2 : 8;
    var indicatorY = bounds.Y + (bounds.Height - indicatorSize) / 2;

    using (var pen = new Pen(this.ForeColor, 1.5f)) {
      if (group.IsExpanded) {
        g.DrawLine(pen, indicatorX, indicatorY + 2, indicatorX + indicatorSize / 2, indicatorY + indicatorSize - 2);
        g.DrawLine(pen, indicatorX + indicatorSize / 2, indicatorY + indicatorSize - 2, indicatorX + indicatorSize, indicatorY + 2);
      } else {
        g.DrawLine(pen, indicatorX + 2, indicatorY, indicatorX + indicatorSize - 2, indicatorY + indicatorSize / 2);
        g.DrawLine(pen, indicatorX + indicatorSize - 2, indicatorY + indicatorSize / 2, indicatorX + 2, indicatorY + indicatorSize);
      }
    }

    if (isCollapsed)
      return;

    // Draw icon
    var textX = 24;
    if (group.Icon != null) {
      var iconRect = new Rectangle(textX, bounds.Y + (bounds.Height - 16) / 2, 16, 16);
      g.DrawImage(group.Icon, iconRect);
      textX += 20;
    }

    // Draw header text
    using var font = new Font(this.Font, FontStyle.Bold);
    var textRect = new Rectangle(textX, bounds.Y, bounds.Width - textX - 8, bounds.Height);
    TextRenderer.DrawText(g, group.Header, font, textRect, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
  }

  private void _DrawItem(Graphics g, NavigationItem item, Rectangle bounds, bool isCollapsed) {
    var isSelected = item == this._selectedItem;
    var isHovered = item == this._hoveredItem;

    // Draw background
    if (isSelected) {
      using var brush = new SolidBrush(this._selectedColor);
      g.FillRectangle(brush, bounds);
    } else if (isHovered) {
      using var brush = new SolidBrush(this._hoverColor);
      g.FillRectangle(brush, bounds);
    }

    var iconSize = 20;
    var iconX = isCollapsed ? (bounds.Width - iconSize) / 2 : bounds.X + 12;
    var iconY = bounds.Y + (bounds.Height - iconSize) / 2;

    // Draw icon
    if (item.Icon != null) {
      var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);
      g.DrawImage(item.Icon, iconRect);
    }

    // Draw text (only when expanded)
    if (!isCollapsed) {
      var textX = iconX + iconSize + 8;
      var textRect = new Rectangle(textX, bounds.Y, bounds.Width - textX - 40, bounds.Height);
      var textColor = isSelected ? Color.White : this.ForeColor;
      TextRenderer.DrawText(g, item.Text, this.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

      // Draw badge if present
      if (item.BadgeCount > 0) {
        var badgeText = item.BadgeCount > 99 ? "99+" : item.BadgeCount.ToString();
        var badgeSize = TextRenderer.MeasureText(badgeText, this.Font);
        var badgeWidth = Math.Max(20, badgeSize.Width + 8);
        var badgeRect = new Rectangle(bounds.Right - badgeWidth - 8, bounds.Y + (bounds.Height - 18) / 2, badgeWidth, 18);

        using (var brush = new SolidBrush(Color.Red))
          this._FillRoundedRectangle(g, brush, badgeRect, 9);

        TextRenderer.DrawText(g, badgeText, this.Font, badgeRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }
    }
  }

  private void _DrawCollapseButton(Graphics g, Rectangle bounds, bool isCollapsed) {
    var buttonHeight = 32;
    this._collapseButtonRect = new Rectangle(0, bounds.Height - buttonHeight, bounds.Width, buttonHeight);

    // Draw separator
    using (var pen = new Pen(SystemColors.ControlDark))
      g.DrawLine(pen, 0, this._collapseButtonRect.Top, bounds.Width, this._collapseButtonRect.Top);

    // Draw hover background
    if (this._collapseButtonHover) {
      using var brush = new SolidBrush(this._hoverColor);
      g.FillRectangle(brush, this._collapseButtonRect);
    }

    // Draw arrow
    var arrowSize = 10;
    var arrowX = (bounds.Width - arrowSize) / 2;
    var arrowY = this._collapseButtonRect.Y + (buttonHeight - arrowSize) / 2;

    using var arrowPen = new Pen(this.ForeColor, 2f);
    if (isCollapsed) {
      // Point right (expand)
      g.DrawLine(arrowPen, arrowX + 2, arrowY, arrowX + arrowSize - 2, arrowY + arrowSize / 2);
      g.DrawLine(arrowPen, arrowX + arrowSize - 2, arrowY + arrowSize / 2, arrowX + 2, arrowY + arrowSize);
    } else {
      // Point left (collapse)
      g.DrawLine(arrowPen, arrowX + arrowSize - 2, arrowY, arrowX + 2, arrowY + arrowSize / 2);
      g.DrawLine(arrowPen, arrowX + 2, arrowY + arrowSize / 2, arrowX + arrowSize - 2, arrowY + arrowSize);
    }
  }

  private void _FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius) {
    using var path = new GraphicsPath();
    var diameter = radius * 2;
    var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

    path.AddArc(arc, 180, 90);
    arc.X = rect.Right - diameter;
    path.AddArc(arc, 270, 90);
    arc.Y = rect.Bottom - diameter;
    path.AddArc(arc, 0, 90);
    arc.X = rect.Left;
    path.AddArc(arc, 90, 90);
    path.CloseFigure();

    g.FillPath(brush, path);
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    var oldHoveredItem = this._hoveredItem;
    var oldHoveredGroup = this._hoveredGroup;
    var oldCollapseHover = this._collapseButtonHover;

    this._hoveredItem = null;
    this._hoveredGroup = null;
    this._collapseButtonHover = false;

    if (this._allowCollapse && this._collapseButtonRect.Contains(e.Location)) {
      this._collapseButtonHover = true;
    } else {
      this._HitTest(e.Location, out this._hoveredGroup, out this._hoveredItem);
    }

    if (oldHoveredItem != this._hoveredItem || oldHoveredGroup != this._hoveredGroup || oldCollapseHover != this._collapseButtonHover)
      this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);

    if (this._hoveredItem != null || this._hoveredGroup != null || this._collapseButtonHover) {
      this._hoveredItem = null;
      this._hoveredGroup = null;
      this._collapseButtonHover = false;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (e.Button != MouseButtons.Left)
      return;

    if (this._allowCollapse && this._collapseButtonRect.Contains(e.Location)) {
      this.Toggle();
      return;
    }

    this._HitTest(e.Location, out var group, out var item);

    if (item != null && item.IsEnabled)
      this.SelectedItem = item;
    else if (group != null)
      group.IsExpanded = !group.IsExpanded;
  }

  private void _HitTest(Point location, out NavigationGroup hitGroup, out NavigationItem hitItem) {
    hitGroup = null;
    hitItem = null;

    var y = 0;
    var itemHeight = 36;
    var groupHeaderHeight = 32;
    var isCollapsed = this._displayMode == NavigationPaneDisplayMode.Collapsed || this.Width <= this._collapsedWidth;
    var collapseButtonHeight = this._allowCollapse ? 32 : 0;

    if (location.Y >= this.Height - collapseButtonHeight)
      return;

    foreach (var group in this._groups) {
      if (!group.IsVisible)
        continue;

      var headerRect = new Rectangle(0, y - this._scrollOffset, this.Width, groupHeaderHeight);
      if (headerRect.Contains(location)) {
        hitGroup = group;
        return;
      }

      y += groupHeaderHeight;

      if (!group.IsExpanded)
        continue;

      foreach (var item in group.Items) {
        if (!item.IsEnabled)
          continue;

        var itemRect = new Rectangle(0, y - this._scrollOffset, this.Width, itemHeight);
        if (itemRect.Contains(location)) {
          hitItem = item;
          return;
        }

        y += itemHeight;

        foreach (var subItem in item.SubItems) {
          if (!subItem.IsEnabled)
            continue;

          var subItemRect = new Rectangle(isCollapsed ? 0 : 20, y - this._scrollOffset, this.Width - (isCollapsed ? 0 : 20), itemHeight);
          if (subItemRect.Contains(location)) {
            hitItem = subItem;
            return;
          }

          y += itemHeight;
        }
      }
    }
  }

  /// <inheritdoc />
  protected override void OnMouseWheel(MouseEventArgs e) {
    base.OnMouseWheel(e);

    var collapseButtonHeight = this._allowCollapse ? 32 : 0;
    var maxScroll = Math.Max(0, this._totalContentHeight - (this.Height - collapseButtonHeight));
    this._scrollOffset = Math.Max(0, Math.Min(maxScroll, this._scrollOffset - e.Delta / 3));
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._animationTimer?.Stop();
      this._animationTimer?.Dispose();
      this._animationTimer = null;
    }

    base.Dispose(disposing);
  }

  internal void InvalidatePane() => this.Invalidate();
}

/// <summary>
/// Represents a group in a <see cref="NavigationPane"/>.
/// </summary>
public class NavigationGroup {
  private readonly NavigationPane _owner;
  private readonly List<NavigationItem> _items = [];
  private bool _isExpanded = true;

  internal NavigationGroup(NavigationPane owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets or sets the header text.
  /// </summary>
  public string Header { get; set; }

  /// <summary>
  /// Gets or sets the header icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets whether the group is expanded.
  /// </summary>
  public bool IsExpanded {
    get => this._isExpanded;
    set {
      if (this._isExpanded == value)
        return;
      this._isExpanded = value;
      this._owner?.InvalidatePane();
    }
  }

  /// <summary>
  /// Gets or sets whether the group is visible.
  /// </summary>
  public bool IsVisible { get; set; } = true;

  /// <summary>
  /// Gets the items in this group.
  /// </summary>
  public IReadOnlyList<NavigationItem> Items => this._items.AsIReadOnlyList();

  /// <summary>
  /// Adds an item to this group.
  /// </summary>
  public NavigationItem AddItem(string text, Image icon = null, object tag = null) {
    var item = new NavigationItem {
      Text = text,
      Icon = icon,
      Tag = tag
    };
    this._items.Add(item);
    this._owner?.InvalidatePane();
    return item;
  }

  /// <summary>
  /// Removes an item from this group.
  /// </summary>
  public void RemoveItem(NavigationItem item) {
    if (this._items.Remove(item))
      this._owner?.InvalidatePane();
  }
}

/// <summary>
/// Represents an item in a <see cref="NavigationGroup"/>.
/// </summary>
public class NavigationItem {
  private readonly List<NavigationItem> _subItems = [];

  /// <summary>
  /// Gets or sets the item text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets the item icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets the badge count.
  /// </summary>
  public int BadgeCount { get; set; }

  /// <summary>
  /// Gets or sets whether this item is selected.
  /// </summary>
  public bool IsSelected { get; set; }

  /// <summary>
  /// Gets or sets whether this item is enabled.
  /// </summary>
  public bool IsEnabled { get; set; } = true;

  /// <summary>
  /// Gets or sets custom data associated with this item.
  /// </summary>
  public object Tag { get; set; }

  /// <summary>
  /// Gets the sub-items for nested navigation.
  /// </summary>
  public IReadOnlyList<NavigationItem> SubItems => this._subItems.AsIReadOnlyList();

  /// <summary>
  /// Adds a sub-item to this item.
  /// </summary>
  public NavigationItem AddSubItem(string text, Image icon = null, object tag = null) {
    var item = new NavigationItem {
      Text = text,
      Icon = icon,
      Tag = tag
    };
    this._subItems.Add(item);
    return item;
  }
}

/// <summary>
/// Specifies the display mode for the <see cref="NavigationPane"/>.
/// </summary>
public enum NavigationPaneDisplayMode {
  /// <summary>
  /// Fully expanded with text and icons.
  /// </summary>
  Expanded,

  /// <summary>
  /// Collapsed to show only icons.
  /// </summary>
  Collapsed,

  /// <summary>
  /// Minimal mode with zero width.
  /// </summary>
  Minimal
}

/// <summary>
/// Provides data for navigation item events.
/// </summary>
public class NavigationItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the navigation item.
  /// </summary>
  public NavigationItem Item { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="NavigationItemEventArgs"/> class.
  /// </summary>
  public NavigationItemEventArgs(NavigationItem item) {
    this.Item = item;
  }
}
