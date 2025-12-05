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

#if !SUPPORTS_MAYBE_NULL_WHEN_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class MaybeNullWhenAttribute : Attribute {
  /// <summary>
  /// Gets the return value condition.
  /// </summary>
  public bool ReturnValue { get; }

  /// <summary>
  /// Initializes the attribute with the specified return value condition.
  /// </summary>
  /// <param name="returnValue">The return value condition. If the method returns this value, the associated parameter may be null.</param>
  public MaybeNullWhenAttribute(bool returnValue) => this.ReturnValue = returnValue;
}

#endif
