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
/// Renderer for Venn diagrams showing overlapping sets.
/// </summary>
public class VennDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.VennDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var sets = context.Diagram.Sets;
    var intersections = context.Diagram.SetIntersections;

    if (sets == null || sets.Count == 0)
      return;

    var colors = GetDefaultColors();
    var center = new PointF(
      context.PlotArea.Left + context.PlotArea.Width / 2,
      context.PlotArea.Top + context.PlotArea.Height / 2
    );

    // Calculate circle positions for up to 3 sets in classic Venn layout
    var baseRadius = Math.Min(context.PlotArea.Width, context.PlotArea.Height) * 0.3f * (float)context.AnimationProgress;
    var setPositions = new Dictionary<string, (PointF Center, float Radius)>();
    var overlap = baseRadius * 0.5f;

    var angleStep = 360.0 / Math.Max(sets.Count, 1);

    // Normalize sizes to get relative scale factors (0.5 to 1.5 range)
    var maxSize = sets.Max(s => s.Size);
    var minSize = sets.Min(s => s.Size);
    var sizeRange = Math.Max(maxSize - minSize, 1);

    if (sets.Count == 1) {
      setPositions[sets[0].Id] = (center, baseRadius);
    } else if (sets.Count == 2) {
      var radius0 = baseRadius * (0.7f + 0.6f * (float)((sets[0].Size - minSize) / sizeRange));
      var radius1 = baseRadius * (0.7f + 0.6f * (float)((sets[1].Size - minSize) / sizeRange));
      setPositions[sets[0].Id] = (new PointF(center.X - overlap / 2, center.Y), radius0);
      setPositions[sets[1].Id] = (new PointF(center.X + overlap / 2, center.Y), radius1);
    } else {
      // Arrange in a triangle for 3+ sets
      for (var i = 0; i < sets.Count; ++i) {
        var angle = (angleStep * i - 90) * Math.PI / 180;
        var offset = overlap * 0.8f;
        var setCenter = new PointF(
          center.X + (float)(Math.Cos(angle) * offset),
          center.Y + (float)(Math.Sin(angle) * offset)
        );
        // Normalize size to a reasonable scale factor (0.7 to 1.3)
        var scaleFactor = 0.7f + 0.6f * (float)((sets[i].Size - minSize) / sizeRange);
        setPositions[sets[i].Id] = (setCenter, baseRadius * scaleFactor);
      }
    }

    // Draw sets as semi-transparent circles
    for (var i = 0; i < sets.Count; ++i) {
      var set = sets[i];
      if (!setPositions.TryGetValue(set.Id, out var pos))
        continue;

      var color = set.Color ?? colors[i % colors.Length];
      color = Color.FromArgb(100, color);

      using (var brush = new SolidBrush(color))
        g.FillEllipse(brush, pos.Center.X - pos.Radius, pos.Center.Y - pos.Radius, pos.Radius * 2, pos.Radius * 2);

      using (var pen = new Pen(Darken(set.Color ?? colors[i % colors.Length]), 2))
        g.DrawEllipse(pen, pos.Center.X - pos.Radius, pos.Center.Y - pos.Radius, pos.Radius * 2, pos.Radius * 2);

      // Draw label outside the circle
      var labelAngle = sets.Count <= 2 ? (i == 0 ? Math.PI : 0) : (angleStep * i - 90) * Math.PI / 180;
      var labelDist = pos.Radius + 20;
      var labelPos = new PointF(
        pos.Center.X + (float)(Math.Cos(labelAngle) * labelDist),
        pos.Center.Y + (float)(Math.Sin(labelAngle) * labelDist)
      );

      var label = set.Label ?? set.Id;
      var labelSize = g.MeasureString(label, context.Diagram.Font);
      using var labelBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(label, context.Diagram.Font, labelBrush, labelPos.X - labelSize.Width / 2, labelPos.Y - labelSize.Height / 2);

      // Register hit test
      context.RegisterHitTestRect(
        set,
        new RectangleF(pos.Center.X - pos.Radius, pos.Center.Y - pos.Radius, pos.Radius * 2, pos.Radius * 2)
      );
    }

    // Draw intersection labels
    if (intersections != null) {
      foreach (var intersection in intersections) {
        if (string.IsNullOrEmpty(intersection.Label))
          continue;

        // Calculate intersection center (average of set centers)
        var validSets = intersection.SetIds.Where(id => setPositions.ContainsKey(id)).ToList();
        if (validSets.Count == 0)
          continue;

        var intCenter = new PointF(
          validSets.Average(id => setPositions[id].Center.X),
          validSets.Average(id => setPositions[id].Center.Y)
        );

        var label = intersection.Label;
        var labelSize = g.MeasureString(label, context.Diagram.Font);
        using var brush = new SolidBrush(context.Diagram.ForeColor);
        g.DrawString(label, context.Diagram.Font, brush, intCenter.X - labelSize.Width / 2, intCenter.Y - labelSize.Height / 2);
      }
    }
  }
}

