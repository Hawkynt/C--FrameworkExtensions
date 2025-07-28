using NUnit.Framework;

namespace System.Collections.Concurrent;

[TestFixture]
internal class ConcurrentBagTests {
  [Test]
  public void Clear_Removes_All_Items() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);

    bag.Clear();

    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  public void Clear_EmptyBag_DoesNothing() {
    var bag = new ConcurrentBag<int>();

    bag.Clear();

    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  public void Clear_Null_Throws() {
    ConcurrentBag<int>? bag = null;
    Assert.Throws<NullReferenceException>(() => bag!.Clear());
  }
}
