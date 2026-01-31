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

namespace System.Windows.Forms.Charting;

/// <summary>
/// Specifies the type of advanced chart to render.
/// </summary>
public enum AdvancedChartType {
  // Comparison Charts
  Bar,
  Column,
  GroupedBar,
  GroupedColumn,
  StackedBar,
  StackedColumn,
  StackedBar100,
  StackedColumn100,
  DivergingStackedBar,
  Lollipop,
  DotPlot,
  Dumbbell,
  Bullet,
  Radar,
  PolarArea,
  Nightingale,
  RangePlot,
  SmallMultiples,

  // Trend/Time Series Charts
  Line,
  MultiLine,
  Spline,
  Area,
  StackedArea,
  StackedArea100,
  Step,
  StepArea,
  StreamGraph,
  Sparkline,
  RangeArea,
  BumpArea,
  Barcode,

  // Part-to-Whole Charts
  Pie,
  Donut,
  SemiCircleDonut,
  NestedDonut,
  Treemap,
  CircularTreemap,
  ConvexTreemap,
  Sunburst,
  Waffle,
  Icicle,
  Mosaic,
  Marimekko,
  Parliament,
  Unit,

  // Distribution Charts
  Histogram,
  RadialHistogram,
  BoxPlot,
  Violin,
  Density,
  Beeswarm,
  StripPlot,
  JitterPlot,
  Ridgeline,
  Horizon,
  Cumulative,
  PopulationPyramid,
  OneDimensionalHeatmap,

  // Correlation Charts
  Scatter,
  CategoricalScatter,
  Bubble,
  ConnectedScatter,
  Heatmap,
  Correlogram,
  ScatterMatrix,
  Hexbin,
  Contour,
  QuadrantChart,
  MatrixChart,

  // Ranking Charts
  OrderedBar,
  Slope,
  Bump,
  ParallelCoordinates,
  RadialBar,
  TableHeatmap,
  TableChart,

  // Geospatial Charts
  Choropleth,
  GeographicHeatmap,
  TileMap,
  BubbleMap,
  ConnectionMap,
  DotMap,

  // Temporal Charts
  Timeline,
  Gantt,
  CalendarHeatmap,
  Seasonal,
  Spiral,

  // Financial Charts
  Candlestick,
  OHLC,
  Kagi,
  Renko,
  Waterfall,
  PointFigure,

  // Specialized Charts
  Funnel,
  Pyramid,
  Gauge,
  CircularGauge,
  WordCloud,
  Pictogram,
  Venn,
  EulerDiagram,
  IconArray
}

/// <summary>
/// Specifies the position of the chart legend.
/// </summary>
public enum ChartLegendPosition {
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
/// Specifies the orientation of a chart element.
/// </summary>
public enum ChartOrientation {
  Horizontal,
  Vertical
}

/// <summary>
/// Specifies the style of markers on data points.
/// </summary>
public enum AdvancedMarkerStyle {
  None,
  Circle,
  Square,
  Diamond,
  Triangle,
  InvertedTriangle,
  Cross,
  Plus,
  Star,
  Pentagon,
  Hexagon
}

/// <summary>
/// Specifies the line style for chart elements.
/// </summary>
public enum ChartLineStyle {
  Solid,
  Dash,
  Dot,
  DashDot,
  DashDotDot
}

/// <summary>
/// Specifies the selection mode for chart elements.
/// </summary>
public enum ChartSelectionMode {
  None,
  Single,
  Multiple,
  Series
}

/// <summary>
/// Specifies the axis type.
/// </summary>
public enum ChartAxisType {
  Primary,
  Secondary
}

/// <summary>
/// Specifies the scale type for an axis.
/// </summary>
public enum ChartScaleType {
  Linear,
  Logarithmic,
  DateTime,
  Category
}

/// <summary>
/// Specifies the position of axis labels.
/// </summary>
public enum ChartAxisLabelPosition {
  Outside,
  Inside,
  None
}

/// <summary>
/// Specifies the position of data labels.
/// </summary>
public enum ChartDataLabelPosition {
  None,
  Center,
  Inside,
  Outside,
  Top,
  Bottom,
  Left,
  Right,
  Auto
}

/// <summary>
/// Specifies the animation style for chart transitions.
/// </summary>
public enum ChartAnimationStyle {
  None,
  Fade,
  Grow,
  Slide,
  Bounce
}

/// <summary>
/// Specifies the tooltip trigger mode.
/// </summary>
public enum ChartTooltipTrigger {
  None,
  Hover,
  Click,
  Both
}

/// <summary>
/// Specifies the grid line style.
/// </summary>
public enum ChartGridLineStyle {
  None,
  Solid,
  Dashed,
  Dotted
}

/// <summary>
/// Specifies how stacked values are calculated.
/// </summary>
public enum ChartStackMode {
  Normal,
  Percentage
}

/// <summary>
/// Specifies the interpolation method for line charts.
/// </summary>
public enum ChartInterpolation {
  Linear,
  Spline,
  Step,
  StepBefore,
  StepAfter,
  Basis,
  Cardinal,
  Monotone
}

/// <summary>
/// Specifies the layout for hierarchical charts.
/// </summary>
public enum HierarchicalLayout {
  Squarify,
  Slice,
  Dice,
  SliceDice,
  Binary
}

/// <summary>
/// Specifies the direction for flow charts.
/// </summary>
public enum FlowDirection {
  LeftToRight,
  RightToLeft,
  TopToBottom,
  BottomToTop
}

/// <summary>
/// Specifies the shape for specialized charts.
/// </summary>
public enum ChartShape {
  Rectangle,
  RoundedRectangle,
  Circle,
  Ellipse,
  Diamond,
  Triangle,
  Custom
}

/// <summary>
/// Specifies the color palette for charts.
/// </summary>
public enum ChartColorPalette {
  Default,
  Pastel,
  Bright,
  Dark,
  Monochrome,
  Gradient,
  Category10,
  Category20,
  Tableau10,
  Material,
  Custom
}

/// <summary>
/// Specifies the export format for charts.
/// </summary>
public enum ChartExportFormat {
  Png,
  Jpeg,
  Gif,
  Bmp,
  Tiff,
  Svg,
  Pdf,
  Emf
}
