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
/// Renderer for UML class diagrams with compartmented class boxes.
/// </summary>
public class ClassDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.ClassDiagram;

  /// <summary>Class box width.</summary>
  public float ClassWidth { get; set; } = 150;

  /// <summary>Compartment height for header.</summary>
  public float HeaderHeight { get; set; } = 25;

  /// <summary>Line height for members.</summary>
  public float MemberLineHeight { get; set; } = 18;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var classNodes = context.Diagram.ClassNodes;
    var classRelations = context.Diagram.ClassRelations;

    if (classNodes == null || classNodes.Count == 0)
      return;

    // Position classes in a grid if not positioned
    var positions = new Dictionary<string, RectangleF>();

    // Check if positions are explicitly set
    var hasExplicitPositions = classNodes.Any(cls => cls.Position != PointF.Empty);

    // Calculate max height for uniform grid cells
    var maxMemberCount = classNodes.Max(cls => (cls.Fields?.Count ?? 0) + (cls.Methods?.Count ?? 0));
    var maxHeight = this.HeaderHeight + maxMemberCount * this.MemberLineHeight + 20;

    if (hasExplicitPositions) {
      for (var i = 0; i < classNodes.Count; ++i) {
        var cls = classNodes[i];
        var memberCount = (cls.Fields?.Count ?? 0) + (cls.Methods?.Count ?? 0);
        var height = this.HeaderHeight + memberCount * this.MemberLineHeight + 10;

        float x, y;
        if (cls.Position != PointF.Empty) {
          x = context.PlotArea.Left + cls.Position.X / 100f * context.PlotArea.Width;
          y = context.PlotArea.Top + cls.Position.Y / 100f * context.PlotArea.Height;
        } else {
          x = context.PlotArea.Left + context.PlotArea.Width / 2 - this.ClassWidth / 2;
          y = context.PlotArea.Top + context.PlotArea.Height / 2 - height / 2;
        }
        positions[cls.Id] = new RectangleF(x, y, this.ClassWidth, height);
      }
    } else {
      // Auto-layout: use proper grid with margins to avoid overlap
      var margin = 30f;
      var gridLayout = CalculateGridLayout(classNodes.Count, this.ClassWidth, maxHeight, margin, context.PlotArea);

      for (var i = 0; i < classNodes.Count; ++i) {
        var cls = classNodes[i];
        var memberCount = (cls.Fields?.Count ?? 0) + (cls.Methods?.Count ?? 0);
        var height = this.HeaderHeight + memberCount * this.MemberLineHeight + 10;

        if (gridLayout.TryGetValue(i, out var gridRect)) {
          // Use grid position but actual height
          positions[cls.Id] = new RectangleF(gridRect.X, gridRect.Y, this.ClassWidth, height);
        }
      }

      // Minimize edge crossings for grid layout
      if (classRelations != null && classRelations.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          classNodes,
          c => c.Id,
          c => classRelations.Where(r => r.From == c.Id).Select(r => r.To)
               .Concat(classRelations.Where(r => r.To == c.Id).Select(r => r.From)),
          positions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(positions, context.PlotArea);
    }

    var colors = GetDefaultColors();

    // Draw relationships first (behind classes)
    if (classRelations != null) {
      foreach (var relation in classRelations) {
        if (!positions.ContainsKey(relation.From) || !positions.ContainsKey(relation.To))
          continue;

        var fromRect = positions[relation.From];
        var toRect = positions[relation.To];

        var fromCenter = new PointF(fromRect.X + fromRect.Width / 2, fromRect.Y + fromRect.Height / 2);
        var toCenter = new PointF(toRect.X + toRect.Width / 2, toRect.Y + toRect.Height / 2);

        // Find edge points
        var fromEdge = this._GetRectEdgePoint(fromRect, toCenter);
        var toEdge = this._GetRectEdgePoint(toRect, fromCenter);

        // Apply animation
        toEdge = new PointF(
          fromEdge.X + (toEdge.X - fromEdge.X) * (float)context.AnimationProgress,
          fromEdge.Y + (toEdge.Y - fromEdge.Y) * (float)context.AnimationProgress
        );

        var lineColor = Color.Black;
        this._DrawRelationship(g, fromEdge, toEdge, relation.RelationType, lineColor, relation.Label);
      }
    }

    // Draw classes
    for (var i = 0; i < classNodes.Count; ++i) {
      var cls = classNodes[i];
      if (!positions.ContainsKey(cls.Id))
        continue;

      var rect = positions[cls.Id];
      var color = cls.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Skip if too small
      if (animatedRect.Width < 1 || animatedRect.Height < 1)
        continue;

      // Draw class box
      using (var brush = new SolidBrush(Color.White))
        g.FillRectangle(brush, animatedRect);

      using (var pen = new Pen(Color.Black, 1))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      var y = animatedRect.Y;

      // Draw header with stereotype
      var headerRect = new RectangleF(animatedRect.X, y, animatedRect.Width, this.HeaderHeight);
      using (var headerBrush = new SolidBrush(Lighten(color, 0.7f)))
        g.FillRectangle(headerBrush, headerRect);

      // Stereotype
      if (!string.IsNullOrEmpty(cls.Stereotype)) {
        var stereoText = $"<<{cls.Stereotype}>>";
        var stereoSize = g.MeasureString(stereoText, context.Diagram.Font);
        using var stereoFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f, FontStyle.Italic);
        using var stereoBrush = new SolidBrush(Color.Gray);
        g.DrawString(stereoText, stereoFont, stereoBrush, animatedRect.X + (animatedRect.Width - stereoSize.Width) / 2, y + 2);
      }

      // Class name
      using var nameFont = new Font(context.Diagram.Font, FontStyle.Bold);
      var className = cls.ClassName ?? cls.Id;
      var nameSize = g.MeasureString(className, nameFont);
      using (var nameBrush = new SolidBrush(Color.Black))
        g.DrawString(className, nameFont, nameBrush, animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, y + this.HeaderHeight - nameSize.Height - 2);

      y += this.HeaderHeight;

      // Draw separator
      using (var pen = new Pen(Color.Black, 1))
        g.DrawLine(pen, animatedRect.X, y, animatedRect.Right, y);

      // Draw fields
      if (cls.Fields != null && cls.Fields.Count > 0) {
        foreach (var field in cls.Fields) {
          var visibility = this._GetVisibilitySymbol(field.Visibility);
          var staticPrefix = field.IsStatic ? "«static» " : "";
          var text = $"{visibility} {staticPrefix}{field.Name}: {field.Type}";

          using var textBrush = new SolidBrush(Color.Black);
          g.DrawString(text, context.Diagram.Font, textBrush, animatedRect.X + 5, y + 2);
          y += this.MemberLineHeight;
        }
      }

      // Draw separator
      using (var pen = new Pen(Color.Black, 1))
        g.DrawLine(pen, animatedRect.X, y, animatedRect.Right, y);

      // Draw methods
      if (cls.Methods != null && cls.Methods.Count > 0) {
        foreach (var method in cls.Methods) {
          var visibility = this._GetVisibilitySymbol(method.Visibility);
          var staticPrefix = method.IsStatic ? "«static» " : "";
          var abstractPrefix = method.IsAbstract ? "«abstract» " : "";
          var text = $"{visibility} {staticPrefix}{abstractPrefix}{method.Name}(): {method.Type}";

          using var textBrush = new SolidBrush(Color.Black);
          if (method.IsAbstract) {
            using var methodFont = new Font(context.Diagram.Font, FontStyle.Italic);
            g.DrawString(text, methodFont, textBrush, animatedRect.X + 5, y + 2);
          } else
            g.DrawString(text, context.Diagram.Font, textBrush, animatedRect.X + 5, y + 2);
          y += this.MemberLineHeight;
        }
      }

      context.RegisterHitTestRect(cls, animatedRect);
    }
  }

  private string _GetVisibilitySymbol(DiagramVisibility visibility) => visibility switch {
    DiagramVisibility.Public => "+",
    DiagramVisibility.Private => "-",
    DiagramVisibility.Protected => "#",
    DiagramVisibility.Internal => "~",
    DiagramVisibility.Package => "~",
    _ => "+"
  };

  private PointF _GetRectEdgePoint(RectangleF rect, PointF target) {
    var center = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    var dx = target.X - center.X;
    var dy = target.Y - center.Y;

    if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
      return center;

    var scale = Math.Min(
      Math.Abs(rect.Width / 2 / (Math.Abs(dx) < 0.001 ? 1 : dx)),
      Math.Abs(rect.Height / 2 / (Math.Abs(dy) < 0.001 ? 1 : dy))
    );

    return new PointF(center.X + dx * (float)scale, center.Y + dy * (float)scale);
  }

  private void _DrawRelationship(Graphics g, PointF from, PointF to, DiagramRelationType relationType, Color color, string label) {
    using var pen = new Pen(color, 1);

    switch (relationType) {
      case DiagramRelationType.Inheritance:
        pen.CustomEndCap = new AdjustableArrowCap(8, 8, false);
        g.DrawLine(pen, from, to);
        break;

      case DiagramRelationType.Implementation:
        pen.DashStyle = DashStyle.Dash;
        pen.CustomEndCap = new AdjustableArrowCap(8, 8, false);
        g.DrawLine(pen, from, to);
        break;

      case DiagramRelationType.Association:
        g.DrawLine(pen, from, to);
        break;

      case DiagramRelationType.Aggregation:
        g.DrawLine(pen, from, to);
        this._DrawDiamond(g, to, from, false, color);
        break;

      case DiagramRelationType.Composition:
        g.DrawLine(pen, from, to);
        this._DrawDiamond(g, to, from, true, color);
        break;

      case DiagramRelationType.Dependency:
        pen.DashStyle = DashStyle.Dash;
        pen.EndCap = LineCap.ArrowAnchor;
        g.DrawLine(pen, from, to);
        break;

      default:
        g.DrawLine(pen, from, to);
        break;
    }

    // Draw label
    if (!string.IsNullOrEmpty(label)) {
      var midPoint = new PointF((from.X + to.X) / 2, (from.Y + to.Y) / 2);
      var labelSize = g.MeasureString(label, SystemFonts.DefaultFont);

      using var bgBrush = new SolidBrush(Color.White);
      g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);

      using var labelBrush = new SolidBrush(Color.Black);
      g.DrawString(label, SystemFonts.DefaultFont, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
    }
  }

  private void _DrawDiamond(Graphics g, PointF point, PointF direction, bool filled, Color color) {
    var dx = direction.X - point.X;
    var dy = direction.Y - point.Y;
    var length = (float)Math.Sqrt(dx * dx + dy * dy);
    if (length < 1)
      return;

    dx /= length;
    dy /= length;

    var size = 10f;
    var points = new[] {
      point,
      new PointF(point.X + size / 2 * dy + size * dx, point.Y - size / 2 * dx + size * dy),
      new PointF(point.X + size * 2 * dx, point.Y + size * 2 * dy),
      new PointF(point.X - size / 2 * dy + size * dx, point.Y + size / 2 * dx + size * dy)
    };

    if (filled) {
      using var brush = new SolidBrush(color);
      g.FillPolygon(brush, points);
    } else {
      using var brush = new SolidBrush(Color.White);
      g.FillPolygon(brush, points);
    }

    using var pen = new Pen(color, 1);
    g.DrawPolygon(pen, points);
  }
}

