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
public class ConvertTests {

  #region ToHexString

  [Test]
  [Category("HappyPath")]
  public void ToHexString_ByteArray_ReturnsHex() {
    var bytes = new byte[] { 0x12, 0xAB, 0xCD };
    var result = Convert.ToHexString(bytes);
    Assert.That(result, Is.EqualTo("12ABCD"));
  }

  [Test]
  [Category("HappyPath")]
  public void ToHexString_EmptyArray_ReturnsEmpty() {
    var result = Convert.ToHexString(new byte[0]);
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  #endregion

  #region FromHexString

  [Test]
  [Category("HappyPath")]
  public void FromHexString_ValidHex_ReturnsBytes() {
    var result = Convert.FromHexString("12ABCD");
    Assert.That(result, Is.EqualTo(new byte[] { 0x12, 0xAB, 0xCD }));
  }

  [Test]
  [Category("HappyPath")]
  public void FromHexString_LowercaseHex_ReturnsBytes() {
    var result = Convert.FromHexString("12abcd");
    Assert.That(result, Is.EqualTo(new byte[] { 0x12, 0xAB, 0xCD }));
  }

  [Test]
  [Category("HappyPath")]
  public void FromHexString_EmptyString_ReturnsEmptyArray() {
    var result = Convert.FromHexString("");
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void FromHexString_OddLength_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => Convert.FromHexString("123"));
  }

  [Test]
  [Category("Exception")]
  public void FromHexString_InvalidChar_ThrowsFormatException() {
    Assert.Throws<FormatException>(() => Convert.FromHexString("1G"));
  }

  #endregion

}
