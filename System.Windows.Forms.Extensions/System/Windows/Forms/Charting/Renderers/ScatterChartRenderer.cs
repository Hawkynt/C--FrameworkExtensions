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
using System.Linq;

namespace System.Windows.Forms.Charting.Renderers;

/// <summary>
/// Renderer for scatter plot charts.
/// </summary>
public class ScatterChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Scatter;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Circle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var isHighlighted = context.HighlightedSeriesIndex.HasValue
                          && context.Series.IndexOf(series) == context.HighlightedSeriesIndex.Value;

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        // Apply animation (fade in)
        var alpha = (int)(255 * context.AnimationProgress);
        var color = dp.Color ?? series.Color;
        color = Color.FromArgb(alpha, color);

        var markerSize = isHighlighted ? series.MarkerSize + 2 : series.MarkerSize;

        DrawMarker(g, new PointF(px, py), series.MarkerStyle, markerSize, color, Color.FromArgb(alpha, Color.White));
        context.RegisterHitTestRect(dp, new RectangleF(px - markerSize, py - markerSize, markerSize * 2, markerSize * 2));

        // Data labels
        if (context.ShowDataLabels) {
          var label = dp.Label ?? $"({dp.X:N1}, {dp.Y:N1})";
          DrawDataLabel(g, label, new PointF(px, py), context.Chart.Font, Color.Black, context.DataLabelPosition);
        }
      }
    }
  }

  /// <inheritdoc />
  public override ChartHitTestResult HitTest(ChartRenderContext context, PointF point) {
    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      for (var pi = 0; pi < series.Points.Count; ++pi) {
        var dp = series.Points[pi];
        if (context.HitTestRects.TryGetValue(dp, out var rect) && rect.Contains(point))
          return new ChartHitTestResult {
            Series = series,
            DataPoint = dp,
            SeriesIndex = si,
            PointIndex = pi,
            ElementType = ChartHitTestElement.DataPoint
          };
      }
    }
    return null;
  }
}

/// <summary>
/// Renderer for bubble charts.
/// </summary>
public class BubbleChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Bubble;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Circle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // For bubble charts, we need to get size information from Tag or a third value
    // For now, use the Y value to determine bubble size relative to others

    // Find size range across all series
    var allYValues = context.Series.SelectMany(s => s.Points.Select(p => Math.Abs(p.Y))).ToList();
    var minSize = allYValues.Count > 0 ? allYValues.Min() : 1;
    var maxSize = allYValues.Count > 0 ? allYValues.Max() : 1;
    if (Math.Abs(maxSize - minSize) < 0.001)
      maxSize = minSize + 1;

    const float minBubbleSize = 10;
    const float maxBubbleSize = 50;

    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var isHighlighted = context.HighlightedSeriesIndex.HasValue
                          && context.Series.IndexOf(series) == context.HighlightedSeriesIndex.Value;

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        // Calculate bubble size
        var sizeRatio = (Math.Abs(dp.Y) - minSize) / (maxSize - minSize);
        var bubbleSize = minBubbleSize + (float)(sizeRatio * (maxBubbleSize - minBubbleSize));

        // Apply animation
        if (context.AnimationProgress < 1)
          bubbleSize *= (float)context.AnimationProgress;

        if (isHighlighted)
          bubbleSize += 5;

        var color = dp.Color ?? series.Color;
        var halfSize = bubbleSize / 2;

        // Skip drawing if bubble is too small (prevents GDI+ exceptions)
        if (bubbleSize < 1f) {
          context.RegisterHitTestRect(dp, new RectangleF(px - 2, py - 2, 4, 4));
          continue;
        }

        // Draw bubble with gradient
        var bubbleRect = new RectangleF(px - halfSize, py - halfSize, bubbleSize, bubbleSize);
        using (var path = new GraphicsPath()) {
          path.AddEllipse(bubbleRect);
          using var brush = new PathGradientBrush(path) {
            CenterColor = Lighten(color, 0.4f),
            SurroundColors = new[] { color }
          };
          g.FillEllipse(brush, bubbleRect);
        }

        // Draw border
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawEllipse(pen, bubbleRect);

        context.RegisterHitTestRect(dp, bubbleRect);

        // Data labels
        if (context.ShowDataLabels) {
          var label = dp.Label ?? dp.Y.ToString("N1");
          var labelSize = g.MeasureString(label, context.Chart.Font);
          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, px - labelSize.Width / 2, py - labelSize.Height / 2);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for connected scatter plots.
