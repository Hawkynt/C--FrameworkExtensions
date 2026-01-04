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

// Task.FromCanceled was added in .NET Framework 4.6
// This provides a polyfill for .NET Framework 4.5 and earlier
#if !SUPPORTS_TASK_COMPLETEDTASK

namespace System.Threading.Tasks;

/// <summary>
/// Provides polyfill methods for <see cref="Task"/> on older frameworks.
/// </summary>
public static partial class TaskFromCanceledPolyfill {

  extension(Task) {

    /// <summary>
    /// Creates a <see cref="Task"/> that's completed due to cancellation with a specified cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
    /// <returns>The canceled task.</returns>
    public static Task FromCanceled(CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<object?>();
      tcs.SetCanceled();
      return tcs.Task;
    }

    /// <summary>
    /// Creates a <see cref="Task{TResult}"/> that's completed due to cancellation with a specified cancellation token.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
    /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
    /// <returns>The canceled task.</returns>
    public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) {
      var tcs = new TaskCompletionSource<TResult>();
      tcs.SetCanceled();
      return tcs.Task;
    }

  }

}

#endif
