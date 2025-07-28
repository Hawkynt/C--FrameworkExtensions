using NUnit.Framework;

namespace System.Collections.Concurrent;

[TestFixture]
internal class ConcurrentBagTests {

  [Test]
  public void Clear_Removes_All_Items() {
    ConcurrentBag<int> bag = new();
    bag.Add(1);
    bag.Add(2);
    bag.Clear();
    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  public void Clear_EmptyBag_DoesNothing() {
    ConcurrentBag<int> bag = new();
    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  public void Clear_Null_Throws() {
    ConcurrentBag<int>? bag = null;
    Assert.Throws<NullReferenceException>(() => bag!.Clear());
  }

}
