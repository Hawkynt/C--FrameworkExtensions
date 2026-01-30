using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class SchedulerControlTests {
  private SchedulerControl _scheduler;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._scheduler = new SchedulerControl();
    this._form = new Form();
    this._form.Controls.Add(this._scheduler);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._scheduler?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._scheduler.ViewType, Is.EqualTo(SchedulerViewType.Week));
    Assert.That(this._scheduler.CurrentDate.Date, Is.EqualTo(DateTime.Today));
    Assert.That(this._scheduler.DayStartTime, Is.EqualTo(TimeSpan.FromHours(8)));
    Assert.That(this._scheduler.DayEndTime, Is.EqualTo(TimeSpan.FromHours(18)));
    Assert.That(this._scheduler.TimeSlotMinutes, Is.EqualTo(30));
    Assert.That(this._scheduler.ShowAllDayArea, Is.True);
    Assert.That(this._scheduler.AllowItemResize, Is.True);
    Assert.That(this._scheduler.AllowItemMove, Is.True);
    Assert.That(this._scheduler.AllowItemCreate, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ViewType_CanBeSetAndRetrieved() {
    this._scheduler.ViewType = SchedulerViewType.Day;
    Assert.That(this._scheduler.ViewType, Is.EqualTo(SchedulerViewType.Day));

    this._scheduler.ViewType = SchedulerViewType.Month;
    Assert.That(this._scheduler.ViewType, Is.EqualTo(SchedulerViewType.Month));
  }

  [Test]
  [Category("HappyPath")]
  public void CurrentDate_CanBeSetAndRetrieved() {
    var date = new DateTime(2024, 6, 15);
    this._scheduler.CurrentDate = date;

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(date));
  }

  [Test]
  [Category("HappyPath")]
  public void DateChanged_IsRaisedWhenDateChanges() {
    var eventRaised = false;
    this._scheduler.DateChanged += (s, e) => eventRaised = true;

    this._scheduler.CurrentDate = DateTime.Today.AddDays(5);

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void DateChanged_NotRaisedWhenSameDate() {
    this._scheduler.CurrentDate = DateTime.Today;
    var eventRaised = false;
    this._scheduler.DateChanged += (s, e) => eventRaised = true;

    this._scheduler.CurrentDate = DateTime.Today;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Items_CanBeAdded() {
    var item = new SchedulerItem {
      Subject = "Meeting",
      Start = DateTime.Today.AddHours(10),
      End = DateTime.Today.AddHours(11)
    };
    this._scheduler.Items.Add(item);

    Assert.That(this._scheduler.Items.Count, Is.EqualTo(1));
    Assert.That(this._scheduler.Items[0].Subject, Is.EqualTo("Meeting"));
  }

  [Test]
  [Category("HappyPath")]
  public void Resources_CanBeAdded() {
    var resource = new SchedulerResource {
      Name = "Room A",
      Color = Color.Blue
    };
    this._scheduler.Resources.Add(resource);

    Assert.That(this._scheduler.Resources.Count, Is.EqualTo(1));
    Assert.That(this._scheduler.Resources[0].Name, Is.EqualTo("Room A"));
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateDate_Day_Works() {
    this._scheduler.ViewType = SchedulerViewType.Day;
    var initialDate = this._scheduler.CurrentDate;

    this._scheduler.NavigateDate(1);

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(initialDate.AddDays(1)));
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateDate_Week_Works() {
    this._scheduler.ViewType = SchedulerViewType.Week;
    var initialDate = this._scheduler.CurrentDate;

    this._scheduler.NavigateDate(1);

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(initialDate.AddDays(7)));
  }

  [Test]
  [Category("HappyPath")]
  public void NavigateDate_Month_Works() {
    this._scheduler.ViewType = SchedulerViewType.Month;
    var initialDate = this._scheduler.CurrentDate;

    this._scheduler.NavigateDate(1);

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(initialDate.AddMonths(1)));
  }

  [Test]
  [Category("HappyPath")]
  public void GoToToday_SetsCurrentDateToToday() {
    this._scheduler.CurrentDate = new DateTime(2020, 1, 1);
    this._scheduler.GoToToday();

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(DateTime.Today));
  }

  [Test]
  [Category("HappyPath")]
  public void GoToDate_SetsCurrentDate() {
    var targetDate = new DateTime(2024, 12, 25);
    this._scheduler.GoToDate(targetDate);

    Assert.That(this._scheduler.CurrentDate, Is.EqualTo(targetDate));
  }

  [Test]
  [Category("HappyPath")]
  public void ViewStart_DayView_ReturnsCorrectDate() {
    this._scheduler.ViewType = SchedulerViewType.Day;
    this._scheduler.CurrentDate = new DateTime(2024, 6, 15);

    Assert.That(this._scheduler.ViewStart, Is.EqualTo(new DateTime(2024, 6, 15)));
  }

  [Test]
  [Category("HappyPath")]
  public void ViewStart_WeekView_ReturnsSunday() {
    this._scheduler.ViewType = SchedulerViewType.Week;
    this._scheduler.CurrentDate = new DateTime(2024, 6, 13); // Thursday

    // Should return the previous Sunday
    Assert.That(this._scheduler.ViewStart.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
  }

  [Test]
  [Category("HappyPath")]
  public void ViewStart_MonthView_ReturnsFirstOfMonth() {
    this._scheduler.ViewType = SchedulerViewType.Month;
    this._scheduler.CurrentDate = new DateTime(2024, 6, 15);

    Assert.That(this._scheduler.ViewStart, Is.EqualTo(new DateTime(2024, 6, 1)));
  }

  [Test]
  [Category("HappyPath")]
  public void DayStartTime_CanBeSetAndRetrieved() {
    this._scheduler.DayStartTime = TimeSpan.FromHours(7);
    Assert.That(this._scheduler.DayStartTime, Is.EqualTo(TimeSpan.FromHours(7)));
  }

  [Test]
  [Category("HappyPath")]
  public void DayEndTime_CanBeSetAndRetrieved() {
    this._scheduler.DayEndTime = TimeSpan.FromHours(20);
    Assert.That(this._scheduler.DayEndTime, Is.EqualTo(TimeSpan.FromHours(20)));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSlotMinutes_CanBeSetAndRetrieved() {
    this._scheduler.TimeSlotMinutes = 15;
    Assert.That(this._scheduler.TimeSlotMinutes, Is.EqualTo(15));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeSlotMinutes_HasMinimumValue() {
    this._scheduler.TimeSlotMinutes = 1;
    Assert.That(this._scheduler.TimeSlotMinutes, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeSlotMinutes_HasMaximumValue() {
    this._scheduler.TimeSlotMinutes = 120;
    Assert.That(this._scheduler.TimeSlotMinutes, Is.EqualTo(60));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowAllDayArea_CanBeSetAndRetrieved() {
    this._scheduler.ShowAllDayArea = false;
    Assert.That(this._scheduler.ShowAllDayArea, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowItemResize_CanBeSetAndRetrieved() {
    this._scheduler.AllowItemResize = false;
    Assert.That(this._scheduler.AllowItemResize, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowItemMove_CanBeSetAndRetrieved() {
    this._scheduler.AllowItemMove = false;
    Assert.That(this._scheduler.AllowItemMove, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowItemCreate_CanBeSetAndRetrieved() {
    this._scheduler.AllowItemCreate = false;
    Assert.That(this._scheduler.AllowItemCreate, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void WorkingHoursColor_CanBeSetAndRetrieved() {
    this._scheduler.WorkingHoursColor = Color.LightYellow;
    Assert.That(this._scheduler.WorkingHoursColor, Is.EqualTo(Color.LightYellow));
  }

  [Test]
  [Category("HappyPath")]
  public void NonWorkingHoursColor_CanBeSetAndRetrieved() {
    this._scheduler.NonWorkingHoursColor = Color.LightGray;
    Assert.That(this._scheduler.NonWorkingHoursColor, Is.EqualTo(Color.LightGray));
  }

  [Test]
  [Category("HappyPath")]
  public void SchedulerItem_DefaultValues() {
    var item = new SchedulerItem();

    Assert.That(item.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(item.Color, Is.EqualTo(Color.DodgerBlue));
    Assert.That(item.AllDay, Is.False);
    Assert.That(item.IsRecurring, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SchedulerItem_PropertiesWork() {
    var item = new SchedulerItem {
      Subject = "Test Meeting",
      Description = "Test Description",
      Start = new DateTime(2024, 6, 15, 10, 0, 0),
      End = new DateTime(2024, 6, 15, 11, 0, 0),
      AllDay = false,
      Color = Color.Red,
      IsRecurring = true,
      RecurrencePattern = "FREQ=DAILY",
      Tag = "custom tag"
    };

    Assert.That(item.Subject, Is.EqualTo("Test Meeting"));
    Assert.That(item.Description, Is.EqualTo("Test Description"));
    Assert.That(item.Start, Is.EqualTo(new DateTime(2024, 6, 15, 10, 0, 0)));
    Assert.That(item.End, Is.EqualTo(new DateTime(2024, 6, 15, 11, 0, 0)));
    Assert.That(item.AllDay, Is.False);
    Assert.That(item.Color, Is.EqualTo(Color.Red));
    Assert.That(item.IsRecurring, Is.True);
    Assert.That(item.RecurrencePattern, Is.EqualTo("FREQ=DAILY"));
    Assert.That(item.Tag, Is.EqualTo("custom tag"));
  }

  [Test]
  [Category("HappyPath")]
  public void SchedulerResource_DefaultValues() {
    var resource = new SchedulerResource();

    Assert.That(resource.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(resource.Color, Is.EqualTo(Color.DodgerBlue));
  }

  [Test]
  [Category("HappyPath")]
  public void SchedulerResource_PropertiesWork() {
    using var icon = new Bitmap(16, 16);
    var resource = new SchedulerResource {
      Name = "Conference Room",
      Color = Color.Green,
      Icon = icon
    };

    Assert.That(resource.Name, Is.EqualTo("Conference Room"));
    Assert.That(resource.Color, Is.EqualTo(Color.Green));
    Assert.That(resource.Icon, Is.EqualTo(icon));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._scheduler.Width, Is.EqualTo(600));
    Assert.That(this._scheduler.Height, Is.EqualTo(400));
  }
}
