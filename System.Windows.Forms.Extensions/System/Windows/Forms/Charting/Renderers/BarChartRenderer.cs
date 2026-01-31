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
/// Renderer for horizontal bar charts.
/// </summary>
public class BarChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Bar;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    // Calculate total categories
    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var categoryCount = allCategories.Count;
    if (categoryCount == 0)
      return;

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var barGroupHeight = context.PlotArea.Height / categoryCount;
    var barHeight = barGroupHeight * 0.7f / visibleSeries.Count;
    var barGap = barGroupHeight * 0.05f;

    // For horizontal bar charts, calculate value range from Y data (bar lengths)
    // The X axis should show values starting from 0
    var minValue = 0.0;
    var maxValue = visibleSeries.SelectMany(s => s.Points).Max(p => p.Y);
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    for (var seriesIndex = 0; seriesIndex < visibleSeries.Count; ++seriesIndex) {
      var series = visibleSeries[seriesIndex];
      var isHighlighted = context.HighlightedSeriesIndex.HasValue
                          && context.Series.IndexOf(series) == context.HighlightedSeriesIndex.Value;

      foreach (var dp in series.Points) {
        var categoryIndex = sortedCategories.IndexOf(dp.X);
        if (categoryIndex < 0)
          continue;

        var baseY = context.PlotArea.Top + categoryIndex * barGroupHeight + barGap;
        var y = baseY + seriesIndex * barHeight + seriesIndex * barGap;

        // Calculate X positions using the value range (0 to maxValue)
        var zeroX = context.PlotArea.Left;
        var valueX = (float)(context.PlotArea.Left + (dp.Y - minValue) / valueRange * context.PlotArea.Width);

        // Apply animation
        if (context.AnimationProgress < 1)
          valueX = zeroX + (float)((valueX - zeroX) * context.AnimationProgress);

        var left = Math.Min(zeroX, valueX);
        var width = Math.Abs(valueX - zeroX);
        var rect = new RectangleF(left, y, width, barHeight);

        // Draw bar
        var color = dp.Color ?? series.Color;
        if (isHighlighted)
          color = Lighten(color, 0.2f);

        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        // Draw border
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        // Register hit test
        context.RegisterHitTestRect(dp, rect);

        // Draw data label
        if (context.ShowDataLabels) {
          var label = dp.Label ?? dp.Y.ToString("N1");
          var labelSize = g.MeasureString(label, context.Chart.Font);
          var labelX = dp.Y >= 0 ? valueX + 4 : valueX - labelSize.Width - 4;
          var labelY = y + barHeight / 2 - labelSize.Height / 2;

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, labelX, labelY);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for vertical column charts.
/// </summary>
public class ColumnChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Column;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    // Calculate total categories
    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var categoryCount = allCategories.Count;
    if (categoryCount == 0)
      return;

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var barGroupWidth = context.PlotArea.Width / categoryCount;
    var barWidth = barGroupWidth * 0.7f / visibleSeries.Count;
    var barGap = barGroupWidth * 0.05f;

    // For vertical column charts, calculate value range from Y data (column heights)
    // The Y axis should show values starting from 0
    var minValue = 0.0;
    var maxValue = visibleSeries.SelectMany(s => s.Points).Max(p => p.Y);
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    for (var seriesIndex = 0; seriesIndex < visibleSeries.Count; ++seriesIndex) {
      var series = visibleSeries[seriesIndex];
      var isHighlighted = context.HighlightedSeriesIndex.HasValue
                          && context.Series.IndexOf(series) == context.HighlightedSeriesIndex.Value;

      foreach (var dp in series.Points) {
        var categoryIndex = sortedCategories.IndexOf(dp.X);
        if (categoryIndex < 0)
          continue;

        var baseX = context.PlotArea.Left + categoryIndex * barGroupWidth + barGap;
        var x = baseX + seriesIndex * barWidth + seriesIndex * barGap;

        // Calculate Y positions using the value range (0 to maxValue)
        var zeroY = context.PlotArea.Bottom;
        var valueY = (float)(context.PlotArea.Bottom - (dp.Y - minValue) / valueRange * context.PlotArea.Height);

        // Apply animation
        if (context.AnimationProgress < 1)
          valueY = zeroY + (float)((valueY - zeroY) * context.AnimationProgress);

        var top = Math.Min(zeroY, valueY);
        var height = Math.Abs(valueY - zeroY);
        var rect = new RectangleF(x, top, barWidth, height);

        // Draw bar
        var color = dp.Color ?? series.Color;
        if (isHighlighted)
          color = Lighten(color, 0.2f);

        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        // Draw border
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        // Register hit test
        context.RegisterHitTestRect(dp, rect);

        // Draw data label
        if (context.ShowDataLabels) {
          var label = dp.Label ?? dp.Y.ToString("N1");
          var labelSize = g.MeasureString(label, context.Chart.Font);
          var labelX = x + barWidth / 2 - labelSize.Width / 2;
          var labelY = dp.Y >= 0 ? top - labelSize.Height - 2 : top + height + 2;

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, labelX, labelY);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for grouped bar charts.
/// </summary>
public class GroupedBarRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.GroupedBar;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    // Grouped bar is essentially the same as Bar for multiple series
    new BarChartRenderer().Render(context);
  }
}

/// <summary>
/// Renderer for grouped column charts.
/// </summary>
public class GroupedColumnRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.GroupedColumn;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    // Grouped column is essentially the same as Column for multiple series
    new ColumnChartRenderer().Render(context);
  }
}

