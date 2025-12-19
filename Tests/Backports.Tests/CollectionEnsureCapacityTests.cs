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
public class CollectionEnsureCapacityTests {

  #region Queue<T>.EnsureCapacity

  [Test]
  [Category("HappyPath")]
  public void Queue_EnsureCapacity_ReturnsAtLeastRequestedCapacity() {
    var queue = new Queue<int>();
    var result = queue.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Queue_EnsureCapacity_PreservesExistingItems() {
    var queue = new Queue<int>();
    queue.Enqueue(1);
    queue.Enqueue(2);
    queue.Enqueue(3);

    queue.EnsureCapacity(100);

    Assert.That(queue.Count, Is.EqualTo(3));
    Assert.That(queue.Dequeue(), Is.EqualTo(1));
    Assert.That(queue.Dequeue(), Is.EqualTo(2));
    Assert.That(queue.Dequeue(), Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Queue_EnsureCapacity_ZeroCapacity_ReturnsCurrentCapacity() {
    var queue = new Queue<int>();
    queue.Enqueue(1);
    var result = queue.EnsureCapacity(0);
    Assert.That(result, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Queue_EnsureCapacity_AlreadySufficient_ReturnsCurrentCapacity() {
    var queue = new Queue<int>(200);
    var result = queue.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("Exception")]
  public void Queue_EnsureCapacity_NegativeCapacity_ThrowsArgumentOutOfRange() {
    var queue = new Queue<int>();
    Assert.Throws<ArgumentOutOfRangeException>(() => queue.EnsureCapacity(-1));
  }

  #endregion

  #region Stack<T>.EnsureCapacity

  [Test]
  [Category("HappyPath")]
  public void Stack_EnsureCapacity_ReturnsAtLeastRequestedCapacity() {
    var stack = new Stack<int>();
    var result = stack.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Stack_EnsureCapacity_PreservesExistingItems() {
    var stack = new Stack<int>();
    stack.Push(1);
    stack.Push(2);
    stack.Push(3);

    stack.EnsureCapacity(100);

    Assert.That(stack.Count, Is.EqualTo(3));
    Assert.That(stack.Pop(), Is.EqualTo(3));
    Assert.That(stack.Pop(), Is.EqualTo(2));
    Assert.That(stack.Pop(), Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Stack_EnsureCapacity_ZeroCapacity_ReturnsCurrentCapacity() {
    var stack = new Stack<int>();
    stack.Push(1);
    var result = stack.EnsureCapacity(0);
    Assert.That(result, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void Stack_EnsureCapacity_NegativeCapacity_ThrowsArgumentOutOfRange() {
    var stack = new Stack<int>();
    Assert.Throws<ArgumentOutOfRangeException>(() => stack.EnsureCapacity(-1));
  }

  #endregion

  #region HashSet<T>.EnsureCapacity

  [Test]
  [Category("HappyPath")]
  public void HashSet_EnsureCapacity_ReturnsAtLeastRequestedCapacity() {
    var set = new HashSet<int>();
    var result = set.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void HashSet_EnsureCapacity_PreservesExistingItems() {
    var set = new HashSet<int> { 1, 2, 3 };

    set.EnsureCapacity(100);

    Assert.That(set.Count, Is.EqualTo(3));
    Assert.That(set.Contains(1), Is.True);
    Assert.That(set.Contains(2), Is.True);
    Assert.That(set.Contains(3), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void HashSet_EnsureCapacity_ZeroCapacity_ReturnsCurrentCapacity() {
    var set = new HashSet<int> { 1 };
    var result = set.EnsureCapacity(0);
    Assert.That(result, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void HashSet_EnsureCapacity_NegativeCapacity_ThrowsArgumentOutOfRange() {
    var set = new HashSet<int>();
    Assert.Throws<ArgumentOutOfRangeException>(() => set.EnsureCapacity(-1));
  }

  #endregion

  #region Dictionary<TKey, TValue>.EnsureCapacity

  [Test]
  [Category("HappyPath")]
  public void Dictionary_EnsureCapacity_ReturnsAtLeastRequestedCapacity() {
    var dict = new Dictionary<string, int>();
    var result = dict.EnsureCapacity(100);
    Assert.That(result, Is.GreaterThanOrEqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Dictionary_EnsureCapacity_PreservesExistingItems() {
    var dict = new Dictionary<string, int> {
      ["one"] = 1,
      ["two"] = 2,
      ["three"] = 3
    };

    dict.EnsureCapacity(100);

    Assert.That(dict.Count, Is.EqualTo(3));
    Assert.That(dict["one"], Is.EqualTo(1));
    Assert.That(dict["two"], Is.EqualTo(2));
    Assert.That(dict["three"], Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Dictionary_EnsureCapacity_ZeroCapacity_ReturnsCurrentCapacity() {
    var dict = new Dictionary<string, int> { ["one"] = 1 };
    var result = dict.EnsureCapacity(0);
    Assert.That(result, Is.GreaterThanOrEqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void Dictionary_EnsureCapacity_NegativeCapacity_ThrowsArgumentOutOfRange() {
    var dict = new Dictionary<string, int>();
    Assert.Throws<ArgumentOutOfRangeException>(() => dict.EnsureCapacity(-1));
  }

  #endregion

}
