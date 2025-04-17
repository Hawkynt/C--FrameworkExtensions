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

  private static readonly object _contextLock = new();
  private static PrincipalContext _sharedContext;

  /// <summary>
  /// Gets a shared domain context for Active Directory operations.
  /// If the context has been disposed, a new one is automatically created.
  /// </summary>
  internal static PrincipalContext SharedDomainContext {
    get {
      // Fast path - if context exists and isn't disposed, return it
      var result = _sharedContext;
      if (result != null)
        try {
          // Try to access a property to check if it's been disposed
          _ = result.ConnectedServer;
          return result;
        } catch (ObjectDisposedException) {
          // Context was disposed, we'll recreate it below
        }
      

      // Slow path - create a new context under lock
      lock (_contextLock) {
        // Check again in case another thread created it while we were waiting
        result = _sharedContext;
        if (result != null)
          try {
            _ = result.ConnectedServer;
            return result;
          } catch (ObjectDisposedException) {
            // Context was disposed, recreate it
          }
        
        _sharedContext = result = new(ContextType.Domain);
        return result;
      }
    }
  }

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
  public static IEnumerable<UserPrincipal> ResolveUsersForSamAccountName(string samAccountName, bool allowCached = false) 
    => GroupPrincipalExtensions.TryGetGroupFromSamAccountName(samAccountName, out var group) 
      ? group.GetAllMembers(allowCached) 
      : UserPrincipalExtensions.FindDomainUserBySamAccountName(samAccountName, allowCached) is { } up 
        ? [up] 
        : []
  ;
}