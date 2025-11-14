# Extensions to Microsoft Office

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Microsoft.Office.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Microsoft.Office)](https://www.nuget.org/packages/FrameworkExtensions.Microsoft.Office/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for Microsoft Office Interop.

---

## Extension Methods

### Word Extensions

#### Document Extensions (`Microsoft.Office.Interop.Word.Document`)

- **`File()`** - Returns the FileInfo for the document's file path

#### Application Extensions (`Microsoft.Office.Interop.Word.Application`)

- **`CreatePrintToken(string printer)`** - Creates a disposable PrintToken that temporarily sets the active printer
  - Returns PrintToken which restores original printer on disposal
  - Use with `using` statement for automatic cleanup

---

### Excel Extensions

#### Application Extensions (`Microsoft.Office.Interop.Excel.Application`)

- **`OpenWorkbook(FileInfo file, ...)`** - Opens an Excel workbook with extensive configuration options
  - **UpdateLinks** - Control how links are updated (DontUpdate, ExternalOnly, RemoteOnly, ExternalAndRemote)
  - **Mode** - ReadOnly or ReadWrite
  - **Format** - Delimiter for text files (Tabs, Commas, Spaces, Semicolons, Nothing, CustomCharacter)
  - **Passwords** - Optional open/write passwords
  - **IgnoreReadOnly** - Control read-only recommended message
  - **Platform** - XlPlatform setting (default: xlWindows)
  - **Delimiter** - Custom delimiter character (default: comma)
  - **Edit** - Whether template is editable
  - **Notify** - File notification when unavailable
  - **Converter** - File converter index
  - **AddToMru** - Add to recently used files
  - **Local** - Save against Excel language or VBA language
  - **CorruptLoad** - How to handle corrupt files (NormalLoad, RepairFile, ExtractData)

---

### Outlook Extensions

#### MailItem Extensions (`Microsoft.Office.Interop.Outlook.MailItem`)

- **`AddMailToRecipients(params string[] mailTo)`** - Adds email addresses to recipients and resolves them
- **`AddAttachments(params FileInfo[] filesToAttach)`** - Adds file attachments to the email

---

## Installation

```bash
dotnet add package FrameworkExtensions.Microsoft.Office
```

**Requirements**: Requires Microsoft Office installation and Primary Interop Assemblies (PIAs).

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
