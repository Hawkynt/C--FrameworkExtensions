# Corlib Extensions

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)
[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Corlib.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Corlib)](https://www.nuget.org/packages/FrameworkExtensions.Corlib/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

## Overview

Extension methods for core .NET types, providing additional functionality across strings, collections, math, I/O, threading, and other common operations. Supports .NET 3.5 through .NET 9.0.

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

> **Completeness Note**: This README serves as a comprehensive reference document covering all major extension method categories and new types in the library. While the library contains lots of extension methods, this documentation should provide:
> - All extension method **categories** with representative methods
> - All **new types** (classes, structs, records, interfaces, enums)
> - Complete coverage of **public API surface**
>
> For specific overload details and parameter variations, use IntelliSense in your IDE.

### Object Extensions (`object`)

General-purpose object manipulation and reflection utilities

- **Null Checking** - `IsNull()`, `IsNotNull()` - Null reference checks with nullable annotations
- **Type Checking** - `Is<T>()`, `TypeIsAnyOf()` - Runtime type verification and pattern matching
- **Type Casting** - `As<T>()` - Safe type casting with generic support
- **Conditional Operations** - `IsTrue()`, `IsFalse()`, `IsAnyOf()` - Predicate-based conditionals and set membership
- **Conditional Execution** - `WhenNull()`, `WhenNotNull()` - Execute actions based on null state
- **Reflection Access** - `GetProperties()`, `GetFields()` - Retrieve object members with filtering options
- **State Management** - `ResetDefaultValues()` - Reset object fields/properties to their default values

### Boolean Extensions (`bool`)

Boolean logic and conditional operations

- **Logical Operations** - `And()`, `Or()`, `Xor()`, `Nand()`, `Nor()`, `Equ()`, `Not()` - Functional-style boolean operations
- **Conditional Execution** - `When()`, `WhenTrue()`, `WhenFalse()` - Execute actions/functions based on boolean value (supports both Action and Func variants)
- **String Conversion** - `ToYesOrNoString()`, `ToOneOrZeroString()`, `ToTrueOrFalseString()` - Convert to human-readable strings

### Char Extensions (`char`)

Character testing and manipulation

- **Whitespace** - `IsWhiteSpace()`, `IsNullOrWhiteSpace()`, `IsNotNullOrWhiteSpace()` - Whitespace checking including null character
- **Digit Classification** - `IsDigit()`, `IsNotDigit()` - Check if character is a digit
- **Case Classification** - `IsUpper()`, `IsNotUpper()`, `IsLower()`, `IsNotLower()` - Case checking
- **Letter Classification** - `IsLetter()`, `IsNotLetter()` - Check if character is a letter
- **Control Characters** - `IsControl()`, `IsNotControl()`, `IsControlButNoWhiteSpace()` - Control character detection
- **Case Conversion** - `ToUpper()`, `ToLower()` - Case conversion with optional culture parameter
- **Set Membership** - `IsAnyOf()` - Check if character is in a set
- **Repetition** - `Repeat(count)` - Create string by repeating character N times

### Nullable Extensions (`T?`)

Operations for nullable value types

- **Null Checking** - `IsNull()`, `IsNotNull()` - Check if nullable has value (with NotNullWhen attributes)

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

#### Case Conversion

Intelligent case transformations with word boundary detection

- **`ToPascalCase()` / `ToPascalCaseInvariant()`** - Convert to PascalCase (e.g., "hello_world" → "HelloWorld")
- **`ToCamelCase()` / `ToCamelCaseInvariant()`** - Convert to camelCase (e.g., "hello_world" → "helloWorld")
- **`ToSnakeCase()` / `ToSnakeCaseInvariant()`** - Convert to snake_case (e.g., "HelloWorld" → "hello_world")
- **`ToUpperSnakeCase()` / `ToUpperSnakeCaseInvariant()`** - Convert to UPPER_SNAKE_CASE (e.g., "HelloWorld" → "HELLO_WORLD")
- **`ToKebabCase()` / `ToKebabCaseInvariant()`** - Convert to kebab-case (e.g., "HelloWorld" → "hello-world")
- **`ToUpperKebabCase()` / `ToUpperKebabCaseInvariant()`** - Convert to UPPER-KEBAB-CASE (e.g., "HelloWorld" → "HELLO-WORLD")
- **`UpperFirst()` / `UpperFirstInvariant()`** - Capitalize first character only (e.g., "hello" → "Hello")
- **`LowerFirst()` / `LowerFirstInvariant()`** - Lowercase first character only (e.g., "Hello" → "hello")

#### String Manipulation & Modification

Common string transformations and editing operations

- **`ExchangeAt(index, replacement)`** - Replace character at specific position (e.g., "hello".ExchangeAt(1, 'a') → "hallo")
- **`ExchangeAt(index, count, replacement)`** - Replace substring range (e.g., "hello".ExchangeAt(1, 2, "ay") → "haylo")
- **`Repeat(count)`** - Repeat string N times (e.g., "ab".Repeat(3) → "ababab")
- **`RemoveFirst(count)` / `RemoveLast(count)`** - Remove N characters from start/end (e.g., "hello".RemoveFirst(2) → "llo")
- **`RemoveAtStart(what)` / `RemoveAtEnd(what)`** - Remove specific prefix/suffix (e.g., "hello".RemoveAtEnd("lo") → "hel")
- **`SubString(start, end)`** - Alternative substring using end index instead of length
- **`Left(count)` / `Right(count)`** - Get N leftmost/rightmost characters safely (returns shorter if string too short)
- **`Split(int)` / `Split(Regex)`** - Split into fixed-size chunks or by regex pattern

#### Replace Operations

Flexible string replacement methods

- **`ReplaceFirst(what, replacement)`** - Replace only first occurrence (e.g., "hello".ReplaceFirst("l", "L") → "heLlo")
- **`ReplaceLast(what, replacement)`** - Replace only last occurrence (e.g., "hello".ReplaceLast("l", "L") → "helLo")
- **`MultipleReplace(dict)`** - Replace multiple patterns at once (e.g., "a b c".MultipleReplace({{"a","x"},{"b","y"}}) → "x y c")
- **`MultipleReplace(target, replacements)`** - Replace target with one of many values
- **`ReplaceRegex(pattern, replacement)`** - Regex-based replacement with capture groups
- **`Replace()` with limits** - Limit number of replacements (e.g., Replace("l", "L", maxCount: 1))

#### StartsWith/EndsWith Operations

Comprehensive prefix and suffix checking with flexible comparison

- **`StartsWith()`** - Check if string starts with char/string (supports StringComparison)
- **`StartsNotWith()`** - Negated prefix check
- **`StartsWithAny()`** - Check if starts with any of multiple values (e.g., "hello".StartsWithAny("hi", "he") → true)
- **`StartsNotWithAny()`** - Check if doesn't start with any value
- **`EndsWith()`** - Check if string ends with char/string (supports StringComparison)
- **`EndsNotWith()`** - Negated suffix check
- **`EndsWithAny()`** - Check if ends with any of multiple values
- **`EndsNotWithAny()`** - Check if doesn't end with any value

#### Contains & Search Operations

Advanced substring searching and set membership

- **`Contains()`** - Check if string contains substring (with StringComparison support)
- **`ContainsNot()`** - Check if string doesn't contain substring
- **`ContainsAll()`** - Check if contains all of multiple substrings (e.g., "hello world".ContainsAll("hello", "world") → true)
- **`ContainsAny()`** - Check if contains any of multiple substrings
- **`ContainsNotAny()`** - Check if doesn't contain any of multiple substrings
- **`IndexOf()` variants** - Enhanced searching with culture/comparison options
- **`IsAnyOf()`** - Check if string equals any in a set (e.g., "cat".IsAnyOf("dog", "cat", "bird") → true)
- **`IsNotAnyOf()`** - Check if string doesn't equal any in a set

#### Null & State Checking

Comprehensive null, empty, and whitespace validation

- **`IsNull()` / `IsNotNull()`** - Check if string is null
- **`IsEmpty()` / `IsNotEmpty()`** - Check if string is "" (empty but not null)
- **`IsNullOrEmpty()` / `IsNotNullOrEmpty()`** - Combined null or empty check
- **`IsNullOrWhiteSpace()` / `IsNotNullOrWhiteSpace()`** - Check for null, empty, or only whitespace
- **`IsWhiteSpace()` / `IsNotWhiteSpace()`** - Check if string contains only whitespace (but not empty)
- **`DefaultIf()` / `DefaultIfEmpty()` / `DefaultIfNullOrEmpty()`** - Return default value based on state (e.g., "".DefaultIfEmpty("N/A") → "N/A")

#### Text Processing & Analysis

Linguistic analysis and text processing utilities

- **`SanitizeForFileName()`** - Remove/replace invalid filesystem characters (e.g., "file:name".SanitizeForFileName() → "file_name")
- **`GetSoundexRepresentation()`** - Phonetic encoding for fuzzy matching (e.g., "Robert".GetSoundexRepresentation() → "R163")
- **`TextAnalysis()`** - Comprehensive text analysis returning:
  - **Word Count** - Total words and unique words
  - **Sentence Count** - Number of sentences (handles abbreviations)
  - **Syllable Count** - Syllables per word (multi-language support)
  - **Word Frequency** - Histogram of word occurrences
  - **Readability Metrics** - Flesch-Kincaid, Gunning Fog, etc.
- **`Truncate()`** - Shorten text with ellipsis (e.g., "Long text".Truncate(8) → "Long te...")
- **`WordWrap()`** - Wrap text to specified line width
- **`RemoveDiacritics()`** - Remove accents and diacritical marks (e.g., "café" → "cafe")

#### Cryptography & Hashing

- **`ComputeHash<TAlgorithm>()`** - Generic hash computation with any HashAlgorithm
- **`ComputeHash(hashAlgorithm)`** - Hash with specific algorithm instance

#### Regular Expressions

- **`IsMatch(regex)` / `IsNotMatch(regex)`** - Pattern matching with Regex objects
- **`IsMatch(pattern, options)` / `IsNotMatch(pattern, options)`** - Pattern matching with string patterns
- **`Matches(pattern, options)`** - Get all pattern matches
- **`MatchGroups(pattern, options)`** - Extract regex capture groups
- **`AsRegularExpression(options)`** - Convert string to compiled Regex

#### Advanced Formatting

- **`FormatWith(parameters)`** - Enhanced string.Format with better error handling
- **`FormatWithEx(fields, comparer)`** - Template-based formatting with custom field resolution
- **`FormatWithEx(KeyValuePair<string, object>[])`** - Formatting with key-value pairs
- **`FormatWithEx(IDictionary<string, string>)`** - Dictionary-based formatting
- **`FormatWithObject<T>(object)`** - Format using object properties via reflection

#### Type-Safe Parsing

Generated via T4 templates for 15 data types: Float, Double, Decimal, Byte, SByte, UInt16, Int16, UInt32, Int32, UInt64, Int64, TimeSpan, DateTime, Boolean, Color

**Each type provides:**

- **`Parse{Type}()`** - Basic parsing
- **`Parse{Type}(IFormatProvider/NumberStyles)`** - Culture-aware parsing
- **`TryParse{Type}(out result)`** - Safe parsing variants
- **`Parse{Type}OrDefault(defaultValue)`** - Parsing with fallback values
- **`Parse{Type}OrNull()`** - Nullable parsing for value types
- **ReadOnlySpan&lt;char&gt; support** for zero-allocation parsing

#### Database & Special Formats

- **`ToLinq2SqlConnectionString()`** - Convert to LINQ-to-SQL connection format
- **`MsSqlIdentifierEscape()`** - Escape SQL Server identifiers
- **Line breaking and special format utilities**

#### Character Access

- **`First()` / `FirstOrDefault(defaultChar)`** - Get first character safely
- **`Last()` / `LastOrDefault(defaultChar)`** - Get last character safely

#### Modern .NET Features

- **`CopyTo(Span<char> target)`** - Copy to span for zero-allocation scenarios
- **Span-based operations** with `ReadOnlySpan<char>` support
- **Performance-optimized implementations** using `stackalloc` for small strings

---

### Enum Extensions (`Enum`)

Enhanced enum operations with attribute support

- **Attribute Access** - `GetFieldDescription<T>()`, `GetFieldDisplayName<T>()`, `GetFieldAttribute<T,TAttr>()` - Retrieve attributes from enum values
- **String Conversion** - `ToString<T,TAttr>()`, `ToStringOrDefault<T,TAttr>()` - Convert enum to string using attribute values
- **Parsing** - `ParseEnum<T,TAttr>()`, `ParseEnumOrDefault<T,TAttr>()` - Parse strings to enum values via attributes
- **Flag Operations** - `HasFlag()`, `SetFlag()`, `ClearFlag()`, `ToggleFlag()` - Manipulate flag enums
- **Enumeration** - `GetValues()`, `GetNames()`, `GetFlags()` - Retrieve all enum values/names/flags

### Random Extensions (`Random`)

Advanced random generation with type-specific methods

- **Password Generation** - `GeneratePassword()` - Generate secure passwords with customizable rules
- **Boolean** - `GetBoolean()` - Random true/false values
- **Dice Rolling** - `RollADice()` - Simulate dice rolls (default 6-sided)
- **Type-Specific Generators** - `GetValueFor<T>()` - Generate random value for any supported type
- **Integer Ranges** - `GetInt8()`, `GetInt16()`, `GetInt32()`, `GetInt64()`, `GetUInt8()`, `GetUInt16()`, `GetUInt32()`, `GetUInt64()` - Full range random integers
- **Floating-Point** - `GetFloat()`, `GetDouble()`, `GetDecimal()` - Random floating-point with control over special values (NaN, Infinity)
- **Characters** - `GetChar()` - Random character with filters (ASCII, control chars, whitespace, surrogates)
- **Strings** - `GetString()` - Random strings with length constraints
- **Ranged Double** - `NextDouble(min, max)` - Random double within specified range

### Console Extensions (`Console`)

Enhanced console I/O operations

- **Colored Output** - `WriteLineColored()`, `WriteColored()` - Write text with foreground/background colors
- **Formatted Output** - `WriteLineFormatted()` - Write formatted text with color codes
- **Input** - `ReadLineSecure()` - Read input without echoing (for passwords)
- **Progress Indicators** - `WriteProgress()` - Display progress bars and indicators

### Convert Extensions

Additional encoding and conversion utilities

- **Base91 Encoding** - `ToBase91String()`, `FromBase91String()` - Efficient base91 encoding (more compact than base64)
- **Quoted-Printable** - `ToQuotedPrintableString()`, `FromQuotedPrintableString()` - Email-compatible encoding

### Uri Extensions (`Uri`)

Web resource access and manipulation

- **Content Retrieval** - `ReadAllText()`, `ReadAllBytes()` - Download content from URIs
- **Async Operations** - `ReadAllTextTaskAsync()`, `ReadAllBytesTaskAsync()` - Asynchronous download methods
- **File Download** - `DownloadToFile()` - Download content directly to file
- **URI Manipulation** - `BaseUri()`, `Path()` - Construct and manipulate URIs
- **Response Handling** - `GetResponseUri()` - Get final URI after redirects
- **HTTP Support** - Custom headers, POST data, retry logic for all methods

### StringBuilder Extensions (`StringBuilder`)

StringBuilder manipulation and utilities

- **Character Operations** - `Append()` overloads for various types
- **Line Operations** - `AppendLine()` variants with formatting
- **Conditional Append** - `AppendIf()`, `AppendLineIf()` - Conditional appending
- **Modification** - `Replace()`, `Remove()`, `Insert()` - Enhanced manipulation methods
- **Inspection** - `Contains()`, `StartsWith()`, `EndsWith()` - Content checking

### Regex Extensions (`Regex`, `Match`)

Regular expression utilities

- **Match Operations** - `MatchAll()`, `GetMatches()` - Retrieve all matches
- **Replace Operations** - `ReplaceWith()` - Functional replacement patterns
- **Testing** - `IsMatch()`, `HasMatch()` - Pattern testing
- **Group Access** - `GetGroupValue()`, `GetGroupValues()` - Extract capture groups
- **Match Extensions** - `GetValue()`, `GetValues()` - Value extraction from Match objects

### CultureInfo Extensions (`CultureInfo`)

Culture and localization utilities

- **Culture Operations** - `IsNeutral()`, `IsSpecific()` - Culture type checks
- **Hierarchy** - `GetParent()`, `GetAncestors()` - Culture hierarchy navigation
- **Comparison** - `IsAncestorOf()`, `IsDescendantOf()` - Culture relationship checks

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

#### BinaryReader Extensions (`BinaryReader`)

Binary reading utilities

- **Extended Reads** - `ReadBytes()`, `ReadString()` with various encodings
- **Positioned Reading** - Read at specific positions without seeking
- **Array Reading** - Read arrays of primitives efficiently

#### TextReader Extensions (`TextReader`)

Text reading utilities

- **Line Operations** - `ReadLines()`, `ReadAllLines()` - Enumerate or read all lines
- **Buffered Reading** - `ReadBlock()` - Read fixed-size blocks
- **Async Operations** - Async versions of read operations

#### DriveInfo Extensions (`DriveInfo`)

Drive information and management

- **Drive Operations** - `GetFreeSpace()`, `GetTotalSpace()` - Space queries
- **Drive Checks** - `IsReady()`, `IsFixed()`, `IsRemovable()` - Drive type checking
- **Volume Information** - `GetVolumeLabel()`, `SetVolumeLabel()` - Label management

#### Volume Extensions (`Volume`)

System volume enumeration and management

- **Volume Discovery** - Enumerate all system volumes
- **Mount Point Operations** - Get/set volume mount points
- **Volume Properties** - Access volume-specific information

#### Path & FileSystem Extensions

Path manipulation and file system utilities

- **Temporary Resource Management** - Temporary file/directory creation with auto-cleanup
- **Cross-Platform Support** - Multi-OS temporary directory resolution
- **UNC Path Operations** - Network path parsing and manipulation
- **Relative Path Calculation** - Relative paths between file system objects

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

**Saturating Arithmetic:**

Operations for all integer types (byte, sbyte, short, ushort, int, uint, long, ulong, UInt96, Int96, UInt128, Int128) that clamp results to type bounds instead of overflowing:

- **`SaturatingAdd(value)`** - Add with saturation (overflow → MaxValue, underflow → MinValue)
- **`SaturatingSubtract(value)`** - Subtract with saturation (overflow → MaxValue, underflow → MinValue)
- **`SaturatingMultiply(value)`** - Multiply with saturation (clamps on overflow/underflow)
- **`SaturatingDivide(value)`** - Divide with saturation (signed types: `MinValue / -1` → `MaxValue`)
- **`SaturatingNegate()`** - Negate with saturation (signed types only; `MinValue` → `MaxValue`)

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
- SIMD and Vector optimizations where hardware supports it
- Lookup tables for bit manipulation operations
- Branch reduction using bitwise operations

---

### DateTime Extensions (`DateTime`)

Comprehensive date and time operations

- **Day Boundaries** - `StartOfDay()`, `EndOfDay()` - Get start/end of current day
- **Week Operations** - `AddWeeks()`, `DateOfDayOfCurrentWeek()`, `StartOfWeek()`, `DayInCurrentWeek()` - Week-based calculations
- **Month Boundaries** - `FirstDayOfMonth()`, `LastDayOfMonth()` - Get first/last day of month
- **Year Boundaries** - `FirstDayOfYear()`, `LastDayOfYear()` - Get first/last day of year
- **Comparison** - `Max()`, `Min()` - Compare and return min/max dates
- **Date Ranges** - `DaysTill()` - Enumerate days between dates
- **Sequence Generation** - `Sequence(start, end, step)`, `InfiniteSequence(start, step)` - Generate finite or infinite sequences of DateTime values with a TimeSpan step
- **Subtraction Methods** - `SubstractTicks()`, `SubstractMilliseconds()`, `SubstractSeconds()`, `SubstractMinutes()`, `SubstractHours()`, `SubstractDays()` - Subtraction alternatives to Add with negative values
- **Formatting** - Culture-aware formatting and parsing

### TimeSpan Extensions (`TimeSpan`)

Duration calculations and utilities

- **Arithmetic** - `Multiply()`, `Divide()` - Scalar multiplication and division
- **Comparison** - `IsPositive()`, `IsNegative()`, `IsZero()` - Duration state checks
- **Formatting** - `ToHumanReadable()` - Convert to friendly format (e.g., "2 hours, 30 minutes")
- **Conversion** - `TotalWeeks()` - Get total weeks as double

### Type Extensions (`Type`)

Type reflection and inspection utilities

- **Type Constants** - `TypeVoid`, `TypeBool`, `TypeChar`, `TypeString`, etc. - Static type references
- **Castability** - `IsCastableTo()` - Check if type can be cast to another
- **Hierarchy** - `GetBaseTypes()`, `GetInterfaces()` - Enumerate type hierarchy
- **Attributes** - `HasAttribute<T>()`, `GetAttribute<T>()` - Attribute presence and retrieval
- **Type Checks** - `IsNumeric()`, `IsNullable()`, `IsEnum()`, `IsDelegate()` - Common type category checks
- **Generic Support** - `GetGenericArguments()`, `IsGenericType()` - Generic type inspection
- **Default Values** - `GetDefault()` - Get default value for type

### BitConverter Extensions

Enhanced byte array conversion

- **Primitive Types** - `ToSByte()`, `ToByte()`, `ToShort()`, `ToChar()` - Convert byte arrays to primitives
- **Nullable Support** - `ToNSByte()`, `ToNByte()`, `ToNChar()` - Convert to nullable primitives with null marker
- **Reverse Operations** - `GetBytes()` - Convert primitives (including nullables) to byte arrays
- **Endianness Control** - Methods for both big-endian and little-endian conversions

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
- **Saturating Atomic Operations** - Atomic operations with saturation semantics (int, uint, long, ulong):
  - `SaturatingAdd(ref source, value)` - Atomic saturating add
  - `SaturatingSubtract(ref source, value)` - Atomic saturating subtract
  - `SaturatingMultiply(ref source, value)` - Atomic saturating multiply
  - `SaturatingDivide(ref source, value)` - Atomic saturating divide

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

### Diagnostics Extensions

Process and performance monitoring utilities

#### Process Extensions (`Process`)

- **Process Management** - `Kill()`, `WaitForExit()` - Process lifecycle management
- **Priority** - `SetPriority()`, `GetPriority()` - Process priority management
- **Information** - `GetCommandLine()`, `GetParent()` - Process information retrieval

#### ProcessStartInfo Extensions (`ProcessStartInfo`)

- **Fluent Configuration** - Fluent API for configuring process start parameters
- **Argument Building** - `AddArgument()`, `AddArguments()` - Argument list building
- **Environment** - `AddEnvironmentVariable()` - Environment variable management

#### Stopwatch Extensions (`Stopwatch`)

- **Measurement** - `Measure()` - Measure execution time of actions
- **Reset Operations** - `RestartWith()` - Reset and start with new measurement
- **Timing Utilities** - `GetElapsedAndRestart()` - Get elapsed time and restart

---

### Reflection Extensions

Advanced reflection utilities for types and metadata

#### Assembly Extensions (`Assembly`)

- **Attribute Access** - `GetCustomAttribute<T>()`, `GetCustomAttributes<T>()`, `HasAttribute<T>()` - Retrieve assembly-level attributes
- **Type Discovery** - `GetLoadableTypes()`, `GetTypesImplementing<T>()` - Find types within assemblies
- **Version Info** - `GetFileVersion()`, `GetProductVersion()` - Retrieve version information

#### MethodInfo Extensions (`MethodInfo`)

- **Invocation** - `Invoke<T>()` - Type-safe method invocation
- **Signature Matching** - `MatchesSignature()` - Compare method signatures
- **Extension Method Detection** - `IsExtensionMethod()` - Check if method is an extension method
- **Attribute Retrieval** - `GetCustomAttribute<T>()` - Get method attributes

#### PropertyInfo Extensions (`PropertyInfo`)

- **Value Access** - `GetValue<T>()`, `SetValue<T>()` - Type-safe property access
- **Attribute Retrieval** - `GetCustomAttribute<T>()` - Get property attributes
- **Backing Field Access** - `GetBackingField()` - Access compiler-generated backing fields

#### FieldInfo Extensions (`FieldInfo`)

- **Value Access** - `GetValue<T>()`, `SetValue<T>()` - Type-safe field access
- **Attribute Retrieval** - `GetCustomAttribute<T>()` - Get field attributes

#### ParameterInfo Extensions (`ParameterInfo`)

- **Attribute Access** - `GetCustomAttribute<T>()`, `HasAttribute<T>()` - Retrieve parameter attributes
- **Default Values** - `GetDefaultValue<T>()` - Get parameter default value

---

### Networking Extensions

Network-related utilities and operations

#### IPAddress Extensions (`IPAddress`)

- **Address Manipulation** - `IsInRange()`, `IsPrivate()`, `IsLoopback()` - IP address classification
- **Subnet Operations** - `GetNetworkAddress()`, `GetBroadcastAddress()` - Calculate network addresses
- **Conversion** - `ToUInt32()`, `ToBytes()` - Convert IP addresses to numeric formats

#### WebHeaderCollection Extensions (`WebHeaderCollection`)

- **Header Access** - Simplified access to common HTTP headers
- **Bulk Operations** - `AddRange()`, `SetRange()` - Add/set multiple headers at once

### Security Extensions

Cryptographic and security utilities

#### HashAlgorithm Extensions

- **File Hashing** - `ComputeHash()` - Compute file hashes
- **Stream Hashing** - `ComputeHash()` - Compute stream hashes
- **String Hashing** - Extensions for computing hashes from strings

#### SecureString Extensions (`SecureString`)

- **Conversion** - `ToUnsecureString()`, `ToByteArray()` - Convert SecureString to usable formats
- **Comparison** - `EqualsSecure()` - Secure string comparison

---

### LINQ Extensions

Enhanced LINQ operations and queries

#### IQueryable Extensions (`IQueryable<T>`)

- **Query Building** - `Where()`, `OrderBy()`, `Select()` enhancements
- **Paging** - `Skip()`, `Take()` - Pagination support
- **Expression Utilities** - Expression tree manipulation

#### Enumerable Extensions (See Collection Extensions)

Comprehensive LINQ-style operations documented in Collections section above

---

### XML Extensions

XML manipulation and querying utilities

#### XmlNode Extensions (`XmlNode`)

- **Navigation** - `GetChildNodes()`, `GetDescendants()` - Node traversal
- **Query** - `SelectNodes()`, `SelectSingleNode()` enhancements
- **Modification** - `AddChild()`, `RemoveChild()` - Node manipulation
- **Attribute Access** - `GetAttribute()`, `SetAttribute()` - Attribute management

#### XmlAttributeCollection Extensions (`XmlAttributeCollection`)

- **Collection Operations** - Enhanced attribute collection manipulation
- **Lookup** - `ContainsAttribute()`, `TryGetAttribute()` - Attribute retrieval

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

#### Text Processing

- **`TextAnalyzer`** - Advanced text analysis with readability metrics
- **`ReadabilityScoreCalculator`** - Multiple readability algorithm implementations

---

### String Types

Specialized string types for interoperability, memory efficiency, and encoding-specific scenarios.

#### Overview

| Type | Storage | Encoding | Behavior |
|------|---------|----------|----------|
| `StringZ` | `string` | UTF-16 | Cuts at first '\0' |
| `AsciiZ` | `byte[]` (7-bit packed) | 7-bit ASCII | Cuts at first 0x00 |
| `AnsiZ` | `byte[]` | Windows-1252 | Cuts at first 0x00 |
| `AsciiString` | `byte[]` (7-bit packed) | 7-bit ASCII | Full content preserved |
| `AnsiString` | `byte[]` | Windows-1252 | Full content preserved |
| `FixedString` | `char[]` | UTF-16 | Fixed capacity via constructor |
| `FixedAscii` | `byte[]` (7-bit packed) | 7-bit ASCII | Fixed capacity via constructor |
| `FixedAnsi` | `byte[]` | Windows-1252 | Fixed capacity via constructor |

**Memory Efficiency**: ASCII types use 7-bit packing, storing 8 characters in 7 bytes (12.5% memory savings). SIMD-accelerated operations for validation, packing, and unpacking.

#### Zero-Terminated Strings

Null-terminated string types for C/native interoperability. Content after the first NUL character is discarded.

- **`StringZ`** - Zero-terminated UTF-16 string wrapper around `string`
- **`AsciiZ`** - Zero-terminated 7-bit ASCII string (values 0-127 only)
- **`AnsiZ`** - Zero-terminated Windows-1252 (ANSI) string

```csharp
// StringZ - UTF-16 zero-terminated
StringZ sz = "Hello\0World";  // Only "Hello" is stored
Console.WriteLine(sz.Length); // 5
Console.WriteLine(sz);        // "Hello"

// AsciiZ - 7-bit ASCII zero-terminated with 12.5% memory savings
var az = new AsciiZ("Hello\0World");
Console.WriteLine(az.Length); // 5
byte[] forPInvoke = az.ToNullTerminatedArray(); // For native interop

// AnsiZ - Windows-1252 zero-terminated
var anz = new AnsiZ("Héllo\0World"); // Supports extended characters (128-255)
Console.WriteLine(anz.Length); // 5
```

#### Variable-Length Strings

Full-content string types that preserve all bytes including embedded NUL characters.

- **`AsciiString`** - 7-bit ASCII string (values 0-127 only, 7-bit packed)
- **`AnsiString`** - Windows-1252 (ANSI) string (full 0-255 range)

```csharp
// AsciiString - preserves embedded nulls, 7-bit packed storage
var ascii = new AsciiString("Hello\0World");
Console.WriteLine(ascii.Length);  // 11 (embedded null preserved)
Console.WriteLine(ascii[5]);      // 0 (the null byte)

// AnsiString - Windows-1252 encoding
AnsiString ansi = "Café résumé";
Console.WriteLine(ansi.Length);   // 11
byte[] bytes = ansi.ToArray();    // Get raw bytes

// Implicit conversions
string s = ascii;                 // AsciiString → string
AnsiString a = "text";            // string → AnsiString
```

#### Fixed-Capacity Strings

Fixed-length string types with capacity specified at construction. Useful for structured data, binary protocols, and memory-mapped scenarios.

- **`FixedString`** - Fixed-capacity UTF-16 string
- **`FixedAscii`** - Fixed-capacity 7-bit ASCII string (7-bit packed)
- **`FixedAnsi`** - Fixed-capacity Windows-1252 string

```csharp
// FixedString - 32-char capacity, UTF-16
var name = new FixedString(32, "John Doe");
Console.WriteLine(name.Capacity); // 32
Console.WriteLine(name.Length);   // 8
var padded = name.PadRight();     // Pad to capacity with '\0'

// FixedAscii - 20-byte capacity, 7-bit packed (saves 12.5% memory)
var code = new FixedAscii(20, "ABC123");
Console.WriteLine(code.Capacity); // 20
Console.WriteLine(code.Length);   // 6
var leftPad = code.PadLeft((byte)' '); // Pad left with spaces

// FixedAnsi - 50-byte capacity, Windows-1252
var desc = new FixedAnsi(50, "Prodüct Déscription");
Console.WriteLine(desc.Capacity); // 50
var trimmed = desc.TrimEnd();     // Remove trailing nulls/whitespace
```

#### Invalid Character Handling

Control how non-ASCII characters are handled in ASCII types:

```csharp
// InvalidCharBehavior enum
public enum InvalidCharBehavior {
  Throw,   // Throw ArgumentException (default)
  Replace, // Replace with '?' (0x3F)
  Skip     // Skip invalid characters entirely
}

// Usage examples
var strict = new AsciiString("Héllo");                              // Throws - 'é' > 127
var replaced = new AsciiString("Héllo", InvalidCharBehavior.Replace); // "H?llo"
var skipped = new AsciiString("Héllo", InvalidCharBehavior.Skip);     // "Hllo"

// Works with all ASCII types
var fa = new FixedAscii(10, "Tëst", InvalidCharBehavior.Replace); // "T?st"
```

#### Type Conversions

Implicit conversions (safe, no precision loss):
- `AsciiZ` → `AsciiString`, `AnsiZ`, `AnsiString`
- `AsciiString` → `AnsiString`
- `FixedAscii` → `AsciiString`, `AnsiString`
- `AnsiZ` → `AnsiString`
- `FixedAnsi` → `AnsiString`
- All types → `string`

Explicit conversions (may truncate or throw):
- `AsciiString` → `AsciiZ` (truncates at first null)
- `FixedAscii` → `AsciiZ` (truncates at first null)
- `AnsiString` → `AsciiString` (throws if bytes > 127)
- `AnsiZ` → `AsciiZ`, `AsciiString` (throws if bytes > 127)
- `FixedAnsi` → `AsciiString` (throws if bytes > 127)
- `FixedString` → `StringZ` (truncates at first null)

```csharp
// Implicit - always safe
AsciiString ascii = new AsciiZ("Hello");
AnsiString ansi = ascii;  // ASCII is subset of ANSI
string str = ansi;

// Explicit - may truncate or throw
AsciiZ az = (AsciiZ)new AsciiString("Hello\0World"); // Truncates to "Hello"
AsciiString a = (AsciiString)new AnsiString("Test"); // OK if all bytes ≤ 127
// AsciiString a2 = (AsciiString)new AnsiString("Tëst"); // Throws!
```

#### Common API Surface

All string types implement:

**Properties:**
- `Length` - Number of characters
- `IsEmpty` - True if length is zero
- `Capacity` - Maximum characters (fixed types only)

**Indexers:**
- `this[int index]` - Character/byte at position
- `this[Index index]` - Character/byte using Index (^1 for last)
- `this[Range range]` - Substring using Range (1..4)

**Methods:**
- `Substring(start)`, `Substring(start, length)` - Extract substring
- `AsSpan()` - Get ReadOnlySpan without allocation
- `ToString()` - Convert to string
- `ToArray()` - Get byte array (byte-based types)
- `ToNullTerminatedArray()` - Get null-terminated array for P/Invoke
- `GetPinnableReference()` - For use with `fixed` statement

**Fixed types additionally:**
- `PadRight(char)`, `PadLeft(char)` - Pad to capacity
- `TrimEnd()` - Remove trailing nulls/whitespace

**Operators:**
- `==`, `!=`, `<`, `>`, `<=`, `>=` - Comparison
- `+` - Concatenation
- Implicit/explicit conversions as documented above

#### P/Invoke and Unsafe Usage

```csharp
// Get null-terminated array for native calls
var ascii = new AsciiZ("filename.txt");
byte[] nullTerminated = ascii.ToNullTerminatedArray();

// Use with fixed statement
fixed (byte* ptr = ascii) {
  // ptr points to packed data
  NativeMethod(ptr);
}

// Direct span access (no allocation)
ReadOnlySpan<byte> span = ascii.AsSpan();
```

---

### Numeric Types

Extended numeric types for machine learning, scientific computing, and scenarios requiring non-standard precision.

#### Overview

| Type | Size | Format | Exponent | Mantissa | Bias | Use Case |
|------|------|--------|----------|----------|------|----------|
| `BFloat8` | 8-bit | 1+5+2 | 5 bits | 2 bits | 15 | Truncated Half, ML inference |
| `BFloat16` | 16-bit | 1+8+7 | 8 bits | 7 bits | 127 | Upper 16 bits of float32, ML training |
| `BFloat32` | 32-bit | 1+11+20 | 11 bits | 20 bits | 1023 | Upper 32 bits of double |
| `BFloat64` | 64-bit | 1+15+48 | 15 bits | 48 bits | 16383 | Extended range (quad exponent) |
| `Quarter` | 8-bit | 1+5+2 | 5 bits | 2 bits | 15 | IEEE 754 minifloat |
| `E4M3` | 8-bit | 1+4+3 | 4 bits | 3 bits | 7 | ML format, no infinity |
| `E5M2` | 8-bit | 1+5+2 | 5 bits | 2 bits | 15 | ML format, IEEE 754 conventions |
| `Int96` | 96-bit | signed | - | - | - | Extended integer range |
| `UInt96` | 96-bit | unsigned | - | - | - | Extended unsigned integer range |

#### Brain Float Types

Brain Float (BFloat) types truncate the mantissa of standard IEEE 754 formats while preserving the full exponent range. This provides the same dynamic range with reduced precision, ideal for machine learning where the range matters more than precision.

- **`BFloat8`** - 8-bit brain float (1+5+2), same range as Half
- **`BFloat16`** - 16-bit brain float (1+8+7), same range as float
- **`BFloat32`** - 32-bit brain float (1+11+20), same range as double
- **`BFloat64`** - 64-bit brain float (1+15+48), quad-precision exponent range

```csharp
// BFloat16 - widely used in ML training (same range as float, half the bits)
BFloat16 weight = (BFloat16)0.5f;
float backToFloat = (float)weight;
Console.WriteLine(BFloat16.IsNaN(weight));      // false
Console.WriteLine(BFloat16.IsInfinity(weight)); // false

// BFloat8 - compact 8-bit format
BFloat8 compact = (BFloat8)1.5f;
Console.WriteLine(compact);  // ~1.5 (reduced precision)

// Special values
var inf = BFloat16.PositiveInfinity;
var nan = BFloat16.NaN;
var max = BFloat16.MaxValue;
var eps = BFloat16.Epsilon;  // Smallest positive subnormal
```

#### ML Floating-Point Formats

Specialized 8-bit formats optimized for machine learning workloads, balancing range and precision differently.

- **`E4M3`** - 8-bit ML format (1+4+3), more precision, no infinity representation
- **`E5M2`** - 8-bit ML format (1+5+2), more range, IEEE 754 infinity/NaN

```csharp
// E4M3 - 4 exponent bits, 3 mantissa bits (more precision, no infinity)
E4M3 e4 = (E4M3)1.25f;
Console.WriteLine(E4M3.IsFinite(e4));           // true (E4M3 has no infinity)
Console.WriteLine(E4M3.IsNaN(E4M3.MaxValue));   // false

// E5M2 - 5 exponent bits, 2 mantissa bits (more range, has infinity)
E5M2 e5 = (E5M2)100.0f;
Console.WriteLine(E5M2.IsInfinity(E5M2.PositiveInfinity)); // true
Console.WriteLine(E5M2.IsNaN(E5M2.NaN));                    // true

// Conversions between formats
float original = 3.14159f;
E4M3 e4val = (E4M3)original;
E5M2 e5val = (E5M2)original;
float fromE4 = (float)e4val;  // ~3.0 (3 mantissa bits)
float fromE5 = (float)e5val;  // ~3.0 (2 mantissa bits)
```

#### IEEE 754 Minifloat

- **`Quarter`** - 8-bit IEEE 754 minifloat (1+5+2), standard IEEE 754 semantics

```csharp
// Quarter - standard 8-bit IEEE 754 minifloat
Quarter q = (Quarter)1.0f;
Console.WriteLine(q == Quarter.One);  // true

// Full IEEE 754 semantics
Console.WriteLine(Quarter.IsNaN(Quarter.NaN));                    // true
Console.WriteLine(Quarter.IsInfinity(Quarter.PositiveInfinity)); // true
Console.WriteLine(Quarter.IsSubnormal(Quarter.Epsilon));          // true

// Arithmetic and comparisons
Quarter a = (Quarter)2.0f;
Quarter b = (Quarter)3.0f;
Console.WriteLine(a < b);  // true
```

#### Extended Integer Types

96-bit integer types for scenarios requiring values beyond the 64-bit range.

- **`Int96`** - 96-bit signed integer (range: -2^95 to 2^95-1)
- **`UInt96`** - 96-bit unsigned integer (range: 0 to 2^96-1)

```csharp
// Int96 - 96-bit signed integer
Int96 big = new Int96(0x12345678, 0xDEADBEEFCAFEBABE);
Console.WriteLine(Int96.IsNegative(big));   // false
Console.WriteLine(Int96.IsPositive(big));   // true
Console.WriteLine(Int96.IsPow2(Int96.One)); // true

// UInt96 - 96-bit unsigned integer
UInt96 huge = UInt96.MaxValue;
Console.WriteLine(huge);  // 79228162514264337593543950335

// Arithmetic operations
Int96 a = new Int96(0, 100);
Int96 b = new Int96(0, 50);
Int96 sum = a + b;
Int96 diff = a - b;
Int96 neg = -a;

// Comparison
Console.WriteLine(a > b);               // true
Console.WriteLine(a == new Int96(0, 100)); // true

// Bit operations
Int96 shifted = a << 10;
Int96 anded = a & b;
Int96 ored = a | b;

// Conversion
long smallValue = (long)new Int96(0, 42);  // 42
```

#### Common Numeric API Surface

All numeric types implement:

**Interfaces:**
- `IComparable`, `IComparable<T>` - Comparison support
- `IEquatable<T>` - Equality support
- `IFormattable` - String formatting support
- `IParsable<T>` - Parsing support

**Properties (floating-point types):**
- `RawValue` - Raw bit representation
- `Zero`, `One` - Common values
- `Epsilon` - Smallest positive subnormal
- `MaxValue`, `MinValue` - Finite bounds
- `PositiveInfinity`, `NegativeInfinity` - Infinity values (except E4M3)
- `NaN` - Not a Number value

**Static Methods (floating-point types):**
- `IsNaN(value)` - Check for NaN
- `IsInfinity(value)` - Check for infinity
- `IsPositiveInfinity(value)`, `IsNegativeInfinity(value)` - Specific infinity checks
- `IsFinite(value)` - Check if finite (not NaN or infinity)
- `IsSubnormal(value)` - Check for subnormal values
- `FromRaw(bits)` - Create from raw bits

**Properties (integer types):**
- `Upper`, `Lower` - Component access
- `Zero`, `One` - Common values
- `MaxValue`, `MinValue` - Bounds

**Static Methods (integer types):**
- `IsNegative(value)`, `IsPositive(value)` - Sign checks
- `IsEvenInteger(value)`, `IsOddInteger(value)` - Parity checks
- `IsPow2(value)` - Power of two check

**Operators:**
- `==`, `!=`, `<`, `>`, `<=`, `>=` - Comparison
- `+`, `-`, `*`, `/` - Arithmetic (integer types)
- `&`, `|`, `^`, `~` - Bitwise (integer types)
- `<<`, `>>` - Shift (integer types)
- Explicit/implicit conversions to/from standard types

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
