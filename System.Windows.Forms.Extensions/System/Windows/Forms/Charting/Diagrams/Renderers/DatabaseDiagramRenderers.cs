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
/// Renderer for Entity-Relationship (ER) diagrams with crow's foot notation.
/// </summary>
public class EntityRelationshipDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.EntityRelationship;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var entities = context.Diagram.Entities;
    var relationships = context.Diagram.Relationships;

    if (entities == null || entities.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Calculate entity positions - auto-layout if positions not set
    var entityPositions = new Dictionary<string, RectangleF>();
    var entityWidth = Math.Min(150f, plotArea.Width / Math.Max(entities.Count, 1) - 20);
    var attributeHeight = 20f;

    // Check if positions are explicitly set (any non-zero position)
    var hasExplicitPositions = entities.Any(e => e.Position.X > 0 || e.Position.Y > 0);

    if (hasExplicitPositions) {
      // Use explicit positions
      for (var i = 0; i < entities.Count; ++i) {
        var entity = entities[i];
        var entityHeight = 30 + (entity.Attributes?.Count ?? 0) * attributeHeight;
        var x = plotArea.Left + 20 + entity.Position.X / 100f * (plotArea.Width - entityWidth - 40);
        var y = plotArea.Top + 20 + entity.Position.Y / 100f * (plotArea.Height - entityHeight - 40);
        entityPositions[entity.Id] = new RectangleF(x, y, entityWidth, entityHeight);
      }
    } else {
      // Auto-layout in a grid pattern with proper margins to avoid overlap
      var margin = 30f;

      // Calculate maximum entity height (including attributes)
      var maxEntityHeight = 30f + attributeHeight * 8; // Assume max 8 attributes for sizing
      foreach (var entity in entities) {
        var entityHeight = 30 + (entity.Attributes?.Count ?? 0) * attributeHeight;
        maxEntityHeight = Math.Max(maxEntityHeight, entityHeight);
      }

      // Use proper grid layout calculation
      var gridLayout = CalculateGridLayout(entities.Count, entityWidth, maxEntityHeight, margin, plotArea);

      for (var i = 0; i < entities.Count; ++i) {
        var entity = entities[i];
        if (gridLayout.TryGetValue(i, out var gridRect)) {
          // Adjust height based on actual attributes
          var actualHeight = 30 + (entity.Attributes?.Count ?? 0) * attributeHeight;
          entityPositions[entity.Id] = new RectangleF(gridRect.X, gridRect.Y, entityWidth, actualHeight);
        }
      }

      // Minimize edge crossings for grid layout
      if (relationships != null && relationships.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          entities,
          e => e.Id,
          e => relationships.Where(r => r.From == e.Id).Select(r => r.To)
               .Concat(relationships.Where(r => r.To == e.Id).Select(r => r.From)),
          entityPositions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(entityPositions, plotArea);
    }

    // Draw relationships first (behind entities)
    if (relationships != null) {
      foreach (var rel in relationships) {
        if (!entityPositions.TryGetValue(rel.From, out var fromRect) ||
            !entityPositions.TryGetValue(rel.To, out var toRect))
          continue;

        var fromCenter = new PointF(fromRect.Left + fromRect.Width / 2, fromRect.Top + fromRect.Height / 2);
        var toCenter = new PointF(toRect.Left + toRect.Width / 2, toRect.Top + toRect.Height / 2);

        // Find edge points
        var fromEdge = this._GetEdgePoint(fromRect, toCenter);
        var toEdge = this._GetEdgePoint(toRect, fromCenter);

        // Apply animation
        toEdge = new PointF(
          fromEdge.X + (toEdge.X - fromEdge.X) * (float)context.AnimationProgress,
          fromEdge.Y + (toEdge.Y - fromEdge.Y) * (float)context.AnimationProgress
        );

        using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
          g.DrawLine(pen, fromEdge, toEdge);

        // Draw crow's foot notation
        this._DrawCardinality(g, fromEdge, toCenter, rel.FromCardinality);
        this._DrawCardinality(g, toEdge, fromCenter, rel.ToCardinality);

        // Draw relationship label
        if (!string.IsNullOrEmpty(rel.Label)) {
          var midPoint = new PointF((fromEdge.X + toEdge.X) / 2, (fromEdge.Y + toEdge.Y) / 2);
          var labelSize = g.MeasureString(rel.Label, context.Diagram.Font);
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(rel.Label, context.Diagram.Font, brush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height / 2);
        }
      }
    }

    // Draw entities
    for (var i = 0; i < entities.Count; ++i) {
      var entity = entities[i];
      if (!entityPositions.TryGetValue(entity.Id, out var rect))
        continue;

      var color = entity.Color ?? colors[i % colors.Length];
      var animatedRect = new RectangleF(
        rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
        rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      // Draw entity box
      using (var brush = new SolidBrush(Color.White))
        g.FillRectangle(brush, animatedRect);

      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw entity name header
      var headerRect = new RectangleF(animatedRect.X, animatedRect.Y, animatedRect.Width, 28);
      using (var brush = new SolidBrush(Color.FromArgb(50, color)))
        g.FillRectangle(brush, headerRect);

      using (var pen = new Pen(color, 1))
        g.DrawLine(pen, headerRect.Left, headerRect.Bottom, headerRect.Right, headerRect.Bottom);

      using var headerFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.1f, FontStyle.Bold);
      var nameSize = g.MeasureString(entity.Name ?? entity.Id, headerFont);
      using var nameBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(entity.Name ?? entity.Id, headerFont, nameBrush,
        animatedRect.X + (animatedRect.Width - nameSize.Width) / 2,
        animatedRect.Y + (headerRect.Height - nameSize.Height) / 2);

      // Draw attributes
      if (entity.Attributes != null) {
        var attrY = animatedRect.Y + 30;
        foreach (var attr in entity.Attributes) {
          if (attrY > animatedRect.Bottom - 5)
            break;

          var prefix = attr.IsPrimaryKey ? "PK " : attr.IsForeignKey ? "FK " : "";
          var suffix = attr.IsNullable ? "" : " *";
          var attrText = $"{prefix}{attr.Name}: {attr.DataType}{suffix}";

          using var attrBrush = new SolidBrush(attr.IsPrimaryKey ? Color.DarkBlue : attr.IsForeignKey ? Color.DarkGreen : context.Diagram.ForeColor);
          using var attrFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f, attr.IsPrimaryKey ? FontStyle.Bold : FontStyle.Regular);
          g.DrawString(attrText, attrFont, attrBrush, animatedRect.X + 5, attrY);
          attrY += attributeHeight;
        }
      }

      context.RegisterHitTestRect(entity, animatedRect);
    }
  }

  private PointF _GetEdgePoint(RectangleF rect, PointF target) {
    var center = new PointF(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
    var dx = target.X - center.X;
    var dy = target.Y - center.Y;

    if (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f)
      return center;

    var scaleX = dx != 0 ? (rect.Width / 2) / Math.Abs(dx) : float.MaxValue;
    var scaleY = dy != 0 ? (rect.Height / 2) / Math.Abs(dy) : float.MaxValue;
    var scale = Math.Min(scaleX, scaleY);

    return new PointF(center.X + dx * (float)scale, center.Y + dy * (float)scale);
  }

  private void _DrawCardinality(Graphics g, PointF point, PointF direction, DiagramCardinality cardinality) {
    var dx = direction.X - point.X;
    var dy = direction.Y - point.Y;
    var length = (float)Math.Sqrt(dx * dx + dy * dy);
    if (length < 1) return;

    dx /= length;
    dy /= length;

    var perpX = -dy;
    var perpY = dx;
    var offset = 15f;
    var spread = 8f;

    var basePoint = new PointF(point.X + dx * offset, point.Y + dy * offset);

    using var pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f);

    switch (cardinality) {
      case DiagramCardinality.One:
        // Single line
        g.DrawLine(pen, basePoint.X - perpX * spread, basePoint.Y - perpY * spread,
          basePoint.X + perpX * spread, basePoint.Y + perpY * spread);
        break;

      case DiagramCardinality.Many:
        // Crow's foot
        g.DrawLine(pen, point, new PointF(basePoint.X - perpX * spread, basePoint.Y - perpY * spread));
        g.DrawLine(pen, point, new PointF(basePoint.X + perpX * spread, basePoint.Y + perpY * spread));
        g.DrawLine(pen, point, basePoint);
        break;

      case DiagramCardinality.ZeroOrOne:
        // Circle + line
        g.DrawEllipse(pen, basePoint.X - 4, basePoint.Y - 4, 8, 8);
        var linePoint = new PointF(basePoint.X + dx * 10, basePoint.Y + dy * 10);
        g.DrawLine(pen, linePoint.X - perpX * spread, linePoint.Y - perpY * spread,
          linePoint.X + perpX * spread, linePoint.Y + perpY * spread);
        break;

      case DiagramCardinality.OneOrMore:
        // Line + crow's foot
        g.DrawLine(pen, basePoint.X - perpX * spread, basePoint.Y - perpY * spread,
          basePoint.X + perpX * spread, basePoint.Y + perpY * spread);
        var footPoint = new PointF(point.X + dx * 5, point.Y + dy * 5);
        g.DrawLine(pen, footPoint, new PointF(basePoint.X - perpX * spread, basePoint.Y - perpY * spread));
        g.DrawLine(pen, footPoint, new PointF(basePoint.X + perpX * spread, basePoint.Y + perpY * spread));
        break;

      case DiagramCardinality.ZeroOrMore:
        // Circle + crow's foot
        g.DrawEllipse(pen, basePoint.X - 4, basePoint.Y - 4, 8, 8);
        var footBase = new PointF(point.X + dx * 5, point.Y + dy * 5);
        g.DrawLine(pen, footBase, new PointF(basePoint.X - perpX * spread, basePoint.Y - perpY * spread));
        g.DrawLine(pen, footBase, new PointF(basePoint.X + perpX * spread, basePoint.Y + perpY * spread));
        break;
    }
  }
}

