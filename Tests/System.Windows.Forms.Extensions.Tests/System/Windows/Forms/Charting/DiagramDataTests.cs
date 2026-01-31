using System.Drawing;
using System.Windows.Forms.Charting.Diagrams;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class DiagramDataTests {
  #region DiagramNode Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramNode_ConstructorWithIdOnly_SetsIdAndLabelToId() {
    var node = new DiagramNode("test-id");

    Assert.That(node.Id, Is.EqualTo("test-id"));
    Assert.That(node.Label, Is.EqualTo("test-id"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNode_ConstructorWithIdAndLabel_SetsBothProperties() {
    var node = new DiagramNode("test-id", "Test Label");

    Assert.That(node.Id, Is.EqualTo("test-id"));
    Assert.That(node.Label, Is.EqualTo("Test Label"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNode_Properties_CanBeSet() {
    var node = new DiagramNode("id") {
      Label = "My Node",
      Size = 25.5,
      Color = Color.Blue,
      Shape = DiagramNodeShape.Diamond,
      Position = new PointF(100, 200),
      Group = "Group A",
      Tag = "custom data"
    };

    Assert.That(node.Label, Is.EqualTo("My Node"));
    Assert.That(node.Size, Is.EqualTo(25.5));
    Assert.That(node.Color, Is.EqualTo(Color.Blue));
    Assert.That(node.Shape, Is.EqualTo(DiagramNodeShape.Diamond));
    Assert.That(node.Position.X, Is.EqualTo(100));
    Assert.That(node.Position.Y, Is.EqualTo(200));
    Assert.That(node.Group, Is.EqualTo("Group A"));
    Assert.That(node.Tag, Is.EqualTo("custom data"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNode_DefaultSize_IsOne() {
    var node = new DiagramNode("id");

    Assert.That(node.Size, Is.EqualTo(1.0));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNode_DefaultShape_IsRoundedRectangle() {
    var node = new DiagramNode("id");

    Assert.That(node.Shape, Is.EqualTo(DiagramNodeShape.RoundedRectangle));
  }

  #endregion

  #region DiagramEdge Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramEdge_Properties_CanBeSet() {
    var edge = new DiagramEdge {
      Source = "nodeA",
      Target = "nodeB",
      Weight = 5.5,
      Label = "Connection",
      Color = Color.Red,
      Directed = true,
      Tag = "edge data"
    };

    Assert.That(edge.Source, Is.EqualTo("nodeA"));
    Assert.That(edge.Target, Is.EqualTo("nodeB"));
    Assert.That(edge.Weight, Is.EqualTo(5.5));
    Assert.That(edge.Label, Is.EqualTo("Connection"));
    Assert.That(edge.Color, Is.EqualTo(Color.Red));
    Assert.That(edge.Directed, Is.True);
    Assert.That(edge.Tag, Is.EqualTo("edge data"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramEdge_DefaultWeight_IsOne() {
    var edge = new DiagramEdge { Source = "A", Target = "B" };

    Assert.That(edge.Weight, Is.EqualTo(1.0));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramEdge_DefaultDirected_IsFalse() {
    var edge = new DiagramEdge { Source = "A", Target = "B" };

    Assert.That(edge.Directed, Is.False);
  }

  #endregion

  #region DiagramSankeyLink Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramSankeyLink_Constructor_SetsProperties() {
    var link = new DiagramSankeyLink("source", "target", 100);

    Assert.That(link.Source, Is.EqualTo("source"));
    Assert.That(link.Target, Is.EqualTo("target"));
    Assert.That(link.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSankeyLink_Properties_CanBeSet() {
    var link = new DiagramSankeyLink("A", "B", 50) {
      Color = Color.Gray,
      Label = "Flow",
      Tag = "link data"
    };

    Assert.That(link.Source, Is.EqualTo("A"));
    Assert.That(link.Target, Is.EqualTo("B"));
    Assert.That(link.Value, Is.EqualTo(50));
    Assert.That(link.Color, Is.EqualTo(Color.Gray));
    Assert.That(link.Label, Is.EqualTo("Flow"));
    Assert.That(link.Tag, Is.EqualTo("link data"));
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramSankeyLink_ZeroValue_Allowed() {
    var link = new DiagramSankeyLink("A", "B", 0);

    Assert.That(link.Value, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramSankeyLink_NegativeValue_Allowed() {
    var link = new DiagramSankeyLink("A", "B", -50);

    Assert.That(link.Value, Is.EqualTo(-50));
  }

  #endregion

  #region DiagramHierarchyNode Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramHierarchyNode_Properties_CanBeSet() {
    var node = new DiagramHierarchyNode {
      Id = "root",
      Label = "Root Node",
      ParentId = null,
      Value = 1000,
      Color = Color.Orange,
      Tag = "hierarchy data"
    };

    Assert.That(node.Id, Is.EqualTo("root"));
    Assert.That(node.Label, Is.EqualTo("Root Node"));
    Assert.That(node.ParentId, Is.Null);
    Assert.That(node.Value, Is.EqualTo(1000));
    Assert.That(node.Color, Is.EqualTo(Color.Orange));
    Assert.That(node.Tag, Is.EqualTo("hierarchy data"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramHierarchyNode_WithParent_SetsParentId() {
    var child = new DiagramHierarchyNode {
      Id = "child1",
      Label = "Child Node",
      ParentId = "root",
      Value = 100
    };

    Assert.That(child.ParentId, Is.EqualTo("root"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramHierarchyNode_DefaultValue_IsZero() {
    var node = new DiagramHierarchyNode { Id = "test" };

    Assert.That(node.Value, Is.EqualTo(0));
  }

  #endregion

  #region DiagramNetworkData Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramNetworkData_Constructor_SetsCollections() {
    var diagram = new DiagramControl();
    var nodes = diagram.Nodes;
    var edges = diagram.Edges;

    var networkData = diagram.NetworkData;

    Assert.That(networkData.Nodes, Is.Not.Null);
    Assert.That(networkData.Edges, Is.Not.Null);
  }

  #endregion

  #region Collection Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramNodeCollection_Add_IncreasesCount() {
    var diagram = new DiagramControl();

    diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    diagram.Nodes.Add(new DiagramNode("B", "Node B"));

    Assert.That(diagram.Nodes.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNodeCollection_Clear_RemovesAllItems() {
    var diagram = new DiagramControl();
    diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    diagram.Nodes.Add(new DiagramNode("B", "Node B"));

    diagram.Nodes.Clear();

    Assert.That(diagram.Nodes.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramEdgeCollection_Add_IncreasesCount() {
    var diagram = new DiagramControl();

    diagram.Edges.Add(new DiagramEdge { Source = "A", Target = "B" });
    diagram.Edges.Add(new DiagramEdge { Source = "B", Target = "C" });

    Assert.That(diagram.Edges.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSankeyLinkCollection_Add_IncreasesCount() {
    var diagram = new DiagramControl();

    diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 100));
    diagram.SankeyLinks.Add(new DiagramSankeyLink("B", "C", 50));

    Assert.That(diagram.SankeyLinks.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramHierarchyNodeCollection_Add_IncreasesCount() {
    var diagram = new DiagramControl();

    diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "root", Label = "Root" });
    diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "child", Label = "Child", ParentId = "root" });

    Assert.That(diagram.HierarchyNodes.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNodeCollection_Indexer_ReturnsCorrectItem() {
    var diagram = new DiagramControl();
    diagram.Nodes.Add(new DiagramNode("A", "Node A"));
    diagram.Nodes.Add(new DiagramNode("B", "Node B"));

    Assert.That(diagram.Nodes[0].Id, Is.EqualTo("A"));
    Assert.That(diagram.Nodes[1].Id, Is.EqualTo("B"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramNodeCollection_Remove_DecreasesCount() {
    var diagram = new DiagramControl();
    var node = new DiagramNode("A", "Node A");
    diagram.Nodes.Add(node);
    diagram.Nodes.Add(new DiagramNode("B", "Node B"));

    diagram.Nodes.Remove(node);

    Assert.That(diagram.Nodes.Count, Is.EqualTo(1));
    Assert.That(diagram.Nodes[0].Id, Is.EqualTo("B"));
  }

  #endregion

  #region DiagramNodeShape Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramNodeShape_AllValuesExist() {
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Rectangle));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.RoundedRectangle));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Circle));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Ellipse));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Diamond));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Triangle));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Hexagon));
    Assert.That(Enum.GetValues(typeof(DiagramNodeShape)), Has.Member(DiagramNodeShape.Custom));
  }

  #endregion

  #region DiagramType Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramType_AllValuesExist() {
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Sankey));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Chord));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Arc));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Network));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Tree));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Dendrogram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.CirclePacking));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.FlowChart));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void DiagramNode_EmptyId_Allowed() {
    var node = new DiagramNode("");

    Assert.That(node.Id, Is.EqualTo(""));
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramNode_NullColor_UsesDefault() {
    var node = new DiagramNode("id");

    Assert.That(node.Color, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramEdge_SameSourceAndTarget_Allowed() {
    var edge = new DiagramEdge {
      Source = "A",
      Target = "A"
    };

    Assert.That(edge.Source, Is.EqualTo(edge.Target));
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramSankeyLink_VeryLargeValue_Allowed() {
    var link = new DiagramSankeyLink("A", "B", double.MaxValue);

    Assert.That(link.Value, Is.EqualTo(double.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void DiagramHierarchyNode_CircularReference_AllowedInData() {
    var diagram = new DiagramControl();
    diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "A", ParentId = "B" });
    diagram.HierarchyNodes.Add(new DiagramHierarchyNode { Id = "B", ParentId = "A" });

    Assert.That(diagram.HierarchyNodes.Count, Is.EqualTo(2));
  }

  #endregion
}
