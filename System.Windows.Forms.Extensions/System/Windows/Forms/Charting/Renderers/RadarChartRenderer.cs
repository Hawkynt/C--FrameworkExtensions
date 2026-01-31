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
/// Renderer for radar (spider) charts.
/// </summary>
public class RadarChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Radar;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    if (context.Series.Count == 0)
      return;

    // Find all categories (X values represent categories)
    var allCategories = new SortedSet<double>();
    foreach (var series in context.Series)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var categories = allCategories.ToList();
    var categoryCount = categories.Count;

    if (categoryCount < 3)
      return;

    // Calculate radar dimensions
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var radius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.4f;

    // Find value range
    var maxValue = context.Series.SelectMany(s => s.Points).Max(p => p.Y);
    var minValue = context.Series.SelectMany(s => s.Points).Min(p => p.Y);
    if (minValue > 0)
      minValue = 0;
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    // Draw grid
    this._DrawRadarGrid(g, centerX, centerY, radius, categoryCount, 5);

    // Draw category labels
    this._DrawCategoryLabels(g, context, centerX, centerY, radius, categories);

    // Draw series
    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var points = new List<PointF>();

      foreach (var category in categories) {
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - category) < 0.001);
        var value = point?.Y ?? 0;

        // Normalize value to 0-1 range
        var normalizedValue = (value - minValue) / valueRange;
        normalizedValue *= context.AnimationProgress;

        var categoryIndex = categories.IndexOf(category);
        var angle = 2 * Math.PI * categoryIndex / categoryCount - Math.PI / 2;
        var px = centerX + (float)(Math.Cos(angle) * radius * normalizedValue);
        var py = centerY + (float)(Math.Sin(angle) * radius * normalizedValue);

        points.Add(new PointF(px, py));
      }

      if (points.Count < 3)
        continue;

      // Close the polygon
      points.Add(points[0]);

      // Fill area
      using (var brush = new SolidBrush(Color.FromArgb(80, series.Color)))
        g.FillPolygon(brush, points.ToArray());

      // Draw outline
      using (var pen = new Pen(series.Color, series.LineWidth))
        g.DrawPolygon(pen, points.Take(points.Count - 1).ToArray());

      // Draw markers and register hit tests
      for (var i = 0; i < points.Count - 1; ++i) {
        var pt = points[i];
        var category = categories[i];
        var dp = series.Points.FirstOrDefault(p => Math.Abs(p.X - category) < 0.001);

        DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, series.Color, Color.White);

        if (dp != null)
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
      }
    }
  }

  private void _DrawRadarGrid(Graphics g, float centerX, float centerY, float radius, int spokes, int rings) {
    using var gridPen = new Pen(Color.LightGray, 1);
    using var spokePen = new Pen(Color.LightGray, 1);

    // Draw concentric rings
    for (var r = 1; r <= rings; ++r) {
      var ringRadius = radius * r / rings;
      var ringPoints = new PointF[spokes];

      for (var s = 0; s < spokes; ++s) {
        var angle = 2 * Math.PI * s / spokes - Math.PI / 2;
        ringPoints[s] = new PointF(
          centerX + (float)(Math.Cos(angle) * ringRadius),
          centerY + (float)(Math.Sin(angle) * ringRadius)
        );
      }

      g.DrawPolygon(gridPen, ringPoints);
    }

    // Draw spokes
    for (var s = 0; s < spokes; ++s) {
      var angle = 2 * Math.PI * s / spokes - Math.PI / 2;
      var endX = centerX + (float)(Math.Cos(angle) * radius);
      var endY = centerY + (float)(Math.Sin(angle) * radius);
      g.DrawLine(spokePen, centerX, centerY, endX, endY);
    }
  }

  private void _DrawCategoryLabels(Graphics g, ChartRenderContext context, float centerX, float centerY, float radius, IList<double> categories) {
    var labelRadius = radius + 15;

    for (var i = 0; i < categories.Count; ++i) {
      var angle = 2 * Math.PI * i / categories.Count - Math.PI / 2;
      var labelX = centerX + (float)(Math.Cos(angle) * labelRadius);
      var labelY = centerY + (float)(Math.Sin(angle) * labelRadius);

      var label = context.Chart.XAxis.Categories != null && i < context.Chart.XAxis.Categories.Length
        ? context.Chart.XAxis.Categories[i]
        : categories[i].ToString("N0");

      var labelSize = g.MeasureString(label, context.Chart.Font);

      // Adjust position based on angle
      if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
        labelX -= labelSize.Width;
      else if (Math.Abs(angle - Math.PI / 2) < 0.1 || Math.Abs(angle + Math.PI / 2) < 0.1)
        labelX -= labelSize.Width / 2;

      using var brush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Chart.Font, brush, labelX, labelY - labelSize.Height / 2);
    }
  }
}

