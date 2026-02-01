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
/// Renderer for simple block diagrams with labeled blocks and connectors.
/// </summary>
public class BlockDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.BlockDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    // Position blocks in a grid with proper non-overlapping layout
    var blockWidth = 120f;
    var blockHeight = 60f;
    var margin = 30f;
    var positions = new Dictionary<string, RectangleF>();

    // Check if positions are explicitly set
    var hasExplicitPositions = nodes.Any(n => n.Position.X > 0 || n.Position.Y > 0);

    if (hasExplicitPositions) {
      for (var i = 0; i < nodes.Count; ++i) {
        var node = nodes[i];
        float x, y;
        if (node.Position.X > 0 || node.Position.Y > 0) {
          x = plotArea.Left + node.Position.X / 100f * (plotArea.Width - blockWidth);
          y = plotArea.Top + node.Position.Y / 100f * (plotArea.Height - blockHeight);
        } else {
          x = plotArea.Left + plotArea.Width / 2 - blockWidth / 2;
          y = plotArea.Top + plotArea.Height / 2 - blockHeight / 2;
        }
        positions[node.Id] = new RectangleF(x, y, blockWidth, blockHeight);
      }
    } else {
      // Auto-layout: use proper grid with margins to avoid overlap
      var gridLayout = CalculateGridLayout(nodes.Count, blockWidth, blockHeight, margin, plotArea);
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

    // Draw connections
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var fromRect) ||
            !positions.TryGetValue(edge.Target, out var toRect))
          continue;

        var from = new PointF(fromRect.Left + fromRect.Width / 2, fromRect.Bottom);
        var to = new PointF(toRect.Left + toRect.Width / 2, toRect.Top);

        // Adjust connection points based on relative position
        if (fromRect.Right < toRect.Left) {
          from = new PointF(fromRect.Right, fromRect.Top + fromRect.Height / 2);
          to = new PointF(toRect.Left, toRect.Top + toRect.Height / 2);
        } else if (fromRect.Left > toRect.Right) {
          from = new PointF(fromRect.Left, fromRect.Top + fromRect.Height / 2);
          to = new PointF(toRect.Right, toRect.Top + toRect.Height / 2);
        }

        // Animate
        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        var lineColor = edge.Color ?? Color.FromArgb(80, 80, 80);
        using var pen = new Pen(lineColor, 2);
        if (edge.Directed)
          pen.EndCap = LineCap.ArrowAnchor;

        g.DrawLine(pen, from, to);

        // Draw label
        if (!string.IsNullOrEmpty(edge.Label)) {
          var midPoint = new PointF((from.X + to.X) / 2, (from.Y + to.Y) / 2);
          var labelSize = g.MeasureString(edge.Label, context.Diagram.Font);
          using var brush = new SolidBrush(context.Diagram.ForeColor);
          g.DrawString(edge.Label, context.Diagram.Font, brush, midPoint.X - labelSize.Width / 2, midPoint.Y - labelSize.Height - 3);
        }
      }
    }

    // Draw blocks
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var rect))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      var animatedRect = new RectangleF(
        rect.X + rect.Width * (1 - (float)context.AnimationProgress) / 2,
        rect.Y + rect.Height * (1 - (float)context.AnimationProgress) / 2,
        rect.Width * (float)context.AnimationProgress,
        rect.Height * (float)context.AnimationProgress
      );

      using (var brush = new SolidBrush(color))
        FillRoundedRectangle(g, brush, animatedRect, 6);

      using (var pen = new Pen(Darken(color), 2))
        DrawRoundedRectangle(g, pen, animatedRect, 6);

      // Draw label
      if (!string.IsNullOrEmpty(node.Label)) {
        using var brush = new SolidBrush(GetContrastColor(color));
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(node.Label, context.Diagram.Font, brush, animatedRect, format);
      }

      context.RegisterHitTestRect(node, animatedRect);
    }
  }
}

