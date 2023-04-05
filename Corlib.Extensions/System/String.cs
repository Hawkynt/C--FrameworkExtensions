#region (c)2010-2042 Hawkynt
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

using System.Collections;
using System.Collections.Generic;
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Guard;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class StringExtensions {
    #region consts
    /// <summary>
    /// This is a list of services which are registered to certain ports according to IANA.
    /// It allows us to use names for these ports if we want to.
    /// </summary>
    private static readonly Dictionary<string, ushort> _OFFICIAL_PORT_NAMES = new() {
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
    /// Exchanges a certain part of the string with the given newString.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="index">The index to start the modification at.</param>
    /// <param name="newString">The new string to insert.</param>
    /// <returns>
    /// The modified string
    /// </returns>
    public static string ExchangeAt(this string @this, int index, string newString) => ExchangeAt(@this, index, newString.Length, newString);

    /// <summary>
    /// Exchanges a certain character of the string with the given character.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="index">The index to start the modification at.</param>
    /// <param name="character">The character.</param>
    /// <returns>
    /// The modified string
    /// </returns>
    public static string ExchangeAt(this string @this, int index, char character) => ExchangeAt(@this, index, 1, character.ToString());

    /// <summary>
    /// Exchanges a certain part of the string with the given newString.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="index">The index to start the modification at.</param>
    /// <param name="count">The number of characters to replace.</param>
    /// <param name="newString">The new string to insert.</param>
    /// <returns>The modified string</returns>
    public static string ExchangeAt(this string @this, int index, int count, string newString) => Left(@this, index) + newString + (@this.Length > index + count ? @this.Substring(index + count) : string.Empty);

    public static bool IsIn(this string @this, IEnumerable<string> values) => values.Contains(@this);
    public static bool IsNotIn(this string @this, IEnumerable<string> values) => !IsIn(@this, values);

    /// <summary>
    /// Repeats the specified string a certain number of times.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="count">The count.</param>
    /// <returns></returns>
    public static string Repeat(this string @this, int count) {
      if (@this == null)
        return null;

      if (count < 1)
        return string.Empty;

      var n = new StringBuilder(@this.Length * count);

      for (var i = 0; i < count; i++)
        n.Append(@this);

      return n.ToString();
    }

    /// <summary>
    /// Removes the last n chars from a string.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <returns>The new string</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string RemoveLast(this string @this, int count) => string.IsNullOrEmpty(@this) || count < 1 ? @this : @this.Length < count ? string.Empty : @this.Substring(0, @this.Length - count);

    /// <summary>
    /// Removes the first n chars from a string.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="count">The number of characters to remove.</param>
    /// <returns>The new string</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string RemoveFirst(this string @this, int count) => string.IsNullOrEmpty(@this) || count < 1 ? @this : @this.Length < count ? string.Empty : @this.Substring(count);

    /// <summary>
    /// Gets a substring.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="start">The start index of the first char that should be contained in the result; can be negative to indicate a "from the end".</param>
    /// <param name="end">The end index of the first char not contained in the result; can be negative to indicate a "from-the-end".</param>
    /// <returns>the substring</returns>
    public static string SubString(this string @this, int start, int end = 0) {
      if (@this == null)
        return null;
      string result;
      var length = @this.Length;
      if (length > 0) {

        // allow specifying start from back of the string when negative
        if (start < 0)
          start += length;

        if (start < 0)
          start = 0;

        // allow specifying end from back of the string when negative
        if (end <= 0)
          end += length;

        var len = end - start;

        if (len > length)
          len = length - start;

        // when reading too less chars -> returns empty string
        if (len <= 0)
          return string.Empty;

#if SUPPORTS_CONTRACTS
        Contract.Assume(len >= 0);
        Contract.Assume(start + len <= @this.Length);
#endif
        result = @this.Substring(start, len);
      } else {
        result = string.Empty;
      }
      return result;
    }
    /// <summary>
    /// Gets the first n chars from a string.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="length">The number of chars to get.</param>
    /// <returns>A string with the first n chars.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string Left(this string @this, int length) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(length >= 0);
#endif
      return @this?.Substring(0, Math.Min(length, @this.Length));
    }

    /// <summary>
    /// Gets the last n chars from a string.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="length">The number of chars to get.</param>
    /// <returns>A string with the last n chars.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string Right(this string @this, int length) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(length >= 0);
#endif
      // ReSharper disable once UseNullPropagation
      if (@this == null)
        return null;

      var totalLen = @this.Length;
      return @this.Substring(totalLen - Math.Min(totalLen, length));
    }
    
    private static readonly Lazy<HashSet<char>> _INVALID_FILE_NAME_CHARS = new(() => Path.GetInvalidFileNameChars().ToHashSet(c=>c));

    /// <summary>
    /// Sanitizes the text to use as a filename.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <returns>The sanitized string.</returns>
    public static string SanitizeForFileName(this string @this) {
      if (IsNullOrEmpty(@this))
        AlwaysThrow.ArgumentNullException(nameof(@this));

      var invalidFileNameChars = _INVALID_FILE_NAME_CHARS.Value;
      var result = @this.ToCharArray();
      for (var i = 0; i < result.Length; ++i) {
        if (invalidFileNameChars.Contains(result[i]))
          result[i] = '_';
      }

      return result.ToStringInstance();
    }

    #region needed consts for converting filename patterns into regexes
    private static readonly Regex _ILLEGAL_FILENAME_CHARACTERS = new("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
    private static readonly Regex _CATCH_FILENAME_EXTENSION = new(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
    
    #endregion

    /// <summary>
    /// Converts a given filename pattern into a regular expression.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The regex.</returns>
    private static Regex _ConvertFilePatternToRegex(string pattern) {

      const string nonDotCharacters = @"[^.]*";

      if (pattern == null)
        throw new ArgumentNullException();

      pattern = pattern.Trim();
      if (pattern.Length == 0)
        throw new ArgumentException("Pattern is empty.");

      if (_ILLEGAL_FILENAME_CHARACTERS.IsMatch(pattern))
        throw new ArgumentException("Patterns contains ilegal characters.");

      var hasExtension = _CATCH_FILENAME_EXTENSION.IsMatch(pattern);
      var matchExact = false;

      if (pattern.IndexOf('?') >= 0)
        matchExact = true;
      else if (hasExtension)
        matchExact = _CATCH_FILENAME_EXTENSION.Match(pattern).Groups[1].Length != 3;

      var regexString = Regex.Escape(pattern);
      regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
      regexString = Regex.Replace(regexString, @"\\\?", ".");

      if (!matchExact && hasExtension)
        regexString += nonDotCharacters;

      regexString += "$";
      return new Regex(regexString, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Determines if the given string matches a given file pattern or not.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="pattern">The pattern to apply.</param>
    /// <returns><c>true</c> if the string matches the file pattern; otherwise, <c>false</c>.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool MatchesFilePattern(this string @this, string pattern) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(pattern != null);
#endif
      return _ConvertFilePatternToRegex(pattern).IsMatch(@this);
    }

    /// <summary>
    /// Determines whether the specified string matches the given regex.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex.</param>
    /// <returns>
    ///   <c>true</c> if it matches; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsMatch(this string This, Regex regex) => This != null && regex.IsMatch(This);

    /// <summary>
    /// Determines whether the specified string matches the given regex.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex.</param>
    /// <returns>
    ///   <c>false</c> if it matches; otherwise, <c>true</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNotMatch(this string This, Regex regex) => !IsMatch(This, regex);

    /// <summary>
    /// Determines whether the specified string matches the given regex.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns><c>true</c> if it matches; otherwise, <c>false</c>.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsMatch(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) => This != null && This.IsMatch(new Regex(regex, regexOptions));

    /// <summary>
    /// Determines whether the specified string matches the given regex.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns><c>false</c> if it matches; otherwise, <c>true</c>.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNotMatch(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) => !IsMatch(This, regex, regexOptions);

    /// <summary>
    /// Matches the specified regex.
    /// </summary>
    /// <param name="regex">The regex.</param>
    /// <param name="This">The data.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns>A <see cref="MatchCollection"/> containing the matches.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static MatchCollection Matches(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
#endif
      return new Regex(regex, regexOptions).Matches(This);
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
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static GroupCollection MatchGroups(this string This, string regex, RegexOptions regexOptions = RegexOptions.None) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
#endif
      return new Regex(regex, regexOptions).Match(This).Groups;
    }

    /// <summary>
    /// Uses the string as a format string.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="parameters">The parameters to use for formatting.</param>
    /// <returns>A formatted string.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static string FormatWith(this string This, params object[] parameters) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(parameters != null);
#endif
      return string.Format(This, parameters);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns></returns>
    public static string FormatWithEx(this string @this, IEnumerable<KeyValuePair<string, object>> fields, IEqualityComparer<string> comparer = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(fields != null);
#endif
      var fieldCache = fields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer);
      return FormatWithEx(@this, f => fieldCache.ContainsKey(f) ? fieldCache[f] : null);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="comparer">The comparer.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string FormatWithEx(this string @this, IEqualityComparer<string> comparer, params KeyValuePair<string, object>[] fields) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(fields != null);
#endif
      return FormatWithEx(@this, fields, comparer);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string FormatWithEx(this string @this, params KeyValuePair<string, object>[] fields) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(fields != null);
#endif
      return FormatWithEx(@this, fields, null);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string FormatWithEx(this string @this, IDictionary<string, string> fields) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(fields != null);
#endif
      return FormatWithEx(@this, f => fields[f]);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="fields">The fields.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string FormatWithEx(this string @this, Hashtable fields) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(fields != null);
#endif
      return FormatWithEx(@this, f => fields[f]);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="object">The source to get the data from using properties of the same name.</param>
    /// <returns>The string with values</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string FormatWithObject<T>(this string @this, T @object) {

      if (@object == null)
        return @this.FormatWithEx(_ => null);

      var cache = new Dictionary<string, Func<object>>();
      var type = @object.GetType();

      object Getter(string field) {
        if (cache.TryGetValue(field, out var call))
          return call();

        var method = type.GetProperty(field)?.GetGetMethod();
        if (method == null || method.GetParameters().Length != 0) {
          cache.Add(field, () => null);
          return null;
        }

        var returnType = method.ReturnType;

        // short-cut: non-generic return values
        if (!returnType.IsGenericType) {
          call = () => method.Invoke(@object, null);
          cache.Add(field, call);
          return call();
        }

        var genericType = returnType.GetGenericTypeDefinition();
        if (genericType == typeof(Func<,>)) {

          // support for Func<string,>
          var genericArguments = returnType.GetGenericArguments();
          if (genericArguments.Length == 2 && genericArguments[0] == typeof(string))
            call = () => {
              var func = (Delegate)method.Invoke(@object, null);
              return func.DynamicInvoke(field);
            };
          else /* something else */
            call = () => method.Invoke(@object, null);
        } else if (genericType == typeof(Func<>))

          // support for Func<>
          call = () => {
            var func = (Delegate)method.Invoke(@object, null);
            return func.DynamicInvoke();
          };
        else /* non Func<,> / Func<> */
          call = () => method.Invoke(@object, null);

        cache.Add(field, call);
        return call();
      }

      return @this.FormatWithEx(Getter);
    }

    /// <summary>
    /// Uses the string as a format string allowing an extended syntax to get the fields eg. {FieldName:FieldFormat}
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="fieldGetter">The field getter.</param>
    /// <param name="passFieldFormatToGetter">if set to <c>true</c> passes the field format to getter.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatWithEx(this string @this, Func<string, object> fieldGetter, bool passFieldFormatToGetter = false) {
      if (@this == null)
        throw new NullReferenceException();
      if (fieldGetter == null)
        throw new ArgumentNullException(nameof(fieldGetter));

      var length = @this.Length;

      // we will store parts of the newly generated string here
      var result = new StringBuilder(length);

      var i = 0;
      var lastStartPos = 0;
      var isInField = false;

      // looping through all characters breaking it up into parts that need to be get using the field getter
      // and parts that simply need to be copied
      while (i < length) {
        var current = @this[i++];
        var next = i < length ? @this[i].ToString() : null;

        var fieldContentLength = i - lastStartPos - 1;
#if SUPPORTS_CONTRACTS
        Contract.Assume(fieldContentLength >= 0 && fieldContentLength < @this.Length);
#endif

        if (isInField) {

          // we're currently reading a field
          if (current != '}')
            continue;

          // end of field found, pass field description to field getter
          isInField = false;
          var fieldContent = @this.Substring(lastStartPos, fieldContentLength);
          lastStartPos = i;

          int formatStartIndex;
          if (passFieldFormatToGetter || (formatStartIndex = fieldContent.IndexOf(':')) < 0)
            result.Append(fieldGetter(fieldContent));
          else {
            var fieldName = fieldContent.Left(formatStartIndex);
            var fieldFormat = fieldContent.Substring(formatStartIndex + 1);
            result.Append(string.Format($"{{0:{fieldFormat}}}", fieldGetter(fieldName)));
          }
        } else {

          // we're currently copying
          if (current == '{') {

            // copy what we've already got
            var textContent = @this.Substring(lastStartPos, fieldContentLength);
            lastStartPos = i;
            result.Append(textContent);

            // filter out double brackets
            if (next != null && next == "{") {

              // skip the following bracket
              ++i;
            } else {

              // field start found, switch mode
              isInField = true;
            }
          } else if (current == '}' && next != null && next == "}") {

            // copy what we've already got
            var textContent = @this.Substring(lastStartPos, fieldContentLength);
            lastStartPos = i;
            result.Append(textContent);

            // skip double brackets
            ++i;
          }
        }
      }
      var remainingContent = @this.Substring(lastStartPos);
      result.Append(remainingContent);
      return result.ToString();
    }

    /// <summary>
    /// Uses the string as a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns>An new instance of RegularExpression.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Regex AsRegularExpression(this string This) => This == null ? null : new Regex(This);

    /// <summary>
    /// Uses the string as a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="options">The regex options.</param>
    /// <returns>
    /// An new instance of RegularExpression.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Regex AsRegularExpression(this string This, RegexOptions options) => This == null ? null : new Regex(This, options);

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string MultipleReplace(this string This, params KeyValuePair<string, object>[] replacements) => MultipleReplace(This, (IEnumerable<KeyValuePair<string, object>>)replacements);


