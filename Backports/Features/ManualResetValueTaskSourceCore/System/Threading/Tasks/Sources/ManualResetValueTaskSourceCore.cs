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

// ManualResetValueTaskSourceCore was added in .NET Core 2.1
// Required for async iterators (async IAsyncEnumerable<T> methods)
// OFFICIAL_VALUETASK means System.Threading.Tasks.Extensions package is present,
// which includes ManualResetValueTaskSourceCore in its netstandard2.0 lib
// Uses ExceptionDispatchInfo which is polyfilled for older frameworks
#if !SUPPORTS_MANUAL_RESET_VALUE_TASK_SOURCE_CORE && !OFFICIAL_VALUETASK

#nullable disable

using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace System.Threading.Tasks.Sources;

/// <summary>
/// Flags passed to <see cref="IValueTaskSource.OnCompleted"/> and <see cref="IValueTaskSource{TResult}.OnCompleted"/>.
/// </summary>
[Flags]
public enum ValueTaskSourceOnCompletedFlags {
  /// <summary>No flags.</summary>
  None = 0,
  /// <summary>
  /// Indicates that the continuation should be invoked using the current synchronization context or task scheduler.
  /// </summary>
  UseSchedulingContext = 1,
  /// <summary>
  /// Indicates that the continuation should be invoked inline if possible rather than being scheduled.
  /// </summary>
  FlowExecutionContext = 2
}

/// <summary>
/// Indicates the state of an <see cref="IValueTaskSource"/> or <see cref="IValueTaskSource{TResult}"/>.
/// </summary>
public enum ValueTaskSourceStatus {
  /// <summary>The operation has not completed yet.</summary>
  Pending = 0,
  /// <summary>The operation completed successfully.</summary>
  Succeeded = 1,
  /// <summary>The operation completed with an error.</summary>
  Faulted = 2,
  /// <summary>The operation was canceled.</summary>
  Canceled = 3
}

/// <summary>
/// Represents an object that can be wrapped by a <see cref="ValueTask"/>.
/// </summary>
public interface IValueTaskSource {
  /// <summary>Gets the status of the current operation.</summary>
  /// <param name="token">The token that was passed when the operation was created.</param>
  /// <returns>The current status of the operation.</returns>
  ValueTaskSourceStatus GetStatus(short token);

  /// <summary>Schedules the continuation action for this operation.</summary>
  /// <param name="continuation">The action to invoke when the operation completes.</param>
  /// <param name="state">The state object to pass to the continuation.</param>
  /// <param name="token">The token that was passed when the operation was created.</param>
  /// <param name="flags">Flags describing how the continuation should be scheduled.</param>
  void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags);

  /// <summary>Gets the result of the operation.</summary>
  /// <param name="token">The token that was passed when the operation was created.</param>
  void GetResult(short token);
}

/// <summary>
/// Represents an object that can be wrapped by a <see cref="ValueTask{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IValueTaskSource<out TResult> {
  /// <summary>Gets the status of the current operation.</summary>
  /// <param name="token">The token that was passed when the operation was created.</param>
  /// <returns>The current status of the operation.</returns>
  ValueTaskSourceStatus GetStatus(short token);

  /// <summary>Schedules the continuation action for this operation.</summary>
  /// <param name="continuation">The action to invoke when the operation completes.</param>
  /// <param name="state">The state object to pass to the continuation.</param>
  /// <param name="token">The token that was passed when the operation was created.</param>
  /// <param name="flags">Flags describing how the continuation should be scheduled.</param>
  void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags);

  /// <summary>Gets the result of the operation.</summary>
  /// <param name="token">The token that was passed when the operation was created.</param>
  /// <returns>The result of the operation.</returns>
  TResult GetResult(short token);
}

/// <summary>
/// Provides the core logic for implementing a manual-reset <see cref="IValueTaskSource"/> or <see cref="IValueTaskSource{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
[StructLayout(LayoutKind.Auto)]
public struct ManualResetValueTaskSourceCore<TResult> {
  private Action<object?>? _continuation;
  private object? _continuationState;
  private ExecutionContext? _executionContext;
  private object? _capturedContext;
  private bool _completed;
  private TResult? _result;
  private ExceptionDispatchInfo? _error;

  /// <summary>Gets or sets whether to force continuations to run asynchronously.</summary>
  public bool RunContinuationsAsynchronously { get; set; }

  /// <summary>Gets the current version number.</summary>
  public short Version { get; private set; }

