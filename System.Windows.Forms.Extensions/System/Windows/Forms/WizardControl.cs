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
/// Event arguments for wizard page change events.
/// </summary>
public class WizardPageChangingEventArgs : CancelEventArgs {
  /// <summary>
  /// Gets the current page index.
  /// </summary>
  public int CurrentIndex { get; }

  /// <summary>
  /// Gets the new page index.
  /// </summary>
  public int NewIndex { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="WizardPageChangingEventArgs"/> class.
  /// </summary>
  public WizardPageChangingEventArgs(int currentIndex, int newIndex) {
    this.CurrentIndex = currentIndex;
    this.NewIndex = newIndex;
  }
}

/// <summary>
/// Event arguments for wizard page events.
/// </summary>
public class WizardPageEventArgs : EventArgs {
  /// <summary>
  /// Gets the page.
  /// </summary>
  public WizardPage Page { get; }

  /// <summary>
  /// Gets the page index.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="WizardPageEventArgs"/> class.
  /// </summary>
  public WizardPageEventArgs(WizardPage page, int index) {
    this.Page = page;
    this.Index = index;
  }
}

/// <summary>
/// Represents a page in a wizard control.
/// </summary>
public class WizardPage {
  private readonly Panel _contentPanel;

  /// <summary>
  /// Gets or sets the page title.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// Gets or sets the page description.
  /// </summary>
  public string Description { get; set; }

  /// <summary>
  /// Gets or sets the page icon.
  /// </summary>
  public Image Icon { get; set; }

  /// <summary>
  /// Gets the content panel.
  /// </summary>
  public Panel ContentPanel => this._contentPanel;

  /// <summary>
  /// Gets or sets whether the user can move to the next page.
  /// </summary>
  public bool CanMoveNext { get; set; } = true;

  /// <summary>
  /// Gets or sets whether the user can move to the previous page.
  /// </summary>
  public bool CanMoveBack { get; set; } = true;

  /// <summary>
  /// Gets or sets custom data associated with this page.
  /// </summary>
  public object Tag { get; set; }

  internal WizardPage(Panel contentPanel) {
    this._contentPanel = contentPanel;
  }
}

/// <summary>
/// A multi-step wizard control with navigation.
/// </summary>
/// <example>
/// <code>
/// var wizard = new WizardControl();
/// var page1 = wizard.AddPage("Welcome", "Welcome to the wizard");
/// page1.ContentPanel.Controls.Add(new Label { Text = "Page 1 content" });
/// var page2 = wizard.AddPage("Configuration", "Configure settings");
/// wizard.Finished += (s, e) => MessageBox.Show("Wizard completed!");
/// </code>
/// </example>
public class WizardControl : ContainerControl {
  private readonly List<WizardPage> _pages = new();
  private int _currentPageIndex;
  private bool _showStepIndicator = true;
  private bool _showNavigationButtons = true;
  private string _nextButtonText = "Next >";
  private string _backButtonText = "< Back";
  private string _finishButtonText = "Finish";
  private readonly Panel _pageContainer;
  private readonly Panel _navigationPanel;
  private readonly Button _backButton;
  private readonly Button _nextButton;
  private readonly Panel _stepIndicatorPanel;

  /// <summary>
  /// Occurs when the page is about to change.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the page is about to change.")]
  public event EventHandler<WizardPageChangingEventArgs> PageChanging;

  /// <summary>
  /// Occurs when the page has changed.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the page has changed.")]
  public event EventHandler<WizardPageEventArgs> PageChanged;

  /// <summary>
  /// Occurs when the wizard is finished.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the wizard is finished.")]
  public event EventHandler Finished;

  /// <summary>
  /// Initializes a new instance of the <see cref="WizardControl"/> class.
  /// </summary>
  public WizardControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this._stepIndicatorPanel = new Panel {
      Dock = DockStyle.Top,
      Height = 60,
      BackColor = SystemColors.ControlLight
    };
    this._stepIndicatorPanel.Paint += this._OnStepIndicatorPaint;

    this._navigationPanel = new Panel {
      Dock = DockStyle.Bottom,
      Height = 45,
      BackColor = SystemColors.Control
    };

    this._backButton = new Button {
      Text = this._backButtonText,
      Size = new Size(80, 30),
      Anchor = AnchorStyles.Bottom | AnchorStyles.Left
    };
    this._backButton.Click += (s, e) => this.Back();

