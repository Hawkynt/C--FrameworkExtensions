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
/// Renderer for histogram charts.
/// </summary>
public class HistogramRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Histogram;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => false;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Rectangle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var barCount = series.Points.Count;
    var barWidth = context.PlotArea.Width / barCount * 0.9f;
    var gap = context.PlotArea.Width / barCount * 0.1f;

    var baseY = ValueToPixelY(context, 0);
    baseY = Math.Min(Math.Max(baseY, context.PlotArea.Top), context.PlotArea.Bottom);

    for (var i = 0; i < series.Points.Count; ++i) {
      var dp = series.Points[i];
      var x = context.PlotArea.Left + i * (barWidth + gap) + gap / 2;
      var valueY = ValueToPixelY(context, dp.Y);

      // Apply animation
      if (context.AnimationProgress < 1)
        valueY = baseY + (float)((valueY - baseY) * context.AnimationProgress);

      var barHeight = Math.Abs(valueY - baseY);
      var barTop = Math.Min(valueY, baseY);

      var color = dp.Color ?? series.Color;

      // Draw bar
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, x, barTop, barWidth, barHeight);

      // Draw outline
      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, x, barTop, barWidth, barHeight);

      context.RegisterHitTestRect(dp, new RectangleF(x, barTop, barWidth, barHeight));

      // Data labels
      if (context.ShowDataLabels) {
        var label = dp.Label ?? dp.Y.ToString("N0");
        DrawDataLabel(g, label, new PointF(x + barWidth / 2, barTop), context.Chart.Font, Color.Black, context.DataLabelPosition);
      }
    }
  }
}

/// <summary>
/// Renderer for box plot charts.
/// </summary>
public class BoxPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.BoxPlot;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Box plot requires BoxPlotData from the chart's data collection
    var boxPlotData = context.Chart.BoxPlotData;
    if (boxPlotData == null || boxPlotData.Count == 0)
      return;

    var boxCount = boxPlotData.Count;
    var boxWidth = context.PlotArea.Width / (boxCount + 1) * 0.6f;

    for (var i = 0; i < boxPlotData.Count; ++i) {
      var data = boxPlotData[i];
      var centerX = context.PlotArea.Left + (i + 1) * context.PlotArea.Width / (boxCount + 1);

      var minY = ValueToPixelY(context, data.Minimum);
      var q1Y = ValueToPixelY(context, data.Q1);
      var medianY = ValueToPixelY(context, data.Median);
      var q3Y = ValueToPixelY(context, data.Q3);
      var maxY = ValueToPixelY(context, data.Maximum);

      // Apply animation
      if (context.AnimationProgress < 1) {
        var midY = ValueToPixelY(context, data.Median);
        minY = midY + (float)((minY - midY) * context.AnimationProgress);
        q1Y = midY + (float)((q1Y - midY) * context.AnimationProgress);
        q3Y = midY + (float)((q3Y - midY) * context.AnimationProgress);
        maxY = midY + (float)((maxY - midY) * context.AnimationProgress);
      }

      var color = data.Color ?? Color.FromArgb(52, 152, 219);

      // Draw whiskers (vertical lines)
      using (var pen = new Pen(color, 1)) {
        // Lower whisker
        g.DrawLine(pen, centerX, q1Y, centerX, minY);
        g.DrawLine(pen, centerX - boxWidth / 4, minY, centerX + boxWidth / 4, minY);

        // Upper whisker
        g.DrawLine(pen, centerX, q3Y, centerX, maxY);
        g.DrawLine(pen, centerX - boxWidth / 4, maxY, centerX + boxWidth / 4, maxY);
      }

      // Draw box
      var boxRect = new RectangleF(centerX - boxWidth / 2, q3Y, boxWidth, q1Y - q3Y);
      using (var brush = new SolidBrush(Color.FromArgb(180, color)))
        g.FillRectangle(brush, boxRect);
      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, boxRect.X, boxRect.Y, boxRect.Width, boxRect.Height);

      // Draw median line
      using (var pen = new Pen(Darken(color, 0.3f), 2))
        g.DrawLine(pen, centerX - boxWidth / 2, medianY, centerX + boxWidth / 2, medianY);

      // Draw outliers
      if (data.Outliers != null) {
        foreach (var outlier in data.Outliers) {
          var outlierY = ValueToPixelY(context, outlier);
          if (context.AnimationProgress < 1)
            outlierY = medianY + (float)((outlierY - medianY) * context.AnimationProgress);

          DrawMarker(g, new PointF(centerX, outlierY), AdvancedMarkerStyle.Circle, 4, color, Color.White);
        }
      }

      // Draw mean point if available
      if (data.Mean.HasValue) {
        var meanY = ValueToPixelY(context, data.Mean.Value);
        if (context.AnimationProgress < 1)
          meanY = medianY + (float)((meanY - medianY) * context.AnimationProgress);

        DrawMarker(g, new PointF(centerX, meanY), AdvancedMarkerStyle.Diamond, 6, Darken(color, 0.2f), Color.White);
      }
    }
  }
}

