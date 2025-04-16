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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace System.DirectoryServices.AccountManagement;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static partial class GroupPrincipalExtensions {
 
  private static readonly ConcurrentDictionary<string, CachedEnumeration<UserPrincipal>> _GROUP_MEMBER_CACHE = [];

  /// <summary>
  /// Retrieves all <see cref="UserPrincipal"/> members of the specified <see cref="GroupPrincipal"/>,  
  /// including members of nested groups.
  /// </summary>
  /// <param name="this">The <see cref="GroupPrincipal"/> whose full membership should be resolved.</param>
  /// <param name="allowCached">
  /// (Optional: defaults to <see langword="false"/>)  
  /// If <see langword="true"/>, cached group membership data may be used to improve performance.
  /// </param>
  /// <returns>
  /// An <see cref="IEnumerable{T}"/> of <see cref="UserPrincipal"/> objects representing all direct and indirect members of the group.
  /// </returns>
  /// <remarks>
  /// This method performs recursive expansion of nested group memberships while avoiding cycles using a
  /// case-insensitive hash set of visited SAM account names.
  /// </remarks>
  /// <example>
  /// <code>
  /// using var context = new PrincipalContext(ContextType.Domain);
  /// var group = GroupPrincipal.FindByIdentity(context, "Developers");
  /// foreach (var member in group.GetAllMembers())
  ///   Console.WriteLine(member.SamAccountName);
  /// </code>
  /// </example>
  public static IEnumerable<UserPrincipal> GetAllMembers(this GroupPrincipal @this, bool allowCached = false) {
    var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    return GroupPrincipalExtensions._ResolveUserForSamAccountNameInternal(@this.SamAccountName, visited, allowCached);
  }

  private static IEnumerable<UserPrincipal> _ResolveUserForSamAccountNameInternal(string samAccountName, HashSet<string> visited, bool allowCached) {
    if (samAccountName.IsNullOrEmpty())
      return [];

    if (allowCached && GroupPrincipalExtensions._GROUP_MEMBER_CACHE.TryGetValue(samAccountName, out var cachedResult))
      return cachedResult;

    if (!visited.Add(samAccountName))
      return [];

    if (GroupPrincipalExtensions.TryGetGroupFromSamAccountName(samAccountName, out var group)) {
      var allMembers = group
        .Members.SelectMany(m => GroupPrincipalExtensions._ResolveUserForSamAccountNameInternal(m.SamAccountName, visited, allowCached))
        .Distinct(new UserPrincipalEqualityComparer())
        .ToCache();

      GroupPrincipalExtensions._GROUP_MEMBER_CACHE[samAccountName] = allMembers;
      return allMembers;
    }

    var userPrincipal = UserPrincipalExtensions.FindDomainUserBySamAccountName(samAccountName);
    if (userPrincipal != null) {
      var singleUserArray = new[] { userPrincipal };
      GroupPrincipalExtensions._GROUP_MEMBER_CACHE[samAccountName] = singleUserArray.ToCache();
      return singleUserArray;
    }

    GroupPrincipalExtensions._GROUP_MEMBER_CACHE[samAccountName] = new UserPrincipal[0].ToCache();
    return [];
  }

  /// <summary>
  /// Attempts to resolve a <see cref="GroupPrincipal"/> from the specified <paramref name="samAccountName"/>.
  /// </summary>
  /// <param name="samAccountName">The SAM account name to look up.</param>
  /// <param name="group">
  /// When this method returns, contains the resolved <see cref="GroupPrincipal"/> if the lookup succeeded;
  /// otherwise <see langword="null"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if a group was found with the specified <paramref name="samAccountName"/>;  
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// if (TryGetGroupFromSamAccountName("Admins", out var group))
  ///   Console.WriteLine(group.DistinguishedName);
  /// </code>
  /// </example>
  public static bool TryGetGroupFromSamAccountName(string samAccountName, out GroupPrincipal group) {
    var principal = Principal.FindByIdentity(new(ContextType.Domain), IdentityType.SamAccountName, samAccountName);

    if (principal is GroupPrincipal groupPrincipal) {
      group = groupPrincipal;
      return true;
    }

    group = null!;
    return false;
  }

  private sealed class UserPrincipalEqualityComparer : IEqualityComparer<UserPrincipal> {
    public bool Equals(UserPrincipal x, UserPrincipal y) {
      if (ReferenceEquals(x, y))
        return true;

      if (x is null || y is null)
        return false;

      return x.SamAccountName == y.SamAccountName;
    }

    public int GetHashCode(UserPrincipal obj)
      => obj.SamAccountName?.GetHashCode() ?? 0;
  }

}