  /// <summary>Resets the state to allow the instance to be reused.</summary>
  public void Reset() {
    this.Version++;
    this._completed = false;
    this._result = default;
    this._error = null;
    this._continuation = null;
    this._continuationState = null;
    this._executionContext = null;
    this._capturedContext = null;
  }

  /// <summary>Completes with a successful result.</summary>
  /// <param name="result">The result.</param>
  public void SetResult(TResult result) {
    this._result = result;
    this._SignalCompletion();
  }

  /// <summary>Completes with an error.</summary>
  /// <param name="error">The exception.</param>
  public void SetException(Exception error) {
    this._error = ExceptionDispatchInfo.Capture(error);
    this._SignalCompletion();
  }

  /// <summary>Gets the status of the operation.</summary>
  /// <param name="token">The token to validate.</param>
  /// <returns>The status of the operation.</returns>
  public ValueTaskSourceStatus GetStatus(short token) {
    this._ValidateToken(token);

    if (!this._completed)
      return ValueTaskSourceStatus.Pending;

    if (this._error != null)
      return this._error.SourceException is OperationCanceledException
        ? ValueTaskSourceStatus.Canceled
        : ValueTaskSourceStatus.Faulted;

    return ValueTaskSourceStatus.Succeeded;
  }

  /// <summary>Gets the result of the operation.</summary>
  /// <param name="token">The token to validate.</param>
  /// <returns>The result.</returns>
  public TResult GetResult(short token) {
    this._ValidateToken(token);

    if (!this._completed)
      throw new InvalidOperationException("The operation has not completed.");

    this._error?.Throw();
    return this._result!;
  }

  /// <summary>Schedules a continuation.</summary>
  /// <param name="continuation">The continuation action.</param>
  /// <param name="state">The state to pass to the continuation.</param>
  /// <param name="token">The token to validate.</param>
  /// <param name="flags">Flags for scheduling.</param>
  public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) {
    ArgumentNullException.ThrowIfNull(continuation);
    this._ValidateToken(token);

    if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
      this._executionContext = ExecutionContext.Capture();

    if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0) {
      var sc = SynchronizationContext.Current;
      if (sc != null && sc.GetType() != typeof(SynchronizationContext))
        this._capturedContext = sc;
      else {
        var ts = TaskScheduler.Current;
        if (ts != TaskScheduler.Default)
          this._capturedContext = ts;
      }
    }

    this._continuationState = state;
    var previousContinuation = Interlocked.CompareExchange(ref this._continuation, continuation, null);

    if (previousContinuation == null)
      return;

    if (!ReferenceEquals(previousContinuation, _Sentinel))
      throw new InvalidOperationException("Multiple continuations are not supported.");

    this._InvokeContinuation(continuation, state);
  }

  private static readonly Action<object?> _Sentinel = _ => throw new InvalidOperationException();

  private void _ValidateToken(short token) {
    if (token != this.Version)
      throw new InvalidOperationException("The token does not match the current version.");
  }

  private void _SignalCompletion() {
    if (this._completed)
      throw new InvalidOperationException("The operation has already completed.");

    this._completed = true;

    var continuation = Interlocked.CompareExchange(ref this._continuation, _Sentinel, null);
    if (continuation != null)
      this._InvokeContinuation(continuation, this._continuationState);
  }

  private void _InvokeContinuation(Action<object?> continuation, object? state) {
    var capturedContext = this._capturedContext;
    this._capturedContext = null;

    switch (capturedContext) {
      case SynchronizationContext sc:
        sc.Post(s => {
          var tuple = (Tuple<Action<object>, object>)s!;
          tuple.Item1(tuple.Item2);
        }, Tuple.Create(continuation, state));
        break;
      case TaskScheduler ts:
        Task.Factory.StartNew(continuation, state, CancellationToken.None, Utilities.TaskCreationOptions.DenyChildAttach, ts);
        break;
      default: {
        if (this.RunContinuationsAsynchronously) {
          Utilities.ThreadPoolHelper.QueueUserWorkItem(s => {
            var tuple = (Tuple<Action<object>, object>)s!;
            tuple.Item1(tuple.Item2);
          }, Tuple.Create(continuation, state));
        } else {
          var ec = this._executionContext;
          if (ec == null)
            continuation(state);
          else {
            this._executionContext = null;
            ExecutionContext.Run(ec, s => {
              var tuple = (Tuple<Action<object>, object>)s!;
              tuple.Item1(tuple.Item2);
            }, Tuple.Create(continuation, state));
          }
        }

        break;
      }
    }
  }
}

#endif
