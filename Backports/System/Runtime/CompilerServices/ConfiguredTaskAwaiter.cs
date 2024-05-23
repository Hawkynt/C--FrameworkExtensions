#if !SUPPORTS_TASK_AWAITER && SUPPORTS_ASYNC

#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

public readonly struct ConfiguredTaskAwaitable {
  private readonly Task _task;
  private readonly bool _continueOnCapturedContext;

  public ConfiguredTaskAwaitable(Task task, bool continueOnCapturedContext) {
    this._task = task ?? throw new ArgumentNullException(nameof(task));
    this._continueOnCapturedContext = continueOnCapturedContext;
  }

  public ConfiguredTaskAwaiter GetAwaiter() => new(this._task, this._continueOnCapturedContext);

  public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion {
    private readonly Task _task;
    private readonly bool _continueOnCapturedContext;

    public ConfiguredTaskAwaiter(Task task, bool continueOnCapturedContext) {
      this._task = task ?? throw new ArgumentNullException(nameof(task));
      this._continueOnCapturedContext = continueOnCapturedContext;
    }

    public bool IsCompleted => this._task.IsCompleted;

    public void OnCompleted(Action continuation) {
      if (continuation == null)
        throw new ArgumentNullException(nameof(continuation));

      this._task.ContinueWith(t => continuation(), this._continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default);
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

public readonly struct ConfiguredTaskAwaitable<TResult> {
  private readonly Task<TResult> _task;
  private readonly bool _continueOnCapturedContext;

  public ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext) {
    this._task = task ?? throw new ArgumentNullException(nameof(task));
    this._continueOnCapturedContext = continueOnCapturedContext;
  }

  public ConfiguredTaskAwaiter GetAwaiter() => new(this._task, this._continueOnCapturedContext);

  public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion {
    private readonly Task<TResult> _task;
    private readonly bool _continueOnCapturedContext;

    public ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext) {
      this._task = task ?? throw new ArgumentNullException(nameof(task));
      this._continueOnCapturedContext = continueOnCapturedContext;
    }

    public bool IsCompleted => this._task.IsCompleted;

    public void OnCompleted(Action continuation) {
      if (continuation == null)
        throw new ArgumentNullException(nameof(continuation));

      this._task.ContinueWith(t => continuation(), this._continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default);
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