/// <summary>
/// Renderer for violin plot charts.
/// </summary>
public class ViolinPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Violin;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Violin plot requires distribution data; for now, render as enhanced box plot
    var boxPlotData = context.Chart.BoxPlotData;
    if (boxPlotData == null || boxPlotData.Count == 0)
      return;

    var violinCount = boxPlotData.Count;
    var violinWidth = context.PlotArea.Width / (violinCount + 1) * 0.8f;

    for (var i = 0; i < boxPlotData.Count; ++i) {
      var data = boxPlotData[i];
      var centerX = context.PlotArea.Left + (i + 1) * context.PlotArea.Width / (violinCount + 1);

      var minY = ValueToPixelY(context, data.Minimum);
      var maxY = ValueToPixelY(context, data.Maximum);
      var medianY = ValueToPixelY(context, data.Median);
      var q1Y = ValueToPixelY(context, data.Q1);
      var q3Y = ValueToPixelY(context, data.Q3);

      // Apply animation
      if (context.AnimationProgress < 1) {
        var range = (maxY - minY) * context.AnimationProgress;
        minY = medianY + (float)(range / 2);
        maxY = medianY - (float)(range / 2);
        q1Y = medianY + (float)((q1Y - medianY) * context.AnimationProgress);
        q3Y = medianY + (float)((q3Y - medianY) * context.AnimationProgress);
      }

      var color = data.Color ?? Color.FromArgb(52, 152, 219);

      // Create violin shape (symmetric curved path simulating density)
      using var path = new GraphicsPath();

      // Simulate violin shape using a smooth curve
      var points = new List<PointF>();
      var segments = 20;

      // Left side (going down)
      for (var s = 0; s <= segments; ++s) {
        var t = s / (float)segments;
        var y = maxY + (minY - maxY) * t;

        // Simulate density - wider at median, narrower at extremes
        var distFromMedian = Math.Abs(y - medianY) / Math.Max(1, Math.Abs(minY - maxY));
        var width = (1 - distFromMedian * distFromMedian) * violinWidth / 2;
        width *= (float)context.AnimationProgress;

        points.Add(new PointF(centerX - (float)width, y));
      }

      // Right side (going up)
      for (var s = segments; s >= 0; --s) {
        var t = s / (float)segments;
        var y = maxY + (minY - maxY) * t;

        var distFromMedian = Math.Abs(y - medianY) / Math.Max(1, Math.Abs(minY - maxY));
        var width = (1 - distFromMedian * distFromMedian) * violinWidth / 2;
        width *= (float)context.AnimationProgress;

        points.Add(new PointF(centerX + (float)width, y));
      }

      if (points.Count > 2) {
        path.AddPolygon(points.ToArray());

        // Fill violin
        using (var brush = new SolidBrush(Color.FromArgb(120, color)))
          g.FillPath(brush, path);

        // Draw outline
        using (var pen = new Pen(color, 1))
          g.DrawPath(pen, path);
      }

      // Draw box plot inside
      var innerBoxWidth = violinWidth * 0.15f;
      using (var brush = new SolidBrush(Darken(color, 0.2f)))
        g.FillRectangle(brush, centerX - innerBoxWidth / 2, q3Y, innerBoxWidth, q1Y - q3Y);

      // Draw median
      using (var pen = new Pen(Color.White, 2))
        g.DrawLine(pen, centerX - innerBoxWidth / 2, medianY, centerX + innerBoxWidth / 2, medianY);
    }
  }
}

