# Package Integration Changes

## Summary

This document explains the changes made to resolve conflicts between custom backport implementations and official Microsoft BCL packages.

## Problem

The Backports package was implementing custom versions of types like `Span<T>`, `ValueTuple`, `Unsafe`, etc. for older frameworks. However, Microsoft also provides official NuGet packages for many of these types that work on older frameworks. When users referenced both the Backports package and official packages (directly or transitively), they experienced type conflicts and compilation errors.

## Solution

Implemented a hybrid approach where:

1. **Official packages are used when available** - The Backports package now conditionally references official Microsoft packages based on the target framework
2. **Custom implementations for gaps** - Custom backports are only compiled for frameworks where official packages don't exist
3. **Single package reference** - Users only need to reference `FrameworkExtensions.Backports`, and all dependencies are automatically included

## Changes Made

### 1. Backports.csproj

Added conditional `PackageReference` elements and compilation symbols for official Microsoft packages:

For each target framework where an official package is used, we:
1. Define `OFFICIAL_<FEATURE>` compilation symbols via `DefineConstants`
2. Add the corresponding `PackageReference`

This ensures that when an official package is referenced, both the package and the compilation symbol are set together.

**Packages and Symbols:**

- **System.ValueTuple** (v4.5.0) → `OFFICIAL_VALUE_TUPLE`
  - Target: .NET Framework 4.0
  - Target: .NET Framework 4.5-4.6.2

- **System.Memory** (v4.5.5) → `OFFICIAL_SPAN`, `OFFICIAL_MEMORY`
  - Target: .NET Framework 4.5-4.8
  - Target: .NET Standard 2.0
  - Provides: Span<T>, ReadOnlySpan<T>, Memory<T>, MemoryMarshal

- **System.Buffers** (v4.5.1) → `OFFICIAL_ARRAYPOOL`
  - Target: .NET Framework 4.5-4.8
  - Target: .NET Standard 2.0
  - Provides: ArrayPool<T>

- **System.Runtime.CompilerServices.Unsafe** (v6.0.0) → `OFFICIAL_UNSAFE`
  - Target: .NET Framework 4.5-4.8
  - Target: .NET Standard 2.0, 2.1
  - Target: .NET Core 3.1
  - Provides: Unsafe class

- **System.Numerics.Vectors** (v4.5.0) → `OFFICIAL_VECTOR`
  - Target: .NET Framework 4.5-4.8
  - Target: .NET Standard 2.0
  - Provides: Vector<T> types

- **System.Threading.Tasks.Extensions** (v4.5.4) → `OFFICIAL_VALUETASK`
  - Target: .NET Framework 4.5-4.8
  - Target: .NET Standard 2.0
  - Provides: ValueTask

- **Microsoft.Bcl.HashCode** (v1.1.1) → `OFFICIAL_HASHCODE`
  - Target: .NET Framework 4.6.1-4.8
  - Target: .NET Standard 2.0
  - Provides: HashCode

### 2. C# and T4 Template Files

Updated all backport implementation files to check both `SUPPORTS_*` and `OFFICIAL_*` symbols:

**Old pattern:**
```csharp
#if !SUPPORTS_SPAN
// Custom Span<T> implementation
#endif
```

**New pattern:**
```csharp
#if !SUPPORTS_SPAN && !OFFICIAL_SPAN
// Custom Span<T> implementation
#endif
```

This ensures custom implementations are only compiled when:
- The framework doesn't have built-in support (`!SUPPORTS_*`), AND
- We're not using an official package (`!OFFICIAL_*`)

**Affected symbols:**
- `SUPPORTS_SPAN` / `OFFICIAL_SPAN`
- `SUPPORTS_VALUE_TUPLE` / `OFFICIAL_VALUE_TUPLE`
- `SUPPORTS_UNSAFE` / `OFFICIAL_UNSAFE`
- `SUPPORTS_VECTOR` / `OFFICIAL_VECTOR`
- `SUPPORTS_ARRAYPOOL` / `OFFICIAL_ARRAYPOOL`
- `SUPPORTS_SYSTEM_HASHCODE` / `OFFICIAL_HASHCODE`

### 3. VersionSpecificSymbols.Common.prop

No changes made to this file. The `SUPPORTS_*` symbols continue to reflect only built-in framework support, not official packages. The `OFFICIAL_*` symbols are defined in Backports.csproj alongside the package references.

### 3. Backports/Readme.md

Added new "Architecture" section documenting:
- The hybrid approach (official packages vs. custom implementations)
- Benefits of this approach
- List of official packages included
- When each package is used

## Framework-Specific Behavior

| Target Framework | Official Packages Used | Custom Implementations |
|-----------------|------------------------|------------------------|
| net20, net35 | None | All features (custom) |
| net40 | System.ValueTuple | Most features (custom) |
| net45-net46 | System.ValueTuple, System.Memory, System.Buffers, System.Runtime.CompilerServices.Unsafe, System.Numerics.Vectors, System.Threading.Tasks.Extensions | Attributes, some LINQ extensions |
| net461-net48 | Above + Microsoft.Bcl.HashCode | Attributes, some LINQ extensions |
| netstandard2.0 | System.Memory, System.Buffers, System.Runtime.CompilerServices.Unsafe, System.Numerics.Vectors, System.Threading.Tasks.Extensions, Microsoft.Bcl.HashCode | Attributes, some LINQ extensions |
| netstandard2.1+ | System.Runtime.CompilerServices.Unsafe (for compatibility) | Few custom implementations |
| netcoreapp3.1 | System.Runtime.CompilerServices.Unsafe (for compatibility) | Few custom implementations |
| net5.0+ | None | Very few custom implementations |

## Testing

To verify these changes work correctly:

1. Build the Backports project for all target frameworks:
   ```bash
   dotnet build Backports/Backports.csproj -c Release
   ```

2. Create a test project targeting various frameworks and verify:
   - No type conflicts occur
   - Types like `Span<T>`, `ValueTuple`, etc. are available
   - Using statements work without ambiguity

3. Run existing unit tests to ensure functionality is preserved

## Benefits

1. **No conflicts** - Official and custom implementations don't collide
2. **Better performance** - Official implementations are often better optimized
3. **Better compatibility** - Official packages are more thoroughly tested
4. **Simpler for users** - Only need to reference one package
5. **Automatic updates** - Official packages get security and performance updates
6. **Reduced maintenance** - Less custom code to maintain for common types

## Migration Notes

For existing users:

- **No breaking changes** - The API surface remains the same
- **No code changes needed** - Just update to the new version
- **Package dependencies** - NuGet will automatically restore the official packages
- **Binary size** - May slightly increase due to additional package dependencies, but these are shared across projects

## Future Considerations

- Monitor new official packages from Microsoft
- Add support for new backport packages as they become available
- Consider removing custom implementations entirely for types where official packages cover all target frameworks
- Potentially drop support for very old frameworks (net20, net35) if maintenance burden becomes too high
