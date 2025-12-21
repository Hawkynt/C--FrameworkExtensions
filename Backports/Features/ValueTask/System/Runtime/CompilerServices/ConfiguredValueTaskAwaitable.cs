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

#if !SUPPORTS_VALUE_TASK && !OFFICIAL_VALUETASK

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides an awaitable type that enables configured awaits on a <see cref="ValueTask"/>.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct ConfiguredValueTaskAwaitable {
  private readonly ValueTask _value;
  private readonly bool _continueOnCapturedContext;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfiguredValueTaskAwaitable"/> struct.
  /// </summary>
  /// <param name="value">The wrapped <see cref="ValueTask"/>.</param>
  /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
  internal ConfiguredValueTaskAwaitable(ValueTask value, bool continueOnCapturedContext) {
    this._value = value;
    this._continueOnCapturedContext = continueOnCapturedContext;
  }

  /// <summary>
  /// Returns an awaiter for this <see cref="ConfiguredValueTaskAwaitable"/> instance.
  /// </summary>
  /// <returns>The awaiter.</returns>
  public ConfiguredValueTaskAwaiter GetAwaiter() => new(this._value, this._continueOnCapturedContext);

  /// <summary>
  /// Provides an awaiter for a <see cref="ConfiguredValueTaskAwaitable"/>.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public readonly struct ConfiguredValueTaskAwaiter : ICriticalNotifyCompletion {
    private readonly ValueTask _value;
    private readonly bool _continueOnCapturedContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredValueTaskAwaiter"/> struct.
    /// </summary>
    /// <param name="value">The value to be awaited.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
    internal ConfiguredValueTaskAwaiter(ValueTask value, bool continueOnCapturedContext) {
      this._value = value;
      this._continueOnCapturedContext = continueOnCapturedContext;
    }

    /// <summary>
    /// Gets a value that indicates whether the operation has completed.
    /// </summary>
    public bool IsCompleted => this._value.IsCompleted;

    /// <summary>
    /// Ends the await on the completed operation.
    /// </summary>
    public void GetResult() {
      var task = this._value.AsTask();
      if (task.IsFaulted)
        _ThrowException(task.Exception!.InnerException!);
      if (task.IsCanceled)
        _ThrowException(new TaskCanceledException(task));
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void _ThrowException(Exception e) => throw e;
    
    /// <summary>
    /// Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable"/>.
    /// </summary>
    /// <param name="continuation">The action to invoke when the operation completes.</param>
    public void OnCompleted(Action continuation) {
      ArgumentNullException.ThrowIfNull(continuation);
      this._value.AsTask().ContinueWith(
        _ => continuation(),
        this._continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default
      );
    }

    /// <summary>
    /// Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable"/>.
    /// </summary>
    /// <param name="continuation">The action to invoke when the operation completes.</param>
    public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);
  }
}

/// <summary>
/// Provides an awaitable type that enables configured awaits on a <see cref="ValueTask{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct ConfiguredValueTaskAwaitable<TResult> {
  private readonly ValueTask<TResult> _value;
  private readonly bool _continueOnCapturedContext;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfiguredValueTaskAwaitable{TResult}"/> struct.
  /// </summary>
  /// <param name="value">The wrapped <see cref="ValueTask{TResult}"/>.</param>
  /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
  internal ConfiguredValueTaskAwaitable(ValueTask<TResult> value, bool continueOnCapturedContext) {
    this._value = value;
    this._continueOnCapturedContext = continueOnCapturedContext;
  }

  /// <summary>
  /// Returns an awaiter for this <see cref="ConfiguredValueTaskAwaitable{TResult}"/> instance.
  /// </summary>
  /// <returns>The awaiter.</returns>
  public ConfiguredValueTaskAwaiter GetAwaiter() => new(this._value, this._continueOnCapturedContext);

  /// <summary>
  /// Provides an awaiter for a <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public readonly struct ConfiguredValueTaskAwaiter : ICriticalNotifyCompletion {
    private readonly ValueTask<TResult> _value;
    private readonly bool _continueOnCapturedContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredValueTaskAwaiter"/> struct.
    /// </summary>
    /// <param name="value">The value to be awaited.</param>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
    internal ConfiguredValueTaskAwaiter(ValueTask<TResult> value, bool continueOnCapturedContext) {
      this._value = value;
      this._continueOnCapturedContext = continueOnCapturedContext;
    }

    /// <summary>
    /// Gets a value that indicates whether the operation has completed.
    /// </summary>
    public bool IsCompleted => this._value.IsCompleted;

    /// <summary>
    /// Ends the await on the completed operation.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public TResult GetResult() {
      var task = this._value.AsTask();
      if (task.IsFaulted)
        _ThrowException(task.Exception!.InnerException!);
      if (task.IsCanceled)
        _ThrowException(new TaskCanceledException(task));

      return task.Result;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void _ThrowException(Exception e) => throw e;

    /// <summary>
    /// Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.
    /// </summary>
    /// <param name="continuation">The action to invoke when the operation completes.</param>
    public void OnCompleted(Action continuation) {
      ArgumentNullException.ThrowIfNull(continuation);
      this._value.AsTask().ContinueWith(
        _ => continuation(),
        this._continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default
      );
    }

    /// <summary>
    /// Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.
    /// </summary>
    /// <param name="continuation">The action to invoke when the operation completes.</param>
    public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);
  }
}

#endif