/// <summary>
/// Renderer for stacked bar charts.
/// </summary>
public class StackedBarRenderer : ChartRenderer {
  private readonly bool _percentage;

  /// <inheritdoc />
  public override AdvancedChartType ChartType => this._percentage ? AdvancedChartType.StackedBar100 : AdvancedChartType.StackedBar;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  public StackedBarRenderer(bool percentage = false) => this._percentage = percentage;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    // Collect all categories
    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var categoryCount = sortedCategories.Count;
    if (categoryCount == 0)
      return;

    var barHeight = context.PlotArea.Height / categoryCount * 0.7f;
    var barGap = context.PlotArea.Height / categoryCount * 0.15f;

    // Calculate max stacked value for proper scaling
    var maxStackedValue = 0.0;
    foreach (var category in sortedCategories) {
      var stackValue = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001))
        .Sum(p => Math.Abs(p.Y));
      maxStackedValue = Math.Max(maxStackedValue, stackValue);
    }
    if (maxStackedValue <= 0)
      maxStackedValue = 1;

    // For percentage mode, max is 100%
    if (this._percentage)
      maxStackedValue = 100;

    foreach (var category in sortedCategories) {
      var categoryIndex = sortedCategories.IndexOf(category);
      var y = context.PlotArea.Top + categoryIndex * (context.PlotArea.Height / categoryCount) + barGap;

      // Calculate totals for percentage mode
      var positiveTotal = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001 && p.Y > 0))
        .Sum(p => p.Y);
      var negativeTotal = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001 && p.Y < 0))
        .Sum(p => Math.Abs(p.Y));

      // Start from left edge (value 0)
      var positiveOffset = context.PlotArea.Left;
      var negativeOffset = positiveOffset;

      foreach (var series in visibleSeries) {
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - category) < 0.001);
        if (point == null)
          continue;

        var value = point.Y;
        if (this._percentage) {
          var total = value >= 0 ? positiveTotal : negativeTotal;
          if (total > 0)
            value = (value / total) * 100;
        }

        var startX = value >= 0 ? positiveOffset : negativeOffset;
        var barWidth = (float)(Math.Abs(value) / maxStackedValue * context.PlotArea.Width);

        if (context.AnimationProgress < 1)
          barWidth *= (float)context.AnimationProgress;

        var endX = value >= 0 ? startX + barWidth : startX - barWidth;
        var rect = new RectangleF(Math.Min(startX, endX), y, barWidth, barHeight);

        var color = point.Color ?? series.Color;
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        context.RegisterHitTestRect(point, rect);

        if (value >= 0)
          positiveOffset += barWidth;
        else
          negativeOffset -= barWidth;
      }
    }
  }
}

