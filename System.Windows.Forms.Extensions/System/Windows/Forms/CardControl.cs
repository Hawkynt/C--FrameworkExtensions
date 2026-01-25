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
/// A Material Design-style card container control.
/// </summary>
/// <example>
/// <code>
/// var card = new CardControl {
///   Title = "User Profile",
///   ShowShadow = true,
///   CornerRadius = 8
/// };
/// card.ContentPanel.Controls.Add(new Label { Text = "Content here" });
/// </code>
/// </example>
public class CardControl : ContainerControl {
  private string _title = string.Empty;
  private Image _titleIcon;
  private bool _showShadow = true;
  private int _shadowDepth = 5;
  private int _cornerRadius = 8;
  private Color _cardColor = Color.White;
  private readonly Panel _contentPanel;
  private readonly Panel _actionPanel;
  private readonly Panel _headerPanel;

  /// <summary>
  /// Initializes a new instance of the <see cref="CardControl"/> class.
  /// </summary>
  public CardControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor,
      true
    );

    this._headerPanel = new Panel {
      Dock = DockStyle.Top,
      Height = 0,
      BackColor = Color.Transparent
    };

    this._contentPanel = new Panel {
      Dock = DockStyle.Fill,
      BackColor = Color.Transparent,
      Padding = new Padding(8)
    };

    this._actionPanel = new Panel {
      Dock = DockStyle.Bottom,
      Height = 0,
      BackColor = Color.Transparent,
      Padding = new Padding(8, 4, 8, 4)
    };

    this.Controls.Add(this._contentPanel);
    this.Controls.Add(this._actionPanel);
    this.Controls.Add(this._headerPanel);

    this.Size = new Size(300, 200);
    this.BackColor = Color.Transparent;
    this.Padding = new Padding(this._shadowDepth);

    this._UpdateLayout();
  }

  /// <summary>
  /// Gets or sets the card title.
  /// </summary>
  [Category("Appearance")]
  [Description("The card title.")]
  [DefaultValue("")]
  public string Title {
    get => this._title;
    set {
      this._title = value ?? string.Empty;
      this._UpdateLayout();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the title icon.
  /// </summary>
  [Category("Appearance")]
  [Description("The icon displayed next to the title.")]
  public Image TitleIcon {
    get => this._titleIcon;
    set {
      this._titleIcon = value;
      this._UpdateLayout();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the shadow.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the shadow.")]
  [DefaultValue(true)]
  public bool ShowShadow {
    get => this._showShadow;
    set {
      if (this._showShadow == value)
        return;
      this._showShadow = value;
      this._UpdateLayout();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the shadow depth.
  /// </summary>
  [Category("Appearance")]
  [Description("The shadow depth in pixels.")]
  [DefaultValue(5)]
  public int ShadowDepth {
    get => this._shadowDepth;
    set {
      value = Math.Max(0, Math.Min(20, value));
      if (this._shadowDepth == value)
        return;
      this._shadowDepth = value;
      this._UpdateLayout();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the corner radius.
  /// </summary>
  [Category("Appearance")]
  [Description("The corner radius.")]
  [DefaultValue(8)]
  public int CornerRadius {
    get => this._cornerRadius;
    set {
      value = Math.Max(0, value);
      if (this._cornerRadius == value)
        return;
      this._cornerRadius = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the card background color.
  /// </summary>
  [Category("Appearance")]
  [Description("The card background color.")]
  public Color CardColor {
    get => this._cardColor;
    set {
      if (this._cardColor == value)
        return;
      this._cardColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets the content panel where main content should be placed.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public Panel ContentPanel => this._contentPanel;

  /// <summary>
  /// Gets the action panel where action buttons should be placed.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public Panel ActionPanel => this._actionPanel;

  private bool ShouldSerializeCardColor() => this._cardColor != Color.White;
  private void ResetCardColor() => this._cardColor = Color.White;

  private void _UpdateLayout() {
    var shadowOffset = this._showShadow ? this._shadowDepth : 0;
    this.Padding = new Padding(shadowOffset, shadowOffset, shadowOffset, shadowOffset);

    var hasTitle = !string.IsNullOrEmpty(this._title) || this._titleIcon != null;
    this._headerPanel.Height = hasTitle ? 40 : 0;

    if (this._actionPanel.Controls.Count > 0)
      this._actionPanel.Height = 44;
    else
      this._actionPanel.Height = 0;
  }

  /// <summary>
  /// Shows the action panel with the specified height.
  /// </summary>
  public void ShowActionPanel(int height = 44) {
    this._actionPanel.Height = height;
  }

  /// <summary>
  /// Hides the action panel.
  /// </summary>
  public void HideActionPanel() {
    this._actionPanel.Height = 0;
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var shadowOffset = this._showShadow ? this._shadowDepth : 0;
    var cardRect = new Rectangle(
      shadowOffset,
      shadowOffset,
      this.Width - shadowOffset * 2,
      this.Height - shadowOffset * 2
    );

    // Draw shadow
    if (this._showShadow) {
      for (var i = this._shadowDepth; i > 0; --i) {
        var alpha = (int)(30 * (1 - (float)i / this._shadowDepth));
        using var shadowBrush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
        var shadowRect = new Rectangle(
          shadowOffset + i,
          shadowOffset + i,
          cardRect.Width,
          cardRect.Height
        );
        using var shadowPath = this._CreateRoundedRectangle(shadowRect, this._cornerRadius);
        g.FillPath(shadowBrush, shadowPath);
      }
    }

    // Draw card background
    using (var path = this._CreateRoundedRectangle(cardRect, this._cornerRadius)) {
      using var brush = new SolidBrush(this._cardColor);
      g.FillPath(brush, path);

      using var pen = new Pen(Color.FromArgb(30, Color.Black));
      g.DrawPath(pen, path);
    }

    // Draw title
    if (string.IsNullOrEmpty(this._title) && this._titleIcon == null)
      return;

    var titleY = shadowOffset + 8;
    var titleX = shadowOffset + 12;

    if (this._titleIcon != null) {
      g.DrawImage(this._titleIcon, titleX, titleY, 24, 24);
      titleX += 32;
    }

    if (!string.IsNullOrEmpty(this._title)) {
      using var titleFont = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold);
      TextRenderer.DrawText(g, this._title, titleFont,
        new Rectangle(titleX, titleY, cardRect.Width - titleX - 12, 24),
        this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
  }

  private GraphicsPath _CreateRoundedRectangle(Rectangle rect, int radius) {
    var path = new GraphicsPath();

    if (radius <= 0) {
      path.AddRectangle(rect);
      return path;
    }

    var diameter = radius * 2;
    var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

    path.AddArc(arc, 180, 90);
    arc.X = rect.Right - diameter;
    path.AddArc(arc, 270, 90);
    arc.Y = rect.Bottom - diameter;
    path.AddArc(arc, 0, 90);
    arc.X = rect.Left;
    path.AddArc(arc, 90, 90);
    path.CloseFigure();

    return path;
  }
}
