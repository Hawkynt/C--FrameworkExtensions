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
/// Specifies the data type of a JSON value.
/// </summary>
public enum JsonValueKind : byte {
  /// <summary>
  /// There is no value (as distinct from <see cref="Null"/>).
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// A JSON object.
  /// </summary>
  Object = 1,

  /// <summary>
  /// A JSON array.
  /// </summary>
  Array = 2,

  /// <summary>
  /// A JSON string.
  /// </summary>
  String = 3,

  /// <summary>
  /// A JSON number.
  /// </summary>
  Number = 4,

  /// <summary>
  /// The JSON value true.
  /// </summary>
  True = 5,

  /// <summary>
  /// The JSON value false.
  /// </summary>
  False = 6,

  /// <summary>
  /// The JSON value null.
  /// </summary>
  Null = 7,
}

#endif
