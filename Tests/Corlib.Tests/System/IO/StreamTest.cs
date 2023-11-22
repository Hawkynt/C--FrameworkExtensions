using System.IO;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

internal class StreamTest {

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(-1)]
  [TestCase(short.MinValue)]
  [TestCase(short.MaxValue)]
  [TestCase(1 << 8)]
  public void WriteShort(short value) {
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);
    
    using var memoryStream2 = new MemoryStream(result);
    var readBack = memoryStream2.ReadInt16();
    Assert.AreEqual(value, readBack);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(-1)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  [TestCase(1 << 8)]
  [TestCase(1 << 16)]
  [TestCase(1 << 24)]
  public void WriteInt(int value) {
    using var memoryStream=new MemoryStream();
    memoryStream.Write(value);
    var result=memoryStream.ToArray();
    Assert.AreEqual(4, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);
    Assert.AreEqual((byte)(value >> 16), result[2]);
    Assert.AreEqual((byte)(value >> 24), result[3]);

    using var memoryStream2 = new MemoryStream(result);
    var readBack = memoryStream2.ReadInt32();
    Assert.AreEqual(value, readBack);
  }

  [Test]
  [TestCase(0L)]
  [TestCase(1L)]
  [TestCase(-1L)]
  [TestCase(long.MinValue)]
  [TestCase(long.MaxValue)]
  [TestCase(1L << 8)]
  [TestCase(1L << 16)]
  [TestCase(1L << 24)]
  [TestCase(1L << 32)]
  [TestCase(1L << 40)]
  [TestCase(1L << 48)]
  [TestCase(1L << 56)]
  public void WriteLong(long value) {
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(8, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);
    Assert.AreEqual((byte)(value >> 16), result[2]);
    Assert.AreEqual((byte)(value >> 24), result[3]);
    Assert.AreEqual((byte)(value >> 32), result[4]);
    Assert.AreEqual((byte)(value >> 40), result[5]);
    Assert.AreEqual((byte)(value >> 48), result[6]);
    Assert.AreEqual((byte)(value >> 56), result[7]);

    using var memoryStream2 = new MemoryStream(result);
    var readBack = memoryStream2.ReadInt64();
    Assert.AreEqual(value, readBack);
  }
}
