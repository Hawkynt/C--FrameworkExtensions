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
/// A dual-thumb slider for range selection.
/// </summary>
/// <example>
/// <code>
/// var slider = new RangeSlider {
///   Minimum = 0,
///   Maximum = 100,
///   LowerValue = 25,
///   UpperValue = 75
/// };
/// slider.RangeChanged += (s, e) => UpdateRange(slider.LowerValue, slider.UpperValue);
/// </code>
/// </example>
public class RangeSlider : Control {
  private double _minimum;
  private double _maximum = 100;
  private double _lowerValue;
  private double _upperValue = 100;
  private double _smallChange = 1;
  private double _largeChange = 10;
  private bool _snapToTicks;
  private double _tickFrequency = 10;
  private Orientation _orientation = Orientation.Horizontal;
  private Color _trackColor = Color.LightGray;
  private Color _rangeColor = Color.DodgerBlue;
  private Color _thumbColor = Color.White;

  private ThumbType _draggedThumb = ThumbType.None;
  private bool _lowerThumbFocused = true;

  private enum ThumbType { None, Lower, Upper }

  /// <summary>
  /// Occurs when the <see cref="LowerValue"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the LowerValue property changes.")]
  public event EventHandler LowerValueChanged;

  /// <summary>
  /// Occurs when the <see cref="UpperValue"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the UpperValue property changes.")]
  public event EventHandler UpperValueChanged;