/// <summary>
/// Renderer for Matrix/Quadrant diagrams with items positioned on X/Y axes.
/// </summary>
public class MatrixDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Matrix;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var quadrants = context.Diagram.Quadrants;
    var items = context.Diagram.MatrixItems;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Get quadrant config (use first one or defaults)
    var quadrant = quadrants?.Count > 0 ? quadrants[0] : new DiagramQuadrant();

    var halfWidth = plotArea.Width / 2;
    var halfHeight = plotArea.Height / 2;

    // Draw quadrant backgrounds
    var quadrantColors = new[] {
      quadrant.TopLeftColor ?? Color.FromArgb(50, colors[0]),
      quadrant.TopRightColor ?? Color.FromArgb(50, colors[1]),
      quadrant.BottomLeftColor ?? Color.FromArgb(50, colors[2]),
      quadrant.BottomRightColor ?? Color.FromArgb(50, colors[3])
    };

    var quadrantRects = new[] {
      new RectangleF(plotArea.Left, plotArea.Top, halfWidth, halfHeight),
      new RectangleF(plotArea.Left + halfWidth, plotArea.Top, halfWidth, halfHeight),
      new RectangleF(plotArea.Left, plotArea.Top + halfHeight, halfWidth, halfHeight),
      new RectangleF(plotArea.Left + halfWidth, plotArea.Top + halfHeight, halfWidth, halfHeight)
    };

    var quadrantLabels = new[] {
      quadrant.TopLeftLabel,
      quadrant.TopRightLabel,
      quadrant.BottomLeftLabel,
      quadrant.BottomRightLabel
    };

    for (var i = 0; i < 4; ++i) {
      var rect = quadrantRects[i];
      rect.Width *= (float)context.AnimationProgress;
      rect.Height *= (float)context.AnimationProgress;

      if (i == 1 || i == 3)
        rect.X = plotArea.Left + plotArea.Width - rect.Width;
      if (i >= 2)
        rect.Y = plotArea.Top + plotArea.Height - rect.Height;

      using (var brush = new SolidBrush(quadrantColors[i]))
        g.FillRectangle(brush, rect);

      // Draw quadrant label
      if (!string.IsNullOrEmpty(quadrantLabels[i])) {
        using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.2f, FontStyle.Bold);
        var labelSize = g.MeasureString(quadrantLabels[i], font);
        var labelX = quadrantRects[i].Left + (quadrantRects[i].Width - labelSize.Width) / 2;
        var labelY = quadrantRects[i].Top + 10;
        using var brush = new SolidBrush(Color.FromArgb(150, context.Diagram.ForeColor));
        g.DrawString(quadrantLabels[i], font, brush, labelX, labelY);
      }
    }

    // Draw axes
    using (var axisPen = new Pen(context.Diagram.ForeColor, 2)) {
      g.DrawLine(axisPen, plotArea.Left, plotArea.Top + halfHeight, plotArea.Right, plotArea.Top + halfHeight);
      g.DrawLine(axisPen, plotArea.Left + halfWidth, plotArea.Top, plotArea.Left + halfWidth, plotArea.Bottom);
    }

    // Draw axis labels
    if (!string.IsNullOrEmpty(quadrant.XAxisLabel)) {
      var labelSize = g.MeasureString(quadrant.XAxisLabel, context.Diagram.Font);
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(quadrant.XAxisLabel, context.Diagram.Font, brush, plotArea.Right - labelSize.Width - 5, plotArea.Top + halfHeight + 5);
    }

    if (!string.IsNullOrEmpty(quadrant.YAxisLabel)) {
      var labelSize = g.MeasureString(quadrant.YAxisLabel, context.Diagram.Font);
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(quadrant.YAxisLabel, context.Diagram.Font, brush, plotArea.Left + halfWidth + 5, plotArea.Top + 5);
    }

    // Draw items
    if (items != null) {
      for (var i = 0; i < items.Count; ++i) {
        var item = items[i];
        var x = plotArea.Left + (float)(item.X / 100.0 * plotArea.Width);
        var y = plotArea.Bottom - (float)(item.Y / 100.0 * plotArea.Height);
        var size = (float)(10 + item.Size * 10) * (float)context.AnimationProgress;

        var color = item.Color ?? colors[i % colors.Length];

        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, x - size / 2, y - size / 2, size, size);

        using (var pen = new Pen(Darken(color), 1.5f))
          g.DrawEllipse(pen, x - size / 2, y - size / 2, size, size);

        // Draw label
        if (!string.IsNullOrEmpty(item.Label)) {
          var labelSize = g.MeasureString(item.Label, context.Diagram.Font);
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(item.Label, context.Diagram.Font, brush, x - labelSize.Width / 2, y + size / 2 + 2);
        }

        context.RegisterHitTestRect(
          item,
          new RectangleF(x - size / 2, y - size / 2, size, size)
        );
      }
    }
  }
}

