# FrameworkExtensions.Backports

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Backports)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Backports)](https://www.nuget.org/packages/FrameworkExtensions.Backports/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

__FrameworkExtensions.Backports__ is a NuGet package that provides a collection of extensions (Polyfills) to ensure that newer compiler features work in older versions of the .NET Framework/Standard/Core. This package allows developers to use modern C# language features and .NET APIs even when working on projects targeting earlier versions.

__Note__: Performance is not a primary concern here. This focuses mainly on functionality and ready-to-be-built without making adjustments to code.

## Features

### Interfaces

* System
  * [IGrouping](https://learn.microsoft.com/dotnet/api/system.linq.igrouping-2)&lt;out TKey, TElement&gt;
  * [IParsable](https://learn.microsoft.com/dotnet/api/system.iparsable-1)
  * [ISpanFormattable](https://learn.microsoft.com/dotnet/api/system.ispanformattable)
  * [ISpanParsable](https://learn.microsoft.com/dotnet/api/system.ispanparsable-1)
* System.Collections
  * [IStructuralComparable](https://learn.microsoft.com/dotnet/api/system.collections.istructuralcomparable)
  * [IStructuralEquatable](https://learn.microsoft.com/dotnet/api/system.collections.istructuralequatable)
* System.Collections.Generic
  * [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)&lt;TKey, TValue&gt;
* System.Runtime.CompilerServices
  * [IAsyncStateMachine](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.iasyncstatemachine)
  * [ICriticalNotifyCompletion](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.icriticalnotifycompletion)
  * [INotifyCompletion](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.inotifycompletion)
  * [ITuple](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.ituple)

### Types

* System
  * [Index](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.ituple)
  * [Lazy](https://learn.microsoft.com/dotnet/api/system.lazy-1)&lt;T&gt;
  * [MathF](https://learn.microsoft.com/dotnet/api/system.mathf)
  * [Range](https://learn.microsoft.com/dotnet/api/system.range)
  * [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)&lt;T&gt;
  * [Span](https://learn.microsoft.com/dotnet/api/system.span-1)&lt;T&gt;
  * [Tuple](https://learn.microsoft.com/dotnet/api/system.tuple)&lt;T&gt; (up to 8 types)
  * [ValueTuple](https://learn.microsoft.com/dotnet/api/system.valuetuple)&lt;T&gt; (up to 8 types)
* System.Buffers
  * [ArrayPool](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)&lt;T&gt;
* System.Collections.Concurrent
  * [ConcurrentBag](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentbag-1)&lt;T&gt;
  * [ConcurrentDictionary](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)&lt;TKey, TValue&gt;
  * [ConcurrentQueue](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentqueue-1)&lt;T&gt;
  * [ConcurrentStack](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentstack-1)&lt;T&gt;
* System.Collections.Generic
  * [HashSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1)&lt;T&gt;
  * [ReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.objectmodel.readonlydictionary-2)&lt;TKey, TValue&gt;
* System.Numerics
  * [BitOperations](https://learn.microsoft.com/dotnet/api/system.numerics.bitoperations)
* System.Runtime.CompilerServices
  * [AsyncTaskMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asynctaskmethodbuilder)&lt;T&gt;
  * [ConfiguredTaskAwaitable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredtaskawaitable)&lt;T&gt;
  * [IsExternalInit](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.isexternalinit)
  * [TaskAwaiter](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.taskawaiter)&lt;T&gt;
  * [Unsafe](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.unsafe)
* System.Runtime.InteropServices
  * [MemoryMarshal](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.memorymarshal)
* System.Runtime.Intrinsics
  * [Vector64](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector64)
  * [Vector64](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector64-1)&lt;T&gt;
  * [Vector128](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector128)
  * [Vector128](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector128-1)&lt;T&gt;
  * [Vector256](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector256)
  * [Vector256](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector256-1)&lt;T&gt;
  * [Vector512](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector512)
  * [Vector512](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.vector512-1)&lt;T&gt;
* System.Runtime.Intrinsics.Arm
  * [AdvSimd](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.arm.advsimd)
* System.Runtime.Intrinsics.X86
  * [Avx](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx)
  * [Avx2](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx2)
  * [Avx512F](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx512f)
  * [Sse](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse)
  * [Sse2](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse2)
  * [Sse3](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse3)
  * [Sse41](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse41)
  * [Sse42](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse42)
  * [Ssse3](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.ssse3)
* System.Threading
  * [ManualResetEventSlim](https://learn.microsoft.com/dotnet/api/system.threading.manualreseteventslim)

### Attributes

* System.Diagnostics
  * [StackTraceHidden](https://learn.microsoft.com/dotnet/api/system.diagnostics.stacktracehiddenattribute)
* System.Diagnostics.CodeAnalysis
  * [DisallowNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.disallownullattribute)
  * [DoesNotReturn](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.doesnotreturnattribute)
  * [NotNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.notnullattribute)
  * [NotNullWhen](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.notnullwhenattribute)
* System.Runtime.CompilerServices
  * [CallerArgumentExpression](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute)
  * [CallerFilePath](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerfilepathattribute)
  * [CallerLineNumber](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerlinenumberattribute)
  * [CallerMemberName](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callermembernameattribute)
  * [Extension](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.extensionattribute)
  * [TupleElementNames](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.tupleelementnamesattribute)

### Delegates

* System
  * [Func](https://learn.microsoft.com/dotnet/api/system.func-1)&lt;T&gt; (up to 16 types)
  * [Action](https://learn.microsoft.com/dotnet/api/system.action)&lt;T&gt; (up to 16 types)

### Methods

* System.Array
  * Span&lt;T&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan-1%28-0%28%29%29)&lt;T&gt;(this T[] @this)
  * Span&lt;T&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan-1%28-0%28%29-system-int32%29)&lt;T&gt;(this T[] @this, int start)
  * Span&lt;T&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan-1%28-0%28%29-system-int32-system-int32%29)&lt;T&gt;(this T[] @this, int start, int length)
  * Span&lt;T&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan-1%28-0%28%29-system-index%29)&lt;T&gt;(this T[] @this, Index startIndex)
  * Span&lt;T&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan-1%28-0%28%29-system-range%29)&lt;T&gt;(this T[] @this, Range range)
* System.ReadOnlySpan
  * bool [SequenceEqual](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sequenceequal#system-memoryextensions-sequenceequal-1%28system-span%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this Span&lt;T&gt; span, ReadOnlySpan&lt;T&gt; other)
  * bool [SequenceEqual](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sequenceequal#system-memoryextensions-sequenceequal-1%28system-readonlyspan%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, ReadOnlySpan&lt;T&gt; other)
  * bool [SequenceEqual](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sequenceequal#system-memoryextensions-sequenceequal-1%28system-readonlyspan%28%28-0%29%29-system-readonlyspan%28%28-0%29%29-system-collections-generic-iequalitycomparer%28%28-0%29%29%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, ReadOnlySpan&lt;T&gt; other, IEqualityComparer&lt;T&gt; comparer)
  * int [IndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.indexof#system-memoryextensions-indexof-1%28system-readonlyspan%28%28-0%29%29-0%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, T value)
  * int [IndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.indexof#system-memoryextensions-indexof-1%28system-readonlyspan%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, ReadOnlySpan&lt;T&gt; value)
  * int [IndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.indexof#system-memoryextensions-indexof%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-stringcomparison%29)(this ReadOnlySpan&lt;char&gt; span, ReadOnlySpan&lt;char&gt; value, StringComparison comparisonType)
  * int [LastIndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.lastindexof#system-memoryextensions-lastindexof-1%28system-readonlyspan%28%28-0%29%29-0%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, T value)
  * int [LastIndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.lastindexof#system-memoryextensions-lastindexof-1%28system-readonlyspan%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, ReadOnlySpan&lt;T&gt; value)
  * int [LastIndexOf](https://learn.microsoft.com/dotnet/api/system.memoryextensions.lastindexof#system-memoryextensions-lastindexof%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-stringcomparison%29)(this ReadOnlySpan&lt;char&gt; span, ReadOnlySpan&lt;char&gt; value, StringComparison comparisonType)
  * bool [StartsWith](https://learn.microsoft.com/dotnet/api/system.memoryextensions.startswith#system-memoryextensions-startswith%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-stringcomparison%29)(this ReadOnlySpan&lt;char&gt; span, ReadOnlySpan&lt;char&gt; value, StringComparison comparisonType)
  * bool [StartsWith](https://learn.microsoft.com/dotnet/api/system.memoryextensions.startswith#system-memoryextensions-startswith-1%28system-readonlyspan%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, ReadOnlySpan&lt;T&gt; value)
  * bool [StartsWith](https://learn.microsoft.com/dotnet/api/system.memoryextensions.startswith#system-memoryextensions-startswith-1%28system-span%28%28-0%29%29-system-readonlyspan%28%28-0%29%29%29)&lt;T&gt;(this Span&lt;T&gt; span, ReadOnlySpan&lt;T&gt; value)
* System.Collections.Concurrent.ConcurrentQueue
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentqueue-1.clear)&lt;T&gt;(this ConcurrentQueue&lt;T&gt;)
* System.Collections.Concurrent.ConcurrentStack
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentstack-1.clear)&lt;T&gt;(this ConcurrentStack&lt;T&gt;)
* System.Collections.Generic.IEnumerable
  * HashSet&lt;TItem&gt; [ToHashSet](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tohashset#system-linq-enumerable-tohashset-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this)
  * HashSet&lt;TItem&gt; [ToHashSet](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tohashset#system-linq-enumerable-tohashset-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-collections-generic-iequalitycomparer%28%28-0%29%29%29)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, IEqualityComparer&lt;TItem&gt; comparer)
* System.Collections.Generic.KeyValuePair
  * void [Deconstruct](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2.deconstruct)&lt;TKey, TValue&gt;(this KeyValuePair&lt;TKey, TValue&gt; @this, out TKey key, out TValue value)
* System.Collections.Generic.Stack
  * bool [TryPop](https://learn.microsoft.com/dotnet/api/system.collections.generic.stack-1.trypop)&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
  * bool [TryPeek](https://learn.microsoft.com/dotnet/api/system.collections.generic.stack-1.trypeek)&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
* System.DateTimeOffset
  * long [ToUnixTimeMilliseconds](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.tounixtimemilliseconds)(this DateTimeOffset @this)
  * long [ToUnixTimeSeconds](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.tounixtimeseconds)(this DateTimeOffset @this)
* System.Diagnostics
  * void [Restart](https://learn.microsoft.com/dotnet/api/system.diagnostics.stopwatch.restart)(this Stopwatch @this)
* System.Enum
  * bool [HasFlag](https://learn.microsoft.com/dotnet/api/system.enum.hasflag)&lt;T&gt;(this T @this, T flag)
* System.IO.DirectoryInfo
  * IEnumerable&lt;FileSystemInfo&gt; [EnumerateFileSystemInfos](https://learn.microsoft.com/dotnet/api/system.io.directoryinfo.enumeratefilesysteminfos#system-io-directoryinfo-enumeratefilesysteminfos)(this DirectoryInfo @this)
  * IEnumerable&lt;FileInfo&gt; [EnumerateFiles](https://learn.microsoft.com/dotnet/api/system.io.directoryinfo.enumeratefiles#system-io-directoryinfo-enumeratefiles)(this DirectoryInfo @this)
  * IEnumerable&lt;DirectoryInfo&gt; [EnumerateDirectories](https://learn.microsoft.com/dotnet/api/system.io.directoryinfo.enumeratedirectories#system-io-directoryinfo-enumeratedirectories)(this DirectoryInfo @this)
  * IEnumerable&lt;FileInfo&gt; [EnumerateFiles](https://learn.microsoft.com/dotnet/api/system.io.directoryinfo.enumeratefiles#system-io-directoryinfo-enumeratefiles%28system-string-system-io-searchoption%29)(this DirectoryInfo @this, string searchPattern, SearchOption searchOption)
  * IEnumerable&lt;DirectoryInfo&gt; [EnumerateDirectories](https://learn.microsoft.com/dotnet/api/system.io.directoryinfo.enumeratedirectories#system-io-directoryinfo-enumeratedirectories%28system-string-system-io-searchoption%29)(this DirectoryInfo @this, string searchPattern, SearchOption searchOption)
* System.IO.FileInfo
  * void [MoveTo](https://learn.microsoft.com/dotnet/api/system.io.fileinfo.moveto#system-io-fileinfo-moveto%28system-string-system-boolean%29)(this FileInfo @this, string destFileName, bool overwrite)
* System.IO.Stream
  * void [CopyTo](https://learn.microsoft.com/dotnet/api/system.io.stream.copyto#system-io-stream-copyto%28system-io-stream%29)(this Stream @this, Stream target)
  * void [Flush](https://learn.microsoft.com/dotnet/api/system.io.filestream.flush#system-io-filestream-flush%28system-boolean%29)(this Stream @this, bool flushToDisk)
  * Task&lt;int&gt; [ReadAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.readasync#system-io-stream-readasync%28system-byte%28%29-system-int32-system-int32%29)(this Stream @this, byte[] buffer, int offset, int count)
  * Task&lt;int&gt; [ReadAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.readasync#system-io-stream-readasync%28system-byte%28%29-system-int32-system-int32-system-threading-cancellationtoken%29)(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
  * Task [WriteAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.writeasync#system-io-stream-writeasync%28system-byte%28%29-system-int32-system-int32%29)(this Stream @this, byte[] buffer, int offset, int count)
  * Task [WriteAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.writeasync#system-io-stream-writeasync%28system-byte%28%29-system-int32-system-int32-system-threading-cancellationtoken%29)(this Stream @this, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
  * Task [CopyToAsync](https://learn.microsoft.com/dotnet/api/system.io.stream.copytoasync#system-io-stream-copytoasync%28system-io-stream-system-int32-system-threading-cancellationtoken%29)(this Stream @this, Stream destination, int bufferSize, CancellationToken cancellationToken)
  * int [Read](https://learn.microsoft.com/dotnet/api/system.io.stream.read#system-io-stream-read%28system-span%28%28system-byte%29%29%29)(this Stream @this, Span&lt;byte&gt; buffer)
  * void [Write](https://learn.microsoft.com/dotnet/api/system.io.stream.write#system-io-stream-write%28system-readonlyspan%28%28system-byte%29%29%29)(this Stream @this, ReadOnlySpan&lt;byte&gt; buffer)
* System.Linq
  * TResult[] [ToArray](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.toarray)&lt;TResult&gt;(this IEnumerable&lt;TResult&gt; @this)
  * IEnumerable&lt;TResult&gt; [Cast](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.cast)&lt;TResult&gt;(this IEnumerable @this)
  * IEnumerable&lt;TSource&gt; [Where](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.where#system-linq-enumerable-where-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * IEnumerable&lt;TSource&gt; [Where](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.where#system-linq-enumerable-where-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-int32-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, int, bool&gt; predicate)
  * IEnumerable&lt;TResult&gt; [Select](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.select#system-linq-enumerable-select-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-1%29%29%29)&lt;TSource, TResult&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TResult&gt; selector)
  * IEnumerable&lt;TResult&gt; [Select](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.select#system-linq-enumerable-select-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-int32-1%29%29%29)&lt;TSource, TResult&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, int, TResult&gt; selector)
  * IEnumerable&lt;TSource&gt; [OrderBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.orderby#system-linq-enumerable-orderby-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-1%29%29%29)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; [OrderBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.orderby#system-linq-enumerable-orderby-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-1%29%29-system-collections-generic-icomparer%28%28-1%29%29%29)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * IEnumerable&lt;TSource&gt; [ThenBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.thenby#system-linq-enumerable-thenby-2%28system-linq-iorderedenumerable%28%28-0%29%29-system-func%28%28-0-1%29%29%29)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; [ThenBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.thenby#system-linq-enumerable-thenby-2%28system-linq-iorderedenumerable%28%28-0%29%29-system-func%28%28-0-1%29%29-system-collections-generic-icomparer%28%28-1%29%29%29)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * TSource [First](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.first#system-linq-enumerable-first-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [First](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.first#system-linq-enumerable-first-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [FirstOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.firstordefault#system-linq-enumerable-firstordefault-1%28system-collections-generic-ienumerable%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [FirstOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.firstordefault#system-linq-enumerable-firstordefault-1%28system-collections-generic-ienumerable%28-0%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, TSource defaultValue)
  * TSource [FirstOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.firstordefault#system-linq-enumerable-firstordefault-1%28system-collections-generic-ienumerable%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [FirstOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.firstordefault#system-linq-enumerable-firstordefault-1%28system-collections-generic-ienumerable%28-0%29%29-system-func%28%28-0-system-boolean%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate, TSource defaultValue)
  * TSource [Single](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.single#system-linq-enumerable-single-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [Single](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.single#system-linq-enumerable-single-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [SingleOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.singleordefault#system-linq-enumerable-singleordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [SingleOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.singleordefault#system-linq-enumerable-singleordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, TSource defaultValue)
  * TSource [SingleOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.singleordefault#system-linq-enumerable-singleordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate, TSource defaultValue)
  * TSource [SingleOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.singleordefault#system-linq-enumerable-singleordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [Last](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.last#system-linq-enumerable-last-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [Last](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.last#system-linq-enumerable-last-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [LastOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.lastordefault#system-linq-enumerable-lastordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * TSource [LastOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.lastordefault#system-linq-enumerable-lastordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, TSource defaultValue)
  * TSource [LastOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.lastordefault#system-linq-enumerable-lastordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TSource [LastOrDefault](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.lastordefault#system-linq-enumerable-lastordefault-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-system-boolean%29%29-0%29)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate, TSource defaultValue)
  * TSource [Min](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.min)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * TItem [MinBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.minby#system-linq-queryable-minby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector)
  * TItem [MinBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.minby#system-linq-queryable-minby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29-system-collections-generic-icomparer%28%28-0%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * TSource [Max](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.max)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * TItem [MaxBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.maxby#system-linq-queryable-maxby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector)
  * TItem [MaxBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.maxby#system-linq-queryable-maxby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29-system-collections-generic-icomparer%28%28-0%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; [GroupBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.groupby#system-linq-enumerable-groupby-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-1%29%29%29)&lt;TSource, TKey&gt;(IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TItem&gt; [Prepend](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.prepend)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * IEnumerable&lt;TItem&gt; [Append](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.append)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * bool [TryGetNonEnumeratedCount](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.trygetnonenumeratedcount)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; source, out int count)
  * IEnumerable&lt;TItem&gt; [Zip](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.zip#system-linq-enumerable-zip-3%28system-collections-generic-ienumerable%28%28-0%29%29-system-collections-generic-ienumerable%28%28-1%29%29-system-func%28%28-0-1-2%29%29%29)&lt;TFirst, TSecond, TResult&gt;(IEnumerable&lt;TFirst&gt; @this, IEnumerable&lt;TSecond&gt; second, Func&lt;TFirst, TSecond, TResult&gt; resultSelector)
* System.Random
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64)(this Random @this)
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64%28system-int64%29)(this Random @this, long maxValue)
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64%28system-int64-system-int64%29)(this Random @this, long minValue, long maxValue)
* System.Reflection.MethodInfo
  * Delegate [CreateDelegate](https://learn.microsoft.com/dotnet/api/system.reflection.methodinfo.createdelegate#system-reflection-methodinfo-createdelegate%28system-type%29)(this MethodInfo @this, Type result)
* System.String
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-string-system-stringcomparison%29)(this string @this, string value, StringComparison comparisonType)
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-char%29)(this string @this, char value)
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-char-system-stringcomparison%29)(this string @this, char value, StringComparison comparisonType)
  * string[] [Split](https://learn.microsoft.com/dotnet/api/system.string.split#system-string-split%28system-char-system-stringsplitoptions%29)(this string @this, char separator, StringSplitOptions options = StringSplitOptions.None)
  * string[] [Split](https://learn.microsoft.com/dotnet/api/system.string.split#system-string-split%28system-char-system-int32-system-stringsplitoptions%29)(this string @this, char separator, int count, StringSplitOptions options = StringSplitOptions.None)
  * string[] [Split](https://learn.microsoft.com/dotnet/api/system.string.split#system-string-split%28system-string-system-stringsplitoptions%29)(this string @this, string separator, StringSplitOptions options = StringSplitOptions.None)
  * string[] [Split](https://learn.microsoft.com/dotnet/api/system.string.split#system-string-split%28system-string-system-int32-system-stringsplitoptions%29)(this string @this, string separator, int count, StringSplitOptions options = StringSplitOptions.None)
  * int [IndexOf](https://learn.microsoft.com/dotnet/api/system.string.indexof#system-string-indexof%28system-char-system-stringcomparison%29)(this string @this, char value, StringComparison comparisonType)
  * ReadOnlySpan&lt;char&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan%28system-string%29)(this string @this)
  * ReadOnlySpan&lt;char&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan%28system-string-system-int32%29)(this string @this, int start)
  * ReadOnlySpan&lt;char&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan%28system-string-system-int32-system-int32%29)(this string @this, int start, int length)
  * ReadOnlySpan&lt;char&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan%28system-string-system-index%29)(this string @this, Index startIndex)
  * ReadOnlySpan&lt;char&gt; [AsSpan](https://learn.microsoft.com/dotnet/api/system.memoryextensions.asspan#system-memoryextensions-asspan%28system-string-system-range%29)(this string @this, Range range)
* System.Text.StringBuilder
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder.clear#system-text-stringbuilder-clear)(this StringBuilder @this)
* System.Threading.Tasks
  * TaskAwaiter [GetAwaiter](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.getawaiter#system-threading-tasks-task-getawaiter)(this Task task)
  * TaskAwaiter&lt;TResult&gt; [GetAwaiter](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.getawaiter)&lt;TResult&gt;(this Task&lt;TResult&gt; task)
  * ConfiguredTaskAwaitable [ConfigureAwait](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.configureawait#system-threading-tasks-task-configureawait%28system-boolean%29)(this Task task, bool continueOnCapturedContext)
  * ConfiguredTaskAwaitable&lt;TResult&gt; [ConfigureAwait](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.configureawait#system-threading-tasks-task-1-configureawait%28system-boolean%29)&lt;TResult&gt;(this Task&lt;TResult&gt; task, bool continueOnCapturedContext)
* System.Threading.WaitHandle
  * void [Dispose](https://learn.microsoft.com/dotnet/api/system.threading.waithandle.dispose#system-threading-waithandle-dispose)(this WaitHandle @this)

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
4. __Remember__: Everything *public* in here should polyfill existing Microsoft functionality and thus should be mentioned in the [Readme](Readme.md) with a link to its original documentation.
5. Submit a pull request.

## License

This project is licensed under the LGPL-3.0-or-later License. See the [LICENSE](../LICENSE) file for details.

## Acknowledgments

We appreciate the contributions of the .NET community in making this package possible.
