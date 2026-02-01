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

namespace System.Windows.Forms.Charting.Diagrams;

#region Node Data

/// <summary>
/// Represents a node in a network or flow diagram.
/// </summary>
public class DiagramNode {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the display label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the node size.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets the node color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the node position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the group/category this node belongs to.</summary>
  public string Group { get; set; }

  /// <summary>Gets or sets the node shape for flow charts.</summary>
  public DiagramNodeShape Shape { get; set; } = DiagramNodeShape.RoundedRectangle;

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramNode() { }

  public DiagramNode(string id, string label = null, double size = 1) {
    this.Id = id;
    this.Label = label ?? id;
    this.Size = size;
  }
}

/// <summary>
/// Collection of diagram nodes.
/// </summary>
public class DiagramNodeCollection(DiagramControl owner) : List<DiagramNode> {
  public new void Add(DiagramNode node) {
    base.Add(node);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramNode node) {
    var result = base.Remove(node);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramNode> nodes) {
    base.AddRange(nodes);
    owner?.Invalidate();
  }
}

#endregion

#region Edge Data

/// <summary>
/// Represents an edge/link between nodes in a network.
/// </summary>
public class DiagramEdge {
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

  public DiagramEdge() { }

  public DiagramEdge(string source, string target, double weight = 1, bool directed = false) {
    this.Source = source;
    this.Target = target;
    this.Weight = weight;
    this.Directed = directed;
  }
}

/// <summary>
/// Collection of diagram edges.
/// </summary>
public class DiagramEdgeCollection(DiagramControl owner) : List<DiagramEdge> {
  public new void Add(DiagramEdge edge) {
    base.Add(edge);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramEdge edge) {
    var result = base.Remove(edge);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramEdge> edges) {
    base.AddRange(edges);
    owner?.Invalidate();
  }
}

#endregion

#region Sankey Data

/// <summary>
/// Represents a link in a Sankey diagram.
/// </summary>
public class DiagramSankeyLink {
  /// <summary>Gets or sets the source node identifier.</summary>
  public string Source { get; set; }

  /// <summary>Gets or sets the target node identifier.</summary>
  public string Target { get; set; }

  /// <summary>Gets or sets the flow value.</summary>
  public double Value { get; set; }

  /// <summary>Gets or sets the link color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets an optional label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramSankeyLink() { }

  public DiagramSankeyLink(string source, string target, double value) {
    this.Source = source;
    this.Target = target;
    this.Value = value;
  }
}

/// <summary>
/// Collection of Sankey links.
/// </summary>
public class DiagramSankeyLinkCollection(DiagramControl owner) : List<DiagramSankeyLink> {
  public new void Add(DiagramSankeyLink link) {
    base.Add(link);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramSankeyLink link) {
    var result = base.Remove(link);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramSankeyLink> links) {
    base.AddRange(links);
    owner?.Invalidate();
  }
}

#endregion

#region Hierarchical Data

/// <summary>
/// Represents a node in a hierarchical data structure (for tree diagrams, dendrograms, circle packing).
/// </summary>
public class DiagramHierarchyNode {
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

  /// <summary>Gets or sets child nodes (built from flat list).</summary>
  public List<DiagramHierarchyNode> Children { get; set; } = new();

  /// <summary>Gets the depth level in the hierarchy.</summary>
  public int Depth { get; internal set; }

  public DiagramHierarchyNode() { }

  public DiagramHierarchyNode(string id, string label, double value, string parentId = null) {
    this.Id = id;
    this.Label = label;
    this.Value = value;
    this.ParentId = parentId;
  }
}

/// <summary>
/// Collection of hierarchical diagram nodes.
/// </summary>
public class DiagramHierarchyNodeCollection(DiagramControl owner) : List<DiagramHierarchyNode> {
  public new void Add(DiagramHierarchyNode node) {
    base.Add(node);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramHierarchyNode node) {
    var result = base.Remove(node);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramHierarchyNode> nodes) {
    base.AddRange(nodes);
    owner?.Invalidate();
  }

