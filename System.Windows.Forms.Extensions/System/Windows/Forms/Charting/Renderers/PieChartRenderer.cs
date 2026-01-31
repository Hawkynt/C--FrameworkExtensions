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
/// Renderer for pie charts.
/// </summary>
public class PieChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Pie;

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

    var total = series.Points.Where(p => p.Y > 0).Sum(p => p.Y);
    if (total <= 0)
      return;

    // Calculate pie dimensions
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var size = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.85f;
    var pieRect = new RectangleF(centerX - size / 2, centerY - size / 2, size, size);

    // Apply animation
    var animatedSweep = 360f * (float)context.AnimationProgress;

    // Get colors from chart palette
    var colors = this._GetColors(context, series.Points.Count);

    var startAngle = -90f;
    var usedSweep = 0f;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      if (dp.Y <= 0)
        continue;

      var sweepAngle = (float)(dp.Y / total * 360);

      // Apply animation
      var drawSweep = Math.Min(sweepAngle, animatedSweep - usedSweep);
      if (drawSweep <= 0) {
        usedSweep += sweepAngle;
        startAngle += sweepAngle;
        continue;
      }

      var color = dp.Color ?? colors[i % colors.Length];

      // Check if this slice is hovered
      var isHighlighted = context.HighlightedPointIndex == i;
      var sliceRect = pieRect;
      if (isHighlighted) {
        // Explode the slice slightly
        var midAngle = startAngle + drawSweep / 2;
        var midRad = midAngle * Math.PI / 180;
        var explodeDistance = size * 0.03f;
        sliceRect = new RectangleF(
          pieRect.X + (float)(Math.Cos(midRad) * explodeDistance),
          pieRect.Y + (float)(Math.Sin(midRad) * explodeDistance),
          pieRect.Width,
          pieRect.Height
        );
      }

      // Draw pie slice with gradient
      using (var path = new GraphicsPath()) {
        path.AddPie(sliceRect.X, sliceRect.Y, sliceRect.Width, sliceRect.Height, startAngle, drawSweep);

        using (var brush = new LinearGradientBrush(sliceRect, Lighten(color, 0.2f), color, LinearGradientMode.ForwardDiagonal))
          g.FillPath(brush, path);

        using (var pen = new Pen(Color.White, 2))
          g.DrawPath(pen, path);
      }

      // Calculate hit test region
      var midAngleForHit = startAngle + sweepAngle / 2;
      var midRadForHit = midAngleForHit * Math.PI / 180;
      var hitX = centerX + (float)(Math.Cos(midRadForHit) * size / 4);
      var hitY = centerY + (float)(Math.Sin(midRadForHit) * size / 4);
      context.RegisterHitTestRect(dp, new RectangleF(hitX - 20, hitY - 20, 40, 40));

      // Draw labels
      if (context.ShowDataLabels) {
        var labelAngle = startAngle + drawSweep / 2;
        var labelRad = labelAngle * Math.PI / 180;
        var labelDist = size / 2 + 20;
        var labelX = centerX + (float)(Math.Cos(labelRad) * labelDist);
        var labelY = centerY + (float)(Math.Sin(labelRad) * labelDist);

        var label = dp.Label ?? $"{dp.Y:N1} ({dp.Y / total:P0})";
        var labelSize = g.MeasureString(label, context.Chart.Font);

        // Adjust position based on angle
        if (labelAngle > 90 && labelAngle < 270)
          labelX -= labelSize.Width;
        else if (Math.Abs(labelAngle - 90) < 45 || Math.Abs(labelAngle - 270) < 45)
          labelX -= labelSize.Width / 2;

        using var labelBrush = new SolidBrush(Color.Black);
        g.DrawString(label, context.Chart.Font, labelBrush, labelX, labelY - labelSize.Height / 2);

        // Draw leader line
        var lineStartDist = size / 2 + 5;
        var lineEndDist = size / 2 + 15;
        using var linePen = new Pen(Color.Gray, 1);
        g.DrawLine(linePen,
          centerX + (float)(Math.Cos(labelRad) * lineStartDist),
          centerY + (float)(Math.Sin(labelRad) * lineStartDist),
          centerX + (float)(Math.Cos(labelRad) * lineEndDist),
          centerY + (float)(Math.Sin(labelRad) * lineEndDist)
        );
      }

      usedSweep += sweepAngle;
      startAngle += sweepAngle;
    }
  }

  /// <inheritdoc />
  public override IList<LegendItem> GetLegendItems(AdvancedChart chart) {
    var items = new List<LegendItem>();
    var series = chart.Series.FirstOrDefault();

    if (series == null)
      return items;

    var colors = this._GetColors(new ChartRenderContext { Chart = chart }, series.Points.Count);

    for (var i = 0; i < series.Points.Count; ++i) {
      var point = series.Points[i];
      if (point.Y <= 0)
        continue;

      items.Add(new LegendItem {
        Text = point.Label ?? $"Item {i + 1}",
        Color = point.Color ?? colors[i % colors.Length],
        SymbolType = LegendSymbolType.Rectangle,
        Visible = true,
        Tag = point
      });
    }

    return items;
  }

  private Color[] _GetColors(ChartRenderContext context, int count) {
    var baseColors = new[] {
      Color.FromArgb(31, 119, 180),
      Color.FromArgb(255, 127, 14),
      Color.FromArgb(44, 160, 44),
      Color.FromArgb(214, 39, 40),
      Color.FromArgb(148, 103, 189),
      Color.FromArgb(140, 86, 75),
      Color.FromArgb(227, 119, 194),
      Color.FromArgb(127, 127, 127),
      Color.FromArgb(188, 189, 34),
      Color.FromArgb(23, 190, 207)
    };

    if (count <= baseColors.Length)
      return baseColors;

    // Generate more colors if needed
    var colors = new Color[count];
    for (var i = 0; i < count; ++i) {
      if (i < baseColors.Length)
        colors[i] = baseColors[i];
      else {
        // Generate additional colors by varying hue
        var hue = (i * 137.508f) % 360; // Golden angle for even distribution
        colors[i] = _HslToRgb(hue, 0.7f, 0.5f);
      }
    }

    return colors;
  }

  private static Color _HslToRgb(float h, float s, float l) {
    float r, g, b;

    if (Math.Abs(s) < 0.001f)
      r = g = b = l;
    else {
      var q = l < 0.5f ? l * (1 + s) : l + s - l * s;
      var p = 2 * l - q;
      r = _HueToRgb(p, q, h / 360 + 1f / 3);
      g = _HueToRgb(p, q, h / 360);
      b = _HueToRgb(p, q, h / 360 - 1f / 3);
    }

    return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
  }

  private static float _HueToRgb(float p, float q, float t) {
    if (t < 0)
      t += 1;
    if (t > 1)
      t -= 1;
    if (t < 1f / 6)
      return p + (q - p) * 6 * t;
    if (t < 1f / 2)
      return q;
    if (t < 2f / 3)
      return p + (q - p) * (2f / 3 - t) * 6;
    return p;
  }
}

