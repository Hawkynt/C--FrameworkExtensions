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

using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace System.Windows.Forms.Charting.Renderers;

/// <summary>Renders a choropleth map (regions colored by value).</summary>
public class ChoroplethRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.Choropleth;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;

    // Choropleth requires geographic region data
    // For demonstration, render a grid-based representation
    var mapData = context.Chart.MapData;
    if (mapData == null || mapData.Count == 0) {
      this.DrawPlaceholder(g, plotArea, "Choropleth Map\n(Requires geographic data)");
      return;
    }

    var minValue = mapData.Min(m => m.Value);
    var maxValue = mapData.Max(m => m.Value);
    var valueRange = maxValue - minValue;
    if (valueRange == 0)
      valueRange = 1;

    foreach (var region in mapData) {
      var intensity = (float)((region.Value - minValue) / valueRange);
      var color = this.InterpolateColor(Color.FromArgb(255, 255, 220), Color.FromArgb(178, 34, 34), intensity);

      if (region.Bounds.HasValue) {
        var bounds = region.Bounds.Value;
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, bounds);
        using var pen = new Pen(Color.DarkGray, 1);
        g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
      }
    }
  }

  private Color InterpolateColor(Color from, Color to, float t) {
    var r = (int)(from.R + (to.R - from.R) * t);
    var green = (int)(from.G + (to.G - from.G) * t);
    var b = (int)(from.B + (to.B - from.B) * t);
    return Color.FromArgb(r, green, b);
  }

  private void DrawPlaceholder(Graphics g, RectangleF plotArea, string text) {
    using var brush = new SolidBrush(Color.LightGray);
    g.FillRectangle(brush, plotArea);
    using var textBrush = new SolidBrush(Color.Gray);
    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    g.DrawString(text, SystemFonts.DefaultFont, textBrush, plotArea, format);
  }
}

/// <summary>Renders a geographic heatmap (intensity-based coloring on map).</summary>
public class GeographicHeatmapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.GeographicHeatmap;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    if (series.Count == 0 || series[0].Points.Count == 0) {
      this.DrawPlaceholder(g, plotArea, "Geographic Heatmap\n(Requires point data)");
      return;
    }

    // Draw background
    using var bgBrush = new SolidBrush(Color.FromArgb(240, 248, 255));
    g.FillRectangle(bgBrush, plotArea);

    var points = series[0].Points;
    var maxY = points.Max(p => p.Y);
    if (maxY == 0)
      maxY = 1;

    // Draw heat points
    foreach (var dp in points) {
      var x = (float)(plotArea.Left + dp.X / 100 * plotArea.Width);
      var y = (float)(plotArea.Top + (100 - dp.X) / 100 * plotArea.Height);
      var intensity = (float)(dp.Y / maxY);
      var radius = 20 + intensity * 30;

      using var path = new System.Drawing.Drawing2D.GraphicsPath();
      path.AddEllipse(x - radius, y - radius, radius * 2, radius * 2);

      using var brush = new System.Drawing.Drawing2D.PathGradientBrush(path) {
        CenterColor = Color.FromArgb((int)(intensity * 200), Color.Red),
        SurroundColors = new[] { Color.Transparent }
      };
      g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
    }
  }

  private void DrawPlaceholder(Graphics g, RectangleF plotArea, string text) {
    using var brush = new SolidBrush(Color.LightGray);
    g.FillRectangle(brush, plotArea);
    using var textBrush = new SolidBrush(Color.Gray);
    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    g.DrawString(text, SystemFonts.DefaultFont, textBrush, plotArea, format);
  }
}

/// <summary>Renders a tile map (grid-based geographic representation).</summary>
public class TileMapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.TileMap;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    var cols = 10;
    var rows = 8;
    var tileWidth = plotArea.Width / cols;
    var tileHeight = plotArea.Height / rows;

    // Use series data to color tiles
    var pointIndex = 0;
    var points = series.Count > 0 ? series[0].Points : null;
    var maxY = points?.Max(p => p.Y) ?? 1;
    if (maxY == 0)
      maxY = 1;

    for (var row = 0; row < rows; ++row) {
      for (var col = 0; col < cols; ++col) {
        var x = plotArea.Left + col * tileWidth;
        var y = plotArea.Top + row * tileHeight;
        var rect = new RectangleF(x + 1, y + 1, tileWidth - 2, tileHeight - 2);

        Color tileColor;
        if (points != null && pointIndex < points.Count) {
          var intensity = (float)(points[pointIndex].Y / maxY);
          tileColor = Color.FromArgb(
            (int)(100 + intensity * 155),
            (int)(150 - intensity * 100),
            (int)(200 - intensity * 150)
          );
          ++pointIndex;
        } else
          tileColor = Color.LightGray;

        using var brush = new SolidBrush(tileColor);
        g.FillRectangle(brush, rect);
        using var pen = new Pen(Color.White, 1);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
      }
    }
  }
}