/// <summary>
/// Renderer for density/KDE plot charts.
/// </summary>
public class DensityPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Density;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count < 2)
        continue;

      // Sort points by X for proper curve drawing
      var sortedPoints = series.Points.OrderBy(p => p.X).ToList();

      var points = new List<PointF>();
      var baseY = context.PlotArea.Bottom;

      foreach (var dp in sortedPoints) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        // Apply animation
        if (context.AnimationProgress < 1)
          py = baseY + (float)((py - baseY) * context.AnimationProgress);

        points.Add(new PointF(px, py));
      }

      if (points.Count < 2)
        continue;

      // Create filled area
      using var path = new GraphicsPath();
      path.AddLine(points[0].X, baseY, points[0].X, points[0].Y);
      if (points.Count > 2)
        path.AddCurve(points.ToArray(), 0.3f);
      else
        path.AddLines(points.ToArray());
      path.AddLine(points[points.Count - 1].X, points[points.Count - 1].Y, points[points.Count - 1].X, baseY);
      path.CloseFigure();

      // Fill with gradient
      var bounds = path.GetBounds();
      if (bounds.Height > 0 && bounds.Width > 0) {
        using var brush = new LinearGradientBrush(
          bounds,
          Color.FromArgb(150, series.Color),
          Color.FromArgb(30, series.Color),
          LinearGradientMode.Vertical
        );
        g.FillPath(brush, path);
      }

      // Draw curve outline
      if (points.Count > 2) {
        using var pen = new Pen(series.Color, series.LineWidth);
        g.DrawCurve(pen, points.ToArray(), 0.3f);
      } else {
        using var pen = new Pen(series.Color, series.LineWidth);
        g.DrawLines(pen, points.ToArray());
      }
    }
  }
}

/// <summary>
/// Renderer for strip plot charts (jittered dots).
/// </summary>
public class StripPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.StripPlot;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Circle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var stripCount = context.Series.Count;
    var stripWidth = context.PlotArea.Width / (stripCount + 1);

    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      if (series.Points.Count == 0)
        continue;

      var centerX = context.PlotArea.Left + (si + 1) * stripWidth;
      var random = new Random(si * 1000); // Consistent jitter per series

      foreach (var dp in series.Points) {
        var py = ValueToPixelY(context, dp.Y);

        // Apply animation (fade in)
        var alpha = (int)(255 * context.AnimationProgress);

        // Add horizontal jitter
        var jitter = (float)(random.NextDouble() - 0.5) * stripWidth * 0.6f;
        var px = centerX + jitter;

        var color = Color.FromArgb(alpha, dp.Color ?? series.Color);

        DrawMarker(g, new PointF(px, py), series.MarkerStyle, series.MarkerSize, color, Color.FromArgb(alpha, Color.White));
        context.RegisterHitTestRect(dp, new RectangleF(px - series.MarkerSize, py - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
      }
    }
  }
}

/// <summary>
/// Renderer for beeswarm charts (non-overlapping dots).
/// </summary>
public class BeeswarmRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Beeswarm;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Circle;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var swarmCount = context.Series.Count;
    var swarmWidth = context.PlotArea.Width / (swarmCount + 1);

    for (var si = 0; si < context.Series.Count; ++si) {
      var series = context.Series[si];
      if (series.Points.Count == 0)
        continue;

      var centerX = context.PlotArea.Left + (si + 1) * swarmWidth;
      var markerSize = series.MarkerSize;

      // Sort points by Y and place them to avoid overlap
      var sortedPoints = series.Points.OrderBy(p => p.Y).ToList();
      var placedPoints = new List<PointF>();

      foreach (var dp in sortedPoints) {
        var py = ValueToPixelY(context, dp.Y);

        // Find non-overlapping X position
        var px = centerX;
        var offset = 0f;
        var placed = false;

        while (!placed && Math.Abs(offset) < swarmWidth / 2) {
          var testX = centerX + offset;
          var overlaps = placedPoints.Any(p =>
            Math.Abs(p.X - testX) < markerSize * 2 &&
            Math.Abs(p.Y - py) < markerSize * 2);

          if (!overlaps) {
            px = testX;
            placed = true;
          } else {
            offset = offset <= 0 ? -offset + markerSize : -offset;
          }
        }

        placedPoints.Add(new PointF(px, py));

        // Apply animation
        var alpha = (int)(255 * context.AnimationProgress);
        var color = Color.FromArgb(alpha, dp.Color ?? series.Color);

        DrawMarker(g, new PointF(px, py), series.MarkerStyle, markerSize, color, Color.FromArgb(alpha, Color.White));
        context.RegisterHitTestRect(dp, new RectangleF(px - markerSize, py - markerSize, markerSize * 2, markerSize * 2));
      }
    }
  }
}