/// <summary>
/// Renderer for donut charts.
/// </summary>
public class DonutChartRenderer : ChartRenderer {
  private readonly float _holeRatio;

  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Donut;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  public DonutChartRenderer(float holeRatio = 0.5f) => this._holeRatio = Math.Max(0.1f, Math.Min(0.9f, holeRatio));

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var total = series.Points.Where(p => p.Y > 0).Sum(p => p.Y);
    if (total <= 0)
      return;

    // Calculate donut dimensions
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var outerSize = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.85f;
    var innerSize = outerSize * this._holeRatio;

    var outerRect = new RectangleF(centerX - outerSize / 2, centerY - outerSize / 2, outerSize, outerSize);
    var innerRect = new RectangleF(centerX - innerSize / 2, centerY - innerSize / 2, innerSize, innerSize);

    // Apply animation
    var animatedSweep = 360f * (float)context.AnimationProgress;

    // Get colors
    var colors = this._GetColors(series.Points.Count);

    var startAngle = -90f;
    var usedSweep = 0f;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      if (dp.Y <= 0)
        continue;

      var sweepAngle = (float)(dp.Y / total * 360);

      // Apply animation
      var drawSweep = Math.Min(sweepAngle, animatedSweep - usedSweep);
      if (drawSweep <= 0) {
        usedSweep += sweepAngle;
        startAngle += sweepAngle;
        continue;
      }

