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
/// Renderer for slope charts.
/// </summary>
public class SlopeChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Slope;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Slope chart shows change between two time points
    // Each series should have exactly 2 points (start and end)
    if (context.Series.Count == 0)
      return;

    var leftX = context.PlotArea.Left + 50;
    var rightX = context.PlotArea.Right - 50;

    // Find value range across all series
    var allValues = context.Series.SelectMany(s => s.Points.Select(p => p.Y)).ToList();
    if (allValues.Count == 0)
      return;

    var minValue = allValues.Min();
    var maxValue = allValues.Max();
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    var colors = this._GetColors(context.Series.Count);

    // Draw axis labels
    using (var brush = new SolidBrush(Color.Black)) {
      var startLabel = context.Chart.XAxis.Categories?.Length > 0 ? context.Chart.XAxis.Categories[0] : "Start";
      var endLabel = context.Chart.XAxis.Categories?.Length > 1 ? context.Chart.XAxis.Categories[1] : "End";

      var startSize = g.MeasureString(startLabel, context.Chart.Font);
      var endSize = g.MeasureString(endLabel, context.Chart.Font);

      g.DrawString(startLabel, context.Chart.Font, brush, leftX - startSize.Width / 2, context.PlotArea.Bottom + 10);
      g.DrawString(endLabel, context.Chart.Font, brush, rightX - endSize.Width / 2, context.PlotArea.Bottom + 10);
    }

    // Draw vertical axis lines
    using (var pen = new Pen(Color.LightGray, 1)) {
      g.DrawLine(pen, leftX, context.PlotArea.Top, leftX, context.PlotArea.Bottom);
      g.DrawLine(pen, rightX, context.PlotArea.Top, rightX, context.PlotArea.Bottom);
    }

    // Draw series
    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      if (series.Points.Count < 2)
        continue;

      var startPoint = series.Points[0];
      var endPoint = series.Points[1];

      var startY = context.PlotArea.Top + (float)((maxValue - startPoint.Y) / valueRange * context.PlotArea.Height);
      var endY = context.PlotArea.Top + (float)((maxValue - endPoint.Y) / valueRange * context.PlotArea.Height);

      // Apply animation
      endY = startY + (endY - startY) * (float)context.AnimationProgress;
      var animatedRightX = leftX + (rightX - leftX) * (float)context.AnimationProgress;

      var color = series.Color.A > 0 ? series.Color : colors[si % colors.Length];

      // Draw connecting line
      using (var pen = new Pen(color, series.LineWidth))
        g.DrawLine(pen, leftX, startY, animatedRightX, endY);

      // Draw start point
      DrawMarker(g, new PointF(leftX, startY), series.MarkerStyle, series.MarkerSize + 2, color, Color.White);
      context.RegisterHitTestRect(startPoint, new RectangleF(leftX - series.MarkerSize, startY - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));

      // Draw end point
      if (context.AnimationProgress >= 1) {
        DrawMarker(g, new PointF(rightX, endY), series.MarkerStyle, series.MarkerSize + 2, color, Color.White);
        context.RegisterHitTestRect(endPoint, new RectangleF(rightX - series.MarkerSize, endY - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
      }

      // Draw labels
      using (var brush = new SolidBrush(color)) {
        var startLabel = $"{series.Name}: {startPoint.Y:N1}";
        var startSize = g.MeasureString(startLabel, context.Chart.Font);
        g.DrawString(startLabel, context.Chart.Font, brush, leftX - startSize.Width - 10, startY - startSize.Height / 2);

        if (context.AnimationProgress >= 1) {
          var endLabel = $"{endPoint.Y:N1}";
          g.DrawString(endLabel, context.Chart.Font, brush, rightX + 10, endY - startSize.Height / 2);
        }
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(230, 126, 34)
  };
}

/// <summary>
/// Renderer for bump charts (ranking over time).
/// </summary>
public class BumpChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Bump;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    if (context.Series.Count == 0)
      return;

    // Collect all X values (time points)
    var allXValues = new SortedSet<double>();
    foreach (var series in context.Series)
    foreach (var point in series.Points)
      allXValues.Add(point.X);

    var xValues = allXValues.ToList();
    if (xValues.Count < 2)
      return;

    var seriesCount = context.Series.Count;
    var timePoints = xValues.Count;

    // Calculate rankings at each time point
    var rankings = new Dictionary<double, Dictionary<string, int>>();
    foreach (var x in xValues) {
      var valuesAtX = context.Series
        .Select(s => (Series: s, Point: s.Points.FirstOrDefault(p => Math.Abs(p.X - x) < 0.001)))
        .Where(sp => sp.Point != null)
        .OrderByDescending(sp => sp.Point.Y)
        .ToList();

      rankings[x] = new Dictionary<string, int>();
      for (var r = 0; r < valuesAtX.Count; ++r)
        rankings[x][valuesAtX[r].Series.Name ?? $"Series{context.Series.IndexOf(valuesAtX[r].Series)}"] = r + 1;
    }

    var colors = this._GetColors(seriesCount);
    var xSpacing = context.PlotArea.Width / (timePoints - 1);
    var ySpacing = context.PlotArea.Height / (seriesCount + 1);

    // Draw time point labels
    using (var brush = new SolidBrush(Color.Black)) {
      for (var t = 0; t < timePoints; ++t) {
        var x = context.PlotArea.Left + t * xSpacing;
        var label = context.Chart.XAxis.Categories?.Length > t ? context.Chart.XAxis.Categories[t] : xValues[t].ToString("N0");
        var labelSize = g.MeasureString(label, context.Chart.Font);
        g.DrawString(label, context.Chart.Font, brush, x - labelSize.Width / 2, context.PlotArea.Bottom + 10);
      }
    }

    // Draw series lines
    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      var seriesKey = series.Name ?? $"Series{si}";
      var color = series.Color.A > 0 ? series.Color : colors[si % colors.Length];

      var points = new List<PointF>();

      for (var t = 0; t < timePoints; ++t) {
        var x = context.PlotArea.Left + t * xSpacing;

        if (rankings[xValues[t]].TryGetValue(seriesKey, out var rank)) {
          var y = context.PlotArea.Top + rank * ySpacing;

          // Apply animation
          if (t > 0 && context.AnimationProgress < 1) {
            var progress = Math.Min(1, context.AnimationProgress * timePoints / (t + 1));
            var prevY = points.Count > 0 ? points[points.Count - 1].Y : y;
            y = prevY + (y - prevY) * (float)progress;
          }

          points.Add(new PointF(x, y));
        }
      }

      if (points.Count < 2)
        continue;

      // Draw smooth curve
      using (var pen = new Pen(color, series.LineWidth + 1))
        g.DrawCurve(pen, points.ToArray(), 0.3f);

      // Draw markers and labels
      for (var i = 0; i < points.Count; ++i) {
        var pt = points[i];
        DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize + 2, color, Color.White);

        if (series.Points.Count > i)
          context.RegisterHitTestRect(series.Points[i], new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));

        // Draw rank number
        var rank = rankings[xValues[i]][seriesKey];
        using (var brush = new SolidBrush(Color.White)) {
          var rankText = rank.ToString();
          var rankSize = g.MeasureString(rankText, context.Chart.Font);
          g.DrawString(rankText, context.Chart.Font, brush, pt.X - rankSize.Width / 2, pt.Y - rankSize.Height / 2);
        }
      }

      // Draw series label at end
      if (points.Count > 0) {
        var lastPoint = points[points.Count - 1];
        using var brush = new SolidBrush(color);
        g.DrawString(series.Name ?? seriesKey, context.Chart.Font, brush, lastPoint.X + 15, lastPoint.Y - 7);
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94)
  };
}

