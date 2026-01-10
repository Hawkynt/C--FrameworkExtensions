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
/// Converts an object or value to or from JSON.
/// </summary>
public abstract class JsonConverter {

  /// <summary>
  /// Gets the type being converted by the current converter instance.
  /// </summary>
  public abstract Type? Type { get; }

  /// <summary>
  /// Determines whether the specified type can be converted.
  /// </summary>
  /// <param name="typeToConvert">The type to compare against.</param>
  /// <returns><see langword="true"/> if the type can be converted; otherwise, <see langword="false"/>.</returns>
  public abstract bool CanConvert(Type typeToConvert);

}

/// <summary>
/// Converts an object or value to or from JSON.
/// </summary>
/// <typeparam name="T">The type of object or value handled by the converter.</typeparam>
public abstract class JsonConverter<T> : JsonConverter {

  /// <summary>
  /// Initializes a new <see cref="JsonConverter{T}"/> instance.
  /// </summary>
  protected JsonConverter() { }

  /// <summary>
  /// Gets the type being converted by the current converter instance.
  /// </summary>
  public sealed override Type Type => typeof(T);

  /// <summary>
  /// Determines whether the specified type can be converted.
  /// </summary>
  /// <param name="typeToConvert">The type to compare against.</param>
  /// <returns><see langword="true"/> if the type can be converted; otherwise, <see langword="false"/>.</returns>
  public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);

  /// <summary>
  /// Reads and converts the JSON to type <typeparamref name="T"/>.
  /// </summary>
  /// <param name="reader">The reader.</param>
  /// <param name="typeToConvert">The type to convert.</param>
  /// <param name="options">An object that specifies serialization options to use.</param>
  /// <returns>The converted value.</returns>
  public abstract T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

  /// <summary>
  /// Writes a specified value as JSON.
  /// </summary>
  /// <param name="writer">The writer to write to.</param>
  /// <param name="value">The value to convert to JSON.</param>
  /// <param name="options">An object that specifies serialization options to use.</param>
  public abstract void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);

  /// <summary>
  /// Gets or sets a value that indicates whether <see langword="null"/> should be passed to the converter on serialization.
  /// </summary>
  public virtual bool HandleNull => false;

}

#endif