/// <summary>
/// Renderer for polar area charts.
/// </summary>
public class PolarAreaRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.PolarArea;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.4f;

    // Find max value
    var maxValue = series.Points.Max(p => p.Y);
    if (maxValue <= 0)
      return;

    var sliceCount = series.Points.Count;
    var sliceAngle = 360f / sliceCount;

    var colors = this._GetColors(sliceCount);

    var startAngle = -90f;

    for (var i = 0; i < sliceCount; ++i) {
      var dp = series.Points[i];
      var radius = (float)(dp.Y / maxValue * maxRadius);
      radius *= (float)context.AnimationProgress;

      var color = dp.Color ?? colors[i % colors.Length];

      // Skip drawing if radius is too small (avoids AddPie exception with invalid parameters)
      if (radius > 0.5f) {
        // Draw polar segment
        using (var path = new GraphicsPath()) {
          path.AddPie(centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sliceAngle);

          using (var brush = new SolidBrush(color))
            g.FillPath(brush, path);

          using (var pen = new Pen(Color.White, 2))
            g.DrawPath(pen, path);
        }
      }

      // Hit test (only when animation complete)
      if (context.AnimationProgress >= 1) {
        var midAngle = startAngle + sliceAngle / 2;
        var midRad = midAngle * Math.PI / 180;
        var hitRadius = radius / 2;
        var hitX = centerX + (float)(Math.Cos(midRad) * hitRadius);
        var hitY = centerY + (float)(Math.Sin(midRad) * hitRadius);
        context.RegisterHitTestRect(dp, new RectangleF(hitX - 15, hitY - 15, 30, 30));
      }

      startAngle += sliceAngle;
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(31, 119, 180),
    Color.FromArgb(255, 127, 14),
    Color.FromArgb(44, 160, 44),
    Color.FromArgb(214, 39, 40),
    Color.FromArgb(148, 103, 189),
    Color.FromArgb(140, 86, 75),
    Color.FromArgb(227, 119, 194),
    Color.FromArgb(127, 127, 127)
  };
}

/// <summary>
/// Renderer for bullet charts.
/// </summary>
public class BulletChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Bullet;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Bullet chart uses special BulletData from chart's data collections
    // For now, render using standard series data as a simplified bullet chart
    var series = context.Series.FirstOrDefault();
    if (series == null || series.Points.Count == 0)
      return;

    var barHeight = context.PlotArea.Height / series.Points.Count * 0.7f;
    var gap = context.PlotArea.Height / series.Points.Count * 0.15f;

    // Calculate value range from data (0 to max of both Y values and X values which represent targets)
    var maxValue = Math.Max(
      series.Points.Max(p => Math.Abs(p.Y)),
      series.Points.Max(p => Math.Abs(p.X))
    );
    if (maxValue <= 0)
      maxValue = 100;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var y = context.PlotArea.Top + i * (context.PlotArea.Height / series.Points.Count) + gap;

      // Calculate positions using our value range
      var zeroX = context.PlotArea.Left;
      var targetValueX = (float)(context.PlotArea.Left + dp.Y / maxValue * context.PlotArea.Width);

      // Apply animation
      var valueX = zeroX + (float)((targetValueX - zeroX) * context.AnimationProgress);

      // Draw background ranges (poor, satisfactory, good)
      var rangeWidth = context.PlotArea.Width;
      var rangeColors = new[] { Color.FromArgb(200, 200, 200), Color.FromArgb(230, 230, 230), Color.FromArgb(245, 245, 245) };
      var rangeWidths = new[] { 0.6f, 0.8f, 1.0f };

      for (var r = rangeWidths.Length - 1; r >= 0; --r) {
        var rWidth = rangeWidth * rangeWidths[r];
        using var brush = new SolidBrush(rangeColors[r]);
        g.FillRectangle(brush, context.PlotArea.Left, y, rWidth, barHeight);
      }

      // Draw value bar
      var barWidth = Math.Abs(valueX - zeroX);
      using (var brush = new SolidBrush(dp.Color ?? series.Color))
        g.FillRectangle(brush, Math.Min(zeroX, valueX), y + barHeight * 0.25f, barWidth, barHeight * 0.5f);

      // Draw target marker (using X value as target)
      var targetX = (float)(context.PlotArea.Left + dp.X / maxValue * context.PlotArea.Width);
      using (var pen = new Pen(Color.Black, 3))
        g.DrawLine(pen, targetX, y + barHeight * 0.1f, targetX, y + barHeight * 0.9f);

      context.RegisterHitTestRect(dp, new RectangleF(context.PlotArea.Left, y, context.PlotArea.Width, barHeight));
    }
  }
}

