using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace System;

[TestFixture]
internal class SpanExtensionsExtraTests {

  #region Clear and Fill Tests

  [Test]
  public void Clear_SetsAllBytesToZero() {
    var data = new byte[] { 1, 2, 3, 4, 5, 255, 128, 64 };
    data.AsSpan().Clear();
    Assert.IsTrue(data.All(b => b == 0));
  }

  [Test]
  public void Clear_WorksWithEmptySpan() {
    var empty = Span<byte>.Empty;
    empty.Clear();
  }

  [Test]
  [TestCase(1)]
  [TestCase(7)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(100)]
  [TestCase(256)]
  public void Clear_WorksWithVariousSizes(int size) {
    var data = Enumerable.Range(0, size).Select(i => (byte)(i & 0xFF)).ToArray();
    data.AsSpan().Clear();
    Assert.IsTrue(data.All(b => b == 0));
  }

  [Test]
  public void Fill_SetsAllBytesToValue() {
    var data = new byte[100];
    data.AsSpan().Fill(0xAB);
    Assert.IsTrue(data.All(b => b == 0xAB));
  }

  [Test]
  [TestCase(1)]
  [TestCase(7)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(100)]
  [TestCase(256)]
  public void Fill_WorksWithVariousSizes(int size) {
    const byte fillValue = 0x55;
    var data = new byte[size];
    data.AsSpan().Fill(fillValue);
    Assert.IsTrue(data.All(b => b == fillValue));
  }

  [Test]
  public void Fill_WorksWithEmptySpan() {
    var empty = Span<byte>.Empty;
    empty.Fill(0xFF);
  }

  [Test]
  public void Clear_TypedSpan_Int_Works() {
    var data = new int[] { 1, 2, 3, 4, 5 };
    data.AsSpan().Clear();
    Assert.IsTrue(data.All(v => v == 0));
  }

  [Test]
  public void Fill_TypedSpan_Int_Works() {
    var data = new int[10];
    data.AsSpan().Fill(42);
    Assert.IsTrue(data.All(v => v == 42));
  }

  [Test]
  public void Fill_TypedSpan_Int_SameBytePattern() {
    var data = new int[10];
    data.AsSpan().Fill(unchecked((int)0xAAAAAAAA));
    Assert.IsTrue(data.All(v => v == unchecked((int)0xAAAAAAAA)));
  }

  [Test]
  public void Fill_TypedSpan_ULong_Works() {
    var data = new ulong[10];
    data.AsSpan().Fill(0x123456789ABCDEF0);
    Assert.IsTrue(data.All(v => v == 0x123456789ABCDEF0));
  }

  #endregion

  #region Span-to-Span And Tests

  [Test]
  public void And_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = data.Zip(operand, (a, b) => (byte)(a & b)).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void And_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = source.Zip(operand, (a, b) => (byte)(a & b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).And(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
    Assert.IsTrue(source.SequenceEqual(new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 }));
  }

