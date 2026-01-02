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

// Task.CompletedTask was added in .NET Framework 4.6
// This provides a polyfill for .NET Framework 4.5 and earlier
#if !SUPPORTS_TASK_COMPLETEDTASK

namespace System.Threading.Tasks;

/// <summary>
/// Provides polyfill properties for <see cref="Task"/> on older frameworks.
/// </summary>
public static partial class TaskCompletedTaskPolyfill {

  private static readonly Task _completedTask = _CreateCompletedTask();

  private static Task _CreateCompletedTask() {
    var tcs = new TaskCompletionSource<object?>();
    tcs.SetResult(null);
    return tcs.Task;
  }

  extension(Task) {

    /// <summary>
    /// Gets a task that has already completed successfully.
    /// </summary>
    /// <value>A successfully completed task.</value>
    public static Task CompletedTask => _completedTask;

  }

}

#endif