/// <summary>
/// Renderer for stacked column charts.
/// </summary>
public class StackedColumnRenderer : ChartRenderer {
  private readonly bool _percentage;

  /// <inheritdoc />
  public override AdvancedChartType ChartType => this._percentage ? AdvancedChartType.StackedColumn100 : AdvancedChartType.StackedColumn;

  public StackedColumnRenderer(bool percentage = false) => this._percentage = percentage;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    // Collect all categories
    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var categoryCount = sortedCategories.Count;
    if (categoryCount == 0)
      return;

    var barWidth = context.PlotArea.Width / categoryCount * 0.7f;
    var barGap = context.PlotArea.Width / categoryCount * 0.15f;

    // Calculate max stacked value for proper scaling
    var maxStackedValue = 0.0;
    foreach (var category in sortedCategories) {
      var stackValue = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001))
        .Sum(p => Math.Abs(p.Y));
      maxStackedValue = Math.Max(maxStackedValue, stackValue);
    }
    if (maxStackedValue <= 0)
      maxStackedValue = 1;

    // For percentage mode, max is 100%
    if (this._percentage)
      maxStackedValue = 100;

    foreach (var category in sortedCategories) {
      var categoryIndex = sortedCategories.IndexOf(category);
      var x = context.PlotArea.Left + categoryIndex * (context.PlotArea.Width / categoryCount) + barGap;

      // Calculate totals for percentage mode
      var positiveTotal = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001 && p.Y > 0))
        .Sum(p => p.Y);
      var negativeTotal = visibleSeries
        .SelectMany(s => s.Points.Where(p => Math.Abs(p.X - category) < 0.001 && p.Y < 0))
        .Sum(p => Math.Abs(p.Y));

      // Start from bottom edge (value 0)
      var positiveOffset = context.PlotArea.Bottom;
      var negativeOffset = positiveOffset;

      foreach (var series in visibleSeries) {
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - category) < 0.001);
        if (point == null)
          continue;

        var value = point.Y;
        if (this._percentage) {
          var total = value >= 0 ? positiveTotal : negativeTotal;
          if (total > 0)
            value = (value / total) * 100;
        }

        var startY = value >= 0 ? positiveOffset : negativeOffset;
        var barHeight = (float)(Math.Abs(value) / maxStackedValue * context.PlotArea.Height);

        if (context.AnimationProgress < 1)
          barHeight *= (float)context.AnimationProgress;

        var endY = value >= 0 ? startY - barHeight : startY + barHeight;
        var rect = new RectangleF(x, Math.Min(startY, endY), barWidth, barHeight);

        var color = point.Color ?? series.Color;
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        context.RegisterHitTestRect(point, rect);

        if (value >= 0)
          positiveOffset -= barHeight;
        else
          negativeOffset += barHeight;
      }
    }
  }
}

