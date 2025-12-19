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
/// Represents an asynchronous operation.
/// </summary>
public class Task : IDisposable {

  private readonly Action? _action;
  private readonly Action<object?>? _actionWithState;
  private readonly object? _state;
  private readonly CancellationToken _cancellationToken;
  private readonly TaskCreationOptions _creationOptions;

  private volatile TaskStatus _status = TaskStatus.Created;
  private readonly ManualResetEvent _completionEvent = new(false);
  private AggregateException? _exception;
  private readonly List<Action<Task>> _continuations = [];
  private readonly object _lock = new();

  /// <summary>
  /// Gets the <see cref="TaskStatus"/> of this task.
  /// </summary>
  public TaskStatus Status => this._status;

  /// <summary>
  /// Gets whether this <see cref="Task"/> instance has completed execution.
  /// </summary>
  public bool IsCompleted => this._status >= TaskStatus.RanToCompletion;

  /// <summary>
  /// Gets whether the <see cref="Task"/> completed due to an unhandled exception.
  /// </summary>
  public bool IsFaulted => this._status == TaskStatus.Faulted;

  /// <summary>
  /// Gets whether this <see cref="Task"/> instance has completed execution due to being canceled.
  /// </summary>
  public bool IsCanceled => this._status == TaskStatus.Canceled;

  /// <summary>
  /// Gets the <see cref="AggregateException"/> that caused the <see cref="Task"/> to end prematurely.
  /// </summary>
  public AggregateException? Exception => this._exception;

