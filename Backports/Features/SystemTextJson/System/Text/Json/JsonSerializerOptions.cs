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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// Provides options to be used with <see cref="JsonSerializer"/>.
/// </summary>
public sealed class JsonSerializerOptions {

  private static JsonSerializerOptions? _default;

  /// <summary>
  /// Gets a read-only, singleton instance of <see cref="JsonSerializerOptions"/> with default configuration.
  /// </summary>
  public static JsonSerializerOptions Default => _default ??= new() { _isReadOnly = true };

  private bool _isReadOnly;
  private bool _propertyNameCaseInsensitive;
  private JsonNamingPolicy? _propertyNamingPolicy;
  private bool _writeIndented;
  private int _maxDepth = 64;
  private JsonIgnoreCondition _defaultIgnoreCondition;
  private bool _includeFields;
  private bool _allowTrailingCommas;
  private JsonCommentHandling _readCommentHandling;
  private bool _ignoreReadOnlyProperties;
  private bool _ignoreReadOnlyFields;
  private JsonNamingPolicy? _dictionaryKeyPolicy;

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonSerializerOptions"/> class.
  /// </summary>
  public JsonSerializerOptions() => this.Converters = new List<JsonConverter>();

  /// <summary>
  /// Copies the options from a <see cref="JsonSerializerOptions"/> instance to a new instance.
  /// </summary>
  /// <param name="options">The options to copy.</param>
  public JsonSerializerOptions(JsonSerializerOptions options) {
    ArgumentNullException.ThrowIfNull(options);
    this._propertyNameCaseInsensitive = options._propertyNameCaseInsensitive;
    this._propertyNamingPolicy = options._propertyNamingPolicy;
    this._writeIndented = options._writeIndented;
    this._maxDepth = options._maxDepth;
    this._defaultIgnoreCondition = options._defaultIgnoreCondition;
    this._includeFields = options._includeFields;
    this._allowTrailingCommas = options._allowTrailingCommas;
    this._readCommentHandling = options._readCommentHandling;
    this._ignoreReadOnlyProperties = options._ignoreReadOnlyProperties;
    this._ignoreReadOnlyFields = options._ignoreReadOnlyFields;
    this._dictionaryKeyPolicy = options._dictionaryKeyPolicy;
    this.Converters = new List<JsonConverter>(options.Converters);
  }

  private void _ThrowIfReadOnly() {
    if (this._isReadOnly)
      throw new InvalidOperationException("Options instance is read-only.");
  }

  /// <summary>
  /// Gets or sets a value that indicates whether property names should use a case-insensitive comparison during deserialization.
  /// </summary>
  public bool PropertyNameCaseInsensitive {
    get => this._propertyNameCaseInsensitive;
    set {
      this._ThrowIfReadOnly();
      this._propertyNameCaseInsensitive = value;
    }
  }

  /// <summary>
  /// Gets or sets the policy used to convert a property's name on an object to another format.
  /// </summary>
  public JsonNamingPolicy? PropertyNamingPolicy {
    get => this._propertyNamingPolicy;
    set {
      this._ThrowIfReadOnly();
      this._propertyNamingPolicy = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that defines whether JSON should use pretty printing.
  /// </summary>
  public bool WriteIndented {
    get => this._writeIndented;
    set {
      this._ThrowIfReadOnly();
      this._writeIndented = value;
    }
  }

  /// <summary>
  /// Gets or sets the maximum depth allowed when serializing or deserializing JSON.
  /// </summary>
  public int MaxDepth {
    get => this._maxDepth;
    set {
      this._ThrowIfReadOnly();
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof(value), "MaxDepth cannot be negative.");
      this._maxDepth = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that determines when properties with default values are ignored during serialization or deserialization.
  /// </summary>
  public JsonIgnoreCondition DefaultIgnoreCondition {
    get => this._defaultIgnoreCondition;
    set {
      this._ThrowIfReadOnly();
      this._defaultIgnoreCondition = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that indicates whether fields are handled during serialization and deserialization.
  /// </summary>
  public bool IncludeFields {
    get => this._includeFields;
    set {
      this._ThrowIfReadOnly();
      this._includeFields = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that defines whether an extra comma at the end of a list of JSON values in an object or array is allowed.
  /// </summary>
  public bool AllowTrailingCommas {
    get => this._allowTrailingCommas;
    set {
      this._ThrowIfReadOnly();
      this._allowTrailingCommas = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that defines how comments are handled during deserialization.
  /// </summary>
  public JsonCommentHandling ReadCommentHandling {
    get => this._readCommentHandling;
    set {
      this._ThrowIfReadOnly();
      this._readCommentHandling = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that indicates whether read-only properties are ignored during serialization.
  /// </summary>
  public bool IgnoreReadOnlyProperties {
    get => this._ignoreReadOnlyProperties;
    set {
      this._ThrowIfReadOnly();
      this._ignoreReadOnlyProperties = value;
    }
  }

  /// <summary>
  /// Gets or sets a value that indicates whether read-only fields are ignored during serialization.
  /// </summary>
  public bool IgnoreReadOnlyFields {
    get => this._ignoreReadOnlyFields;
    set {
      this._ThrowIfReadOnly();
      this._ignoreReadOnlyFields = value;
    }
  }

  /// <summary>
  /// Gets or sets the policy used to convert a dictionary key to another format.
  /// </summary>
  public JsonNamingPolicy? DictionaryKeyPolicy {
    get => this._dictionaryKeyPolicy;
    set {
      this._ThrowIfReadOnly();
      this._dictionaryKeyPolicy = value;
    }
  }

  /// <summary>
  /// Gets a list of user-defined converters that were registered.
  /// </summary>
  public IList<JsonConverter> Converters { get; }

}

/// <summary>
/// Defines how the <see cref="Utf8JsonReader"/> struct handles comments.
/// </summary>
public enum JsonCommentHandling : byte {
  /// <summary>
  /// Doesn't allow comments within the JSON input.
  /// </summary>
  Disallow = 0,

  /// <summary>
  /// Allows comments within the JSON input and ignores them.
  /// </summary>
  Skip = 1,

  /// <summary>
  /// Allows comments within the JSON input and treats them as valid tokens.
  /// </summary>
  Allow = 2,
}

#endif
