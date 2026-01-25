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

namespace System.Windows.Forms;

/// <summary>
/// A TextBox that displays placeholder (watermark) text when empty.
/// </summary>
/// <example>
/// <code>
/// var textBox = new PlaceholderTextBox {
///   PlaceholderText = "Enter your name...",
///   PlaceholderColor = Color.Gray
/// };
/// </code>
/// </example>
public class PlaceholderTextBox : TextBox {
  private string _placeholderText = string.Empty;
  private Color _placeholderColor = SystemColors.GrayText;
  private bool _showPlaceholderOnFocus;

  /// <summary>
  /// Gets or sets the placeholder text displayed when the TextBox is empty.
  /// </summary>
  [Category("Appearance")]
  [Description("The placeholder text to display when the TextBox is empty.")]
  [DefaultValue("")]
  public new string PlaceholderText {
    get => this._placeholderText;
    set {
      if (this._placeholderText == value)
        return;
      this._placeholderText = value ?? string.Empty;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color of the placeholder text.
  /// </summary>
  [Category("Appearance")]
  [Description("The color of the placeholder text.")]
  public Color PlaceholderColor {
    get => this._placeholderColor;
    set {
      if (this._placeholderColor == value)
        return;
      this._placeholderColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the placeholder text when the TextBox has focus.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to show the placeholder text when the TextBox has focus.")]
  [DefaultValue(false)]
  public bool ShowPlaceholderOnFocus {
    get => this._showPlaceholderOnFocus;
    set {
      if (this._showPlaceholderOnFocus == value)
        return;
      this._showPlaceholderOnFocus = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializePlaceholderColor() => this._placeholderColor != SystemColors.GrayText;
  private void ResetPlaceholderColor() => this._placeholderColor = SystemColors.GrayText;

  private bool _ShouldShowPlaceholder => !string.IsNullOrEmpty(this._placeholderText)
                                         && string.IsNullOrEmpty(this.Text)
                                         && (this._showPlaceholderOnFocus || !this.Focused);

  /// <inheritdoc />
  protected override void OnGotFocus(EventArgs e) {
    base.OnGotFocus(e);
    if (!this._showPlaceholderOnFocus)
      this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnLostFocus(EventArgs e) {
    base.OnLostFocus(e);
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnTextChanged(EventArgs e) {
    base.OnTextChanged(e);
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void WndProc(ref Message m) {
    base.WndProc(ref m);

    const int WM_PAINT = 0x000F;
    if (m.Msg == WM_PAINT && this._ShouldShowPlaceholder)
      this._DrawPlaceholder();
  }

  private void _DrawPlaceholder() {
    using var graphics = this.CreateGraphics();
    var textFormatFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

    var bounds = this.ClientRectangle;
    bounds.Offset(1, 1);
    bounds.Inflate(-1, -1);

    TextRenderer.DrawText(
      graphics,
      this._placeholderText,
      this.Font,
      bounds,
      this._placeholderColor,
      textFormatFlags
    );
  }
}
