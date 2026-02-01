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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace System.Windows.Forms;

/// <summary>
/// A simple charting control supporting various chart types.
/// </summary>
/// <example>
/// <code>
/// var chart = new SimpleChart {
///   Title = "Sales Data",
///   ChartType = ChartType.Line
/// };
/// var series = chart.AddSeries("2024");
/// series.AddPoint(1, 100);
/// series.AddPoint(2, 150);
/// series.AddPoint(3, 120);
/// </code>
/// </example>
public class SimpleChart : Control {
  private readonly ChartSeriesCollection _series;
  private bool _showDataLabels;
  private bool _enableTooltips = true;

  private ChartDataPoint _hoveredPoint;
  private ToolTip _toolTip;
  private readonly Dictionary<ChartDataPoint, RectangleF> _hitTestRects = new();

  private static readonly Color[] DefaultColors = [
    Color.DodgerBlue,
    Color.OrangeRed,
    Color.ForestGreen,
    Color.DarkViolet,
    Color.Goldenrod,
    Color.Crimson,
    Color.Teal,
    Color.SlateBlue
  ];

  /// <summary>
  /// Occurs when a data point is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a data point is clicked.")]
  public event EventHandler<ChartDataPointEventArgs> DataPointClicked;

  /// <summary>
  /// Occurs when the mouse hovers over a data point.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the mouse hovers over a data point.")]
  public event EventHandler<ChartDataPointEventArgs> DataPointHovered;

