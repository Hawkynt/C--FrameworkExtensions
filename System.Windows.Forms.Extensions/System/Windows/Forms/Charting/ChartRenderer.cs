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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Charting;

/// <summary>
/// Abstract base class for chart renderers.
/// </summary>
public abstract class ChartRenderer {
  /// <summary>Gets the chart type this renderer handles.</summary>
  public abstract AdvancedChartType ChartType { get; }

  /// <summary>Gets whether this chart type uses axes.</summary>
  public virtual bool UsesAxes => true;

  /// <summary>Gets whether this chart type supports multiple series.</summary>
  public virtual bool SupportsMultipleSeries => true;

  /// <summary>Gets the default orientation for this chart type.</summary>
  public virtual ChartOrientation DefaultOrientation => ChartOrientation.Vertical;

  /// <summary>
  /// Renders the chart to the specified graphics context.
  /// </summary>
  /// <param name="context">The rendering context.</param>
  public abstract void Render(ChartRenderContext context);

  /// <summary>
  /// Gets legend items for this chart.
  /// </summary>
  public virtual IList<LegendItem> GetLegendItems(AdvancedChart chart) {
    var items = new List<LegendItem>();

    foreach (var series in chart.Series) {
      if (!series.Visible)
        continue;

      items.Add(new LegendItem {
        Text = series.Name,
        Color = series.Color,
        SymbolType = this._GetSymbolType(),
        Visible = series.Visible,
        ShowMarker = series.ShowMarkers,
        Tag = series
      });
    }

    return items;
  }

  /// <summary>
  /// Performs hit testing for data points.
  /// </summary>
  public virtual ChartHitTestResult HitTest(ChartRenderContext context, PointF point) => null;

  /// <summary>
  /// Gets the symbol type for legend items.
  /// </summary>
  protected virtual LegendSymbolType _GetSymbolType() => LegendSymbolType.Rectangle;

  #region Helper Methods

  // Maximum pixel coordinate to prevent overflow in GDI+ operations
  private const float MaxPixelCoordinate = 1e6f;
  private const float MinPixelCoordinate = -1e6f;

  /// <summary>
  /// Clamps a pixel coordinate to a safe range to prevent GDI+ overflow.
  /// </summary>
  private static float ClampPixel(float value) {
    if (float.IsNaN(value) || float.IsInfinity(value))
      return 0;
    return Math.Max(MinPixelCoordinate, Math.Min(MaxPixelCoordinate, value));
  }

  /// <summary>
  /// Converts a data value to a pixel coordinate.
  /// </summary>
  protected static float ValueToPixelX(ChartRenderContext ctx, double value) {
    if (double.IsNaN(value) || double.IsInfinity(value))
      return ctx.PlotArea.Left;

    var position = ctx.XAxis.ValueToPosition(value, ctx.XMin, ctx.XMax);
    if (double.IsNaN(position) || double.IsInfinity(position))
      return ctx.PlotArea.Left;

    return ClampPixel(ctx.PlotArea.Left + (float)position * ctx.PlotArea.Width);
  }

  /// <summary>
  /// Converts a data value to a pixel coordinate.
  /// </summary>
  protected static float ValueToPixelY(ChartRenderContext ctx, double value) {
    if (double.IsNaN(value) || double.IsInfinity(value))
      return ctx.PlotArea.Bottom;

    var position = ctx.YAxis.ValueToPosition(value, ctx.YMin, ctx.YMax);
    if (double.IsNaN(position) || double.IsInfinity(position))
      return ctx.PlotArea.Bottom;

    return ClampPixel(ctx.PlotArea.Bottom - (float)position * ctx.PlotArea.Height);
  }

  /// <summary>
  /// Converts a pixel coordinate to a data value.
  /// </summary>
  protected static double PixelToValueX(ChartRenderContext ctx, float pixel) {
    var position = (pixel - ctx.PlotArea.Left) / ctx.PlotArea.Width;
    return ctx.XAxis.PositionToValue(position, ctx.XMin, ctx.XMax);
  }

  /// <summary>
  /// Converts a pixel coordinate to a data value.
  /// </summary>
  protected static double PixelToValueY(ChartRenderContext ctx, float pixel) {
    var position = (ctx.PlotArea.Bottom - pixel) / ctx.PlotArea.Height;
    return ctx.YAxis.PositionToValue(position, ctx.YMin, ctx.YMax);
  }