/// <summary>
/// Renderer for UML sequence diagrams with lifelines and messages.
/// </summary>
public class SequenceDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.SequenceDiagram;

  /// <summary>Lifeline header height.</summary>
  public float HeaderHeight { get; set; } = 40;

  /// <summary>Lifeline spacing.</summary>
  public float LifelineSpacing { get; set; } = 120;

  /// <summary>Message vertical spacing.</summary>
  public float MessageSpacing { get; set; } = 40;

  /// <summary>Activation box width.</summary>
  public float ActivationWidth { get; set; } = 12;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var lifelines = context.Diagram.Lifelines;
    var messages = context.Diagram.Messages;

    if (lifelines == null || lifelines.Count == 0)
      return;

    var colors = GetDefaultColors();

    // Calculate required dimensions
    var requiredWidth = lifelines.Count * this.LifelineSpacing + 100;
    var requiredHeight = this.HeaderHeight + 30 + (messages?.Count ?? 0) * this.MessageSpacing + 40;

    // Calculate scale factor to fit within plot area (scale both in and out)
    var scaleX = context.PlotArea.Width / requiredWidth;
    var scaleY = context.PlotArea.Height / requiredHeight;
    var scale = Math.Min(scaleX, scaleY); // Scale both in and out

    // Apply effective spacing based on scale
    var effectiveLifelineSpacing = this.LifelineSpacing * scale;
    var effectiveMessageSpacing = this.MessageSpacing * scale;
    var effectiveHeaderHeight = this.HeaderHeight * scale;

    // Calculate centered starting position
    var actualWidth = lifelines.Count * effectiveLifelineSpacing;
    var startX = context.PlotArea.Left + (context.PlotArea.Width - actualWidth) / 2 + effectiveLifelineSpacing / 2;

    // Position lifelines with auto-fit
    var lifelinePositions = new Dictionary<string, float>();
    var x = startX;

    foreach (var lifeline in lifelines) {
      lifelinePositions[lifeline.Id] = x;
      x += effectiveLifelineSpacing;
    }

    var headerY = context.PlotArea.Top + 10;
    var lifelineTop = headerY + effectiveHeaderHeight + 10;
    var lifelineBottom = context.PlotArea.Bottom - 20;

    // Draw lifeline headers and lines
    for (var i = 0; i < lifelines.Count; ++i) {
      var lifeline = lifelines[i];
      var lifelineX = lifelinePositions[lifeline.Id];
      var color = lifeline.Color ?? colors[i % colors.Length];

      // Draw header box or actor
      var headerBoxWidth = 80 * scale;
      if (lifeline.Stereotype == "actor") {
        this._DrawActor(g, new PointF(lifelineX, headerY + effectiveHeaderHeight / 2), 15 * scale, color);
      } else {
        var headerRect = new RectangleF(lifelineX - headerBoxWidth / 2, headerY, headerBoxWidth, effectiveHeaderHeight);
        using (var brush = new SolidBrush(Lighten(color, 0.7f)))
          g.FillRectangle(brush, headerRect);
        using (var pen = new Pen(color, 1))
          g.DrawRectangle(pen, headerRect.X, headerRect.Y, headerRect.Width, headerRect.Height);
      }

      // Draw name
      var name = lifeline.Name ?? lifeline.Id;
      var nameSize = g.MeasureString(name, context.Diagram.Font);
      using (var nameBrush = new SolidBrush(Color.Black))
        g.DrawString(name, context.Diagram.Font, nameBrush, lifelineX - nameSize.Width / 2, headerY + (effectiveHeaderHeight - nameSize.Height) / 2);

      // Draw lifeline (dashed vertical line)
      using var pen2 = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
      g.DrawLine(pen2, lifelineX, lifelineTop, lifelineX, lifelineBottom * (float)context.AnimationProgress);
    }

    // Draw messages
    if (messages != null && messages.Count > 0) {
      var messageY = lifelineTop + 20;

      foreach (var message in messages.OrderBy(m => m.SequenceNumber)) {
        if (!lifelinePositions.ContainsKey(message.From) || !lifelinePositions.ContainsKey(message.To))
          continue;

        var fromX = lifelinePositions[message.From];
        var toX = lifelinePositions[message.To];

        // Apply animation
        var animatedToX = fromX + (toX - fromX) * (float)context.AnimationProgress;

        var messageColor = message.MessageType == DiagramMessageType.Return ? Color.Gray : Color.Black;

        using var pen = new Pen(messageColor, 1);

        switch (message.MessageType) {
          case DiagramMessageType.Async:
            pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(pen, fromX, messageY, animatedToX, messageY);
            break;

          case DiagramMessageType.Return:
            pen.DashStyle = DashStyle.Dash;
            pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(pen, fromX, messageY, animatedToX, messageY);
            break;

          case DiagramMessageType.Create:
            pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(pen, fromX, messageY, animatedToX, messageY);
            // Draw <<create>> stereotype
            using (var createBrush = new SolidBrush(Color.Gray)) {
              var createText = "<<create>>";
              var createSize = g.MeasureString(createText, context.Diagram.Font);
              g.DrawString(createText, context.Diagram.Font, createBrush, (fromX + toX) / 2 - createSize.Width / 2, messageY - createSize.Height - 2);
            }
            break;

          case DiagramMessageType.Destroy:
            pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(pen, fromX, messageY, animatedToX, messageY);
            // Draw X at destination
            using (var destroyPen = new Pen(Color.Red, 2)) {
              g.DrawLine(destroyPen, toX - 8, messageY - 8, toX + 8, messageY + 8);
              g.DrawLine(destroyPen, toX - 8, messageY + 8, toX + 8, messageY - 8);
            }
            break;

          case DiagramMessageType.SelfCall:
            // Self-call loops back
            var selfCallOffset = 30 * scale;
            var selfCallHeight = 20 * scale;
            using (var selfPath = new GraphicsPath()) {
              selfPath.AddLine(fromX, messageY, fromX + selfCallOffset, messageY);
              selfPath.AddLine(fromX + selfCallOffset, messageY, fromX + selfCallOffset, messageY + selfCallHeight);
              selfPath.AddLine(fromX + selfCallOffset, messageY + selfCallHeight, fromX, messageY + selfCallHeight);
              pen.EndCap = LineCap.ArrowAnchor;
              g.DrawPath(pen, selfPath);
            }
            messageY += selfCallHeight;
            break;

          default: // Sync
            pen.EndCap = LineCap.ArrowAnchor;
            pen.CustomEndCap = new AdjustableArrowCap(5, 5, true);
            g.DrawLine(pen, fromX, messageY, animatedToX, messageY);
            break;
        }

        // Draw message label
        if (!string.IsNullOrEmpty(message.Label)) {
          var labelSize = g.MeasureString(message.Label, context.Diagram.Font);
          var labelX = Math.Min(fromX, toX) + Math.Abs(toX - fromX) / 2 - labelSize.Width / 2;

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(message.Label, context.Diagram.Font, labelBrush, labelX, messageY - labelSize.Height - 2);
        }

        messageY += effectiveMessageSpacing;
      }
    }
  }

  private void _DrawActor(Graphics g, PointF center, float size, Color color) {
    using var pen = new Pen(color, 2);

    // Head
    g.DrawEllipse(pen, center.X - size / 3, center.Y - size, size * 2 / 3, size * 2 / 3);

    // Body
    g.DrawLine(pen, center.X, center.Y - size / 3, center.X, center.Y + size / 2);

    // Arms
    g.DrawLine(pen, center.X - size / 2, center.Y, center.X + size / 2, center.Y);

    // Legs
    g.DrawLine(pen, center.X, center.Y + size / 2, center.X - size / 2, center.Y + size);
    g.DrawLine(pen, center.X, center.Y + size / 2, center.X + size / 2, center.Y + size);
  }
}

