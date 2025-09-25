using System.Linq;
using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class StackExtensionsTests {
  #region PullTo(Span<T>) Tests

  [Test]
  public void PullTo_Span_EmptyStack_ReturnsEmptySpan() {
    // Arrange
    var stack = new Stack<int>();
    Span<int> buffer = stackalloc int[3];

    // Act
    var result = stack.PullTo(buffer);

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullTo_Span_EmptySpan_ReturnsEmptySpan() {
    // Arrange  
    var stack = new Stack<int>(new[] { 1, 2, 3 });
    var emptyBuffer = Span<int>.Empty;

    // Act
    var result = stack.PullTo(emptyBuffer);

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(3, stack.Count);
  }

  [Test]
  public void PullTo_Span_StackHasFewerItemsThanBuffer_ReturnsAllItems() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(10);
    stack.Push(20);
    Span<int> buffer = stackalloc int[5];

    // Act
    var result = stack.PullTo(buffer);

    // Assert
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual(20, result[0]); // LIFO order
    Assert.AreEqual(10, result[1]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullTo_Span_StackHasMoreItemsThanBuffer_FillsBuffer() {
    // Arrange
    var stack = new Stack<int>();
    for (var i = 1; i <= 5; i++)
      stack.Push(i);

    Span<int> buffer = stackalloc int[3];

    // Act
    var result = stack.PullTo(buffer);

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual(5, result[0]); // LIFO order
    Assert.AreEqual(4, result[1]);
    Assert.AreEqual(3, result[2]);
    Assert.AreEqual(2, stack.Count);
  }

  #endregion

  #region PullTo(T[]) Tests

  [Test]
  public void PullTo_Array_EmptyStack_ReturnsZero() {
    // Arrange
    var stack = new Stack<string>();
    var buffer = new string[3];

    // Act
    var count = stack.PullTo(buffer);

    // Assert
    Assert.AreEqual(0, count);
    Assert.IsNull(buffer[0]);
  }

  [Test]
  public void PullTo_Array_StackHasItems_FillsArrayAndReturnsCount() {
    // Arrange
    var stack = new Stack<string>();
    stack.Push("A");
    stack.Push("B");
    stack.Push("C");
    var buffer = new string[5];

    // Act
    var count = stack.PullTo(buffer);

    // Assert
    Assert.AreEqual(3, count);
    Assert.AreEqual("C", buffer[0]); // LIFO order
    Assert.AreEqual("B", buffer[1]);
    Assert.AreEqual("A", buffer[2]);
    Assert.IsNull(buffer[3]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullTo_Array_NullArray_ThrowsArgumentNullException() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => stack.PullTo((int[])null!));
  }

  #endregion

  #region PullTo(T[], int offset) Tests

  [Test]
  public void PullTo_ArrayWithOffset_ValidOffset_FillsFromOffset() {
    // Arrange
    var stack = new Stack<char>();
    stack.Push('X');
    stack.Push('Y');
    stack.Push('Z');
    var buffer = new char[5];

    // Act
    var count = stack.PullTo(buffer, 2);

    // Assert
    Assert.AreEqual(3, count);
    Assert.AreEqual('\0', buffer[0]);
    Assert.AreEqual('\0', buffer[1]);
    Assert.AreEqual('Z', buffer[2]); // LIFO order
    Assert.AreEqual('Y', buffer[3]);
    Assert.AreEqual('X', buffer[4]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffset_OffsetAtEndOfArray_FillsRemainingSpace() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);
    var buffer = new int[3];

    // Act
    var count = stack.PullTo(buffer, 2);

    // Assert
    Assert.AreEqual(1, count);
    Assert.AreEqual(3, buffer[2]); // Only room for one item
    Assert.AreEqual(2, stack.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffset_InvalidOffset_ThrowsIndexOutOfRangeException() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);
    var buffer = new int[3];

    // Act & Assert
    Assert.Throws<IndexOutOfRangeException>(() => stack.PullTo(buffer, 5));
  }

  #endregion

  #region PullTo(T[], int offset, int maxCount) Tests

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_ValidParameters_LimitsCount() {
    // Arrange
    var stack = new Stack<double>();
    stack.Push(1.1);
    stack.Push(2.2);
    stack.Push(3.3);
    stack.Push(4.4);
    var buffer = new double[5];

    // Act
    var count = stack.PullTo(buffer, 1, 2);

    // Assert
    Assert.AreEqual(2, count);
    Assert.AreEqual(0.0, buffer[0]);
    Assert.AreEqual(4.4, buffer[1]); // LIFO order
    Assert.AreEqual(3.3, buffer[2]);
    Assert.AreEqual(0.0, buffer[3]);
    Assert.AreEqual(2, stack.Count); // 2.2 and 1.1 remain
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_MaxCountExceedsStackSize_ReturnsStackSize() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(100);
    stack.Push(200);
    var buffer = new int[5];

    // Act
    var count = stack.PullTo(buffer, 0, 5);

    // Assert
    Assert.AreEqual(2, count);
    Assert.AreEqual(200, buffer[0]); // LIFO order
    Assert.AreEqual(100, buffer[1]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullTo_ArrayWithOffsetAndMaxCount_InvalidMaxCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);
    var buffer = new int[3];

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => stack.PullTo(buffer, 0, 0));
  }

  #endregion

  #region PullAll Tests

  [Test]
  public void PullAll_EmptyStack_ReturnsEmptyArray() {
    // Arrange
    var stack = new Stack<bool>();

    // Act
    var result = stack.PullAll();

    // Assert
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullAll_StackWithItems_ReturnsAllItemsInLIFOOrder() {
    // Arrange
    var stack = new Stack<string>();
    stack.Push("first");
    stack.Push("second");
    stack.Push("third");

    // Act
    var result = stack.PullAll();

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual("third", result[0]); // LIFO order
    Assert.AreEqual("second", result[1]);
    Assert.AreEqual("first", result[2]);
    Assert.AreEqual(0, stack.Count);
  }

  #endregion

  #region Pull(int maxCount) Tests

  [Test]
  public void Pull_WithMaxCount_LimitsToPulledItems() {
    // Arrange
    var stack = new Stack<int>();
    for (var i = 1; i <= 5; i++)
      stack.Push(i);

    // Act
    var result = stack.Pull(3);

    // Assert
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual(5, result[0]); // LIFO order
    Assert.AreEqual(4, result[1]);
    Assert.AreEqual(3, result[2]);
    Assert.AreEqual(2, stack.Count);
  }

  [Test]
  public void Pull_MaxCountExceedsStackSize_ReturnsAllItems() {
    // Arrange
    var stack = new Stack<char>();
    stack.Push('A');
    stack.Push('B');

    // Act
    var result = stack.Pull(10);

    // Assert
    Assert.AreEqual(2, result.Length);
    Assert.AreEqual('B', result[0]); // LIFO order
    Assert.AreEqual('A', result[1]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void Pull_InvalidMaxCount_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => stack.Pull(-1));
  }

  #endregion

  #region Exchange Tests

  [Test]
  public void Exchange_StackHasItems_ReplacesTopAndReturnsOriginal() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    // Act
    var originalTop = stack.Exchange(99);

    // Assert
    Assert.AreEqual(3, originalTop);
    Assert.AreEqual(99, stack.Peek());
    Assert.AreEqual(3, stack.Count);
  }

  [Test]
  public void Exchange_EmptyStack_ThrowsInvalidOperationException() {
    // Arrange
    var stack = new Stack<string>();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => stack.Exchange("test"));
  }

  #endregion

  #region Invert Tests

  [Test]
  public void Invert_StackWithItems_ReversesOrder() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    // Act
    stack.Invert();

    // Assert
    Assert.AreEqual(3, stack.Count);
    Assert.AreEqual(1, stack.Pop()); // Should be 1 now (was at bottom)
    Assert.AreEqual(2, stack.Pop());
    Assert.AreEqual(3, stack.Pop()); // Should be 3 now (was at top)
  }

  [Test]
  public void Invert_EmptyStack_NoEffect() {
    // Arrange
    var stack = new Stack<string>();

    // Act
    stack.Invert();

    // Assert
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void Invert_SingleItem_NoChange() {
    // Arrange
    var stack = new Stack<char>();
    stack.Push('X');

    // Act
    stack.Invert();

    // Assert
    Assert.AreEqual(1, stack.Count);
    Assert.AreEqual('X', stack.Peek());
  }

  #endregion

  #region AddRange Tests

  [Test]
  public void AddRange_EmptyEnumerable_NoItemsAdded() {
    // Arrange
    var stack = new Stack<int>();
    var items = Enumerable.Empty<int>();

    // Act
    stack.AddRange(items);

    // Assert
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void AddRange_ValidEnumerable_AddsAllItems() {
    // Arrange
    var stack = new Stack<string>();
    var items = new[] { "apple", "banana", "cherry" };

    // Act
    stack.AddRange(items);

    // Assert
    Assert.AreEqual(3, stack.Count);
    Assert.AreEqual("cherry", stack.Pop()); // Last added is on top
    Assert.AreEqual("banana", stack.Pop());
    Assert.AreEqual("apple", stack.Pop());
  }

  [Test]
  public void AddRange_NullEnumerable_ThrowsArgumentNullException() {
    // Arrange
    var stack = new Stack<int>();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => stack.AddRange(null));
  }

  #endregion

  #region Add Tests

  [Test]
  public void Add_ValidItem_AddsToStack() {
    // Arrange
    var stack = new Stack<int>();

    // Act
    stack.Add(42);

    // Assert
    Assert.AreEqual(1, stack.Count);
    Assert.AreEqual(42, stack.Peek());
  }

  [Test]
  public void Add_MultipleItems_MaintainsLIFOOrder() {
    // Arrange
    var stack = new Stack<string>();

    // Act
    stack.Add("first");
    stack.Add("second");

    // Assert
    Assert.AreEqual(2, stack.Count);
    Assert.AreEqual("second", stack.Pop());
    Assert.AreEqual("first", stack.Pop());
  }

  #endregion

  #region Fetch Tests

  [Test]
  public void Fetch_StackHasItems_ReturnsTopItem() {
    // Arrange
    var stack = new Stack<int>();
    stack.Push(10);
    stack.Push(20);
    stack.Push(30);

    // Act
    var result = stack.Fetch();

    // Assert
    Assert.AreEqual(30, result);
    Assert.AreEqual(2, stack.Count);
    Assert.AreEqual(20, stack.Peek());
  }

  [Test]
  public void Fetch_EmptyStack_ThrowsInvalidOperationException() {
    // Arrange
    var stack = new Stack<string>();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => stack.Fetch());
  }

  #endregion

  #region Performance and Edge Case Tests

  [Test]
  public void Performance_LargeStack_PullOperations_ExecuteQuickly() {
    // Arrange
    const int itemCount = 50000;
    var stack = new Stack<int>(Enumerable.Range(0, itemCount).Reverse());

    // Act & Assert - Should execute quickly without throwing
    var allItems = stack.PullAll();
    Assert.AreEqual(itemCount, allItems.Length);
    Assert.AreEqual(0, allItems[0]); // Top item in LIFO
    Assert.AreEqual(itemCount - 1, allItems[itemCount - 1]);
    Assert.AreEqual(0, stack.Count);
  }

  [Test]
  public void PullOperations_MaintainLIFOOrder() {
    // Arrange
    var stack = new Stack<string>();
    var testItems = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };

    foreach (var item in testItems)
      stack.Push(item);

    // Act
    var firstTwo = stack.Pull(2);
    var remaining = stack.PullAll();

    // Assert - LIFO order
    Assert.AreEqual("Epsilon", firstTwo[0]);
    Assert.AreEqual("Delta", firstTwo[1]);
    Assert.AreEqual("Gamma", remaining[0]);
    Assert.AreEqual("Beta", remaining[1]);
    Assert.AreEqual("Alpha", remaining[2]);
  }

  [Test]
  public void StateConsistency_AfterMultipleOperations() {
    // Arrange
    var stack = new Stack<int>();

    // Act - Mixed operations
    stack.AddRange(new[] { 1, 2, 3 }); // 3 is on top
    stack.Add(4); // 4 is now on top
    var topItem = stack.Fetch(); // Remove 4
    var exchangedItem = stack.Exchange(99); // Replace 3 with 99
    var twoItems = stack.Pull(2); // Pull 99 and 2
    stack.Push(5);

    // Assert
    Assert.AreEqual(4, topItem);
    Assert.AreEqual(3, exchangedItem);
    Assert.AreEqual(2, twoItems.Length);
    Assert.AreEqual(99, twoItems[0]);
    Assert.AreEqual(2, twoItems[1]);
    Assert.AreEqual(2, stack.Count); // 5 and 1 remain
    Assert.AreEqual(5, stack.Peek());
  }

  [Test]
  public void NullStackOperations_ThrowsNullReferenceException() {
    // Arrange
    Stack<int>? stack = null;

    // Act & Assert
    Assert.Throws<NullReferenceException>(() => stack.Add(1));
    Assert.Throws<NullReferenceException>(() => stack.Fetch());
    Assert.Throws<NullReferenceException>(() => stack.Exchange(1));
    Assert.Throws<NullReferenceException>(() => stack.PullAll());
  }

  [Test]
  public void AddRange_LargeCollection_HandlesEfficiently() {
    // Arrange
    var stack = new Stack<int>();
    var largeCollection = Enumerable.Range(0, 10000);

    // Act
    stack.AddRange(largeCollection);

    // Assert
    Assert.AreEqual(10000, stack.Count);
    Assert.AreEqual(9999, stack.Peek()); // Last added item is on top

    // Verify LIFO order is preserved
    for (var i = 9999; i >= 9900; i--)
      Assert.AreEqual(i, stack.Pop());
  }

  [Test]
  public void Invert_LargeStack_HandlesEfficiently() {
    // Arrange
    var stack = new Stack<int>();
    const int itemCount = 5000;

    for (var i = 0; i < itemCount; i++)
      stack.Push(i);

    // Act
    stack.Invert();

    // Assert
    Assert.AreEqual(itemCount, stack.Count);
    Assert.AreEqual(0, stack.Peek()); // 0 should now be on top

    // Verify order is inverted
    for (var i = 0; i < 100; i++)
      Assert.AreEqual(i, stack.Pop());
  }

  #endregion
}