  /// <summary>
  /// Draws a marker at the specified point.
  /// </summary>
  protected static void DrawMarker(Graphics g, PointF point, AdvancedMarkerStyle style, int size, Color color, Color? borderColor = null) {
    var halfSize = size / 2f;

    using var brush = new SolidBrush(color);
    using var pen = new Pen(borderColor ?? color, 1);

    switch (style) {
      case AdvancedMarkerStyle.Circle:
        g.FillEllipse(brush, point.X - halfSize, point.Y - halfSize, size, size);
        if (borderColor.HasValue)
          g.DrawEllipse(pen, point.X - halfSize, point.Y - halfSize, size, size);
        break;

      case AdvancedMarkerStyle.Square:
        g.FillRectangle(brush, point.X - halfSize, point.Y - halfSize, size, size);
        if (borderColor.HasValue)
          g.DrawRectangle(pen, point.X - halfSize, point.Y - halfSize, size, size);
        break;

      case AdvancedMarkerStyle.Diamond:
        using (var path = new GraphicsPath()) {
          path.AddPolygon(new[] {
            new PointF(point.X, point.Y - halfSize),
            new PointF(point.X + halfSize, point.Y),
            new PointF(point.X, point.Y + halfSize),
            new PointF(point.X - halfSize, point.Y)
          });
          g.FillPath(brush, path);
          if (borderColor.HasValue)
            g.DrawPath(pen, path);
        }
        break;

      case AdvancedMarkerStyle.Triangle:
        using (var path = new GraphicsPath()) {
          path.AddPolygon(new[] {
            new PointF(point.X, point.Y - halfSize),
            new PointF(point.X + halfSize, point.Y + halfSize),
            new PointF(point.X - halfSize, point.Y + halfSize)
          });
          g.FillPath(brush, path);
          if (borderColor.HasValue)
            g.DrawPath(pen, path);
        }
        break;

      case AdvancedMarkerStyle.InvertedTriangle:
        using (var path = new GraphicsPath()) {
          path.AddPolygon(new[] {
            new PointF(point.X, point.Y + halfSize),
            new PointF(point.X + halfSize, point.Y - halfSize),
            new PointF(point.X - halfSize, point.Y - halfSize)
          });
          g.FillPath(brush, path);
          if (borderColor.HasValue)
            g.DrawPath(pen, path);
        }
        break;

      case AdvancedMarkerStyle.Cross:
        using (var crossPen = new Pen(color, 2)) {
          g.DrawLine(crossPen, point.X - halfSize, point.Y, point.X + halfSize, point.Y);
          g.DrawLine(crossPen, point.X, point.Y - halfSize, point.X, point.Y + halfSize);
        }
        break;

      case AdvancedMarkerStyle.Plus:
        using (var crossPen = new Pen(color, 2)) {
          g.DrawLine(crossPen, point.X - halfSize, point.Y, point.X + halfSize, point.Y);
          g.DrawLine(crossPen, point.X, point.Y - halfSize, point.X, point.Y + halfSize);
        }
        break;

      case AdvancedMarkerStyle.Star:
        DrawStar(g, brush, point, halfSize, 5);
        break;

      case AdvancedMarkerStyle.Pentagon:
        DrawPolygon(g, brush, point, halfSize, 5);
        break;

      case AdvancedMarkerStyle.Hexagon:
        DrawPolygon(g, brush, point, halfSize, 6);
        break;
    }
  }

  /// <summary>
  /// Draws a regular polygon.
  /// </summary>
  protected static void DrawPolygon(Graphics g, Brush brush, PointF center, float radius, int sides) {
    var points = new PointF[sides];
    var angleStep = Math.PI * 2 / sides;
    var startAngle = -Math.PI / 2;

    for (var i = 0; i < sides; ++i) {
      var angle = startAngle + i * angleStep;
      points[i] = new PointF(
        center.X + (float)(radius * Math.Cos(angle)),
        center.Y + (float)(radius * Math.Sin(angle))
      );
    }

    g.FillPolygon(brush, points);
  }

  /// <summary>
  /// Draws a star shape.
  /// </summary>
  protected static void DrawStar(Graphics g, Brush brush, PointF center, float radius, int points) {
    var vertices = new PointF[points * 2];
    var angleStep = Math.PI / points;
    var innerRadius = radius * 0.4f;
    var startAngle = -Math.PI / 2;

    for (var i = 0; i < points * 2; ++i) {
      var angle = startAngle + i * angleStep;
      var r = i % 2 == 0 ? radius : innerRadius;
      vertices[i] = new PointF(
        center.X + (float)(r * Math.Cos(angle)),
        center.Y + (float)(r * Math.Sin(angle))
      );
    }

    g.FillPolygon(brush, vertices);
  }

  /// <summary>
  /// Gets the DashStyle for a ChartLineStyle.
  /// </summary>
  protected static DashStyle GetDashStyle(ChartLineStyle style) => style switch {
    ChartLineStyle.Dash => DashStyle.Dash,
    ChartLineStyle.Dot => DashStyle.Dot,
    ChartLineStyle.DashDot => DashStyle.DashDot,
    ChartLineStyle.DashDotDot => DashStyle.DashDotDot,
    _ => DashStyle.Solid
  };

