# Extensions to DirectoryServices

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/System.DirectoryServices.AccountManagement.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.DirectoryServices)](https://www.nuget.org/packages/FrameworkExtensions.DirectoryServices/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for Active Directory account management and Windows impersonation, part of [Hawkynt's .NET Framework Extensions](https://github.com/Hawkynt/C--FrameworkExtensions).

| Property              | Value                                                                      |
| --------------------- | -------------------------------------------------------------------------- |
| **Package ID**        | `FrameworkExtensions.DirectoryServices`                                    |
| **Target Frameworks** | .NET Framework 3.5/4.0/4.5/4.8, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                                          |

---

## Overview

This library provides extension methods for `GroupPrincipal`, `UserPrincipal`, and `Principal` that simplify common Active Directory operations such as resolving group memberships (including nested groups), looking up users by various identity attributes, querying LDAP group properties, and performing Windows user impersonation. Results are cached using thread-safe concurrent dictionaries for high-performance repeated lookups. On .NET 5+ all AD-related classes are annotated with `[SupportedOSPlatform("windows")]`.

---

## API Reference

### GroupPrincipal Extensions (`System.DirectoryServices.AccountManagement.GroupPrincipal`)

**Static class:** `GroupPrincipalExtensions`

| Method                          | Signature                                                                                    | Description                                                                                                                                                                                                                                         |
| ------------------------------- | -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetAllMembers`                 | `GetAllMembers(this GroupPrincipal, bool allowCached = false) -> IEnumerable<UserPrincipal>` | Recursively resolves all `UserPrincipal` members of a group, including nested group memberships. Uses a case-insensitive hash set of SAM account names to detect and avoid cycles. Results can be cached across calls when `allowCached` is `true`. |
| `TryGetGroupFromSamAccountName` | `TryGetGroupFromSamAccountName(string samAccountName, out GroupPrincipal group) -> bool`     | (Static) Attempts to find a `GroupPrincipal` by SAM account name in the shared domain context. Returns `true` if a group was found; writes the result to `group`.                                                                                   |

---

### Principal Extensions (`System.DirectoryServices.AccountManagement.Principal`)

**Static class:** `PrincipalExtensions`

| Method                          | Signature                                                                                                      | Description                                                                                                                                                                                                                                     |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ResolveUsersForSamAccountName` | `ResolveUsersForSamAccountName(string samAccountName, bool allowCached = false) -> IEnumerable<UserPrincipal>` | (Static) Resolves a SAM account name to one or more `UserPrincipal` instances. If the name identifies a group, returns all resolved group members. If it identifies a user, returns a singleton collection. Returns empty if no match is found. |

---

### UserPrincipal Extensions (`System.DirectoryServices.AccountManagement.UserPrincipal`)

**Static class:** `UserPrincipalExtensions`

#### User Name Methods

| Method            | Signature                                                              | Description                                                                                                                                                                                                                 |
| ----------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetFullName`     | `GetFullName(this UserPrincipal, bool surnameFirst = false) -> string` | Constructs a full name string from `GivenName`, `MiddleName`, and `Surname`. When `surnameFirst` is `true`, formats as `Surname, GivenName MiddleName`. Falls back to `SamAccountName` if no name components are available. |
| `GetEmailAddress` | `GetEmailAddress(this UserPrincipal) -> MailAddress`                   | Creates a `System.Net.Mail.MailAddress` from the user's `EmailAddress` property and full name, UTF-8 encoded.                                                                                                               |

#### User Lookup Methods

| Method                             | Signature                                                                                      | Description                                                                                                                                                   |
| ---------------------------------- | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `FindDomainUserBySamAccountName`   | `FindDomainUserBySamAccountName(string samAccountName) -> UserPrincipal`                       | (Static) Finds a domain user by SAM account name. Always performs a fresh directory lookup. Returns `null` if not found.                                      |
| `FindDomainUserBySamAccountName`   | `FindDomainUserBySamAccountName(string samAccountName, bool allowCached) -> UserPrincipal`     | (Static) Finds a domain user by SAM account name. When `allowCached` is `true`, uses a thread-safe `ConcurrentDictionary` cache to avoid repeated lookups.    |
| `FindFirstDomainUserByDisplayName` | `FindFirstDomainUserByDisplayName(string fullName, bool allowCached = false) -> UserPrincipal` | (Static) Finds the first user whose display name matches the specified full name (comma-separated parts are joined with spaces). Returns `null` if not found. |
| `FindFirstDomainUserByAnyName`     | `FindFirstDomainUserByAnyName(string name, bool allowCached = false) -> UserPrincipal`         | (Static) Searches for a user by SAM account name first, then display name, then surname, returning the first match. Returns `null` if not found.              |

#### LDAP Group Methods

| Method             | Signature                                                         | Description                                                                                                                                                          |
| ------------------ | ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetLDAPGroups`    | `GetLDAPGroups(this UserPrincipal) -> IEnumerable<ILDAPGroup>`    | Returns the direct LDAP group memberships of the user as `ILDAPGroup` instances. Uses an internal LDAP cache keyed by distinguished name.                            |
| `GetAllLDAPGroups` | `GetAllLDAPGroups(this UserPrincipal) -> IEnumerable<ILDAPGroup>` | Recursively returns all LDAP group memberships (direct and transitive), performing depth-first traversal with cycle detection via a hash set of distinguished names. |

---

### ILDAPGroup (interface)

**Namespace:** `System.DirectoryServices.AccountManagement`

Represents the full set of LDAP group attributes as defined in Active Directory schema. For attribute descriptions see [selfADSI: Group Attributes](http://www.selfadsi.de/group-attributes.htm).

| Category             | Properties                                                                                                                                                                                           |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Identity**         | `CN`, `Name`, `DistinguishedName`, `SamAccountName`, `ObjectGUID`, `ObjectSid`, `ObjectCategory`, `ObjectClass`                                                                                      |
| **Display**          | `DisplayName`, `DisplayNamePrintable`, `AdminDisplayName`, `AdminDescription`, `Description`                                                                                                         |
| **Exchange / Mail**  | `Mail`, `MailNickName`, `LegacyExchangeDN`, `MsExchExpansionServerName`, `MsExchHideFromAddressLists`, `MsExchHomeServerName`, `MsExchRequireAuthToSendTo`, `ProxyAddresses`, `TextEncodedORAddress` |
| **Group Membership** | `Member`, `MemberOf` (returns `IEnumerable<ILDAPGroup>`), `GroupType`, `PrimaryGroupToken`                                                                                                           |
| **Permissions**      | `AuthOrig`, `AuthOrigBL`, `DLMemRejectPerms`, `DLMemRejectPermsBL`, `DLMemSubmitPerms`, `DLMemSubmitPermsBL`, `UnauthOrig`, `UnauthOrigBL`                                                           |
| **Timestamps**       | `CreateTimeStamp`, `ModifyTimeStamp`, `WhenChanged`, `WhenCreated`, `USNChanged`, `USNCreated`, `IsDeleted`                                                                                          |
| **Directory**        | `ADsPath`, `CanonicalName`, `Class`, `Parent`, `HomeMTA`, `NTSecurityDescriptor`                                                                                                                     |
| **Management**       | `ManagedBy`, `Info`, `TelephoneNumber`, `DelivContLength`                                                                                                                                            |
| **Unix/SFU**         | `MsSFU30GidNumber`, `MsSFU30Name`, `MsSFU30NisDomain`, `MsSFU30PosixMember`                                                                                                                          |
| **Delivery**         | `OOFReplyToOriginator`, `ReportToOriginator`, `ReportToOwner`                                                                                                                                        |
| **Extension**        | `ExtensionAttribute`                                                                                                                                                                                 |

---

### ILDAPCache (interface)

**Namespace:** `System.DirectoryServices.AccountManagement`

Controls the LDAP directory entry cache behavior.

| Property                        | Description                                                                                                                                                            |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `CacheEntryCount { get; set; }` | Gets or sets the maximum number of `DirectoryEntry` instances retained in the cache. Default: 512. Least-recently-used entries are evicted when the limit is exceeded. |

---

### Impersonation (class)

**Namespace:** `System.Security`

**Availability:** .NET Framework only (excluded from .NET Core, .NET 5+, and .NET Standard)

An `IDisposable` class that performs Windows user impersonation via the Win32 `LogonUser` API and `WindowsIdentity.Impersonate`. The impersonation context is automatically reverted when the object is disposed.

#### Enumerations

| Enum            | Values                                                                                                                   | Description                                             |
| --------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------- |
| `LogonType`     | `Interactive (2)`, `Network (3)`, `Batch (4)`, `Service (5)`, `Unlock (7)`, `NetworkCleartext (8)`, `NewCredentials (9)` | Specifies the type of logon operation to perform.       |
| `LogonProvider` | `Default (0)`, `WinNT35 (1)`, `WinNT40_NTLM (2)`, `WinNT50 (3)`                                                          | Specifies the logon provider to use for authentication. |

#### Constructors

| Constructor                                                                                                                      | Description                                                                                                                                                                                               |
| -------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Impersonation(string username, string password, LogonType type = Interactive, LogonProvider provider = Default)`                | Impersonates the specified user in the current domain. Obtains the domain name automatically from `Domain.GetCurrentDomain()`.                                                                            |
| `Impersonation(string domain, string username, string password, LogonType type = Interactive, LogonProvider provider = Default)` | Impersonates the specified user in the given domain. Throws `InvalidCredentialException` for unknown username or bad password (Win32 error 1326). Throws `ApplicationException` for other logon failures. |

#### Methods

| Method      | Description                                                                                         |
| ----------- | --------------------------------------------------------------------------------------------------- |
| `Dispose()` | Reverts the impersonation context and releases the logon token handle. Safe to call multiple times. |

---

## Usage Examples

### Resolving all members of a group (including nested)

```csharp
using System.DirectoryServices.AccountManagement;

using var context = new PrincipalContext(ContextType.Domain);
var group = GroupPrincipal.FindByIdentity(context, "Developers");
foreach (var member in group.GetAllMembers(allowCached: true))
  Console.WriteLine(member.SamAccountName);
```

### Resolving users from a SAM account name (user or group)

```csharp
using System.DirectoryServices.AccountManagement;

// Works for both users and groups
foreach (var user in PrincipalExtensions.ResolveUsersForSamAccountName("dev-team"))
  Console.WriteLine(user.EmailAddress);
```

### Looking up a user by any name

```csharp
using System.DirectoryServices.AccountManagement;

var user = UserPrincipalExtensions.FindFirstDomainUserByAnyName("John Smith");
if (user != null) {
  Console.WriteLine(user.GetFullName(surnameFirst: true)); // "Smith, John"
  Console.WriteLine(user.GetEmailAddress());               // "John Smith" <jsmith@example.com>
}
```

### Querying LDAP groups

```csharp
using System.DirectoryServices.AccountManagement;

using var context = new PrincipalContext(ContextType.Domain);
var user = UserPrincipal.FindByIdentity(context, "jdoe");

// Direct groups only
foreach (var group in user.GetLDAPGroups())
  Console.WriteLine(group.DisplayName);

// All groups including transitive memberships
foreach (var group in user.GetAllLDAPGroups())
  Console.WriteLine($"{group.DisplayName} ({group.DistinguishedName})");
```

### Windows impersonation

```csharp
using System.Security;

// Impersonate with current domain
using (new Impersonation("serviceaccount", "password123")) {
  File.ReadAllText(@"\\server\share\secret.txt");
}

// Impersonate with explicit domain and logon type
using (new Impersonation("MYDOMAIN", "admin", "p@ssw0rd", Impersonation.LogonType.NetworkCleartext)) {
  // Code runs as impersonated user
}
// Original identity is automatically restored
```

---

## Installation

```bash
dotnet add package FrameworkExtensions.DirectoryServices
```

---

## Dependencies

- `Backports` (project reference)
- `Corlib.Extensions` (project reference)
- `System.DirectoryServices` (framework reference / NuGet)
- `System.DirectoryServices.AccountManagement` (framework reference / NuGet)

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