/// <summary>
/// Renderer for UML state diagrams.
/// </summary>
public class StateDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.StateDiagram;

  /// <summary>State width.</summary>
  public float StateWidth { get; set; } = 100;

  /// <summary>State height.</summary>
  public float StateHeight { get; set; } = 50;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var nodePositions = new Dictionary<string, RectangleF>();

    // Check if positions are explicitly set
    var hasExplicitPositions = nodes.Any(n => n.Position != PointF.Empty && (n.Position.X > 0 || n.Position.Y > 0));

    if (hasExplicitPositions) {
      // Use explicit positions
      foreach (var node in nodes) {
        var x = context.PlotArea.Left + node.Position.X / 100f * context.PlotArea.Width;
        var y = context.PlotArea.Top + node.Position.Y / 100f * context.PlotArea.Height;
        nodePositions[node.Id] = new RectangleF(x - this.StateWidth / 2, y - this.StateHeight / 2, this.StateWidth, this.StateHeight);
      }
    } else if (nodes.Count <= 12) {
      // Circular layout for up to 12 nodes
      var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
      var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;

      // Calculate minimum radius needed to avoid overlap
      // Arc length between adjacent nodes must be > node diagonal
      var nodeDiagonal = (float)Math.Sqrt(this.StateWidth * this.StateWidth + this.StateHeight * this.StateHeight);
      var minSpacing = nodeDiagonal + 20; // Add padding
      var circumference = nodes.Count * minSpacing;
      var minRadius = circumference / (2 * (float)Math.PI);

      // Use the larger of minimum radius or 60% of available space
      var availableRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 - nodeDiagonal / 2;
      var radius = Math.Max(minRadius, availableRadius * 0.6f);

      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        var angle = 2 * Math.PI * i / nodes.Count - Math.PI / 2;
        var x = centerX + (float)(Math.Cos(angle) * radius);
        var y = centerY + (float)(Math.Sin(angle) * radius);
        nodePositions[node.Id] = new RectangleF(x - this.StateWidth / 2, y - this.StateHeight / 2, this.StateWidth, this.StateHeight);
      }
    } else {
      // Grid layout for many nodes - ensures no overlap
      var cols = (int)Math.Ceiling(Math.Sqrt(nodes.Count));
      var rows = (int)Math.Ceiling((float)nodes.Count / cols);

      // Calculate cell size with padding
      var cellWidth = this.StateWidth + 40;
      var cellHeight = this.StateHeight + 40;

      // Calculate total grid size
      var gridWidth = cols * cellWidth;
      var gridHeight = rows * cellHeight;

      // Center the grid in the plot area (may extend beyond for zoom)
      var startX = context.PlotArea.Left + (context.PlotArea.Width - gridWidth) / 2 + cellWidth / 2;
      var startY = context.PlotArea.Top + (context.PlotArea.Height - gridHeight) / 2 + cellHeight / 2;

      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        var col = i % cols;
        var row = i / cols;
        var x = startX + col * cellWidth;
        var y = startY + row * cellHeight;
        nodePositions[node.Id] = new RectangleF(x - this.StateWidth / 2, y - this.StateHeight / 2, this.StateWidth, this.StateHeight);
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          nodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          nodePositions);
      }
    }

    // Adjust layout to fit within plot area
    if (!hasExplicitPositions)
      AdjustRectangleLayoutToFit(nodePositions, context.PlotArea);

    var colors = GetDefaultColors();

    // Draw transitions
    if (edges != null) {
      foreach (var edge in edges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var sourceRect = nodePositions[edge.Source];
        var targetRect = nodePositions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        var color = edge.Color ?? Color.Black;
        using var pen = new Pen(color, 1);
        pen.EndCap = LineCap.ArrowAnchor;
        pen.CustomEndCap = new AdjustableArrowCap(5, 5, true);

        // Draw curved arrow for self-transitions
        if (edge.Source == edge.Target) {
          var selfRect = new RectangleF(sourceRect.Right - 10, sourceRect.Top - 30, 40, 40);
          g.DrawArc(pen, selfRect, 180, 270);
        } else {
          g.DrawLine(pen, sourceCenter, targetCenter);
        }

        // Draw transition label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2 - 10);
          var labelSize = g.MeasureString(edge.Label, context.Diagram.Font);

          using var bgBrush = new SolidBrush(Color.White);
          g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2 - 2, midPoint.Y - labelSize.Height / 2 - 2, labelSize.Width + 4, labelSize.Height + 4);

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(edge.Label, context.Diagram.Font, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }

    // Draw states
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var rect = nodePositions[node.Id];
      var color = node.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw based on shape
      switch (node.Shape) {
        case DiagramNodeShape.InitialState:
          // Filled circle for initial state
          var initialSize = Math.Min(animatedRect.Width, animatedRect.Height) * 0.3f;
          using (var brush = new SolidBrush(Color.Black))
            g.FillEllipse(brush, animatedRect.X + animatedRect.Width / 2 - initialSize / 2,
              animatedRect.Y + animatedRect.Height / 2 - initialSize / 2, initialSize, initialSize);
          break;

        case DiagramNodeShape.FinalState:
          // Double circle for final state
          var finalOuter = Math.Min(animatedRect.Width, animatedRect.Height) * 0.4f;
          var finalInner = finalOuter * 0.6f;
          using (var pen = new Pen(Color.Black, 2))
            g.DrawEllipse(pen, animatedRect.X + animatedRect.Width / 2 - finalOuter / 2,
              animatedRect.Y + animatedRect.Height / 2 - finalOuter / 2, finalOuter, finalOuter);
          using (var brush = new SolidBrush(Color.Black))
            g.FillEllipse(brush, animatedRect.X + animatedRect.Width / 2 - finalInner / 2,
              animatedRect.Y + animatedRect.Height / 2 - finalInner / 2, finalInner, finalInner);
          break;

        default:
          // Regular state: rounded rectangle
          using (var brush = new SolidBrush(Lighten(color, 0.7f)))
            FillRoundedRectangle(g, brush, animatedRect, 15);
          using (var pen = new Pen(color, 2))
            DrawRoundedRectangle(g, pen, animatedRect, 15);

          // Draw label
          var label = node.Label ?? node.Id;
          var labelSize = g.MeasureString(label, context.Diagram.Font);
          using (var labelBrush = new SolidBrush(Color.Black))
            g.DrawString(label, context.Diagram.Font, labelBrush,
              animatedRect.X + (animatedRect.Width - labelSize.Width) / 2,
              animatedRect.Y + (animatedRect.Height - labelSize.Height) / 2);
          break;
      }

      context.RegisterHitTestRect(node, animatedRect);
    }
  }
}

