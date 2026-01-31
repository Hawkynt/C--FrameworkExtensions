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
/// Renderer for Gantt charts.
/// </summary>
public class GanttChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Gantt;

  /// <inheritdoc />
  public override bool UsesAxes => true;

  /// <inheritdoc />
  public override ChartOrientation DefaultOrientation => ChartOrientation.Horizontal;

  /// <summary>
  /// Color for completed portion of tasks.
  /// </summary>
  public Color CompletedColor { get; set; } = Color.FromArgb(38, 166, 91);

  /// <summary>
  /// Color for remaining portion of tasks.
  /// </summary>
  public Color RemainingColor { get; set; } = Color.FromArgb(189, 195, 199);

  /// <summary>
  /// Whether to draw dependency arrows.
  /// </summary>
  public bool ShowDependencies { get; set; } = true;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var ganttTasks = context.Chart.GanttTasks;

    if (ganttTasks == null || ganttTasks.Count == 0)
      return;

    var taskCount = ganttTasks.Count;
    var barHeight = context.PlotArea.Height / taskCount * 0.7f;
    var gap = context.PlotArea.Height / taskCount * 0.15f;

    // Find date range
    var minDate = ganttTasks.Min(t => t.Start);
    var maxDate = ganttTasks.Max(t => t.End);
    var dateRange = (maxDate - minDate).TotalDays;
    if (dateRange <= 0)
      dateRange = 1;

    // Create task lookup for dependencies
    var taskLookup = new Dictionary<string, (GanttTask Task, int Index, RectangleF Rect)>();

    // Draw tasks
    for (var i = 0; i < ganttTasks.Count; ++i) {
      var task = ganttTasks[i];
      var y = context.PlotArea.Top + i * (barHeight + gap * 2) + gap;

      // Calculate X positions based on dates
      var startX = context.PlotArea.Left + (float)((task.Start - minDate).TotalDays / dateRange * context.PlotArea.Width);
      var endX = context.PlotArea.Left + (float)((task.End - minDate).TotalDays / dateRange * context.PlotArea.Width);

      // Apply animation
      if (context.AnimationProgress < 1)
        endX = startX + (float)((endX - startX) * context.AnimationProgress);

      var barWidth = endX - startX;
      var barRect = new RectangleF(startX, y, barWidth, barHeight);

      // Store for dependency drawing
      if (!string.IsNullOrEmpty(task.Id))
        taskLookup[task.Id] = (task, i, barRect);

      var taskColor = task.Color ?? Color.FromArgb(52, 152, 219);

      // Draw remaining (background) bar
      using (var brush = new SolidBrush(this.RemainingColor))
        g.FillRectangle(brush, barRect);

      // Draw completed portion
      if (task.Progress > 0) {
        var completedWidth = barWidth * (float)(task.Progress / 100.0);
        using var brush = new SolidBrush(this.CompletedColor.A > 0 ? this.CompletedColor : taskColor);
        g.FillRectangle(brush, startX, y, completedWidth, barHeight);
      }

      // Draw border
      using (var pen = new Pen(Darken(taskColor, 0.2f), 1))
        g.DrawRectangle(pen, startX, y, barWidth, barHeight);

      // Draw task name
      if (!string.IsNullOrEmpty(task.Name)) {
        var labelSize = g.MeasureString(task.Name, context.Chart.Font);
        using var brush = new SolidBrush(Color.Black);

        // Draw label inside bar if it fits, otherwise to the right
        if (labelSize.Width < barWidth - 4)
          g.DrawString(task.Name, context.Chart.Font, brush, startX + 4, y + (barHeight - labelSize.Height) / 2);
        else
          g.DrawString(task.Name, context.Chart.Font, brush, endX + 4, y + (barHeight - labelSize.Height) / 2);
      }

      // Draw progress percentage
      var progressText = $"{task.Progress:0}%";
      var progressSize = g.MeasureString(progressText, context.Chart.Font);
      using (var brush = new SolidBrush(Color.DimGray))
        g.DrawString(progressText, context.Chart.Font, brush, startX - progressSize.Width - 4, y + (barHeight - progressSize.Height) / 2);
    }

    // Draw dependencies
    if (this.ShowDependencies) {
      using var pen = new Pen(Color.Gray, 1) { EndCap = LineCap.ArrowAnchor };

      foreach (var task in ganttTasks) {
        if (task.Dependencies == null || !taskLookup.ContainsKey(task.Id))
          continue;

        var (_, _, targetRect) = taskLookup[task.Id];

        foreach (var depId in task.Dependencies) {
          if (!taskLookup.ContainsKey(depId))
            continue;

          var (_, _, sourceRect) = taskLookup[depId];

          // Draw arrow from end of source to start of target
          var startPoint = new PointF(sourceRect.Right, sourceRect.Top + sourceRect.Height / 2);
          var endPoint = new PointF(targetRect.Left, targetRect.Top + targetRect.Height / 2);

          // Draw with a curved path
          using var path = new GraphicsPath();
          var midX = (startPoint.X + endPoint.X) / 2;
          path.AddBezier(startPoint, new PointF(midX, startPoint.Y), new PointF(midX, endPoint.Y), endPoint);
          g.DrawPath(pen, path);
        }
      }
    }

    // Draw today line if within range
    var today = DateTime.Today;
    if (today >= minDate && today <= maxDate) {
      var todayX = context.PlotArea.Left + (float)((today - minDate).TotalDays / dateRange * context.PlotArea.Width);
      using var pen = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash };
      g.DrawLine(pen, todayX, context.PlotArea.Top, todayX, context.PlotArea.Bottom);
    }
  }
}

