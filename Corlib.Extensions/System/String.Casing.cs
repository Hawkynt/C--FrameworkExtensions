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

using Guard;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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
    return _ConvertCase(@this, CaseStyle.PascalCase, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.CamelCase, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.PascalCase, CultureInfo.InvariantCulture);
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
    return _ConvertCase(@this, CaseStyle.CamelCase, CultureInfo.InvariantCulture);
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
    return _ConvertCase(@this, CaseStyle.SnakeCaseLower, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.SnakeCaseUpper, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.KebabCaseLower, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.KebabCaseUpper, culture ?? CultureInfo.CurrentCulture);
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
    return _ConvertCase(@this, CaseStyle.SnakeCaseLower, CultureInfo.InvariantCulture);
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
    return _ConvertCase(@this, CaseStyle.SnakeCaseUpper, CultureInfo.InvariantCulture);
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
    return _ConvertCase(@this, CaseStyle.KebabCaseLower, CultureInfo.InvariantCulture);
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
    return _ConvertCase(@this, CaseStyle.KebabCaseUpper, CultureInfo.InvariantCulture);
  }

  private enum CaseStyle {
    CamelCase,
    PascalCase,
    SnakeCaseLower,
    SnakeCaseUpper,
    KebabCaseLower,
    KebabCaseUpper
  }

  // TODO: The most important thing this method has to get right is detecting words using a DFA
  // test     -> test              (consecutive run of lowercase)
  // test123  -> test123           (numbers at word end stay there)
  // testAb   -> test Ab           (split at uppercase hump)
  // test-ab  -> test ab           (split at kebap delimiter)
  // test_ab  -> test ab           (split at snake delimiter)
  // TEST_ab  -> TEST ab           (consecutive run of uppercase)
  // TESTAb   -> TEST Ab           (if uc runs terminate with lc, keep the last uc to the lc part)
  // TestAb   -> Test Ab           (split at uppercase hump)
  // T$$$     -> T                 (ignore special characters at end)
  // Test%$T  -> Test T            (ignore special characters, but they break words)
  // $$Test   -> Test              (ignore special characters at start)
  // Test-5   -> Test5 / Test 5    (breaker doesnt break trailing numbers, but only when separator is used in output)
  // Test-5-5 -> Test55 / Test 5 5 (even multiple breakers dont break trailing numbers, but only when separator is used in output)
  // 5Test    -> 5 Test            (numbers break words)
  // t        -> t                 (single character words are ok)
  // 123-123  -> 123123 / 123 123  (numbers cant be broken by delimiters, but only when separator is used in output)
  // 123--123 -> 123123 / 123 123  (numbers cant be broken by multiple delimiters, but only when separator is used in output)
  // 123$123  -> 123123 / 123 123  (numbers cant be broken by special characters, but only when separator is used in output)
  // Tést     -> Tést              (even uc/lc rules of other languages count)
  // T_T_T    -> T T T             (single character words)
  // T--T_T   -> T T T             (consecutive breakers dont produce empty words)
  // T$-T_T   -> T T T             (special characters can break when already broken)
  // T-$-$-T_ -> T T               (special characters can break when already broken)
  private static string _ConvertCase(string input, CaseStyle style, CultureInfo culture) {
    if (input.IsNullOrEmpty())
      return input;

    culture ??= CultureInfo.InvariantCulture;
    var textInfo = culture.TextInfo;

    const char NO_SEPARATOR = '\0';
    var separator = style switch {
      CaseStyle.SnakeCaseLower or CaseStyle.SnakeCaseUpper => '_',
      CaseStyle.KebabCaseLower or CaseStyle.KebabCaseUpper => '-',
      _ => NO_SEPARATOR
    };

    var isSnakeOrKebab = separator != NO_SEPARATOR;
    var toUpper = style is CaseStyle.SnakeCaseUpper or CaseStyle.KebabCaseUpper;
    var camel = style == CaseStyle.CamelCase;

    const int MODE_START = 0, MODE_UPPER = 1, MODE_LOWER = 2, MODE_DIGIT = 3, MODE_OTHER = 4;
    int prevType = MODE_START, wordIndex = 0, length = input.Length;
    StringBuilder? sb = null;

    for (var i = 0; i < length; ++i) {
      var c = input[i];
      var currentType = char.IsUpper(c) ? MODE_UPPER
                       : char.IsLower(c) ? MODE_LOWER
                       : char.IsDigit(c) ? MODE_DIGIT
                       : MODE_OTHER;

      // ── DROP “other” chars, except preserve existing correct separators ──
      if (currentType == MODE_OTHER) {
        if (isSnakeOrKebab && c == separator) {
          // it’s exactly the separator we want, so leave input as-is (no sb)
          prevType = MODE_OTHER;
          continue;
        }

        // truly unwanted char → transformation
        sb ??= new StringBuilder(length).Append(input, 0, i);
        prevType = MODE_OTHER;
        continue;
      }

      // look-ahead for hump logic
      var nextType = MODE_START;
      if (i + 1 < length) {
        var nc = input[i + 1];
        nextType = char.IsUpper(nc) ? MODE_UPPER
                 : char.IsLower(nc) ? MODE_LOWER
                 : char.IsDigit(nc) ? MODE_DIGIT
                 : MODE_OTHER;
      }

      var isNewWord = currentType switch {
        MODE_DIGIT => prevType != MODE_DIGIT,
        MODE_UPPER => prevType == MODE_START
                      || prevType == MODE_LOWER
                      || prevType == MODE_DIGIT
                      || prevType == MODE_OTHER
                      || (prevType == MODE_UPPER && nextType == MODE_LOWER),
        MODE_LOWER => prevType == MODE_START
                      || prevType == MODE_DIGIT
                      || prevType == MODE_OTHER,
        _ => true
      };

      // ── INSERT separator for snake/kebab if—and only if—it wasn’t already there ──
      if (isNewWord && isSnakeOrKebab && wordIndex > 0)
        if (!(i > 0 && input[i - 1] == separator)) {
          sb ??= new StringBuilder(length).Append(input, 0, i);
          sb.Append(separator);
        }

      // ── DETERMINE output character ──
      char outChar;
      if (currentType == MODE_DIGIT)
        outChar = c;
      else if (isSnakeOrKebab)
        outChar = toUpper
          ? textInfo.ToUpper(c)
          : textInfo.ToLower(c);
      else if (camel) {
        if (isNewWord)
          outChar = wordIndex == 0
              ? textInfo.ToLower(c)
              : textInfo.ToUpper(c);
        else
          outChar = textInfo.ToLower(c);
      } else
        outChar = isNewWord
          ? textInfo.ToUpper(c)
          : textInfo.ToLower(c);

      // ── LAZY‐ALLOCATE and append if it diverges ──
      if (sb is null) {
        if (outChar != input[i])
          sb = new StringBuilder(length)
                   .Append(input, 0, i)
                   .Append(outChar);
      } else
        sb.Append(outChar);

      if (isNewWord)
        ++wordIndex;

      prevType = currentType;
    }

    return sb?.ToString() ?? input;
  }
}
