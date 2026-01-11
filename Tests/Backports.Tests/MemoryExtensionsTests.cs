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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
public class MemoryExtensionsTests {

  #region Count

  [Test]
  [Category("HappyPath")]
  public void Count_CountsOccurrences() {
    var span = new[] { 1, 2, 3, 2, 4, 2 }.AsSpan();
    Assert.That(span.Count(2), Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Count_NoOccurrences_ReturnsZero() {
    var span = new[] { 1, 2, 3 }.AsSpan();
    Assert.That(span.Count(5), Is.EqualTo(0));
  }

  #endregion

  #region Sort

  [Test]
  [Category("HappyPath")]
  public void Sort_SortsSpan() {
    Span<int> span = stackalloc int[] { 5, 2, 8, 1, 9 };
    MemoryExtensions.Sort(span);
    Assert.That(span[0], Is.EqualTo(1));
    Assert.That(span[1], Is.EqualTo(2));
    Assert.That(span[2], Is.EqualTo(5));
    Assert.That(span[3], Is.EqualTo(8));
    Assert.That(span[4], Is.EqualTo(9));
  }

  [Test]
  [Category("HappyPath")]
  public void Sort_WithComparison_SortsDescending() {
    Span<int> span = stackalloc int[] { 1, 5, 3, 2, 4 };
    MemoryExtensions.Sort(span, (a, b) => b.CompareTo(a));
    Assert.That(span[0], Is.EqualTo(5));
    Assert.That(span[4], Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Sort_EmptySpan_NoException() {
    Span<int> span = stackalloc int[0];
    MemoryExtensions.Sort(span);
    Assert.Pass();
  }

  [Test]
  [Category("HappyPath")]
  public void Sort_KeysAndItems_SortsTogether() {
    Span<int> keys = stackalloc int[] { 3, 1, 2 };
    Span<string> items = new[] { "three", "one", "two" };
    MemoryExtensions.Sort(keys, items);
    Assert.That(keys[0], Is.EqualTo(1));
    Assert.That(items[0], Is.EqualTo("one"));
    Assert.That(keys[2], Is.EqualTo(3));
    Assert.That(items[2], Is.EqualTo("three"));
  }

  #endregion

  #region Reverse

  [Test]
  [Category("HappyPath")]
  public void Reverse_ReversesSpan() {
    Span<int> span = stackalloc int[] { 1, 2, 3, 4, 5 };
    MemoryExtensions.Reverse(span);
    Assert.That(span[0], Is.EqualTo(5));
    Assert.That(span[1], Is.EqualTo(4));
    Assert.That(span[2], Is.EqualTo(3));
    Assert.That(span[3], Is.EqualTo(2));
    Assert.That(span[4], Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Reverse_SingleElement_NoChange() {
    Span<int> span = stackalloc int[] { 42 };
    MemoryExtensions.Reverse(span);
    Assert.That(span[0], Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Reverse_EmptySpan_NoException() {
    Span<int> span = stackalloc int[0];
    MemoryExtensions.Reverse(span);
    Assert.Pass();
  }

  [Test]
  [Category("HappyPath")]
  public void Reverse_EvenLength_ReversesCorrectly() {
    Span<char> span = stackalloc char[] { 'a', 'b', 'c', 'd' };
    MemoryExtensions.Reverse(span);
    Assert.That(span[0], Is.EqualTo('d'));
    Assert.That(span[1], Is.EqualTo('c'));
    Assert.That(span[2], Is.EqualTo('b'));
    Assert.That(span[3], Is.EqualTo('a'));
  }

  #endregion

  #region AsMemory

  [Test]
  [Category("HappyPath")]
  public void AsMemory_ReturnsMemoryOfArray() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = array.AsMemory();
    Assert.That(memory.Length, Is.EqualTo(5));
    Assert.That(memory.Span[0], Is.EqualTo(1));
    Assert.That(memory.Span[4], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AsMemory_WithStart_ReturnsSlice() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = array.AsMemory(2);
    Assert.That(memory.Length, Is.EqualTo(3));
    Assert.That(memory.Span[0], Is.EqualTo(3));
    Assert.That(memory.Span[2], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void AsMemory_WithStartAndLength_ReturnsSlice() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var memory = array.AsMemory(1, 3);
    Assert.That(memory.Length, Is.EqualTo(3));
    Assert.That(memory.Span[0], Is.EqualTo(2));
    Assert.That(memory.Span[2], Is.EqualTo(4));
  }

  [Test]
  [Category("EdgeCase")]
  public void AsMemory_NullArray_ReturnsDefaultMemory() {
    int[]? array = null;
    var memory = array.AsMemory();
    Assert.That(memory.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void AsMemory_EmptyArray_ReturnsEmptyMemory() {
    var array = Array.Empty<int>();
    var memory = array.AsMemory();
    Assert.That(memory.IsEmpty, Is.True);
  }

  #endregion
  
  #region Split

  [Test]
  [Category("HappyPath")]
  public void Split_SplitsByCharacter() {
    var span = "a,b,c".AsSpan();
    var enumerator = span.Split(',');
    var parts = new System.Collections.Generic.List<string>();
    foreach (var part in enumerator)
      parts.Add(span[part].ToString());
    Assert.That(parts, Is.EqualTo(new[] { "a", "b", "c" }));
  }

  [Test]
  [Category("EdgeCase")]
  public void Split_EmptySpan_ReturnsSingleEmptyPart() {
    var span = ReadOnlySpan<char>.Empty;
    var enumerator = span.Split(',');
    var count = 0;
    foreach (var _ in enumerator)
      ++count;
    Assert.That(count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Split_NoSeparator_ReturnsSinglePart() {
    var span = "abc".AsSpan();
    var enumerator = span.Split(',');
    var parts = new System.Collections.Generic.List<string>();
    foreach (var part in enumerator)
      parts.Add(span[part].ToString());
    Assert.That(parts, Is.EqualTo(new[] { "abc" }));
  }

  #endregion

}
