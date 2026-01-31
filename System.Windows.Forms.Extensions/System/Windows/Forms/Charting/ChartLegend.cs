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

namespace System.Windows.Forms.Charting;

/// <summary>
/// Represents the legend for a chart.
/// </summary>
public class ChartLegend {
  private readonly AdvancedChart _owner;

  private ChartLegendPosition _position = ChartLegendPosition.Right;
  private bool _visible = true;
  private Font _font;
  private Color _textColor = Color.Black;
  private Color _backgroundColor = Color.Transparent;
  private Color _borderColor = Color.LightGray;
  private int _borderWidth;
  private int _padding = 8;
  private int _itemSpacing = 4;
  private int _symbolWidth = 20;
  private int _symbolHeight = 10;
  private ChartOrientation _orientation = ChartOrientation.Vertical;
  private Point _floatingPosition;
  private int _maxColumns = 1;
  private bool _interactiveHighlight = true;
  private bool _allowToggle = true;

  internal ChartLegend(AdvancedChart owner) => this._owner = owner;

  /// <summary>Gets or sets the legend position.</summary>
  [Category("Layout")]
  [Description("The position of the legend.")]
  [DefaultValue(ChartLegendPosition.Right)]
  public ChartLegendPosition Position {
    get => this._position;
    set {
      this._position = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether the legend is visible.</summary>
  [Category("Behavior")]
  [Description("Whether the legend is visible.")]
  [DefaultValue(true)]
  public bool Visible {
    get => this._visible;
    set {
      this._visible = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the font for legend text.</summary>
  [Category("Appearance")]
  [Description("The font for legend text.")]
  public Font Font {
    get => this._font;
    set {
      this._font = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the text color.</summary>
  [Category("Appearance")]
  [Description("The color of legend text.")]
  public Color TextColor {
    get => this._textColor;
    set {
      this._textColor = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the background color.</summary>
  [Category("Appearance")]
  [Description("The background color of the legend.")]
  public Color BackgroundColor {
    get => this._backgroundColor;
    set {
      this._backgroundColor = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the border color.</summary>
  [Category("Appearance")]
  [Description("The border color of the legend.")]
  public Color BorderColor {
    get => this._borderColor;
    set {
      this._borderColor = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the border width.</summary>
  [Category("Appearance")]
  [Description("The border width in pixels.")]
  [DefaultValue(0)]
  public int BorderWidth {
    get => this._borderWidth;
    set {
      this._borderWidth = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the padding inside the legend.</summary>
  [Category("Layout")]
  [Description("The padding inside the legend in pixels.")]
  [DefaultValue(8)]
  public int Padding {
    get => this._padding;
    set {
      this._padding = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the spacing between items.</summary>
  [Category("Layout")]
  [Description("The spacing between legend items in pixels.")]
  [DefaultValue(4)]
  public int ItemSpacing {
    get => this._itemSpacing;
    set {
      this._itemSpacing = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the width of the symbol/swatch.</summary>
  [Category("Appearance")]
  [Description("The width of the color symbol in pixels.")]
  [DefaultValue(20)]
  public int SymbolWidth {
    get => this._symbolWidth;
    set {
      this._symbolWidth = Math.Max(4, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the height of the symbol/swatch.</summary>
  [Category("Appearance")]
  [Description("The height of the color symbol in pixels.")]
  [DefaultValue(10)]
  public int SymbolHeight {
    get => this._symbolHeight;
    set {
      this._symbolHeight = Math.Max(4, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the legend orientation.</summary>
  [Category("Layout")]
  [Description("The orientation of legend items.")]
  [DefaultValue(ChartOrientation.Vertical)]
  public ChartOrientation Orientation {
    get => this._orientation;
    set {
      this._orientation = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the floating position when Position is Floating.</summary>
  [Category("Layout")]
  [Description("The position when using floating layout.")]
  public Point FloatingPosition {
    get => this._floatingPosition;
    set {
      this._floatingPosition = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the maximum columns in horizontal orientation.</summary>
  [Category("Layout")]
  [Description("The maximum number of columns.")]
  [DefaultValue(1)]
  public int MaxColumns {
    get => this._maxColumns;
    set {
      this._maxColumns = Math.Max(1, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether hovering over legend items highlights the series.</summary>
  [Category("Behavior")]
  [Description("Whether hovering highlights the corresponding series.")]
  [DefaultValue(true)]
  public bool InteractiveHighlight {
    get => this._interactiveHighlight;
    set => this._interactiveHighlight = value;
  }

  /// <summary>Gets or sets whether clicking toggles series visibility.</summary>
  [Category("Behavior")]
  [Description("Whether clicking toggles the series visibility.")]
  [DefaultValue(true)]
  public bool AllowToggle {
    get => this._allowToggle;
    set => this._allowToggle = value;
  }

  /// <summary>
  /// Gets the effective font for the legend.
  /// </summary>
  internal Font GetEffectiveFont() => this._font ?? this._owner?.Font ?? SystemFonts.DefaultFont;

  /// <summary>
  /// Calculates the required size for the legend.
  /// </summary>
  internal SizeF CalculateSize(Graphics g, IEnumerable<LegendItem> items) {
    if (!this._visible || this._position == ChartLegendPosition.None)
      return SizeF.Empty;

    var font = this.GetEffectiveFont();
    var maxTextWidth = 0f;
    var itemHeight = Math.Max(font.Height, this._symbolHeight);
    var itemCount = 0;

    foreach (var item in items) {
      var textSize = g.MeasureString(item.Text, font);
      maxTextWidth = Math.Max(maxTextWidth, textSize.Width);
      ++itemCount;
    }

    if (itemCount == 0)
      return SizeF.Empty;

    var singleItemWidth = this._symbolWidth + this._itemSpacing + maxTextWidth;
    var singleItemHeight = itemHeight;

    float width, height;

    if (this._orientation == ChartOrientation.Vertical) {
      width = singleItemWidth + this._padding * 2;
      height = itemCount * singleItemHeight + (itemCount - 1) * this._itemSpacing + this._padding * 2;
    } else {
      var columns = Math.Min(this._maxColumns, itemCount);
      var rows = (int)Math.Ceiling(itemCount / (double)columns);
      width = columns * singleItemWidth + (columns - 1) * this._itemSpacing + this._padding * 2;
      height = rows * singleItemHeight + (rows - 1) * this._itemSpacing + this._padding * 2;
    }

    return new SizeF(width, height);
  }

  /// <summary>
  /// Calculates the bounds for the legend based on the chart area.
  /// </summary>
  internal RectangleF CalculateBounds(RectangleF chartBounds, SizeF legendSize) {
    if (!this._visible || this._position == ChartLegendPosition.None)
      return RectangleF.Empty;

    switch (this._position) {
      case ChartLegendPosition.Right:
        return new RectangleF(
          chartBounds.Right - legendSize.Width - this._padding,
          chartBounds.Top + (chartBounds.Height - legendSize.Height) / 2,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.Left:
        return new RectangleF(
          chartBounds.Left + this._padding,
          chartBounds.Top + (chartBounds.Height - legendSize.Height) / 2,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.Top:
        return new RectangleF(
          chartBounds.Left + (chartBounds.Width - legendSize.Width) / 2,
          chartBounds.Top + this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.Bottom:
        return new RectangleF(
          chartBounds.Left + (chartBounds.Width - legendSize.Width) / 2,
          chartBounds.Bottom - legendSize.Height - this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.TopLeft:
        return new RectangleF(
          chartBounds.Left + this._padding,
          chartBounds.Top + this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.TopRight:
        return new RectangleF(
          chartBounds.Right - legendSize.Width - this._padding,
          chartBounds.Top + this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.BottomLeft:
        return new RectangleF(
          chartBounds.Left + this._padding,
          chartBounds.Bottom - legendSize.Height - this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.BottomRight:
        return new RectangleF(
          chartBounds.Right - legendSize.Width - this._padding,
          chartBounds.Bottom - legendSize.Height - this._padding,
          legendSize.Width,
          legendSize.Height
        );

      case ChartLegendPosition.Floating:
        return new RectangleF(
          this._floatingPosition.X,
          this._floatingPosition.Y,
          legendSize.Width,
          legendSize.Height
        );

      default:
        return RectangleF.Empty;
    }
  }

  /// <summary>
  /// Draws the legend.
  /// </summary>
  internal void Draw(Graphics g, RectangleF bounds, IList<LegendItem> items, int? highlightedIndex = null) {
    if (!this._visible || this._position == ChartLegendPosition.None || items.Count == 0)
      return;

    // Draw background
    if (this._backgroundColor != Color.Transparent) {
      using var bgBrush = new SolidBrush(this._backgroundColor);
      g.FillRectangle(bgBrush, bounds);
    }

    // Draw border
    if (this._borderWidth > 0) {
      using var borderPen = new Pen(this._borderColor, this._borderWidth);
      g.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    var font = this.GetEffectiveFont();
    var itemHeight = Math.Max(font.Height, this._symbolHeight);
    var x = bounds.X + this._padding;
    var y = bounds.Y + this._padding;

    var columns = this._orientation == ChartOrientation.Horizontal ? Math.Min(this._maxColumns, items.Count) : 1;
    var columnWidth = (bounds.Width - this._padding * 2 + this._itemSpacing) / columns - this._itemSpacing;
    var currentColumn = 0;

    for (var i = 0; i < items.Count; ++i) {
      var item = items[i];
      var alpha = item.Visible ? 255 : 100;
      var isHighlighted = highlightedIndex == i;

      // Draw symbol
      var symbolRect = new RectangleF(x, y + (itemHeight - this._symbolHeight) / 2, this._symbolWidth, this._symbolHeight);
      this._DrawSymbol(g, symbolRect, item, alpha, isHighlighted);

      // Draw text
      var textX = x + this._symbolWidth + this._itemSpacing;
      var textColor = Color.FromArgb(alpha, this._textColor);
      using (var textBrush = new SolidBrush(textColor))
        g.DrawString(item.Text, font, textBrush, textX, y + (itemHeight - font.Height) / 2);

      // Store hit test rectangle
      item.Bounds = new RectangleF(x, y, columnWidth, itemHeight);

      // Move to next position
      if (this._orientation == ChartOrientation.Horizontal) {
        ++currentColumn;
        if (currentColumn >= columns) {
          currentColumn = 0;
          x = bounds.X + this._padding;
          y += itemHeight + this._itemSpacing;
        } else
          x += columnWidth + this._itemSpacing;
      } else
        y += itemHeight + this._itemSpacing;
    }
  }

  private void _DrawSymbol(Graphics g, RectangleF rect, LegendItem item, int alpha, bool highlighted) {
    var color = Color.FromArgb(alpha, item.Color);

    if (highlighted) {
      using var highlightPen = new Pen(Color.Black, 2);
      g.DrawRectangle(highlightPen, rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
    }

    switch (item.SymbolType) {
      case LegendSymbolType.Rectangle:
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);
        break;

      case LegendSymbolType.Circle:
        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, rect);
        break;

      case LegendSymbolType.Line:
        using (var pen = new Pen(color, 2)) {
          g.DrawLine(pen, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);
          if (item.ShowMarker) {
            var centerX = rect.Left + rect.Width / 2;
            var centerY = rect.Top + rect.Height / 2;
            var markerSize = Math.Min(rect.Height, 6);
            using (var brush = new SolidBrush(color))
              g.FillEllipse(brush, centerX - markerSize / 2, centerY - markerSize / 2, markerSize, markerSize);
          }
        }
        break;

      case LegendSymbolType.Area:
        using (var brush = new SolidBrush(Color.FromArgb(100, color)))
          g.FillRectangle(brush, rect);
        using (var pen = new Pen(color, 1))
          g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
        break;

      case LegendSymbolType.Gradient:
        using (var brush = new LinearGradientBrush(rect, color, Color.FromArgb(alpha, ControlPaint.Light(item.Color)), LinearGradientMode.Horizontal))
          g.FillRectangle(brush, rect);
        break;

      case LegendSymbolType.Dashed:
        using (var pen = new Pen(color, 2) { DashStyle = DashStyle.Dash })
          g.DrawLine(pen, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);
        break;

      case LegendSymbolType.Dotted:
        using (var pen = new Pen(color, 2) { DashStyle = DashStyle.Dot })
          g.DrawLine(pen, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);
        break;
    }
  }

  /// <summary>
  /// Performs hit testing for legend items.
  /// </summary>
  internal int HitTest(PointF point, IList<LegendItem> items) {
    for (var i = 0; i < items.Count; ++i) {
      if (items[i].Bounds.Contains(point))
        return i;
    }
    return -1;
  }
}

/// <summary>
/// Represents a legend item.
/// </summary>
public class LegendItem {
  /// <summary>Gets or sets the display text.</summary>
  public string Text { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color Color { get; set; }

  /// <summary>Gets or sets the symbol type.</summary>
  public LegendSymbolType SymbolType { get; set; } = LegendSymbolType.Rectangle;

  /// <summary>Gets or sets whether the item is visible/active.</summary>
  public bool Visible { get; set; } = true;

  /// <summary>Gets or sets whether to show a marker (for line symbols).</summary>
  public bool ShowMarker { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets or sets the hit test bounds (set during rendering).</summary>
  internal RectangleF Bounds { get; set; }

  public LegendItem() { }

  public LegendItem(string text, Color color, LegendSymbolType symbolType = LegendSymbolType.Rectangle) {
    this.Text = text;
    this.Color = color;
    this.SymbolType = symbolType;
  }
}

/// <summary>
/// Specifies the type of legend symbol.
/// </summary>
public enum LegendSymbolType {
  Rectangle,
  Circle,
  Line,
  Area,
  Gradient,
  Dashed,
  Dotted
}