/// <summary>
/// Renderer for UML use case diagrams.
/// </summary>
public class UseCaseDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.UseCaseDiagram;

  /// <summary>Use case oval width.</summary>
  public float UseCaseWidth { get; set; } = 120;

  /// <summary>Use case oval height.</summary>
  public float UseCaseHeight { get; set; } = 50;

  /// <summary>Actor size.</summary>
  public float ActorSize { get; set; } = 40;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var actors = context.Diagram.Actors;
    var useCases = context.Diagram.UseCases;
    var edges = context.Diagram.Edges;

    var colors = GetDefaultColors();
    var positions = new Dictionary<string, RectangleF>();

    // Position actors on left side
    if (actors != null && actors.Count > 0) {
      var actorY = context.PlotArea.Top + 50;
      var actorSpacing = (context.PlotArea.Height - 100) / Math.Max(1, actors.Count);

      // Scale actors to fit spacing (scale both in and out)
      var requiredHeight = this.ActorSize * 1.5f + 20; // Height of actor + name + margin
      var actorScale = actorSpacing / requiredHeight;
      var effectiveActorSize = this.ActorSize * actorScale;

      foreach (var actor in actors) {
        var x = context.PlotArea.Left + 50;
        positions[actor.Id] = new RectangleF(x - effectiveActorSize / 2, actorY, effectiveActorSize, effectiveActorSize * 1.5f);

        // Draw actor stick figure
        var centerX = x;
        var centerY = actorY + effectiveActorSize * 0.5f;

        using var pen = new Pen(Color.Black, 2);

        // Apply animation (combine with actor scale)
        var scale = (float)context.AnimationProgress * actorScale;

        // Head
        g.DrawEllipse(pen, centerX - 8 * scale, centerY - 20 * scale, 16 * scale, 16 * scale);
        // Body
        g.DrawLine(pen, centerX, centerY - 4 * scale, centerX, centerY + 15 * scale);
        // Arms
        g.DrawLine(pen, centerX - 15 * scale, centerY + 5 * scale, centerX + 15 * scale, centerY + 5 * scale);
        // Legs
        g.DrawLine(pen, centerX, centerY + 15 * scale, centerX - 12 * scale, centerY + 35 * scale);
        g.DrawLine(pen, centerX, centerY + 15 * scale, centerX + 12 * scale, centerY + 35 * scale);

        // Name
        var name = actor.Name ?? actor.Id;
        var nameSize = g.MeasureString(name, context.Diagram.Font);
        using var nameBrush = new SolidBrush(Color.Black);
        g.DrawString(name, context.Diagram.Font, nameBrush, centerX - nameSize.Width / 2, centerY + 40 * scale);

        actorY += actorSpacing;
      }
    }

    // Draw system boundary
    var systemRect = new RectangleF(
      context.PlotArea.Left + 120,
      context.PlotArea.Top + 20,
      context.PlotArea.Width - 170,
      context.PlotArea.Height - 40
    );

    using (var boundaryPen = new Pen(Color.Black, 1))
      g.DrawRectangle(boundaryPen, systemRect.X, systemRect.Y, systemRect.Width, systemRect.Height);

    using (var boundaryBrush = new SolidBrush(Color.Black))
      g.DrawString("System", context.Diagram.Font, boundaryBrush, systemRect.X + 5, systemRect.Y + 5);

    // Position use cases inside system boundary
    if (useCases != null && useCases.Count > 0) {
      var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(useCases.Count)));
      var rows = Math.Max(1, (int)Math.Ceiling((double)useCases.Count / cols));
      var cellWidth = systemRect.Width / cols;
      var cellHeight = (systemRect.Height - 30) / rows; // Account for "System" label

      // Calculate scale factor to fit (scale both in and out)
      var margin = 10f;
      var scaleX = cellWidth / (this.UseCaseWidth + margin);
      var scaleY = cellHeight / (this.UseCaseHeight + margin);
      var useCaseScale = Math.Min(scaleX, scaleY); // Scale both in and out

      var effectiveWidth = this.UseCaseWidth * useCaseScale;
      var effectiveHeight = this.UseCaseHeight * useCaseScale;

      for (var i = 0; i < useCases.Count; ++i) {
        var useCase = useCases[i];
        var col = i % cols;
        var row = i / cols;

        var x = systemRect.Left + col * cellWidth + (cellWidth - effectiveWidth) / 2;
        var y = systemRect.Top + 25 + row * cellHeight + (cellHeight - effectiveHeight) / 2;

        var rect = new RectangleF(x, y, effectiveWidth, effectiveHeight);
        positions[useCase.Id] = rect;

        // Apply animation
        var animatedRect = new RectangleF(
          rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
          rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
          rect.Width * (float)context.AnimationProgress,
          rect.Height * (float)context.AnimationProgress
        );

        // Draw use case oval
        using (var brush = new SolidBrush(Lighten(colors[i % colors.Length], 0.8f)))
          g.FillEllipse(brush, animatedRect);
        using (var pen = new Pen(colors[i % colors.Length], 1))
          g.DrawEllipse(pen, animatedRect);

        // Draw name
        var name = useCase.Name ?? useCase.Id;
        var nameSize = g.MeasureString(name, context.Diagram.Font);
        using var nameBrush = new SolidBrush(Color.Black);
        g.DrawString(name, context.Diagram.Font, nameBrush,
          animatedRect.X + (animatedRect.Width - nameSize.Width) / 2,
          animatedRect.Y + (animatedRect.Height - nameSize.Height) / 2);
      }
    }

    // Draw associations
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.ContainsKey(edge.Source) || !positions.ContainsKey(edge.Target))
          continue;

        var sourceRect = positions[edge.Source];
        var targetRect = positions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);

        // Handle <<include>> and <<extend>> stereotypes
        if (!string.IsNullOrEmpty(edge.Label)) {
          if (edge.Label.Contains("include") || edge.Label.Contains("extend"))
            pen.DashStyle = DashStyle.Dash;
          pen.EndCap = LineCap.ArrowAnchor;
        }

        g.DrawLine(pen, sourceCenter, targetCenter);

        // Draw label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2);
          var labelSize = g.MeasureString(edge.Label, context.Diagram.Font);

          using var bgBrush = new SolidBrush(Color.White);
          g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(edge.Label, context.Diagram.Font, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for UML activity diagrams with swimlanes.
/// </summary>
public class ActivityDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.ActivityDiagram;

  /// <summary>Activity node width.</summary>
  public float ActivityWidth { get; set; } = 100;

  /// <summary>Activity node height.</summary>
  public float ActivityHeight { get; set; } = 40;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;
    var swimlanes = context.Diagram.Swimlanes;

    var colors = GetDefaultColors();
    var nodePositions = new Dictionary<string, RectangleF>();

    // Draw swimlanes if present
    if (swimlanes != null && swimlanes.Count > 0) {
      var laneWidth = context.PlotArea.Width / swimlanes.Count;

      for (var i = 0; i < swimlanes.Count; ++i) {
        var lane = swimlanes[i];
        var laneRect = new RectangleF(
          context.PlotArea.Left + i * laneWidth,
          context.PlotArea.Top,
          laneWidth,
          context.PlotArea.Height
        );

        // Draw lane background
        var laneColor = lane.Color ?? Lighten(colors[i % colors.Length], 0.9f);
        using (var brush = new SolidBrush(laneColor))
          g.FillRectangle(brush, laneRect);

        // Draw lane border
        using (var pen = new Pen(Color.Gray, 1))
          g.DrawRectangle(pen, laneRect.X, laneRect.Y, laneRect.Width, laneRect.Height);

        // Draw lane header
        var headerRect = new RectangleF(laneRect.X, laneRect.Y, laneRect.Width, 25);
        using (var headerBrush = new SolidBrush(Darken(laneColor, 0.1f)))
          g.FillRectangle(headerBrush, headerRect);

        var name = lane.Name ?? lane.Id;
        var nameSize = g.MeasureString(name, context.Diagram.Font);
        using (var nameBrush = new SolidBrush(Color.Black))
          g.DrawString(name, context.Diagram.Font, nameBrush,
            headerRect.X + (headerRect.Width - nameSize.Width) / 2,
            headerRect.Y + (headerRect.Height - nameSize.Height) / 2);
      }
    }

    // Position nodes
    if (nodes != null && nodes.Count > 0) {
      // Check if positions are explicitly set
      var hasExplicitPositions = nodes.Any(n => n.Position != PointF.Empty);

      if (hasExplicitPositions) {
        // Use explicit positions
        foreach (var node in nodes) {
          float x, y;
          if (node.Position != PointF.Empty) {
            x = context.PlotArea.Left + node.Position.X / 100f * context.PlotArea.Width;
            y = context.PlotArea.Top + node.Position.Y / 100f * context.PlotArea.Height;
          } else {
            x = context.PlotArea.Left + context.PlotArea.Width / 2;
            y = context.PlotArea.Top + context.PlotArea.Height / 2;
          }
          nodePositions[node.Id] = new RectangleF(x - this.ActivityWidth / 2, y - this.ActivityHeight / 2, this.ActivityWidth, this.ActivityHeight);
        }
      } else {
        // Auto-layout: use proper grid with margins to avoid overlap
        var margin = 30f;
        var availablePlotArea = swimlanes is { Count: > 0 }
          ? new RectangleF(context.PlotArea.Left, context.PlotArea.Top + 30, context.PlotArea.Width, context.PlotArea.Height - 30)
          : context.PlotArea;

        var gridLayout = CalculateGridLayout(nodes.Count, this.ActivityWidth, this.ActivityHeight, margin, availablePlotArea);
        for (var i = 0; i < nodes.Count; ++i) {
          var node = nodes[i];
          if (gridLayout.TryGetValue(i, out var rect))
            nodePositions[node.Id] = rect;
        }

        // Minimize edge crossings for grid layout
        if (edges != null && edges.Count > 0) {
          MinimizeEdgeCrossingsForGrid(
            nodes,
            n => n.Id,
            n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
                 .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
            nodePositions);
        }

        // Adjust layout to fit within plot area
        AdjustRectangleLayoutToFit(nodePositions, availablePlotArea);
      }
    }

    // Draw edges
    if (edges != null) {
      foreach (var edge in edges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var sourceRect = nodePositions[edge.Source];
        var targetRect = nodePositions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Bottom);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Top);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        pen.EndCap = LineCap.ArrowAnchor;
        pen.CustomEndCap = new AdjustableArrowCap(5, 5, true);
        g.DrawLine(pen, sourceCenter, targetCenter);

        // Guard condition label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2);
          var guardText = $"[{edge.Label}]";
          var labelSize = g.MeasureString(guardText, context.Diagram.Font);

          using var bgBrush = new SolidBrush(Color.White);
          g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);

          using var labelBrush = new SolidBrush(Color.Black);
          g.DrawString(guardText, context.Diagram.Font, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }

    // Draw nodes
    if (nodes != null) {
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        if (!nodePositions.ContainsKey(node.Id))
          continue;

        var rect = nodePositions[node.Id];
        var color = node.Color ?? colors[i % colors.Length];

        // Apply animation
        var animatedRect = new RectangleF(
          rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
          rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
          rect.Width * (float)context.AnimationProgress,
          rect.Height * (float)context.AnimationProgress
        );

        this._DrawActivityNode(g, node, animatedRect, color, context.Diagram.Font);
        context.RegisterHitTestRect(node, animatedRect);
      }
    }
  }

  private void _DrawActivityNode(Graphics g, DiagramNode node, RectangleF rect, Color color, Font font) {
    switch (node.Shape) {
      case DiagramNodeShape.InitialState:
        // Filled circle
        var initSize = Math.Min(rect.Width, rect.Height) * 0.5f;
        using (var brush = new SolidBrush(Color.Black))
          g.FillEllipse(brush, rect.X + rect.Width / 2 - initSize / 2, rect.Y + rect.Height / 2 - initSize / 2, initSize, initSize);
        break;

      case DiagramNodeShape.FinalState:
        // Bull's eye
        var finalOuter = Math.Min(rect.Width, rect.Height) * 0.5f;
        var finalInner = finalOuter * 0.5f;
        using (var pen = new Pen(Color.Black, 2))
          g.DrawEllipse(pen, rect.X + rect.Width / 2 - finalOuter / 2, rect.Y + rect.Height / 2 - finalOuter / 2, finalOuter, finalOuter);
        using (var brush = new SolidBrush(Color.Black))
          g.FillEllipse(brush, rect.X + rect.Width / 2 - finalInner / 2, rect.Y + rect.Height / 2 - finalInner / 2, finalInner, finalInner);
        break;

      case DiagramNodeShape.Decision:
      case DiagramNodeShape.Diamond:
        // Diamond for decision/merge
        var center = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        var diamondSize = Math.Min(rect.Width, rect.Height) * 0.8f;
        var points = new[] {
          new PointF(center.X, center.Y - diamondSize / 2),
          new PointF(center.X + diamondSize / 2, center.Y),
          new PointF(center.X, center.Y + diamondSize / 2),
          new PointF(center.X - diamondSize / 2, center.Y)
        };
        using (var brush = new SolidBrush(Lighten(color, 0.7f)))
          g.FillPolygon(brush, points);
        using (var pen = new Pen(color, 2))
          g.DrawPolygon(pen, points);
        break;

      case DiagramNodeShape.Fork:
      case DiagramNodeShape.Join:
        // Horizontal bar
        var barHeight = 5f;
        using (var brush = new SolidBrush(Color.Black))
          g.FillRectangle(brush, rect.X, rect.Y + rect.Height / 2 - barHeight / 2, rect.Width, barHeight);
        break;

      default:
        // Rounded rectangle for action
        using (var brush = new SolidBrush(Lighten(color, 0.6f)))
          FillRoundedRectangle(g, brush, rect, 10);
        using (var pen = new Pen(color, 2))
          DrawRoundedRectangle(g, pen, rect, 10);

        // Label
        var label = node.Label ?? node.Id;
        var labelSize = g.MeasureString(label, font);
        using (var labelBrush = new SolidBrush(Color.Black))
          g.DrawString(label, font, labelBrush,
            rect.X + (rect.Width - labelSize.Width) / 2,
            rect.Y + (rect.Height - labelSize.Height) / 2);
        break;
    }
  }
}

