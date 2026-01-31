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
using System.Drawing.Drawing2D;
using System.Linq;

namespace System.Windows.Forms.Charting.Diagrams.Renderers;

/// <summary>
/// Renderer for Sankey diagrams.
/// </summary>
public class SankeyDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Sankey;

  /// <summary>
  /// Node width.
  /// </summary>
  public float NodeWidth { get; set; } = 20;

  /// <summary>
  /// Padding between nodes.
  /// </summary>
  public float NodePadding { get; set; } = 10;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var sankeyLinks = context.Diagram.SankeyLinks;
    var sankeyNodes = context.Diagram.Nodes;

    if (sankeyLinks == null || sankeyLinks.Count == 0)
      return;

    // Build node set from links if not provided
    var nodeIds = new HashSet<string>();
    foreach (var link in sankeyLinks) {
      nodeIds.Add(link.Source);
      nodeIds.Add(link.Target);
    }

    var nodes = sankeyNodes?.ToDictionary(n => n.Id) ?? new Dictionary<string, DiagramNode>();
    foreach (var id in nodeIds)
      if (!nodes.ContainsKey(id))
        nodes[id] = new DiagramNode { Id = id, Label = id, Color = null };

    // Calculate node levels (columns)
    var nodeLevels = this._CalculateNodeLevels(sankeyLinks, nodes.Keys.ToList());
    var maxLevel = nodeLevels.Values.Max();

    // Calculate node values (sum of incoming/outgoing)
    var nodeValues = new Dictionary<string, double>();
    foreach (var id in nodes.Keys) {
      var incoming = sankeyLinks.Where(l => l.Target == id).Sum(l => l.Value);
      var outgoing = sankeyLinks.Where(l => l.Source == id).Sum(l => l.Value);
      nodeValues[id] = Math.Max(incoming, outgoing);
    }

    // Position nodes
    var levelWidth = (context.PlotArea.Width - this.NodeWidth) / Math.Max(maxLevel, 1);
    var nodePositions = new Dictionary<string, (float X, float Y, float Height)>();

    for (var level = 0; level <= maxLevel; ++level) {
      var levelNodes = nodeLevels.Where(kv => kv.Value == level).Select(kv => kv.Key).ToList();
      var totalValue = levelNodes.Sum(n => nodeValues.ContainsKey(n) ? nodeValues[n] : 0);

      if (totalValue <= 0)
        continue;

      var availableHeight = context.PlotArea.Height - (levelNodes.Count - 1) * this.NodePadding;
      var x = context.PlotArea.Left + level * levelWidth;
      var y = context.PlotArea.Top;

      foreach (var nodeId in levelNodes) {
        var value = nodeValues.ContainsKey(nodeId) ? nodeValues[nodeId] : 0;
        var height = (float)(value / totalValue * availableHeight);
        height = Math.Max(height, 5); // Minimum height

        nodePositions[nodeId] = (x, y, height);
        y += height + this.NodePadding;
      }
    }

    // Draw links first (behind nodes)
    var colors = GetDefaultColors();
    var nodeOutOffsets = nodes.Keys.ToDictionary(k => k, _ => 0f);
    var nodeInOffsets = nodes.Keys.ToDictionary(k => k, _ => 0f);

    for (var i = 0; i < sankeyLinks.Count; ++i) {
      var link = sankeyLinks[i];

      if (!nodePositions.ContainsKey(link.Source) || !nodePositions.ContainsKey(link.Target))
        continue;

      var (sourceX, sourceY, sourceHeight) = nodePositions[link.Source];
      var (targetX, targetY, targetHeight) = nodePositions[link.Target];

      // Calculate link thickness based on value
      var sourceTotal = nodeValues[link.Source];
      var targetTotal = nodeValues[link.Target];

      var sourceLinkHeight = (float)(link.Value / sourceTotal * sourceHeight);
      var targetLinkHeight = (float)(link.Value / targetTotal * targetHeight);

      // Apply animation
      sourceLinkHeight *= (float)context.AnimationProgress;
      targetLinkHeight *= (float)context.AnimationProgress;

      var sourceStartY = sourceY + nodeOutOffsets[link.Source];
      var targetStartY = targetY + nodeInOffsets[link.Target];

      nodeOutOffsets[link.Source] += sourceLinkHeight;
      nodeInOffsets[link.Target] += targetLinkHeight;

      var color = link.Color ?? colors[i % colors.Length];
      color = Color.FromArgb(150, color);

      // Draw curved link
      using var path = new GraphicsPath();
      var startX = sourceX + this.NodeWidth;
      var endX = targetX;

      var cp1 = startX + (endX - startX) * 0.5f;
      var cp2 = startX + (endX - startX) * 0.5f;

      // Top curve
      path.AddBezier(
        startX, sourceStartY,
        cp1, sourceStartY,
        cp2, targetStartY,
        endX, targetStartY
      );

      // Bottom curve (reverse)
      path.AddBezier(
        endX, targetStartY + targetLinkHeight,
        cp2, targetStartY + targetLinkHeight,
        cp1, sourceStartY + sourceLinkHeight,
        startX, sourceStartY + sourceLinkHeight
      );

      path.CloseFigure();

      using (var brush = new SolidBrush(color))
        g.FillPath(brush, path);
    }

    // Draw nodes
    foreach (var (nodeId, (x, y, height)) in nodePositions) {
      var node = nodes[nodeId];
      var color = node.Color ?? Color.FromArgb(52, 73, 94);

      // Apply animation
      var animatedHeight = height * (float)context.AnimationProgress;

      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, x, y, this.NodeWidth, animatedHeight);

      // Draw label
      var label = node.Label ?? nodeId;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(Color.Black);
      var labelX = nodeLevels[nodeId] == 0 ? x - labelSize.Width - 5 : x + this.NodeWidth + 5;
      g.DrawString(label, context.Diagram.Font, labelBrush, labelX, y + (animatedHeight - labelSize.Height) / 2);
    }
  }

  private Dictionary<string, int> _CalculateNodeLevels(IList<DiagramSankeyLink> links, IList<string> nodeIds) {
    var levels = nodeIds.ToDictionary(id => id, _ => 0);
    var changed = true;

    while (changed) {
      changed = false;
      foreach (var link in links) {
        var newLevel = levels[link.Source] + 1;
        if (newLevel > levels[link.Target]) {
          levels[link.Target] = newLevel;
          changed = true;
        }
      }
    }

    return levels;
  }
}

