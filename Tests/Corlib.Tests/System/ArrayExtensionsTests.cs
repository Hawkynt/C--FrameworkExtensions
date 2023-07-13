using System.Linq;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Tests; 

[TestFixture]
public class ArrayTests {

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(128)]
  [TestCase(256)]
  [TestCase(512)]
  [TestCase(1024)]
  [TestCase(2048)]
  [TestCase(4096)]
  [TestCase(8192)]
  [TestCase(1<<14)]
  [TestCase(1<<15)]
  [TestCase(1<<16)]
  [TestCase(1<<17)]
  [TestCase(1<<18)]
  [TestCase(1 << 19)]
  [TestCase(1 << 20)]
  [TestCase(1 << 21)]
  [TestCase(1 << 22)]
  [TestCase(1 << 23 - 1)]
  [TestCase(1 << 24 + 1)]
  public void CopyBytes_Test(int size) {
    var source = new byte[size];
    var target = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < target.Length; ++i)
      expected[i] = source[i] = (byte)~i;

    source.CopyTo(target);

    CollectionAssert.AreEqual(target, expected);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(128)]
  [TestCase(256)]
  [TestCase(512)]
  [TestCase(1024)]
  [TestCase(2048)]
  [TestCase(4096)]
  [TestCase(8192)]
  public void Not_Test(int size) {
    var inout = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i)
      expected[i] = (byte)~(inout[i] = (byte)i);

    inout.Not();

    CollectionAssert.AreEqual(inout,expected);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(128)]
  [TestCase(256)]
  [TestCase(512)]
  [TestCase(1024)]
  [TestCase(2048)]
  [TestCase(4096)]
  [TestCase(8192)]
  public void And_Test(int size) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] & operand[i]);
    }

    inout.And(operand);

    CollectionAssert.AreEqual(inout, expected);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(128)]
  [TestCase(256)]
  [TestCase(512)]
  [TestCase(1024)]
  [TestCase(2048)]
  [TestCase(4096)]
  [TestCase(8192)]
  public void Or_Test(int size) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] | operand[i]);
    }

    inout.Or(operand);

    CollectionAssert.AreEqual(inout, expected);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(4)]
  [TestCase(8)]
  [TestCase(16)]
  [TestCase(32)]
  [TestCase(64)]
  [TestCase(128)]
  [TestCase(256)]
  [TestCase(512)]
  [TestCase(1024)]
  [TestCase(2048)]
  [TestCase(4096)]
  [TestCase(8192)]
  public void Xor_Test(int size) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] ^ operand[i]);
    }

    inout.Xor(operand);

    CollectionAssert.AreEqual(inout, expected);
  }

  [Test]
  [TestCase(0)]
  [TestCase(1)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(7)]
  [TestCase(8)]
  [TestCase(9)]
  [TestCase(15)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(127)]
  [TestCase(128)]
  [TestCase(129)]
  [TestCase(255)]
  [TestCase(256)]
  [TestCase(257)]
  [TestCase(511)]
  [TestCase(512)]
  [TestCase(513)]
  [TestCase(1023)]
  [TestCase(1024)]
  [TestCase(1025)]
  public void SequenceEquals_Test(int size) {
    var source = new byte[size];
    var target = new byte[size];
    for (var i = 0; i < source.Length; ++i)
      source[i] = target[i] = (byte)i;

    var shouldBeTrue = source.SequenceEqual(target);

    Assert.IsTrue(shouldBeTrue, $"Equals for length {size}");

    if (target.Length < 1)
      return;

    target[^1] = 1;
    var shouldBeFalse = source.SequenceEqual(target);
    Assert.IsFalse(shouldBeFalse);
    Assert.IsTrue(shouldBeTrue, $"Unequals for length {size}");
  }


  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "a")]
  public void TryGetFirst(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((string?[])null!).TryGetFirst(out v) : ConvertFromStringToTestArray(input)?.ToArray().TryGetFirst(out v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "b")]
  public void TryGetLast(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((string?[])null!).TryGetLast(out v) : ConvertFromStringToTestArray(input)?.ToArray().TryGetLast(out v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, 0, false, null, typeof(NullReferenceException))]
  [TestCase("", -1, false, null, typeof(IndexOutOfRangeException))]
  [TestCase("a", 1, false, null)]
  [TestCase("a|b|c", 1, true, "b")]
  public void TryGetItem(string? input, int index, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((string?[])null!).TryGetItem(index, out v) : ConvertFromStringToTestArray(input)?.ToArray().TryGetItem(index, out v);
      return (r, v);
    }, (result, expected), exception);
  }

}