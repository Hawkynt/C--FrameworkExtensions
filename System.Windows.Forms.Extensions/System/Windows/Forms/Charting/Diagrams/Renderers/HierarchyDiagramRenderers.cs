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
/// Renderer for organizational chart diagrams with card-style nodes.
/// </summary>
public class OrgChartDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.OrgChart;

  /// <summary>Node card width.</summary>
  public float NodeWidth { get; set; } = 120;

  /// <summary>Node card height.</summary>
  public float NodeHeight { get; set; } = 60;

  /// <summary>Vertical spacing between levels.</summary>
  public float LevelSpacing { get; set; } = 40;

  /// <summary>Horizontal spacing between siblings.</summary>
  public float SiblingSpacing { get; set; } = 20;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var hierarchyData = context.Diagram.HierarchyNodes;

    if (hierarchyData == null || hierarchyData.Count == 0)
      return;

    // Find root nodes
    var allIds = hierarchyData.Select(d => d.Id).ToHashSet();
    var roots = hierarchyData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    // Build children map
    var childrenMap = hierarchyData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(grp => grp.Key, grp => grp.ToList());

    // Measure node sizes (card + label)
    var nodeSizes = new Dictionary<string, SizeF>();
    var labelPadding = 20f;
    var margin = 15f;

    foreach (var node in hierarchyData) {
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      var width = Math.Max(this.NodeWidth, labelSize.Width + labelPadding);
      var height = Math.Max(this.NodeHeight, labelSize.Height + labelPadding);
      nodeSizes[node.Id] = new SizeF(width + margin, height + margin);
    }

    // Find max size for uniform grid cells
    var maxNodeWidth = nodeSizes.Values.Max(s => s.Width);
    var maxNodeHeight = nodeSizes.Values.Max(s => s.Height);

    // Calculate positions using uniform cell sizes
    var nodePositions = new Dictionary<string, RectangleF>();
    var currentX = 0f;

    foreach (var root in roots) {
      this._PositionOrgNode(root, childrenMap, nodePositions, maxNodeWidth, maxNodeHeight, 0, ref currentX);
      currentX += this.SiblingSpacing;
    }

    // Scale and center the diagram to fit within plot area (scale both in and out)
    if (nodePositions.Count > 0) {
      var minX = nodePositions.Values.Min(r => r.Left);
      var maxX = nodePositions.Values.Max(r => r.Right);
      var minY = nodePositions.Values.Min(r => r.Top);
      var maxY = nodePositions.Values.Max(r => r.Bottom);

      var contentWidth = maxX - minX;
      var contentHeight = maxY - minY;
      var padding = 20f;

      var scaleX = contentWidth > 0 ? (context.PlotArea.Width - padding * 2) / contentWidth : 1;
      var scaleY = contentHeight > 0 ? (context.PlotArea.Height - padding * 2) / contentHeight : 1;
      var scale = Math.Min(scaleX, scaleY); // Scale both in and out

      var scaledContentWidth = contentWidth * scale;
      var scaledContentHeight = contentHeight * scale;
      var offsetX = context.PlotArea.Left + (context.PlotArea.Width - scaledContentWidth) / 2 - minX * scale;
      var offsetY = context.PlotArea.Top + (context.PlotArea.Height - scaledContentHeight) / 2 - minY * scale;

      nodePositions = nodePositions.ToDictionary(
        kvp => kvp.Key,
        kvp => new RectangleF(
          kvp.Value.X * scale + offsetX,
          kvp.Value.Y * scale + offsetY,
          kvp.Value.Width * scale,
          kvp.Value.Height * scale
        )
      );
    }

    var colors = GetDefaultColors();

    // Draw connections
    foreach (var node in hierarchyData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var parentRect = nodePositions[node.ParentId];
      var childRect = nodePositions[node.Id];

      var parentBottom = new PointF(parentRect.X + parentRect.Width / 2, parentRect.Bottom);
      var childTop = new PointF(childRect.X + childRect.Width / 2, childRect.Top);

      // Apply animation
      childTop = new PointF(
        parentBottom.X + (childTop.X - parentBottom.X) * (float)context.AnimationProgress,
        parentBottom.Y + (childTop.Y - parentBottom.Y) * (float)context.AnimationProgress
      );

      var midY = parentBottom.Y + (childTop.Y - parentBottom.Y) / 2;

      using var pen = new Pen(Color.Gray, 2);
      g.DrawLine(pen, parentBottom, new PointF(parentBottom.X, midY));
      g.DrawLine(pen, new PointF(parentBottom.X, midY), new PointF(childTop.X, midY));
      g.DrawLine(pen, new PointF(childTop.X, midY), childTop);
    }

    // Draw nodes
    var nodeIndex = 0;
    foreach (var node in hierarchyData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var rect = nodePositions[node.Id];
      var color = node.Color ?? colors[nodeIndex % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw card with shadow
      using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
        FillRoundedRectangle(g, shadowBrush, new RectangleF(animatedRect.X + 3, animatedRect.Y + 3, animatedRect.Width, animatedRect.Height), 8);

      using (var brush = new SolidBrush(Color.White))
        FillRoundedRectangle(g, brush, animatedRect, 8);

      // Draw header with color
      var headerRect = new RectangleF(animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height * 0.35f);
      using (var headerBrush = new SolidBrush(color))
        FillRoundedRectangle(g, headerBrush, headerRect, 8);

      using (var pen = new Pen(Darken(color, 0.2f), 1))
        DrawRoundedRectangle(g, pen, animatedRect, 8);

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush,
        animatedRect.X + (animatedRect.Width - labelSize.Width) / 2,
        animatedRect.Y + animatedRect.Height * 0.5f);

      context.RegisterHitTestRect(node, animatedRect);
      ++nodeIndex;
    }
  }

  private void _PositionOrgNode(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, RectangleF> positions, float cellWidth, float cellHeight, int depth, ref float currentX) {
    var y = depth * (cellHeight + this.LevelSpacing);

    if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0) {
      var startX = currentX;
      foreach (var child in children) {
        this._PositionOrgNode(child, childrenMap, positions, cellWidth, cellHeight, depth + 1, ref currentX);
        currentX += this.SiblingSpacing;
      }
      currentX -= this.SiblingSpacing;

      // Position at center of children
      var endX = currentX;
      var centerX = (startX + endX + cellWidth) / 2;
      positions[node.Id] = new RectangleF(centerX - cellWidth / 2, y, cellWidth, cellHeight);
    } else {
      // Leaf node
      positions[node.Id] = new RectangleF(currentX, y, cellWidth, cellHeight);
      currentX += cellWidth;
    }
  }
}

