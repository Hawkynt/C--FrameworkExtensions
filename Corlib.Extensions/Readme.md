# FrameworkExtensions.Corlib

[![Build](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/NewBuild.yml)
[![Tests](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml/badge.svg)](https://github.com/Hawkynt/C--FrameworkExtensions/actions/workflows/Tests.yml)
[![Last Commit](https://img.shields.io/github/last-commit/Hawkynt/C--FrameworkExtensions?branch=master)](https://github.com/Hawkynt/C--FrameworkExtensions/commits/master/Corlib.Extensions)
[![NuGet Version](https://img.shields.io/nuget/v/FrameworkExtensions.Corlib)](https://www.nuget.org/packages/FrameworkExtensions.Corlib/)
[![License](https://img.shields.io/badge/License-LGPL_3.0-blue)](https://licenses.nuget.org/LGPL-3.0-or-later)

> The core extension-method library: several thousand additions to the BCL types you touch every day, from string and array handling to guards, spans and concurrency.

## 📦 Installation

```bash
dotnet add package FrameworkExtensions.Corlib
```

## ✨ Features
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

## 🧭 Extension methods by type
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
| `PropertyInfo` | `GetBackingField` / `GetValue<T>` / `SetValue<T>`           
| Property access          |
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

See [CONTRIBUTING.md](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/CONTRIBUTING.md) for detailed guidelines on:

- Code style and conventions
- Performance requirements
- Testing categories and patterns
- Architecture principles

## 🚀 Quick start

Add the package, then use the members catalogued above — they are extension methods, so they appear on the framework types directly once the namespace is in scope.

## 📚 API reference

<!-- API:BEGIN generated by Hawkynt/RepositoryTemplate/package-readme — edit the XML docs in source, not here -->

### Namespace `(global namespace)`

[`Memoize`](#memoize)

#### `Memoize`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> GetOrAdd<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TArg4, TResult> GetOrAdd<TArg1, TArg2, TArg3, TArg4, TResult>(Func<TArg1, TArg2, TArg3, TArg4, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TArg3, TResult> GetOrAdd<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TArg2, TResult> GetOrAdd<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> funcToMemoize, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Func<TArg1, TResult> GetOrAdd<TArg1, TResult>(Func<TArg1, TResult> funcToMemoize, string path = null, int line = 0)` |  |

### Namespace `System`

[`ALaw`](#alaw) · [`ActionExtensions`](#actionextensions) · [`AppDomainExtensions`](#appdomainextensions) · [`ArrayExtensions`](#arrayextensions) · [`ArrayExtensions.ArraySlice<TItem>`](#arrayextensionsarrayslicetitem) · [`ArrayExtensions.ChangeType`](#arrayextensionschangetype) · [`ArrayExtensions.IChangeSet<TItem>`](#arrayextensionsichangesettitem) · [`ArrayExtensions.ReadOnlyArraySlice<TItem>`](#arrayextensionsreadonlyarrayslicetitem) · [`BFloat16`](#bfloat16) · [`BFloat32`](#bfloat32) · [`BFloat64`](#bfloat64) · [`BFloat8`](#bfloat8) · [`BitConverterExtension`](#bitconverterextension) · [`BoolExtensions`](#boolextensions) · [`ByteExtensions`](#byteextensions) · [`CharExtensions`](#charextensions) · [`ConfigurableFixedPoint<TStorage>`](#configurablefixedpointtstorage) · [`ConfigurableFloatingPoint<TStorage>`](#configurablefloatingpointtstorage) · [`ConsoleExtensions`](#consoleextensions) · [`ConvertExtensions`](#convertextensions) · [`DateTimeExtensions`](#datetimeextensions) · [`Decimal128`](#decimal128) · [`Decimal16`](#decimal16) · [`Decimal32`](#decimal32) · [`Decimal64`](#decimal64) · [`Decimal8`](#decimal8) · [`DosDateTime`](#dosdatetime) · [`E2M1`](#e2m1) · [`E2M1Codec`](#e2m1codec) · [`E4M3`](#e4m3) · [`E8M0`](#e8m0) · [`EnumExtensions`](#enumextensions) · [`FastLazy<TValue>`](#fastlazytvalue) · [`FileTime`](#filetime) · [`FunctionExtensions`](#functionextensions) · [`GpsTime`](#gpstime) · [`Gray16`](#gray16) · [`Gray32`](#gray32) · [`Gray64`](#gray64) · [`Gray8`](#gray8) · [`HfsPlusDate`](#hfsplusdate) · [`IBitCodec<T>`](#ibitcodect) · [`IG711Convention`](#ig711convention) · [`IbmFloat32`](#ibmfloat32) · [`IndexedProperty<TIndexer, TIndexer2, TIndexer3, TResult>`](#indexedpropertytindexer-tindexer2-tindexer3-tresult) · [`IndexedProperty<TIndexer, TIndexer2, TResult>`](#indexedpropertytindexer-tindexer2-tresult) · [`IndexedProperty<TIndexer, TResult>`](#indexedpropertytindexer-tresult) · [`Int16Extensions`](#int16extensions) · [`Int32Extensions`](#int32extensions) · [`Int64Extensions`](#int64extensions) · [`Int96`](#int96) · [`ItuG711`](#itug711) · [`MBF32`](#mbf32) · [`MBF64`](#mbf64) · [`MXFP4`](#mxfp4) · [`MathEx`](#mathex) · [`MidiNote`](#midinote) · [`MuLaw`](#mulaw) · [`NVFP4`](#nvfp4) · [`NtpTimestamp`](#ntptimestamp) · [`NullableEx<TType>`](#nullableexttype) · [`NullableExtensions`](#nullableextensions) · [`ObjectExtensions`](#objectextensions) · [`OleDate`](#oledate) · [`PackedBCD16`](#packedbcd16) · [`PackedBCD32`](#packedbcd32) · [`PackedBCD64`](#packedbcd64) · [`PackedBCD8`](#packedbcd8) · [`Posit16`](#posit16) · [`Posit32`](#posit32) · [`Posit8`](#posit8) · [`Q15_16`](#q15_16) · [`Q31_32`](#q31_32) · [`Q3_4`](#q3_4) · [`Q7_8`](#q7_8) · [`Quarter`](#quarter) · [`RandomExtensions`](#randomextensions) · [`RandomExtensions.PasswordSettings`](#randomextensionspasswordsettings) · [`RangeExtensions`](#rangeextensions) · [`ReadOnlyIndexedProperty<TIndexer, TResult>`](#readonlyindexedpropertytindexer-tresult) · [`RealtimeProperty<TType>`](#realtimepropertyttype) · [`SByteExtensions`](#sbyteextensions) · [`SignedBitCodec`](#signedbitcodec) · [`SlowProperty<TValue, TIntermediateValue>`](#slowpropertytvalue-tintermediatevalue) · [`SlowProperty<TValue>`](#slowpropertytvalue) · [`SpanExtensions`](#spanextensions) · [`StaticMethodLocal`](#staticmethodlocal) · [`StaticMethodLocal.Storage<T>`](#staticmethodlocalstoraget) · [`StaticMethodLocal<TValue>`](#staticmethodlocaltvalue) · [`StringExtensions`](#stringextensions) · [`StringExtensions.CaseComparison`](#stringextensionscasecomparison) · [`StringExtensions.HostEndPoint`](#stringextensionshostendpoint) · [`StringExtensions.LineBreakMode`](#stringextensionslinebreakmode) · [`StringExtensions.LineJoinMode`](#stringextensionslinejoinmode) · [`StringExtensions.TextAnalyzer`](#stringextensionstextanalyzer) · [`StringExtensions.TextAnalyzer.ReadabilityScoreCalculator`](#stringextensionstextanalyzerreadabilityscorecalculator) · [`StringExtensions.TruncateMode`](#stringextensionstruncatemode) · [`SunG711`](#sung711) · [`TF32`](#tf32) · [`TimeSpanExtensions`](#timespanextensions) · [`TypeExtensions`](#typeextensions) · [`TypeExtensions.PropertyDesignerDetails`](#typeextensionspropertydesignerdetails) · [`UInt16Extensions`](#uint16extensions) · [`UInt32Extensions`](#uint32extensions) · [`UInt64Extensions`](#uint64extensions) · [`UInt96`](#uint96) · [`UQ16_16`](#uq16_16) · [`UQ32_32`](#uq32_32) · [`UQ4_4`](#uq4_4) · [`UQ8_8`](#uq8_8) · [`UnixTime32`](#unixtime32) · [`UnixTime64`](#unixtime64) · [`UnpackedBCD`](#unpackedbcd) · [`UnsignedBitCodec`](#unsignedbitcodec) · [`UnsignedDecimal`](#unsigneddecimal) · [`UnsignedDouble`](#unsigneddouble) · [`UnsignedFloat`](#unsignedfloat) · [`UriExtensions`](#uriextensions) · [`VaxFloat`](#vaxfloat) · [`WebKitTime`](#webkittime) · [`WriteOnlyIndexedProperty<TIndexer, TResult>`](#writeonlyindexedpropertytindexer-tresult) · [`ZigZag16`](#zigzag16) · [`ZigZag32`](#zigzag32) · [`ZigZag64`](#zigzag64) · [`ZigZag8`](#zigzag8) · [`__ClassForcingTag<T>`](#__classforcingtagt) · [`__StructForcingTag<T>`](#__structforcingtagt)

#### `ALaw`

Implements `IComparable`, `IComparable<ALaw>`, `IEquatable<ALaw>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `byte RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(ALaw other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(ALaw other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromPcm16` | `static ALaw FromPcm16(short pcm)` |  |
| `FromPcm16` | `static ALaw FromPcm16<TConvention>(short pcm)` |  |
| `FromRaw` | `static ALaw FromRaw(byte raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToPcm16` | `short ToPcm16()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator ALaw` | `static explicit operator ALaw(short pcm)` |  |
| `implicit operator short` | `static implicit operator short(ALaw value)` |  |
| `operator !=` | `static bool operator !=(ALaw left, ALaw right)` |  |
| `operator ==` | `static bool operator ==(ALaw left, ALaw right)` |  |

#### `ActionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Async` | `static IAsyncResult Async(this Action @this, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2, object state = null)` |  |
| `Async` | `static IAsyncResult Async<T1>(this Action<T1> @this, T1 arg1, object state = null)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke(this Action @this, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2, AsyncCallback callback)` |  |
| `BeginInvoke` | `static IAsyncResult BeginInvoke<T1>(this Action<T1> @this, T1 arg1, AsyncCallback callback)` |  |
| `ExecuteInHours` | `static void ExecuteInHours(this Action @this, double hours)` |  |
| `ExecuteInMilliseconds` | `static void ExecuteInMilliseconds(this Action @this, double milliseconds)` |  |
| `ExecuteInMinutes` | `static void ExecuteInMinutes(this Action @this, double minutes)` |  |
| `ExecuteInSeconds` | `static void ExecuteInSeconds(this Action @this, double seconds)` |  |
| `ExecuteIn` | `static void ExecuteIn(this Action @this, TimeSpan timespan)` |  |
| `RetryOnException` | `static void RetryOnException(this Action @this, int repeatCount, TimeSpan? dueTime = null)` |  |
| `TryInvoke` | `static bool TryInvoke(this Action @this, int repeatCount = 1)` |  |

#### `AppDomainExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `BasePath` | `static DirectoryInfo BasePath { get; }` |  |
| `EnsureSingleInstanceOrExit` | `static void EnsureSingleInstanceOrExit(this AppDomain @this)` |  |
| `EnsureSingleInstanceOrExit` | `static void EnsureSingleInstanceOrExit(this AppDomain @this, string mutexName)` |  |
| `EnsureSingleInstanceOrThrow` | `static void EnsureSingleInstanceOrThrow(this AppDomain @this)` |  |
| `EnsureSingleInstanceOrThrow` | `static void EnsureSingleInstanceOrThrow(this AppDomain @this, string mutexName)` |  |
| `Fork` | `static bool Fork(this AppDomain @this)` |  |
| `GetExecutable` | `static FileInfo GetExecutable(this AppDomain @this)` |  |
| `IsSingleInstance` | `static bool IsSingleInstance(this AppDomain @this)` |  |
| `IsSingleInstance` | `static bool IsSingleInstance(this AppDomain @this, string uniqueName)` |  |

#### `ArrayExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Aggregate` | `static TAccumulate Aggregate<TItem, TAccumulate>(this TItem[] @this, TAccumulate seed, Func<TAccumulate, TItem, TAccumulate> func)` |  |
| `Aggregate` | `static TItem Aggregate<TItem>(this TItem[] @this, Func<TItem, TItem, TItem> func)` |  |
| `And` | `static void And(this byte[] @this, byte[] operand)` |  |
| `And` | `static void And(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `And` | `static void And(this uint[] @this, byte operand, int count)` |  |
| `And` | `static void And(this uint[] @this, byte operand, int offset, int count)` |  |
| `And` | `static void And(this uint[] @this, ushort operand, int count)` |  |
| `And` | `static void And(this uint[] @this, ushort operand, int offset, int count)` |  |
| `And` | `static void And(this ulong[] @this, byte operand, int count)` |  |
| `And` | `static void And(this ulong[] @this, byte operand, int offset, int count)` |  |
| `And` | `static void And(this ulong[] @this, uint operand, int count)` |  |
| `And` | `static void And(this ulong[] @this, uint operand, int offset, int count)` |  |
| `And` | `static void And(this ulong[] @this, ushort operand, int count)` |  |
| `And` | `static void And(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `And` | `static void And(this ushort[] @this, byte operand, int count)` |  |
| `And` | `static void And(this ushort[] @this, byte operand, int offset, int count)` |  |
| `Any` | `static bool Any<TItem>(this TItem[] @this)` |  |
| `Any` | `static bool Any<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `Cast` | `static IEnumerable<TResult> Cast<TResult>(this Array @this)` |  |
| `Clear` | `static void Clear(this byte[] @this)` |  |
| `Clear` | `static void Clear(this double[] @this)` |  |
| `Clear` | `static void Clear(this float[] @this)` |  |
| `Clear` | `static void Clear(this int[] @this)` |  |
| `Clear` | `static void Clear(this long[] @this)` |  |
| `Clear` | `static void Clear(this nint @this, int count)` |  |
| `Clear` | `static void Clear(this short[] @this)` |  |
| `Clear` | `static void Clear(this uint[] @this)` |  |
| `Clear` | `static void Clear(this ulong[] @this)` |  |
| `Clear` | `static void Clear(this ushort[] @this)` |  |
| `CompareTo` | `static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this TItem[] @this, TItem[] other, IEqualityComparer<TItem> comparer = null)` |  |
| `ComputeMD5Hash` | `static byte[] ComputeMD5Hash(this byte[] @this)` |  |
| `ComputeSHA1Hash` | `static byte[] ComputeSHA1Hash(this byte[] @this)` |  |
| `ComputeSHA256Hash` | `static byte[] ComputeSHA256Hash(this byte[] @this)` |  |
| `ComputeSHA384Hash` | `static byte[] ComputeSHA384Hash(this byte[] @this)` |  |
| `ComputeSHA512Hash` | `static byte[] ComputeSHA512Hash(this byte[] @this)` |  |
| `Contains` | `static bool Contains(this Array @this, object value)` |  |
| `Contains` | `static bool Contains<TItem>(this TItem[] @this, TItem value)` |  |
| `ConvertAll` | `static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Converter<TItem, TOutput> converter)` |  |
| `ConvertAll` | `static TOutput[] ConvertAll<TItem, TOutput>(this TItem[] @this, Func<TItem, int, TOutput> converter)` |  |
| `CopyTo` | `static void CopyTo(byte* @this, int count, byte* target)` |  |
| `CopyTo` | `static void CopyTo(byte* @this, int count, byte* target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(byte* @this, int count, int srcIndex, byte* target)` |  |
| `CopyTo` | `static void CopyTo(byte* @this, int srcIndex, byte* target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this bool[] @this, bool[] target)` |  |
| `CopyTo` | `static void CopyTo(this bool[] @this, bool[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this bool[] @this, int srcIndex, bool[] target)` |  |
| `CopyTo` | `static void CopyTo(this bool[] @this, int srcIndex, bool[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, byte[] target)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, byte[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, byte[] target)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this byte[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, char[] target)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, char[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, char[] target)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this char[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this decimal[] @this, decimal[] target)` |  |
| `CopyTo` | `static void CopyTo(this decimal[] @this, decimal[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this decimal[] @this, int srcIndex, decimal[] target)` |  |
| `CopyTo` | `static void CopyTo(this decimal[] @this, int srcIndex, decimal[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, double[] target)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, double[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, double[] target)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this double[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, float[] target)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, float[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, float[] target)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this float[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, int[] target)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int[] target)` |  |
| `CopyTo` | `static void CopyTo(this int[] @this, int[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, long[] target)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, long[] target)` |  |
| `CopyTo` | `static void CopyTo(this long[] @this, long[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this nint @this, int count, int srcIndex, nint target)` |  |
| `CopyTo` | `static void CopyTo(this nint @this, int count, nint target)` |  |
| `CopyTo` | `static void CopyTo(this nint @this, int count, nint target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this nint @this, int srcIndex, nint target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, sbyte[] target)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, sbyte[] target)` |  |
| `CopyTo` | `static void CopyTo(this sbyte[] @this, sbyte[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, short[] target)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, short[] target)` |  |
| `CopyTo` | `static void CopyTo(this short[] @this, short[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this string[] @this, int srcIndex, string[] target)` |  |
| `CopyTo` | `static void CopyTo(this string[] @this, int srcIndex, string[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this string[] @this, string[] target)` |  |
| `CopyTo` | `static void CopyTo(this string[] @this, string[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, uint[] target)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, uint[] target)` |  |
| `CopyTo` | `static void CopyTo(this uint[] @this, uint[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, ulong[] target)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, ulong[] target)` |  |
| `CopyTo` | `static void CopyTo(this ulong[] @this, ulong[] target, int tgtIndex)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, byte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, char[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, double[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, float[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, int[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, long[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, sbyte[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, short[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, uint[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, ulong[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, ushort[] target)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, int srcIndex, ushort[] target, int tgtIndex, int count)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, ushort[] target)` |  |
| `CopyTo` | `static void CopyTo(this ushort[] @this, ushort[] target, int tgtIndex)` |  |
| `Copy` | `static bool[] Copy(this bool[] @this)` |  |
| `Copy` | `static bool[] Copy(this bool[] @this, int index)` |  |
| `Copy` | `static bool[] Copy(this bool[] @this, int index, int count)` |  |
| `Copy` | `static byte[] Copy(this byte[] @this)` |  |
| `Copy` | `static byte[] Copy(this byte[] @this, int index)` |  |
| `Copy` | `static byte[] Copy(this byte[] @this, int index, int count)` |  |
| `Copy` | `static char[] Copy(this char[] @this)` |  |
| `Copy` | `static char[] Copy(this char[] @this, int index)` |  |
| `Copy` | `static char[] Copy(this char[] @this, int index, int count)` |  |
| `Copy` | `static decimal[] Copy(this decimal[] @this)` |  |
| `Copy` | `static decimal[] Copy(this decimal[] @this, int index)` |  |
| `Copy` | `static decimal[] Copy(this decimal[] @this, int index, int count)` |  |
| `Copy` | `static double[] Copy(this double[] @this)` |  |
| `Copy` | `static double[] Copy(this double[] @this, int index)` |  |
| `Copy` | `static double[] Copy(this double[] @this, int index, int count)` |  |
| `Copy` | `static float[] Copy(this float[] @this)` |  |
| `Copy` | `static float[] Copy(this float[] @this, int index)` |  |
| `Copy` | `static float[] Copy(this float[] @this, int index, int count)` |  |
| `Copy` | `static int[] Copy(this int[] @this)` |  |
| `Copy` | `static int[] Copy(this int[] @this, int index)` |  |
| `Copy` | `static int[] Copy(this int[] @this, int index, int count)` |  |
| `Copy` | `static long[] Copy(this long[] @this)` |  |
| `Copy` | `static long[] Copy(this long[] @this, int index)` |  |
| `Copy` | `static long[] Copy(this long[] @this, int index, int count)` |  |
| `Copy` | `static sbyte[] Copy(this sbyte[] @this)` |  |
| `Copy` | `static sbyte[] Copy(this sbyte[] @this, int index)` |  |
| `Copy` | `static sbyte[] Copy(this sbyte[] @this, int index, int count)` |  |
| `Copy` | `static short[] Copy(this short[] @this)` |  |
| `Copy` | `static short[] Copy(this short[] @this, int index)` |  |
| `Copy` | `static short[] Copy(this short[] @this, int index, int count)` |  |
| `Copy` | `static string[] Copy(this string[] @this)` |  |
| `Copy` | `static string[] Copy(this string[] @this, int index)` |  |
| `Copy` | `static string[] Copy(this string[] @this, int index, int count)` |  |
| `Copy` | `static uint[] Copy(this uint[] @this)` |  |
| `Copy` | `static uint[] Copy(this uint[] @this, int index)` |  |
| `Copy` | `static uint[] Copy(this uint[] @this, int index, int count)` |  |
| `Copy` | `static ulong[] Copy(this ulong[] @this)` |  |
| `Copy` | `static ulong[] Copy(this ulong[] @this, int index)` |  |
| `Copy` | `static ulong[] Copy(this ulong[] @this, int index, int count)` |  |
| `Copy` | `static ushort[] Copy(this ushort[] @this)` |  |
| `Copy` | `static ushort[] Copy(this ushort[] @this, int index)` |  |
| `Copy` | `static ushort[] Copy(this ushort[] @this, int index, int count)` |  |
| `Count` | `static int Count<TItem>(this TItem[] @this)` |  |
| `Count` | `static int Count<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `CreatedJaggedArray` | `static TArray CreatedJaggedArray<TArray>(params int[] lengths)` |  |
| `Equ` | `static void Equ(this byte[] @this, byte[] operand)` |  |
| `Equ` | `static void Equ(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `Equ` | `static void Equ(this uint[] @this, byte operand, int count)` |  |
| `Equ` | `static void Equ(this uint[] @this, byte operand, int offset, int count)` |  |
| `Equ` | `static void Equ(this uint[] @this, ushort operand, int count)` |  |
| `Equ` | `static void Equ(this uint[] @this, ushort operand, int offset, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, byte operand, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, byte operand, int offset, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, uint operand, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, uint operand, int offset, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, ushort operand, int count)` |  |
| `Equ` | `static void Equ(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `Equ` | `static void Equ(this ushort[] @this, byte operand, int count)` |  |
| `Equ` | `static void Equ(this ushort[] @this, byte operand, int offset, int count)` |  |
| `Exists` | `static bool Exists<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `Fill` | `static void Fill(this byte[] @this, byte value)` |  |
| `Fill` | `static void Fill(this byte[] @this, byte value, int offset)` |  |
| `Fill` | `static void Fill(this byte[] @this, byte value, int offset, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, uint value, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, uint value, int offset, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, ulong value, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, ulong value, int offset, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, ushort value, int count)` |  |
| `Fill` | `static void Fill(this byte[] @this, ushort value, int offset, int count)` |  |
| `Fill` | `static void Fill(this nint @this, byte value, int count)` |  |
| `Fill` | `static void Fill(this nint @this, byte value, int offset, int count)` |  |
| `Fill` | `static void Fill(this nint @this, uint value, int count)` |  |
| `Fill` | `static void Fill(this nint @this, uint value, int offset, int count)` |  |
| `Fill` | `static void Fill(this nint @this, ulong value, int count)` |  |
| `Fill` | `static void Fill(this nint @this, ulong value, int offset, int count)` |  |
| `Fill` | `static void Fill(this nint @this, ushort value, int count)` |  |
| `Fill` | `static void Fill(this nint @this, ushort value, int offset, int count)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this Array @this, Predicate<TItem> predicate, TItem defaultValue = null)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this TItem[] @this)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, TItem defaultValue)` |  |
| `FirstOrDefault` | `static object FirstOrDefault(this Array @this, Predicate<object> predicate, object defaultValue = null)` |  |
| `First` | `static TItem First<TItem>(this TItem[] @this)` |  |
| `First` | `static TItem First<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Action<TItem, int> action)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Action<TItem, long> action)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Action<TItem> action)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Func<TItem, TItem> worker)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Func<TItem, int, TItem> worker)` |  |
| `ForEach` | `static void ForEach<TItem>(this TItem[] @this, Func<TItem, long, TItem> worker)` |  |
| `GZip` | `static byte[] GZip(this byte[] @this)` |  |
| `GetRandomElement` | `static TItem GetRandomElement<TItem>(this TItem[] @this, Random random = null)` |  |
| `GetValueOrDefault` | `static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index)` |  |
| `GetValueOrDefault` | `static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<TItem> factory)` |  |
| `GetValueOrDefault` | `static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, Func<int, TItem> factory)` |  |
| `GetValueOrDefault` | `static TItem GetValueOrDefault<TItem>(this TItem[] @this, int index, TItem defaultValue)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], byte[], int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<byte[], int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, Func<int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int defaultValue)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], byte[], int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<byte[], int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, Func<int> defaultValueFunc)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault(this byte[] @this, byte[] searchString, int offset, int defaultValue)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<TItem[], int> defaultValueFactory)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, Func<int> defaultValueFactory)` |  |
| `IndexOfOrDefault` | `static int IndexOfOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate, int defaultValue)` |  |
| `IndexOfOrMinusOne` | `static int IndexOfOrMinusOne(this byte[] @this, byte[] searchString, int offset = 0)` |  |
| `IndexOf` | `static int IndexOf(this Array @this, Predicate<object> predicate)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this TItem[] @this, TItem value)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this TItem[] @this, TItem value, IEqualityComparer<TItem> comparer)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this TItem[] @this, TItem value, int offset)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this TItem[] @this, TItem value, int offset, IEqualityComparer<TItem> comparer)` |  |
| `IsMultiple` | `static bool IsMultiple<TValue>(this TValue[] @this)` |  |
| `IsNoMultiple` | `static bool IsNoMultiple<TValue>(this TValue[] @this)` |  |
| `IsNoSingle` | `static bool IsNoSingle<TValue>(this TValue[] @this)` |  |
| `IsNotNullOrEmpty` | `static bool IsNotNullOrEmpty<TItem>(this TItem[] @this)` |  |
| `IsNullOrEmpty` | `static bool IsNullOrEmpty<TItem>(this TItem[] @this)` |  |
| `IsSingle` | `static bool IsSingle<TValue>(this TValue[] @this)` |  |
| `Join` | `static string Join<TItem>(this TItem[] @this, string join = ", ", bool skipDefaults = false, Func<TItem, string> converter = null)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this TItem[] @this)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `Last` | `static TItem Last<TItem>(this TItem[] @this)` |  |
| `Last` | `static TItem Last<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `LongCount` | `static long LongCount<TItem>(this TItem[] @this)` |  |
| `LongCount` | `static long LongCount<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `Nand` | `static void Nand(this byte[] @this, byte[] operand)` |  |
| `Nand` | `static void Nand(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `Nand` | `static void Nand(this uint[] @this, byte operand, int count)` |  |
| `Nand` | `static void Nand(this uint[] @this, byte operand, int offset, int count)` |  |
| `Nand` | `static void Nand(this uint[] @this, ushort operand, int count)` |  |
| `Nand` | `static void Nand(this uint[] @this, ushort operand, int offset, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, byte operand, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, byte operand, int offset, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, uint operand, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, uint operand, int offset, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, ushort operand, int count)` |  |
| `Nand` | `static void Nand(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `Nand` | `static void Nand(this ushort[] @this, byte operand, int count)` |  |
| `Nand` | `static void Nand(this ushort[] @this, byte operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this byte[] @this, byte[] operand)` |  |
| `Nor` | `static void Nor(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `Nor` | `static void Nor(this uint[] @this, byte operand, int count)` |  |
| `Nor` | `static void Nor(this uint[] @this, byte operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this uint[] @this, ushort operand, int count)` |  |
| `Nor` | `static void Nor(this uint[] @this, ushort operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, byte operand, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, byte operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, uint operand, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, uint operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, ushort operand, int count)` |  |
| `Nor` | `static void Nor(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `Nor` | `static void Nor(this ushort[] @this, byte operand, int count)` |  |
| `Nor` | `static void Nor(this ushort[] @this, byte operand, int offset, int count)` |  |
| `Not` | `static void Not(this byte[] @this)` |  |
| `Not` | `static void Not(this byte[] @this, int offset, int count)` |  |
| `OfType` | `static IEnumerable<TResult> OfType<TResult>(this Array @this)` |  |
| `Or` | `static void Or(this byte[] @this, byte[] operand)` |  |
| `Or` | `static void Or(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `Or` | `static void Or(this uint[] @this, byte operand, int count)` |  |
| `Or` | `static void Or(this uint[] @this, byte operand, int offset, int count)` |  |
| `Or` | `static void Or(this uint[] @this, ushort operand, int count)` |  |
| `Or` | `static void Or(this uint[] @this, ushort operand, int offset, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, byte operand, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, byte operand, int offset, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, uint operand, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, uint operand, int offset, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, ushort operand, int count)` |  |
| `Or` | `static void Or(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `Or` | `static void Or(this ushort[] @this, byte operand, int count)` |  |
| `Or` | `static void Or(this ushort[] @this, byte operand, int offset, int count)` |  |
| `Padd` | `static byte[] Padd(this byte[] @this, int length, byte data = 0)` |  |
| `ParallelForEach` | `static void ParallelForEach<TItem>(this TItem[] @this, Action<TItem> action)` |  |
| `ProcessInChunks` | `static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor)` |  |
| `ProcessInChunks` | `static void ProcessInChunks<TItem>(this TItem[] @this, int chunkSize, Action<TItem[], int, int> processor, int length, int offset = 0)` |  |
| `QuickSort` | `static void QuickSort<TItem>(this TItem[] @this)` |  |
| `QuickSorted` | `static TItem[] QuickSorted<TItem>(this TItem[] @this)` |  |
| `RandomizeBuffer` | `static void RandomizeBuffer(this byte[] @this)` |  |
| `Range` | `static TItem[] Range<TItem>(this TItem[] @this, int startIndex, int count)` |  |
| `Range` | `static byte[] Range(this byte[] @this, int offset, int count)` |  |
| `ReadOnlySlice` | `static ReadOnlySpan<TItem> ReadOnlySlice<TItem>(this TItem[] @this, int start, int length = -1)` |  |
| `ReadOnlySlices` | `static IEnumerable<ReadOnlyArraySlice<TItem>> ReadOnlySlices<TItem>(this TItem[] @this, int size)` |  |
| `Reverse` | `static IEnumerable<object> Reverse(this Array @this)` |  |
| `Reverse` | `static TItem[] Reverse<TItem>(this TItem[] @this)` |  |
| `RotateTowardsZero` | `static void RotateTowardsZero<TItem>(this TItem[] @this)` |  |
| `SafelyClone` | `static TItem[] SafelyClone<TItem>(this TItem[] @this)` |  |
| `SelectLong` | `static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector)` |  |
| `SelectLong` | `static IEnumerable<TResult> SelectLong<TItem, TResult>(this TItem[] @this, Func<TItem, long, TResult> selector)` |  |
| `Select` | `static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, TResult> selector)` |  |
| `Select` | `static IEnumerable<TResult> Select<TItem, TResult>(this TItem[] @this, Func<TItem, int, TResult> selector)` |  |
| `SequenceEqual` | `static bool SequenceEqual(this byte[] source, byte[] target)` |  |
| `SequenceEqual` | `static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset)` |  |
| `SequenceEqual` | `static bool SequenceEqual(this byte[] source, int sourceOffset, byte[] target, int targetOffset, int count)` |  |
| `Shuffle` | `static void Shuffle<TItem>(this TItem[] @this, Random entropySource = null)` |  |
| `Slice` | `static Span<TItem> Slice<TItem>(this TItem[] @this, int start, int length = -1)` |  |
| `Slices` | `static IEnumerable<ArraySlice<TItem>> Slices<TItem>(this TItem[] @this, int size)` |  |
| `Swap` | `static void Swap<TItem>(this TItem[] @this, int firstElementIndex, int secondElementIndex)` |  |
| `ToArray` | `static object[] ToArray(this Array @this)` |  |
| `ToBin` | `static string ToBin(this byte[] @this)` |  |
| `ToHex` | `static string ToHex(this byte[] @this, bool allUpperCase = false)` |  |
| `ToNullIfEmpty` | `static TItem[] ToNullIfEmpty<TItem>(this TItem[] @this)` |  |
| `ToStringInstance` | `static string ToStringInstance(this char[] @this)` |  |
| `ToStringInstance` | `static string ToStringInstance(this char[] @this, int startIndex)` |  |
| `ToStringInstance` | `static string ToStringInstance(this char[] @this, int startIndex, int length)` |  |
| `TryGetFirst` | `static bool TryGetFirst<T>(this T[] @this, out T result)` |  |
| `TryGetItem` | `static bool TryGetItem<T>(this T[] @this, int index, out T result)` |  |
| `TryGetLast` | `static bool TryGetLast<T>(this T[] @this, out T result)` |  |
| `TrySetFirst` | `static bool TrySetFirst<T>(this T[] @this, T value)` |  |
| `TrySetItem` | `static bool TrySetItem<T>(this T[] @this, int index, T value)` |  |
| `TrySetLast` | `static bool TrySetLast<T>(this T[] @this, T value)` |  |
| `UnGZip` | `static byte[] UnGZip(this byte[] @this)` |  |
| `WhereLong` | `static IEnumerable<TItem> WhereLong<TItem>(this TItem[] @this, Func<TItem, long, bool> predicate)` |  |
| `Where` | `static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Func<TItem, int, bool> predicate)` |  |
| `Where` | `static IEnumerable<TItem> Where<TItem>(this TItem[] @this, Predicate<TItem> predicate)` |  |
| `Xor` | `static void Xor(this byte[] @this, byte[] operand)` |  |
| `Xor` | `static void Xor(this byte[] @this, int offset, byte[] operand, int operandOffset, int count)` |  |
| `Xor` | `static void Xor(this uint[] @this, byte operand, int count)` |  |
| `Xor` | `static void Xor(this uint[] @this, byte operand, int offset, int count)` |  |
| `Xor` | `static void Xor(this uint[] @this, ushort operand, int count)` |  |
| `Xor` | `static void Xor(this uint[] @this, ushort operand, int offset, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, byte operand, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, byte operand, int offset, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, uint operand, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, uint operand, int offset, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, ushort operand, int count)` |  |
| `Xor` | `static void Xor(this ulong[] @this, ushort operand, int offset, int count)` |  |
| `Xor` | `static void Xor(this ushort[] @this, byte operand, int count)` |  |
| `Xor` | `static void Xor(this ushort[] @this, byte operand, int offset, int count)` |  |

#### `ArrayExtensions.ArraySlice<TItem>`

Inherits `ReadOnlyArraySlice<TItem>`. Implements `IEnumerable`, `IEnumerable<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ArraySlice` | `ArraySlice(TItem[] source, int start, int length)` |  |
| `Item` | `ArraySlice<TItem> this[Range range] { get; }` |  |
| `Item` | `TItem this[Index index] { get; set; }` |  |
| `Item` | `TItem this[int index] { get; set; }` |  |
| `Slice` | `ArraySlice<TItem> Slice(int start, int length = -1)` |  |

#### `ArrayExtensions.ChangeType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Equal` | `0` |  |
| `Changed` | `1` |  |
| `Added` | `2` |  |
| `Removed` | `3` |  |

#### `ArrayExtensions.IChangeSet<TItem>`

| Member | Signature | Summary |
| --- | --- | --- |
| `CurrentIndex` | `int CurrentIndex { get; }` |  |
| `Current` | `TItem Current { get; }` |  |
| `OtherIndex` | `int OtherIndex { get; }` |  |
| `Other` | `TItem Other { get; }` |  |
| `Type` | `ChangeType Type { get; }` |  |

#### `ArrayExtensions.ReadOnlyArraySlice<TItem>`

Implements `IEnumerable`, `IEnumerable<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ReadOnlyArraySlice` | `ReadOnlyArraySlice(TItem[] source, int start, int length)` |  |
| `_source` | `protected readonly TItem[] _source` |  |
| `_start` | `protected readonly int _start` |  |
| `Item` | `ReadOnlyArraySlice<TItem> this[Range range] { get; }` |  |
| `Item` | `TItem this[Index index] { get; }` |  |
| `Item` | `TItem this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `Values` | `IEnumerable<TItem> Values { get; }` |  |
| `GetEnumerator` | `IEnumerator<TItem> GetEnumerator()` |  |
| `ReadOnlySlice` | `ReadOnlyArraySlice<TItem> ReadOnlySlice(int start, int length = -1)` |  |
| `ToArray` | `TItem[] ToArray()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator TItem[]` | `static explicit operator TItem[](ReadOnlyArraySlice<TItem> @this)` |  |

#### `BFloat16`

Implements `IComparable`, `IComparable<BFloat16>`, `IEquatable<BFloat16>`, `IFormattable`, `IParsable<BFloat16>`, `ISpanFormattable`, `ISpanParsable<BFloat16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static BFloat16 Epsilon { get; }` |  |
| `MaxValue` | `static BFloat16 MaxValue { get; }` |  |
| `MinValue` | `static BFloat16 MinValue { get; }` |  |
| `NaN` | `static BFloat16 NaN { get; }` |  |
| `NegativeInfinity` | `static BFloat16 NegativeInfinity { get; }` |  |
| `One` | `static BFloat16 One { get; }` |  |
| `PositiveInfinity` | `static BFloat16 PositiveInfinity { get; }` |  |
| `RawValue` | `ushort RawValue { get; }` |  |
| `Zero` | `static BFloat16 Zero { get; }` |  |
| `Abs` | `static BFloat16 Abs(BFloat16 value)` |  |
| `CompareTo` | `int CompareTo(BFloat16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static BFloat16 CopySign(BFloat16 value, BFloat16 sign)` |  |
| `Equals` | `bool Equals(BFloat16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static BFloat16 FromDouble(double value)` |  |
| `FromRaw` | `static BFloat16 FromRaw(ushort raw)` |  |
| `FromSingle` | `static BFloat16 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(BFloat16 value)` |  |
| `IsInfinity` | `static bool IsInfinity(BFloat16 value)` |  |
| `IsNaN` | `static bool IsNaN(BFloat16 value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(BFloat16 value)` |  |
| `IsNegative` | `static bool IsNegative(BFloat16 value)` |  |
| `IsNormal` | `static bool IsNormal(BFloat16 value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(BFloat16 value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(BFloat16 value)` |  |
| `Max` | `static BFloat16 Max(BFloat16 left, BFloat16 right)` |  |
| `Min` | `static BFloat16 Min(BFloat16 left, BFloat16 right)` |  |
| `Parse` | `static BFloat16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat16 Parse(string s)` |  |
| `Parse` | `static BFloat16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out BFloat16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out BFloat16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out BFloat16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out BFloat16 result)` |  |
| `explicit operator BFloat16` | `static explicit operator BFloat16(Half value)` |  |
| `explicit operator BFloat16` | `static explicit operator BFloat16(double value)` |  |
| `explicit operator BFloat16` | `static explicit operator BFloat16(float value)` |  |
| `implicit operator Half` | `static implicit operator Half(BFloat16 value)` |  |
| `implicit operator double` | `static implicit operator double(BFloat16 value)` |  |
| `implicit operator float` | `static implicit operator float(BFloat16 value)` |  |
| `operator !=` | `static bool operator !=(BFloat16 left, BFloat16 right)` |  |
| `operator %` | `static BFloat16 operator %(BFloat16 left, BFloat16 right)` |  |
| `operator *` | `static BFloat16 operator *(BFloat16 left, BFloat16 right)` |  |
| `operator *` | `static BFloat16 operator *(BFloat16 left, float right)` |  |
| `operator *` | `static BFloat16 operator *(BFloat16 left, int right)` |  |
| `operator *` | `static BFloat16 operator *(float left, BFloat16 right)` |  |
| `operator *` | `static BFloat16 operator *(int left, BFloat16 right)` |  |
| `operator ++` | `static BFloat16 operator ++(BFloat16 value)` |  |
| `operator +` | `static BFloat16 operator +(BFloat16 left, BFloat16 right)` |  |
| `operator +` | `static BFloat16 operator +(BFloat16 left, float right)` |  |
| `operator +` | `static BFloat16 operator +(BFloat16 left, int right)` |  |
| `operator +` | `static BFloat16 operator +(BFloat16 value)` |  |
| `operator +` | `static BFloat16 operator +(float left, BFloat16 right)` |  |
| `operator +` | `static BFloat16 operator +(int left, BFloat16 right)` |  |
| `operator --` | `static BFloat16 operator --(BFloat16 value)` |  |
| `operator -` | `static BFloat16 operator -(BFloat16 left, BFloat16 right)` |  |
| `operator -` | `static BFloat16 operator -(BFloat16 left, float right)` |  |
| `operator -` | `static BFloat16 operator -(BFloat16 left, int right)` |  |
| `operator -` | `static BFloat16 operator -(BFloat16 value)` |  |
| `operator -` | `static BFloat16 operator -(float left, BFloat16 right)` |  |
| `operator -` | `static BFloat16 operator -(int left, BFloat16 right)` |  |
| `operator /` | `static BFloat16 operator /(BFloat16 left, BFloat16 right)` |  |
| `operator /` | `static BFloat16 operator /(BFloat16 left, float right)` |  |
| `operator /` | `static BFloat16 operator /(BFloat16 left, int right)` |  |
| `operator /` | `static BFloat16 operator /(float left, BFloat16 right)` |  |
| `operator /` | `static BFloat16 operator /(int left, BFloat16 right)` |  |
| `operator <=` | `static bool operator <=(BFloat16 left, BFloat16 right)` |  |
| `operator <` | `static bool operator <(BFloat16 left, BFloat16 right)` |  |
| `operator ==` | `static bool operator ==(BFloat16 left, BFloat16 right)` |  |
| `operator >=` | `static bool operator >=(BFloat16 left, BFloat16 right)` |  |
| `operator >` | `static bool operator >(BFloat16 left, BFloat16 right)` |  |

#### `BFloat32`

Implements `IComparable`, `IComparable<BFloat32>`, `IEquatable<BFloat32>`, `IFormattable`, `IParsable<BFloat32>`, `ISpanFormattable`, `ISpanParsable<BFloat32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static BFloat32 Epsilon { get; }` |  |
| `MaxValue` | `static BFloat32 MaxValue { get; }` |  |
| `MinValue` | `static BFloat32 MinValue { get; }` |  |
| `NaN` | `static BFloat32 NaN { get; }` |  |
| `NegativeInfinity` | `static BFloat32 NegativeInfinity { get; }` |  |
| `One` | `static BFloat32 One { get; }` |  |
| `PositiveInfinity` | `static BFloat32 PositiveInfinity { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static BFloat32 Zero { get; }` |  |
| `Abs` | `static BFloat32 Abs(BFloat32 value)` |  |
| `CompareTo` | `int CompareTo(BFloat32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static BFloat32 CopySign(BFloat32 value, BFloat32 sign)` |  |
| `Equals` | `bool Equals(BFloat32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static BFloat32 FromDouble(double value)` |  |
| `FromRaw` | `static BFloat32 FromRaw(uint raw)` |  |
| `FromSingle` | `static BFloat32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(BFloat32 value)` |  |
| `IsInfinity` | `static bool IsInfinity(BFloat32 value)` |  |
| `IsNaN` | `static bool IsNaN(BFloat32 value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(BFloat32 value)` |  |
| `IsNegative` | `static bool IsNegative(BFloat32 value)` |  |
| `IsNormal` | `static bool IsNormal(BFloat32 value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(BFloat32 value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(BFloat32 value)` |  |
| `Max` | `static BFloat32 Max(BFloat32 left, BFloat32 right)` |  |
| `Min` | `static BFloat32 Min(BFloat32 left, BFloat32 right)` |  |
| `Parse` | `static BFloat32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat32 Parse(string s)` |  |
| `Parse` | `static BFloat32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out BFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out BFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out BFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out BFloat32 result)` |  |
| `explicit operator BFloat32` | `static explicit operator BFloat32(double value)` |  |
| `explicit operator BFloat32` | `static explicit operator BFloat32(float value)` |  |
| `explicit operator float` | `static explicit operator float(BFloat32 value)` |  |
| `implicit operator double` | `static implicit operator double(BFloat32 value)` |  |
| `operator !=` | `static bool operator !=(BFloat32 left, BFloat32 right)` |  |
| `operator %` | `static BFloat32 operator %(BFloat32 left, BFloat32 right)` |  |
| `operator *` | `static BFloat32 operator *(BFloat32 left, BFloat32 right)` |  |
| `operator *` | `static BFloat32 operator *(BFloat32 left, double right)` |  |
| `operator *` | `static BFloat32 operator *(BFloat32 left, int right)` |  |
| `operator *` | `static BFloat32 operator *(double left, BFloat32 right)` |  |
| `operator *` | `static BFloat32 operator *(int left, BFloat32 right)` |  |
| `operator ++` | `static BFloat32 operator ++(BFloat32 value)` |  |
| `operator +` | `static BFloat32 operator +(BFloat32 left, BFloat32 right)` |  |
| `operator +` | `static BFloat32 operator +(BFloat32 left, double right)` |  |
| `operator +` | `static BFloat32 operator +(BFloat32 left, int right)` |  |
| `operator +` | `static BFloat32 operator +(BFloat32 value)` |  |
| `operator +` | `static BFloat32 operator +(double left, BFloat32 right)` |  |
| `operator +` | `static BFloat32 operator +(int left, BFloat32 right)` |  |
| `operator --` | `static BFloat32 operator --(BFloat32 value)` |  |
| `operator -` | `static BFloat32 operator -(BFloat32 left, BFloat32 right)` |  |
| `operator -` | `static BFloat32 operator -(BFloat32 left, double right)` |  |
| `operator -` | `static BFloat32 operator -(BFloat32 left, int right)` |  |
| `operator -` | `static BFloat32 operator -(BFloat32 value)` |  |
| `operator -` | `static BFloat32 operator -(double left, BFloat32 right)` |  |
| `operator -` | `static BFloat32 operator -(int left, BFloat32 right)` |  |
| `operator /` | `static BFloat32 operator /(BFloat32 left, BFloat32 right)` |  |
| `operator /` | `static BFloat32 operator /(BFloat32 left, double right)` |  |
| `operator /` | `static BFloat32 operator /(BFloat32 left, int right)` |  |
| `operator /` | `static BFloat32 operator /(double left, BFloat32 right)` |  |
| `operator /` | `static BFloat32 operator /(int left, BFloat32 right)` |  |
| `operator <=` | `static bool operator <=(BFloat32 left, BFloat32 right)` |  |
| `operator <` | `static bool operator <(BFloat32 left, BFloat32 right)` |  |
| `operator ==` | `static bool operator ==(BFloat32 left, BFloat32 right)` |  |
| `operator >=` | `static bool operator >=(BFloat32 left, BFloat32 right)` |  |
| `operator >` | `static bool operator >(BFloat32 left, BFloat32 right)` |  |

#### `BFloat64`

Implements `IComparable`, `IComparable<BFloat64>`, `IEquatable<BFloat64>`, `IFormattable`, `IParsable<BFloat64>`, `ISpanFormattable`, `ISpanParsable<BFloat64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static BFloat64 Epsilon { get; }` |  |
| `MaxValue` | `static BFloat64 MaxValue { get; }` |  |
| `MinValue` | `static BFloat64 MinValue { get; }` |  |
| `NaN` | `static BFloat64 NaN { get; }` |  |
| `NegativeInfinity` | `static BFloat64 NegativeInfinity { get; }` |  |
| `One` | `static BFloat64 One { get; }` |  |
| `PositiveInfinity` | `static BFloat64 PositiveInfinity { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Zero` | `static BFloat64 Zero { get; }` |  |
| `Abs` | `static BFloat64 Abs(BFloat64 value)` |  |
| `CompareTo` | `int CompareTo(BFloat64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static BFloat64 CopySign(BFloat64 value, BFloat64 sign)` |  |
| `Equals` | `bool Equals(BFloat64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static BFloat64 FromDouble(double value)` |  |
| `FromRaw` | `static BFloat64 FromRaw(ulong raw)` |  |
| `FromSingle` | `static BFloat64 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(BFloat64 value)` |  |
| `IsInfinity` | `static bool IsInfinity(BFloat64 value)` |  |
| `IsNaN` | `static bool IsNaN(BFloat64 value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(BFloat64 value)` |  |
| `IsNegative` | `static bool IsNegative(BFloat64 value)` |  |
| `IsNormal` | `static bool IsNormal(BFloat64 value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(BFloat64 value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(BFloat64 value)` |  |
| `Max` | `static BFloat64 Max(BFloat64 left, BFloat64 right)` |  |
| `Min` | `static BFloat64 Min(BFloat64 left, BFloat64 right)` |  |
| `Parse` | `static BFloat64 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat64 Parse(string s)` |  |
| `Parse` | `static BFloat64 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat64 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out BFloat64 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out BFloat64 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out BFloat64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out BFloat64 result)` |  |
| `explicit operator BFloat64` | `static explicit operator BFloat64(double value)` |  |
| `explicit operator BFloat64` | `static explicit operator BFloat64(float value)` |  |
| `explicit operator double` | `static explicit operator double(BFloat64 value)` |  |
| `explicit operator float` | `static explicit operator float(BFloat64 value)` |  |
| `operator !=` | `static bool operator !=(BFloat64 left, BFloat64 right)` |  |
| `operator %` | `static BFloat64 operator %(BFloat64 left, BFloat64 right)` |  |
| `operator *` | `static BFloat64 operator *(BFloat64 left, BFloat64 right)` |  |
| `operator *` | `static BFloat64 operator *(BFloat64 left, double right)` |  |
| `operator *` | `static BFloat64 operator *(BFloat64 left, int right)` |  |
| `operator *` | `static BFloat64 operator *(double left, BFloat64 right)` |  |
| `operator *` | `static BFloat64 operator *(int left, BFloat64 right)` |  |
| `operator ++` | `static BFloat64 operator ++(BFloat64 value)` |  |
| `operator +` | `static BFloat64 operator +(BFloat64 left, BFloat64 right)` |  |
| `operator +` | `static BFloat64 operator +(BFloat64 left, double right)` |  |
| `operator +` | `static BFloat64 operator +(BFloat64 left, int right)` |  |
| `operator +` | `static BFloat64 operator +(BFloat64 value)` |  |
| `operator +` | `static BFloat64 operator +(double left, BFloat64 right)` |  |
| `operator +` | `static BFloat64 operator +(int left, BFloat64 right)` |  |
| `operator --` | `static BFloat64 operator --(BFloat64 value)` |  |
| `operator -` | `static BFloat64 operator -(BFloat64 left, BFloat64 right)` |  |
| `operator -` | `static BFloat64 operator -(BFloat64 left, double right)` |  |
| `operator -` | `static BFloat64 operator -(BFloat64 left, int right)` |  |
| `operator -` | `static BFloat64 operator -(BFloat64 value)` |  |
| `operator -` | `static BFloat64 operator -(double left, BFloat64 right)` |  |
| `operator -` | `static BFloat64 operator -(int left, BFloat64 right)` |  |
| `operator /` | `static BFloat64 operator /(BFloat64 left, BFloat64 right)` |  |
| `operator /` | `static BFloat64 operator /(BFloat64 left, double right)` |  |
| `operator /` | `static BFloat64 operator /(BFloat64 left, int right)` |  |
| `operator /` | `static BFloat64 operator /(double left, BFloat64 right)` |  |
| `operator /` | `static BFloat64 operator /(int left, BFloat64 right)` |  |
| `operator <=` | `static bool operator <=(BFloat64 left, BFloat64 right)` |  |
| `operator <` | `static bool operator <(BFloat64 left, BFloat64 right)` |  |
| `operator ==` | `static bool operator ==(BFloat64 left, BFloat64 right)` |  |
| `operator >=` | `static bool operator >=(BFloat64 left, BFloat64 right)` |  |
| `operator >` | `static bool operator >(BFloat64 left, BFloat64 right)` |  |

#### `BFloat8`

Implements `IComparable`, `IComparable<BFloat8>`, `IEquatable<BFloat8>`, `IFormattable`, `IParsable<BFloat8>`, `ISpanFormattable`, `ISpanParsable<BFloat8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static BFloat8 Epsilon { get; }` |  |
| `MaxValue` | `static BFloat8 MaxValue { get; }` |  |
| `MinValue` | `static BFloat8 MinValue { get; }` |  |
| `NaN` | `static BFloat8 NaN { get; }` |  |
| `NegativeInfinity` | `static BFloat8 NegativeInfinity { get; }` |  |
| `One` | `static BFloat8 One { get; }` |  |
| `PositiveInfinity` | `static BFloat8 PositiveInfinity { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static BFloat8 Zero { get; }` |  |
| `Abs` | `static BFloat8 Abs(BFloat8 value)` |  |
| `CompareTo` | `int CompareTo(BFloat8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static BFloat8 CopySign(BFloat8 value, BFloat8 sign)` |  |
| `Equals` | `bool Equals(BFloat8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static BFloat8 FromDouble(double value)` |  |
| `FromHalf` | `static BFloat8 FromHalf(Half value)` |  |
| `FromRaw` | `static BFloat8 FromRaw(byte raw)` |  |
| `FromSingle` | `static BFloat8 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(BFloat8 value)` |  |
| `IsInfinity` | `static bool IsInfinity(BFloat8 value)` |  |
| `IsNaN` | `static bool IsNaN(BFloat8 value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(BFloat8 value)` |  |
| `IsNegative` | `static bool IsNegative(BFloat8 value)` |  |
| `IsNormal` | `static bool IsNormal(BFloat8 value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(BFloat8 value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(BFloat8 value)` |  |
| `Max` | `static BFloat8 Max(BFloat8 left, BFloat8 right)` |  |
| `Min` | `static BFloat8 Min(BFloat8 left, BFloat8 right)` |  |
| `Parse` | `static BFloat8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat8 Parse(string s)` |  |
| `Parse` | `static BFloat8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static BFloat8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToHalf` | `Half ToHalf()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out BFloat8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out BFloat8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out BFloat8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out BFloat8 result)` |  |
| `explicit operator BFloat8` | `static explicit operator BFloat8(Half value)` |  |
| `explicit operator BFloat8` | `static explicit operator BFloat8(double value)` |  |
| `explicit operator BFloat8` | `static explicit operator BFloat8(float value)` |  |
| `implicit operator Half` | `static implicit operator Half(BFloat8 value)` |  |
| `implicit operator double` | `static implicit operator double(BFloat8 value)` |  |
| `implicit operator float` | `static implicit operator float(BFloat8 value)` |  |
| `operator !=` | `static bool operator !=(BFloat8 left, BFloat8 right)` |  |
| `operator %` | `static BFloat8 operator %(BFloat8 left, BFloat8 right)` |  |
| `operator *` | `static BFloat8 operator *(BFloat8 left, BFloat8 right)` |  |
| `operator *` | `static BFloat8 operator *(BFloat8 left, float right)` |  |
| `operator *` | `static BFloat8 operator *(BFloat8 left, int right)` |  |
| `operator *` | `static BFloat8 operator *(float left, BFloat8 right)` |  |
| `operator *` | `static BFloat8 operator *(int left, BFloat8 right)` |  |
| `operator ++` | `static BFloat8 operator ++(BFloat8 value)` |  |
| `operator +` | `static BFloat8 operator +(BFloat8 left, BFloat8 right)` |  |
| `operator +` | `static BFloat8 operator +(BFloat8 left, float right)` |  |
| `operator +` | `static BFloat8 operator +(BFloat8 left, int right)` |  |
| `operator +` | `static BFloat8 operator +(BFloat8 value)` |  |
| `operator +` | `static BFloat8 operator +(float left, BFloat8 right)` |  |
| `operator +` | `static BFloat8 operator +(int left, BFloat8 right)` |  |
| `operator --` | `static BFloat8 operator --(BFloat8 value)` |  |
| `operator -` | `static BFloat8 operator -(BFloat8 left, BFloat8 right)` |  |
| `operator -` | `static BFloat8 operator -(BFloat8 left, float right)` |  |
| `operator -` | `static BFloat8 operator -(BFloat8 left, int right)` |  |
| `operator -` | `static BFloat8 operator -(BFloat8 value)` |  |
| `operator -` | `static BFloat8 operator -(float left, BFloat8 right)` |  |
| `operator -` | `static BFloat8 operator -(int left, BFloat8 right)` |  |
| `operator /` | `static BFloat8 operator /(BFloat8 left, BFloat8 right)` |  |
| `operator /` | `static BFloat8 operator /(BFloat8 left, float right)` |  |
| `operator /` | `static BFloat8 operator /(BFloat8 left, int right)` |  |
| `operator /` | `static BFloat8 operator /(float left, BFloat8 right)` |  |
| `operator /` | `static BFloat8 operator /(int left, BFloat8 right)` |  |
| `operator <=` | `static bool operator <=(BFloat8 left, BFloat8 right)` |  |
| `operator <` | `static bool operator <(BFloat8 left, BFloat8 right)` |  |
| `operator ==` | `static bool operator ==(BFloat8 left, BFloat8 right)` |  |
| `operator >=` | `static bool operator >=(BFloat8 left, BFloat8 right)` |  |
| `operator >` | `static bool operator >(BFloat8 left, BFloat8 right)` |  |

#### `BitConverterExtension`

| Member | Signature | Summary |
| --- | --- | --- |
| `ISNULL` | `const byte ISNULL` |  |
| `NOTNULL` | `const byte NOTNULL` |  |
| `GetBytes` | `static byte[] GetBytes(DateTime dtVal)` |  |
| `GetBytes` | `static byte[] GetBytes(DateTime? dtVal)` |  |
| `GetBytes` | `static byte[] GetBytes(bool boolVal)` |  |
| `GetBytes` | `static byte[] GetBytes(bool? boolVal)` |  |
| `GetBytes` | `static byte[] GetBytes(byte byteVal)` |  |
| `GetBytes` | `static byte[] GetBytes(byte? byteVal)` |  |
| `GetBytes` | `static byte[] GetBytes(char chrVal)` |  |
| `GetBytes` | `static byte[] GetBytes(char? chrVal)` |  |
| `GetBytes` | `static byte[] GetBytes(decimal decVal)` |  |
| `GetBytes` | `static byte[] GetBytes(decimal? decVal)` |  |
| `GetBytes` | `static byte[] GetBytes(double dblVal)` |  |
| `GetBytes` | `static byte[] GetBytes(double? dblVal)` |  |
| `GetBytes` | `static byte[] GetBytes(float fltVal)` |  |
| `GetBytes` | `static byte[] GetBytes(float? fltVal)` |  |
| `GetBytes` | `static byte[] GetBytes(int intVal)` |  |
| `GetBytes` | `static byte[] GetBytes(int? intVal)` |  |
| `GetBytes` | `static byte[] GetBytes(long longVal)` |  |
| `GetBytes` | `static byte[] GetBytes(long? longVal)` |  |
| `GetBytes` | `static byte[] GetBytes(sbyte sbyteVal)` |  |
| `GetBytes` | `static byte[] GetBytes(sbyte? byteVal)` |  |
| `GetBytes` | `static byte[] GetBytes(short shortVal)` |  |
| `GetBytes` | `static byte[] GetBytes(short? shortVal)` |  |
| `GetBytes` | `static byte[] GetBytes(uint dwordVal)` |  |
| `GetBytes` | `static byte[] GetBytes(uint? dwordVal)` |  |
| `GetBytes` | `static byte[] GetBytes(ulong qwordVal)` |  |
| `GetBytes` | `static byte[] GetBytes(ulong? qwordVal)` |  |
| `GetBytes` | `static byte[] GetBytes(ushort wordVal)` |  |
| `GetBytes` | `static byte[] GetBytes(ushort? wordVal)` |  |
| `ToBool` | `static bool ToBool(byte[] arrBytes, int intOffset = 0)` |  |
| `ToByte` | `static byte ToByte(byte[] arrBytes, int intOffset = 0)` |  |
| `ToChar` | `static char ToChar(byte[] arrBytes, int intOffset = 0)` |  |
| `ToDWord` | `static uint ToDWord(byte[] arrBytes, int intOffset = 0)` |  |
| `ToDateTime` | `static DateTime ToDateTime(byte[] arrBytes, int intOffset = 0)` |  |
| `ToDecimal` | `static decimal ToDecimal(byte[] arrBytes, int intOffset = 0)` |  |
| `ToDouble` | `static double ToDouble(byte[] arrBytes, int intOffset = 0)` |  |
| `ToFloat` | `static float ToFloat(byte[] arrBytes, int intOffset = 0)` |  |
| `ToInt` | `static int ToInt(byte[] arrBytes, int intOffset = 0)` |  |
| `ToLong` | `static long ToLong(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNBool` | `static bool? ToNBool(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNByte` | `static byte? ToNByte(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNChar` | `static char? ToNChar(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNDWord` | `static uint? ToNDWord(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNDateTime` | `static DateTime? ToNDateTime(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNDecimal` | `static decimal? ToNDecimal(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNDouble` | `static double? ToNDouble(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNFloat` | `static float? ToNFloat(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNInt` | `static int? ToNInt(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNLong` | `static long? ToNLong(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNQWord` | `static ulong? ToNQWord(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNSByte` | `static sbyte? ToNSByte(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNShort` | `static short? ToNShort(byte[] arrBytes, int intOffset = 0)` |  |
| `ToNWord` | `static ushort? ToNWord(byte[] arrBytes, int intOffset = 0)` |  |
| `ToQWord` | `static ulong ToQWord(byte[] arrBytes, int intOffset = 0)` |  |
| `ToSByte` | `static sbyte ToSByte(byte[] arrBytes, int intOffset = 0)` |  |
| `ToShort` | `static short ToShort(byte[] arrBytes, int intOffset = 0)` |  |
| `ToWord` | `static ushort ToWord(byte[] arrBytes, int intOffset = 0)` |  |

#### `BoolExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `And` | `static bool And(this bool @this, bool other)` |  |
| `Equ` | `static bool Equ(this bool @this, bool other)` |  |
| `Nand` | `static bool Nand(this bool @this, bool other)` |  |
| `Nor` | `static bool Nor(this bool @this, bool other)` |  |
| `Not` | `static bool Not(this bool @this)` |  |
| `Or` | `static bool Or(this bool @this, bool other)` |  |
| `ToOneOrZeroString` | `static string ToOneOrZeroString(this bool @this)` |  |
| `ToTrueOrFalseString` | `static string ToTrueOrFalseString(this bool @this, bool useLowerCaseOnly = false)` |  |
| `ToYesOrNoString` | `static string ToYesOrNoString(this bool @this, bool useLowerCaseOnly = false)` |  |
| `WhenFalse` | `static TResult WhenFalse<TResult>(this bool @this, Func<TResult> callback)` |  |
| `WhenFalse` | `static TResult WhenFalse<TResult>(this bool @this, Func<bool, TResult> callback)` |  |
| `WhenFalse` | `static bool WhenFalse(this bool @this, Action callback)` |  |
| `WhenFalse` | `static bool WhenFalse(this bool @this, Action<bool> callback)` |  |
| `WhenTrue` | `static TResult WhenTrue<TResult>(this bool @this, Func<TResult> callback)` |  |
| `WhenTrue` | `static TResult WhenTrue<TResult>(this bool @this, Func<bool, TResult> callback)` |  |
| `WhenTrue` | `static bool WhenTrue(this bool @this, Action callback)` |  |
| `WhenTrue` | `static bool WhenTrue(this bool @this, Action<bool> callback)` |  |
| `When` | `static TResult When<TResult>(this bool @this, Func<TResult> @true = null, Func<TResult> @false = null)` |  |
| `When` | `static TResult When<TResult>(this bool @this, Func<bool, TResult> @true = null, Func<bool, TResult> @false = null)` |  |
| `When` | `static void When(this bool @this, Action @true = null, Action @false = null)` |  |
| `When` | `static void When(this bool @this, Action<bool> @true = null, Action<bool> @false = null)` |  |
| `Xor` | `static bool Xor(this bool @this, bool other)` |  |

#### `ByteExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this byte @this, char character)` |  |
| `Times` | `static string Times(this byte @this, string text)` |  |
| `Times` | `static void Times(this byte @this, Action action)` |  |
| `Times` | `static void Times(this byte @this, Action<byte> action)` |  |

#### `CharExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IsAnyOf` | `static bool IsAnyOf(this char @this, IEnumerable<char> list)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this char @this, params char[] list)` |  |
| `IsControlButNoWhiteSpace` | `static bool IsControlButNoWhiteSpace(this char @this)` |  |
| `IsControl` | `static bool IsControl(this char @this)` |  |
| `IsDigit` | `static bool IsDigit(this char @this)` |  |
| `IsLetter` | `static bool IsLetter(this char @this)` |  |
| `IsLower` | `static bool IsLower(this char @this)` |  |
| `IsNotControl` | `static bool IsNotControl(this char @this)` |  |
| `IsNotDigit` | `static bool IsNotDigit(this char @this)` |  |
| `IsNotLetter` | `static bool IsNotLetter(this char @this)` |  |
| `IsNotLower` | `static bool IsNotLower(this char @this)` |  |
| `IsNotNullOrWhiteSpace` | `static bool IsNotNullOrWhiteSpace(this char @this)` |  |
| `IsNotUpper` | `static bool IsNotUpper(this char @this)` |  |
| `IsNullOrWhiteSpace` | `static bool IsNullOrWhiteSpace(this char @this)` |  |
| `IsUpper` | `static bool IsUpper(this char @this)` |  |
| `IsWhiteSpace` | `static bool IsWhiteSpace(this char @this)` |  |
| `Repeat` | `static string Repeat(this char @this, int count)` |  |
| `ToLower` | `static char ToLower(this char @this)` |  |
| `ToLower` | `static char ToLower(this char @this, CultureInfo culture)` |  |
| `ToUpper` | `static char ToUpper(this char @this)` |  |
| `ToUpper` | `static char ToUpper(this char @this, CultureInfo culture)` |  |

#### `ConfigurableFixedPoint<TStorage>`

Implements `IComparable`, `IComparable<ConfigurableFixedPoint<TStorage>>`, `IEquatable<ConfigurableFixedPoint<TStorage>>`, `IFormattable`, `IParsable<ConfigurableFixedPoint<TStorage>>`, `ISpanFormattable`, `ISpanParsable<ConfigurableFixedPoint<TStorage>>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ConfigurableFixedPoint` | `ConfigurableFixedPoint(int fractionalBits)` |  |
| `HasSign` | `static readonly bool HasSign` |  |
| `TotalBits` | `static readonly int TotalBits` |  |
| `AsEpsilon` | `ConfigurableFixedPoint<TStorage> AsEpsilon { get; }` |  |
| `AsMaxValue` | `ConfigurableFixedPoint<TStorage> AsMaxValue { get; }` |  |
| `AsMinValue` | `ConfigurableFixedPoint<TStorage> AsMinValue { get; }` |  |
| `AsOne` | `ConfigurableFixedPoint<TStorage> AsOne { get; }` |  |
| `AsZero` | `ConfigurableFixedPoint<TStorage> AsZero { get; }` |  |
| `FractionalBits` | `int FractionalBits { get; }` |  |
| `IntegerBits` | `int IntegerBits { get; }` |  |
| `RawBits` | `BigInteger RawBits { get; }` |  |
| `RawValue` | `TStorage RawValue { get; }` |  |
| `Scale` | `BigInteger Scale { get; }` |  |
| `Abs` | `static ConfigurableFixedPoint<TStorage> Abs(ConfigurableFixedPoint<TStorage> value)` |  |
| `Add` | `static ConfigurableFixedPoint<TStorage> Add(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `Ceiling` | `static ConfigurableFixedPoint<TStorage> Ceiling(ConfigurableFixedPoint<TStorage> value)` |  |
| `Clamp` | `static ConfigurableFixedPoint<TStorage> Clamp(ConfigurableFixedPoint<TStorage> value, ConfigurableFixedPoint<TStorage> min, ConfigurableFixedPoint<TStorage> max)` |  |
| `CompareTo` | `int CompareTo(ConfigurableFixedPoint<TStorage> other)` |  |
| `CompareTo` | `int CompareTo(ConfigurableFloatingPoint<TStorage> other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `ConvertTo` | `ConfigurableFixedPoint<TStorage> ConvertTo(int fractionalBits)` |  |
| `CreateFromDouble` | `ConfigurableFixedPoint<TStorage> CreateFromDouble(double value)` |  |
| `Divide` | `static ConfigurableFixedPoint<TStorage> Divide(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `Epsilon` | `static ConfigurableFixedPoint<TStorage> Epsilon(int fractionalBits)` |  |
| `Equals` | `bool Equals(ConfigurableFixedPoint<TStorage> other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `Floor` | `static ConfigurableFixedPoint<TStorage> Floor(ConfigurableFixedPoint<TStorage> value)` |  |
| `FractionalPart` | `static ConfigurableFixedPoint<TStorage> FractionalPart(ConfigurableFixedPoint<TStorage> value)` |  |
| `FromBFloat16` | `static ConfigurableFixedPoint<TStorage> FromBFloat16(BFloat16 value, int fractionalBits)` |  |
| `FromBFloat32` | `static ConfigurableFixedPoint<TStorage> FromBFloat32(BFloat32 value, int fractionalBits)` |  |
| `FromBFloat64` | `static ConfigurableFixedPoint<TStorage> FromBFloat64(BFloat64 value, int fractionalBits)` |  |
| `FromBFloat8` | `static ConfigurableFixedPoint<TStorage> FromBFloat8(BFloat8 value, int fractionalBits)` |  |
| `FromBigInteger` | `static ConfigurableFixedPoint<TStorage> FromBigInteger(BigInteger value, int fractionalBits)` |  |
| `FromDecimal` | `static ConfigurableFixedPoint<TStorage> FromDecimal(decimal value, int fractionalBits)` |  |
| `FromDouble` | `static ConfigurableFixedPoint<TStorage> FromDouble(double value, int fractionalBits)` |  |
| `FromE4M3` | `static ConfigurableFixedPoint<TStorage> FromE4M3(E4M3 value, int fractionalBits)` |  |
| `FromHalf` | `static ConfigurableFixedPoint<TStorage> FromHalf(Half value, int fractionalBits)` |  |
| `FromInt32` | `static ConfigurableFixedPoint<TStorage> FromInt32(int value, int fractionalBits)` |  |
| `FromInt64` | `static ConfigurableFixedPoint<TStorage> FromInt64(long value, int fractionalBits)` |  |
| `FromMemory` | `static ConfigurableFixedPoint<TStorage> FromMemory(ReadOnlySpan<byte> data, int fractionalBits)` |  |
| `FromQuarter` | `static ConfigurableFixedPoint<TStorage> FromQuarter(Quarter value, int fractionalBits)` |  |
| `FromRaw` | `static ConfigurableFixedPoint<TStorage> FromRaw(TStorage raw, int fractionalBits)` |  |
| `FromSingle` | `static ConfigurableFixedPoint<TStorage> FromSingle(float value, int fractionalBits)` |  |
| `FromUInt32` | `static ConfigurableFixedPoint<TStorage> FromUInt32(uint value, int fractionalBits)` |  |
| `FromUInt64` | `static ConfigurableFixedPoint<TStorage> FromUInt64(ulong value, int fractionalBits)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `MaxValue` | `static ConfigurableFixedPoint<TStorage> MaxValue(int fractionalBits)` |  |
| `Max` | `static ConfigurableFixedPoint<TStorage> Max(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `MinValue` | `static ConfigurableFixedPoint<TStorage> MinValue(int fractionalBits)` |  |
| `Min` | `static ConfigurableFixedPoint<TStorage> Min(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `Modulo` | `static ConfigurableFixedPoint<TStorage> Modulo(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `Multiply` | `static ConfigurableFixedPoint<TStorage> Multiply(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `Negate` | `static ConfigurableFixedPoint<TStorage> Negate(ConfigurableFixedPoint<TStorage> value)` |  |
| `One` | `static ConfigurableFixedPoint<TStorage> One(int fractionalBits)` |  |
| `Parse` | `static ConfigurableFixedPoint<TStorage> Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ConfigurableFixedPoint<TStorage> Parse(string s)` |  |
| `Parse` | `static ConfigurableFixedPoint<TStorage> Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ConfigurableFixedPoint<TStorage> Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `Round` | `static ConfigurableFixedPoint<TStorage> Round(ConfigurableFixedPoint<TStorage> value)` |  |
| `Subtract` | `static ConfigurableFixedPoint<TStorage> Subtract(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `ToBFloat16` | `BFloat16 ToBFloat16()` |  |
| `ToBFloat32` | `BFloat32 ToBFloat32()` |  |
| `ToBFloat64` | `BFloat64 ToBFloat64()` |  |
| `ToBFloat8` | `BFloat8 ToBFloat8()` |  |
| `ToDecimal` | `decimal ToDecimal()` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToE4M3` | `E4M3 ToE4M3()` |  |
| `ToFloatingPoint` | `ConfigurableFloatingPoint<TStorage> ToFloatingPoint(int mantissaBits)` |  |
| `ToHalf` | `Half ToHalf()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToInt64` | `long ToInt64()` |  |
| `ToIntegerPart` | `BigInteger ToIntegerPart()` |  |
| `ToMemory` | `byte[] ToMemory()` |  |
| `ToMemory` | `int ToMemory(Span<byte> destination)` |  |
| `ToQuarter` | `Quarter ToQuarter()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32()` |  |
| `ToUInt64` | `ulong ToUInt64()` |  |
| `Truncate` | `static ConfigurableFixedPoint<TStorage> Truncate(ConfigurableFixedPoint<TStorage> value)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ConfigurableFixedPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ConfigurableFixedPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ConfigurableFixedPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, out ConfigurableFixedPoint<TStorage> result)` |  |
| `Zero` | `static ConfigurableFixedPoint<TStorage> Zero(int fractionalBits)` |  |
| `explicit operator BFloat16` | `static explicit operator BFloat16(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator BFloat32` | `static explicit operator BFloat32(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator BFloat64` | `static explicit operator BFloat64(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator BFloat8` | `static explicit operator BFloat8(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator E4M3` | `static explicit operator E4M3(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator Half` | `static explicit operator Half(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator byte` | `static explicit operator byte(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator decimal` | `static explicit operator decimal(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator double` | `static explicit operator double(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator float` | `static explicit operator float(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator int` | `static explicit operator int(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator long` | `static explicit operator long(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator short` | `static explicit operator short(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator uint` | `static explicit operator uint(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(ConfigurableFixedPoint<TStorage> value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(ConfigurableFixedPoint<TStorage> value)` |  |
| `operator !=` | `static bool operator !=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator %` | `static ConfigurableFixedPoint<TStorage> operator %(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator %` | `static ConfigurableFixedPoint<TStorage> operator %(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, double right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(ConfigurableFixedPoint<TStorage> left, int right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(double left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFixedPoint<TStorage> operator *(int left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator ++` | `static ConfigurableFixedPoint<TStorage> operator ++(ConfigurableFixedPoint<TStorage> value)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, double right)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> left, int right)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(ConfigurableFixedPoint<TStorage> value)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(double left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFixedPoint<TStorage> operator +(int left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator --` | `static ConfigurableFixedPoint<TStorage> operator --(ConfigurableFixedPoint<TStorage> value)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, double right)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> left, int right)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(ConfigurableFixedPoint<TStorage> value)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(double left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFixedPoint<TStorage> operator -(int left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, double right)` |  |
| `operator /` | `static ConfigurableFixedPoint<TStorage> operator /(ConfigurableFixedPoint<TStorage> left, int right)` |  |
| `operator /` | `static ConfigurableFixedPoint<TStorage> operator /(double left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator <=` | `static bool operator <=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator <` | `static bool operator <(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator ==` | `static bool operator ==(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator >=` | `static bool operator >=(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator >` | `static bool operator >(ConfigurableFixedPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |

#### `ConfigurableFloatingPoint<TStorage>`

Implements `IComparable`, `IComparable<ConfigurableFloatingPoint<TStorage>>`, `IEquatable<ConfigurableFloatingPoint<TStorage>>`, `IFormattable`, `IParsable<ConfigurableFloatingPoint<TStorage>>`, `ISpanFormattable`, `ISpanParsable<ConfigurableFloatingPoint<TStorage>>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ConfigurableFloatingPoint` | `ConfigurableFloatingPoint(int mantissaBits)` |  |
| `HasSign` | `static readonly bool HasSign` |  |
| `TotalBits` | `static readonly int TotalBits` |  |
| `AsEpsilon` | `ConfigurableFloatingPoint<TStorage> AsEpsilon { get; }` |  |
| `AsMaxValue` | `ConfigurableFloatingPoint<TStorage> AsMaxValue { get; }` |  |
| `AsMinValue` | `ConfigurableFloatingPoint<TStorage> AsMinValue { get; }` |  |
| `AsNaN` | `ConfigurableFloatingPoint<TStorage> AsNaN { get; }` |  |
| `AsNegativeInfinity` | `ConfigurableFloatingPoint<TStorage> AsNegativeInfinity { get; }` |  |
| `AsOne` | `ConfigurableFloatingPoint<TStorage> AsOne { get; }` |  |
| `AsPositiveInfinity` | `ConfigurableFloatingPoint<TStorage> AsPositiveInfinity { get; }` |  |
| `AsZero` | `ConfigurableFloatingPoint<TStorage> AsZero { get; }` |  |
| `DefaultMantissaBits` | `static int DefaultMantissaBits { get; }` |  |
| `ExponentBias` | `int ExponentBias { get; }` |  |
| `ExponentBits` | `int ExponentBits { get; }` |  |
| `MantissaBits` | `int MantissaBits { get; }` |  |
| `NaN` | `static ConfigurableFloatingPoint<TStorage> NaN { get; }` |  |
| `NegativeInfinity` | `static ConfigurableFloatingPoint<TStorage> NegativeInfinity { get; }` |  |
| `PositiveInfinity` | `static ConfigurableFloatingPoint<TStorage> PositiveInfinity { get; }` |  |
| `RawBits` | `BigInteger RawBits { get; }` |  |
| `RawValue` | `TStorage RawValue { get; }` |  |
| `Abs` | `static ConfigurableFloatingPoint<TStorage> Abs(ConfigurableFloatingPoint<TStorage> value)` |  |
| `Add` | `static ConfigurableFloatingPoint<TStorage> Add(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `Clamp` | `static ConfigurableFloatingPoint<TStorage> Clamp(ConfigurableFloatingPoint<TStorage> value, ConfigurableFloatingPoint<TStorage> min, ConfigurableFloatingPoint<TStorage> max)` |  |
| `CompareTo` | `int CompareTo(ConfigurableFixedPoint<TStorage> other)` |  |
| `CompareTo` | `int CompareTo(ConfigurableFloatingPoint<TStorage> other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `ConvertTo` | `ConfigurableFloatingPoint<TStorage> ConvertTo(int mantissaBits)` |  |
| `CopySign` | `static ConfigurableFloatingPoint<TStorage> CopySign(ConfigurableFloatingPoint<TStorage> magnitude, ConfigurableFloatingPoint<TStorage> sign)` |  |
| `CreateFromDouble` | `ConfigurableFloatingPoint<TStorage> CreateFromDouble(double value)` |  |
| `CreateNaN` | `static ConfigurableFloatingPoint<TStorage> CreateNaN(int mantissaBits)` |  |
| `CreateNegativeInfinity` | `static ConfigurableFloatingPoint<TStorage> CreateNegativeInfinity(int mantissaBits)` |  |
| `CreatePositiveInfinity` | `static ConfigurableFloatingPoint<TStorage> CreatePositiveInfinity(int mantissaBits)` |  |
| `Divide` | `static ConfigurableFloatingPoint<TStorage> Divide(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `Epsilon` | `static ConfigurableFloatingPoint<TStorage> Epsilon(int mantissaBits)` |  |
| `Equals` | `bool Equals(ConfigurableFloatingPoint<TStorage> other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromBFloat16` | `static ConfigurableFloatingPoint<TStorage> FromBFloat16(BFloat16 value, int mantissaBits)` |  |
| `FromBFloat32` | `static ConfigurableFloatingPoint<TStorage> FromBFloat32(BFloat32 value, int mantissaBits)` |  |
| `FromBFloat64` | `static ConfigurableFloatingPoint<TStorage> FromBFloat64(BFloat64 value, int mantissaBits)` |  |
| `FromBFloat8` | `static ConfigurableFloatingPoint<TStorage> FromBFloat8(BFloat8 value, int mantissaBits)` |  |
| `FromComponents` | `static ConfigurableFloatingPoint<TStorage> FromComponents(BigInteger mantissa, int exponent, bool isNegative, int mantissaBits)` |  |
| `FromDecimal` | `static ConfigurableFloatingPoint<TStorage> FromDecimal(decimal value, int mantissaBits)` |  |
| `FromDouble` | `static ConfigurableFloatingPoint<TStorage> FromDouble(double value, int mantissaBits)` |  |
| `FromE4M3` | `static ConfigurableFloatingPoint<TStorage> FromE4M3(E4M3 value, int mantissaBits)` |  |
| `FromHalf` | `static ConfigurableFloatingPoint<TStorage> FromHalf(Half value, int mantissaBits)` |  |
| `FromMemory` | `static ConfigurableFloatingPoint<TStorage> FromMemory(ReadOnlySpan<byte> data, int mantissaBits)` |  |
| `FromQuarter` | `static ConfigurableFloatingPoint<TStorage> FromQuarter(Quarter value, int mantissaBits)` |  |
| `FromRaw` | `static ConfigurableFloatingPoint<TStorage> FromRaw(TStorage raw, int mantissaBits)` |  |
| `FromSingle` | `static ConfigurableFloatingPoint<TStorage> FromSingle(float value, int mantissaBits)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsInfinity` | `static bool IsInfinity(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsNaN` | `static bool IsNaN(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsNegative` | `static bool IsNegative(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsNormal` | `static bool IsNormal(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(ConfigurableFloatingPoint<TStorage> value)` |  |
| `IsZero` | `static bool IsZero(ConfigurableFloatingPoint<TStorage> value)` |  |
| `MantissaBitsFromExponent` | `static int MantissaBitsFromExponent(int exponentBits)` |  |
| `MaxValue` | `static ConfigurableFloatingPoint<TStorage> MaxValue(int mantissaBits)` |  |
| `Max` | `static ConfigurableFloatingPoint<TStorage> Max(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `MinValue` | `static ConfigurableFloatingPoint<TStorage> MinValue(int mantissaBits)` |  |
| `Min` | `static ConfigurableFloatingPoint<TStorage> Min(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `Modulo` | `static ConfigurableFloatingPoint<TStorage> Modulo(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `Multiply` | `static ConfigurableFloatingPoint<TStorage> Multiply(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `Negate` | `static ConfigurableFloatingPoint<TStorage> Negate(ConfigurableFloatingPoint<TStorage> value)` |  |
| `One` | `static ConfigurableFloatingPoint<TStorage> One(int mantissaBits)` |  |
| `Parse` | `static ConfigurableFloatingPoint<TStorage> Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ConfigurableFloatingPoint<TStorage> Parse(string s)` |  |
| `Parse` | `static ConfigurableFloatingPoint<TStorage> Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ConfigurableFloatingPoint<TStorage> Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `Subtract` | `static ConfigurableFloatingPoint<TStorage> Subtract(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `ToBFloat16` | `BFloat16 ToBFloat16()` |  |
| `ToBFloat32` | `BFloat32 ToBFloat32()` |  |
| `ToBFloat64` | `BFloat64 ToBFloat64()` |  |
| `ToBFloat8` | `BFloat8 ToBFloat8()` |  |
| `ToDecimal` | `decimal ToDecimal()` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToE4M3` | `E4M3 ToE4M3()` |  |
| `ToFixedPoint` | `ConfigurableFixedPoint<TStorage> ToFixedPoint(int fractionalBits)` |  |
| `ToHalf` | `Half ToHalf()` |  |
| `ToMemory` | `byte[] ToMemory()` |  |
| `ToMemory` | `int ToMemory(Span<byte> destination)` |  |
| `ToQuarter` | `Quarter ToQuarter()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ConfigurableFloatingPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ConfigurableFloatingPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ConfigurableFloatingPoint<TStorage> result)` |  |
| `TryParse` | `static bool TryParse(string s, out ConfigurableFloatingPoint<TStorage> result)` |  |
| `Zero` | `static ConfigurableFloatingPoint<TStorage> Zero(int mantissaBits)` |  |
| `explicit operator BFloat16` | `static explicit operator BFloat16(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator BFloat32` | `static explicit operator BFloat32(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator BFloat64` | `static explicit operator BFloat64(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator BFloat8` | `static explicit operator BFloat8(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator E4M3` | `static explicit operator E4M3(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator Half` | `static explicit operator Half(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator byte` | `static explicit operator byte(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator decimal` | `static explicit operator decimal(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator double` | `static explicit operator double(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator float` | `static explicit operator float(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator int` | `static explicit operator int(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator long` | `static explicit operator long(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator short` | `static explicit operator short(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator uint` | `static explicit operator uint(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(ConfigurableFloatingPoint<TStorage> value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(ConfigurableFloatingPoint<TStorage> value)` |  |
| `operator !=` | `static bool operator !=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator %` | `static ConfigurableFloatingPoint<TStorage> operator %(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator %` | `static ConfigurableFloatingPoint<TStorage> operator %(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, double right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(ConfigurableFloatingPoint<TStorage> left, int right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(double left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator *` | `static ConfigurableFloatingPoint<TStorage> operator *(int left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator ++` | `static ConfigurableFloatingPoint<TStorage> operator ++(ConfigurableFloatingPoint<TStorage> value)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, double right)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> left, int right)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(ConfigurableFloatingPoint<TStorage> value)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(double left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator +` | `static ConfigurableFloatingPoint<TStorage> operator +(int left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator --` | `static ConfigurableFloatingPoint<TStorage> operator --(ConfigurableFloatingPoint<TStorage> value)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, double right)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> left, int right)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(ConfigurableFloatingPoint<TStorage> value)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(double left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator -` | `static ConfigurableFloatingPoint<TStorage> operator -(int left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, ConfigurableFixedPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, double right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(ConfigurableFloatingPoint<TStorage> left, int right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(double left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator /` | `static ConfigurableFloatingPoint<TStorage> operator /(int left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator <=` | `static bool operator <=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator <` | `static bool operator <(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator ==` | `static bool operator ==(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator >=` | `static bool operator >=(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |
| `operator >` | `static bool operator >(ConfigurableFloatingPoint<TStorage> left, ConfigurableFloatingPoint<TStorage> right)` |  |

#### `ConsoleExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Lock` | `static readonly object Lock` |  |
| `Background` | `static ConsoleColor Background { get; set; }` |  |
| `Foreground` | `static ConsoleColor Foreground { get; set; }` |  |
| `EscapeAdv` | `static string EscapeAdv<T>(T data)` |  |
| `WriteAdv` | `static void WriteAdv(string format)` |  |
| `WriteLineAdv` | `static void WriteLineAdv(string format)` |  |
| `WriteLineNoSpecials` | `static void WriteLineNoSpecials(string format)` |  |
| `WriteLine` | `static void WriteLine()` |  |
| `WriteLine` | `static void WriteLine<T>(T data)` |  |
| `WriteLine` | `static void WriteLine<T>(T data, ConsoleColor foreground)` |  |
| `WriteLine` | `static void WriteLine<T>(T data, ConsoleColor foreground, ConsoleColor background)` |  |
| `WriteNoSpecials` | `static void WriteNoSpecials(string format)` |  |
| `Write` | `static void Write<T>(T data)` |  |
| `Write` | `static void Write<T>(T data, ConsoleColor foreground)` |  |
| `Write` | `static void Write<T>(T data, ConsoleColor foreground, ConsoleColor background)` |  |

#### `ConvertExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromBase91String` | `static byte[] FromBase91String(string encoded)` |  |
| `FromQuotedPrintableString` | `static byte[] FromQuotedPrintableString(string data)` |  |
| `ToBase91String` | `static string ToBase91String(byte[] data)` |  |
| `ToQuotedPrintableString` | `static string ToQuotedPrintableString(byte[] data)` |  |

#### `DateTimeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddWeeks` | `static DateTime AddWeeks(this DateTime @this, int weeks)` |  |
| `AsUnixMillisecondsUtc` | `static long AsUnixMillisecondsUtc(this DateTime @this)` |  |
| `AsUnixTicksUtc` | `static long AsUnixTicksUtc(this DateTime @this)` |  |
| `DateOfDayOfCurrentWeek` | `static DateTime DateOfDayOfCurrentWeek(this DateTime @this, DayOfWeek weekDay)` |  |
| `DayInCurrentWeek` | `static DateTime DayInCurrentWeek(this DateTime @this, DayOfWeek dayOfWeek, DayOfWeek startDayOfWeek = 1)` |  |
| `DaysTill` | `static IEnumerable<DateTime> DaysTill(this DateTime @this, DateTime endDate)` |  |
| `EndOfDay` | `static DateTime EndOfDay(this DateTime @this, long precisionInTicks = 1)` |  |
| `FirstDayOfMonth` | `static DateTime FirstDayOfMonth(this DateTime @this)` |  |
| `FirstDayOfYear` | `static DateTime FirstDayOfYear(this DateTime @this)` |  |
| `FromUnixSeconds` | `static DateTime FromUnixSeconds(long seconds, DateTimeKind kind = 0)` |  |
| `FromUnixTicks` | `static DateTime FromUnixTicks(long ticks, DateTimeKind kind = 0)` |  |
| `InfiniteSequence` | `static IEnumerable<DateTime> InfiniteSequence(DateTime start, TimeSpan step)` |  |
| `LastDayOfMonth` | `static DateTime LastDayOfMonth(this DateTime @this)` |  |
| `LastDayOfYear` | `static DateTime LastDayOfYear(this DateTime @this)` |  |
| `Max` | `static DateTime Max(this DateTime @this, DateTime other)` |  |
| `Min` | `static DateTime Min(this DateTime @this, DateTime other)` |  |
| `Sequence` | `static IEnumerable<DateTime> Sequence(DateTime start, DateTime endInclusive, TimeSpan step)` |  |
| `StartOfDay` | `static DateTime StartOfDay(this DateTime @this)` |  |
| `StartOfWeek` | `static DateTime StartOfWeek(this DateTime @this, DayOfWeek startDayOfWeek = 1)` |  |
| `SubstractDays` | `static DateTime SubstractDays(this DateTime @this, double value)` |  |
| `SubstractHours` | `static DateTime SubstractHours(this DateTime @this, double value)` |  |
| `SubstractMilliseconds` | `static DateTime SubstractMilliseconds(this DateTime @this, double value)` |  |
| `SubstractMinutes` | `static DateTime SubstractMinutes(this DateTime @this, double value)` |  |
| `SubstractMonths` | `static DateTime SubstractMonths(this DateTime @this, int months)` |  |
| `SubstractSeconds` | `static DateTime SubstractSeconds(this DateTime @this, double value)` |  |
| `SubstractTicks` | `static DateTime SubstractTicks(this DateTime @this, long value)` |  |
| `SubstractWeeks` | `static DateTime SubstractWeeks(this DateTime @this, int weeks)` |  |
| `SubstractYears` | `static DateTime SubstractYears(this DateTime @this, int value)` |  |

#### `Decimal128`

Implements `IComparable`, `IComparable<Decimal128>`, `IEquatable<Decimal128>`, `IFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Coefficient` | `BigInteger Coefficient { get; }` |  |
| `Exponent` | `int Exponent { get; }` |  |
| `High` | `ulong High { get; }` |  |
| `Low` | `ulong Low { get; }` |  |
| `MaxValue` | `static Decimal128 MaxValue { get; }` |  |
| `MinValue` | `static Decimal128 MinValue { get; }` |  |
| `NaN` | `static Decimal128 NaN { get; }` |  |
| `NegativeInfinity` | `static Decimal128 NegativeInfinity { get; }` |  |
| `One` | `static Decimal128 One { get; }` |  |
| `PositiveInfinity` | `static Decimal128 PositiveInfinity { get; }` |  |
| `Zero` | `static Decimal128 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Decimal128 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Decimal128 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Decimal128 FromDouble(double value)` |  |
| `FromRaw` | `static Decimal128 FromRaw(ulong high, ulong low)` |  |
| `FromSingle` | `static Decimal128 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Decimal128 d)` |  |
| `IsInfinity` | `static bool IsInfinity(Decimal128 d)` |  |
| `IsNaN` | `static bool IsNaN(Decimal128 d)` |  |
| `IsNegative` | `static bool IsNegative(Decimal128 d)` |  |
| `Parse` | `static Decimal128 Parse(string s)` |  |
| `Parse` | `static Decimal128 Parse(string s, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Decimal128 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Decimal128 result)` |  |
| `explicit operator Decimal128` | `static explicit operator Decimal128(double value)` |  |
| `implicit operator double` | `static implicit operator double(Decimal128 value)` |  |
| `operator !=` | `static bool operator !=(Decimal128 a, Decimal128 b)` |  |
| `operator *` | `static Decimal128 operator *(Decimal128 a, Decimal128 b)` |  |
| `operator +` | `static Decimal128 operator +(Decimal128 a)` |  |
| `operator +` | `static Decimal128 operator +(Decimal128 a, Decimal128 b)` |  |
| `operator -` | `static Decimal128 operator -(Decimal128 a)` |  |
| `operator -` | `static Decimal128 operator -(Decimal128 a, Decimal128 b)` |  |
| `operator /` | `static Decimal128 operator /(Decimal128 a, Decimal128 b)` |  |
| `operator <=` | `static bool operator <=(Decimal128 a, Decimal128 b)` |  |
| `operator <` | `static bool operator <(Decimal128 a, Decimal128 b)` |  |
| `operator ==` | `static bool operator ==(Decimal128 a, Decimal128 b)` |  |
| `operator >=` | `static bool operator >=(Decimal128 a, Decimal128 b)` |  |
| `operator >` | `static bool operator >(Decimal128 a, Decimal128 b)` |  |

#### `Decimal16`

Implements `IComparable`, `IComparable<Decimal16>`, `IEquatable<Decimal16>`, `IFormattable`, `IParsable<Decimal16>`, `ISpanFormattable`, `ISpanParsable<Decimal16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Coefficient` | `int Coefficient { get; }` |  |
| `Exponent` | `int Exponent { get; }` |  |
| `MaxValue` | `static Decimal16 MaxValue { get; }` |  |
| `MinValue` | `static Decimal16 MinValue { get; }` |  |
| `One` | `static Decimal16 One { get; }` |  |
| `RawValue` | `ushort RawValue { get; }` |  |
| `Zero` | `static Decimal16 Zero { get; }` |  |
| `Abs` | `static Decimal16 Abs(Decimal16 value)` |  |
| `CompareTo` | `int CompareTo(Decimal16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Decimal16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Decimal16 FromDouble(double value)` |  |
| `FromRaw` | `static Decimal16 FromRaw(ushort raw)` |  |
| `FromSingle` | `static Decimal16 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(Decimal16 value)` |  |
| `Max` | `static Decimal16 Max(Decimal16 left, Decimal16 right)` |  |
| `Min` | `static Decimal16 Min(Decimal16 left, Decimal16 right)` |  |
| `Parse` | `static Decimal16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Decimal16 Parse(string s)` |  |
| `Parse` | `static Decimal16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Decimal16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Decimal16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Decimal16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Decimal16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Decimal16 result)` |  |
| `explicit operator Decimal16` | `static explicit operator Decimal16(double value)` |  |
| `explicit operator Decimal16` | `static explicit operator Decimal16(float value)` |  |
| `implicit operator double` | `static implicit operator double(Decimal16 value)` |  |
| `implicit operator float` | `static implicit operator float(Decimal16 value)` |  |
| `operator !=` | `static bool operator !=(Decimal16 left, Decimal16 right)` |  |
| `operator *` | `static Decimal16 operator *(Decimal16 a, Decimal16 b)` |  |
| `operator ++` | `static Decimal16 operator ++(Decimal16 value)` |  |
| `operator +` | `static Decimal16 operator +(Decimal16 a, Decimal16 b)` |  |
| `operator +` | `static Decimal16 operator +(Decimal16 value)` |  |
| `operator --` | `static Decimal16 operator --(Decimal16 value)` |  |
| `operator -` | `static Decimal16 operator -(Decimal16 a, Decimal16 b)` |  |
| `operator -` | `static Decimal16 operator -(Decimal16 value)` |  |
| `operator /` | `static Decimal16 operator /(Decimal16 a, Decimal16 b)` |  |
| `operator <=` | `static bool operator <=(Decimal16 left, Decimal16 right)` |  |
| `operator <` | `static bool operator <(Decimal16 left, Decimal16 right)` |  |
| `operator ==` | `static bool operator ==(Decimal16 left, Decimal16 right)` |  |
| `operator >=` | `static bool operator >=(Decimal16 left, Decimal16 right)` |  |
| `operator >` | `static bool operator >(Decimal16 left, Decimal16 right)` |  |

#### `Decimal32`

Implements `IComparable`, `IComparable<Decimal32>`, `IEquatable<Decimal32>`, `IFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Coefficient` | `BigInteger Coefficient { get; }` |  |
| `Exponent` | `int Exponent { get; }` |  |
| `MaxValue` | `static Decimal32 MaxValue { get; }` |  |
| `MinValue` | `static Decimal32 MinValue { get; }` |  |
| `NaN` | `static Decimal32 NaN { get; }` |  |
| `NegativeInfinity` | `static Decimal32 NegativeInfinity { get; }` |  |
| `One` | `static Decimal32 One { get; }` |  |
| `PositiveInfinity` | `static Decimal32 PositiveInfinity { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static Decimal32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Decimal32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Decimal32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Decimal32 FromDouble(double value)` |  |
| `FromRaw` | `static Decimal32 FromRaw(uint raw)` |  |
| `FromSingle` | `static Decimal32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Decimal32 d)` |  |
| `IsInfinity` | `static bool IsInfinity(Decimal32 d)` |  |
| `IsNaN` | `static bool IsNaN(Decimal32 d)` |  |
| `IsNegative` | `static bool IsNegative(Decimal32 d)` |  |
| `Parse` | `static Decimal32 Parse(string s)` |  |
| `Parse` | `static Decimal32 Parse(string s, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Decimal32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Decimal32 result)` |  |
| `explicit operator Decimal32` | `static explicit operator Decimal32(double value)` |  |
| `implicit operator double` | `static implicit operator double(Decimal32 value)` |  |
| `operator !=` | `static bool operator !=(Decimal32 a, Decimal32 b)` |  |
| `operator *` | `static Decimal32 operator *(Decimal32 a, Decimal32 b)` |  |
| `operator +` | `static Decimal32 operator +(Decimal32 a)` |  |
| `operator +` | `static Decimal32 operator +(Decimal32 a, Decimal32 b)` |  |
| `operator -` | `static Decimal32 operator -(Decimal32 a)` |  |
| `operator -` | `static Decimal32 operator -(Decimal32 a, Decimal32 b)` |  |
| `operator /` | `static Decimal32 operator /(Decimal32 a, Decimal32 b)` |  |
| `operator <=` | `static bool operator <=(Decimal32 a, Decimal32 b)` |  |
| `operator <` | `static bool operator <(Decimal32 a, Decimal32 b)` |  |
| `operator ==` | `static bool operator ==(Decimal32 a, Decimal32 b)` |  |
| `operator >=` | `static bool operator >=(Decimal32 a, Decimal32 b)` |  |
| `operator >` | `static bool operator >(Decimal32 a, Decimal32 b)` |  |

#### `Decimal64`

Implements `IComparable`, `IComparable<Decimal64>`, `IEquatable<Decimal64>`, `IFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Coefficient` | `BigInteger Coefficient { get; }` |  |
| `Exponent` | `int Exponent { get; }` |  |
| `MaxValue` | `static Decimal64 MaxValue { get; }` |  |
| `MinValue` | `static Decimal64 MinValue { get; }` |  |
| `NaN` | `static Decimal64 NaN { get; }` |  |
| `NegativeInfinity` | `static Decimal64 NegativeInfinity { get; }` |  |
| `One` | `static Decimal64 One { get; }` |  |
| `PositiveInfinity` | `static Decimal64 PositiveInfinity { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Zero` | `static Decimal64 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Decimal64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Decimal64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Decimal64 FromDouble(double value)` |  |
| `FromRaw` | `static Decimal64 FromRaw(ulong raw)` |  |
| `FromSingle` | `static Decimal64 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Decimal64 d)` |  |
| `IsInfinity` | `static bool IsInfinity(Decimal64 d)` |  |
| `IsNaN` | `static bool IsNaN(Decimal64 d)` |  |
| `IsNegative` | `static bool IsNegative(Decimal64 d)` |  |
| `Parse` | `static Decimal64 Parse(string s)` |  |
| `Parse` | `static Decimal64 Parse(string s, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Decimal64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Decimal64 result)` |  |
| `explicit operator Decimal64` | `static explicit operator Decimal64(double value)` |  |
| `implicit operator double` | `static implicit operator double(Decimal64 value)` |  |
| `operator !=` | `static bool operator !=(Decimal64 a, Decimal64 b)` |  |
| `operator *` | `static Decimal64 operator *(Decimal64 a, Decimal64 b)` |  |
| `operator +` | `static Decimal64 operator +(Decimal64 a)` |  |
| `operator +` | `static Decimal64 operator +(Decimal64 a, Decimal64 b)` |  |
| `operator -` | `static Decimal64 operator -(Decimal64 a)` |  |
| `operator -` | `static Decimal64 operator -(Decimal64 a, Decimal64 b)` |  |
| `operator /` | `static Decimal64 operator /(Decimal64 a, Decimal64 b)` |  |
| `operator <=` | `static bool operator <=(Decimal64 a, Decimal64 b)` |  |
| `operator <` | `static bool operator <(Decimal64 a, Decimal64 b)` |  |
| `operator ==` | `static bool operator ==(Decimal64 a, Decimal64 b)` |  |
| `operator >=` | `static bool operator >=(Decimal64 a, Decimal64 b)` |  |
| `operator >` | `static bool operator >(Decimal64 a, Decimal64 b)` |  |

#### `Decimal8`

Implements `IComparable`, `IComparable<Decimal8>`, `IEquatable<Decimal8>`, `IFormattable`, `IParsable<Decimal8>`, `ISpanFormattable`, `ISpanParsable<Decimal8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Coefficient` | `int Coefficient { get; }` |  |
| `Exponent` | `int Exponent { get; }` |  |
| `MaxValue` | `static Decimal8 MaxValue { get; }` |  |
| `MinValue` | `static Decimal8 MinValue { get; }` |  |
| `One` | `static Decimal8 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static Decimal8 Zero { get; }` |  |
| `Abs` | `static Decimal8 Abs(Decimal8 value)` |  |
| `CompareTo` | `int CompareTo(Decimal8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Decimal8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Decimal8 FromDouble(double value)` |  |
| `FromRaw` | `static Decimal8 FromRaw(byte raw)` |  |
| `FromSingle` | `static Decimal8 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(Decimal8 value)` |  |
| `Max` | `static Decimal8 Max(Decimal8 left, Decimal8 right)` |  |
| `Min` | `static Decimal8 Min(Decimal8 left, Decimal8 right)` |  |
| `Parse` | `static Decimal8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Decimal8 Parse(string s)` |  |
| `Parse` | `static Decimal8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Decimal8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Decimal8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Decimal8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Decimal8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Decimal8 result)` |  |
| `explicit operator Decimal8` | `static explicit operator Decimal8(double value)` |  |
| `explicit operator Decimal8` | `static explicit operator Decimal8(float value)` |  |
| `implicit operator double` | `static implicit operator double(Decimal8 value)` |  |
| `implicit operator float` | `static implicit operator float(Decimal8 value)` |  |
| `operator !=` | `static bool operator !=(Decimal8 left, Decimal8 right)` |  |
| `operator *` | `static Decimal8 operator *(Decimal8 a, Decimal8 b)` |  |
| `operator ++` | `static Decimal8 operator ++(Decimal8 value)` |  |
| `operator +` | `static Decimal8 operator +(Decimal8 a, Decimal8 b)` |  |
| `operator +` | `static Decimal8 operator +(Decimal8 value)` |  |
| `operator --` | `static Decimal8 operator --(Decimal8 value)` |  |
| `operator -` | `static Decimal8 operator -(Decimal8 a, Decimal8 b)` |  |
| `operator -` | `static Decimal8 operator -(Decimal8 value)` |  |
| `operator /` | `static Decimal8 operator /(Decimal8 a, Decimal8 b)` |  |
| `operator <=` | `static bool operator <=(Decimal8 left, Decimal8 right)` |  |
| `operator <` | `static bool operator <(Decimal8 left, Decimal8 right)` |  |
| `operator ==` | `static bool operator ==(Decimal8 left, Decimal8 right)` |  |
| `operator >=` | `static bool operator >=(Decimal8 left, Decimal8 right)` |  |
| `operator >` | `static bool operator >(Decimal8 left, Decimal8 right)` |  |

#### `DosDateTime`

Implements `IComparable`, `IComparable<DosDateTime>`, `IEquatable<DosDateTime>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Day` | `int Day { get; }` |  |
| `Hour` | `int Hour { get; }` |  |
| `Minute` | `int Minute { get; }` |  |
| `Month` | `int Month { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Second` | `int Second { get; }` |  |
| `Year` | `int Year { get; }` |  |
| `CompareTo` | `int CompareTo(DosDateTime other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(DosDateTime other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static DosDateTime FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static DosDateTime FromRaw(uint raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(DosDateTime value)` |  |
| `operator !=` | `static bool operator !=(DosDateTime left, DosDateTime right)` |  |
| `operator <=` | `static bool operator <=(DosDateTime left, DosDateTime right)` |  |
| `operator <` | `static bool operator <(DosDateTime left, DosDateTime right)` |  |
| `operator ==` | `static bool operator ==(DosDateTime left, DosDateTime right)` |  |
| `operator >=` | `static bool operator >=(DosDateTime left, DosDateTime right)` |  |
| `operator >` | `static bool operator >(DosDateTime left, DosDateTime right)` |  |

#### `E2M1`

Implements `IComparable`, `IComparable<E2M1>`, `IEquatable<E2M1>`, `IFormattable`, `IParsable<E2M1>`, `ISpanFormattable`, `ISpanParsable<E2M1>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static E2M1 Epsilon { get; }` |  |
| `MaxValue` | `static E2M1 MaxValue { get; }` |  |
| `MinValue` | `static E2M1 MinValue { get; }` |  |
| `NegativeZero` | `static E2M1 NegativeZero { get; }` |  |
| `One` | `static E2M1 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static E2M1 Zero { get; }` |  |
| `Abs` | `static E2M1 Abs(E2M1 value)` |  |
| `CompareTo` | `int CompareTo(E2M1 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(E2M1 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static E2M1 FromDouble(double value)` |  |
| `FromRaw` | `static E2M1 FromRaw(byte raw)` |  |
| `FromSingle` | `static E2M1 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(E2M1 value)` |  |
| `Max` | `static E2M1 Max(E2M1 left, E2M1 right)` |  |
| `Min` | `static E2M1 Min(E2M1 left, E2M1 right)` |  |
| `Parse` | `static E2M1 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static E2M1 Parse(string s)` |  |
| `Parse` | `static E2M1 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static E2M1 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out E2M1 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out E2M1 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out E2M1 result)` |  |
| `TryParse` | `static bool TryParse(string s, out E2M1 result)` |  |
| `explicit operator E2M1` | `static explicit operator E2M1(double value)` |  |
| `explicit operator E2M1` | `static explicit operator E2M1(float value)` |  |
| `implicit operator double` | `static implicit operator double(E2M1 value)` |  |
| `implicit operator float` | `static implicit operator float(E2M1 value)` |  |
| `operator !=` | `static bool operator !=(E2M1 left, E2M1 right)` |  |
| `operator *` | `static E2M1 operator *(E2M1 left, E2M1 right)` |  |
| `operator +` | `static E2M1 operator +(E2M1 left, E2M1 right)` |  |
| `operator +` | `static E2M1 operator +(E2M1 value)` |  |
| `operator -` | `static E2M1 operator -(E2M1 left, E2M1 right)` |  |
| `operator -` | `static E2M1 operator -(E2M1 value)` |  |
| `operator /` | `static E2M1 operator /(E2M1 left, E2M1 right)` |  |
| `operator <=` | `static bool operator <=(E2M1 left, E2M1 right)` |  |
| `operator <` | `static bool operator <(E2M1 left, E2M1 right)` |  |
| `operator ==` | `static bool operator ==(E2M1 left, E2M1 right)` |  |
| `operator >=` | `static bool operator >=(E2M1 left, E2M1 right)` |  |
| `operator >` | `static bool operator >(E2M1 left, E2M1 right)` |  |

#### `E2M1Codec`

Implements `IBitCodec<float>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `BitWidth` | `int BitWidth { get; }` |  |
| `Decode` | `float Decode(ulong code)` |  |
| `Encode` | `ulong Encode(float value)` |  |

#### `E4M3`

Implements `IComparable`, `IComparable<E4M3>`, `IEquatable<E4M3>`, `IFormattable`, `IParsable<E4M3>`, `ISpanFormattable`, `ISpanParsable<E4M3>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static E4M3 Epsilon { get; }` |  |
| `MaxValue` | `static E4M3 MaxValue { get; }` |  |
| `MinValue` | `static E4M3 MinValue { get; }` |  |
| `NaN` | `static E4M3 NaN { get; }` |  |
| `One` | `static E4M3 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static E4M3 Zero { get; }` |  |
| `Abs` | `static E4M3 Abs(E4M3 value)` |  |
| `CompareTo` | `int CompareTo(E4M3 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static E4M3 CopySign(E4M3 value, E4M3 sign)` |  |
| `Equals` | `bool Equals(E4M3 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static E4M3 FromDouble(double value)` |  |
| `FromRaw` | `static E4M3 FromRaw(byte raw)` |  |
| `FromSingle` | `static E4M3 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(E4M3 value)` |  |
| `IsNaN` | `static bool IsNaN(E4M3 value)` |  |
| `IsNegative` | `static bool IsNegative(E4M3 value)` |  |
| `IsNormal` | `static bool IsNormal(E4M3 value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(E4M3 value)` |  |
| `Max` | `static E4M3 Max(E4M3 left, E4M3 right)` |  |
| `Min` | `static E4M3 Min(E4M3 left, E4M3 right)` |  |
| `Parse` | `static E4M3 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static E4M3 Parse(string s)` |  |
| `Parse` | `static E4M3 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static E4M3 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out E4M3 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out E4M3 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out E4M3 result)` |  |
| `TryParse` | `static bool TryParse(string s, out E4M3 result)` |  |
| `explicit operator E4M3` | `static explicit operator E4M3(double value)` |  |
| `explicit operator E4M3` | `static explicit operator E4M3(float value)` |  |
| `implicit operator double` | `static implicit operator double(E4M3 value)` |  |
| `implicit operator float` | `static implicit operator float(E4M3 value)` |  |
| `operator !=` | `static bool operator !=(E4M3 left, E4M3 right)` |  |
| `operator %` | `static E4M3 operator %(E4M3 left, E4M3 right)` |  |
| `operator *` | `static E4M3 operator *(E4M3 left, E4M3 right)` |  |
| `operator *` | `static E4M3 operator *(E4M3 left, float right)` |  |
| `operator *` | `static E4M3 operator *(E4M3 left, int right)` |  |
| `operator *` | `static E4M3 operator *(float left, E4M3 right)` |  |
| `operator *` | `static E4M3 operator *(int left, E4M3 right)` |  |
| `operator ++` | `static E4M3 operator ++(E4M3 value)` |  |
| `operator +` | `static E4M3 operator +(E4M3 left, E4M3 right)` |  |
| `operator +` | `static E4M3 operator +(E4M3 left, float right)` |  |
| `operator +` | `static E4M3 operator +(E4M3 left, int right)` |  |
| `operator +` | `static E4M3 operator +(E4M3 value)` |  |
| `operator +` | `static E4M3 operator +(float left, E4M3 right)` |  |
| `operator +` | `static E4M3 operator +(int left, E4M3 right)` |  |
| `operator --` | `static E4M3 operator --(E4M3 value)` |  |
| `operator -` | `static E4M3 operator -(E4M3 left, E4M3 right)` |  |
| `operator -` | `static E4M3 operator -(E4M3 left, float right)` |  |
| `operator -` | `static E4M3 operator -(E4M3 left, int right)` |  |
| `operator -` | `static E4M3 operator -(E4M3 value)` |  |
| `operator -` | `static E4M3 operator -(float left, E4M3 right)` |  |
| `operator -` | `static E4M3 operator -(int left, E4M3 right)` |  |
| `operator /` | `static E4M3 operator /(E4M3 left, E4M3 right)` |  |
| `operator /` | `static E4M3 operator /(E4M3 left, float right)` |  |
| `operator /` | `static E4M3 operator /(E4M3 left, int right)` |  |
| `operator /` | `static E4M3 operator /(float left, E4M3 right)` |  |
| `operator /` | `static E4M3 operator /(int left, E4M3 right)` |  |
| `operator <=` | `static bool operator <=(E4M3 left, E4M3 right)` |  |
| `operator <` | `static bool operator <(E4M3 left, E4M3 right)` |  |
| `operator ==` | `static bool operator ==(E4M3 left, E4M3 right)` |  |
| `operator >=` | `static bool operator >=(E4M3 left, E4M3 right)` |  |
| `operator >` | `static bool operator >(E4M3 left, E4M3 right)` |  |

#### `E8M0`

Implements `IComparable`, `IComparable<E8M0>`, `IEquatable<E8M0>`, `IFormattable`, `ISpanFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Exponent` | `int Exponent { get; }` |  |
| `MaxValue` | `static E8M0 MaxValue { get; }` |  |
| `MinValue` | `static E8M0 MinValue { get; }` |  |
| `NaN` | `static E8M0 NaN { get; }` |  |
| `One` | `static E8M0 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(E8M0 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(E8M0 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromExponent` | `static E8M0 FromExponent(int exponent)` |  |
| `FromRaw` | `static E8M0 FromRaw(byte raw)` |  |
| `FromSingle` | `static E8M0 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNaN` | `static bool IsNaN(E8M0 value)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `implicit operator double` | `static implicit operator double(E8M0 value)` |  |
| `implicit operator float` | `static implicit operator float(E8M0 value)` |  |
| `operator !=` | `static bool operator !=(E8M0 left, E8M0 right)` |  |
| `operator <=` | `static bool operator <=(E8M0 left, E8M0 right)` |  |
| `operator <` | `static bool operator <(E8M0 left, E8M0 right)` |  |
| `operator ==` | `static bool operator ==(E8M0 left, E8M0 right)` |  |
| `operator >=` | `static bool operator >=(E8M0 left, E8M0 right)` |  |
| `operator >` | `static bool operator >(E8M0 left, E8M0 right)` |  |

#### `EnumExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetFieldAttribute` | `static TAttribute GetFieldAttribute<TEnum, TAttribute>(this TEnum field)` |  |
| `GetFieldDescription` | `static string GetFieldDescription<TEnum>(this TEnum field)` |  |
| `GetFieldDisplayNameOrDefault` | `static string GetFieldDisplayNameOrDefault<TEnum>(this TEnum field)` |  |
| `GetFieldDisplayName` | `static string GetFieldDisplayName<TEnum>(this TEnum field)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate, Func<TEnum> defaultValueGenerator)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate, Func<string, TEnum> defaultValueGenerator)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate, TEnum defaultValue)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector, Func<TEnum> defaultValueGenerator)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector, Func<string, TEnum> defaultValueGenerator)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector, TEnum defaultValue)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector)` |  |
| `ParseOrNull` | `static TEnum? ParseOrNull<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate)` |  |
| `ParseOrNull` | `static TEnum? ParseOrNull<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, Func<TEnum, string> defaultValueGenerator)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, Func<string> defaultValueGenerator)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, string defaultValue)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, Func<TEnum, string> defaultValueGenerator)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, Func<string> defaultValueGenerator)` |  |
| `ToStringOrDefault` | `static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, string defaultValue)` |  |
| `ToString` | `static string ToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter)` |  |
| `ToString` | `static string ToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum, TAttribute>(this string @this, Func<TAttribute, string, bool> predicate, out TEnum result)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum, TAttribute>(this string @this, Func<TAttribute, string> selector, out TEnum result)` |  |
| `TryToString` | `static bool TryToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, out string result)` |  |
| `TryToString` | `static bool TryToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, out string result)` |  |

#### `FastLazy<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FastLazy` | `FastLazy(Func<TValue> factory)` |  |
| `FastLazy` | `FastLazy(TValue value)` |  |
| `HasValue` | `bool HasValue { get; }` |  |
| `Value` | `TValue Value { get; }` |  |
| `Reset` | `void Reset()` |  |
| `implicit operator FastLazy<TValue>` | `static implicit operator FastLazy<TValue>(Func<TValue> @this)` |  |
| `implicit operator FastLazy<TValue>` | `static implicit operator FastLazy<TValue>(TValue @this)` |  |
| `implicit operator TValue` | `static implicit operator TValue(FastLazy<TValue> @this)` |  |

#### `FileTime`

Implements `IComparable`, `IComparable<FileTime>`, `IEquatable<FileTime>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `ulong RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(FileTime other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(FileTime other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static FileTime FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static FileTime FromRaw(ulong raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(FileTime value)` |  |
| `operator !=` | `static bool operator !=(FileTime left, FileTime right)` |  |
| `operator <=` | `static bool operator <=(FileTime left, FileTime right)` |  |
| `operator <` | `static bool operator <(FileTime left, FileTime right)` |  |
| `operator ==` | `static bool operator ==(FileTime left, FileTime right)` |  |
| `operator >=` | `static bool operator >=(FileTime left, FileTime right)` |  |
| `operator >` | `static bool operator >(FileTime left, FileTime right)` |  |

#### `FunctionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `OnlyOnceFor` | `static Func<TResult> OnlyOnceFor<TResult>(this Func<TResult> @this, TimeSpan span, bool prolongOnAccess = false)` |  |
| `RetryOnException` | `static TResult RetryOnException<TResult>(this Func<TResult> @this, int repeatCount, TimeSpan? dueTime = null)` |  |
| `TryInvoke` | `static bool TryInvoke<TResult>(this Func<TResult> @this, out TResult result, int repeatCount = 1)` |  |

#### `GpsTime`

Implements `IComparable`, `IComparable<GpsTime>`, `IEquatable<GpsTime>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `uint RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(GpsTime other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(GpsTime other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static GpsTime FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static GpsTime FromRaw(uint raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(GpsTime value)` |  |
| `operator !=` | `static bool operator !=(GpsTime left, GpsTime right)` |  |
| `operator <=` | `static bool operator <=(GpsTime left, GpsTime right)` |  |
| `operator <` | `static bool operator <(GpsTime left, GpsTime right)` |  |
| `operator ==` | `static bool operator ==(GpsTime left, GpsTime right)` |  |
| `operator >=` | `static bool operator >=(GpsTime left, GpsTime right)` |  |
| `operator >` | `static bool operator >(GpsTime left, GpsTime right)` |  |

#### `Gray16`

Implements `IComparable`, `IComparable<Gray16>`, `IEquatable<Gray16>`, `IFormattable`, `IParsable<Gray16>`, `ISpanFormattable`, `ISpanParsable<Gray16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static readonly Gray16 MaxValue` |  |
| `MinValue` | `static readonly Gray16 MinValue` |  |
| `Zero` | `static readonly Gray16 Zero` |  |
| `BinaryValue` | `ushort BinaryValue { get; }` |  |
| `GrayValue` | `ushort GrayValue { get; }` |  |
| `CompareTo` | `int CompareTo(Gray16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Gray16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromBinary` | `static Gray16 FromBinary(ushort binary)` |  |
| `FromGray` | `static Gray16 FromGray(ushort gray)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static Gray16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Gray16 Parse(string s)` |  |
| `Parse` | `static Gray16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Gray16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Gray16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Gray16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Gray16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Gray16 result)` |  |
| `explicit operator Gray8` | `static explicit operator Gray8(Gray16 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(Gray16 gray)` |  |
| `implicit operator Gray16` | `static implicit operator Gray16(ushort binary)` |  |
| `implicit operator Gray32` | `static implicit operator Gray32(Gray16 value)` |  |
| `implicit operator Gray64` | `static implicit operator Gray64(Gray16 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(Gray16 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(Gray16 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(Gray16 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(Gray16 value)` |  |
| `implicit operator int` | `static implicit operator int(Gray16 value)` |  |
| `implicit operator long` | `static implicit operator long(Gray16 value)` |  |
| `implicit operator uint` | `static implicit operator uint(Gray16 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(Gray16 value)` |  |
| `operator !=` | `static bool operator !=(Gray16 left, Gray16 right)` |  |
| `operator ++` | `static Gray16 operator ++(Gray16 value)` |  |
| `operator --` | `static Gray16 operator --(Gray16 value)` |  |
| `operator <=` | `static bool operator <=(Gray16 left, Gray16 right)` |  |
| `operator <` | `static bool operator <(Gray16 left, Gray16 right)` |  |
| `operator ==` | `static bool operator ==(Gray16 left, Gray16 right)` |  |
| `operator >=` | `static bool operator >=(Gray16 left, Gray16 right)` |  |
| `operator >` | `static bool operator >(Gray16 left, Gray16 right)` |  |

#### `Gray32`

Implements `IComparable`, `IComparable<Gray32>`, `IEquatable<Gray32>`, `IFormattable`, `IParsable<Gray32>`, `ISpanFormattable`, `ISpanParsable<Gray32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static readonly Gray32 MaxValue` |  |
| `MinValue` | `static readonly Gray32 MinValue` |  |
| `Zero` | `static readonly Gray32 Zero` |  |
| `BinaryValue` | `uint BinaryValue { get; }` |  |
| `GrayValue` | `uint GrayValue { get; }` |  |
| `CompareTo` | `int CompareTo(Gray32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Gray32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromBinary` | `static Gray32 FromBinary(uint binary)` |  |
| `FromGray` | `static Gray32 FromGray(uint gray)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static Gray32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Gray32 Parse(string s)` |  |
| `Parse` | `static Gray32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Gray32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Gray32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Gray32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Gray32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Gray32 result)` |  |
| `explicit operator Gray16` | `static explicit operator Gray16(Gray32 value)` |  |
| `explicit operator Gray8` | `static explicit operator Gray8(Gray32 value)` |  |
| `explicit operator uint` | `static explicit operator uint(Gray32 gray)` |  |
| `implicit operator Gray32` | `static implicit operator Gray32(uint binary)` |  |
| `implicit operator Gray64` | `static implicit operator Gray64(Gray32 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(Gray32 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(Gray32 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(Gray32 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(Gray32 value)` |  |
| `implicit operator long` | `static implicit operator long(Gray32 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(Gray32 value)` |  |
| `operator !=` | `static bool operator !=(Gray32 left, Gray32 right)` |  |
| `operator ++` | `static Gray32 operator ++(Gray32 value)` |  |
| `operator --` | `static Gray32 operator --(Gray32 value)` |  |
| `operator <=` | `static bool operator <=(Gray32 left, Gray32 right)` |  |
| `operator <` | `static bool operator <(Gray32 left, Gray32 right)` |  |
| `operator ==` | `static bool operator ==(Gray32 left, Gray32 right)` |  |
| `operator >=` | `static bool operator >=(Gray32 left, Gray32 right)` |  |
| `operator >` | `static bool operator >(Gray32 left, Gray32 right)` |  |

#### `Gray64`

Implements `IComparable`, `IComparable<Gray64>`, `IEquatable<Gray64>`, `IFormattable`, `IParsable<Gray64>`, `ISpanFormattable`, `ISpanParsable<Gray64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static readonly Gray64 MaxValue` |  |
| `MinValue` | `static readonly Gray64 MinValue` |  |
| `Zero` | `static readonly Gray64 Zero` |  |
| `BinaryValue` | `ulong BinaryValue { get; }` |  |
| `GrayValue` | `ulong GrayValue { get; }` |  |
| `CompareTo` | `int CompareTo(Gray64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Gray64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromBinary` | `static Gray64 FromBinary(ulong binary)` |  |
| `FromGray` | `static Gray64 FromGray(ulong gray)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static Gray64 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Gray64 Parse(string s)` |  |
| `Parse` | `static Gray64 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Gray64 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Gray64 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Gray64 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Gray64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Gray64 result)` |  |
| `explicit operator Gray16` | `static explicit operator Gray16(Gray64 value)` |  |
| `explicit operator Gray32` | `static explicit operator Gray32(Gray64 value)` |  |
| `explicit operator Gray8` | `static explicit operator Gray8(Gray64 value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(Gray64 gray)` |  |
| `implicit operator Gray64` | `static implicit operator Gray64(ulong binary)` |  |
| `implicit operator Int128` | `static implicit operator Int128(Gray64 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(Gray64 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(Gray64 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(Gray64 value)` |  |
| `operator !=` | `static bool operator !=(Gray64 left, Gray64 right)` |  |
| `operator ++` | `static Gray64 operator ++(Gray64 value)` |  |
| `operator --` | `static Gray64 operator --(Gray64 value)` |  |
| `operator <=` | `static bool operator <=(Gray64 left, Gray64 right)` |  |
| `operator <` | `static bool operator <(Gray64 left, Gray64 right)` |  |
| `operator ==` | `static bool operator ==(Gray64 left, Gray64 right)` |  |
| `operator >=` | `static bool operator >=(Gray64 left, Gray64 right)` |  |
| `operator >` | `static bool operator >(Gray64 left, Gray64 right)` |  |

#### `Gray8`

Implements `IComparable`, `IComparable<Gray8>`, `IEquatable<Gray8>`, `IFormattable`, `IParsable<Gray8>`, `ISpanFormattable`, `ISpanParsable<Gray8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static readonly Gray8 MaxValue` |  |
| `MinValue` | `static readonly Gray8 MinValue` |  |
| `Zero` | `static readonly Gray8 Zero` |  |
| `BinaryValue` | `byte BinaryValue { get; }` |  |
| `GrayValue` | `byte GrayValue { get; }` |  |
| `CompareTo` | `int CompareTo(Gray8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Gray8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromBinary` | `static Gray8 FromBinary(byte binary)` |  |
| `FromGray` | `static Gray8 FromGray(byte gray)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static Gray8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Gray8 Parse(string s)` |  |
| `Parse` | `static Gray8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Gray8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Gray8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Gray8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Gray8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Gray8 result)` |  |
| `explicit operator byte` | `static explicit operator byte(Gray8 gray)` |  |
| `implicit operator Gray16` | `static implicit operator Gray16(Gray8 value)` |  |
| `implicit operator Gray32` | `static implicit operator Gray32(Gray8 value)` |  |
| `implicit operator Gray64` | `static implicit operator Gray64(Gray8 value)` |  |
| `implicit operator Gray8` | `static implicit operator Gray8(byte binary)` |  |
| `implicit operator Int128` | `static implicit operator Int128(Gray8 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(Gray8 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(Gray8 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(Gray8 value)` |  |
| `implicit operator int` | `static implicit operator int(Gray8 value)` |  |
| `implicit operator long` | `static implicit operator long(Gray8 value)` |  |
| `implicit operator short` | `static implicit operator short(Gray8 value)` |  |
| `implicit operator uint` | `static implicit operator uint(Gray8 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(Gray8 value)` |  |
| `implicit operator ushort` | `static implicit operator ushort(Gray8 value)` |  |
| `operator !=` | `static bool operator !=(Gray8 left, Gray8 right)` |  |
| `operator ++` | `static Gray8 operator ++(Gray8 value)` |  |
| `operator --` | `static Gray8 operator --(Gray8 value)` |  |
| `operator <=` | `static bool operator <=(Gray8 left, Gray8 right)` |  |
| `operator <` | `static bool operator <(Gray8 left, Gray8 right)` |  |
| `operator ==` | `static bool operator ==(Gray8 left, Gray8 right)` |  |
| `operator >=` | `static bool operator >=(Gray8 left, Gray8 right)` |  |
| `operator >` | `static bool operator >(Gray8 left, Gray8 right)` |  |

#### `HfsPlusDate`

Implements `IComparable`, `IComparable<HfsPlusDate>`, `IEquatable<HfsPlusDate>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `uint RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(HfsPlusDate other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(HfsPlusDate other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static HfsPlusDate FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static HfsPlusDate FromRaw(uint raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(HfsPlusDate value)` |  |
| `operator !=` | `static bool operator !=(HfsPlusDate left, HfsPlusDate right)` |  |
| `operator <=` | `static bool operator <=(HfsPlusDate left, HfsPlusDate right)` |  |
| `operator <` | `static bool operator <(HfsPlusDate left, HfsPlusDate right)` |  |
| `operator ==` | `static bool operator ==(HfsPlusDate left, HfsPlusDate right)` |  |
| `operator >=` | `static bool operator >=(HfsPlusDate left, HfsPlusDate right)` |  |
| `operator >` | `static bool operator >(HfsPlusDate left, HfsPlusDate right)` |  |

#### `IBitCodec<T>`

| Member | Signature | Summary |
| --- | --- | --- |
| `BitWidth` | `int BitWidth { get; }` |  |
| `Decode` | `T Decode(ulong code)` |  |
| `Encode` | `ulong Encode(T value)` |  |

#### `IG711Convention`

| Member | Signature | Summary |
| --- | --- | --- |
| `EncodeALaw` | `byte EncodeALaw(short pcm)` |  |
| `EncodeMuLaw` | `byte EncodeMuLaw(short pcm)` |  |

#### `IbmFloat32`

Implements `IComparable`, `IComparable<IbmFloat32>`, `IEquatable<IbmFloat32>`, `IFormattable`, `IParsable<IbmFloat32>`, `ISpanFormattable`, `ISpanParsable<IbmFloat32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `One` | `static IbmFloat32 One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static IbmFloat32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(IbmFloat32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(IbmFloat32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static IbmFloat32 FromDouble(double value)` |  |
| `FromRaw` | `static IbmFloat32 FromRaw(uint raw)` |  |
| `FromSingle` | `static IbmFloat32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(IbmFloat32 value)` |  |
| `Parse` | `static IbmFloat32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static IbmFloat32 Parse(string s)` |  |
| `Parse` | `static IbmFloat32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static IbmFloat32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out IbmFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out IbmFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out IbmFloat32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out IbmFloat32 result)` |  |
| `explicit operator IbmFloat32` | `static explicit operator IbmFloat32(double value)` |  |
| `explicit operator IbmFloat32` | `static explicit operator IbmFloat32(float value)` |  |
| `implicit operator double` | `static implicit operator double(IbmFloat32 value)` |  |
| `implicit operator float` | `static implicit operator float(IbmFloat32 value)` |  |
| `operator !=` | `static bool operator !=(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator *` | `static IbmFloat32 operator *(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator +` | `static IbmFloat32 operator +(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator -` | `static IbmFloat32 operator -(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator /` | `static IbmFloat32 operator /(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator <=` | `static bool operator <=(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator <` | `static bool operator <(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator ==` | `static bool operator ==(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator >=` | `static bool operator >=(IbmFloat32 left, IbmFloat32 right)` |  |
| `operator >` | `static bool operator >(IbmFloat32 left, IbmFloat32 right)` |  |

#### `IndexedProperty<TIndexer, TIndexer2, TIndexer3, TResult>`

| Member | Signature | Summary |
| --- | --- | --- |
| `IndexedProperty` | `IndexedProperty(Func<TIndexer, TIndexer2, TIndexer3, TResult> getter, Action<TIndexer, TIndexer2, TIndexer3, TResult> setter)` |  |
| `Item` | `TResult this[TIndexer index, TIndexer2 index2, TIndexer3 index3] { get; set; }` |  |

#### `IndexedProperty<TIndexer, TIndexer2, TResult>`

| Member | Signature | Summary |
| --- | --- | --- |
| `IndexedProperty` | `IndexedProperty(Func<TIndexer, TIndexer2, TResult> getter, Action<TIndexer, TIndexer2, TResult> setter)` |  |
| `Item` | `TResult this[TIndexer index, TIndexer2 index2] { get; set; }` |  |

#### `IndexedProperty<TIndexer, TResult>`

| Member | Signature | Summary |
| --- | --- | --- |
| `IndexedProperty` | `IndexedProperty(Func<TIndexer, TResult> getter, Action<TIndexer, TResult> setter = null)` |  |
| `Item` | `TResult this[TIndexer index] { get; set; }` |  |

#### `Int16Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this short @this, char character)` |  |
| `Times` | `static string Times(this short @this, string text)` |  |
| `Times` | `static void Times(this short @this, Action action)` |  |
| `Times` | `static void Times(this short @this, Action<short> action)` |  |

#### `Int32Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this int @this, char character)` |  |
| `Times` | `static string Times(this int @this, string text)` |  |
| `Times` | `static void Times(this int @this, Action action)` |  |
| `Times` | `static void Times(this int @this, Action<int> action)` |  |

#### `Int64Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this long @this, char character)` |  |
| `Times` | `static string Times(this long @this, string text)` |  |
| `Times` | `static void Times(this long @this, Action action)` |  |
| `Times` | `static void Times(this long @this, Action<long> action)` |  |

#### `Int96`

Implements `IComparable`, `IComparable<Int96>`, `IEquatable<Int96>`, `IFormattable`, `IParsable<Int96>`, `ISpanFormattable`, `ISpanParsable<Int96>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Int96` | `Int96(uint upper, ulong lower)` |  |
| `MaxValue` | `static Int96 MaxValue { get; }` |  |
| `MinValue` | `static Int96 MinValue { get; }` |  |
| `NegativeOne` | `static Int96 NegativeOne { get; }` |  |
| `One` | `static Int96 One { get; }` |  |
| `Zero` | `static Int96 Zero { get; }` |  |
| `Abs` | `static Int96 Abs(Int96 value)` |  |
| `Clamp` | `static Int96 Clamp(Int96 value, Int96 min, Int96 max)` |  |
| `CompareTo` | `int CompareTo(Int96 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static Int96 CopySign(Int96 value, Int96 sign)` |  |
| `DivRem` | `static ValueTuple<Int96, Int96> DivRem(Int96 left, Int96 right)` |  |
| `Equals` | `bool Equals(Int96 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsEvenInteger` | `static bool IsEvenInteger(Int96 value)` |  |
| `IsNegative` | `static bool IsNegative(Int96 value)` |  |
| `IsOddInteger` | `static bool IsOddInteger(Int96 value)` |  |
| `IsPositive` | `static bool IsPositive(Int96 value)` |  |
| `IsPow2` | `static bool IsPow2(Int96 value)` |  |
| `LeadingZeroCount` | `static int LeadingZeroCount(Int96 value)` |  |
| `Log2` | `static int Log2(Int96 value)` |  |
| `MaxMagnitude` | `static Int96 MaxMagnitude(Int96 x, Int96 y)` |  |
| `Max` | `static Int96 Max(Int96 x, Int96 y)` |  |
| `MinMagnitude` | `static Int96 MinMagnitude(Int96 x, Int96 y)` |  |
| `Min` | `static Int96 Min(Int96 x, Int96 y)` |  |
| `Parse` | `static Int96 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Int96 Parse(string s)` |  |
| `Parse` | `static Int96 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Int96 Parse(string s, NumberStyles style)` |  |
| `Parse` | `static Int96 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `PopCount` | `static int PopCount(Int96 value)` |  |
| `RotateLeft` | `static Int96 RotateLeft(Int96 value, int rotateAmount)` |  |
| `RotateRight` | `static Int96 RotateRight(Int96 value, int rotateAmount)` |  |
| `Sign` | `static int Sign(Int96 value)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TrailingZeroCount` | `static int TrailingZeroCount(Int96 value)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Int96 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Int96 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Int96 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Int96 result)` |  |
| `explicit operator Half` | `static explicit operator Half(Int96 value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(Half value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(Quarter value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(UInt96 value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(decimal value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(double value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(float value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(Int96 value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(Int96 value)` |  |
| `explicit operator byte` | `static explicit operator byte(Int96 value)` |  |
| `explicit operator char` | `static explicit operator char(Int96 value)` |  |
| `explicit operator decimal` | `static explicit operator decimal(Int96 value)` |  |
| `explicit operator double` | `static explicit operator double(Int96 value)` |  |
| `explicit operator float` | `static explicit operator float(Int96 value)` |  |
| `explicit operator int` | `static explicit operator int(Int96 value)` |  |
| `explicit operator long` | `static explicit operator long(Int96 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(Int96 value)` |  |
| `explicit operator short` | `static explicit operator short(Int96 value)` |  |
| `explicit operator uint` | `static explicit operator uint(Int96 value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(Int96 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(Int96 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(byte value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(char value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(int value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(long value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(sbyte value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(short value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(uint value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ulong value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ushort value)` |  |
| `operator !=` | `static bool operator !=(Int96 left, Int96 right)` |  |
| `operator %` | `static Int96 operator %(Int96 left, Int96 right)` |  |
| `operator &` | `static Int96 operator &(Int96 left, Int96 right)` |  |
| `operator *` | `static Int96 operator *(Int96 left, Int96 right)` |  |
| `operator ++` | `static Int96 operator ++(Int96 value)` |  |
| `operator +` | `static Int96 operator +(Int96 left, Int96 right)` |  |
| `operator +` | `static Int96 operator +(Int96 value)` |  |
| `operator --` | `static Int96 operator --(Int96 value)` |  |
| `operator -` | `static Int96 operator -(Int96 left, Int96 right)` |  |
| `operator -` | `static Int96 operator -(Int96 value)` |  |
| `operator /` | `static Int96 operator /(Int96 left, Int96 right)` |  |
| `operator <<` | `static Int96 operator <<(Int96 value, int shiftAmount)` |  |
| `operator <=` | `static bool operator <=(Int96 left, Int96 right)` |  |
| `operator <` | `static bool operator <(Int96 left, Int96 right)` |  |
| `operator ==` | `static bool operator ==(Int96 left, Int96 right)` |  |
| `operator >=` | `static bool operator >=(Int96 left, Int96 right)` |  |
| `operator >>>` | `static Int96 operator >>>(Int96 value, int shiftAmount)` |  |
| `operator >>` | `static Int96 operator >>(Int96 value, int shiftAmount)` |  |
| `operator >` | `static bool operator >(Int96 left, Int96 right)` |  |
| `operator ^` | `static Int96 operator ^(Int96 left, Int96 right)` |  |
| `operator \|` | `static Int96 operator \|(Int96 left, Int96 right)` |  |
| `operator ~` | `static Int96 operator ~(Int96 value)` |  |

#### `ItuG711`

Implements `IG711Convention`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EncodeALaw` | `byte EncodeALaw(short pcm)` |  |
| `EncodeMuLaw` | `byte EncodeMuLaw(short pcm)` |  |

#### `MBF32`

Implements `IComparable`, `IComparable<MBF32>`, `IEquatable<MBF32>`, `IFormattable`, `IParsable<MBF32>`, `ISpanFormattable`, `ISpanParsable<MBF32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `One` | `static MBF32 One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static MBF32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(MBF32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(MBF32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static MBF32 FromDouble(double value)` |  |
| `FromRaw` | `static MBF32 FromRaw(uint raw)` |  |
| `FromSingle` | `static MBF32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(MBF32 value)` |  |
| `Parse` | `static MBF32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static MBF32 Parse(string s)` |  |
| `Parse` | `static MBF32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static MBF32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out MBF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out MBF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out MBF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out MBF32 result)` |  |
| `explicit operator MBF32` | `static explicit operator MBF32(double value)` |  |
| `explicit operator MBF32` | `static explicit operator MBF32(float value)` |  |
| `implicit operator double` | `static implicit operator double(MBF32 value)` |  |
| `implicit operator float` | `static implicit operator float(MBF32 value)` |  |
| `operator !=` | `static bool operator !=(MBF32 left, MBF32 right)` |  |
| `operator *` | `static MBF32 operator *(MBF32 left, MBF32 right)` |  |
| `operator +` | `static MBF32 operator +(MBF32 left, MBF32 right)` |  |
| `operator -` | `static MBF32 operator -(MBF32 left, MBF32 right)` |  |
| `operator /` | `static MBF32 operator /(MBF32 left, MBF32 right)` |  |
| `operator <=` | `static bool operator <=(MBF32 left, MBF32 right)` |  |
| `operator <` | `static bool operator <(MBF32 left, MBF32 right)` |  |
| `operator ==` | `static bool operator ==(MBF32 left, MBF32 right)` |  |
| `operator >=` | `static bool operator >=(MBF32 left, MBF32 right)` |  |
| `operator >` | `static bool operator >(MBF32 left, MBF32 right)` |  |

#### `MBF64`

Implements `IComparable`, `IComparable<MBF64>`, `IEquatable<MBF64>`, `IFormattable`, `IParsable<MBF64>`, `ISpanFormattable`, `ISpanParsable<MBF64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `One` | `static MBF64 One { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Zero` | `static MBF64 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(MBF64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(MBF64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static MBF64 FromDouble(double value)` |  |
| `FromRaw` | `static MBF64 FromRaw(ulong raw)` |  |
| `FromSingle` | `static MBF64 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(MBF64 value)` |  |
| `Parse` | `static MBF64 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static MBF64 Parse(string s)` |  |
| `Parse` | `static MBF64 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static MBF64 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out MBF64 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out MBF64 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out MBF64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out MBF64 result)` |  |
| `explicit operator MBF64` | `static explicit operator MBF64(double value)` |  |
| `explicit operator MBF64` | `static explicit operator MBF64(float value)` |  |
| `implicit operator double` | `static implicit operator double(MBF64 value)` |  |
| `implicit operator float` | `static implicit operator float(MBF64 value)` |  |
| `operator !=` | `static bool operator !=(MBF64 left, MBF64 right)` |  |
| `operator *` | `static MBF64 operator *(MBF64 left, MBF64 right)` |  |
| `operator +` | `static MBF64 operator +(MBF64 left, MBF64 right)` |  |
| `operator -` | `static MBF64 operator -(MBF64 left, MBF64 right)` |  |
| `operator /` | `static MBF64 operator /(MBF64 left, MBF64 right)` |  |
| `operator <=` | `static bool operator <=(MBF64 left, MBF64 right)` |  |
| `operator <` | `static bool operator <(MBF64 left, MBF64 right)` |  |
| `operator ==` | `static bool operator ==(MBF64 left, MBF64 right)` |  |
| `operator >=` | `static bool operator >=(MBF64 left, MBF64 right)` |  |
| `operator >` | `static bool operator >(MBF64 left, MBF64 right)` |  |

#### `MXFP4`

| Member | Signature | Summary |
| --- | --- | --- |
| `BlockSize` | `const int BlockSize` |  |
| `BlockCount` | `int BlockCount { get; }` |  |
| `Item` | `float this[int index] { get; set; }` |  |
| `Length` | `int Length { get; }` |  |
| `PackedData` | `ReadOnlySpan<byte> PackedData { get; }` |  |
| `Scales` | `ReadOnlySpan<E8M0> Scales { get; }` |  |
| `DecodeBlock` | `int DecodeBlock(int blockIndex, Span<float> destination)` |  |
| `DecodeTo` | `void DecodeTo(Span<float> destination)` |  |
| `Encode` | `static MXFP4 Encode(ReadOnlySpan<float> values)` |  |
| `FromPacked` | `static MXFP4 FromPacked(byte[] packedCodes, E8M0[] scales, int length)` |  |
| `GetElement` | `E2M1 GetElement(int index)` |  |
| `GetEnumerator` | `IEnumerator<float> GetEnumerator()` |  |
| `GetScale` | `E8M0 GetScale(int blockIndex)` |  |
| `ToArray` | `float[] ToArray()` |  |

#### `MathEx`

| Member | Signature | Summary |
| --- | --- | --- |
| `E` | `static readonly decimal E` |  |
| `Ln10` | `static readonly decimal Ln10` |  |
| `Ln2` | `static readonly decimal Ln2` |  |
| `Pi` | `static readonly decimal Pi` |  |
| `Sqrt10` | `static readonly decimal Sqrt10` |  |
| `Sqrt2` | `static readonly decimal Sqrt2` |  |
| `Sqrt3` | `static readonly decimal Sqrt3` |  |
| `Sqrt5` | `static readonly decimal Sqrt5` |  |
| `Sqrt6` | `static readonly decimal Sqrt6` |  |
| `Sqrt7` | `static readonly decimal Sqrt7` |  |
| `Sqrt8` | `static readonly decimal Sqrt8` |  |
| `EnumeratePrimes` | `static IEnumerable<ulong> EnumeratePrimes { get; }` |  |
| `Abs` | `static Int128 Abs(this Int128 @this)` |  |
| `Abs` | `static Int96 Abs(this Int96 @this)` |  |
| `Abs` | `static decimal Abs(this decimal @this)` |  |
| `Abs` | `static double Abs(this double @this)` |  |
| `Abs` | `static float Abs(this float @this)` |  |
| `Abs` | `static int Abs(this int @this)` |  |
| `Abs` | `static long Abs(this long @this)` |  |
| `Abs` | `static sbyte Abs(this sbyte @this)` |  |
| `Abs` | `static short Abs(this short @this)` |  |
| `Acos` | `static Half Acos(this Half @this)` |  |
| `Acos` | `static double Acos(this double @this)` |  |
| `Acos` | `static float Acos(this float @this)` |  |
| `Acot` | `static Half Acot(this Half @this)` |  |
| `Acot` | `static double Acot(this double @this)` |  |
| `Acot` | `static float Acot(this float @this)` |  |
| `Acsc` | `static Half Acsc(this Half @this)` |  |
| `Acsc` | `static double Acsc(this double @this)` |  |
| `Acsc` | `static float Acsc(this float @this)` |  |
| `Add` | `static Int128 Add(this Int128 @this, Int128 operand)` |  |
| `Add` | `static Int96 Add(this Int96 @this, Int96 operand)` |  |
| `Add` | `static UInt128 Add(this UInt128 @this, UInt128 operand)` |  |
| `Add` | `static UInt96 Add(this UInt96 @this, UInt96 operand)` |  |
| `Add` | `static byte Add(this byte @this, byte operand)` |  |
| `Add` | `static decimal Add(this decimal @this, decimal operand)` |  |
| `Add` | `static double Add(this double @this, double operand)` |  |
| `Add` | `static float Add(this float @this, float operand)` |  |
| `Add` | `static int Add(this int @this, int operand)` |  |
| `Add` | `static long Add(this long @this, long operand)` |  |
| `Add` | `static sbyte Add(this sbyte @this, sbyte operand)` |  |
| `Add` | `static short Add(this short @this, short operand)` |  |
| `Add` | `static uint Add(this uint @this, uint operand)` |  |
| `Add` | `static ulong Add(this ulong @this, ulong operand)` |  |
| `Add` | `static ushort Add(this ushort @this, ushort operand)` |  |
| `And` | `static Int128 And(this Int128 @this, Int128 other)` |  |
| `And` | `static Int96 And(this Int96 @this, Int96 other)` |  |
| `And` | `static UInt128 And(this UInt128 @this, UInt128 other)` |  |
| `And` | `static UInt96 And(this UInt96 @this, UInt96 other)` |  |
| `And` | `static byte And(this byte @this, byte other)` |  |
| `And` | `static int And(this int @this, int other)` |  |
| `And` | `static long And(this long @this, long other)` |  |
| `And` | `static sbyte And(this sbyte @this, sbyte other)` |  |
| `And` | `static short And(this short @this, short other)` |  |
| `And` | `static uint And(this uint @this, uint other)` |  |
| `And` | `static ulong And(this ulong @this, ulong other)` |  |
| `And` | `static ushort And(this ushort @this, ushort other)` |  |
| `Arcosh` | `static Half Arcosh(this Half @this)` |  |
| `Arcosh` | `static double Arcosh(this double @this)` |  |
| `Arcosh` | `static float Arcosh(this float @this)` |  |
| `Arcoth` | `static Half Arcoth(this Half @this)` |  |
| `Arcoth` | `static double Arcoth(this double @this)` |  |
| `Arcoth` | `static float Arcoth(this float @this)` |  |
| `Arcsch` | `static Half Arcsch(this Half @this)` |  |
| `Arcsch` | `static double Arcsch(this double @this)` |  |
| `Arcsch` | `static float Arcsch(this float @this)` |  |
| `ArithmeticShiftLeft` | `static Int128 ArithmeticShiftLeft(this Int128 @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static Int96 ArithmeticShiftLeft(this Int96 @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static UInt128 ArithmeticShiftLeft(this UInt128 @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static UInt96 ArithmeticShiftLeft(this UInt96 @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static byte ArithmeticShiftLeft(this byte @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static int ArithmeticShiftLeft(this int @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static long ArithmeticShiftLeft(this long @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static sbyte ArithmeticShiftLeft(this sbyte @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static short ArithmeticShiftLeft(this short @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static uint ArithmeticShiftLeft(this uint @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static ulong ArithmeticShiftLeft(this ulong @this, byte count)` |  |
| `ArithmeticShiftLeft` | `static ushort ArithmeticShiftLeft(this ushort @this, byte count)` |  |
| `ArithmeticShiftRight` | `static Int128 ArithmeticShiftRight(this Int128 @this, byte count)` |  |
| `ArithmeticShiftRight` | `static Int96 ArithmeticShiftRight(this Int96 @this, byte count)` |  |
| `ArithmeticShiftRight` | `static UInt128 ArithmeticShiftRight(this UInt128 @this, byte count)` |  |
| `ArithmeticShiftRight` | `static UInt96 ArithmeticShiftRight(this UInt96 @this, byte count)` |  |
| `ArithmeticShiftRight` | `static byte ArithmeticShiftRight(this byte @this, byte count)` |  |
| `ArithmeticShiftRight` | `static int ArithmeticShiftRight(this int @this, byte count)` |  |
| `ArithmeticShiftRight` | `static long ArithmeticShiftRight(this long @this, byte count)` |  |
| `ArithmeticShiftRight` | `static sbyte ArithmeticShiftRight(this sbyte @this, byte count)` |  |
| `ArithmeticShiftRight` | `static short ArithmeticShiftRight(this short @this, byte count)` |  |
| `ArithmeticShiftRight` | `static uint ArithmeticShiftRight(this uint @this, byte count)` |  |
| `ArithmeticShiftRight` | `static ulong ArithmeticShiftRight(this ulong @this, byte count)` |  |
| `ArithmeticShiftRight` | `static ushort ArithmeticShiftRight(this ushort @this, byte count)` |  |
| `Arsech` | `static Half Arsech(this Half @this)` |  |
| `Arsech` | `static double Arsech(this double @this)` |  |
| `Arsech` | `static float Arsech(this float @this)` |  |
| `Arsinh` | `static Half Arsinh(this Half @this)` |  |
| `Arsinh` | `static double Arsinh(this double @this)` |  |
| `Arsinh` | `static float Arsinh(this float @this)` |  |
| `Artanh` | `static Half Artanh(this Half @this)` |  |
| `Artanh` | `static double Artanh(this double @this)` |  |
| `Artanh` | `static float Artanh(this float @this)` |  |
| `Asec` | `static Half Asec(this Half @this)` |  |
| `Asec` | `static double Asec(this double @this)` |  |
| `Asec` | `static float Asec(this float @this)` |  |
| `Asin` | `static Half Asin(this Half @this)` |  |
| `Asin` | `static double Asin(this double @this)` |  |
| `Asin` | `static float Asin(this float @this)` |  |
| `Atan` | `static Half Atan(this Half @this)` |  |
| `Atan` | `static decimal Atan(this decimal @this, decimal epsilon)` |  |
| `Atan` | `static double Atan(this double @this)` |  |
| `Atan` | `static float Atan(this float @this)` |  |
| `Average` | `static byte Average(params byte[] values)` |  |
| `Average` | `static decimal Average(params decimal[] values)` |  |
| `Average` | `static double Average(params double[] values)` |  |
| `Average` | `static float Average(params float[] values)` |  |
| `Average` | `static int Average(params int[] values)` |  |
| `Average` | `static long Average(params long[] values)` |  |
| `Average` | `static sbyte Average(params sbyte[] values)` |  |
| `Average` | `static short Average(params short[] values)` |  |
| `Average` | `static uint Average(params uint[] values)` |  |
| `Average` | `static ulong Average(params ulong[] values)` |  |
| `Average` | `static ushort Average(params ushort[] values)` |  |
| `Bits` | `static Int128 Bits(this Int128 @this, byte index, byte count)` |  |
| `Bits` | `static Int96 Bits(this Int96 @this, byte index, byte count)` |  |
| `Bits` | `static UInt128 Bits(this UInt128 @this, byte index, byte count)` |  |
| `Bits` | `static UInt96 Bits(this UInt96 @this, byte index, byte count)` |  |
| `Bits` | `static byte Bits(this byte @this, byte index, byte count)` |  |
| `Bits` | `static int Bits(this int @this, byte index, byte count)` |  |
| `Bits` | `static long Bits(this long @this, byte index, byte count)` |  |
| `Bits` | `static sbyte Bits(this sbyte @this, byte index, byte count)` |  |
| `Bits` | `static short Bits(this short @this, byte index, byte count)` |  |
| `Bits` | `static uint Bits(this uint @this, byte index, byte count)` |  |
| `Bits` | `static ulong Bits(this ulong @this, byte index, byte count)` |  |
| `Bits` | `static ushort Bits(this ushort @this, byte index, byte count)` |  |
| `Cbrt` | `static decimal Cbrt(this decimal @this)` |  |
| `Cbrt` | `static double Cbrt(this double @this)` |  |
| `Cbrt` | `static float Cbrt(this float @this)` |  |
| `Ceiling` | `static Half Ceiling(this Half @this)` |  |
| `Ceiling` | `static decimal Ceiling(this decimal @this)` |  |
| `Ceiling` | `static double Ceiling(this double @this)` |  |
| `Ceiling` | `static float Ceiling(this float @this)` |  |
| `ClampUnchecked` | `static Int128 ClampUnchecked(this Int128 @this, Int128 min, Int128 max)` |  |
| `ClampUnchecked` | `static Int96 ClampUnchecked(this Int96 @this, Int96 min, Int96 max)` |  |
| `ClampUnchecked` | `static UInt128 ClampUnchecked(this UInt128 @this, UInt128 min, UInt128 max)` |  |
| `ClampUnchecked` | `static UInt96 ClampUnchecked(this UInt96 @this, UInt96 min, UInt96 max)` |  |
| `ClampUnchecked` | `static byte ClampUnchecked(this byte @this, byte min, byte max)` |  |
| `ClampUnchecked` | `static decimal ClampUnchecked(this decimal @this, decimal min, decimal max)` |  |
| `ClampUnchecked` | `static double ClampUnchecked(this double @this, double min, double max)` |  |
| `ClampUnchecked` | `static float ClampUnchecked(this float @this, float min, float max)` |  |
| `ClampUnchecked` | `static int ClampUnchecked(this int @this, int min, int max)` |  |
| `ClampUnchecked` | `static long ClampUnchecked(this long @this, long min, long max)` |  |
| `ClampUnchecked` | `static sbyte ClampUnchecked(this sbyte @this, sbyte min, sbyte max)` |  |
| `ClampUnchecked` | `static short ClampUnchecked(this short @this, short min, short max)` |  |
| `ClampUnchecked` | `static uint ClampUnchecked(this uint @this, uint min, uint max)` |  |
| `ClampUnchecked` | `static ulong ClampUnchecked(this ulong @this, ulong min, ulong max)` |  |
| `ClampUnchecked` | `static ushort ClampUnchecked(this ushort @this, ushort min, ushort max)` |  |
| `Clamp` | `static Int128 Clamp(this Int128 @this, Int128 min, Int128 max)` |  |
| `Clamp` | `static Int96 Clamp(this Int96 @this, Int96 min, Int96 max)` |  |
| `Clamp` | `static UInt128 Clamp(this UInt128 @this, UInt128 min, UInt128 max)` |  |
| `Clamp` | `static UInt96 Clamp(this UInt96 @this, UInt96 min, UInt96 max)` |  |
| `Clamp` | `static byte Clamp(this byte @this, byte min, byte max)` |  |
| `Clamp` | `static decimal Clamp(this decimal @this, decimal min, decimal max)` |  |
| `Clamp` | `static double Clamp(this double @this, double min, double max)` |  |
| `Clamp` | `static float Clamp(this float @this, float min, float max)` |  |
| `Clamp` | `static int Clamp(this int @this, int min, int max)` |  |
| `Clamp` | `static long Clamp(this long @this, long min, long max)` |  |
| `Clamp` | `static sbyte Clamp(this sbyte @this, sbyte min, sbyte max)` |  |
| `Clamp` | `static short Clamp(this short @this, short min, short max)` |  |
| `Clamp` | `static uint Clamp(this uint @this, uint min, uint max)` |  |
| `Clamp` | `static ulong Clamp(this ulong @this, ulong min, ulong max)` |  |
| `Clamp` | `static ushort Clamp(this ushort @this, ushort min, ushort max)` |  |
| `ClearBit` | `static Int128 ClearBit(this Int128 @this, byte index)` |  |
| `ClearBit` | `static Int96 ClearBit(this Int96 @this, byte index)` |  |
| `ClearBit` | `static UInt128 ClearBit(this UInt128 @this, byte index)` |  |
| `ClearBit` | `static UInt96 ClearBit(this UInt96 @this, byte index)` |  |
| `ClearBit` | `static byte ClearBit(this byte @this, byte index)` |  |
| `ClearBit` | `static uint ClearBit(this uint @this, byte index)` |  |
| `ClearBit` | `static ulong ClearBit(this ulong @this, byte index)` |  |
| `ClearBit` | `static ushort ClearBit(this ushort @this, byte index)` |  |
| `Cos` | `static decimal Cos(this decimal @this, decimal epsilon)` |  |
| `Cos` | `static double Cos(this double @this)` |  |
| `Cos` | `static float Cos(this float @this)` |  |
| `Cosh` | `static double Cosh(this double @this)` |  |
| `Cosh` | `static float Cosh(this float @this)` |  |
| `Cot` | `static Half Cot(this Half @this)` |  |
| `Cot` | `static double Cot(this double @this)` |  |
| `Cot` | `static float Cot(this float @this)` |  |
| `Coth` | `static Half Coth(this Half @this)` |  |
| `Coth` | `static double Coth(this double @this)` |  |
| `Coth` | `static float Coth(this float @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this Int128 @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this Int96 @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this UInt128 @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this UInt96 @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this byte @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this uint @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this ulong @this)` |  |
| `CountSetBits` | `static byte CountSetBits(this ushort @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this Int128 @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this Int96 @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this UInt128 @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this UInt96 @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this byte @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this uint @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this ulong @this)` |  |
| `CountUnsetBits` | `static byte CountUnsetBits(this ushort @this)` |  |
| `Csc` | `static Half Csc(this Half @this)` |  |
| `Csc` | `static double Csc(this double @this)` |  |
| `Csc` | `static float Csc(this float @this)` |  |
| `Csch` | `static Half Csch(this Half @this)` |  |
| `Csch` | `static double Csch(this double @this)` |  |
| `Csch` | `static float Csch(this float @this)` |  |
| `Cubed` | `static Int128 Cubed(this Int128 @this)` |  |
| `Cubed` | `static Int96 Cubed(this Int96 @this)` |  |
| `Cubed` | `static UInt128 Cubed(this UInt128 @this)` |  |
| `Cubed` | `static UInt96 Cubed(this UInt96 @this)` |  |
| `Cubed` | `static byte Cubed(this byte @this)` |  |
| `Cubed` | `static decimal Cubed(this decimal @this)` |  |
| `Cubed` | `static double Cubed(this double @this)` |  |
| `Cubed` | `static float Cubed(this float @this)` |  |
| `Cubed` | `static int Cubed(this int @this)` |  |
| `Cubed` | `static long Cubed(this long @this)` |  |
| `Cubed` | `static sbyte Cubed(this sbyte @this)` |  |
| `Cubed` | `static short Cubed(this short @this)` |  |
| `Cubed` | `static uint Cubed(this uint @this)` |  |
| `Cubed` | `static ulong Cubed(this ulong @this)` |  |
| `Cubed` | `static ushort Cubed(this ushort @this)` |  |
| `DeinterleaveBits` | `static ValueTuple<byte, byte> DeinterleaveBits(this byte @this)` |  |
| `DeinterleaveBits` | `static ValueTuple<byte, byte> DeinterleaveBits(this ushort @this)` |  |
| `DeinterleaveBits` | `static ValueTuple<uint, uint> DeinterleaveBits(this ulong @this)` |  |
| `DeinterleaveBits` | `static ValueTuple<ushort, ushort> DeinterleaveBits(this uint @this)` |  |
| `DividedBy` | `static Int128 DividedBy(this Int128 @this, Int128 divisor)` |  |
| `DividedBy` | `static Int96 DividedBy(this Int96 @this, Int96 divisor)` |  |
| `DividedBy` | `static UInt128 DividedBy(this UInt128 @this, UInt128 divisor)` |  |
| `DividedBy` | `static UInt96 DividedBy(this UInt96 @this, UInt96 divisor)` |  |
| `DividedBy` | `static byte DividedBy(this byte @this, byte divisor)` |  |
| `DividedBy` | `static decimal DividedBy(this decimal @this, decimal divisor)` |  |
| `DividedBy` | `static double DividedBy(this double @this, double divisor)` |  |
| `DividedBy` | `static float DividedBy(this float @this, float divisor)` |  |
| `DividedBy` | `static int DividedBy(this int @this, int divisor)` |  |
| `DividedBy` | `static long DividedBy(this long @this, long divisor)` |  |
| `DividedBy` | `static sbyte DividedBy(this sbyte @this, sbyte divisor)` |  |
| `DividedBy` | `static short DividedBy(this short @this, short divisor)` |  |
| `DividedBy` | `static uint DividedBy(this uint @this, uint divisor)` |  |
| `DividedBy` | `static ulong DividedBy(this ulong @this, ulong divisor)` |  |
| `DividedBy` | `static ushort DividedBy(this ushort @this, ushort divisor)` |  |
| `Equ` | `static Int128 Equ(this Int128 @this, Int128 other)` |  |
| `Equ` | `static Int96 Equ(this Int96 @this, Int96 other)` |  |
| `Equ` | `static UInt128 Equ(this UInt128 @this, UInt128 other)` |  |
| `Equ` | `static UInt96 Equ(this UInt96 @this, UInt96 other)` |  |
| `Equ` | `static byte Equ(this byte @this, byte other)` |  |
| `Equ` | `static int Equ(this int @this, int other)` |  |
| `Equ` | `static long Equ(this long @this, long other)` |  |
| `Equ` | `static sbyte Equ(this sbyte @this, sbyte other)` |  |
| `Equ` | `static short Equ(this short @this, short other)` |  |
| `Equ` | `static uint Equ(this uint @this, uint other)` |  |
| `Equ` | `static ulong Equ(this ulong @this, ulong other)` |  |
| `Equ` | `static ushort Equ(this ushort @this, ushort other)` |  |
| `Exp` | `static decimal Exp(this decimal @this, decimal epsilon)` |  |
| `Exp` | `static double Exp(this double @this)` |  |
| `Exp` | `static float Exp(this float @this)` |  |
| `FlipBit` | `static Int128 FlipBit(this Int128 @this, byte index)` |  |
| `FlipBit` | `static Int96 FlipBit(this Int96 @this, byte index)` |  |
| `FlipBit` | `static UInt128 FlipBit(this UInt128 @this, byte index)` |  |
| `FlipBit` | `static UInt96 FlipBit(this UInt96 @this, byte index)` |  |
| `FlipBit` | `static byte FlipBit(this byte @this, byte index)` |  |
| `FlipBit` | `static uint FlipBit(this uint @this, byte index)` |  |
| `FlipBit` | `static ulong FlipBit(this ulong @this, byte index)` |  |
| `FlipBit` | `static ushort FlipBit(this ushort @this, byte index)` |  |
| `Floor` | `static Half Floor(this Half @this)` |  |
| `Floor` | `static decimal Floor(this decimal @this)` |  |
| `Floor` | `static double Floor(this double @this)` |  |
| `Floor` | `static float Floor(this float @this)` |  |
| `FusedDivideAdd` | `static Int128 FusedDivideAdd(this Int128 @this, Int128 divisor, Int128 addend)` |  |
| `FusedDivideAdd` | `static Int96 FusedDivideAdd(this Int96 @this, Int96 divisor, Int96 addend)` |  |
| `FusedDivideAdd` | `static UInt128 FusedDivideAdd(this UInt128 @this, UInt128 divisor, UInt128 addend)` |  |
| `FusedDivideAdd` | `static UInt96 FusedDivideAdd(this UInt96 @this, UInt96 divisor, UInt96 addend)` |  |
| `FusedDivideAdd` | `static byte FusedDivideAdd(this byte @this, byte divisor, byte addend)` |  |
| `FusedDivideAdd` | `static decimal FusedDivideAdd(this decimal @this, decimal divisor, decimal addend)` |  |
| `FusedDivideAdd` | `static double FusedDivideAdd(this double @this, double divisor, double addend)` |  |
| `FusedDivideAdd` | `static float FusedDivideAdd(this float @this, float divisor, float addend)` |  |
| `FusedDivideAdd` | `static int FusedDivideAdd(this int @this, int divisor, int addend)` |  |
| `FusedDivideAdd` | `static long FusedDivideAdd(this long @this, long divisor, long addend)` |  |
| `FusedDivideAdd` | `static sbyte FusedDivideAdd(this sbyte @this, sbyte divisor, sbyte addend)` |  |
| `FusedDivideAdd` | `static short FusedDivideAdd(this short @this, short divisor, short addend)` |  |
| `FusedDivideAdd` | `static uint FusedDivideAdd(this uint @this, uint divisor, uint addend)` |  |
| `FusedDivideAdd` | `static ulong FusedDivideAdd(this ulong @this, ulong divisor, ulong addend)` |  |
| `FusedDivideAdd` | `static ushort FusedDivideAdd(this ushort @this, ushort divisor, ushort addend)` |  |
| `FusedDivideSubtract` | `static Int128 FusedDivideSubtract(this Int128 @this, Int128 divisor, Int128 subtrahend)` |  |
| `FusedDivideSubtract` | `static Int96 FusedDivideSubtract(this Int96 @this, Int96 divisor, Int96 subtrahend)` |  |
| `FusedDivideSubtract` | `static UInt128 FusedDivideSubtract(this UInt128 @this, UInt128 divisor, UInt128 subtrahend)` |  |
| `FusedDivideSubtract` | `static UInt96 FusedDivideSubtract(this UInt96 @this, UInt96 divisor, UInt96 subtrahend)` |  |
| `FusedDivideSubtract` | `static byte FusedDivideSubtract(this byte @this, byte divisor, byte subtrahend)` |  |
| `FusedDivideSubtract` | `static decimal FusedDivideSubtract(this decimal @this, decimal divisor, decimal subtrahend)` |  |
| `FusedDivideSubtract` | `static double FusedDivideSubtract(this double @this, double divisor, double subtrahend)` |  |
| `FusedDivideSubtract` | `static float FusedDivideSubtract(this float @this, float divisor, float subtrahend)` |  |
| `FusedDivideSubtract` | `static int FusedDivideSubtract(this int @this, int divisor, int subtrahend)` |  |
| `FusedDivideSubtract` | `static long FusedDivideSubtract(this long @this, long divisor, long subtrahend)` |  |
| `FusedDivideSubtract` | `static sbyte FusedDivideSubtract(this sbyte @this, sbyte divisor, sbyte subtrahend)` |  |
| `FusedDivideSubtract` | `static short FusedDivideSubtract(this short @this, short divisor, short subtrahend)` |  |
| `FusedDivideSubtract` | `static uint FusedDivideSubtract(this uint @this, uint divisor, uint subtrahend)` |  |
| `FusedDivideSubtract` | `static ulong FusedDivideSubtract(this ulong @this, ulong divisor, ulong subtrahend)` |  |
| `FusedDivideSubtract` | `static ushort FusedDivideSubtract(this ushort @this, ushort divisor, ushort subtrahend)` |  |
| `FusedMultiplyAdd` | `static Int128 FusedMultiplyAdd(this Int128 @this, Int128 factor, Int128 addend)` |  |
| `FusedMultiplyAdd` | `static Int96 FusedMultiplyAdd(this Int96 @this, Int96 factor, Int96 addend)` |  |
| `FusedMultiplyAdd` | `static UInt128 FusedMultiplyAdd(this UInt128 @this, UInt128 factor, UInt128 addend)` |  |
| `FusedMultiplyAdd` | `static UInt96 FusedMultiplyAdd(this UInt96 @this, UInt96 factor, UInt96 addend)` |  |
| `FusedMultiplyAdd` | `static byte FusedMultiplyAdd(this byte @this, byte factor, byte addend)` |  |
| `FusedMultiplyAdd` | `static decimal FusedMultiplyAdd(this decimal @this, decimal factor, decimal addend)` |  |
| `FusedMultiplyAdd` | `static double FusedMultiplyAdd(this double @this, double factor, double addend)` |  |
| `FusedMultiplyAdd` | `static float FusedMultiplyAdd(this float @this, float factor, float addend)` |  |
| `FusedMultiplyAdd` | `static int FusedMultiplyAdd(this int @this, int factor, int addend)` |  |
| `FusedMultiplyAdd` | `static long FusedMultiplyAdd(this long @this, long factor, long addend)` |  |
| `FusedMultiplyAdd` | `static sbyte FusedMultiplyAdd(this sbyte @this, sbyte factor, sbyte addend)` |  |
| `FusedMultiplyAdd` | `static short FusedMultiplyAdd(this short @this, short factor, short addend)` |  |
| `FusedMultiplyAdd` | `static uint FusedMultiplyAdd(this uint @this, uint factor, uint addend)` |  |
| `FusedMultiplyAdd` | `static ulong FusedMultiplyAdd(this ulong @this, ulong factor, ulong addend)` |  |
| `FusedMultiplyAdd` | `static ushort FusedMultiplyAdd(this ushort @this, ushort factor, ushort addend)` |  |
| `FusedMultiplySubtract` | `static Int128 FusedMultiplySubtract(this Int128 @this, Int128 factor, Int128 subtrahend)` |  |
| `FusedMultiplySubtract` | `static Int96 FusedMultiplySubtract(this Int96 @this, Int96 factor, Int96 subtrahend)` |  |
| `FusedMultiplySubtract` | `static UInt128 FusedMultiplySubtract(this UInt128 @this, UInt128 factor, UInt128 subtrahend)` |  |
| `FusedMultiplySubtract` | `static UInt96 FusedMultiplySubtract(this UInt96 @this, UInt96 factor, UInt96 subtrahend)` |  |
| `FusedMultiplySubtract` | `static byte FusedMultiplySubtract(this byte @this, byte factor, byte subtrahend)` |  |
| `FusedMultiplySubtract` | `static decimal FusedMultiplySubtract(this decimal @this, decimal factor, decimal subtrahend)` |  |
| `FusedMultiplySubtract` | `static double FusedMultiplySubtract(this double @this, double factor, double subtrahend)` |  |
| `FusedMultiplySubtract` | `static float FusedMultiplySubtract(this float @this, float factor, float subtrahend)` |  |
| `FusedMultiplySubtract` | `static int FusedMultiplySubtract(this int @this, int factor, int subtrahend)` |  |
| `FusedMultiplySubtract` | `static long FusedMultiplySubtract(this long @this, long factor, long subtrahend)` |  |
| `FusedMultiplySubtract` | `static sbyte FusedMultiplySubtract(this sbyte @this, sbyte factor, sbyte subtrahend)` |  |
| `FusedMultiplySubtract` | `static short FusedMultiplySubtract(this short @this, short factor, short subtrahend)` |  |
| `FusedMultiplySubtract` | `static uint FusedMultiplySubtract(this uint @this, uint factor, uint subtrahend)` |  |
| `FusedMultiplySubtract` | `static ulong FusedMultiplySubtract(this ulong @this, ulong factor, ulong subtrahend)` |  |
| `FusedMultiplySubtract` | `static ushort FusedMultiplySubtract(this ushort @this, ushort factor, ushort subtrahend)` |  |
| `GetBit` | `static bool GetBit(this Int128 @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this Int96 @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this UInt128 @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this UInt96 @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this byte @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this uint @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this ulong @this, byte index)` |  |
| `GetBit` | `static bool GetBit(this ushort @this, byte index)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this Int128 @this, Int128 inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this Int96 @this, Int96 inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this UInt128 @this, UInt128 inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this UInt96 @this, UInt96 inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this byte @this, byte inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this decimal @this, decimal inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this double @this, double inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this float @this, float inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this int @this, int inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this long @this, long inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this sbyte @this, sbyte inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this short @this, short inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this uint @this, uint inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this ulong @this, ulong inclusiveLimit)` |  |
| `IsAboveOrEqual` | `static bool IsAboveOrEqual(this ushort @this, ushort inclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this Int128 @this, Int128 exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this Int96 @this, Int96 exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this UInt128 @this, UInt128 exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this UInt96 @this, UInt96 exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this byte @this, byte exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this decimal @this, decimal exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this double @this, double exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this float @this, float exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this int @this, int exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this long @this, long exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this sbyte @this, sbyte exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this short @this, short exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this uint @this, uint exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this ulong @this, ulong exclusiveLimit)` |  |
| `IsAbove` | `static bool IsAbove(this ushort @this, ushort exclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this Int128 @this, Int128 inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this Int96 @this, Int96 inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this UInt128 @this, UInt128 inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this UInt96 @this, UInt96 inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this byte @this, byte inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this decimal @this, decimal inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this double @this, double inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this float @this, float inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this int @this, int inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this long @this, long inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this sbyte @this, sbyte inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this short @this, short inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this uint @this, uint inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this ulong @this, ulong inclusiveLimit)` |  |
| `IsBelowOrEqual` | `static bool IsBelowOrEqual(this ushort @this, ushort inclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this Int128 @this, Int128 exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this Int96 @this, Int96 exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this UInt128 @this, UInt128 exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this UInt96 @this, UInt96 exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this byte @this, byte exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this decimal @this, decimal exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this double @this, double exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this float @this, float exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this int @this, int exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this long @this, long exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this sbyte @this, sbyte exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this short @this, short exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this uint @this, uint exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this ulong @this, ulong exclusiveLimit)` |  |
| `IsBelow` | `static bool IsBelow(this ushort @this, ushort exclusiveLimit)` |  |
| `IsBetween` | `static bool IsBetween(this Int128 @this, Int128 exclusiveLowerLimit, Int128 exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this Int96 @this, Int96 exclusiveLowerLimit, Int96 exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this UInt128 @this, UInt128 exclusiveLowerLimit, UInt128 exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this UInt96 @this, UInt96 exclusiveLowerLimit, UInt96 exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this byte @this, byte exclusiveLowerLimit, byte exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this decimal @this, decimal exclusiveLowerLimit, decimal exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this double @this, double exclusiveLowerLimit, double exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this float @this, float exclusiveLowerLimit, float exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this int @this, int exclusiveLowerLimit, int exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this long @this, long exclusiveLowerLimit, long exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this sbyte @this, sbyte exclusiveLowerLimit, sbyte exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this short @this, short exclusiveLowerLimit, short exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this uint @this, uint exclusiveLowerLimit, uint exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this ulong @this, ulong exclusiveLowerLimit, ulong exclusiveUpperLimit)` |  |
| `IsBetween` | `static bool IsBetween(this ushort @this, ushort exclusiveLowerLimit, ushort exclusiveUpperLimit)` |  |
| `IsEven` | `static bool IsEven(this Int128 @this)` |  |
| `IsEven` | `static bool IsEven(this Int96 @this)` |  |
| `IsEven` | `static bool IsEven(this UInt128 @this)` |  |
| `IsEven` | `static bool IsEven(this UInt96 @this)` |  |
| `IsEven` | `static bool IsEven(this byte @this)` |  |
| `IsEven` | `static bool IsEven(this decimal @this)` |  |
| `IsEven` | `static bool IsEven(this double @this)` |  |
| `IsEven` | `static bool IsEven(this float @this)` |  |
| `IsEven` | `static bool IsEven(this int @this)` |  |
| `IsEven` | `static bool IsEven(this long @this)` |  |
| `IsEven` | `static bool IsEven(this sbyte @this)` |  |
| `IsEven` | `static bool IsEven(this short @this)` |  |
| `IsEven` | `static bool IsEven(this uint @this)` |  |
| `IsEven` | `static bool IsEven(this ulong @this)` |  |
| `IsEven` | `static bool IsEven(this ushort @this)` |  |
| `IsInRange` | `static bool IsInRange(this Int128 @this, Int128 inclusiveLowerLimit, Int128 inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this Int96 @this, Int96 inclusiveLowerLimit, Int96 inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this UInt128 @this, UInt128 inclusiveLowerLimit, UInt128 inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this UInt96 @this, UInt96 inclusiveLowerLimit, UInt96 inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this byte @this, byte inclusiveLowerLimit, byte inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this decimal @this, decimal inclusiveLowerLimit, decimal inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this double @this, double inclusiveLowerLimit, double inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this float @this, float inclusiveLowerLimit, float inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this int @this, int inclusiveLowerLimit, int inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this long @this, long inclusiveLowerLimit, long inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this sbyte @this, sbyte inclusiveLowerLimit, sbyte inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this short @this, short inclusiveLowerLimit, short inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this uint @this, uint inclusiveLowerLimit, uint inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this ulong @this, ulong inclusiveLowerLimit, ulong inclusiveUpperLimit)` |  |
| `IsInRange` | `static bool IsInRange(this ushort @this, ushort inclusiveLowerLimit, ushort inclusiveUpperLimit)` |  |
| `IsIn` | `static bool IsIn(byte @this, byte[] values)` |  |
| `IsIn` | `static bool IsIn(decimal @this, decimal[] values)` |  |
| `IsIn` | `static bool IsIn(double @this, double[] values)` |  |
| `IsIn` | `static bool IsIn(float @this, float[] values)` |  |
| `IsIn` | `static bool IsIn(int @this, int[] values)` |  |
| `IsIn` | `static bool IsIn(long @this, long[] values)` |  |
| `IsIn` | `static bool IsIn(sbyte @this, sbyte[] values)` |  |
| `IsIn` | `static bool IsIn(short @this, short[] values)` |  |
| `IsIn` | `static bool IsIn(uint @this, uint[] values)` |  |
| `IsIn` | `static bool IsIn(ulong @this, ulong[] values)` |  |
| `IsIn` | `static bool IsIn(ushort @this, ushort[] values)` |  |
| `IsInfinity` | `static bool IsInfinity(this double @this)` |  |
| `IsInfinity` | `static bool IsInfinity(this float @this)` |  |
| `IsNaN` | `static bool IsNaN(this double @this)` |  |
| `IsNaN` | `static bool IsNaN(this float @this)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(this double @this)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(this float @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this Int128 @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this Int96 @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this decimal @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this double @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this float @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this int @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this long @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this sbyte @this)` |  |
| `IsNegativeOrZero` | `static bool IsNegativeOrZero(this short @this)` |  |
| `IsNegative` | `static bool IsNegative(this Int128 @this)` |  |
| `IsNegative` | `static bool IsNegative(this Int96 @this)` |  |
| `IsNegative` | `static bool IsNegative(this decimal @this)` |  |
| `IsNegative` | `static bool IsNegative(this double @this)` |  |
| `IsNegative` | `static bool IsNegative(this float @this)` |  |
| `IsNegative` | `static bool IsNegative(this int @this)` |  |
| `IsNegative` | `static bool IsNegative(this long @this)` |  |
| `IsNegative` | `static bool IsNegative(this sbyte @this)` |  |
| `IsNegative` | `static bool IsNegative(this short @this)` |  |
| `IsNonNumeric` | `static bool IsNonNumeric(this double @this)` |  |
| `IsNonNumeric` | `static bool IsNonNumeric(this float @this)` |  |
| `IsNotIn` | `static bool IsNotIn(byte @this, byte[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(decimal @this, decimal[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(double @this, double[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(float @this, float[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(int @this, int[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(long @this, long[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(sbyte @this, sbyte[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(short @this, short[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(uint @this, uint[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(ulong @this, ulong[] values)` |  |
| `IsNotIn` | `static bool IsNotIn(ushort @this, ushort[] values)` |  |
| `IsNotZero` | `static bool IsNotZero(this Int128 @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this Int96 @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this UInt128 @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this UInt96 @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this byte @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this decimal @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this double @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this float @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this int @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this long @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this sbyte @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this short @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this uint @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this ulong @this)` |  |
| `IsNotZero` | `static bool IsNotZero(this ushort @this)` |  |
| `IsNumeric` | `static bool IsNumeric(this double @this)` |  |
| `IsNumeric` | `static bool IsNumeric(this float @this)` |  |
| `IsOdd` | `static bool IsOdd(this Int128 @this)` |  |
| `IsOdd` | `static bool IsOdd(this Int96 @this)` |  |
| `IsOdd` | `static bool IsOdd(this UInt128 @this)` |  |
| `IsOdd` | `static bool IsOdd(this UInt96 @this)` |  |
| `IsOdd` | `static bool IsOdd(this byte @this)` |  |
| `IsOdd` | `static bool IsOdd(this decimal @this)` |  |
| `IsOdd` | `static bool IsOdd(this double @this)` |  |
| `IsOdd` | `static bool IsOdd(this float @this)` |  |
| `IsOdd` | `static bool IsOdd(this int @this)` |  |
| `IsOdd` | `static bool IsOdd(this long @this)` |  |
| `IsOdd` | `static bool IsOdd(this sbyte @this)` |  |
| `IsOdd` | `static bool IsOdd(this short @this)` |  |
| `IsOdd` | `static bool IsOdd(this uint @this)` |  |
| `IsOdd` | `static bool IsOdd(this ulong @this)` |  |
| `IsOdd` | `static bool IsOdd(this ushort @this)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(this double @this)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(this float @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this Int128 @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this Int96 @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this decimal @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this double @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this float @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this int @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this long @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this sbyte @this)` |  |
| `IsPositiveOrZero` | `static bool IsPositiveOrZero(this short @this)` |  |
| `IsPositive` | `static bool IsPositive(this Int128 @this)` |  |
| `IsPositive` | `static bool IsPositive(this Int96 @this)` |  |
| `IsPositive` | `static bool IsPositive(this decimal @this)` |  |
| `IsPositive` | `static bool IsPositive(this double @this)` |  |
| `IsPositive` | `static bool IsPositive(this float @this)` |  |
| `IsPositive` | `static bool IsPositive(this int @this)` |  |
| `IsPositive` | `static bool IsPositive(this long @this)` |  |
| `IsPositive` | `static bool IsPositive(this sbyte @this)` |  |
| `IsPositive` | `static bool IsPositive(this short @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this Int128 @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this Int96 @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this UInt128 @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this UInt96 @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this byte @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this uint @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this ulong @this)` |  |
| `IsPowerOfTwo` | `static bool IsPowerOfTwo(this ushort @this)` |  |
| `IsPrime` | `static bool IsPrime(this ulong candidate)` |  |
| `IsZero` | `static bool IsZero(this Int128 @this)` |  |
| `IsZero` | `static bool IsZero(this Int96 @this)` |  |
| `IsZero` | `static bool IsZero(this UInt128 @this)` |  |
| `IsZero` | `static bool IsZero(this UInt96 @this)` |  |
| `IsZero` | `static bool IsZero(this byte @this)` |  |
| `IsZero` | `static bool IsZero(this decimal @this)` |  |
| `IsZero` | `static bool IsZero(this double @this)` |  |
| `IsZero` | `static bool IsZero(this float @this)` |  |
| `IsZero` | `static bool IsZero(this int @this)` |  |
| `IsZero` | `static bool IsZero(this long @this)` |  |
| `IsZero` | `static bool IsZero(this sbyte @this)` |  |
| `IsZero` | `static bool IsZero(this short @this)` |  |
| `IsZero` | `static bool IsZero(this uint @this)` |  |
| `IsZero` | `static bool IsZero(this ulong @this)` |  |
| `IsZero` | `static bool IsZero(this ushort @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this Int128 @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this Int96 @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this UInt128 @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this UInt96 @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this byte @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this uint @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this ulong @this)` |  |
| `LeadingOneCount` | `static byte LeadingOneCount(this ushort @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this Int128 @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this Int96 @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this UInt128 @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this UInt96 @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this byte @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this uint @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this ulong @this)` |  |
| `LeadingZeroCount` | `static byte LeadingZeroCount(this ushort @this)` |  |
| `LerpUnclamped` | `static byte LerpUnclamped(this byte @this, byte b, double t)` |  |
| `LerpUnclamped` | `static byte LerpUnclamped(this byte @this, byte b, float t)` |  |
| `LerpUnclamped` | `static int LerpUnclamped(this int @this, int b, double t)` |  |
| `LerpUnclamped` | `static int LerpUnclamped(this int @this, int b, float t)` |  |
| `LerpUnclamped` | `static long LerpUnclamped(this long @this, long b, double t)` |  |
| `LerpUnclamped` | `static long LerpUnclamped(this long @this, long b, float t)` |  |
| `LerpUnclamped` | `static sbyte LerpUnclamped(this sbyte @this, sbyte b, double t)` |  |
| `LerpUnclamped` | `static sbyte LerpUnclamped(this sbyte @this, sbyte b, float t)` |  |
| `LerpUnclamped` | `static short LerpUnclamped(this short @this, short b, double t)` |  |
| `LerpUnclamped` | `static short LerpUnclamped(this short @this, short b, float t)` |  |
| `LerpUnclamped` | `static uint LerpUnclamped(this uint @this, uint b, double t)` |  |
| `LerpUnclamped` | `static uint LerpUnclamped(this uint @this, uint b, float t)` |  |
| `LerpUnclamped` | `static ulong LerpUnclamped(this ulong @this, ulong b, double t)` |  |
| `LerpUnclamped` | `static ulong LerpUnclamped(this ulong @this, ulong b, float t)` |  |
| `LerpUnclamped` | `static ushort LerpUnclamped(this ushort @this, ushort b, double t)` |  |
| `LerpUnclamped` | `static ushort LerpUnclamped(this ushort @this, ushort b, float t)` |  |
| `Lerp` | `static byte Lerp(this byte @this, byte b, byte t)` |  |
| `Lerp` | `static byte Lerp(this byte @this, byte b, double t)` |  |
| `Lerp` | `static byte Lerp(this byte @this, byte b, float t)` |  |
| `Lerp` | `static int Lerp(this int @this, int b, double t)` |  |
| `Lerp` | `static int Lerp(this int @this, int b, float t)` |  |
| `Lerp` | `static long Lerp(this long @this, long b, double t)` |  |
| `Lerp` | `static long Lerp(this long @this, long b, float t)` |  |
| `Lerp` | `static sbyte Lerp(this sbyte @this, sbyte b, double t)` |  |
| `Lerp` | `static sbyte Lerp(this sbyte @this, sbyte b, float t)` |  |
| `Lerp` | `static short Lerp(this short @this, short b, double t)` |  |
| `Lerp` | `static short Lerp(this short @this, short b, float t)` |  |
| `Lerp` | `static uint Lerp(this uint @this, uint b, double t)` |  |
| `Lerp` | `static uint Lerp(this uint @this, uint b, float t)` |  |
| `Lerp` | `static uint Lerp(this uint @this, uint b, uint t)` |  |
| `Lerp` | `static ulong Lerp(this ulong @this, ulong b, double t)` |  |
| `Lerp` | `static ulong Lerp(this ulong @this, ulong b, float t)` |  |
| `Lerp` | `static ulong Lerp(this ulong @this, ulong b, ulong t)` |  |
| `Lerp` | `static ushort Lerp(this ushort @this, ushort b, double t)` |  |
| `Lerp` | `static ushort Lerp(this ushort @this, ushort b, float t)` |  |
| `Lerp` | `static ushort Lerp(this ushort @this, ushort b, ushort t)` |  |
| `Log10` | `static decimal Log10(this decimal @this, decimal epsilon)` |  |
| `Log10` | `static double Log10(this double @this)` |  |
| `Log10` | `static float Log10(this float @this)` |  |
| `Log2` | `static decimal Log2(this decimal @this, decimal epsilon = 0)` |  |
| `Log2` | `static double Log2(this double @this)` |  |
| `Log2` | `static float Log2(this float @this)` |  |
| `Log2` | `static int Log2(this Int128 @this)` |  |
| `Log2` | `static int Log2(this Int96 @this)` |  |
| `Log2` | `static int Log2(this UInt128 @this)` |  |
| `Log2` | `static int Log2(this UInt96 @this)` |  |
| `Log2` | `static int Log2(this byte @this)` |  |
| `Log2` | `static int Log2(this uint @this)` |  |
| `Log2` | `static int Log2(this ulong @this)` |  |
| `Log2` | `static int Log2(this ushort @this)` |  |
| `LogN` | `static Half LogN(this Half @this, Half @base)` |  |
| `LogN` | `static decimal LogN(this decimal @this, decimal @base, decimal epsilon = 0)` |  |
| `LogN` | `static double LogN(this double @this, double @base)` |  |
| `LogN` | `static float LogN(this float @this, float @base)` |  |
| `Log` | `static decimal Log(this decimal @this, decimal epsilon)` |  |
| `Log` | `static double Log(this double @this)` |  |
| `Log` | `static float Log(this float @this)` |  |
| `LogicalShiftLeft` | `static Int128 LogicalShiftLeft(this Int128 @this, byte count)` |  |
| `LogicalShiftLeft` | `static Int96 LogicalShiftLeft(this Int96 @this, byte count)` |  |
| `LogicalShiftLeft` | `static UInt128 LogicalShiftLeft(this UInt128 @this, byte count)` |  |
| `LogicalShiftLeft` | `static UInt96 LogicalShiftLeft(this UInt96 @this, byte count)` |  |
| `LogicalShiftLeft` | `static int LogicalShiftLeft(this int @this, byte count)` |  |
| `LogicalShiftLeft` | `static long LogicalShiftLeft(this long @this, byte count)` |  |
| `LogicalShiftLeft` | `static sbyte LogicalShiftLeft(this sbyte @this, byte count)` |  |
| `LogicalShiftLeft` | `static short LogicalShiftLeft(this short @this, byte count)` |  |
| `LogicalShiftRight` | `static Int128 LogicalShiftRight(this Int128 @this, byte count)` |  |
| `LogicalShiftRight` | `static Int96 LogicalShiftRight(this Int96 @this, byte count)` |  |
| `LogicalShiftRight` | `static UInt128 LogicalShiftRight(this UInt128 @this, byte count)` |  |
| `LogicalShiftRight` | `static UInt96 LogicalShiftRight(this UInt96 @this, byte count)` |  |
| `LogicalShiftRight` | `static int LogicalShiftRight(this int @this, byte count)` |  |
| `LogicalShiftRight` | `static long LogicalShiftRight(this long @this, byte count)` |  |
| `LogicalShiftRight` | `static sbyte LogicalShiftRight(this sbyte @this, byte count)` |  |
| `LogicalShiftRight` | `static short LogicalShiftRight(this short @this, byte count)` |  |
| `LowerHalf` | `static byte LowerHalf(this byte @this)` |  |
| `LowerHalf` | `static byte LowerHalf(this ushort @this)` |  |
| `LowerHalf` | `static uint LowerHalf(this ulong @this)` |  |
| `LowerHalf` | `static ushort LowerHalf(this uint @this)` |  |
| `Max` | `static byte Max(params byte[] values)` |  |
| `Max` | `static decimal Max(params decimal[] values)` |  |
| `Max` | `static double Max(params double[] values)` |  |
| `Max` | `static float Max(params float[] values)` |  |
| `Max` | `static int Max(params int[] values)` |  |
| `Max` | `static long Max(params long[] values)` |  |
| `Max` | `static sbyte Max(params sbyte[] values)` |  |
| `Max` | `static short Max(params short[] values)` |  |
| `Max` | `static uint Max(params uint[] values)` |  |
| `Max` | `static ulong Max(params ulong[] values)` |  |
| `Max` | `static ushort Max(params ushort[] values)` |  |
| `Min` | `static byte Min(params byte[] values)` |  |
| `Min` | `static decimal Min(params decimal[] values)` |  |
| `Min` | `static double Min(params double[] values)` |  |
| `Min` | `static float Min(params float[] values)` |  |
| `Min` | `static int Min(params int[] values)` |  |
| `Min` | `static long Min(params long[] values)` |  |
| `Min` | `static sbyte Min(params sbyte[] values)` |  |
| `Min` | `static short Min(params short[] values)` |  |
| `Min` | `static uint Min(params uint[] values)` |  |
| `Min` | `static ulong Min(params ulong[] values)` |  |
| `Min` | `static ushort Min(params ushort[] values)` |  |
| `MultipliedWith` | `static Int128 MultipliedWith(this Int128 @this, Int128 factor)` |  |
| `MultipliedWith` | `static Int96 MultipliedWith(this Int96 @this, Int96 factor)` |  |
| `MultipliedWith` | `static UInt128 MultipliedWith(this UInt128 @this, UInt128 factor)` |  |
| `MultipliedWith` | `static UInt96 MultipliedWith(this UInt96 @this, UInt96 factor)` |  |
| `MultipliedWith` | `static byte MultipliedWith(this byte @this, byte factor)` |  |
| `MultipliedWith` | `static decimal MultipliedWith(this decimal @this, decimal factor)` |  |
| `MultipliedWith` | `static double MultipliedWith(this double @this, double factor)` |  |
| `MultipliedWith` | `static float MultipliedWith(this float @this, float factor)` |  |
| `MultipliedWith` | `static int MultipliedWith(this int @this, int factor)` |  |
| `MultipliedWith` | `static long MultipliedWith(this long @this, long factor)` |  |
| `MultipliedWith` | `static sbyte MultipliedWith(this sbyte @this, sbyte factor)` |  |
| `MultipliedWith` | `static short MultipliedWith(this short @this, short factor)` |  |
| `MultipliedWith` | `static uint MultipliedWith(this uint @this, uint factor)` |  |
| `MultipliedWith` | `static ulong MultipliedWith(this ulong @this, ulong factor)` |  |
| `MultipliedWith` | `static ushort MultipliedWith(this ushort @this, ushort factor)` |  |
| `Nand` | `static Int128 Nand(this Int128 @this, Int128 other)` |  |
| `Nand` | `static Int96 Nand(this Int96 @this, Int96 other)` |  |
| `Nand` | `static UInt128 Nand(this UInt128 @this, UInt128 other)` |  |
| `Nand` | `static UInt96 Nand(this UInt96 @this, UInt96 other)` |  |
| `Nand` | `static byte Nand(this byte @this, byte other)` |  |
| `Nand` | `static int Nand(this int @this, int other)` |  |
| `Nand` | `static long Nand(this long @this, long other)` |  |
| `Nand` | `static sbyte Nand(this sbyte @this, sbyte other)` |  |
| `Nand` | `static short Nand(this short @this, short other)` |  |
| `Nand` | `static uint Nand(this uint @this, uint other)` |  |
| `Nand` | `static ulong Nand(this ulong @this, ulong other)` |  |
| `Nand` | `static ushort Nand(this ushort @this, ushort other)` |  |
| `Nor` | `static Int128 Nor(this Int128 @this, Int128 other)` |  |
| `Nor` | `static Int96 Nor(this Int96 @this, Int96 other)` |  |
| `Nor` | `static UInt128 Nor(this UInt128 @this, UInt128 other)` |  |
| `Nor` | `static UInt96 Nor(this UInt96 @this, UInt96 other)` |  |
| `Nor` | `static byte Nor(this byte @this, byte other)` |  |
| `Nor` | `static int Nor(this int @this, int other)` |  |
| `Nor` | `static long Nor(this long @this, long other)` |  |
| `Nor` | `static sbyte Nor(this sbyte @this, sbyte other)` |  |
| `Nor` | `static short Nor(this short @this, short other)` |  |
| `Nor` | `static uint Nor(this uint @this, uint other)` |  |
| `Nor` | `static ulong Nor(this ulong @this, ulong other)` |  |
| `Nor` | `static ushort Nor(this ushort @this, ushort other)` |  |
| `Not` | `static Int128 Not(this Int128 @this)` |  |
| `Not` | `static Int96 Not(this Int96 @this)` |  |
| `Not` | `static UInt128 Not(this UInt128 @this)` |  |
| `Not` | `static UInt96 Not(this UInt96 @this)` |  |
| `Not` | `static byte Not(this byte @this)` |  |
| `Not` | `static int Not(this int @this)` |  |
| `Not` | `static long Not(this long @this)` |  |
| `Not` | `static sbyte Not(this sbyte @this)` |  |
| `Not` | `static short Not(this short @this)` |  |
| `Not` | `static uint Not(this uint @this)` |  |
| `Not` | `static ulong Not(this ulong @this)` |  |
| `Not` | `static ushort Not(this ushort @this)` |  |
| `Or` | `static Int128 Or(this Int128 @this, Int128 other)` |  |
| `Or` | `static Int96 Or(this Int96 @this, Int96 other)` |  |
| `Or` | `static UInt128 Or(this UInt128 @this, UInt128 other)` |  |
| `Or` | `static UInt96 Or(this UInt96 @this, UInt96 other)` |  |
| `Or` | `static byte Or(this byte @this, byte other)` |  |
| `Or` | `static int Or(this int @this, int other)` |  |
| `Or` | `static long Or(this long @this, long other)` |  |
| `Or` | `static sbyte Or(this sbyte @this, sbyte other)` |  |
| `Or` | `static short Or(this short @this, short other)` |  |
| `Or` | `static uint Or(this uint @this, uint other)` |  |
| `Or` | `static ulong Or(this ulong @this, ulong other)` |  |
| `Or` | `static ushort Or(this ushort @this, ushort other)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<byte, byte> PairwiseDeinterleaveBits(this byte @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<byte, byte> PairwiseDeinterleaveBits(this ushort @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<uint, uint> PairwiseDeinterleaveBits(this ulong @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<ulong, ulong> PairwiseDeinterleaveBits(this Int128 @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<ulong, ulong> PairwiseDeinterleaveBits(this Int96 @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<ulong, ulong> PairwiseDeinterleaveBits(this UInt128 @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<ulong, ulong> PairwiseDeinterleaveBits(this UInt96 @this)` |  |
| `PairwiseDeinterleaveBits` | `static ValueTuple<ushort, ushort> PairwiseDeinterleaveBits(this uint @this)` |  |
| `ParallelBitExtract` | `static Int128 ParallelBitExtract(this Int128 @this, Int128 mask)` |  |
| `ParallelBitExtract` | `static Int96 ParallelBitExtract(this Int96 @this, Int96 mask)` |  |
| `ParallelBitExtract` | `static UInt128 ParallelBitExtract(this UInt128 @this, UInt128 mask)` |  |
| `ParallelBitExtract` | `static UInt96 ParallelBitExtract(this UInt96 @this, UInt96 mask)` |  |
| `ParallelBitExtract` | `static byte ParallelBitExtract(this byte @this, byte mask)` |  |
| `ParallelBitExtract` | `static uint ParallelBitExtract(this uint @this, uint mask)` |  |
| `ParallelBitExtract` | `static ulong ParallelBitExtract(this ulong @this, ulong mask)` |  |
| `ParallelBitExtract` | `static ushort ParallelBitExtract(this ushort @this, ushort mask)` |  |
| `Parity` | `static bool Parity(this Int128 value)` |  |
| `Parity` | `static bool Parity(this Int96 value)` |  |
| `Parity` | `static bool Parity(this UInt128 value)` |  |
| `Parity` | `static bool Parity(this UInt96 value)` |  |
| `Parity` | `static bool Parity(this byte value)` |  |
| `Parity` | `static bool Parity(this uint value)` |  |
| `Parity` | `static bool Parity(this ulong value)` |  |
| `Parity` | `static bool Parity(this ushort value)` |  |
| `Pow` | `static Int128 Pow(this Int128 @this, uint exponent)` |  |
| `Pow` | `static Int96 Pow(this Int96 @this, uint exponent)` |  |
| `Pow` | `static UInt128 Pow(this UInt128 @this, uint exponent)` |  |
| `Pow` | `static UInt96 Pow(this UInt96 @this, uint exponent)` |  |
| `Pow` | `static byte Pow(this byte @this, uint exponent)` |  |
| `Pow` | `static decimal Pow(this decimal @this, decimal exponent, decimal epsilon)` |  |
| `Pow` | `static double Pow(this double @this, double exponent)` |  |
| `Pow` | `static float Pow(this float @this, float exponent)` |  |
| `Pow` | `static int Pow(this int @this, uint exponent)` |  |
| `Pow` | `static long Pow(this long @this, uint exponent)` |  |
| `Pow` | `static sbyte Pow(this sbyte @this, uint exponent)` |  |
| `Pow` | `static short Pow(this short @this, uint exponent)` |  |
| `Pow` | `static uint Pow(this uint @this, uint exponent)` |  |
| `Pow` | `static ulong Pow(this ulong @this, uint exponent)` |  |
| `Pow` | `static ushort Pow(this ushort @this, uint exponent)` |  |
| `ReciprocalEstimate` | `static decimal ReciprocalEstimate(this decimal @this)` |  |
| `ReciprocalEstimate` | `static double ReciprocalEstimate(this double @this)` |  |
| `ReciprocalEstimate` | `static float ReciprocalEstimate(this float @this)` |  |
| `ReverseBits` | `static Int128 ReverseBits(this Int128 @this)` |  |
| `ReverseBits` | `static Int96 ReverseBits(this Int96 @this)` |  |
| `ReverseBits` | `static UInt128 ReverseBits(this UInt128 @this)` |  |
| `ReverseBits` | `static UInt96 ReverseBits(this UInt96 @this)` |  |
| `ReverseBits` | `static byte ReverseBits(this byte @this)` |  |
| `ReverseBits` | `static uint ReverseBits(this uint @this)` |  |
| `ReverseBits` | `static ulong ReverseBits(this ulong @this)` |  |
| `ReverseBits` | `static ushort ReverseBits(this ushort @this)` |  |
| `RotateLeft` | `static UInt128 RotateLeft(this UInt128 @this, byte count)` |  |
| `RotateLeft` | `static UInt96 RotateLeft(this UInt96 @this, byte count)` |  |
| `RotateLeft` | `static byte RotateLeft(this byte @this, byte count)` |  |
| `RotateLeft` | `static uint RotateLeft(this uint @this, byte count)` |  |
| `RotateLeft` | `static ulong RotateLeft(this ulong @this, byte count)` |  |
| `RotateLeft` | `static ushort RotateLeft(this ushort @this, byte count)` |  |
| `RotateRight` | `static UInt128 RotateRight(this UInt128 @this, byte count)` |  |
| `RotateRight` | `static UInt96 RotateRight(this UInt96 @this, byte count)` |  |
| `RotateRight` | `static byte RotateRight(this byte @this, byte count)` |  |
| `RotateRight` | `static uint RotateRight(this uint @this, byte count)` |  |
| `RotateRight` | `static ulong RotateRight(this ulong @this, byte count)` |  |
| `RotateRight` | `static ushort RotateRight(this ushort @this, byte count)` |  |
| `Round` | `static Half Round(this Half @this)` |  |
| `Round` | `static Half Round(this Half @this, MidpointRounding method)` |  |
| `Round` | `static Half Round(this Half @this, int digits)` |  |
| `Round` | `static Half Round(this Half @this, int digits, MidpointRounding method)` |  |
| `Round` | `static decimal Round(this decimal @this)` |  |
| `Round` | `static decimal Round(this decimal @this, MidpointRounding method)` |  |
| `Round` | `static decimal Round(this decimal @this, int digits)` |  |
| `Round` | `static decimal Round(this decimal @this, int digits, MidpointRounding method)` |  |
| `Round` | `static double Round(this double @this)` |  |
| `Round` | `static double Round(this double @this, MidpointRounding method)` |  |
| `Round` | `static double Round(this double @this, int digits)` |  |
| `Round` | `static double Round(this double @this, int digits, MidpointRounding method)` |  |
| `Round` | `static float Round(this float @this)` |  |
| `Round` | `static float Round(this float @this, MidpointRounding method)` |  |
| `Round` | `static float Round(this float @this, int digits)` |  |
| `Round` | `static float Round(this float @this, int digits, MidpointRounding method)` |  |
| `SaturatingAdd` | `static Int128 SaturatingAdd(this Int128 @this, Int128 value)` |  |
| `SaturatingAdd` | `static Int96 SaturatingAdd(this Int96 @this, Int96 value)` |  |
| `SaturatingAdd` | `static UInt128 SaturatingAdd(this UInt128 @this, UInt128 value)` |  |
| `SaturatingAdd` | `static UInt96 SaturatingAdd(this UInt96 @this, UInt96 value)` |  |
| `SaturatingAdd` | `static byte SaturatingAdd(this byte @this, byte value)` |  |
| `SaturatingAdd` | `static int SaturatingAdd(this int @this, int value)` |  |
| `SaturatingAdd` | `static long SaturatingAdd(this long @this, long value)` |  |
| `SaturatingAdd` | `static sbyte SaturatingAdd(this sbyte @this, sbyte value)` |  |
| `SaturatingAdd` | `static short SaturatingAdd(this short @this, short value)` |  |
| `SaturatingAdd` | `static uint SaturatingAdd(this uint @this, uint value)` |  |
| `SaturatingAdd` | `static ulong SaturatingAdd(this ulong @this, ulong value)` |  |
| `SaturatingAdd` | `static ushort SaturatingAdd(this ushort @this, ushort value)` |  |
| `SaturatingDivide` | `static Int128 SaturatingDivide(this Int128 @this, Int128 value)` |  |
| `SaturatingDivide` | `static Int96 SaturatingDivide(this Int96 @this, Int96 value)` |  |
| `SaturatingDivide` | `static UInt128 SaturatingDivide(this UInt128 @this, UInt128 value)` |  |
| `SaturatingDivide` | `static UInt96 SaturatingDivide(this UInt96 @this, UInt96 value)` |  |
| `SaturatingDivide` | `static byte SaturatingDivide(this byte @this, byte value)` |  |
| `SaturatingDivide` | `static int SaturatingDivide(this int @this, int value)` |  |
| `SaturatingDivide` | `static long SaturatingDivide(this long @this, long value)` |  |
| `SaturatingDivide` | `static sbyte SaturatingDivide(this sbyte @this, sbyte value)` |  |
| `SaturatingDivide` | `static short SaturatingDivide(this short @this, short value)` |  |
| `SaturatingDivide` | `static uint SaturatingDivide(this uint @this, uint value)` |  |
| `SaturatingDivide` | `static ulong SaturatingDivide(this ulong @this, ulong value)` |  |
| `SaturatingDivide` | `static ushort SaturatingDivide(this ushort @this, ushort value)` |  |
| `SaturatingMultiply` | `static Int128 SaturatingMultiply(this Int128 @this, Int128 value)` |  |
| `SaturatingMultiply` | `static Int96 SaturatingMultiply(this Int96 @this, Int96 value)` |  |
| `SaturatingMultiply` | `static UInt128 SaturatingMultiply(this UInt128 @this, UInt128 value)` |  |
| `SaturatingMultiply` | `static UInt96 SaturatingMultiply(this UInt96 @this, UInt96 value)` |  |
| `SaturatingMultiply` | `static byte SaturatingMultiply(this byte @this, byte value)` |  |
| `SaturatingMultiply` | `static int SaturatingMultiply(this int @this, int value)` |  |
| `SaturatingMultiply` | `static long SaturatingMultiply(this long @this, long value)` |  |
| `SaturatingMultiply` | `static sbyte SaturatingMultiply(this sbyte @this, sbyte value)` |  |
| `SaturatingMultiply` | `static short SaturatingMultiply(this short @this, short value)` |  |
| `SaturatingMultiply` | `static uint SaturatingMultiply(this uint @this, uint value)` |  |
| `SaturatingMultiply` | `static ulong SaturatingMultiply(this ulong @this, ulong value)` |  |
| `SaturatingMultiply` | `static ushort SaturatingMultiply(this ushort @this, ushort value)` |  |
| `SaturatingNegate` | `static Int128 SaturatingNegate(this Int128 @this)` |  |
| `SaturatingNegate` | `static Int96 SaturatingNegate(this Int96 @this)` |  |
| `SaturatingNegate` | `static int SaturatingNegate(this int @this)` |  |
| `SaturatingNegate` | `static long SaturatingNegate(this long @this)` |  |
| `SaturatingNegate` | `static sbyte SaturatingNegate(this sbyte @this)` |  |
| `SaturatingNegate` | `static short SaturatingNegate(this short @this)` |  |
| `SaturatingSubtract` | `static Int128 SaturatingSubtract(this Int128 @this, Int128 value)` |  |
| `SaturatingSubtract` | `static Int96 SaturatingSubtract(this Int96 @this, Int96 value)` |  |
| `SaturatingSubtract` | `static UInt128 SaturatingSubtract(this UInt128 @this, UInt128 value)` |  |
| `SaturatingSubtract` | `static UInt96 SaturatingSubtract(this UInt96 @this, UInt96 value)` |  |
| `SaturatingSubtract` | `static byte SaturatingSubtract(this byte @this, byte value)` |  |
| `SaturatingSubtract` | `static int SaturatingSubtract(this int @this, int value)` |  |
| `SaturatingSubtract` | `static long SaturatingSubtract(this long @this, long value)` |  |
| `SaturatingSubtract` | `static sbyte SaturatingSubtract(this sbyte @this, sbyte value)` |  |
| `SaturatingSubtract` | `static short SaturatingSubtract(this short @this, short value)` |  |
| `SaturatingSubtract` | `static uint SaturatingSubtract(this uint @this, uint value)` |  |
| `SaturatingSubtract` | `static ulong SaturatingSubtract(this ulong @this, ulong value)` |  |
| `SaturatingSubtract` | `static ushort SaturatingSubtract(this ushort @this, ushort value)` |  |
| `Sec` | `static Half Sec(this Half @this)` |  |
| `Sec` | `static double Sec(this double @this)` |  |
| `Sec` | `static float Sec(this float @this)` |  |
| `Sech` | `static Half Sech(this Half @this)` |  |
| `Sech` | `static double Sech(this double @this)` |  |
| `Sech` | `static float Sech(this float @this)` |  |
| `SetBit` | `static Int128 SetBit(this Int128 @this, byte index)` |  |
| `SetBit` | `static Int96 SetBit(this Int96 @this, byte index)` |  |
| `SetBit` | `static UInt128 SetBit(this UInt128 @this, byte index)` |  |
| `SetBit` | `static UInt96 SetBit(this UInt96 @this, byte index)` |  |
| `SetBit` | `static byte SetBit(this byte @this, byte index)` |  |
| `SetBit` | `static uint SetBit(this uint @this, byte index)` |  |
| `SetBit` | `static ulong SetBit(this ulong @this, byte index)` |  |
| `SetBit` | `static ushort SetBit(this ushort @this, byte index)` |  |
| `Sign` | `static Int128 Sign(this Int128 @this)` |  |
| `Sign` | `static Int96 Sign(this Int96 @this)` |  |
| `Sign` | `static decimal Sign(this decimal @this)` |  |
| `Sign` | `static double Sign(this double @this)` |  |
| `Sign` | `static float Sign(this float @this)` |  |
| `Sign` | `static int Sign(this int @this)` |  |
| `Sign` | `static long Sign(this long @this)` |  |
| `Sign` | `static sbyte Sign(this sbyte @this)` |  |
| `Sign` | `static short Sign(this short @this)` |  |
| `Sin` | `static decimal Sin(this decimal @this, decimal epsilon)` |  |
| `Sin` | `static double Sin(this double @this)` |  |
| `Sin` | `static float Sin(this float @this)` |  |
| `Sinh` | `static double Sinh(this double @this)` |  |
| `Sinh` | `static float Sinh(this float @this)` |  |
| `Sqrt` | `static decimal Sqrt(this decimal @this, decimal epsilon)` |  |
| `Sqrt` | `static double Sqrt(this double @this)` |  |
| `Sqrt` | `static float Sqrt(this float @this)` |  |
| `Squared` | `static Int128 Squared(this Int128 @this)` |  |
| `Squared` | `static Int96 Squared(this Int96 @this)` |  |
| `Squared` | `static UInt128 Squared(this UInt128 @this)` |  |
| `Squared` | `static UInt96 Squared(this UInt96 @this)` |  |
| `Squared` | `static byte Squared(this byte @this)` |  |
| `Squared` | `static decimal Squared(this decimal @this)` |  |
| `Squared` | `static double Squared(this double @this)` |  |
| `Squared` | `static float Squared(this float @this)` |  |
| `Squared` | `static int Squared(this int @this)` |  |
| `Squared` | `static long Squared(this long @this)` |  |
| `Squared` | `static sbyte Squared(this sbyte @this)` |  |
| `Squared` | `static short Squared(this short @this)` |  |
| `Squared` | `static uint Squared(this uint @this)` |  |
| `Squared` | `static ulong Squared(this ulong @this)` |  |
| `Squared` | `static ushort Squared(this ushort @this)` |  |
| `Subtract` | `static Int128 Subtract(this Int128 @this, Int128 minuend)` |  |
| `Subtract` | `static Int96 Subtract(this Int96 @this, Int96 minuend)` |  |
| `Subtract` | `static UInt128 Subtract(this UInt128 @this, UInt128 minuend)` |  |
| `Subtract` | `static UInt96 Subtract(this UInt96 @this, UInt96 minuend)` |  |
| `Subtract` | `static byte Subtract(this byte @this, byte minuend)` |  |
| `Subtract` | `static decimal Subtract(this decimal @this, decimal minuend)` |  |
| `Subtract` | `static double Subtract(this double @this, double minuend)` |  |
| `Subtract` | `static float Subtract(this float @this, float minuend)` |  |
| `Subtract` | `static int Subtract(this int @this, int minuend)` |  |
| `Subtract` | `static long Subtract(this long @this, long minuend)` |  |
| `Subtract` | `static sbyte Subtract(this sbyte @this, sbyte minuend)` |  |
| `Subtract` | `static short Subtract(this short @this, short minuend)` |  |
| `Subtract` | `static uint Subtract(this uint @this, uint minuend)` |  |
| `Subtract` | `static ulong Subtract(this ulong @this, ulong minuend)` |  |
| `Subtract` | `static ushort Subtract(this ushort @this, ushort minuend)` |  |
| `Tan` | `static decimal Tan(this decimal @this, decimal epsilon)` |  |
| `Tan` | `static double Tan(this double @this)` |  |
| `Tan` | `static float Tan(this float @this)` |  |
| `Tanh` | `static double Tanh(this double @this)` |  |
| `Tanh` | `static float Tanh(this float @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this Int128 @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this Int96 @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this UInt128 @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this UInt96 @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this byte @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this uint @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this ulong @this)` |  |
| `TrailingOneCount` | `static byte TrailingOneCount(this ushort @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this Int128 @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this Int96 @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this UInt128 @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this UInt96 @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this byte @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this uint @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this ulong @this)` |  |
| `TrailingZeroCount` | `static byte TrailingZeroCount(this ushort @this)` |  |
| `Truncate` | `static Half Truncate(this Half @this)` |  |
| `Truncate` | `static decimal Truncate(this decimal @this)` |  |
| `Truncate` | `static double Truncate(this double @this)` |  |
| `Truncate` | `static float Truncate(this float @this)` |  |
| `UpperHalf` | `static byte UpperHalf(this byte @this)` |  |
| `UpperHalf` | `static byte UpperHalf(this ushort @this)` |  |
| `UpperHalf` | `static uint UpperHalf(this ulong @this)` |  |
| `UpperHalf` | `static ushort UpperHalf(this uint @this)` |  |
| `Xor` | `static Int128 Xor(this Int128 @this, Int128 other)` |  |
| `Xor` | `static Int96 Xor(this Int96 @this, Int96 other)` |  |
| `Xor` | `static UInt128 Xor(this UInt128 @this, UInt128 other)` |  |
| `Xor` | `static UInt96 Xor(this UInt96 @this, UInt96 other)` |  |
| `Xor` | `static byte Xor(this byte @this, byte other)` |  |
| `Xor` | `static int Xor(this int @this, int other)` |  |
| `Xor` | `static long Xor(this long @this, long other)` |  |
| `Xor` | `static sbyte Xor(this sbyte @this, sbyte other)` |  |
| `Xor` | `static short Xor(this short @this, short other)` |  |
| `Xor` | `static uint Xor(this uint @this, uint other)` |  |
| `Xor` | `static ulong Xor(this ulong @this, ulong other)` |  |
| `Xor` | `static ushort Xor(this ushort @this, ushort other)` |  |

#### `MidiNote`

Implements `IComparable`, `IComparable<MidiNote>`, `IEquatable<MidiNote>`, `IFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Frequency` | `double Frequency { get; }` |  |
| `NoteName` | `string NoteName { get; }` |  |
| `Number` | `byte Number { get; }` |  |
| `Octave` | `int Octave { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(MidiNote other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(MidiNote other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromNumber` | `static MidiNote FromNumber(int number)` |  |
| `FromRaw` | `static MidiNote FromRaw(byte raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `explicit operator MidiNote` | `static explicit operator MidiNote(byte number)` |  |
| `implicit operator byte` | `static implicit operator byte(MidiNote note)` |  |
| `operator !=` | `static bool operator !=(MidiNote left, MidiNote right)` |  |
| `operator ==` | `static bool operator ==(MidiNote left, MidiNote right)` |  |

#### `MuLaw`

Implements `IComparable`, `IComparable<MuLaw>`, `IEquatable<MuLaw>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `byte RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(MuLaw other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(MuLaw other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromPcm16` | `static MuLaw FromPcm16(short pcm)` |  |
| `FromPcm16` | `static MuLaw FromPcm16<TConvention>(short pcm)` |  |
| `FromRaw` | `static MuLaw FromRaw(byte raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToPcm16` | `short ToPcm16()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator MuLaw` | `static explicit operator MuLaw(short pcm)` |  |
| `implicit operator short` | `static implicit operator short(MuLaw value)` |  |
| `operator !=` | `static bool operator !=(MuLaw left, MuLaw right)` |  |
| `operator ==` | `static bool operator ==(MuLaw left, MuLaw right)` |  |

#### `NVFP4`

| Member | Signature | Summary |
| --- | --- | --- |
| `BlockSize` | `const int BlockSize` |  |
| `BlockCount` | `int BlockCount { get; }` |  |
| `Item` | `float this[int index] { get; set; }` |  |
| `Length` | `int Length { get; }` |  |
| `PackedData` | `ReadOnlySpan<byte> PackedData { get; }` |  |
| `Scales` | `ReadOnlySpan<E4M3> Scales { get; }` |  |
| `TensorScale` | `float TensorScale { get; }` |  |
| `DecodeBlock` | `int DecodeBlock(int blockIndex, Span<float> destination)` |  |
| `DecodeTo` | `void DecodeTo(Span<float> destination)` |  |
| `Encode` | `static NVFP4 Encode(ReadOnlySpan<float> values)` |  |
| `FromPacked` | `static NVFP4 FromPacked(byte[] packedCodes, E4M3[] scales, float tensorScale, int length)` |  |
| `GetElement` | `E2M1 GetElement(int index)` |  |
| `GetEnumerator` | `IEnumerator<float> GetEnumerator()` |  |
| `GetScale` | `E4M3 GetScale(int blockIndex)` |  |
| `ToArray` | `float[] ToArray()` |  |

#### `NtpTimestamp`

Implements `IComparable`, `IComparable<NtpTimestamp>`, `IEquatable<NtpTimestamp>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Seconds` | `double Seconds { get; }` |  |
| `CompareTo` | `int CompareTo(NtpTimestamp other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(NtpTimestamp other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static NtpTimestamp FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static NtpTimestamp FromRaw(ulong raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(NtpTimestamp value)` |  |
| `operator !=` | `static bool operator !=(NtpTimestamp left, NtpTimestamp right)` |  |
| `operator <=` | `static bool operator <=(NtpTimestamp left, NtpTimestamp right)` |  |
| `operator <` | `static bool operator <(NtpTimestamp left, NtpTimestamp right)` |  |
| `operator ==` | `static bool operator ==(NtpTimestamp left, NtpTimestamp right)` |  |
| `operator >=` | `static bool operator >=(NtpTimestamp left, NtpTimestamp right)` |  |
| `operator >` | `static bool operator >(NtpTimestamp left, NtpTimestamp right)` |  |

#### `NullableEx<TType>`

| Member | Signature | Summary |
| --- | --- | --- |
| `NullableEx` | `NullableEx(TType value)` |  |
| `HasValue` | `bool HasValue { get; }` |  |
| `Value` | `TType Value { get; }` |  |
| `Equals` | `override bool Equals(object other)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetValueOrDefault` | `TType GetValueOrDefault()` |  |
| `GetValueOrDefault` | `TType GetValueOrDefault(TType defaultValue)` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator TType` | `static explicit operator TType(NullableEx<TType> This)` |  |
| `implicit operator NullableEx<TType>` | `static implicit operator NullableEx<TType>(TType value)` |  |

#### `NullableExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IsNotNull` | `static bool IsNotNull<T>(this T? @this)` |  |
| `IsNull` | `static bool IsNull<T>(this T? @this)` |  |

#### `ObjectExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Apply` | `static T Apply<T>(this T @this, Action<T> action)` |  |
| `As` | `static TType As<TType>(this object @this)` |  |
| `FromXmlFile` | `static TItem FromXmlFile<TItem>(FileInfo file)` |  |
| `GetFields` | `static Dictionary<string, object> GetFields(this object @this, bool flattenHierarchy = true, bool allowNonPublic = true, bool specialNames = true, Func<Exception, object> exceptionHandler = null)` |  |
| `GetMemorySize` | `static long GetMemorySize(this object @this)` |  |
| `GetMemorySize` | `static long GetMemorySize<TValue>(this TValue @this)` |  |
| `GetProperties` | `static Dictionary<string, object> GetProperties(this object @this, bool flattenHierarchy = true, bool allowNonPublic = true, bool specialNames = true, Func<Exception, object> exceptionHandler = null)` |  |
| `IsAnyOf` | `static bool IsAnyOf<TType>(this TType @this, IEnumerable<TType> values)` |  |
| `IsAnyOf` | `static bool IsAnyOf<TType>(this TType @this, params TType[] values)` |  |
| `IsFalse` | `static bool IsFalse<TType>(this TType @this, Predicate<TType> condition)` |  |
| `IsNotNull` | `static bool IsNotNull<T>(this T @this)` |  |
| `IsNull` | `static bool IsNull<T>(this T @this)` |  |
| `IsTrue` | `static bool IsTrue<TType>(this TType @this, Predicate<TType> condition)` |  |
| `Is` | `static bool Is<TType>(this object @this)` |  |
| `RepeatAsArray` | `static T[] RepeatAsArray<T>(this T @this, byte count)` |  |
| `RepeatAsArray` | `static T[] RepeatAsArray<T>(this T @this, int count)` |  |
| `RepeatAsArray` | `static T[] RepeatAsArray<T>(this T @this, sbyte count)` |  |
| `RepeatAsArray` | `static T[] RepeatAsArray<T>(this T @this, short count)` |  |
| `RepeatAsArray` | `static T[] RepeatAsArray<T>(this T @this, ushort count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, byte count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, int count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, long count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, sbyte count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, short count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, uint count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, ulong count)` |  |
| `RepeatAsEnumerable` | `static IEnumerable<T> RepeatAsEnumerable<T>(this T @this, ushort count)` |  |
| `ResetDefaultValues` | `static void ResetDefaultValues(this object @this, bool flattenHierarchy = true)` |  |
| `ToXmlFile` | `static void ToXmlFile<TItem>(this TItem @this, FileInfo file)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf(this object @this, params Type[] types)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf<TType1, TType2, TType3, TType4, TType5, TType6>(this object @this)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf<TType1, TType2, TType3, TType4, TType5>(this object @this)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf<TType1, TType2, TType3, TType4>(this object @this)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf<TType1, TType2, TType3>(this object @this)` |  |
| `TypeIsAnyOf` | `static bool TypeIsAnyOf<TType1, TType2>(this object @this)` |  |
| `WhenNotNull` | `static TResult WhenNotNull<TType, TResult>(this TType @this, Func<TType, TResult> function, TResult defaultValue = null)` |  |
| `WhenNotNull` | `static void WhenNotNull<TType>(this TType @this, Action<TType> action)` |  |
| `WhenNull` | `static TResult WhenNull<TType, TResult>(this TType @this, Func<TResult> function, TResult defaultValue = null)` |  |
| `WhenNull` | `static void WhenNull<TType>(this TType @this, Action action)` |  |

#### `OleDate`

Implements `IComparable`, `IComparable<OleDate>`, `IEquatable<OleDate>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Days` | `double Days { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(OleDate other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(OleDate other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static OleDate FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static OleDate FromRaw(ulong raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(OleDate value)` |  |
| `operator !=` | `static bool operator !=(OleDate left, OleDate right)` |  |
| `operator <=` | `static bool operator <=(OleDate left, OleDate right)` |  |
| `operator <` | `static bool operator <(OleDate left, OleDate right)` |  |
| `operator ==` | `static bool operator ==(OleDate left, OleDate right)` |  |
| `operator >=` | `static bool operator >=(OleDate left, OleDate right)` |  |
| `operator >` | `static bool operator >(OleDate left, OleDate right)` |  |

#### `PackedBCD16`

Implements `IComparable`, `IComparable<PackedBCD16>`, `IEquatable<PackedBCD16>`, `IFormattable`, `IParsable<PackedBCD16>`, `ISpanFormattable`, `ISpanParsable<PackedBCD16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static PackedBCD16 MaxValue { get; }` |  |
| `MinValue` | `static PackedBCD16 MinValue { get; }` |  |
| `One` | `static PackedBCD16 One { get; }` |  |
| `RawValue` | `ushort RawValue { get; }` |  |
| `Value` | `int Value { get; }` |  |
| `Zero` | `static PackedBCD16 Zero { get; }` |  |
| `Clamp` | `static PackedBCD16 Clamp(PackedBCD16 value, PackedBCD16 min, PackedBCD16 max)` |  |
| `CompareTo` | `int CompareTo(PackedBCD16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(PackedBCD16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromRaw` | `static PackedBCD16 FromRaw(ushort raw)` |  |
| `FromValue` | `static PackedBCD16 FromValue(int value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static PackedBCD16 Max(PackedBCD16 left, PackedBCD16 right)` |  |
| `Min` | `static PackedBCD16 Min(PackedBCD16 left, PackedBCD16 right)` |  |
| `Parse` | `static PackedBCD16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD16 Parse(string s)` |  |
| `Parse` | `static PackedBCD16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out PackedBCD16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out PackedBCD16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out PackedBCD16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out PackedBCD16 result)` |  |
| `explicit operator PackedBCD8` | `static explicit operator PackedBCD8(PackedBCD16 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(PackedBCD16 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(PackedBCD16 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(PackedBCD16 value)` |  |
| `implicit operator PackedBCD16` | `static implicit operator PackedBCD16(ushort value)` |  |
| `implicit operator PackedBCD32` | `static implicit operator PackedBCD32(PackedBCD16 value)` |  |
| `implicit operator PackedBCD64` | `static implicit operator PackedBCD64(PackedBCD16 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(PackedBCD16 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(PackedBCD16 value)` |  |
| `implicit operator int` | `static implicit operator int(PackedBCD16 value)` |  |
| `implicit operator long` | `static implicit operator long(PackedBCD16 value)` |  |
| `implicit operator uint` | `static implicit operator uint(PackedBCD16 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(PackedBCD16 value)` |  |
| `operator !=` | `static bool operator !=(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator %` | `static PackedBCD16 operator %(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator *` | `static PackedBCD16 operator *(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator ++` | `static PackedBCD16 operator ++(PackedBCD16 value)` |  |
| `operator +` | `static PackedBCD16 operator +(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator --` | `static PackedBCD16 operator --(PackedBCD16 value)` |  |
| `operator -` | `static PackedBCD16 operator -(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator /` | `static PackedBCD16 operator /(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator <=` | `static bool operator <=(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator <` | `static bool operator <(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator ==` | `static bool operator ==(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator >=` | `static bool operator >=(PackedBCD16 left, PackedBCD16 right)` |  |
| `operator >` | `static bool operator >(PackedBCD16 left, PackedBCD16 right)` |  |

#### `PackedBCD32`

Implements `IComparable`, `IComparable<PackedBCD32>`, `IEquatable<PackedBCD32>`, `IFormattable`, `IParsable<PackedBCD32>`, `ISpanFormattable`, `ISpanParsable<PackedBCD32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static PackedBCD32 MaxValue { get; }` |  |
| `MinValue` | `static PackedBCD32 MinValue { get; }` |  |
| `One` | `static PackedBCD32 One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Value` | `int Value { get; }` |  |
| `Zero` | `static PackedBCD32 Zero { get; }` |  |
| `Clamp` | `static PackedBCD32 Clamp(PackedBCD32 value, PackedBCD32 min, PackedBCD32 max)` |  |
| `CompareTo` | `int CompareTo(PackedBCD32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(PackedBCD32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromRaw` | `static PackedBCD32 FromRaw(uint raw)` |  |
| `FromValue` | `static PackedBCD32 FromValue(int value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static PackedBCD32 Max(PackedBCD32 left, PackedBCD32 right)` |  |
| `Min` | `static PackedBCD32 Min(PackedBCD32 left, PackedBCD32 right)` |  |
| `Parse` | `static PackedBCD32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD32 Parse(string s)` |  |
| `Parse` | `static PackedBCD32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out PackedBCD32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out PackedBCD32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out PackedBCD32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out PackedBCD32 result)` |  |
| `explicit operator PackedBCD16` | `static explicit operator PackedBCD16(PackedBCD32 value)` |  |
| `explicit operator PackedBCD8` | `static explicit operator PackedBCD8(PackedBCD32 value)` |  |
| `explicit operator int` | `static explicit operator int(PackedBCD32 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(PackedBCD32 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(PackedBCD32 value)` |  |
| `implicit operator PackedBCD32` | `static implicit operator PackedBCD32(int value)` |  |
| `implicit operator PackedBCD64` | `static implicit operator PackedBCD64(PackedBCD32 value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(PackedBCD32 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(PackedBCD32 value)` |  |
| `implicit operator long` | `static implicit operator long(PackedBCD32 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(PackedBCD32 value)` |  |
| `operator !=` | `static bool operator !=(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator %` | `static PackedBCD32 operator %(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator *` | `static PackedBCD32 operator *(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator ++` | `static PackedBCD32 operator ++(PackedBCD32 value)` |  |
| `operator +` | `static PackedBCD32 operator +(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator --` | `static PackedBCD32 operator --(PackedBCD32 value)` |  |
| `operator -` | `static PackedBCD32 operator -(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator /` | `static PackedBCD32 operator /(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator <=` | `static bool operator <=(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator <` | `static bool operator <(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator ==` | `static bool operator ==(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator >=` | `static bool operator >=(PackedBCD32 left, PackedBCD32 right)` |  |
| `operator >` | `static bool operator >(PackedBCD32 left, PackedBCD32 right)` |  |

#### `PackedBCD64`

Implements `IComparable`, `IComparable<PackedBCD64>`, `IEquatable<PackedBCD64>`, `IFormattable`, `IParsable<PackedBCD64>`, `ISpanFormattable`, `ISpanParsable<PackedBCD64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static PackedBCD64 MaxValue { get; }` |  |
| `MinValue` | `static PackedBCD64 MinValue { get; }` |  |
| `One` | `static PackedBCD64 One { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Value` | `long Value { get; }` |  |
| `Zero` | `static PackedBCD64 Zero { get; }` |  |
| `Clamp` | `static PackedBCD64 Clamp(PackedBCD64 value, PackedBCD64 min, PackedBCD64 max)` |  |
| `CompareTo` | `int CompareTo(PackedBCD64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(PackedBCD64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromRaw` | `static PackedBCD64 FromRaw(ulong raw)` |  |
| `FromValue` | `static PackedBCD64 FromValue(long value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static PackedBCD64 Max(PackedBCD64 left, PackedBCD64 right)` |  |
| `Min` | `static PackedBCD64 Min(PackedBCD64 left, PackedBCD64 right)` |  |
| `Parse` | `static PackedBCD64 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD64 Parse(string s)` |  |
| `Parse` | `static PackedBCD64 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD64 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out PackedBCD64 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out PackedBCD64 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out PackedBCD64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out PackedBCD64 result)` |  |
| `explicit operator PackedBCD16` | `static explicit operator PackedBCD16(PackedBCD64 value)` |  |
| `explicit operator PackedBCD32` | `static explicit operator PackedBCD32(PackedBCD64 value)` |  |
| `explicit operator PackedBCD8` | `static explicit operator PackedBCD8(PackedBCD64 value)` |  |
| `explicit operator long` | `static explicit operator long(PackedBCD64 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(PackedBCD64 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(PackedBCD64 value)` |  |
| `implicit operator PackedBCD64` | `static implicit operator PackedBCD64(long value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(PackedBCD64 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(PackedBCD64 value)` |  |
| `operator !=` | `static bool operator !=(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator %` | `static PackedBCD64 operator %(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator *` | `static PackedBCD64 operator *(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator ++` | `static PackedBCD64 operator ++(PackedBCD64 value)` |  |
| `operator +` | `static PackedBCD64 operator +(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator --` | `static PackedBCD64 operator --(PackedBCD64 value)` |  |
| `operator -` | `static PackedBCD64 operator -(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator /` | `static PackedBCD64 operator /(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator <=` | `static bool operator <=(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator <` | `static bool operator <(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator ==` | `static bool operator ==(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator >=` | `static bool operator >=(PackedBCD64 left, PackedBCD64 right)` |  |
| `operator >` | `static bool operator >(PackedBCD64 left, PackedBCD64 right)` |  |

#### `PackedBCD8`

Implements `IComparable`, `IComparable<PackedBCD8>`, `IEquatable<PackedBCD8>`, `IFormattable`, `IParsable<PackedBCD8>`, `ISpanFormattable`, `ISpanParsable<PackedBCD8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static PackedBCD8 MaxValue { get; }` |  |
| `MinValue` | `static PackedBCD8 MinValue { get; }` |  |
| `One` | `static PackedBCD8 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Value` | `int Value { get; }` |  |
| `Zero` | `static PackedBCD8 Zero { get; }` |  |
| `Clamp` | `static PackedBCD8 Clamp(PackedBCD8 value, PackedBCD8 min, PackedBCD8 max)` |  |
| `CompareTo` | `int CompareTo(PackedBCD8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(PackedBCD8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromRaw` | `static PackedBCD8 FromRaw(byte raw)` |  |
| `FromValue` | `static PackedBCD8 FromValue(int value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static PackedBCD8 Max(PackedBCD8 left, PackedBCD8 right)` |  |
| `Min` | `static PackedBCD8 Min(PackedBCD8 left, PackedBCD8 right)` |  |
| `Parse` | `static PackedBCD8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD8 Parse(string s)` |  |
| `Parse` | `static PackedBCD8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static PackedBCD8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out PackedBCD8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out PackedBCD8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out PackedBCD8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out PackedBCD8 result)` |  |
| `explicit operator byte` | `static explicit operator byte(PackedBCD8 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(PackedBCD8 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(PackedBCD8 value)` |  |
| `implicit operator PackedBCD16` | `static implicit operator PackedBCD16(PackedBCD8 value)` |  |
| `implicit operator PackedBCD32` | `static implicit operator PackedBCD32(PackedBCD8 value)` |  |
| `implicit operator PackedBCD64` | `static implicit operator PackedBCD64(PackedBCD8 value)` |  |
| `implicit operator PackedBCD8` | `static implicit operator PackedBCD8(byte value)` |  |
| `implicit operator UInt128` | `static implicit operator UInt128(PackedBCD8 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(PackedBCD8 value)` |  |
| `implicit operator int` | `static implicit operator int(PackedBCD8 value)` |  |
| `implicit operator long` | `static implicit operator long(PackedBCD8 value)` |  |
| `implicit operator short` | `static implicit operator short(PackedBCD8 value)` |  |
| `implicit operator uint` | `static implicit operator uint(PackedBCD8 value)` |  |
| `implicit operator ulong` | `static implicit operator ulong(PackedBCD8 value)` |  |
| `implicit operator ushort` | `static implicit operator ushort(PackedBCD8 value)` |  |
| `operator !=` | `static bool operator !=(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator %` | `static PackedBCD8 operator %(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator *` | `static PackedBCD8 operator *(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator ++` | `static PackedBCD8 operator ++(PackedBCD8 value)` |  |
| `operator +` | `static PackedBCD8 operator +(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator --` | `static PackedBCD8 operator --(PackedBCD8 value)` |  |
| `operator -` | `static PackedBCD8 operator -(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator /` | `static PackedBCD8 operator /(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator <=` | `static bool operator <=(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator <` | `static bool operator <(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator ==` | `static bool operator ==(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator >=` | `static bool operator >=(PackedBCD8 left, PackedBCD8 right)` |  |
| `operator >` | `static bool operator >(PackedBCD8 left, PackedBCD8 right)` |  |

#### `Posit16`

Implements `IComparable`, `IComparable<Posit16>`, `IEquatable<Posit16>`, `IFormattable`, `IParsable<Posit16>`, `ISpanFormattable`, `ISpanParsable<Posit16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static Posit16 MaxValue { get; }` |  |
| `MinValue` | `static Posit16 MinValue { get; }` |  |
| `NaR` | `static Posit16 NaR { get; }` |  |
| `One` | `static Posit16 One { get; }` |  |
| `RawValue` | `ushort RawValue { get; }` |  |
| `Zero` | `static Posit16 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Posit16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Posit16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Posit16 FromDouble(double value)` |  |
| `FromRaw` | `static Posit16 FromRaw(ushort raw)` |  |
| `FromSingle` | `static Posit16 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Posit16 value)` |  |
| `IsNaR` | `static bool IsNaR(Posit16 value)` |  |
| `IsNegative` | `static bool IsNegative(Posit16 value)` |  |
| `Parse` | `static Posit16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Posit16 Parse(string s)` |  |
| `Parse` | `static Posit16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Posit16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Posit16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Posit16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Posit16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Posit16 result)` |  |
| `explicit operator Posit16` | `static explicit operator Posit16(double value)` |  |
| `explicit operator Posit16` | `static explicit operator Posit16(float value)` |  |
| `implicit operator double` | `static implicit operator double(Posit16 value)` |  |
| `implicit operator float` | `static implicit operator float(Posit16 value)` |  |
| `operator !=` | `static bool operator !=(Posit16 left, Posit16 right)` |  |
| `operator *` | `static Posit16 operator *(Posit16 left, Posit16 right)` |  |
| `operator +` | `static Posit16 operator +(Posit16 left, Posit16 right)` |  |
| `operator +` | `static Posit16 operator +(Posit16 value)` |  |
| `operator -` | `static Posit16 operator -(Posit16 left, Posit16 right)` |  |
| `operator -` | `static Posit16 operator -(Posit16 value)` |  |
| `operator /` | `static Posit16 operator /(Posit16 left, Posit16 right)` |  |
| `operator <=` | `static bool operator <=(Posit16 left, Posit16 right)` |  |
| `operator <` | `static bool operator <(Posit16 left, Posit16 right)` |  |
| `operator ==` | `static bool operator ==(Posit16 left, Posit16 right)` |  |
| `operator >=` | `static bool operator >=(Posit16 left, Posit16 right)` |  |
| `operator >` | `static bool operator >(Posit16 left, Posit16 right)` |  |

#### `Posit32`

Implements `IComparable`, `IComparable<Posit32>`, `IEquatable<Posit32>`, `IFormattable`, `IParsable<Posit32>`, `ISpanFormattable`, `ISpanParsable<Posit32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static Posit32 MaxValue { get; }` |  |
| `MinValue` | `static Posit32 MinValue { get; }` |  |
| `NaR` | `static Posit32 NaR { get; }` |  |
| `One` | `static Posit32 One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static Posit32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Posit32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Posit32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Posit32 FromDouble(double value)` |  |
| `FromRaw` | `static Posit32 FromRaw(uint raw)` |  |
| `FromSingle` | `static Posit32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Posit32 value)` |  |
| `IsNaR` | `static bool IsNaR(Posit32 value)` |  |
| `IsNegative` | `static bool IsNegative(Posit32 value)` |  |
| `Parse` | `static Posit32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Posit32 Parse(string s)` |  |
| `Parse` | `static Posit32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Posit32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Posit32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Posit32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Posit32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Posit32 result)` |  |
| `explicit operator Posit32` | `static explicit operator Posit32(double value)` |  |
| `explicit operator Posit32` | `static explicit operator Posit32(float value)` |  |
| `implicit operator double` | `static implicit operator double(Posit32 value)` |  |
| `implicit operator float` | `static implicit operator float(Posit32 value)` |  |
| `operator !=` | `static bool operator !=(Posit32 left, Posit32 right)` |  |
| `operator *` | `static Posit32 operator *(Posit32 left, Posit32 right)` |  |
| `operator +` | `static Posit32 operator +(Posit32 left, Posit32 right)` |  |
| `operator +` | `static Posit32 operator +(Posit32 value)` |  |
| `operator -` | `static Posit32 operator -(Posit32 left, Posit32 right)` |  |
| `operator -` | `static Posit32 operator -(Posit32 value)` |  |
| `operator /` | `static Posit32 operator /(Posit32 left, Posit32 right)` |  |
| `operator <=` | `static bool operator <=(Posit32 left, Posit32 right)` |  |
| `operator <` | `static bool operator <(Posit32 left, Posit32 right)` |  |
| `operator ==` | `static bool operator ==(Posit32 left, Posit32 right)` |  |
| `operator >=` | `static bool operator >=(Posit32 left, Posit32 right)` |  |
| `operator >` | `static bool operator >(Posit32 left, Posit32 right)` |  |

#### `Posit8`

Implements `IComparable`, `IComparable<Posit8>`, `IEquatable<Posit8>`, `IFormattable`, `IParsable<Posit8>`, `ISpanFormattable`, `ISpanParsable<Posit8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static Posit8 MaxValue { get; }` |  |
| `MinValue` | `static Posit8 MinValue { get; }` |  |
| `NaR` | `static Posit8 NaR { get; }` |  |
| `One` | `static Posit8 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static Posit8 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(Posit8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Posit8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Posit8 FromDouble(double value)` |  |
| `FromRaw` | `static Posit8 FromRaw(byte raw)` |  |
| `FromSingle` | `static Posit8 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Posit8 value)` |  |
| `IsNaR` | `static bool IsNaR(Posit8 value)` |  |
| `IsNegative` | `static bool IsNegative(Posit8 value)` |  |
| `Parse` | `static Posit8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Posit8 Parse(string s)` |  |
| `Parse` | `static Posit8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Posit8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Posit8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Posit8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Posit8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Posit8 result)` |  |
| `explicit operator Posit8` | `static explicit operator Posit8(double value)` |  |
| `explicit operator Posit8` | `static explicit operator Posit8(float value)` |  |
| `implicit operator double` | `static implicit operator double(Posit8 value)` |  |
| `implicit operator float` | `static implicit operator float(Posit8 value)` |  |
| `operator !=` | `static bool operator !=(Posit8 left, Posit8 right)` |  |
| `operator *` | `static Posit8 operator *(Posit8 left, Posit8 right)` |  |
| `operator +` | `static Posit8 operator +(Posit8 left, Posit8 right)` |  |
| `operator +` | `static Posit8 operator +(Posit8 value)` |  |
| `operator -` | `static Posit8 operator -(Posit8 left, Posit8 right)` |  |
| `operator -` | `static Posit8 operator -(Posit8 value)` |  |
| `operator /` | `static Posit8 operator /(Posit8 left, Posit8 right)` |  |
| `operator <=` | `static bool operator <=(Posit8 left, Posit8 right)` |  |
| `operator <` | `static bool operator <(Posit8 left, Posit8 right)` |  |
| `operator ==` | `static bool operator ==(Posit8 left, Posit8 right)` |  |
| `operator >=` | `static bool operator >=(Posit8 left, Posit8 right)` |  |
| `operator >` | `static bool operator >(Posit8 left, Posit8 right)` |  |

#### `Q15_16`

Implements `IComparable`, `IComparable<Q15_16>`, `IEquatable<Q15_16>`, `IFormattable`, `IParsable<Q15_16>`, `ISpanFormattable`, `ISpanParsable<Q15_16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static Q15_16 Epsilon { get; }` |  |
| `MaxValue` | `static Q15_16 MaxValue { get; }` |  |
| `MinValue` | `static Q15_16 MinValue { get; }` |  |
| `One` | `static Q15_16 One { get; }` |  |
| `RawValue` | `int RawValue { get; }` |  |
| `Zero` | `static Q15_16 Zero { get; }` |  |
| `Abs` | `static Q15_16 Abs(Q15_16 value)` |  |
| `Clamp` | `static Q15_16 Clamp(Q15_16 value, Q15_16 min, Q15_16 max)` |  |
| `CompareTo` | `int CompareTo(Q15_16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Q15_16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Q15_16 FromDouble(double value)` |  |
| `FromInt32` | `static Q15_16 FromInt32(int value)` |  |
| `FromRaw` | `static Q15_16 FromRaw(int raw)` |  |
| `FromSingle` | `static Q15_16 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static Q15_16 Max(Q15_16 left, Q15_16 right)` |  |
| `Min` | `static Q15_16 Min(Q15_16 left, Q15_16 right)` |  |
| `Parse` | `static Q15_16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Q15_16 Parse(string s)` |  |
| `Parse` | `static Q15_16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Q15_16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Q15_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Q15_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Q15_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Q15_16 result)` |  |
| `explicit operator Q15_16` | `static explicit operator Q15_16(Q31_32 value)` |  |
| `explicit operator Q15_16` | `static explicit operator Q15_16(double value)` |  |
| `explicit operator Q15_16` | `static explicit operator Q15_16(float value)` |  |
| `explicit operator Q15_16` | `static explicit operator Q15_16(int raw)` |  |
| `explicit operator Q7_8` | `static explicit operator Q7_8(Q15_16 value)` |  |
| `explicit operator int` | `static explicit operator int(Q15_16 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(Q15_16 value)` |  |
| `explicit operator short` | `static explicit operator short(Q15_16 value)` |  |
| `implicit operator Q15_16` | `static implicit operator Q15_16(sbyte value)` |  |
| `implicit operator Q15_16` | `static implicit operator Q15_16(short value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(Q15_16 value)` |  |
| `implicit operator double` | `static implicit operator double(Q15_16 value)` |  |
| `implicit operator float` | `static implicit operator float(Q15_16 value)` |  |
| `operator !=` | `static bool operator !=(Q15_16 left, Q15_16 right)` |  |
| `operator %` | `static Q15_16 operator %(Q15_16 left, Q15_16 right)` |  |
| `operator *` | `static Q15_16 operator *(Q15_16 left, Q15_16 right)` |  |
| `operator *` | `static Q15_16 operator *(Q15_16 left, int right)` |  |
| `operator *` | `static Q15_16 operator *(int left, Q15_16 right)` |  |
| `operator ++` | `static Q15_16 operator ++(Q15_16 value)` |  |
| `operator +` | `static Q15_16 operator +(Q15_16 left, Q15_16 right)` |  |
| `operator +` | `static Q15_16 operator +(Q15_16 left, int right)` |  |
| `operator +` | `static Q15_16 operator +(Q15_16 value)` |  |
| `operator +` | `static Q15_16 operator +(int left, Q15_16 right)` |  |
| `operator --` | `static Q15_16 operator --(Q15_16 value)` |  |
| `operator -` | `static Q15_16 operator -(Q15_16 left, Q15_16 right)` |  |
| `operator -` | `static Q15_16 operator -(Q15_16 left, int right)` |  |
| `operator -` | `static Q15_16 operator -(Q15_16 value)` |  |
| `operator -` | `static Q15_16 operator -(int left, Q15_16 right)` |  |
| `operator /` | `static Q15_16 operator /(Q15_16 left, Q15_16 right)` |  |
| `operator /` | `static Q15_16 operator /(Q15_16 left, int right)` |  |
| `operator <=` | `static bool operator <=(Q15_16 left, Q15_16 right)` |  |
| `operator <` | `static bool operator <(Q15_16 left, Q15_16 right)` |  |
| `operator ==` | `static bool operator ==(Q15_16 left, Q15_16 right)` |  |
| `operator >=` | `static bool operator >=(Q15_16 left, Q15_16 right)` |  |
| `operator >` | `static bool operator >(Q15_16 left, Q15_16 right)` |  |

#### `Q31_32`

Implements `IComparable`, `IComparable<Q31_32>`, `IEquatable<Q31_32>`, `IFormattable`, `IParsable<Q31_32>`, `ISpanFormattable`, `ISpanParsable<Q31_32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static Q31_32 Epsilon { get; }` |  |
| `MaxValue` | `static Q31_32 MaxValue { get; }` |  |
| `MinValue` | `static Q31_32 MinValue { get; }` |  |
| `One` | `static Q31_32 One { get; }` |  |
| `RawValue` | `long RawValue { get; }` |  |
| `Zero` | `static Q31_32 Zero { get; }` |  |
| `Abs` | `static Q31_32 Abs(Q31_32 value)` |  |
| `Clamp` | `static Q31_32 Clamp(Q31_32 value, Q31_32 min, Q31_32 max)` |  |
| `CompareTo` | `int CompareTo(Q31_32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Q31_32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Q31_32 FromDouble(double value)` |  |
| `FromInt32` | `static Q31_32 FromInt32(int value)` |  |
| `FromInt64` | `static Q31_32 FromInt64(long value)` |  |
| `FromRaw` | `static Q31_32 FromRaw(long raw)` |  |
| `FromSingle` | `static Q31_32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static Q31_32 Max(Q31_32 left, Q31_32 right)` |  |
| `Min` | `static Q31_32 Min(Q31_32 left, Q31_32 right)` |  |
| `Parse` | `static Q31_32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Q31_32 Parse(string s)` |  |
| `Parse` | `static Q31_32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Q31_32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToInt64` | `long ToInt64()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Q31_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Q31_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Q31_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Q31_32 result)` |  |
| `explicit operator Q15_16` | `static explicit operator Q15_16(Q31_32 value)` |  |
| `explicit operator Q31_32` | `static explicit operator Q31_32(double value)` |  |
| `explicit operator Q31_32` | `static explicit operator Q31_32(float value)` |  |
| `explicit operator Q31_32` | `static explicit operator Q31_32(long raw)` |  |
| `explicit operator Q7_8` | `static explicit operator Q7_8(Q31_32 value)` |  |
| `explicit operator float` | `static explicit operator float(Q31_32 value)` |  |
| `explicit operator int` | `static explicit operator int(Q31_32 value)` |  |
| `explicit operator long` | `static explicit operator long(Q31_32 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(Q31_32 value)` |  |
| `explicit operator short` | `static explicit operator short(Q31_32 value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(int value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(sbyte value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(short value)` |  |
| `implicit operator double` | `static implicit operator double(Q31_32 value)` |  |
| `operator !=` | `static bool operator !=(Q31_32 left, Q31_32 right)` |  |
| `operator %` | `static Q31_32 operator %(Q31_32 left, Q31_32 right)` |  |
| `operator *` | `static Q31_32 operator *(Q31_32 left, Q31_32 right)` |  |
| `operator *` | `static Q31_32 operator *(Q31_32 left, long right)` |  |
| `operator *` | `static Q31_32 operator *(long left, Q31_32 right)` |  |
| `operator ++` | `static Q31_32 operator ++(Q31_32 value)` |  |
| `operator +` | `static Q31_32 operator +(Q31_32 left, Q31_32 right)` |  |
| `operator +` | `static Q31_32 operator +(Q31_32 left, long right)` |  |
| `operator +` | `static Q31_32 operator +(Q31_32 value)` |  |
| `operator +` | `static Q31_32 operator +(long left, Q31_32 right)` |  |
| `operator --` | `static Q31_32 operator --(Q31_32 value)` |  |
| `operator -` | `static Q31_32 operator -(Q31_32 left, Q31_32 right)` |  |
| `operator -` | `static Q31_32 operator -(Q31_32 left, long right)` |  |
| `operator -` | `static Q31_32 operator -(Q31_32 value)` |  |
| `operator -` | `static Q31_32 operator -(long left, Q31_32 right)` |  |
| `operator /` | `static Q31_32 operator /(Q31_32 left, Q31_32 right)` |  |
| `operator /` | `static Q31_32 operator /(Q31_32 left, long right)` |  |
| `operator <=` | `static bool operator <=(Q31_32 left, Q31_32 right)` |  |
| `operator <` | `static bool operator <(Q31_32 left, Q31_32 right)` |  |
| `operator ==` | `static bool operator ==(Q31_32 left, Q31_32 right)` |  |
| `operator >=` | `static bool operator >=(Q31_32 left, Q31_32 right)` |  |
| `operator >` | `static bool operator >(Q31_32 left, Q31_32 right)` |  |

#### `Q3_4`

Implements `IComparable`, `IComparable<Q3_4>`, `IEquatable<Q3_4>`, `IFormattable`, `IParsable<Q3_4>`, `ISpanFormattable`, `ISpanParsable<Q3_4>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static Q3_4 Epsilon { get; }` |  |
| `MaxValue` | `static Q3_4 MaxValue { get; }` |  |
| `MinValue` | `static Q3_4 MinValue { get; }` |  |
| `One` | `static Q3_4 One { get; }` |  |
| `RawValue` | `sbyte RawValue { get; }` |  |
| `Zero` | `static Q3_4 Zero { get; }` |  |
| `Abs` | `static Q3_4 Abs(Q3_4 value)` |  |
| `Clamp` | `static Q3_4 Clamp(Q3_4 value, Q3_4 min, Q3_4 max)` |  |
| `CompareTo` | `int CompareTo(Q3_4 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Q3_4 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Q3_4 FromDouble(double value)` |  |
| `FromInt32` | `static Q3_4 FromInt32(int value)` |  |
| `FromRaw` | `static Q3_4 FromRaw(sbyte raw)` |  |
| `FromSingle` | `static Q3_4 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static Q3_4 Max(Q3_4 left, Q3_4 right)` |  |
| `Min` | `static Q3_4 Min(Q3_4 left, Q3_4 right)` |  |
| `Parse` | `static Q3_4 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Q3_4 Parse(string s)` |  |
| `Parse` | `static Q3_4 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Q3_4 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Q3_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Q3_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Q3_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Q3_4 result)` |  |
| `explicit operator Q3_4` | `static explicit operator Q3_4(byte raw)` |  |
| `explicit operator Q3_4` | `static explicit operator Q3_4(double value)` |  |
| `explicit operator Q3_4` | `static explicit operator Q3_4(float value)` |  |
| `explicit operator int` | `static explicit operator int(Q3_4 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(Q3_4 value)` |  |
| `explicit operator short` | `static explicit operator short(Q3_4 value)` |  |
| `implicit operator Q15_16` | `static implicit operator Q15_16(Q3_4 value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(Q3_4 value)` |  |
| `implicit operator Q3_4` | `static implicit operator Q3_4(sbyte value)` |  |
| `implicit operator Q7_8` | `static implicit operator Q7_8(Q3_4 value)` |  |
| `implicit operator double` | `static implicit operator double(Q3_4 value)` |  |
| `implicit operator float` | `static implicit operator float(Q3_4 value)` |  |
| `operator !=` | `static bool operator !=(Q3_4 left, Q3_4 right)` |  |
| `operator %` | `static Q3_4 operator %(Q3_4 left, Q3_4 right)` |  |
| `operator *` | `static Q3_4 operator *(Q3_4 left, Q3_4 right)` |  |
| `operator *` | `static Q3_4 operator *(Q3_4 left, int right)` |  |
| `operator *` | `static Q3_4 operator *(int left, Q3_4 right)` |  |
| `operator ++` | `static Q3_4 operator ++(Q3_4 value)` |  |
| `operator +` | `static Q3_4 operator +(Q3_4 left, Q3_4 right)` |  |
| `operator +` | `static Q3_4 operator +(Q3_4 left, int right)` |  |
| `operator +` | `static Q3_4 operator +(Q3_4 value)` |  |
| `operator +` | `static Q3_4 operator +(int left, Q3_4 right)` |  |
| `operator --` | `static Q3_4 operator --(Q3_4 value)` |  |
| `operator -` | `static Q3_4 operator -(Q3_4 left, Q3_4 right)` |  |
| `operator -` | `static Q3_4 operator -(Q3_4 left, int right)` |  |
| `operator -` | `static Q3_4 operator -(Q3_4 value)` |  |
| `operator -` | `static Q3_4 operator -(int left, Q3_4 right)` |  |
| `operator /` | `static Q3_4 operator /(Q3_4 left, Q3_4 right)` |  |
| `operator /` | `static Q3_4 operator /(Q3_4 left, int right)` |  |
| `operator <=` | `static bool operator <=(Q3_4 left, Q3_4 right)` |  |
| `operator <` | `static bool operator <(Q3_4 left, Q3_4 right)` |  |
| `operator ==` | `static bool operator ==(Q3_4 left, Q3_4 right)` |  |
| `operator >=` | `static bool operator >=(Q3_4 left, Q3_4 right)` |  |
| `operator >` | `static bool operator >(Q3_4 left, Q3_4 right)` |  |

#### `Q7_8`

Implements `IComparable`, `IComparable<Q7_8>`, `IEquatable<Q7_8>`, `IFormattable`, `IParsable<Q7_8>`, `ISpanFormattable`, `ISpanParsable<Q7_8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static Q7_8 Epsilon { get; }` |  |
| `MaxValue` | `static Q7_8 MaxValue { get; }` |  |
| `MinValue` | `static Q7_8 MinValue { get; }` |  |
| `One` | `static Q7_8 One { get; }` |  |
| `RawValue` | `short RawValue { get; }` |  |
| `Zero` | `static Q7_8 Zero { get; }` |  |
| `Abs` | `static Q7_8 Abs(Q7_8 value)` |  |
| `Clamp` | `static Q7_8 Clamp(Q7_8 value, Q7_8 min, Q7_8 max)` |  |
| `CompareTo` | `int CompareTo(Q7_8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(Q7_8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Q7_8 FromDouble(double value)` |  |
| `FromInt32` | `static Q7_8 FromInt32(int value)` |  |
| `FromRaw` | `static Q7_8 FromRaw(short raw)` |  |
| `FromSingle` | `static Q7_8 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static Q7_8 Max(Q7_8 left, Q7_8 right)` |  |
| `Min` | `static Q7_8 Min(Q7_8 left, Q7_8 right)` |  |
| `Parse` | `static Q7_8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Q7_8 Parse(string s)` |  |
| `Parse` | `static Q7_8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Q7_8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Q7_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Q7_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Q7_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out Q7_8 result)` |  |
| `explicit operator Q7_8` | `static explicit operator Q7_8(double value)` |  |
| `explicit operator Q7_8` | `static explicit operator Q7_8(float value)` |  |
| `explicit operator Q7_8` | `static explicit operator Q7_8(short raw)` |  |
| `explicit operator int` | `static explicit operator int(Q7_8 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(Q7_8 value)` |  |
| `explicit operator short` | `static explicit operator short(Q7_8 value)` |  |
| `implicit operator Q15_16` | `static implicit operator Q15_16(Q7_8 value)` |  |
| `implicit operator Q31_32` | `static implicit operator Q31_32(Q7_8 value)` |  |
| `implicit operator Q7_8` | `static implicit operator Q7_8(sbyte value)` |  |
| `implicit operator double` | `static implicit operator double(Q7_8 value)` |  |
| `implicit operator float` | `static implicit operator float(Q7_8 value)` |  |
| `operator !=` | `static bool operator !=(Q7_8 left, Q7_8 right)` |  |
| `operator %` | `static Q7_8 operator %(Q7_8 left, Q7_8 right)` |  |
| `operator *` | `static Q7_8 operator *(Q7_8 left, Q7_8 right)` |  |
| `operator *` | `static Q7_8 operator *(Q7_8 left, int right)` |  |
| `operator *` | `static Q7_8 operator *(int left, Q7_8 right)` |  |
| `operator ++` | `static Q7_8 operator ++(Q7_8 value)` |  |
| `operator +` | `static Q7_8 operator +(Q7_8 left, Q7_8 right)` |  |
| `operator +` | `static Q7_8 operator +(Q7_8 left, int right)` |  |
| `operator +` | `static Q7_8 operator +(Q7_8 value)` |  |
| `operator +` | `static Q7_8 operator +(int left, Q7_8 right)` |  |
| `operator --` | `static Q7_8 operator --(Q7_8 value)` |  |
| `operator -` | `static Q7_8 operator -(Q7_8 left, Q7_8 right)` |  |
| `operator -` | `static Q7_8 operator -(Q7_8 left, int right)` |  |
| `operator -` | `static Q7_8 operator -(Q7_8 value)` |  |
| `operator -` | `static Q7_8 operator -(int left, Q7_8 right)` |  |
| `operator /` | `static Q7_8 operator /(Q7_8 left, Q7_8 right)` |  |
| `operator /` | `static Q7_8 operator /(Q7_8 left, int right)` |  |
| `operator <=` | `static bool operator <=(Q7_8 left, Q7_8 right)` |  |
| `operator <` | `static bool operator <(Q7_8 left, Q7_8 right)` |  |
| `operator ==` | `static bool operator ==(Q7_8 left, Q7_8 right)` |  |
| `operator >=` | `static bool operator >=(Q7_8 left, Q7_8 right)` |  |
| `operator >` | `static bool operator >(Q7_8 left, Q7_8 right)` |  |

#### `Quarter`

Implements `IComparable`, `IComparable<Quarter>`, `IEquatable<Quarter>`, `IFormattable`, `IParsable<Quarter>`, `ISpanFormattable`, `ISpanParsable<Quarter>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static Quarter Epsilon { get; }` |  |
| `MaxValue` | `static Quarter MaxValue { get; }` |  |
| `MinValue` | `static Quarter MinValue { get; }` |  |
| `NaN` | `static Quarter NaN { get; }` |  |
| `NegativeInfinity` | `static Quarter NegativeInfinity { get; }` |  |
| `One` | `static Quarter One { get; }` |  |
| `PositiveInfinity` | `static Quarter PositiveInfinity { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static Quarter Zero { get; }` |  |
| `Abs` | `static Quarter Abs(Quarter value)` |  |
| `CompareTo` | `int CompareTo(Quarter other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `CopySign` | `static Quarter CopySign(Quarter value, Quarter sign)` |  |
| `Equals` | `bool Equals(Quarter other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static Quarter FromDouble(double value)` |  |
| `FromRaw` | `static Quarter FromRaw(byte raw)` |  |
| `FromSingle` | `static Quarter FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(Quarter value)` |  |
| `IsInfinity` | `static bool IsInfinity(Quarter value)` |  |
| `IsNaN` | `static bool IsNaN(Quarter value)` |  |
| `IsNegativeInfinity` | `static bool IsNegativeInfinity(Quarter value)` |  |
| `IsNegative` | `static bool IsNegative(Quarter value)` |  |
| `IsNormal` | `static bool IsNormal(Quarter value)` |  |
| `IsPositiveInfinity` | `static bool IsPositiveInfinity(Quarter value)` |  |
| `IsSubnormal` | `static bool IsSubnormal(Quarter value)` |  |
| `Max` | `static Quarter Max(Quarter left, Quarter right)` |  |
| `Min` | `static Quarter Min(Quarter left, Quarter right)` |  |
| `Parse` | `static Quarter Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static Quarter Parse(string s)` |  |
| `Parse` | `static Quarter Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static Quarter Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Quarter result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out Quarter result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Quarter result)` |  |
| `TryParse` | `static bool TryParse(string s, out Quarter result)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(Half value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(double value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(float value)` |  |
| `implicit operator Half` | `static implicit operator Half(Quarter value)` |  |
| `implicit operator double` | `static implicit operator double(Quarter value)` |  |
| `implicit operator float` | `static implicit operator float(Quarter value)` |  |
| `operator !=` | `static bool operator !=(Quarter left, Quarter right)` |  |
| `operator %` | `static Quarter operator %(Quarter left, Quarter right)` |  |
| `operator *` | `static Quarter operator *(Quarter left, Quarter right)` |  |
| `operator *` | `static Quarter operator *(Quarter left, float right)` |  |
| `operator *` | `static Quarter operator *(Quarter left, int right)` |  |
| `operator *` | `static Quarter operator *(float left, Quarter right)` |  |
| `operator *` | `static Quarter operator *(int left, Quarter right)` |  |
| `operator ++` | `static Quarter operator ++(Quarter value)` |  |
| `operator +` | `static Quarter operator +(Quarter left, Quarter right)` |  |
| `operator +` | `static Quarter operator +(Quarter left, float right)` |  |
| `operator +` | `static Quarter operator +(Quarter left, int right)` |  |
| `operator +` | `static Quarter operator +(Quarter value)` |  |
| `operator +` | `static Quarter operator +(float left, Quarter right)` |  |
| `operator +` | `static Quarter operator +(int left, Quarter right)` |  |
| `operator --` | `static Quarter operator --(Quarter value)` |  |
| `operator -` | `static Quarter operator -(Quarter left, Quarter right)` |  |
| `operator -` | `static Quarter operator -(Quarter left, float right)` |  |
| `operator -` | `static Quarter operator -(Quarter left, int right)` |  |
| `operator -` | `static Quarter operator -(Quarter value)` |  |
| `operator -` | `static Quarter operator -(float left, Quarter right)` |  |
| `operator -` | `static Quarter operator -(int left, Quarter right)` |  |
| `operator /` | `static Quarter operator /(Quarter left, Quarter right)` |  |
| `operator /` | `static Quarter operator /(Quarter left, float right)` |  |
| `operator /` | `static Quarter operator /(Quarter left, int right)` |  |
| `operator /` | `static Quarter operator /(float left, Quarter right)` |  |
| `operator /` | `static Quarter operator /(int left, Quarter right)` |  |
| `operator <=` | `static bool operator <=(Quarter left, Quarter right)` |  |
| `operator <` | `static bool operator <(Quarter left, Quarter right)` |  |
| `operator ==` | `static bool operator ==(Quarter left, Quarter right)` |  |
| `operator >=` | `static bool operator >=(Quarter left, Quarter right)` |  |
| `operator >` | `static bool operator >(Quarter left, Quarter right)` |  |

#### `RandomExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GeneratePassword` | `static string GeneratePassword(this Random @this, PasswordSettings? settings = null, bool useStrongRandomization = false)` |  |
| `GetBoolean` | `static bool GetBoolean(this Random @this)` |  |
| `GetChar` | `static char GetChar(this Random @this, bool only7BitAscii = false, bool only8Bit = false, bool noSurrogates = false, bool noControlCharacters = false, bool noWhiteSpace = false)` |  |
| `GetDecimal` | `static decimal GetDecimal(this Random @this, bool onlyPositive = false)` |  |
| `GetDouble` | `static double GetDouble(this Random @this, bool onlyPositive = false, bool noNaN = false, bool noInfinity = false)` |  |
| `GetFloat` | `static float GetFloat(this Random @this, bool onlyPositive = false, bool noNaN = false, bool noInfinity = false)` |  |
| `GetInt16` | `static short GetInt16(this Random @this, bool onlyPositive = false)` |  |
| `GetInt32` | `static int GetInt32(this Random @this, bool onlyPositive = false)` |  |
| `GetInt64` | `static long GetInt64(this Random @this, bool onlyPositive = false)` |  |
| `GetInt8` | `static sbyte GetInt8(this Random @this, bool onlyPositive = false)` |  |
| `GetString` | `static string GetString(this Random @this, int minLength, int maxLength, bool allowNull = false)` |  |
| `GetUInt16` | `static ushort GetUInt16(this Random @this)` |  |
| `GetUInt32` | `static uint GetUInt32(this Random @this)` |  |
| `GetUInt64` | `static ulong GetUInt64(this Random @this)` |  |
| `GetUInt8` | `static byte GetUInt8(this Random @this)` |  |
| `GetValueFor` | `static T GetValueFor<T>(this Random @this)` |  |
| `NextDouble` | `static double NextDouble(this Random @this, double minimumInclusive, double maximumExclusive)` |  |
| `RollADice` | `static byte RollADice(this Random @this, byte count = 6)` |  |

#### `RandomExtensions.PasswordSettings`

| Member | Signature | Summary |
| --- | --- | --- |
| `PasswordSettings` | `PasswordSettings()` |  |
| `AllowLowerCaseLetters` | `bool AllowLowerCaseLetters { get; init; }` |  |
| `AllowNumbers` | `bool AllowNumbers { get; init; }` |  |
| `AllowSpecialCharacters` | `bool AllowSpecialCharacters { get; init; }` |  |
| `AllowUpperCaseLetters` | `bool AllowUpperCaseLetters { get; init; }` |  |
| `AllowedCharacterSet` | `string AllowedCharacterSet { get; init; }` |  |
| `AvoidDuplicates` | `bool AvoidDuplicates { get; init; }` |  |
| `AvoidVisuallySimilarCharacters` | `bool AvoidVisuallySimilarCharacters { get; init; }` |  |
| `MaximumLength` | `byte MaximumLength { get; init; }` |  |
| `MinimumLength` | `byte MinimumLength { get; init; }` |  |
| `PreferPronouncable` | `bool PreferPronouncable { get; init; }` |  |

#### `RangeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IsInRange` | `static bool IsInRange(this byte @this, IEnumerable<Range> ranges)` |  |
| `IsInRange` | `static bool IsInRange(this int @this, IEnumerable<Range> ranges)` |  |
| `IsInRange` | `static bool IsInRange(this short @this, IEnumerable<Range> ranges)` |  |
| `IsInRange` | `static bool IsInRange(this ushort @this, IEnumerable<Range> ranges)` |  |

#### `ReadOnlyIndexedProperty<TIndexer, TResult>`

| Member | Signature | Summary |
| --- | --- | --- |
| `ReadOnlyIndexedProperty` | `ReadOnlyIndexedProperty(Func<TIndexer, TResult> getter)` |  |
| `Item` | `TResult this[TIndexer index] { get; }` |  |

#### `RealtimeProperty<TType>`

| Member | Signature | Summary |
| --- | --- | --- |
| `RealtimeProperty` | `RealtimeProperty(Func<TType> getter, Action<TType> setter = null, TimeSpan? timeout = null, bool isAsyncSetter = false)` |  |
| `GotValue` | `bool GotValue { get; }` |  |
| `Timeout` | `TimeSpan Timeout { get; set; }` |  |
| `Value` | `TType Value { get; set; }` |  |
| `GetValue` | `TType GetValue(TimeSpan? timeout)` |  |

#### `SByteExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this sbyte @this, char character)` |  |
| `Times` | `static string Times(this sbyte @this, string text)` |  |
| `Times` | `static void Times(this sbyte @this, Action action)` |  |
| `Times` | `static void Times(this sbyte @this, Action<sbyte> action)` |  |

#### `SignedBitCodec`

Implements `IBitCodec<long>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `SignedBitCodec` | `SignedBitCodec(int bitWidth)` |  |
| `BitWidth` | `int BitWidth { get; }` |  |
| `Decode` | `long Decode(ulong code)` |  |
| `Encode` | `ulong Encode(long value)` |  |

#### `SlowProperty<TValue, TIntermediateValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `SlowProperty` | `SlowProperty(Func<SlowProperty<TValue, TIntermediateValue>, TValue> valueGetter, TIntermediateValue intermediateValue = null, Action<SlowProperty<TValue, TIntermediateValue>> valueGeneratedCallback = null, bool captureSynchronizationContext = false)` |  |
| `SlowProperty` | `SlowProperty(SynchronizationContext context, Func<SlowProperty<TValue, TIntermediateValue>, TValue> valueGetter, TIntermediateValue intermediateValue = null, Action<SlowProperty<TValue, TIntermediateValue>> valueGeneratedCallback = null)` |  |
| `RawValue` | `TValue RawValue { get; }` |  |
| `Value` | `TIntermediateValue Value { get; }` |  |
| `Reset` | `void Reset()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator TIntermediateValue` | `static implicit operator TIntermediateValue(SlowProperty<TValue, TIntermediateValue> This)` |  |

#### `SlowProperty<TValue>`

Inherits `SlowProperty<TValue, TValue>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `SlowProperty` | `SlowProperty(Func<SlowProperty<TValue>, TValue> valueGetter, TValue intermediateValue = null, Action<SlowProperty<TValue>> valueGeneratedCallback = null, bool captureSynchronizationContext = false)` |  |
| `SlowProperty` | `SlowProperty(SynchronizationContext context, Func<SlowProperty<TValue>, TValue> valueGetter, TValue intermediateValue = null, Action<SlowProperty<TValue>> valueGeneratedCallback = null)` |  |

#### `SpanExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `And` | `static void And(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `And` | `static void And(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `And` | `static void And(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `And` | `static void And(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `And` | `static void And(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `And` | `static void And(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `And` | `static void And(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `And` | `static void And(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `And` | `static void And(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `And` | `static void And(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `And` | `static void And(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `And` | `static void And(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `And` | `static void And(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `And` | `static void And(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `And` | `static void And(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `And` | `static void And(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `And` | `static void And(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `And` | `static void And(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `And` | `static void And(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `And` | `static void And(this Span<byte> @this, byte operand)` |  |
| `And` | `static void And(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `And` | `static void And(this Span<int> @this, int operand)` |  |
| `And` | `static void And(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `And` | `static void And(this Span<long> @this, long operand)` |  |
| `And` | `static void And(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `And` | `static void And(this Span<sbyte> @this, sbyte operand)` |  |
| `And` | `static void And(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `And` | `static void And(this Span<short> @this, short operand)` |  |
| `And` | `static void And(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `And` | `static void And(this Span<uint> @this, byte operand)` |  |
| `And` | `static void And(this Span<uint> @this, uint operand)` |  |
| `And` | `static void And(this Span<uint> @this, ushort operand)` |  |
| `And` | `static void And(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `And` | `static void And(this Span<ulong> @this, byte operand)` |  |
| `And` | `static void And(this Span<ulong> @this, uint operand)` |  |
| `And` | `static void And(this Span<ulong> @this, ulong operand)` |  |
| `And` | `static void And(this Span<ulong> @this, ushort operand)` |  |
| `And` | `static void And(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `And` | `static void And(this Span<ushort> @this, byte operand)` |  |
| `And` | `static void And(this Span<ushort> @this, ushort operand)` |  |
| `Clear` | `static void Clear(this Span<bool> @this)` |  |
| `Clear` | `static void Clear(this Span<byte> @this)` |  |
| `Clear` | `static void Clear(this Span<int> @this)` |  |
| `Clear` | `static void Clear(this Span<long> @this)` |  |
| `Clear` | `static void Clear(this Span<sbyte> @this)` |  |
| `Clear` | `static void Clear(this Span<short> @this)` |  |
| `Clear` | `static void Clear(this Span<uint> @this)` |  |
| `Clear` | `static void Clear(this Span<ulong> @this)` |  |
| `Clear` | `static void Clear(this Span<ushort> @this)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `Equ` | `static void Equ(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `Equ` | `static void Equ(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `Equ` | `static void Equ(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `Equ` | `static void Equ(this Span<byte> @this, byte operand)` |  |
| `Equ` | `static void Equ(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `Equ` | `static void Equ(this Span<int> @this, int operand)` |  |
| `Equ` | `static void Equ(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `Equ` | `static void Equ(this Span<long> @this, long operand)` |  |
| `Equ` | `static void Equ(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `Equ` | `static void Equ(this Span<sbyte> @this, sbyte operand)` |  |
| `Equ` | `static void Equ(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `Equ` | `static void Equ(this Span<short> @this, short operand)` |  |
| `Equ` | `static void Equ(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `Equ` | `static void Equ(this Span<uint> @this, byte operand)` |  |
| `Equ` | `static void Equ(this Span<uint> @this, uint operand)` |  |
| `Equ` | `static void Equ(this Span<uint> @this, ushort operand)` |  |
| `Equ` | `static void Equ(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `Equ` | `static void Equ(this Span<ulong> @this, byte operand)` |  |
| `Equ` | `static void Equ(this Span<ulong> @this, uint operand)` |  |
| `Equ` | `static void Equ(this Span<ulong> @this, ulong operand)` |  |
| `Equ` | `static void Equ(this Span<ulong> @this, ushort operand)` |  |
| `Equ` | `static void Equ(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `Equ` | `static void Equ(this Span<ushort> @this, byte operand)` |  |
| `Equ` | `static void Equ(this Span<ushort> @this, ushort operand)` |  |
| `Fill` | `static void Fill(this Span<bool> @this, bool value)` |  |
| `Fill` | `static void Fill(this Span<byte> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<int> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<int> @this, int value)` |  |
| `Fill` | `static void Fill(this Span<int> @this, short value)` |  |
| `Fill` | `static void Fill(this Span<int> @this, ushort value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, int value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, long value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, short value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, uint value)` |  |
| `Fill` | `static void Fill(this Span<long> @this, ushort value)` |  |
| `Fill` | `static void Fill(this Span<sbyte> @this, sbyte value)` |  |
| `Fill` | `static void Fill(this Span<short> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<short> @this, short value)` |  |
| `Fill` | `static void Fill(this Span<uint> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<uint> @this, uint value)` |  |
| `Fill` | `static void Fill(this Span<uint> @this, ushort value)` |  |
| `Fill` | `static void Fill(this Span<ulong> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<ulong> @this, uint value)` |  |
| `Fill` | `static void Fill(this Span<ulong> @this, ulong value)` |  |
| `Fill` | `static void Fill(this Span<ulong> @this, ushort value)` |  |
| `Fill` | `static void Fill(this Span<ushort> @this, byte value)` |  |
| `Fill` | `static void Fill(this Span<ushort> @this, ushort value)` |  |
| `IsNotEmpty` | `static bool IsNotEmpty<T>(this Span<T> @this)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `Nand` | `static void Nand(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `Nand` | `static void Nand(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `Nand` | `static void Nand(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `Nand` | `static void Nand(this Span<byte> @this, byte operand)` |  |
| `Nand` | `static void Nand(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `Nand` | `static void Nand(this Span<int> @this, int operand)` |  |
| `Nand` | `static void Nand(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `Nand` | `static void Nand(this Span<long> @this, long operand)` |  |
| `Nand` | `static void Nand(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `Nand` | `static void Nand(this Span<sbyte> @this, sbyte operand)` |  |
| `Nand` | `static void Nand(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `Nand` | `static void Nand(this Span<short> @this, short operand)` |  |
| `Nand` | `static void Nand(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `Nand` | `static void Nand(this Span<uint> @this, byte operand)` |  |
| `Nand` | `static void Nand(this Span<uint> @this, uint operand)` |  |
| `Nand` | `static void Nand(this Span<uint> @this, ushort operand)` |  |
| `Nand` | `static void Nand(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `Nand` | `static void Nand(this Span<ulong> @this, byte operand)` |  |
| `Nand` | `static void Nand(this Span<ulong> @this, uint operand)` |  |
| `Nand` | `static void Nand(this Span<ulong> @this, ulong operand)` |  |
| `Nand` | `static void Nand(this Span<ulong> @this, ushort operand)` |  |
| `Nand` | `static void Nand(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `Nand` | `static void Nand(this Span<ushort> @this, byte operand)` |  |
| `Nand` | `static void Nand(this Span<ushort> @this, ushort operand)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `Nor` | `static void Nor(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `Nor` | `static void Nor(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `Nor` | `static void Nor(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `Nor` | `static void Nor(this Span<byte> @this, byte operand)` |  |
| `Nor` | `static void Nor(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `Nor` | `static void Nor(this Span<int> @this, int operand)` |  |
| `Nor` | `static void Nor(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `Nor` | `static void Nor(this Span<long> @this, long operand)` |  |
| `Nor` | `static void Nor(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `Nor` | `static void Nor(this Span<sbyte> @this, sbyte operand)` |  |
| `Nor` | `static void Nor(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `Nor` | `static void Nor(this Span<short> @this, short operand)` |  |
| `Nor` | `static void Nor(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `Nor` | `static void Nor(this Span<uint> @this, byte operand)` |  |
| `Nor` | `static void Nor(this Span<uint> @this, uint operand)` |  |
| `Nor` | `static void Nor(this Span<uint> @this, ushort operand)` |  |
| `Nor` | `static void Nor(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `Nor` | `static void Nor(this Span<ulong> @this, byte operand)` |  |
| `Nor` | `static void Nor(this Span<ulong> @this, uint operand)` |  |
| `Nor` | `static void Nor(this Span<ulong> @this, ulong operand)` |  |
| `Nor` | `static void Nor(this Span<ulong> @this, ushort operand)` |  |
| `Nor` | `static void Nor(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `Nor` | `static void Nor(this Span<ushort> @this, byte operand)` |  |
| `Nor` | `static void Nor(this Span<ushort> @this, ushort operand)` |  |
| `Not` | `static void Not(this ReadOnlySpan<bool> @this, Span<bool> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<byte> @this, Span<byte> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<int> @this, Span<int> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<long> @this, Span<long> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<sbyte> @this, Span<sbyte> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<short> @this, Span<short> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<uint> @this, Span<uint> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<ulong> @this, Span<ulong> target)` |  |
| `Not` | `static void Not(this ReadOnlySpan<ushort> @this, Span<ushort> target)` |  |
| `Not` | `static void Not(this Span<bool> @this)` |  |
| `Not` | `static void Not(this Span<byte> @this)` |  |
| `Not` | `static void Not(this Span<int> @this)` |  |
| `Not` | `static void Not(this Span<long> @this)` |  |
| `Not` | `static void Not(this Span<sbyte> @this)` |  |
| `Not` | `static void Not(this Span<short> @this)` |  |
| `Not` | `static void Not(this Span<uint> @this)` |  |
| `Not` | `static void Not(this Span<ulong> @this)` |  |
| `Not` | `static void Not(this Span<ushort> @this)` |  |
| `Or` | `static void Or(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `Or` | `static void Or(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `Or` | `static void Or(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `Or` | `static void Or(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `Or` | `static void Or(this Span<byte> @this, byte operand)` |  |
| `Or` | `static void Or(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `Or` | `static void Or(this Span<int> @this, int operand)` |  |
| `Or` | `static void Or(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `Or` | `static void Or(this Span<long> @this, long operand)` |  |
| `Or` | `static void Or(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `Or` | `static void Or(this Span<sbyte> @this, sbyte operand)` |  |
| `Or` | `static void Or(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `Or` | `static void Or(this Span<short> @this, short operand)` |  |
| `Or` | `static void Or(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `Or` | `static void Or(this Span<uint> @this, byte operand)` |  |
| `Or` | `static void Or(this Span<uint> @this, uint operand)` |  |
| `Or` | `static void Or(this Span<uint> @this, ushort operand)` |  |
| `Or` | `static void Or(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `Or` | `static void Or(this Span<ulong> @this, byte operand)` |  |
| `Or` | `static void Or(this Span<ulong> @this, uint operand)` |  |
| `Or` | `static void Or(this Span<ulong> @this, ulong operand)` |  |
| `Or` | `static void Or(this Span<ulong> @this, ushort operand)` |  |
| `Or` | `static void Or(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `Or` | `static void Or(this Span<ushort> @this, byte operand)` |  |
| `Or` | `static void Or(this Span<ushort> @this, ushort operand)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<bool> @this, ReadOnlySpan<bool> operand, Span<bool> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<byte> @this, ReadOnlySpan<byte> operand, Span<byte> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<byte> @this, byte operand, Span<byte> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<int> @this, ReadOnlySpan<int> operand, Span<int> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<int> @this, int operand, Span<int> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<long> @this, ReadOnlySpan<long> operand, Span<long> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<long> @this, long operand, Span<long> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<sbyte> @this, ReadOnlySpan<sbyte> operand, Span<sbyte> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<sbyte> @this, sbyte operand, Span<sbyte> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<short> @this, ReadOnlySpan<short> operand, Span<short> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<short> @this, short operand, Span<short> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<uint> @this, ReadOnlySpan<uint> operand, Span<uint> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<uint> @this, uint operand, Span<uint> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<ulong> @this, ReadOnlySpan<ulong> operand, Span<ulong> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<ulong> @this, ulong operand, Span<ulong> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<ushort> @this, ReadOnlySpan<ushort> operand, Span<ushort> target)` |  |
| `Xor` | `static void Xor(this ReadOnlySpan<ushort> @this, ushort operand, Span<ushort> target)` |  |
| `Xor` | `static void Xor(this Span<bool> @this, ReadOnlySpan<bool> operand)` |  |
| `Xor` | `static void Xor(this Span<byte> @this, ReadOnlySpan<byte> operand)` |  |
| `Xor` | `static void Xor(this Span<byte> @this, byte operand)` |  |
| `Xor` | `static void Xor(this Span<int> @this, ReadOnlySpan<int> operand)` |  |
| `Xor` | `static void Xor(this Span<int> @this, int operand)` |  |
| `Xor` | `static void Xor(this Span<long> @this, ReadOnlySpan<long> operand)` |  |
| `Xor` | `static void Xor(this Span<long> @this, long operand)` |  |
| `Xor` | `static void Xor(this Span<sbyte> @this, ReadOnlySpan<sbyte> operand)` |  |
| `Xor` | `static void Xor(this Span<sbyte> @this, sbyte operand)` |  |
| `Xor` | `static void Xor(this Span<short> @this, ReadOnlySpan<short> operand)` |  |
| `Xor` | `static void Xor(this Span<short> @this, short operand)` |  |
| `Xor` | `static void Xor(this Span<uint> @this, ReadOnlySpan<uint> operand)` |  |
| `Xor` | `static void Xor(this Span<uint> @this, byte operand)` |  |
| `Xor` | `static void Xor(this Span<uint> @this, uint operand)` |  |
| `Xor` | `static void Xor(this Span<uint> @this, ushort operand)` |  |
| `Xor` | `static void Xor(this Span<ulong> @this, ReadOnlySpan<ulong> operand)` |  |
| `Xor` | `static void Xor(this Span<ulong> @this, byte operand)` |  |
| `Xor` | `static void Xor(this Span<ulong> @this, uint operand)` |  |
| `Xor` | `static void Xor(this Span<ulong> @this, ulong operand)` |  |
| `Xor` | `static void Xor(this Span<ulong> @this, ushort operand)` |  |
| `Xor` | `static void Xor(this Span<ushort> @this, ReadOnlySpan<ushort> operand)` |  |
| `Xor` | `static void Xor(this Span<ushort> @this, byte operand)` |  |
| `Xor` | `static void Xor(this Span<ushort> @this, ushort operand)` |  |

#### `StaticMethodLocal`

_No public or protected members._

#### `StaticMethodLocal.Storage<T>`

Implements `IComparable<Storage<T>>`, `IComparable<T>`, `IEquatable<Storage<T>>`, `IEquatable<T>`, `IFormattable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Ref` | `T Ref { get; }` |  |
| `CompareTo` | `int CompareTo(Storage<T> other)` |  |
| `CompareTo` | `int CompareTo(T other)` |  |
| `Equals` | `bool Equals(Storage<T> other)` |  |
| `Equals` | `bool Equals(T other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `implicit operator Storage<T>` | `static implicit operator Storage<T>(T value)` |  |
| `implicit operator T` | `static implicit operator T(Storage<T> storage)` |  |
| `operator !=` | `static bool operator !=(Storage<T> left, Storage<T> right)` |  |
| `operator !=` | `static bool operator !=(Storage<T> left, T right)` |  |
| `operator !=` | `static bool operator !=(T left, Storage<T> right)` |  |
| `operator <=` | `static bool operator <=(Storage<T> left, Storage<T> right)` |  |
| `operator <=` | `static bool operator <=(Storage<T> left, T right)` |  |
| `operator <=` | `static bool operator <=(T left, Storage<T> right)` |  |
| `operator <` | `static bool operator <(Storage<T> left, Storage<T> right)` |  |
| `operator <` | `static bool operator <(Storage<T> left, T right)` |  |
| `operator <` | `static bool operator <(T left, Storage<T> right)` |  |
| `operator ==` | `static bool operator ==(Storage<T> left, Storage<T> right)` |  |
| `operator ==` | `static bool operator ==(Storage<T> left, T right)` |  |
| `operator ==` | `static bool operator ==(T left, Storage<T> right)` |  |
| `operator >=` | `static bool operator >=(Storage<T> left, Storage<T> right)` |  |
| `operator >=` | `static bool operator >=(Storage<T> left, T right)` |  |
| `operator >=` | `static bool operator >=(T left, Storage<T> right)` |  |
| `operator >` | `static bool operator >(Storage<T> left, Storage<T> right)` |  |
| `operator >` | `static bool operator >(Storage<T> left, T right)` |  |
| `operator >` | `static bool operator >(T left, Storage<T> right)` |  |

#### `StaticMethodLocal<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetOrAddByName` | `static Storage<TValue> GetOrAddByName(string name)` |  |
| `GetOrAddByName` | `static Storage<TValue> GetOrAddByName(string name, Func<TValue> valueFactory)` |  |
| `GetOrAddByName` | `static Storage<TValue> GetOrAddByName(string name, TValue defaultValue)` |  |
| `GetOrAdd` | `static Storage<TValue> GetOrAdd(Func<TValue> valueFactory, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Storage<TValue> GetOrAdd(TValue defaultValue, string path = null, int line = 0)` |  |
| `GetOrAdd` | `static Storage<TValue> GetOrAdd(string path = null, int line = 0)` |  |

#### `StringExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AsRegularExpression` | `static Regex AsRegularExpression(this string @this)` |  |
| `AsRegularExpression` | `static Regex AsRegularExpression(this string @this, RegexOptions options)` |  |
| `ComputeHash` | `static string ComputeHash(this string @this, HashAlgorithm hashAlgorithm)` |  |
| `ComputeHash` | `static string ComputeHash<TAlgorithm>(this string @this)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, IEnumerable<string> values)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, IEnumerable<string> values, StringComparer comparer)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, IEnumerable<string> values, StringComparison comparison)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, StringComparer comparer, params string[] other)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, StringComparison comparison, params string[] other)` |  |
| `ContainsAll` | `static bool ContainsAll(this string @this, params string[] other)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, IEnumerable<string> other)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, IEnumerable<string> other, StringComparer comparer)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, IEnumerable<string> other, StringComparison comparisonType)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, StringComparer comparer, params string[] other)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, StringComparison comparisonType, params string[] other)` |  |
| `ContainsAny` | `static bool ContainsAny(this string @this, params string[] other)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, IEnumerable<string> other)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, IEnumerable<string> other, StringComparer comparer)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, IEnumerable<string> other, StringComparison comparison)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, StringComparer comparer, params string[] other)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, StringComparison comparisonType, params string[] other)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny(this string @this, params string[] other)` |  |
| `ContainsNot` | `static bool ContainsNot(this string @this, string value)` |  |
| `ContainsNot` | `static bool ContainsNot(this string @this, string value, StringComparer comparer)` |  |
| `ContainsNot` | `static bool ContainsNot(this string @this, string value, StringComparison comparisonType)` |  |
| `Contains` | `static bool Contains(this string @this, string value, StringComparer comparer)` |  |
| `ConvertFilePatternToRegex` | `static Regex ConvertFilePatternToRegex(this string @this)` |  |
| `DefaultIfEmptyOrWhiteSpace` | `static ReadOnlySpan<char> DefaultIfEmptyOrWhiteSpace(this ReadOnlySpan<char> @this, ReadOnlySpan<char> defaultValue = null)` |  |
| `DefaultIfEmpty` | `static ReadOnlySpan<char> DefaultIfEmpty(this ReadOnlySpan<char> @this, ReadOnlySpan<char> defaultValue = null)` |  |
| `DefaultIfNullOrEmpty` | `static string DefaultIfNullOrEmpty(this string @this, Func<string> factory)` |  |
| `DefaultIfNullOrEmpty` | `static string DefaultIfNullOrEmpty(this string @this, string defaultValue = null)` |  |
| `DefaultIfNullOrWhiteSpace` | `static string DefaultIfNullOrWhiteSpace(this string @this, Func<string> factory)` |  |
| `DefaultIfNullOrWhiteSpace` | `static string DefaultIfNullOrWhiteSpace(this string @this, string defaultValue = null)` |  |
| `DefaultIfNull` | `static string DefaultIfNull(this string @this, Func<string> factory)` |  |
| `DefaultIfNull` | `static string DefaultIfNull(this string @this, string defaultValue)` |  |
| `DetectLineBreakMode` | `static LineBreakMode DetectLineBreakMode(this ReadOnlySpan<char> @this)` |  |
| `DetectLineBreakMode` | `static LineBreakMode DetectLineBreakMode(this string @this)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<char> values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<char> values, StringComparer comparer)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<char> values, StringComparison comparison)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<string> values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<string> values, StringComparer comparer)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, IEnumerable<string> values, StringComparison comparison)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, StringComparer comparer, params char[] values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, StringComparer comparer, params string[] values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, StringComparison comparison, params char[] values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, StringComparison comparison, params string[] values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, params char[] values)` |  |
| `EndsNotWithAny` | `static bool EndsNotWithAny(this string @this, params string[] values)` |  |
| `EndsNotWith` | `static bool EndsNotWith(this string @this, char value, StringComparison stringComparison = 0)` |  |
| `EndsNotWith` | `static bool EndsNotWith(this string @this, char what, StringComparer comparer)` |  |
| `EndsNotWith` | `static bool EndsNotWith(this string @this, string value, StringComparison stringComparison = 0)` |  |
| `EndsNotWith` | `static bool EndsNotWith(this string @this, string what, StringComparer comparer)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<char> values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<char> values, StringComparer comparer)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<char> values, StringComparison stringComparison)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<string> values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<string> values, StringComparer comparer)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, StringComparer comparer, params char[] values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, StringComparer comparer, params string[] values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, StringComparison stringComparison, params char[] values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, StringComparison stringComparison, params string[] values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, params char[] values)` |  |
| `EndsWithAny` | `static bool EndsWithAny(this string @this, params string[] values)` |  |
| `EndsWith` | `static bool EndsWith(this string @this, char value, StringComparer comparer)` |  |
| `EndsWith` | `static bool EndsWith(this string @this, char value, StringComparison stringComparison = 0)` |  |
| `EndsWith` | `static bool EndsWith(this string @this, string what, StringComparer comparer)` |  |
| `EnumerateLines` | `static IEnumerable<string> EnumerateLines(this string @this, LineBreakMode mode, StringSplitOptions options = 0)` |  |
| `EnumerateLines` | `static IEnumerable<string> EnumerateLines(this string @this, LineBreakMode mode, int count, StringSplitOptions options = 0)` |  |
| `EnumerateLines` | `static IEnumerable<string> EnumerateLines(this string @this, StringSplitOptions options = 0)` |  |
| `EnumerateLines` | `static IEnumerable<string> EnumerateLines(this string @this, string delimiter, StringSplitOptions options = 0)` |  |
| `EnumerateLines` | `static IEnumerable<string> EnumerateLines(this string @this, string delimiter, int count, StringSplitOptions options = 0)` |  |
| `ExchangeAt` | `static string ExchangeAt(this string @this, int index, char replacement)` |  |
| `ExchangeAt` | `static string ExchangeAt(this string @this, int index, int count, string replacement)` |  |
| `ExchangeAt` | `static string ExchangeAt(this string @this, int index, string replacement)` |  |
| `FirstOrDefault` | `static char FirstOrDefault(this string @this, char @default = '\0')` |  |
| `First` | `static char First(this string @this)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, Func<string, object> fieldGetter, bool passFieldFormatToGetter = false)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, Hashtable fields)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, IDictionary<string, string> fields)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, IEnumerable<KeyValuePair<string, object>> fields, IEqualityComparer<string> comparer = null)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, IEqualityComparer<string> comparer, params KeyValuePair<string, object>[] fields)` |  |
| `FormatWithEx` | `static string FormatWithEx(this string @this, params KeyValuePair<string, object>[] fields)` |  |
| `FormatWithObject` | `static string FormatWithObject<T>(this string @this, T @object)` |  |
| `FormatWith` | `static string FormatWith(this string @this, params object[] parameters)` |  |
| `FromQuotedPrintable` | `static string FromQuotedPrintable(this string @this)` |  |
| `GetLineJoiner` | `static string GetLineJoiner(LineJoinMode mode)` |  |
| `GetSoundexRepresentationInvariant` | `static string GetSoundexRepresentationInvariant(this string @this)` |  |
| `GetSoundexRepresentationInvariant` | `static string GetSoundexRepresentationInvariant(this string @this, int maxLength)` |  |
| `GetSoundexRepresentation` | `static string GetSoundexRepresentation(this string @this)` |  |
| `GetSoundexRepresentation` | `static string GetSoundexRepresentation(this string @this, CultureInfo culture)` |  |
| `GetSoundexRepresentation` | `static string GetSoundexRepresentation(this string @this, int maxLength)` |  |
| `GetSoundexRepresentation` | `static string GetSoundexRepresentation(this string @this, int maxLength, CultureInfo culture)` |  |
| `IndexOf` | `static int IndexOf(this ReadOnlySpan<char> @this, char value, int startIndex, StringComparison comparison)` |  |
| `IndexOf` | `static int IndexOf(this string @this, char value, int startIndex, StringComparison comparison)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, IEnumerable<string> needles)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, IEnumerable<string> needles, StringComparer comparer)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, IEnumerable<string> needles, StringComparison comparison)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, StringComparer comparer, params string[] needles)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, StringComparison comparison, params string[] needles)` |  |
| `IsAnyOf` | `static bool IsAnyOf(this string @this, params string[] needles)` |  |
| `IsEmptyOrWhiteSpace` | `static bool IsEmptyOrWhiteSpace(this ReadOnlySpan<char> @this)` |  |
| `IsMatch` | `static bool IsMatch(this string @this, Regex regex)` |  |
| `IsMatch` | `static bool IsMatch(this string @this, string regex, RegexOptions regexOptions = 0)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, IEnumerable<string> needles)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, IEnumerable<string> needles, StringComparer comparer)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, IEnumerable<string> needles, StringComparison comparison)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, StringComparer comparer, params string[] needles)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, StringComparison comparison, params string[] needles)` |  |
| `IsNotAnyOf` | `static bool IsNotAnyOf(this string @this, params string[] needles)` |  |
| `IsNotEmptyOrWhiteSpace` | `static bool IsNotEmptyOrWhiteSpace(this ReadOnlySpan<char> @this)` |  |
| `IsNotMatch` | `static bool IsNotMatch(this string @this, Regex regex)` |  |
| `IsNotMatch` | `static bool IsNotMatch(this string @this, string regex, RegexOptions regexOptions = 0)` |  |
| `IsNotNullOrEmpty` | `static bool IsNotNullOrEmpty(this string @this)` |  |
| `IsNotNullOrWhiteSpace` | `static bool IsNotNullOrWhiteSpace(this string @this)` |  |
| `IsNullOrEmpty` | `static bool IsNullOrEmpty(this string @this)` |  |
| `IsNullOrWhiteSpace` | `static bool IsNullOrWhiteSpace(this string @this)` |  |
| `IsSurroundedWith` | `static bool IsSurroundedWith(this string @this, string prefix, string postfix, StringComparison stringComparison = 0)` |  |
| `IsSurroundedWith` | `static bool IsSurroundedWith(this string @this, string text, StringComparison stringComparison = 0)` |  |
| `LastOrDefault` | `static char LastOrDefault(this string @this, char @default = '\0')` |  |
| `Last` | `static char Last(this string @this)` |  |
| `LeftUntil` | `static ReadOnlySpan<char> LeftUntil(this ReadOnlySpan<char> @this, ReadOnlySpan<char> pattern, StringComparison comparison = 0)` |  |
| `LeftUntil` | `static string LeftUntil(this string @this, string pattern, StringComparison comparison = 0)` |  |
| `Left` | `static ReadOnlySpan<char> Left(this ReadOnlySpan<char> @this, int count)` |  |
| `Left` | `static string Left(this string @this, int count)` |  |
| `Like` | `static bool Like(this string @this, string toFind)` |  |
| `LineCount` | `static int LineCount(this string @this, LineBreakMode mode = -3, bool ignoreEmptyLines = false)` |  |
| `Lines` | `static string[] Lines(this string @this, LineBreakMode mode, StringSplitOptions options = 0)` |  |
| `Lines` | `static string[] Lines(this string @this, LineBreakMode mode, int count, StringSplitOptions options = 0)` |  |
| `Lines` | `static string[] Lines(this string @this, StringSplitOptions options = 0)` |  |
| `Lines` | `static string[] Lines(this string @this, string delimiter, StringSplitOptions options = 0)` |  |
| `Lines` | `static string[] Lines(this string @this, string delimiter, int count, StringSplitOptions options = 0)` |  |
| `LongLineCount` | `static long LongLineCount(this string @this, LineBreakMode mode = -3, bool ignoreEmptyLines = false)` |  |
| `LowerFirstInvariant` | `static string LowerFirstInvariant(this string @this)` |  |
| `LowerFirst` | `static string LowerFirst(this string @this, CultureInfo culture = null)` |  |
| `MatchGroups` | `static GroupCollection MatchGroups(this string @this, string regex, RegexOptions regexOptions = 0)` |  |
| `MatchesFilePattern` | `static bool MatchesFilePattern(this string @this, string pattern)` |  |
| `Matches` | `static MatchCollection Matches(this string @this, string regex, RegexOptions regexOptions = 0)` |  |
| `MsSqlDataEscape` | `static string MsSqlDataEscape(this object @this)` |  |
| `MsSqlIdentifierEscape` | `static string MsSqlIdentifierEscape(this string @this)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, IEnumerable<KeyValuePair<string, object>> replacements)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, IEnumerable<KeyValuePair<string, string>> replacements)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, params KeyValuePair<string, object>[] replacements)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, params KeyValuePair<string, string>[] replacements)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, string replacement, string needle1, string needle2, params string[] toReplace)` |  |
| `MultipleReplace` | `static string MultipleReplace(this string @this, string replacement, string[] toReplace)` |  |
| `OnlyCaseDiffersFrom` | `static bool OnlyCaseDiffersFrom(this string @this, string other)` |  |
| `OnlyCaseDiffersFrom` | `static bool OnlyCaseDiffersFrom(this string @this, string other, CaseComparison comparison)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, Func<bool> defaultValueFactory)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, Func<string, bool> defaultValueFactory)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, StringComparison stringComparison)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, StringComparison stringComparison, Func<bool> defaultValueFactory)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, StringComparison stringComparison, Func<string, bool> defaultValueFactory)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, StringComparison stringComparison, bool defaultValue)` |  |
| `ParseBooleanOrDefault` | `static bool ParseBooleanOrDefault(this string @this, string trueValue, string falseValue, bool defaultValue)` |  |
| `ParseBooleanOrNull` | `static bool? ParseBooleanOrNull(this string @this, string trueValue, string falseValue)` |  |
| `ParseBooleanOrNull` | `static bool? ParseBooleanOrNull(this string @this, string trueValue, string falseValue, StringComparison stringComparison)` |  |
| `ParseBoolean` | `static bool ParseBoolean(this string @this, string trueValue)` |  |
| `ParseBoolean` | `static bool ParseBoolean(this string @this, string trueValue, StringComparison stringComparison)` |  |
| `ParseBoolean` | `static bool ParseBoolean(this string @this, string trueValue, string falseValue)` |  |
| `ParseBoolean` | `static bool ParseBoolean(this string @this, string trueValue, string falseValue, StringComparison stringComparison)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this ReadOnlySpan<char> @this, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, Func<string, byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, IFormatProvider provider, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, IFormatProvider provider, Func<string, byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, IFormatProvider provider, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, Func<string, byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, byte> defaultValueFactory)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, NumberStyles style, byte defaultValue)` |  |
| `ParseByteOrDefault` | `static byte ParseByteOrDefault(this string @this, byte defaultValue)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this string @this)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this string @this, NumberStyles style)` |  |
| `ParseByteOrNull` | `static byte? ParseByteOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseByte` | `static byte ParseByte(this ReadOnlySpan<char> @this)` |  |
| `ParseByte` | `static byte ParseByte(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseByte` | `static byte ParseByte(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseByte` | `static byte ParseByte(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseByte` | `static byte ParseByte(this string @this)` |  |
| `ParseByte` | `static byte ParseByte(this string @this, IFormatProvider provider)` |  |
| `ParseByte` | `static byte ParseByte(this string @this, NumberStyles style)` |  |
| `ParseByte` | `static byte ParseByte(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseColorOrDefault` | `static Color ParseColorOrDefault(this string @this)` |  |
| `ParseColorOrDefault` | `static Color ParseColorOrDefault(this string @this, Color defaultValue)` |  |
| `ParseColorOrDefault` | `static Color ParseColorOrDefault(this string @this, Func<Color> defaultValueFactory)` |  |
| `ParseColorOrDefault` | `static Color ParseColorOrDefault(this string @this, Func<string, Color> defaultValueFactory)` |  |
| `ParseColorOrNull` | `static Color? ParseColorOrNull(this string @this)` |  |
| `ParseColor` | `static Color ParseColor(this string @this)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this ReadOnlySpan<char> @this, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, Func<string, uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, IFormatProvider provider, Func<string, uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, IFormatProvider provider, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, IFormatProvider provider, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, Func<string, uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<uint> defaultValueFactory)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, NumberStyles style, uint defaultValue)` |  |
| `ParseDWordOrDefault` | `static uint ParseDWordOrDefault(this string @this, uint defaultValue)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this string @this)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this string @this, NumberStyles style)` |  |
| `ParseDWordOrNull` | `static uint? ParseDWordOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDWord` | `static uint ParseDWord(this ReadOnlySpan<char> @this)` |  |
| `ParseDWord` | `static uint ParseDWord(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDWord` | `static uint ParseDWord(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDWord` | `static uint ParseDWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDWord` | `static uint ParseDWord(this string @this)` |  |
| `ParseDWord` | `static uint ParseDWord(this string @this, IFormatProvider provider)` |  |
| `ParseDWord` | `static uint ParseDWord(this string @this, NumberStyles style)` |  |
| `ParseDWord` | `static uint ParseDWord(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style, IFormatProvider provider, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, DateTimeStyles style, IFormatProvider provider, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, string exactFormat)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, string exactFormat, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this ReadOnlySpan<char> @this, string exactFormat, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, Func<string, DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, IFormatProvider provider, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, IFormatProvider provider, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, DateTimeStyles style, IFormatProvider provider, Func<string, DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, Func<string, DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, IFormatProvider provider, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, IFormatProvider provider, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, IFormatProvider provider, Func<string, DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, string exactFormat)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, string exactFormat, DateTime defaultValue)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, string exactFormat, Func<DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrDefault` | `static DateTime ParseDateTimeOrDefault(this string @this, string exactFormat, Func<string, DateTime> defaultValueFactory)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this ReadOnlySpan<char> @this, DateTimeStyles style)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this ReadOnlySpan<char> @this, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this ReadOnlySpan<char> @this, string exactFormat)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this string @this)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this string @this, DateTimeStyles style)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this string @this, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseDateTimeOrNull` | `static DateTime? ParseDateTimeOrNull(this string @this, string exactFormat)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this, string exactFormat)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, DateTimeStyles style)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, IFormatProvider provider)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this, IFormatProvider provider)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this, string exactFormat)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this, string exactFormat, DateTimeStyles style)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this, string exactFormat, DateTimeStyles style, IFormatProvider provider)` |  |
| `ParseDateTime` | `static DateTime ParseDateTime(this string @this, string exactFormat, IFormatProvider provider)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this ReadOnlySpan<char> @this, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, Func<string, decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, IFormatProvider provider, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, IFormatProvider provider, Func<string, decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, IFormatProvider provider, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, Func<string, decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, decimal> defaultValueFactory)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, IFormatProvider provider, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, NumberStyles style, decimal defaultValue)` |  |
| `ParseDecimalOrDefault` | `static decimal ParseDecimalOrDefault(this string @this, decimal defaultValue)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this string @this)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this string @this, NumberStyles style)` |  |
| `ParseDecimalOrNull` | `static decimal? ParseDecimalOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this ReadOnlySpan<char> @this)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this string @this)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this string @this, IFormatProvider provider)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this string @this, NumberStyles style)` |  |
| `ParseDecimal` | `static decimal ParseDecimal(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this ReadOnlySpan<char> @this, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, Func<string, double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, IFormatProvider provider, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, IFormatProvider provider, Func<string, double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, IFormatProvider provider, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, Func<string, double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, double> defaultValueFactory)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, IFormatProvider provider, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, NumberStyles style, double defaultValue)` |  |
| `ParseDoubleOrDefault` | `static double ParseDoubleOrDefault(this string @this, double defaultValue)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this string @this)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this string @this, NumberStyles style)` |  |
| `ParseDoubleOrNull` | `static double? ParseDoubleOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDouble` | `static double ParseDouble(this ReadOnlySpan<char> @this)` |  |
| `ParseDouble` | `static double ParseDouble(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseDouble` | `static double ParseDouble(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseDouble` | `static double ParseDouble(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseDouble` | `static double ParseDouble(this string @this)` |  |
| `ParseDouble` | `static double ParseDouble(this string @this, IFormatProvider provider)` |  |
| `ParseDouble` | `static double ParseDouble(this string @this, NumberStyles style)` |  |
| `ParseDouble` | `static double ParseDouble(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this, Func<TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this, TEnum defaultValue)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase, Func<TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase, TEnum defaultValue)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, Func<TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, Func<string, TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, TEnum defaultValue)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, bool ignoreCase)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, bool ignoreCase, Func<TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, bool ignoreCase, Func<string, TEnum> defaultValueFactory)` |  |
| `ParseEnumOrDefault` | `static TEnum ParseEnumOrDefault<TEnum>(this string @this, bool ignoreCase, TEnum defaultValue)` |  |
| `ParseEnumOrNull` | `static TEnum? ParseEnumOrNull<TEnum>(this ReadOnlySpan<char> @this)` |  |
| `ParseEnumOrNull` | `static TEnum? ParseEnumOrNull<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase)` |  |
| `ParseEnumOrNull` | `static TEnum? ParseEnumOrNull<TEnum>(this string @this)` |  |
| `ParseEnumOrNull` | `static TEnum? ParseEnumOrNull<TEnum>(this string @this, bool ignoreCase)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum>(this ReadOnlySpan<char> @this)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum>(this string @this)` |  |
| `ParseEnum` | `static TEnum ParseEnum<TEnum>(this string @this, bool ignoreCase)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this ReadOnlySpan<char> @this, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, Func<string, float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, IFormatProvider provider, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, IFormatProvider provider, Func<string, float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, IFormatProvider provider, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, Func<string, float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, float> defaultValueFactory)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, IFormatProvider provider, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, NumberStyles style, float defaultValue)` |  |
| `ParseFloatOrDefault` | `static float ParseFloatOrDefault(this string @this, float defaultValue)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this string @this)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this string @this, NumberStyles style)` |  |
| `ParseFloatOrNull` | `static float? ParseFloatOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseFloat` | `static float ParseFloat(this ReadOnlySpan<char> @this)` |  |
| `ParseFloat` | `static float ParseFloat(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseFloat` | `static float ParseFloat(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseFloat` | `static float ParseFloat(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseFloat` | `static float ParseFloat(this string @this)` |  |
| `ParseFloat` | `static float ParseFloat(this string @this, IFormatProvider provider)` |  |
| `ParseFloat` | `static float ParseFloat(this string @this, NumberStyles style)` |  |
| `ParseFloat` | `static float ParseFloat(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseHostAndPort` | `static HostEndPoint ParseHostAndPort(this string @this)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this ReadOnlySpan<char> @this, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, Func<string, int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, IFormatProvider provider, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, IFormatProvider provider, Func<string, int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, IFormatProvider provider, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, Func<string, int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, int> defaultValueFactory)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, IFormatProvider provider, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, NumberStyles style, int defaultValue)` |  |
| `ParseIntOrDefault` | `static int ParseIntOrDefault(this string @this, int defaultValue)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this string @this)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this string @this, NumberStyles style)` |  |
| `ParseIntOrNull` | `static int? ParseIntOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseInt` | `static int ParseInt(this ReadOnlySpan<char> @this)` |  |
| `ParseInt` | `static int ParseInt(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseInt` | `static int ParseInt(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseInt` | `static int ParseInt(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseInt` | `static int ParseInt(this string @this)` |  |
| `ParseInt` | `static int ParseInt(this string @this, IFormatProvider provider)` |  |
| `ParseInt` | `static int ParseInt(this string @this, NumberStyles style)` |  |
| `ParseInt` | `static int ParseInt(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this ReadOnlySpan<char> @this, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, Func<string, long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, IFormatProvider provider, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, IFormatProvider provider, Func<string, long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, IFormatProvider provider, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, Func<string, long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, long> defaultValueFactory)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, IFormatProvider provider, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, NumberStyles style, long defaultValue)` |  |
| `ParseLongOrDefault` | `static long ParseLongOrDefault(this string @this, long defaultValue)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this string @this)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this string @this, NumberStyles style)` |  |
| `ParseLongOrNull` | `static long? ParseLongOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLong` | `static long ParseLong(this ReadOnlySpan<char> @this)` |  |
| `ParseLong` | `static long ParseLong(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseLong` | `static long ParseLong(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseLong` | `static long ParseLong(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseLong` | `static long ParseLong(this string @this)` |  |
| `ParseLong` | `static long ParseLong(this string @this, IFormatProvider provider)` |  |
| `ParseLong` | `static long ParseLong(this string @this, NumberStyles style)` |  |
| `ParseLong` | `static long ParseLong(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this ReadOnlySpan<char> @this, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, Func<string, ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, IFormatProvider provider, Func<string, ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, IFormatProvider provider, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, IFormatProvider provider, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, Func<string, ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<ulong> defaultValueFactory)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, NumberStyles style, ulong defaultValue)` |  |
| `ParseQWordOrDefault` | `static ulong ParseQWordOrDefault(this string @this, ulong defaultValue)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this string @this)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this string @this, NumberStyles style)` |  |
| `ParseQWordOrNull` | `static ulong? ParseQWordOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWord` | `static ulong ParseQWord(this ReadOnlySpan<char> @this)` |  |
| `ParseQWord` | `static ulong ParseQWord(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseQWord` | `static ulong ParseQWord(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseQWord` | `static ulong ParseQWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseQWord` | `static ulong ParseQWord(this string @this)` |  |
| `ParseQWord` | `static ulong ParseQWord(this string @this, IFormatProvider provider)` |  |
| `ParseQWord` | `static ulong ParseQWord(this string @this, NumberStyles style)` |  |
| `ParseQWord` | `static ulong ParseQWord(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this ReadOnlySpan<char> @this, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, Func<string, sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, IFormatProvider provider, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, IFormatProvider provider, Func<string, sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, IFormatProvider provider, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, Func<string, sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, sbyte> defaultValueFactory)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, IFormatProvider provider, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, NumberStyles style, sbyte defaultValue)` |  |
| `ParseSByteOrDefault` | `static sbyte ParseSByteOrDefault(this string @this, sbyte defaultValue)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this string @this)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this string @this, NumberStyles style)` |  |
| `ParseSByteOrNull` | `static sbyte? ParseSByteOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this ReadOnlySpan<char> @this)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this string @this)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this string @this, IFormatProvider provider)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this string @this, NumberStyles style)` |  |
| `ParseSByte` | `static sbyte ParseSByte(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this ReadOnlySpan<char> @this, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, Func<string, short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, IFormatProvider provider, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, IFormatProvider provider, Func<string, short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, IFormatProvider provider, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, Func<string, short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, short> defaultValueFactory)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, IFormatProvider provider, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, NumberStyles style, short defaultValue)` |  |
| `ParseShortOrDefault` | `static short ParseShortOrDefault(this string @this, short defaultValue)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this string @this)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this string @this, NumberStyles style)` |  |
| `ParseShortOrNull` | `static short? ParseShortOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShort` | `static short ParseShort(this ReadOnlySpan<char> @this)` |  |
| `ParseShort` | `static short ParseShort(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseShort` | `static short ParseShort(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseShort` | `static short ParseShort(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseShort` | `static short ParseShort(this string @this)` |  |
| `ParseShort` | `static short ParseShort(this string @this, IFormatProvider provider)` |  |
| `ParseShort` | `static short ParseShort(this string @this, NumberStyles style)` |  |
| `ParseShort` | `static short ParseShort(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this, Func<TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, TimeSpan defaultValue)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this ReadOnlySpan<char> @this, TimeSpan defaultValue)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, Func<TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, Func<string, TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, IFormatProvider provider, Func<TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, IFormatProvider provider, Func<string, TimeSpan> defaultValueFactory)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, IFormatProvider provider, TimeSpan defaultValue)` |  |
| `ParseTimeSpanOrDefault` | `static TimeSpan ParseTimeSpanOrDefault(this string @this, TimeSpan defaultValue)` |  |
| `ParseTimeSpanOrNull` | `static TimeSpan? ParseTimeSpanOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseTimeSpanOrNull` | `static TimeSpan? ParseTimeSpanOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseTimeSpanOrNull` | `static TimeSpan? ParseTimeSpanOrNull(this string @this)` |  |
| `ParseTimeSpanOrNull` | `static TimeSpan? ParseTimeSpanOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseTimeSpan` | `static TimeSpan ParseTimeSpan(this ReadOnlySpan<char> @this)` |  |
| `ParseTimeSpan` | `static TimeSpan ParseTimeSpan(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseTimeSpan` | `static TimeSpan ParseTimeSpan(this string @this)` |  |
| `ParseTimeSpan` | `static TimeSpan ParseTimeSpan(this string @this, IFormatProvider provider)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, IFormatProvider provider, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, NumberStyles style, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this ReadOnlySpan<char> @this, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, Func<string, ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, IFormatProvider provider)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, IFormatProvider provider, Func<string, ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, IFormatProvider provider, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, IFormatProvider provider, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, Func<string, ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<string, ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, Func<ushort> defaultValueFactory)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, IFormatProvider provider, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, NumberStyles style, ushort defaultValue)` |  |
| `ParseWordOrDefault` | `static ushort ParseWordOrDefault(this string @this, ushort defaultValue)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this ReadOnlySpan<char> @this)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this string @this)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this string @this, IFormatProvider provider)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this string @this, NumberStyles style)` |  |
| `ParseWordOrNull` | `static ushort? ParseWordOrNull(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseWord` | `static ushort ParseWord(this ReadOnlySpan<char> @this)` |  |
| `ParseWord` | `static ushort ParseWord(this ReadOnlySpan<char> @this, IFormatProvider provider)` |  |
| `ParseWord` | `static ushort ParseWord(this ReadOnlySpan<char> @this, NumberStyles style)` |  |
| `ParseWord` | `static ushort ParseWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider)` |  |
| `ParseWord` | `static ushort ParseWord(this string @this)` |  |
| `ParseWord` | `static ushort ParseWord(this string @this, IFormatProvider provider)` |  |
| `ParseWord` | `static ushort ParseWord(this string @this, NumberStyles style)` |  |
| `ParseWord` | `static ushort ParseWord(this string @this, NumberStyles style, IFormatProvider provider)` |  |
| `QuotedSplit` | `static IEnumerable<string> QuotedSplit(this string @this, char[] delimiters, string escapeSequence = "\\", StringSplitOptions options = 0)` |  |
| `QuotedSplit` | `static IEnumerable<string> QuotedSplit(this string @this, string delimiter = ",", string escapeSequence = "\\", StringSplitOptions options = 0)` |  |
| `RemoveAtEnd` | `static ReadOnlySpan<char> RemoveAtEnd(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison comparison = 0)` |  |
| `RemoveAtEnd` | `static string RemoveAtEnd(this string @this, string what, StringComparison comparison = 0)` |  |
| `RemoveAtStart` | `static ReadOnlySpan<char> RemoveAtStart(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison comparison = 0)` |  |
| `RemoveAtStart` | `static string RemoveAtStart(this string @this, string what, StringComparison comparison = 0)` |  |
| `RemoveFirst` | `static ReadOnlySpan<char> RemoveFirst(this ReadOnlySpan<char> @this, int count)` |  |
| `RemoveFirst` | `static string RemoveFirst(this string @this, int count)` |  |
| `RemoveLast` | `static ReadOnlySpan<char> RemoveLast(this ReadOnlySpan<char> @this, int count)` |  |
| `RemoveLast` | `static string RemoveLast(this string @this, int count)` |  |
| `Repeat` | `static string Repeat(this string @this, int count)` |  |
| `ReplaceAnyOf` | `static string ReplaceAnyOf(this string @this, string chars, string replacement)` |  |
| `ReplaceAtEnd` | `static string ReplaceAtEnd(this string @this, string what, string replacement, StringComparison stringComparison = 0)` |  |
| `ReplaceAtStart` | `static string ReplaceAtStart(this string @this, string what, string replacement, StringComparison stringComparison = 0)` |  |
| `ReplaceFirst` | `static string ReplaceFirst(this string @this, string what, string replacement, StringComparison comparison = 0)` |  |
| `ReplaceLast` | `static string ReplaceLast(this string @this, string what, string replacement, StringComparison comparison = 0)` |  |
| `ReplaceRegex` | `static string ReplaceRegex(this string @this, string regex, string newValue = null, RegexOptions regexOptions = 0)` |  |
| `Replace` | `static string Replace(this string @this, Regex regex, string newValue)` |  |
| `Replace` | `static string Replace(this string @this, string oldValue, string newValue, int count, StringComparison comparison = 0)` |  |
| `RightUntil` | `static ReadOnlySpan<char> RightUntil(this ReadOnlySpan<char> @this, ReadOnlySpan<char> pattern, StringComparison comparison = 0)` |  |
| `RightUntil` | `static string RightUntil(this string @this, string pattern, StringComparison comparison = 0)` |  |
| `Right` | `static ReadOnlySpan<char> Right(this ReadOnlySpan<char> @this, int count)` |  |
| `Right` | `static string Right(this string @this, int count)` |  |
| `SanitizeForFileName` | `static string SanitizeForFileName(this string @this, char sanitation = '_')` |  |
| `Split` | `static IEnumerable<string> Split(this string @this, int length)` |  |
| `Split` | `static string[] Split(this string @this, Regex regex)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<char> values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<char> values, StringComparer comparer)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<char> values, StringComparison comparison)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<string> values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<string> values, StringComparer comparer)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, IEnumerable<string> values, StringComparison comparison)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, StringComparer comparer, params char[] values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, StringComparer comparer, params string[] values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, StringComparison comparison, params char[] values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, StringComparison comparison, params string[] values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, params char[] values)` |  |
| `StartsNotWithAny` | `static bool StartsNotWithAny(this string @this, params string[] values)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, char value, StringComparison stringComparison = 0)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, char what, StringComparer comparer)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, string value, StringComparison stringComparison = 0)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, string what, StringComparer comparer)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, string what, int index)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, string what, int index, StringComparer comparer)` |  |
| `StartsNotWith` | `static bool StartsNotWith(this string @this, string what, int index, StringComparison comparison)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<char> values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<char> values, StringComparer comparer)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<char> values, StringComparison stringComparison)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<string> values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<string> values, StringComparer comparer)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, StringComparer comparer, params char[] values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, StringComparer comparer, params string[] values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, StringComparison stringComparison, params char[] values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, StringComparison stringComparison, params string[] values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, params char[] values)` |  |
| `StartsWithAny` | `static bool StartsWithAny(this string @this, params string[] values)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, char value, StringComparison stringComparison = 0)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, char what, StringComparer comparer)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, string what, StringComparer comparer)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, string what, int index)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, string what, int index, StringComparer comparer)` |  |
| `StartsWith` | `static bool StartsWith(this string @this, string what, int index, StringComparison comparison)` |  |
| `SubString` | `static ReadOnlySpan<char> SubString(this ReadOnlySpan<char> @this, int start, int end = 0)` |  |
| `SubString` | `static string SubString(this string @this, int start, int end = 0)` |  |
| `Substring` | `static ReadOnlySpan<char> Substring(this ReadOnlySpan<char> @this, int position)` |  |
| `Substring` | `static ReadOnlySpan<char> Substring(this ReadOnlySpan<char> @this, int position, int count)` |  |
| `TextAnalysisFor` | `static TextAnalyzer TextAnalysisFor(this string @this, CultureInfo culture)` |  |
| `TextAnalysis` | `static TextAnalyzer TextAnalysis(this string @this)` |  |
| `ToCamelCaseInvariant` | `static string ToCamelCaseInvariant(this string @this)` |  |
| `ToCamelCase` | `static string ToCamelCase(this string @this, CultureInfo culture = null)` |  |
| `ToKebabCaseInvariant` | `static string ToKebabCaseInvariant(this string @this)` |  |
| `ToKebabCase` | `static string ToKebabCase(this string @this, CultureInfo culture = null)` |  |
| `ToLinq2SqlConnectionString` | `static string ToLinq2SqlConnectionString(this string @this)` |  |
| `ToPascalCaseInvariant` | `static string ToPascalCaseInvariant(this string @this)` |  |
| `ToPascalCase` | `static string ToPascalCase(this string @this, CultureInfo culture = null)` |  |
| `ToQuotedPrintable` | `static string ToQuotedPrintable(this string @this)` |  |
| `ToSnakeCaseInvariant` | `static string ToSnakeCaseInvariant(this string @this)` |  |
| `ToSnakeCase` | `static string ToSnakeCase(this string @this, CultureInfo culture = null)` |  |
| `ToUpperKebabCaseInvariant` | `static string ToUpperKebabCaseInvariant(this string @this)` |  |
| `ToUpperKebabCase` | `static string ToUpperKebabCase(this string @this, CultureInfo culture = null)` |  |
| `ToUpperSnakeCaseInvariant` | `static string ToUpperSnakeCaseInvariant(this string @this)` |  |
| `ToUpperSnakeCase` | `static string ToUpperSnakeCase(this string @this, CultureInfo culture = null)` |  |
| `TrimEnd` | `static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison stringComparison = 0)` |  |
| `TrimEnd` | `static string TrimEnd(this string @this, string what, StringComparison stringComparison = 0)` |  |
| `TrimStart` | `static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison stringComparison = 0)` |  |
| `TrimStart` | `static string TrimStart(this string @this, string what, StringComparison stringComparison = 0)` |  |
| `Truncate` | `static string Truncate(this string @this, int count)` |  |
| `Truncate` | `static string Truncate(this string @this, int count, TruncateMode mode)` |  |
| `Truncate` | `static string Truncate(this string @this, int count, TruncateMode mode, string ellipse)` |  |
| `Truncate` | `static string Truncate(this string @this, int count, string ellipse)` |  |
| `TryParseBoolean` | `static bool TryParseBoolean(this string @this, string trueValue, string falseValue, StringComparison stringComparison, out bool result)` |  |
| `TryParseBoolean` | `static bool TryParseBoolean(this string @this, string trueValue, string falseValue, out bool result)` |  |
| `TryParseByte` | `static bool TryParseByte(this ReadOnlySpan<char> @this, IFormatProvider provider, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this ReadOnlySpan<char> @this, NumberStyles style, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this ReadOnlySpan<char> @this, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this string @this, IFormatProvider provider, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this string @this, NumberStyles style, IFormatProvider provider, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this string @this, NumberStyles style, out byte result)` |  |
| `TryParseByte` | `static bool TryParseByte(this string @this, out byte result)` |  |
| `TryParseColor` | `static bool TryParseColor(this string @this, out Color result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this ReadOnlySpan<char> @this, IFormatProvider provider, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this ReadOnlySpan<char> @this, NumberStyles style, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this ReadOnlySpan<char> @this, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this string @this, IFormatProvider provider, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this string @this, NumberStyles style, IFormatProvider provider, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this string @this, NumberStyles style, out uint result)` |  |
| `TryParseDWord` | `static bool TryParseDWord(this string @this, out uint result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, DateTimeStyles style, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, DateTimeStyles style, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, DateTimeStyles style, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, DateTimeStyles style, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this ReadOnlySpan<char> @this, string exactFormat, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, DateTimeStyles style, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, DateTimeStyles style, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, string exactFormat, DateTimeStyles style, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, string exactFormat, DateTimeStyles style, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, string exactFormat, IFormatProvider provider, out DateTime result)` |  |
| `TryParseDateTime` | `static bool TryParseDateTime(this string @this, string exactFormat, out DateTime result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this ReadOnlySpan<char> @this, IFormatProvider provider, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this ReadOnlySpan<char> @this, NumberStyles style, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this ReadOnlySpan<char> @this, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this string @this, IFormatProvider provider, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this string @this, NumberStyles style, IFormatProvider provider, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this string @this, NumberStyles style, out decimal result)` |  |
| `TryParseDecimal` | `static bool TryParseDecimal(this string @this, out decimal result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this ReadOnlySpan<char> @this, IFormatProvider provider, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this ReadOnlySpan<char> @this, NumberStyles style, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this ReadOnlySpan<char> @this, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this string @this, IFormatProvider provider, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this string @this, NumberStyles style, IFormatProvider provider, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this string @this, NumberStyles style, out double result)` |  |
| `TryParseDouble` | `static bool TryParseDouble(this string @this, out double result)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum>(this ReadOnlySpan<char> @this, bool ignoreCase, out TEnum result)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum>(this ReadOnlySpan<char> @this, out TEnum result)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum>(this string @this, bool ignoreCase, out TEnum result)` |  |
| `TryParseEnum` | `static bool TryParseEnum<TEnum>(this string @this, out TEnum result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this ReadOnlySpan<char> @this, IFormatProvider provider, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this ReadOnlySpan<char> @this, NumberStyles style, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this ReadOnlySpan<char> @this, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this string @this, IFormatProvider provider, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this string @this, NumberStyles style, IFormatProvider provider, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this string @this, NumberStyles style, out float result)` |  |
| `TryParseFloat` | `static bool TryParseFloat(this string @this, out float result)` |  |
| `TryParseInt` | `static bool TryParseInt(this ReadOnlySpan<char> @this, IFormatProvider provider, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this ReadOnlySpan<char> @this, NumberStyles style, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this ReadOnlySpan<char> @this, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this string @this, IFormatProvider provider, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this string @this, NumberStyles style, IFormatProvider provider, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this string @this, NumberStyles style, out int result)` |  |
| `TryParseInt` | `static bool TryParseInt(this string @this, out int result)` |  |
| `TryParseLong` | `static bool TryParseLong(this ReadOnlySpan<char> @this, IFormatProvider provider, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this ReadOnlySpan<char> @this, NumberStyles style, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this ReadOnlySpan<char> @this, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this string @this, IFormatProvider provider, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this string @this, NumberStyles style, IFormatProvider provider, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this string @this, NumberStyles style, out long result)` |  |
| `TryParseLong` | `static bool TryParseLong(this string @this, out long result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this ReadOnlySpan<char> @this, IFormatProvider provider, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this ReadOnlySpan<char> @this, NumberStyles style, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this ReadOnlySpan<char> @this, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this string @this, IFormatProvider provider, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this string @this, NumberStyles style, IFormatProvider provider, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this string @this, NumberStyles style, out ulong result)` |  |
| `TryParseQWord` | `static bool TryParseQWord(this string @this, out ulong result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this ReadOnlySpan<char> @this, IFormatProvider provider, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this ReadOnlySpan<char> @this, NumberStyles style, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this ReadOnlySpan<char> @this, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this string @this, IFormatProvider provider, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this string @this, NumberStyles style, IFormatProvider provider, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this string @this, NumberStyles style, out sbyte result)` |  |
| `TryParseSByte` | `static bool TryParseSByte(this string @this, out sbyte result)` |  |
| `TryParseShort` | `static bool TryParseShort(this ReadOnlySpan<char> @this, IFormatProvider provider, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this ReadOnlySpan<char> @this, NumberStyles style, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this ReadOnlySpan<char> @this, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this string @this, IFormatProvider provider, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this string @this, NumberStyles style, IFormatProvider provider, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this string @this, NumberStyles style, out short result)` |  |
| `TryParseShort` | `static bool TryParseShort(this string @this, out short result)` |  |
| `TryParseTimeSpan` | `static bool TryParseTimeSpan(this ReadOnlySpan<char> @this, IFormatProvider provider, out TimeSpan result)` |  |
| `TryParseTimeSpan` | `static bool TryParseTimeSpan(this ReadOnlySpan<char> @this, out TimeSpan result)` |  |
| `TryParseTimeSpan` | `static bool TryParseTimeSpan(this string @this, IFormatProvider provider, out TimeSpan result)` |  |
| `TryParseTimeSpan` | `static bool TryParseTimeSpan(this string @this, out TimeSpan result)` |  |
| `TryParseWord` | `static bool TryParseWord(this ReadOnlySpan<char> @this, IFormatProvider provider, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this ReadOnlySpan<char> @this, NumberStyles style, IFormatProvider provider, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this ReadOnlySpan<char> @this, NumberStyles style, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this ReadOnlySpan<char> @this, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this string @this, IFormatProvider provider, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this string @this, NumberStyles style, IFormatProvider provider, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this string @this, NumberStyles style, out ushort result)` |  |
| `TryParseWord` | `static bool TryParseWord(this string @this, out ushort result)` |  |
| `UpperFirstInvariant` | `static string UpperFirstInvariant(this string @this)` |  |
| `UpperFirst` | `static string UpperFirst(this string @this, CultureInfo culture = null)` |  |
| `WordWrap` | `static string WordWrap(this string @this, int count, LineJoinMode mode)` |  |

#### `StringExtensions.CaseComparison`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Ordinal` | `0` |  |
| `CultureSpecific` | `1` |  |
| `InvariantCulture` | `2` |  |

#### `StringExtensions.HostEndPoint`

| Member | Signature | Summary |
| --- | --- | --- |
| `HostEndPoint` | `HostEndPoint(string host, int port)` |  |
| `Host` | `string Host { get; }` |  |
| `Port` | `int Port { get; }` |  |
| `explicit operator IPEndPoint` | `static explicit operator IPEndPoint(HostEndPoint @this)` |  |

#### `StringExtensions.LineBreakMode`

| Value | Numeric | Summary |
| --- | --- | --- |
| `All` | `-3` |  |
| `AutoDetect` | `-2` |  |
| `None` | `-1` |  |
| `CarriageReturn` | `13` |  |
| `LineFeed` | `10` |  |
| `CrLf` | `3338` |  |
| `LfCr` | `2573` |  |
| `FormFeed` | `12` |  |
| `NextLine` | `133` |  |
| `LineSeparator` | `8232` |  |
| `ParagraphSeparator` | `8233` |  |
| `NegativeAcknowledge` | `21` |  |
| `EndOfLine` | `155` |  |
| `Zx` | `118` |  |
| `Null` | `0` |  |
| `OSX` | `10` |  |
| `Linux` | `10` |  |
| `Posix` | `10` |  |
| `Unix` | `10` |  |
| `MacOS` | `10` |  |
| `BSD` | `10` |  |
| `Amiga` | `10` |  |
| `ClassicMacOS` | `13` |  |
| `Commodore` | `13` |  |
| `ZXSpectrum` | `13` |  |
| `Dos` | `3338` |  |
| `Windows` | `3338` |  |
| `SymbianOS` | `3338` |  |
| `Cpm` | `3338` |  |
| `PalmOS` | `3338` |  |
| `AmstradCPC` | `3338` |  |
| `AcornBBC` | `2573` |  |
| `IBM` | `21` |  |
| `Atari` | `155` |  |
| `Zx8` | `118` |  |

#### `StringExtensions.LineJoinMode`

| Value | Numeric | Summary |
| --- | --- | --- |
| `CarriageReturn` | `13` |  |
| `LineFeed` | `10` |  |
| `CrLf` | `3338` |  |
| `LfCr` | `2573` |  |
| `FormFeed` | `12` |  |
| `NextLine` | `133` |  |
| `LineSeparator` | `8232` |  |
| `ParagraphSeparator` | `8233` |  |
| `NegativeAcknowledge` | `21` |  |
| `EndOfLine` | `155` |  |
| `Zx` | `118` |  |
| `Null` | `0` |  |
| `OSX` | `10` |  |
| `Linux` | `10` |  |
| `Posix` | `10` |  |
| `Unix` | `10` |  |
| `MacOS` | `10` |  |
| `BSD` | `10` |  |
| `Amiga` | `10` |  |
| `ClassicMacOS` | `13` |  |
| `Commodore` | `13` |  |
| `ZXSpectrum` | `13` |  |
| `Dos` | `3338` |  |
| `Windows` | `3338` |  |
| `SymbianOS` | `3338` |  |
| `Cpm` | `3338` |  |
| `PalmOS` | `3338` |  |
| `AmstradCPC` | `3338` |  |
| `AcornBBC` | `2573` |  |
| `IBM` | `21` |  |
| `Atari` | `155` |  |
| `Zx8` | `118` |  |

#### `StringExtensions.TextAnalyzer`

| Member | Signature | Summary |
| --- | --- | --- |
| `DistinctWords` | `IEnumerable<string> DistinctWords { get; }` |  |
| `ReadabilityScore` | `ReadabilityScoreCalculator ReadabilityScore { get; }` |  |
| `Sentences` | `string[] Sentences { get; }` |  |
| `TotalSyllables` | `int TotalSyllables { get; }` |  |
| `WordHistogram` | `IDictionary<string, int> WordHistogram { get; }` |  |
| `Words` | `string[] Words { get; }` |  |

#### `StringExtensions.TextAnalyzer.ReadabilityScoreCalculator`

| Member | Signature | Summary |
| --- | --- | --- |
| `Ari` | `double Ari { get; }` |  |
| `ColemanLiau` | `double ColemanLiau { get; }` |  |
| `FleschKincaid` | `double FleschKincaid { get; }` |  |
| `FleschReadingEase` | `double FleschReadingEase { get; }` |  |
| `GunningFog` | `double GunningFog { get; }` |  |
| `Lix` | `double Lix { get; }` |  |
| `Smog` | `double Smog { get; }` |  |
| `Wstf` | `double Wstf { get; }` |  |

#### `StringExtensions.TruncateMode`

| Value | Numeric | Summary |
| --- | --- | --- |
| `KeepStart` | `0` |  |
| `KeepEnd` | `1` |  |
| `KeepStartAndEnd` | `2` |  |
| `KeepMiddle` | `3` |  |

#### `SunG711`

Implements `IG711Convention`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EncodeALaw` | `byte EncodeALaw(short pcm)` |  |
| `EncodeMuLaw` | `byte EncodeMuLaw(short pcm)` |  |

#### `TF32`

Implements `IComparable`, `IComparable<TF32>`, `IEquatable<TF32>`, `IFormattable`, `IParsable<TF32>`, `ISpanFormattable`, `ISpanParsable<TF32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static TF32 MaxValue { get; }` |  |
| `MinValue` | `static TF32 MinValue { get; }` |  |
| `NaN` | `static TF32 NaN { get; }` |  |
| `NegativeInfinity` | `static TF32 NegativeInfinity { get; }` |  |
| `One` | `static TF32 One { get; }` |  |
| `PositiveInfinity` | `static TF32 PositiveInfinity { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static TF32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(TF32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(TF32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static TF32 FromDouble(double value)` |  |
| `FromRaw` | `static TF32 FromRaw(uint raw)` |  |
| `FromSingle` | `static TF32 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsFinite` | `static bool IsFinite(TF32 value)` |  |
| `IsInfinity` | `static bool IsInfinity(TF32 value)` |  |
| `IsNaN` | `static bool IsNaN(TF32 value)` |  |
| `Parse` | `static TF32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static TF32 Parse(string s)` |  |
| `Parse` | `static TF32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static TF32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out TF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out TF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out TF32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out TF32 result)` |  |
| `explicit operator TF32` | `static explicit operator TF32(double value)` |  |
| `explicit operator TF32` | `static explicit operator TF32(float value)` |  |
| `implicit operator double` | `static implicit operator double(TF32 value)` |  |
| `implicit operator float` | `static implicit operator float(TF32 value)` |  |
| `operator !=` | `static bool operator !=(TF32 left, TF32 right)` |  |
| `operator *` | `static TF32 operator *(TF32 left, TF32 right)` |  |
| `operator +` | `static TF32 operator +(TF32 left, TF32 right)` |  |
| `operator +` | `static TF32 operator +(TF32 value)` |  |
| `operator -` | `static TF32 operator -(TF32 left, TF32 right)` |  |
| `operator -` | `static TF32 operator -(TF32 value)` |  |
| `operator /` | `static TF32 operator /(TF32 left, TF32 right)` |  |
| `operator <=` | `static bool operator <=(TF32 left, TF32 right)` |  |
| `operator <` | `static bool operator <(TF32 left, TF32 right)` |  |
| `operator ==` | `static bool operator ==(TF32 left, TF32 right)` |  |
| `operator >=` | `static bool operator >=(TF32 left, TF32 right)` |  |
| `operator >` | `static bool operator >(TF32 left, TF32 right)` |  |

#### `TimeSpanExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `CurrenDrift` | `static TimeSpan CurrenDrift(this TimeSpan @this)` |  |
| `CurrenIteration` | `static double CurrenIteration(this TimeSpan @this)` |  |
| `CurrenIteration` | `static double CurrenIteration(this TimeSpan @this, ulong maxIterations)` |  |
| `Days` | `static TimeSpan Days(this byte @this)` |  |
| `Days` | `static TimeSpan Days(this decimal @this)` |  |
| `Days` | `static TimeSpan Days(this double @this)` |  |
| `Days` | `static TimeSpan Days(this float @this)` |  |
| `Days` | `static TimeSpan Days(this int @this)` |  |
| `Days` | `static TimeSpan Days(this long @this)` |  |
| `Days` | `static TimeSpan Days(this sbyte @this)` |  |
| `Days` | `static TimeSpan Days(this short @this)` |  |
| `Days` | `static TimeSpan Days(this uint @this)` |  |
| `Days` | `static TimeSpan Days(this ulong @this)` |  |
| `Days` | `static TimeSpan Days(this ushort @this)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, byte divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, decimal divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, double divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, float divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, int divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, long divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, sbyte divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, short divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, uint divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, ulong divisor)` |  |
| `DividedBy` | `static TimeSpan DividedBy(this TimeSpan @this, ushort divisor)` |  |
| `DividedBy` | `static double DividedBy(this TimeSpan @this, TimeSpan divisor)` |  |
| `FromNow` | `static DateTime FromNow(this TimeSpan @this)` |  |
| `FromStopwatchTimeStamp` | `static long FromStopwatchTimeStamp(this TimeSpan @this)` |  |
| `FromUtcNow` | `static DateTime FromUtcNow(this TimeSpan @this)` |  |
| `Hours` | `static TimeSpan Hours(this byte @this)` |  |
| `Hours` | `static TimeSpan Hours(this decimal @this)` |  |
| `Hours` | `static TimeSpan Hours(this double @this)` |  |
| `Hours` | `static TimeSpan Hours(this float @this)` |  |
| `Hours` | `static TimeSpan Hours(this int @this)` |  |
| `Hours` | `static TimeSpan Hours(this long @this)` |  |
| `Hours` | `static TimeSpan Hours(this sbyte @this)` |  |
| `Hours` | `static TimeSpan Hours(this short @this)` |  |
| `Hours` | `static TimeSpan Hours(this uint @this)` |  |
| `Hours` | `static TimeSpan Hours(this ulong @this)` |  |
| `Hours` | `static TimeSpan Hours(this ushort @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this byte @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this decimal @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this double @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this float @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this int @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this long @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this sbyte @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this short @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this uint @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this ulong @this)` |  |
| `Milliseconds` | `static TimeSpan Milliseconds(this ushort @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this byte @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this decimal @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this double @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this float @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this int @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this long @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this sbyte @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this short @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this uint @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this ulong @this)` |  |
| `Minutes` | `static TimeSpan Minutes(this ushort @this)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, byte multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, decimal multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, double multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, float multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, int multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, long multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, sbyte multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, short multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, uint multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, ulong multiplier)` |  |
| `MultipliedWith` | `static TimeSpan MultipliedWith(this TimeSpan @this, ushort multiplier)` |  |
| `Seconds` | `static TimeSpan Seconds(this byte @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this decimal @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this double @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this float @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this int @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this long @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this sbyte @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this short @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this uint @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this ulong @this)` |  |
| `Seconds` | `static TimeSpan Seconds(this ushort @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this byte @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this decimal @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this double @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this float @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this int @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this long @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this sbyte @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this short @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this uint @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this ulong @this)` |  |
| `Weeks` | `static TimeSpan Weeks(this ushort @this)` |  |

#### `TypeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `TypeBool` | `static readonly Type TypeBool` |  |
| `TypeByte` | `static readonly Type TypeByte` |  |
| `TypeChar` | `static readonly Type TypeChar` |  |
| `TypeDWord` | `static readonly Type TypeDWord` |  |
| `TypeDateTime` | `static readonly Type TypeDateTime` |  |
| `TypeDecimal` | `static readonly Type TypeDecimal` |  |
| `TypeDouble` | `static readonly Type TypeDouble` |  |
| `TypeFloat` | `static readonly Type TypeFloat` |  |
| `TypeInt` | `static readonly Type TypeInt` |  |
| `TypeLong` | `static readonly Type TypeLong` |  |
| `TypeObject` | `static readonly Type TypeObject` |  |
| `TypeQWord` | `static readonly Type TypeQWord` |  |
| `TypeSByte` | `static readonly Type TypeSByte` |  |
| `TypeShort` | `static readonly Type TypeShort` |  |
| `TypeString` | `static readonly Type TypeString` |  |
| `TypeTimeSpan` | `static readonly Type TypeTimeSpan` |  |
| `TypeVoid` | `static readonly Type TypeVoid` |  |
| `TypeWord` | `static readonly Type TypeWord` |  |
| `CreateInstance` | `static TType CreateInstance<TType>(this Type @this)` |  |
| `CreateInstance` | `static TType CreateInstance<TType>(this Type @this, params object[] parameters)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4, TParam5>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3, TParam4>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2, TParam3>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1, TParam2>(this Type @this, TParam0 param0, TParam1 param1, TParam2 param2)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0, TParam1>(this Type @this, TParam0 param0, TParam1 param1)` |  |
| `FromConstructor` | `static TType FromConstructor<TType, TParam0>(this Type @this, TParam0 param0)` |  |
| `GetAssemblyAttribute` | `static TAttribute GetAssemblyAttribute<TAttribute>(this Type @this, bool inherit = false, int index = 0)` |  |
| `GetAttributes` | `static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type @this, bool inherit = false)` |  |
| `GetDefaultValue` | `static object GetDefaultValue(this Type @this)` |  |
| `GetDescription` | `static string GetDescription(this Type @this)` |  |
| `GetDesignerProperties` | `static PropertyDesignerDetails[] GetDesignerProperties(this Type @this, BindingFlags? bindingFlags = null)` |  |
| `GetDisplayName` | `static string GetDisplayName(this Type @this)` |  |
| `GetFieldOrPropertyAttributeValue` | `static TValue GetFieldOrPropertyAttributeValue<TAttributeType, TValue>(this Type @this, string fieldName, Func<TAttributeType, TValue> getter)` |  |
| `GetImplementedTypes` | `static IEnumerable<Type> GetImplementedTypes(this Type @this)` |  |
| `GetMaxValueForIntType` | `static decimal GetMaxValueForIntType(this Type @this)` |  |
| `GetMinValueForIntType` | `static decimal GetMinValueForIntType(this Type @this)` |  |
| `GetRandomValue` | `static object GetRandomValue(this Type @this, bool allowInstanceCreationForReferenceTypes = false)` |  |
| `GetStaticFieldValue` | `static TType GetStaticFieldValue<TType>(this Type @this, string name)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> GetStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> GetStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5, T6, T7, T8> GetStaticMethod<T1, T2, T3, T4, T5, T6, T7, T8>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5, T6, T7> GetStaticMethod<T1, T2, T3, T4, T5, T6, T7>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5, T6> GetStaticMethod<T1, T2, T3, T4, T5, T6>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4, T5> GetStaticMethod<T1, T2, T3, T4, T5>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3, T4> GetStaticMethod<T1, T2, T3, T4>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2, T3> GetStaticMethod<T1, T2, T3>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T1, T2> GetStaticMethod<T1, T2>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticMethod` | `static Action<T> GetStaticMethod<T>(this Type @this, string name, BindingFlags flags = 16)` |  |
| `GetStaticPropertyValue` | `static TType GetStaticPropertyValue<TType>(this Type @this, string name)` |  |
| `IsBooleanType` | `static bool IsBooleanType(this Type @this)` |  |
| `IsCastableFrom` | `static bool IsCastableFrom(this Type @this, Type source)` |  |
| `IsCastableTo` | `static bool IsCastableTo(this Type @this, Type target)` |  |
| `IsDateTimeType` | `static bool IsDateTimeType(this Type @this)` |  |
| `IsDecimalType` | `static bool IsDecimalType(this Type @this)` |  |
| `IsEnumType` | `static bool IsEnumType(this Type @this)` |  |
| `IsFloatType` | `static bool IsFloatType(this Type @this)` |  |
| `IsIntegerType` | `static bool IsIntegerType(this Type @this)` |  |
| `IsNullable` | `static bool IsNullable(this Type @this)` |  |
| `IsSigned` | `static bool IsSigned(this Type @this)` |  |
| `IsStringType` | `static bool IsStringType(this Type @this)` |  |
| `IsTimeSpanType` | `static bool IsTimeSpanType(this Type @this)` |  |
| `IsUnsigned` | `static bool IsUnsigned(this Type @this)` |  |
| `SimpleName` | `static string SimpleName(this Type @this, bool useLanguageTypes = false)` |  |

#### `TypeExtensions.PropertyDesignerDetails`

| Member | Signature | Summary |
| --- | --- | --- |
| `PropertyDesignerDetails` | `PropertyDesignerDetails(PropertyInfo info)` |  |
| `Info` | `readonly PropertyInfo Info` |  |
| `Browsable` | `bool Browsable { get; }` |  |
| `Category` | `string Category { get; }` |  |
| `Descriptions` | `string[] Descriptions { get; }` |  |
| `DisplayName` | `string DisplayName { get; }` |  |
| `EditorBrowseableStates` | `IEnumerable<EditorBrowsableState> EditorBrowseableStates { get; }` |  |
| `IsReadable` | `bool IsReadable { get; }` |  |
| `IsWritable` | `bool IsWritable { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `PropertyType` | `Type PropertyType { get; }` |  |
| `ReadOnly` | `bool ReadOnly { get; }` |  |
| `GetValueOrDefault` | `TValue GetValueOrDefault<TValue>()` |  |
| `GetValueOrDefault` | `TValue GetValueOrDefault<TValue>(TValue defaultValue)` |  |
| `GetValueOrDefault` | `TValue GetValueOrDefault<TValue>(object instance)` |  |
| `GetValueOrDefault` | `TValue GetValueOrDefault<TValue>(object instance, TValue defaultValue)` |  |
| `GetValueOrDefault` | `object GetValueOrDefault(object defaultValue = null)` |  |
| `GetValueOrDefault` | `object GetValueOrDefault(object instance, object defaultValue)` |  |
| `GetValue` | `TValue GetValue<TValue>(object instance)` |  |
| `GetValue` | `object GetValue(object instance)` |  |
| `SetValue` | `void SetValue(object instance, object value)` |  |
| `SetValue` | `void SetValue(object value)` |  |
| `TryGetValue` | `bool TryGetValue(object instance, out object value)` |  |
| `TryGetValue` | `bool TryGetValue(out object value)` |  |
| `TryGetValue` | `bool TryGetValue<TValue>(object instance, out TValue value)` |  |
| `TryGetValue` | `bool TryGetValue<TValue>(out TValue value)` |  |
| `TrySetValue` | `bool TrySetValue(object instance, object value)` |  |
| `TrySetValue` | `bool TrySetValue(object value)` |  |

#### `UInt16Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this ushort @this, char character)` |  |
| `Times` | `static string Times(this ushort @this, string text)` |  |
| `Times` | `static void Times(this ushort @this, Action action)` |  |
| `Times` | `static void Times(this ushort @this, Action<ushort> action)` |  |

#### `UInt32Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this uint @this, char character)` |  |
| `Times` | `static string Times(this uint @this, string text)` |  |
| `Times` | `static void Times(this uint @this, Action action)` |  |
| `Times` | `static void Times(this uint @this, Action<uint> action)` |  |

#### `UInt64Extensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Times` | `static string Times(this ulong @this, char character)` |  |
| `Times` | `static string Times(this ulong @this, string text)` |  |
| `Times` | `static void Times(this ulong @this, Action action)` |  |
| `Times` | `static void Times(this ulong @this, Action<ulong> action)` |  |

#### `UInt96`

Implements `IComparable`, `IComparable<UInt96>`, `IEquatable<UInt96>`, `IFormattable`, `IParsable<UInt96>`, `ISpanFormattable`, `ISpanParsable<UInt96>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `UInt96` | `UInt96(uint upper, ulong lower)` |  |
| `MaxValue` | `static UInt96 MaxValue { get; }` |  |
| `MinValue` | `static UInt96 MinValue { get; }` |  |
| `One` | `static UInt96 One { get; }` |  |
| `Zero` | `static UInt96 Zero { get; }` |  |
| `Clamp` | `static UInt96 Clamp(UInt96 value, UInt96 min, UInt96 max)` |  |
| `CompareTo` | `int CompareTo(UInt96 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `DivRem` | `static ValueTuple<UInt96, UInt96> DivRem(UInt96 left, UInt96 right)` |  |
| `Equals` | `bool Equals(UInt96 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsEvenInteger` | `static bool IsEvenInteger(UInt96 value)` |  |
| `IsOddInteger` | `static bool IsOddInteger(UInt96 value)` |  |
| `IsPow2` | `static bool IsPow2(UInt96 value)` |  |
| `LeadingZeroCount` | `static int LeadingZeroCount(UInt96 value)` |  |
| `Log2` | `static int Log2(UInt96 value)` |  |
| `Max` | `static UInt96 Max(UInt96 x, UInt96 y)` |  |
| `Min` | `static UInt96 Min(UInt96 x, UInt96 y)` |  |
| `Parse` | `static UInt96 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UInt96 Parse(string s)` |  |
| `Parse` | `static UInt96 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UInt96 Parse(string s, NumberStyles style)` |  |
| `Parse` | `static UInt96 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `PopCount` | `static int PopCount(UInt96 value)` |  |
| `RotateLeft` | `static UInt96 RotateLeft(UInt96 value, int rotateAmount)` |  |
| `RotateRight` | `static UInt96 RotateRight(UInt96 value, int rotateAmount)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TrailingZeroCount` | `static int TrailingZeroCount(UInt96 value)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UInt96 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UInt96 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UInt96 result)` |  |
| `TryParse` | `static bool TryParse(string s, out UInt96 result)` |  |
| `explicit operator Half` | `static explicit operator Half(UInt96 value)` |  |
| `explicit operator Int96` | `static explicit operator Int96(UInt96 value)` |  |
| `explicit operator Quarter` | `static explicit operator Quarter(UInt96 value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(Half value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(Int96 value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(Quarter value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(decimal value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(double value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(float value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(int value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(long value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(sbyte value)` |  |
| `explicit operator UInt96` | `static explicit operator UInt96(short value)` |  |
| `explicit operator byte` | `static explicit operator byte(UInt96 value)` |  |
| `explicit operator char` | `static explicit operator char(UInt96 value)` |  |
| `explicit operator decimal` | `static explicit operator decimal(UInt96 value)` |  |
| `explicit operator double` | `static explicit operator double(UInt96 value)` |  |
| `explicit operator float` | `static explicit operator float(UInt96 value)` |  |
| `explicit operator int` | `static explicit operator int(UInt96 value)` |  |
| `explicit operator long` | `static explicit operator long(UInt96 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(UInt96 value)` |  |
| `explicit operator short` | `static explicit operator short(UInt96 value)` |  |
| `explicit operator uint` | `static explicit operator uint(UInt96 value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(UInt96 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(UInt96 value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(byte value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(char value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(uint value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(ulong value)` |  |
| `implicit operator UInt96` | `static implicit operator UInt96(ushort value)` |  |
| `operator !=` | `static bool operator !=(UInt96 left, UInt96 right)` |  |
| `operator %` | `static UInt96 operator %(UInt96 left, UInt96 right)` |  |
| `operator &` | `static UInt96 operator &(UInt96 left, UInt96 right)` |  |
| `operator *` | `static UInt96 operator *(UInt96 left, UInt96 right)` |  |
| `operator ++` | `static UInt96 operator ++(UInt96 value)` |  |
| `operator +` | `static UInt96 operator +(UInt96 left, UInt96 right)` |  |
| `operator +` | `static UInt96 operator +(UInt96 value)` |  |
| `operator --` | `static UInt96 operator --(UInt96 value)` |  |
| `operator -` | `static UInt96 operator -(UInt96 left, UInt96 right)` |  |
| `operator /` | `static UInt96 operator /(UInt96 left, UInt96 right)` |  |
| `operator <<` | `static UInt96 operator <<(UInt96 value, int shiftAmount)` |  |
| `operator <=` | `static bool operator <=(UInt96 left, UInt96 right)` |  |
| `operator <` | `static bool operator <(UInt96 left, UInt96 right)` |  |
| `operator ==` | `static bool operator ==(UInt96 left, UInt96 right)` |  |
| `operator >=` | `static bool operator >=(UInt96 left, UInt96 right)` |  |
| `operator >>>` | `static UInt96 operator >>>(UInt96 value, int shiftAmount)` |  |
| `operator >>` | `static UInt96 operator >>(UInt96 value, int shiftAmount)` |  |
| `operator >` | `static bool operator >(UInt96 left, UInt96 right)` |  |
| `operator ^` | `static UInt96 operator ^(UInt96 left, UInt96 right)` |  |
| `operator \|` | `static UInt96 operator \|(UInt96 left, UInt96 right)` |  |
| `operator ~` | `static UInt96 operator ~(UInt96 value)` |  |

#### `UQ16_16`

Implements `IComparable`, `IComparable<UQ16_16>`, `IEquatable<UQ16_16>`, `IFormattable`, `IParsable<UQ16_16>`, `ISpanFormattable`, `ISpanParsable<UQ16_16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static UQ16_16 Epsilon { get; }` |  |
| `MaxValue` | `static UQ16_16 MaxValue { get; }` |  |
| `MinValue` | `static UQ16_16 MinValue { get; }` |  |
| `One` | `static UQ16_16 One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static UQ16_16 Zero { get; }` |  |
| `Clamp` | `static UQ16_16 Clamp(UQ16_16 value, UQ16_16 min, UQ16_16 max)` |  |
| `CompareTo` | `int CompareTo(UQ16_16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UQ16_16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static UQ16_16 FromDouble(double value)` |  |
| `FromRaw` | `static UQ16_16 FromRaw(uint raw)` |  |
| `FromSingle` | `static UQ16_16 FromSingle(float value)` |  |
| `FromUInt32` | `static UQ16_16 FromUInt32(uint value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static UQ16_16 Max(UQ16_16 left, UQ16_16 right)` |  |
| `Min` | `static UQ16_16 Min(UQ16_16 left, UQ16_16 right)` |  |
| `Parse` | `static UQ16_16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UQ16_16 Parse(string s)` |  |
| `Parse` | `static UQ16_16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UQ16_16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UQ16_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UQ16_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UQ16_16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out UQ16_16 result)` |  |
| `explicit operator UQ16_16` | `static explicit operator UQ16_16(UQ32_32 value)` |  |
| `explicit operator UQ16_16` | `static explicit operator UQ16_16(double value)` |  |
| `explicit operator UQ16_16` | `static explicit operator UQ16_16(float value)` |  |
| `explicit operator UQ16_16` | `static explicit operator UQ16_16(uint raw)` |  |
| `explicit operator UQ8_8` | `static explicit operator UQ8_8(UQ16_16 value)` |  |
| `explicit operator byte` | `static explicit operator byte(UQ16_16 value)` |  |
| `explicit operator uint` | `static explicit operator uint(UQ16_16 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(UQ16_16 value)` |  |
| `implicit operator UQ16_16` | `static implicit operator UQ16_16(byte value)` |  |
| `implicit operator UQ16_16` | `static implicit operator UQ16_16(ushort value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(UQ16_16 value)` |  |
| `implicit operator double` | `static implicit operator double(UQ16_16 value)` |  |
| `implicit operator float` | `static implicit operator float(UQ16_16 value)` |  |
| `operator !=` | `static bool operator !=(UQ16_16 left, UQ16_16 right)` |  |
| `operator %` | `static UQ16_16 operator %(UQ16_16 left, UQ16_16 right)` |  |
| `operator *` | `static UQ16_16 operator *(UQ16_16 left, UQ16_16 right)` |  |
| `operator *` | `static UQ16_16 operator *(UQ16_16 left, int right)` |  |
| `operator *` | `static UQ16_16 operator *(int left, UQ16_16 right)` |  |
| `operator ++` | `static UQ16_16 operator ++(UQ16_16 value)` |  |
| `operator +` | `static UQ16_16 operator +(UQ16_16 left, UQ16_16 right)` |  |
| `operator +` | `static UQ16_16 operator +(UQ16_16 left, int right)` |  |
| `operator +` | `static UQ16_16 operator +(UQ16_16 value)` |  |
| `operator +` | `static UQ16_16 operator +(int left, UQ16_16 right)` |  |
| `operator --` | `static UQ16_16 operator --(UQ16_16 value)` |  |
| `operator -` | `static UQ16_16 operator -(UQ16_16 left, UQ16_16 right)` |  |
| `operator -` | `static UQ16_16 operator -(UQ16_16 left, int right)` |  |
| `operator -` | `static UQ16_16 operator -(int left, UQ16_16 right)` |  |
| `operator /` | `static UQ16_16 operator /(UQ16_16 left, UQ16_16 right)` |  |
| `operator /` | `static UQ16_16 operator /(UQ16_16 left, int right)` |  |
| `operator <=` | `static bool operator <=(UQ16_16 left, UQ16_16 right)` |  |
| `operator <` | `static bool operator <(UQ16_16 left, UQ16_16 right)` |  |
| `operator ==` | `static bool operator ==(UQ16_16 left, UQ16_16 right)` |  |
| `operator >=` | `static bool operator >=(UQ16_16 left, UQ16_16 right)` |  |
| `operator >` | `static bool operator >(UQ16_16 left, UQ16_16 right)` |  |

#### `UQ32_32`

Implements `IComparable`, `IComparable<UQ32_32>`, `IEquatable<UQ32_32>`, `IFormattable`, `IParsable<UQ32_32>`, `ISpanFormattable`, `ISpanParsable<UQ32_32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static UQ32_32 Epsilon { get; }` |  |
| `MaxValue` | `static UQ32_32 MaxValue { get; }` |  |
| `MinValue` | `static UQ32_32 MinValue { get; }` |  |
| `One` | `static UQ32_32 One { get; }` |  |
| `RawValue` | `ulong RawValue { get; }` |  |
| `Zero` | `static UQ32_32 Zero { get; }` |  |
| `Clamp` | `static UQ32_32 Clamp(UQ32_32 value, UQ32_32 min, UQ32_32 max)` |  |
| `CompareTo` | `int CompareTo(UQ32_32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UQ32_32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static UQ32_32 FromDouble(double value)` |  |
| `FromRaw` | `static UQ32_32 FromRaw(ulong raw)` |  |
| `FromSingle` | `static UQ32_32 FromSingle(float value)` |  |
| `FromUInt32` | `static UQ32_32 FromUInt32(uint value)` |  |
| `FromUInt64` | `static UQ32_32 FromUInt64(ulong value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static UQ32_32 Max(UQ32_32 left, UQ32_32 right)` |  |
| `Min` | `static UQ32_32 Min(UQ32_32 left, UQ32_32 right)` |  |
| `Parse` | `static UQ32_32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UQ32_32 Parse(string s)` |  |
| `Parse` | `static UQ32_32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UQ32_32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32()` |  |
| `ToUInt64` | `ulong ToUInt64()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UQ32_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UQ32_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UQ32_32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out UQ32_32 result)` |  |
| `explicit operator UQ16_16` | `static explicit operator UQ16_16(UQ32_32 value)` |  |
| `explicit operator UQ32_32` | `static explicit operator UQ32_32(double value)` |  |
| `explicit operator UQ32_32` | `static explicit operator UQ32_32(float value)` |  |
| `explicit operator UQ32_32` | `static explicit operator UQ32_32(ulong raw)` |  |
| `explicit operator UQ8_8` | `static explicit operator UQ8_8(UQ32_32 value)` |  |
| `explicit operator byte` | `static explicit operator byte(UQ32_32 value)` |  |
| `explicit operator float` | `static explicit operator float(UQ32_32 value)` |  |
| `explicit operator uint` | `static explicit operator uint(UQ32_32 value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(UQ32_32 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(UQ32_32 value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(byte value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(uint value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(ushort value)` |  |
| `implicit operator double` | `static implicit operator double(UQ32_32 value)` |  |
| `operator !=` | `static bool operator !=(UQ32_32 left, UQ32_32 right)` |  |
| `operator %` | `static UQ32_32 operator %(UQ32_32 left, UQ32_32 right)` |  |
| `operator *` | `static UQ32_32 operator *(UQ32_32 left, UQ32_32 right)` |  |
| `operator *` | `static UQ32_32 operator *(UQ32_32 left, long right)` |  |
| `operator *` | `static UQ32_32 operator *(long left, UQ32_32 right)` |  |
| `operator ++` | `static UQ32_32 operator ++(UQ32_32 value)` |  |
| `operator +` | `static UQ32_32 operator +(UQ32_32 left, UQ32_32 right)` |  |
| `operator +` | `static UQ32_32 operator +(UQ32_32 left, long right)` |  |
| `operator +` | `static UQ32_32 operator +(UQ32_32 value)` |  |
| `operator +` | `static UQ32_32 operator +(long left, UQ32_32 right)` |  |
| `operator --` | `static UQ32_32 operator --(UQ32_32 value)` |  |
| `operator -` | `static UQ32_32 operator -(UQ32_32 left, UQ32_32 right)` |  |
| `operator -` | `static UQ32_32 operator -(UQ32_32 left, long right)` |  |
| `operator -` | `static UQ32_32 operator -(long left, UQ32_32 right)` |  |
| `operator /` | `static UQ32_32 operator /(UQ32_32 left, UQ32_32 right)` |  |
| `operator /` | `static UQ32_32 operator /(UQ32_32 left, long right)` |  |
| `operator <=` | `static bool operator <=(UQ32_32 left, UQ32_32 right)` |  |
| `operator <` | `static bool operator <(UQ32_32 left, UQ32_32 right)` |  |
| `operator ==` | `static bool operator ==(UQ32_32 left, UQ32_32 right)` |  |
| `operator >=` | `static bool operator >=(UQ32_32 left, UQ32_32 right)` |  |
| `operator >` | `static bool operator >(UQ32_32 left, UQ32_32 right)` |  |

#### `UQ4_4`

Implements `IComparable`, `IComparable<UQ4_4>`, `IEquatable<UQ4_4>`, `IFormattable`, `IParsable<UQ4_4>`, `ISpanFormattable`, `ISpanParsable<UQ4_4>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static UQ4_4 Epsilon { get; }` |  |
| `MaxValue` | `static UQ4_4 MaxValue { get; }` |  |
| `MinValue` | `static UQ4_4 MinValue { get; }` |  |
| `One` | `static UQ4_4 One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Zero` | `static UQ4_4 Zero { get; }` |  |
| `Clamp` | `static UQ4_4 Clamp(UQ4_4 value, UQ4_4 min, UQ4_4 max)` |  |
| `CompareTo` | `int CompareTo(UQ4_4 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UQ4_4 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static UQ4_4 FromDouble(double value)` |  |
| `FromInt32` | `static UQ4_4 FromInt32(int value)` |  |
| `FromRaw` | `static UQ4_4 FromRaw(byte raw)` |  |
| `FromSingle` | `static UQ4_4 FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static UQ4_4 Max(UQ4_4 left, UQ4_4 right)` |  |
| `Min` | `static UQ4_4 Min(UQ4_4 left, UQ4_4 right)` |  |
| `Parse` | `static UQ4_4 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UQ4_4 Parse(string s)` |  |
| `Parse` | `static UQ4_4 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UQ4_4 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToInt32` | `int ToInt32()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UQ4_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UQ4_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UQ4_4 result)` |  |
| `TryParse` | `static bool TryParse(string s, out UQ4_4 result)` |  |
| `explicit operator UQ4_4` | `static explicit operator UQ4_4(double value)` |  |
| `explicit operator UQ4_4` | `static explicit operator UQ4_4(float value)` |  |
| `explicit operator UQ4_4` | `static explicit operator UQ4_4(sbyte raw)` |  |
| `explicit operator byte` | `static explicit operator byte(UQ4_4 value)` |  |
| `explicit operator int` | `static explicit operator int(UQ4_4 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(UQ4_4 value)` |  |
| `implicit operator UQ16_16` | `static implicit operator UQ16_16(UQ4_4 value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(UQ4_4 value)` |  |
| `implicit operator UQ4_4` | `static implicit operator UQ4_4(byte value)` |  |
| `implicit operator UQ8_8` | `static implicit operator UQ8_8(UQ4_4 value)` |  |
| `implicit operator double` | `static implicit operator double(UQ4_4 value)` |  |
| `implicit operator float` | `static implicit operator float(UQ4_4 value)` |  |
| `operator !=` | `static bool operator !=(UQ4_4 left, UQ4_4 right)` |  |
| `operator %` | `static UQ4_4 operator %(UQ4_4 left, UQ4_4 right)` |  |
| `operator *` | `static UQ4_4 operator *(UQ4_4 left, UQ4_4 right)` |  |
| `operator *` | `static UQ4_4 operator *(UQ4_4 left, int right)` |  |
| `operator *` | `static UQ4_4 operator *(int left, UQ4_4 right)` |  |
| `operator ++` | `static UQ4_4 operator ++(UQ4_4 value)` |  |
| `operator +` | `static UQ4_4 operator +(UQ4_4 left, UQ4_4 right)` |  |
| `operator +` | `static UQ4_4 operator +(UQ4_4 left, int right)` |  |
| `operator +` | `static UQ4_4 operator +(UQ4_4 value)` |  |
| `operator +` | `static UQ4_4 operator +(int left, UQ4_4 right)` |  |
| `operator --` | `static UQ4_4 operator --(UQ4_4 value)` |  |
| `operator -` | `static UQ4_4 operator -(UQ4_4 left, UQ4_4 right)` |  |
| `operator -` | `static UQ4_4 operator -(UQ4_4 left, int right)` |  |
| `operator -` | `static UQ4_4 operator -(int left, UQ4_4 right)` |  |
| `operator /` | `static UQ4_4 operator /(UQ4_4 left, UQ4_4 right)` |  |
| `operator /` | `static UQ4_4 operator /(UQ4_4 left, int right)` |  |
| `operator <=` | `static bool operator <=(UQ4_4 left, UQ4_4 right)` |  |
| `operator <` | `static bool operator <(UQ4_4 left, UQ4_4 right)` |  |
| `operator ==` | `static bool operator ==(UQ4_4 left, UQ4_4 right)` |  |
| `operator >=` | `static bool operator >=(UQ4_4 left, UQ4_4 right)` |  |
| `operator >` | `static bool operator >(UQ4_4 left, UQ4_4 right)` |  |

#### `UQ8_8`

Implements `IComparable`, `IComparable<UQ8_8>`, `IEquatable<UQ8_8>`, `IFormattable`, `IParsable<UQ8_8>`, `ISpanFormattable`, `ISpanParsable<UQ8_8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Epsilon` | `static UQ8_8 Epsilon { get; }` |  |
| `MaxValue` | `static UQ8_8 MaxValue { get; }` |  |
| `MinValue` | `static UQ8_8 MinValue { get; }` |  |
| `One` | `static UQ8_8 One { get; }` |  |
| `RawValue` | `ushort RawValue { get; }` |  |
| `Zero` | `static UQ8_8 Zero { get; }` |  |
| `Clamp` | `static UQ8_8 Clamp(UQ8_8 value, UQ8_8 min, UQ8_8 max)` |  |
| `CompareTo` | `int CompareTo(UQ8_8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UQ8_8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static UQ8_8 FromDouble(double value)` |  |
| `FromRaw` | `static UQ8_8 FromRaw(ushort raw)` |  |
| `FromSingle` | `static UQ8_8 FromSingle(float value)` |  |
| `FromUInt32` | `static UQ8_8 FromUInt32(uint value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static UQ8_8 Max(UQ8_8 left, UQ8_8 right)` |  |
| `Min` | `static UQ8_8 Min(UQ8_8 left, UQ8_8 right)` |  |
| `Parse` | `static UQ8_8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UQ8_8 Parse(string s)` |  |
| `Parse` | `static UQ8_8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UQ8_8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UQ8_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UQ8_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UQ8_8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out UQ8_8 result)` |  |
| `explicit operator UQ8_8` | `static explicit operator UQ8_8(double value)` |  |
| `explicit operator UQ8_8` | `static explicit operator UQ8_8(float value)` |  |
| `explicit operator UQ8_8` | `static explicit operator UQ8_8(ushort raw)` |  |
| `explicit operator byte` | `static explicit operator byte(UQ8_8 value)` |  |
| `explicit operator uint` | `static explicit operator uint(UQ8_8 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(UQ8_8 value)` |  |
| `implicit operator UQ16_16` | `static implicit operator UQ16_16(UQ8_8 value)` |  |
| `implicit operator UQ32_32` | `static implicit operator UQ32_32(UQ8_8 value)` |  |
| `implicit operator UQ8_8` | `static implicit operator UQ8_8(byte value)` |  |
| `implicit operator double` | `static implicit operator double(UQ8_8 value)` |  |
| `implicit operator float` | `static implicit operator float(UQ8_8 value)` |  |
| `operator !=` | `static bool operator !=(UQ8_8 left, UQ8_8 right)` |  |
| `operator %` | `static UQ8_8 operator %(UQ8_8 left, UQ8_8 right)` |  |
| `operator *` | `static UQ8_8 operator *(UQ8_8 left, UQ8_8 right)` |  |
| `operator *` | `static UQ8_8 operator *(UQ8_8 left, int right)` |  |
| `operator *` | `static UQ8_8 operator *(int left, UQ8_8 right)` |  |
| `operator ++` | `static UQ8_8 operator ++(UQ8_8 value)` |  |
| `operator +` | `static UQ8_8 operator +(UQ8_8 left, UQ8_8 right)` |  |
| `operator +` | `static UQ8_8 operator +(UQ8_8 left, int right)` |  |
| `operator +` | `static UQ8_8 operator +(UQ8_8 value)` |  |
| `operator +` | `static UQ8_8 operator +(int left, UQ8_8 right)` |  |
| `operator --` | `static UQ8_8 operator --(UQ8_8 value)` |  |
| `operator -` | `static UQ8_8 operator -(UQ8_8 left, UQ8_8 right)` |  |
| `operator -` | `static UQ8_8 operator -(UQ8_8 left, int right)` |  |
| `operator -` | `static UQ8_8 operator -(int left, UQ8_8 right)` |  |
| `operator /` | `static UQ8_8 operator /(UQ8_8 left, UQ8_8 right)` |  |
| `operator /` | `static UQ8_8 operator /(UQ8_8 left, int right)` |  |
| `operator <=` | `static bool operator <=(UQ8_8 left, UQ8_8 right)` |  |
| `operator <` | `static bool operator <(UQ8_8 left, UQ8_8 right)` |  |
| `operator ==` | `static bool operator ==(UQ8_8 left, UQ8_8 right)` |  |
| `operator >=` | `static bool operator >=(UQ8_8 left, UQ8_8 right)` |  |
| `operator >` | `static bool operator >(UQ8_8 left, UQ8_8 right)` |  |

#### `UnixTime32`

Implements `IComparable`, `IComparable<UnixTime32>`, `IEquatable<UnixTime32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `uint RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(UnixTime32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UnixTime32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static UnixTime32 FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static UnixTime32 FromRaw(uint raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(UnixTime32 value)` |  |
| `operator !=` | `static bool operator !=(UnixTime32 left, UnixTime32 right)` |  |
| `operator <=` | `static bool operator <=(UnixTime32 left, UnixTime32 right)` |  |
| `operator <` | `static bool operator <(UnixTime32 left, UnixTime32 right)` |  |
| `operator ==` | `static bool operator ==(UnixTime32 left, UnixTime32 right)` |  |
| `operator >=` | `static bool operator >=(UnixTime32 left, UnixTime32 right)` |  |
| `operator >` | `static bool operator >(UnixTime32 left, UnixTime32 right)` |  |

#### `UnixTime64`

Implements `IComparable`, `IComparable<UnixTime64>`, `IEquatable<UnixTime64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `long RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(UnixTime64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UnixTime64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static UnixTime64 FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static UnixTime64 FromRaw(long raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(UnixTime64 value)` |  |
| `operator !=` | `static bool operator !=(UnixTime64 left, UnixTime64 right)` |  |
| `operator <=` | `static bool operator <=(UnixTime64 left, UnixTime64 right)` |  |
| `operator <` | `static bool operator <(UnixTime64 left, UnixTime64 right)` |  |
| `operator ==` | `static bool operator ==(UnixTime64 left, UnixTime64 right)` |  |
| `operator >=` | `static bool operator >=(UnixTime64 left, UnixTime64 right)` |  |
| `operator >` | `static bool operator >(UnixTime64 left, UnixTime64 right)` |  |

#### `UnpackedBCD`

Implements `IComparable`, `IComparable<UnpackedBCD>`, `IEquatable<UnpackedBCD>`, `IFormattable`, `IParsable<UnpackedBCD>`, `ISpanFormattable`, `ISpanParsable<UnpackedBCD>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValue` | `static UnpackedBCD MaxValue { get; }` |  |
| `MinValue` | `static UnpackedBCD MinValue { get; }` |  |
| `One` | `static UnpackedBCD One { get; }` |  |
| `RawValue` | `byte RawValue { get; }` |  |
| `Value` | `int Value { get; }` |  |
| `Zero` | `static UnpackedBCD Zero { get; }` |  |
| `Clamp` | `static UnpackedBCD Clamp(UnpackedBCD value, UnpackedBCD min, UnpackedBCD max)` |  |
| `CompareTo` | `int CompareTo(UnpackedBCD other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(UnpackedBCD other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromValue` | `static UnpackedBCD FromValue(int value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Max` | `static UnpackedBCD Max(UnpackedBCD left, UnpackedBCD right)` |  |
| `Min` | `static UnpackedBCD Min(UnpackedBCD left, UnpackedBCD right)` |  |
| `Parse` | `static UnpackedBCD Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UnpackedBCD Parse(string s)` |  |
| `Parse` | `static UnpackedBCD Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static UnpackedBCD Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UnpackedBCD result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UnpackedBCD result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UnpackedBCD result)` |  |
| `TryParse` | `static bool TryParse(string s, out UnpackedBCD result)` |  |
| `explicit operator byte` | `static explicit operator byte(UnpackedBCD value)` |  |
| `explicit operator int` | `static explicit operator int(UnpackedBCD value)` |  |
| `implicit operator PackedBCD16` | `static implicit operator PackedBCD16(UnpackedBCD value)` |  |
| `implicit operator PackedBCD32` | `static implicit operator PackedBCD32(UnpackedBCD value)` |  |
| `implicit operator PackedBCD8` | `static implicit operator PackedBCD8(UnpackedBCD value)` |  |
| `implicit operator UnpackedBCD` | `static implicit operator UnpackedBCD(byte value)` |  |
| `operator !=` | `static bool operator !=(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator %` | `static UnpackedBCD operator %(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator *` | `static UnpackedBCD operator *(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator ++` | `static UnpackedBCD operator ++(UnpackedBCD value)` |  |
| `operator +` | `static UnpackedBCD operator +(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator --` | `static UnpackedBCD operator --(UnpackedBCD value)` |  |
| `operator -` | `static UnpackedBCD operator -(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator /` | `static UnpackedBCD operator /(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator <=` | `static bool operator <=(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator <` | `static bool operator <(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator ==` | `static bool operator ==(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator >=` | `static bool operator >=(UnpackedBCD left, UnpackedBCD right)` |  |
| `operator >` | `static bool operator >(UnpackedBCD left, UnpackedBCD right)` |  |

#### `UnsignedBitCodec`

Implements `IBitCodec<ulong>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `UnsignedBitCodec` | `UnsignedBitCodec(int bitWidth)` |  |
| `BitWidth` | `int BitWidth { get; }` |  |
| `Decode` | `ulong Decode(ulong code)` |  |
| `Encode` | `ulong Encode(ulong value)` |  |

#### `UnsignedDecimal`

Implements `IComparable`, `IComparable<UnsignedDecimal>`, `IComparable<decimal>`, `IConvertible`, `IEquatable<UnsignedDecimal>`, `IFormattable`, `IParsable<UnsignedDecimal>`, `ISpanFormattable`, `ISpanParsable<UnsignedDecimal>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `UnsignedDecimal` | `UnsignedDecimal(decimal value)` |  |
| `Epsilon` | `static readonly UnsignedDecimal Epsilon` |  |
| `MaxValue` | `static readonly UnsignedDecimal MaxValue` |  |
| `One` | `static readonly UnsignedDecimal One` |  |
| `Zero` | `static readonly UnsignedDecimal Zero` |  |
| `value` | `decimal value { get; init; }` |  |
| `Abs` | `UnsignedDecimal Abs()` |  |
| `Cbrt` | `UnsignedDecimal Cbrt()` |  |
| `Ceiling` | `UnsignedDecimal Ceiling()` |  |
| `CompareTo` | `int CompareTo(UnsignedDecimal other)` |  |
| `CompareTo` | `int CompareTo(decimal other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Cubed` | `UnsignedDecimal Cubed()` |  |
| `Floor` | `UnsignedDecimal Floor()` |  |
| `GetTypeCode` | `TypeCode GetTypeCode()` |  |
| `IsAboveOrEqual` | `bool IsAboveOrEqual(UnsignedDecimal inclusiveLimit)` |  |
| `IsAbove` | `bool IsAbove(UnsignedDecimal exclusiveLimit)` |  |
| `IsBelowOrEqual` | `bool IsBelowOrEqual(UnsignedDecimal inclusiveLimit)` |  |
| `IsBelow` | `bool IsBelow(UnsignedDecimal exclusiveLimit)` |  |
| `IsBetween` | `bool IsBetween(UnsignedDecimal exclusiveLowerLimit, UnsignedDecimal exclusiveUpperLimit)` |  |
| `IsEven` | `bool IsEven()` |  |
| `IsInRange` | `bool IsInRange(UnsignedDecimal inclusiveLowerLimit, UnsignedDecimal inclusiveUpperLimit)` |  |
| `IsIn` | `bool IsIn(params UnsignedDecimal[] values)` |  |
| `IsNegativeOrZero` | `bool IsNegativeOrZero()` |  |
| `IsNegative` | `bool IsNegative()` |  |
| `IsNotIn` | `bool IsNotIn(params UnsignedDecimal[] values)` |  |
| `IsNotZero` | `bool IsNotZero()` |  |
| `IsOdd` | `bool IsOdd()` |  |
| `IsPositiveOrZero` | `bool IsPositiveOrZero()` |  |
| `IsPositive` | `bool IsPositive()` |  |
| `IsZero` | `bool IsZero()` |  |
| `Max` | `static UnsignedDecimal Max(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `Min` | `static UnsignedDecimal Min(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `Parse` | `static UnsignedDecimal Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UnsignedDecimal Parse(string s)` |  |
| `Parse` | `static UnsignedDecimal Parse(string s, IFormatProvider provider)` |  |
| `Pow` | `decimal Pow(decimal exponent)` |  |
| `ReciprocalEstimate` | `UnsignedDecimal ReciprocalEstimate()` |  |
| `Round` | `UnsignedDecimal Round()` |  |
| `Round` | `UnsignedDecimal Round(MidpointRounding method)` |  |
| `Round` | `UnsignedDecimal Round(int digits)` |  |
| `Round` | `UnsignedDecimal Round(int digits, MidpointRounding method)` |  |
| `Sqrt` | `UnsignedDecimal Sqrt()` |  |
| `Squared` | `UnsignedDecimal Squared()` |  |
| `ToBoolean` | `bool ToBoolean(IFormatProvider provider)` |  |
| `ToByte` | `byte ToByte(IFormatProvider provider)` |  |
| `ToChar` | `char ToChar(IFormatProvider provider)` |  |
| `ToDateTime` | `DateTime ToDateTime(IFormatProvider provider)` |  |
| `ToDecimal` | `decimal ToDecimal(IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble(IFormatProvider provider)` |  |
| `ToInt16` | `short ToInt16(IFormatProvider provider)` |  |
| `ToInt32` | `int ToInt32(IFormatProvider provider)` |  |
| `ToInt64` | `long ToInt64(IFormatProvider provider)` |  |
| `ToSByte` | `sbyte ToSByte(IFormatProvider provider)` |  |
| `ToSingle` | `float ToSingle(IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `ToType` | `object ToType(Type conversionType, IFormatProvider provider)` |  |
| `ToUInt16` | `ushort ToUInt16(IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32(IFormatProvider provider)` |  |
| `ToUInt64` | `ulong ToUInt64(IFormatProvider provider)` |  |
| `Truncate` | `UnsignedDecimal Truncate()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UnsignedDecimal result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UnsignedDecimal result)` |  |
| `TryParse` | `static bool TryParse(string s, out UnsignedDecimal result)` |  |
| `explicit operator UnsignedDecimal` | `static explicit operator UnsignedDecimal(decimal value)` |  |
| `implicit operator UnsignedDecimal` | `static implicit operator UnsignedDecimal(byte value)` |  |
| `implicit operator UnsignedDecimal` | `static implicit operator UnsignedDecimal(char value)` |  |
| `implicit operator UnsignedDecimal` | `static implicit operator UnsignedDecimal(uint value)` |  |
| `implicit operator UnsignedDecimal` | `static implicit operator UnsignedDecimal(ulong value)` |  |
| `implicit operator UnsignedDecimal` | `static implicit operator UnsignedDecimal(ushort value)` |  |
| `implicit operator decimal` | `static implicit operator decimal(UnsignedDecimal value)` |  |
| `operator %` | `static UnsignedDecimal operator %(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator %` | `static decimal operator %(UnsignedDecimal left, decimal right)` |  |
| `operator *` | `static UnsignedDecimal operator *(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator *` | `static decimal operator *(UnsignedDecimal left, decimal right)` |  |
| `operator *` | `static decimal operator *(decimal left, UnsignedDecimal right)` |  |
| `operator +` | `static UnsignedDecimal operator +(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator +` | `static decimal operator +(UnsignedDecimal left, decimal right)` |  |
| `operator +` | `static decimal operator +(decimal left, UnsignedDecimal right)` |  |
| `operator -` | `static decimal operator -(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator -` | `static decimal operator -(UnsignedDecimal left, decimal right)` |  |
| `operator -` | `static decimal operator -(decimal left, UnsignedDecimal right)` |  |
| `operator /` | `static UnsignedDecimal operator /(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator /` | `static decimal operator /(UnsignedDecimal left, decimal right)` |  |
| `operator /` | `static decimal operator /(decimal left, UnsignedDecimal right)` |  |
| `operator <=` | `static bool operator <=(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator <=` | `static bool operator <=(UnsignedDecimal left, decimal right)` |  |
| `operator <=` | `static bool operator <=(decimal left, UnsignedDecimal right)` |  |
| `operator <` | `static bool operator <(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator <` | `static bool operator <(UnsignedDecimal left, decimal right)` |  |
| `operator <` | `static bool operator <(decimal left, UnsignedDecimal right)` |  |
| `operator >=` | `static bool operator >=(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator >=` | `static bool operator >=(UnsignedDecimal left, decimal right)` |  |
| `operator >=` | `static bool operator >=(decimal left, UnsignedDecimal right)` |  |
| `operator >` | `static bool operator >(UnsignedDecimal left, UnsignedDecimal right)` |  |
| `operator >` | `static bool operator >(UnsignedDecimal left, decimal right)` |  |
| `operator >` | `static bool operator >(decimal left, UnsignedDecimal right)` |  |

#### `UnsignedDouble`

Implements `IComparable`, `IComparable<UnsignedDouble>`, `IComparable<double>`, `IConvertible`, `IEquatable<UnsignedDouble>`, `IFormattable`, `IParsable<UnsignedDouble>`, `ISpanFormattable`, `ISpanParsable<UnsignedDouble>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `UnsignedDouble` | `UnsignedDouble(double value)` |  |
| `Epsilon` | `static readonly UnsignedDouble Epsilon` |  |
| `MaxValue` | `static readonly UnsignedDouble MaxValue` |  |
| `One` | `static readonly UnsignedDouble One` |  |
| `Zero` | `static readonly UnsignedDouble Zero` |  |
| `value` | `double value { get; init; }` |  |
| `Abs` | `UnsignedDouble Abs()` |  |
| `Cbrt` | `UnsignedDouble Cbrt()` |  |
| `Ceiling` | `UnsignedDouble Ceiling()` |  |
| `CompareTo` | `int CompareTo(UnsignedDouble other)` |  |
| `CompareTo` | `int CompareTo(double other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Cubed` | `UnsignedDouble Cubed()` |  |
| `Floor` | `UnsignedDouble Floor()` |  |
| `GetTypeCode` | `TypeCode GetTypeCode()` |  |
| `IsAboveOrEqual` | `bool IsAboveOrEqual(UnsignedDouble inclusiveLimit)` |  |
| `IsAbove` | `bool IsAbove(UnsignedDouble exclusiveLimit)` |  |
| `IsBelowOrEqual` | `bool IsBelowOrEqual(UnsignedDouble inclusiveLimit)` |  |
| `IsBelow` | `bool IsBelow(UnsignedDouble exclusiveLimit)` |  |
| `IsBetween` | `bool IsBetween(UnsignedDouble exclusiveLowerLimit, UnsignedDouble exclusiveUpperLimit)` |  |
| `IsEven` | `bool IsEven()` |  |
| `IsInRange` | `bool IsInRange(UnsignedDouble inclusiveLowerLimit, UnsignedDouble inclusiveUpperLimit)` |  |
| `IsIn` | `bool IsIn(params UnsignedDouble[] values)` |  |
| `IsInfinity` | `bool IsInfinity()` |  |
| `IsNaN` | `bool IsNaN()` |  |
| `IsNegativeOrZero` | `bool IsNegativeOrZero()` |  |
| `IsNegative` | `bool IsNegative()` |  |
| `IsNonNumeric` | `bool IsNonNumeric()` |  |
| `IsNotIn` | `bool IsNotIn(params UnsignedDouble[] values)` |  |
| `IsNotZero` | `bool IsNotZero()` |  |
| `IsNumeric` | `bool IsNumeric()` |  |
| `IsOdd` | `bool IsOdd()` |  |
| `IsPositiveOrZero` | `bool IsPositiveOrZero()` |  |
| `IsPositive` | `bool IsPositive()` |  |
| `IsZero` | `bool IsZero()` |  |
| `Max` | `static UnsignedDouble Max(UnsignedDouble left, UnsignedDouble right)` |  |
| `Min` | `static UnsignedDouble Min(UnsignedDouble left, UnsignedDouble right)` |  |
| `Parse` | `static UnsignedDouble Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UnsignedDouble Parse(string s)` |  |
| `Parse` | `static UnsignedDouble Parse(string s, IFormatProvider provider)` |  |
| `Pow` | `double Pow(double exponent)` |  |
| `ReciprocalEstimate` | `UnsignedDouble ReciprocalEstimate()` |  |
| `Round` | `UnsignedDouble Round()` |  |
| `Round` | `UnsignedDouble Round(MidpointRounding method)` |  |
| `Round` | `UnsignedDouble Round(int digits)` |  |
| `Round` | `UnsignedDouble Round(int digits, MidpointRounding method)` |  |
| `Sqrt` | `UnsignedDouble Sqrt()` |  |
| `Squared` | `UnsignedDouble Squared()` |  |
| `ToBoolean` | `bool ToBoolean(IFormatProvider provider)` |  |
| `ToByte` | `byte ToByte(IFormatProvider provider)` |  |
| `ToChar` | `char ToChar(IFormatProvider provider)` |  |
| `ToDateTime` | `DateTime ToDateTime(IFormatProvider provider)` |  |
| `ToDecimal` | `decimal ToDecimal(IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble(IFormatProvider provider)` |  |
| `ToInt16` | `short ToInt16(IFormatProvider provider)` |  |
| `ToInt32` | `int ToInt32(IFormatProvider provider)` |  |
| `ToInt64` | `long ToInt64(IFormatProvider provider)` |  |
| `ToSByte` | `sbyte ToSByte(IFormatProvider provider)` |  |
| `ToSingle` | `float ToSingle(IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `ToType` | `object ToType(Type conversionType, IFormatProvider provider)` |  |
| `ToUInt16` | `ushort ToUInt16(IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32(IFormatProvider provider)` |  |
| `ToUInt64` | `ulong ToUInt64(IFormatProvider provider)` |  |
| `Truncate` | `UnsignedDouble Truncate()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UnsignedDouble result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UnsignedDouble result)` |  |
| `TryParse` | `static bool TryParse(string s, out UnsignedDouble result)` |  |
| `explicit operator UnsignedDouble` | `static explicit operator UnsignedDouble(double value)` |  |
| `implicit operator UnsignedDouble` | `static implicit operator UnsignedDouble(byte value)` |  |
| `implicit operator UnsignedDouble` | `static implicit operator UnsignedDouble(char value)` |  |
| `implicit operator UnsignedDouble` | `static implicit operator UnsignedDouble(uint value)` |  |
| `implicit operator UnsignedDouble` | `static implicit operator UnsignedDouble(ulong value)` |  |
| `implicit operator UnsignedDouble` | `static implicit operator UnsignedDouble(ushort value)` |  |
| `implicit operator double` | `static implicit operator double(UnsignedDouble value)` |  |
| `operator %` | `static UnsignedDouble operator %(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator %` | `static double operator %(UnsignedDouble left, double right)` |  |
| `operator *` | `static UnsignedDouble operator *(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator *` | `static double operator *(UnsignedDouble left, double right)` |  |
| `operator *` | `static double operator *(double left, UnsignedDouble right)` |  |
| `operator +` | `static UnsignedDouble operator +(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator +` | `static double operator +(UnsignedDouble left, double right)` |  |
| `operator +` | `static double operator +(double left, UnsignedDouble right)` |  |
| `operator -` | `static double operator -(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator -` | `static double operator -(UnsignedDouble left, double right)` |  |
| `operator -` | `static double operator -(double left, UnsignedDouble right)` |  |
| `operator /` | `static UnsignedDouble operator /(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator /` | `static double operator /(UnsignedDouble left, double right)` |  |
| `operator /` | `static double operator /(double left, UnsignedDouble right)` |  |
| `operator <=` | `static bool operator <=(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator <=` | `static bool operator <=(UnsignedDouble left, double right)` |  |
| `operator <=` | `static bool operator <=(double left, UnsignedDouble right)` |  |
| `operator <` | `static bool operator <(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator <` | `static bool operator <(UnsignedDouble left, double right)` |  |
| `operator <` | `static bool operator <(double left, UnsignedDouble right)` |  |
| `operator >=` | `static bool operator >=(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator >=` | `static bool operator >=(UnsignedDouble left, double right)` |  |
| `operator >=` | `static bool operator >=(double left, UnsignedDouble right)` |  |
| `operator >` | `static bool operator >(UnsignedDouble left, UnsignedDouble right)` |  |
| `operator >` | `static bool operator >(UnsignedDouble left, double right)` |  |
| `operator >` | `static bool operator >(double left, UnsignedDouble right)` |  |

#### `UnsignedFloat`

Implements `IComparable`, `IComparable<UnsignedFloat>`, `IComparable<float>`, `IConvertible`, `IEquatable<UnsignedFloat>`, `IFormattable`, `IParsable<UnsignedFloat>`, `ISpanFormattable`, `ISpanParsable<UnsignedFloat>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `UnsignedFloat` | `UnsignedFloat(float value)` |  |
| `Epsilon` | `static readonly UnsignedFloat Epsilon` |  |
| `MaxValue` | `static readonly UnsignedFloat MaxValue` |  |
| `One` | `static readonly UnsignedFloat One` |  |
| `Zero` | `static readonly UnsignedFloat Zero` |  |
| `value` | `float value { get; init; }` |  |
| `Abs` | `UnsignedFloat Abs()` |  |
| `Cbrt` | `UnsignedFloat Cbrt()` |  |
| `Ceiling` | `UnsignedFloat Ceiling()` |  |
| `CompareTo` | `int CompareTo(UnsignedFloat other)` |  |
| `CompareTo` | `int CompareTo(float other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Cubed` | `UnsignedFloat Cubed()` |  |
| `Floor` | `UnsignedFloat Floor()` |  |
| `GetTypeCode` | `TypeCode GetTypeCode()` |  |
| `IsAboveOrEqual` | `bool IsAboveOrEqual(UnsignedFloat inclusiveLimit)` |  |
| `IsAbove` | `bool IsAbove(UnsignedFloat exclusiveLimit)` |  |
| `IsBelowOrEqual` | `bool IsBelowOrEqual(UnsignedFloat inclusiveLimit)` |  |
| `IsBelow` | `bool IsBelow(UnsignedFloat exclusiveLimit)` |  |
| `IsBetween` | `bool IsBetween(UnsignedFloat exclusiveLowerLimit, UnsignedFloat exclusiveUpperLimit)` |  |
| `IsEven` | `bool IsEven()` |  |
| `IsInRange` | `bool IsInRange(UnsignedFloat inclusiveLowerLimit, UnsignedFloat inclusiveUpperLimit)` |  |
| `IsIn` | `bool IsIn(params UnsignedFloat[] values)` |  |
| `IsInfinity` | `bool IsInfinity()` |  |
| `IsNaN` | `bool IsNaN()` |  |
| `IsNegativeOrZero` | `bool IsNegativeOrZero()` |  |
| `IsNegative` | `bool IsNegative()` |  |
| `IsNonNumeric` | `bool IsNonNumeric()` |  |
| `IsNotIn` | `bool IsNotIn(params UnsignedFloat[] values)` |  |
| `IsNotZero` | `bool IsNotZero()` |  |
| `IsNumeric` | `bool IsNumeric()` |  |
| `IsOdd` | `bool IsOdd()` |  |
| `IsPositiveOrZero` | `bool IsPositiveOrZero()` |  |
| `IsPositive` | `bool IsPositive()` |  |
| `IsZero` | `bool IsZero()` |  |
| `Max` | `static UnsignedFloat Max(UnsignedFloat left, UnsignedFloat right)` |  |
| `Min` | `static UnsignedFloat Min(UnsignedFloat left, UnsignedFloat right)` |  |
| `Parse` | `static UnsignedFloat Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static UnsignedFloat Parse(string s)` |  |
| `Parse` | `static UnsignedFloat Parse(string s, IFormatProvider provider)` |  |
| `Pow` | `float Pow(float exponent)` |  |
| `ReciprocalEstimate` | `UnsignedFloat ReciprocalEstimate()` |  |
| `Round` | `UnsignedFloat Round()` |  |
| `Round` | `UnsignedFloat Round(MidpointRounding method)` |  |
| `Round` | `UnsignedFloat Round(int digits)` |  |
| `Round` | `UnsignedFloat Round(int digits, MidpointRounding method)` |  |
| `Sqrt` | `UnsignedFloat Sqrt()` |  |
| `Squared` | `UnsignedFloat Squared()` |  |
| `ToBoolean` | `bool ToBoolean(IFormatProvider provider)` |  |
| `ToByte` | `byte ToByte(IFormatProvider provider)` |  |
| `ToChar` | `char ToChar(IFormatProvider provider)` |  |
| `ToDateTime` | `DateTime ToDateTime(IFormatProvider provider)` |  |
| `ToDecimal` | `decimal ToDecimal(IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble(IFormatProvider provider)` |  |
| `ToInt16` | `short ToInt16(IFormatProvider provider)` |  |
| `ToInt32` | `int ToInt32(IFormatProvider provider)` |  |
| `ToInt64` | `long ToInt64(IFormatProvider provider)` |  |
| `ToSByte` | `sbyte ToSByte(IFormatProvider provider)` |  |
| `ToSingle` | `float ToSingle(IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider formatProvider)` |  |
| `ToType` | `object ToType(Type conversionType, IFormatProvider provider)` |  |
| `ToUInt16` | `ushort ToUInt16(IFormatProvider provider)` |  |
| `ToUInt32` | `uint ToUInt32(IFormatProvider provider)` |  |
| `ToUInt64` | `ulong ToUInt64(IFormatProvider provider)` |  |
| `Truncate` | `UnsignedFloat Truncate()` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UnsignedFloat result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out UnsignedFloat result)` |  |
| `TryParse` | `static bool TryParse(string s, out UnsignedFloat result)` |  |
| `explicit operator UnsignedFloat` | `static explicit operator UnsignedFloat(float value)` |  |
| `implicit operator UnsignedFloat` | `static implicit operator UnsignedFloat(byte value)` |  |
| `implicit operator UnsignedFloat` | `static implicit operator UnsignedFloat(char value)` |  |
| `implicit operator UnsignedFloat` | `static implicit operator UnsignedFloat(uint value)` |  |
| `implicit operator UnsignedFloat` | `static implicit operator UnsignedFloat(ulong value)` |  |
| `implicit operator UnsignedFloat` | `static implicit operator UnsignedFloat(ushort value)` |  |
| `implicit operator float` | `static implicit operator float(UnsignedFloat value)` |  |
| `operator %` | `static UnsignedFloat operator %(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator %` | `static float operator %(UnsignedFloat left, float right)` |  |
| `operator *` | `static UnsignedFloat operator *(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator *` | `static float operator *(UnsignedFloat left, float right)` |  |
| `operator *` | `static float operator *(float left, UnsignedFloat right)` |  |
| `operator +` | `static UnsignedFloat operator +(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator +` | `static float operator +(UnsignedFloat left, float right)` |  |
| `operator +` | `static float operator +(float left, UnsignedFloat right)` |  |
| `operator -` | `static float operator -(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator -` | `static float operator -(UnsignedFloat left, float right)` |  |
| `operator -` | `static float operator -(float left, UnsignedFloat right)` |  |
| `operator /` | `static UnsignedFloat operator /(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator /` | `static float operator /(UnsignedFloat left, float right)` |  |
| `operator /` | `static float operator /(float left, UnsignedFloat right)` |  |
| `operator <=` | `static bool operator <=(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator <=` | `static bool operator <=(UnsignedFloat left, float right)` |  |
| `operator <=` | `static bool operator <=(float left, UnsignedFloat right)` |  |
| `operator <` | `static bool operator <(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator <` | `static bool operator <(UnsignedFloat left, float right)` |  |
| `operator <` | `static bool operator <(float left, UnsignedFloat right)` |  |
| `operator >=` | `static bool operator >=(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator >=` | `static bool operator >=(UnsignedFloat left, float right)` |  |
| `operator >=` | `static bool operator >=(float left, UnsignedFloat right)` |  |
| `operator >` | `static bool operator >(UnsignedFloat left, UnsignedFloat right)` |  |
| `operator >` | `static bool operator >(UnsignedFloat left, float right)` |  |
| `operator >` | `static bool operator >(float left, UnsignedFloat right)` |  |

#### `UriExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `BaseUri` | `static Uri BaseUri(this Uri @this)` |  |
| `DownloadToFile` | `static void DownloadToFile(this Uri @this, FileInfo file, bool overwrite = false, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null)` |  |
| `GetResponseUri` | `static Uri GetResponseUri(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null)` |  |
| `Path` | `static Uri Path(this Uri @this, string path)` |  |
| `ReadAllBytesTaskAsync` | `static Task<byte[]> ReadAllBytesTaskAsync(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)` |  |
| `ReadAllBytes` | `static byte[] ReadAllBytes(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null)` |  |
| `ReadAllTextTaskAsync` | `static Task<string> ReadAllTextTaskAsync(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null)` |  |
| `ReadAllText` | `static string ReadAllText(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null)` |  |

#### `VaxFloat`

Implements `IComparable`, `IComparable<VaxFloat>`, `IEquatable<VaxFloat>`, `IFormattable`, `IParsable<VaxFloat>`, `ISpanFormattable`, `ISpanParsable<VaxFloat>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `One` | `static VaxFloat One { get; }` |  |
| `RawValue` | `uint RawValue { get; }` |  |
| `Zero` | `static VaxFloat Zero { get; }` |  |
| `CompareTo` | `int CompareTo(VaxFloat other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(VaxFloat other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDouble` | `static VaxFloat FromDouble(double value)` |  |
| `FromRaw` | `static VaxFloat FromRaw(uint raw)` |  |
| `FromSingle` | `static VaxFloat FromSingle(float value)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `IsNegative` | `static bool IsNegative(VaxFloat value)` |  |
| `Parse` | `static VaxFloat Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static VaxFloat Parse(string s)` |  |
| `Parse` | `static VaxFloat Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static VaxFloat Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToDouble` | `double ToDouble()` |  |
| `ToSingle` | `float ToSingle()` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out VaxFloat result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out VaxFloat result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out VaxFloat result)` |  |
| `TryParse` | `static bool TryParse(string s, out VaxFloat result)` |  |
| `explicit operator VaxFloat` | `static explicit operator VaxFloat(double value)` |  |
| `explicit operator VaxFloat` | `static explicit operator VaxFloat(float value)` |  |
| `implicit operator double` | `static implicit operator double(VaxFloat value)` |  |
| `implicit operator float` | `static implicit operator float(VaxFloat value)` |  |
| `operator !=` | `static bool operator !=(VaxFloat left, VaxFloat right)` |  |
| `operator *` | `static VaxFloat operator *(VaxFloat left, VaxFloat right)` |  |
| `operator +` | `static VaxFloat operator +(VaxFloat left, VaxFloat right)` |  |
| `operator -` | `static VaxFloat operator -(VaxFloat left, VaxFloat right)` |  |
| `operator /` | `static VaxFloat operator /(VaxFloat left, VaxFloat right)` |  |
| `operator <=` | `static bool operator <=(VaxFloat left, VaxFloat right)` |  |
| `operator <` | `static bool operator <(VaxFloat left, VaxFloat right)` |  |
| `operator ==` | `static bool operator ==(VaxFloat left, VaxFloat right)` |  |
| `operator >=` | `static bool operator >=(VaxFloat left, VaxFloat right)` |  |
| `operator >` | `static bool operator >(VaxFloat left, VaxFloat right)` |  |

#### `WebKitTime`

Implements `IComparable`, `IComparable<WebKitTime>`, `IEquatable<WebKitTime>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RawValue` | `ulong RawValue { get; }` |  |
| `CompareTo` | `int CompareTo(WebKitTime other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(WebKitTime other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDateTime` | `static WebKitTime FromDateTime(DateTime dt)` |  |
| `FromRaw` | `static WebKitTime FromRaw(ulong raw)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToDateTime` | `DateTime ToDateTime()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator DateTime` | `static implicit operator DateTime(WebKitTime value)` |  |
| `operator !=` | `static bool operator !=(WebKitTime left, WebKitTime right)` |  |
| `operator <=` | `static bool operator <=(WebKitTime left, WebKitTime right)` |  |
| `operator <` | `static bool operator <(WebKitTime left, WebKitTime right)` |  |
| `operator ==` | `static bool operator ==(WebKitTime left, WebKitTime right)` |  |
| `operator >=` | `static bool operator >=(WebKitTime left, WebKitTime right)` |  |
| `operator >` | `static bool operator >(WebKitTime left, WebKitTime right)` |  |

#### `WriteOnlyIndexedProperty<TIndexer, TResult>`

| Member | Signature | Summary |
| --- | --- | --- |
| `WriteOnlyIndexedProperty` | `WriteOnlyIndexedProperty(Action<TIndexer, TResult> setter)` |  |
| `Item` | `TResult this[TIndexer index] { set; }` |  |

#### `ZigZag16`

Implements `IComparable`, `IComparable<ZigZag16>`, `IEquatable<ZigZag16>`, `IFormattable`, `IParsable<ZigZag16>`, `ISpanFormattable`, `ISpanParsable<ZigZag16>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `DecodedValue` | `short DecodedValue { get; }` |  |
| `EncodedValue` | `ushort EncodedValue { get; }` |  |
| `MaxValue` | `static ZigZag16 MaxValue { get; }` |  |
| `MinValue` | `static ZigZag16 MinValue { get; }` |  |
| `Zero` | `static ZigZag16 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(ZigZag16 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(ZigZag16 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDecoded` | `static ZigZag16 FromDecoded(short value)` |  |
| `FromEncoded` | `static ZigZag16 FromEncoded(ushort encoded)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static ZigZag16 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag16 Parse(string s)` |  |
| `Parse` | `static ZigZag16 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag16 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ZigZag16 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ZigZag16 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ZigZag16 result)` |  |
| `TryParse` | `static bool TryParse(string s, out ZigZag16 result)` |  |
| `explicit operator ZigZag16` | `static explicit operator ZigZag16(ushort encoded)` |  |
| `explicit operator short` | `static explicit operator short(ZigZag16 value)` |  |
| `explicit operator ushort` | `static explicit operator ushort(ZigZag16 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(ZigZag16 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ZigZag16 value)` |  |
| `implicit operator ZigZag16` | `static implicit operator ZigZag16(short value)` |  |
| `implicit operator ZigZag32` | `static implicit operator ZigZag32(ZigZag16 value)` |  |
| `implicit operator ZigZag64` | `static implicit operator ZigZag64(ZigZag16 value)` |  |
| `implicit operator int` | `static implicit operator int(ZigZag16 value)` |  |
| `implicit operator long` | `static implicit operator long(ZigZag16 value)` |  |
| `operator !=` | `static bool operator !=(ZigZag16 left, ZigZag16 right)` |  |
| `operator <=` | `static bool operator <=(ZigZag16 left, ZigZag16 right)` |  |
| `operator <` | `static bool operator <(ZigZag16 left, ZigZag16 right)` |  |
| `operator ==` | `static bool operator ==(ZigZag16 left, ZigZag16 right)` |  |
| `operator >=` | `static bool operator >=(ZigZag16 left, ZigZag16 right)` |  |
| `operator >` | `static bool operator >(ZigZag16 left, ZigZag16 right)` |  |

#### `ZigZag32`

Implements `IComparable`, `IComparable<ZigZag32>`, `IEquatable<ZigZag32>`, `IFormattable`, `IParsable<ZigZag32>`, `ISpanFormattable`, `ISpanParsable<ZigZag32>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `DecodedValue` | `int DecodedValue { get; }` |  |
| `EncodedValue` | `uint EncodedValue { get; }` |  |
| `MaxValue` | `static ZigZag32 MaxValue { get; }` |  |
| `MinValue` | `static ZigZag32 MinValue { get; }` |  |
| `Zero` | `static ZigZag32 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(ZigZag32 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(ZigZag32 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDecoded` | `static ZigZag32 FromDecoded(int value)` |  |
| `FromEncoded` | `static ZigZag32 FromEncoded(uint encoded)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static ZigZag32 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag32 Parse(string s)` |  |
| `Parse` | `static ZigZag32 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag32 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ZigZag32 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ZigZag32 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ZigZag32 result)` |  |
| `TryParse` | `static bool TryParse(string s, out ZigZag32 result)` |  |
| `explicit operator ZigZag32` | `static explicit operator ZigZag32(uint encoded)` |  |
| `explicit operator int` | `static explicit operator int(ZigZag32 value)` |  |
| `explicit operator uint` | `static explicit operator uint(ZigZag32 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(ZigZag32 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ZigZag32 value)` |  |
| `implicit operator ZigZag32` | `static implicit operator ZigZag32(int value)` |  |
| `implicit operator ZigZag64` | `static implicit operator ZigZag64(ZigZag32 value)` |  |
| `implicit operator long` | `static implicit operator long(ZigZag32 value)` |  |
| `operator !=` | `static bool operator !=(ZigZag32 left, ZigZag32 right)` |  |
| `operator <=` | `static bool operator <=(ZigZag32 left, ZigZag32 right)` |  |
| `operator <` | `static bool operator <(ZigZag32 left, ZigZag32 right)` |  |
| `operator ==` | `static bool operator ==(ZigZag32 left, ZigZag32 right)` |  |
| `operator >=` | `static bool operator >=(ZigZag32 left, ZigZag32 right)` |  |
| `operator >` | `static bool operator >(ZigZag32 left, ZigZag32 right)` |  |

#### `ZigZag64`

Implements `IComparable`, `IComparable<ZigZag64>`, `IEquatable<ZigZag64>`, `IFormattable`, `IParsable<ZigZag64>`, `ISpanFormattable`, `ISpanParsable<ZigZag64>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `DecodedValue` | `long DecodedValue { get; }` |  |
| `EncodedValue` | `ulong EncodedValue { get; }` |  |
| `MaxValue` | `static ZigZag64 MaxValue { get; }` |  |
| `MinValue` | `static ZigZag64 MinValue { get; }` |  |
| `Zero` | `static ZigZag64 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(ZigZag64 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(ZigZag64 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDecoded` | `static ZigZag64 FromDecoded(long value)` |  |
| `FromEncoded` | `static ZigZag64 FromEncoded(ulong encoded)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static ZigZag64 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag64 Parse(string s)` |  |
| `Parse` | `static ZigZag64 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag64 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ZigZag64 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ZigZag64 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ZigZag64 result)` |  |
| `TryParse` | `static bool TryParse(string s, out ZigZag64 result)` |  |
| `explicit operator ZigZag64` | `static explicit operator ZigZag64(ulong encoded)` |  |
| `explicit operator long` | `static explicit operator long(ZigZag64 value)` |  |
| `explicit operator ulong` | `static explicit operator ulong(ZigZag64 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(ZigZag64 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ZigZag64 value)` |  |
| `implicit operator ZigZag64` | `static implicit operator ZigZag64(long value)` |  |
| `operator !=` | `static bool operator !=(ZigZag64 left, ZigZag64 right)` |  |
| `operator <=` | `static bool operator <=(ZigZag64 left, ZigZag64 right)` |  |
| `operator <` | `static bool operator <(ZigZag64 left, ZigZag64 right)` |  |
| `operator ==` | `static bool operator ==(ZigZag64 left, ZigZag64 right)` |  |
| `operator >=` | `static bool operator >=(ZigZag64 left, ZigZag64 right)` |  |
| `operator >` | `static bool operator >(ZigZag64 left, ZigZag64 right)` |  |

#### `ZigZag8`

Implements `IComparable`, `IComparable<ZigZag8>`, `IEquatable<ZigZag8>`, `IFormattable`, `IParsable<ZigZag8>`, `ISpanFormattable`, `ISpanParsable<ZigZag8>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `DecodedValue` | `sbyte DecodedValue { get; }` |  |
| `EncodedValue` | `byte EncodedValue { get; }` |  |
| `MaxValue` | `static ZigZag8 MaxValue { get; }` |  |
| `MinValue` | `static ZigZag8 MinValue { get; }` |  |
| `Zero` | `static ZigZag8 Zero { get; }` |  |
| `CompareTo` | `int CompareTo(ZigZag8 other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(ZigZag8 other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `FromDecoded` | `static ZigZag8 FromDecoded(sbyte value)` |  |
| `FromEncoded` | `static ZigZag8 FromEncoded(byte encoded)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `Parse` | `static ZigZag8 Parse(ReadOnlySpan<char> s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag8 Parse(string s)` |  |
| `Parse` | `static ZigZag8 Parse(string s, IFormatProvider provider)` |  |
| `Parse` | `static ZigZag8 Parse(string s, NumberStyles style, IFormatProvider provider)` |  |
| `ToString` | `override string ToString()` |  |
| `ToString` | `string ToString(IFormatProvider provider)` |  |
| `ToString` | `string ToString(string format)` |  |
| `ToString` | `string ToString(string format, IFormatProvider provider)` |  |
| `TryFormat` | `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)` |  |
| `TryParse` | `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out ZigZag8 result)` |  |
| `TryParse` | `static bool TryParse(string s, IFormatProvider provider, out ZigZag8 result)` |  |
| `TryParse` | `static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ZigZag8 result)` |  |
| `TryParse` | `static bool TryParse(string s, out ZigZag8 result)` |  |
| `explicit operator ZigZag8` | `static explicit operator ZigZag8(byte encoded)` |  |
| `explicit operator byte` | `static explicit operator byte(ZigZag8 value)` |  |
| `explicit operator sbyte` | `static explicit operator sbyte(ZigZag8 value)` |  |
| `implicit operator Int128` | `static implicit operator Int128(ZigZag8 value)` |  |
| `implicit operator Int96` | `static implicit operator Int96(ZigZag8 value)` |  |
| `implicit operator ZigZag16` | `static implicit operator ZigZag16(ZigZag8 value)` |  |
| `implicit operator ZigZag32` | `static implicit operator ZigZag32(ZigZag8 value)` |  |
| `implicit operator ZigZag64` | `static implicit operator ZigZag64(ZigZag8 value)` |  |
| `implicit operator ZigZag8` | `static implicit operator ZigZag8(sbyte value)` |  |
| `implicit operator int` | `static implicit operator int(ZigZag8 value)` |  |
| `implicit operator long` | `static implicit operator long(ZigZag8 value)` |  |
| `implicit operator short` | `static implicit operator short(ZigZag8 value)` |  |
| `operator !=` | `static bool operator !=(ZigZag8 left, ZigZag8 right)` |  |
| `operator <=` | `static bool operator <=(ZigZag8 left, ZigZag8 right)` |  |
| `operator <` | `static bool operator <(ZigZag8 left, ZigZag8 right)` |  |
| `operator ==` | `static bool operator ==(ZigZag8 left, ZigZag8 right)` |  |
| `operator >=` | `static bool operator >=(ZigZag8 left, ZigZag8 right)` |  |
| `operator >` | `static bool operator >(ZigZag8 left, ZigZag8 right)` |  |

#### `__ClassForcingTag<T>`

_No public or protected members._

#### `__StructForcingTag<T>`

_No public or protected members._

### Namespace `System.Buffers`

[`ArrayPool`](#arraypool) · [`ArrayPool.RentArray<T>`](#arraypoolrentarrayt)

#### `ArrayPool`

| Member | Signature | Summary |
| --- | --- | --- |
| `Use` | `static RentArray<T> Use<T>(this ArrayPool<T> @this, int minimumSize)` |  |

#### `ArrayPool.RentArray<T>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Array` | `T[] Array { get; }` |  |
| `Capacity` | `int Capacity { get; }` |  |
| `Item` | `Span<T> this[Index start, int length] { get; }` |  |
| `Item` | `Span<T> this[Range range] { get; }` |  |
| `Item` | `T this[int index] { get; set; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsReadOnlySpan` | `ReadOnlySpan<T> AsReadOnlySpan()` |  |
| `AsSpan` | `Span<T> AsSpan()` |  |
| `AsSpan` | `Span<T> AsSpan(int start)` |  |
| `AsSpan` | `Span<T> AsSpan(int start, int length)` |  |
| `Dispose` | `void Dispose()` |  |
| `implicit operator ReadOnlySpan<T>` | `static implicit operator ReadOnlySpan<T>(RentArray<T> @this)` |  |
| `implicit operator Span<T>` | `static implicit operator Span<T>(RentArray<T> @this)` |  |
| `implicit operator T[]` | `static implicit operator T[](RentArray<T> @this)` |  |

### Namespace `System.Collections`

[`BitArrayExtensions`](#bitarrayextensions) · [`CollectionExtensions`](#collectionextensions) · [`EnumerableExtensions`](#enumerableextensions) · [`IBitOrder`](#ibitorder) · [`LsbFirst`](#lsbfirst) · [`MsbFirst`](#msbfirst) · [`PackedBitBuffer`](#packedbitbuffer) · [`PackedBitBuffer<TBitOrder>`](#packedbitbuffertbitorder) · [`PackedBuffer<T, TBitOrder>`](#packedbuffert-tbitorder) · [`PackedBuffer<T, TCodec, TBitOrder>`](#packedbuffert-tcodec-tbitorder)

#### `BitArrayExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetSetBits` | `static IEnumerable<int> GetSetBits(this BitArray @this)` |  |
| `GetUnsetBits` | `static IEnumerable<int> GetUnsetBits(this BitArray @this)` |  |

#### `CollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Any` | `static bool Any(this ICollection @this)` |  |
| `ConvertAll` | `static TOUT[] ConvertAll<TOUT>(this ICollection @this, Converter<object, TOUT> converter)` |  |
| `ForEach` | `static void ForEach(this ICollection @this, Action<object> call)` |  |
| `ToArray` | `static object[] ToArray(this ICollection @this)` |  |

#### `EnumerableExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Any` | `static bool Any(this IEnumerable @this)` |  |
| `ConvertAll` | `static IEnumerable ConvertAll<TIn, TOut>(this IEnumerable @this, Func<TIn, TOut> converter)` |  |
| `Count` | `static int Count(this IEnumerable @this)` |  |
| `ForEach` | `static void ForEach<TIn>(this IEnumerable @this, Action<TIn> action)` |  |
| `ToObjectArray` | `static object[] ToObjectArray(this IEnumerable @this)` |  |

#### `IBitOrder`

| Member | Signature | Summary |
| --- | --- | --- |
| `Read` | `ulong Read(byte[] data, long bitOffset, int bits)` |  |
| `Write` | `void Write(byte[] data, long bitOffset, int bits, ulong code)` |  |

#### `LsbFirst`

Implements `IBitOrder`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Read` | `ulong Read(byte[] data, long bitOffset, int bits)` |  |
| `Write` | `void Write(byte[] data, long bitOffset, int bits, ulong code)` |  |

#### `MsbFirst`

Implements `IBitOrder`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Read` | `ulong Read(byte[] data, long bitOffset, int bits)` |  |
| `Write` | `void Write(byte[] data, long bitOffset, int bits, ulong code)` |  |

#### `PackedBitBuffer`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetPackedByteCount` | `static int GetPackedByteCount(int count, int bitsPerElement)` |  |

#### `PackedBitBuffer<TBitOrder>`

| Member | Signature | Summary |
| --- | --- | --- |
| `PackedBitBuffer` | `PackedBitBuffer(int count, int bitsPerElement)` |  |
| `BitsPerElement` | `int BitsPerElement { get; }` |  |
| `Count` | `int Count { get; }` |  |
| `PackedData` | `ReadOnlySpan<byte> PackedData { get; }` |  |
| `FromPacked` | `static PackedBitBuffer<TBitOrder> FromPacked(byte[] packed, int count, int bitsPerElement)` |  |
| `GetBits` | `ulong GetBits(int index)` |  |
| `Pack` | `void Pack(ReadOnlySpan<ulong> codes)` |  |
| `SetBits` | `void SetBits(int index, ulong code)` |  |
| `Unpack` | `void Unpack(Span<ulong> destination)` |  |

#### `PackedBuffer<T, TBitOrder>`

| Member | Signature | Summary |
| --- | --- | --- |
| `PackedBuffer` | `PackedBuffer(PackedBitBuffer<TBitOrder> buffer, IBitCodec<T> codec)` |  |
| `PackedBuffer` | `PackedBuffer(int count, IBitCodec<T> codec)` |  |
| `Count` | `int Count { get; }` |  |
| `Item` | `T this[int index] { get; set; }` |  |
| `Storage` | `PackedBitBuffer<TBitOrder> Storage { get; }` |  |
| `DecodeTo` | `void DecodeTo(Span<T> destination)` |  |
| `EncodeFrom` | `void EncodeFrom(ReadOnlySpan<T> source)` |  |
| `GetEnumerator` | `IEnumerator<T> GetEnumerator()` |  |
| `ToArray` | `T[] ToArray()` |  |

#### `PackedBuffer<T, TCodec, TBitOrder>`

| Member | Signature | Summary |
| --- | --- | --- |
| `PackedBuffer` | `PackedBuffer(PackedBitBuffer<TBitOrder> buffer, TCodec codec = null)` |  |
| `PackedBuffer` | `PackedBuffer(int count, TCodec codec = null)` |  |
| `Count` | `int Count { get; }` |  |
| `Item` | `T this[int index] { get; set; }` |  |
| `Storage` | `PackedBitBuffer<TBitOrder> Storage { get; }` |  |
| `DecodeTo` | `void DecodeTo(Span<T> destination)` |  |
| `EncodeFrom` | `void EncodeFrom(ReadOnlySpan<T> source)` |  |
| `GetEnumerator` | `IEnumerator<T> GetEnumerator()` |  |
| `ToArray` | `T[] ToArray()` |  |

### Namespace `System.Collections.Concurrent`

[`ConcurrentDictionaryExtensions`](#concurrentdictionaryextensions) · [`ConcurrentQueueExtensions`](#concurrentqueueextensions) · [`ConcurrentStackExtensions`](#concurrentstackextensions) · [`ConcurrentWorkingBag<T>`](#concurrentworkingbagt) · [`ExecutiveQueue<TItem>`](#executivequeuetitem)

#### `ConcurrentDictionaryExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TKey key, TValue value)` |  |
| `Add` | `static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, IEnumerable<TKey> keys)` |  |
| `Add` | `static TKey Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, IEnumerator<TKey> keyEnumerator)` |  |
| `Add` | `static Tkey Add<Tkey, TValue>(this ConcurrentDictionary<Tkey, TValue> @this, TValue value, Func<Tkey> keyFunction)` |  |
| `GetOrAdd` | `static TKey GetOrAdd<TKey>(this ConcurrentDictionary<TKey, TKey> @this, TKey key)` |  |
| `Remove` | `static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TKey key)` |  |
| `TryGetKey` | `static bool TryGetKey<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TValue value, out TKey key)` |  |

#### `ConcurrentQueueExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `PullAll` | `static T[] PullAll<T>(this ConcurrentQueue<T> @this)` |  |
| `PullTo` | `static Span<T> PullTo<T>(this ConcurrentQueue<T> @this, Span<T> target)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target, int offset)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentQueue<T> @this, T[] target, int offset, int maxCount)` |  |
| `Pull` | `static T[] Pull<T>(this ConcurrentQueue<T> @this, int maxCount)` |  |

#### `ConcurrentStackExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Pop` | `static TItem Pop<TItem>(this ConcurrentStack<TItem> @this)` |  |
| `PullAll` | `static T[] PullAll<T>(this ConcurrentStack<T> @this)` |  |
| `PullTo` | `static Span<T> PullTo<T>(this ConcurrentStack<T> @this, Span<T> target)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentStack<T> @this, T[] target)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentStack<T> @this, T[] target, int offset)` |  |
| `PullTo` | `static int PullTo<T>(this ConcurrentStack<T> @this, T[] target, int offset, int maxCount)` |  |
| `Pull` | `static T[] Pull<T>(this ConcurrentStack<T> @this, int maxCount)` |  |
| `PushRange` | `static void PushRange<TItem>(this ConcurrentStack<TItem> @this, IEnumerable<TItem> items)` |  |

#### `ConcurrentWorkingBag<T>`

Implements `IEnumerable`, `IEnumerable<T>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ConcurrentWorkingBag` | `ConcurrentWorkingBag()` |  |
| `Count` | `int Count { get; }` |  |
| `AddOrExecute` | `bool AddOrExecute(Func<T, bool> predicate, Action<T> call, Func<T> factory)` |  |
| `AddOrReplace` | `bool AddOrReplace(Func<T, bool> selector, Func<T, T> call, Func<T> factory)` |  |
| `GetEnumerator` | `IEnumerator<T> GetEnumerator()` |  |
| `ToArray` | `T[] ToArray()` |  |
| `TryRemove` | `bool TryRemove(Func<T, bool> selector, out T[] removed)` |  |

#### `ExecutiveQueue<TItem>`

| Member | Signature | Summary |
| --- | --- | --- |
| `ExecutiveQueue` | `ExecutiveQueue(Action<TItem> callback, bool isAsync = true, Action<TItem, Exception> exceptionCallback = null, int maxItems = 2147483647, TimeSpan? executionDelay = null, TimeSpan? dequeueThrottle = null, TimeSpan? overflowThrottle = null)` |  |
| `Count` | `int Count { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Clear` | `void Clear()` |  |
| `Dequeue` | `TItem Dequeue()` |  |
| `Enqueue` | `void Enqueue(TItem item)` |  |
| `ToArray` | `TItem[] ToArray()` |  |
| `TryDequeue` | `bool TryDequeue(out TItem item)` |  |

### Namespace `System.Collections.Generic`

[`BiDictionary<TFirst, TSecond>`](#bidictionarytfirst-tsecond) · [`Cache<TInput, TValue>`](#cachetinput-tvalue) · [`Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue>`](#cachetinput1-tinput2-tinput3-tinput4-tinput5-tinput6-tinput7-tvalue) · [`Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue>`](#cachetinput1-tinput2-tinput3-tinput4-tinput5-tinput6-tvalue) · [`Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue>`](#cachetinput1-tinput2-tinput3-tinput4-tinput5-tvalue) · [`Cache<TInput1, TInput2, TInput3, TInput4, TValue>`](#cachetinput1-tinput2-tinput3-tinput4-tvalue) · [`Cache<TInput1, TInput2, TInput3, TValue>`](#cachetinput1-tinput2-tinput3-tvalue) · [`Cache<TInput1, TInput2, TValue>`](#cachetinput1-tinput2-tvalue) · [`CacheReplacementPolicy`](#cachereplacementpolicy) · [`CachedEnumeration<TItem>`](#cachedenumerationtitem) · [`CollectionExtensions`](#collectionextensions) · [`DictionaryExtensions`](#dictionaryextensions) · [`DictionaryExtensions.ChangeType`](#dictionaryextensionschangetype) · [`DictionaryExtensions.IChangeSet<TKey, TValue>`](#dictionaryextensionsichangesettkey-tvalue) · [`DoubleDictionary<TOuter, TInner, TValue>`](#doubledictionarytouter-tinner-tvalue) · [`EnumerableExtensions`](#enumerableextensions) · [`EnumerableExtensions.ChangeType`](#enumerableextensionschangetype) · [`EnumerableExtensions.IChangeSet<TItem>`](#enumerableextensionsichangesettitem) · [`EnumerableExtensions.IDisposableCollection<T>`](#enumerableextensionsidisposablecollectiont) · [`EnumerableExtensions.IShuffledEnumerable<TItem>`](#enumerableextensionsishuffledenumerabletitem) · [`EnumeratorExtensions`](#enumeratorextensions) · [`FastLookupTable<TItem>`](#fastlookuptabletitem) · [`HashSetExtensions`](#hashsetextensions) · [`HashSetExtensions.ChangeType`](#hashsetextensionschangetype) · [`HashSetExtensions.IChangeSet<TItem>`](#hashsetextensionsichangesettitem) · [`ICache<TInput, TValue>`](#icachetinput-tvalue) · [`ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue>`](#icachetinput1-tinput2-tinput3-tinput4-tinput5-tinput6-tinput7-tvalue) · [`ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue>`](#icachetinput1-tinput2-tinput3-tinput4-tinput5-tinput6-tvalue) · [`ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue>`](#icachetinput1-tinput2-tinput3-tinput4-tinput5-tvalue) · [`ICache<TInput1, TInput2, TInput3, TInput4, TValue>`](#icachetinput1-tinput2-tinput3-tinput4-tvalue) · [`ICache<TInput1, TInput2, TInput3, TValue>`](#icachetinput1-tinput2-tinput3-tvalue) · [`ICache<TInput1, TInput2, TValue>`](#icachetinput1-tinput2-tvalue) · [`IQueue<T>`](#iqueuet) · [`KeyValuePairExtensions`](#keyvaluepairextensions) · [`LinkedListExtensions`](#linkedlistextensions) · [`ListExtensions`](#listextensions) · [`QueueExtensions`](#queueextensions) · [`StackExtensions`](#stackextensions)

#### `BiDictionary<TFirst, TSecond>`

Implements `ICollection`, `ICollection<KeyValuePair<TFirst, TSecond>>`, `IDictionary`, `IDictionary<TFirst, TSecond>`, `IEnumerable`, `IEnumerable<KeyValuePair<TFirst, TSecond>>`, `IReadOnlyCollection<KeyValuePair<TFirst, TSecond>>`, `IReadOnlyDictionary<TFirst, TSecond>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `BiDictionary` | `BiDictionary()` |  |
| `Count` | `int Count { get; }` |  |
| `IsReadOnly` | `bool IsReadOnly { get; }` |  |
| `Item` | `TSecond this[TFirst key] { get; set; }` |  |
| `Keys` | `ICollection<TFirst> Keys { get; }` |  |
| `Reverse` | `IDictionary<TSecond, TFirst> Reverse { get; }` |  |
| `Values` | `ICollection<TSecond> Values { get; }` |  |
| `Add` | `void Add(KeyValuePair<TFirst, TSecond> item)` |  |
| `Add` | `void Add(TFirst key, TSecond value)` |  |
| `Clear` | `void Clear()` |  |
| `ContainsKey` | `bool ContainsKey(TFirst key)` |  |
| `ContainsValue` | `bool ContainsValue(TSecond value)` |  |
| `Contains` | `bool Contains(KeyValuePair<TFirst, TSecond> item)` |  |
| `Contains` | `bool Contains(object key)` |  |
| `CopyTo` | `void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex)` |  |
| `GetEnumerator` | `IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()` |  |
| `Remove` | `bool Remove(KeyValuePair<TFirst, TSecond> item)` |  |
| `Remove` | `bool Remove(TFirst key)` |  |
| `TryGetKey` | `bool TryGetKey(TSecond key, out TFirst value)` |  |
| `TryGetValue` | `bool TryGetValue(TFirst key, out TSecond value)` |  |

#### `Cache<TInput, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput, TValue> FromFactoryWithMaxItemCount(Func<TInput, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput, TValue> FromFactoryWithMaxItemLifetime(Func<TInput, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput, TValue> FromFactoryWithMaxMemorySize(Func<TInput, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TInput5, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TInput3, TInput4, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TInput4, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TInput3, TInput4, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TInput3, TInput4, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TInput3, TInput4, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TInput4, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TInput3, TInput4, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TInput3, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TInput3, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TInput3, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TInput3, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TInput3, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TInput3, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TInput3, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TInput3, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TInput3, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `Cache<TInput1, TInput2, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `FromFactoryWithMaxItemCount` | `static ICache<TInput1, TInput2, TValue> FromFactoryWithMaxItemCount(Func<TInput1, TInput2, TValue> factory, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxItemCount` | `static ICache<TInput1, TInput2, TValue> FromFactoryWithMaxItemLifetimeAndMaxItemCount(Func<TInput1, TInput2, TValue> factory, TimeSpan maxAge, int maxItems, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetimeAndMaxMemorySize` | `static ICache<TInput1, TInput2, TValue> FromFactoryWithMaxItemLifetimeAndMaxMemorySize(Func<TInput1, TInput2, TValue> factory, TimeSpan maxAge, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |
| `FromFactoryWithMaxItemLifetime` | `static ICache<TInput1, TInput2, TValue> FromFactoryWithMaxItemLifetime(Func<TInput1, TInput2, TValue> factory, TimeSpan maxAge)` |  |
| `FromFactoryWithMaxMemorySize` | `static ICache<TInput1, TInput2, TValue> FromFactoryWithMaxMemorySize(Func<TInput1, TInput2, TValue> factory, long maxSizeInBytes, CacheReplacementPolicy policy, bool lazyCollect = false)` |  |

#### `CacheReplacementPolicy`

| Member | Signature | Summary |
| --- | --- | --- |
| `CacheReplacementPolicy` | `protected CacheReplacementPolicy(byte value)` |  |
| `FirstInFirstOut` | `static CacheReplacementPolicy FirstInFirstOut { get; }` |  |
| `LargeValuesFirst` | `static CacheReplacementPolicy LargeValuesFirst { get; }` |  |
| `LastInFirstOut` | `static CacheReplacementPolicy LastInFirstOut { get; }` |  |
| `LeastAvailableLifetimeLeft` | `static CacheReplacementPolicy LeastAvailableLifetimeLeft { get; }` |  |
| `LeastFrequentlyUsed` | `static CacheReplacementPolicy LeastFrequentlyUsed { get; }` |  |
| `LeastRecentlyUsed` | `static CacheReplacementPolicy LeastRecentlyUsed { get; }` |  |
| `MostAvailableLifetimeLeft` | `static CacheReplacementPolicy MostAvailableLifetimeLeft { get; }` |  |
| `MostFrequentlyUsed` | `static CacheReplacementPolicy MostFrequentlyUsed { get; }` |  |
| `MostRecentlyUsed` | `static CacheReplacementPolicy MostRecentlyUsed { get; }` |  |
| `Random` | `static CacheReplacementPolicy Random { get; }` |  |
| `SmallValuesFirst` | `static CacheReplacementPolicy SmallValuesFirst { get; }` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `Equals` | `protected bool Equals(CacheReplacementPolicy other)` |  |
| `Equals` | `static bool Equals(CacheReplacementPolicy a, CacheReplacementPolicy b)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `ToString` | `override string ToString()` |  |
| `operator !=` | `static bool operator !=(CacheReplacementPolicy a, CacheReplacementPolicy b)` |  |
| `operator ==` | `static bool operator ==(CacheReplacementPolicy a, CacheReplacementPolicy b)` |  |

#### `CachedEnumeration<TItem>`

Implements `IDisposable`, `IEnumerable`, `IEnumerable<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `CachedItemCount` | `int CachedItemCount { get; }` |  |
| `Item` | `TItem this[int index] { get; }` |  |
| `Dispose` | `protected virtual void Dispose(bool disposing)` |  |
| `Dispose` | `void Dispose()` |  |
| `GetEnumerator` | `IEnumerator<TItem> GetEnumerator()` |  |
| `Reset` | `void Reset()` |  |

#### `CollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items)` |  |
| `Any` | `static bool Any<TItem>(this ICollection<TItem> @this)` |  |
| `ConvertAll` | `static TOut[] ConvertAll<TIn, TOut>(this ICollection<TIn> @this, Converter<TIn, TOut> converter)` |  |
| `ForEach` | `static void ForEach<TValue>(this ICollection<TValue> @this, Action<TValue> action)` |  |
| `IsMultiple` | `static bool IsMultiple<TValue>(this ICollection<TValue> @this)` |  |
| `IsNoMultiple` | `static bool IsNoMultiple<TValue>(this ICollection<TValue> @this)` |  |
| `IsNoSingle` | `static bool IsNoSingle<TValue>(this ICollection<TValue> @this)` |  |
| `IsSingle` | `static bool IsSingle<TValue>(this ICollection<TValue> @this)` |  |
| `RemoveRange` | `static void RemoveRange<TItem>(this ICollection<TItem> @this, IEnumerable<TItem> items)` |  |

#### `DictionaryExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddOrUpdate` | `static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)` |  |
| `AddOrUpdate` | `static TValue AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)` |  |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> values)` |  |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<Tuple<TKey, TValue>> values)` |  |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)` |  |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, params KeyValuePair<TKey, TValue>[] values)` |  |
| `AddOrUpdate` | `static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, params Tuple<TKey, TValue>[] values)` |  |
| `AddRange` | `static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)` |  |
| `AddRange` | `static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> @this, params object[] keyValuePairs)` |  |
| `Add` | `static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> @this, TValue value, Func<TKey> generatorFunction)` |  |
| `Add` | `static TKey Add<TKey, TValue>(this IDictionary<TKey, TValue> @this, TValue value, IEnumerator<TKey> keyEnumerator)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `AndOrAdd` | `static void AndOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `CompareExchangeOrAdd` | `static Half CompareExchangeOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half value, Half comparand)` |  |
| `CompareExchangeOrAdd` | `static Int128 CompareExchangeOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value, Int128 comparand)` |  |
| `CompareExchangeOrAdd` | `static Int96 CompareExchangeOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value, Int96 comparand)` |  |
| `CompareExchangeOrAdd` | `static UInt128 CompareExchangeOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value, UInt128 comparand)` |  |
| `CompareExchangeOrAdd` | `static UInt96 CompareExchangeOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value, UInt96 comparand)` |  |
| `CompareExchangeOrAdd` | `static byte CompareExchangeOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value, byte comparand)` |  |
| `CompareExchangeOrAdd` | `static decimal CompareExchangeOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal value, decimal comparand)` |  |
| `CompareExchangeOrAdd` | `static double CompareExchangeOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double value, double comparand)` |  |
| `CompareExchangeOrAdd` | `static float CompareExchangeOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float value, float comparand)` |  |
| `CompareExchangeOrAdd` | `static int CompareExchangeOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value, int comparand)` |  |
| `CompareExchangeOrAdd` | `static long CompareExchangeOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value, long comparand)` |  |
| `CompareExchangeOrAdd` | `static sbyte CompareExchangeOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value, sbyte comparand)` |  |
| `CompareExchangeOrAdd` | `static short CompareExchangeOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value, short comparand)` |  |
| `CompareExchangeOrAdd` | `static uint CompareExchangeOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value, uint comparand)` |  |
| `CompareExchangeOrAdd` | `static ulong CompareExchangeOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value, ulong comparand)` |  |
| `CompareExchangeOrAdd` | `static ushort CompareExchangeOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value, ushort comparand)` |  |
| `CompareTo` | `static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this Dictionary<TKey, TValue> @this, Dictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null)` |  |
| `CompareTo` | `static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null)` |  |
| `CompareTo` | `static IEnumerable<IChangeSet<TKey, TValue>> CompareTo<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, IReadOnlyDictionary<TKey, TValue> other, IEqualityComparer<TValue> valueComparer = null, IEqualityComparer<TKey> keyComparer = null)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong amount)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key)` |  |
| `DecrementOrAdd` | `static void DecrementOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort amount)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, double> @this, TKey key, double divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, float> @this, TKey key, float divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, int> @this, TKey key, int divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, long> @this, TKey key, long divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, short> @this, TKey key, short divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong divisor)` |  |
| `DivideOrSet` | `static void DivideOrSet<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort divisor)` |  |
| `ExchangeOrAdd` | `static Half ExchangeOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half value)` |  |
| `ExchangeOrAdd` | `static Int128 ExchangeOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `ExchangeOrAdd` | `static Int96 ExchangeOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `ExchangeOrAdd` | `static UInt128 ExchangeOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `ExchangeOrAdd` | `static UInt96 ExchangeOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `ExchangeOrAdd` | `static byte ExchangeOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `ExchangeOrAdd` | `static decimal ExchangeOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal value)` |  |
| `ExchangeOrAdd` | `static double ExchangeOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double value)` |  |
| `ExchangeOrAdd` | `static float ExchangeOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float value)` |  |
| `ExchangeOrAdd` | `static int ExchangeOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `ExchangeOrAdd` | `static long ExchangeOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `ExchangeOrAdd` | `static sbyte ExchangeOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `ExchangeOrAdd` | `static short ExchangeOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `ExchangeOrAdd` | `static uint ExchangeOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `ExchangeOrAdd` | `static ulong ExchangeOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `ExchangeOrAdd` | `static ushort ExchangeOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `FullCast` | `static Dictionary<TKeyTarget, TValueTarget> FullCast<TKey, TValue, TKeyTarget, TValueTarget>(this IDictionary<TKey, TValue> @this)` |  |
| `GetOrAddDefault` | `static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)` |  |
| `GetOrAdd` | `static TKey GetOrAdd<TKey>(this IDictionary<TKey, TKey> @this, TKey key)` |  |
| `GetOrAdd` | `static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)` |  |
| `GetOrAdd` | `static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue defaultValue)` |  |
| `GetOrAdd` | `static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> defaultValueFactory)` |  |
| `GetOrAdd` | `static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> defaultValueFactory)` |  |
| `GetOrAdd` | `static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue defaultValue)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> defaultValueFactory)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> defaultValueFactory)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key)` |  |
| `GetValueOrDefault` | `static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key, TValue defaultValue)` |  |
| `GetValueOrNull` | `static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, __ClassForcingTag<TValue> _ = null)` |  |
| `GetValueOrNull` | `static TValue? GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, __StructForcingTag<TValue> _ = null)` |  |
| `HasKeyDo` | `static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TKey, TValue> action)` |  |
| `HasKeyDo` | `static bool HasKeyDo<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Action<TValue> action)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong amount)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key)` |  |
| `IncrementOrAdd` | `static void IncrementOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, int amount)` |  |
| `LeftShiftOrAdd` | `static void LeftShiftOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, int amount)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `MaxOrAdd` | `static void MaxOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, double> @this, TKey key, double value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, float> @this, TKey key, float value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `MinOrAdd` | `static void MinOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `MissesKey` | `static bool MissesKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, double> @this, TKey key, double divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, float> @this, TKey key, float divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, int> @this, TKey key, int divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, long> @this, TKey key, long divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, short> @this, TKey key, short divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong divisor)` |  |
| `ModuloOrSet` | `static void ModuloOrSet<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort divisor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, Half> @this, TKey key, Half factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, decimal> @this, TKey key, decimal factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, double> @this, TKey key, double factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, float> @this, TKey key, float factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, int> @this, TKey key, int factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, long> @this, TKey key, long factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, short> @this, TKey key, short factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong factor)` |  |
| `MultiplyOrSet` | `static void MultiplyOrSet<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort factor)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `NandOrAdd` | `static void NandOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `NorOrAdd` | `static void NorOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, Int128> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, Int96> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, UInt128> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, UInt96> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, byte> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, int> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, long> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, sbyte> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, short> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, uint> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, ulong> @this, TKey key)` |  |
| `NotOrSet` | `static void NotOrSet<TKey>(this Dictionary<TKey, ushort> @this, TKey key)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `OrOrAdd` | `static void OrOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, int amount)` |  |
| `RightShiftOrAdd` | `static void RightShiftOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, int amount)` |  |
| `RotateLeftOrAdd` | `static void RotateLeftOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, int amount)` |  |
| `RotateRightOrAdd` | `static void RotateRightOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, int amount)` |  |
| `TryAdd` | `static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)` |  |
| `TryRemove` | `static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, out TValue value)` |  |
| `TryUpdate` | `static bool TryUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue newValue, TValue comparisonValue, IEqualityComparer<TValue> comparer = null)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, int amount)` |  |
| `UnsignedRightShiftOrAdd` | `static void UnsignedRightShiftOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, int amount)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `XnorOrAdd` | `static void XnorOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, Int128> @this, TKey key, Int128 value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, Int96> @this, TKey key, Int96 value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, UInt128> @this, TKey key, UInt128 value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, UInt96> @this, TKey key, UInt96 value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, byte> @this, TKey key, byte value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, int> @this, TKey key, int value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, long> @this, TKey key, long value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, sbyte> @this, TKey key, sbyte value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, short> @this, TKey key, short value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, uint> @this, TKey key, uint value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, ulong> @this, TKey key, ulong value)` |  |
| `XorOrAdd` | `static void XorOrAdd<TKey>(this Dictionary<TKey, ushort> @this, TKey key, ushort value)` |  |

#### `DictionaryExtensions.ChangeType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Equal` | `0` |  |
| `Changed` | `1` |  |
| `Added` | `2` |  |
| `Removed` | `3` |  |

#### `DictionaryExtensions.IChangeSet<TKey, TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `Current` | `TValue Current { get; }` |  |
| `Key` | `TKey Key { get; }` |  |
| `Other` | `TValue Other { get; }` |  |
| `Type` | `ChangeType Type { get; }` |  |

#### `DoubleDictionary<TOuter, TInner, TValue>`

Inherits `Dictionary<TOuter, Dictionary<TInner, TValue>>`. Implements `ICollection`, `ICollection<KeyValuePair<TOuter, Dictionary<TInner, TValue>>>`, `IDeserializationCallback`, `IDictionary`, `IDictionary<TOuter, Dictionary<TInner, TValue>>`, `IEnumerable`, `IEnumerable<KeyValuePair<TOuter, Dictionary<TInner, TValue>>>`, `IReadOnlyCollection<KeyValuePair<TOuter, Dictionary<TInner, TValue>>>`, `IReadOnlyDictionary<TOuter, Dictionary<TInner, TValue>>`, `ISerializable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `DoubleDictionary` | `DoubleDictionary()` |  |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TOuter outerKey, TInner innerKey] { get; set; }` |  |
| `OuterCount` | `int OuterCount { get; }` |  |
| `Add` | `void Add(TOuter outer, TInner inner, TValue value)` |  |
| `ContainsKey` | `bool ContainsKey(TOuter outerKey, TInner innerKey)` |  |
| `Remove` | `void Remove(TOuter outerKey, TInner innerKey)` |  |
| `TryAdd` | `bool TryAdd(TOuter outerKey, TInner innerKey, TValue value)` |  |
| `TryGetValue` | `bool TryGetValue(TOuter outerKey, TInner innerKey, out TValue value)` |  |
| `TryRemove` | `bool TryRemove(TOuter outerKey, TInner innerKey, out TValue value)` |  |

#### `EnumerableExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `All` | `static bool All<TItem>(this IEnumerable<TItem> @this, Func<TItem, int, bool> condition)` |  |
| `Append` | `static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items)` |  |
| `Append` | `static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> @this, params TItem[] items)` |  |
| `AreEqual` | `static bool AreEqual<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer = null)` |  |
| `AsProgressReporting` | `static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, Action<double> progressCallback, bool delayed = false)` |  |
| `AsProgressReporting` | `static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, Action<long, long> progressCallback, bool delayed = false)` |  |
| `AsProgressReporting` | `static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, int length, Action<double> progressCallback, bool delayed = false)` |  |
| `AsProgressReporting` | `static IEnumerable<TItem> AsProgressReporting<TItem>(this IEnumerable<TItem> @this, int length, Action<long, long> progressCallback, bool delayed = false)` |  |
| `AverageOrDefault` | `static decimal AverageOrDefault(this IEnumerable<decimal> @this, decimal defaultValue = 0)` |  |
| `AverageOrDefault` | `static decimal AverageOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector, decimal defaultValue = 0)` |  |
| `AverageOrDefault` | `static decimal? AverageOrDefault(this IEnumerable<decimal?> @this, decimal? defaultValue = null)` |  |
| `AverageOrDefault` | `static double AverageOrDefault(this IEnumerable<double> @this, double defaultValue = 0)` |  |
| `AverageOrDefault` | `static double AverageOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector, double defaultValue = 0)` |  |
| `AverageOrDefault` | `static double? AverageOrDefault(this IEnumerable<double?> @this, double? defaultValue = null)` |  |
| `AverageOrDefault` | `static float AverageOrDefault(this IEnumerable<float> @this, float defaultValue = 0)` |  |
| `AverageOrDefault` | `static float AverageOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector, float defaultValue = 0)` |  |
| `AverageOrDefault` | `static float? AverageOrDefault(this IEnumerable<float?> @this, float? defaultValue = null)` |  |
| `Average` | `static TimeSpan Average(this IEnumerable<TimeSpan> @this)` |  |
| `Average` | `static TimeSpan Average<TIn>(this IEnumerable<TIn> @this, Func<TIn, TimeSpan> selector)` |  |
| `Center` | `static decimal Center(this IEnumerable<decimal> @this)` |  |
| `Center` | `static decimal Center(this IEnumerable<decimal?> @this)` |  |
| `Center` | `static decimal Center<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector)` |  |
| `Center` | `static double Center(this IEnumerable<double> @this)` |  |
| `Center` | `static double Center(this IEnumerable<double?> @this)` |  |
| `Center` | `static double Center<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector)` |  |
| `Center` | `static float Center(this IEnumerable<float> @this)` |  |
| `Center` | `static float Center(this IEnumerable<float?> @this)` |  |
| `Center` | `static float Center<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector)` |  |
| `CompareTo` | `static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, IEqualityComparer<TItem> comparer = null)` |  |
| `ConcatAll` | `static IEnumerable<TItem> ConcatAll<TItem>(this IEnumerable<IEnumerable<TItem>> @this)` |  |
| `ConcatAll` | `static byte[] ConcatAll(this IEnumerable<byte[]> @this)` |  |
| `ContainsAny` | `static bool ContainsAny<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> list, IEqualityComparer<TItem> equalityComparer = null)` |  |
| `ContainsNotAny` | `static bool ContainsNotAny<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items, IEqualityComparer<TItem> comparer = null)` |  |
| `ContainsNot` | `static bool ContainsNot<TItem>(this IEnumerable<TItem> @this, TItem item, IEqualityComparer<TItem> comparer = null)` |  |
| `ConvertAll` | `static IEnumerable<TResult> ConvertAll<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> converter)` |  |
| `ConvertAll` | `static IEnumerable<TResult> ConvertAll<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, int, TResult> converter)` |  |
| `Covariance` | `static decimal Covariance(this IEnumerable<decimal> @this, IEnumerable<decimal> other)` |  |
| `Covariance` | `static decimal Covariance(this IEnumerable<decimal?> @this, IEnumerable<decimal?> other)` |  |
| `Covariance` | `static decimal Covariance<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector1, IEnumerable<TItem> other, Func<TItem, decimal> selector2)` |  |
| `Covariance` | `static decimal Covariance<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, Func<TItem, decimal> selector)` |  |
| `Covariance` | `static double Covariance(this IEnumerable<double> @this, IEnumerable<double> other)` |  |
| `Covariance` | `static double Covariance(this IEnumerable<double?> @this, IEnumerable<double?> other)` |  |
| `Covariance` | `static double Covariance<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector1, IEnumerable<TItem> other, Func<TItem, double> selector2)` |  |
| `Covariance` | `static double Covariance<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, Func<TItem, double> selector)` |  |
| `Covariance` | `static float Covariance(this IEnumerable<float> @this, IEnumerable<float> other)` |  |
| `Covariance` | `static float Covariance(this IEnumerable<float?> @this, IEnumerable<float?> other)` |  |
| `Covariance` | `static float Covariance<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector1, IEnumerable<TItem> other, Func<TItem, float> selector2)` |  |
| `Covariance` | `static float Covariance<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other, Func<TItem, float> selector)` |  |
| `Distinct` | `static IEnumerable<TItem> Distinct<TItem, TCompare>(this IEnumerable<TItem> @this, Func<TItem, TCompare> selector)` |  |
| `DoForEveryItemAsync` | `static Task DoForEveryItemAsync<TItem>(this IEnumerable<TItem> @this, Action<TItem> action)` |  |
| `FilterIfNeeded` | `static IEnumerable<TItem> FilterIfNeeded<TItem>(this IEnumerable<TItem> @this, Func<TItem, string> selector, string query, bool ignoreCase = false)` |  |
| `FilterIfNeeded` | `static IEnumerable<TItem> FilterIfNeeded<TItem>(this IEnumerable<TItem> @this, string query, bool ignoreCase, params Func<TItem, string>[] selectors)` |  |
| `FilterIfNeeded` | `static IEnumerable<TItem> FilterIfNeeded<TItem>(this IEnumerable<TItem> @this, string query, params Func<TItem, string>[] selectors)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TItem> defaultValueFactory)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, TItem> defaultValueFactory)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<TItem> defaultValueFactory)` |  |
| `FirstOrDefault` | `static TItem FirstOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory)` |  |
| `FirstOrNull` | `static TItem FirstOrNull<TItem>(this IEnumerable<TItem> @this, __ClassForcingTag<TItem> _ = null)` |  |
| `FirstOrNull` | `static TItem? FirstOrNull<TItem>(this IEnumerable<TItem> @this, __StructForcingTag<TItem> _ = null)` |  |
| `ForEach` | `static void ForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem, int> action)` |  |
| `ForEach` | `static void ForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem> action)` |  |
| `HasMultiple` | `static bool HasMultiple<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)` |  |
| `HasMultiple` | `static bool HasMultiple<TItem>(this IEnumerable<TItem> @this, TItem value)` |  |
| `HasNoMultiple` | `static bool HasNoMultiple<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)` |  |
| `HasNoMultiple` | `static bool HasNoMultiple<TItem>(this IEnumerable<TItem> @this, TItem value)` |  |
| `HasNoSingle` | `static bool HasNoSingle<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)` |  |
| `HasNoSingle` | `static bool HasNoSingle<TItem>(this IEnumerable<TItem> @this, TItem value)` |  |
| `HasSingle` | `static bool HasSingle<TItem>(this IEnumerable<TItem> @this, Predicate<TItem> predicate)` |  |
| `HasSingle` | `static bool HasSingle<TItem>(this IEnumerable<TItem> @this, TItem value)` |  |
| `IndexOf` | `static int IndexOf<TItem>(this IEnumerable<TItem> @this, TItem item)` |  |
| `IndexOrDefault` | `static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, int> defaultValue)` |  |
| `IndexOrDefault` | `static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<int> defaultValue)` |  |
| `IndexOrDefault` | `static int IndexOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, int defaultValue = -1)` |  |
| `IsMultiple` | `static bool IsMultiple<TItem>(this IEnumerable<TItem> @this)` |  |
| `IsNoMultiple` | `static bool IsNoMultiple<TItem>(this IEnumerable<TItem> @this)` |  |
| `IsNoSingle` | `static bool IsNoSingle<TItem>(this IEnumerable<TItem> @this)` |  |
| `IsNotNullOrEmpty` | `static bool IsNotNullOrEmpty<TItem>(this IEnumerable<TItem> @this)` |  |
| `IsNotNullOrEmpty` | `static bool IsNotNullOrEmpty<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate)` |  |
| `IsNullOrEmpty` | `static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> @this)` |  |
| `IsNullOrEmpty` | `static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate)` |  |
| `IsSingle` | `static bool IsSingle<TItem>(this IEnumerable<TItem> @this)` |  |
| `Join` | `static string Join<TItem>(this IEnumerable<TItem> @this, string join = ", ", bool skipDefaults = false, Func<TItem, string> converter = null)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TItem> defaultValueFactory)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<IEnumerable<TItem>, TItem> defaultValueFactory)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> selector, Func<TItem> defaultValueFactory)` |  |
| `LastOrDefault` | `static TItem LastOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory)` |  |
| `LastOrNull` | `static TItem LastOrNull<TItem>(this IEnumerable<TItem> @this, __ClassForcingTag<TItem> _ = null)` |  |
| `LastOrNull` | `static TItem? LastOrNull<TItem>(this IEnumerable<TItem> @this, __StructForcingTag<TItem> _ = null)` |  |
| `MaxOrDefault` | `static byte MaxOrDefault(this IEnumerable<byte> @this, byte defaultValue = 0)` |  |
| `MaxOrDefault` | `static byte MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, byte> selector, byte defaultValue = 0)` |  |
| `MaxOrDefault` | `static byte? MaxOrDefault(this IEnumerable<byte?> @this, byte? defaultValue = null)` |  |
| `MaxOrDefault` | `static decimal MaxOrDefault(this IEnumerable<decimal> @this, decimal defaultValue = 0)` |  |
| `MaxOrDefault` | `static decimal MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector, decimal defaultValue = 0)` |  |
| `MaxOrDefault` | `static decimal? MaxOrDefault(this IEnumerable<decimal?> @this, decimal? defaultValue = null)` |  |
| `MaxOrDefault` | `static double MaxOrDefault(this IEnumerable<double> @this, double defaultValue = 0)` |  |
| `MaxOrDefault` | `static double MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector, double defaultValue = 0)` |  |
| `MaxOrDefault` | `static double? MaxOrDefault(this IEnumerable<double?> @this, double? defaultValue = null)` |  |
| `MaxOrDefault` | `static float MaxOrDefault(this IEnumerable<float> @this, float defaultValue = 0)` |  |
| `MaxOrDefault` | `static float MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector, float defaultValue = 0)` |  |
| `MaxOrDefault` | `static float? MaxOrDefault(this IEnumerable<float?> @this, float? defaultValue = null)` |  |
| `MaxOrDefault` | `static int MaxOrDefault(this IEnumerable<int> @this, int defaultValue = 0)` |  |
| `MaxOrDefault` | `static int MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, int> selector, int defaultValue = 0)` |  |
| `MaxOrDefault` | `static int? MaxOrDefault(this IEnumerable<int?> @this, int? defaultValue = null)` |  |
| `MaxOrDefault` | `static long MaxOrDefault(this IEnumerable<long> @this, long defaultValue = 0)` |  |
| `MaxOrDefault` | `static long MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, long> selector, long defaultValue = 0)` |  |
| `MaxOrDefault` | `static long? MaxOrDefault(this IEnumerable<long?> @this, long? defaultValue = null)` |  |
| `MaxOrDefault` | `static sbyte MaxOrDefault(this IEnumerable<sbyte> @this, sbyte defaultValue = 0)` |  |
| `MaxOrDefault` | `static sbyte MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, sbyte> selector, sbyte defaultValue = 0)` |  |
| `MaxOrDefault` | `static sbyte? MaxOrDefault(this IEnumerable<sbyte?> @this, sbyte? defaultValue = null)` |  |
| `MaxOrDefault` | `static short MaxOrDefault(this IEnumerable<short> @this, short defaultValue = 0)` |  |
| `MaxOrDefault` | `static short MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, short> selector, short defaultValue = 0)` |  |
| `MaxOrDefault` | `static short? MaxOrDefault(this IEnumerable<short?> @this, short? defaultValue = null)` |  |
| `MaxOrDefault` | `static uint MaxOrDefault(this IEnumerable<uint> @this, uint defaultValue = 0)` |  |
| `MaxOrDefault` | `static uint MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, uint> selector, uint defaultValue = 0)` |  |
| `MaxOrDefault` | `static uint? MaxOrDefault(this IEnumerable<uint?> @this, uint? defaultValue = null)` |  |
| `MaxOrDefault` | `static ulong MaxOrDefault(this IEnumerable<ulong> @this, ulong defaultValue = 0)` |  |
| `MaxOrDefault` | `static ulong MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, ulong> selector, ulong defaultValue = 0)` |  |
| `MaxOrDefault` | `static ulong? MaxOrDefault(this IEnumerable<ulong?> @this, ulong? defaultValue = null)` |  |
| `MaxOrDefault` | `static ushort MaxOrDefault(this IEnumerable<ushort> @this, ushort defaultValue = 0)` |  |
| `MaxOrDefault` | `static ushort MaxOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, ushort> selector, ushort defaultValue = 0)` |  |
| `MaxOrDefault` | `static ushort? MaxOrDefault(this IEnumerable<ushort?> @this, ushort? defaultValue = null)` |  |
| `Max` | `static byte Max(this IEnumerable<byte> @this)` |  |
| `Max` | `static byte Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, byte> selector)` |  |
| `Max` | `static sbyte Max(this IEnumerable<sbyte> @this)` |  |
| `Max` | `static sbyte Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, sbyte> selector)` |  |
| `Max` | `static short Max(this IEnumerable<short> @this)` |  |
| `Max` | `static short Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, short> selector)` |  |
| `Max` | `static uint Max(this IEnumerable<uint> @this)` |  |
| `Max` | `static uint Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, uint> selector)` |  |
| `Max` | `static ulong Max(this IEnumerable<ulong> @this)` |  |
| `Max` | `static ulong Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, ulong> selector)` |  |
| `Max` | `static ushort Max(this IEnumerable<ushort> @this)` |  |
| `Max` | `static ushort Max<TItem>(this IEnumerable<TItem> @this, Func<TItem, ushort> selector)` |  |
| `Median` | `static decimal Median(this IEnumerable<decimal> @this)` |  |
| `Median` | `static decimal Median(this IEnumerable<decimal?> @this)` |  |
| `Median` | `static decimal Median<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector)` |  |
| `Median` | `static double Median(this IEnumerable<double> @this)` |  |
| `Median` | `static double Median(this IEnumerable<double?> @this)` |  |
| `Median` | `static double Median<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector)` |  |
| `Median` | `static float Median(this IEnumerable<float> @this)` |  |
| `Median` | `static float Median(this IEnumerable<float?> @this)` |  |
| `Median` | `static float Median<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector)` |  |
| `MinOrDefault` | `static byte MinOrDefault(this IEnumerable<byte> @this, byte defaultValue = 0)` |  |
| `MinOrDefault` | `static byte MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, byte> selector, byte defaultValue = 0)` |  |
| `MinOrDefault` | `static byte? MinOrDefault(this IEnumerable<byte?> @this, byte? defaultValue = null)` |  |
| `MinOrDefault` | `static decimal MinOrDefault(this IEnumerable<decimal> @this, decimal defaultValue = 0)` |  |
| `MinOrDefault` | `static decimal MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector, decimal defaultValue = 0)` |  |
| `MinOrDefault` | `static decimal? MinOrDefault(this IEnumerable<decimal?> @this, decimal? defaultValue = null)` |  |
| `MinOrDefault` | `static double MinOrDefault(this IEnumerable<double> @this, double defaultValue = 0)` |  |
| `MinOrDefault` | `static double MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector, double defaultValue = 0)` |  |
| `MinOrDefault` | `static double? MinOrDefault(this IEnumerable<double?> @this, double? defaultValue = null)` |  |
| `MinOrDefault` | `static float MinOrDefault(this IEnumerable<float> @this, float defaultValue = 0)` |  |
| `MinOrDefault` | `static float MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector, float defaultValue = 0)` |  |
| `MinOrDefault` | `static float? MinOrDefault(this IEnumerable<float?> @this, float? defaultValue = null)` |  |
| `MinOrDefault` | `static int MinOrDefault(this IEnumerable<int> @this, int defaultValue = 0)` |  |
| `MinOrDefault` | `static int MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, int> selector, int defaultValue = 0)` |  |
| `MinOrDefault` | `static int? MinOrDefault(this IEnumerable<int?> @this, int? defaultValue = null)` |  |
| `MinOrDefault` | `static long MinOrDefault(this IEnumerable<long> @this, long defaultValue = 0)` |  |
| `MinOrDefault` | `static long MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, long> selector, long defaultValue = 0)` |  |
| `MinOrDefault` | `static long? MinOrDefault(this IEnumerable<long?> @this, long? defaultValue = null)` |  |
| `MinOrDefault` | `static sbyte MinOrDefault(this IEnumerable<sbyte> @this, sbyte defaultValue = 0)` |  |
| `MinOrDefault` | `static sbyte MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, sbyte> selector, sbyte defaultValue = 0)` |  |
| `MinOrDefault` | `static sbyte? MinOrDefault(this IEnumerable<sbyte?> @this, sbyte? defaultValue = null)` |  |
| `MinOrDefault` | `static short MinOrDefault(this IEnumerable<short> @this, short defaultValue = 0)` |  |
| `MinOrDefault` | `static short MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, short> selector, short defaultValue = 0)` |  |
| `MinOrDefault` | `static short? MinOrDefault(this IEnumerable<short?> @this, short? defaultValue = null)` |  |
| `MinOrDefault` | `static uint MinOrDefault(this IEnumerable<uint> @this, uint defaultValue = 0)` |  |
| `MinOrDefault` | `static uint MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, uint> selector, uint defaultValue = 0)` |  |
| `MinOrDefault` | `static uint? MinOrDefault(this IEnumerable<uint?> @this, uint? defaultValue = null)` |  |
| `MinOrDefault` | `static ulong MinOrDefault(this IEnumerable<ulong> @this, ulong defaultValue = 0)` |  |
| `MinOrDefault` | `static ulong MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, ulong> selector, ulong defaultValue = 0)` |  |
| `MinOrDefault` | `static ulong? MinOrDefault(this IEnumerable<ulong?> @this, ulong? defaultValue = null)` |  |
| `MinOrDefault` | `static ushort MinOrDefault(this IEnumerable<ushort> @this, ushort defaultValue = 0)` |  |
| `MinOrDefault` | `static ushort MinOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem, ushort> selector, ushort defaultValue = 0)` |  |
| `MinOrDefault` | `static ushort? MinOrDefault(this IEnumerable<ushort?> @this, ushort? defaultValue = null)` |  |
| `Min` | `static byte Min(this IEnumerable<byte> @this)` |  |
| `Min` | `static byte Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, byte> selector)` |  |
| `Min` | `static sbyte Min(this IEnumerable<sbyte> @this)` |  |
| `Min` | `static sbyte Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, sbyte> selector)` |  |
| `Min` | `static short Min(this IEnumerable<short> @this)` |  |
| `Min` | `static short Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, short> selector)` |  |
| `Min` | `static uint Min(this IEnumerable<uint> @this)` |  |
| `Min` | `static uint Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, uint> selector)` |  |
| `Min` | `static ulong Min(this IEnumerable<ulong> @this)` |  |
| `Min` | `static ulong Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, ulong> selector)` |  |
| `Min` | `static ushort Min(this IEnumerable<ushort> @this)` |  |
| `Min` | `static ushort Min<TItem>(this IEnumerable<TItem> @this, Func<TItem, ushort> selector)` |  |
| `Mode` | `static T Mode<T>(this IEnumerable<T> @this)` |  |
| `OrderByDescending` | `static IEnumerable<TItem> OrderByDescending<TItem>(this IEnumerable<TItem> @this)` |  |
| `OrderBy` | `static IEnumerable<TItem> OrderBy<TItem>(this IEnumerable<TItem> @this)` |  |
| `ParallelForEach` | `static void ParallelForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem, int> action)` |  |
| `ParallelForEach` | `static void ParallelForEach<TItem>(this IEnumerable<TItem> @this, Action<TItem> action)` |  |
| `Prepend` | `static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> items)` |  |
| `Prepend` | `static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> @this, params TItem[] items)` |  |
| `Range` | `static byte Range(this IEnumerable<byte> @this)` |  |
| `Range` | `static byte Range(this IEnumerable<byte?> @this)` |  |
| `Range` | `static byte Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, byte> selector)` |  |
| `Range` | `static decimal Range(this IEnumerable<decimal> @this)` |  |
| `Range` | `static decimal Range(this IEnumerable<decimal?> @this)` |  |
| `Range` | `static decimal Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector)` |  |
| `Range` | `static double Range(this IEnumerable<double> @this)` |  |
| `Range` | `static double Range(this IEnumerable<double?> @this)` |  |
| `Range` | `static double Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector)` |  |
| `Range` | `static float Range(this IEnumerable<float> @this)` |  |
| `Range` | `static float Range(this IEnumerable<float?> @this)` |  |
| `Range` | `static float Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector)` |  |
| `Range` | `static int Range(this IEnumerable<int> @this)` |  |
| `Range` | `static int Range(this IEnumerable<int?> @this)` |  |
| `Range` | `static int Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, int> selector)` |  |
| `Range` | `static long Range(this IEnumerable<long> @this)` |  |
| `Range` | `static long Range(this IEnumerable<long?> @this)` |  |
| `Range` | `static long Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, long> selector)` |  |
| `Range` | `static sbyte Range(this IEnumerable<sbyte> @this)` |  |
| `Range` | `static sbyte Range(this IEnumerable<sbyte?> @this)` |  |
| `Range` | `static sbyte Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, sbyte> selector)` |  |
| `Range` | `static short Range(this IEnumerable<short> @this)` |  |
| `Range` | `static short Range(this IEnumerable<short?> @this)` |  |
| `Range` | `static short Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, short> selector)` |  |
| `Range` | `static uint Range(this IEnumerable<uint> @this)` |  |
| `Range` | `static uint Range(this IEnumerable<uint?> @this)` |  |
| `Range` | `static uint Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, uint> selector)` |  |
| `Range` | `static ulong Range(this IEnumerable<ulong> @this)` |  |
| `Range` | `static ulong Range(this IEnumerable<ulong?> @this)` |  |
| `Range` | `static ulong Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, ulong> selector)` |  |
| `Range` | `static ushort Range(this IEnumerable<ushort> @this)` |  |
| `Range` | `static ushort Range(this IEnumerable<ushort?> @this)` |  |
| `Range` | `static ushort Range<TItem>(this IEnumerable<TItem> @this, Func<TItem, ushort> selector)` |  |
| `SelectMany` | `static IEnumerable<TItem> SelectMany<TItem>(this IEnumerable<IEnumerable<TItem>> @this)` |  |
| `Shuffled` | `static IShuffledEnumerable<TItem> Shuffled<TItem>(this IEnumerable<TItem> @this, Random entropySource = null)` |  |
| `SingleOrDefault` | `static TItem SingleOrDefault<TItem>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TItem> defaultValueFactory)` |  |
| `SingleOrDefault` | `static TItem SingleOrDefault<TItem>(this IEnumerable<TItem> @this, Func<TItem> defaultValueFactory)` |  |
| `SingleOrNull` | `static TItem SingleOrNull<TItem>(this IEnumerable<TItem> @this, __ClassForcingTag<TItem> _ = null)` |  |
| `SingleOrNull` | `static TItem? SingleOrNull<TItem>(this IEnumerable<TItem> @this, __StructForcingTag<TItem> _ = null)` |  |
| `SkipUntil` | `static IEnumerable<TItem> SkipUntil<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate)` |  |
| `Split` | `static Tuple<IEnumerable<TItem>, IEnumerable<TItem>> Split<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate)` |  |
| `StdDev` | `static decimal StdDev(this IEnumerable<decimal> @this)` |  |
| `StdDev` | `static decimal StdDev(this IEnumerable<decimal?> @this)` |  |
| `StdDev` | `static decimal StdDev<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector)` |  |
| `StdDev` | `static double StdDev(this IEnumerable<double> @this)` |  |
| `StdDev` | `static double StdDev(this IEnumerable<double?> @this)` |  |
| `StdDev` | `static double StdDev<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector)` |  |
| `StdDev` | `static float StdDev(this IEnumerable<float> @this)` |  |
| `StdDev` | `static float StdDev(this IEnumerable<float?> @this)` |  |
| `StdDev` | `static float StdDev<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector)` |  |
| `Sum` | `static TimeSpan Sum(this IEnumerable<TimeSpan> @this)` |  |
| `Sum` | `static TimeSpan Sum<TIn>(this IEnumerable<TIn> @this, Func<TIn, TimeSpan> selector)` |  |
| `Sum` | `static uint Sum(this IEnumerable<uint> @this)` |  |
| `Sum` | `static uint Sum<TIn>(this IEnumerable<TIn> @this, Func<TIn, uint> selector)` |  |
| `Sum` | `static ulong Sum(this IEnumerable<ulong> @this)` |  |
| `Sum` | `static ulong Sum<TIn>(this IEnumerable<TIn> @this, Func<TIn, ulong> selector)` |  |
| `Sum` | `static ushort Sum(this IEnumerable<ushort> @this)` |  |
| `Sum` | `static ushort Sum<TIn>(this IEnumerable<TIn> @this, Func<TIn, ushort> selector)` |  |
| `TakeUntil` | `static IEnumerable<TItem> TakeUntil<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate)` |  |
| `ToBiDictionary` | `static BiDictionary<TKey, TValue> ToBiDictionary<TItem, TKey, TValue>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector)` |  |
| `ToCache` | `static CachedEnumeration<TItem> ToCache<TItem>(this IEnumerable<TItem> @this)` |  |
| `ToConcurrentDictionary` | `static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TItem, TKey, TValue>(this TItem[] @this, Func<TItem, TKey> keyGetter, Func<TItem, TValue> valueGetter, IEqualityComparer<TKey> equalityComparer = null)` |  |
| `ToHashSet` | `static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> @this, int initialCapacity)` |  |
| `ToHashSet` | `static HashSet<TResult> ToHashSet<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, IEqualityComparer<TResult> comparer = null)` |  |
| `ToNullIfEmpty` | `static IEnumerable<TItem> ToNullIfEmpty<TItem>(this IEnumerable<TItem> @this)` |  |
| `TryGetFirst` | `static bool TryGetFirst<TItem>(this IEnumerable<TItem> @this, out TItem result)` |  |
| `TryGetItem` | `static bool TryGetItem<TItem>(this IEnumerable<TItem> @this, int index, out TItem result)` |  |
| `TryGetLast` | `static bool TryGetLast<TItem>(this IEnumerable<TItem> @this, out TItem result)` |  |
| `TryGetMaxBy` | `static bool TryGetMaxBy<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, out TItem result)` |  |
| `TryGetMax` | `static bool TryGetMax<TItem>(this IEnumerable<TItem> @this, out TItem result)` |  |
| `TryGetMinBy` | `static bool TryGetMinBy<TItem, TResult>(this IEnumerable<TItem> @this, Func<TItem, TResult> selector, out TItem result)` |  |
| `TryGetMin` | `static bool TryGetMin<TItem>(this IEnumerable<TItem> @this, out TItem result)` |  |
| `TryGetSingle` | `static bool TryGetSingle<TItem>(this IEnumerable<TItem> @this, out TItem result)` |  |
| `TryGet` | `static bool TryGet<TItem, TResult>(this IEnumerable<TItem> @this, Func<IEnumerable<TItem>, TResult> selector, out TResult result)` |  |
| `Variance` | `static decimal Variance(this IEnumerable<decimal> @this)` |  |
| `Variance` | `static decimal Variance(this IEnumerable<decimal?> @this)` |  |
| `Variance` | `static decimal Variance<TItem>(this IEnumerable<TItem> @this, Func<TItem, decimal> selector)` |  |
| `Variance` | `static double Variance(this IEnumerable<double> @this)` |  |
| `Variance` | `static double Variance(this IEnumerable<double?> @this)` |  |
| `Variance` | `static double Variance<TItem>(this IEnumerable<TItem> @this, Func<TItem, double> selector)` |  |
| `Variance` | `static float Variance(this IEnumerable<float> @this)` |  |
| `Variance` | `static float Variance(this IEnumerable<float?> @this)` |  |
| `Variance` | `static float Variance<TItem>(this IEnumerable<TItem> @this, Func<TItem, float> selector)` |  |
| `WrapAsDisposableCollection` | `static IDisposableCollection<TItem> WrapAsDisposableCollection<TItem>(this IEnumerable<TItem> @this)` |  |

#### `EnumerableExtensions.ChangeType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Equal` | `0` |  |
| `Changed` | `1` |  |
| `Added` | `2` |  |
| `Removed` | `3` |  |

#### `EnumerableExtensions.IChangeSet<TItem>`

| Member | Signature | Summary |
| --- | --- | --- |
| `CurrentIndex` | `int CurrentIndex { get; }` |  |
| `Current` | `TItem Current { get; }` |  |
| `OtherIndex` | `int OtherIndex { get; }` |  |
| `Other` | `TItem Other { get; }` |  |
| `Type` | `ChangeType Type { get; }` |  |

#### `EnumerableExtensions.IDisposableCollection<T>`

Implements `IDisposable`, `IEnumerable`, `IEnumerable<T>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Item` | `T this[int i] { get; }` |  |

#### `EnumerableExtensions.IShuffledEnumerable<TItem>`

Implements `IEnumerable`, `IEnumerable<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `ToArray` | `TItem[] ToArray()` |  |
| `ToList` | `List<TItem> ToList()` |  |

#### `EnumeratorExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Next` | `static TValue Next<TValue>(this IEnumerator<TValue> @this)` |  |
| `Take` | `static IEnumerable<TValue> Take<TValue>(this IEnumerator<TValue> @this, int count)` |  |

#### `FastLookupTable<TItem>`

Implements `ICloneable`, `ICollection<TItem>`, `IEnumerable`, `IEnumerable<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FastLookupTable` | `FastLookupTable()` |  |
| `Count` | `int Count { get; }` |  |
| `IsReadOnly` | `bool IsReadOnly { get; }` |  |
| `Add` | `void Add(TItem item)` |  |
| `Clear` | `void Clear()` |  |
| `Clone` | `object Clone()` |  |
| `Contains` | `bool Contains(TItem item)` |  |
| `CopyTo` | `void CopyTo(TItem[] array, int arrayIndex)` |  |
| `GetEnumerator` | `IEnumerator<TItem> GetEnumerator()` |  |
| `Remove` | `bool Remove(TItem item)` |  |
| `TryAdd` | `bool TryAdd(TItem item)` |  |

#### `HashSetExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `CompareTo` | `static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this HashSet<TItem> @this, HashSet<TItem> other)` |  |
| `ContainsNot` | `static bool ContainsNot<TItem>(this HashSet<TItem> @this, TItem item)` |  |
| `TryAdd` | `static bool TryAdd<TItem>(this HashSet<TItem> @this, TItem value)` |  |
| `TryRemove` | `static bool TryRemove<TItem>(this HashSet<TItem> @this, TItem item)` |  |

#### `HashSetExtensions.ChangeType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Equal` | `0` |  |
| `Added` | `2` |  |
| `Removed` | `3` |  |

#### `HashSetExtensions.IChangeSet<TItem>`

| Member | Signature | Summary |
| --- | --- | --- |
| `Item` | `TItem Item { get; }` |  |
| `Type` | `ChangeType Type { get; }` |  |

#### `ICache<TInput, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput parameter] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TInput7, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2, TInput3 parameter3, TInput4 parameter4, TInput5 parameter5, TInput6 parameter6, TInput7 parameter7] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TInput6, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2, TInput3 parameter3, TInput4 parameter4, TInput5 parameter5, TInput6 parameter6] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TInput3, TInput4, TInput5, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2, TInput3 parameter3, TInput4 parameter4, TInput5 parameter5] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TInput3, TInput4, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2, TInput3 parameter3, TInput4 parameter4] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TInput3, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2, TInput3 parameter3] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `ICache<TInput1, TInput2, TValue>`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `Item` | `TValue this[TInput1 parameter1, TInput2 parameter2] { get; }` |  |
| `MemoryOverhead` | `long MemoryOverhead { get; }` |  |
| `MemorySize` | `long MemorySize { get; }` |  |
| `Policy` | `CacheReplacementPolicy Policy { get; }` |  |
| `Clear` | `void Clear()` |  |

#### `IQueue<T>`

| Member | Signature | Summary |
| --- | --- | --- |
| `Count` | `int Count { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Clear` | `void Clear()` |  |
| `Dequeue` | `T Dequeue()` |  |
| `Enqueue` | `void Enqueue(T item)` |  |
| `TryDequeue` | `bool TryDequeue(out T item)` |  |

#### `KeyValuePairExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Reverse` | `static KeyValuePair<TValue, TKey> Reverse<TKey, TValue>(this KeyValuePair<TKey, TValue> @this)` |  |
| `ToDictionary` | `static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> @this, IEqualityComparer<TKey> comparer = null)` |  |

#### `LinkedListExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Any` | `static bool Any<T>(this LinkedList<T> @this)` |  |
| `Dequeue` | `static T Dequeue<T>(this LinkedList<T> @this)` |  |
| `Enqueue` | `static void Enqueue<T>(this LinkedList<T> @this, T value)` |  |
| `Peek` | `static T Peek<T>(this LinkedList<T> @this)` |  |
| `Pop` | `static T Pop<T>(this LinkedList<T> @this)` |  |
| `Push` | `static void Push<T>(this LinkedList<T> @this, T value)` |  |
| `TryDequeue` | `static bool TryDequeue<T>(this LinkedList<T> @this, out T result)` |  |
| `TryPeek` | `static bool TryPeek<T>(this LinkedList<T> @this, out T result)` |  |
| `TryPop` | `static bool TryPop<T>(this LinkedList<T> @this, out T result)` |  |

#### `ListExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddIfNotNull` | `static void AddIfNotNull<TInput>(this IList<TInput> @this, TInput item)` |  |
| `AddRange` | `static void AddRange<TInput>(this IList<TInput> @this, IEnumerable<TInput> items)` |  |
| `AddRange` | `static void AddRange<TItem>(this List<TItem> @this, IEnumerable<TItem> items)` |  |
| `AsIReadOnlyList` | `static IReadOnlyList<T> AsIReadOnlyList<T>(this IList<T> @this)` |  |
| `AsIReadOnlyList` | `static IReadOnlyList<T> AsIReadOnlyList<T>(this List<T> @this)` |  |
| `BinarySearchIndex` | `static int BinarySearchIndex<T>(this IList<T> @this, T item)` |  |
| `BinarySearchIndex` | `static int BinarySearchIndex<T>(this IList<T> @this, T item, bool returnNextGreater)` |  |
| `BinarySearchIndex` | `static int BinarySearchIndex<T>(this IList<T> @this, T item, int startAt, int count)` |  |
| `BinarySearchIndex` | `static int BinarySearchIndex<T>(this IList<T> @this, T item, int startAt, int count, bool returnNextGreater)` |  |
| `ConvertAll` | `static TOutput[] ConvertAll<TInput, TOutput>(this IList<TInput> @this, Converter<TInput, TOutput> converter)` |  |
| `ForEach` | `static void ForEach<TInput>(this IList<TInput> @this, Action<TInput> action)` |  |
| `KeepFirst` | `static void KeepFirst<TInput>(this IList<TInput> @this, int count)` |  |
| `KeepLast` | `static void KeepLast<TInput>(this IList<TInput> @this, int count)` |  |
| `Permutate` | `static IEnumerable<T[]> Permutate<T>(this IList<T> @this, bool separateArrays = false)` |  |
| `Permutate` | `static IEnumerable<T[]> Permutate<T>(this IList<T> @this, int length, bool separateArrays = false)` |  |
| `RemoveAll` | `static void RemoveAll<TItem>(this IList<TItem> @this, IEnumerable<TItem> items)` |  |
| `RemoveEvery` | `static void RemoveEvery<TItem>(this IList<TItem> @this, TItem item)` |  |
| `RemoveFirst` | `static void RemoveFirst<TInput>(this IList<TInput> @this, int count)` |  |
| `RemoveLast` | `static void RemoveLast<TInput>(this IList<TInput> @this, int count)` |  |
| `RemoveRange` | `static void RemoveRange<TInput>(this IList<TInput> @this, int start, int count)` |  |
| `Shuffle` | `static void Shuffle<T>(this IList<T> @this, Random entropySource = null)` |  |
| `Splice` | `static T[] Splice<T>(this IList<T> @this, int start, int count)` |  |
| `Swap` | `static void Swap<TItem>(this IList<TItem> @this, int firstElementIndex, int secondElementIndex)` |  |
| `TrySetFirst` | `static bool TrySetFirst<T>(this IList<T> @this, T value)` |  |
| `TrySetFirst` | `static bool TrySetFirst<T>(this List<T> @this, T value)` |  |
| `TrySetItem` | `static bool TrySetItem<T>(this IList<T> @this, int index, T value)` |  |
| `TrySetItem` | `static bool TrySetItem<T>(this List<T> @this, int index, T value)` |  |
| `TrySetLast` | `static bool TrySetLast<T>(this IList<T> @this, T value)` |  |
| `TrySetLast` | `static bool TrySetLast<T>(this List<T> @this, T value)` |  |

#### `QueueExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange<T>(this Queue<T> @this, IEnumerable<T> items)` |  |
| `Add` | `static void Add<T>(this Queue<T> @this, T item)` |  |
| `Fetch` | `static T Fetch<T>(this Queue<T> @this)` |  |
| `PullAll` | `static T[] PullAll<T>(this Queue<T> @this)` |  |
| `PullTo` | `static Span<T> PullTo<T>(this Queue<T> @this, Span<T> target)` |  |
| `PullTo` | `static int PullTo<T>(this Queue<T> @this, T[] target)` |  |
| `PullTo` | `static int PullTo<T>(this Queue<T> @this, T[] target, int offset)` |  |
| `PullTo` | `static int PullTo<T>(this Queue<T> @this, T[] target, int offset, int maxCount)` |  |
| `Pull` | `static T[] Pull<T>(this Queue<T> @this, int maxCount)` |  |

#### `StackExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange<TItem>(this Stack<TItem> @this, IEnumerable<TItem> items)` |  |
| `Add` | `static void Add<TItem>(this Stack<TItem> @this, TItem item)` |  |
| `Exchange` | `static TItem Exchange<TItem>(this Stack<TItem> @this, TItem item)` |  |
| `Fetch` | `static TItem Fetch<TItem>(this Stack<TItem> @this)` |  |
| `Invert` | `static void Invert<TItem>(this Stack<TItem> @this)` |  |
| `PullAll` | `static T[] PullAll<T>(this Stack<T> @this)` |  |
| `PullTo` | `static Span<T> PullTo<T>(this Stack<T> @this, Span<T> target)` |  |
| `PullTo` | `static int PullTo<T>(this Stack<T> @this, T[] target)` |  |
| `PullTo` | `static int PullTo<T>(this Stack<T> @this, T[] target, int offset)` |  |
| `PullTo` | `static int PullTo<T>(this Stack<T> @this, T[] target, int offset, int maxCount)` |  |
| `Pull` | `static T[] Pull<T>(this Stack<T> @this, int maxCount)` |  |

### Namespace `System.Collections.ObjectModel`

[`CollectionExtensions`](#collectionextensions)

#### `CollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange<TItem>(this Collection<TItem> @this, IEnumerable<TItem> items)` |  |

### Namespace `System.Collections.Specialized`

[`StringCollectionExtensions`](#stringcollectionextensions) · [`StringDictionaryExtensions`](#stringdictionaryextensions)

#### `StringCollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ToArray` | `static string[] ToArray(this StringCollection @this)` |  |

#### `StringDictionaryExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddOrUpdate` | `static void AddOrUpdate(this StringDictionary @this, string key, string value)` |  |

### Namespace `System.ComponentModel`

[`BindingListExtensions`](#bindinglistextensions) · [`BindingListView<TItem>`](#bindinglistviewtitem) · [`DefaultValueAttributeExtensions`](#defaultvalueattributeextensions) · [`EnumDisplayNameAttribute`](#enumdisplaynameattribute) · [`EnumerableExtensions`](#enumerableextensions) · [`MaxValueAttribute`](#maxvalueattribute) · [`MinValueAttribute`](#minvalueattribute) · [`PropertyChangedExtensions`](#propertychangedextensions) · [`PropertyChangingExtensions`](#propertychangingextensions) · [`SortableBindingList<TValue>`](#sortablebindinglisttvalue) · [`SynchronizeInvokeExtensions`](#synchronizeinvokeextensions)

#### `BindingListExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items)` |  |
| `Any` | `static bool Any<TItem>(this BindingList<TItem> @this)` |  |
| `MoveRelative` | `static void MoveRelative<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items, int delta)` |  |
| `MoveToBack` | `static void MoveToBack<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items)` |  |
| `MoveToFront` | `static void MoveToFront<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items)` |  |
| `Overhaul` | `static void Overhaul<TItem>(this BindingList<TItem> @this, Action<BindingList<TItem>> action)` |  |
| `RefreshAll` | `static void RefreshAll<T>(this BindingList<T> @this, IEnumerable<T> items, Func<T, string> keyGetter, Func<T, T, T> itemUpdateMethod)` |  |
| `RemoveWhere` | `static int RemoveWhere<TItem>(this BindingList<TItem> @this, Predicate<TItem> selector)` |  |
| `ReplaceAll` | `static void ReplaceAll<T>(this BindingList<T> @this, IEnumerable<T> items)` |  |
| `ToArray` | `static TItem[] ToArray<TItem>(this BindingList<TItem> @this)` |  |

#### `BindingListView<TItem>`

Inherits `SortableBindingList<TItem>`. Implements `IBindingList`, `ICancelAddNew`, `ICollection`, `ICollection<TItem>`, `IEnumerable`, `IEnumerable<TItem>`, `IList`, `IList<TItem>`, `INotifyCollectionChanged`, `IRaiseItemChangedEvents`, `IReadOnlyCollection<TItem>`, `IReadOnlyList<TItem>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `BindingListView` | `BindingListView(BindingList<TItem> baseList)` |  |
| `DataSource` | `BindingList<TItem> DataSource { get; }` |  |
| `FilterPredicate` | `Predicate<TItem> FilterPredicate { get; set; }` |  |
| `IsFiltering` | `bool IsFiltering { get; set; }` |  |
| `AddRange` | `void AddRange(IEnumerable<TItem> items)` |  |
| `Add` | `void Add(TItem item)` |  |

#### `DefaultValueAttributeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `SetPropertiesToDefaultValues` | `static void SetPropertiesToDefaultValues<TType>(this TType @this, bool alsoNonPublic = false, bool flattenHierarchies = true)` |  |

#### `EnumDisplayNameAttribute`

Inherits `DisplayNameAttribute`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EnumDisplayNameAttribute` | `EnumDisplayNameAttribute(string displayName)` |  |
| `DisplayName` | `override string DisplayName { get; }` |  |
| `GetDisplayNameOrDefault` | `static string GetDisplayNameOrDefault(Type type, object value)` |  |
| `GetDisplayNameOrDefault` | `static string GetDisplayNameOrDefault<TEnum>(TEnum value)` |  |
| `GetDisplayName` | `static string GetDisplayName(Type type, object value)` |  |
| `GetDisplayName` | `static string GetDisplayName<TEnum>(TEnum value)` |  |

#### `EnumerableExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ToSortableBindingList` | `static SortableBindingList<TItem> ToSortableBindingList<TItem>(this IEnumerable<TItem> @this)` |  |

#### `MaxValueAttribute`

Inherits `Attribute`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MaxValueAttribute` | `MaxValueAttribute(decimal value)` |  |
| `MaxValueAttribute` | `MaxValueAttribute(int value)` |  |
| `Value` | `decimal Value { get; }` |  |

#### `MinValueAttribute`

Inherits `Attribute`.

| Member | Signature | Summary |
| --- | --- | --- |
| `MinValueAttribute` | `MinValueAttribute(decimal value)` |  |
| `MinValueAttribute` | `MinValueAttribute(int value)` |  |
| `Value` | `decimal Value { get; }` |  |

#### `PropertyChangedExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `SynchronizationContext` | `static ISynchronizeInvoke SynchronizationContext { get; set; }` |  |
| `SafeInvoke` | `static void SafeInvoke(this PropertyChangedEventHandler @this, object sender, PropertyChangedEventArgs e)` |  |
| `SetProperty` | `static bool SetProperty<This, T>(this This @this, Action<string> onPropertyChanged, ref T backingField, T value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref bool backingField, bool value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref byte backingField, byte value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref char backingField, char value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref decimal backingField, decimal value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref double backingField, double value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref float backingField, float value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref int backingField, int value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref long backingField, long value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref sbyte backingField, sbyte value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref short backingField, short value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref string backingField, string value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref uint backingField, uint value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref ulong backingField, ulong value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanged, ref ushort backingField, ushort value, string propertyName = null)` |  |

#### `PropertyChangingExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `SetProperty` | `static bool SetProperty<This, T>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref T backingField, T value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref bool backingField, bool value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref byte backingField, byte value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref char backingField, char value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref decimal backingField, decimal value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref double backingField, double value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref float backingField, float value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref int backingField, int value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref long backingField, long value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref sbyte backingField, sbyte value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref short backingField, short value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref string backingField, string value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref uint backingField, uint value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref ulong backingField, ulong value, string propertyName = null)` |  |
| `SetProperty` | `static bool SetProperty<This>(this This @this, Action<string> onPropertyChanging, Action<string> onPropertyChanged, ref ushort backingField, ushort value, string propertyName = null)` |  |

#### `SortableBindingList<TValue>`

Inherits `BindingList<TValue>`. Implements `IBindingList`, `ICancelAddNew`, `ICollection`, `ICollection<TValue>`, `IEnumerable`, `IEnumerable<TValue>`, `IList`, `IList<TValue>`, `INotifyCollectionChanged`, `IRaiseItemChangedEvents`, `IReadOnlyCollection<TValue>`, `IReadOnlyList<TValue>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `SortableBindingList` | `SortableBindingList()` |  |
| `SortableBindingList` | `SortableBindingList(IEnumerable<TValue> enumerable)` |  |
| `SortableBindingList` | `SortableBindingList(IList<TValue> list)` |  |
| `IsAutomaticallySorted` | `bool IsAutomaticallySorted { get; set; }` |  |
| `IsSortedCore` | `protected override bool IsSortedCore { get; }` |  |
| `SortDirectionCore` | `protected override ListSortDirection SortDirectionCore { get; }` |  |
| `SortPropertyCore` | `protected override PropertyDescriptor SortPropertyCore { get; }` |  |
| `SupportsSortingCore` | `protected override bool SupportsSortingCore { get; }` |  |
| `AddRangeIfNotExists` | `void AddRangeIfNotExists(IEnumerable<TValue> items)` |  |
| `AddRange` | `void AddRange(IEnumerable<TValue> items)` |  |
| `ApplySortCore` | `protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)` |  |
| `OnCollectionChanged` | `protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)` |  |
| `OnListChanged` | `protected override void OnListChanged(ListChangedEventArgs e)` |  |
| `RemoveItem` | `protected override void RemoveItem(int index)` |  |
| `RemoveRange` | `void RemoveRange(IEnumerable<TValue> items)` |  |
| `RemoveSortCore` | `protected override void RemoveSortCore()` |  |
| `SetItem` | `protected override void SetItem(int index, TValue item)` |  |
| `Sort` | `void Sort(string propertyName, ListSortDirection direction)` |  |
| `CollectionChanged` | `event NotifyCollectionChangedEventHandler CollectionChanged` |  |

#### `SynchronizeInvokeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `SafeInvoke` | `static bool SafeInvoke<T>(this T @this, Action<T> call, bool async = false)` |  |

### Namespace `System.Data`

[`DataRecordExtensions`](#datarecordextensions) · [`DataRowExtensions`](#datarowextensions)

#### `DataRecordExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetValueOrDefault` | `static TType GetValueOrDefault<TType>(this IDataRecord @this, string fieldName)` |  |
| `GetValueOrDefault` | `static TType GetValueOrDefault<TType>(this IDataRecord @this, string fieldName, TType defaultValue)` |  |

#### `DataRowExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ToDictionary` | `static Dictionary<string, object> ToDictionary(this DataRow @this)` |  |

### Namespace `System.Diagnostics`

[`ProcessExtensions`](#processextensions) · [`ProcessStartInfoExtensions`](#processstartinfoextensions) · [`ProcessStartInfoExtensions.ConsoleOutputHandler`](#processstartinfoextensionsconsoleoutputhandler) · [`ProcessStartInfoExtensions.IConsoleResult`](#processstartinfoextensionsiconsoleresult) · [`ProcessStartInfoExtensions.ICurrentConsoleOutput`](#processstartinfoextensionsicurrentconsoleoutput) · [`ProcessStartInfoExtensions.IRedirectedRunAsyncResult`](#processstartinfoextensionsiredirectedrunasyncresult) · [`StopwatchExtensions`](#stopwatchextensions)

#### `ProcessExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AllChildren` | `static IEnumerable<Process> AllChildren(this Process @this)` |  |
| `Children` | `static IEnumerable<Process> Children(this Process @this)` |  |
| `GetParentProcessOrNull` | `static Process GetParentProcessOrNull(int processId)` |  |
| `GetParentProcessOrNull` | `static Process GetParentProcessOrNull(nint handle)` |  |
| `GetParentProcessOrNull` | `static Process GetParentProcessOrNull(this Process @this)` |  |
| `GetParentProcess` | `static Process GetParentProcess()` |  |
| `GetParentProcess` | `static Process GetParentProcess(int processId)` |  |
| `GetParentProcess` | `static Process GetParentProcess(nint handle)` |  |
| `GetParentProcess` | `static Process GetParentProcess(this Process @this)` |  |
| `Parent` | `static Process Parent(this Process @this)` |  |
| `Parents` | `static IEnumerable<Process> Parents(this Process @this)` |  |

#### `ProcessStartInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `BeginRedirectedRun` | `static IRedirectedRunAsyncResult BeginRedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null, AsyncCallback callback = null, object state = null)` |  |
| `EndRedirectedRun` | `static IConsoleResult EndRedirectedRun(this ProcessStartInfo @this, IAsyncResult asyncResult)` |  |
| `File` | `static FileInfo File(this ProcessStartInfo @this)` |  |
| `RedirectedRunAsync` | `static Task<IConsoleResult> RedirectedRunAsync(this ProcessStartInfo @this, CancellationToken cancellation, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null)` |  |
| `RedirectedRunAsync` | `static Task<IConsoleResult> RedirectedRunAsync(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null)` |  |
| `RedirectedRun` | `static IConsoleResult RedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null)` |  |

#### `ProcessStartInfoExtensions.ConsoleOutputHandler`

| Member | Signature | Summary |
| --- | --- | --- |
| `ConsoleOutputHandler` | `void ProcessStartInfoExtensions.ConsoleOutputHandler(ICurrentConsoleOutput output)` |  |

#### `ProcessStartInfoExtensions.IConsoleResult`

| Member | Signature | Summary |
| --- | --- | --- |
| `ExitCode` | `int ExitCode { get; }` |  |
| `StandardError` | `string StandardError { get; }` |  |
| `StandardOutput` | `string StandardOutput { get; }` |  |

#### `ProcessStartInfoExtensions.ICurrentConsoleOutput`

| Member | Signature | Summary |
| --- | --- | --- |
| `CurrentLine` | `string CurrentLine { get; }` |  |
| `TotalText` | `string TotalText { get; }` |  |

#### `ProcessStartInfoExtensions.IRedirectedRunAsyncResult`

Implements `IAsyncResult`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Process` | `Process Process { get; }` |  |
| `Result` | `IConsoleResult Result { get; }` |  |

#### `StopwatchExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetElapsedMilliseconds` | `static double GetElapsedMilliseconds(this Stopwatch This)` |  |

### Namespace `System.Globalization`

[`CultureInfoExtensions`](#cultureinfoextensions)

#### `CultureInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetISOCurrencySymbol` | `static string GetISOCurrencySymbol(this CultureInfo @this)` |  |
| `GetRegionInfo` | `static RegionInfo GetRegionInfo(this CultureInfo @this)` |  |

### Namespace `System.IO`

[`BinaryReaderExtensions`](#binaryreaderextensions) · [`BufferedStreamEx`](#bufferedstreamex) · [`ConflictResolutionMode`](#conflictresolutionmode) · [`DirectoryInfoExtensions`](#directoryinfoextensions) · [`DirectoryInfoExtensions.RecursionMode`](#directoryinfoextensionsrecursionmode) · [`DriveInfoExtensions`](#driveinfoextensions) · [`DriveTypeExtensions`](#drivetypeextensions) · [`FastFileOperations`](#fastfileoperations) · [`FastFileOperations.BinaryFileComparer`](#fastfileoperationsbinaryfilecomparer) · [`FastFileOperations.ContinuationType`](#fastfileoperationscontinuationtype) · [`FastFileOperations.FileCreationTimeComparer`](#fastfileoperationsfilecreationtimecomparer) · [`FastFileOperations.FileLastWriteTimeComparer`](#fastfileoperationsfilelastwritetimecomparer) · [`FastFileOperations.FileLengthComparer`](#fastfileoperationsfilelengthcomparer) · [`FastFileOperations.FileReportCallback`](#fastfileoperationsfilereportcallback) · [`FastFileOperations.FileSimpleAttributesComparer`](#fastfileoperationsfilesimpleattributescomparer) · [`FastFileOperations.IDirectoryOperation`](#fastfileoperationsidirectoryoperation) · [`FastFileOperations.IDirectoryReport`](#fastfileoperationsidirectoryreport) · [`FastFileOperations.IFileComparer`](#fastfileoperationsifilecomparer) · [`FastFileOperations.IFileOperation`](#fastfileoperationsifileoperation) · [`FastFileOperations.IFileReport`](#fastfileoperationsifilereport) · [`FastFileOperations.IFileSystemOperation`](#fastfileoperationsifilesystemoperation) · [`FastFileOperations.IFileSystemReport`](#fastfileoperationsifilesystemreport) · [`FastFileOperations.ReportType`](#fastfileoperationsreporttype) · [`FileConflictException`](#fileconflictexception) · [`FileInfoExtensions`](#fileinfoextensions) · [`FileInfoExtensions.IFileInProgress`](#fileinfoextensionsifileinprogress) · [`FileSystemInfoExtensions`](#filesysteminfoextensions) · [`LinkExtensions`](#linkextensions) · [`PathExtensions`](#pathextensions) · [`PathExtensions.ITemporaryDirectoryToken`](#pathextensionsitemporarydirectorytoken) · [`PathExtensions.ITemporaryFileToken`](#pathextensionsitemporaryfiletoken) · [`PathExtensions.NetworkPath`](#pathextensionsnetworkpath) · [`StreamExtensions`](#streamextensions) · [`TextReaderExtensions`](#textreaderextensions) · [`VolumeExtensions`](#volumeextensions) · [`VolumeExtensions.Volume`](#volumeextensionsvolume)

#### `BinaryReaderExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ReadAllBytes` | `static byte[] ReadAllBytes(this BinaryReader @this, uint bufferSize = 65536)` |  |

#### `BufferedStreamEx`

Inherits `Stream`. Implements `IAsyncDisposable`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `BufferedStreamEx` | `BufferedStreamEx(Stream underlyingStream, int bufferSize = 8192, bool dontDisposeUnderlyingStream = false)` |  |
| `CanRead` | `override bool CanRead { get; }` |  |
| `CanSeek` | `override bool CanSeek { get; }` |  |
| `CanWrite` | `override bool CanWrite { get; }` |  |
| `Length` | `override long Length { get; }` |  |
| `Position` | `override long Position { get; set; }` |  |
| `Dispose` | `protected override void Dispose(bool disposing)` |  |
| `Flush` | `override void Flush()` |  |
| `ReadByte` | `override int ReadByte()` |  |
| `Read` | `override int Read(Span<byte> buffer)` |  |
| `Read` | `override int Read(byte[] dest, int offset, int count)` |  |
| `Seek` | `override long Seek(long offset, SeekOrigin origin)` |  |
| `SetLength` | `override void SetLength(long value)` |  |
| `WriteByte` | `override void WriteByte(byte value)` |  |
| `Write` | `override void Write(ReadOnlySpan<byte> buffer)` |  |
| `Write` | `override void Write(byte[] src, int offset, int count)` |  |

#### `ConflictResolutionMode`

| Value | Numeric | Summary |
| --- | --- | --- |
| `None` | `0` |  |
| `LockWithReadShare` | `1` |  |
| `LockExclusive` | `2` |  |
| `CheckLastWriteTimeAndThrow` | `3` |  |
| `CheckLastWriteTimeAndIgnoreUpdate` | `4` |  |
| `CheckChecksumAndThrow` | `5` |  |
| `CheckChecksumAndIgnoreUpdate` | `6` |  |

#### `DirectoryInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Clear` | `static void Clear(this DirectoryInfo @this)` |  |
| `ContainsDirectory` | `static bool ContainsDirectory(this DirectoryInfo This, string directoryName, SearchOption option = 0)` |  |
| `ContainsFile` | `static bool ContainsFile(this DirectoryInfo This, string fileName, SearchOption option = 0)` |  |
| `CopyTo` | `static void CopyTo(this DirectoryInfo @this, DirectoryInfo target)` |  |
| `CreateDirectory` | `static void CreateDirectory(this DirectoryInfo @this)` |  |
| `Directory` | `static DirectoryInfo Directory(this DirectoryInfo @this, bool ignoreCase, params string[] subdirectories)` |  |
| `Directory` | `static DirectoryInfo Directory(this DirectoryInfo @this, params string[] subdirectories)` |  |
| `Directory` | `static DirectoryInfo Directory(this DirectoryInfo @this, string subdirectory)` |  |
| `EnumerateFileSystemInfos` | `static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo @this, RecursionMode mode, Func<DirectoryInfo, bool> recursionFilter = null)` |  |
| `ExistsAndHasFiles` | `static bool ExistsAndHasFiles(this DirectoryInfo @this, string fileMask = "*.*")` |  |
| `File` | `static FileInfo File(this DirectoryInfo @this, bool ignoreCase, params string[] filePath)` |  |
| `File` | `static FileInfo File(this DirectoryInfo @this, params string[] filePath)` |  |
| `File` | `static FileInfo File(this DirectoryInfo @this, string filePath)` |  |
| `GetDirectories` | `static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo @this, SearchOption searchOption)` |  |
| `GetOrAddDirectory` | `static DirectoryInfo GetOrAddDirectory(this DirectoryInfo @this, string name)` |  |
| `GetRealPath` | `static DirectoryInfo GetRealPath(this DirectoryInfo @this)` |  |
| `GetSize` | `static long GetSize(this DirectoryInfo @this)` |  |
| `GetTempFile` | `static FileInfo GetTempFile(this DirectoryInfo @this, string extension = null)` |  |
| `HasDirectory` | `static bool HasDirectory(this DirectoryInfo This, string searchPattern, SearchOption searchOption = 0)` |  |
| `HasFile` | `static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = 0)` |  |
| `NotExists` | `static bool NotExists(this DirectoryInfo This)` |  |
| `RenameTo` | `static void RenameTo(this DirectoryInfo @this, string newName)` |  |
| `SafelyEnumerateDirectories` | `static IEnumerable<DirectoryInfo> SafelyEnumerateDirectories(this DirectoryInfo @this)` |  |
| `SafelyEnumerateFiles` | `static IEnumerable<FileInfo> SafelyEnumerateFiles(this DirectoryInfo @this)` |  |
| `TryCreateFile` | `static FileInfo TryCreateFile(this DirectoryInfo @this, string fileName, FileAttributes attributes = 128)` |  |
| `TryCreate` | `static bool TryCreate(this DirectoryInfo @this)` |  |
| `TryCreate` | `static bool TryCreate(this DirectoryInfo @this, bool recursive)` |  |
| `TryDelete` | `static bool TryDelete(this DirectoryInfo @this, bool recursive = false)` |  |
| `TrySetAttributes` | `static bool TrySetAttributes(this DirectoryInfo @this, FileAttributes attributes)` |  |
| `TrySetCreationTimeUtc` | `static bool TrySetCreationTimeUtc(this DirectoryInfo @this, DateTime creationTimeUtc)` |  |
| `TrySetLastWriteTimeUtc` | `static bool TrySetLastWriteTimeUtc(this DirectoryInfo @this, DateTime lastWriteTimeUtc)` |  |

#### `DirectoryInfoExtensions.RecursionMode`

| Value | Numeric | Summary |
| --- | --- | --- |
| `ToplevelOnly` | `0` |  |
| `ShortestPathFirst` | `1` |  |
| `DeepestPathFirst` | `2` |  |

#### `DriveInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ClusterSize` | `static long ClusterSize(this DriveInfo @this)` |  |
| `Exists` | `static bool Exists(this DriveInfo @this)` |  |
| `PercentFree` | `static double PercentFree(this DriveInfo @this)` |  |
| `PercentUsed` | `static double PercentUsed(this DriveInfo @this)` |  |
| `SectorSize` | `static long SectorSize(this DriveInfo @this)` |  |

#### `DriveTypeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IsFixed` | `static bool IsFixed(this DriveType @this)` |  |
| `IsNetwork` | `static bool IsNetwork(this DriveType @this)` |  |
| `IsNotFixed` | `static bool IsNotFixed(this DriveType @this)` |  |
| `IsNotNetwork` | `static bool IsNotNetwork(this DriveType @this)` |  |
| `IsNotRemovable` | `static bool IsNotRemovable(this DriveType @this)` |  |
| `IsRemovable` | `static bool IsRemovable(this DriveType @this)` |  |

#### `FastFileOperations`

| Member | Signature | Summary |
| --- | --- | --- |
| `CopyToAsync` | `static IDirectoryReport CopyToAsync(this DirectoryInfo @this, DirectoryInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, bool allowIntegrate = false, bool synchronizeTarget = false, Func<FileSystemInfo, bool> predicate = null, Action<IDirectoryReport> callback = null, int crawlerThreads = -1, int streamThreads = -1)` |  |
| `CopyToAsync` | `static IFileReport CopyToAsync(this FileInfo @this, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = -1)` |  |
| `CopyTo` | `static void CopyTo(this DirectoryInfo @this, DirectoryInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, bool allowIntegrate = false, bool synchronizeTarget = false, Func<FileSystemInfo, bool> predicate = null, Action<IDirectoryReport> callback = null, int crawlerThreads = -1, int streamThreads = -1)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, FileInfo target, bool overwrite = false, bool allowHardLinks = false, bool dontResolveSymbolicLinks = false, FileReportCallback callback = null, int allowedStreams = 1, int bufferSize = 524288)` |  |

#### `FastFileOperations.BinaryFileComparer`

Implements `IEqualityComparer<FileInfo>`, `IFileComparer`.

| Member | Signature | Summary |
| --- | --- | --- |
| `BinaryFileComparer` | `BinaryFileComparer()` |  |
| `Equals` | `bool Equals(FileInfo x, FileInfo y)` |  |
| `GetHashCode` | `int GetHashCode(FileInfo obj)` |  |

#### `FastFileOperations.ContinuationType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Proceed` | `0` |  |
| `RetryChunk` | `1` |  |
| `RetryStream` | `2` |  |
| `QueueStream` | `3` |  |
| `AbortOperation` | `4` |  |

#### `FastFileOperations.FileCreationTimeComparer`

Implements `IEqualityComparer<FileInfo>`, `IFileComparer`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FileCreationTimeComparer` | `FileCreationTimeComparer()` |  |
| `Equals` | `bool Equals(FileInfo x, FileInfo y)` |  |
| `GetHashCode` | `int GetHashCode(FileInfo obj)` |  |

#### `FastFileOperations.FileLastWriteTimeComparer`

Implements `IEqualityComparer<FileInfo>`, `IFileComparer`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FileLastWriteTimeComparer` | `FileLastWriteTimeComparer()` |  |
| `Equals` | `bool Equals(FileInfo x, FileInfo y)` |  |
| `GetHashCode` | `int GetHashCode(FileInfo obj)` |  |

#### `FastFileOperations.FileLengthComparer`

Implements `IEqualityComparer<FileInfo>`, `IFileComparer`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FileLengthComparer` | `FileLengthComparer()` |  |
| `Equals` | `bool Equals(FileInfo x, FileInfo y)` |  |
| `GetHashCode` | `int GetHashCode(FileInfo obj)` |  |

#### `FastFileOperations.FileReportCallback`

| Member | Signature | Summary |
| --- | --- | --- |
| `FileReportCallback` | `void FastFileOperations.FileReportCallback(IFileReport report)` |  |

#### `FastFileOperations.FileSimpleAttributesComparer`

Implements `IEqualityComparer<FileInfo>`, `IFileComparer`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FileSimpleAttributesComparer` | `FileSimpleAttributesComparer()` |  |
| `Equals` | `bool Equals(FileInfo x, FileInfo y)` |  |
| `GetHashCode` | `int GetHashCode(FileInfo obj)` |  |

#### `FastFileOperations.IDirectoryOperation`

Implements `IFileSystemOperation`.

| Member | Signature | Summary |
| --- | --- | --- |
| `CrawlerCount` | `int CrawlerCount { get; set; }` |  |

#### `FastFileOperations.IDirectoryReport`

Implements `IFileSystemReport`.

_No public or protected members._

#### `FastFileOperations.IFileComparer`

Implements `IEqualityComparer<FileInfo>`.

_No public or protected members._

#### `FastFileOperations.IFileOperation`

Implements `IFileSystemOperation`.

_No public or protected members._

#### `FastFileOperations.IFileReport`

Implements `IFileSystemReport`.

_No public or protected members._

#### `FastFileOperations.IFileSystemOperation`

| Member | Signature | Summary |
| --- | --- | --- |
| `BytesRead` | `long BytesRead { get; }` |  |
| `BytesToTransfer` | `long BytesToTransfer { get; }` |  |
| `BytesTransferred` | `long BytesTransferred { get; }` |  |
| `Exception` | `Exception Exception { get; }` |  |
| `IsDone` | `bool IsDone { get; }` |  |
| `Source` | `FileSystemInfo Source { get; }` |  |
| `StreamCount` | `int StreamCount { get; set; }` |  |
| `Target` | `FileSystemInfo Target { get; }` |  |
| `ThrewException` | `bool ThrewException { get; }` |  |
| `TotalSize` | `long TotalSize { get; }` |  |
| `Abort` | `void Abort()` |  |
| `WaitTillDone` | `bool WaitTillDone(TimeSpan timeout)` |  |
| `WaitTillDone` | `void WaitTillDone()` |  |

#### `FastFileOperations.IFileSystemReport`

| Member | Signature | Summary |
| --- | --- | --- |
| `ChunkOffset` | `long ChunkOffset { get; }` |  |
| `ChunkSize` | `long ChunkSize { get; }` |  |
| `ContinuationType` | `ContinuationType ContinuationType { get; set; }` |  |
| `Operation` | `IFileSystemOperation Operation { get; }` |  |
| `ReportType` | `ReportType ReportType { get; }` |  |
| `Source` | `FileSystemInfo Source { get; }` |  |
| `StreamIndex` | `int StreamIndex { get; }` |  |
| `StreamOffset` | `long StreamOffset { get; }` |  |
| `StreamSize` | `long StreamSize { get; }` |  |
| `Target` | `FileSystemInfo Target { get; }` |  |

#### `FastFileOperations.ReportType`

| Value | Numeric | Summary |
| --- | --- | --- |
| `StartOperation` | `0` |  |
| `FinishedOperation` | `1` |  |
| `AbortedOperation` | `2` |  |
| `CreatedLink` | `3` |  |
| `StartRead` | `4` |  |
| `StartWrite` | `5` |  |
| `FinishedRead` | `6` |  |
| `FinishedWrite` | `7` |  |

#### `FileConflictException`

Inherits `IOException`. Implements `ISerializable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FileConflictException` | `FileConflictException(string message)` |  |
| `FileConflictException` | `FileConflictException(string message, Exception innerException)` |  |

#### `FileInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AppendAllLines` | `static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents)` |  |
| `AppendAllLines` | `static void AppendAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding)` |  |
| `AppendAllText` | `static void AppendAllText(this FileInfo @this, string contents)` |  |
| `AppendAllText` | `static void AppendAllText(this FileInfo @this, string contents, Encoding encoding)` |  |
| `AppendLine` | `static void AppendLine(this FileInfo @this, string contents)` |  |
| `AppendLine` | `static void AppendLine(this FileInfo @this, string contents, Encoding encoding)` |  |
| `ChangeExtension` | `static void ChangeExtension(this FileInfo @this, string newExtension)` |  |
| `ComputeHash` | `static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider)` |  |
| `ComputeHash` | `static byte[] ComputeHash(this FileInfo @this, HashAlgorithm provider, int blockSize)` |  |
| `ComputeHash` | `static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this)` |  |
| `ComputeHash` | `static byte[] ComputeHash<THashAlgorithm>(this FileInfo @this, int blockSize)` |  |
| `ComputeMD5Hash` | `static byte[] ComputeMD5Hash(this FileInfo @this)` |  |
| `ComputeSHA1Hash` | `static byte[] ComputeSHA1Hash(this FileInfo @this)` |  |
| `ComputeSHA256Hash` | `static byte[] ComputeSHA256Hash(this FileInfo @this)` |  |
| `ComputeSHA384Hash` | `static byte[] ComputeSHA384Hash(this FileInfo @this)` |  |
| `ComputeSHA512Hash` | `static byte[] ComputeSHA512Hash(this FileInfo @this)` |  |
| `ComputeSHA512Hash` | `static byte[] ComputeSHA512Hash(this FileInfo @this, int blockSize)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, CancellationToken token)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite, CancellationToken token)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, FileInfo targetFile)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, FileInfo targetFile, CancellationToken token)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, FileInfo targetFile, bool overwrite)` |  |
| `CopyToAsync` | `static Task CopyToAsync(this FileInfo @this, FileInfo targetFile, bool overwrite, CancellationToken token)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, DirectoryInfo targetDirectory, bool overwrite)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, FileInfo targetFile)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite)` |  |
| `DetectEncoding` | `static Encoding DetectEncoding(this FileInfo @this, int heuristicSize = 4096)` |  |
| `DetectLineBreakMode` | `static LineBreakMode DetectLineBreakMode(this FileInfo @this)` |  |
| `DetectLineBreakMode` | `static LineBreakMode DetectLineBreakMode(this FileInfo @this, Encoding encoding)` |  |
| `EnableCompression` | `static void EnableCompression(this FileInfo @this)` |  |
| `GetEncoding` | `static Encoding GetEncoding(this FileInfo @this)` |  |
| `GetFilenameWithoutExtension` | `static string GetFilenameWithoutExtension(this FileInfo @this)` |  |
| `GetFilename` | `static string GetFilename(this FileInfo @this)` |  |
| `GetTypeDescription` | `static string GetTypeDescription(this FileInfo @this)` |  |
| `IsContentEqualTo` | `static bool IsContentEqualTo(this FileInfo @this, FileInfo other, int bufferSize = 65536)` |  |
| `IsTextFile` | `static bool IsTextFile(this FileInfo @this)` |  |
| `KeepFirstLines` | `static void KeepFirstLines(this FileInfo @this, int count)` |  |
| `KeepFirstLines` | `static void KeepFirstLines(this FileInfo @this, int count, Encoding encoding)` |  |
| `KeepFirstLines` | `static void KeepFirstLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine)` |  |
| `KeepFirstLines` | `static void KeepFirstLines(this FileInfo @this, int count, LineBreakMode newLine)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, Encoding encoding)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, LineBreakMode newLine)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, int offsetInLines)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, Encoding encoding)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, Encoding encoding, LineBreakMode newLine)` |  |
| `KeepLastLines` | `static void KeepLastLines(this FileInfo @this, int count, int offsetInLines, LineBreakMode newLine)` |  |
| `MatchesFilter` | `static bool MatchesFilter(this FileInfo @this, string filter)` |  |
| `MoveTo` | `static void MoveTo(this FileInfo @this, FileInfo destFile)` |  |
| `MoveTo` | `static void MoveTo(this FileInfo @this, FileInfo destFile, TimeSpan timeout)` |  |
| `MoveTo` | `static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite)` |  |
| `MoveTo` | `static void MoveTo(this FileInfo @this, FileInfo destFile, bool overwrite, TimeSpan timeout)` |  |
| `MoveTo` | `static void MoveTo(this FileInfo @this, string destFileName, bool overwrite, TimeSpan timeout)` |  |
| `NotExists` | `static bool NotExists(this FileInfo @this)` |  |
| `Open` | `static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize)` |  |
| `Open` | `static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)` |  |
| `Open` | `static FileStream Open(this FileInfo @this, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)` |  |
| `ReadAllBytesAsync` | `static Task<byte[]> ReadAllBytesAsync(this FileInfo @this)` |  |
| `ReadAllBytesAsync` | `static Task<byte[]> ReadAllBytesAsync(this FileInfo @this, CancellationToken token)` |  |
| `ReadAllBytesOrDefault` | `static byte[] ReadAllBytesOrDefault(this FileInfo @this)` |  |
| `ReadAllBytesOrDefault` | `static byte[] ReadAllBytesOrDefault(this FileInfo @this, Func<FileInfo, byte[]> defaultValueFactory)` |  |
| `ReadAllBytesOrDefault` | `static byte[] ReadAllBytesOrDefault(this FileInfo @this, Func<byte[]> defaultValueFactory)` |  |
| `ReadAllBytesOrDefault` | `static byte[] ReadAllBytesOrDefault(this FileInfo @this, byte[] defaultValue)` |  |
| `ReadAllBytes` | `static byte[] ReadAllBytes(this FileInfo @this)` |  |
| `ReadAllLinesAsync` | `static Task<string[]> ReadAllLinesAsync(this FileInfo @this)` |  |
| `ReadAllLinesAsync` | `static Task<string[]> ReadAllLinesAsync(this FileInfo @this, CancellationToken token)` |  |
| `ReadAllLinesAsync` | `static Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding)` |  |
| `ReadAllLinesAsync` | `static Task<string[]> ReadAllLinesAsync(this FileInfo @this, Encoding encoding, CancellationToken token)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Encoding encoding)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Encoding encoding, Func<FileInfo, string[]> defaultValueFactory)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Encoding encoding, Func<string[]> defaultValueFactory)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Encoding encoding, string[] defaultValue)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Func<FileInfo, string[]> defaultValueFactory)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, Func<string[]> defaultValueFactory)` |  |
| `ReadAllLinesOrDefault` | `static string[] ReadAllLinesOrDefault(this FileInfo @this, string[] defaultValue)` |  |
| `ReadAllLines` | `static string[] ReadAllLines(this FileInfo @this)` |  |
| `ReadAllLines` | `static string[] ReadAllLines(this FileInfo @this, Encoding encoding)` |  |
| `ReadAllTextAsync` | `static Task<string> ReadAllTextAsync(this FileInfo @this)` |  |
| `ReadAllTextAsync` | `static Task<string> ReadAllTextAsync(this FileInfo @this, CancellationToken token)` |  |
| `ReadAllTextAsync` | `static Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding)` |  |
| `ReadAllTextAsync` | `static Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding, CancellationToken token)` |  |
| `ReadAllText` | `static string ReadAllText(this FileInfo @this)` |  |
| `ReadAllText` | `static string ReadAllText(this FileInfo @this, Encoding encoding)` |  |
| `ReadBytes` | `static IEnumerable<byte> ReadBytes(this FileInfo @this)` |  |
| `ReadLines` | `static IEnumerable<string> ReadLines(this FileInfo @this)` |  |
| `ReadLines` | `static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding)` |  |
| `ReadLines` | `static IEnumerable<string> ReadLines(this FileInfo @this, Encoding encoding, FileShare share)` |  |
| `ReadLines` | `static IEnumerable<string> ReadLines(this FileInfo @this, FileShare share)` |  |
| `RemoveFirstLines` | `static void RemoveFirstLines(this FileInfo @this, int count)` |  |
| `RemoveFirstLines` | `static void RemoveFirstLines(this FileInfo @this, int count, Encoding encoding)` |  |
| `RemoveFirstLines` | `static void RemoveFirstLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine)` |  |
| `RemoveFirstLines` | `static void RemoveFirstLines(this FileInfo @this, int count, LineBreakMode newLine)` |  |
| `RemoveLastLines` | `static void RemoveLastLines(this FileInfo @this, int count)` |  |
| `RemoveLastLines` | `static void RemoveLastLines(this FileInfo @this, int count, Encoding encoding)` |  |
| `RemoveLastLines` | `static void RemoveLastLines(this FileInfo @this, int count, Encoding encoding, LineBreakMode newLine)` |  |
| `RemoveLastLines` | `static void RemoveLastLines(this FileInfo @this, int count, LineBreakMode newLine)` |  |
| `RenameTo` | `static void RenameTo(this FileInfo @this, string newName)` |  |
| `ReplaceWith` | `static void ReplaceWith(this FileInfo @this, FileInfo other)` |  |
| `ReplaceWith` | `static void ReplaceWith(this FileInfo @this, FileInfo other, FileInfo backupFile)` |  |
| `ReplaceWith` | `static void ReplaceWith(this FileInfo @this, FileInfo other, FileInfo backupFile, bool ignoreMetaDataErrors)` |  |
| `ReplaceWith` | `static void ReplaceWith(this FileInfo @this, FileInfo other, bool ignoreMetaDataErrors)` |  |
| `StartWorkInProgress` | `static IFileInProgress StartWorkInProgress(this FileInfo @this, bool copyContents = false)` |  |
| `StartWorkInProgress` | `static IFileInProgress StartWorkInProgress(this FileInfo @this, bool copyContents, ConflictResolutionMode conflictMode)` |  |
| `Touch` | `static void Touch(this FileInfo @this)` |  |
| `TryCreate` | `static bool TryCreate(this FileInfo @this, FileAttributes attributes = 128)` |  |
| `TryDelete` | `static bool TryDelete(this FileInfo @this)` |  |
| `TryEnableCompression` | `static bool TryEnableCompression(this FileInfo @this)` |  |
| `TryReadAllBytes` | `static bool TryReadAllBytes(this FileInfo @this, out byte[] result)` |  |
| `TryReadAllLines` | `static bool TryReadAllLines(this FileInfo @this, Encoding encoding, out string[] result)` |  |
| `TryReadAllLines` | `static bool TryReadAllLines(this FileInfo @this, out string[] result)` |  |
| `TryTouch` | `static bool TryTouch(this FileInfo @this)` |  |
| `TryTouch` | `static bool TryTouch(this FileInfo @this, TimeSpan waitTime, int repeat = 3)` |  |
| `WithNewExtension` | `static FileInfo WithNewExtension(this FileInfo @this, string extension)` |  |
| `WriteAllBytesAsync` | `static Task WriteAllBytesAsync(this FileInfo @this, byte[] data)` |  |
| `WriteAllBytesAsync` | `static Task WriteAllBytesAsync(this FileInfo @this, byte[] data, CancellationToken token)` |  |
| `WriteAllBytes` | `static void WriteAllBytes(this FileInfo @this, byte[] bytes)` |  |
| `WriteAllLinesAsync` | `static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data)` |  |
| `WriteAllLinesAsync` | `static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, CancellationToken token)` |  |
| `WriteAllLinesAsync` | `static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding)` |  |
| `WriteAllLinesAsync` | `static Task WriteAllLinesAsync(this FileInfo @this, IEnumerable<string> data, Encoding encoding, CancellationToken token)` |  |
| `WriteAllLines` | `static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents)` |  |
| `WriteAllLines` | `static void WriteAllLines(this FileInfo @this, IEnumerable<string> contents, Encoding encoding)` |  |
| `WriteAllLines` | `static void WriteAllLines(this FileInfo @this, string[] contents)` |  |
| `WriteAllLines` | `static void WriteAllLines(this FileInfo @this, string[] contents, Encoding encoding)` |  |
| `WriteAllTextAsync` | `static Task WriteAllTextAsync(this FileInfo @this, string data)` |  |
| `WriteAllTextAsync` | `static Task WriteAllTextAsync(this FileInfo @this, string data, CancellationToken token)` |  |
| `WriteAllTextAsync` | `static Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding)` |  |
| `WriteAllTextAsync` | `static Task WriteAllTextAsync(this FileInfo @this, string data, Encoding encoding, CancellationToken token)` |  |
| `WriteAllText` | `static void WriteAllText(this FileInfo @this, string contents)` |  |
| `WriteAllText` | `static void WriteAllText(this FileInfo @this, string contents, Encoding encoding)` |  |

#### `FileInfoExtensions.IFileInProgress`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `CancelChanges` | `bool CancelChanges { get; set; }` |  |
| `ConflictMode` | `ConflictResolutionMode ConflictMode { get; }` |  |
| `OriginalFile` | `FileInfo OriginalFile { get; }` |  |
| `AppendAllLines` | `void AppendAllLines(IEnumerable<string> lines)` |  |
| `AppendAllLines` | `void AppendAllLines(IEnumerable<string> lines, Encoding encoding)` |  |
| `AppendAllText` | `void AppendAllText(string text)` |  |
| `AppendAllText` | `void AppendAllText(string text, Encoding encoding)` |  |
| `AppendLine` | `void AppendLine(string line)` |  |
| `AppendLine` | `void AppendLine(string line, Encoding encoding)` |  |
| `CopyFrom` | `void CopyFrom(FileInfo source)` |  |
| `GetEncoding` | `Encoding GetEncoding()` |  |
| `KeepFirstLines` | `void KeepFirstLines(int count)` |  |
| `KeepFirstLines` | `void KeepFirstLines(int count, Encoding encoding)` |  |
| `KeepLastLines` | `void KeepLastLines(int count)` |  |
| `KeepLastLines` | `void KeepLastLines(int count, Encoding encoding)` |  |
| `Open` | `Stream Open(FileAccess access)` |  |
| `ReadAllBytes` | `byte[] ReadAllBytes()` |  |
| `ReadAllText` | `string ReadAllText()` |  |
| `ReadAllText` | `string ReadAllText(Encoding encoding)` |  |
| `ReadBytes` | `IEnumerable<byte> ReadBytes()` |  |
| `ReadLines` | `IEnumerable<string> ReadLines()` |  |
| `ReadLines` | `IEnumerable<string> ReadLines(Encoding encoding)` |  |
| `RemoveFirstLines` | `void RemoveFirstLines(int count)` |  |
| `RemoveFirstLines` | `void RemoveFirstLines(int count, Encoding encoding)` |  |
| `RemoveLastLines` | `void RemoveLastLines(int count)` |  |
| `RemoveLastLines` | `void RemoveLastLines(int count, Encoding encoding)` |  |
| `WriteAllBytes` | `void WriteAllBytes(byte[] data)` |  |
| `WriteAllLines` | `void WriteAllLines(IEnumerable<string> lines)` |  |
| `WriteAllLines` | `void WriteAllLines(IEnumerable<string> lines, Encoding encoding)` |  |
| `WriteAllText` | `void WriteAllText(string text)` |  |
| `WriteAllText` | `void WriteAllText(string text, Encoding encoding)` |  |

#### `FileSystemInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Age` | `static TimeSpan Age(this FileSystemInfo @this)` |  |
| `IsDirectory` | `static bool IsDirectory(this FileSystemInfo @this)` |  |
| `IsNotNullAndExists` | `static bool IsNotNullAndExists(this FileSystemInfo @this)` |  |
| `IsNullOrDoesNotExist` | `static bool IsNullOrDoesNotExist(this FileSystemInfo @this)` |  |
| `IsOnSamePhysicalDrive` | `static bool IsOnSamePhysicalDrive(string path, string other)` |  |
| `IsOnSamePhysicalDrive` | `static bool IsOnSamePhysicalDrive(this FileSystemInfo @this, FileSystemInfo other)` |  |
| `NotExists` | `static bool NotExists(this FileSystemInfo @this)` |  |
| `RelativeTo` | `static string RelativeTo(this FileSystemInfo @this, FileSystemInfo source)` |  |

#### `LinkExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `CopyTo` | `static void CopyTo(this FileInfo @this, FileInfo targetFile, bool overwrite = false, bool allowHardLinking = false)` |  |
| `CopyTo` | `static void CopyTo(this FileInfo @this, string targetFileName, bool overwrite = false, bool allowHardLinking = false)` |  |
| `CreateHardLinkAt` | `static void CreateHardLinkAt(this FileInfo @this, FileInfo target)` |  |
| `CreateHardLinkAt` | `static void CreateHardLinkAt(this FileInfo @this, string target)` |  |
| `CreateHardLinkFrom` | `static void CreateHardLinkFrom(this FileInfo @this, FileInfo source)` |  |
| `CreateHardLinkFrom` | `static void CreateHardLinkFrom(this FileInfo @this, string source)` |  |
| `CreateJunctionAt` | `static void CreateJunctionAt(this DirectoryInfo @this, DirectoryInfo target)` |  |
| `CreateJunctionAt` | `static void CreateJunctionAt(this DirectoryInfo @this, string target)` |  |
| `CreateJunctionFrom` | `static void CreateJunctionFrom(this DirectoryInfo @this, DirectoryInfo source)` |  |
| `CreateJunctionFrom` | `static void CreateJunctionFrom(this DirectoryInfo @this, string source)` |  |
| `CreateSymbolicLinkAt` | `static void CreateSymbolicLinkAt(this DirectoryInfo @this, DirectoryInfo target)` |  |
| `CreateSymbolicLinkAt` | `static void CreateSymbolicLinkAt(this DirectoryInfo @this, string target)` |  |
| `CreateSymbolicLinkAt` | `static void CreateSymbolicLinkAt(this FileInfo @this, FileInfo target)` |  |
| `CreateSymbolicLinkAt` | `static void CreateSymbolicLinkAt(this FileInfo @this, string target)` |  |
| `CreateSymbolicLinkFrom` | `static void CreateSymbolicLinkFrom(this DirectoryInfo @this, DirectoryInfo source)` |  |
| `CreateSymbolicLinkFrom` | `static void CreateSymbolicLinkFrom(this DirectoryInfo @this, string source)` |  |
| `CreateSymbolicLinkFrom` | `static void CreateSymbolicLinkFrom(this FileInfo @this, FileInfo source)` |  |
| `CreateSymbolicLinkFrom` | `static void CreateSymbolicLinkFrom(this FileInfo @this, string source)` |  |
| `GetHardLinkTarget` | `static string GetHardLinkTarget(this FileInfo @this)` |  |
| `GetHardLinkTargets` | `static IEnumerable<FileInfo> GetHardLinkTargets(this FileInfo @this)` |  |
| `GetJunctionTarget` | `static string GetJunctionTarget(this DirectoryInfo @this)` |  |
| `GetSymbolicLinkTarget` | `static string GetSymbolicLinkTarget(this DirectoryInfo @this)` |  |
| `GetSymbolicLinkTarget` | `static string GetSymbolicLinkTarget(this FileInfo @this)` |  |
| `IsHardLink` | `static bool IsHardLink(this FileInfo @this)` |  |
| `IsJunction` | `static bool IsJunction(this DirectoryInfo @this)` |  |
| `IsSymbolicLink` | `static bool IsSymbolicLink(this DirectoryInfo @this)` |  |
| `IsSymbolicLink` | `static bool IsSymbolicLink(this FileInfo @this)` |  |
| `TryCreateHardLinkAt` | `static bool TryCreateHardLinkAt(this FileInfo @this, FileInfo target)` |  |
| `TryCreateHardLinkAt` | `static bool TryCreateHardLinkAt(this FileInfo @this, string target)` |  |
| `TryCreateHardLinkFrom` | `static bool TryCreateHardLinkFrom(this FileInfo @this, FileInfo source)` |  |
| `TryCreateHardLinkFrom` | `static bool TryCreateHardLinkFrom(this FileInfo @this, string source)` |  |
| `TryCreateJunctionAt` | `static bool TryCreateJunctionAt(this DirectoryInfo @this, DirectoryInfo target)` |  |
| `TryCreateJunctionAt` | `static bool TryCreateJunctionAt(this DirectoryInfo @this, string target)` |  |
| `TryCreateJunctionFrom` | `static bool TryCreateJunctionFrom(this DirectoryInfo @this, DirectoryInfo source)` |  |
| `TryCreateJunctionFrom` | `static bool TryCreateJunctionFrom(this DirectoryInfo @this, string source)` |  |
| `TryCreateSymbolicLinkAt` | `static bool TryCreateSymbolicLinkAt(this DirectoryInfo @this, DirectoryInfo target)` |  |
| `TryCreateSymbolicLinkAt` | `static bool TryCreateSymbolicLinkAt(this DirectoryInfo @this, string target)` |  |
| `TryCreateSymbolicLinkAt` | `static bool TryCreateSymbolicLinkAt(this FileInfo @this, FileInfo target)` |  |
| `TryCreateSymbolicLinkAt` | `static bool TryCreateSymbolicLinkAt(this FileInfo @this, string target)` |  |
| `TryCreateSymbolicLinkFrom` | `static bool TryCreateSymbolicLinkFrom(this DirectoryInfo @this, DirectoryInfo source)` |  |
| `TryCreateSymbolicLinkFrom` | `static bool TryCreateSymbolicLinkFrom(this DirectoryInfo @this, string source)` |  |
| `TryCreateSymbolicLinkFrom` | `static bool TryCreateSymbolicLinkFrom(this FileInfo @this, FileInfo source)` |  |
| `TryCreateSymbolicLinkFrom` | `static bool TryCreateSymbolicLinkFrom(this FileInfo @this, string source)` |  |

#### `PathExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetTempDirectoryName` | `static string GetTempDirectoryName(string name = null, string baseDirectory = null)` |  |
| `GetTempDirectoryToken` | `static ITemporaryDirectoryToken GetTempDirectoryToken(string name = null, string baseDirectory = null)` |  |
| `GetTempDirectory` | `static DirectoryInfo GetTempDirectory(string name = null, string baseDirectory = null)` |  |
| `GetTempFileName` | `static string GetTempFileName(string name = null, string baseDirectory = null)` |  |
| `GetTempFileToken` | `static ITemporaryFileToken GetTempFileToken(string name = null, string baseDirectory = null)` |  |
| `GetTempFile` | `static FileInfo GetTempFile(string name = null, string baseDirectory = null)` |  |
| `GetUsableSystemTempDirectoryName` | `static string GetUsableSystemTempDirectoryName()` |  |
| `GetUsableSystemTempDirectory` | `static DirectoryInfo GetUsableSystemTempDirectory()` |  |
| `TryCreateDirectory` | `static bool TryCreateDirectory(string pathName, FileAttributes attributes = 128)` |  |
| `TryCreateFile` | `static bool TryCreateFile(string fileName, FileAttributes attributes = 128)` |  |

#### `PathExtensions.ITemporaryDirectoryToken`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Directory` | `DirectoryInfo Directory { get; }` |  |
| `MinimumLifetimeLeft` | `TimeSpan MinimumLifetimeLeft { get; set; }` |  |

#### `PathExtensions.ITemporaryFileToken`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `File` | `FileInfo File { get; }` |  |
| `MinimumLifetimeLeft` | `TimeSpan MinimumLifetimeLeft { get; set; }` |  |

#### `PathExtensions.NetworkPath`

| Member | Signature | Summary |
| --- | --- | --- |
| `NetworkPath` | `NetworkPath(string uncPath)` |  |
| `DirectoryAndOrFileName` | `string DirectoryAndOrFileName { get; set; }` |  |
| `FullPath` | `string FullPath { get; set; }` |  |
| `Password` | `string Password { get; set; }` |  |
| `Server` | `string Server { get; set; }` |  |
| `Share` | `string Share { get; set; }` |  |
| `UncPath` | `string UncPath { get; set; }` |  |
| `Username` | `string Username { get; set; }` |  |

#### `StreamExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `BeginReadBytes` | `static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = 0)` |  |
| `BeginReadBytes` | `static IAsyncResult BeginReadBytes(this Stream @this, long position, byte[] buffer, int offset, int count, AsyncCallback callback, object state = null, SeekOrigin seekOrigin = 0)` |  |
| `EndReadBytes` | `static void EndReadBytes(this Stream @this, IAsyncResult result)` |  |
| `IsAtEndOfStream` | `static bool IsAtEndOfStream(this Stream @this)` |  |
| `ReadAllBytes` | `static byte[] ReadAllBytes(this Stream @this)` |  |
| `ReadAllText` | `static string ReadAllText(this Stream @this, Encoding encoding = null)` |  |
| `ReadBool` | `static bool ReadBool(this Stream @this)` |  |
| `ReadBytesAsync` | `static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, CancellationToken token, SeekOrigin seekOrigin = 0)` |  |
| `ReadBytesAsync` | `static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = 0)` |  |
| `ReadBytesAsync` | `static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, CancellationToken token, SeekOrigin seekOrigin = 0)` |  |
| `ReadBytesAsync` | `static Task<int> ReadBytesAsync(this Stream @this, long position, byte[] buffer, int offset, int count, SeekOrigin seekOrigin = 0)` |  |
| `ReadBytes` | `static byte[] ReadBytes(this Stream @this, int count)` |  |
| `ReadBytes` | `static void ReadBytes(this Stream @this, long position, byte[] buffer, SeekOrigin seekOrigin = 0)` |  |
| `ReadChar` | `static char ReadChar(this Stream @this)` |  |
| `ReadChar` | `static char ReadChar(this Stream @this, bool bigEndian)` |  |
| `ReadFixedLengthString` | `static string ReadFixedLengthString(this Stream @this, int length, char padding = '\0', Encoding encoding = null)` |  |
| `ReadFloat32` | `static float ReadFloat32(this Stream @this)` |  |
| `ReadFloat32` | `static float ReadFloat32(this Stream @this, bool bigEndian)` |  |
| `ReadFloat64` | `static double ReadFloat64(this Stream @this)` |  |
| `ReadFloat64` | `static double ReadFloat64(this Stream @this, bool bigEndian)` |  |
| `ReadInt16` | `static short ReadInt16(this Stream @this)` |  |
| `ReadInt16` | `static short ReadInt16(this Stream @this, bool bigEndian)` |  |
| `ReadInt32` | `static int ReadInt32(this Stream @this)` |  |
| `ReadInt32` | `static int ReadInt32(this Stream @this, bool bigEndian)` |  |
| `ReadInt64` | `static long ReadInt64(this Stream @this)` |  |
| `ReadInt64` | `static long ReadInt64(this Stream @this, bool bigEndian)` |  |
| `ReadInt8` | `static sbyte ReadInt8(this Stream @this)` |  |
| `ReadLengthPrefixedString` | `static string ReadLengthPrefixedString(this Stream @this, Encoding encoding = null)` |  |
| `ReadMoney128` | `static decimal ReadMoney128(this Stream @this)` |  |
| `ReadMoney128` | `static decimal ReadMoney128(this Stream @this, bool bigEndian)` |  |
| `ReadUInt16` | `static ushort ReadUInt16(this Stream @this)` |  |
| `ReadUInt16` | `static ushort ReadUInt16(this Stream @this, bool bigEndian)` |  |
| `ReadUInt32` | `static uint ReadUInt32(this Stream @this)` |  |
| `ReadUInt32` | `static uint ReadUInt32(this Stream @this, bool bigEndian)` |  |
| `ReadUInt64` | `static ulong ReadUInt64(this Stream @this)` |  |
| `ReadUInt64` | `static ulong ReadUInt64(this Stream @this, bool bigEndian)` |  |
| `ReadUInt8` | `static byte ReadUInt8(this Stream @this)` |  |
| `ReadZeroTerminatedString` | `static string ReadZeroTerminatedString(this Stream @this, Encoding encoding = null)` |  |
| `Read` | `static TStruct Read<TStruct>(this Stream @this)` |  |
| `Read` | `static int Read(this Stream @this, byte[] result)` |  |
| `ToArray` | `static byte[] ToArray(this Stream @this)` |  |
| `WriteAllText` | `static void WriteAllText(this Stream @this, string data, Encoding encoding = null)` |  |
| `WriteFixedLengthString` | `static void WriteFixedLengthString(this Stream @this, string data, int length, char padding = '\0', Encoding encoding = null)` |  |
| `WriteLengthPrefixedString` | `static void WriteLengthPrefixedString(this Stream @this, string data, Encoding encoding = null)` |  |
| `WriteZeroTerminatedString` | `static void WriteZeroTerminatedString(this Stream @this, string data, Encoding encoding = null)` |  |
| `Write` | `static void Write(this Stream @this, bool value)` |  |
| `Write` | `static void Write(this Stream @this, byte value)` |  |
| `Write` | `static void Write(this Stream @this, byte[] data)` |  |
| `Write` | `static void Write(this Stream @this, char value)` |  |
| `Write` | `static void Write(this Stream @this, char value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, decimal value)` |  |
| `Write` | `static void Write(this Stream @this, decimal value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, double value)` |  |
| `Write` | `static void Write(this Stream @this, double value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, float value)` |  |
| `Write` | `static void Write(this Stream @this, float value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, int value)` |  |
| `Write` | `static void Write(this Stream @this, int value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, long value)` |  |
| `Write` | `static void Write(this Stream @this, long value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, sbyte value)` |  |
| `Write` | `static void Write(this Stream @this, short value)` |  |
| `Write` | `static void Write(this Stream @this, short value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, uint value)` |  |
| `Write` | `static void Write(this Stream @this, uint value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, ulong value)` |  |
| `Write` | `static void Write(this Stream @this, ulong value, bool bigEndian)` |  |
| `Write` | `static void Write(this Stream @this, ushort value)` |  |
| `Write` | `static void Write(this Stream @this, ushort value, bool bigEndian)` |  |
| `Write` | `static void Write<TStruct>(this Stream @this, TStruct value)` |  |

#### `TextReaderExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ReadLines` | `static IEnumerable<string> ReadLines(this TextReader @this)` |  |

#### `VolumeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetVolumeMountPoints` | `static IEnumerable<string> GetVolumeMountPoints(string volumeName)` |  |
| `GetVolumePathNames` | `static IEnumerable<string> GetVolumePathNames(string volumeName)` |  |
| `GetVolumes` | `static IEnumerable<Volume> GetVolumes()` |  |
| `GetVolumes` | `static IEnumerable<Volume> GetVolumes(Regex regex)` |  |
| `GetVolumes` | `static IEnumerable<Volume> GetVolumes(string filterMask)` |  |

#### `VolumeExtensions.Volume`

| Member | Signature | Summary |
| --- | --- | --- |
| `Volume` | `Volume(string name)` |  |
| `MountPoints` | `IEnumerable<string> MountPoints { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `PathNames` | `IEnumerable<string> PathNames { get; }` |  |
| `ToString` | `override string ToString()` |  |

### Namespace `System.Linq`

[`IQueryableExtensions`](#iqueryableextensions)

#### `IQueryableExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `FilterIfNeeded` | `static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, Expression<Func<TRow, string>> selector, string query, bool ignoreCase = false)` |  |
| `FilterIfNeeded` | `static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, string query, bool ignoreCase, params Expression<Func<TRow, string>>[] selectors)` |  |
| `FilterIfNeeded` | `static IQueryable<TRow> FilterIfNeeded<TRow>(this IQueryable<TRow> @this, string query, params Expression<Func<TRow, string>>[] selectors)` |  |
| `FirstOrDefaultWithSanitizedDbValue` | `static TRow FirstOrDefaultWithSanitizedDbValue<TRow>(this IQueryable<TRow> @this, Expression<Func<TRow, string>> selector, string value, bool ignoreCase = true)` |  |
| `OrderByPropertyNameDescending` | `static IOrderedQueryable<T> OrderByPropertyNameDescending<T>(this IQueryable<T> @this, string propertyPath)` |  |
| `OrderByPropertyName` | `static IOrderedQueryable<TElement> OrderByPropertyName<TElement>(this IQueryable<TElement> @this, string propertyPath)` |  |
| `ThenByPropertyNameDescending` | `static IOrderedQueryable<T> ThenByPropertyNameDescending<T>(this IOrderedQueryable<T> @this, string propertyPath)` |  |
| `ThenByPropertyName` | `static IOrderedQueryable<T> ThenByPropertyName<T>(this IOrderedQueryable<T> @this, string propertyPath)` |  |

### Namespace `System.Net`

[`IPAddressExtensions`](#ipaddressextensions) · [`IPHelper`](#iphelper) · [`IPHelper.Connection`](#iphelperconnection) · [`IPHelper.ConnectionProtocol`](#iphelperconnectionprotocol) · [`IPHelper.ConnectionState`](#iphelperconnectionstate) · [`WebHeaderCollectionExntenions`](#webheadercollectionexntenions)

#### `IPAddressExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetHostName` | `static string GetHostName(this IPAddress @this)` |  |
| `IsLoopback` | `static bool IsLoopback(this IPAddress @this)` |  |
| `Ping` | `static Tuple<bool, string, PingReply, Exception> Ping(this IPAddress @this, uint retryCount = 0, TimeSpan? timeout = null, PingOptions options = null)` |  |

#### `IPHelper`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetActiveConnections` | `static Connection[] GetActiveConnections()` |  |
| `GetTcpTable` | `static Connection[] GetTcpTable()` |  |
| `GetUdpTable` | `static Connection[] GetUdpTable()` |  |

#### `IPHelper.Connection`

| Member | Signature | Summary |
| --- | --- | --- |
| `Local` | `IPEndPoint Local { get; }` |  |
| `Protocol` | `ConnectionProtocol Protocol { get; }` |  |
| `Remote` | `IPEndPoint Remote { get; }` |  |
| `SourceProcess` | `Process SourceProcess { get; }` |  |
| `State` | `ConnectionState State { get; }` |  |
| `ToString` | `override string ToString()` |  |

#### `IPHelper.ConnectionProtocol`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Unknown` | `0` |  |
| `Tcp` | `1` |  |
| `Udp` | `2` |  |

#### `IPHelper.ConnectionState`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Unknown` | `0` |  |
| `Established` | `5` |  |
| `Listening` | `2` |  |
| `SynSent` | `3` |  |
| `SynReceived` | `4` |  |
| `Closed` | `1` |  |
| `Closing` | `9` |  |
| `CloseWait` | `8` |  |
| `FinWait1` | `6` |  |
| `FinWait2` | `7` |  |
| `LastAcknowledgeWaiting` | `10` |  |
| `TimeoutWaitingForTermination` | `11` |  |
| `DeleteTransmissionControlBlock` | `12` |  |

#### `WebHeaderCollectionExntenions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AddRange` | `static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers)` |  |
| `AddRange` | `static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<HttpResponseHeader, string>> headers)` |  |
| `AddRange` | `static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<string, string>> headers)` |  |

### Namespace `System.Net.NetworkInformation`

[`PhysicalAddressExtensions`](#physicaladdressextensions)

#### `PhysicalAddressExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetIpAdresses` | `static IEnumerable<IPAddress> GetIpAdresses(this PhysicalAddress @this)` |  |
| `MacAdress` | `static string MacAdress(this PhysicalAddress @this)` |  |

### Namespace `System.Net.Sockets`

[`TcpClientExtensions`](#tcpclientextensions)

#### `TcpClientExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetState` | `static TcpState GetState(this TcpClient @this)` |  |
| `IsStillConnected` | `static bool IsStillConnected(this TcpClient @this)` |  |

### Namespace `System.Reflection`

[`AssemblyExtensions`](#assemblyextensions) · [`MemberInfoExtensions`](#memberinfoextensions) · [`MethodBaseExtensions`](#methodbaseextensions) · [`MethodBaseExtensions.ILInstruction`](#methodbaseextensionsilinstruction) · [`MethodInfoExtensions`](#methodinfoextensions) · [`PropertyInfoExtensions`](#propertyinfoextensions)

#### `AssemblyExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetGuidOrFallback` | `static string GetGuidOrFallback(this Assembly @this, string fallbackGuid = null)` |  |
| `GetResourceBinaryReader` | `static BinaryReader GetResourceBinaryReader(this Assembly @this, string fileName)` |  |
| `GetResourceFileStream` | `static Stream GetResourceFileStream(this Assembly @this, string fileName)` |  |
| `GetResourceStreamReader` | `static StreamReader GetResourceStreamReader(this Assembly @this, string fileName)` |  |
| `ReadResourceAllBytes` | `static byte[] ReadResourceAllBytes(this Assembly @this, string fileName)` |  |
| `ReadResourceAllLines` | `static IEnumerable<string> ReadResourceAllLines(this Assembly @this, string fileName)` |  |
| `ReadResourceAllText` | `static string ReadResourceAllText(this Assembly @this, string fileName)` |  |

#### `MemberInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetBrowsableOrDefault` | `static bool GetBrowsableOrDefault(this MemberInfo @this, Func<bool> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetBrowsableOrDefault` | `static bool GetBrowsableOrDefault(this MemberInfo @this, bool defaultValue = false, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCategoryOrDefault` | `static string GetCategoryOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCategoryOrDefault` | `static string GetCategoryOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributeOrDefault` | `static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo @this, Func<TAttribute> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributeOrDefault` | `static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo @this, TAttribute defaultValue = null, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributeValueOrDefault` | `static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, Func<TValue> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributeValueOrDefault` | `static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, TValue defaultValue = null, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributeValue` | `static TValue GetCustomAttributeValue<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttribute` | `static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo @this, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetCustomAttributes` | `static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo @this, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetDescriptionOrDefault` | `static string GetDescriptionOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetDescriptionOrDefault` | `static string GetDescriptionOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetDisplayNameOrDefault` | `static string GetDisplayNameOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetDisplayNameOrDefault` | `static string GetDisplayNameOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetReadOnlyOrDefault` | `static bool GetReadOnlyOrDefault(this MemberInfo @this, Func<bool> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false)` |  |
| `GetReadOnlyOrDefault` | `static bool GetReadOnlyOrDefault(this MemberInfo @this, bool defaultValue = false, bool inherit = true, bool inheritInterfaces = false)` |  |
| `TryGetCustomAttribute` | `static bool TryGetCustomAttribute<TAttribute>(this MemberInfo @this, out TAttribute result, bool inherit = true, bool inheritInterfaces = false)` |  |

#### `MethodBaseExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetInstructions` | `static ILInstruction[] GetInstructions(this MethodBase @this)` |  |
| `IsCompilerGenerated` | `static bool IsCompilerGenerated(this MethodBase @this)` |  |
| `IsGetterOrSetter` | `static bool IsGetterOrSetter(this MethodBase @this)` |  |

#### `MethodBaseExtensions.ILInstruction`

| Member | Signature | Summary |
| --- | --- | --- |
| `ILInstruction` | `ILInstruction()` |  |
| `Code` | `OpCode Code { get; set; }` |  |
| `Offset` | `int Offset { get; set; }` |  |
| `OperandData` | `byte[] OperandData { get; set; }` |  |
| `Operand` | `object Operand { get; set; }` |  |
| `GetCode` | `string GetCode()` |  |
| `ToString` | `override string ToString()` |  |

#### `MethodInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetFullSignature` | `static string GetFullSignature(this MethodInfo @this)` |  |

#### `PropertyInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetValueOrDefault` | `static object GetValueOrDefault(this PropertyInfo @this, object value, object[] index = null, object defaultValue = null)` |  |
| `TrySetValue` | `static bool TrySetValue(this PropertyInfo @this, object instance, object value)` |  |
| `TrySetValue` | `static bool TrySetValue(this PropertyInfo @this, object value)` |  |

### Namespace `System.Runtime`

[`DynamicObjectFactory`](#dynamicobjectfactory)

#### `DynamicObjectFactory`

| Member | Signature | Summary |
| --- | --- | --- |
| `CreateInstance` | `static TClass CreateInstance<TClass>(params object[] arrParams)` |  |
| `CreateInstance` | `static object CreateInstance(string strType, params object[] arrParams)` |  |

### Namespace `System.Runtime.Serialization`

[`SerializationInfoExtensions`](#serializationinfoextensions)

#### `SerializationInfoExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ContainsKey` | `static bool ContainsKey(this SerializationInfo This, string name)` |  |
| `GetValueOrDefault` | `static TType GetValueOrDefault<TType>(this SerializationInfo This, string name)` |  |
| `GetValueOrDefault` | `static TType GetValueOrDefault<TType>(this SerializationInfo This, string name, TType defaultValue)` |  |
| `GetValueOrDefault` | `static bool GetValueOrDefault(this SerializationInfo This, string name, bool defaultValue = false)` |  |
| `GetValueOrDefault` | `static byte GetValueOrDefault(this SerializationInfo This, string name, byte defaultValue = 0)` |  |
| `GetValueOrDefault` | `static decimal GetValueOrDefault(this SerializationInfo This, string name, decimal defaultValue = 0)` |  |
| `GetValueOrDefault` | `static double GetValueOrDefault(this SerializationInfo This, string name, double defaultValue = 0)` |  |
| `GetValueOrDefault` | `static float GetValueOrDefault(this SerializationInfo This, string name, float defaultValue = 0)` |  |
| `GetValueOrDefault` | `static int GetValueOrDefault(this SerializationInfo This, string name, int defaultValue = 0)` |  |
| `GetValueOrDefault` | `static long GetValueOrDefault(this SerializationInfo This, string name, long defaultValue = 0)` |  |
| `GetValueOrDefault` | `static sbyte GetValueOrDefault(this SerializationInfo This, string name, sbyte defaultValue = 0)` |  |
| `GetValueOrDefault` | `static short GetValueOrDefault(this SerializationInfo This, string name, short defaultValue = 0)` |  |
| `GetValueOrDefault` | `static uint GetValueOrDefault(this SerializationInfo This, string name, uint defaultValue = 0)` |  |
| `GetValueOrDefault` | `static ulong GetValueOrDefault(this SerializationInfo This, string name, ulong defaultValue = 0)` |  |
| `GetValueOrDefault` | `static ushort GetValueOrDefault(this SerializationInfo This, string name, ushort defaultValue = 0)` |  |
| `GetValue` | `static TType GetValue<TType>(this SerializationInfo This, string name)` |  |

### Namespace `System.Security.Cryptography`

[`Adler`](#adler) · [`Fletcher`](#fletcher) · [`IAdvancedHashAlgorithm`](#iadvancedhashalgorithm) · [`JavaHash`](#javahash) · [`LRC8`](#lrc8) · [`Pearson`](#pearson) · [`RandomNumberGeneratorExtenions`](#randomnumbergeneratorextenions) · [`Tiger`](#tiger) · [`Whirlpool`](#whirlpool)

#### `Adler`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Adler` | `Adler()` |  |
| `Adler` | `Adler(int outputBits)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int ibStart, int cbSize)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `Fletcher`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Fletcher` | `Fletcher()` |  |
| `Fletcher` | `Fletcher(int outputBits)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int ibStart, int cbSize)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `IAdvancedHashAlgorithm`

| Member | Signature | Summary |
| --- | --- | --- |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |

#### `JavaHash`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `JavaHash` | `JavaHash()` |  |
| `JavaHash` | `JavaHash(int outputBits)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int index, int count)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `LRC8`

Inherits `HashAlgorithm`. Implements `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `LRC8` | `LRC8()` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int index, int count)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `Pearson`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Pearson` | `Pearson()` |  |
| `Pearson` | `Pearson(byte[] iv)` |  |
| `Pearson` | `Pearson(int numberOfResultBits)` |  |
| `Pearson` | `Pearson(int numberOfResultBits, byte[] iv)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int ibStart, int cbSize)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `RandomNumberGeneratorExtenions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Next` | `static int Next(this RandomNumberGenerator @this, int maxValue)` |  |

#### `Tiger`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Tiger` | `Tiger()` |  |
| `Tiger` | `Tiger(int numberOfResultBits)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int ibStart, int cbSize)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

#### `Whirlpool`

Inherits `HashAlgorithm`. Implements `IAdvancedHashAlgorithm`, `ICryptoTransform`, `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `Whirlpool` | `Whirlpool()` |  |
| `Whirlpool` | `Whirlpool(int numberOfResultBits)` |  |
| `IV` | `byte[] IV { get; set; }` |  |
| `MaxIVBits` | `static int MaxIVBits { get; }` |  |
| `MaxOutputBits` | `static int MaxOutputBits { get; }` |  |
| `MinIVBits` | `static int MinIVBits { get; }` |  |
| `MinOutputBits` | `static int MinOutputBits { get; }` |  |
| `Name` | `string Name { get; }` |  |
| `OutputBits` | `int OutputBits { get; set; }` |  |
| `SupportedIVBits` | `static int[] SupportedIVBits { get; }` |  |
| `SupportedOutputBits` | `static int[] SupportedOutputBits { get; }` |  |
| `SupportsIV` | `static bool SupportsIV { get; }` |  |
| `HashCore` | `protected override void HashCore(byte[] array, int ibStart, int cbSize)` |  |
| `HashFinal` | `protected override byte[] HashFinal()` |  |
| `Initialize` | `override void Initialize()` |  |

### Namespace `System.Text`

[`AmstradCpcEncoding`](#amstradcpcencoding) · [`AnsiString`](#ansistring) · [`AnsiZ`](#ansiz) · [`AsciiString`](#asciistring) · [`AsciiZ`](#asciiz) · [`AtasciiEncoding`](#atasciiencoding) · [`EbcdicEncoding`](#ebcdicencoding) · [`FixedAnsi`](#fixedansi) · [`FixedAscii`](#fixedascii) · [`FixedString`](#fixedstring) · [`InvalidCharBehavior`](#invalidcharbehavior) · [`PetsciiEncoding`](#petsciiencoding) · [`RetroSingleByteEncoding`](#retrosinglebyteencoding) · [`StringBuilderExtensions`](#stringbuilderextensions) · [`StringZ`](#stringz)

#### `AmstradCpcEncoding`

Inherits `RetroSingleByteEncoding`. Implements `ICloneable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EncodingName` | `override string EncodingName { get; }` |  |
| `Instance` | `static AmstradCpcEncoding Instance { get; }` |  |

#### `AnsiString`

Implements `IComparable`, `IComparable<AnsiString>`, `IEquatable<AnsiString>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `AnsiString` | `AnsiString(ReadOnlySpan<byte> value)` |  |
| `AnsiString` | `AnsiString(ReadOnlySpan<char> value)` |  |
| `AnsiString` | `AnsiString(byte[] value)` |  |
| `AnsiString` | `AnsiString(string value)` |  |
| `Empty` | `static AnsiString Empty { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `AnsiString this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan(int start)` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan(int start, int length)` |  |
| `CompareTo` | `int CompareTo(AnsiString other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(AnsiString other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `Substring` | `AnsiString Substring(int startIndex)` |  |
| `Substring` | `AnsiString Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator AsciiString` | `static explicit operator AsciiString(AnsiString value)` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(string value)` |  |
| `implicit operator string` | `static implicit operator string(AnsiString value)` |  |
| `operator !=` | `static bool operator !=(AnsiString left, AnsiString right)` |  |
| `operator +` | `static AnsiString operator +(AnsiString left, AnsiString right)` |  |
| `operator <=` | `static bool operator <=(AnsiString left, AnsiString right)` |  |
| `operator <` | `static bool operator <(AnsiString left, AnsiString right)` |  |
| `operator ==` | `static bool operator ==(AnsiString left, AnsiString right)` |  |
| `operator >=` | `static bool operator >=(AnsiString left, AnsiString right)` |  |
| `operator >` | `static bool operator >(AnsiString left, AnsiString right)` |  |

#### `AnsiZ`

Implements `IComparable`, `IComparable<AnsiZ>`, `IEquatable<AnsiZ>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `AnsiZ` | `AnsiZ(ReadOnlySpan<byte> value)` |  |
| `AnsiZ` | `AnsiZ(ReadOnlySpan<char> value)` |  |
| `AnsiZ` | `AnsiZ(byte[] value)` |  |
| `AnsiZ` | `AnsiZ(string value)` |  |
| `Empty` | `static AnsiZ Empty { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `AnsiZ this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan(int start)` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan(int start, int length)` |  |
| `CompareTo` | `int CompareTo(AnsiZ other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(AnsiZ other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `Substring` | `AnsiZ Substring(int startIndex)` |  |
| `Substring` | `AnsiZ Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator AnsiZ` | `static explicit operator AnsiZ(AnsiString value)` |  |
| `explicit operator AnsiZ` | `static explicit operator AnsiZ(FixedAnsi value)` |  |
| `explicit operator AsciiString` | `static explicit operator AsciiString(AnsiZ value)` |  |
| `explicit operator AsciiZ` | `static explicit operator AsciiZ(AnsiZ value)` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(AnsiZ value)` |  |
| `implicit operator AnsiZ` | `static implicit operator AnsiZ(string value)` |  |
| `implicit operator string` | `static implicit operator string(AnsiZ value)` |  |
| `operator !=` | `static bool operator !=(AnsiZ left, AnsiZ right)` |  |
| `operator +` | `static AnsiZ operator +(AnsiZ left, AnsiZ right)` |  |
| `operator <=` | `static bool operator <=(AnsiZ left, AnsiZ right)` |  |
| `operator <` | `static bool operator <(AnsiZ left, AnsiZ right)` |  |
| `operator ==` | `static bool operator ==(AnsiZ left, AnsiZ right)` |  |
| `operator >=` | `static bool operator >=(AnsiZ left, AnsiZ right)` |  |
| `operator >` | `static bool operator >(AnsiZ left, AnsiZ right)` |  |

#### `AsciiString`

Implements `IComparable`, `IComparable<AsciiString>`, `IEquatable<AsciiString>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `AsciiString` | `AsciiString(ReadOnlySpan<byte> value)` |  |
| `AsciiString` | `AsciiString(ReadOnlySpan<byte> value, InvalidCharBehavior behavior)` |  |
| `AsciiString` | `AsciiString(ReadOnlySpan<char> value)` |  |
| `AsciiString` | `AsciiString(ReadOnlySpan<char> value, InvalidCharBehavior behavior)` |  |
| `AsciiString` | `AsciiString(byte[] value)` |  |
| `AsciiString` | `AsciiString(byte[] value, InvalidCharBehavior behavior)` |  |
| `AsciiString` | `AsciiString(string value)` |  |
| `AsciiString` | `AsciiString(string value, InvalidCharBehavior behavior)` |  |
| `Empty` | `static AsciiString Empty { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `AsciiString this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `CompareTo` | `int CompareTo(AsciiString other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(AsciiString other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `Substring` | `AsciiString Substring(int startIndex)` |  |
| `Substring` | `AsciiString Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(AsciiString value)` |  |
| `implicit operator AsciiString` | `static implicit operator AsciiString(string value)` |  |
| `implicit operator string` | `static implicit operator string(AsciiString value)` |  |
| `operator !=` | `static bool operator !=(AsciiString left, AsciiString right)` |  |
| `operator +` | `static AsciiString operator +(AsciiString left, AsciiString right)` |  |
| `operator <=` | `static bool operator <=(AsciiString left, AsciiString right)` |  |
| `operator <` | `static bool operator <(AsciiString left, AsciiString right)` |  |
| `operator ==` | `static bool operator ==(AsciiString left, AsciiString right)` |  |
| `operator >=` | `static bool operator >=(AsciiString left, AsciiString right)` |  |
| `operator >` | `static bool operator >(AsciiString left, AsciiString right)` |  |

#### `AsciiZ`

Implements `IComparable`, `IComparable<AsciiZ>`, `IEquatable<AsciiZ>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `AsciiZ` | `AsciiZ(ReadOnlySpan<byte> value)` |  |
| `AsciiZ` | `AsciiZ(ReadOnlySpan<byte> value, InvalidCharBehavior behavior)` |  |
| `AsciiZ` | `AsciiZ(ReadOnlySpan<char> value)` |  |
| `AsciiZ` | `AsciiZ(ReadOnlySpan<char> value, InvalidCharBehavior behavior)` |  |
| `AsciiZ` | `AsciiZ(byte[] value)` |  |
| `AsciiZ` | `AsciiZ(byte[] value, InvalidCharBehavior behavior)` |  |
| `AsciiZ` | `AsciiZ(string value)` |  |
| `AsciiZ` | `AsciiZ(string value, InvalidCharBehavior behavior)` |  |
| `Empty` | `static AsciiZ Empty { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `AsciiZ this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `CompareTo` | `int CompareTo(AsciiZ other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(AsciiZ other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `Substring` | `AsciiZ Substring(int startIndex)` |  |
| `Substring` | `AsciiZ Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `explicit operator AsciiZ` | `static explicit operator AsciiZ(AsciiString value)` |  |
| `explicit operator AsciiZ` | `static explicit operator AsciiZ(FixedAscii value)` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(AsciiZ value)` |  |
| `implicit operator AnsiZ` | `static implicit operator AnsiZ(AsciiZ value)` |  |
| `implicit operator AsciiString` | `static implicit operator AsciiString(AsciiZ value)` |  |
| `implicit operator AsciiZ` | `static implicit operator AsciiZ(string value)` |  |
| `implicit operator string` | `static implicit operator string(AsciiZ value)` |  |
| `operator !=` | `static bool operator !=(AsciiZ left, AsciiZ right)` |  |
| `operator +` | `static AsciiZ operator +(AsciiZ left, AsciiZ right)` |  |
| `operator <=` | `static bool operator <=(AsciiZ left, AsciiZ right)` |  |
| `operator <` | `static bool operator <(AsciiZ left, AsciiZ right)` |  |
| `operator ==` | `static bool operator ==(AsciiZ left, AsciiZ right)` |  |
| `operator >=` | `static bool operator >=(AsciiZ left, AsciiZ right)` |  |
| `operator >` | `static bool operator >(AsciiZ left, AsciiZ right)` |  |

#### `AtasciiEncoding`

Inherits `RetroSingleByteEncoding`. Implements `ICloneable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EncodingName` | `override string EncodingName { get; }` |  |
| `Instance` | `static AtasciiEncoding Instance { get; }` |  |

#### `EbcdicEncoding`

Inherits `Encoding`. Implements `ICloneable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `EbcdicEncoding` | `EbcdicEncoding()` |  |
| `CP037` | `static EbcdicEncoding CP037 { get; }` |  |
| `FromEbcdic` | `static char FromEbcdic(byte value)` |  |
| `GetByteCount` | `override int GetByteCount(char[] chars, int index, int count)` |  |
| `GetBytes` | `override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)` |  |
| `GetCharCount` | `override int GetCharCount(byte[] bytes, int index, int count)` |  |
| `GetChars` | `override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)` |  |
| `GetMaxByteCount` | `override int GetMaxByteCount(int charCount)` |  |
| `GetMaxCharCount` | `override int GetMaxCharCount(int byteCount)` |  |
| `ToEbcdic` | `static byte ToEbcdic(char value)` |  |

#### `FixedAnsi`

Implements `IComparable`, `IComparable<FixedAnsi>`, `IEquatable<FixedAnsi>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FixedAnsi` | `FixedAnsi(int capacity)` |  |
| `FixedAnsi` | `FixedAnsi(int capacity, ReadOnlySpan<byte> value)` |  |
| `FixedAnsi` | `FixedAnsi(int capacity, ReadOnlySpan<char> value)` |  |
| `FixedAnsi` | `FixedAnsi(int capacity, byte[] value)` |  |
| `FixedAnsi` | `FixedAnsi(int capacity, string value)` |  |
| `Capacity` | `int Capacity { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `FixedAnsi this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsFullSpan` | `ReadOnlySpan<byte> AsFullSpan()` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `CompareTo` | `int CompareTo(FixedAnsi other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(FixedAnsi other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `PadLeft` | `FixedAnsi PadLeft(byte paddingByte = 32)` |  |
| `PadRight` | `FixedAnsi PadRight(byte paddingByte = 0)` |  |
| `Substring` | `FixedAnsi Substring(int startIndex)` |  |
| `Substring` | `FixedAnsi Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `TrimEnd` | `FixedAnsi TrimEnd()` |  |
| `explicit operator AnsiZ` | `static explicit operator AnsiZ(FixedAnsi value)` |  |
| `explicit operator AsciiString` | `static explicit operator AsciiString(FixedAnsi value)` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(FixedAnsi value)` |  |
| `implicit operator string` | `static implicit operator string(FixedAnsi value)` |  |
| `operator !=` | `static bool operator !=(FixedAnsi left, FixedAnsi right)` |  |
| `operator <=` | `static bool operator <=(FixedAnsi left, FixedAnsi right)` |  |
| `operator <` | `static bool operator <(FixedAnsi left, FixedAnsi right)` |  |
| `operator ==` | `static bool operator ==(FixedAnsi left, FixedAnsi right)` |  |
| `operator >=` | `static bool operator >=(FixedAnsi left, FixedAnsi right)` |  |
| `operator >` | `static bool operator >(FixedAnsi left, FixedAnsi right)` |  |

#### `FixedAscii`

Implements `IComparable`, `IComparable<FixedAscii>`, `IEquatable<FixedAscii>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FixedAscii` | `FixedAscii(int capacity)` |  |
| `FixedAscii` | `FixedAscii(int capacity, ReadOnlySpan<char> value)` |  |
| `FixedAscii` | `FixedAscii(int capacity, ReadOnlySpan<char> value, InvalidCharBehavior behavior)` |  |
| `FixedAscii` | `FixedAscii(int capacity, byte[] value)` |  |
| `FixedAscii` | `FixedAscii(int capacity, byte[] value, InvalidCharBehavior behavior)` |  |
| `FixedAscii` | `FixedAscii(int capacity, string value)` |  |
| `FixedAscii` | `FixedAscii(int capacity, string value, InvalidCharBehavior behavior)` |  |
| `Capacity` | `int Capacity { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `FixedAscii this[Range range] { get; }` |  |
| `Item` | `byte this[Index index] { get; }` |  |
| `Item` | `byte this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<byte> AsSpan()` |  |
| `CompareTo` | `int CompareTo(FixedAscii other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(FixedAscii other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `byte GetPinnableReference()` |  |
| `PadLeft` | `FixedAscii PadLeft(byte paddingByte = 32)` |  |
| `PadRight` | `FixedAscii PadRight(byte paddingByte = 0)` |  |
| `Substring` | `FixedAscii Substring(int startIndex)` |  |
| `Substring` | `FixedAscii Substring(int startIndex, int length)` |  |
| `ToArray` | `byte[] ToArray()` |  |
| `ToNullTerminatedArray` | `byte[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `TrimEnd` | `FixedAscii TrimEnd()` |  |
| `explicit operator AsciiZ` | `static explicit operator AsciiZ(FixedAscii value)` |  |
| `implicit operator AnsiString` | `static implicit operator AnsiString(FixedAscii value)` |  |
| `implicit operator AsciiString` | `static implicit operator AsciiString(FixedAscii value)` |  |
| `implicit operator string` | `static implicit operator string(FixedAscii value)` |  |
| `operator !=` | `static bool operator !=(FixedAscii left, FixedAscii right)` |  |
| `operator <=` | `static bool operator <=(FixedAscii left, FixedAscii right)` |  |
| `operator <` | `static bool operator <(FixedAscii left, FixedAscii right)` |  |
| `operator ==` | `static bool operator ==(FixedAscii left, FixedAscii right)` |  |
| `operator >=` | `static bool operator >=(FixedAscii left, FixedAscii right)` |  |
| `operator >` | `static bool operator >(FixedAscii left, FixedAscii right)` |  |

#### `FixedString`

Implements `IComparable`, `IComparable<FixedString>`, `IEquatable<FixedString>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `FixedString` | `FixedString(int capacity)` |  |
| `FixedString` | `FixedString(int capacity, ReadOnlySpan<char> value)` |  |
| `FixedString` | `FixedString(int capacity, string value)` |  |
| `Capacity` | `int Capacity { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `FixedString this[Range range] { get; }` |  |
| `Item` | `char this[Index index] { get; }` |  |
| `Item` | `char this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsFullSpan` | `ReadOnlySpan<char> AsFullSpan()` |  |
| `AsSpan` | `ReadOnlySpan<char> AsSpan()` |  |
| `CompareTo` | `int CompareTo(FixedString other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(FixedString other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `char GetPinnableReference()` |  |
| `PadLeft` | `FixedString PadLeft(char paddingChar = ' ')` |  |
| `PadRight` | `FixedString PadRight(char paddingChar = '\0')` |  |
| `Substring` | `FixedString Substring(int startIndex)` |  |
| `Substring` | `FixedString Substring(int startIndex, int length)` |  |
| `ToNullTerminatedArray` | `char[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `TrimEnd` | `FixedString TrimEnd()` |  |
| `explicit operator StringZ` | `static explicit operator StringZ(FixedString value)` |  |
| `implicit operator string` | `static implicit operator string(FixedString value)` |  |
| `operator !=` | `static bool operator !=(FixedString left, FixedString right)` |  |
| `operator <=` | `static bool operator <=(FixedString left, FixedString right)` |  |
| `operator <` | `static bool operator <(FixedString left, FixedString right)` |  |
| `operator ==` | `static bool operator ==(FixedString left, FixedString right)` |  |
| `operator >=` | `static bool operator >=(FixedString left, FixedString right)` |  |
| `operator >` | `static bool operator >(FixedString left, FixedString right)` |  |

#### `InvalidCharBehavior`

| Value | Numeric | Summary |
| --- | --- | --- |
| `Throw` | `0` |  |
| `Replace` | `1` |  |
| `Skip` | `2` |  |

#### `PetsciiEncoding`

Inherits `Encoding`. Implements `ICloneable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `C64Lowercase` | `static PetsciiEncoding C64Lowercase { get; }` |  |
| `C64Uppercase` | `static PetsciiEncoding C64Uppercase { get; }` |  |
| `EncodingName` | `override string EncodingName { get; }` |  |
| `Vic20Lowercase` | `static PetsciiEncoding Vic20Lowercase { get; }` |  |
| `Vic20Uppercase` | `static PetsciiEncoding Vic20Uppercase { get; }` |  |
| `GetByteCount` | `override int GetByteCount(char[] chars, int index, int count)` |  |
| `GetBytes` | `override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)` |  |
| `GetCharCount` | `override int GetCharCount(byte[] bytes, int index, int count)` |  |
| `GetChars` | `override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)` |  |
| `GetMaxByteCount` | `override int GetMaxByteCount(int charCount)` |  |
| `GetMaxCharCount` | `override int GetMaxCharCount(int byteCount)` |  |
| `ToByte` | `byte ToByte(char value)` |  |
| `ToChar` | `char ToChar(byte value)` |  |

#### `RetroSingleByteEncoding`

Inherits `Encoding`. Implements `ICloneable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `RetroSingleByteEncoding` | `protected RetroSingleByteEncoding(char[] toUnicode)` |  |
| `GetByteCount` | `override int GetByteCount(char[] chars, int index, int count)` |  |
| `GetBytes` | `override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)` |  |
| `GetCharCount` | `override int GetCharCount(byte[] bytes, int index, int count)` |  |
| `GetChars` | `override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)` |  |
| `GetMaxByteCount` | `override int GetMaxByteCount(int charCount)` |  |
| `GetMaxCharCount` | `override int GetMaxCharCount(int byteCount)` |  |
| `ToByte` | `byte ToByte(char value)` |  |
| `ToChar` | `char ToChar(byte value)` |  |

#### `StringBuilderExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AppendLines` | `static void AppendLines(this StringBuilder @this, IEnumerable<string> lines)` |  |

#### `StringZ`

Implements `IComparable`, `IComparable<StringZ>`, `IEquatable<StringZ>`.

| Member | Signature | Summary |
| --- | --- | --- |
| `StringZ` | `StringZ(ReadOnlySpan<char> value)` |  |
| `StringZ` | `StringZ(char[] value)` |  |
| `StringZ` | `StringZ(string value)` |  |
| `Empty` | `static StringZ Empty { get; }` |  |
| `IsEmpty` | `bool IsEmpty { get; }` |  |
| `Item` | `StringZ this[Range range] { get; }` |  |
| `Item` | `char this[Index index] { get; }` |  |
| `Item` | `char this[int index] { get; }` |  |
| `Length` | `int Length { get; }` |  |
| `AsSpan` | `ReadOnlySpan<char> AsSpan()` |  |
| `AsSpan` | `ReadOnlySpan<char> AsSpan(int start)` |  |
| `AsSpan` | `ReadOnlySpan<char> AsSpan(int start, int length)` |  |
| `CompareTo` | `int CompareTo(StringZ other)` |  |
| `CompareTo` | `int CompareTo(object obj)` |  |
| `Equals` | `bool Equals(StringZ other)` |  |
| `Equals` | `override bool Equals(object obj)` |  |
| `GetHashCode` | `override int GetHashCode()` |  |
| `GetPinnableReference` | `char GetPinnableReference()` |  |
| `Substring` | `StringZ Substring(int startIndex)` |  |
| `Substring` | `StringZ Substring(int startIndex, int length)` |  |
| `ToNullTerminatedArray` | `char[] ToNullTerminatedArray()` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator StringZ` | `static implicit operator StringZ(string value)` |  |
| `implicit operator string` | `static implicit operator string(StringZ value)` |  |
| `operator !=` | `static bool operator !=(StringZ left, StringZ right)` |  |
| `operator +` | `static StringZ operator +(StringZ left, StringZ right)` |  |
| `operator <=` | `static bool operator <=(StringZ left, StringZ right)` |  |
| `operator <` | `static bool operator <(StringZ left, StringZ right)` |  |
| `operator ==` | `static bool operator ==(StringZ left, StringZ right)` |  |
| `operator >=` | `static bool operator >=(StringZ left, StringZ right)` |  |
| `operator >` | `static bool operator >(StringZ left, StringZ right)` |  |

### Namespace `System.Text.RegularExpressions`

[`MatchExtensions`](#matchextensions) · [`RegexExtensions`](#regexextensions)

#### `MatchExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetTextSource` | `static string GetTextSource(this Match @this)` |  |
| `LineNumber` | `static int LineNumber(this Match @this)` |  |

#### `RegexExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Compile` | `static Regex Compile(this Regex @this)` |  |
| `GetNamedGroups` | `static Dictionary<int, string> GetNamedGroups(this Regex @this)` |  |
| `Or` | `static Regex Or(this Regex @this, Regex other, RegexOptions options)` |  |
| `Or` | `static Regex Or(this Regex @this, Regex other, bool compileRegex = false)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, int groupId, string replacement)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, int groupId, string replacement, int matchCount)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, string groupName, string replacement)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, string groupName, string replacement, int matchCount)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, string replacement)` |  |
| `ReplaceGroup` | `static string ReplaceGroup(this Regex @this, string source, string replacement, int matchCount)` |  |
| `WithOptions` | `static Regex WithOptions(this Regex @this, RegexOptions options)` |  |

### Namespace `System.Threading`

[`CallOnTimeout`](#callontimeout) · [`EventExtensions`](#eventextensions) · [`Future`](#future) · [`Future<TValue>`](#futuretvalue) · [`InterlockedEx`](#interlockedex) · [`ManualResetEventExtensions`](#manualreseteventextensions) · [`SemaphoreSlimExtensions`](#semaphoreslimextensions) · [`ThreadExtensions`](#threadextensions) · [`TimerExtensions`](#timerextensions)

#### `CallOnTimeout`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `CallOnTimeout` | `CallOnTimeout(TimeSpan timeout, Action<CallOnTimeout> timeoutAction)` |  |
| `ElapsedTime` | `TimeSpan ElapsedTime { get; }` |  |
| `TimeLeft` | `TimeSpan TimeLeft { get; }` |  |
| `Timeout` | `TimeSpan Timeout { get; }` |  |
| `Dispose` | `void Dispose()` |  |
| `Finalize` | `protected override void Finalize()` |  |

#### `EventExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `AsyncInvoke` | `static void AsyncInvoke(this MulticastDelegate @this, params object[] arguments)` |  |
| `AsyncInvoke` | `static void AsyncInvoke<T>(this EventHandler<T> @this, object sender, T eventArgs)` |  |

#### `Future`

| Member | Signature | Summary |
| --- | --- | --- |
| `Future` | `Future(Action action, Action callback = null)` |  |
| `IsCompleted` | `bool IsCompleted { get; }` |  |

#### `Future<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `Future` | `Future(Func<TValue> function, Action<TValue> callback = null)` |  |
| `HasValue` | `bool HasValue { get; }` |  |
| `IsCompleted` | `bool IsCompleted { get; }` |  |
| `Value` | `TValue Value { get; }` |  |
| `ToString` | `override string ToString()` |  |
| `implicit operator TValue` | `static implicit operator TValue(Future<TValue> future)` |  |

#### `InterlockedEx`

| Member | Signature | Summary |
| --- | --- | --- |
| `Add` | `static double Add(ref double source, double value)` |  |
| `Add` | `static float Add(ref float source, float value)` |  |
| `Add` | `static int Add(ref int source, int value)` |  |
| `Add` | `static long Add(ref long source, long value)` |  |
| `Add` | `static uint Add(ref uint source, uint value)` |  |
| `Add` | `static ulong Add(ref ulong source, ulong value)` |  |
| `ArithmeticShiftLeft` | `static int ArithmeticShiftLeft(ref int source, byte value)` |  |
| `ArithmeticShiftLeft` | `static long ArithmeticShiftLeft(ref long source, byte value)` |  |
| `ArithmeticShiftRight` | `static int ArithmeticShiftRight(ref int source, byte value)` |  |
| `ArithmeticShiftRight` | `static long ArithmeticShiftRight(ref long source, byte value)` |  |
| `ClearFlag` | `static TValue ClearFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null)` |  |
| `CompareExchange` | `static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __ClassForcingTag<TValue> _ = null)` |  |
| `CompareExchange` | `static TValue CompareExchange<TValue>(ref TValue source, TValue value, TValue comparand, __StructForcingTag<TValue> _ = null)` |  |
| `CompareExchange` | `static double CompareExchange(ref double source, double value, double comparand)` |  |
| `CompareExchange` | `static float CompareExchange(ref float source, float value, float comparand)` |  |
| `CompareExchange` | `static int CompareExchange(ref int source, int value, int comparand)` |  |
| `CompareExchange` | `static long CompareExchange(ref long source, long value, long comparand)` |  |
| `CompareExchange` | `static uint CompareExchange(ref uint source, uint value, uint comparand)` |  |
| `CompareExchange` | `static ulong CompareExchange(ref ulong source, ulong value, ulong comparand)` |  |
| `Decrement` | `static double Decrement(ref double source)` |  |
| `Decrement` | `static float Decrement(ref float source)` |  |
| `Decrement` | `static int Decrement(ref int source)` |  |
| `Decrement` | `static long Decrement(ref long source)` |  |
| `Decrement` | `static uint Decrement(ref uint source)` |  |
| `Decrement` | `static ulong Decrement(ref ulong source)` |  |
| `Divide` | `static double Divide(ref double source, double value)` |  |
| `Divide` | `static float Divide(ref float source, float value)` |  |
| `Divide` | `static int Divide(ref int source, int value)` |  |
| `Divide` | `static long Divide(ref long source, long value)` |  |
| `Divide` | `static uint Divide(ref uint source, uint value)` |  |
| `Divide` | `static ulong Divide(ref ulong source, ulong value)` |  |
| `Exchange` | `static TValue Exchange<TValue>(ref TValue source, TValue value, __ClassForcingTag<TValue> _ = null)` |  |
| `Exchange` | `static TValue Exchange<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null)` |  |
| `Exchange` | `static double Exchange(ref double source, double value)` |  |
| `Exchange` | `static float Exchange(ref float source, float value)` |  |
| `Exchange` | `static int Exchange(ref int source, int value)` |  |
| `Exchange` | `static long Exchange(ref long source, long value)` |  |
| `Exchange` | `static uint Exchange(ref uint source, uint value)` |  |
| `Exchange` | `static ulong Exchange(ref ulong source, ulong value)` |  |
| `HasFlag` | `static bool HasFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null)` |  |
| `Increment` | `static double Increment(ref double source)` |  |
| `Increment` | `static float Increment(ref float source)` |  |
| `Increment` | `static int Increment(ref int source)` |  |
| `Increment` | `static long Increment(ref long source)` |  |
| `Increment` | `static uint Increment(ref uint source)` |  |
| `Increment` | `static ulong Increment(ref ulong source)` |  |
| `LogicalShiftLeft` | `static int LogicalShiftLeft(ref int source, byte value)` |  |
| `LogicalShiftLeft` | `static long LogicalShiftLeft(ref long source, byte value)` |  |
| `LogicalShiftRight` | `static int LogicalShiftRight(ref int source, byte value)` |  |
| `LogicalShiftRight` | `static long LogicalShiftRight(ref long source, byte value)` |  |
| `Modulo` | `static double Modulo(ref double source, double value)` |  |
| `Modulo` | `static float Modulo(ref float source, float value)` |  |
| `Modulo` | `static int Modulo(ref int source, int value)` |  |
| `Modulo` | `static long Modulo(ref long source, long value)` |  |
| `Modulo` | `static uint Modulo(ref uint source, uint value)` |  |
| `Modulo` | `static ulong Modulo(ref ulong source, ulong value)` |  |
| `Multiply` | `static double Multiply(ref double source, double value)` |  |
| `Multiply` | `static float Multiply(ref float source, float value)` |  |
| `Multiply` | `static int Multiply(ref int source, int value)` |  |
| `Multiply` | `static long Multiply(ref long source, long value)` |  |
| `Multiply` | `static uint Multiply(ref uint source, uint value)` |  |
| `Multiply` | `static ulong Multiply(ref ulong source, ulong value)` |  |
| `NAnd` | `static int NAnd(ref int source, int value)` |  |
| `NAnd` | `static long NAnd(ref long source, long value)` |  |
| `NAnd` | `static uint NAnd(ref uint source, uint value)` |  |
| `NAnd` | `static ulong NAnd(ref ulong source, ulong value)` |  |
| `NOr` | `static int NOr(ref int source, int value)` |  |
| `NOr` | `static long NOr(ref long source, long value)` |  |
| `NOr` | `static uint NOr(ref uint source, uint value)` |  |
| `NOr` | `static ulong NOr(ref ulong source, ulong value)` |  |
| `NXor` | `static int NXor(ref int source, int value)` |  |
| `NXor` | `static long NXor(ref long source, long value)` |  |
| `NXor` | `static uint NXor(ref uint source, uint value)` |  |
| `NXor` | `static ulong NXor(ref ulong source, ulong value)` |  |
| `Not` | `static int Not(ref int source)` |  |
| `Not` | `static long Not(ref long source)` |  |
| `Not` | `static uint Not(ref uint source)` |  |
| `Not` | `static ulong Not(ref ulong source)` |  |
| `Read` | `static TValue Read<TValue>(ref TValue source, __StructForcingTag<TValue> _ = null)` |  |
| `Read` | `static double Read(ref double source)` |  |
| `Read` | `static float Read(ref float source)` |  |
| `Read` | `static int Read(ref int source)` |  |
| `Read` | `static long Read(ref long source)` |  |
| `Read` | `static uint Read(ref uint source)` |  |
| `Read` | `static ulong Read(ref ulong source)` |  |
| `RotateLeft` | `static uint RotateLeft(ref uint source, byte value)` |  |
| `RotateLeft` | `static ulong RotateLeft(ref ulong source, byte value)` |  |
| `RotateRight` | `static uint RotateRight(ref uint source, byte value)` |  |
| `RotateRight` | `static ulong RotateRight(ref ulong source, byte value)` |  |
| `SaturatingAdd` | `static int SaturatingAdd(ref int source, int value)` |  |
| `SaturatingAdd` | `static long SaturatingAdd(ref long source, long value)` |  |
| `SaturatingAdd` | `static uint SaturatingAdd(ref uint source, uint value)` |  |
| `SaturatingAdd` | `static ulong SaturatingAdd(ref ulong source, ulong value)` |  |
| `SaturatingDivide` | `static int SaturatingDivide(ref int source, int value)` |  |
| `SaturatingDivide` | `static long SaturatingDivide(ref long source, long value)` |  |
| `SaturatingDivide` | `static uint SaturatingDivide(ref uint source, uint value)` |  |
| `SaturatingDivide` | `static ulong SaturatingDivide(ref ulong source, ulong value)` |  |
| `SaturatingMultiply` | `static int SaturatingMultiply(ref int source, int value)` |  |
| `SaturatingMultiply` | `static long SaturatingMultiply(ref long source, long value)` |  |
| `SaturatingMultiply` | `static uint SaturatingMultiply(ref uint source, uint value)` |  |
| `SaturatingMultiply` | `static ulong SaturatingMultiply(ref ulong source, ulong value)` |  |
| `SaturatingSubtract` | `static int SaturatingSubtract(ref int source, int value)` |  |
| `SaturatingSubtract` | `static long SaturatingSubtract(ref long source, long value)` |  |
| `SaturatingSubtract` | `static uint SaturatingSubtract(ref uint source, uint value)` |  |
| `SaturatingSubtract` | `static ulong SaturatingSubtract(ref ulong source, ulong value)` |  |
| `SetFlag` | `static TValue SetFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null)` |  |
| `ShiftLeft` | `static uint ShiftLeft(ref uint source, byte value)` |  |
| `ShiftLeft` | `static ulong ShiftLeft(ref ulong source, byte value)` |  |
| `ShiftRight` | `static uint ShiftRight(ref uint source, byte value)` |  |
| `ShiftRight` | `static ulong ShiftRight(ref ulong source, byte value)` |  |
| `Subtract` | `static double Subtract(ref double source, double value)` |  |
| `Subtract` | `static float Subtract(ref float source, float value)` |  |
| `Subtract` | `static int Subtract(ref int source, int value)` |  |
| `Subtract` | `static long Subtract(ref long source, long value)` |  |
| `Subtract` | `static uint Subtract(ref uint source, uint value)` |  |
| `Subtract` | `static ulong Subtract(ref ulong source, ulong value)` |  |
| `ToggleFlag` | `static TValue ToggleFlag<TValue>(ref TValue source, TValue value, __StructForcingTag<TValue> _ = null)` |  |
| `Xor` | `static int Xor(ref int source, int value)` |  |
| `Xor` | `static long Xor(ref long source, long value)` |  |
| `Xor` | `static uint Xor(ref uint source, uint value)` |  |
| `Xor` | `static ulong Xor(ref ulong source, ulong value)` |  |

#### `ManualResetEventExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IsSet` | `static bool IsSet(this ManualResetEvent @this)` |  |

#### `SemaphoreSlimExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Enter` | `static IDisposable Enter(this SemaphoreSlim @this)` |  |
| `TryWait` | `static bool TryWait(this SemaphoreSlim @this)` |  |

#### `ThreadExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `IoBackgroundMode` | `static IDisposable IoBackgroundMode(this Thread @this)` |  |

#### `TimerExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `Start` | `static void Start(this Timer @this, TimeSpan timeout)` |  |
| `Start` | `static void Start(this Timer @this, int timeoutInMilliseconds)` |  |
| `Stop` | `static void Stop(this Timer @this)` |  |

### Namespace `System.Threading.Tasks`

[`DeferredTask<TValue>`](#deferredtasktvalue) · [`ScheduledCombinedTask<TValue>`](#scheduledcombinedtasktvalue) · [`ScheduledTask`](#scheduledtask) · [`ScheduledTask<TValue>`](#scheduledtasktvalue) · [`Sequential`](#sequential) · [`TaskExtensions`](#taskextensions)

#### `DeferredTask<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `DeferredTask` | `DeferredTask(Action<TValue> action, TimeSpan? waitTime = null, bool allowTaskOverlapping = true, bool autoAbortOnSchedule = false)` |  |
| `WaitHandle` | `ManualResetEventSlim WaitHandle { get; }` |  |
| `Abort` | `void Abort()` |  |
| `Now` | `void Now(TValue value)` |  |
| `Schedule` | `void Schedule(TValue value)` |  |

#### `ScheduledCombinedTask<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `ScheduledCombinedTask` | `ScheduledCombinedTask(Action<TValue[]> action, TimeSpan deferredTime, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `ScheduledCombinedTask` | `ScheduledCombinedTask(Action<TValue[]> action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `Abort` | `void Abort()` |  |
| `Execute` | `void Execute(TValue value)` |  |
| `Restart` | `void Restart(TValue value)` |  |
| `Schedule` | `void Schedule(TValue value)` |  |
| `Schedule` | `void Schedule(TValue value, TimeSpan deferredBy)` |  |
| `Schedule` | `void Schedule(TValue value, int deferredBy)` |  |

#### `ScheduledTask`

| Member | Signature | Summary |
| --- | --- | --- |
| `ScheduledTask` | `ScheduledTask(Action action, TimeSpan deferredTime, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `ScheduledTask` | `ScheduledTask(Action action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `WaitHandle` | `ManualResetEventSlim WaitHandle { get; }` |  |
| `Execute` | `void Execute()` |  |
| `ForceExecuteNow` | `void ForceExecuteNow()` |  |
| `Restart` | `void Restart()` |  |
| `Schedule` | `void Schedule()` |  |
| `Schedule` | `void Schedule(TimeSpan deferredBy)` |  |
| `Schedule` | `void Schedule(int deferredBy)` |  |

#### `ScheduledTask<TValue>`

| Member | Signature | Summary |
| --- | --- | --- |
| `ScheduledTask` | `ScheduledTask(Action<TValue> action, TimeSpan deferredTime, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `ScheduledTask` | `ScheduledTask(Action<TValue> action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false)` |  |
| `Execute` | `void Execute(TValue value)` |  |
| `Now` | `void Now(TValue value)` |  |
| `Restart` | `void Restart(TValue value)` |  |
| `Schedule` | `void Schedule(TValue value)` |  |
| `Schedule` | `void Schedule(TValue value, TimeSpan deferredBy)` |  |
| `Schedule` | `void Schedule(TValue value, int deferredBy)` |  |

#### `Sequential`

| Member | Signature | Summary |
| --- | --- | --- |
| `Invoke` | `static void Invoke(params Action[] actions)` |  |

#### `TaskExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetResultOrDefault` | `static TResult GetResultOrDefault<TResult>(this Task<TResult> @this, TResult defaultValue = null)` |  |

### Namespace `System.Timers`

[`HighPrecisionTimer`](#highprecisiontimer) · [`HighPrecisionTimerElapsedEventArgs`](#highprecisiontimerelapsedeventargs) · [`HighPrecisionTimerElapsedEventHandler`](#highprecisiontimerelapsedeventhandler)

#### `HighPrecisionTimer`

Implements `IDisposable`.

| Member | Signature | Summary |
| --- | --- | --- |
| `HighPrecisionTimer` | `HighPrecisionTimer()` |  |
| `HighPrecisionTimer` | `HighPrecisionTimer(TimeSpan interval)` |  |
| `HighPrecisionTimer` | `HighPrecisionTimer(TimeSpan interval, HighPrecisionTimerElapsedEventHandler action)` |  |
| `HighPrecisionTimer` | `HighPrecisionTimer(uint interval)` |  |
| `HighPrecisionTimer` | `HighPrecisionTimer(uint interval, HighPrecisionTimerElapsedEventHandler action)` |  |
| `AutoReset` | `bool AutoReset { get; set; }` |  |
| `Enabled` | `bool Enabled { get; }` |  |
| `Interval` | `uint Interval { get; set; }` |  |
| `Close` | `void Close()` |  |
| `Dispose` | `void Dispose()` |  |
| `Finalize` | `protected override void Finalize()` |  |
| `OnTimer` | `protected virtual void OnTimer()` |  |
| `Start` | `void Start()` |  |
| `Stop` | `void Stop()` |  |
| `Elapsed` | `event HighPrecisionTimerElapsedEventHandler Elapsed` |  |

#### `HighPrecisionTimerElapsedEventArgs`

Inherits `EventArgs`.

| Member | Signature | Summary |
| --- | --- | --- |
| `SignalTime` | `DateTime SignalTime { get; }` |  |

#### `HighPrecisionTimerElapsedEventHandler`

| Member | Signature | Summary |
| --- | --- | --- |
| `HighPrecisionTimerElapsedEventHandler` | `void HighPrecisionTimerElapsedEventHandler(object sender, HighPrecisionTimerElapsedEventArgs e)` |  |

### Namespace `System.Xml`

[`XmlAttributeCollectionExtensions`](#xmlattributecollectionextensions) · [`XmlNodeExtensions`](#xmlnodeextensions)

#### `XmlAttributeCollectionExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetValueOrDefault` | `static string GetValueOrDefault(this XmlAttributeCollection @this, string key, string defaultValue = null)` |  |

#### `XmlNodeExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `ChangeParent` | `static void ChangeParent(this XmlNode @this, XmlNode newParent)` |  |

### Namespace `System.Xml.Linq`

[`XDocumentExtensions`](#xdocumentextensions)

#### `XDocumentExtensions`

| Member | Signature | Summary |
| --- | --- | --- |
| `GetAttributeOrDefault` | `static string GetAttributeOrDefault(this XElement @this, string attributeName, StringComparison comparison = 5)` |  |

<!-- API:END -->

## 🔌 Dependencies

| Package | Why |
| --- | --- |
| [`FrameworkExtensions.Backports`](https://www.nuget.org/packages/FrameworkExtensions.Backports/) | Supplies the newer BCL surface on older targets, so the same extensions compile everywhere. |

## ⚠️ Limitations

- The surface spans nineteen target frameworks. A member depending on a newer BCL feature exists only where that feature does, natively or through Backports.
- These are extension methods on framework types: adding the package changes what IntelliSense offers on `string`, `object` and friends throughout the project.

## ❤️ Support

If this project saves you time or money, consider supporting its development:

[![GitHub Sponsors](https://img.shields.io/badge/GitHub-Sponsor-EA4AAA?logo=githubsponsors)](https://github.com/sponsors/Hawkynt)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-00457C?logo=paypal)](https://www.paypal.me/hawkynt)

## 📜 License

Licensed under LGPL-3.0-or-later — see the repository [LICENSE](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE).
