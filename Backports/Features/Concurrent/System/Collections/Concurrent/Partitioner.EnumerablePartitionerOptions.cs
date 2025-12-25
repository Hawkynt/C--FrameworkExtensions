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

// This extension adds the Partitioner.Create(IEnumerable, EnumerablePartitionerOptions) overload
// for .NET 4.0 which has Partitioner but not the options overload (added in .NET 4.5)
#if SUPPORTS_CONCURRENT_COLLECTIONS && !SUPPORTS_ENUMERABLE_PARTITIONER_OPTIONS

using System.Collections.Generic;

namespace System.Collections.Concurrent;

public static class PartitionerPolyfills {

  extension(Partitioner) {

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

  }

  private sealed class _EnumerablePartitioner<TSource>(IEnumerable<TSource> source, EnumerablePartitionerOptions options)
    : OrderablePartitioner<TSource>(
        keysOrderedInEachPartition: false,
        keysOrderedAcrossPartitions: false,
        keysNormalized: false
      ) {

    private readonly object _lock = new();
    private IEnumerator<TSource>? _enumerator;
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
            if (this._enumerator!.MoveNext()) {
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
      }
    }

  }

}

#endif
