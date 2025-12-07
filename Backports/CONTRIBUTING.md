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

Use feature flags to describe **runtime/API capability**, not “where the code lives”. A few rules:

- **All-or-nothing features**: use a single flag like `SUPPORTS_MATHF`. Either the runtime provides the full type/API or it doesn’t.
- **Layered / partially supported features**: split the feature into meaningful levels:
  - `SUPPORTS_<FEATURE>_BASE` – the minimal type or API shape that exists on some runtimes.
  - `SUPPORTS_<FEATURE>_FULL` – the full API as it existed in the first “complete” runtime.
  - `SUPPORTS_<FEATURE>_ADVANCED` (or more specific suffixes) – new members added in later runtimes.
- **Flags are monotonic**: newer runtimes define *all* relevant flags for a feature (BASE + FULL + ADVANCED), never just the latest one.
- **Polyfill logic**:
  - Base type polyfill files are guarded by `#if !SUPPORTS_<FEATURE>_BASE`.
  - Extension layers for missing members are guarded by `#if !SUPPORTS_<FEATURE>_FULL`, `#if !SUPPORTS_<FEATURE>_ADVANCED`, etc.
- **Official packages**: use `OFFICIAL_<FEATURE>` flags only to express “the API is provided by a package instead of the runtime”; they follow the same BASE/FULL/ADVANCED idea where needed.

### Practical Feature Flag Naming

In practice, feature flags are named after **logical capability blocks**, not after some global, perfectly consistent taxonomy. The unit you care about is: “what set of APIs do I want to treat as one on/off switch?”. That leads to three common patterns:

1. **Group flags (most common)**  
   A flag covers a *cluster* of related members and sometimes multiple types that conceptually belong together and should either all exist or all be missing.  
   - Example: `SUPPORTS_VECTOR_BASIC` might cover the core `Vector128<T>`/`Vector256<T>` types, a small set of factory methods, and basic arithmetic.
   - Example: `SUPPORTS_TASK_RUN` could mean “we have `Task.Run(...)` in the shape we rely on”, even if that pulls in a couple of overloads.

   Rule of thumb: if consumers would reasonably say “either I have this feature or I don’t”, it gets a single group flag named after that feature.

2. **Single-API flags (fine-grained shims)**  
   Sometimes the smallest useful unit really is a single method or tiny surface, usually when it was added later or is optional even on newer runtimes.  
   - Example: `SUPPORTS_STRING_ISNULLORWHITESPACE`
   - Example: `SUPPORTS_SPAN_FILL`

   In that case, name the flag after the method (or property/operator group) exactly. The flag then means: “this particular API is present and behaves like the official one”.

3. **Layered / second-iteration flags**  
   Some features grow in layers: v1 has the type and a few members, later versions add more operators, helpers, or whole extra behavior. The first layer gets the “base” name; later layers get their own flags named after the layer’s *role*, not its internal history.  
   - Example:  
     - `SUPPORTS_VECTOR_BASE` – minimal usable vector API.  
     - `SUPPORTS_VECTOR_OPERATORS` – adds arithmetic/comparison operators.  
     - `SUPPORTS_VECTOR_ADVANCED` – adds newer helpers like `Clamp`, `Max`, etc.

   Older runtimes define none of these, mid-tier runtimes might define only `SUPPORTS_VECTOR_BASE`, and the newest ones define all of them.

General rules to keep you from inventing nonsense:

- Name flags after **what you conceptually get**, not how it’s implemented.
- One flag should always mean “this *logical block* of API is fully usable” – whether that’s one method or ten.
- If you find yourself needing `SUPPORTS_FOO` and `SUPPORTS_FOO_BUT_NOT_BAR`, you’ve already lost. Split them into two clean blocks: `SUPPORTS_FOO` and `SUPPORTS_FOO_BAR`, wire them monotonically, and layer the polyfills accordingly.

### Handling Partial Framework Support

When a type exists only in a reduced form on some targets and is missing entirely on others, don’t duplicate entire type definitions per framework or bury the codebase under `#if` branches. Define a single feature flag for the minimal API shared by all partial implementations, implement that minimal layer once, and then add missing members—instance methods, properties, static members, and even operators—through separate extension blocks guarded by additional feature flags. Frameworks with no implementation get the base polyfill plus every extension layer; frameworks with partial support define only the base flag and automatically pick up the missing APIs; fully modern frameworks define all flags and skip all polyfills. This incremental layering keeps the code organized, avoids redundancy, and ensures each framework gets exactly the pieces it needs.

