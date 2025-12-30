# Contributing to FrameworkExtensions.Backports

Thank you for your interest in contributing to Backports! This guide explains how to add new polyfills/backports to the project.

## Project Vision

The goal of **FrameworkExtensions.Backports** is to make modern .NET features available across all target frameworks, from .NET Framework 2.0 to .NET 9.0. We aim to be:

- **Compiler-complete**: Code that compiles on .NET 9.0 should compile on older targets when referencing this package
- **Runtime-functional**: Backported features should behave identically to their official implementations
- **Non-conflicting**: When official implementations exist (via NuGet packages or the runtime), our polyfills should be excluded

## Architecture Overview

### Target Frameworks

The package targets multiple frameworks simultaneously:
- .NET Framework: net20, net35, net40, net45, net48
- .NET Standard: netstandard2.0, netstandard2.1
- .NET Core/.NET: netcoreapp3.1, net5.0, net6.0, net7.0, net8.0, net9.0

### Conditional Compilation

We use **feature flags** (conditional compilation symbols) to include/exclude code based on what each target framework supports natively.

## Feature Flags Convention

Feature flags follow the naming pattern: `SUPPORTS_<FEATURE_NAME>`

### How Feature Flags Work

1. **When a framework supports a feature natively**, the flag is defined in `VersionSpecificSymbols.Common.prop`
2. **Polyfill code is wrapped** in `#if !SUPPORTS_<FEATURE>` to only compile when the feature is NOT available
3. **Official package references** use `OFFICIAL_<FEATURE>` when we depend on Microsoft's NuGet packages

### Example Feature Flags

| Flag | Meaning | First Available |
|------|---------|----------------|
| `SUPPORTS_SPAN` | Native `Span<T>` support | .NET Core 2.1, .NET Standard 2.1 |
| `SUPPORTS_MEMORY` | Native `Memory<T>` support | .NET Core 2.1, .NET Standard 2.1 |
| `SUPPORTS_RANGE_AND_INDEX` | Native `Index` and `Range` | .NET Core 3.0, .NET Standard 2.1 |
| `SUPPORTS_MATHF` | Native `MathF` class | .NET Core 2.0, .NET Standard 2.1 |
| `SUPPORTS_VECTOR_128` | Native `Vector128<T>` | .NET Core 3.0 |
| `SUPPORTS_VECTOR_256` | Native `Vector256<T>` | .NET Core 3.0 |
| `SUPPORTS_VECTOR_512` | Native `Vector512<T>` | .NET 8.0 |

### Official Package Flags

When we reference official Microsoft NuGet packages, we use `OFFICIAL_` flags:

| Flag | Package | When Defined |
|------|---------|--------------|
| `OFFICIAL_SPAN` | System.Memory | net45+ (when package is referenced) |
| `OFFICIAL_MEMORY` | System.Memory | net45+ (when package is referenced) |
| `OFFICIAL_VALUE_TUPLE` | System.ValueTuple | net40+ (when package is referenced) |

### Designing Feature Flags

Use feature flags to describe **runtime/API capability**, not "where the code lives". Two patterns:

