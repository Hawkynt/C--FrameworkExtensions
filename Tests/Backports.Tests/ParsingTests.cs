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

  #endregion

  #region Int64.TryParse(Span)

  [Test]
  [Category("HappyPath")]
  public void Int64_TryParse_Span_ValidLong_ReturnsTrue() {
    var success = long.TryParse("1234567890123".AsSpan(), out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1234567890123L));
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
