# FrameworkExtensions.Backports

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Backports)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Backports)](https://www.nuget.org/packages/FrameworkExtensions.Backports/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

**FrameworkExtensions.Backports** is a NuGet package that provides a collection of extensions (Polyfills) to ensure that newer compiler features work in older versions of the .NET Framework/Standard/Core. This package allows developers to use modern C# language features and .NET APIs even when working on projects targeting earlier versions.

## Features

### Interfaces
* System
  * IGrouping&lt;out TKey, TElement&gt;
* System.Collections
  * IStructuralComparable
  * IStructuralEquatable
* System.Collections.Generic
  * IReadOnlyDictionary&lt;TKey, TValue&gt;
* System.Runtime.CompilerServices
  * IAsyncStateMachine
  * ICriticalNotifyCompletion
  * INotifyCompletion
  * ITuple

### Types
* System
  * Index
  * Lazy&lt;T&gt;
  * Range
  * ReadOnlySpan&lt;T&gt;
  * Span&lt;T&gt;
  * Tuple&lt;T&gt; (up to 8 types)
  * ValueTuple&lt;T&gt; (up to 8 types)
* System.Collections.Concurrent
  * ConcurrentBag&lt;T&gt;
  * ConcurrentDictionary&lt;TKey, TValue&gt;
  * ConcurrentQueue&lt;T&gt;
* System.Collections.Generic
  * HashSet&lt;T&gt;
  * ReadOnlyDictionary&lt;TKey, TValue&gt;
* System.Runtime.CompilerServices
  * AsyncTaskMethodBuilder&lt;T&gt;
  * ConfiguredTaskAwaitable&lt;T&gt;
  * IsExternalInit
  * TaskAwaiter&lt;T&gt;
* System.Threading
  * ManualResetEventSlim

### Attributes
* System.Diagnostics
  * StackTraceHidden
* System.Diagnostics.CodeAnalysis
  * DisallowNull
  * DoesNotReturn
  * NotNull
  * NotNullWhen
* System.Runtime.CompilerServices
  * CallerArgumentExpression
  * CallerFilePath
  * CallerLineNumber
  * CallerMemberName
  * Extension
  * TupleElementNames

### Delegates
* System.Func&lt;T&gt; (up to 16 types)
* System.Action&lt;T&gt; (up to 16 types)

### Methods
* System.Array
  * Span&lt;T&gt; AsSpan&lt;T&gt;(this T[] array)
* System.Collections.Generic.IEnumerable
  * IEnumerable&lt;TItem&gt; Prepend&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * IEnumerable&lt;TItem&gt; Append&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * TItem MaxBy&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector)
  * TItem MaxBy&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * TItem MinBy&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector)
  * TItem MinBy&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * HashSet&lt;TItem&gt; ToHashSet&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this)
  * HashSet&lt;TItem&gt; ToHashSet&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, IEqualityComparer&lt;TItem&gt; comparer)
* System.Collections.Generic.Stack
  * bool TryPop&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
  * bool TryPeek&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
* System.Diagnostics
  * void Restart(this Stopwatch @this)
* System.Enum
  * bool HasFlag&lt;T&gt;(this T @this, T flag)
* System.IO.DirectoryInfo
  * IEnumerable&lt;FileSystemInfo&gt; EnumerateFileSystemInfos(this DirectoryInfo @this)
  * IEnumerable&lt;FileInfo&gt; EnumerateFiles(this DirectoryInfo @this)
  * IEnumerable&lt;DirectoryInfo&gt; EnumerateDirectories(this DirectoryInfo @this)
  * IEnumerable&lt;FileInfo&gt; EnumerateFiles(this DirectoryInfo @this, string searchPattern, SearchOption searchOption)
  * IEnumerable&lt;DirectoryInfo&gt; EnumerateDirectories(this DirectoryInfo @this, string searchPattern, SearchOption searchOption)
* System.IO.FileInfo
  * void MoveTo(this FileInfo @this, string destFileName, bool overwrite)
