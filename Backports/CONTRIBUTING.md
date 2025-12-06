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

### Step 3: Add Feature Flag to VersionSpecificSymbols.Common.prop

Add your feature flag to the appropriate framework sections:

```xml
<!-- Core 7.0 -->
<DefineConstants Condition="...">$(DefineConstants);
    SUPPORTS_YOUR_FEATURE;
</DefineConstants>
```

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

### Framework Coverage

Tests run on multiple target frameworks. Ensure your tests work on:
- net45, net48 (Framework)
- netcoreapp3.1 (Core)
- net5.0 through net9.0

## Common Patterns

### Extensions

C# 14+ introduces extension members that allow extending types with methods, properties, indexers, and static members. Use this syntax because it improves readability and organization and we have the latest compiler always at hand anyways.

```csharp
#if !SUPPORTS_FEATURE

public static class TargetTypePolyfills {

  // Instance extension block - extends instances of TargetType
  extension(TargetType instance) {
    // Extension method
    public ReturnType MethodName(OtherType param) {
      // 'instance' refers to the extended instance
      return instance.SomeProperty;
    }

    // Extension property
    public PropertyType PropertyName => instance.SomeValue;

    // Extension indexer
    public ElementType this[int index] => instance.GetItem(index);
  }

  // Static extension block - adds static members to TargetType
  extension(TargetType) {
    // Static method appears as TargetType.StaticMethodName()
    public static ReturnType StaticMethodName(OtherType param) {
      // Implementation
    }

    // Static property appears as TargetType.StaticPropertyName
    public static PropertyType StaticPropertyName => /* implementation */;
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

    // Extension method using the block's type parameter
    public IEnumerable<TSource> Where(Func<TSource, bool> predicate) {
      foreach (var element in source)
        if (predicate(element))
          yield return element;
    }

    // Extension property
    public TSource FirstOrDefault => source.FirstOrDefault();
  }

  // Method with additional generic parameters (like Select)
  extension<TSource>(IEnumerable<TSource> source) {
    // TSource comes from the block, TResult is method-specific
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

- [ ] Feature flag added to `VersionSpecificSymbols.Common.prop`
- [ ] Code wrapped in `#if !SUPPORTS_FEATURE`
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