/// <summary>
/// Renderer for lollipop charts.
/// </summary>
public class LollipopChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Lollipop;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var categoryCount = sortedCategories.Count;
    if (categoryCount == 0)
      return;

    var spacing = context.PlotArea.Width / categoryCount;

    // Calculate value range starting from 0
    var minValue = 0.0;
    var maxValue = visibleSeries.SelectMany(s => s.Points).Max(p => p.Y);
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    for (var seriesIndex = 0; seriesIndex < visibleSeries.Count; ++seriesIndex) {
      var series = visibleSeries[seriesIndex];

      foreach (var dp in series.Points) {
        var categoryIndex = sortedCategories.IndexOf(dp.X);
        if (categoryIndex < 0)
          continue;

        var x = context.PlotArea.Left + categoryIndex * spacing + spacing / 2;
        x += (seriesIndex - visibleSeries.Count / 2f) * 10; // Offset for multiple series

        // Calculate Y positions using the value range (0 to maxValue)
        var baseY = context.PlotArea.Bottom;
        var topY = (float)(context.PlotArea.Bottom - (dp.Y - minValue) / valueRange * context.PlotArea.Height);

        if (context.AnimationProgress < 1)
          topY = baseY + (float)((topY - baseY) * context.AnimationProgress);

        // Draw stem
        var color = dp.Color ?? series.Color;
        using (var pen = new Pen(color, 2))
          g.DrawLine(pen, x, baseY, x, topY);

        // Draw lollipop head
        var headSize = series.MarkerSize * 2;
        DrawMarker(g, new PointF(x, topY), AdvancedMarkerStyle.Circle, headSize, color, Color.White);

        context.RegisterHitTestRect(dp, new RectangleF(x - headSize, topY - headSize, headSize * 2, headSize * 2));

        // Data label
        if (context.ShowDataLabels) {
          var label = dp.Label ?? dp.Y.ToString("N1");
          var labelSize = g.MeasureString(label, context.Chart.Font);
          var labelY = dp.Y >= 0 ? topY - headSize - labelSize.Height - 2 : topY + headSize + 2;

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, x - labelSize.Width / 2, labelY);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for dot plot charts.
/// </summary>
public class DotPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.DotPlot;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var visibleSeries = context.Series.Where(s => s.Visible).ToList();

    if (visibleSeries.Count == 0)
      return;

    var allCategories = new HashSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      allCategories.Add(point.X);

    var sortedCategories = allCategories.OrderBy(c => c).ToList();
    var categoryCount = sortedCategories.Count;
    if (categoryCount == 0)
      return;

    var rowHeight = context.PlotArea.Height / categoryCount;

    // Calculate value range starting from 0
    var minValue = 0.0;
    var maxValue = visibleSeries.SelectMany(s => s.Points).Max(p => p.Y);
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    for (var seriesIndex = 0; seriesIndex < visibleSeries.Count; ++seriesIndex) {
      var series = visibleSeries[seriesIndex];

      foreach (var dp in series.Points) {
        var categoryIndex = sortedCategories.IndexOf(dp.X);
        if (categoryIndex < 0)
          continue;

        var y = context.PlotArea.Top + categoryIndex * rowHeight + rowHeight / 2;
        y += (seriesIndex - visibleSeries.Count / 2f) * (series.MarkerSize + 2); // Offset for multiple series

        // Calculate X position using the value range (0 to maxValue)
        var startX = context.PlotArea.Left;
        var x = (float)(context.PlotArea.Left + (dp.Y - minValue) / valueRange * context.PlotArea.Width);

        if (context.AnimationProgress < 1)
          x = startX + (float)((x - startX) * context.AnimationProgress);

        // Draw dot
        var color = dp.Color ?? series.Color;
        DrawMarker(g, new PointF(x, y), series.MarkerStyle, series.MarkerSize, color, Color.White);

        context.RegisterHitTestRect(dp, new RectangleF(x - series.MarkerSize, y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));

        // Data label
        if (context.ShowDataLabels) {
          var label = dp.Label ?? dp.Y.ToString("N1");
          var labelSize = g.MeasureString(label, context.Chart.Font);

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(label, context.Chart.Font, labelBrush, x + series.MarkerSize + 4, y - labelSize.Height / 2);
        }
      }
    }
  }
}