/// <summary>
/// Renderer for calendar heatmap charts.
/// </summary>
public class CalendarHeatmapRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.CalendarHeatmap;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Low value color (cold).
  /// </summary>
  public Color LowColor { get; set; } = Color.FromArgb(238, 238, 238);

  /// <summary>
  /// High value color (hot).
  /// </summary>
  public Color HighColor { get; set; } = Color.FromArgb(39, 174, 96);

  /// <summary>
  /// Number of color gradation steps.
  /// </summary>
  public int ColorSteps { get; set; } = 5;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var calendarData = context.Chart.CalendarHeatmapData;

    if (calendarData == null || calendarData.Count == 0)
      return;

    // Group by year and week
    var dataByDate = calendarData.ToDictionary(d => d.Date.Date, d => d);
    var minDate = calendarData.Min(d => d.Date);
    var maxDate = calendarData.Max(d => d.Date);
    var minValue = calendarData.Min(d => d.Value);
    var maxValue = calendarData.Max(d => d.Value);
    var valueRange = maxValue - minValue;
    if (valueRange <= 0)
      valueRange = 1;

    // Calculate dimensions
    var weeksSpan = (int)Math.Ceiling((maxDate - minDate).TotalDays / 7) + 1;
    var cellWidth = context.PlotArea.Width / Math.Max(weeksSpan, 1);
    var cellHeight = context.PlotArea.Height / 7; // 7 days in a week
    var cellSize = Math.Min(cellWidth, cellHeight) * 0.9f;
    var cellGap = Math.Min(cellWidth, cellHeight) * 0.1f;

    // Recalculate based on cell size
    var startDate = minDate.AddDays(-(int)minDate.DayOfWeek); // Start from Sunday

    // Draw day labels (Mon, Tue, etc.)
    var dayLabels = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    for (var d = 0; d < 7; ++d) {
      var y = context.PlotArea.Top + d * (cellSize + cellGap);
      using var brush = new SolidBrush(Color.Gray);
      g.DrawString(dayLabels[d], context.Chart.Font, brush, context.PlotArea.Left - 35, y + cellSize / 4);
    }

    // Draw cells
    var currentDate = startDate;
    var weekIndex = 0;

    while (currentDate <= maxDate) {
      var dayOfWeek = (int)currentDate.DayOfWeek;
      var x = context.PlotArea.Left + weekIndex * (cellSize + cellGap);
      var y = context.PlotArea.Top + dayOfWeek * (cellSize + cellGap);

      if (currentDate >= minDate && currentDate <= maxDate) {
        Color cellColor;
        if (dataByDate.TryGetValue(currentDate.Date, out var dayData)) {
          // Calculate color based on value
          var normalizedValue = (dayData.Value - minValue) / valueRange;
          normalizedValue *= context.AnimationProgress;
          cellColor = dayData.Color ?? InterpolateColor(this.LowColor, this.HighColor, (float)normalizedValue);
        } else
          cellColor = Color.FromArgb(230, 230, 230);

        // Draw cell
        using (var brush = new SolidBrush(cellColor))
          g.FillRectangle(brush, x, y, cellSize, cellSize);

        // Draw border
        using (var pen = new Pen(Color.White, 1))
          g.DrawRectangle(pen, x, y, cellSize, cellSize);
      }

      currentDate = currentDate.AddDays(1);
      if (currentDate.DayOfWeek == DayOfWeek.Sunday)
        ++weekIndex;
    }

    // Draw month labels
    currentDate = new DateTime(minDate.Year, minDate.Month, 1);
    while (currentDate <= maxDate) {
      var weeksSinceStart = (int)((currentDate - startDate).TotalDays / 7);
      var x = context.PlotArea.Left + weeksSinceStart * (cellSize + cellGap);

      using var brush = new SolidBrush(Color.Black);
      g.DrawString(currentDate.ToString("MMM"), context.Chart.Font, brush, x, context.PlotArea.Top - 20);

      currentDate = currentDate.AddMonths(1);
    }
  }
}

