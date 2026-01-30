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
using System.Globalization;
using System.Linq;

namespace System.Windows.Forms;

/// <summary>
/// A full-featured day/week/month scheduler for appointments.
/// </summary>
/// <example>
/// <code>
/// var scheduler = new SchedulerControl {
///   ViewType = SchedulerViewType.Week,
///   CurrentDate = DateTime.Today
/// };
/// scheduler.Items.Add(new SchedulerItem {
///   Subject = "Meeting",
///   Start = DateTime.Today.AddHours(10),
///   End = DateTime.Today.AddHours(11),
///   Color = Color.Blue
/// });
/// scheduler.ItemClicked += (s, e) => Console.WriteLine($"Clicked: {e.Item.Subject}");
/// </code>
/// </example>
public class SchedulerControl : ContainerControl {
  private SchedulerViewType _viewType = SchedulerViewType.Week;
  private DateTime _currentDate = DateTime.Today;
  private readonly List<SchedulerItem> _items = [];
  private readonly List<SchedulerResource> _resources = [];
  private TimeSpan _dayStartTime = TimeSpan.FromHours(8);
  private TimeSpan _dayEndTime = TimeSpan.FromHours(18);
  private int _timeSlotMinutes = 30;
  private bool _showAllDayArea = true;
  private bool _allowItemResize = true;
  private bool _allowItemMove = true;
  private bool _allowItemCreate = true;
  private Color _workingHoursColor = Color.White;
  private Color _nonWorkingHoursColor = Color.FromArgb(245, 245, 245);

  private VScrollBar _scrollBar;
  private int _headerHeight = 50;
  private int _allDayAreaHeight = 30;
  private int _timeColumnWidth = 60;
  private int _rowHeight = 24;

  private SchedulerItem _hoveredItem;
  private SchedulerItem _selectedItem;
  private SchedulerItem _draggedItem;
  private Point _dragStartPoint;
  private DateTime _dragStartTime;
  private bool _isDragging;
  private bool _isResizing;
  private ResizeEdge _resizeEdge;

  private enum ResizeEdge { None, Top, Bottom }

  /// <summary>
  /// Occurs when an item is clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is clicked.")]
  public event EventHandler<SchedulerItemEventArgs> ItemClicked;

  /// <summary>
  /// Occurs when an item is double-clicked.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is double-clicked.")]
  public event EventHandler<SchedulerItemEventArgs> ItemDoubleClicked;

  /// <summary>
  /// Occurs before an item is created.
  /// </summary>
  [Category("Action")]
  [Description("Occurs before an item is created.")]
  public event EventHandler<SchedulerItemEventArgs> ItemCreating;

  /// <summary>
  /// Occurs after an item is created.
  /// </summary>
  [Category("Action")]
  [Description("Occurs after an item is created.")]
  public event EventHandler<SchedulerItemEventArgs> ItemCreated;

  /// <summary>
  /// Occurs when an item is moved.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is moved.")]
  public event EventHandler<SchedulerItemMovedEventArgs> ItemMoved;

  /// <summary>
  /// Occurs when an item is resized.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when an item is resized.")]
  public event EventHandler<SchedulerItemResizedEventArgs> ItemResized;

