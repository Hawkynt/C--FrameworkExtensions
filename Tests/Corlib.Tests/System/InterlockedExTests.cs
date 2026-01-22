using NUnit.Framework;

namespace System.Threading;

[TestFixture]
internal class InterlockedExTests {
  [Flags]
  private enum TestFlags {
    None = 0,
    A = 1,
    B = 2,
    C = 4
  }

  private sealed class TestClass {
    public int Value;
    public TestClass(int value) => this.Value = value;
  }

  [Test]
  public void CompareExchange_Enum_UpdatesValue() {
    var flags = TestFlags.None;
    var previous = Interlocked.CompareExchange(ref flags, TestFlags.A, TestFlags.None);
    Assert.AreEqual(TestFlags.None, previous);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void CompareExchange_Enum_NoUpdateOnMismatch() {
    var flags = TestFlags.A;
    var previous = Interlocked.CompareExchange(ref flags, TestFlags.B, TestFlags.C);
    Assert.AreEqual(TestFlags.A, previous);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void Exchange_Enum_ReplacesValue() {
    var flags = TestFlags.A;
    var previous = Interlocked.Exchange(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A, previous);
    Assert.AreEqual(TestFlags.B, flags);
  }

  [Test]
  public void Read_Enum_ReturnsCurrentValue() {
    var flags = TestFlags.B;
    var current = Interlocked.Read(ref flags);
    Assert.AreEqual(TestFlags.B, current);
    Assert.AreEqual(TestFlags.B, flags);
  }

  [Test]
  public void FlagOperations_ModifyBitsCorrectly() {
    var flags = TestFlags.None;

    Interlocked.SetFlag(ref flags, TestFlags.A);
    Assert.AreEqual(TestFlags.A, flags);
    Assert.IsTrue(Interlocked.HasFlag(ref flags, TestFlags.A));

    Interlocked.ToggleFlag(ref flags, TestFlags.A | TestFlags.B);
    Assert.AreEqual(TestFlags.B, flags);

    Interlocked.SetFlag(ref flags, TestFlags.C);
    Assert.AreEqual(TestFlags.B | TestFlags.C, flags);

    Interlocked.ClearFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.C, flags);
  }

  [Test]
  public void CompareExchange_Reference_UpdatesValue() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = Interlocked.CompareExchange(ref obj, target, original);
    Assert.AreSame(original, previous);
    Assert.AreSame(target, obj);
  }

