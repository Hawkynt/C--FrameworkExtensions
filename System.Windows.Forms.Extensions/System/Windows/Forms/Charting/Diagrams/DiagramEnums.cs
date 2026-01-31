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

namespace System.Windows.Forms.Charting.Diagrams;

/// <summary>
/// Specifies the type of diagram to render.
/// </summary>
public enum DiagramType {
  /// <summary>Sankey diagram showing flow quantities between nodes.</summary>
  Sankey,
  /// <summary>Chord diagram showing relationships in a circular layout.</summary>
  Chord,
  /// <summary>Arc diagram showing connections as arcs above a linear axis.</summary>
  Arc,
  /// <summary>Network graph with force-directed or circular layout.</summary>
  Network,
  /// <summary>Hierarchical tree diagram.</summary>
  Tree,
  /// <summary>Dendrogram with rectangular connections.</summary>
  Dendrogram,
  /// <summary>Circle packing visualization for hierarchical data.</summary>
  CirclePacking,
  /// <summary>Flow chart with process shapes and connectors.</summary>
  FlowChart
}

/// <summary>
/// Specifies the direction for flow diagrams.
/// </summary>
public enum DiagramFlowDirection {
  LeftToRight,
  RightToLeft,
  TopToBottom,
  BottomToTop
}

/// <summary>
/// Tree diagram orientation.
/// </summary>
public enum TreeOrientation {
  TopDown,
  BottomUp,
  LeftRight,
  RightLeft
}

/// <summary>
/// Specifies the position of the diagram legend.
/// </summary>
public enum DiagramLegendPosition {
  None,
  Top,
  Bottom,
  Left,
  Right,
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight,
  Floating
}

/// <summary>
/// Specifies the selection mode for diagram elements.
/// </summary>
public enum DiagramSelectionMode {
  None,
  Single,
  Multiple
}

/// <summary>
/// Specifies the animation style for diagram transitions.
/// </summary>
public enum DiagramAnimationStyle {
  None,
  Fade,
  Grow,
  Slide
}

/// <summary>
/// Specifies the tooltip trigger mode for diagrams.
/// </summary>
public enum DiagramTooltipTrigger {
  None,
  Hover,
  Click,
  Both
}

/// <summary>
/// Specifies the color palette for diagrams.
/// </summary>
public enum DiagramColorPalette {
  Default,
  Pastel,
  Bright,
  Dark,
  Monochrome,
  Category10,
  Category20,
  Tableau10,
  Material,
  Custom
}

/// <summary>
/// Specifies the shape for diagram nodes.
/// </summary>
public enum DiagramNodeShape {
  Rectangle,
  RoundedRectangle,
  Circle,
  Ellipse,
  Diamond,
  Triangle,
  Hexagon,
  Custom
}
