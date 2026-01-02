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
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Concurrent;

/// <summary>
/// Defines methods to manipulate thread-safe collections intended for producer/consumer usage.
/// </summary>
/// <typeparam name="T">Specifies the type of elements in the collection.</typeparam>
/// <remarks>
/// This interface provides a unified view of producer/consumer collections, allowing items
/// to be added and removed in a thread-safe manner.
/// </remarks>
public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection {

  /// <summary>
  /// Copies the elements of the <see cref="IProducerConsumerCollection{T}"/> to an
  /// <see cref="Array"/>, starting at a specified index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
  /// the <see cref="IProducerConsumerCollection{T}"/>. The array must have zero-based indexing.
  /// </param>
  /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>,
  /// or the number of elements in the collection is greater than the available space from
  /// <paramref name="index"/> to the end of the destination <paramref name="array"/>.
  /// </exception>
  void CopyTo(T[] array, int index);

  /// <summary>
  /// Attempts to add an object to the <see cref="IProducerConsumerCollection{T}"/>.
  /// </summary>
  /// <param name="item">The object to add to the <see cref="IProducerConsumerCollection{T}"/>.</param>
  /// <returns>
  /// <c>true</c> if the object was added successfully; otherwise, <c>false</c>.
  /// </returns>
  bool TryAdd(T item);

  /// <summary>
  /// Attempts to remove and return an object from the <see cref="IProducerConsumerCollection{T}"/>.
  /// </summary>
  /// <param name="item">
  /// When this method returns, if the object was removed and returned successfully, <paramref name="item"/>
  /// contains the removed object. If no object was available to be removed, the value is unspecified.
  /// </param>
  /// <returns>
  /// <c>true</c> if an object was removed and returned successfully; otherwise, <c>false</c>.
  /// </returns>
  bool TryTake([MaybeNullWhen(false)] out T item);

  /// <summary>
  /// Copies the elements contained in the <see cref="IProducerConsumerCollection{T}"/> to a new array.
  /// </summary>
  /// <returns>A new array containing the elements copied from the <see cref="IProducerConsumerCollection{T}"/>.</returns>
  T[] ToArray();

}

#endif