#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string MultipleReplace(this string This, string replacement, params string[] toReplace) => MultipleReplace(This, toReplace.Select(s => new KeyValuePair<string, string>(s, replacement)));

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string MultipleReplace(this string This, params KeyValuePair<string, string>[] replacements) => MultipleReplace(This, (IEnumerable<KeyValuePair<string, string>>)replacements);

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, IEnumerable<KeyValuePair<string, string>> replacements) => MultipleReplace(This, replacements?.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)));

    /// <summary>
    /// Replaces multiple contents.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>A new string containing all parts replaced.</returns>
    public static string MultipleReplace(this string This, IEnumerable<KeyValuePair<string, object>> replacements) {
      if (string.IsNullOrEmpty(This) || replacements == null)
        return This;

      var list = replacements.OrderByDescending(kvp => kvp.Key.Length).ToArray();
      var length = This.Length;
      var result = new StringBuilder(length);
      for (var i = 0; i < length; ++i) {
        var found = false;
        foreach (var kvp in list) {
          var keyLength = kvp.Key.Length;
          if (i + keyLength > length)
            continue;

          var part = This.Substring(i, keyLength);
          if (kvp.Key != part)
            continue;

          result.Append(kvp.Value);
          found = true;

          //support for string replacements greater than 1 char
          i += keyLength - 1;
          break;
        }

        if (!found)
          result.Append(This[i]);

      }

      return result.ToString();
    }

    /// <summary>
    /// Replaces using a regular expression.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="newValue">The replacement.</param>
    /// <param name="regexOptions">The regex options.</param>
    /// <returns>A string with the replacements.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ReplaceRegex(this string This, string regex, string newValue = null, RegexOptions regexOptions = RegexOptions.None) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(regex != null);