/// </summary>
public class ConnectedScatterRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.ConnectedScatter;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var points = new List<PointF>();

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);
        points.Add(new PointF(px, py));
      }

      // Draw connecting lines
      if (points.Count > 1) {
        using var pen = new Pen(series.Color, series.LineWidth) { DashStyle = DashStyle.Dash };
        g.DrawLines(pen, points.ToArray());

        // Draw arrows showing direction
        for (var i = 0; i < points.Count - 1; ++i) {
          var start = points[i];
          var end = points[i + 1];

          // Calculate midpoint
          var midX = (start.X + end.X) / 2;
          var midY = (start.Y + end.Y) / 2;

          // Calculate angle
          var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

          // Draw small arrow at midpoint
          var arrowSize = 6f;
          var arrowPoints = new PointF[] {
            new(midX + (float)(arrowSize * Math.Cos(angle)), midY + (float)(arrowSize * Math.Sin(angle))),
            new(midX + (float)(arrowSize * Math.Cos(angle + 2.5)), midY + (float)(arrowSize * Math.Sin(angle + 2.5))),
            new(midX + (float)(arrowSize * Math.Cos(angle - 2.5)), midY + (float)(arrowSize * Math.Sin(angle - 2.5)))
          };

          using var brush = new SolidBrush(series.Color);
          g.FillPolygon(brush, arrowPoints);
        }
      }

      // Draw points
      for (var i = 0; i < points.Count; ++i) {
        var pt = points[i];
        var dp = series.Points[i];
        var color = dp.Color ?? series.Color;

        // Different marker for first/last points
        AdvancedMarkerStyle style;
        if (i == 0)
          style = AdvancedMarkerStyle.Diamond;
        else if (i == points.Count - 1)
          style = AdvancedMarkerStyle.Square;
        else
          style = series.MarkerStyle;

        DrawMarker(g, pt, style, series.MarkerSize, color, Color.White);
        context.RegisterHitTestRect(dp, new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));

        // Point number label
        if (context.ShowDataLabels) {
          var label = dp.Label ?? (i + 1).ToString();
          var labelSize = g.MeasureString(label, context.Chart.Font);
          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, pt.X - labelSize.Width / 2, pt.Y - series.MarkerSize - labelSize.Height - 2);
        }
      }
    }
  }
}

/// <summary>Renders a categorical scatter plot (scatter with category-based X axis).</summary>
public class CategoricalScatterRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.CategoricalScatter;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var categoryCount = series.Max(s => s.Points.Count);
    if (categoryCount == 0)
      return;

    var categoryWidth = plotArea.Width / categoryCount;
    var yRange = context.YMax - context.YMin;

    foreach (var s in series) {
      for (var i = 0; i < s.Points.Count; ++i) {
        var dp = s.Points[i];
        var x = plotArea.Left + (i + 0.5f) * categoryWidth;
        var y = (float)(plotArea.Bottom - (dp.Y - context.YMin) / yRange * plotArea.Height);

        using var brush = new SolidBrush(dp.Color ?? s.Color);
        g.FillEllipse(brush, x - s.MarkerSize, y - s.MarkerSize, s.MarkerSize * 2, s.MarkerSize * 2);
      }
    }
  }
}

