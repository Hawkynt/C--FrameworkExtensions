# Extensions to Microsoft Office

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Microsoft.Office.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Microsoft.Office)](https://www.nuget.org/packages/FrameworkExtensions.Microsoft.Office/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods and helper types for Microsoft Office Interop (Excel, Outlook, Word), part of [Hawkynt's .NET Framework Extensions](https://github.com/Hawkynt/C--FrameworkExtensions).

| Property              | Value                                                                  |
| --------------------- | ---------------------------------------------------------------------- |
| **Package ID**        | `FrameworkExtensions.Microsoft.Office`                                 |
| **Target Frameworks** | .NET Framework 4.0/4.5/4.8, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                                      |

---

## Overview

This library provides strongly-typed extension methods and helper types for working with Microsoft Office Interop assemblies. It simplifies common Office automation tasks such as opening Excel workbooks with fine-grained options, composing and sending Outlook emails with attachments, temporarily switching Word printers, and retrieving document file information. The Office Interop extensions are conditionally compiled and excluded from .NET Standard builds.

---

## API Reference

### Excel

#### Application Extensions (`Microsoft.Office.Interop.Excel.Application`)

**Static class:** `ApplicationExtensions`

##### Enumerations

| Enum             | Values                                                                                         | Description                                                                       |
| ---------------- | ---------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `Mode`           | `ReadOnly (0)`, `ReadWrite (1)`                                                                | Specifies whether to open a workbook in read-only or read-write mode.             |
| `IgnoreReadOnly` | `IgnoreReadOnlyRecommended`, `DontIgnoreReadOnlyRecommended`                                   | Controls whether the read-only recommended message is displayed on open.          |
| `Edit`           | `NotEditable`, `Editable`                                                                      | Determines if an Excel 4.0 add-in or template is opened for editing.              |
| `Notify`         | `DontNotify (0)`, `Notify (1)`                                                                 | Controls file-notification-list behavior when a file cannot be opened read-write. |
| `AddToMru`       | `DontAddToMru`, `AddToMru`                                                                     | Whether to add the workbook to the Most Recently Used list.                       |
| `Local`          | `NotLocal`, `Local`                                                                            | Controls whether files are saved against the Excel language or the VBA language.  |
| `CorruptLoad`    | `NormalLoad (0)`, `RepairFile (1)`, `ExtractData (2)`                                          | Specifies how to handle potentially corrupt files during open.                    |
| `UpdateLinks`    | `DontUpdate (0)`, `ExternalOnly (1)`, `RemoteOnly (2)`, `ExternalAndRemote (3)`                | Controls how links in the file are updated on open.                               |
| `Format`         | `Tabs (1)`, `Commas (2)`, `Spaces (3)`, `Semicolons (4)`, `Nothing (5)`, `CustomCharacter (6)` | Delimiter format for text file imports.                                           |
| `Converter`      | `Default (0)`                                                                                  | File converter index selection.                                                   |

##### Extension Methods

| Method         | Signature                                                                                                                                                                                                                            | Description                                                                                                                                |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `OpenWorkbook` | `OpenWorkbook(this Application, FileInfo file, UpdateLinks, Mode, Format, string openPassword, string writePassword, IgnoreReadOnly, XlPlatform, char delimiter, Edit, Notify, Converter, AddToMru, Local, CorruptLoad) -> Workbook` | Opens a workbook from a `FileInfo` with strongly-typed, fully configurable parameters. All parameters after `file` have sensible defaults. |

---

### Outlook

#### MailItem Extensions (`Microsoft.Office.Interop.Outlook.MailItem`)

**Static class:** `MailItemExtensions`

| Method                | Signature                                                        | Description                                                                                                                                |
| --------------------- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `AddMailToRecipients` | `AddMailToRecipients(this MailItem, params string[] mailTo)`     | Adds one or more email addresses as recipients to the mail item and resolves all recipients. Null/whitespace entries are silently skipped. |
| `AddAttachments`      | `AddAttachments(this MailItem, params FileInfo[] filesToAttach)` | Attaches one or more files (specified as `FileInfo` objects) to the mail item. Files are attached by value (`olByValue`).                  |

---

### Word

#### PrintToken (class, `Microsoft.Office.Interop.Word`)

An `IDisposable` helper that temporarily changes the active printer on a Word application instance and restores the original printer when disposed.

| Member                                         | Description                                                               |
| ---------------------------------------------- | ------------------------------------------------------------------------- |
| `PrintToken(_Application app, string printer)` | Saves the current active printer, then switches to the specified printer. |
| `Dispose()`                                    | Restores the original active printer that was saved in the constructor.   |

#### Application Extensions (`Microsoft.Office.Interop.Word.Application`)

**Static class:** `ApplicationExtensions`

| Method             | Signature                                                          | Description                                                                                                                                                |
| ------------------ | ------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `CreatePrintToken` | `CreatePrintToken(this Application, string printer) -> PrintToken` | Creates a disposable `PrintToken` that temporarily sets the active printer. Dispose the token (e.g. with a `using` block) to restore the previous printer. |

#### Document Extensions (`Microsoft.Office.Interop.Word.Document`)

**Static class:** `DocumentExtensions`

| Method | Signature                         | Description                                                                             |
| ------ | --------------------------------- | --------------------------------------------------------------------------------------- |
| `File` | `File(this Document) -> FileInfo` | Returns a `System.IO.FileInfo` for the document's full file path (`Document.FullName`). |

---

## Usage Examples

### Opening an Excel workbook

```csharp
using Microsoft.Office.Interop.Excel;
using static Microsoft.Office.Interop.Excel.ApplicationExtensions;

var excelApp = new Application();
var file = new FileInfo(@"C:\Reports\data.xlsx");
var workbook = excelApp.OpenWorkbook(
  file,
  openMode: Mode.ReadOnly,
  updateLinks: UpdateLinks.DontUpdate
);
```

### Composing an Outlook email with attachments

```csharp
using Microsoft.Office.Interop.Outlook;

var outlookApp = new Application();
var mail = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
mail.Subject = "Monthly Report";
mail.Body = "Please find the report attached.";
mail.AddMailToRecipients("alice@example.com", "bob@example.com");
mail.AddAttachments(new FileInfo(@"C:\Reports\report.pdf"));
mail.Send();
```

### Temporarily switching the Word printer

```csharp
using Microsoft.Office.Interop.Word;

var wordApp = new Application();
using (wordApp.CreatePrintToken("PDF Printer")) {
  wordApp.ActiveDocument.PrintOut();
} // original printer is restored here
```

### Getting a document's file info

```csharp
using Microsoft.Office.Interop.Word;

var doc = wordApp.ActiveDocument;
var fileInfo = doc.File();
Console.WriteLine(fileInfo.FullName);    // full path
Console.WriteLine(fileInfo.Length);       // file size in bytes
Console.WriteLine(fileInfo.LastWriteTime); // last modified
```

---

## Installation

```bash
dotnet add package FrameworkExtensions.Microsoft.Office
```

**Requirements**: Requires Microsoft Office installation and Primary Interop Assemblies (PIAs).

---

## Dependencies

- `Backports` (project reference)
- `Microsoft.Office.Interop.Excel` 15.0.4795.1001
- `Microsoft.Office.Interop.Outlook` 15.0.4797.1003
- `Microsoft.Office.Interop.Word` 15.0.4797.1003

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
