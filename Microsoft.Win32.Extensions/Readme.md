# Extensions to Microsoft.Win32

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Microsoft.Win32.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Microsoft.Win32)](https://www.nuget.org/packages/FrameworkExtensions.Microsoft.Win32/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for Windows Registry operations.

---

## Extension Methods

### RegistryKey Extensions (`RegistryKey`)

Registry key navigation and enumeration

- **`GetSubKeyPath()`** - Gets the full subkey path (without the hive prefix)
- **`GetSubKeyName()`** - Gets the name of the subkey (last segment of path)
- **`GetSubKeys(bool recursive = false)`** - Enumerates all subkeys, optionally recursively
  - Returns IEnumerable<RegistryKey> for iteration
- **`GetBaseKeyFromKeyName(string keyName, out string subKeyName)`** - Parses registry key name into base key and subkey path
  - Supports full names (HKEY_LOCAL_MACHINE) and abbreviations (HKLM, HKCU, HKCR, HKCC, HKPD, HKU)
  - Returns the appropriate RegistryKey for the hive

---

## Installation

```bash
dotnet add package FrameworkExtensions.Microsoft.Win32
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
