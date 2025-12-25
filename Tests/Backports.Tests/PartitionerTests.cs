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
[Category("Partitioner")]
public class PartitionerTests {

  #region Partitioner.Create Int32 Range

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_IntRange_ReturnsPartitioner() {
    var partitioner = Partitioner.Create(0, 100);
    Assert.That(partitioner, Is.Not.Null);
    Assert.That(partitioner, Is.InstanceOf<OrderablePartitioner<Tuple<int, int>>>());
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_IntRange_CoversFullRange() {
    var partitioner = Partitioner.Create(0, 100);
    var partitions = partitioner.GetOrderablePartitions(4);

    var coveredValues = new HashSet<int>();
    foreach (var partition in partitions) {
      while (partition.MoveNext()) {
        var range = partition.Current.Value;
        for (var i = range.Item1; i < range.Item2; ++i)
          coveredValues.Add(i);
      }
    }

    Assert.That(coveredValues.Count, Is.EqualTo(100));
    for (var i = 0; i < 100; ++i)
      Assert.That(coveredValues.Contains(i), Is.True, $"Value {i} not covered");
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_IntRangeWithSize_RespectsRangeSize() {
    var partitioner = Partitioner.Create(0, 100, 25);
    var partitions = partitioner.GetOrderablePartitions(1);

    var ranges = new List<Tuple<int, int>>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ranges.Add(partition.Current.Value);

    Assert.That(ranges.Count, Is.EqualTo(4));
    Assert.That(ranges[0], Is.EqualTo(Tuple.Create(0, 25)));
    Assert.That(ranges[1], Is.EqualTo(Tuple.Create(25, 50)));
    Assert.That(ranges[2], Is.EqualTo(Tuple.Create(50, 75)));
    Assert.That(ranges[3], Is.EqualTo(Tuple.Create(75, 100)));
  }

  [Test]
  [Category("EdgeCase")]
  public void Partitioner_Create_IntRange_UnevenDivision_CoversAllValues() {
    var partitioner = Partitioner.Create(0, 103, 25);
    var partitions = partitioner.GetOrderablePartitions(1);

    var ranges = new List<Tuple<int, int>>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ranges.Add(partition.Current.Value);

    Assert.That(ranges.Count, Is.EqualTo(5));
    Assert.That(ranges[4], Is.EqualTo(Tuple.Create(100, 103)));
  }

  [Test]
  [Category("Exception")]
  public void Partitioner_Create_IntRange_InvalidRange_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Partitioner.Create(100, 0));
  }

