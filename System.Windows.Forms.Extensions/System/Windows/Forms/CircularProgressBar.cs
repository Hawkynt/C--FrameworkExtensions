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
/// A circular (ring/donut) progress bar control.
/// </summary>
/// <example>
/// <code>
/// var progress = new CircularProgressBar {
///   Value = 75,
///   Maximum = 100,
///   ProgressColor = Color.Green,
///   ShowText = true
/// };
/// </code>
/// </example>
public class CircularProgressBar : Control {
  private double _value;
  private double _minimum;
  private double _maximum = 100;
  private int _thickness = 10;
  private Color _progressColor = Color.DodgerBlue;
  private Color _trackColor = Color.LightGray;
  private bool _showText = true;
  private string _textFormat = "{0:0}%";
  private bool _isIndeterminate;

  private Timer _indeterminateTimer;
  private float _indeterminateAngle;
  private const int IndeterminateSpeed = 16;

  /// <summary>
  /// Occurs when the <see cref="Value"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the Value property changes.")]
  public event EventHandler ValueChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="CircularProgressBar"/> class.
  /// </summary>
  public CircularProgressBar() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor,
      true
    );

    this.Size = new Size(100, 100);
    this._indeterminateTimer = new Timer { Interval = IndeterminateSpeed };
    this._indeterminateTimer.Tick += this._OnIndeterminateTick;
  }

  /// <summary>
  /// Gets or sets the current value.
  /// </summary>
  [Category("Behavior")]
  [Description("The current value.")]
  [DefaultValue(0d)]
  public double Value {
    get => this._value;
    set {
      value = Math.Max(this._minimum, Math.Min(value, this._maximum));
      if (Math.Abs(this._value - value) < double.Epsilon)
        return;

      this._value = value;
      this.Invalidate();
      this.OnValueChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the minimum value.
  /// </summary>
  [Category("Behavior")]
  [Description("The minimum value.")]
  [DefaultValue(0d)]
  public double Minimum {
    get => this._minimum;
    set {
      if (Math.Abs(this._minimum - value) < double.Epsilon)
        return;

      this._minimum = value;
      if (this._value < value)
        this._value = value;
      if (this._maximum < value)
        this._maximum = value;

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the maximum value.
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum value.")]
  [DefaultValue(100d)]
  public double Maximum {
    get => this._maximum;
    set {
      if (Math.Abs(this._maximum - value) < double.Epsilon)
        return;

      this._maximum = value;
      if (this._value > value)
        this._value = value;
      if (this._minimum > value)
        this._minimum = value;

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the thickness of the progress ring.
  /// </summary>
  [Category("Appearance")]
  [Description("The thickness of the progress ring in pixels.")]
  [DefaultValue(10)]
  public int Thickness {
    get => this._thickness;
    set {
      value = Math.Max(2, value);
      if (this._thickness == value)
        return;

      this._thickness = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the progress indicator.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the progress indicator.")]
  public Color ProgressColor {
    get => this._progressColor;
    set {
      if (this._progressColor == value)
        return;
      this._progressColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the background track.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the background track.")]
  public Color TrackColor {
    get => this._trackColor;
    set {
      if (this._trackColor == value)
        return;
      this._trackColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show text in the center.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show text in the center.")]
  [DefaultValue(true)]
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
  /// Gets or sets the format string for the center text.
  /// </summary>
  [Category("Appearance")]
  [Description("The format string for the center text. Use {0} for the percentage.")]
  [DefaultValue("{0:0}%")]
  public string TextFormat {
    get => this._textFormat;
    set {
      if (this._textFormat == value)
        return;
      this._textFormat = value ?? "{0:0}%";
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether the progress bar is in indeterminate mode.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the progress bar is in indeterminate mode.")]
  [DefaultValue(false)]
  public bool IsIndeterminate {
    get => this._isIndeterminate;
    set {
      if (this._isIndeterminate == value)
        return;

      this._isIndeterminate = value;
      if (value)
        this._indeterminateTimer.Start();
      else {
        this._indeterminateTimer.Stop();
        this._indeterminateAngle = 0;
      }

      this.Invalidate();
    }
  }

  private bool ShouldSerializeProgressColor() => this._progressColor != Color.DodgerBlue;
  private void ResetProgressColor() => this._progressColor = Color.DodgerBlue;
  private bool ShouldSerializeTrackColor() => this._trackColor != Color.LightGray;
  private void ResetTrackColor() => this._trackColor = Color.LightGray;

  /// <summary>
  /// Raises the <see cref="ValueChanged"/> event.
  /// </summary>
  protected virtual void OnValueChanged(EventArgs e) => this.ValueChanged?.Invoke(this, e);

  private void _OnIndeterminateTick(object sender, EventArgs e) {
    this._indeterminateAngle = (this._indeterminateAngle + 10) % 360;
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var size = Math.Min(bounds.Width, bounds.Height);
    var ringRect = new Rectangle(
      (bounds.Width - size) / 2 + this._thickness / 2,
      (bounds.Height - size) / 2 + this._thickness / 2,
      size - this._thickness,
      size - this._thickness
    );

    // Draw background track
    using (var trackPen = new Pen(this._trackColor, this._thickness)) {
      trackPen.StartCap = LineCap.Round;
      trackPen.EndCap = LineCap.Round;
      g.DrawEllipse(trackPen, ringRect);
    }

    // Draw progress arc
    using (var progressPen = new Pen(this._progressColor, this._thickness)) {
      progressPen.StartCap = LineCap.Round;
      progressPen.EndCap = LineCap.Round;

      if (this._isIndeterminate) {
        g.DrawArc(progressPen, ringRect, this._indeterminateAngle, 90);
      } else {
        var range = this._maximum - this._minimum;
        var percentage = range > 0 ? (this._value - this._minimum) / range : 0;
        var sweepAngle = (float)(percentage * 360);

        if (sweepAngle > 0)
          g.DrawArc(progressPen, ringRect, -90, sweepAngle);
      }
    }

    // Draw center text
    if (!this._showText || this._isIndeterminate)
      return;

    var range2 = this._maximum - this._minimum;
    var pct = range2 > 0 ? (this._value - this._minimum) / range2 * 100 : 0;
    var text = string.Format(this._textFormat, pct);
    var textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
    TextRenderer.DrawText(g, text, this.Font, bounds, this.ForeColor, textFlags);
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._indeterminateTimer?.Stop();
      this._indeterminateTimer?.Dispose();
      this._indeterminateTimer = null;
    }

    base.Dispose(disposing);
  }
}
