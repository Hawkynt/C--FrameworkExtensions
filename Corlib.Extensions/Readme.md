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

> **Completeness Note**: This README serves as a comprehensive reference document covering all extension method categories and new types in the library. For specific overload details and parameter variations, use IntelliSense in your IDE.

### Guard

Fluent argument-validation helpers that throw descriptive exceptions with caller information.

| Class         | Description                                                     |
|---------------|-----------------------------------------------------------------|
| `Against`     | Static guard methods for precondition checks                    |
| `AlwaysThrow` | Convenience methods that unconditionally throw typed exceptions |

**`Against` methods:**

| Method                                        | Description                                                 |
|-----------------------------------------------|-------------------------------------------------------------|
| `ThisIsNull`                                  | Throws if `this` reference is null                          |
| `ArgumentIsNull`                              | Throws `ArgumentNullException` if argument is null          |
| `ArgumentIsNullOrEmpty`                       | Throws if string/array/enumerable argument is null or empty |
| `ArgumentIsNullOrWhiteSpace`                  | Throws if string argument is null or whitespace             |
| `ArgumentIsNotOfType<T>`                      | Throws if argument is not of the specified type             |
| `ArgumentIsOfType<T>`                         | Throws if argument is of the specified type                 |
| `False` / `True`                              | Throws if the boolean condition is false/true               |
| `CountsIsNotEqual` / `LengthIsNot`            | Validates collection/array lengths                          |
| `IndexBelowZero` / `IndexOutOfRange`          | Validates index bounds                                      |
| `ValuesAreEqual` / `ValuesAreNotEqual`        | Validates value equality                                    |
| `ValueIsBelowOrEqualZero` / `ValueIsNegative` | Numeric range guards                                        |
| `DuplicateKeys`                               | Throws on duplicate keys in enumerables                     |

---

### Object Extensions (`object`)

General-purpose object manipulation and reflection utilities.

| Method                            | Description                                              |
|-----------------------------------|----------------------------------------------------------|
| `IsNull<T>` / `IsNotNull<T>`      | Null-check with `[NotNullWhen]` annotations              |
| `Is<TType>` / `As<TType>`         | Type-checking and safe-casting                           |
| `TypeIsAnyOf`                     | Checks if object's type matches any of the given types   |
| `IsAnyOf<T>`                      | Checks if value equals any of the given values           |
| `IsTrue` / `IsFalse`              | Evaluates a predicate against the object                 |
| `WhenNull` / `WhenNotNull`        | Executes action/function based on null-state             |
| `GetProperties` / `GetFields`     | Reflection-based property/field value extraction         |
| `ResetDefaultValues`              | Resets properties to their `[DefaultValue]` attributes   |
| `GetMemorySize`                   | Estimates memory footprint including nested objects      |
| `ToXmlFile<T>` / `FromXmlFile<T>` | XML serialization/deserialization to file                |
| `ToFile<T>` / `FromFile<T>`       | Binary serialization with optional compression           |
| `DeepClone<T>`                    | Deep-clones via binary serialization                     |
| `Apply<T>`                        | Fluent mutation -- applies action and returns the object |

---

### Boolean Extensions (`bool`)

| Method                                                          | Description                                                    |
|-----------------------------------------------------------------|----------------------------------------------------------------|
| `And` / `Or` / `Xor` / `Nand` / `Nor` / `Equ` / `Not`           | Functional-style boolean logic gate operations                 |
| `When` / `WhenTrue` / `WhenFalse`                               | Conditional execution (supports both Action and Func variants) |
| `ToYesOrNoString` / `ToOneOrZeroString` / `ToTrueOrFalseString` | String representations                                         |

---

### Char Extensions (`char`)

| Method                                                          | Description                                  |
|-----------------------------------------------------------------|----------------------------------------------|
| `IsWhiteSpace` / `IsNullOrWhiteSpace` / `IsNotNullOrWhiteSpace` | Whitespace checking including null character |
| `IsDigit` / `IsNotDigit`                                        | Check if character is a digit                |
| `IsUpper` / `IsNotUpper` / `IsLower` / `IsNotLower`             | Case checking                                |
| `IsLetter` / `IsNotLetter`                                      | Check if character is a letter               |
| `IsControl` / `IsNotControl` / `IsControlButNoWhiteSpace`       | Control character detection                  |
| `ToUpper` / `ToLower`                                           | Case conversion with optional CultureInfo    |
| `IsAnyOf`                                                       | Check if character is in a set               |
| `Repeat`                                                        | Create string by repeating character N times |

---

### Nullable Extensions (`T?`)

| Method                     | Description                                                   |
|----------------------------|---------------------------------------------------------------|
| `IsNull` / `IsNotNull`     | Check if nullable has value (with `[NotNullWhen]` attributes) |
| `WhenNull` / `WhenNotNull` | Conditional execution based on null state                     |

---

### Array Extensions (`TItem[]`)

One of the largest groups, providing LINQ-like operations optimized for arrays, plus byte-array utilities.

#### Core Operations

| Method                                    | Description                                                           |
|-------------------------------------------|-----------------------------------------------------------------------|
| `CompareTo<TItem>(other, comparer)`       | Produces change-sets (Added/Removed/Changed/Equal) between two arrays |
| `ToNullIfEmpty`                           | Returns null for empty arrays                                         |
| `SafelyClone<TItem>`                      | Null-safe `Clone()`                                                   |
| `Swap<TItem>(firstIndex, secondIndex)`    | High-performance element swapping                                     |
| `Shuffle<TItem>(entropySource)`           | Fisher-Yates shuffle implementation                                   |
| `QuickSort<TItem>` / `QuickSorted<TItem>` | In-place and copy quick-sort                                          |
| `Reverse<TItem>`                          | Returns reversed copy                                                 |
| `RotateTowardsZero<TItem>`                | Rotates array elements left by one position                           |
| `IsNullOrEmpty` / `IsNotNullOrEmpty`      | Null/empty checks with annotations                                    |
| `CreatedJaggedArray`                      | Creates multi-dimensional jagged arrays dynamically                   |
| `ToStringInstance`                        | Converts `char[]` to `string`                                         |

#### Slicing and Partitioning

| Method                                         | Description                                            |
|------------------------------------------------|--------------------------------------------------------|
| `Slice<TItem>(start, length)`                  | Create mutable `Span<T>` slices                        |
| `ReadOnlySlice<TItem>(start, length)`          | Create read-only `ReadOnlySpan<T>` slices              |
| `Slices<TItem>(size)`                          | Partition array into fixed-size `ArraySlice<T>` chunks |
| `ReadOnlySlices<TItem>(size)`                  | Partition array into `ReadOnlyArraySlice<T>` chunks    |
| `ProcessInChunks<TItem>(chunkSize, processor)` | Chunked processing                                     |
| `Range<TItem>(startIndex, count)`              | Extracts a sub-array by index and count                |

#### Element Access and Search

| Method                                                                   | Description                                       |
|--------------------------------------------------------------------------|---------------------------------------------------|
| `GetRandomElement<TItem>(random)`                                        | Random element selection                          |
| `GetValueOrDefault<TItem>(index, defaultValue)`                          | Safe indexed access with default/factory fallback |
| `First<TItem>` / `Last<TItem>` / `FirstOrDefault` / `LastOrDefault`      | LINQ-style element access                         |
| `TryGetFirst` / `TryGetLast` / `TryGetItem`                              | Safe try-pattern accessors                        |
| `TrySetFirst` / `TrySetLast` / `TrySetItem`                              | Safe try-pattern mutators                         |
| `IndexOf<TItem>(value, comparer)` / `IndexOfOrDefault`                   | Enhanced element searching                        |
| `Contains<TItem>(value)` / `Exists<TItem>(predicate)`                    | Membership testing                                |
| `Any<TItem>` / `IsSingle` / `IsMultiple` / `IsNoSingle` / `IsNoMultiple` | Length predicate checks                           |

#### Transformation and Aggregation

| Method                                                          | Description                                          |
|-----------------------------------------------------------------|------------------------------------------------------|
| `ConvertAll<TItem, TOutput>(converter)`                         | Array transformation (with optional index parameter) |
| `ForEach<TItem>(action)` / `ParallelForEach`                    | Element iteration with parallel support              |
| `Join<TItem>(separator, converter)`                             | String joining with custom converters                |
| `Select<TItem, TResult>` / `SelectLong` / `Where` / `WhereLong` | LINQ-style projection/filter                         |
| `OfType<T>` / `Cast<T>`                                         | Type-filtering and casting                           |
| `Aggregate<TItem>(func, seed)` / `Count` / `LongCount`          | Aggregation operations                               |

#### High-Performance Fill Operations

| Method        | Description                                                              |
|---------------|--------------------------------------------------------------------------|
| `Fill(value)` | Optimized fill for `byte[]`, `ushort[]`, `uint[]`, `ulong[]`, `IntPtr[]` |
| `Clear()`     | Optimized clear for all primitive array types                            |

#### Byte-Array Specific

| Method                                                                                              | Description                           |
|-----------------------------------------------------------------------------------------------------|---------------------------------------|
| `ToBin` / `ToHex`                                                                                   | Binary/hex string representations     |
| `RandomizeBuffer`                                                                                   | Fills with cryptographic random bytes |
| `Padd`                                                                                              | Pads to specified length              |
| `GZip` / `UnGZip`                                                                                   | GZip compression/decompression        |
| `IndexOfOrMinusOne` / `IndexOfOrDefault`                                                            | Byte-pattern search                   |
| `ComputeHash<T>` / `ComputeSHA512Hash` / `ComputeSHA256Hash` / `ComputeSHA1Hash` / `ComputeMD5Hash` | Hash computation                      |
| `Xor` / `And` / `Or` / `Nor` / `Nand` / `Not` / `Equ`                                               | Bitwise in-place operations           |

#### Fast Block Copy (T4-generated for all primitive types)

| Method   | Description                                                                                     |
|----------|-------------------------------------------------------------------------------------------------|
| `Copy`   | Creates a copy of a primitive array (or sub-range)                                              |
| `CopyTo` | Block-copies between primitive arrays of different element types using unsafe memory operations |

#### Supporting Types

| Type                                      | Description                                                    |
|-------------------------------------------|----------------------------------------------------------------|
| `ArraySlice<T>` / `ReadOnlyArraySlice<T>` | Lightweight mutable/read-only views over array segments        |
| `IChangeSet<T>` / `ChangeSet<T>`          | Describes additions, removals, changes, and unchanged elements |
| `Block32` / `Block64`                     | Fixed-size value-type blocks for fast memory operations        |

---

### Span Extensions (`Span<T>`, `ReadOnlySpan<T>`)

High-performance span operations with SIMD vectorization for bitwise operations.

#### Core Operations

- **`IsNotEmpty<T>()`** - Check if span is not empty

#### Clear and Fill Operations

Fast memory operations with SIMD acceleration (Vector512/256/128):

- **`Clear()`** - Set all bytes to zero (SIMD-accelerated)
- **`Fill(value)`** - Fill all bytes with a value (SIMD-accelerated)

**Typed Span Support:** Clear and Fill work on typed spans (`Span<sbyte>`, `Span<ushort>`, `Span<short>`, `Span<uint>`, `Span<int>`, `Span<ulong>`, `Span<long>`, `Span<bool>`) with automatic optimization when all bytes in the value are the same.

#### Bitwise Operations (Span-to-Span)

Binary operations between two spans with SIMD vectorization:

| Method          | Description                         |
|-----------------|-------------------------------------|
| `And(operand)`  | Bitwise AND (in-place)              |
| `Or(operand)`   | Bitwise OR (in-place)               |
| `Xor(operand)`  | Bitwise XOR (in-place)              |
| `Nand(operand)` | Bitwise NAND (in-place)             |
| `Nor(operand)`  | Bitwise NOR (in-place)              |
| `Equ(operand)`  | Bitwise equivalence/XNOR (in-place) |
| `Not()`         | Bitwise NOT/complement (in-place)   |

Each operation also has a `source.Op(operand, target)` variant that writes results to a separate target span.

#### Scalar Bitwise Operations

Operations between a span and a scalar value (`And`, `Or`, `Xor`, `Nand`, `Nor`, `Equ` with scalar byte).

