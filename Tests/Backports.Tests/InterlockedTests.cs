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

using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Interlocked")]
public class InterlockedTests {

  #region Interlocked.And (int)

  [Test]
  [Category("HappyPath")]
  public void And_Int32_ReturnsOriginalAndAppliesAnd() {
    var value = 0b1111;
    var original = Interlocked.And(ref value, 0b1010);
    Assert.That(original, Is.EqualTo(0b1111));
    Assert.That(value, Is.EqualTo(0b1010));
  }

  [Test]
  [Category("HappyPath")]
  public void And_Int32_ClearsAllBits() {
    var value = 0xFF;
    Interlocked.And(ref value, 0x00);
    Assert.That(value, Is.EqualTo(0x00));
  }

  [Test]
  [Category("HappyPath")]
  public void And_Int32_PreservesBits() {
    var value = 0xFF;
    Interlocked.And(ref value, 0xFF);
    Assert.That(value, Is.EqualTo(0xFF));
  }

  #endregion

  #region Interlocked.And (long)

  [Test]
  [Category("HappyPath")]
  public void And_Int64_ReturnsOriginalAndAppliesAnd() {
    var value = 0xFFFFL;
    var original = Interlocked.And(ref value, 0x0F0FL);
    Assert.That(original, Is.EqualTo(0xFFFFL));
    Assert.That(value, Is.EqualTo(0x0F0FL));
  }

  #endregion

  #region Interlocked.And (uint)

  [Test]
  [Category("HappyPath")]
  public void And_UInt32_ReturnsOriginalAndAppliesAnd() {
    var value = 0xFFFFu;
    var original = Interlocked.And(ref value, 0x0F0Fu);
    Assert.That(original, Is.EqualTo(0xFFFFu));
    Assert.That(value, Is.EqualTo(0x0F0Fu));
  }

  #endregion

  #region Interlocked.And (ulong)

  [Test]
  [Category("HappyPath")]
  public void And_UInt64_ReturnsOriginalAndAppliesAnd() {
    var value = 0xFFFFFFFFul;
    var original = Interlocked.And(ref value, 0x0F0F0F0Ful);
    Assert.That(original, Is.EqualTo(0xFFFFFFFFul));
    Assert.That(value, Is.EqualTo(0x0F0F0F0Ful));
  }

  #endregion

  #region Interlocked.Or (int)

  [Test]
  [Category("HappyPath")]
  public void Or_Int32_ReturnsOriginalAndAppliesOr() {
    var value = 0b0101;
    var original = Interlocked.Or(ref value, 0b1010);
    Assert.That(original, Is.EqualTo(0b0101));
    Assert.That(value, Is.EqualTo(0b1111));
  }

  [Test]
  [Category("HappyPath")]
  public void Or_Int32_SetsBits() {
    var value = 0x00;
    Interlocked.Or(ref value, 0xFF);
    Assert.That(value, Is.EqualTo(0xFF));
  }

  [Test]
  [Category("HappyPath")]
  public void Or_Int32_NoChangeWhenZero() {
    var value = 0xFF;
    Interlocked.Or(ref value, 0x00);
    Assert.That(value, Is.EqualTo(0xFF));
  }

  #endregion

  #region Interlocked.Or (long)

  [Test]
  [Category("HappyPath")]
  public void Or_Int64_ReturnsOriginalAndAppliesOr() {
    var value = 0x0000L;
    var original = Interlocked.Or(ref value, 0xFFFFL);
    Assert.That(original, Is.EqualTo(0x0000L));
    Assert.That(value, Is.EqualTo(0xFFFFL));
  }

  #endregion

  #region Interlocked.Or (uint)

  [Test]
  [Category("HappyPath")]
  public void Or_UInt32_ReturnsOriginalAndAppliesOr() {
    var value = 0x0000u;
    var original = Interlocked.Or(ref value, 0xFFFFu);
    Assert.That(original, Is.EqualTo(0x0000u));
    Assert.That(value, Is.EqualTo(0xFFFFu));
  }

  #endregion

  #region Interlocked.Or (ulong)

  [Test]
  [Category("HappyPath")]
  public void Or_UInt64_ReturnsOriginalAndAppliesOr() {
    var value = 0x00000000ul;
    var original = Interlocked.Or(ref value, 0xFFFFFFFFul);
    Assert.That(original, Is.EqualTo(0x00000000ul));
    Assert.That(value, Is.EqualTo(0xFFFFFFFFul));
  }

  #endregion

}
