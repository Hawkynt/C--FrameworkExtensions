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
[Category("Guid")]
public class GuidPolyfillTests {

  private static readonly Guid TestGuid = new("12345678-1234-1234-1234-123456789ABC");

  #region Guid.Parse(string)

  [Test]
  [Category("HappyPath")]
  public void Guid_Parse_StandardFormat_ReturnsGuid() {
    var result = Guid.Parse("12345678-1234-1234-1234-123456789ABC");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_Parse_LowerCase_ReturnsGuid() {
    var result = Guid.Parse("12345678-1234-1234-1234-123456789abc");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_Parse_WithBraces_ReturnsGuid() {
    var result = Guid.Parse("{12345678-1234-1234-1234-123456789ABC}");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_Parse_WithParentheses_ReturnsGuid() {
    var result = Guid.Parse("(12345678-1234-1234-1234-123456789ABC)");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_Parse_NoDashes_ReturnsGuid() {
    var result = Guid.Parse("123456781234123412341234567890AB");
    Assert.That(result, Is.Not.EqualTo(Guid.Empty));
  }

  [Test]
  [Category("Exception")]
  public void Guid_Parse_InvalidFormat_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => Guid.Parse("not-a-guid"));
  }

  [Test]
  [Category("Exception")]
  public void Guid_Parse_Null_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Guid.Parse(null));
  }

  [Test]
  [Category("Exception")]
  public void Guid_Parse_Empty_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => Guid.Parse(""));
  }

  #endregion

  #region Guid.TryParse(string)

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_ValidGuid_ReturnsTrue() {
    var success = Guid.TryParse("12345678-1234-1234-1234-123456789ABC", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_WithBraces_ReturnsTrue() {
    var success = Guid.TryParse("{12345678-1234-1234-1234-123456789ABC}", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_InvalidGuid_ReturnsFalse() {
    var success = Guid.TryParse("not-a-guid", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(Guid.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_Null_ReturnsFalse() {
    var success = Guid.TryParse(null, out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(Guid.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParse_Empty_ReturnsFalse() {
    var success = Guid.TryParse("", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(Guid.Empty));
  }

  #endregion

  #region Guid.ParseExact(string, string)

  [Test]
  [Category("HappyPath")]
  public void Guid_ParseExact_FormatD_ReturnsGuid() {
    var result = Guid.ParseExact("12345678-1234-1234-1234-123456789ABC", "D");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_ParseExact_FormatN_ReturnsGuid() {
    var result = Guid.ParseExact("123456781234123412341234567890AB", "N");
    Assert.That(result, Is.Not.EqualTo(Guid.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_ParseExact_FormatB_ReturnsGuid() {
    var result = Guid.ParseExact("{12345678-1234-1234-1234-123456789ABC}", "B");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_ParseExact_FormatP_ReturnsGuid() {
    var result = Guid.ParseExact("(12345678-1234-1234-1234-123456789ABC)", "P");
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("Exception")]
  public void Guid_ParseExact_WrongFormat_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => Guid.ParseExact("{12345678-1234-1234-1234-123456789ABC}", "D"));
  }

  #endregion

  #region Guid.TryParseExact(string, string)

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParseExact_FormatD_ReturnsTrue() {
    var success = Guid.TryParseExact("12345678-1234-1234-1234-123456789ABC", "D", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParseExact_FormatB_ReturnsTrue() {
    var success = Guid.TryParseExact("{12345678-1234-1234-1234-123456789ABC}", "B", out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(TestGuid));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParseExact_WrongFormat_ReturnsFalse() {
    var success = Guid.TryParseExact("{12345678-1234-1234-1234-123456789ABC}", "D", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(Guid.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Guid_TryParseExact_NullInput_ReturnsFalse() {
    var success = Guid.TryParseExact(null, "D", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(Guid.Empty));
  }

  #endregion

}