#endif
      return This == null ? null : new Regex(regex, regexOptions).Replace(This, newValue ?? string.Empty);
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
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string Replace(this string This, Regex regex, string newValue) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(regex != null);
      Contract.Requires(newValue != null);
#endif
      return This == null ? null : regex.Replace(This, newValue);
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
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static string Replace(this string This, string oldValue, string newValue, int count, StringComparison comparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(Enum.IsDefined(typeof(StringComparison), comparison));
#endif
      if (This == null || oldValue == null || count < 1)
        return This;
      if (newValue == null)
        newValue = string.Empty;
      var result = This;

      var removedLength = oldValue.Length;
      var newLength = newValue.Length;

      var pos = 0;
      for (var i = count; i > 0;) {
        --i;
#if SUPPORTS_CONTRACTS
        Contract.Assume(pos < result.Length);
#endif
        var n = result.IndexOf(oldValue, pos, comparison);
        if (n < 0)
          break;
        if (n == 0) {
#if SUPPORTS_CONTRACTS
          Contract.Assume(removedLength <= result.Length);
#endif
          result = newValue + result.Substring(removedLength);
        } else {
#if SUPPORTS_CONTRACTS
          Contract.Assume(n + removedLength <= result.Length);
#endif
          result = result.Substring(0, n) + newValue + result.Substring(n + removedLength);
        }
        pos = n + newLength;
      }
      return result;
    }

    /// <summary>
    /// Uppers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string UpperFirst(this string This) => This == null ? null : This.Length == 1 ? This.ToUpper() : This.Substring(0, 1).ToUpper() + This.Substring(1);

    /// <summary>
    /// Uppers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string UpperFirstInvariant(this string This) => This == null ? null : This.Length == 1 ? This.ToUpperInvariant() : This.Substring(0, 1).ToUpperInvariant() + This.Substring(1);

    /// <summary>
    /// Lowers the first char in a string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string LowerFirst(this string This, CultureInfo culture = null) => This == null ? null : This.Length == 1 ? (culture == null ? This.ToLower() : This.ToLower(culture)) : (culture == null ? This.Substring(0, 1).ToLower() : This.Substring(0, 1).ToLower(culture)) + This.Substring(1);

    /// <summary>
    /// Lowers the first char in a string.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <returns>
    /// A string where the first char was capitalized.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string LowerFirstInvariant(this string @this) => @this == null ? null : @this.Length == 1 ? @this.ToLowerInvariant() : @this.Substring(0, 1).ToLowerInvariant() + @this.Substring(1);

    /// <summary>
    /// Splits a string into equal length parts.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="length">The number of chars each part should have</param>
    /// <returns>An enumeration of string parts</returns>
    public static IEnumerable<string> Split(this string @this,int length) {
      if (@this == null)
        throw new ArgumentNullException(nameof(@this));
      if (length <= 0)
        throw new ArgumentOutOfRangeException(nameof(length));

      for (var i = 0; i < @this.Length; i += length)
        yield return @this.Substring(i, length);
    }

    /// <summary>
    /// Splits the specified string by another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="splitter">The splitter.</param>
    /// <param name="max">The maximum number of splits, 0 means as often as possible.</param>
    /// <returns>The parts.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string[] Split(this string This, char splitter, int max = 0) => This.Split(splitter.ToString(), (ulong)max).ToArray();

    /// <summary>
    /// Splits the specified string by another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="splitter">The splitter.</param>
    /// <param name="max">The maximum number of splits, 0 means as often as possible.</param>
    /// <returns>The parts.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IEnumerable<string> Split(this string This, char splitter, ulong max = 0) => This.Split(splitter.ToString(), max);

    /// <summary>
    /// Splits the specified string by another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="splitter">The splitter.</param>
    /// <param name="max">The maximum number of splits, 0 means as often as possible.</param>
    /// <returns>The parts.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static IEnumerable<string> Split(this string This, string splitter, ulong max = 0) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(!string.IsNullOrEmpty(splitter));
