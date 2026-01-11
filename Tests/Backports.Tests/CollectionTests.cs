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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Collections")]
public class CollectionTests {

  #region HashSet basic tests

  [Test]
  [Category("HappyPath")]
  public void HashSet_Add_AddsElements() {
    var set = new HashSet<int>();
    Assert.That(set.Add(1), Is.True);
    Assert.That(set.Add(2), Is.True);
    Assert.That(set.Add(3), Is.True);
    Assert.That(set.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Add_Duplicate_ReturnsFalse() {
    var set = new HashSet<int>();
    set.Add(1);
    Assert.That(set.Add(1), Is.False);
    Assert.That(set.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Contains_FindsElement() {
    var set = new HashSet<int> { 1, 2, 3 };
    Assert.That(set.Contains(2), Is.True);
    Assert.That(set.Contains(4), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Remove_RemovesElement() {
    var set = new HashSet<int> { 1, 2, 3 };
    Assert.That(set.Remove(2), Is.True);
    Assert.That(set.Count, Is.EqualTo(2));
    Assert.That(set.Contains(2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_Clear_RemovesAllElements() {
    var set = new HashSet<int> { 1, 2, 3 };
    set.Clear();
    Assert.That(set.Count, Is.EqualTo(0));
  }

  #endregion

  #region Stack.TryPop/TryPeek

  [Test]
  [Category("HappyPath")]
  public void Stack_TryPop_WithElements_ReturnsTrue() {
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    var success = stack.TryPop(out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Stack_TryPop_EmptyStack_ReturnsFalse() {
    var stack = new Stack<int>();
    var success = stack.TryPop(out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void Stack_TryPeek_WithElements_ReturnsTrue() {
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    var success = stack.TryPeek(out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(2));
    Assert.That(stack.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Stack_TryPeek_EmptyStack_ReturnsFalse() {
    var stack = new Stack<int>();
    var success = stack.TryPeek(out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  #endregion

  #region Dictionary.TryAdd

  [Test]
  [Category("HappyPath")]
  public void Dictionary_TryAdd_NewKey_ReturnsTrue() {
    var dict = new Dictionary<string, int>();
    var result = dict.TryAdd("key1", 1);
    Assert.That(result, Is.True);
    Assert.That(dict["key1"], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dictionary_TryAdd_ExistingKey_ReturnsFalse() {
    var dict = new Dictionary<string, int> { { "key1", 1 } };
    var result = dict.TryAdd("key1", 2);
    Assert.That(result, Is.False);
    Assert.That(dict["key1"], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Dictionary_TryAdd_MultipleKeys_AddsAll() {
    var dict = new Dictionary<string, int>();
    Assert.That(dict.TryAdd("a", 1), Is.True);
    Assert.That(dict.TryAdd("b", 2), Is.True);
    Assert.That(dict.TryAdd("c", 3), Is.True);
    Assert.That(dict.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Dictionary_TryAdd_NullValue_Succeeds() {
    var dict = new Dictionary<string, string>();
    var result = dict.TryAdd("key1", null);
    Assert.That(result, Is.True);
    Assert.That(dict["key1"], Is.Null);
  }

  #endregion

  #region HashSet.TryGetValue

  [Test]
  [Category("HappyPath")]
  public void HashSet_TryGetValue_ExistingValue_ReturnsTrue() {
    var set = new HashSet<string> { "Hello", "World" };
    var result = set.TryGetValue("Hello", out var actualValue);
    Assert.That(result, Is.True);
    Assert.That(actualValue, Is.EqualTo("Hello"));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_TryGetValue_NonExistingValue_ReturnsFalse() {
    var set = new HashSet<string> { "Hello", "World" };
    var result = set.TryGetValue("NotFound", out var actualValue);
    Assert.That(result, Is.False);
    Assert.That(actualValue, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_TryGetValue_EmptySet_ReturnsFalse() {
    var set = new HashSet<string>();
    var result = set.TryGetValue("Any", out var actualValue);
    Assert.That(result, Is.False);
    Assert.That(actualValue, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_TryGetValue_IntType_ReturnsActualValue() {
    var set = new HashSet<int> { 1, 2, 3 };
    var result = set.TryGetValue(2, out var actualValue);
    Assert.That(result, Is.True);
    Assert.That(actualValue, Is.EqualTo(2));
  }

  #endregion

  #region Queue.TryDequeue

  [Test]
  [Category("HappyPath")]
  public void Queue_TryDequeue_WithElements_ReturnsTrue() {
    var queue = new Queue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    var success = queue.TryDequeue(out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1));
    Assert.That(queue.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Queue_TryDequeue_EmptyQueue_ReturnsFalse() {
    var queue = new Queue<int>();
    var success = queue.TryDequeue(out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void Queue_TryDequeue_FIFO_Order() {
    var queue = new Queue<string>();
    queue.Enqueue("first");
    queue.Enqueue("second");
    queue.Enqueue("third");

    queue.TryDequeue(out var r1);
    queue.TryDequeue(out var r2);
    queue.TryDequeue(out var r3);

    Assert.That(r1, Is.EqualTo("first"));
    Assert.That(r2, Is.EqualTo("second"));
    Assert.That(r3, Is.EqualTo("third"));
  }

  #endregion

  #region Queue.TryPeek

  [Test]
  [Category("HappyPath")]
  public void Queue_TryPeek_WithElements_ReturnsTrue() {
    var queue = new Queue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    var success = queue.TryPeek(out var result);
    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1));
    Assert.That(queue.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Queue_TryPeek_EmptyQueue_ReturnsFalse() {
    var queue = new Queue<int>();
    var success = queue.TryPeek(out var result);
    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  #endregion

  #region SortedSet.TryGetValue

  [Test]
  [Category("HappyPath")]
  public void SortedSet_TryGetValue_ExistingValue_ReturnsTrue() {
    var set = new SortedSet<string> { "Apple", "Banana", "Cherry" };
    var result = set.TryGetValue("Banana", out var actualValue);
    Assert.That(result, Is.True);
    Assert.That(actualValue, Is.EqualTo("Banana"));
  }

  [Test]
  [Category("HappyPath")]
  public void SortedSet_TryGetValue_NonExistingValue_ReturnsFalse() {
    var set = new SortedSet<string> { "Apple", "Banana", "Cherry" };
    var result = set.TryGetValue("Date", out var actualValue);
    Assert.That(result, Is.False);
    Assert.That(actualValue, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void SortedSet_TryGetValue_EmptySet_ReturnsFalse() {
    var set = new SortedSet<string>();
    var result = set.TryGetValue("Any", out var actualValue);
    Assert.That(result, Is.False);
    Assert.That(actualValue, Is.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void SortedSet_TryGetValue_IntType_ReturnsActualValue() {
    var set = new SortedSet<int> { 1, 2, 3 };
    var result = set.TryGetValue(2, out var actualValue);
    Assert.That(result, Is.True);
    Assert.That(actualValue, Is.EqualTo(2));
  }

  #endregion

}
