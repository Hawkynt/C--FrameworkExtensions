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
 
  private static readonly ConcurrentDictionary<string, CachedEnumeration<UserPrincipal>> _GROUP_MEMBER_CACHE = new(StringComparer.OrdinalIgnoreCase);

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
    if (allowCached && GroupPrincipalExtensions._GROUP_MEMBER_CACHE.TryGetValue(@this.SamAccountName, out var cachedResult))
      return cachedResult;

    return GroupPrincipalExtensions._GROUP_MEMBER_CACHE[@this.SamAccountName] = @this
      .Members
      .SelectMany(m => m switch {
        UserPrincipal up => [up],
        GroupPrincipal gp => GetAllMembers(gp, allowCached),
        _ => []
      })
      .Distinct(u => u.SamAccountName.ToLowerInvariant())
      .ToCache()
    ;
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
    group = Principal.FindByIdentity(new(ContextType.Domain), IdentityType.SamAccountName, samAccountName) as GroupPrincipal;
    return group != null;
  }
  
}