      var color = dp.Color ?? colors[i % colors.Length];

      // Check if this segment is hovered
      var isHighlighted = context.HighlightedPointIndex == i;

      // Create donut segment path
      using (var path = new GraphicsPath()) {
        path.AddArc(outerRect, startAngle, drawSweep);
        path.AddArc(innerRect, startAngle + drawSweep, -drawSweep);
        path.CloseFigure();

        // Fill segment
        var fillColor = isHighlighted ? Lighten(color, 0.2f) : color;
        using (var brush = new SolidBrush(fillColor))
          g.FillPath(brush, path);

        // Draw border
        using (var pen = new Pen(Color.White, 2))
          g.DrawPath(pen, path);
      }

      // Calculate hit test region
      var midAngle = startAngle + sweepAngle / 2;
      var midRad = midAngle * Math.PI / 180;
      var hitRadius = (outerSize + innerSize) / 4;
      var hitX = centerX + (float)(Math.Cos(midRad) * hitRadius);
      var hitY = centerY + (float)(Math.Sin(midRad) * hitRadius);
      context.RegisterHitTestRect(dp, new RectangleF(hitX - 15, hitY - 15, 30, 30));

      // Draw labels
      if (context.ShowDataLabels) {
        var labelAngle = startAngle + drawSweep / 2;
        var labelRad = labelAngle * Math.PI / 180;
        var labelDist = outerSize / 2 + 20;
        var labelX = centerX + (float)(Math.Cos(labelRad) * labelDist);
        var labelY = centerY + (float)(Math.Sin(labelRad) * labelDist);

        var label = dp.Label ?? $"{dp.Y / total:P0}";
        var labelSize = g.MeasureString(label, context.Chart.Font);

        if (labelAngle > 90 && labelAngle < 270)
          labelX -= labelSize.Width;

        using var labelBrush = new SolidBrush(Color.Black);
        g.DrawString(label, context.Chart.Font, labelBrush, labelX, labelY - labelSize.Height / 2);
      }

      usedSweep += sweepAngle;
      startAngle += sweepAngle;
    }

    // Draw center text if available
    var centerText = $"{total:N0}";
    var centerTextSize = g.MeasureString(centerText, context.Chart.Font);
    if (centerTextSize.Width < innerSize * 0.8f) {
      using var centerBrush = new SolidBrush(Color.Black);
      g.DrawString(centerText, context.Chart.Font, centerBrush, centerX - centerTextSize.Width / 2, centerY - centerTextSize.Height / 2);
    }
  }

  /// <inheritdoc />
  public override IList<LegendItem> GetLegendItems(AdvancedChart chart) {
    var items = new List<LegendItem>();
    var series = chart.Series.FirstOrDefault();

    if (series == null)
      return items;

    var colors = this._GetColors(series.Points.Count);

    for (var i = 0; i < series.Points.Count; ++i) {
      var point = series.Points[i];
      if (point.Y <= 0)
        continue;

      items.Add(new LegendItem {
        Text = point.Label ?? $"Item {i + 1}",
        Color = point.Color ?? colors[i % colors.Length],
        SymbolType = LegendSymbolType.Rectangle,
        Visible = true,
        Tag = point
      });
    }

    return items;
  }

  private Color[] _GetColors(int count) {
    var baseColors = new[] {
      Color.FromArgb(31, 119, 180),
      Color.FromArgb(255, 127, 14),
      Color.FromArgb(44, 160, 44),
      Color.FromArgb(214, 39, 40),
      Color.FromArgb(148, 103, 189),
      Color.FromArgb(140, 86, 75),
      Color.FromArgb(227, 119, 194),
      Color.FromArgb(127, 127, 127),
      Color.FromArgb(188, 189, 34),
      Color.FromArgb(23, 190, 207)
    };

    return baseColors;
  }
}

