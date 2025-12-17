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
public class CharTests {

  #region IsAscii

  [Test]
  [Category("HappyPath")]
  public void IsAscii_AsciiChar_ReturnsTrue() {
    Assert.That(char.IsAscii('A'), Is.True);
    Assert.That(char.IsAscii('z'), Is.True);
    Assert.That(char.IsAscii('0'), Is.True);
    Assert.That(char.IsAscii(' '), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAscii_NonAsciiChar_ReturnsFalse() {
    Assert.That(char.IsAscii('ä'), Is.False);
    Assert.That(char.IsAscii('ü'), Is.False);
    Assert.That(char.IsAscii('\u0080'), Is.False);
  }

  #endregion

  #region IsAsciiDigit

  [Test]
  [Category("HappyPath")]
  public void IsAsciiDigit_Digit_ReturnsTrue() {
    for (var c = '0'; c <= '9'; ++c)
      Assert.That(char.IsAsciiDigit(c), Is.True, $"'{c}' should be ASCII digit");
  }

  [Test]
  [Category("HappyPath")]
  public void IsAsciiDigit_NonDigit_ReturnsFalse() {
    Assert.That(char.IsAsciiDigit('A'), Is.False);
    Assert.That(char.IsAsciiDigit('z'), Is.False);
  }

  #endregion

  #region IsAsciiLetter

  [Test]
  [Category("HappyPath")]
  public void IsAsciiLetter_Letter_ReturnsTrue() {
    Assert.That(char.IsAsciiLetter('A'), Is.True);
    Assert.That(char.IsAsciiLetter('Z'), Is.True);
    Assert.That(char.IsAsciiLetter('a'), Is.True);
    Assert.That(char.IsAsciiLetter('z'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAsciiLetter_NonLetter_ReturnsFalse() {
    Assert.That(char.IsAsciiLetter('0'), Is.False);
    Assert.That(char.IsAsciiLetter(' '), Is.False);
  }

  #endregion

  #region IsAsciiHexDigit

  [Test]
  [Category("HappyPath")]
  public void IsAsciiHexDigit_HexDigit_ReturnsTrue() {
    Assert.That(char.IsAsciiHexDigit('0'), Is.True);
    Assert.That(char.IsAsciiHexDigit('9'), Is.True);
    Assert.That(char.IsAsciiHexDigit('A'), Is.True);
    Assert.That(char.IsAsciiHexDigit('F'), Is.True);
    Assert.That(char.IsAsciiHexDigit('a'), Is.True);
    Assert.That(char.IsAsciiHexDigit('f'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAsciiHexDigit_NonHexDigit_ReturnsFalse() {
    Assert.That(char.IsAsciiHexDigit('G'), Is.False);
    Assert.That(char.IsAsciiHexDigit('g'), Is.False);
  }

  #endregion

}
