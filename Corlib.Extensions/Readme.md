# Corlib Extensions 🚀

> **Just a small .NET extension library - 3,300+ high-performance extension methods with aggressive optimizations.**

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)
[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Corlib.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Corlib)](https://www.nuget.org/packages/FrameworkExtensions.Corlib/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

This is the most comprehensive .NET extension library ever created, extending virtually every .NET core type with thousands of carefully optimized extension methods. It represents years of development effort to create the ultimate productivity enhancement for .NET developers, featuring aggressive performance optimizations, comprehensive functionality, and cross-platform compatibility.

### 🎯 Key Features
- **3,300+ Extension Methods** across virtually every .NET type
  - **600+ String extensions** - parsing, formatting, case conversion, text analysis, phonetics
  - **900+ Math & Numeric extensions** - all numeric types with SIMD optimizations and hardware intrinsics
  - **300+ Array extensions** - high-performance operations, slicing, advanced algorithms  
  - **400+ I/O extensions** - FileInfo, DirectoryInfo, Stream, and comprehensive file system operations
  - **350+ Collection extensions** - Dictionary, List, HashSet, concurrent collections, LINQ enhancements
  - **200+ Threading extensions** - advanced Interlocked operations, task management, synchronization
  - **550+ Additional extensions** - DateTime, TimeSpan, reflection, data access, networking, and all core types

### 🏆 Unprecedented Scope
- **T4 Code Generation** - Extensive use of T4 templates for generating type-safe methods across all numeric types
- **Hardware-Optimized** - SIMD operations, hardware intrinsics, unsafe code for maximum performance
- **Enterprise-Grade** - Atomic operations, thread-safety, robust error handling, comprehensive validation
- **Modern C# Features** - Nullable reference types, spans, ranges, pattern matching, latest language features
- **Universal Compatibility** - Supports .NET 3.5 through .NET 9.0 with conditional compilation
- **Zero Dependencies** - Only depends on FrameworkExtensions.Backports for backported language features
- **Production-Ready** - Extensive unit, integration, performance, and regression testing

---

## 📚 Methods - Extension Methods by Type

### Array Extensions (`TItem[]`)
**300+ enhanced array operations with aggressive performance optimizations**

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
**600+ comprehensive string manipulation methods - the most complete string library ever created**

#### **Case Conversion (12 methods)**
- **`ToPascalCase(culture)` / `ToPascalCaseInvariant()`** - Convert to PascalCase with intelligent word boundary detection
- **`ToCamelCase(culture)` / `ToCamelCaseInvariant()`** - Convert to camelCase with acronym handling
- **`ToSnakeCase(culture)` / `ToSnakeCaseInvariant()`** - Convert to snake_case with Unicode support
- **`ToUpperSnakeCase(culture)` / `ToUpperSnakeCaseInvariant()`** - Convert to UPPER_SNAKE_CASE
- **`ToKebabCase(culture)` / `ToKebabCaseInvariant()`** - Convert to kebab-case
- **`ToUpperKebabCase(culture)` / `ToUpperKebabCaseInvariant()`** - Convert to UPPER-KEBAB-CASE
- **`UpperFirst(culture)` / `UpperFirstInvariant()`** - Capitalize first character only
- **`LowerFirst(culture)` / `LowerFirstInvariant()`** - Lowercase first character only

#### **String Manipulation & Modification (25+ methods)**
- **`ExchangeAt(index, replacement)`** - Replace characters/strings at specific positions with span optimizations
- **`ExchangeAt(index, count, replacement)`** - Replace ranges with pattern matching
- **`Repeat(count)`** - Optimized string repetition with StringBuilder for large counts
- **`RemoveFirst(count)` / `RemoveLast(count)`** - Remove characters from start/end
- **`RemoveAtStart(what, comparison)` / `RemoveAtEnd(what, comparison)`** - Conditional prefix/suffix removal
- **`SubString(start, end)`** - Alternative substring with end parameter
- **`Left(count)` / `Right(count)`** - Get leftmost/rightmost portions safely
- **`Split(int)` / `Split(Regex)`** - Advanced splitting into fixed-size chunks or regex patterns

#### **Replace Operations (15+ methods)**
- **`ReplaceFirst(what, replacement, comparison)`** - Replace first occurrence with comparison options
- **`ReplaceLast(what, replacement, comparison)`** - Replace last occurrence 
- **`MultipleReplace(KeyValuePair<string, object>[])`** - Bulk replacement operations
- **`MultipleReplace(string, string[])`** - Replace multiple patterns with single value
- **`ReplaceRegex(pattern, replacement, options)`** - Regex-based replacement
- **`Replace(Regex, string)` / `Replace(string, string, int, StringComparison)`** - Advanced replacement with limits

#### **StartsWith/EndsWith Operations (50+ methods)**
- **`StartsWith(char/string, StringComparison/StringComparer)`** - Enhanced prefix checking
- **`StartsNotWith()` variants** - Negative prefix checking
- **`StartsWithAny()/StartsNotWithAny()`** - Multiple prefix checking with various comparison options
- **`EndsWith(char/string, StringComparison/StringComparer)`** - Enhanced suffix checking
- **`EndsNotWith()` variants** - Negative suffix checking  
- **`EndsWithAny()/EndsNotWithAny()`** - Multiple suffix checking

#### **Contains & Search Operations (40+ methods)**
- **`Contains()` variants** - Enhanced contains with comparison options
- **`ContainsNot()` / `ContainsAll()` / `ContainsAny()` / `ContainsNotAny()`** - Advanced set operations
- **`IndexOf()` variants** - Enhanced searching with culture support
- **`IsAnyOf()` / `IsNotAnyOf()`** - Set membership testing

#### **Null & State Checking (15+ methods)**
- **`IsNull()` / `IsNotNull()`** - Null checking
- **`IsEmpty()` / `IsNotEmpty()`** - Empty string checking
- **`IsNullOrEmpty()` / `IsNotNullOrEmpty()`** - Combined null/empty checks
- **`IsNullOrWhiteSpace()` / `IsNotNullOrWhiteSpace()`** - Whitespace checking
- **`IsWhiteSpace()` / `IsNotWhiteSpace()`** - Whitespace-only checking
- **`DefaultIf()` variants** - Conditional default value provision

#### **Text Processing & Analysis (15+ methods)**
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

#### **Cryptography & Hashing (2 methods)**
- **`ComputeHash<TAlgorithm>()`** - Generic hash computation with any HashAlgorithm
- **`ComputeHash(hashAlgorithm)`** - Hash with specific algorithm instance

#### **Regular Expressions (8+ methods)**
- **`IsMatch(regex)` / `IsNotMatch(regex)`** - Pattern matching with Regex objects
- **`IsMatch(pattern, options)` / `IsNotMatch(pattern, options)`** - Pattern matching with string patterns
- **`Matches(pattern, options)`** - Get all pattern matches
- **`MatchGroups(pattern, options)`** - Extract regex capture groups
- **`AsRegularExpression(options)`** - Convert string to compiled Regex

#### **Advanced Formatting (8+ methods)**
- **`FormatWith(parameters)`** - Enhanced string.Format with better error handling
- **`FormatWithEx(fields, comparer)`** - Template-based formatting with custom field resolution
- **`FormatWithEx(KeyValuePair<string, object>[])`** - Formatting with key-value pairs
- **`FormatWithEx(IDictionary<string, string>)`** - Dictionary-based formatting
- **`FormatWithObject<T>(object)`** - Format using object properties via reflection

#### **Type-Safe Parsing (400+ methods)**
*Generated via T4 templates for 15 data types: Float, Double, Decimal, Byte, SByte, UInt16, Int16, UInt32, Int32, UInt64, Int64, TimeSpan, DateTime, Boolean, Color*

**Each type provides:**
- **`Parse{Type}()`** - Basic parsing
- **`Parse{Type}(IFormatProvider/NumberStyles)`** - Culture-aware parsing
- **`TryParse{Type}(out result)`** - Safe parsing variants
- **`Parse{Type}OrDefault(defaultValue)`** - Parsing with fallback values
- **`Parse{Type}OrNull()`** - Nullable parsing for value types
- **ReadOnlySpan<char> support** for zero-allocation parsing

#### **Database & Special Formats (3+ methods)**
- **`ToLinq2SqlConnectionString()`** - Convert to LINQ-to-SQL connection format
- **`MsSqlIdentifierEscape()`** - Escape SQL Server identifiers
- **Line breaking and special format utilities**

#### **Character Access (4 methods)**
- **`First()` / `FirstOrDefault(defaultChar)`** - Get first character safely
- **`Last()` / `LastOrDefault(defaultChar)`** - Get last character safely

#### **Modern .NET Features (5+ methods)**
- **`CopyTo(Span<char> target)`** - Copy to span for zero-allocation scenarios
- **Span-based operations** with `ReadOnlySpan<char>` support
- **Performance-optimized implementations** using `stackalloc` for small strings

---

### Collection Extensions (`Collections`, `Generic Collections`, `Concurrent Collections`)
**350+ methods for enhanced collection operations - comprehensive collection manipulation**

#### **Dictionary Extensions (`IDictionary<TKey, TValue>`) - 34 methods**
**Thread-safe and performance-optimized dictionary operations**

- **`AddRange(keyValuePairs)`** - Bulk addition operations
- **`GetValueOrDefault(key, defaultValue)`** - Safe value retrieval
- **`GetValueOrNull(key)`** - Null-safe value retrieval
- **`AddOrUpdate(key, value)`** - Upsert operations
- **`GetOrAdd(key, valueFactory)`** - Lazy value addition
- **`TryAdd(key, value)` / `TryRemove(key, out value)`** - Safe modifications
- **`TryUpdate(key, newValue, comparisonValue)`** - Conditional updates
- **`CompareTo(other, keyComparer, valueComparer)`** - Dictionary comparison

#### **Generic Collection Extensions - 213+ methods**
**Comprehensive extensions for List, HashSet, Queue, Stack, LinkedList, and more**

- **List Extensions (28 methods)** - TrySetFirst/Last/Item, RemoveEvery, Swap, Shuffle, Permutate, BinarySearchIndex variants
- **HashSet Extensions (4 methods)** - CompareTo, ContainsNot, TryAdd, TryRemove
- **Queue Extensions (10 methods)** - PullTo variants, PullAll, Pull, AddRange, Add, Fetch, TryDequeue
- **Stack Extensions (11 methods)** - PullTo variants, PullAll, Pull, Exchange, Invert, AddRange, Add, Fetch
- **LinkedList Extensions (9 methods)** - Enhanced navigation and manipulation
- **Collection Extensions (9 methods)** - General ICollection<T> utilities
- **Enumerable Extensions (~115+ methods)** - T4-generated LINQ-style operations
- **KeyValuePair Extensions (1 method)** - Reverse key-value pairs

#### **Concurrent Collection Extensions - 21 methods**
**Thread-safe collection operations for high-performance concurrent scenarios**

- **ConcurrentDictionary Extensions (7 methods)** - Enhanced atomic operations
- **ConcurrentQueue Extensions (6 methods)** - Advanced queue operations with bulk processing
- **ConcurrentStack Extensions (8 methods)** - Enhanced stack operations with safety guarantees

#### **Specialized Collection Extensions - 14 methods**
- **StringDictionary Extensions (1 method)** - String-specific dictionary optimizations
- **StringCollection Extensions (1 method)** - String collection utilities
- **BitArray Extensions (2 methods)** - Bit manipulation operations
- **LINQ Extensions (8 methods)** - IQueryable enhancements
- **ObjectModel Collection Extensions (2 methods)** - Observable collections

---

### File System Extensions
**400+ methods for comprehensive file system operations**

#### FileInfo Extensions (`FileInfo`)
**Advanced file operations with async support and performance optimizations**

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
**Enhanced directory operations and navigation**

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

#### **Stream Extensions (`Stream`)**
**70+ methods for enhanced stream operations**

- **Primitive I/O Operations** - Read/Write for all primitive types (bool, byte, short, int, long, float, double, decimal)
- **Endianness Support** - Big-endian and little-endian operations for cross-platform compatibility
- **String Operations** - Length-prefixed, zero-terminated, and fixed-length string reading/writing
- **Advanced Reading** - Struct serialization, positioned reads with seek origin support
- **Async Operations** - Complete async/await support for all positioned operations
- **Stream Analysis** - End-of-stream detection, stream-to-array conversion
- **Buffer Management** - High-performance buffer management using thread-static and shared buffers

#### **Path & FileSystem Extensions**
**20+ methods for path manipulation and file system utilities**

- **Temporary Resource Management** - Advanced temporary file/directory creation with auto-cleanup
- **Cross-Platform Support** - Multi-OS temporary directory resolution (Windows, Linux, macOS, etc.)
- **UNC Path Operations** - Network path parsing and manipulation
- **Relative Path Calculation** - Calculate relative paths between file system objects
- **Volume Operations** - System volume enumeration and mount point management

---

### Math & Numeric Extensions
**900+ high-performance mathematical operations - the most comprehensive math library for .NET**

#### **Bit Manipulation Extensions (60+ methods)**
*Comprehensive bitwise operations with hardware intrinsics support*

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

#### **Advanced Mathematical Functions (80+ methods)**
*High-precision implementations with custom algorithms*

**Standard Math Functions:**
- **`Pow(exponent)` / `Sqrt()` / `Cbrt()`** - Power, square root, cube root
- **`Floor()` / `Ceiling()` / `Truncate()`** - Rounding operations with MidpointRounding support
- **`Round(decimals, midpointRounding)`** - Advanced rounding with banker's rounding
- **`Abs()` / `Sign()`** - Absolute value and sign extraction
- **`LogN(base)` / `Log()` / `Log10()` / `Log2()`** - Logarithmic functions
- **`Exp()`** - Exponential function with Taylor series for decimal precision

**Trigonometric Functions (30+ methods):**
- **`Sin()` / `Cos()` / `Tan()`** - Basic trigonometric functions
- **`Sinh()` / `Cosh()` / `Tanh()`** - Hyperbolic functions
- **`Cot()` / `Coth()`** - Cotangent and hyperbolic cotangent
- **`Csc()` / `Csch()`** - Cosecant and hyperbolic cosecant
- **`Sec()` / `Sech()`** - Secant and hyperbolic secant
- **`Asin()` / `Acos()` / `Atan()`** - Inverse trigonometric functions
- **`Arsinh()` / `Arcosh()` / `Artanh()`** - Inverse hyperbolic functions
- **`Acot()` / `Asec()` / `Acsc()` / `Arcoth()` / `Arsech()` / `Arcsch()`** - Extended inverse functions

#### **Arithmetic Operations (200+ methods)**
*Generated for all numeric types: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal*

**Basic Arithmetic:**
- **`Add()` / `Subtract()` / `MultipliedWith()` / `DividedBy()`** - Functional-style arithmetic
- **`Squared()` / `Cubed()`** - Common power operations with overflow checking
- **`Average(other)`** - Precise average calculation avoiding overflow
- **`FusedMultiplyAdd()` / `FusedMultiplySubtract()`** - Hardware-accelerated fused operations
- **`FusedDivideAdd()` / `FusedDivideSubtract()`** - Custom fused divide operations

**Shift Operations (Integer Types):**
- **`ArithmeticShiftLeft()` / `ArithmeticShiftRight()`** - Sign-preserving bit shifts
- **`LogicalShiftLeft()` / `LogicalShiftRight()`** - Zero-fill bit shifts

#### **Comparison & Range Operations (180+ methods)**
*Comprehensive comparison operations for all numeric types*

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

#### **Type-Safe Unsigned Wrappers (135 methods)**
*UnsignedFloat, UnsignedDouble, UnsignedDecimal - compile-time negative value prevention*

**Complete Arithmetic Support:**
- All standard operators (+, -, *, /, %) with overflow checking
- Complete comparison operators with optimized implementations  
- Implicit conversions from unsigned integer types
- Explicit conversions from signed types with validation
- `IComparable`, `IConvertible`, `IFormattable` interface implementations
- Modern .NET support: `ISpanParsable`, `ISpanFormattable` for .NET 7+

**Mathematical Operations:**
- All standard math functions adapted for unsigned constraints
- Special handling for operations that could produce negative results
- Performance optimizations leveraging the non-negative guarantee

#### **Utility Extensions (94 methods)**

**SIMD-Optimized Operations:**
- **`Min()` / `Max()`** - Hardware-accelerated min/max operations using unsafe code
- **`Bits()`** - Get IEEE 754 bit representation for floating-point types
- **`Equ()`** - High-precision equality comparison with configurable epsilon

**Repetition Extensions (32 methods):**
*For all integer types (byte through long)*
- **`Times(Action)`** - Execute action N times (optimized loop unrolling)
- **`Times(Action<T>)`** - Execute action N times with index parameter
- **`Times(string)`** - Repeat string N times with StringBuilder optimization
- **`Times(char)`** - Repeat character N times using efficient string constructor

#### **Key Performance Features:**

1. **Hardware Intrinsics Integration** - Leverages modern CPU features (SIMD, FMA, etc.)
2. **Aggressive Inlining** - Extensive use of `MethodImplOptions.AggressiveInlining`
3. **Custom High-Precision Algorithms** - Taylor series, Newton-Raphson methods for decimal precision
4. **Overflow Detection** - Safe arithmetic operations with overflow checking
5. **Epsilon-Based Comparisons** - Configurable precision for floating-point operations
6. **SIMD Optimizations** - Vectorized operations where hardware supports it
7. **Lookup Tables** - Pre-computed tables for bit manipulation operations
8. **Branch Reduction** - Optimized conditional logic using bitwise tricks

---

### DateTime & TimeSpan Extensions
**Enhanced date and time manipulation**

- **DateTime Operations** - Advanced date/time arithmetic and formatting
- **TimeSpan Operations** - Enhanced duration calculations and utilities

---

### Threading & Task Extensions
**200+ methods for advanced concurrent programming and synchronization**

#### **Task Management Extensions - 8 methods**
- **`DeferredTask` (2 methods)** - Lazy task execution with dependency management
- **`ScheduledTask` (1 method)** - Cron-like scheduled task execution  
- **`Sequential` (1 method)** - Sequential task processor with ordering guarantees
- **`Task` (1 method)** - Enhanced task utilities
- **`Future<T>` (1 method)** - Future/promise pattern implementation
- **Related task utilities (2 methods)** - Additional task management features

#### **Interlocked Extensions - 49 methods**
**Advanced atomic operations with enum support and flag manipulation**

- **Standard Atomic Operations (15 methods)** - Enhanced versions of standard Interlocked operations
- **T4-Generated Enum Operations (34 methods)** - Type-safe atomic operations for enums:
  - **Flag Operations** - Set, clear, toggle, and test flags atomically
  - **Enum Arithmetic** - Add, subtract, increment, decrement enum values safely
  - **Conditional Updates** - CompareExchange operations with strong typing
  - **Read Operations** - Type-safe atomic reads with memory barriers

#### **Synchronization Primitives - 8 methods**
**Enhanced synchronization with advanced features**

- **`SemaphoreSlim` Extensions (2 methods)** - Enhanced semaphore operations
- **`ManualResetEvent` Extensions (1 method)** - Event signaling utilities
- **`Thread` Extensions (3 methods)** - Thread management and utilities
- **`Timer` Extensions (3 methods)** - Enhanced timer operations

#### **Advanced Threading Utilities**
- **`CallOnTimeout`** - Timeout-based callback execution with cancellation
- **`Event`** - Enhanced event signaling with multiple listeners
- **`HighPrecisionTimer`** - High-resolution timing for performance-critical code

---

### Reflection & Type Extensions
**Advanced reflection utilities with performance optimizations**

#### Type Operations
- **Enhanced type inspection** and metadata operations
- **Assembly Extensions** - Advanced assembly operations
- **MethodInfo Extensions** - Method reflection utilities
- **PropertyInfo Extensions** - Property reflection utilities

---

### Data & ComponentModel Extensions
**Enhanced data binding and database operations**

#### Data Extensions
- **`DataRecord` / `DataRow` / `DataTable` Extensions** - Enhanced data operations
- **LINQ to Data Extensions** - Advanced querying
- **SQL Client Extensions** - Database utilities

#### ComponentModel Extensions
- **`BindingList` / `SortableBindingList`** - Enhanced data binding
- **Property Change Notifications** - Advanced MVVM support
- **Attribute Extensions** - Metadata utilities

---

## 🔧 Types - New Types Added to the System

### Collection Types
**High-performance collection implementations**

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
**Comprehensive change detection and tracking**

- **`IChangeSet<TItem>`** - Interface for change tracking operations
- **`ChangeSet<TItem>`** - Complete change tracking implementation
- **`ChangeType`** - Enumeration: `Added`, `Removed`, `Changed`, `Equal`

---

### File Operations Framework
**Advanced file comparison and processing**

#### File Comparison
- **`IFileComparer`** - Base interface for file comparison strategies
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
**Comprehensive hashing algorithms and security utilities**

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
**Advanced concurrent programming constructs**

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
**Advanced property and state management utilities**

#### Smart Properties
- **`FastLazy<T>`** - High-performance lazy initialization
- **`RealtimeProperty<T>`** - Real-time property with change notifications
- **`SlowProperty<T>`** - Throttled property updates for expensive operations
- **`IndexedProperty<T>`** - Property with indexer support
- **`StaticMethodLocal<T>`** - Static method-local storage

---

### Utility Types
**Essential utility types for common programming patterns**

#### Core Utilities
- **`Range`** - Enhanced range operations and arithmetic
- **`Span` Enhancements** - Extended span utilities and operations

#### Validation Framework
- **`Against`** - Comprehensive parameter validation with performance optimization
- **`AlwaysThrow`** - Exception throwing utilities with stack trace optimization

#### Text Processing
- **`TextAnalyzer`** - Advanced text analysis with readability metrics
- **`ReadabilityScoreCalculator`** - Multiple readability algorithm implementations

---

## 🚀 Performance Features

### Aggressive Optimizations
- **Unsafe Code Blocks** - Direct memory manipulation for maximum performance
- **SIMD-like Operations** - Vectorized operations using block processing
- **Aggressive Inlining** - Extensive use of `MethodImplOptions.AggressiveInlining`
- **Stack Allocation** - `stackalloc` for temporary buffers
- **Span<T> and Memory<T>** - Modern .NET memory management

### Memory Efficiency
- **Zero-Allocation Paths** - Many operations avoid heap allocations
- **Object Pooling** - Reusable object patterns where applicable
- **Block-based Operations** - `Block32`, `Block64` for efficient memory operations
- **Bounds Check Elimination** - Careful loop construction to help JIT optimize

---

## 🔧 Installation & Usage

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

## 🎯 Target Frameworks

**Full multi-targeting support:**
- .NET Framework: `net35`, `net40`, `net45`, `net48`
- .NET Standard: `netstandard2.0`
- .NET Core/5+: `netcoreapp3.1`, `net6.0`, `net7.0`, `net8.0`, `net9.0`

---

## 🌟 Unprecedented Scale & Impact

### **By The Numbers**
- **3,300+ Extension Methods** - More than any other .NET extension library
- **50+ .NET Types Extended** - Covers virtually every major .NET type
- **15+ Data Types** - Type-safe parsing for all common data types
- **Multiple .NET Versions** - Universal compatibility from .NET 3.5 to .NET 9.0
- **Years of Development** - Represents thousands of hours of optimization and testing
- **Production-Ready** - Used in enterprise applications already

### **Performance Engineering**
- **Hardware Intrinsics** - Leverages CPU-specific optimizations (SIMD, FMA, etc.)
- **Unsafe Code Optimization** - Direct memory manipulation where beneficial
- **T4 Code Generation** - Eliminates runtime overhead through compile-time generation
- **Aggressive Inlining** - Micro-optimizations throughout the codebase
- **Memory Pool Usage** - Reduces garbage collection pressure
- **Branch Reduction** - Optimized conditional logic using bitwise operations

### **Enterprise Features**
- **Thread-Safe Operations** - Atomic operations and concurrent collection support
- **Robust Error Handling** - Comprehensive validation with meaningful error messages
- **Cultural Awareness** - Proper globalization support for international applications
- **Backwards Compatibility** - Supports legacy .NET Framework applications
- **Zero Breaking Changes** - Additive-only API design philosophy

### **Developer Productivity Impact**
This library eliminates the need to write thousands of lines of boilerplate code:
- **String manipulation** - No more custom parsing or formatting logic
- **Mathematical operations** - Advanced algorithms ready to use
- **Collection handling** - Sophisticated data structure operations
- **File system operations** - Enterprise-grade I/O with async support
- **Threading utilities** - Complex synchronization made simple

---

## 📈 Testing & Quality

- **600+ Unit Tests** with comprehensive coverage
- **Performance Benchmarks** for critical operations  
- **Cross-Platform CI/CD** on Windows, Linux, and macOS
- **Memory Leak Testing** for all allocation-heavy operations
- **Thread Safety Testing** for concurrent operations

---

## 🤝 Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for detailed guidelines on:
- Code style and conventions
- Performance requirements
- Testing categories and patterns
- Architecture principles

---

## 📄 License

[LGPL-3.0-or-later](https://licenses.nuget.org/LGPL-3.0-or-later) - Use freely, contribute back improvements.
