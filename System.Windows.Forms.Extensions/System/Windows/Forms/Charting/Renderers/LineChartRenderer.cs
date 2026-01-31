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
/// Renderer for line charts.
/// </summary>
public class LineChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Line;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      var isHighlighted = context.HighlightedSeriesIndex.HasValue
                          && context.Series.IndexOf(series) == context.HighlightedSeriesIndex.Value;

      var points = new List<PointF>();
      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        // Apply animation
        if (context.AnimationProgress < 1) {
          var targetY = context.PlotArea.Bottom;
          py = targetY + (float)((py - targetY) * context.AnimationProgress);
        }

        points.Add(new PointF(px, py));
      }

      // Draw line
      if (points.Count > 1) {
        var lineWidth = isHighlighted ? series.LineWidth + 2 : series.LineWidth;
        using var pen = new Pen(series.Color, lineWidth);

        if (isHighlighted)
          pen.Color = Lighten(series.Color, 0.2f);

        g.DrawLines(pen, points.ToArray());
      }

      // Draw markers
      if (series.ShowMarkers) {
        for (var i = 0; i < points.Count; ++i) {
          var pt = points[i];
          var dp = series.Points[i];
          var markerColor = dp.Color ?? series.Color;
          var markerSize = isHighlighted ? series.MarkerSize + 2 : series.MarkerSize;

          DrawMarker(g, pt, series.MarkerStyle, markerSize, markerColor, Color.White);

          // Register hit test
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - markerSize, pt.Y - markerSize, markerSize * 2, markerSize * 2));
        }
      } else {
        // Register hit test even without markers
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
          DrawDataLabel(g, label, pt, context.Chart.Font, series.Color, context.DataLabelPosition);
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
/// Renderer for spline (smooth curve) charts.
/// </summary>
public class SplineChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Spline;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count < 2)
        continue;

      var points = new List<PointF>();
      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        if (context.AnimationProgress < 1) {
          var targetY = context.PlotArea.Bottom;
          py = targetY + (float)((py - targetY) * context.AnimationProgress);
        }

        points.Add(new PointF(px, py));
      }

      // Draw spline
      using var pen = new Pen(series.Color, series.LineWidth);
      g.DrawCurve(pen, points.ToArray(), 0.5f);

      // Draw markers
      if (series.ShowMarkers) {
        for (var i = 0; i < points.Count; ++i) {
          var pt = points[i];
          var dp = series.Points[i];
          var markerColor = dp.Color ?? series.Color;

          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, markerColor, Color.White);
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
        }
      }
    }
  }
}

/// <summary>
/// Renderer for step charts.
/// </summary>
public class StepChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Step;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    foreach (var series in context.Series) {
      if (series.Points.Count < 2)
        continue;

      var points = new List<PointF>();
      PointF? lastPoint = null;

      foreach (var dp in series.Points) {
        var px = ValueToPixelX(context, dp.X);
        var py = ValueToPixelY(context, dp.Y);

        if (context.AnimationProgress < 1) {
          var targetY = context.PlotArea.Bottom;
          py = targetY + (float)((py - targetY) * context.AnimationProgress);
        }

        if (lastPoint.HasValue) {
          // Add horizontal then vertical step
          points.Add(new PointF(px, lastPoint.Value.Y));
        }

        points.Add(new PointF(px, py));
        lastPoint = new PointF(px, py);
      }

      // Draw step line
      using var pen = new Pen(series.Color, series.LineWidth);
      if (points.Count > 1)
        g.DrawLines(pen, points.ToArray());

      // Draw markers at data points only
      if (series.ShowMarkers) {
        var dataPoints = series.Points.Select((dp, i) => {
          var px = ValueToPixelX(context, dp.X);
          var py = ValueToPixelY(context, dp.Y);
          if (context.AnimationProgress < 1)
            py = context.PlotArea.Bottom + (float)((py - context.PlotArea.Bottom) * context.AnimationProgress);
          return new PointF(px, py);
        }).ToList();

        for (var i = 0; i < dataPoints.Count; ++i) {
          var pt = dataPoints[i];
          var dp = series.Points[i];
          var markerColor = dp.Color ?? series.Color;

          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, markerColor, Color.White);
          context.RegisterHitTestRect(dp, new RectangleF(pt.X - series.MarkerSize, pt.Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
        }
      }
    }
  }
}