/// <summary>
/// Renderer for chord diagrams.
/// </summary>
public class ChordDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Chord;

  /// <summary>
  /// Gap between groups in degrees.
  /// </summary>
  public float GroupGap { get; set; } = 3;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    // Chord diagram needs a matrix of connections
    var sankeyLinks = context.Diagram.SankeyLinks;
    if (sankeyLinks == null || sankeyLinks.Count == 0)
      return;

    // Get unique nodes
    var nodeIds = new HashSet<string>();
    foreach (var link in sankeyLinks) {
      nodeIds.Add(link.Source);
      nodeIds.Add(link.Target);
    }

    var nodes = nodeIds.ToList();
    var nodeCount = nodes.Count;
    if (nodeCount == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var outerRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;
    var innerRadius = outerRadius * 0.85f;

    // Calculate total value per node
    var nodeValues = nodes.ToDictionary(n => n, n =>
      sankeyLinks.Where(l => l.Source == n || l.Target == n).Sum(l => l.Value)
    );
    var totalValue = nodeValues.Values.Sum();
    if (totalValue <= 0)
      return;

    // Calculate arc angles for each node
    var totalGap = nodeCount * this.GroupGap;
    var availableDegrees = 360 - totalGap;
    var nodeAngles = new Dictionary<string, (float Start, float Sweep)>();
    var currentAngle = 0f;

    var colors = GetDefaultColors();

    for (var i = 0; i < nodes.Count; ++i) {
      var nodeId = nodes[i];
      var sweep = (float)(nodeValues[nodeId] / totalValue * availableDegrees);
      nodeAngles[nodeId] = (currentAngle, sweep);

      // Draw outer arc
      var color = colors[i % colors.Length];

      using (var pen = new Pen(color, (outerRadius - innerRadius))) {
        var arcRadius = (outerRadius + innerRadius) / 2;
        var arcRect = new RectangleF(centerX - arcRadius, centerY - arcRadius, arcRadius * 2, arcRadius * 2);
        g.DrawArc(pen, arcRect, currentAngle - 90, sweep * (float)context.AnimationProgress);
      }

      // Draw label
      var midAngle = currentAngle + sweep / 2 - 90;
      var labelRadius = outerRadius + 15;
      var labelX = centerX + (float)(Math.Cos(midAngle * Math.PI / 180) * labelRadius);
      var labelY = centerY + (float)(Math.Sin(midAngle * Math.PI / 180) * labelRadius);

      using (var brush = new SolidBrush(Color.Black)) {
        var labelSize = g.MeasureString(nodeId, context.Diagram.Font);
        g.DrawString(nodeId, context.Diagram.Font, brush, labelX - labelSize.Width / 2, labelY - labelSize.Height / 2);
      }

      currentAngle += sweep + this.GroupGap;
    }

    // Draw chords (connections)
    var nodeOffsets = nodes.ToDictionary(n => n, _ => 0f);

    foreach (var link in sankeyLinks) {
      if (!nodeAngles.ContainsKey(link.Source) || !nodeAngles.ContainsKey(link.Target))
        continue;

      var (sourceStart, sourceSweep) = nodeAngles[link.Source];
      var (targetStart, targetSweep) = nodeAngles[link.Target];

      var sourceValue = nodeValues[link.Source];
      var targetValue = nodeValues[link.Target];

      var sourceLinkSweep = (float)(link.Value / sourceValue * sourceSweep);
      var targetLinkSweep = (float)(link.Value / targetValue * targetSweep);

      var sourceAngle1 = sourceStart + nodeOffsets[link.Source] - 90;
      var sourceAngle2 = sourceAngle1 + sourceLinkSweep * (float)context.AnimationProgress;
      var targetAngle1 = targetStart + nodeOffsets[link.Target] - 90;
      var targetAngle2 = targetAngle1 + targetLinkSweep * (float)context.AnimationProgress;

      nodeOffsets[link.Source] += sourceLinkSweep;
      nodeOffsets[link.Target] += targetLinkSweep;

      var color = link.Color ?? colors[nodes.IndexOf(link.Source) % colors.Length];
      color = Color.FromArgb(100, color);

      // Draw chord
      using var path = new GraphicsPath();

      // Start arc points
      var s1 = new PointF(
        centerX + (float)(Math.Cos(sourceAngle1 * Math.PI / 180) * innerRadius),
        centerY + (float)(Math.Sin(sourceAngle1 * Math.PI / 180) * innerRadius)
      );
      var s2 = new PointF(
        centerX + (float)(Math.Cos(sourceAngle2 * Math.PI / 180) * innerRadius),
        centerY + (float)(Math.Sin(sourceAngle2 * Math.PI / 180) * innerRadius)
      );

      // Target arc points
      var t1 = new PointF(
        centerX + (float)(Math.Cos(targetAngle1 * Math.PI / 180) * innerRadius),
        centerY + (float)(Math.Sin(targetAngle1 * Math.PI / 180) * innerRadius)
      );
      var t2 = new PointF(
        centerX + (float)(Math.Cos(targetAngle2 * Math.PI / 180) * innerRadius),
        centerY + (float)(Math.Sin(targetAngle2 * Math.PI / 180) * innerRadius)
      );

      // Draw quadratic bezier curves
      path.AddBezier(s1, new PointF(centerX, centerY), new PointF(centerX, centerY), t1);
      path.AddArc(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2, targetAngle1, targetLinkSweep * (float)context.AnimationProgress);
      path.AddBezier(t2, new PointF(centerX, centerY), new PointF(centerX, centerY), s2);
      path.AddArc(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2, sourceAngle2, -sourceLinkSweep * (float)context.AnimationProgress);

      using (var brush = new SolidBrush(color))
        g.FillPath(brush, path);
    }
  }
}

