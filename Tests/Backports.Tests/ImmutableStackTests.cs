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
[Category("ImmutableStack")]
public class ImmutableStackTests {

  #region Empty

  [Test]
  [Category("HappyPath")]
  public void Empty_ReturnsEmptyStack() {
    var empty = ImmutableStack<int>.Empty;
    Assert.That(empty.IsEmpty, Is.True);
  }

  #endregion

  #region Create

  [Test]
  [Category("HappyPath")]
  public void Create_NoArgs_ReturnsEmptyStack() {
    var stack = ImmutableStack.Create<int>();
    Assert.That(stack.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Create_SingleItem_ReturnsStackWithOneElement() {
    var stack = ImmutableStack.Create(42);
    Assert.That(stack.IsEmpty, Is.False);
    Assert.That(stack.Peek(), Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_MultipleItems_ReturnsStackWithElements() {
    var stack = ImmutableStack.CreateRange(new[] { 1, 2, 3 });
    Assert.That(stack.Peek(), Is.EqualTo(3));
  }

  #endregion

  #region CreateRange

  [Test]
  [Category("HappyPath")]
  public void CreateRange_FromEnumerable_CreatesStack() {
    var source = new[] { 1, 2, 3 };
    var stack = ImmutableStack.CreateRange(source);
    Assert.That(stack.Peek(), Is.EqualTo(3));
  }

  #endregion

  #region Push/Pop/Peek

  [Test]
  [Category("HappyPath")]
  public void Push_ReturnsNewStackWithElement() {
    var stack = ImmutableStack<int>.Empty;
    var newStack = stack.Push(42);
    Assert.That(newStack.IsEmpty, Is.False);
    Assert.That(newStack.Peek(), Is.EqualTo(42));
    Assert.That(stack.IsEmpty, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Push_MultipleTimes_MaintainsLIFOOrder() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
    Assert.That(stack.Peek(), Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Pop_ReturnsNewStackWithoutTopElement() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2);
    var newStack = stack.Pop();
    Assert.That(newStack.Peek(), Is.EqualTo(1));
    Assert.That(stack.Peek(), Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Pop_WithOutValue_ReturnsValueAndNewStack() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2);
    var newStack = stack.Pop(out var value);
    Assert.That(value, Is.EqualTo(2));
    Assert.That(newStack.Peek(), Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void Pop_EmptyStack_ThrowsInvalidOperationException() {
    var stack = ImmutableStack<int>.Empty;
    Assert.Throws<InvalidOperationException>(() => stack.Pop());
  }

  [Test]
  [Category("HappyPath")]
  public void Peek_ReturnsTopElement() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2);
    Assert.That(stack.Peek(), Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void Peek_EmptyStack_ThrowsInvalidOperationException() {
    var stack = ImmutableStack<int>.Empty;
    Assert.Throws<InvalidOperationException>(() => stack.Peek());
  }

  #endregion

  #region Clear

  [Test]
  [Category("HappyPath")]
  public void Clear_ReturnsEmptyStack() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
    var newStack = stack.Clear();
    Assert.That(newStack.IsEmpty, Is.True);
  }

  #endregion

  #region Enumeration

  [Test]
  [Category("HappyPath")]
  public void GetEnumerator_EnumeratesFromTopToBottom() {
    var stack = ImmutableStack<int>.Empty.Push(1).Push(2).Push(3);
    var items = stack.ToArray();
    Assert.That(items, Is.EqualTo(new[] { 3, 2, 1 }));
  }

  #endregion

}
