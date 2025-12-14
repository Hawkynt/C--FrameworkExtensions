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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class BitConverterTests {

  #region Core BitConverter (always available)

  [Test]
  public void DoubleToInt64Bits_RoundTrips() {
    const double value = 3.14159265358979;
    var bits = BitConverter.DoubleToInt64Bits(value);
    var result = BitConverter.Int64BitsToDouble(bits);
    Assert.That(result, Is.EqualTo(value));
  }

  [Test]
  public void Int64BitsToDouble_RoundTrips() {
    const long bits = 0x400921FB54442D18; // pi
    var value = BitConverter.Int64BitsToDouble(bits);
    var result = BitConverter.DoubleToInt64Bits(value);
    Assert.That(result, Is.EqualTo(bits));
  }

  [Test]
  public void GetBytes_Int32_ReturnsCorrectBytes() {
    var bytes = BitConverter.GetBytes(0x12345678);
    Assert.That(bytes.Length, Is.EqualTo(4));
    if (BitConverter.IsLittleEndian) {
      Assert.That(bytes[0], Is.EqualTo(0x78));
      Assert.That(bytes[3], Is.EqualTo(0x12));
    } else {
      Assert.That(bytes[0], Is.EqualTo(0x12));
      Assert.That(bytes[3], Is.EqualTo(0x78));
    }
  }

  [Test]
  public void ToInt32_ReturnsCorrectValue() {
    byte[] bytes = BitConverter.IsLittleEndian
      ? new byte[] { 0x78, 0x56, 0x34, 0x12 }
      : new byte[] { 0x12, 0x34, 0x56, 0x78 };
    var result = BitConverter.ToInt32(bytes, 0);
    Assert.That(result, Is.EqualTo(0x12345678));
  }

  #endregion

  #region Float Conversion (Int32BitsToSingle, SingleToInt32Bits)

  [Test]
  public void Int32BitsToSingle_RoundTrips() {
    const float value = 3.14159f;
    var bits = BitConverter.SingleToInt32Bits(value);
    var result = BitConverter.Int32BitsToSingle(bits);
    Assert.That(result, Is.EqualTo(value));
  }

  [Test]
  public void SingleToInt32Bits_RoundTrips() {
    const int bits = 0x40490FDB; // pi as float
    var value = BitConverter.Int32BitsToSingle(bits);
    var result = BitConverter.SingleToInt32Bits(value);
    Assert.That(result, Is.EqualTo(bits));
  }

  [Test]
  public void Int32BitsToSingle_Zero() {
    var result = BitConverter.Int32BitsToSingle(0);
    Assert.That(result, Is.EqualTo(0.0f));
  }

  [Test]
  public void SingleToInt32Bits_Zero() {
    var result = BitConverter.SingleToInt32Bits(0.0f);
    Assert.That(result, Is.EqualTo(0));
  }

  [Test]
  public void Int32BitsToSingle_NegativeZero() {
    var result = BitConverter.Int32BitsToSingle(unchecked((int)0x80000000));
    Assert.That(result, Is.EqualTo(-0.0f));
    Assert.That(BitConverter.SingleToInt32Bits(result), Is.EqualTo(unchecked((int)0x80000000)));
  }

  #endregion

  #region UInt Conversion Polyfills (UInt32BitsToSingle, etc.)

  [Test]
  public void UInt32BitsToSingle_RoundTrips() {
    const float value = 3.14159f;
    var bits = BitConverter.SingleToUInt32Bits(value);
    var result = BitConverter.UInt32BitsToSingle(bits);
    Assert.That(result, Is.EqualTo(value));
  }

  [Test]
  public void SingleToUInt32Bits_RoundTrips() {
    const uint bits = 0x40490FDB; // pi as float
    var value = BitConverter.UInt32BitsToSingle(bits);
    var result = BitConverter.SingleToUInt32Bits(value);
    Assert.That(result, Is.EqualTo(bits));
  }

  [Test]
  public void UInt64BitsToDouble_RoundTrips() {
    const double value = 3.14159265358979;
    var bits = BitConverter.DoubleToUInt64Bits(value);
    var result = BitConverter.UInt64BitsToDouble(bits);
    Assert.That(result, Is.EqualTo(value));
  }

  [Test]
  public void DoubleToUInt64Bits_RoundTrips() {
    const ulong bits = 0x400921FB54442D18; // pi
    var value = BitConverter.UInt64BitsToDouble(bits);
    var result = BitConverter.DoubleToUInt64Bits(value);
    Assert.That(result, Is.EqualTo(bits));
  }

  #endregion

  #region Span-based Polyfills

  [Test]
  public void ToInt32_Span_ReturnsCorrectValue() {
    ReadOnlySpan<byte> bytes = BitConverter.IsLittleEndian
      ? new byte[] { 0x78, 0x56, 0x34, 0x12 }
      : new byte[] { 0x12, 0x34, 0x56, 0x78 };
    var result = BitConverter.ToInt32(bytes);
    Assert.That(result, Is.EqualTo(0x12345678));
  }

  [Test]
  public void ToInt64_Span_ReturnsCorrectValue() {
    ReadOnlySpan<byte> bytes = BitConverter.IsLittleEndian
      ? new byte[] { 0xEF, 0xCD, 0xAB, 0x90, 0x78, 0x56, 0x34, 0x12 }
      : new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
    var result = BitConverter.ToInt64(bytes);
    Assert.That(result, Is.EqualTo(0x1234567890ABCDEF));
  }

  [Test]
  public void ToBoolean_Span_True() {
    ReadOnlySpan<byte> bytes = new byte[] { 1 };
    var result = BitConverter.ToBoolean(bytes);
    Assert.That(result, Is.True);
  }

  [Test]
  public void ToBoolean_Span_False() {
    ReadOnlySpan<byte> bytes = new byte[] { 0 };
    var result = BitConverter.ToBoolean(bytes);
    Assert.That(result, Is.False);
  }

  [Test]
  public void TryWriteBytes_Int32_Success() {
    Span<byte> buffer = stackalloc byte[4];
    var success = BitConverter.TryWriteBytes(buffer, 0x12345678);
    Assert.That(success, Is.True);
    if (BitConverter.IsLittleEndian) {
      Assert.That(buffer[0], Is.EqualTo(0x78));
      Assert.That(buffer[3], Is.EqualTo(0x12));
    } else {
      Assert.That(buffer[0], Is.EqualTo(0x12));
      Assert.That(buffer[3], Is.EqualTo(0x78));
    }
  }

  [Test]
  public void TryWriteBytes_Int32_BufferTooSmall() {
    Span<byte> buffer = stackalloc byte[3];
    var success = BitConverter.TryWriteBytes(buffer, 0x12345678);
    Assert.That(success, Is.False);
  }

  [Test]
  public void TryWriteBytes_Bool_Success() {
    Span<byte> buffer = stackalloc byte[1];
    var success = BitConverter.TryWriteBytes(buffer, true);
    Assert.That(success, Is.True);
    Assert.That(buffer[0], Is.Not.EqualTo(0));
  }

  #endregion

  #region Half Polyfills

  [Test]
  public void Int16BitsToHalf_RoundTrips() {
    var half = (Half)3.14;
    var bits = BitConverter.HalfToInt16Bits(half);
    var result = BitConverter.Int16BitsToHalf(bits);
    Assert.That(result, Is.EqualTo(half));
  }

  [Test]
  public void HalfToInt16Bits_RoundTrips() {
    const short bits = 0x4248; // approximately 3.14 as Half
    var value = BitConverter.Int16BitsToHalf(bits);
    var result = BitConverter.HalfToInt16Bits(value);
    Assert.That(result, Is.EqualTo(bits));
  }

  [Test]
  public void UInt16BitsToHalf_RoundTrips() {
    var half = (Half)3.14;
    var bits = BitConverter.HalfToUInt16Bits(half);
    var result = BitConverter.UInt16BitsToHalf(bits);
    Assert.That(result, Is.EqualTo(half));
  }

  [Test]
  public void GetBytes_Half_ReturnsCorrectLength() {
    var bytes = BitConverter.GetBytes((Half)3.14);
    Assert.That(bytes.Length, Is.EqualTo(2));
  }

  [Test]
  public void ToHalf_ByteArray_RoundTrips() {
    var original = (Half)3.14;
    var bytes = BitConverter.GetBytes(original);
    var result = BitConverter.ToHalf(bytes, 0);
    Assert.That(result, Is.EqualTo(original));
  }

#if SUPPORTS_SPAN

  [Test]
  public void ToHalf_Span_RoundTrips() {
    var original = (Half)3.14;
    var bytes = BitConverter.GetBytes(original);
    var result = BitConverter.ToHalf((ReadOnlySpan<byte>)bytes);
    Assert.That(result, Is.EqualTo(original));
  }

  [Test]
  public void TryWriteBytes_Half_Success() {
    Span<byte> buffer = stackalloc byte[2];
    var success = BitConverter.TryWriteBytes(buffer, (Half)3.14);
    Assert.That(success, Is.True);
  }

  [Test]
  public void TryWriteBytes_Half_BufferTooSmall() {
    Span<byte> buffer = stackalloc byte[1];
    var success = BitConverter.TryWriteBytes(buffer, (Half)3.14);
    Assert.That(success, Is.False);
  }

#endif

  #endregion

}
