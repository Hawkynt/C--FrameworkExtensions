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

// ParamCollectionAttribute was added in .NET 9.0 for C# 13 params collections
#if !SUPPORTS_PARAM_COLLECTION_ATTRIBUTE

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that a parameter captures a collection of arguments.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is used by the C# compiler to implement the params collection feature
/// introduced in C# 13, which allows params to work with any collection type that has
/// an appropriate Add method or collection expression support.
/// </para>
/// <para>
/// Note: This polyfill only provides the attribute definition for compilation purposes.
/// The actual params collection functionality requires C# 13 compiler support.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class ParamCollectionAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="ParamCollectionAttribute"/> class.
  /// </summary>
  public ParamCollectionAttribute() { }

}

#endif
