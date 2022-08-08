using NUnit.Framework;

namespace System.ArrayExtensionsTests {
  [TestFixture]
  public class IndexOf {
    [Test]
    public void BothArraysAreEqual() {
      var byteArray1 = new byte[] {65, 66, 67};
      var byteArray2 = new byte[] {65, 66, 67};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void DoesNotContainPattern() {
      var byteArray1 = new byte[] {65, 66, 67};
      var byteArray2 = new byte[] {68, 69, 61};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, -1);
    }

    [Test]
    public void DataToShort() {
      var byteArray1 = new byte[] {65, 66};
      var byteArray2 = new byte[] {65, 66, 67};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, -1);
    }

    [Test]
    public void MatchesAtStart() {
      var byteArray1 = new byte[] {65, 66, 67};
      var byteArray2 = new byte[] {65, 66};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void MatchesAtEnd() {
      var byteArray1 = new byte[] {65, 66, 67};
      var byteArray2 = new byte[] {66, 67};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 1);
    }

    [Test]
    public void DataIsEmpty() {
      var byteArray1 = new byte[] { };
      var byteArray2 = new byte[] {65, 66};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, -1);
    }

    [Test]
    public void NeedleIsEmpty() {
      var byteArray1 = new byte[] {65, 66, 67};
      var byteArray2 = new byte[] { };
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void SameBytesButShorterPattern() {
      var byteArray1 = new byte[] {65, 65, 65};
      var byteArray2 = new byte[] {65, 65};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void SameBytesSomeWhereImData() {
      var byteArray1 = new byte[] {63, 62, 65, 65, 65};
      var byteArray2 = new byte[] {65, 65, 65};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2);
      Assert.AreEqual(index, 2);
    }

    [Test]
    public void ReferenceComparision() {
      var byteArray1 = new byte[] {65, 65};
      var index = byteArray1.IndexOfOrMinusOne(byteArray1);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void PatternIsNull() {
      var byteArray1 = new byte[] {65, 65};
      try {
        var index = byteArray1.IndexOfOrMinusOne(null);
        Assert.Fail();
      } catch (ArgumentNullException) { }
    }

    [Test]
    public void DataIsNull() {
      var byteArray2 = new byte[] {65, 65, 65};
      try {
        var index = ArrayExtensions.IndexOfOrMinusOne(null, byteArray2);
        Assert.Fail();
      } catch (ArgumentNullException) { }
    }

    [Test]
    public void BothAreNull() {
      try {
        var index = ArrayExtensions.IndexOfOrMinusOne(null, null);
        ;
        Assert.Fail();
      } catch (ArgumentNullException) { }
    }

    [Test]
    public void BothAreBigger64k() {
      var byteArray1 = new byte[0x111000];
      var byteArray2 = new byte[0x111000];
      var index = byteArray1.IndexOfOrMinusOne(byteArray2, 0);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void BothAreJustInitialized() {
      var byteArray1 = new byte[0x21];
      var byteArray2 = new byte[0x21];
      var index = byteArray1.IndexOfOrMinusOne(byteArray2, 0);
      Assert.AreEqual(index, 0);
    }

    [Test]
    public void OffsetOutOfBounds() {
      var byteArray1 = new byte[] {63, 62, 65, 65, 65};
      var byteArray2 = new byte[] {65, 65, 65};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2, 5);
      Assert.AreEqual(index, -1);
    }

    [Test]
    public void OffsetInRange() {
      var byteArray1 = new byte[] {63, 62, 65, 65, 65};
      var byteArray2 = new byte[] {65, 65, 65};
      var index = byteArray1.IndexOfOrMinusOne(byteArray2, 2);
      Assert.AreEqual(index, 2);
    }
  }
}
