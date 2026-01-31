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
/// Renderer for area charts.
/// </summary>
public class AreaChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Area;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var points = new List<PointF>();
      var baseY = ValueToPixelY(context, 0);

      // Clamp baseY to plot area
      baseY = Math.Min(Math.Max(baseY, context.PlotArea.Top), context.PlotArea.Bottom);

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        // Apply animation
        if (context.AnimationProgress < 1)
          py = baseY + (float)((py - baseY) * context.AnimationProgress);

        points.Add(new PointF(px, py));
      }

      if (points.Count < 2)
        continue;

      // Create area path
      using var path = new GraphicsPath();
      path.AddLine(points[0].X, baseY, points[0].X, points[0].Y);
      for (var i = 1; i < points.Count; ++i)
        path.AddLine(points[i - 1], points[i]);
      path.AddLine(points[points.Count - 1].X, points[points.Count - 1].Y, points[points.Count - 1].X, baseY);
      path.CloseFigure();

      // Fill area with gradient
      var color = series.Color;
      var bounds = path.GetBounds();
      if (bounds.Height > 0 && bounds.Width > 0) {
        using var brush = new LinearGradientBrush(
          new RectangleF(bounds.X, context.PlotArea.Top, bounds.Width, context.PlotArea.Height),
          Color.FromArgb(180, color),
          Color.FromArgb(50, color),
          LinearGradientMode.Vertical
        );
        g.FillPath(brush, path);
      }

      // Draw outline
      using (var pen = new Pen(color, series.LineWidth))
        g.DrawLines(pen, points.ToArray());

      // Draw markers
      if (series.ShowMarkers) {
        for (var i = 0; i < points.Count; ++i) {
          var pt = points[i];
          var dp = series.Points[i];
          var markerColor = dp.Color ?? color;

          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, markerColor, Color.White);
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
        }
      } else {
        // Register hit test regions even without markers
        for (var i = 0; i < points.Count; ++i) {
          var pt = points[i];
          var dp = series.Points[i];
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - 5, pt.Y - 5, 10, 10));
        }
      }

      // Draw data labels
      if (context.ShowDataLabels) {
        for (var i = 0; i < points.Count && i < series.Points.Count; ++i) {
          var dp = series.Points[i];
          var pt = points[i];
          var label = dp.Label ?? dp.Y.ToString("N1");
          DrawDataLabel(g, label, pt, context.Chart.Font, Color.Black, context.DataLabelPosition);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for stacked area charts.
/// </summary>
public class StackedAreaRenderer : ChartRenderer {
  private readonly bool _percentage;

  /// <inheritdoc />
  public override AdvancedChartType ChartType => this._percentage ? AdvancedChartType.StackedArea100 : AdvancedChartType.StackedArea;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  public StackedAreaRenderer(bool percentage = false) => this._percentage = percentage;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    // Collect all X values
    var allXValues = new SortedSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allXValues.Add(point.X);

    var xValues = allXValues.ToList();
    if (xValues.Count < 2)
      return;

    // Calculate max stacked value for proper scaling
    var maxStackedValue = 0.0;
    foreach (var x in xValues) {
      var stackValue = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - x) < 0.001))
        .Sum(p => Math.Abs(p.Y));
      maxStackedValue = Math.Max(maxStackedValue, stackValue);
    }
    if (maxStackedValue <= 0)
      maxStackedValue = 1;

    // For percentage mode, max is 100
    if (this._percentage)
      maxStackedValue = 100;

    var baseY = context.PlotArea.Bottom;

    // Calculate running totals at each X
    var runningTotals = new Dictionary<double, double>();
    foreach (var x in xValues)
      runningTotals[x] = 0;

    // Pre-calculate totals for percentage mode
    Dictionary<double, double> totals = null;
    if (this._percentage) {
      totals = new Dictionary<double, double>();
      foreach (var x in xValues) {
        var sum = visibleSeries.Sum(s => {
          var point = s.Points.FirstOrDefault(p => Math.Abs(p.X - x) < 0.001);
          return point?.Y ?? 0;
        });
        totals[x] = sum > 0 ? sum : 1;
      }
    }

    var previousTopPoints = xValues.Select(x => new PointF(ValueToPixelX(context, x), baseY)).ToList();

    foreach (var series in visibleSeries) {
      var topPoints = new List<PointF>();

      foreach (var x in xValues) {
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - x) < 0.001);
        var value = point?.Y ?? 0;

        if (this._percentage && totals != null)
          value = (value / totals[x]) * 100;

        var newTotal = runningTotals[x] + value;
        var px = ValueToPixelX(context, x);
        // Calculate Y position using calculated max stacked value
        var py = (float)(context.PlotArea.Bottom - newTotal / maxStackedValue * context.PlotArea.Height);

        if (context.AnimationProgress < 1)
          py = baseY + (float)((py - baseY) * context.AnimationProgress);

        topPoints.Add(new PointF(px, py));
        runningTotals[x] = newTotal;
      }

      // Create filled path
      using var path = new GraphicsPath();

      // Add top line (forward)
      for (var i = 0; i < topPoints.Count; ++i) {
        if (i == 0)
          path.AddLine(previousTopPoints[0], topPoints[0]);
        else
          path.AddLine(topPoints[i - 1], topPoints[i]);
      }

      // Add bottom line (backward)
      for (var i = topPoints.Count - 1; i >= 0; --i) {
        if (i == topPoints.Count - 1)
          path.AddLine(topPoints[i], previousTopPoints[i]);
        else
          path.AddLine(previousTopPoints[i + 1], previousTopPoints[i]);
      }

      path.CloseFigure();

      // Fill
      using (var brush = new SolidBrush(Color.FromArgb(180, series.Color)))
        g.FillPath(brush, path);

      // Draw top line
      if (topPoints.Count > 1) {
        using var pen = new Pen(series.Color, series.LineWidth);
        g.DrawLines(pen, topPoints.ToArray());
      }

      // Register hit tests
      for (var i = 0; i < xValues.Count; ++i) {
        var x = xValues[i];
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - x) < 0.001);
        if (point != null)
          context.RegisterHitTestRect(point, new RectangleF(topPoints[i].X - 5, topPoints[i].Y - 5, 10, 10));
      }

      previousTopPoints = topPoints;
    }
  }
}

