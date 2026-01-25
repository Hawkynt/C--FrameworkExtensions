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

namespace System.Windows.Forms;

/// <summary>
/// Represents a color zone on a gauge.
/// </summary>
public class GaugeZone {
  /// <summary>
  /// Gets or sets the start value of this zone.
  /// </summary>
  public double Start { get; set; }

  /// <summary>
  /// Gets or sets the end value of this zone.
  /// </summary>
  public double End { get; set; }

  /// <summary>
  /// Gets or sets the color of this zone.
  /// </summary>
  public Color Color { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="GaugeZone"/> class.
  /// </summary>
  public GaugeZone() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="GaugeZone"/> class with values.
  /// </summary>
  public GaugeZone(double start, double end, Color color) {
    this.Start = start;
    this.End = end;
    this.Color = color;
  }
}

/// <summary>
/// A speedometer/dial gauge control.
/// </summary>
/// <example>
/// <code>
/// var gauge = new Gauge {
///   Value = 50,
///   Maximum = 100,
///   Zones = new[] {
///     new GaugeZone(0, 30, Color.Green),
///     new GaugeZone(30, 70, Color.Yellow),
///     new GaugeZone(70, 100, Color.Red)
///   }
/// };
/// </code>
/// </example>
public class Gauge : Control {
  private double _value;
  private double _minimum;
  private double _maximum = 100;
  private double _startAngle = 225;
  private double _sweepAngle = 270;
  private GaugeZone[] _zones;
  private bool _showTicks = true;
  private int _majorTickCount = 5;
  private int _minorTickCount = 4;
  private bool _showValue = true;
  private string _valueFormat = "{0:0}";
  private string _unit = string.Empty;
  private Color _needleColor = Color.Red;
  private Color _dialColor = Color.WhiteSmoke;

  /// <summary>
  /// Occurs when the <see cref="Value"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the Value property changes.")]
  public event EventHandler ValueChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="Gauge"/> class.
  /// </summary>
  public Gauge() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor,
      true
    );