/// <summary>
/// Renderer for SWOT analysis diagrams (fixed 2x2 grid with Strengths, Weaknesses, Opportunities, Threats).
/// </summary>
public class SWOTDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.SWOT;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var quadrants = context.Diagram.Quadrants;
    var items = context.Diagram.MatrixItems;

    var plotArea = context.PlotArea;

    // SWOT specific colors
    var swotColors = new[] {
      Color.FromArgb(80, 76, 175, 80),   // Strengths - Green (top-left)
      Color.FromArgb(80, 244, 67, 54),   // Weaknesses - Red (top-right)
      Color.FromArgb(80, 33, 150, 243),  // Opportunities - Blue (bottom-left)
      Color.FromArgb(80, 255, 152, 0)    // Threats - Orange (bottom-right)
    };

    var swotLabels = new[] { "Strengths", "Weaknesses", "Opportunities", "Threats" };

    // Use custom labels if provided
    if (quadrants?.Count > 0) {
      var q = quadrants[0];
      if (!string.IsNullOrEmpty(q.TopLeftLabel)) swotLabels[0] = q.TopLeftLabel;
      if (!string.IsNullOrEmpty(q.TopRightLabel)) swotLabels[1] = q.TopRightLabel;
      if (!string.IsNullOrEmpty(q.BottomLeftLabel)) swotLabels[2] = q.BottomLeftLabel;
      if (!string.IsNullOrEmpty(q.BottomRightLabel)) swotLabels[3] = q.BottomRightLabel;

      if (q.TopLeftColor.HasValue) swotColors[0] = Color.FromArgb(80, q.TopLeftColor.Value);
      if (q.TopRightColor.HasValue) swotColors[1] = Color.FromArgb(80, q.TopRightColor.Value);
      if (q.BottomLeftColor.HasValue) swotColors[2] = Color.FromArgb(80, q.BottomLeftColor.Value);
      if (q.BottomRightColor.HasValue) swotColors[3] = Color.FromArgb(80, q.BottomRightColor.Value);
    }

    var halfWidth = plotArea.Width / 2;
    var halfHeight = plotArea.Height / 2;

    var quadrantRects = new[] {
      new RectangleF(plotArea.Left, plotArea.Top, halfWidth, halfHeight),
      new RectangleF(plotArea.Left + halfWidth, plotArea.Top, halfWidth, halfHeight),
      new RectangleF(plotArea.Left, plotArea.Top + halfHeight, halfWidth, halfHeight),
      new RectangleF(plotArea.Left + halfWidth, plotArea.Top + halfHeight, halfWidth, halfHeight)
    };

    // Draw quadrants with animation
    for (var i = 0; i < 4; ++i) {
      var rect = quadrantRects[i];
      var animatedRect = new RectangleF(
        rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
        rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      using (var brush = new SolidBrush(swotColors[i]))
        g.FillRectangle(brush, animatedRect);

      using (var pen = new Pen(Color.FromArgb(150, Color.Gray), 1))
        g.DrawRectangle(pen, animatedRect.X, animatedRect.Y, animatedRect.Width, animatedRect.Height);

      // Draw section header
      using var headerFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.3f, FontStyle.Bold);
      var headerSize = g.MeasureString(swotLabels[i], headerFont);
      var headerX = rect.Left + (rect.Width - headerSize.Width) / 2;
      var headerY = rect.Top + 15;
      using var headerBrush = new SolidBrush(Darken(Color.FromArgb(swotColors[i].R, swotColors[i].G, swotColors[i].B)));
      g.DrawString(swotLabels[i], headerFont, headerBrush, headerX, headerY);
    }

    // Draw items within their respective quadrants
    if (items != null) {
      var itemsByQuadrant = items.GroupBy(item => {
        // Determine quadrant based on X/Y position (0-100 scale)
        if (item.X < 50 && item.Y >= 50) return 0; // Top-left (Strengths)
        if (item.X >= 50 && item.Y >= 50) return 1; // Top-right (Weaknesses)
        if (item.X < 50 && item.Y < 50) return 2; // Bottom-left (Opportunities)
        return 3; // Bottom-right (Threats)
      });

      foreach (var group in itemsByQuadrant) {
        var quadrantIndex = group.Key;
        var rect = quadrantRects[quadrantIndex];
        var yOffset = 45f;

        foreach (var item in group) {
          if (yOffset > rect.Height - 20)
            break;

          var bulletX = rect.Left + 15;
          var textX = rect.Left + 30;
          var itemY = rect.Top + yOffset;

          // Draw bullet point
          using (var bulletBrush = new SolidBrush(context.Diagram.ForeColor))
            g.FillEllipse(bulletBrush, bulletX, itemY + 4, 6, 6);

          // Draw item text
          var label = item.Label ?? item.Id;
          using var textBrush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(label, context.Diagram.Font, textBrush, textX, itemY);

          yOffset += g.MeasureString(label, context.Diagram.Font).Height + 5;

          context.RegisterHitTestRect(
            item,
            new RectangleF(bulletX, itemY, rect.Width - 30, 20)
          );
        }
      }
    }
  }
}

