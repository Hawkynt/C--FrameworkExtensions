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

#if !SUPPORTS_INLINE_ARRAY

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that the instance's storage is sequentially replicated "length" times.
/// </summary>
/// <remarks>
/// This attribute can be used to define a struct that contains a fixed-size buffer of elements,
/// similar to how C# 12's inline arrays work.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class InlineArrayAttribute : Attribute {
  /// <summary>
  /// Gets the length of the inline array.
  /// </summary>
  public int Length { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="InlineArrayAttribute"/> class.
  /// </summary>
  /// <param name="length">The number of times the instance's storage is replicated.</param>
  public InlineArrayAttribute(int length) => this.Length = length;
}

#endif
