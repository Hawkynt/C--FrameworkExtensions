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


}
