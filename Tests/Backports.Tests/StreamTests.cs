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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

/// <summary>
/// Extension methods to unify Task and ValueTask handling across all frameworks.
/// On newer frameworks (net5+), async stream methods return ValueTask which has .AsTask().
/// On older frameworks, polyfills return Task which doesn't have .AsTask().
/// These extensions provide .AsTask() for Task types so the same test code works everywhere.
/// </summary>
internal static class TaskExtensions {
  public static Task AsTask(this Task task) => task;
  public static Task<T> AsTask<T>(this Task<T> task) => task;
}

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

  #region ReadAsync(Memory<byte>)

  [Test]
  [Category("HappyPath")]
  public void ReadAsync_Memory_ReadsData() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    var memory = buffer.AsMemory();
    var read = stream.ReadAsync(memory).AsTask().Result;
    Assert.That(read, Is.EqualTo(5));
    Assert.That(buffer[0], Is.EqualTo(1));
    Assert.That(buffer[4], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadAsync_Memory_PartialRead() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];
    var read = stream.ReadAsync(buffer.AsMemory()).AsTask().Result;
    Assert.That(read, Is.EqualTo(3));
  }

  #endregion

  #region WriteAsync(ReadOnlyMemory<byte>)

  [Test]
  [Category("HappyPath")]
  public void WriteAsync_Memory_WritesData() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream();
    stream.WriteAsync(data.AsMemory()).AsTask().Wait();
    Assert.That(stream.ToArray(), Is.EqualTo(data));
  }

  [Test]
  [Category("EdgeCase")]
  public void WriteAsync_Memory_EmptyDoesNotThrow() {
    using var stream = new MemoryStream();
    stream.WriteAsync(ReadOnlyMemory<byte>.Empty).AsTask().Wait();
    Assert.That(stream.Length, Is.EqualTo(0));
  }

  #endregion

  #region ReadExactlyAsync

  [Test]
  [Category("HappyPath")]
  public void ReadExactlyAsync_Memory_ReadsAllBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    stream.ReadExactlyAsync(buffer.AsMemory()).AsTask().Wait();
    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactlyAsync_Memory_NotEnoughData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    var ex = Assert.Throws<AggregateException>(() => stream.ReadExactlyAsync(buffer.AsMemory()).AsTask().Wait());
    Assert.That(ex.InnerException, Is.TypeOf<EndOfStreamException>());
  }

  [Test]
  [Category("HappyPath")]
  public void ReadExactlyAsync_Array_ReadsAllBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    stream.ReadExactlyAsync(buffer, 0, 5).AsTask().Wait();
    Assert.That(buffer, Is.EqualTo(data));
  }

  [Test]
  [Category("Exception")]
  public void ReadExactlyAsync_Array_NotEnoughData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[5];
    var ex = Assert.Throws<AggregateException>(() => stream.ReadExactlyAsync(buffer, 0, 5).AsTask().Wait());
    Assert.That(ex.InnerException, Is.TypeOf<EndOfStreamException>());
  }

  #endregion

  #region ReadAtLeastAsync

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeastAsync_Memory_ReadsMinimumBytes() {
    var data = new byte[] { 1, 2, 3, 4, 5 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];
    var read = stream.ReadAtLeastAsync(buffer.AsMemory(), 3).AsTask().Result;
    Assert.That(read, Is.GreaterThanOrEqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void ReadAtLeastAsync_Memory_NotEnoughData_ThrowsEndOfStreamException() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];
    var ex = Assert.Throws<AggregateException>(() => stream.ReadAtLeastAsync(buffer.AsMemory(), 5).AsTask().Wait());
    Assert.That(ex.InnerException, Is.TypeOf<EndOfStreamException>());
  }

  [Test]
  [Category("HappyPath")]
  public void ReadAtLeastAsync_Memory_ThrowOnEndOfStreamFalse_ReturnsPartial() {
    var data = new byte[] { 1, 2, 3 };
    using var stream = new MemoryStream(data);
    var buffer = new byte[10];
    var read = stream.ReadAtLeastAsync(buffer.AsMemory(), 5, throwOnEndOfStream: false).AsTask().Result;
    Assert.That(read, Is.EqualTo(3));
  }

  #endregion

  #region TextReader ReadLineAsync with CancellationToken

  [Test]
  [Category("HappyPath")]
  public void TextReader_ReadLineAsync_WithCancellationToken_ReadsLine() {
    using var reader = new StringReader("Line1\nLine2\nLine3");
    var line = reader.ReadLineAsync(CancellationToken.None).AsTask().Result;
    Assert.That(line, Is.EqualTo("Line1"));
  }

  [Test]
  [Category("Exception")]
  public void TextReader_ReadLineAsync_CancelledToken_ThrowsTaskCanceledException() {
    using var reader = new StringReader("Line1\nLine2");
    var cts = new CancellationTokenSource();
    cts.Cancel();
    var ex = Assert.Throws<AggregateException>(() => reader.ReadLineAsync(cts.Token).AsTask().Wait());
    Assert.That(ex.InnerException, Is.TypeOf<TaskCanceledException>());
  }

  #endregion

  #region TextReader ReadToEndAsync with CancellationToken

  [Test]
  [Category("HappyPath")]
  public void TextReader_ReadToEndAsync_WithCancellationToken_ReadsAll() {
    using var reader = new StringReader("Hello World");
    var result = reader.ReadToEndAsync(CancellationToken.None).AsTask().Result;
    Assert.That(result, Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("Exception")]
  public void TextReader_ReadToEndAsync_CancelledToken_ThrowsTaskCanceledException() {
    using var reader = new StringReader("Hello World");
    var cts = new CancellationTokenSource();
    cts.Cancel();
    var ex = Assert.Throws<AggregateException>(() => reader.ReadToEndAsync(cts.Token).AsTask().Wait());
    Assert.That(ex.InnerException, Is.TypeOf<TaskCanceledException>());
  }

  #endregion

  #region File Async Operations

  [Test]
  [Category("HappyPath")]
  public void File_ReadAllBytesAsync_ReadsFileContents() {
    var tempFile = Path.GetTempFileName();
    try {
      var data = new byte[] { 1, 2, 3, 4, 5 };
      File.WriteAllBytesAsync(tempFile, data).Wait();
      var result = File.ReadAllBytesAsync(tempFile).Result;
      Assert.That(result, Is.EqualTo(data));
    } finally {
      File.Delete(tempFile);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void File_ReadAllTextAsync_ReadsFileContents() {
    var tempFile = Path.GetTempFileName();
    try {
      var text = "Hello, World!";
      File.WriteAllTextAsync(tempFile, text).Wait();
      var result = File.ReadAllTextAsync(tempFile).Result;
      Assert.That(result, Is.EqualTo(text));
    } finally {
      File.Delete(tempFile);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void File_ReadAllLinesAsync_ReadsLines() {
    var tempFile = Path.GetTempFileName();
    try {
      var lines = new[] { "Line1", "Line2", "Line3" };
      File.WriteAllLinesAsync(tempFile, lines).Wait();
      var result = File.ReadAllLinesAsync(tempFile).Result;
      Assert.That(result, Is.EqualTo(lines));
    } finally {
      File.Delete(tempFile);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void File_AppendAllTextAsync_AppendsText() {
    var tempFile = Path.GetTempFileName();
    try {
      File.WriteAllTextAsync(tempFile, "Hello").Wait();
      File.AppendAllTextAsync(tempFile, ", World!").Wait();
      var result = File.ReadAllTextAsync(tempFile).Result;
      Assert.That(result, Is.EqualTo("Hello, World!"));
    } finally {
      File.Delete(tempFile);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void File_AppendAllLinesAsync_AppendsLines() {
    var tempFile = Path.GetTempFileName();
    try {
      var initialLines = new[] { "Line1" };
      var appendLines = new[] { "Line2", "Line3" };
      File.WriteAllLinesAsync(tempFile, initialLines).Wait();
      File.AppendAllLinesAsync(tempFile, appendLines).Wait();
      var result = File.ReadAllLinesAsync(tempFile).Result;
      Assert.That(result.Length, Is.EqualTo(3));
    } finally {
      File.Delete(tempFile);
    }
  }

  #endregion

}
