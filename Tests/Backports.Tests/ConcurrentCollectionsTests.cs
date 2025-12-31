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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ConcurrentCollections")]
public class ConcurrentCollectionsTests {

  #region ConcurrentBag<T> Tests

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_Add_AddsItems() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);
    bag.Add(3);

    Assert.That(bag.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_TryTake_ReturnsItem() {
    var bag = new ConcurrentBag<int>();
    bag.Add(42);

    var success = bag.TryTake(out var item);

    Assert.That(success, Is.True);
    Assert.That(item, Is.EqualTo(42));
    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ConcurrentBag_TryTake_EmptyBag_ReturnsFalse() {
    var bag = new ConcurrentBag<int>();

    var success = bag.TryTake(out var item);

    Assert.That(success, Is.False);
    Assert.That(item, Is.EqualTo(default(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_IsEmpty_EmptyBag_ReturnsTrue() {
    var bag = new ConcurrentBag<string>();

    Assert.That(bag.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_IsEmpty_NonEmptyBag_ReturnsFalse() {
    var bag = new ConcurrentBag<string>();
    bag.Add("test");

    Assert.That(bag.IsEmpty, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_Contains_ExistingItem_ReturnsTrue() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);
    bag.Add(3);

    Assert.That(bag.Contains(2), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_Contains_NonExistingItem_ReturnsFalse() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);

    Assert.That(bag.Contains(99), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_Clear_RemovesAllItems() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);
    bag.Add(3);

    bag.Clear();

    Assert.That(bag.IsEmpty, Is.True);
    Assert.That(bag.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_ToArray_ReturnsAllItems() {
    var bag = new ConcurrentBag<int>();
    bag.Add(1);
    bag.Add(2);
    bag.Add(3);

    var array = bag.ToArray();

    Assert.That(array.Length, Is.EqualTo(3));
    Assert.That(array, Does.Contain(1));
    Assert.That(array, Does.Contain(2));
    Assert.That(array, Does.Contain(3));
  }

  #endregion

  #region ConcurrentDictionary<TKey, TValue> Tests

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_TryAdd_NewKey_ReturnsTrue() {
    var dict = new ConcurrentDictionary<string, int>();

    var success = dict.TryAdd("key1", 100);

    Assert.That(success, Is.True);
    Assert.That(dict["key1"], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_TryAdd_ExistingKey_ReturnsFalse() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 100);

    var success = dict.TryAdd("key1", 200);

    Assert.That(success, Is.False);
    Assert.That(dict["key1"], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_GetOrAdd_NewKey_AddsValue() {
    var dict = new ConcurrentDictionary<string, int>();

    var value = dict.GetOrAdd("key1", 42);

    Assert.That(value, Is.EqualTo(42));
    Assert.That(dict["key1"], Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_GetOrAdd_ExistingKey_ReturnsExisting() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 100);

    var value = dict.GetOrAdd("key1", 42);

    Assert.That(value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_GetOrAdd_WithFactory_NewKey_CallsFactory() {
    var dict = new ConcurrentDictionary<string, int>();
    var factoryCalled = false;

    var value = dict.GetOrAdd("key1", _ => {
      factoryCalled = true;
      return 42;
    });

    Assert.That(factoryCalled, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_GetOrAdd_WithFactory_ExistingKey_DoesNotCallFactory() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 100);
    var factoryCalled = false;

    var value = dict.GetOrAdd("key1", _ => {
      factoryCalled = true;
      return 42;
    });

    Assert.That(factoryCalled, Is.False);
    Assert.That(value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_AddOrUpdate_NewKey_AddsValue() {
    var dict = new ConcurrentDictionary<string, int>();

    var value = dict.AddOrUpdate("key1", 10, (_, old) => old + 1);

    Assert.That(value, Is.EqualTo(10));
    Assert.That(dict["key1"], Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_AddOrUpdate_ExistingKey_UpdatesValue() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 10);

    var value = dict.AddOrUpdate("key1", 100, (_, old) => old + 5);

    Assert.That(value, Is.EqualTo(15));
    Assert.That(dict["key1"], Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_AddOrUpdate_WithFactories_NewKey_CallsAddFactory() {
    var dict = new ConcurrentDictionary<string, int>();

    var value = dict.AddOrUpdate("key1", k => k.Length, (_, old) => old + 1);

    Assert.That(value, Is.EqualTo(4)); // "key1".Length
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_TryRemove_ExistingKey_ReturnsTrue() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 100);

    var success = dict.TryRemove("key1", out var value);

    Assert.That(success, Is.True);
    Assert.That(value, Is.EqualTo(100));
    Assert.That(dict.ContainsKey("key1"), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void ConcurrentDictionary_TryRemove_NonExistingKey_ReturnsFalse() {
    var dict = new ConcurrentDictionary<string, int>();

    var success = dict.TryRemove("key1", out _);

    Assert.That(success, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_TryGetValue_ExistingKey_ReturnsTrue() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 42);

    var success = dict.TryGetValue("key1", out var value);

    Assert.That(success, Is.True);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void ConcurrentDictionary_TryGetValue_NonExistingKey_ReturnsFalse() {
    var dict = new ConcurrentDictionary<string, int>();

    var success = dict.TryGetValue("key1", out _);

    Assert.That(success, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_ContainsKey_ExistingKey_ReturnsTrue() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("key1", 100);

    Assert.That(dict.ContainsKey("key1"), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_ContainsKey_NonExistingKey_ReturnsFalse() {
    var dict = new ConcurrentDictionary<string, int>();

    Assert.That(dict.ContainsKey("key1"), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_Count_ReturnsCorrectCount() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("a", 1);
    dict.TryAdd("b", 2);
    dict.TryAdd("c", 3);

    Assert.That(dict.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_Keys_ReturnsAllKeys() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("a", 1);
    dict.TryAdd("b", 2);

    var keys = dict.Keys.ToArray();

    Assert.That(keys.Length, Is.EqualTo(2));
    Assert.That(keys, Does.Contain("a"));
    Assert.That(keys, Does.Contain("b"));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_Values_ReturnsAllValues() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("a", 1);
    dict.TryAdd("b", 2);

    var values = dict.Values.ToArray();

    Assert.That(values.Length, Is.EqualTo(2));
    Assert.That(values, Does.Contain(1));
    Assert.That(values, Does.Contain(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_Indexer_Set_SetsValue() {
    var dict = new ConcurrentDictionary<string, int>();
    dict["key1"] = 100;

    Assert.That(dict["key1"], Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_Indexer_Set_OverwritesExisting() {
    var dict = new ConcurrentDictionary<string, int>();
    dict["key1"] = 100;
    dict["key1"] = 200;

    Assert.That(dict["key1"], Is.EqualTo(200));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_ToArray_ReturnsAllPairs() {
    var dict = new ConcurrentDictionary<string, int>();
    dict.TryAdd("a", 1);
    dict.TryAdd("b", 2);

    var array = dict.ToArray();

    Assert.That(array.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_WithComparer_UsesComparer() {
    var dict = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    dict.TryAdd("KEY", 100);

    Assert.That(dict.ContainsKey("key"), Is.True);
    Assert.That(dict["key"], Is.EqualTo(100));
  }

  #endregion

  #region ConcurrentQueue<T> Tests

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_Enqueue_AddsItems() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    Assert.That(queue.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_TryDequeue_ReturnsItemInFIFOOrder() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    queue.TryDequeue(out var first);
    queue.TryDequeue(out var second);
    queue.TryDequeue(out var third);

    Assert.That(first, Is.EqualTo(1));
    Assert.That(second, Is.EqualTo(2));
    Assert.That(third, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void ConcurrentQueue_TryDequeue_EmptyQueue_ReturnsFalse() {
    var queue = new ConcurrentQueue<int>();

    var success = queue.TryDequeue(out var item);

    Assert.That(success, Is.False);
    Assert.That(item, Is.EqualTo(default(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_IsEmpty_EmptyQueue_ReturnsTrue() {
    var queue = new ConcurrentQueue<string>();

    Assert.That(queue.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_IsEmpty_NonEmptyQueue_ReturnsFalse() {
    var queue = new ConcurrentQueue<string>();
    queue.Enqueue("test");

    Assert.That(queue.IsEmpty, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_Clear_RemovesAllItems() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);

    queue.Clear();

    Assert.That(queue.IsEmpty, Is.True);
    Assert.That(queue.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_ToArray_ReturnsItemsInOrder() {
    var queue = new ConcurrentQueue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    var array = queue.ToArray();

    Assert.That(array, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

  #region ConcurrentStack<T> Tests

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_Push_AddsItems() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    Assert.That(stack.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_TryPop_ReturnsItemInLIFOOrder() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    stack.TryPop(out var first);
    stack.TryPop(out var second);
    stack.TryPop(out var third);

    Assert.That(first, Is.EqualTo(3));
    Assert.That(second, Is.EqualTo(2));
    Assert.That(third, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ConcurrentStack_TryPop_EmptyStack_ReturnsFalse() {
    var stack = new ConcurrentStack<int>();

    var success = stack.TryPop(out var item);

    Assert.That(success, Is.False);
    Assert.That(item, Is.EqualTo(default(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_IsEmpty_EmptyStack_ReturnsTrue() {
    var stack = new ConcurrentStack<string>();

    Assert.That(stack.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_IsEmpty_NonEmptyStack_ReturnsFalse() {
    var stack = new ConcurrentStack<string>();
    stack.Push("test");

    Assert.That(stack.IsEmpty, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_Clear_RemovesAllItems() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);

    stack.Clear();

    Assert.That(stack.IsEmpty, Is.True);
    Assert.That(stack.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_ToArray_ReturnsItemsInLIFOOrder() {
    var stack = new ConcurrentStack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    var array = stack.ToArray();

    Assert.That(array, Is.EqualTo(new[] { 3, 2, 1 }));
  }

  #endregion

  #region Thread Safety Tests

  [Test]
  [Category("HappyPath")]
  public void ConcurrentBag_ParallelAdd_MaintainsCount() {
    var bag = new ConcurrentBag<int>();
    const int itemCount = 1000;

    Parallel.For(0, itemCount, i => bag.Add(i));

    Assert.That(bag.Count, Is.EqualTo(itemCount));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentDictionary_ParallelGetOrAdd_NoRaceConditions() {
    var dict = new ConcurrentDictionary<int, int>();
    const int iterations = 1000;
    var factoryCallCount = 0;

    Parallel.For(0, iterations, _ => {
      dict.GetOrAdd(1, _ => {
        System.Threading.Interlocked.Increment(ref factoryCallCount);
        return 42;
      });
    });

    Assert.That(dict[1], Is.EqualTo(42));
    Assert.That(dict.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentQueue_ParallelEnqueueDequeue_MaintainsIntegrity() {
    var queue = new ConcurrentQueue<int>();
    const int itemCount = 100;
    var dequeuedItems = new ConcurrentBag<int>();

    for (var i = 0; i < itemCount; ++i)
      queue.Enqueue(i);

    Parallel.For(0, itemCount, _ => {
      if (queue.TryDequeue(out var item))
        dequeuedItems.Add(item);
    });

    Assert.That(dequeuedItems.Count, Is.EqualTo(itemCount));
    Assert.That(queue.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ConcurrentStack_ParallelPushPop_MaintainsIntegrity() {
    var stack = new ConcurrentStack<int>();
    const int itemCount = 100;
    var poppedItems = new ConcurrentBag<int>();

    for (var i = 0; i < itemCount; ++i)
      stack.Push(i);

    Parallel.For(0, itemCount, _ => {
      if (stack.TryPop(out var item))
        poppedItems.Add(item);
    });

    Assert.That(poppedItems.Count, Is.EqualTo(itemCount));
    Assert.That(stack.IsEmpty, Is.True);
  }

  #endregion

}
