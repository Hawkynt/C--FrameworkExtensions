using NUnit.Framework;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace System.IO {
  internal class StreamExtensionsAdditionalTests {
    private sealed class NonSeekableStream : MemoryStream {
      public override bool CanSeek => false;
    }

    private struct SampleStruct {
      public int A;
      public short B;
    }

    [Test]
    public void WriteAllTextAndReadAllTextRoundtrip() {
      using var ms = new MemoryStream();
      ms.WriteAllText("Hello World", Encoding.UTF8);
      ms.Position = 0;
      Assert.AreEqual("Hello World", ms.ReadAllText(Encoding.UTF8));
    }

    private sealed class SingleByteEncoding : Encoding {
      public override int GetByteCount(char[] chars, int index, int count) => count;
      public override int GetBytes(char[] chars, int cIndex, int cCount, byte[] bytes, int bIndex) {
        for (int i = 0; i < cCount; i++)
          bytes[bIndex + i] = (byte)chars[cIndex + i];
        return cCount;
      }
      public override int GetCharCount(byte[] bytes, int index, int count) => count;
      public override int GetChars(byte[] bytes, int bIndex, int bCount, char[] chars, int cIndex) {
        for (int i = 0; i < bCount; i++)
          chars[cIndex + i] = (char)bytes[bIndex + i];
        return bCount;
      }
      public override int GetMaxByteCount(int charCount) => charCount;
      public override int GetMaxCharCount(int byteCount) => byteCount;
    }

    [Test]
    public void WriteAndReadFixedLengthString() {
      using var ms = new MemoryStream();
      var encoding = new SingleByteEncoding();
      ms.WriteFixedLengthString("abc", 5, ' ', encoding);
      Assert.AreEqual(5, ms.Length);
      ms.Position = 0;
      Assert.AreEqual("abc", ms.ReadFixedLengthString(5, ' ', encoding));
    }

    [Test]
    public void WriteFixedLengthString_VariableEncoding_Throws() {
      using var ms = new MemoryStream();
      Assert.Throws<ArgumentException>(() => ms.WriteFixedLengthString("abc", 5, ' ', Encoding.UTF8));
    }

    [Test]
    public void IsAtEndOfStreamSeekable() {
      using var ms = new MemoryStream(new byte[] {1,2,3});
      Assert.IsFalse(ms.IsAtEndOfStream());
      ms.Position = ms.Length;
      Assert.IsTrue(ms.IsAtEndOfStream());
    }

    [Test]
    public void IsAtEndOfStreamThrowsOnUnsupportedStream() {
      using var ns = new NonSeekableStream();
      Assert.Throws<InvalidOperationException>(() => ns.IsAtEndOfStream());
    }

    [Test]
    public void IsAtEndOfStreamNetworkStream() {
      var listener = new TcpListener(IPAddress.Loopback, 0);
      listener.Start();
      var port = ((IPEndPoint)listener.LocalEndpoint).Port;
      var client = new TcpClient();
      var acceptTask = listener.AcceptTcpClientAsync();
      client.Connect(IPAddress.Loopback, port);
      using var serverClient = acceptTask.Result;
      using var serverStream = serverClient.GetStream();
      using var clientStream = client.GetStream();

      Assert.IsTrue(serverStream.IsAtEndOfStream());

      clientStream.WriteByte(42);
      clientStream.Flush();
      while (!serverStream.DataAvailable)
        Thread.Sleep(10);

      Assert.IsFalse(serverStream.IsAtEndOfStream());
      Assert.AreEqual(42, serverStream.ReadByte());
      Assert.IsTrue(serverStream.IsAtEndOfStream());
      listener.Stop();
      client.Close();
    }

    [Test]
    public void ReadAndWriteStructRoundtrip() {
      var value = new SampleStruct {A = 123456, B = -123};
      using var ms = new MemoryStream();
      ms.Write(value);
      ms.Position = 0;
      var read = ms.Read<SampleStruct>();
      Assert.AreEqual(value.A, read.A);
      Assert.AreEqual(value.B, read.B);
    }

    [Test]
    public void ReadBytesFromPosition() {
      using var ms = new MemoryStream();
      var data = Enumerable.Range(0, 10).Select(i => (byte)i).ToArray();
      ms.Write(data);
      var buffer = new byte[4];
      ms.ReadBytes(6, buffer);
      Assert.That(buffer, Is.EqualTo(data.Skip(6).Take(4).ToArray()));
    }

    [Test]
    public void WriteAndReadLengthPrefixedString() {
      using var ms = new MemoryStream();
      ms.WriteLengthPrefixedString("hello", Encoding.UTF8);
      ms.Position = 0;
      Assert.AreEqual("hello", ms.ReadLengthPrefixedString(Encoding.UTF8));
    }

    [Test]
    public void WriteAndReadZeroTerminatedString() {
      using var ms = new MemoryStream();
      ms.WriteZeroTerminatedString("hello", Encoding.ASCII);
      ms.Position = 0;
      Assert.AreEqual("hello", ms.ReadZeroTerminatedString(Encoding.ASCII));
    }

    [Test]
    public void WriteZeroTerminatedString_WithNull_Throws() {
      using var ms = new MemoryStream();
      Assert.Throws<ArgumentException>(() => ms.WriteZeroTerminatedString("he\0llo"));
    }

    [Test]
    public void ReadZeroTerminatedString_UnexpectedEnd_Throws() {
      using var ms = new MemoryStream(Encoding.UTF8.GetBytes("no-null"));
      Assert.Throws<EndOfStreamException>(() => ms.ReadZeroTerminatedString(Encoding.UTF8));
    }

#if SUPPORTS_STREAM_ASYNC
    [Test]
    public void ReadBytesAsync_Works() {
      using var ms = new MemoryStream(Enumerable.Range(0, 10).Select(i => (byte)i).ToArray());
      var buffer = new byte[3];
      ms.ReadBytesAsync(4, buffer).GetAwaiter().GetResult();
      Assert.That(buffer, Is.EqualTo(new byte[] {4,5,6}));
    }

    [Test]
    public void BeginReadBytes_Works() {
      using var ms = new MemoryStream(Enumerable.Range(0, 5).Select(i => (byte)i).ToArray());
      var buffer = new byte[2];
      var ar = ms.BeginReadBytes(2, buffer, null, null);
      ms.EndReadBytes(ar);
      Assert.That(buffer, Is.EqualTo(new byte[] {2,3}));
    }
#endif
  }
}
