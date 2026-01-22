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

namespace Corlib.Tests.System;

[TestFixture]
public class UInt96Tests {

  [Test]
  public void UInt96_UnsignedRightShift_Works() {
    UInt96 a = 16;
    Assert.That(a >>> 2, Is.EqualTo((UInt96)4));
  }

  [Test]
  public void UInt96_UnsignedRightShift_SameAsRightShift_ForUnsignedType() {
    var a = UInt96.MaxValue;
    Assert.That(a >>> 4, Is.EqualTo(a >> 4));
  }

  [Test]
  public void UInt96_UnsignedRightShift_ByZero_ReturnsOriginal() {
    UInt96 a = 0x123456;
    Assert.That(a >>> 0, Is.EqualTo(a));
  }

  [Test]
  public void UInt96_UnsignedRightShift_LargeShift_Works() {
    UInt96 a = UInt96.MaxValue;
    var result = a >>> 48;
    Assert.That(result, Is.Not.EqualTo(UInt96.Zero));
  }

  [Test]
  public void UInt96_RotateLeft_Works() {
    UInt96 a = 1;
    var result = UInt96.RotateLeft(a, 4);
    Assert.That(result, Is.EqualTo((UInt96)16));
  }

  [Test]
  public void UInt96_RotateRight_Works() {
    UInt96 a = 16;
    var result = UInt96.RotateRight(a, 4);
    Assert.That(result, Is.EqualTo((UInt96)1));
  }

  [Test]
  public void UInt96_RotateLeft_AndRight_AreInverse() {
    UInt96 a = 0x123456789ABC;
    var rotated = UInt96.RotateLeft(a, 24);
    var restored = UInt96.RotateRight(rotated, 24);
    Assert.That(restored, Is.EqualTo(a));
  }

  [Test]
  public void UInt96_RotateRight_AndLeft_AreInverse() {
    UInt96 a = 0x123456789ABC;
    var rotated = UInt96.RotateRight(a, 32);
    var restored = UInt96.RotateLeft(rotated, 32);
    Assert.That(restored, Is.EqualTo(a));
  }

  [Test]
  public void UInt96_RotateLeftExtension_Works() {
    UInt96 a = 1;
    var result = a.RotateLeft(4);
    Assert.That(result, Is.EqualTo((UInt96)16));
  }

  [Test]
  public void UInt96_RotateRightExtension_Works() {
    UInt96 a = 16;
    var result = a.RotateRight(4);
    Assert.That(result, Is.EqualTo((UInt96)1));
  }

}
