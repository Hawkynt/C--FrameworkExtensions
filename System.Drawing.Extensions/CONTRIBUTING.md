# Contributing to System.Drawing.Extensions

This document extends the [main contributing guidelines](../CONTRIBUTING.md) with specifics for the System.Drawing.Extensions project.

## General Guidelines

Please refer to the [Framework CONTRIBUTING.md](../CONTRIBUTING.md) for:
- Code style and formatting conventions
- Naming conventions
- File layout and organization
- Null-checking and validation patterns
- Testing requirements

## Project-Specific Patterns

### Static Abstract Interface Pattern for Cross-Framework Compatibility

This project targets .NET 3.5 through .NET 9.0+. To enable generic factory methods that work across all targets, we use a pattern that leverages **static abstract interface members** (available in .NET 7+) while falling back to **reflection-based delegates** for older frameworks.

#### The Problem

We want to write generic code like:

```csharp
public static int FindClosest<TColorSpace>(Color[] palette, Color target)
  where TColorSpace : struct, IThreeComponentColor {

  // How do we create TColorSpace from a Color?
  var cs = TColorSpace.FromColor(target);  // Requires static abstract!
}
```

Static abstract interface members (`static abstract IColorSpace FromColor(Color c)`) are only available in C# 11 / .NET 7+. Older frameworks cannot use this syntax.

#### The Solution

1. **Define the interface with conditional static abstract members:**

```csharp
public interface IThreeComponentColor : IColorSpace {
  double Component1 { get; }
  double Component2 { get; }
  double Component3 { get; }

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  static abstract IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a);
#endif
}
```

2. **Implement concrete types with interface return types:**

```csharp
public readonly struct Rgb : IThreeComponentColor {
  // Return interface type, not concrete type!
  public static IColorSpace FromColor(Color color) => new Rgb(color.R, color.G, color.B, color.A);
  public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Rgb(c1, c2, c3, a);
}
```

**Critical:** The `FromColor` and `Create` methods must return **interface types** (`IColorSpace`, `IThreeComponentColor`), not concrete types. This ensures:
- .NET 7+: Static abstract interface members are satisfied
- Pre-.NET 7: Reflection can find methods returning the expected interface type

3. **Create a factory that uses static abstract on .NET 7+ and reflection fallback otherwise:**

```csharp
internal static class ColorSpaceConstructor<TColorSpace>
  where TColorSpace : struct, IThreeComponentColor {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte a)
    => (TColorSpace)TColorSpace.Create(c1, c2, c3, a);
#else
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte a)
    => (TColorSpace)_creator(c1, c2, c3, a);

  private static readonly Func<byte, byte, byte, byte, IThreeComponentColor> _creator = _CreateCreator();

  private static Func<byte, byte, byte, byte, IThreeComponentColor> _CreateCreator() {
    var type = typeof(TColorSpace);
    var method = type.GetMethod(
      nameof(Rgb.Create),  // Use nameof for compile-time validation
      BindingFlags.Public | BindingFlags.Static,
      null,
      [typeof(byte), typeof(byte), typeof(byte), typeof(byte)],
      null
    );

    if (method != null && typeof(IThreeComponentColor).IsAssignableFrom(method.ReturnType))
      return (Func<byte, byte, byte, byte, IThreeComponentColor>)
        Delegate.CreateDelegate(typeof(Func<byte, byte, byte, byte, IThreeComponentColor>), method);

    throw new InvalidOperationException(
      $"Color space type '{type.Name}' must have a public static method: " +
      $"public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a)");
  }
#endif
}
```

#### Why This Works

| Target | Mechanism | Performance |
|--------|-----------|-------------|
| .NET 7+ | Static abstract interface member | Zero overhead, fully inlined |
| Pre-.NET 7 | Cached delegate from reflection | One-time setup, then direct call |

The key insight: **the .NET 7+ static abstract enforces that the method exists at compile time**. Even though pre-.NET 7 targets use reflection, the .NET 7+ build will fail if any implementing type is missing the required method. This provides compile-time safety across ALL targets.

#### Rules for Adding New Color Space Types

When adding a new color space (e.g., `Luv`):

1. **Implement the interface:**
   ```csharp
   public readonly struct Luv : IThreeComponentColor {
     public double Component1 => this.L;
     public double Component2 => this.U;
     public double Component3 => this.V;
   }
   ```

2. **Add factory methods returning interface types:**
   ```csharp
   public static IColorSpace FromColor(Color color) { /* conversion */ return new Luv(...); }
   public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a) => new Luv(c1, c2, c3, a);
   ```

3. **Build for .NET 7+ to verify:** The static abstract constraint will catch missing methods at compile time.

4. **Callers must cast:** Since factory methods return interface types, callers cast to concrete:
   ```csharp
   var luv = (Luv)Luv.FromColor(color);
   ```

### Namespace Organization

Namespaces must match folder structure:

| Folder | Namespace |
|--------|-----------|
| `ColorSpaces/` | `System.Drawing.ColorSpaces` |
| `ColorSpaces/Distances/` | `System.Drawing.ColorSpaces.Distances` |
| `ColorSpaces/Interpolation/` | `System.Drawing.ColorSpaces.Interpolation` |

### Distance Calculator Pattern

Distance calculators use the zero-cost generic struct pattern:

```csharp
public readonly struct MyDistance<TColorSpace> : IColorDistanceCalculator
  where TColorSpace : struct, IThreeComponentColor {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) {
    var cs1 = (TColorSpace)ColorSpaceFactory<TColorSpace>.FromColor(color1);
    var cs2 = (TColorSpace)ColorSpaceFactory<TColorSpace>.FromColor(color2);
    // ... calculate distance using cs1.Component1, cs2.Component1, etc.
  }
}
```

This enables:
- JIT inlining of the entire calculation chain
- No virtual dispatch or allocations
- Performance equivalent to hand-written specialized code

### Adding New Distance Calculators

1. Create file in `ColorSpaces/Distances/` folder
2. Use namespace `System.Drawing.ColorSpaces.Distances`
3. Implement `IColorDistanceCalculator` as a `readonly struct`
4. Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to `Calculate`
5. Consider adding a `*Squared` variant for comparison-only use cases

## Feature Flags

This project uses the following feature flags from `VersionSpecificSymbols.Common.prop`:

| Flag | Purpose |
|------|---------|
| `SUPPORTS_ABSTRACT_INTERFACE_MEMBERS` | Static abstract interface members (C# 11 / .NET 7+) |
| `SUPPORTS_INLINING` | `MethodImplOptions.AggressiveInlining` attribute |

## Testing

Tests should cover:
- Color space conversion accuracy (round-trip tests)
- Distance calculator correctness (known color pairs)
- Edge cases (black, white, fully saturated colors)
- Palette search correctness
