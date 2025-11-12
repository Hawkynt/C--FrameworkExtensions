# Corlib Extensions

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)
[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Corlib.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Corlib)](https://www.nuget.org/packages/FrameworkExtensions.Corlib/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

Extension methods for core .NET types, providing additional functionality across strings, collections, math, I/O, threading, and other common operations. Supports .NET 3.5 through .NET 9.0.

### Key Features

- **3,300+ Extension Methods** across common .NET types
  - **600+ String extensions** - parsing, formatting, case conversion, text analysis, phonetics
  - **900+ Math & Numeric extensions** - numeric types with SIMD optimizations and hardware intrinsics
  - **300+ Array extensions** - array operations, slicing, algorithms
  - **400+ I/O extensions** - FileInfo, DirectoryInfo, Stream operations
  - **350+ Collection extensions** - Dictionary, List, HashSet, concurrent collections, LINQ enhancements
  - **200+ Threading extensions** - Interlocked operations, task management, synchronization
  - **550+ Additional extensions** - DateTime, TimeSpan, reflection, data access, networking

### Technical Details

- **T4 Code Generation** - Type-safe methods generated across numeric types
- **Performance Optimizations** - SIMD operations, hardware intrinsics, unsafe code where beneficial
- **Thread Safety** - Atomic operations, concurrent collection support
- **Modern C# Features** - Nullable reference types, spans, ranges, pattern matching
- **Multi-Framework Support** - .NET 3.5 through .NET 9.0 with conditional compilation
- **Dependencies** - Requires FrameworkExtensions.Backports for backported language features
- **Testing** - Unit, integration, performance, and regression tests

---

## Extension Methods by Type

### Array Extensions (`TItem[]`)

Array operations with performance optimizations

#### Core Operations

- **`CompareTo<TItem>(other, comparer)`** - Compare arrays with detailed changesets
- **`SafelyClone<TItem>()`** - Safe array cloning with proper type handling
- **`Swap<TItem>(firstIndex, secondIndex)`** - High-performance element swapping
- **`Shuffle<TItem>(entropySource)`** - Fisher-Yates shuffle implementation
- **`QuickSort<TItem>()` / `QuickSorted<TItem>()`** - Optimized quicksort
- **`Reverse<TItem>()`** - In-place array reversal
- **`RotateTowardsZero<TItem>()`** - Array rotation operations

#### Slicing & Partitioning

- **`Slice<TItem>(start, length)`** - Create mutable array slices
- **`ReadOnlySlice<TItem>(start, length)`** - Create read-only array slices
- **`Slices<TItem>(size)`** - Split array into chunks
- **`ProcessInChunks<TItem>(chunkSize, processor)`** - Chunked processing

#### High-Performance Fill Operations

- **`Fill(value)`** - Optimized fill for `byte[]`, `ushort[]`, `uint[]`, `ulong[]`, `IntPtr[]`
- **`Clear()`** - Optimized clear for all primitive array types

#### Element Access & Search

- **`GetRandomElement<TItem>(random)`** - Random element selection
- **`GetValueOrDefault<TItem>(index, defaultValue)`** - Safe indexed access
- **`First<TItem>()` / `Last<TItem>()` / `*OrDefault()`** - LINQ-style element access
- **`IndexOf<TItem>(value, comparer)`** - Enhanced element searching
- **`Contains<TItem>(value)`** - Membership testing

#### Transformation & Aggregation

- **`ConvertAll<TItem, TOutput>(converter)`** - Array transformation
- **`ForEach<TItem>(action)`** - Element iteration with parallel support
- **`ParallelForEach<TItem>(action)`** - Parallel processing
- **`Join<TItem>(separator, converter)`** - String joining with custom converters
- **`Select<TItem, TResult>(selector)`** - LINQ-style projection
- **`Aggregate<TItem>(func, seed)`** - Aggregation operations

---

### String Extensions (`string`)

String manipulation methods covering parsing, formatting, and text analysis

#### Case Conversion (12 methods)

- **`ToPascalCase(culture)` / `ToPascalCaseInvariant()`** - Convert to PascalCase with intelligent word boundary detection
- **`ToCamelCase(culture)` / `ToCamelCaseInvariant()`** - Convert to camelCase with acronym handling
- **`ToSnakeCase(culture)` / `ToSnakeCaseInvariant()`** - Convert to snake_case with Unicode support
- **`ToUpperSnakeCase(culture)` / `ToUpperSnakeCaseInvariant()`** - Convert to UPPER_SNAKE_CASE
- **`ToKebabCase(culture)` / `ToKebabCaseInvariant()`** - Convert to kebab-case
- **`ToUpperKebabCase(culture)` / `ToUpperKebabCaseInvariant()`** - Convert to UPPER-KEBAB-CASE
- **`UpperFirst(culture)` / `UpperFirstInvariant()`** - Capitalize first character only
- **`LowerFirst(culture)` / `LowerFirstInvariant()`** - Lowercase first character only

#### String Manipulation & Modification (25+ methods)

- **`ExchangeAt(index, replacement)`** - Replace characters/strings at specific positions with span optimizations
- **`ExchangeAt(index, count, replacement)`** - Replace ranges with pattern matching
- **`Repeat(count)`** - Optimized string repetition with StringBuilder for large counts
- **`RemoveFirst(count)` / `RemoveLast(count)`** - Remove characters from start/end
- **`RemoveAtStart(what, comparison)` / `RemoveAtEnd(what, comparison)`** - Conditional prefix/suffix removal
- **`SubString(start, end)`** - Alternative substring with end parameter
- **`Left(count)` / `Right(count)`** - Get leftmost/rightmost portions safely
- **`Split(int)` / `Split(Regex)`** - Advanced splitting into fixed-size chunks or regex patterns

#### Replace Operations (15+ methods)

- **`ReplaceFirst(what, replacement, comparison)`** - Replace first occurrence with comparison options
- **`ReplaceLast(what, replacement, comparison)`** - Replace last occurrence
- **`MultipleReplace(KeyValuePair<string, object>[])`** - Bulk replacement operations
- **`MultipleReplace(string, string[])`** - Replace multiple patterns with single value
- **`ReplaceRegex(pattern, replacement, options)`** - Regex-based replacement
- **`Replace(Regex, string)` / `Replace(string, string, int, StringComparison)`** - Advanced replacement with limits

#### StartsWith/EndsWith Operations (50+ methods)

- **`StartsWith(char/string, StringComparison/StringComparer)`** - Enhanced prefix checking
- **`StartsNotWith()` variants** - Negative prefix checking
- **`StartsWithAny()/StartsNotWithAny()`** - Multiple prefix checking with various comparison options
- **`EndsWith(char/string, StringComparison/StringComparer)`** - Enhanced suffix checking
- **`EndsNotWith()` variants** - Negative suffix checking  
- **`EndsWithAny()/EndsNotWithAny()`** - Multiple suffix checking

#### Contains & Search Operations (40+ methods)

- **`Contains()` variants** - Enhanced contains with comparison options
- **`ContainsNot()` / `ContainsAll()` / `ContainsAny()` / `ContainsNotAny()`** - Advanced set operations
- **`IndexOf()` variants** - Enhanced searching with culture support
- **`IsAnyOf()` / `IsNotAnyOf()`** - Set membership testing

#### Null & State Checking (15+ methods)

- **`IsNull()` / `IsNotNull()`** - Null checking
- **`IsEmpty()` / `IsNotEmpty()`** - Empty string checking
- **`IsNullOrEmpty()` / `IsNotNullOrEmpty()`** - Combined null/empty checks
- **`IsNullOrWhiteSpace()` / `IsNotNullOrWhiteSpace()`** - Whitespace checking
- **`IsWhiteSpace()` / `IsNotWhiteSpace()`** - Whitespace-only checking
- **`DefaultIf()` variants** - Conditional default value provision

#### Text Processing & Analysis (15+ methods)

- **`SanitizeForFileName(sanitation)`** - Make strings safe for file systems
- **`GetSoundexRepresentation(length, culture)`** - Phonetic matching with German/English rules
- **`TextAnalysis()` / `TextAnalysisFor(culture)`** - Comprehensive linguistic analysis:
  - Word extraction and tokenization
  - Sentence segmentation with abbreviation handling  
  - Syllable counting (German, English, Romance languages)
  - Word frequency histograms and distinct word analysis
- **Text similarity and comparison methods**
- **Line breaking and text wrapping utilities**
- **Truncation methods with various options**

#### Cryptography & Hashing (2 methods)

- **`ComputeHash<TAlgorithm>()`** - Generic hash computation with any HashAlgorithm
- **`ComputeHash(hashAlgorithm)`** - Hash with specific algorithm instance

#### Regular Expressions (8+ methods)

- **`IsMatch(regex)` / `IsNotMatch(regex)`** - Pattern matching with Regex objects
- **`IsMatch(pattern, options)` / `IsNotMatch(pattern, options)`** - Pattern matching with string patterns
- **`Matches(pattern, options)`** - Get all pattern matches
- **`MatchGroups(pattern, options)`** - Extract regex capture groups
- **`AsRegularExpression(options)`** - Convert string to compiled Regex

#### Advanced Formatting (8+ methods)

- **`FormatWith(parameters)`** - Enhanced string.Format with better error handling
- **`FormatWithEx(fields, comparer)`** - Template-based formatting with custom field resolution
- **`FormatWithEx(KeyValuePair<string, object>[])`** - Formatting with key-value pairs
- **`FormatWithEx(IDictionary<string, string>)`** - Dictionary-based formatting
- **`FormatWithObject<T>(object)`** - Format using object properties via reflection

#### Type-Safe Parsing (400+ methods)

Generated via T4 templates for 15 data types: Float, Double, Decimal, Byte, SByte, UInt16, Int16, UInt32, Int32, UInt64, Int64, TimeSpan, DateTime, Boolean, Color

**Each type provides:**

- **`Parse{Type}()`** - Basic parsing
- **`Parse{Type}(IFormatProvider/NumberStyles)`** - Culture-aware parsing
- **`TryParse{Type}(out result)`** - Safe parsing variants
- **`Parse{Type}OrDefault(defaultValue)`** - Parsing with fallback values
- **`Parse{Type}OrNull()`** - Nullable parsing for value types
- **ReadOnlySpan&lt;char&gt; support** for zero-allocation parsing

#### Database & Special Formats (3+ methods)

- **`ToLinq2SqlConnectionString()`** - Convert to LINQ-to-SQL connection format
- **`MsSqlIdentifierEscape()`** - Escape SQL Server identifiers
- **Line breaking and special format utilities**

#### Character Access (4 methods)

- **`First()` / `FirstOrDefault(defaultChar)`** - Get first character safely
- **`Last()` / `LastOrDefault(defaultChar)`** - Get last character safely

#### Modern .NET Features (5+ methods)

- **`CopyTo(Span<char> target)`** - Copy to span for zero-allocation scenarios
- **Span-based operations** with `ReadOnlySpan<char>` support
- **Performance-optimized implementations** using `stackalloc` for small strings

---

### Collection Extensions (`Collections`, `Generic Collections`, `Concurrent Collections`)

Methods for collection operations including dictionaries, lists, and concurrent collections

#### Dictionary Extensions (`IDictionary<TKey, TValue>`)

Dictionary operations with thread-safety support

- **`AddRange(keyValuePairs)`** - Bulk addition operations
- **`GetValueOrDefault(key, defaultValue)`** - Safe value retrieval
- **`GetValueOrNull(key)`** - Null-safe value retrieval
- **`AddOrUpdate(key, value)`** - Upsert operations
- **`GetOrAdd(key, valueFactory)`** - Lazy value addition
- **`TryAdd(key, value)` / `TryRemove(key, out value)`** - Safe modifications
- **`TryUpdate(key, newValue, comparisonValue)`** - Conditional updates
- **`CompareTo(other, keyComparer, valueComparer)`** - Dictionary comparison

#### Generic Collection Extensions

Extensions for List, HashSet, Queue, Stack, LinkedList, and related types

- **List Extensions** - TrySetFirst/Last/Item, RemoveEvery, Swap, Shuffle, Permutate, BinarySearchIndex variants
- **HashSet Extensions** - CompareTo, ContainsNot, TryAdd, TryRemove
- **Queue Extensions** - PullTo variants, PullAll, Pull, AddRange, Add, Fetch, TryDequeue
- **Stack Extensions** - PullTo variants, PullAll, Pull, Exchange, Invert, AddRange, Add, Fetch
- **LinkedList Extensions** - Navigation and manipulation
- **Collection Extensions** - General ICollection<T> utilities
- **Enumerable Extensions** - T4-generated LINQ-style operations
- **KeyValuePair Extensions** - Reverse key-value pairs

#### Concurrent Collection Extensions

Thread-safe collection operations for concurrent scenarios

- **ConcurrentDictionary Extensions** - Atomic operations
- **ConcurrentQueue Extensions** - Queue operations with bulk processing
- **ConcurrentStack Extensions** - Stack operations with safety guarantees

#### Specialized Collection Extensions

- **StringDictionary Extensions** - String-specific dictionary operations
- **StringCollection Extensions** - String collection utilities
- **BitArray Extensions** - Bit manipulation operations
- **LINQ Extensions** - IQueryable enhancements
- **ObjectModel Collection Extensions** - Observable collections

---

### File System Extensions

File system operations including FileInfo, DirectoryInfo, and Stream extensions

#### FileInfo Extensions (`FileInfo`)

File operations with async support

#### File Operations

- **`EnableCompression()` / `TryEnableCompression()`** - NTFS compression
- **`GetTypeDescription()`** - File type identification
- **`RenameTo(newName)`** - Safe file renaming
- **`ChangeExtension(newExtension)`** - Extension modification
- **`MoveTo(destination, timeout, overwrite)`** - Enhanced move with options

#### Async Operations

- **`CopyToAsync(target, cancellationToken, overwrite)`** - Async file copying
- **Enhanced async operations** with progress reporting and cancellation

#### Hash & Integrity

- **`ComputeHash<THashAlgorithm>()`** - Generic hash computation
- **`ComputeHash(provider, blockSize)`** - Custom hash with block size
- **`ComputeSHA512Hash()` / `ComputeSHA384Hash()` / etc.** - Specific algorithms

#### DirectoryInfo Extensions (`DirectoryInfo`)

Directory operations and navigation

#### Directory Management

- **`RenameTo(newName)`** - Directory renaming
- **`Clear()`** - Clear directory contents
- **`GetSize()`** - Calculate total directory size
- **`GetRealPath()`** - Resolve symbolic links
- **`TryCreate(recursive)` / `TryDelete(recursive)`** - Safe operations

#### Navigation & Queries

- **`Directory(subdirectory, ignoreCase)`** - Navigate to subdirectories
- **`File(filePath, ignoreCase)`** - Get files within directory
- **`GetOrAddDirectory(name)`** - Get or create subdirectory
- **`HasDirectory(searchPattern, option)` / `HasFile(searchPattern, option)`** - Content checks
- **`ContainsFile(fileName, option)` / `ContainsDirectory(directoryName, option)`** - Specific searches
- **`ExistsAndHasFiles(fileMask)`** - Existence and content verification

#### Utilities

- **`GetTempFile(extension)`** - Generate temporary files
- **`TryCreateFile(fileName, attributes)`** - Safe file creation
- **`TrySetLastWriteTimeUtc()` / `TrySetCreationTimeUtc()` / `TrySetAttributes()`** - Safe attribute management

#### Stream Extensions (`Stream`)

Stream operations for various data types

- **Primitive I/O Operations** - Read/Write for primitive types (bool, byte, short, int, long, float, double, decimal)
- **Endianness Support** - Big-endian and little-endian operations
- **String Operations** - Length-prefixed, zero-terminated, and fixed-length strings
- **Struct Operations** - Struct serialization, positioned reads with seek origin support
- **Async Operations** - Async/await support for positioned operations
- **Stream Analysis** - End-of-stream detection, stream-to-array conversion
- **Buffer Management** - Buffer management using thread-static and shared buffers

#### Path & FileSystem Extensions

Path manipulation and file system utilities

- **Temporary Resource Management** - Temporary file/directory creation with auto-cleanup
- **Cross-Platform Support** - Multi-OS temporary directory resolution
- **UNC Path Operations** - Network path parsing and manipulation
- **Relative Path Calculation** - Relative paths between file system objects
- **Volume Operations** - System volume enumeration and mount point management

---

### Math & Numeric Extensions

Mathematical operations for numeric types

#### Bit Manipulation Extensions

Bitwise operations with hardware intrinsics support

- **`LowerHalf()` / `UpperHalf()`** - Extract lower/upper bit portions from multi-byte types
- **`RotateLeft(count)` / `RotateRight(count)`** - Bitwise rotation for all integer types
- **`TrailingZeroCount()` / `LeadingZeroCount()`** - Count trailing/leading zeros with SIMD optimization
- **`TrailingOneCount()` / `LeadingOneCount()`** - Count trailing/leading ones
- **`CountSetBits()` / `CountUnsetBits()`** - Population count operations (Brian Kernighan's algorithm)
- **`Parity()`** - Check if number of set bits is even/odd
- **`ReverseBits()`** - Reverse bit order using lookup tables
- **`ParallelBitExtract(mask)`** - Extract bits based on bitmask
- **`DeinterleaveBits()` / `PairwiseDeinterleaveBits()`** - Bit deinterleaving operations
- **`FlipBit()` / `GetBit()` / `SetBit()` / `ClearBit()`** - Individual bit manipulation
- **`IsPowerOfTwo()`** - Fast power-of-2 testing using bitwise AND tricks
- **`And()` / `Or()` / `Xor()` / `Not()` / `Nand()` / `Nor()`** - Bitwise logical operations

#### Advanced Mathematical Functions

Mathematical functions with precision implementations

**Standard Math Functions:**

- **`Pow(exponent)` / `Sqrt()` / `Cbrt()`** - Power, square root, cube root
- **`Floor()` / `Ceiling()` / `Truncate()`** - Rounding operations with MidpointRounding support
- **`Round(decimals, midpointRounding)`** - Advanced rounding with banker's rounding
- **`Abs()` / `Sign()`** - Absolute value and sign extraction
- **`LogN(base)` / `Log()` / `Log10()` / `Log2()`** - Logarithmic functions
- **`Exp()`** - Exponential function with Taylor series for decimal precision

**Trigonometric Functions:**

- **`Sin()` / `Cos()` / `Tan()`** - Basic trigonometric functions
- **`Sinh()` / `Cosh()` / `Tanh()`** - Hyperbolic functions
- **`Cot()` / `Coth()`** - Cotangent and hyperbolic cotangent
- **`Csc()` / `Csch()`** - Cosecant and hyperbolic cosecant
- **`Sec()` / `Sech()`** - Secant and hyperbolic secant
- **`Asin()` / `Acos()` / `Atan()`** - Inverse trigonometric functions
- **`Arsinh()` / `Arcosh()` / `Artanh()`** - Inverse hyperbolic functions
- **`Acot()` / `Asec()` / `Acsc()` / `Arcoth()` / `Arsech()` / `Arcsch()`** - Extended inverse functions

#### Arithmetic Operations

Operations for numeric types: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal

**Basic Arithmetic:**

- **`Add()` / `Subtract()` / `MultipliedWith()` / `DividedBy()`** - Functional-style arithmetic
- **`Squared()` / `Cubed()`** - Common power operations with overflow checking
- **`Average(other)`** - Precise average calculation avoiding overflow
- **`FusedMultiplyAdd()` / `FusedMultiplySubtract()`** - Hardware-accelerated fused operations
- **`FusedDivideAdd()` / `FusedDivideSubtract()`** - Custom fused divide operations

**Shift Operations (Integer Types):**

- **`ArithmeticShiftLeft()` / `ArithmeticShiftRight()`** - Sign-preserving bit shifts
- **`LogicalShiftLeft()` / `LogicalShiftRight()`** - Zero-fill bit shifts

#### Comparison & Range Operations

Comparison operations for numeric types

**Value State Checks:**

- **`IsZero()` / `IsNotZero()`** - Zero comparison with epsilon for floating-point
- **`IsPositive()` / `IsNegative()` / `IsPositiveOrZero()` / `IsNegativeOrZero()`** - Sign checking
- **`IsEven()` / `IsOdd()`** - Parity checking using bitwise AND
- **`IsAbove()` / `IsBelow()` / `IsAboveOrEqual()` / `IsBelowOrEqual()`** - Relational comparisons
- **`IsBetween()` / `IsInRange()`** - Range validation with inclusive/exclusive bounds
- **`IsIn()` / `IsNotIn()`** - Set membership testing

**Floating-Point Specific:**

- **`IsNaN()` / `IsInfinity()` / `IsPositiveInfinity()` / `IsNegativeInfinity()`** - IEEE 754 special value detection
- **`IsNumeric()` / `IsNonNumeric()`** - Validity checking for calculations
- **`ReciprocalEstimate()`** - Fast reciprocal approximation using hardware instructions

#### Type-Safe Unsigned Wrappers

UnsignedFloat, UnsignedDouble, UnsignedDecimal - compile-time negative value prevention

**Arithmetic Support:**

- Standard operators (+, -, *, /, %) with overflow checking
- Comparison operators
- Implicit conversions from unsigned integer types
- Explicit conversions from signed types with validation
- `IComparable`, `IConvertible`, `IFormattable` interface implementations
- `ISpanParsable`, `ISpanFormattable` support for .NET 7+

**Mathematical Operations:**

- Standard math functions adapted for unsigned constraints
- Handling for operations that could produce negative results

#### Utility Extensions

**Operations:**

- **`Min()` / `Max()`** - Min/max operations
- **`Bits()`** - IEEE 754 bit representation for floating-point types
- **`Equ()`** - Equality comparison with configurable epsilon

**Repetition Extensions:**

For integer types (byte through long)

- **`Times(Action)`** - Execute action N times
- **`Times(Action<T>)`** - Execute action N times with index parameter
- **`Times(string)`** - Repeat string N times
- **`Times(char)`** - Repeat character N times

#### Performance Features

- Hardware intrinsics integration (SIMD, FMA, etc.)
- Aggressive inlining using `MethodImplOptions.AggressiveInlining`
- High-precision algorithms (Taylor series, Newton-Raphson methods)
- Overflow detection for safe arithmetic
- Epsilon-based comparisons for floating-point operations
- SIMD optimizations where hardware supports it
- Lookup tables for bit manipulation operations
- Branch reduction using bitwise operations

---

### DateTime & TimeSpan Extensions

Date and time manipulation

- **DateTime Operations** - Date/time arithmetic and formatting
- **TimeSpan Operations** - Duration calculations and utilities

---

### Threading & Task Extensions

Concurrent programming and synchronization utilities

#### Task Management Extensions

- **`DeferredTask`** - Lazy task execution with dependency management
- **`ScheduledTask`** - Scheduled task execution
- **`Sequential`** - Sequential task processor with ordering guarantees
- **`Task`** - Task utilities
- **`Future<T>`** - Future/promise pattern implementation

#### Interlocked Extensions

Atomic operations with enum support and flag manipulation

- **Standard Atomic Operations** - Interlocked operations
- **T4-Generated Enum Operations** - Type-safe atomic operations for enums:
  - Flag operations (set, clear, toggle, test atomically)
  - Enum arithmetic (add, subtract, increment, decrement)
  - Conditional updates (CompareExchange with strong typing)
  - Read operations (atomic reads with memory barriers)

#### Synchronization Primitives

Synchronization with thread-safety features

- **`SemaphoreSlim` Extensions** - Semaphore operations
- **`ManualResetEvent` Extensions** - Event signaling utilities
- **`Thread` Extensions** - Thread management and utilities
- **`Timer` Extensions** - Timer operations

#### Threading Utilities

- **`CallOnTimeout`** - Timeout-based callback execution
- **`Event`** - Event signaling with multiple listeners
- **`HighPrecisionTimer`** - High-resolution timing

---

### Reflection & Type Extensions

Reflection utilities for types and metadata

#### Type Operations

- Type inspection and metadata operations
- **Assembly Extensions** - Assembly operations
- **MethodInfo Extensions** - Method reflection utilities
- **PropertyInfo Extensions** - Property reflection utilities

---

### Data & ComponentModel Extensions

Data binding and database operations

#### Data Extensions

- **`DataRecord` / `DataRow` / `DataTable` Extensions** - Data operations
- **LINQ to Data Extensions** - Querying utilities
- **SQL Client Extensions** - Database utilities

#### ComponentModel Extensions

- **`BindingList` / `SortableBindingList`** - Data binding
- **Property Change Notifications** - MVVM support
- **Attribute Extensions** - Metadata utilities

---

## New Types

### Collection Types

Collection implementations for specialized use cases

#### Array Utilities

- **`ArraySlice<TItem>`** - Mutable array slice with indexer support
- **`ReadOnlyArraySlice<TItem>`** - Read-only array slice with enumeration

#### Advanced Collections

- **`BiDictionary<TFirst, TSecond>`** - Bidirectional dictionary with O(1) reverse lookup
- **`DoubleDictionary<TOuter, TInner, TValue>`** - Two-level nested dictionary
- **`FastLookupTable<TItem>`** - High-performance lookup table with optimized hashing
- **`OrderedDictionary<TKey, TValue>`** - Dictionary maintaining insertion order
- **`CachedEnumeration<TItem>`** - Caching enumeration wrapper for expensive operations

#### Concurrent Collections

- **`ExecutiveQueue<TItem>`** - Advanced queue with priority and executive features
- **`ConcurrentWorkingBag<T>`** - Thread-safe working bag with enumeration

---

### Change Tracking System

Comprehensive change detection and tracking

- **`IChangeSet<TItem>`** - Interface for change tracking operations
- **`ChangeSet<TItem>`** - Complete change tracking implementation
- **`ChangeType`** - Enumeration: `Added`, `Removed`, `Changed`, `Equal`

---

### File Operations Framework

Advanced file comparison and processing

#### File Comparison

- **`IFileComparer`** - Base interface for file comparison strategies
- **`ITemporaryFileToken`** - Temporary files with auto-disposal
- **`ITemporaryDirectoryToken`** - Temporary directories with auto-disposal
- **`BinaryFileComparer`** - Byte-by-byte binary comparison
- **`FileLengthComparer`** - Fast length-based comparison
- **`FileSimpleAttributesComparer`** - Attribute-based comparison
- **`FileCreationTimeComparer`** - Creation time comparison
- **`FileLastWriteTimeComparer`** - Last write time comparison

#### Enhanced I/O

- **`IFileInProgress`** - Interface for file operation progress tracking
- **`FileInProgress`** - File operation progress implementation
- **`CustomTextReader`** - Enhanced text reader with advanced features
- **`BufferedStreamEx`** - Extended buffered stream with performance optimizations

---

### Cryptography & Hashing

Comprehensive hashing algorithms and security utilities

#### Advanced Hashing

- **`IAdvancedHashAlgorithm`** - Interface for advanced hashing features
- **`Adler`** - Adler-32 checksum algorithm
- **`Fletcher`** - Fletcher checksum variants (16/32-bit)
- **`JavaHash`** - Java-compatible string hashing
- **`LRC8`** - 8-bit Longitudinal Redundancy Check
- **`Pearson`** - Pearson hashing algorithm
- **`Tiger`** - Tiger hash algorithm (192-bit)
- **`Whirlpool`** - Whirlpool hash algorithm (512-bit)
- **`RandomNumberProvider`** - Enhanced cryptographically secure random generation

---

### Threading & Concurrency

Advanced concurrent programming constructs

#### Task Management

- **`DeferredTask`** - Lazy task execution with dependency management
- **`ScheduledTask`** - Cron-like scheduled task execution
- **`Sequential`** - Sequential task processor with ordering guarantees

#### Synchronization Primitives

- **`CallOnTimeout`** - Timeout-based callback execution
- **`Event`** - Enhanced event signaling with multiple listeners
- **`Future<T>`** - Future/promise pattern implementation
- **`HighPrecisionTimer`** - High-resolution timing for performance-critical code

---

### Property & State Management

Advanced property and state management utilities

#### Smart Properties

- **`FastLazy<T>`** - High-performance lazy initialization
- **`RealtimeProperty<T>`** - Real-time property with change notifications
- **`SlowProperty<T>`** - Throttled property updates for expensive operations
- **`IndexedProperty<T>`** - Property with indexer support
- **`StaticMethodLocal<T>`** - Static method-local storage

---

### Utility Types

Essential utility types for common programming patterns

#### Core Utilities

- **`Range`** - Enhanced range operations and arithmetic
- **`Span` Enhancements** - Extended span utilities and operations

#### Validation Framework

- **`Against`** - Comprehensive parameter validation with performance optimization
- **`AlwaysThrow`** - Exception throwing utilities with stack trace and inlining optimization

#### Text Processing

- **`TextAnalyzer`** - Advanced text analysis with readability metrics
- **`ReadabilityScoreCalculator`** - Multiple readability algorithm implementations

---

## Performance Features

### Optimizations

- **Unsafe Code Blocks** - Direct memory manipulation for performance
- **SIMD Operations** - Vectorized operations using block processing
- **Aggressive Inlining** - Use of `MethodImplOptions.AggressiveInlining`
- **Stack Allocation** - `stackalloc` for temporary buffers
- **Span<T> and Memory<T>** - Modern .NET memory management

### Memory Efficiency

- **Reduced Allocations** - Operations designed to avoid unnecessary heap allocations
- **Object Pooling** - Reusable object patterns where applicable
- **Block-based Operations** - `Block32`, `Block64` for memory operations
- **Bounds Check Elimination** - Loop construction to help JIT optimize

---

## Installation & Usage

```xml
<PackageReference Include="FrameworkExtensions.Corlib" Version="*" />
```

```csharp
using System;
using Corlib.Extensions;

// Array operations
var numbers = new[] { 3, 1, 4, 1, 5, 9 };
numbers.QuickSort();
var slice = numbers.Slice(1, 3);

// String operations
var text = "hello_world";
var pascalCase = text.ToPascalCase(); // "HelloWorld"
var hash = text.ComputeHash<SHA256>();

// Collection operations
var dict = new Dictionary<string, int>();
dict.AddOrUpdate("key", 42);
var value = dict.GetValueOrDefault("missing", 0);

// File operations
var file = new FileInfo("data.txt");
file.EnableCompression();
var hash = file.ComputeSHA256Hash();
```

---

## Target Frameworks

Multi-targeting support:

- .NET Framework: `net35`, `net40`, `net45`, `net48`
- .NET Standard: `netstandard2.0`
- .NET Core/5+: `netcoreapp3.1`, `net6.0`, `net7.0`, `net8.0`, `net9.0`

---

## Library Statistics

### Overview

- **3,300+ Extension Methods** across common .NET types
- **50+ .NET Types Extended** covering major framework types
- **15+ Data Types** with type-safe parsing support
- **Multiple .NET Versions** from .NET 3.5 to .NET 9.0

### Technical Features

- **Hardware Intrinsics** - CPU-specific optimizations (SIMD, FMA, etc.)
- **Unsafe Code Optimization** - Direct memory manipulation where beneficial
- **T4 Code Generation** - Compile-time code generation
- **Aggressive Inlining** - Micro-optimizations throughout
- **Memory Pool Usage** - Reduced garbage collection pressure
- **Branch Reduction** - Conditional logic using bitwise operations

### Design Principles

- **Thread-Safe Operations** - Atomic operations and concurrent collection support
- **Error Handling** - Validation with meaningful error messages
- **Globalization Support** - Cultural awareness for international applications
- **Backwards Compatibility** - Supports legacy .NET Framework applications
- **Additive API Design** - No breaking changes philosophy

---

## Testing & Quality

- **600+ Unit Tests** with coverage metrics
- **Performance Benchmarks** for critical operations
- **Cross-Platform CI/CD** on Windows, Linux, and macOS
- **Memory Leak Testing** for allocation-heavy operations
- **Thread Safety Testing** for concurrent operations

---

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for detailed guidelines on:

- Code style and conventions
- Performance requirements
- Testing categories and patterns
- Architecture principles

---

## License

[LGPL-3.0-or-later](https://licenses.nuget.org/LGPL-3.0-or-later) - Use freely, contribute back improvements.