/// <summary>
/// Renderer for server rack diagrams showing U positions.
/// </summary>
public class RackDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.RackDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var racks = context.Diagram.Racks;
    var devices = context.Diagram.RackDevices;

    if (racks == null || racks.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    var rackWidth = Math.Min(200f, (plotArea.Width - 40) / racks.Count - 20);
    var baseUnitHeight = 18f;

    // Calculate the maximum rack height and scale if needed
    var maxUnits = racks.Max(r => r.TotalUnits);
    var maxRackHeight = maxUnits * baseUnitHeight + 40;
    var availableHeight = plotArea.Height - 50;
    var unitHeight = maxRackHeight > availableHeight ? baseUnitHeight * (availableHeight / maxRackHeight) : baseUnitHeight;

    for (var r = 0; r < racks.Count; ++r) {
      var rack = racks[r];
      var rackX = plotArea.Left + 20 + r * (rackWidth + 30);
      var rackHeight = rack.TotalUnits * unitHeight + 40;
      var rackRect = new RectangleF(rackX, plotArea.Top + 30, rackWidth, rackHeight * (float)context.AnimationProgress);

      // Draw rack frame
      using (var brush = new SolidBrush(Color.FromArgb(50, 50, 50)))
        g.FillRectangle(brush, rackRect);

      using (var pen = new Pen(Color.FromArgb(30, 30, 30), 2))
        g.DrawRectangle(pen, rackRect.X, rackRect.Y, rackRect.Width, rackRect.Height);

      // Draw rack name
      if (!string.IsNullOrEmpty(rack.Name)) {
        using var nameFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 1.1f, FontStyle.Bold);
        var nameSize = g.MeasureString(rack.Name, nameFont);
        using var nameBrush = new SolidBrush(context.Diagram.ForeColor);
        g.DrawString(rack.Name, nameFont, nameBrush, rackX + (rackWidth - nameSize.Width) / 2, plotArea.Top + 5);
      }

      // Draw U markers
      for (var u = 1; u <= rack.TotalUnits; ++u) {
        var y = rackRect.Bottom - (u * unitHeight);
        if (y < rackRect.Top) break;

        // Draw U number
        using var uFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.65f);
        using var uBrush = new SolidBrush(Color.FromArgb(150, 150, 150));
        g.DrawString(u.ToString(), uFont, uBrush, rackX + 2, y + 2);

        // Draw grid line
        using var gridPen = new Pen(Color.FromArgb(70, 70, 70), 0.5f);
        g.DrawLine(gridPen, rackX + 20, y, rackX + rackWidth - 5, y);
      }

      // Draw devices in this rack
      var rackDevices = devices?.Where(d => d.RackId == rack.Id).ToList() ?? new List<DiagramRackDevice>();
      for (var d = 0; d < rackDevices.Count; ++d) {
        var device = rackDevices[d];
        var deviceY = rackRect.Bottom - (device.StartUnit + device.UnitHeight - 1) * unitHeight;
        var deviceHeight = device.UnitHeight * unitHeight - 2;
        var deviceRect = new RectangleF(rackX + 22, deviceY, rackWidth - 30, deviceHeight);

        if (deviceRect.Top < rackRect.Top || deviceRect.Bottom > rackRect.Bottom)
          continue;

        var deviceColor = device.Color ?? this._GetDeviceColor(device.DeviceType);

        // Draw device
        using (var brush = new SolidBrush(deviceColor))
          FillRoundedRectangle(g, brush, deviceRect, 3);

        using (var pen = new Pen(Darken(deviceColor), 1))
          DrawRoundedRectangle(g, pen, deviceRect, 3);

        // Draw device name
        if (!string.IsNullOrEmpty(device.Name)) {
          using var deviceFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.75f);
          using var deviceBrush = new SolidBrush(GetContrastColor(deviceColor));
          using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
          g.DrawString(device.Name, deviceFont, deviceBrush, deviceRect, format);
        }

        context.RegisterHitTestRect(device, deviceRect);
      }

      context.RegisterHitTestRect(rack, rackRect);
    }
  }

  private Color _GetDeviceColor(string deviceType) {
    return (deviceType?.ToLower()) switch {
      "server" => Color.FromArgb(52, 152, 219),
      "switch" => Color.FromArgb(46, 204, 113),
      "storage" => Color.FromArgb(155, 89, 182),
      "pdu" => Color.FromArgb(241, 196, 15),
      "firewall" => Color.FromArgb(231, 76, 60),
      _ => Color.FromArgb(149, 165, 166)
    };
  }
}

