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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms;

/// <summary>
/// A collapsible panel with header (similar to WPF Expander).
/// </summary>
/// <example>
/// <code>
/// var expander = new ExpanderControl {
///   HeaderText = "Details",
///   IsExpanded = true
/// };
/// expander.ContentPanel.Controls.Add(new Label { Text = "Content here" });
/// </code>
/// </example>
public class ExpanderControl : ContainerControl {
  private string _headerText = "Header";
  private Image _headerIcon;
  private bool _isExpanded = true;
  private int _collapsedHeight = 30;
  private int _expandedHeight = 200;
  private bool _animateExpansion = true;
  private readonly Panel _contentPanel;
  private Timer _animationTimer;
  private int _targetHeight;
  private const int AnimationStep = 20;

  /// <summary>
  /// Occurs when the control is expanded.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the control is expanded.")]
  public event EventHandler Expanded;

  /// <summary>
  /// Occurs when the control is collapsed.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the control is collapsed.")]
  public event EventHandler Collapsed;

  /// <summary>
  /// Occurs when the control is about to expand.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the control is about to expand.")]
  public event EventHandler<CancelEventArgs> Expanding;

  /// <summary>
  /// Occurs when the control is about to collapse.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the control is about to collapse.")]
  public event EventHandler<CancelEventArgs> Collapsing;

  /// <summary>
  /// Initializes a new instance of the <see cref="ExpanderControl"/> class.
  /// </summary>
  public ExpanderControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this._contentPanel = new Panel {
      Location = new Point(0, this._collapsedHeight),
      Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
      BackColor = SystemColors.Control
    };

    this.Controls.Add(this._contentPanel);

    this._animationTimer = new Timer { Interval = 16 };
    this._animationTimer.Tick += this._OnAnimationTick;