/// <summary>
/// Renderer for ridgeline (joy plot) charts.
/// </summary>
public class RidgelineRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Ridgeline;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Area;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    var ridgeCount = context.Series.Count;
    if (ridgeCount == 0)
      return;

    // Use clipping to keep ridgelines within plot area
    var originalClip = g.Clip;
    g.SetClip(context.PlotArea);

    try {
      // Adjusted calculations to fit better within plot area
      var ridgeHeight = context.PlotArea.Height / Math.Max(ridgeCount, 1);
      var overlapFactor = 0.3f; // Reduced overlap for better fit

      // Draw from back to front (last series first)
      for (var si = ridgeCount - 1; si >= 0; --si) {
        var series = context.Series[si];
        if (series.Points.Count < 2)
          continue;

        // Sort points by X
        var sortedPoints = series.Points.OrderBy(p => p.X).ToList();

        // Calculate baseline for this ridge - adjusted to stay within bounds
        var baselineY = context.PlotArea.Top + (si + 1) * ridgeHeight * (1 - overlapFactor);
        baselineY = Math.Min(baselineY, context.PlotArea.Bottom - 5);

        // Find max Y value for scaling - reduced scale factor to fit within ridge height
        var maxY = sortedPoints.Max(p => p.Y);
        var scaleY = ridgeHeight * 0.8f / (maxY > 0 ? maxY : 1);

        var points = new List<PointF>();

        foreach (var dp in sortedPoints) {
          var px = ValueToPixelX(context, dp.X);
          var py = baselineY - (float)(dp.Y * scaleY * context.AnimationProgress);
          points.Add(new PointF(px, py));
        }

        if (points.Count < 2)
          continue;

        // Create filled area
        using var path = new GraphicsPath();
        path.AddLine(points[0].X, baselineY, points[0].X, points[0].Y);
        if (points.Count > 2)
          path.AddCurve(points.ToArray(), 0.3f);
        else
          path.AddLines(points.ToArray());
        path.AddLine(points[points.Count - 1].X, points[points.Count - 1].Y, points[points.Count - 1].X, baselineY);
        path.CloseFigure();

        // Fill with semi-transparent color
        using (var brush = new SolidBrush(Color.FromArgb(200, series.Color)))
          g.FillPath(brush, path);

        // Draw outline
        if (points.Count > 2) {
          using var pen = new Pen(Darken(series.Color, 0.2f), 1);
          g.DrawCurve(pen, points.ToArray(), 0.3f);
        }
      }
    } finally {
      // Restore original clipping region
      g.Clip = originalClip;
    }

    // Draw series labels outside the clipped region (on the left side)
    var labelRidgeHeight = context.PlotArea.Height / Math.Max(ridgeCount, 1);
    var labelOverlapFactor = 0.3f;
    for (var si = 0; si < ridgeCount; ++si) {
      var series = context.Series[si];
      var baselineY = context.PlotArea.Top + (si + 1) * labelRidgeHeight * (1 - labelOverlapFactor);
      baselineY = Math.Min(baselineY, context.PlotArea.Bottom - 5);

      using var brush = new SolidBrush(Color.Black);
      var label = series.Name ?? $"Series {si + 1}";
      var labelSize = g.MeasureString(label, context.Chart.Font);
      g.DrawString(label, context.Chart.Font, brush, context.PlotArea.Left - labelSize.Width - 5, baselineY - labelSize.Height / 2);
    }
  }
}