#endif
      if (This == null)
        yield break;

      var splitterLength = splitter.Length;
      int startIndex;
      if (max == 0)
        max = ulong.MaxValue;

      var currentStartIndex = 0;

#if SUPPORTS_CONTRACTS
      Contract.Assume(currentStartIndex <= This.Length);
#endif
      while (max-- > 0 && (startIndex = This.IndexOf(splitter, currentStartIndex, StringComparison.Ordinal)) >= 0) {
        yield return This.Substring(currentStartIndex, startIndex - currentStartIndex);
        currentStartIndex = startIndex + splitterLength;
      }

#if SUPPORTS_CONTRACTS
      Contract.Assume(currentStartIndex <= This.Length);
#endif
      yield return This.Substring(currentStartIndex);
    }

    /// <summary>
    /// Splits the specified string using a regular expression.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="regex">The regex to use.</param>
    /// <returns>The parts.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string[] Split(this string This, Regex regex) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(regex != null);
#endif
      return regex.Split(This);
    }

    /// <summary>
    /// Splits the specified string.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="splitter">The splitter.</param>
    /// <param name="options">The options.</param>
    /// <returns>The parts</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string[] Split(this string This, string splitter, StringSplitOptions options) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return This.Split(new[] { splitter }, options);
    }

    /// <summary>
    /// Converts a word to camel case.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="culture">The culture to use; defaults to current culture.</param>
    /// <returns>Something like "CamelCase" from "  camel-case_" </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static string ToCamelCase(this string This, CultureInfo culture = null) {
      if (This == null)
        return null;
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      var result = new StringBuilder(This.Length);
      var hump = true;

      // ReSharper disable ConvertToConstant.Local
#pragma warning disable CC0030 // Make Local Variable Constant.
      var numbers = "0123456789";
      var allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
#pragma warning restore CC0030 // Make Local Variable Constant.
      // ReSharper restore ConvertToConstant.Local

      foreach (var chr in This) {
        if (numbers.IndexOf(chr) >= 0) {
          result.Append(chr);
          hump = true;
        } else if (allowedChars.IndexOf(chr) < 0)
          hump = true;
        else {
          result.Append((hump ? char.ToUpper(chr, culture) : chr).ToString(culture));
          hump = false;
        }
      }

      return result.ToString();
    }

    /// <summary>
    /// Converts a word to pascal case.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="culture">The culture to use; defaults to current culture.</param>
    /// <returns>Something like "pascalCase" from "  pascal-case_" </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ToPascalCase(this string This, CultureInfo culture = null) => This.ToCamelCase().LowerFirst(culture);

    /// <summary>
    /// Transforms the given connection string into a linq2sql compatible one by removing the driver.
    /// </summary>
    /// <param name="This">This ConnectionString.</param>
    /// <returns>The transformed result.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ToLinq2SqlConnectionString(this string This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      var regex = new Regex(@"Driver\s*=.*?(;|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
      return regex.Replace(This, string.Empty);
    }

    /// <summary>
    /// Escapes the string to be used as sql data.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string MsSqlDataEscape(this object This) => This == null ? "NULL" : "'" + string.Format(CultureInfo.InvariantCulture, "{0}", This).Replace("'", "''") + "'";

    /// <summary>
    /// Escapes the string to be used as sql identifiers eg. table or column names.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string MsSqlIdentifierEscape(this string This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(!This.IsNullOrWhiteSpace());
#endif
      return "[" + This.Replace("]", "]]") + "]";
    }

    /// <summary>
    /// Checks if the string equals to any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns>
    ///   <c>true</c> if there is at least one string the matches; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EqualsAny(this string @this, params string[] values) => EqualsAny(@this, (IEnumerable<string>)values);

    /// <summary>
    /// Checks if the string equals to any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns><c>true</c> if there is at least one string the matches; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EqualsAny(this string @this, StringComparison stringComparison, params string[] values) => EqualsAny(@this, values, stringComparison);

    /// <summary>
    /// Checks if the string equals to any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string the matches; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EqualsAny(this string @this, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(values != null);
