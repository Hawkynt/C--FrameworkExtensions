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
[Category("Linq")]
public class LinqExtendedTests {

  #region Enumerable.Chunk

  [Test]
  [Category("HappyPath")]
  public void Chunk_SplitsIntoCorrectSizedChunks() {
    var source = new[] { 1, 2, 3, 4, 5, 6, 7 };
    var result = source.Chunk(3).ToArray();
    Assert.That(result.Length, Is.EqualTo(3));
    Assert.That(result[0], Is.EqualTo(new[] { 1, 2, 3 }));
    Assert.That(result[1], Is.EqualTo(new[] { 4, 5, 6 }));
    Assert.That(result[2], Is.EqualTo(new[] { 7 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Chunk_ExactDivision_NoPartialChunks() {
    var source = new[] { 1, 2, 3, 4, 5, 6 };
    var result = source.Chunk(3).ToArray();
    Assert.That(result.Length, Is.EqualTo(2));
    Assert.That(result[0].Length, Is.EqualTo(3));
    Assert.That(result[1].Length, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void Chunk_EmptySource_ReturnsEmptySequence() {
    var source = new int[0];
    var result = source.Chunk(3).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Chunk_ChunkSizeLargerThanSource_ReturnsSingleChunk() {
    var source = new[] { 1, 2, 3 };
    var result = source.Chunk(10).ToArray();
    Assert.That(result.Length, Is.EqualTo(1));
    Assert.That(result[0], Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("Exception")]
  public void Chunk_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.Chunk(3).ToArray());
  }

  [Test]
  [Category("Exception")]
  public void Chunk_ZeroSize_ThrowsArgumentOutOfRangeException() {
    var source = new[] { 1, 2, 3 };
    Assert.Throws<ArgumentOutOfRangeException>(() => source.Chunk(0).ToArray());
  }

  #endregion

  #region Enumerable.DistinctBy

  [Test]
  [Category("HappyPath")]
  public void DistinctBy_RemovesDuplicatesByKey() {
    var source = new[] {
      new { Name = "Alice", Age = 30 },
      new { Name = "Bob", Age = 25 },
      new { Name = "Charlie", Age = 30 }
    };
    var result = source.DistinctBy(x => x.Age).ToArray();
    Assert.That(result.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void DistinctBy_PreservesFirstOccurrence() {
    var source = new[] { "apple", "apricot", "banana", "blueberry" };
    var result = source.DistinctBy(x => x[0]).ToArray();
    Assert.That(result, Is.EqualTo(new[] { "apple", "banana" }));
  }

  [Test]
  [Category("HappyPath")]
  public void DistinctBy_NoDuplicates_ReturnsAll() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.DistinctBy(x => x).ToArray();
    Assert.That(result, Is.EqualTo(source));
  }

  [Test]
  [Category("EdgeCase")]
  public void DistinctBy_EmptySource_ReturnsEmpty() {
    var source = new int[0];
    var result = source.DistinctBy(x => x).ToArray();
    Assert.That(result, Is.Empty);
  }

  #endregion

  #region Enumerable.Order/OrderDescending

  [Test]
  [Category("HappyPath")]
  public void Order_SortsAscending() {
    var source = new[] { 3, 1, 4, 1, 5, 9, 2, 6 };
    var result = source.Order().ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 1, 2, 3, 4, 5, 6, 9 }));
  }

  [Test]
  [Category("HappyPath")]
  public void OrderDescending_SortsDescending() {
    var source = new[] { 3, 1, 4, 1, 5, 9, 2, 6 };
    var result = source.OrderDescending().ToArray();
    Assert.That(result, Is.EqualTo(new[] { 9, 6, 5, 4, 3, 2, 1, 1 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Order_Strings_SortsAlphabetically() {
    var source = new[] { "banana", "apple", "cherry" };
    var result = source.Order().ToArray();
    Assert.That(result, Is.EqualTo(new[] { "apple", "banana", "cherry" }));
  }

  [Test]
  [Category("EdgeCase")]
  public void Order_EmptySource_ReturnsEmpty() {
    var source = new int[0];
    var result = source.Order().ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Order_SingleElement_ReturnsSameElement() {
    var source = new[] { 42 };
    var result = source.Order().ToArray();
    Assert.That(result, Is.EqualTo(new[] { 42 }));
  }

  #endregion

  #region Enumerable.MinBy/MaxBy

  [Test]
  [Category("HappyPath")]
  public void MinBy_ReturnsElementWithMinimumKey() {
    var source = new[] {
      new { Name = "Alice", Age = 30 },
      new { Name = "Bob", Age = 25 },
      new { Name = "Charlie", Age = 35 }
    };
    var result = source.MinBy(x => x.Age);
    Assert.That(result.Name, Is.EqualTo("Bob"));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxBy_ReturnsElementWithMaximumKey() {
    var source = new[] {
      new { Name = "Alice", Age = 30 },
      new { Name = "Bob", Age = 25 },
      new { Name = "Charlie", Age = 35 }
    };
    var result = source.MaxBy(x => x.Age);
    Assert.That(result.Name, Is.EqualTo("Charlie"));
  }

  [Test]
  [Category("HappyPath")]
  public void MinBy_MultipleWithSameKey_ReturnsFirst() {
    var source = new[] { "ab", "cd", "ef" };
    var result = source.MinBy(x => x.Length);
    Assert.That(result, Is.EqualTo("ab"));
  }

  [Test]
  [Category("HappyPath")]
  public void MaxBy_MultipleWithSameKey_ReturnsFirst() {
    var source = new[] { "ab", "cd", "ef" };
    var result = source.MaxBy(x => x.Length);
    Assert.That(result, Is.EqualTo("ab"));
  }

  #endregion


  #region Enumerable.Append/Prepend

  [Test]
  [Category("HappyPath")]
  public void Append_AddsElementAtEnd() {
    var source = new[] { 1, 2, 3 };
    var result = source.Append(4).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Prepend_AddsElementAtBeginning() {
    var source = new[] { 2, 3, 4 };
    var result = source.Prepend(1).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Append_ToEmptySequence_ReturnsSequenceWithElement() {
    var source = new int[0];
    var result = source.Append(1).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Prepend_ToEmptySequence_ReturnsSequenceWithElement() {
    var source = new int[0];
    var result = source.Prepend(1).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1 }));
  }

  [Test]
  [Category("Exception")]
  public void Append_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.Append(1).ToArray());
  }

  [Test]
  [Category("Exception")]
  public void Prepend_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.Prepend(1).ToArray());
  }

  #endregion

  #region Enumerable.TryGetNonEnumeratedCount

  [Test]
  [Category("HappyPath")]
  public void TryGetNonEnumeratedCount_Array_ReturnsTrue() {
    IEnumerable<int> source = new[] { 1, 2, 3 };
    var result = source.TryGetNonEnumeratedCount(out var count);
    Assert.That(result, Is.True);
    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetNonEnumeratedCount_List_ReturnsTrue() {
    IEnumerable<int> source = new List<int> { 1, 2, 3, 4 };
    var result = source.TryGetNonEnumeratedCount(out var count);
    Assert.That(result, Is.True);
    Assert.That(count, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void TryGetNonEnumeratedCount_NonCollection_ReturnsFalse() {
    IEnumerable<int> source = GetNumbers();
    var result = source.TryGetNonEnumeratedCount(out var count);
    Assert.That(result, Is.False);
    Assert.That(count, Is.EqualTo(0));

    static IEnumerable<int> GetNumbers() {
      yield return 1;
      yield return 2;
      yield return 3;
    }
  }

  #endregion

  #region Enumerable.Zip

  [Test]
  [Category("HappyPath")]
  public void Zip_CombinesTwoSequences() {
    var first = new[] { 1, 2, 3 };
    var second = new[] { "a", "b", "c" };
    var result = first.Zip(second, (x, y) => $"{x}{y}").ToArray();
    Assert.That(result, Is.EqualTo(new[] { "1a", "2b", "3c" }));
  }

  [Test]
  [Category("HappyPath")]
  public void Zip_DifferentLengths_StopsAtShorter() {
    var first = new[] { 1, 2, 3, 4, 5 };
    var second = new[] { "a", "b" };
    var result = first.Zip(second, (x, y) => $"{x}{y}").ToArray();
    Assert.That(result, Is.EqualTo(new[] { "1a", "2b" }));
  }

  [Test]
  [Category("EdgeCase")]
  public void Zip_EmptyFirst_ReturnsEmpty() {
    var first = new int[0];
    var second = new[] { "a", "b", "c" };
    var result = first.Zip(second, (x, y) => $"{x}{y}").ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Zip_EmptySecond_ReturnsEmpty() {
    var first = new[] { 1, 2, 3 };
    var second = new string[0];
    var result = first.Zip(second, (x, y) => $"{x}{y}").ToArray();
    Assert.That(result, Is.Empty);
  }

  #endregion


  #region FirstOrDefault/LastOrDefault/SingleOrDefault with default value

  [Test]
  [Category("HappyPath")]
  public void FirstOrDefault_WithDefault_ReturnsFirstElement() {
    var source = new[] { 1, 2, 3 };
    var result = source.FirstOrDefault(99);
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void FirstOrDefault_EmptyWithDefault_ReturnsDefault() {
    var source = new int[0];
    var result = source.FirstOrDefault(99);
    Assert.That(result, Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void LastOrDefault_WithDefault_ReturnsLastElement() {
    var source = new[] { 1, 2, 3 };
    var result = source.LastOrDefault(99);
    Assert.That(result, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void LastOrDefault_EmptyWithDefault_ReturnsDefault() {
    var source = new int[0];
    var result = source.LastOrDefault(99);
    Assert.That(result, Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void SingleOrDefault_SingleElementWithDefault_ReturnsElement() {
    var source = new[] { 42 };
    var result = source.SingleOrDefault(99);
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void SingleOrDefault_EmptyWithDefault_ReturnsDefault() {
    var source = new int[0];
    var result = source.SingleOrDefault(99);
    Assert.That(result, Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void FirstOrDefault_PredicateWithDefault_ReturnsMatchingElement() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.FirstOrDefault(x => x > 3, 99);
    Assert.That(result, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void FirstOrDefault_PredicateNoMatchWithDefault_ReturnsDefault() {
    var source = new[] { 1, 2, 3 };
    var result = source.FirstOrDefault(x => x > 10, 99);
    Assert.That(result, Is.EqualTo(99));
  }

  #endregion

  #region Enumerable.TakeLast

  [Test]
  [Category("HappyPath")]
  public void TakeLast_ReturnsLastNElements() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.TakeLast(3).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 3, 4, 5 }));
  }

  [Test]
  [Category("HappyPath")]
  public void TakeLast_CountExceedsLength_ReturnsAll() {
    var source = new[] { 1, 2, 3 };
    var result = source.TakeLast(10).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void TakeLast_ZeroCount_ReturnsEmpty() {
    var source = new[] { 1, 2, 3 };
    var result = source.TakeLast(0).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void TakeLast_NegativeCount_ReturnsEmpty() {
    var source = new[] { 1, 2, 3 };
    var result = source.TakeLast(-5).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void TakeLast_EmptySource_ReturnsEmpty() {
    var source = new int[0];
    var result = source.TakeLast(3).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void TakeLast_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.TakeLast(1).ToArray());
  }

  [Test]
  [Category("HappyPath")]
  public void TakeLast_WorksWithNonListEnumerable() {
    IEnumerable<int> GenerateSequence() {
      yield return 1;
      yield return 2;
      yield return 3;
      yield return 4;
      yield return 5;
    }
    var result = GenerateSequence().TakeLast(2).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 4, 5 }));
  }

  #endregion

  #region Enumerable.SkipLast

  [Test]
  [Category("HappyPath")]
  public void SkipLast_SkipsLastNElements() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.SkipLast(2).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void SkipLast_CountExceedsLength_ReturnsEmpty() {
    var source = new[] { 1, 2, 3 };
    var result = source.SkipLast(10).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void SkipLast_ZeroCount_ReturnsAll() {
    var source = new[] { 1, 2, 3 };
    var result = source.SkipLast(0).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void SkipLast_NegativeCount_ReturnsAll() {
    var source = new[] { 1, 2, 3 };
    var result = source.SkipLast(-5).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("EdgeCase")]
  public void SkipLast_EmptySource_ReturnsEmpty() {
    var source = new int[0];
    var result = source.SkipLast(3).ToArray();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void SkipLast_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> source = null;
    Assert.Throws<ArgumentNullException>(() => source.SkipLast(1).ToArray());
  }

  [Test]
  [Category("HappyPath")]
  public void SkipLast_WorksWithNonListEnumerable() {
    IEnumerable<int> GenerateSequence() {
      yield return 1;
      yield return 2;
      yield return 3;
      yield return 4;
      yield return 5;
    }
    var result = GenerateSequence().SkipLast(2).ToArray();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

}