/// <summary>
/// Renderer for arc diagrams.
/// </summary>
public class ArcDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Arc;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var networkEdges = context.Diagram.Edges;
    var networkNodes = context.Diagram.Nodes;

    if (networkEdges == null || networkEdges.Count == 0)
      return;

    // Get unique nodes
    var nodeIds = new HashSet<string>();
    foreach (var edge in networkEdges) {
      nodeIds.Add(edge.Source);
      nodeIds.Add(edge.Target);
    }

    var nodes = nodeIds.ToList();
    var nodeCount = nodes.Count;
    if (nodeCount == 0)
      return;

    // Position nodes along a horizontal line
    var nodeSpacing = context.PlotArea.Width / (nodeCount + 1);
    var axisY = context.PlotArea.Top + context.PlotArea.Height * 0.7f;

    var nodePositions = new Dictionary<string, float>();
    for (var i = 0; i < nodes.Count; ++i)
      nodePositions[nodes[i]] = context.PlotArea.Left + (i + 1) * nodeSpacing;

    var colors = GetDefaultColors();

    // Draw axis line
    using (var pen = new Pen(Color.LightGray, 1))
      g.DrawLine(pen, context.PlotArea.Left, axisY, context.PlotArea.Right, axisY);

    // Draw arcs
    for (var i = 0; i < networkEdges.Count; ++i) {
      var edge = networkEdges[i];

      if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
        continue;

      var x1 = nodePositions[edge.Source];
      var x2 = nodePositions[edge.Target];

      if (Math.Abs(x1 - x2) < 1)
        continue;

      var minX = Math.Min(x1, x2);
      var maxX = Math.Max(x1, x2);
      var width = maxX - minX;
      var height = width / 2 * (float)context.AnimationProgress;

      // Skip drawing if arc is too small (prevents GDI+ exception with zero dimensions)
      if (height < 1 || width < 1)
        continue;

      var color = Color.FromArgb(150, colors[i % colors.Length]);
      var lineWidth = (float)Math.Max(1, Math.Min(5, edge.Weight));

      using var pen = new Pen(color, lineWidth);

      // Draw arc above the axis
      g.DrawArc(pen, minX, axisY - height, width, height * 2, 180, 180);
    }

    // Draw nodes
    var nodeLookup = networkNodes?.ToDictionary(n => n.Id) ?? new Dictionary<string, DiagramNode>();

    for (var i = 0; i < nodes.Count; ++i) {
      var nodeId = nodes[i];
      var x = nodePositions[nodeId];
      var nodeSize = 10f;

      if (nodeLookup.TryGetValue(nodeId, out var node))
        nodeSize = (float)Math.Max(8, Math.Min(20, node.Size));

      var color = nodeLookup.TryGetValue(nodeId, out var n) && n.Color.HasValue
        ? n.Color.Value
        : colors[i % colors.Length];

      // Draw node
      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, x - nodeSize / 2, axisY - nodeSize / 2, nodeSize, nodeSize);

      // Draw label
      var label = nodeLookup.TryGetValue(nodeId, out var nd) ? nd.Label ?? nodeId : nodeId;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush, x - labelSize.Width / 2, axisY + nodeSize / 2 + 5);
    }
  }
}