/// <summary>
/// Renderer for cumulative distribution charts.
/// </summary>
public class CumulativeRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Cumulative;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count < 2)
        continue;

      // Sort points by X and calculate cumulative Y
      var sortedPoints = series.Points.OrderBy(p => p.X).ToList();
      var totalY = sortedPoints.Sum(p => p.Y);

      var points = new List<PointF>();
      var cumulative = 0.0;

      foreach (var dp in sortedPoints) {
        cumulative += dp.Y;
        var normalizedY = totalY > 0 ? cumulative / totalY * 100 : 0; // 0-100%

        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, normalizedY);

        // Apply animation
        if (context.AnimationProgress < 1) {
          var baseY = context.PlotArea.Bottom;
          py = baseY + (float)((py - baseY) * context.AnimationProgress);
        }

        points.Add(new PointF(px, py));
      }

      if (points.Count < 2)
        continue;

      // Draw step line (cumulative distributions are typically stepped)
      var stepPoints = new List<PointF>();
      for (var i = 0; i < points.Count; ++i) {
        if (i > 0)
          stepPoints.Add(new PointF(points[i].X, points[i - 1].Y));
        stepPoints.Add(points[i]);
      }

      using (var pen = new Pen(series.Color, series.LineWidth))
        g.DrawLines(pen, stepPoints.ToArray());

      // Draw markers at data points
      if (series.ShowMarkers) {
        for (var i = 0; i < points.Count; ++i) {
          var pt = points[i];
          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, series.Color, Color.White);
          context.RegisterHitTestRect(sortedPoints[i], new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
        }
      }
    }
  }
}

/// <summary>
/// Renderer for population pyramid charts.
/// </summary>
public class PopulationPyramidRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.PopulationPyramid;

  /// <inheritdoc />
  public override bool SupportsMultipleSeries => true;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Population pyramid needs exactly 2 series (male/female or left/right)
    if (context.Series.Count < 2)
      return;

    var leftSeries = context.Series[0];
    var rightSeries = context.Series[1];

    // Find max value for scaling
    var maxValue = Math.Max(
      leftSeries.Points.Count > 0 ? leftSeries.Points.Max(p => Math.Abs(p.Y)) : 0,
      rightSeries.Points.Count > 0 ? rightSeries.Points.Max(p => Math.Abs(p.Y)) : 0
    );
    if (maxValue <= 0)
      maxValue = 1;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var maxBarWidth = context.PlotArea.Width / 2 - 20; // Leave space for labels
    var barCount = Math.Max(leftSeries.Points.Count, rightSeries.Points.Count);
    var barHeight = context.PlotArea.Height / barCount * 0.8f;
    var gap = context.PlotArea.Height / barCount * 0.1f;

    // Draw left bars (extending left from center)
    for (var i = 0; i < leftSeries.Points.Count; ++i) {
      var dp = leftSeries.Points[i];
      var y = context.PlotArea.Top + i * (barHeight + gap * 2) + gap;
      var barWidth = (float)(Math.Abs(dp.Y) / maxValue * maxBarWidth * context.AnimationProgress);

      var color = dp.Color ?? leftSeries.Color;
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, centerX - barWidth, y, barWidth, barHeight);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, centerX - barWidth, y, barWidth, barHeight);

      context.RegisterHitTestRect(dp, new RectangleF(centerX - barWidth, y, barWidth, barHeight));
    }

    // Draw right bars (extending right from center)
    for (var i = 0; i < rightSeries.Points.Count; ++i) {
      var dp = rightSeries.Points[i];
      var y = context.PlotArea.Top + i * (barHeight + gap * 2) + gap;
      var barWidth = (float)(Math.Abs(dp.Y) / maxValue * maxBarWidth * context.AnimationProgress);

      var color = dp.Color ?? rightSeries.Color;
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, centerX, y, barWidth, barHeight);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawRectangle(pen, centerX, y, barWidth, barHeight);

      context.RegisterHitTestRect(dp, new RectangleF(centerX, y, barWidth, barHeight));
    }

    // Draw center axis
    using (var pen = new Pen(Color.DarkGray, 2))
      g.DrawLine(pen, centerX, context.PlotArea.Top, centerX, context.PlotArea.Bottom);

    // Draw category labels (age groups) in center
    var labelPoints = leftSeries.Points.Count >= rightSeries.Points.Count ? leftSeries.Points : rightSeries.Points;
    for (var i = 0; i < labelPoints.Count; ++i) {
      var y = context.PlotArea.Top + i * (barHeight + gap * 2) + gap + barHeight / 2;
      var label = labelPoints[i].Label ?? labelPoints[i].X.ToString("N0");
      var labelSize = g.MeasureString(label, context.Chart.Font);

      using var brush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Chart.Font, brush, centerX - labelSize.Width / 2, y - labelSize.Height / 2);
    }
  }
}

