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
[Category("Version")]
public class VersionPolyfillTests {

  #region Version.Parse(string)

  [Test]
  [Category("HappyPath")]
  public void Version_Parse_TwoComponents_ReturnsVersion() {
    var result = Version.Parse("1.2");
    Assert.That(result.Major, Is.EqualTo(1));
    Assert.That(result.Minor, Is.EqualTo(2));
    Assert.That(result.Build, Is.EqualTo(-1));
    Assert.That(result.Revision, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_Parse_ThreeComponents_ReturnsVersion() {
    var result = Version.Parse("1.2.3");
    Assert.That(result.Major, Is.EqualTo(1));
    Assert.That(result.Minor, Is.EqualTo(2));
    Assert.That(result.Build, Is.EqualTo(3));
    Assert.That(result.Revision, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_Parse_FourComponents_ReturnsVersion() {
    var result = Version.Parse("1.2.3.4");
    Assert.That(result.Major, Is.EqualTo(1));
    Assert.That(result.Minor, Is.EqualTo(2));
    Assert.That(result.Build, Is.EqualTo(3));
    Assert.That(result.Revision, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_Parse_ZeroVersion_ReturnsVersion() {
    var result = Version.Parse("0.0.0.0");
    Assert.That(result.Major, Is.EqualTo(0));
    Assert.That(result.Minor, Is.EqualTo(0));
    Assert.That(result.Build, Is.EqualTo(0));
    Assert.That(result.Revision, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_Parse_LargeNumbers_ReturnsVersion() {
    var result = Version.Parse("65535.65535.65535.65535");
    Assert.That(result.Major, Is.EqualTo(65535));
    Assert.That(result.Minor, Is.EqualTo(65535));
    Assert.That(result.Build, Is.EqualTo(65535));
    Assert.That(result.Revision, Is.EqualTo(65535));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_InvalidFormat_ThrowsException() {
    Assert.Throws<ArgumentException>(() => Version.Parse("not-a-version"));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_Null_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Version.Parse(null));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_Empty_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Version.Parse(""));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_SingleComponent_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Version.Parse("1"));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_TooManyComponents_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Version.Parse("1.2.3.4.5"));
  }

  [Test]
  [Category("Exception")]
  public void Version_Parse_NegativeComponent_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Version.Parse("1.-2.3.4"));
  }

  #endregion

  #region Version.TryParse(string)

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_ValidVersion_ReturnsTrue() {
    var success = Version.TryParse("1.2.3.4", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Major, Is.EqualTo(1));
    Assert.That(result.Minor, Is.EqualTo(2));
    Assert.That(result.Build, Is.EqualTo(3));
    Assert.That(result.Revision, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_TwoComponents_ReturnsTrue() {
    var success = Version.TryParse("2.5", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Major, Is.EqualTo(2));
    Assert.That(result.Minor, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_ThreeComponents_ReturnsTrue() {
    var success = Version.TryParse("3.6.9", out var result);
    Assert.That(success, Is.True);
    Assert.That(result.Major, Is.EqualTo(3));
    Assert.That(result.Minor, Is.EqualTo(6));
    Assert.That(result.Build, Is.EqualTo(9));
  }

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_InvalidVersion_ReturnsFalse() {
    var success = Version.TryParse("invalid", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_Null_ReturnsFalse() {
    var success = Version.TryParse(null, out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Version_TryParse_Empty_ReturnsFalse() {
    var success = Version.TryParse("", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Version_TryParse_SingleComponent_ReturnsFalse() {
    var success = Version.TryParse("1", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Version_TryParse_TooManyComponents_ReturnsFalse() {
    var success = Version.TryParse("1.2.3.4.5", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Version_TryParse_NegativeComponent_ReturnsFalse() {
    var success = Version.TryParse("1.-2.3.4", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Version_TryParse_WhitespaceOnly_ReturnsFalse() {
    var success = Version.TryParse("   ", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Version_TryParse_NonNumeric_ReturnsFalse() {
    var success = Version.TryParse("a.b.c.d", out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.Null);
  }

  #endregion

}
