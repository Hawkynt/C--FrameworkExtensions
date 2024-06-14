#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Form.Extensions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  public static partial class SyntaxHighlightPattern {
    public static ISyntaxHighlightPattern
      FromKeywords(IEnumerable<string> keywords, ISyntaxStyle style, bool ignoreCase = false) => new Pattern(
      new(
        $@"(?<=^|\W)({keywords.Select(Regex.Escape)._FOS_Join("|")})(?=\W|$)",
        RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)
      ),
      style
    );

    public static ISyntaxHighlightPattern FromKeyword(string word, ISyntaxStyle style, bool ignoreCase = false) =>
      new Pattern(
        new(
          $@"(?<=^|\W)({Regex.Escape(word)})(?=\W|$)",
          RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)
        ),
        style
      );

    public static ISyntaxHighlightPattern FromRegex(Regex regex, ISyntaxStyle style) => new Pattern(regex, style);

    public static ISyntaxHighlightPattern FromRegex(Regex regex, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(regex, null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    public static ISyntaxHighlightPattern FromPattern(string pattern, ISyntaxStyle style) =>
      new Pattern(new(pattern, RegexOptions.Compiled), style);

    public static ISyntaxHighlightPattern FromPattern(string pattern, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(new(pattern, RegexOptions.Compiled), null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    public static ISyntaxHighlightPattern FromPattern(string pattern, RegexOptions options, ISyntaxStyle style) =>
      new Pattern(new(pattern, options | RegexOptions.Compiled), style);

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
