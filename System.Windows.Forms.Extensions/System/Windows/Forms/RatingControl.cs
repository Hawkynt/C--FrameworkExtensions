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
/// A star rating control for displaying and inputting ratings.
/// </summary>
/// <example>
/// <code>
/// var rating = new RatingControl {
///   MaxRating = 5,
///   Value = 3,
///   AllowHalfStars = true
/// };
/// rating.ValueChanged += (s, e) => Console.WriteLine($"Rating: {rating.Value}");
/// </code>
/// </example>
public class RatingControl : Control {
  private int _value;
  private int _maxRating = 5;
  private bool _allowHalfStars;
  private bool _readOnly;
  private int _imageSize = 24;
  private int _spacing = 2;
  private float _hoverValue = -1;

  private Color _filledColor = Color.Gold;
  private Color _emptyColor = Color.LightGray;

  /// <summary>
  /// Occurs when the <see cref="Value"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the Value property changes.")]
  public event EventHandler ValueChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="RatingControl"/> class.
  /// </summary>
  public RatingControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor
      | ControlStyles.Selectable,
      true
    );

    this._UpdateSize();
  }

  /// <summary>
  /// Gets or sets the current rating value.
  /// </summary>
  [Category("Appearance")]
  [Description("The current rating value.")]
  [DefaultValue(0)]
  public int Value {
    get => this._value;
    set {
      value = Math.Max(0, Math.Min(value, this._maxRating));
      if (this._value == value)
        return;

      this._value = value;
      this.Invalidate();
      this.OnValueChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the maximum rating value.
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum rating value.")]
  [DefaultValue(5)]
  public int MaxRating {
    get => this._maxRating;
    set {
      value = Math.Max(1, value);
      if (this._maxRating == value)
        return;

      this._maxRating = value;
      if (this._value > value)
        this._value = value;

      this._UpdateSize();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether half-star ratings are allowed.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether half-star ratings are allowed.")]
  [DefaultValue(false)]
  public bool AllowHalfStars {
    get => this._allowHalfStars;
    set {
      if (this._allowHalfStars == value)
        return;
      this._allowHalfStars = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether the control is read-only.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the control is read-only.")]
  [DefaultValue(false)]
  public bool ReadOnly {
    get => this._readOnly;
    set {
      if (this._readOnly == value)
        return;
      this._readOnly = value;
      this.Cursor = value ? Cursors.Default : Cursors.Hand;
    }
  }

  /// <summary>
  /// Gets or sets the size of each star image.
  /// </summary>
  [Category("Appearance")]
  [Description("The size of each star in pixels.")]
  [DefaultValue(24)]
  public int ImageSize {
    get => this._imageSize;
    set {
      value = Math.Max(8, value);
      if (this._imageSize == value)
        return;

      this._imageSize = value;
      this._UpdateSize();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the spacing between stars.
  /// </summary>
  [Category("Appearance")]
  [Description("The spacing between stars in pixels.")]
  [DefaultValue(2)]
  public int Spacing {
    get => this._spacing;
    set {
      value = Math.Max(0, value);
      if (this._spacing == value)
        return;

      this._spacing = value;
      this._UpdateSize();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of filled stars.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of filled stars.")]
  public Color FilledColor {
    get => this._filledColor;
    set {
      if (this._filledColor == value)
        return;
      this._filledColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of empty stars.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of empty stars.")]
  public Color EmptyColor {
    get => this._emptyColor;
    set {
      if (this._emptyColor == value)
        return;
      this._emptyColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeFilledColor() => this._filledColor != Color.Gold;
  private void ResetFilledColor() => this._filledColor = Color.Gold;
  private bool ShouldSerializeEmptyColor() => this._emptyColor != Color.LightGray;
  private void ResetEmptyColor() => this._emptyColor = Color.LightGray;

  /// <summary>
  /// Raises the <see cref="ValueChanged"/> event.
  /// </summary>
  protected virtual void OnValueChanged(EventArgs e) => this.ValueChanged?.Invoke(this, e);

  private void _UpdateSize() {
    var width = this._maxRating * this._imageSize + (this._maxRating - 1) * this._spacing;
    this.Size = new Size(width, this._imageSize);
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var displayValue = this._hoverValue >= 0 ? this._hoverValue : this._value;

    for (var i = 0; i < this._maxRating; ++i) {
      var x = i * (this._imageSize + this._spacing);
      var rect = new Rectangle(x, 0, this._imageSize, this._imageSize);
      var fillAmount = Math.Max(0f, Math.Min(1f, displayValue - i));

      this._DrawStar(g, rect, fillAmount);
    }
  }

  private void _DrawStar(Graphics g, Rectangle bounds, float fillAmount) {
    var starPath = this._CreateStarPath(bounds);

    // Draw empty star background
    using (var emptyBrush = new SolidBrush(this._emptyColor)) {
      g.FillPath(emptyBrush, starPath);
    }

    // Draw filled portion
    if (fillAmount > 0) {
      var clipBounds = bounds;
      clipBounds.Width = (int)(clipBounds.Width * fillAmount);

      var oldClip = g.Clip;
      g.SetClip(clipBounds);

      using (var filledBrush = new SolidBrush(this._filledColor)) {
        g.FillPath(filledBrush, starPath);
      }

      g.Clip = oldClip;
    }

    starPath.Dispose();
  }

  private GraphicsPath _CreateStarPath(Rectangle bounds) {
    var path = new GraphicsPath();
    var cx = bounds.X + bounds.Width / 2f;
    var cy = bounds.Y + bounds.Height / 2f;
    var outerRadius = bounds.Width / 2f - 1;
    var innerRadius = outerRadius * 0.4f;
    const int points = 5;

    var starPoints = new PointF[points * 2];
    for (var i = 0; i < points * 2; ++i) {
      var radius = i % 2 == 0 ? outerRadius : innerRadius;
      var angle = Math.PI / 2 + i * Math.PI / points;
      starPoints[i] = new PointF(
        cx + (float)(radius * Math.Cos(angle)),
        cy - (float)(radius * Math.Sin(angle))
      );
    }

    path.AddPolygon(starPoints);
    return path;
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);
    if (this._readOnly)
      return;

    this._hoverValue = this._GetValueFromPoint(e.Location);
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    this._hoverValue = -1;
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);
    if (this._readOnly || e.Button != MouseButtons.Left)
      return;

    var newValue = this._GetValueFromPoint(e.Location);
    this.Value = (int)Math.Ceiling(newValue);
  }

  private float _GetValueFromPoint(Point point) {
    if (point.X < 0)
      return 0;

    var starWidth = this._imageSize + this._spacing;
    var starIndex = point.X / starWidth;
    var positionInStar = (point.X % starWidth) / (float)this._imageSize;

    if (positionInStar > 1)
      positionInStar = 1;

    var value = starIndex + positionInStar;

    if (!this._allowHalfStars)
      return (float)Math.Ceiling(value);

    // Round to nearest half
    return (float)Math.Ceiling(value * 2) / 2;
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);
    if (this._readOnly)
      return;

    switch (e.KeyCode) {
      case Keys.Left:
      case Keys.Down:
        this.Value = Math.Max(0, this._value - 1);
        e.Handled = true;
        break;
      case Keys.Right:
      case Keys.Up:
        this.Value = Math.Min(this._maxRating, this._value + 1);
        e.Handled = true;
        break;
      case Keys.Home:
        this.Value = 0;
        e.Handled = true;
        break;
      case Keys.End:
        this.Value = this._maxRating;
        e.Handled = true;
        break;
    }
  }
}
