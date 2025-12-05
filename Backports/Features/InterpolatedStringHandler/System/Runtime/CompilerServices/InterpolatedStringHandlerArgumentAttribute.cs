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

#if !SUPPORTS_INTERPOLATED_STRING_HANDLER

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates which arguments to a method involving an interpolated string handler should be passed to that handler.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute {
  /// <summary>
  /// Gets the names of the arguments that should be passed to the handler.
  /// </summary>
  public string[] Arguments { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="InterpolatedStringHandlerArgumentAttribute"/> class.
  /// </summary>
  /// <param name="argument">The name of the argument that should be passed to the handler.</param>
  public InterpolatedStringHandlerArgumentAttribute(string argument) => this.Arguments = new[] { argument };

  /// <summary>
  /// Initializes a new instance of the <see cref="InterpolatedStringHandlerArgumentAttribute"/> class.
  /// </summary>
  /// <param name="arguments">The names of the arguments that should be passed to the handler.</param>
  public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) => this.Arguments = arguments;
}

#endif
