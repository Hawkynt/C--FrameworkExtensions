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
/// An image/content carousel control with navigation and auto-rotation.
/// </summary>
/// <example>
/// <code>
/// var carousel = new CarouselControl {
///   AutoRotate = true,
///   AutoRotateInterval = 5000,
///   ShowIndicators = true
/// };
/// carousel.Items.Add(new CarouselItem { Image = myImage, Title = "Slide 1" });
/// carousel.ItemChanged += (s, e) => Console.WriteLine($"Now showing: {e.Item.Title}");
/// </code>
/// </example>
public class CarouselControl : Control {
  private readonly List<CarouselItem> _items = [];
  private int _currentIndex;
  private bool _autoRotate;
  private int _autoRotateInterval = 5000;
  private bool _showNavigation = true;
  private bool _showIndicators = true;
  private CarouselTransition _transition = CarouselTransition.Slide;
  private int _transitionDuration = 300;
  private bool _enableSwipe = true;
  private bool _loop = true;

  private Timer _autoRotateTimer;
  private Timer _transitionTimer;
  private float _transitionProgress;
  private int _transitionFromIndex = -1;
  private bool _isTransitioning;
  private const int TransitionInterval = 16;

  private Point _mouseDownPoint;
  private bool _isDragging;
  private int _dragOffset;

  private Rectangle _leftArrowRect;
  private Rectangle _rightArrowRect;
  private bool _leftArrowHover;
  private bool _rightArrowHover;

  /// <summary>
  /// Occurs when the current item changes.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when the current item changes.")]
  public event EventHandler<CarouselItemEventArgs> ItemChanged;

  /// <summary>
  /// Occurs when an item is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is clicked.")]
  public event EventHandler<CarouselItemEventArgs> ItemClicked;

