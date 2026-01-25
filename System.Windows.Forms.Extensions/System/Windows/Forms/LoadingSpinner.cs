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
/// Specifies the visual style of the loading spinner.
/// </summary>
public enum SpinnerStyle {
  /// <summary>Circular spinning dots or segments.</summary>
  Circle,
  /// <summary>Bouncing dots in a row.</summary>
  Dots,
  /// <summary>Bars that pulse in sequence.</summary>
  Bars
}

/// <summary>
/// An animated loading spinner control.
/// </summary>
/// <example>
/// <code>
/// var spinner = new LoadingSpinner {
///   Style = SpinnerStyle.Circle,
///   SpinnerColor = Color.DodgerBlue,
///   IsSpinning = true
/// };
/// </code>
/// </example>
public class LoadingSpinner : Control {
  private bool _isSpinning;
  private SpinnerStyle _style = SpinnerStyle.Circle;
  private Color _spinnerColor = Color.DodgerBlue;
  private int _speed = 100;
  private string _loadingText = string.Empty;
  private Timer _animationTimer;
  private int _animationFrame;
  private const int TotalFrames = 12;

  /// <summary>
  /// Initializes a new instance of the <see cref="LoadingSpinner"/> class.
  /// </summary>
  public LoadingSpinner() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor,
      true
    );

    this.Size = new Size(48, 48);
    this._animationTimer = new Timer { Interval = this._speed };
    this._animationTimer.Tick += this._OnAnimationTick;
  }

  /// <summary>
  /// Gets or sets whether the spinner is currently spinning.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the spinner is currently spinning.")]
  [DefaultValue(false)]
  public bool IsSpinning {
    get => this._isSpinning;
    set {
      if (this._isSpinning == value)
        return;

      this._isSpinning = value;
      if (value)
        this._animationTimer.Start();
      else {
        this._animationTimer.Stop();
        this._animationFrame = 0;
      }

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the visual style of the spinner.
  /// </summary>
  [Category("Appearance")]
  [Description("The visual style of the spinner.")]
  [DefaultValue(SpinnerStyle.Circle)]
  public SpinnerStyle Style {
    get => this._style;
    set {
      if (this._style == value)
        return;
      this._style = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the spinner.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the spinner.")]
  public Color SpinnerColor {
    get => this._spinnerColor;
    set {
      if (this._spinnerColor == value)
        return;
      this._spinnerColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the animation speed in milliseconds per frame.
  /// </summary>
  [Category("Behavior")]
  [Description("The animation speed in milliseconds per frame.")]
  [DefaultValue(100)]
  public int Speed {
    get => this._speed;
    set {
      value = Math.Max(10, Math.Min(1000, value));
      if (this._speed == value)
        return;

      this._speed = value;
      this._animationTimer.Interval = value;
    }
  }

  /// <summary>
  /// Gets or sets the loading text displayed below the spinner.
  /// </summary>
  [Category("Appearance")]
  [Description("The loading text displayed below the spinner.")]
  [DefaultValue("")]
  public string LoadingText {
    get => this._loadingText;
    set {
      if (this._loadingText == value)
        return;
      this._loadingText = value ?? string.Empty;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeSpinnerColor() => this._spinnerColor != Color.DodgerBlue;
  private void ResetSpinnerColor() => this._spinnerColor = Color.DodgerBlue;

  /// <summary>
  /// Starts the spinner animation.
  /// </summary>
  public void Start() => this.IsSpinning = true;

  /// <summary>
  /// Stops the spinner animation.
  /// </summary>
  public void Stop() => this.IsSpinning = false;

  private void _OnAnimationTick(object sender, EventArgs e) {
    this._animationFrame = (this._animationFrame + 1) % TotalFrames;
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var spinnerSize = Math.Min(bounds.Width, bounds.Height);

    if (!string.IsNullOrEmpty(this._loadingText)) {
      var textSize = TextRenderer.MeasureText(this._loadingText, this.Font);
      spinnerSize = Math.Min(spinnerSize, bounds.Height - textSize.Height - 4);
    }

    var spinnerBounds = new Rectangle(
      (bounds.Width - spinnerSize) / 2,
      (bounds.Height - spinnerSize - (string.IsNullOrEmpty(this._loadingText) ? 0 : 20)) / 2,
      spinnerSize,
      spinnerSize
    );

    switch (this._style) {
      case SpinnerStyle.Circle:
        this._DrawCircleSpinner(g, spinnerBounds);
        break;
      case SpinnerStyle.Dots:
        this._DrawDotsSpinner(g, spinnerBounds);
        break;
      case SpinnerStyle.Bars:
        this._DrawBarsSpinner(g, spinnerBounds);
        break;
    }

    if (!string.IsNullOrEmpty(this._loadingText)) {
      var textRect = new Rectangle(0, spinnerBounds.Bottom + 4, bounds.Width, bounds.Height - spinnerBounds.Bottom - 4);
      var textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.SingleLine;
      TextRenderer.DrawText(g, this._loadingText, this.Font, textRect, this.ForeColor, textFlags);
    }
  }

  private void _DrawCircleSpinner(Graphics g, Rectangle bounds) {
    var cx = bounds.X + bounds.Width / 2f;
    var cy = bounds.Y + bounds.Height / 2f;
    var radius = bounds.Width / 2f - 4;
    var dotRadius = bounds.Width / 12f;

    for (var i = 0; i < TotalFrames; ++i) {
      var angle = i * 2 * Math.PI / TotalFrames - Math.PI / 2;
      var x = cx + (float)(radius * Math.Cos(angle)) - dotRadius;
      var y = cy + (float)(radius * Math.Sin(angle)) - dotRadius;

      var alpha = (i + this._animationFrame) % TotalFrames;
      var opacity = (int)(255 * (alpha / (float)TotalFrames));

      using var brush = new SolidBrush(Color.FromArgb(opacity, this._spinnerColor));
      g.FillEllipse(brush, x, y, dotRadius * 2, dotRadius * 2);
    }
  }

  private void _DrawDotsSpinner(Graphics g, Rectangle bounds) {
    const int dotCount = 3;
    var dotSize = bounds.Width / 5f;
    var spacing = (bounds.Width - dotCount * dotSize) / (dotCount + 1);
    var cy = bounds.Y + bounds.Height / 2f;

    for (var i = 0; i < dotCount; ++i) {
      var x = bounds.X + spacing + i * (dotSize + spacing);
      var phase = (this._animationFrame + i * 4) % TotalFrames;
      var scale = 0.5f + 0.5f * (float)Math.Sin(phase * 2 * Math.PI / TotalFrames);
      var currentSize = dotSize * scale;
      var offset = (dotSize - currentSize) / 2;

      using var brush = new SolidBrush(this._spinnerColor);
      g.FillEllipse(brush, x + offset, cy - currentSize / 2, currentSize, currentSize);
    }
  }

  private void _DrawBarsSpinner(Graphics g, Rectangle bounds) {
    const int barCount = 5;
    var barWidth = bounds.Width / (barCount * 2f);
    var maxHeight = bounds.Height * 0.8f;
    var spacing = barWidth;
    var totalWidth = barCount * barWidth + (barCount - 1) * spacing;
    var startX = bounds.X + (bounds.Width - totalWidth) / 2;
    var baseY = bounds.Y + (bounds.Height + maxHeight) / 2;

    for (var i = 0; i < barCount; ++i) {
      var phase = (this._animationFrame + i * 2) % TotalFrames;
      var scale = 0.3f + 0.7f * (float)Math.Sin(phase * Math.PI / TotalFrames);
      var barHeight = maxHeight * scale;
      var x = startX + i * (barWidth + spacing);

      using var brush = new SolidBrush(this._spinnerColor);
      g.FillRectangle(brush, x, baseY - barHeight, barWidth, barHeight);
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
}