/// <summary>
/// Renderer for mind map diagrams with radial layout from center.
/// </summary>
public class MindMapDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.MindMap;

  /// <summary>Minimum distance from center.</summary>
  public float MinRadius { get; set; } = 80;

  /// <summary>Distance increment per level.</summary>
  public float RadiusIncrement { get; set; } = 100;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var hierarchyData = context.Diagram.HierarchyNodes;

    if (hierarchyData == null || hierarchyData.Count == 0)
      return;

    // Find root nodes
    var allIds = hierarchyData.Select(d => d.Id).ToHashSet();
    var roots = hierarchyData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
    var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;

    // Build children map
    var childrenMap = hierarchyData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(grp => grp.Key, grp => grp.ToList());

    // Position nodes radially
    var nodePositions = new Dictionary<string, PointF>();
    var nodeSizes = new Dictionary<string, SizeF>();

    // Position root at center
    var root = roots[0];
    nodePositions[root.Id] = new PointF(centerX, centerY);
    nodeSizes[root.Id] = new SizeF(100, 40);

    // Position children in expanding rings
    this._PositionMindMapChildren(root, childrenMap, nodePositions, nodeSizes, new PointF(centerX, centerY), 0, 360, 1, g, context.Diagram.Font);

    // Scale to fit within plot area (scale both in and out)
    if (nodePositions.Count > 0) {
      var minX = float.MaxValue;
      var maxX = float.MinValue;
      var minY = float.MaxValue;
      var maxY = float.MinValue;

      foreach (var kvp in nodePositions) {
        var pos = kvp.Value;
        var size = nodeSizes.ContainsKey(kvp.Key) ? nodeSizes[kvp.Key] : new SizeF(80, 30);
        minX = Math.Min(minX, pos.X - size.Width / 2);
        maxX = Math.Max(maxX, pos.X + size.Width / 2);
        minY = Math.Min(minY, pos.Y - size.Height / 2);
        maxY = Math.Max(maxY, pos.Y + size.Height / 2);
      }

      var contentWidth = maxX - minX;
      var contentHeight = maxY - minY;
      var padding = 20f;

      var scaleX = contentWidth > 0 ? (context.PlotArea.Width - padding * 2) / contentWidth : 1;
      var scaleY = contentHeight > 0 ? (context.PlotArea.Height - padding * 2) / contentHeight : 1;
      var scale = Math.Min(scaleX, scaleY); // Scale both in and out

      var newCenterX = context.PlotArea.Left + context.PlotArea.Width / 2;
      var newCenterY = context.PlotArea.Top + context.PlotArea.Height / 2;
      var contentCenterX = (minX + maxX) / 2;
      var contentCenterY = (minY + maxY) / 2;

      var newPositions = new Dictionary<string, PointF>();
      var newSizes = new Dictionary<string, SizeF>();

      foreach (var kvp in nodePositions) {
        var oldPos = kvp.Value;
        var newPos = new PointF(
          newCenterX + (oldPos.X - contentCenterX) * scale,
          newCenterY + (oldPos.Y - contentCenterY) * scale
        );
        newPositions[kvp.Key] = newPos;

        if (nodeSizes.ContainsKey(kvp.Key)) {
          var oldSize = nodeSizes[kvp.Key];
          newSizes[kvp.Key] = new SizeF(oldSize.Width * scale, oldSize.Height * scale);
        }
      }

      nodePositions = newPositions;
      nodeSizes = newSizes;
    }

    var colors = GetDefaultColors();

    // Draw connections as curved lines
    foreach (var node in hierarchyData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var parentPos = nodePositions[node.ParentId];
      var childPos = nodePositions[node.Id];

      // Apply animation
      childPos = new PointF(
        parentPos.X + (childPos.X - parentPos.X) * (float)context.AnimationProgress,
        parentPos.Y + (childPos.Y - parentPos.Y) * (float)context.AnimationProgress
      );

      var color = node.Color ?? colors[0];
      using var pen = new Pen(Color.FromArgb(150, color), 2);

      // Draw curved connection
      var midX = (parentPos.X + childPos.X) / 2;
      var midY = (parentPos.Y + childPos.Y) / 2;
      var controlOffset = Math.Min(30, Math.Abs(childPos.X - parentPos.X) * 0.3f);

      using var path = new GraphicsPath();
      path.AddBezier(parentPos, new PointF(midX, parentPos.Y + controlOffset), new PointF(midX, childPos.Y - controlOffset), childPos);
      g.DrawPath(pen, path);
    }

    // Draw nodes
    var nodeIndex = 0;
    foreach (var node in hierarchyData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var pos = nodePositions[node.Id];
      var size = nodeSizes.ContainsKey(node.Id) ? nodeSizes[node.Id] : new SizeF(80, 30);
      var color = node.Color ?? colors[nodeIndex % colors.Length];
      var isRoot = roots.Contains(node);

      // Apply animation
      var animatedSize = new SizeF(size.Width * (float)context.AnimationProgress, size.Height * (float)context.AnimationProgress);
      var rect = new RectangleF(pos.X - animatedSize.Width / 2, pos.Y - animatedSize.Height / 2, animatedSize.Width, animatedSize.Height);

      // Skip drawing if rectangle is too small
      if (rect.Width < 1 || rect.Height < 1)
        continue;

      // Root has special styling
      if (isRoot) {
        using var gradient = new LinearGradientBrush(rect, Lighten(color, 0.2f), color, LinearGradientMode.Vertical);
        FillRoundedRectangle(g, gradient, rect, 15);
      } else {
        using var brush = new SolidBrush(Lighten(color, 0.4f));
        FillRoundedRectangle(g, brush, rect, 10);
      }

      using (var pen = new Pen(color, isRoot ? 3 : 2))
        DrawRoundedRectangle(g, pen, rect, isRoot ? 15 : 10);

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(isRoot ? GetContrastColor(color) : Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush, pos.X - labelSize.Width / 2, pos.Y - labelSize.Height / 2);

      ++nodeIndex;
    }
  }

  private void _PositionMindMapChildren(DiagramHierarchyNode parent, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, PointF> positions, Dictionary<string, SizeF> sizes, PointF center, float startAngle, float sweepAngle, int depth, Graphics g, Font font) {
    if (!childrenMap.TryGetValue(parent.Id, out var children) || children.Count == 0)
      return;

    var radius = this.MinRadius + depth * this.RadiusIncrement;
    var anglePerChild = sweepAngle / children.Count;
    var currentAngle = startAngle + anglePerChild / 2;

    foreach (var child in children) {
      var angleRad = currentAngle * Math.PI / 180;
      var x = center.X + (float)(Math.Cos(angleRad) * radius);
      var y = center.Y + (float)(Math.Sin(angleRad) * radius);

      positions[child.Id] = new PointF(x, y);

      var label = child.Label ?? child.Id;
      var labelSize = g.MeasureString(label, font);
      sizes[child.Id] = new SizeF(labelSize.Width + 20, labelSize.Height + 10);

      // Recursively position children with reduced sweep
      this._PositionMindMapChildren(child, childrenMap, positions, sizes, new PointF(x, y), currentAngle - anglePerChild / 2, anglePerChild, depth + 1, g, font);

      currentAngle += anglePerChild;
    }
  }
}

