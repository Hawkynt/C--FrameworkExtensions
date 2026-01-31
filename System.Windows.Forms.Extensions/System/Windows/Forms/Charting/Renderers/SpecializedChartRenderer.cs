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
/// Renderer for funnel charts.
/// </summary>
public class FunnelChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Funnel;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Neck width as percentage of total width.
  /// </summary>
  public float NeckWidth { get; set; } = 0.3f;

  /// <summary>
  /// Neck height as percentage of total height.
  /// </summary>
  public float NeckHeight { get; set; } = 0.4f;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Use FunnelStages if available, otherwise use series data
    IList<(double Value, string Label, Color Color)> stages;

    var funnelStages = context.Chart.FunnelStages;
    if (funnelStages != null && funnelStages.Count > 0) {
      stages = funnelStages.Select(s => (s.Value, s.Label, s.Color ?? Color.Empty)).ToList();
    } else {
      var series = context.Series.FirstOrDefault();
      if (series == null || series.Points.Count == 0)
        return;
      stages = series.Points.Select(p => (p.Y, p.Label ?? p.Y.ToString("N0"), p.Color ?? series.Color)).ToList();
    }

    var stageCount = stages.Count;
    if (stageCount == 0)
      return;

    var maxValue = stages.Max(s => s.Value);
    if (maxValue <= 0)
      maxValue = 1;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var neckWidth = context.PlotArea.Width * this.NeckWidth;
    var neckStartY = context.PlotArea.Top + context.PlotArea.Height * (1 - this.NeckHeight);

    var totalHeight = context.PlotArea.Height;
    var stageHeight = totalHeight / stageCount;

    var colors = this._GetColors(stageCount);

    for (var i = 0; i < stageCount; ++i) {
      var stage = stages[i];
      var topY = context.PlotArea.Top + i * stageHeight;
      var bottomY = topY + stageHeight;

      // Calculate width based on value (proportional to max)
      var widthRatio = (float)(stage.Value / maxValue);

      // Determine if we're in funnel or neck section
      float topWidth, bottomWidth;

      if (topY >= neckStartY) {
        // In neck section - constant width
        topWidth = neckWidth;
        bottomWidth = neckWidth;
      } else if (bottomY <= neckStartY) {
        // Fully in funnel section
        var topProgress = (topY - context.PlotArea.Top) / (neckStartY - context.PlotArea.Top);
        var bottomProgress = (bottomY - context.PlotArea.Top) / (neckStartY - context.PlotArea.Top);

        topWidth = context.PlotArea.Width * widthRatio * (1 - topProgress * (1 - this.NeckWidth));
        bottomWidth = context.PlotArea.Width * widthRatio * (1 - bottomProgress * (1 - this.NeckWidth));
      } else {
        // Crossing from funnel to neck
        var topProgress = (topY - context.PlotArea.Top) / (neckStartY - context.PlotArea.Top);
        topWidth = context.PlotArea.Width * widthRatio * (1 - topProgress * (1 - this.NeckWidth));
        bottomWidth = neckWidth;
      }

      // Apply animation
      topWidth *= (float)context.AnimationProgress;
      bottomWidth *= (float)context.AnimationProgress;

      var color = stage.Color != Color.Empty ? stage.Color : colors[i % colors.Length];  // Non-nullable after tuple projection

      // Draw trapezoid
      var points = new PointF[] {
        new(centerX - topWidth / 2, topY),
        new(centerX + topWidth / 2, topY),
        new(centerX + bottomWidth / 2, bottomY),
        new(centerX - bottomWidth / 2, bottomY)
      };

      using (var brush = new SolidBrush(color))
        g.FillPolygon(brush, points);

      // Draw border
      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawPolygon(pen, points);

      // Draw label
      if (!string.IsNullOrEmpty(stage.Label)) {
        var labelY = topY + stageHeight / 2;
        var labelSize = g.MeasureString(stage.Label, context.Chart.Font);

        // Draw label to the right of the funnel
        using var brush = new SolidBrush(Color.Black);
        g.DrawString(stage.Label, context.Chart.Font, brush, centerX + Math.Max(topWidth, bottomWidth) / 2 + 10, labelY - labelSize.Height / 2);

        // Draw value
        var valueText = stage.Value.ToString("N0");
        var valueSize = g.MeasureString(valueText, context.Chart.Font);
        g.DrawString(valueText, context.Chart.Font, brush, centerX - valueSize.Width / 2, labelY - valueSize.Height / 2);
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(149, 165, 166),
    Color.FromArgb(52, 73, 94)
  };
}

/// <summary>
/// Renderer for pyramid charts.
/// </summary>
public class PyramidChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Pyramid;

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

    var stageCount = series.Points.Count;
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var totalHeight = context.PlotArea.Height;
    var stageHeight = totalHeight / stageCount;
    var maxWidth = context.PlotArea.Width * 0.9f;

    var colors = this._GetColors(stageCount);

    // Pyramid grows from top (narrow) to bottom (wide)
    for (var i = 0; i < stageCount; ++i) {
      var dp = series.Points[i];
      var topY = context.PlotArea.Top + i * stageHeight;
      var bottomY = topY + stageHeight;

      // Width increases linearly from top to bottom
      var topRatio = (float)(i) / stageCount;
      var bottomRatio = (float)(i + 1) / stageCount;

      var topWidth = maxWidth * topRatio * (float)context.AnimationProgress;
      var bottomWidth = maxWidth * bottomRatio * (float)context.AnimationProgress;

      var color = dp.Color ?? colors[i % colors.Length];

      // Draw trapezoid
      var points = new PointF[] {
        new(centerX - topWidth / 2, topY),
        new(centerX + topWidth / 2, topY),
        new(centerX + bottomWidth / 2, bottomY),
        new(centerX - bottomWidth / 2, bottomY)
      };

      using (var brush = new SolidBrush(color))
        g.FillPolygon(brush, points);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawPolygon(pen, points);

      context.RegisterHitTestRect(dp, new RectangleF(centerX - bottomWidth / 2, topY, bottomWidth, stageHeight));

      // Draw label
      var label = dp.Label ?? dp.Y.ToString("N0");
      var labelSize = g.MeasureString(label, context.Chart.Font);
      var labelY = topY + stageHeight / 2 - labelSize.Height / 2;

      using var brush2 = new SolidBrush(Color.Black);
      g.DrawString(label, context.Chart.Font, brush2, centerX + bottomWidth / 2 + 10, labelY);

      // Draw value in center if there's room
      if (bottomWidth > labelSize.Width + 10) {
        using var valueBrush = new SolidBrush(Color.White);
        g.DrawString(dp.Y.ToString("N0"), context.Chart.Font, valueBrush, centerX - labelSize.Width / 2, labelY);
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(52, 73, 94),
    Color.FromArgb(149, 165, 166)
  };
}

