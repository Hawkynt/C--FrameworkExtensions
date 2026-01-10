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

namespace System.Text.Json;

/// <summary>
/// Represents a single property for a JSON object.
/// </summary>
public readonly struct JsonProperty {

  internal JsonProperty(string name, JsonElement value) {
    this.Name = name;
    this.Value = value;
  }

  /// <summary>
  /// Gets the name of the property.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets the value of the property.
  /// </summary>
  public JsonElement Value { get; }

  /// <summary>
  /// Compares the specified string to the name of this property.
  /// </summary>
  /// <param name="text">The text to compare against.</param>
  /// <returns><see langword="true"/> if the names match; otherwise, <see langword="false"/>.</returns>
  public bool NameEquals(string text) => string.Equals(this.Name, text, StringComparison.Ordinal);

  /// <summary>
  /// Returns the string representation of this property.
  /// </summary>
  /// <returns>The string representation.</returns>
  public override string ToString() => $"\"{this.Name}\": {this.Value.GetRawText()}";

}

#endif