#endif
      return values.Any(s => string.Equals(@this, s, stringComparison));
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWith(this string @this, char what, StringComparer comparer) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      return comparer?.Equals(@this.Length>0? @this[0] + string.Empty:string.Empty, what + string.Empty) ?? @this.StartsWith(what);
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsNotWith(this string @this, char what, StringComparer comparer) => !StartsWith(@this,what,comparer);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWith(this string @this, string what, StringComparer comparer) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      if (what == null)
        return @this==null;

      return comparer?.Equals(@this.Substring(0, what.Length), what) ?? @this.StartsWith(what);
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsNotWith(this string @this, string what, StringComparer comparer) => !StartsWith(@this, what, comparer);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWith(this string @this, char what, StringComparer comparer) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      return comparer?.Equals(@this.Length > 0 ? @this[@this.Length - 1] : string.Empty, what) ?? @this.EndsWith(what);
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsNotWith(this string @this, char what, StringComparer comparer) => !EndsWith(@this, what, comparer);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWith(this string @this, string what, StringComparer comparer) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      if (what == null)
        return @this == null;

      return comparer?.Equals(@this.Substring(Math.Max(0,@this.Length - what.Length)), what) ?? @this.EndsWith(what);
    }

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsNotWith(this string @this, string what, StringComparer comparer) => !EndsWith(@this, what, comparer);

    /// <summary>
    /// Checks if the string starts with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWithAny(this string @this, params string[] values) => StartsWithAny(@this, (IEnumerable<string>)values);

    /// <summary>
    /// Checks if the string starts with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWithAny(this string @this, StringComparison stringComparison, params string[] values) => StartsWithAny(@this, values, stringComparison);

    /// <summary>
    /// Checks if the string starts with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(values != null);
#endif
      return values.Any(s => @this.StartsWith(s, stringComparison));
    }

    /// <summary>
    /// Checks if the string ends with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWithAny(this string @this, params string[] values) => EndsWithAny(@this, (IEnumerable<string>)values);

    /// <summary>
    /// Checks if the string ends with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <param name="values">The values to compare to.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWithAny(this string @this, StringComparison stringComparison, params string[] values) => EndsWithAny(@this, values, stringComparison);

    /// <summary>
    /// Checks if the string ends with any from the given list.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="values">The values to compare to.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns><c>true</c> if there is at least one string in the list that matches the start; otherwise, <c>false</c>.</returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(values != null);
#endif
      return values.Any(s => @this.EndsWith(s, stringComparison));
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
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsWith(this string This, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return This.Length > 0 && string.Equals(This[0] + string.Empty, value + string.Empty, stringComparison);
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
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsWith(this string This, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return This.Length > 0 && string.Equals(This[This.Length - 1] + string.Empty, value + string.Empty, stringComparison);
    }

    /// <summary>
    /// Checks whether the given string starts not with the specified text.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool StartsNotWith(this string This, string value, StringComparison stringComparison = StringComparison.CurrentCulture) => !This.StartsWith(value, stringComparison);

    /// <summary>
    /// Checks whether the given string ends not with the specified text.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool EndsNotWith(this string This, string value, StringComparison stringComparison = StringComparison.CurrentCulture) => !This.EndsWith(value, stringComparison);

    /// <summary>
    /// Determines whether the given string is surrounded by another one.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="text">The text that should be around the given string.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>
    ///   <c>true</c> if the given string is surrounded by the given text; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsSurroundedWith(this string This, string text, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(text != null);
#endif
      return This.IsSurroundedWith(text, text, stringComparison);
    }

    /// <summary>
    /// Determines whether the given string is surrounded by two others.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="postfix">The postfix.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>
    ///   <c>true</c> if the given string is surrounded by the given text; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsSurroundedWith(this string This, string prefix, string postfix, StringComparison stringComparison = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(prefix != null);
      Contract.Requires(postfix != null);
#endif
      return This.StartsWith(prefix, stringComparison) && This.EndsWith(postfix, stringComparison);
    }

    /// <summary>
    /// Replaces a specified string at the start of another if possible.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="what">What to replace.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ReplaceAtStart(this string This, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
      if (This == null && what == null)
        return replacement;
      if (This == null || This.Length < what.Length)
        return This;
      return This.StartsWith(what, stringComparison) ? replacement + This.Substring(what.Length) : This;
    }

    /// <summary>
    /// Replaces a specified string at the end of another if possible.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="what">What to replace.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string ReplaceAtEnd(this string This, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
      if (This == null && what == null)
        return replacement;
      if (This == null || This.Length < what.Length)
        return This;
      return This.EndsWith(what, stringComparison) ? This.Substring(0, This.Length - what.Length) + replacement : This;
    }

