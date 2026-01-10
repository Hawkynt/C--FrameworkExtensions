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
/// When placed on a property or type, specifies the converter type to use.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false)]
public class JsonConverterAttribute : JsonAttribute {

  /// <summary>
  /// Initializes a new instance of <see cref="JsonConverterAttribute"/> with the specified converter type.
  /// </summary>
  /// <param name="converterType">The type of the converter.</param>
  public JsonConverterAttribute(Type converterType) => this.ConverterType = converterType;

  /// <summary>
  /// Initializes a new instance of <see cref="JsonConverterAttribute"/>.
  /// </summary>
  protected JsonConverterAttribute() { }

  /// <summary>
  /// Gets the type of the converter to create.
  /// </summary>
  public Type? ConverterType { get; }

  /// <summary>
  /// If overridden and <see cref="ConverterType"/> is <see langword="null"/>, creates the converter for the specific type.
  /// </summary>
  /// <param name="typeToConvert">The type being converted.</param>
  /// <returns>The converter to use for serialization.</returns>
  public virtual JsonConverter? CreateConverter(Type typeToConvert) => null;

}

#endif
