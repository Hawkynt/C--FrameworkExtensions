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
/// Defines the various JSON tokens that make up a JSON text.
/// </summary>
public enum JsonTokenType : byte {
  /// <summary>
  /// There is no value (as distinct from <see cref="Null"/>).
  /// </summary>
  None = 0,

  /// <summary>
  /// The token type is the start of a JSON object.
  /// </summary>
  StartObject = 1,

  /// <summary>
  /// The token type is the end of a JSON object.
  /// </summary>
  EndObject = 2,

  /// <summary>
  /// The token type is the start of a JSON array.
  /// </summary>
  StartArray = 3,

  /// <summary>
  /// The token type is the end of a JSON array.
  /// </summary>
  EndArray = 4,

  /// <summary>
  /// The token type is a JSON property name.
  /// </summary>
  PropertyName = 5,

  /// <summary>
  /// The token type is a comment string.
  /// </summary>
  Comment = 6,

  /// <summary>
  /// The token type is a JSON string.
  /// </summary>
  String = 7,

  /// <summary>
  /// The token type is a JSON number.
  /// </summary>
  Number = 8,

  /// <summary>
  /// The token type is the JSON literal true.
  /// </summary>
  True = 9,

  /// <summary>
  /// The token type is the JSON literal false.
  /// </summary>
  False = 10,

  /// <summary>
  /// The token type is the JSON literal null.
  /// </summary>
  Null = 11,
}

#endif
