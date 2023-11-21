using System.IO;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

internal class StreamTest {

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
  }
}
