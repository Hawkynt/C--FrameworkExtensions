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

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Intrinsics")]
public unsafe class IntrinsicsValidationTests {

  #region Popcnt Tests

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt32_Zero() {
    Assert.That(Popcnt.PopCount(0u), Is.EqualTo(0u));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt32_MaxValue() {
    Assert.That(Popcnt.PopCount(uint.MaxValue), Is.EqualTo(32u));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt32_PowersOfTwo() {
    Assert.That(Popcnt.PopCount(1u), Is.EqualTo(1u));
    Assert.That(Popcnt.PopCount(2u), Is.EqualTo(1u));
    Assert.That(Popcnt.PopCount(0x80000000u), Is.EqualTo(1u));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt32_Patterns() {
    Assert.That(Popcnt.PopCount(0b10101010u), Is.EqualTo(4u));
    Assert.That(Popcnt.PopCount(0b11111111u), Is.EqualTo(8u));
    Assert.That(Popcnt.PopCount(0xAAAAAAAAu), Is.EqualTo(16u));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt64_Zero() {
    Assert.That(Popcnt.X64.PopCount(0UL), Is.EqualTo(0UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt64_MaxValue() {
    Assert.That(Popcnt.X64.PopCount(ulong.MaxValue), Is.EqualTo(64UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Popcnt_PopCount_UInt64_Patterns() {
    Assert.That(Popcnt.X64.PopCount(0xAAAAAAAAAAAAAAAAUL), Is.EqualTo(32UL));
    Assert.That(Popcnt.X64.PopCount(0x8000000000000001UL), Is.EqualTo(2UL));
  }

  #endregion

  #region Lzcnt Tests

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt32_Zero() {
    Assert.That(Lzcnt.LeadingZeroCount(0u), Is.EqualTo(32u));
  }

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt32_MaxValue() {
    Assert.That(Lzcnt.LeadingZeroCount(uint.MaxValue), Is.EqualTo(0u));
  }

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt32_PowersOfTwo() {
    Assert.That(Lzcnt.LeadingZeroCount(1u), Is.EqualTo(31u));
    Assert.That(Lzcnt.LeadingZeroCount(2u), Is.EqualTo(30u));
    Assert.That(Lzcnt.LeadingZeroCount(0x80000000u), Is.EqualTo(0u));
    Assert.That(Lzcnt.LeadingZeroCount(0x40000000u), Is.EqualTo(1u));
  }

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt64_Zero() {
    Assert.That(Lzcnt.X64.LeadingZeroCount(0UL), Is.EqualTo(64UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt64_MaxValue() {
    Assert.That(Lzcnt.X64.LeadingZeroCount(ulong.MaxValue), Is.EqualTo(0UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Lzcnt_LeadingZeroCount_UInt64_PowersOfTwo() {
    Assert.That(Lzcnt.X64.LeadingZeroCount(1UL), Is.EqualTo(63UL));
    Assert.That(Lzcnt.X64.LeadingZeroCount(0x8000000000000000UL), Is.EqualTo(0UL));
    Assert.That(Lzcnt.X64.LeadingZeroCount(0x100000000UL), Is.EqualTo(31UL));
  }

  #endregion

  #region Bmi1 Tests

  [Test]
  [Category("HappyPath")]
  public void Bmi1_AndNot_UInt32() {
    Assert.That(Bmi1.AndNot(0xFFFF0000u, 0xFFFFFFFFu), Is.EqualTo(0x0000FFFFu));
    Assert.That(Bmi1.AndNot(0x00000000u, 0xFFFFFFFFu), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(Bmi1.AndNot(0xFFFFFFFFu, 0xFFFFFFFFu), Is.EqualTo(0x00000000u));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_AndNot_UInt64() {
    Assert.That(Bmi1.X64.AndNot(0xFFFF0000FFFF0000UL, 0xFFFFFFFFFFFFFFFFUL), Is.EqualTo(0x0000FFFF0000FFFFUL));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_TrailingZeroCount_UInt32() {
    Assert.That(Bmi1.TrailingZeroCount(0u), Is.EqualTo(32u));
    Assert.That(Bmi1.TrailingZeroCount(1u), Is.EqualTo(0u));
    Assert.That(Bmi1.TrailingZeroCount(2u), Is.EqualTo(1u));
    Assert.That(Bmi1.TrailingZeroCount(8u), Is.EqualTo(3u));
    Assert.That(Bmi1.TrailingZeroCount(0x80000000u), Is.EqualTo(31u));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_TrailingZeroCount_UInt64() {
    Assert.That(Bmi1.X64.TrailingZeroCount(0UL), Is.EqualTo(64UL));
    Assert.That(Bmi1.X64.TrailingZeroCount(1UL), Is.EqualTo(0UL));
    Assert.That(Bmi1.X64.TrailingZeroCount(0x8000000000000000UL), Is.EqualTo(63UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_ExtractLowestSetBit_UInt32() {
    Assert.That(Bmi1.ExtractLowestSetBit(0u), Is.EqualTo(0u));
    Assert.That(Bmi1.ExtractLowestSetBit(1u), Is.EqualTo(1u));
    Assert.That(Bmi1.ExtractLowestSetBit(0b1100u), Is.EqualTo(0b0100u));
    Assert.That(Bmi1.ExtractLowestSetBit(0b1010u), Is.EqualTo(0b0010u));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_GetMaskUpToLowestSetBit_UInt32() {
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(0u), Is.EqualTo(0xFFFFFFFFu));
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(1u), Is.EqualTo(1u));
    Assert.That(Bmi1.GetMaskUpToLowestSetBit(0b1000u), Is.EqualTo(0b1111u));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_ResetLowestSetBit_UInt32() {
    Assert.That(Bmi1.ResetLowestSetBit(0u), Is.EqualTo(0u));
    Assert.That(Bmi1.ResetLowestSetBit(1u), Is.EqualTo(0u));
    Assert.That(Bmi1.ResetLowestSetBit(0b1100u), Is.EqualTo(0b1000u));
    Assert.That(Bmi1.ResetLowestSetBit(0b1010u), Is.EqualTo(0b1000u));
  }

  [Test]
  [Category("HappyPath")]
  public void Bmi1_BitFieldExtract_UInt32() {
    Assert.That(Bmi1.BitFieldExtract(0b11111111u, 4, 4), Is.EqualTo(0b1111u));
    Assert.That(Bmi1.BitFieldExtract(0xFFFFFFFFu, 0, 8), Is.EqualTo(0xFFu));
    Assert.That(Bmi1.BitFieldExtract(0x12345678u, 8, 8), Is.EqualTo(0x56u));
  }

  #endregion

  #region SSE2 Basic Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sse2_Add_Int32_KnownValues() {
    var left = new[] { 1, 2, 3, 4 };
    var right = new[] { 10, 20, 30, 40 };
    var expected = new[] { 11, 22, 33, 44 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Add(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Subtract_Int32_KnownValues() {
    var left = new[] { 50, 40, 30, 20 };
    var right = new[] { 10, 20, 30, 40 };
    var expected = new[] { 40, 20, 0, -20 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Subtract(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_And_Int32_KnownValues() {
    var left = new[] { unchecked((int)0xFF00FF00), unchecked((int)0xF0F0F0F0), -1, 0 };
    var right = new[] { unchecked((int)0x0FF00FF0), unchecked((int)0x0F0F0F0F), -1, -1 };
    var expected = new[] { unchecked((int)0x0F000F00), unchecked((int)0x00000000), -1, 0 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.And(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Or_Int32_KnownValues() {
    var left = new[] { unchecked((int)0xFF000000), unchecked((int)0x00FF0000), 0, -1 };
    var right = new[] { unchecked((int)0x00FF0000), unchecked((int)0x0000FF00), -1, 0 };
    var expected = new[] { unchecked((int)0xFFFF0000), unchecked((int)0x00FFFF00), -1, -1 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Or(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Xor_Int32_KnownValues() {
    var left = new[] { unchecked((int)0xFFFFFFFF), 0, unchecked((int)0xAAAAAAAA), 12345 };
    var right = new[] { unchecked((int)0xFFFFFFFF), 0, unchecked((int)0x55555555), 12345 };
    var expected = new[] { 0, 0, unchecked((int)0xFFFFFFFF), 0 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Xor(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region SSE2 Byte Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sse2_Add_Byte_KnownValues() {
    var left = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    var right = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160 };
    var expected = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 121, 132, 143, 154, 165, 176 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Add(lVec, rVec);

    var destination = new byte[Vector128<byte>.Count];
    fixed (byte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_AddSaturate_Byte_NoSaturation() {
    var left = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    var right = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160 };
    var expected = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 121, 132, 143, 154, 165, 176 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.AddSaturate(lVec, rVec);

    var destination = new byte[Vector128<byte>.Count];
    fixed (byte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_AddSaturate_Byte_WithSaturation() {
    var left = new byte[] { 200, 255, 250, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    var right = new byte[] { 100, 1, 10, 200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    var expected = new byte[] { 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.AddSaturate(lVec, rVec);

    var destination = new byte[Vector128<byte>.Count];
    fixed (byte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination[..4], Is.EqualTo(expected[..4]));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Min_Byte_KnownValues() {
    var left = new byte[] { 10, 20, 5, 40, 15, 60, 25, 80, 35, 100, 45, 120, 55, 140, 65, 160 };
    var right = new byte[] { 5, 25, 10, 35, 20, 55, 30, 75, 40, 95, 50, 115, 60, 135, 70, 155 };
    var expected = new byte[] { 5, 20, 5, 35, 15, 55, 25, 75, 35, 95, 45, 115, 55, 135, 65, 155 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Min(lVec, rVec);

    var destination = new byte[Vector128<byte>.Count];
    fixed (byte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Max_Byte_KnownValues() {
    var left = new byte[] { 10, 20, 5, 40, 15, 60, 25, 80, 35, 100, 45, 120, 55, 140, 65, 160 };
    var right = new byte[] { 5, 25, 10, 35, 20, 55, 30, 75, 40, 95, 50, 115, 60, 135, 70, 155 };
    var expected = new byte[] { 10, 25, 10, 40, 20, 60, 30, 80, 40, 100, 50, 120, 60, 140, 70, 160 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Max(lVec, rVec);

    var destination = new byte[Vector128<byte>.Count];
    fixed (byte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region SSE2 Short Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sse2_Add_Int16_KnownValues() {
    var left = new short[] { 100, 200, 300, 400, -100, -200, -300, -400 };
    var right = new short[] { 50, 150, 250, 350, 50, 150, 250, 350 };
    var expected = new short[] { 150, 350, 550, 750, -50, -50, -50, -50 };

    Vector128<short> lVec, rVec;
    fixed (short* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (short* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Add(lVec, rVec);

    var destination = new short[Vector128<short>.Count];
    fixed (short* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_MultiplyLow_Int16_KnownValues() {
    var left = new short[] { 2, 3, 4, 5, -2, -3, -4, -5 };
    var right = new short[] { 10, 20, 30, 40, 10, 20, 30, 40 };
    var expected = new short[] { 20, 60, 120, 200, -20, -60, -120, -200 };

    Vector128<short> lVec, rVec;
    fixed (short* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (short* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.MultiplyLow(lVec, rVec);

    var destination = new short[Vector128<short>.Count];
    fixed (short* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region SSE2 Double Operations Tests

  [Test]
  [Category("HappyPath")]
  public void Sse2_Add_Double_KnownValues() {
    var left = new[] { 1.5, 2.5 };
    var right = new[] { 10.5, 20.5 };
    var expected = new[] { 12.0, 23.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Add(lVec, rVec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Multiply_Double_KnownValues() {
    var left = new[] { 2.0, 3.0 };
    var right = new[] { 5.0, 4.0 };
    var expected = new[] { 10.0, 12.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Multiply(lVec, rVec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Divide_Double_KnownValues() {
    var left = new[] { 10.0, 24.0 };
    var right = new[] { 2.0, 6.0 };
    var expected = new[] { 5.0, 4.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Divide(lVec, rVec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Min_Double_KnownValues() {
    var left = new[] { 5.0, 20.0 };
    var right = new[] { 10.0, 15.0 };
    var expected = new[] { 5.0, 15.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Min(lVec, rVec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Max_Double_KnownValues() {
    var left = new[] { 5.0, 20.0 };
    var right = new[] { 10.0, 15.0 };
    var expected = new[] { 10.0, 20.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse2.Max(lVec, rVec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Sse2_Sqrt_Double_KnownValues() {
    var source = new[] { 4.0, 9.0 };
    var expected = new[] { 2.0, 3.0 };

    Vector128<double> vec;
    fixed (double* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse2.Sqrt(vec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

}