/// <summary>
/// Renderer for parallel coordinates charts.
/// </summary>
public class ParallelCoordinatesRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.ParallelCoordinates;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    if (context.Series.Count == 0)
      return;

    // Each point in a series represents a dimension value
    // Find all dimensions (X values)
    var allXValues = new SortedSet<double>();
    foreach (var series in context.Series)
    foreach (var point in series.Points)
      allXValues.Add(point.X);

    var dimensions = allXValues.ToList();
    if (dimensions.Count < 2)
      return;

    var dimCount = dimensions.Count;
    var axisSpacing = context.PlotArea.Width / (dimCount - 1);

    // Calculate min/max for each dimension
    var dimRanges = dimensions.ToDictionary(
      d => d,
      d => {
        var values = context.Series
          .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - d) < 0.001).Select(p => p.Y))
          .ToList();
        if (values.Count == 0)
          return (Min: 0.0, Max: 1.0);
        var min = values.Min();
        var max = values.Max();
        return (Min: min, Max: max == min ? min + 1 : max);
      }
    );

    var colors = this._GetColors(context.Series.Count);

    // Draw axes
    using (var pen = new Pen(Color.Gray, 1)) {
      for (var d = 0; d < dimCount; ++d) {
        var x = context.PlotArea.Left + d * axisSpacing;
        g.DrawLine(pen, x, context.PlotArea.Top, x, context.PlotArea.Bottom);
      }
    }

    // Draw axis labels and ticks
    using (var brush = new SolidBrush(Color.Black)) {
      for (var d = 0; d < dimCount; ++d) {
        var x = context.PlotArea.Left + d * axisSpacing;
        var dim = dimensions[d];

        // Dimension label
        var label = context.Chart.XAxis.Categories?.Length > d ? context.Chart.XAxis.Categories[d] : $"Dim {d + 1}";
        var labelSize = g.MeasureString(label, context.Chart.Font);
        g.DrawString(label, context.Chart.Font, brush, x - labelSize.Width / 2, context.PlotArea.Bottom + 10);

        // Min/max values
        var (min, max) = dimRanges[dim];
        g.DrawString(max.ToString("N1"), context.Chart.Font, brush, x + 5, context.PlotArea.Top - 5);
        g.DrawString(min.ToString("N1"), context.Chart.Font, brush, x + 5, context.PlotArea.Bottom - 15);
      }
    }

    // Draw polylines for each series
    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      var color = series.Color.A > 0 ? series.Color : colors[si % colors.Length];

      var isHighlighted = context.HighlightedSeriesIndex.HasValue && context.HighlightedSeriesIndex.Value == si;
      var alpha = isHighlighted ? 255 : 100;
      var lineWidth = isHighlighted ? series.LineWidth + 1 : series.LineWidth;

      var points = new List<PointF>();

      for (var d = 0; d < dimCount; ++d) {
        var dim = dimensions[d];
        var x = context.PlotArea.Left + d * axisSpacing;

        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - dim) < 0.001);
        if (point == null)
          continue;

        var (min, max) = dimRanges[dim];
        var normalizedY = (point.Y - min) / (max - min);
        var y = context.PlotArea.Bottom - (float)(normalizedY * context.PlotArea.Height);

        // Apply animation
        if (d > 0 && context.AnimationProgress < 1) {
          var prevY = points.Count > 0 ? points[points.Count - 1].Y : y;
          var progress = Math.Min(1, context.AnimationProgress * dimCount / (d + 1));
          y = prevY + (y - prevY) * (float)progress;
        }

        points.Add(new PointF(x, y));
      }

      if (points.Count < 2)
        continue;

      // Draw polyline
      using (var pen = new Pen(Color.FromArgb(alpha, color), lineWidth))
        g.DrawLines(pen, points.ToArray());

      // Draw points on highlighted series
      if (isHighlighted) {
        foreach (var pt in points)
          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, color, Color.White);
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(230, 126, 34)
  };
}