- **All-or-nothing features**: use a single flag like `SUPPORTS_MATHF`. Either the runtime provides the full type/API or it doesn't.
- **Layered features**: use **waves** (`FEATURE_<NAME>_WAVE1`, `FEATURE_<NAME>_WAVE2`, ...) for features that evolve across multiple .NET versions. See [Feature Waves](#feature-waves) below.

Key rules:

- **Flags are monotonic**: newer runtimes define *all* relevant flags for a feature (WAVE1 + WAVE2 + ...), never just the latest one.
- **Polyfill logic**:
  - Base type polyfill is guarded by `#if !FEATURE_<NAME>_WAVE1`.
  - Extension layers for additional members are guarded by `#if !FEATURE_<NAME>_WAVE2`, `#if !FEATURE_<NAME>_WAVE3`, etc.
- **Official packages**: use `OFFICIAL_<FEATURE>` flags only to express "the API is provided by a package instead of the runtime".

### Feature Waves

For features that evolve significantly across multiple .NET versions (like SIMD intrinsics), use the **wave pattern**. Waves provide a clean, numbered progression where each wave adds functionality on top of previous waves.

#### Wave Naming Convention

- **Flag format**: `FEATURE_<TYPE>_WAVE<k>` where `<k>` is a positive integer (1, 2, 3, ...)
- **One type = one wave series**: Each wave series is tied to exactly one type. A generic struct like `Vector128<T>` and its companion static class `Vector128` are **separate** wave series.
- **One wave series = one folder = one file**: Each type gets its own folder containing exactly one file with all waves.
- **File organization**: All waves reside in the **same file**, ordered from top to bottom (Wave 1 first, then Wave 2, etc.)

#### Wave Rules

1. **Wave 1 is the baseline**: Contains the core type and minimal API that was introduced after .NET 2.0. This is the lowest common denominator across all targets where the feature first appeared.

2. **Waves are monotonic**: If a target defines `FEATURE_<TYPE>_WAVE3`, it **must** also define `FEATURE_<TYPE>_WAVE1` and `FEATURE_<TYPE>_WAVE2`. Each wave implies all previous waves are available.

3. **Each wave layers functionality**: Higher waves add methods, properties, operators, or overloads using extension syntax. The base type is defined only in Wave 1.

4. **One type per wave series**: Do not mix multiple types in a single wave series. For example:
   - `Vector128<T>` (the struct) → `FEATURE_VECTOR128_WAVE<k>`
   - `Vector128` (the static class) → `FEATURE_VECTOR128STATIC_WAVE<k>`

   ```
   Features/
   └── Vector128/
       └── System/Runtime/Intrinsics/
           ├── Vector128.Struct.cs          # Vector128<T> struct, all waves
           └── Vector128.Class.cs          # Vector128 static class, all waves
   ```

#### Wave Example

**File: `Features/Vector128/System/Runtime/Intrinsics/Vector128.cs`** (the struct)

```csharp
#region (c)2010-2042 Hawkynt
// License header...
#endregion

namespace System.Runtime.Intrinsics;

// Wave 1: Core struct (e.g., .NET Core 3.0)
#if !FEATURE_VECTOR128_WAVE1

public readonly struct Vector128<T> : IEquatable<Vector128<T>> where T : struct {
  internal readonly ulong _v0, _v1;

  public static int Count => 16 / Unsafe.SizeOf<T>();
  public static Vector128<T> Zero => default;
  public T this[int index] => /* implementation */;
  // ... minimal members
}

#endif

// Wave 2: Instance members added later (e.g., .NET 7.0)
#if !FEATURE_VECTOR128_WAVE2

public static partial class Vector128Polyfills {

  extension<T>(Vector128<T> vector) where T : struct {
    public Vector128<T> IsNegative => /* ... */;
    public Vector128<T> IsPositive => /* ... */;
  }
}

#endif

// Wave 3: More instance members (e.g., .NET 9.0)
#if !FEATURE_VECTOR128_WAVE3

public static partial class Vector128Polyfills {

  extension<T>(Vector128<T> vector) where T : struct {
    public Vector128<T> IsNaN => /* ... */;
    public Vector128<T> IsZero => /* ... */;
  }
}

#endif
```

**File: `Features/Vector128Static/System/Runtime/Intrinsics/Vector128.cs`** (the static class)

```csharp
#region (c)2010-2042 Hawkynt
// License header...
#endregion

namespace System.Runtime.Intrinsics;

// Wave 1: Core static methods (e.g., .NET Core 3.0)
#if !FEATURE_VECTOR128STATIC_WAVE1

public static class Vector128 {
  public static Vector128<byte> Create(byte value) => /* ... */;
  public static T GetElement<T>(Vector128<T> vector, int index) => /* ... */;
  public static Vector128<T> WithElement<T>(Vector128<T> vector, int index, T value) => /* ... */;
}

#endif

// Wave 2: Conversions and System.Numerics interop (e.g., .NET 5.0)
#if !FEATURE_VECTOR128STATIC_WAVE2

public static partial class Vector128Polyfills {

  extension(Vector128) {
    public static Numerics.Vector<T> AsVector<T>(Vector128<T> value) => /* ... */;
    public static Vector128<float> AsVector128(Numerics.Vector2 value) => /* ... */;
    public static Numerics.Vector2 AsVector2(Vector128<float> value) => /* ... */;
  }
}

#endif

// Wave 3: Arithmetic and comparison (e.g., .NET 7.0)
#if !FEATURE_VECTOR128STATIC_WAVE3

public static partial class Vector128Polyfills {

  extension(Vector128) {
    public static Vector128<T> Abs<T>(Vector128<T> vector) => /* ... */;
    public static Vector128<T> Add<T>(Vector128<T> left, Vector128<T> right) => /* ... */;
    public static Vector128<T> Max<T>(Vector128<T> left, Vector128<T> right) => /* ... */;
    public static Vector128<T> Min<T>(Vector128<T> left, Vector128<T> right) => /* ... */;
  }
}

#endif

// Wave 4: Generic factory methods (e.g., .NET 8.0)
#if !FEATURE_VECTOR128STATIC_WAVE4

public static partial class Vector128Polyfills {

  extension(Vector128) {
    public static Vector128<T> CreateScalar<T>(T value) => /* ... */;
    public static Vector128<T> Create<T>(Vector64<T> lower, Vector64<T> upper) => /* ... */;
    public static Vector128<UInt16> WidenLower(Vector128<byte> source) => /* ... */;
  }
}

#endif

// Wave 5: Math functions (e.g., .NET 9.0)
#if !FEATURE_VECTOR128STATIC_WAVE5

public static partial class Vector128Polyfills {

  extension(Vector128) {
    public static Vector128<T> Clamp<T>(Vector128<T> value, Vector128<T> min, Vector128<T> max) => /* ... */;
    public static Vector128<double> Cos(Vector128<double> vector) => /* ... */;
    public static Vector128<double> Sin(Vector128<double> vector) => /* ... */;
  }
}

#endif

// Wave 6: Newest APIs (e.g., .NET 10.0)
#if !FEATURE_VECTOR128STATIC_WAVE6

public static partial class Vector128Polyfills {

  extension(Vector128) {
    public static Vector128<T> AddSaturate<T>(Vector128<T> left, Vector128<T> right) => /* ... */;
    public static bool All<T>(Vector128<T> vector, T value) => /* ... */;
    public static int IndexOf<T>(Vector128<T> vector, T value) => /* ... */;
  }
}

#endif
```

#### Wave Flag Definitions in VersionSpecificSymbols.Common.prop

Configure wave flags monotonically per target framework. Each type has its own wave series:

```xml
<!-- .NET Core 3.0/3.1: Wave 1 for both struct and static -->
<DefineConstants Condition="...netcoreapp3...">$(DefineConstants);                    FEATURE_VECTOR128_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE1;                    </DefineConstants>

<!-- .NET 5.0: Add Wave 2 for static (Numerics interop) -->
<DefineConstants Condition="...net5.0...">$(DefineConstants);                    FEATURE_VECTOR128_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE2;                    </DefineConstants>

<!-- .NET 7.0: Add Wave 2 for struct, Wave 3 for static -->
<DefineConstants Condition="...net7.0...">$(DefineConstants);                    FEATURE_VECTOR128_WAVE1;                    FEATURE_VECTOR128_WAVE2;                    FEATURE_VECTOR128STATIC_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE2;                    FEATURE_VECTOR128STATIC_WAVE3;                    </DefineConstants>

<!-- .NET 9.0: Add Wave 3 for struct, Waves 4-5 for static -->
<DefineConstants Condition="...net9.0...">$(DefineConstants);                    FEATURE_VECTOR128_WAVE1;                    FEATURE_VECTOR128_WAVE2;                    FEATURE_VECTOR128_WAVE3;                    FEATURE_VECTOR128STATIC_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE2;                    FEATURE_VECTOR128STATIC_WAVE3;                    FEATURE_VECTOR128STATIC_WAVE4;                    FEATURE_VECTOR128STATIC_WAVE5;                    </DefineConstants>

<!-- .NET 10.0: All waves for both -->
<DefineConstants Condition="...net10.0...">$(DefineConstants);                    FEATURE_VECTOR128_WAVE1;                    FEATURE_VECTOR128_WAVE2;                    FEATURE_VECTOR128_WAVE3;                    FEATURE_VECTOR128STATIC_WAVE1;                    FEATURE_VECTOR128STATIC_WAVE2;                    FEATURE_VECTOR128STATIC_WAVE3;                    FEATURE_VECTOR128STATIC_WAVE4;                    FEATURE_VECTOR128STATIC_WAVE5;                    FEATURE_VECTOR128STATIC_WAVE6;                    </DefineConstants>
```

Note: The struct and static class may have different numbers of waves since they evolve independently.

#### When to Use Waves vs Single Flags

| Pattern | Use When |
|---------|----------|
| **Waves** | Feature evolves across multiple .NET versions with API additions in each release |
| **Single flag** | Feature is all-or-nothing with no meaningful partial states |

Waves are particularly suited for:
- SIMD intrinsics: `Vector64<T>`, `Vector64`, `Vector128<T>`, `Vector128`, `Vector256<T>`, `Vector256`, `Vector512<T>`, `Vector512` (each as separate wave series)
- Types that gain new members in every major .NET release
- Any type where the API surface grows incrementally across framework versions

### Practical Feature Flag Naming

In practice, feature flags are named after **logical capability blocks**. The unit you care about is: "what set of APIs do I want to treat as one on/off switch?". That leads to two common patterns:

1. **Single flags (all-or-nothing)**
   A flag covers a *cluster* of related members that conceptually belong together and should either all exist or all be missing.
   - Example: `SUPPORTS_MATHF` means "we have the full `MathF` class".
   - Example: `SUPPORTS_STRING_ISNULLORWHITESPACE` means "this particular API is present".

   Rule of thumb: if consumers would reasonably say "either I have this feature or I don't", it gets a single flag.

2. **Wave flags (layered features)**
   Features that grow across .NET versions use numbered waves. Each wave represents a generation of API additions.
   - Example: `FEATURE_VECTOR128_WAVE1` through `FEATURE_VECTOR128_WAVE6`
   - Example: `FEATURE_SPAN_WAVE1`, `FEATURE_SPAN_WAVE2`, etc.

   Older runtimes define no wave flags (get full polyfill), mid-tier runtimes define early waves only, newest runtimes define all waves.

General rules:

- Name flags after **what you conceptually get**, not how it's implemented.
- One flag should always mean "this *logical block* of API is fully usable" – whether that's one method or ten.
- For evolving features, use waves instead of inventing semantic suffixes.

### Handling Partial Framework Support

When a type exists only in a reduced form on some targets and is missing entirely on others, use the wave pattern. Define Wave 1 for the minimal API, then add missing members through extension blocks in subsequent waves. Frameworks with no implementation get the base polyfill plus every extension layer; frameworks with partial support define only early waves and automatically pick up the missing APIs; fully modern frameworks define all waves and skip all polyfills.

```csharp
// Wave 1: minimal type for targets lacking any implementation.
#if !FEATURE_EXAMPLE_WAVE1

namespace System;

public readonly struct Example(int value) : IEquatable<Example> {
  public int Value => value;
  public bool Equals(Example other) => value == other.value;
  public override bool Equals(object o) => o is Example e && Equals(e);
}

#endif

// Wave 2: APIs added in a later .NET version.
#if !FEATURE_EXAMPLE_WAVE2

public static class ExamplePolyfills {

  extension(Example instance) {
    public bool IsZero => instance.Value == 0;
    public Example Increment() => new(instance.Value + 1);
  }

  extension(Example) {
    public static Example Zero => new(0);
    public static Example FromInt32(int v) => new(v);

    // Extension operator
    public static Example operator +(Example l, Example r) => new(l.Value + r.Value);
  }

}

#endif

// Wave 3: newest APIs, added in the most recent .NET version.
#if !FEATURE_EXAMPLE_WAVE3

public static partial class ExamplePolyfills {

  extension(Example instance) {
    public Example Clamp(Example min, Example max)
      => instance.Value < min.Value ? min :
         instance.Value > max.Value ? max :
         instance;
  }

  extension(Example) {
    public static Example Max(Example l, Example r) => l.Value >= r.Value ? l : r;
  }

}

#endif
```

This pattern scales cleanly, keeps each wave isolated, and allows complete reconstruction of the modern API surface—including instance members, static members, properties, and operators—without fragmenting the codebase with multi-branch conditionals.

In `VersionSpecificSymbols.Common.prop` you wire the flags monotonically:

- Old targets: no flags → get the base struct + all extension layers.
- Targets with partial runtime support: `FEATURE_EXAMPLE_WAVE1` only → skip the struct, but still get Wave 2 and Wave 3 extensions.
- Targets with full runtime support: `FEATURE_EXAMPLE_WAVE1`, `FEATURE_EXAMPLE_WAVE2`, and `FEATURE_EXAMPLE_WAVE3` → skip all polyfill layers entirely.

## Folder Structure

Each feature lives in its own folder under `Features/`. For wave-based features, **each type gets its own folder** containing exactly one file with all its waves.

```
Backports/
├── Features/
│   ├── Memory/
│   │   └── System/
│   │       └── Memory.cs              # Memory<T> struct, all waves
│   ├── ReadOnlyMemory/
│   │   └── System/
│   │       └── ReadOnlyMemory.cs      # ReadOnlyMemory<T> struct, all waves
│   ├── MemoryPool/
│   │   └── System/
│   │       └── Buffers/
│   │           └── MemoryPool.cs      # MemoryPool<T>, all waves
│   ├── ThrowIfNull/
│   │   └── System/
│   │       └── ArgumentNullException.cs
│   ├── Vector128/
│   │   └── System/
│   │       └── Runtime/
│   │           └── Intrinsics/
│   │               └── Vector128.cs   # Vector128<T> struct, all waves
│   ├── Vector128Static/
│   │   └── System/
│   │       └── Runtime/
│   │           └── Intrinsics/
│   │               └── Vector128.cs   # Vector128 static class, all waves
│   ├── Vector256/
│   │   └── System/
│   │       └── Runtime/
│   │           └── Intrinsics/
│   │               └── Vector256.cs   # Vector256<T> struct, all waves
│   └── Vector256Static/
│       └── System/
│           └── Runtime/
│               └── Intrinsics/
│                   └── Vector256.cs   # Vector256 static class, all waves
├── Utilities/
│   ├── AlwaysThrow.cs
│   └── Scalar.cs
├── Backports.csproj
└── ReadMe.md
```

For wave-based features, all waves live in a **single file** to maintain logical structure and make the API evolution visible at a glance:

- Wave 1 defines the **base type or minimal API** (e.g., `#if !FEATURE_VECTOR128_WAVE1`).
- Subsequent waves add **extension blocks** for additional members (e.g., `#if !FEATURE_VECTOR128_WAVE2`, etc.).

This approach keeps the complete API evolution for one type in one place, ordered chronologically from oldest to newest.

### Folder Naming Rules

1. **One type = one folder**: Each type gets its own feature folder, even if types are closely related
2. **Struct and static class are separate**: `Vector128<T>` → `Vector128/`, `Vector128` (static) → `Vector128Static/`
3. **Folder name matches type**: Use the type name (e.g., `Memory`, `Vector128`, `Vector128Static`)
4. **Internal folder structure** mirrors the namespace hierarchy (e.g., `System/Runtime/Intrinsics/`)
5. **One file per folder**: Each folder contains exactly one `.cs` file with all waves for that type
6. **Wave ordering**: Waves are ordered top to bottom by wave number within the file

## Adding a New Backport

### Step 1: Check if Official Package Exists

Before implementing a polyfill, check if Microsoft provides an official backport package:

- [System.Memory](https://www.nuget.org/packages/System.Memory) - Span, Memory, MemoryMarshal
- [System.Buffers](https://www.nuget.org/packages/System.Buffers) - ArrayPool
- [System.ValueTuple](https://www.nuget.org/packages/System.ValueTuple) - ValueTuple types
- [Microsoft.Bcl.HashCode](https://www.nuget.org/packages/Microsoft.Bcl.HashCode) - HashCode

If an official package exists, prefer referencing it for frameworks where it's available.

### Step 2: Create the Feature Folder

```
Features/
└── YourFeatureName/
    └── System/
        └── YourType.cs
```

### Step 3: Add Feature Flags

Feature flags must be added inside `<DefineConstants>` blocks in `VersionSpecificSymbols.Common.prop`.
Two constraints apply:

1. **No line breaks are allowed inside the `<DefineConstants>` value.**
   MSBuild’s symbol parser is notoriously brittle: newline characters can trigger warnings, mis-tokenize symbols, or yield silently undefined feature flags.

2. **We intentionally insert large whitespace gaps between symbols.**
   This is *not* cosmetic. Modern editors only soft-wrap long lines when the line exceeds a certain width.
   By padding the line with wide whitespace blocks, each symbol visually appears on its own line when soft-wrapped — while still remaining a single physical line for MSBuild.
   Single spaces do *not* achieve this; the line stays too short and becomes unreadable.

Correct formatting example (mirroring real project usage):

```xml
<!-- Framework 3.5 -->
<DefineConstants Condition="$(IsNetCore) OR $(IsNetStandard) OR ($(IsNetFramework) AND $([System.Version]::Parse('$(NetFrameworkVersion)').CompareTo($([System.Version]::Parse('3.5')))) &gt;= 0)">$(DefineConstants);                                                                                                                                                                                                                   SUPPORTS_ACTION_FUNC;                                                                                                                                                                                                                   SUPPORTS_EXTENSIONS;                                                                                                                                                                                                                   </DefineConstants>
```

Important notes:

- **This is a single logical line** as far as MSBuild is concerned.
  The apparent “multiple lines” above are just editor soft-wrapping created by extremely long whitespace padding.
- **Do not insert actual newlines between symbols.**
- **Use long runs of spaces** (20–300, doesn’t matter) to force editors to wrap each symbol visually.
- **Do not use tabs.** Tabs behave inconsistently across editors and can break wrapping expectations.
- **Never forget $(DefineConstants) first.** Otherwise you're dropping all previous symbols that might already been defined.

If you add a new feature:

- append it after the previous symbol on the same soft-wrapped “line”,
- separated by a long block of spaces to maintain readability,
- inside the appropriate framework block based on where the capability exists.

This formatting discipline is required. Without it, contributors will check in newline-broken DefineConstants lists that compile differently across IDEs, build agents, and CI environments.

### Step 4: Implement the Polyfill

Use the feature flag to exclude your code when the feature is natively available:

```csharp
#region (c)2010-2042 Hawkynt

// License header...

#endregion

#if !SUPPORTS_YOUR_FEATURE

namespace System;

/// <summary>
/// Provides [description matching official docs].
/// </summary>
public class YourType {
  // Implementation matching official API
}

#endif
```

### Step 5: Handle Official Package References

If you need to reference official packages for some targets but provide polyfills for others:

```csharp
#if !SUPPORTS_YOUR_FEATURE && !OFFICIAL_YOUR_FEATURE

// Polyfill implementation for targets without official support

#endif
```

In `Backports.csproj`, add conditional package references:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net45' OR ...">
  <PackageReference Include="System.YourPackage" Version="x.y.z" />
</ItemGroup>
```

### Step 6: Update the README

Add your feature to the appropriate section in `ReadMe.md`:

```markdown
* System.Namespace
  * [YourType](https://learn.microsoft.com/dotnet/api/system.namespace.yourtype)
```

**Important**: Every public type/method MUST have a link to its official Microsoft documentation.

### Step 7: Add Tests

Create tests in `Backports.Tests` under the appropriate test file:

```csharp
[TestFixture]
public class YourTypeTests {
  [Test]
  public void Method_Scenario_ExpectedBehavior() {
    // Test implementation
  }
}
```

## Code Style Guidelines

### File Header

Every file must include the license header:

```csharp
#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion
```

### Naming and Style

- **K&R braces**: Opening brace on same line
- **Prefix increment**: Use `++i` not `i++` in loops
- **Type inference**: Use `var` when type is obvious
- **Target-typed new**: Prefer `new()` over `new ClassName()` when type is clear
- **Underscore prefix**: Private/internal fields use `_camelCase` (e.g., `private readonly int _value;`)
- **Method naming**: Match official API naming exactly

### Method Implementation Pattern

For methods that need to work across different types, use helper classes:

```csharp
// Use Scalar<T> for generic numeric operations
var result = Scalar<T>.Add(left, right);

// Use AlwaysThrow for throwing exceptions without return
AlwaysThrow.ArgumentNullException(nameof(parameter));
```

### Performance Code

When using unsafe code:

1. Mark methods with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` where appropriate
2. Use `Unsafe.As<TFrom, TTo>()` for type reinterpretation
3. Wrap in `#if UNSAFE` if needed for specific targets

## Testing Requirements

Tests are **completely feature-flag agnostic**: they target the final API surface only and must compile and pass on every target without any conditional compilation nor test skipping.

### Test Organization

Tests should be organized by feature:

```
Backports.Tests/
├── BitOperationsTests.cs
├── MemoryTests.cs
├── SpanTests.cs
└── VectorTests.cs
```

### Test Naming

Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Test]
public void LeadingZeroCount_WithZero_Returns32() { ... }

[Test]
public void LeadingZeroCount_WithMaxValue_ReturnsZero() { ... }
```

### Test Design (No Feature Flags, No Target Switches, No access to Polyfill classes)

Tests must validate that **the final API surface and behavior are identical** regardless of whether it comes from the runtime, from official packages, or from polyfills.

That means:

- **Do not use `#if` in tests** for `SUPPORTS_*`, `OFFICIAL_*`, or `TargetFramework` checks.
- **Do not skip tests on old frameworks** just because a feature is polyfilled there.
- **Do not reference polyfill classes or internal implementation details** in tests; only use the public API as documented by Microsoft.
- **Do not write tests that depend on internal implementation details or polyfill-specific behavior**.
- **Do not exclude tests for certain targets**; all tests must run on all targets.
- **Write tests only against the public API** as documented by Microsoft; the tests must pass:
  - when the API is fully native,
  - when the API is fully implemented using official NuGet packages,
  - when the API is partially implemented via official packages + partially polyfilled,
  - when the API is partially native + partially official packages + partially polyfilled,
  - when the API is partially native + partially polyfilled via extensions,
  - when the API is fully polyfilled.
- The only exceptions where it is allowed to use `#if` is for:
  - testing polyfilled scalar types in bcl vectors before they are natively available (e.g., `Vector<T>` with `T` being `System.Half`, `System.Int128`, `System.UIn128`),
  - indexer access as that can't be polyfilled right now:

```csharp
#if !SUPPORTS_FEATURE_NAME_INDEXER
  var value = instance.get_Item(index);
#else
  var value = instance[index];
#endif
```

Bad test example (do **not** do this):

```csharp
#if FEATURE_EXAMPLE_WAVE1
[Test]
public void Example_Zero_IsZero() {
  Assert.That(Example.Zero.IsZero, Is.True);
}
#endif
```

Good test example (what you actually want):

```csharp
[Test]
public void Example_Zero_IsZero() {
  var zero = Example.Zero;
  Assert.That(zero.IsZero, Is.True);
}
```

If a test fails on *any* target, the implementation or flag wiring is wrong – not the test. The tests are the contract that the final API surface is the same everywhere.
Tests are written once, against the full/latest official API surface, and must compile and pass unchanged on all target frameworks.
If you feel the need to add a feature flag or target check to a test, stop – fix the implementation or the flag wiring instead.

### Framework Coverage

Tests run on multiple target frameworks. Ensure your tests work on:

- net20, net35 (Framework)
- net40, net45, net48 (Framework)
- netstandard2.0, netstandard2.1 (Standard)
- netcoreapp3.1 (Core)
- net5.0 through net9.0

## Common Patterns

### Extensions

C# 14+ introduces extension members that allow extending types with methods, properties, indexers, static members, and even operators. Use this syntax because it improves readability and organization and we have the latest compiler always at hand anyways.

```csharp
#if !SUPPORTS_FEATURE

public static class TargetTypePolyfills {

  // Instance extension block - extends instances of TargetType
  extension(TargetType instance) {
    public ReturnType MethodName(OtherType param) => instance.SomeProperty;
    public PropertyType PropertyName => instance.SomeValue;
    public ElementType this[int index] => instance.GetItem(index);
  }

  // Static extension block - adds static members and operators to TargetType
  extension(TargetType) {
    public static ReturnType StaticMethodName(OtherType param) => /* implementation */;
    public static PropertyType StaticPropertyName => /* implementation */;

    // Extension operator
    public static TargetType operator +(TargetType left, TargetType right)
      => /* implementation */;
  }

}

#endif
```

#### Generic Extensions

When extending generic types, type parameters go on the extension block:

```csharp
public static class EnumerablePolyfills {

  // Generic extension block with constraint
  extension<TSource>(IEnumerable<TSource> source)
    where TSource : IEquatable<TSource> {

    public IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
      foreach (var element in source)
        if (predicate(element))
          yield return element;
    }

    public TSource FirstOrDefault => source.FirstOrDefault();
  }

  // Method with additional generic parameters (like Select)
  extension<TSource>(IEnumerable<TSource> source) {
    public IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) {
      foreach (var element in source)
        yield return selector(element);
    }
  }

  // Static extension for generic type
  extension<T>(List<T>) {
    public static List<T> Create() => [];
  }

}
```

### Extension Methods (Legacy Syntax)

For reference, the traditional extension method syntax (still valid):

```csharp
#if !SUPPORTS_FEATURE

public static class TypePolyfills {
  public static ReturnType MethodName(this TargetType @this, OtherType param) {
    // Implementation
  }
}

#endif
```

### Struct Types

For backporting struct types:

```csharp
#if !SUPPORTS_FEATURE

public readonly struct YourStruct<T> : IEquatable<YourStruct<T>> where T : struct {
  // Fields
  private readonly T _value;

  // Constructors
  public YourStruct(T value) => _value = value;

  // Properties
  public T Value => _value;

  // Operators
  public static bool operator ==(YourStruct<T> left, YourStruct<T> right) => ...

  // IEquatable implementation
  public bool Equals(YourStruct<T> other) => ...
  public override bool Equals(object obj) => obj is YourStruct<T> other && Equals(other);
  public override int GetHashCode() => ...
}

#endif
```

## Checklist for New Features

- [ ] Feature flags added to `VersionSpecificSymbols.Common.prop`:
  - `SUPPORTS_<FEATURE>` for all-or-nothing features
  - `FEATURE_<NAME>_WAVE1/WAVE2/...` for layered features (monotonically defined)
- [ ] Polyfill files guarded with appropriate conditions:
  - `#if !SUPPORTS_<FEATURE>` for single-flag features
  - `#if !FEATURE_<NAME>_WAVE<k>` for wave-based features
- [ ] Wave-based features: all waves in single file, ordered top to bottom
- [ ] File header with license included
- [ ] Namespace matches official .NET namespace
- [ ] API matches official API exactly (names, signatures, behavior)
- [ ] XML documentation comments included
- [ ] Tests written and passing
- [ ] README updated with feature and documentation link
- [ ] Builds successfully on all target frameworks

## Getting Help

If you have questions about implementing a feature:

1. Check existing implementations in the `Features/` folder for patterns
2. Refer to [Microsoft's .NET API documentation](https://learn.microsoft.com/dotnet/api/)
3. Look at the [.NET runtime source code](https://github.com/dotnet/runtime) for reference implementations
4. Open an issue to discuss your approach before implementing

## Pull Request Process

1. Fork the repository
2. Create a feature branch with a descriptive name
3. Implement your changes following this guide
4. Ensure all tests pass across all target frameworks
5. Update documentation as needed
6. Submit a pull request with a clear description of changes

Thank you for contributing!
