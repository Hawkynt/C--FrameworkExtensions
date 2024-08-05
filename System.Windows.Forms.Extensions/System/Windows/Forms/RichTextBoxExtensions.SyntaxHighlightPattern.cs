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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Form.Extensions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {

  /// <summary>
  /// Provides factory methods for creating <see cref="ISyntaxHighlightPattern"/> instances.
  /// </summary>
  public static partial class SyntaxHighlightPattern {

    /// <summary>
    /// Creates a syntax highlight pattern from a collection of keywords.
    /// </summary>
    /// <param name="keywords">The collection of keywords to match.</param>
    /// <param name="style">The style to apply to the matched keywords.</param>
    /// <param name="ignoreCase">(Optional: defaults to <see langword="false"/>) If set to <see langword="true"/>, ignores case when matching.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern
      FromKeywords(IEnumerable<string> keywords, ISyntaxStyle style, bool ignoreCase = false) => new Pattern(
      new(
        $@"(?<=^|\W)({keywords.Select(Regex.Escape)._FOS_Join("|")})(?=\W|$)",
        RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)
      ),
      style
    );

    /// <summary>
    /// Creates a syntax highlight pattern from a single keyword.
    /// </summary>
    /// <param name="word">The keyword to match.</param>
    /// <param name="style">The style to apply to the matched keyword.</param>
    /// <param name="ignoreCase">(Optional: defaults to <see langword="false"/>) If set to <see langword="true"/>, ignores case when matching.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromKeyword(string word, ISyntaxStyle style, bool ignoreCase = false) =>
      new Pattern(
        new(
          $@"(?<=^|\W)({Regex.Escape(word)})(?=\W|$)",
          RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)
        ),
        style
      );

    /// <summary>
    /// Creates a syntax highlight pattern from a regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to match.</param>
    /// <param name="style">The style to apply to the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromRegex(Regex regex, ISyntaxStyle style) => new Pattern(regex, style);

    /// <summary>
    /// Creates a syntax highlight pattern from a regular expression with group styles.
    /// </summary>
    /// <param name="regex">The regular expression to match.</param>
    /// <param name="styles">A dictionary of styles to apply to specific groups within the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromRegex(Regex regex, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(regex, null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    /// <summary>
    /// Creates a syntax highlight pattern from a string pattern.
    /// </summary>
    /// <param name="pattern">The string pattern to match.</param>
    /// <param name="style">The style to apply to the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromPattern(string pattern, ISyntaxStyle style) =>
      new Pattern(new(pattern, RegexOptions.Compiled), style);

    /// <summary>
    /// Creates a syntax highlight pattern from a string pattern with group styles.
    /// </summary>
    /// <param name="pattern">The string pattern to match.</param>
    /// <param name="styles">A dictionary of styles to apply to specific groups within the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromPattern(string pattern, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(new(pattern, RegexOptions.Compiled), null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    /// <summary>
    /// Creates a syntax highlight pattern from a string pattern with specified regex options.
    /// </summary>
    /// <param name="pattern">The string pattern to match.</param>
    /// <param name="options">The regex options to apply.</param>
    /// <param name="style">The style to apply to the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromPattern(string pattern, RegexOptions options, ISyntaxStyle style) =>
      new Pattern(new(pattern, options | RegexOptions.Compiled), style);

    /// <summary>
    /// Creates a syntax highlight pattern from a string pattern with specified regex options and group styles.
    /// </summary>
    /// <param name="pattern">The string pattern to match.</param>
    /// <param name="options">The regex options to apply.</param>
    /// <param name="styles">A dictionary of styles to apply to specific groups within the matched text.</param>
    /// <returns>A <see cref="ISyntaxHighlightPattern"/> instance.</returns>
    public static ISyntaxHighlightPattern FromPattern(
      string pattern,
      RegexOptions options,
      IDictionary<string, ISyntaxStyle> styles
    ) => new Pattern(
      new(pattern, options | RegexOptions.Compiled),
      null,
      new ReadOnlyDictionary<string, ISyntaxStyle>(styles)
    );

  }
}
