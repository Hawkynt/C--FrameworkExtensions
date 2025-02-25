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
using System.Linq;
using Guard;

namespace System.Text.RegularExpressions;

public static partial class RegexExtensions {
  /// <summary>
  ///   Replaces groups with replacements.
  /// </summary>
  /// <param name="regex">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="replacement">The replacement.</param>
  /// <param name="groupName">Name of the group, defaults to <c>null</c>.</param>
  /// <param name="groupId">The group id, defaults to <c>null</c>.</param>
  /// <param name="matchCount">The max replaces, default to <c>null</c>.</param>
  /// <returns></returns>
  private static string _ReplaceGroup(Regex regex, string source, string replacement, string groupName, int? groupId, int? matchCount) {
    Against.ThisIsNull(regex);

    if (source == null)
      return null;

    var counter = 0;
    return regex.Replace(
      source,
      m => {
        if (matchCount != null && counter++ >= matchCount.Value)
          return m.Value;

        var result = m.Value;
        var group = groupName != null ? m.Groups[groupName] : m.Groups[groupId.GetValueOrDefault(0) + 1];
        result = result[..(group.Index - m.Index)] + (replacement ?? string.Empty) + result[(group.Index - m.Index + group.Length)..];

        return result;
      }
    );
  }

  /// <summary>
  ///   Replaces the first group in each match with the replacement.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="replacement">The replacement.</param>
  /// <returns>A new string with all groups replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, string replacement) {
    Against.ThisIsNull(@this);

