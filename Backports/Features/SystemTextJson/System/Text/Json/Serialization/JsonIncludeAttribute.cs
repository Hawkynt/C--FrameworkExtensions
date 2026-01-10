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
/// Indicates that the property or field should be included for serialization and deserialization.
/// </summary>
/// <remarks>
/// When applied to a public property, indicates that non-public getters and setters can be used for serialization and deserialization.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonIncludeAttribute : JsonAttribute {

  /// <summary>
  /// Initializes a new instance of <see cref="JsonIncludeAttribute"/>.
  /// </summary>
  public JsonIncludeAttribute() { }

}

#endif
