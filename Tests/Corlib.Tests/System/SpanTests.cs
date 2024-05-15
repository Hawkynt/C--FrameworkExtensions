using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace System;

[TestFixture]
internal class SpanTests {


  private static IEnumerable<int> LengthGenerator(int min, int max) {
    for (var i = min; i <= max; ++i)
      yield return i;
  }

  [Test]
  [TestCaseSource(nameof(SpanTests.LengthGenerator), [0, 256])]
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
  [TestCaseSource(nameof(SpanTests.LengthGenerator), [1, 256])]
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
    int[] sourceArray = { 1, 2, 3, 4, 5 };
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
    int[] sourceArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var destinationArray = new int[5];

    fixed (int* sourcePtr = sourceArray)
    fixed (int* destinationPtr = destinationArray) {
      var sourceSpan = new Span<int>(sourcePtr, 10);
      var destinationSpan = new Span<int>(destinationPtr, 5);

      try {
        sourceSpan.CopyTo(destinationSpan);
        Assert.Fail($"Should throw exception");
      } catch (ArgumentException e) {
        Assert.Pass();
      } catch (Exception e) {
        Assert.Fail($"Got exception: {e}");
      }

    }
  }

  [Test]
  public unsafe void CopyTo_HandlesDifferentTypes() {
    double[] sourceArray = { 1.1, 2.2, 3.3, 4.4, 5.5 };
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
    int[] sourceArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
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

  [Test]
  [TestCaseSource(nameof(SpanTests.LengthGenerator), [1, 256])]
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
  [TestCaseSource(nameof(SpanTests.LengthGenerator), [1, 256])]
  public void CopyTo_CopiesCorrectly_ReferenceTypes(int length) {
    var sourceArray = Enumerable.Range(0, length).Select(i => new string('a', i)).ToArray();
    var destinationArray = new string[length];

    var sourceSpan = sourceArray.AsSpan();
    var destinationSpan = destinationArray.AsSpan();

    sourceSpan.CopyTo(destinationSpan);

    for (var i = 0; i < length; ++i) {
      Assert.AreEqual(sourceArray[i], destinationArray[i]);
    }
  }

  [Test]
  [TestCaseSource(nameof(SpanTests.LengthGenerator), [1, 256])]
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

}