/// <summary>
/// Renderer for Work Breakdown Structure (WBS) diagrams.
/// </summary>
public class WBSDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.WBS;

  /// <summary>Node width.</summary>
  public float NodeWidth { get; set; } = 100;

  /// <summary>Node height.</summary>
  public float NodeHeight { get; set; } = 50;

  /// <summary>Vertical spacing.</summary>
  public float VerticalSpacing { get; set; } = 30;

  /// <summary>Horizontal spacing.</summary>
  public float HorizontalSpacing { get; set; } = 15;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var hierarchyData = context.Diagram.HierarchyNodes;

    if (hierarchyData == null || hierarchyData.Count == 0)
      return;

    // Find root nodes
    var allIds = hierarchyData.Select(d => d.Id).ToHashSet();
    var roots = hierarchyData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    // Build children map
    var childrenMap = hierarchyData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(grp => grp.Key, grp => grp.ToList());

    // Assign WBS numbers
    var wbsNumbers = new Dictionary<string, string>();
    var nodeIndex = 1;
    foreach (var root in roots) {
      this._AssignWBSNumbers(root, childrenMap, wbsNumbers, nodeIndex.ToString());
      ++nodeIndex;
    }

    // Measure node sizes (WBS number + label)
    var nodeSizes = new Dictionary<string, SizeF>();
    var labelPadding = 15f;
    var margin = 10f;

    foreach (var node in hierarchyData) {
      var wbsNumber = wbsNumbers.ContainsKey(node.Id) ? wbsNumbers[node.Id] : "";
      var label = node.Label ?? node.Id;
      var wbsSize = g.MeasureString(wbsNumber, context.Diagram.Font);
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      var width = Math.Max(this.NodeWidth, Math.Max(wbsSize.Width, labelSize.Width) + labelPadding * 2);
      var height = Math.Max(this.NodeHeight, wbsSize.Height + labelSize.Height + labelPadding);
      nodeSizes[node.Id] = new SizeF(width + margin, height + margin);
    }

    // Find max size for uniform grid cells
    var maxNodeWidth = nodeSizes.Values.Max(s => s.Width);
    var maxNodeHeight = nodeSizes.Values.Max(s => s.Height);

    // Position nodes using uniform cell sizes
    var nodePositions = new Dictionary<string, RectangleF>();
    var currentX = 0f;

    foreach (var root in roots) {
      this._PositionWBSNode(root, childrenMap, nodePositions, maxNodeWidth, maxNodeHeight, 0, ref currentX);
      currentX += this.HorizontalSpacing;
    }

    // Scale and center the diagram to fit within plot area (scale both in and out)
    if (nodePositions.Count > 0) {
      var minX = nodePositions.Values.Min(r => r.Left);
      var maxX = nodePositions.Values.Max(r => r.Right);
      var minY = nodePositions.Values.Min(r => r.Top);
      var maxY = nodePositions.Values.Max(r => r.Bottom);

      var contentWidth = maxX - minX;
      var contentHeight = maxY - minY;
      var padding = 20f;

      var scaleX = contentWidth > 0 ? (context.PlotArea.Width - padding * 2) / contentWidth : 1;
      var scaleY = contentHeight > 0 ? (context.PlotArea.Height - padding * 2) / contentHeight : 1;
      var scale = Math.Min(scaleX, scaleY); // Scale both in and out

      var scaledContentWidth = contentWidth * scale;
      var scaledContentHeight = contentHeight * scale;
      var offsetX = context.PlotArea.Left + (context.PlotArea.Width - scaledContentWidth) / 2 - minX * scale;
      var offsetY = context.PlotArea.Top + (context.PlotArea.Height - scaledContentHeight) / 2 - minY * scale;

      nodePositions = nodePositions.ToDictionary(
        kvp => kvp.Key,
        kvp => new RectangleF(
          kvp.Value.X * scale + offsetX,
          kvp.Value.Y * scale + offsetY,
          kvp.Value.Width * scale,
          kvp.Value.Height * scale
        )
      );
    }

    var colors = GetDefaultColors();

    // Draw connections
    foreach (var node in hierarchyData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var parentRect = nodePositions[node.ParentId];
      var childRect = nodePositions[node.Id];

      var parentBottom = new PointF(parentRect.X + parentRect.Width / 2, parentRect.Bottom);
      var childTop = new PointF(childRect.X + childRect.Width / 2, childRect.Top);

      // Apply animation
      childTop = new PointF(
        parentBottom.X + (childTop.X - parentBottom.X) * (float)context.AnimationProgress,
        parentBottom.Y + (childTop.Y - parentBottom.Y) * (float)context.AnimationProgress
      );

      using var pen = new Pen(Color.Gray, 1);
      g.DrawLine(pen, parentBottom, childTop);
    }

    // Draw nodes
    nodeIndex = 0;
    foreach (var node in hierarchyData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var rect = nodePositions[node.Id];
      var color = node.Color ?? colors[nodeIndex % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      using (var brush = new SolidBrush(Lighten(color, 0.6f)))
        g.FillRectangle(brush, animatedRect);

      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw WBS number
      var wbsNumber = wbsNumbers.ContainsKey(node.Id) ? wbsNumbers[node.Id] : "";
      var numberSize = g.MeasureString(wbsNumber, context.Diagram.Font);

      using (var numberBrush = new SolidBrush(color))
        g.DrawString(wbsNumber, context.Diagram.Font, numberBrush, animatedRect.X + 5, animatedRect.Y + 5);

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, context.Diagram.Font, labelBrush,
        animatedRect.X + (animatedRect.Width - labelSize.Width) / 2,
        animatedRect.Y + numberSize.Height + 5);

      ++nodeIndex;
    }
  }

  private void _AssignWBSNumbers(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, string> wbsNumbers, string prefix) {
    wbsNumbers[node.Id] = prefix;

    if (!childrenMap.TryGetValue(node.Id, out var children))
      return;

    var childIndex = 1;
    foreach (var child in children) {
      this._AssignWBSNumbers(child, childrenMap, wbsNumbers, $"{prefix}.{childIndex}");
      ++childIndex;
    }
  }

  private void _PositionWBSNode(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, RectangleF> positions, float cellWidth, float cellHeight, int depth, ref float currentX) {
    var y = depth * (cellHeight + this.VerticalSpacing);

    if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0) {
      var startX = currentX;
      foreach (var child in children) {
        this._PositionWBSNode(child, childrenMap, positions, cellWidth, cellHeight, depth + 1, ref currentX);
        currentX += this.HorizontalSpacing;
      }
      currentX -= this.HorizontalSpacing;

      var endX = currentX;
      var centerX = (startX + endX + cellWidth) / 2;
      positions[node.Id] = new RectangleF(centerX - cellWidth / 2, y, cellWidth, cellHeight);
    } else {
      positions[node.Id] = new RectangleF(currentX, y, cellWidth, cellHeight);
      currentX += cellWidth;
    }
  }
}

