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
public class StreamTests {

  #region ReadExactly

  [Test]
  [Category("HappyPath")]
  public void ReadExactly_ReadsAllBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    stream.ReadExactly(buffer, 0, 5);
    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactly_NotEnoughData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    Assert.Throws<EndOfStreamException>(() => stream.ReadExactly(buffer, 0, 5));
  }

  #endregion

  #region ReadAtLeast

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeast_ReadsMinimumBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];
    var read = stream.ReadAtLeast(buffer.AsSpan(), 3);
    Assert.That(read, Is.GreaterThanOrEqualTo(3));
  }

  #endregion

  #region Read(Span<byte>)

  [Test]
  [Category("HappyPath")]
  public void Read_Span_ReadsData() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    Span<byte> buffer = stackalloc byte[5];
    var read = stream.Read(buffer);
    Assert.That(read, Is.EqualTo(5));
    Assert.That(buffer[0], Is.EqualTo(1));
    Assert.That(buffer[4], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Read_Span_PartialRead() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    Span<byte> buffer = stackalloc byte[10];
    var read = stream.Read(buffer);
    Assert.That(read, Is.EqualTo(3));
  }

  #endregion

  #region TextWriter.Write(Span)

  [Test]
  [Category("HappyPath")]
  public void TextWriter_Write_Span_WritesCorrectly() {
    using var writer = new StringWriter();
    writer.Write("Hello".AsSpan());
    Assert.That(writer.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region TextReader.Read(Span)

  [Test]
  [Category("HappyPath")]
  public void TextReader_Read_Span_ReadsCorrectly() {
    using var reader = new StringReader("Hello World");
    Span<char> buffer = stackalloc char[5];
    var read = reader.Read(buffer);
    Assert.That(read, Is.EqualTo(5));
    Assert.That(buffer.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region Directory.CreateTempSubdirectory

  [Test]
  [Category("HappyPath")]
  public void CreateTempSubdirectory_CreatesDirectory() {
    var dir = Directory.CreateTempSubdirectory("test_");
    try {
      Assert.That(dir.Exists, Is.True);
      Assert.That(dir.Name.StartsWith("test_"), Is.True);
    } finally {
      dir.Delete();
    }
  }

  #endregion

}
