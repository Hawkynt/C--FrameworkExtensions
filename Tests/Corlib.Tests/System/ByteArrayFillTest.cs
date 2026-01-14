using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace System;

[TestFixture]
public class FillWithByte {
  [Test]
  public void FillArray() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = byteArray1.Select(i => (byte)0).ToArray();
    byteArray1.Fill(0);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithOffset() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0
    };
    byteArray1.Fill(0, 10);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill(0, 0, 10);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill(0, 10, 2);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void CountTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill(0, 10, 25);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    Assert.That(() => byteArray1.Fill(0, 30, 10), Throws.TypeOf<IndexOutOfRangeException>());
  }

  [Test]
  public void OffsetTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    Assert.That(() => byteArray1.Fill(0, -5), Throws.TypeOf<IndexOutOfRangeException>());
  }

  [Test]
  public void CountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill(0, 3, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void FillAtIntPtrCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    }; //{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((byte)0, 10);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillAtIntPtrOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill(0, 10, 2);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void IntPtrCountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill(0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }

  [Test]
  public void IntPtrCountTooSmall2() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill(0, 5, -3);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }
}

[TestFixture]
public class FillWithBlock2 {
  [Test]
  public void FillArrayWithCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill((ushort)0, 0, 5);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill((ushort)0, 5, 2);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void CountTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ushort)0, 5, 10);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ushort)0, 15, 2);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ushort)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void CountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ushort)0, 3, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void FillAtIntPtrCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    }; //{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((ushort)0, 5);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillAtIntPtrOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((ushort)0, 5, 2);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void IntPtrCountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((ushort)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }

  [Test]
  public void IntPtrCountTooSmall2() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((ushort)0, 5, -3);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }
}

[TestFixture]
public class FillWithBlock4 {
  [Test]
  public void FillArrayWithCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill((uint)0, 0, 3);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1
    };
    byteArray1.Fill((uint)0, 3, 2);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void CountTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((uint)0, 2, 5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((uint)0, 8, 2);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((uint)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void CountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((uint)0, 3, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void FillAtIntPtrCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    }; //{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((uint)0, 3);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillAtIntPtrOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1
    };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((uint)0, 3, 1);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void IntPtrCountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((uint)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }

  [Test]
  public void IntPtrCountTooSmall2() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((uint)0, 5, -3);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }
}

[TestFixture]
public class FillWithBlock8 {
  [Test]
  public void FillLargeArray() {
    // Use 8MB instead of 64MB to avoid OOM on 32-bit runtimes
    var byteArray1 = new byte[8 * 1024 * 1024];
    var byteArray2 = new byte[8 * 1024 * 1024];
    for (var i = 0; i < byteArray2.Length; ++i)
      byteArray2[i] = 1;

    byteArray1.Fill(1);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill((ulong)0, 0, 2);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillArrayWithOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1
    };
    byteArray1.Fill((ulong)0, 1, 1);
    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void CountTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ulong)0, 1, 2);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooLarge() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ulong)0, 3, 0);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void OffsetTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ulong)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void CountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    try {
      byteArray1.Fill((ulong)0, 3, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { }
  }

  [Test]
  public void FillAtIntPtrCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1
    }; //{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((ulong)0, 2);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void FillAtIntPtrOffsetAndCount() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var byteArray2 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      1,
      1,
      1,
      1,
      1
    };

    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();
    pointer.Fill((ulong)0, 1, 1);
    pinnedArray.Free();

    Assert.IsTrue(byteArray1.SequenceEqual(byteArray2));
  }

  [Test]
  public void IntPtrCountTooSmall() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((ulong)0, -5);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }

  [Test]
  public void IntPtrCountTooSmall2() {
    var byteArray1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var pinnedArray = GCHandle.Alloc(byteArray1, GCHandleType.Pinned);
    var pointer = pinnedArray.AddrOfPinnedObject();

    try {
      pointer.Fill((ulong)0, 5, -3);
      Assert.Fail();
    } catch (ArgumentOutOfRangeException) { } finally {
      //todo: write for other blocks as well
      pinnedArray.Free();
    }
  }
}

[TestFixture]
public class SequenceEquals {
  [Test]
  public void SequenceEqualsBytes() {
    for (var i = 0; i < 16383; ++i) {
      var array1 = Enumerable.Range(0, i).Select(_ => (byte)1).ToArray();
      var array2 = Enumerable.Range(0, i).Select(_ => (byte)1).ToArray();
      for (var j = 0; j < 3; ++j)
        Assert.IsTrue(array1.SequenceEqual(array2));
    }
  }
}

[TestFixture]
public class CopyTo {
  [Test]
  public void CopyToBytes() {
    for (var i = 1; i < 16383; ++i) {
      var array1 = Enumerable.Range(0, i).Select(_ => (byte)1).ToArray();
      var array2 = new byte[array1.Length];
      array1.CopyTo(array2);
      Assert.IsTrue(array1.SequenceEqual(array2));
    }
  }

  [Test]
  public void CopyToDecimals() {
    for (var i = 1; i < 16383; ++i) {
      var array1 = Enumerable.Range(0, i).Select(_ => 1m).ToArray();
      var array2 = new decimal[array1.Length];
      array1.CopyTo(array2);
      Assert.IsTrue(array1.SequenceEqual(array2));
    }
  }

  [Test]
  public void CopyToInts() {
    for (var i = 1; i < 16383; ++i) {
      var array1 = Enumerable.Range(0, i).Select(_ => 1).ToArray();
      var array2 = new int[array1.Length];
      array1.CopyTo(array2);
      Assert.IsTrue(array1.SequenceEqual(array2));
    }
  }
}

[TestFixture]
public class Clear {
  [Test]
  public void ClearFloats() {
    var array1 = new float[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (float)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearDoubles() {
    var array1 = new double[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (double)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearBytes() {
    var array1 = new byte[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (byte)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearUShorts() {
    var array1 = new ushort[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (ushort)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearShorts() {
    var array1 = new short[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (short)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearUInts() {
    var array1 = new uint[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (uint)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearInts() {
    var array1 = new int[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (int)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearULongs() {
    var array1 = new ulong[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (ulong)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void ClearLongs() {
    var array1 = new long[] {
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1,
      1
    };
    var array2 = array1.Select(i => (long)0).ToArray();
    array1.Clear();
    Assert.IsTrue(array1.SequenceEqual(array2));
  }
}
