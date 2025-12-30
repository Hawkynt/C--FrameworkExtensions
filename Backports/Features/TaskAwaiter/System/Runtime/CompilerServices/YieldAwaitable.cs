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

#if !SUPPORTS_TASK_RUN

using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides an awaitable context for switching into a target environment.
/// </summary>
/// <remarks>
/// This type is intended for compiler use rather than for use directly in code.
/// </remarks>
public readonly struct YieldAwaitable {
  /// <summary>
  /// Gets an awaiter for this <see cref="YieldAwaitable"/>.
  /// </summary>
  /// <returns>An awaiter for this awaitable.</returns>
  public YieldAwaiter GetAwaiter() => default;

  /// <summary>
  /// Provides an awaiter that switches into a target environment.
  /// </summary>
  /// <remarks>
  /// This type is intended for compiler use rather than for use directly in code.
  /// </remarks>
  public readonly struct YieldAwaiter : ICriticalNotifyCompletion {
    /// <summary>
    /// Gets whether a yield is not required.
    /// </summary>
    /// <value>Always returns <c>false</c>, as a context switch is always required for <see cref="Task.Yield"/>.</value>
    public bool IsCompleted => false;

    /// <summary>
    /// Posts the <paramref name="continuation"/> back to the current context.
    /// </summary>
    /// <param name="continuation">The action to invoke asynchronously.</param>
    public void OnCompleted(Action continuation) {
      ArgumentNullException.ThrowIfNull(continuation);
      ThreadPool.QueueUserWorkItem(_ => continuation(), null);
    }

    /// <summary>
    /// Posts the <paramref name="continuation"/> back to the current context.
    /// </summary>
    /// <param name="continuation">The action to invoke asynchronously.</param>
    [SecurityCritical]
    public void UnsafeOnCompleted(Action continuation) {
      ArgumentNullException.ThrowIfNull(continuation);
      ThreadPool.QueueUserWorkItem(_ => continuation(), null);
    }

    /// <summary>
    /// Ends the await operation.
    /// </summary>
    public void GetResult() { }
  }
}

#endif