/// <summary>
/// Renderer for timeline charts.
/// </summary>
public class TimelineChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Timeline;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Color for the timeline axis.
  /// </summary>
  public Color AxisColor { get; set; } = Color.Gray;

  /// <summary>
  /// Whether to alternate events above and below the timeline.
  /// </summary>
  public bool AlternatePosition { get; set; } = true;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var timelineEvents = context.Chart.TimelineEvents;

    if (timelineEvents == null || timelineEvents.Count == 0)
      return;

    // Sort events by date
    var sortedEvents = timelineEvents.OrderBy(e => e.Date).ToList();

    var minDate = sortedEvents.First().Date;
    var maxDate = sortedEvents.Last().Date;
    var dateRange = (maxDate - minDate).TotalDays;
    if (dateRange <= 0)
      dateRange = 1;

    // Draw timeline axis
    var axisY = context.PlotArea.Top + context.PlotArea.Height / 2;
    using (var pen = new Pen(this.AxisColor, 2))
      g.DrawLine(pen, context.PlotArea.Left, axisY, context.PlotArea.Right, axisY);

    // Draw events
    for (var i = 0; i < sortedEvents.Count; ++i) {
      var evt = sortedEvents[i];
      var x = context.PlotArea.Left + (float)((evt.Date - minDate).TotalDays / dateRange * context.PlotArea.Width);

      // Alternate above and below
      var isAbove = this.AlternatePosition ? (i % 2 == 0) : true;
      var eventY = isAbove ? axisY - 30 : axisY + 30;
      var cardY = isAbove ? eventY - 80 : eventY + 10;

      // Apply animation
      var alpha = (int)(255 * context.AnimationProgress);

      var color = evt.Color ?? Color.FromArgb(52, 152, 219);
      color = Color.FromArgb(alpha, color);

      // Draw connector line
      using (var pen = new Pen(Color.FromArgb(alpha, this.AxisColor), 1) { DashStyle = DashStyle.Dot })
        g.DrawLine(pen, x, axisY, x, eventY);

      // Draw marker on timeline
      DrawMarker(g, new PointF(x, axisY), AdvancedMarkerStyle.Circle, 6, color, Color.FromArgb(alpha, Color.White));

      // Draw event card
      var cardWidth = 120f;
      var cardHeight = 60f;
      var cardX = x - cardWidth / 2;

      // Rounded rectangle for card
      using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
        FillRoundedRectangle(g, brush, new RectangleF(cardX, cardY, cardWidth, cardHeight), 5);

      using (var pen = new Pen(color, 2))
        DrawRoundedRectangle(g, pen, new RectangleF(cardX, cardY, cardWidth, cardHeight), 5);

      // Draw event title
      if (!string.IsNullOrEmpty(evt.Title)) {
        using var brush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
        using var sf = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
        var titleRect = new RectangleF(cardX + 5, cardY + 5, cardWidth - 10, 20);
        g.DrawString(evt.Title, context.Chart.Font, brush, titleRect, sf);
      }

      // Draw date
      using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.Gray))) {
        using var sf = new StringFormat { Alignment = StringAlignment.Center };
        g.DrawString(evt.Date.ToString("MMM d, yyyy"), context.Chart.Font, brush, new RectangleF(cardX, cardY + 25, cardWidth, 15), sf);
      }

      // Draw description (if any)
      if (!string.IsNullOrEmpty(evt.Description)) {
        using var brush = new SolidBrush(Color.FromArgb(alpha, Color.DimGray));
        using var sf = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
        var descRect = new RectangleF(cardX + 5, cardY + 40, cardWidth - 10, 15);
        g.DrawString(evt.Description, context.Chart.Font, brush, descRect, sf);
      }
    }

    // Draw year markers on timeline
    var currentYear = minDate.Year;
    while (currentYear <= maxDate.Year) {
      var yearDate = new DateTime(currentYear, 1, 1);
      if (yearDate >= minDate && yearDate <= maxDate) {
        var x = context.PlotArea.Left + (float)((yearDate - minDate).TotalDays / dateRange * context.PlotArea.Width);
        using var pen = new Pen(this.AxisColor, 1);
        g.DrawLine(pen, x, axisY - 5, x, axisY + 5);

        using var brush = new SolidBrush(Color.Black);
        var yearText = currentYear.ToString();
        var yearSize = g.MeasureString(yearText, context.Chart.Font);
        g.DrawString(yearText, context.Chart.Font, brush, x - yearSize.Width / 2, axisY + 10);
      }

      ++currentYear;
    }
  }
}

