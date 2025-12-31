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
using System.IO;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("BinaryReader")]
public class BinaryReaderTests {

  #region ReadExactly(int count)

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_Count_ReadsCorrectBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    var result = reader.ReadExactly(5);

    Assert.That(result, Is.EqualTo(data));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_PartialCount_ReadsRequestedBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    var result = reader.ReadExactly(3);

    Assert.That(result, Is.EqualTo(new byte[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadExactly_ZeroCount_ReturnsEmptyArray() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    var result = reader.ReadExactly(0);

    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_NegativeCount_ThrowsArgumentOutOfRangeException() {
    using var stream = new MemoryStream([1, 2, 3]);
    using var reader = new BinaryReader(stream);

    Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadExactly(-1));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    Assert.Throws<EndOfStreamException>(() => reader.ReadExactly(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_MultipleReads_AdvancesPosition() {
    var data = new byte[] { 1, 2, 3, 4, 5, 6 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    var first = reader.ReadExactly(2);
    var second = reader.ReadExactly(2);
    var third = reader.ReadExactly(2);

    Assert.That(first, Is.EqualTo(new byte[] { 1, 2 }));
    Assert.That(second, Is.EqualTo(new byte[] { 3, 4 }));
    Assert.That(third, Is.EqualTo(new byte[] { 5, 6 }));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_AtEndOfStream_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    _ = reader.ReadExactly(3);

    Assert.Throws<EndOfStreamException>(() => reader.ReadExactly(1));
  }

  #endregion

  #region ReadExactly(Span<byte>)

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_Span_FillsBuffer() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    var buffer = new byte[5];

    reader.ReadExactly(buffer.AsSpan());

    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_PartialSpan_FillsRequestedBuffer() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    var buffer = new byte[3];

    reader.ReadExactly(buffer.AsSpan());

    Assert.That(buffer, Is.EqualTo(new byte[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadExactly_EmptySpan_DoesNothing() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    var buffer = Array.Empty<byte>();

    Assert.DoesNotThrow(() => reader.ReadExactly(buffer.AsSpan()));
    Assert.That(stream.Position, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_Span_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    var buffer = new byte[10];

    Assert.Throws<EndOfStreamException>(() => reader.ReadExactly(buffer.AsSpan()));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_LargeBuffer_HandlesChunking() {
    var data = new byte[10000];
    for (var i = 0; i < data.Length; ++i)
      data[i] = (byte)(i % 256);

    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    var buffer = new byte[10000];

    reader.ReadExactly(buffer.AsSpan());

    Assert.That(buffer, Is.EqualTo(data));
  }

  #endregion

}
