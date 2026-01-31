using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.Charting;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class AdvancedChartTests {
  private AdvancedChart _chart;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._chart = new AdvancedChart();
    this._form = new Form();
    this._form.Controls.Add(this._chart);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._chart?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._chart.ChartType, Is.EqualTo(AdvancedChartType.Line));
    Assert.That(this._chart.EnableTooltips, Is.True);
    Assert.That(this._chart.EnableAnimation, Is.False);
    Assert.That(this._chart.EnableZoom, Is.False);
    Assert.That(this._chart.EnablePan, Is.False);
    Assert.That(this._chart.EnableCrosshair, Is.False);
    Assert.That(this._chart.SelectionMode, Is.EqualTo(ChartSelectionMode.None));
  }

  [Test]
  [Category("HappyPath")]
  public void ChartType_CanBeSetAndRetrieved() {
    this._chart.ChartType = AdvancedChartType.Bar;
    Assert.That(this._chart.ChartType, Is.EqualTo(AdvancedChartType.Bar));

    this._chart.ChartType = AdvancedChartType.Pie;
    Assert.That(this._chart.ChartType, Is.EqualTo(AdvancedChartType.Pie));

    this._chart.ChartType = AdvancedChartType.Scatter;
    Assert.That(this._chart.ChartType, Is.EqualTo(AdvancedChartType.Scatter));
  }

  [Test]
  [Category("HappyPath")]
  public void Title_CanBeSetAndRetrieved() {
    this._chart.Title = "Advanced Chart Title";
    Assert.That(this._chart.Title, Is.EqualTo("Advanced Chart Title"));
  }

  [Test]
  [Category("HappyPath")]
  public void Subtitle_CanBeSetAndRetrieved() {
    this._chart.Subtitle = "Chart Subtitle";
    Assert.That(this._chart.Subtitle, Is.EqualTo("Chart Subtitle"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSeries_CreatesSeries() {
    var series = this._chart.AddSeries("Test Series");

    Assert.That(this._chart.Series.Count, Is.EqualTo(1));
    Assert.That(series.Name, Is.EqualTo("Test Series"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSeries_WithChartType_SetsChartTypeOverride() {
    var lineSeries = this._chart.AddSeries("Line Series", AdvancedChartType.Line);
    var barSeries = this._chart.AddSeries("Bar Series", AdvancedChartType.Bar);

    Assert.That(lineSeries.ChartTypeOverride, Is.EqualTo(AdvancedChartType.Line));
    Assert.That(barSeries.ChartTypeOverride, Is.EqualTo(AdvancedChartType.Bar));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSeries_AssignsUniqueColors() {
    var series1 = this._chart.AddSeries("Series 1");
    var series2 = this._chart.AddSeries("Series 2");

    Assert.That(series1.Color, Is.Not.EqualTo(series2.Color));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_AddPoint_AddsDataPoint() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 100);
    series.AddPoint(2, 150);

    Assert.That(series.Points.Count, Is.EqualTo(2));
    Assert.That(series.Points[0].X, Is.EqualTo(1));
    Assert.That(series.Points[0].Y, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_Clear_RemovesAllPoints() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 100);
    series.AddPoint(2, 200);
    series.Clear();

    Assert.That(series.Points.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllSeries() {
    this._chart.AddSeries("Series 1");
    this._chart.AddSeries("Series 2");
    this._chart.Clear();

    Assert.That(this._chart.Series.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void XAxis_Properties_CanBeModified() {
    this._chart.XAxis.Title = "X Axis Title";
    this._chart.XAxis.Visible = true;
    this._chart.XAxis.ShowGrid = true;

    Assert.That(this._chart.XAxis.Title, Is.EqualTo("X Axis Title"));
    Assert.That(this._chart.XAxis.Visible, Is.True);
    Assert.That(this._chart.XAxis.ShowGrid, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void YAxis_Properties_CanBeModified() {
    this._chart.YAxis.Title = "Y Axis Title";
    this._chart.YAxis.Minimum = 0;
    this._chart.YAxis.Maximum = 100;

    Assert.That(this._chart.YAxis.Title, Is.EqualTo("Y Axis Title"));
    Assert.That(this._chart.YAxis.Minimum, Is.EqualTo(0));
    Assert.That(this._chart.YAxis.Maximum, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Legend_IsAccessible() {
    Assert.That(this._chart.Legend, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableAnimation_CanBeSetAndRetrieved() {
    this._chart.EnableAnimation = true;
    Assert.That(this._chart.EnableAnimation, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableZoom_CanBeSetAndRetrieved() {
    this._chart.EnableZoom = true;
    Assert.That(this._chart.EnableZoom, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnablePan_CanBeSetAndRetrieved() {
    this._chart.EnablePan = true;
    Assert.That(this._chart.EnablePan, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableCrosshair_CanBeSetAndRetrieved() {
    this._chart.EnableCrosshair = true;
    Assert.That(this._chart.EnableCrosshair, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableTooltips_CanBeSetAndRetrieved() {
    this._chart.EnableTooltips = false;
    Assert.That(this._chart.EnableTooltips, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectionMode_CanBeSetAndRetrieved() {
    this._chart.SelectionMode = ChartSelectionMode.Single;
    Assert.That(this._chart.SelectionMode, Is.EqualTo(ChartSelectionMode.Single));

    this._chart.SelectionMode = ChartSelectionMode.Multiple;
    Assert.That(this._chart.SelectionMode, Is.EqualTo(ChartSelectionMode.Multiple));
  }

  [Test]
  [Category("HappyPath")]
  public void ToImage_ReturnsValidBitmap() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 10);
    series.AddPoint(2, 20);

    using var bitmap = this._chart.ToImage();

    Assert.That(bitmap, Is.Not.Null);
    Assert.That(bitmap.Width, Is.GreaterThan(0));
    Assert.That(bitmap.Height, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ToImage_WithCustomDimensions_ReturnsCorrectSize() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 10);

    using var bitmap = this._chart.ToImage(800, 600);

    Assert.That(bitmap.Width, Is.EqualTo(800));
    Assert.That(bitmap.Height, Is.EqualTo(600));
  }

  [Test]
  [Category("HappyPath")]
  public void DataPointClicked_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.DataPointClicked += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DataPointHovered_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.DataPointHovered += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectionChanged_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.SelectionChanged += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ZoomChanged_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.ZoomChanged += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ComparisonChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Bar));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Column));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.GroupedBar));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.StackedBar));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Lollipop));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.DotPlot));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Bullet));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Radar));
  }

  [Test]
  [Category("HappyPath")]
  public void TrendChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Line));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Spline));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Area));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.StackedArea));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Step));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Sparkline));
  }

  [Test]
  [Category("HappyPath")]
  public void PartToWholeChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Pie));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Donut));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Treemap));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Sunburst));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Waffle));
  }

  [Test]
  [Category("HappyPath")]
  public void DistributionChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Histogram));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.BoxPlot));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Violin));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.StripPlot));
  }

  [Test]
  [Category("HappyPath")]
  public void CorrelationChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Scatter));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Bubble));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Heatmap));
  }

  [Test]
  [Category("HappyPath")]
  public void GeospatialChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Choropleth));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.GeographicHeatmap));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.TileMap));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.BubbleMap));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.ConnectionMap));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.DotMap));
  }

  [Test]
  [Category("HappyPath")]
  public void FinancialChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Candlestick));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.OHLC));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Waterfall));
  }

  [Test]
  [Category("HappyPath")]
  public void TemporalChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Gantt));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Timeline));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.CalendarHeatmap));
  }

  [Test]
  [Category("HappyPath")]
  public void SpecializedChartTypes_AreSupported() {
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Funnel));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Pyramid));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.Gauge));
    Assert.That(Enum.GetValues(typeof(AdvancedChartType)), Has.Member(AdvancedChartType.WordCloud));
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithEmptyData_DoesNotThrow() {
    Assert.DoesNotThrow(() => {
      this._chart.Refresh();
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithNegativeValues_DoesNotThrow() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(-10, -20);
    series.AddPoint(0, 0);
    series.AddPoint(10, 20);

    Assert.DoesNotThrow(() => {
      this._chart.Refresh();
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithSingleDataPoint_DoesNotThrow() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 100);

    Assert.DoesNotThrow(() => {
      this._chart.Refresh();
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void SetChartType_WithDifferentTypes_UpdatesCorrectly() {
    foreach (AdvancedChartType chartType in Enum.GetValues(typeof(AdvancedChartType))) {
      this._chart.ChartType = chartType;
      Assert.That(this._chart.ChartType, Is.EqualTo(chartType));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void SpecializedDataCollections_AreAccessible() {
    Assert.That(this._chart.OHLCData, Is.Not.Null);
    Assert.That(this._chart.BoxPlotData, Is.Not.Null);
    Assert.That(this._chart.HierarchicalData, Is.Not.Null);
    Assert.That(this._chart.NetworkNodes, Is.Not.Null);
    Assert.That(this._chart.NetworkEdges, Is.Not.Null);
    Assert.That(this._chart.GanttTasks, Is.Not.Null);
    Assert.That(this._chart.TimelineEvents, Is.Not.Null);
    Assert.That(this._chart.CalendarHeatmapData, Is.Not.Null);
    Assert.That(this._chart.FunnelStages, Is.Not.Null);
    Assert.That(this._chart.GaugeZones, Is.Not.Null);
    Assert.That(this._chart.WordCloudData, Is.Not.Null);
    Assert.That(this._chart.HeatmapData, Is.Not.Null);
    Assert.That(this._chart.WaterfallData, Is.Not.Null);
    Assert.That(this._chart.HistogramData, Is.Not.Null);
    Assert.That(this._chart.BulletData, Is.Not.Null);
    Assert.That(this._chart.RangeData, Is.Not.Null);
    Assert.That(this._chart.RadarData, Is.Not.Null);
    Assert.That(this._chart.MapData, Is.Not.Null);
    Assert.That(this._chart.BubbleData, Is.Not.Null);
  }
}
