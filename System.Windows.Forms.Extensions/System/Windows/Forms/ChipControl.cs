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
/// Event arguments for chip events.
/// </summary>
public class ChipEventArgs : EventArgs {
  /// <summary>
  /// Gets the chip that raised the event.
  /// </summary>
  public Chip Chip { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ChipEventArgs"/> class.
  /// </summary>
  public ChipEventArgs(Chip chip) {
    this.Chip = chip;
  }
}

/// <summary>
/// Represents a chip/tag.
/// </summary>
public class Chip {
  /// <summary>
  /// Gets or sets the text.
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets or sets the background color.
  /// </summary>
  public Color BackColor { get; set; } = Color.LightGray;

  /// <summary>
  /// Gets or sets the foreground color.
  /// </summary>
  public Color ForeColor { get; set; } = Color.Black;

  /// <summary>
  /// Gets or sets the icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets or sets whether this chip is selected.
  /// </summary>
  public bool IsSelected { get; set; }

  /// <summary>
  /// Gets or sets whether this chip can be removed.
  /// </summary>
  public bool CanRemove { get; set; } = true;

  /// <summary>
  /// Gets or sets custom data.
  /// </summary>
  public object Tag { get; set; }

  internal Rectangle Bounds { get; set; }
  internal Rectangle RemoveButtonBounds { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="Chip"/> class.
  /// </summary>
  public Chip() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="Chip"/> class.
  /// </summary>
  public Chip(string text, Color? backColor = null) {
    this.Text = text;
    if (backColor.HasValue)
      this.BackColor = backColor.Value;
  }
}

/// <summary>
/// A tag/chip collection control.
/// </summary>
/// <example>
/// <code>
/// var chipControl = new ChipControl();
/// chipControl.AddChip("C#", Color.Blue);
/// chipControl.AddChip(".NET", Color.Purple);
/// chipControl.ChipRemoved += (s, e) => Console.WriteLine($"Removed: {e.Chip.Text}");
/// </code>
/// </example>
public class ChipControl : Control {
  private readonly List<Chip> _chips = new();
  private bool _allowAdd = true;
  private bool _allowRemove = true;
  private bool _allowSelection;
  private SelectionMode _selectionMode = SelectionMode.None;
  private int _chipSpacing = 4;
  private Chip _hoveredChip;
  private bool _hoveringRemoveButton;

  /// <summary>
  /// Occurs when a chip is added.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a chip is added.")]
  public event EventHandler<ChipEventArgs> ChipAdded;

  /// <summary>
  /// Occurs when a chip is removed.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a chip is removed.")]
  public event EventHandler<ChipEventArgs> ChipRemoved;

  /// <summary>
  /// Occurs when a chip is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a chip is clicked.")]
  public event EventHandler<ChipEventArgs> ChipClicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="ChipControl"/> class.
  /// </summary>
  public ChipControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(300, 60);
  }

  /// <summary>
  /// Gets the chips.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public Chip[] Chips => this._chips.ToArray();

  /// <summary>
  /// Gets or sets whether chips can be added.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether chips can be added.")]
  [DefaultValue(true)]
  public bool AllowAdd {
    get => this._allowAdd;
    set => this._allowAdd = value;
  }

