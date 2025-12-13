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
public class Vector512MissingMembersTests {
  [Test]
  public unsafe void LoadStore_Byte() {
    byte[] data = new byte[64];
    for (int i = 0; i < data.Length; i++) data[i] = (byte)i;

    fixed (byte* ptr = data) {
      Vector512<byte> v = Vector512.Load(ptr);

      byte[] outData = new byte[64];
      fixed (byte* outPtr = outData) {
        Vector512.Store(v, outPtr);
      }

      Assert.That(outData, Is.EqualTo(data));
    }
  }

  [Test]
  public void Shuffle_Byte() {
    Vector512<byte> v = Vector512.CreateScalar((byte)0);
    Vector512<byte> indices = Vector512.CreateScalar((byte)0);

    for (int i = 0; i < 64; i++) v = v.WithElement(i, (byte)i);
    for (int i = 0; i < 64; i++) indices = indices.WithElement(i, (byte)(63 - i));

    Vector512<byte> result = Vector512.Shuffle(v, indices);

    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(63));
    Assert.That(Vector512.GetElement(result, 63), Is.EqualTo(0));
  }

  [Test]
  public void ExtractMostSignificantBits_Byte() {
    Vector512<byte> v = Vector512.CreateScalar((byte)0);
    for (int i = 0; i < 64; i += 2) v = v.WithElement(i, (byte)0x80);

    ulong mask = Vector512.ExtractMostSignificantBits(v);
    Assert.That(mask, Is.EqualTo(0x5555555555555555UL));
  }

  [Test]
  public void Create_Double_Overload() {
    var v = Vector512.Create(0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0);
    Assert.That(Vector512.GetElement(v, 7), Is.EqualTo(7.0));
  }

  [Test]
  public void Indices_Int32() {
    Vector512<int> result = Vector512<int>.Indices;
    Assert.That(Vector512.GetElement(result, 0), Is.EqualTo(0));
    Assert.That(Vector512.GetElement(result, 15), Is.EqualTo(15));
  }
}
