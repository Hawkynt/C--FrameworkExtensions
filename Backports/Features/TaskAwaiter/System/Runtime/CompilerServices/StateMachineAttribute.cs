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

namespace System.Runtime.CompilerServices;

/// <summary>
/// Identifies a method as either an asynchronous method or an iterator method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class StateMachineAttribute : Attribute {
  /// <summary>
  /// Initializes a new instance of the <see cref="StateMachineAttribute"/> class.
  /// </summary>
  /// <param name="stateMachineType">The type of the compiler-generated state machine type for the method.</param>
  public StateMachineAttribute(Type stateMachineType) => this.StateMachineType = stateMachineType;

  /// <summary>
  /// Gets the type of the compiler-generated state machine type for the method.
  /// </summary>
  public Type StateMachineType { get; }
}

#endif