  /// <summary>
  /// Initializes a new instance of the <see cref="CarouselControl"/> class.
  /// </summary>
  public CarouselControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor
      | ControlStyles.Selectable,
      true
    );

    this.Size = new Size(400, 250);
    this.BackColor = Color.Black;

    this._autoRotateTimer = new Timer { Interval = this._autoRotateInterval };
    this._autoRotateTimer.Tick += this._OnAutoRotateTick;

    this._transitionTimer = new Timer { Interval = TransitionInterval };
    this._transitionTimer.Tick += this._OnTransitionTick;
  }

  /// <summary>
  /// Gets the collection of carousel items.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList<CarouselItem> Items => this._items;

  /// <summary>
  /// Gets or sets the current item index.
  /// </summary>
  [Category("Behavior")]
  [Description("The index of the currently displayed item.")]
  [DefaultValue(0)]
  public int CurrentIndex {
    get => this._currentIndex;
    set {
      if (this._items.Count == 0)
        return;

      var newIndex = value;
      if (this._loop) {
        if (newIndex < 0)
          newIndex = this._items.Count - 1;
        else if (newIndex >= this._items.Count)
          newIndex = 0;
      } else
        newIndex = Math.Max(0, Math.Min(this._items.Count - 1, newIndex));

      if (this._currentIndex == newIndex)
        return;

      this._StartTransition(newIndex);
    }
  }

  /// <summary>
  /// Gets the currently displayed item.
  /// </summary>
  [Browsable(false)]
  public CarouselItem CurrentItem =>
    this._items.Count > 0 && this._currentIndex >= 0 && this._currentIndex < this._items.Count
      ? this._items[this._currentIndex]
      : null;

  /// <summary>
  /// Gets or sets whether to automatically rotate through items.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether to automatically rotate through items.")]
  [DefaultValue(false)]
  public bool AutoRotate {
    get => this._autoRotate;
    set {
      this._autoRotate = value;
      if (value && !this.DesignMode)
        this._autoRotateTimer.Start();
      else
        this._autoRotateTimer.Stop();
    }
  }

  /// <summary>
  /// Gets or sets the auto-rotate interval in milliseconds.
  /// </summary>
  [Category("Behavior")]
  [Description("The auto-rotate interval in milliseconds.")]
  [DefaultValue(5000)]
  public int AutoRotateInterval {
    get => this._autoRotateInterval;
    set {
      this._autoRotateInterval = Math.Max(500, value);
      this._autoRotateTimer.Interval = this._autoRotateInterval;
    }
  }

  /// <summary>
  /// Gets or sets whether to show navigation arrows.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show navigation arrows.")]
  [DefaultValue(true)]
  public bool ShowNavigation {
    get => this._showNavigation;
    set {
      if (this._showNavigation == value)
        return;
      this._showNavigation = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show indicator dots.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show indicator dots.")]
  [DefaultValue(true)]
  public bool ShowIndicators {
    get => this._showIndicators;
    set {
      if (this._showIndicators == value)
        return;
      this._showIndicators = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the transition type.
  /// </summary>
  [Category("Behavior")]
  [Description("The transition type when changing items.")]
  [DefaultValue(CarouselTransition.Slide)]
  public CarouselTransition Transition {
    get => this._transition;
    set => this._transition = value;
  }

  /// <summary>
  /// Gets or sets the transition duration in milliseconds.
  /// </summary>
  [Category("Behavior")]
  [Description("The transition duration in milliseconds.")]
  [DefaultValue(300)]
  public int TransitionDuration {
    get => this._transitionDuration;
    set => this._transitionDuration = Math.Max(0, value);
  }

  /// <summary>
  /// Gets or sets whether mouse drag navigation is enabled.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether mouse drag navigation is enabled.")]
  [DefaultValue(true)]
  public bool EnableSwipe {
    get => this._enableSwipe;
    set => this._enableSwipe = value;
  }

  /// <summary>
  /// Gets or sets whether the carousel loops.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether the carousel loops from last to first item.")]
  [DefaultValue(true)]
  public bool Loop {
    get => this._loop;
    set => this._loop = value;
  }

  /// <summary>
  /// Navigates to the next item.
  /// </summary>
  public void Next() {
    if (this._items.Count == 0)
      return;
    this.CurrentIndex = this._currentIndex + 1;
    this._ResetAutoRotateTimer();
  }

  /// <summary>
  /// Navigates to the previous item.
  /// </summary>
  public void Previous() {
    if (this._items.Count == 0)
      return;
    this.CurrentIndex = this._currentIndex - 1;
    this._ResetAutoRotateTimer();
  }

  /// <summary>
  /// Navigates to the specified index.
  /// </summary>
  public void GoTo(int index) {
    this.CurrentIndex = index;
    this._ResetAutoRotateTimer();
  }

  /// <summary>
  /// Raises the <see cref="ItemChanged"/> event.
  /// </summary>
  protected virtual void OnItemChanged(CarouselItemEventArgs e) => this.ItemChanged?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemClicked"/> event.
  /// </summary>
  protected virtual void OnItemClicked(CarouselItemEventArgs e) => this.ItemClicked?.Invoke(this, e);

  private void _StartTransition(int newIndex) {
    // Complete any existing transition
    if (this._isTransitioning)
      this._CompleteTransitionSilent();

    this._transitionFromIndex = this._currentIndex;
    this._currentIndex = newIndex;

    // Always raise the event immediately - the animation is just visual
    this.OnItemChanged(new CarouselItemEventArgs(this.CurrentItem, newIndex));

    if (this._transition == CarouselTransition.None || this._transitionDuration == 0 || this.DesignMode) {
      this.Invalidate();
      return;
    }

    this._transitionProgress = 0f;
    this._isTransitioning = true;
    this._transitionTimer.Start();
  }

  private void _CompleteTransitionSilent() {
    if (!this._isTransitioning)
      return;

    this._transitionProgress = 1f;
    this._isTransitioning = false;
    this._transitionTimer.Stop();
    this._transitionFromIndex = -1;
    this.Invalidate();
  }

  private void _OnTransitionTick(object sender, EventArgs e) {
    var step = TransitionInterval / (float)this._transitionDuration;
    this._transitionProgress += step;

    if (this._transitionProgress >= 1f) {
      this._transitionProgress = 1f;
      this._isTransitioning = false;
      this._transitionTimer.Stop();
      this._transitionFromIndex = -1;
    }

    this.Invalidate();
  }

  private void _OnAutoRotateTick(object sender, EventArgs e) {
    if (this._items.Count > 1 && !this._isTransitioning)
      this.Next();
  }

  private void _ResetAutoRotateTimer() {
    if (!this._autoRotate || this.DesignMode)
      return;
    this._autoRotateTimer.Stop();
    this._autoRotateTimer.Start();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

    var bounds = this.ClientRectangle;

    // Fill background
    using (var brush = new SolidBrush(this.BackColor))
      g.FillRectangle(brush, bounds);

    if (this._items.Count == 0) {
      this._DrawNoItemsMessage(g, bounds);
      return;
    }

    // Draw items with transition
    var contentBounds = this._GetContentBounds();
    this._DrawItems(g, contentBounds);

    // Draw navigation arrows
    if (this._showNavigation && this._items.Count > 1)
      this._DrawNavigationArrows(g, bounds);

    // Draw indicators
    if (this._showIndicators && this._items.Count > 1)
      this._DrawIndicators(g, bounds);
  }

  private void _DrawNoItemsMessage(Graphics g, Rectangle bounds) {
    TextRenderer.DrawText(
      g,
      "No items",
      this.Font,
      bounds,
      Color.Gray,
      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
    );
  }

  private Rectangle _GetContentBounds() {
    var bounds = this.ClientRectangle;
    var indicatorHeight = this._showIndicators ? 30 : 0;
    return new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - indicatorHeight);
  }

  private void _DrawItems(Graphics g, Rectangle contentBounds) {
    var clip = g.Clip;
    g.SetClip(contentBounds);

    if (this._isDragging && this._enableSwipe)
      this._DrawDraggedItems(g, contentBounds);
    else if (this._isTransitioning)
      this._DrawTransitioningItems(g, contentBounds);
    else
      this._DrawCurrentItem(g, contentBounds);

    g.Clip = clip;
  }

  private void _DrawCurrentItem(Graphics g, Rectangle bounds) {
    var item = this.CurrentItem;
    if (item == null)
      return;
    this._DrawItem(g, item, bounds, 1f);
  }

  private void _DrawTransitioningItems(Graphics g, Rectangle bounds) {
    var fromItem = this._transitionFromIndex >= 0 && this._transitionFromIndex < this._items.Count
      ? this._items[this._transitionFromIndex]
      : null;
    var toItem = this.CurrentItem;

    var progress = this._EaseInOutCubic(this._transitionProgress);
    var direction = this._currentIndex > this._transitionFromIndex ? 1 : -1;

    switch (this._transition) {
      case CarouselTransition.Slide:
        this._DrawSlideTransition(g, bounds, fromItem, toItem, progress, direction);
        break;
      case CarouselTransition.Fade:
        this._DrawFadeTransition(g, bounds, fromItem, toItem, progress);
        break;
      case CarouselTransition.Push:
        this._DrawPushTransition(g, bounds, fromItem, toItem, progress, direction);
        break;
      default:
        this._DrawCurrentItem(g, bounds);
        break;
    }
  }

  private void _DrawSlideTransition(Graphics g, Rectangle bounds, CarouselItem from, CarouselItem to, float progress, int direction) {
    var offset = (int)(bounds.Width * progress) * direction;

    if (from != null) {
      var fromBounds = new Rectangle(bounds.X - offset, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, from, fromBounds, 1f);
    }

    if (to != null) {
      var toBounds = new Rectangle(bounds.X + bounds.Width * direction - offset, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, to, toBounds, 1f);
    }
  }

  private void _DrawFadeTransition(Graphics g, Rectangle bounds, CarouselItem from, CarouselItem to, float progress) {
    if (from != null)
      this._DrawItem(g, from, bounds, 1f - progress);
    if (to != null)
      this._DrawItem(g, to, bounds, progress);
  }

  private void _DrawPushTransition(Graphics g, Rectangle bounds, CarouselItem from, CarouselItem to, float progress, int direction) {
    var offset = (int)(bounds.Width * progress) * direction;
    var scale = 1f - 0.2f * progress;

    if (from != null) {
      var fromBounds = new Rectangle(bounds.X - offset, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, from, fromBounds, scale);
    }

    if (to != null) {
      var toBounds = new Rectangle(bounds.X + bounds.Width * direction - offset, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, to, toBounds, 1f);
    }
  }

  private void _DrawDraggedItems(Graphics g, Rectangle bounds) {
    var currentItem = this.CurrentItem;
    var currentBounds = new Rectangle(bounds.X + this._dragOffset, bounds.Y, bounds.Width, bounds.Height);
    if (currentItem != null)
      this._DrawItem(g, currentItem, currentBounds, 1f);

    // Draw adjacent item based on drag direction
    if (this._dragOffset > 0 && this._currentIndex > 0) {
      var prevItem = this._items[this._currentIndex - 1];
      var prevBounds = new Rectangle(bounds.X + this._dragOffset - bounds.Width, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, prevItem, prevBounds, 1f);
    } else if (this._dragOffset < 0 && this._currentIndex < this._items.Count - 1) {
      var nextItem = this._items[this._currentIndex + 1];
      var nextBounds = new Rectangle(bounds.X + this._dragOffset + bounds.Width, bounds.Y, bounds.Width, bounds.Height);
      this._DrawItem(g, nextItem, nextBounds, 1f);
    } else if (this._loop && this._items.Count > 1) {
      CarouselItem adjacentItem;
      Rectangle adjacentBounds;
      if (this._dragOffset > 0) {
        adjacentItem = this._items[this._items.Count - 1];
        adjacentBounds = new Rectangle(bounds.X + this._dragOffset - bounds.Width, bounds.Y, bounds.Width, bounds.Height);
      } else {
        adjacentItem = this._items[0];
        adjacentBounds = new Rectangle(bounds.X + this._dragOffset + bounds.Width, bounds.Y, bounds.Width, bounds.Height);
      }
      this._DrawItem(g, adjacentItem, adjacentBounds, 1f);
    }
  }

  private void _DrawItem(Graphics g, CarouselItem item, Rectangle bounds, float opacity) {
    if (item.Content != null) {
      // Custom control content - position it
      item.Content.Bounds = bounds;
      item.Content.Visible = true;
      return;
    }

    if (item.Image != null) {
      // Calculate aspect-fit bounds
      var imgBounds = this._CalculateAspectFitBounds(item.Image, bounds);

      if (opacity < 1f) {
        var colorMatrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = opacity };
        using var attributes = new System.Drawing.Imaging.ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);
        g.DrawImage(item.Image, imgBounds, 0, 0, item.Image.Width, item.Image.Height, GraphicsUnit.Pixel, attributes);
      } else
        g.DrawImage(item.Image, imgBounds);
    }

    // Draw title and description overlay
    if (!string.IsNullOrEmpty(item.Title) || !string.IsNullOrEmpty(item.Description)) {
      var overlayHeight = 60;
      var overlayBounds = new Rectangle(bounds.X, bounds.Bottom - overlayHeight, bounds.Width, overlayHeight);

      using (var overlayBrush = new SolidBrush(Color.FromArgb((int)(180 * opacity), Color.Black)))
        g.FillRectangle(overlayBrush, overlayBounds);

      var textColor = Color.FromArgb((int)(255 * opacity), Color.White);

      if (!string.IsNullOrEmpty(item.Title)) {
        using var titleFont = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold);
        var titleRect = new Rectangle(overlayBounds.X + 10, overlayBounds.Y + 8, overlayBounds.Width - 20, 24);
        TextRenderer.DrawText(g, item.Title, titleFont, titleRect, textColor, TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
      }

      if (!string.IsNullOrEmpty(item.Description)) {
        var descRect = new Rectangle(overlayBounds.X + 10, overlayBounds.Y + 32, overlayBounds.Width - 20, 20);
        TextRenderer.DrawText(g, item.Description, this.Font, descRect, textColor, TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
      }
    }
  }

  private Rectangle _CalculateAspectFitBounds(Image image, Rectangle container) {
    var imageRatio = (float)image.Width / image.Height;
    var containerRatio = (float)container.Width / container.Height;

    int width, height;
    if (imageRatio > containerRatio) {
      width = container.Width;
      height = (int)(container.Width / imageRatio);
    } else {
      height = container.Height;
      width = (int)(container.Height * imageRatio);
    }

    var x = container.X + (container.Width - width) / 2;
    var y = container.Y + (container.Height - height) / 2;

    return new Rectangle(x, y, width, height);
  }

  private void _DrawNavigationArrows(Graphics g, Rectangle bounds) {
    var arrowSize = 36;
    var arrowPadding = 10;

    this._leftArrowRect = new Rectangle(arrowPadding, (bounds.Height - arrowSize) / 2, arrowSize, arrowSize);
    this._rightArrowRect = new Rectangle(bounds.Width - arrowSize - arrowPadding, (bounds.Height - arrowSize) / 2, arrowSize, arrowSize);

    this._DrawArrow(g, this._leftArrowRect, true, this._leftArrowHover);
    this._DrawArrow(g, this._rightArrowRect, false, this._rightArrowHover);
  }

  private void _DrawArrow(Graphics g, Rectangle bounds, bool isLeft, bool isHover) {
    var backColor = isHover ? Color.FromArgb(180, Color.White) : Color.FromArgb(100, Color.White);
    var arrowColor = isHover ? Color.Black : Color.FromArgb(200, Color.Black);

    using (var brush = new SolidBrush(backColor))
      g.FillEllipse(brush, bounds);

    using var pen = new Pen(arrowColor, 2f);
    var centerX = bounds.X + bounds.Width / 2;
    var centerY = bounds.Y + bounds.Height / 2;
    var arrowLength = 8;

    if (isLeft) {
      g.DrawLine(pen, centerX + 3, centerY - arrowLength, centerX - 3, centerY);
      g.DrawLine(pen, centerX - 3, centerY, centerX + 3, centerY + arrowLength);
    } else {
      g.DrawLine(pen, centerX - 3, centerY - arrowLength, centerX + 3, centerY);
      g.DrawLine(pen, centerX + 3, centerY, centerX - 3, centerY + arrowLength);
    }
  }

  private void _DrawIndicators(Graphics g, Rectangle bounds) {
    var indicatorSize = 8;
    var indicatorSpacing = 6;
    var totalWidth = this._items.Count * indicatorSize + (this._items.Count - 1) * indicatorSpacing;
    var startX = (bounds.Width - totalWidth) / 2;
    var y = bounds.Height - 20;

    for (var i = 0; i < this._items.Count; ++i) {
      var x = startX + i * (indicatorSize + indicatorSpacing);
      var rect = new Rectangle(x, y, indicatorSize, indicatorSize);

      var isActive = i == this._currentIndex;
      var color = isActive ? Color.White : Color.FromArgb(100, Color.White);

      using var brush = new SolidBrush(color);
      g.FillEllipse(brush, rect);
    }
  }

  private float _EaseInOutCubic(float t) {
    if (t < 0.5f)
      return 4f * t * t * t;
    var f = 2f * t - 2f;
    return 0.5f * f * f * f + 1f;
  }

  /// <inheritdoc />
  protected override void OnMouseDown(MouseEventArgs e) {
    base.OnMouseDown(e);

    if (e.Button != MouseButtons.Left)
      return;

    if (this._showNavigation) {
      if (this._leftArrowRect.Contains(e.Location)) {
        this.Previous();
        return;
      }
      if (this._rightArrowRect.Contains(e.Location)) {
        this.Next();
        return;
      }
    }

    if (this._enableSwipe && !this._isTransitioning) {
      this._mouseDownPoint = e.Location;
      this._isDragging = true;
      this._dragOffset = 0;
      this.Capture = true;
    }
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (this._isDragging && this._enableSwipe) {
      this._dragOffset = e.X - this._mouseDownPoint.X;
      this.Invalidate();
      return;
    }

    if (this._showNavigation) {
      var leftHover = this._leftArrowRect.Contains(e.Location);
      var rightHover = this._rightArrowRect.Contains(e.Location);

      if (leftHover != this._leftArrowHover || rightHover != this._rightArrowHover) {
        this._leftArrowHover = leftHover;
        this._rightArrowHover = rightHover;
        this.Invalidate();
      }
    }
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);

    if (this._isDragging) {
      this._isDragging = false;
      this.Capture = false;

      var threshold = this.Width / 4;
      if (Math.Abs(this._dragOffset) > threshold) {
        if (this._dragOffset > 0)
          this.Previous();
        else
          this.Next();
      }

      this._dragOffset = 0;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);

    if (this._leftArrowHover || this._rightArrowHover) {
      this._leftArrowHover = false;
      this._rightArrowHover = false;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (e.Button == MouseButtons.Left && !this._showNavigation) {
      var contentBounds = this._GetContentBounds();
      if (contentBounds.Contains(e.Location) && this.CurrentItem != null)
        this.OnItemClicked(new CarouselItemEventArgs(this.CurrentItem, this._currentIndex));
    }
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);

    switch (e.KeyCode) {
      case Keys.Left:
        this.Previous();
        e.Handled = true;
        break;
      case Keys.Right:
        this.Next();
        e.Handled = true;
        break;
      case Keys.Home:
        if (this._items.Count > 0)
          this.GoTo(0);
        e.Handled = true;
        break;
      case Keys.End:
        if (this._items.Count > 0)
          this.GoTo(this._items.Count - 1);
        e.Handled = true;
        break;
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._autoRotateTimer?.Stop();
      this._autoRotateTimer?.Dispose();
      this._autoRotateTimer = null;

      this._transitionTimer?.Stop();
      this._transitionTimer?.Dispose();
      this._transitionTimer = null;
    }

    base.Dispose(disposing);
  }
}

