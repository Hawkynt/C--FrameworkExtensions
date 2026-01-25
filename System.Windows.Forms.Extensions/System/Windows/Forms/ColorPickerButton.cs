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
/// A color selection button with dropdown panel.
/// </summary>
/// <example>
/// <code>
/// var colorPicker = new ColorPickerButton {
///   SelectedColor = Color.Blue,
///   AllowCustomColor = true
/// };
/// colorPicker.SelectedColorChanged += (s, e) => UpdateColor(colorPicker.SelectedColor);
/// </code>
/// </example>
public class ColorPickerButton : Control {
  private Color _selectedColor = Color.Black;
  private Color[] _standardColors;
  private readonly List<Color> _recentColors = new();
  private int _maxRecentColors = 10;
  private bool _allowCustomColor = true;
  private bool _showColorName = true;
  private bool _isDropDownOpen;

  private static readonly Color[] DefaultStandardColors = {
    Color.Black, Color.DarkGray, Color.Gray, Color.LightGray, Color.White,
    Color.DarkRed, Color.Red, Color.OrangeRed, Color.Orange, Color.Gold,
    Color.Yellow, Color.LightYellow, Color.LimeGreen, Color.Green, Color.DarkGreen,
    Color.Cyan, Color.DodgerBlue, Color.Blue, Color.DarkBlue, Color.Navy,
    Color.Purple, Color.Magenta, Color.Pink, Color.Brown, Color.SaddleBrown
  };

