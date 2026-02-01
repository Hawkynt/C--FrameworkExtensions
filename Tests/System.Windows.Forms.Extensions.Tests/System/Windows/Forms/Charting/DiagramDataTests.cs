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
    // Original types
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Sankey));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Chord));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Arc));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Network));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Tree));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Dendrogram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.CirclePacking));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.FlowChart));
    // Hierarchical/Organizational
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.OrgChart));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.MindMap));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.WBS));
    // UML
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.ClassDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.SequenceDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.StateDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.UseCaseDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.ActivityDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.ComponentDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.DeploymentDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.ObjectDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.PackageDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.CommunicationDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.TimingDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.InteractionOverview));
    // Database
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.EntityRelationship));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.DataFlow));
    // Business
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.VennDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Fishbone));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.DecisionTree));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Matrix));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.SWOT));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.JourneyMap));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.BPMN));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Kanban));
    // Architecture
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.C4Context));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.C4Container));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.C4Component));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.C4Deployment));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Gitgraph));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Requirement));
    // Technical
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.BlockDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.RackDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.NetworkTopology));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.PacketDiagram));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.ByteField));
    Assert.That(Enum.GetValues(typeof(DiagramType)), Has.Member(DiagramType.Waveform));
  }

  #endregion

  #region UML Data Structure Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramClassNode_Properties_CanBeSet() {
    var classNode = new DiagramClassNode {
      Id = "MyClass",
      ClassName = "MyClass",
      Stereotype = "interface",
      Color = Color.LightBlue
    };
    classNode.Fields.Add(new DiagramClassMember { Name = "field1", Type = "string", Visibility = DiagramVisibility.Private });
    classNode.Methods.Add(new DiagramClassMember { Name = "Method1", Type = "void", Visibility = DiagramVisibility.Public, IsAbstract = true });

    Assert.That(classNode.Id, Is.EqualTo("MyClass"));
    Assert.That(classNode.ClassName, Is.EqualTo("MyClass"));
    Assert.That(classNode.Stereotype, Is.EqualTo("interface"));
    Assert.That(classNode.Fields.Count, Is.EqualTo(1));
    Assert.That(classNode.Methods.Count, Is.EqualTo(1));
    Assert.That(classNode.Methods[0].IsAbstract, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramClassMember_AllVisibilities() {
    Assert.That(Enum.GetValues(typeof(DiagramVisibility)), Has.Member(DiagramVisibility.Public));
    Assert.That(Enum.GetValues(typeof(DiagramVisibility)), Has.Member(DiagramVisibility.Private));
    Assert.That(Enum.GetValues(typeof(DiagramVisibility)), Has.Member(DiagramVisibility.Protected));
    Assert.That(Enum.GetValues(typeof(DiagramVisibility)), Has.Member(DiagramVisibility.Internal));
    Assert.That(Enum.GetValues(typeof(DiagramVisibility)), Has.Member(DiagramVisibility.Package));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramLifeline_Properties_CanBeSet() {
    var lifeline = new DiagramLifeline {
      Id = "client",
      Name = "Client",
      Stereotype = "actor",
      Color = Color.Green
    };

    Assert.That(lifeline.Id, Is.EqualTo("client"));
    Assert.That(lifeline.Name, Is.EqualTo("Client"));
    Assert.That(lifeline.Stereotype, Is.EqualTo("actor"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramMessage_AllMessageTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Sync));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Async));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Return));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Create));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Destroy));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.SelfCall));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Found));
    Assert.That(Enum.GetValues(typeof(DiagramMessageType)), Has.Member(DiagramMessageType.Lost));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramRelationType_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Inheritance));
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Implementation));
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Association));
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Aggregation));
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Composition));
    Assert.That(Enum.GetValues(typeof(DiagramRelationType)), Has.Member(DiagramRelationType.Dependency));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramActor_Properties_CanBeSet() {
    var actor = new DiagramActor { Id = "user", Name = "User", IsSystem = false };

    Assert.That(actor.Id, Is.EqualTo("user"));
    Assert.That(actor.Name, Is.EqualTo("User"));
    Assert.That(actor.IsSystem, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramUseCase_Properties_CanBeSet() {
    var useCase = new DiagramUseCase {
      Id = "login",
      Name = "Login",
      SystemBoundary = "AuthSystem"
    };
    useCase.ExtensionPoints.Add("timeout");

    Assert.That(useCase.Id, Is.EqualTo("login"));
    Assert.That(useCase.Name, Is.EqualTo("Login"));
    Assert.That(useCase.SystemBoundary, Is.EqualTo("AuthSystem"));
    Assert.That(useCase.ExtensionPoints.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramComponent_Properties_CanBeSet() {
    var component = new DiagramComponent {
      Id = "api",
      Name = "API Server",
      Stereotype = "service"
    };
    component.ProvidedInterfaces.Add("IRestApi");
    component.RequiredInterfaces.Add("IDatabase");

    Assert.That(component.Id, Is.EqualTo("api"));
    Assert.That(component.ProvidedInterfaces.Count, Is.EqualTo(1));
    Assert.That(component.RequiredInterfaces.Count, Is.EqualTo(1));
  }

  #endregion

  #region Database Data Structure Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramEntity_Properties_CanBeSet() {
    var entity = new DiagramEntity {
      Id = "users",
      Name = "Users",
      Color = Color.LightYellow
    };
    entity.Attributes.Add(new DiagramEntityAttribute {
      Name = "Id",
      DataType = "int",
      IsPrimaryKey = true
    });
    entity.Attributes.Add(new DiagramEntityAttribute {
      Name = "Email",
      DataType = "varchar(255)",
      IsUnique = true
    });

    Assert.That(entity.Id, Is.EqualTo("users"));
    Assert.That(entity.Name, Is.EqualTo("Users"));
    Assert.That(entity.Attributes.Count, Is.EqualTo(2));
    Assert.That(entity.Attributes[0].IsPrimaryKey, Is.True);
    Assert.That(entity.Attributes[1].IsUnique, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramCardinality_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramCardinality)), Has.Member(DiagramCardinality.One));
    Assert.That(Enum.GetValues(typeof(DiagramCardinality)), Has.Member(DiagramCardinality.ZeroOrOne));
    Assert.That(Enum.GetValues(typeof(DiagramCardinality)), Has.Member(DiagramCardinality.Many));
    Assert.That(Enum.GetValues(typeof(DiagramCardinality)), Has.Member(DiagramCardinality.OneOrMore));
    Assert.That(Enum.GetValues(typeof(DiagramCardinality)), Has.Member(DiagramCardinality.ZeroOrMore));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramDataFlowElement_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramDFDType)), Has.Member(DiagramDFDType.Process));
    Assert.That(Enum.GetValues(typeof(DiagramDFDType)), Has.Member(DiagramDFDType.DataStore));
    Assert.That(Enum.GetValues(typeof(DiagramDFDType)), Has.Member(DiagramDFDType.ExternalEntity));
    Assert.That(Enum.GetValues(typeof(DiagramDFDType)), Has.Member(DiagramDFDType.DataFlow));
  }

  #endregion

  #region Business Data Structure Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramSet_Properties_CanBeSet() {
    var set = new DiagramSet { Id = "A", Label = "Set A", Size = 100, Color = Color.Blue };

    Assert.That(set.Id, Is.EqualTo("A"));
    Assert.That(set.Label, Is.EqualTo("Set A"));
    Assert.That(set.Size, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSetIntersection_Properties_CanBeSet() {
    var intersection = new DiagramSetIntersection { Label = "A ∩ B", Value = 30 };
    intersection.SetIds.Add("A");
    intersection.SetIds.Add("B");

    Assert.That(intersection.SetIds.Count, Is.EqualTo(2));
    Assert.That(intersection.Value, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramQuadrant_Properties_CanBeSet() {
    var quadrant = new DiagramQuadrant {
      TopLeftLabel = "TL",
      TopRightLabel = "TR",
      BottomLeftLabel = "BL",
      BottomRightLabel = "BR",
      XAxisLabel = "X",
      YAxisLabel = "Y",
      TopLeftColor = Color.Green,
      TopRightColor = Color.Yellow,
      BottomLeftColor = Color.Blue,
      BottomRightColor = Color.Red
    };

    Assert.That(quadrant.TopLeftLabel, Is.EqualTo("TL"));
    Assert.That(quadrant.XAxisLabel, Is.EqualTo("X"));
    Assert.That(quadrant.TopLeftColor, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramJourneyStage_Properties_CanBeSet() {
    var stage = new DiagramJourneyStage { Id = "stage1", Label = "Discovery", Order = 1 };
    stage.Actions.Add(new DiagramJourneyAction { Actor = "User", Action = "Search", Score = 2 });

    Assert.That(stage.Id, Is.EqualTo("stage1"));
    Assert.That(stage.Order, Is.EqualTo(1));
    Assert.That(stage.Actions.Count, Is.EqualTo(1));
    Assert.That(stage.Actions[0].Score, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramBPMNElement_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.Task));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.Gateway));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.StartEvent));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.EndEvent));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.IntermediateEvent));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.Pool));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.Lane));
    Assert.That(Enum.GetValues(typeof(DiagramBPMNType)), Has.Member(DiagramBPMNType.SubProcess));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramKanbanColumn_Properties_CanBeSet() {
    var column = new DiagramKanbanColumn { Id = "doing", Name = "Doing", Order = 2, WipLimit = 3 };

    Assert.That(column.Id, Is.EqualTo("doing"));
    Assert.That(column.Name, Is.EqualTo("Doing"));
    Assert.That(column.WipLimit, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramKanbanCard_Properties_CanBeSet() {
    var card = new DiagramKanbanCard {
      Id = "card1",
      Title = "Implement feature",
      ColumnId = "doing",
      Assignee = "John",
      Color = Color.Yellow,
      Order = 1
    };

    Assert.That(card.Id, Is.EqualTo("card1"));
    Assert.That(card.Title, Is.EqualTo("Implement feature"));
    Assert.That(card.Assignee, Is.EqualTo("John"));
  }

  #endregion

  #region Architecture Data Structure Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramGitCommit_Properties_CanBeSet() {
    var commit = new DiagramGitCommit {
      Id = "abc123",
      Message = "Initial commit",
      Branch = "main",
      Type = DiagramGitCommitType.Normal
    };
    commit.ParentIds.Add("def456");

    Assert.That(commit.Id, Is.EqualTo("abc123"));
    Assert.That(commit.Message, Is.EqualTo("Initial commit"));
    Assert.That(commit.ParentIds.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramGitCommitType_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramGitCommitType)), Has.Member(DiagramGitCommitType.Normal));
    Assert.That(Enum.GetValues(typeof(DiagramGitCommitType)), Has.Member(DiagramGitCommitType.Merge));
    Assert.That(Enum.GetValues(typeof(DiagramGitCommitType)), Has.Member(DiagramGitCommitType.CherryPick));
    Assert.That(Enum.GetValues(typeof(DiagramGitCommitType)), Has.Member(DiagramGitCommitType.Revert));
    Assert.That(Enum.GetValues(typeof(DiagramGitCommitType)), Has.Member(DiagramGitCommitType.Tag));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramRequirement_Properties_CanBeSet() {
    var req = new DiagramRequirement {
      Id = "REQ-001",
      Name = "Login Feature",
      Text = "Users must be able to log in",
      Type = DiagramRequirementType.FunctionalReq,
      Risk = "High",
      VerifyMethod = "Test"
    };

    Assert.That(req.Id, Is.EqualTo("REQ-001"));
    Assert.That(req.Type, Is.EqualTo(DiagramRequirementType.FunctionalReq));
    Assert.That(req.Risk, Is.EqualTo("High"));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramRequirementRelationType_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Contains));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Copies));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Derives));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Refines));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Traces));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Satisfies));
    Assert.That(Enum.GetValues(typeof(DiagramRequirementRelationType)), Has.Member(DiagramRequirementRelationType.Verifies));
  }

  #endregion

  #region Technical Data Structure Tests

  [Test]
  [Category("HappyPath")]
  public void DiagramRack_Properties_CanBeSet() {
    var rack = new DiagramRack { Id = "rack1", Name = "Server Rack 1", TotalUnits = 42 };

    Assert.That(rack.Id, Is.EqualTo("rack1"));
    Assert.That(rack.TotalUnits, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramRackDevice_Properties_CanBeSet() {
    var device = new DiagramRackDevice {
      Id = "server1",
      Name = "Web Server",
      RackId = "rack1",
      DeviceType = "Server",
      StartUnit = 5,
      UnitHeight = 2,
      Color = Color.Blue
    };

    Assert.That(device.StartUnit, Is.EqualTo(5));
    Assert.That(device.UnitHeight, Is.EqualTo(2));
    Assert.That(device.DeviceType, Is.EqualTo("Server"));
  }

  [Test]
  [Category("HappyPath")]
  public void NetworkLayout_AllTypes() {
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Star));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Ring));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Mesh));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Bus));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Tree));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.Hybrid));
    Assert.That(Enum.GetValues(typeof(NetworkLayout)), Has.Member(NetworkLayout.FullMesh));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramPacketField_Properties_CanBeSet() {
    var field = new DiagramPacketField {
      Name = "Version",
      Bits = 4,
      Value = "4",
      Color = Color.LightGray
    };

    Assert.That(field.Name, Is.EqualTo("Version"));
    Assert.That(field.Bits, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramByteField_Properties_CanBeSet() {
    var field = new DiagramByteField {
      StartBit = 0,
      EndBit = 7,
      Name = "Header",
      Value = "0xFF",
      Color = Color.LightBlue
    };

    Assert.That(field.StartBit, Is.EqualTo(0));
    Assert.That(field.EndBit, Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSignal_Properties_CanBeSet() {
    var signal = new DiagramSignal {
      Id = "clk",
      Name = "Clock",
      SignalType = DiagramSignalType.Clock,
      Color = Color.Green
    };
    signal.Transitions.Add(new DiagramSignalTransition { Time = 0, Level = DiagramSignalLevel.Low });
    signal.Transitions.Add(new DiagramSignalTransition { Time = 10, Level = DiagramSignalLevel.High });

    Assert.That(signal.Id, Is.EqualTo("clk"));
    Assert.That(signal.SignalType, Is.EqualTo(DiagramSignalType.Clock));
    Assert.That(signal.Transitions.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSignalType_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramSignalType)), Has.Member(DiagramSignalType.Digital));
    Assert.That(Enum.GetValues(typeof(DiagramSignalType)), Has.Member(DiagramSignalType.Bus));
    Assert.That(Enum.GetValues(typeof(DiagramSignalType)), Has.Member(DiagramSignalType.Analog));
    Assert.That(Enum.GetValues(typeof(DiagramSignalType)), Has.Member(DiagramSignalType.Clock));
  }

  [Test]
  [Category("HappyPath")]
  public void DiagramSignalLevel_AllTypes() {
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.Low));
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.High));
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.HighZ));
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.Unknown));
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.Rising));
    Assert.That(Enum.GetValues(typeof(DiagramSignalLevel)), Has.Member(DiagramSignalLevel.Falling));
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
