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

namespace System.Windows.Forms.Charting.Diagrams;

/// <summary>
/// Abstract base class for diagram renderers.
/// </summary>
public abstract class DiagramRenderer {
  /// <summary>Gets the diagram type this renderer handles.</summary>
  public abstract DiagramType DiagramType { get; }

  /// <summary>
  /// Renders the diagram to the specified graphics context.
  /// </summary>
  /// <param name="context">The rendering context.</param>
  public abstract void Render(DiagramRenderContext context);

  /// <summary>
  /// Gets legend items for this diagram.
  /// </summary>
  public virtual IList<DiagramLegendItem> GetLegendItems(DiagramControl diagram) => new List<DiagramLegendItem>();

  /// <summary>
  /// Performs hit testing for diagram elements.
  /// </summary>
  public virtual DiagramHitTestResult HitTest(DiagramRenderContext context, PointF point) => null;

  #region Helper Methods

  // Maximum pixel coordinate to prevent overflow in GDI+ operations
  private const float MaxPixelCoordinate = 1e6f;
  private const float MinPixelCoordinate = -1e6f;

  /// <summary>
  /// Clamps a pixel coordinate to a safe range to prevent GDI+ overflow.
  /// </summary>
  protected static float ClampPixel(float value) {
    if (float.IsNaN(value) || float.IsInfinity(value))
      return 0;
    return Math.Max(MinPixelCoordinate, Math.Min(MaxPixelCoordinate, value));
  }

  /// <summary>
  /// Draws a regular polygon.
  /// </summary>
  protected static void DrawPolygon(Graphics g, Brush brush, PointF center, float radius, int sides) {
    var points = new PointF[sides];
    var angleStep = Math.PI * 2 / sides;
    var startAngle = -Math.PI / 2;

    for (var i = 0; i < sides; ++i) {
      var angle = startAngle + i * angleStep;
      points[i] = new PointF(
        center.X + (float)(radius * Math.Cos(angle)),
        center.Y + (float)(radius * Math.Sin(angle))
      );
    }

    g.FillPolygon(brush, points);
  }

  /// <summary>
  /// Draws a star shape.
  /// </summary>
  protected static void DrawStar(Graphics g, Brush brush, PointF center, float radius, int points) {
    var vertices = new PointF[points * 2];
    var angleStep = Math.PI / points;
    var innerRadius = radius * 0.4f;
    var startAngle = -Math.PI / 2;

    for (var i = 0; i < points * 2; ++i) {
      var angle = startAngle + i * angleStep;
      var r = i % 2 == 0 ? radius : innerRadius;
      vertices[i] = new PointF(
        center.X + (float)(r * Math.Cos(angle)),
        center.Y + (float)(r * Math.Sin(angle))
      );
    }

    g.FillPolygon(brush, vertices);
  }

  /// <summary>
  /// Creates a color with modified alpha.
  /// </summary>
  protected static Color WithAlpha(Color color, int alpha)
    => Color.FromArgb(alpha, color);

  /// <summary>
  /// Lightens a color.
  /// </summary>
  protected static Color Lighten(Color color, float factor = 0.3f) {
    var r = (int)(color.R + (255 - color.R) * factor);
    var g = (int)(color.G + (255 - color.G) * factor);
    var b = (int)(color.B + (255 - color.B) * factor);
    return Color.FromArgb(color.A, Math.Min(255, r), Math.Min(255, g), Math.Min(255, b));
  }

  /// <summary>
  /// Darkens a color.
  /// </summary>
  protected static Color Darken(Color color, float factor = 0.3f) {
    var r = (int)(color.R * (1 - factor));
    var g = (int)(color.G * (1 - factor));
    var b = (int)(color.B * (1 - factor));
    return Color.FromArgb(color.A, Math.Max(0, r), Math.Max(0, g), Math.Max(0, b));
  }

  /// <summary>
  /// Draws a rounded rectangle.
  /// </summary>
  protected static void DrawRoundedRectangle(Graphics g, Pen pen, RectangleF rect, float radius) {
    using var path = CreateRoundedRectanglePath(rect, radius);
    g.DrawPath(pen, path);
  }

  /// <summary>
  /// Fills a rounded rectangle.
  /// </summary>
  protected static void FillRoundedRectangle(Graphics g, Brush brush, RectangleF rect, float radius) {
    using var path = CreateRoundedRectanglePath(rect, radius);
    g.FillPath(brush, path);
  }

