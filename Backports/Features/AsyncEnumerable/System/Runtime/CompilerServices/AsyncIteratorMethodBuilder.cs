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

#if !SUPPORTS_ASYNC_ITERATOR_METHOD_BUILDER && !OFFICIAL_ASYNC_ENUMERABLE

namespace System.Runtime.CompilerServices;

/// <summary>
/// Represents a builder for asynchronous iterators.
/// </summary>
public struct AsyncIteratorMethodBuilder {
  /// <summary>
  /// Creates an instance of the <see cref="AsyncIteratorMethodBuilder"/> struct.
  /// </summary>
  /// <returns>The initialized instance.</returns>
  public static AsyncIteratorMethodBuilder Create() => default;

  /// <summary>
  /// Invokes <see cref="IAsyncStateMachine.MoveNext"/> on the state machine while guarding the <see cref="ExecutionContext"/>.
  /// </summary>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="stateMachine">The state machine instance, passed by reference.</param>
  public void MoveNext<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    => stateMachine.MoveNext();

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : INotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    var boxedStateMachine = (IAsyncStateMachine)stateMachine;
    awaiter.OnCompleted(boxedStateMachine.MoveNext);
  }

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine {
    var boxedStateMachine = (IAsyncStateMachine)stateMachine;
    awaiter.UnsafeOnCompleted(boxedStateMachine.MoveNext);
  }

  /// <summary>
  /// Marks iteration as being completed, whether successfully or otherwise.
  /// </summary>
  public void Complete() { }
}

#endif
