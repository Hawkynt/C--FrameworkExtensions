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
//

using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.IO;

internal class BufferedStreamExTests {
  [Test]
  public void BufferedStreamEx_Constructor_CreatesValidStream() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream);

    Assert.That(bufferedStream.CanRead, Is.EqualTo(baseStream.CanRead));
    Assert.That(bufferedStream.CanWrite, Is.EqualTo(baseStream.CanWrite));
    Assert.That(bufferedStream.CanSeek, Is.EqualTo(baseStream.CanSeek));
  }

  [Test]
  public void BufferedStreamEx_ReadWriteByte_WorksCorrectly() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream);

    bufferedStream.WriteByte(65); // 'A'
    bufferedStream.WriteByte(66); // 'B'
    bufferedStream.Position = 0;

    Assert.That(bufferedStream.ReadByte(), Is.EqualTo(65));
    Assert.That(bufferedStream.ReadByte(), Is.EqualTo(66));
    Assert.That(bufferedStream.ReadByte(), Is.EqualTo(-1)); // EOF
  }

  [Test]
  public void BufferedStreamEx_ReadWriteArray_HandlesLargeData() {
    var testData = Enumerable.Range(0, 20000).Select(i => (byte)(i % 256)).ToArray();

    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream, 4096);

    bufferedStream.Write(testData, 0, testData.Length);
    bufferedStream.Position = 0;

    var readData = new byte[testData.Length];
    var totalRead = 0;
    var bytesRead = 0;
    while (totalRead < testData.Length && (bytesRead = bufferedStream.Read(readData, totalRead, testData.Length - totalRead)) > 0)
      totalRead += bytesRead;

    Assert.That(totalRead, Is.EqualTo(testData.Length));
    Assert.That(readData, Is.EqualTo(testData));
  }

  [Test]
  public void BufferedStreamEx_Seek_PositionsCorrectly() {
    var testData = "Hello, World!";
    var bytes = Encoding.UTF8.GetBytes(testData);

    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream);

    bufferedStream.Write(bytes, 0, bytes.Length);

    var position = bufferedStream.Seek(-6, SeekOrigin.End);
    Assert.That(position, Is.EqualTo(bytes.Length - 6));

    var readByte = bufferedStream.ReadByte();
    Assert.That(readByte, Is.EqualTo((byte)'W'));
  }

  [Test]
  public void BufferedStreamEx_SetLength_TruncatesCorrectly() {
    var testData = "This is a test string for truncation";
    var bytes = Encoding.UTF8.GetBytes(testData);

    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream);

    bufferedStream.Write(bytes, 0, bytes.Length);
    bufferedStream.SetLength(10);

    Assert.That(bufferedStream.Length, Is.EqualTo(10));

    bufferedStream.Position = 0;
    var readData = new byte[10];
    var bytesRead = bufferedStream.Read(readData, 0, readData.Length);

    Assert.That(bytesRead, Is.EqualTo(10));
    Assert.That(Encoding.UTF8.GetString(readData), Is.EqualTo(testData[..10]));
  }

  [Test]
  public void BufferedStreamEx_Flush_WritesDataToUnderlying() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream, 1024);

    var testData = "Small data that fits in buffer";
    var bytes = Encoding.UTF8.GetBytes(testData);

    bufferedStream.Write(bytes, 0, bytes.Length);

    // Data should still be in buffer
    Assert.That(baseStream.Length, Is.EqualTo(0));

    bufferedStream.Flush();

    // Now data should be in underlying stream
    Assert.That(baseStream.Length, Is.EqualTo(bytes.Length));
  }

  [Test]
  public void BufferedStreamEx_DisposeWithoutDisposingUnderlying_PreservesBaseStream() {
    var baseStream = new MemoryStream();
    var testData = "test data";
    var bytes = Encoding.UTF8.GetBytes(testData);

    using (var bufferedStream = new BufferedStreamEx(baseStream, dontDisposeUnderlyingStream: true))
      bufferedStream.Write(bytes, 0, bytes.Length);

    // Base stream should still be usable
    Assert.That(baseStream.CanRead, Is.True);
    Assert.That(baseStream.Length, Is.EqualTo(bytes.Length));

    baseStream.Position = 0;
    var readData = new byte[bytes.Length];
    baseStream.Read(readData, 0, readData.Length);

    Assert.That(Encoding.UTF8.GetString(readData), Is.EqualTo(testData));

    baseStream.Dispose();
  }

  [Test]
  public void BufferedStreamEx_Length_ReflectsBufferedWrites() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream, 8);

    bufferedStream.WriteByte(1);
    bufferedStream.WriteByte(2);

    // Noch nicht geflusht, aber Length sollte trotzdem korrekt sein
    Assert.That(bufferedStream.Length, Is.EqualTo(2));
  }

  [Test]
  public void BufferedStreamEx_Write_OverwritesUnreadBuffer() {
    var baseData = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();

    using var baseStream = new MemoryStream();
    baseStream.Write(baseData, 0, baseData.Length);

    using var bufferedStream = new BufferedStreamEx(baseStream, 8);
    bufferedStream.Position = 0;

    // Read ersten 4 Bytes, also buffer load
    var read = new byte[4];
    bufferedStream.Read(read, 0, 4);
    Assert.That(read, Is.EqualTo(baseData[..4]));

    // Seek zurück und überschreiben
    bufferedStream.Position = 0;
    bufferedStream.Write(new byte[] { 99, 88, 77, 66 }, 0, 4);

    bufferedStream.Flush();

    baseStream.Position = 0;
    var result = new byte[16];
    baseStream.Read(result, 0, 16);

    // Veränderte Daten prüfen
    Assert.That(result[..4], Is.EqualTo(new byte[] { 99, 88, 77, 66 }));
    Assert.That(result[4..], Is.EqualTo(baseData[4..]));
  }

  [Test]
  public void BufferedStreamEx_SetLength_ShorterThanBufferedData_TruncatesBuffer() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream, 16);

    bufferedStream.Write(Enumerable.Range(0, 10).Select(i => (byte)i).ToArray(), 0, 10);
    bufferedStream.SetLength(5);

    // Length muss 5 sein, auch wenn Buffer ursprünglich 10 hielt
    Assert.That(bufferedStream.Length, Is.EqualTo(5));
  }

  [Test]
  public void BufferedStreamEx_ReadAfterSetLengthBeyondBuffer_DoesNotCrash() {
    using var baseStream = new MemoryStream();
    using var bufferedStream = new BufferedStreamEx(baseStream, 8);

    bufferedStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
    bufferedStream.SetLength(2); // kürzt Buffer intern

    bufferedStream.Position = 0;
    var buf = new byte[4];
    var bytesRead = bufferedStream.Read(buf, 0, 4);

    Assert.That(bytesRead, Is.EqualTo(2));
    Assert.That(buf[0], Is.EqualTo(1));
    Assert.That(buf[1], Is.EqualTo(2));
  }
}