/// <summary>
/// Renderer for radial bar charts.
/// </summary>
public class RadialBarRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.RadialBar;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Rectangle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;

    var barCount = series.Points.Count;
    var barThickness = maxRadius / (barCount + 1) * 0.8f;
    var barGap = maxRadius / (barCount + 1) * 0.2f;

    // Find max value for scaling
    var maxValue = series.Points.Max(p => Math.Abs(p.Y));
    if (maxValue <= 0)
      maxValue = 100;

    var colors = this._GetColors(barCount);

    // Draw background circles and bars
    for (var i = 0; i < barCount; ++i) {
      var dp = series.Points[i];
      var radius = maxRadius - i * (barThickness + barGap) - barThickness / 2;

      // Background arc (full circle)
      using (var pen = new Pen(Color.FromArgb(230, 230, 230), barThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round }) {
        var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawArc(pen, arcRect, 0, 360);
      }

      // Value arc
      var sweepAngle = (float)(dp.Y / maxValue * 360);
      sweepAngle *= (float)context.AnimationProgress;

      var color = dp.Color ?? colors[i % colors.Length];

      using (var pen = new Pen(color, barThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round }) {
        var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
        if (sweepAngle > 0.5f)
          g.DrawArc(pen, arcRect, -90, sweepAngle);
      }

      // Draw label
      var label = dp.Label ?? dp.Y.ToString("N0");
      var labelSize = g.MeasureString(label, context.Chart.Font);

      using (var brush = new SolidBrush(color))
        g.DrawString(label, context.Chart.Font, brush, context.PlotArea.Left - labelSize.Width - 10, centerY - radius - labelSize.Height / 2);

      // Draw value at end of arc
      if (context.AnimationProgress >= 1) {
        var endAngle = -90 + sweepAngle;
        var endRad = endAngle * Math.PI / 180;
        var valueX = centerX + (float)(Math.Cos(endRad) * radius);
        var valueY = centerY + (float)(Math.Sin(endRad) * radius);

        var valueText = dp.Y.ToString("N0");
        var valueSize = g.MeasureString(valueText, context.Chart.Font);

        using var valueBrush = new SolidBrush(Color.Black);
        g.DrawString(valueText, context.Chart.Font, valueBrush, valueX - valueSize.Width / 2, valueY - valueSize.Height / 2);
      }
    }

    // Draw center label (total or title)
    var totalValue = series.Points.Sum(p => p.Y);
    var totalText = totalValue.ToString("N0");
    using (var font = new Font(context.Chart.Font.FontFamily, context.Chart.Font.Size * 2, FontStyle.Bold)) {
      var totalSize = g.MeasureString(totalText, font);
      using var brush = new SolidBrush(Color.Black);
      g.DrawString(totalText, font, brush, centerX - totalSize.Width / 2, centerY - totalSize.Height / 2);
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(230, 126, 34)
  };
}

