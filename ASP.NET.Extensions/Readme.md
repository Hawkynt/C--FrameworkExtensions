# FrameworkExtensions.ASP.NET

[![CI](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/ci.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=main)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/main/ASP.NET.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.ASP.NET)](https://www.nuget.org/packages/FrameworkExtensions.ASP.NET/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

> Extension methods for ASP.NET WebForms data-bound controls, for reaching a row’s underlying data item without the usual cast dance.

| Property              | Value                                                          |
|-----------------------|----------------------------------------------------------------|
| **Package ID**        | `FrameworkExtensions.ASP.NET`                                  |
| **Target Frameworks** | .NET Framework 4.7, .NET Standard 2.0, .NET Core 3.1, .NET 6.0 |
| **License**           | LGPL-3.0-or-later                                              |

---

## 📦 Installation

```bash
dotnet add package FrameworkExtensions.ASP.NET
```

## ✨ Features
This library provides extension methods for ASP.NET WebForms data-bound controls, simplifying access to underlying data in server-side UI components. The `GridViewRow` extensions are conditionally compiled and available only when targeting .NET Framework (`NETFRAMEWORK`).

---

## 🧭 Extension methods by type
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

## 🚀 Quick start

Add the package, then use the members catalogued above — they are extension methods, so they appear on the framework types directly once the namespace is in scope.

## 📚 API reference

This package's only public type, `GridViewRowExtensions`, is compiled **exclusively** for .NET Framework targets. On the .NET Standard, .NET Core and .NET builds the assembly is empty, so there is no generated reference to show. The members are catalogued above.

## 🔌 Dependencies

- `Backports` (project reference)
- `System.Web` (framework reference, .NET Framework only)
- `Microsoft.AspNetCore.Components.Web` 3.1.26

## ⚠️ Limitations

- The `GridViewRow` extensions are conditionally compiled and exist **only** on .NET Framework targets (`NETFRAMEWORK`). On .NET Standard, .NET Core and .NET they are absent.
- WebForms only. Nothing here applies to ASP.NET MVC or ASP.NET Core.

## ❤️ Support

If this project saves you time or money, consider supporting its development:

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor-EA4AAA?logo=githubsponsors)](https://github.com/sponsors/Hawkynt)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?logo=paypal)](https://www.paypal.me/hawkynt)

## 📜 License

Licensed under LGPL-3.0-or-later — see the repository [LICENSE](https://github.com/Hawkynt/C--FrameworkExtensions/blob/main/LICENSE).
