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
  // Existing types (8)
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
  FlowChart,

  // Hierarchical/Organizational (3)
  /// <summary>Organizational hierarchy chart with role boxes.</summary>
  OrgChart,
  /// <summary>Mind map with radial tree from center outward.</summary>
  MindMap,
  /// <summary>Work breakdown structure diagram.</summary>
  WBS,

  // UML Diagrams (12)
  /// <summary>UML class diagram with fields/methods compartments.</summary>
  ClassDiagram,
  /// <summary>UML sequence diagram with lifelines and messages.</summary>
  SequenceDiagram,
  /// <summary>UML state diagram with states and transitions.</summary>
  StateDiagram,
  /// <summary>UML use case diagram with actors and use cases.</summary>
  UseCaseDiagram,
  /// <summary>UML activity diagram with swimlanes, forks, joins.</summary>
  ActivityDiagram,
  /// <summary>UML component diagram with interfaces and dependencies.</summary>
  ComponentDiagram,
  /// <summary>UML deployment diagram with nodes and artifacts.</summary>
  DeploymentDiagram,
  /// <summary>UML object diagram showing object instances.</summary>
  ObjectDiagram,
  /// <summary>UML package diagram with dependencies.</summary>
  PackageDiagram,
  /// <summary>UML communication diagram with numbered messages.</summary>
  CommunicationDiagram,
  /// <summary>UML timing diagram showing state changes over time.</summary>
  TimingDiagram,
  /// <summary>UML interaction overview diagram.</summary>
  InteractionOverview,

  // Database/Data (2)
  /// <summary>Entity-relationship diagram with crow's foot notation.</summary>
  EntityRelationship,
  /// <summary>Data flow diagram with processes and stores.</summary>
  DataFlow,

  // Analytical/Business (8)
  /// <summary>Venn diagram showing set relationships.</summary>
  VennDiagram,
  /// <summary>Fishbone/Ishikawa cause-effect diagram.</summary>
  Fishbone,
  /// <summary>Decision tree with probability outcomes.</summary>
  DecisionTree,
  /// <summary>Matrix/quadrant diagram.</summary>
  Matrix,
  /// <summary>SWOT analysis 4-quadrant diagram.</summary>
  SWOT,
  /// <summary>User journey map with stages and emotions.</summary>
  JourneyMap,
  /// <summary>BPMN business process diagram.</summary>
  BPMN,
  /// <summary>Kanban board with columns and cards.</summary>
  Kanban,

  // Architecture/Software (6)
  /// <summary>C4 system context diagram.</summary>
  C4Context,
  /// <summary>C4 container diagram.</summary>
  C4Container,
  /// <summary>C4 component diagram.</summary>
  C4Component,
  /// <summary>C4 deployment diagram.</summary>
  C4Deployment,
  /// <summary>Git branch/commit visualization.</summary>
  Gitgraph,
  /// <summary>Requirements diagram with relationships.</summary>
  Requirement,

  // Technical/Infrastructure (6)
  /// <summary>Simple block diagram with labeled blocks.</summary>
  BlockDiagram,
  /// <summary>Server rack diagram with U positions.</summary>
  RackDiagram,
  /// <summary>Network topology diagram (star, ring, mesh, bus).</summary>
  NetworkTopology,
  /// <summary>Network packet structure diagram.</summary>
  PacketDiagram,
  /// <summary>Binary data structure visualization.</summary>
  ByteField,
  /// <summary>Digital timing/waveform diagram.</summary>
  Waveform
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
/// Specifies a corner position for diagram overlays.
/// </summary>
public enum DiagramCornerPosition {
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight
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
  // Basic shapes
  Rectangle,
  RoundedRectangle,
  Circle,
  Ellipse,
  Diamond,
  Triangle,
  Hexagon,
  Custom,

  // UML State shapes
  InitialState,
  FinalState,
  ShallowHistory,
  DeepHistory,
  Choice,
  Junction,
  EntryPoint,
  ExitPoint,

  // Activity diagram shapes
  Action,
  Decision,
  Fork,
  Join,
  MergeNode,
  AcceptEvent,
  SendSignal,
  TimeEvent,

