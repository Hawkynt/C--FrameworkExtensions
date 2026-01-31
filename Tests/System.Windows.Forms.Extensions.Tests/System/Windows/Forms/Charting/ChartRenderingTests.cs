using System.Drawing;
using System.Windows.Forms.Charting;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ChartRenderingTests {
  private AdvancedChart _chart;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._chart = new AdvancedChart { Size = new Size(800, 600) };
    this._form = new Form();
    this._form.Controls.Add(this._chart);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._chart?.Dispose();
    this._form?.Dispose();
  }

  #region All Chart Types - Empty Data Rendering

  [Test]
  [Category("EdgeCase")]
  [TestCase(AdvancedChartType.Bar)]
  [TestCase(AdvancedChartType.Column)]
  [TestCase(AdvancedChartType.GroupedBar)]
  [TestCase(AdvancedChartType.GroupedColumn)]
  [TestCase(AdvancedChartType.StackedBar)]
  [TestCase(AdvancedChartType.StackedColumn)]
  [TestCase(AdvancedChartType.StackedBar100)]
  [TestCase(AdvancedChartType.StackedColumn100)]
  [TestCase(AdvancedChartType.DivergingStackedBar)]
  [TestCase(AdvancedChartType.Lollipop)]
  [TestCase(AdvancedChartType.DotPlot)]
  [TestCase(AdvancedChartType.Dumbbell)]
  [TestCase(AdvancedChartType.Bullet)]
  [TestCase(AdvancedChartType.Radar)]
  [TestCase(AdvancedChartType.PolarArea)]
  [TestCase(AdvancedChartType.Nightingale)]
  [TestCase(AdvancedChartType.RangePlot)]
  [TestCase(AdvancedChartType.SmallMultiples)]
  [TestCase(AdvancedChartType.Line)]
  [TestCase(AdvancedChartType.MultiLine)]
  [TestCase(AdvancedChartType.Spline)]
  [TestCase(AdvancedChartType.Area)]
  [TestCase(AdvancedChartType.StackedArea)]
  [TestCase(AdvancedChartType.StackedArea100)]
  [TestCase(AdvancedChartType.Step)]
  [TestCase(AdvancedChartType.StepArea)]
  [TestCase(AdvancedChartType.StreamGraph)]
  [TestCase(AdvancedChartType.Sparkline)]
  [TestCase(AdvancedChartType.RangeArea)]
  [TestCase(AdvancedChartType.BumpArea)]
  [TestCase(AdvancedChartType.Barcode)]
  [TestCase(AdvancedChartType.Pie)]
  [TestCase(AdvancedChartType.Donut)]
  [TestCase(AdvancedChartType.SemiCircleDonut)]
  [TestCase(AdvancedChartType.NestedDonut)]
  [TestCase(AdvancedChartType.Treemap)]
  [TestCase(AdvancedChartType.CircularTreemap)]
  [TestCase(AdvancedChartType.ConvexTreemap)]
  [TestCase(AdvancedChartType.Sunburst)]
  [TestCase(AdvancedChartType.Waffle)]
  [TestCase(AdvancedChartType.Icicle)]
  [TestCase(AdvancedChartType.Mosaic)]
  [TestCase(AdvancedChartType.Marimekko)]
  [TestCase(AdvancedChartType.Parliament)]
  [TestCase(AdvancedChartType.Unit)]
  [TestCase(AdvancedChartType.Histogram)]
  [TestCase(AdvancedChartType.RadialHistogram)]
  [TestCase(AdvancedChartType.BoxPlot)]
  [TestCase(AdvancedChartType.Violin)]
  [TestCase(AdvancedChartType.Density)]
  [TestCase(AdvancedChartType.Beeswarm)]
  [TestCase(AdvancedChartType.StripPlot)]
  [TestCase(AdvancedChartType.JitterPlot)]
  [TestCase(AdvancedChartType.Ridgeline)]
  [TestCase(AdvancedChartType.Horizon)]
  [TestCase(AdvancedChartType.Cumulative)]
  [TestCase(AdvancedChartType.PopulationPyramid)]
  [TestCase(AdvancedChartType.OneDimensionalHeatmap)]
  [TestCase(AdvancedChartType.Scatter)]
  [TestCase(AdvancedChartType.CategoricalScatter)]
  [TestCase(AdvancedChartType.Bubble)]
  [TestCase(AdvancedChartType.ConnectedScatter)]
  [TestCase(AdvancedChartType.Heatmap)]
  [TestCase(AdvancedChartType.Correlogram)]
  [TestCase(AdvancedChartType.ScatterMatrix)]
  [TestCase(AdvancedChartType.Hexbin)]
  [TestCase(AdvancedChartType.Contour)]
  [TestCase(AdvancedChartType.QuadrantChart)]
  [TestCase(AdvancedChartType.MatrixChart)]
  [TestCase(AdvancedChartType.OrderedBar)]
  [TestCase(AdvancedChartType.Slope)]
  [TestCase(AdvancedChartType.Bump)]
  [TestCase(AdvancedChartType.ParallelCoordinates)]
  [TestCase(AdvancedChartType.RadialBar)]
  [TestCase(AdvancedChartType.TableHeatmap)]
  [TestCase(AdvancedChartType.TableChart)]
  [TestCase(AdvancedChartType.Choropleth)]
  [TestCase(AdvancedChartType.GeographicHeatmap)]
  [TestCase(AdvancedChartType.TileMap)]
  [TestCase(AdvancedChartType.BubbleMap)]
  [TestCase(AdvancedChartType.ConnectionMap)]
  [TestCase(AdvancedChartType.DotMap)]
  [TestCase(AdvancedChartType.Timeline)]
  [TestCase(AdvancedChartType.Gantt)]
  [TestCase(AdvancedChartType.CalendarHeatmap)]
  [TestCase(AdvancedChartType.Seasonal)]
  [TestCase(AdvancedChartType.Spiral)]
  [TestCase(AdvancedChartType.Candlestick)]
  [TestCase(AdvancedChartType.OHLC)]
  [TestCase(AdvancedChartType.Kagi)]
  [TestCase(AdvancedChartType.Renko)]
  [TestCase(AdvancedChartType.Waterfall)]
  [TestCase(AdvancedChartType.PointFigure)]
  [TestCase(AdvancedChartType.Funnel)]
  [TestCase(AdvancedChartType.Pyramid)]
  [TestCase(AdvancedChartType.Gauge)]
  [TestCase(AdvancedChartType.CircularGauge)]
  [TestCase(AdvancedChartType.WordCloud)]
  [TestCase(AdvancedChartType.Pictogram)]
  [TestCase(AdvancedChartType.Venn)]
  [TestCase(AdvancedChartType.EulerDiagram)]
  [TestCase(AdvancedChartType.IconArray)]
  public void Render_WithEmptyData_DoesNotThrow(AdvancedChartType chartType) {
    this._chart.ChartType = chartType;

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
      Assert.That(bitmap.Width, Is.EqualTo(800));
      Assert.That(bitmap.Height, Is.EqualTo(600));
    });
  }

  #endregion

  #region All Chart Types - Single Series Rendering

  [Test]
  [Category("HappyPath")]
  [TestCase(AdvancedChartType.Bar)]
  [TestCase(AdvancedChartType.Column)]
  [TestCase(AdvancedChartType.GroupedBar)]
  [TestCase(AdvancedChartType.GroupedColumn)]
  [TestCase(AdvancedChartType.StackedBar)]
  [TestCase(AdvancedChartType.StackedColumn)]
  [TestCase(AdvancedChartType.StackedBar100)]
  [TestCase(AdvancedChartType.StackedColumn100)]
  [TestCase(AdvancedChartType.DivergingStackedBar)]
  [TestCase(AdvancedChartType.Lollipop)]
  [TestCase(AdvancedChartType.DotPlot)]
  [TestCase(AdvancedChartType.Dumbbell)]
  [TestCase(AdvancedChartType.Radar)]
  [TestCase(AdvancedChartType.PolarArea)]
  [TestCase(AdvancedChartType.Nightingale)]
  [TestCase(AdvancedChartType.RangePlot)]
  [TestCase(AdvancedChartType.SmallMultiples)]
  [TestCase(AdvancedChartType.Line)]
  [TestCase(AdvancedChartType.MultiLine)]
  [TestCase(AdvancedChartType.Spline)]
  [TestCase(AdvancedChartType.Area)]
  [TestCase(AdvancedChartType.StackedArea)]
  [TestCase(AdvancedChartType.StackedArea100)]
  [TestCase(AdvancedChartType.Step)]
  [TestCase(AdvancedChartType.StepArea)]
  [TestCase(AdvancedChartType.StreamGraph)]
  [TestCase(AdvancedChartType.Sparkline)]
  [TestCase(AdvancedChartType.RangeArea)]
  [TestCase(AdvancedChartType.BumpArea)]
  [TestCase(AdvancedChartType.Barcode)]
  [TestCase(AdvancedChartType.Pie)]
  [TestCase(AdvancedChartType.Donut)]
  [TestCase(AdvancedChartType.SemiCircleDonut)]
  [TestCase(AdvancedChartType.NestedDonut)]
  [TestCase(AdvancedChartType.Waffle)]
  [TestCase(AdvancedChartType.Mosaic)]
  [TestCase(AdvancedChartType.Marimekko)]
  [TestCase(AdvancedChartType.Parliament)]
  [TestCase(AdvancedChartType.Unit)]
  [TestCase(AdvancedChartType.Histogram)]
  [TestCase(AdvancedChartType.RadialHistogram)]
  [TestCase(AdvancedChartType.Density)]
  [TestCase(AdvancedChartType.Beeswarm)]
  [TestCase(AdvancedChartType.StripPlot)]
  [TestCase(AdvancedChartType.JitterPlot)]
  [TestCase(AdvancedChartType.Ridgeline)]
  [TestCase(AdvancedChartType.Horizon)]
  [TestCase(AdvancedChartType.Cumulative)]
  [TestCase(AdvancedChartType.PopulationPyramid)]
  [TestCase(AdvancedChartType.OneDimensionalHeatmap)]
  [TestCase(AdvancedChartType.Scatter)]
  [TestCase(AdvancedChartType.CategoricalScatter)]
  [TestCase(AdvancedChartType.Bubble)]
  [TestCase(AdvancedChartType.ConnectedScatter)]
  [TestCase(AdvancedChartType.Hexbin)]
  [TestCase(AdvancedChartType.Contour)]
  [TestCase(AdvancedChartType.QuadrantChart)]
  [TestCase(AdvancedChartType.OrderedBar)]
  [TestCase(AdvancedChartType.Slope)]
  [TestCase(AdvancedChartType.Bump)]
  [TestCase(AdvancedChartType.ParallelCoordinates)]
  [TestCase(AdvancedChartType.RadialBar)]
  [TestCase(AdvancedChartType.Seasonal)]
  [TestCase(AdvancedChartType.Spiral)]
  [TestCase(AdvancedChartType.Kagi)]
  [TestCase(AdvancedChartType.Renko)]
  [TestCase(AdvancedChartType.Waterfall)]
  [TestCase(AdvancedChartType.PointFigure)]
  [TestCase(AdvancedChartType.Pictogram)]
  [TestCase(AdvancedChartType.IconArray)]
  public void Render_SingleSeries_DoesNotThrow(AdvancedChartType chartType) {
    this._chart.ChartType = chartType;
    var series = this._chart.AddSeries("Test Series");
    for (var i = 0; i < 10; ++i)
      series.AddPoint(i, 10 + i * 5);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Multi-Series Charts

  [Test]
  [Category("HappyPath")]
  [TestCase(AdvancedChartType.GroupedBar)]
  [TestCase(AdvancedChartType.GroupedColumn)]
  [TestCase(AdvancedChartType.StackedBar)]
  [TestCase(AdvancedChartType.StackedColumn)]
  [TestCase(AdvancedChartType.StackedBar100)]
  [TestCase(AdvancedChartType.StackedColumn100)]
  [TestCase(AdvancedChartType.DivergingStackedBar)]
  [TestCase(AdvancedChartType.Radar)]
  [TestCase(AdvancedChartType.PolarArea)]
  [TestCase(AdvancedChartType.SmallMultiples)]
  [TestCase(AdvancedChartType.MultiLine)]
  [TestCase(AdvancedChartType.StackedArea)]
  [TestCase(AdvancedChartType.StackedArea100)]
  [TestCase(AdvancedChartType.StreamGraph)]
  [TestCase(AdvancedChartType.NestedDonut)]
  [TestCase(AdvancedChartType.Ridgeline)]
  [TestCase(AdvancedChartType.Bump)]
  [TestCase(AdvancedChartType.Slope)]
  [TestCase(AdvancedChartType.ParallelCoordinates)]
  [TestCase(AdvancedChartType.Spiral)]
  public void Render_MultipleSeries_DoesNotThrow(AdvancedChartType chartType) {
    this._chart.ChartType = chartType;

    for (var s = 0; s < 3; ++s) {
      var series = this._chart.AddSeries($"Series {s + 1}");
      for (var i = 0; i < 10; ++i)
        series.AddPoint(i, 10 + i * 5 + s * 10);
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Variable Data Point Counts

  [Test]
  [Category("HappyPath")]
  [TestCase(1)]
  [TestCase(10)]
  [TestCase(100)]
  [TestCase(1000)]
  public void Render_LineChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Line;
    var series = this._chart.AddSeries("Test");

    for (var i = 0; i < pointCount; ++i)
      series.AddPoint(i, Math.Sin(i * 0.1) * 50 + 50);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(1)]
  [TestCase(10)]
  [TestCase(100)]
  [TestCase(1000)]
  public void Render_BarChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Bar;
    var series = this._chart.AddSeries("Test");

    for (var i = 0; i < pointCount; ++i)
      series.AddPoint(i, 10 + i % 50);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(1)]
  [TestCase(10)]
  [TestCase(100)]
  [TestCase(1000)]
  public void Render_ScatterChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Scatter;
    var series = this._chart.AddSeries("Test");
    var rand = new Random(42);

    for (var i = 0; i < pointCount; ++i)
      series.AddPoint(rand.NextDouble() * 100, rand.NextDouble() * 100);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(1)]
  [TestCase(10)]
  [TestCase(100)]
  public void Render_PieChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Pie;
    var series = this._chart.AddSeries("Test");

    for (var i = 0; i < pointCount; ++i)
      series.AddPoint(i, 10 + i % 20, $"Slice {i}");

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(50)]
  [TestCase(100)]
  public void Render_WaterfallChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Waterfall;
    var series = this._chart.AddSeries("Waterfall");
    var rand = new Random(42);

    series.AddPoint(0, 100, "Start");
    for (var i = 1; i < pointCount - 1; ++i)
      series.AddPoint(i, rand.Next(-20, 30), $"Step {i}");

    var total = 100.0;
    foreach (var p in series.Points)
      if (p.Label != "Start")
        total += p.Y;
    series.AddPoint(pointCount - 1, total, "End");

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Specialized Data Collections

  [Test]
  [Category("HappyPath")]
  [TestCase(5)]
  [TestCase(20)]
  [TestCase(100)]
  public void Render_CandlestickChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Candlestick;
    var rand = new Random(42);
    var basePrice = 100.0;

    for (var i = 0; i < pointCount; ++i) {
      var change = (rand.NextDouble() - 0.5) * 10;
      var open = basePrice;
      var close = basePrice + change;
      var high = Math.Max(open, close) + rand.NextDouble() * 5;
      var low = Math.Min(open, close) - rand.NextDouble() * 5;

      this._chart.OHLCData.Add(new OHLCDataPoint {
        Date = DateTime.Today.AddDays(i),
        Open = open,
        High = high,
        Low = low,
        Close = close,
        Volume = rand.Next(1000, 10000)
      });

      basePrice = close;
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(20)]
  public void Render_BoxPlotChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.BoxPlot;
    var rand = new Random(42);

    for (var i = 0; i < pointCount; ++i) {
      var median = 50 + rand.Next(-20, 20);
      this._chart.BoxPlotData.Add(new BoxPlotData {
        Category = $"Group {i + 1}",
        Minimum = median - 30 - rand.Next(10),
        Q1 = median - 15 - rand.Next(5),
        Median = median,
        Q3 = median + 15 + rand.Next(5),
        Maximum = median + 30 + rand.Next(10),
        Mean = median + rand.Next(-5, 5)
      });
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(5)]
  [TestCase(10)]
  public void Render_FunnelChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Funnel;

    var value = 1000.0;
    for (var i = 0; i < pointCount; ++i) {
      this._chart.FunnelStages.Add(new FunnelStage {
        Label = $"Stage {i + 1}",
        Value = value
      });
      value *= 0.7;
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(5)]
  [TestCase(20)]
  [TestCase(50)]
  public void Render_WordCloud_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.WordCloud;
    var rand = new Random(42);

    for (var i = 0; i < pointCount; ++i)
      this._chart.WordCloudData.Add(new WordCloudWord {
        Text = $"Word{i}",
        Weight = rand.Next(10, 100)
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(30)]
  public void Render_GanttChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Gantt;
    var startDate = DateTime.Today;

    for (var i = 0; i < pointCount; ++i)
      this._chart.GanttTasks.Add(new GanttTask {
        Id = $"task{i}",
        Name = $"Task {i + 1}",
        Start = startDate.AddDays(i * 3),
        End = startDate.AddDays(i * 3 + 5),
        Progress = i * 10 % 100
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_TreemapChart_WithHierarchicalData() {
    this._chart.ChartType = AdvancedChartType.Treemap;

    this._chart.HierarchicalData.Add(new HierarchicalDataPoint {
      Id = "root",
      Label = "Root",
      Value = 100,
      Children = new System.Collections.Generic.List<HierarchicalDataPoint> {
        new() { Id = "a", Label = "A", Value = 40 },
        new() { Id = "b", Label = "B", Value = 30 },
        new() { Id = "c", Label = "C", Value = 30 }
      }
    });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_SunburstChart_WithHierarchicalData() {
    this._chart.ChartType = AdvancedChartType.Sunburst;

    this._chart.HierarchicalData.Add(new HierarchicalDataPoint {
      Id = "root",
      Label = "Root",
      Value = 100,
      Children = new System.Collections.Generic.List<HierarchicalDataPoint> {
        new() {
          Id = "a",
          Label = "A",
          Value = 50,
          Children = new System.Collections.Generic.List<HierarchicalDataPoint> {
            new() { Id = "a1", Label = "A1", Value = 25 },
            new() { Id = "a2", Label = "A2", Value = 25 }
          }
        },
        new() { Id = "b", Label = "B", Value = 50 }
      }
    });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(9)]
  [TestCase(25)]
  [TestCase(100)]
  public void Render_HeatmapChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Heatmap;
    var size = (int)Math.Sqrt(pointCount);
    var rand = new Random(42);

    for (var row = 0; row < size; ++row)
    for (var col = 0; col < size; ++col)
      this._chart.HeatmapData.Add(new HeatmapCell(row, col, rand.NextDouble() * 100));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_GaugeChart_WithZones() {
    this._chart.ChartType = AdvancedChartType.Gauge;

    this._chart.GaugeZones.Add(new Charting.GaugeZone { Start = 0, End = 30, Color = Color.Green });
    this._chart.GaugeZones.Add(new Charting.GaugeZone { Start = 30, End = 70, Color = Color.Yellow });
    this._chart.GaugeZones.Add(new Charting.GaugeZone { Start = 70, End = 100, Color = Color.Red });

    var series = this._chart.AddSeries("Value");
    series.AddPoint(0, 65);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(5)]
  [TestCase(20)]
  [TestCase(50)]
  public void Render_BulletChart_VariousDataPointCounts(int pointCount) {
    this._chart.ChartType = AdvancedChartType.Bullet;
    var rand = new Random(42);

    for (var i = 0; i < pointCount; ++i)
      this._chart.BulletData.Add(new BulletData {
        Label = $"Metric {i + 1}",
        Value = rand.Next(50, 90),
        Target = 80,
        Maximum = 100,
        Ranges = new double[] { 30, 60, 90 }
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_ConnectionMap_WithNetworkData() {
    this._chart.ChartType = AdvancedChartType.ConnectionMap;

    this._chart.NetworkNodes.Add(new NetworkNode { Id = "A", Label = "Node A", Position = new PointF(100, 100) });
    this._chart.NetworkNodes.Add(new NetworkNode { Id = "B", Label = "Node B", Position = new PointF(300, 100) });
    this._chart.NetworkNodes.Add(new NetworkNode { Id = "C", Label = "Node C", Position = new PointF(200, 250) });

    this._chart.NetworkEdges.Add(new NetworkEdge { Source = "A", Target = "B", Weight = 1 });
    this._chart.NetworkEdges.Add(new NetworkEdge { Source = "B", Target = "C", Weight = 2 });
    this._chart.NetworkEdges.Add(new NetworkEdge { Source = "C", Target = "A", Weight = 1 });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Render_WithNegativeValues_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Bar;
    var series = this._chart.AddSeries("Test");
    series.AddPoint(0, -50);
    series.AddPoint(1, -25);
    series.AddPoint(2, 0);
    series.AddPoint(3, 25);
    series.AddPoint(4, 50);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithVeryLargeValues_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Line;
    var series = this._chart.AddSeries("Test");
    series.AddPoint(0, 1e12);
    series.AddPoint(1, 2e12);
    series.AddPoint(2, 1.5e12);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithVerySmallValues_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Line;
    var series = this._chart.AddSeries("Test");
    series.AddPoint(0, 1e-12);
    series.AddPoint(1, 2e-12);
    series.AddPoint(2, 1.5e-12);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithIdenticalValues_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Line;
    var series = this._chart.AddSeries("Test");
    for (var i = 0; i < 10; ++i)
      series.AddPoint(i, 50);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithSingleDataPoint_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Line;
    var series = this._chart.AddSeries("Test");
    series.AddPoint(0, 50);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithAnimation_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Bar;
    this._chart.EnableAnimation = true;
    var series = this._chart.AddSeries("Test");
    for (var i = 0; i < 5; ++i)
      series.AddPoint(i, 10 + i * 10);

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithDataLabels_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Column;
    this._chart.ShowDataLabels = true;
    var series = this._chart.AddSeries("Test");
    for (var i = 0; i < 5; ++i)
      series.AddPoint(i, 10 + i * 10, $"Label {i}");

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithLegend_DoesNotThrow() {
    this._chart.ChartType = AdvancedChartType.Line;
    this._chart.Legend.Visible = true;
    this._chart.Legend.Position = ChartLegendPosition.Right;

    for (var s = 0; s < 3; ++s) {
      var series = this._chart.AddSeries($"Series {s + 1}");
      for (var i = 0; i < 10; ++i)
        series.AddPoint(i, 10 + i * 5 + s * 10);
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._chart.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion
}
