using NUnit.Framework;

namespace System;

[TestFixture]
public class BoolTests {
  [Test]
  [TestCase(false, false, false, false, false, true, true, true)]
  [TestCase(false, true, false, true, true, true, false, false)]
  [TestCase(true, false, false, true, true, true, false, false)]
  [TestCase(true, true, true, true, false, false, false, true)]
  public void LogicalOps(bool self, bool operand, bool and, bool or, bool xor, bool nand, bool nor, bool equ) {
    Assert.AreEqual(and, self.And(operand), "And broken");
    Assert.AreEqual(or, self.Or(operand), "Or broken");
    Assert.AreEqual(xor, self.Xor(operand), "Xor broken");
    Assert.AreEqual(nand, self.Nand(operand), "Nand broken");
    Assert.AreEqual(nor, self.Nor(operand), "Nor broken");
    Assert.AreEqual(equ, self.Equ(operand), "Equ broken");
  }
}