/// <summary>Renders a radial histogram (histogram arranged in a circle).</summary>
public class RadialHistogramRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.RadialHistogram;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var histogramData = context.Chart.HistogramData;

    if (histogramData == null || histogramData.Count == 0) {
      // Fall back to series data
      var series = context.Series;
      if (series.Count == 0 || series[0].Points.Count == 0)
        return;

      var points = series[0].Points;
      var centerX = plotArea.Left + plotArea.Width / 2;
      var centerY = plotArea.Top + plotArea.Height / 2;
      var maxRadius = Math.Min(plotArea.Width, plotArea.Height) / 2 - 20;
      var innerRadius = maxRadius * 0.3f;

      var maxY = points.Max(p => p.Y);
      if (maxY == 0)
        maxY = 1;

      var angleStep = 360f / points.Count;

      for (var i = 0; i < points.Count; ++i) {
        var dp = points[i];
        // Apply animation to bar length
        var barLength = (float)((maxRadius - innerRadius) * dp.Y / maxY * context.AnimationProgress);
        var startAngle = i * angleStep - 90;

        // Skip if bar length is too small
        if (barLength < 0.5f)
          continue;

        // Draw arc segment
        using var brush = new SolidBrush(dp.Color ?? series[0].Color);
        var outerRect = new RectangleF(centerX - innerRadius - barLength, centerY - innerRadius - barLength,
          (innerRadius + barLength) * 2, (innerRadius + barLength) * 2);
        var innerRect = new RectangleF(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);

        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(outerRect, startAngle, angleStep - 1);
        path.AddArc(innerRect, startAngle + angleStep - 1, -(angleStep - 1));
        path.CloseFigure();

        g.FillPath(brush, path);
      }
      return;
    }

    // Use histogram bins
    var center = new PointF(plotArea.Left + plotArea.Width / 2, plotArea.Top + plotArea.Height / 2);
    var outerR = Math.Min(plotArea.Width, plotArea.Height) / 2 - 20;
    var innerR = outerR * 0.3f;

    var maxCount = histogramData.Max(h => h.Count);
    if (maxCount == 0)
      maxCount = 1;

    var step = 360f / histogramData.Count;
    var color = context.Series.Count > 0 ? context.Series[0].Color : Color.SteelBlue;

    for (var i = 0; i < histogramData.Count; ++i) {
      var bin = histogramData[i];
      // Apply animation to bar length
      var barLen = (float)((outerR - innerR) * bin.Count / maxCount * context.AnimationProgress);
      var angle = i * step - 90;

      // Skip if bar length is too small
      if (barLen < 0.5f)
        continue;

      using var brush = new SolidBrush(color);
      using var path = new System.Drawing.Drawing2D.GraphicsPath();
      var outer = new RectangleF(center.X - innerR - barLen, center.Y - innerR - barLen,
        (innerR + barLen) * 2, (innerR + barLen) * 2);
      var inner = new RectangleF(center.X - innerR, center.Y - innerR, innerR * 2, innerR * 2);

      path.AddArc(outer, angle, step - 1);
      path.AddArc(inner, angle + step - 1, -(step - 1));
      path.CloseFigure();

      g.FillPath(brush, path);
    }
  }
}

/// <summary>Renders a jitter plot (scatter plot with random horizontal offset).</summary>
public class JitterPlotRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.JitterPlot;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var random = new Random(42); // Fixed seed for consistency
    var jitterWidth = plotArea.Width / (series.Count * 2 + 1);

    for (var seriesIndex = 0; seriesIndex < series.Count; ++seriesIndex) {
      var s = series[seriesIndex];
      var categoryX = plotArea.Left + (seriesIndex + 0.5f) * (plotArea.Width / series.Count);

      foreach (var dp in s.Points) {
        var jitter = (float)((random.NextDouble() - 0.5) * jitterWidth);
        var x = categoryX + jitter;
        var y = (float)(plotArea.Bottom - (dp.Y - context.YMin) / (context.YMax - context.YMin) * plotArea.Height);

        using var brush = new SolidBrush(Color.FromArgb(180, dp.Color ?? s.Color));
        g.FillEllipse(brush, x - 4, y - 4, 8, 8);
      }
    }
  }
}

