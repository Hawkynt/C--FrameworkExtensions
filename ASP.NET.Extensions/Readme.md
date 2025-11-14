# Extensions to ASP.NET

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/ASP.NET.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.ASP.NET)](https://www.nuget.org/packages/FrameworkExtensions.ASP.NET/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods for ASP.NET web controls.

---

## Extension Methods

### GridViewRow Extensions (`GridViewRow`)

Data access utilities for GridView rows

- **`GetDataFromColumn(string columnName)`** - Gets data from a column by name
  - Requires row to be a DataRow (throws ArgumentException otherwise)
  - Returns the column value from the underlying DataRowView

---

## Installation

```bash
dotnet add package FrameworkExtensions.ASP.NET
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