  /// <summary>
  /// Gets or sets whether chips can be removed.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether chips can be removed.")]
  [DefaultValue(true)]
  public bool AllowRemove {
    get => this._allowRemove;
    set {
      this._allowRemove = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether chips can be selected.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether chips can be selected.")]
  [DefaultValue(false)]
  public bool AllowSelection {
    get => this._allowSelection;
    set {
      this._allowSelection = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the selection mode.
  /// </summary>
  [Category("Behavior")]
  [Description("The selection mode.")]
  [DefaultValue(SelectionMode.None)]
  public SelectionMode SelectionMode {
    get => this._selectionMode;
    set {
      this._selectionMode = value;
      if (value == SelectionMode.None) {
        foreach (var chip in this._chips)
          chip.IsSelected = false;
        this.Invalidate();
      }
    }
  }

  /// <summary>
  /// Gets or sets the spacing between chips.
  /// </summary>
  [Category("Appearance")]
  [Description("The spacing between chips.")]
  [DefaultValue(4)]
  public int ChipSpacing {
    get => this._chipSpacing;
    set {
      this._chipSpacing = Math.Max(0, value);
      this.Invalidate();
    }
  }

  /// <summary>
  /// Adds a chip.
  /// </summary>
  public Chip AddChip(string text, Color? color = null) {
    if (!this._allowAdd)
      return null;

    var chip = new Chip(text, color);
    this._chips.Add(chip);
    this.Invalidate();
    this.OnChipAdded(new ChipEventArgs(chip));
    return chip;
  }

  /// <summary>
  /// Removes a chip.
  /// </summary>
  public void RemoveChip(Chip chip) {
    if (!this._chips.Contains(chip))
      return;

    this._chips.Remove(chip);
    this.Invalidate();
    this.OnChipRemoved(new ChipEventArgs(chip));
  }

  /// <summary>
  /// Clears all chips.
  /// </summary>
  public void ClearChips() {
    this._chips.Clear();
    this.Invalidate();
  }

  /// <summary>
  /// Gets the selected chips.
  /// </summary>
  public Chip[] GetSelectedChips() => this._chips.FindAll(c => c.IsSelected).ToArray();

  /// <summary>
  /// Raises the <see cref="ChipAdded"/> event.
  /// </summary>
  protected virtual void OnChipAdded(ChipEventArgs e) => this.ChipAdded?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ChipRemoved"/> event.
  /// </summary>
  protected virtual void OnChipRemoved(ChipEventArgs e) => this.ChipRemoved?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ChipClicked"/> event.
  /// </summary>
  protected virtual void OnChipClicked(ChipEventArgs e) => this.ChipClicked?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var x = 2;
    var y = 2;
    var chipHeight = 24;
    var removeButtonSize = 14;

    foreach (var chip in this._chips) {
      var textSize = TextRenderer.MeasureText(chip.Text, this.Font);
      var iconWidth = chip.Icon != null ? 20 : 0;
      var removeWidth = this._allowRemove && chip.CanRemove ? removeButtonSize + 4 : 0;
      var chipWidth = 8 + iconWidth + textSize.Width + removeWidth + 8;

      if (x + chipWidth > this.Width - 2 && x > 2) {
        x = 2;
        y += chipHeight + this._chipSpacing;
      }

      chip.Bounds = new Rectangle(x, y, chipWidth, chipHeight);

      // Draw chip background
      var backColor = chip.IsSelected ? SystemColors.Highlight : chip.BackColor;
      if (chip == this._hoveredChip && !this._hoveringRemoveButton)
        backColor = ControlPaint.Light(backColor);

      using (var path = this._CreateRoundedRectangle(chip.Bounds, chipHeight / 2)) {
        using var brush = new SolidBrush(backColor);
        g.FillPath(brush, path);

        using var pen = new Pen(ControlPaint.Dark(chip.BackColor));
        g.DrawPath(pen, path);
      }

      var contentX = x + 8;

      // Draw icon
      if (chip.Icon != null) {
        g.DrawImage(chip.Icon, contentX, y + (chipHeight - 16) / 2, 16, 16);
        contentX += 20;
      }

      // Draw text
      var foreColor = chip.IsSelected ? SystemColors.HighlightText : chip.ForeColor;
      TextRenderer.DrawText(g, chip.Text, this.Font, new Rectangle(contentX, y, textSize.Width, chipHeight), foreColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
      contentX += textSize.Width;

      // Draw remove button
      if (this._allowRemove && chip.CanRemove) {
        var removeX = x + chipWidth - removeButtonSize - 6;
        var removeY = y + (chipHeight - removeButtonSize) / 2;
        chip.RemoveButtonBounds = new Rectangle(removeX, removeY, removeButtonSize, removeButtonSize);

        var isHovered = chip == this._hoveredChip && this._hoveringRemoveButton;
        using (var removeBrush = new SolidBrush(isHovered ? Color.DarkRed : Color.Gray)) {
          g.FillEllipse(removeBrush, chip.RemoveButtonBounds);
        }

        using (var xPen = new Pen(Color.White, 1.5f)) {
          var cx = removeX + removeButtonSize / 2;
          var cy = removeY + removeButtonSize / 2;
          g.DrawLine(xPen, cx - 3, cy - 3, cx + 3, cy + 3);
          g.DrawLine(xPen, cx + 3, cy - 3, cx - 3, cy + 3);
        }
      }

      x += chipWidth + this._chipSpacing;
    }
  }

  private GraphicsPath _CreateRoundedRectangle(Rectangle rect, int radius) {
    var path = new GraphicsPath();
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

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    Chip newHovered = null;
    var hoveringRemove = false;

    foreach (var chip in this._chips) {
      if (chip.Bounds.Contains(e.Location)) {
        newHovered = chip;
        hoveringRemove = this._allowRemove && chip.CanRemove && chip.RemoveButtonBounds.Contains(e.Location);
        break;
      }
    }

    if (newHovered != this._hoveredChip || hoveringRemove != this._hoveringRemoveButton) {
      this._hoveredChip = newHovered;
      this._hoveringRemoveButton = hoveringRemove;
      this.Cursor = newHovered != null ? Cursors.Hand : Cursors.Default;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);
    if (this._hoveredChip != null) {
      this._hoveredChip = null;
      this._hoveringRemoveButton = false;
      this.Cursor = Cursors.Default;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (this._hoveredChip == null)
      return;

    if (this._hoveringRemoveButton) {
      this.RemoveChip(this._hoveredChip);
      return;
    }

    if (this._allowSelection) {
      if (this._selectionMode == SelectionMode.One) {
        foreach (var chip in this._chips)
          chip.IsSelected = chip == this._hoveredChip;
      } else if (this._selectionMode == SelectionMode.MultiSimple || this._selectionMode == SelectionMode.MultiExtended)
        this._hoveredChip.IsSelected = !this._hoveredChip.IsSelected;

      this.Invalidate();
    }

    this.OnChipClicked(new ChipEventArgs(this._hoveredChip));
  }
}