```csharp
// Clear and Fill
Span<byte> data = stackalloc byte[256];
data.Fill(0xFF);            // Fill with ones
data.Clear();               // Clear to zeros

// Span-to-span operations
Span<byte> key = stackalloc byte[256];
data.Xor(key);              // XOR encryption/decryption
data.And(key);              // Mask bits

// Scalar operations
data.And(0x0F);             // Mask to lower nibble
data.Xor(0xFF);             // Flip all bits (same as Not)
data.Or(0x80);              // Set high bit on all bytes

// Typed spans
Span<int> ints = stackalloc int[100];
ints.Fill(42);              // Fill with value
ints.And(0x0F0F0F0F);       // Mask pattern

// Result in separate target
ReadOnlySpan<byte> source = GetData();
Span<byte> result = stackalloc byte[source.Length];
source.Xor(key, result);    // XOR without modifying source
```

---

### String Extensions (`string`)

The most feature-rich extension set with over 150 methods.

#### Case Conversion

Intelligent case transformations with word boundary detection.

| Method                                           | Description                                                 |
|--------------------------------------------------|-------------------------------------------------------------|
| `ToPascalCase` / `ToPascalCaseInvariant`         | Convert to PascalCase (e.g., "hello_world" -> "HelloWorld") |
| `ToCamelCase` / `ToCamelCaseInvariant`           | Convert to camelCase (e.g., "hello_world" -> "helloWorld")  |
| `ToSnakeCase` / `ToSnakeCaseInvariant`           | Convert to snake_case (e.g., "HelloWorld" -> "hello_world") |
| `ToUpperSnakeCase` / `ToUpperSnakeCaseInvariant` | Convert to UPPER_SNAKE_CASE                                 |
| `ToKebabCase` / `ToKebabCaseInvariant`           | Convert to kebab-case                                       |
| `ToUpperKebabCase` / `ToUpperKebabCaseInvariant` | Convert to UPPER-KEBAB-CASE                                 |
| `UpperFirst` / `UpperFirstInvariant`             | Capitalize first character only                             |
| `LowerFirst` / `LowerFirstInvariant`             | Lowercase first character only                              |

```csharp
"helloWorld".ToSnakeCase();       // "hello_world"
"XMLHttpRequest".ToKebabCase();   // "xml-http-request"
"hello_world".ToPascalCase();     // "HelloWorld"
```

#### String Manipulation

| Method                                                                  | Description                                      |
|-------------------------------------------------------------------------|--------------------------------------------------|
| `ExchangeAt(index, replacement)`                                        | Replace character(s) at a given position         |
| `ExchangeAt(index, count, replacement)`                                 | Replace substring range                          |
| `Repeat(count)`                                                         | Repeat string N times                            |
| `RemoveFirst(count)` / `RemoveLast(count)`                              | Remove N characters from start/end               |
| `RemoveAtStart(what)` / `RemoveAtEnd(what)`                             | Remove specific prefix/suffix                    |
| `ReplaceAtStart(what, replacement)` / `ReplaceAtEnd(what, replacement)` | Replace prefix/suffix                            |
| `ReplaceFirst(what, replacement)` / `ReplaceLast(what, replacement)`    | Replace first/last occurrence                    |
| `Replace(what, replacement, maxCount)`                                  | Replace up to N occurrences                      |
| `ReplaceRegex` / `ReplaceAnyOf`                                         | Regex and character-set replacements             |
| `MultipleReplace(dict)`                                                 | Apply multiple replacements in a single pass     |
| `SubString(start, end)`                                                 | Python-style substring with negative indexing    |
| `Left(count)` / `Right(count)`                                          | Get N characters from start/end safely           |
| `LeftUntil` / `RightUntil`                                              | Text before/after a pattern                      |
| `SanitizeForFileName`                                                   | Replace invalid filename characters              |
| `Truncate`                                                              | Truncate with ellipsis (KeepStart/KeepEnd modes) |
| `WordWrap`                                                              | Word-wrap text to a specified line width         |
| `RemoveDiacritics`                                                      | Remove accents and diacritical marks             |

#### StartsWith / EndsWith / Contains

| Method                                           | Description                                                 |
|--------------------------------------------------|-------------------------------------------------------------|
| `StartsWith` / `EndsWith`                        | Check prefix/suffix (with char, StringComparison overloads) |
| `StartsNotWith` / `EndsNotWith`                  | Negated prefix/suffix checks                                |
| `StartsWithAny` / `EndsWithAny`                  | Multi-value prefix/suffix checks                            |
| `StartsNotWithAny` / `EndsNotWithAny`            | Negated multi-value checks                                  |
| `Contains` / `ContainsNot`                       | Substring check with StringComparison support               |
| `ContainsAll` / `ContainsAny` / `ContainsNotAny` | Multi-value content checks                                  |
| `IsAnyOf` / `IsNotAnyOf`                         | Set membership                                              |
| `IsSurroundedWith`                               | Check wrapped by prefix and suffix                          |
| `OnlyCaseDiffersFrom`                            | Compare ignoring case only                                  |

#### Null and State Checking

| Method                                                                 | Description                |
|------------------------------------------------------------------------|----------------------------|
| `IsNull` / `IsNotNull`                                                 | Null checks                |
| `IsEmpty` / `IsNotEmpty`                                               | Empty string checks        |
| `IsNullOrEmpty` / `IsNotNullOrEmpty`                                   | Combined null or empty     |
| `IsNullOrWhiteSpace` / `IsNotNullOrWhiteSpace`                         | Null, empty, or whitespace |
| `IsWhiteSpace` / `IsNotWhiteSpace`                                     | Whitespace only check      |
| `DefaultIfNull` / `DefaultIfNullOrEmpty` / `DefaultIfNullOrWhiteSpace` | Fallback values            |

#### Lines

| Method                        | Description                                    |
|-------------------------------|------------------------------------------------|
| `DetectLineBreakMode`         | Auto-detects CR, LF, CRLF, etc.                |
| `EnumerateLines` / `Lines`    | Splits into lines (lazy enumerable or array)   |
| `LineCount` / `LongLineCount` | Counts lines                                   |
| `GetLineJoiner`               | Returns the line-break string for a given mode |

#### Formatting

| Method                        | Description                                                                  |
|-------------------------------|------------------------------------------------------------------------------|
| `FormatWith(parameters)`      | `string.Format` as extension method                                          |
| `FormatWithEx(fields)`        | Named-placeholder formatting with dictionaries, objects, or getter functions |
| `FormatWithObject<T>(object)` | Format using object property values as placeholders                          |

```csharp
"Hello {Name}, you are {Age}!".FormatWithEx(
  new KeyValuePair<string, object>("Name", "World"),
  new KeyValuePair<string, object>("Age", 42)
);
```

#### Regular Expressions

| Method                        | Description                         |
|-------------------------------|-------------------------------------|
| `IsMatch` / `IsNotMatch`      | Regex matching                      |
| `Matches` / `MatchGroups`     | Retrieve match collections/groups   |
| `AsRegularExpression`         | Convert string to `Regex` object    |
| `ConvertFilePatternToRegex`   | Convert file glob patterns to regex |
| `MatchesFilePattern` / `Like` | Glob/SQL-LIKE matching              |

#### Type-Safe Parsing (T4-generated)

For each type (`float`, `double`, `decimal`, `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`, `ulong`, `long`, `bool`, `char`, `DateTime`, `TimeSpan`, `Guid`, `BigInteger`, `Half`, `Color`):

| Method Pattern         | Description                                           |
|------------------------|-------------------------------------------------------|
| `Parse{Type}`          | Parse with optional format provider and number styles |
| `TryParse{Type}`       | Try-pattern parsing                                   |
| `Parse{Type}OrDefault` | Parse or return default/factory value                 |
| `Parse{Type}OrNull`    | Parse or return null (for nullable types)             |

#### Hashing and Encoding

| Method                                               | Description                                       |
|------------------------------------------------------|---------------------------------------------------|
| `ComputeHash<TAlgorithm>` / `ComputeHash(algorithm)` | Hash computation using any `HashAlgorithm`        |
| `GetSoundexRepresentation`                           | Soundex phonetic code (culture-aware)             |
| `ToQuotedPrintable` / `FromQuotedPrintable`          | Quoted-printable encoding/decoding                |
| `ToLinq2SqlConnectionString`                         | Convert ADO.NET connection string for LINQ-to-SQL |
| `MsSqlDataEscape` / `MsSqlIdentifierEscape`          | SQL escaping utilities                            |
| `ParseHostAndPort`                                   | Parse `host:port` strings                         |

#### Text Analysis

| Method                             | Description                                                                     |
|------------------------------------|---------------------------------------------------------------------------------|
| `TextAnalysis` / `TextAnalysisFor` | Returns a `TextAnalyzer` with word/sentence/syllable counts, readability scores |

The `TextAnalyzer` provides comprehensive NLP-style analysis of any string:

- **`Words`** - Array of all word tokens (letters, digits, apostrophes)
- **`DistinctWords`** - Unique words (case-insensitive per culture)
- **`WordHistogram`** - Dictionary mapping each word to its occurrence count
- **`Sentences`** - Intelligent sentence splitting that handles abbreviations (e.g., "e.g.", "Dr.", "z.B.")
- **`TotalSyllables`** - Culture-aware syllable counting (supports English, German, French, Spanish, Italian, Portuguese)
- **`ReadabilityScore`** - Access to a `ReadabilityScoreCalculator` with these metrics:
  - **[`Smog`](https://en.wikipedia.org/wiki/SMOG)** - Simple Measure of Gobbledygook (G. Harry McLaughlin, 1969) - years of education needed
  - **[`FleschReadingEase`](https://en.wikipedia.org/wiki/Flesch%E2%80%93Kincaid_readability_tests#Flesch_reading_ease)** - 0-100 scale, higher = easier ([Rudolf Flesch](https://en.wikipedia.org/wiki/Rudolf_Flesch), 1948; culture-adjusted formula for German by [Toni Amstad](https://de.wikipedia.org/wiki/Lesbarkeitsindex#Flesch-Reading-Ease), 1978)
  - **[`FleschKincaid`](https://en.wikipedia.org/wiki/Flesch%E2%80%93Kincaid_readability_tests#Flesch%E2%80%93Kincaid_grade_level)** - Flesch-Kincaid Grade Level (J. Peter Kincaid et al., 1975) - U.S. grade level required
  - **[`GunningFog`](https://en.wikipedia.org/wiki/Gunning_fog_index)** - Gunning Fog Index (Robert Gunning, 1952) - years of formal education
  - **[`Ari`](https://en.wikipedia.org/wiki/Automated_readability_index)** - Automated Readability Index (Senter & Smith, 1967) - U.S. grade level, character-count based
  - **[`ColemanLiau`](https://en.wikipedia.org/wiki/Coleman%E2%80%93Liau_index)** - Coleman-Liau Index (Meri Coleman & T. L. Liau, 1975) - character-based, no syllable counting needed
  - **[`Lix`](https://en.wikipedia.org/wiki/Lix_(readability_test))** - Lasbarhetsindex (Carl-Hugo Bjornsson, 1968) - Scandinavian readability metric
  - **[`Wstf`](https://de.wikipedia.org/wiki/Wiener_Sachtextformel)** - Wiener Sachtextformel (Richard Bamberger & Erich Vanecek, 1984) - German factual-text readability

```csharp
var analyzer = "The quick brown fox jumps over the lazy dog. It was a sunny day.".TextAnalysis();

Console.WriteLine($"Words: {analyzer.Words.Length}");           // 14
Console.WriteLine($"Sentences: {analyzer.Sentences.Length}");   // 2
Console.WriteLine($"Unique words: {analyzer.DistinctWords.Count()}");
Console.WriteLine($"Syllables: {analyzer.TotalSyllables}");

// Word frequency
foreach (var (word, count) in analyzer.WordHistogram.OrderByDescending(kv => kv.Value))
  Console.WriteLine($"  {word}: {count}x");

// Readability scores
var scores = analyzer.ReadabilityScore;
Console.WriteLine($"Flesch Reading Ease: {scores.FleschReadingEase:F1}");  // higher = easier
Console.WriteLine($"Flesch-Kincaid Grade: {scores.FleschKincaid:F1}");
Console.WriteLine($"Gunning Fog: {scores.GunningFog:F1}");
Console.WriteLine($"SMOG: {scores.Smog:F1}");

// German text with culture-specific rules
var de = "Dies ist ein einfacher Beispielsatz. Er enthält kurze Wörter.".TextAnalysisFor(
  CultureInfo.GetCultureInfo("de-DE")
);
Console.WriteLine($"WSTF: {de.ReadabilityScore.Wstf:F1}");  // German-specific metric
```

#### Splitting

| Method                        | Description                                           |
|-------------------------------|-------------------------------------------------------|
| `Split(int)` / `Split(Regex)` | Split into fixed-length chunks or by regex            |
| `QuotedSplit`                 | Splits respecting quoted regions and escape sequences |

#### Character Access

| Method                                                | Description                                |
|-------------------------------------------------------|--------------------------------------------|
| `First` / `Last` / `FirstOrDefault` / `LastOrDefault` | Character accessors                        |
| `CopyTo(Span<char>)`                                  | Copy to span for zero-allocation scenarios |

---

### Enum Extensions (`Enum`)

| Method                                               | Description                                           |
|------------------------------------------------------|-------------------------------------------------------|
| `GetFieldDescription<T>` / `GetFieldDisplayName<T>`  | Retrieve `[Description]` / `[DisplayName]` attributes |
| `GetFieldAttribute<T,TAttr>`                         | Retrieve any attribute from enum value                |
| `ToString<T,TAttr>` / `ToStringOrDefault<T,TAttr>`   | Convert enum to string using attribute values         |
| `ParseEnum<T,TAttr>` / `ParseEnumOrDefault<T,TAttr>` | Parse strings to enum values via attributes           |
| `HasFlag` / `SetFlag` / `ClearFlag` / `ToggleFlag`   | Flag enum manipulation                                |
| `GetValues` / `GetNames` / `GetFlags`                | Retrieve all enum values/names/flags                  |

---

### Random Extensions (`Random`)

| Method                                                                                                  | Description                                        |
|---------------------------------------------------------------------------------------------------------|----------------------------------------------------|
| `GeneratePassword`                                                                                      | Generate secure passwords with customizable rules  |
| `GetBoolean` / `RollADice`                                                                              | Random boolean and dice roll                       |
| `GetValueFor<T>`                                                                                        | Generate random value for any supported type       |
| `GetInt8` / `GetInt16` / `GetInt32` / `GetInt64` / `GetUInt8` / `GetUInt16` / `GetUInt32` / `GetUInt64` | Full-range random integers                         |
| `GetFloat` / `GetDouble` / `GetDecimal`                                                                 | Random floating-point (with NaN, Infinity control) |
| `GetChar` / `GetString`                                                                                 | Random character/string with filters               |
| `NextDouble(min, max)`                                                                                  | Random double within range                         |
| `NextGaussian`                                                                                          | Gaussian (normal) distribution random              |
| `Shuffle<T>`                                                                                            | Shuffles a list randomly                           |

---

### Console Extensions (`Console`)

| Method                              | Description                                  |
|-------------------------------------|----------------------------------------------|
| `WriteLineColored` / `WriteColored` | Write text with foreground/background colors |
| `WriteLineFormatted`                | Write formatted text with color codes        |
| `ReadLineSecure`                    | Read input without echoing (for passwords)   |
| `WriteProgress`                     | Display progress bars and indicators         |

---

### Convert Extensions

| Method                                                  | Description                                          |
|---------------------------------------------------------|------------------------------------------------------|
| `ToBase91String` / `FromBase91String`                   | Efficient base91 encoding (more compact than base64) |
| `ToQuotedPrintableString` / `FromQuotedPrintableString` | Quoted-printable encoding                            |
| `ChangeType<T>`                                         | Generic `Convert.ChangeType` wrapper                 |

---

### Uri Extensions (`Uri`)

| Method                                           | Description                       |
|--------------------------------------------------|-----------------------------------|
| `ReadAllText` / `ReadAllBytes`                   | Download content from URIs        |
| `ReadAllTextTaskAsync` / `ReadAllBytesTaskAsync` | Async download methods            |
| `DownloadToFile`                                 | Download content directly to file |
| `BaseUri` / `Path`                               | URI manipulation                  |
| `GetResponseUri`                                 | Get final URI after redirects     |

---

### DateTime Extensions (`DateTime`)

| Method                                                                                                                    | Description                                    |
|---------------------------------------------------------------------------------------------------------------------------|------------------------------------------------|
| `StartOfDay` / `EndOfDay`                                                                                                 | Get start/end of current day                   |
| `AddWeeks` / `DateOfDayOfCurrentWeek` / `StartOfWeek` / `DayInCurrentWeek`                                                | Week-based calculations                        |
| `FirstDayOfMonth` / `LastDayOfMonth`                                                                                      | Get first/last day of month                    |
| `FirstDayOfYear` / `LastDayOfYear`                                                                                        | Get first/last day of year                     |
| `Max` / `Min`                                                                                                             | Compare and return min/max dates               |
| `DaysTill`                                                                                                                | Enumerate days between dates                   |
| `Sequence(start, end, step)` / `InfiniteSequence(start, step)`                                                            | Generate finite or infinite DateTime sequences |
| `SubstractTicks` / `SubstractMilliseconds` / `SubstractSeconds` / `SubstractMinutes` / `SubstractHours` / `SubstractDays` | Subtraction alternatives                       |
| `ToUnixTimestamp` / `FromUnixTimestamp`                                                                                   | Unix epoch conversions                         |
| `DaysInYear` / `DaysLeftInYear`                                                                                           | Day calculations                               |
| `IsLeapYear`                                                                                                              | Leap year check                                |
| `Age`                                                                                                                     | Calculate age as `TimeSpan`                    |
| `ToRfc2822`                                                                                                               | RFC 2822 formatted string                      |
| `IsWeekend` / `IsWeekday`                                                                                                 | Day-of-week checks                             |

---

### TimeSpan Extensions (`TimeSpan`)

| Method                                 | Description                        |
|----------------------------------------|------------------------------------|
| `Multiply` / `Divide`                  | Scalar multiplication and division |
| `IsPositive` / `IsNegative` / `IsZero` | Duration state checks              |
| `ToHumanReadable`                      | Convert to friendly format         |
| `TotalWeeks`                           | Get total weeks as double          |

**T4-generated conversions:** For all numeric types (`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`):

`FromTicks`, `FromMilliseconds`, `FromSeconds`, `FromMinutes`, `FromHours`, `FromDays`

---

### Type Extensions (`Type`)

| Method                                                    | Description                                          |
|-----------------------------------------------------------|------------------------------------------------------|
| `IsNumericType` / `IsIntegerType` / `IsFloatingPointType` | Type classification                                  |
| `GetDefault`                                              | Get `default(T)` at runtime                          |
| `IsNullable` / `GetNullableUnderlyingType`                | Nullable type inspection                             |
| `IsAssignableTo` / `Implements` / `InheritsFrom`          | Type hierarchy checks                                |
| `IsCastableTo`                                            | Check if type can be cast to another                 |
| `GetBaseTypes` / `GetInterfaces`                          | Enumerate type hierarchy                             |
| `HasAttribute<T>` / `GetAttribute<T>`                     | Attribute presence and retrieval                     |
| `IsDelegate` / `IsEnum` / `IsGenericType`                 | Common type category checks                          |
| `GetGenericArguments`                                     | Generic type inspection                              |
| `CreateInstance`                                          | Factory creation with constructor-parameter matching |
| `GetAllSubclasses` / `GetAllImplementors`                 | Discover derived types                               |
| `GetPublicProperties` / `GetPublicMethods`                | Cached reflection helpers                            |

---

### BitConverter Extensions

| Method                                      | Description                                             |
|---------------------------------------------|---------------------------------------------------------|
| `ToSByte` / `ToByte` / `ToShort` / `ToChar` | Convert byte arrays to primitives                       |
| `ToNSByte` / `ToNByte` / `ToNChar`          | Convert to nullable primitives with null marker         |
| `GetBytes`                                  | Convert primitives (including nullables) to byte arrays |

---

### Math and Numeric Extensions

#### Bit Manipulation Extensions

| Method                                          | Description                                         |
|-------------------------------------------------|-----------------------------------------------------|
| `LowerHalf` / `UpperHalf`                       | Extract lower/upper bit portions                    |
| `RotateLeft(count)` / `RotateRight(count)`      | Bitwise rotation for all integer types              |
| `TrailingZeroCount` / `LeadingZeroCount`        | Count trailing/leading zeros with SIMD optimization |
| `TrailingOneCount` / `LeadingOneCount`          | Count trailing/leading ones                         |
| `CountSetBits` / `CountUnsetBits`               | Population count (Brian Kernighan's algorithm)      |
| `Parity`                                        | Check if number of set bits is even/odd             |
| `ReverseBits`                                   | Reverse bit order using lookup tables               |
| `ParallelBitExtract(mask)`                      | Extract bits based on bitmask                       |
| `DeinterleaveBits` / `PairwiseDeinterleaveBits` | Bit deinterleaving operations                       |
| `FlipBit` / `GetBit` / `SetBit` / `ClearBit`    | Individual bit manipulation                         |
| `IsPowerOfTwo`                                  | Fast power-of-2 testing                             |
| `And` / `Or` / `Xor` / `Not` / `Nand` / `Nor`   | Bitwise logical operations                          |

#### Arithmetic Operations (T4-generated for all numeric types)

| Method                                              | Description                                   |
|-----------------------------------------------------|-----------------------------------------------|
| `Add` / `Subtract` / `MultipliedWith` / `DividedBy` | Functional-style arithmetic                   |
| `Squared` / `Cubed`                                 | Common power operations                       |
| `Average(other)`                                    | Precise average calculation avoiding overflow |
| `FusedMultiplyAdd` / `FusedMultiplySubtract`        | Hardware-accelerated fused operations         |
| `Clamp(min, max)`                                   | Clamp to range                                |
| `IsBetween` / `IsNotBetween`                        | Range checks                                  |
| `Abs` / `Sign`                                      | Absolute value and sign                       |
| `Min` / `Max`                                       | Two-value min/max                             |

#### Advanced Mathematical Functions

| Method                                                    | Description                   |
|-----------------------------------------------------------|-------------------------------|
| `Pow(exponent)` / `Sqrt` / `Cbrt`                         | Power, square root, cube root |
| `Floor` / `Ceiling` / `Truncate`                          | Rounding operations           |
| `Round(decimals, midpointRounding)`                       | Advanced rounding             |
| `LogN(base)` / `Log` / `Log10` / `Log2`                   | Logarithmic functions         |
| `Exp`                                                     | Exponential function          |
| `Sin` / `Cos` / `Tan` / `Cot` / `Csc` / `Sec`             | Trigonometric functions       |
| `Sinh` / `Cosh` / `Tanh` / `Coth` / `Csch` / `Sech`       | Hyperbolic functions          |
| `Asin` / `Acos` / `Atan`                                  | Inverse trigonometric         |
| `Arsinh` / `Arcosh` / `Artanh` / `Acot` / `Asec` / `Acsc` | Inverse hyperbolic            |

#### Shift Operations (Integer Types)

| Method                                         | Description                |
|------------------------------------------------|----------------------------|
| `ArithmeticShiftLeft` / `ArithmeticShiftRight` | Sign-preserving bit shifts |
| `LogicalShiftLeft` / `LogicalShiftRight`       | Zero-fill bit shifts       |

#### Saturating Arithmetic

Operations for all integer types that clamp results to type boundaries instead of overflowing:

| Method                      | Description                                             |
|-----------------------------|---------------------------------------------------------|
| `SaturatingAdd(value)`      | Add with saturation (overflow clamps to MaxValue)       |
| `SaturatingSubtract(value)` | Subtract with saturation (underflow clamps to MinValue) |
| `SaturatingMultiply(value)` | Multiply with saturation                                |
| `SaturatingDivide(value)`   | Divide with saturation                                  |
| `SaturatingNegate`          | Negate with saturation (signed only)                    |

#### Comparison and Range Operations

| Method                                                                | Description                                       |
|-----------------------------------------------------------------------|---------------------------------------------------|
| `IsZero` / `IsNotZero`                                                | Zero comparison (with epsilon for floating-point) |
| `IsPositive` / `IsNegative` / `IsPositiveOrZero` / `IsNegativeOrZero` | Sign checking                                     |
| `IsEven` / `IsOdd`                                                    | Parity checking                                   |
| `IsAbove` / `IsBelow` / `IsAboveOrEqual` / `IsBelowOrEqual`           | Relational comparisons                            |
| `IsBetween` / `IsInRange`                                             | Range validation                                  |
| `IsIn` / `IsNotIn`                                                    | Set membership testing                            |
| `IsNaN` / `IsInfinity` / `IsPositiveInfinity` / `IsNegativeInfinity`  | IEEE 754 checks                                   |
| `IsNumeric` / `IsNonNumeric`                                          | Validity checking                                 |
| `ReciprocalEstimate`                                                  | Fast reciprocal approximation                     |

#### Repetition Extensions (for integer types)

| Method                          | Description                                 |
|---------------------------------|---------------------------------------------|
| `Times(Action)`                 | Execute action N times                      |
| `Times(Action<T>)`              | Execute action N times with index parameter |
| `Times(string)` / `Times(char)` | Repeat string/char N times                  |

#### Unsigned Type-Safe Wrappers

`UnsignedFloat`, `UnsignedDouble`, `UnsignedDecimal` - compile-time negative value prevention with full arithmetic and interface support.

---

### Collection Extensions

#### ICollection Extensions

| Method             | Description                      |
|--------------------|----------------------------------|
| `Any`              | Check if collection has elements |
| `ForEach`          | Iterate with action              |
| `ConvertAll<TOut>` | Convert all elements             |
| `ToArray`          | Convert to `object[]`            |

#### BitArray Extensions

| Method         | Description                     |
|----------------|---------------------------------|
| `GetSetBits`   | Enumerate indices of set bits   |
| `GetUnsetBits` | Enumerate indices of unset bits |

#### IEnumerable\<T\> Extensions

| Method                                      | Description                                  |
|---------------------------------------------|----------------------------------------------|
| `ForEach` / `ParallelForEach`               | Iteration with action                        |
| `WhereNot` / `WhereNotNull`                 | Inverted/null-filtering Where                |
| `Prepend` / `Append`                        | Add elements at start/end                    |
| `ToBiDictionary`                            | Create a `BiDictionary`                      |
| `ToHashSet`                                 | Convert to `HashSet<T>`                      |
| `Batch` / `Chunk`                           | Group into fixed-size batches                |
| `Interleave` / `ZipAll`                     | Merge multiple sequences                     |
| `Shuffle`                                   | Random shuffling                             |
| `DistinctBy`                                | Distinct by key selector                     |
| `MinBy` / `MaxBy`                           | Minimum/maximum by key                       |
| `Flatten`                                   | Flatten nested enumerables                   |
| `IndexOf` / `FindIndex`                     | Find index of element/predicate              |
| `StartsWith` / `EndsWith` / `SequenceEqual` | Sequence comparisons                         |
| `IsNullOrEmpty` / `IsNotNullOrEmpty`        | Null/empty checks                            |
| `ToDelimitedString`                         | Join with delimiter                          |
| `Scan`                                      | Running aggregation (like Haskell's `scanl`) |
| `Window` / `Pairwise`                       | Sliding window operations                    |
| `TakeWhileIncluding` / `SkipWhileIncluding` | Inclusive take/skip                          |
| `OrderByTopological`                        | Topological sort                             |
| `ExceptBy` / `IntersectBy` / `UnionBy`      | Set operations with key selector             |
| `AsCachedEnumeration`                       | Cache lazily-evaluated sequences             |

#### Dictionary Extensions (`IDictionary<TKey, TValue>`)

| Method                                      | Description                                   |
|---------------------------------------------|-----------------------------------------------|
| `GetValueOrDefault` / `GetValueOrNull`      | Safe lookups with default fallback            |
| `GetOrAdd`                                  | Get existing or add new value                 |
| `AddOrUpdate`                               | Add or update a value                         |
| `AddRange`                                  | Bulk addition                                 |
| `TryAdd` / `TryRemove` / `TryUpdate`        | Safe modifications                            |
| `IncrementOrAdd` (T4 for all numeric types) | Atomically increment counter or initialize    |
| `CompareTo`                                 | Produce `IChangeSet` between two dictionaries |
| `RemoveWhere`                               | Remove entries matching predicate             |
| `Merge`                                     | Merge another dictionary in                   |
| `ToReadOnly`                                | Wrap as `IReadOnlyDictionary`                 |

#### List\<T\> Extensions

| Method                                      | Description                             |
|---------------------------------------------|-----------------------------------------|
| `AddRange` / `RemoveRange`                  | Batch add/remove                        |
| `Shuffle`                                   | Random shuffling                        |
| `BinarySearchIndex`                         | Binary search returning insertion index |
| `Swap` / `Permutate`                        | Element manipulation                    |
| `TrySetFirst` / `TrySetLast` / `TrySetItem` | Safe try-pattern mutators               |
| `RemoveEvery`                               | Remove every Nth element                |
| `AsIReadOnlyList`                           | Wrap as `IReadOnlyList<T>`              |

#### HashSet\<T\> Extensions

| Method                      | Description            |
|-----------------------------|------------------------|
| `AddRange`                  | Add multiple items     |
| `TryAdd` / `TryRemove`      | Try-pattern operations |
| `CompareTo` / `ContainsNot` | Comparison and checks  |

#### Queue\<T\> / Stack\<T\> Extensions

| Method                                    | Description             |
|-------------------------------------------|-------------------------|
| `DequeueOrDefault` / `PopOrDefault`       | Return default if empty |
| `TryDequeue` / `TryPop`                   | Try-pattern operations  |
| `EnqueueRange` / `PushRange` / `AddRange` | Batch operations        |
| `PullTo` / `PullAll` / `Pull`             | Transfer operations     |
| `Exchange` / `Invert` (Stack)             | Stack manipulation      |

#### LinkedList\<T\> Extensions

| Method    | Description         |
|-----------|---------------------|
| `ForEach` | Iterate linked list |
| `ToArray` | Convert to array    |

#### KeyValuePair Extensions

| Method    | Description        |
|-----------|--------------------|
| `Reverse` | Swap key and value |

#### Concurrent Collections

**ConcurrentDictionary Extensions:**

| Method                          | Description                                                     |
|---------------------------------|-----------------------------------------------------------------|
| `AddOrUpdate(key, value)`       | Simplified upsert (no factory functions needed)                 |
| `Add(value, keyFunction)`       | Add with auto-generated key (retries until unique key found)    |
| `Add(value, IEnumerator<TKey>)` | Add using next available key from an enumerator                 |
| `Add(value, IEnumerable<TKey>)` | Add using first available key from a sequence                   |
| `TryGetKey(value, out key)`     | Reverse lookup - find a key by its value                        |
| `Remove(key)`                   | Simplified removal (wraps `TryRemove`)                          |
| `GetOrAdd(key)`                 | For `ConcurrentDictionary<T,T>` - use key as both key and value |

**ConcurrentQueue\<T\> Extensions:**

| Method                                                                  | Description                                                                           |
|-------------------------------------------------------------------------|---------------------------------------------------------------------------------------|
| `PullTo(Span<T>)`                                                       | Dequeue elements into a span; returns the filled portion                              |
| `PullTo(T[])` / `PullTo(T[], offset)` / `PullTo(T[], offset, maxCount)` | Dequeue into array with optional offset and count limit                               |
| `PullAll()`                                                             | Dequeue all elements into a new array                                                 |
| `Pull(maxCount)`                                                        | Dequeue up to N elements into a new array (uses `ArrayPool` chunking for large pulls) |

**ConcurrentStack\<T\> Extensions:**

| Method                                                                  | Description                                                       |
|-------------------------------------------------------------------------|-------------------------------------------------------------------|
| `Pop()`                                                                 | Blocking pop - spins until an item is available                   |
| `PushRange(IEnumerable<T>)`                                             | Push all items from a sequence onto the stack                     |
| `PullTo(Span<T>)`                                                       | Pop elements into a span; returns the filled portion              |
| `PullTo(T[])` / `PullTo(T[], offset)` / `PullTo(T[], offset, maxCount)` | Pop into array with optional offset and count limit               |
| `PullAll()`                                                             | Pop all elements into a new array (LIFO order)                    |
| `Pull(maxCount)`                                                        | Pop up to N elements into a new array (uses `ArrayPool` chunking) |

#### Specialized Collections

**StringDictionary Extensions:**

| Method                    | Description                    |
|---------------------------|--------------------------------|
| `AddOrUpdate(key, value)` | Add or update a key-value pair |

**StringCollection Extensions:**

| Method      | Description                              |
|-------------|------------------------------------------|
| `ToArray()` | Copy collection contents to a `string[]` |

**ObjectModel Collection\<T\> Extensions:**

| Method                     | Description                             |
|----------------------------|-----------------------------------------|
| `AddRange(IEnumerable<T>)` | Add multiple items to a `Collection<T>` |

---

### Custom Collection Types

| Type                                       | Description                                                               |
|--------------------------------------------|---------------------------------------------------------------------------|
| `BiDictionary<TFirst, TSecond>`            | Bidirectional dictionary with O(1) reverse lookup via `.Reverse` property |
| `DoubleDictionary<TOuter, TInner, TValue>` | Two-level nested dictionary                                               |
| `FastLookupTable<TItem>`                   | High-performance lookup table with optimized hashing                      |
| `OrderedDictionary<TKey, TValue>`          | Dictionary maintaining insertion order                                    |
| `CachedEnumeration<TItem>`                 | Lazily caches an `IEnumerable<T>` on first enumeration                    |
| `ConcurrentWorkingBag<T>`                  | Thread-safe bag with atomic AddOrReplace, AddOrExecute, TryRemove         |
| `ExecutiveQueue<T>`                        | Thread-safe queue that auto-executes a callback on enqueue                |

```csharp
// BiDictionary example
var bi = new BiDictionary<string, int>();
bi.Add("one", 1);
int val = bi["one"];        // 1
string key = bi.Reverse[1]; // "one"
```

---

### File System Extensions

#### FileInfo Extensions

| Method                                                                                                                                 | Description                                                                             |
|----------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------|
| `EnableCompression` / `TryEnableCompression`                                                                                           | NTFS compression                                                                        |
| `GetTypeDescription`                                                                                                                   | File type identification                                                                |
| `RenameTo` / `ChangeExtension`                                                                                                         | Safe file renaming and extension modification                                           |
| `MoveTo(destination, timeout, overwrite)`                                                                                              | Enhanced move with options                                                              |
| `CopyToAsync(target, cancellationToken)`                                                                                               | Async file copying with progress                                                        |
| `ComputeHash<THashAlgorithm>` / `ComputeSHA512Hash` / `ComputeSHA384Hash` / `ComputeSHA256Hash` / `ComputeSHA1Hash` / `ComputeMD5Hash` | Hash computation                                                                        |
| `ReadAllLinesOrDefault` / `ReadAllBytesOrDefault` (T4-generated)                                                                       | Read file or return default on failure                                                  |
| `IsSymbolicLink` / `IsHardLink`                                                                                                        | Link type detection                                                                     |
| `GetSymbolicLinkTarget` / `GetHardLinkTarget` / `GetHardLinkTargets`                                                                   | Read link targets                                                                       |
| `CreateSymbolicLinkFrom/At` / `CreateHardLinkFrom/At`                                                                                  | Create symbolic/hard links                                                              |
| `TryCreateSymbolicLinkFrom/At` / `TryCreateHardLinkFrom/At`                                                                            | Try-pattern link creation                                                               |
| `CopyTo` (enhanced)                                                                                                                    | Copy with optional hard-linking, symbolic link resolution, progress                     |
| `StartWorkInProgress`                                                                                                                  | Returns an `IFileInProgress` token for atomic file modification with conflict detection |

**Work-In-Progress File Modification (`IFileInProgress`):**

Modifying files in place is risky -- a crash or power loss mid-write can corrupt the file. The `StartWorkInProgress` pattern creates a temporary copy, lets you make changes through a rich API (read/write text, lines, bytes, append, truncate), and atomically replaces the original only when the token is disposed. If anything goes wrong, set `CancelChanges = true` and the original remains untouched. Conflict detection modes (timestamp checks, SHA-256 checksums, exclusive locking) protect against concurrent modifications.

```csharp
var file = new FileInfo("config.json");

// Safely modify a file with automatic rollback on failure
using (var wip = file.StartWorkInProgress(copyContents: true)) {
  var content = wip.ReadAllText();
  content = content.Replace("\"debug\": false", "\"debug\": true");
  wip.WriteAllText(content);
  // On dispose: temp file atomically replaces the original
}

// With conflict detection -- throws if the file was modified externally during editing
using (var wip = file.StartWorkInProgress(
  copyContents: true,
  conflictMode: ConflictResolutionMode.CheckChecksumAndThrow)) {
  wip.WriteAllText("{\"setting\": \"new value\"}");
  // If another process changed the file since we started, a FileConflictException is thrown
}

// Canceling changes -- original file remains untouched
using (var wip = file.StartWorkInProgress(copyContents: true)) {
  wip.WriteAllText("something destructive");
  wip.CancelChanges = true; // Original file is preserved
}
```

**`IFileInProgress` API Reference:**

| Member | Description |
|--------|-------------|
| **Properties** | |
| `OriginalFile` | The original `FileInfo` being modified |
| `CancelChanges` | Set to `true` to discard all changes on dispose; the original file remains untouched |
| `ConflictMode` | The active `ConflictResolutionMode` governing how concurrent modifications are detected |
| **Reading** | |
| `ReadAllText()` / `ReadAllText(Encoding)` | Read the entire working copy as a string |
| `ReadLines()` / `ReadLines(Encoding)` | Lazily enumerate lines from the working copy |
| `ReadAllBytes()` | Read all bytes from the working copy |
| `ReadBytes()` | Read bytes as a memory-mapped span |
| `GetEncoding()` | Detect the file's text encoding (BOM sniffing) |
| **Writing** | |
| `WriteAllText(string)` / `WriteAllText(string, Encoding)` | Overwrite the working copy with text |
| `WriteAllLines(IEnumerable<string>)` / `WriteAllLines(IEnumerable<string>, Encoding)` | Overwrite with line-by-line content |
| `WriteAllBytes(byte[])` | Overwrite with raw bytes |
| **Appending** | |
| `AppendLine(string)` / `AppendLine(string, Encoding)` | Append a single line |
| `AppendAllLines(IEnumerable<string>)` / `AppendAllLines(IEnumerable<string>, Encoding)` | Append multiple lines |
| `AppendAllText(string)` / `AppendAllText(string, Encoding)` | Append raw text |
| **Truncation** | |
| `KeepFirstLines(int)` / `KeepFirstLines(int, Encoding)` | Keep only the first *n* lines, discard the rest |
| `KeepLastLines(int)` / `KeepLastLines(int, Encoding)` | Keep only the last *n* lines, discard the rest |
| `RemoveFirstLines(int)` / `RemoveFirstLines(int, Encoding)` | Remove the first *n* lines, keep the rest |
| `RemoveLastLines(int)` / `RemoveLastLines(int, Encoding)` | Remove the last *n* lines, keep the rest |
| **Other** | |
| `CopyFrom(FileInfo)` | Replace the working copy's content with another file's content |

**`ConflictResolutionMode` Enum:**

| Value | Description |
|-------|-------------|
| `None` | No conflict detection; the working copy always overwrites the original on dispose |
| `LockWithReadShare` | Lock the original file for the duration; other processes may read but not write |
| `LockExclusive` | Lock the original file exclusively; no other process may read or write |
| `CheckLastWriteTimeAndThrow` | Compare the original's last-write timestamp before replacing; throw `IOException` if it changed |
| `CheckLastWriteTimeAndIgnoreUpdate` | Compare timestamps; silently skip the replacement if the original was modified externally |
| `CheckChecksumAndThrow` | Compute SHA-256 checksums before and after; throw `IOException` if the original's content changed |
| `CheckChecksumAndIgnoreUpdate` | Compute checksums; silently skip the replacement if the original's content changed |

#### DirectoryInfo Extensions

| Method                                                                  | Description                          |
|-------------------------------------------------------------------------|--------------------------------------|
| `RenameTo`                                                              | Rename directory                     |
| `Clear`                                                                 | Delete all contents                  |
| `GetSize`                                                               | Calculate total size recursively     |
| `GetRealPath`                                                           | Resolve symbolic links and junctions |
| `EnumerateFileSystemInfos(mode, filter)`                                | Enumerate with recursion filter      |
| `TrySetLastWriteTimeUtc` / `TrySetCreationTimeUtc` / `TrySetAttributes` | Safe attribute modification          |
| `TryCreate(recursive)` / `TryDelete(recursive)`                         | Safe creation/deletion               |
| `Directory(subdir, ignoreCase)` / `File(path, ignoreCase)`              | Navigate to children                 |
| `GetOrAddDirectory(name)`                                               | Get or create subdirectory           |
| `HasDirectory` / `HasFile` / `ContainsFile` / `ContainsDirectory`       | Content checks                       |
| `ExistsAndHasFiles`                                                     | Existence and content verification   |
| `GetTempFile`                                                           | Generate temporary files             |
| `IsJunction` / `IsSymbolicLink`                                         | Link detection                       |
| `GetJunctionTarget` / `GetSymbolicLinkTarget`                           | Read link targets                    |
| `CreateJunctionFrom/At` / `CreateSymbolicLinkFrom/At`                   | Create junctions and symbolic links  |
| `TryCreateJunctionFrom/At` / `TryCreateSymbolicLinkFrom/At`             | Try-pattern link creation            |

#### FileSystemInfo Extensions

| Method                                                      | Description                           |
|-------------------------------------------------------------|---------------------------------------|
| `NotExists` / `IsNullOrDoesNotExist` / `IsNotNullAndExists` | Existence checks                      |
| `RelativeTo`                                                | Compute relative path                 |
| `IsOnSamePhysicalDrive`                                     | Check if paths share a physical drive |
| `Age`                                                       | Time since last modification          |
| `IsDirectory`                                               | Check if entry is a directory         |

#### Stream Extensions

| Method                                                               | Description                                                |
|----------------------------------------------------------------------|------------------------------------------------------------|
| `CopyTo` / `CopyToAsync`                                             | Copy with progress callbacks                               |
| `ReadAllBytes` / `ReadToEnd`                                         | Read entire stream content                                 |
| `Read<TStruct>` / `Write<TStruct>`                                   | Read/write structs directly                                |
| `ToArray`                                                            | Convert stream to byte array                               |
| `IsAtEnd` / `IsNotAtEnd`                                             | Position checks                                            |
| Primitive I/O (bool, byte, short, int, long, float, double, decimal) | Read/Write with endianness support                         |
| String operations                                                    | Length-prefixed, zero-terminated, and fixed-length strings |

#### Path Extensions

| Method                                       | Description                                          |
|----------------------------------------------|------------------------------------------------------|
| `GetTempFile` / `GetTempDirectory`           | Create temporary files/directories                   |
| `GetTempFileToken` / `GetTempDirectoryToken` | Create disposable auto-cleanup tokens (RAII pattern) |
| `TryCreateFile` / `TryCreateDirectory`       | Safe creation methods                                |
| `GetUsableSystemTempDirectory`               | Find a writable temp directory                       |
| `GetTempFileName` / `GetTempDirectoryName`   | Generate temp path names                             |
| `NetworkPath` struct                         | Parse UNC and network paths                          |

**Temporary File/Directory Tokens (RAII auto-cleanup):**

A common source of bugs is forgetting to clean up temporary files, leading to disk space leaks over time. The token pattern solves this by tying the temp resource's lifetime to a `using` scope -- when the token is disposed, the file or directory is automatically deleted. This works even if an exception is thrown, and the finalizer provides a safety net if `Dispose` is never called.

A background `TemporaryTokenCleaner` singleton handles the actual deletion. If a file is locked or cannot be deleted immediately, a periodic timer (every 30 seconds) retries deletion. On process exit, all remaining tracked resources are forcibly cleaned up. Read-only, system, and hidden attributes are automatically stripped before deletion.

```csharp
// Temporary file with automatic cleanup -- no more orphaned temp files
using var tempFile = PathExtensions.GetTempFileToken();
File.WriteAllText(tempFile.File.FullName, "temporary data");
// ... use the temp file for processing ...
// File is automatically deleted when token is disposed

// Temporary directory with recursive cleanup
using var tempDir = PathExtensions.GetTempDirectoryToken();
File.WriteAllText(Path.Combine(tempDir.Directory.FullName, "data.txt"), "test");
File.WriteAllText(Path.Combine(tempDir.Directory.FullName, "config.json"), "{}");
// Entire directory and all its contents are recursively deleted when token is disposed

// Extending lifetime -- keep the temp file alive for at least 5 more minutes
tempFile.MinimumLifetimeLeft = TimeSpan.FromMinutes(5);
// Even if Dispose() is called now, the cleaner will wait until the minimum lifetime expires
```

**Factory Methods (on `PathExtensions`):**

| Method | Description |
|--------|-------------|
| `GetTempFileToken(name?, baseDirectory?)` | Creates a temporary file and returns an `ITemporaryFileToken`; file is deleted on dispose. Optional `name` for a specific filename; optional `baseDirectory` to override the system temp folder |
| `GetTempDirectoryToken(name?, baseDirectory?)` | Creates a temporary directory and returns an `ITemporaryDirectoryToken`; directory and all contents are recursively deleted on dispose. Same optional parameters as above |

**`ITemporaryFileToken` API:**

| Member | Description |
|--------|-------------|
| `File` | The `FileInfo` pointing to the temporary file |
| `MinimumLifetimeLeft` | `TimeSpan` property; get or set the minimum time the file must remain alive after `Dispose()` is called. Setting this extends the file's life so background tasks can finish before cleanup occurs |
| `Dispose()` | Marks the file for deletion. If `MinimumLifetimeLeft` is set, actual deletion is deferred until the lifetime expires |

**`ITemporaryDirectoryToken` API:**

| Member | Description |
|--------|-------------|
| `Directory` | The `DirectoryInfo` pointing to the temporary directory |
| `MinimumLifetimeLeft` | `TimeSpan` property; get or set the minimum time the directory must remain alive after `Dispose()` is called. Deletion of the directory and all contents is deferred until the lifetime expires |
| `Dispose()` | Marks the directory for recursive deletion. If `MinimumLifetimeLeft` is set, actual deletion is deferred |

**Cleanup Guarantees:**

- **Finalizer safety net**: If `Dispose()` is never called (e.g., the token is not in a `using` block), the destructor triggers cleanup during garbage collection.
- **Retry on failure**: If deletion fails (e.g., file is locked), a background timer retries every 30 seconds.
- **Process exit hook**: On `AppDomain.ProcessExit`, all remaining tracked resources are deleted regardless of their alive status.
- **Attribute stripping**: Read-only, system, and hidden file attributes are automatically removed before deletion to prevent access-denied errors.

#### Other IO

| Type                     | Description                                        |
|--------------------------|----------------------------------------------------|
| `BinaryReaderExtensions` | `ReadAllBytes` for BinaryReader                    |
| `BufferedStreamEx`       | Enhanced buffered stream wrapper                   |
| `TextReaderExtensions`   | Line enumeration for TextReader                    |
| `DriveInfoExtensions`    | Drive type queries, disk space info                |
| `VolumeExtensions`       | Windows volume enumeration and mount-point listing |
| `ConflictResolutionMode` | Enum for file conflict resolution strategies       |

---

### FastFileOperations

High-performance file and directory copying with progress reporting.

| Feature                                                           | Description                                                          |
|-------------------------------------------------------------------|----------------------------------------------------------------------|
| `CopyTo` / `CopyToAsync` (FileInfo)                               | Multi-stream buffered file copy with hard-link optimization          |
| `CopyTo` / `CopyToAsync` (DirectoryInfo)                          | Recursive directory copy with file comparers and conflict resolution |
| `BinaryFileComparer`                                              | Compare files byte-by-byte                                           |
| `FileLengthComparer`                                              | Compare files by length                                              |
| `FileSimpleAttributesComparer`                                    | Compare by attributes                                                |
| `FileCreationTimeComparer` / `FileLastWriteTimeComparer`          | Compare by timestamps                                                |
| `IFileComparer` / `IFileReport` / `IDirectoryReport`              | Interfaces for comparison and reporting                              |
| `IFileSystemOperation` / `IFileOperation` / `IDirectoryOperation` | Operation interfaces                                                 |

The standard `File.Copy` provides no progress feedback, no hard-link optimization, and no control over buffering strategy. `FastFileOperations` addresses all of these: it uses asynchronous multi-stream I/O with configurable read-ahead buffers, can create hard links instead of copying when files are on the same volume, and provides detailed progress callbacks at the chunk level. For directory copies, it uses parallel crawler and stream threads to maximize throughput on SSDs.

**File copy with progress reporting:**

```csharp
// Async file copy with chunk-level progress callbacks
var sourceFile = new FileInfo("large-dataset.bin");
var targetFile = new FileInfo("backup/large-dataset.bin");

var report = sourceFile.CopyToAsync(targetFile, overwrite: true, callback: r => {
  if (r.ReportType == FastFileOperations.ReportType.FinishedWrite) {
    var progress = r.Operation.BytesTransferred * 100 / r.Operation.TotalSize;
    Console.Write($"\rProgress: {progress}%");
  }
});

// Can do other work while the copy runs in the background
report.Operation.WaitTillDone();
if (report.Operation.ThrewException)
  throw report.Operation.Exception;
```

**Directory copy with synchronization and filtering:**

```csharp
var source = new DirectoryInfo("projects/website");
var target = new DirectoryInfo("deployment/website");

// Synchronize directories: copy new/changed files, delete extras in target
source.CopyTo(
  target,
  overwrite: true,
  allowHardLinks: true,          // Use hard links on same volume (instant, no disk space)
  allowIntegrate: true,          // Merge into existing target directory
  synchronizeTarget: true,       // Remove files in target that don't exist in source
  predicate: fsi => fsi.Name != ".git" && fsi.Extension != ".tmp", // Filter unwanted items
  callback: r => {
    if (r.ReportType == FastFileOperations.ReportType.FinishedWrite)
      Console.WriteLine($"Copied: {r.Source.Name}");
  }
);
```

---

### Diagnostics Extensions

| Type               | Method                     | Description                        |
|--------------------|----------------------------|------------------------------------|
| `Process`          | `GetParentProcess`         | Get the parent process             |
| `ProcessStartInfo` | `Execute` / `ExecuteAsync` | Execute process and capture output |
| `Stopwatch`        | `GetElapsedAndRestart`     | Return elapsed time and restart    |

---

### Threading and Concurrency

#### Thread Extensions

| Method             | Description                                                                                    |
|--------------------|------------------------------------------------------------------------------------------------|
| `IoBackgroundMode` | Pushes the current thread into Windows Vista+ low-IO priority mode; returns a disposable token |

**Low-IO Background Mode:**

When performing bulk I/O operations (backups, indexing, log rotation) on a user's machine, you want to avoid starving interactive applications of disk bandwidth. The `IoBackgroundMode` token uses the Windows `SetThreadPriority` API to lower the current thread's I/O priority, then automatically restores it when disposed. This is a simple way to be a "good citizen" during heavy background work.

```csharp
// Reduce I/O priority for background work so the user's apps stay responsive
using (Thread.CurrentThread.IoBackgroundMode()) {
  // All file I/O on this thread now runs at low priority
  foreach (var file in Directory.EnumerateFiles(backupSource, "*", SearchOption.AllDirectories))
    File.Copy(file, Path.Combine(backupTarget, Path.GetFileName(file)), overwrite: true);
}
// I/O priority is automatically restored here
```

#### Synchronization Primitives

| Type                         | Description                                                 |
|------------------------------|-------------------------------------------------------------|
| `CallOnTimeout`              | Disposable timer that calls a delegate after a timeout      |
| `Future<TValue>` / `Future`  | Value computed asynchronously with blocking `Value` access  |
| `EventExtensions`            | `AsyncInvoke` for event handlers and multicast delegates    |
| `ManualResetEventExtensions` | `IsSet` to check if event is signaled                       |
| `SemaphoreSlimExtensions`    | `TryWait`, `Enter` (returns `IDisposable` for using-blocks) |

#### InterlockedEx (T4-generated for int, long, uint, ulong, float, double)

| Method                                                                             | Description                      |
|------------------------------------------------------------------------------------|----------------------------------|
| `CompareExchange` / `Exchange` / `Read`                                            | Atomic read/write/swap           |
| `Increment` / `Decrement` / `Add` / `Subtract`                                     | Atomic arithmetic                |
| `Multiply` / `Divide` / `Modulo`                                                   | Atomic multiplicative operations |
| `NAnd` / `NOr` / `Xor` / `NXor` / `Not`                                            | Atomic bitwise operations        |
| `ArithmeticShiftLeft/Right` / `LogicalShiftLeft/Right`                             | Atomic shifts                    |
| `RotateLeft` / `RotateRight`                                                       | Atomic rotations (uint/ulong)    |
| `SaturatingAdd` / `SaturatingSubtract` / `SaturatingMultiply` / `SaturatingDivide` | Saturating atomic arithmetic     |
| `HasFlag` / `SetFlag` / `ClearFlag` / `ToggleFlag`                                 | Atomic enum flag operations      |

#### Task Management

| Type                       | Description                                                                       |
|----------------------------|-----------------------------------------------------------------------------------|
| `DeferredTask<T>`          | Delays execution until no new values arrive within a cooldown window              |
| `ScheduledTask`            | Coalesces multiple calls into a single deferred execution                         |
| `ScheduledTask<T>`         | Like `ScheduledTask` but passes the latest value to the action                    |
| `ScheduledCombinedTask<T>` | Collects all values during the deferral window and executes once with all of them |
| `Sequential`               | Ensures actions execute sequentially (queued single-threaded execution)           |
| `Future<T>`                | Future/promise pattern implementation                                             |
| `TaskExtensions`           | `TimeoutAfter`, `WhenAll`/`WhenAny` helpers                                       |

**DeferredTask vs ScheduledTask** - Both delay execution and coalesce rapid calls, but they differ in cancellation and value handling:

- **`DeferredTask<T>`**: Each `Schedule(value)` resets the timer. Execution only fires after the delay elapses with no new calls. Supports `Abort()` to cancel the pending execution. Supports `Now(value)` for immediate execution. `allowTaskOverlapping` controls whether a new task can start while the previous action is still running. `autoAbortOnSchedule` automatically aborts the current execution when a new value is scheduled.
- **`ScheduledTask`/`ScheduledTask<T>`**: `Schedule()` starts a timer; subsequent calls during the deferral window update the value but do NOT reset the timer. The action fires once when the timer elapses (always with the latest value). `waitUntilTaskReturnedBeforeNextSchedule` prevents re-scheduling until the action completes. No explicit abort - the task always fires.
- **`ScheduledCombinedTask<T>`**: Like `ScheduledTask<T>` but collects ALL values during the deferral window into an array and passes them all to the action at once. Supports `Abort()` to discard collected values.

```csharp
// DeferredTask: save-to-disk after user stops typing for 500ms
// Each keystroke resets the 500ms timer - only fires once typing stops
var autoSave = new DeferredTask<string>(
  text => File.WriteAllText("draft.txt", text),
  waitTime: TimeSpan.FromMilliseconds(500)
);
textBox.TextChanged += (s, e) => autoSave.Schedule(textBox.Text);

// Cancel if the form is closing
autoSave.Abort();

// ScheduledTask: rate-limit UI refresh to at most once per 200ms
// First call starts the 200ms timer; more calls during that window are ignored
var refreshTask = new ScheduledTask(
  () => UpdateUI(),
  deferredTime: 200
);
dataSource.Changed += () => refreshTask.Schedule();  // may fire 100x/sec, UI updates at most 5x/sec

// ScheduledCombinedTask: batch log writes
// Collects all log entries during the 1-second window, writes them all at once
var batchLogger = new ScheduledCombinedTask<string>(
  entries => File.AppendAllLines("log.txt", entries),
  deferredTime: 1000
);
batchLogger.Schedule("Request received");
batchLogger.Schedule("Processing started");
// ... after 1 second, writes both lines in a single I/O operation
```

---

### System.Timers

| Type                 | Description                                                               |
|----------------------|---------------------------------------------------------------------------|
| `HighPrecisionTimer` | Timer using multimedia/high-resolution APIs for sub-millisecond precision |

---

### Security and Cryptography

#### Custom Hash Algorithms

| Type                              | Description                                             |
|-----------------------------------|---------------------------------------------------------|
| `Adler`                           | Adler checksum (16/32/64-bit)                           |
| `Fletcher`                        | Fletcher checksum (8/16/32/64-bit)                      |
| `JavaHash`                        | Java-compatible hash (32/64-bit)                        |
| `LRC8`                            | Longitudinal Redundancy Check (8-bit)                   |
| `Pearson`                         | Pearson hashing (configurable output size, supports IV) |
| `Tiger`                           | Tiger hash (up to 192-bit)                              |
| `Whirlpool`                       | Whirlpool hash (512-bit)                                |
| `IAdvancedHashAlgorithm`          | Interface exposing supported output/IV bit sizes        |
| `RandomNumberGeneratorExtensions` | `Next(maxValue)` for `RandomNumberGenerator`            |

#### SecureString Extensions

| Method                             | Description                            |
|------------------------------------|----------------------------------------|
| `ToUnsecureString` / `ToByteArray` | Convert SecureString to usable formats |
| `EqualsSecure`                     | Secure string comparison               |

---

### StringBuilder Extensions (`StringBuilder`)

StringBuilder manipulation and utilities.

| Method                                         | Description                                  |
|------------------------------------------------|----------------------------------------------|
| `Append(...)`                                  | Overloads for appending various types        |
| `Prepend(...)`                                 | Insert content at the beginning              |
| `AppendLine(...)`                              | Append with line terminator                  |
| `AppendIf(condition, ...)`                     | Conditional appending                        |
| `AppendLineIf(condition, ...)`                 | Conditional append-line                      |
| `Replace(...)` / `Remove(...)` / `Insert(...)` | Enhanced manipulation                        |
| `Contains(...)`                                | Check if builder contains substring          |
| `StartsWith(...)` / `EndsWith(...)`            | Content boundary checking                    |
| `ToStringAndClear()`                           | Get string and clear the builder in one call |

### Regex Extensions (`Regex`, `Match`)

| Method                             | Description                         |
|------------------------------------|-------------------------------------|
| `MatchAll` / `GetMatches`          | Retrieve all matches                |
| `ReplaceWith(...)`                 | Functional replacement patterns     |
| `IsMatch` / `HasMatch`             | Pattern testing                     |
| `GetGroupValue` / `GetGroupValues` | Extract capture groups              |
| `GetValue` / `GetValues` (Match)   | Value extraction from Match objects |

### CultureInfo Extensions (`CultureInfo`)

| Method                            | Description                  |
|-----------------------------------|------------------------------|
| `IsNeutral` / `IsSpecific`        | Culture type checks          |
| `GetParent` / `GetAncestors`      | Culture hierarchy navigation |
| `IsAncestorOf` / `IsDescendantOf` | Culture relationship checks  |

### Text Encoding

| Type                  | Description                               |
|-----------------------|-------------------------------------------|
| `Ascii7BitPacking`    | 7-bit ASCII packing/unpacking (GSM-style) |
| `Windows1252Encoding` | Windows-1252 encoding implementation      |

---

### Reflection Extensions

| Type           | Method                                                      | Description              |
|----------------|-------------------------------------------------------------|--------------------------|
| `Assembly`     | `GetEmbeddedResource` / `GetEmbeddedResourceNames`          | Resource extraction      |
| `Assembly`     | `GetLoadableTypes` / `GetTypesImplementing<T>`              | Type discovery           |
| `Assembly`     | `GetFileVersion` / `GetProductVersion`                      | Version info             |
| `MemberInfo`   | `GetCustomAttribute<T>` / `HasAttribute<T>`                 | Attribute helpers        |
| `MethodBase`   | `GetParameterTypes`                                         | Get parameter type array |
| `MethodInfo`   | `CreateDelegate` / `IsExtensionMethod` / `MatchesSignature` | Method utilities         |
| `PropertyInfo` | `GetBackingField` / `GetValue<T>` / `SetValue<T>`           | Property access          |
| `FieldInfo`    | `GetValue<T>` / `SetValue<T>`                               | Field access             |

---

### ComponentModel Extensions

| Type                                        | Description                                          |
|---------------------------------------------|------------------------------------------------------|
| `BindingListExtensions`                     | Sorting, filtering, searching helpers                |
| `SortableBindingList<T>`                    | `BindingList<T>` with sorting support                |
| `BindingListView<T>`                        | Filterable/sortable view over a binding list         |
| `SynchronizeInvokeExtensions`               | `InvokeIfRequired` for ISynchronizeInvoke            |
| `DefaultValueAttributeExtensions`           | Extended `DefaultValueAttribute` for enums and types |
| `PropertyChanged` / `PropertyChanging` (T4) | Strongly-typed event raise helpers                   |

---

### Networking Extensions

| Type                  | Method                                                 | Description            |
|-----------------------|--------------------------------------------------------|------------------------|
| `IPAddress`           | `IsInRange` / `GetSubnet` / `IsPrivate` / `IsLoopback` | IP address utilities   |
| `IPHelper`            | `GetLocalIPAddresses` / `IsPortAvailable`              | Network helper methods |
| `PhysicalAddress`     | `ToFormattedString`                                    | MAC address formatting |
| `TcpClient`           | `IsConnected` / `SendAndReceive`                       | TCP connection helpers |
| `WebHeaderCollection` | `ToDictionary`                                         | Header conversion      |

---

### Data Extensions

| Type         | Method                                        | Description                   |
|--------------|-----------------------------------------------|-------------------------------|
| `DataTable`  | `ToEnumerable` / `AddColumn` / `RemoveColumn` | DataTable helpers             |
| `DataRow`    | `GetValue<T>` / `SetValue`                    | Typed row accessors           |
| `DataRecord` | `GetValue<T>`                                 | Typed `IDataRecord` accessors |
| `SqlCommand` | `ExecuteAndReturn` / `AddParameter`           | SqlCommand helpers            |
| `DataContext` / `Table` | LINQ-to-SQL extension helpers                 |

---

### XML Extensions

| Type                     | Method                                                                      | Description                         |
|--------------------------|-----------------------------------------------------------------------------|-------------------------------------|
| `XmlNode`                | `SelectSingleNode<T>` / `GetAttribute` / `GetChildNodes` / `GetDescendants` | Typed XML node access and traversal |
| `XmlAttributeCollection` | `ToEnumerable` / `ContainsAttribute` / `TryGetAttribute`                    | Attribute collection helpers        |
| `XDocument`              | `SaveFormatted`                                                             | Save with formatting                |

---

### LINQ Extensions

| Type         | Method                        | Description                        |
|--------------|-------------------------------|------------------------------------|
| `IQueryable` | `WhereIf` / `OrderByProperty` | Conditional LINQ query composition |

---

### Globalization Extensions

| Type          | Method                            | Description                  |
|---------------|-----------------------------------|------------------------------|
| `CultureInfo` | `IsNeutral` / `IsSpecific`        | Culture type checks          |
| `CultureInfo` | `GetParent` / `GetAncestors`      | Culture hierarchy navigation |
| `CultureInfo` | `IsAncestorOf` / `IsDescendantOf` | Culture relationship checks  |

---

### Buffers Extensions

| Type           | Method          | Description                                               |
|----------------|-----------------|-----------------------------------------------------------|
| `ArrayPool<T>` | `RentAndReturn` | Returns a disposable wrapper that auto-returns the buffer |

---

### AppDomain Extensions

| Method                             | Description                                   |
|------------------------------------|-----------------------------------------------|
| `GetParentProcess`                 | Get the parent process of the current process |
| `GetAllTypes` / `GetAllAssemblies` | Enumerate loaded types and assemblies         |

---

## New Types

### String Types

Specialized string types for interoperability, memory efficiency, and encoding-specific scenarios.

#### Overview

| Type          | Storage                 | Encoding     | Behavior                       |
|---------------|-------------------------|--------------|--------------------------------|
| `StringZ`     | `string`                | UTF-16       | Cuts at first '\0'             |
| `AsciiZ`      | `byte[]` (7-bit packed) | 7-bit ASCII  | Cuts at first 0x00             |
| `AnsiZ`       | `byte[]`                | Windows-1252 | Cuts at first 0x00             |
| `AsciiString` | `byte[]` (7-bit packed) | 7-bit ASCII  | Full content preserved         |
| `AnsiString`  | `byte[]`                | Windows-1252 | Full content preserved         |
| `FixedString` | `char[]`                | UTF-16       | Fixed capacity via constructor |
| `FixedAscii`  | `byte[]` (7-bit packed) | 7-bit ASCII  | Fixed capacity via constructor |
| `FixedAnsi`   | `byte[]`                | Windows-1252 | Fixed capacity via constructor |

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

| Type       | Size   | Format   | Exponent | Mantissa | Bias  | Use Case                              |
|------------|--------|----------|----------|----------|-------|---------------------------------------|
| `BFloat8`  | 8-bit  | 1+5+2    | 5 bits   | 2 bits   | 15    | Truncated Half, ML inference          |
| `BFloat16` | 16-bit | 1+8+7    | 8 bits   | 7 bits   | 127   | Upper 16 bits of float32, ML training |
| `BFloat32` | 32-bit | 1+11+20  | 11 bits  | 20 bits  | 1023  | Upper 32 bits of double               |
| `BFloat64` | 64-bit | 1+15+48  | 15 bits  | 48 bits  | 16383 | Extended range (quad exponent)        |
| `Quarter`  | 8-bit  | 1+5+2    | 5 bits   | 2 bits   | 15    | IEEE 754 minifloat                    |
| `E4M3`     | 8-bit  | 1+4+3    | 4 bits   | 3 bits   | 7     | ML format, no infinity                |
| `Int96`    | 96-bit | signed   | -        | -        | -     | Extended integer range                |
| `UInt96`   | 96-bit | unsigned | -        | -        | -     | Extended unsigned integer range       |

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

Specialized 8-bit format optimized for machine learning workloads, trading range for precision.

- **`E4M3`** - 8-bit ML format (1+4+3), more precision, no infinity representation

```csharp
// E4M3 - 4 exponent bits, 3 mantissa bits (more precision, no infinity)
E4M3 e4 = (E4M3)1.25f;
Console.WriteLine(E4M3.IsFinite(e4));           // true (E4M3 has no infinity)
Console.WriteLine(E4M3.IsNaN(E4M3.MaxValue));   // false

// Conversions
float original = 3.14159f;
E4M3 e4val = (E4M3)original;
float fromE4 = (float)e4val;  // ~3.0 (3 mantissa bits)
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

#### Gray-Code Types

| Type                                     | Description                                          |
|------------------------------------------|------------------------------------------------------|
| `Gray8` / `Gray16` / `Gray32` / `Gray64` | Gray-code encoded unsigned integers (8/16/32/64-bit) |

#### ZigZag Encoding Types

| Type                                             | Description                                                                            |
|--------------------------------------------------|----------------------------------------------------------------------------------------|
| `ZigZag8` / `ZigZag16` / `ZigZag32` / `ZigZag64` | ZigZag-encoded signed integers (maps signed to unsigned for efficient varint encoding) |

#### BCD Types

| Type                                                         | Description                                     |
|--------------------------------------------------------------|-------------------------------------------------|
| `PackedBCD8` / `PackedBCD16` / `PackedBCD32` / `PackedBCD64` | Packed Binary-Coded Decimal (2 digits per byte) |
| `UnpackedBCD`                                                | Arbitrary-precision unpacked BCD                |

#### Fixed-Point Types

| Type      | Description                         |
|-----------|-------------------------------------|
| `Q3_4`    | Signed 3.4 fixed point (8-bit)      |
| `Q7_8`    | Signed 7.8 fixed point (16-bit)     |
| `Q15_16`  | Signed 15.16 fixed point (32-bit)   |
| `Q31_32`  | Signed 31.32 fixed point (64-bit)   |
| `UQ4_4`   | Unsigned 4.4 fixed point (8-bit)    |
| `UQ8_8`   | Unsigned 8.8 fixed point (16-bit)   |
| `UQ16_16` | Unsigned 16.16 fixed point (32-bit) |
| `UQ32_32` | Unsigned 32.32 fixed point (64-bit) |

#### Configurable Floating-Point Types

Generic floating-point types with configurable mantissa size. Exponent bits are computed automatically as `TotalBits - sign - mantissaBits`. The storage type determines signedness: signed types have a sign bit, unsigned types use saturating arithmetic.

- **`ConfigurableFloatingPoint<TStorage>`** - Generic floating-point with configurable bit layout

**Storage Types Supported:** `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`, `ulong`, `long`, `UInt96`, `Int96`, `UInt128`, `Int128`

**Key Features:**

- Storage type signedness determines if sign bit is present
- Single `mantissaBits` parameter; exponent computed automatically
- IEEE 754-like special values (NaN, Infinity, Zero)
- Unsigned types use saturating arithmetic (subtraction saturates to zero)
- Exact BigInteger-based arithmetic (no precision loss for 64-bit+ storage)
- Fast path for same-config operations with mantissa <= 52 bits
- Cross-config arithmetic: left operand's config determines result
- Cross-type arithmetic with `ConfigurableFixedPoint<TStorage>`
- `MantissaBitsFromExponent(int exponentBits)` helper for exponent-based thinking
- `ConvertTo(int mantissaBits)` for config conversion

| Storage   | Default Format | Sign | Exponent | Mantissa |
|-----------|----------------|------|----------|----------|
| `sbyte`   | 1+4+3          | Yes  | 4 bits   | 3 bits   |
| `byte`    | 5+3            | No   | 5 bits   | 3 bits   |
| `short`   | 1+5+10         | Yes  | 5 bits   | 10 bits  |
| `ushort`  | 6+10           | No   | 6 bits   | 10 bits  |
| `int`     | 1+8+23         | Yes  | 8 bits   | 23 bits  |
| `uint`    | 9+23           | No   | 9 bits   | 23 bits  |
| `long`    | 1+11+52        | Yes  | 11 bits  | 52 bits  |
| `ulong`   | 12+52          | No   | 12 bits  | 52 bits  |
| `Int96`   | 1+15+80        | Yes  | 15 bits  | 80 bits  |
| `UInt96`  | 16+80          | No   | 16 bits  | 80 bits  |
| `Int128`  | 1+15+112       | Yes  | 15 bits  | 112 bits |
| `UInt128` | 16+112         | No   | 16 bits  | 112 bits |

Standard floating-point types (float, double) have fixed precision that may be too much or too little for a given use case. `ConfigurableFloatingPoint` lets you define exactly how many bits go to the mantissa vs. exponent, trading range for precision or vice versa. This is valuable in ML inference (where 8-bit floats save memory and bandwidth), scientific simulations (where you need to test numerical stability at different precisions), and embedded/protocol scenarios with non-standard bit widths.

```csharp
// Signed 16-bit floating point (like IEEE 754 binary16 with 10-bit mantissa)
var a = ConfigurableFloatingPoint<short>.FromDouble(3.14, mantissaBits: 10);
var b = ConfigurableFloatingPoint<short>.FromDouble(2.0, mantissaBits: 10);
var result = a * b;  // ~6.28

// 8-bit float for ML inference -- extreme compression with 2-bit mantissa
var weight = ConfigurableFloatingPoint<sbyte>.FromDouble(0.75, mantissaBits: 2);
var activation = ConfigurableFloatingPoint<sbyte>.FromDouble(1.5, mantissaBits: 2);
var output = weight * activation; // approximate result, fits in a single byte

// Unsigned 32-bit float -- no sign bit means one extra exponent bit for extended range
var unsignedVal = ConfigurableFloatingPoint<uint>.FromDouble(1e30, mantissaBits: 23);
// Subtraction uses saturating arithmetic (clamps to 0 instead of going negative)

// Cross-config arithmetic: the left operand's config determines the result's layout
var highPrecision = ConfigurableFloatingPoint<int>.FromDouble(1.0 / 3.0, mantissaBits: 27);
var lowPrecision = ConfigurableFloatingPoint<int>.FromDouble(1.0 / 3.0, mantissaBits: 15);
var mixed = highPrecision + lowPrecision; // Result uses 27-bit mantissa

// Special values (always use default config for the storage type)
var nan = ConfigurableFloatingPoint<int>.NaN;
var inf = ConfigurableFloatingPoint<int>.PositiveInfinity;
var negInf = ConfigurableFloatingPoint<int>.NegativeInfinity;
Console.WriteLine(ConfigurableFloatingPoint<int>.IsNaN(nan));       // true
Console.WriteLine(ConfigurableFloatingPoint<int>.IsInfinity(inf)); // true

// Cross-type arithmetic: floating + fixed
var fp = ConfigurableFloatingPoint<int>.FromDouble(2.5, 23);
var fixedPt = ConfigurableFixedPoint<int>.FromDouble(1.5, 16);
var mixedResult = fp + fixedPt;  // Result is floating-point with fp's config

// Convert between configs
var wide = ConfigurableFloatingPoint<int>.FromDouble(1.0, 23);
var converted = wide.ConvertTo(20);  // Now 20 mantissa bits

// Helper for exponent-based thinking
var m = ConfigurableFloatingPoint<int>.MantissaBitsFromExponent(8);  // 23
```

#### Configurable Fixed-Point Types

Generic fixed-point types with configurable integer and fractional parts. The storage type determines signedness.

- **`ConfigurableFixedPoint<TStorage>`** - Generic fixed-point with configurable precision

**Storage Types Supported:** `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`, `ulong`, `long`, `UInt96`, `Int96`, `UInt128`, `Int128`

**Key Features:**

- Storage type signedness determines if negative values are supported
- Configurable fractional bits via `Configure(fractionalBits)`
- Unsigned types use saturating arithmetic
- High-precision arithmetic using `BigInteger` internally
- Math helpers: `Floor`, `Ceiling`, `Round`, `Truncate`, `FractionalPart`
- Cross-config arithmetic: left operand's config determines result
- Cross-type arithmetic with `ConfigurableFloatingPoint<TStorage>`
- `ConvertTo(int fractionalBits)` for config conversion
- Exact cross-config comparison (rescales to max precision)

| Storage   | Default Format | Sign | Integer | Fractional |
|-----------|----------------|------|---------|------------|
| `sbyte`   | Q3.4           | Yes  | 3 bits  | 4 bits     |
| `byte`    | UQ4.4          | No   | 4 bits  | 4 bits     |
| `short`   | Q7.8           | Yes  | 7 bits  | 8 bits     |
| `ushort`  | UQ8.8          | No   | 8 bits  | 8 bits     |
| `int`     | Q15.16         | Yes  | 15 bits | 16 bits    |
| `uint`    | UQ16.16        | No   | 16 bits | 16 bits    |
| `long`    | Q31.32         | Yes  | 31 bits | 32 bits    |
| `ulong`   | UQ32.32        | No   | 32 bits | 32 bits    |
| `Int96`   | Q47.48         | Yes  | 47 bits | 48 bits    |
| `UInt96`  | UQ48.48        | No   | 48 bits | 48 bits    |
| `Int128`  | Q63.64         | Yes  | 63 bits | 64 bits    |
| `UInt128` | UQ64.64        | No   | 64 bits | 64 bits    |

Fixed-point arithmetic avoids the rounding surprises of floating-point by using a fixed number of fractional bits. This makes it ideal for financial calculations where exact decimal fractions matter, DSP/audio processing where deterministic precision is required, and embedded systems where hardware floating-point is unavailable. The configurable fractional bits let you choose your trade-off between integer range and fractional resolution.

```csharp
// Signed 32-bit fixed point (Q15.16) -- 16 integer bits, 16 fractional bits
var price = ConfigurableFixedPoint<int>.FromDouble(19.99, fractionalBits: 16);
var quantity = ConfigurableFixedPoint<int>.FromDouble(3.0, fractionalBits: 16);
var total = price * quantity; // 59.97 -- exact, no floating-point drift

// High-resolution 64-bit fixed point (Q31.32) -- sub-nanometer precision
var measurement = ConfigurableFixedPoint<long>.FromDouble(3.141592653589793, fractionalBits: 32);
var rounded = measurement.Round(4); // 3.1416

// 8-bit fixed point for embedded/protocol scenarios (Q3.4)
var sensorValue = ConfigurableFixedPoint<sbyte>.FromDouble(5.5, fractionalBits: 4);
var calibrated = sensorValue * ConfigurableFixedPoint<sbyte>.FromDouble(1.1, fractionalBits: 4);

// Unsigned 16-bit fixed point (no negative values)
var x = ConfigurableFixedPoint<ushort>.FromDouble(5.0, 8);
var y = ConfigurableFixedPoint<ushort>.FromDouble(10.0, 8);
var diff = x - y;  // Saturates to zero
Console.WriteLine(diff.ToDouble());  // 0.0

// Math operations
var value = ConfigurableFixedPoint<int>.FromDouble(3.7, 16);
Console.WriteLine(ConfigurableFixedPoint<int>.Floor(value).ToDouble());    // 3.0
Console.WriteLine(ConfigurableFixedPoint<int>.Ceiling(value).ToDouble()); // 4.0
Console.WriteLine(ConfigurableFixedPoint<int>.Round(value).ToDouble());    // 4.0

// Cross-config arithmetic: left operand's config wins
var highPrec = ConfigurableFixedPoint<int>.FromDouble(1.0, 16);
var lowPrec = ConfigurableFixedPoint<int>.FromDouble(2.0, 8);
var crossResult = highPrec + lowPrec;  // Result has 16 fractional bits

// Cross-type arithmetic: fixed + floating
var fixedVal = ConfigurableFixedPoint<int>.FromDouble(2.5, 16);
var floatVal = ConfigurableFloatingPoint<int>.FromDouble(1.5, 23);
var mixedResult = fixedVal + floatVal;  // Result is fixed-point with fixedVal's config

// Convert between configs
var convertedFp = highPrec.ConvertTo(20);  // Now 20 fractional bits
```

#### Common Numeric API Surface

All numeric types implement:

**Interfaces:**

- `IComparable`, `IComparable<T>` - Comparison support
- `IEquatable<T>` - Equality support
- `IFormattable`, `ISpanFormattable` - String formatting support
- `IParsable<T>`, `ISpanParsable<T>` - Parsing support (including span-based parsing)

**Properties (floating-point types):**

- `RawValue` - Raw bit representation
- `Zero`, `One` - Common values
- `Epsilon` - Smallest positive subnormal
- `MaxValue`, `MinValue` - Finite bounds
- `PositiveInfinity`, `NegativeInfinity` - Infinity values (always default config; except E4M3)
- `NaN` - Not a Number value (always default config)
- `DefaultMantissaBits` - IEEE 754 standard mantissa bits for the storage type

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
- Explicit/implicit conversions to/from standard types including `Half` and `Quarter`

---

### Property and State Management

| Type                                        | Description                                                                                                      |
|---------------------------------------------|------------------------------------------------------------------------------------------------------------------|
| `FastLazy<T>`                               | Thread-safe lazy initialization; replaces its getter function pointer after first access for maximum performance |
| `IndexedProperty<TIndex, TResult>`          | Provides indexer syntax (`property[key]`) backed by getter/setter delegates                                      |
| `ReadOnlyIndexedProperty<TIndex, TResult>`  | Read-only indexed property                                                                                       |
| `WriteOnlyIndexedProperty<TIndex, TResult>` | Write-only indexed property                                                                                      |
| `RealtimeProperty<T>`                       | Fetches value asynchronously with timeout, returning last known value                                            |
| `SlowProperty<TValue, TIntermediate>`       | Returns intermediate value while real value loads asynchronously                                                 |
| `StaticMethodLocal<T>`                      | Emulates C-style static local variables scoped to source-code location                                           |
| `DynamicObjectFactory`                      | Runtime object creation using dynamic IL emission                                                                |

```csharp
// FastLazy example
FastLazy<ExpensiveObject> lazy = new(() => new ExpensiveObject());
var value = lazy.Value; // computed once, then cached
lazy.Reset();           // force recomputation on next access

// StaticMethodLocal example
public void MyMethod() {
  var counter = StaticMethodLocal<int>.GetOrAdd();
  counter.Ref++;
  Console.WriteLine($"Called {counter} times");
}

// Shared static locals by name
public void MethodA() { StaticMethodLocal<int>.GetOrAddByName("shared").Ref++; }
public void MethodB() { Console.WriteLine(StaticMethodLocal<int>.GetOrAddByName("shared")); }
```

---

### Change Tracking

| Type                                     | Description                                      |
|------------------------------------------|--------------------------------------------------|
| `IChangeSet<TItem>` / `ChangeSet<TItem>` | Interface and implementation for change tracking |
| `ChangeType`                             | Enum: `Added`, `Removed`, `Changed`, `Equal`     |
| `IChangeSet<TKey, TValue>`               | Dictionary change set with key/value pairs       |

Change tracking is useful whenever you need to compute a diff between two versions of a data structure -- for example, synchronizing local state with a remote source, generating audit logs, or building undo/redo systems. The `CompareTo` extension method is available on arrays, dictionaries, hash sets, and general enumerables. It returns a lazy `IEnumerable` of change-set entries, each tagged with a `ChangeType` indicating what happened to that element.

**Array diff -- detecting added, removed, and unchanged elements:**

```csharp
var oldState = new[] { "Alice", "Bob", "Charlie" };
var newState = new[] { "Alice", "Charlie", "Dave" };

foreach (var change in newState.CompareTo(oldState))
  switch (change.Type) {
    case ChangeType.Added:   Console.WriteLine($"+ {change.Current}"); break; // Dave
    case ChangeType.Removed: Console.WriteLine($"- {change.Other}");   break; // Bob
    case ChangeType.Equal:   Console.WriteLine($"= {change.Current}"); break; // Alice, Charlie
  }
```

**Dictionary diff -- detecting value changes, additions, and removals by key:**

Particularly valuable for configuration management, where you need to know exactly which settings changed, which were added, and which were removed between two snapshots.

```csharp
var oldConfig = new Dictionary<string, int> { ["timeout"] = 30, ["retries"] = 3, ["port"] = 8080 };
var newConfig = new Dictionary<string, int> { ["timeout"] = 60, ["retries"] = 3, ["workers"] = 4 };

foreach (var change in newConfig.CompareTo(oldConfig))
  Console.WriteLine($"{change.Type}: {change.Key} = {change.Current} (was {change.Other})");
// Changed: timeout = 60 (was 30)
// Equal: retries = 3 (was 3)
// Added: workers = 4 (was 0)
// Removed: port = 0 (was 8080)
```

---

### Enums

| Type                     | Description                                       |
|--------------------------|---------------------------------------------------|
| `LineBreakMode`          | CR, LF, CRLF, LFCR, NEL, VT, FF, LS, PS, NUL, All |
| `TruncateMode`           | KeepStart, KeepEnd                                |
| `CaseComparison`         | Ordinal, CurrentCulture, InvariantCulture         |
| `ConflictResolutionMode` | Skip, Overwrite, Rename, etc.                     |
| `RecursionMode`          | None, AllDirectories                              |
| `ReportType`             | FastFileOperations report types                   |
| `ContinuationType`       | FastFileOperations continuation types             |
| `InvalidCharBehavior`    | Throw, Replace, Skip (for ASCII string types)     |

---

## Performance Features

### Optimizations

- **Unsafe Code Blocks** - Direct memory manipulation for performance
- **SIMD Operations** - Vectorized operations using Vector512/256/128 with fallbacks
- **Aggressive Inlining** - Use of `MethodImplOptions.AggressiveInlining`
- **Stack Allocation** - `stackalloc` for temporary buffers
- **Span\<T\> and Memory\<T\>** - Modern .NET memory management
- **Duff's Device Unrolling** - 8x loop unrolling for throughput
- **Hardware Intrinsics** - FMA, SIMD, and CPU-specific optimizations

### Memory Efficiency

- **Reduced Allocations** - Operations designed to avoid unnecessary heap allocations
- **Object Pooling** - Reusable object patterns where applicable
- **Block-based Operations** - `Block32`, `Block64` for memory operations
- **7-bit Packing** - ASCII string types save 12.5% memory
- **Bounds Check Elimination** - Loop construction to help JIT optimize

---

## Installation and Usage

```xml
<PackageReference Include="FrameworkExtensions.Corlib" Version="*" />
```

```csharp
using System;

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
var fileHash = file.ComputeSHA256Hash();
```

---

## Target Frameworks

Multi-targeting support:

- .NET Framework: `net35`, `net40`, `net45`, `net46`, `net461`, `net462`, `net47`, `net471`, `net472`, `net48`
- .NET Standard: `netstandard2.0`
- .NET Core/5+: `netcoreapp3.1`, `net5.0`, `net6.0`, `net7.0`, `net8.0`, `net9.0`

---

## Library Statistics

### Overview

- **3,300+ Extension Methods** across common .NET types
- **50+ .NET Types Extended** covering major framework types
- **15+ Data Types** with type-safe parsing support
- **Multiple .NET Versions** from .NET 3.5 to .NET 9.0
- **200+ Source Files** organized by namespace

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

## Known Limitations

- `DeepClone`, `ToFile`, and `FromFile` methods using `BinaryFormatter` are only available on frameworks where `BinaryFormatter` is not deprecated
- `RealtimeProperty` and `SlowProperty` use `BeginInvoke`/`EndInvoke` which may not be available on all platforms
- File system link operations (hard links, junctions, symbolic links) are Windows-specific via P/Invoke
- `HighPrecisionTimer` uses Windows multimedia timer APIs
- Some T4-generated overloads may produce large binary sizes when used across many numeric types
- `ExecutiveQueue` uses `BeginInvoke` for async callback execution, limiting portability

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
