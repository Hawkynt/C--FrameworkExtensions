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

namespace System.Threading.Tasks;

/// <summary>
/// Represents an asynchronous operation that can return a value.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by this <see cref="Task{TResult}"/>.</typeparam>
public class Task<TResult> : Task {

  private readonly Func<TResult>? _function;
  private readonly Func<object?, TResult>? _functionWithState;
  private readonly object? _state;
  private TResult? _result;

  /// <summary>
  /// Gets the result value of this <see cref="Task{TResult}"/>.
  /// </summary>
  public TResult Result {
    get {
      this.Wait();
      if (this.Exception != null)
        throw this.Exception;
      return this._result!;
    }
  }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  public Task(Func<TResult> function)
    : this(function, CancellationToken.None, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function and cancellation token.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  public Task(Func<TResult> function, CancellationToken cancellationToken)
    : this(function, cancellationToken, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function and creation options.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Func<TResult> function, TaskCreationOptions creationOptions)
    : this(function, CancellationToken.None, creationOptions) { }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function, cancellation token and creation options.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
    : base(() => { }, cancellationToken, creationOptions)
    => this._function = function ?? throw new ArgumentNullException(nameof(function));

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function and state.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the function.</param>
  public Task(Func<object?, TResult> function, object? state)
    : this(function, state, CancellationToken.None, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function, state and cancellation token.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the function.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  public Task(Func<object?, TResult> function, object? state, CancellationToken cancellationToken)
    : this(function, state, cancellationToken, TaskCreationOptions.None) { }

  /// <summary>
  /// Initializes a new <see cref="Task{TResult}"/> with the specified function, state, cancellation token and creation options.
  /// </summary>
  /// <param name="function">The delegate that represents the code to execute in the task.</param>
  /// <param name="state">An object representing data to be used by the function.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the new task will observe.</param>
  /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
  public Task(Func<object?, TResult> function, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
    : base(() => { }, cancellationToken, creationOptions) {
    this._functionWithState = function ?? throw new ArgumentNullException(nameof(function));
    this._state = state;
  }

  // Internal constructor for TaskCompletionSource
  internal Task() { }

  internal override void ExecuteEntry() {
    try {
      if (this._function != null)
        this._result = this._function();
      else if (this._functionWithState != null)
        this._result = this._functionWithState(this._state);

      this.SetResult();
    } catch (OperationCanceledException) {
      this.SetCanceled();
    } catch (Exception ex) {
      this.SetException(ex);
    }
  }

  internal void SetResult(TResult result) {
    this._result = result;
    base.SetResult();
  }

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task{TResult}"/> completes.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task<TResult>> continuationAction)
    => this.ContinueWith(continuationAction, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
  /// </summary>
  /// <param name="continuationAction">An action to run when the <see cref="Task{TResult}"/> completes.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
  /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> to associate with the continuation task.</param>
  /// <returns>A new continuation <see cref="Task"/>.</returns>
  public Task ContinueWith(Action<Task<TResult>> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
    => base.ContinueWith(t => continuationAction((Task<TResult>)t), cancellationToken, continuationOptions, scheduler);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
  /// </summary>
  /// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
  /// <param name="continuationFunction">A function to run when the <see cref="Task{TResult}"/> completes.</param>
  /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
  public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
    => this.ContinueWith(continuationFunction, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

  /// <summary>
  /// Creates a continuation that executes when the target <see cref="Task{TResult}"/> completes.
  /// </summary>
  /// <typeparam name="TNewResult">The type of the result produced by the continuation.</typeparam>
  /// <param name="continuationFunction">A function to run when the <see cref="Task{TResult}"/> completes.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new continuation task.</param>
  /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> to associate with the continuation task.</param>
  /// <returns>A new continuation <see cref="Task{TNewResult}"/>.</returns>
  public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
    => base.ContinueWith(t => continuationFunction((Task<TResult>)t), cancellationToken, continuationOptions, scheduler);

}

#endif