/// <summary>
/// Renderer for step area charts.
/// </summary>
public class StepAreaRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.StepArea;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count < 2)
        continue;

      var baseY = ValueToPixelY(context, 0);
      baseY = Math.Min(Math.Max(baseY, context.PlotArea.Top), context.PlotArea.Bottom);

      var stepPoints = new List<PointF>();
      PointF? lastPoint = null;

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        if (context.AnimationProgress < 1)
          py = baseY + (float)((py - baseY) * context.AnimationProgress);

        if (lastPoint.HasValue) {
          // Add horizontal step
          stepPoints.Add(new PointF(px, lastPoint.Value.Y));
        }

        stepPoints.Add(new PointF(px, py));
        lastPoint = new PointF(px, py);
      }

      if (stepPoints.Count < 2)
        continue;

      // Create area path
      using var path = new GraphicsPath();
      path.AddLine(stepPoints[0].X, baseY, stepPoints[0].X, stepPoints[0].Y);
      for (var i = 1; i < stepPoints.Count; ++i)
        path.AddLine(stepPoints[i - 1], stepPoints[i]);
      path.AddLine(stepPoints[stepPoints.Count - 1].X, stepPoints[stepPoints.Count - 1].Y, stepPoints[stepPoints.Count - 1].X, baseY);
      path.CloseFigure();

      // Fill
      using (var brush = new SolidBrush(Color.FromArgb(100, series.Color)))
        g.FillPath(brush, path);

      // Draw outline
      using (var pen = new Pen(series.Color, series.LineWidth))
        g.DrawLines(pen, stepPoints.ToArray());

      // Register hit tests at data point positions
      var dataPointPositions = series.Points.Select((dp, i) => {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);
        if (context.AnimationProgress < 1)
          py = baseY + (float)((py - baseY) * context.AnimationProgress);
        return new PointF(px, py);
      }).ToList();

      for (var i = 0; i < dataPointPositions.Count; ++i) {
        var pt = dataPointPositions[i];
        var dp = series.Points[i];
        context.RegisterHitTestRect(dp, new RectangleF(pt.X - 5, pt.Y - 5, 10, 10));
      }
    }
  }
}

/// <summary>
/// Renderer for range area charts.
/// </summary>
public class RangeAreaRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.RangeArea;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Range area requires RangePoint data in the chart's specialized collections
    // For now, render as a simple area chart
    new AreaChartRenderer().Render(context);
  }
}

/// <summary>Renders a stream graph (stacked area chart with centered baseline).</summary>
public class StreamGraphRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.StreamGraph;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var pointCount = series.Max(s => s.Points.Count);
    if (pointCount == 0)
      return;

    // Calculate totals at each point for centering
    var totals = new double[pointCount];
    for (var i = 0; i < pointCount; ++i) {
      foreach (var s in series)
        if (i < s.Points.Count)
          totals[i] += Math.Abs(s.Points[i].Y);
    }

    var maxTotal = totals.Max();
    if (maxTotal == 0)
      maxTotal = 1;

    // Draw each series as a stream
    var baselines = new double[pointCount];
    for (var i = 0; i < pointCount; ++i)
      baselines[i] = -totals[i] / 2;

    var centerY = plotArea.Top + plotArea.Height / 2;

    foreach (var s in series) {
      var upperPoints = new List<PointF>();
      var lowerPoints = new List<PointF>();

      for (var i = 0; i < pointCount; ++i) {
        var x = plotArea.Left + i * plotArea.Width / (pointCount - 1);
        var value = i < s.Points.Count ? Math.Abs(s.Points[i].Y) : 0;

        var yLowerTarget = (float)(plotArea.Top + plotArea.Height / 2 + baselines[i] / maxTotal * plotArea.Height);
        var yUpperTarget = (float)(plotArea.Top + plotArea.Height / 2 + (baselines[i] + value) / maxTotal * plotArea.Height);

        // Apply animation (expand from center)
        var yLower = centerY + (float)((yLowerTarget - centerY) * context.AnimationProgress);
        var yUpper = centerY + (float)((yUpperTarget - centerY) * context.AnimationProgress);

        upperPoints.Add(new PointF(x, yUpper));
        lowerPoints.Add(new PointF(x, yLower));

        baselines[i] += value;
      }

      // Create path for the stream
      lowerPoints.Reverse();
      var allPoints = upperPoints.Concat(lowerPoints).ToArray();

      if (allPoints.Length > 2) {
        using var brush = new SolidBrush(Color.FromArgb(180, s.Color));
        g.FillPolygon(brush, allPoints);
      }
    }
  }
}

