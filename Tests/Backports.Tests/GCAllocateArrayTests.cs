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
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("GC")]
public class GCAllocateArrayTests {

  #region Basic Allocation

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_ReturnsArrayOfCorrectLength() {
    var array = GC.AllocateArray<int>(100);
    Assert.That(array.Length, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_ReturnsArrayOfCorrectType() {
    var array = GC.AllocateArray<double>(10);
    Assert.That(array, Is.TypeOf<double[]>());
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_ZeroLength_ReturnsEmptyArray() {
    var array = GC.AllocateArray<int>(0);
    Assert.That(array.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_InitializesToDefault() {
    var array = GC.AllocateArray<int>(5);
    foreach (var item in array)
      Assert.That(item, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_StructType_InitializesToDefault() {
    var array = GC.AllocateArray<TestStruct>(3);
    foreach (var item in array) {
      Assert.That(item.X, Is.EqualTo(0));
      Assert.That(item.Y, Is.EqualTo(0));
    }
  }

  #endregion

  #region Pinned Allocation

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_Pinned_ReturnsArrayOfCorrectLength() {
    var array = GC.AllocateArray<int>(100, pinned: true);
    Assert.That(array.Length, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_Pinned_ArrayCanBeWrittenTo() {
    var array = GC.AllocateArray<int>(5, pinned: true);
    for (var i = 0; i < array.Length; ++i)
      array[i] = i * 2;

    Assert.That(array[0], Is.EqualTo(0));
    Assert.That(array[1], Is.EqualTo(2));
    Assert.That(array[2], Is.EqualTo(4));
    Assert.That(array[3], Is.EqualTo(6));
    Assert.That(array[4], Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void AllocateArray_Pinned_CanGetPointer() {
    var array = GC.AllocateArray<int>(10, pinned: true);
    array[0] = 42;
    array[5] = 99;

    fixed (int* ptr = array) {
      Assert.That(ptr[0], Is.EqualTo(42));
      Assert.That(ptr[5], Is.EqualTo(99));
    }
  }

  [Test]
  [Category("HappyPath")]
  public unsafe void AllocateArray_Pinned_PointerRemainsValid() {
    var array = GC.AllocateArray<int>(100, pinned: true);
    for (var i = 0; i < array.Length; ++i)
      array[i] = i;

    fixed (int* ptr = array) {
      GC.Collect(2, GCCollectionMode.Forced);
      GC.WaitForPendingFinalizers();
      GC.Collect(2, GCCollectionMode.Forced);

      for (var i = 0; i < array.Length; ++i)
        Assert.That(ptr[i], Is.EqualTo(i));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_Pinned_CanBeUsedWithGCHandle() {
    var array = GC.AllocateArray<byte>(256, pinned: true);
    for (var i = 0; i < array.Length; ++i)
      array[i] = (byte)i;

    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
    try {
      var ptr = handle.AddrOfPinnedObject();
      Assert.That(ptr, Is.Not.EqualTo(IntPtr.Zero));

      unsafe {
        var bytePtr = (byte*)ptr;
        Assert.That(bytePtr[0], Is.EqualTo(0));
        Assert.That(bytePtr[128], Is.EqualTo(128));
        Assert.That(bytePtr[255], Is.EqualTo(255));
      }
    } finally {
      handle.Free();
    }
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void AllocateArray_LargeArray_Succeeds() {
    var array = GC.AllocateArray<byte>(1_000_000);
    Assert.That(array.Length, Is.EqualTo(1_000_000));
  }

  [Test]
  [Category("EdgeCase")]
  public void AllocateArray_Pinned_LargeArray_Succeeds() {
    var array = GC.AllocateArray<byte>(1_000_000, pinned: true);
    Assert.That(array.Length, Is.EqualTo(1_000_000));
  }

  [Test]
  [Category("EdgeCase")]
  public void AllocateArray_MultipleAllocations_AllSucceed() {
    var arrays = new int[100][];
    for (var i = 0; i < arrays.Length; ++i)
      arrays[i] = GC.AllocateArray<int>(100, pinned: true);

    for (var i = 0; i < arrays.Length; ++i)
      Assert.That(arrays[i].Length, Is.EqualTo(100));
  }

  #endregion

  #region Different Types

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_ByteType_Works() {
    var array = GC.AllocateArray<byte>(10, pinned: true);
    array[0] = 255;
    Assert.That(array[0], Is.EqualTo(255));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_LongType_Works() {
    var array = GC.AllocateArray<long>(10, pinned: true);
    array[0] = long.MaxValue;
    Assert.That(array[0], Is.EqualTo(long.MaxValue));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_FloatType_Works() {
    var array = GC.AllocateArray<float>(10, pinned: true);
    array[0] = 3.14159f;
    Assert.That(array[0], Is.EqualTo(3.14159f).Within(0.00001f));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_DoubleType_Works() {
    var array = GC.AllocateArray<double>(10, pinned: true);
    array[0] = 3.141592653589793;
    Assert.That(array[0], Is.EqualTo(3.141592653589793).Within(0.0000001));
  }

  [Test]
  [Category("HappyPath")]
  public void AllocateArray_CustomStruct_Works() {
    var array = GC.AllocateArray<TestStruct>(5, pinned: true);
    array[0] = new TestStruct { X = 10, Y = 20 };
    Assert.That(array[0].X, Is.EqualTo(10));
    Assert.That(array[0].Y, Is.EqualTo(20));
  }

  #endregion

  private struct TestStruct {
    public int X;
    public int Y;
  }

}
