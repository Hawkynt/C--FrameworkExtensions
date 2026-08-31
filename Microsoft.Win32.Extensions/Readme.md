# FrameworkExtensions.Microsoft.Win32

[![CI](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/main/Microsoft.Win32.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Microsoft.Win32)](https://www.nuget.org/packages/FrameworkExtensions.Microsoft.Win32/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

> Extension methods for the Windows Registry, turning `RegistryKey` access into typed reads and writes with sensible defaults instead of object casts.

| Property              | Value                                                                      |
| --------------------- | -------------------------------------------------------------------------- |
| **Package ID**        | `FrameworkExtensions.Microsoft.Win32`                                      |
| **Target Frameworks** | .NET Framework 3.5/4.0/4.5/4.8, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                                          |

---

## 📦 Installation

```bash
dotnet add package FrameworkExtensions.Microsoft.Win32
```

## ✨ Features
This library provides extension methods for `Microsoft.Win32.RegistryKey` that simplify navigation and enumeration of the Windows Registry. It includes helpers for parsing full registry key paths, extracting path segments, and recursively enumerating sub-keys. On .NET 5+ the class is annotated with `[SupportedOSPlatform("windows")]`.

---

## 🧭 Extension methods by type
### RegistryKey Extensions (`Microsoft.Win32.RegistryKey`)

**Static class:** `RegistryKeyExtensions`

#### Extension Methods

| Method          | Signature                                                                          | Description                                                                                                                                                       |
| --------------- | ---------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetSubKeyPath` | `GetSubKeyPath(this RegistryKey) -> string`                                        | Returns the full sub-key path (everything after the root hive name). For example, for `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft` this returns `SOFTWARE\Microsoft`.  |
| `GetSubKeyName` | `GetSubKeyName(this RegistryKey) -> string`                                        | Returns the leaf name of the key (the last segment after the final backslash). For example, for `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft` this returns `Microsoft`. |
| `GetSubKeys`    | `GetSubKeys(this RegistryKey, bool recursive = false) -> IEnumerable<RegistryKey>` | Enumerates all immediate sub-keys of the current key. When `recursive` is `true`, performs a depth-first traversal and yields all sub-keys in the entire tree.    |

#### Static Methods

| Method                  | Signature                                                                     | Description                                                                                                                                                                                  |
| ----------------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GetBaseKeyFromKeyName` | `GetBaseKeyFromKeyName(string keyName, out string subKeyName) -> RegistryKey` | Parses a full registry key path string and returns the corresponding root `RegistryKey` hive object along with the remaining sub-key path. Supports standard names and common abbreviations. |

**Supported hive identifiers:**

| Full Name               | Abbreviation | Maps To                    |
| ----------------------- | ------------ | -------------------------- |
| `HKEY_CLASSES_ROOT`     | `HKCM`       | `Registry.ClassesRoot`     |
| `HKEY_CURRENT_CONFIG`   | `HKCC`       | `Registry.CurrentConfig`   |
| `HKEY_CURRENT_USER`     | `HKCU`       | `Registry.CurrentUser`     |
| `HKEY_LOCAL_MACHINE`    | `HKLM`       | `Registry.LocalMachine`    |
| `HKEY_PERFORMANCE_DATA` | `HKPD`       | `Registry.PerformanceData` |
| `HKEY_USERS`            | `HKU`        | `Registry.Users`           |

---

## 🚀 Quick start
### Getting the sub-key path and name

```csharp
using Microsoft.Win32;

using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows");
var path = key.GetSubKeyPath(); // "SOFTWARE\Microsoft\Windows"
var name = key.GetSubKeyName(); // "Windows"
```

### Enumerating sub-keys recursively

```csharp
using Microsoft.Win32;

using var root = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MyApp");
foreach (var subKey in root.GetSubKeys(recursive: true))
  Console.WriteLine(subKey.Name);
```

### Parsing a full key name

```csharp
using Microsoft.Win32;

var baseKey = RegistryKeyExtensions.GetBaseKeyFromKeyName(
  @"HKLM\SOFTWARE\Microsoft",
  out var subKeyName
);
// baseKey    => Registry.LocalMachine
// subKeyName => "SOFTWARE\Microsoft"
```

## 📚 API reference

<!-- API:BEGIN generated by Hawkynt/RepositoryTemplate/package-readme — edit the XML docs in source, not here -->

Every public and protected member of all 1 type, generated from the built assembly and its XML documentation, is in [REFERENCE.md](https://github.com/Hawkynt/C--FrameworkExtensions/blob/main/Microsoft.Win32.Extensions/REFERENCE.md).

<!-- API:END -->

## 🔌 Dependencies

- `Backports` (project reference)
- `Microsoft.Win32.Registry` (NuGet, for .NET Core / .NET Standard targets)

## ⚠️ Limitations

- Windows only. The registry does not exist elsewhere.
- Registry access is bounded by the calling process privileges; writing under `HKEY_LOCAL_MACHINE` normally needs elevation.

## ❤️ Support

If this project saves you time or money, consider supporting its development:

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor-EA4AAA?logo=githubsponsors)](https://github.com/sponsors/Hawkynt)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?logo=paypal)](https://www.paypal.me/hawkynt)

## 📜 License

Licensed under LGPL-3.0-or-later — see the repository [LICENSE](https://github.com/Hawkynt/C--FrameworkExtensions/blob/main/LICENSE).
