# Extensions to ASP.NET

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/ASP.NET.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.ASP.NET)](https://www.nuget.org/packages/FrameworkExtensions.ASP.NET/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for ASP.NET WebForms controls, part of [Hawkynt's .NET Framework Extensions](https://github.com/Hawkynt/C--FrameworkExtensions).

| Property              | Value                                                          |
|-----------------------|----------------------------------------------------------------|
| **Package ID**        | `FrameworkExtensions.ASP.NET`                                  |
| **Target Frameworks** | .NET Framework 4.7, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                              |

---

## Overview

This library provides extension methods for ASP.NET WebForms data-bound controls, simplifying access to underlying data in server-side UI components. The `GridViewRow` extensions are conditionally compiled and available only when targeting .NET Framework (`NETFRAMEWORK`).

---

## API Reference

### GridViewRow Extensions (`System.Web.UI.WebControls.GridViewRow`)

**Static class:** `GridViewRowExtensions`

| Method              | Signature                                                          | Description                                                                                                                                                                                           |
|---------------------|--------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GetDataFromColumn` | `GetDataFromColumn(this GridViewRow, string columnName) -> object` | Retrieves the value of a named column from the underlying `DataRowView` bound to the `GridViewRow`. The row must be of type `DataControlRowType.DataRow`; otherwise an `ArgumentException` is thrown. |

### Usage

```csharp
using System.Web.UI.WebControls;

protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e) {
  if (e.Row.RowType == DataControlRowType.DataRow) {
    var customerName = (string)e.Row.GetDataFromColumn("CustomerName");
    var orderId = (int)e.Row.GetDataFromColumn("OrderId");
  }
}
```

---

## Installation

```bash
dotnet add package FrameworkExtensions.ASP.NET
```

---

## Dependencies

- `Backports` (project reference)
- `System.Web` (framework reference, .NET Framework only)
- `Microsoft.AspNetCore.Components.Web` 3.1.26

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
