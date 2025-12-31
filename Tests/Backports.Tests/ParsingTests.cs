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
using System.Globalization;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
public class ParsingTests {

  #region Guid.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_Span_ValidGuid_ReturnsTrue() {
    var guidString = "12345678-1234-1234-1234-123456789ABC".AsSpan();
    var success = Guid.TryParse(guidString, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(new Guid("12345678-1234-1234-1234-123456789ABC")));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_Span_InvalidGuid_ReturnsFalse() {
    var invalidGuid = "not-a-guid".AsSpan();
    var success = Guid.TryParse(invalidGuid, out _);
    Assert.That(success, Is.False);
  }


  #endregion

  #region Int32.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_ValidInt_ReturnsTrue() {
    var success = int.TryParse("12345".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(12345));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_NegativeNumber_ReturnsTrue() {
    var success = int.TryParse("-42".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(-42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_TryParse_Span_InvalidString_ReturnsFalse() {
    var success = int.TryParse("not-a-number".AsSpan(), out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_TryParse_Span_EmptySpan_ReturnsFalse() {
    var success = int.TryParse(ReadOnlySpan<char>.Empty, out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_MaxValue_ReturnsTrue() {
    var success = int.TryParse(int.MaxValue.ToString().AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(int.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_MinValue_ReturnsTrue() {
    var success = int.TryParse(int.MinValue.ToString().AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(int.MinValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int32_TryParse_Span_Overflow_ReturnsFalse() {
    var success = int.TryParse("9999999999999999999".AsSpan(), out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_WithNumberStyles_Hex_ReturnsTrue() {
    var success = int.TryParse("FF".AsSpan(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_TryParse_Span_WithLeadingWhitespace_ReturnsTrue() {
    var success = int.TryParse("  123".AsSpan(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(123));
  }

  [Test]
  [Category("HappyPath")]
  public void Int32_Parse_Span_ValidInt_ReturnsValue() {
    var result = int.Parse("12345".AsSpan());
    Assert.That(result, Is.EqualTo(12345));
  }

  #endregion

  #region Int64.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Int64_TryParse_Span_ValidLong_ReturnsTrue() {
    var success = long.TryParse("1234567890123".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1234567890123L));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_TryParse_Span_NegativeNumber_ReturnsTrue() {
    var success = long.TryParse("-9876543210".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(-9876543210L));
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_TryParse_Span_MaxValue_ReturnsTrue() {
    var success = long.TryParse(long.MaxValue.ToString().AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(long.MaxValue));
  }

  [Test]
  [Category("EdgeCase")]
  public void Int64_TryParse_Span_InvalidString_ReturnsFalse() {
    var success = long.TryParse("abc".AsSpan(), out _);
    Assert.That(success, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Int64_TryParse_Span_WithNumberStyles_Hex_ReturnsTrue() {
    var success = long.TryParse("FFFFFFFF".AsSpan(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(4294967295L));
  }

  #endregion

  #region Double.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Double_TryParse_Span_ValidDouble_ReturnsTrue() {
    var success = double.TryParse("12345".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(12345.0).Within(0.0001));
  }

  [Test]
  [Category("HappyPath")]
  public void Double_TryParse_Span_DecimalNumber_ReturnsTrue() {
    var success = double.TryParse("123.456".AsSpan(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(123.456).Within(0.0001));
  }

  [Test]
  [Category("HappyPath")]
  public void Double_TryParse_Span_NegativeDecimal_ReturnsTrue() {
    var success = double.TryParse("-99.99".AsSpan(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(-99.99).Within(0.0001));
  }

  [Test]
  [Category("HappyPath")]
  public void Double_TryParse_Span_ScientificNotation_ReturnsTrue() {
    var success = double.TryParse("1.5e10".AsSpan(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1.5e10).Within(1e5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Double_TryParse_Span_InvalidString_ReturnsFalse() {
    var success = double.TryParse("not-a-double".AsSpan(), out _);
    Assert.That(success, Is.False);
  }

  #endregion

  #region TimeSpan.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_Span_ValidTimeSpan_ReturnsTrue() {
    var success = TimeSpan.TryParse("01:30:00".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(90)));
  }

  [Test]
  [Category("HappyPath")]
  public void TimeSpan_TryParse_Span_DaysHoursMinutes_ReturnsTrue() {
    var success = TimeSpan.TryParse("2.03:04:05".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Days, Is.EqualTo(2));
    Assert.That(result.Hours, Is.EqualTo(3));
    Assert.That(result.Minutes, Is.EqualTo(4));
    Assert.That(result.Seconds, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void TimeSpan_TryParse_Span_InvalidFormat_ReturnsFalse() {
    var success = TimeSpan.TryParse("invalid".AsSpan(), out _);
    Assert.That(success, Is.False);
  }

  #endregion

  #region Version.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_Span_ValidVersion_ReturnsTrue() {
    var success = Version.TryParse("1.2.3.4".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(new Version(1, 2, 3, 4)));
  }

  #endregion

}
