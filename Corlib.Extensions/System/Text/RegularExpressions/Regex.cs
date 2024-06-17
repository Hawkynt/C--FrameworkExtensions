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
}
