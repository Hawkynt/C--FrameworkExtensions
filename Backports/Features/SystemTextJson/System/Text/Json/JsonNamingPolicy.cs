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

using System.Text;

namespace System.Text.Json;

/// <summary>
/// Determines the naming policy used to convert a string-based name to another format, such as a camel-casing format.
/// </summary>
public abstract class JsonNamingPolicy {

  /// <summary>
  /// Initializes a new instance of <see cref="JsonNamingPolicy"/>.
  /// </summary>
  protected JsonNamingPolicy() { }

  /// <summary>
  /// Gets the naming policy for camel-casing.
  /// </summary>
  public static JsonNamingPolicy CamelCase { get; } = new CamelCaseJsonNamingPolicy();

  /// <summary>
  /// Gets the naming policy for lowercase snake_case.
  /// </summary>
  public static JsonNamingPolicy SnakeCaseLower { get; } = new SnakeCaseLowerJsonNamingPolicy();

  /// <summary>
  /// Gets the naming policy for uppercase SNAKE_CASE.
  /// </summary>
  public static JsonNamingPolicy SnakeCaseUpper { get; } = new SnakeCaseUpperJsonNamingPolicy();

  /// <summary>
  /// Gets the naming policy for lowercase kebab-case.
  /// </summary>
  public static JsonNamingPolicy KebabCaseLower { get; } = new KebabCaseLowerJsonNamingPolicy();

  /// <summary>
  /// Gets the naming policy for uppercase KEBAB-CASE.
  /// </summary>
  public static JsonNamingPolicy KebabCaseUpper { get; } = new KebabCaseUpperJsonNamingPolicy();

  /// <summary>
  /// When overridden in a derived class, converts the specified name according to the policy.
  /// </summary>
  /// <param name="name">The name to convert.</param>
  /// <returns>The converted name.</returns>
  public abstract string ConvertName(string name);

  internal static string ConvertToSeparatedCase(string name, char separator, bool uppercase) {
    if (string.IsNullOrEmpty(name))
      return name;

    var builder = new StringBuilder();
    var previousWasUpper = false;
    var previousWasDigit = false;
    var previousWasSeparator = true;

    for (var i = 0; i < name.Length; ++i) {
      var current = name[i];
      var isUpper = char.IsUpper(current);
      var isDigit = char.IsDigit(current);
      var isLower = char.IsLower(current);

      if (isUpper) {
        if (!previousWasSeparator && !previousWasUpper)
          builder.Append(separator);
        else if (previousWasUpper && i + 1 < name.Length && char.IsLower(name[i + 1]))
          builder.Append(separator);

        builder.Append(uppercase ? current : char.ToLowerInvariant(current));
      } else if (isDigit) {
        if (!previousWasSeparator && !previousWasDigit)
          builder.Append(separator);

        builder.Append(current);
      } else if (isLower) {
        builder.Append(uppercase ? char.ToUpperInvariant(current) : current);
      } else {
        builder.Append(current);
      }

      previousWasUpper = isUpper;
      previousWasDigit = isDigit;
      previousWasSeparator = current == separator || current == '_' || current == '-';
    }

    return builder.ToString();
  }

}

internal sealed class CamelCaseJsonNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) {
    if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
      return name;

    var chars = name.ToCharArray();
    for (var i = 0; i < chars.Length; ++i) {
      if (i == 1 && !char.IsUpper(chars[i]))
        break;

      var hasNext = i + 1 < chars.Length;
      if (i > 0 && hasNext && !char.IsUpper(chars[i + 1])) {
        if (char.IsSeparator(chars[i + 1])) {
          chars[i] = char.ToLowerInvariant(chars[i]);
        }
        break;
      }
      chars[i] = char.ToLowerInvariant(chars[i]);
    }
    return new string(chars);
  }
}

internal sealed class SnakeCaseLowerJsonNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => ConvertToSeparatedCase(name, '_', false);
}

internal sealed class SnakeCaseUpperJsonNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => ConvertToSeparatedCase(name, '_', true);
}

internal sealed class KebabCaseLowerJsonNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => ConvertToSeparatedCase(name, '-', false);
}

internal sealed class KebabCaseUpperJsonNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => ConvertToSeparatedCase(name, '-', true);
}

#endif
