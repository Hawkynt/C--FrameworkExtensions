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
public class Int96Tests {

  [Test]
  public void Int96_UnsignedRightShift_Works() {
    Int96 a = 16;
    Assert.That(a >>> 2, Is.EqualTo((Int96)4));
  }

  [Test]
  public void Int96_UnsignedRightShift_Negative_ZeroFills() {
    Int96 a = Int96.MinValue;
    var result = a >>> 1;
    Assert.That(Int96.IsNegative(result), Is.False);
    Assert.That(Int96.IsPositive(result), Is.True);
  }

  [Test]
  public void Int96_UnsignedRightShift_DiffersFromRightShift_ForNegative() {
    Int96 a = -1;
    Assert.That(a >> 1, Is.EqualTo((Int96)(-1)));
    Assert.That(a >>> 1, Is.Not.EqualTo(a >> 1));
    Assert.That(Int96.IsPositive(a >>> 1), Is.True);
  }

  [Test]
  public void Int96_RotateLeft_Works() {
    Int96 a = 1;
    var result = Int96.RotateLeft(a, 4);
    Assert.That(result, Is.EqualTo((Int96)16));
  }

  [Test]
  public void Int96_RotateRight_Works() {
    Int96 a = 16;
    var result = Int96.RotateRight(a, 4);
    Assert.That(result, Is.EqualTo((Int96)1));
  }

  [Test]
  public void Int96_RotateLeft_AndRight_AreInverse() {
    Int96 a = 0x123456789ABC;
    var rotated = Int96.RotateLeft(a, 24);
    var restored = Int96.RotateRight(rotated, 24);
    Assert.That(restored, Is.EqualTo(a));
  }

  [Test]
  public void Int96_RotateRight_AndLeft_AreInverse() {
    Int96 a = 0x123456789ABC;
    var rotated = Int96.RotateRight(a, 32);
    var restored = Int96.RotateLeft(rotated, 32);
    Assert.That(restored, Is.EqualTo(a));
  }

}