/// <summary>
/// Represents an item in a <see cref="CarouselControl"/>.
/// </summary>
public class CarouselItem {
  /// <summary>
  /// Gets or sets the image to display.
  /// </summary>
  public Image Image { get; set; }

  /// <summary>
  /// Gets or sets the title text.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// Gets or sets the description text.
  /// </summary>
  public string Description { get; set; }

  /// <summary>
  /// Gets or sets a custom control to display instead of an image.
  /// </summary>
  public Control Content { get; set; }

  /// <summary>
  /// Gets or sets custom data associated with this item.
  /// </summary>
  public object Tag { get; set; }
}

/// <summary>
/// Specifies the transition type for the <see cref="CarouselControl"/>.
/// </summary>
public enum CarouselTransition {
  /// <summary>
  /// No transition, immediate change.
  /// </summary>
  None,

  /// <summary>
  /// Slide transition.
  /// </summary>
  Slide,

  /// <summary>
  /// Fade transition.
  /// </summary>
  Fade,

  /// <summary>
  /// Push transition with scaling.
  /// </summary>
  Push
}

/// <summary>
/// Provides data for carousel item events.
/// </summary>
public class CarouselItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the carousel item.
  /// </summary>
  public CarouselItem Item { get; }

  /// <summary>
  /// Gets the index of the item.
  /// </summary>
  public int Index { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="CarouselItemEventArgs"/> class.
  /// </summary>
  public CarouselItemEventArgs(CarouselItem item, int index) {
    this.Item = item;
    this.Index = index;
  }
}