  /// <summary>
  /// Creates a rounded rectangle path.
  /// </summary>
  protected static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius) {
    var path = new GraphicsPath();

    // Check for minimum valid dimensions to prevent GDI+ exceptions
    if (rect.Width < 1 || rect.Height < 1)
      return path;

    var diameter = radius * 2;

    if (diameter > rect.Width)
      diameter = rect.Width;
    if (diameter > rect.Height)
      diameter = rect.Height;

    // Ensure diameter is at least 1 to prevent AddArc exceptions
    if (diameter < 1)
      diameter = 1;

    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();

    return path;
  }

  /// <summary>
  /// Interpolates between two colors.
  /// </summary>
  protected static Color InterpolateColor(Color color1, Color color2, double t) {
    t = Math.Max(0, Math.Min(1, t));
    return Color.FromArgb(
      (int)(color1.A + (color2.A - color1.A) * t),
      (int)(color1.R + (color2.R - color1.R) * t),
      (int)(color1.G + (color2.G - color1.G) * t),
      (int)(color1.B + (color2.B - color1.B) * t)
    );
  }

  /// <summary>
  /// Gets a color from a gradient scale.
  /// </summary>
  protected static Color GetGradientColor(double value, double min, double max, Color lowColor, Color highColor) {
    if (Math.Abs(max - min) < double.Epsilon)
      return lowColor;

    var t = (value - min) / (max - min);
    return InterpolateColor(lowColor, highColor, t);
  }

  /// <summary>
  /// Gets the contrast color (black or white) for the specified background color.
  /// </summary>
  protected static Color GetContrastColor(Color color) {
    var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
    return luminance > 0.5 ? Color.Black : Color.White;
  }

  /// <summary>
  /// Gets a default color palette for diagrams.
  /// </summary>
  protected static Color[] GetDefaultColors() => new[] {
    Color.FromArgb(52, 152, 219),
    Color.FromArgb(231, 76, 60),
    Color.FromArgb(46, 204, 113),
    Color.FromArgb(155, 89, 182),
    Color.FromArgb(241, 196, 15),
    Color.FromArgb(230, 126, 34),
    Color.FromArgb(26, 188, 156),
    Color.FromArgb(52, 73, 94)
  };

  #endregion

  #region Layout Utilities

  /// <summary>
  /// Calculates a non-overlapping grid layout for items of uniform size.
  /// </summary>
  /// <param name="itemCount">Number of items to lay out.</param>
  /// <param name="itemWidth">Width of each item.</param>
  /// <param name="itemHeight">Height of each item.</param>
  /// <param name="margin">Margin between items.</param>
  /// <param name="plotArea">Available area for layout.</param>
  /// <returns>Dictionary mapping item index to its rectangle.</returns>
  protected static Dictionary<int, RectangleF> CalculateGridLayout(
    int itemCount, float itemWidth, float itemHeight, float margin, RectangleF plotArea) {
    var positions = new Dictionary<int, RectangleF>();
    if (itemCount == 0)
      return positions;

    // Calculate cell size including margin
    var cellWidth = itemWidth + margin;
    var cellHeight = itemHeight + margin;

    // Calculate optimal grid dimensions
    var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(itemCount * (cellWidth / cellHeight))));
    var rows = (int)Math.Ceiling((float)itemCount / cols);

    // Calculate total content size
    var contentWidth = cols * cellWidth;
    var contentHeight = rows * cellHeight;

    // Calculate starting position (centered if fits, otherwise start at origin)
    var startX = plotArea.Left + Math.Max(0, (plotArea.Width - contentWidth) / 2) + margin / 2;
    var startY = plotArea.Top + Math.Max(0, (plotArea.Height - contentHeight) / 2) + margin / 2;

    for (var i = 0; i < itemCount; ++i) {
      var col = i % cols;
      var row = i / cols;
      var x = startX + col * cellWidth;
      var y = startY + row * cellHeight;
      positions[i] = new RectangleF(x, y, itemWidth, itemHeight);
    }

    return positions;
  }

  /// <summary>
  /// Calculates the content bounds from a collection of rectangles and reports them for auto-zoom.
  /// </summary>
  protected static RectangleF CalculateContentBounds(IEnumerable<RectangleF> rectangles, float padding = 20f) {
    var minX = float.MaxValue;
    var minY = float.MaxValue;
    var maxX = float.MinValue;
    var maxY = float.MinValue;
    var hasAny = false;

    foreach (var rect in rectangles) {
      hasAny = true;
      minX = Math.Min(minX, rect.Left);
      minY = Math.Min(minY, rect.Top);
      maxX = Math.Max(maxX, rect.Right);
      maxY = Math.Max(maxY, rect.Bottom);
    }

    if (!hasAny)
      return RectangleF.Empty;

    return new RectangleF(
      minX - padding,
      minY - padding,
      maxX - minX + padding * 2,
      maxY - minY + padding * 2
    );
  }

  /// <summary>
  /// Calculates the zoom level needed to fit content in the viewport.
  /// </summary>
  protected static float CalculateAutoZoom(RectangleF contentBounds, RectangleF viewport) {
    if (contentBounds.Width <= 0 || contentBounds.Height <= 0)
      return 1f;

    var scaleX = viewport.Width / contentBounds.Width;
    var scaleY = viewport.Height / contentBounds.Height;
    var scale = Math.Min(scaleX, scaleY);

    // Clamp to reasonable zoom range
    return Math.Max(0.1f, Math.Min(2f, scale));
  }

  /// <summary>
  /// Adjusts positions to fit within the plot area, scaling both in and out as needed.
  /// Returns the scale factor applied.
  /// </summary>
  protected static float AdjustLayoutToFit(
    Dictionary<string, RectangleF> positions,
    RectangleF plotArea,
    float padding = 20f) {
    if (positions.Count == 0)
      return 1f;

    var contentBounds = CalculateContentBounds(positions.Values, 0);

    // Calculate scale needed to fit (scale both in and out)
    var availableWidth = plotArea.Width - padding * 2;
    var availableHeight = plotArea.Height - padding * 2;

    var scaleX = contentBounds.Width > 0 ? availableWidth / contentBounds.Width : 1f;
    var scaleY = contentBounds.Height > 0 ? availableHeight / contentBounds.Height : 1f;
    var scale = Math.Min(scaleX, scaleY); // Scale both in and out

    // Center the scaled content
    var offsetX = plotArea.Left + padding + (availableWidth - contentBounds.Width * scale) / 2 - contentBounds.Left * scale;
    var offsetY = plotArea.Top + padding + (availableHeight - contentBounds.Height * scale) / 2 - contentBounds.Top * scale;

    // Apply transformation to all positions
    var keys = new List<string>(positions.Keys);
    foreach (var key in keys) {
      var rect = positions[key];
      positions[key] = new RectangleF(
        rect.X * scale + offsetX,
        rect.Y * scale + offsetY,
        rect.Width * scale,
        rect.Height * scale
      );
    }

    return scale;
  }

  /// <summary>
  /// Represents a virtual canvas for layout calculations in relative coordinates.
  /// Nodes are laid out in virtual space and then transformed to fit the viewport.
  /// </summary>
  protected class VirtualCanvas {
    private readonly Dictionary<string, RectangleF> _nodeRects = new();
    private readonly float _cellWidth;
    private readonly float _cellHeight;
    private readonly float _margin;

    /// <summary>Gets the uniform cell width including margin.</summary>
    public float CellWidth => this._cellWidth;

    /// <summary>Gets the uniform cell height including margin.</summary>
    public float CellHeight => this._cellHeight;

    /// <summary>
    /// Creates a virtual canvas with uniform cell sizes based on measured node sizes.
    /// </summary>
    /// <param name="nodeSizes">Dictionary of node ID to measured size (including margin).</param>
    public VirtualCanvas(Dictionary<string, SizeF> nodeSizes, float margin = 10f) {
      this._margin = margin;
      if (nodeSizes.Count == 0) {
        this._cellWidth = 100;
        this._cellHeight = 50;
        return;
      }

      // Find max dimensions for uniform cells
      this._cellWidth = nodeSizes.Values.Max(s => s.Width) + margin;
      this._cellHeight = nodeSizes.Values.Max(s => s.Height) + margin;
    }

    /// <summary>
    /// Creates a virtual canvas with explicit cell sizes.
    /// </summary>
    public VirtualCanvas(float cellWidth, float cellHeight, float margin = 10f) {
      this._cellWidth = cellWidth + margin;
      this._cellHeight = cellHeight + margin;
      this._margin = margin;
    }

    /// <summary>
    /// Places a node at the specified grid position (col, row).
    /// </summary>
    public void PlaceAtGrid(string nodeId, int col, int row) {
      var x = col * this._cellWidth;
      var y = row * this._cellHeight;
      this._nodeRects[nodeId] = new RectangleF(x, y, this._cellWidth - this._margin, this._cellHeight - this._margin);
    }

    /// <summary>
    /// Places a node at an explicit virtual position.
    /// </summary>
    public void PlaceAt(string nodeId, float x, float y, float width, float height)
      => this._nodeRects[nodeId] = new RectangleF(x, y, width, height);

    /// <summary>
    /// Gets all node rectangles in virtual coordinates.
    /// </summary>
    public Dictionary<string, RectangleF> GetVirtualPositions() => new(this._nodeRects);

    /// <summary>
    /// Calculates a grid layout for the specified nodes, optimizing for the aspect ratio.
    /// </summary>
    public void LayoutAsGrid(IList<string> nodeIds, float targetAspectRatio = 1.5f) {
      if (nodeIds.Count == 0)
        return;

      // Calculate optimal columns based on aspect ratio
      var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(nodeIds.Count * targetAspectRatio)));

      for (var i = 0; i < nodeIds.Count; ++i) {
        var col = i % cols;
        var row = i / cols;
        this.PlaceAtGrid(nodeIds[i], col, row);
      }
    }

    /// <summary>
    /// Transforms all virtual positions to fit within the viewport, scaling both in and out.
    /// Returns the center positions for each node.
    /// </summary>
    public Dictionary<string, PointF> TransformToViewport(RectangleF viewport, float padding = 20f) {
      var positions = new Dictionary<string, RectangleF>(this._nodeRects);
      AdjustRectangleLayoutToFit(positions, viewport, padding);

      // Return center points
      var result = new Dictionary<string, PointF>();
      foreach (var kvp in positions)
        result[kvp.Key] = new PointF(kvp.Value.X + kvp.Value.Width / 2, kvp.Value.Y + kvp.Value.Height / 2);
      return result;
    }

    /// <summary>
    /// Transforms all virtual positions to fit within the viewport, returning full rectangles.
    /// </summary>
    public Dictionary<string, RectangleF> TransformToViewportRects(RectangleF viewport, float padding = 20f) {
      var positions = new Dictionary<string, RectangleF>(this._nodeRects);
      AdjustRectangleLayoutToFit(positions, viewport, padding);
      return positions;
    }
  }

  /// <summary>
  /// Measures node sizes (icon + label) for all nodes, returning sizes including margin.
  /// </summary>
  protected static Dictionary<string, SizeF> MeasureNodeSizes(
    Graphics g, Font font, IEnumerable<(string Id, string Label, float IconSize)> nodes, float margin = 15f) {
    var sizes = new Dictionary<string, SizeF>();
    foreach (var (id, label, iconSize) in nodes) {
      var labelSize = g.MeasureString(label ?? id, font);
      var width = Math.Max(iconSize, labelSize.Width) + margin;
      var height = iconSize + labelSize.Height + margin;
      sizes[id] = new SizeF(width, height);
    }
    return sizes;
  }

  /// <summary>
  /// Calculates a hierarchical layout for tree structures.
  /// </summary>
  protected static Dictionary<string, PointF> CalculateHierarchicalLayout<T>(
    IList<T> roots,
    Func<T, string> getId,
    Func<T, IEnumerable<T>> getChildren,
    float nodeWidth, float nodeHeight, float horizontalSpacing, float verticalSpacing,
    RectangleF plotArea) {
    var positions = new Dictionary<string, PointF>();
    if (roots == null || roots.Count == 0)
      return positions;

    // Calculate subtree widths
    var subtreeWidths = new Dictionary<string, float>();
    foreach (var root in roots)
      _CalculateSubtreeWidth(root, getId, getChildren, nodeWidth, horizontalSpacing, subtreeWidths);

    // Position nodes
    var currentX = plotArea.Left + horizontalSpacing;
    foreach (var root in roots) {
      _PositionHierarchyNode(root, getId, getChildren, subtreeWidths, positions,
        0, ref currentX, plotArea.Top + verticalSpacing, nodeWidth, nodeHeight, horizontalSpacing, verticalSpacing);
      currentX += horizontalSpacing;
    }

    return positions;
  }

  private static float _CalculateSubtreeWidth<T>(
    T node, Func<T, string> getId, Func<T, IEnumerable<T>> getChildren,
    float nodeWidth, float spacing, Dictionary<string, float> widths) {
    var id = getId(node);
    var children = getChildren(node)?.ToList() ?? new List<T>();

    if (children.Count == 0) {
      widths[id] = nodeWidth;
      return nodeWidth;
    }

    var totalWidth = 0f;
    foreach (var child in children) {
      if (totalWidth > 0)
        totalWidth += spacing;
      totalWidth += _CalculateSubtreeWidth(child, getId, getChildren, nodeWidth, spacing, widths);
    }

    widths[id] = Math.Max(nodeWidth, totalWidth);
    return widths[id];
  }

  private static void _PositionHierarchyNode<T>(
    T node, Func<T, string> getId, Func<T, IEnumerable<T>> getChildren,
    Dictionary<string, float> subtreeWidths, Dictionary<string, PointF> positions,
    int depth, ref float currentX, float startY,
    float nodeWidth, float nodeHeight, float horizontalSpacing, float verticalSpacing) {
    var id = getId(node);
    var subtreeWidth = subtreeWidths.GetValueOrDefault(id, nodeWidth);
    var children = getChildren(node)?.ToList() ?? new List<T>();

    if (children.Count == 0) {
      positions[id] = new PointF(currentX + nodeWidth / 2, startY + depth * (nodeHeight + verticalSpacing));
      currentX += nodeWidth;
    } else {
      var childStartX = currentX;
      foreach (var child in children) {
        _PositionHierarchyNode(child, getId, getChildren, subtreeWidths, positions,
          depth + 1, ref currentX, startY, nodeWidth, nodeHeight, horizontalSpacing, verticalSpacing);
        currentX += horizontalSpacing;
      }
      currentX -= horizontalSpacing;

      // Center parent above children
      var x = (childStartX + currentX + nodeWidth) / 2;
      positions[id] = new PointF(x, startY + depth * (nodeHeight + verticalSpacing));
    }
  }

  /// <summary>
  /// Simple edge crossing minimization by reordering nodes within layers.
  /// </summary>
  protected static void MinimizeEdgeCrossings<T>(
    IList<T> items,
    Func<T, string> getId,
    Func<T, IEnumerable<string>> getConnectedIds,
    Dictionary<string, PointF> positions) {
    // Group by Y position (layers)
    var layers = positions
      .GroupBy(kvp => (int)(kvp.Value.Y / 10) * 10)
      .OrderBy(g => g.Key)
      .Select(g => g.OrderBy(kvp => kvp.Value.X).Select(kvp => kvp.Key).ToList())
      .ToList();

    // For each pair of adjacent layers, minimize crossings using barycenter heuristic
    for (var i = 1; i < layers.Count; ++i) {
      var upperLayer = layers[i - 1];
      var lowerLayer = layers[i];

      // Calculate barycenter for each node in lower layer
      var barycenters = new Dictionary<string, float>();
      foreach (var nodeId in lowerLayer) {
        var item = items.FirstOrDefault(it => getId(it) == nodeId);
        if (item == null)
          continue;

        var connectedIds = getConnectedIds(item)?.ToList() ?? new List<string>();
        var connectedInUpper = connectedIds.Where(id => upperLayer.Contains(id)).ToList();

        if (connectedInUpper.Count > 0) {
          var sum = connectedInUpper.Sum(id => upperLayer.IndexOf(id));
          barycenters[nodeId] = sum / (float)connectedInUpper.Count;
        } else {
          barycenters[nodeId] = lowerLayer.IndexOf(nodeId);
        }
      }

      // Reorder by barycenter
      layers[i] = lowerLayer.OrderBy(id => barycenters.GetValueOrDefault(id, 0)).ToList();

      // Update positions
      var layerY = positions[layers[i][0]].Y;
      var spacing = layers[i].Count > 1
        ? (positions[layers[i].Last()].X - positions[layers[i].First()].X) / (layers[i].Count - 1)
        : 100f;
      var startX = positions.Values.Min(p => p.X);

      for (var j = 0; j < layers[i].Count; ++j) {
        var id = layers[i][j];
        positions[id] = new PointF(startX + j * spacing, layerY);
      }
    }
  }

  /// <summary>
  /// Minimizes edge crossings for grid layouts by reordering nodes.
  /// Uses barycenter heuristic for adjacent rows.
  /// </summary>
  protected static void MinimizeEdgeCrossingsForGrid<T>(
    IList<T> items,
    Func<T, string> getId,
    Func<T, IEnumerable<string>> getConnectedIds,
    Dictionary<string, RectangleF> positions,
    int iterations = 2) {
    if (positions.Count < 3)
      return;

    // Convert to center points
    var centerPositions = new Dictionary<string, PointF>();
    foreach (var kvp in positions)
      centerPositions[kvp.Key] = new PointF(
        kvp.Value.X + kvp.Value.Width / 2,
        kvp.Value.Y + kvp.Value.Height / 2);

    // Group by Y position (rows)
    var tolerance = positions.Values.First().Height / 2;
    var rows = centerPositions
      .GroupBy(kvp => Math.Round(kvp.Value.Y / tolerance) * tolerance)
      .OrderBy(g => g.Key)
      .Select(g => g.OrderBy(kvp => kvp.Value.X).Select(kvp => kvp.Key).ToList())
      .ToList();

    if (rows.Count < 2)
      return;

    // Run barycenter iterations
    for (var iter = 0; iter < iterations; ++iter) {
      // Forward pass
      for (var rowIdx = 1; rowIdx < rows.Count; ++rowIdx)
        _ReorderRowByBarycenter(rows, rowIdx, items, getId, getConnectedIds, centerPositions, true);

      // Backward pass
      for (var rowIdx = rows.Count - 2; rowIdx >= 0; --rowIdx)
        _ReorderRowByBarycenter(rows, rowIdx, items, getId, getConnectedIds, centerPositions, false);
    }

    // Apply new ordering to rectangles
    foreach (var row in rows) {
      var rowY = positions[row[0]].Y;
      var sortedRects = row.Select(id => positions[id]).OrderBy(r => r.X).ToList();
      for (var i = 0; i < row.Count; ++i) {
        var id = row[i];
        var oldRect = positions[id];
        positions[id] = new RectangleF(sortedRects[i].X, rowY, oldRect.Width, oldRect.Height);
      }
    }
  }

  private static void _ReorderRowByBarycenter<T>(
    List<List<string>> rows, int rowIdx,
    IList<T> items, Func<T, string> getId, Func<T, IEnumerable<string>> getConnectedIds,
    Dictionary<string, PointF> positions, bool useUpperRow) {
    var currentRow = rows[rowIdx];
    var adjacentRow = useUpperRow ? rows[rowIdx - 1] : rows[rowIdx + 1];

    var barycenters = new Dictionary<string, float>();
    foreach (var nodeId in currentRow) {
      var item = items.FirstOrDefault(it => getId(it) == nodeId);
      if (item == null) {
        barycenters[nodeId] = currentRow.IndexOf(nodeId);
        continue;
      }

      var connectedIds = getConnectedIds(item)?.ToList() ?? new List<string>();
      var connectedInAdjacent = connectedIds.Where(id => adjacentRow.Contains(id)).ToList();

      if (connectedInAdjacent.Count > 0) {
        var sum = connectedInAdjacent.Sum(id => adjacentRow.IndexOf(id));
        barycenters[nodeId] = sum / (float)connectedInAdjacent.Count;
      } else {
        barycenters[nodeId] = currentRow.IndexOf(nodeId);
      }
    }

    // Reorder by barycenter
    var newOrder = currentRow.OrderBy(id => barycenters.GetValueOrDefault(id, 0)).ToList();
    rows[rowIdx] = newOrder;

    // Update positions to reflect new order
    var rowY = positions[currentRow[0]].Y;
    var sortedX = currentRow.Select(id => positions[id].X).OrderBy(x => x).ToList();
    for (var i = 0; i < newOrder.Count; ++i)
      positions[newOrder[i]] = new PointF(sortedX[i], rowY);
  }

  /// <summary>
  /// Adjusts a rectangle-based layout to fit within the plot area, scaling and centering as needed.
  /// Returns the scale factor applied (1.0 if no scaling needed).
  /// </summary>
  protected static float AdjustRectangleLayoutToFit(
    Dictionary<string, RectangleF> positions,
    RectangleF plotArea,
    float padding = 20f) {
    if (positions.Count == 0)
      return 1f;

    // Calculate content bounds
    var minX = float.MaxValue;
    var minY = float.MaxValue;
    var maxX = float.MinValue;
    var maxY = float.MinValue;

    foreach (var rect in positions.Values) {
      minX = Math.Min(minX, rect.Left);
      minY = Math.Min(minY, rect.Top);
      maxX = Math.Max(maxX, rect.Right);
      maxY = Math.Max(maxY, rect.Bottom);
    }

    var contentWidth = maxX - minX;
    var contentHeight = maxY - minY;

    if (contentWidth <= 0 || contentHeight <= 0)
      return 1f;

    // Calculate scale needed to fit (scale both in and out to fill viewport)
    var availableWidth = plotArea.Width - padding * 2;
    var availableHeight = plotArea.Height - padding * 2;

    var scaleX = availableWidth / contentWidth;
    var scaleY = availableHeight / contentHeight;
    var scale = Math.Min(scaleX, scaleY); // Uniform scaling to fit

    // Calculate offset to center content
    var scaledWidth = contentWidth * scale;
    var scaledHeight = contentHeight * scale;
    var offsetX = plotArea.Left + padding + (availableWidth - scaledWidth) / 2 - minX * scale;
    var offsetY = plotArea.Top + padding + (availableHeight - scaledHeight) / 2 - minY * scale;

    // Apply transformation
    var keys = new List<string>(positions.Keys);
    foreach (var key in keys) {
      var rect = positions[key];
      positions[key] = new RectangleF(
        rect.X * scale + offsetX,
        rect.Y * scale + offsetY,
        rect.Width * scale,
        rect.Height * scale
      );
    }

    return scale;
  }

  #endregion
}

