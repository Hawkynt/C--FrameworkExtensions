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

#if !SUPPORTS_TASK_AWAITER && !OFFICIAL_TASK_AWAITER
using Guard;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

public readonly struct TaskAwaiter(Task task) : ICriticalNotifyCompletion {
  private readonly Task _task = task ?? AlwaysThrow.ArgumentNullException<Task>(nameof(task));

  public bool IsCompleted => this._task.IsCompleted;

  public void OnCompleted(Action continuation) {
    if (continuation == null)
      AlwaysThrow.ArgumentNullException(nameof(continuation));

    this._task.ContinueWith(t => continuation(), TaskScheduler.Current);
  }

  public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);

  public void GetResult() {
    // Wait for the task to complete if it hasn't already
    try {
      this._task.Wait();
    } catch (AggregateException ae) {
      // Unwrap the AggregateException to match BCL behavior - throw the first inner exception
      throw ae.InnerExceptions.Count == 1 ? ae.InnerExceptions[0] : ae;
    }
  }
}

public readonly struct TaskAwaiter<TResult>(Task<TResult> task) : ICriticalNotifyCompletion {
  private readonly Task<TResult> _task = task ?? AlwaysThrow.ArgumentNullException<Task<TResult>>(nameof(task));

  public bool IsCompleted => this._task.IsCompleted;

  public void OnCompleted(Action continuation) {
    if (continuation == null)
      AlwaysThrow.ArgumentNullException(nameof(continuation));

    this._task.ContinueWith(t => continuation(), TaskScheduler.Current);
  }

  public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);

  public TResult GetResult() {
    // Wait for the task to complete if it hasn't already
    try {
      this._task.Wait();
    } catch (AggregateException ae) {
      // Unwrap the AggregateException to match BCL behavior - throw the first inner exception
      throw ae.InnerExceptions.Count == 1 ? ae.InnerExceptions[0] : ae;
    }

    return this._task.Result;
  }
}

#endif
