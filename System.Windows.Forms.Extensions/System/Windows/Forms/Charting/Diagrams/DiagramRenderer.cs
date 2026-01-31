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
