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

// System.Text.Json polyfill for older frameworks that don't have the official package
#if !SUPPORTS_SYSTEM_TEXT_JSON && !OFFICIAL_SYSTEM_TEXT_JSON

namespace System.Text.Json.Serialization;

/// <summary>
/// Specifies the property name that is present in the JSON when serializing and deserializing.
/// This overrides any naming policy specified by <see cref="JsonNamingPolicy"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonPropertyNameAttribute : JsonAttribute {

  /// <summary>
  /// Initializes a new instance of <see cref="JsonPropertyNameAttribute"/> with the specified property name.
  /// </summary>
  /// <param name="name">The name of the property.</param>
  public JsonPropertyNameAttribute(string name) => this.Name = name;

  /// <summary>
  /// Gets the name of the property.
  /// </summary>
  public string Name { get; }

}

#endif
