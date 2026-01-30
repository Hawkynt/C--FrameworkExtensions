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
/// A label control that can display a notification badge.
/// </summary>
/// <example>
/// <code>
/// var badge = new BadgeLabel {
///   Text = "Notifications",
///   BadgeValue = 5,
///   BadgeColor = Color.Red
/// };
/// </code>
/// </example>
public class BadgeLabel : Control {
  private Image _icon;
  private int _badgeValue;
  private Color _badgeColor = Color.Red;
  private Color _badgeTextColor = Color.White;
  private ContentAlignment _badgePosition = ContentAlignment.TopRight;
  private int _maxBadgeValue = 99;
  private bool _hideWhenZero = true;
  private int _badgeSize = 18;

  /// <summary>
  /// Initializes a new instance of the <see cref="BadgeLabel"/> class.
  /// </summary>
  public BadgeLabel() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor,
      true
    );

    this.Size = new Size(80, 40);
  }

  /// <summary>
  /// Gets or sets the icon image.
  /// </summary>
  [Category("Appearance")]
  [Description("The icon image to display.")]
  [DefaultValue(null)]
  public Image Icon {
    get => this._icon;
    set {
      this._icon = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the badge value (notification count).
  /// </summary>
  [Category("Appearance")]
  [Description("The badge notification count.")]
  [DefaultValue(0)]
  public int BadgeValue {
    get => this._badgeValue;
    set {
      if (this._badgeValue == value)
        return;
      this._badgeValue = Math.Max(0, value);
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the badge background color.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color of the badge.")]
  public Color BadgeColor {
    get => this._badgeColor;
    set {
      if (this._badgeColor == value)
        return;
      this._badgeColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the badge text color.
  /// </summary>
  [Category("Appearance")]
  [Description("The text color of the badge.")]
  public Color BadgeTextColor {
    get => this._badgeTextColor;
    set {
      if (this._badgeTextColor == value)
        return;
      this._badgeTextColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the position of the badge.
  /// </summary>
  [Category("Appearance")]
  [Description("The position of the badge overlay.")]
  [DefaultValue(ContentAlignment.TopRight)]
  public ContentAlignment BadgePosition {
    get => this._badgePosition;
    set {
      if (this._badgePosition == value)
        return;
      this._badgePosition = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the maximum badge value before showing "+".
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum value shown before displaying '+'.")]
  [DefaultValue(99)]
  public int MaxBadgeValue {
    get => this._maxBadgeValue;
    set {
      if (this._maxBadgeValue == value)
        return;
      this._maxBadgeValue = Math.Max(1, value);
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to hide the badge when the value is zero.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to hide the badge when the value is zero.")]
  [DefaultValue(true)]
  public bool HideWhenZero {
    get => this._hideWhenZero;
    set {
      if (this._hideWhenZero == value)
        return;
      this._hideWhenZero = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the size of the badge circle.
  /// </summary>
  [Category("Appearance")]
  [Description("The size of the badge circle in pixels.")]
  [DefaultValue(18)]
  public int BadgeSize {
    get => this._badgeSize;
    set {
      if (this._badgeSize == value)
        return;
      this._badgeSize = Math.Max(12, value);
      this.Invalidate();
    }
  }

  private bool ShouldSerializeBadgeColor() => this._badgeColor != Color.Red;
  private void ResetBadgeColor() => this._badgeColor = Color.Red;
  private bool ShouldSerializeBadgeTextColor() => this._badgeTextColor != Color.White;
  private void ResetBadgeTextColor() => this._badgeTextColor = Color.White;

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var iconSize = this._icon?.Size ?? Size.Empty;
    var textSize = TextRenderer.MeasureText(this.Text, this.Font);

    // Calculate content area (icon + text)
    var contentWidth = iconSize.Width + (iconSize.Width > 0 && !string.IsNullOrEmpty(this.Text) ? 4 : 0) + textSize.Width;
    var contentHeight = Math.Max(iconSize.Height, textSize.Height);
    var contentX = (bounds.Width - contentWidth) / 2;
    var contentY = (bounds.Height - contentHeight) / 2;

    // Content bounds for badge positioning
    var contentBounds = new Rectangle(contentX, contentY, contentWidth, contentHeight);

    // Draw icon
    if (this._icon != null) {
      var iconY = contentY + (contentHeight - iconSize.Height) / 2;
      g.DrawImage(this._icon, contentX, iconY);
      contentX += iconSize.Width + 4;
    }

    // Draw text
    if (!string.IsNullOrEmpty(this.Text)) {
      var textY = contentY + (contentHeight - textSize.Height) / 2;
      TextRenderer.DrawText(g, this.Text, this.Font, new Point(contentX, textY), this.ForeColor);
    }

    // Draw badge relative to content area
    if (this._badgeValue > 0 || !this._hideWhenZero)
      this._DrawBadge(g, contentBounds);
  }

  private void _DrawBadge(Graphics g, Rectangle bounds) {
    var badgeText = this._badgeValue > this._maxBadgeValue
      ? $"{this._maxBadgeValue}+"
      : this._badgeValue.ToString();

    var badgeTextSize = TextRenderer.MeasureText(badgeText, this.Font);
    var badgeWidth = Math.Max(this._badgeSize, badgeTextSize.Width + 8);
    var badgeHeight = this._badgeSize;

    var badgeRect = this._GetBadgeRectangle(bounds, badgeWidth, badgeHeight);

    // Draw badge background
    using (var brush = new SolidBrush(this._badgeColor)) {
      if (badgeWidth == badgeHeight)
        g.FillEllipse(brush, badgeRect);
      else {
        using var path = _CreateRoundedRectanglePath(badgeRect, badgeHeight / 2f);
        g.FillPath(brush, path);
      }
    }

    // Draw badge text
    var textFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
    TextRenderer.DrawText(g, badgeText, this.Font, Rectangle.Round(badgeRect), this._badgeTextColor, textFlags);
  }

  private RectangleF _GetBadgeRectangle(Rectangle bounds, int width, int height) {
    float x, y;

    // Offset to make badge overlap content corner (typical badge behavior)
    var overlapX = width / 3f;
    var overlapY = height / 3f;

    switch (this._badgePosition) {
      case ContentAlignment.TopLeft:
        x = bounds.X - overlapX;
        y = bounds.Y - overlapY;
        break;
      case ContentAlignment.TopCenter:
        x = bounds.X + (bounds.Width - width) / 2f;
        y = bounds.Y - overlapY;
        break;
      case ContentAlignment.TopRight:
        x = bounds.X + bounds.Width - width + overlapX;
        y = bounds.Y - overlapY;
        break;
      case ContentAlignment.MiddleLeft:
        x = bounds.X - overlapX;
        y = bounds.Y + (bounds.Height - height) / 2f;
        break;
      case ContentAlignment.MiddleCenter:
        x = bounds.X + (bounds.Width - width) / 2f;
        y = bounds.Y + (bounds.Height - height) / 2f;
        break;
      case ContentAlignment.MiddleRight:
        x = bounds.X + bounds.Width - width + overlapX;
        y = bounds.Y + (bounds.Height - height) / 2f;
        break;
      case ContentAlignment.BottomLeft:
        x = bounds.X - overlapX;
        y = bounds.Y + bounds.Height - height + overlapY;
        break;
      case ContentAlignment.BottomCenter:
        x = bounds.X + (bounds.Width - width) / 2f;
        y = bounds.Y + bounds.Height - height + overlapY;
        break;
      case ContentAlignment.BottomRight:
      default:
        x = bounds.X + bounds.Width - width + overlapX;
        y = bounds.Y + bounds.Height - height + overlapY;
        break;
    }

    return new RectangleF(x, y, width, height);
  }

  private static GraphicsPath _CreateRoundedRectanglePath(RectangleF bounds, float radius) {
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
