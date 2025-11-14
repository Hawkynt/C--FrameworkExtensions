# Extensions to WPF

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/PresentationCore.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.PresentationCore)](https://www.nuget.org/packages/FrameworkExtensions.PresentationCore/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

Extension methods and utilities for Windows Presentation Foundation (WPF).

---

## Extension Methods

### ItemCollection Extensions (`ItemCollection`)

- **`GetElementByName(string name)`** - Finds a FrameworkElement by its Name property
- **`AddRange(IEnumerable items)`** - Adds multiple items to the collection

---

### Selector Extensions (`Selector`)

- **`TryCastSelectedValue<TType>(ref TType value)`** - Safely casts the SelectedValue to a specified type
  - Returns true on success, false on InvalidCastException

---

### DispatcherObject Extensions (`DispatcherObject`)

Thread-safe invocation for WPF controls

- **`SafelyInvoke(Action action, bool async = false, DispatcherPriority dispatcherPriority = Normal)`** - Invokes action on dispatcher thread if needed
  - Returns true if executed on current thread, false if dispatched
  - Supports both synchronous and asynchronous invocation
- **`Async(Action action)`** - Executes action in another thread
  - If on GUI thread, uses BeginInvoke to run in background
  - If already on background thread, executes action directly
  - Returns true if invoked from background thread, false if from GUI thread

---

### Image Extensions (`System.Drawing.Image`)

GDI+ to WPF conversion (only on .NET Framework, not .NET Core/5+)

- **`ToBitmapImage()`** - Converts System.Drawing.Image to WPF BitmapImage
  - Uses PNG format for conversion via MemoryStream
  - Returns BitmapImage with CacheOption.OnLoad

---

### Event Extensions (`EventHandler<T>`, `MulticastDelegate`)

Safe event invocation with dispatcher support

- **`SafeInvoke<T>(object sender, T eventArgs)`** - Invokes event handlers on their appropriate threads
  - Automatically uses dispatcher for DispatcherObject targets
  - Synchronous invocation
- **`SafeInvoke(params object[] arguments)`** - MulticastDelegate version with parameter array
- **`AsyncSafeInvoke<T>(object sender, T eventArgs)`** - Asynchronous version of SafeInvoke
  - Uses BeginInvoke for non-blocking calls
- **`AsyncSafeInvoke(params object[] arguments)`** - Async MulticastDelegate version
  - Includes retry logic (30 attempts) for dispatcher failures

---

## Custom Types

### ObservableList<T>

Thread-safe observable list with WPF dispatcher integration

- Implements `IList<T>`, `IList`, `INotifyCollectionChanged`
- Requires `T : INotifyPropertyChanged`
- Constructor: `ObservableList(Dispatcher dispatcher = null)`
- All mutations automatically invoke on dispatcher thread
- Supports locking for thread-safe operations

### ObservableDictionary<TKey, TValue>

Thread-safe observable dictionary with WPF dispatcher integration

- Implements `IDictionary<TKey, TValue>`, `INotifyCollectionChanged`, `INotifyPropertyChanged`
- Constructors:
  - `ObservableDictionary(Dispatcher dispatcher = null)`
  - `ObservableDictionary(IDictionary<TKey, TValue> dictionary)`
  - `ObservableDictionary(int capacity)`
- All mutations automatically invoke on dispatcher thread
- Automatically handles property change notifications for values implementing INotifyPropertyChanged

---

## Installation

```bash
dotnet add package FrameworkExtensions.PresentationCore
```

---

## License

LGPL 3.0 or later - See [LICENSE](../LICENSE) for details
