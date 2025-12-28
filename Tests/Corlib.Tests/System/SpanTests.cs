using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace System;

[TestFixture]
internal class SpanTests {
  private static IEnumerable<int> LengthGenerator(bool allowZero) {
    var min = allowZero ? 0 : 1;
    var max = 256;
    int[] others = [512, 1024, 2048, 4096, 8192, 16384, 32768, 65536];

    for (var i = min; i <= max; ++i)
      yield return i;

    foreach (var i in others) {
      yield return i - 1;
      yield return i;
      yield return i + 1;
    }
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void CopyTo_CopiesCorrectly_ManagedInt(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => i + 1).ToArray();
    var destinationArray = new int[length];

    var sourceSpan = sourceArray.AsSpan();
    var destinationSpan = destinationArray.AsSpan();

    sourceSpan.CopyTo(destinationSpan);

    for (var i = 0; i < length; ++i)
      Assert.AreEqual(sourceArray[i], destinationArray[i]);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public unsafe void CopyTo_CopiesCorrectly_UnmanagedByte(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => (byte)(i % 255 + 1)).ToArray();
    var destinationArray = new byte[length];


    fixed (byte* sourcePointer = &sourceArray[0])
    fixed (byte* destinationPointer = &destinationArray[0]) {
      var sourceSpan = new Span<byte>(sourcePointer, length);
      var destinationSpan = new Span<byte>(destinationPointer, length);
      sourceSpan.CopyTo(destinationSpan);
    }

    for (var i = 0; i < length; ++i)
      Assert.AreEqual(sourceArray[i], destinationArray[i]);
  }

  [Test]
  public unsafe void CopyTo_ShouldThrow_OnZeroLength() {
    int[] sourceArray = [1, 2, 3, 4, 5];
    var destinationArray = new int[5];

    fixed (int* sourcePtr = sourceArray)
    fixed (int* destinationPtr = destinationArray) {
      var sourceSpan = new Span<int>(sourcePtr, 5);
      var destinationSpan = new Span<int>(destinationPtr, 5);

      try {
        sourceSpan.CopyTo(destinationSpan[..0]);
        Assert.Fail("Expected exception");
      } catch (ArgumentException) {
        Assert.Pass();
      } catch (Exception e) {
        Assert.Fail($"Got exception: {e}");
      }
    }
  }

  [Test]
  public unsafe void CopyTo_ThrowsArgumentException_OnLengthMismatch() {
    int[] sourceArray = [
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8,
      9,
      10
    ];
    var destinationArray = new int[5];

    fixed (int* sourcePtr = sourceArray)
    fixed (int* destinationPtr = destinationArray) {
      var sourceSpan = new Span<int>(sourcePtr, 10);
      var destinationSpan = new Span<int>(destinationPtr, 5);

      try {
        sourceSpan.CopyTo(destinationSpan);
        Assert.Fail($"Should throw exception");
      } catch (ArgumentException) {
        Assert.Pass();
      } catch (Exception e) {
        Assert.Fail($"Got exception: {e}");
      }
    }
  }

  [Test]
  public unsafe void CopyTo_HandlesDifferentTypes() {
    double[] sourceArray = [1.1, 2.2, 3.3, 4.4, 5.5];
    var destinationArray = new double[5];

    fixed (double* sourcePtr = sourceArray)
    fixed (double* destinationPtr = destinationArray) {
      var sourceSpan = new Span<double>(sourcePtr, 5);
      var destinationSpan = new Span<double>(destinationPtr, 5);

      sourceSpan.CopyTo(destinationSpan);

      for (var i = 0; i < 5; i++)
        Assert.AreEqual(sourceArray[i], destinationArray[i]);
    }
  }

  [Test]
  public unsafe void CopyTo_PartialCopy() {
    int[] sourceArray = [
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8,
      9,
      10
    ];
    var destinationArray = new int[10];

    fixed (int* sourcePtr = sourceArray)
    fixed (int* destinationPtr = destinationArray) {
      var sourceSpan = new Span<int>(sourcePtr, 10);
      var destinationSpan = new Span<int>(destinationPtr, 10);

      // Copy first 5 elements
      sourceSpan[..5].CopyTo(destinationSpan[..5]);

      for (var i = 0; i < 5; ++i)
        Assert.AreEqual(sourceArray[i], destinationArray[i]);

      // Remaining elements should be default value
      for (var i = 5; i < 10; ++i)
        Assert.AreEqual(0, destinationArray[i]);
    }
  }

