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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Stream")]
public class StreamReadExactlyTests {

  #region ReadExactly(Span<byte>)

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_Span_FillsBuffer() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];

    stream.ReadExactly(buffer.AsSpan());

    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_PartialSpan_ReadsRequestedAmount() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[3];

    stream.ReadExactly(buffer.AsSpan());

    Assert.That(buffer, Is.EqualTo(new byte[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadExactly_EmptySpan_ReadsNothing() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = Array.Empty<byte>();

    Assert.DoesNotThrow(() => stream.ReadExactly(buffer.AsSpan()));
    Assert.That(stream.Position, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_Span_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    Assert.Throws<EndOfStreamException>(() => stream.ReadExactly(buffer.AsSpan()));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_MultipleReads_AdvancesPosition() {
    var data = new byte[] { 1, 2, 3, 4, 5, 6 };
    using var stream = new MemoryStream(data);
    var buffer1 = new byte[2];
    var buffer2 = new byte[2];
    var buffer3 = new byte[2];

    stream.ReadExactly(buffer1.AsSpan());
    stream.ReadExactly(buffer2.AsSpan());
    stream.ReadExactly(buffer3.AsSpan());

    Assert.That(buffer1, Is.EqualTo(new byte[] { 1, 2 }));
    Assert.That(buffer2, Is.EqualTo(new byte[] { 3, 4 }));
    Assert.That(buffer3, Is.EqualTo(new byte[] { 5, 6 }));
  }

  #endregion

  #region ReadAtLeast(Span<byte>, int, bool)

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeast_Span_ReturnsMinimumBytesRead() {
    var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeast(buffer.AsSpan(), 5);

    Assert.That(bytesRead, Is.GreaterThanOrEqualTo(5));
    Assert.That(buffer[..bytesRead], Is.EqualTo(data[..bytesRead]));
  }

  [Test]
  [Category("Exception")]
  public void ReadAtLeast_Span_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    Assert.Throws<EndOfStreamException>(() => stream.ReadAtLeast(buffer.AsSpan(), 5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeast_Span_InsufficientData_NoThrow_ReturnsPartial() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeast(buffer.AsSpan(), 5, throwOnEndOfStream: false);

    Assert.That(bytesRead, Is.EqualTo(3));
    Assert.That(buffer[..3], Is.EqualTo(data));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadAtLeast_ZeroMinimum_ReturnsZeroOrMore() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeast(buffer.AsSpan(), 0);

    Assert.That(bytesRead, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void ReadAtLeast_MinimumExceedsBuffer_ThrowsArgumentOutOfRangeException() {
    using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    var buffer = new byte[5];

    Assert.Throws<ArgumentOutOfRangeException>(() => stream.ReadAtLeast(buffer.AsSpan(), 10));
  }

  #endregion

  #region ReadExactlyAsync(Memory<byte>, CancellationToken)

  [Test]
  [Category("HappyPath")]
  public void ReadExactlyAsync_Memory_FillsBuffer() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];

    stream.ReadExactlyAsync(buffer.AsMemory()).GetAwaiter().GetResult();

    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactlyAsync_PartialMemory_ReadsRequestedAmount() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[3];

    stream.ReadExactlyAsync(buffer.AsMemory()).GetAwaiter().GetResult();

    Assert.That(buffer, Is.EqualTo(new byte[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadExactlyAsync_EmptyMemory_ReadsNothing() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = Array.Empty<byte>();

    stream.ReadExactlyAsync(buffer.AsMemory()).GetAwaiter().GetResult();

    Assert.That(stream.Position, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactlyAsync_Memory_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    Assert.ThrowsAsync<EndOfStreamException>(async () => await stream.ReadExactlyAsync(buffer.AsMemory()));
  }

  #endregion

  #region ReadAtLeastAsync(Memory<byte>, int, bool, CancellationToken)

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeastAsync_Memory_ReturnsMinimumBytesRead() {
    var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeastAsync(buffer.AsMemory(), 5).GetAwaiter().GetResult();

    Assert.That(bytesRead, Is.GreaterThanOrEqualTo(5));
  }

  [Test]
  [Category("Exception")]
  public void ReadAtLeastAsync_Memory_InsufficientData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    Assert.ThrowsAsync<EndOfStreamException>(async () => await stream.ReadAtLeastAsync(buffer.AsMemory(), 5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeastAsync_Memory_InsufficientData_NoThrow_ReturnsPartial() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeastAsync(buffer.AsMemory(), 5, throwOnEndOfStream: false).GetAwaiter().GetResult();

    Assert.That(bytesRead, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadAtLeastAsync_ZeroMinimum_ReturnsZeroOrMore() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];

    var bytesRead = stream.ReadAtLeastAsync(buffer.AsMemory(), 0).GetAwaiter().GetResult();

    Assert.That(bytesRead, Is.GreaterThanOrEqualTo(0));
  }

  #endregion

}
