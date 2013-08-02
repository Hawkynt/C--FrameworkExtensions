#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using word = System.UInt16;
using qword = System.UInt64;
namespace System {
  internal static partial class StringExtensions {
    #region consts
    /// <summary>
    /// This is a list of services which are registered to certain ports according to IANA. 
    /// It allows us to use names for these ports if we want to.
    /// </summary>
    private static readonly Dictionary<string, word> _OFFICIAL_PORT_NAMES = new Dictionary<string, ushort> {
      {"tcpmux", 1},
      {"echo", 7},
      {"discard", 9},
      {"daytime", 13},
      {"quote", 17},
      {"chargen", 19},
      {"ftp", 21},
      {"ssh", 22},
      {"telnet", 23},
      {"smtp", 25},
      {"time", 37},
      {"whois", 43},
      {"dns", 53},
      {"mtp", 57},
      {"tftp", 69},
      {"gopher", 70},
      {"finger", 79},
      {"http", 80},
      {"kerberos", 88},
      {"pop2", 109},
      {"pop3", 110},
      {"ident", 113},
      {"auth", 113},
      {"sftp", 115},
      {"sql", 118},
      {"nntp", 119},
      {"ntp", 123},
      {"imap", 143},
      {"bftp", 152},
      {"sgmp", 153},
      {"snmp", 161},
      {"snmptrap", 162},
      {"irc", 194},
      {"ipx", 213},
      {"mpp", 218},
      {"imap3", 220},
      {"https", 443},
      {"rip", 520},
      {"rpc", 530},
      {"nntps", 563},
    };
    #endregion

    /// <summary>
    /// Repeats the specified string a certain number of times.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="count">The count.</param>
    /// <returns></returns>
    public static string Repeat(this string This, int count) {
      if (This == null || count < 1)
        return (null);
      var n = new StringBuilder(This.Length * count);
      for (int i = 0; i < count; i++) {
        n.Append(This);
      }
      return (n.ToString());
    }

    /// <summary>
    /// Removes the last n chars from a string.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <returns>The new string</returns>
    public static string RemoveLast(this string This, int count) {
      if (string.IsNullOrEmpty(This) || count < 1)
        return (This);

      if (This.Length < count)
        return (string.Empty);

      return (This.Substring(0, This.Length - count));
    }
    /// <summary>
    /// Gets a substring.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="start">The start index of the first char that should be contained in the result; can be negative to indicate a "from the end".</param>
    /// <param name="end">The end index of the first char not contained in the result; can be negative to indicate a "from-the-end".</param>
    /// <returns>the substring</returns>
    public static string SubString(this string This, int start, int end = 0) {
      if (This == null)
        return (null);
      string result;
      var length = This.Length;
      if (length > 0) {
        if (start < 0)
          start += length;
        if (start < 0)
          start = 0;
        if (end <= 0)
          end += length;
        var len = end - start;
        if (len > length)
          len = length - start;

        Contract.Assume(len >= 0);
        Contract.Assume(start + len <= This.Length);
        result = This.Substring(start, len);
      } else {
        result = string.Empty;
      }
      return (result);
    }
    /// <summary>
    /// Gets the first n chars from a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="length">The number of chars to get.</param>
    /// <returns>A string with the first n chars.</returns>
    public static string Left(this string This, int length) {
      Contract.Requires(length >= 0);
      if (This == null)
        return null;
      return This.Substring(0, Math.Min(length, This.Length));
    }

    /// <summary>
    /// Gets the last n chars from a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="length">The number of chars to get.</param>
    /// <returns>A string with the last n chars.</returns>
    public static string Right(this string This, int length) {
      Contract.Requires(length >= 0);
      if (This == null)
        return (null);
      var totalLen = This.Length;
      return (This.Substring(totalLen - Math.Min(totalLen, length)));
    }

    #region needed consts for converting filename patterns into regexes
    private static readonly Regex _ILEGAL_CHARACTERS_REGEX = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
    private static readonly Regex _CATCH_EXTENSION_REGEX = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
    #endregion