  /// <summary>
  /// Builds the tree structure from a flat list with parent references.
  /// </summary>
  public DiagramHierarchyNode BuildTree() {
    var lookup = new Dictionary<string, DiagramHierarchyNode>();
    DiagramHierarchyNode root = null;

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

#region Wrapper Classes

/// <summary>
/// Wraps diagram nodes and edges for network-based diagrams.
/// </summary>
public class DiagramNetworkData {
  /// <summary>Gets the network nodes.</summary>
  public DiagramNodeCollection Nodes { get; }

  /// <summary>Gets the network edges.</summary>
  public DiagramEdgeCollection Edges { get; }

  internal DiagramNetworkData(DiagramNodeCollection nodes, DiagramEdgeCollection edges) {
    this.Nodes = nodes;
    this.Edges = edges;
  }
}

#endregion

#region UML Class/Object Diagram Data

/// <summary>
/// Represents a class in a UML class diagram.
/// </summary>
public class DiagramClassNode {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the class name.</summary>
  public string ClassName { get; set; }

  /// <summary>Gets or sets the stereotype (interface, abstract, enum, etc.).</summary>
  public string Stereotype { get; set; }

  /// <summary>Gets or sets the list of fields.</summary>
  public List<DiagramClassMember> Fields { get; set; } = new();

  /// <summary>Gets or sets the list of methods.</summary>
  public List<DiagramClassMember> Methods { get; set; } = new();

  /// <summary>Gets or sets the node color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramClassNode() { }

  public DiagramClassNode(string id, string className, string stereotype = null) {
    this.Id = id;
    this.ClassName = className;
    this.Stereotype = stereotype;
  }
}

/// <summary>
/// Represents a member (field or method) of a UML class.
/// </summary>
public class DiagramClassMember {
  /// <summary>Gets or sets the member name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the type/return type.</summary>
  public string Type { get; set; }

  /// <summary>Gets or sets the visibility.</summary>
  public DiagramVisibility Visibility { get; set; } = DiagramVisibility.Public;

  /// <summary>Gets or sets whether the member is static.</summary>
  public bool IsStatic { get; set; }

  /// <summary>Gets or sets whether the member is abstract.</summary>
  public bool IsAbstract { get; set; }

  /// <summary>Gets or sets the parameters (for methods).</summary>
  public string Parameters { get; set; }

  public DiagramClassMember() { }

  public DiagramClassMember(string name, string type, DiagramVisibility visibility = DiagramVisibility.Public) {
    this.Name = name;
    this.Type = type;
    this.Visibility = visibility;
  }
}

/// <summary>
/// Represents a relationship between classes in a UML class diagram.
/// </summary>
public class DiagramClassRelation {
  /// <summary>Gets or sets the source class ID.</summary>
  public string From { get; set; }

  /// <summary>Gets or sets the target class ID.</summary>
  public string To { get; set; }

  /// <summary>Gets or sets the relationship type.</summary>
  public DiagramRelationType RelationType { get; set; }

  /// <summary>Gets or sets the label on the relationship.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the multiplicity at the source end.</summary>
  public string FromMultiplicity { get; set; }

  /// <summary>Gets or sets the multiplicity at the target end.</summary>
  public string ToMultiplicity { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramClassRelation() { }

  public DiagramClassRelation(string from, string to, DiagramRelationType relationType) {
    this.From = from;
    this.To = to;
    this.RelationType = relationType;
  }
}

/// <summary>
/// Collection of UML class nodes.
/// </summary>
public class DiagramClassNodeCollection(DiagramControl owner) : List<DiagramClassNode> {
  public new void Add(DiagramClassNode node) {
    base.Add(node);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramClassNode node) {
    var result = base.Remove(node);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramClassNode> nodes) {
    base.AddRange(nodes);
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of UML class relations.
/// </summary>
public class DiagramClassRelationCollection(DiagramControl owner) : List<DiagramClassRelation> {
  public new void Add(DiagramClassRelation relation) {
    base.Add(relation);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramClassRelation relation) {
    var result = base.Remove(relation);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramClassRelation> relations) {
    base.AddRange(relations);
    owner?.Invalidate();
  }
}

#endregion

#region UML Sequence Diagram Data

/// <summary>
/// Represents a lifeline in a UML sequence diagram.
/// </summary>
public class DiagramLifeline {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the lifeline name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the stereotype (actor, boundary, control, entity).</summary>
  public string Stereotype { get; set; }

  /// <summary>Gets or sets the lifeline color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the order (left to right position).</summary>
  public int Order { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramLifeline() { }

  public DiagramLifeline(string id, string name, string stereotype = null) {
    this.Id = id;
    this.Name = name;
    this.Stereotype = stereotype;
  }
}

/// <summary>
/// Represents a message in a UML sequence diagram.
/// </summary>
public class DiagramMessage {
  /// <summary>Gets or sets the source lifeline ID.</summary>
  public string From { get; set; }

  /// <summary>Gets or sets the target lifeline ID.</summary>
  public string To { get; set; }

  /// <summary>Gets or sets the message label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the message type.</summary>
  public DiagramMessageType MessageType { get; set; } = DiagramMessageType.Sync;

  /// <summary>Gets or sets the sequence number.</summary>
  public int SequenceNumber { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramMessage() { }

  public DiagramMessage(string from, string to, string label, DiagramMessageType messageType = DiagramMessageType.Sync) {
    this.From = from;
    this.To = to;
    this.Label = label;
    this.MessageType = messageType;
  }
}

/// <summary>
/// Represents an activation (execution specification) in a sequence diagram.
/// </summary>
public class DiagramActivation {
  /// <summary>Gets or sets the lifeline ID.</summary>
  public string LifelineId { get; set; }

  /// <summary>Gets or sets the starting message sequence number.</summary>
  public int StartMessage { get; set; }

  /// <summary>Gets or sets the ending message sequence number.</summary>
  public int EndMessage { get; set; }

  public DiagramActivation() { }

  public DiagramActivation(string lifelineId, int startMessage, int endMessage) {
    this.LifelineId = lifelineId;
    this.StartMessage = startMessage;
    this.EndMessage = endMessage;
  }
}

/// <summary>
/// Collection of activations.
/// </summary>
public class DiagramActivationCollection(DiagramControl owner) : List<DiagramActivation> {
  public new void Add(DiagramActivation activation) {
    base.Add(activation);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramActivation activation) {
    var result = base.Remove(activation);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of lifelines.
/// </summary>
public class DiagramLifelineCollection(DiagramControl owner) : List<DiagramLifeline> {
  public new void Add(DiagramLifeline lifeline) {
    base.Add(lifeline);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramLifeline lifeline) {
    var result = base.Remove(lifeline);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of messages.
/// </summary>
public class DiagramMessageCollection(DiagramControl owner) : List<DiagramMessage> {
  public new void Add(DiagramMessage message) {
    base.Add(message);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramMessage message) {
    var result = base.Remove(message);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region UML Use Case Diagram Data

/// <summary>
/// Represents an actor in a use case diagram.
/// </summary>
public class DiagramActor {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the actor name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets whether this is a system actor.</summary>
  public bool IsSystem { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramActor() { }

  public DiagramActor(string id, string name, bool isSystem = false) {
    this.Id = id;
    this.Name = name;
    this.IsSystem = isSystem;
  }
}

/// <summary>
/// Represents a use case in a use case diagram.
/// </summary>
public class DiagramUseCase {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the use case name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the system boundary this use case belongs to.</summary>
  public string SystemBoundary { get; set; }

  /// <summary>Gets or sets the list of extension points.</summary>
  public List<string> ExtensionPoints { get; set; } = new();

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramUseCase() { }

  public DiagramUseCase(string id, string name, string systemBoundary = null) {
    this.Id = id;
    this.Name = name;
    this.SystemBoundary = systemBoundary;
  }
}

/// <summary>
/// Collection of actors.
/// </summary>
public class DiagramActorCollection(DiagramControl owner) : List<DiagramActor> {
  public new void Add(DiagramActor actor) {
    base.Add(actor);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramActor actor) {
    var result = base.Remove(actor);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of use cases.
/// </summary>
public class DiagramUseCaseCollection(DiagramControl owner) : List<DiagramUseCase> {
  public new void Add(DiagramUseCase useCase) {
    base.Add(useCase);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramUseCase useCase) {
    var result = base.Remove(useCase);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region UML Activity/Component/Deployment Data

/// <summary>
/// Represents a swimlane in an activity diagram.
/// </summary>
public class DiagramSwimlane {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the swimlane name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the swimlane color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the order (left to right position).</summary>
  public int Order { get; set; }

  public DiagramSwimlane() { }

  public DiagramSwimlane(string id, string name) {
    this.Id = id;
    this.Name = name;
  }
}

/// <summary>
/// Represents a component in a component diagram.
/// </summary>
public class DiagramComponent {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the component name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the stereotype.</summary>
  public string Stereotype { get; set; }

  /// <summary>Gets or sets the list of provided interfaces.</summary>
  public List<string> ProvidedInterfaces { get; set; } = new();

  /// <summary>Gets or sets the list of required interfaces.</summary>
  public List<string> RequiredInterfaces { get; set; } = new();

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the component color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramComponent() { }

  public DiagramComponent(string id, string name) {
    this.Id = id;
    this.Name = name;
  }
}

/// <summary>
/// Represents a deployment node in a deployment diagram.
/// </summary>
public class DiagramDeploymentNode {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the node name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the stereotype (device, executionEnvironment).</summary>
  public string Stereotype { get; set; }

  /// <summary>Gets or sets the parent node ID (for nesting).</summary>
  public string ParentId { get; set; }

  /// <summary>Gets or sets the list of artifact IDs deployed on this node.</summary>
  public List<string> ArtifactIds { get; set; } = new();

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the node color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramDeploymentNode() { }

  public DiagramDeploymentNode(string id, string name, string stereotype = "device") {
    this.Id = id;
    this.Name = name;
    this.Stereotype = stereotype;
  }
}

/// <summary>
/// Represents a package in a package diagram.
/// </summary>
public class DiagramPackage {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the package name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the stereotype.</summary>
  public string Stereotype { get; set; }

  /// <summary>Gets or sets the parent package ID.</summary>
  public string ParentId { get; set; }

  /// <summary>Gets or sets the list of contained element IDs.</summary>
  public List<string> ContainedElements { get; set; } = new();

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the package color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramPackage() { }

  public DiagramPackage(string id, string name) {
    this.Id = id;
    this.Name = name;
  }
}

/// <summary>
/// Collection of deployment nodes.
/// </summary>
public class DiagramDeploymentNodeCollection(DiagramControl owner) : List<DiagramDeploymentNode> {
  public new void Add(DiagramDeploymentNode node) {
    base.Add(node);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramDeploymentNode node) {
    var result = base.Remove(node);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of packages.
/// </summary>
public class DiagramPackageCollection(DiagramControl owner) : List<DiagramPackage> {
  public new void Add(DiagramPackage package) {
    base.Add(package);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramPackage package) {
    var result = base.Remove(package);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of swimlanes.
/// </summary>
public class DiagramSwimlaneCollection(DiagramControl owner) : List<DiagramSwimlane> {
  public new void Add(DiagramSwimlane swimlane) {
    base.Add(swimlane);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramSwimlane swimlane) {
    var result = base.Remove(swimlane);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of components.
/// </summary>
public class DiagramComponentCollection(DiagramControl owner) : List<DiagramComponent> {
  public new void Add(DiagramComponent component) {
    base.Add(component);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramComponent component) {
    var result = base.Remove(component);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region Database/Data Diagram Data

/// <summary>
/// Represents an entity in an ER diagram.
/// </summary>
public class DiagramEntity {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the entity name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the list of attributes.</summary>
  public List<DiagramEntityAttribute> Attributes { get; set; } = new();

  /// <summary>Gets or sets the entity color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramEntity() { }

  public DiagramEntity(string id, string name) {
    this.Id = id;
    this.Name = name;
  }
}

/// <summary>
/// Represents an attribute of an entity in an ER diagram.
/// </summary>
public class DiagramEntityAttribute {
  /// <summary>Gets or sets the attribute name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the data type.</summary>
  public string DataType { get; set; }

  /// <summary>Gets or sets whether this is a primary key.</summary>
  public bool IsPrimaryKey { get; set; }

  /// <summary>Gets or sets whether this is a foreign key.</summary>
  public bool IsForeignKey { get; set; }

  /// <summary>Gets or sets whether this attribute is nullable.</summary>
  public bool IsNullable { get; set; } = true;

  /// <summary>Gets or sets whether this attribute is unique.</summary>
  public bool IsUnique { get; set; }

  public DiagramEntityAttribute() { }

  public DiagramEntityAttribute(string name, string dataType, bool isPrimaryKey = false) {
    this.Name = name;
    this.DataType = dataType;
    this.IsPrimaryKey = isPrimaryKey;
  }
}

/// <summary>
/// Represents a relationship between entities in an ER diagram.
/// </summary>
public class DiagramRelationship {
  /// <summary>Gets or sets the source entity ID.</summary>
  public string From { get; set; }

  /// <summary>Gets or sets the target entity ID.</summary>
  public string To { get; set; }

  /// <summary>Gets or sets the relationship label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the cardinality at the source end.</summary>
  public DiagramCardinality FromCardinality { get; set; } = DiagramCardinality.One;

  /// <summary>Gets or sets the cardinality at the target end.</summary>
  public DiagramCardinality ToCardinality { get; set; } = DiagramCardinality.Many;

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramRelationship() { }

  public DiagramRelationship(string from, string to, DiagramCardinality fromCard = DiagramCardinality.One, DiagramCardinality toCard = DiagramCardinality.Many) {
    this.From = from;
    this.To = to;
    this.FromCardinality = fromCard;
    this.ToCardinality = toCard;
  }
}

/// <summary>
/// Represents an element in a data flow diagram.
/// </summary>
public class DiagramDataFlowElement {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the element name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the element type.</summary>
  public DiagramDFDType ElementType { get; set; }

  /// <summary>Gets or sets the process number (for processes).</summary>
  public int ProcessNumber { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the element color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramDataFlowElement() { }

  public DiagramDataFlowElement(string id, string name, DiagramDFDType elementType) {
    this.Id = id;
    this.Name = name;
    this.ElementType = elementType;
  }
}

/// <summary>
/// Collection of entities.
/// </summary>
public class DiagramEntityCollection(DiagramControl owner) : List<DiagramEntity> {
  public new void Add(DiagramEntity entity) {
    base.Add(entity);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramEntity entity) {
    var result = base.Remove(entity);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of ER relationships.
/// </summary>
public class DiagramRelationshipCollection(DiagramControl owner) : List<DiagramRelationship> {
  public new void Add(DiagramRelationship relationship) {
    base.Add(relationship);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramRelationship relationship) {
    var result = base.Remove(relationship);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of data flow elements.
/// </summary>
public class DiagramDataFlowElementCollection(DiagramControl owner) : List<DiagramDataFlowElement> {
  public new void Add(DiagramDataFlowElement element) {
    base.Add(element);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramDataFlowElement element) {
    var result = base.Remove(element);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region Business/Analytical Diagram Data

/// <summary>
/// Represents a set in a Venn diagram.
/// </summary>
public class DiagramSet {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the relative size.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets the set color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramSet() { }

  public DiagramSet(string id, string label, double size = 1) {
    this.Id = id;
    this.Label = label;
    this.Size = size;
  }
}

/// <summary>
/// Represents an intersection between sets in a Venn diagram.
/// </summary>
public class DiagramSetIntersection {
  /// <summary>Gets or sets the list of set IDs that form this intersection.</summary>
  public List<string> SetIds { get; set; } = new();

  /// <summary>Gets or sets the label for this intersection.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the value/count for this intersection.</summary>
  public double Value { get; set; }

  public DiagramSetIntersection() { }

  public DiagramSetIntersection(IEnumerable<string> setIds, double value, string label = null) {
    this.SetIds = new List<string>(setIds);
    this.Value = value;
    this.Label = label;
  }
}

/// <summary>
/// Represents the quadrant configuration for a matrix/SWOT diagram.
/// </summary>
public class DiagramQuadrant {
  /// <summary>Gets or sets the top-left quadrant label.</summary>
  public string TopLeftLabel { get; set; } = "Top Left";

  /// <summary>Gets or sets the top-right quadrant label.</summary>
  public string TopRightLabel { get; set; } = "Top Right";

  /// <summary>Gets or sets the bottom-left quadrant label.</summary>
  public string BottomLeftLabel { get; set; } = "Bottom Left";

  /// <summary>Gets or sets the bottom-right quadrant label.</summary>
  public string BottomRightLabel { get; set; } = "Bottom Right";

  /// <summary>Gets or sets the X-axis label.</summary>
  public string XAxisLabel { get; set; }

  /// <summary>Gets or sets the Y-axis label.</summary>
  public string YAxisLabel { get; set; }

  /// <summary>Gets or sets the top-left quadrant color.</summary>
  public Color? TopLeftColor { get; set; }

  /// <summary>Gets or sets the top-right quadrant color.</summary>
  public Color? TopRightColor { get; set; }

  /// <summary>Gets or sets the bottom-left quadrant color.</summary>
  public Color? BottomLeftColor { get; set; }

  /// <summary>Gets or sets the bottom-right quadrant color.</summary>
  public Color? BottomRightColor { get; set; }
}

/// <summary>
/// Represents an item in a matrix diagram.
/// </summary>
public class DiagramMatrixItem {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the X position (0-100).</summary>
  public double X { get; set; }

  /// <summary>Gets or sets the Y position (0-100).</summary>
  public double Y { get; set; }

  /// <summary>Gets or sets the size.</summary>
  public double Size { get; set; } = 1;

  /// <summary>Gets or sets the item color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramMatrixItem() { }

  public DiagramMatrixItem(string id, string label, double x, double y) {
    this.Id = id;
    this.Label = label;
    this.X = x;
    this.Y = y;
  }
}

/// <summary>
/// Represents a stage in a user journey map.
/// </summary>
public class DiagramJourneyStage {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the stage label.</summary>
  public string Label { get; set; }

  /// <summary>Gets or sets the stage order.</summary>
  public int Order { get; set; }

  /// <summary>Gets or sets the list of actions in this stage.</summary>
  public List<DiagramJourneyAction> Actions { get; set; } = new();

  public DiagramJourneyStage() { }

  public DiagramJourneyStage(string id, string label, int order) {
    this.Id = id;
    this.Label = label;
    this.Order = order;
  }
}

/// <summary>
/// Represents an action in a user journey stage.
/// </summary>
public class DiagramJourneyAction {
  /// <summary>Gets or sets the actor performing the action.</summary>
  public string Actor { get; set; }

  /// <summary>Gets or sets the action description.</summary>
  public string Action { get; set; }

  /// <summary>Gets or sets the satisfaction score (-2 to +2).</summary>
  public int Score { get; set; }

  public DiagramJourneyAction() { }

  public DiagramJourneyAction(string actor, string action, int score = 0) {
    this.Actor = actor;
    this.Action = action;
    this.Score = score;
  }
}

/// <summary>
/// Represents a BPMN element.
/// </summary>
public class DiagramBPMNElement {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the element name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the element type.</summary>
  public DiagramBPMNType ElementType { get; set; }

  /// <summary>Gets or sets the lane ID this element belongs to.</summary>
  public string LaneId { get; set; }

  /// <summary>Gets or sets the gateway type (if ElementType is Gateway).</summary>
  public DiagramGatewayType? GatewayType { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the element color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramBPMNElement() { }

  public DiagramBPMNElement(string id, string name, DiagramBPMNType elementType) {
    this.Id = id;
    this.Name = name;
    this.ElementType = elementType;
  }
}

/// <summary>
/// Represents a column in a Kanban board.
/// </summary>
public class DiagramKanbanColumn {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the column name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the WIP limit (0 = unlimited).</summary>
  public int WipLimit { get; set; }

  /// <summary>Gets or sets the column order.</summary>
  public int Order { get; set; }

  /// <summary>Gets or sets the column color.</summary>
  public Color? Color { get; set; }

  public DiagramKanbanColumn() { }

  public DiagramKanbanColumn(string id, string name, int order, int wipLimit = 0) {
    this.Id = id;
    this.Name = name;
    this.Order = order;
    this.WipLimit = wipLimit;
  }
}

/// <summary>
/// Represents a card in a Kanban board.
/// </summary>
public class DiagramKanbanCard {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the card title.</summary>
  public string Title { get; set; }

  /// <summary>Gets or sets the column ID this card belongs to.</summary>
  public string ColumnId { get; set; }

  /// <summary>Gets or sets the assignee.</summary>
  public string Assignee { get; set; }

  /// <summary>Gets or sets the card color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the card order within its column.</summary>
  public int Order { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramKanbanCard() { }

  public DiagramKanbanCard(string id, string title, string columnId) {
    this.Id = id;
    this.Title = title;
    this.ColumnId = columnId;
  }
}

/// <summary>
/// Collection of sets.
/// </summary>
public class DiagramSetCollection(DiagramControl owner) : List<DiagramSet> {
  public new void Add(DiagramSet set) {
    base.Add(set);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramSet set) {
    var result = base.Remove(set);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of set intersections.
/// </summary>
public class DiagramSetIntersectionCollection(DiagramControl owner) : List<DiagramSetIntersection> {
  public new void Add(DiagramSetIntersection intersection) {
    base.Add(intersection);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramSetIntersection intersection) {
    var result = base.Remove(intersection);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of quadrants.
/// </summary>
public class DiagramQuadrantCollection(DiagramControl owner) : List<DiagramQuadrant> {
  public new void Add(DiagramQuadrant quadrant) {
    base.Add(quadrant);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramQuadrant quadrant) {
    var result = base.Remove(quadrant);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of matrix items.
/// </summary>
public class DiagramMatrixItemCollection(DiagramControl owner) : List<DiagramMatrixItem> {
  public new void Add(DiagramMatrixItem item) {
    base.Add(item);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramMatrixItem item) {
    var result = base.Remove(item);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of journey stages.
/// </summary>
public class DiagramJourneyStageCollection(DiagramControl owner) : List<DiagramJourneyStage> {
  public new void Add(DiagramJourneyStage stage) {
    base.Add(stage);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramJourneyStage stage) {
    var result = base.Remove(stage);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of BPMN elements.
/// </summary>
public class DiagramBPMNElementCollection(DiagramControl owner) : List<DiagramBPMNElement> {
  public new void Add(DiagramBPMNElement element) {
    base.Add(element);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramBPMNElement element) {
    var result = base.Remove(element);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of Kanban columns.
/// </summary>
public class DiagramKanbanColumnCollection(DiagramControl owner) : List<DiagramKanbanColumn> {
  public new void Add(DiagramKanbanColumn column) {
    base.Add(column);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramKanbanColumn column) {
    var result = base.Remove(column);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of Kanban cards.
/// </summary>
public class DiagramKanbanCardCollection(DiagramControl owner) : List<DiagramKanbanCard> {
  public new void Add(DiagramKanbanCard card) {
    base.Add(card);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramKanbanCard card) {
    var result = base.Remove(card);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region Architecture Diagram Data

/// <summary>
/// Represents a git commit in a git graph.
/// </summary>
public class DiagramGitCommit {
  /// <summary>Gets or sets the unique identifier (short hash).</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the commit message.</summary>
  public string Message { get; set; }

  /// <summary>Gets or sets the branch name.</summary>
  public string Branch { get; set; }

  /// <summary>Gets or sets the list of parent commit IDs.</summary>
  public List<string> ParentIds { get; set; } = new();

  /// <summary>Gets or sets the commit type.</summary>
  public DiagramGitCommitType Type { get; set; } = DiagramGitCommitType.Normal;

  /// <summary>Gets or sets the tag name (if Type is Tag).</summary>
  public string TagName { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramGitCommit() { }

  public DiagramGitCommit(string id, string message, string branch) {
    this.Id = id;
    this.Message = message;
    this.Branch = branch;
  }
}

/// <summary>
/// Represents a git branch.
/// </summary>
public class DiagramGitBranch {
  /// <summary>Gets or sets the branch name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the branch color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the branch order (vertical position).</summary>
  public int Order { get; set; }

  public DiagramGitBranch() { }

  public DiagramGitBranch(string name, Color? color = null) {
    this.Name = name;
    this.Color = color;
  }
}

/// <summary>
/// Represents a requirement in a requirements diagram.
/// </summary>
public class DiagramRequirement {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the requirement name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the requirement text.</summary>
  public string Text { get; set; }

  /// <summary>Gets or sets the requirement type.</summary>
  public DiagramRequirementType Type { get; set; } = DiagramRequirementType.Requirement;

  /// <summary>Gets or sets the risk level.</summary>
  public string Risk { get; set; }

  /// <summary>Gets or sets the verification method.</summary>
  public string VerifyMethod { get; set; }

  /// <summary>Gets or sets the position (0-100 normalized coordinates).</summary>
  public PointF Position { get; set; }

  /// <summary>Gets or sets the requirement color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramRequirement() { }

  public DiagramRequirement(string id, string name, string text) {
    this.Id = id;
    this.Name = name;
    this.Text = text;
  }
}

/// <summary>
/// Represents a relationship between requirements.
/// </summary>
public class DiagramRequirementRelation {
  /// <summary>Gets or sets the source requirement ID.</summary>
  public string From { get; set; }

  /// <summary>Gets or sets the target requirement ID.</summary>
  public string To { get; set; }

  /// <summary>Gets or sets the relation type.</summary>
  public DiagramRequirementRelationType Type { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramRequirementRelation() { }

  public DiagramRequirementRelation(string from, string to, DiagramRequirementRelationType type) {
    this.From = from;
    this.To = to;
    this.Type = type;
  }
}

/// <summary>
/// Collection of git commits.
/// </summary>
public class DiagramGitCommitCollection(DiagramControl owner) : List<DiagramGitCommit> {
  public new void Add(DiagramGitCommit commit) {
    base.Add(commit);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramGitCommit commit) {
    var result = base.Remove(commit);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of git branches.
/// </summary>
public class DiagramGitBranchCollection(DiagramControl owner) : List<DiagramGitBranch> {
  public new void Add(DiagramGitBranch branch) {
    base.Add(branch);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramGitBranch branch) {
    var result = base.Remove(branch);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of requirements.
/// </summary>
public class DiagramRequirementCollection(DiagramControl owner) : List<DiagramRequirement> {
  public new void Add(DiagramRequirement requirement) {
    base.Add(requirement);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramRequirement requirement) {
    var result = base.Remove(requirement);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of requirement relations.
/// </summary>
public class DiagramRequirementRelationCollection(DiagramControl owner) : List<DiagramRequirementRelation> {
  public new void Add(DiagramRequirementRelation relation) {
    base.Add(relation);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramRequirementRelation relation) {
    var result = base.Remove(relation);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion

#region Technical/Infrastructure Diagram Data

/// <summary>
/// Represents a server rack.
/// </summary>
public class DiagramRack {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the rack name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the total rack units (typically 42U).</summary>
  public int TotalUnits { get; set; } = 42;

  /// <summary>Gets or sets the rack color.</summary>
  public Color? Color { get; set; }

  public DiagramRack() { }

  public DiagramRack(string id, string name, int totalUnits = 42) {
    this.Id = id;
    this.Name = name;
    this.TotalUnits = totalUnits;
  }
}

/// <summary>
/// Represents a device in a server rack.
/// </summary>
public class DiagramRackDevice {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the device name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the rack ID this device belongs to.</summary>
  public string RackId { get; set; }

  /// <summary>Gets or sets the device type (Server, Switch, Storage, PDU).</summary>
  public string DeviceType { get; set; }

  /// <summary>Gets or sets the starting U position (1-based).</summary>
  public int StartUnit { get; set; }

  /// <summary>Gets or sets the height in U units.</summary>
  public int UnitHeight { get; set; } = 1;

  /// <summary>Gets or sets the device color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }

  public DiagramRackDevice() { }

  public DiagramRackDevice(string id, string name, string rackId, int startUnit, int unitHeight = 1) {
    this.Id = id;
    this.Name = name;
    this.RackId = rackId;
    this.StartUnit = startUnit;
    this.UnitHeight = unitHeight;
  }
}

/// <summary>
/// Represents a field in a network packet diagram.
/// </summary>
public class DiagramPacketField {
  /// <summary>Gets or sets the field name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the field size in bits.</summary>
  public int Bits { get; set; }

  /// <summary>Gets or sets the field value (for display).</summary>
  public string Value { get; set; }

  /// <summary>Gets or sets the field color.</summary>
  public Color? Color { get; set; }

  public DiagramPacketField() { }

  public DiagramPacketField(string name, int bits, string value = null) {
    this.Name = name;
    this.Bits = bits;
    this.Value = value;
  }
}

/// <summary>
/// Represents a field in a byte field diagram.
/// </summary>
public class DiagramByteField {
  /// <summary>Gets or sets the starting bit position.</summary>
  public int StartBit { get; set; }

  /// <summary>Gets or sets the ending bit position.</summary>
  public int EndBit { get; set; }

  /// <summary>Gets or sets the field name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the field value.</summary>
  public string Value { get; set; }

  /// <summary>Gets or sets the field color.</summary>
  public Color? Color { get; set; }

  public DiagramByteField() { }

  public DiagramByteField(string name, int startBit, int endBit, string value = null) {
    this.Name = name;
    this.StartBit = startBit;
    this.EndBit = endBit;
    this.Value = value;
  }
}

/// <summary>
/// Represents a signal in a waveform/timing diagram.
/// </summary>
public class DiagramSignal {
  /// <summary>Gets or sets the unique identifier.</summary>
  public string Id { get; set; }

  /// <summary>Gets or sets the signal name.</summary>
  public string Name { get; set; }

  /// <summary>Gets or sets the signal type.</summary>
  public DiagramSignalType SignalType { get; set; } = DiagramSignalType.Digital;

  /// <summary>Gets or sets the list of signal transitions.</summary>
  public List<DiagramSignalTransition> Transitions { get; set; } = new();

  /// <summary>Gets or sets the signal color.</summary>
  public Color? Color { get; set; }

  /// <summary>Gets or sets the signal order (vertical position).</summary>
  public int Order { get; set; }

  public DiagramSignal() { }

  public DiagramSignal(string id, string name, DiagramSignalType signalType = DiagramSignalType.Digital) {
    this.Id = id;
    this.Name = name;
    this.SignalType = signalType;
  }
}

/// <summary>
/// Represents a transition in a signal.
/// </summary>
public class DiagramSignalTransition {
  /// <summary>Gets or sets the time of the transition.</summary>
  public double Time { get; set; }

  /// <summary>Gets or sets the signal level after transition.</summary>
  public DiagramSignalLevel Level { get; set; }

  /// <summary>Gets or sets the data value (for bus signals).</summary>
  public string Data { get; set; }

  public DiagramSignalTransition() { }

  public DiagramSignalTransition(double time, DiagramSignalLevel level, string data = null) {
    this.Time = time;
    this.Level = level;
    this.Data = data;
  }
}

/// <summary>
/// Collection of racks.
/// </summary>
public class DiagramRackCollection(DiagramControl owner) : List<DiagramRack> {
  public new void Add(DiagramRack rack) {
    base.Add(rack);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramRack rack) {
    var result = base.Remove(rack);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of rack devices.
/// </summary>
public class DiagramRackDeviceCollection(DiagramControl owner) : List<DiagramRackDevice> {
  public new void Add(DiagramRackDevice device) {
    base.Add(device);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramRackDevice device) {
    var result = base.Remove(device);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of packet fields.
/// </summary>
public class DiagramPacketFieldCollection(DiagramControl owner) : List<DiagramPacketField> {
  public new void Add(DiagramPacketField field) {
    base.Add(field);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramPacketField field) {
    var result = base.Remove(field);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of byte fields.
/// </summary>
public class DiagramByteFieldCollection(DiagramControl owner) : List<DiagramByteField> {
  public new void Add(DiagramByteField field) {
    base.Add(field);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramByteField field) {
    var result = base.Remove(field);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

/// <summary>
/// Collection of signals.
/// </summary>
public class DiagramSignalCollection(DiagramControl owner) : List<DiagramSignal> {
  public new void Add(DiagramSignal signal) {
    base.Add(signal);
    owner?.Invalidate();
  }

  public new bool Remove(DiagramSignal signal) {
    var result = base.Remove(signal);
    if (result)
      owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    owner?.Invalidate();
  }
}

#endregion
