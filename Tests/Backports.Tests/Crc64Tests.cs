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
using System.IO.Hashing;
using System.Text;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Crc64")]
public class Crc64Tests {

  #region Static Hash Methods - Array

  [Test]
  [Category("HappyPath")]
  public void Hash_EmptyArray_ReturnsConsistentHash() {
    var data = Array.Empty<byte>();
    var hash1 = Crc64.Hash(data);
    var hash2 = Crc64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
    Assert.That(hash1, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_SimpleData_ReturnsExpectedLength() {
    var data = Encoding.UTF8.GetBytes("Hello, World!");
    var hash = Crc64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_SameData_ReturnsSameHash() {
    var data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = Crc64.Hash(data);
    var hash2 = Crc64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_DifferentData_ReturnsDifferentHashes() {
    var data1 = Encoding.UTF8.GetBytes("Hello");
    var data2 = Encoding.UTF8.GetBytes("World");
    var hash1 = Crc64.Hash(data1);
    var hash2 = Crc64.Hash(data2);

    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Static Hash Methods - Span

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_EmptyData_ReturnsConsistentHash() {
    ReadOnlySpan<byte> data = Array.Empty<byte>();
    var hash1 = Crc64.Hash(data);
    var hash2 = Crc64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
    Assert.That(hash1, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_SimpleData_ReturnsExpectedLength() {
    ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("Hello, World!");
    var hash = Crc64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_SameData_ReturnsSameHash() {
    ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = Crc64.Hash(data);
    var hash2 = Crc64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  #endregion

  #region Instance Methods

  [Test]
  [Category("HappyPath")]
  public void Append_MultipleChunks_ProducesConsistentHash() {
    var data = Encoding.UTF8.GetBytes("Hello, World! This is a longer test string.");

    var hasher1 = new Crc64();
    hasher1.Append(data);
    var hash1 = hasher1.GetCurrentHash();

    var hasher2 = new Crc64();
    var chunk1 = new byte[10];
    var chunk2 = new byte[10];
    var chunk3 = new byte[data.Length - 20];
    Array.Copy(data, 0, chunk1, 0, 10);
    Array.Copy(data, 10, chunk2, 0, 10);
    Array.Copy(data, 20, chunk3, 0, data.Length - 20);
    hasher2.Append(chunk1);
    hasher2.Append(chunk2);
    hasher2.Append(chunk3);
    var hash2 = hasher2.GetCurrentHash();

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Reset_AfterAppend_ClearsState() {
    var hasher = new Crc64();
    hasher.Append(Encoding.UTF8.GetBytes("Some data"));
    hasher.Reset();
    var hash = hasher.GetCurrentHash();

    var freshHasher = new Crc64();
    var freshHash = freshHasher.GetCurrentHash();

    Assert.That(hash, Is.EqualTo(freshHash));
  }

  [Test]
  [Category("HappyPath")]
  public void GetCurrentHash_ReturnsCorrectLength() {
    var hasher = new Crc64();
    hasher.Append(Encoding.UTF8.GetBytes("Test"));
    var hash = hasher.GetCurrentHash();

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  #endregion
  
  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Hash_NullArray_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Crc64.Hash(null!));
  }

  [Test]
  [Category("EdgeCase")]
  public void Append_NullArray_ThrowsArgumentNullException() {
    var hasher = new Crc64();
    Assert.Throws<ArgumentNullException>(() => hasher.Append(((byte[])null)!));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_SingleByte_ReturnsValidHash() {
    var data = new byte[] { 0x42 };
    var hash = Crc64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_AllZeros_ReturnsValidHash() {
    var data = new byte[256];
    var hash = Crc64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_AllOnes_ReturnsValidHash() {
    var data = new byte[256];
    for (var i = 0; i < data.Length; ++i)
      data[i] = 0xFF;

    var hash = Crc64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Array_And_Span_ProduceSameHash() {
    var data = Encoding.UTF8.GetBytes("Test data for comparison");
    var arrayHash = Crc64.Hash(data);
    var spanHash = Crc64.Hash((ReadOnlySpan<byte>)data);

    Assert.That(arrayHash, Is.EqualTo(spanHash));
  }

  #endregion

}