/// <summary>
/// Renderer for Data Flow Diagrams (DFD).
/// </summary>
public class DataFlowDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.DataFlow;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var elements = context.Diagram.DataFlowElements;
    var edges = context.Diagram.Edges;

    if (elements == null || elements.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Calculate element positions - auto-layout if positions not set
    var elementPositions = new Dictionary<string, RectangleF>();

    // Check if positions are explicitly set
    var hasExplicitPositions = elements.Any(e => e.Position.X > 0 || e.Position.Y > 0);

    if (hasExplicitPositions) {
      foreach (var element in elements) {
        var size = this._GetElementSize(element.ElementType);
        var x = plotArea.Left + 20 + element.Position.X / 100f * (plotArea.Width - size.Width - 40);
        var y = plotArea.Top + 20 + element.Position.Y / 100f * (plotArea.Height - size.Height - 40);
        elementPositions[element.Id] = new RectangleF(x, y, size.Width, size.Height);
      }
    } else {
      // Auto-layout in grid with proper margins to avoid overlap
      var margin = 30f;

      // Find the maximum element size
      var maxWidth = 0f;
      var maxHeight = 0f;
      foreach (var element in elements) {
        var size = this._GetElementSize(element.ElementType);
        maxWidth = Math.Max(maxWidth, size.Width);
        maxHeight = Math.Max(maxHeight, size.Height);
      }

      // Use proper grid layout calculation
      var gridLayout = CalculateGridLayout(elements.Count, maxWidth, maxHeight, margin, plotArea);

      for (var i = 0; i < elements.Count; ++i) {
        var element = elements[i];
        if (gridLayout.TryGetValue(i, out var gridRect)) {
          var size = this._GetElementSize(element.ElementType);
          // Center the element within its grid cell
          var x = gridRect.X + (gridRect.Width - size.Width) / 2;
          var y = gridRect.Y + (gridRect.Height - size.Height) / 2;
          elementPositions[element.Id] = new RectangleF(x, y, size.Width, size.Height);
        }
      }

      // Minimize edge crossings for grid layout
      if (edges != null && edges.Count > 0) {
        MinimizeEdgeCrossingsForGrid(
          elements,
          e => e.Id,
          e => edges.Where(ed => ed.Source == e.Id).Select(ed => ed.Target)
               .Concat(edges.Where(ed => ed.Target == e.Id).Select(ed => ed.Source)),
          elementPositions);
      }

      // Adjust layout to fit within plot area
      AdjustRectangleLayoutToFit(elementPositions, plotArea);
    }

    // Draw data flows (edges)
    if (edges != null) {
      using var flowPen = new Pen(Color.FromArgb(52, 73, 94), 1.5f);
      flowPen.EndCap = LineCap.ArrowAnchor;

      foreach (var edge in edges) {
        if (!elementPositions.TryGetValue(edge.Source, out var sourceRect) ||
            !elementPositions.TryGetValue(edge.Target, out var targetRect))
          continue;

        var sourceCenter = new PointF(sourceRect.Left + sourceRect.Width / 2, sourceRect.Top + sourceRect.Height / 2);
        var targetCenter = new PointF(targetRect.Left + targetRect.Width / 2, targetRect.Top + targetRect.Height / 2);

        // Animate
        targetCenter = new PointF(
          sourceCenter.X + (targetCenter.X - sourceCenter.X) * (float)context.AnimationProgress,
          sourceCenter.Y + (targetCenter.Y - sourceCenter.Y) * (float)context.AnimationProgress
        );

        g.DrawLine(flowPen, sourceCenter, targetCenter);

        // Draw label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((sourceCenter.X + targetCenter.X) / 2, (sourceCenter.Y + targetCenter.Y) / 2);
          var labelSize = g.MeasureString(edge.Label, context.Diagram.Font);
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(edge.Label, context.Diagram.Font, brush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height - 5);
        }
      }
    }

    // Draw DFD elements
    for (var i = 0; i < elements.Count; ++i) {
      var element = elements[i];
      if (!elementPositions.TryGetValue(element.Id, out var rect))
        continue;

      var color = element.Color ?? colors[i % colors.Length];
      this._DrawDFDElement(g, context, element, rect, color);
      context.RegisterHitTestRect(element, rect);
    }
  }

  private SizeF _GetElementSize(DiagramDFDType type) {
    return type switch {
      DiagramDFDType.Process => new SizeF(80, 80),
      DiagramDFDType.DataStore => new SizeF(120, 40),
      DiagramDFDType.ExternalEntity => new SizeF(100, 60),
      _ => new SizeF(80, 60)
    };
  }

  private void _DrawDFDElement(Graphics g, DiagramRenderContext context, DiagramDataFlowElement element, RectangleF rect, Color color) {
    var animatedRect = new RectangleF(
      rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
      rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
      rect.Width * (float)context.AnimationProgress,
      rect.Height * (float)context.AnimationProgress
    );

    switch (element.ElementType) {
      case DiagramDFDType.Process:
        // Circle with process number
        using (var brush = new SolidBrush(Color.FromArgb(240, 248, 255)))
          g.FillEllipse(brush, animatedRect);
        using (var pen = new Pen(color, 2))
          g.DrawEllipse(pen, animatedRect);

        // Draw process number
        if (element.ProcessNumber > 0) {
          using var numFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
          var numText = element.ProcessNumber.ToString();
          var numSize = g.MeasureString(numText, numFont);
          using var brush = new SolidBrush(Color.Gray);
          g.DrawString(numText, numFont, brush, animatedRect.X + 5, animatedRect.Y + 5);
        }

        // Draw name
        if (!string.IsNullOrEmpty(element.Name)) {
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
          g.DrawString(element.Name, context.Diagram.Font, brush, animatedRect, format);
        }
        break;

      case DiagramDFDType.DataStore: {
        // Open rectangle (two horizontal lines)
        using (var brush = new SolidBrush(Color.FromArgb(255, 250, 240)))
          g.FillRectangle(brush, animatedRect);
        using (var pen = new Pen(color, 2)) {
          g.DrawLine(pen, animatedRect.Left, animatedRect.Top, animatedRect.Right, animatedRect.Top);
          g.DrawLine(pen, animatedRect.Left, animatedRect.Bottom, animatedRect.Right, animatedRect.Bottom);
          g.DrawLine(pen, animatedRect.Left, animatedRect.Top, animatedRect.Left, animatedRect.Bottom);
        }

        // Draw ID and name
        var idWidth = 25f;
        using (var pen = new Pen(color, 1))
          g.DrawLine(pen, animatedRect.Left + idWidth, animatedRect.Top, animatedRect.Left + idWidth, animatedRect.Bottom);

        using var idFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.85f);
        using var idBrush = new SolidBrush(context.Diagram.ForeColor);
        g.DrawString("D" + (element.ProcessNumber > 0 ? element.ProcessNumber.ToString() : ""), idFont, idBrush, animatedRect.X + 3, animatedRect.Y + (animatedRect.Height - context.Diagram.Font.Height) / 2);

        if (!string.IsNullOrEmpty(element.Name)) {
          using var nameBrush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(element.Name, context.Diagram.Font, nameBrush, animatedRect.X + idWidth + 5, animatedRect.Y + (animatedRect.Height - context.Diagram.Font.Height) / 2);
        }
        break;
      }

      case DiagramDFDType.ExternalEntity: {
        // Rectangle
        using (var brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
          g.FillRectangle(brush, animatedRect);
        using (var pen = new Pen(color, 2))
          g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

        if (!string.IsNullOrEmpty(element.Name)) {
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
          g.DrawString(element.Name, context.Diagram.Font, brush, animatedRect, format);
        }
        break;
      }
    }
  }
}