    /// <summary>
    /// Converts a given filename pattern into a regular expression.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The regex.</returns>
    private static Regex _ConvertFilePatternToRegex(string pattern) {

      const string NonDotCharacters = @"[^.]*";

      if (pattern == null)
        throw new ArgumentNullException();

      pattern = pattern.Trim();
      if (pattern.Length == 0)
        throw new ArgumentException("Pattern is empty.");

      if (_ILEGAL_CHARACTERS_REGEX.IsMatch(pattern))
        throw new ArgumentException("Patterns contains ilegal characters.");

      var hasExtension = _CATCH_EXTENSION_REGEX.IsMatch(pattern);
      var matchExact = false;

      if (pattern.IndexOf('?') >= 0)
        matchExact = true;
      else if (hasExtension)
        matchExact = _CATCH_EXTENSION_REGEX.Match(pattern).Groups[1].Length != 3;

      var regexString = Regex.Escape(pattern);
      regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
      regexString = Regex.Replace(regexString, @"\\\?", ".");

      if (!matchExact && hasExtension)
        regexString += NonDotCharacters;

      regexString += "$";
      return (new Regex(regexString, RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Determines if the given string matches a given file pattern or not.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="pattern">The pattern to apply.</param>
    /// <returns><c>true</c> if the string matches the file pattern; otherwise, <c>false</c>.</returns>
    public static bool MatchesFilePattern(this string This, string pattern) {
      Contract.Requires(This != null);
      Contract.Requires(pattern != null);
      return (_ConvertFilePatternToRegex(pattern).IsMatch(This));
    }

    /// <summary>
    /// Matches the specified regex.
    /// </summary>
    /// <param name="regex">The regex.</param>
    /// <param name="This">The data.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns>A <see cref="MatchCollection"/> containing the matches.</returns>
    public static MatchCollection Matches(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) {
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
      return (new Regex(regex, regexOptions).Matches(This));
    }

    /// <summary>
    /// Matches the specified regex and returns the groups.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns>
    /// A <see cref="GroupCollection"/> containing the found groups.
    /// </returns>
    public static GroupCollection MatchGroups(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) {
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
      return (new Regex(regex, regexOptions).Match(This).Groups);
    }

    /// <summary>
    /// Uses the string as a format string.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="parameters">The parameters to use for formatting.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatWith(this string This, params object[] parameters) {
      Contract.Requires(This != null);
      Contract.Requires(parameters != null);
      return (string.Format(This, parameters));
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns></returns>
    public static string FormatWithEx(this string This, IEnumerable<KeyValuePair<string, object>> fields, IEqualityComparer<string> comparer = null) {
      Contract.Requires(This != null);
      Contract.Requires(fields != null);
      var fieldCache = fields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer);
      return (This.FormatWithEx(f => fieldCache.ContainsKey(f) ? fieldCache[f] : null));
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="comparer">The comparer.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
    public static string FormatWithEx(this string This, IEqualityComparer<string> comparer, params KeyValuePair<string, object>[] fields) {
      Contract.Requires(This != null);
      Contract.Requires(fields != null);
      return (This.FormatWithEx(fields, comparer));
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
    public static string FormatWithEx(this string This, params KeyValuePair<string, object>[] fields) {
      Contract.Requires(This != null);
      Contract.Requires(fields != null);
      return (This.FormatWithEx(fields, null));
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="fieldGetter">The field getter.</param>
    /// <param name="passFieldFormatToGetter">if set to <c>true</c> passes the field format to getter.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatWithEx(this string This, Func<string, object> fieldGetter, bool passFieldFormatToGetter = false) {
      Contract.Requires(This != null);
      Contract.Requires(fieldGetter != null);

      var length = This.Length;

      // we will store parts of the newly generated string here
      var result = new StringBuilder(length);

      var i = 0;
      var lastStartPos = 0;
      var isInField = false;

      // looping through all characters breaking it up into parts that need to be get using the field getter 
      // and parts that simply need to be copied
      while (i < length) {
        var current = This[i++];

        if (isInField) {

          // we're currently reading a field
          if (current == '}') {

            // end of field found, pass field description to field getter
            isInField = false;
            var fieldContent = This.Substring(lastStartPos, i - lastStartPos - 1);
            lastStartPos = i;

            int formatStartIndex;
            if (passFieldFormatToGetter || (formatStartIndex = fieldContent.IndexOf(':')) < 0)
              result.Append(fieldGetter(fieldContent));
            else {
              var fieldName = fieldContent.Left(formatStartIndex);
              var fieldFormat = fieldContent.Substring(formatStartIndex + 1);
              result.Append(string.Format("{0:" + fieldFormat + "}", fieldGetter(fieldName)));
            }

          }
        } else {

          // we're currently copying
          if (current == '{') {

            // copy what we've already got
            var textContent = This.Substring(lastStartPos, i - lastStartPos - 1);
            lastStartPos = i;
            result.Append(textContent);

            // filter out double brackets
            if ((i >= length - 1 || This[i] != '{')) {

              // field start found, switch mode
              isInField = true;
            } else {

              // skip the following bracket
              ++i;
            }
          }
        }
      }
      var remainingContent = This.Substring(lastStartPos);
      result.Append(remainingContent);
      return (result.ToString());
    }

    /// <summary>
    /// Uses the string as a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns>An new instance of RegularExpression.</returns>
    public static Regex AsRegularExpression(this string This) {
      Contract.Requires(This != null);
      return (new Regex(This));
    }

    /// <summary>
    /// Uses the string as a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="options">The regex options.</param>
    /// <returns>
    /// An new instance of RegularExpression.
    /// </returns>
    public static Regex AsRegularExpression(this string This, RegexOptions options) {
      Contract.Requires(This != null);
      return (new Regex(This, options));
    }

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, params KeyValuePair<string, object>[] replacements) {
      return (MultipleReplace(This, (IEnumerable<KeyValuePair<string, object>>)replacements));
    }

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, params KeyValuePair<string, string>[] replacements) {
      return (MultipleReplace(This, (IEnumerable<KeyValuePair<string, string>>)replacements));
    }

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, IEnumerable<KeyValuePair<string, string>> replacements) {
      return (MultipleReplace(This, replacements == null ? null : replacements.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value))));
    }

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, IEnumerable<KeyValuePair<string, object>> replacements) {
      if (string.IsNullOrEmpty(This) || replacements == null)
        return (This);

      var mask = This.Replace("{", "{{").Replace("}", "}}");
      var parameters = new List<object>();
      foreach (var keyValuePair in replacements) {
        if (string.IsNullOrEmpty(keyValuePair.Key))
          continue;
        var index = parameters.Count;
        var oldValue = keyValuePair.Key.Replace("{", "{{").Replace("}", "}}");
        Contract.Assume(mask.Length > 0);
        Contract.Assume(oldValue.Length > 0);
        mask = mask.Replace(oldValue, "{" + index + "}");
        parameters.Add(keyValuePair.Value);
      }
      var result = string.Format(mask, parameters.ToArray());
      return (result);
    }

    /// <summary>
    /// Replaces using a regular expression.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="newValue">The replacement.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns>A string with the replacements.</returns>
    public static string ReplaceRegex(this string This, string regex, string newValue = null, RegexOptions regexOptions = RegexOptions.None) {
      Contract.Requires(regex != null);
      return (This == null ? null : new Regex(regex, regexOptions).Replace(This, newValue ?? string.Empty));
    }
    /// <summary>
    /// Replaces using a regular expression.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="newValue">The replacement.</param>
    /// <returns>
    /// A string with the replacements.
    /// </returns>
    public static string Replace(this string This, Regex regex, string newValue) {
      Contract.Requires(regex != null);
      Contract.Requires(newValue != null);
      return (This == null ? null : regex.Replace(This, newValue));
    }

    /// <summary>
    /// Replaces in a string but only n number of times.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="oldValue">What to replace.</param>
    /// <param name="newValue">The replacement.</param>
    /// <param name="count">The number of times this gets replaced.</param>
    /// <param name="comparison">The comparison mode; defaults to CurrentCulture.</param>
    /// <returns></returns>
    public static string Replace(this string This, string oldValue, string newValue, int count, StringComparison comparison = StringComparison.CurrentCulture) {
      Contract.Requires(Enum.IsDefined(typeof(StringComparison), comparison));
      if (This == null || oldValue == null || count < 1)
        return (This);
      if (newValue == null)
        newValue = string.Empty;
      var result = This;

      var removedLength = oldValue.Length;
      var newLength = newValue.Length;

      var pos = 0;
      for (var i = count; i > 0; ) {
        --i;
        Contract.Assume(pos < result.Length);
        var n = result.IndexOf(oldValue, pos, comparison);
        if (n < 0)
          break;
        if (n == 0) {
          Contract.Assume(removedLength <= result.Length);
          result = newValue + result.Substring(removedLength);
        } else {
          Contract.Assume((n + removedLength) <= result.Length);
          result = result.Substring(0, n) + newValue + result.Substring(n + removedLength);
        }
        pos = n + newLength;
      }
      return (result);
    }

    /// <summary>
    /// Uppers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
    public static string UpperFirst(this string This) {
      if (This == null)
        return null;
      if (This.Length == 1)
        return This.ToUpper();
      Contract.Assume(This.Length > 1);
      return This.Substring(0, 1).ToUpper() + This.Substring(1);
    }

    /// <summary>
    /// Uppers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
    public static string UpperFirstInvariant(this string This) {
      if (This == null)
        return null;
      if (This.Length == 1)
        return This.ToUpperInvariant();
      Contract.Assume(This.Length > 1);
      return This.Substring(0, 1).ToUpperInvariant() + This.Substring(1);
    }

    /// <summary>
    /// Lowers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
    public static string LowerFirst(this string This) {
      if (This == null)
        return null;
      if (This.Length == 1)
        return This.ToLower();
      Contract.Assume(This.Length > 1);
      return This.Substring(0, 1).ToLower() + This.Substring(1);
    }

    /// <summary>
    /// Lowers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
    public static string LowerFirstInvariant(this string This) {
      if (This == null)
        return null;
      if (This.Length == 1)
        return This.ToLowerInvariant();
      Contract.Assume(This.Length > 1);
      return This.Substring(0, 1).ToLowerInvariant() + This.Substring(1);
    }

    /// <summary>
    /// Splits the specified string by another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="splitter">The splitter.</param>
    /// <param name="max">The maximum number of splits, 0 means as often as possible.</param>
    /// <returns>The parts.</returns>
    public static IEnumerable<string> Split(this string This, string splitter, qword max = 0) {
      Contract.Requires(!string.IsNullOrEmpty(splitter));
      if (This == null)
        yield break;

      var splitterLength = splitter.Length;
      int startIndex;
      if (max == 0)
        max = qword.MaxValue;

      var currentStartIndex = 0;

      Contract.Assume(currentStartIndex <= This.Length);
      while (max-- > 0 && (startIndex = This.IndexOf(splitter, currentStartIndex)) >= 0) {
        yield return (This.Substring(currentStartIndex, startIndex - currentStartIndex));
        currentStartIndex = startIndex + splitterLength;
      }

      Contract.Assume(currentStartIndex <= This.Length);
      yield return (This.Substring(currentStartIndex));
    }

    /// <summary>
    /// Splits the specified string using a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex to use.</param>
    /// <returns>The parts.</returns>
    public static string[] Split(this string This, Regex regex) {
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
      return (regex.Split(This));
    }

    /// <summary>
    /// Converts a word to camel case.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="culture">The culture to use; defaults to current culture.</param>
    /// <returns>Something like "CamelCase" from "  camel-case_" </returns>
    public static string ToCamelCase(this string This, CultureInfo culture = null) {
      if (This == null)
        return (null);
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      var result = new StringBuilder();
      var hump = true;
      const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

      foreach (var chr in This) {
        if (allowedChars.IndexOf(chr) < 0)
          hump = true;
        else {
          result.Append(hump ? chr.ToString(culture).ToUpper(culture) : chr.ToString(culture));
          hump = false;
        }
      }

      return (result.ToString());
    }

    /// <summary>
    /// Transforms the given connection string into a linq2sql compatible one by removing the driver.
    /// </summary>
    /// <param name="This">This ConnectionString.</param>
    /// <returns>The transformed result.</returns>
    public static string ToLinq2SqlConnectionString(this string This) {
      Contract.Requires(This != null);
      var regex = new Regex(@"Driver\s*=.*?(;|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
      return (regex.Replace(This, string.Empty));
    }

    /// <summary>
    /// Escapes the string to be used as sql data.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns></returns>
    public static string MsSqlDataEscape(this object This) {
      return (This == null ? "NULL" : "'" + This.ToString().Replace("'", "''") + "'");
    }

    /// <summary>
    /// Escapes the string to be used as sql identifiers eg. table or column names.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns></returns>
    public static string MsSqlIdentifierEscape(this string This) {
      Contract.Requires(!string.IsNullOrWhiteSpace(This));
      return ("[" + This.Replace("]", "]]") + "]");
    }

    /// <summary>
    /// Checks if the string equals to any from the given list.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string the matches; otherwise, <c>false</c>.</returns>
    public static bool EqualsAny(this string This, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(values != null);
      return (values.Any(s => string.Equals(This, s, stringComparison)));
    }

    /// <summary>
    /// Checks if the string starts with any from the given list.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
    public static bool StartsWithAny(this string This, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(This != null);
      Contract.Requires(values != null);
      return (values.Any(s => This.StartsWith(s, stringComparison)));
    }

    /// <summary>
    /// Checks if the string ends with any from the given list.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
    public static bool EndsWithAny(this string This, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(This != null);
      Contract.Requires(values != null);
      return (values.Any(s => This.EndsWith(s, stringComparison)));
    }

    /// <summary>
    /// Checks whether the given string starts with the specified character.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>
    ///   <c>true</c> if the string starts with the given character; otherwise, <c>false</c>.
    /// </returns>
    public static bool StartsWith(this string This, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(This != null);
      return (This.Length > 0 && string.Equals(This[0] + string.Empty, value + string.Empty, stringComparison));
    }

    /// <summary>
    /// Checks whether the given string ends with the specified character.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>
    ///   <c>true</c> if the string ends with the given character; otherwise, <c>false</c>.
    /// </returns>
    public static bool EndsWith(this string This, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(This != null);
      return (This.Length > 0 && string.Equals(This[This.Length - 1] + string.Empty, value + string.Empty, stringComparison));
    }

    /// <summary>
    /// Determines whether the given string is surrounded by another one.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="text">The text that should be around the given string.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>
    ///   <c>true</c> if the given string is surrounded by the given text; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSurroundedWith(this string This, string text, StringComparison stringComparison = StringComparison.CurrentCulture) {
      Contract.Requires(This != null);
      Contract.Requires(text != null);
      return (This.StartsWith(text, stringComparison) && This.EndsWith(text, stringComparison));
    }

    /// <summary>
    /// Replaces a specified string at the start of another if possible.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="what">What to replace.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
    public static string ReplaceAtStart(this string This, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
      if (This == null && what == null)
        return (replacement);
      if (This == null || This.Length < what.Length)
        return (This);
      return (This.StartsWith(what, stringComparison) ? replacement + This.Substring(what.Length) : This);
    }

    /// <summary>
    /// Replaces a specified string at the end of another if possible.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="what">What to replace.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
    public static string ReplaceAtEnd(this string This, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
      if (This == null && what == null)
        return (replacement);
      if (This == null || This.Length < what.Length)
        return (This);
      return (This.EndsWith(what, stringComparison) ? This.Substring(0, This.Length - what.Length) + replacement : This);
    }

    /// <summary>
    /// Determines whether the string is <c>null</c> or empty.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(this string This) {
      return (string.IsNullOrEmpty(This));
    }

    /// <summary>
    /// Determines whether the string is <c>null</c> or whitespace.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is <c>null</c> or whitespace; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string This) {
      return (string.IsNullOrWhiteSpace(This));
    }

    /// <summary>
    /// Determines whether a given string contains another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="other">The string to look for.</param>
    /// <param name="comparisonType">Type of the comparison.</param>
    /// <returns>
    ///   <c>true</c> if the other string is part of the given string; otherwise, <c>false</c>.
    /// </returns>
    public static bool Contains(this string This, string other, StringComparison comparisonType) {
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      return (This.IndexOf(other, comparisonType) >= 0);
    }

    /// <summary>
    /// Returns a default value if the given string is <c>null</c> or empty.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The given string or the given default value.</returns>
    public static string DefaultIfNullOrEmpty(this string This, string defaultValue = null) {
      return (This.IsNullOrEmpty() ? defaultValue : This);
    }

    /// <summary>
    /// Returns a default value if the given string is <c>null</c> or whitespace.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The given string or the given default value.</returns>
    public static string DefaultIfNullOrWhiteSpace(this string This, string defaultValue = null) {
      return (This.IsNullOrWhiteSpace() ? defaultValue : This);
    }

    /// <summary>
    /// Breaks the given string down into lines.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="delimiter">The delimiter, <c>null</c> means try autodetecting the line break.</param>
    /// <param name="count">The maximum number of lines to support, 0 means unlimited.</param>
    /// <param name="options">The options.</param>
    /// <returns>The lines which where identified.</returns>
    public static string[] Lines(this string This, string delimiter = null, int count = 0, StringSplitOptions options = StringSplitOptions.None) {
      if (This == null)
        return (null);

      // if no delimtier given, try auto-detection
      if (delimiter == null) {
        if (This.Contains("\r\n"))
          delimiter = "\r\n";
        else if (This.Contains("\n\r"))
          delimiter = "\n\r";
        else if (This.Contains("\n"))
          delimiter = "\n";
        else if (This.Contains("\r"))
          delimiter = "\r";
      }

      // if no delimiter could be found, just return one line
      if (delimiter == null)
        return (new[] { This });

      return count < 1 ? (This.Split(new[] { delimiter }, options)) : (This.Split(new[] { delimiter }, count, options));
    }

    #region parsers
    #region Word
    public static word ParseWord(this string This) {
      Contract.Requires(This != null);
      return (word.Parse(This));
    }

    public static word ParseWord(this string This, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (word.Parse(This, provider));
    }

    public static word ParseWord(this string This, NumberStyles numberStyles) {
      Contract.Requires(This != null);
      return (word.Parse(This, numberStyles));
    }

    public static word ParseWord(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (word.Parse(This, numberStyles, provider));
    }

    public static bool TryParseWord(this string This, out word result) {
      return (word.TryParse(This, out result));
    }

    public static bool TryParseWord(this string This, NumberStyles numberStyles, IFormatProvider provider, out word result) {
      return (word.TryParse(This, numberStyles, provider, out result));
    }

    public static word ParseWordOrDefault(this string This, word defaultValue = default(word)) {
      word result;
      return (This.TryParseWord(out result) ? result : defaultValue);
    }

    public static word ParseWordOrDefault(this string This, NumberStyles numberStyles, IFormatProvider provider, word defaultValue = default(word)) {
      word result;
      return (This.TryParseWord(numberStyles, provider, out result) ? result : defaultValue);
    }

    public static word? ParseWordOrNull(this string This) {
      word result;
      return (This.TryParseWord(out result) ? result : (word?)null);
    }

    public static word? ParseWordOrNull(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      word result;
      return (This.TryParseWord(numberStyles, provider, out result) ? result : (word?)null);
    }
    #endregion
    #region Integer
    public static int ParseInteger(this string This) {
      Contract.Requires(This != null);
      return (int.Parse(This));
    }

    public static int ParseInteger(this string This, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (int.Parse(This, provider));
    }

    public static int ParseInteger(this string This, NumberStyles numberStyles) {
      Contract.Requires(This != null);
      return (int.Parse(This, numberStyles));
    }

    public static int ParseInteger(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (int.Parse(This, numberStyles, provider));
    }

    public static bool TryParseInteger(this string This, out int result) {
      return (int.TryParse(This, out result));
    }

    public static bool TryParseInteger(this string This, NumberStyles numberStyles, IFormatProvider provider, out int result) {
      return (int.TryParse(This, numberStyles, provider, out result));
    }

    public static int ParseIntegerOrDefault(this string This, int defaultValue = default(int)) {
      int result;
      return (This.TryParseInteger(out result) ? result : defaultValue);
    }

    public static int ParseIntegerOrDefault(this string This, NumberStyles numberStyles, IFormatProvider provider, int defaultValue = default(int)) {
      int result;
      return (This.TryParseInteger(numberStyles, provider, out result) ? result : defaultValue);
    }

    public static int? ParseIntegerOrNull(this string This) {
      int result;
      return (This.TryParseInteger(out result) ? result : (int?)null);
    }

    public static int? ParseIntegerOrNull(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      int result;
      return (This.TryParseInteger(numberStyles, provider, out result) ? result : (int?)null);
    }
    #endregion
    #region Float
    public static float ParseFloat(this string This) {
      Contract.Requires(This != null);
      return (float.Parse(This));
    }

    public static float ParseFloat(this string This, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (float.Parse(This, provider));
    }

    public static float ParseFloat(this string This, NumberStyles numberStyles) {
      Contract.Requires(This != null);
      return (float.Parse(This, numberStyles));
    }

    public static float ParseFloat(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      Contract.Requires(This != null);
      return (float.Parse(This, numberStyles, provider));
    }

    public static bool TryParseFloat(this string This, out float result) {
      return (float.TryParse(This, out result));
    }

    public static bool TryParseFloat(this string This, NumberStyles numberStyles, IFormatProvider provider, out float result) {
      return (float.TryParse(This, numberStyles, provider, out result));
    }

    public static float ParseFloatOrDefault(this string This, float defaultValue = default(float)) {
      float result;
      return (This.TryParseFloat(out result) ? result : defaultValue);
    }

    public static float ParseFloatOrDefault(this string This, NumberStyles numberStyles, IFormatProvider provider, float defaultValue = default(float)) {
      float result;
      return (This.TryParseFloat(numberStyles, provider, out result) ? result : defaultValue);
    }

    public static float? ParseFloatOrNull(this string This) {
      float result;
      return (This.TryParseFloat(out result) ? result : (float?)null);
    }

    public static float? ParseFloatOrNull(this string This, NumberStyles numberStyles, IFormatProvider provider) {
      float result;
      return (This.TryParseFloat(numberStyles, provider, out result) ? result : (float?)null);
    }
    #endregion

    #endregion

    /// <summary>
    /// Splits the given string respecting single and double quotes and allows for escape seququences to be used in these strings.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="delimiter">The delimiter to use.</param>
    /// <param name="escapeSequence">The escape sequence.</param>
    /// <returns>A sequence containing the parts of the string.</returns>
    public static IEnumerable<string> QuotedSplit(this string This, string delimiter = ",", string escapeSequence = "\\") {
      if (This == null)
        yield break;

      if (delimiter.IsNullOrEmpty()) {
        yield return This;
        yield break;
      }

      if (escapeSequence == "")
        escapeSequence = null;

      var length = This.Length;
      var pos = 0;
      var currentlyEscaping = false;
      var currentlyInSingleQuote = false;
      var currentlyInDoubleQuote = false;
      var currentPart = string.Empty;

      while (pos < length) {
        var chr = This[pos++];

        if (currentlyEscaping) {
          currentPart += chr;
          currentlyEscaping = false;
        } else if (currentlyInSingleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && This.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length;
          } else if (chr == '\'')
            currentlyInSingleQuote = false;
          else
            currentPart += chr;
        } else if (currentlyInDoubleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && This.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length;
          } else if (chr == '"')
            currentlyInDoubleQuote = false;
          else
            currentPart += chr;
        } else if (chr == ' ') {
          continue;
        } else if (delimiter.StartsWith(chr) && This.Substring(pos - 1, delimiter.Length) == delimiter) {
          yield return (currentPart);
          currentPart = string.Empty;
          pos += delimiter.Length - 1;
        } else if (currentPart.Length == 0 && chr == '\'') {
          currentlyInSingleQuote = true;
        } else if (currentPart.Length == 0 && chr == '"') {
          currentlyInDoubleQuote = true;
        } else {
          currentPart += chr;
        }

      }
      yield return (currentPart);
    }

    /// <summary>
    /// Parses the host and port from a given string.
    /// </summary>
    /// <param name="This">This String, e.g. 172.17.4.3:http .</param>
    /// <returns>Port and host, <c>null</c> on error during parsing.</returns>
    public static Tuple<string, word> ParseHostAndPort(this string This) {
      if (string.IsNullOrWhiteSpace(This))
        return (null);
      var host = This;
      word port = 0;
      var index = host.IndexOf(':');
      if (index >= 0) {
        var portText = host.Substring(index + 1);
        host = host.Left(index);
        if (!word.TryParse(portText, out port) && !_OFFICIAL_PORT_NAMES.TryGetValue(portText.Trim().ToLower(), out port))
          return (null);
      }
      return (Tuple.Create(host, port));
    }
  }
}