/// <summary>
/// Renderer for ordered bar charts (bar chart sorted by value).
/// </summary>
public class OrderedBarRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.OrderedBar;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Rectangle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    // Sort points by value (descending)
    var sortedPoints = series.Points.OrderByDescending(p => p.Y).ToList();

    var barCount = sortedPoints.Count;
    var barHeight = context.PlotArea.Height / barCount * 0.8f;
    var gap = context.PlotArea.Height / barCount * 0.1f;

    var maxValue = sortedPoints.Max(p => Math.Abs(p.Y));
    if (maxValue <= 0)
      maxValue = 1;

    var colors = this._GetColors(barCount);

    for (var i = 0; i < sortedPoints.Count; ++i) {
      var dp = sortedPoints[i];
      var y = context.PlotArea.Top + i * (barHeight + gap * 2) + gap;

      var barWidth = (float)(dp.Y / maxValue * context.PlotArea.Width);
      barWidth *= (float)context.AnimationProgress;

      var color = dp.Color ?? colors[i % colors.Length];

      // Draw bar
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, context.PlotArea.Left, y, barWidth, barHeight);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, context.PlotArea.Left, y, barWidth, barHeight);

      context.RegisterHitTestRect(dp, new RectangleF(context.PlotArea.Left, y, barWidth, barHeight));

      // Draw label on left
      var label = dp.Label ?? $"#{i + 1}";
      var labelSize = g.MeasureString(label, context.Chart.Font);
      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(label, context.Chart.Font, brush, context.PlotArea.Left - labelSize.Width - 5, y + (barHeight - labelSize.Height) / 2);

      // Draw value on bar
      var valueText = dp.Y.ToString("N0");
      var valueSize = g.MeasureString(valueText, context.Chart.Font);

      if (barWidth > valueSize.Width + 10) {
        using var brush = new SolidBrush(Color.White);
        g.DrawString(valueText, context.Chart.Font, brush, context.PlotArea.Left + barWidth - valueSize.Width - 5, y + (barHeight - valueSize.Height) / 2);
      } else {
        using var brush = new SolidBrush(Color.Black);
        g.DrawString(valueText, context.Chart.Font, brush, context.PlotArea.Left + barWidth + 5, y + (barHeight - valueSize.Height) / 2);
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34)
  };
}