  /// <summary>
  /// Draws a data label at the specified position.
  /// </summary>
  protected static void DrawDataLabel(Graphics g, string text, PointF position, Font font, Color color, ChartDataLabelPosition labelPosition = ChartDataLabelPosition.Top) {
    if (string.IsNullOrEmpty(text))
      return;

    var size = g.MeasureString(text, font);
    var x = position.X - size.Width / 2;
    var y = position.Y;

    switch (labelPosition) {
      case ChartDataLabelPosition.Top:
        y = position.Y - size.Height - 4;
        break;
      case ChartDataLabelPosition.Bottom:
        y = position.Y + 4;
        break;
      case ChartDataLabelPosition.Left:
        x = position.X - size.Width - 4;
        y = position.Y - size.Height / 2;
        break;
      case ChartDataLabelPosition.Right:
        x = position.X + 4;
        y = position.Y - size.Height / 2;
        break;
      case ChartDataLabelPosition.Center:
        y = position.Y - size.Height / 2;
        break;
    }

    using var brush = new SolidBrush(color);
    g.DrawString(text, font, brush, x, y);
  }

  /// <summary>
  /// Calculates the bar width for bar/column charts.
  /// </summary>
  protected static float CalculateBarWidth(ChartRenderContext ctx, int seriesCount, int categoryCount, float groupGap = 0.2f, float barGap = 0.05f) {
    var totalWidth = ctx.PlotArea.Width / categoryCount;
    var groupWidth = totalWidth * (1 - groupGap);
    return groupWidth / seriesCount * (1 - barGap);
  }

  /// <summary>
  /// Creates a color with modified alpha.
  /// </summary>
  protected static Color WithAlpha(Color color, int alpha)
    => Color.FromArgb(alpha, color);

  /// <summary>
  /// Lightens a color.
  /// </summary>
  protected static Color Lighten(Color color, float factor = 0.3f) {
    var r = (int)(color.R + (255 - color.R) * factor);
    var g = (int)(color.G + (255 - color.G) * factor);
    var b = (int)(color.B + (255 - color.B) * factor);
    return Color.FromArgb(color.A, Math.Min(255, r), Math.Min(255, g), Math.Min(255, b));
  }

  /// <summary>
  /// Darkens a color.
  /// </summary>
  protected static Color Darken(Color color, float factor = 0.3f) {
    var r = (int)(color.R * (1 - factor));
    var g = (int)(color.G * (1 - factor));
    var b = (int)(color.B * (1 - factor));
    return Color.FromArgb(color.A, Math.Max(0, r), Math.Max(0, g), Math.Max(0, b));
  }

  /// <summary>
  /// Creates a gradient brush.
  /// </summary>
  protected static LinearGradientBrush CreateGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode mode)
    => new(rect, color1, color2, mode);

  /// <summary>
  /// Draws a rounded rectangle.
  /// </summary>
  protected static void DrawRoundedRectangle(Graphics g, Pen pen, RectangleF rect, float radius) {
    using var path = CreateRoundedRectanglePath(rect, radius);
    g.DrawPath(pen, path);
  }

  /// <summary>
  /// Fills a rounded rectangle.
  /// </summary>
  protected static void FillRoundedRectangle(Graphics g, Brush brush, RectangleF rect, float radius) {
    using var path = CreateRoundedRectanglePath(rect, radius);
    g.FillPath(brush, path);
  }

  /// <summary>
  /// Creates a rounded rectangle path.
  /// </summary>
  protected static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius) {
    var path = new GraphicsPath();

    // Check for minimum valid dimensions to prevent GDI+ exceptions
    if (rect.Width < 1 || rect.Height < 1) {
      // Return an empty path for invalid dimensions
      return path;
    }

    var diameter = radius * 2;

    if (diameter > rect.Width)
      diameter = rect.Width;
    if (diameter > rect.Height)
      diameter = rect.Height;

    // Ensure diameter is at least 1 to prevent AddArc exceptions
    if (diameter < 1)
      diameter = 1;

    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();

    return path;
  }

  /// <summary>
  /// Interpolates between two colors.
  /// </summary>
  protected static Color InterpolateColor(Color color1, Color color2, double t) {
    t = Math.Max(0, Math.Min(1, t));
    return Color.FromArgb(
      (int)(color1.A + (color2.A - color1.A) * t),
      (int)(color1.R + (color2.R - color1.R) * t),
      (int)(color1.G + (color2.G - color1.G) * t),
      (int)(color1.B + (color2.B - color1.B) * t)
    );
  }

  /// <summary>
  /// Gets a color from a gradient scale.
  /// </summary>
  protected static Color GetGradientColor(double value, double min, double max, Color lowColor, Color highColor) {
    if (Math.Abs(max - min) < double.Epsilon)
      return lowColor;

    var t = (value - min) / (max - min);
    return InterpolateColor(lowColor, highColor, t);
  }

  /// <summary>
  /// Gets a color from a multi-stop gradient.
  /// </summary>
  protected static Color GetGradientColor(double value, double min, double max, params (double position, Color color)[] stops) {
    if (stops == null || stops.Length == 0)
      return Color.Black;
    if (stops.Length == 1)
      return stops[0].color;

    var t = Math.Abs(max - min) < double.Epsilon ? 0 : (value - min) / (max - min);
    t = Math.Max(0, Math.Min(1, t));

    for (var i = 0; i < stops.Length - 1; ++i) {
      if (t >= stops[i].position && t <= stops[i + 1].position) {
        var localT = (t - stops[i].position) / (stops[i + 1].position - stops[i].position);
        return InterpolateColor(stops[i].color, stops[i + 1].color, localT);
      }
    }

    return t <= stops[0].position ? stops[0].color : stops[stops.Length - 1].color;
  }

  #endregion
}