    this.Size = new Size(200, this._expandedHeight);
    this._UpdateContentPanelSize();
  }

  /// <summary>
  /// Gets or sets the header text.
  /// </summary>
  [Category("Appearance")]
  [Description("The header text.")]
  [DefaultValue("Header")]
  public string HeaderText {
    get => this._headerText;
    set {
      this._headerText = value ?? "Header";
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the header icon.
  /// </summary>
  [Category("Appearance")]
  [Description("The icon displayed in the header.")]
  public Image HeaderIcon {
    get => this._headerIcon;
    set {
      this._headerIcon = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether the control is expanded.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the control is expanded.")]
  [DefaultValue(true)]
  public bool IsExpanded {
    get => this._isExpanded;
    set {
      if (this._isExpanded == value)
        return;

      if (value) {
        var args = new CancelEventArgs();
        this.OnExpanding(args);
        if (args.Cancel)
          return;
      } else {
        var args = new CancelEventArgs();
        this.OnCollapsing(args);
        if (args.Cancel)
          return;
      }

      this._isExpanded = value;
      this._targetHeight = value ? this._expandedHeight : this._collapsedHeight;

      if (this._animateExpansion)
        this._animationTimer.Start();
      else {
        this.Height = this._targetHeight;
        this._UpdateContentPanelSize();
        if (value)
          this.OnExpanded(EventArgs.Empty);
        else
          this.OnCollapsed(EventArgs.Empty);
      }

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the collapsed height.
  /// </summary>
  [Category("Layout")]
  [Description("The height when collapsed.")]
  [DefaultValue(30)]
  public int CollapsedHeight {
    get => this._collapsedHeight;
    set {
      value = Math.Max(20, value);
      if (this._collapsedHeight == value)
        return;
      this._collapsedHeight = value;
      if (!this._isExpanded)
        this.Height = value;
      this._UpdateContentPanelSize();
    }
  }

  /// <summary>
  /// Gets or sets the expanded height.
  /// </summary>
  [Category("Layout")]
  [Description("The height when expanded.")]
  [DefaultValue(200)]
  public int ExpandedHeight {
    get => this._expandedHeight;
    set {
      value = Math.Max(this._collapsedHeight + 20, value);
      if (this._expandedHeight == value)
        return;
      this._expandedHeight = value;
      if (this._isExpanded)
        this.Height = value;
      this._UpdateContentPanelSize();
    }
  }

  /// <summary>
  /// Gets or sets whether to animate expansion.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to animate the expansion/collapse.")]
  [DefaultValue(true)]
  public bool AnimateExpansion {
    get => this._animateExpansion;
    set => this._animateExpansion = value;
  }

  /// <summary>
  /// Gets the content panel.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public Panel ContentPanel => this._contentPanel;

  /// <summary>
  /// Toggles the expanded state.
  /// </summary>
  public void Toggle() => this.IsExpanded = !this.IsExpanded;

  /// <summary>
  /// Raises the <see cref="Expanded"/> event.
  /// </summary>
  protected virtual void OnExpanded(EventArgs e) => this.Expanded?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="Collapsed"/> event.
  /// </summary>
  protected virtual void OnCollapsed(EventArgs e) => this.Collapsed?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="Expanding"/> event.
  /// </summary>
  protected virtual void OnExpanding(CancelEventArgs e) => this.Expanding?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="Collapsing"/> event.
  /// </summary>
  protected virtual void OnCollapsing(CancelEventArgs e) => this.Collapsing?.Invoke(this, e);

  private void _OnAnimationTick(object sender, EventArgs e) {
    var diff = this._targetHeight - this.Height;
    if (Math.Abs(diff) <= AnimationStep) {
      this.Height = this._targetHeight;
      this._animationTimer.Stop();
      this._UpdateContentPanelSize();

      if (this._isExpanded)
        this.OnExpanded(EventArgs.Empty);
      else
        this.OnCollapsed(EventArgs.Empty);
    } else {
      this.Height += diff > 0 ? AnimationStep : -AnimationStep;
      this._UpdateContentPanelSize();
    }
  }

  private void _UpdateContentPanelSize() {
    this._contentPanel.Location = new Point(0, this._collapsedHeight);
    this._contentPanel.Size = new Size(this.Width, Math.Max(0, this.Height - this._collapsedHeight));
    this._contentPanel.Visible = this.Height > this._collapsedHeight;
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var headerRect = new Rectangle(0, 0, this.Width, this._collapsedHeight);

    // Draw header background
    using (var brush = new LinearGradientBrush(headerRect, SystemColors.ControlLight, SystemColors.Control, 90f)) {
      g.FillRectangle(brush, headerRect);
    }

    // Draw header border
    using (var pen = new Pen(SystemColors.ControlDark)) {
      g.DrawRectangle(pen, 0, 0, this.Width - 1, this._collapsedHeight - 1);
    }

    // Draw expand/collapse arrow
    var arrowX = 10;
    var arrowY = this._collapsedHeight / 2;
    var arrowPoints = this._isExpanded
      ? new[] { new Point(arrowX, arrowY - 4), new Point(arrowX + 8, arrowY - 4), new Point(arrowX + 4, arrowY + 4) }
      : new[] { new Point(arrowX, arrowY - 4), new Point(arrowX + 8, arrowY), new Point(arrowX, arrowY + 4) };

    using (var brush = new SolidBrush(this.ForeColor)) {
      g.FillPolygon(brush, arrowPoints);
    }

    // Draw header icon
    var textX = 24;
    if (this._headerIcon != null) {
      var iconY = (this._collapsedHeight - 16) / 2;
      g.DrawImage(this._headerIcon, textX, iconY, 16, 16);
      textX += 20;
    }

    // Draw header text
    var textRect = new Rectangle(textX, 0, this.Width - textX - 8, this._collapsedHeight);
    TextRenderer.DrawText(g, this._headerText, this.Font, textRect, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

    // Draw content border when expanded
    if (this._isExpanded || this.Height > this._collapsedHeight)
      using (var pen = new Pen(SystemColors.ControlDark)) {
        g.DrawRectangle(pen, 0, this._collapsedHeight, this.Width - 1, this.Height - this._collapsedHeight - 1);
      }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);
    if (e.Y < this._collapsedHeight)
      this.Toggle();
  }

  /// <inheritdoc />
  protected override void OnResize(EventArgs e) {
    base.OnResize(e);
    this._UpdateContentPanelSize();
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._animationTimer?.Stop();
      this._animationTimer?.Dispose();
      this._animationTimer = null;
    }

    base.Dispose(disposing);
  }
}