/// <summary>Renders a diverging stacked bar chart with positive and negative values from a center line.</summary>
public class DivergingStackedBarRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.DivergingStackedBar;
  public override bool UsesAxes => true;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var rowCount = series[0].Points.Count;
    var barHeight = plotArea.Height / Math.Max(1, rowCount) * 0.7f;
    var centerX = plotArea.Left + plotArea.Width / 2;

    // Calculate max stacked value in each direction for proper scaling
    // We need to find the maximum total negative and positive stacks across all rows
    var maxNegativeStack = 0.0;
    var maxPositiveStack = 0.0;

    for (var i = 0; i < rowCount; ++i) {
      var negativeSum = 0.0;
      var positiveSum = 0.0;

      for (var seriesIndex = 0; seriesIndex < series.Count; ++seriesIndex) {
        if (i >= series[seriesIndex].Points.Count)
          continue;

        var value = series[seriesIndex].Points[i].Y;
        if (value < 0)
          negativeSum += Math.Abs(value);
        else
          positiveSum += value;
      }

      maxNegativeStack = Math.Max(maxNegativeStack, negativeSum);
      maxPositiveStack = Math.Max(maxPositiveStack, positiveSum);
    }

    // Use the larger of the two sides for symmetric scaling
    var maxStack = Math.Max(maxNegativeStack, maxPositiveStack);
    if (maxStack <= 0)
      maxStack = 1;

    for (var i = 0; i < rowCount; ++i) {
      var y = plotArea.Top + (i + 0.5f) * (plotArea.Height / rowCount) - barHeight / 2;
      var negativeX = centerX;
      var positiveX = centerX;

      for (var seriesIndex = 0; seriesIndex < series.Count; ++seriesIndex) {
        if (i >= series[seriesIndex].Points.Count)
          continue;

        var dp = series[seriesIndex].Points[i];
        // Scale width using the max stacked value so bars fit within half the plot area
        var width = (float)(Math.Abs(dp.Y) / maxStack * (plotArea.Width / 2));

        // Apply animation
        width *= (float)context.AnimationProgress;

        using var brush = new SolidBrush(series[seriesIndex].Color);

        if (dp.Y < 0) {
          negativeX -= width;
          g.FillRectangle(brush, negativeX, y, width, barHeight);
        } else {
          g.FillRectangle(brush, positiveX, y, width, barHeight);
          positiveX += width;
        }
      }
    }

    // Draw center line
    using var linePen = new Pen(Color.Gray, 1);
    g.DrawLine(linePen, centerX, plotArea.Top, centerX, plotArea.Bottom);
  }
}

/// <summary>Renders a dumbbell chart showing range between two values with connected dots.</summary>
public class DumbbellChartRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Dumbbell;
  public override bool UsesAxes => true;
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count < 2)
      return;

    var series1 = series[0];
    var series2 = series[1];
    var rowCount = Math.Min(series1.Points.Count, series2.Points.Count);

    if (rowCount == 0)
      return;

    var rowHeight = plotArea.Height / rowCount;

    // Calculate value range from Y values (the actual data values)
    var allYValues = series1.Points.Select(p => p.Y).Concat(series2.Points.Select(p => p.Y)).ToList();
    var minValue = Math.Min(0, allYValues.Min());
    var maxValue = allYValues.Max();
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    using var linePen = new Pen(Color.Gray, 2);

    for (var i = 0; i < rowCount; ++i) {
      var y = plotArea.Top + (i + 0.5f) * rowHeight;

      // Calculate X positions from Y values using calculated range
      var startX = plotArea.Left;
      var x1Target = (float)(plotArea.Left + (series1.Points[i].Y - minValue) / valueRange * plotArea.Width);
      var x2Target = (float)(plotArea.Left + (series2.Points[i].Y - minValue) / valueRange * plotArea.Width);

      // Apply animation
      var x1 = startX + (float)((x1Target - startX) * context.AnimationProgress);
      var x2 = startX + (float)((x2Target - startX) * context.AnimationProgress);

      // Draw connecting line
      g.DrawLine(linePen, x1, y, x2, y);

      // Draw first dot
      using var brush1 = new SolidBrush(series1.Color);
      g.FillEllipse(brush1, x1 - 6, y - 6, 12, 12);

      // Draw second dot
      using var brush2 = new SolidBrush(series2.Color);
      g.FillEllipse(brush2, x2 - 6, y - 6, 12, 12);

      // Register hit test areas
      context.RegisterHitTestRect(series1.Points[i], new RectangleF(x1 - 8, y - 8, 16, 16));
      context.RegisterHitTestRect(series2.Points[i], new RectangleF(x2 - 8, y - 8, 16, 16));
    }
  }
}
