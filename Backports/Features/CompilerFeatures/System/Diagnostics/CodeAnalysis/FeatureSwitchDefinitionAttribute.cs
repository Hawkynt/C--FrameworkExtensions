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

// FeatureSwitchDefinitionAttribute was added in .NET 9.0
#if !SUPPORTS_FEATURE_SWITCH_DEFINITION_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that the attributed property is a definition of a feature switch.
/// </summary>
/// <remarks>
/// <para>
/// The property should return true when the feature is enabled and false when disabled.
/// The switch name should be a well-known name that can be configured at build time
/// or in runtimeconfig.json.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class FeatureSwitchDefinitionAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureSwitchDefinitionAttribute"/> class.
  /// </summary>
  /// <param name="switchName">The name of the feature switch.</param>
  public FeatureSwitchDefinitionAttribute(string switchName) => this.SwitchName = switchName;

  /// <summary>
  /// Gets the name of the feature switch.
  /// </summary>
  public string SwitchName { get; }

}

#endif