/// <summary>
/// Renderer for network graphs (force-directed layout).
/// </summary>
public class NetworkDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Network;

  /// <summary>
  /// Minimum node size.
  /// </summary>
  public float MinNodeSize { get; set; } = 10;

  /// <summary>
  /// Maximum node size.
  /// </summary>
  public float MaxNodeSize { get; set; } = 30;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var networkEdges = context.Diagram.Edges;
    var networkNodes = context.Diagram.Nodes;

    if ((networkEdges == null || networkEdges.Count == 0) && (networkNodes == null || networkNodes.Count == 0))
      return;

    // Get all nodes
    var nodeIds = new HashSet<string>();
    if (networkNodes != null)
      foreach (var node in networkNodes)
        nodeIds.Add(node.Id);
    if (networkEdges != null)
      foreach (var edge in networkEdges) {
        nodeIds.Add(edge.Source);
        nodeIds.Add(edge.Target);
      }

    var nodes = nodeIds.ToList();
    var nodeCount = nodes.Count;
    if (nodeCount == 0)
      return;

    var nodeLookup = networkNodes?.ToDictionary(n => n.Id) ?? new Dictionary<string, DiagramNode>();

    // Calculate positions using simple circular layout (or use provided positions)
    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var radius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.7f;

    var nodePositions = new Dictionary<string, PointF>();
    for (var i = 0; i < nodes.Count; ++i) {
      var nodeId = nodes[i];

      // Use provided position if available
      if (nodeLookup.TryGetValue(nodeId, out var node) && node.Position != PointF.Empty) {
        // Map position to plot area (positions are 0-100 percentages)
        nodePositions[nodeId] = new PointF(
          context.PlotArea.Left + node.Position.X / 100f * context.PlotArea.Width,
          context.PlotArea.Top + node.Position.Y / 100f * context.PlotArea.Height
        );
      } else {
        // Circular layout
        var angle = 2 * Math.PI * i / nodeCount - Math.PI / 2;
        nodePositions[nodeId] = new PointF(
          centerX + (float)(Math.Cos(angle) * radius),
          centerY + (float)(Math.Sin(angle) * radius)
        );
      }
    }

    var colors = GetDefaultColors();

    // Draw edges
    if (networkEdges != null) {
      foreach (var edge in networkEdges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var start = nodePositions[edge.Source];
        var end = nodePositions[edge.Target];

        // Apply animation
        end = new PointF(
          start.X + (end.X - start.X) * (float)context.AnimationProgress,
          start.Y + (end.Y - start.Y) * (float)context.AnimationProgress
        );

        var lineWidth = (float)Math.Max(1, Math.Min(5, edge.Weight / 2));
        var color = Color.FromArgb(100, Color.Gray);

        using var pen = new Pen(color, lineWidth);

        if (edge.Directed) {
          pen.EndCap = LineCap.ArrowAnchor;
          pen.CustomEndCap = new AdjustableArrowCap(5, 5);
        }

        g.DrawLine(pen, start, end);
      }
    }

    // Draw nodes
    var maxSize = networkNodes?.Max(n => n.Size) ?? 1;
    var minSize = networkNodes?.Min(n => n.Size) ?? 1;
    var sizeRange = maxSize - minSize;
    if (sizeRange <= 0)
      sizeRange = 1;

    for (var i = 0; i < nodes.Count; ++i) {
      var nodeId = nodes[i];
      var pos = nodePositions[nodeId];

      float nodeSize;
      Color color;
      string label;

      if (nodeLookup.TryGetValue(nodeId, out var node)) {
        var normalizedSize = (node.Size - minSize) / sizeRange;
        nodeSize = this.MinNodeSize + (float)(normalizedSize * (this.MaxNodeSize - this.MinNodeSize));
        color = node.Color ?? colors[i % colors.Length];
        label = node.Label ?? nodeId;
      } else {
        nodeSize = this.MinNodeSize;
        color = colors[i % colors.Length];
        label = nodeId;
      }

      nodeSize *= (float)context.AnimationProgress;

      // Skip drawing if node is too small (prevents GDI+ exceptions)
      if (nodeSize < 1)
        continue;

      // Draw node with gradient
      using (var path = new GraphicsPath()) {
        path.AddEllipse(pos.X - nodeSize / 2, pos.Y - nodeSize / 2, nodeSize, nodeSize);
        using var brush = new PathGradientBrush(path) {
          CenterColor = Lighten(color, 0.3f),
          SurroundColors = new[] { color }
        };
        g.FillPath(brush, path);
      }

      // Draw border
      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawEllipse(pen, pos.X - nodeSize / 2, pos.Y - nodeSize / 2, nodeSize, nodeSize);

      // Draw label
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush, pos.X - labelSize.Width / 2, pos.Y + nodeSize / 2 + 3);
    }
  }
}

