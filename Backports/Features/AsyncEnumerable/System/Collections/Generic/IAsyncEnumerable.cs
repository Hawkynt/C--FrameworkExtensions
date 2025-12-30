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

#if !SUPPORTS_ASYNC_ENUMERABLE && !OFFICIAL_ASYNC_ENUMERABLE

using System.Threading;

namespace System.Collections.Generic;

/// <summary>
/// Exposes an enumerator that provides asynchronous iteration over values of a specified type.
/// </summary>
/// <typeparam name="T">The type of values to enumerate.</typeparam>
public interface IAsyncEnumerable<out T> {
  /// <summary>
  /// Returns an enumerator that iterates asynchronously through the collection.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may be used to cancel the asynchronous iteration.</param>
  /// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
  IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

#endif