/// <summary>
/// Renderer for gauge/dial charts.
/// </summary>
public class GaugeChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Gauge;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Start angle in degrees (0 = right, 90 = bottom).
  /// </summary>
  public float StartAngle { get; set; } = 135;

  /// <summary>
  /// Sweep angle in degrees.
  /// </summary>
  public float SweepAngle { get; set; } = 270;

  /// <summary>
  /// Arc thickness as percentage of radius.
  /// </summary>
  public float ArcThickness { get; set; } = 0.15f;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var gaugeZones = context.Chart.GaugeZones;
    var series = context.Series.FirstOrDefault();

    double value = 0;
    double minValue = 0;
    double maxValue = 100;

    if (series != null && series.Points.Count > 0) {
      value = series.Points[0].Y;
      if (series.Points.Count > 1)
        minValue = series.Points.Min(p => p.Y);
      maxValue = series.Points.Max(p => p.Y);
      if (maxValue <= minValue)
        maxValue = minValue + 100;
    }

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2 + context.PlotArea.Height * 0.1f;
    var radius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.4f;
    var thickness = radius * this.ArcThickness;

    // Draw background arc
    using (var pen = new Pen(Color.FromArgb(230, 230, 230), thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round }) {
      var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
      g.DrawArc(pen, arcRect, this.StartAngle, this.SweepAngle);
    }

    // Draw gauge zones if defined
    if (gaugeZones != null && gaugeZones.Count > 0) {
      foreach (var zone in gaugeZones) {
        var zoneStartAngle = this.StartAngle + (float)((zone.Start - minValue) / (maxValue - minValue) * this.SweepAngle);
        var zoneEndAngle = this.StartAngle + (float)((zone.End - minValue) / (maxValue - minValue) * this.SweepAngle);
        var zoneSweep = zoneEndAngle - zoneStartAngle;

        using var pen = new Pen(zone.Color, thickness) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
        var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
        g.DrawArc(pen, arcRect, zoneStartAngle, zoneSweep);
      }
    }

    // Draw value arc
    var normalizedValue = (value - minValue) / (maxValue - minValue);
    normalizedValue = Math.Max(0, Math.Min(1, normalizedValue));
    normalizedValue *= context.AnimationProgress;

    var valueAngle = (float)(normalizedValue * this.SweepAngle);
    var valueColor = series?.Color ?? Color.FromArgb(52, 152, 219);

    using (var pen = new Pen(valueColor, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round }) {
      var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
      if (valueAngle > 0.5f)
        g.DrawArc(pen, arcRect, this.StartAngle, valueAngle);
    }

    // Draw needle
    var needleAngle = this.StartAngle + valueAngle;
    var needleRad = needleAngle * Math.PI / 180;
    var needleLength = radius * 0.85f;

    var needleTip = new PointF(
      centerX + (float)(Math.Cos(needleRad) * needleLength),
      centerY + (float)(Math.Sin(needleRad) * needleLength)
    );

    using (var pen = new Pen(Color.FromArgb(52, 73, 94), 3))
      g.DrawLine(pen, centerX, centerY, needleTip.X, needleTip.Y);

    // Draw center circle
    using (var brush = new SolidBrush(Color.FromArgb(52, 73, 94)))
      g.FillEllipse(brush, centerX - 8, centerY - 8, 16, 16);

    // Draw value text
    var valueText = value.ToString("N0");
    using (var font = new Font(context.Chart.Font.FontFamily, context.Chart.Font.Size * 2, FontStyle.Bold)) {
      var textSize = g.MeasureString(valueText, font);
      using var brush = new SolidBrush(Color.Black);
      g.DrawString(valueText, font, brush, centerX - textSize.Width / 2, centerY + radius * 0.3f);
    }

    // Draw min/max labels
    var minAngleRad = this.StartAngle * Math.PI / 180;
    var maxAngleRad = (this.StartAngle + this.SweepAngle) * Math.PI / 180;

    using (var brush = new SolidBrush(Color.Gray)) {
      var minText = minValue.ToString("N0");
      var maxText = maxValue.ToString("N0");

      var minPos = new PointF(
        centerX + (float)(Math.Cos(minAngleRad) * (radius + thickness)),
        centerY + (float)(Math.Sin(minAngleRad) * (radius + thickness))
      );

      var maxPos = new PointF(
        centerX + (float)(Math.Cos(maxAngleRad) * (radius + thickness)),
        centerY + (float)(Math.Sin(maxAngleRad) * (radius + thickness))
      );

      g.DrawString(minText, context.Chart.Font, brush, minPos.X - 20, minPos.Y);
      g.DrawString(maxText, context.Chart.Font, brush, maxPos.X, maxPos.Y);
    }
  }
}

/// <summary>
/// Renderer for treemap charts.
/// </summary>
public class TreemapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Treemap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Padding between rectangles.
  /// </summary>
  public int Padding { get; set; } = 2;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var hierarchicalData = context.Chart.HierarchicalData;
    if (hierarchicalData != null && hierarchicalData.Count > 0) {
      this._RenderFromHierarchicalData(context, hierarchicalData);
      return;
    }

    // Fall back to series data
    var series = context.Series.FirstOrDefault();
    if (series == null || series.Points.Count == 0)
      return;

    // Convert series points to rectangles using squarified treemap algorithm
    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var sortedPoints = series.Points.Where(p => p.Y > 0).OrderByDescending(p => p.Y).ToList();

    var colors = this._GetColors(sortedPoints.Count);
    var rects = this._Squarify(sortedPoints.Select(p => p.Y).ToList(), context.PlotArea, totalValue);

    for (var i = 0; i < sortedPoints.Count && i < rects.Count; ++i) {
      var dp = sortedPoints[i];
      var rect = rects[i];

      // Apply animation
      if (context.AnimationProgress < 1) {
        var centerX = rect.X + rect.Width / 2;
        var centerY = rect.Y + rect.Height / 2;
        rect = new RectangleF(
          centerX - rect.Width / 2 * (float)context.AnimationProgress,
          centerY - rect.Height / 2 * (float)context.AnimationProgress,
          rect.Width * (float)context.AnimationProgress,
          rect.Height * (float)context.AnimationProgress
        );
      }

      var color = dp.Color ?? colors[i % colors.Length];

      // Draw rectangle
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, rect);

      using (var pen = new Pen(Color.White, this.Padding))
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

      context.RegisterHitTestRect(dp, rect);

      // Draw label if there's room
      var label = dp.Label ?? dp.Y.ToString("N0");
      var labelSize = g.MeasureString(label, context.Chart.Font);
      if (rect.Width > labelSize.Width + 4 && rect.Height > labelSize.Height + 4) {
        using var brush = new SolidBrush(this._GetContrastColor(color));
        g.DrawString(label, context.Chart.Font, brush, rect.X + 4, rect.Y + 4);
      }
    }
  }

  private void _RenderFromHierarchicalData(ChartRenderContext context, IList<HierarchicalDataPoint> data) {
    var g = context.Graphics;

    // Flatten to top level only for now
    var totalValue = data.Sum(d => Math.Max(0, d.Value));
    if (totalValue <= 0)
      return;

    var sortedData = data.Where(d => d.Value > 0).OrderByDescending(d => d.Value).ToList();

    var colors = this._GetColors(sortedData.Count);
    var rects = this._Squarify(sortedData.Select(d => d.Value).ToList(), context.PlotArea, totalValue);

    for (var i = 0; i < sortedData.Count && i < rects.Count; ++i) {
      var item = sortedData[i];
      var rect = rects[i];

      // Apply animation
      if (context.AnimationProgress < 1) {
        var centerX = rect.X + rect.Width / 2;
        var centerY = rect.Y + rect.Height / 2;
        rect = new RectangleF(
          centerX - rect.Width / 2 * (float)context.AnimationProgress,
          centerY - rect.Height / 2 * (float)context.AnimationProgress,
          rect.Width * (float)context.AnimationProgress,
          rect.Height * (float)context.AnimationProgress
        );
      }

      var color = item.Color ?? colors[i % colors.Length];

      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, rect);

      using (var pen = new Pen(Color.White, this.Padding))
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

      var label = item.Label ?? item.Value.ToString("N0");
      var labelSize = g.MeasureString(label, context.Chart.Font);
      if (rect.Width > labelSize.Width + 4 && rect.Height > labelSize.Height + 4) {
        using var brush = new SolidBrush(this._GetContrastColor(color));
        g.DrawString(label, context.Chart.Font, brush, rect.X + 4, rect.Y + 4);
      }
    }
  }

  private List<RectangleF> _Squarify(IList<double> values, RectangleF bounds, double total) {
    var result = new List<RectangleF>();
    if (values.Count == 0 || total <= 0)
      return result;

    var remaining = new List<double>(values);
    var currentBounds = bounds;

    while (remaining.Count > 0) {
      var isHorizontal = currentBounds.Width >= currentBounds.Height;
      var side = isHorizontal ? currentBounds.Height : currentBounds.Width;

      var row = new List<double>();
      var rowSum = 0.0;
      var worst = double.MaxValue;

      foreach (var val in remaining.ToList()) {
        row.Add(val);
        rowSum += val;

        var newWorst = this._WorstRatio(row, side, rowSum, total, currentBounds);
        if (newWorst > worst) {
          row.RemoveAt(row.Count - 1);
          rowSum -= val;
          break;
        }

        remaining.RemoveAt(0);
        worst = newWorst;
      }

      // Layout the row
      var rowBounds = this._LayoutRow(row, currentBounds, isHorizontal, rowSum, total);
      result.AddRange(rowBounds);

      // Update remaining bounds
      var rowSize = (float)(rowSum / total * (isHorizontal ? currentBounds.Width : currentBounds.Height));
      if (isHorizontal)
        currentBounds = new RectangleF(currentBounds.X + rowSize, currentBounds.Y, currentBounds.Width - rowSize, currentBounds.Height);
      else
        currentBounds = new RectangleF(currentBounds.X, currentBounds.Y + rowSize, currentBounds.Width, currentBounds.Height - rowSize);
    }

    return result;
  }

  private double _WorstRatio(List<double> row, float side, double rowSum, double total, RectangleF bounds) {
    if (row.Count == 0)
      return double.MaxValue;

    var areaFraction = rowSum / total;
    var rowLength = side;
    var rowWidth = (float)(areaFraction * (bounds.Width >= bounds.Height ? bounds.Width : bounds.Height));

    if (rowWidth <= 0)
      return double.MaxValue;

    var worst = 0.0;
    foreach (var val in row) {
      var itemArea = val / total * bounds.Width * bounds.Height;
      var itemHeight = itemArea / rowWidth;
      var ratio = Math.Max(rowWidth / itemHeight, itemHeight / rowWidth);
      worst = Math.Max(worst, ratio);
    }

    return worst;
  }

  private List<RectangleF> _LayoutRow(List<double> row, RectangleF bounds, bool isHorizontal, double rowSum, double total) {
    var result = new List<RectangleF>();
    if (row.Count == 0)
      return result;

    var rowSize = (float)(rowSum / total * (isHorizontal ? bounds.Width : bounds.Height));
    var offset = 0f;

    foreach (var val in row) {
      var itemSize = (float)(val / rowSum * (isHorizontal ? bounds.Height : bounds.Width));

      RectangleF rect;
      if (isHorizontal)
        rect = new RectangleF(bounds.X, bounds.Y + offset, rowSize, itemSize);
      else
        rect = new RectangleF(bounds.X + offset, bounds.Y, itemSize, rowSize);

      result.Add(rect);
      offset += itemSize;
    }

    return result;
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94)
  };
}