/// <summary>
/// Context for diagram rendering operations.
/// </summary>
public class DiagramRenderContext {
  /// <summary>Gets the graphics context.</summary>
  public Graphics Graphics { get; set; }

  /// <summary>Gets the diagram being rendered.</summary>
  public DiagramControl Diagram { get; set; }

  /// <summary>Gets the total bounds of the diagram control.</summary>
  public RectangleF TotalBounds { get; set; }

  /// <summary>Gets the plot area (where diagram is drawn).</summary>
  public RectangleF PlotArea { get; set; }

  /// <summary>Gets the hit test rectangles (populated during rendering).</summary>
  public Dictionary<object, RectangleF> HitTestRects { get; } = new();

  /// <summary>Gets or sets the highlighted node identifier.</summary>
  public string HighlightedNodeId { get; set; }

  /// <summary>Gets or sets the highlighted edge.</summary>
  public DiagramEdge HighlightedEdge { get; set; }

  /// <summary>Gets the animation progress (0-1).</summary>
  public double AnimationProgress { get; set; } = 1.0;

  /// <summary>
  /// Registers a hit test rectangle for a diagram element.
  /// </summary>
  public void RegisterHitTestRect(object element, RectangleF rect) => this.HitTestRects[element] = rect;
}

/// <summary>
/// Result of a diagram hit test.
/// </summary>
public class DiagramHitTestResult {
  /// <summary>Gets or sets the hit node.</summary>
  public DiagramNode Node { get; set; }