  /// <summary>
  /// Occurs when the <see cref="SelectedColor"/> property changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the SelectedColor property changes.")]
  public event EventHandler SelectedColorChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="ColorPickerButton"/> class.
  /// </summary>
  public ColorPickerButton() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.Selectable,
      true
    );

    this.Size = new Size(100, 25);
    this._standardColors = DefaultStandardColors;
  }

  /// <summary>
  /// Gets or sets the selected color.
  /// </summary>
  [Category("Appearance")]
  [Description("The selected color.")]
  public Color SelectedColor {
    get => this._selectedColor;
    set {
      if (this._selectedColor == value)
        return;

      this._selectedColor = value;
      this._AddToRecentColors(value);
      this.Invalidate();
      this.OnSelectedColorChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the standard colors palette.
  /// </summary>
  [Category("Appearance")]
  [Description("The standard colors palette.")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public Color[] StandardColors {
    get => this._standardColors;
    set => this._standardColors = value ?? DefaultStandardColors;
  }

  /// <summary>
  /// Gets the recent colors.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public Color[] RecentColors => this._recentColors.ToArray();

  /// <summary>
  /// Gets or sets the maximum number of recent colors to track.
  /// </summary>
  [Category("Behavior")]
  [Description("The maximum number of recent colors to track.")]
  [DefaultValue(10)]
  public int MaxRecentColors {
    get => this._maxRecentColors;
    set {
      this._maxRecentColors = Math.Max(0, value);
      while (this._recentColors.Count > this._maxRecentColors)
        this._recentColors.RemoveAt(this._recentColors.Count - 1);
    }
  }

  /// <summary>
  /// Gets or sets whether to allow custom color selection via ColorDialog.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to allow custom color selection via ColorDialog.")]
  [DefaultValue(true)]
  public bool AllowCustomColor {
    get => this._allowCustomColor;
    set => this._allowCustomColor = value;
  }

  /// <summary>
  /// Gets or sets whether to show the color name next to the swatch.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the color name next to the swatch.")]
  [DefaultValue(true)]
  public bool ShowColorName {
    get => this._showColorName;
    set {
      if (this._showColorName == value)
        return;
      this._showColorName = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeSelectedColor() => this._selectedColor != Color.Black;
  private void ResetSelectedColor() => this._selectedColor = Color.Black;

  /// <summary>
  /// Raises the <see cref="SelectedColorChanged"/> event.
  /// </summary>
  protected virtual void OnSelectedColorChanged(EventArgs e) => this.SelectedColorChanged?.Invoke(this, e);

  private void _AddToRecentColors(Color color) {
    this._recentColors.Remove(color);
    this._recentColors.Insert(0, color);
    while (this._recentColors.Count > this._maxRecentColors)
      this._recentColors.RemoveAt(this._recentColors.Count - 1);
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var bounds = this.ClientRectangle;
    var swatchSize = Math.Min(bounds.Height - 6, 16);
    var swatchRect = new Rectangle(4, (bounds.Height - swatchSize) / 2, swatchSize, swatchSize);

    // Draw button background
    var backColor = this._isDropDownOpen ? SystemColors.ControlDark : (this.Focused ? SystemColors.ControlLight : SystemColors.Control);
    using (var brush = new SolidBrush(backColor)) {
      g.FillRectangle(brush, bounds);
    }

    // Draw border
    using (var pen = new Pen(SystemColors.ControlDarkDark)) {
      g.DrawRectangle(pen, 0, 0, bounds.Width - 1, bounds.Height - 1);
    }

    // Draw color swatch with checkerboard for transparency
    this._DrawCheckerboard(g, swatchRect);
    using (var brush = new SolidBrush(this._selectedColor)) {
      g.FillRectangle(brush, swatchRect);
    }

    using (var pen = new Pen(Color.Black)) {
      g.DrawRectangle(pen, swatchRect);
    }

    // Draw color name
    if (this._showColorName) {
      var textX = swatchRect.Right + 6;
      var textWidth = bounds.Width - textX - 20;
      var colorName = this._selectedColor.IsNamedColor ? this._selectedColor.Name : $"#{this._selectedColor.R:X2}{this._selectedColor.G:X2}{this._selectedColor.B:X2}";
      var textRect = new Rectangle(textX, 0, textWidth, bounds.Height);
      var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
      TextRenderer.DrawText(g, colorName, this.Font, textRect, this.ForeColor, flags);
    }

    // Draw dropdown arrow
    var arrowX = bounds.Width - 14;
    var arrowY = bounds.Height / 2 - 2;
    using (var brush = new SolidBrush(this.ForeColor)) {
      var arrowPoints = new[] {
        new Point(arrowX, arrowY),
        new Point(arrowX + 8, arrowY),
        new Point(arrowX + 4, arrowY + 4)
      };
      g.FillPolygon(brush, arrowPoints);
    }
  }

  private void _DrawCheckerboard(Graphics g, Rectangle rect) {
    const int cellSize = 4;
    using var lightBrush = new SolidBrush(Color.White);
    using var darkBrush = new SolidBrush(Color.LightGray);

    for (var y = rect.Top; y < rect.Bottom; y += cellSize)
    for (var x = rect.Left; x < rect.Right; x += cellSize) {
      var isDark = ((x - rect.Left) / cellSize + (y - rect.Top) / cellSize) % 2 == 0;
      var cellRect = new Rectangle(x, y, Math.Min(cellSize, rect.Right - x), Math.Min(cellSize, rect.Bottom - y));
      g.FillRectangle(isDark ? darkBrush : lightBrush, cellRect);
    }
  }

  /// <inheritdoc />
  protected override void OnClick(EventArgs e) {
    base.OnClick(e);
    this._ShowColorDropDown();
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);
    if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) {
      this._ShowColorDropDown();
      e.Handled = true;
    }
  }

  private void _ShowColorDropDown() {
    this._isDropDownOpen = true;
    this.Invalidate();

    using var dropDown = new ColorPickerDropDown(this);
    dropDown.ColorSelected += (s, color) => this.SelectedColor = color;

    var screenPos = this.PointToScreen(new Point(0, this.Height));
    dropDown.Location = screenPos;
    dropDown.ShowDialog(this.FindForm());

    this._isDropDownOpen = false;
    this.Invalidate();
  }

  private sealed class ColorPickerDropDown : Form {
    private readonly ColorPickerButton _owner;
    private Color? _hoveredColor;
    private const int SwatchSize = 20;
    private const int CellPadding = 4;
    private const int Columns = 5;
    private Rectangle _moreColorsRect;

    public event EventHandler<Color> ColorSelected;

    public ColorPickerDropDown(ColorPickerButton owner) {
      this._owner = owner;

      this.FormBorderStyle = FormBorderStyle.None;
      this.StartPosition = FormStartPosition.Manual;
      this.ShowInTaskbar = false;
      this.BackColor = SystemColors.Window;
      this.DoubleBuffered = true;

      var rows = (owner._standardColors.Length + Columns - 1) / Columns;
      var recentRows = owner._recentColors.Count > 0 ? 1 : 0;
      var moreColorsHeight = owner._allowCustomColor ? 24 : 0;
      var headerHeight = 20;
      var recentHeaderHeight = recentRows > 0 ? 20 : 0;

      this.ClientSize = new Size(
        Columns * SwatchSize + CellPadding * 2,
        headerHeight + rows * SwatchSize + recentHeaderHeight + recentRows * SwatchSize + moreColorsHeight + CellPadding * 2
      );
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      var g = e.Graphics;
      g.SmoothingMode = SmoothingMode.AntiAlias;

      // Border
      using (var pen = new Pen(SystemColors.ControlDarkDark)) {
        g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
      }

      var y = CellPadding;

      // Standard colors header
      TextRenderer.DrawText(g, "Standard Colors", this.Font, new Rectangle(CellPadding, y, this.Width - CellPadding * 2, 16), SystemColors.ControlDarkDark, TextFormatFlags.Left);
      y += 20;

      // Standard colors
      for (var i = 0; i < this._owner._standardColors.Length; ++i) {
        var col = i % Columns;
        var row = i / Columns;
        var rect = new Rectangle(CellPadding + col * SwatchSize, y + row * SwatchSize, SwatchSize - 2, SwatchSize - 2);
        this._DrawSwatch(g, rect, this._owner._standardColors[i]);
      }

      y += ((this._owner._standardColors.Length + Columns - 1) / Columns) * SwatchSize;

      // Recent colors
      if (this._owner._recentColors.Count > 0) {
        TextRenderer.DrawText(g, "Recent", this.Font, new Rectangle(CellPadding, y, this.Width - CellPadding * 2, 16), SystemColors.ControlDarkDark, TextFormatFlags.Left);
        y += 20;

        for (var i = 0; i < Math.Min(this._owner._recentColors.Count, Columns); ++i) {
          var rect = new Rectangle(CellPadding + i * SwatchSize, y, SwatchSize - 2, SwatchSize - 2);
          this._DrawSwatch(g, rect, this._owner._recentColors[i]);
        }

        y += SwatchSize;
      }

      // More Colors button
      if (!this._owner._allowCustomColor)
        return;

      this._moreColorsRect = new Rectangle(CellPadding, y, this.Width - CellPadding * 2, 20);
      var isHovered = this._moreColorsRect.Contains(this.PointToClient(MousePosition));
      using (var brush = new SolidBrush(isHovered ? SystemColors.Highlight : SystemColors.Control)) {
        g.FillRectangle(brush, this._moreColorsRect);
      }

      TextRenderer.DrawText(g, "More Colors...", this.Font, this._moreColorsRect, isHovered ? SystemColors.HighlightText : SystemColors.ControlText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void _DrawSwatch(Graphics g, Rectangle rect, Color color) {
      var isHovered = this._hoveredColor.HasValue && this._hoveredColor.Value == color;
      var isSelected = this._owner._selectedColor == color;

      if (isHovered || isSelected) {
        using var highlightPen = new Pen(SystemColors.Highlight, 2);
        g.DrawRectangle(highlightPen, rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
      }

      using (var brush = new SolidBrush(color)) {
        g.FillRectangle(brush, rect);
      }

      using (var pen = new Pen(Color.Black)) {
        g.DrawRectangle(pen, rect);
      }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      base.OnMouseMove(e);
      var newHovered = this._GetColorAtPoint(e.Location);
      if (newHovered != this._hoveredColor) {
        this._hoveredColor = newHovered;
        this.Invalidate();
      }
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      base.OnMouseClick(e);

      if (this._owner._allowCustomColor && this._moreColorsRect.Contains(e.Location)) {
        using var dialog = new ColorDialog { Color = this._owner._selectedColor, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) {
          this.ColorSelected?.Invoke(this, dialog.Color);
        }

        this.DialogResult = DialogResult.OK;
        return;
      }

      var color = this._GetColorAtPoint(e.Location);
      if (!color.HasValue)
        return;

      this.ColorSelected?.Invoke(this, color.Value);
      this.DialogResult = DialogResult.OK;
    }

    protected override void OnDeactivate(EventArgs e) {
      base.OnDeactivate(e);
      this.Close();
    }

    private Color? _GetColorAtPoint(Point pt) {
      var y = CellPadding + 20;

      for (var i = 0; i < this._owner._standardColors.Length; ++i) {
        var col = i % Columns;
        var row = i / Columns;
        var rect = new Rectangle(CellPadding + col * SwatchSize, y + row * SwatchSize, SwatchSize, SwatchSize);
        if (rect.Contains(pt))
          return this._owner._standardColors[i];
      }

      y += ((this._owner._standardColors.Length + Columns - 1) / Columns) * SwatchSize;

      if (this._owner._recentColors.Count <= 0)
        return null;

      y += 20;
      for (var i = 0; i < Math.Min(this._owner._recentColors.Count, Columns); ++i) {
        var rect = new Rectangle(CellPadding + i * SwatchSize, y, SwatchSize, SwatchSize);
        if (rect.Contains(pt))
          return this._owner._recentColors[i];
      }

      return null;
    }
  }
}
