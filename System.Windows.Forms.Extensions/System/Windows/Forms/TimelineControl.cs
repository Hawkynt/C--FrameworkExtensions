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
using System.Drawing.Drawing2D;

namespace System.Windows.Forms;

/// <summary>
/// Event arguments for timeline item events.
/// </summary>
public class TimelineItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the item that raised the event.
  /// </summary>
  public TimelineItem Item { get; }

  /// <summary>
  /// Gets the index of the item.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="TimelineItemEventArgs"/> class.
  /// </summary>
  public TimelineItemEventArgs(TimelineItem item, int index) {
    this.Item = item;
    this.Index = index;
  }
}

/// <summary>
/// Represents an item in a timeline.
/// </summary>
public class TimelineItem {
  /// <summary>
  /// Gets or sets the date/time.
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Gets or sets the title.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// Gets or sets the description.
  /// </summary>
  public string Description { get; set; }

  /// <summary>
  /// Gets or sets the icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets the node color.
  /// </summary>
  public Color NodeColor { get; set; } = Color.DodgerBlue;

  /// <summary>
  /// Gets or sets custom data.
  /// </summary>
  public object Tag { get; set; }

  internal Rectangle Bounds { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="TimelineItem"/> class.
  /// </summary>
  public TimelineItem() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TimelineItem"/> class.
  /// </summary>
  public TimelineItem(DateTime date, string title, string description = null) {
    this.Date = date;
    this.Title = title;
    this.Description = description;
  }
}

/// <summary>
/// Specifies the layout of the timeline.
/// </summary>
public enum TimelineLayout {
  /// <summary>Items on the left of the line.</summary>
  Left,
  /// <summary>Items on the right of the line.</summary>
  Right,
  /// <summary>Items alternate left and right.</summary>
  Alternating
}

/// <summary>
/// A vertical timeline control for events.
/// </summary>
/// <example>
/// <code>
/// var timeline = new TimelineControl();
/// timeline.AddItem(DateTime.Now, "Project Started", "Initial commit");
/// timeline.AddItem(DateTime.Now.AddDays(7), "First Release", "v1.0.0");
/// </code>
/// </example>
public class TimelineControl : Control {
  private readonly List<TimelineItem> _items = new();
  private TimelineLayout _layout = TimelineLayout.Left;
  private Color _lineColor = Color.LightGray;
  private int _nodeSize = 16;
  private int _scrollOffset;
  private TimelineItem _hoveredItem;

  /// <summary>
  /// Occurs when an item is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is clicked.")]
  public event EventHandler<TimelineItemEventArgs> ItemClicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="TimelineControl"/> class.
  /// </summary>
  public TimelineControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(300, 400);
  }

  /// <summary>
  /// Gets the items.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public TimelineItem[] Items => this._items.ToArray();

