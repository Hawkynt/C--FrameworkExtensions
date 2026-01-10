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

// JsonNamingPolicy.SnakeCaseLower/KebabCaseLower were added in .NET 8.0
// System.Text.Json is available in .NET Core 3.0+ (native) or via NuGet package for net462+, netstandard2.0+
#if !SUPPORTS_JSON_NAMING_POLICY_CASES && (SUPPORTS_SYSTEM_TEXT_JSON || OFFICIAL_SYSTEM_TEXT_JSON)

using System.Text;

namespace System.Text.Json;

/// <summary>
/// Provides additional naming policies for JSON serialization.
/// </summary>
public static class JsonNamingPolicyExtensions {

  /// <summary>
  /// Gets a naming policy that converts names to snake_case (lowercase with underscores).
  /// </summary>
  public static JsonNamingPolicy SnakeCaseLower { get; } = new SnakeCaseLowerNamingPolicy();

  /// <summary>
  /// Gets a naming policy that converts names to SNAKE_CASE_UPPER (uppercase with underscores).
  /// </summary>
  public static JsonNamingPolicy SnakeCaseUpper { get; } = new SnakeCaseUpperNamingPolicy();

  /// <summary>
  /// Gets a naming policy that converts names to kebab-case (lowercase with hyphens).
  /// </summary>
  public static JsonNamingPolicy KebabCaseLower { get; } = new KebabCaseLowerNamingPolicy();

  /// <summary>
  /// Gets a naming policy that converts names to KEBAB-CASE-UPPER (uppercase with hyphens).
  /// </summary>
  public static JsonNamingPolicy KebabCaseUpper { get; } = new KebabCaseUpperNamingPolicy();

}

internal sealed class SnakeCaseLowerNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => JsonNamingPolicyHelper.ConvertToSeparatedCase(name, '_', false);
}

internal sealed class SnakeCaseUpperNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => JsonNamingPolicyHelper.ConvertToSeparatedCase(name, '_', true);
}

internal sealed class KebabCaseLowerNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => JsonNamingPolicyHelper.ConvertToSeparatedCase(name, '-', false);
}

internal sealed class KebabCaseUpperNamingPolicy : JsonNamingPolicy {
  public override string ConvertName(string name) => JsonNamingPolicyHelper.ConvertToSeparatedCase(name, '-', true);
}

internal static class JsonNamingPolicyHelper {

  public static string ConvertToSeparatedCase(string name, char separator, bool uppercase) {
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

#endif
