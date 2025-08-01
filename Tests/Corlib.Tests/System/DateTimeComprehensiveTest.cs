using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public class DateTimeComprehensiveTest {

  #region EndOfDay Tests

  [Test]
  public void DateTimeExtensions_EndOfDay_StandardDate_ReturnsEndOfDay() {
    var date = new DateTime(2023, 6, 15, 14, 30, 45);
    var result = date.EndOfDay();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(6));
    Assert.That(result.Day, Is.EqualTo(15));
    Assert.That(result.Hour, Is.EqualTo(23));
    Assert.That(result.Minute, Is.EqualTo(59));
    Assert.That(result.Second, Is.EqualTo(59));
    Assert.That(result.Millisecond, Is.EqualTo(999));
    Assert.That(result.Ticks, Is.EqualTo(new DateTime(2023, 6, 16).Ticks - 1));
  }

  [Test]
  public void DateTimeExtensions_EndOfDay_WithCustomPrecision_RespectsParameter() {
    var date = new DateTime(2023, 6, 15);
    var precision = 1000; // 1000 ticks
    var result = date.EndOfDay(precision);
    
    var expected = new DateTime(2023, 6, 16).Ticks - precision;
    Assert.That(result.Ticks, Is.EqualTo(expected));
  }

  [Test]
  public void DateTimeExtensions_EndOfDay_LeapYearFebruary_HandlesCorrectly() {
    var leapYearFeb28 = new DateTime(2020, 2, 28, 10, 0, 0);
    var result = leapYearFeb28.EndOfDay();
    
    Assert.That(result.Year, Is.EqualTo(2020));
    Assert.That(result.Month, Is.EqualTo(2));
    Assert.That(result.Day, Is.EqualTo(28));
    Assert.That(result.Hour, Is.EqualTo(23));
    Assert.That(result.Minute, Is.EqualTo(59));
  }

  [Test]
  public void DateTimeExtensions_EndOfDay_ZeroPrecision_HandlesCorrectly() {
    var date = new DateTime(2023, 6, 15);
    var result = date.EndOfDay(0);
    
    var expected = new DateTime(2023, 6, 16).Ticks;
    Assert.That(result.Ticks, Is.EqualTo(expected));
  }

  #endregion

  #region StartOfDay Tests

  [Test]
  public void DateTimeExtensions_StartOfDay_StandardDate_ReturnsStartOfDay() {
    var date = new DateTime(2023, 6, 15, 14, 30, 45, 123);
    var result = date.StartOfDay();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(6));
    Assert.That(result.Day, Is.EqualTo(15));
    Assert.That(result.Hour, Is.EqualTo(0));
    Assert.That(result.Minute, Is.EqualTo(0));
    Assert.That(result.Second, Is.EqualTo(0));
    Assert.That(result.Millisecond, Is.EqualTo(0));
    Assert.That(result.Ticks, Is.EqualTo(new DateTime(2023, 6, 15).Ticks));
  }

  [Test]
  public void DateTimeExtensions_StartOfDay_AlreadyStartOfDay_RemainsUnchanged() {
    var date = new DateTime(2023, 6, 15, 0, 0, 0, 0);
    var result = date.StartOfDay();
    
    Assert.That(result, Is.EqualTo(date));
  }

  [Test]
  public void DateTimeExtensions_StartOfDay_MinValue_HandlesCorrectly() {
    var result = DateTime.MinValue.StartOfDay();
    
    Assert.That(result, Is.EqualTo(DateTime.MinValue));
  }

  #endregion

  #region AddWeeks and SubtractWeeks Tests

  [Test]
  public void DateTimeExtensions_AddWeeks_PositiveWeeks_AddsCorrectDays() {
    var date = new DateTime(2023, 6, 15);
    var result = date.AddWeeks(2);
    
    Assert.That(result, Is.EqualTo(date.AddDays(14)));
  }

  [Test]
  public void DateTimeExtensions_AddWeeks_NegativeWeeks_SubtractsCorrectDays() {
    var date = new DateTime(2023, 6, 15);
    var result = date.AddWeeks(-3);
    
    Assert.That(result, Is.EqualTo(date.AddDays(-21)));
  }

  [Test]
  public void DateTimeExtensions_AddWeeks_ZeroWeeks_ReturnsOriginal() {
    var date = new DateTime(2023, 6, 15);
    var result = date.AddWeeks(0);
    
    Assert.That(result, Is.EqualTo(date));
  }

  [Test]
  public void DateTimeExtensions_SubstractWeeks_PositiveWeeks_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15);
    var result = date.SubstractWeeks(2);
    
    Assert.That(result, Is.EqualTo(date.AddWeeks(-2)));
    Assert.That(result, Is.EqualTo(date.AddDays(-14)));
  }

  [Test]
  public void DateTimeExtensions_AddWeeks_CrossMonthBoundary_HandlesCorrectly() {
    var endOfMonth = new DateTime(2023, 6, 30);
    var result = endOfMonth.AddWeeks(1);
    
    Assert.That(result.Month, Is.EqualTo(7));
    Assert.That(result.Day, Is.EqualTo(7));
  }

  #endregion

  #region Week Calculation Tests - StartOfWeek

  [Test]
  public void DateTimeExtensions_StartOfWeek_MondayStart_StandardWeek_ReturnsCorrectly() {
    // Wednesday, June 14, 2023
    var wednesday = new DateTime(2023, 6, 14);
    var result = wednesday.StartOfWeek(DayOfWeek.Monday);
    
    // Should return Monday, June 12, 2023
    Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
    Assert.That(result.Date, Is.EqualTo(new DateTime(2023, 6, 12)));
  }

  [Test]
  public void DateTimeExtensions_StartOfWeek_SundayStart_StandardWeek_ReturnsCorrectly() {
    // Wednesday, June 14, 2023
    var wednesday = new DateTime(2023, 6, 14);
    var result = wednesday.StartOfWeek(DayOfWeek.Sunday);
    
    // Should return Sunday, June 11, 2023
    Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
    Assert.That(result.Date, Is.EqualTo(new DateTime(2023, 6, 11)));
  }

  [Test]
  public void DateTimeExtensions_StartOfWeek_SameAsStartDay_ReturnsSameDate() {
    var monday = new DateTime(2023, 6, 12); // Monday
    var result = monday.StartOfWeek(DayOfWeek.Monday);
    
    Assert.That(result.Date, Is.EqualTo(monday.Date));
  }

  [Test]
  public void DateTimeExtensions_StartOfWeek_AllDaysOfWeek_CalculatesCorrectly() {
    var reference = new DateTime(2023, 6, 14); // Wednesday
    
    var resultMonday = reference.StartOfWeek(DayOfWeek.Monday);
    var resultTuesday = reference.StartOfWeek(DayOfWeek.Tuesday);
    var resultWednesday = reference.StartOfWeek(DayOfWeek.Wednesday);
    var resultThursday = reference.StartOfWeek(DayOfWeek.Thursday);
    var resultFriday = reference.StartOfWeek(DayOfWeek.Friday);
    var resultSaturday = reference.StartOfWeek(DayOfWeek.Saturday);
    var resultSunday = reference.StartOfWeek(DayOfWeek.Sunday);
    
    Assert.That(resultMonday.Date, Is.EqualTo(new DateTime(2023, 6, 12))); // Monday
    Assert.That(resultTuesday.Date, Is.EqualTo(new DateTime(2023, 6, 13))); // Tuesday
    Assert.That(resultWednesday.Date, Is.EqualTo(new DateTime(2023, 6, 14))); // Wednesday (same)
    Assert.That(resultThursday.Date, Is.EqualTo(new DateTime(2023, 6, 8)));  // Previous Thursday
    Assert.That(resultFriday.Date, Is.EqualTo(new DateTime(2023, 6, 9)));    // Previous Friday
    Assert.That(resultSaturday.Date, Is.EqualTo(new DateTime(2023, 6, 10))); // Previous Saturday
    Assert.That(resultSunday.Date, Is.EqualTo(new DateTime(2023, 6, 11)));   // Previous Sunday
  }

  [Test]
  public void DateTimeExtensions_StartOfWeek_CrossMonthBoundary_HandlesCorrectly() {
    var firstOfMonth = new DateTime(2023, 7, 1); // Saturday
    var result = firstOfMonth.StartOfWeek(DayOfWeek.Monday);
    
    // Should go back to previous month
    Assert.That(result.Month, Is.EqualTo(6));
    Assert.That(result.Day, Is.EqualTo(26)); // Monday, June 26
    Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
  }

  #endregion

  #region DayInCurrentWeek Tests

  [Test]
  public void DateTimeExtensions_DayInCurrentWeek_SameWeek_ReturnsCorrectDay() {
    var wednesday = new DateTime(2023, 6, 14); // Wednesday
    var result = wednesday.DayInCurrentWeek(DayOfWeek.Friday, DayOfWeek.Monday);
    
    // Should return Friday of same week
    Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Friday));
    Assert.That(result.Date, Is.EqualTo(new DateTime(2023, 6, 16)));
  }

  [Test]
  public void DateTimeExtensions_DayInCurrentWeek_SameDayRequested_ReturnsSameDate() {
    var wednesday = new DateTime(2023, 6, 14);
    var result = wednesday.DayInCurrentWeek(DayOfWeek.Wednesday, DayOfWeek.Monday);
    
    Assert.That(result.Date, Is.EqualTo(wednesday.Date));
  }

  [Test]
  public void DateTimeExtensions_DayInCurrentWeek_DifferentStartDays_ProduceDifferentResults() {
    var wednesday = new DateTime(2023, 6, 14);
    
    var mondayStart = wednesday.DayInCurrentWeek(DayOfWeek.Sunday, DayOfWeek.Monday);
    var sundayStart = wednesday.DayInCurrentWeek(DayOfWeek.Sunday, DayOfWeek.Sunday);
    
    // With Monday start, Sunday should be end of current week
    // With Sunday start, Sunday should be start of current week
    Assert.That(mondayStart.Date, Is.EqualTo(new DateTime(2023, 6, 18))); // Sunday of current week (Monday start)
    Assert.That(sundayStart.Date, Is.EqualTo(new DateTime(2023, 6, 11))); // Sunday of current week (Sunday start)
  }

  #endregion

  #region DateOfDayOfCurrentWeek Tests

  [Test]
  public void DateTimeExtensions_DateOfDayOfCurrentWeek_ReturnsEndOfDay() {
    var wednesday = new DateTime(2023, 6, 14);
    var result = wednesday.DateOfDayOfCurrentWeek(DayOfWeek.Friday);
    
    // Should return end of Friday in same week
    Assert.That(result.Date, Is.EqualTo(new DateTime(2023, 6, 16)));
    Assert.That(result.Hour, Is.EqualTo(23));
    Assert.That(result.Minute, Is.EqualTo(59));
    Assert.That(result.Second, Is.EqualTo(59));
  }

  #endregion

  #region Month and Year Boundary Tests

  [Test]
  public void DateTimeExtensions_FirstDayOfMonth_StandardMonth_ReturnsFirst() {
    var midMonth = new DateTime(2023, 6, 15);
    var result = midMonth.FirstDayOfMonth();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(6));
    Assert.That(result.Day, Is.EqualTo(1));
  }

  [Test]
  public void DateTimeExtensions_LastDayOfMonth_StandardMonth_ReturnsLast() {
    var midMonth = new DateTime(2023, 6, 15);
    var result = midMonth.LastDayOfMonth();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(6));
    Assert.That(result.Day, Is.EqualTo(30)); // June has 30 days
  }

  [Test]
  public void DateTimeExtensions_LastDayOfMonth_February_LeapYear_Returns29() {
    var leapYearFeb = new DateTime(2020, 2, 15);
    var result = leapYearFeb.LastDayOfMonth();
    
    Assert.That(result.Day, Is.EqualTo(29)); // 2020 is leap year
  }

  [Test]
  public void DateTimeExtensions_LastDayOfMonth_February_NonLeapYear_Returns28() {
    var nonLeapYearFeb = new DateTime(2021, 2, 15);
    var result = nonLeapYearFeb.LastDayOfMonth();
    
    Assert.That(result.Day, Is.EqualTo(28)); // 2021 is not leap year
  }

  [Test]
  public void DateTimeExtensions_LastDayOfMonth_AllMonths_ReturnsCorrectDays() {
    var year = 2023;
    var expectedDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    
    for (var month = 1; month <= 12; month++) {
      var date = new DateTime(year, month, 15);
      var result = date.LastDayOfMonth();
      
      Assert.That(result.Day, Is.EqualTo(expectedDays[month - 1]), 
        $"Month {month} should have {expectedDays[month - 1]} days");
    }
  }

  [Test]
  public void DateTimeExtensions_FirstDayOfYear_ReturnsJanuary1() {
    var midYear = new DateTime(2023, 6, 15);
    var result = midYear.FirstDayOfYear();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(1));
    Assert.That(result.Day, Is.EqualTo(1));
  }

  [Test]
  public void DateTimeExtensions_LastDayOfYear_ReturnsDecember31() {
    var midYear = new DateTime(2023, 6, 15);
    var result = midYear.LastDayOfYear();
    
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(12));
    Assert.That(result.Day, Is.EqualTo(31));
  }

  #endregion

  #region Min/Max Comparison Tests

  [Test]
  public void DateTimeExtensions_Max_FirstIsLater_ReturnsFirst() {
    var later = new DateTime(2023, 6, 15);
    var earlier = new DateTime(2023, 6, 10);
    
    var result = later.Max(earlier);
    
    Assert.That(result, Is.EqualTo(later));
  }

  [Test]
  public void DateTimeExtensions_Max_SecondIsLater_ReturnsSecond() {
    var earlier = new DateTime(2023, 6, 10);
    var later = new DateTime(2023, 6, 15);
    
    var result = earlier.Max(later);
    
    Assert.That(result, Is.EqualTo(later));
  }

  [Test]
  public void DateTimeExtensions_Max_EqualDates_ReturnsEither() {
    var date1 = new DateTime(2023, 6, 15);
    var date2 = new DateTime(2023, 6, 15);
    
    var result = date1.Max(date2);
    
    Assert.That(result, Is.EqualTo(date1));
    Assert.That(result, Is.EqualTo(date2));
  }

  [Test]
  public void DateTimeExtensions_Min_FirstIsEarlier_ReturnsFirst() {
    var earlier = new DateTime(2023, 6, 10);
    var later = new DateTime(2023, 6, 15);
    
    var result = earlier.Min(later);
    
    Assert.That(result, Is.EqualTo(earlier));
  }

  [Test]
  public void DateTimeExtensions_Min_SecondIsEarlier_ReturnsSecond() {
    var later = new DateTime(2023, 6, 15);
    var earlier = new DateTime(2023, 6, 10);
    
    var result = later.Min(earlier);
    
    Assert.That(result, Is.EqualTo(earlier));
  }

  [Test]
  public void DateTimeExtensions_MinMax_ExtremeValues_HandlesCorrectly() {
    var minResult = DateTime.MinValue.Max(DateTime.MaxValue);
    var maxResult = DateTime.MaxValue.Min(DateTime.MinValue);
    
    Assert.That(minResult, Is.EqualTo(DateTime.MaxValue));
    Assert.That(maxResult, Is.EqualTo(DateTime.MinValue));
  }

  #endregion

  #region DaysTill Enumeration Tests

  [Test]
  public void DateTimeExtensions_DaysTill_SameDate_ReturnsSingleDate() {
    var date = new DateTime(2023, 6, 15);
    var result = date.DaysTill(date).ToList();
    
    Assert.That(result.Count, Is.EqualTo(1));
    Assert.That(result[0].Date, Is.EqualTo(date.Date));
  }

  [Test]
  public void DateTimeExtensions_DaysTill_ConsecutiveDays_ReturnsCorrectSequence() {
    var start = new DateTime(2023, 6, 10);
    var end = new DateTime(2023, 6, 12);
    var result = start.DaysTill(end).ToList();
    
    Assert.That(result.Count, Is.EqualTo(3));
    Assert.That(result[0].Date, Is.EqualTo(new DateTime(2023, 6, 10)));
    Assert.That(result[1].Date, Is.EqualTo(new DateTime(2023, 6, 11)));
    Assert.That(result[2].Date, Is.EqualTo(new DateTime(2023, 6, 12)));
  }

  [Test]
  public void DateTimeExtensions_DaysTill_CrossMonthBoundary_HandlesCorrectly() {
    var start = new DateTime(2023, 6, 29);
    var end = new DateTime(2023, 7, 2);
    var result = start.DaysTill(end).ToList();
    
    Assert.That(result.Count, Is.EqualTo(4));
    Assert.That(result[0].Date, Is.EqualTo(new DateTime(2023, 6, 29)));
    Assert.That(result[1].Date, Is.EqualTo(new DateTime(2023, 6, 30)));
    Assert.That(result[2].Date, Is.EqualTo(new DateTime(2023, 7, 1)));
    Assert.That(result[3].Date, Is.EqualTo(new DateTime(2023, 7, 2)));
  }

  [Test]
  public void DateTimeExtensions_DaysTill_EndBeforeStart_ReturnsEmpty() {
    var start = new DateTime(2023, 6, 15);
    var end = new DateTime(2023, 6, 10);
    var result = start.DaysTill(end).ToList();
    
    Assert.That(result, Is.Empty);
  }

  [Test]
  public void DateTimeExtensions_DaysTill_OnlyDateComponent_IgnoresTime() {
    var start = new DateTime(2023, 6, 10, 14, 30, 45);
    var end = new DateTime(2023, 6, 11, 8, 15, 20);
    var result = start.DaysTill(end).ToList();
    
    Assert.That(result.Count, Is.EqualTo(2));
    Assert.That(result[0], Is.EqualTo(new DateTime(2023, 6, 10))); // Time component stripped
    Assert.That(result[1], Is.EqualTo(new DateTime(2023, 6, 11))); // Time component stripped
  }

  [Test]
  public void DateTimeExtensions_DaysTill_LeapYear_HandlesFebruary29() {
    var start = new DateTime(2020, 2, 28);
    var end = new DateTime(2020, 3, 1);
    var result = start.DaysTill(end).ToList();
    
    Assert.That(result.Count, Is.EqualTo(3));
    Assert.That(result[0].Date, Is.EqualTo(new DateTime(2020, 2, 28)));
    Assert.That(result[1].Date, Is.EqualTo(new DateTime(2020, 2, 29))); // Leap day
    Assert.That(result[2].Date, Is.EqualTo(new DateTime(2020, 3, 1)));
  }

  #endregion

  #region Subtraction Helper Tests

  [Test]
  public void DateTimeExtensions_SubstractTicks_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15, 12, 0, 0);
    var result = date.SubstractTicks(1000);
    
    Assert.That(result, Is.EqualTo(date.AddTicks(-1000)));
  }

  [Test]
  public void DateTimeExtensions_SubstractSeconds_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15, 12, 0, 30);
    var result = date.SubstractSeconds(15.5);
    
    Assert.That(result, Is.EqualTo(date.AddSeconds(-15.5)));
  }

  [Test]
  public void DateTimeExtensions_SubstractMinutes_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15, 12, 30, 0);
    var result = date.SubstractMinutes(45.5);
    
    Assert.That(result, Is.EqualTo(date.AddMinutes(-45.5)));
  }

  [Test]
  public void DateTimeExtensions_SubstractHours_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15, 12, 0, 0);
    var result = date.SubstractHours(6.25);
    
    Assert.That(result, Is.EqualTo(date.AddHours(-6.25)));
  }

  [Test]
  public void DateTimeExtensions_SubstractDays_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15);
    var result = date.SubstractDays(10.5);
    
    Assert.That(result, Is.EqualTo(date.AddDays(-10.5)));
  }

  [Test]
  public void DateTimeExtensions_SubstractMonths_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15);
    var result = date.SubstractMonths(3);
    
    Assert.That(result, Is.EqualTo(date.AddMonths(-3)));
  }

  [Test]
  public void DateTimeExtensions_SubstractYears_PositiveValue_SubtractsCorrectly() {
    var date = new DateTime(2023, 6, 15);
    var result = date.SubstractYears(2);
    
    Assert.That(result, Is.EqualTo(date.AddYears(-2)));
  }

  [Test]
  public void DateTimeExtensions_SubstractionMethods_NegativeValues_AddCorrectly() {
    var date = new DateTime(2023, 6, 15, 12, 30, 45);
    
    Assert.That(date.SubstractTicks(-1000), Is.EqualTo(date.AddTicks(1000)));
    Assert.That(date.SubstractSeconds(-30), Is.EqualTo(date.AddSeconds(30)));
    Assert.That(date.SubstractMinutes(-15), Is.EqualTo(date.AddMinutes(15)));
    Assert.That(date.SubstractHours(-6), Is.EqualTo(date.AddHours(6)));
    Assert.That(date.SubstractDays(-5), Is.EqualTo(date.AddDays(5)));
    Assert.That(date.SubstractWeeks(-2), Is.EqualTo(date.AddWeeks(2)));
    Assert.That(date.SubstractMonths(-3), Is.EqualTo(date.AddMonths(3)));
    Assert.That(date.SubstractYears(-1), Is.EqualTo(date.AddYears(1)));
  }

  [Test]
  public void DateTimeExtensions_SubstractionMethods_ZeroValues_ReturnOriginal() {
    var date = new DateTime(2023, 6, 15, 12, 30, 45, 123);
    
    Assert.That(date.SubstractTicks(0), Is.EqualTo(date));
    Assert.That(date.SubstractMilliseconds(0), Is.EqualTo(date));
    Assert.That(date.SubstractSeconds(0), Is.EqualTo(date));
    Assert.That(date.SubstractMinutes(0), Is.EqualTo(date));
    Assert.That(date.SubstractHours(0), Is.EqualTo(date));
    Assert.That(date.SubstractDays(0), Is.EqualTo(date));
    Assert.That(date.SubstractWeeks(0), Is.EqualTo(date));
    Assert.That(date.SubstractMonths(0), Is.EqualTo(date));
    Assert.That(date.SubstractYears(0), Is.EqualTo(date));
  }

  #endregion

  #region Unix Timestamp Tests

  [Test]
  public void DateTimeExtensions_AsUnixTicksUtc_UnixEpoch_ReturnsZero() {
    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var result = unixEpoch.AsUnixTicksUtc();
    
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  public void DateTimeExtensions_AsUnixMillisecondsUtc_UnixEpoch_ReturnsZero() {
    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var result = unixEpoch.AsUnixMillisecondsUtc();
    
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  public void DateTimeExtensions_AsUnixTicksUtc_KnownDate_ReturnsCorrectValue() {
    var date = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Utc);
    var result = date.AsUnixTicksUtc();
    
    // Verify it's a positive value after Unix epoch
    Assert.That(result, Is.GreaterThan(0));
    
    // Verify conversion back
    var converted = DateTimeExtensions.FromUnixTicks(result, DateTimeKind.Utc);
    Assert.That(converted.Date, Is.EqualTo(date.Date));
  }

  [Test]
  public void DateTimeExtensions_UnixTimestamp_RoundTrip_MaintainsAccuracy() {
    var original = new DateTime(2023, 6, 15, 12, 30, 45, DateTimeKind.Utc);
    
    var ticks = original.AsUnixTicksUtc();
    var milliseconds = original.AsUnixMillisecondsUtc();
    
    var fromTicks = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Utc);
    var fromSeconds = DateTimeExtensions.FromUnixSeconds(milliseconds / 1000, DateTimeKind.Utc);
    
    // Tick-level precision should be exact
    Assert.That(fromTicks, Is.EqualTo(original));
    
    // Second-level precision should be close (within 1 second)
    Assert.That(Math.Abs((fromSeconds - original).TotalSeconds), Is.LessThan(1.0));
  }

  [Test]
  public void DateTimeExtensions_FromUnixTicks_DifferentKinds_SetsCorrectKind() {
    var ticks = 1000000000L;
    
    var utc = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Utc);
    var local = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Local);
    var unspecified = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Unspecified);
    
    Assert.That(utc.Kind, Is.EqualTo(DateTimeKind.Utc));
    Assert.That(local.Kind, Is.EqualTo(DateTimeKind.Local));
    Assert.That(unspecified.Kind, Is.EqualTo(DateTimeKind.Unspecified));
  }

  [Test]
  public void DateTimeExtensions_FromUnixSeconds_DifferentKinds_SetsCorrectKind() {
    var seconds = 1000000L;
    
    var utc = DateTimeExtensions.FromUnixSeconds(seconds, DateTimeKind.Utc);
    var local = DateTimeExtensions.FromUnixSeconds(seconds, DateTimeKind.Local);
    var unspecified = DateTimeExtensions.FromUnixSeconds(seconds, DateTimeKind.Unspecified);
    
    Assert.That(utc.Kind, Is.EqualTo(DateTimeKind.Utc));
    Assert.That(local.Kind, Is.EqualTo(DateTimeKind.Local));
    Assert.That(unspecified.Kind, Is.EqualTo(DateTimeKind.Unspecified));
  }

  [Test]
  public void DateTimeExtensions_AsUnixTimestamp_PreUnixEpoch_ReturnsNegative() {
    var preEpoch = new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var result = preEpoch.AsUnixTicksUtc();
    
    Assert.That(result, Is.LessThan(0));
  }

  [Test]
  public void DateTimeExtensions_FromUnixTicks_NegativeValue_HandlesCorrectly() {
    var negativeTicks = -1000000L;
    var result = DateTimeExtensions.FromUnixTicks(negativeTicks, DateTimeKind.Utc);
    
    Assert.That(result, Is.LessThan(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
  }

  #endregion

  #region Performance Tests

  [Test]
  public void DateTimeExtensions_DaysTill_LargeRange_PerformanceTest() {
    var start = new DateTime(2020, 1, 1);
    var end = new DateTime(2025, 12, 31); // ~6 years = ~2190 days
    
    var sw = global::System.Diagnostics.Stopwatch.StartNew();
    var result = start.DaysTill(end).ToList();
    sw.Stop();
    
    Assert.That(result.Count, Is.GreaterThan(2000));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100)); // Should be fast
  }

  [Test]
  public void DateTimeExtensions_StartOfWeek_Performance_FastCalculation() {
    var dates = Enumerable.Range(0, 1000)
      .Select(i => DateTime.Today.AddDays(i))
      .ToList();
    
    var sw = global::System.Diagnostics.Stopwatch.StartNew();
    foreach (var date in dates) {
      date.StartOfWeek(DayOfWeek.Monday);
    }
    sw.Stop();
    
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(50)); // Should be very fast
  }

  [Test]
  public void DateTimeExtensions_UnixTimestamp_Performance_FastConversion() {
    var dates = Enumerable.Range(0, 1000)
      .Select(i => DateTime.UtcNow.AddDays(i))
      .ToList();
    
    var sw = global::System.Diagnostics.Stopwatch.StartNew();
    foreach (var date in dates) {
      var ticks = date.AsUnixTicksUtc();
      var converted = DateTimeExtensions.FromUnixTicks(ticks, DateTimeKind.Utc);
    }
    sw.Stop();
    
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100));
  }

  #endregion

  #region Edge Cases and Boundary Tests

  [Test]
  public void DateTimeExtensions_Boundaries_MinMaxValues_HandleGracefully() {
    // These should not throw exceptions
    Assert.DoesNotThrow(() => DateTime.MinValue.StartOfDay());
    Assert.DoesNotThrow(() => DateTime.MaxValue.StartOfDay());
    Assert.DoesNotThrow(() => DateTime.MinValue.FirstDayOfMonth());
    Assert.DoesNotThrow(() => DateTime.MaxValue.LastDayOfMonth());
    Assert.DoesNotThrow(() => DateTime.MinValue.FirstDayOfYear());
    Assert.DoesNotThrow(() => DateTime.MaxValue.LastDayOfYear());
  }

  [Test]
  public void DateTimeExtensions_AddWeeks_LargeValues_HandlesOverflow() {
    var date = new DateTime(2023, 6, 15);
    
    // Large positive value - should not throw but may clamp to MaxValue
    Assert.Throws<ArgumentOutOfRangeException>(() => date.AddWeeks(int.MaxValue / 7));
    
    // Large negative value - should not throw but may clamp to MinValue  
    Assert.Throws<ArgumentOutOfRangeException>(() => date.AddWeeks(int.MinValue / 7));
  }

  [Test]
  public void DateTimeExtensions_WeekCalculations_YearBoundary_HandlesCorrectly() {
    var newYear = new DateTime(2024, 1, 1); // Monday
    var result = newYear.StartOfWeek(DayOfWeek.Sunday);
    
    // Should go back to previous year
    Assert.That(result.Year, Is.EqualTo(2023));
    Assert.That(result.Month, Is.EqualTo(12));
    Assert.That(result.Day, Is.EqualTo(31)); // Sunday, December 31, 2023
  }

  #endregion
}