  [Test]
  [TestCase(1)]
  [TestCase(7)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(100)]
  public void And_WorksWithVariousSizes(int size) {
    var data = Enumerable.Range(0, size).Select(i => (byte)0xFF).ToArray();
    var operand = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
    var expected = data.Zip(operand, (a, b) => (byte)(a & b)).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void And_HandlesEmptySpans() {
    var empty = Span<byte>.Empty;
    var emptyOperand = ReadOnlySpan<byte>.Empty;
    empty.And(emptyOperand);
  }

  #endregion

  #region Span-to-Span Or Tests

  [Test]
  public void Or_InPlace_Works() {
    var data = new byte[] { 0x00, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = data.Zip(operand, (a, b) => (byte)(a | b)).ToArray();

    data.AsSpan().Or(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Or_SourceTarget_Works() {
    var source = new byte[] { 0x00, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = source.Zip(operand, (a, b) => (byte)(a | b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Or(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  [Test]
  [TestCase(1)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(100)]
  public void Or_WorksWithVariousSizes(int size) {
    var data = Enumerable.Range(0, size).Select(i => (byte)0x00).ToArray();
    var operand = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
    var expected = data.Zip(operand, (a, b) => (byte)(a | b)).ToArray();

    data.AsSpan().Or(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  #endregion

  #region Span-to-Span Xor Tests

  [Test]
  public void Xor_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0xFF, 0xFF };
    var expected = data.Zip(operand, (a, b) => (byte)(a ^ b)).ToArray();

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Xor_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0xFF, 0xFF };
    var expected = source.Zip(operand, (a, b) => (byte)(a ^ b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Xor(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  [Test]
  public void Xor_SelfXor_IsZero() {
    var data = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
    var operand = (byte[])data.Clone();

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.All(b => b == 0));
  }

  #endregion

  #region Span-to-Span Nand Tests

  [Test]
  public void Nand_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = data.Zip(operand, (a, b) => (byte)~(a & b)).ToArray();

    data.AsSpan().Nand(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Nand_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = source.Zip(operand, (a, b) => (byte)~(a & b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Nand(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Span-to-Span Nor Tests

  [Test]
  public void Nor_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = data.Zip(operand, (a, b) => (byte)~(a | b)).ToArray();

    data.AsSpan().Nor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Nor_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x0F, 0x0F, 0x0F, 0x55, 0xAA };
    var expected = source.Zip(operand, (a, b) => (byte)~(a | b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Nor(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Span-to-Span Equ Tests

  [Test]
  public void Equ_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var expected = data.Zip(operand, (a, b) => (byte)~(a ^ b)).ToArray();

    data.AsSpan().Equ(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
    Assert.IsTrue(data.All(b => b == 0xFF));
  }

  [Test]
  public void Equ_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var operand = new byte[] { 0x00, 0x0F, 0xF0, 0x55, 0xAA };
    var expected = source.Zip(operand, (a, b) => (byte)~(a ^ b)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Equ(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Scalar And Tests

  [Test]
  public void And_Scalar_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = data.Select(b => (byte)(b & operand)).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void And_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = source.Select(b => (byte)(b & operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).And(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  [Test]
  [TestCase(1)]
  [TestCase(7)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(100)]
  public void And_Scalar_WorksWithVariousSizes(int size) {
    const byte operand = 0x0F;
    var data = Enumerable.Range(0, size).Select(i => (byte)0xFF).ToArray();
    var expected = data.Select(b => (byte)(b & operand)).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  #endregion

  #region Scalar Or Tests

  [Test]
  public void Or_Scalar_InPlace_Works() {
    var data = new byte[] { 0x00, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = data.Select(b => (byte)(b | operand)).ToArray();

    data.AsSpan().Or(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Or_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0x00, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = source.Select(b => (byte)(b | operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Or(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Scalar Xor Tests

  [Test]
  public void Xor_Scalar_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0xFF;
    var expected = data.Select(b => (byte)(b ^ operand)).ToArray();

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Xor_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0xFF;
    var expected = source.Select(b => (byte)(b ^ operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Xor(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  [Test]
  public void Xor_Scalar_DoubleXor_IsOriginal() {
    var original = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
    var data = (byte[])original.Clone();
    const byte operand = 0xAB;

    data.AsSpan().Xor(operand);
    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(original));
  }

  #endregion

  #region Scalar Nand Tests

  [Test]
  public void Nand_Scalar_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = data.Select(b => (byte)~(b & operand)).ToArray();

    data.AsSpan().Nand(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Nand_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = source.Select(b => (byte)~(b & operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Nand(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Scalar Nor Tests

  [Test]
  public void Nor_Scalar_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = data.Select(b => (byte)~(b | operand)).ToArray();

    data.AsSpan().Nor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Nor_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0x0F;
    var expected = source.Select(b => (byte)~(b | operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Nor(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Scalar Equ Tests

  [Test]
  public void Equ_Scalar_InPlace_Works() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0xFF;
    var expected = data.Select(b => (byte)~(b ^ operand)).ToArray();

    data.AsSpan().Equ(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Equ_Scalar_SourceTarget_Works() {
    var source = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    const byte operand = 0xFF;
    var expected = source.Select(b => (byte)~(b ^ operand)).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Equ(operand, target);
    Assert.IsTrue(target.SequenceEqual(expected));
  }

  #endregion

  #region Typed Span Operations Tests

  [Test]
  public void And_TypedSpan_Int_Works() {
    var data = new int[] { unchecked((int)0xFFFFFFFF), 0x12345678, unchecked((int)0xABCDEF01) };
    var operand = new int[] { 0x0F0F0F0F, unchecked((int)0xF0F0F0F0), 0x55555555 };
    var expected = data.Zip(operand, (a, b) => a & b).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Or_TypedSpan_ULong_Works() {
    var data = new ulong[] { 0x0000000000000000, 0x1234567890ABCDEF };
    var operand = new ulong[] { 0xFFFFFFFFFFFFFFFF, 0x0000000000000000 };
    var expected = data.Zip(operand, (a, b) => a | b).ToArray();

    data.AsSpan().Or(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Xor_TypedSpan_Short_Works() {
    var data = new short[] { 0x1234, 0x5678, unchecked((short)0xABCD) };
    var operand = new short[] { unchecked((short)0xFFFF), unchecked((short)0xFFFF), unchecked((short)0xFFFF) };
    var expected = data.Zip(operand, (a, b) => (short)(a ^ b)).ToArray();

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  #endregion

  #region Typed Scalar Operations Tests

  [Test]
  public void And_TypedScalar_Int_Works() {
    var data = new int[] { unchecked((int)0xFFFFFFFF), 0x12345678, unchecked((int)0xABCDEF01) };
    const int operand = 0x0F0F0F0F;
    var expected = data.Select(v => v & operand).ToArray();

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Or_TypedScalar_UShort_Works() {
    var data = new ushort[] { 0x0000, 0x1234, 0xABCD };
    const ushort operand = 0x00FF;
    var expected = data.Select(v => (ushort)(v | operand)).ToArray();

    data.AsSpan().Or(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Xor_TypedScalar_Long_Works() {
    var data = new long[] { 0x0123456789ABCDEF, -1L, 0L };
    const long operand = unchecked((long)0xFFFFFFFFFFFFFFFF);
    var expected = data.Select(v => v ^ operand).ToArray();

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  #endregion

  #region Edge Cases and Large Data Tests

  [Test]
  public void And_LargeData_Works() {
    const int size = 1024 * 1024;
    var data = new byte[size];
    var operand = new byte[size];
    for (var i = 0; i < size; ++i) {
      data[i] = (byte)(i & 0xFF);
      operand[i] = (byte)(~i & 0xFF);
    }
    var expected = new byte[size];
    for (var i = 0; i < size; ++i)
      expected[i] = (byte)(data[i] & operand[i]);

    data.AsSpan().And(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Xor_LargeData_Works() {
    const int size = 1024 * 1024;
    var data = new byte[size];
    var operand = new byte[size];
    for (var i = 0; i < size; ++i) {
      data[i] = (byte)(i & 0xFF);
      operand[i] = (byte)(~i & 0xFF);
    }
    var expected = new byte[size];
    for (var i = 0; i < size; ++i)
      expected[i] = (byte)(data[i] ^ operand[i]);

    data.AsSpan().Xor(operand);
    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Fill_LargeData_Works() {
    const int size = 1024 * 1024;
    const byte fillValue = 0xAB;
    var data = new byte[size];
    data.AsSpan().Fill(fillValue);
    Assert.IsTrue(data.All(b => b == fillValue));
  }

  [Test]
  public void Scalar_Operations_WithZeroValue() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var copy = (byte[])data.Clone();

    copy.AsSpan().And(0x00);
    Assert.IsTrue(copy.All(b => b == 0x00));

    copy = (byte[])data.Clone();
    copy.AsSpan().Or(0x00);
    Assert.IsTrue(copy.SequenceEqual(data));

    copy = (byte[])data.Clone();
    copy.AsSpan().Xor(0x00);
    Assert.IsTrue(copy.SequenceEqual(data));
  }

  [Test]
  public void Scalar_Operations_WithMaxValue() {
    var data = new byte[] { 0xFF, 0xF0, 0x0F, 0xAA, 0x55 };
    var copy = (byte[])data.Clone();

    copy.AsSpan().And(0xFF);
    Assert.IsTrue(copy.SequenceEqual(data));

    copy = (byte[])data.Clone();
    copy.AsSpan().Or(0xFF);
    Assert.IsTrue(copy.All(b => b == 0xFF));

    copy = (byte[])data.Clone();
    var expected = data.Select(b => (byte)(b ^ 0xFF)).ToArray();
    copy.AsSpan().Xor(0xFF);
    Assert.IsTrue(copy.SequenceEqual(expected));
  }

  #endregion
  [Test]
  public void IsNotEmpty_ReturnsCorrectValue() {
    var empty = Span<int>.Empty;
    Assert.IsFalse(empty.IsNotEmpty());

    Span<int> nonEmpty = stackalloc int[3];
    Assert.IsTrue(nonEmpty.IsNotEmpty());
  }

  [Test]
  public void Not_InPlace_ComplementsBytes() {
    var data = Enumerable.Range(0, 15).Select(i => (byte)i).ToArray();
    var expected = data.Select(b => (byte)~b).ToArray();

    data.AsSpan().Not();

    Assert.IsTrue(data.SequenceEqual(expected));
  }

  [Test]
  public void Not_SourceTarget_ComplementsIntoTarget() {
    var source = Enumerable.Range(0, 15).Select(i => (byte)i).ToArray();
    var target = new byte[source.Length];

    ((ReadOnlySpan<byte>)source).Not(target);

    for (var i = 0; i < source.Length; ++i) {
      Assert.AreEqual((byte)~source[i], target[i]);
      Assert.AreEqual((byte)i, source[i]);
    }
  }

  [Test]
  public void Not_HandlesEmptySpans() {
    var emptySpan = Span<byte>.Empty;
    emptySpan.Not();

    var readOnly = ReadOnlySpan<byte>.Empty;
    var target = Span<byte>.Empty;
    readOnly.Not(target);
  }

  [Test]
  public void Not_SByte_Works() {
    sbyte[] data = { 1, -2, 3 };
    var expected = data.Select(v => (sbyte)~v).ToArray();

    var copy = (sbyte[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new sbyte[data.Length];
    SpanExtensions.Not((ReadOnlySpan<sbyte>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_UShort_Works() {
    ushort[] data = { 1, 2, 3 };
    var expected = data.Select(v => (ushort)~v).ToArray();

    var copy = (ushort[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new ushort[data.Length];
    SpanExtensions.Not((ReadOnlySpan<ushort>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_Short_Works() {
    short[] data = { 1, -2, 3 };
    var expected = data.Select(v => (short)~v).ToArray();

    var copy = (short[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new short[data.Length];
    SpanExtensions.Not((ReadOnlySpan<short>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_UInt_Works() {
    uint[] data = { 1u, 2u, 3u };
    var expected = data.Select(v => ~v).ToArray();

    var copy = (uint[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new uint[data.Length];
    SpanExtensions.Not((ReadOnlySpan<uint>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_Int_Works() {
    int[] data = { 1, -2, 3 };
    var expected = data.Select(v => ~v).ToArray();

    var copy = (int[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new int[data.Length];
    SpanExtensions.Not((ReadOnlySpan<int>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_ULong_Works() {
    ulong[] data = { 1ul, 2ul, 3ul };
    var expected = data.Select(v => ~v).ToArray();

    var copy = (ulong[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new ulong[data.Length];
    SpanExtensions.Not((ReadOnlySpan<ulong>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_Long_Works() {
    long[] data = { 1L, -2L, 3L };
    var expected = data.Select(v => ~v).ToArray();

    var copy = (long[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new long[data.Length];
    SpanExtensions.Not((ReadOnlySpan<long>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }

  [Test]
  public void Not_Bool_Works() {
    bool[] data = { true, false, true };
    var bytes = new byte[data.Length];
    MemoryMarshal.AsBytes(data.AsSpan()).CopyTo(bytes);
    var expectedBytes = (byte[])bytes.Clone();
    SpanExtensions.Not(expectedBytes.AsSpan());
    var expected = MemoryMarshal.Cast<byte, bool>(expectedBytes).ToArray();

    var copy = (bool[])data.Clone();
    SpanExtensions.Not(copy.AsSpan());
    Assert.IsTrue(copy.AsSpan().SequenceEqual(expected));

    var target = new bool[data.Length];
    SpanExtensions.Not((ReadOnlySpan<bool>)data, target);
    Assert.IsTrue(target.AsSpan().SequenceEqual(expected));
  }
}
