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
/// Represents a particular manner of splitting an orderable data source into multiple partitions.
/// </summary>
/// <typeparam name="TSource">Type of the elements in the collection.</typeparam>
public abstract class OrderablePartitioner<TSource> : Partitioner<TSource> {

  /// <summary>
  /// Initializes a new instance of the <see cref="OrderablePartitioner{TSource}"/> class
  /// with the specified constraints on the index keys.
  /// </summary>
  /// <param name="keysOrderedInEachPartition">
  /// Indicates whether the elements in each partition are yielded in the order of increasing keys.
  /// </param>
  /// <param name="keysOrderedAcrossPartitions">
  /// Indicates whether elements in an earlier partition always come before elements in a later partition.
  /// If <see langword="true"/>, each element in partition 0 has a smaller order key than any element
  /// in partition 1, each element in partition 1 has a smaller order key than any element
  /// in partition 2, and so on.
  /// </param>
  /// <param name="keysNormalized">
  /// Indicates whether keys are normalized. If <see langword="true"/>, all order keys are distinct
  /// integers in the range [0 .. numberOfElements-1]. If <see langword="false"/>, order keys must still
  /// be distinct, but only their relative order is considered, not their absolute values.
  /// </param>
  protected OrderablePartitioner(
    bool keysOrderedInEachPartition,
    bool keysOrderedAcrossPartitions,
    bool keysNormalized
  ) {
    this.KeysOrderedInEachPartition = keysOrderedInEachPartition;
    this.KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
    this.KeysNormalized = keysNormalized;
  }

  /// <summary>
  /// Gets whether elements in each partition are yielded in the order of increasing keys.
  /// </summary>
  public bool KeysOrderedInEachPartition { get; }

  /// <summary>
  /// Gets whether elements in an earlier partition always come before elements in a later partition.
  /// </summary>
  public bool KeysOrderedAcrossPartitions { get; }

  /// <summary>
  /// Gets whether order keys are normalized.
  /// </summary>
  public bool KeysNormalized { get; }

  /// <summary>
  /// Partitions the underlying collection into the specified number of orderable partitions.
  /// </summary>
  /// <param name="partitionCount">The number of partitions to create.</param>
  /// <returns>A list containing <paramref name="partitionCount"/> enumerators.</returns>
  public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

  /// <inheritdoc/>
  public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount) {
    var orderablePartitions = this.GetOrderablePartitions(partitionCount);
    var partitions = new IEnumerator<TSource>[orderablePartitions.Count];
    for (var i = 0; i < orderablePartitions.Count; ++i)
      partitions[i] = new _KeyValuePairUnwrapper(orderablePartitions[i]);
    return partitions;
  }

  /// <summary>
  /// Creates an object that can partition the underlying collection into a variable number of partitions.
  /// </summary>
  /// <returns>An object that can create partitions over the underlying data source.</returns>
  /// <exception cref="NotSupportedException">
  /// Dynamic partitioning is not supported by this partitioner.
  /// </exception>
  public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
    => throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");

  private sealed class _KeyValuePairUnwrapper(IEnumerator<KeyValuePair<long, TSource>> source) : IEnumerator<TSource> {

    public TSource Current => source.Current.Value;
    object IEnumerator.Current => this.Current;
    public bool MoveNext() => source.MoveNext();
    public void Reset() => source.Reset();
    public void Dispose() => source.Dispose();

  }

}

#endif