  /// <summary>
  /// Gets the <see cref="TaskFactory"/> for creating tasks.
  /// </summary>
  public static TaskFactory Factory { get; } = new();

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  public Task(Action action)
    : this(action, CancellationToken.None, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action and cancellation token.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  public Task(Action action, CancellationToken cancellationToken)
    : this(action, cancellationToken, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action and creation options.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Action action, TaskCreationOptions creationOptions)
    : this(action, CancellationToken.None, creationOptions) { }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action, cancellation token and creation options.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) {
    this._action = action ?? throw new ArgumentNullException(nameof(action));
    this._cancellationToken = cancellationToken;
    this._creationOptions = creationOptions;
  }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action and state.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the action.</param>
  public Task(Action<object?> action, object? state)
    : this(action, state, CancellationToken.None, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action, state and cancellation token.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the action.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  public Task(Action<object?> action, object? state, CancellationToken cancellationToken)
    : this(action, state, cancellationToken, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task"/> with the specified action, state, cancellation token and creation options.
  /// </summary>
  /// <param name="action">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the action.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Action<object?> action, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) {
    this._actionWithState = action ?? throw new ArgumentNullException(nameof(action));
    this._state = state;
    this._cancellationToken = cancellationToken;
    this._creationOptions = creationOptions;
  }

  // Internal constructor for TaskCompletionSource
  internal Task() { }

  /// <summary>
  /// Starts the <see cref="Task"/>, scheduling it for execution to the current <see cref="TaskScheduler"/>.
  /// </summary>
  public void Start() => this.Start(TaskScheduler.Default);

  /// <summary>
  /// Starts the <see cref="Task"/>, scheduling it for execution to the specified <see cref="TaskScheduler"/>.
  /// </summary>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> with which to associate and execute this task.</param>
  public void Start(TaskScheduler scheduler) {
    if (this._status != TaskStatus.Created)
      throw new InvalidOperationException("Task has already been started.");

    this._status = TaskStatus.WaitingToRun;
    scheduler.QueueTask(this);
  }

  /// <summary>
  /// Runs the <see cref="Task"/> synchronously on the current <see cref="TaskScheduler"/>.
  /// </summary>
  public void RunSynchronously() => this.RunSynchronously(TaskScheduler.Default);

  /// <summary>
  /// Runs the <see cref="Task"/> synchronously on the specified <see cref="TaskScheduler"/>.
  /// </summary>
  /// <param name="scheduler">The scheduler on which to attempt to run this task inline.</param>
  public void RunSynchronously(TaskScheduler scheduler) {
    if (this._status != TaskStatus.Created)
      throw new InvalidOperationException("Task has already been started.");

    this._status = TaskStatus.WaitingToRun;
    this.ExecuteEntry();
  }

  internal virtual void ExecuteEntry() {
    if (this._cancellationToken.IsCancellationRequested) {
      this._status = TaskStatus.Canceled;
      this._completionEvent.Set();
      this._RunContinuations();
      return;
    }

    this._status = TaskStatus.Running;

    try {
      if (this._action != null)
        this._action();
      else
        this._actionWithState?.Invoke(this._state);

      this._status = TaskStatus.RanToCompletion;
    } catch (OperationCanceledException) {
      this._status = TaskStatus.Canceled;
    } catch (Exception ex) {
      this._exception = new AggregateException(ex);
      this._status = TaskStatus.Faulted;
    } finally {
      this._completionEvent.Set();
      this._RunContinuations();
    }
  }

  private void _RunContinuations() {
    Action<Task>[] continuations;
    lock (this._lock)
      continuations = [.. this._continuations];

    foreach (var continuation in continuations)
      try {
        continuation(this);
      } catch {
        // Ignore exceptions in continuations
      }
  }

  /// <summary>
  /// Waits for the <see cref="Task"/> to complete execution.
  /// </summary>
  public void Wait() => this.Wait(Timeout.Infinite, CancellationToken.None);

  /// <summary>
  /// Waits for the <see cref="Task"/> to complete execution within a specified number of milliseconds.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait.</param>
  /// <returns><see langword="true"/> if the task completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
  public bool Wait(int millisecondsTimeout) => this.Wait(millisecondsTimeout, CancellationToken.None);

  /// <summary>
  /// Waits for the <see cref="Task"/> to complete execution within a specified time interval.
  /// </summary>
  /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait.</param>
  /// <returns><see langword="true"/> if the task completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
  public bool Wait(TimeSpan timeout) => this.Wait((int)timeout.TotalMilliseconds, CancellationToken.None);

  /// <summary>
  /// Waits for the <see cref="Task"/> to complete execution.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
  public void Wait(CancellationToken cancellationToken) => this.Wait(Timeout.Infinite, cancellationToken);

  /// <summary>
  /// Waits for the <see cref="Task"/> to complete execution within a specified number of milliseconds.
  /// </summary>
  /// <param name="millisecondsTimeout">The number of milliseconds to wait.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
  /// <returns><see langword="true"/> if the task completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
  public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) {
    if (!this.IsCompleted) {
      var result = this._completionEvent.WaitOne(millisecondsTimeout);

      if (cancellationToken.IsCancellationRequested)
        throw new OperationCanceledException("The operation was canceled.");

      if (!result)
        return false;
    }

    if (this._exception != null)
      throw this._exception;

    return true;
  }

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task"/> completes.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task> continuationAction)
    => this.ContinueWith(continuationAction, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task"/> completes.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task> continuationAction, CancellationToken cancellationToken)
    => this.ContinueWith(continuationAction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task"/> completes.</param>
  /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
    => this.ContinueWith(continuationAction, CancellationToken.None, continuationOptions, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task"/> completes.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> to associate with the continuation task.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task> continuationAction, TaskScheduler scheduler)
    => this.ContinueWith(continuationAction, CancellationToken.None, TaskContinuationOptions.None, scheduler);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task"/> completes.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
  /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> to associate with the continuation task.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler) {
    ArgumentNullException.ThrowIfNull(continuationAction);
    ArgumentNullException.ThrowIfNull(scheduler);

    var continuationTask = new Task(() => continuationAction(this), cancellationToken, (TaskCreationOptions)((int)continuationOptions & 0x7));

    if (this.IsCompleted)
      Wrapper(this);
    else
      lock (this._lock)
        if (this.IsCompleted)
          Wrapper(this);
        else
          this._continuations.Add(Wrapper);

    return continuationTask;

    void Wrapper(Task _) {
      if (!this._ShouldRunContinuation(continuationOptions))
        return;

      if ((continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0)
        continuationTask.RunSynchronously(scheduler);
      else
        continuationTask.Start(scheduler);
    }
  }

  private bool _ShouldRunContinuation(TaskContinuationOptions options) {
    if ((options & TaskContinuationOptions.NotOnRanToCompletion) != 0 && this._status == TaskStatus.RanToCompletion)
      return false;
    if ((options & TaskContinuationOptions.NotOnFaulted) != 0 && this._status == TaskStatus.Faulted)
      return false;
    if ((options & TaskContinuationOptions.NotOnCanceled) != 0 && this._status == TaskStatus.Canceled)
      return false;
    return true;
  }

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
  /// <param name="continuationFunction">A function to run when the <see cref="Task"/> completes.</param>
  /// <returns>A new continuation <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
    => this.ContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task"/> completes.
  /// </summary>
  /// <typeparam name="TResult">The type of the result produced by the continuation.</typeparam>
  /// <param name="continuationFunction">A function to run when the <see cref="Task"/> completes.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
  /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> to associate with the continuation task.</param>
  /// <returns>A new continuation <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler) {
    ArgumentNullException.ThrowIfNull(continuationFunction);
    ArgumentNullException.ThrowIfNull(scheduler);

    var continuationTask = new Task<TResult>(() => continuationFunction(this), cancellationToken, (TaskCreationOptions)((int)continuationOptions & 0x7));

    if (this.IsCompleted)
      Wrapper(this);
    else
      lock (this._lock)
        if (this.IsCompleted)
          Wrapper(this);
        else
          this._continuations.Add(Wrapper);

    return continuationTask;

    void Wrapper(Task _) {
      if (!this._ShouldRunContinuation(continuationOptions))
        return;

      if ((continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0)
        continuationTask.RunSynchronously(scheduler);
      else
        continuationTask.Start(scheduler);
    }
  }

  /// <summary>
  /// Releases all resources used by the current instance of the <see cref="Task"/> class.
  /// </summary>
  public void Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Disposes the <see cref="Task"/>, releasing all of its unmanaged resources.
  /// </summary>
  /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources.</param>
  protected virtual void Dispose(bool disposing) {
    if (disposing)
      this._completionEvent?.Close();
  }

  // Internal methods for TaskCompletionSource
  internal void SetResult() {
    this._status = TaskStatus.RanToCompletion;
    this._completionEvent.Set();
    this._RunContinuations();
  }

  internal void SetCanceled() {
    this._status = TaskStatus.Canceled;
    this._completionEvent.Set();
    this._RunContinuations();
  }

  internal void SetException(Exception exception) {
    this._exception = exception as AggregateException ?? new(exception);
    this._status = TaskStatus.Faulted;
    this._completionEvent.Set();
    this._RunContinuations();
  }

  internal void SetException(IEnumerable<Exception> exceptions) {
    this._exception = new(exceptions);
    this._status = TaskStatus.Faulted;
    this._completionEvent.Set();
    this._RunContinuations();
  }

}

#endif
