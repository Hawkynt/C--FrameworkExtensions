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
/// Base class for C4 diagram renderers.
/// </summary>
public abstract class C4DiagramRendererBase : DiagramRenderer {
  protected void DrawC4Person(Graphics g, DiagramRenderContext context, RectangleF rect, string name, string description, Color color) {
    var animatedRect = new RectangleF(
      rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
      rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
      rect.Width * (float)context.AnimationProgress,
      rect.Height * (float)context.AnimationProgress
    );

    // Draw head (circle)
    var headSize = animatedRect.Width * 0.4f;
    var headRect = new RectangleF(
      animatedRect.X + (animatedRect.Width - headSize) / 2,
      animatedRect.Y + 5,
      headSize,
      headSize
    );

    using (var brush = new SolidBrush(color))
      g.FillEllipse(brush, headRect);

    // Draw body (rounded rectangle)
    var bodyRect = new RectangleF(
      animatedRect.X + 10,
      headRect.Bottom + 5,
      animatedRect.Width - 20,
      animatedRect.Height - headSize - 30
    );

    using (var brush = new SolidBrush(color))
      FillRoundedRectangle(g, brush, bodyRect, 8);

    // Draw name
    if (!string.IsNullOrEmpty(name)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f, FontStyle.Bold);
      var nameSize = g.MeasureString(name, font);
      using var brush = new SolidBrush(Color.White);
      g.DrawString(name, font, brush, animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, bodyRect.Top + 5);
    }

    // Draw description
    if (!string.IsNullOrEmpty(description)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
      using var brush = new SolidBrush(Color.White);
      using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
      var descRect = new RectangleF(bodyRect.X + 5, bodyRect.Top + 25, bodyRect.Width - 10, bodyRect.Height - 30);
      g.DrawString(description, font, brush, descRect, format);
    }
  }

  protected void DrawC4Container(Graphics g, DiagramRenderContext context, RectangleF rect, string name, string technology, string description, Color color) {
    var animatedRect = new RectangleF(
      rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
      rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
      rect.Width * (float)context.AnimationProgress,
      rect.Height * (float)context.AnimationProgress
    );

    using (var brush = new SolidBrush(color))
      FillRoundedRectangle(g, brush, animatedRect, 8);

    using (var pen = new Pen(Darken(color), 2))
      DrawRoundedRectangle(g, pen, animatedRect, 8);

    var yOffset = animatedRect.Y + 10;

    // Draw name
    if (!string.IsNullOrEmpty(name)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.1f, FontStyle.Bold);
      var nameSize = g.MeasureString(name, font);
      using var brush = new SolidBrush(Color.White);
      g.DrawString(name, font, brush, animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, yOffset);
      yOffset += nameSize.Height + 2;
    }

    // Draw technology
    if (!string.IsNullOrEmpty(technology)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
      var techText = $"[{technology}]";
      var techSize = g.MeasureString(techText, font);
      using var brush = new SolidBrush(Color.FromArgb(200, Color.White));
      g.DrawString(techText, font, brush, animatedRect.X + (animatedRect.Width - techSize.Width) / 2, yOffset);
      yOffset += techSize.Height + 5;
    }

    // Draw description
    if (!string.IsNullOrEmpty(description)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.85f);
      using var brush = new SolidBrush(Color.White);
      using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
      var descRect = new RectangleF(animatedRect.X + 10, yOffset, animatedRect.Width - 20, animatedRect.Bottom - yOffset - 10);
      g.DrawString(description, font, brush, descRect, format);
    }
  }

  protected void DrawC4SystemBoundary(Graphics g, DiagramRenderContext context, RectangleF rect, string name) {
    using var pen = new Pen(Color.FromArgb(150, Color.Gray), 2) { DashStyle = DashStyle.Dash };
    DrawRoundedRectangle(g, pen, rect, 8);

    if (!string.IsNullOrEmpty(name)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f);
      using var brush = new SolidBrush(Color.Gray);
      g.DrawString(name, font, brush, rect.X + 10, rect.Y + 5);
    }
  }

  protected void DrawC4Relationship(Graphics g, PointF from, PointF to, string label, string technology, Font font) {
    using var pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f);
    pen.EndCap = LineCap.ArrowAnchor;
    g.DrawLine(pen, from, to);

    var midPoint = new PointF((from.X + to.X) / 2, (from.Y + to.Y) / 2);

    if (!string.IsNullOrEmpty(label) || !string.IsNullOrEmpty(technology)) {
      var text = label ?? "";
      if (!string.IsNullOrEmpty(technology))
        text += $"\n[{technology}]";

      using var brush = new SolidBrush(Color.FromArgb(80, 80, 80));
      var textSize = g.MeasureString(text, font);
      g.DrawString(text, font, brush, midPoint.X - textSize.Width / 2, midPoint.Y - textSize.Height / 2);
    }
  }
}