/// <summary>
/// Renderer for UML component diagrams.
/// </summary>
public class ComponentDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.ComponentDiagram;

  /// <summary>Component box width.</summary>
  public float ComponentWidth { get; set; } = 120;

  /// <summary>Component box height.</summary>
  public float ComponentHeight { get; set; } = 60;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var components = context.Diagram.Components;
    var edges = context.Diagram.Edges;

    if (components == null || components.Count == 0)
      return;

    var colors = GetDefaultColors();
    var positions = new Dictionary<string, RectangleF>();
    var margin = 40f;

    // Use grid layout with proper margins to avoid overlap
    var gridLayout = CalculateGridLayout(components.Count, this.ComponentWidth, this.ComponentHeight, margin, context.PlotArea);
    for (var i = 0; i < components.Count; ++i) {
      var component = components[i];
      positions[component.Id] = gridLayout[i];
    }

    // Minimize edge crossings for grid layout
    if (edges != null && edges.Count > 0) {
      MinimizeEdgeCrossingsForGrid(
        components,
        c => c.Id,
        c => edges.Where(e => e.Source == c.Id).Select(e => e.Target)
             .Concat(edges.Where(e => e.Target == c.Id).Select(e => e.Source)),
        positions);
    }

    // Adjust layout to fit within plot area
    AdjustRectangleLayoutToFit(positions, context.PlotArea);

    // Draw dependencies
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.ContainsKey(edge.Source) || !positions.ContainsKey(edge.Target))
          continue;

        var sourceRect = positions[edge.Source];
        var targetRect = positions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        pen.DashStyle = DashStyle.Dash;
        pen.EndCap = LineCap.ArrowAnchor;
        g.DrawLine(pen, sourceCenter, targetCenter);
      }
    }

    // Draw components
    for (var i = 0; i < components.Count; ++i) {
      var component = components[i];
      if (!positions.ContainsKey(component.Id))
        continue;

      var rect = positions[component.Id];
      var color = Lighten(colors[i % colors.Length], 0.7f);

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw component box
      using (var brush = new SolidBrush(color))
        g.FillRectangle(brush, animatedRect);
      using (var pen = new Pen(Color.Black, 1))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw component symbol (two small rectangles on the left)
      var symbolSize = 12f;
      var symbolX = animatedRect.Right - symbolSize - 5;
      using (var pen = new Pen(Color.Black, 1)) {
        g.DrawRectangle(pen, symbolX, animatedRect.Y + 5, symbolSize, symbolSize / 2);
        g.DrawRectangle(pen, symbolX - 3, animatedRect.Y + 7, 3, symbolSize / 2 - 4);
        g.DrawRectangle(pen, symbolX, animatedRect.Y + 5 + symbolSize, symbolSize, symbolSize / 2);
        g.DrawRectangle(pen, symbolX - 3, animatedRect.Y + 7 + symbolSize, 3, symbolSize / 2 - 4);
      }

      // Draw name
      var name = component.Name ?? component.Id;
      var nameSize = g.MeasureString(name, context.Diagram.Font);
      using var nameBrush = new SolidBrush(Color.Black);
      g.DrawString(name, context.Diagram.Font, nameBrush,
        animatedRect.X + (animatedRect.Width - nameSize.Width) / 2,
        animatedRect.Y + (animatedRect.Height - nameSize.Height) / 2);

      // Draw provided interfaces (lollipops on right)
      if (component.ProvidedInterfaces != null) {
        var interfaceY = animatedRect.Y + animatedRect.Height / 2;
        foreach (var iface in component.ProvidedInterfaces) {
          g.DrawLine(Pens.Black, animatedRect.Right, interfaceY, animatedRect.Right + 15, interfaceY);
          g.DrawEllipse(Pens.Black, animatedRect.Right + 15, interfaceY - 5, 10, 10);
        }
      }

      // Draw required interfaces (sockets on left)
      if (component.RequiredInterfaces != null) {
        var interfaceY = animatedRect.Y + animatedRect.Height / 2;
        foreach (var iface in component.RequiredInterfaces) {
          g.DrawLine(Pens.Black, animatedRect.Left - 15, interfaceY, animatedRect.Left, interfaceY);
          g.DrawArc(Pens.Black, animatedRect.Left - 25, interfaceY - 5, 10, 10, 270, 180);
        }
      }
    }
  }
}

