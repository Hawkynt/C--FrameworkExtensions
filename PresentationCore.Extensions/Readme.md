# FrameworkExtensions.PresentationCore

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/PresentationCore.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.PresentationCore)](https://www.nuget.org/packages/FrameworkExtensions.PresentationCore/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

> Extension methods for WPF, covering the visual-tree walking and dispatcher chores that every WPF codebase ends up re-implementing.

| Property              | Value                                                       |
| --------------------- | ----------------------------------------------------------- |
| **Package ID**        | `FrameworkExtensions.PresentationCore`                      |
| **Target Frameworks** | .NET Framework 4.0/4.5/4.8, .NET Core 3.1, .NET 6.0-windows |
| **License**           | LGPL-3.0-or-later                                           |

---

## 📦 Installation

```bash
dotnet add package FrameworkExtensions.PresentationCore
```

## ✨ Features
This library provides WPF-specific utilities including dispatcher-aware observable collections, GDI+ to WPF image conversion, safe cross-thread event invocation, and extension methods for WPF controls. All types are designed to integrate seamlessly with WPF's dispatcher-based threading model and data-binding infrastructure.

---

## 🧭 Extension methods by type
### Observable Collections

#### `ObservableDictionary<TKey, TValue>` (class)

**Namespace:** `System.Collections.ObjectModel`

A dictionary that implements `IDictionary<TKey, TValue>`, `INotifyCollectionChanged`, and `INotifyPropertyChanged`. All mutating operations are automatically marshaled to the WPF dispatcher thread. When values implement `INotifyPropertyChanged` or `INotifyCollectionChanged`, the dictionary subscribes to their change events and propagates notifications upward.

| Member                                                       | Description                                                                                                                                                 |
| ------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ObservableDictionary(Dispatcher dispatcher = null)`         | Creates an empty dictionary bound to the specified dispatcher (defaults to `Application.Current.Dispatcher` or `Dispatcher.CurrentDispatcher`).             |
| `ObservableDictionary(IDictionary<TKey, TValue> dictionary)` | Creates a dictionary initialized from an existing dictionary.                                                                                               |
| `ObservableDictionary(int capacity)`                         | Creates a dictionary with the specified initial capacity.                                                                                                   |
| `Add(TKey, TValue)`                                          | Adds a key-value pair, dispatching to the UI thread if necessary. Raises `CollectionChanged` with `Add` action.                                             |
| `Add(KeyValuePair<TKey, TValue>)`                            | Adds a key-value pair from a `KeyValuePair`.                                                                                                                |
| `Remove(TKey) -> bool`                                       | Removes a key, dispatching to the UI thread if necessary. Raises `CollectionChanged` with `Remove` action. Returns `true` if the key was found and removed. |
| `Remove(KeyValuePair<TKey, TValue>) -> bool`                 | Removes a specific key-value pair.                                                                                                                          |
| `Clear()`                                                    | Clears all entries, unsubscribing from value change events. Raises `CollectionChanged` with `Reset` action.                                                 |
| `this[TKey]`                                                 | Gets or replaces a value by key. Setter raises `CollectionChanged` with `Replace` action and manages change-event subscriptions.                            |
| `ContainsKey(TKey) -> bool`                                  | Checks if a key exists.                                                                                                                                     |
| `Contains(KeyValuePair<TKey, TValue>) -> bool`               | Checks if a key-value pair exists.                                                                                                                          |
| `TryGetValue(TKey, out TValue) -> bool`                      | Attempts to get a value by key.                                                                                                                             |
| `Keys -> ICollection<TKey>`                                  | Returns all keys.                                                                                                                                           |
| `Values -> ICollection<TValue>`                              | Returns all values.                                                                                                                                         |
| `Count -> int`                                               | Returns the number of entries.                                                                                                                              |
| `IsReadOnly -> bool`                                         | Returns whether the dictionary is read-only.                                                                                                                |
| `CollectionChanged`                                          | Event raised on add, remove, replace, or reset.                                                                                                             |
| `PropertyChanged`                                            | Event raised when a contained value's property changes.                                                                                                     |

#### `ObservableList<T>` (class)

**Namespace:** `System.Collections.Specialized`

**Constraint:** `T : INotifyPropertyChanged`

A thread-safe, dispatcher-aware list implementing `IList<T>`, `IList`, and `INotifyCollectionChanged`. All mutating operations are marshaled to the associated dispatcher thread and protected by a lock for thread safety.

| Member                                         | Description                                                                                       |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| `ObservableList(Dispatcher dispatcher = null)` | Creates an empty list bound to the given dispatcher (defaults to `Dispatcher.CurrentDispatcher`). |
| `Dispatcher`                                   | The `Dispatcher` this list is bound to.                                                           |
| `Add(T)`                                       | Appends an item. Raises `CollectionChanged` with `Add` action.                                    |
| `AddRange(IEnumerable<T>)`                     | Appends multiple items in a single operation. Raises `CollectionChanged` with `Reset` action.     |
| `Insert(int index, T)`                         | Inserts an item at the specified index.                                                           |
| `Remove(T) -> bool`                            | Removes the first occurrence of the specified item. Returns `true` if found.                      |
| `RemoveAt(int index)`                          | Removes the item at the specified index.                                                          |
| `Clear()`                                      | Removes all items. Raises `CollectionChanged` with `Reset` action.                                |
| `this[int index]`                              | Gets or replaces an item at an index. Setter raises `CollectionChanged` with `Replace` action.    |
| `IndexOf(T) -> int`                            | Returns the index of the specified item, or -1 if not found.                                      |
| `Contains(T) -> bool`                          | Checks if the item exists in the list.                                                            |
| `CopyTo(T[], int)`                             | Copies the list contents to an array.                                                             |
| `ToArray() -> T[]`                             | Returns a thread-safe snapshot of the list as an array.                                           |
| `Count -> int`                                 | Returns the current item count.                                                                   |
| `IsReadOnly -> bool`                           | Always returns `false`.                                                                           |
| `IsFixedSize -> bool`                          | Always returns `false`.                                                                           |
| `CollectionChanged`                            | Event raised on add, remove, replace, or reset.                                                   |

---

### Image Conversion

#### Image Extensions (`System.Drawing.Image`)

**Static class:** `ImageExtensions`

**Namespace:** `System.Drawing`

**Availability:** .NET Framework only (excluded from .NET Core and .NET 5+)

| Method          | Signature                                  | Description                                                                                                                                                                                                                   |
| --------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ToBitmapImage` | `ToBitmapImage(this Image) -> BitmapImage` | Converts a GDI+ `System.Drawing.Image` into a WPF `BitmapImage` by encoding to PNG in a `MemoryStream` and loading it with `BitmapCacheOption.OnLoad`. The stream is fully consumed and the image is ready for immediate use. |

---

### WPF Control Extensions

#### ItemCollection Extensions (`System.Windows.Controls.ItemCollection`)

**Static class:** `ItemCollectionExtensions`

| Method             | Signature                                                                | Description                                                                                                                              |
| ------------------ | ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `GetElementByName` | `GetElementByName(this ItemCollection, string name) -> FrameworkElement` | Finds and returns the first `FrameworkElement` in the collection whose `Name` property matches the given string, or `null` if not found. |
| `AddRange`         | `AddRange(this ItemCollection, IEnumerable items)`                       | Adds all items from an `IEnumerable` to the `ItemCollection` one by one.                                                                 |

#### Selector Extensions (`System.Windows.Controls.Primitives.Selector`)

**Static class:** `SelectorExtensions`

| Method                        | Signature                                                             | Description                                                                                                                                                                                                            |
| ----------------------------- | --------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `TryCastSelectedValue<TType>` | `TryCastSelectedValue<TType>(this Selector, ref TType value) -> bool` | Attempts to cast the `SelectedValue` of a `Selector` control to the specified type. Returns `true` on success and writes the value to the `ref` parameter. Returns `false` if the cast fails (`InvalidCastException`). |

---

### Dispatcher & Threading

#### DispatcherObject Extensions (`System.Windows.Threading.DispatcherObject`)

**Static class:** `DispatcherObjectExtensions`

| Method         | Signature                                                                                                              | Description                                                                                                                                                                                                             |
| -------------- | ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SafelyInvoke` | `SafelyInvoke(this DispatcherObject, Action action, bool async = false, DispatcherPriority priority = Normal) -> bool` | Executes an action on the dispatcher thread. If already on the correct thread, invokes immediately and returns `true`. Otherwise dispatches synchronously (or asynchronously if `async` is `true`) and returns `false`. |
| `Async`        | `Async(this DispatcherObject, Action action) -> bool`                                                                  | If called from the UI thread, runs the action asynchronously on a background thread using `BeginInvoke` and returns `false`. If called from a background thread, runs the action inline and returns `true`.             |

#### Event Extensions (`System.Windows.Threading`)

**Static class:** `EventExtensions`

Safe event invocation methods that respect WPF's dispatcher model. Each subscriber in the invocation list is automatically marshaled to its owning dispatcher thread if it is a `DispatcherObject`.

| Method               | Signature                                                              | Description                                                                                                                                                                                                   |
| -------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SafeInvoke<T>`      | `SafeInvoke<T>(this EventHandler<T>, object sender, T eventArgs)`      | Synchronously invokes each subscriber on its owning dispatcher thread. If a subscriber's target is a `DispatcherObject`, the call is marshaled to that object's dispatcher; otherwise it is invoked directly. |
| `SafeInvoke`         | `SafeInvoke(this MulticastDelegate, params object[] arguments)`        | Synchronously invokes each delegate in the invocation list on its owning dispatcher thread.                                                                                                                   |
| `AsyncSafeInvoke<T>` | `AsyncSafeInvoke<T>(this EventHandler<T>, object sender, T eventArgs)` | Asynchronous variant of `SafeInvoke<T>`. Each subscriber is invoked on a background thread, with `BeginInvoke` used for `DispatcherObject` targets.                                                           |
| `AsyncSafeInvoke`    | `AsyncSafeInvoke(this MulticastDelegate, params object[] arguments)`   | Asynchronous variant of `SafeInvoke` for `MulticastDelegate`. Includes retry logic (up to 30 attempts) for dispatcher invocation failures.                                                                    |

---

## 🚀 Quick start
### Using ObservableDictionary with WPF data binding

```csharp
using System.Collections.ObjectModel;

var dict = new ObservableDictionary<string, int>();
dict.CollectionChanged += (s, e) => Console.WriteLine($"Action: {e.Action}");
dict.Add("apples", 3);
dict.Add("bananas", 5);
dict.Remove("apples");
```

### Using ObservableList with dispatcher awareness

```csharp
using System.Collections.Specialized;

var list = new ObservableList<MyViewModel>();
list.CollectionChanged += (s, e) => UpdateUI();
// Safe to call from any thread - automatically dispatches to UI thread
list.Add(new MyViewModel { Name = "Item 1" });
list.AddRange(new[] { viewModel2, viewModel3 });
```

### Converting a GDI+ image to WPF BitmapImage

```csharp
using System.Drawing;

var bitmap = new Bitmap(@"C:\Images\photo.png");
var wpfImage = bitmap.ToBitmapImage();
myWpfImageControl.Source = wpfImage;
```

### Safe cross-thread event invocation

```csharp
using System.Windows.Threading;

// In a background service or ViewModel:
public event EventHandler<DataEventArgs> DataReady;

// Invoke safely - each subscriber gets the event on its own dispatcher thread
DataReady.SafeInvoke(this, new DataEventArgs(result));

// Or fire-and-forget asynchronously
DataReady.AsyncSafeInvoke(this, new DataEventArgs(result));
```

### Safely invoking actions on WPF controls

```csharp
using System.Windows.Threading;

// From any thread, safely update a UI element
myTextBlock.SafelyInvoke(() => myTextBlock.Text = "Updated!");

// Run a long operation off the UI thread
myControl.Async(() => {
  var data = LoadDataFromDatabase();
  myControl.SafelyInvoke(() => myGrid.ItemsSource = data);
});
```

## 📚 API reference

<!-- API:BEGIN generated by Hawkynt/RepositoryTemplate/package-readme — edit the XML docs in source, not here -->

### Namespace `System.Collections.ObjectModel`

[`ObservableDictionary<TKey, TValue>`](#observabledictionarytkey-tvalue)

#### `ObservableDictionary<TKey, TValue>`

Implements `ICollection<KeyValuePair<TKey, TValue>>`, `IDictionary<TKey, TValue>`, `IEnumerable`, `IEnumerable<KeyValuePair<TKey, TValue>>`, `INotifyCollectionChanged`, `INotifyPropertyChanged`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ObservableDictionary` | `ObservableDictionary(Dispatcher dispatcher = null)` |  |
| `ObservableDictionary` | `ObservableDictionary(IDictionary<TKey, TValue> dictionary)` |  |
| `ObservableDictionary` | `ObservableDictionary(int capacity)` |  |
| `Count` | `int Count { get; }` |  |
| `IsReadOnly` | `bool IsReadOnly { get; }` |  |
| `Item` | `TValue this[TKey key] { get; set; }` |  |
| `Keys` | `ICollection<TKey> Keys { get; }` |  |
| `Values` | `ICollection<TValue> Values { get; }` |  |
| `Add` | `void Add(KeyValuePair<TKey, TValue> item)` |  |
| `Add` | `void Add(TKey key, TValue value)` |  |
| `Clear` | `void Clear()` |  |
| `ContainsKey` | `bool ContainsKey(TKey key)` |  |
| `Contains` | `bool Contains(KeyValuePair<TKey, TValue> item)` |  |
| `CopyTo` | `void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)` |  |
| `GetEnumerator` | `IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()` |  |
| `OnCollectionChanged` | `protected void OnCollectionChanged(NotifyCollectionChangedAction action)` |  |
| `OnCollectionChanged` | `protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue item)` |  |
| `OnCollectionChanged` | `protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue oldItem, TValue newItem)` |  |
| `OnCollectionChanged` | `protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)` |  |
| `OnPropertyChanged` | `protected void OnPropertyChanged(PropertyChangedEventArgs e)` |  |
| `OnPropertyChanged` | `protected void OnPropertyChanged(string property)` |  |
| `Remove` | `bool Remove(KeyValuePair<TKey, TValue> item)` |  |
| `Remove` | `bool Remove(TKey key)` |  |
| `TryGetValue` | `bool TryGetValue(TKey key, out TValue value)` |  |
| `CollectionChanged` | `event NotifyCollectionChangedEventHandler CollectionChanged` |  |
| `PropertyChanged` | `event PropertyChangedEventHandler PropertyChanged` |  |

### Namespace `System.Collections.Specialized`

[`ObservableList<T>`](#observablelistt)

#### `ObservableList<T>`

Implements `ICollection`, `ICollection<T>`, `IEnumerable`, `IEnumerable<T>`, `IList`, `IList<T>`, `INotifyCollectionChanged`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ObservableList` | `ObservableList(Dispatcher dispatcher = null)` |  |
| `Count` | `int Count { get; }` |  |
| `Dispatcher` | `Dispatcher Dispatcher { get; }` |  |
| `IsFixedSize` | `bool IsFixedSize { get; }` |  |
| `IsReadOnly` | `bool IsReadOnly { get; }` |  |
| `IsSynchronized` | `bool IsSynchronized { get; }` |  |
| `Item` | `T this[int index] { get; set; }` |  |
| `SyncRoot` | `object SyncRoot { get; }` |  |
| `AddRange` | `void AddRange(IEnumerable<T> items)` |  |
| `Add` | `int Add(object item)` |  |
| `Add` | `void Add(T item)` |  |
| `Clear` | `void Clear()` |  |
| `Contains` | `bool Contains(T item)` |  |
| `Contains` | `bool Contains(object value)` |  |
| `CopyTo` | `void CopyTo(Array array, int index)` |  |
| `CopyTo` | `void CopyTo(T[] array, int arrayIndex)` |  |
| `GetEnumerator` | `IEnumerator<T> GetEnumerator()` |  |
| `IndexOf` | `int IndexOf(T item)` |  |
| `IndexOf` | `int IndexOf(object value)` |  |
| `Insert` | `void Insert(int index, T item)` |  |
| `Insert` | `void Insert(int index, object value)` |  |
| `RemoveAt` | `void RemoveAt(int index)` |  |
| `Remove` | `bool Remove(T item)` |  |
| `Remove` | `void Remove(object value)` |  |
| `ToArray` | `T[] ToArray()` |  |
| `CollectionChanged` | `event NotifyCollectionChangedEventHandler CollectionChanged` |  |