/// <summary>
/// Renderer for C4 Context diagrams.
/// </summary>
public class C4ContextDiagramRenderer : C4DiagramRendererBase {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.C4Context;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Position nodes in a grid with proper non-overlapping layout
    var nodeWidth = 150f;
    var nodeHeight = 120f;
    var margin = 30f;
    var positions = new Dictionary<string, RectangleF>();

    // Check if positions are explicitly set
    var hasExplicitPositions = nodes.Any(n => n.Position.X > 0 || n.Position.Y > 0);

    if (hasExplicitPositions) {
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        float x, y;
        if (node.Position.X > 0 || node.Position.Y > 0) {
          x = plotArea.Left + node.Position.X / 100f * (plotArea.Width - nodeWidth);
          y = plotArea.Top + node.Position.Y / 100f * (plotArea.Height - nodeHeight);
        } else {
          x = plotArea.Left + plotArea.Width / 2 - nodeWidth / 2;
          y = plotArea.Top + plotArea.Height / 2 - nodeHeight / 2;
        }
        positions[node.Id] = new RectangleF(x, y, nodeWidth, nodeHeight);
      }
    } else {
      // Auto-layout: use proper grid with margins to avoid overlap
      var gridLayout = CalculateGridLayout(nodes.Count, nodeWidth, nodeHeight, margin, plotArea);
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        if (gridLayout.TryGetValue(i, out var rect))
          positions[node.Id] = rect;
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          nodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          positions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(positions, plotArea);
    }

    // Draw relationships
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var fromRect) ||
            !positions.TryGetValue(edge.Target, out var toRect))
          continue;

        var from = new PointF(fromRect.Left + fromRect.Width / 2, fromRect.Bottom);
        var to = new PointF(toRect.Left + toRect.Width / 2, toRect.Top);

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        this.DrawC4Relationship(g, from, to, edge.Label, null, context.Diagram.Font);
      }
    }

    // Draw nodes
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var rect))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      var shape = node.Shape;

      if (shape == DiagramNodeShape.Custom || node.Group == "Person")
        this.DrawC4Person(g, context, rect, node.Label ?? node.Id, node.Tag as string, color);
      else
        this.DrawC4Container(g, context, rect, node.Label ?? node.Id, null, node.Tag as string, color);

      context.RegisterHitTestRect(node, rect);
    }
  }
}

/// <summary>
/// Renderer for C4 Container diagrams.
/// </summary>
public class C4ContainerDiagramRenderer : C4DiagramRendererBase {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.C4Container;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Group nodes by system
    var systemNodes = nodes.Where(n => n.Group != null).GroupBy(n => n.Group).ToList();
    var standaloneNodes = nodes.Where(n => n.Group == null).ToList();

    var positions = new Dictionary<string, RectangleF>();
    var nodeWidth = 140f;
    var nodeHeight = 100f;

    // Draw system boundaries and position nodes
    var systemY = plotArea.Top + 20;
    foreach (var system in systemNodes) {
      var systemNodes2 = system.ToList();
      var systemWidth = Math.Max(200, systemNodes2.Count * (nodeWidth + 20) + 40);
      var systemRect = new RectangleF(plotArea.Left + 20, systemY, systemWidth, nodeHeight + 60);

      this.DrawC4SystemBoundary(g, context, systemRect, system.Key);

      for (var i = 0; i < systemNodes2.Count; ++i) {
        var node = systemNodes2[i];
        var x = systemRect.X + 20 + i * (nodeWidth + 20);
        var y = systemRect.Y + 30;
        positions[node.Id] = new RectangleF(x, y, nodeWidth, nodeHeight);
      }

      systemY += systemRect.Height + 30;
    }

    // Position standalone nodes
    var standaloneX = plotArea.Left + 20;
    foreach (var node in standaloneNodes) {
      positions[node.Id] = new RectangleF(standaloneX, systemY, nodeWidth, nodeHeight);
      standaloneX += nodeWidth + 20;
    }

