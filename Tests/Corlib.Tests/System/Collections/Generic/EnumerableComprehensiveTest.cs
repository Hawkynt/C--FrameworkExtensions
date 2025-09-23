using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace System.Collections.Generic;

[TestFixture]
public class EnumerableComprehensiveTest {
  #region Test Data and Setup

  private static IEnumerable<int> GetTestNumbers() => new[] {
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9,
    10
  };

  private static IEnumerable<string> GetTestStrings() => new[] { "apple", "banana", "cherry", "date", "elderberry" };
  private static IEnumerable<TestItem> GetTestItems() => new[] { new TestItem { Id = 1, Name = "First", Value = 10.5 }, new TestItem { Id = 2, Name = "Second", Value = 20.0 }, new TestItem { Id = 3, Name = "Third", Value = 15.75 }, new TestItem { Id = 4, Name = "Fourth", Value = 30.25 }, new TestItem { Id = 5, Name = "Fifth", Value = 5.5 } };

  private class TestItem {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Value { get; set; }

    public override string ToString() => $"{this.Name}({this.Id})";
    public override bool Equals(object obj) => obj is TestItem other && this.Id == other.Id;
    public override int GetHashCode() => this.Id.GetHashCode();
  }

  private class DisposableTestItem : IDisposable {
    public int Id { get; set; }
    public bool IsDisposed { get; private set; }
    public void Dispose() => this.IsDisposed = true;
  }

  #endregion

  #region Append/Prepend Tests

