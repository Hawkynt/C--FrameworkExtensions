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

#if !SUPPORTS_REQUIRES_DYNAMIC_CODE_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that the specified method requires the ability to generate new code at runtime,
/// for example through <see cref="System.Reflection.Emit"/> or other mechanisms.
/// </summary>
/// <remarks>
/// This allows tools to understand which members require runtime code generation.
/// This attribute is applied to methods that may not work when running in ahead-of-time (AOT)
/// compiled environments.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
public sealed class RequiresDynamicCodeAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute"/> class
  /// with the specified message.
  /// </summary>
  /// <param name="message">
  /// A message that contains information about the usage of dynamic code.
  /// </param>
  public RequiresDynamicCodeAttribute(string message)
    => this.Message = message;

  /// <summary>
  /// Gets a message that contains information about the usage of dynamic code.
  /// </summary>
  public string Message { get; }

  /// <summary>
  /// Gets or sets an optional URL that contains more information about the method,
  /// why it requires dynamic code, and what options a consumer has to deal with it.
  /// </summary>
  public string? Url { get; set; }

}

#endif