  // User-defined struct
  [StructLayout(LayoutKind.Sequential)]
  private struct MyStruct {
    public int X;
    public float Y;
  }

  // User-defined reference type
  private sealed class MyClass(int x, float y) : IEquatable<MyClass> {
    public readonly int X = x;
    public readonly float Y = y;

    #region Equality members

    /// <inheritdoc />
    public bool Equals(MyClass? other) => !ReferenceEquals(other, null) && this.X == other.X && this.Y.Equals(other.Y);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MyClass other && this.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() {
      unchecked {
        return (this.X * 397) ^ this.Y.GetHashCode();
      }
    }

    public static bool operator ==(MyClass left, MyClass right) => left.Equals(right);

    public static bool operator !=(MyClass left, MyClass right) => !left.Equals(right);

    #endregion
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public void CopyTo_CopiesCorrectly_UserDefinedStruct(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => new MyStruct { X = i, Y = i + 0.5f }).ToArray();
    var destinationArray = new MyStruct[length];

    var sourceSpan = sourceArray.AsSpan();
    var destinationSpan = destinationArray.AsSpan();

    sourceSpan.CopyTo(destinationSpan);

    for (var i = 0; i < length; ++i) {
      Assert.AreEqual(sourceArray[i].X, destinationArray[i].X);
      Assert.AreEqual(sourceArray[i].Y, destinationArray[i].Y);
    }
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public void CopyTo_CopiesCorrectly_Strings(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => new string('a', i % 128)).ToArray();
    var destinationArray = new string[length];

    var sourceSpan = sourceArray.AsSpan();
    var destinationSpan = destinationArray.AsSpan();

    sourceSpan.CopyTo(destinationSpan);

    for (var i = 0; i < length; ++i)
      Assert.AreEqual(sourceArray[i], destinationArray[i]);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public void CopyTo_CopiesCorrectly_ReferenceTypes(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => new MyClass(i, i * 1.5f)).ToArray();
    var destinationArray = new MyClass[length];

    var sourceSpan = sourceArray.AsSpan();
    var destinationSpan = destinationArray.AsSpan();

    sourceSpan.CopyTo(destinationSpan);

    for (var i = 0; i < length; ++i)
      Assert.AreEqual(sourceArray[i], destinationArray[i]);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public unsafe void CopyTo_CopiesCorrectly_BlittableTypes(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => new MyStruct { X = i, Y = i + 0.5f }).ToArray();
    var destinationArray = new MyStruct[length];

    fixed (MyStruct* sourcePtr = sourceArray)
    fixed (MyStruct* destinationPtr = destinationArray) {
      var sourceSpan = new Span<MyStruct>(sourcePtr, length);
      var destinationSpan = new Span<MyStruct>(destinationPtr, length);

      sourceSpan.CopyTo(destinationSpan);
    }

    for (var i = 0; i < length; ++i) {
      Assert.AreEqual(sourceArray[i].X, destinationArray[i].X);
      Assert.AreEqual(sourceArray[i].Y, destinationArray[i].Y);
    }
  }

  [Test]
  [TestCase("")]
  [TestCase("TEST")]
  [TestCase("1234567890123456789012345678901234567890123456789012345678901234567890")]
  public void CopyTo_StringToCharArray(string source) {
    var sourceSpan = source.AsSpan();

    var targetChars = sourceSpan.ToArray();
    var target = new string(targetChars);

    Assert.AreEqual(source, target);
  }

  [Test]
  public void CopyTo_StringPartsToCharArray() {
    var source = "TEST";
    var sourceSpan = source.AsSpan()[1..2];

    var targetChars = sourceSpan.ToArray();
    var target = new string(targetChars);

    Assert.AreEqual(source[1..2], target);
  }

