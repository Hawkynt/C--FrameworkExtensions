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
/// Represents a particular manner of splitting a data source into multiple partitions.
/// </summary>
/// <typeparam name="TSource">Type of the elements in the collection.</typeparam>
public abstract class Partitioner<TSource> {

  /// <summary>
  /// Partitions the underlying collection into the given number of partitions.
  /// </summary>
  /// <param name="partitionCount">The number of partitions to create.</param>
  /// <returns>A list containing <paramref name="partitionCount"/> enumerators.</returns>
  public abstract IList<IEnumerator<TSource>> GetPartitions(int partitionCount);

  /// <summary>
  /// Creates an object that can partition the underlying collection into a variable number of partitions.
  /// </summary>
  /// <returns>An object that can create partitions over the underlying data source.</returns>
  /// <exception cref="NotSupportedException">
  /// Dynamic partitioning is not supported by this partitioner.
  /// </exception>
  public virtual IEnumerable<TSource> GetDynamicPartitions()
    => throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");

  /// <summary>
  /// Gets whether additional partitions can be created dynamically.
  /// </summary>
  /// <returns>
  /// <see langword="true"/> if the <see cref="Partitioner{TSource}"/> can create partitions dynamically
  /// as they are requested; <see langword="false"/> if the <see cref="Partitioner{TSource}"/> can only
  /// allocate partitions statically.
  /// </returns>
  public virtual bool SupportsDynamicPartitions => false;

}

#endif
