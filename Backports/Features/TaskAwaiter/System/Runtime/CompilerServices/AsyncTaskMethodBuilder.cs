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

public struct AsyncTaskMethodBuilder {
  private TaskCompletionSource<object> _tcs;

  public Task Task => this._tcs.Task;

  public static AsyncTaskMethodBuilder Create() => new() { _tcs = new() };

  public void SetException(Exception exception) => this._tcs.SetException(exception);

  public void SetResult() => this._tcs.SetResult(null);

  public void SetStateMachine(IAsyncStateMachine stateMachine) { }

  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
      where TAwaiter : INotifyCompletion
      where TStateMachine : IAsyncStateMachine 
    => awaiter.OnCompleted(stateMachine.MoveNext)
    ;

  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine 
    => awaiter.OnCompleted(stateMachine.MoveNext)
    ;
}

public struct AsyncTaskMethodBuilder<TResult> {
  private TaskCompletionSource<TResult> _tcs;

  public Task<TResult> Task => this._tcs.Task;

  public static AsyncTaskMethodBuilder<TResult> Create() => new() { _tcs = new() };

  public void SetException(Exception exception) => this._tcs.SetException(exception);

  public void SetResult(TResult result) => this._tcs.SetResult(result);

  public void SetStateMachine(IAsyncStateMachine stateMachine) { }

  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();

  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
      where TAwaiter : INotifyCompletion
      where TStateMachine : IAsyncStateMachine
    => awaiter.OnCompleted(stateMachine.MoveNext)
    ;

  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine 
    => awaiter.OnCompleted(stateMachine.MoveNext)
    ;

}

#endif