    return _ReplaceGroup(@this, source, replacement, null, null, null);
  }

  /// <summary>
  ///   Replaces the first group in the first n matches.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="replacement">The replacement.</param>
  /// <param name="matchCount">The number of matches.</param>
  /// <returns>A new string with the first n matches replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, string replacement, int matchCount) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(matchCount);

    return _ReplaceGroup(@this, source, replacement, null, null, matchCount);
  }

  /// <summary>
  ///   Replaces the group with the given index.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="groupId">The group id.</param>
  /// <param name="replacement">The replacement.</param>
  /// <returns>A new string with all groups with the given index replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, int groupId, string replacement) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(groupId);

    return _ReplaceGroup(@this, source, replacement, null, groupId, null);
  }

  /// <summary>
  ///   Replaces the group with given index n times.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="groupId">The group id.</param>
  /// <param name="replacement">The replacement.</param>
  /// <param name="matchCount">The number of matches.</param>
  /// <returns>A new string with the firstn n groups with the given index replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, int groupId, string replacement, int matchCount) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(groupId);
    Against.CountBelowZero(matchCount);

    return _ReplaceGroup(@this, source, replacement, null, groupId, matchCount);
  }

  /// <summary>
  ///   Replaces the group with the given name.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="groupName">The group name.</param>
  /// <param name="replacement">The replacement.</param>
  /// <returns>A new string with all groups with the given index replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, string groupName, string replacement) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(groupName);

    return _ReplaceGroup(@this, source, replacement, groupName, null, null);
  }

  /// <summary>
  ///   Replaces the group with given name n times.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <param name="source">The source string.</param>
  /// <param name="groupName">The group name.</param>
  /// <param name="replacement">The replacement.</param>
  /// <param name="matchCount">The number of matches.</param>
  /// <returns>A new string with the firstn n groups with the given index replaced.</returns>
  public static string ReplaceGroup(this Regex @this, string source, string groupName, string replacement, int matchCount) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(groupName);
    Against.CountBelowZero(matchCount);

    return _ReplaceGroup(@this, source, replacement, groupName, null, matchCount);
  }

  /// <summary>
  ///   Gets the named groups.
  /// </summary>
  /// <param name="this">This Regex.</param>
  /// <returns>A dictionary with all named groups and their indexes.</returns>
  public static Dictionary<int, string> GetNamedGroups(this Regex @this) {
    Against.ThisIsNull(@this);

    var groupNames = @this.GetGroupNames();
    var groupNumbers = @this.GetGroupNumbers();
    return Enumerable.Range(0, Math.Min(groupNames.Length, groupNumbers.Length)).Where(i => groupNames[i] != groupNumbers[i].ToString("0")).ToDictionary(i => groupNumbers[i], i => groupNames[i]);
  }

  /// <summary>
  /// Combines two <see cref="Regex"/> patterns into a single regex that matches either pattern, using the specified <see cref="RegexOptions"/>.
  /// </summary>
  /// <param name="this">The first regex pattern.</param>
  /// <param name="other">The second regex pattern to fuse with <paramref name="this"/>.</param>
  /// <param name="options">The <see cref="RegexOptions"/> to apply to the resulting regex.</param>
  /// <returns>
  /// A new <see cref="Regex"/> instance that matches either the pattern from <paramref name="this"/> or <paramref name="other"/>, 
  /// with the specified <paramref name="options"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="other"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// Regex regex1 = new Regex(@"\d+");
  /// Regex regex2 = new Regex(@"[A-Z]+");
  /// Regex fused = regex1.Or(regex2, RegexOptions.IgnoreCase);
  /// Console.WriteLine(fused.IsMatch("123"));   // Output: True
  /// Console.WriteLine(fused.IsMatch("abc"));   // Output: True
  /// Console.WriteLine(fused.IsMatch("!@#"));   // Output: False
  /// </code>
  /// </example>
  public static Regex Or(this Regex @this, Regex other, RegexOptions options) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);
    
    var target = $"(?:{@this})|(?:{other})";
    var result = new Regex(target, options);
    return result;
  }

  /// <summary>
  /// Combines two <see cref="Regex"/> patterns into a single regex that matches either pattern.
  /// </summary>
  /// <param name="this">The first regex pattern. Must have compatible options with <paramref name="other"/>.</param>
  /// <param name="other">The second regex pattern to fuse with <paramref name="this"/>. Must have compatible options.</param>
  /// <param name="compileRegex">(Optional: defaults to <see langword="false"/>)
  /// If set to <see langword="true"/>, the resulting regex will be compiled for faster execution.</param>
  /// <returns>
  /// A new <see cref="Regex"/> instance that matches either the pattern from <paramref name="this"/> or <paramref name="other"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="other"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown if <paramref name="this"/> and <paramref name="other"/> have incompatible <see cref="RegexOptions"/> settings.
  /// </exception>
  /// <remarks>
  /// - The resulting regex retains shared compatible options from both input patterns.
  /// - If the options differ, explicit inline modifiers are used to preserve behavior.
  /// </remarks>
  /// <example>
  /// <code>
  /// Regex regex1 = new Regex(@"\d+");
  /// Regex regex2 = new Regex(@"[a-z]+", RegexOptions.IgnoreCase);
  /// Regex fused = regex1.Or(regex2);
  /// Console.WriteLine(fused.IsMatch("123"));   // Output: True
  /// Console.WriteLine(fused.IsMatch("aBc"));   // Output: True
  /// Console.WriteLine(fused.IsMatch("!@#"));   // Output: False
  /// </code>
  /// </example>
  public static Regex Or(this Regex @this, Regex other, bool compileRegex = false) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);
    Against.ValuesAreNotEqual(@this.Options & RegexOptions.RightToLeft, other.Options & RegexOptions.RightToLeft);
    Against.ValuesAreNotEqual(@this.Options & RegexOptions.CultureInvariant, other.Options & RegexOptions.CultureInvariant);
    Against.ValuesAreNotEqual(@this.Options & RegexOptions.ECMAScript, other.Options & RegexOptions.ECMAScript);

    RegexOptions options;
    string target;
    if (@this.Options == other.Options) {
      options = @this.Options;
      target = $"(?:{@this})|(?:{other})";
    } else {
      options = @this.Options & (RegexOptions.RightToLeft | RegexOptions.CultureInvariant | RegexOptions.ECMAScript);
      target = $"(?{OptionsToModifiers(@this.Options)}:{@this})|(?{OptionsToModifiers(other.Options)}:{other})";
    }

    if (compileRegex)
      options |= RegexOptions.Compiled;

    var result = new Regex(target, options);
    return result;

    static StringBuilder OptionsToModifiers(RegexOptions options) {
      var results = new StringBuilder(5);
      if (options.HasFlag(RegexOptions.IgnoreCase))
        results.Append('i');
      if (options.HasFlag(RegexOptions.Multiline))
        results.Append('m');
      if (options.HasFlag(RegexOptions.Singleline))
        results.Append('s');
      if (options.HasFlag(RegexOptions.ExplicitCapture))
        results.Append('n');
      if (options.HasFlag(RegexOptions.IgnorePatternWhitespace))
        results.Append('x');
      return results;
    }
  }

  /// <summary>
  /// Creates a new <see cref="Regex"/> instance with the specified <see cref="RegexOptions"/>, preserving the pattern of the original regex.
  /// </summary>
  /// <param name="this">The original <see cref="Regex"/> instance. Must not be <see langword="null"/>.</param>
  /// <param name="options">The <see cref="RegexOptions"/> to apply to the new regex instance.</param>
  /// <returns>
  /// A new <see cref="Regex"/> instance with the same pattern as <paramref name="this"/>, but using the specified <paramref name="options"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// Regex original = new Regex(@"\w+");
  /// Regex modified = original.WithOptions(RegexOptions.IgnoreCase);
  /// Console.WriteLine(modified.IsMatch("HELLO")); // Output: True
  /// </code>
  /// </example>
  public static Regex WithOptions(this Regex @this, RegexOptions options) {
    Against.ThisIsNull(@this);

    return new(@this.ToString(), options);
  }

  /// <summary>
  /// Creates a new <see cref="Regex"/> instance with the same pattern and options as the original, but with <see cref="RegexOptions.Compiled"/> enabled.
  /// </summary>
  /// <param name="this">The original <see cref="Regex"/> instance. Must not be <see langword="null"/>.</param>
  /// <returns>
  /// A new <see cref="Regex"/> instance with <see cref="RegexOptions.Compiled"/> added to its options.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <remarks>
  /// Compiling a regex improves execution speed for frequent matches but increases JIT compilation time.
  /// Use <see cref="RegexOptions.Compiled"/> only for performance-critical, frequently executed expressions.
  /// </remarks>
  /// <example>
  /// <code>
  /// Regex original = new Regex(@"\d+");
  /// Regex compiled = original.Compile();
  /// Console.WriteLine(compiled.IsMatch("123")); // Output: True
  /// </code>
  /// </example>
  public static Regex Compile(this Regex @this) => WithOptions(@this, @this.Options | RegexOptions.Compiled);
  
}
