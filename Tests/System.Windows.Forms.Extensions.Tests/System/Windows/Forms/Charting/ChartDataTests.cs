using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.Charting;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ChartDataTests {
  [Test]
  [Category("HappyPath")]
  public void ChartPoint_DefaultValues_AreCorrect() {
    var point = new ChartPoint();

    Assert.That(point.X, Is.EqualTo(0));
    Assert.That(point.Y, Is.EqualTo(0));
    Assert.That(point.Label, Is.Null);
    Assert.That(point.Color, Is.Null);
    Assert.That(point.Tag, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void ChartPoint_Properties_CanBeSet() {
    var point = new ChartPoint {
      X = 5.5,
      Y = 10.5,
      Label = "Test Point",
      Color = Color.Red,
      Tag = "custom data"
    };

    Assert.That(point.X, Is.EqualTo(5.5));
    Assert.That(point.Y, Is.EqualTo(10.5));
    Assert.That(point.Label, Is.EqualTo("Test Point"));
    Assert.That(point.Color, Is.EqualTo(Color.Red));
    Assert.That(point.Tag, Is.EqualTo("custom data"));
  }

  [Test]
  [Category("HappyPath")]
  public void ChartPoint_ConstructorWithParameters_SetsValues() {
    var point = new ChartPoint(1.5, 2.5, "Label");

    Assert.That(point.X, Is.EqualTo(1.5));
    Assert.That(point.Y, Is.EqualTo(2.5));
    Assert.That(point.Label, Is.EqualTo("Label"));
  }

  [Test]
  [Category("HappyPath")]
  public void OHLCDataPoint_Properties_CanBeSet() {
    var ohlc = new OHLCDataPoint {
      Date = new DateTime(2024, 1, 15),
      Open = 100,
      High = 110,
      Low = 95,
      Close = 105,
      Volume = 1000000
    };

    Assert.That(ohlc.Date, Is.EqualTo(new DateTime(2024, 1, 15)));
    Assert.That(ohlc.Open, Is.EqualTo(100));
    Assert.That(ohlc.High, Is.EqualTo(110));
    Assert.That(ohlc.Low, Is.EqualTo(95));
    Assert.That(ohlc.Close, Is.EqualTo(105));
    Assert.That(ohlc.Volume, Is.EqualTo(1000000));
  }

  [Test]
  [Category("HappyPath")]
  public void OHLCDataPoint_IsBullish_CalculatesCorrectly() {
    var bullish = new OHLCDataPoint { Open = 100, Close = 110 };
    var bearish = new OHLCDataPoint { Open = 110, Close = 100 };

    Assert.That(bullish.IsBullish, Is.True);
    Assert.That(bearish.IsBullish, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void GanttTask_Properties_CanBeSet() {
    var task = new GanttTask {
      Id = "task1",
      Name = "Design Phase",
      Start = new DateTime(2024, 1, 1),
      End = new DateTime(2024, 1, 31),
      Progress = 50,
      Resource = "Team A",
      Color = Color.Blue
    };

    Assert.That(task.Id, Is.EqualTo("task1"));
    Assert.That(task.Name, Is.EqualTo("Design Phase"));
    Assert.That(task.Start, Is.EqualTo(new DateTime(2024, 1, 1)));
    Assert.That(task.End, Is.EqualTo(new DateTime(2024, 1, 31)));
    Assert.That(task.Progress, Is.EqualTo(50));
    Assert.That(task.Resource, Is.EqualTo("Team A"));
    Assert.That(task.Color, Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void GanttTask_Duration_CalculatesCorrectly() {
    var task = new GanttTask {
      Start = new DateTime(2024, 1, 1),
      End = new DateTime(2024, 1, 8)
    };

    Assert.That(task.Duration, Is.EqualTo(TimeSpan.FromDays(7)));
  }

  [Test]
  [Category("HappyPath")]
  public void GanttTask_Dependencies_CanBeSet() {
    var task = new GanttTask {
      Id = "task2",
      Dependencies = new List<string> { "task1" }
    };

    Assert.That(task.Dependencies.Count, Is.EqualTo(1));
    Assert.That(task.Dependencies[0], Is.EqualTo("task1"));
  }

  [Test]
  [Category("HappyPath")]
  public void NetworkNode_Properties_CanBeSet() {
    var node = new NetworkNode {
      Id = "node1",
      Label = "Server",
      Size = 20,
      Color = Color.Green,
      Position = new PointF(100, 200)
    };

    Assert.That(node.Id, Is.EqualTo("node1"));
    Assert.That(node.Label, Is.EqualTo("Server"));
    Assert.That(node.Size, Is.EqualTo(20));
    Assert.That(node.Color, Is.EqualTo(Color.Green));
    Assert.That(node.Position.X, Is.EqualTo(100));
    Assert.That(node.Position.Y, Is.EqualTo(200));
  }

  [Test]
  [Category("HappyPath")]
  public void NetworkEdge_Properties_CanBeSet() {
    var edge = new NetworkEdge {
      Source = "node1",
      Target = "node2",
      Weight = 5,
      Directed = true
    };

    Assert.That(edge.Source, Is.EqualTo("node1"));
    Assert.That(edge.Target, Is.EqualTo("node2"));
    Assert.That(edge.Weight, Is.EqualTo(5));
    Assert.That(edge.Directed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HierarchicalDataPoint_Properties_CanBeSet() {
    var data = new HierarchicalDataPoint {
      Id = "root",
      Label = "Root Node",
      ParentId = null,
      Value = 1000,
      Color = Color.Orange
    };

    Assert.That(data.Id, Is.EqualTo("root"));
    Assert.That(data.Label, Is.EqualTo("Root Node"));
    Assert.That(data.ParentId, Is.Null);
    Assert.That(data.Value, Is.EqualTo(1000));
    Assert.That(data.Color, Is.EqualTo(Color.Orange));
  }

  [Test]
  [Category("HappyPath")]
  public void HierarchicalDataPoint_Children_CanBeSet() {
    var child1 = new HierarchicalDataPoint { Id = "child1", Label = "Child 1", Value = 300 };
    var child2 = new HierarchicalDataPoint { Id = "child2", Label = "Child 2", Value = 200 };

    var parent = new HierarchicalDataPoint {
      Id = "parent",
      Label = "Parent",
      Value = 500,
      Children = new List<HierarchicalDataPoint> { child1, child2 }
    };

    Assert.That(parent.Children.Count, Is.EqualTo(2));
    Assert.That(parent.Children[0].Id, Is.EqualTo("child1"));
    Assert.That(parent.Children[1].Id, Is.EqualTo("child2"));
  }

  [Test]
  [Category("HappyPath")]
  public void BoxPlotData_Properties_CanBeSet() {
    var boxPlot = new BoxPlotData {
      Category = "Group A",
      Minimum = 10,
      Q1 = 25,
      Median = 50,
      Q3 = 75,
      Maximum = 90,
      Mean = 48,
      Color = Color.Purple
    };

    Assert.That(boxPlot.Category, Is.EqualTo("Group A"));
    Assert.That(boxPlot.Minimum, Is.EqualTo(10));
    Assert.That(boxPlot.Q1, Is.EqualTo(25));
    Assert.That(boxPlot.Median, Is.EqualTo(50));
    Assert.That(boxPlot.Q3, Is.EqualTo(75));
    Assert.That(boxPlot.Maximum, Is.EqualTo(90));
    Assert.That(boxPlot.Mean, Is.EqualTo(48));
    Assert.That(boxPlot.Color, Is.EqualTo(Color.Purple));
  }

  [Test]
  [Category("HappyPath")]
  public void BoxPlotData_IQR_CalculatesCorrectly() {
    var boxPlot = new BoxPlotData {
      Q1 = 25,
      Q3 = 75
    };

    Assert.That(boxPlot.IQR, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void BoxPlotData_Outliers_CanBeSet() {
    var boxPlot = new BoxPlotData {
      Category = "Group B",
      Outliers = new List<double> { 5.0, 95.0, 100.0 }
    };

    Assert.That(boxPlot.Outliers.Count, Is.EqualTo(3));
    Assert.That(boxPlot.Outliers[0], Is.EqualTo(5.0));
  }

  [Test]
  [Category("HappyPath")]
  public void HeatmapCell_Properties_CanBeSet() {
    var cell = new HeatmapCell(2, 3, 75.5) {
      Label = "Cell Label",
      Color = Color.Red
    };

    Assert.That(cell.Row, Is.EqualTo(2));
    Assert.That(cell.Column, Is.EqualTo(3));
    Assert.That(cell.Value, Is.EqualTo(75.5));
    Assert.That(cell.Label, Is.EqualTo("Cell Label"));
    Assert.That(cell.Color, Is.EqualTo(Color.Red));
  }

  [Test]
  [Category("HappyPath")]
  public void FunnelStage_Properties_CanBeSet() {
    var stage = new FunnelStage {
      Label = "Awareness",
      Value = 10000,
      Color = Color.Blue
    };

    Assert.That(stage.Label, Is.EqualTo("Awareness"));
    Assert.That(stage.Value, Is.EqualTo(10000));
    Assert.That(stage.Color, Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void WordCloudWord_Properties_CanBeSet() {
    var word = new WordCloudWord {
      Text = "Innovation",
      Weight = 50,
      Color = Color.DarkBlue
    };

    Assert.That(word.Text, Is.EqualTo("Innovation"));
    Assert.That(word.Weight, Is.EqualTo(50));
    Assert.That(word.Color, Is.EqualTo(Color.DarkBlue));
  }

  [Test]
  [Category("HappyPath")]
  public void TimelineEvent_Properties_CanBeSet() {
    var evt = new TimelineEvent {
      Date = new DateTime(2024, 6, 15),
      Title = "Product Launch",
      Description = "Major release",
      Color = Color.Green
    };

    Assert.That(evt.Date, Is.EqualTo(new DateTime(2024, 6, 15)));
    Assert.That(evt.Title, Is.EqualTo("Product Launch"));
    Assert.That(evt.Description, Is.EqualTo("Major release"));
    Assert.That(evt.Color, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void CalendarHeatmapDay_Properties_CanBeSet() {
    var day = new CalendarHeatmapDay {
      Date = new DateTime(2024, 3, 20),
      Value = 42,
      Label = "42 commits",
      Color = Color.DarkGreen
    };

    Assert.That(day.Date, Is.EqualTo(new DateTime(2024, 3, 20)));
    Assert.That(day.Value, Is.EqualTo(42));
    Assert.That(day.Label, Is.EqualTo("42 commits"));
    Assert.That(day.Color, Is.EqualTo(Color.DarkGreen));
  }

  [Test]
  [Category("HappyPath")]
  public void BulletData_Properties_CanBeSet() {
    var bullet = new BulletData {
      Label = "Revenue",
      Value = 85,
      Target = 100,
      Maximum = 120
    };

    Assert.That(bullet.Label, Is.EqualTo("Revenue"));
    Assert.That(bullet.Value, Is.EqualTo(85));
    Assert.That(bullet.Target, Is.EqualTo(100));
    Assert.That(bullet.Maximum, Is.EqualTo(120));
  }

  [Test]
  [Category("HappyPath")]
  public void WaterfallStep_Properties_CanBeSet() {
    var step = new WaterfallStep {
      Label = "Q1 Sales",
      Value = 5000,
      IsTotal = false,
      Color = Color.Green
    };

    Assert.That(step.Label, Is.EqualTo("Q1 Sales"));
    Assert.That(step.Value, Is.EqualTo(5000));
    Assert.That(step.IsTotal, Is.False);
    Assert.That(step.Color, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void WaterfallStep_IsPositive_CalculatesCorrectly() {
    var positive = new WaterfallStep { Value = 100 };
    var negative = new WaterfallStep { Value = -50 };
    var zero = new WaterfallStep { Value = 0 };

    Assert.That(positive.IsPositive, Is.True);
    Assert.That(negative.IsPositive, Is.False);
    Assert.That(zero.IsPositive, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void HistogramBin_Properties_CanBeSet() {
    var bin = new HistogramBin {
      Start = 0,
      End = 10,
      Count = 25,
      Density = 0.25
    };

    Assert.That(bin.Start, Is.EqualTo(0));
    Assert.That(bin.End, Is.EqualTo(10));
    Assert.That(bin.Count, Is.EqualTo(25));
    Assert.That(bin.Density, Is.EqualTo(0.25));
  }

  [Test]
  [Category("HappyPath")]
  public void HistogramBin_Width_CalculatesCorrectly() {
    var bin = new HistogramBin { Start = 10, End = 20 };

    Assert.That(bin.Width, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void HistogramBin_Center_CalculatesCorrectly() {
    var bin = new HistogramBin { Start = 10, End = 20 };

    Assert.That(bin.Center, Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void GaugeZone_Properties_CanBeSet() {
    var zone = new GaugeZone {
      Start = 0,
      End = 50,
      Color = Color.Green
    };

    Assert.That(zone.Start, Is.EqualTo(0));
    Assert.That(zone.End, Is.EqualTo(50));
    Assert.That(zone.Color, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void RangePoint_Properties_CanBeSet() {
    var point = new RangePoint {
      X = 5,
      Low = 10,
      High = 30,
      Mid = 20,
      Label = "Error Range"
    };

    Assert.That(point.X, Is.EqualTo(5));
    Assert.That(point.Low, Is.EqualTo(10));
    Assert.That(point.High, Is.EqualTo(30));
    Assert.That(point.Mid, Is.EqualTo(20));
    Assert.That(point.Label, Is.EqualTo("Error Range"));
  }

  [Test]
  [Category("HappyPath")]
  public void BubblePoint_Properties_CanBeSet() {
    var bubble = new BubblePoint {
      X = 10,
      Y = 20,
      Size = 5,
      Z = 15,
      Label = "Bubble"
    };

    Assert.That(bubble.X, Is.EqualTo(10));
    Assert.That(bubble.Y, Is.EqualTo(20));
    Assert.That(bubble.Size, Is.EqualTo(5));
    Assert.That(bubble.Z, Is.EqualTo(15));
    Assert.That(bubble.Label, Is.EqualTo("Bubble"));
  }

  [Test]
  [Category("EdgeCase")]
  public void ChartPoint_NegativeValues_Allowed() {
    var point = new ChartPoint {
      X = -100,
      Y = -200
    };

    Assert.That(point.X, Is.EqualTo(-100));
    Assert.That(point.Y, Is.EqualTo(-200));
  }

  [Test]
  [Category("EdgeCase")]
  public void BoxPlotData_EqualQuartiles_Allowed() {
    var boxPlot = new BoxPlotData {
      Minimum = 50,
      Q1 = 50,
      Median = 50,
      Q3 = 50,
      Maximum = 50
    };

    Assert.That(boxPlot.Minimum, Is.EqualTo(boxPlot.Maximum));
    Assert.That(boxPlot.Q1, Is.EqualTo(boxPlot.Q3));
  }

  [Test]
  [Category("EdgeCase")]
  public void BoxPlotData_ZeroIQR_Allowed() {
    var boxPlot = new BoxPlotData {
      Q1 = 50,
      Q3 = 50
    };

    Assert.That(boxPlot.IQR, Is.EqualTo(0));
  }
}
