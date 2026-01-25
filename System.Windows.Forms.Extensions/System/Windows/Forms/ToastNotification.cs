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
/// Specifies the type of toast notification.
/// </summary>
public enum ToastType {
  /// <summary>Information toast.</summary>
  Info,
  /// <summary>Success toast.</summary>
  Success,
  /// <summary>Warning toast.</summary>
  Warning,
  /// <summary>Error toast.</summary>
  Error
}

/// <summary>
/// Specifies the position of toast notifications.
/// </summary>
public enum ToastPosition {
  /// <summary>Top-left corner.</summary>
  TopLeft,
  /// <summary>Top-right corner.</summary>
  TopRight,
  /// <summary>Bottom-left corner.</summary>
  BottomLeft,
  /// <summary>Bottom-right corner.</summary>
  BottomRight,
  /// <summary>Top-center.</summary>
  TopCenter,
  /// <summary>Bottom-center.</summary>
  BottomCenter
}

/// <summary>
/// Options for showing a toast notification.
/// </summary>
public class ToastOptions {
  /// <summary>
  /// Gets or sets the message.
  /// </summary>
  public string Message { get; set; }

  /// <summary>
  /// Gets or sets the title.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// Gets or sets the toast type.
  /// </summary>
  public ToastType Type { get; set; } = ToastType.Info;

  /// <summary>
  /// Gets or sets the duration in milliseconds (0 = persistent).
  /// </summary>
  public int Duration { get; set; } = 3000;

  /// <summary>
  /// Gets or sets whether to show the close button.
  /// </summary>
  public bool ShowCloseButton { get; set; } = true;

  /// <summary>
  /// Gets or sets a custom icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets the position.
  /// </summary>
  public ToastPosition? Position { get; set; }
}

/// <summary>
/// Manages toast notifications.
/// </summary>
public static class ToastManager {
  private static readonly List<ToastNotification> _activeToasts = new();
  private static ToastPosition _defaultPosition = ToastPosition.BottomRight;
  private static int _maxVisible = 5;

  /// <summary>
  /// Gets or sets the default position for toasts.
  /// </summary>
  public static ToastPosition DefaultPosition {
    get => _defaultPosition;
    set => _defaultPosition = value;
  }

  /// <summary>
  /// Gets or sets the maximum number of visible toasts.
  /// </summary>
  public static int MaxVisible {
    get => _maxVisible;
    set => _maxVisible = Math.Max(1, value);
  }

  /// <summary>
  /// Shows a toast notification.
  /// </summary>
  public static void Show(string message, ToastType type = ToastType.Info, int duration = 3000) {
    Show(new ToastOptions { Message = message, Type = type, Duration = duration });
  }

  /// <summary>
  /// Shows a toast notification with options.
  /// </summary>
  public static void Show(ToastOptions options) {
    if (_activeToasts.Count >= _maxVisible) {
      var oldest = _activeToasts[0];
      oldest.Close();
    }

    var toast = new ToastNotification {
      Message = options.Message,
      Title = options.Title,
      Type = options.Type,
      Duration = options.Duration,
      ShowCloseButton = options.ShowCloseButton,
      Icon = options.Icon
    };

    var position = options.Position ?? _defaultPosition;
    toast.Show(position);

    _activeToasts.Add(toast);
    toast.FormClosed += (s, e) => _activeToasts.Remove(toast);

    _RepositionToasts(position);
  }

  private static void _RepositionToasts(ToastPosition position) {
    var screen = Screen.PrimaryScreen.WorkingArea;
    const int margin = 10;
    const int spacing = 5;

    var y = position switch {
      ToastPosition.TopLeft or ToastPosition.TopRight or ToastPosition.TopCenter => screen.Top + margin,
      _ => screen.Bottom - margin
    };

    foreach (var toast in _activeToasts) {
      var x = position switch {
        ToastPosition.TopLeft or ToastPosition.BottomLeft => screen.Left + margin,
        ToastPosition.TopRight or ToastPosition.BottomRight => screen.Right - toast.Width - margin,
        _ => screen.Left + (screen.Width - toast.Width) / 2
      };

      if (position is ToastPosition.BottomLeft or ToastPosition.BottomRight or ToastPosition.BottomCenter)
        y -= toast.Height;

      toast.Location = new Point(x, y);

      if (position is ToastPosition.TopLeft or ToastPosition.TopRight or ToastPosition.TopCenter)
        y += toast.Height + spacing;
      else
        y -= spacing;
    }
  }
}

/// <summary>
/// A non-modal popup notification.
/// </summary>
/// <example>
/// <code>
/// ToastManager.Show("File saved successfully!", ToastType.Success);
/// </code>
/// </example>
public class ToastNotification : Form {
  private string _message = string.Empty;
  private string _title = string.Empty;
  private ToastType _type = ToastType.Info;
  private int _duration = 3000;
  private bool _showCloseButton = true;
  private Image _icon;
  private Timer _dismissTimer;
  private Rectangle _closeButtonRect;
  private bool _closeButtonHovered;

  /// <summary>
  /// Occurs when the toast is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the toast is clicked.")]
  public event EventHandler Clicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="ToastNotification"/> class.
  /// </summary>
  public ToastNotification() {
    this.FormBorderStyle = FormBorderStyle.None;
    this.StartPosition = FormStartPosition.Manual;
    this.ShowInTaskbar = false;
    this.TopMost = true;
    this.DoubleBuffered = true;

    this.Size = new Size(300, 80);

    this._dismissTimer = new Timer();
    this._dismissTimer.Tick += (s, e) => this.Close();
  }

