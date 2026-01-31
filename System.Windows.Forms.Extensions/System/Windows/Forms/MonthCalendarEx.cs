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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace System.Windows.Forms;

/// <summary>
/// An enhanced month calendar with appointment indicators and custom rendering.
/// </summary>
/// <example>
/// <code>
/// var calendar = new MonthCalendarEx {
///   ShowWeekNumbers = true,
///   ShowTodayCircle = true
/// };
/// calendar.DateMarkers.Add(new CalendarDateMarker {
///   Date = DateTime.Today.AddDays(3),
///   Color = Color.Red,
///   Tooltip = "Meeting"
/// });
/// calendar.DateSelected += (s, e) => Console.WriteLine($"Selected: {e.Date}");
/// </code>
/// </example>
public class MonthCalendarEx : Control {
  private DateTime _selectedDate = DateTime.Today;
  private readonly List<DateTime> _selectedDates = [];
  private DateTime _viewMonth;
  private bool _showWeekNumbers;
  private bool _showTodayCircle = true;
  private bool _allowMultiSelect;
  private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;
  private readonly List<CalendarDateMarker> _dateMarkers = [];
  private readonly List<CalendarAppointment> _appointments = [];
  private Color _todayColor = Color.Red;
  private Color _selectedColor = SystemColors.Highlight;

  private DateTime? _hoveredDate;
  private Rectangle _headerRect;
  private Rectangle _prevMonthRect;
  private Rectangle _nextMonthRect;
  private Rectangle[,] _dayCellRects;
  private DateTime[,] _dayCellDates;

  private const int CellWidth = 32;
  private const int CellHeight = 28;
  private const int HeaderHeight = 32;
  private const int DayHeaderHeight = 24;
  private const int WeekNumberWidth = 28;

  /// <summary>
  /// Occurs when a date is selected.
  /// </summary>
  [Category("Action")]
  [Description("Occurs when a date is selected.")]
  public event EventHandler<DateSelectedEventArgs> DateSelected;

  /// <summary>
  /// Occurs when the selected date changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the selected date changes.")]
  public event EventHandler<DateChangedEventArgs> DateChanged;

  /// <summary>
  /// Occurs when the view month changes.
  /// </summary>
  [Category("Property Changed")]
  [Description("Occurs when the view month changes.")]
  public event EventHandler<MonthChangedEventArgs> MonthChanged;

  /// <summary>
  /// Initializes a new instance of the <see cref="MonthCalendarEx"/> class.
  /// </summary>
  public MonthCalendarEx() {
    this.SetStyle(
      ControlStyles.AllPaintingInWmPaint
      | ControlStyles.UserPaint
      | ControlStyles.OptimizedDoubleBuffer
      | ControlStyles.ResizeRedraw
      | ControlStyles.SupportsTransparentBackColor
      | ControlStyles.Selectable,
      true
    );

    this._viewMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    this._dayCellRects = new Rectangle[6, 7];
    this._dayCellDates = new DateTime[6, 7];
    this._UpdateSize();
    this.BackColor = SystemColors.Window;
    this.ForeColor = SystemColors.WindowText;
  }

