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
/// Represents the current stage in the lifecycle of a <see cref="Task"/>.
/// </summary>
public enum TaskStatus {
  /// <summary>
  /// The task has been initialized but has not yet been scheduled.
  /// </summary>
  Created = 0,

  /// <summary>
  /// The task is waiting to be activated and scheduled internally.
  /// </summary>
  WaitingForActivation = 1,

  /// <summary>
  /// The task has been scheduled for execution but has not yet begun executing.
  /// </summary>
  WaitingToRun = 2,

  /// <summary>
  /// The task is running but has not yet completed.
  /// </summary>
  Running = 3,

  /// <summary>
  /// The task has finished executing and is implicitly waiting for attached child tasks to complete.
  /// </summary>
  WaitingForChildrenToComplete = 4,

  /// <summary>
  /// The task completed execution successfully.
  /// </summary>
  RanToCompletion = 5,

  /// <summary>
  /// The task acknowledged cancellation by throwing an OperationCanceledException.
  /// </summary>
  Canceled = 6,

  /// <summary>
  /// The task completed due to an unhandled exception.
  /// </summary>
  Faulted = 7
}

/// <summary>
/// Specifies flags that control optional behavior for the creation and execution of tasks.
/// </summary>
[Flags]
public enum TaskCreationOptions {
  /// <summary>
  /// Specifies that the default behavior should be used.
  /// </summary>
  None = 0,

  /// <summary>
  /// Hints to the TaskScheduler that you want this task to run sooner.
  /// </summary>
  PreferFairness = 1,

  /// <summary>
  /// Specifies that a task will be a long-running operation.
  /// </summary>
  LongRunning = 2,

  /// <summary>
  /// Specifies that a task is attached to a parent in the task hierarchy.
  /// </summary>
  AttachedToParent = 4
}

/// <summary>
/// Specifies the behavior for a task that is created by using continuation methods.
/// </summary>
[Flags]
public enum TaskContinuationOptions {
  /// <summary>
  /// Default behavior.
  /// </summary>
  None = 0,

  /// <summary>
  /// Hints to the TaskScheduler that you want this task to run sooner.
  /// </summary>
  PreferFairness = 1,

  /// <summary>
  /// Specifies that a task will be a long-running operation.
  /// </summary>
  LongRunning = 2,

  /// <summary>
  /// Specifies that a task is attached to a parent in the task hierarchy.
  /// </summary>
  AttachedToParent = 4,

  /// <summary>
  /// Specifies that the continuation task should not be scheduled if its antecedent was canceled.
  /// </summary>
  NotOnCanceled = 262144,

  /// <summary>
  /// Specifies that the continuation task should not be scheduled if its antecedent threw an unhandled exception.
  /// </summary>
  NotOnFaulted = 131072,

  /// <summary>
  /// Specifies that the continuation task should not be scheduled if its antecedent ran to completion.
  /// </summary>
  NotOnRanToCompletion = 65536,

  /// <summary>
  /// Specifies that the continuation task should be scheduled only if its antecedent was canceled.
  /// </summary>
  OnlyOnCanceled = NotOnRanToCompletion | NotOnFaulted,

  /// <summary>
  /// Specifies that the continuation task should be scheduled only if its antecedent threw an unhandled exception.
  /// </summary>
  OnlyOnFaulted = NotOnRanToCompletion | NotOnCanceled,

  /// <summary>
  /// Specifies that the continuation task should be scheduled only if its antecedent ran to completion.
  /// </summary>
  OnlyOnRanToCompletion = NotOnFaulted | NotOnCanceled,

  /// <summary>
  /// Specifies that the continuation task should be executed synchronously.
  /// </summary>
  ExecuteSynchronously = 524288
}

#endif