/// <summary>
/// Renderer for sunburst charts.
/// </summary>
public class SunburstRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Sunburst;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    IList<HierarchicalDataPoint> hierarchicalData = context.Chart.HierarchicalData;
    if (hierarchicalData == null || hierarchicalData.Count == 0) {
      // Fall back to simple pie-like rendering
      var series = context.Series.FirstOrDefault();
      if (series == null || series.Points.Count == 0)
        return;

      // Convert to single-ring sunburst
      hierarchicalData = series.Points.Select(p => new HierarchicalDataPoint {
        Id = p.Label ?? p.X.ToString(),
        Label = p.Label ?? p.Y.ToString("N0"),
        Value = p.Y,
        Color = p.Color ?? series.Color
      }).ToList();
    }

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;
    var innerRadius = maxRadius * 0.2f;

    // Calculate depth of hierarchy
    var maxDepth = this._GetMaxDepth(hierarchicalData);
    var ringWidth = (maxRadius - innerRadius) / maxDepth;

    // Render recursively
    var totalValue = hierarchicalData.Sum(d => Math.Max(0, d.Value));
    if (totalValue <= 0)
      return;

    var colors = this._GetColors(hierarchicalData.Count);
    var startAngle = -90f;

    for (var i = 0; i < hierarchicalData.Count; ++i) {
      var item = hierarchicalData[i];
      var sweepAngle = (float)(item.Value / totalValue * 360);
      var color = item.Color ?? colors[i % colors.Length];

      this._DrawSunburstSegment(g, context, centerX, centerY, innerRadius, ringWidth, startAngle, sweepAngle, color, item.Label, 0, maxDepth);

      // Recursively draw children
      if (item.Children != null && item.Children.Count > 0)
        this._DrawSunburstChildren(g, context, centerX, centerY, innerRadius, ringWidth, startAngle, sweepAngle, item.Children.ToArray(), 1, maxDepth, color);

      startAngle += sweepAngle;
    }
  }

  private void _DrawSunburstSegment(Graphics g, ChartRenderContext context, float centerX, float centerY, float innerRadius, float ringWidth, float startAngle, float sweepAngle, Color color, string label, int depth, int maxDepth) {
    var outerRadius = innerRadius + ringWidth * (depth + 1);
    var segmentInnerRadius = innerRadius + ringWidth * depth;

    // Apply animation
    sweepAngle *= (float)context.AnimationProgress;

    if (sweepAngle < 0.5f)
      return;

    // Draw arc segment
    using var path = new GraphicsPath();
    path.AddArc(centerX - outerRadius, centerY - outerRadius, outerRadius * 2, outerRadius * 2, startAngle, sweepAngle);
    path.AddArc(centerX - segmentInnerRadius, centerY - segmentInnerRadius, segmentInnerRadius * 2, segmentInnerRadius * 2, startAngle + sweepAngle, -sweepAngle);
    path.CloseFigure();

    using (var brush = new SolidBrush(color))
      g.FillPath(brush, path);

    using (var pen = new Pen(Color.White, 1))
      g.DrawPath(pen, path);

    // Draw label if there's room
    if (sweepAngle > 20 && !string.IsNullOrEmpty(label)) {
      var midAngle = startAngle + sweepAngle / 2;
      var midRadius = (segmentInnerRadius + outerRadius) / 2;
      var labelX = centerX + (float)(Math.Cos(midAngle * Math.PI / 180) * midRadius);
      var labelY = centerY + (float)(Math.Sin(midAngle * Math.PI / 180) * midRadius);

      var labelSize = g.MeasureString(label, context.Chart.Font);
      using var brush = new SolidBrush(this._GetContrastColor(color));
      g.DrawString(label, context.Chart.Font, brush, labelX - labelSize.Width / 2, labelY - labelSize.Height / 2);
    }
  }

  private void _DrawSunburstChildren(Graphics g, ChartRenderContext context, float centerX, float centerY, float innerRadius, float ringWidth, float parentStartAngle, float parentSweepAngle, HierarchicalDataPoint[] children, int depth, int maxDepth, Color parentColor) {
    var totalValue = children.Sum(c => Math.Max(0, c.Value));
    if (totalValue <= 0)
      return;

    var startAngle = parentStartAngle;

    for (var i = 0; i < children.Length; ++i) {
      var child = children[i];
      var sweepAngle = (float)(child.Value / totalValue * parentSweepAngle);
      var color = child.Color ?? Lighten(parentColor, 0.2f * depth);

      this._DrawSunburstSegment(g, context, centerX, centerY, innerRadius, ringWidth, startAngle, sweepAngle, color, child.Label, depth, maxDepth);

      if (child.Children != null && child.Children.Count > 0 && depth < maxDepth - 1)
        this._DrawSunburstChildren(g, context, centerX, centerY, innerRadius, ringWidth, startAngle, sweepAngle, child.Children.ToArray(), depth + 1, maxDepth, color);

      startAngle += sweepAngle;
    }
  }

  private int _GetMaxDepth(IList<HierarchicalDataPoint> data) {
    var maxDepth = 1;
    foreach (var item in data)
      if (item.Children != null && item.Children.Count > 0)
        maxDepth = Math.Max(maxDepth, 1 + this._GetMaxDepth(item.Children));
    return maxDepth;
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94)
  };
}