/// <summary>Renders a Nightingale/Rose/Coxcomb chart with variable-length polar sectors.</summary>
public class NightingaleChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Nightingale;
  public override bool UsesAxes => false;
  public override bool SupportsMultipleSeries => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var centerX = plotArea.Left + plotArea.Width / 2;
    var centerY = plotArea.Top + plotArea.Height / 2;
    var maxRadius = Math.Min(plotArea.Width, plotArea.Height) / 2 - 10;

    var points = series[0].Points;
    var maxValue = points.Max(p => Math.Abs(p.Y));
    if (maxValue <= 0)
      maxValue = 1;
    var angleStep = 360f / points.Count;

    for (var i = 0; i < points.Count; ++i) {
      var dp = points[i];
      var radius = (float)(maxRadius * Math.Sqrt(Math.Abs(dp.Y) / maxValue));
      // Apply animation
      radius *= (float)context.AnimationProgress;
      var startAngle = i * angleStep - 90;

      // Skip if radius too small (avoids GDI+ exception)
      if (radius > 0.5f) {
        using var brush = new SolidBrush(dp.Color ?? series[0].Color);
        g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, angleStep);
        using var pen = new Pen(Color.White, 1);
        g.DrawPie(pen, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, angleStep);
      }
    }
  }
}

/// <summary>Renders a range plot showing min/max ranges for categories.</summary>
public class RangePlotRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.RangePlot;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var rangeData = context.Chart.RangeData;

    if (rangeData == null || rangeData.Count == 0) {
      new LineChartRenderer().Render(context);
      return;
    }

    var xRange = context.XMax - context.XMin;
    var yRange = context.YMax - context.YMin;
    var color = context.Series.Count > 0 ? context.Series[0].Color : Color.DodgerBlue;

    using var linePen = new Pen(color, 2);
    using var capPen = new Pen(color, 2);

    foreach (var rp in rangeData) {
      var x = (float)(plotArea.Left + (rp.X - context.XMin) / xRange * plotArea.Width);
      var yLow = (float)(plotArea.Bottom - (rp.Low - context.YMin) / yRange * plotArea.Height);
      var yHigh = (float)(plotArea.Bottom - (rp.High - context.YMin) / yRange * plotArea.Height);

      // Draw vertical range line
      g.DrawLine(linePen, x, yLow, x, yHigh);

      // Draw caps
      g.DrawLine(capPen, x - 4, yLow, x + 4, yLow);
      g.DrawLine(capPen, x - 4, yHigh, x + 4, yHigh);

      // Draw mid point if available
      if (rp.Mid.HasValue) {
        var yMid = (float)(plotArea.Bottom - (rp.Mid.Value - context.YMin) / yRange * plotArea.Height);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, x - 4, yMid - 4, 8, 8);
      }
    }
  }
}

/// <summary>Renders small multiples (trellis/lattice/panel chart) showing multiple small charts.</summary>
public class SmallMultiplesRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.SmallMultiples;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var cols = (int)Math.Ceiling(Math.Sqrt(series.Count));
    var rows = (int)Math.Ceiling((double)series.Count / cols);

    var cellWidth = plotArea.Width / cols;
    var cellHeight = plotArea.Height / rows;
    var padding = 5f;

    for (var i = 0; i < series.Count; ++i) {
      var row = i / cols;
      var col = i % cols;
      var cellRect = new RectangleF(
        plotArea.Left + col * cellWidth + padding,
        plotArea.Top + row * cellHeight + padding,
        cellWidth - 2 * padding,
        cellHeight - 2 * padding
      );

      // Draw cell border
      using var borderPen = new Pen(Color.LightGray, 1);
      g.DrawRectangle(borderPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

      // Draw series name
      var nameRect = new RectangleF(cellRect.X, cellRect.Y, cellRect.Width, 15);
      using var textBrush = new SolidBrush(Color.Black);
      g.DrawString(series[i].Name ?? $"Series {i + 1}", context.Chart.Font, textBrush, nameRect);

      // Draw mini line chart with animation
      var chartRect = new RectangleF(cellRect.X, cellRect.Y + 18, cellRect.Width, cellRect.Height - 18);
      this._DrawMiniLineChart(g, series[i], chartRect, context.AnimationProgress);
    }
  }

  private void _DrawMiniLineChart(Graphics g, ChartDataSeries series, RectangleF rect, double animationProgress) {
    if (series.Points.Count < 2)
      return;

    var minY = series.Points.Min(p => p.Y);
    var maxY = series.Points.Max(p => p.Y);
    var yRange = maxY - minY;
    if (yRange == 0)
      yRange = 1;

    var baseY = rect.Bottom;
    var points = new PointF[series.Points.Count];
    for (var i = 0; i < series.Points.Count; ++i) {
      var x = rect.Left + i * rect.Width / (series.Points.Count - 1);
      var targetY = rect.Bottom - (float)((series.Points[i].Y - minY) / yRange * rect.Height);
      // Apply animation: start from bottom and animate to target
      var y = baseY + (float)((targetY - baseY) * animationProgress);
      points[i] = new PointF(x, y);
    }

    using var pen = new Pen(series.Color, 1.5f);
    g.DrawLines(pen, points);
  }
}