    this._nextButton = new Button {
      Text = this._nextButtonText,
      Size = new Size(80, 30),
      Anchor = AnchorStyles.Bottom | AnchorStyles.Right
    };
    this._nextButton.Click += this._OnNextButtonClick;

    this._navigationPanel.Controls.Add(this._backButton);
    this._navigationPanel.Controls.Add(this._nextButton);

    this._pageContainer = new Panel {
      Dock = DockStyle.Fill,
      BackColor = SystemColors.Window,
      Padding = new Padding(10)
    };

    this.Controls.Add(this._pageContainer);
    this.Controls.Add(this._navigationPanel);
    this.Controls.Add(this._stepIndicatorPanel);

    this.Size = new Size(500, 400);
    this._UpdateLayout();
  }

  /// <summary>
  /// Gets the pages.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public WizardPage[] Pages => this._pages.ToArray();

  /// <summary>
  /// Gets or sets the current page index.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public int CurrentPageIndex {
    get => this._currentPageIndex;
    set => this.GoToPage(value);
  }

  /// <summary>
  /// Gets the current page.
  /// </summary>
  [Browsable(false)]
  public WizardPage CurrentPage => this._pages.Count > 0 && this._currentPageIndex < this._pages.Count
    ? this._pages[this._currentPageIndex]
    : null;

  /// <summary>
  /// Gets or sets whether to show the step indicator.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the step indicator.")]
  [DefaultValue(true)]
  public bool ShowStepIndicator {
    get => this._showStepIndicator;
    set {
      this._showStepIndicator = value;
      this._stepIndicatorPanel.Visible = value;
    }
  }

  /// <summary>
  /// Gets or sets whether to show navigation buttons.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show navigation buttons.")]
  [DefaultValue(true)]
  public bool ShowNavigationButtons {
    get => this._showNavigationButtons;
    set {
      this._showNavigationButtons = value;
      this._navigationPanel.Visible = value;
    }
  }

  /// <summary>
  /// Gets or sets the next button text.
  /// </summary>
  [Category("Appearance")]
  [Description("The text for the next button.")]
  [DefaultValue("Next >")]
  public string NextButtonText {
    get => this._nextButtonText;
    set {
      this._nextButtonText = value ?? "Next >";
      this._UpdateButtonText();
    }
  }

  /// <summary>
  /// Gets or sets the back button text.
  /// </summary>
  [Category("Appearance")]
  [Description("The text for the back button.")]
  [DefaultValue("< Back")]
  public string BackButtonText {
    get => this._backButtonText;
    set {
      this._backButtonText = value ?? "< Back";
      this._backButton.Text = this._backButtonText;
    }
  }

  /// <summary>
  /// Gets or sets the finish button text.
  /// </summary>
  [Category("Appearance")]
  [Description("The text for the finish button.")]
  [DefaultValue("Finish")]
  public string FinishButtonText {
    get => this._finishButtonText;
    set {
      this._finishButtonText = value ?? "Finish";
      this._UpdateButtonText();
    }
  }

  /// <summary>
  /// Adds a new page.
  /// </summary>
  public WizardPage AddPage(string title, string description = null) {
    var contentPanel = new Panel {
      Dock = DockStyle.Fill,
      Visible = this._pages.Count == 0
    };

    var page = new WizardPage(contentPanel) {
      Title = title,
      Description = description
    };

    this._pages.Add(page);
    this._pageContainer.Controls.Add(contentPanel);

    this._UpdateLayout();
    this._stepIndicatorPanel.Invalidate();

    return page;
  }

  /// <summary>
  /// Removes a page.
  /// </summary>
  public void RemovePage(WizardPage page) {
    var index = this._pages.IndexOf(page);
    if (index < 0)
      return;

    this._pages.Remove(page);
    this._pageContainer.Controls.Remove(page.ContentPanel);
    page.ContentPanel.Dispose();

    if (this._currentPageIndex >= this._pages.Count)
      this._currentPageIndex = Math.Max(0, this._pages.Count - 1);

    this._UpdateLayout();
    this._stepIndicatorPanel.Invalidate();
  }

  /// <summary>
  /// Navigates to the next page.
  /// </summary>
  public void Next() {
    if (this._currentPageIndex >= this._pages.Count - 1)
      return;

    this.GoToPage(this._currentPageIndex + 1);
  }

  /// <summary>
  /// Navigates to the previous page.
  /// </summary>
  public void Back() {
    if (this._currentPageIndex <= 0)
      return;

    this.GoToPage(this._currentPageIndex - 1);
  }

  /// <summary>
  /// Navigates to a specific page.
  /// </summary>
  public void GoToPage(int index) {
    if (index < 0 || index >= this._pages.Count || index == this._currentPageIndex)
      return;

    var args = new WizardPageChangingEventArgs(this._currentPageIndex, index);
    this.OnPageChanging(args);
    if (args.Cancel)
      return;

    if (this._currentPageIndex < this._pages.Count)
      this._pages[this._currentPageIndex].ContentPanel.Visible = false;

    this._currentPageIndex = index;
    this._pages[this._currentPageIndex].ContentPanel.Visible = true;

    this._UpdateLayout();
    this._stepIndicatorPanel.Invalidate();

    this.OnPageChanged(new WizardPageEventArgs(this._pages[this._currentPageIndex], this._currentPageIndex));
  }

  /// <summary>
  /// Raises the <see cref="PageChanging"/> event.
  /// </summary>
  protected virtual void OnPageChanging(WizardPageChangingEventArgs e) => this.PageChanging?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="PageChanged"/> event.
  /// </summary>
  protected virtual void OnPageChanged(WizardPageEventArgs e) => this.PageChanged?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="Finished"/> event.
  /// </summary>
  protected virtual void OnFinished(EventArgs e) => this.Finished?.Invoke(this, e);

  private void _OnNextButtonClick(object sender, EventArgs e) {
    if (this._currentPageIndex >= this._pages.Count - 1)
      this.OnFinished(EventArgs.Empty);
    else
      this.Next();
  }

  private void _UpdateLayout() {
    this._backButton.Location = new Point(10, (this._navigationPanel.Height - this._backButton.Height) / 2);
    this._nextButton.Location = new Point(this._navigationPanel.Width - this._nextButton.Width - 10,
      (this._navigationPanel.Height - this._nextButton.Height) / 2);

    var currentPage = this.CurrentPage;
    this._backButton.Enabled = this._currentPageIndex > 0 && (currentPage?.CanMoveBack ?? true);
    this._nextButton.Enabled = currentPage?.CanMoveNext ?? true;

    this._UpdateButtonText();
  }

  private void _UpdateButtonText() {
    this._nextButton.Text = this._currentPageIndex >= this._pages.Count - 1
      ? this._finishButtonText
      : this._nextButtonText;
  }

  private void _OnStepIndicatorPaint(object sender, PaintEventArgs e) {
    if (this._pages.Count == 0)
      return;

    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;

    var stepWidth = this._stepIndicatorPanel.Width / (float)this._pages.Count;
    var circleSize = 24;
    var lineY = this._stepIndicatorPanel.Height / 2;

    for (var i = 0; i < this._pages.Count; ++i) {
      var centerX = stepWidth * i + stepWidth / 2;

      // Draw connecting line
      if (i > 0) {
        var prevCenterX = stepWidth * (i - 1) + stepWidth / 2;
        using var linePen = new Pen(i <= this._currentPageIndex ? SystemColors.Highlight : SystemColors.ControlDark, 2);
        g.DrawLine(linePen, prevCenterX + circleSize / 2 + 2, lineY, centerX - circleSize / 2 - 2, lineY);
      }

      // Draw circle
      var circleRect = new Rectangle((int)(centerX - circleSize / 2), lineY - circleSize / 2, circleSize, circleSize);
      using (var brush = new SolidBrush(i <= this._currentPageIndex ? SystemColors.Highlight : SystemColors.ControlDark)) {
        g.FillEllipse(brush, circleRect);
      }

      // Draw step number
      var stepNumber = (i + 1).ToString();
      TextRenderer.DrawText(g, stepNumber, this.Font, circleRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

      // Draw page title below
      var titleRect = new Rectangle((int)(centerX - stepWidth / 2), lineY + circleSize / 2 + 4, (int)stepWidth, 20);
      var page = this._pages[i];
      TextRenderer.DrawText(g, page.Title ?? $"Step {i + 1}", this.Font, titleRect,
        i == this._currentPageIndex ? SystemColors.Highlight : this.ForeColor,
        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
    }
  }

  /// <inheritdoc />
  protected override void OnResize(EventArgs e) {
    base.OnResize(e);
    this._UpdateLayout();
    this._stepIndicatorPanel.Invalidate();
  }
}
