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

#if !SUPPORTS_DOES_NOT_RETURN_IF_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that the method will not return if the associated <see cref="Boolean"/> parameter is passed the specified value.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class DoesNotReturnIfAttribute : Attribute {
  /// <summary>
  /// Gets the condition parameter value.
  /// </summary>
  public bool ParameterValue { get; }

  /// <summary>
  /// Initializes the attribute with the specified parameter value.
  /// </summary>
  /// <param name="parameterValue">The condition parameter value. Code after the method will be considered unreachable by flow analysis if the argument to the associated parameter matches this value.</param>
  public DoesNotReturnIfAttribute(bool parameterValue) => this.ParameterValue = parameterValue;
}

#endif
