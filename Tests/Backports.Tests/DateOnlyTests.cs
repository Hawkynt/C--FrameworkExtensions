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
public class DateOnlyTests {

  [Test]
  public void DateOnly_Constructor_Works() {
    var date = new DateOnly(2024, 6, 15);
    Assert.That(date.Year, Is.EqualTo(2024));
    Assert.That(date.Month, Is.EqualTo(6));
    Assert.That(date.Day, Is.EqualTo(15));
  }

  [Test]
  public void DateOnly_MinValue_IsCorrect() {
    var min = DateOnly.MinValue;
    Assert.That(min.Year, Is.EqualTo(1));
    Assert.That(min.Month, Is.EqualTo(1));
    Assert.That(min.Day, Is.EqualTo(1));
  }

  [Test]
  public void DateOnly_MaxValue_IsCorrect() {
    var max = DateOnly.MaxValue;
    Assert.That(max.Year, Is.EqualTo(9999));
    Assert.That(max.Month, Is.EqualTo(12));
    Assert.That(max.Day, Is.EqualTo(31));
  }

  [Test]
  public void DateOnly_DayOfWeek_Works() {
    var date = new DateOnly(2024, 6, 15); // Saturday
    Assert.That(date.DayOfWeek, Is.EqualTo(DayOfWeek.Saturday));
  }

  [Test]
  public void DateOnly_DayOfYear_Works() {
    var date = new DateOnly(2024, 1, 15);
    Assert.That(date.DayOfYear, Is.EqualTo(15));
  }

  [Test]
  public void DateOnly_AddDays_Works() {
    var date = new DateOnly(2024, 6, 15);
    var result = date.AddDays(10);
    Assert.That(result.Day, Is.EqualTo(25));
  }

  [Test]
  public void DateOnly_AddMonths_Works() {
    var date = new DateOnly(2024, 6, 15);
    var result = date.AddMonths(2);
    Assert.That(result.Month, Is.EqualTo(8));
  }

  [Test]
  public void DateOnly_AddYears_Works() {
    var date = new DateOnly(2024, 6, 15);
    var result = date.AddYears(1);
    Assert.That(result.Year, Is.EqualTo(2025));
  }

  [Test]
  public void DateOnly_FromDateTime_Works() {
    var dateTime = new DateTime(2024, 6, 15, 14, 30, 0);
    var date = DateOnly.FromDateTime(dateTime);
    Assert.That(date.Year, Is.EqualTo(2024));
    Assert.That(date.Month, Is.EqualTo(6));
    Assert.That(date.Day, Is.EqualTo(15));
  }

  [Test]
  public void DateOnly_ToDateTime_Works() {
    var date = new DateOnly(2024, 6, 15);
    var time = new TimeOnly(14, 30, 0);
    var dateTime = date.ToDateTime(time);
    Assert.That(dateTime.Year, Is.EqualTo(2024));
    Assert.That(dateTime.Month, Is.EqualTo(6));
    Assert.That(dateTime.Day, Is.EqualTo(15));
    Assert.That(dateTime.Hour, Is.EqualTo(14));
    Assert.That(dateTime.Minute, Is.EqualTo(30));
  }

  [Test]
  public void DateOnly_Comparison_Works() {
    var a = new DateOnly(2024, 6, 15);
    var b = new DateOnly(2024, 7, 15);
    Assert.That(a < b, Is.True);
    Assert.That(b > a, Is.True);
    Assert.That(a <= b, Is.True);
    Assert.That(b >= a, Is.True);
    Assert.That(a != b, Is.True);
  }

  [Test]
  public void DateOnly_Equality_Works() {
    var a = new DateOnly(2024, 6, 15);
    var b = new DateOnly(2024, 6, 15);
    Assert.That(a == b, Is.True);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  public void DateOnly_ToString_Works() {
    var date = new DateOnly(2024, 6, 15);
    var str = date.ToString();
    Assert.That(str, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  public void DateOnly_Parse_Works() {
    var date = DateOnly.Parse("2024-06-15");
    Assert.That(date.Year, Is.EqualTo(2024));
    Assert.That(date.Month, Is.EqualTo(6));
    Assert.That(date.Day, Is.EqualTo(15));
  }

  [Test]
  public void DateOnly_TryParse_ValidInput_ReturnsTrue() {
    var success = DateOnly.TryParse("2024-06-15", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Year, Is.EqualTo(2024));
  }

  [Test]
  public void DateOnly_TryParse_InvalidInput_ReturnsFalse() {
    var success = DateOnly.TryParse("not a date", out _);
    Assert.That(success, Is.False);
  }

  [Test]
  public void DateOnly_CompareTo_Works() {
    var a = new DateOnly(2024, 6, 15);
    var b = new DateOnly(2024, 7, 15);
    Assert.That(a.CompareTo(b), Is.LessThan(0));
    Assert.That(b.CompareTo(a), Is.GreaterThan(0));
    Assert.That(a.CompareTo(a), Is.EqualTo(0));
  }

  [Test]
  public void DateOnly_GetHashCode_ConsistentForEqualValues() {
    var a = new DateOnly(2024, 6, 15);
    var b = new DateOnly(2024, 6, 15);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  [Test]
  public void DateOnly_FromDayNumber_Works() {
    var date = DateOnly.FromDayNumber(1);
    Assert.That(date.Year, Is.EqualTo(1));
    Assert.That(date.Month, Is.EqualTo(1));
    Assert.That(date.Day, Is.EqualTo(2));
  }

  [Test]
  public void DateOnly_DayNumber_Works() {
    var date = new DateOnly(1, 1, 1);
    Assert.That(date.DayNumber, Is.EqualTo(0));
  }

}