/// <summary>
/// Renderer for User Journey Map diagrams showing stages with actions and emotions.
/// </summary>
public class JourneyMapDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.JourneyMap;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var stages = context.Diagram.JourneyStages;

    if (stages == null || stages.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();
    var orderedStages = stages.OrderBy(s => s.Order).ToList();

    var stageWidth = plotArea.Width / orderedStages.Count;
    var headerHeight = 50f;
    var actionAreaHeight = plotArea.Height - headerHeight - 80;
    var emotionLineY = plotArea.Bottom - 40;

    // Draw stage backgrounds and headers
    for (var i = 0; i < orderedStages.Count; ++i) {
      var stage = orderedStages[i];
      var x = plotArea.Left + i * stageWidth;
      var color = colors[i % colors.Length];

      // Animate width
      var animatedWidth = stageWidth * (float)context.AnimationProgress;

      // Draw header background
      using (var brush = new SolidBrush(Color.FromArgb(60, color)))
        g.FillRectangle(brush, x, plotArea.Top, animatedWidth, headerHeight);

      // Draw header text
      using var headerFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.1f, FontStyle.Bold);
      var label = stage.Label ?? stage.Id;
      var labelSize = g.MeasureString(label, headerFont);
      using var headerBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(label, headerFont, headerBrush, x + (stageWidth - labelSize.Width) / 2, plotArea.Top + (headerHeight - labelSize.Height) / 2);

      // Draw vertical separator
      if (i < orderedStages.Count - 1) {
        using var sepPen = new Pen(Color.FromArgb(100, Color.Gray), 1) { DashStyle = DashStyle.Dash };
        g.DrawLine(sepPen, x + stageWidth, plotArea.Top, x + stageWidth, plotArea.Bottom - 50);
      }

      // Draw actions within this stage
      var actionY = plotArea.Top + headerHeight + 15;
      if (stage.Actions != null) {
        foreach (var action in stage.Actions) {
          if (actionY > plotArea.Top + headerHeight + actionAreaHeight - 20)
            break;

          // Draw action card
          var cardRect = new RectangleF(x + 5, actionY, stageWidth - 10, 35);
          var cardColor = this._GetEmotionColor(action.Score);

          using (var cardBrush = new SolidBrush(Color.FromArgb(40, cardColor)))
            FillRoundedRectangle(g, cardBrush, cardRect, 4);

          using (var cardPen = new Pen(cardColor, 1))
            DrawRoundedRectangle(g, cardPen, cardRect, 4);

          // Draw actor and action text
          var actionText = $"{action.Actor}: {action.Action}";
          using var textBrush = new SolidBrush(context.Diagram.ForeColor);
          var textRect = new RectangleF(cardRect.X + 5, cardRect.Y + 2, cardRect.Width - 10, cardRect.Height - 4);
          using var format = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
          g.DrawString(actionText, context.Diagram.Font, textBrush, textRect, format);

          // Draw emotion indicator
          var emotionX = cardRect.Right - 20;
          var emotionY = cardRect.Top + 10;
          this._DrawEmotionIcon(g, emotionX, emotionY, action.Score, 12);

          actionY += 42;

          context.RegisterHitTestRect(action, cardRect);
        }
      }

      context.RegisterHitTestRect(
        stage,
        new RectangleF(x, plotArea.Top, stageWidth, headerHeight)
      );
    }

    // Draw emotion trend line
    if (orderedStages.Any(s => s.Actions?.Count > 0)) {
      var points = new List<PointF>();
      for (var i = 0; i < orderedStages.Count; ++i) {
        var stage = orderedStages[i];
        var avgScore = stage.Actions?.Count > 0 ? stage.Actions.Average(a => a.Score) : 0;
        var x = plotArea.Left + i * stageWidth + stageWidth / 2;
        var y = emotionLineY - (float)(avgScore / 2.0 * 20);
        points.Add(new PointF(x, y));
      }

      if (points.Count > 1) {
        using var linePen = new Pen(Color.FromArgb(200, 100, 100, 100), 2);
        linePen.DashStyle = DashStyle.Dot;
        for (var i = 0; i < points.Count - 1; ++i) {
          var progress = (float)context.AnimationProgress;
          var endX = points[i].X + (points[i + 1].X - points[i].X) * progress;
          var endY = points[i].Y + (points[i + 1].Y - points[i].Y) * progress;
          g.DrawLine(linePen, points[i], new PointF(endX, endY));
        }

        // Draw points
        foreach (var point in points) {
          using var pointBrush = new SolidBrush(Color.FromArgb(52, 73, 94));
          g.FillEllipse(pointBrush, point.X - 5, point.Y - 5, 10, 10);
        }
      }
    }
  }

  private Color _GetEmotionColor(int score) {
    return score switch {
      >= 2 => Color.FromArgb(76, 175, 80),  // Very positive - Green
      1 => Color.FromArgb(139, 195, 74),     // Positive - Light Green
      0 => Color.FromArgb(158, 158, 158),    // Neutral - Gray
      -1 => Color.FromArgb(255, 152, 0),     // Negative - Orange
      _ => Color.FromArgb(244, 67, 54)       // Very negative - Red
    };
  }

  private void _DrawEmotionIcon(Graphics g, float x, float y, int score, float size) {
    var color = this._GetEmotionColor(score);
    using var brush = new SolidBrush(color);
    g.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
  }
}