/// <summary>
/// Renderer for network topology diagrams with various layouts.
/// </summary>
public class NetworkTopologyDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.NetworkTopology;

  /// <summary>Gets or sets the network layout type.</summary>
  public NetworkLayout Layout { get; set; } = NetworkLayout.Star;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var nodes = context.Diagram.Nodes;
    var edges = context.Diagram.Edges;

    if (nodes == null || nodes.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();
    var center = new PointF(plotArea.Left + plotArea.Width / 2, plotArea.Top + plotArea.Height / 2);
    var radius = Math.Min(plotArea.Width, plotArea.Height) * 0.35f;

    var positions = new Dictionary<string, PointF>();

    // Calculate positions based on layout
    switch (this.Layout) {
      case NetworkLayout.Star:
        this._LayoutStar(nodes, center, radius, positions);
        break;
      case NetworkLayout.Ring:
        this._LayoutRing(nodes, center, radius, positions);
        break;
      case NetworkLayout.Bus:
        this._LayoutBus(nodes, plotArea, positions);
        break;
      case NetworkLayout.Mesh:
      case NetworkLayout.FullMesh:
        this._LayoutMesh(nodes, center, radius, positions);
        break;
      default:
        this._LayoutStar(nodes, center, radius, positions);
        break;
    }

    // Draw connections
    if (edges != null) {
      foreach (var edge in edges) {
        if (!positions.TryGetValue(edge.Source, out var from) ||
            !positions.TryGetValue(edge.Target, out var to))
          continue;

        to = new PointF(
          from.X + (to.X - from.X) * (float)context.AnimationProgress,
          from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
        );

        var lineColor = edge.Color ?? Color.FromArgb(100, 100, 100);
        using var pen = new Pen(lineColor, 2);
        g.DrawLine(pen, from, to);
      }
    } else if (this.Layout == NetworkLayout.FullMesh) {
      // Auto-draw full mesh connections
      var nodeList = nodes.ToList();
      for (var i = 0; i < nodeList.Count; ++i) {
        for (var j = i + 1; j < nodeList.Count; ++j) {
          if (!positions.TryGetValue(nodeList[i].Id, out var from) ||
              !positions.TryGetValue(nodeList[j].Id, out var to))
            continue;

          to = new PointF(
            from.X + (to.X - from.X) * (float)context.AnimationProgress,
            from.Y + (to.Y - from.Y) * (float)context.AnimationProgress
          );

          using var pen = new Pen(Color.FromArgb(80, 100, 100, 100), 1);
          g.DrawLine(pen, from, to);
        }
      }
    }

    // Draw nodes
    var nodeSize = 40f;
    for (var i = 0; i < nodes.Count; ++i) {
      var node = nodes[i];
      if (!positions.TryGetValue(node.Id, out var pos))
        continue;

      var color = node.Color ?? colors[i % colors.Length];
      var animatedSize = nodeSize * (float)context.AnimationProgress;
      var nodeRect = new RectangleF(pos.X - animatedSize / 2, pos.Y - animatedSize / 2, animatedSize, animatedSize);

      this._DrawNetworkNode(g, context, node, nodeRect, color);
      context.RegisterHitTestRect(node, nodeRect);
    }
  }

  private void _LayoutStar(IList<DiagramNode> nodes, PointF center, float radius, Dictionary<string, PointF> positions) {
    if (nodes.Count == 0) return;

    // First node is the hub
    positions[nodes[0].Id] = center;

    // Other nodes around the hub
    for (var i = 1; i < nodes.Count; ++i) {
      var angle = (360.0 / (nodes.Count - 1) * (i - 1) - 90) * Math.PI / 180;
      positions[nodes[i].Id] = new PointF(
        center.X + (float)(Math.Cos(angle) * radius),
        center.Y + (float)(Math.Sin(angle) * radius)
      );
    }
  }

  private void _LayoutRing(IList<DiagramNode> nodes, PointF center, float radius, Dictionary<string, PointF> positions) {
    for (var i = 0; i < nodes.Count; ++i) {
      var angle = (360.0 / nodes.Count * i - 90) * Math.PI / 180;
      positions[nodes[i].Id] = new PointF(
        center.X + (float)(Math.Cos(angle) * radius),
        center.Y + (float)(Math.Sin(angle) * radius)
      );
    }
  }

  private void _LayoutBus(IList<DiagramNode> nodes, RectangleF plotArea, Dictionary<string, PointF> positions) {
    var spacing = plotArea.Width / (nodes.Count + 1);
    for (var i = 0; i < nodes.Count; ++i)
      positions[nodes[i].Id] = new PointF(
        plotArea.Left + spacing * (i + 1),
        plotArea.Top + plotArea.Height / 2
      );
  }

  private void _LayoutMesh(IList<DiagramNode> nodes, PointF center, float radius, Dictionary<string, PointF> positions) {
    this._LayoutRing(nodes, center, radius, positions);
  }

  private void _DrawNetworkNode(Graphics g, DiagramRenderContext context, DiagramNode node, RectangleF rect, Color color) {
    // Draw based on shape or guess from label
    var nodeType = (node.Label?.ToLower()) switch {
      var s when s?.Contains("router") ?? false => "router",
      var s when s?.Contains("switch") ?? false => "switch",
      var s when s?.Contains("server") ?? false => "server",
      var s when s?.Contains("pc") ?? false || (s?.Contains("computer") ?? false) => "workstation",
      var s when s?.Contains("cloud") ?? false => "cloud",
      _ => "default"
    };

    switch (nodeType) {
      case "router":
        // Circle with arrows
        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, rect);
        using (var pen = new Pen(Darken(color), 2))
          g.DrawEllipse(pen, rect);
        break;

      case "switch":
        // Rectangle
        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);
        using (var pen = new Pen(Darken(color), 2))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        break;

      case "cloud":
        // Cloud shape (simplified as ellipse)
        using (var brush = new SolidBrush(Color.FromArgb(100, color)))
          g.FillEllipse(brush, rect.X - 5, rect.Y, rect.Width + 10, rect.Height);
        break;

      default:
        // Default circle
        using (var brush = new SolidBrush(color))
          g.FillEllipse(brush, rect);
        using (var pen = new Pen(Darken(color), 2))
          g.DrawEllipse(pen, rect);
        break;
    }

    // Draw label
    if (!string.IsNullOrEmpty(node.Label)) {
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
      var labelSize = g.MeasureString(node.Label, font);
      using var brush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(node.Label, font, brush, rect.Left + rect.Width / 2 - labelSize.Width / 2, rect.Bottom + 3);
    }
  }
}

