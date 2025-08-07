using System.Linq;
using NUnit.Framework;

namespace System.Collections.Concurrent;

[TestFixture]
internal class ConcurrentQueueTests {
  [Test]
  public void PullTo_Span_ReturnsCorrectItems() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    Span<int> buffer = stackalloc int[2];
    var result = queue.PullTo(buffer);

    Assert.That(result.Length, Is.EqualTo(2));
    Assert.That(result[0], Is.EqualTo(1));
    Assert.That(result[1], Is.EqualTo(2));
  }

  [Test]
  public void PullTo_Array_ReturnsCorrectItems() {
    var queue = new ConcurrentQueue<string>();
    queue.Enqueue("A");
    queue.Enqueue("B");

    var buffer = new string[5];
    var pulled = queue.PullTo(buffer);

    Assert.That(pulled, Is.EqualTo(2));
    Assert.That(buffer[0], Is.EqualTo("A"));
    Assert.That(buffer[1], Is.EqualTo("B"));
  }

  [Test]
  public void PullTo_ArrayWithOffset_PullsIntoOffset() {
    var queue = new ConcurrentQueue<char>();
    queue.Enqueue('X');
    queue.Enqueue('Y');
    queue.Enqueue('Z');

    var buffer = new char[5];
    var pulled = queue.PullTo(buffer, 2);

    Assert.That(pulled, Is.EqualTo(3));
    Assert.That(buffer[2], Is.EqualTo('X'));
    Assert.That(buffer[3], Is.EqualTo('Y'));
    Assert.That(buffer[4], Is.EqualTo('Z'));
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_PullsCorrectly() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    var buffer = new int[5];
    var pulled = queue.PullTo(buffer, 1, 2);

    Assert.That(pulled, Is.EqualTo(2));
    Assert.That(buffer[1], Is.EqualTo(1));
    Assert.That(buffer[2], Is.EqualTo(2));
  }

  [Test]
  public void PullAll_ReturnsAllElements() {
    var queue = new ConcurrentQueue<string>();
    queue.Enqueue("one");
    queue.Enqueue("two");

    var result = queue.PullAll();

    Assert.That(result, Is.EqualTo(new[] { "one", "two" }));
    Assert.That(queue.Count, Is.Zero);
  }

  [Test]
  [TestCase(2)]
  [TestCase(63)]
  [TestCase(64)]
  [TestCase(65)]
  [TestCase(127)]
  [TestCase(128)]
  [TestCase(129)]
  [TestCase(2052)]
  [TestCase(65535)]
  [TestCase(65536)]
  [TestCase(65537)]
  [TestCase(1 << 24)]
  public void Pull_MaxCount_ReturnsUpToThatMany(int maxCount) {
    var items = Enumerable.Range(1, maxCount).ToArray();
    items.Shuffle();

    var queue = new ConcurrentQueue<int>();
    foreach (var item in items)
      queue.Enqueue(item);

    // pull less items than available
    var lessItemCount = maxCount / 2;
    var lessItems = queue.Pull(lessItemCount);

    // pull more items than available
    var moreItemCount = maxCount - lessItemCount;
    var moreItems = queue.Pull(moreItemCount);


    Assert.That(lessItems.Length, Is.EqualTo(lessItemCount));
    Assert.That(lessItems, Is.EqualTo(items.Take(lessItemCount).ToArray()));

    Assert.That(moreItems.Length, Is.LessThanOrEqualTo(moreItemCount));
    Assert.That(moreItems, Is.EqualTo(items.Skip(lessItemCount).ToArray()));
  }

  [Test]
  public void Pull_EmptyQueue_ReturnsEmptyArray() {
    var queue = new ConcurrentQueue<int>();
    var result = queue.Pull(5);
    Assert.That(result.Length, Is.EqualTo(0));
  }
}