/// <summary>Renders a sparkline (minimal line chart without axes).</summary>
public class SparklineRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Sparkline;
  public override bool UsesAxes => false;
  public override bool SupportsMultipleSeries => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count < 2)
      return;

    var points = series[0].Points;
    var minY = points.Min(p => p.Y);
    var maxY = points.Max(p => p.Y);
    var yRange = maxY - minY;
    if (yRange == 0)
      yRange = 1;

    var baseY = plotArea.Bottom;
    var linePoints = new PointF[points.Count];
    for (var i = 0; i < points.Count; ++i) {
      var x = plotArea.Left + i * plotArea.Width / (points.Count - 1);
      var yTarget = plotArea.Bottom - (float)((points[i].Y - minY) / yRange * plotArea.Height);
      // Apply animation
      var y = baseY + (float)((yTarget - baseY) * context.AnimationProgress);
      linePoints[i] = new PointF(x, y);
    }

    using var pen = new Pen(series[0].Color, 1.5f);
    g.DrawLines(pen, linePoints);

    // Draw end point marker
    using var brush = new SolidBrush(series[0].Color);
    var lastPoint = linePoints[^1];
    g.FillEllipse(brush, lastPoint.X - 3, lastPoint.Y - 3, 6, 6);
  }
}

/// <summary>Renders a bump area chart (area version of bump chart).</summary>
public class BumpAreaRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.BumpArea;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var pointCount = series.Max(s => s.Points.Count);
    if (pointCount == 0)
      return;

    var centerY = plotArea.Top + plotArea.Height / 2;

    // Draw each series as a filled area based on ranking
    foreach (var s in series) {
      if (s.Points.Count < 2)
        continue;

      var points = new List<PointF>();
      var bottomPoints = new List<PointF>();

      for (var i = 0; i < s.Points.Count; ++i) {
        var x = plotArea.Left + i * plotArea.Width / (s.Points.Count - 1);
        var rank = s.Points[i].Y;
        var yTopTarget = (float)(plotArea.Top + (rank - 0.5) / series.Count * plotArea.Height);
        var yBottomTarget = (float)(plotArea.Top + (rank + 0.5) / series.Count * plotArea.Height);

        // Apply animation (expand from center)
        var yTop = centerY + (float)((yTopTarget - centerY) * context.AnimationProgress);
        var yBottom = centerY + (float)((yBottomTarget - centerY) * context.AnimationProgress);

        points.Add(new PointF(x, yTop));
        bottomPoints.Add(new PointF(x, yBottom));
      }

      bottomPoints.Reverse();
      var allPoints = points.Concat(bottomPoints).ToArray();

      if (allPoints.Length > 2) {
        using var brush = new SolidBrush(Color.FromArgb(150, s.Color));
        g.FillPolygon(brush, allPoints);
        using var pen = new Pen(s.Color, 1);
        g.DrawPolygon(pen, allPoints);
      }
    }
  }
}

/// <summary>Renders a barcode chart (dense vertical lines for time series).</summary>
public class BarcodeChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Barcode;
  public override bool UsesAxes => true;
  public override bool SupportsMultipleSeries => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var points = series[0].Points;
    var xRange = context.XMax - context.XMin;
    var yRange = context.YMax - context.YMin;

    var centerY = plotArea.Top + plotArea.Height / 2;

    foreach (var dp in points) {
      var x = (float)(plotArea.Left + (dp.X - context.XMin) / xRange * plotArea.Width);
      var intensity = (float)((dp.Y - context.YMin) / yRange);
      var alpha = (int)(intensity * 255 * context.AnimationProgress);

      // Apply animation (expand from center)
      var lineTop = centerY - (float)((centerY - plotArea.Top) * context.AnimationProgress);
      var lineBottom = centerY + (float)((plotArea.Bottom - centerY) * context.AnimationProgress);

      using var pen = new Pen(Color.FromArgb(alpha, series[0].Color), 1);
      g.DrawLine(pen, x, lineTop, x, lineBottom);
    }
  }
}
