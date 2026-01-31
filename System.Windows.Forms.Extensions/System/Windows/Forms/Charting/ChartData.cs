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
using System.Linq;

namespace System.Windows.Forms.Charting;

#region Base Data Classes

/// <summary>
/// Represents a basic data point with X and Y values.
/// </summary>
public class ChartPoint {
  /// <summary>Gets or sets the X value.</summary>
  public double X { get; set; }

  /// <summary>Gets or sets the Y value.</summary>
  public double Y { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets an optional color override.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public ChartPoint() { }

  public ChartPoint(double x, double y, string label = null) {
    this.X = x;
    this.Y = y;
    this.Label = label;
  }
}

/// <summary>
/// Represents a data series containing multiple points.
/// </summary>
public class ChartDataSeries {
  private readonly AdvancedChart _owner;
  private string _name;
  private Color _color = Color.DodgerBlue;
  private int _lineWidth = 2;
  private bool _showMarkers = true;
  private int _markerSize = 6;
  private AdvancedMarkerStyle _markerStyle = AdvancedMarkerStyle.Circle;
  private bool _visible = true;
  private AdvancedChartType? _chartTypeOverride;
  private ChartAxisType _yAxisType = ChartAxisType.Primary;

  internal ChartDataSeries(AdvancedChart owner) {
    this._owner = owner;
    this.Points = new ChartPointCollection(owner);
  }

