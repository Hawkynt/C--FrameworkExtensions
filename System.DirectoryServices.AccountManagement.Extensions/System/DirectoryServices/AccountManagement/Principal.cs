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

using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace System.DirectoryServices.AccountManagement;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static partial class PrincipalExtensions {

  /// <summary>
  /// Resolves one or more <see cref="UserPrincipal"/> instances for the specified <paramref name="samAccountName"/>,
  /// handling both individual users and security groups.
  /// </summary>
  /// <param name="samAccountName">The SAM account name to resolve. May represent a user or a group.</param>
  /// <param name="allowCached">
  /// (Optional: defaults to <see langword="false"/>)  
  /// If <see langword="true"/>, allows using cached lookups for improved performance.
  /// </param>
  /// <returns>
  /// An <see cref="IEnumerable{T}"/> of <see cref="UserPrincipal"/>s:
  /// - If the account is a group, returns all its resolved members.  
  /// - If the account is a user, returns a singleton collection with that user.  
  /// - Returns an empty sequence if no match is found.
  /// </returns>
  /// <example>
  /// <code>
  /// foreach (var user in ResolveUserForSamAccountName("dev-team", allowCached: true))
  ///   Console.WriteLine(user.EmailAddress);
  /// </code>
  /// </example>
  public static IEnumerable<UserPrincipal> ResolveUsersForSamAccountName(string samAccountName, bool allowCached = false) {
    if (GroupPrincipalExtensions.TryGetGroupFromSamAccountName(samAccountName, out var group))
      return group.GetAllMembers(allowCached);

    var user = UserPrincipalExtensions.FindDomainUserBySamAccountName(samAccountName, allowCached);
    return user == null ? [] : new[] { user };
  }

}