#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if UNSAFE

    public static unsafe string TrimEnd(this string @this, string what) {
      if (@this == null || what == null)
        return @this;

      var count = what.Length;
      var index = @this.Length - count;

      // source shorter than search string, exit  
      if (index < 0 || count == 0)
        return @this;

      fixed (char* srcFix = @this)
      fixed (char* cmpFix = what) {
        var srcPtr = srcFix + index;
        var cmpPtr = cmpFix;
        while (true) {
          switch (count) {
            case 0:
              return @this.Substring(0, index);
            case 1:
              if (*srcPtr != *cmpPtr)
                return @this;

              goto case 0;
            case 2:
              if (*(int*)srcPtr != *(int*)cmpPtr)
                return @this;

              goto case 0;
            case 3:
              if (*(int*)srcPtr != *(int*)cmpPtr)
                return @this;
              if (srcPtr[2] != cmpPtr[2])
                return @this;

              goto case 0;
            case 4:
              if (*(long*)srcPtr != *(long*)cmpPtr)
                return @this;

              goto case 0;

#if PLATFORM_X86
            default:
              const int stepSize = 4;
              var cntSteps = count >> 2;
              count &= 3;
              do {
                if (*(int*)srcPtr != *(int*)cmpPtr)
                  return @this;
                if (((int*)srcPtr)[1] != ((int*)cmpPtr)[1])
                  return @this;

                srcPtr += stepSize;
                cmpPtr += stepSize;
              } while (--cntSteps > 0);
              break;
#else
            case 5:
              if (*(long*)srcPtr != *(long*)cmpPtr)
                return @this;
              if (srcPtr[4] != cmpPtr[4])
                return @this;

              goto case 0;
            case 6:
              if (*(long*)srcPtr != *(long*)cmpPtr)
                return @this;
              if (((int*)srcPtr)[2] != ((int*)cmpPtr)[2])
                return @this;

              goto case 0;
            case 7:
              if (*(long*)srcPtr != *(long*)cmpPtr)
                return @this;
              if (((int*)srcPtr)[2] != ((int*)cmpPtr)[2])
                return @this;
              if (srcPtr[6] != cmpPtr[6])
                return @this;

              goto case 0;
            case 8:
              if (*(long*)srcPtr != *(long*)cmpPtr)
                return @this;
              if (((long*)srcPtr)[1] != ((long*)cmpPtr)[1])
                return @this;

              goto case 0;
            default:
              const int stepSize = 8;
              var cntSteps = count >> 3;
              count &= 7;
              do {
                if (*(long*)srcPtr != *(long*)cmpPtr)
                  return @this;
                if (((long*)srcPtr)[1] != ((long*)cmpPtr)[1])
                  return @this;

                srcPtr += stepSize;
                cmpPtr += stepSize;
              } while (--cntSteps > 0);
              break;
#endif
          }
        }

      }
    }

#else

    public static string TrimEnd(this string @this, string what) {
      if (@this == null || what == null)
        return @this;

      var index = @this.Length - what.Length;

      // source shorter than search string, exit  
      if (index < 0)
        return @this;

      for (int j = index, i = 0; i < what.Length; ++j, ++i)
        if (@this[j] != what[i])
          return @this;

      return @this.Substring(0, index);
    }

#endif

    /// <summary>
    /// Determines whether the string is <c>null</c> or empty.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNullOrEmpty(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(false)] 
#endif
      this string @this) => string.IsNullOrEmpty(@this);

    /// <summary>
    /// Determines whether the string is not <c>null</c> or empty.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is not <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNotNullOrEmpty(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(true)] 
#endif
      this string @this) => !string.IsNullOrEmpty(@this);

    /// <summary>
    /// Determines whether the string is <c>null</c> or whitespace.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is <c>null</c> or whitespace; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_CONTRACTS
    [Pure]
    public static bool IsNullOrWhiteSpace(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(false)] 
#endif
    this string @this) => string.IsNullOrWhiteSpace(@this);
#else
    public static bool IsNullOrWhiteSpace(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(false)] 
#endif
      this string @this) => @this == null || @this.Trim().Length < 1;
#endif

    /// <summary>
    /// Determines whether the string is not <c>null</c> or whitespace.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <returns>
    ///   <c>true</c> if the string is not <c>null</c> or whitespace; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if SUPPORTS_CONTRACTS
    [Pure]
    public static bool IsNotNullOrWhiteSpace(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(true)] 
#endif
      this string @this) => !string.IsNullOrWhiteSpace(@this);
#else
    public static bool IsNotNullOrWhiteSpace(
#if SUPPORTS_NOT_NULL_WHEN_ATTRIBUTE
      [NotNullWhen(true)] 
#endif
      this string @this) => !IsNullOrWhiteSpace(@this);
#endif

    /// <summary>
    /// Determines whether a given string contains another one.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="other">The string to look for.</param>
    /// <param name="comparisonType">Type of the comparison.</param>
    /// <returns>
    ///   <c>true</c> if the other string is part of the given string; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool Contains(this string This, string other, StringComparison comparisonType) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      if (other == null)
        return false;

      return This.IndexOf(other, comparisonType) >= 0;
    }


#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool ContainsAny(
      this string This,
      params string[] other
      ) => ContainsAny(This, (IEnumerable<string>)other);

