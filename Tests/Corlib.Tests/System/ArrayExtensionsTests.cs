using NUnit.Framework;

namespace System.Tests; 

[TestFixture]
public class ArrayExtensionsTests {
    
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

    target[target.Length-1] = 1;
    var shouldBeFalse = source.SequenceEqual(target);
    Assert.IsFalse(shouldBeFalse);
    Assert.IsTrue(shouldBeTrue, $"Unequals for length {size}");
  }
}