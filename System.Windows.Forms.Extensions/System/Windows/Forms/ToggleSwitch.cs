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
/// An iOS/Android-style toggle switch control.
/// </summary>
/// <example>
/// <code>
/// var toggle = new ToggleSwitch {
///   Checked = true,
///   OnColor = Color.Green,
///   OffColor = Color.Gray
/// };
/// toggle.CheckedChanged += (s, e) => Console.WriteLine($"Toggled: {toggle.Checked}");
/// </code>
/// </example>
public class ToggleSwitch : Control {
  private bool _checked;
  private Color _onColor = Color.DodgerBlue;
  private Color _offColor = Color.LightGray;
  private Color _thumbColor = Color.White;
  private string _onText = "ON";
  private string _offText = "OFF";
  private bool _showText;
  private bool _animateTransition = true;

  private float _animationProgress;
  private Timer _animationTimer;
  private const int AnimationInterval = 16;
  private const float AnimationStep = 0.15f;

  /// <summary>
  /// Occurs when the <see cref="Checked"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the Checked property changes.")]
  public event EventHandler CheckedChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
  /// </summary>
  public ToggleSwitch() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor
      | ControlStyles.Selectable,
      true
    );

    this.Size = new Size(50, 24);
    this._animationProgress = 0f;
    this._animationTimer = new Timer { Interval = AnimationInterval };
    this._animationTimer.Tick += this._OnAnimationTick;
  }

  /// <summary>
  /// Gets or sets whether the switch is in the on (checked) state.
  /// </summary>
  [Category("Appearance")]
  [Description("Indicates whether the switch is on.")]
  [DefaultValue(false)]
  public bool Checked {
    get => this._checked;
    set {
      if (this._checked == value)
        return;

      this._checked = value;

      if (this._animateTransition && !this.DesignMode)
        this._animationTimer.Start();
      else {
        this._animationProgress = value ? 1f : 0f;
        this.Invalidate();
      }

      this.OnCheckedChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the color when the switch is on.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color when the switch is on.")]
  public Color OnColor {
    get => this._onColor;
    set {
      if (this._onColor == value)
        return;
      this._onColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color when the switch is off.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color when the switch is off.")]
  public Color OffColor {
    get => this._offColor;
    set {
      if (this._offColor == value)
        return;
      this._offColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the switch thumb.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the switch thumb.")]
  public Color ThumbColor {
    get => this._thumbColor;
    set {
      if (this._thumbColor == value)
        return;
      this._thumbColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the text displayed when the switch is on.
  /// </summary>
  [Category("Appearance")]
  [Description("The text displayed when the switch is on.")]
  [DefaultValue("ON")]
  public string OnText {
    get => this._onText;
    set {
      if (this._onText == value)
        return;
      this._onText = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the text displayed when the switch is off.
  /// </summary>
  [Category("Appearance")]
  [Description("The text displayed when the switch is off.")]
  [DefaultValue("OFF")]
  public string OffText {
    get => this._offText;
    set {
      if (this._offText == value)
        return;
      this._offText = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show text on the switch.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show text on the switch.")]
  [DefaultValue(false)]
  public bool ShowText {
    get => this._showText;
    set {
      if (this._showText == value)
        return;
      this._showText = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to animate the switch transition.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to animate the switch transition.")]
  [DefaultValue(true)]
  public bool AnimateTransition {
    get => this._animateTransition;
    set => this._animateTransition = value;
  }

  private bool ShouldSerializeOnColor() => this._onColor != Color.DodgerBlue;
  private void ResetOnColor() => this._onColor = Color.DodgerBlue;
  private bool ShouldSerializeOffColor() => this._offColor != Color.LightGray;
  private void ResetOffColor() => this._offColor = Color.LightGray;
  private bool ShouldSerializeThumbColor() => this._thumbColor != Color.White;
  private void ResetThumbColor() => this._thumbColor = Color.White;

  /// <summary>
  /// Raises the <see cref="CheckedChanged"/> event.
  /// </summary>
  protected virtual void OnCheckedChanged(EventArgs e) => this.CheckedChanged?.Invoke(this, e);

  private void _OnAnimationTick(object sender, EventArgs e) {
    var targetProgress = this._checked ? 1f : 0f;
    var diff = targetProgress - this._animationProgress;

    if (Math.Abs(diff) < 0.01f) {
      this._animationProgress = targetProgress;
      this._animationTimer.Stop();
    } else
      this._animationProgress += diff > 0 ? AnimationStep : -AnimationStep;

    this._animationProgress = Math.Max(0f, Math.Min(1f, this._animationProgress));
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var height = bounds.Height;
    var radius = height / 2f;
    var thumbPadding = 2;
    var thumbDiameter = height - thumbPadding * 2;

    // Interpolate background color
    var backColor = _InterpolateColor(this._offColor, this._onColor, this._animationProgress);

    // Draw background track
    using (var backBrush = new SolidBrush(backColor)) {
      using var path = _CreateRoundedRectanglePath(bounds, radius);
      g.FillPath(backBrush, path);
    }

    // Draw thumb
    var thumbX = thumbPadding + (bounds.Width - thumbDiameter - thumbPadding * 2) * this._animationProgress;
    var thumbRect = new RectangleF(thumbX, thumbPadding, thumbDiameter, thumbDiameter);

    using (var thumbBrush = new SolidBrush(this._thumbColor)) {
      g.FillEllipse(thumbBrush, thumbRect);
    }

    // Draw text if enabled
    if (!this._showText)
      return;

    var text = this._animationProgress > 0.5f ? this._onText : this._offText;
    var textColor = this._animationProgress > 0.5f ? this._thumbColor : this._thumbColor;
    var textBounds = bounds;
    textBounds.Inflate(-thumbDiameter - thumbPadding * 2, 0);

    if (this._animationProgress > 0.5f)
      textBounds.X = thumbPadding;
    else
      textBounds.X = (int)thumbRect.Right + thumbPadding;

    var textFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine;
    TextRenderer.DrawText(g, text, this.Font, textBounds, textColor, textFormat);
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);
    if (e.Button == MouseButtons.Left)
      this.Checked = !this.Checked;
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);
    if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
      this.Checked = !this.Checked;
      e.Handled = true;
    }
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

  private static Color _InterpolateColor(Color from, Color to, float progress) {
    var r = (int)(from.R + (to.R - from.R) * progress);
    var g = (int)(from.G + (to.G - from.G) * progress);
    var b = (int)(from.B + (to.B - from.B) * progress);
    var a = (int)(from.A + (to.A - from.A) * progress);
    return Color.FromArgb(a, r, g, b);
  }

  private static GraphicsPath _CreateRoundedRectanglePath(Rectangle bounds, float radius) {
    var path = new GraphicsPath();
    var diameter = radius * 2;
    var arc = new RectangleF(bounds.X, bounds.Y, diameter, diameter);

    path.AddArc(arc, 180, 90);
    arc.X = bounds.Right - diameter;
    path.AddArc(arc, 270, 90);
    arc.Y = bounds.Bottom - diameter;
    path.AddArc(arc, 0, 90);
    arc.X = bounds.Left;
    path.AddArc(arc, 90, 90);
    path.CloseFigure();

    return path;
  }
}