/// <summary>
/// Renderer for seasonal/periodic charts.
/// </summary>
public class SeasonalChartRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Seasonal;

  /// <inheritdoc />
  protected override LegendSymbolType _GetSymbolType() => LegendSymbolType.Line;

  /// <summary>
  /// Period type for seasonal analysis.
  /// </summary>
  public SeasonalPeriod Period { get; set; } = SeasonalPeriod.Monthly;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;

    // Group series data by period (e.g., month, quarter)
    foreach (var series in context.Series) {
      if (series.Points.Count == 0)
        continue;

      // Assume X values are dates (as double ticks or ordinal)
      var periodGroups = new Dictionary<int, List<double>>();

      foreach (var dp in series.Points) {
        var periodIndex = this._GetPeriodIndex(dp.X);
        if (!periodGroups.ContainsKey(periodIndex))
          periodGroups[periodIndex] = new List<double>();
        periodGroups[periodIndex].Add(dp.Y);
      }

      // Calculate average for each period
      var periodCount = this._GetPeriodCount();
      var points = new List<PointF>();

      for (var p = 0; p < periodCount; ++p) {
        double avgValue = 0;
        if (periodGroups.ContainsKey(p) && periodGroups[p].Count > 0)
          avgValue = periodGroups[p].Average();

        var px = context.PlotArea.Left + (p + 0.5f) * context.PlotArea.Width / periodCount;
        var py = ValueToPixelY(context, avgValue);

        // Apply animation
        if (context.AnimationProgress < 1) {
          var baseY = context.PlotArea.Bottom;
          py = baseY + (float)((py - baseY) * context.AnimationProgress);
        }

        points.Add(new PointF(px, py));
      }

      if (points.Count < 2)
        continue;

      // Draw line
      using (var pen = new Pen(series.Color, series.LineWidth))
        g.DrawLines(pen, points.ToArray());

      // Draw markers
      if (series.ShowMarkers) {
        foreach (var pt in points)
          DrawMarker(g, pt, series.MarkerStyle, series.MarkerSize, series.Color, Color.White);
      }
    }

    // Draw period labels on X axis
    var periodLabels = this._GetPeriodLabels();
    var periodCount2 = periodLabels.Length;
    for (var p = 0; p < periodCount2; ++p) {
      var x = context.PlotArea.Left + (p + 0.5f) * context.PlotArea.Width / periodCount2;
      var labelSize = g.MeasureString(periodLabels[p], context.Chart.Font);
      using var brush = new SolidBrush(Color.Black);
      g.DrawString(periodLabels[p], context.Chart.Font, brush, x - labelSize.Width / 2, context.PlotArea.Bottom + 5);
    }
  }

  private int _GetPeriodIndex(double x) =>
    this.Period switch {
      SeasonalPeriod.Monthly => (int)(x % 12),
      SeasonalPeriod.Quarterly => (int)(x % 4),
      SeasonalPeriod.Weekly => (int)(x % 52),
      SeasonalPeriod.Daily => (int)(x % 365),
      _ => (int)(x % 12)
    };

  private int _GetPeriodCount() =>
    this.Period switch {
      SeasonalPeriod.Monthly => 12,
      SeasonalPeriod.Quarterly => 4,
      SeasonalPeriod.Weekly => 52,
      SeasonalPeriod.Daily => 365,
      _ => 12
    };

  private string[] _GetPeriodLabels() =>
    this.Period switch {
      SeasonalPeriod.Monthly => new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
      SeasonalPeriod.Quarterly => new[] { "Q1", "Q2", "Q3", "Q4" },
      SeasonalPeriod.Weekly => Enumerable.Range(1, 52).Select(w => $"W{w}").ToArray(),
      SeasonalPeriod.Daily => Enumerable.Range(1, 365).Select(d => $"{d}").ToArray(),
      _ => new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
    };
}

