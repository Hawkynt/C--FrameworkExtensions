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

#if !SUPPORTS_REQUIRES_LOCATION_ATTRIBUTE

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved for use by a compiler for tracking metadata.
/// This attribute should not be used by developers in source code.
/// </summary>
/// <remarks>
/// This attribute is used by the C# compiler to mark parameters that require a location
/// (for ref readonly parameters in C# 12).
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class RequiresLocationAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="RequiresLocationAttribute"/> class.
  /// </summary>
  public RequiresLocationAttribute() { }

}

#endif
