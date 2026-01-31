using System.Drawing;
using System.Windows.Forms.Charting.Diagrams;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class DiagramControlTests {
  private DiagramControl _diagram;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._diagram = new DiagramControl { Size = new Size(800, 600) };
    this._form = new Form();
    this._form.Controls.Add(this._diagram);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._diagram?.Dispose();
    this._form?.Dispose();
  }

  #region Basic Properties

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._diagram.DiagramType, Is.EqualTo(DiagramType.Network));
    Assert.That(this._diagram.EnableTooltips, Is.True);
    Assert.That(this._diagram.EnableAnimation, Is.False);
    Assert.That(this._diagram.ShowLegend, Is.True);
    Assert.That(this._diagram.SelectionMode, Is.EqualTo(DiagramSelectionMode.None));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramType_CanBeSetAndRetrieved() {
    foreach (DiagramType diagramType in Enum.GetValues(typeof(DiagramType))) {
      this._diagram.DiagramType = diagramType;
      Assert.That(this._diagram.DiagramType, Is.EqualTo(diagramType));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void Title_CanBeSetAndRetrieved() {
    this._diagram.Title = "Diagram Title";
    Assert.That(this._diagram.Title, Is.EqualTo("Diagram Title"));
  }

  [Test]
  [Category("HappyPath")]
  public void Subtitle_CanBeSetAndRetrieved() {
    this._diagram.Subtitle = "Diagram Subtitle";
    Assert.That(this._diagram.Subtitle, Is.EqualTo("Diagram Subtitle"));
  }

  [Test]
  [Category("HappyPath")]
  public void DataCollections_AreAccessible() {
    Assert.That(this._diagram.Nodes, Is.Not.Null);
    Assert.That(this._diagram.Edges, Is.Not.Null);
    Assert.That(this._diagram.SankeyLinks, Is.Not.Null);
    Assert.That(this._diagram.HierarchyNodes, Is.Not.Null);
    Assert.That(this._diagram.NetworkData, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void EnableAnimation_CanBeSetAndRetrieved() {
    this._diagram.EnableAnimation = true;
    Assert.That(this._diagram.EnableAnimation, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowLegend_CanBeSetAndRetrieved() {
    this._diagram.ShowLegend = false;
    Assert.That(this._diagram.ShowLegend, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void LegendPosition_CanBeSetAndRetrieved() {
    this._diagram.LegendPosition = DiagramLegendPosition.Bottom;
    Assert.That(this._diagram.LegendPosition, Is.EqualTo(DiagramLegendPosition.Bottom));
  }

  [Test]
  [Category("HappyPath")]
  public void ColorPalette_CanBeSetAndRetrieved() {
    this._diagram.ColorPalette = DiagramColorPalette.Material;
    Assert.That(this._diagram.ColorPalette, Is.EqualTo(DiagramColorPalette.Material));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllData() {
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Edges.Add(new DiagramEdge { Source = "A", Target = "B" });
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 100));
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "root", Label = "Root" });

    this._diagram.Clear();

    Assert.That(this._diagram.Nodes.Count, Is.EqualTo(0));
    Assert.That(this._diagram.Edges.Count, Is.EqualTo(0));
    Assert.That(this._diagram.SankeyLinks.Count, Is.EqualTo(0));
    Assert.That(this._diagram.HierarchyNodes.Count, Is.EqualTo(0));
  }

  #endregion

  #region All Diagram Types - Empty Data Rendering

  [Test]
  [Category("EdgeCase")]
  [TestCase(DiagramType.Sankey)]
  [TestCase(DiagramType.Chord)]
  [TestCase(DiagramType.Arc)]
  [TestCase(DiagramType.Network)]
  [TestCase(DiagramType.Tree)]
  [TestCase(DiagramType.Dendrogram)]
  [TestCase(DiagramType.CirclePacking)]
  [TestCase(DiagramType.FlowChart)]
  public void Render_WithEmptyData_DoesNotThrow(DiagramType diagramType) {
    this._diagram.DiagramType = diagramType;

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
      Assert.That(bitmap.Width, Is.EqualTo(800));
      Assert.That(bitmap.Height, Is.EqualTo(600));
    });
  }

  #endregion

  #region Network Diagram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(50)]
  [TestCase(100)]
  public void Render_NetworkDiagram_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.Network;
    var rand = new Random(42);

    for (var i = 0; i < nodeCount; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"N{i}", $"Node {i}") {
        Size = 10 + rand.Next(20)
      });

    for (var i = 0; i < nodeCount - 1; ++i)
      this._diagram.Edges.Add(new DiagramEdge {
        Source = $"N{i}",
        Target = $"N{(i + 1) % nodeCount}",
        Weight = rand.Next(1, 10)
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_NetworkDiagram_WithMultipleEdgesPerNode() {
    this._diagram.DiagramType = DiagramType.Network;

    for (var i = 0; i < 5; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"N{i}", $"Node {i}"));

    this._diagram.Edges.Add(new DiagramEdge { Source = "N0", Target = "N1" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N0", Target = "N2" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N0", Target = "N3" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N0", Target = "N4" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N1", Target = "N2" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N2", Target = "N3" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N3", Target = "N4" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "N4", Target = "N1" });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Sankey Diagram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(30)]
  [TestCase(50)]
  public void Render_SankeyDiagram_VariousLinkCounts(int linkCount) {
    this._diagram.DiagramType = DiagramType.Sankey;
    var rand = new Random(42);

    var sources = new[] { "Source A", "Source B", "Source C" };
    var targets = new[] { "Target X", "Target Y", "Target Z" };

    for (var i = 0; i < sources.Length; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"S{i}", sources[i]));

    for (var i = 0; i < targets.Length; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"T{i}", targets[i]));

    for (var i = 0; i < linkCount; ++i)
      this._diagram.SankeyLinks.Add(new DiagramSankeyLink(
        $"S{i % sources.Length}",
        $"T{i % targets.Length}",
        rand.Next(10, 100)
      ));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_SankeyDiagram_WithMultipleLayers() {
    this._diagram.DiagramType = DiagramType.Sankey;

    this._diagram.Nodes.Add(new DiagramNode("A", "Input A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "Input B"));
    this._diagram.Nodes.Add(new DiagramNode("M1", "Middle 1"));
    this._diagram.Nodes.Add(new DiagramNode("M2", "Middle 2"));
    this._diagram.Nodes.Add(new DiagramNode("X", "Output X"));
    this._diagram.Nodes.Add(new DiagramNode("Y", "Output Y"));

    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "M1", 60));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "M2", 40));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("B", "M1", 30));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("B", "M2", 70));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("M1", "X", 50));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("M1", "Y", 40));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("M2", "X", 60));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("M2", "Y", 50));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Chord Diagram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(5)]
  [TestCase(10)]
  [TestCase(20)]
  public void Render_ChordDiagram_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.Chord;
    var rand = new Random(42);

    for (var i = 0; i < nodeCount; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"N{i}", $"Node {i}"));

    for (var i = 0; i < nodeCount; ++i)
    for (var j = i + 1; j < nodeCount; ++j)
      if (rand.NextDouble() > 0.5)
        this._diagram.Edges.Add(new DiagramEdge {
          Source = $"N{i}",
          Target = $"N{j}",
          Weight = rand.Next(1, 50)
        });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Arc Diagram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(5)]
  [TestCase(10)]
  [TestCase(30)]
  public void Render_ArcDiagram_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.Arc;
    var rand = new Random(42);

    for (var i = 0; i < nodeCount; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"N{i}", $"Node {i}"));

    for (var i = 0; i < nodeCount * 2; ++i) {
      var source = rand.Next(nodeCount);
      var target = rand.Next(nodeCount);
      if (source != target)
        this._diagram.Edges.Add(new DiagramEdge {
          Source = $"N{source}",
          Target = $"N{target}",
          Weight = rand.Next(1, 10)
        });
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Tree Diagram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(7)]
  [TestCase(15)]
  [TestCase(31)]
  public void Render_TreeDiagram_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.Tree;

    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
      Id = "root",
      Label = "Root",
      Value = 100
    });

    for (var i = 1; i < nodeCount; ++i) {
      var parentIndex = (i - 1) / 2;
      this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
        Id = $"N{i}",
        Label = $"Node {i}",
        ParentId = parentIndex == 0 ? "root" : $"N{parentIndex}",
        Value = 50 + i * 5
      });
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_TreeDiagram_WithDeepHierarchy() {
    this._diagram.DiagramType = DiagramType.Tree;

    var depth = 10;
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
      Id = "root",
      Label = "Root"
    });

    for (var i = 1; i <= depth; ++i)
      this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
        Id = $"Level{i}",
        Label = $"Level {i}",
        ParentId = i == 1 ? "root" : $"Level{i - 1}"
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_TreeDiagram_WithWideHierarchy() {
    this._diagram.DiagramType = DiagramType.Tree;

    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
      Id = "root",
      Label = "Root"
    });

    for (var i = 0; i < 20; ++i)
      this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
        Id = $"Child{i}",
        Label = $"Child {i}",
        ParentId = "root"
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Dendrogram Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(10)]
  [TestCase(20)]
  public void Render_Dendrogram_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.Dendrogram;

    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
      Id = "root",
      Label = "Root"
    });

    for (var i = 1; i < nodeCount; ++i) {
      var parentIndex = (i - 1) / 2;
      this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
        Id = $"N{i}",
        Label = $"N{i}",
        ParentId = parentIndex == 0 ? "root" : $"N{parentIndex}"
      });
    }

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Circle Packing Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(5)]
  [TestCase(15)]
  [TestCase(30)]
  public void Render_CirclePacking_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.CirclePacking;
    var rand = new Random(42);

    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
      Id = "root",
      Label = "Root",
      Value = 1000
    });

    for (var i = 1; i < nodeCount; ++i)
      this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode {
        Id = $"N{i}",
        Label = $"N{i}",
        ParentId = "root",
        Value = rand.Next(10, 100)
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_CirclePacking_WithNestedHierarchy() {
    this._diagram.DiagramType = DiagramType.CirclePacking;

    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "root", Label = "Root", Value = 1000 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "A", Label = "A", ParentId = "root", Value = 400 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "B", Label = "B", ParentId = "root", Value = 300 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "C", Label = "C", ParentId = "root", Value = 300 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "A1", Label = "A1", ParentId = "A", Value = 200 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "A2", Label = "A2", ParentId = "A", Value = 200 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "B1", Label = "B1", ParentId = "B", Value = 150 });
    this._diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "B2", Label = "B2", ParentId = "B", Value = 150 });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region FlowChart Tests

  [Test]
  [Category("HappyPath")]
  [TestCase(3)]
  [TestCase(7)]
  [TestCase(15)]
  public void Render_FlowChart_VariousNodeCounts(int nodeCount) {
    this._diagram.DiagramType = DiagramType.FlowChart;

    for (var i = 0; i < nodeCount; ++i)
      this._diagram.Nodes.Add(new DiagramNode($"Step{i}", $"Step {i}") {
        Shape = i == 0 ? DiagramNodeShape.Ellipse :
          i == nodeCount - 1 ? DiagramNodeShape.Ellipse :
          DiagramNodeShape.Rectangle
      });

    for (var i = 0; i < nodeCount - 1; ++i)
      this._diagram.Edges.Add(new DiagramEdge {
        Source = $"Step{i}",
        Target = $"Step{i + 1}",
        Directed = true
      });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("HappyPath")]
  public void Render_FlowChart_WithBranching() {
    this._diagram.DiagramType = DiagramType.FlowChart;

    this._diagram.Nodes.Add(new DiagramNode("start", "Start") { Shape = DiagramNodeShape.Ellipse });
    this._diagram.Nodes.Add(new DiagramNode("decision", "Decision?") { Shape = DiagramNodeShape.Diamond });
    this._diagram.Nodes.Add(new DiagramNode("yes", "Yes Path") { Shape = DiagramNodeShape.Rectangle });
    this._diagram.Nodes.Add(new DiagramNode("no", "No Path") { Shape = DiagramNodeShape.Rectangle });
    this._diagram.Nodes.Add(new DiagramNode("end", "End") { Shape = DiagramNodeShape.Ellipse });

    this._diagram.Edges.Add(new DiagramEdge { Source = "start", Target = "decision", Directed = true });
    this._diagram.Edges.Add(new DiagramEdge { Source = "decision", Target = "yes", Directed = true, Label = "Yes" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "decision", Target = "no", Directed = true, Label = "No" });
    this._diagram.Edges.Add(new DiagramEdge { Source = "yes", Target = "end", Directed = true });
    this._diagram.Edges.Add(new DiagramEdge { Source = "no", Target = "end", Directed = true });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  #endregion

  #region Event Tests

  [Test]
  [Category("HappyPath")]
  public void NodeClicked_EventCanBeSubscribed() {
    var eventRaised = false;
    this._diagram.NodeClicked += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void EdgeClicked_EventCanBeSubscribed() {
    var eventRaised = false;
    this._diagram.EdgeClicked += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NodeHovered_EventCanBeSubscribed() {
    var eventRaised = false;
    this._diagram.NodeHovered += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void LinkClicked_EventCanBeSubscribed() {
    var eventRaised = false;
    this._diagram.LinkClicked += (s, e) => eventRaised = true;

    Assert.That(eventRaised, Is.False);
  }

  #endregion

  #region Export Tests

  [Test]
  [Category("HappyPath")]
  public void ToImage_ReturnsValidBitmap() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "Node B"));
    this._diagram.Edges.Add(new DiagramEdge { Source = "A", Target = "B" });

    using var bitmap = this._diagram.ToImage();

    Assert.That(bitmap, Is.Not.Null);
    Assert.That(bitmap.Width, Is.GreaterThan(0));
    Assert.That(bitmap.Height, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ToImage_WithCustomDimensions_ReturnsCorrectSize() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));

    using var bitmap = this._diagram.ToImage(1024, 768);

    Assert.That(bitmap.Width, Is.EqualTo(1024));
    Assert.That(bitmap.Height, Is.EqualTo(768));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Render_WithSingleNode_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("only", "Only Node"));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithSelfReferencingEdge_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Edges.Add(new DiagramEdge { Source = "A", Target = "A" });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithDisconnectedNodes_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "Node B"));
    this._diagram.Nodes.Add(new DiagramNode("C", "Node C"));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithZeroValueLinks_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Sankey;
    this._diagram.Nodes.Add(new DiagramNode("A", "A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "B"));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 0));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithVeryLargeValues_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Sankey;
    this._diagram.Nodes.Add(new DiagramNode("A", "A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "B"));
    this._diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 1e12));

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithAnimation_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.EnableAnimation = true;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "Node B"));
    this._diagram.Edges.Add(new DiagramEdge { Source = "A", Target = "B" });

    Assert.DoesNotThrow(() => {
      using var bitmap = this._diagram.ToImage();
      Assert.That(bitmap, Is.Not.Null);
    });
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithLegendPositions_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.ShowLegend = true;
    this._diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "Node B"));

    foreach (DiagramLegendPosition position in Enum.GetValues(typeof(DiagramLegendPosition))) {
      this._diagram.LegendPosition = position;
      Assert.DoesNotThrow(() => {
        using var bitmap = this._diagram.ToImage();
        Assert.That(bitmap, Is.Not.Null);
      });
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void Render_WithAllColorPalettes_DoesNotThrow() {
    this._diagram.DiagramType = DiagramType.Network;
    this._diagram.Nodes.Add(new DiagramNode("A", "A"));
    this._diagram.Nodes.Add(new DiagramNode("B", "B"));
    this._diagram.Nodes.Add(new DiagramNode("C", "C"));

    foreach (DiagramColorPalette palette in Enum.GetValues(typeof(DiagramColorPalette))) {
      if (palette == DiagramColorPalette.Custom)
        continue;

      this._diagram.ColorPalette = palette;
      Assert.DoesNotThrow(() => {
        using var bitmap = this._diagram.ToImage();
        Assert.That(bitmap, Is.Not.Null);
      });
    }
  }

  #endregion
}
