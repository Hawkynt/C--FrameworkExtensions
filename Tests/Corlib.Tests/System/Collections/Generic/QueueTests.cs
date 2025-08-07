using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
internal class QueueTests {
  [Test]
  public void PullTo_Span_ReturnsCorrectItems() {
    var queue = new Queue<int>();
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
    var queue = new Queue<string>();
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
    var queue = new Queue<char>();
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
    var queue = new Queue<int>();
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
    var queue = new Queue<string>();
    queue.Enqueue("one");
    queue.Enqueue("two");

    var result = queue.PullAll();

    Assert.That(result, Is.EqualTo(new[] { "one", "two" }));
    Assert.That(queue.Count, Is.Zero);
  }

  [Test]
  public void Pull_MaxCount_ReturnsUpToThatMany() {
    var queue = new Queue<int>();
    queue.Enqueue(5);
    queue.Enqueue(6);

    var result = queue.Pull(3);

    Assert.That(result.Length, Is.EqualTo(2));
    Assert.That(result[0], Is.EqualTo(5));
    Assert.That(result[1], Is.EqualTo(6));
  }

  [Test]
  public void Pull_EmptyQueue_ReturnsEmptyArray() {
    var queue = new Queue<int>();
    var result = queue.Pull(5);
    Assert.That(result.Length, Is.EqualTo(0));
  }
}