  // Network/Infrastructure shapes
  Router,
  Switch,
  Hub,
  Firewall,
  Server,
  Workstation,
  Cloud,
  Database,
  LoadBalancer,

  // BPMN shapes
  Task,
  Gateway,
  Event,
  Pool,

  // Use Case shapes
  Actor,
  UseCase,
  SystemBoundary,

  // Component shapes
  Component,
  Interface,
  Port,

  // Data shapes
  DataStore,
  Document,
  ManualInput,
  Parallelogram,
  Cylinder
}

#region UML Enums

/// <summary>
/// Specifies the visibility of UML class members.
/// </summary>
public enum DiagramVisibility {
  /// <summary>Public visibility (+).</summary>
  Public,
  /// <summary>Private visibility (-).</summary>
  Private,
  /// <summary>Protected visibility (#).</summary>
  Protected,
  /// <summary>Internal/package visibility (~).</summary>
  Internal,
  /// <summary>Package visibility (~).</summary>
  Package
}

/// <summary>
/// Specifies the type of UML relationship.
/// </summary>
public enum DiagramRelationType {
  /// <summary>Inheritance/generalization (solid line, hollow triangle).</summary>
  Inheritance,
  /// <summary>Interface implementation (dashed line, hollow triangle).</summary>
  Implementation,
  /// <summary>Association (solid line).</summary>
  Association,
  /// <summary>Aggregation (solid line, hollow diamond).</summary>
  Aggregation,
  /// <summary>Composition (solid line, filled diamond).</summary>
  Composition,
  /// <summary>Dependency (dashed line, open arrow).</summary>
  Dependency,
  /// <summary>Realization (dashed line, hollow triangle).</summary>
  Realization,
  /// <summary>Usage dependency (dashed line with &lt;&lt;use&gt;&gt;).</summary>
  Usage
}

/// <summary>
/// Specifies the type of UML sequence diagram message.
/// </summary>
public enum DiagramMessageType {
  /// <summary>Synchronous message (solid line, filled arrow).</summary>
  Sync,
  /// <summary>Asynchronous message (solid line, open arrow).</summary>
  Async,
  /// <summary>Return message (dashed line, open arrow).</summary>
  Return,
  /// <summary>Create message (dashed line, open arrow, &lt;&lt;create&gt;&gt;).</summary>
  Create,
  /// <summary>Destroy message (X at end).</summary>
  Destroy,
  /// <summary>Self-call (message to same lifeline).</summary>
  SelfCall,
  /// <summary>Found message (from outside the diagram).</summary>
  Found,
  /// <summary>Lost message (to outside the diagram).</summary>
  Lost
}

/// <summary>
/// Specifies the type of BPMN/UML gateway.
/// </summary>
public enum DiagramGatewayType {
  /// <summary>Exclusive gateway (XOR) - only one path taken.</summary>
  Exclusive,
  /// <summary>Inclusive gateway (OR) - one or more paths taken.</summary>
  Inclusive,
  /// <summary>Parallel gateway (AND) - all paths taken.</summary>
  Parallel,
  /// <summary>Event-based gateway - path determined by event.</summary>
  EventBased,
  /// <summary>Complex gateway - custom routing logic.</summary>
  Complex
}

#endregion

#region Database Enums

/// <summary>
/// Specifies the cardinality in ER relationships.
/// </summary>
public enum DiagramCardinality {
  /// <summary>Exactly one (|).</summary>
  One,
  /// <summary>Zero or one (o|).</summary>
  ZeroOrOne,
  /// <summary>Many (crow's foot).</summary>
  Many,
  /// <summary>One or more (|&lt;).</summary>
  OneOrMore,
  /// <summary>Zero or more (o&lt;).</summary>
  ZeroOrMore
}

/// <summary>
/// Specifies the type of data flow diagram element.
/// </summary>
public enum DiagramDFDType {
  /// <summary>Process (circle or rounded rectangle).</summary>
  Process,
  /// <summary>Data store (open-ended rectangle).</summary>
  DataStore,
  /// <summary>External entity (rectangle).</summary>
  ExternalEntity,
  /// <summary>Data flow (arrow).</summary>
  DataFlow
}