/// <summary>
/// Renderer for network packet structure diagrams.
/// </summary>
public class PacketDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.PacketDiagram;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var fields = context.Diagram.PacketFields;

    if (fields == null || fields.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    var totalBits = fields.Sum(f => f.Bits);
    var bitsPerRow = 32;
    var rowHeight = 40f;
    var bitWidth = (plotArea.Width - 60) / bitsPerRow;
    var headerHeight = 25f;

    // Draw bit position header
    for (var i = 0; i < bitsPerRow; ++i) {
      var x = plotArea.Left + 30 + i * bitWidth;
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.65f);
      using var brush = new SolidBrush(Color.Gray);
      g.DrawString(i.ToString(), font, brush, x + bitWidth / 2 - 4, plotArea.Top + 5);
    }

    // Draw fields
    var currentBit = 0;
    var fieldIndex = 0;

    foreach (var field in fields) {
      var startBit = currentBit;
      var endBit = currentBit + field.Bits - 1;

      for (var bit = startBit; bit <= endBit;) {
        var row = bit / bitsPerRow;
        var colStart = bit % bitsPerRow;
        var colEnd = Math.Min(colStart + (endBit - bit), bitsPerRow - 1);
        var bitsInSegment = colEnd - colStart + 1;

        var x = plotArea.Left + 30 + colStart * bitWidth;
        var y = plotArea.Top + headerHeight + row * rowHeight;
        var width = bitsInSegment * bitWidth * (float)context.AnimationProgress;
        var rect = new RectangleF(x, y, width, rowHeight - 2);

        var color = field.Color ?? colors[fieldIndex % colors.Length];

        using (var brush = new SolidBrush(color))
          g.FillRectangle(brush, rect);

        using (var pen = new Pen(Darken(color), 1))
          g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

        // Draw field name and value (only in first segment if field spans multiple rows)
        if (bit == startBit || colStart == 0) {
          var text = field.Name;
          if (!string.IsNullOrEmpty(field.Value))
            text += $"\n{field.Value}";

          using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.8f);
          using var brush = new SolidBrush(GetContrastColor(color));
          using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
          g.DrawString(text, font, brush, rect, format);
        }

        context.RegisterHitTestRect(field, rect);
        bit += bitsInSegment;
      }

      currentBit = endBit + 1;
      ++fieldIndex;
    }

    // Draw row labels (byte positions)
    var numRows = (totalBits + bitsPerRow - 1) / bitsPerRow;
    for (var row = 0; row < numRows; ++row) {
      var y = plotArea.Top + headerHeight + row * rowHeight + rowHeight / 2;
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.7f);
      using var brush = new SolidBrush(Color.Gray);
      g.DrawString($"{row * bitsPerRow / 8}", font, brush, plotArea.Left + 5, y - font.Height / 2);
    }
  }
}

