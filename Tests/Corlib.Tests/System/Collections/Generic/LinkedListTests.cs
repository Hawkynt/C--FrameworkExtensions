using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class LinkedListTests {
  
  [Test]
  public void TestQueueBehavior() {
    var queue = new Queue<int>();
    var list = new LinkedList<int>();

    for (var i = 0; i < 10; ++i) {
      queue.Enqueue(i);
      list.Enqueue(i);
    }

    var y0 = queue.Dequeue();
    var y1 = list.Dequeue();

    Assert.That(y0,Is.EqualTo(y1));
    Assert.That(queue.ToArray(), Is.EqualTo(list.ToArray()));
  }

  [Test]
  public void TestStackBehavior() {
    var stack = new Stack<int>();
    var list = new LinkedList<int>();

    for (var i = 0; i < 10; ++i) {
      stack.Push(i);
      list.Push(i);
    }

    var y0 = stack.Pop();
    var y1 = list.Pop();

    Assert.That(y0, Is.EqualTo(y1));
    Assert.That(stack.ToArray(), Is.EqualTo(list.ToArray()));
  }

}