/// <summary>
/// Renderer for UML deployment diagrams.
/// </summary>
public class DeploymentDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.DeploymentDiagram;

  /// <summary>Node box width.</summary>
  public float NodeWidth { get; set; } = 150;

  /// <summary>Node box height.</summary>
  public float NodeHeight { get; set; } = 100;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var deploymentNodes = context.Diagram.DeploymentNodes;
    var edges = context.Diagram.Edges;

    if (deploymentNodes == null || deploymentNodes.Count == 0)
      return;

    var colors = GetDefaultColors();
    var positions = new Dictionary<string, RectangleF>();
    var margin = 40f;

    // Check for explicit positions
    var hasExplicitPositions = deploymentNodes.Any(n => !string.IsNullOrEmpty(n.ParentId) || deploymentNodes.Count == 1);

    // For now, just use grid layout with proper margins
    var gridLayout = CalculateGridLayout(deploymentNodes.Count, this.NodeWidth, this.NodeHeight, margin, context.PlotArea);
    for (var i = 0; i < deploymentNodes.Count; ++i) {
      var node = deploymentNodes[i];
      positions[node.Id] = gridLayout[i];
    }

    // Minimize edge crossings for grid layout
    if (edges != null && edges.Count > 0) {
      MinimizeEdgeCrossingsForGrid(
        deploymentNodes,
        n => n.Id,
        n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
             .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
        positions);
    }

    // Adjust layout to fit within plot area
    AdjustRectangleLayoutToFit(positions, context.PlotArea);

    // Draw communication paths
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.ContainsKey(edge.Source) || !positions.ContainsKey(edge.Target))
          continue;

        var sourceRect = positions[edge.Source];
        var targetRect = positions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, sourceCenter, targetCenter);

        // Protocol label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2);
          var labelText = $"<<{edge.Label}>>";
          var labelSize = g.MeasureString(labelText, context.Diagram.Font);

          using var bgBrush = new SolidBrush(Color.White);
          g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);

          using var labelBrush = new SolidBrush(Color.Gray);
          g.DrawString(labelText, context.Diagram.Font, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }

    // Draw deployment nodes
    for (var i = 0; i < deploymentNodes.Count; ++i) {
      var node = deploymentNodes[i];
      if (!positions.ContainsKey(node.Id))
        continue;

      var rect = positions[node.Id];
      var color = Lighten(colors[i % colors.Length], 0.8f);

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw 3D box (cube effect)
      var depth = 10f;

      // Top face
      var topFace = new[] {
        new PointF(animatedRect.Left, animatedRect.Top),
        new PointF(animatedRect.Left + depth, animatedRect.Top - depth),
        new PointF(animatedRect.Right + depth, animatedRect.Top - depth),
        new PointF(animatedRect.Right, animatedRect.Top)
      };
      using (var topBrush = new SolidBrush(Lighten(color, 0.2f)))
        g.FillPolygon(topBrush, topFace);
      using (var topPen = new Pen(Color.Black, 1))
        g.DrawPolygon(topPen, topFace);

      // Right face
      var rightFace = new[] {
        new PointF(animatedRect.Right, animatedRect.Top),
        new PointF(animatedRect.Right + depth, animatedRect.Top - depth),
        new PointF(animatedRect.Right + depth, animatedRect.Bottom - depth),
        new PointF(animatedRect.Right, animatedRect.Bottom)
      };
      using (var rightBrush = new SolidBrush(Darken(color, 0.1f)))
        g.FillPolygon(rightBrush, rightFace);
      using (var rightPen = new Pen(Color.Black, 1))
        g.DrawPolygon(rightPen, rightFace);

      // Front face
      using (var frontBrush = new SolidBrush(color))
        g.FillRectangle(frontBrush, animatedRect);
      using (var frontPen = new Pen(Color.Black, 1))
        g.DrawRectangle(frontPen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Stereotype
      var stereoY = animatedRect.Y + 5;
      if (!string.IsNullOrEmpty(node.Stereotype)) {
        var stereoText = $"<<{node.Stereotype}>>";
        var stereoSize = g.MeasureString(stereoText, context.Diagram.Font);
        using var stereoBrush = new SolidBrush(Color.Gray);
        g.DrawString(stereoText, context.Diagram.Font, stereoBrush,
          animatedRect.X + (animatedRect.Width - stereoSize.Width) / 2, stereoY);
        stereoY += stereoSize.Height;
      }

      // Name
      var name = node.Name ?? node.Id;
      var nameSize = g.MeasureString(name, context.Diagram.Font);
      using var nameBrush = new SolidBrush(Color.Black);
      g.DrawString(name, context.Diagram.Font, nameBrush,
        animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, stereoY + 5);
    }
  }
}

/// <summary>
/// Renderer for UML object diagrams (instances of classes).
/// </summary>
public class ObjectDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.ObjectDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    // Object diagram is similar to class diagram but with underlined names and instance values
    var g = context.Graphics;
    var classNodes = context.Diagram.ClassNodes;
    var classRelations = context.Diagram.ClassRelations;

    if (classNodes == null || classNodes.Count == 0)
      return;

    var colors = GetDefaultColors();
    var positions = new Dictionary<string, RectangleF>();
    var objectWidth = 130f;
    var objectHeight = 60f;
    var margin = 30f;

    // Check for explicit positions
    var hasExplicitPositions = classNodes.Any(n => n.Position != PointF.Empty && (n.Position.X > 0 || n.Position.Y > 0));

    if (hasExplicitPositions) {
      foreach (var obj in classNodes) {
        var x = context.PlotArea.Left + obj.Position.X / 100f * (context.PlotArea.Width - objectWidth);
        var y = context.PlotArea.Top + obj.Position.Y / 100f * (context.PlotArea.Height - objectHeight);
        positions[obj.Id] = new RectangleF(x, y, objectWidth, objectHeight);
      }
    } else {
      // Use grid layout with proper margins to avoid overlap
      var gridLayout = CalculateGridLayout(classNodes.Count, objectWidth, objectHeight, margin, context.PlotArea);
      for (var i = 0; i < classNodes.Count; ++i) {
        var obj = classNodes[i];
        positions[obj.Id] = gridLayout[i];
      }

      // Minimize edge crossings for grid layout
      if (classRelations != null && classRelations.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          classNodes,
          o => o.Id,
          o => classRelations.Where(r => r.From == o.Id).Select(r => r.To)
               .Concat(classRelations.Where(r => r.To == o.Id).Select(r => r.From)),
          positions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(positions, context.PlotArea);
    }

    // Draw links
    if (classRelations != null) {
      foreach (var relation in classRelations) {
        if (!positions.ContainsKey(relation.From) || !positions.ContainsKey(relation.To))
          continue;

        var sourceRect = positions[relation.From];
        var targetRect = positions[relation.To];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, sourceCenter, targetCenter);
      }
    }

    // Draw objects
    for (var i = 0; i < classNodes.Count; ++i) {
      var obj = classNodes[i];
      if (!positions.ContainsKey(obj.Id))
        continue;

      var rect = positions[obj.Id];
      var color = obj.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw object box
      using (var brush = new SolidBrush(Lighten(color, 0.8f)))
        g.FillRectangle(brush, animatedRect);
      using (var pen = new Pen(Color.Black, 1))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw underlined name (objectName : ClassName)
      var nameText = $"{obj.Id} : {obj.ClassName ?? "Object"}";
      var nameFont = new Font(context.Diagram.Font, FontStyle.Underline);
      var nameSize = g.MeasureString(nameText, nameFont);

      using (var nameBrush = new SolidBrush(Color.Black))
        g.DrawString(nameText, nameFont, nameBrush,
          animatedRect.X + (animatedRect.Width - nameSize.Width) / 2, animatedRect.Y + 5);

      // Draw attribute values
      if (obj.Fields != null && obj.Fields.Count > 0) {
        var attrY = animatedRect.Y + nameSize.Height + 10;
        foreach (var field in obj.Fields.Take(2)) {
          var attrText = $"{field.Name} = {field.Type}";
          using var attrBrush = new SolidBrush(Color.Black);
          g.DrawString(attrText, context.Diagram.Font, attrBrush, animatedRect.X + 5, attrY);
          attrY += 15;
        }
      }
    }
  }
}

/// <summary>
/// Renderer for UML package diagrams.
/// </summary>
public class PackageDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.PackageDiagram;

  /// <summary>Package width.</summary>
  public float PackageWidth { get; set; } = 120;

  /// <summary>Package height.</summary>
  public float PackageHeight { get; set; } = 80;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var packages = context.Diagram.Packages;
    var edges = context.Diagram.Edges;

    if (packages == null || packages.Count == 0)
      return;

    var colors = GetDefaultColors();
    var positions = new Dictionary<string, RectangleF>();

    // Position packages with proper non-overlapping layout
    var margin = 30f;
    var tabHeight = 15f; // Account for package tab in height calculation
    var totalHeight = this.PackageHeight + tabHeight;

    var gridLayout = CalculateGridLayout(packages.Count, this.PackageWidth, totalHeight, margin, context.PlotArea);

    for (var i = 0; i < packages.Count; ++i) {
      var pkg = packages[i];
      if (gridLayout.TryGetValue(i, out var gridRect)) {
        // Offset Y to account for tab height
        positions[pkg.Id] = new RectangleF(gridRect.X, gridRect.Y + tabHeight, this.PackageWidth, this.PackageHeight);
      }
    }

    // Minimize edge crossings for grid layout
    if (edges != null && edges.Count > 0) {
      MinimizeEdgeCrossingsForGrid(
        packages,
        p => p.Id,
        p => edges.Where(e => e.Source == p.Id).Select(e => e.Target)
             .Concat(edges.Where(e => e.Target == p.Id).Select(e => e.Source)),
        positions);
    }

    // Adjust layout to fit within plot area
    AdjustRectangleLayoutToFit(positions, context.PlotArea);

    // Draw dependencies
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.ContainsKey(edge.Source) || !positions.ContainsKey(edge.Target))
          continue;

        var sourceRect = positions[edge.Source];
        var targetRect = positions[edge.Target];

        var sourceCenter = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Y + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Y + targetRect.Height / 2);

        // Apply animation
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        pen.DashStyle = DashStyle.Dash;
        pen.EndCap = LineCap.ArrowAnchor;
        g.DrawLine(pen, sourceCenter, targetCenter);

        // <<import>> or <<access>> stereotype
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2);
          var labelText = $"<<{edge.Label}>>";
          var labelSize = g.MeasureString(labelText, context.Diagram.Font);

          using var bgBrush = new SolidBrush(Color.White);
          g.FillRectangle(bgBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2, labelSize.Width, labelSize.Height);

          using var labelBrush = new SolidBrush(Color.Gray);
          g.DrawString(labelText, context.Diagram.Font, labelBrush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }

    // Draw packages
    for (var i = 0; i < packages.Count; ++i) {
      var pkg = packages[i];
      if (!positions.ContainsKey(pkg.Id))
        continue;

      var rect = positions[pkg.Id];
      var color = Lighten(colors[i % colors.Length], 0.8f);

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw package tab
      var tabWidth = animatedRect.Width * 0.4f;
      var tabRect = new RectangleF(animatedRect.X, animatedRect.Y - tabHeight, tabWidth, tabHeight);

      using (var tabBrush = new SolidBrush(color))
        g.FillRectangle(tabBrush, tabRect);
      using (var tabPen = new Pen(Color.Black, 1)) {
        g.DrawLine(tabPen, tabRect.Left, tabRect.Top, tabRect.Right, tabRect.Top);
        g.DrawLine(tabPen, tabRect.Left, tabRect.Top, tabRect.Left, tabRect.Bottom);
        g.DrawLine(tabPen, tabRect.Right, tabRect.Top, tabRect.Right, tabRect.Bottom);
      }

      // Draw package body
      using (var bodyBrush = new SolidBrush(color))
        g.FillRectangle(bodyBrush, animatedRect);
      using (var bodyPen = new Pen(Color.Black, 1))
        g.DrawRectangle(bodyPen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw name
      var name = pkg.Name ?? pkg.Id;
      var nameSize = g.MeasureString(name, context.Diagram.Font);
      using var nameBrush = new SolidBrush(Color.Black);
      g.DrawString(name, context.Diagram.Font, nameBrush,
        animatedRect.X + (animatedRect.Width - nameSize.Width) / 2,
        animatedRect.Y + 5);
    }
  }
}