### Namespace `System.Windows.Controls`

[`ItemCollectionExtensions`](#itemcollectionextensions)

#### `ItemCollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange(this ItemCollection @this, IEnumerable items)` | Adds a bunch of items. |
| `GetElementByName` | `static FrameworkElement GetElementByName(this ItemCollection @this, string name)` | Gets the element in this collection by it's name tag. |

### Namespace `System.Windows.Controls.Primitives`

[`SelectorExtensions`](#selectorextensions)

#### `SelectorExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `TryCastSelectedValue` | `static bool TryCastSelectedValue<TType>(this Selector @this, ref TType value)` | Tries to cast the selected value into the given type. |

### Namespace `System.Windows.Threading`

[`DispatcherObjectExtensions`](#dispatcherobjectextensions) · [`EventExtensions`](#eventextensions)

#### `DispatcherObjectExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Async` | `static bool Async(this DispatcherObject This, Action action)` | Invokes the action in another thread. |
| `SafelyInvoke` | `static bool SafelyInvoke(this DispatcherObject @this, Action action, bool async = false, DispatcherPriority dispatcherPriority = 9)` | Safely invokes an action on a dispatcher object. |

#### `EventExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AsyncSafeInvoke` | `static void AsyncSafeInvoke(this MulticastDelegate @this, params object[] arguments)` | Invokes an event safely asychroneously from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads. |
| `AsyncSafeInvoke` | `static void AsyncSafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs)` | Invokes an event safely asynchroneously from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads. |
| `SafeInvoke` | `static void SafeInvoke(this MulticastDelegate @this, params object[] arguments)` | Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads. |
| `SafeInvoke` | `static void SafeInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs)` | Invokes an event safely from no matter what thread and makes sure that the subscribers who needs it, get the event invocation in their own threads. |

<!-- API:END -->

## 🔌 Dependencies

- `Backports` (project reference)
- WPF (`UseWpf` enabled in project)

## ⚠️ Limitations

- Windows only — WPF is not available on other platforms.
- Visual-tree helpers must be called on the UI thread unless a member documents otherwise.

## ❤️ Support

If this project saves you time or money, consider supporting its development:

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor-EA4AAA?logo=githubsponsors)](https://github.com/sponsors/Hawkynt)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?logo=paypal)](https://www.paypal.me/hawkynt)

## 📜 License

Licensed under LGPL-3.0-or-later — see the repository [LICENSE](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE).
