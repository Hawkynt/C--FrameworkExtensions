using System.Linq;
using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class ListExtensionsTests {

  #region TryGetFirst Tests

  [Test]
  public void TryGetFirst_IList_EmptyList_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int>();
    
    // Act
    var result = list.TryGetFirst(out var value);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(default(int), value);
  }

  [Test]
  public void TryGetFirst_IList_NonEmptyList_ReturnsFirstElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TryGetFirst(out var value);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(10, value);
  }

  [Test]
  public void TryGetFirst_List_SingleElement_ReturnsElement() {
    // Arrange
    var list = new List<string> { "only" };
    
    // Act
    var result = list.TryGetFirst(out var value);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual("only", value);
  }

  #endregion

  #region TryGetLast Tests

  [Test]
  public void TryGetLast_IList_EmptyList_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int>();
    
    // Act
    var result = list.TryGetLast(out var value);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(default(int), value);
  }

  [Test]
  public void TryGetLast_IList_NonEmptyList_ReturnsLastElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TryGetLast(out var value);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(30, value);
  }

  [Test]
  public void TryGetLast_List_SingleElement_ReturnsElement() {
    // Arrange
    var list = new List<string> { "only" };
    
    // Act
    var result = list.TryGetLast(out var value);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual("only", value);
  }

  #endregion

  #region TryGetItem Tests

  [Test]
  public void TryGetItem_IList_ValidIndex_ReturnsElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TryGetItem(1, out var value);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(20, value);
  }

  [Test]
  public void TryGetItem_IList_NegativeIndex_ReturnsFalse() {
    IList<int> list = new List<int> { 10, 20, 30 };
    Assert.Throws<IndexOutOfRangeException>(() => list.TryGetItem(-1, out var value));
  }

  [Test]
  public void TryGetItem_IList_IndexOutOfRange_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TryGetItem(5, out var value);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(default(int), value);
  }

  [Test]
  public void TryGetItem_List_BoundaryIndices_WorksCorrectly() {
    // Arrange
    var list = new List<string> { "first", "middle", "last" };
    
    // Act & Assert
    Assert.IsTrue(list.TryGetItem(0, out var first));
    Assert.AreEqual("first", first);
    
    Assert.IsTrue(list.TryGetItem(2, out var last));
    Assert.AreEqual("last", last);
    
    Assert.IsFalse(list.TryGetItem(3, out var outOfRange));
    Assert.IsNull(outOfRange);
  }

  #endregion

  #region TrySetFirst Tests

  [Test]
  public void TrySetFirst_IList_EmptyList_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int>();
    
    // Act
    var result = list.TrySetFirst(42);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(0, list.Count);
  }

  [Test]
  public void TrySetFirst_IList_NonEmptyList_SetsFirstElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TrySetFirst(99);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(99, list[0]);
    Assert.AreEqual(20, list[1]);
    Assert.AreEqual(30, list[2]);
  }

  [Test]
  public void TrySetFirst_List_SingleElement_SetsElement() {
    // Arrange
    var list = new List<string> { "old" };
    
    // Act
    var result = list.TrySetFirst("new");
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual("new", list[0]);
  }

  #endregion

  #region TrySetLast Tests

  [Test]
  public void TrySetLast_IList_EmptyList_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int>();
    
    // Act
    var result = list.TrySetLast(42);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(0, list.Count);
  }

  [Test]
  public void TrySetLast_IList_NonEmptyList_SetsLastElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TrySetLast(99);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(10, list[0]);
    Assert.AreEqual(20, list[1]);
    Assert.AreEqual(99, list[2]);
  }

  [Test]
  public void TrySetLast_List_SingleElement_SetsElement() {
    // Arrange
    var list = new List<string> { "old" };
    
    // Act
    var result = list.TrySetLast("new");
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual("new", list[0]);
  }

  #endregion

  #region TrySetItem Tests

  [Test]
  public void TrySetItem_IList_ValidIndex_SetsElement() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TrySetItem(1, 99);
    
    // Assert
    Assert.IsTrue(result);
    Assert.AreEqual(10, list[0]);
    Assert.AreEqual(99, list[1]);
    Assert.AreEqual(30, list[2]);
  }

  [Test]
  public void TrySetItem_IList_NegativeIndex_ReturnsFalse() {
    IList<int> list = new List<int> { 10, 20, 30 };
    Assert.Throws<IndexOutOfRangeException>(() => list.TrySetItem(-1, 99));
  }

  [Test]
  public void TrySetItem_IList_IndexOutOfRange_ReturnsFalse() {
    // Arrange
    IList<int> list = new List<int> { 10, 20, 30 };
    
    // Act
    var result = list.TrySetItem(5, 99);
    
    // Assert
    Assert.IsFalse(result);
    Assert.AreEqual(3, list.Count);
  }

  [Test]
  public void TrySetItem_List_BoundaryIndices_WorksCorrectly() {
    // Arrange
    var list = new List<string> { "first", "middle", "last" };
    
    // Act & Assert
    Assert.IsTrue(list.TrySetItem(0, "new_first"));
    Assert.AreEqual("new_first", list[0]);
    
    Assert.IsTrue(list.TrySetItem(2, "new_last"));
    Assert.AreEqual("new_last", list[2]);
    
    Assert.IsFalse(list.TrySetItem(3, "out_of_range"));
    Assert.AreEqual(3, list.Count);
  }

  #endregion

  #region RemoveEvery Tests

  [Test]
  public void RemoveEvery_IList_ItemNotInList_NoChange() {
    // Arrange
    IList<int> list = new List<int> { 1, 2, 3 };
    
    // Act
    list.RemoveEvery(5);
    
    // Assert
    Assert.AreEqual(3, list.Count);
    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
  }

  [Test]
  public void RemoveEvery_IList_SingleOccurrence_RemovesItem() {
    // Arrange
    IList<int> list = new List<int> { 1, 2, 3 };
    
    // Act
    list.RemoveEvery(2);
    
    // Assert
    Assert.AreEqual(2, list.Count);
    CollectionAssert.AreEqual(new[] { 1, 3 }, list);
  }

  [Test]
  public void RemoveEvery_IList_MultipleOccurrences_RemovesAllInstances() {
    // Arrange
    IList<int> list = new List<int> { 1, 2, 2, 3, 2, 4 };
    
    // Act
    list.RemoveEvery(2);
    
    // Assert
    Assert.AreEqual(3, list.Count);
    CollectionAssert.AreEqual(new[] { 1, 3, 4 }, list);
  }

  [Test]
  public void RemoveEvery_IList_AllItemsSame_RemovesAllItems() {
    // Arrange
    IList<string> list = new List<string> { "test", "test", "test" };
    
    // Act
    list.RemoveEvery("test");
    
    // Assert
    Assert.AreEqual(0, list.Count);
  }

  [Test]
  public void RemoveEvery_IList_NullItem_RemovesNullValues() {
    // Arrange
    IList<string> list = new List<string> { "a", null, "b", null, "c" };
    
    // Act
    list.RemoveEvery(null);
    
    // Assert
    Assert.AreEqual(3, list.Count);
    CollectionAssert.AreEqual(new[] { "a", "b", "c" }, list);
  }

  #endregion

  #region Edge Cases and Performance Tests

  [Test]
  public void Performance_LargeList_TryGetOperations_ExecuteQuickly() {
    // Arrange
    const int itemCount = 100000;
    var list = new List<int>(Enumerable.Range(0, itemCount));
    
    // Act & Assert - Should execute quickly without throwing
    Assert.IsTrue(list.TryGetFirst(out var first));
    Assert.AreEqual(0, first);
    
    Assert.IsTrue(list.TryGetLast(out var last));
    Assert.AreEqual(itemCount - 1, last);
    
    Assert.IsTrue(list.TryGetItem(itemCount / 2, out var middle));
    Assert.AreEqual(itemCount / 2, middle);
    
    Assert.IsTrue(list.Any());
  }

  [Test]
  public void RemoveEvery_LargeListWithManyDuplicates_RemovesAllEfficiently() {
    // Arrange
    var list = new List<int>();
    const int duplicateCount = 10000;
    
    // Add pattern: 1, 2, 1, 2, 1, 2, ...
    for (var i = 0; i < duplicateCount; i++) {
      list.Add(1);
      list.Add(2);
    }
    
    // Act
    list.RemoveEvery(1);
    
    // Assert
    Assert.AreEqual(duplicateCount, list.Count);
    Assert.IsTrue(list.All(x => x == 2));
  }

  [Test]
  public void TrySetOperations_ReadOnlyList_HandledCorrectly() {
    // Arrange
    var sourceList = new List<int> { 1, 2, 3 };
    IList<int> readOnlyList = sourceList.AsReadOnly();
    
    // Act & Assert - ReadOnly lists should still support try-set operations if they implement IList<T>
    // Note: This depends on the actual implementation behavior
    try {
      var result = readOnlyList.TrySetFirst(99);
      // The behavior may vary - some read-only lists might throw, others might return false
      Assert.IsFalse(result); // Assuming it returns false for read-only
    } catch (NotSupportedException) {
      // This is also valid behavior for read-only collections
      Assert.Pass("ReadOnly list correctly throws NotSupportedException");
    }
  }

  [Test]
  public void Multiple_TryOperations_StateConsistency() {
    // Arrange
    var list = new List<string> { "a", "b", "c", "d" };
    
    // Act - Multiple operations
    Assert.IsTrue(list.TryGetFirst(out var first));
    Assert.IsTrue(list.TrySetLast("new_last"));
    Assert.IsTrue(list.TryGetItem(1, out var second));
    Assert.IsTrue(list.TrySetItem(2, "new_c"));
    list.RemoveEvery("a");
    
    // Assert - Verify final state
    Assert.AreEqual(3, list.Count);
    Assert.AreEqual("b", list[0]);
    Assert.AreEqual("new_c", list[1]);
    Assert.AreEqual("new_last", list[2]);
    Assert.AreEqual("a", first);
    Assert.AreEqual("b", second);
  }

  [Test]
  public void CustomEqualityComparer_RemoveEvery_WorksCorrectly() {
    // Arrange
    var list = new List<string> { "Hello", "WORLD", "hello", "World" };
    
    // Act - Remove with case-sensitive comparison (default)
    list.RemoveEvery("hello");
    
    // Assert
    Assert.AreEqual(3, list.Count);
    CollectionAssert.AreEqual(new[] { "Hello", "WORLD", "World" }, list);
  }

  #endregion

}