/// <summary>
/// Renderer for byte field / binary data structure diagrams.
/// </summary>
public class ByteFieldDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.ByteField;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var fields = context.Diagram.ByteFields;

    if (fields == null || fields.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();

    var totalBits = fields.Max(f => f.EndBit) + 1;
    var bitWidth = Math.Min(20f, (plotArea.Width - 80) / totalBits);
    var rowHeight = 50f;

    // Draw bit position header
    for (var i = 0; i < totalBits; ++i) {
      var x = plotArea.Left + 40 + i * bitWidth;
      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.6f);
      using var brush = new SolidBrush(Color.Gray);
      g.DrawString(i.ToString(), font, brush, x + bitWidth / 2 - 3, plotArea.Top + 5);
    }

    // Draw grid
    for (var i = 0; i <= totalBits; ++i) {
      var x = plotArea.Left + 40 + i * bitWidth;
      using var pen = new Pen(Color.FromArgb(100, Color.Gray), 0.5f);
      g.DrawLine(pen, x, plotArea.Top + 20, x, plotArea.Top + 20 + rowHeight);
    }

    // Draw fields
    for (var i = 0; i < fields.Count; ++i) {
      var field = fields[i];
      var x = plotArea.Left + 40 + field.StartBit * bitWidth;
      var width = (field.EndBit - field.StartBit + 1) * bitWidth * (float)context.AnimationProgress;
      var rect = new RectangleF(x, plotArea.Top + 20, width, rowHeight);

      var color = field.Color ?? colors[i % colors.Length];

      using (var brush = new SolidBrush(Color.FromArgb(150, color)))
        g.FillRectangle(brush, rect);

      using (var pen = new Pen(color, 2))
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

      // Draw field name
      var text = field.Name;
      if (!string.IsNullOrEmpty(field.Value))
        text += $"\n{field.Value}";

      using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.85f);
      using var brush2 = new SolidBrush(context.Diagram.ForeColor);
      using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString(text, font, brush2, rect, format);

      // Draw bit range below
      var rangeText = field.StartBit == field.EndBit ? $"[{field.StartBit}]" : $"[{field.StartBit}:{field.EndBit}]";
      using var rangeFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.7f);
      var rangeSize = g.MeasureString(rangeText, rangeFont);
      using var rangeBrush = new SolidBrush(Color.Gray);
      g.DrawString(rangeText, rangeFont, rangeBrush, x + width / 2 - rangeSize.Width / 2, rect.Bottom + 3);

      context.RegisterHitTestRect(field, rect);
    }
  }
}

/// <summary>
/// Renderer for digital waveform / timing diagrams.
/// </summary>
public class WaveformDiagramRenderer : DiagramRenderer {
  /// <inheritdoc />
  public override DiagramType DiagramType => DiagramType.Waveform;

