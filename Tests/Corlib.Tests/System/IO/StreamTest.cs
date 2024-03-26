using System;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Corlib.Tests.System.IO;

internal class StreamTest {

  [Test]
  [TestCase(true)]
  [TestCase(false)]
  public void WriteBool(bool value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(1, result.Length);
    if (value)
      Assert.AreNotEqual(0, result[0]);
    else
      Assert.AreEqual(0, result[0]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadBool();
    Assert.AreEqual(value, readBack);
  }

  [Test]
  [TestCase(100)]
  [TestCase(byte.MinValue)]
  [TestCase(byte.MaxValue)]
  public void WriteByte(byte value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(1, result.Length);
    Assert.AreEqual(value, result[0]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadUInt8();
    Assert.AreEqual(value, readBack);
  }

  [Test]
  [TestCase(0)]
  [TestCase(-1)]
  [TestCase(1)]
  [TestCase(sbyte.MinValue)]
  [TestCase(sbyte.MaxValue)]
  public void WriteSByte(sbyte value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(1, result.Length);
    Assert.AreEqual((byte)value, result[0]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadInt8();
    Assert.AreEqual(value, readBack);
  }

  [Test]
  [TestCase('a')]
  [TestCase('Z')]
  [TestCase('0')]
  [TestCase(' ')]
  [TestCase('\0')]
  [TestCase('\uff00')]
  [TestCase(char.MaxValue)]
  [TestCase(char.MinValue)]
  public void WriteChar(char value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual((byte)(value & 0xFF), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadChar();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadChar(true));
  }

  [Test]
  [TestCase(ushort.MinValue)]
  [TestCase(ushort.MaxValue)]
  [TestCase((ushort)0)]
  [TestCase((ushort)1)]
  [TestCase((ushort)(1 << 8))]
  [TestCase((ushort)(1 << 4))]
  [TestCase((ushort)12345)]
  public void WriteUShort(ushort value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadUInt16();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadUInt16(true));
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(-1)]
  [TestCase(short.MinValue)]
  [TestCase(short.MaxValue)]
  [TestCase(1 << 8)]
  public void WriteShort(short value) {

    // test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);

    // test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadInt16();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadInt16(true));
  }

  [Test]
  [TestCase(uint.MinValue)]
  [TestCase(uint.MaxValue)]
  [TestCase(0u)]
  [TestCase(1u)]
  [TestCase(1u << 8)]
  [TestCase(1u << 16)]
  [TestCase(1u << 24)]
  [TestCase(1234567890u)]
  public void WriteUInt(uint value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(4, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);
    Assert.AreEqual((byte)(value >> 16), result[2]);
    Assert.AreEqual((byte)(value >> 24), result[3]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadUInt32();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadUInt32(true));
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

    // test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(4, result.Length);
    Assert.AreEqual((byte)(value >> 0), result[0]);
    Assert.AreEqual((byte)(value >> 8), result[1]);
    Assert.AreEqual((byte)(value >> 16), result[2]);
    Assert.AreEqual((byte)(value >> 24), result[3]);

    // test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadInt32();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadInt32(true));
  }

  [Test]
  [TestCase(0ul)]
  [TestCase(1ul)]
  [TestCase(ulong.MinValue)]
  [TestCase(ulong.MaxValue)]
  [TestCase(1UL << 8)]
  [TestCase(1UL << 16)]
  [TestCase(1UL << 24)]
  [TestCase(1UL << 32)]
  [TestCase(1UL << 40)]
  [TestCase(1UL << 48)]
  [TestCase(1UL << 56)]
  [TestCase(12345678910111213ul)]
  public void WriteULong(ulong value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(8, result.Length);
    for (var i = 0; i < 8; ++i)
      Assert.AreEqual((byte)(value >> (8 * i)), result[i]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadUInt64();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadUInt64(true));
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

    // test writing
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

    // test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadInt64();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadInt64(true));
  }

  [Test]
  [TestCase(0f)]
  [TestCase(1f)]
  [TestCase(-1f)]
  [TestCase(float.MinValue)]
  [TestCase(float.MaxValue)]
  [TestCase(float.Epsilon)]
  [TestCase(float.PositiveInfinity)]
  [TestCase(float.NegativeInfinity)]
  [TestCase(float.NaN)]
  public void WriteFloat(float value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(4, result.Length);

    // Convert float to bytes for comparison
    var valueBytes = BitConverter.GetBytes(value);
    for (var i = 0; i < 4; ++i)
      Assert.AreEqual(valueBytes[i], result[i]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadFloat32();
    if (float.IsNaN(value)) {
      Assert.IsTrue(float.IsNaN(readBack));
    } else {
      Assert.AreEqual(value, readBack);
    }

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    if (float.IsNaN(value)) {
      Assert.IsTrue(float.IsNaN(memoryStream.ReadFloat32(true)));
    } else {
      Assert.AreEqual(value, memoryStream.ReadFloat32(true));
    }
  }

  [Test]
  [TestCase(0d)]
  [TestCase(1d)]
  [TestCase(-1d)]
  [TestCase(double.MinValue)]
  [TestCase(double.MaxValue)]
  [TestCase(double.Epsilon)]
  [TestCase(double.PositiveInfinity)]
  [TestCase(double.NegativeInfinity)]
  [TestCase(double.NaN)]
  public void WriteDouble(double value) {

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(8, result.Length);

    // Convert float to bytes for comparison
    var valueBytes = BitConverter.GetBytes(value);
    for (var i = 0; i < 8; ++i)
      Assert.AreEqual(valueBytes[i], result[i]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadFloat64();
    if (double.IsNaN(value)) {
      Assert.IsTrue(double.IsNaN(readBack));
    } else {
      Assert.AreEqual(value, readBack);
    }

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    if (double.IsNaN(value)) {
      Assert.IsTrue(double.IsNaN(memoryStream.ReadFloat64(true)));
    } else {
      Assert.AreEqual(value, memoryStream.ReadFloat64(true));
    }

  }

  [Test]
  [TestCase(0d)]
  [TestCase(1d)]
  [TestCase(-1d)]
  [TestCase(double.NegativeInfinity)]
  [TestCase(double.PositiveInfinity)]
  [TestCase(12345.6789d)]
  [TestCase(-12345.6789d)]
  public void WriteDecimal(double valueAsDouble) {
    var value = double.IsNegativeInfinity(valueAsDouble) ? decimal.MinValue : double.IsPositiveInfinity(valueAsDouble) ? decimal.MaxValue : new(valueAsDouble);

    // Test writing
    using var memoryStream = new MemoryStream();
    memoryStream.Write(value);
    var result = memoryStream.ToArray();
    Assert.AreEqual(16, result.Length);

    // Convert decimal to bytes for comparison
    var valueBytes = decimal.GetBits(value)
      .SelectMany(BitConverter.GetBytes)
      .ToArray();
    Assert.AreEqual(valueBytes.Length, result.Length);
    for (var i = 0; i < valueBytes.Length; ++i)
      Assert.AreEqual(valueBytes[i], result[i]);

    // Test reading
    memoryStream.Position = 0;
    var readBack = memoryStream.ReadMoney128();
    Assert.AreEqual(value, readBack);

    // test writing big endian
    memoryStream.SetLength(0);
    memoryStream.Write(value, true);
    Assert.AreEqual(result.Length, memoryStream.Length);

    memoryStream.Position = 0;
    for (var i = 0; i < result.Length; ++i)
      Assert.AreEqual(result[result.Length - 1 - i], memoryStream.ReadByte());

    // test readin big endian
    memoryStream.Position = 0;
    Assert.AreEqual(value, memoryStream.ReadMoney128(true));
  }

  private static IEnumerable<int> _CountGenerator() {
    for (var i = 0; i <= 511; ++i)
      yield return i;
  }

  [Test]
  [TestCaseSource(nameof(_CountGenerator))]
  public void ToArrayOnSeekableStream(int size) {
    using var memoryStream = new MemoryStream(size + 1);
    var bytes = new byte[size];
    if (size < byte.MaxValue)
      new Random().NextBytes(bytes);
    else
      for (var i = 0; i < size; ++i)
        bytes[i] = (byte)i;

    memoryStream.Write(bytes);

    memoryStream.Position = 0;
    var result = memoryStream.ToArray();
    Assert.That(result, Is.EquivalentTo(bytes));
  }

  [Test]
  [TestCaseSource(nameof(_CountGenerator))]
  public void ToArrayOnUnseekableStream(int size) {
    var bytes = new byte[size];
    if (size < byte.MaxValue)
      new Random().NextBytes(bytes);
    else
      for (var i = 0; i < size; ++i)
        bytes[i] = (byte)i;

    var aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.Zeros;
    aes.KeySize = 128;
    aes.BlockSize = 128;

    byte[] cipherText;
    using (var memoryStream = new MemoryStream(size + 1)) {
      aes.Key = new byte[16];
      aes.IV = new byte[16];
      var cipher = aes.CreateEncryptor();
      using (var cs = new CryptoStream(memoryStream, cipher, CryptoStreamMode.Write))
        memoryStream.Write(bytes);
      cipherText = memoryStream.ToArray();
    }

    byte[] result;
    using (var memoryStream = new MemoryStream(cipherText)) {
      aes.Key = new byte[16];
      aes.IV = new byte[16];
      var cipher = aes.CreateDecryptor();
      using (var cs = new CryptoStream(memoryStream, cipher, CryptoStreamMode.Read))
        result = memoryStream.ToArray();
    }

    Assert.That(result, Is.EquivalentTo(bytes));
  }

  [Test]
  [TestCaseSource(nameof(_CountGenerator))]
  public void ReadAllBytesOnSeekableStream_Should_Return_Bytes_Left(int size) {
    using var memoryStream = new MemoryStream(size + 1);
    var bytes = new byte[size];
    if (size < byte.MaxValue)
      new Random().NextBytes(bytes);
    else
      for (var i = 0; i < size; ++i)
        bytes[i] = (byte)i;

    var dummy = new byte[size];
    for (var i = 0; i < size; ++i)
      dummy[i] = (byte)(255 - bytes[i]);

    memoryStream.Write(dummy); // we write bytes before so we can discard them later again
    memoryStream.Write(bytes);

    memoryStream.Position = 0;
    memoryStream.ReadBytes(dummy.Length); // discard the dummy

    var result = memoryStream.ReadAllBytes();
    Assert.That(result, Is.EquivalentTo(bytes));
  }

  [Test]
  [TestCaseSource(nameof(_CountGenerator))]
  public void ReadAllBytesOnUnseekableStream_Should_Return_Bytes_Left(int size) {
    var bytes = new byte[size];
    if (size < byte.MaxValue)
      new Random().NextBytes(bytes);
    else
      for (var i = 0; i < size; ++i)
        bytes[i] = (byte)i;

    var dummy = new byte[size];
    for (var i = 0; i < size; ++i)
      dummy[i] = (byte)(255 - bytes[i]);

    var aes = Aes.Create();
    aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.Zeros;
    aes.KeySize = 128;
    aes.BlockSize = 128;

    byte[] cipherText;
    using (var memoryStream = new MemoryStream(size + 1)) {
      aes.Key = new byte[16];
      aes.IV = new byte[16];
      var cipher = aes.CreateEncryptor();
      using (var cs = new CryptoStream(memoryStream, cipher, CryptoStreamMode.Write)) {
        memoryStream.Write(dummy);
        memoryStream.Write(bytes);
      }

      cipherText = memoryStream.ToArray();
    }

    byte[] result;
    using (var memoryStream = new MemoryStream(cipherText)) {
      aes.Key = new byte[16];
      aes.IV = new byte[16];
      var cipher = aes.CreateDecryptor();
      using (var cs = new CryptoStream(memoryStream, cipher, CryptoStreamMode.Read)) {
        memoryStream.ReadBytes(dummy.Length);
        result = memoryStream.ReadAllBytes();
      }
    }

    Assert.That(result, Is.EquivalentTo(bytes));
  }

}
