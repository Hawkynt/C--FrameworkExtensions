using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class SimpleChartTests {
  private SimpleChart _chart;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._chart = new SimpleChart();
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
    Assert.That(this._chart.ChartType, Is.EqualTo(ChartType.Line));
    Assert.That(this._chart.ShowLegend, Is.True);
    Assert.That(this._chart.LegendPosition, Is.EqualTo(LegendPosition.Right));
    Assert.That(this._chart.ShowGrid, Is.True);
    Assert.That(this._chart.AutoScale, Is.True);
    Assert.That(this._chart.ShowDataLabels, Is.False);
    Assert.That(this._chart.EnableTooltips, Is.True);
    Assert.That(this._chart.XAxisMin, Is.EqualTo(0));
    Assert.That(this._chart.XAxisMax, Is.EqualTo(100));
    Assert.That(this._chart.YAxisMin, Is.EqualTo(0));
    Assert.That(this._chart.YAxisMax, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ChartType_CanBeSetAndRetrieved() {
    this._chart.ChartType = ChartType.Bar;
    Assert.That(this._chart.ChartType, Is.EqualTo(ChartType.Bar));

    this._chart.ChartType = ChartType.Pie;
    Assert.That(this._chart.ChartType, Is.EqualTo(ChartType.Pie));
  }

  [Test]
  [Category("HappyPath")]
  public void Title_CanBeSetAndRetrieved() {
    this._chart.Title = "Sales Chart";
    Assert.That(this._chart.Title, Is.EqualTo("Sales Chart"));
  }

  [Test]
  [Category("HappyPath")]
  public void AxisTitles_CanBeSetAndRetrieved() {
    this._chart.XAxisTitle = "Month";
    this._chart.YAxisTitle = "Revenue ($)";

    Assert.That(this._chart.XAxisTitle, Is.EqualTo("Month"));
    Assert.That(this._chart.YAxisTitle, Is.EqualTo("Revenue ($)"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSeries_CreatesSeries() {
    var series = this._chart.AddSeries("Series 1");

    Assert.That(this._chart.Series.Count, Is.EqualTo(1));
    Assert.That(series.Name, Is.EqualTo("Series 1"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSeries_WithChartType_SetsType() {
    var series = this._chart.AddSeries("Line Series", ChartType.Line);
    var barSeries = this._chart.AddSeries("Bar Series", ChartType.Bar);

    Assert.That(series.ChartType, Is.EqualTo(ChartType.Line));
    Assert.That(barSeries.ChartType, Is.EqualTo(ChartType.Bar));
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

    Assert.That(series.DataPoints.Count, Is.EqualTo(2));
    Assert.That(series.DataPoints[0].X, Is.EqualTo(1));
    Assert.That(series.DataPoints[0].Y, Is.EqualTo(100));
    Assert.That(series.DataPoints[1].X, Is.EqualTo(2));
    Assert.That(series.DataPoints[1].Y, Is.EqualTo(150));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_AddPoint_WithLabel_SetsLabel() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 100, "January");

    Assert.That(series.DataPoints[0].Label, Is.EqualTo("January"));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_AddPoints_AddsMultiplePoints() {
    var series = this._chart.AddSeries("Test");
    series.AddPoints(new[] { (1.0, 10.0), (2.0, 20.0), (3.0, 30.0) });

    Assert.That(series.DataPoints.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_Clear_RemovesAllPoints() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 100);
    series.AddPoint(2, 200);
    series.Clear();

    Assert.That(series.DataPoints.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Chart_Clear_RemovesAllSeries() {
    this._chart.AddSeries("Series 1");
    this._chart.AddSeries("Series 2");
    this._chart.Clear();

    Assert.That(this._chart.Series.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_Properties_CanBeModified() {
    var series = this._chart.AddSeries("Test");

    series.Color = Color.Red;
    series.LineWidth = 3;
    series.ShowMarkers = false;
    series.MarkerSize = 10;
    series.MarkerStyle = ChartMarkerStyle.Square;
    series.Visible = false;

    Assert.That(series.Color, Is.EqualTo(Color.Red));
    Assert.That(series.LineWidth, Is.EqualTo(3));
    Assert.That(series.ShowMarkers, Is.False);
    Assert.That(series.MarkerSize, Is.EqualTo(10));
    Assert.That(series.MarkerStyle, Is.EqualTo(ChartMarkerStyle.Square));
    Assert.That(series.Visible, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowLegend_CanBeSetAndRetrieved() {
    this._chart.ShowLegend = false;
    Assert.That(this._chart.ShowLegend, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void LegendPosition_CanBeSetAndRetrieved() {
    this._chart.LegendPosition = LegendPosition.Bottom;
    Assert.That(this._chart.LegendPosition, Is.EqualTo(LegendPosition.Bottom));

    this._chart.LegendPosition = LegendPosition.None;
    Assert.That(this._chart.LegendPosition, Is.EqualTo(LegendPosition.None));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowGrid_CanBeSetAndRetrieved() {
    this._chart.ShowGrid = false;
    Assert.That(this._chart.ShowGrid, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void GridColor_CanBeSetAndRetrieved() {
    this._chart.GridColor = Color.DarkGray;
    Assert.That(this._chart.GridColor, Is.EqualTo(Color.DarkGray));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoScale_CanBeSetAndRetrieved() {
    this._chart.AutoScale = false;
    Assert.That(this._chart.AutoScale, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AxisBounds_CanBeSetAndRetrieved() {
    this._chart.XAxisMin = -10;
    this._chart.XAxisMax = 200;
    this._chart.YAxisMin = -50;
    this._chart.YAxisMax = 500;

    Assert.That(this._chart.XAxisMin, Is.EqualTo(-10));
    Assert.That(this._chart.XAxisMax, Is.EqualTo(200));
    Assert.That(this._chart.YAxisMin, Is.EqualTo(-50));
    Assert.That(this._chart.YAxisMax, Is.EqualTo(500));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowDataLabels_CanBeSetAndRetrieved() {
    this._chart.ShowDataLabels = true;
    Assert.That(this._chart.ShowDataLabels, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableTooltips_CanBeSetAndRetrieved() {
    this._chart.EnableTooltips = false;
    Assert.That(this._chart.EnableTooltips, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ChartDataPoint_Properties_Work() {
    var point = new ChartDataPoint {
      X = 5.5,
      Y = 10.5,
      Label = "Test Point",
      Color = Color.Green,
      Tag = "custom data"
    };

    Assert.That(point.X, Is.EqualTo(5.5));
    Assert.That(point.Y, Is.EqualTo(10.5));
    Assert.That(point.Label, Is.EqualTo("Test Point"));
    Assert.That(point.Color, Is.EqualTo(Color.Green));
    Assert.That(point.Tag, Is.EqualTo("custom data"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToImage_ReturnsValidBitmap() {
    var series = this._chart.AddSeries("Test");
    series.AddPoint(1, 10);
    series.AddPoint(2, 20);

    using var bitmap = this._chart.ToImage();

    Assert.That(bitmap, Is.Not.Null);
    Assert.That(bitmap.Width, Is.EqualTo(this._chart.Width));
    Assert.That(bitmap.Height, Is.EqualTo(this._chart.Height));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._chart.Width, Is.EqualTo(400));
    Assert.That(this._chart.Height, Is.EqualTo(300));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_DefaultValues_AreCorrect() {
    var series = this._chart.AddSeries("Test");

    Assert.That(series.LineWidth, Is.EqualTo(2));
    Assert.That(series.ShowMarkers, Is.True);
    Assert.That(series.MarkerSize, Is.EqualTo(6));
    Assert.That(series.MarkerStyle, Is.EqualTo(ChartMarkerStyle.Circle));
    Assert.That(series.Visible, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Series_LineWidth_HasMinimumValue() {
    var series = this._chart.AddSeries("Test");
    series.LineWidth = 0;

    Assert.That(series.LineWidth, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Series_MarkerSize_HasMinimumValue() {
    var series = this._chart.AddSeries("Test");
    series.MarkerSize = 0;

    Assert.That(series.MarkerSize, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AllChartTypes_Supported() {
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Line));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Bar));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Column));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Area));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Pie));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Donut));
    Assert.That(Enum.GetValues(typeof(ChartType)), Has.Member(ChartType.Scatter));
  }

  [Test]
  [Category("HappyPath")]
  public void AllLegendPositions_Supported() {
    Assert.That(Enum.GetValues(typeof(LegendPosition)), Has.Member(LegendPosition.Top));
    Assert.That(Enum.GetValues(typeof(LegendPosition)), Has.Member(LegendPosition.Bottom));
    Assert.That(Enum.GetValues(typeof(LegendPosition)), Has.Member(LegendPosition.Left));
    Assert.That(Enum.GetValues(typeof(LegendPosition)), Has.Member(LegendPosition.Right));
    Assert.That(Enum.GetValues(typeof(LegendPosition)), Has.Member(LegendPosition.None));
  }

  [Test]
  [Category("HappyPath")]
  public void AllMarkerStyles_Supported() {
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.None));
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.Circle));
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.Square));
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.Diamond));
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.Triangle));
    Assert.That(Enum.GetValues(typeof(ChartMarkerStyle)), Has.Member(ChartMarkerStyle.Cross));
  }

  [Test]
  [Category("HappyPath")]
  public void Series_ChartType_CanOverrideDefault() {
    this._chart.ChartType = ChartType.Line;
    var barSeries = this._chart.AddSeries("Bar", ChartType.Bar);
    var lineSeries = this._chart.AddSeries("Line");

    Assert.That(barSeries.ChartType, Is.EqualTo(ChartType.Bar));
    Assert.That(lineSeries.ChartType, Is.EqualTo(ChartType.Line));
  }

  [Test]
  [Category("HappyPath")]
  public void DataPointClicked_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.DataPointClicked += (s, e) => eventRaised = true;

    // Event subscription should work without throwing
    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DataPointHovered_EventCanBeSubscribed() {
    var eventRaised = false;
    this._chart.DataPointHovered += (s, e) => eventRaised = true;

    // Event subscription should work without throwing
    Assert.That(eventRaised, Is.False);
  }
}