/// <summary>Renders a horizon chart (layered band chart for dense time series).</summary>
public class HorizonChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Horizon;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0)
      return;

    var bands = 4;
    var rowHeight = plotArea.Height / series.Count;

    for (var seriesIndex = 0; seriesIndex < series.Count; ++seriesIndex) {
      var s = series[seriesIndex];
      if (s.Points.Count < 2)
        continue;

      var rowTop = plotArea.Top + seriesIndex * rowHeight;
      var minY = s.Points.Min(p => p.Y);
      var maxY = s.Points.Max(p => p.Y);
      var yRange = maxY - minY;
      if (yRange == 0)
        yRange = 1;

      var bandHeight = rowHeight / bands;
      var rowBottom = rowTop + rowHeight;

      // Draw each band layer
      for (var band = 0; band < bands; ++band) {
        // Apply animation to alpha
        var alpha = (int)((50 + band * 50) * context.AnimationProgress);
        var color = Color.FromArgb(alpha, s.Color);

        var points = new List<PointF> { new(plotArea.Left, rowBottom) };

        for (var i = 0; i < s.Points.Count; ++i) {
          var x = plotArea.Left + i * plotArea.Width / (s.Points.Count - 1);
          var normalizedY = (s.Points[i].Y - minY) / yRange;
          var bandValue = Math.Min(1, Math.Max(0, normalizedY * bands - band));
          // Apply animation to band height
          var animatedBandValue = (float)(bandValue * context.AnimationProgress);
          var y = rowBottom - animatedBandValue * bandHeight;

          points.Add(new PointF(x, y));
        }

        points.Add(new PointF(plotArea.Right, rowBottom));

        if (points.Count > 2) {
          using var brush = new SolidBrush(color);
          g.FillPolygon(brush, points.ToArray());
        }
      }

      // Draw series name
      using var textBrush = new SolidBrush(Color.Black);
      g.DrawString(s.Name ?? $"Series {seriesIndex + 1}", context.Chart.Font, textBrush, plotArea.Left + 5, rowTop + 2);
    }
  }
}

/// <summary>Renders a one-dimensional heatmap (single row/column heatmap).</summary>
public class OneDimensionalHeatmapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.OneDimensionalHeatmap;
  public override bool UsesAxes => false;
  public override bool SupportsMultipleSeries => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var points = series[0].Points;
    var minY = points.Min(p => p.Y);
    var maxY = points.Max(p => p.Y);
    var yRange = maxY - minY;
    if (yRange == 0)
      yRange = 1;

    var cellWidth = plotArea.Width / points.Count;
    var targetCellHeight = Math.Min(plotArea.Height, 50);
    // Apply animation to cell height (expand from center)
    var cellHeight = targetCellHeight * (float)context.AnimationProgress;
    var centerY = plotArea.Top + plotArea.Height / 2;
    var top = centerY - cellHeight / 2;

    for (var i = 0; i < points.Count; ++i) {
      var dp = points[i];
      // Apply animation to intensity
      var intensity = (float)((dp.Y - minY) / yRange * context.AnimationProgress);
      var color = this.InterpolateColor(Color.White, series[0].Color, intensity);

      var x = plotArea.Left + i * cellWidth;
      using var brush = new SolidBrush(color);
      g.FillRectangle(brush, x, top, cellWidth, cellHeight);
    }

    // Draw border
    using var pen = new Pen(Color.Gray, 1);
    g.DrawRectangle(pen, plotArea.Left, top, plotArea.Width, cellHeight);
  }

  private Color InterpolateColor(Color from, Color to, float t) {
    var r = (int)(from.R + (to.R - from.R) * t);
    var green = (int)(from.G + (to.G - from.G) * t);
    var b = (int)(from.B + (to.B - from.B) * t);
    return Color.FromArgb(r, green, b);
  }
}
