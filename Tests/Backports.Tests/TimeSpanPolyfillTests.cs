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
[Category("Unit")]
[Category("Backports")]
[Category("TimeSpan")]
public class TimeSpanPolyfillTests {

  #region TimeSpan.Parse(string)

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_Days_ReturnsTimeSpan() {
    var result = TimeSpan.Parse("5");
    Assert.That(result.Days, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_HoursMinutesSeconds_ReturnsTimeSpan() {
    var result = TimeSpan.Parse("12:30:45");
    Assert.That(result.Hours, Is.EqualTo(12));
    Assert.That(result.Minutes, Is.EqualTo(30));
    Assert.That(result.Seconds, Is.EqualTo(45));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_DaysHoursMinutesSeconds_ReturnsTimeSpan() {
    var result = TimeSpan.Parse("3.12:30:45");
    Assert.That(result.Days, Is.EqualTo(3));
    Assert.That(result.Hours, Is.EqualTo(12));
    Assert.That(result.Minutes, Is.EqualTo(30));
    Assert.That(result.Seconds, Is.EqualTo(45));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_WithMilliseconds_ReturnsTimeSpan() {
    var result = TimeSpan.Parse("1:02:03.123");
    Assert.That(result.Hours, Is.EqualTo(1));
    Assert.That(result.Minutes, Is.EqualTo(2));
    Assert.That(result.Seconds, Is.EqualTo(3));
    Assert.That(result.Milliseconds, Is.EqualTo(123));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_Negative_ReturnsNegativeTimeSpan() {
    var result = TimeSpan.Parse("-1:30:00");
    Assert.That(result.TotalHours, Is.LessThan(0));
    Assert.That(result.Hours, Is.EqualTo(-1));
    Assert.That(result.Minutes, Is.EqualTo(-30));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_Parse_Zero_ReturnsZeroTimeSpan() {
    var result = TimeSpan.Parse("0:00:00");
    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("Exception")]
  public void TimeSpan_Parse_InvalidFormat_ThrowsException() {
    Assert.Throws<FormatException>(() => TimeSpan.Parse("not-a-timespan"));
  }

  [Test]
  [Category("Exception")]
  public void TimeSpan_Parse_Null_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => TimeSpan.Parse(null));
  }

  [Test]
  [Category("Exception")]
  public void TimeSpan_Parse_Empty_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => TimeSpan.Parse(""));
  }

  #endregion

  #region TimeSpan.TryParse(string)

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_ValidTimeSpan_ReturnsTrue() {
    var success = TimeSpan.TryParse("12:30:45", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Hours, Is.EqualTo(12));
    Assert.That(result.Minutes, Is.EqualTo(30));
    Assert.That(result.Seconds, Is.EqualTo(45));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_DaysFormat_ReturnsTrue() {
    var success = TimeSpan.TryParse("2.05:30:00", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Days, Is.EqualTo(2));
    Assert.That(result.Hours, Is.EqualTo(5));
    Assert.That(result.Minutes, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_NegativeValue_ReturnsTrue() {
    var success = TimeSpan.TryParse("-3:45:00", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Hours, Is.EqualTo(-3));
    Assert.That(result.Minutes, Is.EqualTo(-45));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_InvalidTimeSpan_ReturnsFalse() {
    var success = TimeSpan.TryParse("invalid", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_Null_ReturnsFalse() {
    var success = TimeSpan.TryParse(null, out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_Empty_ReturnsFalse() {
    var success = TimeSpan.TryParse("", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeSpan_TryParse_MaxValue_ReturnsTrue() {
    var maxTimeSpan = TimeSpan.MaxValue;
    var maxString = maxTimeSpan.ToString();
    var success = TimeSpan.TryParse(maxString, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(maxTimeSpan));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeSpan_TryParse_MinValue_ReturnsTrue() {
    var minTimeSpan = TimeSpan.MinValue;
    var minString = minTimeSpan.ToString();
    var success = TimeSpan.TryParse(minString, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(minTimeSpan));
  }

  #endregion

}
