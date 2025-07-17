using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System;

[TestFixture]
public class ArrayTests {

  [Test]
  [TestCase(0,typeof(ArgumentOutOfRangeException))]
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
  public void CopyBytes_Test(int size, Type? exception = null) {
    var source = new byte[size];
    var target = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < target.Length; ++i)
      expected[i] = source[i] = (byte)~i;

    ExecuteTest(() => source.CopyTo(target),()=> CollectionAssert.AreEqual(target, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Not_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i)
      expected[i] = (byte)~(inout[i] = (byte)i);

    ExecuteTest(inout.Not, () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void And_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] & operand[i]);
    }

    ExecuteTest(() => inout.And(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Nand_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)~(inout[i] & operand[i]);
    }

    ExecuteTest(() => inout.Nand(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Or_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] | operand[i]);
    }

    ExecuteTest(() => inout.Or(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Nor_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)~(inout[i] | operand[i]);
    }

    ExecuteTest(() => inout.Nor(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Xor_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)(inout[i] ^ operand[i]);
    }

    ExecuteTest(() => inout.Xor(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
  }

  [Test]
  [TestCase(0, typeof(ArgumentOutOfRangeException))]
  [TestCase(1)]
  [TestCase(8192)]
  public void Equ_Test(int size, Type? exception = null) {
    var inout = new byte[size];
    var operand = new byte[size];
    var expected = new byte[size];
    for (var i = 0; i < inout.Length; ++i) {
      inout[i] = (byte)i;
      operand[i] = (byte)~i;
      expected[i] = (byte)~(inout[i] ^ operand[i]);
    }

    ExecuteTest(() => inout.Equ(operand), () => CollectionAssert.AreEqual(inout, expected), exception);
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

    if (target.Length <= 0)
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

  [Test]
  [TestCase(null, null, null, typeof(NullReferenceException))]
  [TestCase("", null, null, typeof(ArgumentNullException))]
  [TestCase("", "", "")]
  [TestCase("a", "a", "0,=,0")]
  [TestCase("a", "b", "0,*,0")]
  [TestCase("", "a", "-1,-,0")]
  [TestCase("a", "", "0,+,-1")]
  [TestCase("a|b", "b", "0,+,-1|1,=,0")]
  [TestCase("b|a", "b", "0,=,0|1,+,-1")]
  public void CompareTo(string? input, string? other, string? expected, Type? exception = null) {
    var a = ConvertFromStringToTestArray(input)?.ToArray();
    var b = ConvertFromStringToTestArray(other)?.ToArray();
    ArrayExtensions.IChangeSet<string?>[] Provider() => a.CompareTo(b).ToArray();

    if (exception != null) {
      Assert.That(Provider, Throws.TypeOf(exception));
      return;
    }

    var ex = ConvertFromStringToTestArray(expected)!.Select(i => i!.Split(',')).Select(t => new {
      CurrentIndex = int.Parse(t[0]), OtherIndex = int.Parse(t[2]), Type = t[1] switch {
        "+" => ArrayExtensions.ChangeType.Added,
        "-" => ArrayExtensions.ChangeType.Removed,
        "=" => ArrayExtensions.ChangeType.Equal,
        "*" => ArrayExtensions.ChangeType.Changed,
        _ => throw new ArgumentOutOfRangeException()
      }
    }).ToArray();

    var c = Provider();
    Assert.That(c.Length, Is.EqualTo(ex.Length));
    for (var i = 0; i < ex.Length; ++i) {
      Assert.That(c[i].Type, Is.EqualTo(ex[i].Type));
      Assert.That(c[i].CurrentIndex, Is.EqualTo(ex[i].CurrentIndex));
      Assert.That(c[i].OtherIndex, Is.EqualTo(ex[i].OtherIndex));
    }

  }

  [Test]
  [TestCase(null,null)]
  [TestCase("a", "a")]
  [TestCase("!", "!")]
  [TestCase("a|b", "a|b")]
  [TestCase("!|b", "!|b")]
  [TestCase("a|!", "a|!")]
  public void SafelyClone(string? input, string? output) {
    ExecuteTest(()=>ConvertFromStringToTestArray(input)?.ToArray().SafelyClone(),ConvertFromStringToTestArray(output)?.ToArray(),null);
  }
  
  private static IEnumerable<TestCaseData> _ToHexGenerator() {
    foreach (var i in new byte[]{0x00,0x01,0x20,0xa0,0x0b,0xcd,0xff}) {
      yield return new(new[] { i }, true, i.ToString("X2"));
      yield return new(new[] { i }, false, i.ToString("x2"));
    }

    yield return new(new byte[] { 0x00, 0x01, 0x20, 0xa0, 0x0b, 0xcd, 0xff }, false, "000120a00bcdff");
    yield return new(new byte[] { 0x00, 0x01, 0x20, 0xa0, 0x0b, 0xcd, 0xff }, true, "000120A00BCDFF");
  }
  
  [Test]
  [TestCaseSource(nameof(_ToHexGenerator))]
  public void ToHex(byte[] input,bool allUpperCase,string expected) {
    var got = input.ToHex(allUpperCase);
    Assert.AreEqual(expected,got);
  }
  
  private static IEnumerable<TestCaseData> _ToBinGenerator() {
    foreach (var i in new byte[] { 0x00, 0x01, 0x20, 0xa0, 0x0b, 0xcd, 0xff })
      yield return new(new[] { i }, ToBin(i));

    yield return new(new byte[] { 0x00, 0x01, 0x20, 0xa0, 0x0b, 0xcd, 0xef }, "0000 0000   0000 0001   0010 0000   1010 0000   0000 1011   1100 1101   1110 1111".Replace(" ", string.Empty));

    static string ToBin(int value) => Convert.ToString(value, 2).PadLeft(8, '0');

  }

  [Test]
  [TestCaseSource(nameof(_ToBinGenerator))]
  public void ToBin(byte[] input, string expected) {
    var got = input.ToBin();
    Assert.AreEqual(expected, got);
  }

}