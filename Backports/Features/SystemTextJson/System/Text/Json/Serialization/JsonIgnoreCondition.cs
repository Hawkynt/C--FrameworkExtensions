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
/// Controls how the <see cref="JsonIgnoreAttribute"/> ignores properties on serialization and deserialization.
/// </summary>
public enum JsonIgnoreCondition {
  /// <summary>
  /// The property is always serialized and deserialized, regardless of <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>.
  /// </summary>
  Never = 0,

  /// <summary>
  /// The property is always ignored.
  /// </summary>
  Always = 1,

  /// <summary>
  /// The property is ignored only if it equals the default value for its type.
  /// </summary>
  WhenWritingDefault = 2,

  /// <summary>
  /// The property is ignored if its value is <see langword="null"/>. This is applied only to reference type and <see cref="Nullable{T}"/> properties.
  /// </summary>
  WhenWritingNull = 3,
}

#endif
