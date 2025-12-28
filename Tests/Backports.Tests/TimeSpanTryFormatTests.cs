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
[Category("TimeSpan")]
public class TimeSpanTryFormatTests {

  #region TryFormat with default format

  [Test]
  [Category("HappyPath")]
  public void TryFormat_DefaultFormat_FormatsCorrectly() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.GreaterThan(0));
    var result = buffer[..charsWritten].ToString();
    Assert.That(result, Does.Contain("1.02:03:04"));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_ZeroTimeSpan_FormatsCorrectly() {
    var timeSpan = TimeSpan.Zero;
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.True);
    var result = buffer[..charsWritten].ToString();
    Assert.That(result, Is.EqualTo("00:00:00"));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_NegativeTimeSpan_FormatsCorrectly() {
    var timeSpan = TimeSpan.FromHours(-1.5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.True);
    var result = buffer[..charsWritten].ToString();
    Assert.That(result, Does.StartWith("-"));
  }

  #endregion

  #region TryFormat with custom formats

  [Test]
  [Category("HappyPath")]
  public void TryFormat_ConstantFormat_FormatsCorrectly() {
    var timeSpan = new TimeSpan(0, 2, 30, 45, 0);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "c");
    Assert.That(success, Is.True);
    var result = buffer[..charsWritten].ToString();
    Assert.That(result, Is.EqualTo("02:30:45"));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_GeneralShortFormat_FormatsCorrectly() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "g", CultureInfo.InvariantCulture);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_GeneralLongFormat_FormatsCorrectly() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "G", CultureInfo.InvariantCulture);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_CustomFormat_FormatsCorrectly() {
    var timeSpan = new TimeSpan(0, 5, 30, 0, 0);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, @"hh\:mm");
    Assert.That(success, Is.True);
    var result = buffer[..charsWritten].ToString();
    Assert.That(result, Is.EqualTo("05:30"));
  }

  #endregion

  #region TryFormat with insufficient buffer

  [Test]
  [Category("EdgeCase")]
  public void TryFormat_BufferTooSmall_ReturnsFalse() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[3];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.False);
    Assert.That(charsWritten, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void TryFormat_EmptyBuffer_ReturnsFalse() {
    var timeSpan = TimeSpan.FromHours(1);
    Span<char> buffer = Span<char>.Empty;
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.False);
    Assert.That(charsWritten, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void TryFormat_ExactSizeBuffer_ReturnsTrue() {
    var timeSpan = TimeSpan.Zero;
    var expected = timeSpan.ToString();
    Span<char> buffer = stackalloc char[expected.Length];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.EqualTo(expected.Length));
  }

  #endregion

  #region TryFormat charsWritten accuracy

  [Test]
  [Category("HappyPath")]
  public void TryFormat_CharsWritten_MatchesStringLength() {
    var timeSpan = new TimeSpan(2, 15, 30, 45, 123);
    Span<char> buffer = stackalloc char[100];
    var success = timeSpan.TryFormat(buffer, out var charsWritten);
    Assert.That(success, Is.True);
    var expected = timeSpan.ToString();
    Assert.That(charsWritten, Is.EqualTo(expected.Length));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_WithFormat_CharsWrittenMatchesStringLength() {
    var timeSpan = new TimeSpan(0, 12, 34, 56, 0);
    Span<char> buffer = stackalloc char[100];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "c");
    Assert.That(success, Is.True);
    var expected = timeSpan.ToString("c");
    Assert.That(charsWritten, Is.EqualTo(expected.Length));
  }

  #endregion

  #region TryFormat with culture-specific provider

  [Test]
  [Category("HappyPath")]
  public void TryFormat_WithInvariantCulture_FormatsCorrectly() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "g", CultureInfo.InvariantCulture);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TryFormat_WithNullProvider_FormatsCorrectly() {
    var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
    Span<char> buffer = stackalloc char[50];
    var success = timeSpan.TryFormat(buffer, out var charsWritten, "c", null);
    Assert.That(success, Is.True);
    Assert.That(charsWritten, Is.GreaterThan(0));
  }

  #endregion

}