#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool ContainsAny(
      this string This,
      StringComparison comparisonType,
      params string[] other
      ) => ContainsAny(This, other, comparisonType);

    /// <summary>
    /// Checks whether the given string matches any of the provided
    /// </summary>
    /// <param name="this">This <see cref="String"/></param>
    /// <param name="needles">String to compare to</param>
    /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsAnyOf(this string @this, IEnumerable<string> needles) {
      if (needles == null)
        throw new ArgumentNullException(nameof(needles));

      return needles.Any(n => n == @this);
    }

    /// <summary>
    /// Checks whether the given string matches any of the provided
    /// </summary>
    /// <param name="this">This <see cref="String"/></param>
    /// <param name="needles">String to compare to</param>
    /// <param name="comparison">The comparison mode</param>
    /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsAnyOf(this string @this, IEnumerable<string> needles,StringComparison comparison) {
      if (needles == null)
        throw new ArgumentNullException(nameof(needles));

      return needles.Any(n => string.Equals(n, @this, comparison));
    }

    /// <summary>
    /// Determines whether a given string contains one of others.
    /// </summary>
    /// <param name="This">This string.</param>
    /// <param name="other">The strings to look for.</param>
    /// <param name="comparisonType">Type of the comparison.</param>
    /// <returns>
    ///   <c>true</c> if any of the other strings is part of the given string; otherwise, <c>false</c>.
    /// </returns>
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool ContainsAny(this string This, IEnumerable<string> other, StringComparison comparisonType = StringComparison.CurrentCulture) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      return other.Any(item => This.Contains(item, comparisonType));
    }

    /// <summary>
    /// Returns a default value if the given string is <c>null</c> or empty.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The given string or the given default value.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string DefaultIfNullOrEmpty(this string This, string defaultValue = null) => This.IsNullOrEmpty() ? defaultValue : This;

    /// <summary>
    /// Returns a default value if the given string is <c>null</c> or whitespace.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The given string or the given default value.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static string DefaultIfNullOrWhiteSpace(this string @this, Func<string> defaultValue) {
      if (defaultValue == null)
        throw new ArgumentNullException(nameof(defaultValue));

      return @this.IsNullOrWhiteSpace() ? defaultValue() : @this;
    }

    /// <summary>
    /// Returns a default value if the given string is <c>null</c> or whitespace.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="defaultValue">The default value; optional, defaults to <c>null</c>.</param>
    /// <returns>The given string or the given default value.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string DefaultIfNullOrWhiteSpace(this string @this, string defaultValue = null) => @this.IsNullOrWhiteSpace() ? defaultValue : @this;

    /// <summary>
    /// Breaks the given string down into lines.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="delimiter">The delimiter, <c>null</c> means try autodetecting the line break.</param>
    /// <param name="count">The maximum number of lines to support, 0 means unlimited.</param>
    /// <param name="options">The options.</param>
    /// <returns>The lines which where identified.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static string[] Lines(this string This, string delimiter = null, int count = 0, StringSplitOptions options = StringSplitOptions.None) {
      if (This == null)
        return null;

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
        return new[] { This };

      var result = count < 1 ? This.Split(new[] { delimiter }, options) : This.Split(new[] { delimiter }, count, options);
      return result;
    }

    /// <summary>
    /// Splits the given string respecting single and double quotes and allows for escape seququences to be used in these strings.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="delimiter">The delimiter to use.</param>
    /// <param name="escapeSequence">The escape sequence.</param>
    /// <param name="options">The options.</param>
    /// <returns>
    /// A sequence containing the parts of the string.
    /// </returns>
    public static IEnumerable<string> QuotedSplit(this string @this, string delimiter = ",", string escapeSequence = "\\", StringSplitOptions options = StringSplitOptions.None) {
      if (@this == null)
        yield break;

      if (delimiter.IsNullOrEmpty()) {
        yield return @this;
        yield break;
      }

      if (escapeSequence == "")
        escapeSequence = null;

      var length = @this.Length;
      var pos = 0;
      var currentlyEscaping = false;
      var currentlyInSingleQuote = false;
      var currentlyInDoubleQuote = false;
      var currentPart = new StringBuilder();

      while (pos < length) {
        var chr = @this[pos++];

        if (currentlyEscaping) {
          currentPart.Append(chr);
          currentlyEscaping = false;
        } else if (currentlyInSingleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length;
          } else if (chr == '\'')
            currentlyInSingleQuote = false;
          else
            currentPart.Append(chr);
        } else if (currentlyInDoubleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length;
          } else if (chr == '"')
            currentlyInDoubleQuote = false;
          else
            currentPart.Append(chr);
        } else if (delimiter.StartsWith(chr) && @this.Substring(pos - 1, delimiter.Length) == delimiter) {

          if (options == StringSplitOptions.None || currentPart.Length > 0)
            yield return currentPart.ToString();

          currentPart.Clear();
          pos += delimiter.Length - 1;
        } else if (/*currentPart.Length == 0 &&*/ chr == '\'') {
          currentlyInSingleQuote = true;
        } else if (/*currentPart.Length == 0 &&*/ chr == '"') {
          currentlyInDoubleQuote = true;
        } else if (chr == ' ') {

        } else {
          currentPart.Append(chr);
        }

      }

      if (options == StringSplitOptions.None || currentPart.Length > 0)
        yield return currentPart.ToString();
    }

    /// <summary>
    /// Splits the given string respecting single and double quotes and allows for escape seququences to be used in these strings.
    /// </summary>
    /// <param name="this">This String.</param>
    /// <param name="delimiters">The delimiters.</param>
    /// <param name="escapeSequence">The escape sequence.</param>
    /// <param name="options">The options.</param>
    /// <returns>
    /// A sequence containing the parts of the string.
    /// </returns>
    public static IEnumerable<string> QuotedSplit(this string @this, char[] delimiters, string escapeSequence = "\\", StringSplitOptions options = StringSplitOptions.None) {
      if (@this == null)
        yield break;

      if (delimiters == null || delimiters.Length < 1) {
        yield return @this;
        yield break;
      }

      if (escapeSequence == "")
        escapeSequence = null;

      var length = @this.Length;
      var pos = 0;
      var currentlyEscaping = false;
      var currentlyInSingleQuote = false;
      var currentlyInDoubleQuote = false;
      var currentPart = new StringBuilder();

      while (pos < length) {
        var chr = @this[pos++];

        if (currentlyEscaping) {
          currentPart.Append(chr);
          currentlyEscaping = false;
        } else if (currentlyInSingleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length - 1;
          } else if (chr == '\'')
            currentlyInSingleQuote = false;
          else
            currentPart.Append(chr);
        } else if (currentlyInDoubleQuote) {
          if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
            currentlyEscaping = true;
            pos += escapeSequence.Length - 1;
          } else if (chr == '"')
            currentlyInDoubleQuote = false;
          else
            currentPart.Append(chr);
        } else if (delimiters.Any(i => i == chr) && delimiters.Any(i => @this[pos - 1] == i)) {

          if (options == StringSplitOptions.None || currentPart.Length > 0)
            yield return currentPart.ToString();

          currentPart.Clear();
        } else if (/*currentPart.Length == 0 &&*/ chr == '\'') {
          currentlyInSingleQuote = true;
        } else if (/*currentPart.Length == 0 &&*/ chr == '"') {
          currentlyInDoubleQuote = true;
        } else if (chr == ' ') {

        } else {
          currentPart.Append(chr);
        }

      }

      if (options == StringSplitOptions.None || currentPart.Length > 0)
        yield return currentPart.ToString();
    }

#region nested HostEndPoint
    /// <summary>
    /// A host endpoint with port.
    /// </summary>
    public class HostEndPoint {
      public HostEndPoint(string host, int port) {
        this.Host = host;
        this.Port = port;
      }

      /// <summary>
      /// Gets the host.
      /// </summary>
      public string Host { get; }

      /// <summary>
      /// Gets the port.
      /// </summary>
      public int Port { get; }

      public static explicit operator IPEndPoint(HostEndPoint This) => new IPEndPoint(Dns.GetHostEntry(This.Host).AddressList[0], This.Port);
    }
