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
| `ColorProcessing/Resizing/` | `Hawkynt.ColorProcessing.Resizing` |
| `ColorProcessing/Codecs/` | `Hawkynt.ColorProcessing.Codecs` |
| `ColorProcessing/Metrics/` | `Hawkynt.ColorProcessing.Metrics` |
| `ColorProcessing/ColorMath/` | `Hawkynt.ColorProcessing.ColorMath` |
| `ColorProcessing/Spaces/` | `Hawkynt.ColorProcessing.Spaces`

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

---

## ColorProcessing Module

The `Hawkynt.ColorProcessing` namespace provides a high-performance image scaling framework. This section documents how to implement scalers, kernels, color spaces, and projectors.

### The Three Spaces Architecture

ColorProcessing uses three distinct color representations:

| Space | Interface | Purpose | Examples |
|-------|-----------|---------|----------|
| **Storage** | `IStorageSpace` | Byte-oriented packed pixel formats | `Bgra8888`, `Rgb24`, `Rgb565` |
| **Working** | `IColorSpace` | Float-based linear space for math | `LinearRgbF`, `LinearRgbaF` |
| **Key** | `IColorSpace` | Perceptual space for comparisons | `OklabF`, `YuvF`, `LabF` |

**Data flow:**
```
Storage (TPixel) → Decode → Working (TWork) → Project → Key (TKey)
     ↑                           ↓
     └── Encode ← Working (TWork)
```

- **Decode** (`IDecode`): Converts packed bytes to linear float (applies gamma expansion)
- **Encode** (`IEncode`): Converts linear float back to packed bytes (applies gamma compression)
- **Project** (`IProject`): Transforms working space to perceptual key space for pattern matching

### Generic Type Parameters

Scalers and kernels use these type parameters:

| Parameter | Purpose | Constraint | Example |
|-----------|---------|------------|---------|
| `TWork` | Working color for interpolation | `IColorSpace` | `LinearRgbaF` |
| `TKey` | Key color for pattern matching | `IColorSpace` | `YuvF`, `OklabF` |
| `TPixel` | Storage pixel format | `IStorageSpace` | `Bgra8888` |
| `TDistance` | Color distance metric | `IColorMetric<TKey>` | `Euclidean3F` |
| `TEquality` | Color equality comparer | `IColorEquality<TKey>` | `ExactEquality` |
| `TLerp` | Interpolation operation | `ILerp<TWork>` | `Color4FLerp` |
| `TEncode` | Work→Pixel encoder | `IEncode<TWork,TPixel>` | `LinearRgbaFToSrgb32` |

### Implementing a Pixel Scaler

#### The IPixelScaler Interface

The entry point for any scaler:

```csharp
[ScalerInfo("My Scaler", Author = "Your Name", Year = 2024,
  Description = "Brief description", Category = ScalerCategory.PixelArt)]
public readonly struct MyScaler : IPixelScaler {

  public ScaleFactor Scale => new(2, 2);

  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new MyKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));
}
```

#### The Kernel (IScaler)

The kernel performs the actual pixel transformation:

```csharp
file readonly struct MyKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder) {

    // Access neighbors via window properties
    var center = window.P0P0.Work;
    var topLeft = window.M1M1.Work;

    // Use lerp for interpolation
    var blended = lerp.Lerp(center, topLeft, 0.5f);

    // Write output using encoder
    destTopLeft[0] = encoder.Encode(blended);
  }
}
```

#### NeighborWindow Naming Convention

The `NeighborWindow` provides access to a 5×5 pixel neighborhood:

- **M** = minus (negative offset), **P** = plus (positive offset)
- Format: `[M|P][Row][M|P][Column]`

| Property | Position | Description |
|----------|----------|-------------|
| `P0P0` | (0, 0) | Center pixel |
| `M1M1` | (-1, -1) | Top-left |
| `M1P0` | (-1, 0) | Top-center |
| `M1P1` | (-1, +1) | Top-right |
| `P0M1` | (0, -1) | Middle-left |
| `P0P1` | (0, +1) | Middle-right |
| `P1M1` | (+1, -1) | Bottom-left |
| `P1P0` | (+1, 0) | Bottom-center |
| `P1P1` | (+1, +1) | Bottom-right |
| `M2M2`..`P2P2` | (-2,-2)..(+2,+2) | Extended 5×5 neighborhood |