#endregion

#region Business Enums

/// <summary>
/// Specifies the type of BPMN element.
/// </summary>
public enum DiagramBPMNType {
  /// <summary>Task/activity.</summary>
  Task,
  /// <summary>Gateway (decision point).</summary>
  Gateway,
  /// <summary>Start event.</summary>
  StartEvent,
  /// <summary>End event.</summary>
  EndEvent,
  /// <summary>Intermediate event.</summary>
  IntermediateEvent,
  /// <summary>Pool (participant).</summary>
  Pool,
  /// <summary>Lane (role within pool).</summary>
  Lane,
  /// <summary>Sub-process.</summary>
  SubProcess
}

#endregion

#region Architecture Enums

/// <summary>
/// Specifies the type of git commit.
/// </summary>
public enum DiagramGitCommitType {
  /// <summary>Normal commit.</summary>
  Normal,
  /// <summary>Merge commit.</summary>
  Merge,
  /// <summary>Cherry-pick commit.</summary>
  CherryPick,
  /// <summary>Revert commit.</summary>
  Revert,
  /// <summary>Tag reference.</summary>
  Tag
}

/// <summary>
/// Specifies the type of requirement.
/// </summary>
public enum DiagramRequirementType {
  /// <summary>Generic requirement.</summary>
  Requirement,
  /// <summary>Functional requirement.</summary>
  FunctionalReq,
  /// <summary>Performance requirement.</summary>
  PerformanceReq,
  /// <summary>Interface requirement.</summary>
  InterfaceReq,
  /// <summary>Design constraint.</summary>
  DesignConstraint
}

/// <summary>
/// Specifies the type of requirement relationship.
/// </summary>
public enum DiagramRequirementRelationType {
  /// <summary>Contains (parent-child).</summary>
  Contains,
  /// <summary>Copies (duplicate).</summary>
  Copies,
  /// <summary>Derives (derived from).</summary>
  Derives,
  /// <summary>Refines (more specific).</summary>
  Refines,
  /// <summary>Traces (traceability link).</summary>
  Traces,
  /// <summary>Satisfies (implementation satisfies requirement).</summary>
  Satisfies,
  /// <summary>Verifies (test verifies requirement).</summary>
  Verifies
}

#endregion

#region Technical Enums

/// <summary>
/// Specifies the network topology layout.
/// </summary>
public enum NetworkLayout {
  /// <summary>Star topology (central hub).</summary>
  Star,
  /// <summary>Ring topology (circular connection).</summary>
  Ring,
  /// <summary>Mesh topology (multiple connections).</summary>
  Mesh,
  /// <summary>Bus topology (linear connection).</summary>
  Bus,
  /// <summary>Tree topology (hierarchical).</summary>
  Tree,
  /// <summary>Hybrid topology (combination).</summary>
  Hybrid,
  /// <summary>Full mesh (every node connected).</summary>
  FullMesh
}

/// <summary>
/// Specifies the type of digital signal.
/// </summary>
public enum DiagramSignalType {
  /// <summary>Digital signal (binary).</summary>
  Digital,
  /// <summary>Bus signal (multi-bit).</summary>
  Bus,
  /// <summary>Analog signal (continuous).</summary>
  Analog,
  /// <summary>Clock signal.</summary>
  Clock
}

/// <summary>
/// Specifies the level of a digital signal.
/// </summary>
public enum DiagramSignalLevel {
  /// <summary>Low (0).</summary>
  Low,
  /// <summary>High (1).</summary>
  High,
  /// <summary>High impedance (Z).</summary>
  HighZ,
  /// <summary>Unknown (X).</summary>
  Unknown,
  /// <summary>Rising edge.</summary>
  Rising,
  /// <summary>Falling edge.</summary>
  Falling,
  /// <summary>Data value.</summary>
  Data,
  /// <summary>Don't care (X).</summary>
  X
}

#endregion
