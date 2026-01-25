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
/// A text box with search icon and clear button.
/// </summary>
/// <example>
/// <code>
/// var searchBox = new SearchTextBox {
///   PlaceholderText = "Search...",
///   SearchDelay = 300
/// };
/// searchBox.SearchTriggered += (s, e) => PerformSearch(searchBox.Text);
/// </code>
/// </example>
public class SearchTextBox : UserControl {
  private readonly TextBox _textBox;
  private readonly PictureBox _searchIcon;
  private readonly Button _clearButton;
  private Timer _searchDelayTimer;
  private string _placeholderText = "Search...";
  private int _searchDelay = 300;
  private bool _showClearButton = true;
  private bool _isPlaceholderActive;

  /// <summary>
  /// Occurs when the search should be triggered.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the search should be triggered (after delay or Enter key).")]
  public event EventHandler SearchTriggered;

  /// <summary>
  /// Occurs when the text is cleared.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the text is cleared.")]
  public event EventHandler Cleared;

  /// <summary>
  /// Initializes a new instance of the <see cref="SearchTextBox"/> class.
  /// </summary>
  public SearchTextBox() {
    this._searchIcon = new PictureBox {
      Size = new Size(20, 20),
      SizeMode = PictureBoxSizeMode.CenterImage,
      Cursor = Cursors.Default
    };

    this._textBox = new TextBox {
      BorderStyle = BorderStyle.None,
      Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
    };

    this._clearButton = new Button {
      Size = new Size(20, 20),
      FlatStyle = FlatStyle.Flat,
      Text = "\u00D7",
      Cursor = Cursors.Hand,
      Visible = false
    };
    this._clearButton.FlatAppearance.BorderSize = 0;

    this._searchDelayTimer = new Timer { Interval = this._searchDelay };
    this._searchDelayTimer.Tick += this._OnSearchDelayTick;

    this._textBox.TextChanged += this._OnTextBoxTextChanged;
    this._textBox.KeyDown += this._OnTextBoxKeyDown;
    this._textBox.GotFocus += this._OnTextBoxGotFocus;
    this._textBox.LostFocus += this._OnTextBoxLostFocus;
    this._clearButton.Click += this._OnClearButtonClick;

    this.Controls.Add(this._searchIcon);
    this.Controls.Add(this._textBox);
    this.Controls.Add(this._clearButton);

    this.Size = new Size(200, 26);
    this.BackColor = SystemColors.Window;
    this.BorderStyle = BorderStyle.FixedSingle;

    this._UpdateLayout();
    this._ShowPlaceholder();
  }