Each property returns a `NeighborPixel<TWork, TKey>` with:
- `.Work` - Working space color (for interpolation)
- `.Key` - Key space color (for pattern matching/comparison)

### Color Spaces and Projectors

#### Creating a Color Space

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct MyColorF(float C1, float C2, float C3)
  : IColorSpace3F<MyColorF> {

  float IColorSpace3F<MyColorF>.C1 => this.C1;
  float IColorSpace3F<MyColorF>.C2 => this.C2;
  float IColorSpace3F<MyColorF>.C3 => this.C3;
}
```

#### Creating a Projector

Projectors convert between color spaces:

```csharp
public readonly struct LinearRgbFToMyColorF : IProject<LinearRgbF, MyColorF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public MyColorF Project(in LinearRgbF rgb) {
    // Transform linear RGB to your color space
    return new MyColorF(/* transformed values */);
  }
}
```

### Metrics: Distance and Equality

#### IColorMetric<TKey> - Distance

```csharp
public readonly struct MyMetric<TKey> : IColorMetric<TKey>
  where TKey : unmanaged, IColorSpace3F<TKey> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return MathF.Sqrt(d1 * d1 + d2 * d2 + d3 * d3);
  }
}
```

#### IColorEquality<TKey> - Equality

Used for pattern-matching scalers (EPX, Eagle, Scale2x):

```csharp
public readonly struct MyEquality<TKey> : IColorEquality<TKey>
  where TKey : unmanaged {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(in TKey a, in TKey b)
    => /* your equality check */;
}
```

### Safe Component Access (CRITICAL)

#### The Problem

Scalers receive generic `TWork` types. You cannot assume the memory layout because different color spaces have different component arrangements:

- `LinearRgbaF` has (R, G, B, A)
- `OklabF` has (L, a, b)
- `YuvF` has (Y, U, V)
- `LabF` has L in 0-100, a/b in -128 to +127

#### GOOD Pattern: Use ColorConverter

```csharp
// Extract components safely (always normalized 0-1 range)
var (r, g, b) = ColorConverter.GetNormalizedRgb(pixel);
var (r, g, b, a) = ColorConverter.GetNormalizedRgba(pixel);
var alpha = ColorConverter.GetAlpha(pixel);
var luminance = ColorConverter.GetLuminance(pixel);

// Get as concrete working types
LinearRgbF rgb = ColorConverter.GetLinearRgb(pixel);
LinearRgbaF rgba = ColorConverter.GetLinearRgba(pixel);

