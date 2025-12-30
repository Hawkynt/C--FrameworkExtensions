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
[Category("XxHash64")]
public class XxHash64Tests {

  #region Static Hash Methods - Array

  [Test]
  [Category("HappyPath")]
  public void Hash_EmptyArray_ReturnsConsistentHash() {
    var data = Array.Empty<byte>();
    var hash1 = XxHash64.Hash(data);
    var hash2 = XxHash64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
    Assert.That(hash1, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_SimpleData_ReturnsExpectedLength() {
    var data = Encoding.UTF8.GetBytes("Hello, World!");
    var hash = XxHash64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_SameDataSameSeed_ReturnsSameHash() {
    var data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = XxHash64.Hash(data, 42);
    var hash2 = XxHash64.Hash(data, 42);

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_DifferentSeeds_ReturnsDifferentHashes() {
    var data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = XxHash64.Hash(data, 0);
    var hash2 = XxHash64.Hash(data, 1);

    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_DifferentData_ReturnsDifferentHashes() {
    var data1 = Encoding.UTF8.GetBytes("Hello");
    var data2 = Encoding.UTF8.GetBytes("World");
    var hash1 = XxHash64.Hash(data1);
    var hash2 = XxHash64.Hash(data2);

    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Static Hash Methods - Span

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_EmptyData_ReturnsConsistentHash() {
    ReadOnlySpan<byte> data = Array.Empty<byte>();
    var hash1 = XxHash64.Hash(data);
    var hash2 = XxHash64.Hash(data);

    Assert.That(hash1, Is.EqualTo(hash2));
    Assert.That(hash1, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_SimpleData_ReturnsExpectedLength() {
    ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("Hello, World!");
    var hash = XxHash64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_SameDataSameSeed_ReturnsSameHash() {
    ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = XxHash64.Hash(data, 42);
    var hash2 = XxHash64.Hash(data, 42);

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Hash_Span_DifferentSeeds_ReturnsDifferentHashes() {
    ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes("Test data for hashing");
    var hash1 = XxHash64.Hash(data, 0);
    var hash2 = XxHash64.Hash(data, 1);

    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Instance Methods

  [Test]
  [Category("HappyPath")]
  public void Append_MultipleChunks_ProducesConsistentHash() {
    var data = Encoding.UTF8.GetBytes("Hello, World! This is a longer test string for XXH64.");

    var hasher1 = new XxHash64();
    hasher1.Append(data);
    var hash1 = hasher1.GetCurrentHash();

    var hasher2 = new XxHash64();
    var chunk1 = new byte[16];
    var chunk2 = new byte[16];
    var chunk3 = new byte[data.Length - 32];
    Array.Copy(data, 0, chunk1, 0, 16);
    Array.Copy(data, 16, chunk2, 0, 16);
    Array.Copy(data, 32, chunk3, 0, data.Length - 32);
    hasher2.Append(chunk1);
    hasher2.Append(chunk2);
    hasher2.Append(chunk3);
    var hash2 = hasher2.GetCurrentHash();

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void Reset_AfterAppend_ClearsState() {
    var hasher = new XxHash64();
    hasher.Append(Encoding.UTF8.GetBytes("Some data"));
    hasher.Reset();
    var hash = hasher.GetCurrentHash();

    var freshHasher = new XxHash64();
    var freshHash = freshHasher.GetCurrentHash();

    Assert.That(hash, Is.EqualTo(freshHash));
  }

  [Test]
  [Category("HappyPath")]
  public void GetCurrentHash_ReturnsCorrectLength() {
    var hasher = new XxHash64();
    hasher.Append(Encoding.UTF8.GetBytes("Test"));
    var hash = hasher.GetCurrentHash();

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithSeed_ProducesDifferentHash() {
    var data = Encoding.UTF8.GetBytes("Test data");

    var hasher1 = new XxHash64(0);
    hasher1.Append(data);
    var hash1 = hasher1.GetCurrentHash();

    var hasher2 = new XxHash64(12345);
    hasher2.Append(data);
    var hash2 = hasher2.GetCurrentHash();

    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Instance Methods - Span

  [Test]
  [Category("HappyPath")]
  public void Append_Span_MultipleChunks_ProducesConsistentHash() {
    var data = Encoding.UTF8.GetBytes("Hello, World! This is a longer test string for XXH64.");

    var hasher1 = new XxHash64();
    hasher1.Append((ReadOnlySpan<byte>)data);
    var hash1 = hasher1.GetCurrentHash();

    var hasher2 = new XxHash64();
    hasher2.Append(new ReadOnlySpan<byte>(data, 0, 16));
    hasher2.Append(new ReadOnlySpan<byte>(data, 16, 16));
    hasher2.Append(new ReadOnlySpan<byte>(data, 32, data.Length - 32));
    var hash2 = hasher2.GetCurrentHash();

    Assert.That(hash1, Is.EqualTo(hash2));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Hash_NullArray_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => XxHash64.Hash(null!));
  }

  [Test]
  [Category("EdgeCase")]
  public void Append_NullArray_ThrowsArgumentNullException() {
    var hasher = new XxHash64();
    Assert.Throws<ArgumentNullException>(() => hasher.Append((byte[])null!));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_SingleByte_ReturnsValidHash() {
    var data = new byte[] { 0x42 };
    var hash = XxHash64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_ExactlyOneBlock_ReturnsValidHash() {
    var data = new byte[32];
    new Random(42).NextBytes(data);
    var hash = XxHash64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Hash_MultipleBlocks_ReturnsValidHash() {
    var data = new byte[128];
    new Random(42).NextBytes(data);
    var hash = XxHash64.Hash(data);

    Assert.That(hash, Has.Length.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void Array_And_Span_ProduceSameHash() {
    var data = Encoding.UTF8.GetBytes("Test data for comparison");
    var arrayHash = XxHash64.Hash(data);
    var spanHash = XxHash64.Hash((ReadOnlySpan<byte>)data);

    Assert.That(arrayHash, Is.EqualTo(spanHash));
  }

  #endregion

}
