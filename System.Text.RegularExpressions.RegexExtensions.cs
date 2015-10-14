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
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Text.RegularExpressions {
  internal static partial class RegexExtensions {
    /// <summary>
    /// Replaces groups with replacements.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="groupName">Name of the group, defaults to <c>null</c>.</param>
    /// <param name="groupId">The group id, defaults to <c>null</c>.</param>
    /// <param name="matchCount">The max replaces, default to <c>null</c>.</param>
    /// <returns></returns>
    private static string _ReplaceGroup(this Regex regex, string source, string replacement, string groupName, int? groupId, int? matchCount) {
      Contract.Requires(regex != null);
      if (source == null)
        return (null);
      var counter = 0;
      return (regex.Replace(source, m => {
        if (matchCount != null && counter++ >= matchCount.Value)
          return (m.Value);
        var result = m.Value;
        var group = groupName != null ? m.Groups[groupName] : m.Groups[groupId.GetValueOrDefault(0) + 1];
        result = result.Substring(0, group.Index - m.Index) + (replacement ?? string.Empty) + result.Substring(group.Index - m.Index + group.Length);
        return (result);
      }));
    }

    /// <summary>
    /// Replaces the first group in each match with the replacement.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>A new string with all groups replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, string replacement) {
      Contract.Requires(regex != null);
      return (regex._ReplaceGroup(source, replacement, null, null, null));
    }

    /// <summary>
    /// Replaces the first group in the first n matches.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="matchCount">The number of matches.</param>
    /// <returns>A new string with the first n matches replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, string replacement, int matchCount) {
      Contract.Requires(regex != null);
      Contract.Requires(matchCount >= 0);
      return (regex._ReplaceGroup(source, replacement, null, null, matchCount));
    }

    /// <summary>
    /// Replaces the group with the given index.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="groupId">The group id.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>A new string with all groups with the given index replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, int groupId, string replacement) {
      Contract.Requires(regex != null);
      Contract.Requires(groupId >= 0);
      return (regex._ReplaceGroup(source, replacement, null, groupId, null));
    }

    /// <summary>
    /// Replaces the group with given index n times.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="groupId">The group id.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="matchCount">The number of matches.</param>
    /// <returns>A new string with the firstn n groups with the given index replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, int groupId, string replacement, int matchCount) {
      Contract.Requires(regex != null);
      Contract.Requires(groupId >= 0);
      Contract.Requires(matchCount >= 0);
      return (regex._ReplaceGroup(source, replacement, null, groupId, matchCount));
    }

    /// <summary>
    /// Replaces the group with the given name.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>A new string with all groups with the given index replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, string groupName, string replacement) {
      Contract.Requires(regex != null);
      Contract.Requires(groupName != null);
      return (regex._ReplaceGroup(source, replacement, groupName, null, null));
    }

    /// <summary>
    /// Replaces the group with given name n times.
    /// </summary>
    /// <param name="regex">This Regex.</param>
    /// <param name="source">The source string.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="matchCount">The number of matches.</param>
    /// <returns>A new string with the firstn n groups with the given index replaced.</returns>
    public static string ReplaceGroup(this Regex regex, string source, string groupName, string replacement, int matchCount) {
      Contract.Requires(regex != null);
      Contract.Requires(groupName != null);
      Contract.Requires(matchCount >= 0);
      return (regex._ReplaceGroup(source, replacement, groupName, null, matchCount));
    }

    /// <summary>
    /// Gets the named groups.
    /// </summary>
    /// <param name="This">This Regex.</param>
    /// <returns>A dictionary with all named groups and their indexes.</returns>
    public static Dictionary<int, string> GetNamedGroups(this Regex This) {
      Contract.Requires(This != null);
      var groupNames = This.GetGroupNames();
      var groupNumbers = This.GetGroupNumbers();
      return (Enumerable.Range(0, Math.Min(groupNames.Length, groupNumbers.Length)).Where(i => groupNames[i] != groupNumbers[i].ToString("0")).ToDictionary(i => groupNumbers[i], i => groupNames[i]));
    }
  }
}
