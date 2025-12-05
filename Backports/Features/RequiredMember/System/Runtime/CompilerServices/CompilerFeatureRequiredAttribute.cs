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

#if !SUPPORTS_REQUIRED_MEMBERS

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public sealed class CompilerFeatureRequiredAttribute : Attribute {
  /// <summary>
  /// The name of the compiler feature.
  /// </summary>
  public string FeatureName { get; }

  /// <summary>
  /// If true, the compiler can choose to allow access to the location where this attribute is applied if it does not understand <see cref="FeatureName"/>.
  /// </summary>
  public bool IsOptional { get; init; }

  /// <summary>
  /// The <see cref="FeatureName"/> used for the ref structs C# feature.
  /// </summary>
  public const string RefStructs = nameof(RefStructs);

  /// <summary>
  /// The <see cref="FeatureName"/> used for the required members C# feature.
  /// </summary>
  public const string RequiredMembers = nameof(RequiredMembers);

  /// <summary>
  /// Initializes a new instance of the <see cref="CompilerFeatureRequiredAttribute"/> class.
  /// </summary>
  /// <param name="featureName">The name of the required compiler feature.</param>
  public CompilerFeatureRequiredAttribute(string featureName) => this.FeatureName = featureName;
}

#endif
