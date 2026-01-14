#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ImmutableQueue")]
public class ImmutableQueueTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyQueue() {
    var empty = ImmutableQueue<int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptyQueue() {
    var queue = ImmutableQueue.Create<int>();
    Assert.That(queue.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SingleItem_ReturnsQueueWithOneElement() {
    var queue = ImmutableQueue.Create(42);
    Assert.That(queue.IsEmpty, Is.False);
    Assert.That(queue.Peek(), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_MultipleItems_ReturnsQueueWithElements() {
    var queue = ImmutableQueue.CreateRange([1, 2, 3]);
    Assert.That(queue.Peek(), Is.EqualTo(1));
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesQueue() {
    var source = new[] { 1, 2, 3 };
    var queue = ImmutableQueue.CreateRange(source);
    Assert.That(queue.Peek(), Is.EqualTo(1));
  }

  #endregion

  #region Enqueue/Dequeue/Peek

  [Test]
  [Category("HappyPath")]
  public void Enqueue_ReturnsNewQueueWithElement() {
    var queue = ImmutableQueue<int>.Empty;
    var newQueue = queue.Enqueue(42);
    Assert.That(newQueue.IsEmpty, Is.False);
    Assert.That(newQueue.Peek(), Is.EqualTo(42));
    Assert.That(queue.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Enqueue_MultipleTimes_MaintainsFIFOOrder() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
    Assert.That(queue.Peek(), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dequeue_ReturnsNewQueueWithoutFrontElement() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2);
    var newQueue = queue.Dequeue();
    Assert.That(newQueue.Peek(), Is.EqualTo(2));
    Assert.That(queue.Peek(), Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dequeue_WithOutValue_ReturnsValueAndNewQueue() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2);
    var newQueue = queue.Dequeue(out var value);
    Assert.That(value, Is.EqualTo(1));
    Assert.That(newQueue.Peek(), Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void Dequeue_EmptyQueue_ThrowsInvalidOperationException() {
    var queue = ImmutableQueue<int>.Empty;
    Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
  }

  [Test]
  [Category("HappyPath")]
  public void Peek_ReturnsFrontElement() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2);
    Assert.That(queue.Peek(), Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void Peek_EmptyQueue_ThrowsInvalidOperationException() {
    var queue = ImmutableQueue<int>.Empty;
    Assert.Throws<InvalidOperationException>(() => queue.Peek());
  }

  #endregion

  #region Clear

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptyQueue() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
    var newQueue = queue.Clear();
    Assert.That(newQueue.IsEmpty, Is.True);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesFromFrontToBack() {
    var queue = ImmutableQueue<int>.Empty.Enqueue(1).Enqueue(2).Enqueue(3);
    var items = queue.ToArray();
    Assert.That(items, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

}
