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

  // Zoom and pan state
  private float _zoomLevel = 1.0f;
  private PointF _panOffset = PointF.Empty;
  private bool _enableZoom = true;
  private bool _showZoomIndicator;
  private DiagramCornerPosition _zoomIndicatorPosition = DiagramCornerPosition.BottomRight;
  private bool _isUserZoomed;
  private float _minZoom = 0.1f;
  private float _maxZoom = 10.0f;
  private float _zoomStep = 0.1f;
  private bool _isPanning;
  private Point _lastPanPoint;
  private bool _isDraggingSlider;
  private RectangleF _zoomSliderTrackRect;
  private RectangleF _zoomTextRect;
  private TextBox _zoomInputBox;
  private bool _showZoomSlider = true;

  private static readonly Dictionary<DiagramType, DiagramRenderer> RendererCache = new();

  // Data collections - Base
  private readonly DiagramNodeCollection _nodes;
  private readonly DiagramEdgeCollection _edges;
  private readonly DiagramSankeyLinkCollection _sankeyLinks;
  private readonly DiagramHierarchyNodeCollection _hierarchyNodes;

  // Data collections - UML
  private readonly DiagramClassNodeCollection _classNodes;
  private readonly DiagramClassRelationCollection _classRelations;
  private readonly DiagramLifelineCollection _lifelines;
  private readonly DiagramMessageCollection _messages;
  private readonly DiagramActivationCollection _activations;
  private readonly DiagramActorCollection _actors;
  private readonly DiagramUseCaseCollection _useCases;
  private readonly DiagramSwimlaneCollection _swimlanes;
  private readonly DiagramComponentCollection _components;
  private readonly DiagramDeploymentNodeCollection _deploymentNodes;
  private readonly DiagramPackageCollection _packages;

  // Data collections - Database
  private readonly DiagramEntityCollection _entities;
  private readonly DiagramRelationshipCollection _relationships;
  private readonly DiagramDataFlowElementCollection _dataFlowElements;

  // Data collections - Business
  private readonly DiagramSetCollection _sets;
  private readonly DiagramSetIntersectionCollection _setIntersections;
  private readonly DiagramQuadrantCollection _quadrants;
  private readonly DiagramMatrixItemCollection _matrixItems;
  private readonly DiagramJourneyStageCollection _journeyStages;
  private readonly DiagramBPMNElementCollection _bpmnElements;
  private readonly DiagramKanbanColumnCollection _kanbanColumns;
  private readonly DiagramKanbanCardCollection _kanbanCards;

  // Data collections - Architecture
  private readonly DiagramGitCommitCollection _gitCommits;
  private readonly DiagramGitBranchCollection _gitBranches;
  private readonly DiagramRequirementCollection _requirements;
  private readonly DiagramRequirementRelationCollection _requirementRelations;

  // Data collections - Technical
  private readonly DiagramRackCollection _racks;
  private readonly DiagramRackDeviceCollection _rackDevices;
  private readonly DiagramPacketFieldCollection _packetFields;
  private readonly DiagramByteFieldCollection _byteFields;
  private readonly DiagramSignalCollection _signals;

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

  /// <summary>Occurs when any diagram element is clicked.</summary>
  [Category("Action")]
  [Description("Occurs when any diagram element is clicked.")]
  public event EventHandler<DiagramElementEventArgs> ElementClicked;

  /// <summary>Occurs when any diagram element is hovered.</summary>
  [Category("Action")]
  [Description("Occurs when any diagram element is hovered.")]
  public event EventHandler<DiagramElementEventArgs> ElementHovered;

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

    // Initialize data collections - Base
    this._nodes = new DiagramNodeCollection(this);
    this._edges = new DiagramEdgeCollection(this);
    this._sankeyLinks = new DiagramSankeyLinkCollection(this);
    this._hierarchyNodes = new DiagramHierarchyNodeCollection(this);

    // Initialize data collections - UML
    this._classNodes = new DiagramClassNodeCollection(this);
    this._classRelations = new DiagramClassRelationCollection(this);
    this._lifelines = new DiagramLifelineCollection(this);
    this._messages = new DiagramMessageCollection(this);
    this._activations = new DiagramActivationCollection(this);
    this._actors = new DiagramActorCollection(this);
    this._useCases = new DiagramUseCaseCollection(this);
    this._swimlanes = new DiagramSwimlaneCollection(this);
    this._components = new DiagramComponentCollection(this);
    this._deploymentNodes = new DiagramDeploymentNodeCollection(this);
    this._packages = new DiagramPackageCollection(this);

    // Initialize data collections - Database
    this._entities = new DiagramEntityCollection(this);
    this._relationships = new DiagramRelationshipCollection(this);
    this._dataFlowElements = new DiagramDataFlowElementCollection(this);

    // Initialize data collections - Business
    this._sets = new DiagramSetCollection(this);
    this._setIntersections = new DiagramSetIntersectionCollection(this);
    this._quadrants = new DiagramQuadrantCollection(this);
    this._matrixItems = new DiagramMatrixItemCollection(this);
    this._journeyStages = new DiagramJourneyStageCollection(this);
    this._bpmnElements = new DiagramBPMNElementCollection(this);
    this._kanbanColumns = new DiagramKanbanColumnCollection(this);
    this._kanbanCards = new DiagramKanbanCardCollection(this);

    // Initialize data collections - Architecture
    this._gitCommits = new DiagramGitCommitCollection(this);
    this._gitBranches = new DiagramGitBranchCollection(this);
    this._requirements = new DiagramRequirementCollection(this);
    this._requirementRelations = new DiagramRequirementRelationCollection(this);

    // Initialize data collections - Technical
    this._racks = new DiagramRackCollection(this);
    this._rackDevices = new DiagramRackDeviceCollection(this);
    this._packetFields = new DiagramPacketFieldCollection(this);
    this._byteFields = new DiagramByteFieldCollection(this);
    this._signals = new DiagramSignalCollection(this);
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

  #region UML Data Collections

  /// <summary>Gets the class nodes collection for class/object diagrams.</summary>
  [Category("Data")]
  [Description("Class diagram nodes with fields and methods.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramClassNodeCollection ClassNodes => this._classNodes;

  /// <summary>Gets the class relations collection for class/object diagrams.</summary>
  [Category("Data")]
  [Description("Class diagram relationships (inheritance, association, etc.).")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramClassRelationCollection ClassRelations => this._classRelations;

  /// <summary>Gets the lifelines collection for sequence diagrams.</summary>
  [Category("Data")]
  [Description("Sequence diagram lifelines (participants).")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramLifelineCollection Lifelines => this._lifelines;

  /// <summary>Gets the messages collection for sequence diagrams.</summary>
  [Category("Data")]
  [Description("Sequence diagram messages between lifelines.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramMessageCollection Messages => this._messages;

  /// <summary>Gets the activations collection for sequence diagrams.</summary>
  [Category("Data")]
  [Description("Sequence diagram activation boxes on lifelines.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramActivationCollection Activations => this._activations;

  /// <summary>Gets the actors collection for use case diagrams.</summary>
  [Category("Data")]
  [Description("Use case diagram actors.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramActorCollection Actors => this._actors;

  /// <summary>Gets the use cases collection for use case diagrams.</summary>
  [Category("Data")]
  [Description("Use case diagram use cases.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramUseCaseCollection UseCases => this._useCases;

  /// <summary>Gets the swimlanes collection for activity diagrams.</summary>
  [Category("Data")]
  [Description("Activity diagram swimlanes.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramSwimlaneCollection Swimlanes => this._swimlanes;

  /// <summary>Gets the components collection for component diagrams.</summary>
  [Category("Data")]
  [Description("Component diagram components with interfaces.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramComponentCollection Components => this._components;

  /// <summary>Gets the deployment nodes collection for deployment diagrams.</summary>
  [Category("Data")]
  [Description("Deployment diagram nodes (devices, execution environments).")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramDeploymentNodeCollection DeploymentNodes => this._deploymentNodes;

  /// <summary>Gets the packages collection for package diagrams.</summary>
  [Category("Data")]
  [Description("Package diagram packages.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramPackageCollection Packages => this._packages;

  #endregion

  #region Database Data Collections

  /// <summary>Gets the entities collection for ER diagrams.</summary>
  [Category("Data")]
  [Description("ER diagram entities.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramEntityCollection Entities => this._entities;

  /// <summary>Gets the relationships collection for ER diagrams.</summary>
  [Category("Data")]
  [Description("ER diagram relationships with cardinality.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramRelationshipCollection Relationships => this._relationships;

  /// <summary>Gets the data flow elements collection for DFD diagrams.</summary>
  [Category("Data")]
  [Description("Data flow diagram elements (processes, stores, external entities).")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramDataFlowElementCollection DataFlowElements => this._dataFlowElements;

  #endregion

  #region Business Data Collections

  /// <summary>Gets the sets collection for Venn diagrams.</summary>
  [Category("Data")]
  [Description("Venn diagram sets.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramSetCollection Sets => this._sets;

  /// <summary>Gets the set intersections collection for Venn diagrams.</summary>
  [Category("Data")]
  [Description("Venn diagram set intersections.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramSetIntersectionCollection SetIntersections => this._setIntersections;

  /// <summary>Gets the quadrants collection for matrix/SWOT diagrams.</summary>
  [Category("Data")]
  [Description("Matrix/SWOT diagram quadrant definitions.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramQuadrantCollection Quadrants => this._quadrants;

  /// <summary>Gets the matrix items collection for matrix diagrams.</summary>
  [Category("Data")]
  [Description("Matrix diagram items positioned in quadrants.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramMatrixItemCollection MatrixItems => this._matrixItems;

  /// <summary>Gets the journey stages collection for journey map diagrams.</summary>
  [Category("Data")]
  [Description("Journey map stages with actions and scores.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramJourneyStageCollection JourneyStages => this._journeyStages;

  /// <summary>Gets the BPMN elements collection for BPMN diagrams.</summary>
  [Category("Data")]
  [Description("BPMN diagram elements (tasks, gateways, events).")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramBPMNElementCollection BPMNElements => this._bpmnElements;

  /// <summary>Gets the Kanban columns collection for Kanban diagrams.</summary>
  [Category("Data")]
  [Description("Kanban board columns with WIP limits.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramKanbanColumnCollection KanbanColumns => this._kanbanColumns;

  /// <summary>Gets the Kanban cards collection for Kanban diagrams.</summary>
  [Category("Data")]
  [Description("Kanban board cards.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramKanbanCardCollection KanbanCards => this._kanbanCards;

  #endregion

  #region Architecture Data Collections

  /// <summary>Gets the Git commits collection for Gitgraph diagrams.</summary>
  [Category("Data")]
  [Description("Git graph commits.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramGitCommitCollection GitCommits => this._gitCommits;

  /// <summary>Gets the Git branches collection for Gitgraph diagrams.</summary>
  [Category("Data")]
  [Description("Git graph branches.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramGitBranchCollection GitBranches => this._gitBranches;

  /// <summary>Gets the requirements collection for Requirement diagrams.</summary>
  [Category("Data")]
  [Description("Requirement diagram requirements.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramRequirementCollection Requirements => this._requirements;

  /// <summary>Gets the requirement relations collection for Requirement diagrams.</summary>
  [Category("Data")]
  [Description("Requirement diagram relationships.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramRequirementRelationCollection RequirementRelations => this._requirementRelations;

  #endregion

  #region Technical Data Collections

  /// <summary>Gets the racks collection for rack diagrams.</summary>
  [Category("Data")]
  [Description("Server rack definitions.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramRackCollection Racks => this._racks;

  /// <summary>Gets the rack devices collection for rack diagrams.</summary>
  [Category("Data")]
  [Description("Devices installed in server racks.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramRackDeviceCollection RackDevices => this._rackDevices;

  /// <summary>Gets the packet fields collection for packet diagrams.</summary>
  [Category("Data")]
  [Description("Network packet fields.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramPacketFieldCollection PacketFields => this._packetFields;

  /// <summary>Gets the byte fields collection for byte field diagrams.</summary>
  [Category("Data")]
  [Description("Binary data structure fields.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramByteFieldCollection ByteFields => this._byteFields;

  /// <summary>Gets the signals collection for waveform/timing diagrams.</summary>
  [Category("Data")]
  [Description("Digital waveform signals.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public DiagramSignalCollection Signals => this._signals;

  #endregion

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

  #region Zoom Properties

  /// <summary>Gets or sets the current zoom level.</summary>
  [Category("Zoom")]
  [Description("The current zoom level (1.0 = 100%).")]
  [DefaultValue(1.0f)]
  public float ZoomLevel {
    get => this._zoomLevel;
    set {
      value = Math.Max(this._minZoom, Math.Min(this._maxZoom, value));
      if (Math.Abs(this._zoomLevel - value) < 0.0001f)
        return;
      this._zoomLevel = value;
      this._isUserZoomed = Math.Abs(value - 1.0f) > 0.0001f;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets whether zooming is enabled.</summary>
  [Category("Zoom")]
  [Description("Whether to enable mouse wheel zooming.")]
  [DefaultValue(true)]
  public bool EnableZoom {
    get => this._enableZoom;
    set => this._enableZoom = value;
  }

  /// <summary>Gets or sets whether to show the zoom indicator.</summary>
  [Category("Zoom")]
  [Description("Whether to show a zoom level indicator in a corner.")]
  [DefaultValue(false)]
  public bool ShowZoomIndicator {
    get => this._showZoomIndicator;
    set {
      this._showZoomIndicator = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the zoom indicator position.</summary>
  [Category("Zoom")]
  [Description("The corner position of the zoom indicator.")]
  [DefaultValue(DiagramCornerPosition.BottomRight)]
  public DiagramCornerPosition ZoomIndicatorPosition {
    get => this._zoomIndicatorPosition;
    set {
      this._zoomIndicatorPosition = value;
      this.Invalidate();
    }
  }

  /// <summary>Gets or sets the minimum zoom level.</summary>
  [Category("Zoom")]
  [Description("The minimum allowed zoom level.")]
  [DefaultValue(0.1f)]
  public float MinZoom {
    get => this._minZoom;
    set => this._minZoom = Math.Max(0.01f, value);
  }

  /// <summary>Gets or sets the maximum zoom level.</summary>
  [Category("Zoom")]
  [Description("The maximum allowed zoom level.")]
  [DefaultValue(10.0f)]
  public float MaxZoom {
    get => this._maxZoom;
    set => this._maxZoom = Math.Max(this._minZoom, value);
  }

  /// <summary>Gets or sets the zoom step for mouse wheel.</summary>
  [Category("Zoom")]
  [Description("The amount to zoom per mouse wheel notch.")]
  [DefaultValue(0.1f)]
  public float ZoomStep {
    get => this._zoomStep;
    set => this._zoomStep = Math.Max(0.01f, value);
  }

  /// <summary>Gets the current pan offset.</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public PointF PanOffset => this._panOffset;

  /// <summary>Gets whether the user has manually zoomed.</summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public bool IsUserZoomed => this._isUserZoomed;

  /// <summary>Gets or sets whether to show a slider next to the zoom indicator.</summary>
  [Category("Zoom")]
  [Description("Whether to show a zoom slider next to the zoom indicator.")]
  [DefaultValue(true)]
  public bool ShowZoomSlider {
    get => this._showZoomSlider;
    set {
      this._showZoomSlider = value;
      this.Invalidate();
    }
  }

  #endregion

  #endregion

  #region Public Methods

  /// <summary>
  /// Clears all diagram data.
  /// </summary>
  public void Clear() {
    // Clear base collections
    this._nodes.Clear();
    this._edges.Clear();
    this._sankeyLinks.Clear();
    this._hierarchyNodes.Clear();

    // Clear UML collections
    this._classNodes.Clear();
    this._classRelations.Clear();
    this._lifelines.Clear();
    this._messages.Clear();
    this._activations.Clear();
    this._actors.Clear();
    this._useCases.Clear();
    this._swimlanes.Clear();
    this._components.Clear();
    this._deploymentNodes.Clear();
    this._packages.Clear();

    // Clear Database collections
    this._entities.Clear();
    this._relationships.Clear();
    this._dataFlowElements.Clear();

    // Clear Business collections
    this._sets.Clear();
    this._setIntersections.Clear();
    this._quadrants.Clear();
    this._matrixItems.Clear();
    this._journeyStages.Clear();
    this._bpmnElements.Clear();
    this._kanbanColumns.Clear();
    this._kanbanCards.Clear();

    // Clear Architecture collections
    this._gitCommits.Clear();
    this._gitBranches.Clear();
    this._requirements.Clear();
    this._requirementRelations.Clear();

    // Clear Technical collections
    this._racks.Clear();
    this._rackDevices.Clear();
    this._packetFields.Clear();
    this._byteFields.Clear();
    this._signals.Clear();

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

  /// <summary>
  /// Resets zoom to 100% and clears pan offset.
  /// </summary>
  public void ResetZoom() {
    this._zoomLevel = 1.0f;
    this._panOffset = PointF.Empty;
    this._isUserZoomed = false;
    this.Invalidate();
  }

  /// <summary>
  /// Zooms to fit all content within the view.
  /// </summary>
  public void ZoomToFit() {
    this._zoomLevel = 1.0f;
    this._panOffset = PointF.Empty;
    this._isUserZoomed = false;
    this.Invalidate();
  }

  /// <summary>
  /// Sets zoom level centered on a specific point.
  /// </summary>
  public void ZoomAt(float newZoom, PointF center) {
    newZoom = Math.Max(this._minZoom, Math.Min(this._maxZoom, newZoom));

    // Calculate the point in content coordinates before zoom
    var contentX = (center.X - this._plotArea.Left - this._panOffset.X) / this._zoomLevel;
    var contentY = (center.Y - this._plotArea.Top - this._panOffset.Y) / this._zoomLevel;

    // Update zoom
    this._zoomLevel = newZoom;
    this._isUserZoomed = Math.Abs(newZoom - 1.0f) > 0.0001f;

    // Calculate new pan offset to keep the same content point under the mouse
    this._panOffset = new PointF(
      center.X - this._plotArea.Left - contentX * this._zoomLevel,
      center.Y - this._plotArea.Top - contentY * this._zoomLevel
    );

    this.Invalidate();
  }

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
  protected override void OnMouseWheel(MouseEventArgs e) {
    base.OnMouseWheel(e);

    if (!this._enableZoom || !this._plotArea.Contains(e.Location))
      return;

    var delta = e.Delta > 0 ? this._zoomStep : -this._zoomStep;
    var newZoom = this._zoomLevel + delta * this._zoomLevel; // Proportional zoom

    this.ZoomAt(newZoom, e.Location);
  }

  /// <inheritdoc />
  protected override void OnMouseDown(MouseEventArgs e) {
    base.OnMouseDown(e);

    if (e.Button == MouseButtons.Left) {
      // Check zoom slider drag start
      if (this._showZoomIndicator && this._showZoomSlider) {
        var thumbExtent = new RectangleF(
          this._zoomSliderTrackRect.Left - 6,
          this._zoomSliderTrackRect.Top - 10,
          this._zoomSliderTrackRect.Width + 12,
          this._zoomSliderTrackRect.Height + 20
        );
        if (thumbExtent.Contains(e.Location)) {
          this._isDraggingSlider = true;
          this._SetZoomFromSliderPosition(e.Location.X);
          return;
        }
      }
    }

    if (!this._plotArea.Contains(e.Location))
      return;

    // Middle mouse button or Left button when zoomed for panning
    if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && this._isUserZoomed)) {
      this._isPanning = true;
      this._lastPanPoint = e.Location;
      this.Cursor = Cursors.SizeAll;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (this._isDraggingSlider) {
      this._SetZoomFromSliderPosition(e.Location.X);
      return;
    }

    if (this._isPanning) {
      var dx = e.Location.X - this._lastPanPoint.X;
      var dy = e.Location.Y - this._lastPanPoint.Y;
      this._panOffset = new PointF(this._panOffset.X + dx, this._panOffset.Y + dy);
      this._lastPanPoint = e.Location;
      this.Invalidate();
      return;
    }

    this._HandleHover(e.Location);
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);

    if (this._isDraggingSlider) {
      this._isDraggingSlider = false;
      return;
    }

    if (this._isPanning) {
      this._isPanning = false;
      this.Cursor = Cursors.Default;
      return;
    }

    // Handle zoom indicator interaction
    if (e.Button == MouseButtons.Left && this._showZoomIndicator) {
      this._HandleZoomIndicatorClick(e.Location);
    }

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

  /// <summary>Raises the ElementClicked event.</summary>
  protected virtual void OnElementClicked(DiagramElementEventArgs e) => this.ElementClicked?.Invoke(this, e);

  /// <summary>Raises the ElementHovered event.</summary>
  protected virtual void OnElementHovered(DiagramElementEventArgs e) => this.ElementHovered?.Invoke(this, e);

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
      // Original 8 types
      DiagramType.Sankey => new Renderers.SankeyDiagramRenderer(),
      DiagramType.Chord => new Renderers.ChordDiagramRenderer(),
      DiagramType.Arc => new Renderers.ArcDiagramRenderer(),
      DiagramType.Network => new Renderers.NetworkDiagramRenderer(),
      DiagramType.Tree => new Renderers.TreeDiagramRenderer(),
      DiagramType.Dendrogram => new Renderers.DendrogramDiagramRenderer(),
      DiagramType.CirclePacking => new Renderers.CirclePackingDiagramRenderer(),
      DiagramType.FlowChart => new Renderers.FlowChartDiagramRenderer(),

      // Hierarchical/Organizational (reuse HierarchyNodes)
      DiagramType.OrgChart => new Renderers.OrgChartDiagramRenderer(),
      DiagramType.MindMap => new Renderers.MindMapDiagramRenderer(),
      DiagramType.WBS => new Renderers.WBSDiagramRenderer(),
      DiagramType.Fishbone => new Renderers.FishboneDiagramRenderer(),
      DiagramType.DecisionTree => new Renderers.DecisionTreeDiagramRenderer(),

      // UML Diagrams
      DiagramType.ClassDiagram => new Renderers.ClassDiagramRenderer(),
      DiagramType.SequenceDiagram => new Renderers.SequenceDiagramRenderer(),
      DiagramType.StateDiagram => new Renderers.StateDiagramRenderer(),
      DiagramType.UseCaseDiagram => new Renderers.UseCaseDiagramRenderer(),
      DiagramType.ActivityDiagram => new Renderers.ActivityDiagramRenderer(),
      DiagramType.ComponentDiagram => new Renderers.ComponentDiagramRenderer(),
      DiagramType.DeploymentDiagram => new Renderers.DeploymentDiagramRenderer(),
      DiagramType.ObjectDiagram => new Renderers.ObjectDiagramRenderer(),
      DiagramType.PackageDiagram => new Renderers.PackageDiagramRenderer(),
      DiagramType.CommunicationDiagram => new Renderers.CommunicationDiagramRenderer(),
      DiagramType.TimingDiagram => new Renderers.TimingDiagramRenderer(),
      DiagramType.InteractionOverview => new Renderers.InteractionOverviewDiagramRenderer(),

      // Database/Data
      DiagramType.EntityRelationship => new Renderers.EntityRelationshipDiagramRenderer(),
      DiagramType.DataFlow => new Renderers.DataFlowDiagramRenderer(),

      // Business/Analytical
      DiagramType.VennDiagram => new Renderers.VennDiagramRenderer(),
      DiagramType.Matrix => new Renderers.MatrixDiagramRenderer(),
      DiagramType.SWOT => new Renderers.SWOTDiagramRenderer(),
      DiagramType.JourneyMap => new Renderers.JourneyMapDiagramRenderer(),
      DiagramType.BPMN => new Renderers.BPMNDiagramRenderer(),
      DiagramType.Kanban => new Renderers.KanbanDiagramRenderer(),

      // Architecture/Software
      DiagramType.C4Context => new Renderers.C4ContextDiagramRenderer(),
      DiagramType.C4Container => new Renderers.C4ContainerDiagramRenderer(),
      DiagramType.C4Component => new Renderers.C4ComponentDiagramRenderer(),
      DiagramType.C4Deployment => new Renderers.C4DeploymentDiagramRenderer(),
      DiagramType.Gitgraph => new Renderers.GitgraphDiagramRenderer(),
      DiagramType.Requirement => new Renderers.RequirementDiagramRenderer(),

      // Technical/Infrastructure
      DiagramType.BlockDiagram => new Renderers.BlockDiagramRenderer(),
      DiagramType.RackDiagram => new Renderers.RackDiagramRenderer(),
      DiagramType.NetworkTopology => new Renderers.NetworkTopologyDiagramRenderer(),
      DiagramType.PacketDiagram => new Renderers.PacketDiagramRenderer(),
      DiagramType.ByteField => new Renderers.ByteFieldDiagramRenderer(),
      DiagramType.Waveform => new Renderers.WaveformDiagramRenderer(),

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

    // Render the diagram with clipping and zoom/pan transform
    var renderer = this._GetRenderer();
    if (renderer != null) {
      var previousClip = context.Graphics.Clip;
      var previousTransform = context.Graphics.Transform;
      context.Graphics.SetClip(this._plotArea);
      try {
        // Apply zoom and pan transform
        if (Math.Abs(this._zoomLevel - 1.0f) > 0.0001f || this._panOffset != PointF.Empty) {
          var transform = new Matrix();
          transform.Translate(this._plotArea.Left + this._panOffset.X, this._plotArea.Top + this._panOffset.Y);
          transform.Scale(this._zoomLevel, this._zoomLevel);
          transform.Translate(-this._plotArea.Left, -this._plotArea.Top);
          context.Graphics.MultiplyTransform(transform);
        }

        renderer.Render(context);
      } finally {
        context.Graphics.Transform = previousTransform;
        context.Graphics.Clip = previousClip;
      }
    }

    // Copy hit test rects (transformed for zoom/pan)
    foreach (var kvp in context.HitTestRects) {
      var rect = kvp.Value;
      if (Math.Abs(this._zoomLevel - 1.0f) > 0.0001f || this._panOffset != PointF.Empty) {
        rect = new RectangleF(
          (rect.X - this._plotArea.Left) * this._zoomLevel + this._plotArea.Left + this._panOffset.X,
          (rect.Y - this._plotArea.Top) * this._zoomLevel + this._plotArea.Top + this._panOffset.Y,
          rect.Width * this._zoomLevel,
          rect.Height * this._zoomLevel
        );
      }
      this._hitTestRects[kvp.Key] = rect;
    }

    // Draw legend
    if (this._showLegend && this._legendPosition != DiagramLegendPosition.None && this._legendItems.Count > 0)
      this._DrawLegend(context.Graphics, context.TotalBounds, this._legendItems);

    // Draw zoom indicator
    if (this._showZoomIndicator)
      this._DrawZoomIndicator(context.Graphics);
  }

  private void _DrawZoomIndicator(Graphics g) {
    var zoomText = $"{this._zoomLevel * 100:F0}%";
    var textSize = g.MeasureString(zoomText, this.Font);
    var padding = 5f;
    var sliderWidth = this._showZoomSlider ? 100f : 0f;
    var buttonSize = 18f;
    var totalWidth = textSize.Width + padding * 4 + sliderWidth + (this._showZoomSlider ? buttonSize * 2 + padding * 2 : 0);
    var indicatorHeight = Math.Max(textSize.Height, buttonSize) + padding * 2;

    float x, y;
    switch (this._zoomIndicatorPosition) {
      case DiagramCornerPosition.TopLeft:
        x = this._plotArea.Left + padding;
        y = this._plotArea.Top + padding;
        break;
      case DiagramCornerPosition.TopRight:
        x = this._plotArea.Right - totalWidth - padding;
        y = this._plotArea.Top + padding;
        break;
      case DiagramCornerPosition.BottomLeft:
        x = this._plotArea.Left + padding;
        y = this._plotArea.Bottom - indicatorHeight - padding;
        break;
      case DiagramCornerPosition.BottomRight:
      default:
        x = this._plotArea.Right - totalWidth - padding;
        y = this._plotArea.Bottom - indicatorHeight - padding;
        break;
    }

    var indicatorRect = new RectangleF(x, y, totalWidth, indicatorHeight);

    // Draw background
    using (var bgBrush = new SolidBrush(Color.FromArgb(230, 250, 250, 250)))
      g.FillRectangle(bgBrush, indicatorRect);

    using (var borderPen = new Pen(Color.FromArgb(180, 180, 180)))
      g.DrawRectangle(borderPen, indicatorRect.X, indicatorRect.Y, indicatorRect.Width, indicatorRect.Height);

    var currentX = x + padding;
    var centerY = y + indicatorHeight / 2;

    if (this._showZoomSlider) {
      // Draw minus button
      var minusRect = new RectangleF(currentX, centerY - buttonSize / 2, buttonSize, buttonSize);
      this._DrawZoomButton(g, minusRect, "-", Color.FromArgb(220, 220, 220));
      currentX += buttonSize + padding;

      // Draw slider track
      var trackHeight = 6f;
      var trackRect = new RectangleF(currentX, centerY - trackHeight / 2, sliderWidth, trackHeight);
      this._zoomSliderTrackRect = trackRect;

      using (var trackBrush = new SolidBrush(Color.FromArgb(200, 200, 200)))
        g.FillRectangle(trackBrush, trackRect);

      // Draw slider thumb position based on zoom level (logarithmic scale)
      var normalizedZoom = (float)((Math.Log(this._zoomLevel) - Math.Log(this._minZoom)) / (Math.Log(this._maxZoom) - Math.Log(this._minZoom)));
      normalizedZoom = Math.Max(0, Math.Min(1, normalizedZoom));
      var thumbX = trackRect.Left + normalizedZoom * (trackRect.Width - 12);
      var thumbRect = new RectangleF(thumbX, centerY - 8, 12, 16);

      using (var thumbBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
        g.FillRectangle(thumbBrush, thumbRect);

      // Draw 100% marker
      var hundredPctPos = (float)((Math.Log(1.0) - Math.Log(this._minZoom)) / (Math.Log(this._maxZoom) - Math.Log(this._minZoom)));
      var markerX = trackRect.Left + hundredPctPos * trackRect.Width;
      using (var markerPen = new Pen(Color.FromArgb(100, 100, 100), 1))
        g.DrawLine(markerPen, markerX, trackRect.Top - 2, markerX, trackRect.Bottom + 2);

      currentX += sliderWidth + padding;

      // Draw plus button
      var plusRect = new RectangleF(currentX, centerY - buttonSize / 2, buttonSize, buttonSize);
      this._DrawZoomButton(g, plusRect, "+", Color.FromArgb(220, 220, 220));
      currentX += buttonSize + padding;
    }

    // Draw zoom text (clickable to edit)
    this._zoomTextRect = new RectangleF(currentX, y + padding, textSize.Width + padding * 2, indicatorHeight - padding * 2);

    using (var textBgBrush = new SolidBrush(Color.White))
      g.FillRectangle(textBgBrush, this._zoomTextRect);

    using (var textBorderPen = new Pen(Color.FromArgb(180, 180, 180)))
      g.DrawRectangle(textBorderPen, this._zoomTextRect.X, this._zoomTextRect.Y, this._zoomTextRect.Width, this._zoomTextRect.Height);

    using var textBrush = new SolidBrush(Color.Black);
    g.DrawString(zoomText, this.Font, textBrush, currentX + padding, centerY - textSize.Height / 2);
  }

  private void _DrawZoomButton(Graphics g, RectangleF rect, string symbol, Color bgColor) {
    using (var brush = new SolidBrush(bgColor))
      g.FillRectangle(brush, rect);

    using (var pen = new Pen(Color.FromArgb(150, 150, 150)))
      g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

    using var font = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold);
    var textSize = g.MeasureString(symbol, font);
    using var brush2 = new SolidBrush(Color.FromArgb(60, 60, 60));
    g.DrawString(symbol, font, brush2, rect.X + (rect.Width - textSize.Width) / 2, rect.Y + (rect.Height - textSize.Height) / 2);
  }

  private void _HandleZoomIndicatorClick(Point location) {
    if (!this._showZoomIndicator)
      return;

    // Check text area click for manual input
    if (this._zoomTextRect.Contains(location)) {
      this._ShowZoomInputBox();
      return;
    }

    if (!this._showZoomSlider)
      return;

    // Check slider track click
    if (this._zoomSliderTrackRect.Contains(location)) {
      this._SetZoomFromSliderPosition(location.X);
      return;
    }

    // Check minus button
    var padding = 5f;
    var buttonSize = 18f;
    var baseX = this._zoomSliderTrackRect.Left - buttonSize - padding;
    var baseY = this._zoomSliderTrackRect.Top + this._zoomSliderTrackRect.Height / 2 - buttonSize / 2;
    var minusRect = new RectangleF(baseX, baseY, buttonSize, buttonSize);
    if (minusRect.Contains(location)) {
      this.ZoomLevel = this._zoomLevel / 1.2f;
      return;
    }

    // Check plus button
    var plusX = this._zoomSliderTrackRect.Right + padding;
    var plusRect = new RectangleF(plusX, baseY, buttonSize, buttonSize);
    if (plusRect.Contains(location))
      this.ZoomLevel = this._zoomLevel * 1.2f;
  }

  private void _SetZoomFromSliderPosition(float mouseX) {
    var normalized = (mouseX - this._zoomSliderTrackRect.Left) / this._zoomSliderTrackRect.Width;
    normalized = Math.Max(0, Math.Min(1, normalized));
    // Logarithmic scale
    var logZoom = Math.Log(this._minZoom) + normalized * (Math.Log(this._maxZoom) - Math.Log(this._minZoom));
    this.ZoomLevel = (float)Math.Exp(logZoom);
  }

  private void _ShowZoomInputBox() {
    if (this._zoomInputBox != null) {
      this._zoomInputBox.Focus();
      return;
    }

    // Ensure minimum height for text input (at least 20 pixels)
    var boxHeight = Math.Max(20, (int)this._zoomTextRect.Height);
    var boxWidth = Math.Max(50, (int)this._zoomTextRect.Width);

    this._zoomInputBox = new TextBox {
      Text = $"{this._zoomLevel * 100:F0}",
      Location = new Point((int)this._zoomTextRect.X, (int)this._zoomTextRect.Y - (boxHeight - (int)this._zoomTextRect.Height) / 2),
      Size = new Size(boxWidth, boxHeight),
      TextAlign = HorizontalAlignment.Center,
      BorderStyle = BorderStyle.FixedSingle,
      Font = this.Font
    };

    this._zoomInputBox.KeyDown += (s, e) => {
      if (e.KeyCode == Keys.Enter) {
        this._ApplyZoomInputAndClose();
        e.Handled = true;
        e.SuppressKeyPress = true;
      } else if (e.KeyCode == Keys.Escape)
        this._CloseZoomInputBox();
    };

    this._zoomInputBox.LostFocus += (s, e) => this._ApplyZoomInputAndClose();

    this.Controls.Add(this._zoomInputBox);
    this._zoomInputBox.BringToFront();
    this._zoomInputBox.Focus();
    this._zoomInputBox.SelectAll();
  }

  private void _ApplyZoomInputAndClose() {
    if (this._zoomInputBox == null)
      return;

    if (float.TryParse(this._zoomInputBox.Text.Replace("%", "").Trim(), out var pct))
      this.ZoomLevel = pct / 100f;

    this._CloseZoomInputBox();
  }

  private void _CloseZoomInputBox() {
    var inputBox = this._zoomInputBox;
    if (inputBox == null)
      return;

    // Set to null first to prevent re-entrancy from LostFocus during Dispose
    this._zoomInputBox = null;
    this.Controls.Remove(inputBox);
    inputBox.Dispose();
    this.Invalidate();
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
        // Always fire the generic element event
        this.OnElementClicked(new DiagramElementEventArgs(kvp.Key, kvp.Value));

        // Also fire specific events for known types
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
          default:
            return; // Generic element event was already fired
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

/// <summary>Event arguments for any diagram element events.</summary>
public class DiagramElementEventArgs : EventArgs {
  /// <summary>Gets the element that was clicked or hovered.</summary>
  public object Element { get; }

  /// <summary>Gets the bounds of the element.</summary>
  public RectangleF Bounds { get; }

  /// <summary>Gets the element type name.</summary>
  public string ElementTypeName => this.Element?.GetType().Name ?? "Unknown";

  public DiagramElementEventArgs(object element, RectangleF bounds) {
    this.Element = element;
    this.Bounds = bounds;
  }
}

#endregion
