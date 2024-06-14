#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_TASK_AWAITER && SUPPORTS_ASYNC
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

public readonly struct ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext) {
  private readonly Task _task = task ?? throw new ArgumentNullException(nameof(task));

  public ConfiguredTaskAwaiter GetAwaiter() => new(this._task, continueOnCapturedContext);

  public readonly struct ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext) : ICriticalNotifyCompletion {
    private readonly Task _task = task ?? throw new ArgumentNullException(nameof(task));

    public bool IsCompleted => this._task.IsCompleted;

    public void OnCompleted(Action continuation) {
      if (continuation == null)
        throw new ArgumentNullException(nameof(continuation));

      this._task.ContinueWith(t => continuation(), continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default);
    }

    public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);

    public void GetResult() {
      if (this._task.IsFaulted)
        throw this._task.Exception.InnerException;

      if (this._task.IsCanceled)
        throw new TaskCanceledException(this._task);
    }
  }
}

public readonly struct ConfiguredTaskAwaitable<TResult>(Task<TResult> task, bool continueOnCapturedContext) {
  private readonly Task<TResult> _task = task ?? throw new ArgumentNullException(nameof(task));

  public ConfiguredTaskAwaiter GetAwaiter() => new(this._task, continueOnCapturedContext);

  public readonly struct ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext) : ICriticalNotifyCompletion {
    private readonly Task<TResult> _task = task ?? throw new ArgumentNullException(nameof(task));

    public bool IsCompleted => this._task.IsCompleted;

    public void OnCompleted(Action continuation) {
      if (continuation == null)
        throw new ArgumentNullException(nameof(continuation));

      this._task.ContinueWith(t => continuation(), continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default);
    }

    public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);

    public TResult GetResult() {
      if (this._task.IsFaulted)
        throw this._task.Exception.InnerException;
      
      if (this._task.IsCanceled)
        throw new TaskCanceledException(this._task);

      return this._task.Result;
    }
  }
}

#endif