/// <summary>Renders a bubble map (bubbles placed on geographic locations).</summary>
public class BubbleMapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.BubbleMap;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var bubbleData = context.Chart.BubbleData;

    // Draw map background
    using var bgBrush = new SolidBrush(Color.FromArgb(230, 230, 230));
    g.FillRectangle(bgBrush, plotArea);

    if (bubbleData == null || bubbleData.Count == 0) {
      // Use regular series data if no bubble data
      var series = context.Series;
      if (series.Count > 0 && series[0].Points.Count > 0) {
        var maxY = series[0].Points.Max(p => p.Y);
        if (maxY == 0)
          maxY = 1;

        foreach (var dp in series[0].Points) {
          var x = (float)(plotArea.Left + (dp.X - context.XMin) / (context.XMax - context.XMin) * plotArea.Width);
          var y = (float)(plotArea.Bottom - (dp.Y - context.YMin) / (context.YMax - context.YMin) * plotArea.Height);
          var radius = (float)(10 + dp.Y / maxY * 30);

          using var brush = new SolidBrush(Color.FromArgb(150, series[0].Color));
          g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
          using var pen = new Pen(series[0].Color, 1);
          g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
        }
      }
      return;
    }

    var maxSize = bubbleData.Max(b => b.Size);
    if (maxSize == 0)
      maxSize = 1;

    foreach (var bubble in bubbleData) {
      var x = (float)(plotArea.Left + bubble.X / 100 * plotArea.Width);
      var y = (float)(plotArea.Top + (100 - bubble.Y) / 100 * plotArea.Height);
      var radius = (float)(10 + bubble.Size / maxSize * 40);
      var color = bubble.Color ?? Color.DodgerBlue;

      using var brush = new SolidBrush(Color.FromArgb(150, color));
      g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
      using var pen = new Pen(color, 1);
      g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
    }
  }
}

/// <summary>Renders a connection map (lines connecting geographic points).</summary>
public class ConnectionMapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.ConnectionMap;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var networkData = context.Chart.NetworkData;

    // Draw map background
    using var bgBrush = new SolidBrush(Color.FromArgb(20, 30, 48));
    g.FillRectangle(bgBrush, plotArea);

    if (networkData == null) {
      using var textBrush = new SolidBrush(Color.White);
      var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
      g.DrawString("Connection Map\n(Requires network data)", SystemFonts.DefaultFont, textBrush, plotArea, format);
      return;
    }

    // Draw connections first
    foreach (var edge in networkData.Edges) {
      var sourceNode = networkData.Nodes.FirstOrDefault(n => n.Id == edge.Source);
      var targetNode = networkData.Nodes.FirstOrDefault(n => n.Id == edge.Target);

      if (sourceNode != null && targetNode != null) {
        var x1 = plotArea.Left + sourceNode.Position.X / 100 * plotArea.Width;
        var y1 = plotArea.Top + (100 - sourceNode.Position.Y) / 100 * plotArea.Height;
        var x2 = plotArea.Left + targetNode.Position.X / 100 * plotArea.Width;
        var y2 = plotArea.Top + (100 - targetNode.Position.Y) / 100 * plotArea.Height;

        var lineColor = edge.Color ?? Color.FromArgb(100, Color.Cyan);
        using var pen = new Pen(lineColor, (float)(1 + edge.Weight * 2));
        g.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
      }
    }

    // Draw nodes
    foreach (var node in networkData.Nodes) {
      var x = (float)(plotArea.Left + node.Position.X / 100 * plotArea.Width);
      var y = (float)(plotArea.Top + (100 - node.Position.Y) / 100 * plotArea.Height);
      var radius = (float)(4 + node.Size * 4);
      var color = node.Color ?? Color.White;

      using var brush = new SolidBrush(color);
      g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
    }
  }
}

/// <summary>Renders a dot map (dots placed at geographic coordinates).</summary>
public class DotMapRenderer : ChartRenderer {
  public override AdvancedChartType ChartType => AdvancedChartType.DotMap;
  public override bool UsesAxes => false;

  public override void Render(ChartRenderContext context) {
    var g = context.Graphics;
    var plotArea = context.PlotArea;
    var series = context.Series;

    // Draw map background
    using var bgBrush = new SolidBrush(Color.FromArgb(245, 245, 245));
    g.FillRectangle(bgBrush, plotArea);

    if (series.Count == 0 || series[0].Points.Count == 0)
      return;

    var points = series[0].Points;
    var dotSize = 4f;

    foreach (var dp in points) {
      // Treat X and Y as normalized coordinates (0-100)
      var x = (float)(plotArea.Left + dp.X / 100 * plotArea.Width);
      var y = (float)(plotArea.Top + (100 - dp.Y) / 100 * plotArea.Height);
      var color = dp.Color ?? series[0].Color;

      using var brush = new SolidBrush(color);
      g.FillEllipse(brush, x - dotSize / 2, y - dotSize / 2, dotSize, dotSize);
    }
  }
}
