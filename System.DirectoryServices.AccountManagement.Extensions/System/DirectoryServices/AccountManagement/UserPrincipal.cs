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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace System.DirectoryServices.AccountManagement;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static partial class UserPrincipalExtensions {
  private static bool _FOS_IsNotNullOrWhiteSpace(this string text)
#if SUPPORTS_STRING_IS_NULL_OR_WHITESPACE
    => !string.IsNullOrWhiteSpace(text)
#else
    => text != null && text.Trim().Length > 0
#endif
  ;

  /// <summary>
  ///   Gets the full name.
  /// </summary>
  /// <param name="this">This UserPrincipal.</param>
  /// <param name="surnameFirst">if set to <c>true</c> surname goes first, separated by a comma.</param>
  /// <returns>
  ///   The full name.
  /// </returns>
  public static string GetFullName(this UserPrincipal @this, bool surnameFirst = false) {
    var result = new StringBuilder(128);

    if (@this.GivenName._FOS_IsNotNullOrWhiteSpace())
      result.Append(@this.GivenName);

    if (@this.MiddleName._FOS_IsNotNullOrWhiteSpace()) {
      if (result.Length > 0)
        result.Append(' ');

      result.Append(@this.MiddleName);
    }

    if (@this.Surname._FOS_IsNotNullOrWhiteSpace())
      switch (surnameFirst) {
        case true when result.Length > 0:
          result.Insert(0, ", ");
          result.Insert(0, @this.Surname);
          break;
        case true:
          result.Append(@this.Surname);
          break;
        case false when result.Length > 0:
          result.Append(' ');
          result.Append(@this.Surname);
          break;
        case false:
          result.Append(@this.Surname);
          break;
      }

    if (result.Length < 1)
      result.Append(@this.SamAccountName);

    return result.ToString();
  }

  public static MailAddress GetEmailAddress(this UserPrincipal @this) => new(@this.EmailAddress, GetFullName(@this), Encoding.UTF8);
  public static UserPrincipal FindDomainUserBySamAccountName(string samAccountName) => FindDomainUserBySamAccountName(samAccountName, false);

  private static readonly ConcurrentDictionary<string, UserPrincipal> _DOMAIN_USER_CACHE = new(StringComparer.OrdinalIgnoreCase);

  public static UserPrincipal FindDomainUserBySamAccountName(string samAccountName, bool allowCached) => allowCached ? _DOMAIN_USER_CACHE.GetOrAdd(samAccountName, _FindDomainUserBySamAccountName) : _FindDomainUserBySamAccountName(samAccountName);
  public static UserPrincipal FindFirstDomainUserByDisplayName(string fullName, bool allowCached = false) => allowCached ? _DOMAIN_USER_CACHE.GetOrAdd(fullName, _FindDomainUserByDisplayName) : _FindDomainUserByDisplayName(fullName);

  public static UserPrincipal FindFirstDomainUserByAnyName(string name, bool allowCached = false) {
    UserPrincipal FindDomainUserByName() => _FindDomainUserBySamAccountName(name) ?? _FindDomainUserByDisplayName(name) ?? _FindDomainUserBySurname(name);

    return allowCached ? _DOMAIN_USER_CACHE.GetOrAdd(name, _ => FindDomainUserByName()) : FindDomainUserByName();
  }

  private static UserPrincipal _FindDomainUserBySamAccountName(string samAccountName) {
    using var context = new PrincipalContext(ContextType.Domain);
    return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
  }

  private static UserPrincipal _FindDomainUserByDisplayName(string fullName) {
    using var context = new PrincipalContext(ContextType.Domain);
    using var searcher = new PrincipalSearcher();
    var user = new UserPrincipal(context) { DisplayName = string.Join(" ", fullName.Split(',').Select(s => s.Trim()).ToArray()) };
    searcher.QueryFilter = user;
    return (UserPrincipal)searcher.FindOne();
  }

  private static UserPrincipal _FindDomainUserBySurname(string surname) {
    using var context = new PrincipalContext(ContextType.Domain);
    using var searcher = new PrincipalSearcher();
    var user = new UserPrincipal(context) { Surname = surname };
    searcher.QueryFilter = user;
    return (UserPrincipal)searcher.FindOne();
  }

  #region LDAP stuff

  #region caching

  public interface ILDAPCache {
    int CacheEntryCount { get; set; }
  }

  private sealed class LDAPCache : ILDAPCache {
    /// <summary>
    ///   NOTE: Using struct to avoid excessive pressure on the garbage collector
    /// </summary>
    private readonly struct Entry(DirectoryEntry entry) {
      private readonly DateTime _lastAccessed = DateTime.UtcNow;
      public readonly DirectoryEntry item = entry;
      public TimeSpan Age => DateTime.UtcNow - this._lastAccessed;

      public Entry MarkUsage() => new(this.item);

      public void Destroy() {
        // if the item wasn't accessed in the last second, dispose immediately
        var directoryEntry = this.item;
        if (this.Age.TotalSeconds > 1) {
          directoryEntry?.Dispose();
          return;
        }

        // otherwise schedule disposal in 1sec.
        var action = () => {
          Thread.Sleep(1000);
          directoryEntry?.Dispose();
        };
        action.BeginInvoke(action.EndInvoke, null);
      }
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    /// <summary>
    ///   Gets or adds the given element to the cache.
    /// </summary>
    /// <param name="distinguishedName">Distinguished name of the element.</param>
    /// <returns></returns>
    public DirectoryEntry GetOrAdd(string distinguishedName) {
      var entry = this._entries.GetOrAdd(distinguishedName, _ => new(new("LDAP://" + distinguishedName)));

      // update usage
      this._entries[distinguishedName] = entry.MarkUsage();

      // remove excess elements
      this._CheckCache();

      return entry.item;
    }

    /// <summary>
    ///   Clears this cache entirely.
    ///   Note: Clearing while adding might leave elements in the cache.
    /// </summary>
    public void Clear() {
      foreach (var distinguishedName in this._entries.Keys.ToArray())
        this.Remove(distinguishedName);
    }

    /// <summary>
    ///   Removes the element with the given distinguished name.
    /// </summary>
    /// <param name="distinguishedName">Distinguished name of the element.</param>
    public void Remove(string distinguishedName) {
      this._entries.TryRemove(distinguishedName, out var result);

      // INFO: !worst-case destroying an item while another reference is still pointing at it during usage
      //       we're trying to work around this by waiting at most 1sec.
      result.Destroy();
    }

    /// <summary>
    ///   Clear unneeded elements from the cache.
    /// </summary>
    private void _CheckCache() {
      if (this._entries.Count <= this.CacheEntryCount)
        return;

      // find the least accessed n elements and remove them
      var distinguishedNames = this._entries.OrderByDescending(e => e.Value.Age).Select(e => e.Key).Take(this._entries.Count - this.CacheEntryCount).ToArray();
      foreach (var distinguishedName in distinguishedNames)
        this.Remove(distinguishedName);
    }

    #region Implementation of ILDAPCache

    public int CacheEntryCount { get; set; }

    #endregion
  }

  /// <summary>
  ///   This holds the last 512 used entries, because there is a chance that they will be heavily re-used.
  /// </summary>
  private static readonly LDAPCache _LDAP_CACHE = new() { CacheEntryCount = 512 };

  #endregion

  /// <summary>
  ///   LDAP Group properties, see http://www.selfadsi.de/group-attributes.htm
  /// </summary>
  public interface ILDAPGroup {
    string AdminDescription { get; }
    string AdminDisplayName { get; }
    string ADsPath { get; }
    string AuthOrig { get; }
    string AuthOrigBL { get; }
    string CanonicalName { get; }
    string Class { get; }
    string CN { get; }
    string CreateTimeStamp { get; }
    string DelivContLength { get; }
    string Description { get; }
    string DisplayName { get; }
    string DisplayNamePrintable { get; }
    string DistinguishedName { get; }
    string DLMemRejectPerms { get; }
    string DLMemRejectPermsBL { get; }
    string DLMemSubmitPerms { get; }
    string DLMemSubmitPermsBL { get; }
    string ExtensionAttribute { get; }
    string GroupType { get; }
    string HomeMTA { get; }
    string Info { get; }
    string IsDeleted { get; }
    string LegacyExchangeDN { get; }
    string Mail { get; }
    string MailNickName { get; }
    string ManagedBy { get; }
    string Member { get; }
    IEnumerable<ILDAPGroup> MemberOf { get; }
    string ModifyTimeStamp { get; }
    string MsExchExpansionServerName { get; }
    string MsExchHideFromAddressLists { get; }
    string MsExchHomeServerName { get; }
    string MsExchRequireAuthToSendTo { get; }
    string MsSFU30GidNumber { get; }
    string MsSFU30Name { get; }
    string MsSFU30NisDomain { get; }
    string MsSFU30PosixMember { get; }
    string Name { get; }
    string NTSecurityDescriptor { get; }
    string ObjectCategory { get; }
    string ObjectClass { get; }
    string ObjectGUID { get; }
    string ObjectSid { get; }
    string OOFReplyToOriginator { get; }
    string Parent { get; }
    string PrimaryGroupToken { get; }
    string ProxyAddresses { get; }
    string ReportToOriginator { get; }
    string ReportToOwner { get; }
    string SamAccountName { get; }
    string TelephoneNumber { get; }
    string TextEncodedORAddress { get; }
    string UnauthOrig { get; }
    string UnauthOrigBL { get; }
    string USNChanged { get; }
    string USNCreated { get; }
    string WhenChanged { get; }
    string WhenCreated { get; }
  }

  /// <summary>
  ///   Wraps an LDAP group.
  ///   Dtor tries to remove the associated cache elements; this is not needed but nice.
  ///   We don't care if elements are removed twice or removed but still used and stuff,
  ///   because the cache will re-created them if needed.
  /// </summary>
  private sealed class LDAPGroup(string distinguishedName) : ILDAPGroup {
    ~LDAPGroup() => _LDAP_CACHE.Remove(distinguishedName);

    private DirectoryEntry _GetEntry() => _LDAP_CACHE.GetOrAdd(distinguishedName);

    private string _ReadProperty(string propertyName) => this._GetEntry().Properties[propertyName].Cast<string>().FirstOrDefault();

    #region Implementation of ILDAPGroup

    public string AdminDescription => this._ReadProperty("adminDescription");
    public string AdminDisplayName => this._ReadProperty("adminDisplayName");
    public string ADsPath => this._ReadProperty("ADsPath");
    public string AuthOrig => this._ReadProperty("authOrig");
    public string AuthOrigBL => this._ReadProperty("authOrigBL");
    public string CanonicalName => this._ReadProperty("canonicalName");
    public string Class => this._ReadProperty("Class");
    public string CN => this._ReadProperty("cn");
    public string CreateTimeStamp => this._ReadProperty("createTimeStamp");
    public string DelivContLength => this._ReadProperty("delivContLength");
    public string Description => this._ReadProperty("description");
    public string DisplayName => this._ReadProperty("displayName");
    public string DisplayNamePrintable => this._ReadProperty("displayNamePrintable");
    public string DistinguishedName => this._ReadProperty("distinguishedName");
    public string DLMemRejectPerms => this._ReadProperty("dLMemRejectPerms");
    public string DLMemRejectPermsBL => this._ReadProperty("dLMemRejectPermsBL");
    public string DLMemSubmitPerms => this._ReadProperty("dLMemSubmitPerms");
    public string DLMemSubmitPermsBL => this._ReadProperty("dLMemSubmitPermsBL");
    public string ExtensionAttribute => this._ReadProperty("extensionAttribute");
    public string GroupType => this._ReadProperty("groupType");
    public string HomeMTA => this._ReadProperty("homeMTA");
    public string Info => this._ReadProperty("info");
    public string IsDeleted => this._ReadProperty("isDeleted");
    public string LegacyExchangeDN => this._ReadProperty("legacyExchangeDN");
    public string Mail => this._ReadProperty("mail");
    public string MailNickName => this._ReadProperty("mailNickName");
    public string ManagedBy => this._ReadProperty("managedBy");
    public string Member => this._ReadProperty("member");

    public IEnumerable<ILDAPGroup> MemberOf {
      get {
        var entry = this._GetEntry();
        foreach (string distinguishedName in entry.Properties["memberOf"])
          yield return new LDAPGroup(distinguishedName);
      }
    }

    public string ModifyTimeStamp => this._ReadProperty("modifyTimeStamp");
    public string MsExchExpansionServerName => this._ReadProperty("msExchExpansionServerName");
    public string MsExchHideFromAddressLists => this._ReadProperty("msExchHideFromAddressLists");
    public string MsExchHomeServerName => this._ReadProperty("msExchHomeServerName");
    public string MsExchRequireAuthToSendTo => this._ReadProperty("msExchRequireAuthToSendTo");
    public string MsSFU30GidNumber => this._ReadProperty("msSFU30GidNumber");
    public string MsSFU30Name => this._ReadProperty("msSFU30Name");
    public string MsSFU30NisDomain => this._ReadProperty("msSFU30NisDomain");
    public string MsSFU30PosixMember => this._ReadProperty("msSFU30PosixMember");
    public string Name => this._ReadProperty("Name");
    public string NTSecurityDescriptor => this._ReadProperty("nTSecurityDescriptor");
    public string ObjectCategory => this._ReadProperty("objectCategory");
    public string ObjectClass => this._ReadProperty("objectClass");
    public string ObjectGUID => this._ReadProperty("objectGUID");
    public string ObjectSid => this._ReadProperty("objectSid");
    public string OOFReplyToOriginator => this._ReadProperty("oOFReplyToOriginator");
    public string Parent => this._ReadProperty("Parent");
    public string PrimaryGroupToken => this._ReadProperty("primaryGroupToken");
    public string ProxyAddresses => this._ReadProperty("proxyAddresses");
    public string ReportToOriginator => this._ReadProperty("reportToOriginator");
    public string ReportToOwner => this._ReadProperty("reportToOwner");
    public string SamAccountName => this._ReadProperty("sAMAccountName");
    public string TelephoneNumber => this._ReadProperty("telephoneNumber");
    public string TextEncodedORAddress => this._ReadProperty("textEncodedORAddress");
    public string UnauthOrig => this._ReadProperty("unauthOrig");
    public string UnauthOrigBL => this._ReadProperty("unauthOrigBL");
    public string USNChanged => this._ReadProperty("uSNChanged");
    public string USNCreated => this._ReadProperty("uSNCreated");
    public string WhenChanged => this._ReadProperty("whenChanged");
    public string WhenCreated => this._ReadProperty("whenCreated");

    #endregion
  }

  public static IEnumerable<ILDAPGroup> GetLDAPGroups(this UserPrincipal @this) {
    var userEntry = _LDAP_CACHE.GetOrAdd(@this.DistinguishedName);
    foreach (string distinguishedName in userEntry.Properties["memberOf"])
      yield return new LDAPGroup(distinguishedName);
  }

  public static IEnumerable<ILDAPGroup> GetAllLDAPGroups(this UserPrincipal @this) {
    var alreadyReturned = new HashSet<string>();
    var stack = new Stack<IEnumerable<ILDAPGroup>>();
    stack.Push(GetLDAPGroups(@this));
    while (stack.Count > 0) {
      var currentEnumeration = stack.Pop();
      foreach (var group in currentEnumeration) {
        var distinguishedName = group.DistinguishedName;
        if (!alreadyReturned.Add(distinguishedName))
          continue;

        yield return group;
        stack.Push(group.MemberOf);
      }
    }
  }

  #endregion
}