/// <summary>
/// Renderer for tree diagrams.
/// </summary>
public class TreeDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Tree;

  /// <summary>
  /// Orientation of the tree.
  /// </summary>
  public TreeOrientation Orientation { get; set; } = TreeOrientation.TopDown;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var hierarchicalData = context.Diagram.HierarchyNodes;
    if (hierarchicalData == null || hierarchicalData.Count == 0)
      return;

    // Find root nodes (no parent or parent not in data)
    var allIds = hierarchicalData.Select(d => d.Id).ToHashSet();
    var roots = hierarchicalData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    // Build tree structure
    var childrenMap = hierarchicalData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(g2 => g2.Key, g2 => g2.ToList());

    // Calculate depth
    var maxDepth = this._CalculateMaxDepth(roots, childrenMap);
    var levelHeight = context.PlotArea.Height / (maxDepth + 1);

    // Position nodes
    var nodePositions = new Dictionary<string, PointF>();
    var leafCount = this._CountLeaves(roots, childrenMap);
    var nodeWidth = context.PlotArea.Width / Math.Max(leafCount, 1);
    var currentLeaf = 0;

    foreach (var root in roots)
      currentLeaf = this._PositionNode(root, childrenMap, nodePositions, context.PlotArea, 0, levelHeight, nodeWidth, ref currentLeaf);

    var colors = GetDefaultColors();

    // Draw connections
    foreach (var node in hierarchicalData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var parentPos = nodePositions[node.ParentId];
      var childPos = nodePositions[node.Id];

      // Apply animation
      childPos = new PointF(
        parentPos.X + (childPos.X - parentPos.X) * (float)context.AnimationProgress,
        parentPos.Y + (childPos.Y - parentPos.Y) * (float)context.AnimationProgress
      );

      using var pen = new Pen(Color.Gray, 1);
      g.DrawLine(pen, parentPos.X, parentPos.Y + 10, childPos.X, childPos.Y - 10);
    }

    // Draw nodes
    var nodeIndex = 0;
    foreach (var node in hierarchicalData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var pos = nodePositions[node.Id];
      var color = node.Color ?? colors[nodeIndex % colors.Length];

      // Draw node circle
      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, pos.X - 10, pos.Y - 10, 20, 20);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        g.DrawEllipse(pen, pos.X - 10, pos.Y - 10, 20, 20);

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush, pos.X - labelSize.Width / 2, pos.Y + 12);

      ++nodeIndex;
    }
  }

  private int _CalculateMaxDepth(IList<DiagramHierarchyNode> nodes, Dictionary<string, List<DiagramHierarchyNode>> childrenMap) {
    var maxDepth = 0;
    foreach (var node in nodes) {
      var depth = 1;
      if (childrenMap.TryGetValue(node.Id, out var children))
        depth += this._CalculateMaxDepth(children, childrenMap);
      maxDepth = Math.Max(maxDepth, depth);
    }
    return maxDepth;
  }

  private int _CountLeaves(IList<DiagramHierarchyNode> nodes, Dictionary<string, List<DiagramHierarchyNode>> childrenMap) {
    var count = 0;
    foreach (var node in nodes) {
      if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0)
        count += this._CountLeaves(children, childrenMap);
      else
        ++count;
    }
    return count;
  }

  private int _PositionNode(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, PointF> positions, RectangleF bounds, int depth, float levelHeight, float nodeWidth, ref int currentLeaf) {
    var y = bounds.Top + depth * levelHeight + levelHeight / 2;

    if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0) {
      var startLeaf = currentLeaf;
      foreach (var child in children)
        currentLeaf = this._PositionNode(child, childrenMap, positions, bounds, depth + 1, levelHeight, nodeWidth, ref currentLeaf);

      // Position at center of children
      var x = bounds.Left + (startLeaf + (currentLeaf - 1)) / 2f * nodeWidth + nodeWidth / 2;
      positions[node.Id] = new PointF(x, y);
    } else {
      // Leaf node
      var x = bounds.Left + currentLeaf * nodeWidth + nodeWidth / 2;
      positions[node.Id] = new PointF(x, y);
      ++currentLeaf;
    }

    return currentLeaf;
  }
}