  /// <summary>
  /// Gets or sets the message.
  /// </summary>
  [Category("Appearance")]
  [Description("The message text.")]
  public string Message {
    get => this._message;
    set {
      this._message = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the title.
  /// </summary>
  [Category("Appearance")]
  [Description("The title text.")]
  public string Title {
    get => this._title;
    set {
      this._title = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the toast type.
  /// </summary>
  [Category("Appearance")]
  [Description("The toast type.")]
  [DefaultValue(ToastType.Info)]
  public ToastType Type {
    get => this._type;
    set {
      this._type = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the duration in milliseconds (0 = persistent).
  /// </summary>
  [Category("Behavior")]
  [Description("The duration in milliseconds (0 = persistent).")]
  [DefaultValue(3000)]
  public int Duration {
    get => this._duration;
    set => this._duration = Math.Max(0, value);
  }

  /// <summary>
  /// Gets or sets whether to show the close button.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the close button.")]
  [DefaultValue(true)]
  public bool ShowCloseButton {
    get => this._showCloseButton;
    set {
      this._showCloseButton = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets a custom icon.
  /// </summary>
  [Category("Appearance")]
  [Description("A custom icon.")]
  public new Image Icon {
    get => this._icon;
    set {
      this._icon = value;
      this.Invalidate();
    }
  }

  internal void Show(ToastPosition position) {
    this.Show();

    if (this._duration > 0) {
      this._dismissTimer.Interval = this._duration;
      this._dismissTimer.Start();
    }
  }

  /// <summary>
  /// Raises the <see cref="Clicked"/> event.
  /// </summary>
  protected virtual void OnClicked(EventArgs e) => this.Clicked?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    base.OnPaint(e);
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;

    // Determine colors based on type
    var (backColor, accentColor) = this._type switch {
      ToastType.Success => (Color.FromArgb(240, 255, 240), Color.Green),
      ToastType.Warning => (Color.FromArgb(255, 250, 230), Color.Orange),
      ToastType.Error => (Color.FromArgb(255, 240, 240), Color.Red),
      _ => (Color.FromArgb(240, 248, 255), Color.DodgerBlue)
    };

    // Draw background with rounded corners
    using (var path = this._CreateRoundedRectangle(bounds, 8)) {
      using var brush = new SolidBrush(backColor);
      g.FillPath(brush, path);

      using var pen = new Pen(accentColor, 2);
      g.DrawPath(pen, path);
    }

    // Draw accent bar on left
    using (var accentBrush = new SolidBrush(accentColor)) {
      g.FillRectangle(accentBrush, 0, 8, 4, bounds.Height - 16);
    }

    var contentX = 16;

    // Draw icon
    if (this._icon != null) {
      g.DrawImage(this._icon, contentX, (bounds.Height - 32) / 2, 32, 32);
      contentX += 40;
    } else {
      // Draw default type icon
      var iconChar = this._type switch {
        ToastType.Success => "\u2713",
        ToastType.Warning => "\u26A0",
        ToastType.Error => "\u2717",
        _ => "\u2139"
      };
      using var iconFont = new Font("Segoe UI Symbol", 16);
      TextRenderer.DrawText(g, iconChar, iconFont, new Rectangle(contentX, (bounds.Height - 24) / 2, 24, 24), accentColor);
      contentX += 32;
    }

    // Draw title and message
    var textWidth = bounds.Width - contentX - (this._showCloseButton ? 30 : 10);
    var textY = 10;

    if (!string.IsNullOrEmpty(this._title)) {
      using var titleFont = new Font(this.Font, FontStyle.Bold);
      TextRenderer.DrawText(g, this._title, titleFont, new Rectangle(contentX, textY, textWidth, 20), Color.Black, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
      textY += 20;
    }

    if (!string.IsNullOrEmpty(this._message))
      TextRenderer.DrawText(g, this._message, this.Font, new Rectangle(contentX, textY, textWidth, bounds.Height - textY - 10), Color.DarkGray, TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

    // Draw close button
    if (!this._showCloseButton)
      return;

    this._closeButtonRect = new Rectangle(bounds.Width - 24, 8, 16, 16);
    var closeColor = this._closeButtonHovered ? Color.Red : Color.Gray;
    using (var closePen = new Pen(closeColor, 2)) {
      g.DrawLine(closePen, this._closeButtonRect.Left + 3, this._closeButtonRect.Top + 3, this._closeButtonRect.Right - 3, this._closeButtonRect.Bottom - 3);
      g.DrawLine(closePen, this._closeButtonRect.Right - 3, this._closeButtonRect.Top + 3, this._closeButtonRect.Left + 3, this._closeButtonRect.Bottom - 3);
    }
  }

  private GraphicsPath _CreateRoundedRectangle(Rectangle rect, int radius) {
    var path = new GraphicsPath();
    var diameter = radius * 2;
    var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

    path.AddArc(arc, 180, 90);
    arc.X = rect.Right - diameter - 1;
    path.AddArc(arc, 270, 90);
    arc.Y = rect.Bottom - diameter - 1;
    path.AddArc(arc, 0, 90);
    arc.X = rect.Left;
    path.AddArc(arc, 90, 90);
    path.CloseFigure();

    return path;
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    var newHovered = this._showCloseButton && this._closeButtonRect.Contains(e.Location);
    if (newHovered != this._closeButtonHovered) {
      this._closeButtonHovered = newHovered;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (this._closeButtonHovered) {
      this.Close();
      return;
    }

    this.OnClicked(EventArgs.Empty);
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._dismissTimer?.Stop();
      this._dismissTimer?.Dispose();
      this._dismissTimer = null;
    }

    base.Dispose(disposing);
  }
}
