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
public class Vector128MissingMembersTests {
  [Test]
  public unsafe void LoadStore_Byte() {
    byte[] data = new byte[16];
    for (int i = 0; i < data.Length; i++) data[i] = (byte)i;

    fixed (byte* ptr = data) {
      Vector128<byte> v = Vector128.Load(ptr);
      
      byte[] outData = new byte[16];
      fixed (byte* outPtr = outData) {
        Vector128.Store<byte>(v, outPtr);
      }

      Assert.That(outData, Is.EqualTo(data));
    }
  }

  [Test]
  public void Shuffle_Byte() {
    Vector128<byte> v = Vector128.Create((byte)10, (byte)11, (byte)12, (byte)13, (byte)14, (byte)15, (byte)16, (byte)17, (byte)18, (byte)19, (byte)20, (byte)21, (byte)22, (byte)23, (byte)24, (byte)25);
    Vector128<byte> indices = Vector128.Create((byte)15, (byte)14, (byte)13, (byte)12, (byte)11, (byte)10, (byte)9, (byte)8, (byte)7, (byte)6, (byte)5, (byte)4, (byte)3, (byte)2, (byte)1, (byte)0);

    Vector128<byte> result = Vector128.Shuffle(v, indices);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(25));
    Assert.That(Vector128.GetElement(result, 15), Is.EqualTo(10));
  }

  [Test]
  public void ExtractMostSignificantBits_Byte() {
    // 0x80 has MSB set (10000000), 0x7F has MSB unset (01111111)
    Vector128<byte> v = Vector128.Create(
      (byte)0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00,
      0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00
    );

    uint mask = Vector128.ExtractMostSignificantBits(v);
    // Expecting bits 0, 2, 4, ... 14 to be set.
    // Binary: 0101 0101 0101 0101 = 0x5555
    Assert.That(mask, Is.EqualTo(0x5555u));
  }

  [Test]
  public void ConditionalSelect_Int32() {
    Vector128<int> left = Vector128.Create(1);
    Vector128<int> right = Vector128.Create(2);
    // Condition: All bits set for true, zero for false
    Vector128<int> condition = Vector128.Create(-1, 0, -1, 0);

    Vector128<int> result = Vector128.ConditionalSelect(condition, left, right);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(2));
  }

  [Test]
  public void ShiftLeft_Int32() {
    Vector128<int> v = Vector128.Create(1);
    Vector128<int> result = Vector128.ShiftLeft(v, 1);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(2));
  }

  [Test]
  public void ShiftRightLogical_Int32() {
    Vector128<int> v = Vector128.Create(-1); // All bits set
    Vector128<int> result = Vector128.ShiftRightLogical(v, 1);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(int.MaxValue)); // Sign bit cleared
  }

  [Test]
  public void ShiftRightArithmetic_Int32() {
    Vector128<int> v = Vector128.Create(-2);
    Vector128<int> result = Vector128.ShiftRightArithmetic(v, 1);
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(-1)); // Sign bit preserved
  }

  [Test]
  public void Indices_Int32() {
    Vector128<int> result = Vector128<int>.Indices;
    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(0));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(1));
    Assert.That(Vector128.GetElement(result, 2), Is.EqualTo(2));
    Assert.That(Vector128.GetElement(result, 3), Is.EqualTo(3));
  }

  [Test]
  public void Create_Byte_Overload() {
    var v = Vector128.Create((byte)0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
    Assert.That(Vector128.GetElement(v, 15), Is.EqualTo((byte)15));
  }
}