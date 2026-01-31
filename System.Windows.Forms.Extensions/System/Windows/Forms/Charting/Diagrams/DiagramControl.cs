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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace System.Windows.Forms.Charting.Diagrams;

/// <summary>
/// Control for displaying structural and relational diagrams (Sankey, Chord, Network, Tree, etc.).
/// </summary>
/// <example>
/// <code>
/// var diagram = new DiagramControl {
///   DiagramType = DiagramType.Sankey,
///   Title = "Energy Flow"
/// };
/// diagram.Nodes.Add(new DiagramNode("A", "Source A"));
/// diagram.Nodes.Add(new DiagramNode("B", "Target B"));
/// diagram.SankeyLinks.Add(new DiagramSankeyLink("A", "B", 100));
/// </code>
/// </example>
public class DiagramControl : Control {
  #region Fields

  private DiagramType _diagramType = DiagramType.Network;
  private DiagramRenderer _customRenderer;

  private string _title;
  private string _subtitle;
  private Font _titleFont;
  private Font _subtitleFont;
  private Color _titleColor = Color.Black;
  private Color _subtitleColor = Color.Gray;
  private DiagramColorPalette _colorPalette = DiagramColorPalette.Default;
  private Color[] _customColors;
  private bool _enableTooltips = true;
  private DiagramTooltipTrigger _tooltipTrigger = DiagramTooltipTrigger.Hover;
  private DiagramSelectionMode _selectionMode = DiagramSelectionMode.None;
  private bool _enableAnimation;
  private DiagramAnimationStyle _animationStyle = DiagramAnimationStyle.Grow;
  private int _animationDuration = 500;
  private DiagramLegendPosition _legendPosition = DiagramLegendPosition.Right;
  private bool _showLegend = true;

  private int _padding = 10;
  private Color _plotAreaBackground = Color.White;
  private Color _plotAreaBorderColor = Color.LightGray;
  private int _plotAreaBorderWidth = 1;

  private ToolTip _toolTip;
  private readonly Dictionary<object, RectangleF> _hitTestRects = new();
  private DiagramNode _hoveredNode;
  private DiagramEdge _hoveredEdge;
  private DiagramSankeyLink _hoveredLink;
  private int _highlightedLegendIndex = -1;
  private List<DiagramLegendItem> _legendItems = new();

  private RectangleF _plotArea;

  // Animation state
  private Timer _animationTimer;
  private double _animationProgress;
  private DateTime _animationStartTime;

  private static readonly Dictionary<DiagramType, DiagramRenderer> RendererCache = new();

  // Data collections
  private readonly DiagramNodeCollection _nodes;
  private readonly DiagramEdgeCollection _edges;
  private readonly DiagramSankeyLinkCollection _sankeyLinks;
  private readonly DiagramHierarchyNodeCollection _hierarchyNodes;

  #endregion

  #region Events

  /// <summary>Occurs when a node is clicked.</summary>
  [Category("Action")]
  [Description("Occurs when a node is clicked.")]
  public event EventHandler<DiagramNodeEventArgs> NodeClicked;

  /// <summary>Occurs when an edge is clicked.</summary>
  [Category("Action")]
  [Description("Occurs when an edge is clicked.")]
  public event EventHandler<DiagramEdgeEventArgs> EdgeClicked;

  /// <summary>Occurs when a node is hovered.</summary>
  [Category("Action")]
  [Description("Occurs when a node is hovered.")]
  public event EventHandler<DiagramNodeEventArgs> NodeHovered;

  /// <summary>Occurs when an edge is hovered.</summary>
  [Category("Action")]
  [Description("Occurs when an edge is hovered.")]
  public event EventHandler<DiagramEdgeEventArgs> EdgeHovered;

  /// <summary>Occurs when a Sankey link is clicked.</summary>
  [Category("Action")]
  [Description("Occurs when a Sankey link is clicked.")]
  public event EventHandler<DiagramLinkEventArgs> LinkClicked;

  #endregion

  #region Constructor

  /// <summary>
  /// Initializes a new instance of the <see cref="DiagramControl"/> class.
  /// </summary>
  public DiagramControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(500, 400);
    this._toolTip = new ToolTip();

