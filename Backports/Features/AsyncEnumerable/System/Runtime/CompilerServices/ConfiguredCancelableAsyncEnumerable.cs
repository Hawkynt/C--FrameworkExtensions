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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides an awaitable async enumerable that enables cancelable iteration and configured awaits.
/// </summary>
/// <typeparam name="T">The type of values to enumerate.</typeparam>
public readonly struct ConfiguredCancelableAsyncEnumerable<T> {
  private readonly IAsyncEnumerable<T> _enumerable;
  private readonly CancellationToken _cancellationToken;
  private readonly bool _continueOnCapturedContext;

  internal ConfiguredCancelableAsyncEnumerable(IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, bool continueOnCapturedContext) {
    _enumerable = enumerable;
    _cancellationToken = cancellationToken;
    _continueOnCapturedContext = continueOnCapturedContext;
  }

  /// <summary>
  /// Configures how awaits on the tasks returned from an async iteration will be performed.
  /// </summary>
  /// <param name="continueOnCapturedContext">
  /// <see langword="true"/> to capture and marshal back to the current context; otherwise, <see langword="false"/>.
  /// </param>
  /// <returns>The configured enumerable.</returns>
  public ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait(bool continueOnCapturedContext)
    => new(_enumerable, _cancellationToken, continueOnCapturedContext);

  /// <summary>
  /// Sets the <see cref="CancellationToken"/> to be passed to <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator"/>
  /// when iterating.
  /// </summary>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
  /// <returns>The configured enumerable.</returns>
  public ConfiguredCancelableAsyncEnumerable<T> WithCancellation(CancellationToken cancellationToken)
    => new(_enumerable, cancellationToken, _continueOnCapturedContext);

  /// <summary>
  /// Returns an enumerator that iterates asynchronously through collections that enables cancelable iteration and configured awaits.
  /// </summary>
  /// <returns>An enumerator for the <see cref="ConfiguredCancelableAsyncEnumerable{T}"/>.</returns>
  public Enumerator GetAsyncEnumerator() => new(_enumerable.GetAsyncEnumerator(_cancellationToken), _continueOnCapturedContext);

  /// <summary>
  /// Provides an awaitable async enumerator that enables cancelable iteration and configured awaits.
  /// </summary>
  public readonly struct Enumerator {
    private readonly IAsyncEnumerator<T> _enumerator;
    private readonly bool _continueOnCapturedContext;

    internal Enumerator(IAsyncEnumerator<T> enumerator, bool continueOnCapturedContext) {
      _enumerator = enumerator;
      _continueOnCapturedContext = continueOnCapturedContext;
    }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    public T Current => _enumerator.Current;

    /// <summary>
    /// Advances the enumerator asynchronously to the next element of the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="ConfiguredValueTaskAwaitable{TResult}"/> that will complete with a result of <c>true</c>
    /// if the enumerator was successfully advanced to the next element, or <c>false</c> if the enumerator has
    /// passed the end of the collection.
    /// </returns>
    public ConfiguredValueTaskAwaitable<bool> MoveNextAsync()
      => _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public ConfiguredValueTaskAwaitable DisposeAsync()
      => _enumerator.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
  }
}

/// <summary>
/// Provides extension methods for <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
public static class AsyncEnumerableExtensions {
  /// <summary>
  /// Configures how awaits on the tasks returned from an async iteration will be performed.
  /// </summary>
  /// <typeparam name="T">The type of values to enumerate.</typeparam>
  /// <param name="source">The source enumerable being iterated.</param>
  /// <param name="continueOnCapturedContext">
  /// <see langword="true"/> to capture and marshal back to the current context; otherwise, <see langword="false"/>.
  /// </param>
  /// <returns>The configured enumerable.</returns>
  public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait<T>(this IAsyncEnumerable<T> source, bool continueOnCapturedContext)
    => new(source, default, continueOnCapturedContext);

  /// <summary>
  /// Sets the <see cref="CancellationToken"/> to be passed to <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator"/>
  /// when iterating.
  /// </summary>
  /// <typeparam name="T">The type of values to enumerate.</typeparam>
  /// <param name="source">The source enumerable being iterated.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
  /// <returns>The configured enumerable.</returns>
  public static ConfiguredCancelableAsyncEnumerable<T> WithCancellation<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
    => new(source, cancellationToken, true);
}

#endif