    // Draw relationships
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var fromRect) ||
            !positions.TryGetValue(edge.Target, out var toRect))
          continue;

        var from = new PointF(fromRect.Left + fromRect.Width / 2, fromRect.Bottom);
        var to = new PointF(toRect.Left + toRect.Width / 2, toRect.Top);

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        this.DrawC4Relationship(g, from, to, edge.Label, null, context.Diagram.Font);
      }
    }

    // Draw nodes
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var rect))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      this.DrawC4Container(g, context, rect, node.Label ?? node.Id, null, node.Tag as string, color);
      context.RegisterHitTestRect(node, rect);
    }
  }
}

/// <summary>
/// Renderer for C4 Component diagrams.
/// </summary>
public class C4ComponentDiagramRenderer : C4DiagramRendererBase {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.C4Component;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    // Similar to C4Container but with more detail
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    var positions = new Dictionary<string, RectangleF>();
    var nodeWidth = 120f;
    var nodeHeight = 80f;
    var margin = 25f;

    // Check if positions are explicitly set
    var hasExplicitPositions = nodes.Any(n => n.Position.X > 0 || n.Position.Y > 0);

    if (hasExplicitPositions) {
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        float x, y;
        if (node.Position.X > 0 || node.Position.Y > 0) {
          x = plotArea.Left + node.Position.X / 100f * (plotArea.Width - nodeWidth);
          y = plotArea.Top + node.Position.Y / 100f * (plotArea.Height - nodeHeight);
        } else {
          x = plotArea.Left + plotArea.Width / 2 - nodeWidth / 2;
          y = plotArea.Top + plotArea.Height / 2 - nodeHeight / 2;
        }
        positions[node.Id] = new RectangleF(x, y, nodeWidth, nodeHeight);
      }
    } else {
      // Auto-layout: use proper grid with margins to avoid overlap
      var gridLayout = CalculateGridLayout(nodes.Count, nodeWidth, nodeHeight, margin, plotArea);
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        if (gridLayout.TryGetValue(i, out var rect))
          positions[node.Id] = rect;
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          nodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          positions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(positions, plotArea);
    }

    // Draw relationships
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var fromRect) ||
            !positions.TryGetValue(edge.Target, out var toRect))
          continue;

        var from = new PointF(fromRect.Right, fromRect.Top + fromRect.Height / 2);
        var to = new PointF(toRect.Left, toRect.Top + toRect.Height / 2);

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        this.DrawC4Relationship(g, from, to, edge.Label, null, context.Diagram.Font);
      }
    }

    // Draw components
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var rect))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      this.DrawC4Container(g, context, rect, node.Label ?? node.Id, node.Group, node.Tag as string, color);
      context.RegisterHitTestRect(node, rect);
    }
  }
}

/// <summary>
/// Renderer for C4 Deployment diagrams.
/// </summary>
public class C4DeploymentDiagramRenderer : C4DiagramRendererBase {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.C4Deployment;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Group by deployment nodes
    var deploymentGroups = nodes.Where(n => n.Group != null).GroupBy(n => n.Group).ToList();
    var standaloneNodes = nodes.Where(n => n.Group == null).ToList();
    var positions = new Dictionary<string, RectangleF>();

    var groupY = plotArea.Top + 20;
    var nodeWidth = 130f;
    var nodeHeight = 90f;

    // If no groups and all nodes are standalone, arrange in a grid
    if (deploymentGroups.Count == 0 && standaloneNodes.Count > 0) {
      // Use proper grid layout with margins to avoid overlap
      var margin = 25f;
      var gridLayout = CalculateGridLayout(standaloneNodes.Count, nodeWidth, nodeHeight, margin, plotArea);

      for (var i = 0; i < standaloneNodes.Count; ++i) {
        var node = standaloneNodes[i];
        if (gridLayout.TryGetValue(i, out var rect))
          positions[node.Id] = rect;
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          standaloneNodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          positions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(positions, plotArea);
    } else {
      // Draw grouped nodes
      foreach (var group in deploymentGroups) {
        var groupNodes = group.ToList();
        var groupWidth = Math.Max(180, groupNodes.Count * (nodeWidth + 15) + 30);
        var groupRect = new RectangleF(plotArea.Left + 20, groupY, groupWidth, nodeHeight + 50);

        // Draw deployment node boundary (with 3D effect)
        using (var brush = new SolidBrush(Color.FromArgb(20, Color.Gray)))
          g.FillRectangle(brush, groupRect);
        using (var pen = new Pen(Color.Gray, 1))
          g.DrawRectangle(pen, groupRect.X, groupRect.Y, groupRect.Width, groupRect.Height);

        using var groupFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.85f);
        using var groupBrush = new SolidBrush(Color.Gray);
        g.DrawString(group.Key, groupFont, groupBrush, groupRect.X + 5, groupRect.Y + 3);

        for (var i = 0; i < groupNodes.Count; ++i) {
          var node = groupNodes[i];
          var x = groupRect.X + 15 + i * (nodeWidth + 15);
          var y = groupRect.Y + 25;
          positions[node.Id] = new RectangleF(x, y, nodeWidth, nodeHeight);
        }

        groupY += groupRect.Height + 20;
      }

      // Position standalone nodes after groups
      if (standaloneNodes.Count > 0) {
        var standaloneX = plotArea.Left + 20;
        foreach (var node in standaloneNodes) {
          if (standaloneX + nodeWidth > plotArea.Right - 20) {
            standaloneX = plotArea.Left + 20;
            groupY += nodeHeight + 20;
          }
          positions[node.Id] = new RectangleF(standaloneX, groupY, nodeWidth, nodeHeight);
          standaloneX += nodeWidth + 20;
        }
      }
    }

