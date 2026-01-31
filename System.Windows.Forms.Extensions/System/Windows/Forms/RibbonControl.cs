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
using System.Linq;

namespace System.Windows.Forms;

/// <summary>
/// An Office-style ribbon control with tabs, groups, and various button types.
/// </summary>
/// <example>
/// <code>
/// var ribbon = new RibbonControl();
/// var homeTab = ribbon.AddTab("Home");
/// var clipboardGroup = homeTab.AddGroup("Clipboard");
/// clipboardGroup.AddButton("Paste", pasteIcon, RibbonButtonStyle.Large);
/// clipboardGroup.AddButton("Cut", cutIcon, RibbonButtonStyle.Small);
/// ribbon.ItemClicked += (s, e) => Console.WriteLine($"Clicked: {e.Item.Text}");
/// </code>
/// </example>
public class RibbonControl : ContainerControl {
  private readonly List<RibbonTab> _tabs = [];
  private int _selectedTabIndex;
  private bool _showApplicationButton = true;
  private Image _applicationButtonImage;
  private string _applicationButtonText = "File";
  private ContextMenuStrip _applicationMenu;
  private readonly RibbonQuickAccessToolbar _quickAccessToolbar;
  private bool _minimized;
  private bool _allowMinimize = true;

  private const int TabHeight = 24;
  private const int ContentHeight = 92;
  private const int ApplicationButtonWidth = 60;
  private const int QuickAccessHeight = 22;

  private Rectangle _applicationButtonRect;
  private bool _applicationButtonHover;
  private RibbonTab _hoveredTab;
  private RibbonItem _hoveredItem;

  /// <summary>
  /// Occurs when a tab is selected.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a tab is selected.")]
  public event EventHandler<RibbonTabEventArgs> TabSelected;

  /// <summary>
  /// Occurs when an item is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is clicked.")]
  public event EventHandler<RibbonItemEventArgs> ItemClicked;