  /// <summary>
  /// Initializes a new instance of the <see cref="SimpleChart"/> class.
  /// </summary>
  public SimpleChart() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new(400, 300);
    this._series = new(this);
    this._toolTip = new();
  }

  /// <summary>
  /// Gets or sets the chart type.
  /// </summary>
  [Category("Appearance")]
  [Description("The type of chart to display.")]
  [DefaultValue(ChartType.Line)]
  public ChartType ChartType {
    get;
    set {
      if (field == value)
        return;
      field = value;
      this.Invalidate();
    }
  } = ChartType.Line;

  /// <summary>
  /// Gets the collection of data series.
  /// </summary>
  [Category("Data")]
  [Description("The collection of data series.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartSeriesCollection Series => this._series;

  /// <summary>
  /// Gets or sets the chart title.
  /// </summary>
  [Category("Appearance")]
  [Description("The title displayed at the top of the chart.")]
  [DefaultValue(null)]
  public string Title {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the X-axis title.
  /// </summary>
  [Category("Appearance")]
  [Description("The title for the X-axis.")]
  [DefaultValue(null)]
  public string XAxisTitle {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the Y-axis title.
  /// </summary>
  [Category("Appearance")]
  [Description("The title for the Y-axis.")]
  [DefaultValue(null)]
  public string YAxisTitle {
    get;
    set {
      if (field == value)
        return;
      field = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the legend.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the legend.")]
  [DefaultValue(true)]
  public bool ShowLegend {
    get;
    set {
      if (field == value)
        return;
      
      field = value;
      this.Invalidate();
    }
  } = true;

  /// <summary>
  /// Gets or sets the legend position.
  /// </summary>
  [Category("Appearance")]
  [Description("The position of the legend.")]
  [DefaultValue(LegendPosition.Right)]
  public LegendPosition LegendPosition {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  } = LegendPosition.Right;

  /// <summary>
  /// Gets or sets whether to show grid lines.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show grid lines.")]
  [DefaultValue(true)]
  public bool ShowGrid {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  } = true;

  /// <summary>
  /// Gets or sets the grid line color.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the grid lines.")]
  public Color GridColor {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  } = Color.LightGray;

  /// <summary>
  /// Gets or sets whether to auto-scale the axes.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to automatically scale axes based on data.")]
  [DefaultValue(true)]
  public bool AutoScale {
    get;
    set {
      if (field == value)
        return;

      field = value;
      this.Invalidate();
    }
  } = true;

  /// <summary>
  /// Gets or sets the minimum X-axis value.
  /// </summary>
  [Category("Behavior")]
  [Description("The minimum value for the X-axis.")]
  [DefaultValue(0d)]
  public double XAxisMin {
    get;
    set {
      if (Math.Abs(field - value) < double.Epsilon)
        return;

      field = value;
      this.Invalidate();
    }
  } = 0;

  /// <summary>
  /// Gets or sets the maximum X-axis value.
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum value for the X-axis.")]
  [DefaultValue(100d)]
  public double XAxisMax {
    get;
    set {
      if (Math.Abs(field - value) < double.Epsilon)
        return;

      field = value;
      this.Invalidate();
    }
  } = 100;

  /// <summary>
  /// Gets or sets the minimum Y-axis value.
  /// </summary>
  [Category("Behavior")]
  [Description("The minimum value for the Y-axis.")]
  [DefaultValue(0d)]
  public double YAxisMin {
    get;
    set {
      if (Math.Abs(field - value) < double.Epsilon)
        return;

      field = value;
      this.Invalidate();
    }
  } = 0;

  /// <summary>
  /// Gets or sets the maximum Y-axis value.
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum value for the Y-axis.")]
  [DefaultValue(100d)]
  public double YAxisMax {
    get;
    set {
      if (Math.Abs(field - value) < double.Epsilon)
        return;

      field = value;
      this.Invalidate();
    }
  } = 100;

  /// <summary>
  /// Gets or sets whether to show data labels.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show data labels on points.")]
  [DefaultValue(false)]
  public bool ShowDataLabels {
    get => this._showDataLabels;
    set {
      if (this._showDataLabels == value)
        return;
      this._showDataLabels = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to enable tooltips on hover.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to enable tooltips when hovering over data points.")]
  [DefaultValue(true)]
  public bool EnableTooltips {
    get => this._enableTooltips;
    set => this._enableTooltips = value;
  }

  private bool ShouldSerializeGridColor() => this.GridColor != Color.LightGray;
  private void ResetGridColor() => this.GridColor = Color.LightGray;

  /// <summary>
  /// Adds a new series to the chart.
  /// </summary>
  /// <param name="name">The name of the series.</param>
  /// <param name="type">Optional chart type override for this series.</param>
  /// <returns>The created series.</returns>
  public ChartSeries AddSeries(string name, ChartType? type = null) {
    var series = new ChartSeries(this) {
      Name = name,
      ChartType = type ?? this.ChartType,
      Color = DefaultColors[this._series.Count % DefaultColors.Length]
    };
    this._series.Add(series);
    return series;
  }

  /// <summary>
  /// Removes all series from the chart.
  /// </summary>
  public void Clear() {
    this._series.Clear();
    this.Invalidate();
  }

  /// <summary>
  /// Exports the chart to an image.
  /// </summary>
  /// <returns>A bitmap of the chart.</returns>
  public Bitmap ToImage() {
    var bmp = new Bitmap(this.Width, this.Height);
    using (var g = Graphics.FromImage(bmp)) {
      g.Clear(this.BackColor);
      this._PaintChart(g);
    }
    return bmp;
  }

  /// <summary>
  /// Saves the chart as an image file.
  /// </summary>
  /// <param name="path">The file path to save to.</param>
  public void SaveAsImage(string path) {
    using var bmp = this.ToImage();
    bmp.Save(path);
  }

  /// <summary>
  /// Raises the <see cref="DataPointClicked"/> event.
  /// </summary>
  protected virtual void OnDataPointClicked(ChartDataPointEventArgs e) => this.DataPointClicked?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="DataPointHovered"/> event.
  /// </summary>
  protected virtual void OnDataPointHovered(ChartDataPointEventArgs e) => this.DataPointHovered?.Invoke(this, e);

  internal void NotifySeriesChanged() => this.Invalidate();

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    base.OnPaint(e);
    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    this._PaintChart(e.Graphics);
  }

  private void _PaintChart(Graphics g) {
    this._hitTestRects.Clear();

    var chartArea = this._CalculateChartArea(g);
    this._CalculateAxisBounds(out var xMin, out var xMax, out var yMin, out var yMax);

    // Draw title
    if (!string.IsNullOrEmpty(this.Title)) {
      using var titleFont = new Font(this.Font.FontFamily, this.Font.Size + 4, FontStyle.Bold);
      var titleSize = g.MeasureString(this.Title, titleFont);
      g.DrawString(this.Title, titleFont, Brushes.Black, (this.Width - titleSize.Width) / 2, 5);
    }

    // Draw based on chart type
    if (this.ChartType is ChartType.Pie or ChartType.Donut)
      this._DrawPieChart(g, chartArea);
    else {
      // Draw axes and grid
      this._DrawAxes(g, chartArea, xMin, xMax, yMin, yMax);

      // Draw series
      foreach (var series in this._series.Where(s => s.Visible)) {
        var seriesType = series.ChartType;
        switch (seriesType) {
          case ChartType.Line:
            this._DrawLineSeries(g, chartArea, series, xMin, xMax, yMin, yMax);
            break;
          case ChartType.Bar:
            this._DrawBarSeries(g, chartArea, series, xMin, xMax, yMin, yMax, true);
            break;
          case ChartType.Column:
            this._DrawBarSeries(g, chartArea, series, xMin, xMax, yMin, yMax, false);
            break;
          case ChartType.Area:
            this._DrawAreaSeries(g, chartArea, series, xMin, xMax, yMin, yMax);
            break;
          case ChartType.Scatter:
            this._DrawScatterSeries(g, chartArea, series, xMin, xMax, yMin, yMax);
            break;
        }
      }
    }

    // Draw legend
    if (this.ShowLegend && this.LegendPosition != LegendPosition.None)
      this._DrawLegend(g, chartArea);
  }

  private RectangleF _CalculateChartArea(Graphics g) {
    var left = 60f;
    var top = string.IsNullOrEmpty(this.Title) ? 20f : 40f;
    var right = 20f;
    var bottom = 40f;

    if (!string.IsNullOrEmpty(this.YAxisTitle))
      left += 20;
    if (!string.IsNullOrEmpty(this.XAxisTitle))
      bottom += 20;

    if (!this.ShowLegend || this._series.Count <= 0)
      return new(left, top, this.Width - left - right, this.Height - top - bottom);

    switch (this.LegendPosition) {
      case LegendPosition.Right:
        right += this._CalculateLegendWidth(g) + 10;
        break;
      case LegendPosition.Left:
        left += this._CalculateLegendWidth(g) + 10;
        break;
      case LegendPosition.Top:
        top += 25;
        break;
      case LegendPosition.Bottom:
        bottom += 25;
        break;
    }

    return new(left, top, this.Width - left - right, this.Height - top - bottom);
  }

  private float _CalculateLegendWidth(Graphics g) {
    var maxWidth = 0f;
    foreach (var series in this._series) {
      var size = g.MeasureString(series.Name, this.Font);
      maxWidth = Math.Max(maxWidth, size.Width + 25);
    }
    return maxWidth;
  }

  private void _CalculateAxisBounds(out double xMin, out double xMax, out double yMin, out double yMax) {
    if (!this.AutoScale || this._series.Count == 0 || this._series.All(s => s.DataPoints.Count == 0)) {
      xMin = this.XAxisMin;
      xMax = this.XAxisMax;
      yMin = this.YAxisMin;
      yMax = this.YAxisMax;
      return;
    }

    xMin = double.MaxValue;
    xMax = double.MinValue;
    yMin = double.MaxValue;
    yMax = double.MinValue;

    foreach (var series in this._series) {
      foreach (var point in series.DataPoints) {
        xMin = Math.Min(xMin, point.X);
        xMax = Math.Max(xMax, point.X);
        yMin = Math.Min(yMin, point.Y);
        yMax = Math.Max(yMax, point.Y);
      }
    }

    // Add padding
    var xPadding = (xMax - xMin) * 0.1;
    var yPadding = (yMax - yMin) * 0.1;

    if (Math.Abs(xPadding) < 0.01)
      xPadding = 1;
    if (Math.Abs(yPadding) < 0.01)
      yPadding = 1;

    xMin -= xPadding;
    xMax += xPadding;
    yMin = Math.Min(0, yMin - yPadding);
    yMax += yPadding;
  }

  private void _DrawAxes(Graphics g, RectangleF chartArea, double xMin, double xMax, double yMin, double yMax) {
    using var axisPen = new Pen(Color.Black, 1);
    using var gridPen = new Pen(this.GridColor, 1);
    gridPen.DashStyle = DashStyle.Dash;

    // Draw Y-axis
    g.DrawLine(axisPen, chartArea.Left, chartArea.Top, chartArea.Left, chartArea.Bottom);

    // Draw X-axis
    g.DrawLine(axisPen, chartArea.Left, chartArea.Bottom, chartArea.Right, chartArea.Bottom);

    // Draw grid and labels
    const int tickCount = 5;

    // Y-axis ticks and grid
    for (var i = 0; i <= tickCount; ++i) {
      var y = chartArea.Bottom - (i * chartArea.Height / tickCount);
      var value = yMin + (i * (yMax - yMin) / tickCount);

      if (this.ShowGrid && i > 0)
        g.DrawLine(gridPen, chartArea.Left, y, chartArea.Right, y);

      g.DrawLine(axisPen, chartArea.Left - 5, y, chartArea.Left, y);
      var label = value.ToString("N1");
      var labelSize = g.MeasureString(label, this.Font);
      g.DrawString(label, this.Font, Brushes.Black, chartArea.Left - labelSize.Width - 8, y - labelSize.Height / 2);
    }

    // X-axis ticks and grid
    for (var i = 0; i <= tickCount; ++i) {
      var x = chartArea.Left + (i * chartArea.Width / tickCount);
      var value = xMin + (i * (xMax - xMin) / tickCount);

      if (this.ShowGrid && i > 0)
        g.DrawLine(gridPen, x, chartArea.Top, x, chartArea.Bottom);

      g.DrawLine(axisPen, x, chartArea.Bottom, x, chartArea.Bottom + 5);
      var label = value.ToString("N1");
      var labelSize = g.MeasureString(label, this.Font);
      g.DrawString(label, this.Font, Brushes.Black, x - labelSize.Width / 2, chartArea.Bottom + 8);
    }

    // Axis titles
    if (!string.IsNullOrEmpty(this.YAxisTitle)) {
      var yTitleSize = g.MeasureString(this.YAxisTitle, this.Font);
      var state = g.Save();
      g.TranslateTransform(15, chartArea.Top + chartArea.Height / 2 + yTitleSize.Width / 2);
      g.RotateTransform(-90);
      g.DrawString(this.YAxisTitle, this.Font, Brushes.Black, 0, 0);
      g.Restore(state);
    }

    if (!string.IsNullOrEmpty(this.XAxisTitle)) {
      var xTitleSize = g.MeasureString(this.XAxisTitle, this.Font);
      g.DrawString(this.XAxisTitle, this.Font, Brushes.Black,
        chartArea.Left + chartArea.Width / 2 - xTitleSize.Width / 2,
        chartArea.Bottom + 25);
    }
  }

  private void _DrawLineSeries(Graphics g, RectangleF chartArea, ChartSeries series, double xMin, double xMax, double yMin, double yMax) {
    if (series.DataPoints.Count == 0)
      return;

    using var pen = new Pen(series.Color, series.LineWidth);
    var points = new List<PointF>();

    foreach (var dp in series.DataPoints) {
      var px = chartArea.Left + (float)((dp.X - xMin) / (xMax - xMin) * chartArea.Width);
      var py = chartArea.Bottom - (float)((dp.Y - yMin) / (yMax - yMin) * chartArea.Height);
      points.Add(new(px, py));
    }

    if (points.Count > 1)
      g.DrawLines(pen, points.ToArray());

    // Draw markers
    if (series.ShowMarkers)
      _DrawMarkers(g, series, points.ToArray());

    // Register hit test regions
    for (var i = 0; i < series.DataPoints.Count; ++i)
      this._hitTestRects[series.DataPoints[i]] = new(points[i].X - 5, points[i].Y - 5, 10, 10);

    // Draw data labels
    if (this._showDataLabels)
      this._DrawDataLabels(g, series, points.ToArray());
  }

  private void _DrawAreaSeries(Graphics g, RectangleF chartArea, ChartSeries series, double xMin, double xMax, double yMin, double yMax) {
    if (series.DataPoints.Count == 0)
      return;

    var points = new List<PointF>();
    var baseY = chartArea.Bottom - (float)((0 - yMin) / (yMax - yMin) * chartArea.Height);
    baseY = Math.Min(Math.Max(baseY, chartArea.Top), chartArea.Bottom);

    foreach (var dp in series.DataPoints) {
      var px = chartArea.Left + (float)((dp.X - xMin) / (xMax - xMin) * chartArea.Width);
      var py = chartArea.Bottom - (float)((dp.Y - yMin) / (yMax - yMin) * chartArea.Height);
      points.Add(new(px, py));
    }

    // Create polygon for area fill
    using var path = new GraphicsPath();
    if (points.Count > 0) {
      path.AddLine(points[0].X, baseY, points[0].X, points[0].Y);
      for (var i = 1; i < points.Count; ++i)
        path.AddLine(points[i - 1], points[i]);

      path.AddLine(points[^1].X, points[^1].Y, points[^1].X, baseY);
      path.CloseFigure();

      using var brush = new SolidBrush(Color.FromArgb(100, series.Color));
      g.FillPath(brush, path);
    }

    // Draw line on top
    using var pen = new Pen(series.Color, series.LineWidth);
    if (points.Count > 1)
      g.DrawLines(pen, points.ToArray());

    // Register hit test regions
    for (var i = 0; i < series.DataPoints.Count; ++i)
      this._hitTestRects[series.DataPoints[i]] = new(points[i].X - 5, points[i].Y - 5, 10, 10);
  }

  private void _DrawBarSeries(Graphics g, RectangleF chartArea, ChartSeries series, double xMin, double xMax, double yMin, double yMax, bool horizontal) {
    if (series.DataPoints.Count == 0)
      return;

    var seriesIndex = this._series.IndexOf(series);
    var visibleSeriesCount = this._series.Count(s => s.Visible && s.ChartType == series.ChartType);
    var seriesOffset = this._series.Take(seriesIndex).Count(s => s.Visible && s.ChartType == series.ChartType);

    using var brush = new SolidBrush(series.Color);

    foreach (var dp in series.DataPoints) {
      RectangleF rect;

      if (horizontal) {
        // Horizontal bars (Bar chart)
        var barHeight = chartArea.Height / series.DataPoints.Count / visibleSeriesCount * 0.7f;
        var py = chartArea.Bottom - (float)((dp.X - xMin) / (xMax - xMin) * chartArea.Height);
        py -= barHeight * (seriesOffset + 0.5f);
        var barWidth = (float)((dp.Y - yMin) / (yMax - yMin) * chartArea.Width);
        var px = chartArea.Left;

        rect = new(px, py - barHeight / 2, barWidth, barHeight);
      } else {
        // Vertical bars (Column chart)
        var barWidth = chartArea.Width / series.DataPoints.Count / visibleSeriesCount * 0.7f;
        var px = chartArea.Left + (float)((dp.X - xMin) / (xMax - xMin) * chartArea.Width);
        px += barWidth * (seriesOffset - visibleSeriesCount / 2f + 0.5f);
        var baseY = chartArea.Bottom - (float)((Math.Max(0, yMin) - yMin) / (yMax - yMin) * chartArea.Height);
        var barHeight = (float)((dp.Y - Math.Max(0, yMin)) / (yMax - yMin) * chartArea.Height);

        rect = new(px - barWidth / 2, baseY - barHeight, barWidth, barHeight);
      }

      g.FillRectangle(brush, rect);
      this._hitTestRects[dp] = rect;

      if (this._showDataLabels) {
        var label = dp.Label ?? dp.Y.ToString("N1");
        var labelSize = g.MeasureString(label, this.Font);
        g.DrawString(label, this.Font, Brushes.Black,
          rect.X + rect.Width / 2 - labelSize.Width / 2,
          horizontal ? rect.Y - labelSize.Height : rect.Y - labelSize.Height - 2);
      }
    }
  }

  private void _DrawScatterSeries(Graphics g, RectangleF chartArea, ChartSeries series, double xMin, double xMax, double yMin, double yMax) {
    if (series.DataPoints.Count == 0)
      return;

    var points = new List<PointF>();

    foreach (var dp in series.DataPoints) {
      var px = chartArea.Left + (float)((dp.X - xMin) / (xMax - xMin) * chartArea.Width);
      var py = chartArea.Bottom - (float)((dp.Y - yMin) / (yMax - yMin) * chartArea.Height);
      points.Add(new(px, py));
    }

    _DrawMarkers(g, series, points.ToArray());

    // Register hit test regions
    for (var i = 0; i < series.DataPoints.Count; ++i)
      this._hitTestRects[series.DataPoints[i]] = new(points[i].X - series.MarkerSize / 2, points[i].Y - series.MarkerSize / 2, series.MarkerSize, series.MarkerSize);

    if (this._showDataLabels)
      this._DrawDataLabels(g, series, points.ToArray());
  }

  private void _DrawPieChart(Graphics g, RectangleF chartArea) {
    if (this._series.Count == 0)
      return;

    var series = this._series.FirstOrDefault(s => s.Visible);
    if (series == null || series.DataPoints.Count == 0)
      return;

    var total = series.DataPoints.Sum(dp => Math.Max(0, dp.Y));
    if (total <= 0)
      return;

    var size = Math.Min(chartArea.Width, chartArea.Height) * 0.8f;
    var centerX = chartArea.Left + chartArea.Width / 2;
    var centerY = chartArea.Top + chartArea.Height / 2;
    var pieRect = new RectangleF(centerX - size / 2, centerY - size / 2, size, size);

    var donutHoleSize = this.ChartType == ChartType.Donut ? size * 0.5f : 0;
    var startAngle = -90f;

    for (var i = 0; i < series.DataPoints.Count; ++i) {
      var dp = series.DataPoints[i];
      var sweepAngle = (float)(dp.Y / total * 360);
      var color = dp.Color ?? DefaultColors[i % DefaultColors.Length];

      using var brush = new SolidBrush(color);
      using var path = new GraphicsPath();

      if (this.ChartType == ChartType.Donut) {
        var innerRect = new RectangleF(centerX - donutHoleSize / 2, centerY - donutHoleSize / 2, donutHoleSize, donutHoleSize);
        path.AddArc(pieRect, startAngle, sweepAngle);
        path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
        path.CloseFigure();
      } else {
        path.AddPie(pieRect.X, pieRect.Y, pieRect.Width, pieRect.Height, startAngle, sweepAngle);
      }

      g.FillPath(brush, path);

      // Calculate hit test region (approximate with rectangle)
      var midAngle = startAngle + sweepAngle / 2;
      var midRad = midAngle * Math.PI / 180;
      var hitX = centerX + (float)(Math.Cos(midRad) * size / 4);
      var hitY = centerY + (float)(Math.Sin(midRad) * size / 4);
      this._hitTestRects[dp] = new(hitX - 15, hitY - 15, 30, 30);

      // Draw label
      if (this._showDataLabels) {
        var labelRad = (startAngle + sweepAngle / 2) * Math.PI / 180;
        var labelDist = size / 2 + 20;
        var labelX = centerX + (float)(Math.Cos(labelRad) * labelDist);
        var labelY = centerY + (float)(Math.Sin(labelRad) * labelDist);
        var label = dp.Label ?? $"{dp.Y:N1} ({dp.Y / total:P0})";
        var labelSize = g.MeasureString(label, this.Font);
        g.DrawString(label, this.Font, Brushes.Black, labelX - labelSize.Width / 2, labelY - labelSize.Height / 2);
      }

      startAngle += sweepAngle;
    }
  }

  private static void _DrawMarkers(Graphics g, ChartSeries series, PointF[] points) {
    using var brush = new SolidBrush(series.Color);
    using var pen = new Pen(series.Color, 1);

    var size = series.MarkerSize;
    var halfSize = size / 2f;

    for (var i = 0; i < points.Length; ++i) {
      var pt = points[i];
      var dp = series.DataPoints[i];
      var markerColor = dp.Color ?? series.Color;

      using var markerBrush = new SolidBrush(markerColor);

      switch (series.MarkerStyle) {
        case ChartMarkerStyle.Circle:
          g.FillEllipse(markerBrush, pt.X - halfSize, pt.Y - halfSize, size, size);
          break;
        case ChartMarkerStyle.Square:
          g.FillRectangle(markerBrush, pt.X - halfSize, pt.Y - halfSize, size, size);
          break;
        case ChartMarkerStyle.Diamond:
          using (var path = new GraphicsPath()) {
            path.AddPolygon(
            [
              new PointF(pt.X, pt.Y - halfSize),
              new PointF(pt.X + halfSize, pt.Y),
              new PointF(pt.X, pt.Y + halfSize),
              new PointF(pt.X - halfSize, pt.Y)
            ]
            );
            g.FillPath(markerBrush, path);
          }
          break;
        case ChartMarkerStyle.Triangle:
          using (var path = new GraphicsPath()) {
            path.AddPolygon(
            [
              new PointF(pt.X, pt.Y - halfSize),
              new PointF(pt.X + halfSize, pt.Y + halfSize),
              new PointF(pt.X - halfSize, pt.Y + halfSize)
            ]
            );
            g.FillPath(markerBrush, path);
          }
          break;
        case ChartMarkerStyle.Cross:
          using (var crossPen = new Pen(markerColor, 2)) {
            g.DrawLine(crossPen, pt.X - halfSize, pt.Y, pt.X + halfSize, pt.Y);
            g.DrawLine(crossPen, pt.X, pt.Y - halfSize, pt.X, pt.Y + halfSize);
          }
          break;
      }
    }
  }

  private void _DrawDataLabels(Graphics g, ChartSeries series, PointF[] points) {
    for (var i = 0; i < points.Length && i < series.DataPoints.Count; ++i) {
      var dp = series.DataPoints[i];
      var pt = points[i];
      var label = dp.Label ?? dp.Y.ToString("N1");
      var labelSize = g.MeasureString(label, this.Font);
      g.DrawString(label, this.Font, Brushes.Black, pt.X - labelSize.Width / 2, pt.Y - labelSize.Height - 5);
    }
  }

  private void _DrawLegend(Graphics g, RectangleF chartArea) {
    if (this._series.Count == 0)
      return;

    var legendWidth = this._CalculateLegendWidth(g);
    var itemHeight = this.Font.Height + 4;
    var legendHeight = this._series.Count * itemHeight;
    RectangleF legendRect;

    switch (this.LegendPosition) {
      case LegendPosition.Right:
        legendRect = new(chartArea.Right + 15, chartArea.Top + (chartArea.Height - legendHeight) / 2, legendWidth, legendHeight);
        break;
      case LegendPosition.Left:
        legendRect = new(chartArea.Left - legendWidth - 15, chartArea.Top + (chartArea.Height - legendHeight) / 2, legendWidth, legendHeight);
        break;
      case LegendPosition.Top:
        legendRect = new(chartArea.Left + (chartArea.Width - legendWidth) / 2, chartArea.Top - legendHeight - 10, legendWidth, legendHeight);
        break;
      case LegendPosition.Bottom:
        legendRect = new(chartArea.Left + (chartArea.Width - legendWidth) / 2, chartArea.Bottom + 30, legendWidth, legendHeight);
        break;
      default:
        return;
    }

    var y = legendRect.Y;
    foreach (var series in this._series) {
      using var brush = new SolidBrush(series.Color);
      g.FillRectangle(brush, legendRect.X, y + itemHeight / 2 - 5, 15, 10);
      g.DrawString(series.Name, this.Font, Brushes.Black, legendRect.X + 20, y);
      y += itemHeight;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (!this._enableTooltips)
      return;

    ChartDataPoint hoveredPoint = null;
    foreach (var kvp in this._hitTestRects) {
      if (!kvp.Value.Contains(e.Location))
        continue;

      hoveredPoint = kvp.Key;
      break;
    }

    if (hoveredPoint == this._hoveredPoint)
      return;

    this._hoveredPoint = hoveredPoint;

    if (hoveredPoint != null) {
      var tooltipText = hoveredPoint.Label ?? $"X: {hoveredPoint.X:N2}, Y: {hoveredPoint.Y:N2}";
      this._toolTip.Show(tooltipText, this, e.X + 15, e.Y + 15);
      this.OnDataPointHovered(new(hoveredPoint));
    } else
      this._toolTip.Hide(this);
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    foreach (var kvp in this._hitTestRects) {
      if (!kvp.Value.Contains(e.Location))
        continue;

      this.OnDataPointClicked(new(kvp.Key));
      break;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    this._toolTip.Hide(this);
    this._hoveredPoint = null;
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._toolTip?.Dispose();
      this._toolTip = null;
    }
    base.Dispose(disposing);
  }
}

/// <summary>
/// Represents a data series in a chart.
/// </summary>
public class ChartSeries {
  private readonly SimpleChart _owner;

  internal ChartSeries(SimpleChart owner) {
    this._owner = owner;
    this.DataPoints = new(owner);
  }

  /// <summary>
  /// Gets or sets the name of the series.
  /// </summary>
  public string Name {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  }

  /// <summary>
  /// Gets or sets the chart type for this series.
  /// </summary>
  public ChartType ChartType {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  }

  /// <summary>
  /// Gets the collection of data points.
  /// </summary>
  public ChartDataPointCollection DataPoints { get; }

  /// <summary>
  /// Gets or sets the color of the series.
  /// </summary>
  public Color Color {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  } = Color.DodgerBlue;

  /// <summary>
  /// Gets or sets the line width.
  /// </summary>
  public int LineWidth {
    get;
    set {
      field = Math.Max(1, value);
      this._owner?.NotifySeriesChanged();
    }
  } = 2;

  /// <summary>
  /// Gets or sets whether to show markers on data points.
  /// </summary>
  public bool ShowMarkers {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  } = true;

  /// <summary>
  /// Gets or sets the marker size.
  /// </summary>
  public int MarkerSize {
    get;
    set {
      field = Math.Max(2, value);
      this._owner?.NotifySeriesChanged();
    }
  } = 6;

  /// <summary>
  /// Gets or sets the marker style.
  /// </summary>
  public ChartMarkerStyle MarkerStyle {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  } = ChartMarkerStyle.Circle;

  /// <summary>
  /// Gets or sets whether the series is visible.
  /// </summary>
  public bool Visible {
    get;
    set {
      field = value;
      this._owner?.NotifySeriesChanged();
    }
  } = true;

  /// <summary>
  /// Adds a data point to the series.
  /// </summary>
  /// <param name="x">The X value.</param>
  /// <param name="y">The Y value.</param>
  /// <param name="label">Optional label for the point.</param>
  public void AddPoint(double x, double y, string label = null) => this.DataPoints.Add(new() { X = x, Y = y, Label = label });

  /// <summary>
  /// Adds multiple data points to the series.
  /// </summary>
  /// <param name="points">The points to add.</param>
  public void AddPoints(IEnumerable<(double x, double y)> points) {
    foreach (var (x, y) in points)
      this.AddPoint(x, y);
  }

  /// <summary>
  /// Removes all data points from the series.
  /// </summary>
  public void Clear() {
    this.DataPoints.Clear();
    this._owner?.NotifySeriesChanged();
  }
}

/// <summary>
/// Represents a data point in a chart series.
/// </summary>
public class ChartDataPoint {
  /// <summary>
  /// Gets or sets the X value.
  /// </summary>
  public double X { get; set; }

  /// <summary>
  /// Gets or sets the Y value.
  /// </summary>
  public double Y { get; set; }

  /// <summary>
  /// Gets or sets an optional label.
  /// </summary>
  public string Label { get; set; }

  /// <summary>
  /// Gets or sets an optional override color.
  /// </summary>
  public Color? Color { get; set; }

  /// <summary>
  /// Gets or sets custom tag data.
  /// </summary>
  public object Tag { get; set; }
}

/// <summary>
/// A collection of chart series.
/// </summary>
public class ChartSeriesCollection : List<ChartSeries> {
  private readonly SimpleChart _owner;

  internal ChartSeriesCollection(SimpleChart owner) => this._owner = owner;

  /// <summary>
  /// Adds a series to the collection.
  /// </summary>
  public new void Add(ChartSeries series) {
    base.Add(series);
    this._owner?.NotifySeriesChanged();
  }

  /// <summary>
  /// Removes a series from the collection.
  /// </summary>
  public new bool Remove(ChartSeries series) {
    var result = base.Remove(series);
    if (result)
      this._owner?.NotifySeriesChanged();
    return result;
  }

  /// <summary>
  /// Clears all series from the collection.
  /// </summary>
  public new void Clear() {
    base.Clear();
    this._owner?.NotifySeriesChanged();
  }
}

/// <summary>
/// A collection of chart data points.
/// </summary>
public class ChartDataPointCollection : List<ChartDataPoint> {
  private readonly SimpleChart _owner;

  internal ChartDataPointCollection(SimpleChart owner) => this._owner = owner;

  /// <summary>
  /// Adds a data point to the collection.
  /// </summary>
  public new void Add(ChartDataPoint point) {
    base.Add(point);
    this._owner?.NotifySeriesChanged();
  }

  /// <summary>
  /// Removes a data point from the collection.
  /// </summary>
  public new bool Remove(ChartDataPoint point) {
    var result = base.Remove(point);
    if (result)
      this._owner?.NotifySeriesChanged();
    return result;
  }

  /// <summary>
  /// Clears all data points from the collection.
  /// </summary>
  public new void Clear() {
    base.Clear();
    this._owner?.NotifySeriesChanged();
  }
}

/// <summary>
/// Event arguments for chart data point events.
/// </summary>
public class ChartDataPointEventArgs : EventArgs {
  /// <summary>
  /// Gets the data point.
  /// </summary>
  public ChartDataPoint DataPoint { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ChartDataPointEventArgs"/> class.
  /// </summary>
  /// <param name="dataPoint">The data point.</param>
  public ChartDataPointEventArgs(ChartDataPoint dataPoint) => this.DataPoint = dataPoint;
}

/// <summary>
/// Specifies the type of chart.
/// </summary>
public enum ChartType {
  /// <summary>
  /// Line chart with connected points.
  /// </summary>
  Line,

  /// <summary>
  /// Horizontal bar chart.
  /// </summary>
  Bar,

  /// <summary>
  /// Vertical column chart.
  /// </summary>
  Column,

  /// <summary>
  /// Area chart (filled line chart).
  /// </summary>
  Area,

  /// <summary>
  /// Pie chart.
  /// </summary>
  Pie,

  /// <summary>
  /// Donut chart (pie with center hole).
  /// </summary>
  Donut,

  /// <summary>
  /// Scatter plot (points only).
  /// </summary>
  Scatter
}

/// <summary>
/// Specifies the position of the chart legend.
/// </summary>
public enum LegendPosition {
  /// <summary>
  /// Legend at the top of the chart.
  /// </summary>
  Top,

  /// <summary>
  /// Legend at the bottom of the chart.
  /// </summary>
  Bottom,

  /// <summary>
  /// Legend to the left of the chart.
  /// </summary>
  Left,

  /// <summary>
  /// Legend to the right of the chart.
  /// </summary>
  Right,

  /// <summary>
  /// No legend displayed.
  /// </summary>
  None
}

/// <summary>
/// Specifies the style of markers on data points.
/// </summary>
public enum ChartMarkerStyle {
  /// <summary>
  /// No markers displayed.
  /// </summary>
  None,

  /// <summary>
  /// Circular markers.
  /// </summary>
  Circle,

  /// <summary>
  /// Square markers.
  /// </summary>
  Square,

  /// <summary>
  /// Diamond-shaped markers.
  /// </summary>
  Diamond,

  /// <summary>
  /// Triangular markers.
  /// </summary>
  Triangle,

  /// <summary>
  /// Cross-shaped markers.
  /// </summary>
  Cross
}