    // Initialize data collections
    this._nodes = new DiagramNodeCollection(this);
    this._edges = new DiagramEdgeCollection(this);
    this._sankeyLinks = new DiagramSankeyLinkCollection(this);
    this._hierarchyNodes = new DiagramHierarchyNodeCollection(this);
  }

  #endregion

  #region Properties

  /// <summary>Gets or sets the diagram type.</summary>
  [Category("Appearance")]
  [Description("The type of diagram to display.")]
  [DefaultValue(DiagramType.Network)]
  public DiagramType DiagramType {
    get => this._diagramType;
    set {
      if (this._diagramType == value)
        return;
      this._diagramType = value;
      this._StartAnimation();
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets a custom renderer (overrides DiagramType).</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public DiagramRenderer CustomRenderer {
    get => this._customRenderer;
    set {
      this._customRenderer = value;
      this.Invalidate();
    }
  }

  #region Data Collections

  /// <summary>Gets the nodes collection.</summary>
  [Category("Data")]
  [Description("Network/graph nodes.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramNodeCollection Nodes => this._nodes;

  /// <summary>Gets the edges collection.</summary>
  [Category("Data")]
  [Description("Network/graph edges.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramEdgeCollection Edges => this._edges;

  /// <summary>Gets the Sankey links collection.</summary>
  [Category("Data")]
  [Description("Sankey diagram links.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramSankeyLinkCollection SankeyLinks => this._sankeyLinks;

  /// <summary>Gets the hierarchy nodes collection.</summary>
  [Category("Data")]
  [Description("Hierarchical nodes for tree diagrams.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramHierarchyNodeCollection HierarchyNodes => this._hierarchyNodes;

  /// <summary>Gets the combined network data (nodes and edges).</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public DiagramNetworkData NetworkData => new(this._nodes, this._edges);

  #endregion

  /// <summary>Gets or sets the diagram title.</summary>
  [Category("Appearance")]
  [Description("The title displayed at the top of the diagram.")]
  [DefaultValue(null)]
  public string Title {
    get => this._title;
    set {
      if (this._title == value)
        return;
      this._title = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the diagram subtitle.</summary>
  [Category("Appearance")]
  [Description("The subtitle displayed below the title.")]
  [DefaultValue(null)]
  public string Subtitle {
    get => this._subtitle;
    set {
      if (this._subtitle == value)
        return;
      this._subtitle = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the title font.</summary>
  [Category("Appearance")]
  [Description("The font for the diagram title.")]
  public Font TitleFont {
    get => this._titleFont;
    set {
      this._titleFont = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the subtitle font.</summary>
  [Category("Appearance")]
  [Description("The font for the diagram subtitle.")]
  public Font SubtitleFont {
    get => this._subtitleFont;
    set {
      this._subtitleFont = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the title color.</summary>
  [Category("Appearance")]
  [Description("The color of the diagram title.")]
  public Color TitleColor {
    get => this._titleColor;
    set {
      this._titleColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the subtitle color.</summary>
  [Category("Appearance")]
  [Description("The color of the diagram subtitle.")]
  public Color SubtitleColor {
    get => this._subtitleColor;
    set {
      this._subtitleColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the color palette.</summary>
  [Category("Appearance")]
  [Description("The color palette for diagram elements.")]
  [DefaultValue(DiagramColorPalette.Default)]
  public DiagramColorPalette ColorPalette {
    get => this._colorPalette;
    set {
      this._colorPalette = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets custom colors (when ColorPalette is Custom).</summary>
  [Category("Appearance")]
  [Description("Custom colors for diagram elements.")]
  public Color[] CustomColors {
    get => this._customColors;
    set {
      this._customColors = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to show the legend.</summary>
  [Category("Legend")]
  [Description("Whether to show the legend.")]
  [DefaultValue(true)]
  public bool ShowLegend {
    get => this._showLegend;
    set {
      this._showLegend = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the legend position.</summary>
  [Category("Legend")]
  [Description("The position of the legend.")]
  [DefaultValue(DiagramLegendPosition.Right)]
  public DiagramLegendPosition LegendPosition {
    get => this._legendPosition;
    set {
      this._legendPosition = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to enable tooltips.</summary>
  [Category("Behavior")]
  [Description("Whether to show tooltips on hover.")]
  [DefaultValue(true)]
  public bool EnableTooltips {
    get => this._enableTooltips;
    set => this._enableTooltips = value;
  }

  /// <summary>Gets or sets the tooltip trigger mode.</summary>
  [Category("Behavior")]
  [Description("How tooltips are triggered.")]
  [DefaultValue(DiagramTooltipTrigger.Hover)]
  public DiagramTooltipTrigger TooltipTrigger {
    get => this._tooltipTrigger;
    set => this._tooltipTrigger = value;
  }

  /// <summary>Gets or sets the selection mode.</summary>
  [Category("Behavior")]
  [Description("The selection mode for diagram elements.")]
  [DefaultValue(DiagramSelectionMode.None)]
  public DiagramSelectionMode SelectionMode {
    get => this._selectionMode;
    set => this._selectionMode = value;
  }

  /// <summary>Gets or sets whether to enable animation.</summary>
  [Category("Behavior")]
  [Description("Whether to animate diagram transitions.")]
  [DefaultValue(false)]
  public bool EnableAnimation {
    get => this._enableAnimation;
    set {
      this._enableAnimation = value;
      if (value)
        this._animationProgress = 0;
    }
  }

  /// <summary>Gets or sets the animation style.</summary>
  [Category("Behavior")]
  [Description("The style of animation.")]
  [DefaultValue(DiagramAnimationStyle.Grow)]
  public DiagramAnimationStyle AnimationStyle {
    get => this._animationStyle;
    set => this._animationStyle = value;
  }

  /// <summary>Gets or sets the animation duration in milliseconds.</summary>
  [Category("Behavior")]
  [Description("The duration of animations in milliseconds.")]
  [DefaultValue(500)]
  public int AnimationDuration {
    get => this._animationDuration;
    set => this._animationDuration = Math.Max(0, value);
  }

  /// <summary>Gets or sets the padding around the diagram.</summary>
  [Category("Layout")]
  [Description("The padding around the diagram in pixels.")]
  [DefaultValue(10)]
  public int DiagramPadding {
    get => this._padding;
    set {
      this._padding = Math.Max(0, value);
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area background color.</summary>
  [Category("Appearance")]
  [Description("The background color of the plot area.")]
  public Color PlotAreaBackground {
    get => this._plotAreaBackground;
    set {
      this._plotAreaBackground = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area border color.</summary>
  [Category("Appearance")]
  [Description("The border color of the plot area.")]
  public Color PlotAreaBorderColor {
    get => this._plotAreaBorderColor;
    set {
      this._plotAreaBorderColor = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the plot area border width.</summary>
  [Category("Appearance")]
  [Description("The border width of the plot area.")]
  [DefaultValue(1)]
  public int PlotAreaBorderWidth {
    get => this._plotAreaBorderWidth;
    set {
      this._plotAreaBorderWidth = Math.Max(0, value);
      this.Invalidate();
    }
  }

  #endregion

  #region Public Methods

  /// <summary>
  /// Clears all diagram data.
  /// </summary>
  public void Clear() {
    this._nodes.Clear();
    this._edges.Clear();
    this._sankeyLinks.Clear();
    this._hierarchyNodes.Clear();
    this.Invalidate();
  }

  /// <summary>
  /// Triggers diagram animation.
  /// </summary>
  public void TriggerAnimation() {
    if (!this.IsHandleCreated) {
      this.HandleCreated += this._OnHandleCreatedForAnimation;
      return;
    }
    this._StartAnimation();
    this.Invalidate();
    this.Update();
  }

  private void _OnHandleCreatedForAnimation(object sender, EventArgs e) {
    this.HandleCreated -= this._OnHandleCreatedForAnimation;
    this._StartAnimation();
    this.Invalidate();
    this.Update();
  }

  /// <summary>
  /// Refreshes the diagram display.
  /// </summary>
  public new void Refresh() {
    base.Refresh();
    if (this._enableAnimation)
      this._StartAnimation();
    this.Invalidate();
  }

  /// <summary>
  /// Exports the diagram to an image.
  /// </summary>
  public Bitmap ToImage(int width = 0, int height = 0) {
    if (width <= 0)
      width = this.Width;
    if (height <= 0)
      height = this.Height;

    var bmp = new Bitmap(width, height);
    using (var g = Graphics.FromImage(bmp)) {
      g.Clear(this.BackColor);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

      var context = this._CreateRenderContext(g, new RectangleF(0, 0, width, height));
      this._RenderDiagram(context);
    }
    return bmp;
  }

  /// <summary>
  /// Saves the diagram as an image file.
  /// </summary>
  public void SaveAsImage(string path, ImageFormat format = null) {
    using var bmp = this.ToImage();
    bmp.Save(path, format ?? ImageFormat.Png);
  }

  /// <summary>
  /// Registers a custom renderer for a diagram type.
  /// </summary>
  public static void RegisterRenderer(DiagramType diagramType, DiagramRenderer renderer)
    => RendererCache[diagramType] = renderer;

  /// <summary>
  /// Gets the renderer for a diagram type.
  /// </summary>
  public static DiagramRenderer GetRenderer(DiagramType diagramType)
    => RendererCache.TryGetValue(diagramType, out var renderer) ? renderer : null;

  #endregion

  #region Protected Methods

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    base.OnPaint(e);
    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    var context = this._CreateRenderContext(e.Graphics, this.ClientRectangle);
    this._RenderDiagram(context);
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);
    this._HandleHover(e.Location);
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);
    this._HandleClick(e.Location, e.Button);
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    this._toolTip?.Hide(this);
    this._hoveredNode = null;
    this._hoveredEdge = null;
    this._hoveredLink = null;
    this._highlightedLegendIndex = -1;
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._toolTip?.Dispose();
      this._toolTip = null;
      this._animationTimer?.Dispose();
      this._animationTimer = null;
    }
    base.Dispose(disposing);
  }

  /// <summary>Raises the NodeClicked event.</summary>
  protected virtual void OnNodeClicked(DiagramNodeEventArgs e) => this.NodeClicked?.Invoke(this, e);

  /// <summary>Raises the EdgeClicked event.</summary>
  protected virtual void OnEdgeClicked(DiagramEdgeEventArgs e) => this.EdgeClicked?.Invoke(this, e);

  /// <summary>Raises the NodeHovered event.</summary>
  protected virtual void OnNodeHovered(DiagramNodeEventArgs e) => this.NodeHovered?.Invoke(this, e);

  /// <summary>Raises the EdgeHovered event.</summary>
  protected virtual void OnEdgeHovered(DiagramEdgeEventArgs e) => this.EdgeHovered?.Invoke(this, e);

  /// <summary>Raises the LinkClicked event.</summary>
  protected virtual void OnLinkClicked(DiagramLinkEventArgs e) => this.LinkClicked?.Invoke(this, e);

  #endregion

  #region Private Methods

  private DiagramRenderer _GetRenderer() {
    if (this._customRenderer != null)
      return this._customRenderer;

    if (RendererCache.TryGetValue(this._diagramType, out var renderer))
      return renderer;

    // Create and cache default renderers
    renderer = this._CreateDefaultRenderer(this._diagramType);
    if (renderer != null)
      RendererCache[this._diagramType] = renderer;

    return renderer;
  }

  private DiagramRenderer _CreateDefaultRenderer(DiagramType diagramType) {
    return diagramType switch {
      DiagramType.Sankey => new Renderers.SankeyDiagramRenderer(),
      DiagramType.Chord => new Renderers.ChordDiagramRenderer(),
      DiagramType.Arc => new Renderers.ArcDiagramRenderer(),
      DiagramType.Network => new Renderers.NetworkDiagramRenderer(),
      DiagramType.Tree => new Renderers.TreeDiagramRenderer(),
      DiagramType.Dendrogram => new Renderers.DendrogramDiagramRenderer(),
      DiagramType.CirclePacking => new Renderers.CirclePackingDiagramRenderer(),
      DiagramType.FlowChart => new Renderers.FlowChartDiagramRenderer(),
      _ => new Renderers.NetworkDiagramRenderer() // Default fallback
    };
  }

  private DiagramRenderContext _CreateRenderContext(Graphics g, RectangleF bounds) {
    this._CalculateLayout(g, bounds);

    return new DiagramRenderContext {
      Graphics = g,
      Diagram = this,
      TotalBounds = bounds,
      PlotArea = this._plotArea,
      HighlightedNodeId = this._hoveredNode?.Id,
      HighlightedEdge = this._hoveredEdge,
      AnimationProgress = this._animationProgress
    };
  }

  private void _CalculateLayout(Graphics g, RectangleF bounds) {
    var left = bounds.Left + this._padding;
    var top = bounds.Top + this._padding;
    var right = bounds.Right - this._padding;
    var bottom = bounds.Bottom - this._padding;

    // Title area
    if (!string.IsNullOrEmpty(this._title)) {
      var titleFont = this._titleFont ?? new Font(this.Font.FontFamily, this.Font.Size + 4, FontStyle.Bold);
      var titleSize = g.MeasureString(this._title, titleFont);
      top += titleSize.Height + 5;
    }

    // Subtitle area
    if (!string.IsNullOrEmpty(this._subtitle)) {
      var subtitleFont = this._subtitleFont ?? this.Font;
      var subtitleSize = g.MeasureString(this._subtitle, subtitleFont);
      top += subtitleSize.Height + 5;
    }

    // Legend area
    if (this._showLegend && this._legendPosition != DiagramLegendPosition.None) {
      this._legendItems = this._GetLegendItems();
      if (this._legendItems.Count > 0) {
        var legendSize = this._CalculateLegendSize(g, this._legendItems);

        switch (this._legendPosition) {
          case DiagramLegendPosition.Right:
          case DiagramLegendPosition.TopRight:
          case DiagramLegendPosition.BottomRight:
            right -= legendSize.Width + 10;
            break;
          case DiagramLegendPosition.Left:
          case DiagramLegendPosition.TopLeft:
          case DiagramLegendPosition.BottomLeft:
            left += legendSize.Width + 10;
            break;
          case DiagramLegendPosition.Top:
            top += legendSize.Height + 5;
            break;
          case DiagramLegendPosition.Bottom:
            bottom -= legendSize.Height + 5;
            break;
        }
      }
    }

    this._plotArea = new RectangleF(left, top, right - left, bottom - top);
  }

  private void _RenderDiagram(DiagramRenderContext context) {
    this._hitTestRects.Clear();

    // Draw title
    this._DrawTitle(context.Graphics);

    // Draw plot area background
    if (this._plotAreaBackground != Color.Transparent) {
      using var bgBrush = new SolidBrush(this._plotAreaBackground);
      context.Graphics.FillRectangle(bgBrush, this._plotArea);
    }

    // Draw plot area border
    if (this._plotAreaBorderWidth > 0) {
      using var borderPen = new Pen(this._plotAreaBorderColor, this._plotAreaBorderWidth);
      context.Graphics.DrawRectangle(borderPen, this._plotArea.X, this._plotArea.Y, this._plotArea.Width, this._plotArea.Height);
    }

    // Render the diagram with clipping
    var renderer = this._GetRenderer();
    if (renderer != null) {
      var previousClip = context.Graphics.Clip;
      context.Graphics.SetClip(this._plotArea);
      try {
        renderer.Render(context);
      } finally {
        context.Graphics.Clip = previousClip;
      }
    }

    // Copy hit test rects
    foreach (var kvp in context.HitTestRects)
      this._hitTestRects[kvp.Key] = kvp.Value;

    // Draw legend
    if (this._showLegend && this._legendPosition != DiagramLegendPosition.None && this._legendItems.Count > 0)
      this._DrawLegend(context.Graphics, context.TotalBounds, this._legendItems);
  }

  private void _DrawTitle(Graphics g) {
    var y = (float)this._padding;

    if (!string.IsNullOrEmpty(this._title)) {
      var titleFont = this._titleFont ?? new Font(this.Font.FontFamily, this.Font.Size + 4, FontStyle.Bold);
      var titleSize = g.MeasureString(this._title, titleFont);
      using var brush = new SolidBrush(this._titleColor);
      g.DrawString(this._title, titleFont, brush, (this.Width - titleSize.Width) / 2, y);
      y += titleSize.Height + 5;
    }

    if (!string.IsNullOrEmpty(this._subtitle)) {
      var subtitleFont = this._subtitleFont ?? this.Font;
      var subtitleSize = g.MeasureString(this._subtitle, subtitleFont);
      using var brush = new SolidBrush(this._subtitleColor);
      g.DrawString(this._subtitle, subtitleFont, brush, (this.Width - subtitleSize.Width) / 2, y);
    }
  }

  private List<DiagramLegendItem> _GetLegendItems() {
    var renderer = this._GetRenderer();
    if (renderer != null)
      return new List<DiagramLegendItem>(renderer.GetLegendItems(this));

    // Default legend items based on nodes
    var colors = this._GetColorPalette();
    return this._nodes.Select((n, i) => new DiagramLegendItem {
      Text = n.Label ?? n.Id,
      Color = n.Color ?? colors[i % colors.Length],
      Visible = true,
      Tag = n
    }).ToList();
  }

  private SizeF _CalculateLegendSize(Graphics g, IList<DiagramLegendItem> items) {
    var maxWidth = 0f;
    var totalHeight = 10f; // Padding

    foreach (var item in items) {
      var textSize = g.MeasureString(item.Text, this.Font);
      maxWidth = Math.Max(maxWidth, textSize.Width + 25); // Icon + spacing
      totalHeight += textSize.Height + 5;
    }

    return new SizeF(maxWidth + 20, totalHeight);
  }

  private void _DrawLegend(Graphics g, RectangleF totalBounds, IList<DiagramLegendItem> items) {
    var legendSize = this._CalculateLegendSize(g, items);
    var legendBounds = this._GetLegendBounds(totalBounds, legendSize);

    // Draw background
    using (var brush = new SolidBrush(Color.FromArgb(245, 245, 245)))
      g.FillRectangle(brush, legendBounds);

    using (var pen = new Pen(Color.LightGray))
      g.DrawRectangle(pen, legendBounds.X, legendBounds.Y, legendBounds.Width, legendBounds.Height);

    // Draw items
    var y = legendBounds.Top + 5;
    for (var i = 0; i < items.Count; ++i) {
      var item = items[i];
      var x = legendBounds.Left + 5;

      // Draw color box
      using (var brush = new SolidBrush(item.Color))
        g.FillRectangle(brush, x, y + 2, 12, 12);

      using (var pen = new Pen(Color.Gray))
        g.DrawRectangle(pen, x, y + 2, 12, 12);

      // Draw text
      x += 20;
      var textColor = i == this._highlightedLegendIndex ? Color.Blue : Color.Black;
      using (var brush = new SolidBrush(textColor))
        g.DrawString(item.Text, this.Font, brush, x, y);

      y += g.MeasureString(item.Text, this.Font).Height + 5;
    }
  }

  private RectangleF _GetLegendBounds(RectangleF totalBounds, SizeF legendSize) {
    return this._legendPosition switch {
      DiagramLegendPosition.Right => new RectangleF(totalBounds.Right - legendSize.Width - this._padding, this._plotArea.Top, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.Left => new RectangleF(totalBounds.Left + this._padding, this._plotArea.Top, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.Top => new RectangleF(totalBounds.Left + (totalBounds.Width - legendSize.Width) / 2, totalBounds.Top + this._padding, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.Bottom => new RectangleF(totalBounds.Left + (totalBounds.Width - legendSize.Width) / 2, totalBounds.Bottom - legendSize.Height - this._padding, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.TopRight => new RectangleF(totalBounds.Right - legendSize.Width - this._padding, totalBounds.Top + this._padding, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.TopLeft => new RectangleF(totalBounds.Left + this._padding, totalBounds.Top + this._padding, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.BottomRight => new RectangleF(totalBounds.Right - legendSize.Width - this._padding, totalBounds.Bottom - legendSize.Height - this._padding, legendSize.Width, legendSize.Height),
      DiagramLegendPosition.BottomLeft => new RectangleF(totalBounds.Left + this._padding, totalBounds.Bottom - legendSize.Height - this._padding, legendSize.Width, legendSize.Height),
      _ => new RectangleF(totalBounds.Right - legendSize.Width - this._padding, this._plotArea.Top, legendSize.Width, legendSize.Height)
    };
  }

  internal Color[] _GetColorPalette() => this._colorPalette switch {
    DiagramColorPalette.Custom when this._customColors != null => this._customColors,
    DiagramColorPalette.Pastel => new[] { Color.FromArgb(174, 198, 207), Color.FromArgb(255, 179, 186), Color.FromArgb(255, 223, 186), Color.FromArgb(255, 255, 186), Color.FromArgb(186, 255, 201), Color.FromArgb(186, 225, 255) },
    DiagramColorPalette.Bright => new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple },
    DiagramColorPalette.Dark => new[] { Color.DarkRed, Color.DarkOrange, Color.DarkGoldenrod, Color.DarkGreen, Color.DarkBlue, Color.DarkViolet },
    DiagramColorPalette.Monochrome => new[] { Color.FromArgb(50, 50, 50), Color.FromArgb(100, 100, 100), Color.FromArgb(150, 150, 150), Color.FromArgb(200, 200, 200) },
    DiagramColorPalette.Category10 => new[] { Color.FromArgb(31, 119, 180), Color.FromArgb(255, 127, 14), Color.FromArgb(44, 160, 44), Color.FromArgb(214, 39, 40), Color.FromArgb(148, 103, 189), Color.FromArgb(140, 86, 75), Color.FromArgb(227, 119, 194), Color.FromArgb(127, 127, 127), Color.FromArgb(188, 189, 34), Color.FromArgb(23, 190, 207) },
    DiagramColorPalette.Tableau10 => new[] { Color.FromArgb(78, 121, 167), Color.FromArgb(242, 142, 44), Color.FromArgb(225, 87, 89), Color.FromArgb(118, 183, 178), Color.FromArgb(89, 161, 79), Color.FromArgb(237, 201, 73), Color.FromArgb(175, 122, 161), Color.FromArgb(255, 157, 167), Color.FromArgb(156, 117, 95), Color.FromArgb(186, 176, 171) },
    DiagramColorPalette.Material => new[] { Color.FromArgb(244, 67, 54), Color.FromArgb(233, 30, 99), Color.FromArgb(156, 39, 176), Color.FromArgb(103, 58, 183), Color.FromArgb(63, 81, 181), Color.FromArgb(33, 150, 243), Color.FromArgb(0, 188, 212), Color.FromArgb(0, 150, 136), Color.FromArgb(76, 175, 80), Color.FromArgb(255, 193, 7) },
    _ => new[] { Color.FromArgb(52, 152, 219), Color.FromArgb(231, 76, 60), Color.FromArgb(46, 204, 113), Color.FromArgb(155, 89, 182), Color.FromArgb(241, 196, 15), Color.FromArgb(230, 126, 34), Color.FromArgb(26, 188, 156), Color.FromArgb(52, 73, 94) }
  };

  private void _HandleHover(Point location) {
    // Check legend hit
    if (this._showLegend && this._legendItems.Count > 0) {
      var legendSize = this._CalculateLegendSize(this.CreateGraphics(), this._legendItems);
      var legendBounds = this._GetLegendBounds(this.ClientRectangle, legendSize);
      if (legendBounds.Contains(location)) {
        // Calculate which item
        var y = legendBounds.Top + 5;
        using var g = this.CreateGraphics();
        for (var i = 0; i < this._legendItems.Count; ++i) {
          var itemHeight = g.MeasureString(this._legendItems[i].Text, this.Font).Height + 5;
          if (location.Y >= y && location.Y < y + itemHeight) {
            if (this._highlightedLegendIndex != i) {
              this._highlightedLegendIndex = i;
              this.Invalidate();
            }
            return;
          }
          y += itemHeight;
        }
      }
    }

    if (this._highlightedLegendIndex != -1) {
      this._highlightedLegendIndex = -1;
      this.Invalidate();
    }

    // Check element hit
    DiagramNode hoveredNode = null;
    DiagramEdge hoveredEdge = null;
    DiagramSankeyLink hoveredLink = null;

    foreach (var kvp in this._hitTestRects) {
      if (kvp.Value.Contains(location)) {
        switch (kvp.Key) {
          case DiagramNode node:
            hoveredNode = node;
            break;
          case DiagramEdge edge:
            hoveredEdge = edge;
            break;
          case DiagramSankeyLink link:
            hoveredLink = link;
            break;
        }
        break;
      }
    }

    var changed = hoveredNode != this._hoveredNode || hoveredEdge != this._hoveredEdge || hoveredLink != this._hoveredLink;
    this._hoveredNode = hoveredNode;
    this._hoveredEdge = hoveredEdge;
    this._hoveredLink = hoveredLink;

    if (changed) {
      if (this._enableTooltips) {
        string tooltipText = null;

        if (hoveredNode != null) {
          tooltipText = hoveredNode.Label ?? hoveredNode.Id;
          if (hoveredNode.Size != 1)
            tooltipText += $"\nSize: {hoveredNode.Size:N2}";
          this.OnNodeHovered(new DiagramNodeEventArgs(hoveredNode));
        } else if (hoveredEdge != null) {
          tooltipText = $"{hoveredEdge.Source} → {hoveredEdge.Target}";
          if (hoveredEdge.Weight != 1)
            tooltipText += $"\nWeight: {hoveredEdge.Weight:N2}";
          this.OnEdgeHovered(new DiagramEdgeEventArgs(hoveredEdge));
        } else if (hoveredLink != null)
          tooltipText = $"{hoveredLink.Source} → {hoveredLink.Target}\nValue: {hoveredLink.Value:N2}";

        if (tooltipText != null)
          this._toolTip?.Show(tooltipText, this, location.X + 15, location.Y + 15);
        else
          this._toolTip?.Hide(this);
      }

      this.Invalidate();
    }
  }

  private void _HandleClick(Point location, MouseButtons button) {
    if (button != MouseButtons.Left)
      return;

    // Check element hit
    foreach (var kvp in this._hitTestRects) {
      if (kvp.Value.Contains(location)) {
        switch (kvp.Key) {
          case DiagramNode node:
            this.OnNodeClicked(new DiagramNodeEventArgs(node));
            return;
          case DiagramEdge edge:
            this.OnEdgeClicked(new DiagramEdgeEventArgs(edge));
            return;
          case DiagramSankeyLink link:
            this.OnLinkClicked(new DiagramLinkEventArgs(link));
            return;
        }
      }
    }
  }

  private void _StartAnimation() {
    if (!this._enableAnimation || this._animationDuration <= 0) {
      this._animationProgress = 1.0;
      return;
    }

    this._animationTimer?.Stop();

    this._animationProgress = 0;
    this._animationStartTime = DateTime.Now;

    if (this._animationTimer == null) {
      this._animationTimer = new Timer { Interval = 16 }; // ~60 FPS
      this._animationTimer.Tick += this._OnAnimationTick;
    }

    this._animationTimer.Start();
  }

  private void _OnAnimationTick(object sender, EventArgs e) {
    var elapsed = (DateTime.Now - this._animationStartTime).TotalMilliseconds;
    this._animationProgress = Math.Min(1.0, elapsed / this._animationDuration);

    // Apply easing
    this._animationProgress = this._EaseOutCubic(this._animationProgress);

    this.Invalidate();
    this.Update();

    if (this._animationProgress >= 1.0)
      this._animationTimer.Stop();
  }

  private double _EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);

  #endregion
}

#region Event Args

/// <summary>Event arguments for diagram node events.</summary>
public class DiagramNodeEventArgs : EventArgs {
  public DiagramNode Node { get; }

  public DiagramNodeEventArgs(DiagramNode node) => this.Node = node;
}

/// <summary>Event arguments for diagram edge events.</summary>
public class DiagramEdgeEventArgs : EventArgs {
  public DiagramEdge Edge { get; }

  public DiagramEdgeEventArgs(DiagramEdge edge) => this.Edge = edge;
}

/// <summary>Event arguments for diagram link events.</summary>
public class DiagramLinkEventArgs : EventArgs {
  public DiagramSankeyLink Link { get; }

  public DiagramLinkEventArgs(DiagramSankeyLink link) => this.Link = link;
}

#endregion