/// <summary>
/// Renderer for table heatmap charts.
/// </summary>
public class TableHeatmapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.TableHeatmap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Low value color.
  /// </summary>
  public Color LowColor { get; set; } = Color.FromArgb(255, 255, 255);

  /// <summary>
  /// High value color.
  /// </summary>
  public Color HighColor { get; set; } = Color.FromArgb(52, 152, 219);

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Use series as rows, points as columns
    if (context.Series.Count == 0)
      return;

    var rowCount = context.Series.Count;
    var colCount = context.Series.Max(s => s.Points.Count);

    if (colCount == 0)
      return;

    var cellWidth = context.PlotArea.Width / colCount;
    var cellHeight = context.PlotArea.Height / rowCount;

    // Find value range
    var allValues = context.Series.SelectMany(s => s.Points.Select(p => p.Y)).ToList();
    var minValue = allValues.Min();
    var maxValue = allValues.Max();
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    // Draw cells
    for (var row = 0; row < rowCount; ++row) {
      var series = context.Series[row];
      var y = context.PlotArea.Top + row * cellHeight;

      // Draw row label
      var rowLabel = series.Name ?? $"Row {row + 1}";
      var rowLabelSize = g.MeasureString(rowLabel, context.Chart.Font);
      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(rowLabel, context.Chart.Font, brush, context.PlotArea.Left - rowLabelSize.Width - 10, y + (cellHeight - rowLabelSize.Height) / 2);

      for (var col = 0; col < series.Points.Count; ++col) {
        var dp = series.Points[col];
        var x = context.PlotArea.Left + col * cellWidth;

        var normalizedValue = (dp.Y - minValue) / valueRange;
        normalizedValue *= context.AnimationProgress;

        var cellColor = InterpolateColor(this.LowColor, this.HighColor, (float)normalizedValue);

        using (var brush = new SolidBrush(cellColor))
          g.FillRectangle(brush, x + 1, y + 1, cellWidth - 2, cellHeight - 2);

        context.RegisterHitTestRect(dp, new RectangleF(x, y, cellWidth, cellHeight));

        // Draw value
        var valueText = dp.Y.ToString("N1");
        var valueSize = g.MeasureString(valueText, context.Chart.Font);
        if (valueSize.Width < cellWidth - 4 && valueSize.Height < cellHeight - 4) {
          var textColor = normalizedValue > 0.5 ? Color.White : Color.Black;
          using var brush = new SolidBrush(textColor);
          g.DrawString(valueText, context.Chart.Font, brush, x + (cellWidth - valueSize.Width) / 2, y + (cellHeight - valueSize.Height) / 2);
        }
      }
    }

    // Draw column labels
    for (var col = 0; col < colCount; ++col) {
      var x = context.PlotArea.Left + col * cellWidth;
      var colLabel = context.Chart.XAxis.Categories?.Length > col ? context.Chart.XAxis.Categories[col] : $"Col {col + 1}";
      var colLabelSize = g.MeasureString(colLabel, context.Chart.Font);

      using var brush = new SolidBrush(Color.Black);
      g.DrawString(colLabel, context.Chart.Font, brush, x + (cellWidth - colLabelSize.Width) / 2, context.PlotArea.Bottom + 5);
    }
  }
}

/// <summary>Renders a table chart (data displayed in table format).</summary>
public class TableChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.TableChart;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var maxPoints = series.Max(s => s.Points.Count);
    if (maxPoints == 0)
      return;

    var cols = series.Count + 1; // +1 for row headers
    var rows = maxPoints + 1;    // +1 for column headers
    var cellWidth = plotArea.Width / cols;
    var cellHeight = Math.Min(plotArea.Height / rows, 30);

    // Draw header row
    using var headerBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
    using var borderPen = new Pen(Color.LightGray, 1);
    using var textBrush = new SolidBrush(Color.Black);

    // Top-left corner cell (empty)
    var headerRect = new RectangleF(plotArea.Left, plotArea.Top, cellWidth, cellHeight);
    g.FillRectangle(headerBrush, headerRect);
    g.DrawRectangle(borderPen, headerRect.X, headerRect.Y, headerRect.Width, headerRect.Height);

    // Column headers (series names)
    for (var col = 0; col < series.Count; ++col) {
      var cellRect = new RectangleF(plotArea.Left + (col + 1) * cellWidth, plotArea.Top, cellWidth, cellHeight);
      g.FillRectangle(headerBrush, cellRect);
      g.DrawRectangle(borderPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString(series[col].Name ?? $"Series {col + 1}", context.Chart.Font, textBrush, cellRect, format);
    }

    // Data rows
    for (var row = 0; row < maxPoints; ++row) {
      var y = plotArea.Top + (row + 1) * cellHeight;

      // Row header (X value or label)
      var rowHeaderRect = new RectangleF(plotArea.Left, y, cellWidth, cellHeight);
      g.FillRectangle(headerBrush, rowHeaderRect);
      g.DrawRectangle(borderPen, rowHeaderRect.X, rowHeaderRect.Y, rowHeaderRect.Width, rowHeaderRect.Height);

      var rowLabel = row < series[0].Points.Count ? (series[0].Points[row].Label ?? series[0].Points[row].X.ToString("N1")) : "";
      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString(rowLabel, context.Chart.Font, textBrush, rowHeaderRect, format);

      // Data cells
      for (var col = 0; col < series.Count; ++col) {
        var cellRect = new RectangleF(plotArea.Left + (col + 1) * cellWidth, y, cellWidth, cellHeight);
        g.DrawRectangle(borderPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

        if (row < series[col].Points.Count) {
          var value = series[col].Points[row].Y.ToString("N2");
          g.DrawString(value, context.Chart.Font, textBrush, cellRect, format);
        }
      }
    }
  }
}
