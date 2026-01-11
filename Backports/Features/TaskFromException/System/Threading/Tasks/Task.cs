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

// Task.FromException was added in .NET Framework 4.6
// This provides a polyfill for .NET Framework 4.5 and earlier
#if !SUPPORTS_TASK_COMPLETEDTASK

namespace System.Threading.Tasks;

/// <summary>
/// Provides polyfill methods for <see cref="Task"/> on older frameworks.
/// </summary>
public static partial class TaskFromExceptionPolyfill {

  extension(Task) {

    /// <summary>
    /// Creates a <see cref="Task"/> that has completed with a specified exception.
    /// </summary>
    /// <param name="exception">The exception with which to complete the task.</param>
    /// <returns>The faulted task.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static Task FromException(Exception exception) {
      ArgumentNullException.ThrowIfNull(exception);
      var tcs = new TaskCompletionSource<object?>();
      tcs.SetException(exception);
      return tcs.Task;
    }

    /// <summary>
    /// Creates a <see cref="Task{TResult}"/> that has completed with a specified exception.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
    /// <param name="exception">The exception with which to complete the task.</param>
    /// <returns>The faulted task.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static Task<TResult> FromException<TResult>(Exception exception) {
      ArgumentNullException.ThrowIfNull(exception);
      var tcs = new TaskCompletionSource<TResult>();
      tcs.SetException(exception);
      return tcs.Task;
    }

  }

}

#endif