#endregion

    /// <summary>
    /// Parses the host and port from a given string.
    /// </summary>
    /// <param name="This">This String, e.g. 172.17.4.3:http .</param>
    /// <returns>Port and host, <c>null</c> on error during parsing.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static HostEndPoint ParseHostAndPort(this string This) {
      if (This.IsNullOrWhiteSpace())
        return null;

      var host = This;
      ushort port = 0;
      var index = host.IndexOf(':');
      if (index < 0)
        return new HostEndPoint(host, port);

      var portText = host.Substring(index + 1);
      host = host.Left(index);
      if (!ushort.TryParse(portText, out port) && !_OFFICIAL_PORT_NAMES.TryGetValue(portText.Trim().ToLower(), out port))
        return null;
      return new HostEndPoint(host, port);
    }

    /// <summary>
    /// Replaces any of the given characters in the string.
    /// </summary>
    /// <param name="This">This String.</param>
    /// <param name="chars">The chars to replace.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The new string with replacements done.</returns>
#if SUPPORTS_CONTRACTS
    [Pure]
#endif
    public static string ReplaceAnyOf(this string This, string chars, string replacement) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(chars != null);
#endif
      return string.Join(
        string.Empty, (
          from c in This
          select chars.IndexOf(c) >= 0 ? replacement ?? string.Empty : c.ToString()
        ).ToArray()
      );
    }

    /// <summary>
    /// Returns all characters left of a certain string.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <param name="pattern">The pattern to find.</param>
    /// <param name="comparison">The comparison mode.</param>
    /// <returns>All characters left to the given text or the original string if text was not found.</returns>
    public static string LeftUntil(this string @this, string pattern, StringComparison comparison = StringComparison.CurrentCulture) {
      if (@this == null)
        return null;

      var index = @this.IndexOf(pattern, comparison);
      return index < 0 ? @this : @this.Substring(0, index);
    }

    /// <summary>
    /// Counts the number of lines in the given text.
    /// </summary>
    /// <param name="this">This string.</param>
    /// <returns>The number of lines.</returns>
    public static long LineCount(this string @this) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      var crlf = @this.Split(new[] { "\r\n" }, StringSplitOptions.None);
      var lfcr = string.Join("\n\r", crlf).Split(new[] { "\n\r" }, StringSplitOptions.None);
      var cr = string.Join("\n", lfcr).Split(new[] { "\n" }, StringSplitOptions.None);
      var lf = string.Join("\r", cr).Split(new[] { "\r" }, StringSplitOptions.None);
      return lf.Length;
    }

    private static readonly Regex _SQL_LIKE_ESCAPING = new(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", RegexOptions.Compiled);

    /// <summary>
    /// Equivalent to SQL LIKE Statement.
    /// </summary>
    /// <param name="this">The text to search.</param>
    /// <param name="toFind">The text to find.</param>
    /// <returns>True if the LIKE matched.</returns>
    public static bool Like(this string @this, string toFind)
      => new Regex(@"\A" 
                   + _SQL_LIKE_ESCAPING
                     .Replace(toFind, ch => @"\" + ch)
                     .Replace('_', '.')
                     .Replace("%", ".*") + @"\z", RegexOptions.Singleline)
        .IsMatch(@this)
    ;

    /// <summary>
    /// Converts a string to a <a href="https://en.wikipedia.org/wiki/Quoted-printable">quoted-printable</a> version of it.
    /// </summary>
    /// <param name="this">The string to convert</param>
    /// <returns>The quoted-printable string</returns>
    public static string ToQuotedPrintable(this string @this) {
      if (string.IsNullOrEmpty(@this)) 
        return @this;

      // see https://github.com/dotnet/runtime/blob/v5.0.3/src/libraries/Common/src/System/HexConverter.cs for the inner workings
#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      void ByteToHex2(byte value, out char highNibble, out char lowNibble) {
        var temp = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 0x8989);
        temp = (uint)((int)((uint)(-(int)temp & 0x7070) >> 4) + (int)temp + 0xB9B9);
        lowNibble = (char)(temp & 0xff);
        highNibble = (char)(temp >> 8);
      }
      
      var bytes = Encoding.UTF8.GetBytes(@this);
      var result = new StringBuilder(bytes.Length * 2);
      foreach(var ch in bytes)
        if (ch < 32 || ch > 126 || ch == '=') {
          ByteToHex2(ch,out var high,out var low);
          result.Append('=');
          result.Append(high);
          result.Append(low);
        } else
          result.Append((char)ch);

      return result.ToString();
    }

    private static readonly byte[] _CHAR_TO_HEX_LOOKUP={
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0x0,  0x1,  0x2,  0x3,  0x4,  0x5,  0x6,  0x7,  0x8,  0x9,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xA,  0xB,  0xC,  0xD,  0xE,  0xF,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xa,  0xb,  0xc,  0xd,  0xe,  0xf,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF  
    };

    /// <summary>
    /// Converts a <a href="https://en.wikipedia.org/wiki/Quoted-printable">quoted-printable</a> string to a normal version of it.
    /// </summary>
    /// <param name="this">The string to convert</param>
    /// <returns>The non-quoted-printable string</returns>

    public static string FromQuotedPrintable(this string @this) {
      if (string.IsNullOrEmpty(@this))
        return @this;

#if SUPPORTS_INLINING
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      int Hex2Short(char highNibble, char lowNibble) {
        var high = _CHAR_TO_HEX_LOOKUP[highNibble];
        var low = _CHAR_TO_HEX_LOOKUP[lowNibble];
        if ((high | low) == 0xff)
          Throw(highNibble, lowNibble);

        return high << 4 | low;
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      void Throw(char highNibble, char lowNibble) => throw new ArgumentOutOfRangeException($"'{highNibble}{lowNibble}' is not a hexadecimal digit");

      var bytes = new List<byte>(@this.Length);
      for (var index = 0; index < @this.Length; ++index)
        if (@this[index] == '=')
          bytes.Add((byte)Hex2Short(@this[++index], @this[++index]));
        else
          bytes.Add((byte)@this[index]);

      return Encoding.UTF8.GetString(bytes.ToArray());
    }

  }
}