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
using System.Drawing.Imaging;
using System.Linq;

namespace System.Windows.Forms.Charting;

/// <summary>
/// Advanced charting control supporting multiple chart types with a modular renderer architecture.
/// </summary>
/// <example>
/// <code>
/// var chart = new AdvancedChart {
///   ChartType = AdvancedChartType.Line,
///   Title = "Sales Data"
/// };
/// var series = chart.AddSeries("2024");
/// series.AddPoint(1, 100);
/// series.AddPoint(2, 150);
/// </code>
/// </example>
public partial class AdvancedChart : Control {
  #region Fields

  private AdvancedChartType _chartType = AdvancedChartType.Line;
  private ChartRenderer _customRenderer;
  private readonly ChartSeriesCollection _series;
  private readonly ChartAxis _xAxis;
  private readonly ChartAxis _yAxis;
  private readonly ChartAxis _y2Axis;
  private readonly ChartLegend _legend;

  private string _title;
  private string _subtitle;
  private Font _titleFont;
  private Font _subtitleFont;
  private Color _titleColor = Color.Black;
  private Color _subtitleColor = Color.Gray;
  private ChartColorPalette _colorPalette = ChartColorPalette.Default;
  private Color[] _customColors;
  private bool _showDataLabels;
  private ChartDataLabelPosition _dataLabelPosition = ChartDataLabelPosition.Top;
  private bool _enableTooltips = true;
  private ChartTooltipTrigger _tooltipTrigger = ChartTooltipTrigger.Hover;
  private bool _enableZoom;
  private bool _enablePan;
  private bool _enableCrosshair;
  private ChartSelectionMode _selectionMode = ChartSelectionMode.None;
  private bool _enableAnimation;
  private ChartAnimationStyle _animationStyle = ChartAnimationStyle.Grow;
  private int _animationDuration = 500;

  private int _padding = 10;
  private Color _plotAreaBackground = Color.White;
  private Color _plotAreaBorderColor = Color.LightGray;
  private int _plotAreaBorderWidth = 1;

  private ToolTip _toolTip;
  private readonly Dictionary<object, RectangleF> _hitTestRects = new();
  private ChartPoint _hoveredPoint;
  private ChartDataSeries _hoveredSeries;
  private int _highlightedLegendIndex = -1;
  private List<LegendItem> _legendItems = new();

  private RectangleF _plotArea;
  private double _xMin, _xMax, _yMin, _yMax, _y2Min, _y2Max;

  // Zoom/Pan state
  private PointF? _panStart;
  private double _zoomXMin, _zoomXMax, _zoomYMin, _zoomYMax;
  private bool _isZoomed;

  // Animation state
  private Timer _animationTimer;
  private double _animationProgress;
  private DateTime _animationStartTime;

  private static readonly Dictionary<AdvancedChartType, ChartRenderer> RendererCache = new();

  // Specialized data collections
  private readonly OHLCDataCollection _ohlcData;
  private readonly BoxPlotDataCollection _boxPlotData;
  private readonly HierarchicalDataCollection _hierarchicalData;
  private readonly NetworkNodeCollection _networkNodes;
  private readonly NetworkEdgeCollection _networkEdges;
  private readonly GanttTaskCollection _ganttTasks;
  private readonly TimelineEventCollection _timelineEvents;
  private readonly CalendarHeatmapCollection _calendarHeatmapData;
  private readonly FunnelStageCollection _funnelStages;
  private readonly GaugeZoneCollection _gaugeZones;
  private readonly WordCloudCollection _wordCloudData;
  private readonly HeatmapCellCollection _heatmapData;
  private readonly WaterfallStepCollection _waterfallData;
  private readonly HistogramBinCollection _histogramData;
  private readonly BulletDataCollection _bulletData;
  private readonly RangePointCollection _rangeData;
  private readonly RadarPointCollection _radarData;
  private readonly MapRegionCollection _mapData;
  private readonly BubbleMapPointCollection _bubbleData;

  #endregion

  #region Events

  /// <summary>Occurs when a data point is clicked.</summary>
  [Category("Action")]
  [Description("Occurs when a data point is clicked.")]
  public event EventHandler<ChartClickEventArgs> DataPointClicked;

  /// <summary>Occurs when a data point is hovered.</summary>
  [Category("Action")]
  [Description("Occurs when a data point is hovered.")]
  public event EventHandler<ChartHoverEventArgs> DataPointHovered;

  /// <summary>Occurs when the selection changes.</summary>
  [Category("Action")]
  [Description("Occurs when the selection changes.")]
  public event EventHandler<ChartSelectionEventArgs> SelectionChanged;

  /// <summary>Occurs when the zoom level changes.</summary>
  [Category("Action")]
  [Description("Occurs when the zoom level changes.")]
  public event EventHandler<ChartZoomEventArgs> ZoomChanged;

  #endregion

  #region Constructor

  /// <summary>
  /// Initializes a new instance of the <see cref="AdvancedChart"/> class.
  /// </summary>
  public AdvancedChart() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(500, 400);
    this._series = new ChartSeriesCollection(this);
    this._xAxis = new ChartAxis(this, ChartAxisType.Primary, true);
    this._yAxis = new ChartAxis(this, ChartAxisType.Primary, false);
    this._y2Axis = new ChartAxis(this, ChartAxisType.Secondary, false) { Visible = false };
    this._legend = new ChartLegend(this);
    this._toolTip = new ToolTip();

    // Initialize specialized data collections
    this._ohlcData = new OHLCDataCollection(this);
    this._boxPlotData = new BoxPlotDataCollection(this);
    this._hierarchicalData = new HierarchicalDataCollection(this);
    this._networkNodes = new NetworkNodeCollection(this);
    this._networkEdges = new NetworkEdgeCollection(this);
    this._ganttTasks = new GanttTaskCollection(this);
    this._timelineEvents = new TimelineEventCollection(this);
    this._calendarHeatmapData = new CalendarHeatmapCollection(this);
    this._funnelStages = new FunnelStageCollection(this);
    this._gaugeZones = new GaugeZoneCollection(this);
    this._wordCloudData = new WordCloudCollection(this);
    this._heatmapData = new HeatmapCellCollection(this);
    this._waterfallData = new WaterfallStepCollection(this);
    this._histogramData = new HistogramBinCollection(this);
    this._bulletData = new BulletDataCollection(this);
    this._rangeData = new RangePointCollection(this);
    this._radarData = new RadarPointCollection(this);
    this._mapData = new MapRegionCollection(this);
    this._bubbleData = new BubbleMapPointCollection(this);