  /// <summary>
  /// Gets or sets the layout.
  /// </summary>
  [Category("Appearance")]
  [Description("The layout of the timeline.")]
  [DefaultValue(TimelineLayout.Left)]
  public new TimelineLayout Layout {
    get => this._layout;
    set {
      this._layout = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the line color.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the timeline line.")]
  public Color LineColor {
    get => this._lineColor;
    set {
      this._lineColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the node size.
  /// </summary>
  [Category("Appearance")]
  [Description("The size of the timeline nodes.")]
  [DefaultValue(16)]
  public int NodeSize {
    get => this._nodeSize;
    set {
      this._nodeSize = Math.Max(8, value);
      this.Invalidate();
    }
  }

  private bool ShouldSerializeLineColor() => this._lineColor != Color.LightGray;
  private void ResetLineColor() => this._lineColor = Color.LightGray;

  /// <summary>
  /// Adds an item.
  /// </summary>
  public TimelineItem AddItem(DateTime date, string title, string description = null) {
    var item = new TimelineItem(date, title, description);
    this._items.Add(item);
    this._items.Sort((a, b) => a.Date.CompareTo(b.Date));
    this.Invalidate();
    return item;
  }

  /// <summary>
  /// Removes an item.
  /// </summary>
  public void RemoveItem(TimelineItem item) {
    this._items.Remove(item);
    this.Invalidate();
  }

  /// <summary>
  /// Clears all items.
  /// </summary>
  public void ClearItems() {
    this._items.Clear();
    this.Invalidate();
  }

  /// <summary>
  /// Raises the <see cref="ItemClicked"/> event.
  /// </summary>
  protected virtual void OnItemClicked(TimelineItemEventArgs e) => this.ItemClicked?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    if (this._items.Count == 0)
      return;

    // For Left layout: items on left, so line is on right side
    // For Right layout: items on right, so line is on left side
    // For Alternating: line in center
    var lineX = this._layout switch {
      TimelineLayout.Left => this.Width - 50,
      TimelineLayout.Right => 50,
      _ => this.Width / 2
    };

    // Draw vertical line
    using (var linePen = new Pen(this._lineColor, 2)) {
      g.DrawLine(linePen, lineX, 0, lineX, this._items.Count * 100 + 50);
    }

    var y = 50 - this._scrollOffset; // Start lower to ensure first card is fully visible
    var isLeft = true;

    for (var i = 0; i < this._items.Count; ++i) {
      var item = this._items[i];
      var itemOnLeft = this._layout == TimelineLayout.Left || (this._layout == TimelineLayout.Alternating && isLeft);

      // Draw node
      var nodeRect = new Rectangle(lineX - this._nodeSize / 2, y - this._nodeSize / 2, this._nodeSize, this._nodeSize);
      using (var nodeBrush = new SolidBrush(item.NodeColor)) {
        g.FillEllipse(nodeBrush, nodeRect);
      }

      using (var nodePen = new Pen(ControlPaint.Dark(item.NodeColor), 2)) {
        g.DrawEllipse(nodePen, nodeRect);
      }

      // Calculate card position
      var cardWidth = itemOnLeft ? lineX - 50 : this.Width - lineX - 50;
      cardWidth = Math.Max(80, cardWidth); // Ensure minimum width
      var cardX = itemOnLeft ? lineX - cardWidth - 20 : lineX + 20;
      var cardHeight = 70;
      var cardRect = new Rectangle(cardX, y - cardHeight / 2, cardWidth, cardHeight);

      item.Bounds = cardRect;

      // Draw connector line
      using (var connectorPen = new Pen(this._lineColor)) {
        var connectorStartX = itemOnLeft ? cardX + cardWidth : cardX;
        var connectorEndX = itemOnLeft ? lineX - this._nodeSize / 2 - 2 : lineX + this._nodeSize / 2 + 2;
        g.DrawLine(connectorPen, connectorStartX, y, connectorEndX, y);
      }

      // Draw card background
      var isHovered = item == this._hoveredItem;
      var cardColor = isHovered ? SystemColors.ControlLight : SystemColors.Window;
      using (var cardPath = this._CreateRoundedRectangle(cardRect, 4)) {
        using var cardBrush = new SolidBrush(cardColor);
        g.FillPath(cardBrush, cardPath);

        using var cardPen = new Pen(isHovered ? item.NodeColor : SystemColors.ControlDark);
        g.DrawPath(cardPen, cardPath);
      }

      // Draw content
      var contentX = cardX + 8;
      var contentY = cardRect.Top + 6;
      var contentWidth = cardWidth - 16;

      // Draw date
      var dateText = item.Date.ToString("MMM dd, yyyy");
      using (var dateFont = new Font(this.Font.FontFamily, this.Font.Size * 0.8f)) {
        TextRenderer.DrawText(g, dateText, dateFont, new Rectangle(contentX, contentY, contentWidth, 14), SystemColors.GrayText, TextFormatFlags.Left);
      }

      contentY += 16;

      // Draw icon and title
      if (item.Icon != null) {
        g.DrawImage(item.Icon, contentX, contentY, 16, 16);
        contentX += 20;
        contentWidth -= 20;
      }

      using (var titleFont = new Font(this.Font, FontStyle.Bold)) {
        TextRenderer.DrawText(g, item.Title, titleFont, new Rectangle(contentX, contentY, contentWidth, 18), this.ForeColor, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
      }

      contentY += 20;

      // Draw description
      if (!string.IsNullOrEmpty(item.Description))
        TextRenderer.DrawText(g, item.Description, this.Font, new Rectangle(cardX + 8, contentY, cardWidth - 16, cardRect.Bottom - contentY - 4), SystemColors.GrayText, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak);

      y += 100;
      isLeft = !isLeft;
    }
  }

  private GraphicsPath _CreateRoundedRectangle(Rectangle rect, int radius) {
    var path = new GraphicsPath();
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

    TimelineItem newHovered = null;
    foreach (var item in this._items)
      if (item.Bounds.Contains(e.Location)) {
        newHovered = item;
        break;
      }

    if (newHovered != this._hoveredItem) {
      this._hoveredItem = newHovered;
      this.Cursor = newHovered != null ? Cursors.Hand : Cursors.Default;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    if (this._hoveredItem != null) {
      this._hoveredItem = null;
      this.Cursor = Cursors.Default;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (this._hoveredItem == null)
      return;

    var index = this._items.IndexOf(this._hoveredItem);
    this.OnItemClicked(new TimelineItemEventArgs(this._hoveredItem, index));
  }

  /// <inheritdoc />
  protected override void OnMouseWheel(MouseEventArgs e) {
    base.OnMouseWheel(e);
    this._scrollOffset = Math.Max(0, this._scrollOffset - e.Delta / 4);
    this.Invalidate();
  }
}