* System.IO.Stream
  * void CopyTo(this Stream @this, Stream target)
  * void Flush(this Stream @this, bool flush)
  * Task&lt;int&gt; ReadAsync(this Stream @this, byte[] buffer, int offset, int count)
  * Task&lt;int&gt; ReadAsync(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
  * Task WriteAsync(this Stream @this, byte[] buffer, int offset, int count)
  * Task WriteAsync(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
  * Task CopyToAsync(this Stream @this, Stream destination, int bufferSize, CancellationToken cancellationToken)
  * int Read(this Stream @this, Span&lt;byte&gt; buffer)
* System.Linq
  * TResult[] ToArray&lt;TResult&gt;(this IEnumerable&lt;TResult&gt; @this)
  * IEnumerable&lt;TResult&gt; Cast&lt;TResult&gt;(this IEnumerable @this)
  * IEnumerable&lt;TSource&gt; Where&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * IEnumerable&lt;TResult&gt; Select&lt;TSource, TResult&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TResult&gt; selector)
  * IEnumerable&lt;TResult&gt; Select&lt;TSource, TResult&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, int, TResult&gt; selector)
  * IEnumerable&lt;TSource&gt; OrderBy&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; ThenBy&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector)
  * TSource Single&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource Last&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource Min&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * TSource Max&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; GroupBy&lt;TSource, TKey&gt;(IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector)
* System.Random
  * long NextInt64(this Random @this)
  * long NextInt64(this Random @this, long maxValue)
  * long NextInt64(this Random @this, long minValue, long maxValue)
* System.Reflection.MethodInfo
  * Delegate CreateDelegate(this MethodInfo @this, Type result)
* System.String
  * bool Contains(this string @this, string value, StringComparison comparisonType)
* System.Text.StringBuilder
  * void Clear(this StringBuilder @this)
* System.Threading.Tasks
  * TaskAwaiter GetAwaiter(this Task task)
  * TaskAwaiter&lt;TResult&gt; GetAwaiter&lt;TResult&gt;(this Task&lt;TResult&gt; task)
  * ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext)
  * ConfiguredTaskAwaitable&lt;TResult&gt; ConfigureAwait&lt;TResult&gt;(this Task&lt;TResult&gt; task, bool continueOnCapturedContext)
* System.Threading.WaitHandle
  * void Dispose(this WaitHandle @this)

## Installation

You can install the FrameworkExtensions.Backports package via NuGet Package Manager or the .NET CLI:

### NuGet Package Manager

```sh
Install-Package FrameworkExtensions.Backports
```

### .NET CLI

```ps
dotnet add package FrameworkExtensions.Backports
```

## Usage

Below are some examples of how to use the features provided by this package. Note that the namespaces are kept original, so no additional using directives are needed.

### Range and Index

```csharp
public class Program
{
    public static void Main()
    {
        int[] numbers = { 1, 2, 3, 4, 5 };
        var slice = numbers[1..^1];
        Console.WriteLine(string.Join(", ", slice)); // Output: 2, 3, 4
    }
}
```

### Lazy Initialization

```csharp
public class Program
{
    private static Lazy<int> lazyValue = new Lazy<int>(() => ComputeValue());

    public static void Main()
    {
        Console.WriteLine(lazyValue.Value); // Output: Computed Value
    }

    private static int ComputeValue()
    {
        Console.WriteLine("Computed Value");
        return 42;
    }
}
```

### LINQ Methods

```csharp
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

        var max = numbers.Max();
        var minBy = numbers.MinBy(n => n);

        Console.WriteLine($"Max: {max}");    // Output: Max: 5
        Console.WriteLine($"MinBy: {minBy}");// Output: MinBy: 1
    }
}
```

### Task Extensions

```csharp
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        Task task = Task.Delay(1000);
        await task.ConfigureAwait(false);
        Console.WriteLine("Task completed");
    }
}
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch with a descriptive name.
3. Make your changes.
4. Submit a pull request.

## License

This project is licensed under the LGPL-3.0-or-later License. See the [LICENSE](../LICENSE) file for details.

## Acknowledgments

We appreciate the contributions of the .NET community in making this package possible.