/// <summary>
/// Renderer for dendrogram charts.
/// </summary>
public class DendrogramDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Dendrogram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    // Dendrogram is similar to tree but with rectangular connections
    var g = context.Graphics;

    var hierarchicalData = context.Diagram.HierarchyNodes;
    if (hierarchicalData == null || hierarchicalData.Count == 0)
      return;

    // Build tree structure
    var allIds = hierarchicalData.Select(d => d.Id).ToHashSet();
    var roots = hierarchicalData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    var childrenMap = hierarchicalData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(g2 => g2.Key, g2 => g2.ToList());

    // Calculate layout
    var leafCount = this._CountLeaves(roots, childrenMap);
    var nodeSpacing = context.PlotArea.Height / Math.Max(leafCount, 1);
    var maxDepth = this._CalculateMaxDepth(roots, childrenMap);
    var depthSpacing = context.PlotArea.Width / (maxDepth + 1);

    var nodePositions = new Dictionary<string, PointF>();
    var currentLeaf = 0f;

    foreach (var root in roots)
      this._PositionDendrogramNode(root, childrenMap, nodePositions, context.PlotArea, 0, depthSpacing, nodeSpacing, ref currentLeaf);

    var colors = GetDefaultColors();

    // Draw connections (rectangular style)
    foreach (var node in hierarchicalData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var parentPos = nodePositions[node.ParentId];
      var childPos = nodePositions[node.Id];

      // Apply animation
      var animatedX = parentPos.X + (childPos.X - parentPos.X) * (float)context.AnimationProgress;

      using var pen = new Pen(Color.Gray, 1);

      // Draw rectangular connection (horizontal then vertical)
      g.DrawLine(pen, parentPos.X, parentPos.Y, animatedX, parentPos.Y);
      g.DrawLine(pen, animatedX, parentPos.Y, animatedX, childPos.Y);
    }

    // Draw nodes (only leaf nodes get circles)
    var nodeIndex = 0;
    foreach (var node in hierarchicalData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var pos = nodePositions[node.Id];
      var isLeaf = !childrenMap.ContainsKey(node.Id) || childrenMap[node.Id].Count == 0;

      if (isLeaf) {
        var color = node.Color ?? colors[nodeIndex % colors.Length];

        // Draw node
        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, pos.X - 5, pos.Y - 5, 10, 10);

        // Draw label
        var label = node.Label ?? node.Id;
        using var labelBrush = new SolidBrush(Color.Black);
        g.DrawString(label, context.Diagram.Font, labelBrush, pos.X + 10, pos.Y - 7);
      }

      ++nodeIndex;
    }
  }

  private int _CalculateMaxDepth(IList<DiagramHierarchyNode> nodes, Dictionary<string, List<DiagramHierarchyNode>> childrenMap) {
    var maxDepth = 0;
    foreach (var node in nodes) {
      var depth = 1;
      if (childrenMap.TryGetValue(node.Id, out var children))
        depth += this._CalculateMaxDepth(children, childrenMap);
      maxDepth = Math.Max(maxDepth, depth);
    }
    return maxDepth;
  }

  private int _CountLeaves(IList<DiagramHierarchyNode> nodes, Dictionary<string, List<DiagramHierarchyNode>> childrenMap) {
    var count = 0;
    foreach (var node in nodes) {
      if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0)
        count += this._CountLeaves(children, childrenMap);
      else
        ++count;
    }
    return count;
  }

  private void _PositionDendrogramNode(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, PointF> positions, RectangleF bounds, int depth, float depthSpacing, float nodeSpacing, ref float currentLeaf) {
    var x = bounds.Left + depth * depthSpacing + depthSpacing / 2;

    if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0) {
      var startLeaf = currentLeaf;
      foreach (var child in children)
        this._PositionDendrogramNode(child, childrenMap, positions, bounds, depth + 1, depthSpacing, nodeSpacing, ref currentLeaf);

      // Position at center of children (Y-axis)
      var y = bounds.Top + (startLeaf + currentLeaf - 1) / 2f * nodeSpacing + nodeSpacing / 2;
      positions[node.Id] = new PointF(x, y);
    } else {
      // Leaf node
      var y = bounds.Top + currentLeaf * nodeSpacing + nodeSpacing / 2;
      positions[node.Id] = new PointF(x, y);
      currentLeaf += 1;
    }
  }
}