  /// <summary>Gets or sets the name of the series.</summary>
  public string Name {
    get => this._name;
    set {
      this._name = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the color of the series.</summary>
  public Color Color {
    get => this._color;
    set {
      this._color = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the line width.</summary>
  public int LineWidth {
    get => this._lineWidth;
    set {
      this._lineWidth = Math.Max(1, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to show markers on data points.</summary>
  public bool ShowMarkers {
    get => this._showMarkers;
    set {
      this._showMarkers = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the marker size.</summary>
  public int MarkerSize {
    get => this._markerSize;
    set {
      this._markerSize = Math.Max(2, value);
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the marker style.</summary>
  public AdvancedMarkerStyle MarkerStyle {
    get => this._markerStyle;
    set {
      this._markerStyle = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether the series is visible.</summary>
  public bool Visible {
    get => this._visible;
    set {
      this._visible = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets an optional chart type override for this series.</summary>
  public AdvancedChartType? ChartTypeOverride {
    get => this._chartTypeOverride;
    set {
      this._chartTypeOverride = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets which Y-axis this series uses.</summary>
  public ChartAxisType YAxisType {
    get => this._yAxisType;
    set {
      this._yAxisType = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets the collection of data points.</summary>
  public ChartPointCollection Points { get; }

  /// <summary>Adds a data point to the series.</summary>
  public void AddPoint(double x, double y, string label = null)
    => this.Points.Add(new ChartPoint(x, y, label));

  /// <summary>Adds multiple data points to the series.</summary>
  public void AddPoints(IEnumerable<(double x, double y)> points) {
    foreach (var (x, y) in points)
      this.AddPoint(x, y);
  }

  /// <summary>Removes all data points from the series.</summary>
  public void Clear() => this.Points.Clear();
}

/// <summary>
/// Collection of chart data points.
/// </summary>
public class ChartPointCollection : List<ChartPoint> {
  private readonly AdvancedChart _owner;

  internal ChartPointCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(ChartPoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new bool Remove(ChartPoint point) {
    var result = base.Remove(point);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<ChartPoint> points) {
    base.AddRange(points);
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of chart data series.
/// </summary>
public class ChartSeriesCollection : List<ChartDataSeries> {
  private readonly AdvancedChart _owner;

  internal ChartSeriesCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(ChartDataSeries series) {
    base.Add(series);
    this._owner?.Invalidate();
  }

  public new bool Remove(ChartDataSeries series) {
    var result = base.Remove(series);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Financial Data

/// <summary>
/// Represents OHLC (Open, High, Low, Close) data for financial charts.
/// </summary>
public class OHLCDataPoint {
  /// <summary>Gets or sets the date/time.</summary>
  public DateTime Date { get; set; }

  /// <summary>Gets or sets the opening price.</summary>
  public double Open { get; set; }

  /// <summary>Gets or sets the highest price.</summary>
  public double High { get; set; }

  /// <summary>Gets or sets the lowest price.</summary>
  public double Low { get; set; }

  /// <summary>Gets or sets the closing price.</summary>
  public double Close { get; set; }

  /// <summary>Gets or sets the trading volume.</summary>
  public double Volume { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets whether this is a bullish (price went up) candle.</summary>
  public bool IsBullish => this.Close >= this.Open;

  public OHLCDataPoint() { }

  public OHLCDataPoint(DateTime date, double open, double high, double low, double close, double volume = 0) {
    this.Date = date;
    this.Open = open;
    this.High = high;
    this.Low = low;
    this.Close = close;
    this.Volume = volume;
  }
}

/// <summary>
/// Collection of OHLC data points.
/// </summary>
public class OHLCDataCollection : List<OHLCDataPoint> {
  private readonly AdvancedChart _owner;

  internal OHLCDataCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(OHLCDataPoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Hierarchical Data

/// <summary>
/// Represents a node in a hierarchical data structure (for treemaps, sunbursts, etc.).
/// </summary>
public class HierarchicalDataPoint {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the display label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the parent identifier (null for root nodes).</summary>
  public string ParentId { get; set; }

  /// <summary>Gets or sets the value (size/weight).</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets or sets child nodes.</summary>
  public List<HierarchicalDataPoint> Children { get; set; } = new();

  /// <summary>Gets the depth level in the hierarchy.</summary>
  public int Depth { get; internal set; }

  public HierarchicalDataPoint() { }

  public HierarchicalDataPoint(string id, string label, double value, string parentId = null) {
    this.Id = id;
    this.Label = label;
    this.Value = value;
    this.ParentId = parentId;
  }
}

/// <summary>
/// Collection of hierarchical data points.
/// </summary>
public class HierarchicalDataCollection : List<HierarchicalDataPoint> {
  private readonly AdvancedChart _owner;

  internal HierarchicalDataCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(HierarchicalDataPoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  /// <summary>
  /// Builds the tree structure from a flat list with parent references.
  /// </summary>
  public HierarchicalDataPoint BuildTree() {
    var lookup = new Dictionary<string, HierarchicalDataPoint>();
    HierarchicalDataPoint root = null;

    foreach (var item in this)
      lookup[item.Id] = item;

    foreach (var item in this) {
      if (string.IsNullOrEmpty(item.ParentId)) {
        root = item;
        item.Depth = 0;
      } else if (lookup.TryGetValue(item.ParentId, out var parent)) {
        parent.Children.Add(item);
        item.Depth = parent.Depth + 1;
      }
    }

    return root;
  }
}

#endregion

#region Network Data

/// <summary>
/// Represents a node in a network or connection map.
/// </summary>
public class NetworkNode {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the display label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the node size.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets the node color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the node position (normalized 0-100 coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the group/category this node belongs to.</summary>
  public string Group { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public NetworkNode() { }

  public NetworkNode(string id, string label = null, double size = 1) {
    this.Id = id;
    this.Label = label ?? id;
    this.Size = size;
  }
}

/// <summary>
/// Represents an edge/link between nodes in a network.
/// </summary>
public class NetworkEdge {
  /// <summary>Gets or sets the source node identifier.</summary>
  public string Source { get; set; }

  /// <summary>Gets or sets the target node identifier.</summary>
  public string Target { get; set; }

  /// <summary>Gets or sets the edge weight/value.</summary>
  public double Weight { get; set; } = 1;

  /// <summary>Gets or sets whether the edge is directed.</summary>
  public bool Directed { get; set; }

  /// <summary>Gets or sets the edge color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public NetworkEdge() { }

  public NetworkEdge(string source, string target, double weight = 1, bool directed = false) {
    this.Source = source;
    this.Target = target;
    this.Weight = weight;
    this.Directed = directed;
  }
}

/// <summary>
/// Collection of network nodes.
/// </summary>
public class NetworkNodeCollection : List<NetworkNode> {
  private readonly AdvancedChart _owner;

  internal NetworkNodeCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(NetworkNode node) {
    base.Add(node);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of network edges.
/// </summary>
public class NetworkEdgeCollection : List<NetworkEdge> {
  private readonly AdvancedChart _owner;

  internal NetworkEdgeCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(NetworkEdge edge) {
    base.Add(edge);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Wraps network nodes and edges for network-based charts.
/// </summary>
public class NetworkData {
  /// <summary>Gets the network nodes.</summary>
  public NetworkNodeCollection Nodes { get; }

  /// <summary>Gets the network edges.</summary>
  public NetworkEdgeCollection Edges { get; }

  internal NetworkData(NetworkNodeCollection nodes, NetworkEdgeCollection edges) {
    this.Nodes = nodes;
    this.Edges = edges;
  }
}

#endregion

#region Temporal Data

/// <summary>
/// Represents a task in a Gantt chart.
/// </summary>
public class GanttTask {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the task name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the start date.</summary>
  public DateTime Start { get; set; }

  /// <summary>Gets or sets the end date.</summary>
  public DateTime End { get; set; }

  /// <summary>Gets or sets the completion progress (0-100).</summary>
  public double Progress { get; set; }

  /// <summary>Gets or sets task dependencies (IDs of tasks this depends on).</summary>
  public List<string> Dependencies { get; set; } = new();

  /// <summary>Gets or sets the resource/person assigned.</summary>
  public string Resource { get; set; }

  /// <summary>Gets or sets the task color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the group/phase this task belongs to.</summary>
  public string Group { get; set; }

  /// <summary>Gets or sets whether this is a milestone (zero duration).</summary>
  public bool IsMilestone { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets the duration of the task.</summary>
  public TimeSpan Duration => this.End - this.Start;

  public GanttTask() { }

  public GanttTask(string id, string name, DateTime start, DateTime end) {
    this.Id = id;
    this.Name = name;
    this.Start = start;
    this.End = end;
  }
}

/// <summary>
/// Represents an event on a timeline.
/// </summary>
public class TimelineEvent {
  /// <summary>Gets or sets the event date.</summary>
  public DateTime Date { get; set; }

  /// <summary>Gets or sets the event title.</summary>
  public string Title { get; set; }

  /// <summary>Gets or sets the event description.</summary>
  public string Description { get; set; }

  /// <summary>Gets or sets the event icon.</summary>
  public Image Icon { get; set; }

  /// <summary>Gets or sets the event color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the category/group.</summary>
  public string Category { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public TimelineEvent() { }

  public TimelineEvent(DateTime date, string title, string description = null) {
    this.Date = date;
    this.Title = title;
    this.Description = description;
  }
}

/// <summary>
/// Represents a day's data for a calendar heatmap.
/// </summary>
public class CalendarHeatmapDay {
  /// <summary>Gets or sets the date.</summary>
  public DateTime Date { get; set; }

  /// <summary>Gets or sets the value for this day.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the color override.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public CalendarHeatmapDay() { }

  public CalendarHeatmapDay(DateTime date, double value) {
    this.Date = date;
    this.Value = value;
  }
}

/// <summary>
/// Collection of Gantt tasks.
/// </summary>
public class GanttTaskCollection : List<GanttTask> {
  private readonly AdvancedChart _owner;

  internal GanttTaskCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(GanttTask task) {
    base.Add(task);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of timeline events.
/// </summary>
public class TimelineEventCollection : List<TimelineEvent> {
  private readonly AdvancedChart _owner;

  internal TimelineEventCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(TimelineEvent evt) {
    base.Add(evt);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of calendar heatmap days.
/// </summary>
public class CalendarHeatmapCollection : List<CalendarHeatmapDay> {
  private readonly AdvancedChart _owner;

  internal CalendarHeatmapCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(CalendarHeatmapDay day) {
    base.Add(day);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Statistical Data

/// <summary>
/// Represents a box plot data point with statistical summary.
/// </summary>
public class BoxPlotData {
  /// <summary>Gets or sets the category label.</summary>
  public string Category { get; set; }

  /// <summary>Gets or sets the minimum value.</summary>
  public double Minimum { get; set; }

  /// <summary>Gets or sets the first quartile (Q1, 25th percentile).</summary>
  public double Q1 { get; set; }

  /// <summary>Gets or sets the median (Q2, 50th percentile).</summary>
  public double Median { get; set; }

  /// <summary>Gets or sets the third quartile (Q3, 75th percentile).</summary>
  public double Q3 { get; set; }

  /// <summary>Gets or sets the maximum value.</summary>
  public double Maximum { get; set; }

  /// <summary>Gets or sets outlier values.</summary>
  public List<double> Outliers { get; set; } = new();

  /// <summary>Gets or sets the mean (optional).</summary>
  public double? Mean { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets the interquartile range (IQR).</summary>
  public double IQR => this.Q3 - this.Q1;

  public BoxPlotData() { }

  public BoxPlotData(string category, double min, double q1, double median, double q3, double max) {
    this.Category = category;
    this.Minimum = min;
    this.Q1 = q1;
    this.Median = median;
    this.Q3 = q3;
    this.Maximum = max;
  }

  /// <summary>
  /// Creates box plot data from a collection of values.
  /// </summary>
  public static BoxPlotData FromValues(string category, IEnumerable<double> values) {
    var sorted = new List<double>(values);
    sorted.Sort();

    if (sorted.Count == 0)
      return new BoxPlotData { Category = category };

    var result = new BoxPlotData {
      Category = category,
      Minimum = sorted[0],
      Maximum = sorted[sorted.Count - 1],
      Median = Percentile(sorted, 0.5),
      Q1 = Percentile(sorted, 0.25),
      Q3 = Percentile(sorted, 0.75),
      Mean = sorted.Count > 0 ? sorted.Average() : null
    };

    // Calculate outliers (values beyond 1.5 * IQR)
    var lowerFence = result.Q1 - 1.5 * result.IQR;
    var upperFence = result.Q3 + 1.5 * result.IQR;

    result.Minimum = sorted.FirstOrDefault(v => v >= lowerFence);
    result.Maximum = sorted.LastOrDefault(v => v <= upperFence);

    result.Outliers = sorted.Where(v => v < lowerFence || v > upperFence).ToList();

    return result;
  }

  private static double Percentile(List<double> sorted, double p) {
    if (sorted.Count == 0)
      return 0;
    if (sorted.Count == 1)
      return sorted[0];

    var n = (sorted.Count - 1) * p;
    var k = (int)n;
    var d = n - k;

    if (k + 1 < sorted.Count)
      return sorted[k] + d * (sorted[k + 1] - sorted[k]);

    return sorted[k];
  }

  private double Average() {
    double sum = 0;
    var count = 0;
    foreach (var o in this.Outliers) {
      sum += o;
      ++count;
    }
    return count > 0 ? sum / count : 0;
  }
}

/// <summary>
/// Represents histogram bin data.
/// </summary>
public class HistogramBin {
  /// <summary>Gets or sets the bin start value.</summary>
  public double Start { get; set; }

  /// <summary>Gets or sets the bin end value.</summary>
  public double End { get; set; }

  /// <summary>Gets or sets the count/frequency.</summary>
  public int Count { get; set; }

  /// <summary>Gets or sets the density (for normalized histograms).</summary>
  public double Density { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets the bin width.</summary>
  public double Width => this.End - this.Start;

  /// <summary>Gets the bin center.</summary>
  public double Center => (this.Start + this.End) / 2;

  public HistogramBin() { }

  public HistogramBin(double start, double end, int count) {
    this.Start = start;
    this.End = end;
    this.Count = count;
  }
}

/// <summary>
/// Represents a bubble/sized scatter point.
/// </summary>
public class BubblePoint : ChartPoint {
  /// <summary>Gets or sets the size/radius value.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets an optional Z value for 3D bubbles.</summary>
  public double? Z { get; set; }

  public BubblePoint() { }

  public BubblePoint(double x, double y, double size, string label = null) : base(x, y, label) {
    this.Size = size;
  }
}

/// <summary>
/// Represents a cell in a heatmap.
/// </summary>
public class HeatmapCell {
  /// <summary>Gets or sets the row index.</summary>
  public int Row { get; set; }

  /// <summary>Gets or sets the column index.</summary>
  public int Column { get; set; }

  /// <summary>Gets or sets the cell value.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the color override.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public HeatmapCell() { }

  public HeatmapCell(int row, int column, double value) {
    this.Row = row;
    this.Column = column;
    this.Value = value;
  }
}

/// <summary>
/// Collection of box plot data.
/// </summary>
public class BoxPlotDataCollection : List<BoxPlotData> {
  private readonly AdvancedChart _owner;

  internal BoxPlotDataCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(BoxPlotData data) {
    base.Add(data);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of histogram bins.
/// </summary>
public class HistogramBinCollection : List<HistogramBin> {
  private readonly AdvancedChart _owner;

  internal HistogramBinCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(HistogramBin bin) {
    base.Add(bin);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  /// <summary>
  /// Creates histogram bins from a collection of values.
  /// </summary>
  public static HistogramBinCollection FromValues(AdvancedChart owner, IEnumerable<double> values, int binCount = 10) {
    var result = new HistogramBinCollection(owner);
    var valueList = new List<double>(values);

    if (valueList.Count == 0)
      return result;

    var min = valueList.Min();
    var max = valueList.Max();
    var binWidth = (max - min) / binCount;

    if (binWidth <= 0)
      binWidth = 1;

    var bins = new int[binCount];
    foreach (var value in valueList) {
      var binIndex = (int)((value - min) / binWidth);
      if (binIndex >= binCount)
        binIndex = binCount - 1;
      if (binIndex < 0)
        binIndex = 0;
      bins[binIndex]++;
    }

    var totalCount = valueList.Count;
    for (var i = 0; i < binCount; ++i) {
      var binStart = min + i * binWidth;
      var binEnd = binStart + binWidth;
      result.Add(new HistogramBin {
        Start = binStart,
        End = binEnd,
        Count = bins[i],
        Density = bins[i] / (double)totalCount / binWidth
      });
    }

    return result;
  }
}

/// <summary>
/// Collection of heatmap cells.
/// </summary>
public class HeatmapCellCollection : List<HeatmapCell> {
  private readonly AdvancedChart _owner;

  internal HeatmapCellCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(HeatmapCell cell) {
    base.Add(cell);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Specialized Data

/// <summary>
/// Represents a funnel/pyramid stage.
/// </summary>
public class FunnelStage {
  /// <summary>Gets or sets the stage label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the value.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public FunnelStage() { }

  public FunnelStage(string label, double value) {
    this.Label = label;
    this.Value = value;
  }
}

/// <summary>
/// Represents a word for word cloud visualization.
/// </summary>
public class WordCloudWord {
  /// <summary>Gets or sets the word text.</summary>
  public string Text { get; set; }

  /// <summary>Gets or sets the word weight/frequency.</summary>
  public double Weight { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public WordCloudWord() { }

  public WordCloudWord(string text, double weight) {
    this.Text = text;
    this.Weight = weight;
  }
}

/// <summary>
/// Represents a gauge zone with color indication.
/// </summary>
public class GaugeZone {
  /// <summary>Gets or sets the zone start value.</summary>
  public double Start { get; set; }

  /// <summary>Gets or sets the zone end value.</summary>
  public double End { get; set; }

  /// <summary>Gets or sets the zone color.</summary>
  public Color Color { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  public GaugeZone() { }

  public GaugeZone(double start, double end, Color color, string label = null) {
    this.Start = start;
    this.End = end;
    this.Color = color;
    this.Label = label;
  }
}

/// <summary>
/// Represents a bullet chart data point.
/// </summary>
public class BulletData {
  /// <summary>Gets or sets the category label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the actual/current value.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets the target/comparison value.</summary>
  public double Target { get; set; }

  /// <summary>Gets or sets the range thresholds (e.g., poor, satisfactory, good).</summary>
  public double[] Ranges { get; set; }

  /// <summary>Gets or sets the colors for each range.</summary>
  public Color[] RangeColors { get; set; }

  /// <summary>Gets or sets the maximum value.</summary>
  public double Maximum { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public BulletData() { }

  public BulletData(string label, double value, double target, double maximum, params double[] ranges) {
    this.Label = label;
    this.Value = value;
    this.Target = target;
    this.Maximum = maximum;
    this.Ranges = ranges;
  }
}

/// <summary>
/// Collection of funnel stages.
/// </summary>
public class FunnelStageCollection : List<FunnelStage> {
  private readonly AdvancedChart _owner;

  internal FunnelStageCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(FunnelStage stage) {
    base.Add(stage);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of word cloud words.
/// </summary>
public class WordCloudCollection : List<WordCloudWord> {
  private readonly AdvancedChart _owner;

  internal WordCloudCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(WordCloudWord word) {
    base.Add(word);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of gauge zones.
/// </summary>
public class GaugeZoneCollection : List<GaugeZone> {
  private readonly AdvancedChart _owner;

  internal GaugeZoneCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(GaugeZone zone) {
    base.Add(zone);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Collection of bullet data.
/// </summary>
public class BulletDataCollection : List<BulletData> {
  private readonly AdvancedChart _owner;

  internal BulletDataCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(BulletData data) {
    base.Add(data);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Waterfall Data

/// <summary>
/// Represents a step in a waterfall chart.
/// </summary>
public class WaterfallStep {
  /// <summary>Gets or sets the step label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the step value (positive or negative).</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets whether this is a subtotal/total step.</summary>
  public bool IsTotal { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  /// <summary>Gets whether the value is positive.</summary>
  public bool IsPositive => this.Value >= 0;

  public WaterfallStep() { }

  public WaterfallStep(string label, double value, bool isTotal = false) {
    this.Label = label;
    this.Value = value;
    this.IsTotal = isTotal;
  }
}

/// <summary>
/// Collection of waterfall steps.
/// </summary>
public class WaterfallStepCollection : List<WaterfallStep> {
  private readonly AdvancedChart _owner;

  internal WaterfallStepCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(WaterfallStep step) {
    base.Add(step);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Range Data

/// <summary>
/// Represents a data point with a range (for error bars, range charts, etc.).
/// </summary>
public class RangePoint {
  /// <summary>Gets or sets the X value.</summary>
  public double X { get; set; }

  /// <summary>Gets or sets the low/minimum Y value.</summary>
  public double Low { get; set; }

  /// <summary>Gets or sets the high/maximum Y value.</summary>
  public double High { get; set; }

  /// <summary>Gets or sets the center/mid Y value.</summary>
  public double? Mid { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public RangePoint() { }

  public RangePoint(double x, double low, double high, double? mid = null) {
    this.X = x;
    this.Low = low;
    this.High = high;
    this.Mid = mid;
  }
}

/// <summary>
/// Collection of range points.
/// </summary>
public class RangePointCollection : List<RangePoint> {
  private readonly AdvancedChart _owner;

  internal RangePointCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(RangePoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Radar/Polar Data

/// <summary>
/// Represents a data point for radar/polar charts.
/// </summary>
public class RadarPoint {
  /// <summary>Gets or sets the axis/category label.</summary>
  public string Axis { get; set; }

  /// <summary>Gets or sets the value.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public RadarPoint() { }

  public RadarPoint(string axis, double value) {
    this.Axis = axis;
    this.Value = value;
  }
}

/// <summary>
/// Collection of radar points.
/// </summary>
public class RadarPointCollection : List<RadarPoint> {
  private readonly AdvancedChart _owner;

  internal RadarPointCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(RadarPoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion

#region Geospatial Data

/// <summary>
/// Represents a geographic map region for choropleth maps.
/// </summary>
public class MapRegion {
  /// <summary>Gets or sets the region identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the region name/label.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the data value for this region.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets the region bounds for drawing.</summary>
  public RectangleF? Bounds { get; set; }

  /// <summary>Gets or sets the color override.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public MapRegion() { }

  public MapRegion(string id, string name, double value, RectangleF? bounds = null) {
    this.Id = id;
    this.Name = name;
    this.Value = value;
    this.Bounds = bounds;
  }
}

/// <summary>
/// Collection of map regions for geospatial charts.
/// </summary>
public class MapRegionCollection : List<MapRegion> {
  private readonly AdvancedChart _owner;

  internal MapRegionCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(MapRegion region) {
    base.Add(region);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

/// <summary>
/// Represents a bubble data point for bubble maps.
/// </summary>
public class BubbleMapPoint {
  /// <summary>Gets or sets the X coordinate (0-100 normalized).</summary>
  public double X { get; set; }

  /// <summary>Gets or sets the Y coordinate (0-100 normalized).</summary>
  public double Y { get; set; }

  /// <summary>Gets or sets the bubble size.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets the bubble label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the bubble color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public BubbleMapPoint() { }

  public BubbleMapPoint(double x, double y, double size, string label = null) {
    this.X = x;
    this.Y = y;
    this.Size = size;
    this.Label = label;
  }
}

/// <summary>
/// Collection of bubble map points.
/// </summary>
public class BubbleMapPointCollection : List<BubbleMapPoint> {
  private readonly AdvancedChart _owner;

  internal BubbleMapPointCollection(AdvancedChart owner) => this._owner = owner;

  public new void Add(BubbleMapPoint point) {
    base.Add(point);
    this._owner?.Invalidate();
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }
}

#endregion