/// <summary>
/// Renderer for heatmap charts.
/// </summary>
public class HeatmapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Heatmap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Low value color.
  /// </summary>
  public Color LowColor { get; set; } = Color.FromArgb(255, 255, 255);

  /// <summary>
  /// High value color.
  /// </summary>
  public Color HighColor { get; set; } = Color.FromArgb(231, 76, 60);

  /// <summary>
  /// Cell padding.
  /// </summary>
  public int CellPadding { get; set; } = 1;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var heatmapData = context.Chart.HeatmapData;
    if (heatmapData == null || heatmapData.Count == 0)
      return;

    // Find grid dimensions
    var minRow = heatmapData.Min(c => c.Row);
    var maxRow = heatmapData.Max(c => c.Row);
    var minCol = heatmapData.Min(c => c.Column);
    var maxCol = heatmapData.Max(c => c.Column);
    var minValue = heatmapData.Min(c => c.Value);
    var maxValue = heatmapData.Max(c => c.Value);

    var rows = maxRow - minRow + 1;
    var cols = maxCol - minCol + 1;

    var cellWidth = context.PlotArea.Width / cols;
    var cellHeight = context.PlotArea.Height / rows;

    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    // Create lookup for quick access
    var dataLookup = heatmapData.ToDictionary(c => (c.Row, c.Column), c => c);

    for (var row = minRow; row <= maxRow; ++row) {
      for (var col = minCol; col <= maxCol; ++col) {
        var x = context.PlotArea.Left + (col - minCol) * cellWidth;
        var y = context.PlotArea.Top + (row - minRow) * cellHeight;

        Color cellColor;
        string label = null;

        if (dataLookup.TryGetValue((row, col), out var cell)) {
          var normalizedValue = (cell.Value - minValue) / valueRange;
          normalizedValue *= context.AnimationProgress;
          cellColor = cell.Color ?? InterpolateColor(this.LowColor, this.HighColor, (float)normalizedValue);
          label = cell.Label;
        } else
          cellColor = this.LowColor;

        var cellRect = new RectangleF(x + this.CellPadding, y + this.CellPadding, cellWidth - this.CellPadding * 2, cellHeight - this.CellPadding * 2);

        using (var brush = new SolidBrush(cellColor))
          g.FillRectangle(brush, cellRect);

        // Draw label if provided and fits
        if (!string.IsNullOrEmpty(label)) {
          var labelSize = g.MeasureString(label, context.Chart.Font);
          if (labelSize.Width < cellRect.Width && labelSize.Height < cellRect.Height) {
            using var brush = new SolidBrush(this._GetContrastColor(cellColor));
            g.DrawString(label, context.Chart.Font, brush, cellRect.X + (cellRect.Width - labelSize.Width) / 2, cellRect.Y + (cellRect.Height - labelSize.Height) / 2);
          }
        }
      }
    }

    // Draw color scale legend
    this._DrawColorScale(g, context, minValue, maxValue);
  }

  private void _DrawColorScale(Graphics g, ChartRenderContext context, double minValue, double maxValue) {
    var scaleWidth = 20f;
    var scaleHeight = context.PlotArea.Height * 0.6f;
    var scaleX = context.PlotArea.Right + 20;
    var scaleY = context.PlotArea.Top + (context.PlotArea.Height - scaleHeight) / 2;

    // Draw gradient
    for (var i = 0; i < scaleHeight; ++i) {
      var normalizedValue = 1 - i / scaleHeight;
      var color = InterpolateColor(this.LowColor, this.HighColor, (float)normalizedValue);
      using var pen = new Pen(color);
      g.DrawLine(pen, scaleX, scaleY + i, scaleX + scaleWidth, scaleY + i);
    }

    // Draw border
    using (var pen = new Pen(Color.Gray))
      g.DrawRectangle(pen, scaleX, scaleY, scaleWidth, scaleHeight);

    // Draw labels
    using (var brush = new SolidBrush(Color.Black)) {
      g.DrawString(maxValue.ToString("N1"), context.Chart.Font, brush, scaleX + scaleWidth + 5, scaleY - 5);
      g.DrawString(minValue.ToString("N1"), context.Chart.Font, brush, scaleX + scaleWidth + 5, scaleY + scaleHeight - 10);
    }
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
  }
}

/// <summary>
/// Renderer for word cloud charts.
/// </summary>
public class WordCloudRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.WordCloud;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Minimum font size.
  /// </summary>
  public float MinFontSize { get; set; } = 10;

  /// <summary>
  /// Maximum font size.
  /// </summary>
  public float MaxFontSize { get; set; } = 60;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    IList<WordCloudWord> wordCloudData = context.Chart.WordCloudData;
    if (wordCloudData == null || wordCloudData.Count == 0) {
      // Fall back to series data (use X as frequency, Label as word)
      var series = context.Series.FirstOrDefault();
      if (series == null || series.Points.Count == 0)
        return;

      wordCloudData = series.Points.Select(p => new WordCloudWord {
        Text = p.Label ?? $"Word{p.X}",
        Weight = p.Y,
        Color = p.Color ?? series.Color
      }).ToList();
    }

    // Sort by weight (largest first)
    var sortedWords = wordCloudData.OrderByDescending(w => w.Weight).ToList();
    var maxWeight = sortedWords.First().Weight;
    var minWeight = sortedWords.Last().Weight;
    var weightRange = maxWeight - minWeight;
    if (weightRange <= 0)
      weightRange = 1;

    var colors = this._GetColors(sortedWords.Count);
    var placedRects = new List<RectangleF>();
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var random = new Random(42); // Consistent layout

    for (var i = 0; i < sortedWords.Count; ++i) {
      var word = sortedWords[i];

      // Calculate font size based on weight
      var normalizedWeight = (word.Weight - minWeight) / weightRange;
      var fontSize = this.MinFontSize + (float)(normalizedWeight * (this.MaxFontSize - this.MinFontSize));
      fontSize *= (float)context.AnimationProgress;

      if (fontSize < this.MinFontSize)
        continue;

      using var font = new Font(context.Chart.Font.FontFamily, fontSize, FontStyle.Bold);
      var textSize = g.MeasureString(word.Text, font);

      // Find position using spiral placement
      var placed = false;
      var angle = 0.0;
      var radius = 0.0;

      while (!placed && radius < Math.Max(context.PlotArea.Width, context.PlotArea.Height)) {
        var x = centerX + (float)(Math.Cos(angle) * radius) - textSize.Width / 2;
        var y = centerY + (float)(Math.Sin(angle) * radius) - textSize.Height / 2;

        var rect = new RectangleF(x, y, textSize.Width, textSize.Height);

        // Check bounds
        if (rect.Left >= context.PlotArea.Left && rect.Right <= context.PlotArea.Right &&
            rect.Top >= context.PlotArea.Top && rect.Bottom <= context.PlotArea.Bottom) {
          // Check overlap with placed words
          var overlaps = placedRects.Any(r => r.IntersectsWith(rect));

          if (!overlaps) {
            var color = word.Color ?? colors[i % colors.Length];

            using var brush = new SolidBrush(color);
            g.DrawString(word.Text, font, brush, x, y);

            placedRects.Add(rect);
            placed = true;
          }
        }

        angle += 0.5;
        radius += 2;
      }
    }
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94)
  };
}