  /// <inheritdoc />
  public override void Render(DiagramRenderContext context) {
    var g = context.Graphics;
    var signals = context.Diagram.Signals;

    if (signals == null || signals.Count == 0)
      return;

    var plotArea = context.PlotArea;
    var colors = GetDefaultColors();
    var orderedSignals = signals.OrderBy(s => s.Order).ToList();

    var signalHeight = Math.Min(50f, (plotArea.Height - 40) / orderedSignals.Count);
    var labelWidth = 80f;
    var waveformWidth = plotArea.Width - labelWidth - 20;

    // Find time range
    var maxTime = signals.SelectMany(s => s.Transitions ?? new List<DiagramSignalTransition>()).Select(t => t.Time).DefaultIfEmpty(10).Max();
    var timeScale = waveformWidth / (float)maxTime;

    // Draw time axis
    using (var axisPen = new Pen(Color.FromArgb(150, Color.Gray), 1)) {
      g.DrawLine(axisPen, plotArea.Left + labelWidth, plotArea.Bottom - 20, plotArea.Right - 10, plotArea.Bottom - 20);

      // Draw time markers
      var numMarkers = Math.Max(1, (int)(waveformWidth / 50));
      for (var i = 0; i <= numMarkers; ++i) {
        var x = plotArea.Left + labelWidth + i * (waveformWidth / numMarkers);
        g.DrawLine(axisPen, x, plotArea.Bottom - 25, x, plotArea.Bottom - 15);

        var timeValue = maxTime * i / numMarkers;
        using var font = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.7f);
        using var brush = new SolidBrush(Color.Gray);
        g.DrawString(timeValue.ToString("F1"), font, brush, x - 10, plotArea.Bottom - 12);
      }
    }

    // Draw signals
    for (var s = 0; s < orderedSignals.Count; ++s) {
      var signal = orderedSignals[s];
      var signalY = plotArea.Top + 20 + s * signalHeight;
      var color = signal.Color ?? colors[s % colors.Length];

      // Draw signal label
      using var labelFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.9f);
      using var labelBrush = new SolidBrush(context.Diagram.ForeColor);
      g.DrawString(signal.Name ?? signal.Id, labelFont, labelBrush, plotArea.Left + 5, signalY + signalHeight / 2 - labelFont.Height / 2);

      // Draw waveform
      if (signal.Transitions == null || signal.Transitions.Count == 0)
        continue;

      var orderedTransitions = signal.Transitions.OrderBy(t => t.Time).ToList();
      using var wavePen = new Pen(color, 2);

      var highY = signalY + 8;
      var lowY = signalY + signalHeight - 8;
      var midY = signalY + signalHeight / 2;

      for (var t = 0; t < orderedTransitions.Count; ++t) {
        var transition = orderedTransitions[t];
        var x = plotArea.Left + labelWidth + (float)(transition.Time * timeScale) * (float)context.AnimationProgress;
        var nextX = t + 1 < orderedTransitions.Count
          ? plotArea.Left + labelWidth + (float)(orderedTransitions[t + 1].Time * timeScale) * (float)context.AnimationProgress
          : plotArea.Right - 10;

        var y = transition.Level switch {
          DiagramSignalLevel.High => highY,
          DiagramSignalLevel.Low => lowY,
          _ => midY
        };

        // Draw horizontal line at current level
        g.DrawLine(wavePen, x, y, nextX, y);

        // Draw vertical transition line
        if (t > 0) {
          var prevY = orderedTransitions[t - 1].Level switch {
            DiagramSignalLevel.High => highY,
            DiagramSignalLevel.Low => lowY,
            _ => midY
          };
          g.DrawLine(wavePen, x, prevY, x, y);
        }

        // Draw data value for bus signals
        if (signal.SignalType == DiagramSignalType.Bus && !string.IsNullOrEmpty(transition.Data)) {
          using var dataFont = new Font(context.Diagram.Font.FontFamily, context.Diagram.Font.Size * 0.7f);
          using var dataBrush = new SolidBrush(color);
          g.DrawString(transition.Data, dataFont, dataBrush, x + 3, midY - dataFont.Height / 2);
        }
      }

      context.RegisterHitTestRect(signal, new RectangleF(plotArea.Left, signalY, plotArea.Width, signalHeight));
    }
  }
}