    // Draw relationships
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var fromRect) ||
            !positions.TryGetValue(edge.Target, out var toRect))
          continue;

        var from = new PointF(fromRect.Left + fromRect.Width / 2, fromRect.Bottom);
        var to = new PointF(toRect.Left + toRect.Width / 2, toRect.Top);

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        this.DrawC4Relationship(g, from, to, edge.Label, null, context.Diagram.Font);
      }
    }

    // Draw nodes
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var rect))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      this.DrawC4Container(g, context, rect, node.Label ?? node.Id, null, node.Tag as string, color);
      context.RegisterHitTestRect(node, rect);
    }
  }
}

/// <summary>
/// Renderer for Git graph diagrams showing branches and commits.
/// </summary>
public class GitgraphDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Gitgraph;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var commits = context.Diagram.GitCommits;
    var branches = context.Diagram.GitBranches;

    if (commits == null || commits.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Build branch positions
    var branchPositions = new Dictionary<string, int>();
    var orderedBranches = branches?.OrderBy(b => b.Order).ToList() ?? new List<DiagramGitBranch>();
    var uniqueBranches = commits.Select(c => c.Branch).Distinct().ToList();

    for (var i = 0; i < orderedBranches.Count; ++i)
      branchPositions[orderedBranches[i].Name] = i;

    foreach (var branch in uniqueBranches)
      if (!branchPositions.ContainsKey(branch))
        branchPositions[branch] = branchPositions.Count;

    var branchSpacing = Math.Min(50f, (plotArea.Height - 40) / Math.Max(branchPositions.Count, 1));
    var commitSpacing = Math.Min(80f, (plotArea.Width - 100) / Math.Max(commits.Count, 1));
    var commitRadius = 10f;

    // Calculate commit positions
    var commitPositions = new Dictionary<string, PointF>();
    for (var i = 0; i < commits.Count; ++i) {
      var commit = commits[i];
      var branchIndex = branchPositions.ContainsKey(commit.Branch) ? branchPositions[commit.Branch] : 0;
      var x = plotArea.Left + 80 + i * commitSpacing;
      var y = plotArea.Top + 30 + branchIndex * branchSpacing;
      commitPositions[commit.Id] = new PointF(x, y);
    }

    // Draw branch labels
    foreach (var branch in branchPositions) {
      var y = plotArea.Top + 30 + branch.Value * branchSpacing;
      var branchColor = orderedBranches.FirstOrDefault(b => b.Name == branch.Key)?.Color ?? colors[branch.Value % colors.Length];

      using var brush = new SolidBrush(branchColor);
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f);
      g.DrawString(branch.Key, font, brush, plotArea.Left + 5, y - font.Height / 2);
    }

    // Draw branch lines
    foreach (var branch in branchPositions) {
      var y = plotArea.Top + 30 + branch.Value * branchSpacing;
      var branchColor = orderedBranches.FirstOrDefault(b => b.Name == branch.Key)?.Color ?? colors[branch.Value % colors.Length];

      using var pen = new Pen(Color.FromArgb(100, branchColor), 2);
      g.DrawLine(pen, plotArea.Left + 70, y, plotArea.Right - 10, y);
    }

    // Draw connections (parent links)
    foreach (var commit in commits) {
      if (!commitPositions.TryGetValue(commit.Id, out var pos))
        continue;

      foreach (var parentId in commit.ParentIds) {
        if (!commitPositions.TryGetValue(parentId, out var parentPos))
          continue;

        var branchColor = orderedBranches.FirstOrDefault(b => b.Name == commit.Branch)?.Color ?? colors[branchPositions.GetValueOrDefault(commit.Branch, 0) % colors.Length];

        using var pen = new Pen(branchColor, 2);

        // Animate
        var animatedPos = new PointF(
          parentPos.X + (pos.X - parentPos.X) * (float)context.AnimationProgress,
          parentPos.Y + (pos.Y - parentPos.Y) * (float)context.AnimationProgress
        );

        if (Math.Abs(pos.Y - parentPos.Y) < 1)
          g.DrawLine(pen, parentPos, animatedPos);
        else {
          // Draw curved line for branch/merge
          using var path = new GraphicsPath();
          path.AddBezier(parentPos,
            new PointF(parentPos.X + commitSpacing / 3, parentPos.Y),
            new PointF(animatedPos.X - commitSpacing / 3, animatedPos.Y),
            animatedPos);
          g.DrawPath(pen, path);
        }
      }
    }

    // Draw commits
    for (var i = 0; i < commits.Count; ++i) {
      var commit = commits[i];
      if (!commitPositions.TryGetValue(commit.Id, out var pos))
        continue;

      var branchColor = orderedBranches.FirstOrDefault(b => b.Name == commit.Branch)?.Color ?? colors[branchPositions.GetValueOrDefault(commit.Branch, 0) % colors.Length];
      var animatedRadius = commitRadius * (float)context.AnimationProgress;

      // Draw commit circle
      if (commit.Type == DiagramGitCommitType.Merge) {
        using var pen = new Pen(branchColor, 2);
        g.DrawEllipse(pen, pos.X - animatedRadius, pos.Y - animatedRadius, animatedRadius * 2, animatedRadius * 2);
      } else {
        using var brush = new SolidBrush(branchColor);
        g.FillEllipse(brush, pos.X - animatedRadius, pos.Y - animatedRadius, animatedRadius * 2, animatedRadius * 2);
      }

      // Draw commit ID
      using var idFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.7f);
      using var idBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(commit.Id, idFont, idBrush, pos.X - 15, pos.Y + animatedRadius + 2);

      // Draw tag if present
      if (!string.IsNullOrEmpty(commit.TagName)) {
        using var tagFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.75f, FontStyle.Bold);
        var tagSize = g.MeasureString(commit.TagName, tagFont);
        var tagRect = new RectangleF(pos.X + animatedRadius + 5, pos.Y - tagSize.Height / 2, tagSize.Width + 8, tagSize.Height + 4);

        using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
          FillRoundedRectangle(g, brush, tagRect, 3);

        using var tagBrush = new SolidBrush(Color.Black);
        g.DrawString(commit.TagName, tagFont, tagBrush, tagRect.X + 4, tagRect.Y + 2);
      }

      context.RegisterHitTestRect(commit, new RectangleF(pos.X - commitRadius, pos.Y - commitRadius, commitRadius * 2, commitRadius * 2));
    }
  }
}

