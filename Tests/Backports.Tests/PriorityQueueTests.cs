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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("PriorityQueue")]
public class PriorityQueueTests {

  // Helper comparer for tests - Comparer<T>.Create is not available in .NET 4.0
  private sealed class ReverseIntComparer : IComparer<int> {
    public static readonly ReverseIntComparer Instance = new();
    public int Compare(int x, int y) => y.CompareTo(x);
  }

  #region Constructors

  [Test]
  [Category("HappyPath")]
  public void Constructor_Default_CreatesEmptyQueue() {
    var queue = new PriorityQueue<string, int>();
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithComparer_UsesComparer() {
    var queue = new PriorityQueue<string, int>(ReverseIntComparer.Instance);
    Assert.That(queue.Comparer, Is.SameAs(ReverseIntComparer.Instance));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithInitialCapacity_CreatesEmptyQueue() {
    var queue = new PriorityQueue<string, int>(10);
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new PriorityQueue<string, int>(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromItems_PopulatesQueue() {
    var items = new[] { ("a", 3), ("b", 1), ("c", 2) };
    var queue = new PriorityQueue<string, int>(items);
    Assert.That(queue.Count, Is.EqualTo(3));
    Assert.That(queue.Dequeue(), Is.EqualTo("b")); // priority 1 is lowest
  }

  [Test]
  [Category("Exception")]
  public void Constructor_NullItems_ThrowsArgumentNullException() {
    IEnumerable<(string, int)> items = null!;
    Assert.Throws<ArgumentNullException>(() => new PriorityQueue<string, int>(items));
  }

  #endregion

  #region Enqueue and Dequeue

  [Test]
  [Category("HappyPath")]
  public void Enqueue_SingleItem_IncreasesCount() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("item", 1);
    Assert.That(queue.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dequeue_SingleItem_ReturnsAndRemovesItem() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("item", 1);
    var result = queue.Dequeue();
    Assert.That(result, Is.EqualTo("item"));
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Dequeue_ReturnsLowestPriorityFirst() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("high", 100);
    queue.Enqueue("low", 1);
    queue.Enqueue("medium", 50);
    Assert.That(queue.Dequeue(), Is.EqualTo("low"));
    Assert.That(queue.Dequeue(), Is.EqualTo("medium"));
    Assert.That(queue.Dequeue(), Is.EqualTo("high"));
  }

  [Test]
  [Category("Exception")]
  public void Dequeue_EmptyQueue_ThrowsInvalidOperationException() {
    var queue = new PriorityQueue<string, int>();
    Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
  }

  [Test]
  [Category("HappyPath")]
  public void Dequeue_WithCustomComparer_UsesComparer() {
    var queue = new PriorityQueue<string, int>(ReverseIntComparer.Instance); // Max-heap
    queue.Enqueue("high", 100);
    queue.Enqueue("low", 1);
    Assert.That(queue.Dequeue(), Is.EqualTo("high")); // Now highest priority first
  }

  #endregion

  #region Peek

  [Test]
  [Category("HappyPath")]
  public void Peek_ReturnsLowestPriorityWithoutRemoving() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("high", 100);
    queue.Enqueue("low", 1);
    Assert.That(queue.Peek(), Is.EqualTo("low"));
    Assert.That(queue.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void Peek_EmptyQueue_ThrowsInvalidOperationException() {
    var queue = new PriorityQueue<string, int>();
    Assert.Throws<InvalidOperationException>(() => queue.Peek());
  }

  #endregion

  #region TryDequeue and TryPeek

  [Test]
  [Category("HappyPath")]
  public void TryDequeue_HasItems_ReturnsTrueAndElement() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("item", 5);
    var result = queue.TryDequeue(out var element, out var priority);
    Assert.That(result, Is.True);
    Assert.That(element, Is.EqualTo("item"));
    Assert.That(priority, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void TryDequeue_EmptyQueue_ReturnsFalse() {
    var queue = new PriorityQueue<string, int>();
    var result = queue.TryDequeue(out _, out _);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TryPeek_HasItems_ReturnsTrueAndElement() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("item", 5);
    var result = queue.TryPeek(out var element, out var priority);
    Assert.That(result, Is.True);
    Assert.That(element, Is.EqualTo("item"));
    Assert.That(priority, Is.EqualTo(5));
    Assert.That(queue.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void TryPeek_EmptyQueue_ReturnsFalse() {
    var queue = new PriorityQueue<string, int>();
    var result = queue.TryPeek(out _, out _);
    Assert.That(result, Is.False);
  }

  #endregion

  #region EnqueueDequeue

  [Test]
  [Category("HappyPath")]
  public void EnqueueDequeue_NewItemHasLowerPriority_ReturnsNewItem() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("existing", 10);
    var result = queue.EnqueueDequeue("new", 5);
    Assert.That(result, Is.EqualTo("new")); // New item has lower priority, returned immediately
    Assert.That(queue.Peek(), Is.EqualTo("existing"));
  }

  [Test]
  [Category("HappyPath")]
  public void EnqueueDequeue_NewItemHasHigherPriority_ReturnsExisting() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("existing", 5);
    var result = queue.EnqueueDequeue("new", 10);
    Assert.That(result, Is.EqualTo("existing")); // Existing has lower priority, returned
    Assert.That(queue.Peek(), Is.EqualTo("new"));
  }

  [Test]
  [Category("EdgeCase")]
  public void EnqueueDequeue_EmptyQueue_ReturnsNewItem() {
    var queue = new PriorityQueue<string, int>();
    var result = queue.EnqueueDequeue("item", 5);
    Assert.That(result, Is.EqualTo("item"));
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  #endregion

  #region EnqueueRange

  [Test]
  [Category("HappyPath")]
  public void EnqueueRange_WithPairs_AddsAllItems() {
    var queue = new PriorityQueue<string, int>();
    queue.EnqueueRange(new[] { ("a", 1), ("b", 2), ("c", 3) });
    Assert.That(queue.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void EnqueueRange_WithSharedPriority_AddsAllItemsWithSamePriority() {
    var queue = new PriorityQueue<string, int>();
    queue.EnqueueRange(new[] { "a", "b", "c" }, 5);
    Assert.That(queue.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void EnqueueRange_NullPairs_ThrowsArgumentNullException() {
    var queue = new PriorityQueue<string, int>();
    IEnumerable<(string, int)> items = null!;
    Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(items));
  }

  [Test]
  [Category("Exception")]
  public void EnqueueRange_NullElements_ThrowsArgumentNullException() {
    var queue = new PriorityQueue<string, int>();
    IEnumerable<string> elements = null!;
    Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(elements, 5));
  }

  #endregion

  #region Clear

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllItems() {
    var queue = new PriorityQueue<string, int>();
    queue.EnqueueRange(new[] { ("a", 1), ("b", 2), ("c", 3) });
    queue.Clear();
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clear_EmptyQueue_RemainsEmpty() {
    var queue = new PriorityQueue<string, int>();
    queue.Clear();
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  #endregion

  #region EnsureCapacity and TrimExcess

  [Test]
  [Category("HappyPath")]
  public void EnsureCapacity_ReturnsAtLeastRequestedCapacity() {
    var queue = new PriorityQueue<string, int>();
    var result = queue.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("Exception")]
  public void EnsureCapacity_NegativeCapacity_ThrowsArgumentOutOfRangeException() {
    var queue = new PriorityQueue<string, int>();
    Assert.Throws<ArgumentOutOfRangeException>(() => queue.EnsureCapacity(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void TrimExcess_ReducesCapacity() {
    var queue = new PriorityQueue<string, int>(100);
    queue.Enqueue("item", 1);
    var capacityBefore = queue.EnsureCapacity(0);
    queue.TrimExcess();
    var capacityAfter = queue.EnsureCapacity(0);
    Assert.That(capacityAfter, Is.LessThanOrEqualTo(capacityBefore));
  }

  #endregion

  #region UnorderedItems

  [Test]
  [Category("HappyPath")]
  public void UnorderedItems_EnumeratesAllItems() {
    var queue = new PriorityQueue<string, int>();
    queue.EnqueueRange(new[] { ("a", 1), ("b", 2), ("c", 3) });
    var items = queue.UnorderedItems.ToList();
    Assert.That(items.Count, Is.EqualTo(3));
    Assert.That(items.Select(x => x.Element), Does.Contain("a"));
    Assert.That(items.Select(x => x.Element), Does.Contain("b"));
    Assert.That(items.Select(x => x.Element), Does.Contain("c"));
  }

  [Test]
  [Category("HappyPath")]
  public void UnorderedItems_Count_ReturnsCorrectCount() {
    var queue = new PriorityQueue<string, int>();
    queue.EnqueueRange(new[] { ("a", 1), ("b", 2) });
    Assert.That(queue.UnorderedItems.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void UnorderedItems_EmptyQueue_ReturnsEmptyCollection() {
    var queue = new PriorityQueue<string, int>();
    var items = queue.UnorderedItems.ToList();
    Assert.That(items, Is.Empty);
  }

  #endregion

  #region Priority ordering

  [Test]
  [Category("HappyPath")]
  public void PriorityOrder_IntPriority_MinHeapBehavior() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("five", 5);
    queue.Enqueue("one", 1);
    queue.Enqueue("three", 3);
    queue.Enqueue("two", 2);
    queue.Enqueue("four", 4);
    Assert.That(queue.Dequeue(), Is.EqualTo("one"));
    Assert.That(queue.Dequeue(), Is.EqualTo("two"));
    Assert.That(queue.Dequeue(), Is.EqualTo("three"));
    Assert.That(queue.Dequeue(), Is.EqualTo("four"));
    Assert.That(queue.Dequeue(), Is.EqualTo("five"));
  }

  [Test]
  [Category("HappyPath")]
  public void PriorityOrder_StringPriority_AlphabeticalOrder() {
    var queue = new PriorityQueue<int, string>();
    queue.Enqueue(3, "cherry");
    queue.Enqueue(1, "apple");
    queue.Enqueue(2, "banana");
    Assert.That(queue.Dequeue(), Is.EqualTo(1)); // "apple"
    Assert.That(queue.Dequeue(), Is.EqualTo(2)); // "banana"
    Assert.That(queue.Dequeue(), Is.EqualTo(3)); // "cherry"
  }

  [Test]
  [Category("HappyPath")]
  public void PriorityOrder_SamePriority_AllDequeued() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("a", 1);
    queue.Enqueue("b", 1);
    queue.Enqueue("c", 1);
    var dequeued = new List<string>();
    while (queue.Count > 0)
      dequeued.Add(queue.Dequeue());
    Assert.That(dequeued, Has.Count.EqualTo(3));
    Assert.That(dequeued, Does.Contain("a"));
    Assert.That(dequeued, Does.Contain("b"));
    Assert.That(dequeued, Does.Contain("c"));
  }

  #endregion

  #region Count

  [Test]
  [Category("HappyPath")]
  public void Count_EmptyQueue_ReturnsZero() {
    var queue = new PriorityQueue<string, int>();
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Count_AfterEnqueue_IncreasesCount() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("a", 1);
    queue.Enqueue("b", 2);
    Assert.That(queue.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Count_AfterDequeue_DecreasesCount() {
    var queue = new PriorityQueue<string, int>();
    queue.Enqueue("a", 1);
    queue.Enqueue("b", 2);
    queue.Dequeue();
    Assert.That(queue.Count, Is.EqualTo(1));
  }

  #endregion

  #region Comparer property

  [Test]
  [Category("HappyPath")]
  public void Comparer_DefaultConstructor_ReturnsDefaultComparer() {
    var queue = new PriorityQueue<string, int>();
    Assert.That(queue.Comparer, Is.EqualTo(Comparer<int>.Default));
  }

  [Test]
  [Category("HappyPath")]
  public void Comparer_CustomComparer_ReturnsCustomComparer() {
    var queue = new PriorityQueue<string, int>(ReverseIntComparer.Instance);
    Assert.That(queue.Comparer, Is.SameAs(ReverseIntComparer.Instance));
  }

  #endregion

}
