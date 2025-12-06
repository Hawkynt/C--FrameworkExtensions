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
[Category("Math")]
public class MathTests {

  #region Math.Clamp (Int32)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5, 0, 10);
    Assert.That(result, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5, 0, 10);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Int_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15, 0, 10);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_ValueEqualsMin_ReturnsMin() {
    var result = Math.Clamp(0, 0, 10);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_ValueEqualsMax_ReturnsMax() {
    var result = Math.Clamp(10, 0, 10);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clamp_Int_MinEqualsMax_ReturnsMinMax() {
    var result = Math.Clamp(5, 3, 3);
    Assert.That(result, Is.EqualTo(3));
  }

  #endregion

  #region Math.Clamp (Double)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(5.5));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(0.0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Double_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15.5, 0.0, 10.0);
    Assert.That(result, Is.EqualTo(10.0));
  }

  #endregion

  #region Math.Clamp (Single)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(5.5f));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(0.0f));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Single_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15.5f, 0.0f, 10.0f);
    Assert.That(result, Is.EqualTo(10.0f));
  }

  #endregion

  #region Math.Clamp (Long)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp(5L, 0L, 10L);
    Assert.That(result, Is.EqualTo(5L));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp(-5L, 0L, 10L);
    Assert.That(result, Is.EqualTo(0L));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Long_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp(15L, 0L, 10L);
    Assert.That(result, Is.EqualTo(10L));
  }

  #endregion

  #region Math.Clamp (Byte)

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueWithinRange_ReturnsValue() {
    var result = Math.Clamp((byte)50, (byte)0, (byte)100);
    Assert.That(result, Is.EqualTo((byte)50));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueBelowMin_ReturnsMin() {
    var result = Math.Clamp((byte)0, (byte)10, (byte)100);
    Assert.That(result, Is.EqualTo((byte)10));
  }

  [Test]
  [Category("HappyPath")]
  public void Clamp_Byte_ValueAboveMax_ReturnsMax() {
    var result = Math.Clamp((byte)150, (byte)0, (byte)100);
    Assert.That(result, Is.EqualTo((byte)100));
  }

  #endregion

}
