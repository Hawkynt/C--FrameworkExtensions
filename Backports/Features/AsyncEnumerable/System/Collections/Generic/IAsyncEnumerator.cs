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

#if SUPPORTS_TASK_AWAITER || OFFICIAL_TASK_AWAITER

using System.Threading.Tasks;

namespace System.Collections.Generic;

/// <summary>
/// Supports a simple asynchronous iteration over a generic collection.
/// </summary>
/// <typeparam name="T">The type of objects to enumerate.</typeparam>
public interface IAsyncEnumerator<out T> : IAsyncDisposable {
  /// <summary>
  /// Gets the element in the collection at the current position of the enumerator.
  /// </summary>
  T Current { get; }

  /// <summary>
  /// Advances the enumerator asynchronously to the next element of the collection.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that will complete with a result of <c>true</c> if the enumerator
  /// was successfully advanced to the next element, or <c>false</c> if the enumerator has passed the end
  /// of the collection.
  /// </returns>
#if SUPPORTS_VALUE_TASK || OFFICIAL_VALUETASK
  ValueTask<bool> MoveNextAsync();
#else
  Task<bool> MoveNextAsync();
#endif
}

#endif

#endif
