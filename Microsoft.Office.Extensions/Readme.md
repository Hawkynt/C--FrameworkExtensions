# FrameworkExtensions.Microsoft.Office

[![CI](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/main/Microsoft.Office.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Microsoft.Office)](https://www.nuget.org/packages/FrameworkExtensions.Microsoft.Office/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

> Extension methods for the Microsoft Office COM interop object model, smoothing over the parts of Excel and Word automation that are awkward from C#.

| Property              | Value                                                                  |
| --------------------- | ---------------------------------------------------------------------- |
| **Package ID**        | `FrameworkExtensions.Microsoft.Office`                                 |
| **Target Frameworks** | .NET Framework 4.0/4.5/4.8, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                                      |

---

## 📦 Installation

```bash
dotnet add package FrameworkExtensions.Microsoft.Office
```

## ✨ Features
This library provides strongly-typed extension methods and helper types for working with Microsoft Office Interop assemblies. It simplifies common Office automation tasks such as opening Excel workbooks with fine-grained options, composing and sending Outlook emails with attachments, temporarily switching Word printers, and retrieving document file information. The Office Interop extensions are conditionally compiled and excluded from .NET Standard builds.

---

## 🧭 Extension methods by type
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

## 🚀 Quick start
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

## 📚 API reference

<!-- API:BEGIN generated by Hawkynt/RepositoryTemplate/package-readme — edit the XML docs in source, not here -->

### Namespace `Microsoft.Office.Interop.Excel`

[`ApplicationExtensions.AddToMru`](#applicationextensionsaddtomru) · [`ApplicationExtensions.Converter`](#applicationextensionsconverter) · [`ApplicationExtensions.CorruptLoad`](#applicationextensionscorruptload) · [`ApplicationExtensions.Edit`](#applicationextensionsedit) · [`ApplicationExtensions.Format`](#applicationextensionsformat) · [`ApplicationExtensions.IgnoreReadOnly`](#applicationextensionsignorereadonly) · [`ApplicationExtensions.Local`](#applicationextensionslocal) · [`ApplicationExtensions.Mode`](#applicationextensionsmode) · [`ApplicationExtensions.Notify`](#applicationextensionsnotify) · [`ApplicationExtensions.UpdateLinks`](#applicationextensionsupdatelinks)

#### `ApplicationExtensions.AddToMru`

True to add this workbook to the list of recently used files. The default value is False.

| Value | Numeric | Summary |
| --- | --- | --- |
| `DontAddToMru` | `0` |  |
| `AddToMru` | `1` |  |

#### `ApplicationExtensions.Converter`

The index of the first file converter to try when opening the file. The specified file converter is tried first; if this converter doesnt recognize the file, all other converters are tried. The converter index consists of the row numbers of the converters returned by the FileConverters property.

| Value | Numeric | Summary |
| --- | --- | --- |
| `Default` | `0` |  |

#### `ApplicationExtensions.CorruptLoad`

| Value | Numeric | Summary |
| --- | --- | --- |
| `NormalLoad` | `0` |  |
| `RepairFile` | `1` |  |
| `ExtractData` | `2` |  |

#### `ApplicationExtensions.Edit`

If the file is a Microsoft Excel 4.0 add-in, this argument is True to open the add-in so that its a visible window. If this argument is False or omitted, the add-in is opened as hidden, and it cannot be unhidden. This option doesn't apply to add-ins created in Microsoft Excel 5.0 or later. If the file is an Excel template, True to open the specified template for editing. False to open a new workbook based on the specified template. The default value is False.

| Value | Numeric | Summary |
| --- | --- | --- |
| `NotEditable` | `0` |  |
| `Editable` | `1` |  |

#### `ApplicationExtensions.Format`

If Microsoft Excel is opening a text file, this argument specifies the delimiter character, as shown in the following table. If this argument is omitted, the current delimiter is used.

| Value | Numeric | Summary |
| --- | --- | --- |
| `Tabs` | `1` |  |
| `Commas` | `2` |  |
| `Spaces` | `3` |  |
| `Semicolons` | `4` |  |
| `Nothing` | `5` |  |
| `CustomCharacter` | `6` |  |

#### `ApplicationExtensions.IgnoreReadOnly`

True to have Microsoft Excel not display the read-only recommended message (if the workbook was saved with the Read-Only Recommended option).

| Value | Numeric | Summary |
| --- | --- | --- |
| `IgnoreReadOnlyRecommended` | `0` |  |
| `DontIgnoreReadOnlyRecommended` | `1` |  |

#### `ApplicationExtensions.Local`

True saves files against the language of Microsoft Excel (including control panel settings). False (default) saves files against the language of Visual Basic for Applications (VBA) (which is typically US English unless the VBA project where Workbooks.Open is run from is an old internationalized XL5/95 VBA project).

| Value | Numeric | Summary |
| --- | --- | --- |
| `NotLocal` | `0` |  |
| `Local` | `1` |  |

#### `ApplicationExtensions.Mode`

True to open the workbook in read-only mode.

| Value | Numeric | Summary |
| --- | --- | --- |
| `ReadOnly` | `0` |  |
| `ReadWrite` | `1` |  |

#### `ApplicationExtensions.Notify`

If the file cannot be opened in read/write mode, this argument is True to add the file to the file notification list. Microsoft Excel will open the file as read-only, poll the file notification list, and then notify the user when the file becomes available. If this argument is False or omitted, no notification is requested, and any attempts to open an unavailable file will fail.

| Value | Numeric | Summary |
| --- | --- | --- |
| `DontNotify` | `0` |  |
| `Notify` | `1` |  |

#### `ApplicationExtensions.UpdateLinks`

Specifies the way links in the file are updated. If this argument is omitted, the user is prompted to specify how links will be updated. Otherwise, this argument is one of the values listed in the following table.

| Value | Numeric | Summary |
| --- | --- | --- |
| `DontUpdate` | `0` |  |
| `ExternalOnly` | `1` |  |
| `RemoteOnly` | `2` |  |
| `ExternalAndRemote` | `3` |  |

<!-- API:END -->

## 🔌 Dependencies

- `Backports` (project reference)
- `Microsoft.Office.Interop.Excel` 15.0.4795.1001
- `Microsoft.Office.Interop.Outlook` 15.0.4797.1003
- `Microsoft.Office.Interop.Word` 15.0.4797.1003

## ⚠️ Limitations

- Requires Microsoft Office to be installed. These are COM interop helpers, not a file-format library.
- Windows only, with the usual Office interop caveats around releasing COM objects.

## ❤️ Support

If this project saves you time or money, consider supporting its development:

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor-EA4AAA?logo=githubsponsors)](https://github.com/sponsors/Hawkynt)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?logo=paypal)](https://www.paypal.me/hawkynt)

## 📜 License

Licensed under LGPL-3.0-or-later — see the repository [LICENSE](https://github.com/Hawkynt/C--FrameworkExtensions/blob/main/LICENSE).
