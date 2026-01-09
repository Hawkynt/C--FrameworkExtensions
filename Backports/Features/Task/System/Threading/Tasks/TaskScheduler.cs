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
/// Represents an object that handles the low-level work of queuing tasks onto threads.
/// </summary>
public abstract class TaskScheduler {
  /// <summary>
  /// Occurs when a faulted task's unobserved exception is about to trigger exception escalation policy.
  /// </summary>
  public static event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;

  /// <summary>
  /// Raises the <see cref="UnobservedTaskException"/> event.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The event data.</param>
  internal static void PublishUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    => UnobservedTaskException?.Invoke(sender, e);

  /// <summary>
  /// Gets the default <see cref="TaskScheduler"/> instance that is provided by the .NET Framework.
  /// </summary>
  public static TaskScheduler Default { get; } = new ThreadPoolTaskScheduler();

  /// <summary>
  /// Gets the <see cref="TaskScheduler"/> associated with the currently executing task.
  /// </summary>
  public static TaskScheduler Current => Default;

  /// <summary>
  /// Queues a <see cref="Task"/> to the scheduler.
  /// </summary>
  /// <param name="task">The <see cref="Task"/> to be queued.</param>
  protected internal abstract void QueueTask(Task task);

  /// <summary>
  /// Determines whether the provided <see cref="Task"/> can be executed synchronously in this call.
  /// </summary>
  /// <param name="task">The <see cref="Task"/> to be executed.</param>
  /// <param name="taskWasPreviouslyQueued">A Boolean denoting whether the task has previously been queued.</param>
  /// <returns>A Boolean value indicating whether the task was executed inline.</returns>
  protected abstract bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued);

  /// <summary>
  /// For debugger support only, generates an enumerable of <see cref="Task"/> instances currently queued to the scheduler.
  /// </summary>
  /// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.</returns>
  protected abstract IEnumerable<Task> GetScheduledTasks();

  /// <summary>
  /// Attempts to execute the provided <see cref="Task"/> on this scheduler.
  /// </summary>
  /// <param name="task">The task to execute.</param>
  /// <returns><see langword="true"/> if the task was executed; otherwise, <see langword="false"/>.</returns>
  protected bool TryExecuteTask(Task task) {
    if (task == null)
      return false;

    task.ExecuteEntry();
    return true;
  }

  /// <summary>
  /// Gets the maximum concurrency level supported by this scheduler.
  /// </summary>
  public virtual int MaximumConcurrencyLevel => int.MaxValue;

}

internal sealed class ThreadPoolTaskScheduler : TaskScheduler {

  protected internal override void QueueTask(Task task)
    => Utilities.ThreadPoolHelper.QueueUserWorkItem(_ => this.TryExecuteTask(task));

  protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
    this.TryExecuteTask(task);
    return true;
  }

  protected override IEnumerable<Task> GetScheduledTasks() => [];

}

#endif