    this._RegisterDefaultRenderers();
  }

  #endregion

  #region Properties

  /// <summary>Gets or sets the chart type.</summary>
  [Category("Appearance")]
  [Description("The type of chart to display.")]
  [DefaultValue(AdvancedChartType.Line)]
  public AdvancedChartType ChartType {
    get => this._chartType;
    set {
      if (this._chartType == value)
        return;
      this._chartType = value;
      this._StartAnimation();
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets a custom renderer (overrides ChartType).</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public ChartRenderer CustomRenderer {
    get => this._customRenderer;
    set {
      this._customRenderer = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets the collection of data series.</summary>
  [Category("Data")]
  [Description("The collection of data series.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartSeriesCollection Series => this._series;

  /// <summary>Gets the X-axis.</summary>
  [Category("Axes")]
  [Description("The X-axis configuration.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartAxis XAxis => this._xAxis;

  /// <summary>Gets the primary Y-axis.</summary>
  [Category("Axes")]
  [Description("The primary Y-axis configuration.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartAxis YAxis => this._yAxis;

  /// <summary>Gets the secondary Y-axis.</summary>
  [Category("Axes")]
  [Description("The secondary Y-axis configuration.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartAxis Y2Axis => this._y2Axis;

  #region Specialized Data Collections

  /// <summary>Gets the OHLC data collection for candlestick/OHLC charts.</summary>
  [Category("Data")]
  [Description("OHLC data for financial charts.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public OHLCDataCollection OHLCData => this._ohlcData;

  /// <summary>Gets the box plot data collection.</summary>
  [Category("Data")]
  [Description("Box plot statistical data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public BoxPlotDataCollection BoxPlotData => this._boxPlotData;

  /// <summary>Gets the hierarchical data collection for treemaps/sunbursts.</summary>
  [Category("Data")]
  [Description("Hierarchical data for treemaps and sunbursts.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public HierarchicalDataCollection HierarchicalData => this._hierarchicalData;

  /// <summary>Gets the network nodes collection for connection maps.</summary>
  [Category("Data")]
  [Description("Network nodes for connection map visualizations.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public NetworkNodeCollection NetworkNodes => this._networkNodes;

  /// <summary>Gets the network edges collection for connection maps.</summary>
  [Category("Data")]
  [Description("Network edges for connection map visualizations.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public NetworkEdgeCollection NetworkEdges => this._networkEdges;

  /// <summary>Gets the combined network data (nodes and edges) for connection maps.</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public NetworkData NetworkData => new(this._networkNodes, this._networkEdges);

  /// <summary>Gets the Gantt tasks collection.</summary>
  [Category("Data")]
  [Description("Gantt chart tasks.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public GanttTaskCollection GanttTasks => this._ganttTasks;

  /// <summary>Gets the timeline events collection.</summary>
  [Category("Data")]
  [Description("Timeline events.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public TimelineEventCollection TimelineEvents => this._timelineEvents;

  /// <summary>Gets the calendar heatmap data collection.</summary>
  [Category("Data")]
  [Description("Calendar heatmap day data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public CalendarHeatmapCollection CalendarHeatmapData => this._calendarHeatmapData;

  /// <summary>Gets the funnel stages collection.</summary>
  [Category("Data")]
  [Description("Funnel/pyramid chart stages.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public FunnelStageCollection FunnelStages => this._funnelStages;

  /// <summary>Gets the gauge zones collection.</summary>
  [Category("Data")]
  [Description("Gauge chart zones.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public GaugeZoneCollection GaugeZones => this._gaugeZones;

  /// <summary>Gets the word cloud data collection.</summary>
  [Category("Data")]
  [Description("Word cloud words and weights.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public WordCloudCollection WordCloudData => this._wordCloudData;

  /// <summary>Gets the heatmap cells collection.</summary>
  [Category("Data")]
  [Description("Heatmap cell data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public HeatmapCellCollection HeatmapData => this._heatmapData;

  /// <summary>Gets the waterfall steps collection.</summary>
  [Category("Data")]
  [Description("Waterfall chart steps.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public WaterfallStepCollection WaterfallData => this._waterfallData;

  /// <summary>Gets the histogram bins collection.</summary>
  [Category("Data")]
  [Description("Histogram bin data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public HistogramBinCollection HistogramData => this._histogramData;

  /// <summary>Gets the bullet chart data collection.</summary>
  [Category("Data")]
  [Description("Bullet chart data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public BulletDataCollection BulletData => this._bulletData;

  /// <summary>Gets the range data collection for error bars/range charts.</summary>
  [Category("Data")]
  [Description("Range data for error bars and range charts.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public RangePointCollection RangeData => this._rangeData;

  /// <summary>Gets the radar data collection.</summary>
  [Category("Data")]
  [Description("Radar/spider chart data.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public RadarPointCollection RadarData => this._radarData;

  /// <summary>Gets the map region data collection for choropleth/geospatial charts.</summary>
  [Category("Data")]
  [Description("Map region data for choropleth and geospatial charts.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public MapRegionCollection MapData => this._mapData;

  /// <summary>Gets the bubble map data collection.</summary>
  [Category("Data")]
  [Description("Bubble data for bubble map charts.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public BubbleMapPointCollection BubbleData => this._bubbleData;

  #endregion

  /// <summary>Gets the chart legend.</summary>
  [Category("Legend")]
  [Description("The legend configuration.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public ChartLegend Legend => this._legend;

  /// <summary>Gets or sets the chart title.</summary>
  [Category("Appearance")]
  [Description("The title displayed at the top of the chart.")]
  [DefaultValue(null)]
  public string Title {
    get => this._title;
    set {
      if (this._title == value)
        return;
      this._title = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the chart subtitle.</summary>
  [Category("Appearance")]
  [Description("The subtitle displayed below the title.")]
  [DefaultValue(null)]
  public string Subtitle {
    get => this._subtitle;
    set {
      if (this._subtitle == value)
        return;
      this._subtitle = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the title font.</summary>
  [Category("Appearance")]
  [Description("The font for the chart title.")]
  public Font TitleFont {
    get => this._titleFont;
    set {
      this._titleFont = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the subtitle font.</summary>
  [Category("Appearance")]
  [Description("The font for the chart subtitle.")]
  public Font SubtitleFont {
    get => this._subtitleFont;
    set {
      this._subtitleFont = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the title color.</summary>
  [Category("Appearance")]
  [Description("The color of the chart title.")]
  public Color TitleColor {
    get => this._titleColor;
    set {
      this._titleColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the subtitle color.</summary>
  [Category("Appearance")]
  [Description("The color of the chart subtitle.")]
  public Color SubtitleColor {
    get => this._subtitleColor;
    set {
      this._subtitleColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the color palette.</summary>
  [Category("Appearance")]
  [Description("The color palette for series.")]
  [DefaultValue(ChartColorPalette.Default)]
  public ChartColorPalette ColorPalette {
    get => this._colorPalette;
    set {
      this._colorPalette = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets custom colors (when ColorPalette is Custom).</summary>
  [Category("Appearance")]
  [Description("Custom colors for series.")]
  public Color[] CustomColors {
    get => this._customColors;
    set {
      this._customColors = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to show data labels.</summary>
  [Category("Appearance")]
  [Description("Whether to show data labels.")]
  [DefaultValue(false)]
  public bool ShowDataLabels {
    get => this._showDataLabels;
    set {
      this._showDataLabels = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the data label position.</summary>
  [Category("Appearance")]
  [Description("The position of data labels.")]
  [DefaultValue(ChartDataLabelPosition.Top)]
  public ChartDataLabelPosition DataLabelPosition {
    get => this._dataLabelPosition;
    set {
      this._dataLabelPosition = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to enable tooltips.</summary>
  [Category("Behavior")]
  [Description("Whether to show tooltips on hover.")]
  [DefaultValue(true)]
  public bool EnableTooltips {
    get => this._enableTooltips;
    set => this._enableTooltips = value;
  }

  /// <summary>Gets or sets the tooltip trigger mode.</summary>
  [Category("Behavior")]
  [Description("How tooltips are triggered.")]
  [DefaultValue(ChartTooltipTrigger.Hover)]
  public ChartTooltipTrigger TooltipTrigger {
    get => this._tooltipTrigger;
    set => this._tooltipTrigger = value;
  }

  /// <summary>Gets or sets whether to enable zooming.</summary>
  [Category("Behavior")]
  [Description("Whether to enable mouse wheel zooming.")]
  [DefaultValue(false)]
  public bool EnableZoom {
    get => this._enableZoom;
    set => this._enableZoom = value;
  }

  /// <summary>Gets or sets whether to enable panning.</summary>
  [Category("Behavior")]
  [Description("Whether to enable mouse drag panning.")]
  [DefaultValue(false)]
  public bool EnablePan {
    get => this._enablePan;
    set => this._enablePan = value;
  }

  /// <summary>Gets or sets whether to enable crosshair.</summary>
  [Category("Behavior")]
  [Description("Whether to show crosshair on hover.")]
  [DefaultValue(false)]
  public bool EnableCrosshair {
    get => this._enableCrosshair;
    set {
      this._enableCrosshair = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the selection mode.</summary>
  [Category("Behavior")]
  [Description("The selection mode for data points.")]
  [DefaultValue(ChartSelectionMode.None)]
  public ChartSelectionMode SelectionMode {
    get => this._selectionMode;
    set => this._selectionMode = value;
  }

  /// <summary>Gets or sets whether to enable animation.</summary>
  [Category("Behavior")]
  [Description("Whether to animate chart transitions.")]
  [DefaultValue(false)]
  public bool EnableAnimation {
    get => this._enableAnimation;
    set {
      this._enableAnimation = value;
      // When animation is enabled, start at progress 0 so initial render shows starting state
      if (value)
        this._animationProgress = 0;
    }
  }

  /// <summary>Gets or sets the animation style.</summary>
  [Category("Behavior")]
  [Description("The style of animation.")]
  [DefaultValue(ChartAnimationStyle.Grow)]
  public ChartAnimationStyle AnimationStyle {
    get => this._animationStyle;
    set => this._animationStyle = value;
  }

  /// <summary>Gets or sets the animation duration in milliseconds.</summary>
  [Category("Behavior")]
  [Description("The duration of animations in milliseconds.")]
  [DefaultValue(500)]
  public int AnimationDuration {
    get => this._animationDuration;
    set => this._animationDuration = Math.Max(0, value);
  }

  /// <summary>Gets or sets the padding around the chart.</summary>
  [Category("Layout")]
  [Description("The padding around the chart in pixels.")]
  [DefaultValue(10)]
  public int ChartPadding {
    get => this._padding;
    set {
      this._padding = Math.Max(0, value);
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area background color.</summary>
  [Category("Appearance")]
  [Description("The background color of the plot area.")]
  public Color PlotAreaBackground {
    get => this._plotAreaBackground;
    set {
      this._plotAreaBackground = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area border color.</summary>
  [Category("Appearance")]
  [Description("The border color of the plot area.")]
  public Color PlotAreaBorderColor {
    get => this._plotAreaBorderColor;
    set {
      this._plotAreaBorderColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area border width.</summary>
  [Category("Appearance")]
  [Description("The border width of the plot area.")]
  [DefaultValue(1)]
  public int PlotAreaBorderWidth {
    get => this._plotAreaBorderWidth;
    set {
      this._plotAreaBorderWidth = Math.Max(0, value);
      this.Invalidate();
    }
  }

  #endregion

  #region Public Methods

  /// <summary>
  /// Adds a new series to the chart.
  /// </summary>
  public ChartDataSeries AddSeries(string name, AdvancedChartType? type = null) {
    var series = new ChartDataSeries(this) {
      Name = name,
      ChartTypeOverride = type,
      Color = this._GetSeriesColor(this._series.Count)
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
  /// Resets the zoom to show all data.
  /// </summary>
  public void ResetZoom() {
    this._isZoomed = false;
    this.Invalidate();
    this.ZoomChanged?.Invoke(this, new ChartZoomEventArgs(1.0, 1.0));
  }

  /// <summary>
  /// Triggers chart animation (useful when data changes or to replay animation).
  /// </summary>
  public void TriggerAnimation() {
    if (!this.IsHandleCreated) {
      // Defer animation until handle is created
      this.HandleCreated += this._OnHandleCreatedForAnimation;
      return;
    }
    this._StartAnimation();
    this.Invalidate();
    this.Update();
  }

  private void _OnHandleCreatedForAnimation(object sender, EventArgs e) {
    this.HandleCreated -= this._OnHandleCreatedForAnimation;
    this._StartAnimation();
    this.Invalidate();
    this.Update();
  }

  /// <summary>
  /// Refreshes the chart display.
  /// </summary>
  public new void Refresh() {
    base.Refresh();
    if (this._enableAnimation)
      this._StartAnimation();
    this.Invalidate();
  }

  /// <summary>
  /// Exports the chart to an image.
  /// </summary>
  public Bitmap ToImage(int width = 0, int height = 0) {
    if (width <= 0)
      width = this.Width;
    if (height <= 0)
      height = this.Height;

    var bmp = new Bitmap(width, height);
    using (var g = Graphics.FromImage(bmp)) {
      g.Clear(this.BackColor);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

      var context = this._CreateRenderContext(g, new RectangleF(0, 0, width, height));
      this._RenderChart(context);
    }
    return bmp;
  }

  /// <summary>
  /// Saves the chart as an image file.
  /// </summary>
  public void SaveAsImage(string path, ImageFormat format = null) {
    using var bmp = this.ToImage();
    bmp.Save(path, format ?? ImageFormat.Png);
  }

  /// <summary>
  /// Registers a custom renderer for a chart type.
  /// </summary>
  public static void RegisterRenderer(AdvancedChartType chartType, ChartRenderer renderer)
    => RendererCache[chartType] = renderer;

  /// <summary>
  /// Gets the renderer for a chart type.
  /// </summary>
  public static ChartRenderer GetRenderer(AdvancedChartType chartType)
    => RendererCache.TryGetValue(chartType, out var renderer) ? renderer : null;

  #endregion

  #region Protected Methods

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    base.OnPaint(e);
    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    var context = this._CreateRenderContext(e.Graphics, this.ClientRectangle);
    this._RenderChart(context);
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (this._enablePan && this._panStart.HasValue) {
      this._HandlePan(e.Location);
      return;
    }

    this._HandleHover(e.Location);
  }

  /// <inheritdoc />
  protected override void OnMouseDown(MouseEventArgs e) {
    base.OnMouseDown(e);

    if (this._enablePan && e.Button == MouseButtons.Left && this._plotArea.Contains(e.Location)) {
      this._panStart = e.Location;
      this.Cursor = Cursors.Hand;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);

    if (this._panStart.HasValue) {
      this._panStart = null;
      this.Cursor = Cursors.Default;
    }

    this._HandleClick(e.Location, e.Button);
  }

  /// <inheritdoc />
  protected override void OnMouseWheel(MouseEventArgs e) {
    base.OnMouseWheel(e);

    if (this._enableZoom && this._plotArea.Contains(e.Location))
      this._HandleZoom(e.Location, e.Delta);
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    this._toolTip?.Hide(this);
    this._hoveredPoint = null;
    this._hoveredSeries = null;
    this._highlightedLegendIndex = -1;
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._toolTip?.Dispose();
      this._toolTip = null;
      this._animationTimer?.Dispose();
      this._animationTimer = null;
    }
    base.Dispose(disposing);
  }

  /// <summary>Raises the DataPointClicked event.</summary>
  protected virtual void OnDataPointClicked(ChartClickEventArgs e) => this.DataPointClicked?.Invoke(this, e);

  /// <summary>Raises the DataPointHovered event.</summary>
  protected virtual void OnDataPointHovered(ChartHoverEventArgs e) => this.DataPointHovered?.Invoke(this, e);

  /// <summary>Raises the SelectionChanged event.</summary>
  protected virtual void OnSelectionChanged(ChartSelectionEventArgs e) => this.SelectionChanged?.Invoke(this, e);

  /// <summary>Raises the ZoomChanged event.</summary>
  protected virtual void OnZoomChanged(ChartZoomEventArgs e) => this.ZoomChanged?.Invoke(this, e);

  #endregion

  #region Private Methods

  private void _RegisterDefaultRenderers() {
    // Register built-in renderers (lazy - will be created on demand)
  }

  private ChartRenderer _GetRenderer() {
    if (this._customRenderer != null)
      return this._customRenderer;

    if (RendererCache.TryGetValue(this._chartType, out var renderer))
      return renderer;

    // Create and cache default renderers
    renderer = this._CreateDefaultRenderer(this._chartType);
    if (renderer != null)
      RendererCache[this._chartType] = renderer;

    return renderer;
  }

  private ChartRenderer _CreateDefaultRenderer(AdvancedChartType chartType) {
    return chartType switch {
      // Comparison Charts
      AdvancedChartType.Bar => new Renderers.BarChartRenderer(),
      AdvancedChartType.Column => new Renderers.ColumnChartRenderer(),
      AdvancedChartType.GroupedBar => new Renderers.GroupedBarRenderer(),
      AdvancedChartType.GroupedColumn => new Renderers.GroupedColumnRenderer(),
      AdvancedChartType.StackedBar or AdvancedChartType.StackedBar100 => new Renderers.StackedBarRenderer(),
      AdvancedChartType.StackedColumn or AdvancedChartType.StackedColumn100 => new Renderers.StackedColumnRenderer(),
      AdvancedChartType.DivergingStackedBar => new Renderers.DivergingStackedBarRenderer(),
      AdvancedChartType.Lollipop => new Renderers.LollipopChartRenderer(),
      AdvancedChartType.DotPlot => new Renderers.DotPlotRenderer(),
      AdvancedChartType.Dumbbell => new Renderers.DumbbellChartRenderer(),
      AdvancedChartType.Bullet => new Renderers.BulletChartRenderer(),
      AdvancedChartType.Radar => new Renderers.RadarChartRenderer(),
      AdvancedChartType.PolarArea => new Renderers.PolarAreaRenderer(),
      AdvancedChartType.Nightingale => new Renderers.NightingaleChartRenderer(),
      AdvancedChartType.RangePlot => new Renderers.RangePlotRenderer(),
      AdvancedChartType.SmallMultiples => new Renderers.SmallMultiplesRenderer(),

      // Trend/Time Series Charts
      AdvancedChartType.Line or AdvancedChartType.MultiLine => new Renderers.LineChartRenderer(),
      AdvancedChartType.Spline => new Renderers.SplineChartRenderer(),
      AdvancedChartType.Area => new Renderers.AreaChartRenderer(),
      AdvancedChartType.StackedArea or AdvancedChartType.StackedArea100 => new Renderers.StackedAreaRenderer(),
      AdvancedChartType.Step => new Renderers.StepChartRenderer(),
      AdvancedChartType.StepArea => new Renderers.StepAreaRenderer(),
      AdvancedChartType.StreamGraph => new Renderers.StreamGraphRenderer(),
      AdvancedChartType.Sparkline => new Renderers.SparklineRenderer(),
      AdvancedChartType.RangeArea => new Renderers.RangeAreaRenderer(),
      AdvancedChartType.BumpArea => new Renderers.BumpAreaRenderer(),
      AdvancedChartType.Barcode => new Renderers.BarcodeChartRenderer(),

      // Part-to-Whole Charts
      AdvancedChartType.Pie => new Renderers.PieChartRenderer(),
      AdvancedChartType.Donut => new Renderers.DonutChartRenderer(),
      AdvancedChartType.SemiCircleDonut => new Renderers.SemiCircleDonutRenderer(),
      AdvancedChartType.NestedDonut => new Renderers.NestedDonutRenderer(),
      AdvancedChartType.Treemap => new Renderers.TreemapRenderer(),
      AdvancedChartType.CircularTreemap => new Renderers.CircularTreemapRenderer(),
      AdvancedChartType.ConvexTreemap => new Renderers.ConvexTreemapRenderer(),
      AdvancedChartType.Sunburst => new Renderers.SunburstRenderer(),
      AdvancedChartType.Waffle => new Renderers.WaffleChartRenderer(),
      AdvancedChartType.Icicle => new Renderers.IcicleChartRenderer(),
      AdvancedChartType.Mosaic or AdvancedChartType.Marimekko => new Renderers.MosaicChartRenderer(),
      AdvancedChartType.Parliament => new Renderers.ParliamentChartRenderer(),
      AdvancedChartType.Unit => new Renderers.UnitChartRenderer(),

      // Distribution Charts
      AdvancedChartType.Histogram => new Renderers.HistogramRenderer(),
      AdvancedChartType.RadialHistogram => new Renderers.RadialHistogramRenderer(),
      AdvancedChartType.BoxPlot => new Renderers.BoxPlotRenderer(),
      AdvancedChartType.Violin => new Renderers.ViolinPlotRenderer(),
      AdvancedChartType.Density => new Renderers.DensityPlotRenderer(),
      AdvancedChartType.Beeswarm => new Renderers.BeeswarmRenderer(),
      AdvancedChartType.StripPlot => new Renderers.StripPlotRenderer(),
      AdvancedChartType.JitterPlot => new Renderers.JitterPlotRenderer(),
      AdvancedChartType.Ridgeline => new Renderers.RidgelineRenderer(),
      AdvancedChartType.Horizon => new Renderers.HorizonChartRenderer(),
      AdvancedChartType.Cumulative => new Renderers.CumulativeRenderer(),
      AdvancedChartType.PopulationPyramid => new Renderers.PopulationPyramidRenderer(),
      AdvancedChartType.OneDimensionalHeatmap => new Renderers.OneDimensionalHeatmapRenderer(),

      // Correlation Charts
      AdvancedChartType.Scatter => new Renderers.ScatterChartRenderer(),
      AdvancedChartType.CategoricalScatter => new Renderers.CategoricalScatterRenderer(),
      AdvancedChartType.Bubble => new Renderers.BubbleChartRenderer(),
      AdvancedChartType.ConnectedScatter => new Renderers.ConnectedScatterRenderer(),
      AdvancedChartType.Heatmap => new Renderers.HeatmapRenderer(),
      AdvancedChartType.Correlogram => new Renderers.CorrelogramRenderer(),
      AdvancedChartType.ScatterMatrix => new Renderers.ScatterMatrixRenderer(),
      AdvancedChartType.Hexbin => new Renderers.HexbinRenderer(),
      AdvancedChartType.Contour => new Renderers.ContourPlotRenderer(),
      AdvancedChartType.QuadrantChart => new Renderers.QuadrantChartRenderer(),
      AdvancedChartType.MatrixChart => new Renderers.MatrixChartRenderer(),

      // Ranking Charts
      AdvancedChartType.OrderedBar => new Renderers.OrderedBarRenderer(),
      AdvancedChartType.Slope => new Renderers.SlopeChartRenderer(),
      AdvancedChartType.Bump => new Renderers.BumpChartRenderer(),
      AdvancedChartType.ParallelCoordinates => new Renderers.ParallelCoordinatesRenderer(),
      AdvancedChartType.RadialBar => new Renderers.RadialBarRenderer(),
      AdvancedChartType.TableHeatmap => new Renderers.TableHeatmapRenderer(),
      AdvancedChartType.TableChart => new Renderers.TableChartRenderer(),

      // Geospatial Charts
      AdvancedChartType.Choropleth => new Renderers.ChoroplethRenderer(),
      AdvancedChartType.GeographicHeatmap => new Renderers.GeographicHeatmapRenderer(),
      AdvancedChartType.TileMap => new Renderers.TileMapRenderer(),
      AdvancedChartType.BubbleMap => new Renderers.BubbleMapRenderer(),
      AdvancedChartType.ConnectionMap => new Renderers.ConnectionMapRenderer(),
      AdvancedChartType.DotMap => new Renderers.DotMapRenderer(),

      // Temporal Charts
      AdvancedChartType.Timeline => new Renderers.TimelineChartRenderer(),
      AdvancedChartType.Gantt => new Renderers.GanttChartRenderer(),
      AdvancedChartType.CalendarHeatmap => new Renderers.CalendarHeatmapRenderer(),
      AdvancedChartType.Seasonal => new Renderers.SeasonalChartRenderer(),
      AdvancedChartType.Spiral => new Renderers.SpiralPlotRenderer(),

      // Financial Charts
      AdvancedChartType.Candlestick => new Renderers.CandlestickRenderer(),
      AdvancedChartType.OHLC => new Renderers.OHLCRenderer(),
      AdvancedChartType.Kagi => new Renderers.KagiChartRenderer(),
      AdvancedChartType.Renko => new Renderers.RenkoChartRenderer(),
      AdvancedChartType.Waterfall => new Renderers.WaterfallChartRenderer(),
      AdvancedChartType.PointFigure => new Renderers.PointFigureRenderer(),

      // Specialized Charts
      AdvancedChartType.Funnel => new Renderers.FunnelChartRenderer(),
      AdvancedChartType.Pyramid => new Renderers.PyramidChartRenderer(),
      AdvancedChartType.Gauge => new Renderers.GaugeChartRenderer(),
      AdvancedChartType.CircularGauge => new Renderers.CircularGaugeRenderer(),
      AdvancedChartType.WordCloud => new Renderers.WordCloudRenderer(),
      AdvancedChartType.Pictogram => new Renderers.PictogramRenderer(),
      AdvancedChartType.Venn => new Renderers.VennDiagramRenderer(),
      AdvancedChartType.EulerDiagram => new Renderers.EulerDiagramRenderer(),
      AdvancedChartType.IconArray => new Renderers.IconArrayRenderer(),

      _ => new Renderers.LineChartRenderer() // Default fallback
    };
  }

  private ChartRenderContext _CreateRenderContext(Graphics g, RectangleF bounds) {
    this._CalculateLayout(g, bounds);
    this._CalculateAxisBounds();

    return new ChartRenderContext {
      Graphics = g,
      Chart = this,
      TotalBounds = bounds,
      PlotArea = this._plotArea,
      XAxis = this._xAxis,
      YAxis = this._yAxis,
      Y2Axis = this._y2Axis,
      XMin = this._isZoomed ? this._zoomXMin : this._xMin,
      XMax = this._isZoomed ? this._zoomXMax : this._xMax,
      YMin = this._isZoomed ? this._zoomYMin : this._yMin,
      YMax = this._isZoomed ? this._zoomYMax : this._yMax,
      Y2Min = this._y2Min,
      Y2Max = this._y2Max,
      Series = this._series.Where(s => s.Visible).ToList(),
      HighlightedSeriesIndex = this._hoveredSeries != null ? this._series.IndexOf(this._hoveredSeries) : (int?)null,
      ShowDataLabels = this._showDataLabels,
      DataLabelPosition = this._dataLabelPosition,
      AnimationProgress = this._animationProgress
    };
  }

  private void _CalculateLayout(Graphics g, RectangleF bounds) {
    var left = bounds.Left + this._padding;
    var top = bounds.Top + this._padding;
    var right = bounds.Right - this._padding;
    var bottom = bounds.Bottom - this._padding;

    // Title area
    if (!string.IsNullOrEmpty(this._title)) {
      var titleFont = this._titleFont ?? new Font(this.Font.FontFamily, this.Font.Size + 4, FontStyle.Bold);
      var titleSize = g.MeasureString(this._title, titleFont);
      top += titleSize.Height + 5;
    }

    // Subtitle area
    if (!string.IsNullOrEmpty(this._subtitle)) {
      var subtitleFont = this._subtitleFont ?? this.Font;
      var subtitleSize = g.MeasureString(this._subtitle, subtitleFont);
      top += subtitleSize.Height + 5;
    }

    // Get renderer to check if axes are used
    var renderer = this._GetRenderer();
    var usesAxes = renderer?.UsesAxes ?? true;

    if (usesAxes) {
      // Y-axis label area
      if (this._yAxis.Visible) {
        left += 50; // Space for labels
        if (!string.IsNullOrEmpty(this._yAxis.Title))
          left += 20;
      }

      // X-axis label area
      if (this._xAxis.Visible) {
        bottom -= 30; // Space for labels
        if (!string.IsNullOrEmpty(this._xAxis.Title))
          bottom -= 20;
      }

      // Secondary Y-axis
      if (this._y2Axis.Visible) {
        right -= 50;
        if (!string.IsNullOrEmpty(this._y2Axis.Title))
          right -= 20;
      }
    }

    // Legend area
    if (this._legend.Visible && this._legend.Position != ChartLegendPosition.None) {
      this._legendItems = this._GetLegendItems();
      var legendSize = this._legend.CalculateSize(g, this._legendItems);

      switch (this._legend.Position) {
        case ChartLegendPosition.Right:
        case ChartLegendPosition.TopRight:
        case ChartLegendPosition.BottomRight:
          right -= legendSize.Width + 10;
          break;
        case ChartLegendPosition.Left:
        case ChartLegendPosition.TopLeft:
        case ChartLegendPosition.BottomLeft:
          left += legendSize.Width + 10;
          break;
        case ChartLegendPosition.Top:
          top += legendSize.Height + 5;
          break;
        case ChartLegendPosition.Bottom:
          bottom -= legendSize.Height + 5;
          break;
      }
    }

    this._plotArea = new RectangleF(left, top, right - left, bottom - top);
  }

  private void _CalculateAxisBounds() {
    this._xMin = double.MaxValue;
    this._xMax = double.MinValue;
    this._yMin = double.MaxValue;
    this._yMax = double.MinValue;
    this._y2Min = double.MaxValue;
    this._y2Max = double.MinValue;

    foreach (var series in this._series) {
      if (!series.Visible || series.Points.Count == 0)
        continue;

      foreach (var point in series.Points) {
        this._xMin = Math.Min(this._xMin, point.X);
        this._xMax = Math.Max(this._xMax, point.X);

        if (series.YAxisType == ChartAxisType.Primary) {
          this._yMin = Math.Min(this._yMin, point.Y);
          this._yMax = Math.Max(this._yMax, point.Y);
        } else {
          this._y2Min = Math.Min(this._y2Min, point.Y);
          this._y2Max = Math.Max(this._y2Max, point.Y);
        }
      }
    }

    // For stacked chart types, calculate Y bounds from stacked totals instead of individual values
    if (this._IsStackedChartType())
      this._CalculateStackedYBounds();

    // For waterfall charts, calculate Y bounds from running totals
    if (this._chartType == AdvancedChartType.Waterfall)
      this._CalculateWaterfallYBounds();

    // Include OHLC data bounds
    if (this._ohlcData.Count > 0) {
      for (var i = 0; i < this._ohlcData.Count; ++i) {
        var ohlc = this._ohlcData[i];
        this._xMin = Math.Min(this._xMin, i);
        this._xMax = Math.Max(this._xMax, i);
        this._yMin = Math.Min(this._yMin, ohlc.Low);
        this._yMax = Math.Max(this._yMax, ohlc.High);
      }
    }

    // Include BoxPlot data bounds
    if (this._boxPlotData.Count > 0) {
      for (var i = 0; i < this._boxPlotData.Count; ++i) {
        var box = this._boxPlotData[i];
        this._xMin = Math.Min(this._xMin, i);
        this._xMax = Math.Max(this._xMax, i + 1);
        this._yMin = Math.Min(this._yMin, box.Minimum);
        this._yMax = Math.Max(this._yMax, box.Maximum);
      }
    }

    // Include HeatmapData bounds
    if (this._heatmapData.Count > 0) {
      foreach (var cell in this._heatmapData) {
        this._xMin = Math.Min(this._xMin, cell.Column);
        this._xMax = Math.Max(this._xMax, cell.Column + 1);
        this._yMin = Math.Min(this._yMin, cell.Row);
        this._yMax = Math.Max(this._yMax, cell.Row + 1);
      }
    }

    // Apply nice bounds if auto-scaling
    if (this._xAxis.AutoMinimum || this._xAxis.AutoMaximum) {
      var (min, max, _) = ChartAxis.CalculateNiceBounds(this._xMin, this._xMax);
      if (this._xAxis.AutoMinimum)
        this._xMin = min;
      if (this._xAxis.AutoMaximum)
        this._xMax = max;
    } else {
      this._xMin = this._xAxis.Minimum;
      this._xMax = this._xAxis.Maximum;
    }

    if (this._yAxis.AutoMinimum || this._yAxis.AutoMaximum) {
      var (min, max, _) = ChartAxis.CalculateNiceBounds(this._yMin, this._yMax);
      if (this._yAxis.AutoMinimum)
        this._yMin = min;
      if (this._yAxis.AutoMaximum)
        this._yMax = max;
    } else {
      this._yMin = this._yAxis.Minimum;
      this._yMax = this._yAxis.Maximum;
    }

    // Ensure valid ranges
    if (this._xMin >= this._xMax)
      this._xMax = this._xMin + 1;
    if (this._yMin >= this._yMax)
      this._yMax = this._yMin + 1;
  }

  private bool _IsStackedChartType() =>
    this._chartType is AdvancedChartType.StackedArea
      or AdvancedChartType.StackedArea100
      or AdvancedChartType.StackedBar
      or AdvancedChartType.StackedColumn
      or AdvancedChartType.StackedBar100
      or AdvancedChartType.StackedColumn100
      or AdvancedChartType.DivergingStackedBar
      or AdvancedChartType.StreamGraph;

  private void _CalculateStackedYBounds() {
    var visibleSeries = this._series.Where(s => s.Visible && s.Points.Count > 0).ToList();
    if (visibleSeries.Count == 0)
      return;

    // Collect all unique X values
    var xValues = new SortedSet<double>();
    foreach (var series in visibleSeries)
    foreach (var point in series.Points)
      xValues.Add(point.X);

    // Calculate max stacked value at each X point
    var maxPositiveStack = 0.0;
    var maxNegativeStack = 0.0;

    foreach (var x in xValues) {
      var positiveSum = 0.0;
      var negativeSum = 0.0;

      foreach (var series in visibleSeries) {
        var point = series.Points.FirstOrDefault(p => Math.Abs(p.X - x) < 0.001);
        if (point == null)
          continue;

        if (point.Y >= 0)
          positiveSum += point.Y;
        else
          negativeSum += Math.Abs(point.Y);
      }

      maxPositiveStack = Math.Max(maxPositiveStack, positiveSum);
      maxNegativeStack = Math.Max(maxNegativeStack, negativeSum);
    }

    // For percentage stacked charts, Y range is always 0-100
    if (this._chartType is AdvancedChartType.StackedArea100 or AdvancedChartType.StackedBar100 or AdvancedChartType.StackedColumn100) {
      this._yMin = 0;
      this._yMax = 100;
      return;
    }

    // For diverging stacked bar, we need symmetric bounds
    if (this._chartType == AdvancedChartType.DivergingStackedBar) {
      var maxStack = Math.Max(maxPositiveStack, maxNegativeStack);
      this._yMin = -maxStack;
      this._yMax = maxStack;
      return;
    }

    // For regular stacked charts, use 0 to max stacked value
    this._yMin = maxNegativeStack > 0 ? -maxNegativeStack : 0;
    this._yMax = maxPositiveStack;
  }

  private void _CalculateWaterfallYBounds() {
    // Waterfall charts display running totals, not individual values
    // We need to calculate Y bounds from the cumulative sums
    var visibleSeries = this._series.Where(s => s.Visible && s.Points.Count > 0).ToList();
    if (visibleSeries.Count == 0)
      return;

    var minRunningTotal = double.MaxValue;
    var maxRunningTotal = double.MinValue;

    foreach (var series in visibleSeries) {
      var runningTotal = 0.0;

      foreach (var point in series.Points) {
        // First point and any "total" points start fresh
        // Total points typically have a specific label or are first/last
        var isTotal = point.Label != null &&
                      (point.Label.Equals("Start", StringComparison.OrdinalIgnoreCase) ||
                       point.Label.Equals("End", StringComparison.OrdinalIgnoreCase) ||
                       point.Label.Equals("Total", StringComparison.OrdinalIgnoreCase));

        if (isTotal)
          runningTotal = point.Y;
        else
          runningTotal += point.Y;

        // Track both the current running total and the previous running total
        // because the bar spans between them
        minRunningTotal = Math.Min(minRunningTotal, runningTotal);
        maxRunningTotal = Math.Max(maxRunningTotal, runningTotal);

        // For negative values, the bar goes from runningTotal to runningTotal - point.Y
        // So we also need to track where the bar started
        if (!isTotal) {
          var barStart = runningTotal - point.Y;
          minRunningTotal = Math.Min(minRunningTotal, barStart);
          maxRunningTotal = Math.Max(maxRunningTotal, barStart);
        }
      }
    }

    // Update bounds if we found valid values
    if (minRunningTotal < double.MaxValue && maxRunningTotal > double.MinValue) {
      this._yMin = minRunningTotal;
      this._yMax = maxRunningTotal;
    }
  }

  private void _RenderChart(ChartRenderContext context) {
    this._hitTestRects.Clear();

    // Draw title
    this._DrawTitle(context.Graphics);

    var renderer = this._GetRenderer();
    if (renderer == null)
      return;

    if (renderer.UsesAxes) {
      // Draw plot area background
      if (this._plotAreaBackground != Color.Transparent) {
        using var bgBrush = new SolidBrush(this._plotAreaBackground);
        context.Graphics.FillRectangle(bgBrush, this._plotArea);
      }

      // Draw axes and grid
      this._DrawAxes(context);

      // Draw plot area border
      if (this._plotAreaBorderWidth > 0) {
        using var borderPen = new Pen(this._plotAreaBorderColor, this._plotAreaBorderWidth);
        context.Graphics.DrawRectangle(borderPen, this._plotArea.X, this._plotArea.Y, this._plotArea.Width, this._plotArea.Height);
      }
    }

    // Render the chart data with clipping to prevent drawing outside plot area
    var previousClip = context.Graphics.Clip;
    context.Graphics.SetClip(this._plotArea);
    try {
      renderer.Render(context);
    } finally {
      context.Graphics.Clip = previousClip;
    }

    // Copy hit test rects
    foreach (var kvp in context.HitTestRects)
      this._hitTestRects[kvp.Key] = kvp.Value;

    // Draw legend
    if (this._legend.Visible && this._legend.Position != ChartLegendPosition.None && this._legendItems.Count > 0) {
      var legendSize = this._legend.CalculateSize(context.Graphics, this._legendItems);
      var legendBounds = this._legend.CalculateBounds(context.TotalBounds, legendSize);
      this._legend.Draw(context.Graphics, legendBounds, this._legendItems, this._highlightedLegendIndex >= 0 ? this._highlightedLegendIndex : (int?)null);
    }

    // Draw crosshair
    if (this._enableCrosshair && this._hoveredPoint != null)
      this._DrawCrosshair(context);
  }

  private void _DrawTitle(Graphics g) {
    var y = (float)this._padding;

    if (!string.IsNullOrEmpty(this._title)) {
      var titleFont = this._titleFont ?? new Font(this.Font.FontFamily, this.Font.Size + 4, FontStyle.Bold);
      var titleSize = g.MeasureString(this._title, titleFont);
      using var brush = new SolidBrush(this._titleColor);
      g.DrawString(this._title, titleFont, brush, (this.Width - titleSize.Width) / 2, y);
      y += titleSize.Height + 5;
    }

    if (!string.IsNullOrEmpty(this._subtitle)) {
      var subtitleFont = this._subtitleFont ?? this.Font;
      var subtitleSize = g.MeasureString(this._subtitle, subtitleFont);
      using var brush = new SolidBrush(this._subtitleColor);
      g.DrawString(this._subtitle, subtitleFont, brush, (this.Width - subtitleSize.Width) / 2, y);
    }
  }

  private void _DrawAxes(ChartRenderContext context) {
    var g = context.Graphics;
    var xMin = context.XMin;
    var xMax = context.XMax;
    var yMin = context.YMin;
    var yMax = context.YMax;

    // Draw Y-axis
    if (this._yAxis.Visible) {
      using var axisPen = new Pen(this._yAxis.LineColor, this._yAxis.LineWidth);
      g.DrawLine(axisPen, this._plotArea.Left, this._plotArea.Top, this._plotArea.Left, this._plotArea.Bottom);

      // Grid and labels
      var interval = this._yAxis.Interval ?? (yMax - yMin) / 5;
      var value = Math.Ceiling(yMin / interval) * interval;

      while (value <= yMax) {
        var y = this._plotArea.Bottom - (float)((value - yMin) / (yMax - yMin) * this._plotArea.Height);

        if (this._yAxis.ShowGrid && value > yMin) {
          using var gridPen = new Pen(this._yAxis.GridColor, 1) { DashStyle = GetDashStyle(this._yAxis.GridLineStyle) };
          g.DrawLine(gridPen, this._plotArea.Left, y, this._plotArea.Right, y);
        }

        // Tick and label
        g.DrawLine(axisPen, this._plotArea.Left - this._yAxis.MajorTickLength, y, this._plotArea.Left, y);
        var label = this._yAxis.FormatValue(value);
        var labelSize = g.MeasureString(label, this._yAxis.GetEffectiveLabelFont());
        using var labelBrush = new SolidBrush(this._yAxis.LabelColor);
        g.DrawString(label, this._yAxis.GetEffectiveLabelFont(), labelBrush, this._plotArea.Left - labelSize.Width - this._yAxis.MajorTickLength - 2, y - labelSize.Height / 2);

        value += interval;
      }

      // Y-axis title
      if (!string.IsNullOrEmpty(this._yAxis.Title)) {
        var titleFont = this._yAxis.GetEffectiveTitleFont();
        var titleSize = g.MeasureString(this._yAxis.Title, titleFont);
        var state = g.Save();
        g.TranslateTransform(this._padding + 5, this._plotArea.Top + this._plotArea.Height / 2 + titleSize.Width / 2);
        g.RotateTransform(-90);
        using var titleBrush = new SolidBrush(this._yAxis.TitleColor);
        g.DrawString(this._yAxis.Title, titleFont, titleBrush, 0, 0);
        g.Restore(state);
      }
    }

    // Draw X-axis
    if (this._xAxis.Visible) {
      using var axisPen = new Pen(this._xAxis.LineColor, this._xAxis.LineWidth);
      g.DrawLine(axisPen, this._plotArea.Left, this._plotArea.Bottom, this._plotArea.Right, this._plotArea.Bottom);

      // Grid and labels
      var interval = this._xAxis.Interval ?? (xMax - xMin) / 5;
      var value = Math.Ceiling(xMin / interval) * interval;

      while (value <= xMax) {
        var x = this._plotArea.Left + (float)((value - xMin) / (xMax - xMin) * this._plotArea.Width);

        if (this._xAxis.ShowGrid && value > xMin) {
          using var gridPen = new Pen(this._xAxis.GridColor, 1) { DashStyle = GetDashStyle(this._xAxis.GridLineStyle) };
          g.DrawLine(gridPen, x, this._plotArea.Top, x, this._plotArea.Bottom);
        }

        // Tick and label
        g.DrawLine(axisPen, x, this._plotArea.Bottom, x, this._plotArea.Bottom + this._xAxis.MajorTickLength);
        var label = this._xAxis.FormatValue(value);
        var labelSize = g.MeasureString(label, this._xAxis.GetEffectiveLabelFont());
        using var labelBrush = new SolidBrush(this._xAxis.LabelColor);
        g.DrawString(label, this._xAxis.GetEffectiveLabelFont(), labelBrush, x - labelSize.Width / 2, this._plotArea.Bottom + this._xAxis.MajorTickLength + 2);

        value += interval;
      }

      // X-axis title
      if (!string.IsNullOrEmpty(this._xAxis.Title)) {
        var titleFont = this._xAxis.GetEffectiveTitleFont();
        var titleSize = g.MeasureString(this._xAxis.Title, titleFont);
        using var titleBrush = new SolidBrush(this._xAxis.TitleColor);
        g.DrawString(this._xAxis.Title, titleFont, titleBrush, this._plotArea.Left + this._plotArea.Width / 2 - titleSize.Width / 2, this._plotArea.Bottom + 25);
      }
    }
  }

  private void _DrawCrosshair(ChartRenderContext context) {
    if (this._hoveredPoint == null)
      return;

    var g = context.Graphics;
    var px = this._plotArea.Left + (float)((this._hoveredPoint.X - context.XMin) / (context.XMax - context.XMin) * this._plotArea.Width);
    var py = this._plotArea.Bottom - (float)((this._hoveredPoint.Y - context.YMin) / (context.YMax - context.YMin) * this._plotArea.Height);

    using var pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
    g.DrawLine(pen, px, this._plotArea.Top, px, this._plotArea.Bottom);
    g.DrawLine(pen, this._plotArea.Left, py, this._plotArea.Right, py);
  }

  private static DashStyle GetDashStyle(ChartGridLineStyle style) => style switch {
    ChartGridLineStyle.Solid => DashStyle.Solid,
    ChartGridLineStyle.Dashed => DashStyle.Dash,
    ChartGridLineStyle.Dotted => DashStyle.Dot,
    _ => DashStyle.Solid
  };

  private List<LegendItem> _GetLegendItems() {
    var renderer = this._GetRenderer();
    if (renderer != null)
      return new List<LegendItem>(renderer.GetLegendItems(this));

    return this._series.Select(s => new LegendItem {
      Text = s.Name,
      Color = s.Color,
      Visible = s.Visible,
      Tag = s
    }).ToList();
  }

  private Color _GetSeriesColor(int index) {
    var colors = this._GetColorPalette();
    return colors[index % colors.Length];
  }

  private Color[] _GetColorPalette() => this._colorPalette switch {
    ChartColorPalette.Custom when this._customColors != null => this._customColors,
    ChartColorPalette.Pastel => new[] { Color.FromArgb(174, 198, 207), Color.FromArgb(255, 179, 186), Color.FromArgb(255, 223, 186), Color.FromArgb(255, 255, 186), Color.FromArgb(186, 255, 201), Color.FromArgb(186, 225, 255) },
    ChartColorPalette.Bright => new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple },
    ChartColorPalette.Dark => new[] { Color.DarkRed, Color.DarkOrange, Color.DarkGoldenrod, Color.DarkGreen, Color.DarkBlue, Color.DarkViolet },
    ChartColorPalette.Monochrome => new[] { Color.FromArgb(50, 50, 50), Color.FromArgb(100, 100, 100), Color.FromArgb(150, 150, 150), Color.FromArgb(200, 200, 200) },
    ChartColorPalette.Category10 => new[] { Color.FromArgb(31, 119, 180), Color.FromArgb(255, 127, 14), Color.FromArgb(44, 160, 44), Color.FromArgb(214, 39, 40), Color.FromArgb(148, 103, 189), Color.FromArgb(140, 86, 75), Color.FromArgb(227, 119, 194), Color.FromArgb(127, 127, 127), Color.FromArgb(188, 189, 34), Color.FromArgb(23, 190, 207) },
    ChartColorPalette.Tableau10 => new[] { Color.FromArgb(78, 121, 167), Color.FromArgb(242, 142, 44), Color.FromArgb(225, 87, 89), Color.FromArgb(118, 183, 178), Color.FromArgb(89, 161, 79), Color.FromArgb(237, 201, 73), Color.FromArgb(175, 122, 161), Color.FromArgb(255, 157, 167), Color.FromArgb(156, 117, 95), Color.FromArgb(186, 176, 171) },
    ChartColorPalette.Material => new[] { Color.FromArgb(244, 67, 54), Color.FromArgb(233, 30, 99), Color.FromArgb(156, 39, 176), Color.FromArgb(103, 58, 183), Color.FromArgb(63, 81, 181), Color.FromArgb(33, 150, 243), Color.FromArgb(0, 188, 212), Color.FromArgb(0, 150, 136), Color.FromArgb(76, 175, 80), Color.FromArgb(255, 193, 7) },
    _ => new[] { Color.DodgerBlue, Color.OrangeRed, Color.ForestGreen, Color.DarkViolet, Color.Goldenrod, Color.Crimson, Color.Teal, Color.SlateBlue }
  };

  private void _HandleHover(Point location) {
    // Check legend hit
    if (this._legend.Visible && this._legendItems.Count > 0) {
      var legendIndex = this._legend.HitTest(location, this._legendItems);
      if (legendIndex != this._highlightedLegendIndex) {
        this._highlightedLegendIndex = legendIndex;
        this.Invalidate();
      }
    }

    // Check data point hit
    ChartPoint hoveredPoint = null;
    ChartDataSeries hoveredSeries = null;

    foreach (var kvp in this._hitTestRects) {
      if (kvp.Value.Contains(location)) {
        if (kvp.Key is ChartPoint point) {
          hoveredPoint = point;
          hoveredSeries = this._series.FirstOrDefault(s => s.Points.Contains(point));
          break;
        }
      }
    }

    if (hoveredPoint != this._hoveredPoint) {
      this._hoveredPoint = hoveredPoint;
      this._hoveredSeries = hoveredSeries;

      if (hoveredPoint != null && this._enableTooltips) {
        var tooltipText = hoveredPoint.Label ?? $"X: {hoveredPoint.X:N2}, Y: {hoveredPoint.Y:N2}";
        if (hoveredSeries != null)
          tooltipText = $"{hoveredSeries.Name}\n{tooltipText}";
        this._toolTip?.Show(tooltipText, this, location.X + 15, location.Y + 15);

        this.OnDataPointHovered(new ChartHoverEventArgs(hoveredPoint, hoveredSeries));
      } else
        this._toolTip?.Hide(this);

      this.Invalidate();
    }
  }

  private void _HandleClick(Point location, MouseButtons button) {
    if (button != MouseButtons.Left)
      return;

    // Check legend click for toggle
    if (this._legend.AllowToggle && this._legend.Visible && this._legendItems.Count > 0) {
      var legendIndex = this._legend.HitTest(location, this._legendItems);
      if (legendIndex >= 0 && legendIndex < this._series.Count) {
        this._series[legendIndex].Visible = !this._series[legendIndex].Visible;
        this.Invalidate();
        return;
      }
    }

    // Check data point click
    foreach (var kvp in this._hitTestRects) {
      if (kvp.Value.Contains(location)) {
        if (kvp.Key is ChartPoint point) {
          var series = this._series.FirstOrDefault(s => s.Points.Contains(point));
          this.OnDataPointClicked(new ChartClickEventArgs(point, series, MouseButtons.Left));
          return;
        }
      }
    }
  }

  private void _HandleZoom(Point location, int delta) {
    var zoomFactor = delta > 0 ? 0.9 : 1.1;

    if (!this._isZoomed) {
      this._zoomXMin = this._xMin;
      this._zoomXMax = this._xMax;
      this._zoomYMin = this._yMin;
      this._zoomYMax = this._yMax;
      this._isZoomed = true;
    }

    var xRatio = (location.X - this._plotArea.Left) / this._plotArea.Width;
    var yRatio = (this._plotArea.Bottom - location.Y) / this._plotArea.Height;

    var xPivot = this._zoomXMin + xRatio * (this._zoomXMax - this._zoomXMin);
    var yPivot = this._zoomYMin + yRatio * (this._zoomYMax - this._zoomYMin);

    this._zoomXMin = xPivot - (xPivot - this._zoomXMin) * zoomFactor;
    this._zoomXMax = xPivot + (this._zoomXMax - xPivot) * zoomFactor;
    this._zoomYMin = yPivot - (yPivot - this._zoomYMin) * zoomFactor;
    this._zoomYMax = yPivot + (this._zoomYMax - yPivot) * zoomFactor;

    this.Invalidate();
    this.OnZoomChanged(new ChartZoomEventArgs(
      (this._xMax - this._xMin) / (this._zoomXMax - this._zoomXMin),
      (this._yMax - this._yMin) / (this._zoomYMax - this._zoomYMin)
    ));
  }

  private void _HandlePan(Point location) {
    if (!this._panStart.HasValue)
      return;

    var dx = location.X - this._panStart.Value.X;
    var dy = location.Y - this._panStart.Value.Y;

    if (!this._isZoomed) {
      this._zoomXMin = this._xMin;
      this._zoomXMax = this._xMax;
      this._zoomYMin = this._yMin;
      this._zoomYMax = this._yMax;
      this._isZoomed = true;
    }

    var xDelta = -dx / this._plotArea.Width * (this._zoomXMax - this._zoomXMin);
    var yDelta = dy / this._plotArea.Height * (this._zoomYMax - this._zoomYMin);

    this._zoomXMin += xDelta;
    this._zoomXMax += xDelta;
    this._zoomYMin += yDelta;
    this._zoomYMax += yDelta;

    this._panStart = location;
    this.Invalidate();
  }

  private void _StartAnimation() {
    if (!this._enableAnimation || this._animationDuration <= 0) {
      this._animationProgress = 1.0;
      return;
    }

    // Stop any existing animation first
    this._animationTimer?.Stop();

    this._animationProgress = 0;
    this._animationStartTime = DateTime.Now;

    if (this._animationTimer == null) {
      this._animationTimer = new Timer { Interval = 16 }; // ~60 FPS
      this._animationTimer.Tick += this._OnAnimationTick;
    }

    this._animationTimer.Start();
  }

  private void _OnAnimationTick(object sender, EventArgs e) {
    var elapsed = (DateTime.Now - this._animationStartTime).TotalMilliseconds;
    this._animationProgress = Math.Min(1.0, elapsed / this._animationDuration);

    // Apply easing
    this._animationProgress = this._EaseOutCubic(this._animationProgress);

    // Force immediate repaint for smooth animation
    this.Invalidate();
    this.Update();

    if (this._animationProgress >= 1.0)
      this._animationTimer.Stop();
  }

  private double _EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);

  #endregion
}

#region Event Args

/// <summary>Event arguments for chart click events.</summary>
public class ChartClickEventArgs : EventArgs {
  public ChartPoint DataPoint { get; }
  public ChartDataSeries Series { get; }
  public MouseButtons Button { get; }

  public ChartClickEventArgs(ChartPoint dataPoint, ChartDataSeries series, MouseButtons button) {
    this.DataPoint = dataPoint;
    this.Series = series;
    this.Button = button;
  }
}

/// <summary>Event arguments for chart hover events.</summary>
public class ChartHoverEventArgs : EventArgs {
  public ChartPoint DataPoint { get; }
  public ChartDataSeries Series { get; }

  public ChartHoverEventArgs(ChartPoint dataPoint, ChartDataSeries series) {
    this.DataPoint = dataPoint;
    this.Series = series;
  }
}

/// <summary>Event arguments for chart selection events.</summary>
public class ChartSelectionEventArgs : EventArgs {
  public IList<ChartPoint> SelectedPoints { get; }

  public ChartSelectionEventArgs(IList<ChartPoint> selectedPoints) {
    this.SelectedPoints = selectedPoints;
  }
}

/// <summary>Event arguments for chart zoom events.</summary>
public class ChartZoomEventArgs : EventArgs {
  public double XZoomFactor { get; }
  public double YZoomFactor { get; }

  public ChartZoomEventArgs(double xZoomFactor, double yZoomFactor) {
    this.XZoomFactor = xZoomFactor;
    this.YZoomFactor = yZoomFactor;
  }
}

#endregion