  /// <summary>
  /// Occurs when the application button is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the application button is clicked.")]
  public event EventHandler ApplicationButtonClicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="RibbonControl"/> class.
  /// </summary>
  public RibbonControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this._quickAccessToolbar = new RibbonQuickAccessToolbar(this);
    this.Size = new Size(600, QuickAccessHeight + TabHeight + ContentHeight);
    this.BackColor = Color.FromArgb(245, 246, 247);
    this.Dock = DockStyle.Top;
  }

  /// <summary>
  /// Gets the collection of tabs.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IReadOnlyList<RibbonTab> Tabs => this._tabs.AsIReadOnlyList();

  /// <summary>
  /// Gets or sets the selected tab.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public RibbonTab SelectedTab {
    get => this._selectedTabIndex >= 0 && this._selectedTabIndex < this._tabs.Count ? this._tabs[this._selectedTabIndex] : null;
    set {
      var index = this._tabs.IndexOf(value);
      if (index >= 0)
        this.SelectedTabIndex = index;
    }
  }

  /// <summary>
  /// Gets or sets the selected tab index.
  /// </summary>
  [Category("Behavior")]
  [Description("The index of the selected tab.")]
  [DefaultValue(0)]
  public int SelectedTabIndex {
    get => this._selectedTabIndex;
    set {
      if (value < 0 || value >= this._tabs.Count)
        return;
      if (this._selectedTabIndex == value)
        return;

      this._selectedTabIndex = value;
      this.OnTabSelected(new RibbonTabEventArgs(this.SelectedTab));
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the application button.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the application button.")]
  [DefaultValue(true)]
  public bool ShowApplicationButton {
    get => this._showApplicationButton;
    set {
      if (this._showApplicationButton == value)
        return;
      this._showApplicationButton = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the application button image.
  /// </summary>
  [Category("Appearance")]
  [Description("The image for the application button.")]
  public Image ApplicationButtonImage {
    get => this._applicationButtonImage;
    set {
      this._applicationButtonImage = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the application button text.
  /// </summary>
  [Category("Appearance")]
  [Description("The text for the application button.")]
  [DefaultValue("File")]
  public string ApplicationButtonText {
    get => this._applicationButtonText;
    set {
      this._applicationButtonText = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the application menu.
  /// </summary>
  [Category("Behavior")]
  [Description("The context menu for the application button.")]
  public ContextMenuStrip ApplicationMenu {
    get => this._applicationMenu;
    set => this._applicationMenu = value;
  }

  /// <summary>
  /// Gets the quick access toolbar.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public RibbonQuickAccessToolbar QuickAccessToolbar => this._quickAccessToolbar;

  /// <summary>
  /// Gets or sets whether the ribbon is minimized.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the ribbon is minimized.")]
  [DefaultValue(false)]
  public bool Minimized {
    get => this._minimized;
    set {
      if (this._minimized == value)
        return;
      this._minimized = value;
      this.Height = value ? QuickAccessHeight + TabHeight : QuickAccessHeight + TabHeight + ContentHeight;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether minimization is allowed.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the ribbon can be minimized.")]
  [DefaultValue(true)]
  public bool AllowMinimize {
    get => this._allowMinimize;
    set => this._allowMinimize = value;
  }

  /// <summary>
  /// Adds a new tab.
  /// </summary>
  public RibbonTab AddTab(string text) {
    var tab = new RibbonTab(this) { Text = text };
    this._tabs.Add(tab);
    if (this._tabs.Count == 1)
      this._selectedTabIndex = 0;
    this.Invalidate();
    return tab;
  }

  /// <summary>
  /// Removes a tab.
  /// </summary>
  public void RemoveTab(RibbonTab tab) {
    var index = this._tabs.IndexOf(tab);
    if (index < 0)
      return;

    this._tabs.Remove(tab);
    if (this._selectedTabIndex >= this._tabs.Count)
      this._selectedTabIndex = Math.Max(0, this._tabs.Count - 1);
    this.Invalidate();
  }

  /// <summary>
  /// Minimizes the ribbon.
  /// </summary>
  public void Minimize() {
    if (this._allowMinimize)
      this.Minimized = true;
  }

  /// <summary>
  /// Restores the ribbon from minimized state.
  /// </summary>
  public void Restore() {
    this.Minimized = false;
  }

  /// <summary>
  /// Raises the <see cref="TabSelected"/> event.
  /// </summary>
  protected virtual void OnTabSelected(RibbonTabEventArgs e) => this.TabSelected?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemClicked"/> event.
  /// </summary>
  protected virtual void OnItemClicked(RibbonItemEventArgs e) => this.ItemClicked?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ApplicationButtonClicked"/> event.
  /// </summary>
  protected virtual void OnApplicationButtonClicked(EventArgs e) => this.ApplicationButtonClicked?.Invoke(this, e);

  internal void RaiseItemClicked(RibbonItem item) {
    this.OnItemClicked(new RibbonItemEventArgs(item));
  }

  internal void InvalidateRibbon() => this.Invalidate();

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    var bounds = this.ClientRectangle;

    // Draw background
    using (var brush = new SolidBrush(this.BackColor))
      g.FillRectangle(brush, bounds);

    // Draw quick access toolbar area
    this._DrawQuickAccessToolbar(g, bounds);

    // Draw tabs
    this._DrawTabs(g, bounds);

    // Draw content area
    if (!this._minimized)
      this._DrawContent(g, bounds);

    // Draw bottom border
    using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
      g.DrawLine(pen, 0, bounds.Height - 1, bounds.Width, bounds.Height - 1);
  }

  private void _DrawQuickAccessToolbar(Graphics g, Rectangle bounds) {
    var qatRect = new Rectangle(0, 0, bounds.Width, QuickAccessHeight);

    // Draw application button
    if (this._showApplicationButton) {
      this._applicationButtonRect = new Rectangle(0, 0, ApplicationButtonWidth, QuickAccessHeight + TabHeight);

      var bgColor = this._applicationButtonHover ? Color.FromArgb(41, 140, 225) : Color.FromArgb(25, 121, 202);
      using (var brush = new SolidBrush(bgColor))
        g.FillRectangle(brush, this._applicationButtonRect);

      var textColor = Color.White;
      if (this._applicationButtonImage != null) {
        var imgRect = new Rectangle(this._applicationButtonRect.X + (this._applicationButtonRect.Width - 16) / 2, 4, 16, 16);
        g.DrawImage(this._applicationButtonImage, imgRect);
      }

      var textRect = new Rectangle(this._applicationButtonRect.X, QuickAccessHeight, this._applicationButtonRect.Width, TabHeight);
      TextRenderer.DrawText(g, this._applicationButtonText, this.Font, textRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // Draw quick access toolbar items
    var qatX = this._showApplicationButton ? ApplicationButtonWidth + 4 : 4;
    foreach (var item in this._quickAccessToolbar.Items) {
      if (!item.Visible)
        continue;

      var itemRect = new Rectangle(qatX, 2, 20, 18);
      var isHovered = item == this._hoveredItem;

      if (isHovered) {
        using var brush = new SolidBrush(Color.FromArgb(200, 220, 240));
        g.FillRectangle(brush, itemRect);
      }

      if (item.SmallImage != null) {
        var imgRect = new Rectangle(itemRect.X + 2, itemRect.Y + 1, 16, 16);
        g.DrawImage(item.SmallImage, imgRect);
      }

      qatX += 22;
    }
  }

  private void _DrawTabs(Graphics g, Rectangle bounds) {
    var tabY = QuickAccessHeight;
    var tabX = this._showApplicationButton ? ApplicationButtonWidth : 0;

    // Draw tab background
    using (var brush = new SolidBrush(Color.FromArgb(245, 246, 247)))
      g.FillRectangle(brush, tabX, tabY, bounds.Width - tabX, TabHeight);

    foreach (var tab in this._tabs) {
      if (!tab.IsVisible)
        continue;

      var tabWidth = TextRenderer.MeasureText(tab.Text, this.Font).Width + 20;
      var tabRect = new Rectangle(tabX, tabY, tabWidth, TabHeight);
      var isSelected = tab == this.SelectedTab;
      var isHovered = tab == this._hoveredTab;

      // Draw tab background
      if (isSelected) {
        using var brush = new SolidBrush(Color.White);
        g.FillRectangle(brush, tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height + 1);

        using var pen = new Pen(Color.FromArgb(200, 200, 200));
        g.DrawLine(pen, tabRect.Left, tabRect.Top, tabRect.Left, tabRect.Bottom);
        g.DrawLine(pen, tabRect.Left, tabRect.Top, tabRect.Right, tabRect.Top);
        g.DrawLine(pen, tabRect.Right, tabRect.Top, tabRect.Right, tabRect.Bottom);
      } else if (isHovered) {
        using var brush = new SolidBrush(Color.FromArgb(232, 239, 247));
        g.FillRectangle(brush, tabRect);
      }

      // Draw contextual tab color
      if (tab.IsContextual) {
        using var brush = new SolidBrush(Color.FromArgb(50, tab.ContextualColor));
        g.FillRectangle(brush, tabRect);

        using var pen = new Pen(tab.ContextualColor, 2);
        g.DrawLine(pen, tabRect.Left, tabRect.Top + 2, tabRect.Right, tabRect.Top + 2);
      }

      // Draw tab text
      var textColor = isSelected ? Color.FromArgb(25, 121, 202) : this.ForeColor;
      TextRenderer.DrawText(g, tab.Text, this.Font, tabRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

      tab.Bounds = tabRect;
      tabX += tabWidth;
    }
  }

  private void _DrawContent(Graphics g, Rectangle bounds) {
    var contentRect = new Rectangle(0, QuickAccessHeight + TabHeight, bounds.Width, ContentHeight);

    // Draw content background
    using (var brush = new SolidBrush(Color.White))
      g.FillRectangle(brush, contentRect);

    // Draw groups for selected tab
    var tab = this.SelectedTab;
    if (tab == null)
      return;

    var groupX = 4;
    foreach (var group in tab.Groups) {
      var groupWidth = this._CalculateGroupWidth(group);
      var groupRect = new Rectangle(groupX, contentRect.Y + 2, groupWidth, contentRect.Height - 4);

      this._DrawGroup(g, group, groupRect);
      groupX += groupWidth + 6;
    }
  }

  private int _CalculateGroupWidth(RibbonGroup group) {
    var itemsWidth = 0;
    var largeButtonCount = 0;
    var smallButtonCount = 0;

    foreach (var item in group.Items) {
      if (!item.Visible)
        continue;

      if (item is RibbonButton button && button.Style == RibbonButtonStyle.Large) {
        ++largeButtonCount;
        itemsWidth += 50;
      } else {
        ++smallButtonCount;
        itemsWidth += 80;
      }
    }

    // Arrange small buttons in rows of 3
    if (smallButtonCount > 0)
      itemsWidth = largeButtonCount * 50 + ((smallButtonCount + 2) / 3) * 80;

    var labelWidth = TextRenderer.MeasureText(group.Text, this.Font).Width;
    return Math.Max(itemsWidth + 8, labelWidth + 24);
  }

  private void _DrawGroup(Graphics g, RibbonGroup group, Rectangle bounds) {
    // Draw group border
    using (var pen = new Pen(Color.FromArgb(220, 220, 220))) {
      g.DrawLine(pen, bounds.Right, bounds.Top + 4, bounds.Right, bounds.Bottom - 20);
    }

    // Draw group label
    var labelRect = new Rectangle(bounds.X, bounds.Bottom - 18, bounds.Width, 16);
    TextRenderer.DrawText(g, group.Text, this.Font, labelRect, Color.FromArgb(100, 100, 100), TextFormatFlags.HorizontalCenter);

    // Draw items
    var itemX = bounds.X + 4;
    var smallItemY = bounds.Y + 2;
    var smallItemRow = 0;

    foreach (var item in group.Items) {
      if (!item.Visible)
        continue;

      if (item is RibbonButton button && button.Style == RibbonButtonStyle.Large) {
        var itemRect = new Rectangle(itemX, bounds.Y + 2, 46, 66);
        this._DrawLargeButton(g, button, itemRect);
        itemX += 50;
        smallItemY = bounds.Y + 2;
        smallItemRow = 0;
      } else {
        var itemRect = new Rectangle(itemX, smallItemY, 76, 22);
        this._DrawSmallButton(g, item, itemRect);
        smallItemY += 22;
        ++smallItemRow;

        if (smallItemRow >= 3) {
          smallItemRow = 0;
          smallItemY = bounds.Y + 2;
          itemX += 80;
        }
      }

      item.Bounds = item is RibbonButton btn && btn.Style == RibbonButtonStyle.Large
        ? new Rectangle(itemX - 50, bounds.Y + 2, 46, 66)
        : new Rectangle(itemX, smallItemY - 22, 76, 22);
    }

    group.Bounds = bounds;
  }

  private void _DrawLargeButton(Graphics g, RibbonButton button, Rectangle bounds) {
    var isHovered = button == this._hoveredItem;
    var isPressed = button.Checked;

    // Draw background
    if (isHovered || isPressed) {
      var bgColor = isPressed ? Color.FromArgb(200, 220, 240) : Color.FromArgb(220, 235, 250);
      using var brush = new SolidBrush(bgColor);
      using var path = this._CreateRoundedRectangle(bounds, 3);
      g.FillPath(brush, path);

      using var pen = new Pen(Color.FromArgb(160, 190, 220));
      g.DrawPath(pen, path);
    }

    // Draw image
    if (button.LargeImage != null) {
      var imgRect = new Rectangle(bounds.X + (bounds.Width - 32) / 2, bounds.Y + 4, 32, 32);
      if (!button.Enabled) {
        var attributes = new System.Drawing.Imaging.ImageAttributes();
        var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.5f };
        attributes.SetColorMatrix(matrix);
        g.DrawImage(button.LargeImage, imgRect, 0, 0, button.LargeImage.Width, button.LargeImage.Height, GraphicsUnit.Pixel, attributes);
      } else
        g.DrawImage(button.LargeImage, imgRect);
    }

    // Draw text
    var textRect = new Rectangle(bounds.X, bounds.Y + 40, bounds.Width, bounds.Height - 40);
    var textColor = button.Enabled ? this.ForeColor : SystemColors.GrayText;
    TextRenderer.DrawText(g, button.Text, this.Font, textRect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
  }

  private void _DrawSmallButton(Graphics g, RibbonItem item, Rectangle bounds) {
    var isHovered = item == this._hoveredItem;
    var isChecked = item is RibbonButton btn && btn.Checked;

    // Draw background
    if (isHovered || isChecked) {
      var bgColor = isChecked ? Color.FromArgb(200, 220, 240) : Color.FromArgb(220, 235, 250);
      using var brush = new SolidBrush(bgColor);
      using var path = this._CreateRoundedRectangle(bounds, 2);
      g.FillPath(brush, path);

      using var pen = new Pen(Color.FromArgb(160, 190, 220));
      g.DrawPath(pen, path);
    }

    // Draw image
    if (item.SmallImage != null) {
      var imgRect = new Rectangle(bounds.X + 2, bounds.Y + 3, 16, 16);
      if (!item.Enabled) {
        var attributes = new System.Drawing.Imaging.ImageAttributes();
        var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.5f };
        attributes.SetColorMatrix(matrix);
        g.DrawImage(item.SmallImage, imgRect, 0, 0, item.SmallImage.Width, item.SmallImage.Height, GraphicsUnit.Pixel, attributes);
      } else
        g.DrawImage(item.SmallImage, imgRect);
    }

    // Draw text
    var textRect = new Rectangle(bounds.X + 20, bounds.Y, bounds.Width - 24, bounds.Height);
    var textColor = item.Enabled ? this.ForeColor : SystemColors.GrayText;
    TextRenderer.DrawText(g, item.Text, this.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

    // Draw dropdown arrow for split/dropdown buttons
    if (item is RibbonSplitButton || item is RibbonDropDownButton) {
      var arrowRect = new Rectangle(bounds.Right - 12, bounds.Y + bounds.Height / 2 - 2, 8, 4);
      using var pen = new Pen(textColor);
      g.DrawLine(pen, arrowRect.X, arrowRect.Y, arrowRect.X + 4, arrowRect.Bottom);
      g.DrawLine(pen, arrowRect.X + 4, arrowRect.Bottom, arrowRect.Right, arrowRect.Y);
    }
  }

  private GraphicsPath _CreateRoundedRectangle(Rectangle rect, int radius) {
    var path = new GraphicsPath();

    if (radius <= 0) {
      path.AddRectangle(rect);
      return path;
    }

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

    return path;
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    var oldHoveredItem = this._hoveredItem;
    var oldHoveredTab = this._hoveredTab;
    var oldAppButtonHover = this._applicationButtonHover;

    this._hoveredItem = null;
    this._hoveredTab = null;
    this._applicationButtonHover = false;

    // Check application button
    if (this._showApplicationButton && this._applicationButtonRect.Contains(e.Location)) {
      this._applicationButtonHover = true;
    }
    // Check tabs
    else {
      foreach (var tab in this._tabs) {
        if (tab.Bounds.Contains(e.Location)) {
          this._hoveredTab = tab;
          break;
        }
      }
    }

    // Check items in selected tab
    if (!this._minimized && this._hoveredTab == null && this.SelectedTab != null) {
      foreach (var group in this.SelectedTab.Groups) {
        foreach (var item in group.Items) {
          if (item.Visible && item.Bounds.Contains(e.Location)) {
            this._hoveredItem = item;
            break;
          }
        }
        if (this._hoveredItem != null)
          break;
      }
    }

    // Check quick access items
    if (this._hoveredItem == null) {
      foreach (var item in this._quickAccessToolbar.Items) {
        if (item.Visible && item.Bounds.Contains(e.Location)) {
          this._hoveredItem = item;
          break;
        }
      }
    }

    if (oldHoveredItem != this._hoveredItem || oldHoveredTab != this._hoveredTab || oldAppButtonHover != this._applicationButtonHover)
      this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);

    if (this._hoveredItem != null || this._hoveredTab != null || this._applicationButtonHover) {
      this._hoveredItem = null;
      this._hoveredTab = null;
      this._applicationButtonHover = false;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (e.Button != MouseButtons.Left)
      return;

    // Check application button
    if (this._showApplicationButton && this._applicationButtonRect.Contains(e.Location)) {
      this.OnApplicationButtonClicked(EventArgs.Empty);
      if (this._applicationMenu != null)
        this._applicationMenu.Show(this, new Point(this._applicationButtonRect.Left, this._applicationButtonRect.Bottom));
      return;
    }

    // Check tabs
    foreach (var tab in this._tabs) {
      if (tab.Bounds.Contains(e.Location)) {
        this.SelectedTab = tab;
        if (this._minimized)
          this.Restore();
        return;
      }
    }

    // Check items
    if (!this._minimized && this.SelectedTab != null) {
      foreach (var group in this.SelectedTab.Groups) {
        foreach (var item in group.Items) {
          if (item.Visible && item.Enabled && item.Bounds.Contains(e.Location)) {
            if (item is RibbonSplitButton splitButton) {
              splitButton.OnClick();
              if (splitButton.DropDownMenu != null) {
                var dropDownRect = new Rectangle(item.Bounds.Right - 16, item.Bounds.Top, 16, item.Bounds.Height);
                if (dropDownRect.Contains(e.Location))
                  splitButton.DropDownMenu.Show(this, new Point(item.Bounds.Left, item.Bounds.Bottom));
              }
            } else if (item is RibbonDropDownButton dropButton) {
              if (dropButton.DropDownMenu != null)
                dropButton.DropDownMenu.Show(this, new Point(item.Bounds.Left, item.Bounds.Bottom));
            } else if (item is RibbonButton button)
              button.OnClick();
            return;
          }
        }
      }
    }

    // Check quick access items
    foreach (var item in this._quickAccessToolbar.Items) {
      if (item.Visible && item.Enabled && item.Bounds.Contains(e.Location)) {
        if (item is RibbonButton button)
          button.OnClick();
        return;
      }
    }
  }

  /// <inheritdoc />
  protected override void OnMouseDoubleClick(MouseEventArgs e) {
    base.OnMouseDoubleClick(e);

    // Double-click on tab area toggles minimized state
    var tabY = QuickAccessHeight;
    var tabRect = new Rectangle(0, tabY, this.Width, TabHeight);
    if (this._allowMinimize && tabRect.Contains(e.Location)) {
      this.Minimized = !this.Minimized;
    }
  }
}

/// <summary>
/// Represents a tab in a <see cref="RibbonControl"/>.
/// </summary>
public class RibbonTab {
  private readonly RibbonControl _owner;
  private readonly List<RibbonGroup> _groups = [];

  internal RibbonTab(RibbonControl owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets or sets the tab text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets whether this is a contextual tab.
  /// </summary>
  public bool IsContextual { get; set; }

  /// <summary>
  /// Gets or sets the contextual color.
  /// </summary>
  public Color ContextualColor { get; set; } = Color.Orange;

  /// <summary>
  /// Gets or sets whether the tab is visible.
  /// </summary>
  public bool IsVisible { get; set; } = true;

  /// <summary>
  /// Gets the groups in this tab.
  /// </summary>
  public IReadOnlyList<RibbonGroup> Groups => this._groups.AsIReadOnlyList();

  internal Rectangle Bounds { get; set; }

  /// <summary>
  /// Adds a group to this tab.
  /// </summary>
  public RibbonGroup AddGroup(string text) {
    var group = new RibbonGroup(this._owner) { Text = text };
    this._groups.Add(group);
    this._owner.InvalidateRibbon();
    return group;
  }

  /// <summary>
  /// Removes a group from this tab.
  /// </summary>
  public void RemoveGroup(RibbonGroup group) {
    if (this._groups.Remove(group))
      this._owner.InvalidateRibbon();
  }
}

/// <summary>
/// Represents a group in a <see cref="RibbonTab"/>.
/// </summary>
public class RibbonGroup {
  private readonly RibbonControl _owner;
  private readonly List<RibbonItem> _items = [];

  internal RibbonGroup(RibbonControl owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets or sets the group text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets whether to show the dialog launcher.
  /// </summary>
  public bool ShowDialogLauncher { get; set; }

  /// <summary>
  /// Gets the items in this group.
  /// </summary>
  public IReadOnlyList<RibbonItem> Items => this._items.AsIReadOnlyList();

  internal Rectangle Bounds { get; set; }

  /// <summary>
  /// Occurs when the dialog launcher is clicked.
  /// </summary>
  public event EventHandler DialogLauncherClicked;

  /// <summary>
  /// Adds a button to this group.
  /// </summary>
  public RibbonButton AddButton(string text, Image image, RibbonButtonStyle style = RibbonButtonStyle.Normal) {
    var button = new RibbonButton(this._owner) {
      Text = text,
      Style = style
    };

    if (style == RibbonButtonStyle.Large)
      button.LargeImage = image;
    else
      button.SmallImage = image;

    this._items.Add(button);
    this._owner.InvalidateRibbon();
    return button;
  }

  /// <summary>
  /// Adds a split button to this group.
  /// </summary>
  public RibbonSplitButton AddSplitButton(string text, Image image) {
    var button = new RibbonSplitButton(this._owner) {
      Text = text,
      SmallImage = image
    };
    this._items.Add(button);
    this._owner.InvalidateRibbon();
    return button;
  }

  /// <summary>
  /// Adds a dropdown button to this group.
  /// </summary>
  public RibbonDropDownButton AddDropDownButton(string text, Image image) {
    var button = new RibbonDropDownButton(this._owner) {
      Text = text,
      SmallImage = image
    };
    this._items.Add(button);
    this._owner.InvalidateRibbon();
    return button;
  }

  /// <summary>
  /// Adds a separator to this group.
  /// </summary>
  public RibbonSeparator AddSeparator() {
    var separator = new RibbonSeparator();
    this._items.Add(separator);
    return separator;
  }

  internal void RaiseDialogLauncherClicked() => this.DialogLauncherClicked?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Base class for ribbon items.
/// </summary>
public abstract class RibbonItem {
  /// <summary>
  /// Gets or sets the item text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets the small image (16x16).
  /// </summary>
  public Image SmallImage { get; set; }

  /// <summary>
  /// Gets or sets the large image (32x32).
  /// </summary>
  public Image LargeImage { get; set; }

  /// <summary>
  /// Gets or sets the tooltip text.
  /// </summary>
  public string ToolTipText { get; set; }

  /// <summary>
  /// Gets or sets whether the item is enabled.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Gets or sets whether the item is visible.
  /// </summary>
  public bool Visible { get; set; } = true;

  /// <summary>
  /// Gets or sets custom data.
  /// </summary>
  public object Tag { get; set; }

  internal Rectangle Bounds { get; set; }
}

/// <summary>
/// Represents a button in a ribbon.
/// </summary>
public class RibbonButton : RibbonItem {
  private readonly RibbonControl _owner;

  internal RibbonButton(RibbonControl owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets or sets the button style.
  /// </summary>
  public RibbonButtonStyle Style { get; set; } = RibbonButtonStyle.Normal;

  /// <summary>
  /// Gets or sets whether the button is checked (for toggle buttons).
  /// </summary>
  public bool Checked { get; set; }

  /// <summary>
  /// Gets or sets the shortcut keys.
  /// </summary>
  public Keys ShortcutKeys { get; set; }

  /// <summary>
  /// Occurs when the button is clicked.
  /// </summary>
  public event EventHandler Click;

  internal void OnClick() {
    this.Click?.Invoke(this, EventArgs.Empty);
    this._owner.RaiseItemClicked(this);
  }
}

/// <summary>
/// Represents a split button in a ribbon.
/// </summary>
public class RibbonSplitButton : RibbonButton {
  internal RibbonSplitButton(RibbonControl owner) : base(owner) {
  }

  /// <summary>
  /// Gets or sets the dropdown menu.
  /// </summary>
  public ContextMenuStrip DropDownMenu { get; set; }
}

/// <summary>
/// Represents a dropdown button in a ribbon.
/// </summary>
public class RibbonDropDownButton : RibbonItem {
  private readonly RibbonControl _owner;

  internal RibbonDropDownButton(RibbonControl owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets or sets the dropdown menu.
  /// </summary>
  public ContextMenuStrip DropDownMenu { get; set; }
}

/// <summary>
/// Represents a separator in a ribbon.
/// </summary>
public class RibbonSeparator : RibbonItem {
}

/// <summary>
/// Represents the quick access toolbar in a ribbon.
/// </summary>
public class RibbonQuickAccessToolbar {
  private readonly RibbonControl _owner;
  private readonly List<RibbonItem> _items = [];

  internal RibbonQuickAccessToolbar(RibbonControl owner) {
    this._owner = owner;
  }

  /// <summary>
  /// Gets the items in the quick access toolbar.
  /// </summary>
  public IReadOnlyList<RibbonItem> Items => this._items.AsIReadOnlyList();

  /// <summary>
  /// Adds a button to the quick access toolbar.
  /// </summary>
  public RibbonButton AddButton(string text, Image smallImage) {
    var button = new RibbonButton(this._owner) {
      Text = text,
      SmallImage = smallImage
    };
    this._items.Add(button);
    this._owner.InvalidateRibbon();
    return button;
  }
}

/// <summary>
/// Specifies the style of a ribbon button.
/// </summary>
public enum RibbonButtonStyle {
  /// <summary>
  /// Normal size button.
  /// </summary>
  Normal,

  /// <summary>
  /// Large button with icon on top.
  /// </summary>
  Large,

  /// <summary>
  /// Small button.
  /// </summary>
  Small,

  /// <summary>
  /// Button with dropdown.
  /// </summary>
  DropDown,

  /// <summary>
  /// Split button with dropdown.
  /// </summary>
  SplitDropDown
}

/// <summary>
/// Provides data for ribbon tab events.
/// </summary>
public class RibbonTabEventArgs : EventArgs {
  /// <summary>
  /// Gets the tab.
  /// </summary>
  public RibbonTab Tab { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RibbonTabEventArgs"/> class.
  /// </summary>
  public RibbonTabEventArgs(RibbonTab tab) {
    this.Tab = tab;
  }
}

/// <summary>
/// Provides data for ribbon item events.
/// </summary>
public class RibbonItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the item.
  /// </summary>
  public RibbonItem Item { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RibbonItemEventArgs"/> class.
  /// </summary>
  public RibbonItemEventArgs(RibbonItem item) {
    this.Item = item;
  }
}
