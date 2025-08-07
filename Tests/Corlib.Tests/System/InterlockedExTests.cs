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
    var previous = InterlockedEx.CompareExchange(ref flags, TestFlags.A, TestFlags.None);
    Assert.AreEqual(TestFlags.None, previous);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void CompareExchange_Enum_NoUpdateOnMismatch() {
    var flags = TestFlags.A;
    var previous = InterlockedEx.CompareExchange(ref flags, TestFlags.B, TestFlags.C);
    Assert.AreEqual(TestFlags.A, previous);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void Exchange_Enum_ReplacesValue() {
    var flags = TestFlags.A;
    var previous = InterlockedEx.Exchange(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A, previous);
    Assert.AreEqual(TestFlags.B, flags);
  }

  [Test]
  public void Read_Enum_ReturnsCurrentValue() {
    var flags = TestFlags.B;
    var current = InterlockedEx.Read(ref flags);
    Assert.AreEqual(TestFlags.B, current);
    Assert.AreEqual(TestFlags.B, flags);
  }

  [Test]
  public void FlagOperations_ModifyBitsCorrectly() {
    var flags = TestFlags.None;

    InterlockedEx.SetFlag(ref flags, TestFlags.A);
    Assert.AreEqual(TestFlags.A, flags);
    Assert.IsTrue(InterlockedEx.HasFlag(ref flags, TestFlags.A));

    InterlockedEx.ToggleFlag(ref flags, TestFlags.A | TestFlags.B);
    Assert.AreEqual(TestFlags.B, flags);

    InterlockedEx.SetFlag(ref flags, TestFlags.C);
    Assert.AreEqual(TestFlags.B | TestFlags.C, flags);

    InterlockedEx.ClearFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.C, flags);
  }

  [Test]
  public void CompareExchange_Reference_UpdatesValue() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = InterlockedEx.CompareExchange(ref obj, target, original);
    Assert.AreSame(original, previous);
    Assert.AreSame(target, obj);
  }

  [Test]
  public void Exchange_Reference_ReplacesValue() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = InterlockedEx.Exchange(ref obj, target);
    Assert.AreSame(original, previous);
    Assert.AreSame(target, obj);
  }

  [Test]
  public void CompareExchange_Reference_NoUpdateOnMismatch() {
    var original = new TestClass(1);
    var target = new TestClass(2);
    var obj = original;
    var previous = InterlockedEx.CompareExchange(ref obj, target, null);
    Assert.AreSame(original, previous);
    Assert.AreSame(original, obj);
  }

  [Test]
  public void ClearFlag_UnsetFlag_DoesNotChangeValue() {
    var flags = TestFlags.A;
    var result = InterlockedEx.ClearFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A, result);
    Assert.AreEqual(TestFlags.A, flags);
  }

  [Test]
  public void ToggleFlag_TogglesBitsCorrectly() {
    var flags = TestFlags.A;
    var first = InterlockedEx.ToggleFlag(ref flags, TestFlags.B | TestFlags.C);
    Assert.AreEqual(TestFlags.A | TestFlags.B | TestFlags.C, first);
    var second = InterlockedEx.ToggleFlag(ref flags, TestFlags.B);
    Assert.AreEqual(TestFlags.A | TestFlags.C, second);
  }

  [Test]
  public void HasFlag_FalseWhenNotSet() {
    var flags = TestFlags.A;
    Assert.IsFalse(InterlockedEx.HasFlag(ref flags, TestFlags.B));
  }

  [Test]
  public void Int32_Operations_WorkCorrectly() {
    var value = 0;
    Assert.AreEqual(0, InterlockedEx.CompareExchange(ref value, 1, 0));
    Assert.AreEqual(1, value);
    Assert.AreEqual(1, InterlockedEx.Exchange(ref value, 2));
    Assert.AreEqual(2, value);
    Assert.AreEqual(2, InterlockedEx.Read(ref value));
    Assert.AreEqual(3, InterlockedEx.Increment(ref value));
    Assert.AreEqual(2, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(7, InterlockedEx.Add(ref value, 5));
    Assert.AreEqual(5, InterlockedEx.Substract(ref value, 2));
    Assert.AreEqual(15, InterlockedEx.Multiply(ref value, 3));
    Assert.AreEqual(5, InterlockedEx.Divide(ref value, 3));
    Assert.AreEqual(2, InterlockedEx.Modulo(ref value, 3));
    Assert.AreEqual(2, InterlockedEx.And(ref value, 6));
    Assert.AreEqual(-1, InterlockedEx.NAnd(ref value, 1));
    Assert.AreEqual(-1, InterlockedEx.Or(ref value, 1));
    Assert.AreEqual(0, InterlockedEx.NOr(ref value, 0));
    Assert.AreEqual(1, InterlockedEx.Xor(ref value, 1));
    Assert.AreEqual(-1, InterlockedEx.NXor(ref value, 1));
    Assert.AreEqual(0, InterlockedEx.Not(ref value));
    Assert.AreEqual(0, InterlockedEx.ArithmeticShiftLeft(ref value, 1));
    Assert.AreEqual(0, InterlockedEx.ArithmeticShiftRight(ref value, 1));
    Assert.AreEqual(0, InterlockedEx.LogicalShiftLeft(ref value, 1));
    Assert.AreEqual(0, InterlockedEx.LogicalShiftRight(ref value, 1));
  }

  [Test]
  public void UInt32_Operations_WorkCorrectly() {
    uint value = 0;
    Assert.AreEqual(0u, InterlockedEx.CompareExchange(ref value, 1u, 0u));
    Assert.AreEqual(1u, value);
    Assert.AreEqual(1u, InterlockedEx.Exchange(ref value, 2u));
    Assert.AreEqual(2u, value);
    Assert.AreEqual(2u, InterlockedEx.Read(ref value));
    Assert.AreEqual(3u, InterlockedEx.Increment(ref value));
    Assert.AreEqual(2u, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(7u, InterlockedEx.Add(ref value, 5u));
    Assert.AreEqual(5u, InterlockedEx.Substract(ref value, 2u));
    Assert.AreEqual(15u, InterlockedEx.Multiply(ref value, 3u));
    Assert.AreEqual(5u, InterlockedEx.Divide(ref value, 3u));
    Assert.AreEqual(2u, InterlockedEx.Modulo(ref value, 3u));
    Assert.AreEqual(2u, InterlockedEx.And(ref value, 6u));
    Assert.AreEqual(uint.MaxValue, InterlockedEx.NAnd(ref value, 1u));
    Assert.AreEqual(uint.MaxValue, InterlockedEx.Or(ref value, 1u));
    Assert.AreEqual(0u, InterlockedEx.NOr(ref value, 0u));
    Assert.AreEqual(1u, InterlockedEx.Xor(ref value, 1u));
    Assert.AreEqual(uint.MaxValue, InterlockedEx.NXor(ref value, 1u));
    Assert.AreEqual(0u, InterlockedEx.Not(ref value));
    Assert.AreEqual(0u, InterlockedEx.ShiftLeft(ref value, 1));
    Assert.AreEqual(0u, InterlockedEx.ShiftRight(ref value, 1));
    Assert.AreEqual(0u, InterlockedEx.RotateLeft(ref value, 1));
    Assert.AreEqual(0u, InterlockedEx.RotateRight(ref value, 1));
  }

  [Test]
  public void Int64_Operations_WorkCorrectly() {
    long value = 0;
    Assert.AreEqual(0L, InterlockedEx.CompareExchange(ref value, 1L, 0L));
    Assert.AreEqual(1L, value);
    Assert.AreEqual(1L, InterlockedEx.Exchange(ref value, 2L));
    Assert.AreEqual(2L, value);
    Assert.AreEqual(2L, InterlockedEx.Read(ref value));
    Assert.AreEqual(3L, InterlockedEx.Increment(ref value));
    Assert.AreEqual(2L, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(7L, InterlockedEx.Add(ref value, 5L));
    Assert.AreEqual(5L, InterlockedEx.Substract(ref value, 2L));
    Assert.AreEqual(15L, InterlockedEx.Multiply(ref value, 3L));
    Assert.AreEqual(5L, InterlockedEx.Divide(ref value, 3L));
    Assert.AreEqual(2L, InterlockedEx.Modulo(ref value, 3L));
    Assert.AreEqual(2L, InterlockedEx.And(ref value, 6L));
    Assert.AreEqual(-1L, InterlockedEx.NAnd(ref value, 1L));
    Assert.AreEqual(-1L, InterlockedEx.Or(ref value, 1L));
    Assert.AreEqual(0L, InterlockedEx.NOr(ref value, 0L));
    Assert.AreEqual(1L, InterlockedEx.Xor(ref value, 1L));
    Assert.AreEqual(-1L, InterlockedEx.NXor(ref value, 1L));
    Assert.AreEqual(0L, InterlockedEx.Not(ref value));
    Assert.AreEqual(0L, InterlockedEx.ArithmeticShiftLeft(ref value, 1));
    Assert.AreEqual(0L, InterlockedEx.ArithmeticShiftRight(ref value, 1));
    Assert.AreEqual(0L, InterlockedEx.LogicalShiftLeft(ref value, 1));
    Assert.AreEqual(0L, InterlockedEx.LogicalShiftRight(ref value, 1));
  }

  [Test]
  public void UInt64_Operations_WorkCorrectly() {
    ulong value = 0;
    Assert.AreEqual(0ul, InterlockedEx.CompareExchange(ref value, 1ul, 0ul));
    Assert.AreEqual(1ul, value);
    Assert.AreEqual(1ul, InterlockedEx.Exchange(ref value, 2ul));
    Assert.AreEqual(2ul, value);
    Assert.AreEqual(2ul, InterlockedEx.Read(ref value));
    Assert.AreEqual(3ul, InterlockedEx.Increment(ref value));
    Assert.AreEqual(2ul, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(7ul, InterlockedEx.Add(ref value, 5ul));
    Assert.AreEqual(5ul, InterlockedEx.Substract(ref value, 2ul));
    Assert.AreEqual(15ul, InterlockedEx.Multiply(ref value, 3ul));
    Assert.AreEqual(5ul, InterlockedEx.Divide(ref value, 3ul));
    Assert.AreEqual(2ul, InterlockedEx.Modulo(ref value, 3ul));
    Assert.AreEqual(2ul, InterlockedEx.And(ref value, 6ul));
    Assert.AreEqual(ulong.MaxValue, InterlockedEx.NAnd(ref value, 1ul));
    Assert.AreEqual(ulong.MaxValue, InterlockedEx.Or(ref value, 1ul));
    Assert.AreEqual(0ul, InterlockedEx.NOr(ref value, 0ul));
    Assert.AreEqual(1ul, InterlockedEx.Xor(ref value, 1ul));
    Assert.AreEqual(ulong.MaxValue, InterlockedEx.NXor(ref value, 1ul));
    Assert.AreEqual(0ul, InterlockedEx.Not(ref value));
    Assert.AreEqual(0ul, InterlockedEx.ShiftLeft(ref value, 1));
    Assert.AreEqual(0ul, InterlockedEx.ShiftRight(ref value, 1));
    Assert.AreEqual(0ul, InterlockedEx.RotateLeft(ref value, 1));
    Assert.AreEqual(0ul, InterlockedEx.RotateRight(ref value, 1));
  }

  [Test]
  public void Single_Operations_WorkCorrectly() {
    var value = 1f;
    Assert.AreEqual(1f, InterlockedEx.CompareExchange(ref value, 2f, 1f));
    Assert.AreEqual(2f, value);
    Assert.AreEqual(2f, InterlockedEx.Exchange(ref value, 3f));
    Assert.AreEqual(3f, value);
    Assert.AreEqual(3f, InterlockedEx.Read(ref value));
    Assert.AreEqual(4f, InterlockedEx.Increment(ref value));
    Assert.AreEqual(3f, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(5f, InterlockedEx.Add(ref value, 2f));
    Assert.AreEqual(4f, InterlockedEx.Substract(ref value, 1f));
    Assert.AreEqual(8f, InterlockedEx.Multiply(ref value, 2f));
    Assert.AreEqual(4f, InterlockedEx.Divide(ref value, 2f));
    Assert.AreEqual(1f, InterlockedEx.Modulo(ref value, 3f));
  }

  [Test]
  public void Double_Operations_WorkCorrectly() {
    var value = 1d;
    Assert.AreEqual(1d, InterlockedEx.CompareExchange(ref value, 2d, 1d));
    Assert.AreEqual(2d, value);
    Assert.AreEqual(2d, InterlockedEx.Exchange(ref value, 3d));
    Assert.AreEqual(3d, value);
    Assert.AreEqual(3d, InterlockedEx.Read(ref value));
    Assert.AreEqual(4d, InterlockedEx.Increment(ref value));
    Assert.AreEqual(3d, InterlockedEx.Decrement(ref value));
    Assert.AreEqual(5d, InterlockedEx.Add(ref value, 2d));
    Assert.AreEqual(4d, InterlockedEx.Substract(ref value, 1d));
    Assert.AreEqual(8d, InterlockedEx.Multiply(ref value, 2d));
    Assert.AreEqual(4d, InterlockedEx.Divide(ref value, 2d));
    Assert.AreEqual(1d, InterlockedEx.Modulo(ref value, 3d));
  }

  [Test]
  public void ShiftOperations_ThrowOnOverflow() {
    var intVal = 0x40000000;
    Assert.Throws<OverflowException>(() => InterlockedEx.ArithmeticShiftLeft(ref intVal, 1));
    intVal = int.MinValue;
    Assert.Throws<OverflowException>(() => InterlockedEx.LogicalShiftRight(ref intVal, 1));

    var longVal = 0x4000000000000000L;
    Assert.Throws<OverflowException>(() => InterlockedEx.ArithmeticShiftLeft(ref longVal, 1));
    longVal = long.MinValue;
    Assert.Throws<OverflowException>(() => InterlockedEx.LogicalShiftRight(ref longVal, 1));
  }
}