  [Test]
  public void CopyTo_PlainStringsShouldBeTheSameReference() {
    var source = "TEST";
    var sourceSpan = source.AsSpan();
    var target = sourceSpan.ToString();

    Assert.AreEqual(source, target);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void SequenceEqual_ShouldEqual_ManagedByte(int length) {
    ReadOnlySpan<byte> source = Enumerable.Range(0, length).Select(i => (byte)~(i & 0xff)).ToArray().AsSpan();
    ReadOnlySpan<byte> target = Enumerable.Range(0, length).Select(i => (byte)~(i & 0xff)).ToArray().AsSpan();
    Assert.That(source.SequenceEqual(target), Is.True);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public unsafe void SequenceEqual_ShouldEqual_UnmanagedInt(int length) {
    var a = Enumerable.Range(0, length).ToArray();
    var b = Enumerable.Range(0, length).ToArray();
    fixed (int* pa = a)
    fixed (int* pb = b)
      Assert.That(new Span<int>(pa, length).SequenceEqual(new(pb, length)), Is.True);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void SequenceEqual_ShouldEqual_String(int length) {
    var str1 = new string(Enumerable.Range(0, length).Select(i => (char)(65 + i % 26)).ToArray());
    var str2 = new string(Enumerable.Range(0, length).Select(i => (char)(65 + i % 26)).ToArray());
    var span1 = str1.AsSpan();
    var span2 = str2.AsSpan();
    Assert.That(span1.SequenceEqual(span2), Is.True);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void SequenceEqual_ShouldEqual_ReferenceTypes(int length) {
    var a = Enumerable.Range(0, length).Select(i => new MyClass(i, i * 1.5f)).ToArray();
    var b = a.ToList().ToArray();
    var span1 = a.AsSpan();
    var span2 = b.AsSpan();
    Assert.That(span1.SequenceEqual(span2), Is.True);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void SequenceEqual_ShouldEqual_UserDefinedStruct(int length) {
    var a = Enumerable.Range(0, length).Select(i => new MyStruct { X = i, Y = i + 0.5f }).ToArray();
    var b = a.ToList().ToArray();
    var span1 = a.AsSpan();
    var span2 = b.AsSpan();
    Assert.That(span1.SequenceEqual(span2), Is.True);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [false])]
  public void SequenceEqual_ShouldNotEqual_ManagedByte(int length) {
    ReadOnlySpan<byte> source = Enumerable.Range(0, length).Select(i => (byte)~(i & 0xff)).ToArray().AsSpan();
    ReadOnlySpan<byte> target = Enumerable.Range(0, length).Select(i => (byte)(i & 0xff)).ToArray().AsSpan();
    Assert.That(source.SequenceEqual(target), Is.False);

    var temp = Enumerable.Range(0, length).Select(i => (byte)~(i & 0xff)).ToArray().AsSpan();
    temp[0] = (byte)~source[0];
    target = temp;
    Assert.That(source.SequenceEqual(target), Is.False);

    temp[0] = source[0];
    temp[^1] = (byte)~source[^1];
    target = temp;
    Assert.That(source.SequenceEqual(target), Is.False);

    temp[^1] = source[^1];
    temp[temp.Length / 2] = (byte)~source[source.Length / 2];
    target = temp;
    Assert.That(source.SequenceEqual(target), Is.False);
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void And_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)(x & y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).And(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().And(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Or_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)(x | y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Or(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Or(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Xor_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)(x ^ y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Xor(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Xor(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Nand_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)~(x & y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Nand(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Nand(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Nor_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)~(x | y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Nor(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Nor(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Equ_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var o = a.Select(i => (byte)~i).ToArray();
    var expected = a.Zip(o, (x, y) => (byte)~(x ^ y)).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Equ(o, t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Equ(o);
    Assert.That(a.SequenceEqual(expected));
  }

  [Test]
  [TestCaseSource(nameof(LengthGenerator), [true])]
  public void Not_ShouldWorkAsExpected(int length) {
    var a = Enumerable.Range(0, length).Select(i => (byte)(i * 7)).ToArray();
    var expected = a.Select(i => (byte)~i).ToArray();

    var t = new byte[length];

    ((ReadOnlySpan<byte>)a.AsSpan()).Not(t);
    Assert.That(t.SequenceEqual(expected));

    a.AsSpan().Not();
    Assert.That(a.SequenceEqual(expected));
  }
}
