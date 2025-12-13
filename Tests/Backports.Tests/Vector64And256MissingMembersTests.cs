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
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class Vector64MissingMembersTests {
  [Test]
  public unsafe void LoadStore_Byte() {
    byte[] data = new byte[8];
    for (int i = 0; i < data.Length; i++) data[i] = (byte)i;

    fixed (byte* ptr = data) {
      Vector64<byte> v = Vector64.Load(ptr);

      byte[] outData = new byte[8];
      fixed (byte* outPtr = outData) {
        Vector64.Store(v, outPtr);
      }

      Assert.That(outData, Is.EqualTo(data));
    }
  }

  [Test]
  public void Shuffle_Byte() {
    Vector64<byte> v = Vector64.Create((byte)10, 11, 12, 13, 14, 15, 16, 17);
    Vector64<byte> indices = Vector64.Create((byte)7, 6, 5, 4, 3, 2, 1, 0);

    Vector64<byte> result = Vector64.Shuffle(v, indices);

    Assert.That(Vector64.GetElement(result, 0), Is.EqualTo(17));
    Assert.That(Vector64.GetElement(result, 7), Is.EqualTo(10));
  }

  [Test]
  public void ExtractMostSignificantBits_Byte() {
    Vector64<byte> v = Vector64.Create(
      (byte)0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00
    );

    uint mask = Vector64.ExtractMostSignificantBits(v);
    Assert.That(mask, Is.EqualTo(0x55u));
  }

  [Test]
  public void Create_Byte_Overload() {
    var v = Vector64.Create((byte)0, 1, 2, 3, 4, 5, 6, 7);
    Assert.That(Vector64.GetElement(v, 7), Is.EqualTo((byte)7));
  }
}

[TestFixture]
public class Vector256MissingMembersTests {
  [Test]
  public unsafe void LoadStore_Byte() {
    byte[] data = new byte[32];
    for (int i = 0; i < data.Length; i++) data[i] = (byte)i;

    fixed (byte* ptr = data) {
      Vector256<byte> v = Vector256.Load(ptr);

      byte[] outData = new byte[32];
      fixed (byte* outPtr = outData) {
        Vector256.Store(v, outPtr);
      }

      Assert.That(outData, Is.EqualTo(data));
    }
  }

  [Test]
  public void Create_Byte_Overload() {
    var v = Vector256.Create(
        (byte)0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
    );
    Assert.That(Vector256.GetElement(v, 31), Is.EqualTo((byte)31));
  }
}