  /// <summary>
  /// Gets or sets the selected date.
  /// </summary>
  [Category("Behavior")]
  [Description("The currently selected date.")]
  public DateTime SelectedDate {
    get => this._selectedDate;
    set {
      if (this._selectedDate == value.Date)
        return;

      var oldDate = this._selectedDate;
      this._selectedDate = value.Date;

      if (this._selectedDate.Year != this._viewMonth.Year || this._selectedDate.Month != this._viewMonth.Month)
        this._viewMonth = new DateTime(this._selectedDate.Year, this._selectedDate.Month, 1);

      this.OnDateChanged(new DateChangedEventArgs(oldDate, this._selectedDate));
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets the selected dates when multi-select is enabled.
  /// </summary>
  [Browsable(false)]
  public IReadOnlyList<DateTime> SelectedDates => this._selectedDates.AsIReadOnlyList();

  /// <summary>
  /// Gets or sets the displayed month.
  /// </summary>
  [Browsable(false)]
  public DateTime ViewMonth {
    get => this._viewMonth;
    set {
      var newMonth = new DateTime(value.Year, value.Month, 1);
      if (this._viewMonth == newMonth)
        return;

      var oldMonth = this._viewMonth;
      this._viewMonth = newMonth;
      this.OnMonthChanged(new MonthChangedEventArgs(oldMonth, this._viewMonth));
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show week numbers.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show week numbers.")]
  [DefaultValue(false)]
  public bool ShowWeekNumbers {
    get => this._showWeekNumbers;
    set {
      if (this._showWeekNumbers == value)
        return;
      this._showWeekNumbers = value;
      this._UpdateSize();
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether to show a circle around today's date.
  /// </summary>
  [Category("Appearance")]
  [Description("Whether to show a circle around today's date.")]
  [DefaultValue(true)]
  public bool ShowTodayCircle {
    get => this._showTodayCircle;
    set {
      if (this._showTodayCircle == value)
        return;
      this._showTodayCircle = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets whether multiple dates can be selected.
  /// </summary>
  [Category("Behavior")]
  [Description("Whether multiple dates can be selected.")]
  [DefaultValue(false)]
  public bool AllowMultiSelect {
    get => this._allowMultiSelect;
    set => this._allowMultiSelect = value;
  }

  /// <summary>
  /// Gets or sets the first day of the week.
  /// </summary>
  [Category("Behavior")]
  [Description("The first day of the week.")]
  [DefaultValue(DayOfWeek.Sunday)]
  public DayOfWeek FirstDayOfWeek {
    get => this._firstDayOfWeek;
    set {
      if (this._firstDayOfWeek == value)
        return;
      this._firstDayOfWeek = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets the collection of date markers.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList<CalendarDateMarker> DateMarkers => this._dateMarkers;

  /// <summary>
  /// Gets the collection of appointments.
  /// </summary>
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public IList<CalendarAppointment> Appointments => this._appointments;

  /// <summary>
  /// Gets or sets the color for today's date circle.
  /// </summary>
  [Category("Appearance")]
  [Description("The color for today's date circle.")]
  public Color TodayColor {
    get => this._todayColor;
    set {
      if (this._todayColor == value)
        return;
      this._todayColor = value;
      this.Invalidate();
    }
  }

  /// <summary>
  /// Gets or sets the color for selected dates.
  /// </summary>
  [Category("Appearance")]
  [Description("The color for selected dates.")]
  public Color SelectedColor {
    get => this._selectedColor;
    set {
      if (this._selectedColor == value)
        return;
      this._selectedColor = value;
      this.Invalidate();
    }
  }

  private bool ShouldSerializeTodayColor() => this._todayColor != Color.Red;
  private void ResetTodayColor() => this._todayColor = Color.Red;
  private bool ShouldSerializeSelectedColor() => this._selectedColor != SystemColors.Highlight;
  private void ResetSelectedColor() => this._selectedColor = SystemColors.Highlight;

  /// <summary>
  /// Navigates to a different month.
  /// </summary>
  /// <param name="offset">The number of months to navigate (positive for forward, negative for backward).</param>
  public void NavigateMonth(int offset) {
    this.ViewMonth = this._viewMonth.AddMonths(offset);
  }

  /// <summary>
  /// Navigates to today's date.
  /// </summary>
  public void GoToToday() {
    this.SelectedDate = DateTime.Today;
  }

  /// <summary>
  /// Raises the <see cref="DateSelected"/> event.
  /// </summary>
  protected virtual void OnDateSelected(DateSelectedEventArgs e) => this.DateSelected?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="DateChanged"/> event.
  /// </summary>
  protected virtual void OnDateChanged(DateChangedEventArgs e) => this.DateChanged?.Invoke(this, e);

  /// <summary>
  /// Raises the <see cref="MonthChanged"/> event.
  /// </summary>
  protected virtual void OnMonthChanged(MonthChangedEventArgs e) => this.MonthChanged?.Invoke(this, e);

  private void _UpdateSize() {
    var weekNumberOffset = this._showWeekNumbers ? WeekNumberWidth : 0;
    this.Size = new Size(weekNumberOffset + 7 * CellWidth + 2, HeaderHeight + DayHeaderHeight + 6 * CellHeight + 2);
  }

  /// <inheritdoc />
  protected override void OnPaint(PaintEventArgs e) {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

    var bounds = this.ClientRectangle;

    // Draw background
    using (var brush = new SolidBrush(this.BackColor))
      g.FillRectangle(brush, bounds);

    // Draw border
    using (var pen = new Pen(SystemColors.ControlDark))
      g.DrawRectangle(pen, 0, 0, bounds.Width - 1, bounds.Height - 1);

    this._DrawHeader(g, bounds);
    this._DrawDayHeaders(g, bounds);
    this._DrawDayCells(g, bounds);
  }

  private void _DrawHeader(Graphics g, Rectangle bounds) {
    var weekNumberOffset = this._showWeekNumbers ? WeekNumberWidth : 0;
    this._headerRect = new Rectangle(1, 1, bounds.Width - 2, HeaderHeight);

    // Draw header background
    using (var brush = new SolidBrush(SystemColors.Control))
      g.FillRectangle(brush, this._headerRect);

    // Draw navigation arrows
    var arrowSize = 16;
    var arrowPadding = 8;

    this._prevMonthRect = new Rectangle(arrowPadding, (HeaderHeight - arrowSize) / 2, arrowSize, arrowSize);
    this._nextMonthRect = new Rectangle(bounds.Width - arrowSize - arrowPadding, (HeaderHeight - arrowSize) / 2, arrowSize, arrowSize);

    using (var pen = new Pen(this.ForeColor, 2f)) {
      // Left arrow
      g.DrawLine(pen, this._prevMonthRect.Right - 4, this._prevMonthRect.Y + 4, this._prevMonthRect.X + 4, this._prevMonthRect.Y + arrowSize / 2);
      g.DrawLine(pen, this._prevMonthRect.X + 4, this._prevMonthRect.Y + arrowSize / 2, this._prevMonthRect.Right - 4, this._prevMonthRect.Bottom - 4);

      // Right arrow
      g.DrawLine(pen, this._nextMonthRect.X + 4, this._nextMonthRect.Y + 4, this._nextMonthRect.Right - 4, this._nextMonthRect.Y + arrowSize / 2);
      g.DrawLine(pen, this._nextMonthRect.Right - 4, this._nextMonthRect.Y + arrowSize / 2, this._nextMonthRect.X + 4, this._nextMonthRect.Bottom - 4);
    }

    // Draw month/year text
    var monthYearText = this._viewMonth.ToString("MMMM yyyy");
    using var font = new Font(this.Font.FontFamily, this.Font.Size + 1, FontStyle.Bold);
    var textRect = new Rectangle(this._prevMonthRect.Right + 8, 0, this._nextMonthRect.Left - this._prevMonthRect.Right - 16, HeaderHeight);
    TextRenderer.DrawText(g, monthYearText, font, textRect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
  }

  private void _DrawDayHeaders(Graphics g, Rectangle bounds) {
    var weekNumberOffset = this._showWeekNumbers ? WeekNumberWidth : 0;
    var y = HeaderHeight + 1;

    // Draw week number header if enabled
    if (this._showWeekNumbers) {
      var wnRect = new Rectangle(1, y, WeekNumberWidth, DayHeaderHeight);
      TextRenderer.DrawText(g, "Wk", this.Font, wnRect, SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // Draw day headers
    var culture = CultureInfo.CurrentCulture;
    for (var i = 0; i < 7; ++i) {
      var dayOfWeek = (DayOfWeek)(((int)this._firstDayOfWeek + i) % 7);
      var dayName = culture.DateTimeFormat.AbbreviatedDayNames[(int)dayOfWeek];

      var dayRect = new Rectangle(weekNumberOffset + 1 + i * CellWidth, y, CellWidth, DayHeaderHeight);
      TextRenderer.DrawText(g, dayName, this.Font, dayRect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // Draw separator line
    using var pen = new Pen(SystemColors.ControlDark);
    g.DrawLine(pen, 1, y + DayHeaderHeight - 1, bounds.Width - 2, y + DayHeaderHeight - 1);
  }

  private void _DrawDayCells(Graphics g, Rectangle bounds) {
    var weekNumberOffset = this._showWeekNumbers ? WeekNumberWidth : 0;
    var startY = HeaderHeight + DayHeaderHeight + 1;

    var firstDayOfMonth = this._viewMonth;
    var daysInMonth = DateTime.DaysInMonth(this._viewMonth.Year, this._viewMonth.Month);

    // Calculate starting position
    var firstDayOffset = ((int)firstDayOfMonth.DayOfWeek - (int)this._firstDayOfWeek + 7) % 7;

    // Calculate first date to display
    var currentDate = firstDayOfMonth.AddDays(-firstDayOffset);

    for (var row = 0; row < 6; ++row) {
      var y = startY + row * CellHeight;

      // Draw week number if enabled
      if (this._showWeekNumbers) {
        var weekNumber = this._GetWeekNumber(currentDate);
        var wnRect = new Rectangle(1, y, WeekNumberWidth, CellHeight);
        TextRenderer.DrawText(g, weekNumber.ToString(), this.Font, wnRect, SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }

      for (var col = 0; col < 7; ++col) {
        var x = weekNumberOffset + 1 + col * CellWidth;
        var cellRect = new Rectangle(x, y, CellWidth, CellHeight);

        this._dayCellRects[row, col] = cellRect;
        this._dayCellDates[row, col] = currentDate;

        this._DrawDayCell(g, cellRect, currentDate);
        currentDate = currentDate.AddDays(1);
      }
    }
  }

  private void _DrawDayCell(Graphics g, Rectangle bounds, DateTime date) {
    var isCurrentMonth = date.Month == this._viewMonth.Month;
    var isToday = date.Date == DateTime.Today;
    var isSelected = date.Date == this._selectedDate || (this._allowMultiSelect && this._selectedDates.Contains(date.Date));
    var isHovered = this._hoveredDate.HasValue && date.Date == this._hoveredDate.Value;

    // Draw selection/hover background
    if (isSelected) {
      using var brush = new SolidBrush(this._selectedColor);
      var selRect = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
      this._FillRoundedRectangle(g, brush, selRect, 4);
    } else if (isHovered) {
      using var brush = new SolidBrush(Color.FromArgb(50, this._selectedColor));
      var hoverRect = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4);
      this._FillRoundedRectangle(g, brush, hoverRect, 4);
    }

    // Draw today circle
    if (isToday && this._showTodayCircle) {
      using var pen = new Pen(this._todayColor, 2f);
      var circleRect = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
      g.DrawEllipse(pen, circleRect);
    }

    // Draw date text
    var textColor = isSelected ? Color.White : (isCurrentMonth ? this.ForeColor : SystemColors.GrayText);
    TextRenderer.DrawText(g, date.Day.ToString(), this.Font, bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

    // Draw markers
    this._DrawMarkers(g, bounds, date);
  }

  private void _DrawMarkers(Graphics g, Rectangle cellBounds, DateTime date) {
    var markers = new List<CalendarDateMarker>();
    var appointments = new List<CalendarAppointment>();

    foreach (var marker in this._dateMarkers)
      if (marker.Date.Date == date.Date)
        markers.Add(marker);

    foreach (var appt in this._appointments)
      if (appt.Date.Date == date.Date)
        appointments.Add(appt);

    if (markers.Count == 0 && appointments.Count == 0)
      return;

    // Draw up to 3 markers at the bottom of the cell
    var markerY = cellBounds.Bottom - 6;
    var markerCount = Math.Min(markers.Count + appointments.Count, 3);
    var totalWidth = markerCount * 6 + (markerCount - 1) * 2;
    var startX = cellBounds.X + (cellBounds.Width - totalWidth) / 2;

    var index = 0;
    foreach (var marker in markers) {
      if (index >= 3)
        break;

      var markerRect = new Rectangle(startX + index * 8, markerY, 5, 5);

      switch (marker.Style) {
        case CalendarMarkerStyle.Dot:
          using (var brush = new SolidBrush(marker.Color))
            g.FillEllipse(brush, markerRect);
          break;
        case CalendarMarkerStyle.Circle:
          using (var pen = new Pen(marker.Color, 1.5f))
            g.DrawEllipse(pen, markerRect);
          break;
        case CalendarMarkerStyle.Fill:
          using (var brush = new SolidBrush(Color.FromArgb(50, marker.Color)))
            g.FillRectangle(brush, cellBounds);
          break;
        case CalendarMarkerStyle.Underline:
          using (var pen = new Pen(marker.Color, 2f))
            g.DrawLine(pen, cellBounds.X + 4, cellBounds.Bottom - 3, cellBounds.Right - 4, cellBounds.Bottom - 3);
          break;
      }

      ++index;
    }

    foreach (var appt in appointments) {
      if (index >= 3)
        break;

      var markerRect = new Rectangle(startX + index * 8, markerY, 5, 5);
      using var brush = new SolidBrush(appt.Color);
      g.FillEllipse(brush, markerRect);

      ++index;
    }
  }

  private void _FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius) {
    using var path = new GraphicsPath();
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

    g.FillPath(brush, path);
  }

  private int _GetWeekNumber(DateTime date) {
    var culture = CultureInfo.CurrentCulture;
    return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, this._firstDayOfWeek);
  }

  /// <inheritdoc />
  protected override void OnMouseMove(MouseEventArgs e) {
    base.OnMouseMove(e);

    var oldHovered = this._hoveredDate;
    this._hoveredDate = this._HitTestDate(e.Location);

    if (oldHovered != this._hoveredDate)
      this.Invalidate();
  }

  /// <inheritdoc />
  protected override void OnMouseLeave(EventArgs e) {
    base.OnMouseLeave(e);

    if (this._hoveredDate.HasValue) {
      this._hoveredDate = null;
      this.Invalidate();
    }
  }

  /// <inheritdoc />
  protected override void OnMouseClick(MouseEventArgs e) {
    base.OnMouseClick(e);

    if (e.Button != MouseButtons.Left)
      return;

    // Check navigation arrows
    if (this._prevMonthRect.Contains(e.Location)) {
      this.NavigateMonth(-1);
      return;
    }

    if (this._nextMonthRect.Contains(e.Location)) {
      this.NavigateMonth(1);
      return;
    }

    // Check date cells
    var clickedDate = this._HitTestDate(e.Location);
    if (clickedDate.HasValue) {
      if (this._allowMultiSelect && (ModifierKeys & Keys.Control) != 0) {
        if (this._selectedDates.Contains(clickedDate.Value))
          this._selectedDates.Remove(clickedDate.Value);
        else
          this._selectedDates.Add(clickedDate.Value);
        this.Invalidate();
      } else {
        this._selectedDates.Clear();
        this.SelectedDate = clickedDate.Value;
      }

      this.OnDateSelected(new DateSelectedEventArgs(clickedDate.Value));
    }
  }

  private DateTime? _HitTestDate(Point location) {
    for (var row = 0; row < 6; ++row) {
      for (var col = 0; col < 7; ++col) {
        if (this._dayCellRects[row, col].Contains(location))
          return this._dayCellDates[row, col];
      }
    }

    return null;
  }

  /// <inheritdoc />
  protected override void OnKeyDown(KeyEventArgs e) {
    base.OnKeyDown(e);

    switch (e.KeyCode) {
      case Keys.Left:
        this.SelectedDate = this._selectedDate.AddDays(-1);
        e.Handled = true;
        break;
      case Keys.Right:
        this.SelectedDate = this._selectedDate.AddDays(1);
        e.Handled = true;
        break;
      case Keys.Up:
        this.SelectedDate = this._selectedDate.AddDays(-7);
        e.Handled = true;
        break;
      case Keys.Down:
        this.SelectedDate = this._selectedDate.AddDays(7);
        e.Handled = true;
        break;
      case Keys.PageUp:
        this.NavigateMonth(-1);
        e.Handled = true;
        break;
      case Keys.PageDown:
        this.NavigateMonth(1);
        e.Handled = true;
        break;
      case Keys.Home:
        this.GoToToday();
        e.Handled = true;
        break;
    }
  }
}

/// <summary>
/// Represents a marker for a specific date in a <see cref="MonthCalendarEx"/>.
/// </summary>
public class CalendarDateMarker {
  /// <summary>
  /// Gets or sets the date to mark.
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Gets or sets the marker color.
  /// </summary>
  public Color Color { get; set; } = Color.DodgerBlue;

  /// <summary>
  /// Gets or sets the tooltip text.
  /// </summary>
  public string Tooltip { get; set; }

  /// <summary>
  /// Gets or sets the marker style.
  /// </summary>
  public CalendarMarkerStyle Style { get; set; } = CalendarMarkerStyle.Dot;
}

/// <summary>
/// Represents an appointment in a <see cref="MonthCalendarEx"/>.
/// </summary>
public class CalendarAppointment {
  /// <summary>
  /// Gets or sets the appointment date.
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Gets or sets the appointment title.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// Gets or sets the appointment color.
  /// </summary>
  public Color Color { get; set; } = Color.DodgerBlue;
}

/// <summary>
/// Specifies the marker style for <see cref="CalendarDateMarker"/>.
/// </summary>
public enum CalendarMarkerStyle {
  /// <summary>
  /// A small filled dot.
  /// </summary>
  Dot,

  /// <summary>
  /// A small circle outline.
  /// </summary>
  Circle,

  /// <summary>
  /// Fill the entire cell with a transparent color.
  /// </summary>
  Fill,

  /// <summary>
  /// An underline below the date.
  /// </summary>
  Underline
}

/// <summary>
/// Provides data for the <see cref="MonthCalendarEx.DateSelected"/> event.
/// </summary>
public class DateSelectedEventArgs : EventArgs {
  /// <summary>
  /// Gets the selected date.
  /// </summary>
  public DateTime Date { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DateSelectedEventArgs"/> class.
  /// </summary>
  public DateSelectedEventArgs(DateTime date) {
    this.Date = date;
  }
}

/// <summary>
/// Provides data for the <see cref="MonthCalendarEx.DateChanged"/> event.
/// </summary>
public class DateChangedEventArgs : EventArgs {
  /// <summary>
  /// Gets the old date.
  /// </summary>
  public DateTime OldDate { get; }

  /// <summary>
  /// Gets the new date.
  /// </summary>
  public DateTime NewDate { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="DateChangedEventArgs"/> class.
  /// </summary>
  public DateChangedEventArgs(DateTime oldDate, DateTime newDate) {
    this.OldDate = oldDate;
    this.NewDate = newDate;
  }
}

/// <summary>
/// Provides data for the <see cref="MonthCalendarEx.MonthChanged"/> event.
/// </summary>
public class MonthChangedEventArgs : EventArgs {
  /// <summary>
  /// Gets the old month.
  /// </summary>
  public DateTime OldMonth { get; }

  /// <summary>
  /// Gets the new month.
  /// </summary>
  public DateTime NewMonth { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="MonthChangedEventArgs"/> class.
  /// </summary>
  public MonthChangedEventArgs(DateTime oldMonth, DateTime newMonth) {
    this.OldMonth = oldMonth;
    this.NewMonth = newMonth;
  }
}
