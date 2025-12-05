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

#if !SUPPORTS_ASYNC_ITERATOR_STATE_MACHINE_ATTRIBUTE

#if SUPPORTS_TASK_AWAITER || OFFICIAL_TASK_AWAITER

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates whether a method is an asynchronous iterator.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AsyncIteratorStateMachineAttribute : StateMachineAttribute {
  /// <summary>
  /// Initializes a new instance of the <see cref="AsyncIteratorStateMachineAttribute"/> class.
  /// </summary>
  /// <param name="stateMachineType">The type of the compiler-generated state machine type for the async iterator method.</param>
  public AsyncIteratorStateMachineAttribute(Type stateMachineType)
    : base(stateMachineType) { }
}

#endif

#endif
