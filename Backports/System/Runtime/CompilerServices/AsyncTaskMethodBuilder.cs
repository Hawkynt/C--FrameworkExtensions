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