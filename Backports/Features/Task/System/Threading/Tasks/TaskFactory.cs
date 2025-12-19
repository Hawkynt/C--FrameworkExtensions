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
/// Provides support for creating and scheduling <see cref="Task"/> objects.
/// </summary>
public class TaskFactory {
  /// <summary>
  /// Initializes a <see cref="TaskFactory"/> instance with the default configuration.
  /// </summary>
  public TaskFactory()
    : this(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default) { }

  /// <summary>
  /// Initializes a <see cref="TaskFactory"/> instance with the specified configuration.
  /// </summary>
  /// <param name="cancellationToken">The default <see cref="CancellationToken"/> that will be assigned to tasks created by this <see cref="TaskFactory"/>.</param>
  public TaskFactory(CancellationToken cancellationToken)
    : this(cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default) { }

  /// <summary>
  /// Initializes a <see cref="TaskFactory"/> instance with the specified configuration.
  /// </summary>
  /// <param name="scheduler">The default <see cref="TaskScheduler"/> to use to schedule any tasks created with this <see cref="TaskFactory"/>.</param>
  public TaskFactory(TaskScheduler scheduler)
    : this(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler) { }

  /// <summary>
  /// Initializes a <see cref="TaskFactory"/> instance with the specified configuration.
  /// </summary>
  /// <param name="creationOptions">The default <see cref="TaskCreationOptions"/> to use when creating tasks with this <see cref="TaskFactory"/>.</param>
  /// <param name="continuationOptions">The default <see cref="TaskContinuationOptions"/> to use when creating continuation tasks with this <see cref="TaskFactory"/>.</param>
  public TaskFactory(TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
    : this(CancellationToken.None, creationOptions, continuationOptions, TaskScheduler.Default) { }

  /// <summary>
  /// Initializes a <see cref="TaskFactory"/> instance with the specified configuration.
  /// </summary>
  /// <param name="cancellationToken">The default <see cref="CancellationToken"/> that will be assigned to tasks created by this <see cref="TaskFactory"/>.</param>
  /// <param name="creationOptions">The default <see cref="TaskCreationOptions"/> to use when creating tasks with this <see cref="TaskFactory"/>.</param>
  /// <param name="continuationOptions">The default <see cref="TaskContinuationOptions"/> to use when creating continuation tasks with this <see cref="TaskFactory"/>.</param>
  /// <param name="scheduler">The default <see cref="TaskScheduler"/> to use to schedule any tasks created with this <see cref="TaskFactory"/>.</param>
  public TaskFactory(CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler scheduler) {
    this.CancellationToken = cancellationToken;
    this.CreationOptions = creationOptions;
    this.ContinuationOptions = continuationOptions;
    this.Scheduler = scheduler ?? TaskScheduler.Default;
  }

  /// <summary>
  /// Gets the default <see cref="CancellationToken"/> for this <see cref="TaskFactory"/>.
  /// </summary>
  public CancellationToken CancellationToken { get; }

  /// <summary>
  /// Gets the default <see cref="TaskScheduler"/> for this <see cref="TaskFactory"/>.
  /// </summary>
  public TaskScheduler Scheduler { get; }

  /// <summary>
  /// Gets the default <see cref="TaskCreationOptions"/> for this <see cref="TaskFactory"/>.
  /// </summary>
  public TaskCreationOptions CreationOptions { get; }

  /// <summary>
  /// Gets the default <see cref="TaskContinuationOptions"/> for this <see cref="TaskFactory"/>.
  /// </summary>
  public TaskContinuationOptions ContinuationOptions { get; }

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action action)
    => this.StartNew(action, this.CancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action action, CancellationToken cancellationToken)
    => this.StartNew(action, cancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task"/>.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action action, TaskCreationOptions creationOptions)
    => this.StartNew(action, this.CancellationToken, creationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task"/>.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> that is used to schedule the created <see cref="Task"/>.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler) {
    var task = new Task(action, cancellationToken, creationOptions);
    task.Start(scheduler);
    return task;
  }

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <param name="state">An object containing data to be used by the action delegate.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action<object?> action, object? state)
    => this.StartNew(action, state, this.CancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task"/>.
  /// </summary>
  /// <param name="action">The action delegate to execute asynchronously.</param>
  /// <param name="state">An object containing data to be used by the action delegate.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task"/>.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> that is used to schedule the created <see cref="Task"/>.</param>
  /// <returns>The started <see cref="Task"/>.</returns>
  public Task StartNew(Action<object?> action, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler) {
    var task = new Task(action, state, cancellationToken, creationOptions);
    task.Start(scheduler);
    return task;
  }

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<TResult> function)
    => this.StartNew(function, this.CancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<TResult> function, CancellationToken cancellationToken)
    => this.StartNew(function, cancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task{TResult}"/>.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
    => this.StartNew(function, this.CancellationToken, creationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task{TResult}"/>.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> that is used to schedule the created <see cref="Task{TResult}"/>.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler) {
    var task = new Task<TResult>(function, cancellationToken, creationOptions);
    task.Start(scheduler);
    return task;
  }

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <param name="state">An object containing data to be used by the function delegate.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<object?, TResult> function, object? state)
    => this.StartNew(function, state, this.CancellationToken, this.CreationOptions, this.Scheduler);

  /// <summary>
  /// Creates and starts a <see cref="Task{TResult}"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the result available through the <see cref="Task{TResult}"/>.</typeparam>
  /// <param name="function">A function delegate that returns the future result to be available through the <see cref="Task{TResult}"/>.</param>
  /// <param name="state">An object containing data to be used by the function delegate.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
  /// <param name="creationOptions">A <see cref="TaskCreationOptions"/> value that controls the behavior of the created <see cref="Task{TResult}"/>.</param>
  /// <param name="scheduler">The <see cref="TaskScheduler"/> that is used to schedule the created <see cref="Task{TResult}"/>.</param>
  /// <returns>The started <see cref="Task{TResult}"/>.</returns>
  public Task<TResult> StartNew<TResult>(Func<object?, TResult> function, object? state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler) {
    var task = new Task<TResult>(function, state, cancellationToken, creationOptions);
    task.Start(scheduler);
    return task;
  }

}

#endif