/// <summary>
/// Renderer for circle packing charts.
/// </summary>
public class CirclePackingDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.CirclePacking;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var hierarchicalData = context.Diagram.HierarchyNodes;
    if (hierarchicalData == null || hierarchicalData.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var maxRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 * 0.9f;

    // Simple circle packing for flat list (no hierarchy)
    var totalValue = hierarchicalData.Sum(d => Math.Max(0, d.Value));
    if (totalValue <= 0)
      return;

    var sortedData = hierarchicalData.Where(d => d.Value > 0).OrderByDescending(d => d.Value).ToList();
    var colors = GetDefaultColors();

    // Calculate circle sizes
    var circles = sortedData.Select((d, i) => {
      var areaRatio = d.Value / totalValue;
      var radius = (float)Math.Sqrt(areaRatio) * maxRadius * 0.8f;
      return new { Data = d, Radius = radius, Position = PointF.Empty, Color = d.Color ?? colors[i % colors.Length] };
    }).ToList();

    // Position circles using simple spiral packing
    var placedCircles = new List<(PointF Center, float Radius)>();

    for (var i = 0; i < circles.Count; ++i) {
      var circle = circles[i];
      var radius = circle.Radius * (float)context.AnimationProgress;
      PointF position = default;

      if (i == 0) {
        // First circle at center
        position = new PointF(centerX, centerY);
      } else {
        // Find position that doesn't overlap
        var angle = 0.0;
        var spiralRadius = 0.0;
        var placed = false;

        while (!placed && spiralRadius < maxRadius * 2) {
          var testPos = new PointF(
            centerX + (float)(Math.Cos(angle) * spiralRadius),
            centerY + (float)(Math.Sin(angle) * spiralRadius)
          );

          var overlaps = placedCircles.Any(p => {
            var dist = Math.Sqrt(Math.Pow(p.Center.X - testPos.X, 2) + Math.Pow(p.Center.Y - testPos.Y, 2));
            return dist < p.Radius + radius + 2;
          });

          if (!overlaps) {
            position = testPos;
            placed = true;
          } else {
            angle += 0.3;
            spiralRadius += 2;
          }
        }

        if (!placed)
          position = new PointF(centerX + (float)spiralRadius, centerY);
      }

      placedCircles.Add((position, radius));

      // Draw circle
      using (var brush = new SolidBrush(Color.FromArgb(180, circle.Color)))
        g.FillEllipse(brush, position.X - radius, position.Y - radius, radius * 2, radius * 2);

      using (var pen = new Pen(Darken(circle.Color, 0.2f), 1))
        g.DrawEllipse(pen, position.X - radius, position.Y - radius, radius * 2, radius * 2);

      // Draw label if circle is large enough
      if (radius > 20 && !string.IsNullOrEmpty(circle.Data.Label)) {
        var label = circle.Data.Label;
        var labelSize = g.MeasureString(label, context.Diagram.Font);
        if (labelSize.Width < radius * 1.5f) {
          using var labelBrush = new SolidBrush(GetContrastColor(circle.Color));
          g.DrawString(label, context.Diagram.Font, labelBrush, position.X - labelSize.Width / 2, position.Y - labelSize.Height / 2);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for flow chart diagrams (process flow with shapes and connectors).
/// </summary>
public class FlowChartDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.FlowChart;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;

    var networkNodes = context.Diagram.Nodes;
    var networkEdges = context.Diagram.Edges;

    if (networkNodes == null || networkNodes.Count == 0) {
      this._DrawPlaceholder(g, context.PlotArea, "Flow Chart\n(Requires network data)");
      return;
    }

    // Position nodes in a grid-like flow layout
    var nodeCount = networkNodes.Count;
    var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(nodeCount)));
    var rows = (int)Math.Ceiling((double)nodeCount / cols);

    var cellWidth = context.PlotArea.Width / cols;
    var cellHeight = context.PlotArea.Height / rows;
    var nodeWidth = cellWidth * 0.7f;
    var nodeHeight = cellHeight * 0.5f;

    var nodePositions = new Dictionary<string, RectangleF>();

    // Position nodes
    for (var i = 0; i < networkNodes.Count; ++i) {
      var node = networkNodes[i];
      var col = i % cols;
      var row = i / cols;

      var x = context.PlotArea.Left + col * cellWidth + (cellWidth - nodeWidth) / 2;
      var y = context.PlotArea.Top + row * cellHeight + (cellHeight - nodeHeight) / 2;

      nodePositions[node.Id] = new RectangleF(x, y, nodeWidth, nodeHeight);
    }

    // Draw connectors first (behind nodes)
    if (networkEdges != null) {
      foreach (var edge in networkEdges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var sourceRect = nodePositions[edge.Source];
        var targetRect = nodePositions[edge.Target];

        // Calculate connection points
        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Determine best connection points
        PointF startPoint, endPoint;
        if (Math.Abs(targetCenter.Y - sourceCenter.Y) > Math.Abs(targetCenter.X - sourceCenter.X)) {
          // Vertical connection
          startPoint = targetCenter.Y > sourceCenter.Y
            ? new PointF(sourceCenter.X, sourceRect.Bottom)
            : new PointF(sourceCenter.X, sourceRect.Top);
          endPoint = targetCenter.Y > sourceCenter.Y
            ? new PointF(targetCenter.X, targetRect.Top)
            : new PointF(targetCenter.X, targetRect.Bottom);
        } else {
          // Horizontal connection
          startPoint = targetCenter.X > sourceCenter.X
            ? new PointF(sourceRect.Right, sourceCenter.Y)
            : new PointF(sourceRect.Left, sourceCenter.Y);
          endPoint = targetCenter.X > sourceCenter.X
            ? new PointF(targetRect.Left, targetCenter.Y)
            : new PointF(targetRect.Right, targetCenter.Y);
        }

        // Apply animation
        endPoint = new PointF(
          startPoint.X + (endPoint.X - startPoint.X) * (float)context.AnimationProgress,
          startPoint.Y + (endPoint.Y - startPoint.Y) * (float)context.AnimationProgress
        );

        var lineColor = edge.Color ?? Color.FromArgb(100, 100, 100);
        using var pen = new Pen(lineColor, 2);
        if (edge.Directed) {
          pen.EndCap = LineCap.ArrowAnchor;
          pen.CustomEndCap = new AdjustableArrowCap(5, 5);
        }
        g.DrawLine(pen, startPoint, endPoint);
      }
    }

    // Draw nodes
    var colors = GetDefaultColors();
    for (var i = 0; i < networkNodes.Count; ++i) {
      var node = networkNodes[i];
      var rect = nodePositions[node.Id];
      var color = node.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw shape based on node shape property
      this._DrawNodeShape(g, node.Shape, animatedRect, color);

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      if (labelSize.Width < animatedRect.Width - 4 && labelSize.Height < animatedRect.Height - 4) {
        using var brush = new SolidBrush(GetContrastColor(color));
        g.DrawString(label, context.Diagram.Font, brush,
          animatedRect.X + (animatedRect.Width - labelSize.Width) / 2,
          animatedRect.Y + (animatedRect.Height - labelSize.Height) / 2);
      }
    }
  }

  private void _DrawNodeShape(Graphics g, DiagramNodeShape shape, RectangleF rect, Color color) {
    switch (shape) {
      case DiagramNodeShape.Rectangle:
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        break;

      case DiagramNodeShape.RoundedRectangle:
        using (var brush = new SolidBrush(color))
          FillRoundedRectangle(g, brush, rect, 8);
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          DrawRoundedRectangle(g, pen, rect, 8);
        break;

      case DiagramNodeShape.Circle:
      case DiagramNodeShape.Ellipse:
        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, rect);
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawEllipse(pen, rect);
        break;

      case DiagramNodeShape.Diamond:
        var centerX = rect.X + rect.Width / 2;
        var centerY = rect.Y + rect.Height / 2;
        var points = new[] {
          new PointF(centerX, rect.Top),
          new PointF(rect.Right, centerY),
          new PointF(centerX, rect.Bottom),
          new PointF(rect.Left, centerY)
        };
        using (var brush = new SolidBrush(color))
          g.FillPolygon(brush, points);
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawPolygon(pen, points);
        break;

      case DiagramNodeShape.Triangle:
        var triPoints = new[] {
          new PointF(rect.X + rect.Width / 2, rect.Top),
          new PointF(rect.Right, rect.Bottom),
          new PointF(rect.Left, rect.Bottom)
        };
        using (var brush = new SolidBrush(color))
          g.FillPolygon(brush, triPoints);
        using (var pen = new Pen(Darken(color, 0.2f), 1))
          g.DrawPolygon(pen, triPoints);
        break;

      case DiagramNodeShape.Hexagon:
        var hexCenter = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        var hexRadius = Math.Min(rect.Width, rect.Height) / 2;
        using (var brush = new SolidBrush(color))
          DrawPolygon(g, brush, hexCenter, hexRadius, 6);
        break;

      default:
        // Default to rounded rectangle
        using (var defaultBrush = new SolidBrush(color))
          FillRoundedRectangle(g, defaultBrush, rect, 8);
        using (var defaultPen = new Pen(Darken(color, 0.2f), 1))
          DrawRoundedRectangle(g, defaultPen, rect, 8);
        break;
    }
  }

  private void _DrawPlaceholder(Graphics g, RectangleF plotArea, string text) {
    using var brush = new SolidBrush(Color.LightGray);
    g.FillRectangle(brush, plotArea);
    using var textBrush = new SolidBrush(Color.Gray);
    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    g.DrawString(text, SystemFonts.DefaultFont, textBrush, plotArea, format);
  }
}
