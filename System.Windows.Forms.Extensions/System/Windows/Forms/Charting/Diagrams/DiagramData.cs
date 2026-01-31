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
public class DiagramNodeCollection : List<DiagramNode> {
  private readonly DiagramControl _owner;

  internal DiagramNodeCollection(DiagramControl owner) => this._owner = owner;

  public new void Add(DiagramNode node) {
    base.Add(node);
    this._owner?.Invalidate();
  }

  public new bool Remove(DiagramNode node) {
    var result = base.Remove(node);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramNode> nodes) {
    base.AddRange(nodes);
    this._owner?.Invalidate();
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
public class DiagramEdgeCollection : List<DiagramEdge> {
  private readonly DiagramControl _owner;

  internal DiagramEdgeCollection(DiagramControl owner) => this._owner = owner;

  public new void Add(DiagramEdge edge) {
    base.Add(edge);
    this._owner?.Invalidate();
  }

  public new bool Remove(DiagramEdge edge) {
    var result = base.Remove(edge);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramEdge> edges) {
    base.AddRange(edges);
    this._owner?.Invalidate();
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
public class DiagramSankeyLinkCollection : List<DiagramSankeyLink> {
  private readonly DiagramControl _owner;

  internal DiagramSankeyLinkCollection(DiagramControl owner) => this._owner = owner;

  public new void Add(DiagramSankeyLink link) {
    base.Add(link);
    this._owner?.Invalidate();
  }

  public new bool Remove(DiagramSankeyLink link) {
    var result = base.Remove(link);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramSankeyLink> links) {
    base.AddRange(links);
    this._owner?.Invalidate();
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
public class DiagramHierarchyNodeCollection : List<DiagramHierarchyNode> {
  private readonly DiagramControl _owner;

  internal DiagramHierarchyNodeCollection(DiagramControl owner) => this._owner = owner;

  public new void Add(DiagramHierarchyNode node) {
    base.Add(node);
    this._owner?.Invalidate();
  }

  public new bool Remove(DiagramHierarchyNode node) {
    var result = base.Remove(node);
    if (result)
      this._owner?.Invalidate();
    return result;
  }

  public new void Clear() {
    base.Clear();
    this._owner?.Invalidate();
  }

  public new void AddRange(IEnumerable<DiagramHierarchyNode> nodes) {
    base.AddRange(nodes);
    this._owner?.Invalidate();
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