/// <summary>
/// Context for chart rendering operations.
/// </summary>
public class ChartRenderContext {
  /// <summary>Gets the graphics context.</summary>
  public Graphics Graphics { get; set; }

  /// <summary>Gets the chart being rendered.</summary>
  public AdvancedChart Chart { get; set; }

  /// <summary>Gets the total bounds of the chart control.</summary>
  public RectangleF TotalBounds { get; set; }

  /// <summary>Gets the plot area (where data is drawn).</summary>
  public RectangleF PlotArea { get; set; }

  /// <summary>Gets the X-axis.</summary>
  public ChartAxis XAxis { get; set; }

  /// <summary>Gets the primary Y-axis.</summary>
  public ChartAxis YAxis { get; set; }

  /// <summary>Gets the secondary Y-axis.</summary>
  public ChartAxis Y2Axis { get; set; }

  /// <summary>Gets the minimum X value.</summary>
  public double XMin { get; set; }

  /// <summary>Gets the maximum X value.</summary>
  public double XMax { get; set; }

  /// <summary>Gets the minimum Y value.</summary>
  public double YMin { get; set; }

  /// <summary>Gets the maximum Y value.</summary>
  public double YMax { get; set; }

  /// <summary>Gets the minimum secondary Y value.</summary>
  public double Y2Min { get; set; }

  /// <summary>Gets the maximum secondary Y value.</summary>
  public double Y2Max { get; set; }

  /// <summary>Gets the series to render.</summary>
  public IList<ChartDataSeries> Series { get; set; }

  /// <summary>Gets the hit test rectangles (populated during rendering).</summary>
  public Dictionary<object, RectangleF> HitTestRects { get; } = new();

  /// <summary>Gets or sets the highlighted series index.</summary>
  public int? HighlightedSeriesIndex { get; set; }

  /// <summary>Gets or sets the highlighted point index.</summary>
  public int? HighlightedPointIndex { get; set; }

  /// <summary>Gets whether to show data labels.</summary>
  public bool ShowDataLabels { get; set; }

  /// <summary>Gets the data label position.</summary>
  public ChartDataLabelPosition DataLabelPosition { get; set; }

  /// <summary>Gets the animation progress (0-1).</summary>
  public double AnimationProgress { get; set; } = 1.0;

  /// <summary>
  /// Registers a hit test rectangle for a data point.
  /// </summary>
  public void RegisterHitTestRect(object dataPoint, RectangleF rect) => this.HitTestRects[dataPoint] = rect;
}

/// <summary>
/// Result of a chart hit test.
/// </summary>
public class ChartHitTestResult {
  /// <summary>Gets or sets the hit series.</summary>
  public ChartDataSeries Series { get; set; }

  /// <summary>Gets or sets the hit data point.</summary>
  public ChartPoint DataPoint { get; set; }

  /// <summary>Gets or sets the series index.</summary>
  public int SeriesIndex { get; set; }

  /// <summary>Gets or sets the point index.</summary>
  public int PointIndex { get; set; }

  /// <summary>Gets or sets the hit element type.</summary>
  public ChartHitTestElement ElementType { get; set; }

  /// <summary>Gets or sets additional hit information.</summary>
  public object AdditionalInfo { get; set; }
}

/// <summary>
/// Specifies the type of chart element hit.
/// </summary>
public enum ChartHitTestElement {
  None,
  DataPoint,
  Series,
  Axis,
  Legend,
  Title,
  PlotArea,
  GridLine
}