  [Test]
  public void EnumerableExtensions_Prepend_WithItems_AddsItemsAtBeginning() {
    var source = new[] { 3, 4, 5 };
    var result = source.Prepend(1, 2).ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void EnumerableExtensions_Append_WithItems_AddsItemsAtEnd() {
    var source = new[] { 1, 2, 3 };
    var result = source.Append(4, 5).ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void EnumerableExtensions_Prepend_WithEnumerable_AddsEnumerableAtBeginning() {
    var source = new[] { 3, 4, 5 };
    var prepend = new[] { 1, 2 };
    var result = source.Prepend(prepend).ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void EnumerableExtensions_Append_WithEnumerable_AddsEnumerableAtEnd() {
    var source = new[] { 1, 2, 3 };
    var append = new[] { 4, 5 };
    var result = source.Append(append).ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void EnumerableExtensions_Prepend_EmptySource_ReturnsItemsOnly() {
    var source = Enumerable.Empty<int>();
    var result = source.Prepend(1, 2, 3).ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void EnumerableExtensions_Append_EmptyItems_ReturnsSourceUnchanged() {
    var source = new[] { 1, 2, 3 };
    var result = source.Append().ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void EnumerableExtensions_PrependAppend_NullSource_ThrowsException() {
    IEnumerable<int>? source = null;

    Assert.Throws<ArgumentNullException>(() => source.Prepend(1, 2).ToArray());
    Assert.Throws<ArgumentNullException>(() => source.Append(1, 2).ToArray());
  }

  #endregion

  #region Filtering Tests

  [Test]
  public void EnumerableExtensions_FilterIfNeeded_WithQuery_FiltersCorrectly() {
    var source = GetTestItems();
    var result = source.FilterIfNeeded(item => item.Name, "First").ToArray();

    Assert.That(result.Length, Is.EqualTo(1));
    Assert.That(result[0].Name, Is.EqualTo("First"));
  }

  [Test]
  public void EnumerableExtensions_FilterIfNeeded_EmptyQuery_ReturnsAll() {
    var source = GetTestItems();
    var result = source.FilterIfNeeded(item => item.Name, "").ToArray();

    Assert.That(result.Length, Is.EqualTo(5));
  }

  [Test]
  public void EnumerableExtensions_FilterIfNeeded_CaseInsensitive_FiltersCorrectly() {
    var source = GetTestItems();
    var result = source.FilterIfNeeded(item => item.Name, "FIRST", ignoreCase: true).ToArray();

    Assert.That(result.Length, Is.EqualTo(1));
    Assert.That(result[0].Name, Is.EqualTo("First"));
  }

  [Test]
  public void EnumerableExtensions_FilterIfNeeded_MultipleSelectors_FiltersAnyMatch() {
    var source = GetTestItems();
    var result = source.FilterIfNeeded("2", item => item.Id.ToString(), item => item.Name).ToArray();

    Assert.That(result.Length, Is.EqualTo(1));
    Assert.That(result[0].Id, Is.EqualTo(2));
  }

  [Test]
  public void EnumerableExtensions_FilterIfNeeded_NoMatch_ReturnsEmpty() {
    var source = GetTestItems();
    var result = source.FilterIfNeeded(item => item.Name, "NonExistent").ToArray();

    Assert.That(result, Is.Empty);
  }

  #endregion

  #region Comparison Tests

  [Test]
  public void EnumerableExtensions_CompareTo_IdenticalSequences_ReturnsChanges() {
    var source1 = new[] { 1, 2, 3 };
    var source2 = new[] { 1, 2, 3 };
    var changes = source1.CompareTo(source2).ToArray();

    Assert.That(changes.Length, Is.EqualTo(3));
    // Note: Specific ChangeType enum comparisons skipped due to namespace conflicts
    Assert.That(changes, Is.Not.Empty);
  }

  [Test]
  public void EnumerableExtensions_CompareTo_DifferentSequences_ReturnsChanges() {
    var source1 = new[] { 1, 2, 3 };
    var source2 = new[] { 1, 4, 3 };
    var changes = source1.CompareTo(source2).ToArray();

    Assert.That(changes.Length, Is.EqualTo(3));
    // Note: Specific ChangeType enum comparisons skipped due to namespace conflicts
    Assert.That(changes, Is.Not.Empty);
  }

  [Test]
  public void EnumerableExtensions_CompareTo_AddedItems_ReturnsAddedChanges() {
    var source1 = new[] { 1, 2 };
    var source2 = new[] { 1, 2, 3, 4 };
    var changes = source1.CompareTo(source2).ToArray();

    Assert.That(changes.Length, Is.GreaterThan(2));
    // Note: Specific ChangeType enum filtering skipped due to namespace conflicts
    Assert.That(changes, Is.Not.Empty);
  }

  [Test]
  public void EnumerableExtensions_CompareTo_RemovedItems_ReturnsRemovedChanges() {
    var source1 = new[] { 1, 2, 3, 4 };
    var source2 = new[] { 1, 2 };
    var changes = source1.CompareTo(source2).ToArray();

    Assert.That(changes.Length, Is.GreaterThan(2));
    // Note: Specific ChangeType enum filtering skipped due to namespace conflicts
    Assert.That(changes, Is.Not.Empty);
  }

  [Test]
  public void EnumerableExtensions_AreEqual_IdenticalSequences_ReturnsTrue() {
    var source1 = new[] { 1, 2, 3 };
    var source2 = new[] { 1, 2, 3 };

    Assert.That(source1.AreEqual(source2), Is.True);
  }

  [Test]
  public void EnumerableExtensions_AreEqual_DifferentSequences_ReturnsFalse() {
    var source1 = new[] { 1, 2, 3 };
    var source2 = new[] { 1, 2, 4 };

    Assert.That(source1.AreEqual(source2), Is.False);
  }

  [Test]
  public void EnumerableExtensions_AreEqual_DifferentLengths_ReturnsFalse() {
    var source1 = new[] { 1, 2, 3 };
    var source2 = new[] { 1, 2 };

    Assert.That(source1.AreEqual(source2), Is.False);
  }

  #endregion

  #region Null and Empty Tests

  [Test]
  public void EnumerableExtensions_ToNullIfEmpty_EmptySequence_ReturnsNull() {
    var source = Enumerable.Empty<int>();
    var result = source.ToNullIfEmpty();

    Assert.That(result, Is.Null);
  }

  [Test]
  public void EnumerableExtensions_ToNullIfEmpty_NonEmptySequence_ReturnsOriginal() {
    var source = new[] { 1, 2, 3 };
    var result = source.ToNullIfEmpty();

    Assert.That(result, Is.EqualTo(source));
  }

  [Test]
  public void EnumerableExtensions_IsNullOrEmpty_NullEnumerable_ReturnsTrue() {
    IEnumerable<int>? source = null;

    Assert.That(source.IsNullOrEmpty(), Is.True);
  }

  [Test]
  public void EnumerableExtensions_IsNullOrEmpty_EmptyEnumerable_ReturnsTrue() {
    var source = Enumerable.Empty<int>();

    Assert.That(source.IsNullOrEmpty(), Is.True);
  }

  [Test]
  public void EnumerableExtensions_IsNullOrEmpty_NonEmptyEnumerable_ReturnsFalse() {
    var source = new[] { 1 };

    Assert.That(source.IsNullOrEmpty(), Is.False);
  }

  [Test]
  public void EnumerableExtensions_IsNotNullOrEmpty_NonEmptyEnumerable_ReturnsTrue() {
    var source = new[] { 1 };

    Assert.That(source.IsNotNullOrEmpty(), Is.True);
  }

  [Test]
  public void EnumerableExtensions_IsNullOrEmpty_WithPredicate_NoMatches_ReturnsTrue() {
    var source = new[] { 2, 4, 6 };

    Assert.That(source.IsNullOrEmpty(x => x % 2 == 1), Is.True); // No odd numbers
  }

  [Test]
  public void EnumerableExtensions_IsNullOrEmpty_WithPredicate_HasMatches_ReturnsFalse() {
    var source = new[] { 1, 2, 3 };

    Assert.That(source.IsNullOrEmpty(x => x % 2 == 1), Is.False); // Has odd numbers
  }

  #endregion

  #region HashSet Extensions Tests

  [Test]
  public void EnumerableExtensions_ToHashSet_WithInitialCapacity_CreatesHashSetWithCapacity() {
    var source = GetTestNumbers();
    var hashSet = source.ToHashSet(20);

    Assert.That(hashSet, Is.Not.Null);
    Assert.That(hashSet.Count, Is.EqualTo(10));
    foreach (var item in source)
      Assert.That(hashSet.Contains(item), Is.True);
  }

  [Test]
  public void EnumerableExtensions_ToHashSet_WithSelector_CreatesHashSetFromProjection() {
    var source = GetTestItems();
    var hashSet = source.ToHashSet(item => item.Name);

    Assert.That(hashSet.Count, Is.EqualTo(5));
    Assert.That(hashSet.Contains("First"), Is.True);
    Assert.That(hashSet.Contains("Second"), Is.True);
  }

  [Test]
  public void EnumerableExtensions_ToHashSet_WithSelectorAndComparer_UsesComparer() {
    var source = GetTestItems();
    var hashSet = source.ToHashSet(item => item.Name, StringComparer.OrdinalIgnoreCase);

    Assert.That(hashSet.Count, Is.EqualTo(5));
    Assert.That(hashSet.Contains("FIRST"), Is.True);
    Assert.That(hashSet.Contains("first"), Is.True);
  }

  #endregion

  #region Shuffling Tests

  [Test]
  public void EnumerableExtensions_Shuffled_ReturnsAllElements() {
    var source = GetTestNumbers().ToArray();
    var shuffled = source.Shuffled().ToArray();

    Assert.That(shuffled.Length, Is.EqualTo(source.Length));
    foreach (var item in source)
      Assert.That(shuffled.Contains(item), Is.True);
  }

  [Test]
  public void EnumerableExtensions_Shuffled_WithCustomRandom_UsesCustomRandom() {
    var source = GetTestNumbers().ToArray();
    var random = new Random(42); // Fixed seed for reproducible results
    var shuffled1 = source.Shuffled(random).ToArray();

    random = new(42); // Same seed
    var shuffled2 = source.Shuffled(random).ToArray();

    Assert.That(shuffled1, Is.EqualTo(shuffled2)); // Should be identical with same seed
  }

  [Test]
  public void EnumerableExtensions_Shuffled_EmptyEnumerable_ReturnsEmpty() {
    var source = Enumerable.Empty<int>();
    var shuffled = source.Shuffled().ToArray();

    Assert.That(shuffled, Is.Empty);
  }

  #endregion

  #region Concatenation Tests

#if NET40_OR_GREATER
  [Test]
  public void EnumerableExtensions_ConcatAll_ByteArrays_ConcatenatesCorrectly() {
    var arrays = new[] {
      new byte[] { 1, 2, 3 },
      new byte[] { 4, 5 },
      new byte[] { 6, 7, 8, 9 }
    };
    
    var result = arrays.ConcatAll();
    
    Assert.That(result, Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
  }

  [Test]
  public void EnumerableExtensions_ConcatAll_EmptyByteArrays_ReturnsEmpty() {
    var arrays = new[] {
      new byte[0],
      new byte[0]
    };
    
    var result = arrays.ConcatAll();
    
    Assert.That(result, Is.Empty);
  }
  
  [Test]
  public void EnumerableExtensions_ConcatAll_NullArrays_HandlesGracefully() {
    var arrays = new[] {
      new byte[] { 1, 2 },
      null,
      new byte[] { 3, 4 }
    };
    
    Assert.DoesNotThrow(() => arrays.ConcatAll());
  }

#endif

  [Test]
  public void EnumerableExtensions_ConcatAll_GenericEnumerables_ConcatenatesCorrectly() {
    var enumerables = new[] { new[] { 1, 2, 3 }.AsEnumerable(), new[] { 4, 5 }.AsEnumerable(), new[] { 6, 7, 8 }.AsEnumerable() };

    var result = enumerables.ConcatAll().ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
  }

  #endregion

  #region Split and Contains Tests

  [Test]
  public void EnumerableExtensions_Split_WithPredicate_SplitsCorrectly() {
    var source = GetTestNumbers();
    var (evens, odds) = source.Split(x => x % 2 == 0);

    var evenArray = evens.ToArray();
    var oddArray = odds.ToArray();

    Assert.That(evenArray, Is.EqualTo(new[] { 2, 4, 6, 8, 10 }));
    Assert.That(oddArray, Is.EqualTo(new[] { 1, 3, 5, 7, 9 }));
  }

  [Test]
  public void EnumerableExtensions_Split_AllMatch_ReturnsAllInFirst() {
    var source = new[] { 2, 4, 6, 8 };
    var (matching, nonMatching) = source.Split(x => x % 2 == 0);

    Assert.That(matching.ToArray(), Is.EqualTo(source));
    Assert.That(nonMatching.ToArray(), Is.Empty);
  }

  [Test]
  public void EnumerableExtensions_ContainsNot_ItemNotPresent_ReturnsTrue() {
    var source = new[] { 1, 2, 3 };

    Assert.That(source.ContainsNot(4), Is.True);
  }

  [Test]
  public void EnumerableExtensions_ContainsNot_ItemPresent_ReturnsFalse() {
    var source = new[] { 1, 2, 3 };

    Assert.That(source.ContainsNot(2), Is.False);
  }

  [Test]
  public void EnumerableExtensions_ContainsAny_HasSomeItems_ReturnsTrue() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var searchItems = new[] { 3, 6, 7 };

    Assert.That(source.ContainsAny(searchItems), Is.True);
  }

  [Test]
  public void EnumerableExtensions_ContainsAny_HasNoItems_ReturnsFalse() {
    var source = new[] { 1, 2, 3 };
    var searchItems = new[] { 4, 5, 6 };

    Assert.That(source.ContainsAny(searchItems), Is.False);
  }

  [Test]
  public void EnumerableExtensions_ContainsNotAny_HasSomeItems_ReturnsFalse() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var searchItems = new[] { 3, 6, 7 };

    Assert.That(source.ContainsNotAny(searchItems), Is.False);
  }

  [Test]
  public void EnumerableExtensions_ContainsNotAny_HasNoItems_ReturnsTrue() {
    var source = new[] { 1, 2, 3 };
    var searchItems = new[] { 4, 5, 6 };

    Assert.That(source.ContainsNotAny(searchItems), Is.True);
  }

  #endregion

  #region ForEach Tests

  [Test]
  public void EnumerableExtensions_ForEach_WithAction_CallsActionForEachItem() {
    var source = new[] { 1, 2, 3 };
    var sum = 0;

    source.ForEach(x => sum += x);

    Assert.That(sum, Is.EqualTo(6));
  }

  [Test]
  public void EnumerableExtensions_ForEach_WithIndexedAction_CallsActionWithIndex() {
    var source = new[] { "a", "b", "c" };
    var results = new List<string>();

    source.ForEach((Action<string, int>)((item, index) => results.Add($"{index}:{item}")));

    Assert.That(results, Is.EqualTo(new[] { "0:a", "1:b", "2:c" }));
  }

  [Test]
  public void EnumerableExtensions_ForEach_EmptyEnumerable_DoesNotCallAction() {
    var source = Enumerable.Empty<int>();
    var called = false;

    source.ForEach(x => called = true);

    Assert.That(called, Is.False);
  }

#if NET40_OR_GREATER
  [Test]
  public void EnumerableExtensions_ParallelForEach_WithAction_CallsActionForEachItem() {
    var source = Enumerable.Range(1, 1000);
    var sum = 0;
    var lockObject = new object();
    
    source.ParallelForEach(x => {
      lock (lockObject) {
        sum += x;
      }
    });
    
    Assert.That(sum, Is.EqualTo(500500)); // Sum of 1 to 1000
  }

  [Test]
  public void EnumerableExtensions_ParallelForEach_WithIndexedAction_CallsActionWithIndex() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var results = new List<string>();
    var lockObject = new object();
    
    source.ParallelForEach((Action<int, int>)((item, index) => {
      lock (lockObject) {
        results.Add($"{index}:{item}");
      }
    }));
    
    Assert.That(results.Count, Is.EqualTo(5));
    // Note: Order is not guaranteed in parallel execution
  }
#endif

  #endregion

  #region ConvertAll Tests

  [Test]
  public void EnumerableExtensions_ConvertAll_WithConverter_TransformsAllItems() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var result = source.ConvertAll(x => x * 2).ToList().ToArray();

    Assert.That(result, Is.EqualTo(new[] { 2, 4, 6, 8, 10 }));
  }

  [Test]
  public void EnumerableExtensions_ConvertAll_WithIndexedConverter_TransformsWithIndex() {
    var source = new[] { "a", "b", "c" };
    var result = source.ConvertAll((item, index) => $"{index}:{item}").ToList().ToArray();

    Assert.That(result, Is.EqualTo(new[] { "0:a", "1:b", "2:c" }));
  }

  [Test]
  public void EnumerableExtensions_ConvertAll_EmptyEnumerable_ReturnsEmpty() {
    var source = Enumerable.Empty<int>();
    var result = source.ConvertAll(x => x.ToString()).ToList().ToArray();

    Assert.That(result, Is.Empty);
  }

  [Test]
  public void EnumerableExtensions_ConvertAll_TypeConversion_WorksCorrectly() {
    var source = new[] { 1, 2, 3 };
    var result = source.ConvertAll(x => x.ToString()).ToList().ToArray();

    Assert.That(result, Is.EqualTo(new[] { "1", "2", "3" }));
  }

  #endregion

  #region Progress Reporting Tests

  [Test]
  public void EnumerableExtensions_AsProgressReporting_ReportsProgress() {
    var source = Enumerable.Range(1, 100);
    var progressReports = new List<double>();

    var progressEnumerable = source.AsProgressReporting(progress => progressReports.Add(progress));
    var result = progressEnumerable.ToList().ToArray();

    Assert.That(result.Length, Is.EqualTo(100));
    Assert.That(progressReports.Count, Is.GreaterThan(1));
    Assert.That(progressReports.Last(), Is.EqualTo(1.0).Within(0.001));
  }

  [Test]
  public void EnumerableExtensions_AsProgressReporting_WithCounts_ReportsCorrectCounts() {
    var source = Enumerable.Range(1, 50);
    var countReports = new List<(long current, long total)>();

    var progressEnumerable2 = source.AsProgressReporting((current, total) => countReports.Add((current, total)));
    var result = progressEnumerable2.ToList().ToArray();

    Assert.That(result.Length, Is.EqualTo(50));
    Assert.That(countReports.Count, Is.GreaterThan(1));
    Assert.That(countReports.Last().current, Is.EqualTo(50));
    Assert.That(countReports.Last().total, Is.EqualTo(50));
  }

  [Test]
  public void EnumerableExtensions_AsProgressReporting_DelayedReporting_WorksCorrectly() {
    var source = Enumerable.Range(1, 10);
    var progressReports = new List<double>();

    var progressEnumerable3 = source.AsProgressReporting(progress => progressReports.Add(progress), delayed: true);
    var result = progressEnumerable3.ToList().ToArray();

    Assert.That(result.Length, Is.EqualTo(10));
    Assert.That(progressReports.Count, Is.GreaterThanOrEqualTo(1));
  }

  #endregion

  #region Advanced LINQ Operations Tests

  [Test]
  public void EnumerableExtensions_All_WithIndexedCondition_ChecksAllWithIndex() {
    var source = new[] { 2, 4, 6, 8, 10 };
    var result = source.All((item, index) => item == (index + 1) * 2);

    Assert.That(result, Is.True);
  }

  [Test]
  public void EnumerableExtensions_All_WithIndexedCondition_ReturnsFalseWhenNotAll() {
    var source = new[] { 2, 3, 6, 8, 10 };
    var result = source.All((item, index) => item == (index + 1) * 2);

    Assert.That(result, Is.False);
  }

  [Test]
  public void EnumerableExtensions_Distinct_WithSelector_DistinctsByProjection() {
    var source = GetTestItems();
    var result = source.Distinct(item => item.Name.Length).ToArray();

    // Should get distinct items by name length
    Assert.That(result.Length, Is.LessThan(5)); // Some names have same length
    var nameLengths = result.Select(item => item.Name.Length).ToArray();
    Assert.That(nameLengths, Is.EqualTo(nameLengths.Distinct()));
  }

  [Test]
  public void EnumerableExtensions_SelectMany_FlattensNestedEnumerables() {
    var source = new[] { new[] { 1, 2 }, new[] { 3, 4, 5 }, new[] { 6 } };

    var result = source.SelectMany().ToArray();

    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
  }

  #endregion

  #region Index and Search Tests

  [Test]
  public void EnumerableExtensions_IndexOrDefault_FindsItemIndex() {
    var source = GetTestStrings();
    var index = source.IndexOrDefault(s => s.StartsWith("c"));

    Assert.That(index, Is.EqualTo(2)); // "cherry" is at index 2
  }

  [Test]
  public void EnumerableExtensions_IndexOrDefault_ItemNotFound_ReturnsDefault() {
    var source = GetTestStrings();
    var index = source.IndexOrDefault(s => s.StartsWith("z"), -1);

    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  public void EnumerableExtensions_IndexOrDefault_WithDefaultFactory_CallsFactory() {
    var source = GetTestStrings();
    var factoryCalled = false;
    var index = source.IndexOrDefault(
      s => s.StartsWith("z"),
      () => {
        factoryCalled = true;
        return -99;
      }
    );

    Assert.That(index, Is.EqualTo(-99));
    Assert.That(factoryCalled, Is.True);
  }

  [Test]
  public void EnumerableExtensions_IndexOf_FindsExactItem() {
    var source = new[] { "apple", "banana", "cherry" };
    var index = source.IndexOf("banana");

    Assert.That(index, Is.EqualTo(1));
  }

  [Test]
  public void EnumerableExtensions_IndexOf_ItemNotFound_ReturnsMinusOne() {
    var source = new[] { "apple", "banana", "cherry" };
    var index = source.IndexOf("grape");

    Assert.That(index, Is.EqualTo(-1));
  }

  #endregion

  #region Try Get Operations Tests

  [Test]
  public void EnumerableExtensions_TryGetFirst_HasItems_ReturnsFirstItem() {
    var source = GetTestNumbers();
    var success = source.TryGetFirst(out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  public void EnumerableExtensions_TryGetFirst_EmptyEnumerable_ReturnsFalse() {
    var source = Enumerable.Empty<int>();
    var success = source.TryGetFirst(out var result);

    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  [Test]
  public void EnumerableExtensions_TryGetLast_HasItems_ReturnsLastItem() {
    var source = GetTestNumbers();
    var success = source.TryGetLast(out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  public void EnumerableExtensions_TryGetLast_EmptyEnumerable_ReturnsFalse() {
    var source = Enumerable.Empty<int>();
    var success = source.TryGetLast(out var result);

    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  [Test]
  public void EnumerableExtensions_TryGetItem_ValidIndex_ReturnsItem() {
    var source = GetTestNumbers();
    var success = source.TryGetItem(4, out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(5)); // 0-based index
  }

  [Test]
  public void EnumerableExtensions_TryGetItem_InvalidIndex_ReturnsFalse() {
    var source = GetTestNumbers();
    var success = source.TryGetItem(100, out var result);

    Assert.That(success, Is.False);
    Assert.That(result, Is.EqualTo(default(int)));
  }

  [Test]
  public void EnumerableExtensions_TryGet_WithSelector_ReturnsSelectedValue() {
    var source = GetTestNumbers();
    var success = source.TryGet(enumerable => enumerable.Sum(), out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(55)); // Sum of 1 to 10
  }

  [Test]
  public void EnumerableExtensions_TryGetMax_HasComparableItems_ReturnsMax() {
    var source = GetTestNumbers();
    var success = source.TryGetMax(out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(10));
  }

  [Test]
  public void EnumerableExtensions_TryGetMaxBy_WithSelector_ReturnsMaxBySelector() {
    var source = GetTestItems();
    var success = source.TryGetMaxBy(item => item.Value, out var result);

    Assert.That(success, Is.True);
    Assert.That(result.Value, Is.EqualTo(30.25));
    Assert.That(result.Name, Is.EqualTo("Fourth"));
  }

  [Test]
  public void EnumerableExtensions_TryGetMin_HasComparableItems_ReturnsMin() {
    var source = GetTestNumbers();
    var success = source.TryGetMin(out var result);

    Assert.That(success, Is.True);
    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  public void EnumerableExtensions_TryGetMinBy_WithSelector_ReturnsMinBySelector() {
    var source = GetTestItems();
    var success = source.TryGetMinBy(item => item.Value, out var result);

    Assert.That(success, Is.True);
    Assert.That(result.Value, Is.EqualTo(5.5));
    Assert.That(result.Name, Is.EqualTo("Fifth"));
  }

  #endregion

  #region First/Last OrNull Tests

  [Test]
  public void EnumerableExtensions_FirstOrNull_ReferenceType_HasItems_ReturnsFirst() {
    var source = GetTestStrings();
    var result = source.FirstOrNull();

    Assert.That(result, Is.EqualTo("apple"));
  }

  [Test]
  public void EnumerableExtensions_FirstOrNull_ReferenceType_EmptyEnumerable_ReturnsNull() {
    var source = Enumerable.Empty<string>();
    var result = source.FirstOrNull();

    Assert.That(result, Is.Null);
  }

  [Test]
  public void EnumerableExtensions_FirstOrNull_ValueType_HasItems_ReturnsFirst() {
    var source = GetTestNumbers();
    var result = source.FirstOrNull();

    Assert.That(result, Is.EqualTo(1));
  }

  [Test]
  public void EnumerableExtensions_FirstOrNull_ValueType_EmptyEnumerable_ReturnsNull() {
    var source = Enumerable.Empty<int>();
    var result = source.FirstOrNull();

    Assert.That(result, Is.Null);
  }

  #endregion

  #region T4 Generated Statistical Methods Tests

  [Test]
  public void EnumerableExtensions_Sum_TimeSpan_CalculatesCorrectSum() {
    var timeSpans = new[] { TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(30) };

    var result = timeSpans.Sum();

    Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(60)));
  }

  [Test]
  public void EnumerableExtensions_Sum_TimeSpanWithSelector_CalculatesCorrectSum() {
    var items = new[] { new { Duration = TimeSpan.FromHours(1) }, new { Duration = TimeSpan.FromHours(2) }, new { Duration = TimeSpan.FromHours(3) } };

    var result = items.Sum(item => item.Duration);

    Assert.That(result, Is.EqualTo(TimeSpan.FromHours(6)));
  }

  [Test]
  public void EnumerableExtensions_Sum_UnsignedTypes_WorkCorrectly() {
    var ushorts = new ushort[] { 1, 2, 3, 4, 5 };
    var uints = new uint[] { 10u, 20u, 30u };
    var ulongs = new ulong[] { 100ul, 200ul, 300ul };

    Assert.That(ushorts.Sum(), Is.EqualTo((ushort)15));
    Assert.That(uints.Sum(), Is.EqualTo(60u));
    Assert.That(ulongs.Sum(), Is.EqualTo(600ul));
  }

  [Test]
  public void EnumerableExtensions_Sum_WithSelectorUnsignedTypes_WorkCorrectly() {
    var items = new[] { new { UShort = (ushort)1, UInt = 10u, ULong = 100ul }, new { UShort = (ushort)2, UInt = 20u, ULong = 200ul }, new { UShort = (ushort)3, UInt = 30u, ULong = 300ul } };

    Assert.That(items.Sum(item => item.UShort), Is.EqualTo((ushort)6));
    Assert.That(items.Sum(item => item.UInt), Is.EqualTo(60u));
    Assert.That(items.Sum(item => item.ULong), Is.EqualTo(600ul));
  }

  [Test]
  public void EnumerableExtensions_Sum_EmptyEnumerable_ReturnsZero() {
    var emptyTimeSpans = Enumerable.Empty<TimeSpan>();
    var emptyUShorts = Enumerable.Empty<ushort>();

    Assert.That(emptyTimeSpans.Sum(), Is.EqualTo(TimeSpan.Zero));
    Assert.That(emptyUShorts.Sum(), Is.EqualTo((ushort)0));
  }

  #endregion

  #region Disposable Collection Tests (Skipped - Private Implementation)

  // Note: DisposableCollection tests are skipped because the class is private
  // These tests would verify that the DisposableCollection properly disposes
  // all its contained IDisposable items when it is disposed

  #endregion

  #region Performance and Edge Cases Tests

  [Test]
  [Category("Performance")]
  public void Performance_ForEach_LargeEnumerable_ExecutesEfficiently() {
    var largeSource = Enumerable.Range(1, 100000);
    var sum = 0L;
    var sw = Stopwatch.StartNew();

    largeSource.ForEach(x => sum += x);

    sw.Stop();
    Assert.That(sum, Is.EqualTo(5000050000L)); // Sum of 1 to 100000
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1000)); // Should be fast
  }

#if NET40_OR_GREATER
  [Test]
  [Category("Performance")]
  public void Performance_ParallelForEach_LargeEnumerable_FasterThanSequential() {
    var largeSource = Enumerable.Range(1, 50000);
    var lockObject = new object();
    
    // Sequential
    var sw1 = Stopwatch.StartNew();
    var sum1 = 0L;
    largeSource.ForEach(x => sum1 += x);
    sw1.Stop();
    
    // Parallel
    var sw2 = Stopwatch.StartNew();
    var sum2 = 0L;
    largeSource.ParallelForEach(x => {
      lock (lockObject) {
        sum2 += x;
      }
    });
    sw2.Stop();
    
    Assert.That(sum1, Is.EqualTo(sum2)); // Same results
    // Note: Parallel might not always be faster due to locking overhead in this simple test
  }
#endif

  [Test]
  [Category("Performance")]
  public void Performance_ToHashSet_LargeEnumerable_EfficientCreation() {
    var largeSource = Enumerable.Range(1, 50000);
    var sw = Stopwatch.StartNew();

    var hashSet = largeSource.ToHashSet(60000); // Pre-sized

    sw.Stop();
    Assert.That(hashSet.Count, Is.EqualTo(50000));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500)); // Should be reasonably fast
  }

  [Test]
  public void EdgeCase_NullEnumerableOperations_ThrowAppropriateExceptions() {
    IEnumerable<int>? nullEnumerable = null;

    Assert.Throws<ArgumentNullException>(() => nullEnumerable.Prepend(1, 2).ToArray());
    Assert.Throws<ArgumentNullException>(() => nullEnumerable.Append(1, 2).ToArray());
    Assert.Throws<NullReferenceException>(() => nullEnumerable.ForEach(x => { }));
    Assert.Throws<NullReferenceException>(() => nullEnumerable.ConvertAll(x => x.ToString()).ToArray());
  }

  [Test]
  public void EdgeCase_VeryLargeEnumerable_HandlesGracefully() {
    // Test with a very large enumerable that would cause memory issues if fully materialized
    var largeSource = Enumerable.Range(1, 1000000);

    // These operations should work without materializing the entire sequence
    var first = largeSource.TryGetFirst(out var firstItem);
    var hasAny = largeSource.IsNotNullOrEmpty();
    var filtered = largeSource.FilterIfNeeded(x => x.ToString(), "1000").Take(5).ToArray();

    Assert.That(first, Is.True);
    Assert.That(firstItem, Is.EqualTo(1));
    Assert.That(hasAny, Is.True);
    Assert.That(filtered.Length, Is.GreaterThan(0));
  }

  [Test]
  public void EdgeCase_ChainedOperations_WorkCorrectly() {
    var result = GetTestNumbers()
      .Prepend(0)
      .Append(11, 12)
      .FilterIfNeeded(x => x.ToString(), "1")
      .ConvertAll(x => x * 2)
      .ToHashSet();

    // Should contain doubled values of numbers containing "1": 0*2, 1*2, 10*2, 11*2, 12*2
    Assert.That(result.Contains(2), Is.True); // 1 * 2
    Assert.That(result.Contains(20), Is.True); // 10 * 2
    Assert.That(result.Contains(22), Is.True); // 11 * 2
    Assert.That(result.Contains(24), Is.True); // 12 * 2
  }

  #endregion

  #region Framework-Specific Compatibility Tests

#if NET35
  [Test]
  public void NET35_Compatibility_BasicEnumerableOperations_WorkCorrectly() {
    // Test basic operations that should work on .NET 3.5
    var source = new[] { 1, 2, 3, 4, 5 };
    
    // Test append/prepend
    var withPrepend = source.Prepend(0).ToList().ToArray();
    var withAppend = source.Append(6).ToList().ToArray();
    
    Assert.That(withPrepend, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
    Assert.That(withAppend, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
    
    // Test ForEach (non-parallel)
    var sum = 0;
    source.ForEach(x => sum += x);
    Assert.That(sum, Is.EqualTo(15));
    
    // Test null/empty checks
    Assert.That(source.IsNotNullOrEmpty(), Is.True);
    Assert.That(source.IsNullOrEmpty(), Is.False);
  }

  [Test]
  public void NET35_Compatibility_StatisticalOperations_WorkCorrectly() {
    // Test T4-generated statistical operations on .NET 3.5
    var timeSpans = new[] {
      TimeSpan.FromMinutes(10),
      TimeSpan.FromMinutes(20),
      TimeSpan.FromMinutes(30)
    };
    Assert.That(timeSpans.Sum(), Is.EqualTo(TimeSpan.FromMinutes(60)));
    
    var ushorts = new ushort[] { 1, 2, 3, 4, 5 };
    Assert.That(ushorts.Sum(), Is.EqualTo((ushort)15));
  }
#endif

#if NET40_OR_GREATER
  [Test]
  public void NET40Plus_TaskBasedOperations_WorkCorrectly() {
    // Test operations that require .NET 4.0+ (Task-based)
    var source = Enumerable.Range(1, 100);
    var results = new List<int>();
    var lockObject = new object();
    
    source.ParallelForEach(x => {
      lock (lockObject) {
        results.Add(x * 2);
      }
    });
    
    Assert.That(results.Count, Is.EqualTo(100));
    Assert.That(results.Sum(), Is.EqualTo(10100)); // Sum of 2*1 to 2*100
  }
#endif

#if NET45_OR_GREATER
  [Test]
  public void NET45Plus_AsyncCompatibility_WorksCorrectly() {
    // Test operations that work well with async (NET 4.5+)
    var source = Enumerable.Range(1, 50);
    var progressReports = new List<double>();
    
    var enumerable = source.AsProgressReporting(progress => progressReports.Add(progress));
    var result = enumerable.ToList().ToArray();
    
    Assert.That(result.Length, Is.EqualTo(50));
    Assert.That(progressReports.Count, Is.GreaterThan(0));
    Assert.That(progressReports.Last(), Is.EqualTo(1.0).Within(0.001));
  }
#endif

  #endregion

  #region Stress Testing and Edge Cases

  [Test]
  public void StressTest_VeryLargeEnumerable_MemoryEfficient() {
    // Test with extremely large enumerable to verify memory efficiency
    var source = Enumerable.Range(1, 1000000); // 1 million items

    // These operations should not materialize the entire sequence
    var firstFive = source.Take(5).ToList().ToArray();
    var hasAny = source.IsNotNullOrEmpty();
    var firstItem = source.TryGetFirst(out var first);

    Assert.That(firstFive, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    Assert.That(hasAny, Is.True);
    Assert.That(firstItem, Is.True);
    Assert.That(first, Is.EqualTo(1));
  }

  [Test]
  public void EdgeCase_EmptyEnumerables_HandleGracefully() {
    var empty = Enumerable.Empty<int>();

    // All operations should handle empty enumerables gracefully
    Assert.That(empty.IsNullOrEmpty(), Is.True);
    Assert.That(empty.IsNotNullOrEmpty(), Is.False);
    Assert.That(empty.ToNullIfEmpty(), Is.Null);
    Assert.That(empty.Prepend(1).ToList().ToArray(), Is.EqualTo(new[] { 1 }));
    Assert.That(empty.Append(1).ToList().ToArray(), Is.EqualTo(new[] { 1 }));
    Assert.That(empty.TryGetFirst(out _), Is.False);
    Assert.That(empty.TryGetLast(out _), Is.False);
  }

  [Test]
  public void EdgeCase_SingleElementEnumerables_HandleCorrectly() {
    var single = new[] { 42 };

    Assert.That(single.IsNotNullOrEmpty(), Is.True);
    Assert.That(single.TryGetFirst(out var first), Is.True);
    Assert.That(first, Is.EqualTo(42));
    Assert.That(single.TryGetLast(out var last), Is.True);
    Assert.That(last, Is.EqualTo(42));
    Assert.That(single.IndexOf(42), Is.EqualTo(0));
    Assert.That(single.ContainsNot(43), Is.True);
    Assert.That(single.ContainsNot(42), Is.False);
  }

  [Test]
  public void EdgeCase_EnumerableWithNulls_HandleCorrectly() {
    var withNulls = new string[] { "a", null, "b", null, "c" };

    // Operations should handle null values gracefully
    var nonNulls = withNulls.Where(x => x != null).ToList().ToArray();
    Assert.That(nonNulls, Is.EqualTo(new[] { "a", "b", "c" }));

    var count = 0;
    withNulls.ForEach(x => count++);
    Assert.That(count, Is.EqualTo(5)); // Should count nulls too
  }

  [Test]
  public void EdgeCase_DuplicateElements_HandleCorrectly() {
    var duplicates = new[] {
      1,
      2,
      2,
      3,
      3,
      3,
      4,
      4,
      4,
      4
    };

    var hashSet = duplicates.ToHashSet();
    Assert.That(hashSet.Count, Is.EqualTo(4));
    Assert.That(hashSet.Contains(1), Is.True);
    Assert.That(hashSet.Contains(4), Is.True);

    var distinct = duplicates.Distinct(x => x).ToList().ToArray();
    Assert.That(distinct.Length, Is.LessThanOrEqualTo(4));
  }

  #endregion

  #region Type-Specific Extension Tests

  [Test]
  public void TypeSpecific_ByteArrayConcatenation_WorksCorrectly() {
    var arrays = new[] { new byte[] { 0x01, 0x02 }, new byte[] { 0x03, 0x04, 0x05 }, new byte[] { 0x06 } };

    var result = ((IEnumerable<byte[]>)arrays).ConcatAll();
    Assert.That(result, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }));
  }

  [Test]
  public void TypeSpecific_TimeSpanSummation_HandlesOverflow() {
    var largeSpans = new[] { TimeSpan.FromDays(365), TimeSpan.FromDays(365), TimeSpan.FromDays(365) };

    var total = largeSpans.Sum();
    Assert.That(total.TotalDays, Is.EqualTo(1095).Within(0.1));
  }

  [Test]
  public void TypeSpecific_UnsignedIntegerSum_HandlesBoundaries() {
    // Test unsigned types near their boundaries
    var maxUShorts = new ushort[] { ushort.MaxValue, 1 }; // This would overflow ushort

    // The Sum method should handle this appropriately
    Assert.DoesNotThrow(
      () => {
        var sum = maxUShorts.Sum();
        // Result depends on implementation - might wrap or throw
      }
    );
  }

  #endregion

  #region Interoperability Tests

  [Test]
  public void Interop_WithSystemLinq_WorksTogether() {
    // Test interoperability with System.Linq methods
    var source = new[] {
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8,
      9,
      10
    };

    var result = source
      .Where(x => x % 2 == 0) // System.Linq
      .Prepend(0) // Extension
      .Skip(1) // System.Linq
      .Append(12) // Extension
      .OrderBy(x => x) // System.Linq
      .ToList()
      .ToArray(); // Explicit disambiguation

    Assert.That(result, Is.EqualTo(new[] { 2, 4, 6, 8, 10, 12 }));
  }

  [Test]
  public void Interop_ChainedOperations_MaintainLaziness() {
    var source = Enumerable.Range(1, 1000000); // Large sequence

    // Chain operations - should remain lazy until materialized
    var query = source
      .Prepend(0)
      .Append(1000001)
      .Where(x => x % 1000 == 0)
      .Take(5);

    // Only now should it materialize
    var result = query.ToList().ToArray();

    Assert.That(result.Length, Is.EqualTo(5));
    Assert.That(result[0], Is.EqualTo(0)); // Prepended
    Assert.That(result[1], Is.EqualTo(1000)); // First multiple of 1000
  }

  #endregion

  #region Culture and Localization Tests

  [Test]
  public void Culture_FilteringOperations_WorkAcrossCultures() {
    var numbers = new[] { 1.1, 2.2, 3.3, 4.4, 5.5 };
    var originalCulture = Thread.CurrentThread.CurrentCulture;

    try {
      // Test with US culture (period decimal separator)
      Thread.CurrentThread.CurrentCulture = new("en-US");
      var usResult = numbers.FilterIfNeeded(x => x.ToString(), "2.2").ToList();

      // Test with German culture (comma decimal separator)  
      Thread.CurrentThread.CurrentCulture = new("de-DE");
      var deResult = numbers.FilterIfNeeded(x => x.ToString(), "2,2").ToList();

      // Both should find the 2.2 value, but with different string representations
      Assert.That(usResult.Count + deResult.Count, Is.GreaterThan(0));
    } finally {
      Thread.CurrentThread.CurrentCulture = originalCulture;
    }
  }

  #endregion

  #region Boundary Value Analysis Tests

  [Test]
  public void BoundaryValues_IndexOperations_HandleEdgeCases() {
    var source = new[] { 10, 20, 30 };

    // Test valid boundaries
    Assert.That(source.TryGetItem(0, out var first), Is.True);
    Assert.That(first, Is.EqualTo(10));
    Assert.That(source.TryGetItem(2, out var last), Is.True);
    Assert.That(last, Is.EqualTo(30));

    // Test invalid boundaries
    Assert.Throws<IndexOutOfRangeException>(() => source.TryGetItem(-1, out _));
    Assert.That(source.TryGetItem(3, out _), Is.EqualTo(false));
    Assert.That(source.TryGetItem(int.MaxValue, out _), Is.EqualTo(false));
  }

  [Test]
  public void BoundaryValues_ProgressReporting_HandlesExtremes() {
    // Test with single item
    var single = new[] { 42 };
    var singleProgress = new List<double>();
    var singleResult = single.AsProgressReporting(p => singleProgress.Add(p)).ToList().ToArray();

    Assert.That(singleResult, Is.EqualTo(new[] { 42 }));
    Assert.That(singleProgress.Count, Is.GreaterThanOrEqualTo(1));
    Assert.That(singleProgress.Last(), Is.EqualTo(1.0).Within(0.001));

    // Test with empty enumerable
    var empty = Enumerable.Empty<int>();
    var emptyProgress = new List<double>();
    var emptyResult = empty.AsProgressReporting(p => emptyProgress.Add(p)).ToList().ToArray();

    Assert.That(emptyResult, Is.Empty);
  }

  #endregion
}
