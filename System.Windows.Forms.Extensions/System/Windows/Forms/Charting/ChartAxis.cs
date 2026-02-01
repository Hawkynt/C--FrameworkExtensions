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

namespace System.Windows.Forms.Charting;

/// <summary>
/// Represents a chart axis with configurable properties.
/// </summary>
public class ChartAxis {
  private readonly AdvancedChart _owner;
  private readonly ChartAxisType _axisType;
  private readonly bool _isXAxis;

  internal ChartAxis(AdvancedChart owner, ChartAxisType axisType, bool isXAxis) {
    this._owner = owner;
    this._axisType = axisType;
    this._isXAxis = isXAxis;
  }

  /// <summary>Gets the axis type (primary or secondary).</summary>
  public ChartAxisType AxisType => this._axisType;

  /// <summary>Gets whether this is an X-axis.</summary>
  public bool IsXAxis => this._isXAxis;

  /// <summary>Gets or sets the axis title.</summary>
  [Category("Appearance")]
  [Description("The title displayed along the axis.")]
  public string Title {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the font used for the title.</summary>
  [Category("Appearance")]
  [Description("The font used for the axis title.")]
  public Font TitleFont {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the color of the title.</summary>
  [Category("Appearance")]
  [Description("The color of the axis title.")]
  public Color TitleColor {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = Color.Black;

  /// <summary>Gets or sets the font used for labels.</summary>
  [Category("Appearance")]
  [Description("The font used for axis labels.")]
  public Font LabelFont {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the color of the labels.</summary>
  [Category("Appearance")]
  [Description("The color of axis labels.")]
  public Color LabelColor {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = Color.Black;

  /// <summary>Gets or sets the color of the axis line.</summary>
  [Category("Appearance")]
  [Description("The color of the axis line.")]
  public Color LineColor {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = Color.Black;

  /// <summary>Gets or sets the width of the axis line.</summary>
  [Category("Appearance")]
  [Description("The width of the axis line.")]
  [DefaultValue(1)]
  public int LineWidth {
    get;
    set {
      field = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  } = 1;

  /// <summary>Gets or sets whether the axis is visible.</summary>
  [Category("Behavior")]
  [Description("Whether the axis is visible.")]
  [DefaultValue(true)]
  public bool Visible {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = true;

  /// <summary>Gets or sets whether to show grid lines.</summary>
  [Category("Appearance")]
  [Description("Whether to show grid lines for this axis.")]
  [DefaultValue(true)]
  public bool ShowGrid {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = true;

  /// <summary>Gets or sets the color of grid lines.</summary>
  [Category("Appearance")]
  [Description("The color of grid lines.")]
  public Color GridColor {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = Color.LightGray;

  /// <summary>Gets or sets the style of grid lines.</summary>
  [Category("Appearance")]
  [Description("The style of grid lines.")]
  [DefaultValue(ChartGridLineStyle.Dashed)]
  public ChartGridLineStyle GridLineStyle {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = ChartGridLineStyle.Dashed;

  /// <summary>Gets or sets the scale type.</summary>
  [Category("Behavior")]
  [Description("The type of scale for this axis.")]
  [DefaultValue(ChartScaleType.Linear)]
  public ChartScaleType ScaleType {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = ChartScaleType.Linear;

  /// <summary>Gets or sets the minimum value.</summary>
  [Category("Behavior")]
  [Description("The minimum value for the axis. Use NaN for auto-scaling.")]
  public double Minimum {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = double.NaN;

  /// <summary>Gets or sets the maximum value.</summary>
  [Category("Behavior")]
  [Description("The maximum value for the axis. Use NaN for auto-scaling.")]
  public double Maximum {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = double.NaN;

  /// <summary>Gets or sets the interval between major ticks.</summary>
  [Category("Behavior")]
  [Description("The interval between major tick marks. Null for auto.")]
  public double? Interval {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the format string for labels.</summary>
  [Category("Appearance")]
  [Description("The format string for axis labels.")]
  [DefaultValue("N1")]
  public string LabelFormat {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = "N1";

  /// <summary>Gets or sets the position of axis labels.</summary>
  [Category("Appearance")]
  [Description("The position of axis labels.")]
  [DefaultValue(ChartAxisLabelPosition.Outside)]
  public ChartAxisLabelPosition LabelPosition {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = ChartAxisLabelPosition.Outside;

  /// <summary>Gets or sets the angle of labels in degrees.</summary>
  [Category("Appearance")]
  [Description("The rotation angle of labels in degrees.")]
  [DefaultValue(0)]
  public int LabelAngle {
    get;
    set {
      field = value % 360;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether the axis is inverted.</summary>
  [Category("Behavior")]
  [Description("Whether the axis direction is inverted.")]
  [DefaultValue(false)]
  public bool Inverted {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets where this axis crosses the perpendicular axis.</summary>
  [Category("Behavior")]
  [Description("The value where this axis crosses the perpendicular axis.")]
  public double CrossingValue {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = double.NaN;

  /// <summary>Gets or sets whether to show minor tick marks.</summary>
  [Category("Appearance")]
  [Description("Whether to show minor tick marks.")]
  [DefaultValue(false)]
  public bool ShowMinorTicks {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets the number of minor ticks between major ticks.</summary>
  [Category("Appearance")]
  [Description("The number of minor ticks between major ticks.")]
  [DefaultValue(4)]
  public int MinorTickCount {
    get;
    set {
      field = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  } = 4;

  /// <summary>Gets or sets the length of major tick marks in pixels.</summary>
  [Category("Appearance")]
  [Description("The length of major tick marks in pixels.")]
  [DefaultValue(6)]
  public int MajorTickLength {
    get;
    set {
      field = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  } = 6;

  /// <summary>Gets or sets the length of minor tick marks in pixels.</summary>
  [Category("Appearance")]
  [Description("The length of minor tick marks in pixels.")]
  [DefaultValue(3)]
  public int MinorTickLength {
    get;
    set {
      field = Math.Max(0, value);
      this._owner?.Invalidate();
    }
  } = 3;

  /// <summary>Gets or sets the category labels for category axes.</summary>
  [Category("Data")]
  [Description("The category labels for category scale axes.")]
  public string[] Categories {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  }

  /// <summary>Gets or sets whether to use base 10 for logarithmic scale.</summary>
  [Category("Behavior")]
  [Description("Whether to use base 10 (true) or base e (false) for logarithmic scale.")]
  [DefaultValue(true)]
  public bool LogarithmicBase10 {
    get;
    set {
      field = value;
      this._owner?.Invalidate();
    }
  } = true;

  /// <summary>Gets whether auto-scaling is enabled for minimum.</summary>
  public bool AutoMinimum => double.IsNaN(this.Minimum);

  /// <summary>Gets whether auto-scaling is enabled for maximum.</summary>
  public bool AutoMaximum => double.IsNaN(this.Maximum);

  /// <summary>
  /// Gets the effective font for labels.
  /// </summary>
  internal Font GetEffectiveLabelFont() => this.LabelFont ?? this._owner?.Font ?? SystemFonts.DefaultFont;

  /// <summary>
  /// Gets the effective font for the title.
  /// </summary>
  internal Font GetEffectiveTitleFont() {
    if (this.TitleFont != null)
      return this.TitleFont;

    var baseFont = this._owner?.Font ?? SystemFonts.DefaultFont;
    return new Font(baseFont.FontFamily, baseFont.Size + 2, FontStyle.Bold);
  }

  /// <summary>
  /// Converts a value to axis position (0-1 range).
  /// </summary>
  public double ValueToPosition(double value, double min, double max) {
    if (Math.Abs(max - min) < double.Epsilon)
      return 0.5;

    double position;

    if (this.ScaleType == ChartScaleType.Logarithmic) {
      if (value <= 0 || min <= 0)
        return 0;

      var logBase = this.LogarithmicBase10 ? 10 : Math.E;
      var logMin = Math.Log(min) / Math.Log(logBase);
      var logMax = Math.Log(max) / Math.Log(logBase);
      var logValue = Math.Log(value) / Math.Log(logBase);

      position = (logValue - logMin) / (logMax - logMin);
    } else
      position = (value - min) / (max - min);

    return this.Inverted ? 1 - position : position;
  }

  /// <summary>
  /// Converts an axis position (0-1 range) to a value.
  /// </summary>
  public double PositionToValue(double position, double min, double max) {
    if (this.Inverted)
      position = 1 - position;

    if (this.ScaleType == ChartScaleType.Logarithmic) {
      if (min <= 0)
        return min;

      var logBase = this.LogarithmicBase10 ? 10 : Math.E;
      var logMin = Math.Log(min) / Math.Log(logBase);
      var logMax = Math.Log(max) / Math.Log(logBase);
      var logValue = logMin + position * (logMax - logMin);

      return Math.Pow(logBase, logValue);
    }

    return min + position * (max - min);
  }

  /// <summary>
  /// Calculates nice axis bounds for a given data range.
  /// </summary>
  public static (double min, double max, double interval) CalculateNiceBounds(double dataMin, double dataMax, int targetTickCount = 5) {
    if (dataMin >= dataMax) {
      dataMax = dataMin + 1;
      if (dataMin > 0)
        dataMin = 0;
    }

    var range = NiceNumber(dataMax - dataMin, false);
    var interval = NiceNumber(range / (targetTickCount - 1), true);
    var min = Math.Floor(dataMin / interval) * interval;
    var max = Math.Ceiling(dataMax / interval) * interval;

    return (min, max, interval);
  }

  private static double NiceNumber(double range, bool round) {
    var exponent = Math.Floor(Math.Log10(range));
    var fraction = range / Math.Pow(10, exponent);

    double niceFraction;
    if (round) {
      if (fraction < 1.5)
        niceFraction = 1;
      else if (fraction < 3)
        niceFraction = 2;
      else if (fraction < 7)
        niceFraction = 5;
      else
        niceFraction = 10;
    } else {
      if (fraction <= 1)
        niceFraction = 1;
      else if (fraction <= 2)
        niceFraction = 2;
      else if (fraction <= 5)
        niceFraction = 5;
      else
        niceFraction = 10;
    }

    return niceFraction * Math.Pow(10, exponent);
  }

  /// <summary>
  /// Formats a value according to the axis settings.
  /// </summary>
  public string FormatValue(double value) {
    if (this.ScaleType == ChartScaleType.Category && this.Categories != null) {
      var index = (int)Math.Round(value);
      if (index >= 0 && index < this.Categories.Length)
        return this.Categories[index];
    }

    if (this.ScaleType == ChartScaleType.DateTime) {
      try {
        var date = DateTime.FromOADate(value);
        return date.ToString(string.IsNullOrEmpty(this.LabelFormat) ? "d" : this.LabelFormat);
      } catch {
        return value.ToString(this.LabelFormat ?? "N1");
      }
    }

    try {
      return value.ToString(this.LabelFormat ?? "N1");
    } catch {
      return value.ToString();
    }
  }
}