  /// <summary>
  /// Gets or sets the text in the search box.
  /// </summary>
  [Category("Appearance")]
  [Description("The text in the search box.")]
  [Browsable(true)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
  public override string Text {
    get => this._isPlaceholderActive ? string.Empty : this._textBox.Text;
    set {
      if (string.IsNullOrEmpty(value)) {
        this._textBox.Text = string.Empty;
        if (!this._textBox.Focused)
          this._ShowPlaceholder();
      } else {
        this._HidePlaceholder();
        this._textBox.Text = value;
      }
    }
  }

  /// <summary>
  /// Gets or sets the placeholder text.
  /// </summary>
  [Category("Appearance")]
  [Description("The placeholder text displayed when the search box is empty.")]
  [DefaultValue("Search...")]
  public string PlaceholderText {
    get => this._placeholderText;
    set {
      this._placeholderText = value ?? "Search...";
      if (this._isPlaceholderActive)
        this._textBox.Text = this._placeholderText;
    }
  }

  /// <summary>
  /// Gets or sets whether to show the clear button.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the clear button when text is present.")]
  [DefaultValue(true)]
  public bool ShowClearButton {
    get => this._showClearButton;
    set {
      this._showClearButton = value;
      this._UpdateClearButtonVisibility();
    }
  }

  /// <summary>
  /// Gets or sets the delay in milliseconds before triggering the search.
  /// </summary>
  [Category("Behavior")]
  [Description("The delay in milliseconds before triggering the search after typing stops.")]
  [DefaultValue(300)]
  public int SearchDelay {
    get => this._searchDelay;
    set {
      this._searchDelay = Math.Max(0, value);
      this._searchDelayTimer.Interval = Math.Max(1, this._searchDelay);
    }
  }

  /// <summary>
  /// Gets or sets the search icon image.
  /// </summary>
  [Category("Appearance")]
  [Description("The search icon image.")]
  public Image SearchIcon {
    get => this._searchIcon.Image;
    set => this._searchIcon.Image = value;
  }

  private void _ShowPlaceholder() {
    if (this._isPlaceholderActive)
      return;

    this._isPlaceholderActive = true;
    this._textBox.Text = this._placeholderText;
    this._textBox.ForeColor = SystemColors.GrayText;
  }

  private void _HidePlaceholder() {
    if (!this._isPlaceholderActive)
      return;

    this._isPlaceholderActive = false;
    this._textBox.Text = string.Empty;
    this._textBox.ForeColor = this.ForeColor;
  }

  private void _UpdateLayout() {
    var padding = 4;
    var iconWidth = 20;
    var clearWidth = this._showClearButton ? 20 : 0;

    this._searchIcon.Location = new Point(padding, (this.ClientSize.Height - this._searchIcon.Height) / 2);
    this._textBox.Location = new Point(padding + iconWidth + padding, (this.ClientSize.Height - this._textBox.Height) / 2);
    this._textBox.Width = this.ClientSize.Width - iconWidth - clearWidth - padding * 4;
    this._clearButton.Location = new Point(this.ClientSize.Width - clearWidth - padding, (this.ClientSize.Height - this._clearButton.Height) / 2);

    this._DrawSearchIcon();
  }

  private void _DrawSearchIcon() {
    if (this._searchIcon.Image != null)
      return;

    var bmp = new Bitmap(16, 16);
    using (var g = Graphics.FromImage(bmp)) {
      g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
      using var pen = new Pen(SystemColors.GrayText, 1.5f);
      g.DrawEllipse(pen, 2, 2, 8, 8);
      g.DrawLine(pen, 9, 9, 13, 13);
    }

    this._searchIcon.Image = bmp;
  }

  private void _UpdateClearButtonVisibility() {
    this._clearButton.Visible = this._showClearButton && !this._isPlaceholderActive && !string.IsNullOrEmpty(this._textBox.Text);
  }

  private void _OnTextBoxTextChanged(object sender, EventArgs e) {
    this._UpdateClearButtonVisibility();

    if (this._isPlaceholderActive)
      return;

    this._searchDelayTimer.Stop();
    if (this._searchDelay > 0)
      this._searchDelayTimer.Start();
    else
      this.OnSearchTriggered(EventArgs.Empty);

    this.OnTextChanged(EventArgs.Empty);
  }

  private void _OnTextBoxKeyDown(object sender, KeyEventArgs e) {
    if (e.KeyCode != Keys.Enter)
      return;

    this._searchDelayTimer.Stop();
    this.OnSearchTriggered(EventArgs.Empty);
    e.SuppressKeyPress = true;
  }

  private void _OnTextBoxGotFocus(object sender, EventArgs e) {
    if (this._isPlaceholderActive)
      this._HidePlaceholder();
  }

  private void _OnTextBoxLostFocus(object sender, EventArgs e) {
    if (string.IsNullOrEmpty(this._textBox.Text))
      this._ShowPlaceholder();
  }

  private void _OnClearButtonClick(object sender, EventArgs e) {
    this.Text = string.Empty;
    this._textBox.Focus();
    this.OnCleared(EventArgs.Empty);
  }

  private void _OnSearchDelayTick(object sender, EventArgs e) {
    this._searchDelayTimer.Stop();
    this.OnSearchTriggered(EventArgs.Empty);
  }

  /// <summary>
  /// Raises the <see cref="SearchTriggered"/> event.
  /// </summary>
  protected virtual void OnSearchTriggered(EventArgs e) => this.SearchTriggered?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="Cleared"/> event.
  /// </summary>
  protected virtual void OnCleared(EventArgs e) => this.Cleared?.Invoke(this, e);

  /// <inheritdoc />
  protected override void OnResize(EventArgs e) {
    base.OnResize(e);
    this._UpdateLayout();
  }

  /// <inheritdoc />
  protected override void OnForeColorChanged(EventArgs e) {
    base.OnForeColorChanged(e);
    if (!this._isPlaceholderActive)
      this._textBox.ForeColor = this.ForeColor;
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._searchDelayTimer?.Stop();
      this._searchDelayTimer?.Dispose();
      this._searchDelayTimer = null;
      this._searchIcon?.Image?.Dispose();
    }

    base.Dispose(disposing);
  }
}