/// <summary>
/// Seasonal period type.
/// </summary>
public enum SeasonalPeriod {
  Monthly,
  Quarterly,
  Weekly,
  Daily
}

/// <summary>
/// Renderer for spiral plot charts.
/// </summary>
public class SpiralPlotRenderer : ChartRenderer {
  /// <inheritdoc />
  public override AdvancedChartType ChartType => AdvancedChartType.Spiral;

  /// <inheritdoc />
  public override bool UsesAxes => false;

  /// <summary>
  /// Number of turns in the spiral.
  /// </summary>
  public int Turns { get; set; } = 3;

  /// <inheritdoc />
  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var series = context.Series.FirstOrDefault();

    if (series == null || series.Points.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;

    var pointCount = series.Points.Count;
    var totalAngle = this.Turns * 2 * Math.PI;
    var maxValue = series.Points.Max(p => Math.Abs(p.Y));
    if (maxValue <= 0)
      maxValue = 1;

    var points = new List<PointF>();
    var lastPoint = PointF.Empty;

    for (var i = 0; i < pointCount; ++i) {
      var dp = series.Points[i];
      var angle = (double)i / pointCount * totalAngle - Math.PI / 2; // Start at top
      var baseRadius = (float)i / pointCount * maxRadius * 0.8f + maxRadius * 0.1f;

      // Modulate radius by value
      var valueRadius = baseRadius + (float)(dp.Y / maxValue * maxRadius * 0.1);

      // Apply animation
      valueRadius *= (float)context.AnimationProgress;

      var px = centerX + (float)(Math.Cos(angle) * valueRadius);
      var py = centerY + (float)(Math.Sin(angle) * valueRadius);
      points.Add(new PointF(px, py));

      // Draw segment with color based on value
      if (i > 0) {
        var normalizedValue = dp.Y / maxValue;
        var color = InterpolateColor(
          Color.FromArgb(52, 152, 219), // Low
          Color.FromArgb(231, 76, 60),   // High
          (float)((normalizedValue + 1) / 2) // Normalize to 0-1
        );

        using var pen = new Pen(color, series.LineWidth);
        g.DrawLine(pen, lastPoint, points[i]);
      }

      lastPoint = points[i];
    }

    // Draw center marker
    DrawMarker(g, new PointF(centerX, centerY), AdvancedMarkerStyle.Circle, 8, series.Color, Color.White);

    // Draw data point markers
    if (series.ShowMarkers) {
      for (var i = 0; i < points.Count; i += Math.Max(1, pointCount / 20)) {
        var dp = series.Points[i];
        DrawMarker(g, points[i], series.MarkerStyle, series.MarkerSize, dp.Color ?? series.Color, Color.White);
        context.RegisterHitTestRect(dp, new RectangleF(points[i].X - series.MarkerSize, points[i].Y - series.MarkerSize, series.MarkerSize * 2, series.MarkerSize * 2));
      }
    }
  }
}
