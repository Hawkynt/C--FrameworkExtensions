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

// FeatureGuardAttribute was added in .NET 9.0
#if !SUPPORTS_FEATURE_GUARD_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that the attributed property is a guard for a feature.
/// </summary>
/// <remarks>
/// <para>
/// When a property returns true, the feature is considered available.
/// This is used by the trimmer and AOT compiler to understand feature availability patterns.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class FeatureGuardAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureGuardAttribute"/> class.
  /// </summary>
  /// <param name="featureType">The type that represents the feature being guarded.</param>
  public FeatureGuardAttribute(Type featureType) => this.FeatureType = featureType;

  /// <summary>
  /// Gets the type that represents the feature being guarded.
  /// </summary>
  public Type FeatureType { get; }

}

#endif