// Create colors safely
var result = ColorConverter.FromNormalizedRgba<TWork>(r, g, b, alpha);
var result = ColorConverter.FromNormalizedRgb<TWork>(r, g, b);  // alpha = 1.0
```

#### BAD Pattern: Unsafe Pointer Casts (DO NOT USE)

```csharp
// WRONG - assumes specific memory layout
var ptr = (float*)Unsafe.AsPointer(ref color);
float r = ptr[0];  // Breaks for OklabF, YuvF, etc.!
```

**Why this fails:**
- Different color spaces have different component meanings at index 0
- Some types have 3 components, others have 4
- Component ranges vary (0-1, 0-100, -128 to +127)

### Format Considerations

#### Byte vs Float

| Space | Format | Range |
|-------|--------|-------|
| Storage (`TPixel`) | Byte (0-255), sometimes 16-bit | Discrete |
| Working (`TWork`) | Float | 0.0-1.0 (normalized) |

Use `IDecode`/`IEncode` for conversions between formats.

#### Linear vs Gamma (sRGB)

| Space | Encoding | Use Case |
|-------|----------|----------|
| Storage | Often gamma-compressed (sRGB) | Display, file formats |
| Working | Must be linear | Correct blending/interpolation |

- `IDecode` applies gamma expansion (sRGB → linear)
- `IEncode` applies gamma compression (linear → sRGB)

#### Perceptual vs Physical

| Type | Space | Use Case |
|------|-------|----------|
| Physical | `LinearRgbF` | Light calculations, correct blending |
| Perceptual | `OklabF`, `YuvF` | Human perception, pattern matching |

- Use **Key space** for perceptual comparisons (pattern detection)
- Use **Work space** for physical blending (interpolation)

### Checklist for New Scalers

- [ ] Implement `IPixelScaler` with correct `Scale` property
- [ ] Create file-scoped kernel struct(s) implementing `IScaler`
- [ ] Use `equality` parameter for pattern matching (operates on `TKey`)
- [ ] Use `lerp` parameter for interpolation (operates on `TWork`)
- [ ] Use `encoder` to write final pixels (converts to `TPixel`)
- [ ] Use `ColorConverter` for component access, never unsafe pointer casts
- [ ] Add `ScalerInfo` attribute with author, year, description, category
- [ ] Add static `SupportedScales` and `GetPossibleTargets` for discoverability
- [ ] Document in README.md with required links (see Algorithm Documentation Requirements below)

---

## Algorithm Documentation Requirements

Every algorithm (quantizer, ditherer, scaler, resampler) added to this library **MUST** be documented in `ReadMe.md` with:

### Required Information

1. **Algorithm Name** - Linked to primary paper/wiki/description
2. **Author** - Original inventor(s) or significant contributors
3. **Year** - Year of publication or creation
4. **Brief Description** - What the algorithm does and its key characteristics

### Required Links

Each algorithm entry **MUST** include BOTH:

1. **Paper/Wiki/Description Link** - A link to:
   - The original academic paper (DOI link preferred)
   - Wikipedia article describing the algorithm
   - Author's official documentation/blog post
   - Technical specification or patent

2. **Reference Implementation Link** - A link to at least one working implementation in ANY programming language:
   - GitHub/GitLab repository
   - Official library implementation
   - Well-documented open-source implementation
   - Even implementations in C, Python, JavaScript, etc. are acceptable

### Format Example

```markdown
| [`AlgorithmName`](https://link-to-paper-or-wiki) | Author Name | Year | Description | [Impl](https://link-to-reference-implementation) |
```

Or for algorithms with integrated links:

```markdown
| [`AlgorithmName`](https://link-to-paper-or-wiki) | Author Name | Year | Description |
```

With a reference implementation link in the description or as a separate column.

### Why Both Links Are Required

1. **Paper/Wiki Link**: Allows users to understand the theoretical foundation, trade-offs, and mathematical basis of the algorithm
2. **Reference Implementation Link**: Provides:
   - Verification that our implementation is correct
   - Alternative implementation details when the paper is unclear
   - Source for edge case handling that papers often omit
   - Debugging reference when issues arise

### Acceptable Reference Sources

**For Papers/Wiki:**
- DOI links (https://doi.org/...)
- Wikipedia (https://en.wikipedia.org/wiki/...)
- arXiv (https://arxiv.org/...)
- Author blogs with technical details
- Patent documents (for patented algorithms)

**For Reference Implementations:**
- GitHub/GitLab/Bitbucket repositories
- Official library documentation with code
- SourceForge projects
- Archive.org preserved implementations
- Language standard library implementations
- Well-known open-source projects (FFmpeg, ImageMagick, libretro, etc.)

### When No Published Paper Exists

For algorithms that were developed empirically (like many pixel art scalers from the demoscene era):
- Link to the original forum post or release announcement
- Link to the author's website or project page
- Link to a well-documented explanation (e.g., Wikipedia, technical blog)
- The reference implementation may serve as the primary documentation

### Checklist for New Algorithms

- [ ] Added entry to appropriate table in README.md
- [ ] Algorithm name links to paper/wiki/description
- [ ] Reference implementation link provided
- [ ] Author and year documented (if known)
- [ ] Brief description included
