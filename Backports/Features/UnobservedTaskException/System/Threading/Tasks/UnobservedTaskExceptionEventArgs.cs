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

#if !SUPPORTS_UNOBSERVED_TASK_EXCEPTION

namespace System.Threading.Tasks;

/// <summary>
/// Provides data for the event that is raised when a faulted <see cref="Task"/>'s exception goes unobserved.
/// </summary>
/// <remarks>
/// This is a polyfill for .NET 2.0/3.5.
/// </remarks>
public class UnobservedTaskExceptionEventArgs : EventArgs {
  private bool _observed;

  /// <summary>
  /// Initializes a new instance with the unobserved exception.
  /// </summary>
  /// <param name="exception">The Exception that has gone unobserved.</param>
  public UnobservedTaskExceptionEventArgs(AggregateException exception) => this.Exception = exception;

  /// <summary>
  /// Gets whether this exception has been marked as "observed."
  /// </summary>
  public bool Observed => this._observed;

  /// <summary>
  /// Gets the unobserved exception.
  /// </summary>
  public AggregateException Exception { get; }

  /// <summary>
  /// Marks the <see cref="Exception"/> as "observed," thus preventing it from triggering exception escalation policy.
  /// </summary>
  public void SetObserved() => this._observed = true;
}

#endif
