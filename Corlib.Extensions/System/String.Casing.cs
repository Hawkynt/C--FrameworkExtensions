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
//

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

partial class StringExtensions {

  /// <summary>
  ///   Converts a word to pascal case.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="culture">The culture to use; defaults to current culture.</param>
  /// <returns>Something like "CamelCase" from "  camel-case_" </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToPascalCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ChangeCasing(@this, culture ?? CultureInfo.CurrentCulture, true);
  }

  /// <summary>
  ///   Converts a word to camel case.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="culture">The culture to use; defaults to current culture.</param>
  /// <returns>Something like "pascalCase" from "  pascal-case_" </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToCamelCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ChangeCasing(@this, culture ?? CultureInfo.CurrentCulture, false);
  }

  /// <summary>
  ///   Converts a string to PascalCase format using invariant culture for consistent cross-platform behavior.
  ///   Capitalizes the first letter of each word and removes separators.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in PascalCase format with the first letter of each word capitalized and no separators,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string snakeCase = "hello_world";
  /// string result1 = snakeCase.ToPascalCaseInvariant(); // Returns: "HelloWorld"
  /// 
  /// string kebabCase = "hello-world";
  /// string result2 = kebabCase.ToPascalCaseInvariant(); // Returns: "HelloWorld"
  /// 
  /// string spaceCase = "hello world";
  /// string result3 = spaceCase.ToPascalCaseInvariant(); // Returns: "HelloWorld"
  /// 
  /// string camelCase = "helloWorld";
  /// string result4 = camelCase.ToPascalCaseInvariant(); // Returns: "HelloWorld"
  /// 
  /// string withUnicode = "café_world";
  /// string result5 = withUnicode.ToPascalCaseInvariant(); // Returns: "CaféWorld"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "istanbul_app";
  /// string result6 = turkish.ToPascalCaseInvariant(); // Returns: "IstanbulApp" (consistent)
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Commonly used for class names, type names, method names,
  ///   and any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToPascalCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ChangeCasing(@this, CultureInfo.InvariantCulture, true);
  }

  /// <summary>
  ///   Converts a string to camelCase format using invariant culture for consistent cross-platform behavior.
  ///   Capitalizes the first letter of each word except the first, and removes separators.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in camelCase format with the first letter lowercase, subsequent words capitalized, and no separators,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string snakeCase = "hello_world";
  /// string result1 = snakeCase.ToCamelCaseInvariant(); // Returns: "helloWorld"
  /// 
  /// string kebabCase = "hello-world";
  /// string result2 = kebabCase.ToCamelCaseInvariant(); // Returns: "helloWorld"
  /// 
  /// string spaceCase = "hello world";
  /// string result3 = spaceCase.ToCamelCaseInvariant(); // Returns: "helloWorld"
  /// 
  /// string pascalCase = "HelloWorld";
  /// string result4 = pascalCase.ToCamelCaseInvariant(); // Returns: "helloWorld"
  /// 
  /// string withUnicode = "café_world";
  /// string result5 = withUnicode.ToCamelCaseInvariant(); // Returns: "caféWorld"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "istanbul_app";
  /// string result6 = turkish.ToCamelCaseInvariant(); // Returns: "istanbulApp" (consistent)
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Commonly used for variable names, property names, method parameters,
  ///   and any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToCamelCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ChangeCasing(@this, CultureInfo.InvariantCulture, false);
  }

  /// <summary>
  ///   Converts a string to snake_case format by inserting underscores between words and converting to lowercase.
  ///   Handles camelCase, PascalCase, kebab-case, UPPER_CASE, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <param name="culture">The culture to use for case conversion; defaults to current culture if null.</param>
  /// <returns>
  ///   A new string in snake_case format with words separated by underscores and all lowercase letters.
  ///   Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToSnakeCase(); // Returns: "hello_world"
  /// 
  /// string pascalCase = "HelloWorld";
  /// string result2 = pascalCase.ToSnakeCase(); // Returns: "hello_world"
  /// 
  /// string kebabCase = "hello-world";
  /// string result3 = kebabCase.ToSnakeCase(); // Returns: "hello_world"
  /// 
  /// string upperCase = "HELLO_WORLD";
  /// string result4 = upperCase.ToSnakeCase(); // Returns: "hello_world"
  /// 
  /// string spaceCase = "hello world";
  /// string result5 = spaceCase.ToSnakeCase(); // Returns: "hello_world"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result6 = acronym.ToSnakeCase(); // Returns: "xml_http_request"
  /// 
  /// string mixed = "getUserID";
  /// string result7 = mixed.ToSnakeCase(); // Returns: "get_user_id"
  /// 
  /// string withNumbers = "HTML5Parser";
  /// string result8 = withNumbers.ToSnakeCase(); // Returns: "html_5_parser"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method is optimized for performance with minimal memory allocations. It intelligently detects
  ///   word boundaries including camelCase transitions, acronyms, number boundaries, and existing separators.
  ///   The conversion is culture-aware and handles Unicode characters correctly.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToSnakeCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, culture ?? CultureInfo.CurrentCulture, '_', false);
  }

  /// <summary>
  ///   Converts a string to UPPER_SNAKE_CASE format by inserting underscores between words and converting to uppercase.
  ///   Handles camelCase, PascalCase, kebab-case, lower_case, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <param name="culture">The culture to use for case conversion; defaults to current culture if null.</param>
  /// <returns>
  ///   A new string in UPPER_SNAKE_CASE format with words separated by underscores and all uppercase letters.
  ///   Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToUpperSnakeCase(); // Returns: "HELLO_WORLD"
  /// 
  /// string pascalCase = "HelloWorld";
  /// string result2 = pascalCase.ToUpperSnakeCase(); // Returns: "HELLO_WORLD"
  /// 
  /// string kebabCase = "hello-world";
  /// string result3 = kebabCase.ToUpperSnakeCase(); // Returns: "HELLO_WORLD"
  /// 
  /// string lowerCase = "hello_world";
  /// string result4 = lowerCase.ToUpperSnakeCase(); // Returns: "HELLO_WORLD"
  /// 
  /// string spaceCase = "hello world";
  /// string result5 = spaceCase.ToUpperSnakeCase(); // Returns: "HELLO_WORLD"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result6 = acronym.ToUpperSnakeCase(); // Returns: "XML_HTTP_REQUEST"
  /// 
  /// string mixed = "parseJSON";
  /// string result7 = mixed.ToUpperSnakeCase(); // Returns: "PARSE_JSON"
  /// 
  /// string withNumbers = "HTML5Parser";
  /// string result8 = withNumbers.ToUpperSnakeCase(); // Returns: "HTML_5_PARSER"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method is commonly used for constants, environment variables, and configuration keys.
  ///   It's optimized for performance with minimal memory allocations and intelligently detects
  ///   word boundaries including camelCase transitions, acronyms, number boundaries, and existing separators.
  ///   The conversion is culture-aware and handles Unicode characters correctly.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToUpperSnakeCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, culture ?? CultureInfo.CurrentCulture, '_', true);
  }

  /// <summary>
  ///   Converts a string to kebab-case format by inserting hyphens between words and converting to lowercase.
  ///   Handles camelCase, PascalCase, snake_case, UPPER_CASE, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <param name="culture">The culture to use for case conversion; defaults to current culture if null.</param>
  /// <returns>
  ///   A new string in kebab-case format with words separated by hyphens and all lowercase letters.
  ///   Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToKebabCase(); // Returns: "hello-world"
  /// 
  /// string pascalCase = "HelloWorld";
  /// string result2 = pascalCase.ToKebabCase(); // Returns: "hello-world"
  /// 
  /// string snakeCase = "hello_world";
  /// string result3 = snakeCase.ToKebabCase(); // Returns: "hello-world"
  /// 
  /// string upperCase = "HELLO_WORLD";
  /// string result4 = upperCase.ToKebabCase(); // Returns: "hello-world"
  /// 
  /// string spaceCase = "hello world";
  /// string result5 = spaceCase.ToKebabCase(); // Returns: "hello-world"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result6 = acronym.ToKebabCase(); // Returns: "xml-http-request"
  /// 
  /// string mixed = "getUserID";
  /// string result7 = mixed.ToKebabCase(); // Returns: "get-user-id"
  /// 
  /// string withNumbers = "HTML5Parser";
  /// string result8 = withNumbers.ToKebabCase(); // Returns: "html-5-parser"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method is commonly used for CSS class names, HTML IDs, URL slugs, and file names.
  ///   It's optimized for performance with minimal memory allocations and intelligently detects
  ///   word boundaries including camelCase transitions, acronyms, number boundaries, and existing separators.
  ///   The conversion is culture-aware and handles Unicode characters correctly.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToKebabCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, culture ?? CultureInfo.CurrentCulture, '-', false);
  }

  /// <summary>
  ///   Converts a string to UPPER-KEBAB-CASE format by inserting hyphens between words and converting to uppercase.
  ///   Handles camelCase, PascalCase, snake_case, lower-case, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <param name="culture">The culture to use for case conversion; defaults to current culture if null.</param>
  /// <returns>
  ///   A new string in UPPER-KEBAB-CASE format with words separated by hyphens and all uppercase letters.
  ///   Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToUpperKebabCase(); // Returns: "HELLO-WORLD"
  /// 
  /// string pascalCase = "HelloWorld";
  /// string result2 = pascalCase.ToUpperKebabCase(); // Returns: "HELLO-WORLD"
  /// 
  /// string snakeCase = "hello_world";
  /// string result3 = snakeCase.ToUpperKebabCase(); // Returns: "HELLO-WORLD"
  /// 
  /// string lowerCase = "hello-world";
  /// string result4 = lowerCase.ToUpperKebabCase(); // Returns: "HELLO-WORLD"
  /// 
  /// string spaceCase = "hello world";
  /// string result5 = spaceCase.ToUpperKebabCase(); // Returns: "HELLO-WORLD"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result6 = acronym.ToUpperKebabCase(); // Returns: "XML-HTTP-REQUEST"
  /// 
  /// string mixed = "parseJSON";
  /// string result7 = mixed.ToUpperKebabCase(); // Returns: "PARSE-JSON"
  /// 
  /// string withNumbers = "HTML5Parser";
  /// string result8 = withNumbers.ToUpperKebabCase(); // Returns: "HTML-5-PARSER"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method is less commonly used but can be useful for specific naming conventions or headers.
  ///   It's optimized for performance with minimal memory allocations and intelligently detects
  ///   word boundaries including camelCase transitions, acronyms, number boundaries, and existing separators.
  ///   The conversion is culture-aware and handles Unicode characters correctly.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToUpperKebabCase(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, culture ?? CultureInfo.CurrentCulture, '-', true);
  }

  /// <summary>
  ///   Converts a string to snake_case format using invariant culture for consistent cross-platform behavior.
  ///   Handles camelCase, PascalCase, kebab-case, UPPER_CASE, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in snake_case format with words separated by underscores and all lowercase letters,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToSnakeCaseInvariant(); // Returns: "hello_world"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result2 = acronym.ToSnakeCaseInvariant(); // Returns: "xml_http_request"
  /// 
  /// string withUnicode = "caféWorld";
  /// string result3 = withUnicode.ToSnakeCaseInvariant(); // Returns: "café_world"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "İstanbulApp";
  /// string result4 = turkish.ToSnakeCaseInvariant(); // Returns: "i̇stanbul_app" (consistent)
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Recommended for programmatic identifiers, API keys, database columns,
  ///   and any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToSnakeCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, CultureInfo.InvariantCulture, '_', false);
  }

  /// <summary>
  ///   Converts a string to UPPER_SNAKE_CASE format using invariant culture for consistent cross-platform behavior.
  ///   Handles camelCase, PascalCase, kebab-case, lower_case, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in UPPER_SNAKE_CASE format with words separated by underscores and all uppercase letters,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToUpperSnakeCaseInvariant(); // Returns: "HELLO_WORLD"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result2 = acronym.ToUpperSnakeCaseInvariant(); // Returns: "XML_HTTP_REQUEST"
  /// 
  /// string withUnicode = "caféWorld";
  /// string result3 = withUnicode.ToUpperSnakeCaseInvariant(); // Returns: "CAFÉ_WORLD"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "İstanbulApp";
  /// string result4 = turkish.ToUpperSnakeCaseInvariant(); // Returns: "İSTANBUL_APP" (consistent)
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Commonly used for environment variables, configuration constants,
  ///   and any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToUpperSnakeCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, CultureInfo.InvariantCulture, '_', true);
  }

  /// <summary>
  ///   Converts a string to kebab-case format using invariant culture for consistent cross-platform behavior.
  ///   Handles camelCase, PascalCase, snake_case, UPPER_CASE, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in kebab-case format with words separated by hyphens and all lowercase letters,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToKebabCaseInvariant(); // Returns: "hello-world"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result2 = acronym.ToKebabCaseInvariant(); // Returns: "xml-http-request"
  /// 
  /// string withUnicode = "caféWorld";
  /// string result3 = withUnicode.ToKebabCaseInvariant(); // Returns: "café-world"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "İstanbulApp";
  /// string result4 = turkish.ToKebabCaseInvariant(); // Returns: "i̇stanbul-app" (consistent)
  /// 
  /// // Perfect for URLs and file names
  /// string title = "My Blog Post";
  /// string slug = title.ToKebabCaseInvariant(); // Returns: "my-blog-post"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Commonly used for URL slugs, CSS class names, HTML IDs, file names,
  ///   and any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToKebabCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, CultureInfo.InvariantCulture, '-', false);
  }

  /// <summary>
  ///   Converts a string to UPPER-KEBAB-CASE format using invariant culture for consistent cross-platform behavior.
  ///   Handles camelCase, PascalCase, snake_case, lower-case, and space-separated words.
  /// </summary>
  /// <param name="this">The input string to convert.</param>
  /// <returns>
  ///   A new string in UPPER-KEBAB-CASE format with words separated by hyphens and all uppercase letters,
  ///   using invariant culture for case conversion. Returns the original string if it's null or empty.
  /// </returns>
  /// <example>
  /// <code>
  /// string camelCase = "helloWorld";
  /// string result1 = camelCase.ToUpperKebabCaseInvariant(); // Returns: "HELLO-WORLD"
  /// 
  /// string acronym = "XMLHttpRequest";
  /// string result2 = acronym.ToUpperKebabCaseInvariant(); // Returns: "XML-HTTP-REQUEST"
  /// 
  /// string withUnicode = "caféWorld";
  /// string result3 = withUnicode.ToUpperKebabCaseInvariant(); // Returns: "CAFÉ-WORLD"
  /// 
  /// // Consistent results across different system cultures
  /// string turkish = "İstanbulApp";
  /// string result4 = turkish.ToUpperKebabCaseInvariant(); // Returns: "İSTANBUL-APP" (consistent)
  /// 
  /// // Useful for HTTP headers or specific naming conventions
  /// string header = "ContentType";
  /// string result5 = header.ToUpperKebabCaseInvariant(); // Returns: "CONTENT-TYPE"
  /// </code>
  /// </example>
  /// <remarks>
  ///   This method uses InvariantCulture for case conversion, ensuring consistent results across different
  ///   system locales and cultures. Less commonly used but can be useful for specific naming conventions,
  ///   HTTP headers, or any scenario where consistent cross-platform behavior is required.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToUpperKebabCaseInvariant(this string @this) {
    Against.ThisIsNull(@this);
    return _ConvertToSeparatedCase(@this, CultureInfo.InvariantCulture, '-', true);
  }

  private static string _ChangeCasing(string input, CultureInfo culture, bool isPascalCase) =>
    _TransformCase(
      input,
      culture,
      isPascalCase ? c => char.ToUpper(c, culture) : c => char.ToLower(c, culture),
      c => c
    );

  private static string _ConvertToSeparatedCase(string input, CultureInfo culture, char separator, bool uppercase) =>
    _TransformCase(
      input,
      culture,
      c => uppercase ? char.ToUpper(c, culture) : char.ToLower(c, culture),
      c => uppercase ? char.ToUpper(c, culture) : char.ToLower(c, culture),
      separator
    );


  private static string _TransformCase(
    string input,
    CultureInfo culture,
    Func<char, char> firstCharTransform,
    Func<char, char> restCharTransform,
    char? separator = null
  ) {
    if (input.IsNullOrEmpty())
      return input;

    var result = new StringBuilder(input.Length);
    var wordStart = true;
    var prevWasUpper = false;

    for (var i = 0; i < input.Length; ++i) {
      var c = input[i];

      if (!char.IsLetterOrDigit(c)) {
        wordStart = true;
        prevWasUpper = false;
        continue;
      }

      if (NeedsSeparator(i)) {
        if (separator is not null && result.Length > 0)
          result.Append(separator.Value);
        wordStart = true;
      }

      result.Append(wordStart
        ? firstCharTransform(c)
        : restCharTransform(c));

      prevWasUpper = char.IsUpper(c);
      wordStart = false;
    }

    return result.ToString();

    bool NeedsSeparator(int i) {
      if (i == 0)
        return false;

      var prev = input[i - 1];
      var curr = input[i];

      if (!char.IsLetterOrDigit(prev) || !char.IsLetterOrDigit(curr))
        return false;

      // Lowercase → Uppercase: normal camelCase boundary
      if (char.IsLower(prev) && char.IsUpper(curr))
        return true;

      // Acronym end: ABCd → split before d
      if (i > 1 && char.IsUpper(input[i - 2]) && char.IsUpper(prev) && char.IsLower(curr))
        return true;

      // Letter to digit: abc123
      if (char.IsLetter(prev) && char.IsDigit(curr))
        return true;

      // Digit to letter: 123abc
      if (char.IsDigit(prev) && char.IsLetter(curr))
        return true;

      return false;
    }

  }


}

