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
[Category("ArgumentOutOfRangeException")]
public class ArgumentOutOfRangeExceptionTests {

  #region ThrowIfZero

  [Test]
  [Category("HappyPath")]
  public void ThrowIfZero_PositiveInt_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfZero(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfZero_NegativeInt_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfZero(-1));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfZero_ZeroInt_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfZero(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfZero_PositiveDouble_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfZero(0.1d));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfZero_ZeroDouble_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfZero(0.0d));
  }

  #endregion

  #region ThrowIfNegative

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegative_PositiveInt_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegative(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegative_ZeroInt_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegative(0));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegative_NegativeInt_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegative(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ThrowIfNegative_MinValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegative(int.MinValue));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegative_PositiveDecimal_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegative(0.001m));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegative_NegativeDecimal_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegative(-0.001m));
  }

  #endregion

  #region ThrowIfNegativeOrZero

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNegativeOrZero_PositiveInt_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(1));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegativeOrZero_ZeroInt_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(0));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNegativeOrZero_NegativeInt_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ThrowIfNegativeOrZero_MinPositiveDouble_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(double.Epsilon));
  }

  #endregion

  #region ThrowIfGreaterThan

  [Test]
  [Category("HappyPath")]
  public void ThrowIfGreaterThan_LessValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(5, 10));
  }

  [Test]
  [Category("EdgeCase")]
  public void ThrowIfGreaterThan_EqualValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfGreaterThan_GreaterValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfGreaterThan(15, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfGreaterThan_StringComparison_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThan("apple", "banana"));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfGreaterThan_StringComparison_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfGreaterThan("cherry", "banana"));
  }

  #endregion

  #region ThrowIfGreaterThanOrEqual

  [Test]
  [Category("HappyPath")]
  public void ThrowIfGreaterThanOrEqual_LessValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(5, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfGreaterThanOrEqual_EqualValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfGreaterThanOrEqual_GreaterValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(15, 10));
  }

  #endregion

  #region ThrowIfLessThan

  [Test]
  [Category("HappyPath")]
  public void ThrowIfLessThan_GreaterValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfLessThan(15, 10));
  }

  [Test]
  [Category("EdgeCase")]
  public void ThrowIfLessThan_EqualValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfLessThan(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfLessThan_LessValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfLessThan(5, 10));
  }

  #endregion

  #region ThrowIfLessThanOrEqual

  [Test]
  [Category("HappyPath")]
  public void ThrowIfLessThanOrEqual_GreaterValue_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(15, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfLessThanOrEqual_EqualValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfLessThanOrEqual_LessValue_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(5, 10));
  }

  #endregion

  #region ThrowIfEqual

  [Test]
  [Category("HappyPath")]
  public void ThrowIfEqual_DifferentValues_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfEqual(5, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfEqual_SameValues_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfEqual(10, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfEqual_DifferentStrings_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfEqual("hello", "world"));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfEqual_SameStrings_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfEqual("hello", "hello"));
  }

  #endregion

  #region ThrowIfNotEqual

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNotEqual_SameValues_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNotEqual(10, 10));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNotEqual_DifferentValues_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNotEqual(5, 10));
  }

  [Test]
  [Category("HappyPath")]
  public void ThrowIfNotEqual_SameStrings_DoesNotThrow() {
    Assert.DoesNotThrow(() => ArgumentOutOfRangeException.ThrowIfNotEqual("hello", "hello"));
  }

  [Test]
  [Category("Exception")]
  public void ThrowIfNotEqual_DifferentStrings_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentOutOfRangeException.ThrowIfNotEqual("hello", "world"));
  }

  #endregion

}
