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

#if !SUPPORTS_COLLECTION_BUILDER

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that a collection type should be built using the specified builder type and method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class CollectionBuilderAttribute : Attribute {
  /// <summary>
  /// Gets the type that contains the builder method.
  /// </summary>
  public Type BuilderType { get; }

  /// <summary>
  /// Gets the name of the builder method.
  /// </summary>
  public string MethodName { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="CollectionBuilderAttribute"/> class.
  /// </summary>
  /// <param name="builderType">The type that contains the builder method.</param>
  /// <param name="methodName">The name of the builder method.</param>
  public CollectionBuilderAttribute(Type builderType, string methodName) {
    this.BuilderType = builderType;
    this.MethodName = methodName;
  }
}

#endif