/// <summary>
/// Renderer for UML communication diagrams.
/// </summary>
public class CommunicationDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.CommunicationDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    // Communication diagram is like a network with numbered messages
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var colors = GetDefaultColors();
    var nodeSize = 60f;
    var margin = 30f;
    var nodePositions = new Dictionary<string, PointF>();

    // Check for explicit positions
    var hasExplicitPositions = nodes.Any(n => n.Position != PointF.Empty && (n.Position.X > 0 || n.Position.Y > 0));

    if (hasExplicitPositions) {
      foreach (var node in nodes) {
        var x = context.PlotArea.Left + node.Position.X / 100f * context.PlotArea.Width;
        var y = context.PlotArea.Top + node.Position.Y / 100f * context.PlotArea.Height;
        nodePositions[node.Id] = new PointF(x, y);
      }
    } else if (nodes.Count <= 10) {
      // Circular layout for smaller counts with proper spacing
      var centerX = context.PlotArea.Left + context.PlotArea.Width / 2;
      var centerY = context.PlotArea.Top + context.PlotArea.Height / 2;

      // Calculate minimum radius to avoid overlap
      var minSpacing = nodeSize + margin;
      var circumference = nodes.Count * minSpacing;
      var minRadius = circumference / (2 * (float)Math.PI);
      var availableRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) / 2 - nodeSize;
      var radius = Math.Max(minRadius, availableRadius * 0.6f);

      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        var angle = 2 * Math.PI * i / nodes.Count - Math.PI / 2;
        var x = centerX + (float)(Math.Cos(angle) * radius);
        var y = centerY + (float)(Math.Sin(angle) * radius);
        nodePositions[node.Id] = new PointF(x, y);
      }
    } else {
      // Grid layout for many nodes
      var gridLayout = CalculateGridLayout(nodes.Count, nodeSize, nodeSize, margin, context.PlotArea);
      var rectPositions = new Dictionary<string, RectangleF>();
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        rectPositions[node.Id] = gridLayout[i];
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          nodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          rectPositions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(rectPositions, context.PlotArea);

      // Convert back to PointF
      foreach (var kvp in rectPositions)
        nodePositions[kvp.Key] = new PointF(kvp.Value.X + kvp.Value.Width / 2, kvp.Value.Y + kvp.Value.Height / 2);
    }

    // Draw connections with message numbers
    if (edges != null) {
      var messageNum = 1;
      foreach (var edge in edges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var sourcePos = nodePositions[edge.Source];
        var targetPos = nodePositions[edge.Target];

        // Apply animation
        targetPos = new PointF(
          sourcePos.X + (targetPos.X - sourcePos.X) * (float)context.AnimationProgress,
          sourcePos.Y + (targetPos.Y - sourcePos.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        g.DrawLine(pen, sourcePos, targetPos);

        // Draw message number and name
        var midPoint = new PointF((sourcePos.X + targetPos.X) / 2, (sourcePos.Y + targetPos.Y) / 2);
        var messageText = $"{messageNum}: {edge.Label ?? "message"}";
        var messageSize = g.MeasureString(messageText, context.Diagram.Font);

        // Offset to avoid line overlap
        var dx = targetPos.X - sourcePos.X;
        var dy = targetPos.Y - sourcePos.Y;
        var length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length > 0) {
          var offsetX = -dy / length * 10;
          var offsetY = dx / length * 10;
          midPoint = new PointF(midPoint.X + offsetX, midPoint.Y + offsetY);
        }

        using var bgBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255));
        g.FillRectangle(bgBrush, midPoint.X - messageSize.Width / 2, midPoint.Y - messageSize.Height / 2, messageSize.Width, messageSize.Height);

        using var messageBrush = new SolidBrush(Color.Blue);
        g.DrawString(messageText, context.Diagram.Font, messageBrush, midPoint.X - messageSize.Width / 2, midPoint.Y - messageSize.Height / 2);

        ++messageNum;
      }
    }

    // Draw nodes
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var pos = nodePositions[node.Id];
      var color = node.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedSize = nodeSize * (float)context.AnimationProgress;
      var rect = new RectangleF(pos.X - animatedSize / 2, pos.Y - animatedSize / 2, animatedSize, animatedSize);

      using (var brush = new SolidBrush(Lighten(color, 0.7f)))
        g.FillRectangle(brush, rect);
      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

      // Draw underlined name (like object)
      var name = node.Label ?? node.Id;
      var nameFont = new Font(context.Diagram.Font, FontStyle.Underline);
      var nameSize = g.MeasureString(name, nameFont);

      using var nameBrush = new SolidBrush(Color.Black);
      g.DrawString(name, nameFont, nameBrush, pos.X - nameSize.Width / 2, pos.Y - nameSize.Height / 2);
    }
  }
}

/// <summary>
/// Renderer for UML timing diagrams.
/// </summary>
public class TimingDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.TimingDiagram;

  /// <summary>Row height per lifeline.</summary>
  public float RowHeight { get; set; } = 60;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var lifelines = context.Diagram.Lifelines;
    var signals = context.Diagram.Signals;

    if ((lifelines == null || lifelines.Count == 0) && (signals == null || signals.Count == 0))
      return;

    var colors = GetDefaultColors();

    // Calculate layout
    var labelWidth = 80f;
    var timelineStart = context.PlotArea.Left + labelWidth + 20;
    var timelineWidth = context.PlotArea.Width - labelWidth - 40;
    var startY = context.PlotArea.Top + 30;

    // Draw time axis
    using (var axisPen = new Pen(Color.Black, 1))
      g.DrawLine(axisPen, timelineStart, context.PlotArea.Bottom - 20, timelineStart + timelineWidth * (float)context.AnimationProgress, context.PlotArea.Bottom - 20);

    // Draw time markers
    for (var t = 0; t <= 10; ++t) {
      var x = timelineStart + t * timelineWidth / 10 * (float)context.AnimationProgress;
      using var tickPen = new Pen(Color.Black, 1);
      g.DrawLine(tickPen, x, context.PlotArea.Bottom - 25, x, context.PlotArea.Bottom - 15);

      using var tickBrush = new SolidBrush(Color.Black);
      g.DrawString($"t{t}", context.Diagram.Font, tickBrush, x - 5, context.PlotArea.Bottom - 15);
    }

    // Draw signals/waveforms
    if (signals != null) {
      var rowIndex = 0;
      foreach (var signal in signals) {
        var y = startY + rowIndex * this.RowHeight;
        var color = signal.Color ?? colors[rowIndex % colors.Length];

        // Draw signal name
        using (var nameBrush = new SolidBrush(Color.Black))
          g.DrawString(signal.Name ?? signal.Id, context.Diagram.Font, nameBrush, context.PlotArea.Left + 5, y + this.RowHeight / 2 - 7);

        // Draw state lanes
        using (var lanePen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot }) {
          g.DrawLine(lanePen, timelineStart, y + 10, timelineStart + timelineWidth, y + 10); // High
          g.DrawLine(lanePen, timelineStart, y + this.RowHeight - 10, timelineStart + timelineWidth, y + this.RowHeight - 10); // Low
        }

        // Draw waveform from transitions
        if (signal.Transitions != null && signal.Transitions.Count > 0) {
          using var waveformPen = new Pen(color, 2);

          var prevX = timelineStart;
          var prevY = y + this.RowHeight - 10; // Start low

          foreach (var transition in signal.Transitions.OrderBy(t => t.Time)) {
            var transX = timelineStart + (float)(transition.Time / 100.0 * timelineWidth) * (float)context.AnimationProgress;

            var transY = transition.Level switch {
              DiagramSignalLevel.High => y + 10,
              DiagramSignalLevel.Low => y + this.RowHeight - 10,
              _ => y + this.RowHeight / 2
            };

            // Draw horizontal line to transition point
            g.DrawLine(waveformPen, prevX, prevY, transX, prevY);
            // Draw vertical transition
            g.DrawLine(waveformPen, transX, prevY, transX, transY);

            prevX = transX;
            prevY = transY;
          }

          // Extend to end
          g.DrawLine(waveformPen, prevX, prevY, timelineStart + timelineWidth * (float)context.AnimationProgress, prevY);
        }

        ++rowIndex;
      }
    }

    // Draw lifeline states if present
    if (lifelines != null) {
      var rowIndex = signals?.Count ?? 0;
      foreach (var lifeline in lifelines) {
        var y = startY + rowIndex * this.RowHeight;
        var color = lifeline.Color ?? colors[rowIndex % colors.Length];

        // Draw lifeline name
        using (var nameBrush = new SolidBrush(Color.Black))
          g.DrawString(lifeline.Name ?? lifeline.Id, context.Diagram.Font, nameBrush, context.PlotArea.Left + 5, y + this.RowHeight / 2 - 7);

        // Draw state timeline
        using var timelinePen = new Pen(color, 2);
        g.DrawLine(timelinePen, timelineStart, y + this.RowHeight / 2, timelineStart + timelineWidth * (float)context.AnimationProgress, y + this.RowHeight / 2);

        ++rowIndex;
      }
    }
  }
}

