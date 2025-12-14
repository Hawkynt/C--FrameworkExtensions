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

using System;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class TimeOnlyTests {

  [Test]
  public void TimeOnly_Constructor_HourMinute_Works() {
    var time = new TimeOnly(14, 30);
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
    Assert.That(time.Second, Is.EqualTo(0));
    Assert.That(time.Millisecond, Is.EqualTo(0));
  }

  [Test]
  public void TimeOnly_Constructor_HourMinuteSecond_Works() {
    var time = new TimeOnly(14, 30, 45);
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
    Assert.That(time.Second, Is.EqualTo(45));
  }

  [Test]
  public void TimeOnly_Constructor_Full_Works() {
    var time = new TimeOnly(14, 30, 45, 500);
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
    Assert.That(time.Second, Is.EqualTo(45));
    Assert.That(time.Millisecond, Is.EqualTo(500));
  }

  [Test]
  public void TimeOnly_MinValue_IsMidnight() {
    var min = TimeOnly.MinValue;
    Assert.That(min.Hour, Is.EqualTo(0));
    Assert.That(min.Minute, Is.EqualTo(0));
    Assert.That(min.Second, Is.EqualTo(0));
  }

  [Test]
  public void TimeOnly_MaxValue_IsEndOfDay() {
    var max = TimeOnly.MaxValue;
    Assert.That(max.Hour, Is.EqualTo(23));
    Assert.That(max.Minute, Is.EqualTo(59));
    Assert.That(max.Second, Is.EqualTo(59));
  }

  [Test]
  public void TimeOnly_Add_Works() {
    var time = new TimeOnly(14, 30);
    var result = time.Add(TimeSpan.FromHours(2));
    Assert.That(result.Hour, Is.EqualTo(16));
    Assert.That(result.Minute, Is.EqualTo(30));
  }

  [Test]
  public void TimeOnly_Add_WrapsAround() {
    var time = new TimeOnly(23, 0);
    var result = time.Add(TimeSpan.FromHours(2), out var wrappedDays);
    Assert.That(result.Hour, Is.EqualTo(1));
    Assert.That(wrappedDays, Is.EqualTo(1));
  }

  [Test]
  public void TimeOnly_AddHours_Works() {
    var time = new TimeOnly(14, 30);
    var result = time.AddHours(2);
    Assert.That(result.Hour, Is.EqualTo(16));
  }

  [Test]
  public void TimeOnly_AddMinutes_Works() {
    var time = new TimeOnly(14, 30);
    var result = time.AddMinutes(45);
    Assert.That(result.Hour, Is.EqualTo(15));
    Assert.That(result.Minute, Is.EqualTo(15));
  }

  [Test]
  public void TimeOnly_IsBetween_Works() {
    var time = new TimeOnly(12, 0);
    Assert.That(time.IsBetween(new TimeOnly(10, 0), new TimeOnly(14, 0)), Is.True);
    Assert.That(time.IsBetween(new TimeOnly(14, 0), new TimeOnly(16, 0)), Is.False);
  }

  [Test]
  public void TimeOnly_IsBetween_WrapsAround() {
    var time = new TimeOnly(2, 0);
    Assert.That(time.IsBetween(new TimeOnly(22, 0), new TimeOnly(6, 0)), Is.True);
  }

  [Test]
  public void TimeOnly_FromDateTime_Works() {
    var dateTime = new DateTime(2024, 6, 15, 14, 30, 45);
    var time = TimeOnly.FromDateTime(dateTime);
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
    Assert.That(time.Second, Is.EqualTo(45));
  }

  [Test]
  public void TimeOnly_FromTimeSpan_Works() {
    var timeSpan = TimeSpan.FromHours(14.5);
    var time = TimeOnly.FromTimeSpan(timeSpan);
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
  }

  [Test]
  public void TimeOnly_ToTimeSpan_Works() {
    var time = new TimeOnly(14, 30);
    var timeSpan = time.ToTimeSpan();
    Assert.That(timeSpan.Hours, Is.EqualTo(14));
    Assert.That(timeSpan.Minutes, Is.EqualTo(30));
  }

  [Test]
  public void TimeOnly_Comparison_Works() {
    var a = new TimeOnly(14, 30);
    var b = new TimeOnly(16, 0);
    Assert.That(a < b, Is.True);
    Assert.That(b > a, Is.True);
    Assert.That(a <= b, Is.True);
    Assert.That(b >= a, Is.True);
    Assert.That(a != b, Is.True);
  }

  [Test]
  public void TimeOnly_Equality_Works() {
    var a = new TimeOnly(14, 30);
    var b = new TimeOnly(14, 30);
    Assert.That(a == b, Is.True);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  public void TimeOnly_Subtraction_Works() {
    var a = new TimeOnly(16, 30);
    var b = new TimeOnly(14, 0);
    var diff = a - b;
    Assert.That(diff.Hours, Is.EqualTo(2));
    Assert.That(diff.Minutes, Is.EqualTo(30));
  }

  [Test]
  public void TimeOnly_ToString_Works() {
    var time = new TimeOnly(14, 30);
    var str = time.ToString();
    Assert.That(str, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  public void TimeOnly_Parse_Works() {
    var time = TimeOnly.Parse("14:30:00");
    Assert.That(time.Hour, Is.EqualTo(14));
    Assert.That(time.Minute, Is.EqualTo(30));
  }

  [Test]
  public void TimeOnly_TryParse_ValidInput_ReturnsTrue() {
    var success = TimeOnly.TryParse("14:30:00", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Hour, Is.EqualTo(14));
  }

  [Test]
  public void TimeOnly_TryParse_InvalidInput_ReturnsFalse() {
    var success = TimeOnly.TryParse("not a time", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  public void TimeOnly_CompareTo_Works() {
    var a = new TimeOnly(14, 30);
    var b = new TimeOnly(16, 0);
    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(a), Is.EqualTo(0));
  }

  [Test]
  public void TimeOnly_GetHashCode_ConsistentForEqualValues() {
    var a = new TimeOnly(14, 30);
    var b = new TimeOnly(14, 30);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  [Test]
  public void TimeOnly_Ticks_Works() {
    var time = new TimeOnly(1, 0);
    Assert.That(time.Ticks, Is.EqualTo(TimeSpan.TicksPerHour));
  }

}
