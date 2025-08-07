using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace System;

[TestFixture]
internal class SpanExtensionsExtraTests {
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