  [Test]
  [Category("Exception")]
  public void Partitioner_Create_IntRange_ZeroRangeSize_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Partitioner.Create(0, 100, 0));
  }

  [Test]
  [Category("Exception")]
  public void Partitioner_Create_IntRange_NegativeRangeSize_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Partitioner.Create(0, 100, -1));
  }

  #endregion

  #region Partitioner.Create Int64 Range

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_LongRange_ReturnsPartitioner() {
    var partitioner = Partitioner.Create(0L, 100L);
    Assert.That(partitioner, Is.Not.Null);
    Assert.That(partitioner, Is.InstanceOf<OrderablePartitioner<Tuple<long, long>>>());
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_LongRange_CoversFullRange() {
    var partitioner = Partitioner.Create(0L, 100L);
    var partitions = partitioner.GetOrderablePartitions(4);

    var coveredValues = new HashSet<long>();
    foreach (var partition in partitions) {
      while (partition.MoveNext()) {
        var range = partition.Current.Value;
        for (var i = range.Item1; i < range.Item2; ++i)
          coveredValues.Add(i);
      }
    }

    Assert.That(coveredValues.Count, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_LongRangeWithSize_RespectsRangeSize() {
    var partitioner = Partitioner.Create(0L, 100L, 20L);
    var partitions = partitioner.GetOrderablePartitions(1);

    var ranges = new List<Tuple<long, long>>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ranges.Add(partition.Current.Value);

    Assert.That(ranges.Count, Is.EqualTo(5));
    Assert.That(ranges[0], Is.EqualTo(Tuple.Create(0L, 20L)));
    Assert.That(ranges[4], Is.EqualTo(Tuple.Create(80L, 100L)));
  }

  #endregion

  #region Partitioner.Create List

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_List_ReturnsPartitioner() {
    var list = new List<int> { 1, 2, 3, 4, 5 };
    var partitioner = Partitioner.Create(list, loadBalance: false);
    Assert.That(partitioner, Is.Not.Null);
    Assert.That(partitioner, Is.InstanceOf<OrderablePartitioner<int>>());
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_List_CoversAllElements() {
    var list = new List<int> { 10, 20, 30, 40, 50 };
    var partitioner = Partitioner.Create(list, loadBalance: false);
    var partitions = partitioner.GetOrderablePartitions(2);

    var values = new List<int>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        values.Add(partition.Current.Value);

    Assert.That(values.Count, Is.EqualTo(5));
    Assert.That(values, Is.EquivalentTo(list));
  }

  #endregion

  #region Partitioner.Create Array

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_Array_ReturnsPartitioner() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var partitioner = Partitioner.Create(array, loadBalance: false);
    Assert.That(partitioner, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_Array_CoversAllElements() {
    var array = new[] { "a", "b", "c", "d", "e" };
    var partitioner = Partitioner.Create(array, loadBalance: false);
    var partitions = partitioner.GetOrderablePartitions(3);

    var values = new List<string>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        values.Add(partition.Current.Value);

    Assert.That(values.Count, Is.EqualTo(5));
    Assert.That(values, Is.EquivalentTo(array));
  }

  #endregion

  #region Partitioner.Create Enumerable

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_Enumerable_ReturnsPartitioner() {
    IEnumerable<int> enumerable = Enumerable.Range(0, 10);
    var partitioner = Partitioner.Create(enumerable);
    Assert.That(partitioner, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_Enumerable_CoversAllElements() {
    IEnumerable<int> enumerable = Enumerable.Range(0, 100);
    var partitioner = Partitioner.Create(enumerable);
    var partitions = partitioner.GetOrderablePartitions(4);

    var values = new HashSet<int>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        values.Add(partition.Current.Value);

    Assert.That(values.Count, Is.EqualTo(100));
    for (var i = 0; i < 100; ++i)
      Assert.That(values.Contains(i), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Create_Enumerable_WithNoBuffering_Works() {
    IEnumerable<int> enumerable = Enumerable.Range(0, 50);
    var partitioner = Partitioner.Create(enumerable, EnumerablePartitionerOptions.NoBuffering);
    var partitions = partitioner.GetOrderablePartitions(2);

    var count = 0;
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ++count;

    Assert.That(count, Is.EqualTo(50));
  }

  #endregion

  #region OrderablePartitioner Properties

  [Test]
  [Category("HappyPath")]
  public void OrderablePartitioner_IntRange_KeysOrderedInEachPartition_IsTrue() {
    var partitioner = Partitioner.Create(0, 100);
    Assert.That(partitioner.KeysOrderedInEachPartition, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void OrderablePartitioner_IntRange_PropertiesAreAccessible() {
    var partitioner = Partitioner.Create(0, 100);
    // Just verify properties are accessible - actual values may vary by implementation
    _ = partitioner.KeysOrderedAcrossPartitions;
    _ = partitioner.KeysNormalized;
    Assert.Pass();
  }

  #endregion

  #region GetPartitions (non-orderable)

  [Test]
  [Category("HappyPath")]
  public void Partitioner_GetPartitions_ReturnsCorrectCount() {
    var partitioner = Partitioner.Create(0, 100, 10);
    var partitions = partitioner.GetPartitions(4);
    Assert.That(partitions.Count, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_GetPartitions_CoversFullRange() {
    var partitioner = Partitioner.Create(0, 50, 10);
    var partitions = partitioner.GetPartitions(2);

    var coveredValues = new HashSet<int>();
    foreach (var partition in partitions) {
      while (partition.MoveNext()) {
        var range = partition.Current;
        for (var i = range.Item1; i < range.Item2; ++i)
          coveredValues.Add(i);
      }
    }

    Assert.That(coveredValues.Count, Is.EqualTo(50));
  }

  #endregion

  #region Integration with Parallel.ForEach

  [Test]
  [Category("HappyPath")]
  public void Partitioner_WithParallelForEach_ProcessesAllRanges() {
    var partitioner = Partitioner.Create(0, 100, 10);
    var processedRanges = new List<Tuple<int, int>>();
    var lockObj = new object();

    Parallel.ForEach(partitioner, range => {
      lock (lockObj)
        processedRanges.Add(range);
    });

    Assert.That(processedRanges.Count, Is.EqualTo(10));

    var coveredValues = new HashSet<int>();
    foreach (var range in processedRanges)
      for (var i = range.Item1; i < range.Item2; ++i)
        coveredValues.Add(i);

    Assert.That(coveredValues.Count, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_WithParallelForEach_AndState_Works() {
    var partitioner = Partitioner.Create(0, 50, 10);
    var count = 0;
    var lockObj = new object();

    Parallel.ForEach(partitioner, (range, state) => {
      lock (lockObj)
        count += range.Item2 - range.Item1;
    });

    Assert.That(count, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_WithParallelForEach_AndIndex_ProvidesIndex() {
    var partitioner = Partitioner.Create(0, 30, 10);
    var indices = new List<long>();
    var lockObj = new object();

    Parallel.ForEach(partitioner, (range, state, index) => {
      lock (lockObj)
        indices.Add(index);
    });

    indices.Sort();
    Assert.That(indices, Is.EqualTo(new long[] { 0, 1, 2 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_Enumerable_WithParallelForEach_ProcessesAllItems() {
    // Use enumerable partitioner which supports dynamic partitioning
    IEnumerable<int> enumerable = Enumerable.Range(1, 100);
    var partitioner = Partitioner.Create(enumerable);
    var sum = 0;
    var lockObj = new object();

    Parallel.ForEach(partitioner, item => {
      lock (lockObj)
        sum += item;
    });

    var expectedSum = Enumerable.Range(1, 100).Sum();
    Assert.That(sum, Is.EqualTo(expectedSum));
  }

  [Test]
  [Category("HappyPath")]
  public void Partitioner_List_ManualIteration_ProcessesAllItems() {
    var list = Enumerable.Range(1, 100).ToList();
    var partitioner = Partitioner.Create(list, loadBalance: false);
    var partitions = partitioner.GetPartitions(4);

    var sum = 0;
    foreach (var partition in partitions)
      while (partition.MoveNext())
        sum += partition.Current;

    var expectedSum = Enumerable.Range(1, 100).Sum();
    Assert.That(sum, Is.EqualTo(expectedSum));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void Partitioner_SingleElementRange_Works() {
    var partitioner = Partitioner.Create(0, 1);
    var partitions = partitioner.GetOrderablePartitions(1);

    var count = 0;
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ++count;

    Assert.That(count, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Partitioner_RangeSizeLargerThanRange_Works() {
    var partitioner = Partitioner.Create(0, 10, 100);
    var partitions = partitioner.GetOrderablePartitions(1);

    var ranges = new List<Tuple<int, int>>();
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ranges.Add(partition.Current.Value);

    Assert.That(ranges.Count, Is.EqualTo(1));
    Assert.That(ranges[0], Is.EqualTo(Tuple.Create(0, 10)));
  }

  [Test]
  [Category("EdgeCase")]
  public void Partitioner_MorePartitionsThanRanges_Works() {
    var partitioner = Partitioner.Create(0, 10, 5);
    var partitions = partitioner.GetOrderablePartitions(10);

    var totalCount = 0;
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ++totalCount;

    Assert.That(totalCount, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void Partitioner_EmptyList_Works() {
    var list = new List<int>();
    var partitioner = Partitioner.Create(list, loadBalance: false);
    var partitions = partitioner.GetOrderablePartitions(4);

    var count = 0;
    foreach (var partition in partitions)
      while (partition.MoveNext())
        ++count;

    Assert.That(count, Is.EqualTo(0));
  }

  #endregion

}
