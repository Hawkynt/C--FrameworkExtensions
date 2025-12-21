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

#if !SUPPORTS_VALUE_TASK && !OFFICIAL_VALUETASK

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading.Tasks;

/// <summary>
/// Provides an awaitable result of an asynchronous operation.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder))]
public readonly struct ValueTask : IEquatable<ValueTask> {
  private static readonly Task _completedTask = _CreateCompletedTask();

  private static Task _CreateCompletedTask() {
    var tcs = new TaskCompletionSource<object?>();
    tcs.SetResult(null);
    return tcs.Task;
  }

  private readonly Task? _task;

  /// <summary>
  /// Initializes a new instance of the <see cref="ValueTask"/> structure using the supplied task that represents the operation.
  /// </summary>
  /// <param name="task">The task that represents the operation.</param>
  public ValueTask(Task task) => this._task = task ?? throw new ArgumentNullException(nameof(task));

  /// <summary>
  /// Gets a value that indicates whether the operation has completed.
  /// </summary>
  public bool IsCompleted => this._task?.IsCompleted ?? true;

  /// <summary>
  /// Gets a value that indicates whether the operation completed successfully.
  /// </summary>
  public bool IsCompletedSuccessfully => this._task == null || this._task.Status == TaskStatus.RanToCompletion;

  /// <summary>
  /// Gets a value that indicates whether the operation completed with an exception.
  /// </summary>
  public bool IsFaulted => this._task?.IsFaulted ?? false;

  /// <summary>
  /// Gets a value that indicates whether the operation was canceled.
  /// </summary>
  public bool IsCanceled => this._task?.IsCanceled ?? false;

  /// <summary>
  /// Retrieves a <see cref="Task"/> object that represents this <see cref="ValueTask"/>.
  /// </summary>
  /// <returns>The <see cref="Task"/> object that is wrapped in this <see cref="ValueTask"/> if one exists, or a new <see cref="Task"/> object that represents the result.</returns>
  public Task AsTask() => this._task ?? _completedTask;

  /// <summary>
  /// Creates an awaiter used to await this <see cref="ValueTask"/>.
  /// </summary>
  /// <returns>The awaiter instance.</returns>
  public ValueTaskAwaiter GetAwaiter() => new(this);

  /// <summary>
  /// Configures an awaiter used to await this <see cref="ValueTask"/>.
  /// </summary>
  /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
  /// <returns>The configured awaiter.</returns>
  public ConfiguredValueTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => new(this, continueOnCapturedContext);

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is ValueTask other && this.Equals(other);

  /// <inheritdoc />
  public bool Equals(ValueTask other) => this._task == other._task;

  /// <inheritdoc />
  public override int GetHashCode() => this._task?.GetHashCode() ?? 0;

  /// <summary>Determines whether two <see cref="ValueTask"/> values are equal.</summary>
  public static bool operator ==(ValueTask left, ValueTask right) => left.Equals(right);

  /// <summary>Determines whether two <see cref="ValueTask"/> values are not equal.</summary>
  public static bool operator !=(ValueTask left, ValueTask right) => !left.Equals(right);
}

/// <summary>
/// Provides a value type that wraps a <see cref="Task{TResult}"/> and a <typeparamref name="TResult"/>,
/// only one of which is used.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
[StructLayout(LayoutKind.Auto)]
[AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder<>))]
public readonly struct ValueTask<TResult> : IEquatable<ValueTask<TResult>> {
  private readonly Task<TResult>? _task;
  private readonly TResult? _result;

  /// <summary>
  /// Initializes a new instance of the <see cref="ValueTask{TResult}"/> structure using the supplied result of a successful operation.
  /// </summary>
  /// <param name="result">The result.</param>
  public ValueTask(TResult result) {
    this._result = result;
    this._task = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ValueTask{TResult}"/> structure using the supplied task that represents the operation.
  /// </summary>
  /// <param name="task">The task that represents the operation.</param>
  public ValueTask(Task<TResult> task) {
    this._task = task ?? throw new ArgumentNullException(nameof(task));
    this._result = default;
  }

  /// <summary>
  /// Gets the result of the operation.
  /// </summary>
  public TResult Result => this._task != null ? this._task.Result : this._result!;

  /// <summary>
  /// Gets a value that indicates whether the operation has completed.
  /// </summary>
  public bool IsCompleted => this._task?.IsCompleted ?? true;

  /// <summary>
  /// Gets a value that indicates whether the operation completed successfully.
  /// </summary>
  public bool IsCompletedSuccessfully => this._task == null || this._task.Status == TaskStatus.RanToCompletion;

  /// <summary>
  /// Gets a value that indicates whether the operation completed with an exception.
  /// </summary>
  public bool IsFaulted => this._task?.IsFaulted ?? false;

  /// <summary>
  /// Gets a value that indicates whether the operation was canceled.
  /// </summary>
  public bool IsCanceled => this._task?.IsCanceled ?? false;

  /// <summary>
  /// Retrieves a <see cref="Task{TResult}"/> object that represents this <see cref="ValueTask{TResult}"/>.
  /// </summary>
  /// <returns>The <see cref="Task{TResult}"/> object that is wrapped in this <see cref="ValueTask{TResult}"/> if one exists, or a new <see cref="Task{TResult}"/> object that represents the result.</returns>
  public Task<TResult> AsTask() => this._task ?? Task.FromResult(this._result!);

  /// <summary>
  /// Creates an awaiter used to await this <see cref="ValueTask{TResult}"/>.
  /// </summary>
  /// <returns>The awaiter instance.</returns>
  public ValueTaskAwaiter<TResult> GetAwaiter() => new(this);

  /// <summary>
  /// Configures an awaiter used to await this <see cref="ValueTask{TResult}"/>.
  /// </summary>
  /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
  /// <returns>The configured awaiter.</returns>
  public ConfiguredValueTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) => new(this, continueOnCapturedContext);

  /// <inheritdoc />
  public override bool Equals(object? obj) => obj is ValueTask<TResult> other && this.Equals(other);

  /// <inheritdoc />
  public bool Equals(ValueTask<TResult> other) =>
    this._task != null || other._task != null
      ? this._task == other._task
      : EqualityComparer<TResult>.Default.Equals(this._result!, other._result!);

  /// <inheritdoc />
  public override int GetHashCode() => this._task?.GetHashCode() ?? (this._result?.GetHashCode() ?? 0);

  /// <summary>Determines whether two <see cref="ValueTask{TResult}"/> values are equal.</summary>
  public static bool operator ==(ValueTask<TResult> left, ValueTask<TResult> right) => left.Equals(right);

  /// <summary>Determines whether two <see cref="ValueTask{TResult}"/> values are not equal.</summary>
  public static bool operator !=(ValueTask<TResult> left, ValueTask<TResult> right) => !left.Equals(right);
}

#endif
