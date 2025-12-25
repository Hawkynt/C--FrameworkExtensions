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

#if !SUPPORTS_CONCURRENT_COLLECTIONS

using System.Collections.Generic;

namespace System.Collections.Concurrent;

/// <summary>
/// Provides common partitioning strategies for arrays, lists, and enumerables.
/// </summary>
public static class Partitioner {

  /// <summary>
  /// Creates a partitioner that chunks the user-specified range.
  /// </summary>
  /// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
  /// <param name="toExclusive">The upper, exclusive bound of the range.</param>
  /// <returns>A partitioner.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="toExclusive"/> is less than or equal to <paramref name="fromInclusive"/>.
  /// </exception>
  public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive) {
    var rangeCount = Environment.ProcessorCount * 3;
    var totalRange = toExclusive - fromInclusive;
    var rangeSize = (totalRange + rangeCount - 1) / rangeCount;
    rangeSize = Math.Max(1, rangeSize);
    return Create(fromInclusive, toExclusive, rangeSize);
  }

  /// <summary>
  /// Creates a partitioner that chunks the user-specified range.
  /// </summary>
  /// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
  /// <param name="toExclusive">The upper, exclusive bound of the range.</param>
  /// <param name="rangeSize">The size of each subrange.</param>
  /// <returns>A partitioner.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="toExclusive"/> is less than or equal to <paramref name="fromInclusive"/>.
  /// -or- <paramref name="rangeSize"/> is less than or equal to zero.
  /// </exception>
  public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive, int rangeSize) {
    if (toExclusive <= fromInclusive)
      throw new ArgumentOutOfRangeException(nameof(toExclusive), "toExclusive must be greater than fromInclusive.");
    if (rangeSize <= 0)
      throw new ArgumentOutOfRangeException(nameof(rangeSize), "rangeSize must be positive.");
    return new _Int32RangePartitioner(fromInclusive, toExclusive, rangeSize);
  }

  /// <summary>
  /// Creates a partitioner that chunks the user-specified range.
  /// </summary>
  /// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
  /// <param name="toExclusive">The upper, exclusive bound of the range.</param>
  /// <returns>A partitioner.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="toExclusive"/> is less than or equal to <paramref name="fromInclusive"/>.
  /// </exception>
  public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive) {
    var rangeCount = Environment.ProcessorCount * 3;
    var totalRange = toExclusive - fromInclusive;
    var rangeSize = (totalRange + rangeCount - 1) / rangeCount;
    rangeSize = Math.Max(1L, rangeSize);
    return Create(fromInclusive, toExclusive, rangeSize);
  }

  /// <summary>
  /// Creates a partitioner that chunks the user-specified range.
  /// </summary>
  /// <param name="fromInclusive">The lower, inclusive bound of the range.</param>
  /// <param name="toExclusive">The upper, exclusive bound of the range.</param>
  /// <param name="rangeSize">The size of each subrange.</param>
  /// <returns>A partitioner.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="toExclusive"/> is less than or equal to <paramref name="fromInclusive"/>.
  /// -or- <paramref name="rangeSize"/> is less than or equal to zero.
  /// </exception>
  public static OrderablePartitioner<Tuple<long, long>> Create(long fromInclusive, long toExclusive, long rangeSize) {
    if (toExclusive <= fromInclusive)
      throw new ArgumentOutOfRangeException(nameof(toExclusive), "toExclusive must be greater than fromInclusive.");
    if (rangeSize <= 0L)
      throw new ArgumentOutOfRangeException(nameof(rangeSize), "rangeSize must be positive.");
    return new _Int64RangePartitioner(fromInclusive, toExclusive, rangeSize);
  }

  /// <summary>
  /// Creates an orderable partitioner from an <see cref="IList{T}"/> instance.
  /// </summary>
  /// <typeparam name="TSource">Type of the elements in the source list.</typeparam>
  /// <param name="list">The list to be partitioned.</param>
  /// <param name="loadBalance">
  /// <see langword="true"/> to dynamically distribute items; <see langword="false"/> for static allocation.
  /// </param>
  /// <returns>An orderable partitioner based on the input list.</returns>
  public static OrderablePartitioner<TSource> Create<TSource>(IList<TSource> list, bool loadBalance)
    => new _ListPartitioner<TSource>(list, loadBalance);

  /// <summary>
  /// Creates an orderable partitioner from an array.
  /// </summary>
  /// <typeparam name="TSource">Type of the elements in the source array.</typeparam>
  /// <param name="array">The array to be partitioned.</param>
  /// <param name="loadBalance">
  /// <see langword="true"/> to dynamically distribute items; <see langword="false"/> for static allocation.
  /// </param>
  /// <returns>An orderable partitioner based on the input array.</returns>
  public static OrderablePartitioner<TSource> Create<TSource>(TSource[] array, bool loadBalance)
    => new _ListPartitioner<TSource>(array, loadBalance);

  /// <summary>
  /// Creates an orderable partitioner from a <see cref="IEnumerable{T}"/> instance.
  /// </summary>
  /// <typeparam name="TSource">Type of the elements in the source enumerable.</typeparam>
  /// <param name="source">The enumerable to be partitioned.</param>
  /// <returns>An orderable partitioner based on the input enumerable.</returns>
  public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source)
    => Create(source, EnumerablePartitionerOptions.None);

  /// <summary>
  /// Creates an orderable partitioner from a <see cref="IEnumerable{T}"/> instance.
  /// </summary>
  /// <typeparam name="TSource">Type of the elements in the source enumerable.</typeparam>
  /// <param name="source">The enumerable to be partitioned.</param>
  /// <param name="partitionerOptions">Options to control the buffering behavior of the partitioner.</param>
  /// <returns>An orderable partitioner based on the input enumerable.</returns>
  public static OrderablePartitioner<TSource> Create<TSource>(
    IEnumerable<TSource> source,
    EnumerablePartitionerOptions partitionerOptions
  ) => new _EnumerablePartitioner<TSource>(source, partitionerOptions);

  #region Internal Partitioner Implementations

  private sealed class _Int32RangePartitioner(int from, int to, int rangeSize)
    : OrderablePartitioner<Tuple<int, int>>(
        keysOrderedInEachPartition: true,
        keysOrderedAcrossPartitions: true,
        keysNormalized: true
      ) {

    public override IList<IEnumerator<KeyValuePair<long, Tuple<int, int>>>> GetOrderablePartitions(int partitionCount) {
      var ranges = new List<KeyValuePair<long, Tuple<int, int>>>();
      var index = 0L;
      for (var start = from; start < to; start += rangeSize) {
        var end = Math.Min(start + rangeSize, to);
        ranges.Add(new(index++, Tuple.Create(start, end)));
      }

      var partitions = new IEnumerator<KeyValuePair<long, Tuple<int, int>>>[partitionCount];
      var rangesPerPartition = (ranges.Count + partitionCount - 1) / partitionCount;
      for (var i = 0; i < partitionCount; ++i) {
        var startRange = i * rangesPerPartition;
        var endRange = Math.Min(startRange + rangesPerPartition, ranges.Count);
        partitions[i] = _GetRangesEnumerator(ranges, startRange, endRange);
      }
      return partitions;
    }

    private static IEnumerator<KeyValuePair<long, Tuple<int, int>>> _GetRangesEnumerator(
      List<KeyValuePair<long, Tuple<int, int>>> ranges,
      int start,
      int end
    ) {
      for (var i = start; i < end; ++i)
        yield return ranges[i];
    }

  }

  private sealed class _Int64RangePartitioner(long from, long to, long rangeSize)
    : OrderablePartitioner<Tuple<long, long>>(
        keysOrderedInEachPartition: true,
        keysOrderedAcrossPartitions: true,
        keysNormalized: true
      ) {

    public override IList<IEnumerator<KeyValuePair<long, Tuple<long, long>>>> GetOrderablePartitions(int partitionCount) {
      var ranges = new List<KeyValuePair<long, Tuple<long, long>>>();
      var index = 0L;
      for (var start = from; start < to; start += rangeSize) {
        var end = Math.Min(start + rangeSize, to);
        ranges.Add(new(index++, Tuple.Create(start, end)));
      }

      var partitions = new IEnumerator<KeyValuePair<long, Tuple<long, long>>>[partitionCount];
      var rangesPerPartition = (ranges.Count + partitionCount - 1) / partitionCount;
      for (var i = 0; i < partitionCount; ++i) {
        var startRange = i * rangesPerPartition;
        var endRange = Math.Min(startRange + rangesPerPartition, ranges.Count);
        partitions[i] = _GetRangesEnumerator(ranges, startRange, endRange);
      }
      return partitions;
    }

    private static IEnumerator<KeyValuePair<long, Tuple<long, long>>> _GetRangesEnumerator(
      List<KeyValuePair<long, Tuple<long, long>>> ranges,
      int start,
      int end
    ) {
      for (var i = start; i < end; ++i)
        yield return ranges[i];
    }

  }

  private sealed class _ListPartitioner<TSource>(IList<TSource> list, bool loadBalance)
    : OrderablePartitioner<TSource>(
        keysOrderedInEachPartition: !loadBalance,
        keysOrderedAcrossPartitions: !loadBalance,
        keysNormalized: true
      ) {

    public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount) {
      var partitions = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
      var itemsPerPartition = (list.Count + partitionCount - 1) / partitionCount;

      for (var i = 0; i < partitionCount; ++i) {
        var start = i * itemsPerPartition;
        var end = Math.Min(start + itemsPerPartition, list.Count);
        partitions[i] = _GetPartitionEnumerator(start, end);
      }
      return partitions;
    }

    private IEnumerator<KeyValuePair<long, TSource>> _GetPartitionEnumerator(int start, int end) {
      for (var i = start; i < end; ++i)
        yield return new(i, list[i]);
    }

  }

  private sealed class _EnumerablePartitioner<TSource>(IEnumerable<TSource> source, EnumerablePartitionerOptions options)
    : OrderablePartitioner<TSource>(
        keysOrderedInEachPartition: false,
        keysOrderedAcrossPartitions: false,
        keysNormalized: false
      ) {

    private readonly object _lock = new();
    private IEnumerator<TSource> _enumerator;
    private long _currentIndex = -1;
    private bool _exhausted;

    public override bool SupportsDynamicPartitions => true;

    public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount) {
      this._enumerator = source.GetEnumerator();
      var partitions = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
      for (var i = 0; i < partitionCount; ++i)
        partitions[i] = this._GetPartitionEnumerator();
      return partitions;
    }

    public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() {
      this._enumerator ??= source.GetEnumerator();
      while (true) {
        KeyValuePair<long, TSource>? item = null;
        lock (this._lock) {
          if (!this._exhausted && this._enumerator.MoveNext()) {
            ++this._currentIndex;
            item = new(this._currentIndex, this._enumerator.Current);
          } else
            this._exhausted = true;
        }

        if (item == null)
          yield break;

        yield return item.Value;
      }
    }

    private IEnumerator<KeyValuePair<long, TSource>> _GetPartitionEnumerator() {
      var useBuffering = (options & EnumerablePartitionerOptions.NoBuffering) == 0;
      var bufferSize = useBuffering ? 16 : 1;

      while (true) {
        var buffer = new List<KeyValuePair<long, TSource>>(bufferSize);
        lock (this._lock) {
          if (this._exhausted)
            yield break;

          for (var i = 0; i < bufferSize && !this._exhausted; ++i) {
            if (this._enumerator.MoveNext()) {
              ++this._currentIndex;
              buffer.Add(new(this._currentIndex, this._enumerator.Current));
            } else
              this._exhausted = true;
          }
        }

        if (buffer.Count == 0)
          yield break;

        foreach (var item in buffer)
          yield return item;

        if (buffer.Count < bufferSize)
          yield break;
      }
    }

  }

  #endregion

}

#endif
