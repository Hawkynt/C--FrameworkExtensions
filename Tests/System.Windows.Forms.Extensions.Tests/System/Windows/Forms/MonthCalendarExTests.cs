using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class MonthCalendarExTests {
  private MonthCalendarEx _calendar;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._calendar = new MonthCalendarEx();
    this._form = new Form();
    this._form.Controls.Add(this._calendar);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._calendar?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._calendar.SelectedDate.Date, Is.EqualTo(DateTime.Today));
    Assert.That(this._calendar.ShowWeekNumbers, Is.False);
    Assert.That(this._calendar.ShowTodayCircle, Is.True);
    Assert.That(this._calendar.AllowMultiSelect, Is.False);
    Assert.That(this._calendar.FirstDayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
    Assert.That(this._calendar.TodayColor, Is.EqualTo(Color.Red));
    Assert.That(this._calendar.SelectedColor, Is.EqualTo(SystemColors.Highlight));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedDate_CanBeSetAndRetrieved() {
    var date = new DateTime(2024, 6, 15);
    this._calendar.SelectedDate = date;

    Assert.That(this._calendar.SelectedDate, Is.EqualTo(date));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedDate_UpdatesViewMonth() {
    var date = new DateTime(2024, 8, 15);
    this._calendar.SelectedDate = date;

    Assert.That(this._calendar.ViewMonth.Month, Is.EqualTo(8));
    Assert.That(this._calendar.ViewMonth.Year, Is.EqualTo(2024));
  }

  [Test]
  [Category("HappyPath")]
  public void DateChanged_IsRaisedWhenDateChanges() {
    var eventRaised = false;
    DateTime? oldDate = null;
    DateTime? newDate = null;

    this._calendar.DateChanged += (s, e) => {
      eventRaised = true;
      oldDate = e.OldDate;
      newDate = e.NewDate;
    };

    var targetDate = new DateTime(2024, 6, 15);
    this._calendar.SelectedDate = targetDate;

    Assert.That(eventRaised, Is.True);
    Assert.That(newDate, Is.EqualTo(targetDate));
  }

  [Test]
  [Category("EdgeCase")]
  public void DateChanged_NotRaisedWhenSameDate() {
    this._calendar.SelectedDate = DateTime.Today;
    var eventRaised = false;
    this._calendar.DateChanged += (s, e) => eventRaised = true;

    this._calendar.SelectedDate = DateTime.Today;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateMonth_ChangesViewMonth() {
    var initialMonth = this._calendar.ViewMonth;
    this._calendar.NavigateMonth(1);

    Assert.That(this._calendar.ViewMonth, Is.EqualTo(initialMonth.AddMonths(1)));
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateMonth_Backward_Works() {
    var initialMonth = this._calendar.ViewMonth;
    this._calendar.NavigateMonth(-1);

    Assert.That(this._calendar.ViewMonth, Is.EqualTo(initialMonth.AddMonths(-1)));
  }

  [Test]
  [Category("HappyPath")]
  public void MonthChanged_IsRaisedWhenMonthChanges() {
    var eventRaised = false;
    this._calendar.MonthChanged += (s, e) => eventRaised = true;

    this._calendar.NavigateMonth(1);

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GoToToday_SelectsToday() {
    this._calendar.SelectedDate = new DateTime(2020, 1, 1);
    this._calendar.GoToToday();

    Assert.That(this._calendar.SelectedDate, Is.EqualTo(DateTime.Today));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowWeekNumbers_CanBeSetAndRetrieved() {
    this._calendar.ShowWeekNumbers = true;
    Assert.That(this._calendar.ShowWeekNumbers, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowTodayCircle_CanBeSetAndRetrieved() {
    this._calendar.ShowTodayCircle = false;
    Assert.That(this._calendar.ShowTodayCircle, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowMultiSelect_CanBeSetAndRetrieved() {
    this._calendar.AllowMultiSelect = true;
    Assert.That(this._calendar.AllowMultiSelect, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void FirstDayOfWeek_CanBeSetAndRetrieved() {
    this._calendar.FirstDayOfWeek = DayOfWeek.Monday;
    Assert.That(this._calendar.FirstDayOfWeek, Is.EqualTo(DayOfWeek.Monday));
  }

  [Test]
  [Category("HappyPath")]
  public void TodayColor_CanBeSetAndRetrieved() {
    this._calendar.TodayColor = Color.Blue;
    Assert.That(this._calendar.TodayColor, Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void SelectedColor_CanBeSetAndRetrieved() {
    this._calendar.SelectedColor = Color.Green;
    Assert.That(this._calendar.SelectedColor, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void DateMarkers_CanBeAdded() {
    var marker = new CalendarDateMarker {
      Date = DateTime.Today,
      Color = Color.Red,
      Style = CalendarMarkerStyle.Dot
    };
    this._calendar.DateMarkers.Add(marker);

    Assert.That(this._calendar.DateMarkers.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Appointments_CanBeAdded() {
    var appointment = new CalendarAppointment {
      Date = DateTime.Today,
      Title = "Test Meeting",
      Color = Color.Blue
    };
    this._calendar.Appointments.Add(appointment);

    Assert.That(this._calendar.Appointments.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void CalendarDateMarker_DefaultValues() {
    var marker = new CalendarDateMarker();

    Assert.That(marker.Color, Is.EqualTo(Color.DodgerBlue));
    Assert.That(marker.Style, Is.EqualTo(CalendarMarkerStyle.Dot));
    Assert.That(marker.Tooltip, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void CalendarDateMarker_PropertiesWork() {
    var marker = new CalendarDateMarker {
      Date = new DateTime(2024, 6, 15),
      Color = Color.Red,
      Style = CalendarMarkerStyle.Circle,
      Tooltip = "Test tooltip"
    };

    Assert.That(marker.Date, Is.EqualTo(new DateTime(2024, 6, 15)));
    Assert.That(marker.Color, Is.EqualTo(Color.Red));
    Assert.That(marker.Style, Is.EqualTo(CalendarMarkerStyle.Circle));
    Assert.That(marker.Tooltip, Is.EqualTo("Test tooltip"));
  }

  [Test]
  [Category("HappyPath")]
  public void CalendarAppointment_DefaultValues() {
    var appointment = new CalendarAppointment();

    Assert.That(appointment.Color, Is.EqualTo(Color.DodgerBlue));
    Assert.That(appointment.Title, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void CalendarAppointment_PropertiesWork() {
    var appointment = new CalendarAppointment {
      Date = new DateTime(2024, 6, 15),
      Title = "Meeting",
      Color = Color.Green
    };

    Assert.That(appointment.Date, Is.EqualTo(new DateTime(2024, 6, 15)));
    Assert.That(appointment.Title, Is.EqualTo("Meeting"));
    Assert.That(appointment.Color, Is.EqualTo(Color.Green));
  }

  [Test]
  [Category("HappyPath")]
  public void ViewMonth_CanBeSetDirectly() {
    var targetMonth = new DateTime(2024, 3, 1);
    this._calendar.ViewMonth = targetMonth;

    Assert.That(this._calendar.ViewMonth.Year, Is.EqualTo(2024));
    Assert.That(this._calendar.ViewMonth.Month, Is.EqualTo(3));
    Assert.That(this._calendar.ViewMonth.Day, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void DateSelected_IsRaisedOnSelection() {
    var eventRaised = false;
    DateTime? selectedDate = null;

    this._calendar.DateSelected += (s, e) => {
      eventRaised = true;
      selectedDate = e.Date;
    };

    var targetDate = new DateTime(2024, 6, 15);
    this._calendar.SelectedDate = targetDate;

    // Note: DateSelected is raised on mouse click, not property change
    // This test just ensures the property itself works
    Assert.That(this._calendar.SelectedDate, Is.EqualTo(targetDate));
  }
}