/// <summary>
/// Renderer for Ishikawa (fishbone/cause-effect) diagrams.
/// </summary>
public class FishboneDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Fishbone;

  /// <summary>Bone angle in degrees.</summary>
  public float BoneAngle { get; set; } = 60;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var hierarchyData = context.Diagram.HierarchyNodes;

    if (hierarchyData == null || hierarchyData.Count == 0)
      return;

    // Find root (effect) and main causes
    var allIds = hierarchyData.Select(d => d.Id).ToHashSet();
    var root = hierarchyData.FirstOrDefault(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId));

    if (root == null)
      return;

    // Build children map
    var childrenMap = hierarchyData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(grp => grp.Key, grp => grp.ToList());

    var colors = GetDefaultColors();

    // Draw main spine
    var spineY = context.PlotArea.Top + context.PlotArea.Height / 2;
    var spineStartX = context.PlotArea.Left + 50;
    var spineEndX = context.PlotArea.Right - 100;

    using (var spinePen = new Pen(Color.DarkGray, 3))
      g.DrawLine(spinePen, spineStartX, spineY, spineEndX * (float)context.AnimationProgress, spineY);

    // Draw effect (head)
    var headRect = new RectangleF(spineEndX - 10, spineY - 30, 120, 60);
    using (var headBrush = new SolidBrush(colors[0]))
      FillRoundedRectangle(g, headBrush, headRect, 8);
    using (var headPen = new Pen(Darken(colors[0], 0.2f), 2))
      DrawRoundedRectangle(g, headPen, headRect, 8);

    var effectLabel = root.Label ?? root.Id;
    var effectSize = g.MeasureString(effectLabel, context.Diagram.Font);
    using (var labelBrush = new SolidBrush(GetContrastColor(colors[0])))
      g.DrawString(effectLabel, context.Diagram.Font, labelBrush,
        headRect.X + (headRect.Width - effectSize.Width) / 2,
        headRect.Y + (headRect.Height - effectSize.Height) / 2);

    // Draw main causes (bones)
    if (!childrenMap.TryGetValue(root.Id, out var mainCauses))
      return;

    var spineLength = spineEndX - spineStartX - 100;
    var angleRad = this.BoneAngle * Math.PI / 180;
    var boneLength = context.PlotArea.Height * 0.35f;

    // Calculate how many main causes we can display (minimum spacing of ~40 pixels per bone)
    var maxMainCauses = Math.Max(1, (int)(spineLength / 40));
    var displayCauses = mainCauses.Take(maxMainCauses).ToList();
    var boneSpacing = spineLength / Math.Max(1, (displayCauses.Count + 1));

    // If we're truncating, show indicator
    if (mainCauses.Count > maxMainCauses) {
      using var truncFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f, FontStyle.Italic);
      using var truncBrush = new SolidBrush(Color.Gray);
      var truncText = $"... and {mainCauses.Count - maxMainCauses} more causes";
      g.DrawString(truncText, truncFont, truncBrush, spineStartX + 10, spineY + 5);
    }

    for (var i = 0; i < displayCauses.Count; ++i) {
      var cause = displayCauses[i];
      var boneX = spineStartX + (i + 1) * boneSpacing;
      var isTop = i % 2 == 0;

      var startPoint = new PointF(boneX, spineY);
      var endY = isTop ? spineY - boneLength : spineY + boneLength;
      var endX = boneX - (float)(boneLength / Math.Tan(angleRad)) * (isTop ? 1 : -1);
      if (!isTop)
        endX = boneX + (float)(boneLength / Math.Tan(angleRad));
      endX = boneX - (float)(boneLength * Math.Cos(angleRad));
      endY = isTop ? spineY - (float)(boneLength * Math.Sin(angleRad)) : spineY + (float)(boneLength * Math.Sin(angleRad));

      var endPoint = new PointF(
        startPoint.X + (endX - startPoint.X) * (float)context.AnimationProgress,
        startPoint.Y + (endY - startPoint.Y) * (float)context.AnimationProgress
      );

      var color = cause.Color ?? colors[(i + 1) % colors.Length];

      // Draw bone
      using (var bonePen = new Pen(color, 2))
        g.DrawLine(bonePen, startPoint, endPoint);

      // Draw cause label
      var label = cause.Label ?? cause.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      var labelX = endX - labelSize.Width / 2;
      var labelY = isTop ? endY - labelSize.Height - 5 : endY + 5;

      using (var labelBrush = new SolidBrush(color))
        g.DrawString(label, context.Diagram.Font, labelBrush, labelX, labelY);

      // Draw sub-causes (ribs)
      if (childrenMap.TryGetValue(cause.Id, out var subCauses)) {
        var ribLength = boneLength * 0.4f;
        var ribSpacing = boneLength / (subCauses.Count + 1);

        for (var j = 0; j < subCauses.Count; ++j) {
          var subCause = subCauses[j];
          var t = (j + 1.0f) / (subCauses.Count + 1);

          var ribStartX = startPoint.X + (endX - startPoint.X) * t;
          var ribStartY = startPoint.Y + (endY - startPoint.Y) * t;
          var ribStart = new PointF(ribStartX, ribStartY);

          var ribEndX = ribStartX - ribLength * 0.8f;
          var ribEndY = isTop ? ribStartY - ribLength * 0.3f : ribStartY + ribLength * 0.3f;

          var ribEnd = new PointF(
            ribStart.X + (ribEndX - ribStart.X) * (float)context.AnimationProgress,
            ribStart.Y + (ribEndY - ribStart.Y) * (float)context.AnimationProgress
          );

          using (var ribPen = new Pen(Lighten(color, 0.3f), 1))
            g.DrawLine(ribPen, ribStart, ribEnd);

          var subLabel = subCause.Label ?? subCause.Id;
          var subLabelSize = g.MeasureString(subLabel, context.Diagram.Font);

          using var subLabelFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f);
          using (var subLabelBrush = new SolidBrush(Color.DarkGray))
            g.DrawString(subLabel, subLabelFont, subLabelBrush, ribEndX - subLabelSize.Width, ribEndY - subLabelSize.Height / 2);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for decision tree diagrams.
/// </summary>
public class DecisionTreeDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.DecisionTree;

  /// <summary>Decision node size.</summary>
  public float DecisionSize { get; set; } = 40;

  /// <summary>Outcome node width.</summary>
  public float OutcomeWidth { get; set; } = 80;

  /// <summary>Outcome node height.</summary>
  public float OutcomeHeight { get; set; } = 30;

  /// <summary>Vertical spacing.</summary>
  public float VerticalSpacing { get; set; } = 50;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var hierarchyData = context.Diagram.HierarchyNodes;

    if (hierarchyData == null || hierarchyData.Count == 0)
      return;

    // Find root nodes
    var allIds = hierarchyData.Select(d => d.Id).ToHashSet();
    var roots = hierarchyData.Where(d => string.IsNullOrEmpty(d.ParentId) || !allIds.Contains(d.ParentId)).ToList();

    if (roots.Count == 0)
      return;

    // Build children map
    var childrenMap = hierarchyData
      .Where(d => !string.IsNullOrEmpty(d.ParentId))
      .GroupBy(d => d.ParentId)
      .ToDictionary(grp => grp.Key, grp => grp.ToList());

    // Measure node sizes (decision diamond or outcome rectangle + label)
    var nodeSizes = new Dictionary<string, SizeF>();
    var margin = 20f;
    var labelPadding = 10f;

    foreach (var node in hierarchyData) {
      var isDecision = childrenMap.ContainsKey(node.Id) && childrenMap[node.Id].Count > 0;
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);

      if (isDecision) {
        // Diamond: size based on label fitting inside
        var diagonalSize = Math.Max(this.DecisionSize, (float)Math.Sqrt(labelSize.Width * labelSize.Width + labelSize.Height * labelSize.Height) + labelPadding);
        nodeSizes[node.Id] = new SizeF(diagonalSize + margin, diagonalSize + margin);
      } else {
        // Outcome rectangle
        var width = Math.Max(this.OutcomeWidth, labelSize.Width + labelPadding * 2);
        var height = Math.Max(this.OutcomeHeight, labelSize.Height + labelPadding);
        nodeSizes[node.Id] = new SizeF(width + margin, height + margin);
      }
    }

    // Find max size for uniform grid cells
    var maxNodeWidth = nodeSizes.Values.Max(s => s.Width);
    var maxNodeHeight = nodeSizes.Values.Max(s => s.Height);

    // Position nodes using uniform cell sizes
    var nodePositions = new Dictionary<string, PointF>();
    var leafCount = this._CountLeaves(roots, childrenMap);
    var maxDepth = this._GetMaxDepth(roots, childrenMap, 0);

    var nodeSpacing = maxNodeWidth;
    var verticalSpacing = maxNodeHeight;

    var currentLeaf = 0f;
    foreach (var root in roots)
      this._PositionDecisionNode(root, childrenMap, nodePositions, 0, nodeSpacing, verticalSpacing, ref currentLeaf);

    // Scale content to fit (scale both in and out)
    if (nodePositions.Count > 0) {
      var minX = float.MaxValue;
      var maxX = float.MinValue;
      var minY = float.MaxValue;
      var maxY = float.MinValue;

      foreach (var kvp in nodePositions) {
        var pos = kvp.Value;
        var size = nodeSizes[kvp.Key];
        minX = Math.Min(minX, pos.X - size.Width / 2);
        maxX = Math.Max(maxX, pos.X + size.Width / 2);
        minY = Math.Min(minY, pos.Y - size.Height / 2);
        maxY = Math.Max(maxY, pos.Y + size.Height / 2);
      }

      var contentWidth = maxX - minX;
      var contentHeight = maxY - minY;
      var padding = 20f;

      var scaleX = contentWidth > 0 ? (context.PlotArea.Width - padding * 2) / contentWidth : 1f;
      var scaleY = contentHeight > 0 ? (context.PlotArea.Height - padding * 2) / contentHeight : 1f;
      var scale = Math.Min(scaleX, scaleY); // Scale both in and out

      var centerX = (minX + maxX) / 2;
      var centerY = (minY + maxY) / 2;
      var newCenterX = context.PlotArea.Left + context.PlotArea.Width / 2;
      var newCenterY = context.PlotArea.Top + context.PlotArea.Height / 2;

      var newPositions = new Dictionary<string, PointF>();
      foreach (var kvp in nodePositions) {
        var oldPos = kvp.Value;
        newPositions[kvp.Key] = new PointF(
          newCenterX + (oldPos.X - centerX) * scale,
          newCenterY + (oldPos.Y - centerY) * scale
        );
      }
      nodePositions = newPositions;
    }

    var colors = GetDefaultColors();

    // Draw connections
    foreach (var node in hierarchyData.Where(d => !string.IsNullOrEmpty(d.ParentId) && nodePositions.ContainsKey(d.ParentId))) {
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
      g.DrawLine(pen, parentPos, childPos);

      // Draw edge label (probability/condition)
      if (node.Value > 0) {
        var midPoint = new PointF((parentPos.X + childPos.X) / 2, (parentPos.Y + childPos.Y) / 2);
        var probLabel = $"{node.Value:P0}";
        var probSize = g.MeasureString(probLabel, context.Diagram.Font);

        using var bgBrush = new SolidBrush(Color.White);
        g.FillRectangle(bgBrush, midPoint.X - probSize.Width / 2 - 2, midPoint.Y - probSize.Height / 2 - 2, probSize.Width + 4, probSize.Height + 4);

        using var probBrush = new SolidBrush(Color.Blue);
        g.DrawString(probLabel, context.Diagram.Font, probBrush, midPoint.X - probSize.Width / 2, midPoint.Y - probSize.Height / 2);
      }
    }

    // Draw nodes
    var nodeIndex = 0;
    foreach (var node in hierarchyData) {
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var pos = nodePositions[node.Id];
      var isDecision = childrenMap.ContainsKey(node.Id) && childrenMap[node.Id].Count > 0;
      var color = node.Color ?? colors[nodeIndex % colors.Length];

      if (isDecision) {
        // Decision node: diamond
        var size = this.DecisionSize * (float)context.AnimationProgress;
        var points = new[] {
          new PointF(pos.X, pos.Y - size / 2),
          new PointF(pos.X + size / 2, pos.Y),
          new PointF(pos.X, pos.Y + size / 2),
          new PointF(pos.X - size / 2, pos.Y)
        };

        using (var brush = new SolidBrush(Lighten(color, 0.5f)))
          g.FillPolygon(brush, points);

        using (var pen = new Pen(color, 2))
          g.DrawPolygon(pen, points);
      } else {
        // Outcome/leaf node: rounded rectangle
        var rect = new RectangleF(
          pos.X - this.OutcomeWidth / 2 * (float)context.AnimationProgress,
          pos.Y - this.OutcomeHeight / 2 * (float)context.AnimationProgress,
          this.OutcomeWidth * (float)context.AnimationProgress,
          this.OutcomeHeight * (float)context.AnimationProgress
        );

        using (var brush = new SolidBrush(color))
          FillRoundedRectangle(g, brush, rect, 5);

        using (var pen = new Pen(Darken(color, 0.2f), 1))
          DrawRoundedRectangle(g, pen, rect, 5);
      }

      // Draw label
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      var labelColor = isDecision ? Color.Black : GetContrastColor(color);

      using var labelBrush = new SolidBrush(labelColor);
      g.DrawString(label, context.Diagram.Font, labelBrush, pos.X - labelSize.Width / 2, pos.Y - labelSize.Height / 2);

      ++nodeIndex;
    }
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

  private int _GetMaxDepth(IList<DiagramHierarchyNode> nodes, Dictionary<string, List<DiagramHierarchyNode>> childrenMap, int currentDepth) {
    var maxDepth = currentDepth;
    foreach (var node in nodes) {
      if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0)
        maxDepth = Math.Max(maxDepth, this._GetMaxDepth(children, childrenMap, currentDepth + 1));
    }
    return maxDepth;
  }

  private void _PositionDecisionNode(DiagramHierarchyNode node, Dictionary<string, List<DiagramHierarchyNode>> childrenMap,
    Dictionary<string, PointF> positions, int depth, float nodeSpacing, float verticalSpacing, ref float currentLeaf) {
    var y = depth * verticalSpacing + verticalSpacing / 2;

    if (childrenMap.TryGetValue(node.Id, out var children) && children.Count > 0) {
      var startLeaf = currentLeaf;
      foreach (var child in children)
        this._PositionDecisionNode(child, childrenMap, positions, depth + 1, nodeSpacing, verticalSpacing, ref currentLeaf);

      // Position at center of children
      var x = (startLeaf + (currentLeaf - 1)) / 2 * nodeSpacing + nodeSpacing / 2;
      positions[node.Id] = new PointF(x, y);
    } else {
      // Leaf node
      var x = currentLeaf * nodeSpacing + nodeSpacing / 2;
      positions[node.Id] = new PointF(x, y);
      currentLeaf += 1;
    }
  }
}