    this.Size = new Size(150, 150);
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
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the starting angle of the gauge arc in degrees.
  /// </summary>
  [Category("Appearance")]
  [Description("The starting angle of the gauge arc in degrees (0 = right, 90 = down).")]
  [DefaultValue(225d)]
  public double StartAngle {
    get => this._startAngle;
    set {
      if (Math.Abs(this._startAngle - value) < double.Epsilon)
        return;
      this._startAngle = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the sweep angle of the gauge arc in degrees.
  /// </summary>
  [Category("Appearance")]
  [Description("The sweep angle of the gauge arc in degrees.")]
  [DefaultValue(270d)]
  public double SweepAngle {
    get => this._sweepAngle;
    set {
      if (Math.Abs(this._sweepAngle - value) < double.Epsilon)
        return;
      this._sweepAngle = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color zones on the gauge.
  /// </summary>
  [Category("Appearance")]
  [Description("The color zones on the gauge.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public GaugeZone[] Zones {
    get => this._zones;
    set {
      this._zones = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show tick marks.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show tick marks.")]
  [DefaultValue(true)]
  public bool ShowTicks {
    get => this._showTicks;
    set {
      if (this._showTicks == value)
        return;
      this._showTicks = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the number of major tick marks.
  /// </summary>
  [Category("Appearance")]
  [Description("The number of major tick marks.")]
  [DefaultValue(5)]
  public int MajorTickCount {
    get => this._majorTickCount;
    set {
      value = Math.Max(2, value);
      if (this._majorTickCount == value)
        return;
      this._majorTickCount = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the number of minor ticks between major ticks.
  /// </summary>
  [Category("Appearance")]
  [Description("The number of minor ticks between major ticks.")]
  [DefaultValue(4)]
  public int MinorTickCount {
    get => this._minorTickCount;
    set {
      value = Math.Max(0, value);
      if (this._minorTickCount == value)
        return;
      this._minorTickCount = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the value text.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the value text.")]
  [DefaultValue(true)]
  public bool ShowValue {
    get => this._showValue;
    set {
      if (this._showValue == value)
        return;
      this._showValue = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the format string for the value display.
  /// </summary>
  [Category("Appearance")]
  [Description("The format string for the value display.")]
  [DefaultValue("{0:0}")]
  public string ValueFormat {
    get => this._valueFormat;
    set {
      if (this._valueFormat == value)
        return;
      this._valueFormat = value ?? "{0:0}";
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the unit label displayed below the value.
  /// </summary>
  [Category("Appearance")]
  [Description("The unit label displayed below the value.")]
  [DefaultValue("")]
  public string Unit {
    get => this._unit;
    set {
      if (this._unit == value)
        return;
      this._unit = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the needle.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the needle.")]
  public Color NeedleColor {
    get => this._needleColor;
    set {
      if (this._needleColor == value)
        return;
      this._needleColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the dial face.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the dial face.")]
  public Color DialColor {
    get => this._dialColor;
    set {
      if (this._dialColor == value)
        return;
      this._dialColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeNeedleColor() => this._needleColor != Color.Red;
  private void ResetNeedleColor() => this._needleColor = Color.Red;
  private bool ShouldSerializeDialColor() => this._dialColor != Color.WhiteSmoke;
  private void ResetDialColor() => this._dialColor = Color.WhiteSmoke;

  /// <summary>
  /// Raises the <see cref="ValueChanged"/> event.
  /// </summary>
  protected virtual void OnValueChanged(EventArgs e) => this.ValueChanged?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var size = Math.Min(bounds.Width, bounds.Height);
    var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
    var radius = size / 2f - 10;

    // Draw dial face
    using (var brush = new SolidBrush(this._dialColor)) {
      g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
    }

    // Draw zones
    if (this._zones != null)
      foreach (var zone in this._zones)
        this._DrawZone(g, center, radius - 5, zone);

    // Draw ticks
    if (this._showTicks)
      this._DrawTicks(g, center, radius);

    // Draw needle
    this._DrawNeedle(g, center, radius - 15);

    // Draw center cap
    using (var brush = new SolidBrush(Color.DarkGray)) {
      g.FillEllipse(brush, center.X - 8, center.Y - 8, 16, 16);
    }

    // Draw value text
    if (!this._showValue)
      return;

    var valueText = string.Format(this._valueFormat, this._value);
    var valueSize = TextRenderer.MeasureText(valueText, this.Font);
    var valueY = center.Y + radius * 0.3f;

    TextRenderer.DrawText(g, valueText, this.Font,
      new Rectangle((int)(center.X - valueSize.Width / 2), (int)valueY, valueSize.Width, valueSize.Height),
      this.ForeColor);

    if (string.IsNullOrEmpty(this._unit))
      return;

    using var smallFont = new Font(this.Font.FontFamily, this.Font.Size * 0.7f);
    var unitSize = TextRenderer.MeasureText(this._unit, smallFont);
    TextRenderer.DrawText(g, this._unit, smallFont,
      new Rectangle((int)(center.X - unitSize.Width / 2), (int)(valueY + valueSize.Height), unitSize.Width, unitSize.Height),
      this.ForeColor);
  }

  private void _DrawZone(Graphics g, PointF center, float radius, GaugeZone zone) {
    var range = this._maximum - this._minimum;
    if (range <= 0)
      return;

    var startPct = (zone.Start - this._minimum) / range;
    var endPct = (zone.End - this._minimum) / range;

    var startAng = this._startAngle + startPct * this._sweepAngle;
    var endAng = this._startAngle + endPct * this._sweepAngle;

    using var pen = new Pen(zone.Color, 8);
    var rect = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
    g.DrawArc(pen, rect, (float)startAng, (float)(endAng - startAng));
  }

  private void _DrawTicks(Graphics g, PointF center, float radius) {
    var totalTicks = (this._majorTickCount - 1) * (this._minorTickCount + 1) + 1;
    var angleStep = this._sweepAngle / (totalTicks - 1);

    for (var i = 0; i < totalTicks; ++i) {
      var isMajor = i % (this._minorTickCount + 1) == 0;
      var angle = this._startAngle + i * angleStep;
      var radAngle = angle * Math.PI / 180;

      var innerRadius = radius - (isMajor ? 15 : 8);
      var outerRadius = radius - 2;

      var innerX = center.X + (float)(innerRadius * Math.Cos(radAngle));
      var innerY = center.Y + (float)(innerRadius * Math.Sin(radAngle));
      var outerX = center.X + (float)(outerRadius * Math.Cos(radAngle));
      var outerY = center.Y + (float)(outerRadius * Math.Sin(radAngle));

      using var pen = new Pen(Color.DarkGray, isMajor ? 2 : 1);
      g.DrawLine(pen, innerX, innerY, outerX, outerY);

      if (!isMajor)
        continue;

      // Draw label
      var tickIndex = i / (this._minorTickCount + 1);
      var tickValue = this._minimum + (this._maximum - this._minimum) * tickIndex / (this._majorTickCount - 1);
      var labelRadius = radius - 25;
      var labelX = center.X + (float)(labelRadius * Math.Cos(radAngle));
      var labelY = center.Y + (float)(labelRadius * Math.Sin(radAngle));

      var label = tickValue.ToString("0");
      var labelSize = TextRenderer.MeasureText(label, this.Font);
      TextRenderer.DrawText(g, label, this.Font,
        new Rectangle((int)(labelX - labelSize.Width / 2), (int)(labelY - labelSize.Height / 2), labelSize.Width, labelSize.Height),
        this.ForeColor);
    }
  }

  private void _DrawNeedle(Graphics g, PointF center, float length) {
    var range = this._maximum - this._minimum;
    var pct = range > 0 ? (this._value - this._minimum) / range : 0;
    var angle = this._startAngle + pct * this._sweepAngle;
    var radAngle = angle * Math.PI / 180;

    var tipX = center.X + (float)(length * Math.Cos(radAngle));
    var tipY = center.Y + (float)(length * Math.Sin(radAngle));
    var tailX = center.X - (float)(10 * Math.Cos(radAngle));
    var tailY = center.Y - (float)(10 * Math.Sin(radAngle));

    // Draw needle shadow
    using (var shadowPen = new Pen(Color.FromArgb(50, 0, 0, 0), 4)) {
      g.DrawLine(shadowPen, tailX + 2, tailY + 2, tipX + 2, tipY + 2);
    }

    // Draw needle
    using (var needlePen = new Pen(this._needleColor, 3)) {
      needlePen.StartCap = LineCap.Round;
      needlePen.EndCap = LineCap.ArrowAnchor;
      g.DrawLine(needlePen, tailX, tailY, tipX, tipY);
    }
  }
}
