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
public class TimeSpanTests {

  #region TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void TryParse_Span_ValidTimeSpan_ReturnsTrue() {
    var success = TimeSpan.TryParse("01:30:00".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(1.5)));
  }

  #endregion

  #region ParseExact

  [Test]
  [Category("HappyPath")]
  public void ParseExact_ConstantFormat_ParsesCorrectly() {
    var result = TimeSpan.ParseExact("1.02:03:04.0050000", "c", null);
    Assert.That(result.Days, Is.EqualTo(1));
    Assert.That(result.Hours, Is.EqualTo(2));
    Assert.That(result.Minutes, Is.EqualTo(3));
    Assert.That(result.Seconds, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ParseExact_NegativeTimeSpan_ParsesCorrectly() {
    var result = TimeSpan.ParseExact("-01:30:00", "c", null);
    Assert.That(result.TotalHours, Is.EqualTo(-1.5).Within(0.001));
  }

  #endregion

  #region TryParseExact

  [Test]
  [Category("HappyPath")]
  public void TryParseExact_ValidFormat_ReturnsTrue() {
    var success = TimeSpan.TryParseExact("02:30:00", "c", null, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(2.5)));
  }

  [Test]
  [Category("HappyPath")]
  public void TryParseExact_CustomFormat_ParsesCorrectly() {
    var success = TimeSpan.TryParseExact("05:30", @"hh\:mm", null, out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Hours, Is.EqualTo(5));
    Assert.That(result.Minutes, Is.EqualTo(30));
  }

  [Test]
  [Category("EdgeCase")]
  public void TryParseExact_InvalidFormat_ReturnsFalse() {
    var success = TimeSpan.TryParseExact("invalid", "c", null, out _);
    Assert.That(success, Is.False);
  }

  #endregion

  #region TimeSpanStyles

  [Test]
  [Category("HappyPath")]
  public void TimeSpanStyles_EnumValues_Exist() {
    Assert.That((int)System.Globalization.TimeSpanStyles.None, Is.EqualTo(0));
    Assert.That((int)System.Globalization.TimeSpanStyles.AssumeNegative, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpanStyles_None_ParsesAsPositive() {
    var success = TimeSpan.TryParseExact("01:30:00", "c", null, System.Globalization.TimeSpanStyles.None, out var result);
    Assert.That(success, Is.True);
    Assert.That(result.TotalHours, Is.EqualTo(1.5).Within(0.001));
  }

  #endregion

}
