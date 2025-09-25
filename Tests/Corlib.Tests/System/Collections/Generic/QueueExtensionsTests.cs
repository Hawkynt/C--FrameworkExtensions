using System.Linq;
using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class QueueExtensionsTests {
  #region PullTo(Span<T>) Tests

  [Test]
  public void PullTo_Span_EmptyQueue_ReturnsEmptySpan() {
    // Arrange
    var queue = new Queue<int>();
    Span<int> buffer = stackalloc int[3];

    // Act
    var result = queue.PullTo(buffer);

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullTo_Span_EmptySpan_ReturnsEmptySpan() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });
    var emptyBuffer = Span<int>.Empty;

    // Act
    var result = queue.PullTo(emptyBuffer);

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(3, queue.Count);
  }

  [Test]
  public void PullTo_Span_QueueHasFewerItemsThanBuffer_ReturnsAllItems() {
    // Arrange
    var queue = new Queue<int>(new[] { 10, 20 });
    Span<int> buffer = stackalloc int[5];

    // Act
    var result = queue.PullTo(buffer);

    // Assert
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual(10, result[0]);
    Assert.AreEqual(20, result[1]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullTo_Span_QueueHasMoreItemsThanBuffer_FillsBuffer() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3, 4, 5 });
    Span<int> buffer = stackalloc int[3];

    // Act
    var result = queue.PullTo(buffer);

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual(1, result[0]);
    Assert.AreEqual(2, result[1]);
    Assert.AreEqual(3, result[2]);
    Assert.AreEqual(2, queue.Count);
  }

  #endregion

  #region PullTo(T[]) Tests

  [Test]
  public void PullTo_Array_EmptyQueue_ReturnsZero() {
    // Arrange
    var queue = new Queue<string>();
    var buffer = new string[3];

    // Act
    var count = queue.PullTo(buffer);

    // Assert
    Assert.AreEqual(0, count);
    Assert.IsNull(buffer[0]);
  }

  [Test]
  public void PullTo_Array_QueueHasItems_FillsArrayAndReturnsCount() {
    // Arrange
    var queue = new Queue<string>(new[] { "A", "B", "C" });
    var buffer = new string[5];

    // Act
    var count = queue.PullTo(buffer);

    // Assert
    Assert.AreEqual(3, count);
    Assert.AreEqual("A", buffer[0]);
    Assert.AreEqual("B", buffer[1]);
    Assert.AreEqual("C", buffer[2]);
    Assert.IsNull(buffer[3]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullTo_Array_NullArray_ThrowsArgumentNullException() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => queue.PullTo((int[])null!));
  }

  #endregion

  #region PullTo(T[], int offset) Tests

  [Test]
  public void PullTo_ArrayWithOffset_ValidOffset_FillsFromOffset() {
    // Arrange
    var queue = new Queue<char>(new[] { 'X', 'Y', 'Z' });
    var buffer = new char[5];

    // Act
    var count = queue.PullTo(buffer, 2);

    // Assert
    Assert.AreEqual(3, count);
    Assert.AreEqual('\0', buffer[0]);
    Assert.AreEqual('\0', buffer[1]);
    Assert.AreEqual('X', buffer[2]);
    Assert.AreEqual('Y', buffer[3]);
    Assert.AreEqual('Z', buffer[4]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffset_OffsetAtEndOfArray_ReturnsZero() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });
    var buffer = new int[3];

    // Act
    var count = queue.PullTo(buffer, 2);

    // Assert
    Assert.AreEqual(1, count);
    Assert.AreEqual(1, buffer[2]);
    Assert.AreEqual(2, queue.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffset_InvalidOffset_ThrowsIndexOutOfRangeException() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });
    var buffer = new int[3];

    // Act & Assert
    Assert.Throws<IndexOutOfRangeException>(() => queue.PullTo(buffer, 5));
  }

  #endregion

  #region PullTo(T[], int offset, int maxCount) Tests

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_ValidParameters_LimitsCount() {
    // Arrange
    var queue = new Queue<double>(new[] { 1.1, 2.2, 3.3, 4.4 });
    var buffer = new double[5];

    // Act
    var count = queue.PullTo(buffer, 1, 2);

    // Assert
    Assert.AreEqual(2, count);
    Assert.AreEqual(0.0, buffer[0]);
    Assert.AreEqual(1.1, buffer[1]);
    Assert.AreEqual(2.2, buffer[2]);
    Assert.AreEqual(0.0, buffer[3]);
    Assert.AreEqual(2, queue.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_MaxCountExceedsQueueSize_ReturnsQueueSize() {
    // Arrange
    var queue = new Queue<int>(new[] { 100, 200 });
    var buffer = new int[5];

    // Act
    var count = queue.PullTo(buffer, 0, 5);

    // Assert
    Assert.AreEqual(2, count);
    Assert.AreEqual(100, buffer[0]);
    Assert.AreEqual(200, buffer[1]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_InvalidMaxCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });
    var buffer = new int[3];

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => queue.PullTo(buffer, 0, 0));
  }

  #endregion

  #region PullAll Tests

  [Test]
  public void PullAll_EmptyQueue_ReturnsEmptyArray() {
    // Arrange
    var queue = new Queue<bool>();

    // Act
    var result = queue.PullAll();

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullAll_QueueWithItems_ReturnsAllItemsInOrder() {
    // Arrange
    var queue = new Queue<string>(new[] { "first", "second", "third" });

    // Act
    var result = queue.PullAll();

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual("first", result[0]);
    Assert.AreEqual("second", result[1]);
    Assert.AreEqual("third", result[2]);
    Assert.AreEqual(0, queue.Count);
  }

  #endregion

  #region Pull(int maxCount) Tests

  [Test]
  public void Pull_WithMaxCount_LimitsToPulledItems() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3, 4, 5 });

    // Act
    var result = queue.Pull(3);

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual(1, result[0]);
    Assert.AreEqual(2, result[1]);
    Assert.AreEqual(3, result[2]);
    Assert.AreEqual(2, queue.Count);
  }

  [Test]
  public void Pull_MaxCountExceedsQueueSize_ReturnsAllItems() {
    // Arrange
    var queue = new Queue<char>(new[] { 'A', 'B' });

    // Act
    var result = queue.Pull(10);

    // Assert
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual('A', result[0]);
    Assert.AreEqual('B', result[1]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void Pull_InvalidMaxCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var queue = new Queue<int>(new[] { 1, 2, 3 });

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => queue.Pull(-1));
  }

  #endregion

  #region AddRange Tests

  [Test]
  public void AddRange_EmptyEnumerable_NoItemsAdded() {
    // Arrange
    var queue = new Queue<int>();
    var items = Enumerable.Empty<int>();

    // Act
    queue.AddRange(items);

    // Assert
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void AddRange_ValidEnumerable_AddsAllItems() {
    // Arrange
    var queue = new Queue<string>();
    var items = new[] { "apple", "banana", "cherry" };

    // Act
    queue.AddRange(items);

    // Assert
    Assert.AreEqual(3, queue.Count);
    Assert.AreEqual("apple", queue.Dequeue());
    Assert.AreEqual("banana", queue.Dequeue());
    Assert.AreEqual("cherry", queue.Dequeue());
  }

  [Test]
  public void AddRange_NullEnumerable_ThrowsArgumentNullException() {
    // Arrange
    var queue = new Queue<int>();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => queue.AddRange(null));
  }

  #endregion

  #region Add Tests

  [Test]
  public void Add_ValidItem_AddsToQueue() {
    // Arrange
    var queue = new Queue<int>();

    // Act
    queue.Add(42);

    // Assert
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(42, queue.Peek());
  }

  [Test]
  public void Add_MultipleItems_MaintainsOrder() {
    // Arrange
    var queue = new Queue<string>();

    // Act
    queue.Add("first");
    queue.Add("second");

    // Assert
    Assert.AreEqual(2, queue.Count);
    Assert.AreEqual("first", queue.Dequeue());
    Assert.AreEqual("second", queue.Dequeue());
  }

  #endregion

  #region Fetch Tests

  [Test]
  public void Fetch_QueueHasItems_ReturnsFirstItem() {
    // Arrange
    var queue = new Queue<int>(new[] { 10, 20, 30 });

    // Act
    var result = queue.Fetch();

    // Assert
    Assert.AreEqual(10, result);
    Assert.AreEqual(2, queue.Count);
    Assert.AreEqual(20, queue.Peek());
  }

  [Test]
  public void Fetch_EmptyQueue_ThrowsInvalidOperationException() {
    // Arrange
    var queue = new Queue<string>();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => queue.Fetch());
  }

  #endregion

  #region TryDequeue Tests

  [Test]
  public void TryDequeue_QueueHasItems_ReturnsTrueAndItem() {
    // Arrange
    var queue = new Queue<int>(new[] { 42, 84 });

    // Act
    var success = queue.TryDequeue(out var result);

    // Assert
    Assert.IsTrue(success);
    Assert.AreEqual(42, result);
    Assert.AreEqual(1, queue.Count);
  }

  [Test]
  public void TryDequeue_EmptyQueue_ReturnsFalseAndDefault() {
    // Arrange
    var queue = new Queue<string>();

    // Act
    var success = queue.TryDequeue(out var result);

    // Assert
    Assert.IsFalse(success);
    Assert.IsNull(result);
    Assert.AreEqual(0, queue.Count);
  }

  #endregion

  #region Performance and Edge Case Tests

  [Test]
  public void Performance_LargeQueue_PullOperations_ExecuteQuickly() {
    // Arrange
    const int itemCount = 50000;
    var queue = new Queue<int>(Enumerable.Range(0, itemCount));

    // Act & Assert - Should execute quickly without throwing
    var allItems = queue.PullAll();
    Assert.AreEqual(itemCount, allItems.Length);
    Assert.AreEqual(0, allItems[0]);
    Assert.AreEqual(itemCount - 1, allItems[itemCount - 1]);
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void PullOperations_MaintainFIFOOrder() {
    // Arrange
    var queue = new Queue<string>();
    var testItems = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };

    foreach (var item in testItems)
      queue.Enqueue(item);

    // Act
    var firstTwo = queue.Pull(2);
    var remaining = queue.PullAll();

    // Assert
    Assert.AreEqual("Alpha", firstTwo[0]);
    Assert.AreEqual("Beta", firstTwo[1]);
    Assert.AreEqual("Gamma", remaining[0]);
    Assert.AreEqual("Delta", remaining[1]);
    Assert.AreEqual("Epsilon", remaining[2]);
  }

  [Test]
  public void StateConsistency_AfterMultipleOperations() {
    // Arrange
    var queue = new Queue<int>();

    // Act - Mixed operations
    queue.AddRange(new[] { 1, 2, 3 });
    queue.Add(4);
    var firstItem = queue.Fetch();
    queue.TryDequeue(out var secondItem);
    var twoItems = queue.Pull(2);
    queue.Enqueue(5);

    // Assert
    Assert.AreEqual(1, firstItem);
    Assert.AreEqual(2, secondItem);
    Assert.AreEqual(2, twoItems.Length);
    Assert.AreEqual(3, twoItems[0]);
    Assert.AreEqual(4, twoItems[1]);
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(5, queue.Peek());
  }

  [Test]
  public void NullQueueOperations_ThrowsNullReferenceException() {
    // Arrange
    Queue<int>? queue = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => queue.Add(1));
    Assert.Throws<NullReferenceException>(() => queue.Fetch());
    Assert.Throws<NullReferenceException>(() => queue.TryDequeue(out _));
    Assert.Throws<NullReferenceException>(() => queue.PullAll());
  }

  [Test]
  public void AddRange_LargeCollection_HandlesEfficiently() {
    // Arrange
    var queue = new Queue<int>();
    var largeCollection = Enumerable.Range(0, 10000);

    // Act
    queue.AddRange(largeCollection);

    // Assert
    Assert.AreEqual(10000, queue.Count);
    Assert.AreEqual(0, queue.Peek());

    // Verify order is preserved
    for (var i = 0; i < 100; i++)
      Assert.AreEqual(i, queue.Dequeue());
  }

  #endregion
}