  [Test]
  public void Exchange_Reference_ReplacesValue() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = Interlocked.Exchange(ref obj, target);
    Assert.AreSame(original, previous);
    Assert.AreSame(target, obj);
  }

  [Test]
  public void CompareExchange_Reference_NoUpdateOnMismatch() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = Interlocked.CompareExchange(ref obj, target, null);
    Assert.AreSame(original, previous);
    Assert.AreSame(original, obj);
  }

  [Test]
  public void ClearFlag_UnsetFlag_DoesNotChangeValue() {
    var flags = TestFlags.A;
    var result = Interlocked.ClearFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A, result);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void ToggleFlag_TogglesBitsCorrectly() {
    var flags = TestFlags.A;
    var first = Interlocked.ToggleFlag(ref flags, TestFlags.B | TestFlags.C);
    Assert.AreEqual(TestFlags.A | TestFlags.B | TestFlags.C, first);
    var second = Interlocked.ToggleFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A | TestFlags.C, second);
  }

  [Test]
  public void HasFlag_FalseWhenNotSet() {
    var flags = TestFlags.A;
    Assert.IsFalse(Interlocked.HasFlag(ref flags, TestFlags.B));
  }

  [Test]
  public void Int32_Operations_WorkCorrectly() {
    var value = 0;
    Assert.AreEqual(0, Interlocked.CompareExchange(ref value, 1, 0));
    Assert.AreEqual(1, value);
    Assert.AreEqual(1, Interlocked.Exchange(ref value, 2));
    Assert.AreEqual(2, value);
    Assert.AreEqual(2, Interlocked.Read(ref value));
    Assert.AreEqual(3, Interlocked.Increment(ref value));
    Assert.AreEqual(2, Interlocked.Decrement(ref value));
    Assert.AreEqual(7, Interlocked.Add(ref value, 5));
    Assert.AreEqual(5, Interlocked.Subtract(ref value, 2));
    Assert.AreEqual(15, Interlocked.Multiply(ref value, 3));
    Assert.AreEqual(5, Interlocked.Divide(ref value, 3));
    Assert.AreEqual(2, Interlocked.Modulo(ref value, 3));
    Assert.AreEqual(2, Interlocked.And(ref value, 6));
    Assert.AreEqual(-1, Interlocked.NAnd(ref value, 1));
    Assert.AreEqual(-1, Interlocked.Or(ref value, 1));
    Assert.AreEqual(0, Interlocked.NOr(ref value, 0));
    Assert.AreEqual(1, Interlocked.Xor(ref value, 1));
    Assert.AreEqual(-1, Interlocked.NXor(ref value, 1));
    Assert.AreEqual(0, Interlocked.Not(ref value));
    Assert.AreEqual(0, Interlocked.ArithmeticShiftLeft(ref value, 1));
    Assert.AreEqual(0, Interlocked.ArithmeticShiftRight(ref value, 1));
    Assert.AreEqual(0, Interlocked.LogicalShiftLeft(ref value, 1));
    Assert.AreEqual(0, Interlocked.LogicalShiftRight(ref value, 1));
  }

  [Test]
  public void UInt32_Operations_WorkCorrectly() {
    uint value = 0;
    Assert.AreEqual(0u, Interlocked.CompareExchange(ref value, 1u, 0u));
    Assert.AreEqual(1u, value);
    Assert.AreEqual(1u, Interlocked.Exchange(ref value, 2u));
    Assert.AreEqual(2u, value);
    Assert.AreEqual(2u, Interlocked.Read(ref value));
    Assert.AreEqual(3u, Interlocked.Increment(ref value));
    Assert.AreEqual(2u, Interlocked.Decrement(ref value));
    Assert.AreEqual(7u, Interlocked.Add(ref value, 5u));
    Assert.AreEqual(5u, Interlocked.Subtract(ref value, 2u));
    Assert.AreEqual(15u, Interlocked.Multiply(ref value, 3u));
    Assert.AreEqual(5u, Interlocked.Divide(ref value, 3u));
    Assert.AreEqual(2u, Interlocked.Modulo(ref value, 3u));
    Assert.AreEqual(2u, Interlocked.And(ref value, 6u));
    Assert.AreEqual(uint.MaxValue, Interlocked.NAnd(ref value, 1u));
    Assert.AreEqual(uint.MaxValue, Interlocked.Or(ref value, 1u));
    Assert.AreEqual(0u, Interlocked.NOr(ref value, 0u));
    Assert.AreEqual(1u, Interlocked.Xor(ref value, 1u));
    Assert.AreEqual(uint.MaxValue, Interlocked.NXor(ref value, 1u));
    Assert.AreEqual(0u, Interlocked.Not(ref value));
    Assert.AreEqual(0u, Interlocked.ShiftLeft(ref value, 1));
    Assert.AreEqual(0u, Interlocked.ShiftRight(ref value, 1));
    Assert.AreEqual(0u, Interlocked.RotateLeft(ref value, 1));
    Assert.AreEqual(0u, Interlocked.RotateRight(ref value, 1));
  }

  [Test]
  public void Int64_Operations_WorkCorrectly() {
    long value = 0;
    Assert.AreEqual(0L, Interlocked.CompareExchange(ref value, 1L, 0L));
    Assert.AreEqual(1L, value);
    Assert.AreEqual(1L, Interlocked.Exchange(ref value, 2L));
    Assert.AreEqual(2L, value);
    Assert.AreEqual(2L, Interlocked.Read(ref value));
    Assert.AreEqual(3L, Interlocked.Increment(ref value));
    Assert.AreEqual(2L, Interlocked.Decrement(ref value));
    Assert.AreEqual(7L, Interlocked.Add(ref value, 5L));
    Assert.AreEqual(5L, Interlocked.Subtract(ref value, 2L));
    Assert.AreEqual(15L, Interlocked.Multiply(ref value, 3L));
    Assert.AreEqual(5L, Interlocked.Divide(ref value, 3L));
    Assert.AreEqual(2L, Interlocked.Modulo(ref value, 3L));
    Assert.AreEqual(2L, Interlocked.And(ref value, 6L));
    Assert.AreEqual(-1L, Interlocked.NAnd(ref value, 1L));
    Assert.AreEqual(-1L, Interlocked.Or(ref value, 1L));
    Assert.AreEqual(0L, Interlocked.NOr(ref value, 0L));
    Assert.AreEqual(1L, Interlocked.Xor(ref value, 1L));
    Assert.AreEqual(-1L, Interlocked.NXor(ref value, 1L));
    Assert.AreEqual(0L, Interlocked.Not(ref value));
    Assert.AreEqual(0L, Interlocked.ArithmeticShiftLeft(ref value, 1));
    Assert.AreEqual(0L, Interlocked.ArithmeticShiftRight(ref value, 1));
    Assert.AreEqual(0L, Interlocked.LogicalShiftLeft(ref value, 1));
    Assert.AreEqual(0L, Interlocked.LogicalShiftRight(ref value, 1));
  }

  [Test]
  public void UInt64_Operations_WorkCorrectly() {
    ulong value = 0;
    Assert.AreEqual(0ul, Interlocked.CompareExchange(ref value, 1ul, 0ul));
    Assert.AreEqual(1ul, value);
    Assert.AreEqual(1ul, Interlocked.Exchange(ref value, 2ul));
    Assert.AreEqual(2ul, value);
    Assert.AreEqual(2ul, Interlocked.Read(ref value));
    Assert.AreEqual(3ul, Interlocked.Increment(ref value));
    Assert.AreEqual(2ul, Interlocked.Decrement(ref value));
    Assert.AreEqual(7ul, Interlocked.Add(ref value, 5ul));
    Assert.AreEqual(5ul, Interlocked.Subtract(ref value, 2ul));
    Assert.AreEqual(15ul, Interlocked.Multiply(ref value, 3ul));
    Assert.AreEqual(5ul, Interlocked.Divide(ref value, 3ul));
    Assert.AreEqual(2ul, Interlocked.Modulo(ref value, 3ul));
    Assert.AreEqual(2ul, Interlocked.And(ref value, 6ul));
    Assert.AreEqual(ulong.MaxValue, Interlocked.NAnd(ref value, 1ul));
    Assert.AreEqual(ulong.MaxValue, Interlocked.Or(ref value, 1ul));
    Assert.AreEqual(0ul, Interlocked.NOr(ref value, 0ul));
    Assert.AreEqual(1ul, Interlocked.Xor(ref value, 1ul));
    Assert.AreEqual(ulong.MaxValue, Interlocked.NXor(ref value, 1ul));
    Assert.AreEqual(0ul, Interlocked.Not(ref value));
    Assert.AreEqual(0ul, Interlocked.ShiftLeft(ref value, 1));
    Assert.AreEqual(0ul, Interlocked.ShiftRight(ref value, 1));
    Assert.AreEqual(0ul, Interlocked.RotateLeft(ref value, 1));
    Assert.AreEqual(0ul, Interlocked.RotateRight(ref value, 1));
  }

  [Test]
  public void Single_Operations_WorkCorrectly() {
    var value = 1f;
    Assert.AreEqual(1f, Interlocked.CompareExchange(ref value, 2f, 1f));
    Assert.AreEqual(2f, value);
    Assert.AreEqual(2f, Interlocked.Exchange(ref value, 3f));
    Assert.AreEqual(3f, value);
    Assert.AreEqual(3f, Interlocked.Read(ref value));
    Assert.AreEqual(4f, Interlocked.Increment(ref value));
    Assert.AreEqual(3f, Interlocked.Decrement(ref value));
    Assert.AreEqual(5f, Interlocked.Add(ref value, 2f));
    Assert.AreEqual(4f, Interlocked.Subtract(ref value, 1f));
    Assert.AreEqual(8f, Interlocked.Multiply(ref value, 2f));
    Assert.AreEqual(4f, Interlocked.Divide(ref value, 2f));
    Assert.AreEqual(1f, Interlocked.Modulo(ref value, 3f));
  }

  [Test]
  public void Double_Operations_WorkCorrectly() {
    var value = 1d;
    Assert.AreEqual(1d, Interlocked.CompareExchange(ref value, 2d, 1d));
    Assert.AreEqual(2d, value);
    Assert.AreEqual(2d, Interlocked.Exchange(ref value, 3d));
    Assert.AreEqual(3d, value);
    Assert.AreEqual(3d, Interlocked.Read(ref value));
    Assert.AreEqual(4d, Interlocked.Increment(ref value));
    Assert.AreEqual(3d, Interlocked.Decrement(ref value));
    Assert.AreEqual(5d, Interlocked.Add(ref value, 2d));
    Assert.AreEqual(4d, Interlocked.Subtract(ref value, 1d));
    Assert.AreEqual(8d, Interlocked.Multiply(ref value, 2d));
    Assert.AreEqual(4d, Interlocked.Divide(ref value, 2d));
    Assert.AreEqual(1d, Interlocked.Modulo(ref value, 3d));
  }

  [Test]
  public void ShiftOperations_ThrowOnOverflow() {
    var intVal = 0x40000000;
    Assert.Throws<OverflowException>(() => Interlocked.ArithmeticShiftLeft(ref intVal, 1));
    intVal = int.MinValue;
    Assert.Throws<OverflowException>(() => Interlocked.LogicalShiftRight(ref intVal, 1));

    var longVal = 0x4000000000000000L;
    Assert.Throws<OverflowException>(() => Interlocked.ArithmeticShiftLeft(ref longVal, 1));
    longVal = long.MinValue;
    Assert.Throws<OverflowException>(() => Interlocked.LogicalShiftRight(ref longVal, 1));
  }

  [Test]
  public void ExtensionBlock_Int32_CanBeCalledViaInterlocked() {
    var value = 10;
    Assert.AreEqual(10, Interlocked.Read(ref value));
    Assert.AreEqual(20, Interlocked.Add(ref value, 10));
    Assert.AreEqual(15, Interlocked.Subtract(ref value, 5));
    Assert.AreEqual(30, Interlocked.Multiply(ref value, 2));
    Assert.AreEqual(10, Interlocked.Divide(ref value, 3));
    Assert.AreEqual(1, Interlocked.Modulo(ref value, 3));
  }

  [Test]
  public void ExtensionBlock_UInt32_CanBeCalledViaInterlocked() {
    var value = 10u;
    Assert.AreEqual(10u, Interlocked.Read(ref value));
    Assert.AreEqual(20u, Interlocked.Add(ref value, 10u));
    Assert.AreEqual(15u, Interlocked.Subtract(ref value, 5u));
    Assert.AreEqual(30u, Interlocked.Multiply(ref value, 2u));
    Assert.AreEqual(10u, Interlocked.Divide(ref value, 3u));
    Assert.AreEqual(1u, Interlocked.Modulo(ref value, 3u));
  }

  [Test]
  public void ExtensionBlock_Int64_CanBeCalledViaInterlocked() {
    var value = 10L;
    Assert.AreEqual(10L, Interlocked.Read(ref value));
    Assert.AreEqual(20L, Interlocked.Add(ref value, 10L));
    Assert.AreEqual(15L, Interlocked.Subtract(ref value, 5L));
    Assert.AreEqual(30L, Interlocked.Multiply(ref value, 2L));
    Assert.AreEqual(10L, Interlocked.Divide(ref value, 3L));
    Assert.AreEqual(1L, Interlocked.Modulo(ref value, 3L));
  }

  [Test]
  public void ExtensionBlock_UInt64_CanBeCalledViaInterlocked() {
    var value = 10ul;
    Assert.AreEqual(10ul, Interlocked.Read(ref value));
    Assert.AreEqual(20ul, Interlocked.Add(ref value, 10ul));
    Assert.AreEqual(15ul, Interlocked.Subtract(ref value, 5ul));
    Assert.AreEqual(30ul, Interlocked.Multiply(ref value, 2ul));
    Assert.AreEqual(10ul, Interlocked.Divide(ref value, 3ul));
    Assert.AreEqual(1ul, Interlocked.Modulo(ref value, 3ul));
  }

  [Test]
  public void ExtensionBlock_Float_CanBeCalledViaInterlocked() {
    var value = 10f;
    Assert.AreEqual(10f, Interlocked.Read(ref value));
    Assert.AreEqual(20f, Interlocked.Add(ref value, 10f));
    Assert.AreEqual(15f, Interlocked.Subtract(ref value, 5f));
    Assert.AreEqual(30f, Interlocked.Multiply(ref value, 2f));
    Assert.AreEqual(15f, Interlocked.Divide(ref value, 2f));
    Assert.AreEqual(0f, Interlocked.Modulo(ref value, 3f));
  }

  [Test]
  public void ExtensionBlock_Double_CanBeCalledViaInterlocked() {
    var value = 10d;
    Assert.AreEqual(10d, Interlocked.Read(ref value));
    Assert.AreEqual(20d, Interlocked.Add(ref value, 10d));
    Assert.AreEqual(15d, Interlocked.Subtract(ref value, 5d));
    Assert.AreEqual(30d, Interlocked.Multiply(ref value, 2d));
    Assert.AreEqual(15d, Interlocked.Divide(ref value, 2d));
    Assert.AreEqual(0d, Interlocked.Modulo(ref value, 3d));
  }

  [Test]
  public void ExtensionBlock_BitwiseOps_CanBeCalledViaInterlocked() {
    var intVal = 0b1111;
    Interlocked.And(ref intVal, 0b0101);
    Assert.AreEqual(0b0101, intVal);
    Interlocked.Or(ref intVal, 0b1010);
    Assert.AreEqual(0b1111, intVal);
    Interlocked.Xor(ref intVal, 0b1111);
    Assert.AreEqual(0b0000, intVal);
    Interlocked.Not(ref intVal);
    Assert.AreEqual(-1, intVal);

    var uintVal = 0b1111u;
    Interlocked.And(ref uintVal, 0b0101u);
    Assert.AreEqual(0b0101u, uintVal);
    Interlocked.Or(ref uintVal, 0b1010u);
    Assert.AreEqual(0b1111u, uintVal);
    Interlocked.Xor(ref uintVal, 0b1111u);
    Assert.AreEqual(0b0000u, uintVal);
    Interlocked.Not(ref uintVal);
    Assert.AreEqual(uint.MaxValue, uintVal);
  }

  [Test]
  public void ExtensionBlock_ShiftOps_CanBeCalledViaInterlocked() {
    var uintVal = 0b0001u;
    Interlocked.ShiftLeft(ref uintVal, 1);
    Assert.AreEqual(0b0010u, uintVal);
    Interlocked.ShiftRight(ref uintVal, 1);
    Assert.AreEqual(0b0001u, uintVal);

    var ulongVal = 0b0001ul;
    Interlocked.ShiftLeft(ref ulongVal, 1);
    Assert.AreEqual(0b0010ul, ulongVal);
    Interlocked.ShiftRight(ref ulongVal, 1);
    Assert.AreEqual(0b0001ul, ulongVal);
  }

  [Test]
  public void ExtensionBlock_RotateOps_CanBeCalledViaInterlocked() {
    var uintVal = 0x80000000u;
    Interlocked.RotateLeft(ref uintVal, 1);
    Assert.AreEqual(0x00000001u, uintVal);
    Interlocked.RotateRight(ref uintVal, 1);
    Assert.AreEqual(0x80000000u, uintVal);

    var ulongVal = 0x8000000000000000ul;
    Interlocked.RotateLeft(ref ulongVal, 1);
    Assert.AreEqual(0x0000000000000001ul, ulongVal);
    Interlocked.RotateRight(ref ulongVal, 1);
    Assert.AreEqual(0x8000000000000000ul, ulongVal);
  }
}