  /// <summary>Gets or sets the hit edge.</summary>
  public DiagramEdge Edge { get; set; }

  /// <summary>Gets or sets the hit Sankey link.</summary>
  public DiagramSankeyLink SankeyLink { get; set; }

  /// <summary>Gets or sets the hit hierarchy node.</summary>
  public DiagramHierarchyNode HierarchyNode { get; set; }

  /// <summary>Gets or sets the hit element type.</summary>
  public DiagramHitTestElement ElementType { get; set; }

  /// <summary>Gets or sets additional hit information.</summary>
  public object AdditionalInfo { get; set; }
}

/// <summary>
/// Specifies the type of diagram element hit.
/// </summary>
public enum DiagramHitTestElement {
  None,
  Node,
  Edge,
  Link,
  Legend,
  Title,
  PlotArea
}

/// <summary>
/// Represents an item in the diagram legend.
/// </summary>
public class DiagramLegendItem {
  /// <summary>Gets or sets the legend item text.</summary>
  public string Text { get; set; }

  /// <summary>Gets or sets the legend item color.</summary>
  public Color Color { get; set; }

  /// <summary>Gets or sets whether the item is visible.</summary>
  public bool Visible { get; set; } = true;

  /// <summary>Gets or sets custom tag data.</summary>
  public object Tag { get; set; }
}