/// <summary>Renders a correlogram (matrix of scatter plots for correlation analysis).</summary>
public class CorrelogramRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Correlogram;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count < 2)
      return;

    var n = series.Count;
    var cellWidth = plotArea.Width / n;
    var cellHeight = plotArea.Height / n;

    for (var row = 0; row < n; ++row) {
      for (var col = 0; col < n; ++col) {
        var cellX = plotArea.Left + col * cellWidth;
        var cellY = plotArea.Top + row * cellHeight;
        var cellRect = new RectangleF(cellX + 2, cellY + 2, cellWidth - 4, cellHeight - 4);

        if (row == col) {
          // Diagonal: draw series name
          using var textBrush = new SolidBrush(Color.Black);
          var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
          g.DrawString(series[row].Name ?? $"S{row + 1}", context.Chart.Font, textBrush, cellRect, format);
        } else {
          // Off-diagonal: draw mini scatter plot
          this.DrawMiniScatter(g, series[col], series[row], cellRect);
        }

        // Draw cell border
        using var pen = new Pen(Color.LightGray, 1);
        g.DrawRectangle(pen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
      }
    }
  }

  private void DrawMiniScatter(Graphics g, ChartDataSeries xSeries, ChartDataSeries ySeries, RectangleF rect) {
    var count = Math.Min(xSeries.Points.Count, ySeries.Points.Count);
    if (count == 0)
      return;

    var xMin = xSeries.Points.Min(p => p.Y);
    var xMax = xSeries.Points.Max(p => p.Y);
    var yMin = ySeries.Points.Min(p => p.Y);
    var yMax = ySeries.Points.Max(p => p.Y);

    var xRange = xMax - xMin;
    var yRange = yMax - yMin;
    if (xRange == 0) xRange = 1;
    if (yRange == 0) yRange = 1;

    using var brush = new SolidBrush(Color.FromArgb(150, Color.DodgerBlue));
    for (var i = 0; i < count; ++i) {
      var x = rect.Left + (float)((xSeries.Points[i].Y - xMin) / xRange * rect.Width);
      var y = rect.Bottom - (float)((ySeries.Points[i].Y - yMin) / yRange * rect.Height);
      g.FillEllipse(brush, x - 2, y - 2, 4, 4);
    }
  }
}

/// <summary>Renders a scatter matrix (grid of scatter plots between all variable pairs).</summary>
public class ScatterMatrixRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.ScatterMatrix;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    // Same as correlogram
    new CorrelogramRenderer().Render(context);
  }
}

/// <summary>Renders a hexbin plot (hexagonal binning for dense scatter data).</summary>
public class HexbinRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Hexbin;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var points = series[0].Points;
    var hexSize = 20f;
    var bins = new Dictionary<(int, int), int>();

    var xRange = context.XMax - context.XMin;
    var yRange = context.YMax - context.YMin;

    // Bin points into hexagons
    foreach (var dp in points) {
      var x = (float)((dp.X - context.XMin) / xRange * plotArea.Width);
      var y = (float)((dp.Y - context.YMin) / yRange * plotArea.Height);

      var col = (int)(x / (hexSize * 1.5f));
      var row = (int)(y / (hexSize * Math.Sqrt(3)));

      var key = (col, row);
      bins[key] = bins.GetValueOrDefault(key, 0) + 1;
    }

    if (bins.Count == 0)
      return;

    var maxCount = bins.Values.Max();

    // Draw hexagons
    foreach (var kvp in bins) {
      var (col, row) = kvp.Key;
      var count = kvp.Value;

      var centerX = plotArea.Left + col * hexSize * 1.5f + hexSize;
      var centerY = plotArea.Bottom - (row * hexSize * (float)Math.Sqrt(3) + hexSize);
      if (col % 2 == 1)
        centerY -= hexSize * (float)Math.Sqrt(3) / 2;

      var intensity = (float)count / maxCount;
      var color = Color.FromArgb((int)(50 + intensity * 200), series[0].Color);

      this.DrawHexagon(g, centerX, centerY, hexSize * 0.9f, color);
    }
  }

  private void DrawHexagon(Graphics g, float cx, float cy, float size, Color color) {
    var points = new PointF[6];
    for (var i = 0; i < 6; ++i) {
      var angle = Math.PI / 3 * i - Math.PI / 6;
      points[i] = new PointF(cx + size * (float)Math.Cos(angle), cy + size * (float)Math.Sin(angle));
    }

    using var brush = new SolidBrush(color);
    g.FillPolygon(brush, points);
    using var pen = new Pen(Color.White, 1);
    g.DrawPolygon(pen, points);
  }
}

