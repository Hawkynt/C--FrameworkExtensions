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

// Task.WhenEach was added in .NET 9.0
// Requires IAsyncEnumerable (polyfilled for older targets) and ManualResetValueTaskSourceCore (polyfilled or from package)
#if !SUPPORTS_TASK_WHENEACH

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks;

public static partial class TaskPolyfills {

  extension(Task) {

    /// <summary>
    /// Creates an <see cref="IAsyncEnumerable{T}"/> that will yield the supplied tasks as they complete.
    /// </summary>
    /// <typeparam name="TResult">The type of the results produced by the tasks.</typeparam>
    /// <param name="tasks">The tasks to wait on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the tasks as they complete.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static IAsyncEnumerable<Task<TResult>> WhenEach<TResult>(params Task<TResult>[] tasks) {
      ArgumentNullException.ThrowIfNull(tasks);
      return new WhenEachEnumerable<TResult>(tasks);
    }

    /// <summary>
    /// Creates an <see cref="IAsyncEnumerable{T}"/> that will yield the supplied tasks as they complete.
    /// </summary>
    /// <typeparam name="TResult">The type of the results produced by the tasks.</typeparam>
    /// <param name="tasks">The tasks to wait on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the tasks as they complete.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static IAsyncEnumerable<Task<TResult>> WhenEach<TResult>(IEnumerable<Task<TResult>> tasks) {
      ArgumentNullException.ThrowIfNull(tasks);
      return new WhenEachEnumerable<TResult>(tasks);
    }

    /// <summary>
    /// Creates an <see cref="IAsyncEnumerable{T}"/> that will yield the supplied tasks as they complete.
    /// </summary>
    /// <param name="tasks">The tasks to wait on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the tasks as they complete.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static IAsyncEnumerable<Task> WhenEach(params Task[] tasks) {
      ArgumentNullException.ThrowIfNull(tasks);
      return new WhenEachEnumerableNonGeneric(tasks);
    }

    /// <summary>
    /// Creates an <see cref="IAsyncEnumerable{T}"/> that will yield the supplied tasks as they complete.
    /// </summary>
    /// <param name="tasks">The tasks to wait on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields the tasks as they complete.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static IAsyncEnumerable<Task> WhenEach(IEnumerable<Task> tasks) {
      ArgumentNullException.ThrowIfNull(tasks);
      return new WhenEachEnumerableNonGeneric(tasks);
    }

  }

}

file sealed class WhenEachEnumerable<TResult>(IEnumerable<Task<TResult>> tasks) : IAsyncEnumerable<Task<TResult>> {
  public IAsyncEnumerator<Task<TResult>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    => new WhenEachEnumerator<TResult>(tasks, cancellationToken);
}

file sealed class WhenEachEnumerator<TResult>(IEnumerable<Task<TResult>> tasks, CancellationToken cancellationToken)
  : IAsyncEnumerator<Task<TResult>> {
  private readonly List<Task<TResult>> _remaining = [..tasks];
  private Task<TResult>? _current;

  public Task<TResult> Current => this._current ?? throw new InvalidOperationException();

  public async ValueTask<bool> MoveNextAsync() {
    cancellationToken.ThrowIfCancellationRequested();

    if (this._remaining.Count == 0)
      return false;

    var completed = await Task.WhenAny(this._remaining.ToArray()).ConfigureAwait(false);
    this._remaining.Remove(completed);
    this._current = completed;
    return true;
  }

  public ValueTask DisposeAsync() {
    this._remaining.Clear();
    return default;
  }
}

file sealed class WhenEachEnumerableNonGeneric(IEnumerable<Task> tasks) : IAsyncEnumerable<Task> {
  public IAsyncEnumerator<Task> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    => new WhenEachEnumeratorNonGeneric(tasks, cancellationToken);
}

file sealed class WhenEachEnumeratorNonGeneric(IEnumerable<Task> tasks, CancellationToken cancellationToken)
  : IAsyncEnumerator<Task> {
  private readonly List<Task> _remaining = [..tasks];
  private Task? _current;

  public Task Current => this._current ?? throw new InvalidOperationException();

  public async ValueTask<bool> MoveNextAsync() {
    cancellationToken.ThrowIfCancellationRequested();

    if (this._remaining.Count == 0)
      return false;

    var completed = await Task.WhenAny(this._remaining.ToArray()).ConfigureAwait(false);
    this._remaining.Remove(completed);
    this._current = completed;
    return true;
  }

  public ValueTask DisposeAsync() {
    this._remaining.Clear();
    return default;
  }
}

#endif
