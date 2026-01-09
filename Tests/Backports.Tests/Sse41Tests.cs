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
[Category("SSE41")]
public unsafe class Sse41Tests {

  #region Blend Tests

  [Test]
  [Category("HappyPath")]
  public void Blend_Float_ShouldBlendCorrectly() {
    var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var right = new[] { 10.0f, 20.0f, 30.0f, 40.0f };
    var expected = new[] { 1.0f, 20.0f, 3.0f, 40.0f };

    Vector128<float> lVec, rVec;
    fixed (float* lPtr = left)
      lVec = Sse.LoadVector128(lPtr);
    fixed (float* rPtr = right)
      rVec = Sse.LoadVector128(rPtr);

    var result = Sse41.Blend(lVec, rVec, 0b1010);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Blend_Double_ShouldBlendCorrectly() {
    var left = new[] { 1.0, 2.0 };
    var right = new[] { 10.0, 20.0 };
    var expected = new[] { 10.0, 2.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.Blend(lVec, rVec, 0b01);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void BlendVariable_Float_ShouldBlendCorrectly() {
    var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var right = new[] { 10.0f, 20.0f, 30.0f, 40.0f };
    var maskInts = new[] { -1, 0, -1, 0 };
    var expected = new[] { 10.0f, 2.0f, 30.0f, 4.0f };

    Vector128<float> lVec, rVec;
    Vector128<int> maskIntVec;
    fixed (float* lPtr = left)
      lVec = Sse.LoadVector128(lPtr);
    fixed (float* rPtr = right)
      rVec = Sse.LoadVector128(rPtr);
    fixed (int* maskPtr = maskInts)
      maskIntVec = Sse2.LoadVector128(maskPtr);

    var result = Sse41.BlendVariable(lVec, rVec, maskIntVec.AsSingle());

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Rounding Tests

  [Test]
  [Category("HappyPath")]
  public void Ceiling_Float_ShouldRoundUp() {
    var source = new[] { 1.1f, 2.5f, 3.9f, -1.5f };
    var expected = new[] { 2.0f, 3.0f, 4.0f, -1.0f };

    Vector128<float> vec;
    fixed (float* ptr = source)
      vec = Sse.LoadVector128(ptr);

    var result = Sse41.Ceiling(vec);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Floor_Float_ShouldRoundDown() {
    var source = new[] { 1.1f, 2.5f, 3.9f, -1.5f };
    var expected = new[] { 1.0f, 2.0f, 3.0f, -2.0f };

    Vector128<float> vec;
    fixed (float* ptr = source)
      vec = Sse.LoadVector128(ptr);

    var result = Sse41.Floor(vec);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void RoundToZero_Double_ShouldTruncate() {
    var source = new[] { 1.9, -2.9 };
    var expected = new[] { 1.0, -2.0 };

    Vector128<double> vec;
    fixed (double* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse41.RoundToZero(vec);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void RoundToNearestInteger_Float_ShouldRoundToEven() {
    var source = new[] { 1.5f, 2.5f, 3.5f, 4.5f };
    var expected = new[] { 2.0f, 2.0f, 4.0f, 4.0f };

    Vector128<float> vec;
    fixed (float* ptr = source)
      vec = Sse.LoadVector128(ptr);

    var result = Sse41.RoundToNearestInteger(vec);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Conversion Tests

  [Test]
  [Category("HappyPath")]
  public void ConvertToVector128Int16_FromSByte_ShouldSignExtend() {
    var source = new sbyte[] { 1, -2, 3, -4, 5, -6, 7, -8, 9, -10, 11, -12, 13, -14, 15, -16 };
    var expected = new short[] { 1, -2, 3, -4, 5, -6, 7, -8 };

    Vector128<sbyte> vec;
    fixed (sbyte* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse41.ConvertToVector128Int16(vec);

    var destination = new short[Vector128<short>.Count];
    fixed (short* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ConvertToVector128Int32_FromShort_ShouldSignExtend() {
    var source = new short[] { 1, -2, 3, -4, 5, -6, 7, -8 };
    var expected = new[] { 1, -2, 3, -4 };

    Vector128<short> vec;
    fixed (short* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse41.ConvertToVector128Int32(vec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ConvertToVector128Int64_FromInt_ShouldSignExtend() {
    var source = new[] { 100, -200, 300, -400 };
    var expected = new long[] { 100, -200 };

    Vector128<int> vec;
    fixed (int* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse41.ConvertToVector128Int64(vec);

    var destination = new long[Vector128<long>.Count];
    fixed (long* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Dot Product Tests

  [Test]
  [Category("HappyPath")]
  public void DotProduct_Float_ShouldComputeCorrectly() {
    var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var right = new[] { 5.0f, 6.0f, 7.0f, 8.0f };
    var expected = new[] { 70.0f, 70.0f, 0.0f, 0.0f };

    Vector128<float> lVec, rVec;
    fixed (float* lPtr = left)
      lVec = Sse.LoadVector128(lPtr);
    fixed (float* rPtr = right)
      rVec = Sse.LoadVector128(rPtr);

    var result = Sse41.DotProduct(lVec, rVec, 0b11110011);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void DotProduct_Double_ShouldComputeCorrectly() {
    var left = new[] { 3.0, 4.0 };
    var right = new[] { 5.0, 6.0 };
    var expected = new[] { 39.0, 39.0 };

    Vector128<double> lVec, rVec;
    fixed (double* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (double* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.DotProduct(lVec, rVec, 0b00110011);

    var destination = new double[Vector128<double>.Count];
    fixed (double* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Extract/Insert Tests

  [Test]
  [Category("HappyPath")]
  public void Extract_Int_ShouldExtractCorrectElement() {
    var source = new[] { 10, 20, 30, 40 };

    Vector128<int> vec;
    fixed (int* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    Assert.That(Sse41.Extract(vec, 0), Is.EqualTo(10));
    Assert.That(Sse41.Extract(vec, 1), Is.EqualTo(20));
    Assert.That(Sse41.Extract(vec, 2), Is.EqualTo(30));
    Assert.That(Sse41.Extract(vec, 3), Is.EqualTo(40));
  }

  [Test]
  [Category("HappyPath")]
  public void Insert_Float_ShouldInsertCorrectly() {
    var source = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var data = new[] { 99.0f, 0.0f, 0.0f, 0.0f };
    var expected = new[] { 1.0f, 2.0f, 99.0f, 4.0f };

    Vector128<float> vec, dataVec;
    fixed (float* ptr = source)
      vec = Sse.LoadVector128(ptr);
    fixed (float* ptr = data)
      dataVec = Sse.LoadVector128(ptr);

    // Control byte: bits 7:6 = source index (0), bits 5:4 = dest index (2), bits 3:0 = zero mask (0) = 0x20
    var result = Sse41.Insert(vec, dataVec, 0x20);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void Extract_Byte_ShouldWrapIndex() {
    var source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

    Vector128<byte> vec;
    fixed (byte* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    Assert.That(Sse41.Extract(vec, 16), Is.EqualTo(0));
    Assert.That(Sse41.Extract(vec, 17), Is.EqualTo(1));
  }

  #endregion

  #region Extended Min/Max Tests

  [Test]
  [Category("HappyPath")]
  public void Max_SByte_ShouldReturnMaximums() {
    var left = new sbyte[] { -10, 5, -20, 30, -5, 15, -25, 40, 1, 2, 3, 4, 5, 6, 7, 8 };
    var right = new sbyte[] { -5, 10, -15, 25, -10, 20, -30, 35, 2, 1, 4, 3, 6, 5, 8, 7 };
    var expected = new sbyte[] { -5, 10, -15, 30, -5, 20, -25, 40, 2, 2, 4, 4, 6, 6, 8, 8 };

    Vector128<sbyte> lVec, rVec;
    fixed (sbyte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (sbyte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.Max(lVec, rVec);

    var destination = new sbyte[Vector128<sbyte>.Count];
    fixed (sbyte* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void Min_UInt32_ShouldReturnMinimums() {
    var left = new uint[] { 100, 50, 300, 25 };
    var right = new uint[] { 75, 100, 250, 50 };
    var expected = new uint[] { 75, 50, 250, 25 };

    Vector128<uint> lVec, rVec;
    fixed (uint* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (uint* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.Min(lVec, rVec);

    var destination = new uint[Vector128<uint>.Count];
    fixed (uint* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void MinPos_ShouldFindMinimumAndPosition() {
    var source = new ushort[] { 500, 300, 100, 700, 200, 600, 400, 800 };

    Vector128<ushort> vec;
    fixed (ushort* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    var result = Sse41.MinHorizontal(vec);

    Assert.That(Vector128.GetElement(result, 0), Is.EqualTo(100));
    Assert.That(Vector128.GetElement(result, 1), Is.EqualTo(2));
  }

  #endregion

  #region Multiply Tests

  [Test]
  [Category("HappyPath")]
  public void MultiplyLow_Int32_ShouldMultiplyCorrectly() {
    var left = new[] { 2, 3, 4, 5 };
    var right = new[] { 10, 20, 30, 40 };
    var expected = new[] { 20, 60, 120, 200 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.MultiplyLow(lVec, rVec);

    var destination = new int[Vector128<int>.Count];
    fixed (int* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void MultiplyLow_UInt32_ShouldHandleOverflow() {
    var left = new uint[] { 0xFFFFFFFF, 2, 3, 4 };
    var right = new uint[] { 2, 0xFFFFFFFF, 5, 6 };
    var expected = new uint[] { 0xFFFFFFFE, 0xFFFFFFFE, 15, 24 };

    Vector128<uint> lVec, rVec;
    fixed (uint* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (uint* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.MultiplyLow(lVec, rVec);

    var destination = new uint[Vector128<uint>.Count];
    fixed (uint* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Pack Tests

  [Test]
  [Category("HappyPath")]
  public void PackUnsignedSaturate_ShouldPackCorrectly() {
    var left = new[] { 100, 200, 300, 400 };
    var right = new[] { 500, -10, 70000, 50 };
    var expected = new ushort[] { 100, 200, 300, 400, 500, 0, 65535, 50 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.PackUnsignedSaturate(lVec, rVec);

    var destination = new ushort[Vector128<ushort>.Count];
    fixed (ushort* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void PackUnsignedSaturate_ShouldSaturateNegativeToZero() {
    var left = new[] { -100, -200, -300, -400 };
    var right = new[] { -1, -2, -3, -4 };
    var expected = new ushort[] { 0, 0, 0, 0, 0, 0, 0, 0 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    var result = Sse41.PackUnsignedSaturate(lVec, rVec);

    var destination = new ushort[Vector128<ushort>.Count];
    fixed (ushort* destPtr = destination)
      Sse2.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Test Operations Tests

  [Test]
  [Category("HappyPath")]
  public void TestC_WithAllBitsSet_ShouldReturnTrueForAllOnes() {
    var allOnes = new int[] { -1, -1, -1, -1 };

    Vector128<int> vec;
    fixed (int* ptr = allOnes)
      vec = Sse2.LoadVector128(ptr);

    // TestC(value, AllBitsSet) returns true if (~value & AllBitsSet) == 0, i.e., all bits in value are set
    Assert.That(Sse41.TestC(vec, Vector128<int>.AllBitsSet), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TestC_WithAllBitsSet_ShouldReturnFalseForNotAllOnes() {
    var notAllOnes = new int[] { -1, 0, -1, -1 };

    Vector128<int> vec;
    fixed (int* ptr = notAllOnes)
      vec = Sse2.LoadVector128(ptr);

    // TestC(value, AllBitsSet) returns false if some bits are not set
    Assert.That(Sse41.TestC(vec, Vector128<int>.AllBitsSet), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TestZ_ShouldReturnTrueWhenAndIsZero() {
    var left = new byte[] { 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0, 0xF0 };
    var right = new byte[] { 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    // TestZ returns true when (left & right) == 0
    Assert.That(Sse41.TestZ(lVec, rVec), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TestZ_ShouldReturnFalseWhenAndIsNotZero() {
    var left = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    var right = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };

    Vector128<byte> lVec, rVec;
    fixed (byte* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (byte* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    Assert.That(Sse41.TestZ(lVec, rVec), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TestC_ShouldReturnTrueWhenNotLeftAndRightIsZero() {
    var left = new int[] { -1, -1, -1, -1 };
    var right = new int[] { 0, 0, 0, 0 };

    Vector128<int> lVec, rVec;
    fixed (int* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (int* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    Assert.That(Sse41.TestC(lVec, rVec), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TestNotZAndNotC_ShouldReturnTrueForMixedConditions() {
    var left = new short[] { unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00), unchecked((short)0xFF00) };
    var right = new short[] { unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF) };

    Vector128<short> lVec, rVec;
    fixed (short* lPtr = left)
      lVec = Sse2.LoadVector128(lPtr);
    fixed (short* rPtr = right)
      rVec = Sse2.LoadVector128(rPtr);

    Assert.That(Sse41.TestNotZAndNotC(lVec, rVec), Is.True);
  }

  #endregion

  #region Regression Tests

  [Test]
  [Category("Regression")]
  public void Blend_AllZeroMask_ShouldReturnLeft() {
    var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var right = new[] { 10.0f, 20.0f, 30.0f, 40.0f };

    Vector128<float> lVec, rVec;
    fixed (float* lPtr = left)
      lVec = Sse.LoadVector128(lPtr);
    fixed (float* rPtr = right)
      rVec = Sse.LoadVector128(rPtr);

    var result = Sse41.Blend(lVec, rVec, 0);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(left));
  }

  [Test]
  [Category("Regression")]
  public void DotProduct_ZeroControlMask_ShouldReturnZero() {
    var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
    var right = new[] { 5.0f, 6.0f, 7.0f, 8.0f };
    var expected = new[] { 0.0f, 0.0f, 0.0f, 0.0f };

    Vector128<float> lVec, rVec;
    fixed (float* lPtr = left)
      lVec = Sse.LoadVector128(lPtr);
    fixed (float* rPtr = right)
      rVec = Sse.LoadVector128(rPtr);

    var result = Sse41.DotProduct(lVec, rVec, 0);

    var destination = new float[Vector128<float>.Count];
    fixed (float* destPtr = destination)
      Sse.Store(destPtr, result);

    Assert.That(destination, Is.EqualTo(expected));
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  public void MinPos_LargeDataset_ShouldCompleteCorrectly() {
    var source = new ushort[Vector128<ushort>.Count];
    for (var i = 0; i < source.Length; ++i)
      source[i] = (ushort)(1000 - i * 10);

    Vector128<ushort> vec;
    fixed (ushort* ptr = source)
      vec = Sse2.LoadVector128(ptr);

    // Run multiple iterations to verify correctness under repeated execution
    for (var i = 0; i < 1000; ++i) {
      var result = Sse41.MinHorizontal(vec);
      var minValue = Vector128.GetElement(result, 0);
      var minIndex = Vector128.GetElement(result, 1);

      // Verify correct minimum value and position
      Assert.That(minValue, Is.EqualTo(930)); // 1000 - 7*10 = 930 (last element)
      Assert.That(minIndex, Is.EqualTo(7));   // Index of minimum
    }
  }

  #endregion

}