  /// <summary>
  /// Occurs when either value changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when either the LowerValue or UpperValue changes.")]
  public event EventHandler RangeChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="RangeSlider"/> class.
  /// </summary>
  public RangeSlider() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.Selectable,
      true
    );

    this.Size = new Size(200, 30);
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
      if (this._lowerValue < value)
        this.LowerValue = value;
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
      if (this._upperValue > value)
        this.UpperValue = value;
      if (this._minimum > value)
        this._minimum = value;

      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the lower value.
  /// </summary>
  [Category("Behavior")]
  [Description("The lower value of the range.")]
  [DefaultValue(0d)]
  public double LowerValue {
    get => this._lowerValue;
    set {
      value = Math.Max(this._minimum, Math.Min(value, this._upperValue));
      if (this._snapToTicks)
        value = this._SnapToTick(value);

      if (Math.Abs(this._lowerValue - value) < double.Epsilon)
        return;

      this._lowerValue = value;
      this.Invalidate();
      this.OnLowerValueChanged(EventArgs.Empty);
      this.OnRangeChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the upper value.
  /// </summary>
  [Category("Behavior")]
  [Description("The upper value of the range.")]
  [DefaultValue(100d)]
  public double UpperValue {
    get => this._upperValue;
    set {
      value = Math.Max(this._lowerValue, Math.Min(value, this._maximum));
      if (this._snapToTicks)
        value = this._SnapToTick(value);

      if (Math.Abs(this._upperValue - value) < double.Epsilon)
        return;

      this._upperValue = value;
      this.Invalidate();
      this.OnUpperValueChanged(EventArgs.Empty);
      this.OnRangeChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the small change value.
  /// </summary>
  [Category("Behavior")]
  [Description("The value to add or subtract when using arrow keys.")]
  [DefaultValue(1d)]
  public double SmallChange {
    get => this._smallChange;
    set => this._smallChange = Math.Max(0, value);
  }

  /// <summary>
  /// Gets or sets the large change value.
  /// </summary>
  [Category("Behavior")]
  [Description("The value to add or subtract when using Page Up/Down.")]
  [DefaultValue(10d)]
  public double LargeChange {
    get => this._largeChange;
    set => this._largeChange = Math.Max(0, value);
  }

  /// <summary>
  /// Gets or sets whether to snap to tick marks.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to snap values to tick marks.")]
  [DefaultValue(false)]
  public bool SnapToTicks {
    get => this._snapToTicks;
    set => this._snapToTicks = value;
  }

  /// <summary>
  /// Gets or sets the tick frequency.
  /// </summary>
  [Category("Appearance")]
  [Description("The interval between tick marks.")]
  [DefaultValue(10d)]
  public double TickFrequency {
    get => this._tickFrequency;
    set {
      this._tickFrequency = Math.Max(0.001, value);
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the orientation.
  /// </summary>
  [Category("Appearance")]
  [Description("The orientation of the slider.")]
  [DefaultValue(Orientation.Horizontal)]
  public Orientation Orientation {
    get => this._orientation;
    set {
      if (this._orientation == value)
        return;
      this._orientation = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the track color.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the track.")]
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
  /// Gets or sets the range highlight color.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the selected range.")]
  public Color RangeColor {
    get => this._rangeColor;
    set {
      if (this._rangeColor == value)
        return;
      this._rangeColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the thumb color.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the thumbs.")]
  public Color ThumbColor {
    get => this._thumbColor;
    set {
      if (this._thumbColor == value)
        return;
      this._thumbColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeTrackColor() => this._trackColor != Color.LightGray;
  private void ResetTrackColor() => this._trackColor = Color.LightGray;
  private bool ShouldSerializeRangeColor() => this._rangeColor != Color.DodgerBlue;
  private void ResetRangeColor() => this._rangeColor = Color.DodgerBlue;
  private bool ShouldSerializeThumbColor() => this._thumbColor != Color.White;
  private void ResetThumbColor() => this._thumbColor = Color.White;

  /// <summary>
  /// Raises the <see cref="LowerValueChanged"/> event.
  /// </summary>
  protected virtual void OnLowerValueChanged(EventArgs e) => this.LowerValueChanged?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="UpperValueChanged"/> event.
  /// </summary>
  protected virtual void OnUpperValueChanged(EventArgs e) => this.UpperValueChanged?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="RangeChanged"/> event.
  /// </summary>
  protected virtual void OnRangeChanged(EventArgs e) => this.RangeChanged?.Invoke(this, e);

  private double _SnapToTick(double value) {
    var tickCount = (value - this._minimum) / this._tickFrequency;
    return this._minimum + Math.Round(tickCount) * this._tickFrequency;
  }

  private double _ValueFromPosition(int pos) {
    var trackLength = this._GetTrackLength();
    var offset = this._GetTrackOffset();
    var pct = Math.Max(0, Math.Min(1, (pos - offset) / (double)trackLength));
    return this._minimum + pct * (this._maximum - this._minimum);
  }

  private int _PositionFromValue(double value) {
    var range = this._maximum - this._minimum;
    if (range <= 0)
      return this._GetTrackOffset();

    var pct = (value - this._minimum) / range;
    return this._GetTrackOffset() + (int)(pct * this._GetTrackLength());
  }

  private int _GetTrackLength() => this._orientation == Orientation.Horizontal
    ? this.ClientSize.Width - 20
    : this.ClientSize.Height - 20;

  private int _GetTrackOffset() => 10;

  private Rectangle _GetThumbRect(ThumbType thumb) {
    var pos = this._PositionFromValue(thumb == ThumbType.Lower ? this._lowerValue : this._upperValue);
    const int thumbSize = 16;

    if (this._orientation == Orientation.Horizontal)
      return new Rectangle(pos - thumbSize / 2, (this.ClientSize.Height - thumbSize) / 2, thumbSize, thumbSize);

    return new Rectangle((this.ClientSize.Width - thumbSize) / 2, pos - thumbSize / 2, thumbSize, thumbSize);
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var isHorizontal = this._orientation == Orientation.Horizontal;
    var trackThickness = 4;
    var trackOffset = this._GetTrackOffset();
    var trackLength = this._GetTrackLength();

    // Track background
    Rectangle trackRect;
    if (isHorizontal)
      trackRect = new Rectangle(trackOffset, (bounds.Height - trackThickness) / 2, trackLength, trackThickness);
    else
      trackRect = new Rectangle((bounds.Width - trackThickness) / 2, trackOffset, trackThickness, trackLength);

    using (var brush = new SolidBrush(this._trackColor)) {
      g.FillRectangle(brush, trackRect);
    }

    // Range highlight
    var lowerPos = this._PositionFromValue(this._lowerValue);
    var upperPos = this._PositionFromValue(this._upperValue);

    Rectangle rangeRect;
    if (isHorizontal)
      rangeRect = new Rectangle(lowerPos, (bounds.Height - trackThickness) / 2, upperPos - lowerPos, trackThickness);
    else
      rangeRect = new Rectangle((bounds.Width - trackThickness) / 2, lowerPos, trackThickness, upperPos - lowerPos);

    using (var brush = new SolidBrush(this._rangeColor)) {
      g.FillRectangle(brush, rangeRect);
    }

    // Draw ticks
    this._DrawTicks(g, isHorizontal);

    // Draw thumbs
    this._DrawThumb(g, ThumbType.Lower);
    this._DrawThumb(g, ThumbType.Upper);
  }

  private void _DrawTicks(Graphics g, bool isHorizontal) {
    if (this._tickFrequency <= 0)
      return;

    using var pen = new Pen(Color.Gray, 1);
    var centerOffset = isHorizontal ? this.ClientSize.Height / 2 + 12 : this.ClientSize.Width / 2 + 12;

    for (var value = this._minimum; value <= this._maximum; value += this._tickFrequency) {
      var pos = this._PositionFromValue(value);
      if (isHorizontal)
        g.DrawLine(pen, pos, centerOffset, pos, centerOffset + 4);
      else
        g.DrawLine(pen, centerOffset, pos, centerOffset + 4, pos);
    }
  }

  private void _DrawThumb(Graphics g, ThumbType thumb) {
    var rect = this._GetThumbRect(thumb);
    var isFocused = this.Focused && ((thumb == ThumbType.Lower && this._lowerThumbFocused) || (thumb == ThumbType.Upper && !this._lowerThumbFocused));

    // Shadow
    using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0))) {
      g.FillEllipse(shadowBrush, rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
    }

    // Thumb body
    using (var brush = new SolidBrush(this._thumbColor)) {
      g.FillEllipse(brush, rect);
    }

    // Border
    using (var pen = new Pen(isFocused ? SystemColors.Highlight : Color.Gray, isFocused ? 2 : 1)) {
      g.DrawEllipse(pen, rect);
    }
  }

  /// <inheritdoc />
  protected override void OnMouseDown(MouseEventArgs e) {
    base.OnMouseDown(e);
    this.Focus();

    var lowerRect = this._GetThumbRect(ThumbType.Lower);
    var upperRect = this._GetThumbRect(ThumbType.Upper);

    if (lowerRect.Contains(e.Location)) {
      this._draggedThumb = ThumbType.Lower;
      this._lowerThumbFocused = true;
    } else if (upperRect.Contains(e.Location)) {
      this._draggedThumb = ThumbType.Upper;
      this._lowerThumbFocused = false;
    } else {
      // Click on track - move nearest thumb
      var pos = this._orientation == Orientation.Horizontal ? e.X : e.Y;
      var value = this._ValueFromPosition(pos);
      var distToLower = Math.Abs(value - this._lowerValue);
      var distToUpper = Math.Abs(value - this._upperValue);

      if (distToLower <= distToUpper) {
        this.LowerValue = value;
        this._lowerThumbFocused = true;
      } else {
        this.UpperValue = value;
        this._lowerThumbFocused = false;
      }
    }

    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (this._draggedThumb == ThumbType.None)
      return;

    var pos = this._orientation == Orientation.Horizontal ? e.X : e.Y;
    var value = this._ValueFromPosition(pos);

    if (this._draggedThumb == ThumbType.Lower)
      this.LowerValue = value;
    else
      this.UpperValue = value;
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);
    this._draggedThumb = ThumbType.None;
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);

    var change = e.Modifiers.HasFlag(Keys.Control) ? this._largeChange : this._smallChange;

    switch (e.KeyCode) {
      case Keys.Left:
      case Keys.Down:
        if (this._lowerThumbFocused)
          this.LowerValue -= change;
        else
          this.UpperValue -= change;
        e.Handled = true;
        break;
      case Keys.Right:
      case Keys.Up:
        if (this._lowerThumbFocused)
          this.LowerValue += change;
        else
          this.UpperValue += change;
        e.Handled = true;
        break;
      case Keys.Tab:
        if (!e.Shift) {
          if (this._lowerThumbFocused) {
            this._lowerThumbFocused = false;
            this.Invalidate();
            e.Handled = true;
          }
        } else {
          if (!this._lowerThumbFocused) {
            this._lowerThumbFocused = true;
            this.Invalidate();
            e.Handled = true;
          }
        }

        break;
      case Keys.Home:
        if (this._lowerThumbFocused)
          this.LowerValue = this._minimum;
        else
          this.UpperValue = this._lowerValue;
        e.Handled = true;
        break;
      case Keys.End:
        if (this._lowerThumbFocused)
          this.LowerValue = this._upperValue;
        else
          this.UpperValue = this._maximum;
        e.Handled = true;
        break;
    }
  }

  /// <inheritdoc />
  protected override void OnGotFocus(EventArgs e) {
    base.OnGotFocus(e);
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnLostFocus(EventArgs e) {
    base.OnLostFocus(e);
    this.Invalidate();
  }
}
