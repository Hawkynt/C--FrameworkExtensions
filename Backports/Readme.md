# FrameworkExtensions.Backports

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)

[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Backports)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Backports)](https://www.nuget.org/packages/FrameworkExtensions.Backports/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

__FrameworkExtensions.Backports__ is a NuGet package that provides a collection of extensions (Polyfills) to ensure that newer compiler features work in older versions of the .NET Framework/Standard/Core. This package allows developers to use modern C# language features and .NET APIs even when working on projects targeting earlier versions down to Net2.0.

__Note__: Performance is not a primary concern here. This focuses mainly on functionality and ready-to-be-built without making adjustments to code.

## Architecture

### Official Package Integration

To avoid conflicts with official Microsoft BCL backport packages and ensure optimal compatibility, **FrameworkExtensions.Backports** uses a hybrid approach:

- **When official packages exist**: For target frameworks where Microsoft provides official backport packages (such as System.Memory, System.Buffers, System.ValueTuple, etc.), this package automatically references and uses those official implementations.

- **When official packages don't exist**: For older target frameworks (like .NET Framework 2.0, 3.5) or for features not covered by official packages, this package provides custom backport implementations.

This approach means:
- ✅ **Users only need to reference `FrameworkExtensions.Backports`** - all necessary dependencies are included automatically
- ✅ **No package conflicts** - official implementations are used when available, avoiding type conflicts
- ✅ **Better performance and compatibility** - official Microsoft implementations are optimized and thoroughly tested
- ✅ **Seamless experience** - the same API surface works across all target frameworks

### Official Packages Included

The following official Microsoft packages are conditionally referenced based on your target framework:

**Active Packages:**
- **System.Memory** - Provides `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`, and `MemoryMarshal` (.NET Framework 4.5+, .NET Standard 2.0)
- **System.Buffers** - Provides `ArrayPool<T>` (.NET Framework 4.5+, .NET Standard 2.0)
- **System.ValueTuple** - Provides `ValueTuple` types (.NET Framework 4.0+)
- **System.Runtime.CompilerServices.Unsafe** - Provides the `Unsafe` class (.NET Framework 4.5+, .NET Standard 1.0+)
- **System.Numerics.Vectors** - Provides `Vector` types (.NET Framework 4.5+, .NET Standard 2.0)
- **System.Threading.Tasks.Extensions** - Provides `ValueTask` (.NET Framework 4.5+, .NET Standard 2.0)
- **Microsoft.Bcl.HashCode** - Provides `HashCode` (.NET Framework 4.6.1+, .NET Standard 2.0)

**Deprecated Packages (still included for compatibility):**
- **Microsoft.Bcl** - Provides `CallerMemberNameAttribute` and related attributes (.NET Framework 4.0 only)
- **Microsoft.Bcl.Async** - Provides `TaskAwaiter` and async/await support (.NET Framework 4.0 only)

> **Note:** Microsoft.Bcl and Microsoft.Bcl.Async are officially deprecated but are still included for .NET Framework 4.0 to avoid conflicts with existing projects that may reference these packages.

For target frameworks where these packages are not available (e.g., .NET Framework 2.0/3.5), custom implementations are provided.

## Features

### Interfaces

* System
  * [IGrouping](https://learn.microsoft.com/dotnet/api/system.linq.igrouping-2)&lt;out TKey, TElement&gt;
  * [ILookup](https://learn.microsoft.com/dotnet/api/system.linq.ilookup-2)&lt;TKey, TElement&gt;
  * [IParsable](https://learn.microsoft.com/dotnet/api/system.iparsable-1)
  * [ISpanFormattable](https://learn.microsoft.com/dotnet/api/system.ispanformattable)
  * [ISpanParsable](https://learn.microsoft.com/dotnet/api/system.ispanparsable-1)

* System.Collections
  * [IStructuralComparable](https://learn.microsoft.com/dotnet/api/system.collections.istructuralcomparable)
  * [IStructuralEquatable](https://learn.microsoft.com/dotnet/api/system.collections.istructuralequatable)

* System
  * [IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

* System.Collections.Generic
  * [IAsyncEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)&lt;T&gt;
  * [IAsyncEnumerator](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerator-1)&lt;T&gt;
  * [IReadOnlyCollection](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlycollection-1)&lt;T&gt;
  * [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)&lt;TKey, TValue&gt;
  * [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)&lt;T&gt;
  * [IReadOnlySet](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlyset-1)&lt;T&gt;
  * [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset-1)&lt;T&gt;

* System.Runtime.CompilerServices
  * [IAsyncStateMachine](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.iasyncstatemachine)
  * [ICriticalNotifyCompletion](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.icriticalnotifycompletion)
  * [INotifyCompletion](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.inotifycompletion)
  * [ITuple](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.ituple)

### Types

* System
  * [DateOnly](https://learn.microsoft.com/dotnet/api/system.dateonly)
  * [Half](https://learn.microsoft.com/dotnet/api/system.half)
  * [Index](https://learn.microsoft.com/dotnet/api/system.index)
  * [Int128](https://learn.microsoft.com/dotnet/api/system.int128)
  * [Lazy](https://learn.microsoft.com/dotnet/api/system.lazy-1)&lt;T&gt;
  * [MathF](https://learn.microsoft.com/dotnet/api/system.mathf)
  * [Range](https://learn.microsoft.com/dotnet/api/system.range)
  * [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)&lt;T&gt;
  * [Span](https://learn.microsoft.com/dotnet/api/system.span-1)&lt;T&gt;
  * [TimeOnly](https://learn.microsoft.com/dotnet/api/system.timeonly)
  * [Tuple](https://learn.microsoft.com/dotnet/api/system.tuple)&lt;T&gt; (up to 8 types)
  * [UInt128](https://learn.microsoft.com/dotnet/api/system.uint128)
  * [ValueTuple](https://learn.microsoft.com/dotnet/api/system.valuetuple)&lt;T&gt; (up to 8 types)

* System
  * [Memory](https://learn.microsoft.com/dotnet/api/system.memory-1)&lt;T&gt;
  * [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory-1)&lt;T&gt;
  * [MemoryHandle](https://learn.microsoft.com/dotnet/api/system.buffers.memoryhandle)
  * [SequencePosition](https://learn.microsoft.com/dotnet/api/system.sequenceposition)

* System.Buffers
  * [ArrayPool](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)&lt;T&gt;
  * [IMemoryOwner](https://learn.microsoft.com/dotnet/api/system.buffers.imemoryowner-1)&lt;T&gt;
  * [MemoryManager](https://learn.microsoft.com/dotnet/api/system.buffers.memorymanager-1)&lt;T&gt;
  * [MemoryPool](https://learn.microsoft.com/dotnet/api/system.buffers.memorypool-1)&lt;T&gt;
  * [ReadOnlySequence](https://learn.microsoft.com/dotnet/api/system.buffers.readonlysequence-1)&lt;T&gt;
  * [ReadOnlySequenceSegment](https://learn.microsoft.com/dotnet/api/system.buffers.readonlysequencesegment-1)&lt;T&gt;
  * [SequenceReader](https://learn.microsoft.com/dotnet/api/system.buffers.sequencereader-1)&lt;T&gt;
  * [SearchValues](https://learn.microsoft.com/dotnet/api/system.buffers.searchvalues-1)&lt;T&gt;

* System.Collections.Concurrent
  * [ConcurrentBag](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentbag-1)&lt;T&gt;
  * [ConcurrentDictionary](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2)&lt;TKey, TValue&gt;
  * [ConcurrentQueue](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentqueue-1)&lt;T&gt;
  * [ConcurrentStack](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentstack-1)&lt;T&gt;
  * [EnumerablePartitionerOptions](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.enumerablepartitioneroptions)
  * [OrderablePartitioner](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.orderablepartitioner-1)&lt;TSource&gt;
  * [Partitioner](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.partitioner)
  * [Partitioner](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.partitioner-1)&lt;TSource&gt;

* System.Collections.Frozen
  * [FrozenDictionary](https://learn.microsoft.com/dotnet/api/system.collections.frozen.frozendictionary-2)&lt;TKey, TValue&gt;
  * [FrozenSet](https://learn.microsoft.com/dotnet/api/system.collections.frozen.frozenset-1)&lt;T&gt;

* System.Collections.Generic
  * [HashSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1)&lt;T&gt;
  * [OrderedDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ordereddictionary-2)&lt;TKey, TValue&gt;
  * [PriorityQueue](https://learn.microsoft.com/dotnet/api/system.collections.generic.priorityqueue-2)&lt;TElement, TPriority&gt;
  * [ReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.objectmodel.readonlydictionary-2)&lt;TKey, TValue&gt;

* System.IO.Compression
  * [CompressionLevel](https://learn.microsoft.com/dotnet/api/system.io.compression.compressionlevel)
  * [ZipArchive](https://learn.microsoft.com/dotnet/api/system.io.compression.ziparchive)
  * [ZipArchiveEntry](https://learn.microsoft.com/dotnet/api/system.io.compression.ziparchiveentry)
  * [ZipArchiveMode](https://learn.microsoft.com/dotnet/api/system.io.compression.ziparchivemode)
  * [ZipFile](https://learn.microsoft.com/dotnet/api/system.io.compression.zipfile)
  * [ZipFileExtensions](https://learn.microsoft.com/dotnet/api/system.io.compression.zipfileextensions)

* System.Numerics
  * [BitOperations](https://learn.microsoft.com/dotnet/api/system.numerics.bitoperations)
  * [Matrix3x2](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2)
  * [Matrix4x4](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4)
  * [Plane](https://learn.microsoft.com/dotnet/api/system.numerics.plane)
  * [Quaternion](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion)
  * [Vector2](https://learn.microsoft.com/dotnet/api/system.numerics.vector2)
  * [Vector3](https://learn.microsoft.com/dotnet/api/system.numerics.vector3)
  * [Vector4](https://learn.microsoft.com/dotnet/api/system.numerics.vector4)

* System.Runtime.CompilerServices
  * [AsyncIteratorMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asynciteratormethodbuilder)
  * [AsyncTaskMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asynctaskmethodbuilder)&lt;T&gt;
  * [AsyncValueTaskMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asyncvaluetaskmethodbuilder)
  * [AsyncValueTaskMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asyncvaluetaskmethodbuilder-1)&lt;TResult&gt;
  * [ConfiguredCancelableAsyncEnumerable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredcancelableasyncenumerable-1)&lt;T&gt;
  * [ConfiguredTaskAwaitable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredtaskawaitable)&lt;T&gt;
  * [ConfiguredValueTaskAwaitable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredvaluetaskawaitable)
  * [ConfiguredValueTaskAwaitable](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.configuredvaluetaskawaitable-1)&lt;TResult&gt;
  * [IsExternalInit](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.isexternalinit)
  * [TaskAwaiter](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.taskawaiter)&lt;T&gt;
  * [Unsafe](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.unsafe)
  * [ValueTaskAwaiter](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.valuetaskawaiter)
  * [ValueTaskAwaiter](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.valuetaskawaiter-1)&lt;TResult&gt;

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
  * [ArmBase](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.arm.armbase)

* System.Runtime.Intrinsics.X86
  * [Aes](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.aes)
  * [Avx](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx)
  * [Avx2](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx2)
  * [Avx512F](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.avx512f)
  * [Bmi1](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.bmi1)
  * [Bmi2](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.bmi2)
  * [Lzcnt](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.lzcnt)
  * [Pclmulqdq](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.pclmulqdq)
  * [Popcnt](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.popcnt)
  * [Sse](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse)
  * [Sse2](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse2)
  * [Sse3](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse3)
  * [Sse41](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse41)
  * [Sse42](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.sse42)
  * [Ssse3](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.ssse3)
  * [X86Base](https://learn.microsoft.com/dotnet/api/system.runtime.intrinsics.x86.x86base)

* System.Threading
  * [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)
  * [CancellationTokenSource](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource)
  * [ManualResetEventSlim](https://learn.microsoft.com/dotnet/api/system.threading.manualreseteventslim)

* System.Threading.Tasks
  * [Parallel](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel)
  * [ParallelLoopResult](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallelloopresult)
  * [ParallelLoopState](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallelloopstate)
  * [ParallelOptions](https://learn.microsoft.com/dotnet/api/system.threading.tasks.paralleloptions)
  * [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
  * [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)&lt;TResult&gt;
  * [TaskCompletionSource](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskcompletionsource-1)&lt;TResult&gt;
  * [TaskContinuationOptions](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskcontinuationoptions)
  * [TaskCreationOptions](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskcreationoptions)
  * [TaskFactory](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskfactory)
  * [TaskScheduler](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskscheduler)
  * [TaskStatus](https://learn.microsoft.com/dotnet/api/system.threading.tasks.taskstatus)
  * [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
  * [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)&lt;TResult&gt;

* System
  * [AggregateException](https://learn.microsoft.com/dotnet/api/system.aggregateexception)

### Attributes

* System.Diagnostics
  * [StackTraceHidden](https://learn.microsoft.com/dotnet/api/system.diagnostics.stacktracehiddenattribute)

* System.Diagnostics.CodeAnalysis
  * [AllowNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.allownullattribute)
  * [ConstantExpected](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.constantexpectedattribute)
  * [DisallowNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.disallownullattribute)
  * [DoesNotReturn](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.doesnotreturnattribute)
  * [DoesNotReturnIf](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.doesnotreturnifattribute)
  * [Experimental](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.experimentalattribute)
  * [MaybeNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.maybenullattribute)
  * [MaybeNullWhen](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.maybenullwhenattribute)
  * [MemberNotNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.membernotnullattribute)
  * [MemberNotNullWhen](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.membernotnullwhenattribute)
  * [NotNull](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.notnullattribute)
  * [NotNullWhen](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.notnullwhenattribute)
  * [SetsRequiredMembers](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.setsrequiredmembersattribute)
  * [StringSyntax](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.stringsyntaxattribute)
  * [UnscopedRef](https://learn.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.unscopedrefattribute)

* System.Runtime.CompilerServices
  * [AsyncIteratorStateMachine](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asynciteratorstatemachineattribute)
  * [AsyncMethodBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.asyncmethodbuilderattribute)
  * [CallerArgumentExpression](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute)
  * [CallerFilePath](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerfilepathattribute)
  * [CallerLineNumber](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callerlinenumberattribute)
  * [CallerMemberName](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.callermembernameattribute)
  * [CollectionBuilder](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.collectionbuilderattribute)
  * [CompilerFeatureRequired](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.compilerfeaturerequiredattribute)
  * [EnumeratorCancellation](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.enumeratorcancellationattribute)
  * [Extension](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.extensionattribute)
  * [InlineArray](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.inlinearrayattribute)
  * [InterpolatedStringHandler](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.interpolatedstringhandlerattribute)
  * [InterpolatedStringHandlerArgument](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.interpolatedstringhandlerargumentattribute)
  * [ModuleInitializer](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.moduleinitializerattribute)
  * [OverloadResolutionPriority](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.overloadresolutionpriorityattribute)
  * [RefSafetyRules](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.refsafetyrulesattribute)
  * [RequiredMember](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.requiredmemberattribute)
  * [TupleElementNames](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.tupleelementnamesattribute)

### Delegates

* System
  * [Func](https://learn.microsoft.com/dotnet/api/system.func-1)&lt;T&gt; (up to 16 types)
  * [Action](https://learn.microsoft.com/dotnet/api/system.action)&lt;T&gt; (up to 16 types)

### Static Methods

* System.Array
  * T[] [Empty](https://learn.microsoft.com/dotnet/api/system.array.empty)&lt;T&gt;()
  * void [Fill](https://learn.microsoft.com/dotnet/api/system.array.fill#system-array-fill-1%28-0%28%29--0%29)&lt;T&gt;(T[] array, T value)
  * void [Fill](https://learn.microsoft.com/dotnet/api/system.array.fill#system-array-fill-1%28-0%28%29--0-system-int32-system-int32%29)&lt;T&gt;(T[] array, T value, int startIndex, int count)
  * int [MaxLength](https://learn.microsoft.com/dotnet/api/system.array.maxlength) (static property)

* System.BitConverter
  * bool [TryWriteBytes](https://learn.microsoft.com/dotnet/api/system.bitconverter.trywritebytes)(Span&lt;byte&gt; destination, int value)
  * bool [TryWriteBytes](https://learn.microsoft.com/dotnet/api/system.bitconverter.trywritebytes)(Span&lt;byte&gt; destination, long value)
  * bool [TryWriteBytes](https://learn.microsoft.com/dotnet/api/system.bitconverter.trywritebytes)(Span&lt;byte&gt; destination, short value)
  * bool [TryWriteBytes](https://learn.microsoft.com/dotnet/api/system.bitconverter.trywritebytes)(Span&lt;byte&gt; destination, float value)
  * bool [TryWriteBytes](https://learn.microsoft.com/dotnet/api/system.bitconverter.trywritebytes)(Span&lt;byte&gt; destination, double value)

* System.Char
  * bool [IsAscii](https://learn.microsoft.com/dotnet/api/system.char.isascii)(char c)
  * bool [IsAsciiDigit](https://learn.microsoft.com/dotnet/api/system.char.isasciidigit)(char c)
  * bool [IsAsciiLetter](https://learn.microsoft.com/dotnet/api/system.char.isasciiletter)(char c)
  * bool [IsAsciiLetterLower](https://learn.microsoft.com/dotnet/api/system.char.isasciiletterlower)(char c)
  * bool [IsAsciiLetterUpper](https://learn.microsoft.com/dotnet/api/system.char.isasciiletterupper)(char c)
  * bool [IsAsciiHexDigit](https://learn.microsoft.com/dotnet/api/system.char.isasciihexdigit)(char c)
  * bool [IsAsciiHexDigitLower](https://learn.microsoft.com/dotnet/api/system.char.isasciihexdigitlower)(char c)
  * bool [IsAsciiHexDigitUpper](https://learn.microsoft.com/dotnet/api/system.char.isasciihexdigitupper)(char c)

* System.Convert
  * string [ToHexString](https://learn.microsoft.com/dotnet/api/system.convert.tohexstring)(byte[] inArray)
  * string [ToHexString](https://learn.microsoft.com/dotnet/api/system.convert.tohexstring)(byte[] inArray, int offset, int length)
  * string [ToHexString](https://learn.microsoft.com/dotnet/api/system.convert.tohexstring)(ReadOnlySpan&lt;byte&gt; bytes)
  * byte[] [FromHexString](https://learn.microsoft.com/dotnet/api/system.convert.fromhexstring)(string s)
  * byte[] [FromHexString](https://learn.microsoft.com/dotnet/api/system.convert.fromhexstring)(ReadOnlySpan&lt;char&gt; chars)

* System.Guid
  * bool [TryParse](https://learn.microsoft.com/dotnet/api/system.guid.tryparse#system-guid-tryparse%28system-readonlyspan%28%28system-char%29%29-system-guid%40%29)(ReadOnlySpan&lt;char&gt; input, out Guid result)
  * Guid [Parse](https://learn.microsoft.com/dotnet/api/system.guid.parse#system-guid-parse%28system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; input)

* System.IO.Path
  * string [Join](https://learn.microsoft.com/dotnet/api/system.io.path.join#system-io-path-join%28system-string-system-string%29)(string path1, string path2)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.io.path.join#system-io-path-join%28system-string-system-string-system-string%29)(string path1, string path2, string path3)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.io.path.join#system-io-path-join%28system-string-system-string-system-string-system-string%29)(string path1, string path2, string path3, string path4)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.io.path.join#system-io-path-join%28system-string%28%29%29)(params string[] paths)
  * string [Combine](https://learn.microsoft.com/dotnet/api/system.io.path.combine#system-io-path-combine%28system-string-system-string-system-string%29)(string path1, string path2, string path3)
  * string [Combine](https://learn.microsoft.com/dotnet/api/system.io.path.combine#system-io-path-combine%28system-string-system-string-system-string-system-string%29)(string path1, string path2, string path3, string path4)
  * string [Combine](https://learn.microsoft.com/dotnet/api/system.io.path.combine#system-io-path-combine%28system-string%28%29%29)(params string[] paths)
  * string [GetRelativePath](https://learn.microsoft.com/dotnet/api/system.io.path.getrelativepath)(string relativeTo, string path)
  * bool [Exists](https://learn.microsoft.com/dotnet/api/system.io.path.exists)(string? path)
  * bool [EndsInDirectorySeparator](https://learn.microsoft.com/dotnet/api/system.io.path.endsindirectoryseparator)(string? path)
  * bool [EndsInDirectorySeparator](https://learn.microsoft.com/dotnet/api/system.io.path.endsindirectoryseparator)(ReadOnlySpan&lt;char&gt; path)
  * string [TrimEndingDirectorySeparator](https://learn.microsoft.com/dotnet/api/system.io.path.trimendingdirectoryseparator)(string? path)
  * ReadOnlySpan&lt;char&gt; [TrimEndingDirectorySeparator](https://learn.microsoft.com/dotnet/api/system.io.path.trimendingdirectoryseparator)(ReadOnlySpan&lt;char&gt; path)

* System.Math
  * T [Clamp](https://learn.microsoft.com/dotnet/api/system.math.clamp)(T value, T min, T max) - for byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal
  * double [CopySign](https://learn.microsoft.com/dotnet/api/system.math.copysign)(double x, double y)
  * double [FusedMultiplyAdd](https://learn.microsoft.com/dotnet/api/system.math.fusedmultiplyadd)(double x, double y, double z)
  * double [ScaleB](https://learn.microsoft.com/dotnet/api/system.math.scaleb)(double x, int n)
  * int [ILogB](https://learn.microsoft.com/dotnet/api/system.math.ilogb)(double x)
  * double [ReciprocalEstimate](https://learn.microsoft.com/dotnet/api/system.math.reciprocalestimate)(double x)
  * double [ReciprocalSqrtEstimate](https://learn.microsoft.com/dotnet/api/system.math.reciprocalsqrtestimate)(double x)

* System.MathF
  * float [CopySign](https://learn.microsoft.com/dotnet/api/system.mathf.copysign)(float x, float y)
  * float [FusedMultiplyAdd](https://learn.microsoft.com/dotnet/api/system.mathf.fusedmultiplyadd)(float x, float y, float z)
  * float [ReciprocalEstimate](https://learn.microsoft.com/dotnet/api/system.mathf.reciprocalestimate)(float x)
  * float [ReciprocalSqrtEstimate](https://learn.microsoft.com/dotnet/api/system.mathf.reciprocalsqrtestimate)(float x)

* System.DateTime
  * DateTime [UnixEpoch](https://learn.microsoft.com/dotnet/api/system.datetime.unixepoch)
  * DateTime [Parse](https://learn.microsoft.com/dotnet/api/system.datetime.parse#system-datetime-parse%28system-readonlyspan%28%28system-char%29%29-system-iformatprovider-system-globalization-datetimestyles%29)
  * DateTime [ParseExact](https://learn.microsoft.com/dotnet/api/system.datetime.parseexact)
  * bool [TryParse](https://learn.microsoft.com/dotnet/api/system.datetime.tryparse)
  * bool [TryParseExact](https://learn.microsoft.com/dotnet/api/system.datetime.tryparseexact)
  * DateTime [FromDateAndTime](https://learn.microsoft.com/dotnet/api/system.datetime.-ctor#system-datetime-ctor%28system-dateonly-system-timeonly%29)

* System.Single (float)
  * bool [IsFinite](https://learn.microsoft.com/dotnet/api/system.single.isfinite)
  * bool [IsNegative](https://learn.microsoft.com/dotnet/api/system.single.isnegative)
  * bool [IsNormal](https://learn.microsoft.com/dotnet/api/system.single.isnormal)
  * bool [IsSubnormal](https://learn.microsoft.com/dotnet/api/system.single.issubnormal)
  * float [Lerp](https://learn.microsoft.com/dotnet/api/system.single.lerp)
  * float [DegreesToRadians](https://learn.microsoft.com/dotnet/api/system.single.degreestoradians)
  * float [RadiansToDegrees](https://learn.microsoft.com/dotnet/api/system.single.radianstodegrees)

* System.Double (double)
  * bool [IsFinite](https://learn.microsoft.com/dotnet/api/system.double.isfinite)
  * bool [IsNegative](https://learn.microsoft.com/dotnet/api/system.double.isnegative)
  * bool [IsNormal](https://learn.microsoft.com/dotnet/api/system.double.isnormal)
  * bool [IsSubnormal](https://learn.microsoft.com/dotnet/api/system.double.issubnormal)

* System.Random
  * Random [Shared](https://learn.microsoft.com/dotnet/api/system.random.shared) (static property)

* System.TimeSpan
  * bool [TryParse](https://learn.microsoft.com/dotnet/api/system.timespan.tryparse#system-timespan-tryparse%28system-readonlyspan%28%28system-char%29%29-system-timespan%40%29)(ReadOnlySpan&lt;char&gt; input, out TimeSpan result)
  * TimeSpan [Parse](https://learn.microsoft.com/dotnet/api/system.timespan.parse#system-timespan-parse%28system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; input)
  * string [ToString](https://learn.microsoft.com/dotnet/api/system.timespan.tostring#system-timespan-tostring%28system-string-system-iformatprovider%29)(this TimeSpan @this, string? format, IFormatProvider? formatProvider)
  * bool [TryFormat](https://learn.microsoft.com/dotnet/api/system.timespan.tryformat)(this TimeSpan @this, Span&lt;char&gt; destination, out int charsWritten, ...)

* System.Version
  * bool [TryParse](https://learn.microsoft.com/dotnet/api/system.version.tryparse#system-version-tryparse%28system-readonlyspan%28%28system-char%29%29-system-version%40%29)(ReadOnlySpan&lt;char&gt; input, out Version? result)
  * Version [Parse](https://learn.microsoft.com/dotnet/api/system.version.parse#system-version-parse%28system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; input)

* System.Int32, System.Int64, System.Double, System.Decimal
  * bool [TryParse](https://learn.microsoft.com/dotnet/api/system.int32.tryparse#system-int32-tryparse%28system-readonlyspan%28%28system-char%29%29-system-int32%40%29)(ReadOnlySpan&lt;char&gt; s, out T result)
  * T [Parse](https://learn.microsoft.com/dotnet/api/system.int32.parse#system-int32-parse%28system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; s)

* System.Int32 (int)
  * int [Abs](https://learn.microsoft.com/dotnet/api/system.int32.abs)(int value)
  * (int Quotient, int Remainder) [DivRem](https://learn.microsoft.com/dotnet/api/system.int32.divrem)(int left, int right)
  * bool [IsEvenInteger](https://learn.microsoft.com/dotnet/api/system.int32.iseveninteger)(int value)
  * bool [IsNegative](https://learn.microsoft.com/dotnet/api/system.int32.isnegative)(int value)
  * bool [IsOddInteger](https://learn.microsoft.com/dotnet/api/system.int32.isoddinteger)(int value)
  * bool [IsPositive](https://learn.microsoft.com/dotnet/api/system.int32.ispositive)(int value)
  * int [LeadingZeroCount](https://learn.microsoft.com/dotnet/api/system.int32.leadingzerocount)(int value)
  * int [Log2](https://learn.microsoft.com/dotnet/api/system.int32.log2)(int value)
  * int [Max](https://learn.microsoft.com/dotnet/api/system.int32.max)(int x, int y)
  * int [MaxMagnitude](https://learn.microsoft.com/dotnet/api/system.int32.maxmagnitude)(int x, int y)
  * int [Min](https://learn.microsoft.com/dotnet/api/system.int32.min)(int x, int y)
  * int [MinMagnitude](https://learn.microsoft.com/dotnet/api/system.int32.minmagnitude)(int x, int y)
  * int [PopCount](https://learn.microsoft.com/dotnet/api/system.int32.popcount)(int value)
  * int [RotateLeft](https://learn.microsoft.com/dotnet/api/system.int32.rotateleft)(int value, int rotateAmount)
  * int [RotateRight](https://learn.microsoft.com/dotnet/api/system.int32.rotateright)(int value, int rotateAmount)
  * int [Sign](https://learn.microsoft.com/dotnet/api/system.int32.sign)(int value)
  * int [TrailingZeroCount](https://learn.microsoft.com/dotnet/api/system.int32.trailingzerocount)(int value)
  * int [Clamp](https://learn.microsoft.com/dotnet/api/system.int32.clamp)(int value, int min, int max)
  * int [CopySign](https://learn.microsoft.com/dotnet/api/system.int32.copysign)(int value, int sign)

* System.Int64 (long)
  * long [Abs](https://learn.microsoft.com/dotnet/api/system.int64.abs)(long value)
  * (long Quotient, long Remainder) [DivRem](https://learn.microsoft.com/dotnet/api/system.int64.divrem)(long left, long right)
  * bool [IsEvenInteger](https://learn.microsoft.com/dotnet/api/system.int64.iseveninteger)(long value)
  * bool [IsNegative](https://learn.microsoft.com/dotnet/api/system.int64.isnegative)(long value)
  * bool [IsOddInteger](https://learn.microsoft.com/dotnet/api/system.int64.isoddinteger)(long value)
  * bool [IsPositive](https://learn.microsoft.com/dotnet/api/system.int64.ispositive)(long value)
  * int [LeadingZeroCount](https://learn.microsoft.com/dotnet/api/system.int64.leadingzerocount)(long value)
  * int [Log2](https://learn.microsoft.com/dotnet/api/system.int64.log2)(long value)
  * long [Max](https://learn.microsoft.com/dotnet/api/system.int64.max)(long x, long y)
  * long [MaxMagnitude](https://learn.microsoft.com/dotnet/api/system.int64.maxmagnitude)(long x, long y)
  * long [Min](https://learn.microsoft.com/dotnet/api/system.int64.min)(long x, long y)
  * long [MinMagnitude](https://learn.microsoft.com/dotnet/api/system.int64.minmagnitude)(long x, long y)
  * int [PopCount](https://learn.microsoft.com/dotnet/api/system.int64.popcount)(long value)
  * long [RotateLeft](https://learn.microsoft.com/dotnet/api/system.int64.rotateleft)(long value, int rotateAmount)
  * long [RotateRight](https://learn.microsoft.com/dotnet/api/system.int64.rotateright)(long value, int rotateAmount)
  * int [Sign](https://learn.microsoft.com/dotnet/api/system.int64.sign)(long value)
  * int [TrailingZeroCount](https://learn.microsoft.com/dotnet/api/system.int64.trailingzerocount)(long value)
  * long [Clamp](https://learn.microsoft.com/dotnet/api/system.int64.clamp)(long value, long min, long max)
  * long [CopySign](https://learn.microsoft.com/dotnet/api/system.int64.copysign)(long value, long sign)

* System.String
  * string [Create](https://learn.microsoft.com/dotnet/api/system.string.create)&lt;TState&gt;(int length, TState state, SpanAction&lt;char, TState&gt; action)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.string.join#system-string-join%28system-char-system-string%28%29%29)(char separator, params string[] values)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.string.join#system-string-join-1%28system-char-system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;T&gt;(char separator, IEnumerable&lt;T&gt; values)
  * string [Join](https://learn.microsoft.com/dotnet/api/system.string.join#system-string-join%28system-char-system-object%28%29%29)(char separator, params object[] values)
  * string [Concat](https://learn.microsoft.com/dotnet/api/system.string.concat#system-string-concat%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; str0, ReadOnlySpan&lt;char&gt; str1)
  * string [Concat](https://learn.microsoft.com/dotnet/api/system.string.concat#system-string-concat%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; str0, ReadOnlySpan&lt;char&gt; str1, ReadOnlySpan&lt;char&gt; str2)
  * string [Concat](https://learn.microsoft.com/dotnet/api/system.string.concat#system-string-concat%28system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29-system-readonlyspan%28%28system-char%29%29%29)(ReadOnlySpan&lt;char&gt; str0, ReadOnlySpan&lt;char&gt; str1, ReadOnlySpan&lt;char&gt; str2, ReadOnlySpan&lt;char&gt; str3)

* System.ArgumentNullException
  * void [ThrowIfNull](https://learn.microsoft.com/dotnet/api/system.argumentnullexception.throwifnull)(object? argument, string? paramName = null)
  * void [ThrowIfNull](https://learn.microsoft.com/dotnet/api/system.argumentnullexception.throwifnull)(void* argument, string? paramName = null)

* System.ArgumentException
  * void [ThrowIfNullOrEmpty](https://learn.microsoft.com/dotnet/api/system.argumentexception.throwifnullorempty)(string? argument, string? paramName = null)
  * void [ThrowIfNullOrWhiteSpace](https://learn.microsoft.com/dotnet/api/system.argumentexception.throwifnullorwhitespace)(string? argument, string? paramName = null)

* System.ArgumentOutOfRangeException
  * void [ThrowIfZero](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifzero)&lt;T&gt;(T value, string? paramName = null)
  * void [ThrowIfNegative](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifnegative)&lt;T&gt;(T value, string? paramName = null)
  * void [ThrowIfNegativeOrZero](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifnegativeorzero)&lt;T&gt;(T value, string? paramName = null)
  * void [ThrowIfGreaterThan](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthan)&lt;T&gt;(T value, T other, string? paramName = null)
  * void [ThrowIfGreaterThanOrEqual](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifgreaterthanorequal)&lt;T&gt;(T value, T other, string? paramName = null)
  * void [ThrowIfLessThan](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwiflessthan)&lt;T&gt;(T value, T other, string? paramName = null)
  * void [ThrowIfLessThanOrEqual](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwiflessthanorequal)&lt;T&gt;(T value, T other, string? paramName = null)
  * void [ThrowIfEqual](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifequal)&lt;T&gt;(T value, T other, string? paramName = null)
  * void [ThrowIfNotEqual](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception.throwifnotequal)&lt;T&gt;(T value, T other, string? paramName = null)

* System.ObjectDisposedException
  * void [ThrowIf](https://learn.microsoft.com/dotnet/api/system.objectdisposedexception.throwif)(bool condition, object instance)
  * void [ThrowIf](https://learn.microsoft.com/dotnet/api/system.objectdisposedexception.throwif)(bool condition, Type type)

* System.Threading.Interlocked
  * int [And](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.and#system-threading-interlocked-and%28system-int32%40-system-int32%29)(ref int location1, int value)
  * long [And](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.and#system-threading-interlocked-and%28system-int64%40-system-int64%29)(ref long location1, long value)
  * uint [And](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.and#system-threading-interlocked-and%28system-uint32%40-system-uint32%29)(ref uint location1, uint value)
  * ulong [And](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.and#system-threading-interlocked-and%28system-uint64%40-system-uint64%29)(ref ulong location1, ulong value)
  * int [Or](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.or#system-threading-interlocked-or%28system-int32%40-system-int32%29)(ref int location1, int value)
  * long [Or](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.or#system-threading-interlocked-or%28system-int64%40-system-int64%29)(ref long location1, long value)
  * uint [Or](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.or#system-threading-interlocked-or%28system-uint32%40-system-uint32%29)(ref uint location1, uint value)
  * ulong [Or](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.or#system-threading-interlocked-or%28system-uint64%40-system-uint64%29)(ref ulong location1, ulong value)

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
  * int [Count](https://learn.microsoft.com/dotnet/api/system.memoryextensions.count)&lt;T&gt;(this Span&lt;T&gt; span, T value)
  * int [Count](https://learn.microsoft.com/dotnet/api/system.memoryextensions.count)&lt;T&gt;(this ReadOnlySpan&lt;T&gt; span, T value)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;T&gt;(Span&lt;T&gt; span)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;T&gt;(Span&lt;T&gt; span, Comparison&lt;T&gt; comparison)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;T, TComparer&gt;(Span&lt;T&gt; span, TComparer comparer)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;TKey, TValue&gt;(Span&lt;TKey&gt; keys, Span&lt;TValue&gt; items)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;TKey, TValue&gt;(Span&lt;TKey&gt; keys, Span&lt;TValue&gt; items, Comparison&lt;TKey&gt; comparison)
  * void [Sort](https://learn.microsoft.com/dotnet/api/system.memoryextensions.sort)&lt;TKey, TValue, TComparer&gt;(Span&lt;TKey&gt; keys, Span&lt;TValue&gt; items, TComparer comparer)
  * void [Reverse](https://learn.microsoft.com/dotnet/api/system.memoryextensions.reverse)&lt;T&gt;(Span&lt;T&gt; span)

* System.Collections.Concurrent.ConcurrentBag
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentbag-1.clear)&lt;T&gt;(this ConcurrentBag&lt;T&gt;)

* System.Collections.Concurrent.ConcurrentDictionary
  * TValue [AddOrUpdate](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary-2.addorupdate#system-collections-concurrent-concurrentdictionary-2-addorupdate-1%28-0-system-func%28%28-0-0-1%29%29-system-func%28%28-0-1-0-1%29%29-0%29)&lt;TKey, TValue, TArg&gt;(this ConcurrentDictionary&lt;TKey, TValue&gt; @this, TKey key, Func&lt;TKey, TArg, TValue&gt; addValueFactory, Func&lt;TKey, TValue, TArg, TValue&gt; updateValueFactory, TArg factoryArgument)

* System.Collections.Concurrent.ConcurrentQueue
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentqueue-1.clear)&lt;T&gt;(this ConcurrentQueue&lt;T&gt;)

* System.Collections.Concurrent.ConcurrentStack
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentstack-1.clear)&lt;T&gt;(this ConcurrentStack&lt;T&gt;)

* System.Collections.Generic.IEnumerable
  * HashSet&lt;TItem&gt; [ToHashSet](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tohashset#system-linq-enumerable-tohashset-1%28system-collections-generic-ienumerable%28%28-0%29%29%29)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this)
  * HashSet&lt;TItem&gt; [ToHashSet](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tohashset#system-linq-enumerable-tohashset-1%28system-collections-generic-ienumerable%28%28-0%29%29-system-collections-generic-iequalitycomparer%28%28-0%29%29%29)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, IEqualityComparer&lt;TItem&gt; comparer)

* System.Collections.Generic.KeyValuePair
  * KeyValuePair&lt;TKey, TValue&gt; [Create](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair.create)&lt;TKey, TValue&gt;(TKey key, TValue value)
  * void [Deconstruct](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2.deconstruct)&lt;TKey, TValue&gt;(this KeyValuePair&lt;TKey, TValue&gt; @this, out TKey key, out TValue value)

* System.Collections.Generic.Stack
  * bool [TryPop](https://learn.microsoft.com/dotnet/api/system.collections.generic.stack-1.trypop)&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
  * bool [TryPeek](https://learn.microsoft.com/dotnet/api/system.collections.generic.stack-1.trypeek)&lt;TItem&gt;(this Stack&lt;TItem&gt; @this, out TItem result)
  * int [EnsureCapacity](https://learn.microsoft.com/dotnet/api/system.collections.generic.stack-1.ensurecapacity)&lt;T&gt;(this Stack&lt;T&gt; @this, int capacity)

* System.Collections.Generic.Queue
  * bool [TryDequeue](https://learn.microsoft.com/dotnet/api/system.collections.generic.queue-1.trydequeue)&lt;TItem&gt;(this Queue&lt;TItem&gt; @this, out TItem result)
  * int [EnsureCapacity](https://learn.microsoft.com/dotnet/api/system.collections.generic.queue-1.ensurecapacity)&lt;T&gt;(this Queue&lt;T&gt; @this, int capacity)
  * bool [TryPeek](https://learn.microsoft.com/dotnet/api/system.collections.generic.queue-1.trypeek)&lt;TItem&gt;(this Queue&lt;TItem&gt; @this, out TItem result)

* System.Collections.Generic.Dictionary
  * int [EnsureCapacity](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2.ensurecapacity)&lt;TKey, TValue&gt;(this Dictionary&lt;TKey, TValue&gt; @this, int capacity)

* System.Collections.Generic.HashSet
  * int [EnsureCapacity](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset-1.ensurecapacity)&lt;T&gt;(this HashSet&lt;T&gt; @this, int capacity)

* System.Collections.Generic.List
  * List&lt;T&gt; [Slice](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.slice)&lt;T&gt;(this List&lt;T&gt; @this, int start, int length)
  * int [EnsureCapacity](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.ensurecapacity)&lt;T&gt;(this List&lt;T&gt; @this, int capacity)

* System.DateTime
  * int [Microsecond](https://learn.microsoft.com/dotnet/api/system.datetime.microsecond)(this DateTime @this)
  * int [Nanosecond](https://learn.microsoft.com/dotnet/api/system.datetime.nanosecond)(this DateTime @this)
  * DateTime [AddMicroseconds](https://learn.microsoft.com/dotnet/api/system.datetime.addmicroseconds)(this DateTime @this, double value)
  * bool [TryFormat](https://learn.microsoft.com/dotnet/api/system.datetime.tryformat)(this DateTime @this, Span&lt;char&gt; destination, out int charsWritten, ...)
  * void [Deconstruct](https://learn.microsoft.com/dotnet/api/system.datetime.deconstruct)(this DateTime @this, out DateOnly date, out TimeOnly time)
  * void [Deconstruct](https://learn.microsoft.com/dotnet/api/system.datetime.deconstruct)(this DateTime @this, out int year, out int month, out int day)

* System.DateTimeOffset
  * long [ToUnixTimeMilliseconds](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.tounixtimemilliseconds)(this DateTimeOffset @this)
  * long [ToUnixTimeSeconds](https://learn.microsoft.com/dotnet/api/system.datetimeoffset.tounixtimeseconds)(this DateTimeOffset @this)

* System.Diagnostics.Stopwatch
  * void [Restart](https://learn.microsoft.com/dotnet/api/system.diagnostics.stopwatch.restart)(this Stopwatch @this)
  * TimeSpan [GetElapsedTime](https://learn.microsoft.com/dotnet/api/system.diagnostics.stopwatch.getelapsedtime#system-diagnostics-stopwatch-getelapsedtime%28system-int64%29)(long startingTimestamp)
  * TimeSpan [GetElapsedTime](https://learn.microsoft.com/dotnet/api/system.diagnostics.stopwatch.getelapsedtime#system-diagnostics-stopwatch-getelapsedtime%28system-int64-system-int64%29)(long startingTimestamp, long endingTimestamp)

* System.Environment
  * long [TickCount64](https://learn.microsoft.com/dotnet/api/system.environment.tickcount64)
  * int [ProcessId](https://learn.microsoft.com/dotnet/api/system.environment.processid)
  * string? [ProcessPath](https://learn.microsoft.com/dotnet/api/system.environment.processpath)
  * bool [IsPrivilegedProcess](https://learn.microsoft.com/dotnet/api/system.environment.isprivilegedprocess)

* System.GC
  * T[] [AllocateArray](https://learn.microsoft.com/dotnet/api/system.gc.allocatearray)&lt;T&gt;(int length, bool pinned = false)

* System.Enum
  * bool [HasFlag](https://learn.microsoft.com/dotnet/api/system.enum.hasflag)&lt;T&gt;(this T @this, T flag)
  * T[] [GetValues](https://learn.microsoft.com/dotnet/api/system.enum.getvalues)&lt;T&gt;()
  * string[] [GetNames](https://learn.microsoft.com/dotnet/api/system.enum.getnames)&lt;T&gt;()
  * string? [GetName](https://learn.microsoft.com/dotnet/api/system.enum.getname)&lt;T&gt;(T value)
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.enum.isdefined)&lt;T&gt;(T value)

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
  * void [ReadExactly](https://learn.microsoft.com/dotnet/api/system.io.stream.readexactly)(this Stream @this, byte[] buffer, int offset, int count)
  * void [ReadExactly](https://learn.microsoft.com/dotnet/api/system.io.stream.readexactly)(this Stream @this, Span&lt;byte&gt; buffer)
  * int [ReadAtLeast](https://learn.microsoft.com/dotnet/api/system.io.stream.readatleast)(this Stream @this, Span&lt;byte&gt; buffer, int minimumBytes, bool throwOnEndOfStream = true)

* System.IO.TextWriter
  * void [Write](https://learn.microsoft.com/dotnet/api/system.io.textwriter.write#system-io-textwriter-write%28system-readonlyspan%28%28system-char%29%29%29)(this TextWriter @this, ReadOnlySpan&lt;char&gt; buffer)
  * void [WriteLine](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writeline#system-io-textwriter-writeline%28system-readonlyspan%28%28system-char%29%29%29)(this TextWriter @this, ReadOnlySpan&lt;char&gt; buffer)
  * Task [WriteAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writeasync#system-io-textwriter-writeasync%28system-string%29)(this TextWriter @this, string? value)
  * Task [WriteAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writeasync#system-io-textwriter-writeasync%28system-char%29)(this TextWriter @this, char value)
  * Task [WriteAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writeasync#system-io-textwriter-writeasync%28system-char%28%29-system-int32-system-int32%29)(this TextWriter @this, char[] buffer, int index, int count)
  * Task [WriteLineAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writelineasync#system-io-textwriter-writelineasync)(this TextWriter @this)
  * Task [WriteLineAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writelineasync#system-io-textwriter-writelineasync%28system-string%29)(this TextWriter @this, string? value)
  * Task [WriteLineAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writelineasync#system-io-textwriter-writelineasync%28system-char%29)(this TextWriter @this, char value)
  * Task [WriteLineAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.writelineasync#system-io-textwriter-writelineasync%28system-char%28%29-system-int32-system-int32%29)(this TextWriter @this, char[] buffer, int index, int count)
  * Task [FlushAsync](https://learn.microsoft.com/dotnet/api/system.io.textwriter.flushasync)(this TextWriter @this)

* System.IO.TextReader
  * int [Read](https://learn.microsoft.com/dotnet/api/system.io.textreader.read#system-io-textreader-read%28system-span%28%28system-char%29%29%29)(this TextReader @this, Span&lt;char&gt; buffer)
  * Task&lt;string?&gt; [ReadLineAsync](https://learn.microsoft.com/dotnet/api/system.io.textreader.readlineasync#system-io-textreader-readlineasync)(this TextReader @this)
  * Task&lt;string&gt; [ReadToEndAsync](https://learn.microsoft.com/dotnet/api/system.io.textreader.readtoendasync#system-io-textreader-readtoendasync)(this TextReader @this)
  * Task&lt;int&gt; [ReadAsync](https://learn.microsoft.com/dotnet/api/system.io.textreader.readasync#system-io-textreader-readasync%28system-char%28%29-system-int32-system-int32%29)(this TextReader @this, char[] buffer, int index, int count)
  * Task&lt;int&gt; [ReadBlockAsync](https://learn.microsoft.com/dotnet/api/system.io.textreader.readblockasync#system-io-textreader-readblockasync%28system-char%28%29-system-int32-system-int32%29)(this TextReader @this, char[] buffer, int index, int count)

* System.IO.Directory
  * DirectoryInfo [CreateTempSubdirectory](https://learn.microsoft.com/dotnet/api/system.io.directory.createtempsubdirectory)(string? prefix = null)

* System.Type
  * bool [IsAssignableTo](https://learn.microsoft.com/dotnet/api/system.type.isassignableto)(this Type @this, Type? targetType)

* System.Text.StringBuilder
  * StringBuilder [Append](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder.append#system-text-stringbuilder-append%28system-readonlyspan%28%28system-char%29%29%29)(this StringBuilder @this, ReadOnlySpan&lt;char&gt; value)
  * StringBuilder [Insert](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder.insert#system-text-stringbuilder-insert%28system-int32-system-readonlyspan%28%28system-char%29%29%29)(this StringBuilder @this, int index, ReadOnlySpan&lt;char&gt; value)

* System.Linq
  * TResult[] [ToArray](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.toarray)&lt;TResult&gt;(this IEnumerable&lt;TResult&gt; @this)
  * List&lt;TResult&gt; [ToList](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tolist)&lt;TResult&gt;(this IEnumerable&lt;TResult&gt; @this)
  * ILookup&lt;TKey, TSource&gt; [ToLookup](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tolookup)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector)
  * ILookup&lt;TKey, TElement&gt; [ToLookup](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.tolookup)&lt;TSource, TKey, TElement&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TKey&gt; keySelector, Func&lt;TSource, TElement&gt; elementSelector)
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
  * bool [Any](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.any)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * bool [Any](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.any)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * bool [All](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.all)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * int [Count](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.count)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this)
  * int [Count](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.count)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, bool&gt; predicate)
  * TAccumulate [Aggregate](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.aggregate)&lt;TSource, TAccumulate&gt;(this IEnumerable&lt;TSource&gt; @this, TAccumulate seed, Func&lt;TAccumulate, TSource, TAccumulate&gt; func)
  * TResult [Aggregate](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.aggregate)&lt;TSource, TAccumulate, TResult&gt;(this IEnumerable&lt;TSource&gt; @this, TAccumulate seed, Func&lt;TAccumulate, TSource, TAccumulate&gt; func, Func&lt;TAccumulate, TResult&gt; resultSelector)
  * TSource [Aggregate](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.aggregate)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; @this, Func&lt;TSource, TSource, TSource&gt; func)
  * TItem [MaxBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.maxby#system-linq-queryable-maxby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector)
  * TItem [MaxBy](https://learn.microsoft.com/dotnet/api/system.linq.queryable.maxby#system-linq-queryable-maxby-2%28system-linq-iqueryable%28%28-0%29%29-system-linq-expressions-expression%28%28system-func%28%28-0-1%29%29%29%29-system-collections-generic-icomparer%28%28-0%29%29%29)&lt;TItem, TKey&gt;(this IEnumerable&lt;TItem&gt; @this, Func&lt;TItem, TKey&gt; keySelector, IComparer&lt;TKey&gt; comparer)
  * IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; [GroupBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.groupby#system-linq-enumerable-groupby-2%28system-collections-generic-ienumerable%28%28-0%29%29-system-func%28%28-0-1%29%29%29)&lt;TSource, TKey&gt;(IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TItem&gt; [Prepend](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.prepend)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * IEnumerable&lt;TItem&gt; [Append](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.append)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; @this, TItem item)
  * bool [TryGetNonEnumeratedCount](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.trygetnonenumeratedcount)&lt;TItem&gt;(this IEnumerable&lt;TItem&gt; source, out int count)
  * IEnumerable&lt;TItem&gt; [Zip](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.zip#system-linq-enumerable-zip-3%28system-collections-generic-ienumerable%28%28-0%29%29-system-collections-generic-ienumerable%28%28-1%29%29-system-func%28%28-0-1-2%29%29%29)&lt;TFirst, TSecond, TResult&gt;(IEnumerable&lt;TFirst&gt; @this, IEnumerable&lt;TSecond&gt; second, Func&lt;TFirst, TSecond, TResult&gt; resultSelector)
  * IEnumerable&lt;TSource[]&gt; [Chunk](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.chunk)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source, int size)
  * IEnumerable&lt;TSource&gt; [DistinctBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.distinctby)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; [ExceptBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.exceptby)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; first, IEnumerable&lt;TKey&gt; second, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; [IntersectBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.intersectby)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; first, IEnumerable&lt;TKey&gt; second, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;TSource&gt; [UnionBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.unionby)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; first, IEnumerable&lt;TSource&gt; second, Func&lt;TSource, TKey&gt; keySelector)
  * IOrderedEnumerable&lt;T&gt; [Order](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.order)&lt;T&gt;(this IEnumerable&lt;T&gt; source)
  * IOrderedEnumerable&lt;T&gt; [OrderDescending](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.orderdescending)&lt;T&gt;(this IEnumerable&lt;T&gt; source)
  * IEnumerable&lt;(int Index, TSource Item)&gt; [Index](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.index)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * IEnumerable&lt;KeyValuePair&lt;TKey, int&gt;&gt; [CountBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.countby)&lt;TSource, TKey&gt;(this IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector)
  * IEnumerable&lt;KeyValuePair&lt;TKey, TAccumulate&gt;&gt; [AggregateBy](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.aggregateby)&lt;TSource, TKey, TAccumulate&gt;(this IEnumerable&lt;TSource&gt; source, Func&lt;TSource, TKey&gt; keySelector, Func&lt;TKey, TAccumulate&gt; seed, Func&lt;TAccumulate, TSource, TAccumulate&gt; func)
  * IEnumerable&lt;TSource&gt; [Shuffle](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.shuffle)&lt;TSource&gt;(this IEnumerable&lt;TSource&gt; source)
  * IEnumerable&lt;TResult&gt; [LeftJoin](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.leftjoin)&lt;TOuter, TInner, TKey, TResult&gt;(this IEnumerable&lt;TOuter&gt; outer, IEnumerable&lt;TInner&gt; inner, Func&lt;TOuter, TKey&gt; outerKeySelector, Func&lt;TInner, TKey&gt; innerKeySelector, Func&lt;TOuter, TInner, TResult&gt; resultSelector)
  * IEnumerable&lt;TResult&gt; [RightJoin](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.rightjoin)&lt;TOuter, TInner, TKey, TResult&gt;(this IEnumerable&lt;TOuter&gt; outer, IEnumerable&lt;TInner&gt; inner, Func&lt;TOuter, TKey&gt; outerKeySelector, Func&lt;TInner, TKey&gt; innerKeySelector, Func&lt;TOuter, TInner, TResult&gt; resultSelector)
  * IEnumerable&lt;TSource&gt; [Reverse](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.reverse)&lt;TSource&gt;(this TSource[] source)
  * IEnumerable&lt;T&gt; [Sequence](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.sequence)&lt;T&gt;(T start, T endInclusive, T step)
  * IEnumerable&lt;T&gt; [InfiniteSequence](https://learn.microsoft.com/dotnet/api/system.linq.enumerable.infinitesequence)&lt;T&gt;(T start, T step)

* System.Random
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64)(this Random @this)
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64%28system-int64%29)(this Random @this, long maxValue)
  * long [NextInt64](https://learn.microsoft.com/dotnet/api/system.random.nextint64#system-random-nextint64%28system-int64-system-int64%29)(this Random @this, long minValue, long maxValue)
  * float [NextSingle](https://learn.microsoft.com/dotnet/api/system.random.nextsingle)(this Random @this)
  * void [NextBytes](https://learn.microsoft.com/dotnet/api/system.random.nextbytes#system-random-nextbytes%28system-span%28%28system-byte%29%29%29)(this Random @this, Span&lt;byte&gt; buffer)
  * T[] [GetItems](https://learn.microsoft.com/dotnet/api/system.random.getitems)&lt;T&gt;(this Random @this, T[] choices, int length)
  * void [GetItems](https://learn.microsoft.com/dotnet/api/system.random.getitems)&lt;T&gt;(this Random @this, ReadOnlySpan&lt;T&gt; choices, Span&lt;T&gt; destination)
  * void [Shuffle](https://learn.microsoft.com/dotnet/api/system.random.shuffle)&lt;T&gt;(this Random @this, T[] values)
  * void [Shuffle](https://learn.microsoft.com/dotnet/api/system.random.shuffle)&lt;T&gt;(this Random @this, Span&lt;T&gt; values)
  * string [GetHexString](https://learn.microsoft.com/dotnet/api/system.random.gethexstring#system-random-gethexstring%28system-int32-system-boolean%29)(this Random @this, int length, bool lowercase = false)
  * void [GetHexString](https://learn.microsoft.com/dotnet/api/system.random.gethexstring#system-random-gethexstring%28system-span%28%28system-char%29%29-system-boolean%29)(this Random @this, Span&lt;char&gt; destination, bool lowercase = false)
  * string [GetString](https://learn.microsoft.com/dotnet/api/system.random.getstring)(this Random @this, ReadOnlySpan&lt;char&gt; choices, int length)

* System.Reflection.Assembly  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-assembly-system-type%29)(this Assembly element, Type attributeType)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-assembly%29)&lt;T&gt;(this Assembly element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-assembly%29)(this Assembly element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-assembly-system-type%29)(this Assembly element, Type attributeType)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-assembly%29)&lt;T&gt;(this Assembly element)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-assembly-system-type%29)(this Assembly element, Type attributeType)

* System.Reflection.Module  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-module-system-type%29)(this Module element, Type attributeType)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-module%29)&lt;T&gt;(this Module element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-module%29)(this Module element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-module-system-type%29)(this Module element, Type attributeType)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-module%29)&lt;T&gt;(this Module element)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-module-system-type%29)(this Module element, Type attributeType)

* System.Reflection.MemberInfo  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-memberinfo-system-type%29)(this MemberInfo element, Type attributeType)  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-memberinfo-system-type-system-boolean%29)(this MemberInfo element, Type attributeType, bool inherit)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-memberinfo%29)&lt;T&gt;(this MemberInfo element)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-memberinfo-system-boolean%29)&lt;T&gt;(this MemberInfo element, bool inherit)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo%29)(this MemberInfo element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo-system-boolean%29)(this MemberInfo element, bool inherit)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo-system-type%29)(this MemberInfo element, Type attributeType)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo-system-type-system-boolean%29)(this MemberInfo element, Type attributeType, bool inherit)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo%29)&lt;T&gt;(this MemberInfo element)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-memberinfo-system-boolean%29)&lt;T&gt;(this MemberInfo element, bool inherit)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-memberinfo-system-type%29)(this MemberInfo element, Type attributeType)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-memberinfo-system-type-system-boolean%29)(this MemberInfo element, Type attributeType, bool inherit)

* System.Reflection.ParameterInfo  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-parameterinfo-system-type%29)(this ParameterInfo element, Type attributeType)  
  * Attribute [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-parameterinfo-system-type-system-boolean%29)(this ParameterInfo element, Type attributeType, bool inherit)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-parameterinfo%29)&lt;T&gt;(this ParameterInfo element)  
  * T [GetCustomAttribute](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattribute#system-reflection-customattributeextensions-getcustomattribute%28system-reflection-parameterinfo-system-boolean%29)&lt;T&gt;(this ParameterInfo element, bool inherit)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo%29)(this ParameterInfo element)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo-system-boolean%29)(this ParameterInfo element, bool inherit)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo-system-type%29)(this ParameterInfo element, Type attributeType)  
  * IEnumerable&lt;Attribute&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo-system-type-system-boolean%29)(this ParameterInfo element, Type attributeType, bool inherit)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo%29)&lt;T&gt;(this ParameterInfo element)  
  * IEnumerable&lt;T&gt; [GetCustomAttributes](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.getcustomattributes#system-reflection-customattributeextensions-getcustomattributes%28system-reflection-parameterinfo-system-boolean%29)&lt;T&gt;(this ParameterInfo element, bool inherit)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-parameterinfo-system-type%29)(this ParameterInfo element, Type attributeType)  
  * bool [IsDefined](https://learn.microsoft.com/dotnet/api/system.reflection.customattributeextensions.isdefined#system-reflection-customattributeextensions-isdefined%28system-reflection-parameterinfo-system-type-system-boolean%29)(this ParameterInfo element, Type attributeType, bool inherit)

* System.Reflection.MethodInfo
  * Delegate [CreateDelegate](https://learn.microsoft.com/dotnet/api/system.reflection.methodinfo.createdelegate#system-reflection-methodinfo-createdelegate%28system-type%29)(this MethodInfo @this, Type result)

* System.Reflection.PropertyInfo
  * object [GetValue](https://learn.microsoft.com/dotnet/api/system.reflection.propertyinfo.getvalue#system-reflection-propertyinfo-getvalue%28system-object%29)(this PropertyInfo @this, object obj)
  * void [SetValue](https://learn.microsoft.com/dotnet/api/system.reflection.propertyinfo.setvalue#system-reflection-propertyinfo-setvalue%28system-object-system-object%29)(this PropertyInfo @this, object obj, object value)

* System.String
  * int [GetHashCode](https://learn.microsoft.com/dotnet/api/system.string.gethashcode#system-string-gethashcode%28system-stringcomparison%29)(this string @this, StringComparison comparisonType)
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-string-system-stringcomparison%29)(this string @this, string value, StringComparison comparisonType)
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-char%29)(this string @this, char value)
  * bool [Contains](https://learn.microsoft.com/dotnet/api/system.string.contains#system-string-contains%28system-char-system-stringcomparison%29)(this string @this, char value, StringComparison comparisonType)
  * bool [StartsWith](https://learn.microsoft.com/dotnet/api/system.string.startswith#system-string-startswith%28system-char%29)(this string @this, char value)
  * bool [EndsWith](https://learn.microsoft.com/dotnet/api/system.string.endswith#system-string-endswith%28system-char%29)(this string @this, char value)
  * string [Trim](https://learn.microsoft.com/dotnet/api/system.string.trim#system-string-trim%28system-char%29)(this string @this, char trimChar)
  * string [TrimStart](https://learn.microsoft.com/dotnet/api/system.string.trimstart#system-string-trimstart%28system-char%29)(this string @this, char trimChar)
  * string [TrimEnd](https://learn.microsoft.com/dotnet/api/system.string.trimend#system-string-trimend%28system-char%29)(this string @this, char trimChar)
  * string [ReplaceLineEndings](https://learn.microsoft.com/dotnet/api/system.string.replacelineendings#system-string-replacelineendings)(this string @this)
  * string [ReplaceLineEndings](https://learn.microsoft.com/dotnet/api/system.string.replacelineendings#system-string-replacelineendings%28system-string%29)(this string @this, string replacementText)
  * void [CopyTo](https://learn.microsoft.com/dotnet/api/system.string.copyto#system-string-copyto%28system-span%28%28system-char%29%29%29)(this string @this, Span&lt;char&gt; destination)
  * bool [TryCopyTo](https://learn.microsoft.com/dotnet/api/system.string.trycopyto)(this string @this, Span&lt;char&gt; destination)
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

* System.Numerics.Matrix3x2
  * float [GetElement](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.getelement)(this Matrix3x2 @this, int row, int column)
  * Matrix3x2 [WithElement](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.withelement)(this Matrix3x2 @this, int row, int column, float value)
  * Vector2 [GetRow](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.getrow)(this Matrix3x2 @this, int index)
  * Matrix3x2 [WithRow](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.withrow)(this Matrix3x2 @this, int index, Vector2 value)
  * Vector2 [X](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.x) / [Y](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.y) / [Z](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.z) (extension properties)
  * Matrix3x2 [WithX](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.withx)(this Matrix3x2 @this, Vector2 value) / WithY / WithZ
  * Matrix3x2 [Create](https://learn.microsoft.com/dotnet/api/system.numerics.matrix3x2.create)(...) - multiple overloads

* System.Numerics.Matrix4x4
  * Vector4 [GetRow](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.getrow)(this Matrix4x4 @this, int index)
  * Matrix4x4 [WithRow](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.withrow)(this Matrix4x4 @this, int index, Vector4 value)
  * Vector4 [X](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.x) / [Y](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.y) / [Z](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.z) / [W](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.w) (extension properties)
  * Matrix4x4 [WithX](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.withx)(this Matrix4x4 @this, Vector4 value) / WithY / WithZ / WithW
  * Matrix4x4 [Create](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.create)(...) - multiple overloads
  * Matrix4x4 [CreateBillboardLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createbillboardlefthanded)(...)
  * Matrix4x4 [CreateConstrainedBillboardLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createconstrainedbillboardlefthanded)(...)
  * Matrix4x4 [CreateLookAtLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createlookatlefthanded)(...)
  * Matrix4x4 [CreateLookToLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createlooktolefthanded)(...)
  * Matrix4x4 [CreateOrthographicLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createorthographiclefthanded)(...)
  * Matrix4x4 [CreateOrthographicOffCenterLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createorthographicoffcenterlefthanded)(...)
  * Matrix4x4 [CreatePerspectiveLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createperspectivelefthanded)(...)
  * Matrix4x4 [CreatePerspectiveFieldOfViewLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createperspectivefieldofviewlefthanded)(...)
  * Matrix4x4 [CreatePerspectiveOffCenterLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createperspectiveoffcenterlefthanded)(...)
  * Matrix4x4 [CreateViewportLeftHanded](https://learn.microsoft.com/dotnet/api/system.numerics.matrix4x4.createviewportlefthanded)(...)

* System.Numerics.Plane
  * Plane [Create](https://learn.microsoft.com/dotnet/api/system.numerics.plane.create)(Vector4 value)
  * Plane [Create](https://learn.microsoft.com/dotnet/api/system.numerics.plane.create)(Vector3 normal, float d)
  * Plane [Create](https://learn.microsoft.com/dotnet/api/system.numerics.plane.create)(float x, float y, float z, float d)

* System.Numerics.Quaternion
  * Quaternion [Zero](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion.zero) (static property)
  * float [GetElement](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion.item)(this Quaternion @this, int index)
  * Quaternion [WithElement](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion.item)(this Quaternion @this, int index, float value)
  * Quaternion [Create](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion.create)(Vector3 vectorPart, float scalarPart)
  * Quaternion [Create](https://learn.microsoft.com/dotnet/api/system.numerics.quaternion.create)(float x, float y, float z, float w)

* System.RuntimeServices.CompilerServices.RuntimeHelpers
  * T[] [GetSubArray](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.runtimehelpers.getsubarray)&lt;T&gt;(T[] array, Range range)
  * int [OffsetToStringData](https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.runtimehelpers.offsettostringdata)

* System.Text.StringBuilder
  * void [Clear](https://learn.microsoft.com/dotnet/api/system.text.stringbuilder.clear#system-text-stringbuilder-clear)(this StringBuilder @this)

* System.Threading.Tasks
  * TaskAwaiter [GetAwaiter](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.getawaiter#system-threading-tasks-task-getawaiter)(this Task task)
  * TaskAwaiter&lt;TResult&gt; [GetAwaiter](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.getawaiter)&lt;TResult&gt;(this Task&lt;TResult&gt; task)
  * ConfiguredTaskAwaitable [ConfigureAwait](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.configureawait#system-threading-tasks-task-configureawait%28system-boolean%29)(this Task task, bool continueOnCapturedContext)
  * ConfiguredTaskAwaitable&lt;TResult&gt; [ConfigureAwait](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.configureawait#system-threading-tasks-task-1-configureawait%28system-boolean%29)&lt;TResult&gt;(this Task&lt;TResult&gt; task, bool continueOnCapturedContext)

* System.Threading.CancellationTokenSource
  * bool [TryReset](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource.tryreset)(this CancellationTokenSource @this)

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
public class Program {
    public static void Main() {
        int[] numbers = { 1, 2, 3, 4, 5 };
        var slice = numbers[1..^1];
        Console.WriteLine(string.Join(", ", slice)); // Output: 2, 3, 4
    }
}
```

### Lazy Initialization

```csharp
public class Program {
    private static Lazy<int> lazyValue = new Lazy<int>(() => ComputeValue());

    public static void Main() {
        Console.WriteLine(lazyValue.Value); // Output: Computed Value
    }

    private static int ComputeValue() {
        Console.WriteLine("Computed Value");
        return 42;
    }
}
```

### LINQ Methods

```csharp
using System.Collections.Generic;
using System.Linq;

public class Program {
    public static void Main() {
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

public class Program {
    public static async Task Main() {
        Task task = Task.Delay(1000);
        await task.ConfigureAwait(false);
        Console.WriteLine("Task completed");
    }
}
```

### Span Extensions

```csharp
using System;

public class Program {
    public static void Main() {
        // Create a Span from an array
        int[] numbers = { 1, 2, 3, 4, 5 };
        Span<int> span = numbers.AsSpan();
        
        // Slice operations
        Span<int> slice = span.Slice(1, 3); // { 2, 3, 4 }
        Console.WriteLine($"Slice length: {slice.Length}"); // Output: 3
        
        // Fill and Clear
        Span<int> buffer = stackalloc int[10];
        buffer.Fill(42);
        Console.WriteLine($"First element: {buffer[0]}"); // Output: 42
        
        // ReadOnlySpan from string
        ReadOnlySpan<char> text = "Hello World".AsSpan();
        ReadOnlySpan<char> world = text.Slice(6); // "World"
        Console.WriteLine(world.ToString()); // Output: World
        
        // Sequence comparison
        ReadOnlySpan<int> a = new[] { 1, 2, 3 };
        ReadOnlySpan<int> b = new[] { 1, 2, 3 };
        Console.WriteLine(a.SequenceEqual(b)); // Output: True
        
        // Index and LastIndex operations
        ReadOnlySpan<int> data = new[] { 1, 2, 3, 2, 1 };
        Console.WriteLine(data.IndexOf(2));     // Output: 1
        Console.WriteLine(data.LastIndexOf(2)); // Output: 3
    }
}
```

### Vector Extensions

```csharp
using System;
using System.Numerics;

public class Program {
    public static void Main() {
        // Vector2 operations
        Vector2 position = new Vector2(3.0f, 4.0f);
        Console.WriteLine($"Length: {position.Length()}"); // Output: 5
        
        Vector2 normalized = Vector2.Normalize(position);
        Console.WriteLine($"Normalized: {normalized}"); // Output: <0.6, 0.8>
        
        // Vector3 operations
        Vector3 v1 = new Vector3(1, 0, 0);
        Vector3 v2 = new Vector3(0, 1, 0);
        Vector3 cross = Vector3.Cross(v1, v2);
        Console.WriteLine($"Cross product: {cross}"); // Output: <0, 0, 1>
        
        // Vector4 transformations
        Vector4 point = new Vector4(1, 2, 3, 1);
        Matrix4x4 scale = Matrix4x4.CreateScale(2.0f);
        Vector4 transformed = Vector4.Transform(new Vector3(1, 2, 3), scale);
        Console.WriteLine($"Scaled: {transformed}");
        
        // Linear interpolation
        Vector3 start = Vector3.Zero;
        Vector3 end = new Vector3(10, 10, 10);
        Vector3 midpoint = Vector3.Lerp(start, end, 0.5f);
        Console.WriteLine($"Midpoint: {midpoint}"); // Output: <5, 5, 5>
        
        // Matrix operations
        Matrix4x4 rotation = Matrix4x4.CreateRotationZ(MathF.PI / 4); // 45 degrees
        Vector4 row = rotation.GetRow(0);
        Console.WriteLine($"First row: {row}");
    }
}
```

### Intrinsics Extensions

```csharp
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public class Program {
    public static unsafe void Main() {
        // Note: Hardware intrinsics provide SIMD operations.
        // When native support is unavailable, software fallbacks are used.
        
        // Vector128 creation and manipulation
        Vector128<float> vec1 = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
        Vector128<float> vec2 = Vector128.Create(5.0f, 6.0f, 7.0f, 8.0f);
        
        // Element access
        float first = Vector128.GetElement(vec1, 0);
        Console.WriteLine($"First element: {first}"); // Output: 1
        
        // SSE operations (with software fallback)
        // Software fallback always available
        if (Sse.IsSupported || true) {
            Vector128<float> sum = Sse.Add(vec1, vec2);
            float[] result = new float[4];
            fixed (float* ptr = result)
                Sse.Store(ptr, sum);
            
            Console.WriteLine($"Sum: [{result[0]}, {result[1]}, {result[2]}, {result[3]}]");
            // Output: Sum: [6, 8, 10, 12]
        }
        
        // SSE2 integer operations
        Vector128<int> intVec1 = Vector128.Create(10, 20, 30, 40);
        Vector128<int> intVec2 = Vector128.Create(1, 2, 3, 4);
        Vector128<int> intSum = Sse2.Add(intVec1, intVec2);
        
        // SSE41 advanced operations
        float[] data = { 1.5f, 2.7f, 3.2f, 4.9f };
        fixed (float* ptr = data) {
            Vector128<float> loaded = Sse.LoadVector128(ptr);
            Vector128<float> rounded = Sse41.Floor(loaded);
            // Result: { 1.0, 2.0, 3.0, 4.0 }
        }
        
        // Dot product calculation
        Vector128<float> a = Vector128.Create(1.0f, 2.0f, 3.0f, 4.0f);
        Vector128<float> b = Vector128.Create(2.0f, 3.0f, 4.0f, 5.0f);
        Vector128<float> dot = Sse41.DotProduct(a, b, 0xFF);
        // Result contains dot product in all elements: 40 (1*2 + 2*3 + 3*4 + 4*5)
    }
}
```

## Planned Features

The goal is to make Backports **compiler-complete** for older targets with the same runtime functionality as modern .NET.

### Test Coverage

* Add comprehensive tests for all existing polyfills
* Add tests for Memory<T>, MemoryPool<T>, ReadOnlySequence<T>, Vector128/256/512<T>
* Ensure test parity across all target frameworks (net35-net9.0)

## Known Issues

* **net2.0** - Hard to write unit tests for as I didn't have an nunit compatible nuget package at hand.
* **netstandard2.0/2.1 test targets** - These are API specifications, not runtimes. Tests compile but cannot execute directly; functionality is verified via compatible runtimes (netcoreapp3.1, net5.0+)
* **.NET SDK 10.0 test platform** - VSTest 18.x has compatibility issues with older .NET Core runtimes when running from CLI; Visual Studio Test Explorer handles this better

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