/// <summary>
/// Renderer for Requirements diagrams.
/// </summary>
public class RequirementDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Requirement;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var requirements = context.Diagram.Requirements;
    var relations = context.Diagram.RequirementRelations;

    if (requirements == null || requirements.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Position requirements
    var reqPositions = new Dictionary<string, RectangleF>();
    var reqWidth = 180f;
    var reqHeight = 80f;

    // Check if positions are explicitly set
    var hasExplicitPositions = requirements.Any(r => r.Position.X > 0 || r.Position.Y > 0);

    if (hasExplicitPositions) {
      foreach (var req in requirements) {
        var x = plotArea.Left + 20 + req.Position.X / 100f * (plotArea.Width - reqWidth - 40);
        var y = plotArea.Top + 20 + req.Position.Y / 100f * (plotArea.Height - reqHeight - 40);
        reqPositions[req.Id] = new RectangleF(x, y, reqWidth, reqHeight);
      }
    } else {
      // Auto-layout in a grid pattern
      var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(requirements.Count)));
      var rows = (int)Math.Ceiling((float)requirements.Count / cols);
      var cellWidth = (plotArea.Width - 40) / Math.Max(cols, 1);
      var cellHeight = (plotArea.Height - 40) / Math.Max(rows, 1);

      for (var i = 0; i < requirements.Count; ++i) {
        var req = requirements[i];
        var col = i % cols;
        var row = i / cols;
        var x = plotArea.Left + 20 + col * cellWidth + (cellWidth - reqWidth) / 2;
        var y = plotArea.Top + 20 + row * cellHeight + (cellHeight - reqHeight) / 2;
        reqPositions[req.Id] = new RectangleF(x, y, reqWidth, reqHeight);
      }
    }

    // Draw relationships
    if (relations != null) {
      foreach (var rel in relations) {
        if (!reqPositions.TryGetValue(rel.From, out var fromRect) ||
            !reqPositions.TryGetValue(rel.To, out var toRect))
          continue;

        var from = new PointF(fromRect.Right, fromRect.Top + fromRect.Height / 2);
        var to = new PointF(toRect.Left, toRect.Top + toRect.Height / 2);

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        var relColor = this._GetRelationColor(rel.Type);
        using var pen = new Pen(relColor, 1.5f);
        pen.DashStyle = this._GetRelationDashStyle(rel.Type);
        pen.EndCap = LineCap.ArrowAnchor;
        g.DrawLine(pen, from, to);

        // Draw relation type label
        var midPoint = new PointF((from.X + to.X) / 2, (from.Y + to.Y) / 2);
        var label = $"<<{rel.Type}>>";
        using var labelFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.75f);
        var labelSize = g.MeasureString(label, labelFont);
        using var brush = new SolidBrush(relColor);
        g.DrawString(label, labelFont, brush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height - 3);
      }
    }

    // Draw requirements
    for (var i = 0; i < requirements.Count; ++i) {
      var req = requirements[i];
      if (!reqPositions.TryGetValue(req.Id, out var rect))
        continue;

      var color = req.Color ?? colors[i % colors.Length];
      var animatedRect = new RectangleF(
        rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
        rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw requirement box
      using (var brush = new SolidBrush(Color.White))
        g.FillRectangle(brush, animatedRect);

      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw stereotype
      var stereotype = $"<<{req.Type}>>";
      using var stereotypeFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.75f);
      var stereotypeSize = g.MeasureString(stereotype, stereotypeFont);
      using var stereotypeBrush = new SolidBrush(Color.Gray);
      g.DrawString(stereotype, stereotypeFont, stereotypeBrush, animatedRect.X + (animatedRect.Width - stereotypeSize.Width) / 2, animatedRect.Y + 3);

      // Draw name
      if (!string.IsNullOrEmpty(req.Name)) {
        using var nameFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.95f, FontStyle.Bold);
        var nameSize = g.MeasureString(req.Name, nameFont);
        using var nameBrush = new SolidBrush(context.Diagram.ForeColor);
        g.DrawString(req.Name, nameFont, nameBrush, animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, animatedRect.Y + 18);
      }

      // Draw ID
      using var idFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.75f);
      using var idBrush = new SolidBrush(Color.Gray);
      g.DrawString($"Id: {req.Id}", idFont, idBrush, animatedRect.X + 5, animatedRect.Y + 38);

      // Draw text (truncated)
      if (!string.IsNullOrEmpty(req.Text)) {
        using var textFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
        using var textBrush = new SolidBrush(context.Diagram.ForeColor);
        var textRect = new RectangleF(animatedRect.X + 5, animatedRect.Y + 52, animatedRect.Width - 10, 25);
        using var format = new StringFormat { Trimming = StringTrimming.EllipsisWord };
        g.DrawString(req.Text, textFont, textBrush, textRect, format);
      }

      context.RegisterHitTestRect(req, animatedRect);
    }
  }

  private Color _GetRelationColor(DiagramRequirementRelationType type) {
    return type switch {
      DiagramRequirementRelationType.Derives => Color.FromArgb(33, 150, 243),
      DiagramRequirementRelationType.Satisfies => Color.FromArgb(76, 175, 80),
      DiagramRequirementRelationType.Verifies => Color.FromArgb(156, 39, 176),
      DiagramRequirementRelationType.Refines => Color.FromArgb(255, 152, 0),
      DiagramRequirementRelationType.Traces => Color.FromArgb(158, 158, 158),
      _ => Color.FromArgb(80, 80, 80)
    };
  }

  private DashStyle _GetRelationDashStyle(DiagramRequirementRelationType type) {
    return type switch {
      DiagramRequirementRelationType.Traces => DashStyle.Dash,
      DiagramRequirementRelationType.Copies => DashStyle.Dot,
      _ => DashStyle.Solid
    };
  }
}