/// <summary>Renders a contour plot (isolines showing equal values).</summary>
public class ContourPlotRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Contour;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var heatmapData = context.Chart.HeatmapData;

    if (heatmapData == null || heatmapData.Count == 0) {
      // Fall back to a simple representation
      using var brush = new SolidBrush(Color.LightGray);
      g.FillRectangle(brush, plotArea);
      using var textBrush = new SolidBrush(Color.Gray);
      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString("Contour Plot\n(Requires heatmap data)", SystemFonts.DefaultFont, textBrush, plotArea, format);
      return;
    }

    // Draw filled contours based on heatmap data
    var minValue = heatmapData.Min(h => h.Value);
    var maxValue = heatmapData.Max(h => h.Value);
    var valueRange = maxValue - minValue;
    if (valueRange == 0)
      valueRange = 1;

    // Calculate grid dimensions from data
    var maxRow = heatmapData.Max(h => h.Row) + 1;
    var maxCol = heatmapData.Max(h => h.Column) + 1;
    var cellWidth = plotArea.Width / maxCol;
    var cellHeight = plotArea.Height / maxRow;

    var colors = new[] { Color.DarkBlue, Color.Blue, Color.Cyan, Color.Green, Color.Yellow, Color.Orange, Color.Red, Color.DarkRed };

    foreach (var cell in heatmapData) {
      var intensity = (cell.Value - minValue) / valueRange;
      var colorIndex = (int)(intensity * (colors.Length - 1));
      colorIndex = Math.Max(0, Math.Min(colors.Length - 1, colorIndex));

      var x = plotArea.Left + cell.Column * cellWidth;
      var y = plotArea.Top + cell.Row * cellHeight;

      using var brush = new SolidBrush(colors[colorIndex]);
      g.FillRectangle(brush, x, y, cellWidth, cellHeight);
    }
  }
}

/// <summary>Renders a quadrant chart (scatter plot divided into four quadrants).</summary>
public class QuadrantChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.QuadrantChart;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    // Draw quadrant lines
    var centerX = plotArea.Left + plotArea.Width / 2;
    var centerY = plotArea.Top + plotArea.Height / 2;

    using var quadrantPen = new Pen(Color.LightGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
    g.DrawLine(quadrantPen, centerX, plotArea.Top, centerX, plotArea.Bottom);
    g.DrawLine(quadrantPen, plotArea.Left, centerY, plotArea.Right, centerY);

    // Draw quadrant labels
    using var textBrush = new SolidBrush(Color.FromArgb(100, Color.Gray));
    g.DrawString("II", new Font(context.Chart.Font.FontFamily, 20), textBrush, plotArea.Left + 10, plotArea.Top + 10);
    g.DrawString("I", new Font(context.Chart.Font.FontFamily, 20), textBrush, plotArea.Right - 30, plotArea.Top + 10);
    g.DrawString("III", new Font(context.Chart.Font.FontFamily, 20), textBrush, plotArea.Left + 10, plotArea.Bottom - 35);
    g.DrawString("IV", new Font(context.Chart.Font.FontFamily, 20), textBrush, plotArea.Right - 35, plotArea.Bottom - 35);

    // Draw scatter points
    new ScatterChartRenderer().Render(context);
  }
}

/// <summary>Renders a matrix chart (matrix diagram showing relationships).</summary>
public class MatrixChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.MatrixChart;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var heatmapData = context.Chart.HeatmapData;

    if (heatmapData == null || heatmapData.Count == 0) {
      // Draw placeholder
      using var brush = new SolidBrush(Color.LightGray);
      g.FillRectangle(brush, plotArea);
      using var textBrush = new SolidBrush(Color.Gray);
      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString("Matrix Chart\n(Requires matrix data)", SystemFonts.DefaultFont, textBrush, plotArea, format);
      return;
    }

    var minValue = heatmapData.Min(h => h.Value);
    var maxValue = heatmapData.Max(h => h.Value);
    var valueRange = maxValue - minValue;
    if (valueRange == 0)
      valueRange = 1;

    // Calculate grid dimensions from data
    var maxRow = heatmapData.Max(h => h.Row) + 1;
    var maxCol = heatmapData.Max(h => h.Column) + 1;
    var cellWidth = plotArea.Width / maxCol;
    var cellHeight = plotArea.Height / maxRow;

    foreach (var cell in heatmapData) {
      var intensity = (float)((cell.Value - minValue) / valueRange);
      var color = this.InterpolateColor(Color.White, Color.DarkBlue, intensity);

      var x = plotArea.Left + cell.Column * cellWidth;
      var y = plotArea.Top + cell.Row * cellHeight;

      using var brush = new SolidBrush(color);
      g.FillRectangle(brush, x, y, cellWidth, cellHeight);
      using var pen = new Pen(Color.White, 1);
      g.DrawRectangle(pen, x, y, cellWidth, cellHeight);
    }
  }

  private Color InterpolateColor(Color from, Color to, float t) {
    var r = (int)(from.R + (to.R - from.R) * t);
    var green = (int)(from.G + (to.G - from.G) * t);
    var b = (int)(from.B + (to.B - from.B) * t);
    return Color.FromArgb(r, green, b);
  }
}