/// <summary>
/// Renderer for BPMN (Business Process Model and Notation) diagrams.
/// </summary>
public class BPMNDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.BPMN;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var elements = context.Diagram.BPMNElements;
    var edges = context.Diagram.Edges;

    if (elements == null || elements.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Group elements by lane
    var lanes = elements.Where(e => e.ElementType == DiagramBPMNType.Lane).OrderBy(e => e.Position.Y).ToList();
    var pools = elements.Where(e => e.ElementType == DiagramBPMNType.Pool).ToList();
    var otherElements = elements.Where(e => e.ElementType != DiagramBPMNType.Lane && e.ElementType != DiagramBPMNType.Pool).ToList();

    // Draw pools and lanes first
    if (pools.Count > 0) {
      var poolHeight = plotArea.Height / pools.Count;
      for (var i = 0; i < pools.Count; ++i) {
        var pool = pools[i];
        var poolRect = new RectangleF(plotArea.Left, plotArea.Top + i * poolHeight, plotArea.Width * (float)context.AnimationProgress, poolHeight - 5);

        using (var brush = new SolidBrush(Color.FromArgb(30, colors[i % colors.Length])))
          g.FillRectangle(brush, poolRect);

        using (var pen = new Pen(colors[i % colors.Length], 2))
          g.DrawRectangle(pen, poolRect.X, poolRect.Y, poolRect.Width, poolRect.Height);

        // Draw pool name vertically on the left
        if (!string.IsNullOrEmpty(pool.Name)) {
          var state = g.Save();
          g.TranslateTransform(poolRect.Left + 15, poolRect.Top + poolRect.Height / 2);
          g.RotateTransform(-90);
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          var nameSize = g.MeasureString(pool.Name, context.Diagram.Font);
          g.DrawString(pool.Name, context.Diagram.Font, brush, -nameSize.Width / 2, -nameSize.Height / 2);
          g.Restore(state);
        }

        context.RegisterHitTestRect(pool, poolRect);
      }
    } else if (lanes.Count > 0) {
      var laneHeight = plotArea.Height / lanes.Count;
      for (var i = 0; i < lanes.Count; ++i) {
        var lane = lanes[i];
        var laneRect = new RectangleF(plotArea.Left, plotArea.Top + i * laneHeight, plotArea.Width * (float)context.AnimationProgress, laneHeight - 2);

        using (var brush = new SolidBrush(Color.FromArgb(20, colors[i % colors.Length])))
          g.FillRectangle(brush, laneRect);

        using (var pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash })
          g.DrawLine(pen, laneRect.Left, laneRect.Bottom, laneRect.Right, laneRect.Bottom);

        // Draw lane name
        if (!string.IsNullOrEmpty(lane.Name)) {
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(lane.Name, context.Diagram.Font, brush, laneRect.Left + 5, laneRect.Top + 5);
        }

        context.RegisterHitTestRect(lane, laneRect);
      }
    }

    // Check if elements have explicit positions
    var hasExplicitPositions = otherElements.Any(e => e.Position.X > 0 || e.Position.Y > 0);

    // Calculate element positions based on normalized coordinates or auto-layout
    var elementPositions = new Dictionary<string, RectangleF>();
    if (hasExplicitPositions) {
      foreach (var element in otherElements) {
        var x = plotArea.Left + 40 + element.Position.X / 100f * (plotArea.Width - 80);
        var y = plotArea.Top + 40 + element.Position.Y / 100f * (plotArea.Height - 80);
        var size = this._GetElementSize(element.ElementType);
        var rect = new RectangleF(x - size.Width / 2, y - size.Height / 2, size.Width, size.Height);
        elementPositions[element.Id] = rect;
      }
    } else {
      // Auto-layout: arrange elements in a flow pattern
      // Separate by type: events at edges, tasks in middle, gateways between
      var startEvents = otherElements.Where(e => e.ElementType == DiagramBPMNType.StartEvent).ToList();
      var endEvents = otherElements.Where(e => e.ElementType == DiagramBPMNType.EndEvent).ToList();
      var tasks = otherElements.Where(e => e.ElementType == DiagramBPMNType.Task || e.ElementType == DiagramBPMNType.SubProcess).ToList();
      var gateways = otherElements.Where(e => e.ElementType == DiagramBPMNType.Gateway).ToList();
      var otherBpmn = otherElements.Where(e => e.ElementType == DiagramBPMNType.IntermediateEvent).ToList();

      var allOrdered = startEvents.Concat(tasks).Concat(gateways).Concat(otherBpmn).Concat(endEvents).ToList();
      var totalElements = allOrdered.Count;
      var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(totalElements * 1.5)));
      var rows = (int)Math.Ceiling((float)totalElements / cols);
      var cellWidth = (plotArea.Width - 60) / Math.Max(cols, 1);
      var cellHeight = (plotArea.Height - 80) / Math.Max(rows, 1);

      for (var i = 0; i < allOrdered.Count; ++i) {
        var element = allOrdered[i];
        var col = i % cols;
        var row = i / cols;
        var size = this._GetElementSize(element.ElementType);
        var x = plotArea.Left + 40 + col * cellWidth + (cellWidth - size.Width) / 2;
        var y = plotArea.Top + 50 + row * cellHeight + (cellHeight - size.Height) / 2;
        elementPositions[element.Id] = new RectangleF(x, y, size.Width, size.Height);
      }
    }

    // Draw sequence flows (edges)
    if (edges != null) {
      using var flowPen = new Pen(Color.FromArgb(52, 73, 94), 1.5f);
      flowPen.EndCap = LineCap.ArrowAnchor;

      foreach (var edge in edges) {
        if (!elementPositions.TryGetValue(edge.Source, out var sourceRect) ||
            !elementPositions.TryGetValue(edge.Target, out var targetRect))
          continue;

        var startPoint = new PointF(sourceRect.Right, sourceRect.Top + sourceRect.Height / 2);
        var endPoint = new PointF(targetRect.Left, targetRect.Top + targetRect.Height / 2);

        // Adjust for vertical connections
        if (Math.Abs(sourceRect.Left - targetRect.Left) < 50) {
          if (sourceRect.Bottom < targetRect.Top) {
            startPoint = new PointF(sourceRect.Left + sourceRect.Width / 2, sourceRect.Bottom);
            endPoint = new PointF(targetRect.Left + targetRect.Width / 2, targetRect.Top);
          } else {
            startPoint = new PointF(sourceRect.Left + sourceRect.Width / 2, sourceRect.Top);
            endPoint = new PointF(targetRect.Left + targetRect.Width / 2, targetRect.Bottom);
          }
        }

        g.DrawLine(flowPen, startPoint, endPoint);
      }
    }

    // Draw BPMN elements
    for (var i = 0; i < otherElements.Count; ++i) {
      var element = otherElements[i];
      if (!elementPositions.TryGetValue(element.Id, out var rect))
        continue;

      var color = element.Color ?? colors[i % colors.Length];
      this._DrawBPMNElement(g, context, element, rect, color);

      context.RegisterHitTestRect(element, rect);
    }
  }

  private SizeF _GetElementSize(DiagramBPMNType type) {
    return type switch {
      DiagramBPMNType.Task => new SizeF(100, 60),
      DiagramBPMNType.SubProcess => new SizeF(120, 80),
      DiagramBPMNType.Gateway => new SizeF(40, 40),
      DiagramBPMNType.StartEvent or DiagramBPMNType.EndEvent or DiagramBPMNType.IntermediateEvent => new SizeF(30, 30),
      _ => new SizeF(80, 50)
    };
  }

  private void _DrawBPMNElement(Graphics g, DiagramRenderContext context, DiagramBPMNElement element, RectangleF rect, Color color) {
    switch (element.ElementType) {
      case DiagramBPMNType.Task:
        this._DrawTask(g, context, element, rect, color);
        break;
      case DiagramBPMNType.SubProcess:
        this._DrawSubProcess(g, context, element, rect, color);
        break;
      case DiagramBPMNType.Gateway:
        this._DrawGateway(g, context, element, rect, color);
        break;
      case DiagramBPMNType.StartEvent:
        this._DrawEvent(g, context, element, rect, Color.FromArgb(76, 175, 80), 2);
        break;
      case DiagramBPMNType.EndEvent:
        this._DrawEvent(g, context, element, rect, Color.FromArgb(244, 67, 54), 3);
        break;
      case DiagramBPMNType.IntermediateEvent:
        this._DrawEvent(g, context, element, rect, Color.FromArgb(255, 152, 0), 2);
        break;
    }
  }

  private void _DrawTask(Graphics g, DiagramRenderContext context, DiagramBPMNElement element, RectangleF rect, Color color) {
    using (var brush = new SolidBrush(Color.FromArgb(240, 248, 255)))
      FillRoundedRectangle(g, brush, rect, 8);

    using (var pen = new Pen(color, 2))
      DrawRoundedRectangle(g, pen, rect, 8);

    // Draw task name
    if (!string.IsNullOrEmpty(element.Name)) {
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString(element.Name, context.Diagram.Font, brush, rect, format);
    }
  }

  private void _DrawSubProcess(Graphics g, DiagramRenderContext context, DiagramBPMNElement element, RectangleF rect, Color color) {
    using (var brush = new SolidBrush(Color.FromArgb(255, 250, 240)))
      FillRoundedRectangle(g, brush, rect, 8);

    using (var pen = new Pen(color, 2))
      DrawRoundedRectangle(g, pen, rect, 8);

    // Draw collapse indicator (+)
    var indicatorRect = new RectangleF(rect.Left + rect.Width / 2 - 8, rect.Bottom - 18, 16, 16);
    using (var pen = new Pen(color, 1))
      g.DrawRectangle(pen, indicatorRect.X, indicatorRect.Y, indicatorRect.Width, indicatorRect.Height);

    using (var pen = new Pen(color, 1)) {
      g.DrawLine(pen, indicatorRect.Left + 4, indicatorRect.Top + 8, indicatorRect.Right - 4, indicatorRect.Top + 8);
      g.DrawLine(pen, indicatorRect.Left + 8, indicatorRect.Top + 4, indicatorRect.Left + 8, indicatorRect.Bottom - 4);
    }

    // Draw name
    if (!string.IsNullOrEmpty(element.Name)) {
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      var textRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - 20);
      g.DrawString(element.Name, context.Diagram.Font, brush, textRect, format);
    }
  }

  private void _DrawGateway(Graphics g, DiagramRenderContext context, DiagramBPMNElement element, RectangleF rect, Color color) {
    var center = new PointF(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
    var size = rect.Width / 2;

    var points = new[] {
      new PointF(center.X, center.Y - size),
      new PointF(center.X + size, center.Y),
      new PointF(center.X, center.Y + size),
      new PointF(center.X - size, center.Y)
    };

    using (var brush = new SolidBrush(Color.FromArgb(255, 253, 231)))
      g.FillPolygon(brush, points);

    using (var pen = new Pen(color, 2))
      g.DrawPolygon(pen, points);

    // Draw gateway type indicator
    var gatewayType = element.GatewayType ?? DiagramGatewayType.Exclusive;
    using var symbolPen = new Pen(color, 2);

    switch (gatewayType) {
      case DiagramGatewayType.Exclusive:
        // X symbol
        g.DrawLine(symbolPen, center.X - 8, center.Y - 8, center.X + 8, center.Y + 8);
        g.DrawLine(symbolPen, center.X + 8, center.Y - 8, center.X - 8, center.Y + 8);
        break;
      case DiagramGatewayType.Parallel:
        // + symbol
        g.DrawLine(symbolPen, center.X - 8, center.Y, center.X + 8, center.Y);
        g.DrawLine(symbolPen, center.X, center.Y - 8, center.X, center.Y + 8);
        break;
      case DiagramGatewayType.Inclusive:
        // O symbol
        g.DrawEllipse(symbolPen, center.X - 8, center.Y - 8, 16, 16);
        break;
      case DiagramGatewayType.EventBased:
        // Pentagon
        g.DrawEllipse(symbolPen, center.X - 8, center.Y - 8, 16, 16);
        g.DrawEllipse(symbolPen, center.X - 5, center.Y - 5, 10, 10);
        break;
    }
  }

  private void _DrawEvent(Graphics g, DiagramRenderContext context, DiagramBPMNElement element, RectangleF rect, Color color, float penWidth) {
    using (var brush = new SolidBrush(Color.White))
      g.FillEllipse(brush, rect);

    using (var pen = new Pen(color, penWidth))
      g.DrawEllipse(pen, rect);

    // Draw name below
    if (!string.IsNullOrEmpty(element.Name)) {
      var nameSize = g.MeasureString(element.Name, context.Diagram.Font);
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(element.Name, context.Diagram.Font, brush, rect.Left + rect.Width / 2 - nameSize.Width / 2, rect.Bottom + 5);
    }
  }
}