/// <summary>
/// Renderer for Venn diagram charts.
/// </summary>
public class VennDiagramRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Venn;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Venn diagram typically shows 2-3 overlapping sets
    // For simplicity, we'll render based on series count
    var seriesCount = Math.Min(context.Series.Count, 3);
    if (seriesCount == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.8f;

    var colors = new[] {
      Color.FromArgb(100, 52, 152, 219),
      Color.FromArgb(100, 231, 76, 60),
      Color.FromArgb(100, 46, 204, 113)
    };

    // Calculate circle positions
    var circleRadius = maxRadius * 0.7f * (float)context.AnimationProgress;
    var offset = maxRadius * 0.35f;

    var positions = seriesCount switch {
      1 => new[] { new PointF(centerX, centerY) },
      2 => new[] {
        new PointF(centerX - offset, centerY),
        new PointF(centerX + offset, centerY)
      },
      _ => new[] {
        new PointF(centerX, centerY - offset * 0.8f),
        new PointF(centerX - offset, centerY + offset * 0.5f),
        new PointF(centerX + offset, centerY + offset * 0.5f)
      }
    };

    // Draw circles
    for (var i = 0; i < seriesCount; ++i) {
      var series = context.Series[i];
      var pos = positions[i];
      var color = series.Color.A > 0 ? Color.FromArgb(100, series.Color) : colors[i];

      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, pos.X - circleRadius, pos.Y - circleRadius, circleRadius * 2, circleRadius * 2);

      using (var pen = new Pen(Color.FromArgb(200, color.R, color.G, color.B), 2))
        g.DrawEllipse(pen, pos.X - circleRadius, pos.Y - circleRadius, circleRadius * 2, circleRadius * 2);

      // Draw label
      var label = series.Name ?? $"Set {i + 1}";
      var labelSize = g.MeasureString(label, context.Chart.Font);

      // Position label outside the circle
      var labelOffset = circleRadius + 10;
      var labelAngle = seriesCount == 1 ? -90 : (i == 0 ? -90 : (i == 1 && seriesCount == 2 ? 90 : (i == 1 ? -150 : -30)));
      var labelRad = labelAngle * Math.PI / 180;
      var labelX = pos.X + (float)(Math.Cos(labelRad) * labelOffset) - labelSize.Width / 2;
      var labelY = pos.Y + (float)(Math.Sin(labelRad) * labelOffset) - labelSize.Height / 2;

      using var brush2 = new SolidBrush(Color.Black);
      g.DrawString(label, context.Chart.Font, brush2, labelX, labelY);

      // Draw value if available
      if (series.Points.Count > 0) {
        var valueText = series.Points[0].Y.ToString("N0");
        var valueSize = g.MeasureString(valueText, context.Chart.Font);
        using var valueBrush = new SolidBrush(Color.DimGray);
        g.DrawString(valueText, context.Chart.Font, valueBrush, pos.X - valueSize.Width / 2, pos.Y - valueSize.Height / 2);
      }
    }
  }
}