/// <summary>
/// Renderer for UML interaction overview diagrams.
/// </summary>
public class InteractionOverviewDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.InteractionOverview;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    // Interaction overview is like activity diagram with interaction fragments
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var colors = GetDefaultColors();
    var nodePositions = new Dictionary<string, RectangleF>();

    // Measure actual node sizes including labels
    var nodeSizes = new Dictionary<string, SizeF>();
    var baseWidth = 150f;
    var baseHeight = 60f;
    var labelPadding = 10f;

    foreach (var node in nodes) {
      var label = node.Label ?? node.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      var width = Math.Max(baseWidth, labelSize.Width + labelPadding * 2);
      var height = baseHeight + labelSize.Height;
      nodeSizes[node.Id] = new SizeF(width, height);
    }

    // Find max size for uniform grid cells
    var maxWidth = nodeSizes.Values.Max(s => s.Width);
    var maxHeight = nodeSizes.Values.Max(s => s.Height);
    var margin = 20f;
    var cellWidth = maxWidth + margin;
    var cellHeight = maxHeight + margin;

    // Check if positions are explicitly set
    var hasExplicitPositions = nodes.Any(n => n.Position.X > 0 || n.Position.Y > 0);

    if (hasExplicitPositions) {
      // Use explicit positions
      foreach (var node in nodes) {
        var size = nodeSizes[node.Id];
        var x = node.Position.X / 100f * context.PlotArea.Width;
        var y = node.Position.Y / 100f * context.PlotArea.Height;
        nodePositions[node.Id] = new RectangleF(x, y, size.Width, size.Height);
      }
    } else {
      // Auto-layout: calculate grid dimensions
      var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(nodes.Count)));
      var rows = (int)Math.Ceiling((float)nodes.Count / cols);

      // Position nodes in grid (positions are relative, will be scaled to fit)
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        var col = i % cols;
        var row = i / cols;
        var size = nodeSizes[node.Id];
        var x = col * cellWidth + (cellWidth - size.Width) / 2;
        var y = row * cellHeight + (cellHeight - size.Height) / 2;
        nodePositions[node.Id] = new RectangleF(x, y, size.Width, size.Height);
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          nodes,
          n => n.Id,
          n => edges.Where(e => e.Source == n.Id).Select(e => e.Target)
               .Concat(edges.Where(e => e.Target == n.Id).Select(e => e.Source)),
          nodePositions);
      }
    }

    // Scale and center to fit viewport (both in and out)
    AdjustRectangleLayoutToFit(nodePositions, context.PlotArea);

    // Draw flow connections
    if (edges != null) {
      foreach (var edge in edges) {
        if (!nodePositions.ContainsKey(edge.Source) || !nodePositions.ContainsKey(edge.Target))
          continue;

        var sourceRect = nodePositions[edge.Source];
        var targetRect = nodePositions[edge.Target];

        var sourceBottom = new PointF(sourceRect.X + sourceRect.Width / 2, sourceRect.Bottom);
        var targetTop = new PointF(targetRect.X + targetRect.Width / 2, targetRect.Top);

        // Apply animation
        targetTop = new PointF(
          sourceBottom.X + (targetTop.X - sourceBottom.X) * (float)context.AnimationProgress,
          sourceBottom.Y + (targetTop.Y - sourceBottom.Y) * (float)context.AnimationProgress
        );

        using var pen = new Pen(Color.Black, 1);
        pen.EndCap = LineCap.ArrowAnchor;
        pen.CustomEndCap = new AdjustableArrowCap(5, 5, true);
        g.DrawLine(pen, sourceBottom, targetTop);

        // Guard condition
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceBottom.X + targetTop.X) / 2 + 10, (sourceBottom.Y + targetTop.Y) / 2);
          var guardText = $"[{edge.Label}]";
          using var guardBrush = new SolidBrush(Color.Black);
          g.DrawString(guardText, context.Diagram.Font, guardBrush, midPoint);
        }
      }
    }

    // Draw nodes as interaction frames
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!nodePositions.ContainsKey(node.Id))
        continue;

      var rect = nodePositions[node.Id];
      var color = node.Color ?? colors[i % colors.Length];

      // Apply animation
      var animatedRect = new RectangleF(
        rect.X + rect.Width / 2 - rect.Width / 2 * (float)context.AnimationProgress,
        rect.Y + rect.Height / 2 - rect.Height / 2 * (float)context.AnimationProgress,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Check node type by shape
      switch (node.Shape) {
        case DiagramNodeShape.InitialState:
          var initSize = 15f * (float)context.AnimationProgress;
          using (var brush = new SolidBrush(Color.Black))
            g.FillEllipse(brush, animatedRect.X + animatedRect.Width / 2 - initSize / 2,
              animatedRect.Y + animatedRect.Height / 2 - initSize / 2, initSize, initSize);
          break;

        case DiagramNodeShape.FinalState:
          var finalOuter = 20f * (float)context.AnimationProgress;
          var finalInner = 12f * (float)context.AnimationProgress;
          using (var pen = new Pen(Color.Black, 2))
            g.DrawEllipse(pen, animatedRect.X + animatedRect.Width / 2 - finalOuter / 2,
              animatedRect.Y + animatedRect.Height / 2 - finalOuter / 2, finalOuter, finalOuter);
          using (var brush = new SolidBrush(Color.Black))
            g.FillEllipse(brush, animatedRect.X + animatedRect.Width / 2 - finalInner / 2,
              animatedRect.Y + animatedRect.Height / 2 - finalInner / 2, finalInner, finalInner);
          break;

        case DiagramNodeShape.Diamond:
          var center = new PointF(animatedRect.X + animatedRect.Width / 2, animatedRect.Y + animatedRect.Height / 2);
          var size = Math.Min(animatedRect.Width, animatedRect.Height) * 0.6f;
          var points = new[] {
            new PointF(center.X, center.Y - size / 2),
            new PointF(center.X + size / 2, center.Y),
            new PointF(center.X, center.Y + size / 2),
            new PointF(center.X - size / 2, center.Y)
          };
          using (var brush = new SolidBrush(Lighten(color, 0.7f)))
            g.FillPolygon(brush, points);
          using (var pen = new Pen(color, 2))
            g.DrawPolygon(pen, points);
          break;

        default:
          // Interaction frame (ref, sd, etc.)
          using (var brush = new SolidBrush(Color.White))
            g.FillRectangle(brush, animatedRect);
          using (var pen = new Pen(Color.Black, 1))
            g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

          // Pentagon tag in top-left
          var tagWidth = 30f;
          var tagHeight = 18f;
          var tagPoints = new[] {
            new PointF(animatedRect.X, animatedRect.Y),
            new PointF(animatedRect.X + tagWidth, animatedRect.Y),
            new PointF(animatedRect.X + tagWidth, animatedRect.Y + tagHeight - 5),
            new PointF(animatedRect.X + tagWidth - 5, animatedRect.Y + tagHeight),
            new PointF(animatedRect.X, animatedRect.Y + tagHeight)
          };
          using (var tagBrush = new SolidBrush(Lighten(color, 0.8f)))
            g.FillPolygon(tagBrush, tagPoints);
          using (var tagPen = new Pen(Color.Black, 1))
            g.DrawPolygon(tagPen, tagPoints);

          // Type label in tag
          var typeText = "ref";
          using (var typeBrush = new SolidBrush(Color.Black)) {
            var typeFont = new Font(context.Diagram.Font.FontFamily, 7f);
            g.DrawString(typeText, typeFont, typeBrush, animatedRect.X + 2, animatedRect.Y + 2);
          }

          // Interaction name
          var name = node.Label ?? node.Id;
          var nameSize = g.MeasureString(name, context.Diagram.Font);
          using (var nameBrush = new SolidBrush(Color.Black))
            g.DrawString(name, context.Diagram.Font, nameBrush,
              animatedRect.X + (animatedRect.Width - nameSize.Width) / 2,
              animatedRect.Y + (animatedRect.Height - nameSize.Height) / 2 + 5);
          break;
      }

      context.RegisterHitTestRect(node, animatedRect);
    }
  }
}
