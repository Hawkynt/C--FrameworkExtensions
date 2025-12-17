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

#if !SUPPORTS_ASYNC

using System.Collections.Generic;

namespace System.Threading.Tasks;

/// <summary>
/// Represents the producer side of a <see cref="Task{TResult}"/> unbound to a delegate, providing access to the consumer side through the <see cref="Task"/> property.
/// </summary>
/// <typeparam name="TResult">The type of the result value associated with this <see cref="TaskCompletionSource{TResult}"/>.</typeparam>
public class TaskCompletionSource<TResult> {
  private readonly object _lock = new();

  /// <summary>
  /// Gets the <see cref="Task{TResult}"/> created by this <see cref="TaskCompletionSource{TResult}"/>.
  /// </summary>
  public Task<TResult> Task { get; }

  /// <summary>
  /// Creates a <see cref="TaskCompletionSource{TResult}"/>.
  /// </summary>
  public TaskCompletionSource()
    => this.Task = new Task<TResult>();

  /// <summary>
  /// Creates a <see cref="TaskCompletionSource{TResult}"/> with the specified state.
  /// </summary>
  /// <param name="state">The state to use as the underlying <see cref="Task{TResult}"/>'s AsyncState.</param>
  public TaskCompletionSource(object state)
    => this.Task = new Task<TResult>();

  /// <summary>
  /// Creates a <see cref="TaskCompletionSource{TResult}"/> with the specified options.
  /// </summary>
  /// <param name="creationOptions">The options to use when creating the underlying <see cref="Task{TResult}"/>.</param>
  public TaskCompletionSource(TaskCreationOptions creationOptions)
    => this.Task = new Task<TResult>();

  /// <summary>
  /// Creates a <see cref="TaskCompletionSource{TResult}"/> with the specified state and options.
  /// </summary>
  /// <param name="state">The state to use as the underlying <see cref="Task{TResult}"/>'s AsyncState.</param>
  /// <param name="creationOptions">The options to use when creating the underlying <see cref="Task{TResult}"/>.</param>
  public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
    => this.Task = new Task<TResult>();

  /// <summary>
  /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
  /// </summary>
  /// <param name="result">The result value to bind to this <see cref="Task{TResult}"/>.</param>
  public void SetResult(TResult result) {
    if (!this.TrySetResult(result))
      throw new InvalidOperationException("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
  }

  /// <summary>
  /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
  /// </summary>
  /// <param name="result">The result value to bind to this <see cref="Task{TResult}"/>.</param>
  /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
  public bool TrySetResult(TResult result) {
    lock (this._lock) {
      if (this.Task.IsCompleted)
        return false;

      this.Task.SetResult(result);
      return true;
    }
  }

  /// <summary>
  /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Canceled"/> state.
  /// </summary>
  public void SetCanceled() {
    if (!this.TrySetCanceled())
      throw new InvalidOperationException("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
  }

  /// <summary>
  /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Canceled"/> state.
  /// </summary>
  /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
  public bool TrySetCanceled() {
    lock (this._lock) {
      if (this.Task.IsCompleted)
        return false;

      this.Task.SetCanceled();
      return true;
    }
  }

  /// <summary>
  /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Faulted"/> state.
  /// </summary>
  /// <param name="exception">The exception to bind to this <see cref="Task{TResult}"/>.</param>
  public void SetException(Exception exception) {
    if (!this.TrySetException(exception))
      throw new InvalidOperationException("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
  }

  /// <summary>
  /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Faulted"/> state.
  /// </summary>
  /// <param name="exceptions">The collection of exceptions to bind to this <see cref="Task{TResult}"/>.</param>
  public void SetException(IEnumerable<Exception> exceptions) {
    if (!this.TrySetException(exceptions))
      throw new InvalidOperationException("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
  }

  /// <summary>
  /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Faulted"/> state.
  /// </summary>
  /// <param name="exception">The exception to bind to this <see cref="Task{TResult}"/>.</param>
  /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
  public bool TrySetException(Exception exception) {
    ArgumentNullException.ThrowIfNull(exception);

    lock (this._lock) {
      if (this.Task.IsCompleted)
        return false;

      this.Task.SetException(exception);
      return true;
    }
  }

  /// <summary>
  /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Faulted"/> state.
  /// </summary>
  /// <param name="exceptions">The collection of exceptions to bind to this <see cref="Task{TResult}"/>.</param>
  /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
  public bool TrySetException(IEnumerable<Exception> exceptions) {
    ArgumentNullException.ThrowIfNull(exceptions);

    lock (this._lock) {
      if (this.Task.IsCompleted)
        return false;

      this.Task.SetException(exceptions);
      return true;
    }
  }

}

#endif