/// <summary>
/// Renderer for icon array charts.
/// </summary>
public class IconArrayRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.IconArray;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Number of icons per row.
  /// </summary>
  public int IconsPerRow { get; set; } = 10;

  /// <summary>
  /// Total number of icons.
  /// </summary>
  public int TotalIcons { get; set; } = 100;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var rows = (int)Math.Ceiling((double)this.TotalIcons / this.IconsPerRow);
    var iconWidth = context.PlotArea.Width / this.IconsPerRow;
    var iconHeight = context.PlotArea.Height / rows;
    var iconSize = Math.Min(iconWidth, iconHeight) * 0.8f;

    // Calculate how many icons each category gets
    var iconCounts = new List<(ChartPoint Point, int Count)>();
    var remainingIcons = this.TotalIcons;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var count = (int)Math.Round(dp.Y / totalValue * this.TotalIcons);
      if (i == series.Points.Count - 1)
        count = remainingIcons; // Last category gets remaining
      else
        count = Math.Min(count, remainingIcons);

      iconCounts.Add((dp, count));
      remainingIcons -= count;
    }

    // Draw icons
    var iconIndex = 0;
    var colors = this._GetColors(series.Points.Count);

    foreach (var (dp, count) in iconCounts) {
      var color = dp.Color ?? colors[iconCounts.IndexOf((dp, count)) % colors.Length];

      for (var c = 0; c < count; ++c) {
        var row = iconIndex / this.IconsPerRow;
        var col = iconIndex % this.IconsPerRow;

        var x = context.PlotArea.Left + col * iconWidth + (iconWidth - iconSize) / 2;
        var y = context.PlotArea.Top + row * iconHeight + (iconHeight - iconSize) / 2;

        // Apply animation (fade in from top-left)
        var animationIndex = (float)iconIndex / this.TotalIcons;
        if (animationIndex > context.AnimationProgress) {
          ++iconIndex;
          continue;
        }

        // Draw a person icon (simplified)
        this._DrawPersonIcon(g, x, y, iconSize, color);
        ++iconIndex;
      }
    }

    // Draw legend
    var legendY = context.PlotArea.Bottom + 20;
    var legendX = context.PlotArea.Left;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var color = dp.Color ?? colors[i % colors.Length];
      var label = dp.Label ?? dp.Y.ToString("N0");

      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, legendX, legendY, 15, 15);

      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(label, context.Chart.Font, brush, legendX + 20, legendY);

      var labelSize = g.MeasureString(label, context.Chart.Font);
      legendX += 30 + (int)labelSize.Width + 10;
    }
  }

  private void _DrawPersonIcon(Graphics g, float x, float y, float size, Color color) {
    var headSize = size * 0.3f;
    var bodyHeight = size * 0.5f;

    // Head
    using (var brush = new SolidBrush(color))
      g.FillEllipse(brush, x + size / 2 - headSize / 2, y, headSize, headSize);

    // Body (simplified triangle)
    var bodyTop = y + headSize + 2;
    var bodyPoints = new PointF[] {
      new(x + size / 2, bodyTop),
      new(x + size * 0.2f, bodyTop + bodyHeight),
      new(x + size * 0.8f, bodyTop + bodyHeight)
    };
    using (var brush = new SolidBrush(color))
      g.FillPolygon(brush, bodyPoints);
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
/// Renderer for waffle charts.
/// </summary>
public class WaffleChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Waffle;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <summary>
  /// Grid size (cells per row/column).
  /// </summary>
  public int GridSize { get; set; } = 10;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var totalCells = this.GridSize * this.GridSize;
    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var cellWidth = context.PlotArea.Width / this.GridSize;
    var cellHeight = context.PlotArea.Height / this.GridSize;
    var cellSize = Math.Min(cellWidth, cellHeight) * 0.9f;
    var cellGap = Math.Min(cellWidth, cellHeight) * 0.1f;

    // Calculate cells per category
    var cellAssignments = new int[totalCells];
    var cellIndex = 0;

    var colors = this._GetColors(series.Points.Count);

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var cellCount = (int)Math.Round(dp.Y / totalValue * totalCells);
      if (i == series.Points.Count - 1)
        cellCount = totalCells - cellIndex;

      for (var c = 0; c < cellCount && cellIndex < totalCells; ++c)
        cellAssignments[cellIndex++] = i;
    }

    // Draw cells
    for (var row = 0; row < this.GridSize; ++row) {
      for (var col = 0; col < this.GridSize; ++col) {
        var index = row * this.GridSize + col;
        var x = context.PlotArea.Left + col * cellWidth + cellGap / 2;
        var y = context.PlotArea.Top + row * cellHeight + cellGap / 2;

        // Apply animation
        var animationIndex = (float)index / totalCells;
        var alpha = animationIndex <= context.AnimationProgress ? 255 : 0;

        if (alpha == 0)
          continue;

        var categoryIndex = cellAssignments[index];
        var color = series.Points[categoryIndex].Color ?? colors[categoryIndex % colors.Length];

        using (var brush = new SolidBrush(color))
          FillRoundedRectangle(g, brush, new RectangleF(x, y, cellSize, cellSize), 2);
      }
    }

    // Draw legend
    var legendY = context.PlotArea.Bottom + 20;
    var legendX = context.PlotArea.Left;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var color = dp.Color ?? colors[i % colors.Length];
      var percentage = dp.Y / totalValue * 100;
      var label = $"{dp.Label ?? i.ToString()}: {percentage:0}%";

      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, legendX, legendY, 15, 15);

      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(label, context.Chart.Font, brush, legendX + 20, legendY);

      var labelSize = g.MeasureString(label, context.Chart.Font);
      legendX += 30 + (int)labelSize.Width + 10;
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
/// Renderer for icicle charts (horizontal hierarchical visualization).
/// </summary>
public class IcicleChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Icicle;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    IList<HierarchicalDataPoint> hierarchicalData = context.Chart.HierarchicalData;
    if (hierarchicalData == null || hierarchicalData.Count == 0) {
      var series = context.Series.FirstOrDefault();
      if (series == null || series.Points.Count == 0)
        return;

      hierarchicalData = series.Points.Select(p => new HierarchicalDataPoint {
        Id = p.Label ?? p.X.ToString(),
        Label = p.Label ?? p.Y.ToString("N0"),
        Value = p.Y,
        Color = p.Color ?? series.Color
      }).ToList();
    }

    // Calculate depth
    var maxDepth = this._GetMaxDepth(hierarchicalData);
    var layerWidth = context.PlotArea.Width / maxDepth;

    // Draw from left to right (root on left)
    var totalValue = hierarchicalData.Sum(d => Math.Max(0, d.Value));
    if (totalValue <= 0)
      return;

    var colors = this._GetColors(hierarchicalData.Count);
    var currentY = context.PlotArea.Top;

    for (var i = 0; i < hierarchicalData.Count; ++i) {
      var item = hierarchicalData[i];
      var itemHeight = (float)(item.Value / totalValue * context.PlotArea.Height);
      var color = item.Color ?? colors[i % colors.Length];

      this._DrawIcicleSegment(g, context, context.PlotArea.Left, currentY, layerWidth, itemHeight, color, item.Label, 0, maxDepth);

      if (item.Children != null && item.Children.Count > 0)
        this._DrawIcicleChildren(g, context, context.PlotArea.Left, currentY, layerWidth, itemHeight, item.Children.ToArray(), 1, maxDepth, color);

      currentY += itemHeight;
    }
  }

  private void _DrawIcicleSegment(Graphics g, ChartRenderContext context, float x, float y, float width, float height, Color color, string label, int depth, int maxDepth) {
    var animatedWidth = width * (float)context.AnimationProgress;

    if (animatedWidth < 1 || height < 1)
      return;

    var rect = new RectangleF(x + depth * width, y, animatedWidth, height);

    using (var brush = new SolidBrush(color))
      g.FillRectangle(brush, rect);

    using (var pen = new Pen(Color.White, 1))
      g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

    if (!string.IsNullOrEmpty(label) && rect.Width > 20 && rect.Height > 12) {
      var labelSize = g.MeasureString(label, context.Chart.Font);
      if (labelSize.Width < rect.Width - 4) {
        using var brush = new SolidBrush(this._GetContrastColor(color));
        g.DrawString(label, context.Chart.Font, brush, rect.X + 2, rect.Y + (rect.Height - labelSize.Height) / 2);
      }
    }
  }

  private void _DrawIcicleChildren(Graphics g, ChartRenderContext context, float parentX, float parentY, float layerWidth, float parentHeight, HierarchicalDataPoint[] children, int depth, int maxDepth, Color parentColor) {
    var totalValue = children.Sum(c => Math.Max(0, c.Value));
    if (totalValue <= 0)
      return;

    var currentY = parentY;
    for (var i = 0; i < children.Length; ++i) {
      var child = children[i];
      var childHeight = (float)(child.Value / totalValue * parentHeight);
      var color = child.Color ?? Lighten(parentColor, 0.15f * depth);

      this._DrawIcicleSegment(g, context, parentX, currentY, layerWidth, childHeight, color, child.Label, depth, maxDepth);

      if (child.Children != null && child.Children.Count > 0 && depth < maxDepth - 1)
        this._DrawIcicleChildren(g, context, parentX, currentY, layerWidth, childHeight, child.Children.ToArray(), depth + 1, maxDepth, color);

      currentY += childHeight;
    }
  }

  private int _GetMaxDepth(IList<HierarchicalDataPoint> data) {
    var maxDepth = 1;
    foreach (var item in data)
      if (item.Children != null && item.Children.Count > 0)
        maxDepth = Math.Max(maxDepth, 1 + this._GetMaxDepth(item.Children));
    return maxDepth;
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
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
/// Renderer for mosaic/Marimekko charts (variable-width stacked bar charts).
/// </summary>
public class MosaicChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Mosaic;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series;

    if (series.Count == 0)
      return;

    // Each series represents a column, points in each series represent segments
    var totalWidth = series.Sum(s => s.Points.Sum(p => Math.Max(0, p.Y)));
    if (totalWidth <= 0)
      return;

    var colors = this._GetColors(series.Count * 10);
    var currentX = context.PlotArea.Left;
    var colorIndex = 0;

    for (var si = 0; si < series.Count; ++si) {
      var s = series[si];
      var columnTotal = s.Points.Sum(p => Math.Max(0, p.Y));
      var columnWidth = (float)(columnTotal / totalWidth * context.PlotArea.Width) * (float)context.AnimationProgress;

      if (columnWidth < 1)
        continue;

      var currentY = context.PlotArea.Top;
      var segmentTotal = s.Points.Sum(p => Math.Max(0, p.Y));
      if (segmentTotal <= 0) {
        currentX += columnWidth;
        continue;
      }

      for (var pi = 0; pi < s.Points.Count; ++pi) {
        var dp = s.Points[pi];
        var segmentHeight = (float)(dp.Y / segmentTotal * context.PlotArea.Height);

        if (segmentHeight < 1) {
          currentY += segmentHeight;
          continue;
        }

        var color = dp.Color ?? colors[colorIndex % colors.Length];
        var rect = new RectangleF(currentX, currentY, columnWidth, segmentHeight);

        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        using (var pen = new Pen(Color.White, 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        context.RegisterHitTestRect(dp, rect);

        if (rect.Width > 30 && rect.Height > 15 && !string.IsNullOrEmpty(dp.Label)) {
          var labelSize = g.MeasureString(dp.Label, context.Chart.Font);
          if (labelSize.Width < rect.Width - 4) {
            using var brush = new SolidBrush(this._GetContrastColor(color));
            g.DrawString(dp.Label, context.Chart.Font, brush,
              rect.X + (rect.Width - labelSize.Width) / 2,
              rect.Y + (rect.Height - labelSize.Height) / 2);
          }
        }

        currentY += segmentHeight;
        ++colorIndex;
      }

      // Draw column label
      var columnLabel = s.Name ?? $"Column {si + 1}";
      var columnLabelSize = g.MeasureString(columnLabel, context.Chart.Font);
      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(columnLabel, context.Chart.Font, brush,
          currentX + (columnWidth - columnLabelSize.Width) / 2,
          context.PlotArea.Bottom + 5);

      currentX += columnWidth;
    }
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
  }

  private Color[] _GetColors(int count) => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94),
    Color.FromArgb(149, 165, 166),
    Color.FromArgb(127, 140, 141)
  };
}