```csharp
// Layer 0: minimal type for targets lacking any implementation.
#if !SUPPORTS_EXAMPLE_BASE

namespace System;

public readonly struct Example(int value) : IEquatable<Example> {
  public int Value => value;
  public bool Equals(Example other) => value == other.value;
  public override bool Equals(object o) => o is Example e && Equals(e);
}

#endif

// Layer 1: APIs missing from partial runtimes.
#if !SUPPORTS_EXAMPLE_FULL

public static class ExampleBaseExtensions {

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

// Layer 2: newest APIs, added by extension if the runtime lacks them.
#if !SUPPORTS_EXAMPLE_ADVANCED

public static class ExampleAdvancedExtensions {

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

This pattern scales cleanly, keeps each feature layer isolated, and allows complete reconstruction of the modern API surface—including instance members, static members, properties, and operators—without fragmenting the codebase with multi-branch conditionals.

In `VersionSpecificSymbols.Common.prop` you then wire the flags so that:

- Old targets: no flags → get the base struct + all extension layers.
- Targets with partial runtime support: `SUPPORTS_EXAMPLE_BASE` only → skip the struct, but still get extension layers.
- Targets with full runtime support: `SUPPORTS_EXAMPLE_BASE` and `SUPPORTS_EXAMPLE_FULL` and `SUPPORTS_EXAMPLE_ADVANCED` → skip all polyfill layers entirely.

## Folder Structure

Each feature lives in its own folder under `Features/`:

```
Backports/
├── Features/
│   ├── FeatureName/
│   │   └── System/
│   │       └── ClassName.cs
│   ├── Memory/
│   │   └── System/
│   │       ├── Memory.cs
│   │       ├── ReadOnlyMemory.cs
│   │       └── Buffers/
│   │           ├── MemoryPool.cs
│   │           └── IMemoryOwner.cs
│   ├── ThrowIfNull/
│   │   └── System/
│   │       └── ArgumentNullException.cs
│   └── Vector/
│       └── System/
│           └── Runtime/
│               └── Intrinsics/
│                   ├── Vector128.cs
│                   ├── Vector128.Struct.cs
│                   ├── Vector256.cs
│                   └── Vector256.Struct.cs
├── Utilities/
│   ├── AlwaysThrow.cs
│   └── Scalar.cs
├── Backports.csproj
└── ReadMe.md
```

In practice this does **not** mean you must always use a single giant `BASE/FULL/ADVANCED` file. The pattern is:

- One flag that describes the **existence of the base type or minimal API** (e.g. `SUPPORTS_EXAMPLE` or `SUPPORTS_EXAMPLE_BASE`).
- Additional flags for each **independent capability block** you care about, which may be a layer (`SUPPORTS_EXAMPLE_FULL`, `SUPPORTS_EXAMPLE_ADVANCED`) or a fine-grained shim (`SUPPORTS_STRING_ISNULLORWHITESPACE`, `SUPPORTS_EXAMPLE_CLAMP`, etc.).

Each such flag is treated as its **own feature**: it gets its own small extension file (or very tight group of files) under the feature folder. That way you can selectively light up well-defined API pieces depending on what a given target already provides, without ever needing huge “full vs advanced” monoliths or files with multiple unrelated `#if` blocks.

### Folder Naming Rules

1. **Feature folder name** should match the feature concept (e.g., `Memory`, `Span`, `ThrowIfNull`)
2. **Internal folder structure** mirrors the namespace hierarchy (e.g., `System/Buffers/` for `System.Buffers`)
3. **One feature per folder** - if a file contains multiple unrelated features, split them into separate folders
4. **Struct vs Static separation** - for types with both struct and static helper class (like `Vector128<T>` and `Vector128`), use suffixes like `.Struct.cs` and `.cs`

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

### Test Design (No Feature Flags, No Target Switches)

Tests must validate that **the final API surface and behavior are identical** regardless of whether it comes from the runtime, from official packages, or from polyfills.

That means:

- **Do not use `#if` in tests** for `SUPPORTS_*`, `OFFICIAL_*`, or `TargetFramework` checks.
- **Do not skip tests on old frameworks** just because a feature is polyfilled there.
- **Write tests only against the public API** as documented by Microsoft; the tests must pass:
  - when the API is fully native,
  - when the API is fully implemented using official NuGet packages,
  - when the API is partially implemented via official packages + partially polyfilled,
  - when the API is partially native + partially official packages + partially polyfilled,
  - when the API is partially native + partially polyfilled via extensions,
  - when the API is fully polyfilled.

Bad test example (do **not** do this):

```csharp
#if SUPPORTS_EXAMPLE_BASE
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

- [ ] Feature flags added to `VersionSpecificSymbols.Common.prop` (`SUPPORTS_FEATURE` for all-or-nothing, or `SUPPORTS_FEATURE_BASE/FULL/...` for layered features)
- [ ] Polyfill files guarded with the appropriate `#if !SUPPORTS_...` conditions (BASE/FULL/ADVANCED as described above)
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
