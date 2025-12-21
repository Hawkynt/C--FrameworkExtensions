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

using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates the type of the async method builder that should be used by a language compiler to
/// build the attributed async method or to build the attributed type when used as the return type
/// of an async method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AsyncMethodBuilderAttribute : Attribute {
  /// <summary>
  /// Initializes the <see cref="AsyncMethodBuilderAttribute"/>.
  /// </summary>
  /// <param name="builderType">The <see cref="Type"/> of the associated builder.</param>
  public AsyncMethodBuilderAttribute(Type builderType) => this.BuilderType = builderType;

  /// <summary>
  /// Gets the <see cref="Type"/> of the associated builder.
  /// </summary>
  public Type BuilderType { get; }
}

/// <summary>
/// Represents a builder for asynchronous methods that return a <see cref="ValueTask"/>.
/// </summary>
public struct AsyncValueTaskMethodBuilder {
  private AsyncTaskMethodBuilder _methodBuilder;

  /// <summary>
  /// Gets the <see cref="ValueTask"/> for this builder.
  /// </summary>
  public ValueTask Task => new(this._methodBuilder.Task);

  /// <summary>
  /// Creates an instance of the <see cref="AsyncValueTaskMethodBuilder"/> struct.
  /// </summary>
  /// <returns>The initialized instance.</returns>
  public static AsyncValueTaskMethodBuilder Create() => new() { _methodBuilder = AsyncTaskMethodBuilder.Create() };

  /// <summary>
  /// Begins running the builder with the associated state machine.
  /// </summary>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="stateMachine">The state machine instance, passed by reference.</param>
  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.Start(ref stateMachine);

  /// <summary>
  /// Associates the builder with the specified state machine.
  /// </summary>
  /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
  public void SetStateMachine(IAsyncStateMachine stateMachine)
    => this._methodBuilder.SetStateMachine(stateMachine);

  /// <summary>
  /// Marks the task as successfully completed.
  /// </summary>
  public void SetResult()
    => this._methodBuilder.SetResult();

  /// <summary>
  /// Marks the task as failed and binds the specified exception to the task.
  /// </summary>
  /// <param name="exception">The exception to bind to the task.</param>
  public void SetException(Exception exception)
    => this._methodBuilder.SetException(exception);

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : INotifyCompletion
    where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
}

/// <summary>
/// Represents a builder for asynchronous methods that return a <see cref="ValueTask{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public struct AsyncValueTaskMethodBuilder<TResult> {
  private AsyncTaskMethodBuilder<TResult> _methodBuilder;

  /// <summary>
  /// Gets the <see cref="ValueTask{TResult}"/> for this builder.
  /// </summary>
  public ValueTask<TResult> Task => new(this._methodBuilder.Task);

  /// <summary>
  /// Creates an instance of the <see cref="AsyncValueTaskMethodBuilder{TResult}"/> struct.
  /// </summary>
  /// <returns>The initialized instance.</returns>
  public static AsyncValueTaskMethodBuilder<TResult> Create() => new() { _methodBuilder = AsyncTaskMethodBuilder<TResult>.Create() };

  /// <summary>
  /// Begins running the builder with the associated state machine.
  /// </summary>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="stateMachine">The state machine instance, passed by reference.</param>
  public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.Start(ref stateMachine);

  /// <summary>
  /// Associates the builder with the specified state machine.
  /// </summary>
  /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
  public void SetStateMachine(IAsyncStateMachine stateMachine)
    => this._methodBuilder.SetStateMachine(stateMachine);

  /// <summary>
  /// Marks the task as successfully completed.
  /// </summary>
  /// <param name="result">The result to use to complete the task.</param>
  public void SetResult(TResult result)
    => this._methodBuilder.SetResult(result);

  /// <summary>
  /// Marks the task as failed and binds the specified exception to the task.
  /// </summary>
  /// <param name="exception">The exception to bind to the task.</param>
  public void SetException(Exception exception)
    => this._methodBuilder.SetException(exception);

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : INotifyCompletion
    where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);

  /// <summary>
  /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
  /// </summary>
  /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
  /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
  /// <param name="awaiter">The awaiter.</param>
  /// <param name="stateMachine">The state machine.</param>
  public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
    where TAwaiter : ICriticalNotifyCompletion
    where TStateMachine : IAsyncStateMachine
    => this._methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
}

#endif