/// <summary>
/// Renderer for parliament/hemicycle charts.
/// </summary>
public class ParliamentChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Parliament;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Bottom - 20;
    var maxRadius = Math.Min(context.PlotArea.Width / 2, context.PlotArea.Height - 40) * 0.95f;
    var minRadius = maxRadius * 0.3f;

    // Calculate total seats
    var totalSeats = (int)series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalSeats <= 0)
      return;

    // Determine number of rows
    var numRows = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(totalSeats / 10.0)));
    var rowSpacing = (maxRadius - minRadius) / numRows;

    var colors = this._GetColors(series.Points.Count);
    var seatColors = new List<Color>();

    // Build seat color list
    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var seatCount = (int)Math.Max(0, dp.Y);
      var color = dp.Color ?? colors[i % colors.Length];
      for (var s = 0; s < seatCount; ++s)
        seatColors.Add(color);
    }

    // Distribute seats across rows
    var seatIndex = 0;
    for (var row = 0; row < numRows && seatIndex < seatColors.Count; ++row) {
      var rowRadius = minRadius + row * rowSpacing + rowSpacing / 2;
      var circumference = Math.PI * rowRadius;
      var seatsInRow = (int)(circumference / (rowSpacing * 0.9));
      seatsInRow = Math.Min(seatsInRow, seatColors.Count - seatIndex);

      if (seatsInRow <= 0)
        continue;

      var seatSize = (float)(rowSpacing * 0.7) * (float)context.AnimationProgress;
      var angleStep = 180.0 / (seatsInRow + 1);

      for (var s = 0; s < seatsInRow && seatIndex < seatColors.Count; ++s) {
        var angle = 180 - (s + 1) * angleStep;
        var rad = angle * Math.PI / 180;
        var x = centerX + (float)(Math.Cos(rad) * rowRadius);
        var y = centerY - (float)(Math.Sin(rad) * rowRadius);

        using (var brush = new SolidBrush(seatColors[seatIndex]))
          g.FillEllipse(brush, x - seatSize / 2, y - seatSize / 2, seatSize, seatSize);

        ++seatIndex;
      }
    }

    // Draw legend
    var legendX = context.PlotArea.Left;
    var legendY = context.PlotArea.Bottom + 10;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var color = dp.Color ?? colors[i % colors.Length];
      var label = $"{dp.Label ?? i.ToString()}: {(int)dp.Y}";

      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, legendX, legendY, 12, 12);

      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(label, context.Chart.Font, brush, legendX + 16, legendY - 2);

      var labelSize = g.MeasureString(label, context.Chart.Font);
      legendX += 20 + (int)labelSize.Width + 10;
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
/// Renderer for unit charts (similar to icon arrays but simpler squares).
/// </summary>
public class UnitChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Unit;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Number of units per row.
  /// </summary>
  public int UnitsPerRow { get; set; } = 10;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var totalUnits = (int)totalValue;
    var rows = (int)Math.Ceiling((double)totalUnits / this.UnitsPerRow);
    var unitWidth = context.PlotArea.Width / this.UnitsPerRow;
    var unitHeight = context.PlotArea.Height / rows;
    var unitSize = Math.Min(unitWidth, unitHeight) * 0.85f;
    var padding = Math.Min(unitWidth, unitHeight) * 0.15f / 2;

    var colors = this._GetColors(series.Points.Count);
    var unitIndex = 0;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var unitCount = (int)Math.Max(0, dp.Y);
      var color = dp.Color ?? colors[i % colors.Length];

      for (var u = 0; u < unitCount && unitIndex < totalUnits; ++u) {
        var row = unitIndex / this.UnitsPerRow;
        var col = unitIndex % this.UnitsPerRow;

        var x = context.PlotArea.Left + col * unitWidth + padding;
        var y = context.PlotArea.Top + row * unitHeight + padding;

        // Apply animation
        var animationIndex = (float)unitIndex / totalUnits;
        if (animationIndex > context.AnimationProgress) {
          ++unitIndex;
          continue;
        }

        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, x, y, unitSize, unitSize);

        ++unitIndex;
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
/// Renderer for pictogram charts (icons representing data).
/// </summary>
public class PictogramRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Pictogram;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Number of icons per row.
  /// </summary>
  public int IconsPerRow { get; set; } = 5;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var maxValue = series.Points.Max(p => p.Y);
    if (maxValue <= 0)
      return;

    var colors = this._GetColors(series.Points.Count);
    var rowHeight = context.PlotArea.Height / series.Points.Count;
    var iconSize = Math.Min(rowHeight * 0.7f, context.PlotArea.Width / (this.IconsPerRow + 2));

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var color = dp.Color ?? colors[i % colors.Length];
      var y = context.PlotArea.Top + i * rowHeight + (rowHeight - iconSize) / 2;

      // Draw label
      var label = dp.Label ?? i.ToString();
      using (var brush = new SolidBrush(Color.Black))
        g.DrawString(label, context.Chart.Font, brush, context.PlotArea.Left, y + iconSize / 4);

      // Calculate number of icons
      var iconCount = dp.Y / maxValue * this.IconsPerRow;
      var fullIcons = (int)iconCount;
      var partialIcon = iconCount - fullIcons;

      var startX = context.PlotArea.Left + 80;

      // Draw full icons
      for (var ic = 0; ic < fullIcons; ++ic) {
        var animationIndex = (float)(i * this.IconsPerRow + ic) / (series.Points.Count * this.IconsPerRow);
        if (animationIndex > context.AnimationProgress)
          continue;

        var x = startX + ic * (iconSize + 5);
        this._DrawIcon(g, x, y, iconSize, color);
      }

      // Draw partial icon
      if (partialIcon > 0.1) {
        var x = startX + fullIcons * (iconSize + 5);
        this._DrawPartialIcon(g, x, y, iconSize, color, (float)partialIcon);
      }
    }
  }

  private void _DrawIcon(Graphics g, float x, float y, float size, Color color) {
    // Draw a simple bar/rectangle icon
    using var brush = new SolidBrush(color);
    g.FillRectangle(brush, x, y, size, size);
  }

  private void _DrawPartialIcon(Graphics g, float x, float y, float size, Color color, float fraction) {
    // Draw background (empty)
    using (var brush = new SolidBrush(Color.FromArgb(50, color)))
      g.FillRectangle(brush, x, y, size, size);

    // Draw filled portion
    using (var brush = new SolidBrush(color))
      g.FillRectangle(brush, x, y + size * (1 - fraction), size, size * fraction);
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
/// Renderer for circular treemaps (treemap in circular/radial layout).
/// </summary>
public class CircularTreemapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.CircularTreemap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;

    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var colors = this._GetColors(series.Points.Count);
    var startAngle = -90f;

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var sweepAngle = (float)(dp.Y / totalValue * 360) * (float)context.AnimationProgress;
      var segmentRadius = (float)Math.Sqrt(dp.Y / totalValue) * maxRadius;
      var color = dp.Color ?? colors[i % colors.Length];

      if (sweepAngle < 0.5f)
        continue;

      // Draw pie slice with variable radius
      using var path = new GraphicsPath();
      path.AddPie(centerX - segmentRadius, centerY - segmentRadius, segmentRadius * 2, segmentRadius * 2, startAngle, sweepAngle);

      using (var brush = new SolidBrush(color))
        g.FillPath(brush, path);

      using (var pen = new Pen(Color.White, 2))
        g.DrawPath(pen, path);

      context.RegisterHitTestRect(dp, new RectangleF(centerX - segmentRadius, centerY - segmentRadius, segmentRadius * 2, segmentRadius * 2));

      // Draw label if segment is large enough
      if (sweepAngle > 20 && !string.IsNullOrEmpty(dp.Label)) {
        var midAngle = startAngle + sweepAngle / 2;
        var labelRadius = segmentRadius * 0.6f;
        var labelX = centerX + (float)(Math.Cos(midAngle * Math.PI / 180) * labelRadius);
        var labelY = centerY + (float)(Math.Sin(midAngle * Math.PI / 180) * labelRadius);

        var labelSize = g.MeasureString(dp.Label, context.Chart.Font);
        using var brush = new SolidBrush(this._GetContrastColor(color));
        g.DrawString(dp.Label, context.Chart.Font, brush, labelX - labelSize.Width / 2, labelY - labelSize.Height / 2);
      }

      startAngle += sweepAngle;
    }
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
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
/// Renderer for convex treemaps (Voronoi-like treemap).
/// </summary>
public class ConvexTreemapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.ConvexTreemap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    // Simplified convex treemap - uses circular packing as approximation
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;

    var totalValue = series.Points.Sum(p => Math.Max(0, p.Y));
    if (totalValue <= 0)
      return;

    var sortedPoints = series.Points.Where(p => p.Y > 0).OrderByDescending(p => p.Y).ToList();
    var colors = this._GetColors(sortedPoints.Count);

    var placedCircles = new List<(PointF Center, float Radius, ChartPoint Point, Color Color)>();

    for (var i = 0; i < sortedPoints.Count; ++i) {
      var dp = sortedPoints[i];
      var areaRatio = dp.Y / totalValue;
      var radius = (float)(Math.Sqrt(areaRatio / Math.PI) * maxRadius * 1.5f * context.AnimationProgress);
      var color = dp.Color ?? colors[i % colors.Length];

      PointF position;
      if (i == 0)
        position = new PointF(centerX, centerY);
      else {
        var bestPosition = new PointF(centerX, centerY);
        var bestDistance = float.MaxValue;

        // Find position closest to center that doesn't overlap
        for (var angle = 0.0; angle < Math.PI * 2; angle += 0.1) {
          for (var r = 0.0; r < maxRadius; r += 5) {
            var testPos = new PointF(
              centerX + (float)(Math.Cos(angle) * r),
              centerY + (float)(Math.Sin(angle) * r)
            );

            var overlaps = placedCircles.Any(c => {
              var dist = Math.Sqrt(Math.Pow(c.Center.X - testPos.X, 2) + Math.Pow(c.Center.Y - testPos.Y, 2));
              return dist < c.Radius + radius + 2;
            });

            if (!overlaps) {
              var distToCenter = Math.Sqrt(Math.Pow(testPos.X - centerX, 2) + Math.Pow(testPos.Y - centerY, 2));
              if (distToCenter < bestDistance) {
                bestDistance = (float)distToCenter;
                bestPosition = testPos;
              }
            }
          }
        }
        position = bestPosition;
      }

      placedCircles.Add((position, radius, dp, color));

      // Draw circle
      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, position.X - radius, position.Y - radius, radius * 2, radius * 2);

      using (var pen = new Pen(Color.White, 1))
        g.DrawEllipse(pen, position.X - radius, position.Y - radius, radius * 2, radius * 2);

      context.RegisterHitTestRect(dp, new RectangleF(position.X - radius, position.Y - radius, radius * 2, radius * 2));

      // Draw label
      if (radius > 15 && !string.IsNullOrEmpty(dp.Label)) {
        var labelSize = g.MeasureString(dp.Label, context.Chart.Font);
        if (labelSize.Width < radius * 1.5f) {
          using var brush = new SolidBrush(this._GetContrastColor(color));
          g.DrawString(dp.Label, context.Chart.Font, brush, position.X - labelSize.Width / 2, position.Y - labelSize.Height / 2);
        }
      }
    }
  }

  private Color _GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
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
/// Renderer for circular gauge charts (full circle gauge).
/// </summary>
public class CircularGaugeRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.CircularGauge;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    double value = 0;
    double minValue = 0;
    double maxValue = 100;

    if (series != null && series.Points.Count > 0) {
      value = series.Points[0].Y;
      maxValue = series.Points.Count > 1 ? series.Points.Max(p => p.Y) : 100;
    }

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var radius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.8f;
    var thickness = radius * 0.15f;

    // Draw background circle
    using (var pen = new Pen(Color.FromArgb(230, 230, 230), thickness))
      g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);

    // Draw value arc (full circle = 360 degrees)
    var normalizedValue = Math.Max(0, Math.Min(1, (value - minValue) / (maxValue - minValue)));
    var sweepAngle = (float)(normalizedValue * 360 * context.AnimationProgress);
    var valueColor = series?.Color ?? Color.FromArgb(52, 152, 219);

    using (var pen = new Pen(valueColor, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round }) {
      var arcRect = new RectangleF(centerX - radius, centerY - radius, radius * 2, radius * 2);
      if (sweepAngle > 0.5f)
        g.DrawArc(pen, arcRect, -90, sweepAngle);
    }

    // Draw value text
    var valueText = $"{value:N0}";
    using (var font = new Font(context.Chart.Font.FontFamily, context.Chart.Font.Size * 2.5f, FontStyle.Bold)) {
      var textSize = g.MeasureString(valueText, font);
      using var brush = new SolidBrush(Color.Black);
      g.DrawString(valueText, font, brush, centerX - textSize.Width / 2, centerY - textSize.Height / 2);
    }

    // Draw percentage
    var percentText = $"{normalizedValue * 100:N0}%";
    var percentSize = g.MeasureString(percentText, context.Chart.Font);
    using (var brush = new SolidBrush(Color.Gray))
      g.DrawString(percentText, context.Chart.Font, brush, centerX - percentSize.Width / 2, centerY + 20);
  }
}