  /// <summary>
  /// Occurs when the current date changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the current date changes.")]
  public event EventHandler<SchedulerDateEventArgs> DateChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerControl"/> class.
  /// </summary>
  public SchedulerControl() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw,
      true
    );

    this.Size = new Size(600, 400);
    this.BackColor = Color.White;

    this._scrollBar = new VScrollBar {
      Dock = DockStyle.Right,
      Minimum = 0,
      SmallChange = this._rowHeight,
      LargeChange = this._rowHeight * 4
    };
    this._scrollBar.Scroll += (s, e) => this.Invalidate();
    this.Controls.Add(this._scrollBar);

    this._UpdateScrollBar();
  }

  /// <summary>
  /// Gets or sets the view type.
  /// </summary>
  [Category("Appearance")]
  [Description("The view type.")]
  [DefaultValue(SchedulerViewType.Week)]
  public SchedulerViewType ViewType {
    get => this._viewType;
    set {
      if (this._viewType == value)
        return;
      this._viewType = value;
      this._UpdateScrollBar();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the current date.
  /// </summary>
  [Category("Behavior")]
  [Description("The current date.")]
  public DateTime CurrentDate {
    get => this._currentDate;
    set {
      if (this._currentDate.Date == value.Date)
        return;
      var oldDate = this._currentDate;
      this._currentDate = value.Date;
      this.OnDateChanged(new SchedulerDateEventArgs(oldDate, this._currentDate));
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets the start of the current view.
  /// </summary>
  [Browsable(false)]
  public DateTime ViewStart => this._GetViewStart();

  /// <summary>
  /// Gets the end of the current view.
  /// </summary>
  [Browsable(false)]
  public DateTime ViewEnd => this._GetViewEnd();

  /// <summary>
  /// Gets the collection of scheduler items.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList<SchedulerItem> Items => this._items;

  /// <summary>
  /// Gets the collection of resources.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList<SchedulerResource> Resources => this._resources;

  /// <summary>
  /// Gets or sets the day start time.
  /// </summary>
  [Category("Behavior")]
  [Description("The day start time.")]
  public TimeSpan DayStartTime {
    get => this._dayStartTime;
    set {
      this._dayStartTime = value;
      this._UpdateScrollBar();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the day end time.
  /// </summary>
  [Category("Behavior")]
  [Description("The day end time.")]
  public TimeSpan DayEndTime {
    get => this._dayEndTime;
    set {
      this._dayEndTime = value;
      this._UpdateScrollBar();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the time slot duration in minutes.
  /// </summary>
  [Category("Behavior")]
  [Description("The time slot duration in minutes.")]
  [DefaultValue(30)]
  public int TimeSlotMinutes {
    get => this._timeSlotMinutes;
    set {
      this._timeSlotMinutes = Math.Max(5, Math.Min(60, value));
      this._UpdateScrollBar();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show the all-day area.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show the all-day area.")]
  [DefaultValue(true)]
  public bool ShowAllDayArea {
    get => this._showAllDayArea;
    set {
      this._showAllDayArea = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether items can be resized.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether items can be resized.")]
  [DefaultValue(true)]
  public bool AllowItemResize {
    get => this._allowItemResize;
    set => this._allowItemResize = value;
  }

  /// <summary>
  /// Gets or sets whether items can be moved.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether items can be moved.")]
  [DefaultValue(true)]
  public bool AllowItemMove {
    get => this._allowItemMove;
    set => this._allowItemMove = value;
  }

  /// <summary>
  /// Gets or sets whether items can be created by clicking.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether items can be created by clicking.")]
  [DefaultValue(true)]
  public bool AllowItemCreate {
    get => this._allowItemCreate;
    set => this._allowItemCreate = value;
  }

  /// <summary>
  /// Gets or sets the color for working hours.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color for working hours.")]
  public Color WorkingHoursColor {
    get => this._workingHoursColor;
    set {
      this._workingHoursColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color for non-working hours.
  /// </summary>
  [Category("Appearance")]
  [Description("The background color for non-working hours.")]
  public Color NonWorkingHoursColor {
    get => this._nonWorkingHoursColor;
    set {
      this._nonWorkingHoursColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeWorkingHoursColor() => this._workingHoursColor != Color.White;
  private void ResetWorkingHoursColor() => this._workingHoursColor = Color.White;
  private bool ShouldSerializeNonWorkingHoursColor() => this._nonWorkingHoursColor != Color.FromArgb(245, 245, 245);
  private void ResetNonWorkingHoursColor() => this._nonWorkingHoursColor = Color.FromArgb(245, 245, 245);

  /// <summary>
  /// Navigates by the specified offset.
  /// </summary>
  public void NavigateDate(int offset) {
    switch (this._viewType) {
      case SchedulerViewType.Day:
        this.CurrentDate = this._currentDate.AddDays(offset);
        break;
      case SchedulerViewType.Week:
      case SchedulerViewType.WorkWeek:
        this.CurrentDate = this._currentDate.AddDays(offset * 7);
        break;
      case SchedulerViewType.Month:
        this.CurrentDate = this._currentDate.AddMonths(offset);
        break;
      case SchedulerViewType.Timeline:
        this.CurrentDate = this._currentDate.AddDays(offset);
        break;
    }
  }

  /// <summary>
  /// Navigates to today.
  /// </summary>
  public void GoToToday() {
    this.CurrentDate = DateTime.Today;
  }

  /// <summary>
  /// Navigates to a specific date.
  /// </summary>
  public void GoToDate(DateTime date) {
    this.CurrentDate = date;
  }

  /// <summary>
  /// Raises the <see cref="ItemClicked"/> event.
  /// </summary>
  protected virtual void OnItemClicked(SchedulerItemEventArgs e) => this.ItemClicked?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemDoubleClicked"/> event.
  /// </summary>
  protected virtual void OnItemDoubleClicked(SchedulerItemEventArgs e) => this.ItemDoubleClicked?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemCreating"/> event.
  /// </summary>
  protected virtual void OnItemCreating(SchedulerItemEventArgs e) => this.ItemCreating?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemCreated"/> event.
  /// </summary>
  protected virtual void OnItemCreated(SchedulerItemEventArgs e) => this.ItemCreated?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemMoved"/> event.
  /// </summary>
  protected virtual void OnItemMoved(SchedulerItemMovedEventArgs e) => this.ItemMoved?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="ItemResized"/> event.
  /// </summary>
  protected virtual void OnItemResized(SchedulerItemResizedEventArgs e) => this.ItemResized?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="DateChanged"/> event.
  /// </summary>
  protected virtual void OnDateChanged(SchedulerDateEventArgs e) => this.DateChanged?.Invoke(this, e);

  private DateTime _GetViewStart() {
    switch (this._viewType) {
      case SchedulerViewType.Day:
        return this._currentDate.Date;
      case SchedulerViewType.Week:
        var diff = (7 + (this._currentDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return this._currentDate.AddDays(-diff).Date;
      case SchedulerViewType.WorkWeek:
        var wdiff = (7 + (this._currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        return this._currentDate.AddDays(-wdiff).Date;
      case SchedulerViewType.Month:
        return new DateTime(this._currentDate.Year, this._currentDate.Month, 1);
      case SchedulerViewType.Timeline:
        return this._currentDate.Date;
      default:
        return this._currentDate.Date;
    }
  }

  private DateTime _GetViewEnd() {
    switch (this._viewType) {
      case SchedulerViewType.Day:
        return this._currentDate.Date.AddDays(1);
      case SchedulerViewType.Week:
        return this._GetViewStart().AddDays(7);
      case SchedulerViewType.WorkWeek:
        return this._GetViewStart().AddDays(5);
      case SchedulerViewType.Month:
        return this._GetViewStart().AddMonths(1);
      case SchedulerViewType.Timeline:
        return this._currentDate.Date.AddDays(7);
      default:
        return this._currentDate.Date.AddDays(1);
    }
  }

  private int _GetDayCount() {
    return this._viewType switch {
      SchedulerViewType.Day => 1,
      SchedulerViewType.Week => 7,
      SchedulerViewType.WorkWeek => 5,
      SchedulerViewType.Month => DateTime.DaysInMonth(this._currentDate.Year, this._currentDate.Month),
      SchedulerViewType.Timeline => 7,
      _ => 1
    };
  }

  private void _UpdateScrollBar() {
    if (this._scrollBar == null)
      return;

    if (this._viewType == SchedulerViewType.Month) {
      this._scrollBar.Visible = false;
      return;
    }

    this._scrollBar.Visible = true;
    var totalMinutes = (int)(this._dayEndTime - this._dayStartTime).TotalMinutes;
    var totalSlots = totalMinutes / this._timeSlotMinutes;
    var totalHeight = totalSlots * this._rowHeight;
    var visibleHeight = this.Height - this._headerHeight - (this._showAllDayArea ? this._allDayAreaHeight : 0);

    this._scrollBar.Maximum = Math.Max(0, totalHeight - visibleHeight + this._scrollBar.LargeChange);
    this._scrollBar.Value = Math.Min(this._scrollBar.Value, Math.Max(0, this._scrollBar.Maximum - this._scrollBar.LargeChange));
  }

  /// <inheritdoc />
  protected override void OnResize(EventArgs e) {
    base.OnResize(e);
    this._UpdateScrollBar();
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    if (this._viewType == SchedulerViewType.Month)
      this._DrawMonthView(g);
    else
      this._DrawDayWeekView(g);
  }

  private void _DrawDayWeekView(Graphics g) {
    var bounds = new Rectangle(0, 0, this.Width - this._scrollBar.Width, this.Height);
    var dayCount = this._GetDayCount();
    var dayWidth = (bounds.Width - this._timeColumnWidth) / dayCount;

    // Draw header
    this._DrawDayWeekHeader(g, bounds, dayCount, dayWidth);

    // Draw time column and grid
    var contentTop = this._headerHeight + (this._showAllDayArea ? this._allDayAreaHeight : 0);
    var contentBounds = new Rectangle(0, contentTop, bounds.Width, bounds.Height - contentTop);

    g.SetClip(contentBounds);
    this._DrawTimeColumn(g, contentBounds);
    this._DrawDayColumns(g, contentBounds, dayCount, dayWidth);
    this._DrawItems(g, contentBounds, dayCount, dayWidth);
    g.ResetClip();

    // Draw all-day area
    if (this._showAllDayArea)
      this._DrawAllDayArea(g, bounds, dayCount, dayWidth);
  }

  private void _DrawDayWeekHeader(Graphics g, Rectangle bounds, int dayCount, int dayWidth) {
    var headerRect = new Rectangle(0, 0, bounds.Width, this._headerHeight);

    // Draw background
    using (var brush = new SolidBrush(SystemColors.Control))
      g.FillRectangle(brush, headerRect);

    // Draw border
    using (var pen = new Pen(SystemColors.ControlDark))
      g.DrawLine(pen, 0, this._headerHeight - 1, bounds.Width, this._headerHeight - 1);

    // Draw day headers
    var viewStart = this._GetViewStart();
    for (var i = 0; i < dayCount; ++i) {
      var date = viewStart.AddDays(i);
      var x = this._timeColumnWidth + i * dayWidth;
      var dayRect = new Rectangle(x, 0, dayWidth, this._headerHeight);

      var isToday = date.Date == DateTime.Today;
      if (isToday) {
        using var brush = new SolidBrush(Color.FromArgb(230, 240, 255));
        g.FillRectangle(brush, dayRect);
      }

      var dayName = date.ToString("ddd");
      var dayNumber = date.Day.ToString();

      using var dayFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
      using var numFont = new Font(this.Font.FontFamily, this.Font.Size + 4, isToday ? FontStyle.Bold : FontStyle.Regular);

      var dayColor = isToday ? Color.DodgerBlue : this.ForeColor;
      TextRenderer.DrawText(g, dayName, dayFont, new Rectangle(dayRect.X, dayRect.Y + 4, dayRect.Width, 16), dayColor, TextFormatFlags.HorizontalCenter);
      TextRenderer.DrawText(g, dayNumber, numFont, new Rectangle(dayRect.X, dayRect.Y + 20, dayRect.Width, 24), dayColor, TextFormatFlags.HorizontalCenter);

      // Draw separator
      if (i > 0) {
        using var pen = new Pen(SystemColors.ControlLight);
        g.DrawLine(pen, x, 0, x, this._headerHeight);
      }
    }
  }

  private void _DrawAllDayArea(Graphics g, Rectangle bounds, int dayCount, int dayWidth) {
    var allDayRect = new Rectangle(0, this._headerHeight, bounds.Width, this._allDayAreaHeight);

    // Draw background
    using (var brush = new SolidBrush(Color.FromArgb(250, 250, 250)))
      g.FillRectangle(brush, allDayRect);

    // Draw label
    var labelRect = new Rectangle(0, this._headerHeight, this._timeColumnWidth, this._allDayAreaHeight);
    TextRenderer.DrawText(g, "All day", this.Font, labelRect, SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

    // Draw borders
    using (var pen = new Pen(SystemColors.ControlDark))
      g.DrawLine(pen, 0, allDayRect.Bottom - 1, bounds.Width, allDayRect.Bottom - 1);

    // Draw all-day items
    var viewStart = this._GetViewStart();
    foreach (var item in this._items.Where(i => i.AllDay)) {
      for (var day = 0; day < dayCount; ++day) {
        var date = viewStart.AddDays(day);
        if (item.Start.Date <= date && item.End.Date >= date) {
          var x = this._timeColumnWidth + day * dayWidth + 2;
          var itemRect = new Rectangle(x, this._headerHeight + 4, dayWidth - 4, this._allDayAreaHeight - 8);
          this._DrawSchedulerItem(g, item, itemRect);
        }
      }
    }
  }

  private void _DrawTimeColumn(Graphics g, Rectangle bounds) {
    var scrollOffset = this._scrollBar.Value;
    var totalMinutes = (int)(this._dayEndTime - this._dayStartTime).TotalMinutes;
    var slotCount = totalMinutes / this._timeSlotMinutes;

    for (var i = 0; i <= slotCount; ++i) {
      var time = this._dayStartTime.Add(TimeSpan.FromMinutes(i * this._timeSlotMinutes));
      var y = bounds.Top + i * this._rowHeight - scrollOffset;

      if (y < bounds.Top - this._rowHeight || y > bounds.Bottom)
        continue;

      // Draw time label (only on hour boundaries)
      if (time.Minutes == 0) {
        var timeText = DateTime.Today.Add(time).ToString("h tt");
        var timeRect = new Rectangle(0, y - 8, this._timeColumnWidth - 4, 16);
        TextRenderer.DrawText(g, timeText, this.Font, timeRect, SystemColors.GrayText, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
      }
    }
  }

  private void _DrawDayColumns(Graphics g, Rectangle bounds, int dayCount, int dayWidth) {
    var scrollOffset = this._scrollBar.Value;
    var totalMinutes = (int)(this._dayEndTime - this._dayStartTime).TotalMinutes;
    var slotCount = totalMinutes / this._timeSlotMinutes;
    var viewStart = this._GetViewStart();

    // Draw column backgrounds
    for (var day = 0; day < dayCount; ++day) {
      var date = viewStart.AddDays(day);
      var x = this._timeColumnWidth + day * dayWidth;

      // Draw non-working hours background
      for (var slot = 0; slot < slotCount; ++slot) {
        var time = this._dayStartTime.Add(TimeSpan.FromMinutes(slot * this._timeSlotMinutes));
        var y = bounds.Top + slot * this._rowHeight - scrollOffset;

        if (y < bounds.Top - this._rowHeight || y > bounds.Bottom)
          continue;

        var isWorkingHour = time >= TimeSpan.FromHours(9) && time < TimeSpan.FromHours(17);
        var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        var bgColor = isWorkingHour && !isWeekend ? this._workingHoursColor : this._nonWorkingHoursColor;

        using (var brush = new SolidBrush(bgColor))
          g.FillRectangle(brush, x, y, dayWidth, this._rowHeight);
      }
    }

    // Draw grid lines
    using var lightPen = new Pen(Color.FromArgb(230, 230, 230));
    using var darkPen = new Pen(SystemColors.ControlLight);

    for (var slot = 0; slot <= slotCount; ++slot) {
      var time = this._dayStartTime.Add(TimeSpan.FromMinutes(slot * this._timeSlotMinutes));
      var y = bounds.Top + slot * this._rowHeight - scrollOffset;

      if (y < bounds.Top || y > bounds.Bottom)
        continue;

      var pen = time.Minutes == 0 ? darkPen : lightPen;
      g.DrawLine(pen, this._timeColumnWidth, y, bounds.Width, y);
    }

    // Draw column separators
    for (var day = 0; day <= dayCount; ++day) {
      var x = this._timeColumnWidth + day * dayWidth;
      g.DrawLine(darkPen, x, bounds.Top, x, bounds.Bottom);
    }
  }

  private void _DrawItems(Graphics g, Rectangle bounds, int dayCount, int dayWidth) {
    var scrollOffset = this._scrollBar.Value;
    var viewStart = this._GetViewStart();
    var viewEnd = this._GetViewEnd();

    var visibleItems = this._items
      .Where(i => !i.AllDay && i.Start < viewEnd && i.End > viewStart)
      .OrderBy(i => i.Start)
      .ThenBy(i => i.End - i.Start)
      .ToList();

    // Group items by day and calculate overlap columns
    var itemsByDay = new Dictionary<int, List<(SchedulerItem Item, int Column, int TotalColumns)>>();

    for (var day = 0; day < dayCount; ++day)
      itemsByDay[day] = [];

    foreach (var item in visibleItems) {
      var itemStart = item.Start < viewStart ? viewStart : item.Start;
      var itemEnd = item.End > viewEnd ? viewEnd : item.End;

      var dayIndex = (int)(itemStart.Date - viewStart).TotalDays;
      if (dayIndex < 0 || dayIndex >= dayCount)
        continue;

      itemsByDay[dayIndex].Add((item, 0, 1));
    }

    // Process each day to assign columns for overlapping items
    foreach (var day in itemsByDay.Keys.ToList()) {
      var dayItems = itemsByDay[day];
      if (dayItems.Count <= 1)
        continue;

      // Find overlapping groups and assign columns
      var layoutInfo = this._CalculateOverlapLayout(dayItems.Select(x => x.Item).ToList(), viewStart);
      itemsByDay[day] = layoutInfo;
    }

    // Draw all items with proper positioning
    foreach (var day in itemsByDay.Keys) {
      foreach (var (item, column, totalColumns) in itemsByDay[day]) {
        var itemStart = item.Start < viewStart ? viewStart : item.Start;
        var itemEnd = item.End > viewEnd ? viewEnd : item.End;

        var startMinutes = (itemStart.TimeOfDay - this._dayStartTime).TotalMinutes;
        var endMinutes = (itemEnd.TimeOfDay - this._dayStartTime).TotalMinutes;

        if (endMinutes <= 0 || startMinutes >= (this._dayEndTime - this._dayStartTime).TotalMinutes)
          continue;

        startMinutes = Math.Max(0, startMinutes);
        endMinutes = Math.Min((this._dayEndTime - this._dayStartTime).TotalMinutes, endMinutes);

        var availableWidth = dayWidth - 4;
        var itemWidth = availableWidth / totalColumns;
        var x = this._timeColumnWidth + day * dayWidth + 2 + column * itemWidth;

        var y = bounds.Top + (int)(startMinutes / this._timeSlotMinutes * this._rowHeight) - scrollOffset;
        var height = (int)((endMinutes - startMinutes) / this._timeSlotMinutes * this._rowHeight);

        var itemRect = new Rectangle(x, y, itemWidth - 1, Math.Max(this._rowHeight / 2, height));
        if (itemRect.Bottom < bounds.Top || itemRect.Top > bounds.Bottom)
          continue;

        this._DrawSchedulerItem(g, item, itemRect);
      }
    }
  }

  private List<(SchedulerItem Item, int Column, int TotalColumns)> _CalculateOverlapLayout(List<SchedulerItem> items, DateTime viewStart) {
    if (items.Count == 0)
      return [];

    // Sort by start time, then by duration (longer first)
    items = items.OrderBy(i => i.Start).ThenByDescending(i => i.End - i.Start).ToList();

    // Track column assignments: (item, column index)
    var columns = new List<List<SchedulerItem>>();
    var itemColumns = new Dictionary<SchedulerItem, int>();

    foreach (var item in items) {
      // Find first column where this item doesn't overlap with existing items
      var placed = false;
      for (var col = 0; col < columns.Count; ++col) {
        var canPlace = true;
        foreach (var existing in columns[col]) {
          if (this._ItemsOverlap(item, existing)) {
            canPlace = false;
            break;
          }
        }

        if (canPlace) {
          columns[col].Add(item);
          itemColumns[item] = col;
          placed = true;
          break;
        }
      }

      if (!placed) {
        // Need a new column
        columns.Add([item]);
        itemColumns[item] = columns.Count - 1;
      }
    }

    // Now find connected overlap groups and assign total columns per group
    var result = new List<(SchedulerItem Item, int Column, int TotalColumns)>();
    var processed = new HashSet<SchedulerItem>();

    foreach (var item in items) {
      if (processed.Contains(item))
        continue;

      // Find all items that are connected through overlaps
      var group = new List<SchedulerItem>();
      var queue = new Queue<SchedulerItem>();
      queue.Enqueue(item);

      while (queue.Count > 0) {
        var current = queue.Dequeue();
        if (processed.Contains(current))
          continue;

        processed.Add(current);
        group.Add(current);

        // Find all items that overlap with current
        foreach (var other in items) {
          if (!processed.Contains(other) && this._ItemsOverlap(current, other))
            queue.Enqueue(other);
        }
      }

      // Find max column used in this group
      var maxColumn = group.Max(i => itemColumns[i]) + 1;

      // Add all items in group with their column and total columns
      foreach (var groupItem in group)
        result.Add((groupItem, itemColumns[groupItem], maxColumn));
    }

    return result;
  }

  private bool _ItemsOverlap(SchedulerItem a, SchedulerItem b)
    => a.Start < b.End && b.Start < a.End;

  private void _DrawSchedulerItem(Graphics g, SchedulerItem item, Rectangle bounds) {
    var isSelected = item == this._selectedItem;
    var isHovered = item == this._hoveredItem;

    // Draw background
    var bgColor = isSelected ? Color.FromArgb(Math.Max(0, item.Color.R - 30), Math.Max(0, item.Color.G - 30), Math.Max(0, item.Color.B - 30)) : item.Color;
    using (var brush = new SolidBrush(bgColor)) {
      using var path = this._CreateRoundedRectangle(bounds, 4);
      g.FillPath(brush, path);
    }

    // Draw border
    if (isSelected || isHovered) {
      using var pen = new Pen(Color.FromArgb(Math.Max(0, item.Color.R - 60), Math.Max(0, item.Color.G - 60), Math.Max(0, item.Color.B - 60)), isSelected ? 2f : 1f);
      using var path = this._CreateRoundedRectangle(bounds, 4);
      g.DrawPath(pen, path);
    }

    // Draw text
    var textRect = new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4);
    var textColor = this._GetContrastColor(item.Color);
    TextRenderer.DrawText(g, item.Subject, this.Font, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak);
  }

  private void _DrawMonthView(Graphics g) {
    var bounds = this.ClientRectangle;
    var cellWidth = bounds.Width / 7;
    var cellHeight = (bounds.Height - this._headerHeight) / 6;

    // Draw header
    this._DrawMonthHeader(g, bounds, cellWidth);

    // Draw cells
    var viewStart = this._GetViewStart();
    var firstDayOffset = ((int)viewStart.DayOfWeek + 7) % 7;
    var currentDate = viewStart.AddDays(-firstDayOffset);

    for (var row = 0; row < 6; ++row) {
      for (var col = 0; col < 7; ++col) {
        var x = col * cellWidth;
        var y = this._headerHeight + row * cellHeight;
        var cellRect = new Rectangle(x, y, cellWidth, cellHeight);

        this._DrawMonthCell(g, cellRect, currentDate);
        currentDate = currentDate.AddDays(1);
      }
    }
  }

  private void _DrawMonthHeader(Graphics g, Rectangle bounds, int cellWidth) {
    // Draw navigation header
    var navRect = new Rectangle(0, 0, bounds.Width, 30);
    using (var brush = new SolidBrush(SystemColors.Control))
      g.FillRectangle(brush, navRect);

    var monthYear = this._currentDate.ToString("MMMM yyyy");
    using var font = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold);
    TextRenderer.DrawText(g, monthYear, font, navRect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

    // Draw day headers
    var dayHeaderRect = new Rectangle(0, 30, bounds.Width, 20);
    var dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;

    for (var i = 0; i < 7; ++i) {
      var x = i * cellWidth;
      var rect = new Rectangle(x, 30, cellWidth, 20);
      TextRenderer.DrawText(g, dayNames[i], this.Font, rect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
  }

  private void _DrawMonthCell(Graphics g, Rectangle bounds, DateTime date) {
    var isCurrentMonth = date.Month == this._currentDate.Month;
    var isToday = date.Date == DateTime.Today;

    // Draw border
    using (var pen = new Pen(SystemColors.ControlLight))
      g.DrawRectangle(pen, bounds);

    // Draw background for today
    if (isToday) {
      using var brush = new SolidBrush(Color.FromArgb(230, 240, 255));
      g.FillRectangle(brush, bounds);
    }

    // Draw date number
    var textColor = isCurrentMonth ? this.ForeColor : SystemColors.GrayText;
    var dateRect = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, 16);
    TextRenderer.DrawText(g, date.Day.ToString(), this.Font, dateRect, textColor, TextFormatFlags.Right);

    // Draw items
    var y = bounds.Y + 20;
    var itemHeight = 14;
    var itemsForDay = this._items.Where(i => i.Start.Date <= date && i.End.Date >= date).Take(3).ToList();

    foreach (var item in itemsForDay) {
      if (y + itemHeight > bounds.Bottom - 2)
        break;

      var itemRect = new Rectangle(bounds.X + 2, y, bounds.Width - 4, itemHeight);
      using (var brush = new SolidBrush(item.Color)) {
        using var path = this._CreateRoundedRectangle(itemRect, 2);
        g.FillPath(brush, path);
      }

      var textRect = new Rectangle(itemRect.X + 2, itemRect.Y, itemRect.Width - 4, itemRect.Height);
      TextRenderer.DrawText(g, item.Subject, this.Font, textRect, this._GetContrastColor(item.Color), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

      y += itemHeight + 1;
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

  private Color _GetContrastColor(Color background) {
    var brightness = (background.R * 299 + background.G * 587 + background.B * 114) / 1000;
    return brightness > 128 ? Color.Black : Color.White;
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    if (this._isDragging && this._draggedItem != null && this._allowItemMove) {
      this._HandleItemDrag(e.Location);
      return;
    }

    if (this._isResizing && this._draggedItem != null && this._allowItemResize) {
      this._HandleItemResize(e.Location);
      return;
    }

    var oldHovered = this._hoveredItem;
    this._hoveredItem = this._HitTestItem(e.Location);

    // Check for resize edge
    if (this._hoveredItem != null && this._allowItemResize) {
      var edge = this._HitTestResizeEdge(e.Location, this._hoveredItem);
      this.Cursor = edge != ResizeEdge.None ? Cursors.SizeNS : Cursors.Default;
    } else
      this.Cursor = Cursors.Default;

    if (oldHovered != this._hoveredItem)
      this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseDown(MouseEventArgs e) {
    base.OnMouseDown(e);

    if (e.Button != MouseButtons.Left)
      return;

    var item = this._HitTestItem(e.Location);

    if (item != null) {
      this._selectedItem = item;
      this._draggedItem = item;
      this._dragStartPoint = e.Location;
      this._dragStartTime = item.Start;

      var edge = this._HitTestResizeEdge(e.Location, item);
      if (edge != ResizeEdge.None && this._allowItemResize) {
        this._isResizing = true;
        this._resizeEdge = edge;
      } else if (this._allowItemMove)
        this._isDragging = true;

      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseUp(MouseEventArgs e) {
    base.OnMouseUp(e);

    if (this._isDragging && this._draggedItem != null) {
      this._isDragging = false;
      this.OnItemMoved(new SchedulerItemMovedEventArgs(this._draggedItem, this._dragStartTime, this._draggedItem.Start));
    }

    if (this._isResizing && this._draggedItem != null) {
      this._isResizing = false;
      this.OnItemResized(new SchedulerItemResizedEventArgs(this._draggedItem));
    }

    this._draggedItem = null;
    this._resizeEdge = ResizeEdge.None;
    this.Cursor = Cursors.Default;
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (e.Button == MouseButtons.Left && !this._isDragging && !this._isResizing) {
      var item = this._HitTestItem(e.Location);
      if (item != null)
        this.OnItemClicked(new SchedulerItemEventArgs(item));
    }
  }

  /// <inheritdoc />
  protected override void OnMouseDoubleClick(MouseEventArgs e) {
    base.OnMouseDoubleClick(e);

    var item = this._HitTestItem(e.Location);
    if (item != null)
      this.OnItemDoubleClicked(new SchedulerItemEventArgs(item));
    else if (this._allowItemCreate)
      this._CreateItemAtPoint(e.Location);
  }

  private Dictionary<SchedulerItem, Rectangle> _GetItemBounds() {
    var result = new Dictionary<SchedulerItem, Rectangle>();

    if (this._viewType == SchedulerViewType.Month)
      return result;

    var bounds = new Rectangle(0, 0, this.Width - this._scrollBar.Width, this.Height);
    var dayCount = this._GetDayCount();
    var dayWidth = (bounds.Width - this._timeColumnWidth) / dayCount;
    var contentTop = this._headerHeight + (this._showAllDayArea ? this._allDayAreaHeight : 0);
    var scrollOffset = this._scrollBar.Value;
    var viewStart = this._GetViewStart();
    var viewEnd = this._GetViewEnd();

    var visibleItems = this._items
      .Where(i => !i.AllDay && i.Start < viewEnd && i.End > viewStart)
      .OrderBy(i => i.Start)
      .ThenBy(i => i.End - i.Start)
      .ToList();

    // Group items by day and calculate overlap columns
    var itemsByDay = new Dictionary<int, List<(SchedulerItem Item, int Column, int TotalColumns)>>();

    for (var day = 0; day < dayCount; ++day)
      itemsByDay[day] = [];

    foreach (var item in visibleItems) {
      var itemStart = item.Start < viewStart ? viewStart : item.Start;
      var dayIndex = (int)(itemStart.Date - viewStart).TotalDays;
      if (dayIndex < 0 || dayIndex >= dayCount)
        continue;

      itemsByDay[dayIndex].Add((item, 0, 1));
    }

    // Process each day to assign columns for overlapping items
    foreach (var day in itemsByDay.Keys.ToList()) {
      var dayItems = itemsByDay[day];
      if (dayItems.Count <= 1)
        continue;

      var layoutInfo = this._CalculateOverlapLayout(dayItems.Select(x => x.Item).ToList(), viewStart);
      itemsByDay[day] = layoutInfo;
    }

    // Calculate bounds for all items
    foreach (var day in itemsByDay.Keys) {
      foreach (var (item, column, totalColumns) in itemsByDay[day]) {
        var itemStart = item.Start < viewStart ? viewStart : item.Start;
        var itemEnd = item.End > viewEnd ? viewEnd : item.End;

        var startMinutes = (itemStart.TimeOfDay - this._dayStartTime).TotalMinutes;
        var endMinutes = (itemEnd.TimeOfDay - this._dayStartTime).TotalMinutes;

        if (endMinutes <= 0 || startMinutes >= (this._dayEndTime - this._dayStartTime).TotalMinutes)
          continue;

        startMinutes = Math.Max(0, startMinutes);
        endMinutes = Math.Min((this._dayEndTime - this._dayStartTime).TotalMinutes, endMinutes);

        var availableWidth = dayWidth - 4;
        var itemWidth = availableWidth / totalColumns;
        var x = this._timeColumnWidth + day * dayWidth + 2 + column * itemWidth;

        var y = contentTop + (int)(startMinutes / this._timeSlotMinutes * this._rowHeight) - scrollOffset;
        var height = (int)((endMinutes - startMinutes) / this._timeSlotMinutes * this._rowHeight);

        result[item] = new Rectangle(x, y, itemWidth - 1, Math.Max(this._rowHeight / 2, height));
      }
    }

    return result;
  }

  private SchedulerItem _HitTestItem(Point location) {
    if (this._viewType == SchedulerViewType.Month)
      return null;

    var itemBounds = this._GetItemBounds();

    // Check items in reverse order (topmost first for overlapping)
    foreach (var kvp in itemBounds.Reverse()) {
      if (kvp.Value.Contains(location))
        return kvp.Key;
    }

    return null;
  }

  private ResizeEdge _HitTestResizeEdge(Point location, SchedulerItem item) {
    if (this._viewType == SchedulerViewType.Month)
      return ResizeEdge.None;

    var itemBounds = this._GetItemBounds();
    if (!itemBounds.TryGetValue(item, out var rect))
      return ResizeEdge.None;

    var edgeSize = 6;
    var topEdge = new Rectangle(rect.X, rect.Y, rect.Width, edgeSize);
    var bottomEdge = new Rectangle(rect.X, rect.Y + rect.Height - edgeSize, rect.Width, edgeSize);

    if (topEdge.Contains(location))
      return ResizeEdge.Top;
    if (bottomEdge.Contains(location))
      return ResizeEdge.Bottom;

    return ResizeEdge.None;
  }

  private void _HandleItemDrag(Point location) {
    var time = this._GetTimeAtPoint(location);
    if (time.HasValue) {
      var duration = this._draggedItem.End - this._draggedItem.Start;
      this._draggedItem.Start = time.Value;
      this._draggedItem.End = time.Value + duration;
      this.Invalidate();
    }
  }

  private void _HandleItemResize(Point location) {
    var time = this._GetTimeAtPoint(location);
    if (!time.HasValue)
      return;

    if (this._resizeEdge == ResizeEdge.Top) {
      if (time.Value < this._draggedItem.End.AddMinutes(-this._timeSlotMinutes)) {
        this._draggedItem.Start = time.Value;
        this.Invalidate();
      }
    } else if (this._resizeEdge == ResizeEdge.Bottom) {
      if (time.Value > this._draggedItem.Start.AddMinutes(this._timeSlotMinutes)) {
        this._draggedItem.End = time.Value;
        this.Invalidate();
      }
    }
  }

  private DateTime? _GetTimeAtPoint(Point location) {
    if (this._viewType == SchedulerViewType.Month)
      return null;

    var bounds = new Rectangle(0, 0, this.Width - this._scrollBar.Width, this.Height);
    var dayCount = this._GetDayCount();
    var dayWidth = (bounds.Width - this._timeColumnWidth) / dayCount;
    var contentTop = this._headerHeight + (this._showAllDayArea ? this._allDayAreaHeight : 0);
    var scrollOffset = this._scrollBar.Value;
    var viewStart = this._GetViewStart();

    if (location.X < this._timeColumnWidth || location.Y < contentTop)
      return null;

    var dayIndex = (location.X - this._timeColumnWidth) / dayWidth;
    if (dayIndex < 0 || dayIndex >= dayCount)
      return null;

    var date = viewStart.AddDays(dayIndex);
    var minutesFromTop = ((location.Y - contentTop + scrollOffset) * this._timeSlotMinutes) / this._rowHeight;
    var time = this._dayStartTime.Add(TimeSpan.FromMinutes(minutesFromTop));

    // Snap to time slot
    var snappedMinutes = ((int)time.TotalMinutes / this._timeSlotMinutes) * this._timeSlotMinutes;
    return date.Add(TimeSpan.FromMinutes(snappedMinutes));
  }

  private void _CreateItemAtPoint(Point location) {
    var time = this._GetTimeAtPoint(location);
    if (!time.HasValue)
      return;

    var newItem = new SchedulerItem {
      Id = Guid.NewGuid(),
      Subject = "New Appointment",
      Start = time.Value,
      End = time.Value.AddMinutes(this._timeSlotMinutes * 2),
      Color = Color.DodgerBlue
    };

    this.OnItemCreating(new SchedulerItemEventArgs(newItem));
    this._items.Add(newItem);
    this._selectedItem = newItem;
    this.OnItemCreated(new SchedulerItemEventArgs(newItem));
    this.Invalidate();
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing) {
    if (disposing) {
      this._scrollBar?.Dispose();
    }

    base.Dispose(disposing);
  }
}

/// <summary>
/// Represents an item in a <see cref="SchedulerControl"/>.
/// </summary>
public class SchedulerItem {
  /// <summary>
  /// Gets or sets the unique identifier.
  /// </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
  /// Gets or sets the subject.
  /// </summary>
  public string Subject { get; set; }

  /// <summary>
  /// Gets or sets the description.
  /// </summary>
  public string Description { get; set; }

  /// <summary>
  /// Gets or sets the start time.
  /// </summary>
  public DateTime Start { get; set; }

  /// <summary>
  /// Gets or sets the end time.
  /// </summary>
  public DateTime End { get; set; }

  /// <summary>
  /// Gets or sets whether this is an all-day event.
  /// </summary>
  public bool AllDay { get; set; }

  /// <summary>
  /// Gets or sets the item color.
  /// </summary>
  public Color Color { get; set; } = Color.DodgerBlue;

  /// <summary>
  /// Gets or sets the associated resource.
  /// </summary>
  public SchedulerResource Resource { get; set; }

  /// <summary>
  /// Gets or sets custom data.
  /// </summary>
  public object Tag { get; set; }

  /// <summary>
  /// Gets or sets whether this is a recurring event.
  /// </summary>
  public bool IsRecurring { get; set; }

  /// <summary>
  /// Gets or sets the recurrence pattern in iCalendar RRULE format.
  /// </summary>
  public string RecurrencePattern { get; set; }
}

/// <summary>
/// Represents a resource in a <see cref="SchedulerControl"/>.
/// </summary>
public class SchedulerResource {
  /// <summary>
  /// Gets or sets the unique identifier.
  /// </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>
  /// Gets or sets the name.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Gets or sets the color.
  /// </summary>
  public Color Color { get; set; } = Color.DodgerBlue;

  /// <summary>
  /// Gets or sets the icon.
  /// </summary>
  public Image Icon { get; set; }
}

/// <summary>
/// Specifies the view type for <see cref="SchedulerControl"/>.
/// </summary>
public enum SchedulerViewType {
  /// <summary>
  /// Single day view.
  /// </summary>
  Day,

  /// <summary>
  /// Full week view (Sunday to Saturday).
  /// </summary>
  Week,

  /// <summary>
  /// Work week view (Monday to Friday).
  /// </summary>
  WorkWeek,

  /// <summary>
  /// Month view.
  /// </summary>
  Month,

  /// <summary>
  /// Timeline view.
  /// </summary>
  Timeline
}

/// <summary>
/// Provides data for scheduler item events.
/// </summary>
public class SchedulerItemEventArgs : EventArgs {
  /// <summary>
  /// Gets the scheduler item.
  /// </summary>
  public SchedulerItem Item { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerItemEventArgs"/> class.
  /// </summary>
  public SchedulerItemEventArgs(SchedulerItem item) {
    this.Item = item;
  }
}

/// <summary>
/// Provides data for the item moved event.
/// </summary>
public class SchedulerItemMovedEventArgs : SchedulerItemEventArgs {
  /// <summary>
  /// Gets the original start time.
  /// </summary>
  public DateTime OriginalStart { get; }

  /// <summary>
  /// Gets the new start time.
  /// </summary>
  public DateTime NewStart { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerItemMovedEventArgs"/> class.
  /// </summary>
  public SchedulerItemMovedEventArgs(SchedulerItem item, DateTime originalStart, DateTime newStart)
    : base(item) {
    this.OriginalStart = originalStart;
    this.NewStart = newStart;
  }
}

/// <summary>
/// Provides data for the item resized event.
/// </summary>
public class SchedulerItemResizedEventArgs : SchedulerItemEventArgs {
  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerItemResizedEventArgs"/> class.
  /// </summary>
  public SchedulerItemResizedEventArgs(SchedulerItem item)
    : base(item) {
  }
}

/// <summary>
/// Provides data for date change events.
/// </summary>
public class SchedulerDateEventArgs : EventArgs {
  /// <summary>
  /// Gets the old date.
  /// </summary>
  public DateTime OldDate { get; }

  /// <summary>
  /// Gets the new date.
  /// </summary>
  public DateTime NewDate { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="SchedulerDateEventArgs"/> class.
  /// </summary>
  public SchedulerDateEventArgs(DateTime oldDate, DateTime newDate) {
    this.OldDate = oldDate;
    this.NewDate = newDate;
  }
}
