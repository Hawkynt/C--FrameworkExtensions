using NUnit.Framework;

namespace System.Collections.Concurrent;

[TestFixture]
internal class ConcurrentStackTests {

  [Test]
  public void PullTo_Span_ReturnsTopToBottom() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    Span<int> buffer = stackalloc int[2];
    var result = stack.PullTo(buffer);

    Assert.That(result.Length, Is.EqualTo(2));
    Assert.That(result[0], Is.EqualTo(3));
    Assert.That(result[1], Is.EqualTo(2));
  }

  [Test]
  public void PullTo_Array_CopiesFromTop() {
    var stack = new ConcurrentStack<string>();
    stack.Push("A");
    stack.Push("B");

    var buffer = new string[5];
    var copied = stack.PullTo(buffer);

    Assert.That(copied, Is.EqualTo(2));
    Assert.That(buffer[0], Is.EqualTo("B"));
    Assert.That(buffer[1], Is.EqualTo("A"));
  }

  [Test]
  public void PullTo_ArrayWithOffset_FillsCorrectRegion() {
    var stack = new ConcurrentStack<char>();
    stack.Push('Z');
    stack.Push('Y');
    stack.Push('X');

    var buffer = new char[5];
    var copied = stack.PullTo(buffer, 2);

    Assert.That(copied, Is.EqualTo(3));
    Assert.That(buffer[2], Is.EqualTo('X'));
    Assert.That(buffer[3], Is.EqualTo('Y'));
    Assert.That(buffer[4], Is.EqualTo('Z'));
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMax_PullsLimited() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    var buffer = new int[5];
    var copied = stack.PullTo(buffer, 1, 2);

    Assert.That(copied, Is.EqualTo(2));
    Assert.That(buffer[1], Is.EqualTo(3));
    Assert.That(buffer[2], Is.EqualTo(2));
  }

  [Test]
  public void PullAll_ReturnsAllTopToBottom() {
    var stack = new ConcurrentStack<string>();
    stack.Push("C");
    stack.Push("B");
    stack.Push("A");

    var result = stack.PullAll();

    Assert.That(result, Is.EqualTo(new[] { "A", "B", "C" }));
    Assert.That(stack.Count, Is.EqualTo(0));
  }

  [Test]
  public void Pull_MaxCount_LimitedToAvailable() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);

    var result = stack.Pull(3);

    Assert.That(result.Length, Is.EqualTo(2));
    Assert.That(result[0], Is.EqualTo(2));
    Assert.That(result[1], Is.EqualTo(1));
  }

  [Test]
  public void Pull_EmptyStack_ReturnsEmpty() {
    var stack = new ConcurrentStack<string>();
    var result = stack.Pull(5);
    Assert.That(result, Is.Empty);
  }

  [Test]
  public void PullTo_ZeroBuffer_DoesNotCrash() {
    var stack = new ConcurrentStack<int>();
    Span<int> empty = [];
    var result = stack.PullTo(empty);
    Assert.That(result.Length, Is.EqualTo(0));
  }
}