/// <summary>
/// Renderer for Euler diagrams (generalized Venn diagrams).
/// </summary>
public class EulerDiagramRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.EulerDiagram;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.8f;

    var colors = new[] {
      Color.FromArgb(100, 52, 152, 219),
      Color.FromArgb(100, 231, 76, 60),
      Color.FromArgb(100, 46, 204, 113),
      Color.FromArgb(100, 155, 89, 182),
      Color.FromArgb(100, 241, 196, 15)
    };

    // Calculate sizes based on values
    var maxValue = series.Max(s => s.Points.Count > 0 ? s.Points[0].Y : 1);
    if (maxValue <= 0)
      maxValue = 1;

    var setCount = Math.Min(series.Count, 5);

    for (var i = 0; i < setCount; ++i) {
      var s = series[i];
      var value = s.Points.Count > 0 ? s.Points[0].Y : 1;
      var sizeRatio = Math.Sqrt(value / maxValue);
      var setRadius = (float)(maxRadius * sizeRatio * 0.7f * context.AnimationProgress);

      // Position sets in a pattern
      var angle = (i * 360.0 / setCount - 90) * Math.PI / 180;
      var offset = setCount > 1 ? maxRadius * 0.3f : 0;
      var setX = centerX + (float)(Math.Cos(angle) * offset);
      var setY = centerY + (float)(Math.Sin(angle) * offset);

      var color = i < colors.Length ? colors[i] : Color.FromArgb(100, s.Color);

      // Draw ellipse
      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, setX - setRadius, setY - setRadius, setRadius * 2, setRadius * 2);

      using (var pen = new Pen(Color.FromArgb(200, color.R, color.G, color.B), 2))
        g.DrawEllipse(pen, setX - setRadius, setY - setRadius, setRadius * 2, setRadius * 2);

      // Draw label
      var label = s.Name ?? $"Set {i + 1}";
      var labelSize = g.MeasureString(label, context.Chart.Font);
      var labelAngle = angle + Math.PI; // Opposite side
      var labelRadius = setRadius + 20;
      var labelX = setX + (float)(Math.Cos(labelAngle) * labelRadius) - labelSize.Width / 2;
      var labelY = setY + (float)(Math.Sin(labelAngle) * labelRadius) - labelSize.Height / 2;

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Chart.Font, labelBrush, labelX, labelY);

      // Draw value in center of set
      if (s.Points.Count > 0) {
        var valueText = s.Points[0].Y.ToString("N0");
        var valueSize = g.MeasureString(valueText, context.Chart.Font);
        using var valueBrush = new SolidBrush(Color.DimGray);
        g.DrawString(valueText, context.Chart.Font, valueBrush, setX - valueSize.Width / 2, setY - valueSize.Height / 2);
      }
    }
  }
}