/// <summary>
/// Renderer for Kanban board diagrams with columns and cards.
/// </summary>
public class KanbanDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Kanban;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var columns = context.Diagram.KanbanColumns;
    var cards = context.Diagram.KanbanCards;

    if (columns == null || columns.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();
    var orderedColumns = columns.OrderBy(c => c.Order).ToList();

    var columnWidth = (plotArea.Width - 10) / orderedColumns.Count;
    var headerHeight = 45f;
    var cardHeight = 60f;
    var cardMargin = 8f;

    // Draw columns
    for (var i = 0; i < orderedColumns.Count; ++i) {
      var column = orderedColumns[i];
      var x = plotArea.Left + 5 + i * columnWidth;
      var columnRect = new RectangleF(x, plotArea.Top, columnWidth - 5, plotArea.Height * (float)context.AnimationProgress);

      var columnColor = column.Color ?? colors[i % colors.Length];

      // Draw column background
      using (var brush = new SolidBrush(Color.FromArgb(30, columnColor)))
        FillRoundedRectangle(g, brush, columnRect, 6);

      using (var pen = new Pen(Color.FromArgb(100, columnColor), 1))
        DrawRoundedRectangle(g, pen, columnRect, 6);

      // Draw header
      var headerRect = new RectangleF(x, plotArea.Top, columnWidth - 5, headerHeight);
      using (var brush = new SolidBrush(Color.FromArgb(80, columnColor)))
        FillRoundedRectangle(g, brush, headerRect, 6);

      // Draw column name and WIP
      var columnLabel = column.Name ?? column.Id;
      var columnCards = cards?.Where(c => c.ColumnId == column.Id).ToList() ?? new List<DiagramKanbanCard>();
      var wipText = column.WipLimit > 0 ? $" ({columnCards.Count}/{column.WipLimit})" : $" ({columnCards.Count})";
      var fullLabel = columnLabel + wipText;

      using var headerFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.1f, FontStyle.Bold);
      var labelSize = g.MeasureString(fullLabel, headerFont);
      using var labelBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(fullLabel, headerFont, labelBrush, x + (columnWidth - 5 - labelSize.Width) / 2, plotArea.Top + (headerHeight - labelSize.Height) / 2);

      // Draw WIP limit warning
      if (column.WipLimit > 0 && columnCards.Count > column.WipLimit) {
        using var warningPen = new Pen(Color.FromArgb(244, 67, 54), 3);
        DrawRoundedRectangle(g, warningPen, headerRect, 6);
      }

      // Draw cards in this column
      var cardY = plotArea.Top + headerHeight + cardMargin;
      var orderedCards = columnCards.OrderBy(c => c.Order).ToList();

      foreach (var card in orderedCards) {
        if (cardY + cardHeight > plotArea.Bottom - 10)
          break;

        var cardRect = new RectangleF(x + cardMargin, cardY, columnWidth - 5 - cardMargin * 2, cardHeight);
        var cardColor = card.Color ?? Color.White;

        // Draw card shadow
        using (var shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black))) {
          var shadowRect = cardRect;
          shadowRect.Offset(2, 2);
          FillRoundedRectangle(g, shadowBrush, shadowRect, 4);
        }

        // Draw card background
        using (var brush = new SolidBrush(cardColor))
          FillRoundedRectangle(g, brush, cardRect, 4);

        using (var pen = new Pen(Color.FromArgb(100, Color.Gray), 1))
          DrawRoundedRectangle(g, pen, cardRect, 4);

        // Draw color indicator on left edge
        var indicatorRect = new RectangleF(cardRect.Left, cardRect.Top, 4, cardRect.Height);
        using (var brush = new SolidBrush(columnColor))
          FillRoundedRectangle(g, brush, indicatorRect, 2);

        // Draw card title
        var title = card.Title ?? card.Id;
        using var titleBrush = new SolidBrush(context.Diagram.ForeColor);
        using var titleFormat = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
        var titleRect = new RectangleF(cardRect.Left + 10, cardRect.Top + 8, cardRect.Width - 16, 20);
        g.DrawString(title, context.Diagram.Font, titleBrush, titleRect, titleFormat);

        // Draw assignee
        if (!string.IsNullOrEmpty(card.Assignee)) {
          using var assigneeFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.85f);
          using var assigneeBrush = new SolidBrush(Color.FromArgb(150, context.Diagram.ForeColor));
          g.DrawString(card.Assignee, assigneeFont, assigneeBrush, cardRect.Left + 10, cardRect.Bottom - 22);

          // Draw avatar circle
          var avatarSize = 18f;
          var avatarX = cardRect.Right - avatarSize - 8;
          var avatarY = cardRect.Bottom - avatarSize - 5;
          using (var avatarBrush = new SolidBrush(Color.FromArgb(100, columnColor)))
            g.FillEllipse(avatarBrush, avatarX, avatarY, avatarSize, avatarSize);

          // Draw initials
          var initials = card.Assignee.Length > 0 ? card.Assignee[0].ToString().ToUpper() : "?";
          using var initialFont = new Font(context.Diagram.Font.FontFamily, 8f, FontStyle.Bold);
          var initialSize = g.MeasureString(initials, initialFont);
          using var initialBrush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(initials, initialFont, initialBrush, avatarX + (avatarSize - initialSize.Width) / 2, avatarY + (avatarSize - initialSize.Height) / 2);
        }

        context.RegisterHitTestRect(card, cardRect);
        cardY += cardHeight + cardMargin;
      }

      context.RegisterHitTestRect(column, headerRect);
    }
  }
}
