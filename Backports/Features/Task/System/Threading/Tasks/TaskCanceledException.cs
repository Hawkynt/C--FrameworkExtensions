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
/// Represents an exception used to communicate task cancellation.
/// </summary>
public class TaskCanceledException : OperationCanceledException {

  /// <summary>
  /// Gets the task associated with this exception.
  /// </summary>
  public Task? Task { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="TaskCanceledException"/> class.
  /// </summary>
  public TaskCanceledException()
    : base("A task was canceled.") { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TaskCanceledException"/> class with a specified error message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public TaskCanceledException(string message)
    : base(message) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TaskCanceledException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public TaskCanceledException(string message, Exception innerException)
    : base(message, innerException) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="TaskCanceledException"/> class with a reference to the <see cref="System.Threading.Tasks.Task"/> that has been canceled.
  /// </summary>
  /// <param name="task">A task that has been canceled.</param>
  public TaskCanceledException(Task task)
    : base("A task was canceled.")
    => this.Task = task;

}

#endif
