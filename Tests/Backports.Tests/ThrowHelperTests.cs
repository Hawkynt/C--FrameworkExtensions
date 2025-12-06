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
[Category("ThrowHelpers")]
public class ThrowHelperTests {

  #region ArgumentNullException.ThrowIfNull

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNull_WithNonNullValue_DoesNotThrow() {
    var value = new object();
    Assert.DoesNotThrow(() => ArgumentNullException.ThrowIfNull(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNull_WithNullValue_ThrowsArgumentNullException() {
    object value = null;
    Assert.Throws<ArgumentNullException>(() => ArgumentNullException.ThrowIfNull(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNull_WithNullValue_ContainsCorrectParamName() {
    object myParameter = null;
    var ex = Assert.Throws<ArgumentNullException>(() => ArgumentNullException.ThrowIfNull(myParameter));
    Assert.That(ex.ParamName, Is.EqualTo("myParameter"));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNull_WithNonNullString_DoesNotThrow() {
    var value = "test";
    Assert.DoesNotThrow(() => ArgumentNullException.ThrowIfNull(value));
  }

  #endregion

  #region ArgumentException.ThrowIfNullOrEmpty

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNullOrEmpty_WithNonEmptyString_DoesNotThrow() {
    var value = "test";
    Assert.DoesNotThrow(() => ArgumentException.ThrowIfNullOrEmpty(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNullOrEmpty_WithNullString_ThrowsArgumentNullException() {
    string value = null;
    Assert.Throws<ArgumentNullException>(() => ArgumentException.ThrowIfNullOrEmpty(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNullOrEmpty_WithEmptyString_ThrowsArgumentException() {
    var value = string.Empty;
    Assert.Throws<ArgumentException>(() => ArgumentException.ThrowIfNullOrEmpty(value));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNullOrWhiteSpace_WithNonWhitespaceString_DoesNotThrow() {
    var value = "test";
    Assert.DoesNotThrow(() => ArgumentException.ThrowIfNullOrWhiteSpace(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNullOrWhiteSpace_WithNullString_ThrowsArgumentNullException() {
    string value = null;
    Assert.Throws<ArgumentNullException>(() => ArgumentException.ThrowIfNullOrWhiteSpace(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNullOrWhiteSpace_WithEmptyString_ThrowsArgumentException() {
    var value = string.Empty;
    Assert.Throws<ArgumentException>(() => ArgumentException.ThrowIfNullOrWhiteSpace(value));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNullOrWhiteSpace_WithWhitespaceString_ThrowsArgumentException() {
    var value = "   ";
    Assert.Throws<ArgumentException>(() => ArgumentException.ThrowIfNullOrWhiteSpace(value));
  }

  #endregion

  #region ArgumentOutOfRangeException ThrowIf methods

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegative_WithPositiveValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegative(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegative_WithZero_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegative(0));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegative_WithNegativeValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegative(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegativeOrZero_WithPositiveValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(1));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegativeOrZero_WithZero_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(0));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegativeOrZero_WithNegativeValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfZero_WithNonZeroValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfZero(1));
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfZero(-1));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfZero_WithZero_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfZero(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfGreaterThan_WithLesserValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(5, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfGreaterThan_WithEqualValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfGreaterThan_WithGreaterValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(15, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfLessThan_WithGreaterValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfLessThan(15, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfLessThan_WithEqualValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfLessThan(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfLessThan_WithLesserValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfLessThan(5, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfEqual_WithDifferentValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfEqual(5, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfEqual_WithEqualValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfEqual(10, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNotEqual_WithEqualValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNotEqual(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNotEqual_WithDifferentValue_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNotEqual(5, 10));
  }

  #endregion

  #region ObjectDisposedException.ThrowIf

  [Test]
  [Category("HappyPath")]
  public void ObjectDisposedExceptionThrowIf_WhenNotDisposed_DoesNotThrow() {
    Assert.DoesNotThrow(() => ObjectDisposedException.ThrowIf(false, typeof(ThrowHelperTests)));
  }

  [Test]
  [Category("Exception")]
  public void ObjectDisposedExceptionThrowIf_WhenDisposed_ThrowsObjectDisposedException() {
    Assert.Throws<ObjectDisposedException>(() => ObjectDisposedException.ThrowIf(true, typeof(ThrowHelperTests)));
  }

  [Test]
  [Category("HappyPath")]
  public void ObjectDisposedExceptionThrowIf_WithInstance_WhenNotDisposed_DoesNotThrow() {
    var instance = new object();
    Assert.DoesNotThrow(() => ObjectDisposedException.ThrowIf(false, instance));
  }

  [Test]
  [Category("Exception")]
  public void ObjectDisposedExceptionThrowIf_WithInstance_WhenDisposed_ThrowsObjectDisposedException() {
    var instance = new object();
    Assert.Throws<ObjectDisposedException>(() => ObjectDisposedException.ThrowIf(true, instance));
  }

  #endregion

}