/// <summary>
/// Renderer for nested donut (multi-level) charts.
/// </summary>
public class NestedDonutRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.NestedDonut;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    if (context.Series.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxSize = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.85f;

    var seriesCount = context.Series.Count;
    var ringWidth = maxSize / (seriesCount * 2 + 1);

    for (var seriesIndex = 0; seriesIndex < seriesCount; ++seriesIndex) {
      var series = context.Series[seriesIndex];
      if (series.Points.Count == 0)
        continue;

      var total = series.Points.Where(p => p.Y > 0).Sum(p => p.Y);
      if (total <= 0)
        continue;

      var outerRadius = maxSize / 2 - seriesIndex * ringWidth;
      var innerRadius = outerRadius - ringWidth;

      var outerRect = new RectangleF(centerX - outerRadius, centerY - outerRadius, outerRadius * 2, outerRadius * 2);
      var innerRect = new RectangleF(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);

      var startAngle = -90f;

      for (var i = 0; i < series.Points.Count; ++i) {
        var dp = series.Points[i];
        if (dp.Y <= 0)
          continue;

        var sweepAngle = (float)(dp.Y / total * 360 * context.AnimationProgress);
        var color = dp.Color ?? series.Color;

        using (var path = new GraphicsPath()) {
          path.AddArc(outerRect, startAngle, sweepAngle);
          path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
          path.CloseFigure();

          using (var brush = new SolidBrush(color))
            g.FillPath(brush, path);

          using (var pen = new Pen(Color.White, 1))
            g.DrawPath(pen, path);
        }

        // Hit test
        var midAngle = startAngle + sweepAngle / 2;
        var midRad = midAngle * Math.PI / 180;
        var hitRadius = (outerRadius + innerRadius) / 2;
        var hitX = centerX + (float)(Math.Cos(midRad) * hitRadius);
        var hitY = centerY + (float)(Math.Sin(midRad) * hitRadius);
        context.RegisterHitTestRect(dp, new RectangleF(hitX - 10, hitY - 10, 20, 20));

        startAngle += sweepAngle;
      }
    }
  }
}

/// <summary>Renders a semi-circle donut chart (half donut).</summary>
public class SemiCircleDonutRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.SemiCircleDonut;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var centerX = plotArea.Left + plotArea.Width / 2;
    var centerY = plotArea.Bottom - 20;
    var outerRadius = Math.Min(plotArea.Width / 2, plotArea.Height) - 20;
    var innerRadius = outerRadius * 0.6f;

    var points = series[0].Points;
    var total = points.Sum(p => Math.Abs(p.Y));
    if (total == 0)
      total = 1;

    // Apply animation to total sweep
    var animatedTotalSweep = 180f * (float)context.AnimationProgress;
    var startAngle = 180f;
    var usedSweep = 0f;

    foreach (var dp in points) {
      var sweepAngle = (float)(Math.Abs(dp.Y) / total * 180);

      // Apply animation
      var drawSweep = Math.Min(sweepAngle, animatedTotalSweep - usedSweep);
      if (drawSweep <= 0) {
        usedSweep += sweepAngle;
        startAngle += sweepAngle;
        continue;
      }

      var color = dp.Color ?? series[0].Color;

      using var path = new System.Drawing.Drawing2D.GraphicsPath();
      path.AddArc(centerX - outerRadius, centerY - outerRadius, outerRadius * 2, outerRadius * 2, startAngle, drawSweep);
      path.AddArc(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2, startAngle + drawSweep, -drawSweep);
      path.CloseFigure();

      using var brush = new SolidBrush(color);
      g.FillPath(brush, path);
      using var pen = new Pen(Color.White, 2);
      g.DrawPath(pen, path);

      usedSweep += sweepAngle;
      startAngle += sweepAngle;
    }
  }
}
