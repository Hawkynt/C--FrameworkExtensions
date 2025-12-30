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
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

public struct AsyncTaskMethodBuilder {
  private TaskCompletionSource<object?> _tcs;
  private IAsyncStateMachine? _boxedStateMachine;

  public Task Task => this._tcs.Task;

  public static AsyncTaskMethodBuilder Create() => new() { _tcs = new() };

  public void SetException(Exception exception) => this._tcs.SetException(exception);

  public void SetResult() => this._tcs.SetResult(null);

  public void SetStateMachine(IAsyncStateMachine stateMachine) => this._boxedStateMachine = stateMachine;

  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    => stateMachine.MoveNext();

  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : INotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    // Box the state machine if not already boxed - this ensures the continuation
    // operates on the actual state machine instance rather than a copy.
    // We must also call SetStateMachine on the boxed instance to ensure its builder
    // has a reference to itself for subsequent awaits.
    if (this._boxedStateMachine == null) {
      this._boxedStateMachine = stateMachine;
      this._boxedStateMachine.SetStateMachine(this._boxedStateMachine);
    }

    awaiter.OnCompleted(this._boxedStateMachine.MoveNext);
  }

  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    if (this._boxedStateMachine == null) {
      this._boxedStateMachine = stateMachine;
      this._boxedStateMachine.SetStateMachine(this._boxedStateMachine);
    }

    awaiter.OnCompleted(this._boxedStateMachine.MoveNext);
  }
}

public struct AsyncTaskMethodBuilder<TResult> {
  private TaskCompletionSource<TResult> _tcs;
  private IAsyncStateMachine? _boxedStateMachine;

  public Task<TResult> Task => this._tcs.Task;

  public static AsyncTaskMethodBuilder<TResult> Create() => new() { _tcs = new() };

  public void SetException(Exception exception) => this._tcs.SetException(exception);

  public void SetResult(TResult result) => this._tcs.SetResult(result);

  public void SetStateMachine(IAsyncStateMachine stateMachine) => this._boxedStateMachine = stateMachine;

  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    => stateMachine.MoveNext();

  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : INotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    if (this._boxedStateMachine == null) {
      this._boxedStateMachine = stateMachine;
      this._boxedStateMachine.SetStateMachine(this._boxedStateMachine);
    }

    awaiter.OnCompleted(this._boxedStateMachine.MoveNext);
  }

  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    if (this._boxedStateMachine == null) {
      this._boxedStateMachine = stateMachine;
      this._boxedStateMachine.SetStateMachine(this._boxedStateMachine);
    }

    awaiter.OnCompleted(this._boxedStateMachine.MoveNext);
  }
}

#endif
